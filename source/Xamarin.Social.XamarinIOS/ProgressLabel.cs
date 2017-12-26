using System;

#if __UNIFIED__
using UIKit;
using Foundation;
using CoreAnimation;
using CoreGraphics;

using System.Drawing;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;

using System.Drawing;
using CGRect = global::System.Drawing.RectangleF;
using CGPoint = global::System.Drawing.PointF;
using CGSize = global::System.Drawing.SizeF;
using nfloat = global::System.Single;
using nint = global::System.Int32;
using nuint = global::System.UInt32;
#endif

namespace Xamarin.Social
{
	internal class ProgressLabel : UIView
	{
		UIActivityIndicatorView activity;
		
		public ProgressLabel (string text)
			: base (new RectangleF (0, 0, 200, 44))
		{
			BackgroundColor = UIColor.Clear;
			
			activity = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.White) {
				Frame = new RectangleF (0, 11.5f, 21, 21),
				HidesWhenStopped = false,
				Hidden = false,
			};
			AddSubview (activity);
			
			var label = new UILabel () {
				Text = text,
				TextColor = UIColor.White,
				Font = UIFont.BoldSystemFontOfSize (20),
				BackgroundColor = UIColor.Clear,
				#if ! __UNIFIED__
				Frame = new RectangleF (25f, 0f, Frame.Width - 25, 44f),
				#else
				Frame = new RectangleF (25f, 0f, (float)(Frame.Width - 25), 44f),
				#endif
			};
			AddSubview (label);
			
			var f = Frame;
			f.Width = label.Frame.X + UIStringDrawing.StringSize (label.Text, label.Font).Width;
			Frame = f;
		}
		
		public void StartAnimating ()
		{
			activity.StartAnimating ();
		}
		
		public void StopAnimating ()
		{
			activity.StopAnimating ();
		}
	}
}

