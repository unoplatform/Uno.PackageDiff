using Mono.Cecil;

namespace Uno.PackageDiff.Tests
{
	public class ComparisonContext
	{
		public AssemblyDefinition BaseAssembly { get; }
		public AssemblyDefinition TargetAssembly { get; }
		public IgnoreSet IgnoreSet { get; }

		public ComparisonContext(AssemblyDefinition assemblyDefinition1, AssemblyDefinition assemblyDefinition2, IgnoreSet ignoreSet)
		{
			BaseAssembly = assemblyDefinition1;
			TargetAssembly = assemblyDefinition2;
			IgnoreSet = ignoreSet;
		}
	}
}
