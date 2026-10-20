using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Data;
using InventorySystem.Controls;
using InventorySystem.Helpers;

namespace InventorySystem.Forms
{
    public class ProductSelectorForm : BaseModalForm
    {
        private TextBox txtSearch;
        private DataGridView dgvProducts;
        
        public int SelectedPartId { get; private set; }
        public string SelectedPartName { get; private set; }
        public decimal SelectedPrice { get; private set; }
        public int SelectedStock { get; private set; }

        public ProductSelectorForm()
        {
            InitializeComponent();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();
            LoadProducts();
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            this.TitleText = LocalizationManager.GetString("Title_SelectProduct");
            SetFooterButtons(
                LocalizationManager.GetString("Msg_AddSelected"),
                LocalizationManager.GetString("Popup_Cancel"),
                (s, e) => SelectAndClose(),
                (s, e) => { DialogResult = DialogResult.Cancel; Close(); }
            );
            LocalizeGridHeaders();
        }

        private void LocalizeGridHeaders()
        {
            if (dgvProducts?.Columns == null) return;
            if (dgvProducts.Columns["Name"] != null) dgvProducts.Columns["Name"].HeaderText = LocalizationManager.GetString("Prod_GridName");
            if (dgvProducts.Columns["SKU"] != null) dgvProducts.Columns["SKU"].HeaderText = LocalizationManager.GetString("Prod_GridSKU");
            if (dgvProducts.Columns["Barcode"] != null) dgvProducts.Columns["Barcode"].HeaderText = LocalizationManager.GetString("Prod_GridBarcode");
            if (dgvProducts.Columns["Price"] != null) dgvProducts.Columns["Price"].HeaderText = LocalizationManager.GetString("Prod_GridPrice");
            if (dgvProducts.Columns["Stock"] != null) dgvProducts.Columns["Stock"].HeaderText = LocalizationManager.GetString("Prod_GridStock");
        }

        private void InitializeComponent()
        {
            // Adaptive sizing handled by BaseModalForm.OnLoad
            this.TitleText = LocalizationManager.GetString("Title_SelectProduct");

            TableLayoutPanel tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(15) };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            txtSearch = new TextBox { Dock = DockStyle.Fill, Font = ThemeConfig.SubHeaderFont, Margin = new Padding(0, 0, 0, 10) };
            txtSearch.TextChanged += (s, e) => LoadProducts();
            
            dgvProducts = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false };
            dgvProducts.DataError += (s, e) => { e.ThrowException = false; };
            dgvProducts.CellDoubleClick += (s, e) => SelectAndClose();
            
            tlp.Controls.Add(txtSearch, 0, 0);
            tlp.Controls.Add(dgvProducts, 0, 1);

            this.ContentPanel.Controls.Add(tlp);

            ApplyTheme();
        }

        private void ApplyTheme()
        {
            ThemeConfig.ApplyGridTheme(dgvProducts);
            // Footer buttons are styled automatically by BaseModalForm
        }

        private void LoadProducts()
        {
            try
            {
                string search = txtSearch.Text.Trim();
                string sql = "SELECT id, part_name as Name, part_number as SKU, barcode as Barcode, selling_price as Price, quantity_in_stock as Stock FROM parts WHERE date_deleted IS NULL";
                if (!string.IsNullOrEmpty(search))
                {
                    sql += $" AND (part_name LIKE '%{search}%' OR part_number LIKE '%{search}%' OR barcode LIKE '%{search}%')";
                }
                dgvProducts.DataSource = DatabaseHelper.ExecuteDataTable(sql);
                if (dgvProducts.Columns["id"] != null) dgvProducts.Columns["id"].Visible = false;
                LocalizeGridHeaders();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(ex.Message);
            }
        }

        private void SelectAndClose()
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                var row = dgvProducts.SelectedRows[0];
                SelectedPartId = Convert.ToInt32(row.Cells["id"].Value);
                SelectedPartName = row.Cells["Name"].Value.ToString();
                SelectedPrice = Convert.ToDecimal(row.Cells["Price"].Value);
                SelectedStock = Convert.ToInt32(row.Cells["Stock"].Value);

                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}

