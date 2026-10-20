using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Helpers;
using InventorySystem.Services;
using InventorySystem.Controls;

namespace InventorySystem.Forms
{
    public partial class BlindReturnForm : BaseModalForm
    {
        private DataGridView dgvItems;
        private TextBox txtReason;
        private ComboBox cmbCustomer;
        private Label lblTotalRefund;
        private DataTable _itemsTable;
        private InventoryService _inventoryService;
        private ReturnService _returnService;
        
        // Barcode Scanner Buffer
        private DateTime _lastScanTime = DateTime.Now;
        private string _scanBuffer = "";

        public BlindReturnForm()
        {
            _inventoryService = new InventoryService();
            _returnService = new ReturnService();
            
            this.TitleText = LocalizationManager.GetString("Title_ItemReturn");
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();

            InitializeForm();
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            this.TitleText = LocalizationManager.GetString("Title_ItemReturn");
            if (dgvItems != null && dgvItems.Columns.Count > 0)
            {
                if (dgvItems.Columns.Contains("PartName")) dgvItems.Columns["PartName"].HeaderText = LocalizationManager.GetString("Parts_GridProduct");
                if (dgvItems.Columns.Contains("UnitPrice")) dgvItems.Columns["UnitPrice"].HeaderText = LocalizationManager.GetString("POS_GridPrice");
                if (dgvItems.Columns.Contains("QtyToReturn")) dgvItems.Columns["QtyToReturn"].HeaderText = LocalizationManager.GetString("Return_Qty");
            }
        }

        private void InitializeForm()
        {
            this.SuspendLayout();

            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(20)
            };
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Search area
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid space
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 260F)); // Bottom area

            // Label indicating scanner is active
            Panel pnlSearch = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            Label lblScannerReady = new Label { Text = LocalizationManager.GetString("Return_ReadyToScan"), Font = ThemeConfig.SubHeaderFont, ForeColor = ThemeConfig.SecondaryColor, AutoSize = true, Location = new Point(0, 15) };
            pnlSearch.Controls.Add(lblScannerReady);
            tlpMain.Controls.Add(pnlSearch, 0, 0);

            // Global Barcode Scan Support
            this.KeyPreview = true;
            this.KeyPress += BlindReturnForm_KeyPress;

            // Item Grid
            dgvItems = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AutoGenerateColumns = false, BackgroundColor = ThemeConfig.SurfaceColor, BorderStyle = BorderStyle.None };
            ThemeConfig.ApplyGridTheme(dgvItems);
            
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "PartID", DataPropertyName = "part_id", Visible = false });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "PartName", DataPropertyName = "part_name", HeaderText = LocalizationManager.GetString("Parts_GridProduct"), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", DataPropertyName = "price", HeaderText = LocalizationManager.GetString("POS_GridPrice"), Width = 100, ReadOnly = true });
            
            DataGridViewTextBoxColumn colReturn = new DataGridViewTextBoxColumn { Name = "QtyToReturn", DataPropertyName = "quantity", HeaderText = LocalizationManager.GetString("Return_Qty"), Width = 120 };
            colReturn.DefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
            dgvItems.Columns.Add(colReturn);
            
            DataGridViewButtonColumn colRemove = new DataGridViewButtonColumn { Name = "Remove", HeaderText = "", Text = "X", UseColumnTextForButtonValue = true, Width = 50, FlatStyle = FlatStyle.Flat };
            colRemove.DefaultCellStyle.ForeColor = ThemeConfig.DangerColor;
            dgvItems.Columns.Add(colRemove);

            dgvItems.CellValueChanged += DgvItems_CellValueChanged;
            dgvItems.CellContentClick += DgvItems_CellContentClick;
            
            Panel pnlGridCard = ThemeConfig.CreateCardPanel(dgvItems);
            tlpMain.Controls.Add(pnlGridCard, 0, 1);

            _itemsTable = new DataTable();
            _itemsTable.Columns.Add("part_id", typeof(int));
            _itemsTable.Columns.Add("part_name", typeof(string));
            _itemsTable.Columns.Add("price", typeof(decimal));
            _itemsTable.Columns.Add("quantity", typeof(int));
            dgvItems.DataSource = _itemsTable;

            // Bottom Area
            TableLayoutPanel tlpBottom = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(0, 10, 0, 0)
            };
            tlpBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tlpBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tlpBottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpBottom.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));

            // Reason and Customer Section (Left)
            Panel pnlLeft = new Panel { Dock = DockStyle.Fill };
            
            // Customer Section
            Panel pnlCust = new Panel { Dock = DockStyle.Top, Height = 70, Margin = new Padding(0, 0, 0, 10) };
            Label lblCustomer = new Label { Text = (LocalizationManager.GetString("Cust_Title")) + ":", AutoSize = true, Font = ThemeConfig.SubHeaderFont, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 5) };
            cmbCustomer = new ComboBox { Font = ThemeConfig.StandardFont, DropDownStyle = ComboBoxStyle.DropDownList };
            ThemeConfig.ApplyComboBoxStyle(cmbCustomer);
            Panel pnlCmbWrapper = ThemeConfig.WrapInStyledInput(cmbCustomer, 35); 
            pnlCmbWrapper.Dock = DockStyle.Fill;
            pnlCust.Controls.Add(pnlCmbWrapper); pnlCmbWrapper.BringToFront();
            pnlCust.Controls.Add(lblCustomer);
            
            // Reason Section
            Panel pnlReason = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 10, 0) };
            Label lblReason = new Label { Text = (LocalizationManager.GetString("Msg_ReturnReason")) + ":", AutoSize = true, Font = ThemeConfig.SubHeaderFont, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 5) };
            txtReason = new TextBox { Multiline = true, Dock = DockStyle.Fill, Font = ThemeConfig.StandardFont, BorderStyle = BorderStyle.None };
            Panel pnlTxtWrapper = ThemeConfig.WrapInStyledInput(txtReason, 80, true); 
            pnlTxtWrapper.Dock = DockStyle.Fill;
            pnlReason.Controls.Add(pnlTxtWrapper); pnlTxtWrapper.BringToFront();
            pnlReason.Controls.Add(lblReason);

            pnlLeft.Controls.Add(pnlReason); pnlReason.BringToFront();
            pnlLeft.Controls.Add(pnlCust);
            
            tlpBottom.Controls.Add(pnlLeft, 0, 0);

            LoadCustomers();

            // Summary Section (Right)
            lblTotalRefund = new Label { Text = LocalizationManager.GetString("Msg_TotalRefund") + CurrencyService.Format(0), Dock = DockStyle.Fill, Font = ThemeConfig.HeaderFont, ForeColor = ThemeConfig.PrimaryColor, TextAlign = ContentAlignment.BottomRight };
            tlpBottom.Controls.Add(lblTotalRefund, 1, 1);
            tlpBottom.SetRowSpan(pnlLeft, 2);

            tlpMain.Controls.Add(tlpBottom, 0, 2);
            this.ContentPanel.Controls.Add(tlpMain);

            SetFooterButtons(
                LocalizationManager.GetString("Msg_ProcessReturn"),
                LocalizationManager.GetString("Popup_Cancel"),
                BtnSubmit_Click,
                (s, e) => this.Close()
            );

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadCustomers()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteDataTable("SELECT id, name FROM customers WHERE date_deleted IS NULL");
                dt.Rows.InsertAt(dt.NewRow(), 0);
                dt.Rows[0]["id"] = -1;
                dt.Rows[0]["name"] = LocalizationManager.GetString("Msg_CashReturn");
                
                cmbCustomer.DisplayMember = "name";
                cmbCustomer.ValueMember = "id";
                cmbCustomer.DataSource = dt;
            }
            catch { }
        }

        private void BlindReturnForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - _lastScanTime;
            if (elapsed.TotalMilliseconds > 100) 
            {
                _scanBuffer = "";
            }
            _lastScanTime = DateTime.Now;

            if (e.KeyChar != (char)Keys.Enter)
            {
                _scanBuffer += e.KeyChar;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter && this.Visible)
            {
                TimeSpan elapsed = DateTime.Now - _lastScanTime;
                if (elapsed.TotalMilliseconds <= 100 && !string.IsNullOrEmpty(_scanBuffer))
                {
                    string barcode = _scanBuffer.Trim();
                    _scanBuffer = "";

                    DataRow partInfo = _inventoryService.GetPartByBarcodeOrNumber(barcode);
                    
                    if (partInfo != null)
                    {
                        int partId = Convert.ToInt32(partInfo["id"]);
                        string partName = partInfo["part_name"].ToString();
                        decimal price = Convert.ToDecimal(partInfo["selling_price"]);

                        bool exists = false;
                        foreach (DataRow row in _itemsTable.Rows)
                        {
                            if (Convert.ToInt32(row["part_id"]) == partId)
                            {
                                row["quantity"] = Convert.ToInt32(row["quantity"]) + 1;
                                exists = true;
                                break;
                            }
                        }

                        if (!exists)
                        {
                            _itemsTable.Rows.Add(partId, partName, price, 1);
                        }

                        CalculateTotal();
                    }
                    else
                    {
                        string warnMsg = string.Format(
                            LocalizationManager.GetString("BlindReturn_ItemNotFound", "Item not found in inventory (Barcode: {0}). Cannot return this item."),
                            barcode
                        );
                        MessageHelper.ShowWarning(warnMsg);
                    }

                    return true; // Suppress Enter key!
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void DgvItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvItems.Columns[e.ColumnIndex].Name == "QtyToReturn")
            {
                CalculateTotal();
            }
        }

        private void DgvItems_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvItems.Columns[e.ColumnIndex].Name == "Remove")
            {
                _itemsTable.Rows.RemoveAt(e.RowIndex);
                CalculateTotal();
            }
        }

        private void CalculateTotal()
        {
            decimal total = 0;
            foreach (DataRow row in _itemsTable.Rows)
            {
                if (row["quantity"] != DBNull.Value && int.TryParse(row["quantity"].ToString(), out int qty))
                {
                    decimal price = Convert.ToDecimal(row["price"]);
                    total += qty * price;
                }
            }
            lblTotalRefund.Text = LocalizationManager.GetString("Msg_TotalRefund") + CurrencyService.Format(total);
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            List<ReturnItemInfo> returnItems = new List<ReturnItemInfo>();
            foreach (DataRow row in _itemsTable.Rows)
            {
                if (row["quantity"] != DBNull.Value && int.TryParse(row["quantity"].ToString(), out int qty) && qty > 0)
                {
                    returnItems.Add(new ReturnItemInfo
                    {
                        PartId = Convert.ToInt32(row["part_id"]),
                        Quantity = qty,
                        RefundAmount = qty * Convert.ToDecimal(row["price"])
                    });
                }
            }

            if (returnItems.Count == 0)
            {
                string msg = LocalizationManager.GetString("Msg_ReturnOneItem");
                MessageHelper.ShowWarning(msg);
                return;
            }

            // Reason is now optional


            int customerId = -1;
            if (cmbCustomer.SelectedValue != null && int.TryParse(cmbCustomer.SelectedValue.ToString(), out int cid) && cid > 0)
            {
                customerId = cid;
            }

            string confirmMsg = LocalizationManager.GetString("Msg_ConfirmReturn");
            if (MessageHelper.ConfirmAction(confirmMsg))
            {
                try
                {
                    _returnService.ProcessBlindReturn(returnItems, txtReason.Text, customerId);
                    MessageHelper.ShowSuccess(LocalizationManager.GetString("Msg_ReturnSuccess"));
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageHelper.ShowError("Failed to process return: " + ex.Message);
                }
            }
        }
    }
}
