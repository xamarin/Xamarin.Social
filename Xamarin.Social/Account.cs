using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Xamarin.Social
{
	/// <summary>
	/// An Account that reprents and authenticated user of a social network.
	/// </summary>
	public class Account
	{
		public virtual string Username { get; set; }
		public virtual Dictionary<string, string> Properties { get; private set; }
		public virtual CookieContainer Cookies { get; private set; }

		public Account ()
			: this ("", null, null)
		{
		}

		public Account (string username)
			: this (username, null, null)
		{
		}

		public Account (string username, CookieContainer cookies)
			: this (username, null, cookies)
		{
		}

		public Account (string username, IDictionary<string, string> properties)
			: this (username, properties, null)
		{
		}

		public Account (string username, IDictionary<string, string> properties, CookieContainer cookies)
		{
			Username = username;
			Properties = (properties == null) ?
				new Dictionary<string, string> () :
				new Dictionary<string, string> (properties);
			Cookies = (cookies == null) ?
				new CookieContainer () :
				cookies;
		}

		/// <summary>
		/// Serialize this account into a string that can be deserialized.
		/// </summary>
		public string Serialize ()
		{
			var sb = new StringBuilder ();

			sb.Append ("__username__=");
			sb.Append (Uri.EscapeDataString (Username));

			foreach (var p in Properties) {
				sb.Append ("&");
				sb.Append (Uri.EscapeDataString (p.Key));
				sb.Append ("=");
				sb.Append (Uri.EscapeDataString (p.Value));
			}

			if (Cookies.Count > 0) {
				sb.Append ("&__cookies__=");
				sb.Append (Uri.EscapeDataString (SerializeCookies ()));
			}

			return sb.ToString ();
		}

		public static Account Deserialize (string serializedString)
		{
			var acct = new Account ();

			foreach (var p in serializedString.Split ('&')) {
				var kv = p.Split ('=');

				var key = Uri.UnescapeDataString (kv[0]);
				var val = kv.Length > 1 ? Uri.UnescapeDataString (kv[1]) : "";

				if (key == "__cookies__") {
					acct.Cookies = DeserializeCookies (val);
				}
				else if (key == "__username__") {
					acct.Username = val;
				}
				else {
					acct.Properties[key] = val;
				}
			}

			return acct;
		}

		string SerializeCookies ()
		{
			var f = new BinaryFormatter ();
			using (var s = new MemoryStream ()) {
				f.Serialize (s, Cookies);
				return Convert.ToBase64String (s.GetBuffer (), 0, (int)s.Length);
			}
		}

		static CookieContainer DeserializeCookies (string cookiesString)
		{
			var f = new BinaryFormatter ();
			using (var s = new MemoryStream (Convert.FromBase64String (cookiesString))) {
				return (CookieContainer)f.Deserialize (s);
			}
		}

		public override string ToString ()
		{
			return Username;
		}
	}
}

