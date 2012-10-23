using System;
using System.Collections.Generic;

namespace Xamarin.Auth
{
	/// <summary>
	/// A persistent storage for <see cref="Account"/>s. This storage is encrypted.
	/// Accounts are stored using a service ID and the username of the account
	/// as a primary key.
	/// </summary>
	public abstract class AccountStore
	{
#if PLATFORM_IOS
		/// <summary>
		/// Create an account store.
		/// </summary>
		public static AccountStore Create ()
		{
			return new KeyChainAccountStore ();
		}
#elif PLATFORM_ANDROID
		/// <summary>
		/// Create an account store.
		/// </summary>
		public static AccountStore Create (Android.Content.Context context)
		{
			return new AndroidAccountStore (context);
		}
#else
		/// <summary>
		/// Create an account store.
		/// </summary>
		public static AccountStore Create ()
		{
			throw new NotSupportedException ("Cannot save account on this platform");
		}
#endif

		/// <summary>
		/// Finds the accounts for a given service.
		/// </summary>
		/// <returns>
		/// The accounts for the service.
		/// </returns>
		/// <param name='serviceId'>
		/// Service identifier.
		/// </param>
		public abstract IEnumerable<Account> FindAccountsForService (string serviceId);

		/// <summary>
		/// Save the specified account by combinging its username and the serviceId
		/// to form its primary key.
		/// </summary>
		/// <param name='account'>
		/// Account to store.
		/// </param>
		/// <param name='serviceId'>
		/// Service identifier.
		/// </param>
		public abstract void Save (Account account, string serviceId);
	}
}

