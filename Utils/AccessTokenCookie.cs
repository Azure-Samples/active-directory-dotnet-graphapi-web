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
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;
    using System.Linq;
    using System.Text;
    using System.Web;
    using Newtonsoft.Json;
    using ADAL = Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// Access token cookie.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AccessTokenCookie
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccessTokenCookie"/> class.
        /// </summary>
        public AccessTokenCookie()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessTokenCookie"/> class.
        /// </summary>
        /// <param name="accessToken">Access token.</param>
        /// <param name="idToken">ID token.</param>
        /// <param name="refreshToken">Refresh token.</param>
        public AccessTokenCookie(string accessToken, string idToken, string refreshToken)
        {
            this.AccessToken = accessToken;
            this.IdToken = idToken;
            this.RefreshToken = refreshToken;

            this.JwtAccessToken = new JwtSecurityToken(accessToken);
            if (!string.IsNullOrEmpty(idToken))
            {
                this.JwtIdentityToken = new JwtSecurityToken(idToken);
            }
        }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        [JsonProperty]
        public string IdToken { get; set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        [JsonProperty]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        [JsonProperty]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the Jwt access token.
        /// </summary>
        public JwtSecurityToken JwtAccessToken { get; set; }

        /// <summary>
        /// Gets or sets the Jwt identity token.
        /// </summary>
        public JwtSecurityToken JwtIdentityToken { get; set; }

        /// <summary>
        /// Get the JSON representation of the object.
        /// </summary>
        /// <returns>Encoded json string.</returns>
        public string ToCookieString()
        {
            string jsonString = JsonConvert.SerializeObject(this);
            byte[] cookieBlob = Encoding.UTF8.GetBytes(jsonString);
            return Convert.ToBase64String(cookieBlob);
        }

        /// <summary>
        /// Parses the access token cookie.
        /// </summary>
        /// <param name="accessTokenCookie">Token string.</param>
        /// <returns>Parsed access token.</returns>
        public static AccessTokenCookie Parse(string accessTokenCookie)
        {
            if (String.IsNullOrEmpty(accessTokenCookie))
            {
                return null;
            }

            try
            {
                byte[] cookieBlob = Convert.FromBase64String(accessTokenCookie);
                string decodedCookieBlob = Encoding.UTF8.GetString(cookieBlob);

                AccessTokenCookie tokenCookie =
                    JsonConvert.DeserializeObject<AccessTokenCookie>(decodedCookieBlob);

                tokenCookie.JwtAccessToken = new JwtSecurityToken(tokenCookie.AccessToken);
                if (!string.IsNullOrEmpty(tokenCookie.IdToken))
                {
                    tokenCookie.JwtIdentityToken = new JwtSecurityToken(tokenCookie.IdToken);
                }

                return tokenCookie;
            }
            catch (FormatException formatException)
            {
                Logger.Error(formatException.ToString());
                return null;
            }
            catch (ArgumentException argumentException)
            {
                Logger.Error(argumentException.ToString());
                return null;
            }
        }

        /// <summary>
        /// Gets the current access token from the http context.
        /// </summary>
        /// <returns>Current access token cookie.</returns>
        public static AccessTokenCookie GetCurrent()
        {
            if (HttpContext.Current.Items[Constants.AccessTokenCookieName] != null)
            {
                return HttpContext.Current.Items[Constants.AccessTokenCookieName] as AccessTokenCookie;
            }
            
            return AuthenticationHelper.AppOnlyAccessTokenCookie;
        }
    }
}