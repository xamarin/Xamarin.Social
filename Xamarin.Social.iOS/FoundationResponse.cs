using System;
using MonoTouch.Foundation;
using System.IO;
using System.Collections.Generic;

namespace Xamarin.Social
{
	class FoundationResponse : Response
	{
		Stream stream;
		Dictionary<string, string> headers;
		int statusCode;

		public override int StatusCode {
			get {
				return statusCode;
			}
		}

		public override System.Collections.Generic.IDictionary<string, string> Headers {
			get {
				return headers;
			}
		}

		public FoundationResponse (NSData responseData, NSHttpUrlResponse urlResponse)
		{
			statusCode = urlResponse.StatusCode;

			var mutableData = responseData as NSMutableData;
			if (mutableData != null) {
				unsafe {
					stream = new UnmanagedMemoryStream ((byte*)mutableData.Bytes, mutableData.Length);
				}
			}
			else {
				stream = responseData.AsStream ();
			}

			headers = new Dictionary<string, string> ();
			var hs = urlResponse.AllHeaderFields;
			foreach (var k in hs.Keys) {
				headers[k.ToString ()] = hs.ObjectForKey (k).ToString ();
			}
		}

		public override Stream GetResponseStream ()
		{
			return stream;
		}
	}
}

