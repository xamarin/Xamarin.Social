using System;
using System.IO;
using System.Linq;

#if PLATFORM_IOS
using MonoTouch.UIKit;
#elif PLATFORM_ANDROID
using Android.Graphics;
#else
using System.Drawing;
using System.Drawing.Imaging;
#endif

namespace Xamarin.Social
{
	/// <summary>
	/// Represents an image to be shared with a service.
	/// </summary>
	/// <remarks>
	/// This object disposes of the Data property.
	/// </remarks>
	public class ImageData : FileData
	{
		public ImageData (Stream data, string mimeType)
			: this (data, "image." + (mimeType == "image/jpeg" ? "jpg" : "png"), mimeType)
		{
		}

		public ImageData (Stream data, string filename, string mimeType)
			: base (data, filename, mimeType)
		{
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			Image = null;
		}

#if PLATFORM_IOS

		public UIImage Image { get; private set; }

		public ImageData (UIImage image)
			: this (image, "image.jpg")
		{
		}

		public ImageData (string path)
			: this (UIImage.FromFile (path), Path.GetFileName (path))
		{
		}

		public ImageData (UIImage image, string filename)
		{
			if (image == null) {
				throw new ArgumentNullException ("image");
			}
			if (string.IsNullOrEmpty (filename)) {
				throw new ArgumentException ("filename");
			}

			Image = image;
			Filename = filename;

			MimeType = (filename.ToLowerInvariant ().EndsWith (".png")) ?
				"image/png" : "image/jpeg";

			if (MimeType == "image/png") {
				Data = new NSDataStream (image.AsPNG ());
			}
			else {
				Data = new NSDataStream (image.AsJPEG ());
			}
		}

		public static implicit operator ImageData (UIImage image)
		{
			return new ImageData (image);
		}

		public static implicit operator ImageData (string path)
		{
			return new ImageData (path);
		}

#elif PLATFORM_ANDROID

		public Bitmap Image { get; private set; }
		
		public ImageData (Bitmap image)
			: this (image, "image.jpg")
		{
		}
		
		public ImageData (string path)
			: this (BitmapFactory.DecodeFile (path), System.IO.Path.GetFileName (path))
		{
		}

		public ImageData (Bitmap image, string filename)
		{
			if (image == null) {
				throw new ArgumentNullException ("image");
			}
			if (string.IsNullOrEmpty (filename)) {
				throw new ArgumentException ("filename");
			}

			Image = image;
			Filename = filename;

			var compressFormat = Bitmap.CompressFormat.Jpeg;
			MimeType = "image/jpeg";
			if (filename.ToLowerInvariant ().EndsWith (".png")) {
				MimeType = "image/png";
				compressFormat = Bitmap.CompressFormat.Png;
			}

			var stream = new MemoryStream ();			
			image.Compress (compressFormat, 100, stream);
			stream.Position = 0;
			
			Data = stream;
		}
		
		public static implicit operator ImageData (Bitmap image)
		{
			return new ImageData (image);
		}
		
		public static implicit operator ImageData (string path)
		{
			return new ImageData (path);
		}

#elif PLATFORM_XAML
#else

		public Bitmap Image { get; private set; }
		
		public ImageData (Bitmap image)
			: this (image, "image.jpg")
		{
		}
		
		public ImageData (string path)
			: this (new Bitmap (path), Path.GetFileName (path))
		{
		}
		
		public ImageData (Bitmap image, string filename)
		{
			Image = image;
			Filename = filename;
			
			MimeType = (filename.ToLowerInvariant ().EndsWith (".png")) ?
				"image/png" : "image/jpeg";
			
			var jpegEncoder = ImageCodecInfo.GetImageEncoders ().First (x => x.MimeType == MimeType);
			var ps = new EncoderParameters (1);
			ps.Param[0] = new EncoderParameter (Encoder.Quality, 100);
			
			var stream = new MemoryStream ();			
			image.Save (stream, jpegEncoder, ps);
			stream.Position = 0;
			
			Data = stream;
		}
		
		public static implicit operator ImageData (Bitmap image)
		{
			return new ImageData (image);
		}
		
		public static implicit operator ImageData (string path)
		{
			return new ImageData (path);
		}

#endif
	}
}

