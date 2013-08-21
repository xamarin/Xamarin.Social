using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using MonoTouch.Foundation;
using Xamarin.Auth;

namespace Xamarin.Social
{
	class FoundationResponse : Response
	{
		NSData responseData;
		Dictionary<string, string> headers;
		int statusCode;

		public override HttpStatusCode StatusCode {
			get {
				return (HttpStatusCode)statusCode;
			}
		}

		public override System.Collections.Generic.IDictionary<string, string> Headers {
			get {
				return headers;
			}
		}

		public FoundationResponse (NSData responseData, NSHttpUrlResponse urlResponse)
		{
			this.responseData = responseData;
			statusCode = urlResponse.StatusCode;

			headers = new Dictionary<string, string> ();
			var hs = urlResponse.AllHeaderFields;
			foreach (var k in hs.Keys) {
				var o = hs.ObjectForKey (k);
				if (k == null || o == null)
					continue;

				var key = k.ToString ();
				if (key == null)
					continue;

				headers[key] = o.ToString ();
			}
		}

		public override Stream GetResponseStream ()
		{
			var mutableData = responseData as NSMutableData;
			if (mutableData != null) {
				unsafe {
					return new UnmanagedMemoryStream ((byte*)mutableData.Bytes, mutableData.Length);
				}
			} else {
				return responseData.AsStream ();
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (responseData != null) {
				responseData.Dispose ();
				responseData = null;
			}

			base.Dispose (disposing);
		}
	}
}

