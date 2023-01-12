using Mono.Cecil;
using NuGet.Packaging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Uno.PackageDiff
{
	class Program
	{
		static async Task<int> Main(string[] args)
		{
			var context = new RunningContext(args);
			var writer = BuildReportWriter(context);
			var result = 1;
			(string path, NuspecReader nuspecReader) source = default;
			(string path, NuspecReader nuspecReader) target = default;

			if(writer is null)
			{
				return 1;
			}
			context.TryGetValue(CommandLineOptions.@base, out var sourceArgument);
			context.TryGetValue(CommandLineOptions.diffignore, out var diffIgnoreFile);
			try
			{
				source = await context.GetPackage(sourceArgument);
				if(context.TryGetValue(CommandLineOptions.other, out var targetArgument))
				{
					target = await context.GetPackage(targetArgument);
				}
				else
				{
					target = await context.GetPackage(sourceArgument, true);
				}

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
									context.Warn($"The assembly {targetFileName} could not be found in the target package");
									differences = true;
								}
							}
							else
							{
								context.Info($"Comparing {sourceFile} and {targetFile}");

								differences |= CompareAssemblies(context, writer, sourceFile, targetFile, ignoreSet);
							}
						}
					}
				}

				var withDifferences = differences ? ", with differences" : "";
				context.Info($"Done comparing{withDifferences}.");

				if(differences)
				{
					context.Error($"Build failed with unexpected differences. Modifications to the public API introduce binary breaking changes and should be avoided. If these modifications were expected and intended, see https://github.com/nventive/Uno.PackageDiff#how-to-provide-an-ignore-set on how to ignore them.");
				}

				result = differences ? 1 : 0;
			}
			catch(Exception ex)
			{
				context.Error(ex.Message);
				return 1;
			}
			finally
			{
				if(source != default)
				{
					Directory.Delete(source.path, true);
				}

				if(target != default)
				{
					Directory.Delete(target.path, true);
				}
			}

			return result;
		}

		public static bool CompareAssemblies(RunningContext context, IReportWriter writer, string sourceFile, string targetFile, IgnoreSet ignoreSet)
		{
			using(var source = ReadModule(sourceFile))
			using(var target = ReadModule(targetFile))
			{
				writer.WriteAssemblyName(Path.GetFileName(sourceFile));
				var results = AssemblyComparer.CompareTypes(source, target);

				return ReportAnalyzer.GenerateReport(context, writer, results, ignoreSet);
			}
		}

		private static AssemblyDefinition ReadModule(string path)
		{
			var resolver = new DefaultAssemblyResolver();

			return AssemblyDefinition.ReadAssembly(path, new ReaderParameters() { AssemblyResolver = resolver });
		}


		private static IReportWriter BuildReportWriter(RunningContext context)
		{
			IReportWriter writer = null;
			context.TryGetValue(CommandLineOptions.outtype, out var outputType);
			if(string.IsNullOrEmpty(outputType))
			{
				writer = new Writers.MarkdownWriter(context);
			}
			else
			{
				var types = outputType.Split(",", StringSplitOptions.RemoveEmptyEntries);
				if(types.Length == 0)
				{
					writer = new Writers.MarkdownWriter(context);
				}
				else
				{
					var composite = new Writers.CompositeWriter();
					foreach(var type in types)
					{
						if(string.Equals("md", type, StringComparison.OrdinalIgnoreCase))
						{
							composite.Add(new Writers.MarkdownWriter(context));
						}
						else if(string.Equals("diff", type, StringComparison.OrdinalIgnoreCase))
						{
							composite.Add(new Writers.XmlIgnoreWriter(context));
						}
						else if(string.Equals("github", type, StringComparison.OrdinalIgnoreCase))
						{
							composite.Add(new Writers.GitHubWriter(context));
						}
						else
						{
							context.Error($"Invalid outtype {type}.");
						}
					}
					if(composite.Count == types.Length)
					{
						writer = composite;
					}
				}
			}
			return writer;
		}

	}
}
