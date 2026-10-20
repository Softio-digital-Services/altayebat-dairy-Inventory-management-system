using System.Windows.Forms;
using InventorySystem.Forms;

namespace InventorySystem
{
    /// <summary>
    /// Standardized message display helper
    /// Provides consistent user messaging throughout the application
    /// </summary>
    public static class MessageHelper
    {
        public static void ShowSuccess(string message)
        {
            ModernMessageBox.Show(message, InventorySystem.Helpers.LocalizationManager.GetString("Msg_Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowError(string message)
        {
            ModernMessageBox.Show(message, InventorySystem.Helpers.LocalizationManager.GetString("Msg_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowWarning(string message)
        {
            ModernMessageBox.Show(message, InventorySystem.Helpers.LocalizationManager.GetString("Msg_Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public static void ShowInfo(string message)
        {
            ModernMessageBox.Show(message, InventorySystem.Helpers.LocalizationManager.GetString("Msg_Info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static bool ShowConfirmation(string message)
        {
            return ConfirmAction(message);
        }

        public static bool ConfirmAction(string message)
        {
            DialogResult result = ModernMessageBox.Show(message, InventorySystem.Helpers.LocalizationManager.GetString("Msg_Confirm"), 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }

        public static bool ShowConfirm(string message)
        {
            return ConfirmAction(message);
        }

        public static void ShowDatabaseError(string context)
        {
            string template = InventorySystem.Helpers.LocalizationManager.GetString("Msg_DbError");
            ShowError(string.Format(template, context));
        }
    }
}
