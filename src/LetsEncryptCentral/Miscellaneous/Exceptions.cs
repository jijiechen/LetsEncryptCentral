using ACMESharp;
using System;
using System.Collections.Generic;

namespace LetsEncryptCentral
{
    class AuthorizationFailedException: Exception
    {
        public IList<AuthorizationState> AuthorizationResults { get; }

        public AuthorizationFailedException(IList<AuthorizationState> results)
            : base("One or more hostname is failed to be authorized")
        {
            AuthorizationResults = results;
        }
    }


    class CertificateApplicationException: Exception
    {
        public CertificateRequest CertificateRequest { get; }

        public string CommonName { get; }

        public string[] AlternativeDnsNames { get; } = new string[0];

        public CertificateApplicationException(CertificateRequest certRequest, string commonName, string[] alternativeDnsNames = null)
            : base($"{certRequest.StatusCode} returned when trying to creating the certificate for {commonName}")
        {
            this.CertificateRequest = certRequest;
            this.CommonName = commonName;

            if(alternativeDnsNames != null)
            {
                this.AlternativeDnsNames = alternativeDnsNames;
            }
        }
    }

}
