using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.Foundation;
using System.Drawing;
using System.Threading.Tasks;
using System.Linq;

namespace Xamarin.Social
{
	class ShareController : UIViewController
	{
		UITextView textEditor;
		ProgressLabel progress;

		static UIFont TextEditorFont = UIFont.SystemFontOfSize (18);
		static readonly UIColor FieldColor = UIColor.FromRGB (56, 84, 135);

		public ShareController (ShareViewModel viewModel)
		{
			Title = NSBundle.MainBundle.LocalizedString (viewModel.Service.ShareTitle, "Title of Share dialog");

			View.BackgroundColor = UIColor.White;

			var b = View.Bounds;

			//
			// Fields
			//
			var fieldHeight = 33;

			ChoiceField afield = null;
			if (viewModel.Accounts.Count > 1) {
				afield = new ChoiceField (
					new RectangleF (0, b.Y, b.Width, 33),
					this,
					NSBundle.MainBundle.LocalizedString ("From", "From title when sharing"),
					viewModel.Accounts.Select (x => x.Username));
				View.AddSubview (afield);
				b.Y += fieldHeight;
				b.Height -= fieldHeight;
			}

			//
			// Text Editor
			//
			textEditor = new UITextView (new RectangleF (0, b.Y, b.Width, b.Height)) {
				Font = TextEditorFont,
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
				Text = viewModel.Item.Text,
			};
			View.AddSubview (textEditor);
			textEditor.BecomeFirstResponder ();

			//
			// Navigation Items
			//
			NavigationItem.LeftBarButtonItem = new UIBarButtonItem (
				UIBarButtonSystemItem.Cancel,
				delegate {

				ParentViewController.DismissModalViewControllerAnimated (true);

				viewModel.Cancel ();

			});

			bool sharing = false;
			NavigationItem.RightBarButtonItem = new UIBarButtonItem (
				NSBundle.MainBundle.LocalizedString ("Send", "Send button text when sharing"),
				UIBarButtonItemStyle.Done,
				delegate {

				if (!sharing) {

					sharing = true;
					NavigationItem.RightBarButtonItem.Enabled = false;
					StartProgress ();

					viewModel.Item.Text = textEditor.Text;

					if (viewModel.Accounts.Count > 1 && afield != null) {
						viewModel.UseAccount = viewModel.Accounts.First (x => x.Username == afield.SelectedItem);
					}

					viewModel.ShareAsync ().ContinueWith (shareTask => {

						sharing = false;
						NavigationItem.RightBarButtonItem.Enabled = true;
						StopProgress ();

						if (shareTask.IsFaulted) {
							this.ShowError ("Share Error", shareTask.Exception);
						}
						else {
							ParentViewController.DismissModalViewControllerAnimated (true);
						}

					}, TaskScheduler.FromCurrentSynchronizationContext ());
				}

			});
		}

		void StartProgress ()
		{
			if (progress == null) {
				progress = new ProgressLabel (NSBundle.MainBundle.LocalizedString ("Sending...", "Sending... status message when sharing"));
				NavigationItem.TitleView = progress;
				progress.StartAnimating ();
			}
		}

		void StopProgress ()
		{
			if (progress != null) {
				progress.StopAnimating ();
				NavigationItem.TitleView = null;
				progress = null;
			}
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return true;
		}

		void ResignFirstResponders ()
		{
			textEditor.ResignFirstResponder ();
		}

		abstract class Field : UIView
		{
			public ShareController Controller { get; private set; }
			public UILabel TitleLabel { get; private set; }

			public Field (RectangleF frame, ShareController controller, string title)
				: base (frame)
			{
				Controller = controller;

				BackgroundColor = UIColor.White;
				Opaque = true;
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth;

				TitleLabel = new UILabel () {
					BackgroundColor = UIColor.White,
					Font = TextEditorFont,
					Text = title + ":",
					TextColor = UIColor.Gray,
				};

				var w = TitleLabel.StringSize (TitleLabel.Text, TextEditorFont).Width + 8;
				TitleLabel.Frame = new RectangleF (8, 0, w, frame.Height - 1);

				AddSubview (TitleLabel);
			}

			public override void Draw (RectangleF rect)
			{
				var b = Bounds;
				using (var c = UIGraphics.GetCurrentContext ()) {
					UIColor.LightGray.SetStroke ();
					c.SetLineWidth (1.0f);
					c.MoveTo (0, b.Bottom);
					c.AddLineToPoint (b.Right, b.Bottom);
					c.StrokePath ();
				}
			}
		}

		class ChoiceField : Field
		{
			public string SelectedItem {
				get { return picker.SelectedItem; }
			}

			public LabelButton ValueLabel { get; private set; }
			public CheckedPickerView picker { get; private set; }

			public ChoiceField (RectangleF frame, ShareController controller, string title, IEnumerable<string> items)
				: base (frame, controller, title)
			{
				ValueLabel = new LabelButton () {
					BackgroundColor = UIColor.White,
					Font = TextEditorFont,
					TextColor = UIColor.DarkTextColor,
					AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
				};				
				var tf = TitleLabel.Frame;
				ValueLabel.Frame = new RectangleF (tf.Right, 0, frame.Width - tf.Right, frame.Height - 1);

				ValueLabel.TouchUpInside += HandleTouchUpInside;

				AddSubview (ValueLabel);

				picker = new CheckedPickerView (new RectangleF (0, 0, 320, 216), items);
				picker.Hidden = true;
				picker.SelectedItemChanged += delegate {
					ValueLabel.Text = picker.SelectedItem;
				};
				controller.View.AddSubview (picker);

				ValueLabel.Text = picker.SelectedItem;
			}

			void HandleTouchUpInside (object sender, EventArgs e)
			{
				Controller.ResignFirstResponders ();

				var v = Controller.View;

				picker.Hidden = false;
				picker.Frame = new RectangleF (0, v.Bounds.Bottom - 216, 320, 216);
				v.BringSubviewToFront (picker);
			}
		}

		class LabelButton : UILabel
		{
			public event EventHandler TouchUpInside;

			public LabelButton ()
			{
				UserInteractionEnabled = true;
			}

			public override void TouchesBegan (NSSet touches, UIEvent evt)
			{
				TextColor = FieldColor;
			}

			public override void TouchesEnded (NSSet touches, UIEvent evt)
			{
				TextColor = UIColor.DarkTextColor;

				var t = touches.ToArray<UITouch> ().First ();
				if (Bounds.Contains (t.LocationInView (this))) {
					var ev = TouchUpInside;
					if (ev != null) {
						ev (this, EventArgs.Empty);
					}
				}
			}

			public override void TouchesCancelled (NSSet touches, UIEvent evt)
			{
				TextColor = UIColor.DarkTextColor;
			}
		}
	}
}



