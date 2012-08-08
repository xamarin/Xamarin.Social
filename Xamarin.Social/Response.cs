using System;
using System.Collections.Generic;
using System.IO;

namespace Xamarin.Social
{
	/// <summary>
	/// An HTTP Response.
	/// </summary>
	public abstract class Response
	{
		public abstract int StatusCode { get; }
		public abstract IDictionary<string, string> Headers { get; }
		public abstract Stream GetResponseStream ();

		public Response ()
		{
		}
	}
}

