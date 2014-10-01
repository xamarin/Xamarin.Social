//
//  Copyright 2012, Xamarin Inc.
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
using System.Collections.Generic;
using System.IO;
using System.Net;
#if __UNIFIED__
using UIKit;
using Foundation;
using CoreAnimation;
using CoreGraphics;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;

using System.Drawing;
using CGRect = global::System.Drawing.RectangleF;
using CGPoint = global::System.Drawing.PointF;
using CGSize = global::System.Drawing.SizeF;
using nfloat = global::System.Single;
using nint = global::System.Int32;
using nuint = global::System.UInt32;
#endif
using Xamarin.Auth;

namespace Xamarin.Social
{
	class FoundationResponse : Response
	{
		NSData data;
		Dictionary<string, string> headers;

		#if ! __UNIFIED__
		int statusCode;
		#else 
		nint statusCode;
		#endif

		public override HttpStatusCode StatusCode {
			get {
				#if ! __UNIFIED__
				return (HttpStatusCode)statusCode;
				#else
				return (HttpStatusCode) (int)statusCode;
				#endif
			}
		}

		public override System.Collections.Generic.IDictionary<string, string> Headers {
			get {
				return headers;
			}
		}

		public FoundationResponse (NSData responseData, NSHttpUrlResponse urlResponse)
		{
			data = responseData;
			statusCode = urlResponse.StatusCode;

			headers = new Dictionary<string, string> ();
			var hs = urlResponse.AllHeaderFields;
			foreach (var k in hs.Keys) {
				headers[k.ToString ()] = hs.ObjectForKey (k).ToString ();
			}
		}

		public override Stream GetResponseStream ()
		{
			var mutableData = data as NSMutableData;
			if (mutableData != null) {
				unsafe {
					#if ! __UNIFIED__
					return new UnmanagedMemoryStream ((byte*)mutableData.Bytes, mutableData.Length);
					#else
					return new UnmanagedMemoryStream ((byte*)mutableData.Bytes, (long) mutableData.Length);
					#endif
				}
			} else {
				return data.AsStream ();
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (data != null) {
					data.Dispose ();
					data = null;
				}
			}

			base.Dispose (disposing);
		}
	}
}

