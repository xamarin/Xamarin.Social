using System;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Collections.Generic;

namespace Xamarin.Social
{
	public class OAuth1Request : Request
	{
		public OAuth1Request (string method, Uri url, IDictionary<string, string> parameters, Account account)
			: base (method, url, parameters, account)
		{
		}
		
		public override Task<Response> GetResponseAsync (CancellationToken cancellationToken)
		{
			//
			// Make sure we have an account
			//
			if (Account == null) {
				throw new InvalidOperationException ("You must specify an Account for this request to proceed");
			}

			//
			// Sign the request before getting the response
			//
			var req = GetPreparedWebRequest ();

			//
			// Make sure that the parameters array contains
			// mulitpart keys also
			//
			var ps = new Dictionary<string, string> (Parameters);
			foreach (var p in Multiparts) {
				if (!string.IsNullOrEmpty (p.TextData)) {
					ps[p.Name] = p.TextData;
				}
			}
			
			var authorization = OAuth1.GetAuthorizationHeader (
				Method,
				Url,
				ps,
				Account.Properties["oauth_consumer_key"],
				Account.Properties["oauth_consumer_secret"],
				Account.Properties["oauth_token"],
				Account.Properties["oauth_token_secret"]);
			
			req.Headers[HttpRequestHeader.Authorization] = authorization;
			
			return base.GetResponseAsync (cancellationToken);
		}
	}
}

