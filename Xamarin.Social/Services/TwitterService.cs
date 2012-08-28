using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Xamarin.Social.Services
{
	public class TwitterService : OAuth1Service
	{
		public TwitterService ()
			: base ("Twitter", "Twitter")
		{
			CreateAccountLink = new Uri ("https://twitter.com/signup");

			ShareTitle = "Tweet";

			CanShareText = true;
			CanShareLinks = true;
			CanShareImages = true;

			RequestTokenUrl = new Uri ("https://api.twitter.com/oauth/request_token");
			AuthorizeUrl = new Uri ("https://api.twitter.com/oauth/authorize");
			AccessTokenUrl = new Uri ("https://api.twitter.com/oauth/access_token");
		}
	}
}

