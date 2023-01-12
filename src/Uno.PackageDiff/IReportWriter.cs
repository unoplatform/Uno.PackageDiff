using System;
using NuGet.Versioning;
using Mono.Cecil;
using System.Collections.Generic;

namespace Uno.PackageDiff;

internal interface IReportWriter : IDisposable
{
	void WriteAssemblyName(string v);

	void WriteHeader((string Package, NuGetVersion Version) source, (string Package, NuGetVersion Version) target);
	void WritePlatform(string platform);
	void Write(IReadOnlyList<(TypeDefinition Type, bool IsIgnored, string IgnoreReason)> invalidTypes);
	void Write(IReadOnlyList<(string Type, IReadOnlyList<(PropertyDefinition Property, bool IsIgnored, string IgnoreReason)> InvalidProperties)> invalidProperties);
	void Write(IReadOnlyList<(string Type, IReadOnlyList<(FieldDefinition Field, bool IsIgnored, string IgnoreReason)> InvalidFields)> invalidFields);
	void Write(IReadOnlyList<(string Type, IReadOnlyList<(MethodDefinition Method, bool IsIgnored, string IgnoreReason)> InvalidMethods)> invalidMethods);
	void Write(IReadOnlyList<(string Type, IReadOnlyList<(EventDefinition Event, bool IsIgnored, string IgnoreReason)> InvalidEvents)> invalidEvents);
}
