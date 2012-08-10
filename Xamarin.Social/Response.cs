using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Xamarin.Social
{
	/// <summary>
	/// An HTTP Response.
	/// </summary>
	public class Response
	{
		HttpWebResponse response;

		public virtual HttpStatusCode StatusCode { get; private set; }
		public virtual IDictionary<string, string> Headers { get; private set; }


		public Response (HttpWebResponse response)
		{
			this.response = response;

			StatusCode = response.StatusCode;
			Headers = new Dictionary<string, string> ();

		}

		public Response ()
		{
		}

		public virtual Stream GetResponseStream ()
		{
			return response.GetResponseStream ();
		}
	}
}

