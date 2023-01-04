using System;
using NuGet.Versioning;
using Mono.Cecil;

namespace Uno.PackageDiff
{
	internal interface IDiffWriter : IDisposable
	{
		void WriteAssemblyName(string v);

		void WriteHeader((string Package, NuGetVersion Version) source, (string Package, NuGetVersion Version) target);

		void WritePlatform(string platform);

		void WriteMissingTypesStart(int invalidTypesCount);
		void WriteMissingType(TypeDefinition typeDef, bool isIgnored);
		void WriteMissingTypesEnd();

		void WriteMissingPropertiesStart(int invalidPropertiesCount);
		void WriteMissingPropertiesForTypeStart(string typeName);
		void WriteMissingProperty(PropertyDefinition propertyDef, bool isIgnored);
		void WriteMissingPropertiesForTypeEnd();
		void WriteMissingPropertiesEnd();

		void WriteMissingFieldsStart(int invalidFieldsCount);
		void WriteMissingFieldForTypeStart(string typeName);
		void WriteMissingField(FieldDefinition fieldDef, bool isIgnored);
		void WriteMissingFieldForTypeEnd();
		void WriteMissingFieldsEnd();

		void WriteMissingMethodsStart(int invalidMethodsCount);
		void WriteMissingMethodsForTypeStart(string typeName);
		void WriteMissingMethod(MethodDefinition methodDef, bool isIgnored);
		void WriteMissingMethodsForTypeEnd();
		void WriteMissingMethodsEnd();

		void WriteMissingEventsStart(int invalidEventsCount);
		void WriteMissingEventsForTypeStart(string typeName);
		void WriteMissingEvent(EventDefinition eventDef, bool isIgnored);
		void WriteMissingEventsForTypeEnd();
		void WriteMissingEventsEnd();
	}
}
