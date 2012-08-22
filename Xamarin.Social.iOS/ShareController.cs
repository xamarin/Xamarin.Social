using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.Foundation;
using System.Drawing;
using System.Threading.Tasks;

namespace Xamarin.Social
{
	class ShareController : UIViewController
	{
		public ShareController (ShareViewModel viewModel)
		{
			Title = NSBundle.MainBundle.LocalizedString (viewModel.Service.ShareTitle, "Title of Share dialog");

			View.BackgroundColor = UIColor.White;

			var b = View.Bounds;
			
			var text = new UITextView (new RectangleF (0, 0, b.Width, b.Height)) {
				Font = UIFont.SystemFontOfSize (18),
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
				Text = viewModel.Item.Text,
			};
			View.AddSubview (text);
			text.BecomeFirstResponder ();

			NavigationItem.LeftBarButtonItem = new UIBarButtonItem (
				UIBarButtonSystemItem.Cancel,
				delegate {

				ParentViewController.DismissModalViewControllerAnimated (true);

				viewModel.Cancel ();

			});

			NavigationItem.RightBarButtonItem = new UIBarButtonItem (
				NSBundle.MainBundle.LocalizedString ("Send", "Send button text when sharing"),
				UIBarButtonItemStyle.Done,
				delegate {

				viewModel.Item.Text = text.Text;

				viewModel.ShareAsync ().ContinueWith (shareTask => {

					if (shareTask.IsFaulted) {
						ShowError (shareTask.Exception);
					}
					else {
						ParentViewController.DismissModalViewControllerAnimated (true);
					}

				}, TaskScheduler.FromCurrentSynchronizationContext ());

			});
		}

		void ShowError (Exception error)
		{
			var mainBundle = NSBundle.MainBundle;
			
			var alert = new UIAlertView (
				mainBundle.LocalizedString ("Share Error", "Error message title when failed to share"),
				mainBundle.LocalizedString (error.GetUserMessage (), "Error"),
				null,
				mainBundle.LocalizedString ("OK", "Dismiss button title when failed to share"));
			
			alert.Show ();
		}
	}
}



