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

		/// <summary>
		/// Gets the response URI.
		/// </summary>
		public virtual Uri ResponseUri { get; private set; }

		/// <summary>
		/// Gets the response status code.
		/// </summary>
		public virtual HttpStatusCode StatusCode { get; private set; }

		/// <summary>
		/// Gets the headers returned with this response.
		/// </summary>
		public virtual IDictionary<string, string> Headers { get; private set; }

		/// <summary>
		/// Initializes a new <see cref="Xamarin.Social.Response"/> that wraps a <see cref="System.Net.HttpWebResponse"/>.
		/// </summary>
		/// <param name='response'>
		/// The System.Net response that this response will wrap.
		/// </param>
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

		/// <summary>
		/// Initializes a new blank <see cref="Xamarin.Social.Response"/>.
		/// </summary>
		protected Response ()
		{
		}

		/// <summary>
		/// Reads all the response data and interprets it as a string.
		/// </summary>
		/// <returns>
		/// The response text.
		/// </returns>
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

		/// <summary>
		/// Gets the response stream.
		/// </summary>
		/// <returns>
		/// The response stream.
		/// </returns>
		public virtual Stream GetResponseStream ()
		{
			return response.GetResponseStream ();
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Xamarin.Social.Response"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current <see cref="Xamarin.Social.Response"/>.
		/// </returns>
		public override string ToString ()
		{
			return string.Format ("{0} {1}", StatusCode, ResponseUri);
		}
	}
}

