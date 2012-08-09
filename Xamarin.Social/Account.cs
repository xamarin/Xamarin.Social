using System;

namespace Xamarin.Social
{
	/// <summary>
	/// An Account that reprents and authenticated user of a social network.
	/// </summary>
	public abstract class Account
	{
		public abstract string Username { get; }

		public Account ()
		{
		}
	}
}

