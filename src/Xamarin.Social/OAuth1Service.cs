using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Auth;

namespace Xamarin.Social
{
	public abstract class OAuth1Service : Service
	{
		public string ConsumerKey { get; set; }
		public string ConsumerSecret { get; set; }

		public Uri RequestTokenUrl { get; protected set; }
		public Uri AuthorizeUrl { get; protected set; }
		public Uri AccessTokenUrl { get; protected set; }
		public Uri CallbackUrl { get; set; }

		public OAuth1Service (string serviceId, string title)
			: base (serviceId, title)
		{
			//
			// This is a reliable URL to redirect to
			//
			CallbackUrl = new Uri ("http://www.facebook.com/connect/login_success.html");
		}

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
			
			return new OAuth1Authenticator (
				consumerKey: ConsumerKey,
				consumerSecret: ConsumerSecret,
				requestTokenUrl: RequestTokenUrl,
				authorizeUrl: AuthorizeUrl,
				accessTokenUrl: AccessTokenUrl,
				callbackUrl: CallbackUrl,
				getUsernameAsync: GetUsernameAsync);
		}
		
		public override Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters, Account account)
		{
			return new OAuth1Request (method, url, parameters, account);
		}

		static readonly string[] UsernameKeys = new string[] {
			"username",
			"user_name",
			"screenname",
			"screen_name",
			"email",
			"email_address",
		};
		
		protected virtual Task<string> GetUsernameAsync (IDictionary<string, string> accountProperties)
		{
			return Task.Factory.StartNew (delegate {
				foreach (var k in UsernameKeys) {
					if (accountProperties.ContainsKey (k)) {
						return accountProperties[k];
					}
				}			
				throw new SocialException ("Could not determine username.");
			});
		}
	}
}

