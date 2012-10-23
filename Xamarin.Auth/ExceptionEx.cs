using System;

namespace Xamarin.Utilities
{
	static class ExceptionEx
	{
		public static string GetUserMessage (this Exception error)
		{
			var e = error;
			while (e.InnerException != null) {
				e = e.InnerException;
			}
			return e.Message;
		}
	}
}

