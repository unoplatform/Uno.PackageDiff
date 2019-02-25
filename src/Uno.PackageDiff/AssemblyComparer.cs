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
					 where targetMethod.IsPublic
					 where !type.sourceType.Methods.Any(m => m.Name == targetMethod.Name && targetMethodParams.SequenceEqual(getMethodParamsSignature(m)))
					 select targetMethod;

			return q1.ToArray();
		}

		private static string ExpandMethod(MethodDefinition method)
		{
			var parms = string.Join(", ", method.Parameters.Select(p => $"{p.ParameterType} {p.Name}"));
			return $"{method.ReturnType} {method.DeclaringType}.{method.Name}({parms})";
		}

		private static EventDefinition[] FindMissingEvents(IEnumerable<(TypeDefinition sourceType, TypeDefinition targetType)> existingTypes)
		{
			var q1 = from type in existingTypes
					 from targetEvent in type.targetType.Events
					 where targetEvent.AddMethod?.IsPublic ?? false
					 where !type.sourceType.Events.Any(p => p.Name == targetEvent.Name && p.EventType.FullName != targetEvent.FullName)
					 select targetEvent;

			return q1.ToArray();
		}

		private static FieldDefinition[] FindMissingFields(IEnumerable<(TypeDefinition sourceType, TypeDefinition targetType)> existingTypes)
		{
			var q1 = from type in existingTypes
					 from targetField in type.targetType.Fields
					 where targetField.IsPublic
					 where !type.sourceType.Fields.Any(p => p.Name == targetField.Name && p.FieldType.FullName == targetField.FieldType.FullName)
					 select targetField;

			return q1.ToArray();
		}

		private static PropertyDefinition[] FindMissingProperties(IEnumerable<(TypeDefinition sourceType, TypeDefinition targetType)> existingTypes)
		{
			var q1 = from type in existingTypes
					 from targetProp in type.targetType.Properties
					 where targetProp.GetMethod?.IsPublic ?? false
					 where !type.sourceType.Properties.Any(p => p.Name == targetProp.Name && p.PropertyType.FullName == targetProp.PropertyType.FullName)
					 select targetProp;

			return q1.ToArray();
		}

	}
}
