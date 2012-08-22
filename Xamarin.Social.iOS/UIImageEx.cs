using System;
using MonoTouch.UIKit;
using System.IO;
using MonoTouch.Foundation;
using System.Runtime.InteropServices;

namespace Xamarin.Social
{
	public static class UIImageEx
	{
		class NSDataStream : Stream
		{
			NSData data;
			uint pos;

			public NSDataStream (NSData data)
			{
				this.data = data;
			}

			protected override void Dispose (bool disposing)
			{
				if (data != null) {
					data.Dispose ();
					data = null;
				}
			}

			public override void Flush ()
			{
			}

			public override int Read (byte[] buffer, int offset, int count)
			{
				if (pos >= data.Length) {
					return 0;
				}
				else {
					var len = (int)Math.Min (count, data.Length - pos);
					Marshal.Copy (new IntPtr (data.Bytes.ToInt64 () + pos), buffer, offset, len);
					pos += (uint)len;
					return len;
				}
			}

			public override long Seek (long offset, SeekOrigin origin)
			{
				throw new NotSupportedException ();
			}

			public override void SetLength (long value)
			{
				throw new NotSupportedException ();
			}

			public override void Write (byte[] buffer, int offset, int count)
			{
				throw new NotSupportedException ();
			}

			public override bool CanRead {
				get {
					return true;
				}
			}

			public override bool CanSeek {
				get {
					return false;
				}
			}

			public override bool CanWrite {
				get {
					return false;
				}
			}

			public override long Length {
				get {
					return data.Length;
				}
			}

			public override long Position {
				get {
					return pos;
				}
				set {
				}
			}
		}

		public static ImageData GetImageData (this UIImage image, string mimeType)
		{
			if (mimeType == "image/jpeg") {
				var data = image.AsJPEG ();
				unsafe {
					return new ImageData (new NSDataStream (data), "image/jpeg");
				}
			}
			else if (mimeType == "image/png") {
				var data = image.AsPNG ();
				unsafe {
					return new ImageData (new NSDataStream (data), "image/png");
				}
			}
			else {
				throw new NotSupportedException (mimeType + " not supported");
			}
		}
	}
}

