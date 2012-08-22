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

		protected override Task<ShareResult> ShareItemAsync (Item item, Account account, CancellationToken cancellationToken)
		{
			var req = CreateRequest ("POST", new Uri ("https://pinterest.com/pin/create/"));
			req.Account = account;

			req.AddMultipartData ("board", "451978581292091392");
			req.AddMultipartData ("details", item.Text);
			req.AddMultipartData ("link", item.Links.Count > 0 ? item.Links.First ().AbsoluteUri : "");
			req.AddMultipartData ("img_url", "");
			req.AddMultipartData ("tags", "");
			req.AddMultipartData ("replies", "");
			req.AddMultipartData ("buyable", "");
			var imageData = item.Images.First ().GetImageData ("image/jpeg");
			req.AddMultipartData ("img", imageData.Stream, imageData.MimeType, imageData.Filename);
			req.AddMultipartData ("csrfmiddlewaretoken", "???");

			return req.GetResponseAsync ().ContinueWith (reqTask => {

				var res = reqTask.Result;

				throw new NotImplementedException ();

				return ShareResult.Done;

			}, cancellationToken);
		}
	}
}

