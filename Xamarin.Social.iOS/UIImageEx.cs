using System;
using MonoTouch.UIKit;

namespace Xamarin.Social
{
	public static class UIImageEx
	{
		public static ImageData GetImageData (this UIImage image)
		{
			throw new NotSupportedException ("Cannot convert from UIImage to ImageData");
		}
	}
}

