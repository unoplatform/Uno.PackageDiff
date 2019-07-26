using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Host;

namespace Uno.PackageDiff.Tests
{
	static class ModuleBuilder
	{
		public static Mono.Cecil.AssemblyDefinition BuildAssemblyDefinition(string sourceCode)
		{
			var st = SyntaxFactory.ParseCompilationUnit(sourceCode);

			var compilation = CSharpLanguage.Instance
			  .CreateLibraryCompilation(assemblyName: "InMemoryAssembly", enableOptimisations: false)
			  .AddSyntaxTrees(new[] { st.SyntaxTree });

			var stream = new MemoryStream();
			var emitResult = compilation.Emit(stream);

			if(!emitResult.Success)
			{
				var msg = string.Join("\n", emitResult.Diagnostics
					.Where(d => d.Severity == DiagnosticSeverity.Error)
					.Select(d => d.GetMessage()));
				throw new InvalidOperationException($"Unable to compile source code.  Errors:\n{msg}");
			}
			stream.Position = 0;

			return Mono.Cecil.AssemblyDefinition.ReadAssembly(stream);
		}

		public class CSharpLanguage : ILanguageService
		{
			private readonly IEnumerable<MetadataReference> _references;

			public static CSharpLanguage Instance { get; } = new CSharpLanguage();

			private readonly MetadataReference CorlibReference =
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

			private readonly MetadataReference ConsoleReference =
				MetadataReference.CreateFromFile(typeof(System.Console).Assembly.Location);

			private readonly MetadataReference DecimalReference =
				MetadataReference.CreateFromFile(typeof(System.Decimal).Assembly.Location);

			private readonly MetadataReference SystemCoreReference =
				MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);

			private readonly MetadataReference CodeAnalysisReference =
				MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);

			private CSharpLanguage()
			{
				var sdkFiles = this.GetType().Assembly.GetManifestResourceNames().Where(f => f.Contains("mono_sdk"));

				_references = new[] {
						CorlibReference,
						ConsoleReference,
						DecimalReference,
						SystemCoreReference,
						CodeAnalysisReference,
					};
			}

			public Compilation CreateLibraryCompilation(string assemblyName, bool enableOptimisations)
			{
				var options = new CSharpCompilationOptions(
					OutputKind.DynamicallyLinkedLibrary,
					optimizationLevel: OptimizationLevel.Release,
					allowUnsafe: true)
					// Disabling concurrent builds allows for the emit to finish.
					.WithConcurrentBuild(false)
					;

				return CSharpCompilation.Create(assemblyName, options: options, references: _references);
			}
		}

	}
}
