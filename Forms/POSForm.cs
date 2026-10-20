using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using InventorySystem.Controls;
using InventorySystem.Data;
using InventorySystem.Helpers;
using InventorySystem.Services;

namespace InventorySystem.Forms
{
    public partial class POSForm : UserControl
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
        // -- LEFT PANEL CONTROLS ----------------------------------------------
        private FlowLayoutPanel pnlProducts;   // product card grid
        private FlowLayoutPanel pnlChips;      // category chip strip
        private Panel pnlCatHeader;
        private Panel pnlChipsWrapper;
        private Label lblCatTitle;
        private Label btnCatPrev;
        private Label btnCatNext;
        private Label _lblPageTitle;
        private InventorySystem.Controls.ModernTextBox txtProductSearch;

        // -- RIGHT PANEL CONTROLS ---------------------------------------------
        private PosOrderPanel _orderPanel;
        private StatCard cardTodayOrders, cardTodaySales, cardPending;

        // -- STATE -------------------------------------------------------------
        private DataTable cartTable => _orderPanel?.ActiveCart;
        private DashboardService _dashboardService;
        private string _activeCategory = null; // null = "All"
        private DateTime _lastScanTime = DateTime.Now;
        private string _scanBuffer = "";
        private ShippingDetailsForm _shippingDetails
        {
            get => _orderPanel?.ShippingDetails;
            set { if (_orderPanel != null) _orderPanel.ShippingDetails = value; }
        }
        private Button btnManageDrafts;
        private Button btnAddShipping;

        // Product grid sizing: cards are measured so a full row always fits.
        private const int ProductCardsPerRow = 4;
        private const int ProductCardGap = 12;
        private int _productCardWidth;
        private System.Windows.Forms.Timer _productRegridTimer;

        // ---------------------------------------------------------------------
        // CONSTRUCTOR
        // ---------------------------------------------------------------------
        public POSForm()
        {
            InitializeComponent();
            _dashboardService = new DashboardService();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();
            ApplyPermissions();
        }

        // ---------------------------------------------------------------------
        // LIFECYCLE
        // ---------------------------------------------------------------------
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible && !this.DesignMode)
            {
                if (cartTable == null) InitializeCart();
                RefreshStats();
                LoadCustomers();
                LoadProducts();
                this.ActiveControl = null;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Form parent = this.FindForm();
            if (parent != null)
            {
                parent.KeyPreview = true;
                parent.KeyPress -= POSForm_KeyPress;
                parent.KeyPress += POSForm_KeyPress;
            }
        }

        // ---------------------------------------------------------------------
        // INITIALIZE COMPONENT  (layout)
        // ---------------------------------------------------------------------
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Size = new Size(1200, 800);
            this.BackColor = ThemeConfig.BackgroundColor;

            // -- Root split: 70% left | 30% right -------------------------------
            TableLayoutPanel tlpRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0),
                BackColor = ThemeConfig.BackgroundColor
            };
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68F));
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.Controls.Add(tlpRoot);

            // ------------------------------------------------------------------
            // LEFT PANEL — product browser
            // ------------------------------------------------------------------
            TableLayoutPanel tlpLeft = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = ThemeConfig.BackgroundColor,
                Padding = new Padding(16, 16, 8, 16)
            };
            // Row 0  Header (Title + Search + Actions)
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            // Row 1  Stat cards (extra height + card bottom margin shows all four rounded corners)
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Absolute, 98F));
            // Row 2  Category section (title + single row of chips)
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Absolute, 104F));
            // Row 3  Product grid
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRoot.Controls.Add(tlpLeft, 0, 0);

            // -- Header -------------------------------------------------------
            string pageTitle = LocalizationManager.GetString("POS_PageTitle");
            _lblPageTitle = ThemeConfig.CreateStandardHeader(pageTitle == "POS_PageTitle" ? "Point of Sale" : pageTitle);
            _lblPageTitle.Name = "POS_PageTitle";

            txtProductSearch = new InventorySystem.Controls.ModernTextBox
            {
                IsSearch = true,
                ShowLabel = false,
                PlaceholderText = LocalizationManager.GetString("POS_SearchProducts", "Search products..."),
                Size = new Size(320, 35)
            };
            txtProductSearch.TextChanged += (s, ev) => LoadProducts(txtProductSearch.Text);

            btnManageDrafts = new InventorySystem.Controls.ModernButton { Name = "btnManageDrafts", Text = LocalizationManager.GetString("POS_ManageDrafts"), Cursor = Cursors.Hand, Height = 35, Width = 160 };
            btnAddShipping = new InventorySystem.Controls.ModernButton { Name = "btnAddShipping", Text = LocalizationManager.GetString("POS_AddShipping"), Cursor = Cursors.Hand, Height = 35, Width = 200 };

            ThemeConfig.ApplyPaletteButton(btnManageDrafts, Color.FromArgb(99, 102, 241)); // Indigo
            ThemeConfig.ApplyPaletteButton(btnAddShipping, ThemeConfig.PrimaryColor); // Gold

            btnManageDrafts.Click += (s, ev) =>
            {
                string msg = LocalizationManager.GetString("Msg_ManageDraftsInHistory");
                if (msg == "Msg_ManageDraftsInHistory") msg = "Please navigate to the History tab to manage drafts.";
                MessageHelper.ShowInfo(msg);
            };
            btnAddShipping.Click += (s, ev) =>
            {
                if (_shippingDetails == null) _shippingDetails = new ShippingDetailsForm();
                if (_shippingDetails.ShowDialog() == DialogResult.OK)
                {
                    ApplyLocalization();
                }
            };

            var actionButtons = new Control[] { btnManageDrafts, btnAddShipping };
            TableLayoutPanel tlpHeader = ThemeConfig.CreateGlobalFormHeader(_lblPageTitle, txtProductSearch, actionButtons);
            tlpLeft.Controls.Add(tlpHeader, 0, 0);

            // -- Stat Cards ----------------------------------------------------
            TableLayoutPanel tlpStats = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 0),
                Padding = new Padding(0, 2, 0, 8),
                BackColor = Color.Transparent
            };
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            // Without an explicit row style the single row is AutoSize, so it takes the
            // card's own height and overflows the panel, clipping the bottom corners.
            tlpStats.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Icons are tinted to each card's accent so the row reads as one set
            // instead of three unrelated multi-colour glyphs.
            cardTodayOrders = new StatCard
            {
                Title = "Orders", Value = "0", Compact = true,
                IconImage = ThemeConfig.TintImage(ThemeConfig.GetNuricon("pos"), ThemeConfig.PrimaryColor),
                ThemeColor = ThemeConfig.PrimaryColor,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 10, 0),
                MinimumSize = new Size(100, 72)
            };
            cardTodaySales = new StatCard
            {
                Title = "Sales", Value = "$0", Compact = true,
                IconImage = ThemeConfig.TintImage(ThemeConfig.GetNuricon("revenue"), ThemeConfig.SuccessColor),
                ThemeColor = ThemeConfig.SuccessColor,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 10, 0),
                MinimumSize = new Size(100, 72)
            };
            cardPending = new StatCard
            {
                Title = "Pending", Value = "0", Compact = true,
                IconImage = ThemeConfig.TintImage(ThemeConfig.GetNuricon("orders"), ThemeConfig.WarningColor),
                ThemeColor = ThemeConfig.WarningColor,
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                MinimumSize = new Size(100, 72)
            };

            tlpStats.Controls.Add(cardTodayOrders, 0, 0);
            tlpStats.Controls.Add(cardTodaySales, 1, 0);
            tlpStats.Controls.Add(cardPending, 2, 0);

            tlpLeft.Controls.Add(tlpStats, 0, 1);

            // -- Category section (title bar + scrollable cards) -------------
            Panel pnlCategorySection = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 6)
            };
            tlpLeft.Controls.Add(pnlCategorySection, 0, 2);

            // Title row: "Menu" + prev/next — docked so ApplyRTL can flip sides cleanly
            pnlCatHeader = new Panel
            {
                Height = 30,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent
            };
            string menuTitleTrans = LocalizationManager.GetString("POS_MenuTitle");
            lblCatTitle = new Label
            {
                Name = "POS_MenuTitle",
                Text = menuTitleTrans == "POS_MenuTitle" ? "Menu" : menuTitleTrans,
                Font = ThemeConfig.CardTitleFont,
                ForeColor = ThemeConfig.TextColorDark,
                AutoSize = true,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(2, 4, 8, 0),
                BackColor = Color.Transparent
            };

            var pnlCatNav = new Panel
            {
                Name = "pnlPosCatNav",
                Width = 68,
                Dock = DockStyle.Right,
                BackColor = Color.Transparent
            };
            btnCatNext = CreateChipNavButton("\u203A");
            btnCatPrev = CreateChipNavButton("\u2039");
            btnCatPrev.Location = new Point(4, 1);
            btnCatNext.Location = new Point(38, 1);
            pnlCatNav.Controls.Add(btnCatPrev);
            pnlCatNav.Controls.Add(btnCatNext);

            pnlCatHeader.Controls.Add(lblCatTitle);
            pnlCatHeader.Controls.Add(pnlCatNav);
            pnlCategorySection.Controls.Add(pnlCatHeader);

            // Clips the chip strip: the flow panel is deliberately taller than this
            // wrapper so its horizontal scrollbar falls outside the visible area.
            pnlChipsWrapper = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };
            pnlCategorySection.Controls.Add(pnlChipsWrapper);
            pnlChipsWrapper.BringToFront();

            pnlChips = new FlowLayoutPanel
            {
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 2, 0, 0),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                RightToLeft = RightToLeft.No
            };
            pnlChipsWrapper.Controls.Add(pnlChips);
            pnlChipsWrapper.Resize += (s, e) => LayoutChipStrip(pnlChipsWrapper);
            LayoutChipStrip(pnlChipsWrapper);

            btnCatNext.Click += (s, e) => ScrollChips(+ChipScrollStep);
            btnCatPrev.Click += (s, e) => ScrollChips(-ChipScrollStep);
            pnlChips.MouseWheel += (s, e) => ScrollChips(e.Delta > 0 ? -ChipScrollStep : ChipScrollStep);
            pnlChips.VerticalScroll.Enabled = false;
            pnlChips.VerticalScroll.Visible = false;

            // -- Product grid ------------------------------------------------
            pnlProducts = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 8, 0, 0),
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            // Hide horizontal scrollbar; keep vertical scroll for product list
            pnlProducts.HorizontalScroll.Enabled = false;
            pnlProducts.HorizontalScroll.Visible = false;
            pnlProducts.Resize += (s, e) =>
            {
                pnlProducts.HorizontalScroll.Maximum = 0;
                pnlProducts.HorizontalScroll.Visible = false;
                RequestProductRegrid();
            };
            tlpLeft.Controls.Add(pnlProducts, 0, 3);

            // ------------------------------------------------------------------
            // RIGHT PANEL — modern multi-order sidebar
            // ------------------------------------------------------------------
            _orderPanel = new PosOrderPanel { Dock = DockStyle.Fill, Margin = new Padding(4, 16, 16, 16) };
            WireOrderPanelEvents();
            tlpRoot.Controls.Add(_orderPanel, 1, 0);

            this.ResumeLayout(false);

            // Build chips after layout
            this.Load += (s, ev) => BuildCategoryChips();
        }


        private void WireOrderPanelEvents()
        {
            if (_orderPanel == null) return;
            _orderPanel.CartChanged += (s, e) => UpdateProductCardQtyAll();
            _orderPanel.ReturnClick += (s, e) =>
            {
                using var frm = new BlindReturnForm();
                frm.ShowDialog();
            };
            _orderPanel.DraftClick += (s, e) => PlaceActiveOrder("Draft", requireCustomer: false, paid: false,
                LocalizationManager.GetString("Msg_DraftSaved", "Draft saved successfully!"));
            _orderPanel.QuoteClick += (s, e) =>
            {
                int quoteId = PlaceActiveOrder("Quotation", requireCustomer: false, paid: false,
                    LocalizationManager.GetString("Msg_QuotationSaved", "Quotation saved successfully!"));
                if (quoteId <= 0) return;

                // Open the printable quotation, then switch to the Quotations page.
                using (var preview = new QuotationPreviewForm(quoteId))
                    preview.ShowDialog(FindForm());

                if (FindForm() is MainForm main)
                    main.NavigateToTab("btnQuotations");
            };
            _orderPanel.BillClick += (s, e) => PlaceActiveOrder("Completed", requireCustomer: true, paid: false,
                LocalizationManager.GetString("Msg_AddedToBill", "Successfully added to customer bill!"));
            _orderPanel.CheckoutClick += BtnCheckout_Click;
            _orderPanel.PrintClick += BtnPrintReceipt_Click;
        }

        /// <summary>Returns the new order id on success, or 0 on failure / cancelled.</summary>
        private int PlaceActiveOrder(string status, bool requireCustomer, bool paid, string successMsg)
        {
            if (cartTable == null || cartTable.Rows.Count == 0)
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("CartEmpty"));
                return 0;
            }
            int cid = _orderPanel.SelectedCustomerId;
            if (requireCustomer && cid <= 0)
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("POS_SelectCustomerForBill", "Please select a registered customer to add to their bill."));
                return 0;
            }

            decimal totalAmount = 0;
            var items = new List<InventorySystem.Services.OrderItem>();
            foreach (DataRow r in cartTable.Rows)
            {
                if (r.RowState == DataRowState.Deleted) continue;
                totalAmount += (decimal)r["Total"];
                items.Add(new InventorySystem.Services.OrderItem
                {
                    PartId = (int)r["PartID"],
                    Quantity = (int)r["Quantity"],
                    UnitPrice = (decimal)r["SellingPrice"]
                });
            }
            decimal tax = _orderPanel.ApplyVat ? totalAmount * 0.11m : 0;
            decimal ship = _orderPanel.ApplyShipping ? _orderPanel.ShippingAmount : 0;
            decimal disc = _orderPanel.GetDiscountMoney();
            totalAmount = Math.Max(0, totalAmount + tax + ship - disc);

            DateTime? dDate = _shippingDetails != null && !string.IsNullOrWhiteSpace(_shippingDetails.ShippingTo) ? _shippingDetails.DeliveryDate : (DateTime?)null;
            DateTime? pDate = _shippingDetails != null && !string.IsNullOrWhiteSpace(_shippingDetails.ShippingTo) ? _shippingDetails.PaymentDueDate : (DateTime?)null;
            string sAddr = _shippingDetails?.ShippingTo;

            int orderId = new InventorySystem.Services.OrderService().PlaceOrder(cid, items, totalAmount, paid, status, pDate, sAddr, dDate);
            if (orderId > 0)
            {
                MessageHelper.ShowInfo(successMsg);
                _orderPanel.ClearActiveCart();
                return orderId;
            }
            return 0;
        }

        // ---------------------------------------------------------------------
        private void BuildCategoryChips()
        {
            if (pnlChips == null) return;
            pnlChips.SuspendLayout();
            foreach (Control c in pnlChips.Controls)
                c.Dispose();
            pnlChips.Controls.Clear();

            bool isRTL = LocalizationManager.IsArabic;
            // Keep RightToLeft.No so ApplyRTL does not fight FlowDirection, and we
            // control strip direction explicitly here.
            pnlChips.RightToLeft = RightToLeft.No;
            pnlChips.FlowDirection = isRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            string allCatTrans = LocalizationManager.GetString("POS_AllCategories");
            AddCategoryChip(allCatTrans == "POS_AllCategories" ? "All Categories" : allCatTrans, null);

            try
            {
                var categories = CategoryData.GetAllCategories();
                foreach (var cat in categories)
                    AddCategoryChip(cat.CategoryName, cat.CategoryName);

                var allParts = PartData.GetAllParts();
                int othersCount = allParts.FindAll(p => string.IsNullOrEmpty(p.CategoryName)).Count;
                if (othersCount > 0)
                    AddCategoryChip(LocalizationManager.GetString("POS_Others"), "Others");
            }
            catch { }

            pnlChips.ResumeLayout(true);
            pnlChips.VerticalScroll.Enabled = false;
            pnlChips.VerticalScroll.Visible = false;
            pnlChips.AutoScrollPosition = new Point(0, 0);
            pnlChips.PerformLayout();
            if (pnlChipsWrapper != null) LayoutChipStrip(pnlChipsWrapper);
        }

        /// <summary>
        /// Card width that lets exactly <see cref="ProductCardsPerRow"/> cards fit in
        /// one row of the grid, including each card's right margin.
        /// </summary>
        private int ProductCardWidth()
        {
            int available = (pnlProducts?.ClientSize.Width ?? 760) - (pnlProducts?.Padding.Horizontal ?? 0);
            if (pnlProducts != null && !pnlProducts.VerticalScroll.Visible)
                available -= SystemInformation.VerticalScrollBarWidth;
            int w = (available - ProductCardGap * ProductCardsPerRow) / ProductCardsPerRow;
            return Math.Max(148, Math.Min(240, w));
        }

        /// <summary>Cards are laid out absolutely, so a width change means a rebuild.</summary>
        private void RequestProductRegrid()
        {
            if (pnlProducts == null || !this.Visible) return;
            if (ProductCardWidth() == _productCardWidth) return;

            if (_productRegridTimer == null)
            {
                _productRegridTimer = new System.Windows.Forms.Timer { Interval = 250 };
                _productRegridTimer.Tick += (s, e) =>
                {
                    _productRegridTimer.Stop();
                    if (ProductCardWidth() != _productCardWidth)
                        LoadProducts(txtProductSearch?.Text);
                };
            }
            _productRegridTimer.Stop();
            _productRegridTimer.Start();
        }

        private const int ChipScrollStep = 200;

        private static Label CreateChipNavButton(string glyph)
        {
            var btn = new Label
            {
                Text = glyph,
                AutoSize = false,
                Size = new Size(28, 28),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = Color.Transparent,
                ForeColor = ThemeConfig.SecondaryColor,
                Margin = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter
            };
            bool hover = false;
            btn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using (var br = new SolidBrush(hover ? Color.FromArgb(238, 241, 245) : ThemeConfig.SurfaceColor))
                    e.Graphics.FillEllipse(br, r);
                using (var pen = new Pen(ThemeConfig.BorderColor, 1f))
                    e.Graphics.DrawEllipse(pen, r);
                TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, r,
                    hover ? ThemeConfig.TextColorDark : ThemeConfig.SecondaryColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btn.MouseEnter += (s, e) => { hover = true; btn.Invalidate(); };
            btn.MouseLeave += (s, e) => { hover = false; btn.Invalidate(); };
            return btn;
        }

        /// <summary>
        /// Sizes the chip strip so its horizontal scrollbar sits below the clipping
        /// wrapper: scrolling still works, but the thick native bar is never drawn.
        /// </summary>
        private void LayoutChipStrip(Panel wrapper)
        {
            if (pnlChips == null || wrapper == null) return;
            if (wrapper.ClientSize.Width < 4 || wrapper.ClientSize.Height < 4) return;
            pnlChips.SetBounds(0, 0, wrapper.ClientSize.Width,
                wrapper.ClientSize.Height + SystemInformation.HorizontalScrollBarHeight + 2);
        }

        private void ScrollChips(int delta)
        {
            if (pnlChips == null) return;
            int current = -pnlChips.AutoScrollPosition.X;
            int max = Math.Max(0, pnlChips.HorizontalScroll.Maximum);
            int target = Math.Min(max, Math.Max(0, current + delta));
            pnlChips.AutoScrollPosition = new Point(target, 0);
        }

        /// <summary>Wheel messages land on the focused control, so chips forward them.</summary>
        private void HookChipWheel(Control c)
        {
            c.MouseWheel += (s, e) => ScrollChips(e.Delta > 0 ? -ChipScrollStep : ChipScrollStep);
            foreach (Control child in c.Controls)
                HookChipWheel(child);
        }

        private void AddCategoryChip(string label, string categoryKey)
        {
            bool isActive = (_activeCategory == categoryKey) ||
                            (_activeCategory == null && categoryKey == null);

            // Count items in this category
            int itemCount = 0;
            try
            {
                var allParts = PartData.GetAllParts();
                itemCount = categoryKey == null
                    ? allParts.Count
                    : categoryKey == "Others"
                        ? allParts.FindAll(p => string.IsNullOrEmpty(p.CategoryName)).Count
                        : allParts.FindAll(p => string.Equals(p.CategoryName, categoryKey, StringComparison.OrdinalIgnoreCase)).Count;
            }
            catch { }

            string itemsWord = LocalizationManager.GetString("POS_ItemsCount", "items");
            if (itemsWord == "POS_ItemsCount") itemsWord = "items";
            string countText = $"{itemCount} {itemsWord}";

            Image chipIcon = categoryKey == null ? ThemeConfig.GetNuricon("dashboard") : ThemeConfig.GetNuricon("category_placeholder");
            if (categoryKey != null)
            {
                try
                {
                    var cats = CategoryData.GetAllCategories();
                    var cat = cats.Find(c => string.Equals(c.CategoryName, categoryKey, StringComparison.OrdinalIgnoreCase));
                    if (cat != null && !string.IsNullOrEmpty(cat.CategoryImage))
                    {
                        string fullPath = System.IO.Path.Combine(Application.StartupPath, cat.CategoryImage);
                        if (System.IO.File.Exists(fullPath))
                        {
                            using (var ms = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(fullPath)))
                            {
                                chipIcon = Image.FromStream(ms);
                            }
                        }
                    }
                }
                catch { }
            }

            // Card dimensions  wider to accommodate icon + text
            var nameFont = ThemeConfig.SmallBoldFont ?? new Font("Segoe UI", 9F, FontStyle.Bold);
            var countFont = new Font("Segoe UI", 7.5F);
            int nameW = TextRenderer.MeasureText(label, nameFont).Width;
            int cntW = TextRenderer.MeasureText(countText, countFont).Width;
            countFont.Dispose();
            int cardW = Math.Max(nameW, cntW) + 76;  // icon + padding + textWidth + right-pad
            cardW = Math.Max(cardW, 148);
            const int CARD_H = 62;

            Color activeBorder = ThemeConfig.POS_ChipActiveBorder;
            Color inactiveBg = ThemeConfig.SurfaceColor;
            bool rtl = LocalizationManager.IsArabic;

            var chip = new Panel
            {
                AutoSize = false,
                Width = cardW,
                Height = CARD_H,
                Cursor = Cursors.Hand,
                // Trailing gap follows reading direction so chips don't pile into each other in RTL.
                Margin = rtl ? new Padding(10, 0, 0, 0) : new Padding(0, 0, 10, 0),
                BackColor = Color.Transparent,
                ForeColor = ThemeConfig.TextColorDark,
                RightToLeft = RightToLeft.No,
                Tag = categoryKey
            };
            bool hover = false;

            chip.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                using (var parentBrush = new SolidBrush(ThemeConfig.GetParentColor(chip)))
                    g.FillRectangle(parentBrush, -1, -1, chip.Width + 2, chip.Height + 2);

                const int cardRadius = 16;

                Color bgFill = isActive
                    ? Color.FromArgb(18, activeBorder.R, activeBorder.G, activeBorder.B)
                    : hover ? Color.FromArgb(250, 251, 253) : inactiveBg;

                ThemeConfig.FillRoundedBackground(g, chip.ClientRectangle, cardRadius, bgFill);

                Color borderColor = isActive
                    ? activeBorder
                    : hover ? ThemeConfig.SecondaryColor : ThemeConfig.BorderColor;
                ThemeConfig.DrawRoundedBorder(g, chip.ClientRectangle, cardRadius, borderColor, isActive ? 1.6f : 1f);

                int iconSize = 38;
                int cy = (CARD_H - iconSize) / 2;
                int cx = rtl ? chip.Width - iconSize - 9 : 9;
                Color tileFill = isActive
                    ? Color.FromArgb(38, activeBorder.R, activeBorder.G, activeBorder.B)
                    : Color.FromArgb(243, 245, 248);
                using (var iconBgPath = RoundedPath(new Rectangle(cx, cy, iconSize, iconSize), 12))
                using (var iconBr = new SolidBrush(tileFill))
                    g.FillPath(iconBr, iconBgPath);

                if (chipIcon != null)
                {
                    float scale = Math.Min(iconSize * 0.62f / chipIcon.Width, iconSize * 0.62f / chipIcon.Height);
                    float sw = chipIcon.Width * scale;
                    float sh = chipIcon.Height * scale;
                    float dx = cx + (iconSize - sw) / 2f;
                    float dy = cy + (iconSize - sh) / 2f;
                    g.DrawImage(chipIcon, new RectangleF(dx, dy, sw, sh));
                }

                int textX = rtl ? 10 : cx + iconSize + 10;
                int textW = rtl ? Math.Max(20, cx - 10 - 8) : chip.Width - textX - 10;
                int textBlockY = cy + 1;
                var textFlags = (rtl ? TextFormatFlags.Right : TextFormatFlags.Left)
                    | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;

                TextRenderer.DrawText(g, label, nameFont,
                    new Rectangle(textX, textBlockY, textW, 17), ThemeConfig.TextColorDark, textFlags);

                using (var cf = new Font("Segoe UI", 7.5F))
                    TextRenderer.DrawText(g, countText, cf,
                        new Rectangle(textX, textBlockY + 19, textW, 16),
                        isActive ? activeBorder : ThemeConfig.SecondaryColor, textFlags);
            };

            chip.MouseEnter += (s, e) => { hover = true; chip.Invalidate(); };
            chip.MouseLeave += (s, e) => { hover = false; chip.Invalidate(); };

            chip.Click += (s, e) =>
            {
                _activeCategory = categoryKey;
                BuildCategoryChips();
                LoadProducts(txtProductSearch?.Text);
            };

            HookChipWheel(chip);
            pnlChips.Controls.Add(chip);
        }

        // ---------------------------------------------------------------------
        // PRODUCT LOADING
        // ---------------------------------------------------------------------
        public async void LoadProducts(string search = null)
        {
            _productCardWidth = ProductCardWidth();
            pnlProducts.SuspendLayout();
            foreach (Control c in pnlProducts.Controls) c.Dispose();
            pnlProducts.Controls.Clear();

            try
            {
                List<PartData> all = string.IsNullOrWhiteSpace(search)
                    ? await System.Threading.Tasks.Task.Run(() => PartData.GetAllParts(_activeCategory))
                    : await System.Threading.Tasks.Task.Run(() => PartData.SearchParts(search.Trim(), _activeCategory));

                if (all.Count == 0)
                {
                    Label noResults = new Label
                    {
                        Text = LocalizationManager.GetString("POS_NoProductsFound"),
                        Font = ThemeConfig.StandardFont,
                        ForeColor = ThemeConfig.SecondaryColor,
                        AutoSize = true,
                        Margin = new Padding(16)
                    };
                    pnlProducts.Controls.Add(noResults);
                }
                else
                {
                    var controlsList = new System.Collections.Generic.List<Control>();
                    foreach (var part in all)
                    {
                        controlsList.Add(CreateProductCard(part));
                    }
                    pnlProducts.Controls.AddRange(controlsList.ToArray());
                }

                pnlProducts.HorizontalScroll.Maximum = 0;
                pnlProducts.HorizontalScroll.Visible = false;
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(string.Format(LocalizationManager.GetString("Msg_ErrorLoadingProducts"), ex.Message));
            }

            pnlProducts.ResumeLayout();
        }

        // ---------------------------------------------------------------------
        // PRODUCT CARD
        // ---------------------------------------------------------------------
        private Panel CreateProductCard(PartData part)
        {
            bool isService = string.Equals(part.ItemType, "Service", StringComparison.OrdinalIgnoreCase);
            bool outOfStock = !isService && part.QuantityInStock <= 0;
            // cartQty is read live in Paint/layout so the border updates instantly
            int GetLiveQty() => GetCartQty(part.Id);

            Color cardBgColor = outOfStock ? Color.FromArgb(240, 240, 242) : ThemeConfig.SurfaceColor;

            // -- Card shell --------------------------------------------------
            // Width is derived from the grid so a full row of cards always fits.
            int CARD_W = _productCardWidth > 0 ? _productCardWidth : ProductCardWidth();
            const int CARD_H = 212;
            const int IMG_SIZE = 110;  // larger image circle like reference
            const int RADIUS = 16;
            const int BTN_SIZE = 26;

            Panel card = new Panel
            {
                Size = new Size(CARD_W, CARD_H),
                BackColor = ThemeConfig.SurfaceColor,
                Margin = new Padding(0, 0, ProductCardGap, ProductCardGap),
                Cursor = outOfStock ? Cursors.No : Cursors.Hand,
                Tag = part.Id,
            };
            // Enable double-buffering via reflection (DoubleBuffered is protected on Panel)
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(card, true);


            bool hovered = false;
            card.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                // erase parent color first so rounded corners show correctly
                using (var parentBrush = new SolidBrush(ThemeConfig.GetParentColor(card)))
                    pe.Graphics.FillRectangle(parentBrush, -1, -1, card.Width + 2, card.Height + 2);

                int liveQty = GetLiveQty();
                Color borderColor;
                if (liveQty > 0) borderColor = ThemeConfig.PrimaryColor;   // in-cart: teal accent
                else if (hovered && !outOfStock) borderColor = ThemeConfig.PrimaryColor;
                else borderColor = ThemeConfig.BorderColor;

                ThemeConfig.FillRoundedBackground(pe.Graphics, card.ClientRectangle, RADIUS, cardBgColor);
                float borderW = (liveQty > 0 || (hovered && !outOfStock)) ? 1.8f : 1f;
                ThemeConfig.DrawRoundedBorder(pe.Graphics, card.ClientRectangle, RADIUS, borderColor, borderW);
            };
            // Hover wiring deferred  applied after all children are built (see PropagateHover below)

            // ------------------------------------------------------------------
            // SECTION 1  Image container  (div.card-image)
            // A transparent panel that centres the circular image
            // ------------------------------------------------------------------
            const int IMG_SECTION_H = 128; // height of image zone
            Panel pnlImageSection = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(CARD_W, IMG_SECTION_H),
                BackColor = Color.Transparent
            };
            card.Controls.Add(pnlImageSection);

            // Circular background disc  centred in the image section
            int circleDiameter = IMG_SIZE + 6;
            int circleX = (CARD_W - circleDiameter) / 2;
            int circleY = (IMG_SECTION_H - circleDiameter) / 2;

            Panel pnlImgBg = new Panel
            {
                Location = new Point(circleX, circleY),
                Size = new Size(circleDiameter, circleDiameter),
                BackColor = Color.Transparent   // parent handles clearing
            };

            // Load image once  drawn directly in Paint (no PictureBox needed)
            var bmp = LoadProductImage(part.PartImage, part.CategoryName, IMG_SIZE);

            pnlImgBg.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                int w = pnlImgBg.Width;
                int h = pnlImgBg.Height;

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                // -- Step 1: fill the background circle ----------------------
                using (var br = new SolidBrush(ThemeConfig.BackgroundColor))
                    g.FillEllipse(br, 0, 0, w - 1, h - 1);

                // -- Step 2: clip to circle, then draw the image perfectly centred -
                if (bmp != null)
                {
                    // Scale to fit inside a PAD-inset square, preserving aspect ratio
                    const int PAD = 6;
                    float avail = Math.Min(w, h) - PAD * 2f;   // usable diameter
                    float scale = Math.Min(avail / bmp.Width, avail / bmp.Height);

                    // Use float throughout  integer truncation causes systematic 1px error
                    float scaledW = bmp.Width * scale;
                    float scaledH = bmp.Height * scale;

                    // Centre image exactly at (w/2, h/2) regardless of bitmap dimensions
                    float dx = (w - scaledW) / 2f;
                    float dy = (h - scaledH) / 2f;

                    using (var clipPath = new GraphicsPath())
                    {
                        clipPath.AddEllipse(1, 1, w - 3, h - 3);
                        g.SetClip(clipPath);
                        g.DrawImage(bmp, new RectangleF(dx, dy, scaledW, scaledH));
                        g.ResetClip();
                    }
                }
            };
            pnlImageSection.Controls.Add(pnlImgBg);

            // ------------------------------------------------------------------
            // SECTION 2  Text container  (div.card-body)
            // Category italic label + bold product name, both centred
            // ------------------------------------------------------------------
            const int TEXT_SECTION_H = 56;
            int textSectionY = IMG_SECTION_H;  // sits directly under the image section
            Panel pnlTextSection = new Panel
            {
                Location = new Point(0, textSectionY),
                Size = new Size(CARD_W, TEXT_SECTION_H),
                BackColor = Color.Transparent,
                Padding = new Padding(10, 0, 10, 0)
            };
            card.Controls.Add(pnlTextSection);

            // Category  small italic grey (like reference)
            Label lblCat = new Label
            {
                Text = part.CategoryName,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Italic),
                ForeColor = ThemeConfig.SecondaryColor,
                AutoSize = false,
                Width = CARD_W - 20,
                Height = 16,
                Location = new Point(10, 2),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            pnlTextSection.Controls.Add(lblCat);

            // Product name  bold, dark, 2-line wrap
            Label lblName = new Label
            {
                Text = part.PartName,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                AutoSize = false,
                Width = CARD_W - 20,
                Height = 18,
                Location = new Point(10, 18),
                TextAlign = ContentAlignment.TopCenter,
                BackColor = Color.Transparent
            };
            pnlTextSection.Controls.Add(lblName);

            Label lblDesc = new Label
            {
                Text = part.Description,
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = Color.Gray,
                AutoSize = false,
                Width = CARD_W - 20,
                Height = 16,
                Location = new Point(10, 36),
                TextAlign = ContentAlignment.TopCenter,
                BackColor = Color.Transparent
            };
            pnlTextSection.Controls.Add(lblDesc);

            // ------------------------------------------------------------------
            // SECTION 3  Footer row  (div.card-footer)
            // Price (left) |  qty + (right), all vertically centred
            // ------------------------------------------------------------------
            int footerY = textSectionY + TEXT_SECTION_H;
            const int FOOTER_H = CARD_H - IMG_SECTION_H - TEXT_SECTION_H;
            Panel pnlFooterRow = new Panel
            {
                Location = new Point(0, footerY),
                Size = new Size(CARD_W, FOOTER_H),
                BackColor = Color.Transparent,  // transparent keeps card rounded corners intact
                Padding = new Padding(10, 0, 10, 0)
            };
            card.Controls.Add(pnlFooterRow);

            // -- Price label (left side of footer) --------------------------
            Label lblPrice = new Label
            {
                Text = CurrencyService.Format(part.SellingPrice),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = ThemeConfig.PrimaryColor,
                AutoSize = false,          // fixed size so DoFooterLayout() can centre it immediately
                Width = 80,
                Height = BTN_SIZE,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            // -- - panel button (outlined rounded rectangle) -----------------
            // Using Panel instead of Button: Panels have no ButtonBase clip insets.
            Panel btnMinus = new Panel
            {
                Name = "btnMinus_" + part.Id,
                Size = new Size(BTN_SIZE, BTN_SIZE),
                Cursor = Cursors.Hand,
                BackColor = cardBgColor,
            };
            btnMinus.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(1, 1, btnMinus.Width - 2, btnMinus.Height - 2);
                using (var path = RoundedPath(rect, 7))
                using (var pen = new Pen(btnMinus.Enabled ? ThemeConfig.BorderColor : ThemeConfig.SecondaryColor, 1.5f))
                    pe.Graphics.DrawPath(pen, path);
                Color textCol = btnMinus.Enabled ? ThemeConfig.TextColorDark : ThemeConfig.SecondaryColor;
                TextRenderer.DrawText(pe.Graphics, "-",
                    new Font("Segoe UI", 10F),
                    new Rectangle(0, 0, btnMinus.Width, btnMinus.Height),
                    textCol,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            };

            TextBox txtQty = new TextBox
            {
                Text = "1",
                Size = new Size(30, BTN_SIZE),
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                BackColor = cardBgColor,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0),
                Tag = "qtyLabel_" + part.Id
            };
            txtQty.KeyPress += (sender, ev) => {
                if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar)) ev.Handled = true;
                if (ev.KeyChar == (char)Keys.Enter) { ev.Handled = true; card.Focus(); }
            };
            txtQty.Leave += (sender, ev) => {
                if (string.IsNullOrWhiteSpace(txtQty.Text) || txtQty.Text == "0") txtQty.Text = "1";
            };
            
            int GetInputQty()
            {
                if (int.TryParse(txtQty.Text, out int q) && q > 0) return q;
                return 1;
            }

            // -- + panel button (filled circle, primary color) -----------------
            Panel btnPlus = new Panel
            {
                Size = new Size(BTN_SIZE, BTN_SIZE),
                Cursor = Cursors.Hand,
                BackColor = cardBgColor,
            };
            btnPlus.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                pe.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                Color circleColor = outOfStock ? ThemeConfig.SecondaryColor : ThemeConfig.PrimaryColor;
                // Draw perfect circle: diameter = BTN_SIZE-2, centred 1px from each edge
                int d = BTN_SIZE - 2;
                int cx = 1;
                int cy = 1;
                using (var br = new SolidBrush(circleColor))
                    pe.Graphics.FillEllipse(br, cx, cy, d, d);
                // + glyph centred exactly inside the circle bounding rect
                using (var sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    using (var wb = new SolidBrush(Color.White))
                        pe.Graphics.DrawString("+", new Font("Segoe UI", 11F, FontStyle.Bold), wb,
                            new RectangleF(cx, cy, d, d), sf);
                }
            };

            // Disable qty controls when out of stock
            if (outOfStock)
            {
                btnPlus.Enabled = false;
                btnMinus.Enabled = false;
            }

            // -- Layout helper  called immediately AND on every resize --------
            void DoFooterLayout()
            {
                int fy = (FOOTER_H - BTN_SIZE) / 2;   // vertical centre
                int rEdge = CARD_W - 10;                               // Right ? Left: [?+] [qty] []
                btnPlus.Location = new Point(rEdge - BTN_SIZE, fy);
                txtQty.Location = new Point(rEdge - BTN_SIZE - 30, fy + 4);
                btnMinus.Location = new Point(rEdge - BTN_SIZE - 30 - BTN_SIZE, fy);

                // Price: left-aligned, width capped so it never overlaps btnMinus
                lblPrice.Location = new Point(10, fy);
                lblPrice.Width = btnMinus.Location.X - 10 - 4;  // 4px safety gap
            }

            // CRITICAL z-order: add lblPrice LAST so it becomes Controls[n-1] = BACK.
            // Controls[0] = FRONT (painted last/on top). If lblPrice were FRONT and its right
            // edge overlapped btnMinus, its transparent repaint would erase btnMinus's left border.
            // By being BACK, Windows clips lblPrice's DC to EXCLUDE the area of front controls.
            pnlFooterRow.Controls.AddRange(new Control[] { btnPlus, txtQty, btnMinus, lblPrice });

            // Position controls right now (before Resize ever fires)
            DoFooterLayout();

            // Also re-layout if the panel is ever resized at runtime
            pnlFooterRow.Resize += (s, ev) => DoFooterLayout();

            // ------------------------------------------------------------------
            // BADGES / OVERLAYS
            // ------------------------------------------------------------------
            if (outOfStock)
            {
                Label lblBadge = new Label
                {
                    Text = LocalizationManager.GetString("POS_OutOfStockBadge"),
                    Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = false,
                    Width = CARD_W - 20,
                    Height = 18,
                    Location = new Point(10, textSectionY + 2),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                lblBadge.Paint += (s, pe) =>
                {
                    pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = RoundedPath(new Rectangle(0, 0, lblBadge.Width - 1, lblBadge.Height - 1), 6))
                    using (var br = new SolidBrush(ThemeConfig.DangerColor))
                        pe.Graphics.FillPath(br, path);
                    TextRenderer.DrawText(pe.Graphics, lblBadge.Text, lblBadge.Font,
                        new Rectangle(0, 0, lblBadge.Width, lblBadge.Height), Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                };
                card.Controls.Add(lblBadge);
            }
            else if (!isService && part.QuantityInStock <= part.MinimumStockLevel && part.MinimumStockLevel > 0)
            {
                // "Low stock" hint shown as a small tinted label inside footer (above price)
                Label lblStock = new Label
                {
                    Text = string.Format(LocalizationManager.GetString("POS_LowStock"), part.QuantityInStock),
                    Font = new Font("Segoe UI", 7F),
                    ForeColor = ThemeConfig.WarningColor,
                    AutoSize = true,
                    Location = new Point(10, footerY - 14),
                    BackColor = Color.Transparent
                };
                card.Controls.Add(lblStock);
            }

            // ------------------------------------------------------------------
            // HOVER PROPAGATION
            // WinForms fires MouseLeave on 'card' the instant the cursor enters
            // any child control, so we must attach Enter/Leave to every
            // descendant as well.  We use a recursive helper called AFTER all
            // children have been added.
            // ------------------------------------------------------------------
            void SetHover(bool value)
            {
                hovered = value;
                card.Invalidate();
            }

            void PropagateHover(Control parent)
            {
                parent.MouseEnter += (s, e) => SetHover(true);
                parent.MouseLeave += (s, e) =>
                {
                    // Only clear hover if the cursor is truly outside the card
                    Point cursor = card.PointToClient(Control.MousePosition);
                    if (!card.ClientRectangle.Contains(cursor))
                        SetHover(false);
                };
                foreach (Control child in parent.Controls)
                    PropagateHover(child);
            }
            PropagateHover(card);

            // ------------------------------------------------------------------
            // CLICK HANDLERS  clicking image/text/card body adds to cart
            // ------------------------------------------------------------------
            Action addToCart = () => { 
                if (!outOfStock) {
                    AddToCart(part.Id, part.PartName, part.SellingPrice, part.QuantityInStock, GetInputQty(), isService); 
                    txtQty.Text = "1"; 
                }
            };

            card.Click += (s, e) => addToCart();
            pnlImageSection.Click += (s, e) => addToCart();
            pnlImgBg.Click += (s, e) => addToCart();

            pnlTextSection.Click += (s, e) => addToCart();
            lblCat.Click += (s, e) => addToCart();
            lblName.Click += (s, e) => addToCart();
            lblPrice.Click += (s, e) => addToCart();

            btnPlus.Click += (s, e) => { addToCart(); };
            btnMinus.Click += (s, e) => { 
                if (!outOfStock) {
                    RemoveOneFromCart(part.Id, GetInputQty()); 
                    txtQty.Text = "1"; 
                }
            };

            return card;
        }




        // ---------------------------------------------------------------------
        // CART DISPLAY  simple text rows matching reference design
        // ---------------------------------------------------------------------
        public void RefreshCartDisplay()
        {
            _orderPanel?.RefreshCartDisplay();
            UpdateProductCardQtyAll();
        }

        // ---------------------------------------------------------------------
        // HELPERS
        // ---------------------------------------------------------------------
        private int GetCartQty(int partId) => _orderPanel?.GetCartQty(partId) ?? 0;

        private void UpdateQtyControl(Control parent, int pid, int qty)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is Panel pnl && pnl.Name == "btnMinus_" + pid)
                {
                    pnl.Enabled = (qty > 0);
                    pnl.Invalidate();
                }
                else if (child is Label lbl && lbl.Tag is string tl && tl == "qtyLabel_" + pid)
                {
                    lbl.Text = qty.ToString();
                }
                if (child.HasChildren) UpdateQtyControl(child, pid, qty);
            }
        }

        private void UpdateProductCardQtyAll()
        {
            foreach (Control c in pnlProducts.Controls)
            {
                if (c is Panel card && card.Tag is int pid)
                {
                    int qty = GetCartQty(pid);
                    UpdateQtyControl(card, pid, qty);
                }
            }
        }

        private void SetCartQty(PartData part, int newQty)
        {
            bool isService = string.Equals(part.ItemType, "Service", StringComparison.OrdinalIgnoreCase);
            if (!isService && newQty > part.QuantityInStock) newQty = part.QuantityInStock;

            foreach (DataRow r in cartTable.Rows)
            {
                if (r.RowState != DataRowState.Deleted && (int)r["PartID"] == part.Id)
                {
                    if (newQty <= 0)
                        cartTable.Rows.Remove(r);
                    else
                        r["Quantity"] = newQty;
                    RefreshCartDisplay();
                    return;
                }
            }

            if (newQty > 0)
            {
                cartTable.Rows.Add(part.Id, part.PartName, newQty, 0, part.SellingPrice);
                RefreshCartDisplay();
            }
        }

        private void RemoveOneFromCart(int partId, int qtyToRem = 1)
        {
            foreach (DataRow r in cartTable.Rows)
            {
                if (r.RowState != DataRowState.Deleted && (int)r["PartID"] == partId)
                {
                    int q = (int)r["Quantity"];
                    if (q <= 1)
                        cartTable.Rows.Remove(r);
                    else
                        r["Quantity"] = q - 1;
                    break;
                }
            }
            RefreshCartDisplay();
        }

        private void RemoveFromCart(int partId)
        {
            DataRow toRemove = null;
            foreach (DataRow r in cartTable.Rows)
                if (r.RowState != DataRowState.Deleted && (int)r["PartID"] == partId)
                { toRemove = r; break; }
            if (toRemove != null) cartTable.Rows.Remove(toRemove);
            RefreshCartDisplay();
        }

        private Panel CreateSeparator()
        {
            Panel sep = new Panel { Dock = DockStyle.Fill, BackColor = ThemeConfig.POS_SeparatorColor, Height = 1 };
            return sep;
        }

        // Shared rounded path helper
        private static GraphicsPath RoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ---------------------------------------------------------------------
        // IMAGE LOADING
        // ---------------------------------------------------------------------
        private static Bitmap LoadProductImage(string imagePath, string categoryName, int size = 80)
        {
            return InventorySystem.Helpers.CacheManager.GetProductImage(imagePath, categoryName, size);
        }

        // ---------------------------------------------------------------------
        // INITIALIZE CART  (business logic  preserved)
        // ---------------------------------------------------------------------
        private void InitializeCart()
        {
            _orderPanel?.LoadCustomers();
        }

        // ---------------------------------------------------------------------
        // LOAD CUSTOMERS  (preserved)
        // ---------------------------------------------------------------------
        private void LoadCustomers()
        {
            _orderPanel?.LoadCustomers();
        }

        // ---------------------------------------------------------------------
        // APPLY PERMISSIONS  (preserved)
        // ---------------------------------------------------------------------
        private void ApplyPermissions() { }

        // ---------------------------------------------------------------------
        // REFRESH STATS  (preserved)
        // ---------------------------------------------------------------------
        public void RefreshStats()
        {
            try
            {
                if (cardTodayOrders != null)
                    cardTodayOrders.Value = _dashboardService.GetOrdersCount("Today").ToString();
                if (cardTodaySales != null)
                    cardTodaySales.Value = "$" + _dashboardService.GetSales("Today").ToString("N0");
                if (cardPending != null)
                    cardPending.Value = _dashboardService.GetPendingOrdersCount().ToString();
            }
            catch (Exception ex) { Console.WriteLine("Stats Error: " + ex.Message); }
        }

        // ---------------------------------------------------------------------
        // APPLY LOCALIZATION  (preserved + new controls)
        // ---------------------------------------------------------------------
        private void ApplyLocalization()
        {
            Func<string, string> L = LocalizationManager.GetString;
            bool isAr = LocalizationManager.IsArabic;

            if (_lblPageTitle != null)
            {
                string title = L("POS_PageTitle");
                _lblPageTitle.Text = title == "POS_PageTitle" ? "Point of Sale" : title;
                _lblPageTitle.RightToLeft = RightToLeft.No;
                _lblPageTitle.TextAlign = isAr ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
            }

            if (txtProductSearch != null)
                txtProductSearch.PlaceholderText = L("POS_SearchProducts");

            if (btnManageDrafts != null) btnManageDrafts.Text = L("POS_ManageDrafts");
            if (btnAddShipping != null)
            {
                btnAddShipping.Text = _shippingDetails != null
                    ? L("POS_ViewShipping")
                    : L("POS_AddShipping");
            }

            if (cardTodayOrders != null) cardTodayOrders.Title = L("POS_Orders");
            if (cardTodaySales != null) cardTodaySales.Title = L("POS_Sales");
            if (cardPending != null) cardPending.Title = L("POS_Pending");

            // Menu title — always refresh text; dock handles LTR/RTL side after ApplyRTL.
            if (lblCatTitle != null)
            {
                string menu = L("POS_MenuTitle");
                lblCatTitle.Text = menu == "POS_MenuTitle" ? "Menu" : menu;
                lblCatTitle.TextAlign = isAr ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
            }
            LayoutCatHeader();

            _orderPanel?.ApplyLocalization();

            if (pnlChips != null)
            {
                BuildCategoryChips();
            }
            if (pnlProducts != null)
            {
                pnlProducts.RightToLeft = RightToLeft.No;
                pnlProducts.FlowDirection = isAr ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                LoadProducts(txtProductSearch?.Text);
            }
        }

        /// <summary>
        /// Re-assert Menu/nav docking after MainForm.ApplyRTL so absolute-location
        /// mirroring cannot leave the header overlapping or stuck mid-strip.
        /// </summary>
        private void LayoutCatHeader()
        {
            if (pnlCatHeader == null || lblCatTitle == null) return;
            bool isAr = LocalizationManager.IsArabic;

            // Title stays on the content-start edge; nav on the content-end edge.
            lblCatTitle.Dock = isAr ? DockStyle.Right : DockStyle.Left;
            lblCatTitle.RightToLeft = RightToLeft.No;

            var nav = pnlCatHeader.Controls.Find("pnlPosCatNav", false).FirstOrDefault();
            if (nav != null)
            {
                nav.Dock = isAr ? DockStyle.Left : DockStyle.Right;
                nav.RightToLeft = RightToLeft.No;
                // Pin glyph buttons so ApplyRTL absolute mirroring cannot shuffle them.
                if (btnCatPrev != null) btnCatPrev.Location = new Point(4, 1);
                if (btnCatNext != null) btnCatNext.Location = new Point(38, 1);
            }

            pnlCatHeader.PerformLayout();
            lblCatTitle.BringToFront();
            if (nav != null) nav.BringToFront();
        }

        private void AddToCart(int id, string name, decimal price, int stock, int qtyToAdd = 1, bool isService = false)
        {
            _orderPanel?.AddToCart(id, name, price, stock, qtyToAdd, isService);
            UpdateProductCardQtyAll();
        }

        // ---------------------------------------------------------------------
        // UPDATE TOTAL
        // ---------------------------------------------------------------------
        private void UpdateTotal()
        {
            _orderPanel?.RefreshCartDisplay();
        }

        // ---------------------------------------------------------------------
        // CHECKOUT
        // ---------------------------------------------------------------------
        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (cartTable == null || cartTable.Rows.Count == 0)
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("CartEmpty", LocalizationManager.GetString("CartEmpty")));
                return;
            }

            decimal total = _orderPanel.GetGrandTotal();
            if (ModernMessageBox.Show(string.Format(LocalizationManager.GetString("ConfirmSale"), $"{total:N2}"), LocalizationManager.GetString("Msg_ConfirmCaption"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                return;

            try
            {
                List<OrderItem> items = new List<OrderItem>();
                foreach (DataRow row in cartTable.Rows)
                {
                    if (row.RowState == DataRowState.Deleted) continue;
                    items.Add(new OrderItem { PartId = (int)row["PartID"], Quantity = (int)row["Quantity"], UnitPrice = (decimal)row["SellingPrice"] });
                }
                int customerId = _orderPanel.SelectedCustomerId;
                DateTime? dDateC = _shippingDetails != null && !string.IsNullOrWhiteSpace(_shippingDetails.ShippingTo) ? _shippingDetails.DeliveryDate : (DateTime?)null;
                DateTime? pDateC = _shippingDetails != null && !string.IsNullOrWhiteSpace(_shippingDetails.ShippingTo) ? _shippingDetails.PaymentDueDate : (DateTime?)null;
                string sAddrC = _shippingDetails?.ShippingTo;
                int orderId = new OrderService().PlaceOrder(customerId, items, total, true, "Completed", pDateC, sAddrC, dDateC);
                DatabaseHelper.LogTransaction("SALE", "Order #" + orderId, "Paid Total: $" + total);
                InventoryBroadcaster.BroadcastStockChange("desktop-sale");
                MessageHelper.ShowSuccess("Order Sent! Order #" + orderId);
                _orderPanel.ClearActiveCart();
                RefreshStats();
            }
            catch (Exception ex) { MessageHelper.ShowError("Error: " + ex.Message); }
        }



        // ---------------------------------------------------------------------
        // PRINT RECEIPT  (preserved exactly)
        // ---------------------------------------------------------------------
        private void BtnPrintReceipt_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0) { MessageHelper.ShowWarning("Cart is empty!"); return; }
            System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();
            try { pd.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("Receipt", 315, 700); } catch { }
            pd.PrintPage += PrintReceiptPage;
            var preview = new PrintPreviewDialog
            {
                Document = pd,
                Text = LocalizationManager.GetString("POS_PrintReceipt")
            };
            ThemeConfig.ApplyPrintPreviewTheme(preview);
            preview.ShowDialog();
        }

        private void PrintReceiptPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Font fH = new Font("Segoe UI", 12, FontStyle.Bold), fS = new Font("Segoe UI", 9), fI = new Font("Consolas", 9);
            int y = 20, m = 10, w = Math.Min(e.PageBounds.Width, 300) - (m * 2);
            StringFormat cA = new StringFormat { Alignment = StringAlignment.Center }, rA = new StringFormat { Alignment = StringAlignment.Far };
            g.DrawString(ThemeConfig.CompanyName.ToUpper(), fH, Brushes.Black, new Rectangle(m, y, w, 25), cA); y += 30;
            g.DrawString("SALES RECEIPT", fS, Brushes.Black, new Rectangle(m, y, w, 20), cA); y += 20;
            g.DrawString(DateTime.Now.ToString("g"), fS, Brushes.Black, new Rectangle(m, y, w, 20), cA); y += 25;
            g.DrawLine(Pens.Black, m, y, m + w, y); y += 10;
            g.DrawString("QTY", fI, Brushes.Black, m, y); g.DrawString("ITEM", fI, Brushes.Black, m + 40, y); g.DrawString("PRICE", fI, Brushes.Black, new Rectangle(m, y, w, 20), rA);
            y += 20; g.DrawLine(Pens.Black, m, y, m + w, y); y += 10;
            foreach (DataRow r in cartTable.Rows)
            {
                string n = r["PartName"].ToString(); if (n.Length > 18) n = n.Substring(0, 15) + "...";
                g.DrawString(r["Quantity"].ToString(), fI, Brushes.Black, m, y); g.DrawString(n, fI, Brushes.Black, m + 40, y);
                g.DrawString(CurrencyService.Format((decimal)r["Total"]), fI, Brushes.Black, new Rectangle(m, y, w, 20), rA); y += 20;
            }
            y += 10; g.DrawLine(Pens.Black, m, y, m + w, y); y += 10;

            decimal s = 0; foreach (DataRow r in cartTable.Rows) if (r.RowState != DataRowState.Deleted) s += (decimal)r["Total"];
            decimal t = (_orderPanel?.ApplyVat ?? false) ? (s * 0.11m) : 0;
            decimal ship = (_orderPanel?.ApplyShipping ?? false) ? (_orderPanel?.ShippingAmount ?? 0) : 0;
            decimal disc = _orderPanel?.GetDiscountMoney() ?? 0;

            g.DrawString("Subtotal:", fS, Brushes.Black, m, y); g.DrawString(CurrencyService.Format(s), fS, Brushes.Black, new Rectangle(m, y, w, 20), rA); y += 20;
            if (t > 0) { g.DrawString("VAT (11%):", fS, Brushes.Black, m, y); g.DrawString(CurrencyService.Format(t), fS, Brushes.Black, new Rectangle(m, y, w, 20), rA); y += 20; }
            if (ship > 0) { g.DrawString("Shipping:", fS, Brushes.Black, m, y); g.DrawString(CurrencyService.Format(ship), fS, Brushes.Black, new Rectangle(m, y, w, 20), rA); y += 20; }
            if (disc > 0) { g.DrawString("Discount:", fS, Brushes.Black, m, y); g.DrawString("-" + CurrencyService.Format(disc), fS, Brushes.Black, new Rectangle(m, y, w, 20), rA); y += 20; }
            y += 5; g.DrawLine(Pens.Black, m, y, m + w, y); y += 10;

            g.DrawString("GRAND TOTAL:", fH, Brushes.Black, m, y); g.DrawString(CurrencyService.Format(Math.Max(0, s + t + ship - disc)), fH, Brushes.Black, new Rectangle(m, y, w, 25), rA);
            y += 40;
            if (_shippingDetails != null && !string.IsNullOrWhiteSpace(_shippingDetails.ShippingTo))
            {
                g.DrawLine(Pens.Black, m, y, m + w, y); y += 10;
                g.DrawString("--- SHIPPING DETAILS ---", fS, Brushes.Black, new Rectangle(m, y, w, 20), cA); y += 25;
                
                if (_shippingDetails.SelectedCustomerId > 0)
                {
                    string customerName = InventorySystem.DatabaseHelper.ExecuteScalar<string>($"SELECT COALESCE(full_name, '') FROM customers WHERE customer_id = {_shippingDetails.SelectedCustomerId}");
                    if (!string.IsNullOrEmpty(customerName))
                    {
                        g.DrawString("Customer: " + customerName, fS, Brushes.Black, new Rectangle(m, y, w, 20)); y += 20;
                    }
                }
                
                string address = _shippingDetails.ShippingTo.Replace("\r\n", ", ").Replace("\n", ", ");
                g.DrawString("Address: " + address, fS, Brushes.Black, new Rectangle(m, y, w, 40)); y += 40;
                g.DrawString("Delivery: " + _shippingDetails.DeliveryDate.ToShortDateString(), fS, Brushes.Black, m, y); y += 20;
                y += 10;
            }
            g.DrawString("Thank you!", fS, Brushes.Black, new Rectangle(m, y, w, 20), cA); e.HasMorePages = false;
        }



        // ---------------------------------------------------------------------
        // BARCODE SCANNER  (preserved exactly)
        // ---------------------------------------------------------------------
        private void POSForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!this.Visible) return;

            TimeSpan elapsed = DateTime.Now - _lastScanTime;
            if (elapsed.TotalMilliseconds > 100)
                _scanBuffer = "";
            _lastScanTime = DateTime.Now;

            if (e.KeyChar != (char)Keys.Enter)
                _scanBuffer += e.KeyChar;
        }

        public bool HandleKeyPress(Keys keyData)
        {
            if (keyData == Keys.Enter && this.Visible)
            {
                TimeSpan elapsed = DateTime.Now - _lastScanTime;
                if (elapsed.TotalMilliseconds <= 100 && !string.IsNullOrEmpty(_scanBuffer))
                {
                    string barcode = _scanBuffer.Trim();
                    _scanBuffer = "";

                    // Check if barcode is TM-A17 Scale Printed Barcode (EAN-13 prefix 20..29)
                    if (ScaleBarcodeParser.IsScaleBarcode(barcode))
                    {
                        var parsed = ScaleBarcodeParser.Parse(barcode);
                        if (parsed.IsSuccess)
                        {
                            DataTable dtScale = DatabaseHelper.ExecuteDataTable($"SELECT id,part_name,selling_price,quantity_in_stock,item_type FROM parts WHERE (barcode='{barcode}' OR part_number='{barcode}' OR part_number LIKE '%{parsed.ProductCode}%' OR barcode LIKE '%{parsed.ProductCode}%') AND date_deleted IS NULL");
                            if (dtScale.Rows.Count > 0)
                            {
                                DataRow r = dtScale.Rows[0];
                                string itemType = r["item_type"] != DBNull.Value ? r["item_type"].ToString() : "Product";
                                bool isService = string.Equals(itemType, "Service", StringComparison.OrdinalIgnoreCase);
                                decimal unitPrice = Convert.ToDecimal(r["selling_price"]);
                                int id = Convert.ToInt32(r["id"]);
                                string name = r["part_name"].ToString();
                                int stock = Convert.ToInt32(r["quantity_in_stock"]);

                                if (parsed.BarcodeType == ScaleBarcodeType.WeightBased)
                                {
                                    decimal weight = parsed.WeightKg;
                                    decimal finalPrice = Math.Round(unitPrice * weight, 2);
                                    int qty = (int)Math.Max(1, Math.Round(weight));
                                    AddToCart(id, $"{name} ({weight:N3}kg)", finalPrice, stock, qty, isService);
                                }
                                else if (parsed.BarcodeType == ScaleBarcodeType.PriceBased)
                                {
                                    AddToCart(id, name, parsed.TotalPrice, stock, 1, isService);
                                }
                                return true;
                            }
                        }
                    }

                    DataTable dt = DatabaseHelper.ExecuteDataTable($"SELECT id,part_name,selling_price,quantity_in_stock,item_type FROM parts WHERE (barcode='{barcode}' OR part_number='{barcode}') AND date_deleted IS NULL");
                    if (dt.Rows.Count > 0)
                    {
                        DataRow r = dt.Rows[0];
                        string itemType = r["item_type"] != DBNull.Value ? r["item_type"].ToString() : "Product";
                        bool isService = string.Equals(itemType, "Service", StringComparison.OrdinalIgnoreCase);
                        AddToCart(Convert.ToInt32(r["id"]), r["part_name"].ToString(), Convert.ToDecimal(r["selling_price"]), Convert.ToInt32(r["quantity_in_stock"]), 1, isService);
                    }
                    else
                    {
                        string notFoundMsg = LocalizationManager.GetString("POS_ProductNotFound", $"Item not found for barcode: {barcode}");
                        MessageHelper.ShowInfo(notFoundMsg);
                    }

                    return true; // Suppress Enter key so it doesn't click focused buttons
                }
            }
            return false;
        }

        // ---------------------------------------------------------------------
        // LOCAL GetRoundedRect alias  (kept for CreateCardPanel compatibility)
        // ---------------------------------------------------------------------
        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            return RoundedPath(rect, radius);
        }

        private Panel CreateCardPanel()
        {
            Panel p = new Panel();
            p.BackColor = ThemeConfig.SurfaceColor;
            p.BorderStyle = BorderStyle.None;
            p.Padding = new Padding(15);
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                Color parentColor = ThemeConfig.GetParentColor(p);
                using (var brush = new SolidBrush(parentColor))
                    e.Graphics.FillRectangle(brush, -1, -1, p.Width + 2, p.Height + 2);
                ThemeConfig.FillRoundedBackground(e.Graphics, p.ClientRectangle, 15f, ThemeConfig.SurfaceColor);
                ThemeConfig.DrawRoundedBorder(e.Graphics, p.ClientRectangle, 15f, ThemeConfig.BorderColor, 1f);
            };
            return p;
        }
    }
}
