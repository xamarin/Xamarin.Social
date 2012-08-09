using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Xamarin.Social
{
	public class FacebookService : Service
	{
		public FacebookService ()
			: base ("Facebook")
		{
		}

		public override string Description {
			get {
				return "A social utility that connects people, to keep up with friends, upload photos, share links and videos.";
			}
		}

		public override bool CanShareText {
			get {
				return true;
			}
		}

		protected override Authenticator GetAuthenticator (IDictionary<string, string> parameters)
		{
			if (parameters == null) {
				throw new ArgumentNullException ("parameters");
			}
			if (!parameters.ContainsKey ("client_id")) {
				throw new ArgumentException ("parameters must include the client_id of your Facebook application. " +
					"It is called 'App ID' at https://developers.facebook.com/apps", "parameters");
			}
			if (!parameters.ContainsKey ("scope")) {
				throw new ArgumentException ("parameters must include the scope of your Facebook application. " +
					"This is a comma separated list of permissions. To share, \"publish_actions\" is required. " +
					"The full list of permissions is available at https://developers.facebook.com/docs/authentication/permissions/", "parameters");
			}
			return new FacebookAuthenticator (parameters["client_id"], parameters["scope"], this);
		}

		class FacebookAuthenticator : WebAuthenticator
		{
			string clientId;
			string scope;
			FacebookService service;

			public FacebookAuthenticator (string clientId, string scope, FacebookService service)
			{
				this.clientId = clientId;
				this.scope = scope;
			}

			public override string Title {
				get {
					return "Facebook";
				}
			}

			public override Uri InitialUrl
			{
				get {
					var url = string.Format (
						"https://m.facebook.com/dialog/oauth/?client_id={0}&redirect_uri={1}&response_type=token&scope={2}",
						clientId,
						Uri.EscapeDataString ("http://www.facebook.com/connect/login_success.html"),
						scope);
					return new Uri (url);
				}
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
				return request.GetResponseAsync ().ContinueWith (reqTask => {
					return "";
				});
			}
		}

		public override Task<ShareResult> ShareAsync (Item item)
		{
			throw new NotImplementedException ();
		}

		public override Request CreateRequest (string method, Uri url, IDictionary<string, string> paramters = null)
		{
			throw new NotImplementedException ();
		}
	}
}

