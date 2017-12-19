//
//  Copyright 2012, Xamarin Inc.
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
using Android.Widget;
using Android.Graphics;
using Android.Views;
using Android.Content;

namespace Xamarin.Social
{
	class ToolbarView : TableLayout
	{
		static Color ToolbarColor = Color.Argb (0xFF, 0xDD, 0xDD, 0xDD);

		ProgressBar progress;
		Button sendButton;

		bool isProgressing = false;
		public bool IsProgressing {
			get { return isProgressing; }
			set {
				if (isProgressing != value) {
					if (isProgressing) {
						sendButton.Enabled = false;
						progress.Visibility = ViewStates.Visible;
					}
					else {
						sendButton.Enabled = true;
						progress.Visibility = ViewStates.Invisible;
					}
					isProgressing = value;
				}
			}
		}

		public event EventHandler Clicked;

		public ToolbarView (Context context, string title)
			: base (context)
		{
			var tlabel = new TextView (context) {
				Text = title,
				TextSize = 24,
				LayoutParameters = new TableRow.LayoutParams (TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent) {
					Column = 0,
					TopMargin = 4,
					BottomMargin = 0,
					LeftMargin = 8,
				},
			};
			tlabel.SetTextColor (Color.Black);

			progress = new ProgressBar (context) {
				Indeterminate = true,
				Visibility = ViewStates.Invisible,
				LayoutParameters = new TableRow.LayoutParams (TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent) {
					TopMargin = 2,
					RightMargin = 6,
					Column = 2,
				},
			};

			sendButton = new Button (context) {
				Text = "Send",
				TextSize = 20,
				Enabled = true,
				LayoutParameters = new TableRow.LayoutParams (TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent) {
					TopMargin = 2,
					BottomMargin = 2,
					RightMargin = 2,
					Column = 3,
				},
			};
			sendButton.Click += delegate {
				var ev = Clicked;
				if (ev != null) {
					ev (this, EventArgs.Empty);
				}
			};

			var toolbarRow = new TableRow (context) {
			};
			toolbarRow.AddView (tlabel);
			toolbarRow.AddView (progress);
			toolbarRow.AddView (sendButton);

			LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent) {
			};
			SetBackgroundColor (ToolbarColor);
			SetColumnStretchable (1, true);
			SetColumnShrinkable (1, true);

			AddView (toolbarRow);
		}
	}
}

