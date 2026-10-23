using System;
using System.Windows.Forms;
using InventorySystem.Helpers.Plugins;
using InventorySystem.Helpers;

namespace InventorySystem.Plugins
{
    public class MilkProductionPlugin : ITabPlugin
    {
        private PluginContext _context;

        public string Id => "com.softio.plugins.milkproduction";
        public string Name => "Milk Processing";
        public string Version => "1.0.0";
        public string Description => "Track what raw milk is turned into (cheese, labneh, laban...).";
        public string Author => "Softio Services";

        public bool RequiresLicense => false;
        public string LicenseFeatureKey => "Plugin_MilkProduction";

        public string TabId => "btnMilkProduction";
        public string TabTitle => LocalizationManager.GetString("Nav_MilkProduction");
        public string TabIcon => "inventory";
        public int TabOrder => 91;

        public void Initialize(PluginContext context)
        {
            _context = context;
        }

        public UserControl CreateTabContent()
        {
            bool allowed = _context.IsAdmin || _context.UserRole == "Admin" || _context.UserRole == "Staff" || _context.UserRole == "Accountant";
            if (allowed) return new InventorySystem.Forms.MilkProductionForm();

            return new UserControl { BackColor = System.Drawing.Color.Red };
        }

        public void Shutdown() { }
    }
}
