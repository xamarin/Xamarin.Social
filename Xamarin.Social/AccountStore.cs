using System;
using System.Collections.Generic;

namespace Xamarin.Social
{
	public abstract class AccountStore
	{
		public static AccountStore Create ()
		{
#if PLATFORM_IOS
			return new KeyChainAccountStore ();
#else
			throw new NotSupportedException ("Cannot save account on this platform");
#endif
		}

		public abstract Account[] FindAccountsForService (string serviceId);

		public abstract void Save (Account account, string serviceId);
	}
}

