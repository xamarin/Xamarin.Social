using System;
using NUnit.Framework;
using Android.NUnit;
using Xamarin.Social.Services;

namespace Xamarin.Social.Android.Test
{
	[TestFixture]
	public class PinterestTest
	{
		PinterestService CreateService ()
		{
			return new PinterestService ();
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
		public void Manual_ShareTextImageLinks ()
		{
			var service = CreateService ();
			
			var item = new Item {
				Text = "Hello, World from Android",
			};

			item.Images.Add (new ImageData (TestRunner.Shared.Assets.Open ("what_does_that_mean_trollcat.jpg"), "image/jpeg"));
			item.Links.Add (new Uri ("http://xamarin.com"));
			item.Links.Add (new Uri ("https://twitter.com/xamarinhq"));
			
			var intent = service.GetShareUI (TestRunner.Shared, item, result => {
				Console.WriteLine ("SHARE RESULT = " + result);
				item.Dispose ();
			});
			TestRunner.Shared.StartActivityForResult (intent, 42);
		}
	}
}
