using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using InventorySystem.Data;
using InventorySystem.Helpers;
using InventorySystem.Services;
using InventorySystem.Controls;

namespace InventorySystem.Forms
{
    public partial class SuppliersForm : UserControl
    {
        private DataGridView dgvSuppliers;
        private Button btnAdd;
        private Button btnDetails;
        private Button btnImport;
        private Button btnExport;
        private Label lblSuppliersTitle;
        private ModernTextBox txtSearch;

        public SuppliersForm()
        {
            InitializeComponent();
            ApplyTheme();

            InventorySystem.Helpers.LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();
            ApplyPermissions();
        }

        private void ApplyLocalization()
        {
            InventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            InventorySystem.Helpers.LocalizationManager.TranslateControl(this);
            Func<string, string> L = InventorySystem.Helpers.LocalizationManager.GetString;

            if (lblSuppliersTitle != null) lblSuppliersTitle.Text = L("Sup_Title");

            if (txtSearch != null)
            {
                txtSearch.PlaceholderText = L("Sup_Search");
            }

            if (btnAdd != null) ThemeConfig.ApplyStandardAddButton(btnAdd, "Sup_AddSupplier");
            if (btnDetails != null) btnDetails.Invalidate();
            if (btnImport != null) btnImport.Invalidate();
            if (btnExport != null) btnExport.Invalidate();

            var ctrlDel = this.Controls.Find("btnDeleteSelected", true);
            if (ctrlDel.Length > 0 && ctrlDel[0] is Button bDel) ThemeConfig.ApplyStandardDeleteButton(bDel, "Sup_Delete");

            if (dgvSuppliers != null && dgvSuppliers.Columns.Count > 0)
            {
                if (dgvSuppliers.Columns.Contains("colCompany")) dgvSuppliers.Columns["colCompany"].HeaderText = L("Sup_GridCompany");
                if (dgvSuppliers.Columns.Contains("colContact")) dgvSuppliers.Columns["colContact"].HeaderText = L("Sup_GridContact");
                if (dgvSuppliers.Columns.Contains("colPhone")) dgvSuppliers.Columns["colPhone"].HeaderText = L("Sup_GridPhone");
                if (dgvSuppliers.Columns.Contains("colEmail")) dgvSuppliers.Columns["colEmail"].HeaderText = L("Sup_GridEmail");
                if (dgvSuppliers.Columns.Contains("colAddress")) dgvSuppliers.Columns["colAddress"].HeaderText = LocalizationManager.GetString("Cust_GridAddress", "Address");
                if (dgvSuppliers.Columns.Contains("colBalance")) dgvSuppliers.Columns["colBalance"].HeaderText = LocalizationManager.GetString("Cust_GridBalance", "Balance Due");
                if (dgvSuppliers.Columns.Contains("colCreditLimit")) dgvSuppliers.Columns["colCreditLimit"].HeaderText = L("AddCust_CreditLimit");
                if (dgvSuppliers.Columns.Contains("colDueDate")) dgvSuppliers.Columns["colDueDate"].HeaderText = LocalizationManager.GetString("AddSup_DueDate", "Due Date");
                if (dgvSuppliers.Columns.Contains("colActions")) dgvSuppliers.Columns["colActions"].HeaderText = L("Sup_GridActions");
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible && !this.DesignMode)
            {
                LoadData();
            }
        }

        private void InitializeComponent()
        {
            this.dgvSuppliers = new System.Windows.Forms.DataGridView();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDetails = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.lblSuppliersTitle = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSuppliers)).BeginInit();
            this.SuspendLayout();

            // Main Layout
            TableLayoutPanel tlpMain = new TableLayoutPanel();
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.RowCount = 2;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.Dock = DockStyle.Fill;

            tlpMain.Padding = new Padding(20);

            // Header Panel
            this.lblSuppliersTitle = ThemeConfig.CreateStandardHeader("Supplier Management");
            this.lblSuppliersTitle.Name = "lblSuppliersTitle";

            // Search Bar
            this.txtSearch = new ModernTextBox();
            txtSearch.IsSearch = true;
            txtSearch.ShowLabel = false;
            txtSearch.PlaceholderText = LocalizationManager.GetString("Sup_Search");
            txtSearch.Size = new Size(320, 35);
            txtSearch.TextChanged += (s, e) =>
            {
                string ph = InventorySystem.Helpers.LocalizationManager.GetString("Sup_Search");
                if (txtSearch.Text != ph && txtSearch.Text != "Search...")
                    LoadData(txtSearch.Text);
            };

            // Buttons
            // Add New Supplier Button
            this.btnAdd.Size = new System.Drawing.Size(160, 35);
            this.btnAdd.Name = "btnAddSupplier";
            this.btnAdd.Click += BtnAdd_Click;
            ThemeConfig.ApplyStandardAddButton(this.btnAdd, "Sup_AddSupplier");

            // Supplier Details Button
            this.btnDetails.Size = new System.Drawing.Size(160, 35);
            this.btnDetails.Text = "";
            this.btnDetails.Name = "btnDetailsSup";
            this.btnDetails.FlatStyle = FlatStyle.Flat;
            this.btnDetails.FlatAppearance.BorderSize = 0;
            this.btnDetails.BackColor = Color.Transparent;
            this.btnDetails.Cursor = Cursors.Hand;
            this.btnDetails.Click += BtnDetails_Click;
            this.btnDetails.Paint += (s, e) => ThemeConfig.DrawIconButton(btnDetails, e.Graphics, "view", "Sup_Details", ThemeConfig.TextColorLight, Color.FromArgb(139, 92, 246), false);

            // Delete Selected Button
            Button btnDeleteSelected = new Button();
            btnDeleteSelected.Size = new Size(130, 35);
            btnDeleteSelected.Name = "btnDeleteSelected";
            btnDeleteSelected.Click += (s, e) =>
            {
                var checkedIds = new System.Collections.Generic.List<int>();
                foreach (DataGridViewRow row in dgvSuppliers.Rows)
                {
                    var chkCell = row.Cells["colCheck"] as DataGridViewCheckBoxCell;
                    if (chkCell != null && Convert.ToBoolean(chkCell.Value ?? false))
                    {
                        if (int.TryParse(row.Cells["ID"].Value?.ToString(), out int sId))
                            checkedIds.Add(sId);
                    }
                }
                if (checkedIds.Count == 0)
                {
                    MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_SelectOneSupplier"));
                    return;
                }
                string confirmMsg = string.Format(LocalizationManager.GetString("Msg_ConfirmDeleteSuppliers", "Are you sure you want to delete {0} selected suppliers?"), checkedIds.Count);
                if (MessageHelper.ConfirmAction(confirmMsg))
                {
                    Services.SupplierService supplierService = new Services.SupplierService();
                    foreach (int i in checkedIds) supplierService.DeleteSupplier(i);
                    LoadData();
                }
            };
            ThemeConfig.ApplyStandardDeleteButton(btnDeleteSelected, "Sup_Delete");

            // Export Button
            this.btnExport.Size = new System.Drawing.Size(100, 35);
            this.btnExport.Name = "btnExportSup";
            this.btnExport.Click += BtnExport_Click;
            ThemeConfig.ApplyStandardExportButton(this.btnExport, "Sup_Export");

            // Import Button
            this.btnImport.Size = new System.Drawing.Size(100, 35);
            this.btnImport.Name = "btnImportSup";
            this.btnImport.Click += BtnImport_Click;
            ThemeConfig.ApplyStandardImportButton(this.btnImport, "Sup_Import");

            var actionButtons = new Control[] { btnDeleteSelected, btnImport, btnExport, btnAdd, btnDetails };
            TableLayoutPanel tlpHeader = ThemeConfig.CreateGlobalFormHeader(this.lblSuppliersTitle, this.txtSearch, actionButtons);

            tlpMain.Controls.Add(tlpHeader, 0, 0);

            // Grid Config
            this.dgvSuppliers.Dock = DockStyle.Fill;
            this.dgvSuppliers.AllowUserToAddRows = false;
            this.dgvSuppliers.ReadOnly = false;
            this.dgvSuppliers.RowHeadersVisible = false;
            this.dgvSuppliers.BackgroundColor = ThemeConfig.SurfaceColor;
            this.dgvSuppliers.BorderStyle = BorderStyle.None;
            this.dgvSuppliers.AutoGenerateColumns = false;
            this.dgvSuppliers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvSuppliers.MultiSelect = false;

            dgvSuppliers.RowTemplate.Height = 60;
            dgvSuppliers.DefaultCellStyle.BackColor = ThemeConfig.SurfaceColor;
            dgvSuppliers.DefaultCellStyle.ForeColor = ThemeConfig.TextColorDark;
            dgvSuppliers.DefaultCellStyle.SelectionBackColor = ThemeConfig.ActiveBackColor;
            dgvSuppliers.DefaultCellStyle.SelectionForeColor = ThemeConfig.TextColorDark;
            dgvSuppliers.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvSuppliers.GridColor = ThemeConfig.BorderColor;

            ThemeConfig.ApplyGridTheme(dgvSuppliers);
            ThemeConfig.ApplyHeaderCheckBox(dgvSuppliers, "colCheck");

            dgvSuppliers.CellPainting += DgvSuppliers_CellPainting;
            dgvSuppliers.CellMouseDown += DgvSuppliers_CellMouseDown;
            dgvSuppliers.CellMouseMove += DgvSuppliers_CellMouseMove;
            dgvSuppliers.CellMouseLeave += DgvSuppliers_CellMouseLeave;
            dgvSuppliers.DataError += (s, e) =>
            {
                Console.WriteLine("DataError: " + (e.Exception != null ? e.Exception.Message : "Unknown"));
                e.ThrowException = false;
            };

            // Columns
            dgvSuppliers.Columns.Clear();
            dgvSuppliers.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colCheck", HeaderText = "", Width = 30, ReadOnly = false });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCompany", HeaderText = LocalizationManager.GetString("Sup_GridCompany"), DataPropertyName = "supplier_name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPhone", HeaderText = LocalizationManager.GetString("Sup_GridPhone"), DataPropertyName = "phone", Width = 120, ReadOnly = true });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEmail", HeaderText = LocalizationManager.GetString("Sup_GridEmail"), DataPropertyName = "email", Width = 200, ReadOnly = true });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAddress", HeaderText = LocalizationManager.GetString("Cust_GridAddress"), DataPropertyName = "address", Width = 200, ReadOnly = true });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBalance", HeaderText = LocalizationManager.GetString("Cust_GridBalance"), DataPropertyName = "balance_due", Width = 120, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDueDate", HeaderText = LocalizationManager.GetString("AddSup_DueDate"), DataPropertyName = "payment_due_date", Width = 100, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" } });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colActions", HeaderText = LocalizationManager.GetString("Sup_GridActions"), Width = 100, ReadOnly = true });

            // Hidden Fields
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", DataPropertyName = "id", Visible = false });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "reminder_days", DataPropertyName = "reminder_days", Visible = false });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colContact", DataPropertyName = "contact_person", Visible = false });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "type", DataPropertyName = "type", Visible = false });

            // Card Panel (Rounded body)
            Panel pnlCard = ThemeConfig.CreateCardPanel(dgvSuppliers);
            tlpMain.Controls.Add(tlpHeader, 0, 0);
            tlpMain.Controls.Add(pnlCard, 0, 1);

            this.Controls.Add(tlpMain);
            this.Size = new System.Drawing.Size(950, 600);

            ((System.ComponentModel.ISupportInitialize)(this.dgvSuppliers)).EndInit();
            this.ResumeLayout(false);
        }

        private void DgvSuppliers_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (dgvSuppliers.Columns[e.ColumnIndex].Name == "colActions")
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                int iconSize = 24;
                int gap = 12;
                int totalWidth = (iconSize * 2) + gap;
                int startX = e.CellBounds.X + (e.CellBounds.Width - totalWidth) / 2;
                int startY = e.CellBounds.Y + (e.CellBounds.Height - iconSize) / 2;

                // Edit Icon
                Rectangle editRect = new Rectangle(startX, startY, iconSize, iconSize);
                Image imgEdit = ThemeConfig.GetNuricon("edit");
                if (imgEdit != null) e.Graphics.DrawImage(imgEdit, editRect);

                // Delete Icon
                Rectangle delRect = new Rectangle(startX + iconSize + gap, startY, iconSize, iconSize);
                Image imgDelete = ThemeConfig.GetNuricon("delete");
                if (imgDelete != null) e.Graphics.DrawImage(imgDelete, delRect);
            }
        }

        private void DgvSuppliers_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.Button != MouseButtons.Left) return;

            string colName = dgvSuppliers.Columns[e.ColumnIndex].Name;
            if (colName != "colActions") return;

            int id = Convert.ToInt32(dgvSuppliers.Rows[e.RowIndex].Cells["ID"].Value);

            int colWidth = dgvSuppliers.Columns[e.ColumnIndex].Width;
            int iconSize = 24;
            int gap = 12;
            int totalWidth = (iconSize * 2) + gap;
            int startX = (colWidth - totalWidth) / 2;

            if (e.X >= startX - 4 && e.X <= startX + iconSize + 4) // Edit Rect with tolerance
            {
                // Edit
                if (!InventorySystem.Helpers.UserSession.IsAdmin)
                {
                    MessageHelper.ShowWarning(LocalizationManager.GetString("Error_NoPermissionEditSuppliers", "You do not have permission to edit suppliers."));
                    return;
                }

                string name = dgvSuppliers.Rows[e.RowIndex].Cells["colCompany"].Value?.ToString() ?? "";
                string phone = dgvSuppliers.Rows[e.RowIndex].Cells["colPhone"].Value?.ToString() ?? "";
                string email = dgvSuppliers.Rows[e.RowIndex].Cells["colEmail"].Value?.ToString() ?? "";
                string contact = dgvSuppliers.Rows[e.RowIndex].Cells["colContact"].Value?.ToString() ?? "";
                string address = dgvSuppliers.Rows[e.RowIndex].Cells["colAddress"].Value?.ToString() ?? "";
                string type = dgvSuppliers.Rows[e.RowIndex].Cells["type"].Value?.ToString() ?? "Company";
                DateTime? dueDate = dgvSuppliers.Rows[e.RowIndex].Cells["colDueDate"].Value as DateTime?;

                object remVal = dgvSuppliers.Rows[e.RowIndex].Cells["reminder_days"].Value;
                int reminderDays = (remVal == null || remVal == DBNull.Value) ? 0 : Convert.ToInt32(remVal);

                AddSupplierForm form = new AddSupplierForm(id, name, phone, email, address, type, dueDate, reminderDays, contact);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    SupplierData.UpdateSupplier(id, form.SupplierName, form.Phone, form.Email, form.Address, form.SupplierType, form.ContactPerson, form.DueDate, form.ReminderDays);
                    LoadData();
                }
            }
            else if (e.X >= startX + iconSize + gap - 4 && e.X <= startX + totalWidth + 4) // Delete Rect with tolerance
            {
                // Delete
                if (!InventorySystem.Helpers.UserSession.IsAdmin)
                {
                    MessageHelper.ShowWarning(LocalizationManager.GetString("Error_NoPermissionDeleteSuppliers", "You do not have permission to delete suppliers."));
                    return;
                }

                string deleteConfirm = LocalizationManager.GetString("Msg_ConfirmDelete");
                if (MessageHelper.ConfirmAction(deleteConfirm))
                {
                    SupplierData.DeleteSupplier(id);
                    LoadData();
                }
            }
        }

        private void DgvSuppliers_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvSuppliers.Columns[e.ColumnIndex].Name == "colActions")
            {
                dgvSuppliers.Cursor = Cursors.Hand;
            }
            else
            {
                dgvSuppliers.Cursor = Cursors.Default;
            }
        }

        private void DgvSuppliers_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dgvSuppliers.Cursor = Cursors.Default;
        }

        private void DrawRoundedButton(Graphics g, Rectangle rect, string text, Color bgColor, Color textColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, rect.Width - 1, rect.Height - 1);
            using (var path = GetRoundedRect(r, 8))
            using (var brush = new SolidBrush(bgColor))
            {
                g.FillPath(brush, path);
            }
            TextRenderer.DrawText(g, text, ThemeConfig.ButtonFont, r, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void ApplyTheme()
        {

        }

        private void LoadData(string search = "")
        {
            try
            {
                string sql = "SELECT id, supplier_name, contact_person, phone, email, address, type, 0 as active_orders, payment_due_date, reminder_days FROM suppliers WHERE date_deleted IS NULL";

                if (!string.IsNullOrEmpty(search))
                {
                    sql += $" AND (supplier_name LIKE '%{search}%' OR phone LIKE '%{search}%' OR email LIKE '%{search}%' OR contact_person LIKE '%{search}%')";
                }

                sql += " ORDER BY supplier_name";

                DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
                dgvSuppliers.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(("Error loading data: ") + ex.Message);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            AddSupplierForm form = new AddSupplierForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                SupplierData.AddSupplier(form.SupplierName, form.Phone, form.Email, form.Address, form.SupplierType, form.ContactPerson, form.DueDate, form.ReminderDays);
                LoadData();
            }
        }

        private void BtnDetails_Click(object sender, EventArgs e)
        {
            if (dgvSuppliers.SelectedRows.Count == 0) { MessageHelper.ShowInfo("Select a supplier."); return; }
            int id = Convert.ToInt32(dgvSuppliers.SelectedRows[0].Cells["ID"].Value);
            string name = dgvSuppliers.SelectedRows[0].Cells["colCompany"].Value.ToString();
            var form = new SupplierDetailsForm(id, name);
            form.ShowDialog();
            LoadData();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            ExportToCsv();
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            ImportFromCsv();
        }

        private void ExportToCsv()
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv";
                saveDialog.FileName = $"Suppliers_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                saveDialog.Title = "Export Suppliers";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    DataTable dt = Services.CsvImportExportService.BuildSuppliersExportTable();

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoDataExport"));
                        return;
                    }

                    bool ok = saveDialog.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                        ? Helpers.ImportExportHelper.ExportToCsv(dt, saveDialog.FileName)
                        : Helpers.ImportExportHelper.ExportToXlsx(dt, saveDialog.FileName, "Suppliers");
                    if (ok)
                    {
                        string successMsg = string.Format(LocalizationManager.GetString("Msg_ExportSuppliersSuccess", "Exported {0} suppliers successfully!"), dt.Rows.Count);
                        MessageHelper.ShowSuccess(successMsg);
                    }
                    else
                    {
                        MessageHelper.ShowError(LocalizationManager.GetString("Msg_ExportFailed"));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError((LocalizationManager.GetString("Msg_ExportError")) + ex.Message);
            }
        }

        private void ImportFromCsv()
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "CSV Files (*.csv)|*.csv";
                openDialog.Title = "Import Suppliers from CSV";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    DataTable dt = Helpers.ImportExportHelper.ImportFromCsv(openDialog.FileName);

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoDataFile"));
                        return;
                    }

                    var rowDicts = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, string>>();
                    foreach (DataRow row in dt.Rows)
                        rowDicts.Add(Services.CsvImportExportService.RowFromDataRow(row));

                    var result = Services.CsvImportExportService.ImportSuppliers(rowDicts);
                    LoadData();
                    string completeMsg = string.Format(
                        LocalizationManager.GetString("Msg_ImportCompleteResults", "Import complete!\nImported: {0}\nUpdated: {1}\nSkipped: {2}"),
                        result.Imported, result.Updated, result.Skipped);
                    MessageHelper.ShowSuccess(completeMsg);
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError((LocalizationManager.GetString("Msg_ImportError")) + ex.Message);
            }
        }

        private void ImportFromExcel()
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls|All Files (*.*)|*.*";
                openDialog.Title = "Import Suppliers from Excel";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    DataTable dt = Helpers.ImportExportHelper.ImportFromExcel(openDialog.FileName);

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoDataFile"));
                        return;
                    }

                    var rowDicts = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, string>>();
                    foreach (DataRow row in dt.Rows)
                        rowDicts.Add(Services.CsvImportExportService.RowFromDataRow(row));

                    var result = Services.CsvImportExportService.ImportSuppliers(rowDicts);
                    LoadData();
                    string completeMsg = string.Format(
                        LocalizationManager.GetString("Msg_ImportCompleteResults", "Import complete!\nImported: {0}\nUpdated: {1}\nSkipped: {2}"),
                        result.Imported, result.Updated, result.Skipped);
                    MessageHelper.ShowSuccess(completeMsg);
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Import error: {ex.Message}");
            }
        }

        private void ApplyPermissions()
        {
            if (!InventorySystem.Helpers.UserSession.IsAdmin)
            {
                var ctrlDel = this.Controls.Find("btnDeleteSelected", true);
                if (ctrlDel.Length > 0) ctrlDel[0].Visible = false;
            }
        }
    }
}

