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
		ShareViewModel viewModel;

		UITextView textEditor;
		ProgressLabel progress;
		TextLengthLabel textLengthLabel;

		static UIFont TextEditorFont = UIFont.SystemFontOfSize (18);
		static readonly UIColor FieldColor = UIColor.FromRGB (56, 84, 135);

		public ShareController (ShareViewModel viewModel)
		{
			this.viewModel = viewModel;

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
			textEditor.Delegate = new TextEditorDelegate (this);
			View.AddSubview (textEditor);
			textEditor.BecomeFirstResponder ();

			//
			// Remaining Text Length
			//
			if (viewModel.HasMaxTextLength) {
				textLengthLabel = new TextLengthLabel (
					new RectangleF (4, b.Bottom - 22, b.Width - 8, 22),
					viewModel.Service.MaxTextLength) {
					TextLength = viewModel.TextLength,
				};
				View.AddSubview (textLengthLabel);
				var f = textEditor.Frame;
				f.Height -= 22;
				textEditor.Frame = f;
			}

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

			//
			// Watch for the keyboard
			//
			NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.DidShowNotification, HandleKeyboardDidShow);
			NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillHideNotification, HandleKeyboardDidHide);
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

		void HandleKeyboardDidShow (NSNotification n)
		{
			var size = UIKeyboard.BoundsFromNotification (n).Size;
			
			var f = textEditor.Frame;
			f.Height -= size.Height;
			textEditor.Frame = f;

			if (textLengthLabel != null) {
				f = textLengthLabel.Frame;
				f.Y -= size.Height;
				textLengthLabel.Frame = f;
			}
		}
		
		void HandleKeyboardDidHide (NSNotification n)
		{
			var size = UIKeyboard.BoundsFromNotification (n).Size;
			
			UIView.BeginAnimations ("kbd");
			
			var f = textEditor.Frame;
			f.Height += size.Height;
			textEditor.Frame = f;

			if (textLengthLabel != null) {
				f = textLengthLabel.Frame;
				f.Y += size.Height;
				textLengthLabel.Frame = f;
			}
			
			UIView.CommitAnimations ();
		}

		class TextEditorDelegate : UITextViewDelegate
		{
			ShareController controller;
			public TextEditorDelegate (ShareController controller)
			{
				this.controller = controller;
			}
			public override void Changed (UITextView textView)
			{
				controller.viewModel.Item.Text = textView.Text;
				if (controller.textLengthLabel != null) {
					controller.textLengthLabel.TextLength = controller.viewModel.TextLength;
				}
			}
		}

		class TextLengthLabel : UILabel
		{
			int maxLength;
			int textLength;

			static readonly UIColor okColor = UIColor.FromRGB (124, 124, 124);
			static readonly UIColor errorColor = UIColor.FromRGB (166, 80, 80);

			public int TextLength {
				get {
					return textLength;
				}
				set {
					textLength = value;
					Update ();
				}
			}

			public TextLengthLabel (RectangleF frame, int maxLength)
				: base (frame)
			{
				this.maxLength = maxLength;
				this.textLength = 0;
				UserInteractionEnabled = false;
				BackgroundColor = UIColor.Clear;
				AutoresizingMask = 
					UIViewAutoresizing.FlexibleWidth | 
					UIViewAutoresizing.FlexibleBottomMargin |
					UIViewAutoresizing.FlexibleTopMargin;
				TextAlignment = UITextAlignment.Right;
				Font = UIFont.BoldSystemFontOfSize (16);
				TextColor = okColor;
			}

			void Update ()
			{
				var rem = maxLength - textLength;
				Text = rem.ToString ();
				if (rem < 0) {
					TextColor = errorColor;
				}
				else {
					TextColor = okColor;
				}
			}
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



