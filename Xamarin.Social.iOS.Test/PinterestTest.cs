
using System;
using NUnit.Framework;
using MonoTouch.UIKit;
using MonoTouch.Accounts;
using Xamarin.Social.Services;

namespace Xamarin.Social.iOS.Test
{
	[TestFixture]
	public class PinterestTest
	{
		[Test]
		public void Manual_Authenticate ()
		{
			var service = new PinterestService ();

			var vc = service.GetAuthenticateUI (account => {
				Console.WriteLine ("AUTHENTICATE RESULT = " + account);
				AppDelegate.Shared.RootViewController.DismissModalViewControllerAnimated (true);
			});
			AppDelegate.Shared.RootViewController.PresentViewController (vc, true, null);
		}

		[Test]
		public void Manual_ShareImageTextLink ()
		{
			var service = new PinterestService ();

			var item = new Item {
				Text = "Hello, World",
			};
			item.Images.Add ("Images/xamarin-logo.png");
			item.Links.Add (new Uri ("http://xamarin.com"));

			var vc = service.GetShareUI (item, result => {
				Console.WriteLine ("SHARE RESULT = " + result);
				item.Dispose ();
				AppDelegate.Shared.RootViewController.DismissModalViewControllerAnimated (true);
			});
			AppDelegate.Shared.RootViewController.PresentViewController (vc, true, null);
		}
	}
}
