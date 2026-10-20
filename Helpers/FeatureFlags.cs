using System;
using Microsoft.Data.Sqlite;

namespace InventorySystem.Helpers
{
    /// <summary>
    /// Runtime feature flags persisted in SQLite. Softio Super Admin controls toggles.
    /// </summary>
    public static class FeatureFlags
    {
        public const string ScaleEnabledKey = "feature.scale.enabled";
        public const string QuickSaleEnabledKey = "feature.quicksale.enabled";

        public static bool ScaleEnabled
        {
            get => GetBool(ScaleEnabledKey, false);
            set => SetBool(ScaleEnabledKey, value);
        }

        public static bool QuickSaleEnabled
        {
            get => GetBool(QuickSaleEnabledKey, false);
            set => SetBool(QuickSaleEnabledKey, value);
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            try
            {
                EnsureTable();
                string v = DatabaseHelper.ExecuteScalar<string>(
                    "SELECT value FROM app_settings WHERE key = @k LIMIT 1",
                    new SqliteParameter("@k", key));
                if (string.IsNullOrWhiteSpace(v)) return defaultValue;
                v = v.Trim();
                return v == "1" || v.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static void SetBool(string key, bool value)
        {
            EnsureTable();
            DatabaseHelper.ExecuteNonQuery(
                @"INSERT INTO app_settings (key, value) VALUES (@k, @v)
                  ON CONFLICT(key) DO UPDATE SET value = excluded.value",
                new SqliteParameter("@k", key),
                new SqliteParameter("@v", value ? "1" : "0"));
        }

        public static void EnsureTable()
        {
            try
            {
                DatabaseHelper.ExecuteNonQuery(@"
                    CREATE TABLE IF NOT EXISTS app_settings (
                        key TEXT PRIMARY KEY NOT NULL,
                        value TEXT NOT NULL
                    );");
            }
            catch { /* already exists or DB not ready */ }
        }
    }
}
