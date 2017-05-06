using ACMESharp;
using ACMESharp.JOSE;
using System.IO;
using ACMESharp.PKI;
using CertManager.DnsProviders;
using System;

namespace CertManager
{
    class Program
    {
        public static CertManagerConfiguration GlobalConfiguration = new CertManagerConfiguration();

        static void Main(string[] args)
        {
            var appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var signerPath = Path.Combine(appPath, "signer");
            var registrationPath = Path.Combine(appPath, "registration");

            var defaultContact = "someone@someplace.com";
            var hostName = "hehe.com";
            var dnspodUser = "me";
            var dnspodApiKey = "you";
            var pfxFilePath = Path.Combine(Directory.GetCurrentDirectory(), hostName + ".pfx");
            var pfxPassword = string.Empty;

            Console.Title = "CertManager";
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            RS256Signer signer;
            AcmeRegistration registration;
            if (File.Exists(registrationPath) && File.Exists(signerPath))
            {
                registration = RegistrationHelper.LoadFromFile(registrationPath);
                signer = SignerHelper.LoadFromFile(signerPath);
            }
            else
            {
                signer = new RS256Signer();
                signer.Init();

                registration = RegistrationHelper.CreateNew(signer, defaultContact);
                SignerHelper.SaveToFile(signer, signerPath);
            }


            var client = ClientHelper.CreateAcmeClient(signer, registration);
            var certProvider = CertificateProvider.GetProvider();

            try
            {
                var dnsProvider = new DnsPodProvider(dnspodUser, dnspodApiKey, hostName);
                DnsAuthorizer.Authorize(client, dnsProvider, hostName);

                var cert = CertificateClient.RequestCertificate(client, certProvider, hostName);
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
    }
}
