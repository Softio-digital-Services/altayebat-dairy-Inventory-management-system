using System.Windows.Forms;

namespace InventorySystem.Helpers.Plugins
{
    /// <summary>
    /// Implement this on top of IPlugin to add a tab to the main sidebar navigation.
    /// </summary>
    public interface ITabPlugin : IPlugin
    {
        /// <summary>Unique ID for the tab, used for programmatic navigation (e.g., from notifications)</summary>
        string TabId { get; }

        /// <summary>Sidebar button label (will be translated if IsArabic is set)</summary>
        string TabTitle { get; }

        /// <summary>Nuricon name for the sidebar icon, e.g. "calculator", "chart", "backup"</summary>
        string TabIcon { get; }

        /// <summary>
        /// Lower numbers appear first. Built-in tabs use 0-99; plugins should use 100+.
        /// </summary>
        int TabOrder { get; }

        /// <summary>
        /// Factory -- called the FIRST time the user clicks this tab.
        /// Return the UserControl that will fill panel3.
        /// </summary>
        UserControl CreateTabContent();
    }
}
