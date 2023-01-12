using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using Mono.Options;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using System.Linq;
using NuGet.Protocol;

namespace Uno.PackageDiff
{
	public class RunningContext : IReadOnlyDictionary<string, string>
	{
		readonly Dictionary<string, string> arguments = new();
		readonly Action<string> info;
		readonly Action<string> debug;
		readonly Action<string> error;
		readonly Action<string> warn;

		public bool IsGitHub_Actions { get; } =
		   string.Equals(Environment.GetEnvironmentVariable("GITHUB_ACTION"), "true", StringComparison.OrdinalIgnoreCase);

		public bool IsAzzure_Pipeline { get; } =
			string.Equals(Environment.GetEnvironmentVariable("TF_BUILD"), "true", StringComparison.OrdinalIgnoreCase);

		internal RunningContext(string[] args)
		{

			var p = new OptionSet() {
				{ CommandLineOptions.@base+'=', s => arguments[CommandLineOptions.@base] = s },
				{ CommandLineOptions.other+'=', s => arguments[CommandLineOptions.other] = s },
				{ CommandLineOptions.outfile+'=', s => arguments[CommandLineOptions.outfile] = s },
				{ CommandLineOptions.diffignore+'=', s => arguments[CommandLineOptions.diffignore] = s },
				{ CommandLineOptions.outtype+'=', s => arguments[CommandLineOptions.outtype] = s },

				//
				// GitHub PR comments related
				//
				{ CommandLineOptions.github_pat+'=', s => arguments[CommandLineOptions.github_pat] = s },
				{ CommandLineOptions.github_repository+'=', s => arguments[CommandLineOptions.github_repository] = s },
				{ CommandLineOptions.github_pr_id+'=', s => arguments[CommandLineOptions.github_pr_id] = s },
			};

			p.Parse(args);
			info = static message => Console.Out.WriteLine(message);

			if(IsGitHub_Actions)
			{
				debug = static message => Console.WriteLine($"::debug::{message}");
				warn = static message => Console.WriteLine($"::warning::{message}");
				error = static message => Console.WriteLine($"::error::{message}");
			}
			else if(IsAzzure_Pipeline)
			{
				debug = static message => Console.WriteLine($"##[debug]{message}");
				warn = static message => Console.WriteLine($"##[warning]{message}");
				error = static message => Console.WriteLine($"##[error]{message}");
			}
			else
			{
				debug = static message => Console.WriteLine($"Debug:{message}");
				warn = static message => Console.WriteLine($"Warning:{message}");
				error = static message => Console.WriteLine($"Wrror:{message}");
			}
		}

		public string this[string key] => arguments[key];

		public IEnumerable<string> Keys => arguments.Keys;

		public IEnumerable<string> Values => arguments.Values;

		public int Count => arguments.Count;

		public bool ContainsKey(string key) => arguments.ContainsKey(key);

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => arguments.GetEnumerator();
		public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value) => arguments.TryGetValue(key, out value);
		IEnumerator IEnumerable.GetEnumerator() => arguments.GetEnumerator();

		public void Info(string message) =>
			info?.Invoke(message);

		public void Debug(string message) =>
			debug?.Invoke(message);

		public void Warn(string message) =>
			warn?.Invoke(message);

		public void Error(string message) =>
			error?.Invoke(message);

		internal async Task<(string path, NuspecReader nuspecReader)> GetPackage(string packagePath, bool isPrerelease = false)
		{
			if(!packagePath.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase))
			{
				var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

				var searchResource = repository.GetResource<PackageSearchResource>();

				var packages = (await searchResource.SearchAsync(packagePath, new SearchFilter(true, SearchFilterType.IsLatestVersion), skip: 0, take: 1000, log: new NullLogger(), cancellationToken: CancellationToken.None)).ToArray();

				if(packages.Any())
				{
					var latestStable = (await packages.First().GetVersionsAsync())
						.OrderByDescending(v => v.Version)
						.Where(v => v.Version.IsPrerelease == isPrerelease)
						.FirstOrDefault();

					if(latestStable != null)
					{
						var packageId = packagePath.ToLowerInvariant();
						var version = latestStable.Version.ToNormalizedString().ToLowerInvariant();

						// https://docs.microsoft.com/en-us/nuget/api/package-base-address-resource#download-package-content-nupkg
						var url = $"https://api.nuget.org/v3-flatcontainer/{packageId}/{version}/{packageId}.{version}.nupkg";

						Info($"Downloading {url}");
						var outFile = Path.GetTempFileName();
						await new WebClient().DownloadFileTaskAsync(new Uri(url), outFile);
						return UnpackArchive(outFile);
					}
					else
					{
						throw new InvalidOperationException($"Unable to find stable {packagePath} in {repository.PackageSource.SourceUri}");
					}
				}
				else
				{
					throw new InvalidOperationException($"Unable to find {packagePath} in {repository.PackageSource.SourceUri}");
				}
			}
			else
			{
				return UnpackArchive(packagePath);
			}
		}

		private (string path, NuspecReader nuspecReader) UnpackArchive(string packagePath)
		{
			var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Trim('{', '}'));

			Directory.CreateDirectory(path);

			Debug($"Extracting {packagePath} -> {path}");
			using(var file = File.OpenRead(packagePath))
			{
				using(var archive = new ZipArchive(file, ZipArchiveMode.Read))
				{
					archive.ExtractToDirectory(path);
				}
			}

			if(Directory.GetFiles(path, "*.nuspec", SearchOption.AllDirectories).FirstOrDefault() is string nuspecFile)
			{
				return (path, new NuspecReader(nuspecFile));
			}
			else
			{
				throw new InvalidOperationException($"Unable to find nuspec file in {packagePath}");
			}
		}

	}
}
