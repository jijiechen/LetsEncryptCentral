using ACMESharp;
using ACMESharp.JOSE;
using System;
using System.Net;

namespace LetsEncryptCentral.CertManager
{
    class ClientHelper
    {
        public static AcmeClient CreateAcmeClient(ISigner signer, AcmeRegistration registration)
        {
            var client = new AcmeClient(new Uri(Program.GlobalConfiguration.AcmeServerBaseUri), new AcmeServerDirectory(), signer, registration);
            if (!string.IsNullOrWhiteSpace(Program.GlobalConfiguration.ProxyUri))
            {
                client.Proxy = new WebProxy(Program.GlobalConfiguration.ProxyUri, false, new string[0], new NetworkCredential(Program.GlobalConfiguration.ProxyUserName, Program.GlobalConfiguration.ProxyPassword));
            }

            client.Init();
            client.GetDirectory(true);
            return client;
        }
    }
}
