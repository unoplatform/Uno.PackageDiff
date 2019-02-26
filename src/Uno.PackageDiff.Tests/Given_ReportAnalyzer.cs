using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Uno.PackageDiff.Tests
{
	[TestClass]
	public class Given_ReportAnalyzer
	{
		private readonly TestBuilder _builder;

		public Given_ReportAnalyzer()
		{
			_builder = new TestBuilder(GetType());
		}

		[TestMethod]
		public void When_Empty_IgnoreSet()
		{
			var context = _builder.BuildAssemblies();

			var res = AssemblyComparer.CompareTypes(context.BaseAssembly, context.TargetAssembly);

			Assert.IsFalse(ReportAnalyzer.IsDiffFailed(res, context.IgnoreSet));
		}

		[TestMethod]
		public void When_IgnoreProperty()
		{
			var context = _builder.BuildAssemblies();

			var res = AssemblyComparer.CompareTypes(context.BaseAssembly, context.TargetAssembly);

			Assert.IsNotNull(context.IgnoreSet);
			Assert.IsTrue(ReportAnalyzer.IsDiffFailed(res, context.IgnoreSet));
		}
	}
}
