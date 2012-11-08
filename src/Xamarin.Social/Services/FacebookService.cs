using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Threading;
using Xamarin.Auth;

namespace Xamarin.Social.Services
{
	public class FacebookService : OAuth2Service
	{
		public FacebookService ()
			: base ("Facebook", "Facebook")
		{
			CreateAccountLink = new Uri ("https://www.facebook.com");

			MaxTextLength = int.MaxValue;
			MaxImages = 1;
			MaxLinks = 1;

			AuthorizeUrl = new Uri ("https://m.facebook.com/dialog/oauth/");
			RedirectUrl = new Uri ("http://www.facebook.com/connect/login_success.html");

			Scope = "publish_stream";
		}

		protected override Task<string> GetUsernameAsync (IDictionary<string, string> accountProperties)
		{
			var request = CreateRequest (
				"GET",
				new Uri ("https://graph.facebook.com/me"),
				new Account ("", accountProperties));

			return request.GetResponseAsync ().ContinueWith (reqTask => {
				var json = reqTask.Result.GetResponseText ();
				var username = GetValueFromJson (json, "username");
				if (string.IsNullOrEmpty (username)) {
					throw new Exception ("Could not read username from the /me API call");
				}
				else {
					return username;
				}
			});
		}
		
		static string GetValueFromJson (string json, string key)
		{
			var p = json.IndexOf ("\"" + key + "\"");
			if (p < 0) return "";
			var c = json.IndexOf (":", p);
			if (c < 0) return "";
			var q = json.IndexOf ("\"", c);
			if (q < 0) return "";
			var b = q + 1;
			var e = b;
			for (; e < json.Length && json[e] != '\"'; e++) {
			}
			var r = json.Substring (b, e - b);
			return r;
		}

		public override Task ShareItemAsync (Item item, Account account, CancellationToken cancellationToken)
		{
			Request req;

			if (item.Images.Count > 0) {
				req = CreateRequest ("POST", new Uri ("https://graph.facebook.com/me/photos"), account);
				req.AddMultipartData ("source", item.Images.First ());

				var message = new StringBuilder ();
				message.Append (item.Text);
				foreach (var l in item.Links) {
					message.AppendLine ();
					message.Append (l.AbsoluteUri);
				}
				req.AddMultipartData ("message", message.ToString ());
			}
			else {
				req = CreateRequest ("POST", new Uri ("https://graph.facebook.com/me/feed"), account);
				req.Parameters["message"] = item.Text;
				if (item.Links.Count > 0) {
					req.Parameters["link"] = item.Links.First ().AbsoluteUri;
				}
			}

			return req.GetResponseAsync (cancellationToken).ContinueWith (reqTask => {
				var content = reqTask.Result.GetResponseText ();
				if (!content.Contains ("\"id\"")) {
					throw new SocialException ("Facebook returned an unrecognized response.");
				}
			});
		}
	}
}

