using System;
using System.Collections.Generic;

namespace Xamarin.Social
{
	/// <summary>
	/// Account credential.
	/// </summary>
	public abstract class Authenticator
	{
		/// <summary>
		/// Implementations should call this method when they have succeeded
		/// or failed at authenticating.
		/// </summary>
		/// <param name='account'>
		/// Account.
		/// </param>
		protected virtual void Save (Account account)
		{
		}
	}
}

