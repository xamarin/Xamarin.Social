using System;
using MonoTouch.UIKit;

namespace Xamarin.Social
{
	public class FormAuthenticatorController : UITableViewController
	{
		FormAuthenticator authenticator;

		public FormAuthenticatorController (FormAuthenticator authenticator)
		{
			this.authenticator = authenticator;

			Title = authenticator.Title;
		}
	}
}

