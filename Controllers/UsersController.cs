// Copyright © Microsoft
//
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION
// ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A
// PARTICULAR PURPOSE, MERCHANTABILITY OR NON-INFRINGEMENT.
//
// See the Apache License, Version 2.0 for the specific language
// governing permissions and limitations under the License.

namespace AadExplorer.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using AadExplorer.Utils;
    using Microsoft.Azure.ActiveDirectory.GraphClient;

    /// <summary>
    /// User controller to get/set/update/delete users.
    /// </summary>
    [AccessTokenAuthorize]
    public class UsersController : Controller
    {
        /// <summary>
        /// Gets a list of <see cref="User"/> objects from Graph.
        /// </summary>
        /// <returns>A view with the list of <see cref="User"/> objects.</returns>
        public ActionResult Index()
        {
            if (!AuthenticationHelper.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            CallContext currentCallContext = new CallContext(AccessTokenCookie.GetCurrent().AccessToken, Guid.NewGuid(), AadExplorer.Constants.GraphApiVersion);
            //CallContext currentCallContext = new CallContext(AccessTokenCookie.GetCurrent().AccessToken);
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
            CallContext currentCallContext = new CallContext(
                AccessTokenCookie.GetCurrent().AccessToken, Guid.NewGuid(), AadExplorer.Constants.GraphApiVersion);
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
            try
            {
                CallContext currentCallContext = new CallContext(
                    AccessTokenCookie.GetCurrent().AccessToken, Guid.NewGuid(), AadExplorer.Constants.GraphApiVersion);
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
            CallContext currentCallContext = new CallContext(
                AccessTokenCookie.GetCurrent().AccessToken, Guid.NewGuid(), AadExplorer.Constants.GraphApiVersion);
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
            CallContext currentCallContext = new CallContext(
                AccessTokenCookie.GetCurrent().AccessToken, Guid.NewGuid(), AadExplorer.Constants.GraphApiVersion);
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
            try
            {
                // TODO: Add update logic here
                CallContext currentCallContext = new CallContext(
                AccessTokenCookie.GetCurrent().AccessToken, Guid.NewGuid(), AadExplorer.Constants.GraphApiVersion);
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
            try
            {
                CallContext currentCallContext = new CallContext(
                AccessTokenCookie.GetCurrent().AccessToken, Guid.NewGuid(), AadExplorer.Constants.GraphApiVersion);
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
            try
            {
                CallContext currentCallContext = new CallContext(
                AccessTokenCookie.GetCurrent().AccessToken, Guid.NewGuid(), AadExplorer.Constants.GraphApiVersion);
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
