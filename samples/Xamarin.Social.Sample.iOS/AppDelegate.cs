using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using Xamarin.Social.Services;

namespace Xamarin.Social.Sample.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		UIWindow window;
		DialogViewController dialog;

		List<Service> services = new List<Service> {

			new FacebookService {
				ClientId = "App ID/API Key from https://developers.facebook.com/apps",
				RedirectUrl = new Uri ("Redirect URL from https://developers.facebook.com/apps")
			},

			new FlickrService {
				ConsumerKey = "Key from http://www.flickr.com/services/apps/by/me",
				ConsumerSecret = "Secret from http://www.flickr.com/services/apps/by/me",
				CallbackUrl = new Uri ("Callback URL from http://www.flickr.com/services/apps/by/me")
			},

			new TwitterService {
				ConsumerKey = "Consumer key from https://dev.twitter.com/apps",
				ConsumerSecret = "Consumer secret from https://dev.twitter.com/apps",
				CallbackUrl = new Uri ("Callback URL from https://dev.twitter.com/apps")
			},

			new Twitter5Service {
			},
		};

		void Share (Service service, Section section)
		{
			Item item = new Item {
				Text = "I'm sharing great things using Xamarin!",
				Links = new List<Uri> {
					new Uri ("http://xamarin.com"),
				},
			};

			UIViewController vc = service.GetShareUI (item, shareResult => {
				dialog.DismissViewController (true, null);

				var shareButton = section.Elements[0].GetActiveCell ();
				shareButton.TextLabel.Text = "Share (" + shareResult + ")";
			});
			dialog.PresentViewController (vc, true, null);
		}

		void Authenticate (Service service, Section section)
		{
			UIViewController vc = service.GetAuthenticateUI (account => {
				dialog.DismissViewController (true, null);
			});
			dialog.PresentViewController (vc, true, null);
		}

		Section CreateSectionForService (Service service)
		{
			var section = new Section (service.GetType ().Name);

			var shareButton = new StringElement ("Share");
			shareButton.Tapped += delegate {
				Share (service, section);
			};
			section.Add (shareButton);

			if (service.SupportsAuthentication) {
				var authButton = new StringElement ("Authenticate");
				authButton.Tapped += delegate {
					Authenticate (service, section);
				};
				section.Add (authButton);
			}

			return section;
		}

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			var root = new RootElement ("Xamarin.Social Sample");

			root.Add (services.Select (CreateSectionForService));

			dialog = new DialogViewController (root);

			window = new UIWindow (UIScreen.MainScreen.Bounds);
			window.RootViewController = new UINavigationController (dialog);
			window.MakeKeyAndVisible ();
			
			return true;
		}

		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			UIApplication.Main (args, null, "AppDelegate");
		}
	}
}

