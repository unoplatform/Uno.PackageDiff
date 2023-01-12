using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Mono.Cecil;
using NuGet.Versioning;

namespace Uno.PackageDiff.Writers
{
	internal class XmlIgnoreWriter : IReportWriter
	{
		private readonly string outputFile;
		private (string Package, NuGetVersion Version) source;
		private (string Package, NuGetVersion Version) target;
		private readonly HashSet<string> events = new();
		private readonly HashSet<string> fields = new();
		private readonly HashSet<string> methods = new();
		private readonly HashSet<string> properties = new();
		private readonly HashSet<string> types = new();

		public XmlIgnoreWriter(RunningContext arguments)
		{
			arguments.TryGetValue(CommandLineOptions.outfile, out var outputFile);
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
		public void WritePlatform(string platform) { }

		public void Write(IReadOnlyList<(TypeDefinition Type, bool IsIgnored, string IgnoreReason)> invalidTypes)
		{
			foreach(var invalidType in invalidTypes.Where(i => !i.IsIgnored).Select(i => i.Type))
			{
				types.Add(invalidType.ToSignature());
			}
		}
		public void Write(IReadOnlyList<(string Type, IReadOnlyList<(PropertyDefinition Property, bool IsIgnored, string IgnoreReason)> InvalidProperties)> invalidProperties)
		{
			foreach(var invalidProperty in invalidProperties.SelectMany(i => i.InvalidProperties).Where(i => !i.IsIgnored).Select(i => i.Property))
			{
				properties.Add(invalidProperty.ToSignature());
			}
		}

		public void Write(IReadOnlyList<(string Type, IReadOnlyList<(FieldDefinition Field, bool IsIgnored, string IgnoreReason)> InvalidFields)> invalidFields)
		{
			foreach(var invalidField in invalidFields.SelectMany(i => i.InvalidFields).Where(i => !i.IsIgnored).Select(i => i.Field))
			{
				fields.Add(invalidField.ToSignature());
			}
		}

		public void Write(IReadOnlyList<(string Type, IReadOnlyList<(MethodDefinition Method, bool IsIgnored, string IgnoreReason)> InvalidMethods)> invalidMethods)
		{
			foreach(var invalidMethod in invalidMethods.SelectMany(i => i.InvalidMethods).Where(i => !i.IsIgnored).Select(i => i.Method))
			{
				methods.Add(invalidMethod.ToSignature());
			}
		}

		public void Write(IReadOnlyList<(string Type, IReadOnlyList<(EventDefinition Event, bool IsIgnored, string IgnoreReason)> InvalidEvents)> invalidEvents)
		{
			foreach(var invalidField in invalidEvents.SelectMany(i => i.InvalidEvents).Where(i => !i.IsIgnored).Select(i => i.Event))
			{
				events.Add(invalidField.ToSignature());
			}
		}
	}
}
