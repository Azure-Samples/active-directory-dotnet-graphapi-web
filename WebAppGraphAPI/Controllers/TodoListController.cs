using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Configuration;
using System.Threading.Tasks;
using System.Security.Claims;
using WebAppGraphAPI.Utils;
using WebAppGraphAPI.Models;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace WebAppGraphAPI.Controllers
{
    [Authorize]
    public class TodoListController : Controller
    {
        private string todoListResourceId = "https://graphDir1.onMicrosoft.com/TodoListService";
        private string todoListBaseAddress = "https://localhost:44321";
        private const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";

        //
        // GET: /TodoList/
        public async Task<ActionResult> Index()
        {
            //
            // Retrieve the user's tenantID and access token since they are parameters used to call the To Do service.
            //
            string tenantId = ClaimsPrincipal.Current.FindFirst(TenantIdClaimType).Value;
            string accessToken = TokenCacheUtils.GetAccessTokenFromCacheOrRefreshToken(tenantId, todoListResourceId);
            List<TodoItem> itemList = new List<TodoItem>();

            //
            // If the user doesn't have an access token, they need to re-authorize.
            //
            if (accessToken == null)
            {
                //
                // If refresh is set to true, the user has clicked the link to be sent to be authorized again.
                //
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
                TodoItem newItem = new TodoItem();
                newItem.Title = "(Sign-in required to view to do list.)";
                itemList.Add(newItem);
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View(itemList);
            }

            //
            // Retrieve the user's To Do List.
            //
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, todoListBaseAddress + "/api/todolist");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await client.SendAsync(request);

            //
            // Return the To Do List in the view.
            //
            if (response.IsSuccessStatusCode)
            {
                List<Dictionary<String, String>> responseElements = new List<Dictionary<String, String>>();
                JsonSerializerSettings settings = new JsonSerializerSettings();
                String responseString = await response.Content.ReadAsStringAsync();
                responseElements = JsonConvert.DeserializeObject<List<Dictionary<String, String>>>(responseString, settings);
                foreach (Dictionary<String, String> responseElement in responseElements)
                {
                    TodoItem newItem = new TodoItem();
                    newItem.Title = responseElement["Title"];
                    newItem.Owner = responseElement["Owner"];
                    itemList.Add(newItem);
                }

                return View(itemList);
            }
            else
            {
                //
                // If the call failed with access denied, then drop the current access token from the cache, 
                //     and show the user an error indicating they might need to sign-in again.
                //
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TokenCacheUtils.RemoveAccessTokenFromCache(todoListResourceId);

                    ViewBag.ErrorMessage = "UnexpectedError";
                    TodoItem newItem = new TodoItem();
                    newItem.Title = "(No items in list)";
                    itemList.Add(newItem);
                    return View(itemList);
                }
            }

            //
            // If the call failed for any other reason, show the user an error.
            //
            return View("Error");

        }

        [HttpPost]
        public async Task<ActionResult> Index(string item)
        {
            if (ModelState.IsValid)
            {
                //
                // Retrieve the user's tenantID and access token since they are parameters used to call the To Do service.
                //
                string tenantId = ClaimsPrincipal.Current.FindFirst(TenantIdClaimType).Value;
                string accessToken = TokenCacheUtils.GetAccessTokenFromCacheOrRefreshToken(tenantId, todoListResourceId);
                List<TodoItem> itemList = new List<TodoItem>();

                //
                // If the user doesn't have an access token, they need to re-authorize.
                //
                if (accessToken == null)
                {
                    //
                    // The user needs to re-authorize.  Show them a message to that effect.
                    //
                    TodoItem newItem = new TodoItem();
                    newItem.Title = "(No items in list)";
                    itemList.Add(newItem);
                    ViewBag.ErrorMessage = "AuthorizationRequired";
                    return View(itemList);
                }

                // Forms encode todo item, to POST to the todo list web api.
                HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("Title", item) });

                //
                // Add the item to user's To Do List.
                //
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, todoListBaseAddress + "/api/todolist");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Content = content;
                HttpResponseMessage response = await client.SendAsync(request);

                //
                // Return the To Do List in the view.
                //
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    //
                    // If the call failed with access denied, then drop the current access token from the cache, 
                    //     and show the user an error indicating they might need to sign-in again.
                    //
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        TokenCacheUtils.RemoveAccessTokenFromCache(todoListResourceId);

                        ViewBag.ErrorMessage = "UnexpectedError";
                        TodoItem newItem = new TodoItem();
                        newItem.Title = "(No items in list)";
                        itemList.Add(newItem);
                        return View(newItem);
                    }
                }

                //
                // If the call failed for any other reason, show the user an error.
                //
                return View("Error");

            }

            return View("Error");
        }
	}
}