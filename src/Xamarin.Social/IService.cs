using System;
using System.Collections.Generic;
using Xamarin.Auth;

namespace Xamarin.Social
{
	public interface IService
	{
		Request CreateRequest (string method, Uri url);
		Request CreateRequest (string method, Uri url, Account account);
		Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters);
		Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters, Account account);
	}
}

