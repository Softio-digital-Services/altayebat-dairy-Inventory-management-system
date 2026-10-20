using System;
using System.Linq;
using System.Text;

namespace InventorySystem.Helpers
{
    /// <summary>
    /// Manages license validation, activation, and generation
    /// </summary>
    public static class LicenseManager
    {
        private const string ProductCode = "ATIMS"; // Al-Tayebat Inventory Management System
        private const int TrialDays = 30;

        /// <summary>
        /// Validates a license key format and checksum
        /// </summary>
        public static bool ValidateLicenseKey(string licenseKey, string customerName, string expectedProductId = ProductCode)
        {
            if (string.IsNullOrWhiteSpace(licenseKey) || string.IsNullOrWhiteSpace(customerName))
                return false;

            // Remove dashes and spaces
            licenseKey = licenseKey.Replace("-", "").Replace(" ", "").ToUpper();

            // Check length (25 characters: 5 segments of 5)
            if (licenseKey.Length != 25)
                return false;

            // Extract components
            string productCode = licenseKey.Substring(0, 5);
            string typeCode = licenseKey.Substring(5, 5);
            string hwHash = licenseKey.Substring(10, 5);
            string dateCode = licenseKey.Substring(15, 5);
            string checksum = licenseKey.Substring(20, 5);

            // Verify product code
            if (expectedProductId != "*" && productCode != expectedProductId)
                return false;

            // Verify checksum. Try universal first, then legacy bound to customer name
            string dataToHashUniversal = productCode + typeCode + hwHash + dateCode;
            string computedChecksumUniversal = ComputeChecksum(dataToHashUniversal);

            string dataToHashBound = productCode + typeCode + hwHash + dateCode + customerName.Trim().ToUpper();
            string computedChecksumBound = ComputeChecksum(dataToHashBound);

            return checksum == computedChecksumUniversal || checksum == computedChecksumBound;
        }

        /// <summary>
        /// Activates a license key for the current machine
        /// </summary>
        public static LicenseKey ActivateLicense(string licenseKey, string customerName, string expectedProductId = ProductCode)
        {
            if (!ValidateLicenseKey(licenseKey, customerName, expectedProductId))
                return null;

            licenseKey = licenseKey.Replace("-", "").Replace(" ", "").ToUpper();

            try
            {
                // Extract components
                string productCode = licenseKey.Substring(0, 5);
                string typeCode = licenseKey.Substring(5, 5);
                string dateCode = licenseKey.Substring(15, 5);

                // Decode license type
                string licenseType = DecodeLicenseType(typeCode);

                // Decode expiration date
                DateTime expirationDate = DecodeDate(dateCode);

                // Create license object
                LicenseKey license = new LicenseKey
                {
                    Key = FormatLicenseKey(licenseKey),
                    LicenseType = licenseType,
                    CustomerName = customerName.Trim(),
                    ActivationDate = DateTime.Now,
                    ExpirationDate = expirationDate,
                    HardwareId = HardwareInfo.GetMachineFingerprint(),
                    IsActive = true,
                    ProductId = productCode
                };

                // Verify hardware binding (Strict validation)
                string hwHash = licenseKey.Substring(10, 5);
                string currentHwHash = HardwareInfo.GetShortHardwareId().Substring(0, 5);
                string legacyHwHash = HardwareInfo.GetLegacyMachineFingerprint().Substring(0, 5);

                if (hwHash != currentHwHash && hwHash != legacyHwHash && hwHash != "00000")
                    return null;

                // Save license
                if (LicenseStorage.SaveLicense(license))
                    return license;

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the current license status
        /// </summary>
        public static LicenseKey GetCurrentLicense()
        {
            LicenseKey license = LicenseStorage.LoadLicense();
            if (license == null)
            {
                license = StartTrial();
            }
            return license;
        }

        /// <summary>
        /// Checks if a valid license exists
        /// </summary>
        public static bool HasValidLicense()
        {
            LicenseKey license = GetCurrentLicense();
            if (license == null || !license.IsValid())
                return false;

            // Strict checksum validation for non-trial licenses
            if (!license.IsTrial())
            {
                return ValidateLicenseKey(license.Key, license.CustomerName, ProductCode);
            }

            return true;
        }

        /// <summary>
        /// Checks if a specific feature/plugin is enabled by the current license.
        /// 
        /// Currently the license types work as follows:
        ///   TRIAL  -> all RequiresLicense plugins are HIDDEN
        ///   YEARLY -> all RequiresLicense plugins are VISIBLE
        /// 
        /// In future you can embed a comma-separated feature list in the license key
        /// and parse featureKey out of it for per-feature gating.
        /// </summary>
        public static bool IsFeatureEnabled(string featureKey)
        {
            if (string.IsNullOrWhiteSpace(featureKey)) return true; // no key needed -> always on

            LicenseKey license = GetCurrentLicense();
            if (license == null || !license.IsValid()) return false;
            if (license.IsTrial()) return false; // trial -> no premium plugins

            // Full paid license -> all currently-defined features enabled
            // (Extend this to parse license.FeatureFlags in future)
            return ValidateLicenseKey(license.Key, license.CustomerName, ProductCode);
        }

        /// <summary>
        /// Starts a trial period
        /// </summary>
        public static LicenseKey StartTrial()
        {
            LicenseKey trial = new LicenseKey
            {
                Key = "TRIAL-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                LicenseType = "TRIAL",
                ActivationDate = DateTime.Now,
                ExpirationDate = DateTime.Now.AddDays(TrialDays),
                HardwareId = HardwareInfo.GetMachineFingerprint(),
                IsActive = true,
                ProductId = ProductCode
            };

            if (LicenseStorage.SaveLicense(trial))
                return trial;

            return null;
        }

        /// <summary>
        /// Deactivates the current license
        /// </summary>
        public static void DeactivateLicense()
        {
            LicenseStorage.DeleteLicense();
        }

        /// <summary>
        /// Generates a new license key (for admin use)
        /// </summary>
        public static string GenerateLicenseKey(string licenseType, DateTime expirationDate, string customerName, string hardwareId = null, string productId = ProductCode)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                throw new ArgumentException("Customer name is required for license generation.");
            if (string.IsNullOrWhiteSpace(productId) || productId.Length != 5)
                throw new ArgumentException("Product code must be exactly 5 characters.");

            productId = productId.ToUpper();

            string typeCode = EncodeLicenseType(licenseType);
            string hwHash = string.IsNullOrEmpty(hardwareId) ? "00000" : (hardwareId.Length >= 5 ? hardwareId.Substring(0, 5).ToUpper() : hardwareId.ToUpper().PadRight(5, 'X'));
            string dateCode = EncodeDate(expirationDate);

            // Compute checksum without customer name to allow key-only activation (Universal)
            string dataToHash = productId + typeCode + hwHash + dateCode;
            string checksum = ComputeChecksum(dataToHash);

            string fullKey = productId + typeCode + hwHash + dateCode + checksum;
            return FormatLicenseKey(fullKey);
        }

        #region Helper Methods

        private static string EncodeLicenseType(string type)
        {
            switch (type.ToUpper())
            {
                case "TRIAL":
                    return "TRIAL";
                case "YEARLY":
                case "ANNUAL":
                    return "YEAR1";
                default:
                    return "YEAR1";
            }
        }

        private static string DecodeLicenseType(string code)
        {
            if (code == "TRIAL")
                return "TRIAL";
            if (code.StartsWith("YEAR"))
                return "YEARLY";
            return "YEARLY";
        }

        private static string EncodeDate(DateTime date)
        {
            // Encode as: YY + DDD (year + day of year)
            // Example: 2025-12-31 -> "25365"
            int year = date.Year % 100; // Last 2 digits
            int dayOfYear = date.DayOfYear;
            return $"{year:D2}{dayOfYear:D3}";
        }

        private static DateTime DecodeDate(string code)
        {
            try
            {
                int year = int.Parse(code.Substring(0, 2)) + 2000;
                int dayOfYear = int.Parse(code.Substring(2, 3));
                return new DateTime(year, 1, 1).AddDays(dayOfYear - 1);
            }
            catch
            {
                return DateTime.Now.AddYears(1); // Default to 1 year from now
            }
        }

        private static string ComputeChecksum(string data)
        {
            // Simple checksum: sum of ASCII values mod 99999
            int sum = data.Sum(c => (int)c);
            return (sum % 99999).ToString("D5");
        }

        private static string FormatLicenseKey(string key)
        {
            // Format as: XXXXX-XXXXX-XXXXX-XXXXX-XXXXX
            if (key.Length != 25)
                return key;

            return $"{key.Substring(0, 5)}-{key.Substring(5, 5)}-{key.Substring(10, 5)}-{key.Substring(15, 5)}-{key.Substring(20, 5)}";
        }

        #endregion
    }
}
