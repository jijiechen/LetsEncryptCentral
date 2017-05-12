using ACMESharp.JOSE;
using LetsEncryptCentral.CertManager;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.IO;

namespace LetsEncryptCentral.Commands
{
    class RegisterAccountCommand
    {
        public void Setup(CommandLineApplication command)
        {
            command.Description = "Create a new Let's Encrypt registration.";

            var optionTos = command.Option("--agree-tos", "Accept to the terms of services and subscriber agreement at https://letsencrypt.org/repository/", CommandOptionType.SingleValue);
            var optionContact = command.Option("-c|--contact <CONTACT_EMAIL>", "Email address to contact.", CommandOptionType.SingleValue);
            var optionOutReg = command.Option("-r|--out-reg <REGISTERTION_OUTPUT_FILE>", "A file path to output registeration information.", CommandOptionType.SingleValue);
            var optionOutSigner = command.Option("-s|--out-signer <REGISTERTION_OUTPUT_SIGNER>", "A file path to output signer information corresponds to the registeration.", CommandOptionType.SingleValue);
            
            command.HelpOption("-?|-h|--help");
            command.OnExecute(() =>
            {
                var opt = new RegisterCommandOptions
                {
                    AcceptTos = optionTos.HasValue(),
                    ContactEmailAddress = optionContact.Value(),
                    OutputPathRegisteration = optionOutReg.Value(),
                    OutputPathSigner = optionOutSigner.Value()
                };
                return Execute(opt);
            });
        }

        int Execute(RegisterCommandOptions options)
        {
            if (!options.AcceptTos)
            {
                Console.Error.WriteLine("Could not create a registration before you accept the terms of services.");
                return 11;
            }

            UseDefaultOptionsIfNeed(ref options);

            
            var signer = new RS256Signer();
            signer.Init(); 

            var client = ClientHelper.CreateAcmeClient(signer, null);
            var registration = RegistrationHelper.CreateNew(client, options.ContactEmailAddress);

            RegistrationHelper.SaveToFile(registration, options.OutputPathRegisteration);
            SignerHelper.SaveToFile(signer, options.OutputPathSigner);

            return 0;
        }

        static void UseDefaultOptionsIfNeed(ref RegisterCommandOptions context)
        {
            if (string.IsNullOrWhiteSpace(context.ContactEmailAddress))
            {
                context.ContactEmailAddress = string.Concat(Program.ApplicationName, "user.", Guid.NewGuid().ToString("n").Substring(15, 8));
            }


            var appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrWhiteSpace(context.OutputPathRegisteration))
            {
                context.OutputPathRegisteration = Path.Combine(appPath, "reg.json");
            }
            else if (string.IsNullOrWhiteSpace(context.OutputPathSigner))
            {
                context.OutputPathSigner = Path.Combine(Path.GetDirectoryName(context.OutputPathRegisteration), "signer.key");
            }

            if (string.IsNullOrWhiteSpace(context.OutputPathSigner))
            {
                context.OutputPathSigner = Path.Combine(appPath, "signer.key"); ;
            }
            else if (string.IsNullOrWhiteSpace(context.OutputPathRegisteration))
            {
                context.OutputPathRegisteration = Path.Combine(Path.GetDirectoryName(context.OutputPathSigner), "reg.json");
            }
        }

        class RegisterCommandOptions
        {
            public string ContactEmailAddress { get; set; }
            public string OutputPathRegisteration { get; set; }
            public string OutputPathSigner { get; set; }
            public bool AcceptTos { get; set; }
        }
    }
    
}
