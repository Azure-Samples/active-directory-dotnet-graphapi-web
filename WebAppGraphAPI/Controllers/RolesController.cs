using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
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
    public class RolesController : Controller
    {
        private string graphResourceId = ConfigurationManager.AppSettings["ida:GraphUrl"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:AppKey"];

        /// <summary>
        /// Gets a list of <see cref="Role"/> objects from Graph.
        /// </summary>
        /// <returns>A view with the list of <see cref="Role"/> objects.</returns>
        public ActionResult Index()
        {
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;
            List<Role> roleList = new List<Role>();

            try
            {
                // Get the access token from the cache
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority,
                    new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                result = authContext.AcquireTokenSilent(graphResourceId, credential,
                    new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

                //Setup Graph API connection and get a list of roles
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);

                PagedResults<Role> pagedResults = graphConnection.List<Role>(null, new FilterGenerator());
                roleList.AddRange(pagedResults.Results);
                
                // Get roles for all pages
                while (!pagedResults.IsLastPage)
                {
                    pagedResults = graphConnection.List<Role>(pagedResults.PageToken, new FilterGenerator());
                    roleList.AddRange(pagedResults.Results);
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
            return View(roleList);
        }

        /// <summary>
        /// Gets details of a single <see cref="Role"/> Graph.
        /// </summary>
        /// <returns>A view with the details of a single <see cref="Role"/>.</returns>
        public ActionResult Details(string objectId)
        {
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;
            Role contact = null;

            try
            {
                // Get the access token from the cache
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority,
                    new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                result = authContext.AcquireTokenSilent(graphResourceId, credential,
                    new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

                //Setup Graph API connection and get a list of roles
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);

                contact = graphConnection.Get<Role>(objectId);
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
        /// Gets a list of <see cref="User"/> objects that are members of a give <see cref="Role"/>.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="Role"/>.</param>
        /// <returns>A view with the list of <see cref="User"/> objects.</returns>
        public ActionResult GetMembers(string objectId)
        {
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;
            IList<User> users = new List<User>();

            try
            {
                // Get the access token from the cache
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority,
                    new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                result = authContext.AcquireTokenSilent(graphResourceId, credential,
                    new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

                // Setup Graph API connection and get a list of roles
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);

                Role role = graphConnection.Get<Role>(objectId);
                IList<GraphObject> members = graphConnection.GetAllDirectLinks(role, LinkProperty.Members);

                // Filter for users
                foreach (GraphObject obj in members)
                {
                    if (obj is User)
                    {
                        users.Add((User)obj);
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

            return View(users);
        }
    }
}