using System;
using MonoTouch.Accounts;

namespace Xamarin.Social
{
	public class ACAccountWrapper : Account
	{
		public ACAccount ACAccount { get; private set; }

		public ACAccountWrapper (ACAccount account)
		{
			if (account == null) {
				throw new ArgumentNullException ("account");
			}
			this.ACAccount = account;
		}

		public static ACAccountWrapper GetForAccount (ACAccount account)
		{
			if (account == null) {
				return null;
			}
			else {
				return new ACAccountWrapper (account);
			}
		}
	}
}

