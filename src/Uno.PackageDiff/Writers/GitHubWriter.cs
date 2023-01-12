using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Mono.Cecil;
using NuGet.Versioning;

namespace Uno.PackageDiff.Writers;

internal class GitHubWriter : IReportWriter
{
	private (string Package, NuGetVersion Version) source;
	private (string Package, NuGetVersion Version) target;
	private readonly HashSet<(string Signature, string Reason)> events = new();
	private readonly HashSet<(string Signature, string Reason)> fields = new();
	private readonly HashSet<(string Signature, string Reason)> methods = new();
	private readonly HashSet<(string Signature, string Reason)> properties = new();
	private readonly HashSet<(string Signature, string Reason)> types = new();
	private readonly string githubPAT;
	private readonly string sourceRepository;
	private readonly string githubPRid;

	public GitHubWriter(RunningContext arguments)
	{
		arguments.TryGetValue(CommandLineOptions.github_pat, out githubPAT);
		arguments.TryGetValue(CommandLineOptions.github_repository, out sourceRepository);
		arguments.TryGetValue(CommandLineOptions.github_pr_id, out githubPRid);
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
		foreach(var invalidType in invalidTypes)
		{
			types.Add((invalidType.Type.ToSignature(), invalidType.IgnoreReason));
		}
	}
	public void Write(IReadOnlyList<(string Type, IReadOnlyList<(PropertyDefinition Property, bool IsIgnored, string IgnoreReason)> InvalidProperties)> invalidProperties)
	{
		foreach(var invalidProperty in invalidProperties.SelectMany(i => i.InvalidProperties))
		{
			properties.Add((invalidProperty.Property.ToSignature(), invalidProperty.IgnoreReason));
		}
	}

	public void Write(IReadOnlyList<(string Type, IReadOnlyList<(FieldDefinition Field, bool IsIgnored, string IgnoreReason)> InvalidFields)> invalidFields)
	{
		foreach(var invalidField in invalidFields.SelectMany(i => i.InvalidFields))
		{
			fields.Add((invalidField.Field.ToSignature(), invalidField.IgnoreReason));
		}
	}

	public void Write(IReadOnlyList<(string Type, IReadOnlyList<(MethodDefinition Method, bool IsIgnored, string IgnoreReason)> InvalidMethods)> invalidMethods)
	{
		foreach(var invalidMethod in invalidMethods.SelectMany(i => i.InvalidMethods))
		{
			methods.Add((invalidMethod.Method.ToSignature(), invalidMethod.IgnoreReason));
		}
	}

	public void Write(IReadOnlyList<(string Type, IReadOnlyList<(EventDefinition Event, bool IsIgnored, string IgnoreReason)> InvalidEvents)> invalidEvents)
	{
		foreach(var invalidEvent in invalidEvents.SelectMany(i => i.InvalidEvents))
		{
			events.Add((invalidEvent.Event.ToSignature(), invalidEvent.IgnoreReason));
		}
	}

	public void Dispose()
	{
		using var ms = new System.IO.MemoryStream();
		using var sw = new System.IO.StreamWriter(ms);


		sw.WriteLine($"# Nuget Package Api Surface Validation");
		sw.WriteLine();

		if(types.Count + properties.Count + events.Count + methods.Count + fields.Count > 0)
		{
			sw.WriteLine("🚨🚨 **detected some breaking changes** If these modifications were expected and intended, add following line at end of `PackageDiffIgnore.xml`. 🚨🚨");
			sw.WriteLine("<details>");
			sw.WriteLine("<summary>");
			sw.WriteLine($"Comparison report for {source.Package} **{source.Version}** with {target.Package} **{target.Version}**");
			sw.WriteLine("</summary>");
			sw.WriteLine();
			sw.WriteLine("```xml");
			sw.Flush();
			var settings = new XmlWriterSettings()
			{
				OmitXmlDeclaration = true,
				Indent = true,
				Encoding = new UTF8Encoding(false, false),
			};
			var writer = XmlWriter.Create(ms, settings);
			writer.WriteStartDocument();

			writer.WriteStartElement("IgnoreSet");
			writer.WriteAttributeString("baseVersion", $"{source.Version.Major}.{source.Version.Minor}");

			writer.WriteStartElement("Types");
			foreach(var type in types)
			{
				writer.WriteStartElement("Member");
				writer.WriteAttributeString("fullName", type.Signature);
				writer.WriteAttributeString("reason", type.Reason);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("Events");
			foreach(var @event in events)
			{
				writer.WriteStartElement("Member");
				writer.WriteAttributeString("fullName", @event.Signature);
				writer.WriteAttributeString("reason", @event.Reason);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("Fields");
			foreach(var field in fields)
			{
				writer.WriteStartElement("Member");
				writer.WriteAttributeString("fullName", field.Signature);
				writer.WriteAttributeString("reason", field.Reason);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("Properties");
			foreach(var property in properties)
			{
				writer.WriteStartElement("Member");
				writer.WriteAttributeString("fullName", property.Signature);
				writer.WriteAttributeString("reason", property.Reason);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();


			writer.WriteStartElement("Methods");
			foreach(var method in methods)
			{
				writer.WriteStartElement("Member");
				writer.WriteAttributeString("fullName", method.Signature);
				writer.WriteAttributeString("reason", method.Reason);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteEndElement();
			writer.WriteEndDocument();
			writer.Flush();
			sw.WriteLine();
			sw.WriteLine("```");
			sw.WriteLine();
			sw.WriteLine("</details>");
			sw.Flush();
			ms.Position = 0;
		}
		else
		{
			sw.WriteLine("**no breaking changes detected**");
		}
		var reader = new System.IO.StreamReader(ms);
		var comment = reader.ReadToEnd();
		GitHubClient.PostPRCommentsAsync(githubPAT, sourceRepository, githubPRid, comment).ConfigureAwait(false).GetAwaiter().GetResult();
	}
}
