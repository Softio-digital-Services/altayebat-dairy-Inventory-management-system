using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using InventorySystem.Controls;
using InventorySystem.Helpers;
using InventorySystem.Services;

namespace InventorySystem
{
    public partial class MainForm : Form
    {
        // NOTE: WS_EX_COMPOSITED removed — it causes see-through rendering artifacts
        // on borderless maximized WinForms windows (other apps show behind the form).
        private Forms.PartsForm partsForm;
        private Forms.UsersForm usersForm;
        private InventorySystem.Forms.DashboardForm dashboardForm;
        private InventorySystem.Forms.ReportsForm reportsForm;
        private InventorySystem.Forms.HistoryForm historyForm;
        private Forms.POSForm posForm;

        // Header Controls
        private PictureBox pbNotification;
        private PictureBox pbUserAvatar;
        private PictureBox pbLanguage;
        private ToolTip _languageToolTip;
        private ContextMenuStrip menuNotifications;
        private Label lblVersion;
        private Label lblDeveloper;


        private Services.DashboardService _dashboardService;
        private System.Windows.Forms.Timer _notificationTimer;
        private int _alertCount = 0;
        private Helpers.Plugins.PluginContext _pluginContext;

        public MainForm()
        {
            InitializeComponent();
            ApplyTheme();
            InitializeNavigation();
            RefineNavigationLayout();
            ApplyPermissions();
            InitializeNotificationSystem();

            panel1.MouseDown += Header_MouseDown;
            label2.MouseDown += Header_MouseDown;

            LocalizationManager.LanguageChanged += (s, e) =>
            {
                if (IsDisposed) return;
                void Run()
                {
                    if (IsDisposed) return;
                    ApplyLocalization();
                }
                if (!IsHandleCreated) return;
                if (InvokeRequired) BeginInvoke((Action)Run);
                else Run();
            };
            ApplyLocalization();

            LoadPlugins();
            this.FormClosed += (s, e) => Helpers.Plugins.PluginManager.ShutdownAll();
        }

        protected override void WndProc(ref Message m)
        {
            BorderlessFormHelper.HandleGetMinMaxInfo(this, ref m);
            base.WndProc(ref m);
        }

        private void LoadPlugins()
        {
            var pnlNav = this.Controls.Find("pnlNav", true).FirstOrDefault() as Panel;
            _pluginContext = new Helpers.Plugins.PluginContext
            {
                ConnectionString = DatabaseConfig.ConnectionString,
                CurrentUser = UserSession.Username,
                UserRole = UserSession.Role,
                IsAdmin = UserSession.IsAdmin,
                CheckLicense = (key) => Helpers.LicenseManager.IsFeatureEnabled(key),
                ShowSuccess = (msg) => MessageHelper.ShowSuccess(msg),
                ShowError = (msg) => MessageHelper.ShowError(msg),
                ShowInfo = (msg) => MessageHelper.ShowInfo(msg),
                AddTab = (tabTitle, iconName, tabOrder, contentFactory, tabId) =>
                {
                    // Filter out Calculator and Backup from sidebar as they are now in the header
                    if (tabTitle.Contains("Calculator") || tabTitle.Contains("Backup") || tabTitle.Contains("\u062d\u0627\u0633\u0628\u0629") || tabTitle.Contains("\u0646\u0633\u062e\u0629")) return;

                    if (this.InvokeRequired) this.Invoke((Action)(() => AddPluginTab(tabTitle, iconName, contentFactory, pnlNav, tabId)));
                    else AddPluginTab(tabTitle, iconName, contentFactory, pnlNav, tabId);
                },
                AddMenuItem = (group, item) =>
                {
                    if (this.InvokeRequired) this.Invoke((Action)(() => AddPluginMenuItem(group, item)));
                    else AddPluginMenuItem(group, item);
                }
            };
            Helpers.Plugins.PluginManager.DiscoverAndLoad(_pluginContext);
        }

        private void AddPluginTab(string tabTitle, string iconName, Func<UserControl> contentFactory, Panel pnlNav, string tabId)
        {
            UserControl cachedContent = null;
            Button btn = CreateNavigationButton(tabTitle, iconName, (s, e) =>
            {
                if (cachedContent == null)
                {
                    cachedContent = contentFactory();
                    ThemeConfig.ApplyGlobalTheme(cachedContent);
                    cachedContent.Dock = DockStyle.Fill;
                    panel3.Controls.Add(cachedContent);
                }
                ShowForm(cachedContent);
            });
            btn.Dock = DockStyle.Top;
            btn.Margin = new Padding(0);
            if (!string.IsNullOrEmpty(tabId)) btn.Name = tabId;
            if (pnlNav != null) { pnlNav.Controls.Add(btn); btn.BringToFront(); }
        }

        private void AddPluginMenuItem(string group, Helpers.Plugins.PluginMenuItem item)
        {
            ContextMenuStrip strip = panel1.Tag as ContextMenuStrip;
            if (strip == null)
            {
                strip = new ContextMenuStrip();
                ThemeConfig.ApplyModernMenuTheme(strip);
                panel1.Tag = strip;
            }
            ToolStripMenuItem groupMenu = null;
            foreach (ToolStripItem si in strip.Items)
            {
                if (si is ToolStripMenuItem tsm && tsm.Text == group) { groupMenu = tsm; break; }
            }
            if (groupMenu == null)
            {
                groupMenu = new ToolStripMenuItem(group);
                groupMenu.Font = ThemeConfig.StandardFont;
                strip.Items.Add(groupMenu);
            }
            if (item.IsSeparator) { groupMenu.DropDownItems.Add(new ToolStripSeparator()); return; }
            var tsItem = new ToolStripMenuItem(item.Label);
            tsItem.Font = ThemeConfig.StandardFont;
            if (!string.IsNullOrEmpty(item.Icon)) tsItem.Image = ThemeConfig.GetNuricon(item.Icon);
            if (item.OnClick != null) tsItem.Click += (s, e) => item.OnClick();
            groupMenu.DropDownItems.Add(tsItem);
        }

        private void ApplyLocalization()
        {
            bool isAr = LocalizationManager.IsArabic;
            SuspendLayout();
            LocalizationManager.ApplyRTL(this);
            Func<string, string> L = LocalizationManager.GetString;

            label2.Text = L("Nav_MainTitle");
            Dashboard_btn.Text = "  " + L("Nav_Dashboard");

            UpdateNavText("btnInventory", "Nav_Inventory");
            UpdateNavText("btnCustomers", "Nav_Customers");
            UpdateNavText("btnSuppliers", "Nav_Suppliers");
            UpdateNavText("btnPOS", "Nav_POS");
            UpdateNavText("btnReports", "Nav_Reports");
            UpdateNavText("btnHistory", "Nav_History");
            UpdateNavText("btnQuotations", "Nav_Quotations");
            UpdateNavText("btnCurrencies", "Nav_Currencies");
            UpdateNavText("btnExpenses", "Nav_Expenses");
            UpdateNavText("btnUsers", "Nav_Users");
            UpdateNavText("btnLabels", "Nav_Barcode");

            button3.Text = "  " + L("Nav_Logout");
            if (itemAddUser != null) itemAddUser.Text = L("Nav_AddUser");
            if (itemLicenseInfo != null) itemLicenseInfo.Text = L("Nav_LicenseInfo");
            if (itemLogout != null) itemLogout.Text = L("Nav_Logout");
            if (itemLogout != null) itemLogout.Text = L("Nav_Logout");
            if (btnLock != null) btnLock.Text = ""; // Icon is set via btnLock.Image below

            if (lblVersion != null)
                lblVersion.Text = string.Format(L("Nav_VersionFooter"), L("Nav_MainTitle"));
            if (lblDeveloper != null)
                lblDeveloper.Text = L("Msg_DevelopedBy");

            if (pbLanguage != null)
            {
                if (_languageToolTip == null) _languageToolTip = new ToolTip();
                _languageToolTip.SetToolTip(pbLanguage, L("Nav_LanguageToolTip"));
            }

            RefreshSidebarButtonLayout();

            var pnlHeaderIcons = this.Controls.Find("rightPanel", true).FirstOrDefault() as Panel;
            if (pnlHeaderIcons != null)
            {
                // Arabic mirrors the whole chrome to the left, matching the modal windows.
                pnlHeaderIcons.Dock = isAr ? DockStyle.Left : DockStyle.Right;
                pnlHeaderIcons.RightToLeft = RightToLeft.No;
                LayoutHeaderChrome(pnlHeaderIcons);
                panel2.Dock = isAr ? DockStyle.Right : DockStyle.Left;
                panel2.RightToLeft = RightToLeft.No;
                var pnlNavRtl = Controls.Find("pnlNav", true).FirstOrDefault();
                if (pnlNavRtl != null) pnlNavRtl.RightToLeft = RightToLeft.No;
            }
            label2.Visible = false; // Forced hide to prevent clipping
            label2.Location = isAr ? new Point(panel1.Width - label2.Width - 10, (panel1.Height - label2.Height) / 2) : new Point(10, (panel1.Height - label2.Height) / 2);

            LocalizationManager.TranslateControl(this);
            ResumeLayout(true);
        }

        // Title-bar chrome metrics. Kept in one place so the bar height, the icon
        // pitch and the strip width can never drift apart.
        private const int HeaderBarHeight = 44;
        private const int HeaderIconSize = 30;
        private const int HeaderWinBtnW = 38;
        private const int HeaderWinBtnH = 28;
        private const int HeaderIconGap = 2;
        private const int HeaderGroupGap = 14;
        private const int HeaderEdgePad = 8;
        private bool _layingOutHeader;

        /// <summary>
        /// Places the title-bar controls in one pass, starting from the window edge the
        /// current language sits on: window buttons outermost, then the tool icons.
        /// Positions are absolute (the strip is pinned to RightToLeft.No) so switching
        /// language cannot mirror an icon into the wrong slot.
        /// </summary>
        private void LayoutHeaderChrome(Panel rightPanel)
        {
            if (rightPanel == null || _layingOutHeader) return;
            _layingOutHeader = true;
            try
            {
                Control Find(string name) => rightPanel.Controls.Find(name, false).FirstOrDefault();

                // Outer edge first, then inward.
                var winButtons = new[] { Find("btnWinClose"), Find("btnWinMax"), Find("btnWinMin") };
                var toolIcons = new Control[]
                {
                    pbUserAvatar, pbNotification, btnLock,
                    Find("headerIcon_calc"), Find("headerIcon_backup"),
                    Find("headerIcon_currencies"), Find("headerIcon_about"), pbLanguage
                };

                int total = HeaderEdgePad * 2 + HeaderGroupGap;
                foreach (var b in winButtons)
                    if (b != null) total += HeaderWinBtnW;
                foreach (var t in toolIcons)
                    if (t != null) total += HeaderIconSize + HeaderIconGap;

                rightPanel.Width = total;
                int h = rightPanel.ClientSize.Height > 0 ? rightPanel.ClientSize.Height : HeaderBarHeight;
                bool isAr = LocalizationManager.IsArabic;

                // cursor walks inward from the outer edge in both directions
                int cursor = isAr ? HeaderEdgePad : rightPanel.Width - HeaderEdgePad;

                void Place(Control c, int cw, int ch)
                {
                    if (c == null) return;
                    c.Size = new Size(cw, ch);
                    int x = isAr ? cursor : cursor - cw;
                    c.Location = new Point(x, Math.Max(0, (h - ch) / 2));
                    cursor += isAr ? cw : -cw;
                }

                foreach (var b in winButtons)
                    Place(b, HeaderWinBtnW, HeaderWinBtnH);

                cursor += isAr ? HeaderGroupGap : -HeaderGroupGap;

                foreach (var t in toolIcons)
                {
                    Place(t, HeaderIconSize, HeaderIconSize);
                    cursor += isAr ? HeaderIconGap : -HeaderIconGap;
                }

                pbLanguage?.BringToFront();
            }
            finally
            {
                _layingOutHeader = false;
            }
        }

        private void RefreshSidebarButtonLayout()
        {
            bool dashActive = Dashboard_btn?.Tag != null && Dashboard_btn.Tag is bool d && d;
            ThemeConfig.ApplySidebarButtonIcon(Dashboard_btn, Dashboard_btn?.Image, dashActive);

            string[] navIds = { "btnInventory", "btnCustomers", "btnSuppliers", "btnPOS", "btnReports", "btnHistory", "btnQuotations", "btnCurrencies", "btnExpenses", "btnUsers", "btnLabels" };
            foreach (string id in navIds)
            {
                var found = Controls.Find(id, true);
                if (found.Length > 0 && found[0] is Button nb)
                {
                    bool active = nb.Tag != null && nb.Tag is bool b && b;
                    ThemeConfig.ApplySidebarButtonIcon(nb, nb.Image, active);
                }
            }

            if (button3 != null)
            {
                // Same RTL rule as ApplySidebarButtonIcon — avoid flipping danger-styled logout colors.
                bool isAr = LocalizationManager.IsArabic;
                button3.RightToLeft = RightToLeft.No;
                button3.TextAlign = isAr ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
                button3.ImageAlign = isAr ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
                button3.TextImageRelation = isAr ? TextImageRelation.TextBeforeImage : TextImageRelation.ImageBeforeText;
                button3.Padding = new Padding(isAr ? 0 : 15, 0, isAr ? 15 : 0, 0);
            }

            var pnlNav = Controls.Find("pnlNav", true).FirstOrDefault() as Panel;
            if (pnlNav != null)
            {
                foreach (Control c in pnlNav.Controls)
                {
                    if (c is Button sb && sb != Dashboard_btn && sb != button3)
                    {
                        bool active = sb.Tag != null && sb.Tag is bool b && b;
                        ThemeConfig.ApplySidebarButtonIcon(sb, sb.Image, active);
                    }
                }
            }
        }

        private void UpdateNavText(string name, string key)
        {
            var btns = this.Controls.Find(name, true);
            if (btns.Length > 0) btns[0].Text = "  " + LocalizationManager.GetString(key);
        }

        private void InitializeNavigation()
        {
            ThemeConfig.ApplyFormIcon(this);
            Panel pnlNav = new Panel { Name = "pnlNav", Dock = DockStyle.Fill, Padding = new Padding(0), BackColor = Color.Transparent, AutoScroll = true };
            panel2.Controls.Add(pnlNav);
            pnlNav.BringToFront();

            // Dedicated Logo Container to prevent layout overrides and ensure margins
            Panel pnlLogoContainer = new Panel
            {
                Name = "pnlLogoContainer",
                Dock = DockStyle.Top,
                Height = 105, // Container height adjusted to allow bottom gap
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 25) // 10px top margin and 25px bottom gap to Dashboard button
            };
            pnlNav.Controls.Add(pnlLogoContainer);
            pnlLogoContainer.BringToFront();

            Label lblSidebarBrand = new Label
            {
                Name = "lblSidebarBrand",
                Dock = DockStyle.Fill,
                Text = ThemeConfig.CompanyName,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ThemeConfig.PrimaryColor,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            pnlLogoContainer.Controls.Add(lblSidebarBrand);
            // In WinForms Dock=Top: The control with the HIGHEST z-order index is at the top.
            // BringToFront sets index to 0. SendToBack sets to last.
            // So for A to be above B: A should have higher index than B.
            // A.SendToBack() makes it top if it's the first one. 
            // Let's just use the simplest logic: Add them and BringToFront each.
            // If I add Logo then BringToFront: Logo is index 0.
            // If I add Dashboard then BringToFront: Dashboard is 0, Logo is 1.
            // Now Logo (1) is ABOVE Dashboard (0).
            // This is exactly what we want.

            // Dashboard - wrapped in try/catch so a DB error never crashes MainForm
            try
            {
                dashboardForm = new InventorySystem.Forms.DashboardForm { Dock = DockStyle.Fill };
                ThemeConfig.ApplyGlobalTheme(dashboardForm);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Dashboard init error: " + ex.Message);
                dashboardForm = new InventorySystem.Forms.DashboardForm();
                dashboardForm.Dock = DockStyle.Fill;
            }
            panel3.Controls.Add(dashboardForm);
            panel3.Controls.Add(dashboardForm);

            // Consolidate Dashboard Button logic to avoid redundant subscriptions and visual lag
            Dashboard_btn.Click -= button1_Click;
            Dashboard_btn.Click += (s, e) =>
            {
                ShowForm(dashboardForm);
                HighlightSelectedButton(Dashboard_btn);
            };

            Image dashIcon = ThemeConfig.GetNuricon("dashboard");
            ThemeConfig.ApplySidebarButtonIcon(Dashboard_btn, dashIcon != null ? ResizeImage(dashIcon, 18, 18) : null, false);
            Dashboard_btn.Text = "  " + LocalizationManager.GetString("Nav_Dashboard");

            // Forms Setup
            usersForm = InitializeForm<Forms.UsersForm>();
            partsForm = InitializeForm<Forms.PartsForm>();
            posForm = InitializeForm<Forms.POSForm>();
            reportsForm = InitializeForm<Forms.ReportsForm>();
            historyForm = InitializeForm<Forms.HistoryForm>();

            // Navigation Buttons
            Dashboard_btn.Height = 50;
            Dashboard_btn.Width = 225; // Force exact width matching panel2
            Dashboard_btn.Margin = new Padding(0);
            Dashboard_btn.Dock = DockStyle.Top;
            Dashboard_btn.Text = "  " + LocalizationManager.GetString("Nav_Dashboard");
            Dashboard_btn.Image = ResizeImage(ThemeConfig.GetNuricon("dashboard"), 18, 18);
            ThemeConfig.ApplySidebarButtonIcon(Dashboard_btn, Dashboard_btn.Image, false);
            Dashboard_btn.BringToFront(); // Place below logo
            pnlNav.Controls.Add(Dashboard_btn);
            Dashboard_btn.BringToFront();

            bool isAdmin = UserSession.IsAdmin;
            bool isAccountant = UserSession.IsAccountant;
            bool isWorker = UserSession.IsStaff;

            // Worker can see POS, Inventory
            if (isAdmin || isWorker || isAccountant) AddNavButton(pnlNav, "Nav_Inventory", "inventory", "btnInventory", () => ShowForm(partsForm));
            if (isAdmin || isWorker) AddNavButton(pnlNav, "Nav_POS", "pos", "btnPOS", () => ShowForm(posForm));

            // Reports & History — admin, staff, accountant
            if (isAdmin || isWorker || isAccountant)
            {
                AddNavButton(pnlNav, "Nav_Reports", "reports", "btnReports", () => { reportsForm.RefreshData(); ShowForm(reportsForm); });
                AddNavButton(pnlNav, "Nav_History", "history", "btnHistory", () => { historyForm.LoadHistory(); ShowForm(historyForm); });
            }

            // Admin Only
            if (isAdmin)
            {
                AddNavButton(pnlNav, "Nav_Users", "users", "btnUsers", () => ShowForm(usersForm));
            }

            ShowForm(dashboardForm);
            HighlightSelectedButton(Dashboard_btn);
        }

        private T InitializeForm<T>() where T : UserControl, new()
        {
            T f = new T { Dock = DockStyle.Fill, Visible = false };
            ThemeConfig.ApplyGlobalTheme(f);
            panel3.Controls.Add(f);
            return f;
        }

        private void AddNavButton(Panel pnl, string resourceKey, string icon, string name, Action clickAction)
        {
            string text = LocalizationManager.GetString(resourceKey);
            Button btn = CreateNavigationButton(text, icon, (s, e) => clickAction());
            btn.Name = name; btn.Dock = DockStyle.Top; btn.Margin = new Padding(0);
            pnl.Controls.Add(btn);
            btn.BringToFront(); // Stack below previous items
        }

        private void RefineNavigationLayout()
        {
            panel2.Controls.Remove(label4); label4.Visible = false;
            // Re-order panel2 to ensure pnlNav fills the remaining space
            Panel pnlNav = panel2.Controls.Find("pnlNav", true).FirstOrDefault() as Panel;
            if (pnlNav != null) panel2.Controls.Remove(pnlNav);

            // Now re-add pnlNav to fill the REMAINING space
            if (pnlNav != null)
            {
                panel2.Controls.Add(pnlNav);
                pnlNav.Dock = DockStyle.Fill;
                pnlNav.BringToFront();
            }

            // Logout Button - Moved to the extreme bottom of the sidebar
            button3.Parent = panel2;
            button3.Dock = DockStyle.Bottom;
            button3.SendToBack(); // Puts it at the absolute bottom edge
            button3.Height = 50;
            button3.Text = "  " + LocalizationManager.GetString("Nav_Logout");
            button3.ForeColor = ThemeConfig.DangerColor;
            Image logoutIcon = ThemeConfig.GetNuricon("logout");
            if (logoutIcon != null) { button3.Image = ResizeImage(logoutIcon, 22, 22); }
            button3.Font = Dashboard_btn.Font;
            button3.FlatAppearance.MouseOverBackColor = ThemeConfig.DangerLight;

            SetupHeaderIcons();
            SetupFooter();
            RefreshSidebarButtonLayout();
        }

        private void SetupFooter()
        {
            Panel pnlFooter = new Panel
            {
                Name = "pnlFooter",
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = ThemeConfig.SurfaceColor,
                Padding = new Padding(15, 0, 15, 0)
            };

            lblVersion = new Label
            {
                Text = string.Format(LocalizationManager.GetString("Nav_VersionFooter"), LocalizationManager.GetString("Nav_MainTitle")),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ThemeConfig.TextColorDark,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Left
            };
            pnlFooter.Controls.Add(lblVersion);

            lblDeveloper = new Label
            {
                Text = LocalizationManager.GetString("Msg_DevelopedBy", "Developed by Softio"),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = ThemeConfig.PrimaryColor,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Right
            };
            pnlFooter.Controls.Add(lblDeveloper);

            this.Controls.Add(pnlFooter);

            // Docked children are laid out from the highest z-order index down, so the
            // Fill panel must sit at index 0. Re-adding an existing child only sends it
            // to the back, which left panel3 above the footer and let page content run
            // 30px under it - the footer then painted over card bottoms and borders.
            this.Controls.SetChildIndex(panel3, 0);
            this.Controls.SetChildIndex(pnlFooter, 1);
            this.Controls.SetChildIndex(panel2, 2);
            this.Controls.SetChildIndex(panel1, 3);
        }

        private void SetupHeaderIcons()
        {
            // The strip docks to whichever side the language wants; LayoutHeaderChrome
            // owns every size and position so both directions stay in step.
            Panel rightPanel = new Panel
            {
                Name = "rightPanel",
                BackColor = Color.Transparent,
                Dock = LocalizationManager.IsArabic ? DockStyle.Left : DockStyle.Right,
                RightToLeft = RightToLeft.No
            };

            AddHeaderButton(rightPanel, "Close", "btnWinClose", () => Application.Exit());
            AddHeaderButton(rightPanel, "Maximize", "btnWinMax", () => { this.WindowState = this.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized; });
            AddHeaderButton(rightPanel, "Minimize", "btnWinMin", () => this.WindowState = FormWindowState.Minimized);

            pbUserAvatar = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("user"), Color.White) };
            ThemeConfig.ApplyHeaderIconStyle(pbUserAvatar);
            pbUserAvatar.Click += (s, e) => menuUser.Show(pbUserAvatar, new Point(0, pbUserAvatar.Height));
            rightPanel.Controls.Add(pbUserAvatar);

            pbNotification = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("bell"), Color.White) };
            ThemeConfig.ApplyHeaderIconStyle(pbNotification);
            pbNotification.Paint += (s, e) =>
            {
                if (_alertCount <= 0) return;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using SolidBrush b = new SolidBrush(ThemeConfig.DangerColorBright);
                e.Graphics.FillEllipse(b, pbNotification.Width - 12, 4, 7, 7);
            };
            pbNotification.Click += (s, e) => ShowNotifications(s, e);
            rightPanel.Controls.Add(pbNotification);

            btnLock.Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("lock"), Color.White);
            btnLock.SizeMode = PictureBoxSizeMode.Zoom;
            btnLock.Anchor = AnchorStyles.None;
            ThemeConfig.ApplyHeaderIconStyle(btnLock);
            btnLock.Click += (s, e) => BtnLock_Click(s, e);
            rightPanel.Controls.Add(btnLock);

            // Calculator
            PictureBox pbCalc = new PictureBox { Name = "headerIcon_calc", SizeMode = PictureBoxSizeMode.Zoom, Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("calculator"), Color.White) };
            ThemeConfig.ApplyHeaderIconStyle(pbCalc);
            pbCalc.Click += (s, e) => ShowInPopup(new Plugins.CalculatorPanel(), LocalizationManager.GetString("Plugins_CalcTitle", "Calculator"), 380, 580);
            rightPanel.Controls.Add(pbCalc);

            // Backup
            PictureBox pbBackup = new PictureBox { Name = "headerIcon_backup", SizeMode = PictureBoxSizeMode.Zoom, Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("backup"), Color.White) };
            ThemeConfig.ApplyHeaderIconStyle(pbBackup);
            pbBackup.Click += (s, e) => ShowInPopup(new Plugins.BackupPanel(_pluginContext), LocalizationManager.GetString("Plugins_BackupRestore", "Backup & Restore"), 520, 500);
            rightPanel.Controls.Add(pbBackup);

            // Currencies
            PictureBox pbCurrencies = new PictureBox { Name = "headerIcon_currencies", SizeMode = PictureBoxSizeMode.Zoom, Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("currencies"), Color.White) };
            ThemeConfig.ApplyHeaderIconStyle(pbCurrencies);
            pbCurrencies.Click += (s, e) => { using (var f = new Forms.CurrencySettingsForm()) f.ShowDialog(this); };
            rightPanel.Controls.Add(pbCurrencies);

            // About Us
            PictureBox pbAbout = new PictureBox { Name = "headerIcon_about", SizeMode = PictureBoxSizeMode.Zoom, Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("info"), Color.White) };
            ThemeConfig.ApplyHeaderIconStyle(pbAbout, 0.62f);
            ToolTip ttAbout = new ToolTip(); ttAbout.SetToolTip(pbAbout, LocalizationManager.GetString("Nav_AboutUs", "About Us"));
            pbAbout.Click += (s, e) => { using (var f = new Forms.AboutUsForm()) f.ShowDialog(this); };
            rightPanel.Controls.Add(pbAbout);

            // Language Switcher
            pbLanguage = new PictureBox
            {
                Name = "pbLanguage",
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("language"), Color.White)
            };
            ThemeConfig.ApplyHeaderIconStyle(pbLanguage);
            pbLanguage.Click += (s, e) =>
            {
                string nextLang = LocalizationManager.IsArabic ? "en-US" : "ar";
                LocalizationManager.SetLanguage(nextLang);
            };
            rightPanel.Controls.Add(pbLanguage);
            pbLanguage.BringToFront();

            rightPanel.SizeChanged += (s, e) => LayoutHeaderChrome(rightPanel);
            panel1.Controls.Add(rightPanel);
            LayoutHeaderChrome(rightPanel);
            pbLanguage.BringToFront();
        }

        private void ShowInPopup(UserControl control, string title, int width, int height)
        {
            using (var f = new Forms.BaseModalForm())
            {
                f.TitleText = title;
                f.Width = width;
                f.Height = height + 70; // Header offset

                control.Dock = DockStyle.Fill;
                f.ContentPanel.Controls.Add(control);

                // BaseModalForm.OnLoad will call FitToContent() which will expand if needed
                f.ShowDialog(this);
            }
        }

        private void AddHeaderButton(Panel p, string type, string name, Action click)
        {
            Button b = new Button { Name = name, Size = new Size(HeaderWinBtnW, HeaderWinBtnH) };
            ThemeConfig.ApplyWindowControl(b, type); b.Click += (s, e) => click();
            p.Controls.Add(b);
        }

        private LockOverlay _lockOverlay;
        private void BtnLock_Click(object sender, EventArgs e)
        {
            if (_lockOverlay == null)
            {
                _lockOverlay = new LockOverlay();
                _lockOverlay.Unlocked += (s, ev) =>
                {
                    _lockOverlay.Visible = false;
                    this.Controls.Remove(_lockOverlay);
                    _lockOverlay.Dispose();
                    _lockOverlay = null;
                };
            }

            if (!this.Controls.Contains(_lockOverlay))
            {
                this.Controls.Add(_lockOverlay);
                _lockOverlay.BringToFront();
            }
            _lockOverlay.Visible = true;
            _lockOverlay.Focus();
        }

        private void ShowNotifications(object sender, EventArgs e)
        {
            if (menuNotifications == null) { menuNotifications = new ContextMenuStrip(); ThemeConfig.ApplyModernMenuTheme(menuNotifications); }
            menuNotifications.Items.Clear();
            var notifications = _dashboardService.GetNotifications();
            bool isAr = LocalizationManager.IsArabic;
            if (notifications.Count == 0) menuNotifications.Items.Add(LocalizationManager.GetString("Main_NoNotifications")).Enabled = false;
            else
            {
                foreach (var n in notifications)
                {
                    var item = new ToolStripMenuItem($"{n.Title}: {n.Message}") { Tag = n, Font = ThemeConfig.StandardFont, Image = ThemeConfig.GetNuricon(n.Type == "LowStock" || n.Type == "OutOfStock" ? "warning" : "check") };
                    item.Click += (s, ev) =>
                    {
                        if (n.Target == "btnInventory") ShowForm(partsForm);
                        else if (n.Target == "btnCustomers") ClickNavButton("btnCustomers");
                        else if (n.Target == "btnSuppliers") ClickNavButton("btnSuppliers");
                        else ShowForm(dashboardForm);
                    };
                    menuNotifications.Items.Add(item);
                }
            }
            menuNotifications.Show(sender as Control, new Point(0, (sender as Control).Height));
        }

        private void InitializeNotificationSystem()
        {
            _dashboardService = new Services.DashboardService();
            var expenseService = new Services.ExpenseService();
            expenseService.ProcessRecurringExpenses(); // Check for month-end expenses

            _notificationTimer = new System.Windows.Forms.Timer { Interval = 30000 };
            _notificationTimer.Tick += (s, e) => RefreshNotificationBadge(); _notificationTimer.Start(); RefreshNotificationBadge();
        }

        private void RefreshNotificationBadge()
        {
            int oldCount = _alertCount;
            _alertCount = _dashboardService.GetLowStockCount();
            if (oldCount != _alertCount && pbNotification != null) pbNotification.Invalidate();
        }

        private void ApplyTheme()
        {
            this.Text = ThemeConfig.AppTitle;

            label2.Text = ThemeConfig.AppTitle;
            label2.Font = ThemeConfig.HeaderFont;
            label2.ForeColor = Color.White; // New: White on Blue
            label2.Visible = false; // Explicitly hide the title
            panel1.Height = HeaderBarHeight;
            panel1.Padding = Padding.Empty;
            panel1.Margin = Padding.Empty;
            panel1.BorderStyle = BorderStyle.None; // Remove border that adds padding
            label1.Text = string.Format(LocalizationManager.GetString("WelcomeUser", "Welcome, {0}"), UserSession.FullName, UserSession.Role);
            label1.Font = ThemeConfig.SmallBoldFont;
            label1.ForeColor = Color.FromArgb(180, 255, 255, 255); // Subtle white

            panel1.BackColor = ThemeConfig.HeaderColor; // Theme Red Header
            panel1.Paint += (s, e) =>
            {
                // No border needed for deep blue header
            };

            panel2.BackColor = Color.FromArgb(248, 250, 252); // Light Gray Sidebar
            panel3.BackColor = ThemeConfig.BackgroundColor;

            itemAddUser.Click += ItemAddUser_Click;
            itemLicenseInfo.Click += ItemLicenseInfo_Click;
            itemLogout.Click += ItemLogout_Click;
            btnLock.Click += BtnLock_Click;
        }

        private Button CreateNavigationButton(string text, string iconName, EventHandler clickHandler)
        {
            SidebarButton btn = new SidebarButton
            {
                Height = 50,
                Dock = DockStyle.Top,
                Text = "  " + text,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Image icon = ThemeConfig.GetNuricon(iconName);
            if (icon != null) btn.Image = ResizeImage(icon, 18, 18);
            ThemeConfig.ApplySidebarButtonIcon(btn, btn.Image, false);
            btn.Click += clickHandler; btn.Click += (s, e) => HighlightSelectedButton(btn);
            return btn;
        }

        private Button selectedButton = null;
        private void HighlightSelectedButton(Button btn)
        {
            if (selectedButton == btn) return;
            if (selectedButton != null)
            {
                ThemeConfig.ApplySidebarButtonIcon(selectedButton, selectedButton.Image, false);
                selectedButton.Tag = false;
                selectedButton.Paint -= DrawSelectionBorder;
                selectedButton.Invalidate();
            }
            selectedButton = btn;
            ThemeConfig.ApplySidebarButtonIcon(selectedButton, selectedButton.Image, true);
            selectedButton.Tag = true;
            selectedButton.Paint += DrawSelectionBorder;
            selectedButton.Invalidate();
            selectedButton.Update();
            RefreshSidebarButtonLayout();
        }

        private void DrawSelectionBorder(object sender, PaintEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null && (bool)btn.Tag)
            {
                using (Pen pen = new Pen(ThemeConfig.PrimaryColor, 5))
                {
                    if (LocalizationManager.IsArabic)
                        e.Graphics.DrawLine(pen, btn.Width - 1, 0, btn.Width - 1, btn.Height);
                    else
                        e.Graphics.DrawLine(pen, 0, 0, 0, btn.Height);
                }
            }
        }



        private void ClickNavButton(string id)
        {
            var btns = this.Controls.Find(id, true);
            if (btns.Length > 0 && btns[0] is Button btn) { btn.PerformClick(); }
        }

        /// <summary>Switches the shell to a sidebar tab by button name (e.g. btnQuotations).</summary>
        public void NavigateToTab(string buttonName)
        {
            if (string.IsNullOrEmpty(buttonName)) return;
            ClickNavButton(buttonName);
        }

        private void ShowForm(UserControl form)
        {
            foreach (Control c in panel3.Controls) if (c is UserControl) c.Visible = false;
            form.Visible = true; form.BringToFront();
            panel3.Focus(); // Focus the main panel to prevent auto-selecting the first control (like search bar) in the UserControl
            if (form is InventorySystem.Forms.DashboardForm dash) dash.RefreshDashboard();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (posForm != null && posForm.Visible)
            {
                if (posForm.HandleKeyPress(keyData))
                {
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ItemAddUser_Click(object sender, EventArgs e) { new Forms.AddUserForm().ShowDialog(this); }
        private void ItemLicenseInfo_Click(object sender, EventArgs e) { new Forms.LicenseInfoForm().ShowDialog(this); }
        private void ItemLogout_Click(object sender, EventArgs e) { button3_Click_1(sender, e); }
        private void button1_Click(object sender, EventArgs e) { ShowForm(dashboardForm); }
        private void ApplyPermissions()
        {
            bool isAdmin = UserSession.IsAdmin;
            bool isStaff = UserSession.Role == "Staff";
            bool isAccountant = UserSession.Role == "Accountant";

            // Sidebar Buttons Hide Logic
            SetNavVisibility("btnReports", isAdmin || isStaff || isAccountant);
            SetNavVisibility("btnCurrencies", isAdmin);
            SetNavVisibility("btnHistory", isAdmin || isStaff || isAccountant);
            SetNavVisibility("btnQuotations", isAdmin || isStaff || isAccountant);
            SetNavVisibility("btnExpenses", isAdmin || isStaff || isAccountant);

            if (isAccountant)
            {
                // Hide most except POS and History/PO maybe? 
                // Based on previous logic:
                string[] toHide = { "Dashboard_btn", "btnInventory", "btnCustomers", "btnSuppliers", "btnReports", "btnQuotations", "btnCurrencies" };
                foreach (string name in toHide) SetNavVisibility(name, false);

                // Auto-show POS
                var btnPOS = this.Controls.Find("btnPOS", true);
                if (btnPOS.Length > 0) { ShowForm(posForm); HighlightSelectedButton((Button)btnPOS[0]); }
            }

            if (!isAdmin && pbUserAvatar != null) pbUserAvatar.Visible = false;
            label1.Text = string.Format(LocalizationManager.GetString("WelcomeUser", "Welcome, {0} ({1})"), UserSession.FullName, UserSession.Role);
        }

        private void SetNavVisibility(string name, bool visible)
        {
            var ctrls = this.Controls.Find(name, true);
            if (ctrls.Length > 0) ctrls[0].Visible = visible;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (MessageHelper.ConfirmAction(LocalizationManager.GetString("Msg_ConfirmLogout", "Logout?"))) { new LoginForm().Show(); this.Hide(); }
        }

        [System.Runtime.InteropServices.DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void Header_MouseDown(object sender, MouseEventArgs e) { if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(this.Handle, 0x112, 0xf012, 0); } }

        private Image ResizeImage(Image img, int width, int height)
        {
            Bitmap b = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, width, height);
            }
            return b;
        }
    }
}


