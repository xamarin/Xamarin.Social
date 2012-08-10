using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

#if PLATFORM_IOS
using UIContext = MonoTouch.UIKit.UIViewController;
#else
using UIContext = System.Object;
#endif

namespace Xamarin.Social
{
	/// <summary>
	/// An authenticator that displays a web page.
	/// </summary>
	public abstract class WebAuthenticator : Authenticator
	{
		public abstract Uri InitialUrl { get; }

		public abstract void OnPageLoaded (Uri url);

		protected override void PresentUI (UIContext context)
		{
#if PLATFORM_IOS
			context.PresentModalViewController (new MonoTouch.UIKit.UINavigationController (
				new WebAuthenticatorController (this)), true);
#else
			throw new System.NotImplementedException ("This platform does not support web authentication.");
#endif
		}
	}
}

