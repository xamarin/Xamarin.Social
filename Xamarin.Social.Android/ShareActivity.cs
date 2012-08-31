using System;
using Android.App;
using Android.OS;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.Widget;
using System.Linq;
using Android.Views;
using Android.Util;
using Android.Graphics;
using Android.Graphics.Drawables;

namespace Xamarin.Social
{
	[Activity (Label = "Share")]
	public class ShareActivity : Activity
	{
		LinearLayout layout;
		TextView acctPicker;
		EditText composer;

		internal class State : Java.Lang.Object
		{
			public Service Service;
			public Item Item;
			public Action<ShareResult> CompletionHandler;

			public List<Account> Accounts;
			public Account ActiveAccount;
		}
		internal static readonly ActivityStateRepository<State> StateRepo = new ActivityStateRepository<State> ();

		State state;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			//
			// Load the state either from a configuration change or from the intent.
			//
			state = LastNonConfigurationInstance as State;
			if (state == null && Intent.HasExtra ("StateKey")) {
				var stateKey = Intent.GetStringExtra ("StateKey");
				state = StateRepo.Remove (stateKey);
			}
			if (state == null) {
				Finish ();
				return;
			}

			//
			// Build the UI or fetch account then build the UI
			//
			BuildUI (savedInstanceState);

			if (state.Accounts == null) {
				state.Service.GetAccountsAsync (this).ContinueWith (t => {
					if (t.IsFaulted) {
						this.ShowError ("Share Error", t.Exception);
					}
					else {
						state.Accounts = t.Result;
						state.ActiveAccount = state.Accounts.FirstOrDefault ();
						UpdateAccountUI ();
					}
				}, TaskScheduler.FromCurrentSynchronizationContext ());
			}
		}

		void BuildUI (Bundle savedInstanceState)
		{
			var labelTextSize = 28;
			var composeTextSize = 24;
			var hMargin = 24;

			Title = state.Service.ShareTitle;
			layout = new LinearLayout (this) {
				Orientation = Orientation.Vertical,
			};
			layout.SetBackgroundColor (Color.White);
			SetContentView (layout);

			//
			// Toolbar
			//

			//
			// Account
			//
			var acctLabel = new TextView (this) {
				Text = "From",
				LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent) {
					RightMargin = 12,
				},
			};
			acctLabel.SetTextColor (Color.DarkGray);
			acctLabel.SetTextSize (ComplexUnitType.Sp, labelTextSize);

			acctPicker = new TextView (this) {
				Text = state.ActiveAccount != null ? state.ActiveAccount.Username : "",
				Typeface = Typeface.DefaultFromStyle (TypefaceStyle.Bold),
			};
			acctPicker.SetTextColor (Color.Black);
			acctPicker.SetTextSize (ComplexUnitType.Sp, labelTextSize);
			acctPicker.Click += PickAccount;

			var acctLayout = new LinearLayout (this) {
				Orientation = Orientation.Horizontal,
				LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent) {
					TopMargin = 36,
					LeftMargin = hMargin,
					RightMargin = hMargin,
				},
			};
			acctLayout.SetGravity (GravityFlags.Center);
			acctLayout.AddView (acctLabel);
			acctLayout.AddView (acctPicker);

			layout.AddView (acctLayout);

			//
			// Composer
			//
			composer = new EditText (this) {
				Id = 301,
				Text = savedInstanceState != null ? savedInstanceState.GetString ("ComposerText") : state.Item.Text,
				LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent) {
					TopMargin = 24,
					LeftMargin = hMargin,
					RightMargin = hMargin,
				},
			};
			composer.SetTextSize (ComplexUnitType.Sp, composeTextSize);

			layout.AddView (composer);
		}

		public override Java.Lang.Object OnRetainNonConfigurationInstance ()
		{
			return state;
		}

		protected override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);

			outState.PutString ("ComposerText", composer.Text);
		}

		void PickAccount (object sender, EventArgs e)
		{

		}

		void UpdateAccountUI ()
		{
			acctPicker.Text = state.ActiveAccount != null ? state.ActiveAccount.Username : "";
		}
	}
}

