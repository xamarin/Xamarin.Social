using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Social
{
	class CheckedPickerView : UIPickerView
	{
		static readonly UIColor FieldColor = UIColor.FromRGB (56, 84, 135);

		string selectedItem;

		public IList<string> Items { get; set; }
		
		public string SelectedItem {
			get { return selectedItem; }
			set {
				if (selectedItem != value) {
					selectedItem = value;

					var ev = SelectedItemChanged;
					if (ev != null) {
						ev (this, EventArgs.Empty);
					}
				}
			}
		}

		public event EventHandler SelectedItemChanged;

		public CheckedPickerView (RectangleF frame, IEnumerable<string> items)
			: base (frame)
		{
			Items = items.ToList ();
			selectedItem = items.FirstOrDefault ();

			Delegate = new PickerDelegate ();
			DataSource = new PickerDataSource ();
		}

		class PickerDelegate : UIPickerViewDelegate
		{
			public override UIView GetView (UIPickerView pickerView, int row, int component, UIView view)
			{
				var label = view as PickerLabel;
				
				if (label == null) {
					label = new PickerLabel (new RectangleF (16, 0, pickerView.Bounds.Width - 32, 44));
				}

				var cpv = (CheckedPickerView)pickerView;

				var item = cpv.Items [row];
				label.Text = item;
				label.IsSelected = item == cpv.selectedItem;
				
				return label;
			}
			
			public override void Selected (UIPickerView pickerView, int row, int component)
			{
				var n = pickerView.RowsInComponent (0);

				var cpv = (CheckedPickerView)pickerView;

				cpv.SelectedItem = cpv.Items[row];
				
				for (var i = 0; i < n; i++) { 
					var label = (PickerLabel)pickerView.ViewFor (i, 0);
					label.IsSelected = cpv.selectedItem == label.Text;
				}
			}
		}
		
		class PickerDataSource : UIPickerViewDataSource
		{
			public override int GetRowsInComponent (UIPickerView pickerView, int component)
			{
				var cpv = (CheckedPickerView)pickerView;
				return cpv.Items.Count;
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
}

