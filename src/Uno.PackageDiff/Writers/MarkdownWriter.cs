using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using NuGet.Versioning;

namespace Uno.PackageDiff.Writers;

internal class MarkdownWriter : IReportWriter
{
	private readonly StreamWriter writer;

	public MarkdownWriter(RunningContext context)
	{
		context.TryGetValue(CommandLineOptions.outfile, out var outputFile);
		var extension = Path.GetExtension(outputFile);
		if(!string.Equals(".md", extension, System.StringComparison.OrdinalIgnoreCase))
		{
			outputFile = Path.Combine(Path.GetDirectoryName(outputFile)
				, Path.GetFileNameWithoutExtension(outputFile) + ".md");
		}
		writer = new StreamWriter(outputFile);
	}

	public void Dispose() =>
	 writer?.Dispose();

	public void WriteAssemblyName(string assemblyName) =>
		writer.WriteLine($"## {Path.GetFileName(assemblyName)}");

	public void WriteHeader((string Package, NuGetVersion Version) source, (string Package, NuGetVersion Version) target) =>
		writer.WriteLine($"Comparison report for {source.Package} **{source.Version}** with {target.Package} **{target.Version}**");

	public void WritePlatform(string platform) =>
		writer.WriteLine($"# {platform} Platform");


	public void Write(IReadOnlyList<(TypeDefinition Type, bool IsIgnored, string IgnoreReason)> invalidTypes)
	{
		writer.WriteLine("### {0} missing types:", invalidTypes.Count);
		foreach(var invalidType in invalidTypes)
		{
			var strike = invalidType.IsIgnored
				? "~~" : "";
			writer.WriteLine($"* {strike}`{invalidType.Type.ToSignature()}`{strike}");
		}
	}
	public void Write(IReadOnlyList<(string Type, IReadOnlyList<(PropertyDefinition Property, bool IsIgnored, string IgnoreReason)> InvalidProperties)> invalidProperties)
	{
		writer.WriteLine("### {0} missing or changed properties in existing types:", invalidProperties.Count);
		foreach(var invalidType in invalidProperties)
		{
			writer.WriteLine("- `{0}`", invalidType.Type);
			foreach(var invalidProperty in invalidType.InvalidProperties)
			{
				var strike = invalidProperty.IsIgnored
					? "~~"
					: "";
				writer.WriteLine($"\t* {strike}``{invalidProperty.Property.ToSignature()}``{strike}");

			}
		}
	}

	public void Write(IReadOnlyList<(string Type, IReadOnlyList<(FieldDefinition Field, bool IsIgnored, string IgnoreReason)> InvalidFields)> invalidFields)
	{
		writer.WriteLine("### {0} missing or changed fields in existing types:", invalidFields.Count);
		foreach(var invalidType in invalidFields)
		{
			writer.WriteLine("- `{0}`", invalidType.Type);
			foreach(var invalidProperty in invalidType.InvalidFields)
			{
				var strike = invalidProperty.IsIgnored
					? "~~"
					: "";
				writer.WriteLine($"\t* {strike}``{invalidProperty.Field.ToSignature()}``{strike}");
			}
		}
	}

	public void Write(IReadOnlyList<(string Type, IReadOnlyList<(MethodDefinition Method, bool IsIgnored, string IgnoreReason)> InvalidMethods)> invalidMethods)
	{
		writer.WriteLine("### {0} missing or changed methods in existing types:", invalidMethods.Count);
		foreach(var invalidType in invalidMethods)
		{
			writer.WriteLine("- `{0}`", invalidType.Type);
			foreach(var invalidMethod in invalidType.InvalidMethods)
			{
				var strike = invalidMethod.IsIgnored
					? "~~"
					: "";
				writer.WriteLine($"\t* {strike}``{invalidMethod.Method.ToSignature()}``{strike}");
			}
		}
	}
	public void Write(IReadOnlyList<(string Type, IReadOnlyList<(EventDefinition Event, bool IsIgnored, string IgnoreReason)> InvalidEvents)> invalidEvents)
	{
		writer.WriteLine("### {0} missing or changed events in existing types:", invalidEvents.Count);
		foreach(var invalidType in invalidEvents)
		{
			writer.WriteLine("- `{0}`", invalidType.Type);
			foreach(var invalidEvent in invalidType.InvalidEvents)
			{
				var strike = invalidEvent.IsIgnored
					? "~~"
					: "";
				writer.WriteLine($"\t* {strike}``{invalidEvent.Event.ToSignature()}``{strike}");
			}
		}
	}
}
