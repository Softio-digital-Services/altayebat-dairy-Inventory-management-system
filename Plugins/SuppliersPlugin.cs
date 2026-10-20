using System;
using System.Windows.Forms;
using InventorySystem.Helpers.Plugins;
using InventorySystem.Helpers;

namespace InventorySystem.Plugins
{
    public class SuppliersPlugin : ITabPlugin
    {
        private PluginContext _context;

        public string Id => "com.softio.plugins.suppliers";
        public string Name => "Suppliers Management";
        public string Version => "1.0.0";
        public string Description => "Manage Suppliers.";
        public string Author => "Softio Services";

        public bool RequiresLicense => false;
        public string LicenseFeatureKey => "Plugin_Suppliers";

        public string TabId => "btnSuppliers";
        public string TabTitle => LocalizationManager.GetString("Nav_Suppliers");
        public string TabIcon => "suppliers";
        public int TabOrder => 40;

        public void Initialize(PluginContext context)
        {
            _context = context;
        }

        public UserControl CreateTabContent()
        {
            var allowed = false;
            foreach (var role in new string[] { "Admin", "Staff", "Accountant" })
            {
                if (_context.UserRole == role || (_context.IsAdmin && role == "Admin")) allowed = true;
            }

            if (allowed)
            {
                var form = new InventorySystem.Forms.SuppliersForm();
                var loadMethod = form.GetType().GetMethod("LoadData") ?? form.GetType().GetMethod("LoadQuotations");
                if (loadMethod != null) { if (loadMethod.GetParameters().Length == 1) loadMethod.Invoke(form, new object[] { "" }); else loadMethod.Invoke(form, null); }
                return form;
            }

            return new UserControl { BackColor = System.Drawing.Color.Red };
        }

        public void Shutdown() { }
    }
}


