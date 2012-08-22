
using System;
using NUnit.Framework;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace Xamarin.Social.iOS.Test
{
	[TestFixture]
	public class Manual_FacebookTest
	{
		const string TestClientId = "110632278980691";

		[Test]
		public void AddAccount ()
		{
			Service.Facebook.AppId = TestClientId;
			Service.Facebook.Permissions = "publish_actions";
			Service.Facebook.AddAccountAsync (AppDelegate.Shared.RootViewController).ContinueWith (task => {
				if (task.IsFaulted) {
					Console.WriteLine (task.Exception);
				}
				else {
					Console.WriteLine ("RESULT " + task.Result);
					Service.Facebook.GetSavedAccountsAsync ().ContinueWith (accountsTask => {
						Console.WriteLine ("ACCOUNTS = " + accountsTask.Result.Length);
					});
				}
			});
		}

		[Test]
		public void BadClientId ()
		{
			Service.Facebook.AppId = "537639364986381";
			Service.Facebook.Permissions = "publish_actions";
			Service.Facebook.AddAccountAsync (AppDelegate.Shared.RootViewController).ContinueWith (task => {
				Console.WriteLine ("RESULT " + task.Result);
			});
		}
	}
}
