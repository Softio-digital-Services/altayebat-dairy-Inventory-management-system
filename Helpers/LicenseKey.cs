using System;

namespace InventorySystem.Helpers
{
    /// <summary>
    /// Represents a software license
    /// </summary>
    public class LicenseKey
    {
        public string Key { get; set; }
        public string LicenseType { get; set; } // TRIAL, YEARLY
        public string CustomerName { get; set; }
        public DateTime ActivationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string HardwareId { get; set; }
        public bool IsActive { get; set; }
        public string MachineName { get; set; }
        public string ProductId { get; set; }

        public LicenseKey()
        {
            IsActive = true;
            MachineName = Environment.MachineName;
        }

        /// <summary>
        /// Checks if the license is currently valid
        /// </summary>
        public bool IsValid()
        {
            if (!IsActive)
                return false;

            if (DateTime.Now > ExpirationDate)
                return false;

            // Verify hardware binding
            string currentHardwareId = HardwareInfo.GetMachineFingerprint();
            
            if (!string.IsNullOrEmpty(HardwareId) && HardwareId != currentHardwareId)
            {
                // Fallback to legacy fingerprint to prevent breaking existing valid licenses
                string legacyHardwareId = HardwareInfo.GetLegacyMachineFingerprint();
                if (HardwareId != legacyHardwareId)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the number of days remaining until expiration
        /// </summary>
        public int DaysRemaining()
        {
            if (DateTime.Now >= ExpirationDate)
                return 0;
            TimeSpan remaining = ExpirationDate - DateTime.Now;
            return Math.Max(0, (int)Math.Ceiling(remaining.TotalDays));
        }

        /// <summary>
        /// Checks if license is expiring soon (within 30 days)
        /// </summary>
        public bool IsExpiringSoon()
        {
            return DaysRemaining() <= 30 && DaysRemaining() > 0;
        }

        /// <summary>
        /// Checks if this is a trial license
        /// </summary>
        public bool IsTrial()
        {
            return LicenseType == "TRIAL";
        }
    }
}
