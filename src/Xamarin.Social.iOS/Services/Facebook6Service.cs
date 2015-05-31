using System;
using MonoTouch.Accounts;
using MonoTouch.Social;
using Xamarin.Auth;

namespace Xamarin.Social.Services
{
	public class Facebook6Service : OSSocialService
	{
		public string FacebookAppId { get; set; }
		public ACFacebookAudience Audience { get; set; }
		public string Scope { get; set; }

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
	}
}