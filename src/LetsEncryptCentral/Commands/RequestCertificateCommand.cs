using ACMESharp;
using ACMESharp.JOSE;
using ACMESharp.PKI;
using LetsEncryptCentral.CertManager;
using LetsEncryptCentral.DnsProviders;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static LetsEncryptCentral.ConsoleUtils;
using static LetsEncryptCentral.PathUtils;

namespace LetsEncryptCentral.Commands
{

    class RequestCertificateCommand
    {
        static readonly Dictionary<string, Type> AllSupportedDnsProviderTypes = new Dictionary<string, Type>
        {
            {"DnsPod", typeof(DnsPodProvider) }
        };


        public void Setup(CommandLineApplication command)
        {
            command.Description = "Request a new certificate from the open Let's Encrypt CA.";

            var argCN = command.Argument("cn", "Common name of the certificate");

            var optionOutFile = command.Option("-o|--out <OUT_FILE>", "The output file path to which the issued certificate file generate.", CommandOptionType.SingleValue);
            var optionOutType = command.Option("-t|--out-type <OUT_TYPE>", "The file type to export from the issued certificate.", CommandOptionType.SingleValue);

            var optionReg = command.Option("--reg <REG_FILE>", "The file that contains the registeration that will be used to request the certificate.", CommandOptionType.SingleValue);
            var optionSigner = command.Option("--signer <SIGNER_FILE>", "The signer correspondes to the registeration file.", CommandOptionType.SingleValue);

            var optionDnsName = command.Option("--dns <DNS_PROVIDER_NAME>", "The provider program name of your dynamic dns service provider.", CommandOptionType.SingleValue);
            var optionDnsConf = command.Option("--dns-conf <DNS_PROVIDER_CONFIGURATION>", "Configuration string to initialize the DNS provider program.", CommandOptionType.SingleValue);


            command.HelpOption("-?|-h|--help");
            command.OnExecute(() =>
            {
                var opt = new RequestNewCertificateOptions
                {
                    CommonName = argCN.Value?.Trim(),
                    OutputFile = optionOutFile.Value()?.Trim(),

                    RegisterationFile = optionReg.Value()?.Trim(),
                    SignerFile = optionSigner.Value()?.Trim(),

                    DnsProviderName = optionDnsName.Value()?.Trim(),
                    DnsProviderConfiguration = optionDnsConf.Value()?.Trim()
                };

                CertOutputType outType;
                if(Enum.TryParse(optionOutType.Value(), out outType))
                {
                    opt.OutputType = outType;
                }

                return Execute(opt);
            });
        }


        int Execute(RequestNewCertificateOptions options)
        {
            if(OptionsInBadFormat(options, out int errorCode))
            {
                return errorCode;
            }

            var requestContext = InitializeRequestContext(options);
            if (requestContext == null)
            {
                return 210;
            }

            Console.Write("Initializing...");
            CertificateProvider certProvider = null;
            var client = ClientHelper.CreateAcmeClient(requestContext.Signer, requestContext.Registration);

            try
            {
                Console.WriteLine("Done.");
                if (IsSubDomainName(options.CommonName, out string toplevel))
                {
                    Console.Write("Authorizing top level domain name {0}...", toplevel);
                    DnsAuthorizer.Authorize(client, requestContext.DnsProvider, toplevel);
                    Console.WriteLine("Done.");
                }
                Console.Write("Authorizing domain name {0}...", options.CommonName);
                DnsAuthorizer.Authorize(client, requestContext.DnsProvider, options.CommonName);
                Console.WriteLine("Done.");

                Console.Write("Requesting a new certificate for common name {0}...", options.CommonName);
                certProvider = CertificateProvider.GetProvider();
                var cert = CertificateClient.RequestCertificate(client, certProvider, options.CommonName);

                Console.WriteLine("Done.");
                Console.WriteLine("Exporting certificate to file...");

                var outTypeString = options.OutputType.ToString().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(options.OutputFile))
                {
                    options.OutputFile = Path.Combine(AppliationPath, string.Concat(options.CommonName, '-', DateTime.Now.ToString("yyyyMMddHHmm"), '.', outTypeString));
                }
                options.OutputFile = PrepareOutputFilePath(options.OutputFile, out string dir);
                CertificateExporter.Export(certProvider, cert, options.OutputType, options.OutputFile);
                Console.WriteLine("Certificate has been exported as {0} format at {1}.", outTypeString, options.OutputFile); 
            }
            finally
            {
                client.Dispose();
                certProvider?.Dispose();
                requestContext.Signer.Dispose();
                requestContext.DnsProvider.Dispose();                
            }

            return 0;
        }

        static bool IsSubDomainName(string domainName, out string toplevel)
        {
            var parts = domainName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if(parts.Length > 2)
            {
                toplevel = string.Concat(parts[parts.Length - 2], '.', parts[parts.Length - 1]);
                return true;
            }

            toplevel = domainName;
            return false;
        }

        private CertRequestContext InitializeRequestContext(RequestNewCertificateOptions options)
        {
            var context = new CertRequestContext();

            try
            {
                context.Registration = RegistrationHelper.LoadFromFile(options.RegisterationFile);
            }catch(Exception ex)
            {
                ConsoleErrorOutput($"Could not load registration file: {ex.Message}");
                goto errorHandling;
            }

            try
            {
                context.Signer = SignerHelper.LoadFromFile(options.SignerFile);
            }
            catch (Exception ex)
            {
                ConsoleErrorOutput($"Could not load signer file: {ex.Message}");
                goto errorHandling;
            }
            

            try
            {
                var dnsProviderType = AllSupportedDnsProviderTypes[options.DnsProviderName];
                context.DnsProvider = Activator.CreateInstance(dnsProviderType) as IDnsProvider;

                context.DnsProvider.Initialize(options.DnsProviderConfiguration ?? string.Empty);
            }
            catch(Exception ex)
            {
                ConsoleErrorOutput($"Could not initialize dns provider: {ex.Message}");
                goto errorHandling;
            }
            return context;

            errorHandling:
            return null;
        }

        static bool OptionsInBadFormat(RequestNewCertificateOptions options, out int exitCode)
        {
            if (string.IsNullOrEmpty(options.CommonName))
            {
                ConsoleErrorOutput("Could not request a certificate without a common name.");
                exitCode = 21;
                return true;
            }

            if (!File.Exists(options.RegisterationFile))
            {
                ConsoleErrorOutput($"Registeration file does not exist at {options.RegisterationFile}.");
                exitCode = 22;
                return true;
            }


            if (!File.Exists(options.SignerFile))
            {
                ConsoleErrorOutput($"Signer file does not exist at {options.SignerFile}.");
                exitCode = 23;
                return true;
            }

            if (!AllSupportedDnsProviderTypes.Keys.Contains(options.DnsProviderName))
            {
                ConsoleErrorOutput($"Unknown DNS provider '{options.DnsProviderName}'. The supported providers are: {AllSupportedDnsProviderTypes}");
                exitCode = 24;
                return true;
            }
            
            exitCode = 0;
            return false;
        }


        class CertRequestContext
        {
            public ISigner Signer { get; set; }
            public AcmeRegistration Registration { get; set; }
            public IDnsProvider DnsProvider { get; set; }
        }

        class RequestNewCertificateOptions
        {
            public string CommonName { get; set; }

            public CertOutputType OutputType { get; set; } = CertOutputType.Pem;
            public string OutputFile { get; set; }
            
            public string SignerFile { get; set; }
            public string RegisterationFile { get; set; }

            public string DnsProviderName { get; set; }
            public string DnsProviderConfiguration { get; set; }
        }

    }

}
