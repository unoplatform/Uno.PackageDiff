using System;

namespace Uno.PackageDiff.Tests.Sources
{
	internal interface IPopup
	{
		event Action<object> Opened;
	}

	public class PopupBase : IPopup
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
}
