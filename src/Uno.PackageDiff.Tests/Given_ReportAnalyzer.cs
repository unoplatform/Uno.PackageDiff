using System;
using System.Collections.Generic;
using System.IO;
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

			Assert.IsFalse(ReportAnalyzer.GenerateReport(StreamWriter.Null, res, context.IgnoreSet));
		}

		[TestMethod]
		public void When_IgnoreProperty()
		{
			var context = _builder.BuildAssemblies();

			var res = AssemblyComparer.CompareTypes(context.BaseAssembly, context.TargetAssembly);

			Assert.IsNotNull(context.IgnoreSet);
			Assert.IsTrue(ReportAnalyzer.GenerateReport(StreamWriter.Null, res, context.IgnoreSet));
		}

		[TestMethod]
		public void When_Ignore_All_Changes_To_Type()
		{
			var context = _builder.BuildAssemblies();

			var comparison = AssemblyComparer.CompareTypes(context.BaseAssembly, context.TargetAssembly);

			Assert.IsNotNull(context.IgnoreSet);
			Assert.IsFalse(ReportAnalyzer.GenerateReport(StreamWriter.Null, comparison, context.IgnoreSet));
		}

		[TestMethod]
		public void When_Ignore_All_Changes_With_Major_Minor_Only()
		{
			var context = _builder.BuildAssemblies();

			var comparison = AssemblyComparer.CompareTypes(context.BaseAssembly, context.TargetAssembly);

			Assert.IsNotNull(context.IgnoreSet);
			Assert.IsFalse(ReportAnalyzer.GenerateReport(StreamWriter.Null, comparison, context.IgnoreSet));
		}

		[TestMethod]
		public void When_Ignore_All_Changes_With_Major_Only()
		{
			var context = _builder.BuildAssemblies();

			var comparison = AssemblyComparer.CompareTypes(context.BaseAssembly, context.TargetAssembly);

			Assert.IsNotNull(context.IgnoreSet);
			Assert.IsFalse(ReportAnalyzer.GenerateReport(StreamWriter.Null, comparison, context.IgnoreSet));
		}

		[TestMethod]
		public void When_Ignore_With_Regex()
		{
			var context = _builder.BuildAssemblies();

			var res = AssemblyComparer.CompareTypes(context.BaseAssembly, context.TargetAssembly);

			Assert.IsNotNull(context.IgnoreSet);
			using var writer = new StringWriter();
			Assert.IsTrue(ReportAnalyzer.GenerateReport(writer, res, context.IgnoreSet));
			var result = writer.ToString();
			Assert.AreEqual("""
				### 0 missing types:
				### 8 missing or changed method in existing types:
				- `Uno.PackageDiff.Tests.Sources.Given_ReportAnalyzer_When_Ignore_With_Regex`
					* ``System.Int32 Uno.PackageDiff.Tests.Sources.Given_ReportAnalyzer_When_Ignore_With_Regex.get_P1_Ignored()``
					* ``System.Void Uno.PackageDiff.Tests.Sources.Given_ReportAnalyzer_When_Ignore_With_Regex.set_P1_Ignored(System.Int32 value)``
					* ``System.Int32 Uno.PackageDiff.Tests.Sources.Given_ReportAnalyzer_When_Ignore_With_Regex.get_P2_Ignored()``
					* ``System.Void Uno.PackageDiff.Tests.Sources.Given_ReportAnalyzer_When_Ignore_With_Regex.set_P2_Ignored(System.Int32 value)``
					* ``System.Int32 Uno.PackageDiff.Tests.Sources.Given_ReportAnalyzer_When_Ignore_With_Regex.get_P3_Ignored()``
					* ``System.Void Uno.PackageDiff.Tests.Sources.Given_ReportAnalyzer_When_Ignore_With_Regex.set_P3_Ignored(System.Int32 value)``
					* ``System.Int32 Uno.PackageDiff.Tests.Sources.Given_ReportAnalyzer_When_Ignore_With_Regex.get_P_NotIgnored()``
					* ``System.Void Uno.PackageDiff.Tests.Sources.Given_ReportAnalyzer_When_Ignore_With_Regex.set_P_NotIgnored(System.Int32 value)``
				### 0 missing or changed events in existing types:
				### 0 missing or changed fields in existing types:
				### 4 missing or changed properties in existing types:
				- `Uno.PackageDiff.Tests.Sources.Given_ReportAnalyzer_When_Ignore_With_Regex`
					* ~~``System.Int32 Uno.PackageDiff.Tests.Sources.Given_ReportAnalyzer_When_Ignore_With_Regex::P1_Ignored()``~~
					* ~~``System.Int32 Uno.PackageDiff.Tests.Sources.Given_ReportAnalyzer_When_Ignore_With_Regex::P2_Ignored()``~~
					* ~~``System.Int32 Uno.PackageDiff.Tests.Sources.Given_ReportAnalyzer_When_Ignore_With_Regex::P3_Ignored()``~~
					* ``System.Int32 Uno.PackageDiff.Tests.Sources.Given_ReportAnalyzer_When_Ignore_With_Regex::P_NotIgnored()``

				""", result);
		}
	}
}
