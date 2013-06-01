using System;
using System.Collections.Generic;
using Xamarin.Auth;
using System.Threading.Tasks;

namespace Xamarin.Social
{
	public interface IService
	{
		Request CreateRequest (string method, Uri url);
		Request CreateRequest (string method, Uri url, Account account);
		Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters);
		Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters, Account account);
#if PLATFORM_IOS
		Task<IEnumerable<Account>> GetAccountsAsync ();
		Task<IEnumerable<Account>> GetAccountsAsync (IExternalUrlManager externalUrlManager);
		void DeleteAccount (Account account);
		void SaveAccount (Account account);
#endif
		Task<Account> Reauthorize (Account account);
		Task<string> GetOAuthTokenAsync (Account acc);
		bool SupportsAuthentication { get; }
		bool SupportsDeletion { get; }
		bool SupportsReauthorization { get; }
		string Title { get; }
		string ServiceId { get; }
	}
}

