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
using System.IO;

namespace Xamarin.Social
{
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
			#if __PORTABLE__
			#else
			Data = File.OpenRead (path);
			#endif
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

		public void AddToRequest (global::Xamarin.Auth.Request request, string name)
		{
			if (request == null) {
				throw new ArgumentNullException ("request");
			}
			if (string.IsNullOrWhiteSpace (name)) {
				throw new ArgumentException ("Must provide a name for the file in the request.", "name");
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
			if (!disposing)
				return;

			if (Data != null) {
				Data.Dispose ();
				Data = null;
			}
		}
	}
}

