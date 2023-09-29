using System;
using System.IO;

namespace Launcher.controllers
{
    public class Logger
    {
        public static void LogInfo(string message)
        {
            Log($"[INFO] {message}");
        }

        public static void LogWarning(string message)
        {
            Log($"[WARNING] {message}");
        }

        public static void LogError(string message)
        {
            Log($"[ERROR] {message}");
        }

        public static void LogException(Exception ex)
        {
            Log($"[EXCEPTION] {ex.Message}\n{ex.StackTrace}");
        }

        private static void Log(string message)
        {
            string logFilePath = Properties.Settings.Default.logFilePath;

            // Create the log file if it doesn't exist
            if (!File.Exists(logFilePath))
            {
                File.Create(logFilePath).Close();
            }

            try
            {
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    writer.WriteLine($"{DateTime.Now} - {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
                Logger.LogException(ex);
            }
        }
    }
}
