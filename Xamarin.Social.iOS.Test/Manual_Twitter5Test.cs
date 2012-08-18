
using System;
using NUnit.Framework;
using MonoTouch.UIKit;

namespace Xamarin.Social.iOS.Test
{
	[TestFixture]
	public class Manual_Twitter5Test
	{
		[Test]
		public void BlankTweet ()
		{
			Service.Twitter.ShareAsync (AppDelegate.Shared.RootViewController, new Item ()).ContinueWith (task => {
				Console.WriteLine ("RESULT == " + task.Result);
			});
		}

		[Test]
		public void TextLinksAndImages ()
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
