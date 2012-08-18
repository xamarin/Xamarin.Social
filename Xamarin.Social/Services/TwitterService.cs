using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Xamarin.Social.Services
{
	public abstract class TwitterService : Service
	{
		public TwitterService ()
			: base ("Twitter", "Twitter")
		{
			CreateAccountLink = new Uri ("https://twitter.com/signup");
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

		protected override Authenticator GetAuthenticator (IDictionary<string, string> parameters)
		{
			throw new NotImplementedException ();
		}

		public override Request CreateRequest (string method, Uri url, IDictionary<string, string> paramters = null)
		{
			throw new System.NotImplementedException ();
		}
	}
}

