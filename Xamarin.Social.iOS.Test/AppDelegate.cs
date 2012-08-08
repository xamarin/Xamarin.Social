using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.NUnit.UI;

namespace Xamarin.Social.iOS.Test
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		UIWindow window;
		TouchRunner runner;

		public static AppDelegate Shared { get { return (AppDelegate)UIApplication.SharedApplication.Delegate; } }

		public UIViewController RootViewController { get { return window.RootViewController; } }

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			runner = new TouchRunner (window);

			runner.Add (System.Reflection.Assembly.GetExecutingAssembly ());

			window.RootViewController = new UINavigationController (runner.GetViewController ());
			
			window.MakeKeyAndVisible ();
			
			return true;
		}
	}
}

