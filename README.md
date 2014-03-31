WebApp-GraphAPI-DotNet
=========================================
This is a sample MVC Web application that shows how to make basic Azure Active Directory Graph API calls. It includes uses the Azure AD Authenication Library (ADAL .Net) for OAuth 2.0, and the new Graph API .Net libray. This sample Application demostrates both the OAuth 2.0 Client Credential authentication flow (Client ID/Secret), and OAuth 2.0 Authorization code flow.

For more information about how the protocols work in this scenario and other scenarios, see the REST API and Authentication Scenarios on http://msdn.microsoft.com/aad.

## How To Run This Sample

To run this sample you will need:
- Visual Studio 2012 or 2013
- An Internet connection
- An Azure subscription (a free trial is sufficient)

Every Azure subscription has an associated Azure Active Directory tenant.  If you don't already have an Azure subscription, you can get a free subscription by signing up at [http://wwww.windowsazure.com](http://www.windowsazure.com).  All of the Azure AD features used by this sample are available free of charge.

### Step 1:  Clone or download this repository

From your shell or command line:

`git clone git@github.com:AzureADSamples/WebApp-GraphAPI-DotNet.git`

### Step 2:  Run the sample in Visual Studio

The sample app is preconfigured to read data from a Demonstration company (GraphDir1.onMicrosoft.com) in Azure AD. Run the sample application, and from the main page, authenticate using either the Application or User Authentication.

### Step 3:  Running this application with your Azure Active Directory tenant

### Step 3:  Register the sample with your Azure Active Directory tenant

#### Register the MVC Sample app for your own tenant

1. Sign in to the [Azure management portal](https://manage.windowsazure.com).
2. Click on Active Directory in the left hand nav.
3. Click the directory tenant where you wish to register the sample application.
4. Click the Applications tab.
5. In the drawer, click Add.
6. Click "Add an application my organization is developing".
7. Enter a friendly name for the application, for example "WebApp for Azure AD", select "Web Application and/or Web API", and click next.
8. For the sign-on URL, enter the base URL for the sample, which is by default `https://localhost:44321`.
9. For the App ID URI, enter `https://<your_tenant_name>/MvcGraphApp`, replacing `<your_tenant_name>` with the domain name of your Azure AD tenant. For Example "https://contoso.com/MvcGraphApp".  Click OK to complete the registration.
10. While still in the Azure portal, click the Configure tab of your application.
11. Find the Client ID value and copy it aside, you will need this later when configuring your application.
12. Add a key- select a key duration of either 1 year or 2 year. When you save this page, the key value will be displayed, copy and save the value in a safe location - you will need this key later to configurate OAuth Client Credentials for this app - this key value will not be displayed again, nor retrievable by any other means.
13. In the Reply URL, add the reply URL address used to return the authorization code returned during the OAuth 2.0 Authorization code flow.  For example: "https://localhost:44312/Home/CatchCode".
14. Permissions must be configured if you want to use both OAuth 2.0 Client Credential and Authoriztion Code authentication flows.  Under the "Permissions to other applications" section, select application "Windows Azure Active Directory" (this is the Graph API), and under the first permission (Application permission), select Read or Read+Write Directory Data. For the Authorization Code flow, select the appropriate permission for user delegated permissions.
<TODO List information on each type of delegated permssion>

## How To Deploy This Sample to Azure

Coming soon.

## About The Code

Coming soon.

