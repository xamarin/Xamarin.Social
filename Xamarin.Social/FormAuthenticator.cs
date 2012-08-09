using System;
using System.Collections.Generic;

namespace Xamarin.Social
{
	/// <summary>
	/// An authenticator that presents a form to the user.
	/// </summary>
	public abstract class FormAuthenticator
	{
		protected IList<FormAuthenticatorField> Fields { get; private set; }

		protected abstract Account OnSignIn ();
	}

	/// <summary>
	/// Account credential form field.
	/// </summary>
	public class FormAuthenticatorField
	{
		public string Key { get; private set; }
		public string DisplayName { get; set; }
		public string Placeholder { get; set; }
		public string Value { get; set; }

		public FormAuthenticatorField (string key, string displayName, FormAuthenticatorFieldType fieldType, string placeholder = "", string defaultValue = "")
		{
			if (string.IsNullOrWhiteSpace (key)) {
				throw new ArgumentException ("key must not be blank", "key");
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

