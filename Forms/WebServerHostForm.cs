using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using InventorySystem.Helpers;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace InventorySystem
{
    /// <summary>
    /// Borderless desktop host embedding the branded web UI.
    /// Sized to the screen WorkingArea so the Windows taskbar stays visible.
    /// </summary>
    public class WebServerHostForm : Form
    {
        private static string PortalUrl => AppHostConfig.LoopbackUrl;
        private static readonly Color BrandColor = ThemeConfig.PrimaryColor;

        private readonly WebView2 _webView;
        private double _zoom = 1.0; // display size requested by the page (Settings)
        private readonly Panel _splash;
        private readonly Panel _spinner;
        private readonly System.Windows.Forms.Timer _spinnerTimer;
        private Control _logoControl;
        private float _spinnerAngle;
        private bool _isMaximized = true;
        private Rectangle _restoreBounds;
        private Task<CoreWebView2Environment> _envTask;

        private const int WM_SYSCOMMAND = 0x112;
        private const int SC_MOVE = 0xF010;
        private const int HTCAPTION = 0x2;

        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public WebServerHostForm()
        {
            Text = ThemeConfig.AppTitle;
            MinimumSize = new Size(960, 640);
            StartPosition = FormStartPosition.Manual;
            BackColor = BrandColor;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = true;
            ShowIcon = true;
            Opacity = 1;

            try
            {
                string iconPath = System.IO.Path.Combine(Application.StartupPath, "Assets", "icon.ico");
                if (System.IO.File.Exists(iconPath))
                    Icon = new Icon(iconPath);
            }
            catch { }

            FitToWorkingArea();

            // Start WebView2 environment immediately (parallel with server boot)
            var userData = System.IO.Path.Combine(DatabaseConfig.UserDataDirectory, "WebView2");
            try { _envTask = CoreWebView2Environment.CreateAsync(null, userData, CreateBrowserOptions()); }
            catch { _envTask = null; }

            _splash = new Panel { Dock = DockStyle.Fill, BackColor = BrandColor };

            _spinner = new Panel
            {
                Size = new Size(44, 44),
                BackColor = BrandColor
            };
            _spinner.Paint += Spinner_Paint;
            _splash.Controls.Add(_spinner);

            _spinnerTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _spinnerTimer.Tick += (s, e) =>
            {
                _spinnerAngle = (_spinnerAngle + 8f) % 360f;
                _spinner.Invalidate();
            };
            _spinnerTimer.Start();

            try
            {
                string logoPath = System.IO.Path.Combine(Application.StartupPath, "Assets", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    var pb = new PictureBox
                    {
                        Size = new Size(168, 168),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = BrandColor, // same as the splash: logo only, no white tile
                        Image = Image.FromFile(logoPath)
                    };
                    _logoControl = pb;
                    _splash.Controls.Add(pb);
                }
                else
                {
                    var brand = new Label
                    {
                        AutoSize = false,
                        Size = new Size(280, 72),
                        Text = ThemeConfig.CompanyName,
                        Font = new Font("Segoe UI", 32f, FontStyle.Bold),
                        ForeColor = Color.White,
                        BackColor = Color.Transparent,
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    _logoControl = brand;
                    _splash.Controls.Add(brand);
                }
            }
            catch
            {
                var logo = new Panel { Size = new Size(120, 120), BackColor = Color.White };
                _logoControl = logo;
                _splash.Controls.Add(logo);
            }

            _splash.Resize += (s, e) => LayoutSplash();
            LayoutSplash();

            // The browser stays Visible underneath the splash: a hidden WebView2 is treated like a
            // background tab (low-priority loading, paused animation frames), which made startup crawl.
            _webView = new WebView2
            {
                Dock = DockStyle.Fill,
                Visible = true,
                DefaultBackgroundColor = BrandColor
            };
            Controls.Add(_webView);
            Controls.Add(_splash);
            _splash.BringToFront();
            Resize += (s, e) =>
            {
                if (_printUiInset) ApplyPrintUiInset();
            };

            Shown += async (s, e) => await InitializeAsync();
        }

        private bool _printUiInset;
        private bool _uiRevealed;
        private System.Windows.Forms.Timer _revealFallbackTimer;

        private void LayoutSplash()
        {
            if (_logoControl == null) return;
            int gap = 28;
            int totalH = _logoControl.Height + gap + _spinner.Height;
            int top = Math.Max(0, (_splash.Height - totalH) / 2);
            _logoControl.Left = (_splash.Width - _logoControl.Width) / 2;
            _logoControl.Top = top;
            if (!(_logoControl is PictureBox))
                ApplyRoundedCorners(_logoControl, 24);
            _spinner.Left = (_splash.Width - _spinner.Width) / 2;
            _spinner.Top = _logoControl.Bottom + gap;
        }

        private void Spinner_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int pad = 4;
            var rect = new Rectangle(pad, pad, _spinner.Width - pad * 2, _spinner.Height - pad * 2);
            using (var track = new Pen(Color.FromArgb(70, 255, 255, 255), 3.5f))
            {
                track.StartCap = LineCap.Round;
                track.EndCap = LineCap.Round;
                e.Graphics.DrawEllipse(track, rect);
            }
            using (var arc = new Pen(Color.White, 3.5f))
            {
                arc.StartCap = LineCap.Round;
                arc.EndCap = LineCap.Round;
                e.Graphics.DrawArc(arc, rect, _spinnerAngle, 90f);
            }
        }

        private static void ApplyRoundedCorners(Control control, int radius)
        {
            if (control == null || control.Width <= 0 || control.Height <= 0) return;
            try
            {
                using var path = new GraphicsPath();
                int d = radius * 2;
                var r = new Rectangle(0, 0, control.Width, control.Height);
                path.AddArc(r.X, r.Y, d, d, 180, 90);
                path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                control.Region?.Dispose();
                control.Region = new Region(path);
            }
            catch { }
        }

        private void FitToWorkingArea()
        {
            Rectangle wa = Screen.FromPoint(Cursor.Position).WorkingArea;
            Bounds = wa;
            _isMaximized = true;
            _restoreBounds = new Rectangle(
                wa.X + Math.Max(40, (wa.Width - 1280) / 2),
                wa.Y + Math.Max(40, (wa.Height - 800) / 2),
                Math.Min(1280, wa.Width - 80),
                Math.Min(800, wa.Height - 80));
        }

        private void ToggleMaximize()
        {
            if (_isMaximized)
            {
                Bounds = _restoreBounds;
                _isMaximized = false;
            }
            else
            {
                _restoreBounds = Bounds;
                Bounds = Screen.FromControl(this).WorkingArea;
                _isMaximized = true;
            }
            PostWindowState();
        }

        /// <summary>
        /// Starts a native window move from the web title bar. If the host is
        /// "maximized" (fitted to the working area), restores under the cursor first
        /// so the window can be dragged onto another monitor.
        /// </summary>
        private void BeginTitleBarDrag()
        {
            if (_isMaximized)
            {
                Point cursor = Cursor.Position;
                Rectangle restore = _restoreBounds;
                if (restore.Width < MinimumSize.Width || restore.Height < MinimumSize.Height)
                {
                    Rectangle wa = Screen.FromPoint(cursor).WorkingArea;
                    restore = new Rectangle(
                        wa.X + Math.Max(40, (wa.Width - 1280) / 2),
                        wa.Y + Math.Max(40, (wa.Height - 800) / 2),
                        Math.Min(1280, Math.Max(MinimumSize.Width, wa.Width - 80)),
                        Math.Min(800, Math.Max(MinimumSize.Height, wa.Height - 80)));
                }

                double ratio = (double)(cursor.X - Bounds.Left) / Math.Max(1, Bounds.Width);
                ratio = Math.Max(0.05, Math.Min(0.95, ratio));
                int newX = cursor.X - (int)(restore.Width * ratio);
                int newY = cursor.Y - 26; // keep pointer in the title-bar band

                Rectangle targetScreen = Screen.FromPoint(cursor).WorkingArea;
                newX = Math.Max(targetScreen.Left - restore.Width + 80,
                    Math.Min(newX, targetScreen.Right - 80));
                newY = Math.Max(targetScreen.Top,
                    Math.Min(newY, targetScreen.Bottom - 80));

                Bounds = new Rectangle(newX, newY, restore.Width, restore.Height);
                _isMaximized = false;
                _restoreBounds = Bounds;
                PostWindowState();
            }

            // Steal mouse capture from WebView2 while the button is still down,
            // then enter the native move loop (same pattern as MainForm title drag).
            try { if (_webView != null && !_webView.IsDisposed) _webView.Capture = false; } catch { }
            Capture = false;
            ReleaseCapture();
            // SC_MOVE | HTCAPTION — more reliable than WM_NCLBUTTONDOWN alone with WebView2
            SendMessage(Handle, WM_SYSCOMMAND, SC_MOVE | HTCAPTION, 0);
        }

        private async Task InitializeAsync()
        {
            try
            {
                var readyTask = WaitForServerAsync(TimeSpan.FromSeconds(20));
                CoreWebView2Environment env = null;
                if (_envTask != null)
                {
                    try { env = await _envTask; } catch { env = null; }
                    InventorySystem.Helpers.StartupTrace.Mark("ui: webview environment ready");
                }
                if (env == null)
                {
                    var userData = System.IO.Path.Combine(DatabaseConfig.UserDataDirectory, "WebView2");
                    env = await CoreWebView2Environment.CreateAsync(null, userData, CreateBrowserOptions());
                }

                // Create the browser while the local server is still starting (both take a few seconds)
                var createTask = _webView.EnsureCoreWebView2Async(env);

                bool serverOk = await readyTask;
                InventorySystem.Helpers.StartupTrace.Mark("ui: server answered = " + serverOk);
                if (!serverOk)
                {
                    ShowFatal(ThemeConfig.CompanyName + " could not start the local service.\nPlease restart the application.");
                    return;
                }

                await createTask;
                InventorySystem.Helpers.StartupTrace.Mark("ui: webview created");
                try { _webView.ZoomFactor = _zoom; } catch { }
                _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                _webView.CoreWebView2.Settings.IsZoomControlEnabled = false;
                _webView.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
                _webView.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;

                _webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

                // Don't wait for every image/font: once the page structure is ready, reveal shortly after
                // (the page normally reports "uiReady" first; this is only the safety net).
                _webView.CoreWebView2.DOMContentLoaded += (s, e) =>
                {
                    InventorySystem.Helpers.StartupTrace.Mark("ui: page structure ready");
                    var t = new System.Windows.Forms.Timer { Interval = 1500 };
                    t.Tick += (s2, e2) => { t.Stop(); t.Dispose(); RevealUi(); };
                    t.Start();
                };

                var tcs = new TaskCompletionSource<bool>();
                void OnNav(object s, CoreWebView2NavigationCompletedEventArgs e)
                {
                    try { _webView.ZoomFactor = _zoom; } catch { }
                    tcs.TrySetResult(e.IsSuccess);
                }
                _webView.CoreWebView2.NavigationCompleted += OnNav;
                _webView.CoreWebView2.Navigate(PortalUrl);
                bool ok = await tcs.Task;
                InventorySystem.Helpers.StartupTrace.Mark("ui: page loaded = " + ok);
                _webView.CoreWebView2.NavigationCompleted -= OnNav;

                if (!ok)
                {
                    ShowFatal(ThemeConfig.CompanyName + " failed to load the interface.\nPlease restart the application.");
                    return;
                }

                // Keep splash on top until the web UI signals it is painted/ready
                // (avoids white flash / layout glitch right after the loading screen).
                _webView.Visible = true;
                _webView.SendToBack();
                _splash.Visible = true;
                _splash.BringToFront();

                _revealFallbackTimer?.Stop();
                _revealFallbackTimer?.Dispose();
                _revealFallbackTimer = new System.Windows.Forms.Timer { Interval = 4000 };
                _revealFallbackTimer.Tick += (s, e) =>
                {
                    _revealFallbackTimer.Stop();
                    RevealUi();
                };
                _revealFallbackTimer.Start();
            }
            catch (WebView2RuntimeNotFoundException)
            {
                ShowFatal("Microsoft Edge WebView2 Runtime is required.\nDownload: https://go.microsoft.com/fwlink/p/?LinkId=2124703");
            }
            catch (Exception ex)
            {
                ShowFatal(ThemeConfig.CompanyName + " failed to open:\n" + ex.Message);
            }
        }

        // Browser flags: keep full speed even if the window is behind other windows while loading
        private static CoreWebView2EnvironmentOptions CreateBrowserOptions()
        {
            return new CoreWebView2EnvironmentOptions(
                "--disable-features=CalculateNativeWinOcclusion " +
                "--disable-backgrounding-occluded-windows " +
                "--disable-renderer-backgrounding " +
                "--disable-background-timer-throttling");
        }

        private void RevealUi()
        {
            if (_uiRevealed || IsDisposed) return;
            _uiRevealed = true;
            InventorySystem.Helpers.StartupTrace.Mark("ui: revealed");
            try
            {
                _revealFallbackTimer?.Stop();
                _revealFallbackTimer?.Dispose();
                _revealFallbackTimer = null;
            }
            catch { }

            try { _spinnerTimer.Stop(); } catch { }

            _splash.Visible = false;
            _webView.Visible = true;
            _webView.BringToFront();
            BackColor = BrandColor;
            PostWindowState();
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string json = e.TryGetWebMessageAsString();
                if (string.IsNullOrWhiteSpace(json)) return;

                // Capture values before JsonDocument is disposed (BeginInvoke is async)
                string action;
                string openUrl = null;
                double zoom = 1.0;
                using (var doc = JsonDocument.Parse(json))
                {
                    action = doc.RootElement.TryGetProperty("action", out var a) ? a.GetString() : null;
                    if (string.IsNullOrEmpty(action)) return;
                    if (action == "openUrl" && doc.RootElement.TryGetProperty("url", out var u))
                        openUrl = u.GetString();
                    if (action == "setZoom" && doc.RootElement.TryGetProperty("value", out var z) && z.ValueKind == JsonValueKind.Number)
                        zoom = Math.Max(0.5, Math.Min(2.0, z.GetDouble()));
                }

                // App display size chosen in Settings (zoom control is otherwise disabled)
                if (action == "setZoom")
                {
                    BeginInvoke(new Action(() => { _zoom = zoom; try { _webView.ZoomFactor = zoom; } catch { } }));
                    return;
                }

                // Drag must run synchronously while the left mouse button is still down.
                // BeginInvoke is too late — WebView2 has already finished the click by then.
                if (action == "drag")
                {
                    void DoDrag()
                    {
                        try { BeginTitleBarDrag(); } catch { }
                    }
                    if (InvokeRequired) Invoke((Action)DoDrag);
                    else DoDrag();
                    return;
                }

                BeginInvoke(new Action(() =>
                {
                    switch (action)
                    {
                        case "minimize":
                            WindowState = FormWindowState.Minimized;
                            break;
                        case "maximize":
                            ToggleMaximize();
                            break;
                        case "close":
                            Close();
                            break;
                        case "openUrl":
                            if (!string.IsNullOrWhiteSpace(openUrl))
                            {
                                try
                                {
                                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(openUrl)
                                    {
                                        UseShellExecute = true
                                    });
                                }
                                catch { }
                            }
                            break;
                        case "beginPrint":
                            SetPrintUiInset(true);
                            break;
                        case "endPrint":
                            SetPrintUiInset(false);
                            break;
                        case "uiReady":
                            RevealUi();
                            break;
                        case "listPrinters":
                            HandleListPrinters();
                            break;
                        case "printBarcodes":
                            HandlePrintBarcodes(json);
                            break;
                        case "printReceipt":
                            HandlePrintReceipt(json);
                            break;
                    }
                }));
            }
            catch { }
        }

        private void SetPrintUiInset(bool inset)
        {
            _printUiInset = inset;
            if (inset) ApplyPrintUiInset();
            else RestoreWebViewFill();
        }

        private void ApplyPrintUiInset()
        {
            if (_webView == null || !_webView.IsHandleCreated) return;
            const int left = 20;
            const int right = 20;
            const int top = 28;
            const int bottom = 28;
            _webView.Dock = DockStyle.None;
            int w = Math.Max(320, ClientSize.Width - left - right);
            int h = Math.Max(240, ClientSize.Height - top - bottom);
            _webView.Bounds = new Rectangle(left, top, w, h);
            BackColor = Color.FromArgb(15, 23, 42);
            _webView.BringToFront();
        }

        private void RestoreWebViewFill()
        {
            if (_webView == null) return;
            _webView.Dock = DockStyle.Fill;
            BackColor = BrandColor;
        }

        private void HandleListPrinters()
        {
            try
            {
                var printers = new List<string>();
                string defaultPrinter = "";
                try
                {
                    foreach (string name in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                        printers.Add(name);
                    using var probe = new System.Drawing.Printing.PrintDocument();
                    defaultPrinter = probe.PrinterSettings.PrinterName ?? "";
                }
                catch { }

                var namesJson = string.Join(",", printers.Select(p =>
                    "\"" + p.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\""));
                var defEsc = (defaultPrinter ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                _webView?.CoreWebView2?.PostWebMessageAsJson(
                    $"{{\"type\":\"printers\",\"printers\":[{namesJson}],\"defaultPrinter\":\"{defEsc}\"}}");
            }
            catch { }
        }

        private void HandlePrintBarcodes(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (!root.TryGetProperty("items", out var arr) || arr.ValueKind != JsonValueKind.Array)
                    return;

                var items = new List<LabelPrintItem>();
                foreach (var el in arr.EnumerateArray())
                {
                    string name = el.TryGetProperty("name", out var n) ? n.GetString() : "";
                    string sku = el.TryGetProperty("sku", out var s) ? s.GetString() : "";
                    if (string.IsNullOrWhiteSpace(sku) && el.TryGetProperty("barcode", out var b))
                        sku = b.GetString();
                    decimal price = 0;
                    if (el.TryGetProperty("price", out var p))
                    {
                        if (p.ValueKind == JsonValueKind.Number) price = p.GetDecimal();
                        else decimal.TryParse(p.GetString(), out price);
                    }
                    int qty = 1;
                    if (el.TryGetProperty("quantity", out var q) && q.ValueKind == JsonValueKind.Number)
                        qty = Math.Max(1, q.GetInt32());

                    if (string.IsNullOrWhiteSpace(sku) && string.IsNullOrWhiteSpace(name)) continue;
                    items.Add(new LabelPrintItem
                    {
                        Name = name ?? "",
                        SKU = string.IsNullOrWhiteSpace(sku) ? (name ?? "") : sku,
                        Price = price,
                        Quantity = qty
                    });
                }

                if (items.Count == 0) return;

                var options = new LabelPrintOptions();
                if (root.TryGetProperty("printerName", out var pn) && pn.ValueKind == JsonValueKind.String)
                    options.PrinterName = pn.GetString();
                if (root.TryGetProperty("copies", out var c) && c.ValueKind == JsonValueKind.Number)
                    options.Copies = Math.Max(1, Math.Min(99, c.GetInt32()));
                if (root.TryGetProperty("landscape", out var ls))
                    options.Landscape = ls.ValueKind == JsonValueKind.True ||
                        (ls.ValueKind == JsonValueKind.String && ls.GetString() == "true");
                if (root.TryGetProperty("color", out var col))
                    options.Color = col.ValueKind != JsonValueKind.False &&
                        !(col.ValueKind == JsonValueKind.String && col.GetString() == "false");
                if (root.TryGetProperty("pageRange", out var pr) && pr.ValueKind == JsonValueKind.String)
                    options.PageRange = pr.GetString() ?? "all";
                if (root.TryGetProperty("labelWidthMm", out var lw) && lw.ValueKind == JsonValueKind.Number)
                    options.LabelWidthMm = lw.GetDouble();
                if (root.TryGetProperty("labelHeightMm", out var lh) && lh.ValueKind == JsonValueKind.Number)
                    options.LabelHeightMm = lh.GetDouble();
                if (root.TryGetProperty("labelGapMm", out var lg) && lg.ValueKind == JsonValueKind.Number)
                    options.LabelGapMm = lg.GetDouble();
                if (root.TryGetProperty("marginMm", out var lm) && lm.ValueKind == JsonValueKind.Number)
                    options.MarginMm = lm.GetDouble();
                if (root.TryGetProperty("marginTopMm", out var mt) && mt.ValueKind == JsonValueKind.Number)
                    options.MarginTopMm = mt.GetDouble();
                if (root.TryGetProperty("marginRightMm", out var mr) && mr.ValueKind == JsonValueKind.Number)
                    options.MarginRightMm = mr.GetDouble();
                if (root.TryGetProperty("marginBottomMm", out var mb) && mb.ValueKind == JsonValueKind.Number)
                    options.MarginBottomMm = mb.GetDouble();
                if (root.TryGetProperty("marginLeftMm", out var ml) && ml.ValueKind == JsonValueKind.Number)
                    options.MarginLeftMm = ml.GetDouble();
                if (root.TryGetProperty("columns", out var cols) && cols.ValueKind == JsonValueKind.Number)
                    options.Columns = cols.GetInt32();
                if (root.TryGetProperty("paperMode", out var pm) && pm.ValueKind == JsonValueKind.String)
                    options.PaperMode = pm.GetString();
                if (root.TryGetProperty("pageWidthMm", out var pw) && pw.ValueKind == JsonValueKind.Number)
                    options.PageWidthMm = pw.GetDouble();
                if (root.TryGetProperty("pageHeightMm", out var ph) && ph.ValueKind == JsonValueKind.Number)
                    options.PageHeightMm = ph.GetDouble();

                ThermalLabelHelper.PrintLabels(items, options, this);
                PostPrintResult(true, null);
            }
            catch (Exception ex)
            {
                PostPrintResult(false, ex.Message);
                MessageBox.Show(this, "Could not print labels:\n" + ex.Message,
                    ThemeConfig.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void HandlePrintReceipt(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var options = new ReceiptPrintOptions();

                if (root.TryGetProperty("customerName", out var cn) && cn.ValueKind == JsonValueKind.String)
                    options.CustomerName = cn.GetString();
                if (root.TryGetProperty("shippingTo", out var st) && st.ValueKind == JsonValueKind.String)
                    options.ShippingTo = st.GetString();
                if (root.TryGetProperty("currencySymbol", out var cs) && cs.ValueKind == JsonValueKind.String)
                    options.CurrencySymbol = cs.GetString() ?? "$";
                if (root.TryGetProperty("printerName", out var pn) && pn.ValueKind == JsonValueKind.String)
                    options.PrinterName = pn.GetString();
                if (root.TryGetProperty("copies", out var c) && c.ValueKind == JsonValueKind.Number)
                    options.Copies = Math.Max(1, Math.Min(99, c.GetInt32()));
                if (root.TryGetProperty("landscape", out var ls))
                    options.Landscape = ls.ValueKind == JsonValueKind.True;
                if (root.TryGetProperty("color", out var col))
                    options.Color = col.ValueKind != JsonValueKind.False;
                if (root.TryGetProperty("paperWidthMm", out var pw) && pw.ValueKind == JsonValueKind.Number)
                    options.PaperWidthMm = pw.GetDouble();
                if (root.TryGetProperty("paperHeightMm", out var ph) && ph.ValueKind == JsonValueKind.Number)
                    options.PaperHeightMm = ph.GetDouble();
                if (root.TryGetProperty("marginMm", out var mm) && mm.ValueKind == JsonValueKind.Number)
                    options.MarginMm = mm.GetDouble();
                if (root.TryGetProperty("marginTopMm", out var rmt) && rmt.ValueKind == JsonValueKind.Number)
                    options.MarginTopMm = rmt.GetDouble();
                if (root.TryGetProperty("marginRightMm", out var rmr) && rmr.ValueKind == JsonValueKind.Number)
                    options.MarginRightMm = rmr.GetDouble();
                if (root.TryGetProperty("marginBottomMm", out var rmb) && rmb.ValueKind == JsonValueKind.Number)
                    options.MarginBottomMm = rmb.GetDouble();
                if (root.TryGetProperty("marginLeftMm", out var rml) && rml.ValueKind == JsonValueKind.Number)
                    options.MarginLeftMm = rml.GetDouble();

                decimal ReadDec(string name)
                {
                    if (!root.TryGetProperty(name, out var p)) return 0;
                    if (p.ValueKind == JsonValueKind.Number) return p.GetDecimal();
                    if (p.ValueKind == JsonValueKind.String && decimal.TryParse(p.GetString(), out var d)) return d;
                    return 0;
                }
                options.Subtotal = ReadDec("subtotal");
                options.Vat = ReadDec("vat");
                options.Shipping = ReadDec("shipping");
                options.Discount = ReadDec("discount");
                options.Total = ReadDec("total");

                if (root.TryGetProperty("items", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in arr.EnumerateArray())
                    {
                        string name = el.TryGetProperty("name", out var n) ? n.GetString() : "";
                        int qty = 1;
                        if (el.TryGetProperty("qty", out var q) && q.ValueKind == JsonValueKind.Number)
                            qty = Math.Max(1, q.GetInt32());
                        decimal price = 0, total = 0;
                        if (el.TryGetProperty("price", out var pr))
                        {
                            if (pr.ValueKind == JsonValueKind.Number) price = pr.GetDecimal();
                            else decimal.TryParse(pr.GetString(), out price);
                        }
                        if (el.TryGetProperty("total", out var tot))
                        {
                            if (tot.ValueKind == JsonValueKind.Number) total = tot.GetDecimal();
                            else decimal.TryParse(tot.GetString(), out total);
                        }
                        if (total == 0) total = price * qty;
                        options.Items.Add(new ReceiptPrintItem
                        {
                            Name = name ?? "",
                            Qty = qty,
                            Price = price,
                            Total = total
                        });
                    }
                }

                if (options.Items.Count == 0)
                {
                    PostPrintResult(false, "Cart is empty");
                    return;
                }

                ReceiptPrintHelper.Print(options);
                PostPrintResult(true, null);
            }
            catch (Exception ex)
            {
                PostPrintResult(false, ex.Message);
                MessageBox.Show(this, "Could not print receipt:\n" + ex.Message,
                    ThemeConfig.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void PostPrintResult(bool ok, string message)
        {
            try
            {
                if (_webView?.CoreWebView2 == null) return;
                var msgEsc = (message ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                _webView.CoreWebView2.PostWebMessageAsJson(
                    $"{{\"type\":\"printResult\",\"ok\":{(ok ? "true" : "false")},\"message\":\"{msgEsc}\"}}");
            }
            catch { }
        }

        private void PostWindowState()
        {
            try
            {
                if (_webView?.CoreWebView2 == null) return;
                _webView.CoreWebView2.PostWebMessageAsJson(
                    $"{{\"type\":\"windowState\",\"maximized\":{(_isMaximized ? "true" : "false")}}}");
            }
            catch { }
        }

        private void ShowFatal(string message)
        {
            Opacity = 1;
            _splash.Visible = false;
            MessageBox.Show(this, message, ThemeConfig.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }

        private static async Task<bool> WaitForServerAsync(TimeSpan timeout)
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromMilliseconds(800) };
            var deadline = DateTime.UtcNow + timeout;
            string brandUrl = AppHostConfig.LoopbackUrl.TrimEnd('/') + "/api/brand";
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    using var res = await http.GetAsync(brandUrl);
                    if (!res.IsSuccessStatusCode)
                    {
                        await Task.Delay(40);
                        continue;
                    }

                    string json = await res.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    JsonElement id;
                    if (!doc.RootElement.TryGetProperty("brandId", out id)
                        && !doc.RootElement.TryGetProperty("BrandId", out id))
                    {
                        await Task.Delay(40);
                        continue;
                    }

                    if (string.Equals(id.GetString(), AppHostConfig.BrandId, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                catch { }
                await Task.Delay(40);
            }
            return false;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try { _spinnerTimer?.Stop(); _spinnerTimer?.Dispose(); } catch { }
            try { _webView?.Dispose(); } catch { }
            base.OnFormClosed(e);
            Application.Exit();
        }
    }
}
