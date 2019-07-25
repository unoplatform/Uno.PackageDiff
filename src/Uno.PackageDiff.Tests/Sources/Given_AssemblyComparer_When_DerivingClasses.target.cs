namespace Uno.PackageDiff.Tests.Sources
{
	public class When_DerivingClasses : When_DerivingClasses_Base
	{
		// "VirtualMethod1" override removed here: not a breaking change
	}

	public class When_DerivingClasses_Base
	{
		public virtual void VirtualMethod1()
		{

		}
		// "VirtualMethod2" virtual removed here: this one is breaking

		public void VirtualMethod3() // no more virtual
		{
		}
	}
}
