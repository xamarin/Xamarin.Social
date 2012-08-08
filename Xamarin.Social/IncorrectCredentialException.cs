using System;

namespace Xamarin.Social
{
	/// <summary>
	/// The provided credentials do not authenticate.
	/// </summary>
	public class IncorrectCredentialException : Exception
	{
		public IncorrectCredentialException ()
		{
		}

		public IncorrectCredentialException (string message)
			: base (message)
		{
		}

		public IncorrectCredentialException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
	}
}

