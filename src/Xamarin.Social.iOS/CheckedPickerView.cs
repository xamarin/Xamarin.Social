//
//  Copyright 2012-2013, Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

		private ObservableCollection<string> items;
		public IList<string> Items
		{
			get { return items ?? (Items = new string[0]); }
			set {
				if (items != null)
					items.CollectionChanged -= OnItemsChanged;

				items = new ObservableCollection<string> (value.ToList());
				items.CollectionChanged += OnItemsChanged;

				OnItemsChanged (this, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
			}
		}

		private void OnItemsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (selectedItem == null || !items.Contains (selectedItem))
				SelectedItem = items.FirstOrDefault();

			ReloadAllComponents();
		}

		public string SelectedItem {
			get { return selectedItem; }
			set {
				if (this.selectedItem == value)
					return;

				this.selectedItem = value;

				var ev = this.SelectedItemChanged;
				if (ev != null) {
					ev (this, EventArgs.Empty);
				}
			}
		}

		public event EventHandler SelectedItemChanged;

		public CheckedPickerView (RectangleF frame)
			: base (frame)
		{
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

