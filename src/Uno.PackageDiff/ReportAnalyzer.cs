using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uno.PackageDiff
{
	public class ReportAnalyzer
	{
		public static bool IsDiffFailed(ComparisonResult results, IgnoreSet ignoreSet)
		{
			var ignoredTypes = ignoreSet.Types.Select(t2 => t2.FullName);
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
	}
}
