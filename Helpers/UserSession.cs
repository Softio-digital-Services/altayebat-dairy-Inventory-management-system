using System;

namespace InventorySystem.Helpers
{
    public static class UserSession
    {
        public static string Username { get; set; } = "Guest";
        public static string FullName { get; set; } = "Guest User";
        public static string Role { get; set; } = "Guest";

        public static bool IsAdmin => Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true || 
                                      Role?.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) == true || 
                                      Role?.Contains("admin", StringComparison.OrdinalIgnoreCase) == true;
        public static bool IsStaff => Role?.Equals("Staff", StringComparison.OrdinalIgnoreCase) ?? false;
        public static bool IsAccountant => Role?.Equals("Accountant", StringComparison.OrdinalIgnoreCase) ?? false;

        /// <summary>Built-in Softio Super Admin — only this account can toggle Softio-managed features.</summary>
        public static bool IsSoftioSuperAdmin =>
            !string.IsNullOrWhiteSpace(Username)
            && Username.Trim().Equals("Softio.Admin", StringComparison.OrdinalIgnoreCase);

        public static void Clear()
        {
            Username = "Guest";
            FullName = "Guest User";
            Role = "Guest";
        }
    }
}
