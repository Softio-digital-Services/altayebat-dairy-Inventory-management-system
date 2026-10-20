using Microsoft.Win32;
using System;
using System.IO;
using System.Globalization;

namespace InventorySystem.Helpers
{
    /// <summary>
    /// Handles secure storage and retrieval of license information
    /// Uses both Registry and encrypted file for redundancy
    /// </summary>
    public static class LicenseStorage
    {
        private static readonly string RegistryPath = @"SOFTWARE\CarPartsInventory";
        private static readonly string LicenseFileName = "license.dat";
        private static readonly string LicenseFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CarPartsInventory",
            LicenseFileName
        );

        /// <summary>
        /// Saves license information to both Registry and encrypted file
        /// </summary>
        public static bool SaveLicense(LicenseKey license)
        {
            try
            {
                // Save to Registry
                SaveToRegistry(license);

                // Save to encrypted file
                SaveToFile(license);

                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "LicenseStorage.SaveLicense");
                return false;
            }
        }

        /// <summary>
        /// Loads license information from Registry or file
        /// </summary>
        public static LicenseKey LoadLicense()
        {
            try
            {
                // Try Registry first
                LicenseKey license = LoadFromRegistry();
                if (license != null)
                    return license;

                // Fallback to file
                return LoadFromFile();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "LicenseStorage.LoadLicense");
                return null;
            }
        }

        /// <summary>
        /// Deletes all stored license information
        /// </summary>
        public static void DeleteLicense()
        {
            try
            {
                // Delete from Registry
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue("LicenseKey", false);
                        key.DeleteValue("LicenseType", false);
                        key.DeleteValue("CustomerName", false);
                        key.DeleteValue("ActivationDate", false);
                        key.DeleteValue("ExpirationDate", false);
                        key.DeleteValue("HardwareId", false);
                    }
                }

                // Delete file
                if (File.Exists(LicenseFilePath))
                    File.Delete(LicenseFilePath);
            }
            catch { }
        }

        private static void SaveToRegistry(LicenseKey license)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                if (key != null)
                {
                    key.SetValue("LicenseKey", Encryption.Encrypt(license.Key));
                    key.SetValue("LicenseType", Encryption.Encrypt(license.LicenseType));
                    key.SetValue("CustomerName", Encryption.Encrypt(license.CustomerName ?? ""));
                    key.SetValue("ActivationDate", Encryption.Encrypt(license.ActivationDate.ToString("o")));
                    key.SetValue("ExpirationDate", Encryption.Encrypt(license.ExpirationDate.ToString("o")));
                    key.SetValue("HardwareId", Encryption.Encrypt(license.HardwareId));
                }
            }
        }

        private static LicenseKey LoadFromRegistry()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
            {
                if (key == null)
                    return null;

                try
                {
                    string licenseKey = Encryption.Decrypt(key.GetValue("LicenseKey")?.ToString());
                    if (string.IsNullOrEmpty(licenseKey))
                        return null;

                    return new LicenseKey
                    {
                        Key = licenseKey,
                        LicenseType = Encryption.Decrypt(key.GetValue("LicenseType")?.ToString()),
                        CustomerName = Encryption.Decrypt(key.GetValue("CustomerName")?.ToString()),
                        ActivationDate = DateTime.Parse(Encryption.Decrypt(key.GetValue("ActivationDate")?.ToString()), CultureInfo.InvariantCulture),
                        ExpirationDate = DateTime.Parse(Encryption.Decrypt(key.GetValue("ExpirationDate")?.ToString()), CultureInfo.InvariantCulture),
                        HardwareId = Encryption.Decrypt(key.GetValue("HardwareId")?.ToString())
                    };
                }
                catch
                {
                    return null;
                }
            }
        }

        private static void SaveToFile(LicenseKey license)
        {
            // Ensure directory exists
            string directory = Path.GetDirectoryName(LicenseFilePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Create license data string
            string data = $"{license.Key}|{license.LicenseType}|{license.ActivationDate:o}|{license.ExpirationDate:o}|{license.HardwareId}|{license.CustomerName}";

            // Encrypt and save
            string encrypted = Encryption.Encrypt(data);
            File.WriteAllText(LicenseFilePath, encrypted);
        }

        private static LicenseKey LoadFromFile()
        {
            if (!File.Exists(LicenseFilePath))
                return null;

            try
            {
                string encrypted = File.ReadAllText(LicenseFilePath);
                string decrypted = Encryption.Decrypt(encrypted);

                if (string.IsNullOrEmpty(decrypted))
                    return null;

                string[] parts = decrypted.Split('|');
                if (parts.Length < 5)
                    return null;

                return new LicenseKey
                {
                    Key = parts[0],
                    LicenseType = parts[1],
                    ActivationDate = DateTime.Parse(parts[2], CultureInfo.InvariantCulture),
                    ExpirationDate = DateTime.Parse(parts[3], CultureInfo.InvariantCulture),
                    HardwareId = parts[4],
                    CustomerName = parts.Length > 5 ? parts[5] : ""
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
