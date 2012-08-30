using System;
using Android.App;
using Android.Webkit;
using Android.OS;
using System.Threading.Tasks;

namespace Xamarin.Social
{
	[Activity (Label = "Web Authenticator")]
	public class WebAuthenticatorActivity : Activity
	{
		WebView webView;

		internal class State : Java.Lang.Object
		{
			public WebAuthenticator Authenticator;
		}
		internal static readonly ActivityStateRepository<State> StateRepo = new ActivityStateRepository<State> ();

		State state;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			//
			// Load the state either from a configuration change or from the intent.
			//
			state = LastNonConfigurationInstance as State;
			if (state == null && Intent.HasExtra ("StateKey")) {
				var stateKey = Intent.GetStringExtra ("StateKey");
				state = StateRepo.Remove (stateKey);
			}
			if (state == null) {
				Finish ();
			}

			//
			// Watch for completion
			//
			state.Authenticator.Succeeded += delegate {
				SetResult (Result.Ok);
				Finish ();
			};
			state.Authenticator.Cancelled += delegate {
				SetResult (Result.Canceled);
				Finish ();
			};
			state.Authenticator.Error += (s, e) => {
				if (e.Exception != null) {
					this.ShowError ("Authentication Error", e.Exception);
				}
				else {
					this.ShowError ("Authentication Error", e.Message);
				}
				BeginLoadingInitialUrl ();
			};

			//
			// Build the UI
			//
			webView = new WebView (this) {
				Id = 42,
			};
			webView.SetWebViewClient (new Client (this));
			SetContentView (webView);

			//
			// Restore the UI state or start over
			//
			if (savedInstanceState != null) {
				webView.RestoreState (savedInstanceState);
			}
			else {
				Android.Webkit.CookieManager.Instance.RemoveAllCookie ();
				BeginLoadingInitialUrl ();
			}
		}

		void BeginLoadingInitialUrl ()
		{
			state.Authenticator.GetInitialUrlAsync ().ContinueWith (t => {
				if (t.IsFaulted) {
					this.ShowError ("Authentication Error", t.Exception);
				}
				else {
					webView.LoadUrl (t.Result.AbsoluteUri);
				}
			}, TaskScheduler.FromCurrentSynchronizationContext ());
		}

		public override Java.Lang.Object OnRetainNonConfigurationInstance ()
		{
			return state;
		}

		protected override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);
			webView.SaveState (outState);
		}

		class Client : WebViewClient
		{
			WebAuthenticatorActivity activity;

			public Client (WebAuthenticatorActivity activity)
			{
				this.activity = activity;
			}

			public override bool ShouldOverrideUrlLoading (WebView view, string url)
			{
				return false;
			}

			public override void OnPageFinished (WebView view, string url)
			{
				activity.state.Authenticator.OnPageLoaded (new Uri (url));
			}
		}
	}
}

