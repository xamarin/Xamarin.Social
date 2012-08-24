using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;

namespace Xamarin.Social.Services
{
	public class PinterestService : Service
	{
		public PinterestService ()
			: base ("Pinterest", "Pinterest")
		{
			CreateAccountLink = new Uri ("https://pinterest.com/join/register/");
			ShareTitle = "Pin";
			CanShareText = true;
			CanShareImages = true;
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

						var loginHtml = getTask.Result.GetResponseText ();
						
						var email = GetFieldValue ("email");
						var password = GetFieldValue ("password");

						return http
							.PostUrlFormEncodedAsync ("https://pinterest.com/login/?next=%2Flogin%2F", new Dictionary<string, string> {
								{ "email", email },
								{ "password", password },
								{ "csrfmiddlewaretoken", ReadInputValue (loginHtml, "csrfmiddlewaretoken") },
								{ "next", "/" },
							})
							.ContinueWith (postTask => {

								if (postTask.Result.ResponseUri.AbsoluteUri.Contains ("/login")) {
									throw new ApplicationException ("The email or password is incorrect.");
								}
								else {
									return new Account (
											email,
											new Dictionary<string, string> {
												{ "email", email },
												{ "password", password }
											},
											http.Cookies);
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

		protected override Authenticator GetAuthenticator ()
		{
			return new PinterestAuthenticator ();
		}

		protected override Task ShareItemAsync (Item item, Account account, CancellationToken cancellationToken)
		{
			var req = CreateRequest ("POST", new Uri ("https://pinterest.com/pin/create/"), account);

			req.AddMultipartData ("board", "451978581292091392");
			req.AddMultipartData ("details", item.Text);
			req.AddMultipartData ("link", item.Links.Count > 0 ? item.Links.First ().AbsoluteUri : "");
			req.AddMultipartData ("img_url", "");
			req.AddMultipartData ("tags", "");
			req.AddMultipartData ("replies", "");
			req.AddMultipartData ("buyable", "");
			req.AddMultipartData ("img", item.Images.First ());
			req.AddMultipartData ("csrfmiddlewaretoken", account.Cookies.GetCookie (new Uri ("https://pinterest.com"), "csrftoken"));

			return req.GetResponseAsync (cancellationToken).ContinueWith (t => {
				var body = t.Result.GetResponseText ();
				if (!body.Contains ("\"success\"")) {
					throw new ApplicationException ("Pinterest did not accept the new Pin.");
				}
			});
		}
	}
}

