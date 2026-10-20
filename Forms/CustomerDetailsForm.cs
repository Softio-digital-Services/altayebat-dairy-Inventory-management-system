using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using InventorySystem.Data;
using InventorySystem.Helpers;
using InventorySystem.Controls;

namespace InventorySystem.Forms
{
    public partial class CustomerDetailsForm : BaseModalForm
    {
        private int _customerId;
        private string _customerName;
        
        private Label lblName;
        private Label lblType;
        private Label lblBalance;
        private Label lblBalTitle;
        private DataGridView dgvHistory;
        private Button btnRecordSale;
        private Button btnReceivePayment;
        private Label lblDueDate;

        public CustomerDetailsForm(int id, string name)
        {
            _customerId = id;
            _customerName = name;
            this.Width = 1050; // Increased width to ensure all buttons fit in one row
            InitializeComponent();
            LoadDetails();
            ApplyLocalization();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
        }

        private void InitializeComponent()
        {
            this.TitleText = LocalizationManager.GetString("DetailsForm_TitlePrefix", "Details - ") + _customerName;

            // Main Layout container
            TableLayoutPanel tlpMain = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F)); // Fixed height for header section
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid takes the rest
            
            // --- HEADER ---
            TableLayoutPanel pnlHeader = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,

                Padding = new Padding(5)
            };
            // Left column for Name (AutoSize), Right for Balance/Buttons (Percent)
            pnlHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); 
            pnlHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            
            // Header Left: Name & Type
            FlowLayoutPanel flpLeft = new FlowLayoutPanel {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(10)
            };
            
            lblName = new Label() { Font = ThemeConfig.HeaderFont, ForeColor = ThemeConfig.PrimaryColor, AutoSize = true, Margin = new Padding(0,0,0,5) };
            lblType = new Label() { AutoSize = true, ForeColor = ThemeConfig.SecondaryColor, Font = ThemeConfig.StandardFont };
            flpLeft.Controls.Add(lblName);
            flpLeft.Controls.Add(lblType);
            
            // Header Right: Balance + Buttons
            FlowLayoutPanel flpRight = new FlowLayoutPanel {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 0),
                WrapContents = true // Allow wrapping if space is tight
            };
            
            // 1. Balance Panel
            TableLayoutPanel tlpBalance = new TableLayoutPanel
            {
                Size = new Size(220, 100),

                Margin = new Padding(10, 0, 10, 0),
                Padding = new Padding(15, 10, 15, 10),
                ColumnCount = 1,
                RowCount = 3
            };
            tlpBalance.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            tlpBalance.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tlpBalance.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));

            lblBalance = new Label() { Font = ThemeConfig.HeaderFont, ForeColor = ThemeConfig.WarningColor, AutoSize = true, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft };
            lblBalTitle = new Label() { Text = LocalizationManager.GetString("DetailsForm_BalanceDue"), Font = ThemeConfig.StandardFont, ForeColor = ThemeConfig.SecondaryColor, AutoSize = true, Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft };
            lblDueDate = new Label() { Font = ThemeConfig.SmallBoldFont, ForeColor = ThemeConfig.DangerColor, AutoSize = true, Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft, Visible = false };

            tlpBalance.Controls.Add(lblBalance, 0, 0);
            tlpBalance.Controls.Add(lblBalTitle, 0, 1);
            tlpBalance.Controls.Add(lblDueDate, 0, 2);
            
            // 2. Receive Payment Button
            btnReceivePayment = new ModernButton { Text = "\uD83D\uDCB5 " + LocalizationManager.GetString("DetailsForm_ReceivePayment", "Receive Payment"), Size = new Size(165, 45) };
            ThemeConfig.ApplyEmojiButton(btnReceivePayment, ThemeConfig.SuccessColor, ThemeConfig.SuccessColor, Color.White);
            btnReceivePayment.Click += BtnReceivePayment_Click;
            btnReceivePayment.Margin = new Padding(5, 5, 5, 5);

            // 3. Record Sale button
            btnRecordSale = new ModernButton { Text = "\uD83D\uDED2 " + LocalizationManager.GetString("DetailsForm_RecordSale", "Record Sale"), Size = new Size(155, 45) };
            ThemeConfig.ApplyEmojiButton(btnRecordSale, ThemeConfig.PrimaryColor, ThemeConfig.PrimaryColor, Color.White);
            btnRecordSale.Click += BtnRecordSale_Click;
            btnRecordSale.Margin = new Padding(5, 5, 5, 5);

            flpRight.Controls.Add(tlpBalance);
            flpRight.Controls.Add(btnReceivePayment);
            flpRight.Controls.Add(btnRecordSale);
            
            pnlHeader.Controls.Add(flpLeft, 0, 0);
            pnlHeader.Controls.Add(flpRight, 1, 0);

            // --- GRID ---
            dgvHistory = new DataGridView();
            dgvHistory.DataError += (s, e) => { e.ThrowException = false; };
            dgvHistory.Dock = DockStyle.Fill;
            dgvHistory.AllowUserToAddRows = false;
            dgvHistory.ReadOnly = true;
            dgvHistory.RowHeadersVisible = false;
            dgvHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHistory.CellFormatting += DgvHistory_CellFormatting;
            ThemeConfig.ApplyGridTheme(dgvHistory);

            tlpMain.Controls.Add(pnlHeader, 0, 0);
            tlpMain.Controls.Add(dgvHistory, 0, 1);

            this.ContentPanel.Controls.Add(tlpMain);

            SetFooterButtons(
                LocalizationManager.GetString("Btn_Close", "Close"),
                "",
                (s, e) => this.Close(),
                null
            );
        }

        private void LoadDetails()
        {
            try 
            {
                // Load Info
                string sqlInfo = $"SELECT * FROM customers WHERE customer_id = {_customerId}";
                DataTable dtInfo = DatabaseHelper.ExecuteDataTable(sqlInfo);
                if(dtInfo.Rows.Count > 0)
                {
                    DataRow row = dtInfo.Rows[0];
                    lblName.Text = row["full_name"].ToString();
                    string typeVal = row.Table.Columns.Contains("type") && row["type"] != DBNull.Value ? row["type"].ToString() : "Individual";
                    string phone = row.Table.Columns.Contains("phone") && row["phone"] != DBNull.Value ? row["phone"].ToString() : "-";
                    lblType.Text = typeVal + " | " + phone;
                    
                    decimal bal = row.Table.Columns.Contains("current_balance") && row["current_balance"] != DBNull.Value 
                        ? Convert.ToDecimal(row["current_balance"]) : 0m;
                    lblBalance.Text = $"${bal:N2}";

                    // Fetch the earliest upcoming due date from transactions
                    string sqlUpcoming = $@"SELECT due_date FROM payments 
                                          WHERE entity_type = 'Customer' AND entity_id = {_customerId} 
                                          AND due_date IS NOT NULL AND due_date != ''
                                          AND date(due_date) >= date('now')
                                          ORDER BY date(due_date) ASC LIMIT 1";
                    object nextDue = DatabaseHelper.ExecuteScalar<object>(sqlUpcoming);

                    if (nextDue != null && nextDue != DBNull.Value)
                    {
                        if (DateTime.TryParse(nextDue.ToString(), out DateTime due))
                        {
                            lblDueDate.Text = string.Format(LocalizationManager.GetString("DetailsForm_NextDue", "Next Due: {0}"), due.ToString("yyyy-MM-dd"));
                            lblDueDate.Visible = true;
                        }
                    }
                    else
                    {
                        lblDueDate.Visible = false;
                    }
                }

                // Load History - combines sales (balance additions) and payments received
                string sqlHistory = $@"
                    SELECT payment_date as 'Date', 
                           CASE WHEN notes LIKE '[Sale]%' OR notes LIKE '%Sale%' OR notes LIKE '%Order%' THEN 'Tran_PaymentDue'
                                 ELSE 'Tran_PaymentReceived' END as 'Action',
                           amount as 'Amount',
                           due_date as 'Due Date',
                           CASE WHEN notes IS NULL OR notes = '' OR notes = 'None' THEN 'None'
                                 ELSE REPLACE(REPLACE(notes, '[Sale] ', ''), '[Payment] ', '') END as 'Details'
                    FROM payments 
                    WHERE entity_type = 'Customer' AND entity_id = {_customerId}
                    ORDER BY payment_date DESC";
                
                dgvHistory.DataSource = DatabaseHelper.ExecuteDataTable(sqlHistory);
                ApplyGridLocalizations();
            }
            catch(Exception ex) { MessageHelper.ShowError(ex.Message); }
        }

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            this.TitleText = LocalizationManager.GetString("DetailsForm_TitlePrefix", "Details - ") + _customerName;
            lblBalTitle.Text = LocalizationManager.GetString("DetailsForm_BalanceDue", "Balance Due");
            btnReceivePayment.Text = "\uD83D\uDCB5 " + LocalizationManager.GetString("DetailsForm_ReceivePayment", "Receive Payment");
            btnRecordSale.Text = "\uD83D\uDED2 " + LocalizationManager.GetString("DetailsForm_RecordSale", "Record Sale");

            SetFooterButtons(
                LocalizationManager.GetString("Btn_Close", "Close"),
                "",
                (s, e) => this.Close(),
                null
            );

            ApplyGridLocalizations();
        }

        private void ApplyGridLocalizations()
        {
            if (dgvHistory == null || dgvHistory.Columns.Count == 0) return;

            if (dgvHistory.Columns["Date"] != null) dgvHistory.Columns["Date"].HeaderText = LocalizationManager.GetString("Hist_ColDate");
            if (dgvHistory.Columns["Action"] != null) dgvHistory.Columns["Action"].HeaderText = LocalizationManager.GetString("Hist_ColAction");
            if (dgvHistory.Columns["Amount"] != null) dgvHistory.Columns["Amount"].HeaderText = LocalizationManager.GetString("DetailsForm_ColPayment", "Payment");
            if (dgvHistory.Columns["Due Date"] != null) dgvHistory.Columns["Due Date"].HeaderText = LocalizationManager.GetString("Tran_DueDateLabel", "Due Date");
            if (dgvHistory.Columns["Details"] != null) dgvHistory.Columns["Details"].HeaderText = LocalizationManager.GetString("Hist_ColDetails");
        }
        
        private void DgvHistory_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null || e.Value == DBNull.Value || string.IsNullOrEmpty(e.Value.ToString())) return;

            string colName = dgvHistory.Columns[e.ColumnIndex].Name;

            if (colName == "Action")
            {
                string actionKey = e.Value.ToString();
                e.Value = LocalizationManager.GetString(actionKey);
            }
            else if (colName == "Details")
            {
                string val = e.Value.ToString();
                if (val == "None") e.Value = LocalizationManager.GetString("Tran_None", val);
            }
        }

        private void BtnRecordSale_Click(object sender, EventArgs e)
        {
            decimal currentBalance = ParseBalance();

            Func<string, string> L = InventorySystem.Helpers.LocalizationManager.GetString;
            TransactionEntryForm form = new TransactionEntryForm(
                L("Cust_RecordSale"),
                string.Format(L("Prompt_RecordSale"), _customerName),
                "0.00",
                true);
            
            if(form.ShowDialog() == DialogResult.OK)
            {
                decimal amount = form.Amount;
                string userNotes = string.IsNullOrWhiteSpace(form.Notes) ? "None" : form.Notes;
                string dbNotes = "[Sale] " + userNotes;
                string amountStr = amount.ToString(CultureInfo.InvariantCulture);

                // 1. Update Balance & Due Date
                string sql1 = "UPDATE customers SET current_balance = current_balance + @amount";
                var parameters = new System.Collections.Generic.List<Microsoft.Data.Sqlite.SqliteParameter> {
                    new Microsoft.Data.Sqlite.SqliteParameter("@amount", amount),
                    new Microsoft.Data.Sqlite.SqliteParameter("@cid", _customerId)
                };

                if (form.DueDate.HasValue)
                {
                    sql1 += ", payment_due_date = @dueDate";
                    parameters.Add(new Microsoft.Data.Sqlite.SqliteParameter("@dueDate", form.DueDate.Value));
                }

                sql1 += " WHERE customer_id = @cid";
                DatabaseHelper.ExecuteNonQuery(sql1, parameters.ToArray());

                // 2. Record transaction log
                string sql2 = "INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes, due_date) VALUES ('Customer', @cid, @amount, datetime('now'), @notes, @ddate)";
                DatabaseHelper.ExecuteNonQuery(sql2, 
                    new Microsoft.Data.Sqlite.SqliteParameter("@cid", _customerId),
                    new Microsoft.Data.Sqlite.SqliteParameter("@amount", amount),
                    new Microsoft.Data.Sqlite.SqliteParameter("@notes", dbNotes),
                    new Microsoft.Data.Sqlite.SqliteParameter("@ddate", (object)form.DueDate ?? DBNull.Value));

                GlobalEvents.RaiseCustomersUpdated(); // Sync main grid
                LoadDetails();
            }
        }

        private void BtnReceivePayment_Click(object sender, EventArgs e)
        {
            decimal currentBalance = ParseBalance();

            Func<string, string> L = InventorySystem.Helpers.LocalizationManager.GetString;
            TransactionEntryForm form = new TransactionEntryForm(
                L("Cust_ReceivePayment"),
                string.Format(L("Prompt_ReceivePayment"), _customerName));
            
            if(form.ShowDialog() == DialogResult.OK)
            {
                decimal amount = form.Amount;
                string userNotes = string.IsNullOrWhiteSpace(form.Notes) ? "None" : form.Notes;
                string dbNotes = "[Payment] " + userNotes;

                if(amount > currentBalance)
                {
                    if(!MessageHelper.ConfirmAction($"Payment (${amount:N2}) exceeds balance (${currentBalance:N2}). Continue?"))
                        return;
                }

                string amountStr = amount.ToString(CultureInfo.InvariantCulture);

                // 1. Update Balance - customers table uses current_balance
                string sql1 = $"UPDATE customers SET current_balance = current_balance - {amountStr} WHERE customer_id = {_customerId}";
                DatabaseHelper.ExecuteNonQuery(sql1);
 
                // 2. Record Payment
                string sql2 = $"INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes) VALUES ('Customer', {_customerId}, {amountStr}, datetime('now'), @notes)";
                DatabaseHelper.ExecuteNonQuery(sql2, new Microsoft.Data.Sqlite.SqliteParameter("@notes", dbNotes));
 
                GlobalEvents.RaiseCustomersUpdated(); // Sync main grid
                LoadDetails();
            }
        }

        private decimal ParseBalance()
        {
            string balStr = lblBalance.Text.Replace("$", "").Replace(",", "").Trim();
            decimal.TryParse(balStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal bal);
            return bal;
        }
    }
}
