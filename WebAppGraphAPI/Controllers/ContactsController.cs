using Microsoft.Azure.ActiveDirectory.GraphClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;
using Microsoft.Owin.Security.OpenIdConnect;
using WebAppGraphAPI.Utils;

namespace WebAppGraphAPI.Controllers
{
    [Authorize]
    public class ContactsController : Controller
    {

        private const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        private string graphResourceId = ConfigurationManager.AppSettings["ida:GraphUrl"];
        private static string graphApiVersion = ConfigurationManager.AppSettings["ida:GraphApiVersion"];

        //
        // GET: /Contacts/
        public ActionResult Index()
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

            Guid ClientRequestId = Guid.NewGuid();
            GraphSettings graphSettings = new GraphSettings();
            graphSettings.ApiVersion = graphApiVersion;
            GraphConnection graphConnection = new GraphConnection(accessToken, ClientRequestId, graphSettings);

            PagedResults<Contact> pagedResults = graphConnection.List<Contact>(null, new FilterGenerator());

            return View(pagedResults.Results);
        }

        /// <summary>
        /// Gets details of a single <see cref="Contact"/> Graph.
        /// </summary>
        /// <returns>A view with the details of a single <see cref="Contact"/>.</returns>
        public ActionResult Details(string objectId)
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

            // Setup Graph API connection and get single User
            Guid ClientRequestId = Guid.NewGuid();
            GraphSettings graphSettings = new GraphSettings();
            graphSettings.ApiVersion = graphApiVersion;
            GraphConnection graphConnection = new GraphConnection(accessToken, ClientRequestId, graphSettings);

            Contact contact = graphConnection.Get<Contact>(objectId);
            return View(contact);
        }

        /// <summary>
        /// Gets a list of <see cref="Group"/> objects that a given <see cref="Contact"/> is member of.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="Contact"/>.</param>
        /// <returns>A view with the list of <see cref="Group"/> objects.</returns>
        public ActionResult GetGroups(string objectId)
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
            // Setup Graph API connection and get Group membership
            Guid ClientRequestId = Guid.NewGuid();
            GraphSettings graphSettings = new GraphSettings();
            graphSettings.ApiVersion = graphApiVersion;
            GraphConnection graphConnection = new GraphConnection(accessToken, ClientRequestId, graphSettings);

            GraphObject graphContact = graphConnection.Get<Contact>(objectId);
            PagedResults<GraphObject> memberShip = graphConnection.GetLinkedObjects(graphContact, LinkProperty.MemberOf, null, 999);

            // users can be members of Groups and Roles
            // for this app, we will filter and only show Groups

            int count = 0;
            IList<Group> groupMembership = new List<Group>();
            foreach (GraphObject graphObj in memberShip.Results)
            {
                if (graphObj.ODataTypeName.Contains("Group"))
                {
                    Group groupMember = (Group)memberShip.Results[count];
                    groupMembership.Add(groupMember);
                }
                ++count;
            }

            //return View(groupIds);
            return View(groupMembership);
        }
	}
}