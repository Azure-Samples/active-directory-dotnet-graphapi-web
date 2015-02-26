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
    /// <summary>
    ///     Group controller to get/set/update/delete users.
    /// </summary>
    [Authorize]
    public class GroupsController : Controller
    {
        /// <summary>
        ///     Gets a list of <see cref="Group" /> objects from Graph.
        /// </summary>
        /// <returns>A view with the list of <see cref="Group" /> objects.</returns>
        public async Task<ActionResult> Index()
        {
            List<Group> groupList = new List<Group>();

            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                IPagedCollection<IGroup> pagedCollection = await client.Groups.ExecuteAsync();
                do
                {
                    List<IGroup> groups = pagedCollection.CurrentPage.ToList();
                    foreach (IGroup group in groups)
                    {
                        groupList.Add((Group) group);
                    }
                    pagedCollection = pagedCollection.GetNextPageAsync().Result;
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
                return View(groupList);
            }
            return View(groupList);
        }

        /// <summary>
        ///     Gets details of a single <see cref="Group" /> Graph.
        /// </summary>
        /// <returns>A view with the details of a single <see cref="Group" />.</returns>
        public async Task<ActionResult> Details(string objectId)
        {
            Group group = null;
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                group = (Group) await client.Groups.GetByObjectId(objectId).ExecuteAsync();
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

            return View(group);
        }

        /// <summary>
        ///     Creates a view to for adding a new <see cref="Group" /> to Graph.
        /// </summary>
        /// <returns>A view with the details to add a new <see cref="Group" /> objects</returns>
        public async Task<ActionResult> Create()
        {
            return View();
        }

        /// <summary>
        ///     Processes creation of a new <see cref="Group" /> to Graph.
        /// </summary>
        /// <param name="group"><see cref="Group" /> to be created.</param>
        /// <returns>A view with the details to all <see cref="Group" /> objects</returns>
        [HttpPost]
        public async Task<ActionResult> Create(
            [Bind(Include = "DisplayName,Description,MailNickName,SecurityEnabled")] Group group)
        {
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                await client.Groups.AddGroupAsync(group);
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
        ///     Creates a view to for editing an existing <see cref="Group" /> in Graph.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="Group" />.</param>
        /// <returns>A view with details to edit <see cref="Group" />.</returns>
        public async Task<ActionResult> Edit(string objectId)
        {
            Group group = null;
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                group = (Group) await client.Groups.GetByObjectId(objectId).ExecuteAsync();
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
            return View(group);
        }

        /// <summary>
        ///     Processes editing of an existing <see cref="Group" />.
        /// </summary>
        /// <param name="group"><see cref="Group" /> to be edited.</param>
        /// <returns>A view with list of all <see cref="Group" /> objects.</returns>
        [HttpPost]
        public async Task<ActionResult> Edit(Group group, FormCollection values)
        {
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                IGroup toUpdate = await client.Groups.GetByObjectId(group.ObjectId).ExecuteAsync();
                Helper.CopyUpdatedValues(toUpdate, group, values);
                await toUpdate.UpdateAsync();
                return RedirectToAction("Index");
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View();
            }
        }

        /// <summary>
        ///     Creates a view to delete an existing <see cref="Group" />.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="Group" />.</param>
        /// <returns>A view of the <see cref="Group" /> to be deleted.</returns>
        public async Task<ActionResult> Delete(string objectId)
        {
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                Group group = (Group) await client.Groups.GetByObjectId(objectId).ExecuteAsync();
                return View(group);
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View();
            }
        }

        /// <summary>
        ///     Processes the deletion of a given <see cref="Group" />.
        /// </summary>
        /// <param name="group"><see cref="Group" /> to be deleted.</param>
        /// <returns>A view to display all the existing <see cref="Group" /> objects.</returns>
        [HttpPost]
        public async Task<ActionResult> Delete(Group group)
        {
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                IGroup toDelete = await client.Groups.GetByObjectId(@group.ObjectId).ExecuteAsync();
                await toDelete.DeleteAsync();
                return RedirectToAction("Index");
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View(group);
            }
        }

        /// <summary>
        ///     Gets a list of <see cref="Group" /> objects that a given <see cref="Group" /> is member of.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="Group" />.</param>
        /// <returns>A view with the list of <see cref="Group" /> objects.</returns>
        public async Task<ActionResult> GetGroups(string objectId)
        {
            IList<Group> groupMemberShip = new List<Group>();

            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                IGroup group = await client.Groups.GetByObjectId(objectId).ExecuteAsync();
                IGroupFetcher groupFetcher = group as IGroupFetcher;
                IPagedCollection<IDirectoryObject> pagedCollection = await groupFetcher.MemberOf.ExecuteAsync();
                do
                {
                    List<IDirectoryObject> directoryObjects = pagedCollection.CurrentPage.ToList();
                    foreach (IDirectoryObject directoryObject in directoryObjects)
                    {
                        if (directoryObject is Group)
                        {
                            groupMemberShip.Add((Group) directoryObject);
                        }
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
                return View();
            }

            return View(groupMemberShip);
        }

        /// <summary>
        ///     Gets a list of <see cref="User" /> objects that are members of a give <see cref="Group" />.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="Group" />.</param>
        /// <returns>A view with the list of <see cref="User" /> objects.</returns>
        public async Task<ActionResult> GetMembers(string objectId)
        {
            IList<User> users = new List<User>();
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                IGroup group = await client.Groups.GetByObjectId(objectId).ExecuteAsync();
                IGroupFetcher groupFetcher = group as IGroupFetcher;
                IPagedCollection<IDirectoryObject> pagedCollection = await groupFetcher.Members.ExecuteAsync();
                do
                {
                    List<IDirectoryObject> directoryObjects = pagedCollection.CurrentPage.ToList();
                    foreach (IDirectoryObject directoryObject in directoryObjects)
                    {
                        if (directoryObject is User)
                        {
                            users.Add((User) directoryObject);
                        }
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
                return View();
            }

            return View(users);
        }
    }
}