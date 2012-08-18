using System;
using System.Threading;

namespace Xamarin.Social
{
	class ShareProgress
	{
		public ManualResetEvent DoneEvent { get; private set; }
		public ShareResult Result { get; set; }

		public ShareProgress ()
		{
			DoneEvent = new ManualResetEvent (false);
		}
	}
}

