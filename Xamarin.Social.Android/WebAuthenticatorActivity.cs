using System;
using Android.App;
using Android.Webkit;
using Android.OS;

namespace Xamarin.Social
{
	[Activity (Label = "Web Authenticator")]
	public class WebAuthenticatorActivity : Activity
	{
		WebView webView;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			webView = new WebView (this);
			webView.SetWebViewClient (new Client ());

			SetContentView (webView);

			webView.LoadUrl ("http://google.com/");
		}

		class Client : WebViewClient
		{
			public override bool ShouldOverrideUrlLoading (WebView view, string url)
			{
				return false;
			} 
		}
	}
}

