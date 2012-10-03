
using System;
using NUnit.Framework;
using MonoTouch.UIKit;
using MonoTouch.Accounts;
using MonoTouch.Twitter;
using MonoTouch.Foundation;

namespace Xamarin.Social.iOS.Test
{
	[TestFixture]
	public class NativeTwitterTest
	{
		[Test]
		public void Manual_PresentsDialog ()
		{
			var c = new TWTweetComposeViewController ();

			c.SetCompletionHandler (result => {
				Console.WriteLine ("COMPLETED " + result);
				c.DismissModalViewControllerAnimated (true);
			});

			AppDelegate.Shared.RootViewController.PresentModalViewController (c, true);
		}

		[Test]
		public void Manual_HasMultipleAccounts ()
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
