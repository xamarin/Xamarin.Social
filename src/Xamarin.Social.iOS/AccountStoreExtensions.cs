using System;
using System.Threading.Tasks;
using MonoTouch.Accounts;
using MonoTouch.Foundation;

namespace Xamarin.Social
{
	internal static class AccountStoreExtensions
	{
		public static Task<bool> RequestAccessAsync (this ACAccountStore store, ACAccountType accountType, AccountStoreOptions options)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool> ();
			store.RequestAccess (accountType, options, delegate (bool granted, NSError error)
			                    {
				if (error != null)
				{
					tcs.SetException (new NSErrorException (error));
				}
				else
				{
					tcs.SetResult (granted);
				}
			});
			return tcs.Task;
		}

		public static Task<ACAccountCredentialRenewResult> RenewCredentialsAsync (this ACAccountStore store, ACAccount account)
		{
			TaskCompletionSource<ACAccountCredentialRenewResult> tcs = new TaskCompletionSource<ACAccountCredentialRenewResult> ();
			store.RenewCredentials (account, delegate (ACAccountCredentialRenewResult arg1, NSError arg2)
			{
				if (arg2 != null)
				{
					tcs.SetException (new NSErrorException (arg2));
				}
				else
				{
					tcs.SetResult (arg1);
				}
			});
			return tcs.Task;
		}
	}
}

