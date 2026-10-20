using System;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using InventorySystem.Controls;
using InventorySystem.Data;
using InventorySystem.Helpers;
using InventorySystem.Services;

namespace InventorySystem.Forms
{
    public class MonthlyExpensesForm : UserControl
    {
        private DataGridView dgvExpenses;
        private Label lblExpensesTitle;
        private ModernTextBox txtSearch;
        private ModernNumericUpDown numAmount;
        private ModernTextBox txtDescription;
        private ModernComboBox cmbCategory;
        private FlatDateTimePicker dtpDate;
        private Label lblDateRef;
        private Button btnAdd;
        private Button btnDelete;
        private Label lblTotal;
        private CheckBox chkRecurring;
        private ExpenseService _expenseService = new ExpenseService();

        public MonthlyExpensesForm()
        {
            InitializeComponent();
            ApplyTheme();
            LoadData();
            ApplyLocalization();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1100, 750);


            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180F)); // Entry (Increased for labels)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid
            mainLayout.Padding = new Padding(20);
            this.Controls.Add(mainLayout);

            // Title
            lblExpensesTitle = ThemeConfig.CreateStandardHeader(LocalizationManager.GetString("Exp_MonthlyExpenses"));
            lblExpensesTitle.Name = "lblExpensesTitle";

            txtSearch = new ModernTextBox {
                IsSearch = true,
                ShowLabel = false,
                PlaceholderText = LocalizationManager.GetString("Parts_Search", "Search expenses..."),
                Size = new Size(320, 35)
            };
            txtSearch.TextChanged += (s, e) => LoadData(txtSearch.Text);

            lblTotal = new Label { 
                Font = new Font(ThemeConfig.AppFontFamily, 11F, FontStyle.Bold), 
                ForeColor = ThemeConfig.PrimaryColor, 
                AutoSize = true, 
                TextAlign = ContentAlignment.MiddleRight,
                RightToLeft = RightToLeft.No
            };

            var headerControls = new Control[] { lblTotal };
            TableLayoutPanel tlpHeader = ThemeConfig.CreateGlobalFormHeader(lblExpensesTitle, txtSearch, headerControls);
            mainLayout.Controls.Add(tlpHeader, 0, 0);

            // --- ENTRY SECTION ---
            TableLayoutPanel grid = new TableLayoutPanel { 
                Dock = DockStyle.Fill, 
                ColumnCount = 6, 
                RowCount = 2, 
                Padding = new Padding(10),
                BackColor = ThemeConfig.SurfaceColor 
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F)); // Category
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F)); // Date
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F)); // Amount
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // Description
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10F));  // Spacer
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320F)); // Actions
            
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 85F)); // Increased to 85 to prevent clipping
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F)); // Sub Row (Recurring)

            // 1. Category with Add Button
            TableLayoutPanel pnlCatContainer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(15, 5, 5, 10), Width = 220 };
            pnlCatContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlCatContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 45F));

            cmbCategory = new ModernComboBox { 
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 5, 0),
                LabelText = LocalizationManager.GetString("Exp_CategoryLabel")
            };
            cmbCategory.Items.AddRange(new object[] { "Rent", "Utilities", "Wages", "Supplies", "Maintenance", "Other" });
            
            Button btnAddCategory = new Button 
            { 
                Width = 35, 
                Margin = new Padding(0, 25, 0, 0)
            };
            ThemeConfig.ApplyStandardAddButton(btnAddCategory, "");
            btnAddCategory.Click += BtnAddCategory_Click;

            pnlCatContainer.Controls.Add(cmbCategory, 0, 0);
            pnlCatContainer.Controls.Add(btnAddCategory, 1, 0);
            
            Panel pnlDate = new Panel { Dock = DockStyle.Fill, Margin = new Padding(10, 5, 5, 10) };
            lblDateRef = new Label {
                Text = LocalizationManager.GetString("Hist_ColDate", "Date"),
                // Match the label font used inside ModernNumericUpDown / ModernComboBox
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                Location = new Point(0, 0),
                AutoSize = true
            };
            dtpDate = new FlatDateTimePicker { Width = 170, Location = new Point(0, 25) };
            pnlDate.Controls.Add(dtpDate);
            pnlDate.Controls.Add(lblDateRef);

            
            numAmount = new ModernNumericUpDown { 
                LabelText = LocalizationManager.GetString("Exp_AmountLabel"),
                DecimalPlaces = 2, 
                Maximum = 1000000, 
                Width = 120 
            };

            
            txtDescription = new ModernTextBox { 
                Dock = DockStyle.Fill, 
                LabelText = LocalizationManager.GetString("Exp_DescriptionLabel"),
                PlaceholderText = LocalizationManager.GetString("Exp_Details"),
                Margin = new Padding(5, 5, 5, 10),
                Multiline = true
            };

            // Actions Container
            FlowLayoutPanel pnlActions = new FlowLayoutPanel { 
                Dock = DockStyle.Fill, 
                FlowDirection = FlowDirection.LeftToRight, 
                Padding = new Padding(0, 30, 0, 0),
                WrapContents = false
            };

            // Increased width to 140
            btnAdd = new Button { Size = new Size(140, 35), Margin = new Padding(5, 0, 5, 0) };
            btnAdd.Click += BtnAdd_Click;
            ThemeConfig.ApplyStandardAddButton(btnAdd, "Exp_Add");

            btnDelete = new Button { Size = new Size(140, 35), Margin = new Padding(5, 0, 5, 0) };
            btnDelete.Click += BtnDelete_Click;
            ThemeConfig.ApplyStandardDeleteButton(btnDelete, "Exp_Delete");

            chkRecurring = new CheckBox { 
                Text = LocalizationManager.GetString("Exp_Recurring"), 
                Font = ThemeConfig.StandardFont, 
                AutoSize = true, 
                Margin = new Padding(5, 5, 0, 0),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.System
            };
            
            pnlActions.Controls.Add(btnAdd);
            pnlActions.Controls.Add(btnDelete);
            
            // Wrap numAmount in a centering panel so the TLP cell height doesn't
            // stretch the control and break the spinner button positions.
            Panel pnlAmountWrapper = new Panel { Dock = DockStyle.Fill, Margin = new Padding(5, 5, 5, 10) };
            numAmount = new ModernNumericUpDown
            {
                LabelText = LocalizationManager.GetString("Exp_AmountLabel"),
                DecimalPlaces = 2,
                Maximum = 1000000,
                Width = 140
            };
            // Position the numAmount at the top within the wrapper to align with other labels
            pnlAmountWrapper.Controls.Add(numAmount);
            pnlAmountWrapper.Resize += (s, e) =>
            {
                numAmount.Width = pnlAmountWrapper.Width;
                numAmount.Location = new Point(0, 0);
            };

            grid.Controls.Add(pnlCatContainer, 0, 0);
            grid.Controls.Add(pnlDate, 1, 0);
            grid.Controls.Add(pnlAmountWrapper, 2, 0);
            grid.Controls.Add(txtDescription, 3, 0);
            grid.Controls.Add(pnlActions, 5, 0);
            grid.Controls.Add(chkRecurring, 0, 1);

            // Card for Entry Panel
            Panel pnlEntryCard = ThemeConfig.CreateCardPanel(grid);
            pnlEntryCard.Margin = new Padding(0, 0, 0, 15);
            mainLayout.Controls.Add(pnlEntryCard, 0, 1);


            // Grid
            dgvExpenses = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = ThemeConfig.SurfaceColor, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, AutoGenerateColumns = false };
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = LocalizationManager.GetString("Users_GridID"), DataPropertyName = "expense_id", Width = 60 });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = LocalizationManager.GetString("Exp_Date"), DataPropertyName = "expense_date", Width = 130 });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", HeaderText = LocalizationManager.GetString("Exp_CategoryLabel"), DataPropertyName = "category", Width = 120 });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = LocalizationManager.GetString("Exp_AmountLabel"), DataPropertyName = "amount", Width = 100 });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = LocalizationManager.GetString("Exp_DescriptionLabel"), DataPropertyName = "description", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            
            // Hidden data columns
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "is_paid", DataPropertyName = "is_paid", Visible = false });
            
            // New Status Columns
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = LocalizationManager.GetString("Exp_Status"), Width = 100 });
            dgvExpenses.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Recurring", HeaderText = LocalizationManager.GetString("Exp_Auto"), DataPropertyName = "is_recurring", Width = 60 });
            
            mainLayout.Controls.Add(dgvExpenses, 0, 2);
            ThemeConfig.ApplyGridTheme(dgvExpenses);

            ApplyLocalization();

            DataGridViewButtonColumn btnPaid = new DataGridViewButtonColumn { 
                Name = "Action", 
                HeaderText = LocalizationManager.GetString("Exp_Action"), 
                Text = LocalizationManager.GetString("Exp_PayNow"), 
                UseColumnTextForButtonValue = true, 
                Width = 120,
                FlatStyle = FlatStyle.Flat
            };
            dgvExpenses.Columns.Add(btnPaid);
            dgvExpenses.CellPainting += (s, e) => {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvExpenses.Columns[e.ColumnIndex].Name == "Action") {
                    e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                    var drv = dgvExpenses.Rows[e.RowIndex].DataBoundItem as DataRowView;
                    bool isPaid = drv != null && drv["is_paid"] != DBNull.Value ? Convert.ToBoolean(drv["is_paid"]) : true;
                    
                    if (!isPaid) {
                        Rectangle r = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + 8, e.CellBounds.Width - 16, e.CellBounds.Height - 16);
                        using (var path = ThemeConfig.GetRoundedPathPublic(r, 8))
                        using (var brush = new SolidBrush(ThemeConfig.PrimaryColor)) {
                            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            e.Graphics.FillPath(brush, path);
                            TextRenderer.DrawText(e.Graphics, LocalizationManager.GetString("Exp_PayNow"), ThemeConfig.SmallBoldFont, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }
                    e.Handled = true;
                }
            };
            dgvExpenses.CellContentClick += DgvExpenses_CellContentClick;

            // Card for Grid
            Panel pnlGridCard = ThemeConfig.CreateCardPanel(dgvExpenses);
            mainLayout.Controls.Add(pnlGridCard, 0, 2);
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            lblExpensesTitle.Text = LocalizationManager.GetString("Exp_Title");
            
            ThemeConfig.ApplyStandardAddButton(btnAdd, "Exp_Add");
            ThemeConfig.ApplyStandardDeleteButton(btnDelete, "Exp_Delete");
            
            if (txtDescription != null)
            {
                txtDescription.LabelText = LocalizationManager.GetString("Exp_DescriptionLabel");
                txtDescription.PlaceholderText = LocalizationManager.GetString("Exp_Description");
            }
            if (cmbCategory != null)
            {
                cmbCategory.LabelText = LocalizationManager.GetString("Exp_CategoryLabel");
                cmbCategory.PlaceholderText = LocalizationManager.GetString("Exp_Category");
            }
            if (numAmount != null)
            {
                numAmount.LabelText = LocalizationManager.GetString("Exp_AmountLabel");
            }
            if (lblDateRef != null)
            {
                lblDateRef.Text = LocalizationManager.GetString("Hist_ColDate", "Date");
            }
            
            if (dgvExpenses.Columns.Contains("Id")) dgvExpenses.Columns["Id"].HeaderText = LocalizationManager.GetString("Users_GridID");
            if (dgvExpenses.Columns.Contains("Category")) dgvExpenses.Columns["Category"].HeaderText = LocalizationManager.GetString("Exp_CategoryLabel");
            if (dgvExpenses.Columns.Contains("Date")) dgvExpenses.Columns["Date"].HeaderText = LocalizationManager.GetString("Exp_Date");
            if (dgvExpenses.Columns.Contains("Amount")) dgvExpenses.Columns["Amount"].HeaderText = LocalizationManager.GetString("Exp_AmountLabel");
            if (dgvExpenses.Columns.Contains("Description")) dgvExpenses.Columns["Description"].HeaderText = LocalizationManager.GetString("Exp_DescriptionLabel");
            if (dgvExpenses.Columns.Contains("Status")) dgvExpenses.Columns["Status"].HeaderText = LocalizationManager.GetString("Exp_Status");
            if (dgvExpenses.Columns.Contains("Recurring")) dgvExpenses.Columns["Recurring"].HeaderText = LocalizationManager.GetString("Exp_Auto");
            if (dgvExpenses.Columns.Contains("Action")) dgvExpenses.Columns["Action"].HeaderText = LocalizationManager.GetString("Exp_Action");
            
            if (chkRecurring != null)
            {
                chkRecurring.Text = LocalizationManager.GetString("Exp_Recurring");
                chkRecurring.Anchor = LocalizationManager.IsArabic ? AnchorStyles.Right : AnchorStyles.Left;
            }
            
            LoadCategories(); // Refresh categories in dropdown
        }

        private void ApplyTheme() 
        { 
            ThemeConfig.ApplyGridTheme(dgvExpenses); 
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbCategory.SelectedIndex == -1)
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("Exp_ReqCategory"));
                return;
            }

            decimal amount = numAmount.Value;
            if (amount <= 0) {
                MessageHelper.ShowWarning(LocalizationManager.GetString("Exp_GreaterZero"));
                return;
            }

            DatabaseHelper.ExecuteNonQuery("INSERT INTO expenses (category, expense_date, amount, description, recorded_by, is_recurring, is_paid) VALUES (@cat, @date, @amt, @desc, @usr, @rec, 1)",
                new SqliteParameter("@cat", cmbCategory.SelectedItem.ToString()),
                new SqliteParameter("@date", dtpDate.Value),
                new SqliteParameter("@amt", amount),
                new SqliteParameter("@desc", txtDescription.Text),
                new SqliteParameter("@usr", UserSession.Username),
                new SqliteParameter("@rec", chkRecurring.Checked));
            
            ClearForm();
            LoadData();
        }

        private void DgvExpenses_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvExpenses.Columns[e.ColumnIndex].Name == "Action")
            {
                var drv = dgvExpenses.Rows[e.RowIndex].DataBoundItem as DataRowView;
                if (drv == null) return;

                int id = Convert.ToInt32(drv["expense_id"]);
                bool isPaid = drv["is_paid"] != DBNull.Value ? Convert.ToBoolean(drv["is_paid"]) : true;
                
                if (!isPaid) {
                    _expenseService.MarkAsPaid(id);
                    LoadData();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvExpenses.SelectedRows.Count == 0) return;
            if (MessageHelper.ShowConfirmation(LocalizationManager.GetString("Exp_ConfirmDelete")))
            {
                int id = Convert.ToInt32(dgvExpenses.SelectedRows[0].Cells["Id"].Value);
                DatabaseHelper.ExecuteNonQuery("DELETE FROM expenses WHERE expense_id = @id", new SqliteParameter("@id", id));
                LoadData();
            }
        }

        private void ClearForm()
        {
            cmbCategory.SelectedIndex = -1;
            numAmount.Value = 0;
            txtDescription.Clear();
            dtpDate.Value = DateTime.Now;
        }

        public void LoadData(string search = "")
        {
            string query = "SELECT * FROM expenses WHERE category != 'System'";
            DataTable dt;
            if (!string.IsNullOrEmpty(search))
            {
                query += " AND (category LIKE @s OR description LIKE @s)";
                dt = DatabaseHelper.ExecuteDataTable(query + " ORDER BY expense_date DESC", new SqliteParameter("@s", "%" + search + "%"));
            }
            else
            {
                dt = DatabaseHelper.ExecuteDataTable(query + " ORDER BY expense_date DESC");
            }

            dgvExpenses.DataSource = dt;
            
            decimal total = 0;
            foreach (DataGridViewRow row in dgvExpenses.Rows) {
                var drv = row.DataBoundItem as DataRowView;
                if (drv == null) continue;

                bool isPaid = drv["is_paid"] != DBNull.Value ? Convert.ToBoolean(drv["is_paid"]) : true;
                row.Cells["Status"].Value = isPaid ? LocalizationManager.GetString("Exp_Paid") : LocalizationManager.GetString("Exp_Unpaid");
                row.DefaultCellStyle.ForeColor = isPaid ? Color.Black : Color.Red;
                
                total += drv["amount"] != DBNull.Value ? Convert.ToDecimal(drv["amount"]) : 0;
            }

            lblTotal.Text = $"{LocalizationManager.GetString("Exp_Total")}: {CurrencyService.Format(total)}";
        }

        private void LoadCategories()
        {
            cmbCategory.Items.Clear();
            
            DataTable dt = DatabaseHelper.ExecuteDataTable("SELECT category_name FROM expense_categories ORDER BY category_name");
            foreach (DataRow row in dt.Rows)
            {
                cmbCategory.Items.Add(row["category_name"].ToString());
            }
        }

        private void BtnAddCategory_Click(object sender, EventArgs e)
        {
            using (var frm = new ManageExpenseCategoriesForm())
            {
                frm.ShowDialog();
                LoadCategories();
            }
        }
    }
}

