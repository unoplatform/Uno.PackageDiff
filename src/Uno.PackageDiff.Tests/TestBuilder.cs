using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;
using Mono.Cecil;

namespace Uno.PackageDiff.Tests
{
	internal class TestBuilder
	{
		private readonly Type _ownerType;

		public TestBuilder(Type owner)
		{
			this._ownerType = owner;
		}

		public ComparisonContext BuildAssemblies([CallerMemberName] string testName = null, string currentVersion = "1.0.0")
		{
			var baseFile = _ownerType.Assembly.GetManifestResourceNames().FirstOrDefault(f => f.Contains($"{_ownerType.Name}_{testName}.base.cs"));
			var targetFile = _ownerType.Assembly.GetManifestResourceNames().FirstOrDefault(f => f.Contains($"{_ownerType.Name}_{testName}.target.cs"));
			var diffIgnore = _ownerType.Assembly.GetManifestResourceNames().FirstOrDefault(f => f.Contains($"{_ownerType.Name}_{testName}.diff.xml"));

			return new ComparisonContext(
				BuildDefinition(baseFile),
				BuildDefinition(targetFile),
				diffIgnore != null ? BuildDiffIgnore(currentVersion, diffIgnore) : null
			);
		}

		private IgnoreSet BuildDiffIgnore(string currentVersion, string diffIgnore)
		{
			var reader = new StreamReader(_ownerType.Assembly.GetManifestResourceStream(diffIgnore)).ReadToEnd();

			using(var s = _ownerType.Assembly.GetManifestResourceStream(diffIgnore))
			{
				return DiffIgnore.ParseDiffIgnore(currentVersion, s);
			}
		}

		public AssemblyDefinition BuildDefinition(string name)
		{
			using(var s = new StreamReader(_ownerType.Assembly.GetManifestResourceStream(name)))
			{
				return ModuleBuilder.BuildAssemblyDefinition(s.ReadToEnd());
			}
		}
	}
}
