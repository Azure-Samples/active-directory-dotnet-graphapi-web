using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Globalization;

namespace WebAppGraphAPI.Utils
{
    public class TokenCacheUtils
    {
        private static readonly string clientId = ConfigurationManager.AppSettings["ida:ClientID"];
        private static readonly string AppKey = ConfigurationManager.AppSettings["ida:AppKey"];
        private static readonly string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private const string CachePrefix = "AzureAdCache#";

        public static string GetAccessTokenFromCacheOrRefreshToken(string tenantId, string resourceId)
        {
            //
            // First try to get an access token for this resource from the cookie-based cache.
            // If there is no access token in the cache for this resource, see if there is a refresh token in the cache that can be used to get a new access token.
            // If all fails, return null signalling the caller to do an OpenID Connect sign-in request.
            //
            string accessToken = (string)GetAccessTokenFromCache(resourceId);

            if (accessToken != null) return accessToken;

            accessToken = GetAccessTokenFromRefreshToken(tenantId, resourceId);

            if (accessToken != null) return accessToken;

            return null;
        }

        public static string GetAccessTokenFromRefreshToken(string tenantId, string resourceId)
        {
            //
            // Try to get a new access token for this resource using a refresh token.
            // If this fails, return null signalling the caller to do an OpenID Connect sign-in request.
            //
            AuthenticationResult result = null;
            string refreshToken = null;

            //
            // Fetch the refresh token from the cache
            //
            refreshToken = (string)GetRefreshTokenFromCache();
            if (refreshToken == null)
            {
                //
                // No refresh token - the caller will need to send the user to get an auth code.  Return null.
                //
                return null;
            }

            try
            {
                //
                // Redeem the refresh token for an access token
                //
                ClientCredential clientcred = new ClientCredential(clientId, AppKey);
                string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenantId);
                AuthenticationContext authcontext = new AuthenticationContext(authority);
                result = authcontext.AcquireTokenByRefreshToken(refreshToken, clientId, clientcred, resourceId);

                //
                // Save the authorization header for this resource and the refresh token in separate cookies
                //
                SaveAccessTokenInCache(resourceId, result.AccessToken, (result.ExpiresOn.AddMinutes(-5)).ToString());
                SaveRefreshTokenInCache(result.RefreshToken);

                return result.AccessToken;
            }
            catch
            {
                //
                // If the refresh token is also expired, remove it from the cache, and send the user off to do an OpenID Connect sign-in request.
                //
                RemoveRefreshTokenFromCache();

                return null;
            }

        }

        public static void SaveAccessTokenInCache(string resourceId, object value, object expiration)
        {
            System.Web.HttpContext.Current.Session[CachePrefix + "AccessToken#" + resourceId] = value;
            System.Web.HttpContext.Current.Session[CachePrefix + "AccessTokenExpiration#" + resourceId] = expiration;
        }

        public static object GetAccessTokenFromCache(string resourceId)
        {
            string accessToken = (string)System.Web.HttpContext.Current.Session[CachePrefix + "AccessToken#" + resourceId];

            if (accessToken != null)
            {
                string expiration = (string)System.Web.HttpContext.Current.Session[CachePrefix + "AccessTokenExpiration#" + resourceId];
                DateTime expirationTime = Convert.ToDateTime(expiration);

                if (expirationTime < DateTime.Now)
                {
                    RemoveAccessTokenFromCache(resourceId);
                    accessToken = null;
                }

            }

            return accessToken;
        }

        public static void RemoveAccessTokenFromCache(string resourceId)
        {
            System.Web.HttpContext.Current.Session.Remove(CachePrefix + "AccessToken#" + resourceId);
            System.Web.HttpContext.Current.Session.Remove(CachePrefix + "AccessTokenExpiration#" + resourceId);
        }

        public static void SaveRefreshTokenInCache(object value)
        {
            System.Web.HttpContext.Current.Session[CachePrefix + "RefreshToken"] = value;
        }

        public static object GetRefreshTokenFromCache()
        {
            return System.Web.HttpContext.Current.Session[CachePrefix + "RefreshToken"];
        }

        public static void RemoveRefreshTokenFromCache()
        {
            System.Web.HttpContext.Current.Session.Remove(CachePrefix + "RefreshToken");
        }

        public static void RemoveAllFromCache()
        {
            List<string> keysToRemove = new List<string>();
            foreach (object session in System.Web.HttpContext.Current.Session)
            {
                string sessionName = (string)session;
                if (sessionName.StartsWith(CachePrefix, StringComparison.Ordinal))
                {
                    keysToRemove.Add(sessionName);
                }
            }

            foreach (string key in keysToRemove)
            {
                System.Web.HttpContext.Current.Session.Remove(key);
            }
        }

    }
}