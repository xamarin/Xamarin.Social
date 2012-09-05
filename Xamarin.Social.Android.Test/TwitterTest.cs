using System;
using NUnit.Framework;
using Android.NUnit;
using Xamarin.Social.Services;

namespace Xamarin.Social.Android.Test
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

		[Test]
		public void Manual_ShareText ()
		{
			var service = CreateService ();

			var item = new Item ("Hello from Android!");

			var intent = service.GetShareUI (TestRunner.Shared, item, result => {
				Console.WriteLine ("AUTHENTICATE RESULT = " + result);
			});
			TestRunner.Shared.StartActivityForResult (intent, 42);
		}

		[Test]
		public void Manual_ShareTextLinkImage ()
		{
			var service = CreateService ();

			var item = new Item ("Hello image from Android!");
			item.Links.Add (new Uri ("http://xamarin.com"));
			item.Images.Add (new ImageData (TestRunner.Shared.Assets.Open ("what_does_that_mean_trollcat.jpg"), "image/jpeg"));

			var intent = service.GetShareUI (TestRunner.Shared, item, result => {
				Console.WriteLine ("AUTHENTICATE RESULT = " + result);
				item.Dispose ();
			});
			TestRunner.Shared.StartActivityForResult (intent, 42);
		}
	}
}
