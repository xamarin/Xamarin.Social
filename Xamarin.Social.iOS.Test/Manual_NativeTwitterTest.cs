
using System;
using NUnit.Framework;
using MonoTouch.UIKit;
using MonoTouch.Accounts;

namespace Xamarin.Social.iOS.Test
{
	[TestFixture]
	public class Manual_NativeTwitterTest
	{
		[Test]
		public void PresentsDialog ()
		{
			var c = new MonoTouch.Twitter.TWTweetComposeViewController ();

			c.SetCompletionHandler (result => {
				Console.WriteLine ("COMPLETED " + result);
				c.DismissModalViewControllerAnimated (true);
			});

			AppDelegate.Shared.RootViewController.PresentModalViewController (c, true);
		}

		[Test]
		public void HasMultipleAccounts ()
		{
			var store = new ACAccountStore ();
			var at = store.FindAccountType (ACAccountType.Twitter);

			store.RequestAccess (at, (granted, error) => {

				if (granted) {
					var accounts = store.FindAccounts (at);

					Assert.That (accounts.Length, Is.GreaterThanOrEqualTo (2));
				}
			});
		}


	}
}
