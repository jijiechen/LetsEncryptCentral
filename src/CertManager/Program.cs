using System;
using Microsoft.Extensions.CommandLineUtils;
using System.Reflection;
using CertManager.Commands;

namespace CertManager
{
    class Program
    {
        public const string ApplicationName = "lec";
        public static CertManagerConfiguration GlobalConfiguration = new CertManagerConfiguration();

        static void Main(string[] args)
        {
            Console.Title = "lec";
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var app = new CommandLineApplication();
            app.Name = ApplicationName;
            app.FullName = "A central Let's Encrypt client that apply certificates use the DNS-01 challenge";

            var optionVerbose = app.Option("-v|--verbose", "Show verbose output", CommandOptionType.NoValue);
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
            Console.Error.WriteLine("!!! Unhandled exception has occured,  application is now exiting!");

            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                Console.Error.WriteLine(exception.Message);
                Console.Error.WriteLine(exception.StackTrace);
            }

            Environment.Exit(999);
       }
        
    }
}
