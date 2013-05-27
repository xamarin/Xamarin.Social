using System;
using System.Threading.Tasks;
using MonoTouch.Social;
using MonoTouch.Accounts;
using Xamarin.Auth;

namespace Xamarin.Social.Services
{
	public class Twitter6Service : SocialService, ITwitterService
	{
		public Twitter6Service ()
			: base ("Twitter", "Twitter", SLServiceKind.Twitter, ACAccountType.Twitter)
		{
		}

		public override Task<string> GetOAuthTokenAsync (Account acc)
		{
			throw new NotImplementedException ();
		}
	}
}