using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;
using Xamarin.Auth;

#if PLATFORM_IOS
using ShareUIType = MonoTouch.UIKit.UIViewController;
using AuthenticateUIType = MonoTouch.UIKit.UIViewController;
#elif PLATFORM_ANDROID
using ShareUIType = Android.Content.Intent;
using AuthenticateUIType = Android.Content.Intent;
using UIContext = Android.App.Activity;
#else
using ShareUIType = System.Object;
using AuthenticateUIType = System.Object;
#endif

namespace Xamarin.Social
{
	/// <summary>
	/// Social Networking Service.
	/// </summary>
	public abstract class Service
	{
		/// <summary>
		/// Uniquely identifies this service type.
		/// </summary>
		public string ServiceId { get; private set; }

		/// <summary>
		/// Text used to label this service in the UI.
		/// </summary>
		public string Title { get; private set; }

		/// <summary>
		/// Text used as the title of screen when editing an item.
		/// </summary>
		public string ShareTitle { get; protected set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Xamarin.Social.Service"/> class.
		/// </summary>
		/// <param name='serviceId'>
		/// Service identifier used when storing accounts.
		/// </param>
		/// <param name='title'>
		/// Title used when displaying its name in UI.
		/// </param>
		protected Service (string serviceId, string title)
		{
			if (string.IsNullOrWhiteSpace (serviceId)) {
				throw new ArgumentException ("serviceId must be a non-blank string", "serviceId");
			}
			ServiceId = serviceId;

			if (string.IsNullOrWhiteSpace (title)) {
				throw new ArgumentException ("title must be a non-blank string", "title");
			}
			Title = title;

			ShareTitle = "Share";
		}


		#region Service Information

		/// <summary>
		/// Link to sign up.
		/// </summary>
		public Uri CreateAccountLink { get; protected set; }

		#endregion


		#region Authentication

#if PLATFORM_ANDROID
		/// <summary>
		/// Gets the saved accounts associated with this service.
		/// </summary>
		public virtual Task<IEnumerable<Account>> GetAccountsAsync (UIContext context)
		{
			return Task.Factory.StartNew (delegate {
				return AccountStore.Create (context).FindAccountsForService (ServiceId);
			});
		}
#else
		/// <summary>
		/// Gets the saved accounts associated with this service.
		/// </summary>
		public virtual Task<IEnumerable<Account>> GetAccountsAsync ()
		{
			return Task.Factory.StartNew (delegate {
				return AccountStore.Create ().FindAccountsForService (ServiceId);
			});
		}
#endif

		/// <summary>
		/// Gets a value indicating whether this <see cref="Xamarin.Social.Service"/> supports authenticating new accounts.
		/// </summary>
		/// <value>
		/// <c>true</c> if supports authentication; otherwise, <c>false</c>.
		/// </value>
		public virtual bool SupportsAuthentication {
			get {
				return true;
			}
		}

		/// <summary>
		/// Gets the authenticator for this service. The authenticator will present
		/// the user interface needed to authenticate a new account for the service.
		/// This account will then be saved.
		/// </summary>
		/// <returns>
		/// The authenticator or null if authentication is not supported.
		/// </returns>
		protected abstract Authenticator GetAuthenticator ();

#if PLATFORM_ANDROID
		/// <summary>
		/// Presents the necessary UI for the user to sign in to their account.
		/// </summary>
		/// <returns>
		/// The task that will complete when they have signed in.
		/// </returns>
		public AuthenticateUIType GetAuthenticateUI (UIContext context, Action<Account> completedHandler)
		{
			if (context == null) {
				throw new ArgumentNullException ("context");
			}
			var auth = GetAuthenticator ();
			if (auth == null) {
				throw new NotSupportedException ("Account authentication in is not supported.");
			}
			auth.Completed += (sender, e) => {
				if (e.IsAuthenticated) {
					AccountStore.Create (context).Save (e.Account, ServiceId);
				}
				if (completedHandler != null) {
					completedHandler (e.Account);
				}
			};
			auth.Title = Title;
			return auth.GetUI (context);
		}

		/// <summary>
		/// Attempts to authenticate user with this service using the system web browser.
		/// </summary>
		/// <param name="customUrlHandler">
		/// A handler that will open a URL in system browser, register a custom
		/// callback URL handler and wait for the app to go foreground.
		/// </param>
		/// <returns>
		/// The task that will complete when they have signed in.
		/// </returns>
		public virtual Task<IEnumerable<Account>> GetAccountsAsync (ICustomUrlHandler customUrlHandler, UIContext context)
		{
			if (customUrlHandler == null)
				throw new ArgumentNullException ("customUrlHandler",
					"This overload needs a handler to launch system browser and wait for redirect. " +
					"You will need to supply your own implementation of ICustomUrlHandler.");

			var tcs = new TaskCompletionSource<IEnumerable<Account>> ();

			var authenticator = GetAuthenticator () as WebAuthenticator;
			if (authenticator == null)
				throw new NotSupportedException ("This service does not support authentication via web browser.");

			authenticator.Completed += (sender, e) => {
				if (e.IsAuthenticated) {
					SaveAccount (e.Account, context);
					tcs.SetResult (new [] { e.Account });
				} else {
					tcs.SetCanceled ();
				}
			};

			authenticator.AuthenticateWithBrowser (customUrlHandler);
			return tcs.Task;
		}
#else
		/// <summary>
		/// Presents the necessary UI for the user to sign in to their account.
		/// </summary>
		/// <returns>
		/// The task that will complete when they have signed in.
		/// </returns>
		public AuthenticateUIType GetAuthenticateUI (Action<Account> completedHandler)
		{
			var auth = GetAuthenticator ();
			if (auth == null) {
				throw new NotSupportedException ("Account authentication in is not supported.");
			}
			auth.Completed += (sender, e) => {
				if (e.IsAuthenticated) {
					SaveAccount (e.Account);
				}
				if (completedHandler != null) {
					completedHandler (e.Account);
				}
			};
			auth.Title = Title;
			return auth.GetUI ();
		}

		/// <summary>
		/// Attempts to authenticate user with this service using the system web browser.
		/// </summary>
		/// <param name="customUrlHandler">
		/// A handler that will open a URL in system browser, register a custom
		/// callback URL handler and wait for the app to go foreground.
		///
		/// You can use <see cref="SafariUrlHandler.Instace" />, given that you call its
		/// <c>WillEnterForeground</c> and <c>HandleOpenUrl</c> methods from your <c>AppDelegate</c>.
		/// </param>
		/// <returns>
		/// The task that will complete when they have signed in.
		/// </returns>
		public virtual Task<IEnumerable<Account>> GetAccountsAsync (ICustomUrlHandler customUrlHandler)
		{
			if (customUrlHandler == null)
				throw new ArgumentNullException ("customUrlHandler",
					"This overload needs a handler to launch system browser and wait for redirect. " +
					"You can use SafariUrlHandler.Instace, given that you call its " +
					"WillEnterForeground and HandleOpenUrl methods from your AppDelegate.");

			var tcs = new TaskCompletionSource<IEnumerable<Account>> ();

			var authenticator = GetAuthenticator () as WebAuthenticator;
			if (authenticator == null)
				throw new NotSupportedException ("This service does not support authentication via web browser.");

			authenticator.Completed += (sender, e) => {
				if (e.IsAuthenticated) {
					SaveAccount (e.Account);
					tcs.SetResult (new [] { e.Account });
				} else {
					tcs.SetCanceled ();
				}
			};

			authenticator.AuthenticateWithBrowser (customUrlHandler);
			return tcs.Task;
		}
#endif

		/// <summary>
		/// Gets a value indicating whether this <see cref="Xamarin.Social.Service"/> supports reauthorizing an existing account.
		/// </summary>
		/// <value>
		/// <c>true</c> if supports reauthorization; otherwise, <c>false</c>.
		/// </value>
		public virtual bool SupportsReauthorization {
			get {
				return false;
			}
		}

		/// <summary>
		/// Attempts to reauthorize an account.
		/// Service implementors may request a new access token or call appropriate APIs.
		/// </summary>
		/// <returns>
		/// A task that completes with a reauthorized account.
		/// </returns>
		public virtual Task<Account> ReauthorizeAsync (Account account)
		{
			throw new NotSupportedException ();
		}

		#endregion

		#region Account management

		/// <summary>
		/// Gets a value indicating whether this <see cref="Xamarin.Social.Service"/> supports saving accounts.
		/// </summary>
		/// <value>
		/// <c>true</c> if supports saving accounts; otherwise, <c>false</c>.
		/// </value>
		public virtual bool SupportsSave {
			get {
				return true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="Xamarin.Social.Service"/> supports deleting accounts.
		/// </summary>
		/// <value>
		/// <c>true</c> if supports deleting accounts; otherwise, <c>false</c>.
		/// </value>
		public virtual bool SupportsDelete {
			get {
				return true;
			}
		}

#if PLATFORM_ANDROID
		/// <summary>
		/// Saves an account and associates it with this service.
		/// </summary>
		public virtual void SaveAccount (Account account, UIContext context)
		{
			AccountStore.Create (context).Save (account, ServiceId);
		}

		/// <summary>
		/// Deletes a previously saved account associated with this service.
		/// </summary>
		public virtual void DeleteAccount (Account account, UIContext context)
		{
			AccountStore.Create (context).Delete (account, ServiceId);
		}
#else
		/// <summary>
		/// Saves an account and associates it with this service.
		/// </summary>
		public virtual void SaveAccount (Account account)
		{
			AccountStore.Create ().Save (account, ServiceId);
		}

		/// <summary>
		/// Deletes a previously saved account associated with this service.
		/// </summary>
		public virtual void DeleteAccount (Account account)
		{
			AccountStore.Create ().Delete (account, ServiceId);
		}
#endif

		#endregion

		#region Sharing

		/// <summary>
		/// The maximum number of characters that you can share.
		/// </summary>
		public int MaxTextLength { get; protected set; }

		/// <summary>
		/// The maximum number of links that you can share.
		/// </summary>
		public int MaxLinks { get; protected set; }

		/// <summary>
		/// The maximum number of images that you can share.
		/// </summary>
		public int MaxImages { get; protected set; }

		/// <summary>
		/// The maximum number of files that you can share.
		/// </summary>
		public int MaxFiles { get; protected set; }
#if SUPPORT_VIDEO
		public int MaxVideos { get; protected set; }
#endif

		/// <summary>
		/// Gets a value indicating whether this instance has limit on the number of
		/// characters that you can share.
		/// </summary>
		public bool HasMaxTextLength { get { return MaxTextLength < int.MaxValue; } }

		/// <summary>
		/// Calculate the text length of an item if links and other media need to be
		/// inlined with the text.
		/// </summary>
		/// <returns>
		/// The text length after inlining media.
		/// </returns>
		/// <param name='item'>
		/// The item whose text length is to be calculated.
		/// </param>
		public virtual int GetTextLength (Item item)
		{
			return item.Text.Length;
		}

#if PLATFORM_IOS
		/// <summary>
		/// Gets an <see cref="MonoTouch.UIKit.UIViewController"/> that can be used to present the share UI.
		/// </summary>
		/// <returns>
		/// The <see cref="MonoTouch.UIKit.UIViewController"/>.
		/// </returns>
		/// <param name='item'>
		/// The item to share.
		/// </param>
		/// <param name='completionHandler'>
		/// Handler called when the share UI has finished. You must dismiss the view controller in this method
		/// as it won't dismiss itself.
		/// </param>
		public virtual ShareUIType GetShareUI (Item item, Action<ShareResult> completionHandler)
		{
			return new MonoTouch.UIKit.UINavigationController (new ShareViewController (this, item, completionHandler));
		}
#elif PLATFORM_ANDROID
		/// <summary>
		/// Gets an <see cref="Android.Content.Intent"/> that can be used to start the share activity.
		/// </summary>
		/// <returns>
		/// The <see cref="Android.Content.Intent"/>.
		/// </returns>
		/// <param name='activity'>
		/// The <see cref="Android.App.Activity"/> that will invoke the returned <see cref="Android.Content.Intent"/>.
		/// </param>
		/// <param name='item'>
		/// The item to share.
		/// </param>
		/// <param name='completionHandler'>
		/// Handler called when the share UI has finished.
		/// </param>
		public virtual ShareUIType GetShareUI (UIContext activity, Item item, Action<ShareResult> completionHandler)
		{
			var intent = new Android.Content.Intent (activity, typeof (ShareActivity));
			var state = new ShareActivity.State {
				Service = this,
				Item = item,
				CompletionHandler = completionHandler,
			};
			intent.PutExtra ("StateKey", ShareActivity.StateRepo.Add (state));
			return intent;
		}
#else
		/// <summary>
		/// Gets the share UI.
		/// </summary>
		/// <returns>
		/// The share UI.
		/// </returns>
		/// <param name='item'>
		/// The item to share.
		/// </param>
		/// <param name='completionHandler'>
		/// Handler called when the share UI has finished.
		/// </param>
		public virtual ShareUIType GetShareUI (Item item, Action<ShareResult> completionHandler)
		{
			throw new NotImplementedException ("Share not implemented on this platform.");
		}
#endif

		/// <summary>
		/// <para>
		/// Shares the passed-in object without presenting any UI to the user.
		/// </para>
		/// </summary>
		/// <param name='item'>
		/// The item to share.
		/// </param>
		/// <param name='account'>
		/// The account to use to share.
		/// </param>
		public Task ShareItemAsync (Item item, Account account)
		{
			return ShareItemAsync (item, account, CancellationToken.None);
		}

		/// <summary>
		/// <para>
		/// Shares the passed-in object without presenting any UI to the user.
		/// </para>
		/// </summary>
		/// <param name='item'>
		/// The item to share.
		/// </param>
		/// <param name='account'>
		/// The account to use to share.
		/// </param>
		/// <param name='cancellationToken'>
		/// Token used to cancel this operation.
		/// </param>
		public virtual Task ShareItemAsync (Item item, Account account, CancellationToken cancellationToken)
		{
			return Task.Factory.StartNew (() => {
				throw new NotSupportedException (Title + " does not support sharing.");
			});
		}

		//
		// More options:
		//   Share location (Dropbox)
		//   Share people (Google circles)
		//
		
		#endregion


		#region Low-level access

		/// <summary>
		/// Creates a base request to access the service. This is a low-level entrypoint for those
		/// who need to access resources that are not covered by this class.
		/// </summary>
		public Request CreateRequest (string method, Uri url)
		{
			return CreateRequest (method, url, null, null);
		}

		/// <summary>
		/// Creates a base request to access the service. This is a low-level entrypoint for those
		/// who need to access resources that are not covered by this class.
		/// </summary>
		public Request CreateRequest (string method, Uri url, Account account)
		{
			return CreateRequest (method, url, null, account);
		}

		/// <summary>
		/// Creates a base request to access the service. This is a low-level entrypoint for those
		/// who need to access resources that are not covered by this class.
		/// </summary>
		public Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters)
		{
			return CreateRequest (method, url, parameters, null);
		}

		/// <summary>
		/// Creates a base request to access the service. This is a low-level entrypoint for those
		/// who need to access resources that are not covered by this class.
		/// </summary>
		public virtual Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters, Account account)
		{
			return new Request (method, url, parameters, account);
		}

		#endregion
	}
}

