using LetsEncryptCentral.CertManager;
using LetsEncryptCentral.Commands;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Reflection;
using static LetsEncryptCentral.ConsoleUtils;

namespace LetsEncryptCentral
{
    class Program
    {
        public const string ApplicationName = "lec";
        public static CertManagerConfiguration GlobalConfiguration = new CertManagerConfiguration();

        static void Main(string[] args)
        {
            Console.Title = "lec";
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

#if DEBUG
            System.Diagnostics.Debugger.Launch();
#endif

            var app = new CommandLineApplication();
            app.Name = ApplicationName;
            app.FullName = "A central Let's Encrypt client that applies certificates using the DNS-01 challenge";

            app.HelpOption("-?|-h|--help");
            app.VersionOption("--version", Assembly.GetExecutingAssembly().GetName().Version.ToString());

            // Show help information if no subcommand/option was specified
            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 9;
            });

            app.Command("reg", new RegisterAccountCommand().Setup);
            app.Command("apply", new RequestCertificateCommand().Setup);

            app.Execute(args);
        }

        
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ConsoleErrorOutput("!!! Unhandled exception has occured,  application is now exiting. !!!");
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                ConsoleErrorOutput(exception.Message);
                ConsoleErrorOutput(exception.StackTrace);
            }

            Environment.Exit(999);
       }
        
    }
}
