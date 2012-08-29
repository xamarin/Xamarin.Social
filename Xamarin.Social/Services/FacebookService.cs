using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

namespace Xamarin.Social.Services
{
	public class FacebookService : OAuth2Service
	{
		public FacebookService ()
			: base ("Facebook", "Facebook")
		{
			CreateAccountLink = new Uri ("https://www.facebook.com");

			MaxTextLength = int.MaxValue;

			AuthorizeUrl = new Uri ("https://m.facebook.com/dialog/oauth/");
			RedirectUrl = new Uri ("http://www.facebook.com/connect/login_success.html");
		}

		protected override Task<string> GetUsernameAsync (string accessToken)
		{
			var request = CreateRequest (
				"GET",
				new Uri ("https://graph.facebook.com/me"),
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
	}
}

