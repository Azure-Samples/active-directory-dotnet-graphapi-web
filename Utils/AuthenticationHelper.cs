// Copyright © Microsoft
//
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION
// ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A
// PARTICULAR PURPOSE, MERCHANTABILITY OR NON-INFRINGEMENT.
//
// See the Apache License, Version 2.0 for the specific language
// governing permissions and limitations under the License.

namespace AadExplorer.Utils
{
    using System;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// Helpers for getting access token for app only scenario.
    /// </summary>
    public class AuthenticationHelper
    {
        /// <summary>
        /// Caches the information if the current logged in user has been authenticated.
        /// </summary>
        public static bool IsAuthenticated;

        /// <summary>
        /// Cache for the app only access token.
        /// </summary>
        public static AccessTokenCookie AppOnlyAccessTokenCookie;

        /// <summary>
        /// Helper to get and store the app only access token.
        /// </summary>
        internal static void GetAppOnlyAccessToken()
        {
            AuthenticationContext ac = new AuthenticationContext(
                String.Format(Constants.AuthorityUrlFormat, Constants.LoginDomainName, Constants.ApplicationDomainName));
            ClientCredential clientCredential = new ClientCredential(Constants.AppPrincipalId, Constants.AppKey);
            AuthenticationResult authenticationResult = ac.AcquireToken("https://" + Constants.GraphDomainName, clientCredential);
            AccessTokenCookie tokenCookie = new AccessTokenCookie(
                authenticationResult.AccessToken, null, authenticationResult.RefreshToken);
            AppOnlyAccessTokenCookie = tokenCookie;
            IsAuthenticated = true;
        }
    }
}