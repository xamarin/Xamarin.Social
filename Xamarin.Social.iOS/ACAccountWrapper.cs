using System;
using MonoTouch.Accounts;

namespace Xamarin.Social
{
	class ACAccountWrapper : Account
	{
		ACAccountStore store;

		public ACAccount ACAccount { get; private set; }

		public override string Username { 
			get { 
				return ACAccount.Username;
			}
			set {
			}
		}

		public ACAccountWrapper (ACAccount account, ACAccountStore store)
		{
			if (account == null) {
				throw new ArgumentNullException ("account");
			}
			this.ACAccount = account;

			this.store = store;
		}
	}
}

