using System;
using System.Windows.Forms;
using InventorySystem.Helpers.Plugins;
using InventorySystem.Helpers;

namespace InventorySystem.Plugins
{
    public class CustomersPlugin : ITabPlugin
    {
        private PluginContext _context;

        public string Id => "com.softio.plugins.customers";
        public string Name => "Customers Management";
        public string Version => "1.0.0";
        public string Description => "Manage customer details and balances.";
        public string Author => "Softio Services";

        public bool RequiresLicense => false;
        public string LicenseFeatureKey => "Plugin_Customers";

        public string TabId => "btnCustomers";
        public string TabTitle => LocalizationManager.GetString("Nav_Customers", "Customers");
        public string TabIcon => "customers";
        public int TabOrder => 30; // After POS

        public void Initialize(PluginContext context)
        {
            _context = context;
        }

        public UserControl CreateTabContent()
        {
            // Only admins, accountants, or staff can access this based on original logic
            // Worker can see Customers, Admins can see Customers, Accountants can see Customers
            if (_context.UserRole == "Staff" || _context.UserRole == "Accountant" || _context.IsAdmin)
            {
                return new InventorySystem.Forms.CustomersForm();
            }

            // Return an empty access denied control or null.
            return new UserControl { BackColor = System.Drawing.Color.Red }; // Placeholder for denied
        }

        public void Shutdown()
        {
        }
    }
}


