using System;
using System.Diagnostics;
using System.IO;

namespace InventorySystem.Helpers
{
    /// <summary>
    /// Records how long each startup stage takes (seconds since the process started)
    /// to %LocalAppData%\AlTayebatInventory\Logs\startup.log, so slow launches can be diagnosed.
    /// </summary>
    public static class StartupTrace
    {
        private static readonly object _lock = new object();
        private static readonly string _path = Path.Combine(DatabaseConfig.DataRoot, "Logs", "startup.log");
        private static bool _started;

        public static void Mark(string stage)
        {
            try
            {
                double secs = (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds;
                lock (_lock)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_path));
                    if (!_started)
                    {
                        _started = true;
                        // keep the log small: only the latest launches
                        if (File.Exists(_path) && new FileInfo(_path).Length > 64 * 1024) File.Delete(_path);
                        File.AppendAllText(_path, Environment.NewLine + "=== launch " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine);
                    }
                    File.AppendAllText(_path, $"{secs,7:0.00}s  {stage}{Environment.NewLine}");
                }
            }
            catch { /* diagnostics must never break startup */ }
        }
    }
}
