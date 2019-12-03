using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Uno.PackageDiff
{
	public class ComparisonResult
	{
		public ComparisonResult(TypeDefinition[] invalidTypes,
						  PropertyDefinition[] invalidProperties,
						  MethodDefinition[] invalidMethods,
						  FieldDefinition[] invalidFields,
						  EventDefinition[] invalidEvents)
		{
			InvalidTypes = invalidTypes;
			InvalidProperties = invalidProperties;
			InvalidMethods = invalidMethods;
			InvalidFields = invalidFields;
			InvalidEvents = invalidEvents;
		}

		public TypeDefinition[] InvalidTypes { get; }
		public PropertyDefinition[] InvalidProperties { get; }
		public MethodDefinition[] InvalidMethods { get; }
		public FieldDefinition[] InvalidFields { get; }
		public EventDefinition[] InvalidEvents { get; }
	}

	public class AssemblyComparer
	{
		public static ComparisonResult CompareTypes(AssemblyDefinition baseAssembly, AssemblyDefinition targetAssembly)
		{
			var baseTypes = baseAssembly.MainModule.GetTypes();
			var targetTypes = targetAssembly.MainModule.GetTypes();

			// Types only in target
			var q = from targetType in baseTypes
					where !targetTypes.Any(t => t.FullName == targetType.FullName)
					where targetType.IsPublic
					select targetType;

			var invalidTypes = q.ToArray();

			var existingTypes = (
				from targetType in baseTypes
				let sourceType = targetTypes.FirstOrDefault(t => t.FullName == targetType.FullName)
				where sourceType != null
				where targetType.IsPublic
				select (sourceType, targetType)
			).ToArray();

			var invalidProperties = FindMissingProperties(existingTypes);
			var invalidMethods = FindMissingMethods(existingTypes);
			var invalidFields = FindMissingFields(existingTypes);
			var invalidEvents = FindMissingEvents(existingTypes);

			return new ComparisonResult(invalidTypes, invalidProperties, invalidMethods, invalidFields, invalidEvents);
		}

		private static MethodDefinition[] FindMissingMethods(IEnumerable<(TypeDefinition sourceType, TypeDefinition targetType)> existingTypes)
		{
			IEnumerable<string> getMethodParamsSignature(MethodDefinition method)
				=> method.Parameters.Select(p => p.Name + p.ParameterType.FullName);

			var q1 = from type in existingTypes
					 from targetMethod in type.targetType.Methods
					 let targetMethodParams = getMethodParamsSignature(targetMethod)
					 where IsVisibleMethod(targetMethod)
					 where !targetMethod.IsVirtual || !targetMethod.IsReuseSlot
					 where !type.sourceType.Methods
						.Any(sourceMethod =>
						sourceMethod.Name == targetMethod.Name
						&& targetMethod.ReturnType.FullName == sourceMethod.ReturnType.FullName
						&& targetMethodParams.SequenceEqual(getMethodParamsSignature(sourceMethod))
						&& IsVisibleMethod(sourceMethod)
						&& targetMethod.IsVirtual == sourceMethod.IsVirtual)
					 select targetMethod;

			return q1.ToArray();
		}

		private static bool IsVisibleMethod(MethodDefinition targetMethod) => targetMethod != null && (targetMethod.IsPublic || targetMethod.IsFamily);

		private static string ExpandMethod(MethodDefinition method)
		{
			var parms = string.Join(", ", method.Parameters.Select(p => $"{p.ParameterType} {p.Name}"));
			return $"{method.ReturnType} {method.DeclaringType}.{method.Name}({parms})";
		}

		private static EventDefinition[] FindMissingEvents(IEnumerable<(TypeDefinition sourceType, TypeDefinition targetType)> existingTypes)
		{
			var q1 = from type in existingTypes
					 from targetEvent in type.targetType.Events
					 where IsVisibleMethod(targetEvent.AddMethod)
					 where !type.sourceType.Events
						.Any(sourceEvent =>
							sourceEvent.Name == targetEvent.Name
							&& sourceEvent.EventType.FullName != targetEvent.FullName
							&& IsVisibleMethod(sourceEvent.AddMethod))
					 select targetEvent;

			return q1.ToArray();
		}

		private static FieldDefinition[] FindMissingFields(IEnumerable<(TypeDefinition sourceType, TypeDefinition targetType)> existingTypes)
		{
			var q1 = from type in existingTypes
					 from targetField in type.targetType.Fields
					 where targetField.IsPublic || targetField.IsFamily
					 where !type.sourceType.Fields.Any(p => p.Name == targetField.Name && p.FieldType.FullName == targetField.FieldType.FullName)
					 select targetField;

			return q1.ToArray();
		}

		private static PropertyDefinition[] FindMissingProperties(IEnumerable<(TypeDefinition sourceType, TypeDefinition targetType)> existingTypes)
		{
			var q1 = from type in existingTypes
					 from targetProp in type.targetType.Properties
					 where IsVisibleMethod(targetProp.GetMethod)
					 where !type.sourceType.Properties
						.Any(sourceProperty =>
							sourceProperty.Name == targetProp.Name
							&& sourceProperty.PropertyType.FullName == targetProp.PropertyType.FullName
							&& IsVisibleMethod(sourceProperty.GetMethod))
					 select targetProp;

			return q1.ToArray();
		}

	}
}
