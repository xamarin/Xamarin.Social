using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Xamarin.Social.Services;
using System.Net;
using Android.NUnit;

namespace Xamarin.Social.Android.Test
{
	[TestFixture]
	public class TwitterTest
	{
		Service CreateService ()
		{
			return Service.Twitter;
		}
		
		[Test]
		public void Manual_Authenticate ()
		{
			var service = CreateService ();

			var ui = service.GetAuthenticateUI (result => {
				Console.WriteLine ("AUTHENTICATE RESULT = " + result);
				TestRunner.Shared.FinishActivity (42);
			});
			TestRunner.Shared.StartActivityForResult (ui, 42);
		}
	}
}
