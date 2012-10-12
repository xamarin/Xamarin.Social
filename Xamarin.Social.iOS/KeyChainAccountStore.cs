using System;
using MonoTouch.Security;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;

namespace Xamarin.Social
{
	class KeyChainAccountStore : AccountStore
	{
		public override IEnumerable<Account> FindAccountsForService (string serviceId)
		{
			var query = new SecRecord (SecKind.GenericPassword);
			query.Service = serviceId;

			SecStatusCode result;
			var records = SecKeyChain.QueryAsRecord (query, 1000, out result);

			return records != null ?
				records.Select (GetAccountFromRecord).ToList () :
				new List<Account> ();
		}

		Account GetAccountFromRecord (SecRecord r)
		{
			var serializedData = NSString.FromData (r.Generic, NSStringEncoding.UTF8);
			return Account.Deserialize (serializedData);
		}

		Account FindAccount (string username, string serviceId)
		{
			var query = new SecRecord (SecKind.GenericPassword);
			query.Service = serviceId;
			query.Account = username;

			SecStatusCode result;
			var record = SecKeyChain.QueryAsRecord (query, out result);

			return record != null ?
				GetAccountFromRecord (record) :
				null;
		}

		public override void Save (Account account, string serviceId)
		{
			var statusCode = SecStatusCode.Success;
			var serializedAccount = account.Serialize ();
			var data = NSData.FromString (serializedAccount, NSStringEncoding.UTF8);

			//
			// Remove any existing record
			//
			var existing = FindAccount (account.Username, serviceId);

			if (existing != null) {
				var query = new SecRecord (SecKind.GenericPassword);
				query.Service = serviceId;
				query.Account = account.Username;

				statusCode = SecKeyChain.Remove (query);
				if (statusCode != SecStatusCode.Success) {
					throw new Exception ("Could not save account to KeyChain: " + statusCode);
				}
			}

			//
			// Add this record
			//
			var record = new SecRecord (SecKind.GenericPassword);
			record.Service = serviceId;
			record.Account = account.Username;
			record.Generic = data;
			record.Accessible = SecAccessible.WhenUnlocked;

			statusCode = SecKeyChain.Add (record);

			if (statusCode != SecStatusCode.Success) {
				throw new Exception ("Could not save account to KeyChain: " + statusCode);
			}
		}
	}
}

