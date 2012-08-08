using System;
using MonoTouch.Twitter;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using System.Threading;
using MonoTouch.UIKit;
using System.IO;
using System.Collections.Generic;

namespace Xamarin.Social
{
	class TwitterService5 : TwitterService
	{
		public TwitterService5 ()
		{
		}


		#region Share

		public override Task<ShareResult> ShareAsync (Item item, Account account = null)
		{
			if (OSAccount == null) {
				throw new InvalidOperationException ("No OS Account available for Twitter");
			}

			if (item.Files.Count > 0) {
				throw new NotSupportedException ("Twitter does not support uploading arbitrary files.");
			}

			var rootVC = UIHelper.RootViewController;
			if (rootVC == null) {

			}

			var completedEvent = new ManualResetEvent (false);
			var shareResult = ShareResult.Done;

			var vc = new TWTweetComposeViewController ();
			vc.SetCompletionHandler (result => {

				shareResult = result == TWTweetComposeViewControllerResult.Done ? ShareResult.Done : ShareResult.Cancelled;
				completedEvent.Set ();

			});

			vc.SetInitialText (item.Text);

			foreach (var image in item.Images) {
				vc.AddImage (image);
			}

			foreach (var link in item.Links) {
				vc.AddUrl (new NSUrl (link.AbsoluteUri));
			}


			rootVC.PresentModalViewController (vc, true);

			return Task.Factory.StartNew (delegate {
				completedEvent.WaitOne ();
				return shareResult;
			}, TaskCreationOptions.LongRunning);
		}

		#endregion


		#region Low-level Requests

		class TwitterRequest : Request
		{
			TWRequest request;

			public TwitterRequest (string method, Uri url, IDictionary<string, string> paramters)
				: base (method, url)
			{
				var ps = new NSMutableDictionary ();
				if (paramters != null) {
					foreach (var p in paramters) {
						ps.SetValueForKey (new NSString (p.Value), new NSString (p.Key));
					}
				}

				var m = TWRequestMethod.Get;
				switch (method.ToLowerInvariant()) {
				case "get":
					m = TWRequestMethod.Get;
					break;
				case "post":
					m = TWRequestMethod.Post;
					break;
				case "delete":
					m = TWRequestMethod.Delete;
					break;
				default:
					throw new NotSupportedException ("Twitter does not support the HTTP method '" + method + "'");
				}

				request = new TWRequest (new NSUrl (url.AbsoluteUri), ps, m);
			}

			public override Account Account {
				get {
					return ACAccountWrapper.GetForAccount (request.Account);
				}
				set {
					if (value == null) {
						request.Account = null;
					}
					else if (value is ACAccountWrapper) {
						request.Account = ((ACAccountWrapper)value).ACAccount;
					}
					else {
						throw new NotSupportedException ("Account type '" + value.GetType().FullName + "'not supported");
					}
				}
			}

			public override void AddMultipartData (System.IO.Stream data, string name, string mimeType, string filename)
			{
				request.AddMultiPartData (NSData.FromStream (data), name, mimeType);
			}

			public override Task<Response> GetResponseAsync ()
			{
				var completedEvent = new ManualResetEvent (false);

				NSError error = null;
				Response response = null;

				request.PerformRequest ((resposeData, urlResponse, err) => {
					error = err;
					response = new FoundationResponse (resposeData, urlResponse);
					completedEvent.Set ();
				});

				return Task.Factory.StartNew (delegate {
					completedEvent.WaitOne ();
					if (error != null) {
						throw new Exception (error.LocalizedDescription);
					}
					return response;
				}, TaskCreationOptions.LongRunning);
			}
		}

		public override Request CreateRequest (string method, Uri url, IDictionary<string, string> paramters = null)
		{
			return new TwitterRequest (method, url, paramters);
		}

		#endregion


		#region Authentication

		class Account5 : Account {
		}

		Account5 account = null;

		public override Account OSAccount {
			get {
				if (TWTweetComposeViewController.CanSendTweet) {
					if (account == null) {
						account = new Account5 ();
					}
				}
				else {
					account = null;
				}
				return account;
			}
		}

		public override Task<Account> AuthenticateAsync (AccountCredential credential)
		{
			throw new NotSupportedException ("Only OS accounts are supported");
		}

		public override AccountCredential GetBlankCredential ()
		{
			throw new NotSupportedException ("Only OS accounts are supported");
		}

		#endregion
	}
}

