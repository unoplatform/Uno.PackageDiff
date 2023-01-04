using System.IO;
using Mono.Cecil;
using NuGet.Versioning;

namespace Uno.PackageDiff.Writers
{
	internal class MDDiffWriter : IDiffWriter
	{
		private readonly StreamWriter writer;

		public MDDiffWriter(string outputFile)
		{
			var extension = Path.GetExtension(outputFile);
			if(!string.Equals(".md", extension,System.StringComparison.OrdinalIgnoreCase))
			{
				outputFile = Path.Combine(Path.GetDirectoryName(outputFile)
					, Path.GetFileNameWithoutExtension(outputFile) + ".md");
			}
			writer = new StreamWriter(outputFile);
		}

		public MDDiffWriter(StreamWriter writer)
		{
			this.writer = writer;
		}

		public void Dispose() =>
		 writer?.Dispose();

		public void WriteAssemblyName(string assemblyName) =>
			writer.WriteLine($"## {Path.GetFileName(assemblyName)}");

		public void WriteHeader((string Package, NuGetVersion Version) source, (string Package, NuGetVersion Version) target) =>
			writer.WriteLine($"Comparison report for {source.Package} **{source.Version}** with {target.Package} **{target.Version}**");

		public void WriteMissingTypesStart(int invalidTypesCount) =>
			writer.WriteLine("### {0} missing types:", invalidTypesCount);

		public void WriteMissingType(Mono.Cecil.TypeDefinition typeDef, bool isIgnored)
		{
			var strike = isIgnored
				? "~~" : "";
			writer.WriteLine($"* {strike}`{typeDef.ToSignature()}`{strike}");
		}

		public void WriteMissingTypesEnd() { }

		public void WritePlatform(string platform) =>
			writer.WriteLine($"# {platform} Platform");

		#region Missing Properties
		public void WriteMissingPropertiesStart(int invalidPropertiesCount) =>
			writer.WriteLine("### {0} missing or changed properties in existing types:", invalidPropertiesCount);

		public void WriteMissingPropertiesForTypeStart(string typeName) =>
			writer.WriteLine("- `{0}`", typeName);
		public void WriteMissingProperty(PropertyDefinition propertyDef, bool isIgnored)
		{
			var strike = isIgnored
				? "~~" : "";
			writer.WriteLine($"\t* {strike}``{propertyDef.ToSignature()}``{strike}");
		}
		public void WriteMissingPropertiesForTypeEnd() { }
		public void WriteMissingPropertiesEnd() { }
		#endregion

		#region Mssing Fileds
		public void WriteMissingFieldsStart(int invalidFieldsCount) =>
			writer.WriteLine("### {0} missing or changed fields in existing types:", invalidFieldsCount);
		public void WriteMissingFieldForTypeStart(string typeName) =>
			writer.WriteLine("- `{0}`", typeName);
		public void WriteMissingField(FieldDefinition fieldDef, bool isIgnored)
		{
			var strike = isIgnored
				? "~~" : "";
			writer.WriteLine($"\t* {strike}``{fieldDef.ToSignature()}``{strike}");

		}
		public void WriteMissingFieldForTypeEnd() { }
		public void WriteMissingFieldsEnd() { }
		#endregion

		#region Missing Methods
		public void WriteMissingMethodsStart(int invalidMethodsCount) =>
			writer.WriteLine("### {0} missing or changed method in existing types:", invalidMethodsCount);
		public void WriteMissingMethodsForTypeStart(string typeName) =>
			writer.WriteLine("- `{0}`", typeName);
		public void WriteMissingMethod(MethodDefinition methodDef, bool isIgnored)
		{
			var strike = isIgnored
				? "~~" : "";
			writer.WriteLine($"\t* {strike}``{methodDef.ToSignature()}``{strike}");

		}
		public void WriteMissingMethodsForTypeEnd() { }
		public void WriteMissingMethodsEnd() { }
		#endregion

		#region MissingEvents
		public void WriteMissingEventsStart(int invalidEventsCount) =>
			writer.WriteLine("### {0} missing or changed events in existing types:", invalidEventsCount);
		public void WriteMissingEventsForTypeStart(string typeName) =>
			writer.WriteLine("- `{0}`", typeName);
		public void WriteMissingEvent(EventDefinition eventDef, bool isIgnored)
		{
			var strike = isIgnored
				? "~~" : "";
			writer.WriteLine($"\t* {strike}``{eventDef.ToSignature()}``{strike}");

		}
		public void WriteMissingEventsForTypeEnd() { }
		public void WriteMissingEventsEnd() { }
		#endregion

	}
}
