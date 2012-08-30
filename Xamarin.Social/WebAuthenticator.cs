using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

#if PLATFORM_IOS
using AuthenticateUIType = MonoTouch.UIKit.UIViewController;
#else
using AuthenticateUIType = System.Object;
#endif

namespace Xamarin.Social
{
	/// <summary>
	/// An authenticator that displays a web page.
	/// </summary>
	public abstract class WebAuthenticator : Authenticator
	{
		public abstract Task<Uri> GetInitialUrlAsync ();

		public abstract void OnPageLoaded (Uri url);

		protected override AuthenticateUIType GetPlatformUI ()
		{
#if PLATFORM_IOS
			return new MonoTouch.UIKit.UINavigationController (new WebAuthenticatorController (this));
#else
			throw new System.NotImplementedException ("This platform does not support web authentication.");
#endif
		}
	}
}

