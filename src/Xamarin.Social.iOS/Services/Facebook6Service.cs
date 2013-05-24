using System;
using MonoTouch.Social;

namespace Xamarin.Social.Services
{
	public class Facebook6Service : SocialService, IFacebookService
	{
		public Facebook6Service ()
			: base ("Facebook", "Facebook", SLServiceKind.Facebook)
		{
		}
	}
}