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
		Task<IEnumerable<Account>> GetAccountsAsync ();
#if PLATFORM_IOS
		Task<IEnumerable<Account>> GetAccountsAsync (IExternalUrlManager externalUrlManager);
#endif
		Task<string> GetOAuthTokenAsync (Account acc);
		bool SupportsAuthentication { get; }
	}
}

