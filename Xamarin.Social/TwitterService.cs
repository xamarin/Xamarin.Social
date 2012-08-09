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

		protected override Authenticator GetAuthenticator (IDictionary<string, string> parameters)
		{
			throw new NotImplementedException ();
		}

		public override Task<ShareResult> ShareAsync (Item item)
		{
			throw new System.NotImplementedException ();
		}

		public override Request CreateRequest (string method, Uri url, IDictionary<string, string> paramters = null)
		{
			throw new System.NotImplementedException ();
		}
	}
}

