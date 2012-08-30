using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

#if PLATFORM_IOS
using AuthenticateUIType = MonoTouch.UIKit.UIViewController;
#elif PLATFORM_ANDROID
using AuthenticateUIType = Android.Content.Intent;
using UIContext = Android.App.Activity;
#else
using AuthenticateUIType = System.Object;
#endif

namespace Xamarin.Social
{
	public delegate void AuthenticateCompletionHandler (Account authenticatedAccount);

	/// <summary>
	/// A process to authenticate the user.
	/// </summary>
	public abstract class Authenticator
	{
		public AuthenticateCompletionHandler CompletionHandler { get; set; }
		public Service Service { get; set; }

#if PLATFORM_ANDROID
		UIContext context;
		public AuthenticateUIType GetUI (UIContext context)
		{
			this.context = context;
			return GetPlatformUI (context);
		}
		protected abstract AuthenticateUIType GetPlatformUI (UIContext context);
#else
		public AuthenticateUIType GetUI ()
		{
			return GetPlatformUI ();
		}
		protected abstract AuthenticateUIType GetPlatformUI ();
#endif


		/// <summary>
		/// Implementations must call this function when they have successfully authenticated.
		/// </summary>
		/// <param name='account'>
		/// The authenticated account.
		/// </param>
		public void OnSucceeded (Account account)
		{
			BeginInvokeOnUIThread (delegate {
				//
				// Store the account
				//
				AccountStore.Create ().Save (account, Service.ServiceId);

				//
				// Notify the work
				//
				if (CompletionHandler != null) {
					CompletionHandler (account);
				}
			});
		}

		/// <summary>
		/// Implementations must call this function when they have successfully authenticated.
		/// </summary>
		/// <param name='username'>
		/// User name of the account.
		/// </param>
		/// <param name='accountProperties'>
		/// Additional data, such as access tokens, that need to be stored with the account. This
		/// information is secured.
		/// </param>
		public void OnSucceeded (string username, IDictionary<string, string> accountProperties)
		{
			OnSucceeded (new Account (username, accountProperties));
		}

		/// <summary>
		/// Implementations must call this function when they have failed to authenticate.
		/// </summary>
		/// <param name='message'>
		/// The reason that this authentication has failed.
		/// </param>
		public void OnError (string message)
		{
			BeginInvokeOnUIThread (delegate {
				var ev = Error;
				if (ev != null) {
					ev (this, new AuthenticationErrorEventArgs (message));
				}
			});
		}

		/// <summary>
		/// Implementations must call this function when they have failed to authenticate.
		/// </summary>
		/// <param name='exception'>
		/// The reason that this authentication has failed.
		/// </param>
		public void OnError (Exception exception)
		{
			BeginInvokeOnUIThread (delegate {
				var ev = Error;
				if (ev != null) {
					ev (this, new AuthenticationErrorEventArgs (exception));
				}
			});
		}

		public event EventHandler<AuthenticationErrorEventArgs> Error;

		/// <summary>
		/// Implementations must call this function when they have cancelled the operation.
		/// </summary>
		public void OnCancelled ()
		{
			BeginInvokeOnUIThread (delegate {
				if (CompletionHandler != null) {
					CompletionHandler (null);
				}
			});
		}

		void BeginInvokeOnUIThread (Action action)
		{
#if PLATFORM_IOS
			MonoTouch.UIKit.UIApplication.SharedApplication.BeginInvokeOnMainThread (delegate { action (); });
#elif PLATFORM_ANDROID
			context.RunOnUiThread (action);
#else
			action ();
#endif
		}
	}

	public class AuthenticationErrorEventArgs : EventArgs
	{
		public string Message { get; private set; }
		public Exception Exception { get; private set; }

		public AuthenticationErrorEventArgs (string message)
		{
			Message = message;
		}

		public AuthenticationErrorEventArgs (Exception exception)
		{
			Message = exception.GetUserMessage ();
			Exception = exception;
		}
	}
}

