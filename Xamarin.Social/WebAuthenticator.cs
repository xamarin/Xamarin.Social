using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

#if PLATFORM_IOS
using AuthenticateUIType = MonoTouch.UIKit.UIViewController;
#elif PLATFORM_ANDROID
using AuthenticateUIType = Android.Content.Intent;
using UIContext = Android.App.Activity;
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

#if PLATFORM_IOS
		protected override AuthenticateUIType GetPlatformUI ()
		{
			return new MonoTouch.UIKit.UINavigationController (new WebAuthenticatorController (this));
		}
#endif

#if PLATFORM_ANDROID
		protected override AuthenticateUIType GetPlatformUI (UIContext context)
		{
			var i = new global::Android.Content.Intent (context, typeof (WebAuthenticatorActivity));
			var state = new WebAuthenticatorActivity.State {
				Authenticator = this,
			};
			i.PutExtra ("StateKey", WebAuthenticatorActivity.StateRepo.Add (state));
			return i;
		}
#endif
	}
}

