using System.Collections.Generic;
using System.IO;
using System.Xml;
using Mono.Cecil;
using NuGet.Versioning;

namespace Uno.PackageDiff.Writers
{
	internal class XmlIngnoreWriter : IDiffWriter
	{
		private readonly string outputFile;
		private (string Package, NuGetVersion Version) source;
		private (string Package, NuGetVersion Version) target;
		private readonly HashSet<string> events = new();
		private readonly HashSet<string> fields = new();
		private readonly HashSet<string> methods = new();
		private readonly HashSet<string> properties = new();
		private readonly HashSet<string> types = new();

		public XmlIngnoreWriter(string outputFile)
		{
			var extension = Path.GetExtension(outputFile);
			if(!string.Equals(".xml", extension, System.StringComparison.OrdinalIgnoreCase))
			{
				outputFile = Path.Combine(Path.GetDirectoryName(outputFile)
					, Path.GetFileNameWithoutExtension(outputFile) + ".xml");
			}
			this.outputFile = outputFile;
		}

		public void Dispose()
		{
			if(types.Count + properties.Count + events.Count + methods.Count + fields.Count > 0)
			{
				var settings = new XmlWriterSettings()
				{
					Indent = true,
				};
				var writer = XmlWriter.Create(outputFile, settings);
				writer.WriteStartDocument();
				writer.WriteComment($"Comparison report for {source.Package} **{source.Version}** with {target.Package} **{target.Version}**");
				writer.WriteStartElement("DiffIgnore");
				writer.WriteStartElement("IgnoreSets");
				writer.WriteStartElement("IgnoreSet");
				writer.WriteAttributeString("baseVersion", $"{source.Version.Major}.{source.Version.Minor}");

				writer.WriteStartElement("Types");
				foreach(var type in types)
				{
					writer.WriteStartElement("Member");
					writer.WriteAttributeString("fullName", type);
					writer.WriteAttributeString("reason", string.Empty);
					writer.WriteEndElement();
				}
				writer.WriteEndElement();

				writer.WriteStartElement("Events");
				foreach(var @event in events)
				{
					writer.WriteStartElement("Member");
					writer.WriteAttributeString("fullName", @event);
					writer.WriteAttributeString("reason", string.Empty);
					writer.WriteEndElement();
				}
				writer.WriteEndElement();

				writer.WriteStartElement("Fields");
				foreach(var field in fields)
				{
					writer.WriteStartElement("Member");
					writer.WriteAttributeString("fullName", field);
					writer.WriteAttributeString("reason", string.Empty);
					writer.WriteEndElement();
				}
				writer.WriteEndElement();

				writer.WriteStartElement("Properties");
				foreach(var property in properties)
				{
					writer.WriteStartElement("Member");
					writer.WriteAttributeString("fullName", property);
					writer.WriteAttributeString("reason", string.Empty);
					writer.WriteEndElement();
				}
				writer.WriteEndElement();


				writer.WriteStartElement("Methods");
				foreach(var method in methods)
				{
					writer.WriteStartElement("Member");
					writer.WriteAttributeString("fullName", method);
					writer.WriteAttributeString("reason", string.Empty);
					writer.WriteEndElement();
				}
				writer.WriteEndElement();

				writer.WriteEndElement();
				writer.WriteEndElement();
				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();
			}
		}


		public void WriteAssemblyName(string assemblyName) { }

		public void WriteHeader((string Package, NuGetVersion Version) source, (string Package, NuGetVersion Version) target)
		{
			this.source = source;
			this.target = target;
		}

		public void WriteMissingEvent(EventDefinition eventDef, bool isIgnored)
		{
			if(!isIgnored)
				events.Add(eventDef.ToSignature());
		}
		public void WriteMissingEventsEnd() { }
		public void WriteMissingEventsForTypeEnd() { }
		public void WriteMissingEventsForTypeStart(string typeName) { }
		public void WriteMissingEventsStart(int invalidEventsCount) { }

		public void WriteMissingField(FieldDefinition fieldDef, bool isIgnored)
		{
			if(!isIgnored)
				fields.Add(fieldDef.ToSignature());
		}

		public void WriteMissingFieldForTypeEnd() { }
		public void WriteMissingFieldForTypeStart(string typeName) { }
		public void WriteMissingFieldsEnd() { }
		public void WriteMissingFieldsStart(int invalidFieldsCount) { }

		public void WriteMissingMethod(MethodDefinition methodDef, bool isIgnored)
		{
			if(!isIgnored)
				methods.Add(methodDef.ToSignature());
		}
		public void WriteMissingMethodsEnd() { }
		public void WriteMissingMethodsForTypeEnd() { }
		public void WriteMissingMethodsForTypeStart(string typeName) { }
		public void WriteMissingMethodsStart(int invalidMethodsCount) { }

		public void WriteMissingPropertiesEnd() { }
		public void WriteMissingPropertiesForTypeEnd() { }
		public void WriteMissingPropertiesForTypeStart(string typeName) { }
		public void WriteMissingPropertiesStart(int invalidPropertiesCount) { }
		public void WriteMissingProperty(PropertyDefinition propertyDef, bool isIgnored)
		{
			if(!isIgnored)
				properties.Add(propertyDef.ToSignature());
		}

		public void WriteMissingType(TypeDefinition typeDef, bool isIgnored)
		{
			if(!isIgnored)
				types.Add(typeDef.ToSignature());
		}
		public void WriteMissingTypesEnd() { }
		public void WriteMissingTypesStart(int invalidTypesCount) { }
		public void WritePlatform(string platform) { }
	}
}
