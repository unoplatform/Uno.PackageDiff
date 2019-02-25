using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;

namespace Uno.PackageDiff.Tests
{
	[TestClass]
	public class Given_AssemblyComparer
	{
		private (AssemblyDefinition baseAssembly, AssemblyDefinition targetAssembly) BuildAssemblies([CallerMemberName] string testName = null)
		{
			var baseFile = GetType().Assembly.GetManifestResourceNames().FirstOrDefault(f => f.Contains(testName + ".base.cs"));
			var targetFile = GetType().Assembly.GetManifestResourceNames().FirstOrDefault(f => f.Contains(testName + ".target.cs"));

			return (BuildDefinition(baseFile), BuildDefinition(targetFile));
		}

		AssemblyDefinition BuildDefinition(string name)
		{
			using(var s = new StreamReader(this.GetType().Assembly.GetManifestResourceStream(name)))
			{
				return ModuleBuilder.BuildAssemblyDefinition(s.ReadToEnd());
			}
		}

		[TestMethod]
		public void When_EmptyAssembly()
		{
			var (baseAssembly, targetAssembly) = BuildAssemblies();

			var r = AssemblyComparer.CompareTypes(baseAssembly, targetAssembly);

			Assert.AreEqual(0, r.InvalidTypes.Length);
			Assert.AreEqual(0, r.InvalidEvents.Length);
			Assert.AreEqual(0, r.InvalidFields.Length);
			Assert.AreEqual(0, r.InvalidMethods.Length);
			Assert.AreEqual(0, r.InvalidProperties.Length);
		}

		[TestMethod]
		public void When_Target_MissingProperty()
		{
			var (baseAssembly, targetAssembly) = BuildAssemblies();

			var r = AssemblyComparer.CompareTypes(baseAssembly, targetAssembly);

			Assert.AreEqual(0, r.InvalidTypes.Length);
			Assert.AreEqual(0, r.InvalidEvents.Length);
			Assert.AreEqual(0, r.InvalidFields.Length);
			Assert.AreEqual(2, r.InvalidMethods.Length);
			Assert.AreEqual(1, r.InvalidProperties.Length);
			Assert.AreEqual("MyProperty", r.InvalidProperties.First().Name);
		}
	}
}
