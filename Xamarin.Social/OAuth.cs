using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Security.Cryptography;

namespace Xamarin.Social
{
	public class OAuth1
	{
		public static string GetBaseString (string method, Uri uri, IDictionary<string, string> parameters)
		{
			var baseBuilder = new StringBuilder ();
			baseBuilder.Append (method);
			baseBuilder.Append ("&");
			baseBuilder.Append (Uri.EscapeDataString (uri.AbsoluteUri));
			baseBuilder.Append ("&");
			var head = "";
			foreach (var key in parameters.Keys.OrderBy (x => x)) {
				var p = head + Uri.EscapeDataString (key) + "=" + Uri.EscapeDataString (parameters[key]);
				baseBuilder.Append (Uri.EscapeDataString (p));
				head = "&";
			}
			return baseBuilder.ToString ();
		}

		public static string GetSignature (string method, Uri uri, IDictionary<string, string> parameters, string consumerSecret, string tokenSecret)
		{
			var baseString = GetBaseString (method, uri, parameters);

			var key = Uri.EscapeDataString (consumerSecret) + "&" + Uri.EscapeDataString (tokenSecret);

			var hashAlgo = new HMACSHA1 (Encoding.ASCII.GetBytes (key));

			var hash = hashAlgo.ComputeHash (Encoding.ASCII.GetBytes (baseString));

			var sig = Convert.ToBase64String (hash);

			return sig;
		}
	}
}

