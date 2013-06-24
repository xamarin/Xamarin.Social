using System;
using Xamarin.Auth;

namespace Xamarin.Social
{
	public class OAuth1PreAService : OAuth1Service
	{
		public OAuth1PreAService (string serviceId, string title) : base (serviceId, title) { }

		protected override Authenticator GetAuthenticator ()
		{
			if (string.IsNullOrEmpty (ConsumerKey)) {
				throw new InvalidOperationException ("Consumer Key not specified.");
			}

			if (string.IsNullOrEmpty (ConsumerSecret)) {
				throw new InvalidOperationException ("Consumer Secret not specified.");
			}

			if (RequestTokenUrl == null) {
				throw new InvalidOperationException ("Request URL not specified.");
			}

			if (AuthorizeUrl == null) {
				throw new InvalidOperationException ("Authorize URL not specified.");
			}

			if (AccessTokenUrl == null) {
				throw new InvalidOperationException ("Access Token URL not specified.");
			}

			if (CallbackUrl == null) {
				throw new InvalidOperationException ("Callback URL not specified.");
			}

			return new OAuth1PreAAuthenticator (
				consumerKey: ConsumerKey,
				consumerSecret: ConsumerSecret,
				requestTokenUrl: RequestTokenUrl,
				authorizeUrl: AuthorizeUrl,
				accessTokenUrl: AccessTokenUrl,
				callbackUrl: CallbackUrl,
				getUsernameAsync: GetUsernameAsync);
		}
	}
}

