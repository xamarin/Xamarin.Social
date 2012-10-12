using System;

namespace Xamarin.Social
{
	/// <summary>
	/// The result of the user trying to share an <see cref="Item"/>.
	/// </summary>
	public enum ShareResult
	{
		/// <summary>
		/// The user canceled that share operation. It's possible that they encountered
		/// errors and gave up by choosing to cancel.
		/// </summary>
		Cancelled,

		/// <summary>
		/// The user successfully shared!
		/// </summary>
		Done
	}
}

