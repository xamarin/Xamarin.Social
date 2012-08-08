using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		/// Gets the OS account associated with this service
		/// </summary>
		/// <value>
		/// The OS account or null if there is no OS account.
		/// </value>
		public virtual Account OSAccount { get { return null; } }

		/// <summary>
		/// Gets a blank credential that the user can fill in to authenticate this service.
		/// </summary>
		/// <returns>
		/// The credential with blank fields.
		/// </returns>
		public abstract AccountCredential GetBlankCredential ();

		/// <summary>
		/// Authenticates the passed in credential.
		/// </summary>
		/// <returns>
		/// The authenticated Account. Throws IncompleteCredentialException is the credential
		/// is not completely filled out. Throws IncorrectCredentialException if authentication
		/// failed.
		/// </returns>
		/// <param name='credential'>
		/// Credential used to authenticate the account.
		/// </param>
		public abstract Task<Account> AuthenticateAsync (AccountCredential credential);

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
		/// Shares the passed-in object using the given account.
		/// </summary>
		/// <remarks>
		/// Must be called from the UI thread because this will cause a dialog to be displayed
		/// allowing the user to edit the item before it is .
		/// </remarks>			
		/// <param name='item'>
		/// The item to share.
		/// </param>
		/// <param name='account'>
		/// The account that is sharing the item. Pass null to use the OS's builtin account.
		/// </param>
		public abstract Task<ShareResult> ShareAsync (Item item, Account account = null);

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

			//RegisterService (Facebook = new FacebookService ());
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

		//public static Service Facebook { get; private set; }
		//public static Service Google { get; private set; }
		//public static Service SinaWeibo { get; private set; }
		public static Service Twitter { get; private set; }

		#endregion
	}
}

