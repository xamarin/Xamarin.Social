using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;

#if PLATFORM_IOS
using ShareUIType = MonoTouch.UIKit.UIViewController;
using AuthenticateUIType = MonoTouch.UIKit.UIViewController;
#elif PLATFORM_ANDROID
using ShareUIType = Android.Content.Intent;
using AuthenticateUIType = Android.Content.Intent;
using UIContext = Android.App.Activity;
#else
using ShareUIType = System.Object;
using AuthenticateUIType = System.Object;
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

#if PLATFORM_ANDROID
		/// <summary>
		/// Gets the saved accounts associated with this service.
		/// </summary>
		public virtual Task<List<Account>> GetAccountsAsync (UIContext context)
		{
			return Task.Factory.StartNew (delegate {
				return AccountStore.Create (context).FindAccountsForService (ServiceId);
			});
		}
#else
		/// <summary>
		/// Gets the saved accounts associated with this service.
		/// </summary>
		public virtual Task<List<Account>> GetAccountsAsync ()
		{
			return Task.Factory.StartNew (delegate {
				return AccountStore.Create ().FindAccountsForService (ServiceId);
			});
		}
#endif

		/// <summary>
		/// Gets the authenticator for this service. The authenticator will present
		/// the user interface needed to authenticate a new account for the service.
		/// This account will then be saved.
		/// </summary>
		/// <returns>
		/// The authenticator or null if authentication is not supported.
		/// </returns>
		protected abstract Authenticator GetAuthenticator ();

#if PLATFORM_ANDROID
		/// <summary>
		/// Presents the necessary UI for the user to sign in to their account.
		/// </summary>
		/// <returns>
		/// The task that will complete when they have signed in.
		/// </returns>
		public AuthenticateUIType GetAuthenticateUI (UIContext context, EventHandler<AuthenticatorCompletedEventArgs> completedHandler)
		{
			var auth = GetAuthenticator ();
			if (auth == null) {
				throw new NotSupportedException ("Account authentication in is not supported.");
			}
			auth.Completed += (sender, e) => {
				if (e.IsAuthenticated) {
					AccountStore.Create (context).Save (e.Account, ServiceId);
				}
			};
			if (completedHandler != null) {
				auth.Completed += completedHandler;
			}
			auth.Title = Title;
			return auth.GetUI (context);
		}
#else
		/// <summary>
		/// Presents the necessary UI for the user to sign in to their account.
		/// </summary>
		/// <returns>
		/// The task that will complete when they have signed in.
		/// </returns>
		public AuthenticateUIType GetAuthenticateUI (EventHandler<AuthenticatorCompletedEventArgs> completedHandler)
		{
			var auth = GetAuthenticator ();
			if (auth == null) {
				throw new NotSupportedException ("Account authentication in is not supported.");
			}
			auth.Completed += (sender, e) => {
				if (e.IsAuthenticated) {
					AccountStore.Create ().Save (e.Account, ServiceId);
				}
			};
			if (completedHandler != null) {
				auth.Completed += completedHandler;
			}
			auth.Title = Title;
			return auth.GetUI ();
		}
#endif
		#endregion


		#region Sharing

		public int MaxTextLength { get; protected set; }
		public int MaxLinks { get; protected set; }
		public int MaxImages { get; protected set; }
		public int MaxFiles { get; protected set; }
#if SUPPORT_VIDEO
		public int MaxVideos { get; protected set; }
#endif

		public bool HasMaxTextLength { get { return MaxTextLength < int.MaxValue; } }

		public virtual int GetTextLength (Item item)
		{
			return item.Text.Length;
		}

#if PLATFORM_IOS
		public virtual ShareUIType GetShareUI (Item item, Action<ShareResult> completionHandler)
		{
			return new MonoTouch.UIKit.UINavigationController (new ShareViewController (this, item, completionHandler));
		}
#elif PLATFORM_ANDROID
		public virtual ShareUIType GetShareUI (UIContext context, Item item, Action<ShareResult> completionHandler)
		{
			var intent = new Android.Content.Intent (context, typeof (ShareActivity));
			var state = new ShareActivity.State {
				Service = this,
				Item = item,
				CompletionHandler = completionHandler,
			};
			intent.PutExtra ("StateKey", ShareActivity.StateRepo.Add (state));
			return intent;
		}
#else
		public virtual ShareUIType GetShareUI (Item item, Action<ShareResult> completionHandler)
		{
			throw new NotImplementedException ("Share not implemented on this platform.");
		}
#endif

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
		public Task ShareItemAsync (Item item, Account account)
		{
			return ShareItemAsync (item, account, CancellationToken.None);
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
		/// <param name='cancellationToken'>
		/// Token used to cancel this operation.
		/// </param>
		public virtual Task ShareItemAsync (Item item, Account account, CancellationToken cancellationToken)
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
#else
			RegisterService (Twitter = new Xamarin.Social.Services.TwitterService ());
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

