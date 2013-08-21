//
//  Copyright 2012-2013, Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Linq;
using Xamarin.Auth;

namespace Xamarin.Social.Services
{
	public class TwitterService : OAuth1Service
	{
		public TwitterService ()
			: base ("Twitter", "Twitter")
		{
			CreateAccountLink = new Uri ("https://twitter.com/signup");

			ShareTitle = "Tweet";

			MaxTextLength = 140;
			MaxLinks = int.MaxValue;
			MaxImages = 1;

			RequestTokenUrl = new Uri ("https://api.twitter.com/oauth/request_token");
			AuthorizeUrl = new Uri ("https://api.twitter.com/oauth/authorize");
			AccessTokenUrl = new Uri ("https://api.twitter.com/oauth/access_token");
		}

		public override int GetTextLength (Item item)
		{
			//
			// There are about 22 chars (eg https://t.co/UoGgfjFd) per attachment
			//
			return item.Text.Length + 22*(item.Links.Count + item.Images.Count + item.Files.Count);
		}

		public override Task ShareItemAsync (Item item, Account account, CancellationToken cancellationToken)
		{
			//
			// Combine the links into the tweet
			//
			var sb = new StringBuilder ();
			sb.Append (item.Text);
			foreach (var l in item.Links) {
				sb.Append (" ");
				sb.Append (l.AbsoluteUri);
			}
			var status = sb.ToString ();

			//
			// Create the request
			//
			Request req;
			if (item.Images.Count == 0) {
				req = CreateRequest ("POST", new Uri ("https://api.twitter.com/1.1/statuses/update.json"), account);
				req.Parameters["status"] = status;
			}
			else {
				req = CreateRequest ("POST", new Uri ("https://api.twitter.com/1.1/statuses/update_with_media.json"), account);
				req.AddMultipartData ("status", status);
				foreach (var i in item.Images.Take (MaxImages)) {
					i.AddToRequest (req, "media[]");
				}
			}

			//
			// Send it
			//
			return req.GetResponseAsync (cancellationToken);/*.ContinueWith ((Task<Response> reqTask) => {
				var content = reqTask.Result.GetResponseText ();
				if (!content.Contains ("<status")) {
					throw new SocialException ("Twitter did not return the expected response.");
				}
			});*/
		}

		
		public override bool SupportsVerification {
			get {
				return true;
			}
		}

		public override Task VerifyAsync (Account account)
		{
			return CreateRequest ("GET",
				new Uri ("https://api.twitter.com/1.1/account/verify_credentials.json"),
				account
			).GetResponseAsync ().ContinueWith (t => {
				if (t.Result.StatusCode != HttpStatusCode.OK)
					throw new SocialException ("Invalid Twitter credentials.");
			});
		}
	}
}

