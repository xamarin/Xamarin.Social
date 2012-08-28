using System;
using NUnit.Framework;
using MonoTouch.UIKit;
using MonoTouch.Accounts;
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
		public void Manual_AddAccount ()
		{
			var service = CreateService ();
			service.AddAccountAsync (AppDelegate.Shared.RootViewController);
		}

		[Test]
		public void Manual_ShareTextLinks ()
		{
			var service = CreateService ();
			
			var item = new Item {
				Text = "This is just a test. Don't mind me...",
			};
			item.Links.Add (new Uri ("http://xamarin.com"));
			item.Links.Add (new Uri ("https://twitter.com/xamarinhq"));
			
			service.ShareAsync (AppDelegate.Shared.RootViewController, item).ContinueWith (t => {
				Console.WriteLine ("SHARE RESULT = " + t.Result);
				item.Dispose ();
			});
		}
		
		[Test]
		public void Manual_ShareImageTextLinks ()
		{
			var service = CreateService ();
			
			var item = new Item {
				Text = "This is just with an image. Don't mind me...",
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
		public void HomeTimeline ()
		{
			var service = CreateService ();
			
			var accounts = service.GetAccountsAsync ().Result;
			
			var req = service.CreateRequest ("GET", new Uri ("http://api.twitter.com/1/statuses/home_timeline.xml"), accounts[0]);

			var content = req.GetResponseAsync ().Result.GetResponseText ();
			
			Assert.IsTrue (content.Contains ("statuses"));
		}
	}
}
