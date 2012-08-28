
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
		[Test]
		public void Manual_AddAccount ()
		{
			var service = new FlickrService () {
				ConsumerKey = "cd876d2995de61c8f57efb44520461e2",
				ConsumerSecret = "34bf6099d244db6a",
			};
			service.AddAccountAsync (AppDelegate.Shared.RootViewController);
		}
		
		[Test]
		public void Manual_ShareImageTextLinks ()
		{
			var service = new FlickrService () {
				ConsumerKey = "cd876d2995de61c8f57efb44520461e2",
				ConsumerSecret = "34bf6099d244db6a",
			};
			
			var item = new Item {
				Text = "Hello, World",
			};
			item.Images.Add ("Images/xamarin-logo.png");
			item.Links.Add (new Uri ("http://xamarin.com"));
			item.Links.Add (new Uri ("https://twitter.com/xamarinhq"));
			
			service.ShareAsync (AppDelegate.Shared.RootViewController, item).ContinueWith (t => {
				Console.WriteLine ("SHARE RESULT = " + t.Result);
				item.Dispose ();
			});
		}

		[Test]
		public void PeopleGetPhotos ()
		{
			var service = new FlickrService () {
				ConsumerKey = "cd876d2995de61c8f57efb44520461e2",
				ConsumerSecret = "34bf6099d244db6a",
			};

			var accounts = service.GetAccountsAsync ().Result;

			var req = service.CreateRequest ("POST", new Uri ("http://www.flickr.com/services/rest"), accounts[0]);
			req.Parameters["user_id"] = "me";
			req.Parameters["method"] = "flickr.people.getPhotos";

			var content = req.GetResponseAsync ().Result.GetResponseText ();

			Console.WriteLine (content);
			Assert.IsTrue (content.Contains ("success"));
		}
	}
}
