using System;
using System.IO;

namespace Xamarin.Social
{
	/// <summary>
	/// Represents a file.
	/// </summary>
	/// <remarks>
	/// This object disposes of the Data property.
	/// </remarks>
	public class FileData : IDisposable
	{
		public Stream Data { get; private set; }
		public string Filename { get; private set; }
		public string MimeType { get; private set; }

		public FileData (Stream data, string filename, string mimeType = null)
		{
			if (data == null) {
				throw new ArgumentNullException ("data");
			}
			if (string.IsNullOrEmpty (filename)) {
				throw new ArgumentException ("filename is required", "filename");
			}

			Data = data;
			Filename = filename;
			MimeType = string.IsNullOrEmpty (mimeType) ? "application/octet-stream" : mimeType;
		}

		public FileData (string path)
		{
			Data = File.OpenRead (path);
			Filename = Path.GetFileName (path);
			MimeType = "application/octet-stream";
		}

		public static implicit operator FileData (string path)
		{
			return new FileData (path);
		}

		~FileData ()
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
			if (Data != null) {
				Data.Dispose ();
				Data = null;
			}
		}
	}
}

