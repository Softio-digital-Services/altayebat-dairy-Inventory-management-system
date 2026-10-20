using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using InventorySystem.Controls;
using InventorySystem.Helpers;
using InventorySystem.Services;

namespace InventorySystem.Forms
{
    /// <summary>
    /// Redesigned POS order rail: flat modern layout, scale-safe TableLayout,
    /// 4 concurrent orders, cart tall enough to show ~4 line items.
    /// </summary>
    public class PosOrderPanel : UserControl
    {
        private const int MaxOrders = 4;
        private const int CartRowH = 76;
        private const int PillControlH = 32;
        private const float CardCornerRadius = 12f;
        private const float ShellCornerRadius = 18f;
        private const float PillCornerRadius = 8f;
        private static readonly Color QtyAccent = Color.FromArgb(15, 118, 110);

        // Summary metrics are fixed so toggling shipping/discount editors never
        // resizes the block and never steals height from the cart list above it.
        private const int SummaryRowH = 27;
        private const int SummaryDividerH = 9;
        private const int SummaryTotalRowH = 34;
        private const int SummaryCurrencyRowH = 41;
        private const int SummaryPadTop = 10;
        private const int SummaryPadBottom = 8;
        private const int SummaryValueW = 78;
        private const int SummaryHeight = SummaryRowH * 4 + SummaryDividerH + SummaryTotalRowH
            + SummaryCurrencyRowH + SummaryPadTop + SummaryPadBottom;

        private static readonly Color RailBg = Color.FromArgb(255, 255, 255);
        private static readonly Color SoftBg = Color.FromArgb(246, 248, 251);
        private static readonly Color Line = Color.FromArgb(226, 232, 240);
        private static readonly Color Muted = Color.FromArgb(100, 116, 139);
        // Cart rows sit on a white rail, so the divider grey used elsewhere is too
        // faint to read as an edge. These are a couple of steps darker.
        private static readonly Color CardBorder = Color.FromArgb(209, 217, 228);
        private static readonly Color PillBorder = Color.FromArgb(190, 201, 216);

        private sealed class OrderSession
        {
            public DataTable Cart;
            public int CustomerId = -1;
            public bool ApplyVat;
            public bool ApplyShipping;
            public decimal ShippingAmount;
            public bool ApplyDiscount;
            public decimal DiscountAmount;
            public ShippingDetailsForm Shipping;
            public int DisplayNumber;

            public static DataTable CreateCart()
            {
                var t = new DataTable();
                t.Columns.Add("PartID", typeof(int));
                t.Columns.Add("PartName", typeof(string));
                t.Columns.Add("Quantity", typeof(int));
                t.Columns.Add("PrivatePrice", typeof(decimal));
                t.Columns.Add("SellingPrice", typeof(decimal));
                t.Columns.Add("Total", typeof(decimal), "Quantity * SellingPrice");
                return t;
            }
        }

        private readonly OrderSession[] _orders = new OrderSession[MaxOrders];
        private int _activeIndex;
        private int _nextOrderNumber = 1;

        private Panel _shell;
        private Label _lblOrderTitle;
        private PictureBox _btnTrash;
        private ModernComboBox _cmbCustomers;
        private Button _btnAddCustomer;
        private Panel _pnlCartHost;
        private Panel _pnlCartScroll;
        private FlowLayoutPanel _pnlCartItems;
        private Label _lblSubtotalTitle, _lblSubtotalVal;
        private Label _lblVatTitle, _lblTaxVal;
        private Label _lblShipTitle, _lblShippingVal;
        private Label _lblDiscountTitle, _lblDiscountVal;
        private Label _lblTotalTitle, _lblTotalVal;
        private CheckBox _chkVat, _chkShip, _chkDiscount;
        private NumericUpDown _numShipping, _numDiscount;
        private Panel _pnlShipAmount, _pnlDiscAmount;
        private Label _lblCurrency;
        private readonly List<Action> _summaryDirectionAppliers = new List<Action>();
        private ModernComboBox _cmbCurrency;
        private ModernButton _btnReturn, _btnDraft, _btnQuote, _btnBill;
        private ModernButton _btnPrint, _btnCheckout;

        public event EventHandler CheckoutClick;
        public event EventHandler PrintClick;
        public event EventHandler DraftClick;
        public event EventHandler QuoteClick;
        public event EventHandler BillClick;
        public event EventHandler ReturnClick;
        public event EventHandler CartChanged;

        public DataTable ActiveCart => _orders[_activeIndex].Cart;

        public int SelectedCustomerId
        {
            get
            {
                if (_cmbCustomers?.SelectedValue == null) return -1;
                return Convert.ToInt32(_cmbCustomers.SelectedValue);
            }
        }

        public bool ApplyVat => _chkVat != null && _chkVat.Checked;
        public bool ApplyShipping => _chkShip != null && _chkShip.Checked;
        public decimal ShippingAmount => _numShipping?.Value ?? 0m;
        public bool ApplyDiscount => _chkDiscount != null && _chkDiscount.Checked;
        /// <summary>Discount percent (0–100) from the POS input.</summary>
        public decimal DiscountAmount => _numDiscount?.Value ?? 0m;
        /// <summary>Money amount = subtotal × discount%.</summary>
        public decimal GetDiscountMoney()
        {
            if (!ApplyDiscount) return 0m;
            return Math.Round(GetSubtotal() * DiscountAmount / 100m, 2, MidpointRounding.AwayFromZero);
        }

        public ShippingDetailsForm ShippingDetails
        {
            get => _orders[_activeIndex].Shipping;
            set => _orders[_activeIndex].Shipping = value;
        }

        public PosOrderPanel()
        {
            for (int i = 0; i < MaxOrders; i++)
            {
                _orders[i] = new OrderSession
                {
                    Cart = OrderSession.CreateCart(),
                    DisplayNumber = _nextOrderNumber++
                };
            }

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            DoubleBuffered = true;
            Dock = DockStyle.Fill;
            BackColor = Color.Transparent;
            Padding = new Padding(8, 12, 12, 12);
            MinimumSize = new Size(280, 520);
            BuildUi();
            SelectOrder(0, force: true);
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            ApplyFlowDirections();
        }

        public void ApplyLocalization()
        {
            string L(string k, string fb) => LocalizationManager.GetString(k, fb);
            RightToLeft = LocalizationManager.IsArabic ? RightToLeft.Yes : RightToLeft.No;

            if (_lblOrderTitle != null) _lblOrderTitle.Text = L("POS_NewOrder", "New Order");
            if (_lblSubtotalTitle != null) _lblSubtotalTitle.Text = L("POS_Subtotal", "Subtotal");
            if (_lblVatTitle != null) _lblVatTitle.Text = L("POS_VAT", "VAT (11%)");
            if (_lblShipTitle != null) _lblShipTitle.Text = L("POS_Shipping", "Shipping");
            if (_lblDiscountTitle != null) _lblDiscountTitle.Text = L("POS_Discount", "Discount (%)");
            if (_lblTotalTitle != null) _lblTotalTitle.Text = L("POS_TotalPayable", "Total");
            if (_lblCurrency != null) _lblCurrency.Text = L("POS_Currency", "Currency");
            if (_btnReturn != null) _btnReturn.Text = L("POS_ItemReturn", "Return");
            if (_btnDraft != null) _btnDraft.Text = L("POS_SaveDraft", "Draft");
            if (_btnQuote != null) _btnQuote.Text = L("POS_Quotation", "Quote");
            if (_btnBill != null) _btnBill.Text = L("POS_CustomerBill", "Bill");
            if (_btnPrint != null) _btnPrint.Text = L("POS_Print", "Print");
            if (_btnCheckout != null) _btnCheckout.Text = L("POS_PlaceOrder", "Checkout");

            ApplyFlowDirections();
            ApplySummaryDirection();
            RefreshCartDisplay();
            LoadCustomers();
        }

        /// <summary>
        /// Re-asserts the summary column order and label alignment. Deferred once as
        /// well because the form-wide ApplyRTL pass runs after this and would set
        /// RightToLeft back to Yes on these rows.
        /// </summary>
        private void ApplySummaryDirection()
        {
            foreach (var apply in _summaryDirectionAppliers) apply();
            if (IsHandleCreated)
                BeginInvoke((Action)(() => { foreach (var apply in _summaryDirectionAppliers) apply(); }));
        }

        public void LoadCustomers()
        {
            if (_cmbCustomers == null) return;
            try
            {
                object prev = _cmbCustomers.SelectedValue;
                DataTable dt = DatabaseHelper.ExecuteDataTable("SELECT customer_id, full_name FROM customers ORDER BY full_name");
                DataRow row = dt.NewRow();
                row["customer_id"] = -1;
                row["full_name"] = LocalizationManager.GetString("POS_WalkIn", "Walk-in Customer");
                dt.Rows.InsertAt(row, 0);
                _cmbCustomers.ValueMember = "customer_id";
                _cmbCustomers.DisplayMember = "full_name";
                _cmbCustomers.DataSource = dt;

                int want = _orders[_activeIndex].CustomerId;
                if (prev != null && prev != DBNull.Value)
                    want = Convert.ToInt32(prev);
                SelectCustomer(want);
            }
            catch { }
        }

        public int GetCartQty(int partId)
        {
            var cart = ActiveCart;
            if (cart == null) return 0;
            foreach (DataRow r in cart.Rows)
                if (r.RowState != DataRowState.Deleted && (int)r["PartID"] == partId)
                    return (int)r["Quantity"];
            return 0;
        }

        public void AddToCart(int id, string name, decimal price, int stock, int qtyToAdd = 1, bool isService = false)
        {
            if (!isService && stock <= 0)
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("Error_OutOfStock"));
                return;
            }

            var cart = ActiveCart;
            foreach (DataRow r in cart.Rows)
            {
                if (r.RowState == DataRowState.Deleted) continue;
                if ((int)r["PartID"] != id) continue;

                int q = (int)r["Quantity"];
                if (!isService && q + qtyToAdd > stock)
                {
                    MessageHelper.ShowWarning(LocalizationManager.GetString("POS_NotEnoughStock", "Not enough stock."));
                    return;
                }
                r["Quantity"] = q + qtyToAdd;
                RefreshCartDisplay();
                return;
            }

            cart.Rows.Add(id, name, qtyToAdd, 0m, price);
            RefreshCartDisplay();
        }

        public void ClearActiveCart()
        {
            ActiveCart.Rows.Clear();
            RefreshCartDisplay();
        }

        public void RefreshCartDisplay()
        {
            if (_pnlCartItems == null) return;
            _pnlCartItems.SuspendLayout();
            foreach (Control c in _pnlCartItems.Controls)
                c.Dispose();
            _pnlCartItems.Controls.Clear();

            foreach (DataRow row in ActiveCart.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;
                Control line = CreateCartRow(row);
                HookCartWheel(line);
                _pnlCartItems.Controls.Add(line);
            }

            if (ActiveCart.Rows.Count == 0)
            {
                _pnlCartItems.Controls.Add(new Label
                {
                    Text = LocalizationManager.GetString("CartEmpty", "Cart is empty"),
                    AutoSize = false,
                    Width = CartRowWidth(),
                    Height = 40,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Muted,
                    Font = ThemeConfig.StandardFont,
                    Margin = new Padding(0, 24, 0, 0)
                });
            }

            _pnlCartItems.ResumeLayout(true);
            ResizeCartRows();
            UpdateTotal();
            CartChanged?.Invoke(this, EventArgs.Empty);
        }

        public decimal GetSubtotal()
        {
            decimal s = 0;
            foreach (DataRow r in ActiveCart.Rows)
                if (r.RowState != DataRowState.Deleted) s += (decimal)r["Total"];
            return s;
        }

        public decimal GetGrandTotal()
        {
            decimal s = GetSubtotal();
            decimal t = ApplyVat ? s * 0.11m : 0;
            decimal ship = ApplyShipping ? ShippingAmount : 0;
            decimal disc = GetDiscountMoney();
            return Math.Max(0, s + t + ship - disc);
        }

        // ==================================================================
        // UI
        // ==================================================================
        private void BuildUi()
        {
            _shell = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = RailBg,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            _shell.Paint += PaintShell;
            // Child panels fill their rectangle with the rail colour, which would
            // square off the rail corners, so clip the rail to its rounded shape.
            _shell.Resize += (s, e) => ApplyShellRegion();
            Controls.Add(_shell);
            ApplyShellRegion();

            // Dock layout runs from the last child to the first, so the Fill child
            // must be added first to end up with just the leftover space. Among the
            // edge-docked blocks, the last added sits at the physical bottom.
            BuildCartArea();
            _pnlCartHost.Dock = DockStyle.Fill;
            _shell.Controls.Add(_pnlCartHost);

            var totals = BuildTotalsBlock();
            totals.Dock = DockStyle.Bottom;
            totals.Height = SummaryHeight + 8; // + outer box padding
            _shell.Controls.Add(totals);

            var actions = BuildActionsBlock();
            actions.Dock = DockStyle.Bottom;
            actions.Height = 76;
            _shell.Controls.Add(actions);

            var footer = BuildFooterBlock();
            footer.Dock = DockStyle.Bottom;
            footer.Height = 52;
            _shell.Controls.Add(footer);

            var top = BuildTopBlock();
            top.Dock = DockStyle.Top;
            top.Height = 100; // title row + customer row + card padding
            _shell.Controls.Add(top);
        }

        private void ApplyShellRegion()
        {
            if (_shell == null || _shell.Width < 8 || _shell.Height < 8) return;
            using var path = ThemeConfig.GetRoundedPathPublic(new Rectangle(0, 0, _shell.Width, _shell.Height), ShellCornerRadius);
            var old = _shell.Region;
            _shell.Region = new Region(path);
            old?.Dispose();
            _shell.Invalidate();
        }

        private void PaintShell(object sender, PaintEventArgs e)
        {
            // The shell is Region-clipped, so fill the full bounds and inset the stroke.
            Rectangle bounds = _shell.ClientRectangle;
            ThemeConfig.FillRoundedBackground(e.Graphics, bounds, ShellCornerRadius, RailBg);
            ThemeConfig.DrawRoundedBorder(e.Graphics, bounds, ShellCornerRadius, Line, 1f);
        }

        private Control BuildTopBlock()
        {
            var outer = new Panel
            {
                BackColor = Color.Transparent,
                Padding = new Padding(12, 4, 12, 4)
            };
            var wrap = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(12, 9, 12, 9)
            };
            wrap.Paint += (s, e) => PaintSummaryCard(e.Graphics, wrap.ClientRectangle);
            outer.Controls.Add(wrap);

            var titleRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 34,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = Color.Transparent
            };
            titleRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            titleRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32F));

            var titles = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0, 4, 0, 0)
            };
            _lblOrderTitle = new Label
            {
                Text = LocalizationManager.GetString("POS_NewOrder", "New Order"),
                Font = new Font(ThemeConfig.AppFontFamily, 12F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                AutoSize = true,
                Margin = new Padding(0, 0, 8, 0),
                BackColor = Color.Transparent
            };
            titles.Controls.Add(_lblOrderTitle);
            titleRow.Controls.Add(titles, 0, 0);

            _btnTrash = new PictureBox
            {
                Image = ThemeConfig.GetNuricon("delete"),
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(26, 26),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.None,
                Margin = new Padding(0, 4, 0, 0)
            };
            _btnTrash.Click += (s, e) =>
            {
                if (ActiveCart.Rows.Count == 0) return;
                if (!MessageHelper.ConfirmAction(LocalizationManager.GetString("POS_ClearCartConfirm")))
                    return;
                ClearActiveCart();
            };
            titleRow.Controls.Add(_btnTrash, 1, 0);

            var custRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 38,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 4, 0, 0),
                BackColor = Color.Transparent
            };
            custRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            custRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38F));

            _cmbCustomers = new ModernComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                ShowLabel = false,
                Margin = new Padding(0, 2, 8, 2)
            };
            _cmbCustomers.InnerComboBox.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbCustomers.SelectedValue == null) return;
                _orders[_activeIndex].CustomerId = Convert.ToInt32(_cmbCustomers.SelectedValue);
            };
            custRow.Controls.Add(_cmbCustomers, 0, 0);

            _btnAddCustomer = new Button
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 0, 2),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            ThemeConfig.ApplyStandardAddButton(_btnAddCustomer, "");
            _btnAddCustomer.Click += (s, e) =>
            {
                using var f = new AddCustomerForm();
                if (f.ShowDialog() == DialogResult.OK)
                    LoadCustomers();
            };
            custRow.Controls.Add(_btnAddCustomer, 1, 0);

            // Dock Top stack: last added sits at the top edge.
            wrap.Controls.Add(custRow);
            wrap.Controls.Add(titleRow);

            return outer;
        }

        private void BuildCartArea()
        {
            // Host keeps the cart band inset to the same 12px gutter as the cards
            // above and below it instead of running into the rail border.
            _pnlCartHost = new Panel
            {
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(12, 2, 12, 2)
            };
            _pnlCartScroll = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SoftBg,
                Margin = new Padding(0),
                Padding = new Padding(8, 6, 4, 6)
            };
            _pnlCartScroll.Resize += (s, e) => ApplyCartRegion();
            _pnlCartHost.Controls.Add(_pnlCartScroll);
            // The flow panel owns the scrolling: a fixed-height Fill child with
            // AutoScroll gives a real vertical scrollbar once the lines overflow.
            _pnlCartItems = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            _pnlCartScroll.Controls.Add(_pnlCartItems);
            _pnlCartScroll.Resize += (s, e) => ResizeCartRows();
            _pnlCartItems.Resize += (s, e) => ResizeCartRows();
            _pnlCartItems.Layout += (s, e) => SuppressCartHScroll();
            ApplyCartRegion();
            SuppressCartHScroll();
        }

        private void ApplyCartRegion()
        {
            if (_pnlCartScroll == null || _pnlCartScroll.Width < 8 || _pnlCartScroll.Height < 8) return;
            using var path = ThemeConfig.GetRoundedPathPublic(
                new Rectangle(0, 0, _pnlCartScroll.Width, _pnlCartScroll.Height), CardCornerRadius);
            var old = _pnlCartScroll.Region;
            _pnlCartScroll.Region = new Region(path);
            old?.Dispose();
        }

        /// <summary>Row width always reserves the scrollbar gutter so it never jitters.</summary>
        private void ResizeCartRows()
        {
            if (_pnlCartItems == null) return;
            foreach (Control c in _pnlCartItems.Controls)
                c.Width = CartRowWidth();
            SuppressCartHScroll();
        }

        /// <summary>The cart only ever scrolls vertically; keep the bottom bar away.</summary>
        private void SuppressCartHScroll()
        {
            if (_pnlCartItems == null) return;
            _pnlCartItems.HorizontalScroll.Maximum = 0;
            _pnlCartItems.HorizontalScroll.Visible = false;
            _pnlCartItems.HorizontalScroll.Enabled = false;
            _pnlCartItems.AutoScrollMinSize = new Size(0, _pnlCartItems.AutoScrollMinSize.Height);
        }

        private int CartRowWidth()
        {
            int available = _pnlCartItems?.ClientSize.Width ?? 240;
            if (_pnlCartItems != null && !_pnlCartItems.VerticalScroll.Visible)
                available -= SystemInformation.VerticalScrollBarWidth;
            return Math.Max(200, available - 4);
        }

        /// <summary>
        /// Wheel messages go to the focused control, so rows forward them to the
        /// cart list to keep hover-scrolling working over inputs and buttons.
        /// </summary>
        private void HookCartWheel(Control c)
        {
            c.MouseWheel += CartRow_MouseWheel;
            foreach (Control child in c.Controls)
                HookCartWheel(child);
        }

        private void CartRow_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_pnlCartItems == null || !_pnlCartItems.VerticalScroll.Visible) return;
            var vs = _pnlCartItems.VerticalScroll;
            int target = Math.Min(vs.Maximum, Math.Max(vs.Minimum, vs.Value - e.Delta));
            _pnlCartItems.AutoScrollPosition = new Point(0, target);
        }

        private Control BuildTotalsBlock()
        {
            var box = new Panel
            {
                BackColor = Color.Transparent,
                Padding = new Padding(12, 4, 12, 4)
            };

            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(12, SummaryPadTop, 12, SummaryPadBottom)
            };
            card.Paint += (s, e) => PaintSummaryCard(e.Graphics, card.ClientRectangle);

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 7,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, SummaryRowH));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, SummaryRowH));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, SummaryRowH));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, SummaryRowH));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, SummaryDividerH));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, SummaryTotalRowH));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, SummaryCurrencyRowH));

            _lblSubtotalTitle = TinyLabel(LocalizationManager.GetString("POS_Subtotal", "Subtotal"));
            _lblSubtotalVal = TinyValue("$0.00");
            grid.Controls.Add(MoneyRow(_lblSubtotalTitle, _lblSubtotalVal, null, null), 0, 0);

            _chkVat = SoftCheck();
            _chkVat.CheckedChanged += (s, e) => { _orders[_activeIndex].ApplyVat = _chkVat.Checked; UpdateTotal(); };
            _lblVatTitle = TinyLabel(LocalizationManager.GetString("POS_VAT", "VAT (11%)"));
            _lblTaxVal = TinyValue("$0.00");
            grid.Controls.Add(MoneyRow(_lblVatTitle, _lblTaxVal, _chkVat, null), 0, 1);

            _chkShip = SoftCheck();
            _numShipping = AmountInput();
            _pnlShipAmount = WrapAmountInput(_numShipping);
            _chkShip.CheckedChanged += (s, e) =>
            {
                _orders[_activeIndex].ApplyShipping = _chkShip.Checked;
                _pnlShipAmount.Visible = _chkShip.Checked;
                UpdateTotal();
            };
            _numShipping.ValueChanged += (s, e) =>
            {
                _orders[_activeIndex].ShippingAmount = _numShipping.Value;
                UpdateTotal();
            };
            _lblShipTitle = TinyLabel(LocalizationManager.GetString("POS_Shipping", "Shipping"));
            _lblShippingVal = TinyValue("$0.00");
            grid.Controls.Add(MoneyRow(_lblShipTitle, _lblShippingVal, _chkShip, _pnlShipAmount), 0, 2);

            _chkDiscount = SoftCheck();
            _numDiscount = AmountInput();
            _numDiscount.Maximum = 100;
            _numDiscount.DecimalPlaces = 1;
            _pnlDiscAmount = WrapAmountInput(_numDiscount);
            _chkDiscount.CheckedChanged += (s, e) =>
            {
                _orders[_activeIndex].ApplyDiscount = _chkDiscount.Checked;
                _pnlDiscAmount.Visible = _chkDiscount.Checked;
                UpdateTotal();
            };
            _numDiscount.ValueChanged += (s, e) =>
            {
                _orders[_activeIndex].DiscountAmount = _numDiscount.Value;
                UpdateTotal();
            };
            _lblDiscountTitle = TinyLabel(LocalizationManager.GetString("POS_Discount", "Discount (%)"));
            _lblDiscountVal = TinyValue("$0.00");
            grid.Controls.Add(MoneyRow(_lblDiscountTitle, _lblDiscountVal, _chkDiscount, _pnlDiscAmount), 0, 3);

            var divider = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Margin = new Padding(0) };
            divider.Paint += (s, e) =>
            {
                using var pen = new Pen(Line, 1f);
                int y = divider.Height / 2;
                e.Graphics.DrawLine(pen, 0, y, divider.Width, y);
            };
            grid.Controls.Add(divider, 0, 4);

            _lblTotalTitle = TinyLabel(LocalizationManager.GetString("POS_TotalPayable", "Total"), true);
            _lblTotalVal = TinyValue("$0.00", true);
            grid.Controls.Add(MoneyRow(_lblTotalTitle, _lblTotalVal, null, null), 0, 5);

            var curr = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0, 3, 0, 0),
                BackColor = Color.Transparent
            };
            curr.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));
            curr.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64F));
            _lblCurrency = TinyLabel(LocalizationManager.GetString("POS_Currency", "Currency"));
            _lblCurrency.AutoSize = false;
            _lblCurrency.Dock = DockStyle.Fill;
            PinLeftToRight(_lblCurrency);
            _cmbCurrency = new ModernComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                ShowLabel = false,
                Margin = new Padding(0)
            };
            try
            {
                foreach (var c in CurrencyService.SupportedCurrencies)
                    _cmbCurrency.Items.Add(c.Code);
                int idx = _cmbCurrency.Items.IndexOf(CurrencyService.ActiveCurrency);
                _cmbCurrency.SelectedIndex = idx >= 0 ? idx : 0;
            }
            catch { }
            _cmbCurrency.InnerComboBox.SelectedIndexChanged += (s, e) =>
            {
                string selected = _cmbCurrency.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selected))
                {
                    CurrencyService.ActiveCurrency = selected;
                    RefreshCartDisplay();
                }
            };
            curr.Controls.Add(_lblCurrency, 0, 0);
            curr.Controls.Add(_cmbCurrency, 1, 0);

            void ApplyCurrencyDirection()
            {
                bool ar = LocalizationManager.IsArabic;
                curr.SuspendLayout();
                curr.RightToLeft = RightToLeft.No;
                // Caption hugs the outer edge of the card, selector takes the rest.
                curr.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, ar ? 64F : 36F);
                curr.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, ar ? 36F : 64F);
                curr.SetColumn(_cmbCurrency, ar ? 0 : 1);
                curr.SetColumn(_lblCurrency, ar ? 1 : 0);
                _lblCurrency.RightToLeft = RightToLeft.No;
                _lblCurrency.TextAlign = ar ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
                _lblCurrency.Margin = ar ? new Padding(6, 0, 0, 0) : new Padding(0, 0, 6, 0);
                curr.ResumeLayout(true);
            }

            ApplyCurrencyDirection();
            _summaryDirectionAppliers.Add(ApplyCurrencyDirection);
            curr.RightToLeftChanged += (s, e) => { if (curr.RightToLeft != RightToLeft.No) ApplyCurrencyDirection(); };
            grid.Controls.Add(curr, 0, 6);

            card.Controls.Add(grid);
            box.Controls.Add(card);
            return box;
        }

        private Control BuildActionsBlock()
        {
            var grid = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(14, 4, 14, 4),
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            _btnReturn = ActionBtn(LocalizationManager.GetString("POS_ItemReturn", "Return"), Color.FromArgb(220, 38, 38));
            _btnDraft = ActionBtn(LocalizationManager.GetString("POS_SaveDraft", "Draft"), Color.FromArgb(124, 58, 237));
            _btnQuote = ActionBtn(LocalizationManager.GetString("POS_Quotation", "Quote"), Color.FromArgb(37, 99, 235));
            _btnBill = ActionBtn(LocalizationManager.GetString("POS_CustomerBill", "Bill"), ThemeConfig.PrimaryColor);

            _btnReturn.Click += (s, e) => ReturnClick?.Invoke(this, EventArgs.Empty);
            _btnDraft.Click += (s, e) => DraftClick?.Invoke(this, EventArgs.Empty);
            _btnQuote.Click += (s, e) => QuoteClick?.Invoke(this, EventArgs.Empty);
            _btnBill.Click += (s, e) => BillClick?.Invoke(this, EventArgs.Empty);

            grid.Controls.Add(_btnReturn, 0, 0);
            grid.Controls.Add(_btnDraft, 1, 0);
            grid.Controls.Add(_btnQuote, 0, 1);
            grid.Controls.Add(_btnBill, 1, 1);
            return grid;
        }

        private Control BuildFooterBlock()
        {
            var row = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(14, 4, 14, 12),
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));

            _btnPrint = new ModernButton
            {
                Text = LocalizationManager.GetString("POS_Print", "Print"),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 6, 0),
                Cursor = Cursors.Hand,
                Font = new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Bold)
            };
            ThemeConfig.ApplyPaletteButton(_btnPrint, ThemeConfig.SuccessColor);
            _btnPrint.Click += (s, e) => PrintClick?.Invoke(this, EventArgs.Empty);

            _btnCheckout = new ModernButton
            {
                Text = LocalizationManager.GetString("POS_PlaceOrder", "Checkout"),
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 0, 0, 0),
                Cursor = Cursors.Hand,
                Font = new Font(ThemeConfig.AppFontFamily, 9.5F, FontStyle.Bold)
            };
            ThemeConfig.ApplyPaletteButton(_btnCheckout, ThemeConfig.SuccessColor);
            _btnCheckout.Click += (s, e) => CheckoutClick?.Invoke(this, EventArgs.Empty);

            row.Controls.Add(_btnPrint, 0, 0);
            row.Controls.Add(_btnCheckout, 1, 0);
            return row;
        }

        private Control CreateCartRow(DataRow row)
        {
            int partId = (int)row["PartID"];
            string partName = row["PartName"]?.ToString() ?? "";
            int qty = (int)row["Quantity"];
            decimal price = (decimal)row["SellingPrice"];
            decimal total = (decimal)row["Total"];
            int width = CartRowWidth();
            bool rtl = LocalizationManager.IsArabic;

            var card = new Panel
            {
                Width = width,
                Height = CartRowH,
                Margin = new Padding(0, 0, 0, 8),
                BackColor = Color.Transparent
            };
            card.Paint += (s, pe) => PaintRoundedCard(pe.Graphics, card.ClientRectangle);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(12, 7, 12, 7),
                Margin = Padding.Empty,
                BackColor = Color.Transparent,
                RightToLeft = rtl ? RightToLeft.Yes : RightToLeft.No
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 21F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, PillControlH + 4));

            var top = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                BackColor = Color.Transparent
            };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            top.Controls.Add(new Label
            {
                Text = partName,
                Dock = DockStyle.Fill,
                Font = new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                TextAlign = rtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                BackColor = Color.Transparent
            }, 0, 0);
            top.Controls.Add(new Label
            {
                Text = CurrencyService.Format(total),
                AutoSize = true,
                Font = new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent,
                Margin = new Padding(8, 0, 0, 0)
            }, 1, 0);
            layout.Controls.Add(top, 0, 0);

            var bottom = BuildLineControls(partId, qty, price, rtl);
            layout.Controls.Add(bottom, 0, 1);
            card.Controls.Add(layout);
            return card;
        }

        /// <summary>
        /// Bottom control strip: quantity stepper on the left, editable price box
        /// (with currency symbol) on the right, remove action at the far end.
        /// Positioned manually so the pieces stay aligned and centered at any width.
        /// </summary>
        private Panel BuildLineControls(int partId, int qty, decimal price, bool rtl)
        {
            var strip = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                BackColor = Color.Transparent
            };

            var priceBox = new Panel
            {
                Height = PillControlH,
                BackColor = Color.Transparent
            };
            var lblCurrency = new Label
            {
                Text = CurrencyService.GetSymbol("USD"),
                AutoSize = false,
                Font = new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Bold),
                ForeColor = Muted,
                BackColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                RightToLeft = RightToLeft.No
            };
            var txtPrice = new TextBox
            {
                Text = price.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                BorderStyle = BorderStyle.None,
                Font = new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                BackColor = Color.White,
                TextAlign = rtl ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                RightToLeft = RightToLeft.No
            };
            priceBox.Controls.Add(lblCurrency);
            priceBox.Controls.Add(txtPrice);
            priceBox.Paint += (s, e) => PaintItemPill(e.Graphics, priceBox.ClientRectangle);
            AttachPriceTierMenu(priceBox, txtPrice, partId);
            lblCurrency.Click += (s, e) => txtPrice.Focus();
            txtPrice.KeyPress += (s, e) =>
            {
                if (char.IsControl(e.KeyChar)) return;
                if (char.IsDigit(e.KeyChar) || e.KeyChar == '.') return;
                e.Handled = true;
            };
            txtPrice.Leave += (s, e) => ApplyPriceText(partId, txtPrice.Text);
            txtPrice.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ApplyPriceText(partId, txtPrice.Text);
                }
            };

            var btnMinus = CreateStepButton("−");
            var txtQty = new TextBox
            {
                Text = qty.ToString(),
                BorderStyle = BorderStyle.None,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font(ThemeConfig.AppFontFamily, 9.5F, FontStyle.Bold),
                ForeColor = QtyAccent,
                BackColor = Color.White,
                RightToLeft = RightToLeft.No
            };
            var btnPlus = CreateStepButton("+");
            btnMinus.Click += (s, e) => AdjustQty(partId, -1);
            btnPlus.Click += (s, e) => AdjustQty(partId, +1);
            txtQty.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
                if (e.KeyChar == (char)Keys.Enter) { e.Handled = true; ApplyQtyText(partId, txtQty.Text); }
            };
            txtQty.Leave += (s, e) => ApplyQtyText(partId, txtQty.Text);

            var btnDelete = new PictureBox
            {
                Image = ThemeConfig.GetNuricon("delete"),
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(20, 20),
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            btnDelete.Click += (s, e) => RemoveCartLine(partId);

            strip.Controls.Add(priceBox);
            strip.Controls.Add(btnMinus);
            strip.Controls.Add(txtQty);
            strip.Controls.Add(btnPlus);
            strip.Controls.Add(btnDelete);

            void LayoutStrip()
            {
                int w = strip.ClientSize.Width;
                int h = strip.ClientSize.Height;
                if (w < 40 || h < 10) return;

                const int step = 24;
                const int qtyW = 30;
                const int trashW = 22;
                const int gap = 6;
                const int symW = 16;

                int rowH = Math.Min(PillControlH, h);
                int top = Math.Max(0, (h - rowH) / 2);
                int qtyGroupW = step + qtyW + step;
                int priceW = Math.Max(72, w - qtyGroupW - trashW - gap * 3);

                // A borderless single-line TextBox clamps its own height to the font,
                // so centre on that value instead of a fixed guess.
                int qtyH = txtQty.PreferredHeight;
                int priceH = txtPrice.PreferredHeight;

                // Mirrors every x-position for Arabic without duplicating the math.
                int MirrorX(int x, int width) => rtl ? w - x - width : x;

                int cursor = 0;
                btnMinus.SetBounds(MirrorX(cursor, step), top, step, rowH);
                cursor += step;
                txtQty.SetBounds(MirrorX(cursor, qtyW), top + Math.Max(0, (rowH - qtyH) / 2), qtyW, qtyH);
                cursor += qtyW;
                btnPlus.SetBounds(MirrorX(cursor, step), top, step, rowH);
                cursor += step + gap;

                priceBox.SetBounds(MirrorX(cursor, priceW), top, priceW, rowH);
                int symX = rtl ? priceW - symW - 6 : 6;
                int valX = rtl ? 6 : symX + symW;
                // Currency label spans the pill so its glyph centres on the same
                // optical line as the editable amount.
                lblCurrency.SetBounds(symX, 0, symW, rowH);
                txtPrice.SetBounds(valX, Math.Max(0, (rowH - priceH) / 2), Math.Max(30, priceW - symW - 12), priceH);
                cursor += priceW + gap;

                btnDelete.SetBounds(MirrorX(cursor, trashW), top + Math.Max(0, (rowH - btnDelete.Height) / 2),
                    trashW, btnDelete.Height);
            }

            strip.Resize += (s, e) => LayoutStrip();
            strip.HandleCreated += (s, e) => LayoutStrip();
            return strip;
        }

        private static Button CreateStepButton(string symbol)
        {
            var btn = new Button
            {
                Text = symbol,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(ThemeConfig.AppFontFamily, 11F, FontStyle.Bold),
                ForeColor = Muted,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                TabStop = false,
                Margin = Padding.Empty,
                UseVisualStyleBackColor = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btn.MouseEnter += (s, e) => btn.ForeColor = ThemeConfig.TextColorDark;
            btn.MouseLeave += (s, e) => btn.ForeColor = Muted;
            return btn;
        }

        /// <summary>
        /// Rounded path on float coordinates. Insetting by half a pixel centres an
        /// anti-aliased 1px stroke on a single pixel column instead of smearing it
        /// across two at half opacity, which is what made these borders vanish.
        /// </summary>
        private static GraphicsPath RoundedPathF(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float d = Math.Min(radius * 2f, Math.Min(r.Width, r.Height));
            if (d <= 0.1f) d = 1f;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static void PaintRoundedCard(Graphics g, Rectangle bounds)
        {
            if (bounds.Width < 3 || bounds.Height < 3) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new RectangleF(0.5f, 0.5f, bounds.Width - 1.5f, bounds.Height - 1.5f);
            using var path = RoundedPathF(rect, CardCornerRadius);
            using var br = new SolidBrush(Color.White);
            using var pen = new Pen(CardBorder, 1.2f);
            g.FillPath(br, path);
            g.DrawPath(pen, path);
        }

        private static void PaintItemPill(Graphics g, Rectangle bounds)
        {
            if (bounds.Width < 3 || bounds.Height < 3) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new RectangleF(0.5f, 0.5f, bounds.Width - 1.5f, bounds.Height - 1.5f);
            float r = Math.Min(PillCornerRadius, Math.Max(4f, rect.Height / 2f));
            using var path = RoundedPathF(rect, r);
            using var br = new SolidBrush(Color.White);
            using var pen = new Pen(PillBorder, 1.3f);
            g.FillPath(br, path);
            g.DrawPath(pen, path);
        }

        private void RemoveCartLine(int partId)
        {
            foreach (DataRow r in ActiveCart.Rows)
            {
                if (r.RowState == DataRowState.Deleted || (int)r["PartID"] != partId) continue;
                ActiveCart.Rows.Remove(r);
                break;
            }
            RefreshCartDisplay();
        }

        private void AttachPriceTierMenu(Control host, TextBox txtPrice, int partId)
        {
            var menu = new ContextMenuStrip();
            try
            {
                DataTable dtPrices = DatabaseHelper.ExecuteDataTable(
                    $"SELECT selling_price, price2, price3, price4 FROM parts WHERE id = {partId}");
                var tiers = new List<decimal>();
                if (dtPrices.Rows.Count > 0)
                {
                    DataRow pr = dtPrices.Rows[0];
                    void AddTier(object o)
                    {
                        if (o == null || o == DBNull.Value) return;
                        decimal v = Convert.ToDecimal(o);
                        if (v > 0 && !tiers.Contains(v)) tiers.Add(v);
                    }
                    AddTier(pr["selling_price"]);
                    AddTier(pr["price2"]);
                    AddTier(pr["price3"]);
                    AddTier(pr["price4"]);
                }
                foreach (decimal tier in tiers)
                {
                    decimal captured = tier;
                    menu.Items.Add(CurrencyService.Format(captured), null, (s, e) => ApplyPriceText(partId, captured.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)));
                }
            }
            catch { }

            if (menu.Items.Count > 0)
            {
                host.ContextMenuStrip = menu;
                txtPrice.ContextMenuStrip = menu;
            }
        }

        private void ApplyPriceText(int partId, string text)
        {
            string t = (text ?? "").Trim();
            if (!decimal.TryParse(t, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.CurrentCulture, out decimal p)
                && !decimal.TryParse(t, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out p))
            {
                RefreshCartDisplay();
                return;
            }
            if (p <= 0)
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("POS_InvalidPrice", "Enter a valid price greater than zero."));
                RefreshCartDisplay();
                return;
            }
            SetLinePrice(partId, p, forceRefresh: true);
        }

        private void SetLinePrice(int partId, decimal price, bool forceRefresh = false)
        {
            foreach (DataRow r in ActiveCart.Rows)
            {
                if (r.RowState == DataRowState.Deleted || (int)r["PartID"] != partId) continue;
                if (!forceRefresh && (decimal)r["SellingPrice"] == price) return;
                r["SellingPrice"] = price;
                break;
            }
            RefreshCartDisplay();
        }

        private void AdjustQty(int partId, int delta)
        {
            foreach (DataRow r in ActiveCart.Rows)
            {
                if (r.RowState == DataRowState.Deleted || (int)r["PartID"] != partId) continue;
                int q = (int)r["Quantity"] + delta;
                if (q <= 0) ActiveCart.Rows.Remove(r);
                else
                {
                    if (delta > 0 && !CanIncrease(partId, q))
                    {
                        MessageHelper.ShowWarning(LocalizationManager.GetString("POS_NotEnoughStock", "Not enough stock."));
                        return;
                    }
                    r["Quantity"] = q;
                }
                break;
            }
            RefreshCartDisplay();
        }

        private void ApplyQtyText(int partId, string text)
        {
            if (!int.TryParse(text, out int q) || q < 0) q = 1;
            foreach (DataRow r in ActiveCart.Rows)
            {
                if (r.RowState == DataRowState.Deleted || (int)r["PartID"] != partId) continue;
                if (q == 0) { ActiveCart.Rows.Remove(r); break; }
                if (q > (int)r["Quantity"] && !CanIncrease(partId, q))
                {
                    MessageHelper.ShowWarning(LocalizationManager.GetString("POS_NotEnoughStock", "Not enough stock."));
                    RefreshCartDisplay();
                    return;
                }
                r["Quantity"] = q;
                break;
            }
            RefreshCartDisplay();
        }

        private bool CanIncrease(int partId, int newQty)
        {
            try
            {
                DataTable partDt = DatabaseHelper.ExecuteDataTable(
                    $"SELECT quantity_in_stock, item_type FROM parts WHERE id={partId}");
                if (partDt.Rows.Count == 0) return true;
                string itemType = partDt.Rows[0]["item_type"] != DBNull.Value
                    ? partDt.Rows[0]["item_type"].ToString() : "Product";
                if (string.Equals(itemType, "Service", StringComparison.OrdinalIgnoreCase))
                    return true;
                return newQty <= Convert.ToInt32(partDt.Rows[0]["quantity_in_stock"]);
            }
            catch { return true; }
        }

        private void SelectOrder(int index, bool force = false)
        {
            if (index < 0 || index >= MaxOrders) return;
            if (!force && index == _activeIndex) return;

            _orders[_activeIndex].ApplyVat = _chkVat?.Checked ?? false;
            _orders[_activeIndex].ApplyShipping = _chkShip?.Checked ?? false;
            _orders[_activeIndex].ShippingAmount = _numShipping?.Value ?? 0;
            _orders[_activeIndex].ApplyDiscount = _chkDiscount?.Checked ?? false;
            _orders[_activeIndex].DiscountAmount = _numDiscount?.Value ?? 0;
            if (_cmbCustomers?.SelectedValue != null)
                _orders[_activeIndex].CustomerId = Convert.ToInt32(_cmbCustomers.SelectedValue);

            _activeIndex = index;
            var sess = _orders[_activeIndex];

            if (_chkVat != null) _chkVat.Checked = sess.ApplyVat;
            if (_chkShip != null) _chkShip.Checked = sess.ApplyShipping;
            if (_numShipping != null)
            {
                _numShipping.Value = Math.Min(_numShipping.Maximum, Math.Max(_numShipping.Minimum, sess.ShippingAmount));
                if (_pnlShipAmount != null) _pnlShipAmount.Visible = sess.ApplyShipping;
            }
            if (_chkDiscount != null) _chkDiscount.Checked = sess.ApplyDiscount;
            if (_numDiscount != null)
            {
                _numDiscount.Value = Math.Min(_numDiscount.Maximum, Math.Max(_numDiscount.Minimum, sess.DiscountAmount));
                if (_pnlDiscAmount != null) _pnlDiscAmount.Visible = sess.ApplyDiscount;
            }
            SelectCustomer(sess.CustomerId);
            RefreshCartDisplay();
        }

        private void SelectCustomer(int customerId)
        {
            if (_cmbCustomers?.DataSource == null) return;
            try { _cmbCustomers.SelectedValue = customerId; }
            catch { _cmbCustomers.SelectedValue = -1; }
        }

        private void UpdateTotal()
        {
            decimal s = GetSubtotal();
            decimal t = ApplyVat ? s * 0.11m : 0;
            decimal ship = ApplyShipping ? ShippingAmount : 0;
            decimal disc = GetDiscountMoney();
            if (_lblSubtotalVal != null) _lblSubtotalVal.Text = CurrencyService.Format(s);
            if (_lblTaxVal != null)
            {
                _lblTaxVal.Text = CurrencyService.Format(t);
                _lblTaxVal.ForeColor = ApplyVat ? ThemeConfig.TextColorDark : Muted;
            }
            if (_lblShippingVal != null)
            {
                _lblShippingVal.Text = CurrencyService.Format(ship);
                _lblShippingVal.Visible = !ApplyShipping;
            }
            if (_lblDiscountVal != null)
            {
                _lblDiscountVal.Text = disc > 0 ? "-" + CurrencyService.Format(disc) : CurrencyService.Format(0);
                _lblDiscountVal.Visible = !ApplyDiscount;
                _lblDiscountVal.ForeColor = disc > 0 ? Color.FromArgb(220, 38, 38) : Muted;
            }
            if (_lblTotalVal != null) _lblTotalVal.Text = CurrencyService.Format(Math.Max(0, s + t + ship - disc));
        }

        private void ApplyFlowDirections()
        {
            bool ar = LocalizationManager.IsArabic;
            ContentAlignment titleAlign = ar ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
            ContentAlignment valueAlign = ar ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleRight;

            foreach (var lbl in new[] { _lblCurrency, _lblSubtotalTitle, _lblVatTitle, _lblShipTitle, _lblDiscountTitle, _lblTotalTitle })
                if (lbl != null) lbl.TextAlign = titleAlign;

            foreach (var lbl in new[] { _lblSubtotalVal, _lblTaxVal, _lblShippingVal, _lblDiscountVal, _lblTotalVal })
                if (lbl != null) lbl.TextAlign = valueAlign;

            foreach (var lbl in new[] { _lblSubtotalTitle, _lblVatTitle, _lblShipTitle, _lblDiscountTitle, _lblTotalTitle })
                if (lbl?.Parent is TableLayoutPanel row)
                    row.RightToLeft = ar ? RightToLeft.Yes : RightToLeft.No;
        }

        private static Label TinyLabel(string text, bool strong = false)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font(ThemeConfig.AppFontFamily, strong ? 10F : 8.5F, FontStyle.Bold),
                ForeColor = strong ? ThemeConfig.TextColorDark : Muted,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
        }

        private static Label TinyValue(string text, bool strong = false)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font(ThemeConfig.AppFontFamily, strong ? 11F : 8.5F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent,
                Margin = new Padding(6, 0, 0, 0)
            };
        }

        /// <summary>
        /// Keeps a label out of the RTL mirror for good. A Label with RightToLeft.Yes
        /// renders TextAlign on the opposite side, so the alignment we pick here would
        /// otherwise invert as soon as the form-wide RTL pass touches it.
        /// </summary>
        private static void PinLeftToRight(Control c)
        {
            c.RightToLeft = RightToLeft.No;
            c.RightToLeftChanged += (s, e) =>
            {
                if (c.RightToLeft != RightToLeft.No) c.RightToLeft = RightToLeft.No;
            };
        }

        /// <summary>
        /// One summary line: optional toggle, caption, optional inline amount editor,
        /// and a fixed-width value column so every figure shares one edge.
        /// Columns are ordered visually rather than left to RightToLeft mirroring,
        /// because mirroring also flips each label's TextAlign and pushed the Arabic
        /// captions to the inside of the card.
        /// </summary>
        private TableLayoutPanel MoneyRow(Label title, Label value, CheckBox chk, Panel amount)
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            for (int i = 0; i < 4; i++) row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0F));

            title.AutoSize = false;
            title.Dock = DockStyle.Fill;
            value.AutoSize = false;
            value.Dock = DockStyle.Fill;
            PinLeftToRight(title);
            PinLeftToRight(value);

            if (chk != null) row.Controls.Add(chk, 0, 0);
            row.Controls.Add(title, 1, 0);
            if (amount != null) row.Controls.Add(amount, 2, 0);
            row.Controls.Add(value, 3, 0);

            void Apply()
            {
                bool ar = LocalizationManager.IsArabic;
                row.SuspendLayout();
                row.RightToLeft = RightToLeft.No;

                int cValue = ar ? 0 : 3;
                int cAmount = ar ? 1 : 2;
                int cTitle = ar ? 2 : 1;
                int cCheck = ar ? 3 : 0;

                row.ColumnStyles[cValue] = new ColumnStyle(SizeType.Absolute, SummaryValueW);
                row.ColumnStyles[cAmount] = new ColumnStyle(SizeType.AutoSize);
                row.ColumnStyles[cTitle] = new ColumnStyle(SizeType.Percent, 100F);
                row.ColumnStyles[cCheck] = new ColumnStyle(SizeType.Absolute, chk == null ? 0F : 22F);

                if (chk != null) row.SetColumn(chk, cCheck);
                row.SetColumn(title, cTitle);
                if (amount != null) row.SetColumn(amount, cAmount);
                row.SetColumn(value, cValue);

                title.RightToLeft = RightToLeft.No;
                title.TextAlign = ar ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
                value.RightToLeft = RightToLeft.No;
                value.TextAlign = ar ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleRight;
                value.Margin = ar ? new Padding(0, 0, 4, 0) : new Padding(4, 0, 0, 0);
                row.ResumeLayout(true);
            }

            Apply();
            _summaryDirectionAppliers.Add(Apply);
            // The form-wide RTL pass sets RightToLeft.Yes on every control, which mirrors
            // these columns straight back. Re-assert whenever that happens.
            row.RightToLeftChanged += (s, e) => { if (row.RightToLeft != RightToLeft.No) Apply(); };
            return row;
        }

        private static CheckBox SoftCheck()
        {
            return new CheckBox
            {
                AutoSize = false,
                Size = new Size(16, 16),
                Anchor = AnchorStyles.None,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
        }

        private static NumericUpDown AmountInput()
        {
            return new NumericUpDown
            {
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 999999,
                BorderStyle = BorderStyle.None,
                TextAlign = HorizontalAlignment.Center,
                BackColor = Color.White,
                ForeColor = ThemeConfig.TextColorDark,
                Font = new Font(ThemeConfig.AppFontFamily, 8.5F, FontStyle.Bold)
            };
        }

        /// <summary>Puts an amount editor inside a rounded box matching the cart pills.</summary>
        private static Panel WrapAmountInput(NumericUpDown num)
        {
            var wrap = new Panel
            {
                Width = 86,
                Height = SummaryRowH - 5,
                Anchor = AnchorStyles.None,
                Margin = new Padding(6, 0, 6, 0),
                BackColor = Color.Transparent,
                Visible = false
            };
            wrap.Controls.Add(num);
            void Layout()
            {
                num.SetBounds(5, Math.Max(0, (wrap.Height - num.Height) / 2),
                    Math.Max(30, wrap.Width - 10), num.Height);
            }
            wrap.Resize += (s, e) => Layout();
            wrap.HandleCreated += (s, e) => Layout();
            wrap.VisibleChanged += (s, e) => Layout();
            wrap.Paint += (s, e) => PaintItemPill(e.Graphics, wrap.ClientRectangle);
            Layout();
            return wrap;
        }

        private static void PaintSummaryCard(Graphics g, Rectangle bounds)
        {
            ThemeConfig.FillRoundedBackground(g, bounds, CardCornerRadius, SoftBg);
            ThemeConfig.DrawRoundedBorder(g, bounds, CardCornerRadius, Line, 1f);
        }

        private static ModernButton ActionBtn(string text, Color color)
        {
            var btn = new ModernButton
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(3),
                Cursor = Cursors.Hand,
                Font = new Font(ThemeConfig.AppFontFamily, 8F, FontStyle.Bold)
            };
            ThemeConfig.ApplyPaletteButton(btn, color);
            return btn;
        }
    }
}
