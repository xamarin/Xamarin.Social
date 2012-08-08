using System;

namespace Xamarin.Social
{
	/// <summary>
	/// The credentials provided are incomplete - some essential information is missing.
	/// </summary>
	public class IncompleteCredentialException : Exception
	{
		public IncompleteCredentialException ()
		{
		}

		public IncompleteCredentialException (string message)
			: base (message)
		{
		}

		public IncompleteCredentialException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
	}
}

