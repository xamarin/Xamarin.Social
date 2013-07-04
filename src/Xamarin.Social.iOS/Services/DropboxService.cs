using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Utilities;

namespace Xamarin.Social.Services
{
	public class DropboxService : OAuth1PreAService
	{
		public string Root { get; set; }

		public DropboxService ()
			: base ("Dropbox", "Dropbox")
		{
			CreateAccountLink = new Uri ("http://www.flickr.com");

			ShareTitle = "Upload";

			Root = "sandbox";

			RequestTokenUrl = new Uri ("https://api.dropbox.com/1/oauth/request_token");
			AuthorizeUrl = new Uri ("https://www.dropbox.com/1/oauth/authorize");
			AccessTokenUrl = new Uri ("https://api.dropbox.com/1/oauth/access_token");
		}

		protected override WebAuthenticator GetEmbeddedAuthenticator ()
		{
			return new DropboxAuthenticator (
				embedded: true,
				consumerKey: ConsumerKey,
				consumerSecret: ConsumerSecret,
				requestTokenUrl: RequestTokenUrl,
				authorizeUrl: AuthorizeUrl,
				accessTokenUrl: AccessTokenUrl,
				callbackUrl: CallbackUrl,
				getUsernameAsync: GetUsernameAsync);
		}

		protected override Authenticator GetAuthenticator ()
		{
			return new DropboxAuthenticator (
				embedded: false,
				consumerKey: ConsumerKey,
				consumerSecret: ConsumerSecret,
				requestTokenUrl: RequestTokenUrl,
				authorizeUrl: AuthorizeUrl,
				accessTokenUrl: AccessTokenUrl,
				callbackUrl: CallbackUrl,
				getUsernameAsync: GetUsernameAsync);
		}

		protected override Task<string> GetUsernameAsync (IDictionary<string, string> accountProperties)
		{
			var request = base.CreateRequest ("GET",
			                                  new Uri ("https://api.dropbox.com/1/account/info"),
			new Account (string.Empty, accountProperties));

			return request.GetResponseAsync ().ContinueWith (reqTask => {
				var responseText = reqTask.Result.GetResponseText ();
				return WebEx.GetValueFromJson (responseText, "display_name");
			});
		}

		public override Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters, Account account)
		{
			return new OAuth1Request (method, url, parameters, account, true);
		}

		class DropboxAuthenticator : OAuth1PreAAuthenticator
		{
			public bool Embedded { get; private set; }
			public DropboxAuthenticator (bool embedded, string consumerKey, string consumerSecret, Uri requestTokenUrl, Uri authorizeUrl, Uri accessTokenUrl, Uri callbackUrl, GetUsernameAsyncFunc getUsernameAsync = null)
				: base (consumerKey, consumerSecret, requestTokenUrl, authorizeUrl, accessTokenUrl, callbackUrl, getUsernameAsync) {
				Embedded = embedded;
				this.authorizeUrl = embedded ? new Uri (authorizeUrl, "?embedded=1") : authorizeUrl;
			}

			public override bool OnPageLoading (Uri url)
			{
				if (Embedded && url.Host == callbackUrl.Host && url.AbsolutePath == callbackUrl.AbsolutePath) {
					GetAccessTokenAsync ().ContinueWith (getTokenTask => {
						if (getTokenTask.IsCanceled) {
							OnCancelled ();
						} else if (getTokenTask.IsFaulted) {
							OnError (getTokenTask.Exception);
						}
					}, TaskContinuationOptions.NotOnRanToCompletion);

					return false;
				}

				return base.OnPageLoading (url);
			}
		}
	}
}