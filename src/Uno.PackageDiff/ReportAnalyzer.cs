using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Uno.PackageDiff
{
	internal class ReportAnalyzer
	{

		public static bool GenerateReport(RunningContext context, IReportWriter writer, ComparisonResult results, IgnoreSet ignoreSet)
		{
			var ignoredTypeNames = ignoreSet?.Types.Select(t2 => t2.FullName).ToArray();
			var isFailed = false;
			isFailed |= ReportMissingTypes(context, writer, results, ignoreSet);
			isFailed |= ReportMethods(context, writer, results, ignoreSet, ignoredTypeNames);
			isFailed |= ReportEvents(context, writer, results, ignoreSet, ignoredTypeNames);
			isFailed |= ReportFields(context, writer, results, ignoreSet, ignoredTypeNames);
			isFailed |= ReportProperties(context, writer, results, ignoreSet, ignoredTypeNames);

			return isFailed;
		}

		private static bool ReportMissingTypes(RunningContext context, IReportWriter writer, ComparisonResult results, IgnoreSet ignoreSet)
		{
			var shouldFail = false;
			List<(TypeDefinition InvalidType, bool IsIgnored, string IgnoreReason)> list = new(results.InvalidTypes.Length);
			foreach(var invalidType in results.InvalidTypes)
			{
				var reason = string.Empty;
				var isIgnored = false;
				if(ignoreSet.Types.FirstOrDefault((t, a) => string.Equals(t.FullName, a, StringComparison.OrdinalIgnoreCase), invalidType.ToSignature()) is Member member)
				{
					reason = member.Reason;
					isIgnored = true;
				}

				if(!isIgnored)
				{
					context?.Error($"Removed type {invalidType.ToSignature()} not found in ignore set.");
					shouldFail = true;
				}

				list.Add((invalidType, isIgnored, reason));
			}
			writer?.Write(list.AsReadOnly());
			return shouldFail;
		}

		private static bool ReportProperties(RunningContext context, IReportWriter writer, ComparisonResult results, IgnoreSet ignoreSet, string[] ignoredTypeNames)
		{
			var shouldFail = false;

			var groupedProperties = from method in results.InvalidProperties
									group method by method.DeclaringType.FullName into types
									select types;

			List<(string Type, IReadOnlyList<(PropertyDefinition Property, bool IsIgnored, string IgnoreReason)> InvalidProperties)> invalidProperties =
				new(results.InvalidProperties.Length);
			foreach(var updatedType in groupedProperties)
			{
				List<(PropertyDefinition Property, bool IsIgnored, string IgnoreReason)> properties = new();
				foreach(var property in updatedType)
				{
					var signature = property.ToSignature();
					var reason = string.Empty;
					var isIgnored = false;
					if(IsDeclaringTypeIgnored(property, ignoredTypeNames))
					{
						reason = $"The property {signature} is declaring in ingnored type {property.DeclaringType.Name}.";
						isIgnored = true;
					}
					else if(ignoreSet.Properties.FirstOrDefault((p, a) => string.Equals(p.FullName, a, StringComparison.OrdinalIgnoreCase), signature) is Member member)
					{
						reason = member.Reason;
						isIgnored = true;
					}

					if(!isIgnored)
					{
						Console.WriteLine($"Removed property {property.ToSignature()} not found in ignore set.");
						shouldFail = true;
					}

					properties.Add((property, isIgnored, reason));
				}
				invalidProperties.Add((updatedType.Key, properties));
			}
			writer?.Write(invalidProperties.AsReadOnly());

			return shouldFail;
		}

		private static bool ReportFields(RunningContext context, IReportWriter writer, ComparisonResult results, IgnoreSet ignoreSet, string[] ignoredTypeNames)
		{
			var shouldFail = false;

			var groupedFields = from method in results.InvalidFields
								group method by method.DeclaringType.FullName into types
								select types;

			List<(string Type, IReadOnlyList<(FieldDefinition Field, bool IsIgnored, string IgnoreReason)> InvalidFields)> invalidFields =
				new(results.InvalidFields.Length);

			foreach(var updatedType in groupedFields)
			{
				List<(FieldDefinition Field, bool IsIgnored, string IgnoreReason)> fields = new();
				foreach(var field in updatedType)
				{
					var signature = field.ToSignature();
					var reason = string.Empty;
					var isIgnored = false;
					if(IsDeclaringTypeIgnored(field, ignoredTypeNames))
					{
						reason = $"The field {signature} is declaring in ingnored type {field.DeclaringType.Name}.";
						isIgnored= true ;
					}
					else if(ignoreSet.Fields.FirstOrDefault((t, a) => string.Equals(t.FullName, a, StringComparison.OrdinalIgnoreCase), signature) is Member member)
					{
						reason = member.Reason;
						isIgnored= true ;
					}

					if(!isIgnored)
					{
						context?.Error($"Removed field {field.ToSignature()} not found in ignore set.");
						shouldFail = true;
					}

					fields.Add((field, isIgnored, reason));
				}
				invalidFields.Add((updatedType.Key, fields));
			}
			writer?.Write(invalidFields.AsReadOnly());
			return shouldFail;
		}

		private static bool ReportMethods(RunningContext context, IReportWriter writer, ComparisonResult results, IgnoreSet ignoreSet, string[] ignoredTypeNames)
		{
			var shouldFail = false;

			var groupedMethods = from method in results.InvalidMethods
								 group method by method.DeclaringType.FullName into types
								 select types;

			List<(string Type, IReadOnlyList<(MethodDefinition Method, bool IsIgnored, string IgnoreReason)> InvalidMethods)> invalidMethods =
				new(results.InvalidMethods.Length);

			foreach(var updatedType in groupedMethods)
			{
				List<(MethodDefinition Method, bool IsIgnored, string IgnoreReason)> methods = new();
				foreach(var method in updatedType)
				{
					var signature = method.ToSignature();
					var reason = string.Empty;
					var isIgnored = false;
					if(IsDeclaringTypeIgnored(method, ignoredTypeNames))
					{
						reason = $"The method {signature} is declaring in ingnored type {method.DeclaringType.Name}.";
						isIgnored = true;
					}
					else if(ignoreSet.Methods.FirstOrDefault((t, a) => string.Equals(t.FullName, a, StringComparison.OrdinalIgnoreCase), signature) is Member member)
					{
						reason = member.Reason;
						isIgnored = true;
					}

					if(!isIgnored)
					{
						context?.Error($"Removed method {method.ToSignature()} not found in ignore set.");
						shouldFail = true;
					}

					methods.Add((method, isIgnored, reason));
				}
				invalidMethods.Add((updatedType.Key, methods));
			}
			writer?.Write(invalidMethods.AsReadOnly());

			return shouldFail;
		}

		private static bool ReportEvents(RunningContext context, IReportWriter writer, ComparisonResult results, IgnoreSet ignoreSet, string[] ignoredTypeNames)
		{
			var shouldFail = false;

			var groupedEvents = from method in results.InvalidEvents
								group method by method.DeclaringType.FullName into types
								select types;

			List<(string Type, IReadOnlyList<(EventDefinition Events, bool IsIgnored, string IgnoreReason)> InvalidEvents)> invalidEvents =
				new(results.InvalidEvents.Length);

			foreach(var updatedType in groupedEvents)
			{
				List<(EventDefinition Event, bool IsIgnored, string IgnoreReason)> events = new();
				foreach(var evt in updatedType)
				{
					var signature = evt.ToSignature();
					var reason = string.Empty;
					var isIgnored = false;
					if(IsDeclaringTypeIgnored(evt, ignoredTypeNames))
					{
						reason = $"The event {signature} is declaring in ingnored type {evt.DeclaringType.Name}.";
						isIgnored = true;
					}
					else if(ignoreSet.Events.FirstOrDefault((t, a) => string.Equals(t.FullName, a, StringComparison.OrdinalIgnoreCase), signature) is Member member)
					{
						reason = member.Reason;
						isIgnored = true;
					}

					if(!isIgnored)
					{
						context?.Error($"Removed event {evt.ToSignature()} not found in ignore set.");
						shouldFail = true;
					}

					events.Add((evt, isIgnored, reason));
				}
				invalidEvents.Add((updatedType.Key, events));
			}
			writer?.Write(invalidEvents.AsReadOnly());

			return shouldFail;
		}

		private static bool IsDeclaringTypeIgnored(IMemberDefinition memberDefinition, string[] ignoredTypes)
			=> ignoredTypes.Contains(memberDefinition.DeclaringType.ToSignature());
	}
}
