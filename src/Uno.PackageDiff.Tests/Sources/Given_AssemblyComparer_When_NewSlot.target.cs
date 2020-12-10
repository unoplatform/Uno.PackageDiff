using System;

namespace Uno.PackageDiff.Tests.Sources
{
	internal interface IPopup
	{
		event Action<object> Opened;
	}

	public class PopupLowerBase : IPopup
	{
		public event Action<object> Opened;

		public void Raise()
		{
			Opened?.Invoke(null);
		}

		public virtual void MyMethod()
		{

		}
	}

	public class PopupBase : PopupLowerBase
	{
		public new event Action<object> Opened
		{
			add => base.Opened += value;
			remove => base.Opened -= value;
		}

		public new void Raise() => base.Raise();

		public override void MyMethod()
		{

		}
	}

}
