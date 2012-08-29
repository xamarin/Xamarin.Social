using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.Foundation;
using System.Drawing;
using System.Threading.Tasks;
using System.Linq;
using MonoTouch.CoreGraphics;

namespace Xamarin.Social
{
	public class ShareViewController : UIViewController
	{
		Service service;
		Item item;
		List<Account> accounts;
		Action<ShareResult> completionHandler;

		UITextView textEditor;
		ProgressLabel progress;
		TextLengthLabel textLengthLabel;
		UILabel linksLabel;
		ChoiceField afield = null;

		bool sharing = false;

		static UIFont TextEditorFont = UIFont.SystemFontOfSize (18);
		static readonly UIColor FieldColor = UIColor.FromRGB (56, 84, 135);

		internal ShareViewController (Service service, Item item, Action<ShareResult> completionHandler)
		{
			this.service = service;
			this.item = item;
			this.completionHandler = completionHandler;

			Title = NSBundle.MainBundle.LocalizedString (service.ShareTitle, "Title of Share dialog");
			
			View.BackgroundColor = UIColor.White;

			service.GetAccountsAsync ().ContinueWith (t => {
				accounts = t.Result;
				BuildUI ();
			}, TaskScheduler.FromCurrentSynchronizationContext ());
		}

		void BuildUI ()
		{
			var b = View.Bounds;

			var statusHeight = 22.0f;

			//
			// Fields
			//
			var fieldHeight = 33;

			if (accounts.Count > 1) {
				afield = new ChoiceField (
					new RectangleF (0, b.Y, b.Width, 33),
					this,
					NSBundle.MainBundle.LocalizedString ("From", "From title when sharing"),
					accounts.Select (x => x.Username));
				View.AddSubview (afield);
				b.Y += fieldHeight;
				b.Height -= fieldHeight;
			}

			//
			// Text Editor
			//
			var editorHeight = b.Height;
			if (service.HasMaxTextLength || item.Links.Count > 0) {
				editorHeight -= statusHeight;
			}
			textEditor = new UITextView (new RectangleF (0, b.Y, b.Width, editorHeight)) {
				Font = TextEditorFont,
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
				Text = item.Text,
			};
			textEditor.Delegate = new TextEditorDelegate (this);
			View.AddSubview (textEditor);
			textEditor.BecomeFirstResponder ();

			//
			// Icons
			//
			if (item.Images.Count > 0) {

				var rem = 4.0f;
				RectangleF f;
				var x = b.Right - AttachmentIcon.Size - 8 - rem*(item.Images.Count - 1);
				var y = textEditor.Frame.Y + 8;

				f = textEditor.Frame;
				f.Width = x - 8 - f.X;
				textEditor.Frame = f;

				foreach (var i in item.Images) {
					var icon = new ImageIcon (i.Image);

					f = icon.Frame;
					f.X = x;
					f.Y = y;
					icon.Frame = f;

					View.AddSubview (icon);

					x += rem;
					y += rem;
				}
			}

			//
			// Remaining Text Length
			//
			if (service.HasMaxTextLength) {
				textLengthLabel = new TextLengthLabel (
					new RectangleF (4, b.Bottom - statusHeight, textEditor.Frame.Width - 8, statusHeight),
					service.MaxTextLength) {
					TextLength = service.GetTextLength (item),
				};
				View.AddSubview (textLengthLabel);
			}
			
			//
			// Links Label
			//
			if (item.Links.Count > 0) {
				linksLabel = new UILabel (
					new RectangleF (4, b.Bottom - statusHeight, textEditor.Frame.Width - 66, statusHeight)) {
					TextColor = UIColor.FromRGB (124, 124, 124),
					AutoresizingMask =
						UIViewAutoresizing.FlexibleTopMargin |
						UIViewAutoresizing.FlexibleBottomMargin |
						UIViewAutoresizing.FlexibleWidth,

					UserInteractionEnabled = false,
					BackgroundColor = UIColor.Clear,
					Font = UIFont.SystemFontOfSize (16),
					LineBreakMode = UILineBreakMode.HeadTruncation,
				};
				if (item.Links.Count == 1) {
					linksLabel.Text = item.Links[0].AbsoluteUri;
				}
				else {
					linksLabel.Text = string.Format (
						NSBundle.MainBundle.LocalizedString ("{0} links", "# of links label"),
						item.Links.Count);
				}
				View.AddSubview (linksLabel);
			}

			//
			// Navigation Items
			//
			NavigationItem.LeftBarButtonItem = new UIBarButtonItem (
				UIBarButtonSystemItem.Cancel,
				delegate {
					completionHandler (ShareResult.Cancelled);
				});


			NavigationItem.RightBarButtonItem = new UIBarButtonItem (
				NSBundle.MainBundle.LocalizedString ("Send", "Send button text when sharing"),
				UIBarButtonItemStyle.Done,
				HandleSend);

			//
			// Watch for the keyboard
			//
			NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.DidShowNotification, HandleKeyboardDidShow);
			NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillHideNotification, HandleKeyboardDidHide);
		}

		void HandleSend (object sender, EventArgs e)
		{
			if (sharing) return;
				
			item.Text = textEditor.Text;
			
			StartSharing ();

			var account = accounts.FirstOrDefault ();
			if (accounts.Count > 1 && afield != null) {
				account = accounts.FirstOrDefault (x => x.Username == afield.SelectedItem);
			}
			
			try {
				service.ShareItemAsync (item, account).ContinueWith (shareTask => {
					
					StopSharing ();
					
					if (shareTask.IsFaulted) {
						this.ShowError ("Share Error", shareTask.Exception);
					}
					else {
						completionHandler (ShareResult.Done);
					}
					
				}, TaskScheduler.FromCurrentSynchronizationContext ());
			}
			catch (Exception ex) {
				StopSharing ();
				this.ShowError ("Share Error", ex);
			}
		}

		void StartSharing ()
		{
			sharing = true;
			NavigationItem.RightBarButtonItem.Enabled = false;

			if (progress == null) {
				progress = new ProgressLabel (NSBundle.MainBundle.LocalizedString ("Sending...", "Sending... status message when sharing"));
				NavigationItem.TitleView = progress;
				progress.StartAnimating ();
			}
		}

		void StopSharing ()
		{
			sharing = false;
			NavigationItem.RightBarButtonItem.Enabled = true;

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

			if (linksLabel != null) {
				f = linksLabel.Frame;
				f.Y -= size.Height;
				linksLabel.Frame = f;
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

			if (linksLabel != null) {
				f = linksLabel.Frame;
				f.Y += size.Height;
				linksLabel.Frame = f;
			}
			
			UIView.CommitAnimations ();
		}

		class TextEditorDelegate : UITextViewDelegate
		{
			ShareViewController controller;
			public TextEditorDelegate (ShareViewController controller)
			{
				this.controller = controller;
			}
			public override void Changed (UITextView textView)
			{
				controller.item.Text = textView.Text;
				if (controller.textLengthLabel != null) {
					controller.textLengthLabel.TextLength =
						controller.service.GetTextLength (controller.item);
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

		abstract class AttachmentIcon : UIImageView
		{
			public static float Size { get { return 72; } }

			static readonly CGColor borderColor = new CGColor (0.75f, 0.75f, 0.75f);
			static readonly CGColor shadowColor = new CGColor (0.25f, 0.25f, 0.25f);

			public AttachmentIcon ()
				: base (new RectangleF (0, 0, Size, Size))
			{
				ContentMode = UIViewContentMode.ScaleAspectFill;
				ClipsToBounds = true;
				AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin;

				Layer.CornerRadius = 4;
				Layer.ShadowOffset = new SizeF (0, 0);
				Layer.ShadowColor = shadowColor;
				Layer.ShadowRadius = 4;
				Layer.ShadowOpacity = 1.0f;
				Layer.BorderColor = borderColor;
				Layer.BorderWidth = 1;
			}
		}

		class ImageIcon : AttachmentIcon
		{
			public ImageIcon (UIImage image)
			{
				Image = image;
			}
		}

		abstract class Field : UIView
		{
			public ShareViewController Controller { get; private set; }
			public UILabel TitleLabel { get; private set; }

			public Field (RectangleF frame, ShareViewController controller, string title)
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

			public ChoiceField (RectangleF frame, ShareViewController controller, string title, IEnumerable<string> items)
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



