using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Mono.Cecil;
using NuGet.Versioning;

namespace Uno.PackageDiff.Writers
{
	internal class GitHubWriter : IDiffWriter
	{
		private (string Package, NuGetVersion Version) source;
		private (string Package, NuGetVersion Version) target;
		private readonly HashSet<string> events = new();
		private readonly HashSet<string> fields = new();
		private readonly HashSet<string> methods = new();
		private readonly HashSet<string> properties = new();
		private readonly HashSet<string> types = new();
		private readonly string githubPAT;
		private readonly string sourceRepository;
		private readonly string githubPRid;

		public GitHubWriter(string githubPAT, string sourceRepository, string githubPRid)
		{
			this.githubPAT = githubPAT;
			this.sourceRepository = sourceRepository;
			this.githubPRid = githubPRid;
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

		public void Dispose()
		{
			using var ms = new System.IO.MemoryStream();
			using var sw = new System.IO.StreamWriter(ms);

			sw.WriteLine($"# Comparison report for {source.Package} **{source.Version}** with {target.Package} **{target.Version}**");
			sw.WriteLine();

			if(types.Count + properties.Count + events.Count + methods.Count + fields.Count > 0)
			{
				sw.WriteLine("🚨🚨**detected some breaking changes** if changes are wanted, add following line at end of `PackageDiffIgnore.xml`.🚨🚨");
				sw.WriteLine();
				sw.WriteLine("```xml");
				sw.Flush();
				var settings = new XmlWriterSettings()
				{
					Indent = true,
					Encoding = new UTF8Encoding(false, false),
				};
				var writer = XmlWriter.Create(ms, settings);
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
				sw.WriteLine();
				sw.WriteLine("```");
				sw.Flush();
				ms.Position = 0;

				var reader = new StreamReader(ms);
				string text = reader.ReadToEnd();

				GitHubClient.PostPRCommentsAsync(githubPAT, sourceRepository, githubPRid, text).ConfigureAwait(false).GetAwaiter().GetResult();
			}
			else
			{
				sw.WriteLine("**no breaking changes detected**");
			}
		}
	}
}
