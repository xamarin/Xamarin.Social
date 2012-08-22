using System;
using System.Collections.Generic;

namespace Xamarin.Social
{
	/// <summary>
	/// Facebook Post. Twitter Status. Google+ Activity.
	/// </summary>
	public class Item : IDisposable
	{
		/// <summary>
		/// Textual content.
		/// </summary>
		public string Text { get; set; }

		public Item (string text)
		{
			Text = text ?? "";

			Links = new List<Uri> ();
			Images = new List<ImageData> ();
			Files = new List<FileData> ();
		}

		public Item ()
			: this ("")
		{
		}

		~Item ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			foreach (var i in Images) {
				i.Dispose ();
			}
			Images.Clear ();

			foreach (var f in Files) {
				f.Dispose ();
			}
			Files.Clear ();
		}


		/// <summary>
		/// Attached link.
		/// </summary>
		public IList<Uri> Links { get; set; }

		/// <summary>
		/// Attached image.
		/// </summary>
		public IList<ImageData> Images { get; set; }

		/// <summary>
		/// Attached image.
		/// </summary>
		public IList<FileData> Files { get; set; }

#if SUPPORT_VIDEO
		/// <summary>
		/// Attached video.
		/// </summary>
		public IList<VideoData> Videos { get; set; }
#endif
	}


}

