using System;
using System.IO;
using System.Windows.Forms;

namespace InventorySystem
{
    /// <summary>
    /// Centralized database and file path configuration (SQLite).
    /// User data lives under LocalAppData so installs under Program Files still work
    /// for non-admin users.
    /// </summary>
    public static class DatabaseConfig
    {
        // ALTAYEBAT_DATA_DIR (optional) points the app at another data folder, e.g. to test a fresh install
        // without touching the real database. Normal installs leave it unset.
        private static readonly string UserDataRoot =
            !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ALTAYEBAT_DATA_DIR"))
                ? Environment.GetEnvironmentVariable("ALTAYEBAT_DATA_DIR")
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AlTayebatInventory");

        /// <summary>Root folder for all user data (database, logs, backups, browser profile).</summary>
        public static string DataRoot => UserDataRoot;

        public static string ConnectionString
        {
            get
            {
                string dbPath = DatabasePath;
                string dir = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                return $"Data Source={dbPath};";
            }
        }

        /// <summary>
        /// SQLite file: %LocalAppData%\AlTayebatInventory\Data\inventory.db
        /// Migrates once from the old Program Files\...\Data\inventory.db if present.
        /// </summary>
        public static string DatabasePath
        {
            get
            {
                string dir = Path.Combine(UserDataRoot, "Data");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string path = Path.Combine(dir, "inventory.db");
                TryMigrateLegacyDatabase(path);
                return path;
            }
        }

        public static string PartsImagesDirectory
        {
            get
            {
                string imagesPath = Path.Combine(UserDataRoot, "Parts_Images");
                if (!Directory.Exists(imagesPath))
                    Directory.CreateDirectory(imagesPath);
                return imagesPath;
            }
        }

        /// <summary>Writable root for logs, backups, and other user files.</summary>
        public static string UserDataDirectory
        {
            get
            {
                if (!Directory.Exists(UserDataRoot))
                    Directory.CreateDirectory(UserDataRoot);
                return UserDataRoot;
            }
        }

        private static void TryMigrateLegacyDatabase(string newPath)
        {
            try
            {
                if (File.Exists(newPath)) return;

                string legacy = Path.Combine(Application.StartupPath, "Data", "inventory.db");
                if (!File.Exists(legacy)) return;

                File.Copy(legacy, newPath, overwrite: false);
                foreach (string suffix in new[] { "-wal", "-shm" })
                {
                    string side = legacy + suffix;
                    if (File.Exists(side))
                        File.Copy(side, newPath + suffix, overwrite: false);
                }
            }
            catch
            {
                // First-run create is fine if copy fails
            }
        }
    }
}
