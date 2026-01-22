using System;
using System.IO;

namespace MagicKeyboardUtilities.App.Diagnostics
{
    public static class Log
    {
        private static string _logPath = "logs/app.log";

        static Log()
        {
            Directory.CreateDirectory("logs");
        }

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Error(string message, Exception ex = null)
        {
            Write("ERROR", $"{message} {ex}");
        }

        private static void Write(string level, string message)
        {
            string line = $"{DateTime.Now:O} [{level}] {message}";
            try
            {
                File.AppendAllText(_logPath, line + Environment.NewLine);
                Console.WriteLine(line);
            }
            catch
            {
                // Ignite
            }
        }
    }
}
