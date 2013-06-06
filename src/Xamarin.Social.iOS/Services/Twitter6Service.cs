using System;
using System.Threading.Tasks;
using MonoTouch.Accounts;
using MonoTouch.Social;
using Xamarin.Auth;
using Xamarin.Utilities;
using System.Collections.Generic;

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

		public override Task<IDictionary<string, string>> GetAccessTokenAsync (Account acc)
		{
			if (string.IsNullOrEmpty (ConsumerKey) || string.IsNullOrEmpty (ConsumerSecret))
				throw new InvalidOperationException ("Cannot perform Twitter Reverse Auth without ConsumerKey and ConsumerSecret set.");

			return new ReverseAuthRequest (acc, ConsumerKey, ConsumerSecret)
				.GetResponseAsync ()
				.ContinueWith (t => {
					var parameters = new Dictionary<string, string> {
						{ "x_reverse_auth_target", ConsumerKey },
						{ "x_reverse_auth_parameters", t.Result.GetResponseText () }
					};

					return this.CreateRequest ("GET", new Uri ("https://api.twitter.com/oauth/access_token"), parameters, acc)
						.GetResponseAsync ()
						.ContinueWith (tokenTask => {
							return WebEx.FormDecode (tokenTask.Result.GetResponseText ());
						});
				}).Unwrap ();
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
					string.Empty);
			}
		}
	}
}