#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using Microsoft.Owin.Security.OpenIdConnect;
using WebAppGraphAPI.Utils;

#endregion

namespace WebAppGraphAPI.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        /// <summary>
        ///     Gets a list of <see cref="Role" /> objects from Graph.
        /// </summary>
        /// <returns>A view with the list of <see cref="Role" /> objects.</returns>
        public async Task<ActionResult> Index()
        {
            List<DirectoryRole> roleList = new List<DirectoryRole>();

            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                IPagedCollection<IDirectoryRole> pagedCollection = await client.DirectoryRoles.ExecuteAsync();
                do
                {
                    List<IDirectoryRole> directoryRoles = pagedCollection.CurrentPage.ToList();
                    foreach (IDirectoryRole directoryRole in directoryRoles)
                    {
                        roleList.Add((DirectoryRole) directoryRole);
                    }
                    pagedCollection = await pagedCollection.GetNextPageAsync();
                } while (pagedCollection != null);
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
                    HttpContext.GetOwinContext()
                        .Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationType);
                }

                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View(roleList);
            }
            return View(roleList);
        }

        /// <summary>
        ///     Gets details of a single <see cref="Role" /> Graph.
        /// </summary>
        /// <returns>A view with the details of a single <see cref="Role" />.</returns>
        public async Task<ActionResult> Details(string objectId)
        {
            DirectoryRole role = null;
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                role = (DirectoryRole) await client.DirectoryRoles.GetByObjectId(objectId).ExecuteAsync();
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
                    HttpContext.GetOwinContext()
                        .Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationType);
                }

                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View();
            }

            return View(role);
        }
    }
}