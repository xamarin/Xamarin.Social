using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace Xamarin.Social
{
	public static class HttpEx
	{
		public static string GetCookie (this CookieContainer containers, Uri domain, string name)
		{
			var c = containers
					.GetCookies (domain)
					.Cast<Cookie> ()
					.FirstOrDefault (x => x.Name == name);
			return c != null ? c.Value : "";
		}
	}


	public class HttpHelper
	{
		public CookieContainer Cookies { get; private set; }

		public HttpHelper ()
			: this (new CookieContainer ())
		{
		}

		public HttpHelper (CookieContainer cookies)
		{
			this.Cookies = cookies;
		}

		public HttpWebRequest CreateHttpWebRequest (string method, string url)
		{
			var req = (HttpWebRequest)WebRequest.Create (url);
			req.Method = method;
			req.CookieContainer = Cookies;
			return req;
		}

		public Task<WebResponse> GetAsync (string url)
		{
			var req = CreateHttpWebRequest ("GET", url);
			return Task.Factory
				.FromAsync<WebResponse> (req.BeginGetResponse, req.EndGetResponse, null);
		}

		public Task<WebResponse> PostUrlFormEncodedAsync (string url, IDictionary<string, string> inputs)
		{
			var req = CreateHttpWebRequest ("POST", url);

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
					return Task.Factory
						.FromAsync<WebResponse> (req.BeginGetResponse, req.EndGetResponse, null)
						.Result;
				});
		}

		public static string ReadResponseText (WebResponse response)
		{
			var httpResponse = response as HttpWebResponse;

			var encoding = Encoding.UTF8;

			if (httpResponse != null) {
				//encoding = response.ContentEncoding;
			}

			using (var s = response.GetResponseStream ()) {
				using (var r = new StreamReader (s, encoding)) {
					return r.ReadToEnd ();
				}
			}
		}
	}
}

