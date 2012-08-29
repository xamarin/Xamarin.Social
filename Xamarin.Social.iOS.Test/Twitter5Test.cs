using System;
using NUnit.Framework;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.IO;
using Xamarin.Social.Services;
using System.Net;

namespace Xamarin.Social.iOS.Test
{
	[TestFixture]
	public class Twitter5Test
	{
		Service CreateService ()
		{
			return Service.Twitter;
		}

		[Test]
		public void TimelineRequest ()
		{
			var ps = new Dictionary<string,string> {
				{ "screen_name", "theSeanCook" },
				{ "count", "5" },
				{ "include_entities", "1" },
				{ "incode_rts", "1" },
			};
			var req = Service.Twitter.CreateRequest ("GET", new Uri ("http://api.twitter.com/1/statuses/user_timeline.json"), ps);
			var res = req.GetResponseAsync ().Result;
			Assert.That (res.StatusCode, Is.EqualTo (HttpStatusCode.OK));
			Assert.That (res.Headers.Count, Is.GreaterThan (0));
			var content = new StreamReader (res.GetResponseStream ()).ReadToEnd ();
			Assert.That (content.Length, Is.GreaterThan (10));
		}

		[Test]
		public void Manual_BlankTweet ()
		{
			var vc = CreateService ().GetShareUI (new Item (), result => {
				Console.WriteLine ("RESULT == " + result);
				AppDelegate.Shared.RootViewController.DismissModalViewControllerAnimated (true);
			});
			AppDelegate.Shared.RootViewController.PresentViewController (vc, true, null);
		}

		[Test]
		public void Manual_TextLinksAndImages ()
		{
			var item = new Item ("Hello, world!");
			item.Links.Add (new Uri ("http://xamarin.com"));
			item.Links.Add (new Uri ("http://microsoft.com"));
			item.Images.Add (UIImage.FromBundle ("Images/xamarin-logo.png"));
			item.Images.Add (UIImage.FromBundle ("Images/guides-block-img.png"));

			var vc = CreateService ().GetShareUI (item, result => {
				Console.WriteLine ("RESULT == " + result);
				AppDelegate.Shared.RootViewController.DismissModalViewControllerAnimated (true);
			});
			AppDelegate.Shared.RootViewController.PresentViewController (vc, true, null);
		}
	}
}
