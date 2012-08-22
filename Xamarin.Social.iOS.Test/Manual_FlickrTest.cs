
using System;
using NUnit.Framework;
using MonoTouch.UIKit;
using MonoTouch.Accounts;
using Xamarin.Social.Services;

namespace Xamarin.Social.iOS.Test
{
	[TestFixture]
	public class Manual_FlickrTest
	{
		[Test]
		public void AddAccount ()
		{
			var service = new FlickrService () {
				Key = "cd876d2995de61c8f57efb44520461e2",
				Secret = "34bf6099d244db6a",
			};
			service.AddAccountAsync (AppDelegate.Shared.RootViewController);
		}
		
		[Test]
		public void ShareImageTextLink ()
		{
			var service = new FlickrService () {
				Key = "cd876d2995de61c8f57efb44520461e2",
				Secret = "34bf6099d244db6a",
			};
			
			var item = new Item {
				Text = "Hello, World",
			};
			item.Images.Add ("Images/xamarin-logo.png");
			item.Links.Add (new Uri ("http://xamarin.com"));
			
			service.ShareAsync (AppDelegate.Shared.RootViewController, item).ContinueWith (t => {
				Console.WriteLine ("SHARE RESULT = " + t.Result);
				item.Dispose ();
			});
		}
	}
}
