using System.Collections.Generic;
using Mono.Cecil;
using NuGet.Versioning;

namespace Uno.PackageDiff.Writers;

internal class CompositeWriter : IReportWriter
{
	private readonly List<IReportWriter> _writers = new();

	public void Dispose() => _writers.ForEach(w => w.Dispose());

	public void WriteAssemblyName(string v) => _writers.ForEach(w => w.WriteAssemblyName(v));

	public void WriteHeader((string Package, NuGetVersion Version) source, (string Package, NuGetVersion Version) target) =>
		_writers.ForEach(w => w.WriteHeader(source, target));
	public void WritePlatform(string platform) => _writers.ForEach(w => w.WritePlatform(platform));

	public void Write(IReadOnlyList<(TypeDefinition Type, bool IsIgnored, string IgnoreReason)> invalidTypes) =>
		_writers.ForEach(w => w.Write(invalidTypes));
	public void Write(IReadOnlyList<(string Type, IReadOnlyList<(PropertyDefinition Property, bool IsIgnored, string IgnoreReason)> InvalidProperties)> invalidProperties) =>
		_writers.ForEach(w => w.Write(invalidProperties));
	public void Write(IReadOnlyList<(string Type, IReadOnlyList<(FieldDefinition Field, bool IsIgnored, string IgnoreReason)> InvalidFields)> invalidFields) =>
		_writers.ForEach(w => w.Write(invalidFields));
	public void Write(IReadOnlyList<(string Type, IReadOnlyList<(MethodDefinition Method, bool IsIgnored, string IgnoreReason)> InvalidMethods)> invalidMethods) =>
		_writers.ForEach(w => w.Write(invalidMethods));
	public void Write(IReadOnlyList<(string Type, IReadOnlyList<(EventDefinition Event, bool IsIgnored, string IgnoreReason)> InvalidEvents)> invalidEvents) =>
		_writers.ForEach(w => w.Write(invalidEvents));

	public int Count => _writers.Count;
	public void Add(IReportWriter writer) => _writers.Add(writer);
}
