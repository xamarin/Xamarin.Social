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
			CreateAccountLink = new Uri ("https://pinterest.com/join/signup/");
		}

		class PinterestAuthenticator : FormAuthenticator
		{
			public PinterestAuthenticator ()
			{
				Fields.Add (new FormAuthenticatorField ("email", "Email", FormAuthenticatorFieldType.PlainText, "sally@example.com", ""));
				Fields.Add (new FormAuthenticatorField ("password", "Password", FormAuthenticatorFieldType.Password, "Required", ""));
			}

			public override void OnSignIn ()
			{
				var http = new HttpHelper ();
				http
					.PostUrlFormEncodedAsync ("https://pinterest.com/login/?next=%2Flogin%2F", new Dictionary<string, string> {
						{ "email", GetFieldValue ("email") },
						{ "password", GetFieldValue ("password") },
						{ "next", "/" },
					})
					.ContinueWith (reqt => {
					});
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

