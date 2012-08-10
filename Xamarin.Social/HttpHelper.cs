using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Xamarin.Social
{
	public class HttpHelper
	{
		CookieContainer cookies;

		public HttpHelper ()
			: this (new CookieContainer ())
		{
		}

		public HttpHelper (CookieContainer cookies)
		{
			this.cookies = cookies;
		}

		public HttpWebRequest CreateHttpWebRequest (string method, string url)
		{
			var req = (HttpWebRequest)WebRequest.Create (url);
			req.Method = method;
			req.CookieContainer = cookies;
			return req;
		}

		public Task<HttpWebResponse> PostUrlFormEncodedAsync (string url, IDictionary<string, string> inputs)
		{
			var req = CreateHttpWebRequest ("GET", url);

			var sb = new StringBuilder ();
			var head = "";
			foreach (var p in inputs) {
				sb.Append (head);
				sb.Append (Uri.EscapeDataString (p.Key));
				sb.Append ("=");
				sb.Append (Uri.EscapeDataString (p.Value));
				head = "&";
			}
			var body = sb.ToString ();
			var bodyData = System.Text.Encoding.UTF8.GetBytes (body);
			req.ContentLength = bodyData.Length;
			req.ContentType = "application/x-www-form-urlencoded";

			return Task.Factory
				.FromAsync<Stream> (req.BeginGetRequestStream, req.EndGetRequestStream, null)
				.ContinueWith (ts => {
					using (ts.Result) {
						ts.Result.Write (bodyData, 0, bodyData.Length);
					}
					return (HttpWebResponse)Task.Factory
						.FromAsync<WebResponse> (req.BeginGetResponse, req.EndGetResponse, null)
						.Result;
				});
		}

		public string ReadResponseText (HttpWebResponse response)
		{
			var encoding = Encoding.UTF8;
			//response.ContentEncoding;

			using (var s = response.GetResponseStream ()) {
				using (var r = new StreamReader (s, encoding)) {
					return r.ReadToEnd ();
				}
			}
		}
	}
}

