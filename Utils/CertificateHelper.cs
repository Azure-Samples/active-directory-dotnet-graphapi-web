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

namespace AadExplorer
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Helper class to read and use certificates.
    /// </summary>
    public class CertificateHelper
    {
        /// <summary>
        /// Get the signing certificate.
        /// </summary>
        /// <returns>Signing certificate.</returns>
        public static X509Certificate2 GetSigningCertificate()
        {
            X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            x509Store.Open(OpenFlags.ReadOnly);

            try
            {
                X509Certificate2Collection matchingCerts = x509Store.Certificates.Find(
                    X509FindType.FindByThumbprint, Constants.CertificateThumbprint, false); // TODO: Change it to true

                if (matchingCerts == null || matchingCerts.Count != 1)
                {
                    throw new ArgumentException("invalid certificate thumbprint", "certificateThumbprint");
                }
                
                return matchingCerts[0];
            }
            finally
            {
                if (x509Store != null)
                {
                    x509Store.Close();
                }
            }
        }
    }
}