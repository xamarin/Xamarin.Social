using System;
using NUnit.Framework;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.IO;

namespace Xamarin.Social.iOS.Test
{
	[TestFixture]
	public class Twitter5Test
	{
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
			Assert.That (res.StatusCode, Is.EqualTo (200));
			Assert.That (res.Headers.Count, Is.GreaterThan (0));
			var content = new StreamReader (res.GetResponseStream ()).ReadToEnd ();
			Assert.That (content.Length, Is.GreaterThan (10));
		}

		[Test]
		public void Manual_BlankTweet ()
		{
			Service.Twitter.ShareAsync (AppDelegate.Shared.RootViewController, new Item ()).ContinueWith (task => {
				Console.WriteLine ("RESULT == " + task.Result);
			});
		}

		[Test]
		public void Manual_TextLinksAndImages ()
		{
			var item = new Item ("Hello, world!");
			item.Links.Add (new Uri ("http://xamarin.com"));
			item.Links.Add (new Uri ("http://microsoft.com"));
			item.Images.Add (UIImage.FromBundle ("Images/xamarin-logo.png"));
			item.Images.Add (UIImage.FromBundle ("Images/guides-block-img.png"));
			Service.Twitter.ShareAsync (AppDelegate.Shared.RootViewController, item).ContinueWith (task => {
				Console.WriteLine ("RESULT == " + task.Result);
			});
		}
	}
}
