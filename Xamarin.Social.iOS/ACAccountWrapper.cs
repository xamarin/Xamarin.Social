using System;
using MonoTouch.Accounts;

namespace Xamarin.Social
{
	class ACAccountWrapper : Account
	{
		public ACAccount ACAccount { get; private set; }

		public override string Username { 
			get { 
				return ACAccount.Username;
			}
			set {
			}
		}

		public ACAccountWrapper (ACAccount account)
		{
			if (account == null) {
				throw new ArgumentNullException ("account");
			}
			this.ACAccount = account;
		}
	}
}

