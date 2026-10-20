using System;
using System.Windows.Forms;

namespace InventorySystem.Helpers.Plugins
{
    /// <summary>
    /// Safe API surface that the core app exposes to every plugin.
    /// Plugins MUST NOT reference MainForm directly -- use this context.
    /// </summary>
    public class PluginContext
    {
        // - Database -
        /// <summary>SQLite / SQL connection string. Plugins may open their own connections.</summary>
        public string ConnectionString { get; set; }

        // - Session -
        public string CurrentUser { get; set; }
        public string UserRole    { get; set; }
        public bool   IsAdmin     { get; set; }

        // - Localization -
        public bool IsArabic => InventorySystem.Helpers.LocalizationManager.IsArabic;

        // - License -
        /// <summary>
        /// Returns true if the given feature key is enabled in the current license.
        /// Plugins call this inside CreateTabContent() for fine-grained gating.
        /// </summary>
        public Func<string, bool> CheckLicense { get; set; }

        // - Navigation callbacks -
        /// <summary>
        /// Register a new sidebar tab. Called by PluginManager -- plugins don't call this directly.
        /// Signature: (tabTitle, iconNuricon, tabOrder, contentFactory, tabId) -> the created Button.
        /// </summary>
        public Action<string, string, int, Func<UserControl>, string> AddTab { get; set; }

        /// <summary>
        /// Add a menu item to a named top-level menu group.
        /// Signature: (groupLabel, menuItem)
        /// </summary>
        public Action<string, PluginMenuItem> AddMenuItem { get; set; }

        // - Notifications -
        public Action<string> ShowSuccess { get; set; }
        public Action<string> ShowError   { get; set; }
        public Action<string> ShowInfo    { get; set; }

        // - App path -
        /// <summary>Directory where the .exe lives (and where the /Plugins folder is).</summary>
        public string AppDirectory => System.Windows.Forms.Application.StartupPath;

        /// <summary>Plugins folder path: [AppDirectory]\Plugins</summary>
        public string PluginsDirectory => System.IO.Path.Combine(AppDirectory, "Plugins");
    }
}
