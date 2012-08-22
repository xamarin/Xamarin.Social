using System;
using MonoTouch.UIKit;
using System.IO;
using MonoTouch.Foundation;

namespace Xamarin.Social
{
	public static class UIImageEx
	{
		class NSDataStream : UnmanagedMemoryStream
		{
			[Preserve]
			NSData data;

			public NSData Data { get { return data; } } // Just to ditch the warning

			public unsafe NSDataStream (NSData data, byte *bytes, uint length)
			{
				//
				// Need to make sure we hang onto the data object
				// so that the memory doesn't get freed
				//
				this.data = data;
			}
		}

		public static ImageData GetImageData (this UIImage image, string mimeType)
		{
			if (mimeType == "image/jpeg") {
				var data = image.AsJPEG ();
				unsafe {
					return new ImageData (new NSDataStream (data, (byte*)data.Bytes, data.Length), "image/jpeg");
				}
			}
			else if (mimeType == "image/png") {
				var data = image.AsPNG ();
				unsafe {
					return new ImageData (new NSDataStream (data, (byte*)data.Bytes, data.Length), "image/png");
				}
			}
			else {
				throw new NotSupportedException (mimeType + " not supported");
			}
		}
	}
}

