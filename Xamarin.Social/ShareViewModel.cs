using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.Social
{
	class ShareViewModel
	{
		public delegate Task<ShareResult> ShareItemAsyncFunc (Item item, Account account, CancellationToken cancellationToken);

		ShareItemAsyncFunc shareFunc;
		ManualResetEvent doneEvent;
		CancellationTokenSource cts;

		/// <summary>
		/// The service that is sharing.
		/// </summary>
		public Service Service { get; private set; }

		/// <summary>
		/// The item being shared.
		/// </summary>
		public Item Item { get; private set; }

		IList<Account> accounts;

		/// <summary>
		/// The various accounts that can be used to share.
		/// </summary>
		public IList<Account> Accounts {
			get { return accounts; }
			set {
				accounts = value;
				UseAccount = accounts.FirstOrDefault ();
			}
		}

		/// <summary>
		/// The account that will be used for this share.
		/// </summary>
		public Account UseAccount { get; set; }

		/// <summary>
		/// The Result of the share operation.
		/// </summary>
		public ShareResult Result { get; private set; }
		
		public ShareViewModel (Service service, Item item, ShareItemAsyncFunc shareFunc)
		{
			Service = service;
			Item = item;
			this.shareFunc = shareFunc;

			doneEvent = new ManualResetEvent (false);

			Accounts = new List<Account> ();
			UseAccount = null;
		}

		/// <summary>
		/// Waits for this share UI to complete.
		/// Do not call this from the UI thread as it is a blocking call
		/// and you will create a deadlock.
		/// </summary>
		public void Wait ()
		{
			doneEvent.WaitOne ();
		}

		/// <summary>
		/// Cancel this share.
		/// </summary>
		public void Cancel ()
		{
			if (cts != null) {
				cts.Cancel ();
			}

			Result = ShareResult.Cancelled;
			doneEvent.Set ();
		}

		/// <summary>
		/// Shares the item.
		/// </summary>
		public Task ShareAsync ()
		{
			if (cts != null) {
				throw new InvalidOperationException ("ShareAsync was called a second time");
			}
			cts = new CancellationTokenSource ();
			var token = cts.Token;

			return shareFunc (Item, UseAccount, token).ContinueWith (shareTask => {
				Result = shareTask.Result;
				doneEvent.Set ();
			}, token);
		}
	}
}

