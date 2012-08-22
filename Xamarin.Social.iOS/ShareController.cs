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
		UITextView TextEditor;

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
			TextEditor = new UITextView (new RectangleF (0, b.Y, b.Width, b.Height)) {
				Font = TextEditorFont,
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
				Text = viewModel.Item.Text,
			};
			View.AddSubview (TextEditor);
			TextEditor.BecomeFirstResponder ();

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

					viewModel.Item.Text = TextEditor.Text;

					if (viewModel.Accounts.Count > 1 && afield != null) {
						viewModel.UseAccount = viewModel.Accounts.First (x => x.Username == afield.SelectedItem);
					}

					viewModel.ShareAsync ().ContinueWith (shareTask => {

						sharing = false;

						if (shareTask.IsFaulted) {
							ShowError (shareTask.Exception);
						}
						else {
							ParentViewController.DismissModalViewControllerAnimated (true);
						}

					}, TaskScheduler.FromCurrentSynchronizationContext ());
				}

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

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return true;
		}

		void ResignFirstResponders ()
		{
			TextEditor.ResignFirstResponder ();
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
			List<string> items;
			string selectedItem;

			public string SelectedItem {
				get { return selectedItem; }
				set {
					if (selectedItem != value) {
						selectedItem = value;
						ValueLabel.Text = selectedItem;
					}
				}
			}

			public LabelButton ValueLabel { get; private set; }
			public UIPickerView picker { get; private set; }

			public ChoiceField (RectangleF frame, ShareController controller, string title, IEnumerable<string> items)
				: base (frame, controller, title)
			{
				this.items = items.ToList ();
				selectedItem = this.items.First ();

				ValueLabel = new LabelButton () {
					BackgroundColor = UIColor.White,
					Font = TextEditorFont,
					Text = selectedItem,
					TextColor = UIColor.DarkTextColor,
					AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
				};				
				var tf = TitleLabel.Frame;
				ValueLabel.Frame = new RectangleF (tf.Right, 0, frame.Width - tf.Right, frame.Height - 1);

				ValueLabel.TouchUpInside += HandleTouchUpInside;


				AddSubview (ValueLabel);

				picker = new UIPickerView (new RectangleF (0, 0, 320, 216));
				picker.DataSource = new PickerDataSource (this);
				picker.Delegate = new PickerDelegate (this);
				picker.Hidden = true;
				controller.View.AddSubview (picker);
			}

			void HandleTouchUpInside (object sender, EventArgs e)
			{
				Controller.ResignFirstResponders ();

				var v = Controller.View;

				picker.Hidden = false;
				picker.Frame = new RectangleF (0, v.Bounds.Bottom - 216, 320, 216);
				v.BringSubviewToFront (picker);
			}

			class PickerDelegate : UIPickerViewDelegate
			{
				ChoiceField field;

				public PickerDelegate (ChoiceField field)
				{
					this.field = field;
				}

				public override UIView GetView (UIPickerView pickerView, int row, int component, UIView view)
				{
					var label = view as PickerLabel;

					if (label == null) {
						label = new PickerLabel (new RectangleF (16, 0, pickerView.Bounds.Width - 32, 44));
					}

					var item = field.items [row];
					label.Text = item;
					label.IsSelected = item == field.selectedItem;

					return label;
				}

				public override void Selected (UIPickerView pickerView, int row, int component)
				{
					var n = pickerView.RowsInComponent (0);

					field.SelectedItem = field.items[row];

					for (var i = 0; i < n; i++) { 
						var label = (PickerLabel)pickerView.ViewFor (i, 0);
						label.IsSelected = field.selectedItem == label.Text;
					}
				}
			}

			class PickerDataSource : UIPickerViewDataSource
			{
				int numRows;

				public PickerDataSource (ChoiceField field)
				{
					numRows = field.items.Count;
				}

				public override int GetRowsInComponent (UIPickerView pickerView, int component)
				{
					return numRows;
				}
				public override int GetComponentCount (UIPickerView pickerView)
				{
					return 1;
				}
			}

			class PickerLabel : UIView
			{
				static UIFont LabelFont = UIFont.BoldSystemFontOfSize (22);

				UILabel label;
				UILabel checkLabel;

				public string Text {
					get { return label.Text; }
					set { label.Text = value; }
				}

				bool isSelected = false;
				public bool IsSelected {
					get { return isSelected; }
					set {
						if (isSelected != value) {
							isSelected = value;
							label.TextColor = isSelected ? FieldColor : UIColor.DarkTextColor;
							checkLabel.TextColor = label.TextColor;
							checkLabel.Text = isSelected ? "\x2714" : "";
						}
					}
				}

				public PickerLabel (RectangleF frame)
					: base (frame)
				{
					Opaque = false;
					BackgroundColor = UIColor.Clear;

					checkLabel = new UILabel (new RectangleF (8, 0, 28, 44)) {
						Font = LabelFont,
						BackgroundColor = UIColor.Clear,
					};
					AddSubview (checkLabel);

					label = new UILabel (new RectangleF (36, 0, frame.Width - 44, 44)) {
						Font = LabelFont,
						BackgroundColor = UIColor.Clear,
					};
					AddSubview (label);					
				}
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



