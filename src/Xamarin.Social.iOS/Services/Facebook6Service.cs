using System;
using System.Threading.Tasks;
using MonoTouch.Social;
using MonoTouch.Accounts;
using Xamarin.Auth;

namespace Xamarin.Social.Services
{
	public class Facebook6Service : SocialService
	{
		public string FacebookAppId { get; set; }

		protected override AccountStoreOptions AccountStoreOptions {
			get {
				var options = new AccountStoreOptions {
					FacebookAppId = FacebookAppId  
				};

				options.SetPermissions (ACFacebookAudience.OnlyMe, "email");
				return options;
			}
		}

		public Facebook6Service ()
			: base ("Facebook", "Facebook", SLServiceKind.Facebook, ACAccountType.Facebook)
		{
		}
	}
}