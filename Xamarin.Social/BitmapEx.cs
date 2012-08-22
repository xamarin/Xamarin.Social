using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;

namespace Xamarin.Social
{
	public static class BitmapEx
	{
		public static ImageData GetImageData (this Bitmap image, string mimeType)
		{
			var jpegEncoder = ImageCodecInfo.GetImageEncoders ().First (x => x.MimeType == mimeType);
			
			var stream = new MemoryStream ();
			
			var ps = new EncoderParameters (1);
			ps.Param[0] = new EncoderParameter (Encoder.Quality, 100);
			
			image.Save (stream, jpegEncoder, ps);
			stream.Position = 0;
			
			return new ImageData (stream, mimeType);
		}

		public static ImageData GetJpegData (this Bitmap image)
		{
			return GetImageData (image, "image/jpeg");
		}
	}
}

