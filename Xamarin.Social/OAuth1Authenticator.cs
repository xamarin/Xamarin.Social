using System;

namespace Xamarin.Social
{
	public class OAuth1Authenticator : WebAuthenticator
	{
		public OAuth1Authenticator (string key, string secret)
		{
		}

		#region implemented abstract members of WebAuthenticator

		public override void OnPageLoaded (Uri url)
		{
		}

		public override Uri InitialUrl {
			get {
				return new Uri ("http://www.flickr.com/services/oauth/authorize");
			}
		}

		#endregion
	}
}

