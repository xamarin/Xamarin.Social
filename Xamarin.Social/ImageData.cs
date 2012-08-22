using System;
using System.IO;

namespace Xamarin.Social
{
	public class ImageData
	{
		public Stream Stream { get; set; }
		public string MimeType { get; set; }
		public string Filename { get; set; }

		public ImageData ()
		{
		}
	}
}

