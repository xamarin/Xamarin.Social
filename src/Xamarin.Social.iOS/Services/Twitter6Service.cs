using System;
using MonoTouch.Accounts;
using MonoTouch.Social;

namespace Xamarin.Social.Services
{
	public class Twitter6Service : SocialService
	{
		public Twitter6Service ()
			: base ("Twitter", "Twitter", SLServiceKind.Twitter, ACAccountType.Twitter)
		{
		}
	}
}