using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

#if PLATFORM_IOS
using UIContext = MonoTouch.UIKit.UIViewController;
#else
using UIContext = System.Object;
#endif

namespace Xamarin.Social
{
	/// <summary>
	/// A process to authenticate the user.
	/// </summary>
	public abstract class Authenticator
	{
		public Service Service { get; private set; }

		ManualResetEvent completedEvent;
		Exception exception;
		string failureMessage;
		Account account;

		/// <summary>
		/// Authenticates the user using an approprate GUI.
		/// </summary>
		/// <returns>
		/// The task notifying the completion (good or bad) of the authentication process.
		/// </returns>
		public Task<Account> AuthenticateAsync (UIContext context, Service service)
		{
			Service = service;

			completedEvent = new ManualResetEvent (false);

			PresentUI (context);

			return Task.Factory.StartNew (delegate {

				completedEvent.WaitOne ();

				if (exception != null) {
					throw new AggregateException (exception);
				}
				else if (!string.IsNullOrEmpty (failureMessage)) {
					throw new ApplicationException (failureMessage);
				}
				else {
					return account;
				}

			}, TaskCreationOptions.LongRunning);
		}

		protected abstract void PresentUI (UIContext context);

		/// <summary>
		/// Implementations must call this function when they have successfully authenticated.
		/// </summary>
		/// <param name='account'>
		/// The authenticated account.
		/// </param>
		public void OnSucceeded (Account account)
		{
			this.account = account;

			//
			// Store the account
			//
			AccountStore.Create ().Save (account, Service.ServiceId);

			//
			// Notify the work
			//
			var ev = Succeeded;
			if (ev != null) {
				ev (this, EventArgs.Empty);
			}

			completedEvent.Set ();
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

		public event EventHandler Succeeded;

		/// <summary>
		/// Implementations must call this function when they have failed to authenticate.
		/// </summary>
		/// <param name='message'>
		/// The reason that this authentication has failed.
		/// </param>
		public void OnFailed (string message)
		{
			this.failureMessage = message;

			var ev = Failed;
			if (ev != null) {
				ev (this, new AuthenticationFailedEventArgs (message));
			}

			completedEvent.Set ();
		}

		/// <summary>
		/// Implementations must call this function when they have failed to authenticate.
		/// </summary>
		/// <param name='exception'>
		/// The reason that this authentication has failed.
		/// </param>
		public void OnFailed (Exception exception)
		{
			this.exception = exception;

			var ev = Failed;
			if (ev != null) {
				ev (this, new AuthenticationFailedEventArgs (exception));
			}

			completedEvent.Set ();
		}

		public event EventHandler<AuthenticationFailedEventArgs> Failed;

		/// <summary>
		/// Implementations must call this function when they have cancelled the operation.
		/// </summary>
		public void OnCancelled ()
		{
			var ev = Cancelled;
			if (ev != null) {
				ev (this, EventArgs.Empty);
			}
			
			completedEvent.Set ();
		}
		
		public event EventHandler Cancelled;
	}

	public class AuthenticationFailedEventArgs : EventArgs
	{
		public string Message { get; private set; }
		public Exception Exception { get; private set; }

		public AuthenticationFailedEventArgs (string message)
		{
			Message = message;
		}

		public AuthenticationFailedEventArgs (Exception exception)
		{
			Message = exception.GetUserMessage ();
			Exception = exception;
		}
	}
}

