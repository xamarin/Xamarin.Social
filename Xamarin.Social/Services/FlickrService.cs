using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using System.Text;

namespace Xamarin.Social
{
	public class FlickrService : Service
	{
		public string Key { get; set; }
		public string Secret { get; set; }

		public FlickrService ()
			: base ("Flickr", "Flickr")
		{
			CreateAccountLink = new Uri ("http://www.flickr.com");
			CanShareImages = true;
			CanShareText = true;
			CanShareLinks = true;
		}

		protected override Authenticator GetAuthenticator ()
		{
			if (string.IsNullOrEmpty (Key)) {
				throw new InvalidOperationException ("You need to specify your app Key. " +
					"Get it at http://www.flickr.com/services/apps/by/me");
			}

			if (string.IsNullOrEmpty (Secret)) {
				throw new InvalidOperationException ("You need to specify your app Secret. " +
					"Get it at http://www.flickr.com/services/apps/by/me");
			}

			return new OAuth1Authenticator (
				consumerKey: Key,
				consumerSecret: Secret,
				requestTokenUrl: new Uri ("http://www.flickr.com/services/oauth/request_token"),
				authorizeUrl: new Uri ("http://www.flickr.com/services/oauth/authorize"),
				accessTokenUrl: new Uri ("http://www.flickr.com/services/oauth/access_token"));
		}

		class OAuth1Request : Request
		{
			public OAuth1Request (string method, Uri url, System.Collections.Generic.IDictionary<string, string> parameters, Account account)
				: base (method, url, parameters, account)
			{
			}

			public override Task<Response> GetResponseAsync (CancellationToken cancellationToken)
			{
				var req = GetPreparedWebRequest ();

				var authorization = OAuth1.GetAuthorizationHeader (
					Method, 
					Url, 
					Parameters, 
					Account.Properties["oauth_consumer_key"],
					Account.Properties["oauth_consumer_secret"],
					Account.Properties["oauth_token"],
					Account.Properties["oauth_token_secret"]);

				req.Headers[System.Net.HttpRequestHeader.Authorization] = authorization;

				return base.GetResponseAsync (cancellationToken);
			}
		}

		public override Request CreateRequest (string method, Uri url, System.Collections.Generic.IDictionary<string, string> parameters, Account account)
		{
			return new OAuth1Request (method, url, parameters, account);
		}

		protected override Task ShareItemAsync (Item item, Account account, CancellationToken cancellationToken)
		{
			var req = CreateRequest (
				"POST",
				new Uri ("http://www.flickr.com/services/upload/"),
				account);

			req.AddMultipartData ("photo", item.Images.First ());

			return req.GetResponseAsync (cancellationToken).ContinueWith (reqTask => {

				var content = reqTask.Result.GetResponseText ();

				var doc = new XmlDocument ();
				doc.LoadXml (content);
				var stat = doc.DocumentElement.GetAttribute ("stat");

				if (stat == "fail") {
					var err = doc.DocumentElement.GetElementsByTagName ("err").OfType<XmlElement> ().FirstOrDefault ();
					if (err != null) {
						throw new ApplicationException (err.GetAttribute ("msg"));
					}
					else {
						throw new ApplicationException ("Flickr returned an unknown error.");
					}
				}

			}, cancellationToken);
		}
	}
}

