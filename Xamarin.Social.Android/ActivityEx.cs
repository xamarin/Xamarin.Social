using System;
using Android.App;

namespace Xamarin.Social
{
	static class ActivityEx
	{
		public static void ShowError (this Activity activity, Exception exception)
		{
			var ex = exception;
			while (ex.InnerException != null) {
				ex = ex.InnerException;
			}

			var b = new AlertDialog.Builder (activity);
			b.SetMessage (ex.ToString ());
			b.SetTitle (ex.GetType ().Name);
			var alert = b.Create ();
			alert.Show ();
		}
	}
}

