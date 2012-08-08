
using System;
using NUnit.Framework;
using MonoTouch.UIKit;

namespace Xamarin.Social.iOS.Test
{
	[TestFixture]
	public class Manual_NativeTwitterTest
	{
		[Test]
		public void PresentsDialog ()
		{
			var c = new MonoTouch.Twitter.TWTweetComposeViewController ();
			AppDelegate.Shared.RootViewController.PresentModalViewController (c, true);
		}
	}
}
