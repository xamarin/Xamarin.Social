using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using MonoTouch.Accounts;
using MonoTouch.Foundation;
using MonoTouch.Twitter;
using MonoTouch.UIKit;
using Xamarin.Auth;

namespace Xamarin.Social.Services
{
	public class Twitter5Service : TwitterService
	{
		public Twitter5Service ()
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
					return base.Account;
				}
				set {
					base.Account = value; 

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
				var tcs = new TaskCompletionSource<Response> ();

				cancellationToken.Register (() => tcs.TrySetCanceled ());

				request.PerformRequest ((resposeData, urlResponse, err) => {
					Response result = null;
					try {
						if (err != null)
							throw new Exception (err.LocalizedDescription);

						result = new FoundationResponse (resposeData, urlResponse);
					} catch (Exception ex) {
						tcs.TrySetException (ex);
						return;
					}

					tcs.TrySetResult (result);
				});

				return tcs.Task;
			}
		}

		public override Request CreateRequest (string method, Uri url, IDictionary<string, string> paramters, Account account)
		{
			return new TwitterRequest (method, url, paramters, account);
		}

		#endregion


		#region Authentication

		ACAccountStore accountStore; // Save this reference since ACAccounts are only good so long as it's alive

		public override Task<IEnumerable<Account>> GetAccountsAsync ()
		{
			if (accountStore == null) {
				accountStore = new ACAccountStore ();
			}
			var store = new ACAccountStore ();
			var at = store.FindAccountType (ACAccountType.Twitter);

			var tcs = new TaskCompletionSource<IEnumerable<Account>> ();

			store.RequestAccess (at, (granted, error) => {
				if (granted) {
					var accounts = store.FindAccounts (at)
						.Select (a => new ACAccountWrapper (a, store))
						.ToList ();

					tcs.SetResult (accounts);
				} else {
					tcs.SetResult (new Account [0]);
				}
			});

			return tcs.Task;
		}

		public override bool SupportsAuthentication
		{
			get {
				return false;
			}
		}

		protected override Authenticator GetAuthenticator ()
		{
			throw new NotSupportedException ("Twitter5Service does support authenticating users. You should direct them to the Settings application.");
		}

		#endregion

		#region Account management

		public override bool SupportsSave {
			get {
				return false;
			}
		}

		public override bool SupportsDelete {
			get {
				return false;
			}
		}

		public override void SaveAccount (Account account)
		{
			throw new NotSupportedException ("Twitter5Service does support saving user accounts. You should direct them to the Settings application.");
		}

		public override void DeleteAccount (Account account)
		{
			throw new NotSupportedException ("Twitter5Service does support deleting user accounts. You should direct them to the Settings application.");
		}

		#endregion
	}
}

