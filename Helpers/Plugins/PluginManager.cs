using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace InventorySystem.Helpers.Plugins
{
    /// <summary>
    /// Scans the /Plugins folder, loads qualifying DLLs, and registers them
    /// into the MainForm navigation and menu bar.
    /// </summary>
    public static class PluginManager
    {
        private static readonly List<IPlugin> _loaded = new List<IPlugin>();
        private static PluginContext _context;

        /// <summary>All successfully loaded and initialized plugins.</summary>
        public static IReadOnlyList<IPlugin> LoadedPlugins => _loaded.AsReadOnly();

        /// <summary>
        /// Call this once from MainForm constructor AFTER InitializeNavigation().
        /// </summary>
        public static void DiscoverAndLoad(PluginContext context)
        {
            _context = context;
            _loaded.Clear(); // Prevent duplication on multiple reloads

            string pluginsDir = Path.Combine(Application.StartupPath, "Plugins");
            if (!Directory.Exists(pluginsDir))
            {
                try { Directory.CreateDirectory(pluginsDir); } catch { }
                return; // No plugins yet -- that's perfectly fine
            }

            // Also load internal (in-tree) plugins shipped with the core app
            LoadInternalPlugins();

            // Then scan the /Plugins folder for external DLLs
            foreach (string dll in Directory.GetFiles(pluginsDir, "*.dll", SearchOption.TopDirectoryOnly))
            {
                LoadAssembly(dll);
            }

            // Sort tab plugins by TabOrder and register them
            var tabPlugins = _loaded.OfType<ITabPlugin>().OrderBy(p => p.TabOrder);
            foreach (var tp in tabPlugins)
            {
                try
                {
                    context.AddTab(tp.TabTitle, tp.TabIcon, tp.TabOrder, tp.CreateTabContent, tp.TabId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PluginManager] Tab registration failed for {tp.Id}: {ex.Message}");
                }
            }

            // Register menu plugins
            var menuPlugins = _loaded.OfType<IMenuPlugin>();
            foreach (var mp in menuPlugins)
            {
                try
                {
                    foreach (var item in mp.GetMenuItems())
                        context.AddMenuItem(mp.MenuGroup, item);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PluginManager] Menu registration failed for {mp.Id}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Gracefully shut down all plugins (call on MainForm.FormClosed).
        /// </summary>
        public static void ShutdownAll()
        {
            foreach (var p in _loaded)
            {
                try { p.Shutdown(); } catch { }
            }
            _loaded.Clear();
        }

        // -
        // Internal helpers
        // -

        /// <summary>Register plugins that ship inside the core assembly.</summary>
        private static void LoadInternalPlugins()
        {
            // Free built-in plugins -- always visible
            TryRegister(new InventorySystem.Plugins.CalculatorPlugin());
            TryRegister(new InventorySystem.Plugins.BackupPlugin());

            // Paid internal plugins (visibility toggled via license)
            TryRegister(new InventorySystem.Plugins.CustomersPlugin());
            TryRegister(new InventorySystem.Plugins.SuppliersPlugin());
            TryRegister(new InventorySystem.Plugins.QuotationsPlugin());
            TryRegister(new InventorySystem.Plugins.MonthlyExpensesPlugin());
            TryRegister(new InventorySystem.Plugins.BarcodeLabelsPlugin());
        }

        private static void LoadAssembly(string path)
        {
            try
            {
                Assembly asm = Assembly.LoadFrom(path);
                foreach (Type t in asm.GetExportedTypes())
                {
                    if (!t.IsClass || t.IsAbstract) continue;
                    if (!typeof(IPlugin).IsAssignableFrom(t)) continue;

                    IPlugin instance = (IPlugin)Activator.CreateInstance(t);
                    TryRegister(instance);
                }
            }
            catch (Exception ex)
            {
                // Bad DLL -- log and skip, never crash the app
                Console.WriteLine($"[PluginManager] Failed to load {Path.GetFileName(path)}: {ex.Message}");
            }
        }

        private static void TryRegister(IPlugin plugin)
        {
            try
            {
                // Prevent duplicate registrations from old leftover DLLs
                if (_loaded.Any(p => p.Id == plugin.Id))
                {
                    Console.WriteLine("[PluginManager] Plugin '" + plugin.Name + "' is already loaded. Skipping duplicate.");
                    return;
                }
                // License gate -- skip (hide) unlicensed plugins entirely
                if (false && plugin.RequiresLicense && !_context.CheckLicense(plugin.LicenseFeatureKey))
                {
                    Console.WriteLine($"[PluginManager] Plugin '{plugin.Name}' hidden -- license key '{plugin.LicenseFeatureKey}' not enabled.");
                    return;
                }

                plugin.Initialize(_context);
                _loaded.Add(plugin);
                Console.WriteLine($"[PluginManager] Loaded: {plugin.Name} v{plugin.Version}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PluginManager] Init failed for {plugin.Id}: {ex.Message}");
            }
        }
    }
}



