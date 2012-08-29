using System;
using System.Collections.Generic;

namespace Xamarin.Social
{
	public class OAuth2Request : Request
	{
		public OAuth2Request (string method, Uri url, IDictionary<string, string> parameters, Account account)
			: base (method, url, parameters, account)
		{
		}
		
		protected override Uri GetPreparedUrl ()
		{
			var a = Account;
			if (a == null) {
				throw new InvalidOperationException ("OAuth2 requests require a valid Account to be set.");
			}
			if (!a.Properties.ContainsKey ("access_token")) {
				throw new InvalidOperationException ("OAuth2 account is missing required access_token property.");
			}
			
			var url = base.GetPreparedUrl ().AbsoluteUri;
			
			if (url.Contains ("?")) {
				url += "&access_token=" + a.Properties["access_token"];
			}
			else {
				url += "?access_token=" + a.Properties["access_token"];
			}
			
			return new Uri (url);
		}
	}
}

