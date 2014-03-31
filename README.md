WebApp-GraphAPI-DotNet
=========================================
This is a sample MVC Web application that shows how to make basic Azure Active Directory Graph API calls. It inlcudes uses the Azure AD Authenication Library (ADAL .Net) for OAuth 2.0, and the new Graph API .Net libray.

For more information about how the protocols work in this scenario and other scenarios, see the REST API and Authentication Scenarios on http://msdn.microsoft.com/aad.

## How To Run This Sample

To run this sample you will need:
- Visual Studio 2013
- An Internet connection
- An Azure subscription (a free trial is sufficient)

Every Azure subscription has an associated Azure Active Directory tenant.  If you don't already have an Azure subscription, you can get a free subscription by signing up at [http://wwww.windowsazure.com](http://www.windowsazure.com).  All of the Azure AD features used by this sample are available free of charge.

### Step 1:  Clone or download this repository

From your shell or command line:

`git clone git@github.com:WindowsAzureADSamples/WebApp-GraphAPI-DotNet.git`

### Step 2:  Run the sample in Visual Studio

The sample app is preconfigured to read data from a Demonstration company (GraphDir1.onMicrosoft.com) in Azure AD. Run the sample application, and from the main page, authenticate using either the Application or User Authentication.

### Step 3:  Running this application with your Azure Active Directory tenant

### Step 3:  Register the sample with your Azure Active Directory tenant

#### Register the MVC Sample app within your tenant

1. Sign in to the [Azure management portal](https://manage.windowsazure.com).
2. Click on Active Directory in the left hand nav.
3. Click the directory tenant where you wish to register the sample application.
4. Click the Applications tab.
5. In the drawer, click Add.
6. Click "Add an application my organization is developing".
7. Enter a friendly name for the application, for example "WebApp for Azure AD", select "Web Application and/or Web API", and click next.
8. For the sign-on URL, enter the base URL for the sample, which is by default `https://localhost:44321`.
9. For the App ID URI, enter `https://<your_tenant_name>/MvcGraphApp`, replacing `<your_tenant_name>` with the name of your Azure AD tenant.  Click OK to complete the registration.
10. While still in the Azure portal, click the Configure tab of your application.
11. Find the Client ID value and copy it aside, you will need this later when configuring your application.
<TODO Add instructions on configuring permissoins>

## How To Deploy This Sample to Azure

Coming soon.

## About The Code

Coming soon.

