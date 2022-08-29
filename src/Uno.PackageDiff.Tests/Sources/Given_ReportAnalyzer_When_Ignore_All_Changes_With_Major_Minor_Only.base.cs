using System;
using System.Collections.Generic;
using System.Text;

namespace Uno.PackageDiff.Tests.Sources
{
	public class When_Ignore_All_Changes_To_Type
	{
		public int OldProperty1 { get; }

		public string OldProperty2 { get; protected set; }

		public void OldMethod1(float f) { }

		public double OldMethod2() => 42d;

		public event Action OldEvent;
	}
}
