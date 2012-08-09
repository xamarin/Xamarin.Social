using System;

namespace Xamarin.Social
{
	public enum AuthenticationResult
	{
		Success,
		MissingRequiredProperty,
		Failed,
		AlreadyExists,
		Cancelled,
	}
}

