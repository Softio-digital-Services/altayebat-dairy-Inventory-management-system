using System.Windows.Forms;

namespace InventorySystem.Helpers.Plugins
{
    /// <summary>
    /// Base interface that every plugin must implement.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>Unique reverse-domain identifier, e.g. "com.carparts.calculator"</summary>
        string Id { get; }

        /// <summary>Human-readable display name shown in the Plugin Manager UI</summary>
        string Name { get; }

        string Version { get; }
        string Description { get; }
        string Author { get; }

        /// <summary>
        /// If true the plugin will only be loaded when LicenseManager.IsFeatureEnabled(LicenseFeatureKey) returns true.
        /// If false the plugin loads for every valid license (including TRIAL).
        /// </summary>
        bool RequiresLicense { get; }

        /// <summary>Key passed to LicenseManager.IsFeatureEnabled() when RequiresLicense is true.</summary>
        string LicenseFeatureKey { get; }

        /// <summary>
        /// Called once at startup after the license check passes.
        /// Store the context for later use in CreateTabContent / GetMenuItems.
        /// </summary>
        void Initialize(PluginContext context);

        /// <summary>Called when the app shuts down -- release resources.</summary>
        void Shutdown();
    }
}
