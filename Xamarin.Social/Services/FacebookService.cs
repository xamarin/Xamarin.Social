using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

namespace Xamarin.Social.Services
{
	public class FacebookService : Service
	{
		/// <summary>
		/// AppId is the 'App ID' at https://developers.facebook.com/apps
		/// and must be set.
		/// </summary>
		public string AppId { get; set; }

		public string Permissions { get; set; }

		public FacebookService ()
			: base ("Facebook", "Facebook")
		{
			CreateAccountLink = new Uri ("https://www.facebook.com");
			CanShareText = true;
		}

		protected override Authenticator GetAuthenticator ()
		{
			if (string.IsNullOrEmpty (AppId)) {
				throw new InvalidOperationException ("AppId must be set before using Facebook. " +
					"Get your 'App ID' at https://developers.facebook.com/apps");
			}
			if (string.IsNullOrEmpty (Permissions)) {
				throw new InvalidOperationException ("You must set the Permissions required by your app. " +
					"To share, \"publish_actions\" is required. " +
					"This is a comma separated list from https://developers.facebook.com/docs/authentication/permissions/");
			}
			return new FacebookAuthenticator (this);
		}

		class FacebookAuthenticator : WebAuthenticator
		{
			FacebookService service;

			public FacebookAuthenticator (FacebookService service)
			{
				this.service = service;
			}

			public override Task<Uri> GetInitialUrlAsync ()
			{
				return Task.Factory.StartNew (() => {
					var url = string.Format (
						"https://m.facebook.com/dialog/oauth/?client_id={0}&redirect_uri={1}&response_type=token&scope={2}",
						service.AppId,
						Uri.EscapeDataString ("http://www.facebook.com/connect/login_success.html"),
						service.Permissions);
					return new Uri (url);
				});
			}

			public override void OnPageLoaded (Uri url)
			{
				if (url.LocalPath == "/dialog/oauth/") {
					//
					// We end up here only if there was an error
					//
					OnFailure (AuthenticationResult.MissingRequiredProperty);
				}
				else if (url.LocalPath == "/connect/login_success.html") {
					//
					// Look for the access_token
					//
					var part = url
						.Fragment
						.Split ('#', '?', '&')
						.FirstOrDefault (p => p.StartsWith ("access_token="));
					if (part != null) {
						var accessToken = part.Substring ("access_token=".Length);

						//
						// Now we just need a username for the account
						//
						GetUsernameAsync (accessToken).ContinueWith (task => {
							if (task.IsFaulted) {
								OnFailure (task.Exception);
							}
							else {
								OnSuccess (task.Result, new Dictionary<string,string> {
									{ "access_token", accessToken },
								});
							}
						}, TaskScheduler.FromCurrentSynchronizationContext ());
					}
				}
			}

			Task<string> GetUsernameAsync (string accessToken)
			{
				var request = service.CreateRequest ("GET", new Uri ("https://graph.facebook.com/me"));
				request.Account = new Account ("?", new Dictionary<string,string> {
					{ "access_token", accessToken },
				});
				return request.GetResponseAsync ().ContinueWith (reqTask => {
					using (var s = reqTask.Result.GetResponseStream ()) {
						var json = new System.IO.StreamReader (s).ReadToEnd ();
						var username = GetValueFromJson (json, "username");
						if (string.IsNullOrEmpty (username)) {
							throw new Exception ("Could not read username from the /me API call");
						}
						else {
							return username;
						}
					}
				});
			}

			string GetValueFromJson (string json, string key)
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

		protected class FacebookRequest : Request
		{
			public FacebookRequest (string method, Uri url, IDictionary<string, string> parameters)
				: base (method, url, parameters)
			{
			}

			protected override Uri GetPreparedUrl ()
			{
				var a = Account;
				if (a == null) {
					throw new InvalidOperationException ("Facebook requests require a valid Account to be set.");
				}
				if (!a.Properties.ContainsKey ("access_token")) {
					throw new InvalidOperationException ("Facebook account is missing required access_token property.");
				}

				var url = base.GetPreparedUrl ().AbsoluteUri;

				if (url.Contains ('?')) {
					url += "&access_token=" + a.Properties["access_token"];
				}
				else {
					url += "?access_token=" + a.Properties["access_token"];
				}

				return new Uri (url);
			}
		}

		public override Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters = null)
		{
			return new FacebookRequest (method, url, parameters);
		}
	}
}

