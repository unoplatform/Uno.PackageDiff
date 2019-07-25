namespace Uno.PackageDiff.Tests.Sources
{
	public class When_DerivingClasses : When_DerivingClasses_Base
	{
		public override void VirtualMethod1()
		{
			base.VirtualMethod1();
		}
	}

	public class When_DerivingClasses_Base
	{
		public virtual void VirtualMethod1()
		{
		}

		public virtual void VirtualMethod2()
		{
		}

		public virtual void VirtualMethod3()
		{
		}
	}
}
