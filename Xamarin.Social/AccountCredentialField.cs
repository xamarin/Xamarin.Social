using System;

namespace Xamarin.Social
{
	/// <summary>
	/// Account credential form field.
	/// </summary>
	public class AccountCredentialField
	{
		public string Key { get; private set; }
		public string DisplayName { get; set; }
		public string Placeholder { get; set; }
		public string Value { get; set; }

		public AccountCredentialField (string key, string displayName, AccountCredentialFieldType fieldType, string placeholder = "", string defaultValue = "")
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
	public enum AccountCredentialFieldType
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

