using System;
using Android.NUnit;
using NUnit.Framework;
using Xamarin.Social.Services;

namespace Xamarin.Social.Android.Test
{
	[TestFixture]
	public class FacebookTest
	{
		FacebookService CreateService ()
		{
			return new FacebookService () {
				ClientId = "346691492084618",
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
