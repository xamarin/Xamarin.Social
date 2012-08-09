using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public string ServiceType { get; private set; }

		protected Service (string serviceType)
		{
			if (string.IsNullOrWhiteSpace (serviceType)) {
				throw new ArgumentException ("serviceType must be a non-blank string", "serviceType");
			}
			ServiceType = serviceType;
		}


		#region Service Information

		/// <summary>
		/// Description of the social network service.
		/// </summary>
		public virtual string Description { get { return ""; } }

		/// <summary>
		/// Text to display on the button to sign up.
		/// </summary>
		public virtual string SignUpTitle { get { return ""; } }

		/// <summary>
		/// Link to sign up.
		/// </summary>
		public virtual Uri SignUpLink { get { return null; } }

		#endregion


		#region Authentication

		/// <summary>
		/// Whether this instance has any saved accounts.
		/// </summary>
		public virtual bool HasSavedAccounts { get { return false; } }

		/// <summary>
		/// Gets the saved accounts associated with this service.
		/// </summary>
		public virtual Task<IEnumerable<Account>> GetSavedAccountsAsync ()
		{
			return Task.Factory.StartNew (() => Enumerable.Empty<Account> ());
		}

		/// <summary>
		/// Gets the authenticator for this service. The authenticator will present
		/// the user interface needed to authenticate a new account for the service.
		/// This account will then be saved.
		/// </summary>
		/// <returns>
		/// The authenticator or null if authentication is not supported.
		/// </returns>
		/// <param name='parameters'>
		/// Set of keys and values required by the authenticator. If required parameters
		/// are not supplied, an exception will be thrown detailing which ones need to be
		/// provided.
		/// </param>
		protected virtual Authenticator GetAuthenticator (IDictionary<string, string> parameters)
		{
			return null;
		}

		/// <summary>
		/// Presents the necessary UI for the user to sign in to their account.
		/// </summary>
		/// <returns>
		/// The task that will complete when they have signed in.
		/// </returns>
		public virtual Task<AuthenticationResult> AddAccountAsync (UIContext context, IDictionary<string, string> authenticaionParameters)
		{
			var auth = GetAuthenticator (authenticaionParameters);
			if (auth == null) {
				throw new NotSupportedException ("Account sign in is not supported.");
			}
			return auth.AuthenticateAsync (context).ContinueWith (task => {
				return task.Result;
			});
		}

		#endregion


		#region Sharing

		public virtual bool CanShareText { get { return false; } }
		public virtual bool CanShareLinks { get { return false; } }
		public virtual bool CanShareImages { get { return false; } }
		public virtual bool CanShareFiles { get { return false; } }
#if SUPPORT_VIDEO
		public virtual bool CanShareVideo { get { return false; } }
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
		public abstract Task<ShareResult> ShareAsync (Item item);

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
		/// <param name='account'>
		/// The optional Account to associate with the request.
		/// </param>
		public abstract Request CreateRequest (string method, Uri url, IDictionary<string, string> paramters = null);

		#endregion


		#region Service Registry

		static Dictionary<string, Service> registry;

		static Service ()
		{
			registry = new Dictionary<string, Service> ();

			RegisterService (Facebook = new FacebookService ());
			//RegisterService (new GoogleService ());


#if PLATFORM_IOS
			RegisterService (Twitter = new TwitterService5 ());
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
				if (registry.TryGetValue (service.ServiceType, out s)) {
					throw new ArgumentException ("Service '" + service.ServiceType + "' is already registered.", "service");
				}
				else {
					registry.Add (service.ServiceType, service);
				}
			}
		}

		public static Service Twitter { get; private set; }
		public static Service Facebook { get; private set; }
		//public static Service Google { get; private set; }
		//public static Service SinaWeibo { get; private set; }


		#endregion
	}
}

