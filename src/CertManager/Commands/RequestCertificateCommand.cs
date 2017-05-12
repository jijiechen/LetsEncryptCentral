using System;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using ACMESharp.PKI;
using System.IO;
using CertManager.DnsProviders;
using ACMESharp;
using ACMESharp.JOSE;
using System.Collections.Generic;

namespace CertManager.Commands
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


            CertificateProvider certProvider = null;
            var client = ClientHelper.CreateAcmeClient(requestContext.Signer, requestContext.Registration);

            try
            {
                if(IsSubDomainName(options.CommonName, out string toplevel))
                {
                    DnsAuthorizer.Authorize(client, requestContext.DnsProvider, toplevel);
                }
                DnsAuthorizer.Authorize(client, requestContext.DnsProvider, options.CommonName);

                certProvider = CertificateProvider.GetProvider();
                var cert = CertificateClient.RequestCertificate(client, certProvider, options.CommonName);

                var outputFile = PrepareOutputFilePath(options, Path.GetDirectoryName(options.OutputFile));
                CertificateExporter.Export(certProvider, cert, options.OutputType, outputFile);
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

        static string PrepareOutputFilePath(RequestNewCertificateOptions options, string outDir)
        {
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            return Path.Combine(Path.GetDirectoryName(options.OutputFile), Path.GetFileName(options.OutputFile));
        }

        private CertRequestContext InitializeRequestContext(RequestNewCertificateOptions options)
        {
            var context = new CertRequestContext();

            try
            {
                context.Registration = RegistrationHelper.LoadFromFile(options.RegisterationFile);
            }catch(Exception ex)
            {
                Console.WriteLine($"Could not load registration file: {ex.Message}");
                goto errorHandling;
            }

            try
            {
                context.Signer = SignerHelper.LoadFromFile(options.SignerFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load registration file: {ex.Message}");
                goto errorHandling;
            }
            

            try
            {
                var dnsProviderType = AllSupportedDnsProviderTypes[options.DnsProviderName];
                context.DnsProvider = Activator.CreateInstance(dnsProviderType) as IDnsProvider;

                context.DnsProvider.Initialize(options.DnsProviderConfiguration);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Could not load registration file: {ex.Message}");
                goto errorHandling;
            }


            errorHandling:
            return null;
        }

        static bool OptionsInBadFormat(RequestNewCertificateOptions options, out int exitCode)
        {
            if (string.IsNullOrEmpty(options.CommonName))
            {
                Console.Error.WriteLine("Could not request a certificate without a common name.");
                exitCode = 21;
                return true;
            }

            if (!File.Exists(options.RegisterationFile))
            {
                Console.Error.WriteLine($"Registeration file does not exist at {options.RegisterationFile}.");
                exitCode = 22;
                return true;
            }


            if (!File.Exists(options.SignerFile))
            {
                Console.Error.WriteLine($"Signer file does not exist at {options.SignerFile}.");
                exitCode = 23;
                return true;
            }

            if (!AllSupportedDnsProviderTypes.Keys.Contains(options.DnsProviderName))
            {
                Console.Error.WriteLine($"Unknown DNS provider '{options.DnsProviderName}'. The supported providers are: {AllSupportedDnsProviderTypes}");
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

            public CertOutputType OutputType { get; set; } = CertOutputType.Pfx;
            public string OutputFile { get; set; }
            
            public string SignerFile { get; set; }
            public string RegisterationFile { get; set; }

            public string DnsProviderName { get; set; }
            public string DnsProviderConfiguration { get; set; }
        }

    }

}
