//
//  Copyright 2012-2013, Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xamarin.Auth;

namespace Xamarin.Social
{
	/// <summary>
	/// Represents an OAuth 2 based service.
	/// </summary>
	public abstract class OAuth2Service : Service
	{
		/// <summary>
		/// Gets or sets the client identifier.
		/// </summary>
		/// <value>The client identifier.</value>
		/// <remarks>http://tools.ietf.org/html/rfc6749#section-2.2</remarks>
		public string ClientId { get; set; }

		/// <summary>
		/// Gets or sets the client secret.
		/// </summary>
		/// <value>The client identifier.</value>
		/// <remarks>http://tools.ietf.org/html/rfc6749#section-4.1</remarks>
		public string ClientSecret { get; set; }

		/// <summary>
		/// Gets or sets the scope of the access token.
		/// </summary>
		/// <remarks>
		/// <para>http://tools.ietf.org/html/rfc6749#section-3.3</para>
		/// <para>
		/// When creating your own <see cref="OAuth2Service"/>, the available options for scopes
		/// will be listed in the documentation for the service you're implementing.
		/// </para>
		/// </remarks>
		public string Scope { get; set; }
		
		public Uri AuthorizeUrl { get; set; }
		public Uri RedirectUrl { get; set; }
		public Uri AccessTokenUrl { get; set; }
		
		public OAuth2Service (string serviceId, string title)
			: base (serviceId, title)
		{
			//
			// This is a reliable URL to redirect to
			//
			RedirectUrl = new Uri ("http://www.facebook.com/connect/login_success.html");
		}
		
		protected override Authenticator GetAuthenticator ()
		{
			if (string.IsNullOrEmpty (ClientId)) {
				throw new InvalidOperationException ("Client ID not specified.");
			}
			
			if (string.IsNullOrEmpty (Scope)) {
				throw new InvalidOperationException ("Scope not specified.");
			}
			
			if (AuthorizeUrl == null) {
				throw new InvalidOperationException ("Authorize URL not specified.");
			}
			
			if (RedirectUrl == null) {
				throw new InvalidOperationException ("Redirect URL not specified.");
			}

			if (AccessTokenUrl != null) {
				return new OAuth2Authenticator (
					clientId: ClientId,
					clientSecret: ClientSecret,
					scope: Scope,
					authorizeUrl: AuthorizeUrl,
					redirectUrl: RedirectUrl,
					accessTokenUrl: AccessTokenUrl,
					getUsernameAsync: GetUsernameAsync);
			} else {
				return new OAuth2Authenticator (
					clientId: ClientId,
					scope: Scope,
					authorizeUrl: AuthorizeUrl,
					redirectUrl: RedirectUrl,
					getUsernameAsync: GetUsernameAsync);
			}
		}

		protected abstract Task<string> GetUsernameAsync (IDictionary<string, string> accountProperties);

		public override Request CreateRequest (string method, Uri url, IDictionary<string, string> parameters, Account account)
		{
			return new OAuth2Request (method, url, parameters, account) {
				Account = account,
			};
		}
	}
}

