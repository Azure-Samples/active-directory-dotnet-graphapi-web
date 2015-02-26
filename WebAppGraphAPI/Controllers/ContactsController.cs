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
    ///     Contact controller to get/set/update/delete users.
    /// </summary>
    [Authorize]
    public class ContactsController : Controller
    {
        /// <summary>
        ///     Gets a list of <see cref="Contact" /> objects from Graph.
        /// </summary>
        /// <returns>A view with the list of <see cref="Contact" /> objects.</returns>
        public async Task<ActionResult> Index()
        {
            List<Contact> contactList = new List<Contact>();

            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                // Get all results into a list
                IPagedCollection<IContact> contacts = await client.Contacts.ExecuteAsync();
                do
                {
                    List<IContact> currentPage = contacts.CurrentPage.ToList();
                    foreach (IContact contact in currentPage)
                    {
                        contactList.Add((Contact) contact);
                    }
                    contacts = await contacts.GetNextPageAsync();
                } while (contacts != null);
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

            return View(contactList);
        }

        /// <summary>
        ///     Gets details of a single <see cref="Contact" /> Graph.
        /// </summary>
        /// <returns>A view with the details of a single <see cref="Contact" />.</returns>
        public async Task<ActionResult> Details(string objectId)
        {
            Contact contact = null;
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                contact = (Contact) await client.Contacts.GetByObjectId(objectId).ExecuteAsync();
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

            return View(contact);
        }

        /// <summary>
        ///     Gets a list of <see cref="Group" /> objects that a given <see cref="Contact" /> is member of.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="Contact" />.</param>
        /// <returns>A view with the list of <see cref="Group" /> objects.</returns>
        public async Task<ActionResult> GetGroups(string objectId)
        {
            IList<Group> groupMembership = new List<Group>();
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                IContact contact = await client.Contacts.GetByObjectId(objectId).ExecuteAsync();
                IContactFetcher contactFetcher = contact as IContactFetcher;
                IPagedCollection<IDirectoryObject> pagedCollection = await contactFetcher.MemberOf.ExecuteAsync();
                do
                {
                    List<IDirectoryObject> currentPage = pagedCollection.CurrentPage.ToList();
                    foreach (IDirectoryObject directoryObject in currentPage)
                    {
                        if (directoryObject is Group)
                        {
                            groupMembership.Add((Group) directoryObject);
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
    }
}