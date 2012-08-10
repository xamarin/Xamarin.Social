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
		Exception error;
		AuthenticationResult? result;

		public abstract string Title { get; }

		/// <summary>
		/// Authenticates the user using an approprate GUI.
		/// </summary>
		/// <returns>
		/// The task notifying the completion (good or bad) of the authentication process.
		/// </returns>
		public Task<AuthenticationResult> AuthenticateAsync (UIContext context, Service service)
		{
			Service = service;

			completedEvent = new ManualResetEvent (false);

			PresentUI (context);

			return Task.Factory.StartNew (delegate {

				completedEvent.WaitOne ();

				if (error != null) {
					throw new AggregateException (error);
				}
				else {
					return result.HasValue ? result.Value : AuthenticationResult.Cancelled;
				}

			}, TaskCreationOptions.LongRunning);
		}

		protected abstract void PresentUI (UIContext context);

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
		public void OnSuccess (string username, IDictionary<string, string> accountProperties)
		{
			this.result = AuthenticationResult.Success;

			//
			// Store the account
			//
			AccountStore.Create ().Save (new Account (username, accountProperties), Service.ServiceId);

			//
			// Notify the work
			//
			var ev = Success;
			if (ev != null) {
				ev (this, EventArgs.Empty);
			}

			completedEvent.Set ();
		}

		public event EventHandler Success;

		/// <summary>
		/// Implementations must call this function when they have failed to authenticate or the
		/// user has cancelled the operation.
		/// </summary>
		/// <param name='result'>
		/// The reason that this authentication has failed.
		/// </param>
		public void OnFailure (AuthenticationResult result)
		{
			this.result = (result == AuthenticationResult.Success) ? AuthenticationResult.Cancelled : result;

			var ev = Failure;
			if (ev != null) {
				ev (this, EventArgs.Empty);
			}

			completedEvent.Set ();
		}

		/// <summary>
		/// Implementations must call this function when they have failed to authenticate or the
		/// user has cancelled the operation.
		/// </summary>
		/// <param name='result'>
		/// The reason that this authentication has failed.
		/// </param>
		public void OnFailure (Exception error)
		{
			this.result = AuthenticationResult.Failed;
			this.error = error;

			var ev = Failure;
			if (ev != null) {
				ev (this, EventArgs.Empty);
			}

			completedEvent.Set ();
		}

		public event EventHandler Failure;
	}
}

