using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppGraphAPI.Models
{
    public class UserProfile
    {
        public string ObjectType { get; set; }
        public List<object> AssignedLicenses { get; set; }
        public List<object> AssignedPlans { get; set; }
        public object City { get; set; }
        public object Country { get; set; }
        public object Department { get; set; }
        public object DirSyncEnabled { get; set; }
        public string DisplayName { get; set; }
        public object FacsimileTelephoneNumber { get; set; }
        public string GivenName { get; set; }
        public object ImmutableId { get; set; }
        public object JobTitle { get; set; }
        public object LastDirSyncTime { get; set; }
        public object Mail { get; set; }
        public string MailNickname { get; set; }
        public object Mobile { get; set; }
        public List<string> OtherMails { get; set; }
        public string PasswordPolicies { get; set; }
        public object PasswordProfile { get; set; }
        public object PhysicalDeliveryOfficeName { get; set; }
        public object PostalCode { get; set; }
        public object PreferredLanguage { get; set; }
        public List<object> ProvisionedPlans { get; set; }
        public List<object> ProvisioningErrors { get; set; }
        public List<object> ProxyAddresses { get; set; }
        public object State { get; set; }
        public object StreetAddress { get; set; }
        public string Surname { get; set; }
        public object TelephoneNumber { get; set; }
        public object UsageLocation { get; set; }
        public string UserPrincipalName { get; set; }
        public string UserType { get; set; }
    }
}