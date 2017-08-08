using LetsEncryptCentral.CertManager;
using LetsEncryptCentral.Commands;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Net;
using System.Reflection;
using ACMESharp;
using static LetsEncryptCentral.ConsoleUtils;

namespace LetsEncryptCentral
{
    public class Program
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

            var app = new CommandLineApplication
            {
                Name = ApplicationName,
                FullName = "A central Let's Encrypt client that applies certificates using the DNS-01 challenge"
            };

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
            Console.WriteLine();
            ConsoleErrorOutput("!!! Unhandled exception has occured,  application is now exiting. !!!");
            var exception = e.ExceptionObject as Exception;
            if (exception == null)
            {
                Environment.Exit(999);
                return;
            }
            

            var exceptionPrinted = false;
            var acmeWebException = exception as AcmeClient.AcmeWebException;
            var webException = exception as WebException;
            if (acmeWebException != null)
            {
                if (!string.IsNullOrEmpty(acmeWebException.Response?.ContentAsString))
                {
                    exceptionPrinted = true;
                    ConsoleErrorOutput(acmeWebException.Response.ContentAsString);
                }
                else if (acmeWebException.WebException != null)
                {
                    exceptionPrinted = true;
                    ConsoleErrorOutput($"{acmeWebException.WebException.Status}: {acmeWebException.WebException.Message}");
                }
            }
            else if (webException != null)
            {
                exceptionPrinted = true;
                ConsoleErrorOutput($"{webException.Status}: {webException.Message}");
            }

            if (!exceptionPrinted)
            {
                ConsoleErrorOutput(exception.Message);
                ConsoleErrorOutput(exception.StackTrace);
            }
            Environment.Exit(1);
        }
    }
}
