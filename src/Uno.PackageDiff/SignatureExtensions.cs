using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Uno.PackageDiff
{
	public static class SignatureExtensions
	{
		public static string ToSignature(this TypeDefinition type)
			=> type.ToString();

		public static string ToSignature(this EventDefinition evt)
			=> evt.ToString();

		public static string ToSignature(this PropertyDefinition property)
			=> property.ToString();

		public static string ToSignature(this FieldDefinition field)
			=> field.ToString();

		public static string ToSignature(this MethodDefinition method)
		{
			var parms = string.Join(", ", method.Parameters.Select(p => $"{p.ParameterType} {p.Name}"));
			return $"{method.ReturnType} {method.DeclaringType}.{method.Name}({parms})";
		}
	}
}
