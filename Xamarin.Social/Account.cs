using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Social
{
	/// <summary>
	/// An Account that reprents and authenticated user of a social network.
	/// </summary>
	public class Account
	{
		public virtual string Username { get; private set; }
		public virtual Dictionary<string, string> Properties { get; private set; }

		public Account ()
			: this (null, null)
		{
		}

		public Account (string username)
			: this (username, null)
		{
		}

		public Account (string username, IDictionary<string, string> properties)
		{
			Username = username;
			Properties = (properties == null) ?
				new Dictionary<string, string> () :
				new Dictionary<string, string> (properties);
		}

		public static string SerializeProperties (IDictionary<string, string> properties)
		{
			var sb = new StringBuilder ();
			var head = "";
			foreach (var p in properties) {
				sb.Append (head);
				sb.Append (Uri.EscapeDataString (p.Key));
				sb.Append ("=");
				sb.Append (Uri.EscapeDataString (p.Value));
				head = "&";
			}
			return sb.ToString ();
		}

		public static Dictionary<string, string> DeserializeProperties (string propertiesString)
		{
			var r = new Dictionary<string, string> ();
			var kvs = propertiesString.Split ('&');
			foreach (var p in kvs) {
				var kv = p.Split ('=');
				r[Uri.UnescapeDataString (kv[0])] = kv.Length > 1 ? Uri.UnescapeDataString (kv[1]) : "";
			}
			return r;
		}
	}
}

