
using System;
using NUnit.Framework;
using MonoTouch.UIKit;
using System.Collections.Generic;
using Xamarin.Social.Services;
using System.Linq;

namespace Xamarin.Social.iOS.Test
{
	[TestFixture]
	public class FacebookTest
	{
		const string TestClientId = "346691492084618";

		FacebookService CreateService ()
		{
			return new FacebookService () {
				ClientId = TestClientId,
			};
		}

		[Test]
		public void Feed ()
		{
			var service = CreateService ();

			var account = service.GetAccountsAsync ().Result.First ();

			var req = service.CreateRequest ("GET", new Uri ("https://graph.facebook.com/me/feed"), account);

			var res = req.GetResponseAsync ().Result.GetResponseText ();

			Assert.That (res.Contains ("\"data\""));
		}

		[Test]
		public void Manual_ShareImageTextLink ()
		{
			var service = CreateService ();
			
			var item = new Item {
				Text = "Hey everyone, I'm going to be spamming Facebook for a little while. Don't mind me!",
			};
			item.Images.Add ("Images/what_does_that_mean_trollcat.jpg");
			item.Links.Add (new Uri ("http://praeclarum.org"));

			var vc = service.GetShareUI (item, result => {
				Console.WriteLine ("SHARE RESULT = " + result);
				item.Dispose ();
				AppDelegate.Shared.RootViewController.DismissModalViewControllerAnimated (true);
			});
			AppDelegate.Shared.RootViewController.PresentViewController (vc, true, null);
		}

		[Test]
		public void Manual_ShareTextLink ()
		{
			var service = CreateService ();
			
			var item = new Item {
				Text = "Hey everyone, I'm going to be spamming Facebook for a little while. Don't mind me!",
			};
			item.Links.Add (new Uri ("http://praeclarum.org"));

			var vc = service.GetShareUI (item, result => {
				Console.WriteLine ("SHARE RESULT = " + result);
				item.Dispose ();
				AppDelegate.Shared.RootViewController.DismissModalViewControllerAnimated (true);
			});
			AppDelegate.Shared.RootViewController.PresentViewController (vc, true, null);
		}

		[Test]
		public void Manual_Authenticate ()
		{
			var service = CreateService ();

			var vc = service.GetAuthenticateUI (result => {
				Console.WriteLine ("AUTHENTICATE RESULT = " + result);
				AppDelegate.Shared.RootViewController.DismissModalViewControllerAnimated (true);
			});
			AppDelegate.Shared.RootViewController.PresentViewController (vc, true, null);
		}

		[Test]
		public void Manual_BadClientId ()
		{
			var service = CreateService ();
			
			var vc = service.GetAuthenticateUI (result => {
				Console.WriteLine ("RESULT " + result);
			});
			AppDelegate.Shared.RootViewController.PresentViewController (vc, true, null);
		}
	}
}
