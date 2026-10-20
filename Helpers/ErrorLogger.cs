using System;
using System.IO;
using System.Windows.Forms;

namespace InventorySystem
{
    /// <summary>
    /// Centralized error logging utility
    /// Logs all errors to daily log files for debugging and troubleshooting
    /// </summary>
    public static class ErrorLogger
    {
        private static string LogDirectory
        {
            get
            {
                string logDir = Path.Combine(DatabaseConfig.UserDataDirectory, "Logs");
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                return logDir;
            }
        }

        private static string LogFilePath
        {
            get
            {
                return Path.Combine(LogDirectory, $"error_log_{DateTime.Now:yyyyMMdd}.txt");
            }
        }

        /// <summary>
        /// Log an exception with context information
        /// </summary>
        /// <param name="ex">The exception to log</param>
        /// <param name="context">Context description (e.g., "adding student", "logging in")</param>
        public static void LogError(Exception ex, string context = "")
        {
            try
            {
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR in {context}\n" +
                                  $"Message: {ex.Message}\n" +
                                  $"Stack Trace: {ex.StackTrace}\n" +
                                  $"Source: {ex.Source}\n" +
                                  $"{new string('-', 80)}\n\n";
                
                File.AppendAllText(LogFilePath, logMessage);
            }
            catch
            {
                // If logging fails, silently fail - don't crash the application
            }
        }

        /// <summary>
        /// Log an informational message
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void LogInfo(string message)
        {
            try
            {
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}\n";
                File.AppendAllText(LogFilePath, logMessage);
            }
            catch
            {
                // If logging fails, silently fail
            }
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="message">The warning message to log</param>
        public static void LogWarning(string message)
        {
            try
            {
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] WARNING: {message}\n";
                File.AppendAllText(LogFilePath, logMessage);
            }
            catch
            {
                // If logging fails, silently fail
            }
        }
    }
}
