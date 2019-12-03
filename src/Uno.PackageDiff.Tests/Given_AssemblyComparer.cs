using System.Diagnostics;
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
		public void When_DerivingClasses()
		{
			var context = _builder.BuildAssemblies();

			var r = AssemblyComparer.CompareTypes(context.BaseAssembly, context.TargetAssembly);

			Assert.AreEqual(0, r.InvalidTypes.Length);
			Assert.AreEqual(0, r.InvalidEvents.Length);
			Assert.AreEqual(0, r.InvalidFields.Length);
			Assert.AreEqual(2, r.InvalidMethods.Length);
			Assert.AreEqual(0, r.InvalidProperties.Length);

			Assert.AreEqual("System.Void Uno.PackageDiff.Tests.Sources.When_DerivingClasses_Base::VirtualMethod2()", r.InvalidMethods.ElementAt(0).ToString());
			Assert.AreEqual("System.Void Uno.PackageDiff.Tests.Sources.When_DerivingClasses_Base::VirtualMethod3()", r.InvalidMethods.ElementAt(1).ToString());
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

		[TestMethod]
		public void When_Target_Internal()
		{
			var context = _builder.BuildAssemblies();

			var r = AssemblyComparer.CompareTypes(context.BaseAssembly, context.TargetAssembly);

			Assert.AreEqual(0, r.InvalidTypes.Length);
			Assert.AreEqual(0, r.InvalidEvents.Length);
			Assert.AreEqual(0, r.InvalidFields.Length);
			Assert.AreEqual(3, r.InvalidMethods.Length);
			Assert.AreEqual(1, r.InvalidProperties.Length);

			Assert.AreEqual("MyProperty", r.InvalidProperties.First().Name);
			Assert.AreEqual("System.Int32 Uno.PackageDiff.Tests.Sources.When_Target_Internal::get_MyProperty()", r.InvalidMethods.ElementAt(0).ToString());
			Assert.AreEqual("System.Void Uno.PackageDiff.Tests.Sources.When_Target_Internal::set_MyProperty(System.Int32)", r.InvalidMethods.ElementAt(1).ToString());
			Assert.AreEqual("System.Void Uno.PackageDiff.Tests.Sources.When_Target_Internal::MyMethod()", r.InvalidMethods.ElementAt(2).ToString());
		}

		[TestMethod]
		public void When_Target_Method_ChangedReturnType()
		{
			var context = _builder.BuildAssemblies();
			
			var r = AssemblyComparer.CompareTypes(context.BaseAssembly, context.TargetAssembly);

			Assert.AreEqual(0, r.InvalidTypes.Length);
			Assert.AreEqual(0, r.InvalidEvents.Length);
			Assert.AreEqual(0, r.InvalidFields.Length);
			Assert.AreEqual(1, r.InvalidMethods.Length);
			Assert.AreEqual(0, r.InvalidProperties.Length);

			Assert.AreEqual("TestMethod", r.InvalidMethods.ElementAt(0).Name);
			Assert.AreEqual("System.Threading.Tasks.Task Uno.PackageDiff.Tests.Sources.When_Target_Method_ChangedReturnType::TestMethod()", r.InvalidMethods.ElementAt(0).ToString());
		}
	}
}
