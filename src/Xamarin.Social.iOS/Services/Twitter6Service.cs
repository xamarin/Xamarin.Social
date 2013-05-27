using System;
using MonoTouch.Social;
using MonoTouch.Accounts;

namespace Xamarin.Social.Services
{
	public class Twitter6Service : SocialService, ITwitterService
	{
		public Twitter6Service ()
			: base ("Twitter", "Twitter", SLServiceKind.Twitter, ACAccountType.Twitter)
		{
		}
	}
}