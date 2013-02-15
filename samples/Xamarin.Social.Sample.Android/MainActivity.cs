using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Xamarin.Social.Services;

namespace Xamarin.Social.Sample.Android
{
	[Activity (Label = "Xamarin.Social Sample", MainLauncher = true)]
	public class MainActivity : Activity
	{
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
		};

		void Share (Service service, Button shareButton)
		{
			Item item = new Item {
				Text = "I'm sharing great things using Xamarin!",
				Links = new List<Uri> {
					new Uri ("http://xamarin.com"),
				},
			};

			Intent intent = service.GetShareUI (this, item, shareResult => {
				shareButton.Text = "Share (" + shareResult + ")";
			});

			StartActivity (intent);
		}

		void Authenticate (Service service)
		{
			Intent intent = service.GetAuthenticateUI (this, account => {
				//
			});

			StartActivity (intent);
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);

			var layout = FindViewById<LinearLayout> (Resource.Id.ServicesLayout);

			// Build the UI
			foreach (var s in services) {
				layout.AddView (CreateServiceView (s));
			}
		}

		View CreateServiceView (Service service)
		{
			var layout = new LinearLayout (this) {
				Orientation = Orientation.Vertical,
				LayoutParameters = new LinearLayout.LayoutParams (320, LinearLayout.LayoutParams.WrapContent) {
					BottomMargin = 40,
				},
			};

			var title = new TextView (this) {
				Text = service.GetType ().Name,
				TextSize = 20,
				LayoutParameters = new LinearLayout.LayoutParams (320, LinearLayout.LayoutParams.WrapContent) {
					BottomMargin = 10,
					LeftMargin = 4,
				},
			};
			layout.AddView (title);

			var shareButton = new Button (this) {
				Text = "Share",
			};
			shareButton.Click += delegate {
				Share (service, shareButton);
			};
			layout.AddView (shareButton);

			if (service.SupportsAuthentication) {
				var authButton = new Button (this) {
					Text = "Authenticate",
				};
				authButton.Click += delegate {
					Authenticate (service);
				};
				layout.AddView (authButton);
			}

			return layout;
		}
	}
}


