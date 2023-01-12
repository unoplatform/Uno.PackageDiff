﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

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

			Assert.IsFalse(ReportAnalyzer.GenerateReport(default, default, res, context.IgnoreSet));
		}

		[TestMethod]
		public void When_IgnoreProperty()
		{
			var context = _builder.BuildAssemblies();

			var res = AssemblyComparer.CompareTypes(context.BaseAssembly, context.TargetAssembly);

			Assert.IsNotNull(context.IgnoreSet);
			Assert.IsTrue(ReportAnalyzer.GenerateReport(default, default, res, context.IgnoreSet));
		}

		[TestMethod]
		public void When_Ignore_All_Changes_To_Type()
		{
			var context = _builder.BuildAssemblies();

			var comparison = AssemblyComparer.CompareTypes(context.BaseAssembly, context.TargetAssembly);

			Assert.IsNotNull(context.IgnoreSet);
			Assert.IsFalse(ReportAnalyzer.GenerateReport(default, default, comparison, context.IgnoreSet));
		}

		[TestMethod]
		public void When_Ignore_All_Changes_With_Major_Minor_Only()
		{
			var context = _builder.BuildAssemblies();

			var comparison = AssemblyComparer.CompareTypes(context.BaseAssembly, context.TargetAssembly);

			Assert.IsNotNull(context.IgnoreSet);
			Assert.IsFalse(ReportAnalyzer.GenerateReport(default, default, comparison, context.IgnoreSet));
		}

		[TestMethod]
		public void When_Ignore_All_Changes_With_Major_Only()
		{
			var context = _builder.BuildAssemblies();

			var comparison = AssemblyComparer.CompareTypes(context.BaseAssembly, context.TargetAssembly);

			Assert.IsNotNull(context.IgnoreSet);
			Assert.IsFalse(ReportAnalyzer.GenerateReport(default, default, comparison, context.IgnoreSet));
		}
	}
}
