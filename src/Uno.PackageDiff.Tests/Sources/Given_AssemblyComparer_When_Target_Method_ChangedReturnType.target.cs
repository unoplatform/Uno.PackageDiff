using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Uno.PackageDiff.Tests.Sources
{
	public class When_Target_Method_ChangedReturnType
	{
		public void VoidMethod() { }
		public int IntMethod() => 0;
		public async Task<bool> TestMethod() { return false; }
	}
}
