using System;
using System.Collections.Generic;
using Xamarin.Auth;

namespace Xamarin.Social
{
	/// <summary>
	/// A request that is authenticated using OAuth2.
	/// </summary>
	public class OAuth2Request : Request
	{
		public OAuth2Request (string method, Uri url, IDictionary<string, string> parameters, Account account)
			: base (method, url, parameters, account)
		{
		}
		
		protected override Uri GetPreparedUrl ()
		{
			return GetAuthenticatedUrl (Account, base.GetPreparedUrl ());
		}

		/// <summary>
		/// Transforms an unauthenticated URL to an authenticated one.
		/// </summary>
		/// <returns>
		/// The authenticated URL.
		/// </returns>
		/// <param name='account'>
		/// The <see cref="Account"/> that's been authenticated.
		/// </param>
		/// <param name='unauthenticatedUrl'>
		/// The unauthenticated URL.
		/// </param>
		public static Uri GetAuthenticatedUrl (Account account, Uri unauthenticatedUrl)
		{
			if (account == null) {
				throw new ArgumentNullException ("account");
			}
			if (!account.Properties.ContainsKey ("access_token")) {
				throw new ArgumentException ("OAuth2 account is missing required access_token property.", "account");
			}
			if (unauthenticatedUrl == null) {
				throw new ArgumentNullException ("unauthenticatedUrl");
			}
			
			var url = unauthenticatedUrl.AbsoluteUri;
			
			if (url.Contains ("?")) {
				url += "&access_token=" + account.Properties["access_token"];
			}
			else {
				url += "?access_token=" + account.Properties["access_token"];
			}
			
			return new Uri (url);
		}

		/// <summary>
		/// Gets an authenticated HTTP Authorization header.
		/// </summary>
		/// <returns>
		/// The authorization header.
		/// </returns>
		/// <param name='account'>
		/// The <see cref="Account"/> that's been authenticated.
		/// </param>
		public static string GetAuthorizationHeader (Account account)
		{
			if (account == null) {
				throw new ArgumentNullException ("account");
			}
			if (!account.Properties.ContainsKey ("access_token")) {
				throw new ArgumentException ("OAuth2 account is missing required access_token property.", "account");
			}
			
			return "Bearer " + account.Properties["access_token"];
		}
	}
}

