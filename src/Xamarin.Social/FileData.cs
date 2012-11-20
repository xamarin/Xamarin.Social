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
		public Stream Data { get; protected set; }
		public string Filename { get; protected set; }
		public string MimeType { get; protected set; }

		protected FileData ()
		{
		}

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

		public long Length {
			get {
				try {
					return Data.Length;
				}
				catch (Exception) {
					return 0;
				}
			}
		}

		public static implicit operator FileData (string path)
		{
			return new FileData (path);
		}

		public void AddToRequest (Xamarin.Auth.Request request, string name)
		{
			if (request == null) {
				throw new ArgumentNullException ("request");
			}
			if (string.IsNullOrEmpty (name)) {
				throw new ArgumentException ("name", "Must provide a name for the file in the request.");
			}
			request.AddMultipartData (name, Data, MimeType, Filename);
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

