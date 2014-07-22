using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using System.Threading.Tasks;
using WebAppGraphAPI.Models;
using System.Security.Claims;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Globalization;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using WebAppGraphAPI.Utils;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace WebAppGraphAPI.Controllers
{
    /// <summary>
    /// User controller to get/set/update/delete users.
    /// </summary>
    [Authorize]
    public class UsersController : Controller
    {
        private string graphResourceId = ConfigurationManager.AppSettings["ida:GraphUrl"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:AppKey"];

        /// <summary>
        /// Gets a list of <see cref="User"/> objects from Graph.
        /// </summary>
        /// <returns>A view with the list of <see cref="User"/> objects.</returns>
        public ActionResult Index()
        {
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;
            List<User> userList = new List<User>();

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

                // Get results from all pages into a list
                PagedResults<User> pagedResults = graphConnection.List<User>(null, new FilterGenerator());
                userList.AddRange(pagedResults.Results);
                while (!pagedResults.IsLastPage)
                {
                    pagedResults = graphConnection.List<User>(pagedResults.PageToken, new FilterGenerator());
                    userList.AddRange(pagedResults.Results);
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

            return View(userList);
        }

        /// <summary>
        /// Gets details of a single <see cref="User"/> Graph.
        /// </summary>
        /// <returns>A view with the details of a single <see cref="User"/>.</returns>
        public ActionResult Details(string objectId)
        {
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;
            User user = null;

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

                user = graphConnection.Get<User>(objectId);
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
            
            return View(user);
        }

        /// <summary>
        /// Creates a view to for adding a new <see cref="User"/> to Graph.
        /// </summary>
        /// <returns>A view with the details to add a new <see cref="User"/> objects</returns>
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Processes creation of a new <see cref="User"/> to Graph.
        /// </summary>
        /// <returns>A view with the details to all <see cref="User"/> objects</returns>
        [HttpPost]
        public ActionResult Create([Bind(Include="UserPrincipalName,AccountEnabled,PasswordProfile,MailNickname,DisplayName,GivenName,Surname,JobTitle,Department")] User user)
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
                // Setup Graph API connection and add User
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);
                graphConnection.Add(user);
                return RedirectToAction("Index");
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View();
            }
        }

        /// <summary>
        /// Creates a view to for editing an existing <see cref="User"/> in Graph.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="User"/>.</param>
        /// <returns>A view with details to edit <see cref="User"/>.</returns>
        public ActionResult Edit(string objectId)
        {
            //Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;
            User user = null;
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

                user = graphConnection.Get<User>(objectId);
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

            return View(user);
        }

        /// <summary>
        /// Gets a list of <see cref="Group"/> objects that a given <see cref="User"/> is member of.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="User"/>.</param>
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

                //Setup Graph API connection and get a list of users
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);

                GraphObject graphUser = graphConnection.Get<User>(objectId);
                PagedResults<GraphObject> memberShip = graphConnection.GetLinkedObjects(graphUser, LinkProperty.MemberOf, null, 999);
                // users can be members of Groups and Roles
                // for this app, we will filter and only show Groups
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
                while (!memberShip.IsLastPage)
                {
                    memberShip = graphConnection.GetLinkedObjects(graphUser, LinkProperty.MemberOf, memberShip.PageToken, 999);
                    count = 0;
                    foreach (GraphObject graphObj in memberShip.Results)
                    {
                        if (graphObj.ODataTypeName.Contains("Group"))
                        {
                            Group groupMember = (Group)memberShip.Results[count];
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

        /// <summary>
        /// Processes editing of an existing <see cref="User"/>.
        /// </summary>
        /// <param name="user"><see cref="User"/> to be edited.</param>
        /// <returns>A view with list of all <see cref="User"/> objects.</returns>
        [HttpPost]
        public ActionResult Edit([Bind(Include = "ObjectId,UserPrincipalName,DisplayName,AccountEnabled,GivenName,Surname,JobTitle,Department,Mobile,StreetAddress,City,State,Country,")] User user, FormCollection values)
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
                // Setup Graph API connection and update single User
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);
                graphConnection.Update(user);

                // update thumbnail photo 
                if (!String.IsNullOrEmpty(values["photofile"]))
                {

                  //string path = AppDomain.CurrentDomain.BaseDirectory + "uploads/";
                  //string filename = Path.GetFileName(values["photofile"]);
                  //Image image = Image.FromFile(filename);
              
                    var imageFile = Path.Combine(Server.MapPath("~/app_data"), values["photofile"]);
                    Image image = Image.FromFile(imageFile);
                    
                  MemoryStream stream = new MemoryStream();
                  image.Save(stream, ImageFormat.Jpeg);
                    
                  // Write the photo file to the Graph service.                    
                  graphConnection.SetStreamProperty(user, GraphProperty.ThumbnailPhoto, stream, "image/jpeg");
                  
                }


                return RedirectToAction("Index");
            }
            catch(Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View();
            }
        }

        /// <summary>
        /// Creates a view to delete an existing <see cref="User"/>.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="User"/>.</param>
        /// <returns>A view of the <see cref="User"/> to be deleted.</returns>
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
                // Setup Graph API connection and get single User
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);
                User user = graphConnection.Get<User>(objectId);
                return View(user);
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View();
            }
        }

        /// <summary>
        /// Processes the deletion of a given <see cref="User"/>.
        /// </summary>
        /// <param name="user"><see cref="User"/> to be deleted.</param>
        /// <returns>A view to display all the existing <see cref="User"/> objects.</returns>
        [HttpPost]
        public ActionResult Delete(User user)
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
                // Setup Graph API connection and delete User
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;               
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);
                graphConnection.Delete(user);
                return RedirectToAction("Index");
            }
            catch(Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View(user);
            }
        }

        /// <summary>
        /// Gets a list of <see cref="User"/> objects that a given <see cref="User"/> has as a direct report.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="User"/>.</param>
        /// <returns>A view with the list of <see cref="User"/> objects.</returns>
        public ActionResult GetDirectReports(string objectId)
        {

            // Get the access token as we need it to make a call to the Graph API
            AuthenticationResult result = null;
            IList<User> reports = new List<User>();

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

                GraphObject graphUser = graphConnection.Get<User>(objectId);
                IList<GraphObject> results = graphConnection.GetAllDirectLinks(graphUser, LinkProperty.DirectReports);
                foreach (GraphObject obj in results)
                {
                    if (obj is User)
                    {
                        User user = (User)obj;
                        reports.Add(user);
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

            return View(reports);
        }

        public ActionResult ShowThumbnail(string id)
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

                // Setup Graph API connection and get Group membership
                Guid ClientRequestId = Guid.NewGuid();
                GraphSettings graphSettings = new GraphSettings();
                graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
                GraphConnection graphConnection = new GraphConnection(result.AccessToken, ClientRequestId, graphSettings);

                // User user = graphConnection.Get<User>(id);
                User user = new User();
                user.ObjectId = id;

                try
                {
                    Stream ms = graphConnection.GetStreamProperty(user, GraphProperty.ThumbnailPhoto, "image/jpeg");
                    user.ThumbnailPhoto = ms;
                }
                catch
                {
                    user.ThumbnailPhoto = null;
                }


                if (user.ThumbnailPhoto != null)
                {
                    return File(user.ThumbnailPhoto, "image/jpeg");
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
            }

            return View();
        }
    }
}
