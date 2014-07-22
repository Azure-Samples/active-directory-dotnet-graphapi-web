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
    /// <summary>
    /// Group controller to get/set/update/delete users.
    /// </summary>
    [Authorize]
    public class GroupsController : Controller
    {
        private string graphResourceId = ConfigurationManager.AppSettings["ida:GraphUrl"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:AppKey"];
        
        /// <summary>
        /// Gets a list of <see cref="Group"/> objects from Graph.
        /// </summary>
        /// <returns>A view with the list of <see cref="Group"/> objects.</returns>
        public ActionResult Index()
        {
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;
            List<Group> groupList = new List<Group>();

            try
            {
                // Get the access token from the cache
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority,
                    new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                result = authContext.AcquireTokenSilent(graphResourceId, credential,
                    new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

                //Setup GRaph API connection and get a list of groups
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);

                // Get all results into a list
                PagedResults<Group> pagedResults = graphConnection.List<Group>(null, new FilterGenerator());
                groupList.AddRange(pagedResults.Results);
                while (!pagedResults.IsLastPage)
                {
                    pagedResults = graphConnection.List<Group>(pagedResults.PageToken, new FilterGenerator());
                    groupList.AddRange(pagedResults.Results);
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
            return View(groupList);
        }

        /// <summary>
        /// Gets details of a single <see cref="Group"/> Graph.
        /// </summary>
        /// <returns>A view with the details of a single <see cref="Group"/>.</returns>
        public ActionResult Details(string objectId)
        {
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;
            Group group = null;

            try
            {
                // Get the access token from the cache
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority,
                    new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                result = authContext.AcquireTokenSilent(graphResourceId, credential,
                    new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

                //Setup Graph API connection and get a list of groups
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);

                group = graphConnection.Get<Group>(objectId);
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

            return View(group);
        }

        /// <summary>
        /// Creates a view to for adding a new <see cref="Group"/> to Graph.
        /// </summary>
        /// <returns>A view with the details to add a new <see cref="Group"/> objects</returns>
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Processes creation of a new <see cref="Group"/> to Graph.
        /// </summary>
        /// <param name="group"><see cref="Group"/> to be created.</param>
        /// <returns>A view with the details to all <see cref="Group"/> objects</returns>
        [HttpPost]
        public ActionResult Create([Bind(Include = "DisplayName,Description,MailNickName,SecurityEnabled")] Group group)
        {
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;

            try
            {
                // Get the access token from the cache
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority,
                    new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                result = authContext.AcquireTokenSilent(graphResourceId, credential,
                    new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
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

            try
            {
                // Setup Graph API connection and add Group
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                if (result != null)
                {
                    GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId,
                        graphSettings);
                    group.MailEnabled = false;
                    graphConnection.Add(group);
                    return RedirectToAction("Index");
                }
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View();
            }
            ViewBag.ErrorMessage = "AuthorizationRequired";
            return View();
        }

        /// <summary>
        /// Creates a view to for editing an existing <see cref="Group"/> in Graph.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="Group"/>.</param>
        /// <returns>A view with details to edit <see cref="Group"/>.</returns>
        public ActionResult Edit(string objectId)
        {
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;
            Group group = null;

            try
            {
                // Get the access token from the cache
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority,
                    new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                result = authContext.AcquireTokenSilent(graphResourceId, credential,
                    new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

                //Setup Graph API connection and get a list of groups
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);

                group = graphConnection.Get<Group>(objectId);
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
            return View(group);
        }

        /// <summary>
        /// Processes editing of an existing <see cref="Group"/>.
        /// </summary>
        /// <param name="group"><see cref="Group"/> to be edited.</param>
        /// <returns>A view with list of all <see cref="Group"/> objects.</returns>
        [HttpPost]
        public ActionResult Edit([Bind(Include="ObjectId,DispalyName,Description,MailNickName,SecurityEnabled")] Group group) 
        {
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;

            try
            {
                // Get the access token from the cache
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority,
                    new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                result = authContext.AcquireTokenSilent(graphResourceId, credential,
                    new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
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

            try
            {
                // Setup Graph API connection and update Group
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);
                graphConnection.Update(group);
                return RedirectToAction("Index");
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View();
            }
        }

        /// <summary>
        /// Creates a view to delete an existing <see cref="Group"/>.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="Group"/>.</param>
        /// <returns>A view of the <see cref="Group"/> to be deleted.</returns>
        public ActionResult Delete(string objectId)
        {
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;

            try
            {
                // Get the access token from the cache
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority,
                    new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                result = authContext.AcquireTokenSilent(graphResourceId, credential,
                    new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
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

            try
            {
                //Setup Graph API and get a single Group
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);
                Group group = graphConnection.Get<Group>(objectId);
                return View(group);
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View();
            }
        }

        /// <summary>
        /// Processes the deletion of a given <see cref="Group"/>.
        /// </summary>
        /// <param name="group"><see cref="Group"/> to be deleted.</param>
        /// <returns>A view to display all the existing <see cref="Group"/> objects.</returns>
        [HttpPost]
        public ActionResult Delete(Group group)
        {
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;

            try
            {
                // Get the access token from the cache
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority,
                    new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                result = authContext.AcquireTokenSilent(graphResourceId, credential,
                    new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
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

            try
            {
                // Setup Graph API connection and delete Group
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);
                graphConnection.Delete(group);
                return RedirectToAction("Index");
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View(group);
            }
        }

        /// <summary>
        /// Gets a list of <see cref="Group"/> objects that a given <see cref="Group"/> is member of.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="Group"/>.</param>
        /// <returns>A view with the list of <see cref="Group"/> objects.</returns>
        public ActionResult GetGroups(string objectId)
        {
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;
            IList<Group> groupMemberShip = new List<Group>();

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

                GraphObject graphGroup = graphConnection.Get<Group>(objectId);
                PagedResults<GraphObject> memberShip = graphConnection.GetLinkedObjects(graphGroup, LinkProperty.MemberOf, null, 999);
                
                // Filter for Groups only
                foreach (GraphObject graphObj in memberShip.Results)
                {
                    if (graphObj is Group)
                    {
                        Group group = (Group)graphObj;
                        groupMemberShip.Add(group);
                    }
                }

                //Do the same filtering for all pages
                while (!memberShip.IsLastPage)
                {
                    memberShip = graphConnection.GetLinkedObjects(graphGroup, LinkProperty.MemberOf,
                        memberShip.PageToken, 999);
                    foreach (GraphObject graphObj in memberShip.Results)
                    {
                        if (graphObj is Group)
                        {
                            Group group = (Group) graphObj;
                            groupMemberShip.Add(group);
                        }
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
            
            return View(groupMemberShip);
        }

        /// <summary>
        /// Gets a list of <see cref="User"/> objects that are members of a give <see cref="Group"/>.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="Group"/>.</param>
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

                // Setup Graph API connection and get Group membership
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);

                Group group = graphConnection.Get<Group>(objectId);
                PagedResults<GraphObject> members = graphConnection.GetLinkedObjects(group, LinkProperty.Members, null, 999);
                
                // Filter for users
                foreach (GraphObject obj in members.Results)
                {
                    if (obj is User)
                    {
                        users.Add((User)obj);
                    }
                }

                // Perform same filter for all pages
                while (!members.IsLastPage)
                {
                    members = graphConnection.GetLinkedObjects(group, LinkProperty.Members, members.PageToken, 999);
                    foreach (GraphObject obj in members.Results)
                    {
                        if (obj is User)
                        {
                            users.Add((User)obj);
                        }
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