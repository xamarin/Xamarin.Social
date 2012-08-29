using System;
using MonoTouch.Twitter;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using System.Threading;
using MonoTouch.UIKit;
using System.IO;
using System.Collections.Generic;
using MonoTouch.Accounts;

namespace Xamarin.Social.Services
{
	class TwitterService5 : TwitterService
	{
		public TwitterService5 ()
		{
		}


		#region Share

		public override UIViewController GetShareUI (Item item, Action<ShareResult> completionHandler)
		{
			//
			// Get the native UI
			//
			var vc = new TWTweetComposeViewController ();

			vc.SetCompletionHandler (result => {
				var shareResult = result == TWTweetComposeViewControllerResult.Done ? ShareResult.Done : ShareResult.Cancelled;
				completionHandler (shareResult);
			});

			vc.SetInitialText (item.Text);

			foreach (var image in item.Images) {
				vc.AddImage (image.Image);
			}

			foreach (var link in item.Links) {
				vc.AddUrl (new NSUrl (link.AbsoluteUri));
			}

			return vc;
		}

		public override Task ShareItemAsync (Item item, Account account, CancellationToken cancellationToken)
		{
			throw new NotSupportedException ("Sharing items without a GUI is not supported. Please use GetShareUI instead.");
		}

		#endregion


		#region Low-level Requests

		class TwitterRequest : Request
		{
			TWRequest request;

			public TwitterRequest (string method, Uri url, IDictionary<string, string> paramters, Account account)
				: base (method, url, paramters, account)
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

				Account = account;
			}

			public override Account Account {
				get {
					return request != null ? new ACAccountWrapper (request.Account) : null;
				}
				set {
					if (request != null) {
						if (value == null) {
							// Don't do anything, not supported
						}
						else if (value is ACAccountWrapper) {
							request.Account = ((ACAccountWrapper)value).ACAccount;
						}
						else {
							throw new NotSupportedException ("Account type '" + value.GetType().FullName + "'not supported");
						}
					}
				}
			}

			public override void AddMultipartData (string name, System.IO.Stream data, string mimeType, string filename)
			{
				request.AddMultiPartData (NSData.FromStream (data), name, mimeType);
			}

			public override Task<Response> GetResponseAsync (CancellationToken cancellationToken)
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
				}, TaskCreationOptions.LongRunning, cancellationToken);
			}
		}

		public override Request CreateRequest (string method, Uri url, IDictionary<string, string> paramters, Account account)
		{
			return new TwitterRequest (method, url, paramters, account);
		}

		#endregion


		#region Authentication

		ACAccountStore accountStore; // Save this reference since ACAccounts are only good so long as it's alive

		public override Task<List<Account>> GetAccountsAsync ()
		{
			if (accountStore == null) {
				accountStore = new ACAccountStore ();
			}
			var store = new ACAccountStore ();
			var at = store.FindAccountType (ACAccountType.Twitter);

			var r = new List<Account> ();

			var completedEvent = new ManualResetEvent (false);

			store.RequestAccess (at, (granted, error) => {
				if (granted) {
					var accounts = store.FindAccounts (at);
					foreach (var a in accounts) {
						r.Add (new ACAccountWrapper (a));
					}
				}
				completedEvent.Set ();
			});

			return Task.Factory.StartNew (delegate {
				completedEvent.WaitOne ();
				return r;
			}, TaskCreationOptions.LongRunning);
		}

		protected override Authenticator GetAuthenticator ()
		{
			throw new NotSupportedException ("Only OS accounts are supported");
		}

		#endregion
	}
}

