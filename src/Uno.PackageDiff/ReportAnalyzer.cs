using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Uno.PackageDiff
{
	public class ReportAnalyzer
	{
		public static bool GenerateReport(StreamWriter writer, ComparisonResult results, IgnoreSet ignoreSet)
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

		private static bool ReportMissingTypes(StreamWriter writer, ComparisonResult results, IgnoreSet ignoreSet)
		{
			var shouldFail = false;
			writer.WriteLine("### {0} missing types:", results.InvalidTypes.Length);
			foreach(var invalidType in results.InvalidTypes)
			{
				var isIgnored = ignoreSet.Types
					.Select(t => t.FullName)
					.Contains(invalidType.ToSignature());
				var strike = isIgnored
					? "~~" : "";

				if(!isIgnored)
				{
					Console.WriteLine($"Error : Removed type {invalidType.ToSignature()} not found in ignore set.");
					shouldFail = true;
				}

				writer.WriteLine($"* {strike}`{invalidType.ToSignature()}`{strike}");
			}

			return shouldFail;
		}

		private static bool ReportProperties(StreamWriter writer, ComparisonResult results, IgnoreSet ignoreSet, string[] ignoredTypeNames)
		{
			var shouldFail = false;

			var groupedProperties = from method in results.InvalidProperties
									group method by method.DeclaringType.FullName into types
									select types;

			writer.WriteLine("### {0} missing or changed properties in existing types:", results.InvalidProperties.Length);

			foreach(var updatedType in groupedProperties)
			{
				writer.WriteLine("- `{0}`", updatedType.Key);
				foreach(var property in updatedType)
				{
					var isIgnored = ignoreSet.Properties
						.Select(t => t.FullName)
						.Contains(property.ToSignature())
						|| IsDeclaringTypeIgnored(property, ignoredTypeNames);

					var strike = isIgnored
						? "~~" : "";

					if(!isIgnored)
					{
						Console.WriteLine($"Error : Removed property {property.ToSignature()} not found in ignore set.");
						shouldFail = true;
					}

					writer.WriteLine($"\t* {strike}``{property.ToSignature()}``{strike}");
				}
			}

			return shouldFail;
		}

		private static bool ReportFields(StreamWriter writer, ComparisonResult results, IgnoreSet ignoreSet, string[] ignoredTypeNames)
		{
			var shouldFail = false;

			var groupedFields = from method in results.InvalidFields
								group method by method.DeclaringType.FullName into types
								select types;

			writer.WriteLine("### {0} missing or changed fields in existing types:", results.InvalidFields.Length);

			foreach(var updatedType in groupedFields)
			{
				writer.WriteLine("- `{0}`", updatedType.Key);
				foreach(var field in updatedType)
				{
					var isIgnored = ignoreSet.Fields
						.Select(t => t.FullName)
						.Contains(field.ToSignature())
						|| IsDeclaringTypeIgnored(field, ignoredTypeNames);

					var strike = isIgnored
						? "~~" : "";

					if(!isIgnored)
					{
						Console.WriteLine($"Error : Removed field {field.ToSignature()} not found in ignore set.");
						shouldFail = true;
					}

					writer.WriteLine($"\t* {strike}``{field.ToSignature()}``{strike}");
				}
			}

			return shouldFail;
		}

		private static bool ReportMethods(StreamWriter writer, ComparisonResult results, IgnoreSet ignoreSet, string[] ignoredTypeNames)
		{
			var shouldFail = false;

			var groupedMethods = from method in results.InvalidMethods
								 group method by method.DeclaringType.FullName into types
								 select types;

			writer.WriteLine("### {0} missing or changed method in existing types:", results.InvalidMethods.Length);

			foreach(var updatedType in groupedMethods)
			{
				writer.WriteLine("- `{0}`", updatedType.Key);
				foreach(var method in updatedType)
				{
					var methodSignature = method.ToSignature();

					var isIgnored = ignoreSet.Methods
						.Select(t => t.FullName)
						.Contains(methodSignature)
						|| IsDeclaringTypeIgnored(method, ignoredTypeNames);

					var strike = isIgnored
						? "~~" : "";

					if(!isIgnored)
					{
						Console.WriteLine($"Error : Removed method {method.ToSignature()} not found in ignore set.");
						shouldFail = true;
					}

					writer.WriteLine($"\t* {strike}``{methodSignature}``{strike}");
				}
			}

			return shouldFail;
		}

		private static bool ReportEvents(StreamWriter writer, ComparisonResult results, IgnoreSet ignoreSet, string[] ignoredTypeNames)
		{
			var shouldFail = false;

			var groupedEvents = from method in results.InvalidEvents
								group method by method.DeclaringType.FullName into types
								select types;

			writer.WriteLine("### {0} missing or changed events in existing types:", results.InvalidEvents.Length);

			foreach(var updatedType in groupedEvents)
			{
				writer.WriteLine("- `{0}`", updatedType.Key);
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

					writer.WriteLine($"\t* {strike}``{evt.ToSignature()}``{strike}");
				}
			}

			return shouldFail;
		}

		private static bool IsDeclaringTypeIgnored(IMemberDefinition memberDefinition, string[] ignoredTypes)
			=> ignoredTypes.Contains(memberDefinition.DeclaringType.ToSignature());
	}
}
