using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Controls;
using InventorySystem.Helpers;
using InventorySystem.Services;

namespace InventorySystem.Forms
{
    /// <summary>
    /// Shows raw milk received (from Expenses) and lets the user record what it was turned into.
    /// Each production adds the made quantity to the chosen product's stock.
    /// </summary>
    public class MilkProductionForm : UserControl
    {
        private Label lblTitle;
        private Label lblRawStock;
        private ModernTextBox txtSearch;
        private CheckBox chkShowFinished;
        private DataGridView dgvIntakes;
        private DataGridView dgvProductions;
        private Label lblIntakes;
        private Label lblProductions;
        private Label lblSelected;
        private ModernComboBox cmbProduct;
        private ModernNumericUpDown numLiters;
        private ModernNumericUpDown numMade;
        private ModernTextBox txtNotes;
        private Button btnAdd;
        private Button btnDelete;
        private readonly MilkProductionService _service = new MilkProductionService();

        public MilkProductionForm()
        {
            InitializeComponent();
            ApplyLocalization();
            LoadData();
            LocalizationManager.LanguageChanged += (s, e) => { ApplyLocalization(); LoadData(txtSearch.Text); };
            GlobalEvents.OnInventoryUpdated += OnInventoryUpdated;
            this.Disposed += (s, e) => GlobalEvents.OnInventoryUpdated -= OnInventoryUpdated;
            this.VisibleChanged += (s, e) => { if (Visible) LoadData(txtSearch.Text); };
        }

        private void OnInventoryUpdated()
        {
            if (!IsHandleCreated || IsDisposed) return;
            BeginInvoke((Action)UpdateRawStockLabel);
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1100, 750);

            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(20) };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));     // Intakes caption
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));      // Intakes grid
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));    // Entry
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));     // Productions caption
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));      // Productions grid
            this.Controls.Add(mainLayout);

            // Header
            lblTitle = ThemeConfig.CreateStandardHeader(LocalizationManager.GetString("Milk_Title", "Milk Processing"));
            txtSearch = new ModernTextBox { IsSearch = true, ShowLabel = false, Size = new Size(320, 35) };
            txtSearch.TextChanged += (s, e) => LoadData(txtSearch.Text);
            lblRawStock = new Label
            {
                Font = new Font(ThemeConfig.AppFontFamily, 11F, FontStyle.Bold),
                ForeColor = ThemeConfig.PrimaryColor,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight,
                RightToLeft = RightToLeft.No
            };
            mainLayout.Controls.Add(ThemeConfig.CreateGlobalFormHeader(lblTitle, txtSearch, new Control[] { lblRawStock }), 0, 0);

            // Intakes caption + "show finished"
            FlowLayoutPanel pnlIntakeCaption = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = new Padding(0) };
            lblIntakes = new Label { AutoSize = true, Font = ThemeConfig.SmallBoldFont, ForeColor = ThemeConfig.TextColorDark, Margin = new Padding(0, 8, 20, 0) };
            chkShowFinished = new CheckBox { AutoSize = true, Font = ThemeConfig.StandardFont, Cursor = Cursors.Hand, FlatStyle = FlatStyle.System, Margin = new Padding(0, 5, 0, 0) };
            chkShowFinished.CheckedChanged += (s, e) => LoadData(txtSearch.Text);
            pnlIntakeCaption.Controls.Add(lblIntakes);
            pnlIntakeCaption.Controls.Add(chkShowFinished);
            mainLayout.Controls.Add(pnlIntakeCaption, 0, 1);

            // Intakes grid
            dgvIntakes = CreateGrid();
            dgvIntakes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "expense_id", Width = 60 });
            dgvIntakes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", DataPropertyName = "expense_date", Width = 140 });
            dgvIntakes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Supplier", DataPropertyName = "supplier_name", Width = 170 });
            dgvIntakes.Columns.Add(new DataGridViewTextBoxColumn { Name = "MilkType", DataPropertyName = "milk_type", Width = 100 });
            dgvIntakes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", DataPropertyName = "description", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvIntakes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Liters", DataPropertyName = "liters", Width = 90, DefaultCellStyle = { Format = "0.##" } });
            dgvIntakes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Used", DataPropertyName = "used", Width = 90, DefaultCellStyle = { Format = "0.##" } });
            dgvIntakes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Remaining", DataPropertyName = "remaining", Width = 100, DefaultCellStyle = { Format = "0.##", Font = ThemeConfig.SmallBoldFont } });
            dgvIntakes.SelectionChanged += (s, e) => OnIntakeSelected();
            mainLayout.Controls.Add(ThemeConfig.CreateCardPanel(dgvIntakes), 0, 2);

            // Entry
            TableLayoutPanel entry = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 2, Padding = new Padding(10), BackColor = ThemeConfig.SurfaceColor };
            entry.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F)); // Product
            entry.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F)); // Liters used
            entry.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F)); // Qty made
            entry.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // Notes
            entry.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F)); // Actions
            entry.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            entry.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            lblSelected = new Label { AutoSize = true, Font = ThemeConfig.SmallBoldFont, ForeColor = ThemeConfig.PrimaryColor, Margin = new Padding(5, 2, 0, 0) };
            entry.Controls.Add(lblSelected, 0, 0);
            entry.SetColumnSpan(lblSelected, 5);

            cmbProduct = new ModernComboBox { Dock = DockStyle.Fill, Margin = new Padding(5, 0, 5, 0), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbProduct.InnerComboBox.SelectedIndexChanged += (s, e) => UpdateMadeLabel();
            numLiters = new ModernNumericUpDown { DecimalPlaces = 2, Maximum = 1000000, Width = 150 };
            numMade = new ModernNumericUpDown { DecimalPlaces = 2, Maximum = 1000000, Width = 150 };
            txtNotes = new ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(5, 0, 5, 0) };

            FlowLayoutPanel pnlActions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(0, 25, 0, 0) };
            btnAdd = new Button { Size = new Size(140, 35), Margin = new Padding(5, 0, 5, 0) };
            btnAdd.Click += BtnAdd_Click;
            btnDelete = new Button { Size = new Size(140, 35), Margin = new Padding(5, 0, 5, 0) };
            btnDelete.Click += BtnDelete_Click;
            pnlActions.Controls.Add(btnAdd);
            pnlActions.Controls.Add(btnDelete);

            entry.Controls.Add(cmbProduct, 0, 1);
            entry.Controls.Add(WrapNumeric(numLiters), 1, 1);
            entry.Controls.Add(WrapNumeric(numMade), 2, 1);
            entry.Controls.Add(txtNotes, 3, 1);
            entry.Controls.Add(pnlActions, 4, 1);

            Panel pnlEntryCard = ThemeConfig.CreateCardPanel(entry);
            pnlEntryCard.Margin = new Padding(0, 10, 0, 10);
            mainLayout.Controls.Add(pnlEntryCard, 0, 3);

            // Productions caption + grid
            lblProductions = new Label { AutoSize = true, Font = ThemeConfig.SmallBoldFont, ForeColor = ThemeConfig.TextColorDark, Margin = new Padding(0, 8, 0, 0) };
            mainLayout.Controls.Add(lblProductions, 0, 4);

            dgvProductions = CreateGrid();
            dgvProductions.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "production_id", Visible = false });
            dgvProductions.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", DataPropertyName = "production_date", Width = 140 });
            dgvProductions.Columns.Add(new DataGridViewTextBoxColumn { Name = "Intake", DataPropertyName = "expense_id", Width = 70 });
            dgvProductions.Columns.Add(new DataGridViewTextBoxColumn { Name = "Product", DataPropertyName = "part_name", Width = 200 });
            dgvProductions.Columns.Add(new DataGridViewTextBoxColumn { Name = "LitersUsed", DataPropertyName = "liters_used", Width = 110, DefaultCellStyle = { Format = "0.##" } });
            dgvProductions.Columns.Add(new DataGridViewTextBoxColumn { Name = "Made", DataPropertyName = "quantity_made", Width = 110, DefaultCellStyle = { Format = "0.##" } });
            dgvProductions.Columns.Add(new DataGridViewTextBoxColumn { Name = "Unit", DataPropertyName = "unit", Width = 70 });
            dgvProductions.Columns.Add(new DataGridViewTextBoxColumn { Name = "Notes", DataPropertyName = "notes", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvProductions.Columns.Add(new DataGridViewTextBoxColumn { Name = "User", DataPropertyName = "recorded_by", Width = 100 });
            mainLayout.Controls.Add(ThemeConfig.CreateCardPanel(dgvProductions), 0, 5);
        }

        private static DataGridView CreateGrid()
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = ThemeConfig.SurfaceColor,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            ThemeConfig.ApplyGridTheme(dgv);
            return dgv;
        }

        // Same trick as MonthlyExpensesForm: keep the TLP cell from stretching the spinner.
        private static Panel WrapNumeric(ModernNumericUpDown num)
        {
            Panel wrapper = new Panel { Dock = DockStyle.Fill, Margin = new Padding(5, 0, 5, 0) };
            wrapper.Controls.Add(num);
            wrapper.Resize += (s, e) => { num.Width = wrapper.Width; num.Location = new Point(0, 0); };
            return wrapper;
        }

        private void ApplyLocalization()
        {
            Func<string, string, string> L = LocalizationManager.GetString;
            LocalizationManager.ApplyRTL(this);

            lblTitle.Text = L("Milk_Title", "Milk Processing");
            txtSearch.PlaceholderText = L("Milk_Search", "Search supplier, description...");
            lblIntakes.Text = L("Milk_Intakes", "Raw milk received");
            chkShowFinished.Text = L("Milk_ShowFinished", "Show fully used");
            lblProductions.Text = L("Milk_Productions", "What was made");
            cmbProduct.LabelText = L("Milk_Product", "Product made");
            numLiters.LabelText = L("Milk_LitersUsed", "Milk used (L)");
            txtNotes.LabelText = L("Milk_Notes", "Notes");
            UpdateMadeLabel();
            ThemeConfig.ApplyStandardAddButton(btnAdd, "Milk_Add");
            ThemeConfig.ApplyStandardDeleteButton(btnDelete, "Milk_Delete");

            dgvIntakes.Columns["Id"].HeaderText = L("Users_GridID", "ID");
            dgvIntakes.Columns["Date"].HeaderText = L("Exp_Date", "Date");
            dgvIntakes.Columns["Supplier"].HeaderText = L("Milk_Supplier", "Supplier");
            dgvIntakes.Columns["MilkType"].HeaderText = L("Milk_Type", "Milk type");
            dgvIntakes.Columns["Description"].HeaderText = L("Exp_DescriptionLabel", "Description");
            dgvIntakes.Columns["Liters"].HeaderText = L("Milk_Liters", "Liters");
            dgvIntakes.Columns["Used"].HeaderText = L("Milk_Used", "Used");
            dgvIntakes.Columns["Remaining"].HeaderText = L("Milk_Remaining", "Remaining");

            dgvProductions.Columns["Date"].HeaderText = L("Exp_Date", "Date");
            dgvProductions.Columns["Intake"].HeaderText = L("Milk_Intake", "Intake #");
            dgvProductions.Columns["Product"].HeaderText = L("Milk_Product", "Product made");
            dgvProductions.Columns["LitersUsed"].HeaderText = L("Milk_LitersUsed", "Milk used (L)");
            dgvProductions.Columns["Made"].HeaderText = L("Milk_Made", "Quantity made");
            dgvProductions.Columns["Unit"].HeaderText = L("Milk_Unit", "Unit");
            dgvProductions.Columns["Notes"].HeaderText = L("Milk_Notes", "Notes");
            dgvProductions.Columns["User"].HeaderText = L("Milk_User", "User");

            UpdateSelectedLabel();
        }

        public void LoadData(string search = "")
        {
            try
            {
                _service.SyncRawMilkStock();
                LoadProducts();

                int? keepId = SelectedIntakeId();
                DataTable dt = _service.GetMilkIntakes(!chkShowFinished.Checked);
                if (!string.IsNullOrWhiteSpace(search))
                {
                    string s = search.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]");
                    dt.DefaultView.RowFilter = $"supplier_name LIKE '%{s}%' OR description LIKE '%{s}%' OR milk_type LIKE '%{s}%'";
                }
                dgvIntakes.DataSource = dt;

                if (keepId.HasValue)
                {
                    foreach (DataGridViewRow row in dgvIntakes.Rows)
                    {
                        if (Convert.ToInt32(row.Cells["Id"].Value) == keepId.Value) { row.Selected = true; dgvIntakes.CurrentCell = row.Cells["Date"]; break; }
                    }
                }
                OnIntakeSelected();
                UpdateRawStockLabel();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "MilkProductionForm.LoadData");
            }
        }

        private void LoadProducts()
        {
            object keep = cmbProduct.SelectedValue;
            var dt = _service.GetProducibleProducts();
            cmbProduct.DisplayMember = "part_name";
            cmbProduct.ValueMember = "id";
            cmbProduct.DataSource = dt;
            if (keep != null) cmbProduct.SelectedValue = keep;
            else cmbProduct.SelectedIndex = -1;
        }

        private int? SelectedIntakeId()
        {
            if (dgvIntakes.CurrentRow == null || dgvIntakes.CurrentRow.Index < 0) return null;
            var v = dgvIntakes.CurrentRow.Cells["Id"].Value;
            return v == null || v == DBNull.Value ? (int?)null : Convert.ToInt32(v);
        }

        private void OnIntakeSelected()
        {
            int? id = SelectedIntakeId();
            dgvProductions.DataSource = _service.GetProductions(id);
            UpdateSelectedLabel();
            if (id.HasValue && numLiters.Value == 0)
            {
                // Suggest using whatever is left
                decimal remaining = _service.GetRemainingLiters(id.Value);
                if (remaining > 0) numLiters.Value = Math.Min(remaining, numLiters.Maximum);
            }
        }

        private void UpdateSelectedLabel()
        {
            int? id = SelectedIntakeId();
            if (!id.HasValue)
            {
                lblSelected.Text = LocalizationManager.GetString("Milk_SelectIntake", "Select a milk intake above to record what was made from it.");
                return;
            }
            var row = dgvIntakes.CurrentRow;
            string supplier = row.Cells["Supplier"].Value?.ToString();
            decimal remaining = row.Cells["Remaining"].Value == null || row.Cells["Remaining"].Value == DBNull.Value ? 0 : Convert.ToDecimal(row.Cells["Remaining"].Value);
            lblSelected.Text = string.Format(LocalizationManager.GetString("Milk_SelectedFmt", "Intake #{0} {1} - {2} L left"),
                id.Value, string.IsNullOrEmpty(supplier) ? "" : "(" + supplier + ")", remaining.ToString("0.##"));
        }

        private void UpdateMadeLabel()
        {
            string unit = "";
            if (cmbProduct.SelectedItem is DataRowView drv) unit = drv["unit"]?.ToString();
            string label = LocalizationManager.GetString("Milk_Made", "Quantity made");
            numMade.LabelText = string.IsNullOrWhiteSpace(unit) ? label : $"{label} ({unit})";
        }

        private void UpdateRawStockLabel()
        {
            int rawId = _service.GetRawMilkPartId();
            decimal stock = DatabaseHelper.ExecuteScalar<decimal>("SELECT quantity_in_stock FROM parts WHERE id = @id",
                new Microsoft.Data.Sqlite.SqliteParameter("@id", rawId));
            lblRawStock.Text = $"{LocalizationManager.GetString("Milk_RawStock", "Raw milk in stock")}: {stock:0.##} L";
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            int? intakeId = SelectedIntakeId();
            if (!intakeId.HasValue)
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("Milk_SelectIntake", "Select a milk intake above to record what was made from it."));
                return;
            }
            if (cmbProduct.SelectedValue == null)
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("Milk_ReqProduct", "Choose the product that was made."));
                return;
            }

            string error = _service.AddProduction(intakeId.Value, Convert.ToInt32(cmbProduct.SelectedValue), numLiters.Value, numMade.Value, txtNotes.Text);
            if (error != null)
            {
                MessageHelper.ShowWarning(error);
                return;
            }

            numLiters.Value = 0;
            numMade.Value = 0;
            txtNotes.Clear();
            LoadData(txtSearch.Text);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProductions.CurrentRow == null || dgvProductions.CurrentRow.Index < 0) return;
            if (!MessageHelper.ShowConfirmation(LocalizationManager.GetString("Milk_ConfirmDelete", "Delete this production? The quantity made will be removed from stock."))) return;

            int id = Convert.ToInt32(dgvProductions.CurrentRow.Cells["Id"].Value);
            _service.DeleteProduction(id);
            LoadData(txtSearch.Text);
        }
    }
}
