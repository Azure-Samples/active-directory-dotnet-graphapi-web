using Microsoft.Azure.ActiveDirectory.GraphClient;
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

        /// <summary>
        /// Gets a list of <see cref="Group"/> objects from Graph.
        /// </summary>
        /// <returns>A view with the list of <see cref="Group"/> objects.</returns>
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

            //Setup GRaph API connection and get a list of groups
            Guid ClientRequestId = Guid.NewGuid();
            GraphSettings graphSettings = new GraphSettings();
            graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
            GraphConnection graphConnection = new GraphConnection(accessToken, ClientRequestId, graphSettings);

            PagedResults<Group> pagedResults = graphConnection.List<Group>(null, new FilterGenerator());

            return View(pagedResults.Results);
        }

        /// <summary>
        /// Gets details of a single <see cref="Group"/> Graph.
        /// </summary>
        /// <returns>A view with the details of a single <see cref="Group"/>.</returns>
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

            // Setup Graph API connection and get single Group
            Guid ClientRequestId = Guid.NewGuid();
            GraphSettings graphSettings = new GraphSettings();
            graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
            GraphConnection graphConnection = new GraphConnection(accessToken, ClientRequestId, graphSettings);

            Group group = graphConnection.Get<Group>(objectId);
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
            string accessToken = AuthUtils.GetAuthToken(Request, HttpContext);
            if (accessToken == null)
            {
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
                GraphConnection graphConnection = new GraphConnection(accessToken, ClientRequestId, graphSettings);
                group.MailEnabled = false;

                graphConnection.Add(group);
                return RedirectToAction("Index");
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View();
            }
        }

        /// <summary>
        /// Creates a view to for editing an existing <see cref="Group"/> in Graph.
        /// </summary>
        /// <param name="objectId">Unique identifier of the <see cref="Group"/>.</param>
        /// <returns>A view with details to edit <see cref="Group"/>.</returns>
        public ActionResult Edit(string objectId)
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

            // Setup Graph API connection and get a single Group
            Guid ClientRequestId = Guid.NewGuid();
            GraphSettings graphSettings = new GraphSettings();
            graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
            GraphConnection graphConnection = new GraphConnection(accessToken, ClientRequestId, graphSettings);

            Group group = graphConnection.Get<Group>(objectId);
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
            string accessToken = AuthUtils.GetAuthToken(Request, HttpContext);
            if (accessToken == null)
            {
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
                GraphConnection graphConnection = new GraphConnection(accessToken, ClientRequestId, graphSettings);
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
            string accessToken = AuthUtils.GetAuthToken(Request, HttpContext);
            if (accessToken == null)
            {
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
                GraphConnection graphConnection = new GraphConnection(accessToken, ClientRequestId, graphSettings);
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
            string accessToken = AuthUtils.GetAuthToken(Request, HttpContext);
            if (accessToken == null)
            {
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
                GraphConnection graphConnection = new GraphConnection(accessToken, ClientRequestId, graphSettings);
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

            GraphObject graphGroup = graphConnection.Get<Group>(objectId);
            PagedResults<GraphObject> memberShip = graphConnection.GetLinkedObjects(graphGroup, LinkProperty.MemberOf, null, 999);

            IList<Group> groupMemberShip = new List<Group>();
            foreach (GraphObject graphObj in memberShip.Results)
            {
                if (graphObj is Group)
                {
                    Group group = (Group)graphObj;
                    groupMemberShip.Add(group);
                }
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
            string accessToken = AuthUtils.GetAuthToken(Request, HttpContext);
            if (accessToken == null)
            {
                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View();
            }

            // Setup Graph API connection and get Group members
            Guid ClientRequestId = Guid.NewGuid();
            GraphSettings graphSettings = new GraphSettings();
            graphSettings.ApiVersion = GraphConfiguration.GraphApiVersion;
            GraphConnection graphConnection = new GraphConnection(accessToken, ClientRequestId, graphSettings);

            Group group = graphConnection.Get<Group>(objectId);
            PagedResults<GraphObject> members = graphConnection.GetLinkedObjects(group, LinkProperty.Members, null, 999);

            IList<User> users = new List<User>();

            foreach (GraphObject obj in members.Results)
            {
                if (obj is User)
                {
                    users.Add((User)obj);
                }
            }

            return View(users);
        }
	}
}