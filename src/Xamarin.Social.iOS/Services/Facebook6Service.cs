using System;
using MonoTouch.Social;
using MonoTouch.Accounts;

namespace Xamarin.Social.Services
{
	public class Facebook6Service : SocialService, IFacebookService
	{
		public string FacebookAppId { get; set; }

		public Facebook6Service ()
			: base ("Facebook", "Facebook", SLServiceKind.Facebook, ACAccountType.Facebook)
		{
		}

		protected override AccountStoreOptions AccountStoreOptions {
			get {
				var options = new AccountStoreOptions {
					FacebookAppId = FacebookAppId  
				};

				options.SetPermissions (ACFacebookAudience.OnlyMe, "email");
				return options;
			}
		}
	}
}