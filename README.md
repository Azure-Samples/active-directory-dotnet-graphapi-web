WebApp-GraphAPI-DotNet
=========================================
This is a sample MVC Web application that shows how to make RESTful calls to the Graph API to access Azure Active Directory data. It includes use of OWIN libraries to authenticate/authorize using Open ID connect, and a Graph API .Net library - these libraries are both available as Nuget packages. 

For more information about how the protocols work in this scenario and other scenarios, see the REST API and Authentication Scenarios on http://msdn.microsoft.com/aad.

## How To Run This Sample

To run this sample you will need:
- Visual 2013
- An Internet connection
- An Azure subscription (a free trial is sufficient)

Every Azure subscription has an associated Azure Active Directory tenant.  If you don't already have an Azure subscription, you can get a free subscription by signing up at [http://wwww.windowsazure.com](http://www.windowsazure.com).  All of the Azure AD features used by this sample are available free of charge.

### Step 1:  Clone or download this repository

From your shell or command line:

`git clone https://github.com/AzureADSamples/WebApp-GraphAPI-DotNet.git`

### Step 2:  Run the sample in Visual Studio 2013

The sample app is preconfigured to read data from a Demonstration company (GraphDir1.onMicrosoft.com) in Azure AD. Run the sample application, and from the main page, authenticate using this demo user account: demoUser@graphDir1.onMicrosoft.com   graphDem0

### Step 3:  Run the application with your own AAD tenant

#### Register the MVC Sample app in your tenant

NOTE: If you already have a user account in your Azure Active Directory tenant, you can skip to the next step.  This sample will not work with a Microsoft account, so if you signed in to the Azure portal with a Microsoft account and have never created a user account in your directory before, you need to do that now.  If you create an account and want to use it to sign-in to the Azure portal, don't forget to add the user account as a co-administrator of your Azure subscription.

1. Sign in to the [Azure management portal](https://manage.windowsazure.com).
2. Click on Active Directory in the left hand nav.
3. Click the directory tenant where you wish to register the sample application.
4. Click the Applications tab.
5. In the drawer, click Add.
6. Click "Add an application my organization is developing".
7. Enter a friendly name for the application, for example "WebApp-GraphAPI", select "Web Application and/or Web API", and click next.
8. For the sign-on URL, enter the base URL for the sample, which is by default `https://localhost:44322/`. NOTE: It is important, due to the way Azure AD matches URLs, to ensure there is a trailing slash on the end of this URL. If you don't include the trailing slash, you will receive an error when the application attempts to redeem an authorization code.
9. For the App ID URI, enter `https://<your_tenant_name>/WebAppGraphAPI`, replacing `<your_tenant_name>` with the domain name of your Azure AD tenant. For Example "https://contoso.com/WebAppGraphAPI".  Click OK to complete the registration.
10. While still in the Azure portal, click the Configure tab of your application.
11. Find the Client ID value and copy it aside, you will need this later when configuring your application.
12. In the Reply URL, add the reply URL address used to return the authorization code returned during Authorization code flow.  For example: "https://localhost:44322/"
13. Under the Keys section, select either a 1 year or 2 year key - the keyValue will be displayed after you save the configuration at the end.
14. Configure Permissions - under the "Permissions to other applications" section, select application "Windows Azure Active Directory" (this is the Graph API), and under the second permission (Delegated permissions), select "Access your organization's directory" and "Enable sign-on and read users' profiles". The 2nd column (Application permission) is not needed for this demo app.
15. Select the Save button at the bottom of the screen - upon sucessful configuration, your Key value should now be displayed - please copy and store this value in a secure location.  The key value is only displayed once, and you will not be able to retrieve it later.

NOTE: the permission "Access your organization's directory" allows the application to access your organization's directory on behalf of the signed-in user - this is a delegation permission and must be consented by the Administrator for web apps (such as this demo app).
The permission "Enable sign-on and read users' profiles" allows users to sign in to the application with their organizational accounts and lets the application read the profiles of signed-in users, such as their email address and contact information - this is a delegation permission, and can be consented to by the user.
The other permissions, "Read Directory data" and "Read and write Directory data", are Delegation and Application Permissions, which only the Administrator can grant consent to.

#### Configure the sample to use your tenant

1. You will need to update the `web.config` file of the sample. From Visual Studio, open the `web.config` file, and under the `<appSettings>` section, modify `"ida:ClientId"` and `"ida:AppKey"` with the values from the previous steps.  Also update the `"ida:Tenant"` with your Azure AD Tenant's domain name e.g. `contoso.onMicrosoft.com`, (or `contoso.com` if that domain is owned by your tenant).
2. In `web.config` add this line in the `<system.web>` section: `<sessionState timeout="525600" />`.  This increases the ASP.Net session state timeout to it's maximum value so that access tokens and refresh tokens cache in session state aren't cleared after the default timeout of 20 minutes.
3. Build and run your application - you will need to authenticate with valid user credentials for your company when you run the application.

Note: there is an issues with uploading jpeg file images for user's thumbnail photos when using this app
from Chrome or FireFox (Internet Explorer works) - we will debug this in more detail and update when this issue
is addressed.

## How To Deploy This Sample to Azure

To deploy the sample to Azure Web Sites, you will create a web site, publish the Visual Studio project to the site, and update your tenant to use the web site instead of IIS Express.

### Create and Publish the sample to an Azure Web Site

1. Sign in to the [Azure management portal](https://manage.windowsazure.com).
2. Click on Web Sites in the left hand nav.
3. Click New in the bottom left hand corner, select Compute --> Web Site --> Quick Create, select the hosting plan and region, and give your web site a name, e.g. webappgraphapi-contoso.azurewebsites.net.  Click Create Web Site.
4. Once the web site is created, click on it to manage it.  For this set of steps, download the publish profile and save it.  Other deployment mechanisms, such as from source control, can also be used.
5. Switch to Visual Studio and go to the WebAppGraphAPI project.  Right click on the project in the Solution Explorer and select Publish.  Click Import, and import the publish profile that you just downloaded.
6. On the Connection tab, update the Destination URL so that it is https, for example https://webappgraphapi-contoso.azurewebsites.net.  Click Next.
7. On the Settings tab, make sure Enable Organizational Authentication is NOT selected.  Click Publish.
8. Visual Studio will publish the project and automatically open a browser to the URL of the project.  If you see the default web page of the project, the publication was successful.

### Update the Application Configurations in the Directory Tenant

1. Navigate to the [Azure management portal](https://manage.windowsazure.com).
2. In the left hand nav, click on Active Directory and select your tenant.
3. On the applications tab, select the appropriate application.
4. On the Configure tab, update the Sign-On URL and Reply URL fields to the address of your service, for example https://webappgraphapi-contoso.azurewebsites.net/.  Save the configuration.

## About The Code

Coming soon.

## How To Recreate This Sample

First, in Visual Studio 2013 create an empty solution to host the  projects.  Then, follow these steps to create the sample.

### Creating the WebAppGraphAPI Project

1. In the solution, create a new ASP.Net MVC web application called WebAppGraphAPI with Authentication set to No Authentication.
2. Set SSL Enabled to be True.  Note the SSL URL.
3. In the project properties, Web properties, set the Project Url to be the SSL URL.
4. Add the following ASP.Net OWIN middleware NuGets: Microsoft.IdentityModel.Protocol.Extensions, System.IdentityModel.Tokens.Jwt, Microsoft.Owin.Security.OpenIdConnect, Microsoft.Owin.Security.Cookies, Microsoft.Owin.Host.SystemWeb.
5. Add the stable release Active Directory Authentication Library NuGet (`Microsoft.IdentityModel.Clients.ActiveDirectory`).
6. Add the AAD GraphAPI client library NuGet (`Microsoft.Azure.ActiveDirectory.GraphClient`) version 1.0.3 using the Package Manager Console: `Install-Package Microsoft.Azure.ActiveDirectory.GraphClient -Version 1.0.3`
7. In the `App_Start` folder, create a class `Startup.Auth.cs`.  You will need to remove `.App_Start` from the namespace name.  Replace the code for the `Startup` class with the code from the same file of the sample app.  Be sure to take the whole class definition!  The definition changes from `public class Startup` to `public partial class Startup`.
8. Right-click on the project, select Add,  select "OWIN Startup class", and name the class "Startup".  If "OWIN Startup Class" doesn't appear in the menu, instead select "Class", and in the search box enter "OWIN".  "OWIN Startup class" will appear as a selection; select it, and name the class `Startup.cs`.
9. In `Startup.cs`, replace the code for the `Startup` class with the code from the same file of the sample app.  Again, note the definition changes from `public class Startup` to `public partial class Startup`.
10. In the `Views` --> `Shared` folder, create a new partial view `_LoginPartial.cshtml`.  Replace the contents of the file with the contents of the file of same name from the sample.
11. In the `Views` --> `Shared` folder, replace the contents of `_Layout.cshtml` with the contents of the file of same name from the sample.  Effectively, all this will do is add a single line, `@Html.Partial("_LoginPartial")`, that lights up the previously added `_LoginPartial` view.
12. If you want the user to be required to sign-in before they can see any page of the app, then in the `HomeController`, decorate the `HomeController` class with the `[Authorize]` attribute.  If you leave this out, the user will be able to see the home page of the app without having to sign-in first, and can click the sign-in link on that page to get signed in.
13. In the `Models` folder add a new class called `UserProfile.cs`.  Copy the implementation of UserProfile from this sample into the class.
14. In the project, create a new folder called `Utils`.  In the folder, create three new classes called `NaiveSessionCache.cs`, `GraphConfiguration.cs` and `Logger.cs`.  Copy their implementations from the sample.
15. Add a new empty MVC5 controller `AccountController` to the project.  Replace the implementation with the contents of the file of same name from the sample.
16. Add a new empty MVC5 controller `ContactsController` to the project.  Copy the implementation of the controller from the sample.  Remember to include the [Authorize] attribute on the class definition.
17. Add three views in the `Views/Contacts` folder, `Index`, `Details`, and `GetGroups`. Copy their implemenations from the sample. 
18. Add a new empty MVC5 controller `GroupsController` to the project.  Copy the implementation of the controller from the sample.  Again, remember to include the [Authorize] attribute on the class definition.
19. Add seven views in the `Views/Groups` folder, `Index`, `Details`, `GetGroups`, `Delete`, `GetMembers`, `Edit`, and `Create`. Copy their implemenations from the sample. 
20. Add a new empty MVC5 controller `RolesController` to the project.  Copy the implementation of the controller from the sample.  Again, remember to include the [Authorize] attribute on the class definition.
21. Add three views in the `Views/Roles` folder, `Index`, `Details`, and `GetMembers`. Copy their implemenations from the sample. 
22. Add a new empty MVC5 controller `UserProfileController` to the project.  Copy the implementation of the controller from the sample.  Again, remember to include the [Authorize] attribute on the class definition.
23. Add one view in the `Views/UserProfile` folder, `Index`. Copy its implemenation from the sample. 
24. Add a new empty MVC5 controller `UsersController` to the project.  Copy the implementation of the controller from the sample.  Again, remember to include the [Authorize] attribute on the class definition.
25. Add seven views in the `Views/Users` folder, `Index`, `Details`, `GetGroups`, `Delete`, `GetDirectReports`, `Edit`, and `Create`. Copy their implemenations from the sample. 
26. In `web.config`, in `<appSettings>`, create keys for `ida:ClientId`, `ida:AppKey`, `ida:AADInstance`, `ida:Tenant`, `ida:PostLogoutRedirectUri`, `ida:GraphApiVersion`, and `ida:GraphUrl` and set the values accordingly.  For the public Azure AD, the value of `ida:AADInstance` is `https://login.windows.net/{0}` the value of `ida:GraphResourceId` is `https://graph.windows.net`, and the value of `ida:GraphUrl` is `https://graph.windows.net/`.
27. In `web.config` add this line in the `<system.web>` section: `<sessionState timeout="525600" />`.  This increases the ASP.Net session state timeout to it's maximum value so that access tokens and refresh tokens cache in session state aren't cleared after the default timeout of 20 minutes.
28. Follow the steps in the "Run the application with your own AAD tenant" above to run the newly created sample.

