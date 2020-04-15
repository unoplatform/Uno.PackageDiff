using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Uno.PackageDiff
{
	public class ReportAnalyzer
	{
		public static bool IsDiffFailed(ComparisonResult results, IgnoreSet ignoreSet)
		{
			var ignoredTypes = ignoreSet?.Types.Select(t2 => t2.FullName);
			var failedTypes = results.InvalidTypes.Any(t => !ignoredTypes.Contains(t.ToSignature()));
			var failedEvents = results.InvalidEvents.Any(e => !ignoreSet.Events.Select(t => t.FullName).Contains(e.ToSignature())
				&& !ignoredTypes.Contains(e.DeclaringType.ToSignature()));
			var failedFields = results.InvalidFields.Any(f => !ignoreSet.Fields.Select(t => t.FullName).Contains(f.ToSignature())
				&& !ignoredTypes.Contains(f.DeclaringType.ToSignature()));
			var failedMethods = results.InvalidMethods.Any(m => !ignoreSet.Methods.Select(t => t.FullName).Contains(m.ToSignature())
				&& !ignoredTypes.Contains(m.DeclaringType.ToSignature()));
			var failedProperties = results.InvalidProperties.Any(p => !ignoreSet.Properties.Select(t => t.FullName).Contains(p.ToSignature())
				&& !ignoredTypes.Contains(p.DeclaringType.ToSignature()));

			return failedTypes
				|| failedEvents
				|| failedFields
				|| failedMethods
				|| failedProperties;
		}

		public static void GetReport(StreamWriter writer, ComparisonResult results, IgnoreSet ignoreSet)
		{
			ReportMissingTypes(writer, results, ignoreSet);
			ReportMethods(writer, results, ignoreSet);
			ReportEvents(writer, results, ignoreSet);
			ReportFields(writer, results, ignoreSet);
			ReportProperties(writer, results, ignoreSet);
		}

		private static void ReportMissingTypes(StreamWriter writer, ComparisonResult results, IgnoreSet ignoreSet)
		{
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
				}

				writer.WriteLine($"* {strike}`{invalidType.ToSignature()}`{strike}");
			}
		}

		private static void ReportProperties(StreamWriter writer, ComparisonResult results, IgnoreSet ignoreSet)
		{
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
						.Contains(property.ToSignature());
					var strike = isIgnored
						? "~~" : "";

					if(!isIgnored)
					{
						Console.WriteLine($"Error : Removed property {property.ToSignature()} not found in ignore set.");
					}

					writer.WriteLine($"\t* {strike}``{property.ToSignature()}``{strike}");
				}
			}
		}

		private static void ReportFields(StreamWriter writer, ComparisonResult results, IgnoreSet ignoreSet)
		{
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
						.Contains(field.ToSignature());
					var strike = isIgnored
						? "~~" : "";

					if(!isIgnored)
					{
						Console.WriteLine($"Error : Removed field {field.ToSignature()} not found in ignore set.");
					}

					writer.WriteLine($"\t* {strike}``{field.ToSignature()}``{strike}");
				}
			}
		}

		private static void ReportMethods(StreamWriter writer, ComparisonResult results, IgnoreSet ignoreSet)
		{
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
						.Contains(methodSignature);
					var strike = isIgnored
						? "~~" : "";

					if(!isIgnored)
					{
						Console.WriteLine($"Error : Removed method {method.ToSignature()} not found in ignore set.");
					}

					writer.WriteLine($"\t* {strike}``{methodSignature}``{strike}");
				}
			}
		}

		private static void ReportEvents(StreamWriter writer, ComparisonResult results, IgnoreSet ignoreSet)
		{
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
						.Contains(evt.ToString());
					var strike = isIgnored
						? "~~" : "";

					if(!isIgnored)
					{
						Console.WriteLine($"Error : Removed event {evt.ToSignature()} not found in ignore set.");
					}

					writer.WriteLine($"\t* {strike}``{evt.ToSignature()}``{strike}");
				}
			}
		}
	}
}
