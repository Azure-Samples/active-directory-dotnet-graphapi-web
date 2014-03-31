namespace AadExplorer
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Web;

    public class Constants
    {
        /// <summary>
        /// Url format for authorization endpoint.
        /// </summary>
        public const string AuthorizeEndpointFormat =
            "https://{0}/common/oauth2/authorize?response_type=code id_token&client_id={1}&resource={2}&redirect_uri={3}&scope=openid&nonce=abcd";

        /// <summary>
        /// Url format for authority url.
        /// </summary>
        public const string AuthorityUrlFormat = "https://{0}/{1}";

        /// <summary>
        /// Consent url format.
        /// </summary>
        public const string ConsentUrlFormat = "https://{0}/Consent.aspx?ClientId={1}";

        /// <summary>
        /// Sign out url format.
        /// </summary>
        public const string SignOutUrlFormat = "https://{0}/common/oauth2/logout?post_logout_redirect_uri={1}";

        /// <summary>
        /// Tenant id claim type
        /// </summary>
        public const string TenantIdClaimType = "tid";

        /// <summary>
        /// Login url format
        /// </summary>
        public const string LoginUrlFormat = "https://{0}/{1}";

        #region Controller names

        /// <summary>
        /// Name of the OAuth controller.
        /// </summary>
        public const string OAuthControllerName = "OAuth";

        /// <summary>
        /// Graph api controller.
        /// </summary>
        public const string GraphApiControllerName = "GraphApi";

        /// <summary>
        /// Users controller.
        /// </summary>
        public const string UsersControllerName = "Users";

        /// <summary>
        /// Users controller.
        /// </summary>
        public const string HomeControllerName = "Home";

        #endregion

        #region Request context

        /// <summary>
        /// Tenant id key
        /// </summary>
        public const string TenantIdKey = "TenantId";

        /// <summary>
        /// Sign up token key.
        /// </summary>
        public const string SignupTokenKey = "signupToken";

        /// <summary>
        // Name of the access token cookie.
        /// </summary>
        public const string AccessTokenCookieName = "AccessToken";
        
        #endregion

        #region Read only

        /// <summary>
        /// Graph user url format
        /// </summary>
        public const string GraphUserUrlFormat = "https://{0}/{1}/users/{2}?api-version=2013-11-08";

        /// <summary>
        /// Client app id
        /// </summary>
        public static readonly string AppPrincipalId = ConfigurationManager.AppSettings["ida:ClientID"];

        /// <summary>
        /// Client app password.
        /// </summary>
        public static readonly string AppKey = ConfigurationManager.AppSettings["ida:Password"];

        /// <summary>
        /// IdentifierUri of the application.
        /// </summary>
        public static readonly string ApplicationIdentifierUri = ConfigurationManager.AppSettings["ida:IdentifierUri"];

        /// <summary>
        /// Domain of the application.
        /// </summary>
        public static readonly string ApplicationDomainName = ConfigurationManager.AppSettings["ida:AppDomainName"];

        /// <summary>
        /// Graph domain name
        /// </summary>
        public static readonly string GraphDomainName = ConfigurationManager.AppSettings["ida:GraphDomainName"];
    
        /// <summary>
        /// Graph resource name.
        /// </summary>
        public static readonly string ResourceId = String.Format("https://{0}", GraphDomainName);
    
        /// <summary>
        /// Domain name for the login endpoint.
        /// </summary>
        public static readonly string LoginDomainName = ConfigurationManager.AppSettings["ida:LoginDomainName"];
    
        /// <summary>
        /// Reply url for catching the auth code.
        /// </summary>
        public static readonly string ReplyUrlForCatchingCode = ConfigurationManager.AppSettings["ida:AppReplyUrl"];

        /// <summary>
        /// Signing certificate thumbprint.
        /// </summary>
        public static readonly string CertificateThumbprint = ConfigurationManager.AppSettings["ida:CertificateThumbprint"];

        /// <summary>
        /// api-version used to query Graph API.
        /// </summary>
        public static readonly string GraphApiVersion = ConfigurationManager.AppSettings["ida:GraphApiVersion"];

        /// <summary>
        /// Consent url.
        /// </summary>
        public static readonly string ConsentUrl = String.Format(
            ConsentUrlFormat,
            ConfigurationManager.AppSettings["ida:ConsentDomainName"],
            AppPrincipalId);

        /// <summary>
        /// Endpoint for authorizing an access.
        /// </summary>
        public static readonly string AuthorizeUrl = String.Format(
            Constants.AuthorizeEndpointFormat,
            LoginDomainName,
            AppPrincipalId,
            HttpUtility.UrlEncode(ResourceId),
            HttpUtility.UrlEncode(ReplyUrlForCatchingCode));

        /// <summary>
        /// Sign out url
        /// </summary>
        public static readonly string SignOutUrl = String.Format(
            Constants.SignOutUrlFormat,
            LoginDomainName,
            HttpUtility.UrlEncode(ConfigurationManager.AppSettings["ida:SignOutUrl"]));

        #endregion
    }
}