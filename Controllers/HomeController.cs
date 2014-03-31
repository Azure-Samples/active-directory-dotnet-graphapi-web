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
    using System.Web.Mvc;
    using Microsoft.Azure.ActiveDirectory.GraphClient;

    /// <summary>
    /// Home controller.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Creates a home view.
        /// </summary>
        /// <returns>Home page view.</returns>
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to the Azure Active Dirctory Explorer.";
            return View();
        }

        /// <summary>
        /// Creates a view to display the information of the application.
        /// </summary>
        /// <returns>View to display the information of the application.</returns>
        public ActionResult About()
        {
            ViewBag.Message = "Explore Your Directory.";

            return View();
        }


        /// <summary>
        /// Creates a view to display who to contact for more information.
        /// </summary>
        /// <returns>View to display the contact information of the application..</returns>
        public ActionResult Contact()
        {
            ViewBag.Message = "Microsoft Corp";
            return View();
        }

        /// <summary>
        /// Creates a view to display <see cref="User"/> objects of Graph.
        /// </summary>
        /// <returns>View to display the information of the application.</returns>
        public ActionResult Users()
        {
            return RedirectToAction("Index", "Users");
        }

        /// <summary>
        /// Creates a view when protected resources are accessed without logging in.
        /// </summary>
        /// <returns>A view when protected resources are accessed without logging in.</returns>
        public ActionResult AccessDenied()
        {
            ViewBag.Message = "Please sign in before accesssing any page.";
            return View();
        }

        /// <summary>
        /// Redeem auth code for an access token
        /// </summary>
        /// <param name="code">Auth code.</param>
        /// <param name="id_token">Id token.</param>
        /// <returns>View</returns>
        public ActionResult CatchCode(string code, string id_token)
        {
            return (new OAuthController()).RedeemCode(code, id_token, HttpContext.Response);
        }
    }
}
