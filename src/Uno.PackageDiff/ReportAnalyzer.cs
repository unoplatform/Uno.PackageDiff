using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Uno.PackageDiff
{
	internal class ReportAnalyzer
	{
		public static bool GenerateReport(IDiffWriter writer, ComparisonResult results, IgnoreSet ignoreSet)
		{
			var ignoredTypeNames = ignoreSet?.Types.Select(t2 => t2.FullName).ToArray();
			var isFailed = false;
			isFailed |= ReportMissingTypes(writer, results, ignoreSet);
			isFailed |= ReportMethods(writer, results, ignoreSet, ignoredTypeNames);
			isFailed |= ReportEvents(writer, results, ignoreSet, ignoredTypeNames);
			isFailed |= ReportFields(writer, results, ignoreSet, ignoredTypeNames);
			isFailed |= ReportProperties(writer, results, ignoreSet, ignoredTypeNames);

			return isFailed;
		}

		private static bool ReportMissingTypes(IDiffWriter writer, ComparisonResult results, IgnoreSet ignoreSet)
		{
			var shouldFail = false;
			writer.WriteMissingTypesStart(results.InvalidTypes.Length);
			foreach(var invalidType in results.InvalidTypes)
			{
				var isIgnored = ignoreSet.Types
					.Select(t => t.FullName)
					.Contains(invalidType.ToSignature());

				if(!isIgnored)
				{
					Console.WriteLine($"Error : Removed type {invalidType.ToSignature()} not found in ignore set.");
					shouldFail = true;
				}

				writer.WriteMissingType(invalidType, isIgnored);
			}
			writer.WriteMissingTypesEnd();

			return shouldFail;
		}

		private static bool ReportProperties(IDiffWriter writer, ComparisonResult results, IgnoreSet ignoreSet, string[] ignoredTypeNames)
		{
			var shouldFail = false;

			var groupedProperties = from method in results.InvalidProperties
									group method by method.DeclaringType.FullName into types
									select types;

			writer.WriteMissingPropertiesStart(results.InvalidProperties.Length);

			foreach(var updatedType in groupedProperties)
			{
				writer.WriteMissingPropertiesForTypeStart(updatedType.Key);
				foreach(var property in updatedType)
				{
					var isIgnored = ignoreSet.Properties
						.Select(t => t.FullName)
						.Contains(property.ToSignature())
						|| IsDeclaringTypeIgnored(property, ignoredTypeNames);


					if(!isIgnored)
					{
						Console.WriteLine($"Error : Removed property {property.ToSignature()} not found in ignore set.");
						shouldFail = true;
					}

					writer.WriteMissingProperty(property, isIgnored);
				}
				writer.WriteMissingPropertiesForTypeEnd();
			}

			writer.WriteMissingPropertiesEnd();

			return shouldFail;
		}

		private static bool ReportFields(IDiffWriter writer, ComparisonResult results, IgnoreSet ignoreSet, string[] ignoredTypeNames)
		{
			var shouldFail = false;

			var groupedFields = from method in results.InvalidFields
								group method by method.DeclaringType.FullName into types
								select types;

			writer.WriteMissingFieldsStart(results.InvalidFields.Length);

			foreach(var updatedType in groupedFields)
			{
				writer.WriteMissingFieldForTypeStart(updatedType.Key);
				foreach(var field in updatedType)
				{
					var isIgnored = ignoreSet.Fields
						.Select(t => t.FullName)
						.Contains(field.ToSignature())
						|| IsDeclaringTypeIgnored(field, ignoredTypeNames);

					if(!isIgnored)
					{
						Console.WriteLine($"Error : Removed field {field.ToSignature()} not found in ignore set.");
						shouldFail = true;
					}

					writer.WriteMissingField(field, isIgnored);
				}
				writer.WriteMissingFieldForTypeEnd();
			}
			writer.WriteMissingFieldsEnd();
			return shouldFail;
		}

		private static bool ReportMethods(IDiffWriter writer, ComparisonResult results, IgnoreSet ignoreSet, string[] ignoredTypeNames)
		{
			var shouldFail = false;

			var groupedMethods = from method in results.InvalidMethods
								 group method by method.DeclaringType.FullName into types
								 select types;

			writer.WriteMissingMethodsStart(results.InvalidMethods.Length);

			foreach(var updatedType in groupedMethods)
			{
				writer.WriteMissingMethodsForTypeStart(updatedType.Key);
				foreach(var method in updatedType)
				{
					var methodSignature = method.ToSignature();

					var isIgnored = ignoreSet.Methods
						.Select(t => t.FullName)
						.Contains(methodSignature)
						|| IsDeclaringTypeIgnored(method, ignoredTypeNames);

					if(!isIgnored)
					{
						Console.WriteLine($"Error : Removed method {method.ToSignature()} not found in ignore set.");
						shouldFail = true;
					}

					writer.WriteMissingMethod(method, isIgnored);
				}
				writer.WriteMissingMethodsForTypeEnd();
			}
			writer.WriteMissingMethodsEnd();

			return shouldFail;
		}

		private static bool ReportEvents(IDiffWriter writer, ComparisonResult results, IgnoreSet ignoreSet, string[] ignoredTypeNames)
		{
			var shouldFail = false;

			var groupedEvents = from method in results.InvalidEvents
								group method by method.DeclaringType.FullName into types
								select types;

			writer.WriteMissingEventsStart(results.InvalidEvents.Length);

			foreach(var updatedType in groupedEvents)
			{
				writer.WriteMissingEventsForTypeStart(updatedType.Key);
				foreach(var evt in updatedType)
				{
					var isIgnored = ignoreSet.Events
						.Select(t => t.FullName)
						.Contains(evt.ToString())
						|| IsDeclaringTypeIgnored(evt, ignoredTypeNames);

					var strike = isIgnored
						? "~~" : "";

					if(!isIgnored)
					{
						Console.WriteLine($"Error : Removed event {evt.ToSignature()} not found in ignore set.");
						shouldFail = true;
					}

					writer.WriteMissingEvent(evt, isIgnored);
				}
				writer.WriteMissingEventsForTypeEnd();
			}
			writer.WriteMissingMethodsEnd();

			return shouldFail;
		}

		private static bool IsDeclaringTypeIgnored(IMemberDefinition memberDefinition, string[] ignoredTypes)
			=> ignoredTypes.Contains(memberDefinition.DeclaringType.ToSignature());
	}
}
