using Microsoft.Azure.ActiveDirectory.GraphClient;
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

        /// <summary>
        /// Gets a list of <see cref="Contact"/> objects from Graph.
        /// </summary>
        /// <returns>A view with the list of <see cref="Contact"/> objects.</returns>
        public ActionResult Index()
        {
            //Get the access token as we need it to make a call to the Graph API
            string accessToken = AuthUtils.GetAuthToken(Request, HttpContext);
            if (accessToken == null)
            {
                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View();
            }

            //Setup Graph API connection and get a list of users
            Guid ClientRequestId = Guid.NewGuid();
            GraphSettings graphSettings = new GraphSettings();
            graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
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
            //Get the access token as we need it to make a call to the Graph API
            string accessToken = AuthUtils.GetAuthToken(Request, HttpContext);
            if (accessToken == null)
            {
                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View();
            }

            // Setup Graph API connection and get single Contact
            Guid ClientRequestId = Guid.NewGuid();
            GraphSettings graphSettings = new GraphSettings();
            graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
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

            //Get the access token as we need it to make a call to the Graph API
            string accessToken = AuthUtils.GetAuthToken(Request, HttpContext);
            if (accessToken == null)
            {
                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View();
            }

            // Setup Graph API connection and get Group membership
            Guid ClientRequestId = Guid.NewGuid();
            GraphSettings graphSettings = new GraphSettings();
            graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
            GraphConnection graphConnection = new GraphConnection(accessToken, ClientRequestId, graphSettings);

            GraphObject graphContact = graphConnection.Get<Contact>(objectId);
            PagedResults<GraphObject> memberShip = graphConnection.GetLinkedObjects(graphContact, LinkProperty.MemberOf, null, 999);

            // filter for Groups only
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

            return View(groupMembership);
        }
	}
}