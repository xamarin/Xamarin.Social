using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.Foundation;

namespace Xamarin.Social
{
	class ShareController : UIViewController
	{
		public ShareController (Service service, IEnumerable<Account> accounts, Item item, ShareProgress progress)
		{
			Title = NSBundle.MainBundle.LocalizedString ("Share", "Title of Share dialog");

			View.BackgroundColor = UIColor.White;

			NavigationItem.LeftBarButtonItem = new UIBarButtonItem (
				UIBarButtonSystemItem.Cancel,
				delegate {

				ParentViewController.DismissModalViewControllerAnimated (true);

				progress.Result = ShareResult.Cancelled;
				progress.DoneEvent.Set ();

			});
		}
	}
}

