using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Threading;
using Xamarin.Auth;

namespace Xamarin.Social.Services
{
	public class AppDotNetService : OAuth2Service
	{
		public AppDotNetService ()
			: base ("app.net", "app.net")
		{
			CreateAccountLink = new Uri ("https://join.app.net");

			MaxTextLength = 256;
			MaxImages = 0;
			MaxLinks = int.MaxValue;

			AuthorizeUrl = new Uri ("https://alpha.app.net/oauth/authenticate");
			RedirectUrl = null;

			Scope = "basic,stream,write_post";
		}

		protected override Task<string> GetUsernameAsync (string accessToken)
		{
			var request = CreateRequest (
				"GET",
				new Uri ("https://alpha-api.app.net/stream/0/users/me"),
				new Account ("?", new Dictionary<string,string> {
					{ "access_token", accessToken },
				}));

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
			var text = item.Text;
			foreach (var link in item.Links) {
				text += " " + link.AbsoluteUri;
			}

			var req = CreateRequest ("POST", new Uri ("https://alpha-api.app.net/stream/0/posts"), account);
			req.Parameters["text"] = text;

			return req.GetResponseAsync (cancellationToken).ContinueWith (reqTask => {
				var content = reqTask.Result.GetResponseText ();
				if (!content.Contains ("\"id\"")) {
					throw new SocialException ("app.net returned an unrecognized response.");
				}
			});
		}
	}
}

