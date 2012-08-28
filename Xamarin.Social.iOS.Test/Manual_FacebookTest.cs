
using System;
using NUnit.Framework;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace Xamarin.Social.iOS.Test
{
	[TestFixture]
	public class FacebookTest
	{
		const string TestClientId = "110632278980691";

		[Test]
		public void Manual_AddAccount ()
		{
			Service.Facebook.AppId = TestClientId;
			Service.Facebook.Permissions = "publish_actions";
			Service.Facebook.AddAccountAsync (AppDelegate.Shared.RootViewController).ContinueWith (task => {
				if (task.IsFaulted) {
					Console.WriteLine (task.Exception);
				}
				else {
					Console.WriteLine ("RESULT " + task.Result);
					Service.Facebook.GetAccountsAsync ().ContinueWith (accountsTask => {
						Console.WriteLine ("ACCOUNTS = " + accountsTask.Result.Count);
					});
				}
			});
		}

		[Test]
		public void Manual_BadClientId ()
		{
			Service.Facebook.AppId = "537639364986381";
			Service.Facebook.Permissions = "publish_actions";
			Service.Facebook.AddAccountAsync (AppDelegate.Shared.RootViewController).ContinueWith (task => {
				Console.WriteLine ("RESULT " + task.Result);
			});
		}
	}
}
