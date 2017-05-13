using System;

namespace LetsEncryptCentral
{
    static class ConsoleUtils
    {
        public static void ConsoleErrorOutput(string message) {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine(message);

            Console.ResetColor();
        }
        public static void ConsoleErrorOutput(string format, params object[] args) {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine(format, args);

            Console.ResetColor();
        }
    }
}
