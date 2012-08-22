using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Xamarin.Social
{
	/// <summary>
	/// An HTTP Response.
	/// </summary>
	public class Response
	{
		HttpWebResponse response;

		public virtual Uri ResponseUri { get; private set; }
		public virtual HttpStatusCode StatusCode { get; private set; }
		public virtual IDictionary<string, string> Headers { get; private set; }

		public Response (HttpWebResponse response)
		{
			this.response = response;

			ResponseUri = response.ResponseUri;
			StatusCode = response.StatusCode;

			Headers = new Dictionary<string, string> ();
			foreach (string h in response.Headers) {
				Headers[h] = response.Headers[h];
			}
		}

		public Response ()
		{
		}

		public virtual string GetResponseText ()
		{
			var encoding = Encoding.UTF8;

			if (Headers.ContainsKey ("Content-Type")) {
				encoding = HttpEx.GetEncodingFromContentType (Headers ["Content-Type"]);
			}

			using (var s = GetResponseStream ()) {
				using (var r = new StreamReader (s, encoding)) {
					return r.ReadToEnd ();
				}
			}
		}

		public virtual Stream GetResponseStream ()
		{
			return response.GetResponseStream ();
		}

		public override string ToString ()
		{
			return string.Format ("{0} {1}", StatusCode, ResponseUri);
		}
	}
}

