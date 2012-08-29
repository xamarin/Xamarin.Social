
using System;
using NUnit.Framework;
using MonoTouch.UIKit;
using MonoTouch.Accounts;
using Xamarin.Social.Services;

namespace Xamarin.Social.iOS.Test
{
	[TestFixture]
	public class FlickrTest
	{
		FlickrService CreateService ()
		{
			return new FlickrService () {
				ConsumerKey = "cd876d2995de61c8f57efb44520461e2",
				ConsumerSecret = "34bf6099d244db6a",
			};
		}

		[Test]
		public void Manual_AddAccount ()
		{
			var service = CreateService ();
			service.AddAccountAsync (AppDelegate.Shared.RootViewController);
		}
		
		[Test]
		public void Manual_ShareImageTextLinks ()
		{
			var service = CreateService ();
			
			var item = new Item {
				Text = "Hello, World",
			};
			item.Images.Add ("Images/what_does_that_mean_trollcat.jpg");
			item.Links.Add (new Uri ("http://xamarin.com"));
			item.Links.Add (new Uri ("https://twitter.com/xamarinhq"));
			
			var vc = service.GetShareUI (item, result => {
				Console.WriteLine ("SHARE RESULT = " + result);
				item.Dispose ();
				AppDelegate.Shared.RootViewController.DismissModalViewControllerAnimated (true);
			});
			AppDelegate.Shared.RootViewController.PresentViewController (vc, true, null);
		}

		[Test]
		public void PeopleGetPhotos ()
		{
			var service = CreateService ();

			var accounts = service.GetAccountsAsync ().Result;

			var req = service.CreateRequest ("GET", new Uri ("http://www.flickr.com/services/rest"), accounts[0]);
			req.Parameters["user_id"] = "me";
			req.Parameters["method"] = "flickr.people.getPhotos";

			var content = req.GetResponseAsync ().Result.GetResponseText ();

			Assert.IsTrue (content.Contains ("stat=\"ok\""));
		}
	}
}
