using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using WebAppGraphAPI.Utils;

namespace WebAppGraphAPI.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {

        private const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        private string graphResourceId = ConfigurationManager.AppSettings["ida:GraphUrl"];
        private static string graphApiVersion = ConfigurationManager.AppSettings["ida:GraphApiVersion"];

        //
        // GET: /Groups/
        public ActionResult Index()
        {
            string accessToken = null;
            string tenantId = ClaimsPrincipal.Current.FindFirst(TenantIdClaimType).Value;
            if (tenantId != null)
            {
                accessToken = WebAppGraphAPI.Utils.TokenCacheUtils.GetAccessTokenFromCacheOrRefreshToken(tenantId, graphResourceId);
            }
            if (accessToken == null)
            {
                //
                // If refresh is set to true, the user has clicked the link to be authorized again.
                //
                if (Request.QueryString["reauth"] == "True")
                {
                    // Send an OpenID Connect sign-in request to get a new set of tokens.
                    // If the user still has a valid session with Azure AD, they will not be prompted for their credentials.
                    // The OpenID Connect middleware will return to this controller after the sign-in response has been handled.
                    //
                    HttpContext.GetOwinContext().Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationType);
                }
                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View();
            }

            Guid ClientRequestId = Guid.NewGuid();
            GraphSettings graphSettings = new GraphSettings();
            graphSettings.ApiVersion = graphApiVersion;
            GraphConnection graphConnection = new GraphConnection(accessToken, ClientRequestId, graphSettings);

            PagedResults<Group> pagedResults = graphConnection.List<Group>(null, new FilterGenerator());

            return View(pagedResults.Results);
        }

        public ActionResult Delete(string objectId)
        {
            string accessToken = null;
            string tenantId = ClaimsPrincipal.Current.FindFirst(TenantIdClaimType).Value;
            if (tenantId != null)
            {
                accessToken = TokenCacheUtils.GetAccessTokenFromCacheOrRefreshToken(tenantId, graphResourceId);
            }
            if (accessToken == null)
            {
                //
                // If refresh is set to true, the user has clicked the link to be authorized again.
                //
                if (Request.QueryString["reauth"] == "True")
                {
                    // Send an OpenID Connect sign-in request to get a new set of tokens.
                    // If the user still has a valid session with Azure AD, they will not be prompted for their credentials.
                    // The OpenID Connect middleware will return to this controller after the sign-in response has been handled.
                    //
                    HttpContext.GetOwinContext().Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationType);
                }
                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View();
            }

            try
            {
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = graphApiVersion;
                GraphConnection graphConnection = new GraphConnection(accessToken, ClientRequestId, graphSettings);
                Group group = graphConnection.Get<Group>(objectId);
                return View(group);
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View();
            }
        }

        [HttpPost]
        public ActionResult Delete(Group group)
        {
            string accessToken = null;
            string tenantId = ClaimsPrincipal.Current.FindFirst(TenantIdClaimType).Value;
            if (tenantId != null)
            {
                accessToken = TokenCacheUtils.GetAccessTokenFromCacheOrRefreshToken(tenantId, graphResourceId);
            }
            if (accessToken == null)
            {
                //
                // If refresh is set to true, the user has clicked the link to be authorized again.
                //
                if (Request.QueryString["reauth"] == "True")
                {
                    // Send an OpenID Connect sign-in request to get a new set of tokens.
                    // If the user still has a valid session with Azure AD, they will not be prompted for their credentials.
                    // The OpenID Connect middleware will return to this controller after the sign-in response has been handled.
                    //
                    HttpContext.GetOwinContext().Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationType);
                }
                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View();
            }
            try
            {
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = graphApiVersion;
                GraphConnection graphConnection = new GraphConnection(accessToken, ClientRequestId, graphSettings);
                graphConnection.Delete(group);
                return RedirectToAction("Index");
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View(group);
            }
            
        }
	}
}