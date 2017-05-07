using ACMESharp;
using ACMESharp.JOSE;
using System.IO;
using ACMESharp.PKI;
using CertManager.DnsProviders;
using System;
using System.Linq;

namespace CertManager
{
    class Program
    {
        public static CertManagerConfiguration GlobalConfiguration = new CertManagerConfiguration();

        static void Main(string[] args)
        {
            var appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var clientSignerPath = Path.Combine(appPath, "sign.key");
            var registrationPath = Path.Combine(appPath, "reg.json");

            var defaultContact = "someone@someplace.com";
            var subdomain = "net";
            var pfxPassword = string.Empty;
#if !DEBUG
            GlobalConfiguration.AcmeServerBaseUri = "https://acme-v01.api.letsencrypt.org/";
#endif
            Console.Title = "CertManager";
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            RS256Signer signer;
            AcmeRegistration registration;
            AcmeClient client;
            if (File.Exists(registrationPath) && File.Exists(clientSignerPath))
            {
                registration = RegistrationHelper.LoadFromFile(registrationPath);
                signer = SignerHelper.LoadFromFile(clientSignerPath);

                client = ClientHelper.CreateAcmeClient(signer, registration);
            }
            else
            {
                signer = new RS256Signer();
                signer.Init();
                SignerHelper.SaveToFile(signer, clientSignerPath);

                client = ClientHelper.CreateAcmeClient(signer, null);
                client.Registration = registration = RegistrationHelper.CreateNew(client, defaultContact);
                RegistrationHelper.SaveToFile(registration, registrationPath);
            }
            
            var certProvider = CertificateProvider.GetProvider();

            try
            {
                var dnsProvider = CreateDnsPodProviderFromFile(out string domainName);
                DnsAuthorizer.Authorize(client, dnsProvider, domainName);

                var hostName = domainName;
                if (!string.IsNullOrEmpty(subdomain))
                {
                    hostName = $"{subdomain}.{domainName}";
                    DnsAuthorizer.Authorize(client, dnsProvider, hostName);
                }

                var cert = CertificateClient.RequestCertificate(client, certProvider, hostName);
                var pfxFilePath = Path.Combine(Directory.GetCurrentDirectory(), hostName + ".pfx");
                ExportPfx(certProvider, cert, pfxFilePath, pfxPassword);
            }
            finally
            {
                signer.Dispose();
                client.Dispose();
                certProvider.Dispose();
            }
        }


        static void ExportPfx(CertificateProvider certProvider, IssuedCertificate certificate, string filePath, string password)
        {
            using (var fileStream = File.Create(filePath))
            {
                certProvider.ExportArchive(
                    certificate.PrivateKey,
                    new[] { certificate.PublicKey, certificate.CAPublicKey },
                    ArchiveFormat.PKCS12,
                    fileStream,
                    password);
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.Error.WriteLine("!!! Unhandled exception has occured,  application is now exiting!");

            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                Console.Error.WriteLine(exception.Message);
                Console.Error.WriteLine(exception.StackTrace);
            }
       }

        static DnsPodProvider CreateDnsPodProviderFromFile(out string domainName)
        {
            var lines = File.ReadLines("dnspod-token.txt").ToArray();
            domainName = lines[2];
            return new DnsPodProvider(int.Parse(lines[0]), lines[1], domainName);
        } 
    }
}
