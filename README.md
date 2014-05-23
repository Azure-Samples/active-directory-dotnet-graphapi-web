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

`git clone git@github.com:AzureADSamples/WebApp-GraphAPI-DotNet.git`

### Step 2:  Run the sample in Visual Studio 2013

The sample app is preconfigured to read data from a Demonstration company (GraphDir1.onMicrosoft.com) in Azure AD. Run the sample application, and from the main page, authenticate using this demo user account: demoUser@graphDir1.onMicrosoft.com   graphDem0

### Step 3:  Running this application with your Azure Active Directory tenant

#### Register the MVC Sample app for your own tenant

1. Sign in to the [Azure management portal](https://manage.windowsazure.com).
2. Click on Active Directory in the left hand nav.
3. Click the directory tenant where you wish to register the sample application.
4. Click the Applications tab.
5. In the drawer, click Add.
6. Click "Add an application my organization is developing".
7. Enter a friendly name for the application, for example "WebApp for Azure AD", select "Web Application and/or Web API", and click next.
8. For the sign-on URL, enter the base URL for the sample, which is by default `https://localhost:44322`.
9. For the App ID URI, enter `https://<your_tenant_name>/WebAppGraphAPI`, replacing `<your_tenant_name>` with the domain name of your Azure AD tenant. For Example "https://contoso.com/WebAppGraphAPI".  Click OK to complete the registration.
10. While still in the Azure portal, click the Configure tab of your application.
11. Find the Client ID value and copy it aside, you will need this later when configuring your application.
12. In the Reply URL, add the reply URL address used to return the authorization code returned during Authorization code flow.  For example: "https://localhost:44322/"
13. Under the Keys section, select either a 1year or 2year key - the keyValue will be displayed after you save the configuration at the end - it will be displayed, and you should save this to a secure location.  Note, that the key value is only displayed once, and you will not be able to retrieve it later.  
14. Configure Permissions - under the "Permissions to other applications" section, select application "Windows Azure Active Directory" (this is the Graph API), and under the second permission (Delegated permissions), select "Access your organization's directory" and "Enable sign-on and read users' profiles". The 2nd column (Application permission) is not needed for this demo app.
Notes: the permission "Access your organization's directory" allows the application to access your organization's directory on behalf of the signed-in user - this is a delegation permission and must be consented by the Administrator for webApps (such as this demo app).
The permission "Enable sign-on and read users' profiles" allows users to sign in to the application with their organizational accounts and lets the application read the profiles of signed-in users, such as their email address and contact information - this is a delegation permission, and can be consented to by the user.
The other permissions, "Read Directory data" and "Read and write Directory data", are Delegation and Application Permissions, which only the Administrator can grant consent to.

15. Selct the Save button at the bottom of the screen - upon sucessful configuration, your Key value should now be displayed - please copy and store this value in a secure location.

15. You will need to update the webconfig file of this Application project. From Visual Studio, open the web.config file, and under the <appSettings> section, modify "ida:ClientId" and "ida:AppKey" and " with the values from the previous steps.  Also update the "ida:Tenant" with your Azure AD Tenant's domain name e.g. Contoso.onMicrosoft.com, (or Contoso.com if that domain is owned by your tenant).
16. In `web.config` add this line in the `<system.web>` section: `<sessionState timeout="525600" />`.  This increases the ASP.Net session state timeout to it's maximum value so that access tokens and refresh tokens cache in session state aren't cleared after the default timeout of 20 minutes.
17. Build and run your application - you will need to authenticate with valid user credentials for your company when you run the application.

Note: there is an issues with uploading jpeg file images for user's thumbnail photos when using this app
from Chrome or FireFox (Internet Explorer works) - we will debug this in more detail and update when this issue
is addressed.

## How To Deploy This Sample to Azure

Coming soon.

## About The Code

Coming soon.

