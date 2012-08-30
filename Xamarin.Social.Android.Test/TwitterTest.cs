using System;
using NUnit.Framework;
using Android.NUnit;
using Xamarin.Social.Services;

namespace Xamarin.Social.iOS.Test
{
	[TestFixture]
	public class TwitterTest
	{
		TwitterService CreateService ()
		{
			return new TwitterService () {
				ConsumerKey = "GsileCDN9PqjHNoIKUfGQ",
				ConsumerSecret = "g7gAeVQPJzTC4zwS0ftVPWsv3brVQVgfyR47gD03lk",
			};
		}

		[Test]
		public void Manual_Authenticate ()
		{
			var service = CreateService ();
			var intent = service.GetAuthenticateUI (TestRunner.Shared, result => {
				Console.WriteLine ("AUTHENTICATE RESULT = " + result);
			});
			TestRunner.Shared.StartActivityForResult (intent, 42);
		}
	}
}
