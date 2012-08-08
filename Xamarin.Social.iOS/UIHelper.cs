using System;
using MonoTouch.UIKit;

namespace Xamarin.Social
{
	public static class UIHelper
	{
		public static UIViewController RootViewController {
			get {
				var wnd = NormalWindow;
				if (wnd != null) {
					return wnd.RootViewController;
				}
				else {
					return null;
				}
			}
		}

		public static UIWindow NormalWindow {
			get {
				var app = UIApplication.SharedApplication;
				var wnd = app.KeyWindow;
				if (wnd.WindowLevel == UIWindow.LevelNormal) {
					return wnd;
				}
				else {
					foreach (var w in app.Windows) {
						if (w.WindowLevel == UIWindow.LevelNormal) {
							return w;
						}
					}
				}
				return null;
			}
		}
	}
}

