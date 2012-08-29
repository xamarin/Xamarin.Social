using System;

namespace Xamarin.Social
{
	public class SocialException : Exception
	{
		public SocialException (string message)
			: base (message)
		{
		}

		public SocialException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
	}
}

