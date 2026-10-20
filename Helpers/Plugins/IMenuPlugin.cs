using System;
using System.Collections.Generic;

namespace InventorySystem.Helpers.Plugins
{
    /// <summary>
    /// Implement this on top of IPlugin to inject items into the top menu-bar.
    /// </summary>
    public interface IMenuPlugin : IPlugin
    {
        /// <summary>
        /// Top-level menu group label, e.g. "Utilities", "Tools", "Reports".
        /// All plugins that share the same group name are merged under one menu.
        /// </summary>
        string MenuGroup { get; }

        /// <summary>Return the list of menu items to add under MenuGroup.</summary>
        IEnumerable<PluginMenuItem> GetMenuItems();
    }

    /// <summary>A single item inside a plugin menu group.</summary>
    public class PluginMenuItem
    {
        public string Label { get; set; }
        public string Icon { get; set; }        // Nuricon name (optional)
        public bool IsSeparator { get; set; }   // true -> draws a separator line
        public Action OnClick { get; set; }
    }
}
