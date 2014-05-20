using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;
using Microsoft.Owin.Security.OpenIdConnect;

namespace WebAppGraphAPI.Utils
{
    public class AuthUtils
    {
        public static string GetAuthToken(HttpRequestBase request, HttpContextBase httpContext)
        {
            // Retrieve the user's tenantID and access token since they are used to call the GraphAPI.
            //
            string accessToken = null;
            string tenantId = ClaimsPrincipal.Current.FindFirst(GraphConfiguration.TenantIdClaimType).Value;
            if (tenantId != null)
            { 
               accessToken = TokenCacheUtils.GetAccessTokenFromCacheOrRefreshToken(tenantId, GraphConfiguration.GraphResourceId);
            }
            if (accessToken == null)
            {
                //
                // If refresh is set to true, the user has clicked the link to be authorized again.
                //
                if (request.QueryString["reauth"] == "True")
                {
                    // Send an OpenID Connect sign-in request to get a new set of tokens.
                    // If the user still has a valid session with Azure AD, they will not be prompted for their credentials.
                    // The OpenID Connect middleware will return to this controller after the sign-in response has been handled.
                    //
                    httpContext.GetOwinContext().Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationType);
                }
            }

            return accessToken;
        }
    }
}