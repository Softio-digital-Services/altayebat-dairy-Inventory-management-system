using System;
using System.Security.Cryptography;
using System.Text;
using System.Net.NetworkInformation;

namespace InventorySystem.Helpers
{
    /// <summary>
    /// Retrieves unique hardware information for license binding
    /// Simplified version using built-in .NET methods
    /// </summary>
    public static class HardwareInfo
    {
        /// <summary>
        /// Gets a unique hardware fingerprint for this machine
        /// Combines Machine Name, User Name, and MAC Address
        /// </summary>
        public static string GetMachineFingerprint()
        {
            try
            {
                string machineName = Environment.MachineName;
                string machineGuid = GetWindowsMachineGuid();
                
                string combined = machineName + machineGuid;
                return ComputeHash(combined);
            }
            catch
            {
                // Fallback to machine name if anything fails
                return ComputeHash(Environment.MachineName);
            }
        }

        /// <summary>
        /// Gets the legacy hardware fingerprint to support older activated licenses
        /// </summary>
        public static string GetLegacyMachineFingerprint()
        {
            try
            {
                string machineName = Environment.MachineName;
                string userName = Environment.UserName;
                string macAddress = GetLegacyMacAddress();
                string osVersion = Environment.OSVersion.ToString();

                string combined = machineName + userName + macAddress + osVersion;
                return ComputeHash(combined);
            }
            catch
            {
                return ComputeHash(Environment.MachineName + Environment.UserName);
            }
        }

        /// <summary>
        /// Gets a short hardware ID for display purposes (first 12 chars)
        /// </summary>
        public static string GetShortHardwareId()
        {
            string fullId = GetMachineFingerprint();
            return fullId.Substring(0, Math.Min(12, fullId.Length)).ToUpper();
        }

        private static string GetWindowsMachineGuid()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("MachineGuid");
                        if (val != null)
                            return val.ToString();
                    }
                }
            }
            catch { }
            return "DEFAULT_GUID";
        }

        private static string GetLegacyMacAddress()
        {
            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up && 
                        nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        string mac = nic.GetPhysicalAddress().ToString();
                        if (!string.IsNullOrEmpty(mac))
                            return mac;
                    }
                }
            }
            catch { }
            return "";
        }

        private static string ComputeHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
