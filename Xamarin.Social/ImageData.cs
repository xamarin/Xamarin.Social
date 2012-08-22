using System;
using System.IO;

namespace Xamarin.Social
{
	public class ImageData
	{
		public Stream Stream { get; private set; }
		public string MimeType { get; private set; }
		public string Filename { get; private set; }

		public ImageData (Stream stream, string mimeType)
			: this (stream, mimeType, "image." + (mimeType == "image/jpeg" ? "jpg" : "png"))
		{
		}

		public ImageData (Stream stream, string mimeType, string filename)
		{
			Stream = stream;
			MimeType = mimeType;
			Filename = filename;
		}
	}
}

