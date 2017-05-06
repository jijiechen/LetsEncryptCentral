using ACMESharp;
using ACMESharp.HTTP;
using ACMESharp.JOSE;
using ACMESharp.PKI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace CertManager
{
    class IssuedCertificate
    {
        public Crt PublicKey { get; set; }
        public PrivateKey PrivateKey { get; set; }
        public Crt CAPublicKey { get; set; }
    }

    class CertificateClient
    {
        public static IssuedCertificate RequestCertificate(AcmeClient client, CertificateProvider certProvider, string hostName, List<string> alternativeNames = null)
        {
            if (alternativeNames == null)
            {
                alternativeNames = new List<string>();
            }

            var csr = GenerateCSR(certProvider, hostName, alternativeNames, out PrivateKey pk);

            var certRequest = client.RequestCertificate(JwsHelper.Base64UrlEncode(csr));
            if (certRequest.StatusCode != HttpStatusCode.Created)
            {
                throw new CertificateApplicationException(certRequest, hostName, alternativeNames.ToArray());
            }

            Crt crt;
            using (var ms = new MemoryStream())
            {
                certRequest.SaveCertificate(ms);
                crt = certProvider.ImportCertificate(EncodingFormat.DER, ms);
            }
            var caCrt = GetCACertificate(certProvider, certRequest);

            return new IssuedCertificate { PublicKey = crt, PrivateKey = pk, CAPublicKey = caCrt };
        }

        static byte[] GenerateCSR(CertificateProvider provider, string hostName, List<string> alternativeNames, out PrivateKey privateKey)
        {
            var csrDetails = new CsrDetails
            {
                CommonName = hostName
            };

            if (alternativeNames.Count > 0)
            {
                csrDetails.AlternativeNames = alternativeNames;
            }

            privateKey = provider.GeneratePrivateKey(new RsaPrivateKeyParams() { NumBits = Program.GlobalConfiguration.RSAKeyBits });
            var csr = provider.GenerateCsr(new CsrParams { Details = csrDetails, }, privateKey, Crt.MessageDigest.SHA256);

            byte[] derRaw;
            using (var bs = new MemoryStream())
            {
                provider.ExportCsr(csr, EncodingFormat.DER, bs);
                derRaw = bs.ToArray();
            }

            return derRaw;
        }

        static Crt GetCACertificate(CertificateProvider cp, CertificateRequest certRequest)
        {
            var links = new LinkCollection(certRequest.Links);
            var upLink = links.GetFirstOrDefault("up");

            var temporaryFileName = Path.GetTempFileName();
            try
            {
                var uri = new Uri(new Uri(Program.GlobalConfiguration.AcmeServerBaseUri), upLink.Uri);
                TheWebClient.DownloadFile(uri, temporaryFileName);

                var cacert = new X509Certificate2(temporaryFileName);
                var serverSN = cacert.GetSerialNumberString();


                using (Stream source = new FileStream(temporaryFileName, FileMode.Open))
                {
                    return cp.ImportCertificate(EncodingFormat.DER, source);
                }
            }
            finally
            {
                if (File.Exists(temporaryFileName))
                    File.Delete(temporaryFileName);
            }
        }

        static WebClient TheWebClient = new WebClient();
    }
}
