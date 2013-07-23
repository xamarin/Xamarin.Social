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
using MonoTouch.Foundation;
using Xamarin.Auth;

namespace Xamarin.Social
{
	class FoundationResponse : Response
	{
		Stream stream;
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

