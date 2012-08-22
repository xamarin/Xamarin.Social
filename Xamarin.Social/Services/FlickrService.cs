using System;

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

			return new OAuth1Authenticator (Key, Secret);
		}
	}
}

