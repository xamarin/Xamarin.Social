using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MonoTouch.Social;
using MonoTouch.Accounts;
using Xamarin.Auth;

namespace Xamarin.Social.Services
{
	public class Twitter6Service : SocialService
	{
		public string ConsumerKey { get; set; }
		public string ConsumerSecret { get; set; }

		public Twitter6Service ()
			: base ("Twitter", "Twitter", SLServiceKind.Twitter, ACAccountType.Twitter)
		{
		}

		public override Task<string> GetOAuthTokenAsync (Account acc)
		{
			var tcs = new TaskCompletionSource<string> ();

			new ReverseAuthRequest (acc, ConsumerKey, ConsumerSecret)
				.GetResponseAsync ()
				.ContinueWith (t => {
					if (t.IsCanceled) {
						tcs.SetCanceled ();
						return;
					} else if (t.IsFaulted) {
						tcs.SetException (t.Exception);
						return;
					}

					var parameters = new Dictionary<string, string> {
						{ "x_reverse_auth_target", ConsumerKey },
						{ "x_reverse_auth_parameters", t.Result.GetResponseText () }
					};

					this.CreateRequest ("GET", new Uri ("https://api.twitter.com/oauth/access_token"), parameters, acc)
						.GetResponseAsync ()
						.ContinueWith (tokenTask => {

						if (tokenTask.IsCanceled) {
							tcs.SetCanceled ();
						} else if (tokenTask.IsFaulted) {
							tcs.SetException (tokenTask.Exception);
						} else {
							tcs.SetResult (tokenTask.Result.GetResponseText ());
						}
					});
				});

			return tcs.Task;
		}

		class ReverseAuthRequest : OAuth1Request {
			private string consumerKey;
			private string consumerSecret;

			public ReverseAuthRequest (Account account, string consumerKey, string consumerSecret)
				: base (
					"GET",
					new Uri ("https://api.twitter.com/oauth/request_token"),
					new Dictionary<string, string> { { "x_auth_mode", "reverse_auth" } },
					account
				)
			{
				this.consumerKey = consumerKey;
				this.consumerSecret = consumerSecret;
			}

			protected override string GetAuthorizationHeader ()
			{
				return OAuth1.GetAuthorizationHeader (
					Method,
					Url,
					new Dictionary<string, string> (Parameters),
					this.consumerKey,
					this.consumerSecret,
					string.Empty,
					string.Empty
				);
			}
		}
	}
}