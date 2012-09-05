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
using Android.Content;

namespace Xamarin.Social
{
	[Activity (Label = "Share")]
	public class ShareActivity : Activity
	{
		static Color ToolbarColor = Color.Argb (0xFF, 0xDD, 0xDD, 0xDD);
		static Color AttachmentColor = Color.Argb (0xFF, 0xEE, 0xEE, 0xEE);

		LinearLayout layout;
		TextView acctPicker;
		EditText composer;
		TextView remaining;
		Button sendButton;
		ProgressBar progress;

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
			var labelTextSize = 24;
			var buttonTextSize = 20;
			var composeTextSize = 24;
			var hMargin = 20;

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

			progress = new ProgressBar (this) {
				Indeterminate = true,
				Visibility = state.IsSending ? ViewStates.Visible : ViewStates.Invisible,
				LayoutParameters = new TableRow.LayoutParams (TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent) {
					TopMargin = 2,
					RightMargin = 6,
					Column = 2,
				},
			};

			sendButton = new Button (this) {
				Text = "Send",
				TextSize = buttonTextSize,
				Enabled = !state.IsSending,
				LayoutParameters = new TableRow.LayoutParams (TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent) {
					TopMargin = 2,
					BottomMargin = 2,
					RightMargin = 2,
					Column = 3,
				},
			};
			sendButton.Click += delegate {
				StartSending ();
			};

			var toolbarRow = new TableRow (this) {
			};
			toolbarRow.AddView (title);
			toolbarRow.AddView (progress);
			toolbarRow.AddView (sendButton);

			var toolbar = new TableLayout (this) {
				LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent) {

				},
			};
			toolbar.SetBackgroundColor (ToolbarColor);
			toolbar.SetColumnStretchable (1, true);
			toolbar.SetColumnShrinkable (1, true);

			toolbar.AddView (toolbarRow);
			layout.AddView (toolbar);

			//
			// Scroll content
			//
			var scroller = new ScrollView (this) {
				LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent) {
				},
			};
			var scrollContent = new LinearLayout (this) {
				Orientation = Orientation.Vertical,
				LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent) {
				},
			};
			scroller.ScrollbarFadingEnabled = true;
			scroller.AddView (scrollContent);
			layout.AddView (scroller);

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
					TopMargin = 24,
					LeftMargin = hMargin,
					RightMargin = hMargin,
				},
			};
			acctLayout.SetGravity (GravityFlags.Center);
			acctLayout.AddView (acctLabel);
			acctLayout.AddView (acctPicker);

			scrollContent.AddView (acctLayout);

			//
			// Attachments
			//
			var attachLayout = new LinearLayout (this) {
				Orientation = Orientation.Vertical,
				Visibility = state.Item.HasAttachments ? ViewStates.Visible : ViewStates.Gone,
				LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent) {
					TopMargin = 24,
					LeftMargin = hMargin,
					RightMargin = hMargin,
				},
			};

			foreach (var x in state.Item.Links) {
				attachLayout.AddView (new AttachmentView (this, x.AbsoluteUri));
			}
			foreach (var x in state.Item.Images) {
				attachLayout.AddView (new AttachmentView (this, x.Filename, x.Length));
			}
			foreach (var x in state.Item.Files) {
				attachLayout.AddView (new AttachmentView (this, x.Filename, x.Length));
			}

			scrollContent.AddView (attachLayout);

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
			composer.AfterTextChanged += delegate {
				UpdateRemainingTextUI ();
			};
			scrollContent.AddView (composer);

			remaining = new TextView (this) {
				LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent) {
					TopMargin = 2,
					LeftMargin = hMargin,
					RightMargin = hMargin,
				},
			};
			remaining.SetTextSize (ComplexUnitType.Sp, composeTextSize);
			UpdateRemainingTextUI ();
			scrollContent.AddView (remaining);
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

			state.IsSending = true;

			sendButton.Enabled = false;
			progress.Visibility = ViewStates.Visible;
			composer.Enabled = false;

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
			progress.Visibility = ViewStates.Invisible;
			composer.Enabled = false;
			state.IsSending = false;
		}

		string GetAddAccountTitle ()
		{
			return "Add Account...";
		}

		void PickAccount (object sender, EventArgs e)
		{
			if (state.IsSending) {
				return;
			}

			if (state.Accounts == null) {
				return;
			}

			if (state.Accounts.Count == 0) {
				AddAccount ();
			}
			else {
				var addAccountTitle = GetAddAccountTitle ();
				var items = state.Accounts.Select (x => x.Username).OrderBy (x => x).Concat (new [] { addAccountTitle }).ToArray ();

				var builder = new AlertDialog.Builder (this);
				builder.SetTitle ("Pick an account");
				builder.SetItems (
					items,
					(ds, de) => {
						var item = items [de.Which];
						if (item == addAccountTitle) {
							AddAccount ();
						} else {
							state.ActiveAccount = state.Accounts.FirstOrDefault (x => x.Username == item);
							UpdateAccountUI ();
						}
					});

				var alert = builder.Create ();
				alert.Show ();
			}
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

		void UpdateRemainingTextUI ()
		{
			if (state.Service.HasMaxTextLength) {
				state.Item.Text = composer.Text;
				var rem = state.Service.MaxTextLength - state.Service.GetTextLength (state.Item);
				remaining.Text = rem.ToString ();
				if (rem < 0) {
					remaining.SetTextColor (Color.DarkRed);
				}
				else {
					remaining.SetTextColor (Color.DarkGray);
				}
			}
			else {
				remaining.Text = "";
			}
		}

		class AttachmentView : TableLayout
		{
			public AttachmentView (Context context, string title)
				: this (context, title, 0)
			{
			}

			public AttachmentView (Context context, string title, long size)
				: base (context)
			{
				var textSize = 20;

				var row = new TableRow (context) {
				};
				row.SetBackgroundColor (AttachmentColor);
				AddView (row);

				var tlabel = new TextView (context) {
					Text = title,
					LayoutParameters = new TableRow.LayoutParams (TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent) {
						LeftMargin = 4,
					},
				};
				tlabel.SetTextColor (Color.Black);
				tlabel.SetTextSize (ComplexUnitType.Sp, textSize);
				row.AddView (tlabel);

				if (size > 0) {
					var slabel = new TextView (context) {
						Text = FormatSize (size),
						LayoutParameters = new TableRow.LayoutParams (TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent) {
							LeftMargin = 4,
							RightMargin = 4,
						},
					};
					slabel.SetTextColor (Color.Black);
					slabel.SetTextSize (ComplexUnitType.Sp, textSize);
					row.AddView (slabel);
				}

				SetColumnStretchable (0, true);
				LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent) {
					TopMargin = 2,
				};
			}

			static string FormatSize (long size)
			{
				if (size < 1024) {
					return string.Format ("{0} bytes", size);
				}
				else if (size < 1024 * 1024) {
					return string.Format ("{0} KB", size / 1024);
				}
				else {
					return string.Format ("{0:0.0} MB", size / (1024.0 * 1024.0));
				}
			}
		}
	}
}

