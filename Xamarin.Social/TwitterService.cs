using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Xamarin.Social
{
	public abstract class TwitterService : Service
	{
		public TwitterService ()
			: base ("Twitter")
		{
		}

		public override string Description {
			get {
				return "Real-time information network that connects you to the latest stories, ideas, opinions and news about what you find interesting.";
			}
		}

		public override Uri SignUpLink {
			get {
				return new Uri ("https://twitter.com/signup");
			}
		}

		public override string SignUpTitle {
			get {
				return "Sign up";
			}
		}

		public override bool CanShareText {
			get {
				return true;
			}
		}

		public override bool CanShareLinks {
			get {
				return true;
			}
		}

		public override bool CanShareImages {
			get {
				return true;
			}
		}

		public override AccountCredential GetBlankCredential ()
		{
			var c = new AccountCredential ();
			c.Fields.Add (new AccountCredentialField ("username", "User Name", Xamarin.Social.AccountCredentialFieldType.PlainText, "@name", ""));
			c.Fields.Add (new AccountCredentialField ("password", "Password", Xamarin.Social.AccountCredentialFieldType.Password, "Required", ""));
			return c;
		}

		public override Task<Account> AuthenticateAsync (AccountCredential credential)
		{
			throw new System.NotImplementedException ();
		}

		public override Task<ShareResult> ShareAsync (Item item, Account account)
		{
			throw new System.NotImplementedException ();
		}

		public override Request CreateRequest (string method, Uri url, IDictionary<string, string> paramters = null)
		{
			throw new System.NotImplementedException ();
		}
	}
}

