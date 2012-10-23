using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xamarin.Auth;

namespace Xamarin.Social
{
	/// <summary>
	/// Implements OAuth 2.0 implicit granting. http://tools.ietf.org/html/draft-ietf-oauth-v2-31#section-4.2
	/// </summary>
	public abstract class OAuth2Service : Service
	{
		/// <summary>
		/// Client identifier. http://tools.ietf.org/html/draft-ietf-oauth-v2-31#section-2.2
		/// </summary>
		/// <value>
		/// The client identifier.
		/// </value>
		public string ClientId { get; set; }

		/// <summary>
		/// Access token scope. http://tools.ietf.org/html/draft-ietf-oauth-v2-31#section-3.3
		/// </summary>
		public string Scope { get; set; }
		
		public Uri AuthorizeUrl { get; protected set; }
		public Uri RedirectUrl { get; set; }
		
		public OAuth2Service (string serviceId, string title)
			: base (serviceId, title)
		{
			//
			// This is a reliable URL to redirect to
			//
			RedirectUrl = new Uri ("http://www.facebook.com/connect/login_success.html");
		}
		
		protected override Authenticator GetAuthenticator ()
		{
			if (string.IsNullOrEmpty (ClientId)) {
				throw new InvalidOperationException ("Client ID not specified.");
			}
			
			if (string.IsNullOrEmpty (Scope)) {
				throw new InvalidOperationException ("Scope not specified.");
			}
			
			if (AuthorizeUrl == null) {
				throw new InvalidOperationException ("Authorize URL not specified.");
			}
			
			if (RedirectUrl == null) {
				throw new InvalidOperationException ("Redirect URL not specified.");
			}
			
			return new OAuth2Authenticator (
				clientId: ClientId,
				scope: Scope,
				authorizeUrl: AuthorizeUrl,
				redirectUrl: RedirectUrl,
				getUsernameAsync: GetUsernameAsync);
		}

		protected abstract Task<string> GetUsernameAsync (string accessToken);

		public override Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters, Account account)
		{
			return new OAuth2Request (method, url, parameters, account) {
				Account = account,
			};
		}
	}
}

