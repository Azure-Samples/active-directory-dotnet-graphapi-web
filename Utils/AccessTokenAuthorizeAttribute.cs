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
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// Authorize that the request has a valid AccessToken.
    /// </summary>
    public class AccessTokenAuthorizeAttribute : AuthorizeAttribute   
    {
        /// <summary>
        /// Validate the access token in cookie.
        /// </summary>
        /// <param name="httpContext">Http context.</param>
        /// <returns>
        /// <see langword="true"/> if the access token is valid. <see langword="false"/> otherwise.
        /// </returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            if (httpContext.Request.Cookies != null && httpContext.Request.Cookies.Get(Constants.AccessTokenCookieName) != null)
            {
                HttpCookie httpCookie = httpContext.Request.Cookies.Get(Constants.AccessTokenCookieName);

                if (httpCookie != null)
                {
                    string accessTokenCookie = httpCookie.Value;

                    if (!String.IsNullOrEmpty(accessTokenCookie))
                    {
                        AccessTokenCookie tokenCookie = AccessTokenCookie.Parse(accessTokenCookie);

                        if (tokenCookie != null)
                        {
                            httpContext.Items[Constants.AccessTokenCookieName] = tokenCookie;
                            return true;
                        }
                    }
                }
            }
            else if (httpContext.Items[Constants.AccessTokenCookieName] != null)
            {
                return true;
            }
            else if (AuthenticationHelper.IsAuthenticated)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Redirect the unauthorized users to the OAuth controller.
        /// </summary>
        /// <param name="filterContext">Authorization context.</param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(new
            RouteValueDictionary(new { controller = "Home", action = "AccessDenied" }));
        }
    }
}