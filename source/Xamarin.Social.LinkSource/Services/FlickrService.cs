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
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using System.Text;
using System.Collections.Generic;

using Xamarin.Auth;

namespace Xamarin.Social.Services
{
	public class FlickrService : OAuth1Service
	{
		public FlickrService ()
			: base ("Flickr", "Flickr")
		{
			CreateAccountLink = new Uri ("http://www.flickr.com");

			ShareTitle = "Upload";

			MaxImages = 1;
			MaxTextLength = int.MaxValue;
			MaxLinks = int.MaxValue;

			RequestTokenUrl = new Uri ("http://www.flickr.com/services/oauth/request_token");
			AuthorizeUrl = new Uri ("http://www.flickr.com/services/oauth/authorize");
			AccessTokenUrl = new Uri ("http://www.flickr.com/services/oauth/access_token");
		}

		public override Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters, Account account)
		{
			return new OAuth1Request (method, url, parameters, account, true);
		}

		public override Task ShareItemAsync (Item item, Account account, CancellationToken cancellationToken)
		{
			var req = CreateRequest (
				"POST",
				new Uri ("http://www.flickr.com/services/upload/"),
				account);

			//
			// Add the image
			//
			item.Images.First ().AddToRequest (req, "photo");

			//
			// Make the description include links
			//
			var sb = new StringBuilder ();
			sb.Append (item.Text);
			if (item.Links.Count > 0) {
				sb.AppendLine ();
				sb.AppendLine ();
				foreach (var l in item.Links) {
					sb.AppendFormat ("<a href=\"{0}\">{0}</a>", Social.Utilities.WebEx.HtmlEncode (l.AbsoluteUri));
					sb.AppendLine ();
				}
			}
			req.AddMultipartData ("description", sb.ToString ());

			return req.GetResponseAsync (cancellationToken).ContinueWith (reqTask => {

				var content = reqTask.Result.GetResponseText ();

				#if __PORTABLE__
				#else
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
				#endif

			}, cancellationToken);
		}
	}
}

