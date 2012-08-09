using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Social
{
	/// <summary>
	/// A process to authenticate the user.
	/// </summary>
	public abstract class Authenticator
	{
		public Account Account { get; private set; }

		/// <summary>
		/// Authenticates the user using an approprate GUI.
		/// </summary>
		/// <returns>
		/// The task notifying the completion (good or bad) of the authentication process.
		/// </returns>
		public abstract Task<AuthenticationResult> AuthenticateAsync ();
	}
}

