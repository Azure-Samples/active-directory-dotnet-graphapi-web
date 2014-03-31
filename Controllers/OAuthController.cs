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

namespace AadExplorer.Controllers
{
    using System;
    using System.IdentityModel.Tokens;
    using System.Linq;
    using System.Security.Claims;
    using System.Web;
    using System.Web.Mvc;
    using AadExplorer.Utils;
    using ADAL = Microsoft.IdentityModel.Clients.ActiveDirectory;

    public class OAuthController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Register the app in the tenant.
        /// </summary>
        /// <returns>Action result.</returns>
        public ActionResult SignUp()
        {
            string signupToken = Guid.NewGuid().ToString();
            string replyUrl = Url.Action(
                "SignUpCallback", 
                Constants.OAuthControllerName, 
                routeValues: new { signupToken = signupToken }, protocol: 
                Request.Url.Scheme);

            string consentUrl = this.CreateConsentUrl(
                "DirectoryReaders", replyUrl);

            // Redirect to the Active Directory consent page asking for permissions.
            return new RedirectResult(consentUrl);
        }

        /// <summary>
        /// Store the fact that the tenant has consented!
        /// </summary>
        /// <returns>Action result.</returns>
        public ActionResult SignUpCallback()
        {
            string tenantId = Request.QueryString[Constants.TenantIdKey];
            string signupToken = Request.QueryString[Constants.SignupTokenKey];

            Request.RequestContext.HttpContext.Items[Constants.SignupTokenKey] = signupToken;
            Request.RequestContext.HttpContext.Items[Constants.TenantIdKey] = tenantId;

            return View();
        }

        /// <summary>
        /// Start the sign in process for app only - get an access token.
        /// </summary>
        /// <returns>Action result.</returns>
        public ActionResult AppOnlySignIn()
        {
            if (!AuthenticationHelper.IsAuthenticated)
            {
                AuthenticationHelper.GetAppOnlyAccessToken();
            }

            // Redirect to home page if the app is already signed in.
            return RedirectToAction("Index", Constants.HomeControllerName);
        }

        /// <summary>
        /// Start the sign in process - get an access token.
        /// </summary>
        /// <returns>Action result.</returns>
        public ActionResult SignIn()
        {
            //return RedirectToAction("Index", Constants.GraphApiControllerName);

            if (AuthenticationHelper.IsAuthenticated)
            {
                // Redirect to home page if the user is already signed in.
                return RedirectToAction("Index", Constants.HomeControllerName);
            }

            return new RedirectResult(Constants.AuthorizeUrl);
        }

        /// <summary>
        /// Redeem auth code for an access token
        /// </summary>
        /// <param name="code">Auth code.</param>
        /// <param name="id_token">Id token.</param>
        /// <returns>View</returns>
        public ActionResult RedeemCode(string code, string id_token, HttpResponseBase httpResponse)
        {
            JwtSecurityToken idToken = new JwtSecurityToken(id_token);
            Claim tenantIdClaim = idToken.Claims.FirstOrDefault(x => x.Type == Constants.TenantIdClaimType);
            
            if (tenantIdClaim == null)
            {
                return View("Tenant id claim not found in the id token.");
            }

            try
            {
                ADAL.AuthenticationContext ac = new ADAL.AuthenticationContext(
                    String.Format(Constants.AuthorityUrlFormat, Constants.LoginDomainName, tenantIdClaim.Value));
                ADAL.ClientCredential clientCredential = 
                  new ADAL.ClientCredential(Constants.AppPrincipalId, Constants.AppKey);

                //ADAL.X509CertificateCredential certificateCredential = new ADAL.X509CertificateCredential(
                //    Constants.AppPrincipalId,
                //    CertificateHelper.GetSigningCertificate());

                ADAL.AuthenticationResult authenticationResult = ac.AcquireTokenByAuthorizationCode(
                    code,
                    new Uri(Constants.ReplyUrlForCatchingCode),
                    clientCredential);
                AccessTokenCookie tokenCookie = new AccessTokenCookie(
                    authenticationResult.AccessToken,
                    id_token,
                    authenticationResult.RefreshToken);
                HttpCookie httpCookie = new HttpCookie(
                    Constants.AccessTokenCookieName, tokenCookie.ToCookieString());
                httpResponse.Cookies.Set(httpCookie);
                 AuthenticationHelper.IsAuthenticated = true;
                return RedirectToAction("Index", Constants.UsersControllerName);
            }
            catch (ADAL.ActiveDirectoryAuthenticationException authenticationException)
            {
                Logger.Error(authenticationException.ToString());
                return View("Error", authenticationException);
            }
        }


        /// <summary>
        /// Redirect the user to the AAD sign out page.
        /// After sign out, the user will land back in application home page.
        /// </summary>
        /// <returns>Action result.</returns>
        public ActionResult SignOut()
        {
            AuthenticationHelper.IsAuthenticated = false;
            HttpContext.Response.Cookies.Remove(Constants.AccessTokenCookieName);
            return new RedirectResult(Constants.SignOutUrl);
        }

        /// <summary>
        /// Create the consent url.
        /// </summary>
        /// <param name="requestedPermissions">Requested permissions.</param>
        /// <param name="consentReturnUrl">Consent return url.</param>
        /// <returns>Consent url.</returns>
        private string CreateConsentUrl(
            string requestedPermissions, string consentReturnUrl)
        {
            string consentUrl = Constants.ConsentUrl;

            if (!String.IsNullOrEmpty(requestedPermissions))
            {
                consentUrl += "&RequestedPermissions=" + HttpUtility.UrlEncode(requestedPermissions);
            }

            if (!String.IsNullOrEmpty(consentReturnUrl))
            {
                consentUrl += "&ConsentReturnURL=" + HttpUtility.UrlEncode(consentReturnUrl);
            }

            return consentUrl;
        }

    }
}
