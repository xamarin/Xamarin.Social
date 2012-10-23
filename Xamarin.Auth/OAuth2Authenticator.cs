using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Xamarin.Auth
{
	/// <summary>
	/// Implements OAuth 2.0 implicit granting. http://tools.ietf.org/html/draft-ietf-oauth-v2-31#section-4.2
	/// </summary>
	public class OAuth2Authenticator : WebAuthenticator
	{
		public delegate Task<string> GetUsernameAsyncFunc (string accessToken);

		public string ResponseType { get; set; }

		string clientId;
		string scope;
		Uri authorizeUrl;
		Uri redirectUrl;
		GetUsernameAsyncFunc getUsernameAsync;

		public OAuth2Authenticator (string clientId, string scope, Uri authorizeUrl, Uri redirectUrl, GetUsernameAsyncFunc getUsernameAsync)
		{
			ResponseType = "token";

			if (string.IsNullOrEmpty (clientId)) {
				throw new ArgumentException ("clientId must be provided", "clientId");
			}
			this.clientId = clientId;

			this.scope = scope ?? "";

			if (authorizeUrl == null) {
				throw new ArgumentNullException ("authorizeUrl");
			}
			this.authorizeUrl = authorizeUrl;

			if (redirectUrl == null) {
				throw new ArgumentNullException ("redirectUrl");
			}
			this.redirectUrl = redirectUrl;

			if (getUsernameAsync == null) {
				throw new ArgumentNullException ("getUsernameAsync");
			}
			this.getUsernameAsync = getUsernameAsync;
		}
		
		public override Task<Uri> GetInitialUrlAsync ()
		{
			var url = new Uri (string.Format (
				"{0}?client_id={1}&redirect_uri={2}&response_type={3}&scope={4}",
				authorizeUrl.AbsoluteUri,
				Uri.EscapeDataString (clientId),
				Uri.EscapeDataString (redirectUrl.AbsoluteUri),
				Uri.EscapeDataString (ResponseType),
				Uri.EscapeDataString (scope)));

			return Task.Factory.StartNew (() => {
				return url;
			});
		}
		
		public override void OnPageLoaded (Uri url)
		{
			if (url.Host == redirectUrl.Host && url.LocalPath == redirectUrl.LocalPath) {
				//
				// Look for the access_token
				//
				var part = url
					.Fragment
						.Split ('#', '?', '&')
						.FirstOrDefault (p => p.StartsWith ("access_token="));
				if (part != null) {
					var accessToken = part.Substring ("access_token=".Length);
					
					//
					// Now we just need a username for the account
					//
					getUsernameAsync (accessToken).ContinueWith (task => {
						if (task.IsFaulted) {
							OnError (task.Exception);
						}
						else {
							OnSucceeded (task.Result, new Dictionary<string,string> {
								{ "access_token", accessToken },
							});
						}
					}, TaskScheduler.FromCurrentSynchronizationContext ());
				}
				else if (ResponseType == "code") {
					throw new NotSupportedException ("ResponseType=code is not supported.");
				}
				else {
					throw new AuthException ("Expected the RedirectUrl to contain an access_token. It did not.");
				}
			}
		}
	}
}


