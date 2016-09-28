#region

using System;
using System.Collections.Generic;
using System.Data.Services.Client;
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
    ///     User controller to get/set/update/delete users.
    /// </summary>
    [Authorize]
    public class UsersController : Controller
    {
        /// <summary>
        ///     Gets a list of <see cref="User" /> objects from Graph.
        /// </summary>
        /// <returns>A view with the list of <see cref="User" /> objects.</returns>
        public async Task<ActionResult> Index()
        {
            var userList = new List<User>();
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                IPagedCollection<IUser> pagedCollection = await client.Users.ExecuteAsync();
                if (pagedCollection != null)
                {
                    do
                    {
                        List<IUser> usersList = pagedCollection.CurrentPage.ToList();
                        foreach (IUser user in usersList)
                        {
                            userList.Add((User) user);
                        }
                        pagedCollection = await pagedCollection.GetNextPageAsync();
                    } while (pagedCollection != null);
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
                    HttpContext.GetOwinContext()
                        .Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationType);
                }

                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View(userList);
            }
            return View(userList);
        }

        /// <summary>
        ///     Gets details of a single <see cref="User" /> Graph.
        /// </summary>
        /// <returns>A view with the details of a single <see cref="User" />.</returns>
        public async Task<ActionResult> Details(string objectId)
        {
            User user = null;
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                user = (User) await client.Users.GetByObjectId(objectId).ExecuteAsync();
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

            return View(user);
        }

        /// <summary>
        ///     Creates a view to for adding a new <see cref="User" /> to Graph.
        /// </summary>
        /// <returns>A view with the details to add a new <see cref="User" /> objects</returns>
        public async Task<ActionResult> Create()
        {
            return View();
        }

        /// <summary>
        ///     Processes creation of a new <see cref="User" /> to Graph.
        /// </summary>
        /// <returns>A view with the details to all <see cref="User" /> objects</returns>
        [HttpPost]
        public async Task<ActionResult> Create(
            [Bind(
                Include =
                    "UserPrincipalName,AccountEnabled,PasswordProfile,MailNickname,DisplayName,GivenName,Surname,JobTitle,Department"
                )] User user)
        {
            ActiveDirectoryClient client = null;
            try
            {
                client = AuthenticationHelper.GetActiveDirectoryClient();
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

            try
            {
                await client.Users.AddUserAsync(user);
                return RedirectToAction("Index");
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View();
            }
        }

        /// <summary>
        ///     Creates a view to for editing an existing <see cref="User" /> in Graph.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="User" />.</param>
        /// <returns>A view with details to edit <see cref="User" />.</returns>
        public async Task<ActionResult> Edit(string objectId)
        {
            User user = null;
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                user = (User) await client.Users.GetByObjectId(objectId).ExecuteAsync();
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

            return View(user);
        }

        /// <summary>
        ///     Gets a list of <see cref="Group" /> objects that a given <see cref="User" /> is member of.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="User" />.</param>
        /// <returns>A view with the list of <see cref="Group" /> objects.</returns>
        public async Task<ActionResult> GetGroups(string objectId)
        {
            IList<Group> groupMembership = new List<Group>();

            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();

                IUser user = await client.Users.GetByObjectId(objectId).ExecuteAsync();
                var userFetcher = (IUserFetcher) user;

                IPagedCollection<IDirectoryObject> pagedCollection = await userFetcher.MemberOf.ExecuteAsync();
                do
                {
                    List<IDirectoryObject> directoryObjects = pagedCollection.CurrentPage.ToList();
                    foreach (IDirectoryObject directoryObject in directoryObjects)
                    {
                        if (directoryObject is Group)
                        {
                            var group = directoryObject as Group;
                            groupMembership.Add(group);
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

            return View(groupMembership);
        }

        /// <summary>
        ///     Processes editing of an existing <see cref="User" />.
        /// </summary>
        /// <param name="user"><see cref="User" /> to be edited.</param>
        /// <param name="values">user input from the form</param>
        /// <param name="photoFile">thumbnail photo file</param>
        /// <returns>A view with list of all <see cref="User" /> objects.</returns>
        [HttpPost]
        public async Task<ActionResult> Edit(
            User user, FormCollection values, HttpPostedFileBase photoFile)
        {
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                string userId = user.ObjectId ?? RouteData.Values["id"].ToString();
                IUser toUpdate = await client.Users.GetByObjectId(userId).ExecuteAsync();
                Helper.CopyUpdatedValues(toUpdate, user, values);
                await toUpdate.UpdateAsync();
                if (photoFile != null && photoFile.ContentLength > 0)
                {
                    await toUpdate.ThumbnailPhoto.UploadAsync(photoFile.InputStream, "application/image");
                }
                return RedirectToAction("Index");
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View();
            }
        }

        /// <summary>
        ///     Creates a view to delete an existing <see cref="User" />.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="User" />.</param>
        /// <returns>A view of the <see cref="User" /> to be deleted.</returns>
        public async Task<ActionResult> Delete(string objectId)
        {
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                var user = (User) await client.Users.GetByObjectId(objectId).ExecuteAsync();
                return View(user);
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View();
            }
        }

        /// <summary>
        ///     Processes the deletion of a given <see cref="User" />.
        /// </summary>
        /// <param name="user"><see cref="User" /> to be deleted.</param>
        /// <returns>A view to display all the existing <see cref="User" /> objects.</returns>
        [HttpPost]
        public async Task<ActionResult> Delete(User user)
        {
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                IUser toDelete = await client.Users.GetByObjectId(user.ObjectId).ExecuteAsync();
                await toDelete.DeleteAsync();
                return RedirectToAction("Index");
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View(user);
            }
        }

        /// <summary>
        ///     Gets a list of <see cref="User" /> objects that a given <see cref="User" /> has as a direct report.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="User" />.</param>
        /// <returns>A view with the list of <see cref="User" /> objects.</returns>
        public async Task<ActionResult> GetDirectReports(string objectId)
        {
            List<User> reports = new List<User>();
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                IUser user = await client.Users.GetByObjectId(objectId).ExecuteAsync();
                var userFetcher = user as IUserFetcher;
                IPagedCollection<IDirectoryObject> directReports = await userFetcher.DirectReports.ExecuteAsync();
                do
                {
                    List<IDirectoryObject> directoryObjects = directReports.CurrentPage.ToList();
                    foreach (IDirectoryObject directoryObject in directoryObjects)
                    {
                        if (directoryObject is User)
                        {
                            reports.Add((User) directoryObject);
                        }
                    }
                    directReports = await directReports.GetNextPageAsync();
                } while (directReports != null);
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

            return View(reports);
        }

        public async Task<ActionResult> ShowThumbnail(string id)
        {
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                IUser user = await client.Users.GetByObjectId(id).ExecuteAsync();
                DataServiceStreamResponse dataServiceStreamResponse = null;
                dataServiceStreamResponse = await user.ThumbnailPhoto.DownloadAsync();
                if (dataServiceStreamResponse != null)
                {
                    return File(dataServiceStreamResponse.Stream, "image/jpeg");
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
                    HttpContext.GetOwinContext()
                        .Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationType);
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