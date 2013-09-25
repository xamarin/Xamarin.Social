using System;
using MonoTouch.Accounts;
using MonoTouch.Social;

namespace Xamarin.Social.Services
{
	public class Twitter6Service : OSSocialService
	{
		public Twitter6Service ()
			: base ("Twitter", "Twitter", SLServiceKind.Twitter, ACAccountType.Twitter)
		{
		}
	}
}