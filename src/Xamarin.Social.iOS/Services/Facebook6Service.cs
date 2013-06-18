using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonoTouch.Accounts;
using MonoTouch.Social;
using Xamarin.Auth;

namespace Xamarin.Social.Services
{
	public class Facebook6Service : SocialService, ISupportScope
	{
		public string FacebookAppId { get; set; }
		public ACFacebookAudience Audience { get; set; }
		public string Scope { get; set; }

		public string [] Scopes {
			set {
				Scope = string.Join (",", value);
			}
		}

		protected override AccountStoreOptions AccountStoreOptions {
			get {
				var options = new AccountStoreOptions {
					FacebookAppId = FacebookAppId
				};

				options.SetPermissions (Audience, Scope);
				return options;
			}
		}

		public Facebook6Service ()
			: base ("Facebook", "Facebook", SLServiceKind.Facebook, ACAccountType.Facebook)
		{
			Audience = ACFacebookAudience.Everyone;
		}

		public override Task<IDictionary<string, string>> GetAccessTokenAsync (Account account)
		{
			var tcs = new TaskCompletionSource<IDictionary<string, string>> ();
			tcs.SetResult (new Dictionary<string, string> {
				{ "access_token", ((ACAccountWrapper) account).ACAccount.Credential.OAuthToken }
			});
			return tcs.Task;
		}

		public override bool SupportsVerification {
			get {
				return true;
			}
		}

		public override Task VerifyAsync (Account account)
		{
			return CreateRequest ("GET",
      			new Uri ("https://graph.facebook.com/me"),
	            account
			).GetResponseAsync ().ContinueWith (t => {
				if (!t.Result.GetResponseText ().Contains ("\"id\""))
					throw new SocialException ("Unrecognized Facebook response.");
			});
		}
	}
}