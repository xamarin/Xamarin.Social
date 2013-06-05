using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using MonoTouch.Accounts;
using MonoTouch.Foundation;
using MonoTouch.Social;
using MonoTouch.UIKit;
using Xamarin.Auth;

namespace Xamarin.Social.Services
{
	/// <summary>
	/// A base service for Social.framework family of services.
	/// </summary>
	public abstract class SocialService : Service
	{
		private SLServiceKind kind;
		private NSString accountTypeIdentifier;

		public SocialService (string serviceId, string title, SLServiceKind kind, NSString accountTypeIdentifier)
			: base (serviceId, title)
		{
			this.kind = kind;
			this.accountTypeIdentifier = accountTypeIdentifier;
		}

		#region Share

		public override UIViewController GetShareUI (Item item, Action<ShareResult> completionHandler)
		{
			//
			// Get the native UI
			//
			var vc = SLComposeViewController.FromService (kind);

			vc.CompletionHandler = (result) => {
				var shareResult = result == SLComposeViewControllerResult.Done ? ShareResult.Done : ShareResult.Cancelled;
				completionHandler (shareResult);
			};

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

		class SocialRequest : Request
		{
			SLRequest request;

			public SocialRequest (SLServiceKind kind, string method, Uri url, IDictionary<string, string> parametrs, Account account)
				: base (method, url, parametrs, account)
			{
				var ps = new NSMutableDictionary ();
				if (parametrs != null) {
					foreach (var p in parametrs) {
						ps.SetValueForKey (new NSString (p.Value), new NSString (p.Key));
					}
				}

				var m = SLRequestMethod.Get;
				switch (method.ToLowerInvariant()) {
					case "get":
					m = SLRequestMethod.Get;
					break;
					case "post":
					m = SLRequestMethod.Post;
					break;
					case "delete":
					m = SLRequestMethod.Delete;
					break;
					default:
					throw new NotSupportedException ("Social framework does not support the HTTP method '" + method + "'");
				}

				request = SLRequest.Create (kind, m, new NSUrl (url.AbsoluteUri), ps);

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
				request.AddMultipartData (NSData.FromStream (data), name, mimeType, string.Empty);
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
			return new SocialRequest (kind, method, url, paramters, account);
		}

		#endregion


		#region Authentication

		Lazy<ACAccountStore> accountStore = new Lazy<ACAccountStore> (); // Save this reference since ACAccounts are only good so long as it's alive

		/// <summary>
		/// Gets the account store options passed to Social.framework.
		/// </summary>
		protected virtual AccountStoreOptions AccountStoreOptions {
			get { return null; }
		}

		public override Task<IEnumerable<Account>> GetAccountsAsync ()
		{
			var store = accountStore.Value;
			var at = store.FindAccountType (this.accountTypeIdentifier);
			var tcs = new TaskCompletionSource<IEnumerable<Account>> ();

			store.RequestAccess (at, AccountStoreOptions, (granted, error) => {
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

		public override Task<Account> ReauthorizeAsync (Account account)
		{
			if (account == null)
				throw new ArgumentNullException ("account");

			var store = accountStore.Value;
			var wrapper = account as ACAccountWrapper;

			if (wrapper == null)
				throw new ArgumentException ("account", "Account type '" + account.GetType ().FullName + "' is not supported.")

			return store.RenewCredentialsAsync (wrapper.ACAccount).ContinueWith (t => {
				switch (t.Result) {
				case ACAccountCredentialRenewResult.Renewed:
					return account;
				default:
					throw new Exception (string.Format ("Could not renew account: {0}", t.Result));
				}
			});
		}

		public override bool SupportsAuthentication
		{
			get {
				return false;
			}
		}

		public override bool SupportsReauthorization {
			get {
				return true;
			}
		}

		protected override Authenticator GetAuthenticator ()
		{
			throw new NotSupportedException ("This service does support authenticating users. You should direct them to the Settings application.");
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