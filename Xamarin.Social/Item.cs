using System;
using System.Collections.Generic;
using System.IO;

#if PLATFORM_IOS
using ImageType = MonoTouch.UIKit.UIImage;
#elif PLATFORM_ANDROID
using ImageType = Android.Graphics.Bitmap;
#elif PLATFORM_XAML
using ImageType = Windows.Media.BitmapSource;
#else
using ImageType = System.Drawing.Bitmap;
#endif

namespace Xamarin.Social
{
	/// <summary>
	/// Facebook Post. Twitter Status. Google+ Activity.
	/// </summary>
	public class Item
	{
		/// <summary>
		/// Textual content.
		/// </summary>
		public string Text { get; set; }

		public Item (string text)
		{
			Text = text ?? "";

			Links = new List<Uri> ();
			Images = new List<ImageType> ();
			Files = new List<File> ();
		}

		public Item ()
			: this ("")
		{
		}

		/// <summary>
		/// Attached link.
		/// </summary>
		public IList<Uri> Links { get; set; }

		/// <summary>
		/// Attached image.
		/// </summary>
		public IList<ImageType> Images { get; set; }

		/// <summary>
		/// Attached image.
		/// </summary>
		public IList<File> Files { get; set; }

#if SUPPORT_VIDEO
		/// <summary>
		/// Attached video.
		/// </summary>
		public IList<object> Videos { get; set; }
#endif
	}

	/// <summary>
	/// Represents a file.
	/// </summary>
	/// <remarks>
	/// The creator of this object is responsible for
	/// closing and disposing of the data stream.
	/// </remarks>
	public class File
	{
		public File (Stream data, string filename, string mimeType = null)
		{
			Data = data;
			Filename = filename;
			MimeType = string.IsNullOrEmpty (mimeType) ? "application/octet-stream" : mimeType;
		}

		public Stream Data { get; set; }
		public string Filename { get; set; }
		public string MimeType { get; set; }
	}
}

