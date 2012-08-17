using System;
using MonoTouch.UIKit;

namespace Xamarin.Social
{
	public class FormAuthenticatorController : UITableViewController
	{
		FormAuthenticator authenticator;

		public FormAuthenticatorController (FormAuthenticator authenticator)
			: base (UITableViewStyle.Grouped)
		{
			this.authenticator = authenticator;

			Title = authenticator.Service.Title;

			TableView.DataSource = new FormDataSource (this);
		}

		class FormDataSource : UITableViewDataSource
		{
			FormAuthenticatorController controller;

			public FormDataSource (FormAuthenticatorController controller)
			{
				this.controller = controller;
			}

			public override int NumberOfSections (UITableView tableView)
			{
				return 3;
			}

			public override int RowsInSection (UITableView tableView, int section)
			{
				if (section == 0) {
					return controller.authenticator.Fields.Count;
				}
				else {
					return 1;
				}
			}

			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				if (indexPath.Section == 0) {

					var field = controller.authenticator.Fields[indexPath.Row];

					var cell = tableView.DequeueReusableCell ("C");
					if (cell == null) {
						cell = new UITableViewCell (UITableViewCellStyle.Default, "C");
						cell.SelectionStyle = UITableViewCellSelectionStyle.None;
					}

					cell.TextLabel.Text = field.Title;

					return cell;
				}
				else if (indexPath.Section == 1) {
					var cell = tableView.DequeueReusableCell ("SignIn");
					if (cell == null) {
						cell = new UITableViewCell (UITableViewCellStyle.Default, "SignIn");
						cell.TextLabel.TextAlignment = UITextAlignment.Center;
					}

					cell.TextLabel.Text = "Sign In";

					return cell;
				}
				else {
					var cell = tableView.DequeueReusableCell ("CreateAccount");
					if (cell == null) {
						cell = new UITableViewCell (UITableViewCellStyle.Default, "CreateAccount");
						cell.TextLabel.TextAlignment = UITextAlignment.Center;
					}

					cell.TextLabel.Text = "Create Account";

					return cell;
				}
			}
		}
	}
}

