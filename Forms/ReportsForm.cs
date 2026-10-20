using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Controls;
using InventorySystem.Helpers;
using InventorySystem.Services;

namespace InventorySystem.Forms
{
    public partial class ReportsForm : UserControl
    {
        private readonly ReportService _reportService = new ReportService();

        private ModernComboBox cmbPeriod;
        private FlatDateTimePicker dtpFrom;
        private FlatDateTimePicker dtpTo;
        private ModernButton btnApply;
        private ModernButton btnExport;
        private Label lblDateRange;
        private Panel pnlFilterBar;
        private Label lblFrom;
        private Label lblTo;

        private StatCard cardExpenses;
        private StatCard cardCost;
        private StatCard cardSales;
        private StatCard cardProfit;
        private StatCard cardProfitAfterExpenses;

        private DataGridView dgvProducts;
        private DataGridView dgvCategories;
        private Label lblProductsTitle;
        private Label lblCategoriesTitle;

        private SalesReportSummary _currentSummary;
        private string _currentPeriodKey = "Daily";
        private bool _suppressPeriodChange;

        public ReportsForm()
        {
            InitializeComponent();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyLocalization();
            ApplyPeriodPreset("Daily");
            LoadReport();
        }

        public void RefreshData()
        {
            if (IsHandleCreated)
                LoadReport();
        }

        private void InitializeComponent()
        {
            this.Controls.Clear();
            this.Size = new Size(1100, 750);
            this.BackColor = ThemeConfig.BackgroundColor;
            this.AutoScroll = true;

            var tlpRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(20),
                BackColor = ThemeConfig.BackgroundColor
            };
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // Header + filter
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F)); // KPIs
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Tables
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 0F));
            this.Controls.Add(tlpRoot);

            // ---- Header (title only — export lives on the filter row) ----
            Label lblTitle = ThemeConfig.CreateStandardHeader(LocalizationManager.GetString("Rep_Title"));
            lblTitle.Name = "lblMainTitle";

            TableLayoutPanel tlpHeader = ThemeConfig.CreateGlobalFormHeader(lblTitle, null, null);
            tlpHeader.Margin = new Padding(0, 0, 0, 4);

            Panel filterHost = BuildFilterBar();

            var headerWrapper = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            headerWrapper.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            headerWrapper.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            headerWrapper.Controls.Add(tlpHeader, 0, 0);
            headerWrapper.Controls.Add(filterHost, 0, 1);
            tlpRoot.Controls.Add(headerWrapper, 0, 0);

            // ---- KPI Cards ----
            var tlpKpis = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1,
                Margin = new Padding(0, 8, 0, 12),
                BackColor = Color.Transparent
            };
            for (int i = 0; i < 5; i++)
                tlpKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            // Explicit row style: an AutoSize row would take each card's own height and
            // overflow the panel, clipping the bottom rounded corners.
            tlpKpis.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            cardExpenses = CreateKpiCard("Monthly Expenses", "revenue", ThemeConfig.WarningColor);
            cardCost = CreateKpiCard("Product Cost", "inventory_dashboard", ThemeConfig.SecondaryColor);
            cardSales = CreateKpiCard("Total Sales", "orders", ThemeConfig.PrimaryColor);
            cardProfit = CreateKpiCard("Total Profit", "revenue", ThemeConfig.SuccessColor);
            cardProfitAfterExpenses = CreateKpiCard("Profit After Expenses", "revenue", ThemeConfig.PrimaryColor);

            tlpKpis.Controls.Add(cardExpenses, 0, 0);
            tlpKpis.Controls.Add(cardCost, 1, 0);
            tlpKpis.Controls.Add(cardSales, 2, 0);
            tlpKpis.Controls.Add(cardProfit, 3, 0);
            tlpKpis.Controls.Add(cardProfitAfterExpenses, 4, 0);
            tlpRoot.Controls.Add(tlpKpis, 0, 1);

            // ---- Tables ----
            var tlpTables = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            tlpTables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
            tlpTables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));

            Panel productsCard = BuildTableCard(
                out lblProductsTitle, out dgvProducts,
                "Best-Selling Products", "lblProductsTitle");
            productsCard.Margin = new Padding(0, 0, 10, 0);
            tlpTables.Controls.Add(productsCard, 0, 0);

            Panel categoriesCard = BuildTableCard(
                out lblCategoriesTitle, out dgvCategories,
                "Best-Selling Categories", "lblCategoriesTitle");
            categoriesCard.Margin = new Padding(10, 0, 0, 0);
            tlpTables.Controls.Add(categoriesCard, 1, 0);

            tlpRoot.Controls.Add(tlpTables, 0, 2);
        }

        private Panel BuildFilterBar()
        {
            var panel = new Panel
            {
                Name = "pnlFilterBar",
                Dock = DockStyle.Top,
                Height = 48,
                Margin = new Padding(0, 0, 0, 0),
                BackColor = Color.Transparent,
                Padding = new Padding(0, 2, 0, 2),
                RightToLeft = RightToLeft.No
            };

            pnlFilterBar = panel;

            const int rowH = 36;
            const int topPad = 4;

            cmbPeriod = new ModernComboBox
            {
                Width = 160,
                Height = rowH,
                Margin = new Padding(0, topPad, 12, 0),
                LabelText = "",
                ShowLabel = false,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPeriod.Items.AddRange(new object[] {
                LocalizationManager.GetString("Rep_Period_Daily"),
                LocalizationManager.GetString("Rep_Period_Weekly"),
                LocalizationManager.GetString("Rep_Period_Monthly"),
                LocalizationManager.GetString("Rep_Period_Yearly"),
                LocalizationManager.GetString("Rep_Period_Custom")
            });
            cmbPeriod.SelectedIndex = 0;
            cmbPeriod.InnerComboBox.SelectedIndexChanged += (s, e) =>
            {
                if (_suppressPeriodChange) return;
                string key = PeriodKeyFromIndex(cmbPeriod.SelectedIndex);
                ApplyPeriodPreset(key);
                if (key != "Custom")
                    LoadReport();
            };

            dtpFrom = new FlatDateTimePicker
            {
                Width = 150,
                Height = rowH,
                Margin = new Padding(0, topPad, 8, 0),
                Value = DateTime.Today
            };
            dtpTo = new FlatDateTimePicker
            {
                Width = 150,
                Height = rowH,
                Margin = new Padding(0, topPad, 12, 0),
                Value = DateTime.Today
            };

            lblFrom = new Label
            {
                Name = "lblFrom",
                Text = LocalizationManager.GetString("Rep_From"),
                AutoSize = true,
                Margin = new Padding(0, 12, 6, 0),
                ForeColor = ThemeConfig.SecondaryColor,
                Font = ThemeConfig.StandardFont
            };
            lblTo = new Label
            {
                Name = "lblTo",
                Text = LocalizationManager.GetString("Rep_To"),
                AutoSize = true,
                Margin = new Padding(0, 12, 6, 0),
                ForeColor = ThemeConfig.SecondaryColor,
                Font = ThemeConfig.StandardFont
            };

            btnApply = new ModernButton
            {
                Name = "btnApply",
                Text = LocalizationManager.GetString("Rep_Apply"),
                Width = 100,
                Height = rowH,
                Margin = new Padding(0, topPad, 8, 0)
            };
            ThemeConfig.ApplySecondaryButton(btnApply);
            btnApply.Click += (s, e) =>
            {
                _currentPeriodKey = "Custom";
                _suppressPeriodChange = true;
                try
                {
                    if (cmbPeriod.SelectedIndex != 4)
                        cmbPeriod.SelectedIndex = 4;
                }
                finally { _suppressPeriodChange = false; }
                SetCustomDateEnabled(true);
                LoadReport();
            };

            btnExport = new ModernButton
            {
                Name = "btnExport",
                Text = LocalizationManager.GetString("Rep_Export", "Export to Excel"),
                Width = 150,
                Height = rowH,
                Margin = new Padding(0, topPad, 12, 0)
            };
            ThemeConfig.ApplySecondaryButton(btnExport);
            // Red text only — keep the same light chip background as Apply.
            btnExport.ForeColor = ThemeConfig.DangerColor;
            btnExport.Click += (s, e) => ExportToExcel();

            lblDateRange = new Label
            {
                Name = "lblDateRange",
                AutoSize = true,
                Margin = new Padding(4, 12, 0, 0),
                ForeColor = ThemeConfig.TextColorDark,
                Font = ThemeConfig.StandardFont,
                Text = ""
            };

            panel.Controls.Add(cmbPeriod);
            panel.Controls.Add(lblFrom);
            panel.Controls.Add(dtpFrom);
            panel.Controls.Add(lblTo);
            panel.Controls.Add(dtpTo);
            panel.Controls.Add(btnApply);
            panel.Controls.Add(btnExport);
            panel.Controls.Add(lblDateRange);

            panel.Resize += (s, e) => LayoutFilterBar();
            panel.HandleCreated += (s, e) => LayoutFilterBar();
            LayoutFilterBar();
            return panel;
        }

        /// <summary>
        /// Positions the filter bar by hand instead of flowing it: Arabic needs the
        /// date filters pinned to the right edge and the actions to the left, which a
        /// single FlowLayoutPanel cannot express in either direction.
        /// </summary>
        private void LayoutFilterBar()
        {
            if (pnlFilterBar == null || cmbPeriod == null || btnExport == null) return;

            int w = pnlFilterBar.ClientSize.Width;
            if (w <= 0) return;

            bool ar = LocalizationManager.IsArabic;
            const int rowH = 36;
            const int topPad = 4;
            const int gap = 8;
            const int groupGap = 16;

            int Top(Control c) => topPad + Math.Max(0, (rowH - c.Height) / 2);

            // Both arrays read left to right on screen.
            Control[] filters = ar
                ? new Control[] { dtpTo, lblTo, dtpFrom, lblFrom, cmbPeriod }
                : new Control[] { cmbPeriod, lblFrom, dtpFrom, lblTo, dtpTo };
            Control[] actions = ar
                ? new Control[] { btnExport, btnApply, lblDateRange }
                : new Control[] { btnApply, btnExport, lblDateRange };

            if (ar)
            {
                // Arabic: actions on the left, filters on the right (unchanged).
                int x = 0;
                foreach (Control c in actions)
                {
                    c.SetBounds(x, Top(c), c.Width, c.Height);
                    x += c.Width + gap;
                }

                int right = w;
                for (int i = filters.Length - 1; i >= 0; i--)
                {
                    Control c = filters[i];
                    right -= c.Width;
                    c.SetBounds(right, Top(c), c.Width, c.Height);
                    right -= gap;
                }
            }
            else
            {
                // English: filters on the left, actions flush to the right edge.
                int x = 0;
                foreach (Control c in filters)
                {
                    c.SetBounds(x, Top(c), c.Width, c.Height);
                    x += c.Width + gap;
                }

                int actionsW = 0;
                for (int i = 0; i < actions.Length; i++)
                    actionsW += actions[i].Width + (i > 0 ? gap : 0);
                int ax = Math.Max(x + groupGap, w - actionsW);
                foreach (Control c in actions)
                {
                    c.SetBounds(ax, Top(c), c.Width, c.Height);
                    ax += c.Width + gap;
                }
            }
        }

        private Panel BuildTableCard(out Label titleLabel, out DataGridView grid, string title, string titleName)
        {
            var content = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            titleLabel = new Label
            {
                Name = titleName,
                Text = title,
                Font = ThemeConfig.SubHeaderFont, 
                ForeColor = ThemeConfig.TextColorDark,
                Dock = DockStyle.Top,
                Height = 32,
                Padding = new Padding(2, 0, 0, 0)
            };

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AllowUserToResizeRows = false
            };
            grid.DataError += (s, e) => { e.ThrowException = false; };
            ThemeConfig.ApplyGridTheme(grid);

            content.Controls.Add(grid);
            content.Controls.Add(titleLabel);
            titleLabel.BringToFront();

            Panel card = ThemeConfig.CreateCardPanel(content);
            card.Dock = DockStyle.Fill;
            return card;
        }

        private StatCard CreateKpiCard(string title, string iconName, Color color)
        {
            return new StatCard
            {
                Title = title,
                IconImage = ThemeConfig.GetNuricon(iconName),
                ThemeColor = color,
                Value = CurrencyService.Format(0),
                Subtitle = "",
                Dock = DockStyle.Fill,
                Margin = new Padding(4)
            };
        }

        private void ApplyPeriodPreset(string preset)
        {
            _currentPeriodKey = preset ?? "Daily";
            var (from, to) = _reportService.GetPresetRange(_currentPeriodKey);

            bool isCustom = string.Equals(_currentPeriodKey, "Custom", StringComparison.OrdinalIgnoreCase);
            SetCustomDateEnabled(isCustom);

            if (!isCustom)
            {
                dtpFrom.Value = from;
                dtpTo.Value = to;
            }
            else if (!dtpFrom.Value.HasValue || !dtpTo.Value.HasValue)
            {
                dtpFrom.Value = DateTime.Today;
                dtpTo.Value = DateTime.Today;
            }
        }

        private static string PeriodKeyFromIndex(int index) => index switch
        {
            1 => "Weekly",
            2 => "Monthly",
            3 => "Yearly",
            4 => "Custom",
            _ => "Daily"
        };

        private void SetCustomDateEnabled(bool enabled)
        {
            dtpFrom.Enabled = true;
            dtpTo.Enabled = true;
            _ = enabled;
        }

        private (DateTime from, DateTime to) GetSelectedRange()
        {
            DateTime from = dtpFrom.Value?.Date ?? DateTime.Today;
            DateTime to = dtpTo.Value?.Date ?? DateTime.Today;
            if (to < from)
            {
                var tmp = from;
                from = to;
                to = tmp;
            }
            return (from, to);
        }

        private void LoadReport()
        {
            try 
            {
                var (from, to) = GetSelectedRange();
                _currentSummary = _reportService.GetSummary(from, to);

                cardExpenses.Value = CurrencyService.Format(_currentSummary.TotalExpenses);
                cardCost.Value = CurrencyService.Format(_currentSummary.TotalCost);
                cardSales.Value = CurrencyService.Format(_currentSummary.TotalSales);
                cardProfit.Value = CurrencyService.Format(_currentSummary.TotalProfit);
                cardProfitAfterExpenses.Value = CurrencyService.Format(_currentSummary.TotalProfitAfterExpenses);

                string rangeText = from == to
                    ? from.ToString("dd MMM yyyy")
                    : $"{from:dd MMM yyyy} – {to:dd MMM yyyy}";
                lblDateRange.Text = rangeText;

                BindProductsGrid(_reportService.GetTopSellingProducts(from, to, 25));
                BindCategoriesGrid(_reportService.GetTopSellingCategories(from, to, 25));
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ReportsForm.LoadReport");
                MessageHelper.ShowError(LocalizationManager.GetString("Msg_LoadError", "Failed to load report: ") + ex.Message);
            }
        }

        private void BindProductsGrid(DataTable dt)
        {
            dgvProducts.DataSource = null;
            dgvProducts.Columns.Clear();

            var display = new DataTable();
            display.Columns.Add("Product", typeof(string));
            display.Columns.Add("Qty", typeof(decimal));
            display.Columns.Add("Unit Price", typeof(string));
            display.Columns.Add("Total Sales", typeof(string));
            display.Columns.Add("Profit", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                display.Rows.Add(
                    row["product_name"]?.ToString() ?? "",
                    Convert.ToDecimal(row["quantity_sold"] == DBNull.Value ? 0 : row["quantity_sold"]),
                    CurrencyService.Format(Convert.ToDecimal(row["unit_price"] == DBNull.Value ? 0 : row["unit_price"])),
                    CurrencyService.Format(Convert.ToDecimal(row["total_sales"] == DBNull.Value ? 0 : row["total_sales"])),
                    CurrencyService.Format(Convert.ToDecimal(row["profit"] == DBNull.Value ? 0 : row["profit"]))
                );
            }

            dgvProducts.DataSource = display;
            LocalizeGridHeaders(dgvProducts, new[]
            {
                ("Product", "Rep_ColProduct"),
                ("Qty", "Rep_ColQty"),
                ("Unit Price", "Rep_ColUnitPrice"),
                ("Total Sales", "Rep_ColTotalSales"),
                ("Profit", "Rep_ColProfit")
            });
        }

        private void BindCategoriesGrid(DataTable dt)
        {
            dgvCategories.DataSource = null;
            dgvCategories.Columns.Clear();

            var display = new DataTable();
            display.Columns.Add("Category", typeof(string));
            display.Columns.Add("Qty", typeof(decimal));
            display.Columns.Add("Total Sales", typeof(string));
            display.Columns.Add("Profit", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                display.Rows.Add(
                    row["category_name"]?.ToString() ?? "",
                    Convert.ToDecimal(row["quantity_sold"] == DBNull.Value ? 0 : row["quantity_sold"]),
                    CurrencyService.Format(Convert.ToDecimal(row["total_sales"] == DBNull.Value ? 0 : row["total_sales"])),
                    CurrencyService.Format(Convert.ToDecimal(row["profit"] == DBNull.Value ? 0 : row["profit"]))
                );
            }

            dgvCategories.DataSource = display;
            LocalizeGridHeaders(dgvCategories, new[]
            {
                ("Category", "Rep_ColCategory"),
                ("Qty", "Rep_ColQty"),
                ("Total Sales", "Rep_ColTotalSales"),
                ("Profit", "Rep_ColProfit")
            });
        }

        private void LocalizeGridHeaders(DataGridView grid, (string col, string key)[] map)
        {
            foreach (var (col, key) in map)
            {
                if (grid.Columns.Contains(col))
                    grid.Columns[col].HeaderText = LocalizationManager.GetString(key, col);
            }
        }

        private void ExportToExcel()
        {
            try
            {
                var (from, to) = GetSelectedRange();
                var summary = _currentSummary ?? _reportService.GetSummary(from, to);
                DataTable detail = _reportService.GetSoldProductsDetail(from, to);

                using var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"{LocalizationManager.GetString("Rep_ExportFilePrefix", "Sales_Report")}_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx",
                    Title = LocalizationManager.GetString("Rep_ExportTitle", "Export Sales Report")
                };

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return;

                string periodLabel = LocalizationManager.GetString("Rep_Period_" + _currentPeriodKey, _currentPeriodKey);

                if (ImportExportHelper.ExportSalesReport(saveDialog.FileName, summary, detail, periodLabel))
                {
                    MessageHelper.ShowSuccess(
                        LocalizationManager.GetString("Rep_ExportSuccess", "Report exported successfully."));
                }
                else
                {
                    MessageHelper.ShowError(
                        LocalizationManager.GetString("Msg_ExportFailed", "Export failed."));
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ReportsForm.ExportToExcel");
                MessageHelper.ShowError(
                    LocalizationManager.GetString("Msg_ExportError", "Export error: ") + ex.Message);
            }
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            string L(string key, string fallback) => LocalizationManager.GetString(key, fallback);

            var title = this.Controls.Find("lblMainTitle", true);
            if (title.Length > 0) title[0].Text = L("Rep_Title", "Sales Reports");

            if (btnExport != null)
            {
                btnExport.Text = L("Rep_Export", "Export to Excel");
                btnExport.ForeColor = ThemeConfig.DangerColor;
                btnExport.Invalidate();
            }
            if (btnApply != null) btnApply.Text = L("Rep_Apply", "Apply");

            if (lblFrom != null) lblFrom.Text = L("Rep_From", "From");
            if (lblTo != null) lblTo.Text = L("Rep_To", "To");

            // Captions change width with the language, so re-place the row.
            LayoutFilterBar();
            if (IsHandleCreated) BeginInvoke((Action)LayoutFilterBar);

            if (lblProductsTitle != null)
                lblProductsTitle.Text = L("Rep_TopProductsTitle", "Best-Selling Products");
            if (lblCategoriesTitle != null)
                lblCategoriesTitle.Text = L("Rep_TopCategoriesTitle", "Best-Selling Categories");

            if (cardExpenses != null) cardExpenses.Title = L("Rep_MonthlyExpenses", "Monthly Expenses");
            if (cardCost != null) cardCost.Title = L("Rep_TotalCost", "Product Cost");
            if (cardSales != null) cardSales.Title = L("Rep_TotalSales", "Total Sales");
            if (cardProfit != null) cardProfit.Title = L("Rep_TotalProfit", "Total Profit");
            if (cardProfitAfterExpenses != null) cardProfitAfterExpenses.Title = L("Rep_ProfitAfterExpenses", "Profit After Expenses");

            // Refresh period combo labels while preserving selection
            if (cmbPeriod != null)
            {
                int idx = cmbPeriod.SelectedIndex;
                _suppressPeriodChange = true;
                try
                {
                    cmbPeriod.Items.Clear();
                    cmbPeriod.Items.Add(L("Rep_Period_Daily", "Daily"));
                    cmbPeriod.Items.Add(L("Rep_Period_Weekly", "Weekly"));
                    cmbPeriod.Items.Add(L("Rep_Period_Monthly", "Monthly"));
                    cmbPeriod.Items.Add(L("Rep_Period_Yearly", "Yearly"));
                    cmbPeriod.Items.Add(L("Rep_Period_Custom", "Custom"));
                    cmbPeriod.SelectedIndex = Math.Max(0, Math.Min(idx, cmbPeriod.Items.Count - 1));
                }
                finally { _suppressPeriodChange = false; }
            }

            if (dgvProducts?.DataSource != null)
                LocalizeGridHeaders(dgvProducts, new[]
                {
                    ("Product", "Rep_ColProduct"),
                    ("Qty", "Rep_ColQty"),
                    ("Unit Price", "Rep_ColUnitPrice"),
                    ("Total Sales", "Rep_ColTotalSales"),
                    ("Profit", "Rep_ColProfit")
                });
            if (dgvCategories?.DataSource != null)
                LocalizeGridHeaders(dgvCategories, new[]
                {
                    ("Category", "Rep_ColCategory"),
                    ("Qty", "Rep_ColQty"),
                    ("Total Sales", "Rep_ColTotalSales"),
                    ("Profit", "Rep_ColProfit")
                });
        }
    }
}
