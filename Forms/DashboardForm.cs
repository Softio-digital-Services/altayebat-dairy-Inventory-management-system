using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting; // Standard Charting
using InventorySystem.Services;
using InventorySystem.Controls;
using InventorySystem.Helpers;
using System.Collections.Generic;
using System.Data;

namespace InventorySystem.Forms
{
    public partial class DashboardForm : UserControl
    {
        // Service
        private DashboardService _dashboardService;
        
        // Layout
        private TableLayoutPanel _mainLayout;
        private TableLayoutPanel _cardsLayout;
        private TableLayoutPanel _middleLayout;
        
        // Controls
        private StatCard _cardInventory;
        private StatCard _cardRevenue;
        private StatCard _cardOrders;
        private StatCard _cardLowStock;
        
        private Chart _chartWeeklyRevenue;
        private Chart _chartTrends; // Bottom Chart
        
        // private Panel _feedPanel; // For Recent Activity or Top Items
        private DataGridView _gridTopItems; // If using grid
        private Label lblDashboardTitle;
        private Label _lblTop;
        private Label _lblTrend;
        private Panel _pnlHeaderActions;
        private Label _lblServerUrl;
        private ModernButton _btnScan;
        private System.ComponentModel.IContainer components = null;

        public DashboardForm()
        {
            _dashboardService = new DashboardService();
            InitializeComponent();
            InitializeDashboardLayout();
            ApplyLocalization();
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
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            LoadData();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Size = new System.Drawing.Size(1200, 800);
        }

        private void ApplyLocalization()
        {
            Func<string, string> L = InventorySystem.Helpers.LocalizationManager.GetString;

            if (this.lblDashboardTitle != null) this.lblDashboardTitle.Text = L("Dash_Title");
            
            // Re-detect controls if they were created dynamically
            var btnScan = this.Controls.Find("btnScan", true).FirstOrDefault() as Button;
            if (btnScan != null) btnScan.Text = L("Dash_ScanToConnect");
            LayoutHeaderActions();
            // The form-wide RTL pass may mirror these absolute positions afterwards.
            if (IsHandleCreated) BeginInvoke((Action)LayoutHeaderActions);

            if (_cardInventory != null) _cardInventory.Title = L("Dash_TotalInventory");
            if (_cardRevenue != null) _cardRevenue.Title = L("Dash_TotalRevenue");
            if (_cardOrders != null) _cardOrders.Title = L("Dash_TotalOrders");
            if (_cardLowStock != null) _cardLowStock.Title = L("Dash_LowStock");

            var lblWeekly = this.Controls.Find("lblTitleWeekly", true).FirstOrDefault() as Label;
            if (lblWeekly != null) lblWeekly.Text = L("Dash_WeeklyRevenue");

            var lblTopTitle = this.Controls.Find("lblTitleTop", true).FirstOrDefault() as Label;
            if (lblTopTitle != null) lblTopTitle.Text = L("Dash_TopSelling");

            var lblTrendTitle = this.Controls.Find("lblTitleTrend", true).FirstOrDefault() as Label;
            if (lblTrendTitle != null) lblTrendTitle.Text = L("Dash_SalesTrends");

            if (_chartWeeklyRevenue != null && _chartWeeklyRevenue.Titles.Count > 0) 
                _chartWeeklyRevenue.Titles[0].Text = L("Dash_WeeklyRevenue");
            
            if (_chartTrends != null && _chartTrends.Titles.Count > 0) 
                _chartTrends.Titles[0].Text = L("Dash_MonthlyTrends");

            LoadData(); // refresh data strings
        }


        public void RefreshDashboard()
        {
            LoadData();
        }

        /// <summary>
        /// Arabic puts the scan button at the left edge with the address to its right;
        /// English keeps the address first. Positions are set by hand so the pair never
        /// depends on flow or column mirroring.
        /// </summary>
        private void LayoutHeaderActions()
        {
            if (_pnlHeaderActions == null || _lblServerUrl == null || _btnScan == null) return;

            _pnlHeaderActions.RightToLeft = RightToLeft.No;
            _lblServerUrl.RightToLeft = RightToLeft.No;

            const int gap = 12;
            int urlW = _lblServerUrl.PreferredSize.Width;
            int urlH = _lblServerUrl.PreferredSize.Height;
            int h = _pnlHeaderActions.Height;

            _pnlHeaderActions.Width = _btnScan.Width + gap + urlW;

            int urlY = Math.Max(0, (h - urlH) / 2);
            int btnY = Math.Max(0, (h - _btnScan.Height) / 2);

            if (LocalizationManager.IsArabic)
            {
                _btnScan.Location = new Point(0, btnY);
                _lblServerUrl.Location = new Point(_btnScan.Width + gap, urlY);
            }
            else
            {
                _lblServerUrl.Location = new Point(0, urlY);
                _btnScan.Location = new Point(urlW + gap, btnY);
            }
        }


        private void InitializeDashboardLayout()
        {
            // Main Container
            _mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(20),
                BackColor = ThemeConfig.BackgroundColor // Horizon Light Gray
            };

            
            // Row Styles
            // Row Styles
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // Header
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F)); // Cards
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));   // Middle (Bar Chart + List)
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));   // Bottom (Line Chart)
            
            this.Controls.Add(_mainLayout);

            lblDashboardTitle = ThemeConfig.CreateStandardHeader(LocalizationManager.GetString("Dash_Title"));
            lblDashboardTitle.Name = "lblDashboardTitle";

            // Live server URL label
            string serverUrl = ScanToConnectForm.GetServerUrl();
            var lblServerUrl = new Label
            {
                Text = "📶 " + serverUrl,
                Font = new Font("Segoe UI", 10f),
                ForeColor = ThemeConfig.PrimaryColor,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                // The address is always latin text, so keep it out of the RTL mirror.
                RightToLeft = RightToLeft.No
            };
            _lblServerUrl = lblServerUrl;
            
            lblServerUrl.Click += (s, e) => {
                Clipboard.SetText(serverUrl);
                string originalText = lblServerUrl.Text;
                lblServerUrl.Text = "✅ " + (LocalizationManager.GetString("Msg_Copied", "Copied!"));
                lblServerUrl.ForeColor = ThemeConfig.PrimaryColor;
                
                System.Windows.Forms.Timer t = new System.Windows.Forms.Timer { Interval = 1500 };
                t.Tick += (ts, te) => {
                    lblServerUrl.Text = originalText;
                    lblServerUrl.ForeColor = ThemeConfig.PrimaryColor;
                    t.Stop();
                    t.Dispose();
                };
                t.Start();
            };

            // Scan-to-Connect button
            var btnScan = new ModernButton
            {
                Name = "btnScan", 
                Text = LocalizationManager.GetString("Dash_ScanToConnect"),
                Width = 145,
                Height = 35
            };
            ThemeConfig.ApplyPrimaryButton(btnScan);
            btnScan.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnScan.Click += (s, e) => new ScanToConnectForm().ShowDialog();
            _btnScan = btnScan;

            // Both pieces live in one manually positioned host: the header helper puts its
            // children in a FlowLayoutPanel whose direction gets mirrored for Arabic, which
            // reversed link/button independently of what we want.
            _pnlHeaderActions = new Panel
            {
                Name = "pnlHeaderActions",
                AutoSize = false,
                Height = 35,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 6, 0, 0)
            };
            _pnlHeaderActions.Controls.Add(lblServerUrl);
            _pnlHeaderActions.Controls.Add(btnScan);
            LayoutHeaderActions();

            var headerControls = new Control[] { _pnlHeaderActions };
            TableLayoutPanel tlpHeader = ThemeConfig.CreateGlobalFormHeader(lblDashboardTitle, null, headerControls);
            _mainLayout.Controls.Add(tlpHeader, 0, 0);

            // 1. Cards Layout (Top)
            _cardsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 15), // Spacing below cards
                BackColor = Color.Transparent
            };
            for (int i = 0; i < 4; i++) _cardsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            
            // Create Cards
            _cardInventory = CreateStatCard(LocalizationManager.GetString("Dash_TotalInventory"), "inventory_dashboard", ThemeConfig.PrimaryColor); 
            _cardRevenue = CreateStatCard(LocalizationManager.GetString("Dash_TotalRevenue"), "revenue", ThemeConfig.PrimaryColor); 
            _cardOrders = CreateStatCard(LocalizationManager.GetString("Dash_TotalOrders"), "orders", ThemeConfig.WarningColor); 
            _cardLowStock = CreateStatCard(LocalizationManager.GetString("Dash_LowStock"), "bell_dashboard", ThemeConfig.DangerColor); 


            _cardsLayout.Controls.Add(_cardInventory, 0, 0);
            _cardsLayout.Controls.Add(_cardRevenue, 1, 0);
            _cardsLayout.Controls.Add(_cardOrders, 2, 0);
            _cardsLayout.Controls.Add(_cardLowStock, 3, 0);
            
            _mainLayout.Controls.Add(_cardsLayout, 0, 1);

            // 2. Middle Section (Charts + Feed)
            _middleLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 15),
                BackColor = Color.Transparent
            };
            _middleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F)); 
            _middleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F)); 
            
            // Bar Chart Card  (with title label above the chart)
            _chartWeeklyRevenue = CreateModernChart();
            Panel pnlWeeklyContent = new Panel { Dock = DockStyle.Fill };
            Label lblWeeklyTitle = new Label
            {
                Name = "lblTitleWeekly",
                Text = LocalizationManager.GetString("Dash_WeeklyRevenue"),
                Font = ThemeConfig.SubHeaderFont,
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = ThemeConfig.TextColorDark
            };
            pnlWeeklyContent.Controls.Add(_chartWeeklyRevenue);
            pnlWeeklyContent.Controls.Add(lblWeeklyTitle);
            lblWeeklyTitle.BringToFront();
            Panel pnlWeeklyCard = ThemeConfig.CreateCardPanel(pnlWeeklyContent);
            pnlWeeklyCard.Margin = new Padding(0, 0, 10, 0);
            _middleLayout.Controls.Add(pnlWeeklyCard, 0, 0);
            
            // Top Items Grid Card
            Panel rightContent = new Panel { Dock = DockStyle.Fill };
            _lblTop = new Label 
            { 
                Name = "lblTitleTop",
                Text = LocalizationManager.GetString("Dash_TopSelling"), 
                Font = ThemeConfig.SubHeaderFont, 
                Dock = DockStyle.Top, 
                Height = 30,
                ForeColor = ThemeConfig.TextColorDark 
            };
            rightContent.Controls.Add(_lblTop);
            
            _gridTopItems = new DataGridView();
            _gridTopItems.DataError += (s, e) => { e.ThrowException = false; };
            _gridTopItems.AllowUserToAddRows = false;
            _gridTopItems.ReadOnly = true;
            ThemeConfig.ApplyGridTheme(_gridTopItems);
            _gridTopItems.Dock = DockStyle.Fill;
            _gridTopItems.ColumnHeadersVisible = true; 
            _gridTopItems.ScrollBars = ScrollBars.Vertical; 
            rightContent.Controls.Add(_gridTopItems);
            _lblTop.BringToFront();

            Panel pnlTopItemsCard = ThemeConfig.CreateCardPanel(rightContent);
            pnlTopItemsCard.Margin = new Padding(10, 0, 0, 0);
            _middleLayout.Controls.Add(pnlTopItemsCard, 1, 0);
            
            _mainLayout.Controls.Add(_middleLayout, 0, 2);

            // 3. Bottom Section (Line Chart)
            Panel bottomContent = new Panel { Dock = DockStyle.Fill };
            _lblTrend = new Label { Name = "lblTitleTrend", Text = LocalizationManager.GetString("Dash_SalesTrends"), Font = ThemeConfig.SubHeaderFont, Dock = DockStyle.Top, Height = 30, ForeColor = ThemeConfig.TextColorDark };
            bottomContent.Controls.Add(_lblTrend);

            _chartTrends = CreateModernChart();
            _chartTrends.Series.Clear(); 
            _chartTrends.Dock = DockStyle.Fill;
            bottomContent.Controls.Add(_chartTrends);
            _lblTrend.BringToFront();

            Panel pnlTrendsCard = ThemeConfig.CreateCardPanel(bottomContent);
            _mainLayout.Controls.Add(pnlTrendsCard, 0, 3);
        }

        private StatCard CreateStatCard(string title, string iconName, Color color)
        {
            StatCard card = new StatCard
            {
                Title = title,
                IconImage = ThemeConfig.GetNuricon(iconName),
                ThemeColor = color,
                Value = "0",
                Subtitle = "Loading...",
                Dock = DockStyle.Fill,
                Margin = new Padding(5) // Spacing between cards
            };
            return card;
        }

        private Chart CreateModernChart()
        {
            Chart chart = new Chart { Dock = DockStyle.Fill, BackColor = ThemeConfig.SurfaceColor };
            ChartArea area = new ChartArea("Default");
            chart.ChartAreas.Add(area);
            
            ThemeConfig.ApplyChartTheme(chart);
            
            return chart;
        }

        public void LoadData()
        {
            if (_dashboardService == null || _chartWeeklyRevenue == null) return;
            try
            {
                // 1. Cards
                decimal totalValue = _dashboardService.GetTotalInventoryValue();
                int totalItems = _dashboardService.GetTotalItems();
                int lowStock = _dashboardService.GetLowStockCount();
                int orders = _dashboardService.GetOrdersCount(); // Default today

                Func<string, string> L = InventorySystem.Helpers.LocalizationManager.GetString;

                if (_cardInventory != null)
                {
                    _cardInventory.Value = totalItems.ToString("N0") + L("Dash_ItemsSuffix");
                    _cardInventory.Subtitle = string.Format(L("Dash_ValuedAt"), $"{totalValue:C}");
                    _cardInventory.FinalizeLayout();
                }
                
                if (_cardRevenue != null)
                {
                    _cardRevenue.Value = _dashboardService.GetSales("Today").ToString("C");
                    _cardRevenue.Subtitle = L("Dash_TodayRevenue");
                    _cardRevenue.FinalizeLayout();
                }
                
                if (_cardOrders != null)
                {
                    _cardOrders.Value = orders.ToString("N0");
                    _cardOrders.Subtitle = L("Dash_NewOrders");
                    _cardOrders.FinalizeLayout();
                }
                
                if (_cardLowStock != null)
                {
                    _cardLowStock.Value = lowStock.ToString("N0");
                    _cardLowStock.Subtitle = L("Dash_NeedsReordering");
                    _cardLowStock.FinalizeLayout();
                    if(lowStock > 0) _cardLowStock.ThemeColor = ThemeConfig.DangerColorBright; // Alert color
                }

                
                // 2. Bar Chart (Weekly Revenue)
                try
                {
                    _chartWeeklyRevenue.Series.Clear();
                    Series seriesBar = new Series(L("Rep_ChartSales"));
                    seriesBar.ChartType = SeriesChartType.Column;
                    seriesBar.Color = ThemeConfig.PrimaryColor;
                    seriesBar["PointWidth"] = "0.25";
                    seriesBar.ChartArea = "Default"; // Explicit link to the area we forced in ThemeConfig
                    
                    // Add series first, then points (safer for some Chart versions)
                    _chartWeeklyRevenue.Series.Add(seriesBar);

                    var weeklyData = _dashboardService.GetWeeklyRevenue();
                    foreach(var kvp in weeklyData)
                    {
                        seriesBar.Points.AddXY(kvp.Key, kvp.Value);
                    }

                    ThemeConfig.ApplyChartTheme(_chartWeeklyRevenue);
                }
                catch { /* Fail gracefully */ }

                // 3. Top Items Grid
                _gridTopItems.ScrollBars = ScrollBars.Both; // Ensure horizontal scroll if needed too
                _gridTopItems.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None; // Enforce fixed height
                
                var topItems = _dashboardService.GetTopSellingItems(50); // Fetch MORE to enable scrolling
                if(topItems != null)
                {
                    _gridTopItems.DataSource = topItems;
                    if(_gridTopItems.Columns.Contains("part_name")) _gridTopItems.Columns["part_name"].HeaderText = L("Parts_GridProduct");
                    if(_gridTopItems.Columns.Contains("total_sold")) _gridTopItems.Columns["total_sold"].HeaderText = L("Dash_Sold");
                }

                // 4. Line Chart (Trends)
                try
                {
                    _chartTrends.Series.Clear();
                    Series seriesSpline = new Series(L("Dash_SalesTrends"));
                    seriesSpline.ChartArea = "Default";
                    seriesSpline.ChartType = SeriesChartType.SplineArea;
                    seriesSpline.Color = Color.FromArgb(40, ThemeConfig.PrimaryColor); 
                    seriesSpline.BorderWidth = 4;
                    seriesSpline.BorderColor = ThemeConfig.PrimaryColor;
                    
                    _chartTrends.Series.Add(seriesSpline);

                    var trendData = _dashboardService.GetMonthlySalesTrend();
                    foreach (var kvp in trendData)
                    {
                        seriesSpline.Points.AddXY(kvp.Key, kvp.Value);
                    }

                    ThemeConfig.ApplyChartTheme(_chartTrends);
                }
                catch { /* Fail gracefully */ }
            }
            catch (Exception ex)
            {
                // Surface the real error so we can debug it - previously this was silent
                System.Diagnostics.Debug.WriteLine("Dashboard LoadData error: " + ex.Message);
                // Show user-visible error only if controls are ready
                if (_cardRevenue != null)
                    _cardRevenue.Subtitle = "DB Error: " + ex.Message.Substring(0, Math.Min(ex.Message.Length, 60));
            }
        }
    }
}



