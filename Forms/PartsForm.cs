using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using InventorySystem.Data;
using InventorySystem.Helpers;
using InventorySystem.Controls;
using InventorySystem.Services;

namespace InventorySystem.Forms
{
    /// <summary>
    /// Inventory Management Screen — card-view + category sidebar layout.
    /// </summary>
    public partial class PartsForm : UserControl
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }
        // ── Toolbar buttons ──────────────────────────────────────────────
        private InventorySystem.Controls.ModernButton btnAdd;
        private Button btnAddCategory;
        // private Button btnFilter;
        private Button btnImport;
        private Button btnExport;
        private ModernTextBox txtSearch;
        private ModernTextBox txtCategorySearch;

        // ── Layout containers ────────────────────────────────────────────
        private Panel          pnlCategoryList;   // scrollable category rows
        private FlowLayoutPanel pnlCardFlow;       // card grid
        private DataGridView   dgvParts;           // list (table) view
        private Panel          pnlGridView;        // wraps dgvParts
        private Panel          pnlCardView;        // wraps pnlCardFlow + header
        private Label          lblItemCount;       // "Desserts (19)"
        private Panel          btnToggleCard;
        private Panel          pnlContentHeader;
        private FlowLayoutPanel pnlRightControls;
        private TableLayoutPanel tlpBody;

        // ── State ─────────────────────────────────────────────────────────
        private InventoryService _inventoryService;
        private bool   _isCardView   = true;
        private string _activeCategory = null;   // null = "All Items"
        private string _searchText      = "";
        private bool   _lowStockOnly    = false;
        private bool   _activeOnly      = false;
        private int    _categorySortMode = 0; // 0=Default, 1=NameAsc, 2=NameDesc, 3=CountDesc, 4=CountAsc

        // ── Card layout constants ─────────────────────────────────────────
        private const int CardW = 160;
        private const int CardH = 200;
        private const int CardGap = 14;

        public PartsForm()
        {
            InitializeComponent();
            _inventoryService = new InventoryService();

            EventHandler langHandler    = (s, e) => ApplyLocalization();
            EventHandler currHandler    = (s, e) => { if (_isCardView) LoadCards(); else dgvParts?.Invalidate(); };
            EventHandler invHandler     = (s, e) => { if (this.Visible) RefreshAll(); };

            InventorySystem.Helpers.LocalizationManager.LanguageChanged += langHandler;
            InventorySystem.Services.CurrencyService.CurrencyChanged    += currHandler;

            ApplyLocalization();
            ApplyPermissions();
            RefreshAll();

            this.Disposed += (s, e) =>
            {
                InventorySystem.Helpers.LocalizationManager.LanguageChanged -= langHandler;
                InventorySystem.Services.CurrencyService.CurrencyChanged    -= currHandler;
            };
        }

        // ─────────────────────────────────────────────────────────────────
        // INITIALIZATION
        // ─────────────────────────────────────────────────────────────────
        private void InitializeComponent()
        {
            this.btnAdd         = new InventorySystem.Controls.ModernButton();
            this.btnImport      = new Button();
            this.btnExport      = new Button();
            this.txtSearch      = new ModernTextBox();
            this.dgvParts       = new DataGridView();

            ((System.ComponentModel.ISupportInitialize)(this.dgvParts)).BeginInit();
            this.SuspendLayout();

            // ── Root: full-width column for header + body ─────────────────
            TableLayoutPanel tlpRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
                Padding = new Padding(20, 16, 20, 16)
            };
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // ── Header ────────────────────────────────────────────────────
            Label lblInventoryTitle = ThemeConfig.CreateStandardHeader(
                LocalizationManager.GetString("Parts_Title"));
            lblInventoryTitle.Name = "lblInventoryTitle";

            txtSearch = new ModernTextBox
            {
                IsSearch = true, ShowLabel = false,
                PlaceholderText = LocalizationManager.GetString("Parts_Search"),
                Size = new Size(280, 35)
            };
            txtSearch.TextChanged += (s, e) =>
            {
                _searchText = (txtSearch.Text == LocalizationManager.GetString("Parts_Search") ||
                               txtSearch.Text == "Search...") ? "" : txtSearch.Text;
                RefreshAll();
            };

            btnAdd.Size = new Size(120, 35);
            btnAdd.Click += BtnAdd_Click;
            ThemeConfig.ApplyStandardAddButton(btnAdd, "Parts_New");

            btnAddCategory = new Button { Size = new Size(140, 35) };
            btnAddCategory.Click += BtnAddCategory_Click;
            ThemeConfig.ApplyStandardAddButton(btnAddCategory, "Parts_AddCategory");

            Button btnDeleteSelected = new Button { Size = new Size(130, 35), Name = "btnDeleteSelected" };
            btnDeleteSelected.Click += (s, e) =>
            {
                var checkedIds = new List<int>();
                if (_isCardView)
                {
                    foreach (Control card in pnlCardFlow.Controls)
                    {
                        if (card is Panel)
                        {
                            foreach (Control c in card.Controls)
                            {
                                if (c is CheckBox chk && chk.Checked && chk.Tag is int pId)
                                {
                                    checkedIds.Add(pId);
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (DataGridViewRow row in dgvParts.Rows)
                    {
                        var chkCell = row.Cells["colCheck"] as DataGridViewCheckBoxCell;
                        if (chkCell != null && Convert.ToBoolean(chkCell.Value ?? false))
                            if (int.TryParse(row.Cells["part_id"].Value?.ToString(), out int pId))
                                checkedIds.Add(pId);
                    }
                }

                if (checkedIds.Count == 0) 
                { 
                    string msg = LocalizationManager.GetString("Msg_SelectOne");
                    MessageHelper.ShowWarning(string.IsNullOrEmpty(msg) ? "Please select at least one item." : msg); 
                    return; 
                }
                
                string confirmMsg = LocalizationManager.GetString("Msg_ConfirmDelete");
                if (string.IsNullOrEmpty(confirmMsg)) confirmMsg = "Are you sure you want to delete {0} items?";
                
                if (MessageHelper.ConfirmAction(string.Format(confirmMsg, checkedIds.Count)))
                {
                    foreach (int i in checkedIds) _inventoryService.DeletePart(i);
                    RefreshAll();
                }
            };
            ThemeConfig.ApplyStandardDeleteButton(btnDeleteSelected, "Parts_Delete");

            btnImport.Size = new Size(100, 35);
            btnImport.Click += BtnImport_Click;
            ThemeConfig.ApplyStandardImportButton(btnImport, "Parts_Import");

            btnExport.Size = new Size(100, 35);
            btnExport.Click += BtnExport_Click;
            ThemeConfig.ApplyStandardExportButton(btnExport, "Parts_Export");

            var actionButtons = new Control[] { btnDeleteSelected, btnImport, btnExport, btnAdd };
            TableLayoutPanel tlpHeader = ThemeConfig.CreateGlobalFormHeader(lblInventoryTitle, txtSearch, actionButtons);
            tlpRoot.Controls.Add(tlpHeader, 0, 0);

            // ── Body: sidebar + content ───────────────────────────────────
            tlpBody = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
                Margin = new Padding(0, 8, 0, 0)
            };
            tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
            tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRoot.Controls.Add(tlpBody, 0, 1);

            // ── Left: Category Sidebar ────────────────────────────────────
            Panel pnlSidebarOuter = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 12, 0),
                Tag = "surface"
            };
            pnlSidebarOuter.Paint += (s, pe) =>
            {
                ThemeConfig.FillRoundedBackground(pe.Graphics, pnlSidebarOuter.ClientRectangle, 14f, ThemeConfig.SurfaceColor);
                ThemeConfig.DrawRoundedBorder(pe.Graphics, pnlSidebarOuter.ClientRectangle, 14f, ThemeConfig.BorderColor, 1f);
            };
            tlpBody.Controls.Add(pnlSidebarOuter, 0, 0);

            // Sidebar inner layout
            TableLayoutPanel tlpSidebar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4,
                BackColor = Color.Transparent, Padding = new Padding(0)
            };
            tlpSidebar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));  // Title
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));  // Search
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Category rows
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));  // Add Category btn
            pnlSidebarOuter.Controls.Add(tlpSidebar);

            // Sidebar title Header Panel
            Panel pnlSidebarHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            tlpSidebar.Controls.Add(pnlSidebarHeader, 0, 0);

            // Sidebar title
            Label lblCatTitle = new Label
            {
                Name = "lblCatTitle",
                Text = LocalizationManager.GetString("Parts_Categories") == "Parts_Categories" ? "Categories" : LocalizationManager.GetString("Parts_Categories"),
                Font = ThemeConfig.CardTitleFont,
                ForeColor = ThemeConfig.TextColorDark,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Padding = new Padding(16, 2, 8, 0),
                AutoSize = true
            };
            pnlSidebarHeader.Controls.Add(lblCatTitle);

            Panel pbSortCat = new Panel
            {
                Name = "pbSortCat",
                Size = new Size(26, 26),
                Location = new Point(Math.Max(8, pnlSidebarHeader.Width - 26 - 12), 9),
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
            pbSortCat.Paint += (s, e) =>
            {
                ThemeConfig.DrawRoundedBorder(e.Graphics, pbSortCat.ClientRectangle, 6f, ThemeConfig.BorderColor, 1f);
                
                Image img = ThemeConfig.GetNuricon("filter") ?? ThemeConfig.GetNuricon("sort");
                if (img != null)
                {
                    using (var tinted = ThemeConfig.TintImage(img, ThemeConfig.SecondaryColor))
                    {
                        int iconSize = 16;
                        e.Graphics.DrawImage(tinted, new Rectangle((pbSortCat.Width - iconSize)/2, (pbSortCat.Height - iconSize)/2, iconSize, iconSize));
                    }
                }
            };
            pbSortCat.Click += (s, e) =>
            {
                var menu = new ContextMenuStrip();
                ThemeConfig.ApplyModernMenuTheme(menu);
                var m1 = menu.Items.Add("Default Order", null, (ms, me) => { _categorySortMode = 0; RefreshCategorySidebar(); });
                var m2 = menu.Items.Add("Name (A-Z)", null, (ms, me) => { _categorySortMode = 1; RefreshCategorySidebar(); });
                var m3 = menu.Items.Add("Name (Z-A)", null, (ms, me) => { _categorySortMode = 2; RefreshCategorySidebar(); });
                var m4 = menu.Items.Add("Highest Item Count", null, (ms, me) => { _categorySortMode = 3; RefreshCategorySidebar(); });
                var m5 = menu.Items.Add("Lowest Item Count", null, (ms, me) => { _categorySortMode = 4; RefreshCategorySidebar(); });
                
                if (_categorySortMode == 0) ((ToolStripMenuItem)m1).Checked = true;
                if (_categorySortMode == 1) ((ToolStripMenuItem)m2).Checked = true;
                if (_categorySortMode == 2) ((ToolStripMenuItem)m3).Checked = true;
                if (_categorySortMode == 3) ((ToolStripMenuItem)m4).Checked = true;
                if (_categorySortMode == 4) ((ToolStripMenuItem)m5).Checked = true;
                
                menu.Show(pbSortCat, new Point(0, pbSortCat.Height));
            };
            pnlSidebarHeader.Controls.Add(pbSortCat);
            pnlSidebarHeader.Resize += (s, e) => LayoutCategorySidebarHeader();
            LayoutCategorySidebarHeader();

            // Category Search Box
            txtCategorySearch = new ModernTextBox
            {
                IsSearch = true, ShowLabel = false,
                PlaceholderText = LocalizationManager.GetString("Parts_SearchCategories"),
                Dock = DockStyle.Fill,
                Margin = new Padding(12, 0, 12, 8)
            };
            txtCategorySearch.TextChanged += (s, e) =>
            {
                string text = txtCategorySearch.Text;
                if (text != LocalizationManager.GetString("Parts_SearchCategories"))
                    RefreshCategorySidebar();
            };
            tlpSidebar.Controls.Add(txtCategorySearch, 0, 1);

            // Scrollable category list — soft background so white rows read as cards
            pnlCategoryList = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeConfig.BackgroundColor,
                Padding = new Padding(10, 6, 10, 6)
            };
            tlpSidebar.Controls.Add(pnlCategoryList, 0, 2);

            // Add Category button at bottom of sidebar
            Panel pnlAddCatWrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = Color.Transparent };
            var btnSidebarAddCat = new InventorySystem.Controls.ModernButton
            {
                Name = "btnSidebarAddCat",
                Dock = DockStyle.Fill,
                Height = 38
            };
            ThemeConfig.ApplyStandardAddButton(btnSidebarAddCat, "Parts_AddCategory");
            btnSidebarAddCat.Click += BtnAddCategory_Click;
            pnlAddCatWrapper.Controls.Add(btnSidebarAddCat);
            tlpSidebar.Controls.Add(pnlAddCatWrapper, 0, 3);

            // ── Right: Content area ────────────────────────────────────────
            Panel pnlContent = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Color.Transparent,
                Margin = new Padding(0),
                Tag = "surface"
            };
            pnlContent.Paint += (s, pe) =>
            {
                ThemeConfig.FillRoundedBackground(pe.Graphics, pnlContent.ClientRectangle, 14f, ThemeConfig.SurfaceColor);
                ThemeConfig.DrawRoundedBorder(pe.Graphics, pnlContent.ClientRectangle, 14f, ThemeConfig.BorderColor, 1f);
            };
            tlpBody.Controls.Add(pnlContent, 1, 0);

            // Content layout
            TableLayoutPanel tlpContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent,
                Padding = new Padding(16)
            };
            tlpContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));  // sub-header
            tlpContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // cards / grid

            pnlContent.Controls.Add(tlpContent);

            // Content sub-header (title + count + grid/filter toolbar)
            pnlContentHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            tlpContent.Controls.Add(pnlContentHeader, 0, 0);

            lblItemCount = new Label
            {
                AutoSize = true, Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark, BackColor = Color.Transparent,
                Location = new Point(4, 8)
            };
            pnlContentHeader.Controls.Add(lblItemCount);

            // Right toolbar: grid indicator + filter. Width fits two 26px buttons
            // plus a 10px mid-gap from their symmetric 5px horizontal margins.
            const int toolbarW = 72;
            pnlRightControls = new FlowLayoutPanel
            {
                Name = "pnlPartsRightControls",
                Size = new Size(toolbarW, 30),
                Location = new Point(Math.Max(0, pnlContentHeader.Width - toolbarW - 8), 4),
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0)
            };

            btnToggleCard = CreateToggleBtn("grid", true);
            // List-view toggle intentionally omitted — inventory stays in card view.

            Panel btnContentFilter = new Panel
            {
                Name = "btnPartsContentFilter",
                Size = new Size(26, 26),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                // Equal L/R margins keep a 10px gap in both LTR and RTL flow.
                Margin = new Padding(5, 1, 5, 0),
                Tag = ThemeConfig.SecondaryColor
            };
            btnContentFilter.Paint += (s, e) =>
            {
                ThemeConfig.DrawRoundedBorder(e.Graphics, btnContentFilter.ClientRectangle, 6f, ThemeConfig.BorderColor, 1f);
                Image img = ThemeConfig.GetNuricon("filter");
                if (img != null)
                {
                    using (var tinted = ThemeConfig.TintImage(img, ThemeConfig.SecondaryColor))
                    {
                        int iconSize = 16;
                        e.Graphics.DrawImage(tinted, new Rectangle((btnContentFilter.Width - iconSize)/2, (btnContentFilter.Height - iconSize)/2, iconSize, iconSize));
                    }
                }
            };
            btnContentFilter.Click += BtnFilter_Click;

            pnlRightControls.Controls.Add(btnToggleCard);
            pnlRightControls.Controls.Add(btnContentFilter);

            pnlContentHeader.Controls.Add(pnlRightControls);
            pnlContentHeader.Resize += (s, e) => LayoutContentToolbar();
            LayoutContentToolbar();

            // ── Card view ──────────────────────────────────────────────────
            pnlCardView = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            pnlCardFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                Padding = new Padding(4, 4, 4, 4),
                BackColor = Color.Transparent,
                WrapContents = true, FlowDirection = FlowDirection.LeftToRight
            };
            pnlCardView.Controls.Add(pnlCardFlow);
            tlpContent.Controls.Add(pnlCardView, 0, 1);

            // ── List (DataGridView) view ──────────────────────────────────
            pnlGridView = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };

            dgvParts.AllowUserToAddRows = false;
            dgvParts.ReadOnly = false;
            dgvParts.AutoGenerateColumns = false;
            dgvParts.BorderStyle = BorderStyle.None;
            dgvParts.BackgroundColor = ThemeConfig.SurfaceColor;
            dgvParts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvParts.CellPainting   += DgvParts_CellPainting;
            dgvParts.CellFormatting += DgvParts_CellFormatting;
            dgvParts.CellMouseClick += DgvParts_CellMouseClick;
            dgvParts.CellMouseMove  += DgvParts_CellMouseMove;
            dgvParts.CellMouseLeave += DgvParts_CellMouseLeave;
            dgvParts.DataError      += (s, e) => e.ThrowException = false;

            Panel pnlGridCard = ThemeConfig.CreateCardPanel(dgvParts);
            pnlGridCard.Dock = DockStyle.Fill;
            pnlGridView.Controls.Add(pnlGridCard);
            tlpContent.Controls.Add(pnlGridView, 0, 1);

            // Define Columns
            Func<string, string> L = LocalizationManager.GetString;
            dgvParts.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colCheck", HeaderText = "", Width = 30, FillWeight = 1, ReadOnly = false });
            var colImage = new DataGridViewImageColumn { Name = "colImage", HeaderText = L("Parts_GridImage"), Width = 60, ImageLayout = DataGridViewImageCellLayout.Zoom, FillWeight = 6, ReadOnly = true };
            colImage.DefaultCellStyle.Padding = new Padding(12);
            dgvParts.Columns.Add(colImage);
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSKU",      HeaderText = L("Parts_GridSKU"),      DataPropertyName = "part_number",          FillWeight = 10, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBarcode",  HeaderText = L("Parts_GridBarcode"),  DataPropertyName = "barcode",              FillWeight = 10, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName",     HeaderText = L("Parts_GridProduct"),  DataPropertyName = "part_name",            FillWeight = 18, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCategory", HeaderText = L("Parts_GridCategory"), DataPropertyName = "category_name",        FillWeight = 12, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colUom",      HeaderText = L("Parts_GridUom"),      DataPropertyName = "unit_of_measure",      FillWeight = 7,  ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLocation", HeaderText = L("Parts_GridLocation"), DataPropertyName = "location",             FillWeight = 10, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colShelf",    HeaderText = L("Parts_GridShelf"),    DataPropertyName = "shelf",                FillWeight = 8,  ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStock",    HeaderText = L("Parts_GridStock"),    DataPropertyName = "quantity_in_stock",    FillWeight = 8,  ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "minimum_stock_level", HeaderText = L("Parts_GridMinStock"), DataPropertyName = "minimum_stock_level", FillWeight = 8, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPrice",    HeaderText = L("Parts_GridPrice"),    DataPropertyName = "selling_price",        FillWeight = 10, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatus",   HeaderText = L("Parts_GridStatus"),   DataPropertyName = "status",               FillWeight = 9,  ReadOnly = true });
            var colActions = new DataGridViewButtonColumn { Name = "colActions", HeaderText = L("Parts_GridActions"), ReadOnly = true, MinimumWidth = 130, Width = 130, AutoSizeMode = DataGridViewAutoSizeColumnMode.None };
            dgvParts.Columns.Add(colActions);
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "part_id",     DataPropertyName = "part_id",    Visible = false });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "part_image",  DataPropertyName = "part_image", Visible = false });

            ThemeConfig.ApplyGridTheme(dgvParts);
            ThemeConfig.ApplyHeaderCheckBox(dgvParts, "colCheck");

            this.Controls.Add(tlpRoot);
            this.Dock = DockStyle.Fill;

            ((System.ComponentModel.ISupportInitialize)(this.dgvParts)).EndInit();
            this.ResumeLayout(false);
        }

        // ─────────────────────────────────────────────────────────────────
        // VIEW TOGGLE
        // ─────────────────────────────────────────────────────────────────
        /// <summary>
        /// Pins the grid/filter toolbar to the trailing edge so EN (right) and AR (left)
        /// keep the same 10px mid-gap after ApplyRTL flips FlowDirection.
        /// </summary>
        private void LayoutContentToolbar()
        {
            if (pnlRightControls == null || pnlContentHeader == null) return;

            const int toolbarW = 72;
            const int edge = 8;
            pnlRightControls.Size = new Size(toolbarW, 30);

            bool isAr = LocalizationManager.IsArabic;
            pnlRightControls.FlowDirection = isAr ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            pnlRightControls.RightToLeft = RightToLeft.No;
            pnlRightControls.Anchor = isAr
                ? (AnchorStyles.Top | AnchorStyles.Left)
                : (AnchorStyles.Top | AnchorStyles.Right);
            pnlRightControls.Location = isAr
                ? new Point(edge, 4)
                : new Point(Math.Max(0, pnlContentHeader.ClientSize.Width - toolbarW - edge), 4);
        }

        private Panel CreateToggleBtn(string iconName, bool startActive)
        {
            bool isActive = startActive;
            var pnl = new Panel
            {
                Size = new Size(26, 26),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Margin = new Padding(5, 1, 5, 0),
                Tag = isActive  // store active state in Tag
            };

            pnl.Paint += (s, e) =>
            {
                bool active = pnl.Tag is bool b && b;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                if (active)
                    ThemeConfig.FillRoundedBackground(e.Graphics, pnl.ClientRectangle, 6f, ThemeConfig.PrimaryColor);
                else
                    ThemeConfig.DrawRoundedBorder(e.Graphics, pnl.ClientRectangle, 6f, ThemeConfig.BorderColor, 1f);

                Image img = ThemeConfig.GetNuricon(iconName);
                if (img != null)
                {
                    Color tint = active ? Color.White : ThemeConfig.SecondaryColor;
                    using (var tinted = ThemeConfig.TintImage(img, tint))
                    {
                        int iconSize = 16;
                        e.Graphics.DrawImage(tinted, new Rectangle((pnl.Width - iconSize)/2, (pnl.Height - iconSize)/2, iconSize, iconSize));
                    }
                }
            };
            return pnl;
        }

        private void SwitchView(bool toCard)
        {
            _isCardView = toCard;
            pnlCardView.Visible  = toCard;
            pnlGridView.Visible  = !toCard;

            if (btnToggleCard != null)
            {
                btnToggleCard.Tag = toCard;
                btnToggleCard.Invalidate();
            }

            if (toCard) LoadCards();
            else        LoadData(_searchText, _lowStockOnly, _activeOnly, _activeCategory);
        }

        // ─────────────────────────────────────────────────────────────────
        // REFRESH ALL
        // ─────────────────────────────────────────────────────────────────
        private void RefreshAll()
        {
            RefreshCategorySidebar();
            if (_isCardView) LoadCards();
            else             LoadData(_searchText, _lowStockOnly, _activeOnly, _activeCategory);
        }

        // ─────────────────────────────────────────────────────────────────
        // CATEGORY SIDEBAR
        // ─────────────────────────────────────────────────────────────────
        private void RefreshCategorySidebar()
        {
            pnlCategoryList.SuspendLayout();
            foreach (Control c in pnlCategoryList.Controls) c.Dispose();
            pnlCategoryList.Controls.Clear();

            int totalItems = CategoryData.GetTotalItemCount();
            pnlCategoryList.Controls.Add(BuildCategoryRow(null, LocalizationManager.GetString("Parts_AllItems", "All Items"), totalItems));

            try
            {
                var cats = CategoryData.GetAllCategories();
                
                string search = txtCategorySearch?.Text?.Trim().ToLower() ?? "";
                if (!string.IsNullOrEmpty(search) && search != "search categories...")
                {
                    cats = cats.Where(c => c.CategoryName.ToLower().Contains(search)).ToList();
                }

                var catWithCounts = cats.Select(c => new { Cat = c, Count = CategoryData.GetItemCount(c.CategoryName) }).ToList();
                
                if (_categorySortMode == 1)
                    catWithCounts = catWithCounts.OrderBy(x => x.Cat.CategoryName).ToList();
                else if (_categorySortMode == 2)
                    catWithCounts = catWithCounts.OrderByDescending(x => x.Cat.CategoryName).ToList();
                else if (_categorySortMode == 3)
                    catWithCounts = catWithCounts.OrderByDescending(x => x.Count).ThenBy(x => x.Cat.CategoryName).ToList();
                else if (_categorySortMode == 4)
                    catWithCounts = catWithCounts.OrderBy(x => x.Count).ThenBy(x => x.Cat.CategoryName).ToList();

                foreach (var item in catWithCounts)
                {
                    pnlCategoryList.Controls.Add(BuildCategoryRow(item.Cat, item.Cat.CategoryName, item.Count));
                }

                int othersCount = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM parts p LEFT JOIN categories c ON p.category_id = c.id WHERE p.date_deleted IS NULL AND (c.category_name IS NULL OR p.category_id = 0 OR c.category_name = '')");
                if (othersCount > 0)
                {
                    var othersCat = new CategoryData { CategoryName = "Others" };
                    pnlCategoryList.Controls.Add(BuildCategoryRow(othersCat, "Others", othersCount));
                }
            }
            catch { }

            pnlCategoryList.ResumeLayout();
        }

        private void LayoutCategorySidebarHeader()
        {
            var title = this.Controls.Find("lblCatTitle", true).FirstOrDefault() as Label;
            var sort = this.Controls.Find("pbSortCat", true).FirstOrDefault() as Panel;
            if (title == null || sort == null || title.Parent == null) return;

            Panel header = title.Parent as Panel;
            if (header == null) return;

            bool isAr = LocalizationManager.IsArabic;
            title.Font = ThemeConfig.CardTitleFont;
            title.Dock = isAr ? DockStyle.Right : DockStyle.Left;
            title.TextAlign = isAr ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
            title.RightToLeft = RightToLeft.No;
            title.Padding = isAr ? new Padding(8, 2, 16, 0) : new Padding(16, 2, 8, 0);

            sort.Anchor = isAr
                ? (AnchorStyles.Top | AnchorStyles.Left)
                : (AnchorStyles.Top | AnchorStyles.Right);
            sort.Location = isAr
                ? new Point(12, 9)
                : new Point(Math.Max(8, header.ClientSize.Width - 26 - 12), 9);
        }

        private Panel BuildCategoryRow(CategoryData cat, string displayName, int count)
        {
            bool isActive = (_activeCategory == (cat?.CategoryName));
            bool isRtl = LocalizationManager.IsArabic;

            Panel wrapper = new Panel
            {
                Height = 56,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 8),
                BackColor = Color.Transparent
            };

            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Tag = cat,
                RightToLeft = RightToLeft.No
            };
            wrapper.Controls.Add(card);

            const int cardRadius = 12;
            card.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                pe.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                using (var clear = new SolidBrush(ThemeConfig.BackgroundColor))
                    pe.Graphics.FillRectangle(clear, -1, -1, card.Width + 2, card.Height + 2);

                ThemeConfig.FillRoundedBackground(pe.Graphics, card.ClientRectangle, cardRadius, ThemeConfig.SurfaceColor);
                ThemeConfig.DrawRoundedBorder(pe.Graphics, card.ClientRectangle, cardRadius,
                    isActive ? ThemeConfig.PrimaryColor : ThemeConfig.BorderColor, isActive ? 1.6f : 1f);
            };

            Image iconImage = cat == null ? ThemeConfig.GetNuricon("dashboard") : ThemeConfig.GetNuricon("category_placeholder");
            if (cat != null && !string.IsNullOrEmpty(cat.CategoryImage))
            {
                try
                {
                    string fullPath = System.IO.Path.Combine(Application.StartupPath, cat.CategoryImage);
                    if (System.IO.File.Exists(fullPath))
                    {
                        using (var ms = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(fullPath)))
                        {
                            iconImage = Image.FromStream(ms);
                        }
                    }
                }
                catch { }
            }

            PictureBox pbIcon = new PictureBox
            {
                Size = new Size(24, 24),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Image = iconImage
            };
            if (pbIcon.Image == null) pbIcon.BackColor = ThemeConfig.BorderColor;
            card.Controls.Add(pbIcon);

            Label lblName = new Label
            {
                Text = displayName,
                Font = isActive
                    ? new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Bold)
                    : new Font(ThemeConfig.AppFontFamily, 9F),
                ForeColor = ThemeConfig.TextColorDark,
                AutoSize = false,
                TextAlign = isRtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft,
                RightToLeft = RightToLeft.No,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblName);

            Label lblCount = new Label
            {
                Text = count.ToString(),
                Font = new Font(ThemeConfig.AppFontFamily, 8F, FontStyle.Bold),
                ForeColor = isActive ? Color.White : ThemeConfig.TextColorDark,
                AutoSize = false,
                Size = new Size(24, 24),
                TextAlign = ContentAlignment.MiddleCenter,
                RightToLeft = RightToLeft.No,
                BackColor = isActive ? ThemeConfig.PrimaryColor : ThemeConfig.BackgroundColor
            };
            lblCount.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                pe.Graphics.Clear(ThemeConfig.SurfaceColor);
                using (var path = ThemeConfig.GetRoundedPathPublic(
                    new Rectangle(0, 0, lblCount.Width - 1, lblCount.Height - 1), lblCount.Width / 2f))
                using (var brush = new SolidBrush(lblCount.BackColor))
                    pe.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(pe.Graphics, lblCount.Text, lblCount.Font,
                    new Rectangle(0, 0, lblCount.Width, lblCount.Height),
                    lblCount.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            card.Controls.Add(lblCount);

            void LayoutRow()
            {
                if (card.Width < 40) return;
                int iconY = Math.Max(0, (card.Height - 24) / 2);
                if (isRtl)
                {
                    pbIcon.Location = new Point(card.Width - 36, iconY);
                    lblCount.Location = new Point(12, iconY);
                    lblName.Bounds = new Rectangle(40, 0, Math.Max(40, card.Width - 40 - 44), card.Height);
                }
                else
                {
                    pbIcon.Location = new Point(12, iconY);
                    lblCount.Location = new Point(card.Width - 36, iconY);
                    lblName.Bounds = new Rectangle(44, 0, Math.Max(40, card.Width - 44 - 40), card.Height);
                }
            }
            card.Resize += (s, e) => LayoutRow();
            LayoutRow();

            EventHandler select = (s, e) =>
            {
                _activeCategory = cat?.CategoryName;
                RefreshCategorySidebar();
                if (_isCardView) LoadCards();
                else LoadData(_searchText, _lowStockOnly, _activeOnly, _activeCategory);
            };
            card.Click += select;
            pbIcon.Click += select;
            lblName.Click += select;
            lblCount.Click += select;

            if (cat != null)
            {
                Action editCategory = () =>
                {
                    using (AddCategoryForm f = new AddCategoryForm())
                    {
                        f.LoadCategoryData(cat.Id, cat.CategoryName, cat.Description, cat.CategoryImage);
                        if (f.ShowDialog() == DialogResult.OK)
                        {
                            _activeCategory = null;
                            RefreshAll();
                        }
                    }
                };

                Action deleteCategory = () =>
                {
                    int itemCount = CategoryData.GetItemCount(cat.CategoryName);
                    string warningMsg = itemCount > 0
                        ? $"Warning: There are {itemCount} items in this category.\n\nAre you sure you want to delete the category \"{cat.CategoryName}\"?"
                        : $"Delete category \"{cat.CategoryName}\"?";

                    if (MessageHelper.ConfirmAction(warningMsg))
                    {
                        try { CategoryData.DeleteCategory(cat.Id); }
                        catch (Exception ex) { MessageHelper.ShowError("Error deleting category: " + ex.Message); }
                        _activeCategory = null;
                        RefreshAll();
                    }
                };

                PictureBox pbEdit = new PictureBox { Size = new Size(18, 18), Cursor = Cursors.Hand, Visible = false, BackColor = Color.Transparent };
                PictureBox pbDelete = new PictureBox { Size = new Size(18, 18), Cursor = Cursors.Hand, Visible = false, BackColor = Color.Transparent };

                pbEdit.Paint += (s, e) => {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    var img = ThemeConfig.TintImage(ThemeConfig.GetNuricon("edit"), ThemeConfig.SecondaryColor);
                    if (img != null) e.Graphics.DrawImage(img, new Rectangle(0, 0, pbEdit.Width, pbEdit.Height));
                };
                pbDelete.Paint += (s, e) => {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    var img = ThemeConfig.TintImage(ThemeConfig.GetNuricon("delete"), ThemeConfig.DangerColor);
                    if (img != null) e.Graphics.DrawImage(img, new Rectangle(0, 0, pbDelete.Width, pbDelete.Height));
                };

                pbEdit.Click += (s, e) => editCategory();
                pbDelete.Click += (s, e) => deleteCategory();
                card.Controls.Add(pbEdit);
                card.Controls.Add(pbDelete);
                pbEdit.BringToFront();
                pbDelete.BringToFront();

                void LayoutHoverActions()
                {
                    int y = Math.Max(0, (card.Height - 18) / 2);
                    if (isRtl)
                    {
                        pbEdit.Location = new Point(40, y);
                        pbDelete.Location = new Point(62, y);
                    }
                    else
                    {
                        pbDelete.Location = new Point(Math.Max(0, card.Width - 64), y);
                        pbEdit.Location = new Point(Math.Max(0, card.Width - 88), y);
                    }
                }
                card.Resize += (s, e) => LayoutHoverActions();
                LayoutHoverActions();

                void ShowActions(object s, EventArgs e) { pbEdit.Visible = true; pbDelete.Visible = true; }
                void HideActions(object s, EventArgs e)
                {
                    if (!card.ClientRectangle.Contains(card.PointToClient(Cursor.Position)))
                    {
                        pbEdit.Visible = false; pbDelete.Visible = false;
                    }
                }

                foreach (Control c in new Control[] { card, pbIcon, lblName, lblCount, pbEdit, pbDelete })
                {
                    c.MouseEnter += ShowActions;
                    c.MouseLeave += HideActions;
                }

                void ShowCatMenu(object s, EventArgs e)
                {
                    var menu = new ContextMenuStrip();
                    ThemeConfig.ApplyModernMenuTheme(menu);
                    menu.Items.Add("Edit Category", ThemeConfig.GetNuricon("edit"), (ms, me) => editCategory());
                    menu.Items.Add("Delete Category", ThemeConfig.GetNuricon("delete"), (ms, me) => deleteCategory());
                    menu.Show(card, new Point(10, card.Height));
                }
                card.MouseClick += (s, e) => { if (((MouseEventArgs)e).Button == MouseButtons.Right) ShowCatMenu(s, e); };
                lblName.MouseClick += (s, e) => { if (((MouseEventArgs)e).Button == MouseButtons.Right) ShowCatMenu(s, e); };
                pbIcon.MouseClick += (s, e) => { if (((MouseEventArgs)e).Button == MouseButtons.Right) ShowCatMenu(s, e); };
            }

            card.BringToFront();
            return wrapper;
        }


        // CARD VIEW — LoadCards
        // ─────────────────────────────────────────────────────────────────
        private async void LoadCards()
        {
            pnlCardFlow.SuspendLayout();
            foreach (Control c in pnlCardFlow.Controls) c.Dispose();
            pnlCardFlow.Controls.Clear();

            // "Add New" placeholder card
            pnlCardFlow.Controls.Add(BuildAddNewCard());

            try
            {
                int totalCount = await System.Threading.Tasks.Task.Run(() => _inventoryService.GetPartsCount(_searchText, _lowStockOnly, _activeOnly, _activeCategory));

                // Limit 0 means no LIMIT clause: the whole filtered list loads at once.
                DataTable dt = await System.Threading.Tasks.Task.Run(() => _inventoryService.GetAllParts(_searchText, _lowStockOnly, _activeOnly, _activeCategory, 0, 0));

                // Update count label
                string catLabel = _activeCategory ?? LocalizationManager.GetString("Parts_AllItems", "All Items");
                if (lblItemCount != null)
                    lblItemCount.Text = $"{catLabel} ({totalCount})";

                var controlsList = new System.Collections.Generic.List<Control>();
                foreach (DataRow row in dt.Rows)
                {
                    controlsList.Add(BuildProductCard(row));
                }
                
                pnlCardFlow.Controls.AddRange(controlsList.ToArray());
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Error loading inventory: {ex.Message}");
            }

            pnlCardFlow.ResumeLayout();
        }

        private Panel BuildAddNewCard()
        {
            Panel card = new Panel
            {
                Size = new Size(CardW, CardH), Margin = new Padding(CardGap / 2),
                BackColor = Color.Transparent, Cursor = Cursors.Hand,
                Tag = "surface"
            };
            card.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                // Inset like every other outline so the dashes reach the corners.
                var r = new RectangleF(1.25f, 1.25f, card.Width - 2.5f, card.Height - 2.5f);
                using (var path = ThemeConfig.GetRoundedPathF(r, 12.75f))
                using (var pen = new Pen(ThemeConfig.PrimaryColor, 1.5f) { DashStyle = DashStyle.Dash })
                    pe.Graphics.DrawPath(pen, path);
            };

            var btnAdd = new InventorySystem.Controls.ModernButton
            {
                Size = new Size(CardW - 24, 40),
                Location = new Point(12, (CardH - 40) / 2)
            };
            ThemeConfig.ApplyStandardAddButton(btnAdd, "Parts_AddProduct");

            EventHandler addClick = (s, e) =>
            {
                using (AddProductServiceForm form = new AddProductServiceForm())
                {
                    if (form.ShowDialog() == DialogResult.OK) RefreshAll();
                }
            };
            card.Click  += addClick;
            btnAdd.Click += addClick;

            card.Controls.Add(btnAdd);
            return card;
        }

        private Panel BuildProductCard(DataRow row)
        {
            int     partId   = Convert.ToInt32(row["part_id"]);
            string  name     = row["part_name"]?.ToString() ?? "";
            string  sku      = row["part_number"]?.ToString() ?? "";
            string  category = row["category_name"]?.ToString() ?? "";
            decimal price    = row["selling_price"] == DBNull.Value ? 0 : Convert.ToDecimal(row["selling_price"]);
            string  status   = row["status"]?.ToString() ?? "Active";
            string  imgPath  = row["part_image"]?.ToString() ?? "";
            string  description = row["description"]?.ToString() ?? "";
            bool    isActive = status.Equals("Active", StringComparison.OrdinalIgnoreCase);

            Panel card = new Panel
            {
                Size = new Size(CardW, CardH), Margin = new Padding(CardGap / 2),
                BackColor = ThemeConfig.SurfaceColor, Cursor = Cursors.Hand, Tag = partId
            };
            card.Paint += (s, pe) =>
            {
                ThemeConfig.FillRoundedBackground(pe.Graphics, card.ClientRectangle, 14f, ThemeConfig.SurfaceColor);
                ThemeConfig.DrawRoundedBorder(pe.Graphics, card.ClientRectangle, 14f, ThemeConfig.BorderColor, 1f);
            };

            // Circular image
            PictureBox pb = new PictureBox
            {
                Size = new Size(64, 64), Location = new Point((CardW - 64) / 2, 18),
                BackColor = Color.Transparent, SizeMode = PictureBoxSizeMode.Zoom
            };
            pb.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var clipPath = new GraphicsPath())
                {
                    clipPath.AddEllipse(0, 0, pb.Width, pb.Height);
                    pe.Graphics.SetClip(clipPath);
                }
                using (var b = new SolidBrush(ThemeConfig.BackgroundColor))
                    pe.Graphics.FillEllipse(b, 0, 0, pb.Width - 1, pb.Height - 1);

                var img = pb.Tag as Image;
                if (img != null)
                    pe.Graphics.DrawImage(img, new Rectangle(4, 4, pb.Width - 8, pb.Height - 8));

                using (var pen = new Pen(ThemeConfig.BorderColor, 1f))
                    pe.Graphics.DrawEllipse(pen, 0, 0, pb.Width - 1, pb.Height - 1);
            };
            pb.Tag = CreateProductImage(imgPath, category);

            // Category label (muted)
            Label lblCat = new Label
            {
                Text = category, Font = new Font("Segoe UI", 7.5F),
                ForeColor = ThemeConfig.SecondaryColor, BackColor = Color.Transparent,
                AutoSize = false, Size = new Size(CardW - 12, 18), Location = new Point(6, 88),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Product name (bold)
            Label lblName = new Label
            {
                Text = name, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark, BackColor = Color.Transparent,
                AutoSize = false, Size = new Size(CardW - 12, 18), Location = new Point(6, 106),
                TextAlign = ContentAlignment.TopCenter
            };

            // Product description
            Label lblDesc = new Label
            {
                Text = description, Font = new Font("Segoe UI", 7.5F),
                ForeColor = Color.Gray, BackColor = Color.Transparent,
                AutoSize = false, Size = new Size(CardW - 12, 16), Location = new Point(6, 124),
                TextAlign = ContentAlignment.TopCenter
            };

            // Price
            Label lblPrice = new Label
            {
                Text = InventorySystem.Services.CurrencyService.Format(price),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = ThemeConfig.PrimaryColor, BackColor = Color.Transparent,
                AutoSize = false, Size = new Size(CardW - 12, 22), Location = new Point(6, 143),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Status badge
            Label lblStatus = new Label
            {
                Text = isActive ? (LocalizationManager.GetString("Status_Active", "Active"))
                                : (LocalizationManager.GetString("Status_Inactive", "Inactive")),
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = isActive ? ThemeConfig.SuccessBadgeText : ThemeConfig.DangerBadgeText,
                BackColor = isActive ? ThemeConfig.SuccessBadgeBg : ThemeConfig.DangerBadgeBg,
                AutoSize = false, Size = new Size(60, 20), Location = new Point((CardW - 60) / 2, 168),
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblStatus.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedPath(new Rectangle(0, 0, lblStatus.Width - 1, lblStatus.Height - 1), 8))
                using (var fill = new SolidBrush(lblStatus.BackColor))
                    pe.Graphics.FillPath(fill, path);
                TextRenderer.DrawText(pe.Graphics, lblStatus.Text, lblStatus.Font,
                    new Rectangle(0, 0, lblStatus.Width, lblStatus.Height),
                    lblStatus.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            // Edit / Delete icons
            PictureBox pbEdit = new PictureBox
            {
                Size = new Size(22, 22), Location = new Point(CardW - 52, 5),
                BackColor = Color.Transparent, Cursor = Cursors.Hand, SizeMode = PictureBoxSizeMode.Zoom,
                Image = ThemeConfig.GetNuricon("edit")
            };
            pbEdit.Click += (s, e) => OpenEditForm(row);

            PictureBox pbDelete = new PictureBox
            {
                Size = new Size(22, 22), Location = new Point(CardW - 28, 5),
                BackColor = Color.Transparent, Cursor = Cursors.Hand, SizeMode = PictureBoxSizeMode.Zoom,
                Image = ThemeConfig.GetNuricon("delete")
            };
            pbDelete.Click += (s, e) =>
            {
                if (!UserSession.IsAdmin) { MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoPermissionDelete")); return; }
                if (MessageHelper.ConfirmAction("Delete this item?"))
                {
                    _inventoryService.DeletePart(partId);
                    RefreshAll();
                }
            };

            // Click on card = edit
            EventHandler editClick = (s, e) => OpenEditForm(row);
            card.Click   += editClick;
            pb.Click     += editClick;
            lblName.Click += editClick;
            lblCat.Click  += editClick;
            lblPrice.Click += editClick;

            card.Controls.Add(pb);
            card.Controls.Add(lblCat);
            card.Controls.Add(lblName);
            card.Controls.Add(lblDesc);
            card.Controls.Add(lblPrice);
            card.Controls.Add(lblStatus);
            card.Controls.Add(pbEdit);
            card.Controls.Add(pbDelete);

            CheckBox chkSelect = new CheckBox
            {
                AutoSize = true,
                Location = new Point(8, 8),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Tag = partId
            };
            card.Controls.Add(chkSelect);

            // Hover effect
            void HoverEnter(object s, EventArgs e)
            {
                card.BackColor = Color.FromArgb(248, 248, 252);
                card.Invalidate();
            }
            void HoverLeave(object s, EventArgs e)
            {
                card.BackColor = ThemeConfig.SurfaceColor;
                card.Invalidate();
            }
            card.MouseEnter   += HoverEnter; card.MouseLeave   += HoverLeave;
            pb.MouseEnter     += HoverEnter; pb.MouseLeave     += HoverLeave;
            lblName.MouseEnter += HoverEnter; lblName.MouseLeave += HoverLeave;

            return card;
        }

        private void OpenEditForm(DataRow row)
        {
            if (!UserSession.IsAdmin) { MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoPermissionEdit")); return; }

            string id       = row["part_id"]?.ToString();
            string name     = row["part_name"]?.ToString() ?? "";
            string sku      = row["part_number"]?.ToString() ?? "";
            string category = row["category_name"]?.ToString() ?? "";
            string status   = row["status"]?.ToString() ?? "Active";
            string barcode  = row["barcode"]?.ToString() ?? "";
            string location = row["location"]?.ToString() ?? "";
            string shelf    = row["shelf"]?.ToString() ?? "";
            string image    = row["part_image"]?.ToString() ?? "";

            int qty      = row["quantity_in_stock"] == DBNull.Value ? 0 : Convert.ToInt32(row["quantity_in_stock"]);
            decimal price = row["selling_price"] == DBNull.Value ? 0 : Convert.ToDecimal(row["selling_price"]);
            int minStock  = row["minimum_stock_level"] == DBNull.Value ? 0 : Convert.ToInt32(row["minimum_stock_level"]);

            var fullPart = PartData.GetAllParts().Find(p => p.Id == int.Parse(id));
            if (fullPart != null)
            {
                using (AddProductServiceForm form = new AddProductServiceForm())
                {
                    form.LoadPartData(fullPart);
                    if (form.ShowDialog() == DialogResult.OK) { RefreshAll(); MessageHelper.ShowSuccess(LocalizationManager.GetString("Msg_UpdateSuccess")); }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // LIST VIEW — LoadData (existing DataGridView approach)
        // ─────────────────────────────────────────────────────────────────
        private async void LoadData(string search = "", bool lowStockOnly = false, bool activeOnly = false, string category = null)
        {
            try
            {
                dgvParts.DataSource = null;

                int totalCount = await System.Threading.Tasks.Task.Run(() => _inventoryService.GetPartsCount(search, lowStockOnly, activeOnly, category));

                DataTable dt = await System.Threading.Tasks.Task.Run(() => _inventoryService.GetAllParts(search, lowStockOnly, activeOnly, category, 0, 0));
                
                dgvParts.DataSource = dt;

                if (lblItemCount != null)
                    lblItemCount.Text = $"{(category ?? LocalizationManager.GetString("Parts_AllItems", "All Items"))} ({totalCount})";

                foreach (DataGridViewRow r in dgvParts.Rows)
                {
                    string imagePath   = r.Cells["part_image"].Value?.ToString();
                    string categoryName = r.Cells["colCategory"].Value?.ToString();
                    r.Cells["colImage"].Value = CreateProductImage(imagePath, categoryName);
                }
            }
            catch (Exception ex) { MessageHelper.ShowError($"Error: {ex.Message}"); }
        }

        // ─────────────────────────────────────────────────────────────────
        // LOCALIZATION
        // ─────────────────────────────────────────────────────────────────
        private void ApplyLocalization()
        {
            InventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            LayoutContentToolbar();
            LayoutCategorySidebarHeader();

            Func<string, string> L = InventorySystem.Helpers.LocalizationManager.GetString;

            var ctrlTitle = this.Controls.Find("lblInventoryTitle", true);
            if (ctrlTitle.Length > 0) ctrlTitle[0].Text = L("Parts_Title");
            if (txtSearch != null) txtSearch.PlaceholderText = L("Parts_Search");

            if (btnAdd != null) { ThemeConfig.ApplyStandardAddButton(btnAdd, "Parts_New"); btnAdd.Invalidate(); }
            if (btnImport != null)  btnImport.Invalidate();
            if (btnExport != null)  btnExport.Invalidate();

            var ctrlDel = this.Controls.Find("btnDeleteSelected", true);
            if (ctrlDel.Length > 0 && ctrlDel[0] is Button bDel) ThemeConfig.ApplyStandardDeleteButton(bDel, "Parts_Delete");

            var ctrlCatTitle = this.Controls.Find("lblCatTitle", true);
            if (ctrlCatTitle.Length > 0)
            {
                ctrlCatTitle[0].Text = L("Parts_Categories");
                ctrlCatTitle[0].Font = ThemeConfig.CardTitleFont;
            }

            var ctrlAddCat = this.Controls.Find("btnSidebarAddCat", true).FirstOrDefault() as Button;
            if (ctrlAddCat != null) { ThemeConfig.ApplyStandardAddButton(ctrlAddCat, "Parts_AddCategory"); ctrlAddCat.Invalidate(); }

            if (txtCategorySearch != null) txtCategorySearch.PlaceholderText = L("Parts_SearchCategories");

            InventorySystem.Helpers.LocalizationManager.TranslateControl(this);

            if (dgvParts != null && dgvParts.Columns.Count > 0)
            {
                if (dgvParts.Columns.Contains("colUom"))          dgvParts.Columns["colUom"].HeaderText          = L("Parts_GridUom");
                if (dgvParts.Columns.Contains("colImage"))          dgvParts.Columns["colImage"].HeaderText          = L("Parts_GridImage");
                if (dgvParts.Columns.Contains("colSKU"))            dgvParts.Columns["colSKU"].HeaderText            = L("Parts_GridSKU");
                if (dgvParts.Columns.Contains("colBarcode"))        dgvParts.Columns["colBarcode"].HeaderText        = L("Parts_GridBarcode");
                if (dgvParts.Columns.Contains("colName"))           dgvParts.Columns["colName"].HeaderText           = L("Parts_GridProduct");
                if (dgvParts.Columns.Contains("colCategory"))       dgvParts.Columns["colCategory"].HeaderText       = L("Parts_GridCategory");
                if (dgvParts.Columns.Contains("colLocation"))       dgvParts.Columns["colLocation"].HeaderText       = L("Parts_GridLocation");
                if (dgvParts.Columns.Contains("colShelf"))          dgvParts.Columns["colShelf"].HeaderText          = L("Parts_GridShelf");
                if (dgvParts.Columns.Contains("colStock"))          dgvParts.Columns["colStock"].HeaderText          = L("Parts_GridStock");
                if (dgvParts.Columns.Contains("minimum_stock_level")) dgvParts.Columns["minimum_stock_level"].HeaderText = L("Parts_GridMinStock");
                if (dgvParts.Columns.Contains("colPrice"))          dgvParts.Columns["colPrice"].HeaderText          = L("Parts_GridPrice");
                if (dgvParts.Columns.Contains("colStatus"))         dgvParts.Columns["colStatus"].HeaderText         = L("Parts_GridStatus");
                if (dgvParts.Columns.Contains("colActions"))        dgvParts.Columns["colActions"].HeaderText        = L("Parts_GridActions");
            }

            if (this.IsHandleCreated)
            {
                RefreshAll();
            }

        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible && !this.DesignMode)
            {
                RefreshAll();
                this.ActiveControl = null;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // IMAGE HELPER
        // ─────────────────────────────────────────────────────────────────
        private Bitmap CreateProductImage(string imagePath = null, string category = null)
        {
            return InventorySystem.Helpers.CacheManager.GetProductImage(imagePath, category, 56);
        }


        // ─────────────────────────────────────────────────────────────────
        // DATAGRIDVIEW PAINTING (for list view)
        // ─────────────────────────────────────────────────────────────────
        private void DgvParts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvParts.Rows[e.RowIndex];
            string category = row.Cells["colCategory"].Value?.ToString() ?? "";
            bool isService = category.Equals("Services", StringComparison.OrdinalIgnoreCase) ||
                             category.Equals("Service",  StringComparison.OrdinalIgnoreCase);

            if (isService)
            {
                if (dgvParts.Columns[e.ColumnIndex].Name == "colStock")            { e.Value = "-"; e.FormattingApplied = true; }
                if (dgvParts.Columns[e.ColumnIndex].Name == "minimum_stock_level") { e.Value = "-"; e.FormattingApplied = true; }
            }

            if (dgvParts.Columns[e.ColumnIndex].Name == "colPrice" && e.Value != null)
                if (decimal.TryParse(e.Value.ToString(), out decimal p)) { e.Value = InventorySystem.Services.CurrencyService.Format(p); e.FormattingApplied = true; }

            var stockCell = row.Cells["colStock"]; var minCell = row.Cells["minimum_stock_level"];
            if (!isService && stockCell.Value != null && minCell.Value != null)
            {
                if (int.TryParse(stockCell.Value.ToString(), out int stock) && int.TryParse(minCell.Value.ToString(), out int minS))
                {
                    if (stock <= minS) { row.DefaultCellStyle.BackColor = ThemeConfig.DangerBadgeBg; row.DefaultCellStyle.SelectionBackColor = ThemeConfig.DangerLight; row.DefaultCellStyle.SelectionForeColor = ThemeConfig.TextColorDark; }
                    else { row.DefaultCellStyle.BackColor = ThemeConfig.SurfaceColor; row.DefaultCellStyle.SelectionBackColor = ThemeConfig.SelectionBackColor; row.DefaultCellStyle.SelectionForeColor = ThemeConfig.TextColorDark; }
                }
            }
        }

        private void DgvParts_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (dgvParts.Columns[e.ColumnIndex].Name == "colStock")
            {
                var row = dgvParts.Rows[e.RowIndex];
                string cat = row.Cells["colCategory"].Value?.ToString() ?? "";
                bool isSvc = cat.Equals("Services", StringComparison.OrdinalIgnoreCase) || cat.Equals("Service", StringComparison.OrdinalIgnoreCase);
                if (isSvc) return;
                var minCell = row.Cells["minimum_stock_level"];
                if (e.Value != null && minCell.Value != null)
                {
                    int.TryParse(e.Value.ToString(), out int stock);
                    int.TryParse(minCell.Value.ToString(), out int minS);
                    if (stock <= minS)
                    {
                        e.Handled = true; e.PaintBackground(e.CellBounds, true);
                        SizeF ts = e.Graphics.MeasureString(e.Value.ToString(), e.CellStyle.Font);
                        float pillW = Math.Max(ts.Width + 16, 40);
                        float pillX = e.CellBounds.X + (e.CellBounds.Width - pillW) / 2;
                        RectangleF pillRect = new RectangleF(pillX, e.CellBounds.Y + (e.CellBounds.Height - 24) / 2, pillW, 24);
                        using (var path = GetRoundedRect(Rectangle.Round(pillRect), 8))
                        using (var brush = new SolidBrush(ThemeConfig.DangerLight))
                            e.Graphics.FillPath(brush, path);
                        TextRenderer.DrawText(e.Graphics, e.Value.ToString(), ThemeConfig.SmallBoldFont, Rectangle.Round(pillRect), ThemeConfig.DangerBadgeText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        return;
                    }
                }
            }

            if (dgvParts.Columns[e.ColumnIndex].Name == "colStatus")
            {
                e.Handled = true; e.PaintBackground(e.CellBounds, true);
                string status = e.Value?.ToString() ?? "Active";
                bool isAct = status.Equals("Active", StringComparison.OrdinalIgnoreCase);
                string dispStatus = isAct ? LocalizationManager.GetString("Status_Active") : LocalizationManager.GetString("Status_Inactive");
                Color borderC = isAct ? ThemeConfig.SuccessBorder : ThemeConfig.DangerBorder;
                Color txtC    = isAct ? ThemeConfig.SuccessBadgeText : ThemeConfig.DangerBadgeText;
                Color fillC   = isAct ? ThemeConfig.SuccessBadgeBg : ThemeConfig.DangerBadgeBg;
                Rectangle badgeRect = new Rectangle(e.CellBounds.X + 5, e.CellBounds.Y + 13, 60, 24);
                using (var path = GetRoundedRect(badgeRect, 10))
                using (var pen = new Pen(borderC, 1))
                using (var brush = new SolidBrush(fillC))
                {
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                    TextRenderer.DrawText(e.Graphics, dispStatus, ThemeConfig.MicroBoldFont, badgeRect, txtC, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }

            if (dgvParts.Columns[e.ColumnIndex].Name == "colActions")
            {
                e.Handled = true; e.PaintBackground(e.CellBounds, true);
                Image imgEdit = ThemeConfig.GetNuricon("edit"); Image imgDel = ThemeConfig.GetNuricon("delete");
                Rectangle editRect = new Rectangle(e.CellBounds.X + 8,      e.CellBounds.Y + (e.CellBounds.Height - 32) / 2, 32, 32);
                Rectangle delRect  = new Rectangle(e.CellBounds.X + 48,     e.CellBounds.Y + (e.CellBounds.Height - 32) / 2, 32, 32);
                Rectangle adjRect  = new Rectangle(e.CellBounds.X + 88 + 2, e.CellBounds.Y + (e.CellBounds.Height - 28) / 2, 28, 28);
                if (imgEdit != null)  e.Graphics.DrawImage(imgEdit, editRect);
                if (imgDel != null)   e.Graphics.DrawImage(imgDel,  delRect);
                Image imgAdj = ThemeConfig.GetNuricon("item_adjustment");
                if (imgAdj != null) e.Graphics.DrawImage(imgAdj, adjRect);
            }
        }

        private void DgvParts_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvParts.Columns[e.ColumnIndex].Name == "colActions")
            {
                if (e.X >= 10 && e.X <= 40)
                {
                    if (!UserSession.IsAdmin) { MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoPermissionEdit")); return; }
                    var row = dgvParts.Rows[e.RowIndex];
                    string id = row.Cells["part_id"].Value?.ToString();
                    if (string.IsNullOrEmpty(id)) return;
                    string name = row.Cells["colName"].Value?.ToString() ?? "";
                    string sku  = row.Cells["colSKU"].Value?.ToString() ?? "";
                    int qty     = Convert.ToInt32(row.Cells["colStock"].Value ?? 0);
                    decimal price = Convert.ToDecimal(row.Cells["colPrice"].Value ?? 0);
                    string status   = row.Cells["colStatus"].Value?.ToString() ?? "Active";
                    string barcode  = row.Cells["colBarcode"].Value?.ToString() ?? "";
                    string location = row.Cells["colLocation"].Value?.ToString() ?? "";
                    string shelf    = row.Cells["colShelf"].Value?.ToString() ?? "";
                    string image    = row.Cells["part_image"].Value?.ToString() ?? "";
                    string category = row.Cells["colCategory"].Value?.ToString() ?? "";
                    int minStock    = Convert.ToInt32(row.Cells["minimum_stock_level"].Value ?? 0);

                    var fullPart = PartData.GetAllParts().Find(p => p.Id == int.Parse(id));
                    if (fullPart != null)
                    {
                        using (AddProductServiceForm form = new AddProductServiceForm())
                        { form.LoadPartData(fullPart); if (form.ShowDialog() == DialogResult.OK) { RefreshAll(); MessageHelper.ShowSuccess(LocalizationManager.GetString("Msg_UpdateSuccess")); } }
                    }
                }
                else if (e.X >= 50 && e.X <= 80)
                {
                    string id = dgvParts.Rows[e.RowIndex].Cells["part_id"].Value?.ToString();
                    if (string.IsNullOrEmpty(id)) return;
                    if (!UserSession.IsAdmin) { MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoPermissionDelete")); return; }
                    if (MessageHelper.ConfirmAction("Delete this item?")) { _inventoryService.DeletePart(int.Parse(id)); RefreshAll(); }
                }
                else if (e.X >= 90 && e.X <= 120)
                {
                    var row = dgvParts.Rows[e.RowIndex];
                    ShowAdjustmentDialog(int.Parse(row.Cells["part_id"].Value?.ToString() ?? "0"), row.Cells["colName"].Value?.ToString() ?? "");
                }
            }
        }

        private void DgvParts_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            dgvParts.Cursor = (e.RowIndex >= 0 && dgvParts.Columns[e.ColumnIndex].Name == "colActions") ? Cursors.Hand : Cursors.Default;
        }

        private void DgvParts_CellMouseLeave(object sender, DataGridViewCellEventArgs e) => dgvParts.Cursor = Cursors.Default;

        // ─────────────────────────────────────────────────────────────────
        // ADJUSTMENT DIALOG
        // ─────────────────────────────────────────────────────────────────
        private void ShowAdjustmentDialog(int partId, string partName)
        {
            string title = LocalizationManager.GetString("Msg_AdjustStock") + partName;
            BaseModalForm f = new BaseModalForm { TitleText = title, Size = new Size(450, 280) };
            TableLayoutPanel tlp = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, RowCount = 5, AutoSize = true, Padding = new Padding(10) };
            for (int i = 0; i < 5; i++) tlp.RowStyles.Add(i < 4 ? new RowStyle(SizeType.AutoSize) : new RowStyle(SizeType.Absolute, 60F));
            ModernNumericUpDown numQty = new ModernNumericUpDown { LabelText = LocalizationManager.GetString("Msg_AdjustInstr", "Enter quantity to add (+) or subtract (-):"), Width = 380, Minimum = -99999, Maximum = 99999, Margin = new Padding(0, 0, 0, 20) };
            Label lblReason = new Label { Text = LocalizationManager.GetString("Msg_Reason"), AutoSize = true, Font = ThemeConfig.StandardFont, Margin = new Padding(0, 0, 0, 5) };
            TextBox txtReason = new TextBox { Width = 380, Font = ThemeConfig.StandardFont, Margin = new Padding(0, 0, 0, 20), Multiline = true, Height = 80 };
            Button btnSave = new ModernButton { Text = LocalizationManager.GetString("Msg_Adjust"), Size = new Size(120, 35), Anchor = AnchorStyles.Right };
            ThemeConfig.ApplyPrimaryButton(btnSave);
            btnSave.Click += (s, e) =>
            {
                if (numQty.Value == 0) { MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_AdjZero")); return; }
                if (string.IsNullOrWhiteSpace(txtReason.Text)) { MessageHelper.ShowWarning("Please provide a reason."); return; }
                try { _inventoryService.AdjustStock(partId, (int)numQty.Value, txtReason.Text); InventoryBroadcaster.BroadcastStockChange("desktop-adjustment"); MessageHelper.ShowSuccess(LocalizationManager.GetString("Msg_AdjSuccess")); f.DialogResult = DialogResult.OK; f.Close(); RefreshAll(); }
                catch (Exception ex) { MessageHelper.ShowError("Error: " + ex.Message); }
            };
            tlp.Controls.Add(numQty, 0, 1); tlp.Controls.Add(lblReason, 0, 2); tlp.Controls.Add(txtReason, 0, 3); tlp.Controls.Add(btnSave, 0, 4);
            f.ContentPanel.Controls.Add(tlp);
            LocalizationManager.ApplyRTL(f);
            f.ShowDialog();
        }

        // ─────────────────────────────────────────────────────────────────
        // BUTTON HANDLERS (unchanged)
        // ─────────────────────────────────────────────────────────────────
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (AddProductServiceForm form = new AddProductServiceForm())
            { if (form.ShowDialog() == DialogResult.OK) RefreshAll(); }
        }

        private void BtnFilter_Click(object sender, EventArgs e)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            ThemeConfig.ApplyModernMenuTheme(menu);
            menu.Items.Add("All Items",       null, (s, a) => { _lowStockOnly = false; _activeOnly = false; _activeCategory = null; RefreshAll(); });
            menu.Items.Add("Low Stock Only",  null, (s, a) => { _lowStockOnly = true;  _activeOnly = false; RefreshAll(); });
            menu.Items.Add("Active Only",     null, (s, a) => { _lowStockOnly = false; _activeOnly = true;  RefreshAll(); });
            menu.Items.Add(new ToolStripSeparator());
            try
            {
                foreach (var cat in CategoryData.GetAllCategories())
                {
                    ToolStripMenuItem catItem = new ToolStripMenuItem(cat.CategoryName);
                    catItem.Click += (s, a) => { _activeCategory = cat.CategoryName; RefreshAll(); };
                    ToolStripMenuItem editItem = new ToolStripMenuItem("Edit Category", ThemeConfig.GetNuricon("edit"));
                    editItem.Click += (s, a) =>
                    {
                        using (AddCategoryForm f = new AddCategoryForm())
                        { f.LoadCategoryData(cat.Id, cat.CategoryName, cat.Description, cat.CategoryImage); if (f.ShowDialog() == DialogResult.OK) RefreshAll(); }
                    };
                    catItem.DropDownItems.Add(editItem);
                    menu.Items.Add(catItem);
                }
            }
            catch { }
            
            if (sender is Control btn)
            {
                menu.Show(btn, new Point(0, btn.Height));
            }
        }

        private void BtnAddCategory_Click(object sender, EventArgs e)
        {
            using (AddCategoryForm form = new AddCategoryForm())
            { if (form.ShowDialog() == DialogResult.OK) { RefreshAll(); MessageHelper.ShowSuccess("Category added!"); } }
        }

        private void BtnExport_Click(object sender, EventArgs e) => ExportToCsv();
        private void BtnImport_Click(object sender, EventArgs e) => ImportFromCsv();

        // ─────────────────────────────────────────────────────────────────
        // PERMISSIONS
        // ─────────────────────────────────────────────────────────────────
        private void ApplyPermissions()
        {
            if (!UserSession.IsAdmin)
            {
                if (btnAdd != null)    btnAdd.Visible = false;
                var ctrlDel = this.Controls.Find("btnDeleteSelected", true);
                if (ctrlDel.Length > 0) ctrlDel[0].Visible = false;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // DRAWING HELPERS
        // ─────────────────────────────────────────────────────────────────
        private static GraphicsPath RoundedPath(Rectangle r, int radius)
        {
            // radius is the visual corner radius; AddArc needs diameter.
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d < 1) d = 1;
            var p = new GraphicsPath();
            p.AddArc(r.X,           r.Y,            d, d, 180, 90);
            p.AddArc(r.Right - d,   r.Y,            d, d, 270, 90);
            p.AddArc(r.Right - d,   r.Bottom - d,   d, d,   0, 90);
            p.AddArc(r.X,           r.Bottom - d,   d, d,  90, 90);
            p.CloseFigure();
            return p;
        }

        private static GraphicsPath GetRoundedRect(Rectangle rect, int radius) => RoundedPath(rect, radius);

        private static Image ResizeImage(Image img, int w, int h)
        {
            var bmp = new System.Drawing.Bitmap(w, h);
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(img, 0, 0, w, h);
            }
            return bmp;
        }

        // ─────────────────────────────────────────────────────────────────
        // EXPORT / IMPORT (preserved exactly)
        // ─────────────────────────────────────────────────────────────────
        private void ExportToCsv()
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog { Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv", FileName = $"Items_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx", Title = "Export Items" };
                if (dlg.ShowDialog() != DialogResult.OK) return;
                DataTable dt = _inventoryService.GetAllParts(_searchText, _lowStockOnly, _activeOnly, _activeCategory);
                if (dt == null || dt.Rows.Count == 0) { MessageHelper.ShowWarning("No data to export."); return; }

                DataTable exportDt = dt.Copy();
                exportDt.PrimaryKey = null;
                exportDt.Constraints.Clear();

                if (exportDt.Columns.Contains("part_id")) exportDt.Columns.Remove("part_id");

                Helpers.ImportExportHelper.MarkCurrencyColumns(exportDt,
                    "selling_price", "purchase_price", "price2", "price3", "price4");

                bool ok = dlg.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                    ? Helpers.ImportExportHelper.ExportToCsv(exportDt, dlg.FileName)
                    : Helpers.ImportExportHelper.ExportToXlsx(exportDt, dlg.FileName, "Inventory");
                if (ok)
                    MessageHelper.ShowSuccess($"Exported {exportDt.Rows.Count} items successfully!");
                else
                    MessageHelper.ShowError("Failed to export data.");
            }
            catch (Exception ex) { MessageHelper.ShowError($"Export error: {ex.Message}"); }
        }

        private void ImportFromCsv()
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog { Filter = "CSV Files (*.csv)|*.csv", Title = "Import Items from CSV" };
                if (dlg.ShowDialog() != DialogResult.OK) return;
                DataTable dt = Helpers.ImportExportHelper.ImportFromCsv(dlg.FileName);
                if (dt == null || dt.Rows.Count == 0) { MessageHelper.ShowWarning("No data found in the file."); return; }
                
                // Allow both old format and new format by checking for either
                bool isNewFormat = dt.Columns.Contains("part_name");
                bool isOldFormat = dt.Columns.Contains("PartName");
                
                if (!isNewFormat && !isOldFormat) { MessageHelper.ShowError("Invalid file format. Could not find part name column."); return; }
                
                int imported = 0, skipped = 0;
                foreach (DataRow row in dt.Rows)
                {
                    try
                    {
                        string name = isNewFormat ? row["part_name"].ToString() : row["PartName"].ToString();
                        string pn = isNewFormat && dt.Columns.Contains("part_number") ? row["part_number"].ToString() : (isOldFormat && dt.Columns.Contains("PartNumber") ? row["PartNumber"].ToString() : "");
                        
                        if (string.IsNullOrWhiteSpace(name)) { skipped++; continue; }
                        if (!string.IsNullOrWhiteSpace(pn) && _inventoryService.PartExists(pn)) { skipped++; continue; }

                        var p = new InventorySystem.Data.PartData();
                        p.PartName = name;
                        p.PartNumber = pn;
                        
                        if (isNewFormat)
                        {
                            if (dt.Columns.Contains("category_name")) p.CategoryName = row["category_name"].ToString();
                            if (dt.Columns.Contains("description")) p.Description = row["description"].ToString();
                            if (dt.Columns.Contains("quantity_in_stock")) p.QuantityInStock = int.TryParse(row["quantity_in_stock"].ToString(), out int q) ? q : 0;
                            if (dt.Columns.Contains("minimum_stock_level")) p.MinimumStockLevel = int.TryParse(row["minimum_stock_level"].ToString(), out int m) ? m : 0;
                            if (dt.Columns.Contains("reorder_quantity")) p.ReorderQuantity = int.TryParse(row["reorder_quantity"].ToString(), out int rq) ? rq : 0;
                            if (dt.Columns.Contains("purchase_price")) p.PurchasePrice = decimal.TryParse(row["purchase_price"].ToString(), out decimal pp) ? pp : 0;
                            if (dt.Columns.Contains("selling_price")) p.SellingPrice = decimal.TryParse(row["selling_price"].ToString(), out decimal sp) ? sp : 0;
                            if (dt.Columns.Contains("location")) p.Location = row["location"].ToString();
                            if (dt.Columns.Contains("shelf")) p.Shelf = row["shelf"].ToString();
                            if (dt.Columns.Contains("barcode")) p.Barcode = row["barcode"].ToString();
                            if (dt.Columns.Contains("status")) p.Status = row["status"].ToString();
                            if (dt.Columns.Contains("item_type")) p.ItemType = row["item_type"].ToString();
                            if (dt.Columns.Contains("unit_of_measure")) p.UnitOfMeasure = row["unit_of_measure"].ToString();
                            if (dt.Columns.Contains("batch_number")) p.BatchNumber = row["batch_number"].ToString();
                            if (dt.Columns.Contains("expiry_date")) p.ExpiryDate = row["expiry_date"].ToString();
                            if (dt.Columns.Contains("is_sales_item")) p.IsSalesItem = row["is_sales_item"].ToString() == "1" || row["is_sales_item"].ToString().ToLower() == "true";
                            if (dt.Columns.Contains("is_purchase_item")) p.IsPurchaseItem = row["is_purchase_item"].ToString() == "1" || row["is_purchase_item"].ToString().ToLower() == "true";
                            if (dt.Columns.Contains("is_inactive")) p.IsInactive = row["is_inactive"].ToString() == "1" || row["is_inactive"].ToString().ToLower() == "true";
                            if (dt.Columns.Contains("tax_rate")) p.TaxRate = decimal.TryParse(row["tax_rate"].ToString(), out decimal t) ? t : 0;
                            if (dt.Columns.Contains("is_stock_tracked")) p.IsStockTracked = row["is_stock_tracked"].ToString() == "1" || row["is_stock_tracked"].ToString().ToLower() == "true";
                            if (dt.Columns.Contains("price2")) p.Price2 = decimal.TryParse(row["price2"].ToString(), out decimal p2) ? p2 : 0;
                            if (dt.Columns.Contains("price3")) p.Price3 = decimal.TryParse(row["price3"].ToString(), out decimal p3) ? p3 : 0;
                            if (dt.Columns.Contains("price4")) p.Price4 = decimal.TryParse(row["price4"].ToString(), out decimal p4) ? p4 : 0;
                        }
                        else
                        {
                            if (dt.Columns.Contains("Category")) p.CategoryName = row["Category"].ToString();
                            if (dt.Columns.Contains("Quantity")) p.QuantityInStock = int.TryParse(row["Quantity"].ToString(), out int q) ? q : 0;
                            if (dt.Columns.Contains("MinimumStock")) p.MinimumStockLevel = int.TryParse(row["MinimumStock"].ToString(), out int m) ? m : 0;
                            if (dt.Columns.Contains("UnitPrice")) p.SellingPrice = decimal.TryParse(row["UnitPrice"].ToString(), out decimal sp) ? sp : 0;
                            if (dt.Columns.Contains("Location")) p.Location = row["Location"].ToString();
                            if (dt.Columns.Contains("Status")) p.Status = row["Status"].ToString();
                        }
                        
                        _inventoryService.SaveProductService(p);
                        imported++;
                    }
                    catch { skipped++; }
                }
                RefreshAll();
                MessageHelper.ShowSuccess($"Import complete!\nImported: {imported}\nSkipped: {skipped}");
            }
            catch (Exception ex) { MessageHelper.ShowError($"Import error: {ex.Message}"); }
        }
    }
}
