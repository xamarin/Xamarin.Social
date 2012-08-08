using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Social
{
	/// <summary>
	/// An HTTP request that provides a convenient way to make requests to the social network.
	/// </summary>
	public abstract class Request
	{
		public string Method { get; private set; }
		public Uri Url { get; private set; }

		public IDictionary<string, string> Parameters { get; private set; }

		public virtual Account Account { get; set; }

		public Request (string method, Uri url)
		{
			Method = method;
			Parameters = new Dictionary<string, string> ();
		}

		public abstract void AddMultipartData (Stream data, string name, string mimeType, string filename);
		
		public abstract Task<Response> GetResponseAsync ();
	}
}

