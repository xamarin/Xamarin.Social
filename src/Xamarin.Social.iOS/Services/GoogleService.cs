using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Utilities;

namespace Xamarin.Social.Services
{
    public class GoogleService : OAuth2Service
    {
        public GoogleService ()
            : base ("Google", "Google")
        {
            AuthorizeUrl = new Uri ("https://accounts.google.com/o/oauth2/auth");
            AccessTokenUrl = new Uri ("https://accounts.google.com/o/oauth2/token");
            Scope = "https://www.googleapis.com/auth/plus.login";
        }

        protected override Authenticator GetAuthenticator ()
        {
            return new OAuth2Authenticator (ClientId, ClientSecret, Scope, AuthorizeUrl, RedirectUrl, AccessTokenUrl, GetUsernameAsync, ResponseFormat.Json);
        }

        protected override Task<string> GetUsernameAsync (IDictionary<string, string> accountProperties)
        {
            var request = base.CreateRequest ("GET", new Uri ("https://www.googleapis.com/plus/v1/people/me"), new Dictionary<string, string> {
                { "fields", "url,id" }
            }, new Account (string.Empty, accountProperties));

            return request.GetResponseAsync ().ContinueWith (reqTask => {
                var responseText = reqTask.Result.GetResponseText ();
                var urlFromJson = WebEx.GetValueFromJson (responseText, "url");

                var prefixes = new [] {
                    "https://plus.google.com/+",
                    "https://plus.google.com/"
                };

                foreach (var prefix in prefixes) {
                    if (urlFromJson.StartsWith (prefix))
                        return urlFromJson.Substring (prefix.Length);
                }

                return WebEx.GetValueFromJson (responseText, "id");
            });
        }
    }
}
