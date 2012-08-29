using System;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Collections.Generic;

namespace Xamarin.Social
{
	public class OAuth1Request : Request
	{
		bool includeMultipartsInSignature;

		public OAuth1Request (string method, Uri url, IDictionary<string, string> parameters, Account account, bool includeMultipartsInSignature = false)
			: base (method, url, parameters, account)
		{
			this.includeMultipartsInSignature = includeMultipartsInSignature;
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
			// mulitpart keys if we're dealing with a buggy
			// OAuth implementation (I'm looking at you Flickr).
			//
			// These normally shouldn't be included: http://tools.ietf.org/html/rfc5849#section-3.4.1.3.1
			//
			var ps = new Dictionary<string, string> (Parameters);
			if (includeMultipartsInSignature) {
				foreach (var p in Multiparts) {
					if (!string.IsNullOrEmpty (p.TextData)) {
						ps[p.Name] = p.TextData;
					}
				}
			}

			//
			// Authorize it
			//
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

