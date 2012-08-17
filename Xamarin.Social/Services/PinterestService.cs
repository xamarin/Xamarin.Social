using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text;

namespace Xamarin.Social.Services
{
	public class PinterestService : Service
	{
		public PinterestService ()
			: base ("Pinterest", "Pinterest")
		{
			CreateAccountLink = new Uri ("https://pinterest.com/join/register/");
		}

		public override bool CanShareText {
			get {
				return true;
			}
		}

		public override bool CanShareImages {
			get {
				return true;
			}
		}

		class PinterestAuthenticator : FormAuthenticator
		{
			public PinterestAuthenticator ()
			{
				Fields.Add (new FormAuthenticatorField ("email", "Email", FormAuthenticatorFieldType.Email, "sally@example.com", ""));
				Fields.Add (new FormAuthenticatorField ("password", "Password", FormAuthenticatorFieldType.Password, "Required", ""));
			}

			public override Task<Account> SignInAsync ()
			{
				var http = new HttpHelper ();
				return http
					.GetAsync ("https://pinterest.com/login/")
					.ContinueWith (getTask => {

						var loginHtml = HttpHelper.ReadResponseText (getTask.Result);
						
						var email = GetFieldValue ("email");

						return http
							.PostUrlFormEncodedAsync ("https://pinterest.com/login/?next=%2Flogin%2F", new Dictionary<string, string> {
								{ "email", GetFieldValue ("email") },
								{ "password", GetFieldValue ("password") },
								{ "csrfmiddlewaretoken", ReadInputValue (loginHtml, "csrfmiddlewaretoken") },
								{ "next", "/" },
							})
							.ContinueWith (postTask => {

								if (postTask.Result.ResponseUri.AbsoluteUri.Contains ("/login")) {
									throw new ApplicationException ("The email or password is incorrect.");
								}
								else {
									return new Account (email, http.Cookies);
								}

							}).Result;
					});
			}

			static string ReadInputValue (string html, string name)
			{
				var ni = html.IndexOf ("name='" + name + "'");

				if (ni < 0) {
					throw new ApplicationException ("Bad response: missing " + name);
				}

				var vi = html.IndexOf ("value='", ni);
				if (vi < 0) {
					throw new ApplicationException ("Bad response: missing value for " + name);
				}

				var qi = html.IndexOf ("'", vi + 7);
				var val = html.Substring (vi + 7, qi - vi - 7);
				return val;
			}
		}

		protected override Authenticator GetAuthenticator (IDictionary<string, string> parameters)
		{
			return new PinterestAuthenticator ();
		}

		public override Task<ShareResult> ShareAsync (Item item)
		{
			throw new System.NotImplementedException ();
		}
	}
}

