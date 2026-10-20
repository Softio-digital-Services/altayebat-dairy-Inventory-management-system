using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace InventorySystem.Helpers
{
    /// <summary>
    /// Per-brand local HTTP ports so Generic, Otargi, and Panache can run at the same time.
    /// Values come from SystemBranding in appsettings.json.
    /// </summary>
    public static class AppHostConfig
    {
        public static string BrandId { get; }
        public static int HttpPort { get; }
        public static int HttpsPort { get; }
        public static string LoopbackUrl => $"http://127.0.0.1:{HttpPort}/";

        private static Mutex _instanceMutex;

        static AppHostConfig()
        {
            string brandId = "AlTayebat";
            int http = 5070;
            int https = 5071;
            try
            {
                string path = FindAppSettings();
                if (path != null)
                {
                    using var doc = JsonDocument.Parse(File.ReadAllText(path));
                    if (doc.RootElement.TryGetProperty("SystemBranding", out JsonElement branding))
                    {
                        if (TryGetString(branding, "BrandId", out string id))
                            brandId = id;
                        else if (TryGetString(branding, "CompanyName", out string company))
                            brandId = company;

                        if (branding.TryGetProperty("HttpPort", out JsonElement hp) && hp.TryGetInt32(out int hpv) && hpv > 0)
                            http = hpv;
                        if (branding.TryGetProperty("HttpsPort", out JsonElement hsp) && hsp.TryGetInt32(out int hspv) && hspv > 0)
                            https = hspv;
                    }
                }
            }
            catch { }

            BrandId = brandId;
            HttpPort = http;
            HttpsPort = https;
        }

        public static bool TryAcquireSingleInstance()
        {
            try
            {
                string name = @"Local\SoftioInventory_" + BrandId;
                var mutex = new Mutex(true, name, out bool created);
                if (!created)
                {
                    mutex.Dispose();
                    return false;
                }
                _instanceMutex = mutex;
                return true;
            }
            catch
            {
                return true;
            }
        }

        private static bool TryGetString(JsonElement parent, string name, out string value)
        {
            value = null;
            if (!parent.TryGetProperty(name, out JsonElement el) || el.ValueKind != JsonValueKind.String)
                return false;
            string v = el.GetString();
            if (string.IsNullOrWhiteSpace(v))
                return false;
            value = v.Trim();
            return true;
        }

        private static string FindAppSettings()
        {
            string[] candidates =
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"),
                Path.Combine(Application.StartupPath, "appsettings.json"),
                "appsettings.json"
            };
            foreach (string c in candidates)
            {
                try
                {
                    if (!string.IsNullOrEmpty(c) && File.Exists(c))
                        return c;
                }
                catch { }
            }
            return null;
        }
    }
}
