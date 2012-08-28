using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;

#if PLATFORM_IOS
using UIContext = MonoTouch.UIKit.UIViewController;
#else
using UIContext = System.Object;
#endif

namespace Xamarin.Social
{
	/// <summary>
	/// Social Networking Service.
	/// </summary>
	public abstract class Service
	{
		/// <summary>
		/// Uniquely identifies this service type.
		/// </summary>
		public string ServiceId { get; private set; }

		/// <summary>
		/// Text used to label this service in the UI.
		/// </summary>
		public string Title { get; private set; }

		/// <summary>
		/// Text used as the title of screen when editing an item.
		/// </summary>
		public string ShareTitle { get; protected set; }

		protected Service (string serviceId, string title)
		{
			if (string.IsNullOrWhiteSpace (serviceId)) {
				throw new ArgumentException ("serviceId must be a non-blank string", "serviceId");
			}
			ServiceId = serviceId;

			if (string.IsNullOrWhiteSpace (title)) {
				throw new ArgumentException ("title must be a non-blank string", "title");
			}
			Title = title;

			ShareTitle = "Share";
		}


		#region Service Information

		/// <summary>
		/// Link to sign up.
		/// </summary>
		public Uri CreateAccountLink { get; protected set; }

		#endregion


		#region Authentication

		/// <summary>
		/// Gets the saved accounts associated with this service.
		/// </summary>
		public virtual Task<List<Account>> GetAccountsAsync ()
		{
			return Task.Factory.StartNew (delegate {
				return AccountStore.Create ().FindAccountsForService (ServiceId);
			});
		}

		/// <summary>
		/// Gets the authenticator for this service. The authenticator will present
		/// the user interface needed to authenticate a new account for the service.
		/// This account will then be saved.
		/// </summary>
		/// <returns>
		/// The authenticator or null if authentication is not supported.
		/// </returns>
		protected abstract Authenticator GetAuthenticator ();

		/// <summary>
		/// Presents the necessary UI for the user to sign in to their account.
		/// </summary>
		/// <returns>
		/// The task that will complete when they have signed in.
		/// </returns>
		public virtual Task<Account> AddAccountAsync (UIContext uiContext)
		{
			var auth = GetAuthenticator ();
			if (auth == null) {
				throw new NotSupportedException ("Account sign in is not supported.");
			}
			return auth.AuthenticateAsync (uiContext, this);
		}

		#endregion


		#region Sharing

		public bool CanShareText { get; protected set; }
		public bool CanShareLinks { get; protected set; }
		public bool CanShareImages { get; protected set; }
		public bool CanShareFiles { get; protected set; }
#if SUPPORT_VIDEO
		public bool CanShareVideo { get; protected set; }
#endif

		/// <summary>
		/// <para>
		/// Shares the passed-in object by presenting the necessary UI to the user.
		/// </para>
		/// <para>
		/// If there are saved accounts, then the composer will allow the user to choose
		/// which account to send to. If there are no accounts, then a sign in dialog will
		/// be presented followed by the compose view.
		/// </para>
		/// </summary>
		/// <remarks>
		/// Must be called from the UI thread because this will cause dialogs to be displayed.
		/// </remarks>			
		/// <param name='item'>
		/// The item to share. It may be edited by the user.
		/// </param>
		public virtual Task<ShareResult> ShareAsync (UIContext uiContext, Item item)
		{
			var viewModel = new ShareViewModel (this, item, ShareItemAsync);

			GetAccountsAsync ().ContinueWith (accountsTask => {
				if (accountsTask.Result.Count > 0) {
					viewModel.Accounts = accountsTask.Result.ToList ();
					PresentShareUI (uiContext, viewModel);
				}
				else {
					AddAccountAsync (uiContext).ContinueWith (addTask => {
						if (addTask.Result != null) {
							viewModel.Accounts = new List<Account> { addTask.Result };
							PresentShareUI (uiContext, viewModel);
						}
						else {
							viewModel.Cancel ();
						}
					}, TaskScheduler.FromCurrentSynchronizationContext ());
				}
			}, TaskScheduler.FromCurrentSynchronizationContext ());

			return Task.Factory.StartNew (delegate {
				viewModel.Wait ();
				return viewModel.Result;
			}, TaskCreationOptions.LongRunning);
		}

		void PresentShareUI (UIContext uiContext, ShareViewModel viewModel)
		{
#if PLATFORM_IOS
			var share = new ShareController (viewModel);
			var nav = new MonoTouch.UIKit.UINavigationController (share);
			uiContext.PresentModalViewController (nav, true);
#else
			throw new NotImplementedException ("Share not implemented on this platform.");
#endif
		}

		/// <summary>
		/// <para>
		/// Shares the passed-in object without presenting any UI to the user.
		/// </para>
		/// </summary>
		/// <param name='item'>
		/// The item to share.
		/// </param>
		/// <param name='account'>
		/// The account to use to share.
		/// </param>
		protected virtual Task ShareItemAsync (Item item, Account account, CancellationToken cancellationToken)
		{
			return Task.Factory.StartNew (() => {
				throw new NotSupportedException (Title + " does not support sharing.");
			});
		}

		//
		// More options:
		//   Share location (Dropbox)
		//   Share people (Google circles)
		//
		
		#endregion


		#region Low-level access

		/// <summary>
		/// Creates a base request to access the service. This is a low-level entrypoint for those
		/// who need to access resources that are not covered by this class.
		/// </summary>
		public Request CreateRequest (string method, Uri url)
		{
			return CreateRequest (method, url, null, null);
		}

		/// <summary>
		/// Creates a base request to access the service. This is a low-level entrypoint for those
		/// who need to access resources that are not covered by this class.
		/// </summary>
		public Request CreateRequest (string method, Uri url, Account account)
		{
			return CreateRequest (method, url, null, account);
		}

		/// <summary>
		/// Creates a base request to access the service. This is a low-level entrypoint for those
		/// who need to access resources that are not covered by this class.
		/// </summary>
		public Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters)
		{
			return CreateRequest (method, url, parameters, null);
		}

		/// <summary>
		/// Creates a base request to access the service. This is a low-level entrypoint for those
		/// who need to access resources that are not covered by this class.
		/// </summary>
		public virtual Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters, Account account)
		{
			return new Request (method, url, parameters, account);
		}

		#endregion


		#region Service Registry

		static Dictionary<string, Service> registry;

		static Service ()
		{
			registry = new Dictionary<string, Service> ();

			RegisterService (Facebook = new Xamarin.Social.Services.FacebookService ());
			//RegisterService (new GoogleService ());


#if PLATFORM_IOS
			RegisterService (Twitter = new Xamarin.Social.Services.TwitterService5 ());
#endif

			//RegisterService (new TwitterService ());
		}

		/// <summary>
		/// Gets all the registered services.
		/// </summary>
		public static Service[] GetServices ()
		{
			lock (registry) {
				return registry.Values.ToArray ();
			}
		}

		/// <summary>
		/// Registers a service.
		/// </summary>
		public static void RegisterService (Service service)
		{
			lock (registry) {
				Service s;
				if (registry.TryGetValue (service.ServiceId, out s)) {
					throw new ArgumentException ("Service '" + service.ServiceId + "' is already registered.", "service");
				}
				else {
					registry.Add (service.ServiceId, service);
				}
			}
		}

		public static Xamarin.Social.Services.TwitterService Twitter { get; private set; }
		public static Xamarin.Social.Services.FacebookService Facebook { get; private set; }
		//public static Service Google { get; private set; }
		//public static Service SinaWeibo { get; private set; }

		#endregion


		#region HTTP Helpers



		#endregion
	}
}

