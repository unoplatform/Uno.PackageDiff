using System;
using System.Collections.Generic;
using Mono.Cecil;
using NuGet.Versioning;

namespace Uno.PackageDiff.Writers
{
	internal class CompositeWriter : IDiffWriter
	{
		private readonly List<IDiffWriter> _writers = new();

		public void Dispose() => _writers.ForEach(w => w.Dispose());

		public void WriteAssemblyName(string v) => _writers.ForEach(w => w.WriteAssemblyName(v));

		public void WriteHeader((string Package, NuGetVersion Version) source, (string Package, NuGetVersion Version) target) =>
			_writers.ForEach(w => w.WriteHeader(source, target));

		public void WriteMissingEvent(EventDefinition eventDef, bool isIgnored) =>
			_writers.ForEach(w => w.WriteMissingEvent(eventDef, isIgnored));
		public void WriteMissingEventsEnd() => _writers.ForEach(w => w?.WriteMissingEventsEnd());
		public void WriteMissingEventsForTypeEnd() => _writers.ForEach(w => w.WriteMissingEventsEnd());
		public void WriteMissingEventsForTypeStart(string typeName) => _writers.ForEach(w => w.WriteMissingEventsForTypeStart(typeName));
		public void WriteMissingEventsStart(int invalidEventsCount) => _writers.ForEach(w => w.WriteMissingEventsStart(invalidEventsCount));

		public void WriteMissingField(FieldDefinition fieldDef, bool isIgnored) => _writers.ForEach(w => w.WriteMissingField(fieldDef, isIgnored));
		public void WriteMissingFieldForTypeEnd() => _writers.ForEach(w => w.WriteMissingFieldForTypeEnd());
		public void WriteMissingFieldForTypeStart(string typeName) => _writers.ForEach(w => w.WriteMissingFieldForTypeStart(typeName));
		public void WriteMissingFieldsEnd() => _writers.ForEach(w => w.WriteMissingFieldsEnd());
		public void WriteMissingFieldsStart(int invalidFieldsCount) => _writers.ForEach(w => w.WriteMissingFieldsStart(invalidFieldsCount));

		public void WriteMissingMethod(MethodDefinition methodDef, bool isIgnored) => _writers.ForEach(w => w.WriteMissingMethod(methodDef, isIgnored));
		public void WriteMissingMethodsEnd() => _writers.ForEach(w => w.WriteMissingMethodsEnd());
		public void WriteMissingMethodsForTypeEnd() => _writers.ForEach(w => w.WriteMissingMethodsForTypeEnd());
		public void WriteMissingMethodsForTypeStart(string typeName) => _writers.ForEach(w => w.WriteMissingMethodsForTypeStart(typeName));
		public void WriteMissingMethodsStart(int invalidMethodsCount) => _writers.ForEach(w => w.WriteMissingMethodsStart(invalidMethodsCount));

		public void WriteMissingPropertiesEnd() => _writers.ForEach(w => w.WriteMissingPropertiesEnd());
		public void WriteMissingPropertiesForTypeEnd() => _writers.ForEach(w => w.WriteMissingPropertiesForTypeEnd());
		public void WriteMissingPropertiesForTypeStart(string typeName) => _writers.ForEach(w => w.WriteMissingPropertiesForTypeStart(typeName));
		public void WriteMissingPropertiesStart(int invalidPropertiesCount) => _writers.ForEach(w => w.WriteMissingPropertiesStart(invalidPropertiesCount));
		public void WriteMissingProperty(PropertyDefinition propertyDef, bool isIgnored) => _writers.ForEach(w => w.WriteMissingProperty(propertyDef, isIgnored));

		public void WriteMissingType(TypeDefinition typeDef, bool isIgnored) => _writers.ForEach(w => w.WriteMissingType(typeDef, isIgnored));
		public void WriteMissingTypesEnd() => _writers.ForEach(w => w.WriteMissingTypesEnd());
		public void WriteMissingTypesStart(int invalidTypesCount) => _writers.ForEach(w => w.WriteMissingTypesStart(invalidTypesCount));

		public void WritePlatform(string platform) => _writers.ForEach(w => w.WritePlatform(platform));

		public int Count => _writers.Count;
		public void Add(IDiffWriter writer) => _writers.Add(writer);
	}
}
