using System;
using System.Collections.Generic;

namespace Xamarin.Social
{
	public abstract class AccountStore
	{
#if PLATFORM_IOS
		public static AccountStore Create ()
		{
			return new KeyChainAccountStore ();
		}
#elif PLATFORM_ANDROID
		public static AccountStore Create (Android.Content.Context context)
		{
			return new AndroidAccountStore (context);
		}
#else
		public static AccountStore Create ()
		{
			throw new NotSupportedException ("Cannot save account on this platform");
		}
#endif

		public abstract IEnumerable<Account> FindAccountsForService (string serviceId);

		public abstract void Save (Account account, string serviceId);
	}
}

