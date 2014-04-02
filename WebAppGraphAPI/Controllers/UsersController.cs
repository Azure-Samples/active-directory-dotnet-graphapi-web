using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

namespace WebAppGraphAPI.Controllers
{
    /// <summary>
    /// User controller to get/set/update/delete users.
    /// </summary>
    [Authorize]
    public class UsersController : Controller
    {

        private const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        private string graphResourceId = ConfigurationManager.AppSettings["ida:GraphUrl"];
        private static string graphApiVersion = ConfigurationManager.AppSettings["ida:GraphApiVersion"];

        /// <summary>
        /// Gets a list of <see cref="User"/> objects from Graph.
        /// </summary>
        /// <returns>A view with the list of <see cref="User"/> objects.</returns>
        public ActionResult Index()
        {
            // Retrieve the user's tenantID and access token since they are used to call the GraphAPI.
            //
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
            
            CallContext currentCallContext = new CallContext(accessToken, Guid.NewGuid(), graphApiVersion);
            GraphConnection graphConnection = new GraphConnection(currentCallContext);
            PagedResults pagedReslts = graphConnection.List<User>(null, new FilterGenerator());
            return View(pagedReslts.Results);
        }

        /// <summary>
        /// Gets details of a single <see cref="User"/> Graph.
        /// </summary>
        /// <returns>A view with the details of a single <see cref="User"/>.</returns>
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

            CallContext currentCallContext = new CallContext(
                accessToken, Guid.NewGuid(), graphApiVersion);
            GraphConnection graphConnection = new GraphConnection(currentCallContext);
            User user = graphConnection.Get<User>(objectId);
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
                CallContext currentCallContext = new CallContext(
                    accessToken, Guid.NewGuid(), graphApiVersion);
                GraphConnection graphConnection = new GraphConnection(currentCallContext);
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
            
            
            CallContext currentCallContext = new CallContext(
               accessToken, Guid.NewGuid(),graphApiVersion);
            GraphConnection graphConnection = new GraphConnection(currentCallContext);
            User user = graphConnection.Get<User>(objectId);
            return View(user);
        }

        /// <summary>
        /// Gets a list of <see cref="Group"/> objects that a given <see cref="User"/> is member of.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="User"/>.</param>
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
            
            
            
            
            CallContext currentCallContext = new CallContext(
                accessToken, Guid.NewGuid(), graphApiVersion);
            GraphConnection graphConnection = new GraphConnection(currentCallContext);
            IList<string> groupIds = graphConnection.GetMemberGroups(new User() { ObjectId = objectId }, false);
            return View(groupIds);
        }

        /// <summary>
        /// Processes editing of an existing <see cref="User"/>.
        /// </summary>
        /// <param name="user"><see cref="User"/> to be edited.</param>
        /// <returns>A view with list of all <see cref="User"/> objects.</returns>
        [HttpPost]
        public ActionResult Edit([Bind(Include="ObjectId,UserPrincipalName,DisplayName,City,Department")] User user)
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
                // TODO: Add update logic here
                CallContext currentCallContext = new CallContext(
                accessToken, Guid.NewGuid(), graphApiVersion);
                GraphConnection graphConnection = new GraphConnection(currentCallContext);
                graphConnection.Update(user);
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
                CallContext currentCallContext = new CallContext(
                accessToken, Guid.NewGuid(), graphApiVersion);
                GraphConnection graphConnection = new GraphConnection(currentCallContext);
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
                CallContext currentCallContext = new CallContext(
                accessToken, Guid.NewGuid(), graphApiVersion);
                GraphConnection graphConnection = new GraphConnection(currentCallContext);
                graphConnection.Delete(user);
                return RedirectToAction("Index");
            }
            catch(Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View(user);
            }
        }
    }
}
