﻿using Mono.Cecil;
using Mono.Options;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Uno.PackageDiff
{
	class Program
	{
		private static readonly PackageSource NuGetOrgSource = new PackageSource("https://api.nuget.org/v3/index.json");

		static async Task<int> Main(string[] args)
		{
			string sourceArgument = null;
			string targetArgument = null;
			string outputFile = null;
			string diffIgnoreFile = null;
			string outputType = null;
			string githubPAT = null;
			string sourceRepository = null;
			string githubPRid = null;

			var p = new OptionSet() {
				{ "base=", s => sourceArgument = s },
				{ "other=", s => targetArgument = s },
				{ "outfile=", s => outputFile = s },
				{ "diffignore=", s => diffIgnoreFile = s },
				{ "outtype=", s => outputType = s},

				//
				// GitHub PR comments related
				//
				{ "github-pat=", s => githubPAT = s },
				{ "source-repository=", s => sourceRepository = s },
				{ "github-pr-id=", s => githubPRid = s  }
			};

			p.Parse(args);

			IDiffWriter writer = null;
			if(string.IsNullOrEmpty(outputType))
			{
				writer = new Writers.MDDiffWriter(outputFile);
			}
			else
			{
				var types = outputType.Split(",", StringSplitOptions.RemoveEmptyEntries);
				if(types.Length == 0)
				{
					writer = new Writers.MDDiffWriter(outputFile);
				}
				else
				{
					var composite = new Writers.CompositeWriter();
					foreach(var type in types)
					{
						if(string.Equals("md", type, StringComparison.OrdinalIgnoreCase))
						{
							composite.Add(new Writers.MDDiffWriter(outputFile));
						}
						else if(string.Equals("diff", type, StringComparison.OrdinalIgnoreCase))
						{
							composite.Add(new Writers.XmlIngnoreWriter(outputFile));
						}
						else if(string.Equals("github", type, StringComparison.OrdinalIgnoreCase))
						{
							composite.Add(new Writers.GitHubWriter(githubPAT, sourceRepository, githubPRid));
						}
					}
					if(composite.Count == 0)
					{
						Console.WriteLine($"Error : Invalid outtype {outputType}.");
						return 1;
					}
					writer = composite;
				}
			}

			var source = await GetPackage(sourceArgument);
			var target = await GetPackage(targetArgument);

			try
			{
				var sourcePlatforms = Directory.GetDirectories(Path.Combine(source.path, "lib"), "*.*", SearchOption.TopDirectoryOnly);
				var targetPlatforms = Directory.GetDirectories(Path.Combine(target.path, "lib"), "*.*", SearchOption.TopDirectoryOnly);
				var platformsFiles = from sourceAssemblyPath in sourcePlatforms
									 join targetAssemblyPath in targetPlatforms on Path.GetFileName(sourceAssemblyPath) equals Path.GetFileName(targetAssemblyPath)
									 select new
									 {
										 Platform = Path.GetFileName(sourceAssemblyPath),
										 Source = sourceAssemblyPath,
										 Target = targetAssemblyPath
									 };


				var ignoreSet = DiffIgnore.ParseDiffIgnore(diffIgnoreFile, source.nuspecReader.GetVersion().ToString());

				bool differences = false;
				using(writer)
				{
					writer.WriteHeader(source: (source.nuspecReader.GetId(), source.nuspecReader.GetVersion())
						, target: (target.nuspecReader.GetId(), target.nuspecReader.GetVersion()));

					foreach(var platform in platformsFiles)
					{
						writer.WritePlatform(platform.Platform);

						foreach(var sourceFile in Directory.GetFiles(platform.Source, "*.dll"))
						{
							var targetFile = Path.Combine(platform.Target, Path.GetFileName(sourceFile));

							if(!File.Exists(targetFile))
							{
								var targetFileName = Path.GetFileNameWithoutExtension(targetFile);

								if(!ignoreSet.Assemblies.Any(m => m.FullName.Equals(targetFileName, StringComparison.OrdinalIgnoreCase)))
								{
									Console.WriteLine($"The assembly {targetFileName} could not be found in the target package");
									differences = true;
								}
							}
							else
							{
								Console.WriteLine($"Comparing {sourceFile} and {targetFile}");

								differences |= CompareAssemblies(writer, sourceFile, targetFile, ignoreSet);
							}
						}
					}
				}

				var withDifferences = differences ? ", with differences" : "";
				Console.WriteLine($"Done comparing{withDifferences}.");

				if(differences)
				{
					Console.WriteLine(@"Error : Build failed with unexpected differences. Modifications to the public API introduce binary breaking changes and should be avoided.");
					Console.WriteLine("If these modifications were expected and intended, see https://github.com/nventive/Uno.PackageDiff#how-to-provide-an-ignore-set on how to ignore them.");
				}

				return differences ? 1 : 0;
			}
			finally
			{
				Directory.Delete(source.path, true);
				Directory.Delete(target.path, true);
			}
		}

		public static bool CompareAssemblies(IDiffWriter writer, string sourceFile, string targetFile, IgnoreSet ignoreSet)
		{
			using(var source = ReadModule(sourceFile))
			using(var target = ReadModule(targetFile))
			{
				writer.WriteAssemblyName(Path.GetFileName(sourceFile));
				var results = AssemblyComparer.CompareTypes(source, target);

				return ReportAnalyzer.GenerateReport(writer, results, ignoreSet);
			}
		}

		private static AssemblyDefinition ReadModule(string path)
		{
			var resolver = new DefaultAssemblyResolver();

			return AssemblyDefinition.ReadAssembly(path, new ReaderParameters() { AssemblyResolver = resolver });
		}

		private static async Task<(string path, NuspecReader nuspecReader)> GetPackage(string packagePath)
		{
			if(!packagePath.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase))
			{
				var settings = Settings.LoadDefaultSettings(null);
				var repositoryProvider = new SourceRepositoryProvider(settings, Repository.Provider.GetCoreV3());

				var repository = repositoryProvider.CreateRepository(NuGetOrgSource, FeedType.HttpV3);

				var searchResource = repository.GetResource<PackageSearchResource>();

				var packages = (await searchResource.SearchAsync(packagePath, new SearchFilter(true, SearchFilterType.IsLatestVersion), skip: 0, take: 1000, log: new NullLogger(), cancellationToken: CancellationToken.None)).ToArray();

				if(packages.Any())
				{
					var latestStable = (await packages.First().GetVersionsAsync())
						.OrderByDescending(v => v.Version)
						.Where(v => !v.Version.IsPrerelease)
						.FirstOrDefault();

					if(latestStable != null)
					{
						var packageId = packagePath.ToLowerInvariant();
						var version = latestStable.Version.ToNormalizedString().ToLowerInvariant();

						// https://docs.microsoft.com/en-us/nuget/api/package-base-address-resource#download-package-content-nupkg
						var url = $"https://api.nuget.org/v3-flatcontainer/{packageId}/{version}/{packageId}.{version}.nupkg";

						Console.WriteLine($"Downloading {url}");
						var outFile = Path.GetTempFileName();
						await new WebClient().DownloadFileTaskAsync(new Uri(url), outFile);
						return UnpackArchive(outFile);
					}
					else
					{
						throw new InvalidOperationException($"Unable to find stable {packagePath} in {NuGetOrgSource.SourceUri}");
					}
				}
				else
				{
					throw new InvalidOperationException($"Unable to find {packagePath} in {NuGetOrgSource.SourceUri}");
				}
			}
			else
			{
				return UnpackArchive(packagePath);
			}
		}

		private static (string path, NuspecReader nuspecReader) UnpackArchive(string packagePath)
		{
			var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Trim('{', '}'));

			Directory.CreateDirectory(path);

			Console.WriteLine($"Extracting {packagePath} -> {path}");
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
