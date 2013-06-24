using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Utilities;

namespace Xamarin.Social.Services
{
	public class DropboxService : OAuth1PreAService
	{
		public string Root { get; set; }

		public DropboxService ()
			: base ("Dropbox", "Dropbox")
		{
			CreateAccountLink = new Uri ("http://www.flickr.com");

			ShareTitle = "Upload";

			Root = "sandbox";

			RequestTokenUrl = new Uri ("https://api.dropbox.com/1/oauth/request_token");
			AuthorizeUrl = new Uri ("https://www.dropbox.com/1/oauth/authorize");
			AccessTokenUrl = new Uri ("https://api.dropbox.com/1/oauth/access_token");
		}

		protected override Task<string> GetUsernameAsync (IDictionary<string, string> accountProperties)
		{
			var request = base.CreateRequest ("GET",
			                                  new Uri ("https://api.dropbox.com/1/account/info"),
			new Account (string.Empty, accountProperties));

			return request.GetResponseAsync ().ContinueWith (reqTask => {
				var responseText = reqTask.Result.GetResponseText ();
				return WebEx.GetValueFromJson (responseText, "display_name");
			});
		}

		public override Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters, Account account)
		{
			return new OAuth1Request (method, url, parameters, account, true);
		}
	}
}