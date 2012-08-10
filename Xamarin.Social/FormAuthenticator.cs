using System;
using System.Collections.Generic;
using System.Linq;

#if PLATFORM_IOS
using UIContext = MonoTouch.UIKit.UIViewController;
#else
using UIContext = System.Object;
#endif

namespace Xamarin.Social
{
	/// <summary>
	/// An authenticator that presents a form to the user.
	/// </summary>
	public abstract class FormAuthenticator : Authenticator
	{
		public IList<FormAuthenticatorField> Fields { get; private set; }

		public FormAuthenticator ()
		{
			Fields = new List<FormAuthenticatorField> ();
		}

		public string GetFieldValue (string key) {
			var f = Fields.FirstOrDefault (x => x.Key == key);
			return (f != null) ? f.Value : null;
		}

		public abstract void OnSignIn ();

		protected override void PresentUI (UIContext context)
		{
#if PLATFORM_IOS
			context.PresentModalViewController (new MonoTouch.UIKit.UINavigationController (
				new FormAuthenticatorController (this)), true);
#else
			throw new System.NotImplementedException ("This platform does not support web authentication.");
#endif
		}
	}

	/// <summary>
	/// Account credential form field.
	/// </summary>
	public class FormAuthenticatorField
	{
		public string Key { get; set; }
		public string DisplayName { get; set; }
		public string Placeholder { get; set; }
		public string Value { get; set; }

		public FormAuthenticatorField (string key, string displayName, FormAuthenticatorFieldType fieldType, string placeholder = "", string defaultValue = "")
		{
			if (string.IsNullOrWhiteSpace (key)) {
			}
			Key = key;

			if (string.IsNullOrWhiteSpace (displayName)) {
				throw new ArgumentException ("displayName must not be blank", "key");
			}
			DisplayName = displayName;

			Placeholder = placeholder ?? "";

			Value = defaultValue ?? "";
		}
	}

	/// <summary>
	/// The display type of a credential field.
	/// </summary>
	public enum FormAuthenticatorFieldType
	{
		/// <summary>
		/// The field is visible plain text.
		/// </summary>
		PlainText,

		/// <summary>
		/// The field is protected from onlookers.
		/// </summary>
		Password,
	}
}

