
using System;
using NUnit.Framework;
using MonoTouch.UIKit;
using System.IO;
using System.Collections.Generic;

namespace Xamarin.Social.iOS.Test
{
	[TestFixture]
	public class Auto_Twitter5Test
	{
		[Test]
		public void HasAccount ()
		{
			Assert.True (Service.Twitter.HasSavedAccounts);
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
			Assert.That (res.StatusCode, Is.EqualTo (200));
			Assert.That (res.Headers.Count, Is.GreaterThan (0));
			var content = new StreamReader (res.GetResponseStream ()).ReadToEnd ();
			Assert.That (content.Length, Is.GreaterThan (10));
		}
	}
}
