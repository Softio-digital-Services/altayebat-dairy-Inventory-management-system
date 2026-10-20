using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace InventorySystem.Helpers
{
    /// <summary>
    /// Database backup helpers and scheduled auto-backup (daily / weekly / monthly).
    /// </summary>
    public static class BackupService
    {
        public const string ScheduleKey = "backup.auto.schedule";
        public const string LastAutoRunKey = "backup.auto.lastRun";
        public const string FolderKey = "backup.folder";

        public static string GetDefaultBackupDirectory()
        {
            return Path.Combine(DatabaseConfig.UserDataDirectory, "Backups");
        }

        public static string GetBackupDirectory()
        {
            string custom = GetSetting(FolderKey, "");
            string root = TryResolveDirectory(custom) ?? GetDefaultBackupDirectory();
            Directory.CreateDirectory(root);
            return root;
        }

        public static bool HasCustomFolder()
        {
            return TryResolveDirectory(GetSetting(FolderKey, "")) != null;
        }

        public static string SetBackupDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                SetSetting(FolderKey, "");
                return GetBackupDirectory();
            }

            string full = Path.GetFullPath(path.Trim());
            Directory.CreateDirectory(full);
            SetSetting(FolderKey, full);
            return GetBackupDirectory();
        }

        public static string PickBackupDirectory()
        {
            string current = GetBackupDirectory();
            string selected = null;
            Exception error = null;
            var thread = new Thread(() =>
            {
                try
                {
                    using var owner = new Form
                    {
                        TopMost = true,
                        ShowInTaskbar = false,
                        Opacity = 0,
                        Width = 1,
                        Height = 1,
                        StartPosition = FormStartPosition.CenterScreen
                    };
                    owner.Show();
                    using var dlg = new FolderBrowserDialog
                    {
                        Description = LocalizationManager.GetString("Backup_ChooseFolder", "Choose backup folder"),
                        SelectedPath = Directory.Exists(current) ? current : GetDefaultBackupDirectory(),
                        ShowNewFolderButton = true
                    };
                    if (dlg.ShowDialog(owner) == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
                        selected = dlg.SelectedPath;
                    owner.Hide();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (error != null) throw error;
            if (string.IsNullOrWhiteSpace(selected)) return null;
            return SetBackupDirectory(selected);
        }

        public static string GetSchedule()
        {
            return NormalizeSchedule(GetSetting(ScheduleKey, "off"));
        }

        public static void SetSchedule(string schedule)
        {
            SetSetting(ScheduleKey, NormalizeSchedule(schedule));
        }

        public static DateTime? GetLastAutoRun()
        {
            string raw = GetSetting(LastAutoRunKey, "");
            if (string.IsNullOrWhiteSpace(raw)) return null;
            return DateTime.TryParse(raw, out DateTime dt) ? dt : null;
        }

        public static string CreateBackup(string prefix = "backup")
        {
            string dir = GetBackupDirectory();
            string dbFile = DatabaseConfig.DatabasePath;
            if (!File.Exists(dbFile))
                throw new FileNotFoundException("Database not found.", dbFile);

            string safePrefix = string.IsNullOrWhiteSpace(prefix) ? "backup" : prefix.Trim();
            foreach (char c in Path.GetInvalidFileNameChars())
                safePrefix = safePrefix.Replace(c, '_');

            string dest = Path.Combine(dir, $"{safePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.db");
            File.Copy(dbFile, dest, true);
            File.WriteAllText(Path.Combine(dir, "last_backup.txt"), DateTime.Now.ToString("o"));
            return dest;
        }

        public static bool IsAutoBackupDue(DateTime now, string schedule, DateTime? lastRun)
        {
            schedule = NormalizeSchedule(schedule);
            if (schedule == "off") return false;
            if (lastRun == null) return true;

            return schedule switch
            {
                "daily" => lastRun.Value.Date < now.Date,
                "weekly" => (now.Date - lastRun.Value.Date).TotalDays >= 7,
                "monthly" => lastRun.Value.Year != now.Year || lastRun.Value.Month != now.Month,
                _ => false
            };
        }

        public static bool RunAutoBackupIfDue()
        {
            string schedule = GetSchedule();
            if (schedule == "off") return false;

            DateTime now = DateTime.Now;
            DateTime? lastRun = GetLastAutoRun();
            if (!IsAutoBackupDue(now, schedule, lastRun)) return false;

            CreateBackup("backup_auto");
            SetSetting(LastAutoRunKey, now.ToString("o"));
            return true;
        }

        public static string NormalizeSchedule(string schedule)
        {
            if (string.IsNullOrWhiteSpace(schedule)) return "off";
            schedule = schedule.Trim().ToLowerInvariant();
            return schedule is "daily" or "weekly" or "monthly" ? schedule : "off";
        }

        private static string TryResolveDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            try
            {
                string full = Path.GetFullPath(path.Trim());
                if (File.Exists(full)) return null;
                Directory.CreateDirectory(full);
                return full;
            }
            catch
            {
                return null;
            }
        }

        private static string GetSetting(string key, string defaultValue)
        {
            try
            {
                FeatureFlags.EnsureTable();
                string v = DatabaseHelper.ExecuteScalar<string>(
                    "SELECT value FROM app_settings WHERE key = @k LIMIT 1",
                    new SqliteParameter("@k", key));
                return string.IsNullOrWhiteSpace(v) ? defaultValue : v.Trim();
            }
            catch
            {
                return defaultValue;
            }
        }

        private static void SetSetting(string key, string value)
        {
            FeatureFlags.EnsureTable();
            DatabaseHelper.ExecuteNonQuery(
                @"INSERT INTO app_settings (key, value) VALUES (@k, @v)
                  ON CONFLICT(key) DO UPDATE SET value = excluded.value",
                new SqliteParameter("@k", key),
                new SqliteParameter("@v", value ?? ""));
        }
    }
}
