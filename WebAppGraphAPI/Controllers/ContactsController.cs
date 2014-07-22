using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using WebAppGraphAPI.Utils;

namespace WebAppGraphAPI.Controllers
{
    /// <summary>
    /// Contact controller to get/set/update/delete users.
    /// </summary>
    [Authorize]
    public class ContactsController : Controller
    {
        private string graphResourceId = ConfigurationManager.AppSettings["ida:GraphUrl"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:AppKey"];

        /// <summary>
        /// Gets a list of <see cref="Contact"/> objects from Graph.
        /// </summary>
        /// <returns>A view with the list of <see cref="Contact"/> objects.</returns>
        public ActionResult Index()
        {
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;
            List<Contact> contactList = new List<Contact>();
            
            try
            {
                // Get the access token from the cache
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority,
                    new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                result = authContext.AcquireTokenSilent(graphResourceId, credential,
                    new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

                //Setup Graph API connection and get a list of users
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);

                // Get all results into a list
                PagedResults<Contact> pagedResults = graphConnection.List<Contact>(null, new FilterGenerator());
                contactList.AddRange(pagedResults.Results);
                while (!pagedResults.IsLastPage)
                {
                    pagedResults = graphConnection.List<Contact>(pagedResults.PageToken, new FilterGenerator());
                    contactList.AddRange(pagedResults.Results);
                }
            }
            catch (Exception e)
            {
                if (Request.QueryString["reauth"] == "True")
                {
                    //
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

            return View(contactList);
        }

        /// <summary>
        /// Gets details of a single <see cref="Contact"/> Graph.
        /// </summary>
        /// <returns>A view with the details of a single <see cref="Contact"/>.</returns>
        public ActionResult Details(string objectId)
        {
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;
            Contact contact = null;

            try
            {
                // Get the access token from the cache
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority,
                    new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                result = authContext.AcquireTokenSilent(graphResourceId, credential,
                    new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

                // Setup Graph API connection and get single Contact
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);

                contact = graphConnection.Get<Contact>(objectId);
                
            }
            catch (Exception e)
            {
                if (Request.QueryString["reauth"] == "True")
                {
                    //
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

            return View(contact);
        }

        /// <summary>
        /// Gets a list of <see cref="Group"/> objects that a given <see cref="Contact"/> is member of.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="Contact"/>.</param>
        /// <returns>A view with the list of <see cref="Group"/> objects.</returns>
        public ActionResult GetGroups(string objectId)
        {   
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;
            IList<Group> groupMembership = new List<Group>();

            try
            {
                // Get the access token from the cache
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority,
                    new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                result = authContext.AcquireTokenSilent(graphResourceId, credential,
                    new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

                // Setup Graph API connection and get Group membership
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);

                GraphObject graphContact = graphConnection.Get<Contact>(objectId);
                PagedResults<GraphObject> memberShip = graphConnection.GetLinkedObjects(graphContact, LinkProperty.MemberOf, null, 999);
                
                // filter for Groups only
                int count = 0;
                foreach (GraphObject graphObj in memberShip.Results)
                {
                    if (graphObj.ODataTypeName.Contains("Group"))
                    {
                        Group groupMember = (Group)memberShip.Results[count];
                        groupMembership.Add(groupMember);
                    }
                    ++count;
                }
                
                // Do the same filtering for all pages
                while (!memberShip.IsLastPage)
                {
                    memberShip = graphConnection.GetLinkedObjects(graphContact, LinkProperty.MemberOf, memberShip.PageToken, 999);
                    // filter for Groups only
                    count = 0;
                    foreach (GraphObject graphObj in memberShip.Results)
                    {
                        if (graphObj.ODataTypeName.Contains("Group"))
                        {
                            Group groupMember = (Group) memberShip.Results[count];
                            groupMembership.Add(groupMember);
                        }
                        ++count;
                    }
                }
            }
            catch (Exception e)
            {
                if (Request.QueryString["reauth"] == "True")
                {
                    //
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

            return View(groupMembership);
        }
	}
}