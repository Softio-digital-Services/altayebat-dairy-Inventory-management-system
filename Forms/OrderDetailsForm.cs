using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Controls;
using InventorySystem.Data;
using InventorySystem.Helpers;
using InventorySystem.Services;

namespace InventorySystem.Forms
{
    public class OrderDetailsForm : BaseModalForm
    {
        private int _orderId;
        private OrderService _orderService;

        public OrderDetailsForm(int orderId)
        {
            _orderId = orderId;
            _orderService = new OrderService();
            this.Size = new Size(700, 650);

            InitializeComponent();

            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            this.TitleText = string.Format(LocalizationManager.GetString("OrderDetails_Title", "Order #{0} Details"), _orderId);
            
            var grids = this.Controls.Find("dgvItems", true);
            if (grids.Length > 0 && grids[0] is DataGridView dgv)
            {
                if (dgv.Columns.Contains("Item")) dgv.Columns["Item"].HeaderText = LocalizationManager.GetString("POS_GridProduct", "Item");
                if (dgv.Columns.Contains("Qty")) dgv.Columns["Qty"].HeaderText = LocalizationManager.GetString("POS_GridQty", "Qty");
                if (dgv.Columns.Contains("Price")) dgv.Columns["Price"].HeaderText = LocalizationManager.GetString("POS_GridPrice", "Price");
                if (dgv.Columns.Contains("Total")) dgv.Columns["Total"].HeaderText = LocalizationManager.GetString("POS_GridTotal", "Total");
            }

            SetFooterButtons(LocalizationManager.GetString("Btn_Close", "Close"), "", (s, e) => { this.Close(); }, null);
        }

        private void InitializeComponent()
        {
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(20)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header Info
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Shipping Info
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Items Grid

            // 1. Fetch Data
            DataRow header = _orderService.GetOrderHeader(_orderId);
            if (header == null)
            {
                Label lblErr = new Label { Text = LocalizationManager.GetString("OrderDetails_NotFound", "Order not found."), ForeColor = Color.Red, AutoSize = true };
                mainLayout.Controls.Add(lblErr, 0, 0);
                this.ContentPanel.Controls.Add(mainLayout);
                return;
            }

            // 2. Header Info Panel
            Panel pnlHeader = ThemeConfig.CreateCardPanel(CreateHeaderGrid(header));
            pnlHeader.Margin = new Padding(0, 0, 0, 15);
            pnlHeader.Height = 160; // Force height to prevent clipping
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // 3. Shipping Info Panel (if exists)
            string shipAddr = header["shipping_address"]?.ToString();
            if (!string.IsNullOrWhiteSpace(shipAddr))
            {
                Panel pnlShipping = ThemeConfig.CreateCardPanel(CreateShippingGrid(header));
                pnlShipping.Margin = new Padding(0, 0, 0, 15);
                pnlShipping.Height = 130; // Force height to prevent clipping
                mainLayout.Controls.Add(pnlShipping, 0, 1);
            }

            // 4. Items Grid
            DataGridView dgvItems = CreateItemsGrid();
            Panel pnlItems = ThemeConfig.CreateCardPanel(dgvItems);
            mainLayout.Controls.Add(pnlItems, 0, 2);

            this.ContentPanel.Controls.Add(mainLayout);

            // Footer Button
            SetFooterButtons(LocalizationManager.GetString("Btn_Close", "Close"), "", (s, e) => { this.Close(); }, null);
        }

        private TableLayoutPanel CreateHeaderGrid(DataRow header)
        {
            TableLayoutPanel tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, AutoSize = true, Padding = new Padding(10) };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            AddRow(tlp, 0, LocalizationManager.GetString("OrderDetails_Customer", "Customer:"), header["customer_name"]?.ToString() ?? LocalizationManager.GetString("POS_WalkInCustomer", "Walk-in"));
            AddRow(tlp, 1, LocalizationManager.GetString("OrderDetails_Date", "Date:"), Convert.ToDateTime(header["order_date"]).ToString("g"));
            AddRow(tlp, 2, LocalizationManager.GetString("OrderDetails_Status", "Status:"), $"{header["status"]} ({header["payment_status"]})");
            AddRow(tlp, 3, LocalizationManager.GetString("OrderDetails_TotalAmount", "Total Amount:"), CurrencyService.Format(Convert.ToDecimal(header["total_amount"])));

            return tlp;
        }

        private TableLayoutPanel CreateShippingGrid(DataRow header)
        {
            TableLayoutPanel tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, AutoSize = true, Padding = new Padding(10) };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            AddRow(tlp, 0, LocalizationManager.GetString("OrderDetails_ShippingTo", "Shipping To:"), header["shipping_address"]?.ToString());
            
            string delDate = header["delivery_date"] != DBNull.Value ? Convert.ToDateTime(header["delivery_date"]).ToString("g") : "N/A";
            AddRow(tlp, 1, LocalizationManager.GetString("OrderDetails_DeliveryDate", "Delivery Date:"), delDate);
            
            string dueDate = header["due_date"] != DBNull.Value ? Convert.ToDateTime(header["due_date"]).ToString("g") : "N/A";
            AddRow(tlp, 2, LocalizationManager.GetString("OrderDetails_PaymentDue", "Payment Due:"), dueDate);

            return tlp;
        }

        private void AddRow(TableLayoutPanel tlp, int row, string label, string value)
        {
            Label lbl = new Label { Text = label, Font = ThemeConfig.SmallBoldFont, ForeColor = ThemeConfig.TextColorDark, AutoSize = true, Margin = new Padding(0, 5, 0, 5) };
            Label val = new Label { Text = value, Font = ThemeConfig.StandardFont, ForeColor = ThemeConfig.TextColorDark, AutoSize = true, Margin = new Padding(0, 5, 0, 5) };
            tlp.Controls.Add(lbl, 0, row);
            tlp.Controls.Add(val, 1, row);
        }

        private DataGridView CreateItemsGrid()
        {
            DataGridView dgv = new DataGridView
            {
                Name = "dgvItems",
                Dock = DockStyle.Fill,
                BackgroundColor = ThemeConfig.SurfaceColor,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };
            ThemeConfig.ApplyGridTheme(dgv);
 
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Item", HeaderText = LocalizationManager.GetString("POS_GridProduct", "Item"), DataPropertyName = "PartName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = LocalizationManager.GetString("POS_GridQty", "Qty"), DataPropertyName = "Quantity", Width = 80 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = LocalizationManager.GetString("POS_GridPrice", "Price"), DataPropertyName = "UnitPrice", Width = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = LocalizationManager.GetString("POS_GridTotal", "Total"), DataPropertyName = "Total", Width = 120 });

            var items = _orderService.GetOrderItems(_orderId);
            dgv.DataSource = items;
            
            return dgv;
        }
    }
}
