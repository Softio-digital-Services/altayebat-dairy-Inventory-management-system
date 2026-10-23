using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using InventorySystem.Services;
using InventorySystem.Helpers;
using InventorySystem.Forms;
using QRCoder;

namespace InventorySystem
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0 &&
                args.Any(a => string.Equals(a, "--test-reports", StringComparison.OrdinalIgnoreCase)))
            {
                Environment.Exit(RunReportsSmokeTest());
                return;
            }
            if (args != null && args.Length > 0 &&
                args.Any(a => string.Equals(a, "--test-export", StringComparison.OrdinalIgnoreCase)))
            {
                Environment.Exit(RunExportSmokeTest());
                return;
            }

            if (args != null && args.Length > 0 &&
                args.Any(a => string.Equals(a, "--test-i18n", StringComparison.OrdinalIgnoreCase)))
            {
                Environment.Exit(RunI18nSmokeTest());
                return;
            }

            InventorySystem.Helpers.StartupTrace.Mark("main start");
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Load initial language from persisted settings, defaulting to en-US
            string savedLanguage = "en-US";
            try
            {
                string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "language.txt");
                if (System.IO.File.Exists(configPath))
                {
                    savedLanguage = System.IO.File.ReadAllText(configPath).Trim();
                }
            }
            catch { }
            InventorySystem.Helpers.LocalizationManager.SetLanguage(savedLanguage);

            if (!AppHostConfig.TryAcquireSingleInstance())
            {
                MessageBox.Show(
                    ThemeConfig.AppTitle + " is already running.",
                    ThemeConfig.AppTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Expose background task for server hosting without blocking UI thread
            _ = Task.Run(() => StartApiServer());

            Application.ThreadException += (s, e) =>
            {
                try { System.IO.File.AppendAllText("crash.txt", DateTime.Now.ToString() + ": " + e.Exception.ToString() + "\n\n"); } catch { }
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                try { System.IO.File.AppendAllText("crash.txt", DateTime.Now.ToString() + ": " + e.ExceptionObject.ToString() + "\n\n"); } catch { }
            };

            try
            {
                // Initialize Database (Create if missing)
                InventorySystem.Helpers.DatabaseInitializer.Initialize();
                InventorySystem.Helpers.StartupTrace.Mark("database initialized");

                // Ensure schema is up to date (add missing columns)
                DatabaseHelper.EnsureSchema();
                InventorySystem.Helpers.StartupTrace.Mark("schema ensured");
                // Demo seed is opt-in via POST /api/dev/seed-demo — do not auto-fill new installs

                // Initialize currency tables and load cached rates
                InventorySystem.Services.CurrencyService.EnsureTable();

                // Check License
                if (!InventorySystem.Helpers.LicenseManager.HasValidLicense())
                {
                    // Show activation form
                    InventorySystem.Forms.LicenseActivationForm activationForm = new InventorySystem.Forms.LicenseActivationForm();
                    if (activationForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        // User cancelled activation - exit application
                        return;
                    }
                }

                // Check for expiring license and show warning
                var license = InventorySystem.Helpers.LicenseManager.GetCurrentLicense();
                if (license != null && license.IsExpiringSoon() && !license.IsTrial())
                {
                    int daysLeft = license.DaysRemaining();
                    InventorySystem.Forms.ModernMessageBox.Show(
                        string.Format(LocalizationManager.GetString("Msg_LicExpiringSoonBody"), daysLeft),
                        LocalizationManager.GetString("Msg_LicExpiringSoon"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }

                // Desktop exe: embedded WebView2 window (no external Chrome)
                InventorySystem.Helpers.StartupTrace.Mark("license checked, opening window");
                Application.Run(new WebServerHostForm());
            }
            catch (Exception ex)
            {
                InventorySystem.Forms.ModernMessageBox.Show(
                    string.Format(LocalizationManager.GetString("Msg_CriticalError"), ex.Message) + $"\n\n{LocalizationManager.GetString("Msg_StackTrace")}\n{ex.StackTrace}",
                    LocalizationManager.GetString("Error_AppCrash"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static int RunI18nSmokeTest()
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "i18n-smoke.log");
            int failures = 0;

            void AgentLog(string checkId, string location, string message, Dictionary<string, object> data)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                    var payload = new Dictionary<string, object>
                    {
                        ["checkId"] = checkId,
                        ["location"] = location,
                        ["message"] = message,
                        ["data"] = data,
                        ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };
                    File.AppendAllText(logPath, System.Text.Json.JsonSerializer.Serialize(payload) + Environment.NewLine);
                }
                catch { }
            }

            try
            {
                LocalizationManager.SetLanguage("en-US");
                string enTitle = LocalizationManager.GetString("Rep_Title");
                bool enOk = !string.IsNullOrEmpty(enTitle) && enTitle.Contains("Sales", StringComparison.OrdinalIgnoreCase);
                AgentLog("H1", "Program.RunI18nSmokeTest", "EN Rep_Title", new Dictionary<string, object> { ["value"] = enTitle ?? "", ["ok"] = enOk });
                if (!enOk) failures++;

                LocalizationManager.SetLanguage("ar");
                string arTitle = LocalizationManager.GetString("Rep_Title");
                bool arOk = LocalizationManager.IsArabic && !string.IsNullOrEmpty(arTitle) && arTitle.Contains("تقارير");
                AgentLog("H1", "Program.RunI18nSmokeTest", "AR Rep_Title", new Dictionary<string, object> { ["value"] = arTitle ?? "", ["isArabic"] = LocalizationManager.IsArabic, ["ok"] = arOk });
                if (!arOk) failures++;

                LocalizationManager.SetLanguage("en-US");
                string afterEn = LocalizationManager.GetString("Rep_Title");
                bool switchBackOk = afterEn.Contains("Sales", StringComparison.OrdinalIgnoreCase);
                AgentLog("H2", "Program.RunI18nSmokeTest", "EN after AR toggle", new Dictionary<string, object> { ["value"] = afterEn ?? "", ["ok"] = switchBackOk });
                if (!switchBackOk) failures++;

                var summary = new SalesReportSummary
                {
                    FromDate = DateTime.Today,
                    ToDate = DateTime.Today,
                    TotalSales = 100m,
                    TotalCost = 40m,
                    TotalProfit = 60m,
                    TotalExpenses = 10m,
                    TotalProfitAfterExpenses = 50m
                };
                var detail = new System.Data.DataTable();
                string exportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "test-exports");
                Directory.CreateDirectory(exportDir);

                LocalizationManager.SetLanguage("ar");
                string arPath = Path.Combine(exportDir, "i18n_ar_export.xlsx");
                bool arExport = ImportExportHelper.ExportSalesReport(arPath, summary, detail, LocalizationManager.GetString("Rep_Period_Monthly"));
                string arCellA1 = "";
                if (arExport && File.Exists(arPath))
                {
                    using var wb = new ClosedXML.Excel.XLWorkbook(arPath);
                    arCellA1 = wb.Worksheet(1).Cell(1, 1).GetString();
                }
                bool arExcelOk = arExport && arCellA1.Contains("تقارير");
                AgentLog("H3", "Program.RunI18nSmokeTest", "AR Excel A1", new Dictionary<string, object> { ["exportOk"] = arExport, ["a1"] = arCellA1, ["ok"] = arExcelOk });
                if (!arExcelOk) failures++;

                LocalizationManager.SetLanguage("en-US");
                string enPath = Path.Combine(exportDir, "i18n_en_export.xlsx");
                bool enExport = ImportExportHelper.ExportSalesReport(enPath, summary, detail, LocalizationManager.GetString("Rep_Period_Monthly"));
                string enCellA1 = "";
                if (enExport && File.Exists(enPath))
                {
                    using var wb = new ClosedXML.Excel.XLWorkbook(enPath);
                    enCellA1 = wb.Worksheet(1).Cell(1, 1).GetString();
                }
                bool enExcelOk = enExport && enCellA1.Contains("Sales", StringComparison.OrdinalIgnoreCase);
                AgentLog("H3", "Program.RunI18nSmokeTest", "EN Excel A1", new Dictionary<string, object> { ["exportOk"] = enExport, ["a1"] = enCellA1, ["ok"] = enExcelOk });
                if (!enExcelOk) failures++;

                string[] navKeys = { "Nav_Inventory", "Nav_Suppliers", "Nav_Quotations", "Nav_Reports", "Nav_Customers" };
                LocalizationManager.SetLanguage("ar");
                var missingAr = new List<string>();
                foreach (var k in navKeys)
                {
                    var v = LocalizationManager.GetString(k);
                    if (string.IsNullOrEmpty(v) || v == k) missingAr.Add(k);
                }
                AgentLog("H4", "Program.RunI18nSmokeTest", "AR nav keys", new Dictionary<string, object> { ["missing"] = string.Join(",", missingAr), ["ok"] = missingAr.Count == 0 });
                if (missingAr.Count > 0) failures++;

                LocalizationManager.SetLanguage("ar");
                string arQuote = LocalizationManager.GetString("QuotePreview_Quote");
                bool quoteArOk = !string.IsNullOrEmpty(arQuote) && arQuote != "QuotePreview_Quote";
                AgentLog("H5", "Program.RunI18nSmokeTest", "AR quotation preview strings", new Dictionary<string, object> { ["QuotePreview_Quote"] = arQuote ?? "", ["ok"] = quoteArOk });
                if (!quoteArOk) failures++;

                Console.WriteLine($"I18N SMOKE: failures={failures} (see {logPath})");
                return failures == 0 ? 0 : 1;
            }
            catch (Exception ex)
            {
                AgentLog("H0", "Program.RunI18nSmokeTest", "exception", new Dictionary<string, object> { ["error"] = ex.Message });
                Console.WriteLine("I18N SMOKE EXCEPTION: " + ex);
                return 2;
            }
        }

        /// <summary>
        /// Writes every export workbook to Data/test-exports and prints the resulting column widths
        /// so clipped columns can be spotted without opening Excel.
        /// </summary>
        private static int RunExportSmokeTest()
        {
            try
            {
                DatabaseInitializer.Initialize();
                DatabaseHelper.EnsureSchema();
                CurrencyService.EnsureTable();

                string exportDir = Path.Combine(Application.StartupPath, "Data", "test-exports");
                Directory.CreateDirectory(exportDir);

                var reportRange = new ReportService().GetPresetRange("Yearly");
                var sets = new (string Name, System.Data.DataTable Table)[]
                {
                    ("products", CsvImportExportService.BuildProductsExportTable()),
                    ("customers", CsvImportExportService.BuildCustomersExportTable()),
                    ("suppliers", CsvImportExportService.BuildSuppliersExportTable()),
                    ("expenses", CsvImportExportService.BuildExpensesExportTable()),
                    ("sales", CsvImportExportService.BuildSalesExportTable()),
                    ("history", CsvImportExportService.BuildHistoryExportTable()),
                    ("report-summary", CsvImportExportService.BuildReportSummaryExportTable(
                        new ReportService().GetSummary(reportRange.from, reportRange.to))),
                    ("report-products", CsvImportExportService.BuildReportProductsExportTable(
                        reportRange.from, reportRange.to))
                };

                foreach (var (name, table) in sets)
                {
                    byte[] bytes = ImportExportHelper.ExportDataTableToXlsx(table, name);
                    string path = Path.Combine(exportDir, name + ".xlsx");
                    File.WriteAllBytes(path, bytes);
                    Console.WriteLine($"[{name}] rows={table.Rows.Count} cols={table.Columns.Count} -> {path}");

                    using var wb = new ClosedXML.Excel.XLWorkbook(path);
                    var ws = wb.Worksheet(1);
                    for (int c = 1; c <= table.Columns.Count; c++)
                    {
                        string header = table.Columns[c - 1].ColumnName;
                        int longest = header.Length;
                        foreach (System.Data.DataRow row in table.Rows)
                        {
                            int len = (row[c - 1]?.ToString() ?? "").Trim().Length;
                            if (len > longest) longest = len;
                        }
                        string fmt = ws.Cell(2, c).Style.NumberFormat.Format;
                        Console.WriteLine($"    {header,-24} longest={longest,4} width={ws.Column(c).Width,5:0.0}" +
                            (string.IsNullOrEmpty(fmt) ? "" : $"  fmt={fmt}"));
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static int RunReportsSmokeTest()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                LocalizationManager.SetLanguage("en-US");
                DatabaseInitializer.Initialize();
                DatabaseHelper.EnsureSchema();
                CurrencyService.EnsureTable();

                var svc = new ReportService();
                string exportDir = Path.Combine(Application.StartupPath, "Data", "test-exports");
                Directory.CreateDirectory(exportDir);

                string[] presets = { "Daily", "Weekly", "Monthly", "Yearly" };
                int failures = 0;

                foreach (string preset in presets)
                {
                    var (from, to) = svc.GetPresetRange(preset);
                    var summary = svc.GetSummary(from, to);
                    var products = svc.GetTopSellingProducts(from, to, 25);
                    var categories = svc.GetTopSellingCategories(from, to, 25);
                    var detail = svc.GetSoldProductsDetail(from, to);

                    Console.WriteLine($"[{preset}] {from:yyyy-MM-dd}..{to:yyyy-MM-dd}");
                    Console.WriteLine($"  Sales={summary.TotalSales:0.00} Cost={summary.TotalCost:0.00} Expenses={summary.TotalExpenses:0.00} Profit={summary.TotalProfit:0.00} AfterExpenses={summary.TotalProfitAfterExpenses:0.00}");
                    Console.WriteLine($"  Products={products.Rows.Count} Categories={categories.Rows.Count} DetailRows={detail.Rows.Count}");

                    if (preset is "Monthly" or "Yearly" or "Weekly")
                    {
                        if (summary.TotalSales <= 0 || products.Rows.Count == 0 || categories.Rows.Count == 0)
                        {
                            Console.WriteLine($"  FAIL: expected seeded sales data for {preset}");
                            failures++;
                        }
                    }

                    string path = Path.Combine(exportDir, $"report_{preset}_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                    bool ok = ImportExportHelper.ExportSalesReport(path, summary, detail, preset);
                    if (!ok || !File.Exists(path))
                    {
                        Console.WriteLine($"  FAIL: export failed for {preset}");
                        failures++;
                    }
                    else
                    {
                        Console.WriteLine($"  Export OK: {path} ({new FileInfo(path).Length} bytes)");
                    }
                }

                // Custom range spanning full year
                var customFrom = new DateTime(DateTime.Today.Year, 1, 1);
                var customTo = DateTime.Today;
                var custom = svc.GetSummary(customFrom, customTo);
                Console.WriteLine($"[Custom] Sales={custom.TotalSales:0.00}");
                if (custom.TotalSales <= 0)
                {
                    Console.WriteLine("  FAIL: custom yearly range empty");
                    failures++;
                }

                if (failures == 0)
                {
                    Console.WriteLine("REPORTS SMOKE TEST PASSED");
                    return 0;
                }

                Console.WriteLine($"REPORTS SMOKE TEST FAILED ({failures} issues)");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("REPORTS SMOKE TEST EXCEPTION: " + ex);
                return 2;
            }
        }

        private static void StartApiServer()
        {
            try
            {
                InventorySystem.Helpers.StartupTrace.Mark("api: starting");
                string appRoot = ResolveAppRoot();

                string certPath = System.IO.Path.Combine(appRoot, "Database", "pos_cert.pfx");
                const string certPassword = "SoftioPos2026!";

                var builder = WebApplication.CreateBuilder(new WebApplicationOptions
                {
                    ContentRootPath = appRoot,
                    WebRootPath = System.IO.Path.Combine(appRoot, "wwwroot")
                });

                // Configure Kestrel for both HTTP and HTTPS
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(AppHostConfig.HttpPort); // HTTP — unique per brand
                    if (System.IO.File.Exists(certPath))
                    {
                        options.ListenAnyIP(AppHostConfig.HttpsPort, listenOptions =>
                        {
                            listenOptions.UseHttps(certPath, certPassword);
                        });
                    }
                });

                builder.Services.AddCors(c => c.AddDefaultPolicy(p =>
                    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

                builder.Services.AddHostedService<AutoBackupBackgroundService>();

                // - SignalR for real-time sync -
                builder.Services.AddSignalR();

                var app = builder.Build();
                InventorySystem.Helpers.StartupTrace.Mark("api: built");

                // --- Discovery Logic ---
                string localIp = "localhost";
                try
                {
                    var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                    localIp = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString() ?? "localhost";
                }
                catch { }

                // Register HubContext so WinForms can broadcast events
                InventoryBroadcaster.HubContext = app.Services
                    .GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<InventoryHub>>();

                // Scale hardware (TM-A17 / COM) — auto-connect only when Softio feature flag is on
                ScaleApiBootstrap.WireBroadcasts();

                app.UseCors();

                // Camera requires HTTPS OR the Chrome Flag (chrome://flags/#unsafely-treat-insecure-origin-as-secure)
                // We'll allow both HTTP and HTTPS to co-exist for easier access
                // if (!builder.Environment.IsDevelopment()) { app.UseHsts(); }
                // app.UseHttpsRedirection();

                app.UseDefaultFiles();
                app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
                {
                    OnPrepareResponse = ctx =>
                    {
                        var ext = System.IO.Path.GetExtension(ctx.File.Name).ToLowerInvariant();
                        if (ext is ".js" or ".css" or ".html" or ".json" or ".svg")
                        {
                            // Force UTF-8 so Arabic strings in app.js parse correctly
                            var ct = ctx.Context.Response.ContentType ?? "application/octet-stream";
                            if (!ct.Contains("charset", StringComparison.OrdinalIgnoreCase))
                                ctx.Context.Response.ContentType = ct + "; charset=utf-8";
                            // Avoid stale cached broken JS/CSS after updates
                            ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                        }
                        else if (ext is ".woff2" or ".woff" or ".ttf" or ".png" or ".jpg" or ".ico")
                        {
                            // Fonts and images never change between launches: let the browser keep them
                            ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=2592000";
                        }
                    }
                });

                // Serve desktop assets to the web portal
                string assetsPath = System.IO.Path.Combine(builder.Environment.ContentRootPath, "Assets");
                if (System.IO.Directory.Exists(assetsPath))
                {
                    app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
                    {
                        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(assetsPath),
                        RequestPath = "/Assets",
                        OnPrepareResponse = ctx =>
                        {
                            ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=604800";
                        }
                    });
                }

                // - SignalR Hub endpoint -
                app.MapHub<InventoryHub>("/hubs/inventory");

                // - Status -
                app.MapGet("/api/status", () => Microsoft.AspNetCore.Http.Results.Ok(new { status = "API Running", version = "2.0", realtime = "SignalR Active" }));

                // - Config/Language -
                app.MapGet("/api/config", () => Microsoft.AspNetCore.Http.Results.Ok(new
                {
                    language = LocalizationManager.IsArabic ? "ar" : "en",
                    isArabic = LocalizationManager.IsArabic,
                    companyName = ThemeConfig.CompanyName,
                    primaryColor = System.Drawing.ColorTranslator.ToHtml(ThemeConfig.PrimaryColor),
                    primaryRgb = $"{ThemeConfig.PrimaryColor.R}, {ThemeConfig.PrimaryColor.G}, {ThemeConfig.PrimaryColor.B}"
                }));

                // - Dashboard stats -
                app.MapGet("/api/dashboard", () =>
                {
                    try
                    {
                        var dash = new DashboardService();
                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            todaySales = dash.GetSales("Today"),
                            inventoryValue = dash.GetTotalInventoryValue(),
                            totalItems = dash.GetTotalItems(),
                            lowStock = dash.GetLowStockCount(),
                            ordersToday = dash.GetOrdersCount("Today"),
                            pendingOrders = dash.GetPendingOrdersCount(),
                            ytdSales = dash.GetTotalSalesYTD(),
                            topProducts = DataTableToList(dash.GetTopSellingItems(5)),
                            topCategories = DataTableToList(dash.GetSalesByCategory())
                        });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Dashboard error: " + ex.Message);
                    }
                });

                // - Scan to Connect (phone / tablet on same Wi‑Fi) -
                app.MapGet("/api/brand", () => Microsoft.AspNetCore.Http.Results.Ok(new
                {
                    brandId = AppHostConfig.BrandId,
                    appName = ThemeConfig.AppTitle,
                    port = AppHostConfig.HttpPort
                }));

                app.MapGet("/api/connect", () =>
                {
                    try
                    {
                        string ip = ScanToConnectForm.GetLocalIpAddress();
                        string url = ScanToConnectForm.GetServerUrl();
                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            url,
                            ip,
                            port = AppHostConfig.HttpPort,
                            qrUrl = "/api/connect/qr"
                        });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem(ex.Message);
                    }
                });

                app.MapGet("/api/connect/qr", () =>
                {
                    try
                    {
                        string url = ScanToConnectForm.GetServerUrl();
                        using var generator = new QRCodeGenerator();
                        using var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M);
                        var png = new PngByteQRCode(data);
                        byte[] bytes = png.GetGraphic(10);
                        return Microsoft.AspNetCore.Http.Results.File(bytes, "image/png");
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem(ex.Message);
                    }
                });

                // - Customers -
                app.MapGet("/api/customers", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT customer_id, full_name, phone, email, address, current_balance, type,
                                     COALESCE(credit_limit, 1000) as credit_limit
                              FROM customers WHERE date_deleted IS NULL ORDER BY full_name");
                        var list = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            list.Add(new
                            {
                                id = Convert.ToInt32(row["customer_id"]),
                                name = row["full_name"].ToString(),
                                phone = row["phone"].ToString(),
                                email = row["email"].ToString(),
                                address = row["address"].ToString(),
                                balance = row["current_balance"] == DBNull.Value ? 0m : Convert.ToDecimal(row["current_balance"]),
                                type = row["type"].ToString(),
                                creditLimit = row["credit_limit"] == DBNull.Value ? 1000m : Convert.ToDecimal(row["credit_limit"])
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(list);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Suppliers -
                app.MapGet("/api/suppliers", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT id, supplier_name, contact_person, phone, email, address, COALESCE(balance_due, 0) as balance_due
                              FROM suppliers WHERE date_deleted IS NULL ORDER BY supplier_name");
                        var list = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            list.Add(new
                            {
                                id = Convert.ToInt32(row["id"]),
                                name = row["supplier_name"].ToString(),
                                contact = row["contact_person"].ToString(),
                                phone = row["phone"].ToString(),
                                email = row["email"].ToString(),
                                address = row["address"].ToString(),
                                balance = Convert.ToDecimal(row["balance_due"])
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(list);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Users -
                app.MapGet("/api/users", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable(
                            "SELECT id, username, full_name, role FROM users ORDER BY username");
                        var list = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            list.Add(new
                            {
                                id = Convert.ToInt32(row["id"]),
                                username = row["username"].ToString(),
                                fullName = row["full_name"].ToString(),
                                role = row["role"].ToString()
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(list);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // Wire up dynamic language broadcast to connected web portals
                LocalizationManager.LanguageChanged += (s, e) =>
                {
                    _ = InventoryBroadcaster.Broadcast("LanguageChanged", LocalizationManager.IsArabic ? "ar" : "en");
                };

                // - Products (live from DB) -
                app.MapGet("/api/products", (string includeInactive = null) =>
                {
                    try
                    {
                        bool showInactive = string.Equals(includeInactive, "1", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(includeInactive, "true", StringComparison.OrdinalIgnoreCase);
                        string statusFilter = showInactive
                            ? "WHERE p.date_deleted IS NULL"
                            : "WHERE p.date_deleted IS NULL AND (p.status = 'Active' OR p.status IS NULL) AND COALESCE(p.is_inactive, 0) = 0";

                        var dt = DatabaseHelper.ExecuteDataTable(
                            $@"SELECT p.id, p.part_name, p.description, p.selling_price, p.purchase_price, p.quantity_in_stock,
                                     p.minimum_stock_level, p.barcode, p.part_number, p.part_image, p.status,
                                     p.location, p.shelf, p.unit_of_measure, p.batch_number, p.expiry_date,
                                     p.item_type, p.is_sales_item, p.is_purchase_item, p.is_inactive, p.tax_rate,
                                     p.is_stock_tracked, COALESCE(p.sell_by_weight, 0) AS sell_by_weight, p.price2, p.price3, p.price4, p.supplier_id,
                                     COALESCE(c.category_name, 'General') AS category,
                                     c.category_image, s.supplier_name
                              FROM parts p
                              LEFT JOIN categories c ON p.category_id = c.id
                              LEFT JOIN suppliers s ON p.supplier_id = s.id
                              {statusFilter}
                              ORDER BY c.category_name, p.part_name");

                        var products = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            string partImage = row["part_image"]?.ToString() ?? "";
                            string catImage = row["category_image"]?.ToString() ?? "";
                            string category = row["category"].ToString();
                            string itemType = row["item_type"]?.ToString() ?? "Product";

                            if (string.IsNullOrEmpty(catImage))
                            {
                                string cleanCat = category.ToLower().Trim();
                                string[] extensions = { ".svg", ".png" };
                                bool found = false;
                                foreach (var ext in extensions)
                                {
                                    string iconFile = $"nuricon_{cleanCat}{ext}";
                                    if (System.IO.File.Exists(System.IO.Path.Combine(assetsPath, iconFile)))
                                    {
                                        catImage = "/Assets/" + iconFile;
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                    catImage = cleanCat.Contains("service") ? "/Assets/nuricon_pos.png" : "";
                            }
                            else if (catImage.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                                catImage = "/" + catImage;
                            else if (!catImage.StartsWith("/") && !catImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                                catImage = "/Assets/" + catImage;

                            if (!string.IsNullOrEmpty(partImage))
                            {
                                if (partImage.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                                    partImage = "/" + partImage;
                                else if (!partImage.StartsWith("/") && !partImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                                    partImage = "/Assets/" + partImage;
                            }

                            bool inactive = row["is_inactive"] != DBNull.Value && Convert.ToInt32(row["is_inactive"]) == 1;
                            string status = row["status"]?.ToString() ?? "Active";
                            if (inactive) status = "Inactive";

                            products.Add(new
                            {
                                id = Convert.ToInt32(row["id"]),
                                name = row["part_name"].ToString(),
                                description = row["description"]?.ToString() ?? "",
                                price = Convert.ToDecimal(row["selling_price"] == DBNull.Value ? 0 : row["selling_price"]),
                                cost = Convert.ToDecimal(row["purchase_price"] == DBNull.Value ? 0 : row["purchase_price"]),
                                stock = Convert.ToInt32(row["quantity_in_stock"] == DBNull.Value ? 0 : row["quantity_in_stock"]),
                                minStock = Convert.ToInt32(row["minimum_stock_level"] == DBNull.Value ? 0 : row["minimum_stock_level"]),
                                barcode = row["barcode"]?.ToString() ?? "",
                                sku = row["part_number"]?.ToString() ?? "",
                                category,
                                image = partImage,
                                categoryImage = catImage,
                                status,
                                location = row["location"]?.ToString() ?? "",
                                shelf = row["shelf"]?.ToString() ?? "",
                                uom = row["unit_of_measure"]?.ToString() ?? "",
                                batch = row["batch_number"]?.ToString() ?? "",
                                expiry = row["expiry_date"]?.ToString() ?? "",
                                itemType,
                                isService = itemType.Equals("Service", StringComparison.OrdinalIgnoreCase),
                                isSalesItem = row["is_sales_item"] == DBNull.Value || Convert.ToInt32(row["is_sales_item"]) == 1,
                                isPurchaseItem = row["is_purchase_item"] != DBNull.Value && Convert.ToInt32(row["is_purchase_item"]) == 1,
                                isInactive = inactive,
                                taxRate = row["tax_rate"] == DBNull.Value ? 0m : Convert.ToDecimal(row["tax_rate"]),
                                isStockTracked = row["is_stock_tracked"] == DBNull.Value || Convert.ToInt32(row["is_stock_tracked"]) == 1,
                                sellByWeight = false,
                                price2 = row["price2"] == DBNull.Value ? 0m : Convert.ToDecimal(row["price2"]),
                                price3 = row["price3"] == DBNull.Value ? 0m : Convert.ToDecimal(row["price3"]),
                                price4 = row["price4"] == DBNull.Value ? 0m : Convert.ToDecimal(row["price4"]),
                                supplierId = row["supplier_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["supplier_id"]),
                                supplierName = row["supplier_name"]?.ToString() ?? ""
                            });
                        }

                        return Microsoft.AspNetCore.Http.Results.Ok(products);
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("DB error: " + ex.Message);
                    }
                });

                // - Categories -
                app.MapGet("/api/categories", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable("SELECT category_name FROM categories ORDER BY category_name");
                        var categories = new System.Collections.Generic.List<string>();
                        foreach (System.Data.DataRow row in dt.Rows)
                            categories.Add(row["category_name"].ToString());

                        return Microsoft.AspNetCore.Http.Results.Ok(categories);
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("DB error: " + ex.Message);
                    }
                });

                // - Login (POST) -
                app.MapPost("/api/login", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<LoginPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || string.IsNullOrEmpty(body.Username) || string.IsNullOrEmpty(body.Password))
                            return Microsoft.AspNetCore.Http.Results.BadRequest("Missing credentials");

                        // Allow Softio super-admin through the web POS too
                        if (body.Username == "Softio.Admin" && body.Password == "Softio@2026!")
                            return Microsoft.AspNetCore.Http.Results.Ok(new
                            {
                                username = "Softio.Admin",
                                role = "Admin",
                                fullName = "Softio Super Admin",
                                isAdmin = true,
                                isStaff = true,
                                isAccountant = true,
                                isSoftioSuperAdmin = true,
                                features = new { scaleEnabled = FeatureFlags.ScaleEnabled, quickSaleEnabled = FeatureFlags.QuickSaleEnabled }
                            });

                        var dt = DatabaseHelper.ExecuteDataTable(
                            "SELECT username, role, full_name FROM users WHERE username = @u AND password = @p",
                            new Microsoft.Data.Sqlite.SqliteParameter("@u", body.Username),
                            new Microsoft.Data.Sqlite.SqliteParameter("@p", body.Password));

                        if (dt.Rows.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.Unauthorized();

                        var row = dt.Rows[0];
                        string role = row["role"].ToString() ?? "";
                        string username = row["username"].ToString() ?? "";
                        bool isAdmin = role.Contains("Admin", StringComparison.OrdinalIgnoreCase);
                        bool isStaff = isAdmin || role.Contains("Staff", StringComparison.OrdinalIgnoreCase) || role.Equals("Worker", StringComparison.OrdinalIgnoreCase);
                        bool isAccountant = isAdmin || role.Contains("Accountant", StringComparison.OrdinalIgnoreCase);
                        bool isSoftio = username.Equals("Softio.Admin", StringComparison.OrdinalIgnoreCase);
                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            username,
                            role = role,
                            fullName = row["full_name"].ToString(),
                            isAdmin,
                            isStaff,
                            isAccountant,
                            isSoftioSuperAdmin = isSoftio,
                            features = new { scaleEnabled = FeatureFlags.ScaleEnabled, quickSaleEnabled = FeatureFlags.QuickSaleEnabled }
                        });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Login error: " + ex.Message);
                    }
                });

                // - Add / Save Product (full PartData) -
                app.MapPost("/api/add-item", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<ProductPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || string.IsNullOrWhiteSpace(body.Name))
                            return Microsoft.AspNetCore.Http.Results.BadRequest("Missing name");

                        if (!string.IsNullOrEmpty(body.Barcode) && new InventoryService().BarcodeExists(body.Barcode, null))
                            return Microsoft.AspNetCore.Http.Results.Conflict(new { error = "Barcode already exists for another item." });

                        var part = MapProductPayload(body, 0);
                        new InventoryService().SaveProductService(part);
                        int newId = 0;
                        try
                        {
                            newId = DatabaseHelper.ExecuteScalar<int>(
                                @"SELECT id FROM parts
                                  WHERE part_name = @n AND date_deleted IS NULL
                                  ORDER BY id DESC LIMIT 1",
                                new Microsoft.Data.Sqlite.SqliteParameter("@n", body.Name.Trim()));
                        }
                        catch { }
                        if (body.SupplierPurchaseItemId.HasValue && body.SupplierPurchaseItemId.Value > 0 && newId > 0)
                        {
                            try { new SupplierPurchaseService().LinkToPart(body.SupplierPurchaseItemId.Value, newId); }
                            catch { }
                        }
                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Item '{body.Name}' added");
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, id = newId, barcode = part.Barcode });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Failed to add item: " + ex.Message);
                    }
                });

                app.MapPost("/api/products/upload-image", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        if (!request.HasFormContentType)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Expected multipart form" });
                        var form = await request.ReadFormAsync();
                        var file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();
                        if (file == null || file.Length == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "No file" });
                        string ext = System.IO.Path.GetExtension(file.FileName);
                        if (string.IsNullOrWhiteSpace(ext)) ext = ".png";
                        string relDir = "Assets/Products";
                        string absDir = System.IO.Path.Combine(appRoot, "Assets", "Products");
                        System.IO.Directory.CreateDirectory(absDir);
                        string name = Guid.NewGuid().ToString("N") + ext.ToLowerInvariant();
                        string abs = System.IO.Path.Combine(absDir, name);
                        using (var stream = System.IO.File.Create(abs))
                            await file.CopyToAsync(stream);
                        string rel = relDir.Replace('\\', '/') + "/" + name;
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, path = rel, url = "/" + rel });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Checkout (POST) - supports Completed / Quotation / Draft + optional customer -
                app.MapPost("/api/checkout", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<CheckoutPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || body.Items == null || body.Items.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest("Empty cart");

                        decimal subtotal = 0;
                        var orderItems = new System.Collections.Generic.List<OrderItem>();
                        foreach (var item in body.Items)
                        {
                            subtotal += item.Price * item.Qty;
                            orderItems.Add(new OrderItem
                            {
                                PartId = item.Id,
                                PartName = item.Name ?? "",
                                Quantity = item.Qty,
                                UnitPrice = item.Price,
                                StockDeductQty = item.StockQty > 0 ? item.StockQty : 0,
                                SkipStock = item.SkipStock || item.Id <= 0
                            });
                        }

                        decimal vat = Math.Max(0, body.VatAmount);
                        decimal ship = Math.Max(0, body.ShippingAmount);
                        decimal disc = Math.Max(0, body.DiscountAmount);
                        decimal calc = Math.Max(0, subtotal + vat + ship - disc);
                        decimal total = body.TotalAmount.HasValue && body.TotalAmount.Value >= 0
                            ? body.TotalAmount.Value
                            : calc;

                        string status = string.IsNullOrWhiteSpace(body.Status) ? "Completed" : body.Status.Trim();
                        if (status != "Completed" && status != "Quotation" && status != "Draft" && status != "Bill")
                            status = "Completed";
                        bool billMode = status == "Bill" || (body.IsPaid == false && status == "Completed");
                        if (status == "Bill") status = "Completed";

                        int customerId = body.CustomerId.GetValueOrDefault() > 0 ? body.CustomerId.Value : -1;
                        bool isPaid = body.IsPaid ?? (status == "Completed" && !billMode);
                        if (status == "Quotation" || status == "Draft") isPaid = false;
                        if (billMode) isPaid = false;

                        int orderId = new OrderService().PlaceOrder(customerId, orderItems, total, isPaid, status,
                            body.DueDate, body.ShippingAddress, body.DeliveryDate);

                        _ = InventoryBroadcaster.Broadcast(
                            status == "Completed" ? "SaleCompleted" : "InventoryChanged",
                            $"{status} #{orderId} - Total: {total:F2}");

                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, orderId, total, status });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Checkout failed: " + ex.Message);
                    }
                });

                // - Currencies (GET) -
                app.MapGet("/api/currencies", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable("SELECT code, name, symbol, rate_vs_usd FROM currency_rates ORDER BY code");
                        var currencies = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            currencies.Add(new
                            {
                                code = row["code"].ToString(),
                                name = row["name"].ToString(),
                                symbol = row["symbol"].ToString(),
                                rate = Convert.ToDecimal(row["rate_vs_usd"])
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(currencies);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Recent Sales (GET) -
                app.MapGet("/api/recent-sales", (string from, string to) =>
                {
                    try
                    {
                        // Optional date range (Sales page filter); without it, the latest 100 sales
                        bool ranged = DateTime.TryParse(from, out var fromDate) & DateTime.TryParse(to, out var toDate);
                        if (ranged && toDate < fromDate) (fromDate, toDate) = (toDate, fromDate);
                        string sql = @"SELECT o.order_id, o.order_date, o.total_amount, o.payment_status,
                                     COALESCE(c.full_name, 'Cash Customer') as customer_name
                              FROM orders o
                              LEFT JOIN customers c ON o.customer_id = c.customer_id
                              WHERE o.status = 'Completed'"
                            + (ranged ? " AND date(o.order_date) BETWEEN date(@from) AND date(@to)" : "")
                            + " ORDER BY o.order_id DESC LIMIT " + (ranged ? "5000" : "100");
                        var dt = ranged
                            ? DatabaseHelper.ExecuteDataTable(sql,
                                new Microsoft.Data.Sqlite.SqliteParameter("@from", fromDate.ToString("yyyy-MM-dd")),
                                new Microsoft.Data.Sqlite.SqliteParameter("@to", toDate.ToString("yyyy-MM-dd")))
                            : DatabaseHelper.ExecuteDataTable(sql);

                        var sales = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            string pay = row["payment_status"]?.ToString() ?? "Paid";
                            sales.Add(new
                            {
                                orderId = Convert.ToInt32(row["order_id"]),
                                date = row["order_date"],
                                total = Convert.ToDecimal(row["total_amount"]),
                                customer = row["customer_name"].ToString(),
                                paymentStatus = pay
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(sales);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Order Details (GET) -
                app.MapGet("/api/order-details/{id}", (int id) =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT COALESCE(oi.part_id, 0) AS part_id,
                                     COALESCE(NULLIF(TRIM(oi.item_name), ''), p.part_name, 'Quick Sale') AS part_name,
                                     COALESCE(p.description, '') AS description,
                                     COALESCE(p.part_image, '') AS part_image,
                                     oi.quantity, oi.price
                              FROM order_items oi
                              LEFT JOIN parts p ON oi.part_id = p.id
                              WHERE oi.order_id = @id",
                            new Microsoft.Data.Sqlite.SqliteParameter("@id", id));

                        var items = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            items.Add(new
                            {
                                partId = Convert.ToInt32(row["part_id"] == DBNull.Value ? 0 : row["part_id"]),
                                name = row["part_name"].ToString(),
                                description = row["description"] == DBNull.Value ? "" : row["description"].ToString(),
                                image = row["part_image"] == DBNull.Value ? "" : row["part_image"].ToString(),
                                qty = Convert.ToInt32(row["quantity"]),
                                price = Convert.ToDecimal(row["price"])
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(items);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Customer recent sales (for POS return) -
                app.MapGet("/api/customers/{id:int}/sales", (int id) =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT o.order_id, o.order_date, o.total_amount, o.payment_status,
                                     (SELECT COUNT(*) FROM order_items oi WHERE oi.order_id = o.order_id) AS item_count
                              FROM orders o
                              WHERE o.customer_id = @cid AND o.status = 'Completed'
                              ORDER BY o.order_id DESC
                              LIMIT 40",
                            new Microsoft.Data.Sqlite.SqliteParameter("@cid", id));
                        var list = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            list.Add(new
                            {
                                orderId = Convert.ToInt32(row["order_id"]),
                                date = row["order_date"],
                                total = Convert.ToDecimal(row["total_amount"]),
                                paymentStatus = row["payment_status"]?.ToString() ?? "Paid",
                                itemCount = Convert.ToInt32(row["item_count"])
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(list);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Order returnable lines (sold / already returned / remaining + unit price) -
                app.MapGet("/api/orders/{id:int}/returnable", (int id) =>
                {
                    try
                    {
                        var orderDt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT o.order_id, o.order_date, o.total_amount, o.payment_status, o.customer_id,
                                     COALESCE(c.full_name, 'Walk-in Customer') AS customer_name
                              FROM orders o
                              LEFT JOIN customers c ON o.customer_id = c.customer_id
                              WHERE o.order_id = @id AND o.status = 'Completed'",
                            new Microsoft.Data.Sqlite.SqliteParameter("@id", id));
                        if (orderDt.Rows.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.NotFound(new { error = "Order not found" });

                        var ord = orderDt.Rows[0];
                        var dt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT oi.part_id, p.part_name, oi.quantity AS sold_qty, oi.price,
                                     COALESCE((
                                         SELECT SUM(ri.quantity)
                                         FROM return_items ri
                                         INNER JOIN returns r ON r.return_id = ri.return_id
                                         WHERE r.order_id = @id AND ri.part_id = oi.part_id
                                     ), 0) AS returned_qty
                              FROM order_items oi
                              JOIN parts p ON p.id = oi.part_id
                              WHERE oi.order_id = @id",
                            new Microsoft.Data.Sqlite.SqliteParameter("@id", id));

                        var items = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            int sold = Convert.ToInt32(row["sold_qty"]);
                            int returned = Convert.ToInt32(row["returned_qty"]);
                            int remaining = Math.Max(0, sold - returned);
                            decimal price = Convert.ToDecimal(row["price"]);
                            items.Add(new
                            {
                                partId = Convert.ToInt32(row["part_id"]),
                                name = row["part_name"].ToString(),
                                soldQty = sold,
                                returnedQty = returned,
                                remainingQty = remaining,
                                price,
                                lineTotal = price * sold
                            });
                        }

                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            orderId = Convert.ToInt32(ord["order_id"]),
                            date = ord["order_date"],
                            total = Convert.ToDecimal(ord["total_amount"]),
                            paymentStatus = ord["payment_status"]?.ToString() ?? "Paid",
                            customerId = ord["customer_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(ord["customer_id"]),
                            customerName = ord["customer_name"].ToString(),
                            items
                        });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Quotation Preview (GET) -
                app.MapGet("/api/quotations/{id:int}", (int id) =>
                {
                    try
                    {
                        var orderDt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT o.order_id, o.order_date, o.total_amount, o.customer_id, o.status,
                                     COALESCE(c.full_name, 'Walk-in Customer') AS customer_name,
                                     COALESCE(c.address, '') AS address,
                                     COALESCE(c.phone, '') AS phone
                              FROM orders o
                              LEFT JOIN customers c ON o.customer_id = c.customer_id
                              WHERE o.order_id = @id",
                            new Microsoft.Data.Sqlite.SqliteParameter("@id", id));
                        if (orderDt.Rows.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.NotFound(new { error = "Quotation not found" });

                        var o = orderDt.Rows[0];
                        var itemsDt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT oi.part_id, p.part_name, p.description, p.part_image, oi.quantity, oi.price
                              FROM order_items oi
                              JOIN parts p ON oi.part_id = p.id
                              WHERE oi.order_id = @id",
                            new Microsoft.Data.Sqlite.SqliteParameter("@id", id));

                        var items = new System.Collections.Generic.List<object>();
                        decimal subtotal = 0;
                        foreach (System.Data.DataRow row in itemsDt.Rows)
                        {
                            var qty = Convert.ToInt32(row["quantity"]);
                            var price = Convert.ToDecimal(row["price"]);
                            subtotal += qty * price;
                            items.Add(new
                            {
                                partId = Convert.ToInt32(row["part_id"]),
                                name = row["part_name"].ToString(),
                                description = row["description"] == DBNull.Value ? "" : row["description"].ToString(),
                                image = row["part_image"] == DBNull.Value ? "" : row["part_image"].ToString(),
                                qty,
                                price
                            });
                        }

                        var totalAmount = Convert.ToDecimal(o["total_amount"]);
                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            orderId = Convert.ToInt32(o["order_id"]),
                            orderDate = o["order_date"],
                            status = o["status"].ToString(),
                            customerId = o["customer_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(o["customer_id"]),
                            customerName = o["customer_name"].ToString(),
                            address = o["address"].ToString(),
                            phone = o["phone"].ToString(),
                            subtotal,
                            totalAmount,
                            validityDays = 15,
                            companyName = ThemeConfig.CompanyName,
                            companyInfo = LocalizationManager.GetString("QuotePreview_CompanyInfo", ""),
                            items
                        });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Return Item (POST) -
                app.MapPost("/api/return-item", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<ReturnPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || body.Items == null || body.Items.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest("No items to return");

                        var returnService = new ReturnService();
                        var items = new System.Collections.Generic.List<ReturnItemInfo>();
                        foreach (var i in body.Items)
                        {
                            items.Add(new ReturnItemInfo
                            {
                                PartId = i.PartId,
                                Quantity = i.Qty,
                                RefundAmount = i.RefundAmount
                            });
                        }

                        // Use a dummy user or extract from context if we have one
                        UserSession.Username = "WebPOS";

                        returnService.ProcessReturn(body.OrderId, items, body.Reason);

                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Return processed for Order #{body.OrderId}");


                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Blind Return (POST) -
                app.MapPost("/api/blind-return", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<BlindReturnPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || body.Items == null || body.Items.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest("No items to return");

                        var returnService = new ReturnService();
                        var items = new System.Collections.Generic.List<ReturnItemInfo>();
                        foreach (var i in body.Items)
                        {
                            items.Add(new ReturnItemInfo
                            {
                                PartId = i.PartId,
                                Quantity = i.Qty,
                                RefundAmount = i.RefundAmount
                            });
                        }

                        UserSession.Username = "WebPOS";
                        int customerId = body.CustomerId.HasValue && body.CustomerId.Value > 0 ? body.CustomerId.Value : -1;
                        returnService.ProcessBlindReturn(items, body.Reason, customerId);

                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", "Blind return processed");

                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Reports -
                app.MapGet("/api/reports/summary", (string preset, string from, string to) =>
                {
                    try
                    {
                        var svc = new ReportService();
                        DateTime fromDate, toDate;
                        if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to)
                            && DateTime.TryParse(from, out fromDate) && DateTime.TryParse(to, out toDate))
                        {
                            if (toDate < fromDate) (fromDate, toDate) = (toDate, fromDate);
                        }
                        else
                        {
                            (fromDate, toDate) = svc.GetPresetRange(string.IsNullOrWhiteSpace(preset) ? "Monthly" : preset);
                        }
                        var s = svc.GetSummary(fromDate, toDate);
                        var act = svc.GetActivity(fromDate, toDate);
                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            fromDate = s.FromDate,
                            toDate = s.ToDate,
                            totalSales = s.TotalSales,
                            totalCost = s.TotalCost,
                            totalExpenses = s.TotalExpenses,
                            totalProfit = s.TotalProfit,
                            totalProfitAfterExpenses = s.TotalProfitAfterExpenses,
                            totalMilkExpenses = s.TotalMilkExpenses,
                            totalMilkLiters = s.TotalMilkLiters,
                            totalGeneralExpenses = s.TotalGeneralExpenses,
                            totalUnpaidMilkExpenses = s.TotalUnpaidMilkExpenses,
                            orderCount = act != null ? Convert.ToInt32(act["order_count"]) : 0,
                            itemsSold = act != null ? Convert.ToDecimal(act["items_sold"]) : 0m,
                            unpaidCount = act != null ? Convert.ToInt32(act["unpaid_count"]) : 0,
                            unpaidTotal = act != null ? Convert.ToDecimal(act["unpaid_total"]) : 0m,
                            returnsCount = act != null ? Convert.ToInt32(act["returns_count"]) : 0,
                            returnsTotal = act != null ? Convert.ToDecimal(act["returns_total"]) : 0m,
                            expenseCount = act != null ? Convert.ToInt32(act["expense_count"]) : 0
                        });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapGet("/api/reports/top-products", (string preset, string from, string to, int? limit) =>
                {
                    try
                    {
                        var svc = new ReportService();
                        DateTime fromDate, toDate;
                        if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to)
                            && DateTime.TryParse(from, out fromDate) && DateTime.TryParse(to, out toDate))
                        {
                            if (toDate < fromDate) (fromDate, toDate) = (toDate, fromDate);
                        }
                        else
                        {
                            (fromDate, toDate) = svc.GetPresetRange(string.IsNullOrWhiteSpace(preset) ? "Monthly" : preset);
                        }
                        var dt = svc.GetTopSellingProducts(fromDate, toDate, limit ?? 20);
                        return Microsoft.AspNetCore.Http.Results.Ok(DataTableToList(dt));
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Report details for the chosen period (products, categories, expenses, trend) -
                app.MapGet("/api/reports/products", (string preset, string from, string to) =>
                {
                    try { var svc = new ReportService(); var (f, t) = svc.ResolveRange(preset, from, to); return Microsoft.AspNetCore.Http.Results.Ok(DataTableToList(svc.GetProductsReport(f, t))); }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapGet("/api/reports/categories", (string preset, string from, string to) =>
                {
                    try { var svc = new ReportService(); var (f, t) = svc.ResolveRange(preset, from, to); return Microsoft.AspNetCore.Http.Results.Ok(DataTableToList(svc.GetTopSellingCategories(f, t, 1000))); }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapGet("/api/reports/expenses", (string preset, string from, string to) =>
                {
                    try { var svc = new ReportService(); var (f, t) = svc.ResolveRange(preset, from, to); return Microsoft.AspNetCore.Http.Results.Ok(DataTableToList(svc.GetExpensesList(f, t))); }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapGet("/api/reports/trend", (string preset, string from, string to) =>
                {
                    try { var svc = new ReportService(); var (f, t) = svc.ResolveRange(preset, from, to); return Microsoft.AspNetCore.Http.Results.Ok(svc.GetTrend(f, t)); }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - History -
                app.MapGet("/api/history/{kind}", (string kind) =>
                {
                    try
                    {
                        var svc = new HistoryService();
                        System.Data.DataTable dt = (kind ?? "").ToLowerInvariant() switch
                        {
                            "inventory" => svc.GetInventoryLogs(),
                            "customers" => svc.GetCustomerHistory(),
                            "orders" => svc.GetOrderHistory(),
                            "all" => svc.GetOrderHistory(),
                            "pay_later" => svc.GetOrderHistory(unpaidOnly: true),
                            "quotations" => svc.GetQuotationHistory(),
                            "suppliers" => svc.GetSupplierHistory(),
                            _ => svc.GetOrderHistory()
                        };
                        return Microsoft.AspNetCore.Http.Results.Ok(DataTableToList(dt));
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Quotations -
                app.MapGet("/api/quotations", () =>
                {
                    try
                    {
                        var dt = new OrderService().GetQuotations();
                        var list = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            list.Add(new
                            {
                                orderId = Convert.ToInt32(row["order_id"]),
                                orderDate = row["order_date"],
                                customerName = row["CustomerName"].ToString(),
                                totalAmount = Convert.ToDecimal(row["total_amount"]),
                                customerId = row["customer_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["customer_id"])
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(list);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapGet("/api/drafts", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT o.order_id, o.order_date, o.total_amount, o.status, o.customer_id,
                                     COALESCE(c.full_name, 'Walk-in') as customer_name
                              FROM orders o
                              LEFT JOIN customers c ON o.customer_id = c.customer_id
                              WHERE o.status = 'Draft' AND (o.date_deleted IS NULL OR o.date_deleted = '')
                              ORDER BY o.order_id DESC LIMIT 100");
                        var list = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            list.Add(new
                            {
                                orderId = Convert.ToInt32(row["order_id"]),
                                orderDate = row["order_date"],
                                customerId = row["customer_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["customer_id"]),
                                customerName = row["customer_name"].ToString(),
                                totalAmount = Convert.ToDecimal(row["total_amount"]),
                                status = row["status"].ToString()
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(list);
                    }
                    catch
                    {
                        // date_deleted may not exist on orders — fallback query
                        try
                        {
                            var dt = DatabaseHelper.ExecuteDataTable(
                                @"SELECT o.order_id, o.order_date, o.total_amount, o.status, o.customer_id,
                                         COALESCE(c.full_name, 'Walk-in') as customer_name
                                  FROM orders o
                                  LEFT JOIN customers c ON o.customer_id = c.customer_id
                                  WHERE o.status = 'Draft'
                                  ORDER BY o.order_id DESC LIMIT 100");
                            var list = new System.Collections.Generic.List<object>();
                            foreach (System.Data.DataRow row in dt.Rows)
                            {
                                list.Add(new
                                {
                                    orderId = Convert.ToInt32(row["order_id"]),
                                    orderDate = row["order_date"],
                                    customerId = row["customer_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["customer_id"]),
                                    customerName = row["customer_name"].ToString(),
                                    totalAmount = Convert.ToDecimal(row["total_amount"]),
                                    status = row["status"].ToString()
                                });
                            }
                            return Microsoft.AspNetCore.Http.Results.Ok(list);
                        }
                        catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                    }
                });

                app.MapPost("/api/quotations/{id:int}/convert", (int id) =>
                {
                    try
                    {
                        bool ok = new OrderService().ConvertToOrder(id);
                        if (ok) _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Quotation #{id} converted");
                        return ok
                            ? Microsoft.AspNetCore.Http.Results.Ok(new { success = true })
                            : Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Convert failed" });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Expenses -
                app.MapGet("/api/expenses", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT e.expense_id, e.expense_date, e.category, e.amount, e.description, e.recorded_by, e.is_paid, e.is_recurring,
                                     e.supplier_id, s.supplier_name, e.liters, e.unit_price, e.milk_type, e.shift
                              FROM expenses e
                              LEFT JOIN suppliers s ON s.id = e.supplier_id
                              WHERE e.date_deleted IS NULL AND e.category != 'System'
                              ORDER BY e.expense_date DESC LIMIT 500");
                        var list = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            list.Add(new
                            {
                                expenseId = Convert.ToInt32(row["expense_id"]),
                                expenseDate = row["expense_date"],
                                category = row["category"].ToString(),
                                amount = Convert.ToDecimal(row["amount"]),
                                description = row["description"].ToString(),
                                recordedBy = row["recorded_by"].ToString(),
                                isPaid = Convert.ToInt32(row["is_paid"]) == 1,
                                isRecurring = Convert.ToInt32(row["is_recurring"]) == 1,
                                supplierId = row["supplier_id"] != DBNull.Value ? (int?)Convert.ToInt32(row["supplier_id"]) : null,
                                supplierName = row["supplier_name"] != DBNull.Value ? row["supplier_name"].ToString() : null,
                                liters = row["liters"] != DBNull.Value ? Convert.ToDecimal(row["liters"]) : 0m,
                                unitPrice = row["unit_price"] != DBNull.Value ? Convert.ToDecimal(row["unit_price"]) : 0m,
                                milkType = row["milk_type"] != DBNull.Value ? row["milk_type"].ToString() : null,
                                shift = row["shift"] != DBNull.Value ? row["shift"].ToString() : null
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(list);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/expenses", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<ExpensePayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null)
                            return Microsoft.AspNetCore.Http.Results.BadRequest("Invalid expense");

                        decimal finalAmt = body.Amount;
                        if (body.Liters > 0 && body.UnitPrice > 0 && finalAmt <= 0)
                        {
                            finalAmt = body.Liters * body.UnitPrice;
                        }
                        if (string.IsNullOrWhiteSpace(body.Category))
                        {
                            body.Category = body.Liters > 0 ? "Raw Milk" : "General";
                        }
                        if (finalAmt <= 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest("Invalid expense amount");

                        DatabaseHelper.ExecuteNonQuery(
                            @"INSERT INTO expenses (category, expense_date, amount, description, recorded_by, is_paid, is_recurring, supplier_id, liters, unit_price, milk_type, shift)
                              VALUES (@c, @d, @a, @desc, @u, @paid, @rec, @sup, @lit, @up, @mt, @sh)",
                            new Microsoft.Data.Sqlite.SqliteParameter("@c", body.Category),
                            new Microsoft.Data.Sqlite.SqliteParameter("@d", body.ExpenseDate == default ? DateTime.Now : body.ExpenseDate),
                            new Microsoft.Data.Sqlite.SqliteParameter("@a", finalAmt),
                            new Microsoft.Data.Sqlite.SqliteParameter("@desc", body.Description ?? ""),
                            new Microsoft.Data.Sqlite.SqliteParameter("@u", string.IsNullOrWhiteSpace(body.RecordedBy) ? "Web" : body.RecordedBy),
                            new Microsoft.Data.Sqlite.SqliteParameter("@paid", body.IsPaid ? 1 : 0),
                            new Microsoft.Data.Sqlite.SqliteParameter("@rec", body.IsRecurring ? 1 : 0),
                            new Microsoft.Data.Sqlite.SqliteParameter("@sup", body.SupplierId.HasValue && body.SupplierId.Value > 0 ? (object)body.SupplierId.Value : DBNull.Value),
                            new Microsoft.Data.Sqlite.SqliteParameter("@lit", body.Liters),
                            new Microsoft.Data.Sqlite.SqliteParameter("@up", body.UnitPrice),
                            new Microsoft.Data.Sqlite.SqliteParameter("@mt", string.IsNullOrWhiteSpace(body.MilkType) ? DBNull.Value : (object)body.MilkType),
                            new Microsoft.Data.Sqlite.SqliteParameter("@sh", string.IsNullOrWhiteSpace(body.Shift) ? DBNull.Value : (object)body.Shift));
                        if (body.Liters > 0) new MilkProductionService().SyncRawMilkStock();
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/expenses/{id:int}/pay", (int id) =>
                {
                    try
                    {
                        return new ExpenseService().MarkAsPaid(id)
                            ? Microsoft.AspNetCore.Http.Results.Ok(new { success = true })
                            : Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Could not update the expense (database busy?). Please try again." });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/expenses/{id:int}/unpay", (int id) =>
                {
                    try
                    {
                        return new ExpenseService().MarkAsUnpaid(id)
                            ? Microsoft.AspNetCore.Http.Results.Ok(new { success = true })
                            : Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Could not update the expense (database busy?). Please try again." });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/expenses/{id:int}/toggle-paid", (int id) =>
                {
                    try
                    {
                        DatabaseHelper.ExecuteNonQuery(
                            "UPDATE expenses SET is_paid = CASE WHEN is_paid = 1 THEN 0 ELSE 1 END WHERE expense_id = @id",
                            new Microsoft.Data.Sqlite.SqliteParameter("@id", id));
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/expenses/settle-supplier", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        using var rdr = new System.IO.StreamReader(request.Body);
                        string json = await rdr.ReadToEndAsync();
                        int supplierId = 0;
                        string from = null;
                        string to = null;
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            using var doc = System.Text.Json.JsonDocument.Parse(json);
                            var root = doc.RootElement;
                            if (root.TryGetProperty("supplierId", out var sp)) supplierId = sp.GetInt32();
                            if (root.TryGetProperty("from", out var fp)) from = fp.GetString();
                            if (root.TryGetProperty("to", out var tp)) to = tp.GetString();
                        }

                        string sql = @"UPDATE expenses SET is_paid = 1 WHERE is_paid = 0 AND date_deleted IS NULL";
                        var parameters = new System.Collections.Generic.List<Microsoft.Data.Sqlite.SqliteParameter>();

                        if (supplierId > 0)
                        {
                            sql += " AND supplier_id = @sid";
                            parameters.Add(new Microsoft.Data.Sqlite.SqliteParameter("@sid", supplierId));
                        }
                        if (!string.IsNullOrWhiteSpace(from) && DateTime.TryParse(from, out var fDate))
                        {
                            sql += " AND date(expense_date) >= date(@f)";
                            parameters.Add(new Microsoft.Data.Sqlite.SqliteParameter("@f", fDate.ToString("yyyy-MM-dd")));
                        }
                        if (!string.IsNullOrWhiteSpace(to) && DateTime.TryParse(to, out var tDate))
                        {
                            sql += " AND date(expense_date) <= date(@t)";
                            parameters.Add(new Microsoft.Data.Sqlite.SqliteParameter("@t", tDate.ToString("yyyy-MM-dd")));
                        }

                        bool ok = DatabaseHelper.ExecuteNonQuery(sql, parameters.ToArray());
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = ok });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapGet("/api/reports/expenses-breakdown", (string preset, string from, string to) =>
                {
                    try
                    {
                        var svc = new ReportService();
                        DateTime fromDate, toDate;
                        if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to)
                            && DateTime.TryParse(from, out fromDate) && DateTime.TryParse(to, out toDate))
                        {
                            if (toDate < fromDate) (fromDate, toDate) = (toDate, fromDate);
                        }
                        else
                        {
                            (fromDate, toDate) = svc.GetPresetRange(string.IsNullOrWhiteSpace(preset) ? "Monthly" : preset);
                        }

                        string fStr = fromDate.ToString("yyyy-MM-dd");
                        string tStr = toDate.ToString("yyyy-MM-dd");

                        var dtSuppliers = DatabaseHelper.ExecuteDataTable(@"
                            SELECT 
                                COALESCE(e.supplier_id, 0) AS supplier_id,
                                COALESCE(s.supplier_name, 'Unassigned / Walk-in') AS supplier_name,
                                COUNT(*) AS delivery_count,
                                COALESCE(SUM(e.liters), 0) AS total_liters,
                                CASE WHEN SUM(e.liters) > 0 THEN SUM(e.amount) / SUM(e.liters) ELSE 0 END AS avg_unit_price,
                                COALESCE(SUM(e.amount), 0) AS total_amount,
                                COALESCE(SUM(CASE WHEN e.is_paid = 1 THEN e.amount ELSE 0 END), 0) AS total_paid,
                                COALESCE(SUM(CASE WHEN e.is_paid = 0 THEN e.amount ELSE 0 END), 0) AS total_unpaid
                            FROM expenses e
                            LEFT JOIN suppliers s ON s.id = e.supplier_id
                            WHERE date(e.expense_date) BETWEEN date(@from) AND date(@to)
                              AND e.date_deleted IS NULL
                              AND (e.category = 'Raw Milk' OR (e.liters IS NOT NULL AND e.liters > 0))
                            GROUP BY e.supplier_id, s.supplier_name
                            ORDER BY total_amount DESC",
                            new Microsoft.Data.Sqlite.SqliteParameter("@from", fStr),
                            new Microsoft.Data.Sqlite.SqliteParameter("@to", tStr));

                        var dtCategories = DatabaseHelper.ExecuteDataTable(@"
                            SELECT 
                                category,
                                COUNT(*) AS expense_count,
                                COALESCE(SUM(amount), 0) AS total_amount,
                                COALESCE(SUM(CASE WHEN is_paid = 1 THEN amount ELSE 0 END), 0) AS paid_amount,
                                COALESCE(SUM(CASE WHEN is_paid = 0 THEN amount ELSE 0 END), 0) AS unpaid_amount
                            FROM expenses
                            WHERE date(expense_date) BETWEEN date(@from) AND date(@to)
                              AND date_deleted IS NULL
                            GROUP BY category
                            ORDER BY total_amount DESC",
                            new Microsoft.Data.Sqlite.SqliteParameter("@from", fStr),
                            new Microsoft.Data.Sqlite.SqliteParameter("@to", tStr));

                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            fromDate = fromDate.ToString("yyyy-MM-dd"),
                            toDate = toDate.ToString("yyyy-MM-dd"),
                            milkSuppliers = DataTableToList(dtSuppliers),
                            categories = DataTableToList(dtCategories)
                        });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Milk processing (what raw milk was turned into) -
                app.MapGet("/api/milk/intakes", (string all) =>
                {
                    try
                    {
                        var svc = new MilkProductionService();
                        svc.SyncRawMilkStock();
                        return Microsoft.AspNetCore.Http.Results.Ok(DataTableToList(svc.GetMilkIntakes(all != "1")));
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapGet("/api/milk/productions", (int? expenseId) =>
                {
                    try { return Microsoft.AspNetCore.Http.Results.Ok(DataTableToList(new MilkProductionService().GetProductions(expenseId))); }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapGet("/api/milk/products", () =>
                {
                    try { return Microsoft.AspNetCore.Http.Results.Ok(DataTableToList(new MilkProductionService().GetProducibleProducts())); }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/milk/productions", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        using var doc = await System.Text.Json.JsonDocument.ParseAsync(request.Body);
                        var root = doc.RootElement;
                        int expenseId = root.GetProperty("expenseId").GetInt32();
                        int partId = root.GetProperty("partId").GetInt32();
                        decimal liters = root.GetProperty("litersUsed").GetDecimal();
                        decimal made = root.GetProperty("quantityMade").GetDecimal();
                        string notes = root.TryGetProperty("notes", out var n) && n.ValueKind == System.Text.Json.JsonValueKind.String ? n.GetString() : null;
                        string user = root.TryGetProperty("recordedBy", out var u) && u.ValueKind == System.Text.Json.JsonValueKind.String ? u.GetString() : null;

                        string error = new MilkProductionService().AddProduction(expenseId, partId, liters, made, notes, user);
                        return error == null
                            ? Microsoft.AspNetCore.Http.Results.Ok(new { success = true })
                            : Microsoft.AspNetCore.Http.Results.BadRequest(new { error });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/milk/productions/{id:int}/delete", (int id) =>
                {
                    try
                    {
                        return new MilkProductionService().DeleteProduction(id)
                            ? Microsoft.AspNetCore.Http.Results.Ok(new { success = true })
                            : Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Production not found" });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapGet("/api/expense-categories", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable(
                            "SELECT category_name FROM expense_categories ORDER BY category_name");
                        var list = new System.Collections.Generic.List<string>();
                        foreach (System.Data.DataRow row in dt.Rows)
                            list.Add(row["category_name"].ToString());
                        if (list.Count == 0)
                            list.AddRange(new[] { "Rent", "Utilities", "Salaries", "Supplies", "Other" });
                        return Microsoft.AspNetCore.Http.Results.Ok(list);
                    }
                    catch { return Microsoft.AspNetCore.Http.Results.Ok(new[] { "Rent", "Utilities", "Salaries", "Supplies", "Other" }); }
                });

                // - Units of measure (defaults + custom + used on products) -
                app.MapGet("/api/uoms", () =>
                {
                    try
                    {
                        var defaults = new[] { "pcs", "box", "pack", "meter", "liter", "g", "kg" };
                        var set = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var d in defaults) set.Add(d);

                        try
                        {
                            var custom = DatabaseHelper.ExecuteDataTable(
                                "SELECT unit_name FROM units_of_measure ORDER BY unit_name");
                            foreach (System.Data.DataRow row in custom.Rows)
                            {
                                string n = row["unit_name"]?.ToString()?.Trim();
                                if (!string.IsNullOrEmpty(n)) set.Add(n);
                            }
                        }
                        catch { }

                        try
                        {
                            var used = DatabaseHelper.ExecuteDataTable(
                                @"SELECT DISTINCT TRIM(unit_of_measure) AS uom
                                  FROM parts
                                  WHERE date_deleted IS NULL
                                    AND unit_of_measure IS NOT NULL
                                    AND TRIM(unit_of_measure) != ''
                                  ORDER BY uom");
                            foreach (System.Data.DataRow row in used.Rows)
                            {
                                string n = row["uom"]?.ToString()?.Trim();
                                if (!string.IsNullOrEmpty(n)) set.Add(n);
                            }
                        }
                        catch { }

                        var list = set.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
                        return Microsoft.AspNetCore.Http.Results.Ok(list);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/uoms", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        using var doc = await System.Text.Json.JsonDocument.ParseAsync(request.Body);
                        string name = doc.RootElement.TryGetProperty("name", out var n) ? n.GetString() : null;
                        if (string.IsNullOrWhiteSpace(name))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Name required" });

                        name = name.Trim();
                        if (name.Length > 32)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Unit name too long" });

                        int exists = DatabaseHelper.ExecuteScalar<int>(
                            "SELECT COUNT(*) FROM units_of_measure WHERE unit_name = @n COLLATE NOCASE",
                            new Microsoft.Data.Sqlite.SqliteParameter("@n", name));
                        if (exists == 0)
                            DatabaseHelper.ExecuteNonQuery(
                                "INSERT INTO units_of_measure (unit_name) VALUES (@n)",
                                new Microsoft.Data.Sqlite.SqliteParameter("@n", name));

                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, name });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Barcode items -
                app.MapGet("/api/barcode/items", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT id, part_name, part_number, selling_price, barcode, quantity_in_stock
                              FROM parts WHERE date_deleted IS NULL AND status = 'Active'
                              ORDER BY part_name");
                        var list = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            list.Add(new
                            {
                                id = Convert.ToInt32(row["id"]),
                                name = row["part_name"].ToString(),
                                sku = row["part_number"].ToString(),
                                price = Convert.ToDecimal(row["selling_price"]),
                                barcode = row["barcode"].ToString(),
                                stock = Convert.ToInt32(row["quantity_in_stock"])
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(list);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - License (display) -
                app.MapGet("/api/license", () =>
                {
                    try
                    {
                        var lic = LicenseManager.GetCurrentLicense();
                        string hwId = HardwareInfo.GetShortHardwareId();
                        if (lic == null)
                            return Microsoft.AspNetCore.Http.Results.Ok(new
                            {
                                isValid = false,
                                isTrial = false,
                                canActivate = true,
                                hardwareId = hwId
                            });

                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            licenseType = lic.LicenseType,
                            customerName = lic.CustomerName,
                            activationDate = lic.ActivationDate,
                            expirationDate = lic.ExpirationDate,
                            daysRemaining = lic.DaysRemaining(),
                            isValid = lic.IsValid(),
                            isTrial = lic.IsTrial(),
                            isActive = lic.IsActive,
                            productId = lic.ProductId,
                            machineName = lic.MachineName,
                            canActivate = lic.IsTrial() || !lic.IsValid(),
                            hardwareId = hwId,
                            keyMasked = string.IsNullOrEmpty(lic.Key) ? "" :
                                (lic.Key.Length <= 8 ? "****" : lic.Key.Substring(0, 5) + "-****-****-" + lic.Key.Substring(Math.Max(0, lic.Key.Length - 5)))
                        });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/license/activate", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<LicenseActivatePayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null || string.IsNullOrWhiteSpace(body.LicenseKey))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "License key required" });

                        string customer = string.IsNullOrWhiteSpace(body.CustomerName) ? "Licensed User" : body.CustomerName.Trim();
                        var license = LicenseManager.ActivateLicense(body.LicenseKey.Trim(), customer);
                        if (license == null)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Invalid license key" });

                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            success = true,
                            licenseType = license.LicenseType,
                            expirationDate = license.ExpirationDate,
                            daysRemaining = license.DaysRemaining()
                        });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/license/start-trial", () =>
                {
                    try
                    {
                        var existing = LicenseManager.GetCurrentLicense();
                        if (existing != null && existing.IsTrial())
                            return Microsoft.AspNetCore.Http.Results.Conflict(new { error = "Trial already used" });

                        var trial = LicenseManager.StartTrial();
                        if (trial == null)
                            return Microsoft.AspNetCore.Http.Results.Problem("Could not start trial");

                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            success = true,
                            daysRemaining = trial.DaysRemaining(),
                            expirationDate = trial.ExpirationDate
                        });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Product CRUD -
                app.MapPost("/api/products/{id:int}/delete", (int id) =>
                {
                    try { new InventoryService().DeletePart(id); return Microsoft.AspNetCore.Http.Results.Ok(new { success = true }); }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapPost("/api/products/{id:int}/update", async (int id, Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<ProductPayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null || string.IsNullOrWhiteSpace(body.Name))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Missing name" });
                        if (!string.IsNullOrEmpty(body.Barcode) && new InventoryService().BarcodeExists(body.Barcode, id))
                            return Microsoft.AspNetCore.Http.Results.Conflict(new { error = "Barcode already exists for another item." });
                        var part = MapProductPayload(body, id);
                        new InventoryService().SaveProductService(part);
                        if (body.SupplierPurchaseItemId.HasValue && body.SupplierPurchaseItemId.Value > 0)
                        {
                            try { new SupplierPurchaseService().LinkToPart(body.SupplierPurchaseItemId.Value, id); }
                            catch { }
                        }
                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Item '{body.Name}' updated");
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, barcode = part.Barcode });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapPost("/api/products/bulk-delete", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        using var doc = await System.Text.Json.JsonDocument.ParseAsync(request.Body);
                        if (!doc.RootElement.TryGetProperty("ids", out var idsEl) || idsEl.ValueKind != System.Text.Json.JsonValueKind.Array)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "ids required" });
                        var svc = new InventoryService();
                        int n = 0;
                        foreach (var el in idsEl.EnumerateArray())
                        {
                            if (el.TryGetInt32(out int id)) { svc.DeletePart(id); n++; }
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, count = n });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapPost("/api/categories", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        using var doc = await System.Text.Json.JsonDocument.ParseAsync(request.Body);
                        string name = doc.RootElement.TryGetProperty("name", out var n) ? n.GetString() : null;
                        if (string.IsNullOrWhiteSpace(name))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Name required" });
                        int exists = DatabaseHelper.ExecuteScalar<int>(
                            "SELECT COUNT(*) FROM categories WHERE category_name = @n",
                            new Microsoft.Data.Sqlite.SqliteParameter("@n", name.Trim()));
                        if (exists == 0)
                            DatabaseHelper.ExecuteNonQuery(
                                "INSERT INTO categories (category_name, description) VALUES (@n, '')",
                                new Microsoft.Data.Sqlite.SqliteParameter("@n", name.Trim()));
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, name = name.Trim() });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapPost("/api/categories/rename", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        using var doc = await System.Text.Json.JsonDocument.ParseAsync(request.Body);
                        string oldName = doc.RootElement.TryGetProperty("oldName", out var o) ? o.GetString() : null;
                        string newName = doc.RootElement.TryGetProperty("newName", out var n) ? n.GetString() : null;
                        if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "oldName and newName required" });
                        int exists = DatabaseHelper.ExecuteScalar<int>(
                            "SELECT COUNT(*) FROM categories WHERE category_name = @o",
                            new Microsoft.Data.Sqlite.SqliteParameter("@o", oldName.Trim()));
                        if (exists == 0) return Microsoft.AspNetCore.Http.Results.NotFound(new { error = "Category not found" });
                        DatabaseHelper.ExecuteNonQuery(
                            "UPDATE categories SET category_name = @n WHERE category_name = @o",
                            new Microsoft.Data.Sqlite.SqliteParameter("@n", newName.Trim()),
                            new Microsoft.Data.Sqlite.SqliteParameter("@o", oldName.Trim()));
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapPost("/api/categories/delete", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        using var doc = await System.Text.Json.JsonDocument.ParseAsync(request.Body);
                        string name = doc.RootElement.TryGetProperty("name", out var n) ? n.GetString() : null;
                        if (string.IsNullOrWhiteSpace(name))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Name required" });
                        int inUse = DatabaseHelper.ExecuteScalar<int>(
                            @"SELECT COUNT(*) FROM parts p JOIN categories c ON p.category_id = c.id
                              WHERE c.category_name = @n AND p.date_deleted IS NULL",
                            new Microsoft.Data.Sqlite.SqliteParameter("@n", name.Trim()));
                        if (inUse > 0)
                            return Microsoft.AspNetCore.Http.Results.Conflict(new { error = "Category has products" });
                        DatabaseHelper.ExecuteNonQuery(
                            "DELETE FROM categories WHERE category_name = @n",
                            new Microsoft.Data.Sqlite.SqliteParameter("@n", name.Trim()));
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/products/{id:int}/adjust-stock", async (int id, Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<StockAdjustPayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null || body.Change == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Change required" });
                        new InventoryService().AdjustStock(id, body.Change, body.Reason ?? "Web adjust");
                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Stock adjusted for #{id}");
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapGet("/api/customers/{id:int}/debt", (int id) =>
                {
                    try
                    {
                        var debt = new CustomerDebtService().GetDebtDetails(id);
                        return Microsoft.AspNetCore.Http.Results.Ok(debt);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/customers/{id:int}/payment", async (int id, Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<DebtPaymentPayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null || body.Amount == 0 && (body.Allocations == null || body.Allocations.Count == 0))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Amount required" });

                        var allocs = new System.Collections.Generic.List<CustomerDebtService.AllocationDto>();
                        if (body.Allocations != null)
                        {
                            foreach (var a in body.Allocations)
                            {
                                allocs.Add(new CustomerDebtService.AllocationDto
                                {
                                    OrderId = a.OrderId,
                                    OrderItemId = a.OrderItemId,
                                    Amount = a.Amount
                                });
                            }
                        }

                        decimal amount = body.Amount;
                        if (amount <= 0 && allocs.Count > 0)
                            amount = allocs.Sum(x => x.Amount);

                        decimal applied = new CustomerDebtService().ApplyPayment(id, amount, body.Note, allocs);
                        DatabaseHelper.LogTransaction("CUSTOMER_PAYMENT", id.ToString(), $"Debt payment {applied:F2}: {body.Note}");
                        _ = InventoryBroadcaster.Broadcast("CustomersChanged", $"Payment for customer #{id}");
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, applied });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/suppliers/{id:int}/payment", async (int id, Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<PaymentPayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null || body.Amount == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Amount required" });
                        DatabaseHelper.ExecuteNonQuery(
                            "UPDATE suppliers SET balance_due = balance_due - @amt WHERE id = @id",
                            new Microsoft.Data.Sqlite.SqliteParameter("@amt", Math.Abs(body.Amount)),
                            new Microsoft.Data.Sqlite.SqliteParameter("@id", id));
                        DatabaseHelper.LogTransaction("SUPPLIER_PAYMENT", id.ToString(), $"Paid supplier: {body.Amount:F2} {body.Note}");
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Supplier purchase / debt products -
                app.MapGet("/api/suppliers/{id:int}/purchases", (int id, string unadded) =>
                {
                    try
                    {
                        bool onlyUnadded = string.Equals(unadded, "1", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(unadded, "true", StringComparison.OrdinalIgnoreCase);
                        var list = new SupplierPurchaseService().ListForSupplier(id, onlyUnadded);
                        return Microsoft.AspNetCore.Http.Results.Ok(list);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/suppliers/{id:int}/purchases", async (int id, Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<SupplierPurchasePayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null || string.IsNullOrWhiteSpace(body.Name))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Name required" });
                        bool isPaid = body.IsPaid || string.Equals(body.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase);
                        var item = new SupplierPurchaseService().AddItem(id, body.Name, body.Category, body.Quantity, body.UnitPrice, isPaid, body.Notes);
                        _ = InventoryBroadcaster.Broadcast("SuppliersChanged", $"Purchase for supplier #{id}");
                        return Microsoft.AspNetCore.Http.Results.Ok(item);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/supplier-purchases/{itemId:int}/pay", async (int itemId, Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        decimal? amt = null;
                        try
                        {
                            var body = await System.Text.Json.JsonSerializer.DeserializeAsync<PaymentPayload>(
                                request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (body != null && body.Amount > 0) amt = body.Amount;
                        }
                        catch { }
                        var item = new SupplierPurchaseService().UpdatePayment(itemId, markPaid: true, payAmount: amt);
                        _ = InventoryBroadcaster.Broadcast("SuppliersChanged", $"Paid supplier purchase #{itemId}");
                        return Microsoft.AspNetCore.Http.Results.Ok(item);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/supplier-purchases/{itemId:int}/debt", (int itemId) =>
                {
                    try
                    {
                        var item = new SupplierPurchaseService().UpdatePayment(itemId, markPaid: false);
                        _ = InventoryBroadcaster.Broadcast("SuppliersChanged", $"Debt supplier purchase #{itemId}");
                        return Microsoft.AspNetCore.Http.Results.Ok(item);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/supplier-purchases/{itemId:int}/delete", (int itemId) =>
                {
                    try
                    {
                        new SupplierPurchaseService().DeleteItem(itemId);
                        _ = InventoryBroadcaster.Broadcast("SuppliersChanged", $"Deleted supplier purchase #{itemId}");
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/supplier-purchases/{itemId:int}/import-to-inventory", (int itemId) =>
                {
                    try
                    {
                        var result = new SupplierPurchaseService().ImportToInventory(itemId);
                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", result.Message);
                        _ = InventoryBroadcaster.Broadcast("SuppliersChanged", $"Imported purchase #{itemId}");
                        return Microsoft.AspNetCore.Http.Results.Ok(result);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/dev/seed-demo", () =>
                {
                    try
                    {
                        var summary = DatabaseHelper.SeedDemoData(force: true);
                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", "Demo seed");
                        _ = InventoryBroadcaster.Broadcast("SuppliersChanged", "Demo seed");
                        _ = InventoryBroadcaster.Broadcast("CustomersChanged", "Demo seed");
                        return Microsoft.AspNetCore.Http.Results.Ok(summary);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/expense-categories", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        using var doc = await System.Text.Json.JsonDocument.ParseAsync(request.Body);
                        string name = doc.RootElement.TryGetProperty("name", out var n) ? n.GetString() : null;
                        if (string.IsNullOrWhiteSpace(name))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Name required" });
                        int exists = DatabaseHelper.ExecuteScalar<int>(
                            "SELECT COUNT(*) FROM expense_categories WHERE category_name = @n",
                            new Microsoft.Data.Sqlite.SqliteParameter("@n", name.Trim()));
                        if (exists == 0)
                            DatabaseHelper.ExecuteNonQuery(
                                "INSERT INTO expense_categories (category_name) VALUES (@n)",
                                new Microsoft.Data.Sqlite.SqliteParameter("@n", name.Trim()));
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapPost("/api/expense-categories/delete", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        using var doc = await System.Text.Json.JsonDocument.ParseAsync(request.Body);
                        string name = doc.RootElement.TryGetProperty("name", out var n) ? n.GetString() : null;
                        if (string.IsNullOrWhiteSpace(name))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Name required" });
                        DatabaseHelper.ExecuteNonQuery(
                            "DELETE FROM expense_categories WHERE category_name = @n",
                            new Microsoft.Data.Sqlite.SqliteParameter("@n", name.Trim()));
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Customer CRUD -
                app.MapPost("/api/customers", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<PartyPayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null || string.IsNullOrWhiteSpace(body.Name))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Name required" });
                        int id = new CustomerService().AddCustomer(body.Name.Trim(), body.Phone ?? "", body.Email ?? "", body.Address ?? "", body.Type ?? "Regular", body.CreditLimit, body.DueDate, body.ReminderDays);
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, id });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapPost("/api/customers/{id:int}/update", async (int id, Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<PartyPayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null || string.IsNullOrWhiteSpace(body.Name))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Name required" });
                        new CustomerService().UpdateCustomer(id, body.Name.Trim(), body.Phone ?? "", body.Email ?? "", body.Address ?? "", body.Type ?? "Regular", body.CreditLimit, body.DueDate, body.ReminderDays);
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapPost("/api/customers/{id:int}/delete", (int id) =>
                {
                    try { new CustomerService().DeleteCustomer(id); return Microsoft.AspNetCore.Http.Results.Ok(new { success = true }); }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Supplier CRUD -
                app.MapPost("/api/suppliers", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<PartyPayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null || string.IsNullOrWhiteSpace(body.Name))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Name required" });
                        new SupplierService().AddSupplier(body.Name.Trim(), body.Phone ?? "", body.Email ?? "", body.Address ?? "", body.Type ?? "Regular", body.DueDate, body.ReminderDays);
                        long newId = Convert.ToInt64(DatabaseHelper.ExecuteScalar<object>("SELECT last_insert_rowid()") ?? 0);
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, id = newId });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapPost("/api/suppliers/{id:int}/update", async (int id, Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<PartyPayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null || string.IsNullOrWhiteSpace(body.Name))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Name required" });
                        new SupplierService().UpdateSupplier(id, body.Name.Trim(), body.Phone ?? "", body.Email ?? "", body.Address ?? "", body.Type ?? "Regular", body.DueDate, body.ReminderDays);
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapPost("/api/suppliers/{id:int}/delete", (int id) =>
                {
                    try { new SupplierService().DeleteSupplier(id); return Microsoft.AspNetCore.Http.Results.Ok(new { success = true }); }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Expense delete -
                app.MapPost("/api/expenses/{id:int}/delete", (int id) =>
                {
                    try
                    {
                        var milk = new MilkProductionService();
                        if (milk.HasProductions(id))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "This milk was already used in Milk Processing. Delete those productions first." });
                        DatabaseHelper.ExecuteNonQuery("DELETE FROM expenses WHERE expense_id = @id",
                            new Microsoft.Data.Sqlite.SqliteParameter("@id", id));
                        milk.SyncRawMilkStock();
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Quotation delete -
                app.MapPost("/api/quotations/{id:int}/delete", (int id) =>
                {
                    try { new OrderService().DeleteOrder(id); return Microsoft.AspNetCore.Http.Results.Ok(new { success = true }); }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Bulk CSV import (products, customers, suppliers, expenses) -
                app.MapGet("/api/products/export", (string format, string includeInactive) =>
                {
                    try
                    {
                        bool showInactive = string.Equals(includeInactive, "1", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(includeInactive, "true", StringComparison.OrdinalIgnoreCase);
                        var dt = CsvImportExportService.BuildProductsExportTable(showInactive);
                        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
                        {
                            var bytes = ImportExportHelper.ExportDataTableToCsvBytes(dt);
                            return Microsoft.AspNetCore.Http.Results.File(bytes, "text/csv", $"inventory_{stamp}.csv");
                        }
                        var xlsx = ImportExportHelper.ExportDataTableToXlsx(dt, "Inventory");
                        return Microsoft.AspNetCore.Http.Results.File(xlsx,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"inventory_{stamp}.xlsx");
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapGet("/api/customers/export", (string format) =>
                {
                    try
                    {
                        var dt = CsvImportExportService.BuildCustomersExportTable();
                        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
                        {
                            var bytes = ImportExportHelper.ExportDataTableToCsvBytes(dt);
                            return Microsoft.AspNetCore.Http.Results.File(bytes, "text/csv", $"customers_{stamp}.csv");
                        }
                        var xlsx = ImportExportHelper.ExportDataTableToXlsx(dt, "Customers");
                        return Microsoft.AspNetCore.Http.Results.File(xlsx,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"customers_{stamp}.xlsx");
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapGet("/api/suppliers/export", (string format) =>
                {
                    try
                    {
                        var dt = CsvImportExportService.BuildSuppliersExportTable();
                        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
                        {
                            var bytes = ImportExportHelper.ExportDataTableToCsvBytes(dt);
                            return Microsoft.AspNetCore.Http.Results.File(bytes, "text/csv", $"suppliers_{stamp}.csv");
                        }
                        var xlsx = ImportExportHelper.ExportDataTableToXlsx(dt, "Suppliers");
                        return Microsoft.AspNetCore.Http.Results.File(xlsx,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"suppliers_{stamp}.xlsx");
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapGet("/api/expenses/export", (string format) =>
                {
                    try
                    {
                        var dt = CsvImportExportService.BuildExpensesExportTable();
                        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
                        {
                            var bytes = ImportExportHelper.ExportDataTableToCsvBytes(dt);
                            return Microsoft.AspNetCore.Http.Results.File(bytes, "text/csv", $"expenses_{stamp}.csv");
                        }
                        var xlsx = ImportExportHelper.ExportDataTableToXlsx(dt, "Expenses");
                        return Microsoft.AspNetCore.Http.Results.File(xlsx,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"expenses_{stamp}.xlsx");
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapGet("/api/sales/export", (string format) =>
                {
                    try
                    {
                        var dt = CsvImportExportService.BuildSalesExportTable();
                        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
                        {
                            var bytes = ImportExportHelper.ExportDataTableToCsvBytes(dt);
                            return Microsoft.AspNetCore.Http.Results.File(bytes, "text/csv", $"sales_{stamp}.csv");
                        }
                        var xlsx = ImportExportHelper.ExportDataTableToXlsx(dt, "Sales");
                        return Microsoft.AspNetCore.Http.Results.File(xlsx,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"sales_{stamp}.xlsx");
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapGet("/api/history/export", (string format) =>
                {
                    try
                    {
                        var dt = CsvImportExportService.BuildHistoryExportTable();
                        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
                        {
                            var bytes = ImportExportHelper.ExportDataTableToCsvBytes(dt);
                            return Microsoft.AspNetCore.Http.Results.File(bytes, "text/csv", $"history_{stamp}.csv");
                        }
                        var xlsx = ImportExportHelper.ExportDataTableToXlsx(dt, "History");
                        return Microsoft.AspNetCore.Http.Results.File(xlsx,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"history_{stamp}.xlsx");
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // Reports export ships two sheets: the headline metrics and the sold-products detail.
                app.MapGet("/api/reports/export", (string format, string from, string to) =>
                {
                    try
                    {
                        var svc = new ReportService();
                        DateTime fromDate = DateTime.TryParse(from, out var f) ? f.Date : DateTime.Today;
                        DateTime toDate = DateTime.TryParse(to, out var t) ? t.Date : DateTime.Today;
                        if (toDate < fromDate) (fromDate, toDate) = (toDate, fromDate);

                        var summary = svc.GetSummary(fromDate, toDate);
                        var summaryTable = CsvImportExportService.BuildReportSummaryExportTable(summary);
                        var productsTable = CsvImportExportService.BuildReportProductsExportTable(fromDate, toDate);

                        string stamp = $"{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}";
                        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
                        {
                            var bytes = ImportExportHelper.ExportDataTableToCsvBytes(productsTable);
                            return Microsoft.AspNetCore.Http.Results.File(bytes, "text/csv", $"report_{stamp}.csv");
                        }

                        var xlsx = ImportExportHelper.ExportTablesToXlsx(new[]
                        {
                            ("Summary", summaryTable),
                            ("Sold Products", productsTable)
                        });
                        return Microsoft.AspNetCore.Http.Results.File(xlsx,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"report_{stamp}.xlsx");
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/products/import", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var payload = await System.Text.Json.JsonSerializer.DeserializeAsync<BulkCsvImportPayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload?.Rows == null || payload.Rows.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "No rows" });

                        var result = CsvImportExportService.ImportProducts(payload.Rows);
                        if (result.Imported + result.Updated > 0)
                            _ = InventoryBroadcaster.Broadcast("InventoryChanged", "CSV import completed");
                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            success = true,
                            imported = result.Imported,
                            updated = result.Updated,
                            skipped = result.Skipped,
                            errors = result.Errors.Take(20).ToList()
                        });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/customers/import", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var payload = await System.Text.Json.JsonSerializer.DeserializeAsync<BulkCsvImportPayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload?.Rows == null || payload.Rows.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "No rows" });

                        var result = CsvImportExportService.ImportCustomers(payload.Rows);
                        if (result.Imported + result.Updated > 0)
                            GlobalEvents.RaiseCustomersUpdated();
                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            success = true,
                            imported = result.Imported,
                            updated = result.Updated,
                            skipped = result.Skipped,
                            errors = result.Errors.Take(20).ToList()
                        });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/suppliers/import", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var payload = await System.Text.Json.JsonSerializer.DeserializeAsync<BulkCsvImportPayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload?.Rows == null || payload.Rows.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "No rows" });

                        var result = CsvImportExportService.ImportSuppliers(payload.Rows);
                        if (result.Imported + result.Updated > 0)
                            GlobalEvents.RaiseSuppliersUpdated();
                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            success = true,
                            imported = result.Imported,
                            updated = result.Updated,
                            skipped = result.Skipped,
                            errors = result.Errors.Take(20).ToList()
                        });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/expenses/import", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var payload = await System.Text.Json.JsonSerializer.DeserializeAsync<BulkCsvImportPayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload?.Rows == null || payload.Rows.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "No rows" });

                        string user = payload.RecordedBy ?? "Web";
                        var result = CsvImportExportService.ImportExpenses(payload.Rows, user);
                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            success = true,
                            imported = result.Imported,
                            updated = result.Updated,
                            skipped = result.Skipped,
                            errors = result.Errors.Take(20).ToList()
                        });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Sales / History / Reports CSV import -
                app.MapPost("/api/sales/import", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var rows = await System.Text.Json.JsonSerializer.DeserializeAsync<System.Collections.Generic.List<SaleImportRow>>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (rows == null || rows.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "No rows" });

                        int imported = 0;
                        foreach (var row in rows)
                        {
                            if (row == null || row.Total <= 0) continue;
                            if (ImportHistoricalSale(row)) imported++;
                        }
                        if (imported > 0) GlobalEvents.RaiseOrdersUpdated();
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, imported });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/history/import", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var rows = await System.Text.Json.JsonSerializer.DeserializeAsync<System.Collections.Generic.List<HistoryImportRow>>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (rows == null || rows.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "No rows" });

                        int imported = 0;
                        int salesImported = 0;
                        foreach (var row in rows)
                        {
                            if (row == null) continue;

                            // Order-shaped history export → create completed sales
                            if (row.Total > 0 && (!string.IsNullOrWhiteSpace(row.Customer) || !string.IsNullOrWhiteSpace(row.Date)))
                            {
                                if (ImportHistoricalSale(new SaleImportRow
                                {
                                    Customer = row.Customer,
                                    Total = row.Total,
                                    Date = row.Date,
                                    Payment = row.Payment ?? row.Status
                                }))
                                {
                                    salesImported++;
                                    imported++;
                                }
                                continue;
                            }

                            string action = string.IsNullOrWhiteSpace(row.Action) ? "IMPORT" : row.Action.Trim();
                            string item = string.IsNullOrWhiteSpace(row.Item) ? (row.Customer ?? "Import") : row.Item.Trim();
                            string details = row.Details ?? row.Description ?? "";
                            string user = string.IsNullOrWhiteSpace(row.User) ? "Import" : row.User.Trim();
                            string ts = NormalizeImportDate(row.Date) ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                            DatabaseHelper.ExecuteNonQuery(
                                @"INSERT INTO transactions (action_type, part_name, description, username, timestamp)
                                  VALUES (@a, @p, @d, @u, @t)",
                                new Microsoft.Data.Sqlite.SqliteParameter("@a", action),
                                new Microsoft.Data.Sqlite.SqliteParameter("@p", item),
                                new Microsoft.Data.Sqlite.SqliteParameter("@d", details),
                                new Microsoft.Data.Sqlite.SqliteParameter("@u", user),
                                new Microsoft.Data.Sqlite.SqliteParameter("@t", ts));
                            imported++;
                        }
                        if (salesImported > 0) GlobalEvents.RaiseOrdersUpdated();
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, imported, salesImported });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/reports/import", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var rows = await System.Text.Json.JsonSerializer.DeserializeAsync<System.Collections.Generic.List<ReportImportRow>>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (rows == null || rows.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "No rows" });

                        int imported = 0;
                        int salesImported = 0;
                        foreach (var row in rows)
                        {
                            if (row == null) continue;

                            // Top-products export → historical sale per product line
                            decimal salesAmt = row.Sales > 0 ? row.Sales : row.Total;
                            if (!string.IsNullOrWhiteSpace(row.Name) && salesAmt > 0)
                            {
                                if (ImportHistoricalSale(new SaleImportRow
                                {
                                    Customer = "Walk-in",
                                    Total = salesAmt,
                                    Date = row.Date,
                                    Payment = "Paid"
                                }))
                                {
                                    DatabaseHelper.LogTransaction("REPORT_IMPORT", row.Name.Trim(),
                                        $"Imported report line qty={row.Qty}, sales={salesAmt}, profit={row.Profit}");
                                    salesImported++;
                                    imported++;
                                }
                                continue;
                            }

                            // Summary metric/value export → audit log entry
                            if (!string.IsNullOrWhiteSpace(row.Metric))
                            {
                                DatabaseHelper.ExecuteNonQuery(
                                    @"INSERT INTO transactions (action_type, part_name, description, username, timestamp)
                                      VALUES ('REPORT_IMPORT', @m, @v, 'Import', datetime('now'))",
                                    new Microsoft.Data.Sqlite.SqliteParameter("@m", row.Metric.Trim()),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@v", row.Value ?? ""));
                                imported++;
                            }
                        }
                        if (salesImported > 0) GlobalEvents.RaiseOrdersUpdated();
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, imported, salesImported });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Users CRUD -
                app.MapPost("/api/users", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<UserPayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null || string.IsNullOrWhiteSpace(body.Username) || string.IsNullOrWhiteSpace(body.Password))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Username and password required" });
                        DatabaseHelper.ExecuteNonQuery(
                            "INSERT INTO users (username, password, full_name, role, date_created) VALUES (@u, @p, @n, @r, datetime('now'))",
                            new Microsoft.Data.Sqlite.SqliteParameter("@u", body.Username.Trim()),
                            new Microsoft.Data.Sqlite.SqliteParameter("@p", body.Password),
                            new Microsoft.Data.Sqlite.SqliteParameter("@n", body.FullName ?? body.Username.Trim()),
                            new Microsoft.Data.Sqlite.SqliteParameter("@r", body.Role ?? "Staff"));
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapPost("/api/users/{id:int}/update", async (int id, Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        string existingName = DatabaseHelper.ExecuteScalar<string>(
                            "SELECT username FROM users WHERE id=@id",
                            new Microsoft.Data.Sqlite.SqliteParameter("@id", id)) ?? "";
                        if (DatabaseInitializer.IsProtectedSuperAdmin(existingName))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Cannot modify Super Admin user" });

                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<UserPayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null || string.IsNullOrWhiteSpace(body.Username))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Username required" });
                        if (DatabaseInitializer.IsProtectedSuperAdmin(body.Username))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Cannot use reserved Super Admin username" });
                        if (!string.IsNullOrWhiteSpace(body.Password))
                            DatabaseHelper.ExecuteNonQuery(
                                "UPDATE users SET username=@u, password=@p, full_name=@n, role=@r WHERE id=@id",
                                new Microsoft.Data.Sqlite.SqliteParameter("@u", body.Username.Trim()),
                                new Microsoft.Data.Sqlite.SqliteParameter("@p", body.Password),
                                new Microsoft.Data.Sqlite.SqliteParameter("@n", body.FullName ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@r", body.Role ?? "Staff"),
                                new Microsoft.Data.Sqlite.SqliteParameter("@id", id));
                        else
                            DatabaseHelper.ExecuteNonQuery(
                                "UPDATE users SET username=@u, full_name=@n, role=@r WHERE id=@id",
                                new Microsoft.Data.Sqlite.SqliteParameter("@u", body.Username.Trim()),
                                new Microsoft.Data.Sqlite.SqliteParameter("@n", body.FullName ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@r", body.Role ?? "Staff"),
                                new Microsoft.Data.Sqlite.SqliteParameter("@id", id));
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapPost("/api/users/{id:int}/delete", (int id) =>
                {
                    try
                    {
                        string uname = DatabaseHelper.ExecuteScalar<string>("SELECT username FROM users WHERE id=@id",
                            new Microsoft.Data.Sqlite.SqliteParameter("@id", id)) ?? "";
                        if (DatabaseInitializer.IsProtectedSuperAdmin(uname)
                            || uname.Equals("admin", StringComparison.OrdinalIgnoreCase))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Cannot delete Super Admin user" });
                        DatabaseHelper.ExecuteNonQuery("DELETE FROM users WHERE id=@id",
                            new Microsoft.Data.Sqlite.SqliteParameter("@id", id));
                        // Softio Super Admin must always remain after any user deletion
                        DatabaseInitializer.EnsureSoftioSuperAdmin();
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Currencies -
                app.MapPost("/api/currencies/refresh", async () =>
                {
                    try
                    {
                        var rates = await CurrencyService.FetchLiveRatesAsync();
                        if (rates == null || rates.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.Problem("Could not fetch rates");
                        CurrencyService.SaveRatesToDb(rates);
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, count = rates.Count });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapPost("/api/currencies", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        using var doc = await System.Text.Json.JsonDocument.ParseAsync(request.Body);
                        string code = doc.RootElement.TryGetProperty("code", out var c) ? c.GetString() : null;
                        string name = doc.RootElement.TryGetProperty("name", out var n) ? n.GetString() : code;
                        string symbol = doc.RootElement.TryGetProperty("symbol", out var s) ? s.GetString() : code;
                        decimal rate = doc.RootElement.TryGetProperty("rate", out var r) ? r.GetDecimal() : 1m;
                        if (string.IsNullOrWhiteSpace(code))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Code required" });
                        DatabaseHelper.ExecuteNonQuery(
                            "INSERT OR REPLACE INTO currency_rates (code, name, symbol, rate_vs_usd, last_updated) VALUES (@c,@n,@s,@r,datetime('now'))",
                            new Microsoft.Data.Sqlite.SqliteParameter("@c", code.Trim().ToUpperInvariant()),
                            new Microsoft.Data.Sqlite.SqliteParameter("@n", name ?? code),
                            new Microsoft.Data.Sqlite.SqliteParameter("@s", symbol ?? code),
                            new Microsoft.Data.Sqlite.SqliteParameter("@r", rate));
                        CurrencyService.LoadRatesFromDb();
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });
                app.MapPost("/api/currencies/{code}/delete", (string code) =>
                {
                    try
                    {
                        if (string.Equals(code, "USD", StringComparison.OrdinalIgnoreCase))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Cannot delete base currency" });
                        DatabaseHelper.ExecuteNonQuery("DELETE FROM currency_rates WHERE code=@c",
                            new Microsoft.Data.Sqlite.SqliteParameter("@c", code));
                        CurrencyService.LoadRatesFromDb();
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Notifications -
                app.MapGet("/api/notifications", (string lang) =>
                {
                    try
                    {
                        var list = new DashboardService().GetNotifications(lang);
                        return Microsoft.AspNetCore.Http.Results.Ok(list.Select(n => new
                        {
                            title = n.Title,
                            message = n.Message,
                            type = n.Type,
                            target = n.Target,
                            timestamp = n.Timestamp
                        }));
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Backup -
                app.MapGet("/api/backup/status", () =>
                {
                    try
                    {
                        string dir = BackupService.GetBackupDirectory();
                        string tsFile = System.IO.Path.Combine(dir, "last_backup.txt");
                        string last = null;
                        if (System.IO.File.Exists(tsFile))
                            last = System.IO.File.ReadAllText(tsFile).Trim();
                        var files = System.IO.Directory.GetFiles(dir, "backup_*")
                            .OrderByDescending(f => f)
                            .Take(10)
                            .Select(f => new { name = System.IO.Path.GetFileName(f), path = f, modified = System.IO.File.GetLastWriteTime(f) });
                        string schedule = BackupService.GetSchedule();
                        DateTime? lastAuto = BackupService.GetLastAutoRun();
                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            lastBackup = last,
                            folder = dir,
                            defaultFolder = BackupService.GetDefaultBackupDirectory(),
                            customFolder = BackupService.HasCustomFolder(),
                            files,
                            autoSchedule = schedule,
                            lastAutoBackup = lastAuto?.ToString("o")
                        });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPut("/api/backup/auto", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        using var doc = await System.Text.Json.JsonDocument.ParseAsync(request.Body);
                        string schedule = doc.RootElement.TryGetProperty("schedule", out var s)
                            ? s.GetString()
                            : "off";
                        BackupService.SetSchedule(schedule);
                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            autoSchedule = BackupService.GetSchedule(),
                            lastAutoBackup = BackupService.GetLastAutoRun()?.ToString("o")
                        });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/backup/choose-folder", () =>
                {
                    try
                    {
                        string folder = BackupService.PickBackupDirectory();
                        if (string.IsNullOrWhiteSpace(folder))
                            return Microsoft.AspNetCore.Http.Results.Ok(new
                            {
                                cancelled = true,
                                folder = BackupService.GetBackupDirectory()
                            });
                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            cancelled = false,
                            folder,
                            customFolder = BackupService.HasCustomFolder()
                        });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/backup/reset-folder", () =>
                {
                    try
                    {
                        string folder = BackupService.SetBackupDirectory(null);
                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            folder,
                            customFolder = false,
                            defaultFolder = BackupService.GetDefaultBackupDirectory()
                        });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/backup/create", () =>
                {
                    try
                    {
                        string dest = BackupService.CreateBackup();
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, file = dest });
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        return Microsoft.AspNetCore.Http.Results.NotFound(new { error = "Database not found" });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/backup/open-folder", () =>
                {
                    try
                    {
                        string dir = BackupService.GetBackupDirectory();
                        System.IO.Directory.CreateDirectory(dir);
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            Arguments = dir,
                            UseShellExecute = true
                        });
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/backup/restore", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        using var doc = await System.Text.Json.JsonDocument.ParseAsync(request.Body);
                        string fileName = doc.RootElement.TryGetProperty("fileName", out var f) ? f.GetString() : null;
                        if (string.IsNullOrWhiteSpace(fileName))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "fileName required" });
                        string safe = System.IO.Path.GetFileName(fileName);
                        if (!safe.StartsWith("backup_", StringComparison.OrdinalIgnoreCase))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Invalid backup file" });
                        string src = System.IO.Path.Combine(BackupService.GetBackupDirectory(), safe);
                        if (!System.IO.File.Exists(src))
                            return Microsoft.AspNetCore.Http.Results.NotFound(new { error = "Backup not found" });
                        string dbFile = DatabaseConfig.DatabasePath;
                        string safety = System.IO.Path.Combine(BackupService.GetBackupDirectory(), $"pre_restore_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                        if (System.IO.File.Exists(dbFile))
                            System.IO.File.Copy(dbFile, safety, true);
                        System.IO.File.Copy(src, dbFile, true);
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, restored = safe, safetyCopy = safety });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapGet("/api/backup/export", () =>
                {
                    try
                    {
                        string dir = BackupService.GetBackupDirectory();
                        System.IO.Directory.CreateDirectory(dir);
                        string dbFile = DatabaseConfig.DatabasePath;
                        if (!System.IO.File.Exists(dbFile))
                            return Microsoft.AspNetCore.Http.Results.NotFound(new { error = "Database not found" });
                        string name = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                        string dest = System.IO.Path.Combine(dir, name);
                        System.IO.File.Copy(dbFile, dest, true);
                        System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "last_backup.txt"), DateTime.Now.ToString("o"));
                        var bytes = System.IO.File.ReadAllBytes(dest);
                        return Microsoft.AspNetCore.Http.Results.File(bytes, "application/octet-stream", name);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/backup/import", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        if (!request.HasFormContentType)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Expected multipart form" });
                        var form = await request.ReadFormAsync();
                        var file = form.Files.FirstOrDefault();
                        if (file == null || file.Length == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "No file uploaded" });
                        string dir = BackupService.GetBackupDirectory();
                        System.IO.Directory.CreateDirectory(dir);
                        string name = $"backup_import_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                        string dest = System.IO.Path.Combine(dir, name);
                        await using (var fs = System.IO.File.Create(dest))
                            await file.CopyToAsync(fs);
                        string dbFile = DatabaseConfig.DatabasePath;
                        string safety = System.IO.Path.Combine(dir, $"pre_import_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                        if (System.IO.File.Exists(dbFile))
                            System.IO.File.Copy(dbFile, safety, true);
                        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                        System.IO.File.Copy(dest, dbFile, true);
                        System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "last_backup.txt"), DateTime.Now.ToString("o"));
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, file = name, safetyCopy = safety });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapPost("/api/backup/factory-reset", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<LoginPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null || string.IsNullOrWhiteSpace(body.Username) || string.IsNullOrWhiteSpace(body.Password))
                            return Microsoft.AspNetCore.Http.Results.Json(
                                new { error = "Admin credentials required." },
                                statusCode: 403);

                        bool isAdmin = body.Username.Trim().Equals("Softio.Admin", StringComparison.OrdinalIgnoreCase)
                            && body.Password == "Softio@2026!";
                        if (!isAdmin)
                        {
                            var dt = DatabaseHelper.ExecuteDataTable(
                                "SELECT role FROM users WHERE username = @u AND password = @p",
                                new Microsoft.Data.Sqlite.SqliteParameter("@u", body.Username.Trim()),
                                new Microsoft.Data.Sqlite.SqliteParameter("@p", body.Password));
                            if (dt.Rows.Count == 0)
                                return Microsoft.AspNetCore.Http.Results.Json(
                                    new { error = "Invalid admin credentials." },
                                    statusCode: 403);
                            string role = dt.Rows[0]["role"]?.ToString() ?? "";
                            isAdmin = role.Contains("Admin", StringComparison.OrdinalIgnoreCase);
                        }
                        if (!isAdmin)
                            return Microsoft.AspNetCore.Http.Results.Json(
                                new { error = "Only administrators can perform a factory reset." },
                                statusCode: 403);

                        string dir = BackupService.GetBackupDirectory();
                        System.IO.Directory.CreateDirectory(dir);
                        string dbFile = DatabaseConfig.DatabasePath;
                        if (System.IO.File.Exists(dbFile))
                        {
                            string safety = System.IO.Path.Combine(dir, $"pre_factory_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                            System.IO.File.Copy(dbFile, safety, true);
                        }
                        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                        if (System.IO.File.Exists(dbFile))
                            System.IO.File.Delete(dbFile);
                        string dataDir = System.IO.Path.GetDirectoryName(dbFile);
                        if (!string.IsNullOrEmpty(dataDir))
                            System.IO.Directory.CreateDirectory(dataDir);
                        // Rebuild empty schema and always re-create Softio Super Admin
                        DatabaseInitializer.Initialize();
                        DatabaseInitializer.EnsureSoftioSuperAdmin();
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Unlock / verify password -
                app.MapPost("/api/verify-password", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<LoginPayload>(
                            request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null || string.IsNullOrEmpty(body.Username) || string.IsNullOrEmpty(body.Password))
                            return Microsoft.AspNetCore.Http.Results.BadRequest();

                        if (body.Username == "Softio.Admin" && body.Password == "Softio@2026!")
                            return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });

                        int count = DatabaseHelper.ExecuteScalar<int>(
                            "SELECT COUNT(*) FROM users WHERE username = @u AND password = @p",
                            new Microsoft.Data.Sqlite.SqliteParameter("@u", body.Username),
                            new Microsoft.Data.Sqlite.SqliteParameter("@p", body.Password));
                        return count > 0
                            ? Microsoft.AspNetCore.Http.Results.Ok(new { success = true })
                            : Microsoft.AspNetCore.Http.Results.Unauthorized();
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // Scale API (TM-A17 / COM) — gated by Softio feature flag at runtime
                ScaleApiBootstrap.MapEndpoints(app);

                // Feature flags (scale / quick sale — Softio Super Admin only for writes)
                // - Sidebar order (everyone reads it; only Softio Super Admin may change it) -
                app.MapGet("/api/settings/nav-order", () =>
                {
                    try
                    {
                        string json = DatabaseHelper.ExecuteScalar<string>("SELECT value FROM app_settings WHERE key = 'nav_order'");
                        var list = string.IsNullOrWhiteSpace(json)
                            ? new System.Collections.Generic.List<string>()
                            : System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(json);
                        return Microsoft.AspNetCore.Http.Results.Ok(list);
                    }
                    catch { return Microsoft.AspNetCore.Http.Results.Ok(new string[0]); }
                });

                app.MapPut("/api/settings/nav-order", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        using var doc = await System.Text.Json.JsonDocument.ParseAsync(request.Body);
                        var root = doc.RootElement;
                        string user = root.TryGetProperty("username", out var u) && u.ValueKind == System.Text.Json.JsonValueKind.String ? u.GetString() : "";
                        if (!string.Equals(user?.Trim(), "Softio.Admin", StringComparison.OrdinalIgnoreCase))
                            return Microsoft.AspNetCore.Http.Results.Json(
                                new { error = "Only Softio Super Admin can change this setting." }, statusCode: 403);

                        var order = new System.Collections.Generic.List<string>();
                        if (root.TryGetProperty("order", out var o) && o.ValueKind == System.Text.Json.JsonValueKind.Array)
                            foreach (var item in o.EnumerateArray())
                                if (item.ValueKind == System.Text.Json.JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                                    order.Add(item.GetString().Trim());

                        if (order.Count == 0)
                            DatabaseHelper.ExecuteNonQuery("DELETE FROM app_settings WHERE key = 'nav_order'");
                        else
                            DatabaseHelper.ExecuteNonQuery("INSERT OR REPLACE INTO app_settings (key, value) VALUES ('nav_order', @v)",
                                new Microsoft.Data.Sqlite.SqliteParameter("@v", System.Text.Json.JsonSerializer.Serialize(order)));
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, order });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                app.MapGet("/api/features", () =>
                    Microsoft.AspNetCore.Http.Results.Ok(new
                    {
                        scaleEnabled = FeatureFlags.ScaleEnabled,
                        quickSaleEnabled = FeatureFlags.QuickSaleEnabled
                    }));

                app.MapPut("/api/features/scale", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<ScaleFeaturePayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Invalid body" });

                        bool isSoftio = !string.IsNullOrWhiteSpace(body.Username)
                            && body.Username.Trim().Equals("Softio.Admin", StringComparison.OrdinalIgnoreCase);
                        if (!isSoftio)
                            return Microsoft.AspNetCore.Http.Results.Json(
                                new { error = "Only Softio Super Admin can change this setting." },
                                statusCode: 403);

                        FeatureFlags.ScaleEnabled = body.Enabled;
                        if (!body.Enabled)
                        {
                            try { ScaleService.Instance.Disconnect(); } catch { }
                        }
                        else if (ScaleService.Instance.Config.AutoConnect)
                        {
                            try { ScaleService.Instance.Connect(); } catch { }
                        }

                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            success = true,
                            scaleEnabled = FeatureFlags.ScaleEnabled,
                            quickSaleEnabled = FeatureFlags.QuickSaleEnabled
                        });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem(ex.Message);
                    }
                });

                app.MapPut("/api/features/quicksale", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<ScaleFeaturePayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Invalid body" });

                        bool isSoftio = !string.IsNullOrWhiteSpace(body.Username)
                            && body.Username.Trim().Equals("Softio.Admin", StringComparison.OrdinalIgnoreCase);
                        if (!isSoftio)
                            return Microsoft.AspNetCore.Http.Results.Json(
                                new { error = "Only Softio Super Admin can change this setting." },
                                statusCode: 403);

                        FeatureFlags.QuickSaleEnabled = body.Enabled;

                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            success = true,
                            scaleEnabled = FeatureFlags.ScaleEnabled,
                            quickSaleEnabled = FeatureFlags.QuickSaleEnabled
                        });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem(ex.Message);
                    }
                });

                // Print dimensions (label + receipt) — any authenticated desktop client
                app.MapGet("/api/print-settings", () =>
                {
                    var s = PrintSettings.GetSnapshot();
                    return Microsoft.AspNetCore.Http.Results.Ok(ToPrintSettingsDto(s));
                });

                app.MapPut("/api/print-settings", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<PrintSettingsPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null)
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Invalid body" });

                        var snap = new PrintSettingsSnapshot
                        {
                            LabelWidthMm = body.LabelWidthMm > 0 ? body.LabelWidthMm : PrintSettings.DefaultLabelWidthMm,
                            LabelHeightMm = body.LabelHeightMm > 0 ? body.LabelHeightMm : PrintSettings.DefaultLabelHeightMm,
                            LabelGapMm = body.LabelGapMm >= 0 ? body.LabelGapMm : PrintSettings.DefaultLabelGapMm,
                            LabelMarginMm = body.LabelMarginMm >= 0 ? body.LabelMarginMm : PrintSettings.DefaultLabelMarginMm,
                            LabelMarginTopMm = body.LabelMarginTopMm >= 0 ? body.LabelMarginTopMm
                                : (body.LabelMarginMm >= 0 ? body.LabelMarginMm : PrintSettings.DefaultLabelMarginMm),
                            LabelMarginRightMm = body.LabelMarginRightMm >= 0 ? body.LabelMarginRightMm
                                : (body.LabelMarginMm >= 0 ? body.LabelMarginMm : PrintSettings.DefaultLabelMarginMm),
                            LabelMarginBottomMm = body.LabelMarginBottomMm >= 0 ? body.LabelMarginBottomMm
                                : (body.LabelMarginMm >= 0 ? body.LabelMarginMm : PrintSettings.DefaultLabelMarginMm),
                            LabelMarginLeftMm = body.LabelMarginLeftMm >= 0 ? body.LabelMarginLeftMm
                                : (body.LabelMarginMm >= 0 ? body.LabelMarginMm : PrintSettings.DefaultLabelMarginMm),
                            LabelColumns = body.LabelColumns >= 0 ? body.LabelColumns : PrintSettings.DefaultLabelColumns,
                            LabelPaperMode = string.IsNullOrWhiteSpace(body.LabelPaperMode)
                                ? PrintSettings.DefaultLabelPaperMode
                                : body.LabelPaperMode,
                            LabelPageWidthMm = body.LabelPageWidthMm > 0 ? body.LabelPageWidthMm : PrintSettings.DefaultLabelPageWidthMm,
                            LabelPageHeightMm = body.LabelPageHeightMm > 0 ? body.LabelPageHeightMm : PrintSettings.DefaultLabelPageHeightMm,
                            ReceiptWidthMm = body.ReceiptWidthMm > 0 ? body.ReceiptWidthMm : PrintSettings.DefaultReceiptWidthMm,
                            ReceiptHeightMm = body.ReceiptHeightMm >= 0 ? body.ReceiptHeightMm : PrintSettings.DefaultReceiptHeightMm,
                            ReceiptMarginMm = body.ReceiptMarginMm >= 0 ? body.ReceiptMarginMm : PrintSettings.DefaultReceiptMarginMm,
                            ReceiptMarginTopMm = body.ReceiptMarginTopMm >= 0 ? body.ReceiptMarginTopMm
                                : (body.ReceiptMarginMm >= 0 ? body.ReceiptMarginMm : PrintSettings.DefaultReceiptMarginMm),
                            ReceiptMarginRightMm = body.ReceiptMarginRightMm >= 0 ? body.ReceiptMarginRightMm
                                : (body.ReceiptMarginMm >= 0 ? body.ReceiptMarginMm : PrintSettings.DefaultReceiptMarginMm),
                            ReceiptMarginBottomMm = body.ReceiptMarginBottomMm >= 0 ? body.ReceiptMarginBottomMm
                                : (body.ReceiptMarginMm >= 0 ? body.ReceiptMarginMm : PrintSettings.DefaultReceiptMarginMm),
                            ReceiptMarginLeftMm = body.ReceiptMarginLeftMm >= 0 ? body.ReceiptMarginLeftMm
                                : (body.ReceiptMarginMm >= 0 ? body.ReceiptMarginMm : PrintSettings.DefaultReceiptMarginMm),
                            LabelPrinter = body.LabelPrinter ?? "",
                            ReceiptPrinter = body.ReceiptPrinter ?? ""
                        };
                        PrintSettings.Save(snap);
                        return Microsoft.AspNetCore.Http.Results.Ok(ToPrintSettingsDto(PrintSettings.GetSnapshot(), true));
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem(ex.Message);
                    }
                });

                app.MapPut("/api/print-settings/printer-profile", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<PrinterProfilePayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body == null || string.IsNullOrWhiteSpace(body.PrinterName))
                            return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "printerName required" });

                        var profile = new PrinterJobProfile
                        {
                            WidthMm = body.WidthMm,
                            HeightMm = body.HeightMm,
                            GapMm = body.GapMm,
                            MarginMm = body.MarginMm,
                            MarginTopMm = body.MarginTopMm,
                            MarginRightMm = body.MarginRightMm,
                            MarginBottomMm = body.MarginBottomMm,
                            MarginLeftMm = body.MarginLeftMm,
                            Columns = body.Columns,
                            PaperMode = body.PaperMode,
                            PageWidthMm = body.PageWidthMm,
                            PageHeightMm = body.PageHeightMm
                        };
                        var saved = PrintSettings.SavePrinterProfile(body.PrinterName, body.JobType ?? "label", profile);
                        return Microsoft.AspNetCore.Http.Results.Ok(ToPrintSettingsDto(saved, true));
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem(ex.Message);
                    }
                });

                app.MapDelete("/api/print-settings/printer-profile", (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        string name = request.Query["printerName"].ToString();
                        bool clearAll = string.Equals(request.Query["all"].ToString(), "1", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(request.Query["all"].ToString(), "true", StringComparison.OrdinalIgnoreCase);
                        var saved = clearAll
                            ? PrintSettings.ClearAllPrinterProfiles()
                            : PrintSettings.DeletePrinterProfile(name);
                        return Microsoft.AspNetCore.Http.Results.Ok(ToPrintSettingsDto(saved, true));
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem(ex.Message);
                    }
                });

                // Ports are configured via Kestrel above
                InventorySystem.Helpers.StartupTrace.Mark("api: running");
                app.Run();
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("server_error.txt", DateTime.Now.ToString() + ": " + ex.ToString() + "\n");
            }
        }


        private static string ResolveAppRoot()
        {
            string[] candidates =
            {
                Application.StartupPath,
                AppContext.BaseDirectory,
                System.IO.Path.GetDirectoryName(Environment.ProcessPath)
            };
            foreach (string c in candidates)
            {
                if (string.IsNullOrWhiteSpace(c)) continue;
                if (System.IO.Directory.Exists(System.IO.Path.Combine(c, "wwwroot")))
                    return c;
            }
            return string.IsNullOrWhiteSpace(Application.StartupPath)
                ? AppContext.BaseDirectory
                : Application.StartupPath;
        }

        private static string GetWebBackupDirectory() => BackupService.GetBackupDirectory();

        // Payload models for API
        private class LoginPayload
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private class LicenseActivatePayload
        {
            public string LicenseKey { get; set; }
            public string CustomerName { get; set; }
        }

        private class PartyPayload
        {
            public string Name { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Address { get; set; }
            public string Type { get; set; }
            public string Contact { get; set; }
            public decimal CreditLimit { get; set; } = 1000;
            public DateTime? DueDate { get; set; }
            public int ReminderDays { get; set; }
        }

        private class UserPayload
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string FullName { get; set; }
            public string Role { get; set; }
        }

        private class ProductPayload
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
            public decimal Price { get; set; }
            public decimal Cost { get; set; }
            public int Stock { get; set; }
            public int MinStock { get; set; }
            public string Barcode { get; set; }
            public string Sku { get; set; }
            public string Image { get; set; }
            public string Location { get; set; }
            public string Shelf { get; set; }
            public string Uom { get; set; }
            public string Batch { get; set; }
            public string Expiry { get; set; }
            public string ItemType { get; set; }
            public bool IsSalesItem { get; set; } = true;
            public bool IsPurchaseItem { get; set; }
            public bool IsInactive { get; set; }
            public decimal TaxRate { get; set; }
            public bool IsStockTracked { get; set; } = true;
            public bool SellByWeight { get; set; }
            public decimal Price2 { get; set; }
            public decimal Price3 { get; set; }
            public decimal Price4 { get; set; }
            public int? SupplierId { get; set; }
            public int? SupplierPurchaseItemId { get; set; }
        }

        private static InventorySystem.Data.PartData MapProductPayload(ProductPayload body, int id)
        {
            bool inactive = body.IsInactive;
            bool byWeight = FeatureFlags.ScaleEnabled && body.SellByWeight;
            string uom = body.Uom ?? "";
            if (byWeight && string.IsNullOrWhiteSpace(uom)) uom = "kg";
            return new InventorySystem.Data.PartData
            {
                Id = id,
                PartName = body.Name.Trim(),
                Description = body.Description ?? "",
                Barcode = body.Barcode ?? "",
                PartNumber = body.Sku ?? "",
                CategoryName = string.IsNullOrWhiteSpace(body.Category) ? "General" : body.Category.Trim(),
                UnitOfMeasure = uom,
                BatchNumber = body.Batch ?? "",
                Location = body.Location ?? "",
                Shelf = body.Shelf ?? "",
                ExpiryDate = body.Expiry ?? "",
                ItemType = string.IsNullOrWhiteSpace(body.ItemType) ? "Product" : body.ItemType,
                IsSalesItem = body.IsSalesItem,
                IsPurchaseItem = body.IsPurchaseItem,
                IsInactive = inactive,
                TaxRate = body.TaxRate,
                IsStockTracked = body.IsStockTracked,
                SellByWeight = byWeight,
                QuantityInStock = body.Stock,
                MinimumStockLevel = body.MinStock,
                PurchasePrice = body.Cost > 0 ? body.Cost : (body.Price * 0.7m),
                SellingPrice = body.Price,
                Price2 = body.Price2,
                Price3 = body.Price3,
                Price4 = body.Price4,
                PartImage = body.Image,
                SupplierId = body.SupplierId,
                Status = inactive ? "Inactive" : "Active"
            };
        }

        private class ScaleFeaturePayload
        {
            public bool Enabled { get; set; }
            public string Username { get; set; }
        }

        private class PrintSettingsPayload
        {
            public double LabelWidthMm { get; set; }
            public double LabelHeightMm { get; set; }
            public double LabelGapMm { get; set; } = -1;
            public double LabelMarginMm { get; set; } = -1;
            public double LabelMarginTopMm { get; set; } = -1;
            public double LabelMarginRightMm { get; set; } = -1;
            public double LabelMarginBottomMm { get; set; } = -1;
            public double LabelMarginLeftMm { get; set; } = -1;
            public int LabelColumns { get; set; } = -1;
            public string LabelPaperMode { get; set; }
            public double LabelPageWidthMm { get; set; }
            public double LabelPageHeightMm { get; set; }
            public double ReceiptWidthMm { get; set; }
            public double ReceiptHeightMm { get; set; } = -1;
            public double ReceiptMarginMm { get; set; } = -1;
            public double ReceiptMarginTopMm { get; set; } = -1;
            public double ReceiptMarginRightMm { get; set; } = -1;
            public double ReceiptMarginBottomMm { get; set; } = -1;
            public double ReceiptMarginLeftMm { get; set; } = -1;
            public string LabelPrinter { get; set; }
            public string ReceiptPrinter { get; set; }
        }

        private class PrinterProfilePayload
        {
            public string PrinterName { get; set; }
            public string JobType { get; set; }
            public double WidthMm { get; set; }
            public double HeightMm { get; set; }
            public double GapMm { get; set; } = -1;
            public double MarginMm { get; set; } = -1;
            public double MarginTopMm { get; set; } = -1;
            public double MarginRightMm { get; set; } = -1;
            public double MarginBottomMm { get; set; } = -1;
            public double MarginLeftMm { get; set; } = -1;
            public int Columns { get; set; }
            public string PaperMode { get; set; }
            public double PageWidthMm { get; set; }
            public double PageHeightMm { get; set; }
        }

        private static object ProfileMargins(PrinterJobProfile p) => p == null ? null : new
        {
            widthMm = p.WidthMm,
            heightMm = p.HeightMm,
            gapMm = p.GapMm,
            marginMm = p.MarginMm,
            marginTopMm = p.MarginTopMm,
            marginRightMm = p.MarginRightMm,
            marginBottomMm = p.MarginBottomMm,
            marginLeftMm = p.MarginLeftMm,
            columns = p.Columns,
            paperMode = p.PaperMode,
            pageWidthMm = p.PageWidthMm,
            pageHeightMm = p.PageHeightMm
        };

        private static object ToPrintSettingsDto(PrintSettingsSnapshot s, bool success = false)
        {
            var profiles = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (s?.PrinterProfiles != null)
            {
                foreach (var kv in s.PrinterProfiles)
                {
                    profiles[kv.Key] = new
                    {
                        label = kv.Value?.Label == null ? null : ProfileMargins(kv.Value.Label),
                        receipt = kv.Value?.Receipt == null ? null : new
                        {
                            widthMm = kv.Value.Receipt.WidthMm,
                            heightMm = kv.Value.Receipt.HeightMm,
                            marginMm = kv.Value.Receipt.MarginMm,
                            marginTopMm = kv.Value.Receipt.MarginTopMm,
                            marginRightMm = kv.Value.Receipt.MarginRightMm,
                            marginBottomMm = kv.Value.Receipt.MarginBottomMm,
                            marginLeftMm = kv.Value.Receipt.MarginLeftMm
                        }
                    };
                }
            }

            return new
            {
                success,
                labelWidthMm = s.LabelWidthMm,
                labelHeightMm = s.LabelHeightMm,
                labelGapMm = s.LabelGapMm,
                labelMarginMm = s.LabelMarginMm,
                labelMarginTopMm = s.LabelMarginTopMm,
                labelMarginRightMm = s.LabelMarginRightMm,
                labelMarginBottomMm = s.LabelMarginBottomMm,
                labelMarginLeftMm = s.LabelMarginLeftMm,
                labelColumns = s.LabelColumns,
                labelPaperMode = s.LabelPaperMode,
                labelPageWidthMm = s.LabelPageWidthMm,
                labelPageHeightMm = s.LabelPageHeightMm,
                receiptWidthMm = s.ReceiptWidthMm,
                receiptHeightMm = s.ReceiptHeightMm,
                receiptMarginMm = s.ReceiptMarginMm,
                receiptMarginTopMm = s.ReceiptMarginTopMm,
                receiptMarginRightMm = s.ReceiptMarginRightMm,
                receiptMarginBottomMm = s.ReceiptMarginBottomMm,
                receiptMarginLeftMm = s.ReceiptMarginLeftMm,
                labelPrinter = s.LabelPrinter ?? "",
                receiptPrinter = s.ReceiptPrinter ?? "",
                printerProfiles = profiles
            };
        }

        private class AddItemPayload
        {
            public string Name { get; set; }
            public string Category { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string Barcode { get; set; }
            public string Sku { get; set; }
        }

        private class CheckoutPayload
        {
            public System.Collections.Generic.List<CheckoutItem> Items { get; set; }
            public int? CustomerId { get; set; }
            public string Status { get; set; }
            public bool? IsPaid { get; set; }
            public string ShippingAddress { get; set; }
            public DateTime? DueDate { get; set; }
            public DateTime? DeliveryDate { get; set; }
            public decimal VatAmount { get; set; }
            public decimal ShippingAmount { get; set; }
            public decimal DiscountAmount { get; set; }
            public decimal? TotalAmount { get; set; }
        }

        private class StockAdjustPayload
        {
            public int Change { get; set; }
            public string Reason { get; set; }
        }

        private class PaymentPayload
        {
            public decimal Amount { get; set; }
            public string Note { get; set; }
        }

        private class SupplierPurchasePayload
        {
            public string Name { get; set; }
            public string Category { get; set; }
            public decimal Quantity { get; set; } = 1;
            public decimal UnitPrice { get; set; }
            public bool IsPaid { get; set; }
            public string PaymentStatus { get; set; }
            public string Notes { get; set; }
        }

        private class DebtPaymentPayload
        {
            public decimal Amount { get; set; }
            public string Note { get; set; }
            public System.Collections.Generic.List<DebtAllocationPayload> Allocations { get; set; }
        }

        private class DebtAllocationPayload
        {
            public int? OrderId { get; set; }
            public int? OrderItemId { get; set; }
            public decimal Amount { get; set; }
        }
        private class CheckoutItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Qty { get; set; }
            /// <summary>Grams (or other stock units) to deduct for weighed lines; 0 = use Qty.</summary>
            public int StockQty { get; set; }
            public decimal WeightKg { get; set; }
            /// <summary>Sell without reducing inventory (car stock / quick sale).</summary>
            public bool SkipStock { get; set; }
        }

        private class ReturnPayload
        {
            public int OrderId { get; set; }
            public string Reason { get; set; }
            public System.Collections.Generic.List<ReturnItemDetail> Items { get; set; }
        }
        private class BlindReturnPayload
        {
            public string Reason { get; set; }
            public int? CustomerId { get; set; }
            public System.Collections.Generic.List<ReturnItemDetail> Items { get; set; }
        }
        private class ReturnItemDetail
        {
            public int PartId { get; set; }
            public int Qty { get; set; }
            public decimal RefundAmount { get; set; }
        }

        private class ExpensePayload
        {
            public string Category { get; set; }
            public DateTime ExpenseDate { get; set; }
            public decimal Amount { get; set; }
            public string Description { get; set; }
            public string RecordedBy { get; set; }
            public bool IsPaid { get; set; }
            public bool IsRecurring { get; set; }
            public int? SupplierId { get; set; }
            public decimal Liters { get; set; }
            public decimal UnitPrice { get; set; }
            public string MilkType { get; set; }
            public string Shift { get; set; }
        }

        private class BulkCsvImportPayload
        {
            public System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, string>> Rows { get; set; }
            public System.Collections.Generic.List<string> Headers { get; set; }
            public string RecordedBy { get; set; }
        }

        private class SaleImportRow
        {
            public string Customer { get; set; }
            public decimal Total { get; set; }
            public string Date { get; set; }
            public string Payment { get; set; }
            public string PaymentStatus { get; set; }
        }

        private class HistoryImportRow
        {
            public string Date { get; set; }
            public string Action { get; set; }
            public string Item { get; set; }
            public string Customer { get; set; }
            public string Details { get; set; }
            public string Description { get; set; }
            public string User { get; set; }
            public string Status { get; set; }
            public string Payment { get; set; }
            public decimal Total { get; set; }
        }

        private class ReportImportRow
        {
            public string Name { get; set; }
            public decimal Qty { get; set; }
            public decimal Sales { get; set; }
            public decimal Profit { get; set; }
            public decimal Total { get; set; }
            public string Date { get; set; }
            public string Metric { get; set; }
            public string Value { get; set; }
        }

        private static string NormalizeImportDate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (DateTime.TryParse(raw, out var dt))
                return dt.ToString("yyyy-MM-dd HH:mm:ss");
            return null;
        }

        /// <summary>
        /// Inserts a completed historical sale without touching stock levels.
        /// Used by Sales / History / Reports CSV import.
        /// </summary>
        private static bool ImportHistoricalSale(SaleImportRow row)
        {
            if (row == null || row.Total <= 0) return false;

            string customerName = (row.Customer ?? "").Trim();
            bool walkIn = string.IsNullOrEmpty(customerName)
                || customerName.Equals("Walk-in", StringComparison.OrdinalIgnoreCase)
                || customerName.Equals("Cash Customer", StringComparison.OrdinalIgnoreCase)
                || customerName.Equals("Walk-in Customer", StringComparison.OrdinalIgnoreCase);

            int? customerId = null;
            if (!walkIn)
            {
                var found = DatabaseHelper.ExecuteScalar<object>(
                    "SELECT customer_id FROM customers WHERE full_name = @n COLLATE NOCASE LIMIT 1",
                    new Microsoft.Data.Sqlite.SqliteParameter("@n", customerName));
                if (found != null && found != DBNull.Value)
                    customerId = Convert.ToInt32(found);
                else
                    customerId = new CustomerService().AddCustomer(customerName, "", "", "", "Regular");
            }

            string payRaw = (row.Payment ?? row.PaymentStatus ?? "Paid").Trim();
            bool isPaid = !payRaw.Equals("Unpaid", StringComparison.OrdinalIgnoreCase)
                && !payRaw.Equals("Pending", StringComparison.OrdinalIgnoreCase);
            string paymentStatus = isPaid ? "Paid" : "Unpaid";
            decimal amountPaid = isPaid ? row.Total : 0;
            string orderDate = NormalizeImportDate(row.Date) ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (customerId.HasValue)
            {
                DatabaseHelper.ExecuteNonQuery(
                    @"INSERT INTO orders (order_date, customer_id, total_amount, status, payment_status, amount_paid)
                      VALUES (@d, @c, @t, 'Completed', @p, @a)",
                    new Microsoft.Data.Sqlite.SqliteParameter("@d", orderDate),
                    new Microsoft.Data.Sqlite.SqliteParameter("@c", customerId.Value),
                    new Microsoft.Data.Sqlite.SqliteParameter("@t", row.Total),
                    new Microsoft.Data.Sqlite.SqliteParameter("@p", paymentStatus),
                    new Microsoft.Data.Sqlite.SqliteParameter("@a", amountPaid));

                if (!isPaid)
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE customers SET current_balance = current_balance + @t WHERE customer_id = @c",
                        new Microsoft.Data.Sqlite.SqliteParameter("@t", row.Total),
                        new Microsoft.Data.Sqlite.SqliteParameter("@c", customerId.Value));
                }
            }
            else
            {
                DatabaseHelper.ExecuteNonQuery(
                    @"INSERT INTO orders (order_date, total_amount, status, payment_status, amount_paid)
                      VALUES (@d, @t, 'Completed', @p, @a)",
                    new Microsoft.Data.Sqlite.SqliteParameter("@d", orderDate),
                    new Microsoft.Data.Sqlite.SqliteParameter("@t", row.Total),
                    new Microsoft.Data.Sqlite.SqliteParameter("@p", paymentStatus),
                    new Microsoft.Data.Sqlite.SqliteParameter("@a", amountPaid));
            }

            return true;
        }

        private static System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>> DataTableToList(System.Data.DataTable dt)
        {
            var list = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>();
            if (dt == null) return list;
            foreach (System.Data.DataRow row in dt.Rows)
            {
                var dict = new System.Collections.Generic.Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (System.Data.DataColumn col in dt.Columns)
                {
                    object val = row[col];
                    dict[col.ColumnName] = val == DBNull.Value ? null : val;
                }
                list.Add(dict);
            }
            return list;
        }
    }
}

// ============================================================
//  SignalR Hub -- manages real-time WebSocket connections
// ============================================================
namespace InventorySystem
{
    using Microsoft.AspNetCore.SignalR;

    /// <summary>
    /// SignalR Hub for real-time inventory synchronization.
    /// Connected clients (web POS tablets and WinForms app) receive
    /// live push events whenever stock or sales data changes.
    /// </summary>
    public class InventoryHub : Hub
    {
        /// <summary>Called by any client to trigger a refresh on all others.</summary>
        public async Task RequestRefresh(string reason = "manual")
        {
            await Clients.Others.SendAsync("StockUpdated", reason);
        }
    }

    /// <summary>
    /// Static broadcaster: lets WinForms code push events to ALL
    /// connected web clients (tablets) with a single line of code.
    /// Usage: _ = InventoryBroadcaster.Broadcast("SaleCompleted", "Order #42");
    /// </summary>
    public static class InventoryBroadcaster
    {
        public static IHubContext<InventoryHub> HubContext { get; set; }

        /// <summary>
        /// Broadcasts a named event + message to every connected SignalR client.
        /// Safe to call fire-and-forget: _ = InventoryBroadcaster.Broadcast(...)
        /// </summary>
        public static async System.Threading.Tasks.Task Broadcast(string eventName, string message = "")
        {
            try
            {
                if (HubContext != null)
                    await HubContext.Clients.All.SendAsync(eventName, message);
            }
            catch { /* Never crash the caller due to broadcast failure */ }
        }

        /// <summary>Broadcast a structured payload (e.g. live scale weight).</summary>
        public static async System.Threading.Tasks.Task BroadcastObject(string eventName, object payload)
        {
            try
            {
                if (HubContext != null)
                    await HubContext.Clients.All.SendAsync(eventName, payload);
            }
            catch { }
        }

        /// <summary>Convenience: broadcast a generic stock-changed event.</summary>
        public static void BroadcastStockChange(string reason = "desktop")
        {
            _ = Broadcast("StockUpdated", reason);
        }
    }
}

