using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Threading.Tasks;

namespace Xamarin.Social
{
	/// <summary>
	/// The ViewController that the WebAuthenticator presents to the user.
	/// </summary>
	class WebAuthenticatorController : UIViewController
	{
		protected WebAuthenticator authenticator;

		UIWebView webView;
		UIActivityIndicatorView activity;

		public WebAuthenticatorController (WebAuthenticator authenticator)
		{
			this.authenticator = authenticator;

			authenticator.Succeeded += HandleSucceeded;
			authenticator.Failed += HandleFailed;
			authenticator.Cancelled += HandleCancelled;

			//
			// Create the UI
			//
			Title = authenticator.Service.Title;

			NavigationItem.LeftBarButtonItem = new UIBarButtonItem (
				UIBarButtonSystemItem.Cancel,
				delegate {
					authenticator.OnCancelled ();
				});

			activity = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.White);
			NavigationItem.RightBarButtonItem = new UIBarButtonItem (activity);

			webView = new UIWebView (View.Frame) {
				Delegate = new WebViewDelegate (this),
			};
			View = webView;

			//
			// Locate our initial URL
			//
			BeginLoadingInitialUrl ();
		}

		void BeginLoadingInitialUrl ()
		{
			authenticator.GetInitialUrlAsync ().ContinueWith (t => {
				if (t.IsFaulted) {
				}
				else {
					//
					// Delete cookies so we can work with multiple accounts
					//
					DeleteCookies (t.Result);
					
					//
					// Begin displaying the page
					//
					LoadInitialUrl (t.Result);
				}
			}, TaskScheduler.FromCurrentSynchronizationContext ());
		}

		void DeleteCookies (Uri url)
		{
			var cookiesUrl = url.Scheme + "://" + url.Host;
			var store = NSHttpCookieStorage.SharedStorage;
			var cookies = store.CookiesForUrl (new NSUrl (cookiesUrl));
			foreach (var c in cookies) {
				store.DeleteCookie (c);
			}
		}
		
		void LoadInitialUrl (Uri url)
		{
			if (url != null) {
				var request = new NSUrlRequest (new NSUrl (url.AbsoluteUri));
				NSUrlCache.SharedCache.RemoveCachedResponse (request); // Always try
				webView.LoadRequest (request);
			}
		}

		bool wantsDismissal = false;
		bool appeared = false;
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			if (wantsDismissal) {
				// Bounce this call so we don't confuse the animation system
				BeginInvokeOnMainThread (delegate {
					DismissModalViewControllerAnimated (animated);
				});
			}
			appeared = true;
		}

		void Dismiss ()
		{
			if (appeared) {
				DismissModalViewControllerAnimated (true);
			}
			else {
				wantsDismissal = true;
			}
		}

		void HandleSucceeded (object sender, EventArgs e)
		{
			BeginInvokeOnMainThread (() => {
				authenticator.Succeeded -= HandleSucceeded;
				Dismiss ();
			});
		}

		void HandleFailed (object sender, AuthenticationFailedEventArgs e)
		{
			BeginInvokeOnMainThread (() => {
				if (e.Exception != null) {
					this.ShowError ("Authentication Error", e.Exception, BeginLoadingInitialUrl);
				}
				else {
					this.ShowError ("Authentication Error", e.Message, BeginLoadingInitialUrl);
				}
			});
		}

		void HandleCancelled (object sender, EventArgs e)
		{
			BeginInvokeOnMainThread (() => {
				authenticator.Cancelled -= HandleCancelled;
				Dismiss ();
			});
		}

		protected class WebViewDelegate : UIWebViewDelegate
		{
			protected WebAuthenticatorController controller;
			public WebViewDelegate (WebAuthenticatorController controller)
			{
				this.controller = controller;
			}

			public override void LoadStarted (UIWebView webView)
			{
				controller.activity.StartAnimating ();
			}

			public override void LoadingFinished (UIWebView webView)
			{
				controller.activity.StopAnimating ();

				var url = new Uri (webView.Request.Url.AbsoluteString);
				controller.authenticator.OnPageLoaded (url);
			}
		}
	}
}

