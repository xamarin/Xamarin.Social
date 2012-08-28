using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using System.Text;

namespace Xamarin.Social
{
	public class FlickrService : OAuth1Service
	{
		public FlickrService ()
			: base ("Flickr", "Flickr")
		{
			CreateAccountLink = new Uri ("http://www.flickr.com");
			CanShareImages = true;
			CanShareText = true;
			CanShareLinks = true;

			RequestTokenUrl = new Uri ("http://www.flickr.com/services/oauth/request_token");
			AuthorizeUrl = new Uri ("http://www.flickr.com/services/oauth/authorize");
			AccessTokenUrl = new Uri ("http://www.flickr.com/services/oauth/access_token");
		}

		protected override Task ShareItemAsync (Item item, Account account, CancellationToken cancellationToken)
		{
			var req = CreateRequest (
				"POST",
				new Uri ("http://www.flickr.com/services/upload/"),
				account);

			//
			// Add the image
			//
			req.AddMultipartData ("photo", item.Images.First ());

			//
			// Make the description include links
			//
			var sb = new StringBuilder ();
			sb.Append (item.Text);
			if (item.Links.Count > 0) {
				sb.AppendLine ();
				sb.AppendLine ();
				foreach (var l in item.Links) {
					sb.AppendFormat ("<a href=\"{0}\">{0}</a>", HttpEx.HtmlEncode (l.AbsoluteUri));
					sb.AppendLine ();
				}
			}
			req.AddMultipartData ("description", sb.ToString ());

			return req.GetResponseAsync (cancellationToken).ContinueWith (reqTask => {

				var content = reqTask.Result.GetResponseText ();

				var doc = new XmlDocument ();
				doc.LoadXml (content);
				var stat = doc.DocumentElement.GetAttribute ("stat");

				if (stat == "fail") {
					var err = doc.DocumentElement.GetElementsByTagName ("err").OfType<XmlElement> ().FirstOrDefault ();
					if (err != null) {
						throw new ApplicationException (err.GetAttribute ("msg"));
					}
					else {
						throw new ApplicationException ("Flickr returned an unknown error.");
					}
				}

			}, cancellationToken);
		}
	}
}

