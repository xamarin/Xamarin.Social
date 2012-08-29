using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Xamarin.Social
{
	/// <summary>
	/// Implements OAuth 2.0 implicit granting. http://tools.ietf.org/html/draft-ietf-oauth-v2-31#section-4.2
	/// </summary>
	public class OAuth2Authenticator : WebAuthenticator
	{
		public delegate Task<string> GetUsernameAsyncFunc (string accessToken);

		string clientId;
		string scope;
		Uri authorizeUrl;
		Uri redirectUrl;
		GetUsernameAsyncFunc getUsernameAsync;

		public OAuth2Authenticator (string clientId, string scope, Uri authorizeUrl, Uri redirectUrl, GetUsernameAsyncFunc getUsernameAsync)
		{
			this.clientId = clientId;
			this.scope = scope;
			this.authorizeUrl = authorizeUrl;
			this.redirectUrl = redirectUrl;

			if (getUsernameAsync == null) {
				throw new ArgumentNullException ("getUsernameAsync");
			}
			this.getUsernameAsync = getUsernameAsync;
		}
		
		public override Task<Uri> GetInitialUrlAsync ()
		{
			var url = new Uri (string.Format (
				"{0}?client_id={1}&redirect_uri={2}&response_type=token&scope={3}",
				authorizeUrl.AbsoluteUri,
				Uri.EscapeDataString (clientId),
				Uri.EscapeDataString (redirectUrl.AbsoluteUri),
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
							OnFailed (task.Exception);
						}
						else {
							OnSucceeded (task.Result, new Dictionary<string,string> {
								{ "access_token", accessToken },
							});
						}
					}, TaskScheduler.FromCurrentSynchronizationContext ());
				}
			}
		}
	}
}


