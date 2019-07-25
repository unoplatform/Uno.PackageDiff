namespace Uno.PackageDiff.Tests.Sources
{
	public class When_DerivingClasses : When_DerivingClasses_Base
	{
		public override void VirtualMethod()
		{
			base.VirtualMethod();
		}
	}

	public class When_DerivingClasses_Base
	{
		public virtual void VirtualMethod()
		{

		}
	}
}
