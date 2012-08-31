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
using Android.Text;
using Android.Text.Style;
using System.Threading;

namespace Xamarin.Social
{
	[Activity (Label = "Share")]
	public class ShareActivity : Activity
	{
		LinearLayout layout;
		TextView acctPicker;
		EditText composer;
		Button sendButton;

		internal class State : Java.Lang.Object
		{
			public Service Service;
			public Item Item;
			public Action<ShareResult> CompletionHandler;

			public List<Account> Accounts;
			public Account ActiveAccount;

			public bool IsSending;
			public CancellationTokenSource CancelSource;
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
				BeginGetAccounts ();
			}
		}

		void BuildUI (Bundle savedInstanceState)
		{
			var labelTextSize = 28;
			var buttonTextSize = 20;
			var composeTextSize = 24;
			var hMargin = 24;

			RequestWindowFeature (WindowFeatures.NoTitle);

			Title = state.Service.ShareTitle;

			layout = new LinearLayout (this) {
				Orientation = Orientation.Vertical,
			};
			layout.SetBackgroundColor (Color.White);
			SetContentView (layout);

			//
			// Toolbar
			//
			var title = new TextView (this) {
				Text = Title,
				TextSize = composeTextSize,
				LayoutParameters = new TableRow.LayoutParams (TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent) {
					Column = 0,
					TopMargin = 4,
					BottomMargin = 0,
					LeftMargin = 8,
				},
			};
			title.SetTextColor (Color.Black);

			sendButton = new Button (this) {
				Text = "Send",
				TextSize = buttonTextSize,
				Enabled = !state.IsSending,
				LayoutParameters = new TableRow.LayoutParams (TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent) {
					TopMargin = 2,
					BottomMargin = 2,
					RightMargin = 2,
					Column = 2,
				},
			};
			sendButton.Click += delegate {
				StartSending ();
			};

			var toolbarRow = new TableRow (this) {
			};
			toolbarRow.AddView (title);
			toolbarRow.AddView (sendButton);

			var toolbar = new TableLayout (this) {
				LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent) {

				},
			};
			toolbar.SetBackgroundColor (Color.LightGray);
			toolbar.SetColumnStretchable (1, true);
			toolbar.SetColumnShrinkable (1, true);

			toolbar.AddView (toolbarRow);
			layout.AddView (toolbar);

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
				Typeface = Typeface.DefaultFromStyle (TypefaceStyle.Bold),
				Clickable = true,
			};
			acctPicker.SetTextColor (Color.Black);
			acctPicker.SetTextSize (ComplexUnitType.Sp, labelTextSize);
			acctPicker.Click += PickAccount;
			UpdateAccountUI ();

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

		void StartSending ()
		{
			if (state.IsSending) {
				return;
			}

			if (state.ActiveAccount == null) {
				this.ShowError ("Send Error", "You must first choose an account to send from.");
				return;
			}

			sendButton.Enabled = false;
			state.IsSending = true;

			state.Item.Text = composer.Text;

			try {
				state.CancelSource = new CancellationTokenSource ();
				state.Service.ShareItemAsync (state.Item, state.ActiveAccount, state.CancelSource.Token).ContinueWith (task => {
					StopSending ();
					if (task.IsFaulted) {
						this.ShowError ("Send Error", task.Exception);
					}
					else {
						if (state.CompletionHandler != null) {
							state.CompletionHandler (ShareResult.Done);
						}
						SetResult (Result.Ok);
						Finish ();
					}
				}, TaskScheduler.FromCurrentSynchronizationContext ());
			}
			catch (Exception ex) {
				StopSending ();
				this.ShowError ("Send Error", ex);
			}
		}

		void StopSending ()
		{
			state.CancelSource = null;
			sendButton.Enabled = true;
			state.IsSending = false;
		}

		string GetAddAccountTitle ()
		{
			return "Add Account...";
		}

		void PickAccount (object sender, EventArgs e)
		{
			if (state.Accounts == null) {
				return;
			}

			if (state.Accounts.Count == 0) {
				AddAccount ();
			}

			var addAccountTitle = GetAddAccountTitle ();
			var items = state.Accounts.Select (x => x.Username).OrderBy (x => x).Concat (new [] { addAccountTitle }).ToArray ();

			var builder = new AlertDialog.Builder (this);
			builder.SetTitle ("Pick an account");
			builder.SetItems (
				items,
				(ds, de) => {
					var item = items[de.Which];
					if (item == addAccountTitle) {
						AddAccount ();
					}
					else {
						state.ActiveAccount = state.Accounts.FirstOrDefault (x => x.Username == item);
						UpdateAccountUI ();
					}
				});
			var alert = builder.Create ();

			alert.Show ();
		}

		void AddAccount ()
		{
			var intent = state.Service.GetAuthenticateUI (this, account => {
				if (account != null) {
					BeginGetAccounts ();
				}
			});
			StartActivity (intent);
		}

		void BeginGetAccounts ()
		{
			state.Service.GetAccountsAsync (this).ContinueWith (t => {
				if (t.IsFaulted) {
					this.ShowError ("Share Error", t.Exception);
				}
				else {
					state.Accounts = t.Result;
					if (state.ActiveAccount == null) {
						state.ActiveAccount = state.Accounts.FirstOrDefault ();
					}
					UpdateAccountUI ();
				}
			}, TaskScheduler.FromCurrentSynchronizationContext ());
		}

		void UpdateAccountUI ()
		{
			var text = state.ActiveAccount != null ? state.ActiveAccount.Username : GetAddAccountTitle ();

			var content = new SpannableString (text);
			content.SetSpan (new UnderlineSpan (), 0, text.Length, (SpanTypes)0);
			acctPicker.SetText (content, TextView.BufferType.Spannable);
		}
	}
}

