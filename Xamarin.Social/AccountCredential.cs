using System;
using System.Collections.Generic;

namespace Xamarin.Social
{
	/// <summary>
	/// Account credential.
	/// </summary>
	public class AccountCredential
	{
		public IList<AccountCredentialField> Fields { get; private set; }

		public AccountCredential ()
		{
			Fields = new List<AccountCredentialField> ();
		}
	}
}

