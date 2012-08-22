using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

namespace Xamarin.Social
{
	/// <summary>
	/// An HTTP request that provides a convenient way to make requests to the social network.
	/// </summary>
	public class Request
	{
		HttpWebRequest request;

		public string Method { get; private set; }
		public Uri Url { get; private set; }

		public IDictionary<string, string> Parameters { get; private set; }

		public virtual Account Account { get; set; }

		public Request (string method, Uri url, IDictionary<string, string> parameters = null)
		{
			Method = method;
			Url = url;
			Parameters = parameters == null ? 
				new Dictionary<string, string> () :
				new Dictionary<string, string> (parameters);
		}

		class Part
		{
			public Stream Data;
			public string Name;
			public string MimeType;
			public string Filename;
		}

		List<Part> parts = new List<Part> ();

		public void AddMultipartData (string name, string data, string mimeType = "", string filename = "")
		{
			AddMultipartData (name, new MemoryStream (Encoding.UTF8.GetBytes (data)), mimeType, filename);
		}

		public virtual void AddMultipartData (string name, Stream data, string mimeType = "", string filename = "")
		{
			parts.Add (new Part {
				Data = data,
				Name = name,
				MimeType = mimeType,
				Filename = filename,
			});
		}

		/// <summary>
		/// Gets the response.
		/// </summary>
		/// <remarks>
		/// Service implementors should override this method to modify the PreparedWebRequest
		/// to authenticate it.
		/// </remarks>
		/// <returns>
		/// The response.
		/// </returns>
		public virtual Task<Response> GetResponseAsync ()
		{
			var request = GetPreparedWebRequest ();

			//
			// TODO: Add the multipart data
			//
			if (parts.Count > 0) {
				
				var boundary = "---------------------------" + new Random ().Next ();
				var boundaryBytes = Encoding.ASCII.GetBytes ("--" + boundary + "\r\n");
				
				request.ContentType = "multipart/form-data; boundary=" + boundary;
				
				using (var s = request.GetRequestStream ()) {
					foreach (var p in parts) {
						s.Write (boundaryBytes, 0, boundaryBytes.Length);

						//
						// Content-Disposition
						//
						var header = "Content-Disposition: form-data; name=\"" + p.Name + "\"";
						if (!string.IsNullOrEmpty (p.Filename)) {
							header += "; filename=\"" + p.Filename + "\"";
						}
						var headerBytes = Encoding.ASCII.GetBytes (header);
						s.Write (headerBytes, 0, headerBytes.Length);
						s.Write (CrLf, 0, CrLf.Length);

						//
						// Content-Type
						//
						if (!string.IsNullOrEmpty (p.MimeType)) {
							header = "Content-Type: " + p.MimeType;
							headerBytes = Encoding.ASCII.GetBytes (header);
							s.Write (headerBytes, 0, headerBytes.Length);
							s.Write (CrLf, 0, CrLf.Length);
						}

						s.Write (CrLf, 0, CrLf.Length);
						s.Write (CrLf, 0, CrLf.Length);

						//
						// Data
						//
						p.Data.CopyTo (s);
						s.Write (CrLf, 0, CrLf.Length);
					}

					//
					// End
					//
					s.Write (boundaryBytes, 0, boundaryBytes.Length);
					s.Write (DashDash, 0, DashDash.Length);
					s.Write (CrLf, 0, CrLf.Length);
				}
			}

			return Task.Factory
				.FromAsync<WebResponse> (request.BeginGetResponse, request.EndGetResponse, null)
				.ContinueWith (task => {
				return new Response ((HttpWebResponse)task.Result);
			});
		}

		static readonly byte[] CrLf = new byte[] { (byte)'\r', (byte)'\n' };
		static readonly byte[] DashDash = new byte[] { (byte)'-', (byte)'-' };

		/// <summary>
		/// Gets the prepared URL.
		/// </summary>
		/// <remarks>
		/// Service implementors should override this function and add any needed parameters
		/// from the Account to the URL before it is used to get the response.
		/// </remarks>
		/// <returns>
		/// The prepared URL.
		/// </returns>
		protected virtual Uri GetPreparedUrl ()
		{
			var url = Url.AbsoluteUri;
			var head = Url.AbsoluteUri.Contains ('?') ? "&" : "?";
			foreach (var p in Parameters) {
				url += head;
				url += Uri.EscapeDataString (p.Key);
				url += "=";
				url += Uri.EscapeDataString (p.Value);
				head = "&";
			}
			return new Uri (url);
		}

		/// <summary>
		/// Returns the HttpWebRequest that will be used for this Request. All properties
		/// should be set to their correct values before accessing this object.
		/// </summary>
		/// <remarks>
		/// Service implementors should modify the returned request to add whatever
		/// authentication data is needed before getting the response.
		/// </remarks>
		/// <returns>
		/// The prepared HTTP web request.
		/// </returns>
		protected virtual HttpWebRequest GetPreparedWebRequest ()
		{
			if (request == null) {
				request = (HttpWebRequest)WebRequest.Create (GetPreparedUrl ());
				request.Method = Method;
			}

			return request;
		}
	}
}

