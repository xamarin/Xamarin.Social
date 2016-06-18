//
//  Copyright 2012-2013, Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;

namespace Xamarin.Social
{
	/// <summary>
	/// Represents a social moment such as status update or shared image.
	/// </summary>
	public class Item : IDisposable
	{
		/// <summary>
		/// Gets or sets textual content.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Initializes a new <see cref="Xamarin.Social.Item"/> with the given text.
		/// </summary>
		/// <param name='text'>
		/// The initial text of the new item.
		/// </param>
		public Item (string text)
		{
			Text = text ?? "";

			Links = new List<Uri> ();
			Images = new List<ImageData> ();
			Files = new List<FileData> ();
		}

		/// <summary>
		/// Initializes an empty <see cref="Xamarin.Social.Item"/>.
		/// </summary>
		public Item ()
			: this ("")
		{
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the <see cref="Xamarin.Social.Item"/> is
		/// reclaimed by garbage collection.
		/// </summary>
		~Item ()
		{
			Dispose (false);
		}

		/// <summary>
		/// Releases all resource used by the <see cref="Xamarin.Social.Item"/>.
		/// </summary>
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		/// <summary>
		/// Releases all resource used by the <see cref="Xamarin.Social.Item"/>.
		/// </summary>
		/// <param name='disposing'>
		/// If <c>true</c> then this was called from the Dispose method; otherwise, it was called from the finalizer.
		/// </param>
		protected virtual void Dispose (bool disposing)
		{
			if (!disposing)
				return;

			if (Images != null) {
				foreach (var i in Images) {
					i.Dispose();
				}

				Images.Clear();
			}

			if (Files != null) {
				foreach (var f in Files) {
					f.Dispose();
				}

				Files.Clear();
			}
		}

		/// <summary>
		/// Gets a value indicating whether this item has attachments.
		/// </summary>
		/// <value>
		/// <c>true</c> if this item has attachments; otherwise, <c>false</c>.
		/// </value>
		public bool HasAttachments {
			get {
				return Links.Count > 0 || Images.Count > 0 || Files.Count > 0;
			}
		}

		/// <summary>
		/// Gets or sets links attached to this item.
		/// </summary>
		public IList<Uri> Links { get; set; }

		/// <summary>
		/// Gets or sets images attached to this item.
		/// </summary>
		public IList<ImageData> Images { get; set; }

		/// <summary>
		/// Gets or sets files attached to this item.
		/// </summary>
		public IList<FileData> Files { get; set; }

        private Location location = null;
		/// <summary>
		/// Gets or sets the location associated with this item.
		/// </summary>
		public Location Location { 
			get {
				if (location == null) {
					location = new Location ();
				}
				return location;
			} 
			set {
				location = value;
			}
		}

#if SUPPORT_VIDEO
		/// <summary>
		/// Attached video.
		/// </summary>
		public IList<VideoData> Videos { get; set; }
#endif
	}


}

