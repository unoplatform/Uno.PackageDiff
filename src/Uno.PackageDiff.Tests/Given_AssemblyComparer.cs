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
		private readonly TestBuilder _builder;

		public Given_AssemblyComparer()
		{
			_builder = new TestBuilder(GetType());
		}

		[TestMethod]
		public void When_EmptyAssembly()
		{
			var context = _builder.BuildAssemblies();

			var r = AssemblyComparer.CompareTypes(context.BaseAssembly, context.TargetAssembly);

			Assert.AreEqual(0, r.InvalidTypes.Length);
			Assert.AreEqual(0, r.InvalidEvents.Length);
			Assert.AreEqual(0, r.InvalidFields.Length);
			Assert.AreEqual(0, r.InvalidMethods.Length);
			Assert.AreEqual(0, r.InvalidProperties.Length);
		}

		[TestMethod]
		public void When_Target_MissingProperty()
		{
			var context = _builder.BuildAssemblies();

			var r = AssemblyComparer.CompareTypes(context.BaseAssembly, context.TargetAssembly);

			Assert.AreEqual(0, r.InvalidTypes.Length);
			Assert.AreEqual(0, r.InvalidEvents.Length);
			Assert.AreEqual(0, r.InvalidFields.Length);
			Assert.AreEqual(2, r.InvalidMethods.Length);
			Assert.AreEqual(1, r.InvalidProperties.Length);
			Assert.AreEqual("MyProperty", r.InvalidProperties.First().Name);
		}
	}
}
