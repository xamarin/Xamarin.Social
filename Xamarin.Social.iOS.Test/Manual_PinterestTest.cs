
using System;
using NUnit.Framework;
using MonoTouch.UIKit;
using MonoTouch.Accounts;
using Xamarin.Social.Services;

namespace Xamarin.Social.iOS.Test
{
	[TestFixture]
	public class Manual_PinterestTest
	{
		[Test]
		public void AddAccount ()
		{
			var pinterest = new PinterestService ();

			pinterest.AddAccountAsync (AppDelegate.Shared.RootViewController);
		}
	}
}
