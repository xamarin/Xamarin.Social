//
//  Copyright 2012-2013, Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;
using Xamarin.Auth;

#if (PLATFORM_IOS || __IOS__) && ! __UNIFIED__
using ShareUIType = MonoTouch.UIKit.UIViewController;
using AuthenticateUIType = MonoTouch.UIKit.UIViewController;
#elif (PLATFORM_IOS || __IOS__) && __UNIFIED__
using ShareUIType = UIKit.UIViewController;
using AuthenticateUIType = UIKit.UIViewController;
#elif PLATFORM_ANDROID || __ANDROID__
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
	/// Represents a social networking service.
	/// </summary>
	public abstract class Service
	{
		/// <summary>
		/// Gets the unique identifier for this service type.
		/// </summary>
		public string ServiceId { get; private set; }

		/// <summary>
		/// Gets text used to label this service in the UI.
		/// </summary>
		public string Title { get; private set; }

		/// <summary>
		/// Gets text used as the title of screen when editing an item.
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
		/// Gets the URL to a sign up page.
		/// </summary>
		public Uri CreateAccountLink { get; protected set; }

		#endregion


		#region Authentication

#if PLATFORM_ANDROID
		/// <summary>
		/// Asynchronously retrieves the saved accounts associated with this service.
		/// </summary>
		public virtual Task<IEnumerable<Account>> GetAccountsAsync (UIContext context)
		{
			return Task.Factory.StartNew (delegate {
				return AccountStore.Create (context).FindAccountsForService (ServiceId);
			});
		}
#else
		/// <summary>
		/// Asynchronously retrieves the saved accounts associated with this service.
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
		/// <c>true</c> if the service supports authentication; otherwise, <c>false</c>.
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
		/// The authenticator or <c>null</c> if authentication is not supported.
		/// </returns>
		protected abstract Authenticator GetAuthenticator ();

#if PLATFORM_ANDROID || __ANDROID__
		/// <summary>
		/// Gets the necessary UI for the user to sign in to their account.
		/// </summary>
		/// <returns>
		/// A platform-specific UI type for the user to present.
		/// </returns>
		/// <param name="context">The context for the UI.</param>
		/// <param name="completedHandler">A callback for when authentication has completed successfuly.</param>
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
#elif __IOS__
		/// <summary>
		/// Gets the necessary UI for the user to sign in to their account.
		/// </summary>
		/// <returns>
		/// A platform-specific UI type for the user to present.
		/// </returns>
		/// <param name="completedHandler">A callback for when authentication has completed successfuly.</param>
		public AuthenticateUIType GetAuthenticateUI (Action<Account> completedHandler)
		{
			var auth = GetAuthenticator ();
			if (auth == null) {
				throw new NotSupportedException ("Account authentication in is not supported.");
			}
			auth.Completed += (sender, e) => {
				if (e.IsAuthenticated) {
					AccountStore.Create ().Save (e.Account, ServiceId);
				}
				if (completedHandler != null) {
					completedHandler (e.Account);
				}
			};
			auth.Title = Title;
			return auth.GetUI ();
		}
#endif
		#endregion

		#region Account management

		/// <summary>
		/// Gets a value indicating whether this <see cref="Xamarin.Social.Service"/> supports saving and deleting accounts.
		/// </summary>
		/// <value>
		/// <c>true</c> if supports saving and deleting accounts; otherwise, <c>false</c>.
		/// </value>
		public virtual bool SupportsSave {
			get {
				return true;
			}
		}

#if PLATFORM_ANDROID || __ANDROID__
		/// <summary>
		/// Saves an account and associates it with this service.
		/// </summary>
		public virtual void SaveAccount (global::Android.Content.Context context, Account account)
		{
			AccountStore.Create (context).Save (account, ServiceId);
		}

		/// <summary>
		/// Deletes a previously saved account associated with this service.
		/// </summary>
		public virtual void DeleteAccount (global::Android.Content.Context context, Account account)
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
		/// Gets the maximum number of characters that you can share.
		/// </summary>
		public int MaxTextLength { get; protected set; }

		/// <summary>
		/// Gets the maximum number of links that you can share.
		/// </summary>
		public int MaxLinks { get; protected set; }

		/// <summary>
		/// Gets the maximum number of images that you can share.
		/// </summary>
		public int MaxImages { get; protected set; }

		/// <summary>
		/// Gest the maximum number of files that you can share.
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
			#if ! __UNIFIED__ 
			return new MonoTouch.UIKit.UINavigationController (new ShareViewController (this, item, completionHandler));
			#else
			return new UIKit.UINavigationController (new ShareViewController (this, item, completionHandler));
			#endif
		}
#elif PLATFORM_ANDROID || __ANDROID__
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
			var intent = new global::Android.Content.Intent (activity, typeof (ShareActivity));
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
		/// Shares the passed-in object without presenting any UI to the user.
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
		/// Shares the passed-in object without presenting any UI to the user.
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
		/// Creates a base request to access the service.
		/// </summary>
		/// <param name="method">The HTTP method to use with the request.</param>
		/// <param name="url">The url of the request.</param>
		/// <remarks>
		/// <para>This is a low-level entrypoint for those who need to access resources that are not covered by this class.</para>
		/// <para>The returned request will not be authenticated.</para>
		/// </remarks>
		public Request CreateRequest (string method, Uri url)
		{
			return CreateRequest (method, url, null, null);
		}

		/// <summary>
		/// Creates a base request to access the service.
		/// </summary>
		/// <param name="method">The HTTP method to use with the request.</param>
		/// <param name="url">The url of the request.</param>
		/// <param name="account">The account to authenticate this request with.</param>
		/// <remarks>
		/// <para>This is a low-level entrypoint for those who need to access resources that are not covered by this class.</para>
		/// </remarks>
		public Request CreateRequest (string method, Uri url, Account account)
		{
			return CreateRequest (method, url, null, account);
		}

		/// <summary>
		/// Creates a base request to access the service.
		/// </summary>
		/// <param name="method">The HTTP method to use with the request.</param>
		/// <param name="url">The url of the request.</param>
		/// <param name="parameters">The parameters to populate the request with.</param>
		/// <remarks>
		/// <para>This is a low-level entrypoint for those who need to access resources that are not covered by this class.</para>
		/// <para>The returned request will not be authenticated.</para>
		/// </remarks>
		public Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters)
		{
			return CreateRequest (method, url, parameters, null);
		}

		/// <summary>
		/// Creates a base request to access the service.
		/// </summary>
		/// <param name="method">The HTTP method to use with the request.</param>
		/// <param name="url">The url of the request.</param>
		/// <param name="parameters">The parameters to populate the request with.</param>
		/// <param name="account">The account to authenticate this request with.</param>
		/// <remarks>
		/// <para>This is a low-level entrypoint for those who need to access resources that are not covered by this class.</para>
		/// </remarks>
		public virtual Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters, Account account)
		{
			return new Request (method, url, parameters, account);
		}

		#endregion
	}
}

