using System;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;

namespace Xamarin.Social
{
	public class OAuth1Authenticator : WebAuthenticator
	{
		string consumerKey;
		string consumerSecret;

		Uri requestTokenUrl;
		Uri authorizeUrl;
		Uri accessTokenUrl;
		Uri callbackUrl;

		string token;
		string tokenSecret;

		string verifier;

		public OAuth1Authenticator (string consumerKey, string consumerSecret, Uri requestTokenUrl, Uri authorizeUrl, Uri accessTokenUrl, Uri callbackUrl)
		{
			this.consumerKey = consumerKey;
			this.consumerSecret = consumerSecret;
			this.requestTokenUrl = requestTokenUrl;
			this.authorizeUrl = authorizeUrl;
			this.accessTokenUrl = accessTokenUrl;
			this.callbackUrl = callbackUrl;
		}

		public override Task<Uri> GetInitialUrlAsync () {
			var req = OAuth1.CreateRequest (
				"GET",
				requestTokenUrl, 
				new Dictionary<string, string>() {
					{ "oauth_callback", callbackUrl.AbsoluteUri },
				},
				consumerKey,
				consumerSecret,
				"");

			return req.GetResponseAsync ().ContinueWith (respTask => {

				var content = respTask.Result.GetResponseText ();

				var r = HttpEx.FormDecode (content);

				token = r["oauth_token"];
				tokenSecret = r["oauth_token_secret"];

				var url = authorizeUrl.AbsoluteUri + "?oauth_token=" + Uri.EscapeDataString (token);

				return new Uri (url);
			});
		}

		public override void OnPageLoaded (Uri url)
		{
			if (url.Host == callbackUrl.Host && url.AbsolutePath == callbackUrl.AbsolutePath) {
				
				var query = url.Query;
				var r = HttpEx.FormDecode (query);
				
				verifier = r["oauth_verifier"];
				
				GetAccessTokenAsync ();
			}
		}

		Task GetAccessTokenAsync ()
		{
			var req = OAuth1.CreateRequest (
				"GET",
				accessTokenUrl,
				new Dictionary<string, string> {
					{ "oauth_verifier", verifier },
					{ "oauth_token", token },
				},
				consumerKey,
				consumerSecret,
				tokenSecret);
			
			return req.GetResponseAsync ().ContinueWith (respTask => {				
				var content = respTask.Result.GetResponseText ();

				var accountProperties = HttpEx.FormDecode (content);
				accountProperties["oauth_consumer_key"] = consumerKey;
				accountProperties["oauth_consumer_secret"] = consumerSecret;

				OnSuccess (accountProperties["username"], accountProperties);				
			});
		}
	}
}

