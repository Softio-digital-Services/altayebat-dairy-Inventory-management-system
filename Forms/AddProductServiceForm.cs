using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using InventorySystem.Data;
using InventorySystem.Controls;
using InventorySystem.Services;
using InventorySystem.Helpers;

namespace InventorySystem.Forms
{
    public class AddProductServiceForm : BaseModalForm
    {
        private PictureBox pbImage;
        private ModernButton btnUpload;
        
        private ModernTextBox txtName;
        private ModernTextBox txtDescription;
        private ModernTextBox txtSku;
        private ModernTextBox txtBarcode;
        private ModernButton btnAutoSku;
        private ModernButton btnScanBarcode;
        private ModernTextBox txtLocation;
        private ModernTextBox txtShelf;
        private ModernComboBox cmbCategory;
        private ModernComboBox cmbUom;
        private ModernTextBox txtBatch;
        private FlatDateTimePicker dtpExpiry;

        private RadioButton rbProduct;
        private RadioButton rbService;
        private CheckBox chkSales;
        private CheckBox chkPurchase;
        private CheckBox chkInactive;

        private ModernComboBox cmbTaxRate;
        private CheckBox chkTrackStock;
        private ModernNumericUpDown numStock;
        private ModernNumericUpDown numLowLevel;

        private ModernComboBox cmbSupplier;
        private ModernNumericUpDown numCost;

        private ModernNumericUpDown[] numPrices = new ModernNumericUpDown[4];
        private ModernTextBox[] txtGrosses = new ModernTextBox[4];
        private ModernTextBox[] txtProfits = new ModernTextBox[4];

        private int? _editPartId = null;
        private string _currentImagePath = null;
        
        private GroupBox gbType;
        private GroupBox gbSettings;
        private Label lblExp;
        private GroupBox gbStock;
        private GroupBox gbSupp;
        private GroupBox gbPrices;
        private Label lblLevel;
        private Label lblPrice;
        private Label lblGross;
        private Label lblProfit;
        private Label[] lblPriceRows = new Label[4];
        
        private GroupBox gbScale;
        private Label lblScaleStatus;
        private Label lblUnitPriceTitle;
        private Label lblWeightReadout;
        private Label lblCalculatedPrice;
        private ModernNumericUpDown numUnitPricePerKg;
        private ModernButton btnConfigScale;
        private ModernButton btnApplyScalePrice;
        private ModernButton btnApplyScaleWeight;
        private ModernButton btnReadScale;
        private decimal _currentScaleWeight = 0m;
        private string _currentScaleUnit = null;

        public AddProductServiceForm()
        {
            this.ClientSize = new Size(1050, 850);
            this.TitleText = LocalizationManager.GetString("AddPart_TitleNew", "Product / Service");
            
            InitializeUI();
            LoadDropdowns();
            
            SetFooterButtons(
                LocalizationManager.GetString("AddPart_Save", "Save"), 
                LocalizationManager.GetString("AddPart_Cancel", "Cancel"), 
                btnSave_Click, 
                btnCancel_Click
            );

            // Wire calculation events
            numCost.ValueChanged += CalculateMargins;
            for(int i=0; i<4; i++) {
                numPrices[i].ValueChanged += CalculateMargins;
            }

            rbProduct.CheckedChanged += (s, e) => { if (rbProduct.Checked) { chkTrackStock.Enabled = true; chkTrackStock.Checked = true; } };
            rbService.CheckedChanged += (s, e) => { if (rbService.Checked) { chkTrackStock.Checked = false; chkTrackStock.Enabled = false; } };
            chkTrackStock.CheckedChanged += (s, e) => { numStock.Enabled = chkTrackStock.Checked; numLowLevel.Enabled = chkTrackStock.Checked; };

            // Wire Scale events
            ScaleService.Instance.WeightReceived += Scale_WeightReceived;
            ScaleService.Instance.StatusChanged += Scale_StatusChanged;
            this.FormClosing += (s, e) => {
                ScaleService.Instance.WeightReceived -= Scale_WeightReceived;
                ScaleService.Instance.StatusChanged -= Scale_StatusChanged;
            };

            cmbUom.InnerComboBox.SelectedIndexChanged += (s, e) => {
                if (string.IsNullOrWhiteSpace(_currentScaleUnit) && cmbUom.SelectedItem != null)
                {
                    _currentScaleUnit = cmbUom.SelectedItem.ToString();
                    UpdateScaleUnitDisplay();
                }
            };

            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            Func<string, string, string> L = LocalizationManager.GetString;

            this.TitleText = _editPartId.HasValue ? L("AddPart_TitleEdit", "Edit Product / Service") : L("AddPart_TitleNew", "Product / Service");

            if (btnUpload != null) btnUpload.Text = L("AddPart_UploadImage", "Upload Image");
            if (gbType != null) gbType.Text = L("AddPart_Type", "Type");
            if (rbProduct != null) rbProduct.Text = L("AddPart_Product", "Product");
            if (rbService != null) rbService.Text = L("AddPart_Service", "Service");
            if (gbSettings != null) gbSettings.Text = L("AddPart_Settings", "Settings");
            if (chkSales != null) chkSales.Text = L("AddPart_SalesItem", "Sales item");
            if (chkPurchase != null) chkPurchase.Text = L("AddPart_PurchaseItem", "Purchase item");
            if (chkInactive != null) chkInactive.Text = L("AddPart_Inactive", "Inactive");
            if (cmbTaxRate != null) cmbTaxRate.LabelText = L("AddPart_TaxRates", "Tax Rates:");
            ReloadTaxRates();
            if (cmbCategory != null) cmbCategory.LabelText = L("AddPart_Category", "Category");
            if (lblExp != null) lblExp.Text = L("AddPart_ExpiryDate", "Expiry Date");
            
            if (txtName != null) txtName.LabelText = L("AddPart_Name", "Name");
            if (txtDescription != null) txtDescription.LabelText = L("AddPart_Description", "Description");
            if (txtSku != null) txtSku.LabelText = L("AddPart_SKU", "SKU");
            if (btnAutoSku != null) btnAutoSku.Text = L("AddPart_Auto", "Auto");
            if (txtBarcode != null) txtBarcode.LabelText = L("AddPart_Barcode", "Barcode");
            if (btnScanBarcode != null) btnScanBarcode.Text = L("AddPart_Scan", "Scan");
            if (txtBatch != null) txtBatch.LabelText = L("AddPart_BatchNo", "Batch No.");
            if (txtLocation != null) txtLocation.LabelText = L("AddPart_Location", "Location");
            if (txtShelf != null) txtShelf.LabelText = L("AddPart_Shelf", "Shelf");
            if (cmbUom != null) cmbUom.LabelText = L("AddPart_UnitOfMeasure", "Unit of Measure");

            if (gbStock != null) gbStock.Text = L("AddPart_StockControl", "Stock control");
            if (chkTrackStock != null) chkTrackStock.Text = L("AddPart_ControlThisItem", "Control this item");
            if (numStock != null) numStock.LabelText = L("AddPart_Stock", "Stock");
            if (numLowLevel != null) numLowLevel.LabelText = L("AddPart_LowLevel", "Low level");

            if (gbSupp != null) gbSupp.Text = L("AddPart_SupplierCost", "Supplier Cost");
            if (cmbSupplier != null) cmbSupplier.LabelText = L("AddPart_Supplier", "Supplier");
            if (numCost != null) numCost.LabelText = L("AddPart_Cost", "Cost");

            if (gbPrices != null) gbPrices.Text = L("AddPart_Prices", "Prices");
            if (lblLevel != null) lblLevel.Text = L("AddPart_Level", "Level");
            if (lblPrice != null) lblPrice.Text = L("AddPart_Price", "Price");
            if (lblGross != null) lblGross.Text = L("AddPart_Gross", "Gross %");
            if (lblProfit != null) lblProfit.Text = L("AddPart_Profit", "Profit");
            
            for (int i = 0; i < 4; i++)
            {
                if (lblPriceRows[i] != null)
                {
                    lblPriceRows[i].Text = L("AddPart_Price" + (i + 1), "Price " + (i + 1));
                }
            }

            if (gbScale != null) gbScale.Text = L("Scale_Title", "TM-A17 Weighing Scale & Price Calculator");
            if (btnConfigScale != null) btnConfigScale.Text = L("Scale_SettingsBtn", "Scale Settings");
            if (btnReadScale != null) btnReadScale.Text = L("Scale_ReadWeight", "Read Weight");
            if (btnApplyScalePrice != null) btnApplyScalePrice.Text = L("Scale_ApplyPrice1", "Apply to Price 1");
            if (btnApplyScaleWeight != null) btnApplyScaleWeight.Text = L("Scale_ApplyStock", "Apply Weight to Stock");
            if (lblScaleStatus != null)
            {
                lblScaleStatus.RightToLeft = RightToLeft.No;
                lblScaleStatus.Text = ScaleService.Instance.IsConnected
                    ? L("Scale_StatusConnected", "Status: Connected")
                    : L("Scale_StatusDisconnected", "Status: Disconnected");
            }

            SetFooterButtons(
                _editPartId.HasValue ? L("AddPart_UpdateBtn", "Update") : L("AddPart_Save", "Save"),
                L("Popup_Cancel", "Cancel"),
                btnSave_Click,
                btnCancel_Click
            );

            UpdateScaleUnitDisplay();
        }

        private void InitializeUI()
        {
            this.ContentPanel.AutoScroll = true;
            TableLayoutPanel tlpMain = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 4, Padding = new Padding(15, 15, 15, 15), AutoSize = true };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // -- Left Pane --
            FlowLayoutPanel flpLeft = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Margin = new Padding(0,0,10,0), AutoSize = true };
            
            // Image
            Panel pnlLeft = new Panel { Width = 280, Height = 280, Margin = new Padding(0,0,0,15) };
            pbImage = new PictureBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = ThemeConfig.BackgroundColor };
            btnUpload = new ModernButton { Text = "Upload Image", Dock = DockStyle.Bottom, Height = 35, Margin = new Padding(0,10,0,0) };
            btnUpload.Click += btnUpload_Click;
            ThemeConfig.ApplyPrimaryButton(btnUpload);
            pnlLeft.Controls.Add(pbImage);
            pnlLeft.Controls.Add(btnUpload);
            flpLeft.Controls.Add(pnlLeft);

            // Type
            gbType = new GroupBox { Text = "Type", Width = 280, Height = 60, Margin = new Padding(0,0,0,15) };
            rbProduct = new RadioButton { Text = "Product", Checked = true, Location = new Point(15, 25), AutoSize = true };
            rbService = new RadioButton { Text = "Service", Location = new Point(120, 25), AutoSize = true };
            gbType.Controls.Add(rbProduct); gbType.Controls.Add(rbService);
            flpLeft.Controls.Add(gbType);
 
            // Settings
            gbSettings = new GroupBox { Text = "Settings", Width = 280, Height = 100, Margin = new Padding(0,0,0,15) };
            chkSales = new CheckBox { Text = "Sales item", Checked = true, Location = new Point(15, 25), AutoSize = true };
            chkPurchase = new CheckBox { Text = "Purchase item", Location = new Point(15, 50), AutoSize = true };
            chkInactive = new CheckBox { Text = "Inactive", Location = new Point(15, 75), AutoSize = true };
            gbSettings.Controls.AddRange(new Control[] { chkSales, chkPurchase, chkInactive });
            flpLeft.Controls.Add(gbSettings);

            // Tax
            cmbTaxRate = new ModernComboBox { LabelText = "Tax Rates:", Width = 280, Margin = new Padding(0,0,0,15), DropDownStyle = ComboBoxStyle.DropDownList };
            flpLeft.Controls.Add(cmbTaxRate);

            // Category
            cmbCategory = new ModernComboBox { LabelText = "Category", Width = 280, Margin = new Padding(0,0,0,10), DropDownStyle = ComboBoxStyle.DropDownList };
            flpLeft.Controls.Add(cmbCategory);

            // Expiry Date
            FlowLayoutPanel flpExp = new FlowLayoutPanel { Width = 280, Height = 67, Margin = new Padding(0,0,0,10), FlowDirection = FlowDirection.TopDown };
            lblExp = new Label { Text = "Expiry Date", AutoSize = true, Font = ThemeConfig.SmallBoldFont ?? new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeConfig.TextColorDark };
            dtpExpiry = new FlatDateTimePicker { Width = 270, Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
            flpExp.Controls.Add(lblExp); flpExp.Controls.Add(dtpExpiry);
            flpLeft.Controls.Add(flpExp);

            tlpMain.Controls.Add(flpLeft, 0, 0);
            tlpMain.SetRowSpan(flpLeft, 3);

            // -- Right Pane (General Info) --
            FlowLayoutPanel flpMiddle = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Margin = new Padding(10,0,0,0), AutoSize = true };
            int fullW = 690; // 690 fits Name/Desc perfectly inside 720
            int halfW = 340; // 340 * 2 + 10 margin = 690
            
            txtName = new ModernTextBox { LabelText = "Name", Width = fullW, Margin = new Padding(0,0,20,10) };
            txtDescription = new ModernTextBox { LabelText = "Description", Width = fullW, Margin = new Padding(0,0,20,10) };
            
            FlowLayoutPanel flpSku = new FlowLayoutPanel { Width = halfW, Height = 67, Margin = new Padding(0,0,10,10), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            txtSku = new ModernTextBox { LabelText = "SKU", Width = halfW - 75, Margin = new Padding(0) };
            btnAutoSku = new ModernButton { Text = "Auto", Width = 65, Height = 35, Margin = new Padding(10,25,0,0) };
            ThemeConfig.ApplyPrimaryButton(btnAutoSku);
            flpSku.Controls.AddRange(new Control[] { txtSku, btnAutoSku });
            btnAutoSku.Click += (s, e) => 
            { 
                string cat = cmbCategory.Text.Trim();
                if (string.IsNullOrEmpty(cat)) cat = "GEN";
                string name = txtName.Text.Trim();
                if (string.IsNullOrEmpty(name)) name = "PRD";
                
                string catPrefix = cat.Length >= 3 ? cat.Substring(0, 3).ToUpper() : cat.ToUpper().PadRight(3, 'X');
                string namePrefix = name.Length >= 3 ? name.Substring(0, 3).ToUpper() : name.ToUpper().PadRight(3, 'X');
                
                txtSku.Text = $"{catPrefix}-{namePrefix}-{DateTime.Now.ToString("yyMMddHHmm")}"; 
            };

            FlowLayoutPanel flpBarcode = new FlowLayoutPanel { Width = halfW, Height = 67, Margin = new Padding(0,0,10,10), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            txtBarcode = new ModernTextBox { LabelText = "Barcode", Width = halfW - 75, Margin = new Padding(0) };
            btnScanBarcode = new ModernButton { Text = "Scan", Width = 65, Height = 35, Margin = new Padding(10,25,0,0) };
            ThemeConfig.ApplyPrimaryButton(btnScanBarcode);
            flpBarcode.Controls.AddRange(new Control[] { txtBarcode, btnScanBarcode });
            btnScanBarcode.Click += (s, e) => {
                string barcode = txtBarcode.Text.Trim();
                if (ScaleBarcodeParser.IsScaleBarcode(barcode)) {
                    var parsed = ScaleBarcodeParser.Parse(barcode);
                    if (parsed.IsSuccess) {
                        if (parsed.BarcodeType == ScaleBarcodeType.WeightBased) {
                            _currentScaleWeight = parsed.WeightKg;
                            UpdateScaleUnitDisplay();
                            MessageHelper.ShowInfo(string.Format(
                                LocalizationManager.GetString("Scale_BarcodeWeightMsg",
                                    "TM-A17 Scale Barcode Detected!{0}PLU: {1}{0}Weight: {2:N3} kg{0}{3}"),
                                Environment.NewLine, parsed.ProductCode, parsed.WeightKg, lblCalculatedPrice.Text));
                        } else if (parsed.BarcodeType == ScaleBarcodeType.PriceBased) {
                            numPrices[0].Value = parsed.TotalPrice;
                            CalculateMargins(null, null);
                            MessageHelper.ShowInfo(string.Format(
                                LocalizationManager.GetString("Scale_BarcodePriceMsg",
                                    "TM-A17 Scale Barcode Detected!{0}PLU: {1}{0}Total Price: {2}"),
                                Environment.NewLine, parsed.ProductCode, CurrencyService.Format(parsed.TotalPrice)));
                        }
                        if (string.IsNullOrWhiteSpace(txtSku.Text)) txtSku.Text = "PLU-" + parsed.ProductCode;
                    }
                } else {
                    MessageHelper.ShowInfo("Ready to scan...");
                    txtBarcode.Focus();
                }
            };
            
            txtBatch = new ModernTextBox { LabelText = "Batch No.", Width = halfW, Margin = new Padding(0,0,10,10) };
            txtLocation = new ModernTextBox { LabelText = "Location", Width = halfW, Margin = new Padding(0,0,10,10) };
            txtShelf = new ModernTextBox { LabelText = "Shelf", Width = halfW, Margin = new Padding(0,0,10,10) };
            cmbUom = new ModernComboBox { LabelText = "Unit of Measure", Width = halfW, Margin = new Padding(0,0,10,10) };
            
            flpMiddle.Controls.AddRange(new Control[] { txtName, txtDescription, flpSku, flpBarcode, txtBatch, txtLocation, txtShelf, cmbUom });
            tlpMain.Controls.Add(flpMiddle, 1, 0);

            // -- Right Pane Row 1: Stock & Supplier --
            FlowLayoutPanel flpStockSupp = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Margin = new Padding(10,0,0,0), AutoSize = true };
            
            gbStock = new GroupBox { Text = "Stock control", Width = halfW, Height = 120, Margin = new Padding(0,0,10,10) };
            chkTrackStock = new CheckBox { Text = "Control this item", Checked = true, Location = new Point(10, 20), AutoSize = true };
            numStock = new ModernNumericUpDown { LabelText = "Stock", Width = (halfW / 2) - 15, Location = new Point(10, 45) };
            numLowLevel = new ModernNumericUpDown { LabelText = "Low level", Width = (halfW / 2) - 15, Location = new Point((halfW / 2) + 5, 45) };
            gbStock.Controls.AddRange(new Control[] { chkTrackStock, numStock, numLowLevel });
            
            gbSupp = new GroupBox { Text = "Supplier Cost", Width = halfW, Height = 120, Margin = new Padding(0,0,10,10) };
            cmbSupplier = new ModernComboBox { LabelText = "Supplier", Width = (halfW / 2) - 15, Location = new Point(10, 45), DropDownStyle = ComboBoxStyle.DropDownList };
            numCost = new ModernNumericUpDown { LabelText = "Cost", Width = (halfW / 2) - 15, Location = new Point((halfW / 2) + 5, 45), DecimalPlaces = 2, Maximum = 1000000 };
            gbSupp.Controls.AddRange(new Control[] { cmbSupplier, numCost });
 
            flpStockSupp.Controls.AddRange(new Control[] { gbStock, gbSupp });
            tlpMain.Controls.Add(flpStockSupp, 1, 1);

            // -- Prices Grid --
            gbPrices = new GroupBox { Text = "Prices", Dock = DockStyle.Top, Margin = new Padding(10,0,20,20), AutoSize = true };
            TableLayoutPanel tlpPrices = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 4, RowCount = 5, Padding = new Padding(10), AutoSize = true };
            tlpPrices.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpPrices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlpPrices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlpPrices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            
            lblLevel = new Label { Text = "Level", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            lblPrice = new Label { Text = "Price", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            lblGross = new Label { Text = "Gross %", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            lblProfit = new Label { Text = "Profit", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            
            tlpPrices.Controls.Add(lblLevel, 0, 0);
            tlpPrices.Controls.Add(lblPrice, 1, 0);
            tlpPrices.Controls.Add(lblGross, 2, 0);
            tlpPrices.Controls.Add(lblProfit, 3, 0);
 
            for (int i=0; i<4; i++) {
                lblPriceRows[i] = new Label { Text = "Price " + (i+1), Anchor = AnchorStyles.Left };
                tlpPrices.Controls.Add(lblPriceRows[i], 0, i+1);
                numPrices[i] = new ModernNumericUpDown { Width = 150, DecimalPlaces = 2, Maximum = 1000000, Margin = new Padding(2), Dock=DockStyle.Fill, ShowLabel=false };
                txtGrosses[i] = new ModernTextBox { Width = 150, ReadOnly = true, ShowLabel = false, Margin = new Padding(2), Dock=DockStyle.Fill };
                txtProfits[i] = new ModernTextBox { Width = 150, ReadOnly = true, ShowLabel = false, Margin = new Padding(2), Dock=DockStyle.Fill };
                
                tlpPrices.Controls.Add(numPrices[i], 1, i+1);
                tlpPrices.Controls.Add(txtGrosses[i], 2, i+1);
                tlpPrices.Controls.Add(txtProfits[i], 3, i+1);
            }
            gbPrices.Controls.Add(tlpPrices);
            tlpMain.Controls.Add(gbPrices, 1, 2);

            // -- Scale Integration Section --
            gbScale = new GroupBox { Name = "gbScale", Text = "TM-A17 Weighing Scale & Price Calculator", Dock = DockStyle.Top, Height = 155, Margin = new Padding(10, 5, 20, 25), Padding = new Padding(10, 20, 10, 10) };

            FlowLayoutPanel flpScaleMain = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = false };
            FlowLayoutPanel flpRow1 = new FlowLayoutPanel { Width = 660, Height = 55, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Margin = new Padding(0, 0, 0, 5) };
            FlowLayoutPanel flpRow2 = new FlowLayoutPanel { Width = 660, Height = 55, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Margin = new Padding(0) };

            lblScaleStatus = new Label {
                Name = "lblScaleStatus",
                Text = ScaleService.Instance.IsConnected
                    ? LocalizationManager.GetString("Scale_StatusConnected", "Status: Connected")
                    : LocalizationManager.GetString("Scale_StatusDisconnected", "Status: Disconnected"),
                ForeColor = ScaleService.Instance.IsConnected ? Color.Green : Color.Red,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 14, 15, 0),
                RightToLeft = RightToLeft.No
            };

            btnConfigScale = new ModernButton { Name = "btnConfigScale", Text = "Scale Settings", Width = 110, Height = 35, Margin = new Padding(0, 5, 10, 0) };
            ThemeConfig.ApplySecondaryButton(btnConfigScale);
            btnConfigScale.Click += (s, e) => {
                if (new ScaleSettingsForm().ShowDialog() == DialogResult.OK)
                {
                    _currentScaleUnit = ScaleService.Instance.Config.DefaultUnit;
                    UpdateScaleUnitDisplay();
                }
            };

            btnReadScale = new ModernButton { Name = "btnReadScale", Text = "Read Weight", Width = 100, Height = 35, Margin = new Padding(0, 5, 15, 0) };
            ThemeConfig.ApplySecondaryButton(btnReadScale);
            btnReadScale.Click += (s, e) => {
                string unit = !string.IsNullOrWhiteSpace(_currentScaleUnit) ? _currentScaleUnit : (string.IsNullOrWhiteSpace(cmbUom.Text) ? "kg" : cmbUom.Text.Trim());
                if (ScaleService.Instance.IsConnected) ScaleService.Instance.RequestWeight();
                else ScaleService.Instance.SimulateWeight(0.500m, unit);
            };

            lblUnitPriceTitle = new Label {
                Name = "lblUnitPriceTitle",
                Text = string.Format(
                    LocalizationManager.GetString("Scale_UnitPrice", "Unit Price (/{0}):"),
                    ScaleService.Instance.Config.DefaultUnit),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                AutoSize = true,
                Margin = new Padding(0, 14, 5, 0),
                RightToLeft = RightToLeft.No
            };

            numUnitPricePerKg = new ModernNumericUpDown { ShowLabel = false, Width = 120, DecimalPlaces = 2, Maximum = 1000000, Margin = new Padding(0, 5, 10, 0) };
            numUnitPricePerKg.ValueChanged += (s, e) => CalculateScalePrice();

            flpRow1.Controls.AddRange(new Control[] { lblScaleStatus, btnConfigScale, btnReadScale, lblUnitPriceTitle, numUnitPricePerKg });

            lblWeightReadout = new Label {
                Name = "lblWeightReadout",
                Text = string.Format(
                    LocalizationManager.GetString("Scale_Weight", "Weight: {0} {1}"),
                    "0.000", "kg"),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                AutoSize = true,
                Margin = new Padding(0, 10, 15, 0),
                RightToLeft = RightToLeft.No
            };

            lblCalculatedPrice = new Label {
                Name = "lblCalculatedPrice",
                Text = string.Format(
                    LocalizationManager.GetString("Scale_CalcPrice", "Calc Price: {0}"),
                    CurrencyService.Format(0)),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                AutoSize = true,
                Margin = new Padding(0, 10, 20, 0),
                RightToLeft = RightToLeft.No
            };

            btnApplyScalePrice = new ModernButton { Name = "btnApplyScalePrice", Text = "Apply to Price 1", Width = 130, Height = 35, Margin = new Padding(0, 2, 10, 0) };
            ThemeConfig.ApplyPrimaryButton(btnApplyScalePrice);
            btnApplyScalePrice.Click += (s, e) => {
                if (lblCalculatedPrice.Tag != null && decimal.TryParse(lblCalculatedPrice.Tag.ToString(), out decimal calcP)) {
                    numPrices[0].Value = calcP;
                    CalculateMargins(null, null);
                }
            };

            btnApplyScaleWeight = new ModernButton { Name = "btnApplyScaleWeight", Text = "Apply Weight to Stock", Width = 150, Height = 35, Margin = new Padding(0, 2, 0, 0) };
            ThemeConfig.ApplySecondaryButton(btnApplyScaleWeight);
            btnApplyScaleWeight.Click += (s, e) => {
                numStock.Value = (int)Math.Max(1, Math.Round(_currentScaleWeight));
            };

            flpRow2.Controls.AddRange(new Control[] { lblWeightReadout, lblCalculatedPrice, btnApplyScalePrice, btnApplyScaleWeight });

            flpScaleMain.Controls.Add(flpRow1);
            flpScaleMain.Controls.Add(flpRow2);
            gbScale.Controls.Add(flpScaleMain);

            tlpMain.Controls.Add(gbScale, 1, 3);
            tlpMain.SetRowSpan(flpLeft, 4);

            this.ContentPanel.Controls.Add(tlpMain);
            UpdateScaleUnitDisplay();
        }

        private void Scale_WeightReceived(decimal weight, string unit, bool isStable)
        {
            if (this.InvokeRequired) {
                this.BeginInvoke(new Action(() => Scale_WeightReceived(weight, unit, isStable)));
                return;
            }
            _currentScaleWeight = weight;
            if (!string.IsNullOrWhiteSpace(unit)) _currentScaleUnit = unit;
            UpdateScaleUnitDisplay(isStable);
        }

        private void Scale_StatusChanged(bool isConnected, string statusText)
        {
            if (this.InvokeRequired) {
                this.BeginInvoke(new Action(() => Scale_StatusChanged(isConnected, statusText)));
                return;
            }
            lblScaleStatus.Text = isConnected
                ? LocalizationManager.GetString("Scale_StatusConnected", "Status: Connected")
                : LocalizationManager.GetString("Scale_StatusDisconnected", "Status: Disconnected");
            lblScaleStatus.ForeColor = isConnected ? Color.Green : Color.Red;
            UpdateScaleUnitDisplay();
        }

        private void UpdateScaleUnitDisplay(bool isStable = true)
        {
            string unit = !string.IsNullOrWhiteSpace(_currentScaleUnit) 
                ? _currentScaleUnit 
                : ScaleService.Instance.Config.DefaultUnit;
            if (string.IsNullOrWhiteSpace(unit)) unit = "kg";

            if (lblUnitPriceTitle != null)
            {
                lblUnitPriceTitle.RightToLeft = RightToLeft.No;
                lblUnitPriceTitle.Text = string.Format(
                    LocalizationManager.GetString("Scale_UnitPrice", "Unit Price (/{0}):"), unit);
            }
            if (lblWeightReadout != null)
            {
                lblWeightReadout.RightToLeft = RightToLeft.No;
                string weightKey = isStable ? "Scale_Weight" : "Scale_WeightMoving";
                string weightFallback = isStable ? "Weight: {0} {1}" : "Weight: {0} {1} (Moving)";
                lblWeightReadout.Text = string.Format(
                    LocalizationManager.GetString(weightKey, weightFallback),
                    _currentScaleWeight.ToString("N3"), unit);
            }
            
            // Sync unit in UOM dropdown if empty
            if (cmbUom != null && string.IsNullOrWhiteSpace(cmbUom.Text)) cmbUom.Text = unit;
            
            CalculateScalePrice();
        }

        private void CalculateScalePrice()
        {
            decimal unitPrice = numUnitPricePerKg.Value;
            decimal calcPrice = Math.Round(_currentScaleWeight * unitPrice, 2);
            if (lblCalculatedPrice != null)
            {
                lblCalculatedPrice.RightToLeft = RightToLeft.No;
                lblCalculatedPrice.Text = string.Format(
                    LocalizationManager.GetString("Scale_CalcPrice", "Calc Price: {0}"),
                    CurrencyService.Format(calcPrice));
                lblCalculatedPrice.Tag = calcPrice;
            }
        }

        private void LoadDropdowns()
        {
            try {
                // Categories
                var cats = CategoryData.GetAllCategories();
                cmbCategory.DisplayMember = "CategoryName"; cmbCategory.ValueMember = "CategoryName";
                cmbCategory.DataSource = cats;

                // Suppliers
                var sups = DatabaseHelper.ExecuteDataTable("SELECT id, supplier_name FROM suppliers WHERE date_deleted IS NULL");
                var dtSup = new System.Data.DataTable();
                dtSup.Columns.Add("id", typeof(int)); dtSup.Columns.Add("name", typeof(string));
                dtSup.Rows.Add(-1, "N/A");
                foreach(System.Data.DataRow r in sups.Rows) dtSup.Rows.Add(r["id"], r["supplier_name"]);
                cmbSupplier.DisplayMember = "name"; cmbSupplier.ValueMember = "id";
                cmbSupplier.DataSource = dtSup;

                // Taxes
                ReloadTaxRates();

                // Units of Measure
                cmbUom.Items.Clear();
                cmbUom.Items.AddRange(new object[] { "g", "kg", "pcs", "pack", "box", "meter", "liter" });
            } catch (Exception ex) { MessageHelper.ShowError("Error loading data: " + ex.Message); }
        }

        private void ReloadTaxRates()
        {
            if (cmbTaxRate == null) return;

            decimal selected = 0m;
            if (cmbTaxRate.SelectedItem != null)
            {
                try { selected = (decimal)((dynamic)cmbTaxRate.SelectedItem).Value; }
                catch { selected = 0m; }
            }

            cmbTaxRate.Items.Clear();
            cmbTaxRate.Items.Add(new
            {
                Text = LocalizationManager.GetString("AddPart_TaxRateNA", "Rate 1 N/A (0%)"),
                Value = 0m
            });
            cmbTaxRate.Items.Add(new
            {
                Text = LocalizationManager.GetString("AddPart_TaxRateStandard", "Standard (15%)"),
                Value = 15m
            });
            cmbTaxRate.Items.Add(new
            {
                Text = LocalizationManager.GetString("AddPart_TaxRateReduced", "Reduced (5%)"),
                Value = 5m
            });
            cmbTaxRate.DisplayMember = "Text";
            cmbTaxRate.ValueMember = "Value";

            int idx = 0;
            for (int i = 0; i < cmbTaxRate.Items.Count; i++)
            {
                dynamic item = cmbTaxRate.Items[i];
                if (item.Value == selected) { idx = i; break; }
            }
            cmbTaxRate.SelectedIndex = idx;
        }

        public void LoadPartData(PartData part)
        {
            if (part == null) return;
            _editPartId = part.Id;
            this.TitleText = LocalizationManager.GetString("AddPart_TitleEdit", "Edit Product / Service");
            
            txtName.Text = part.PartName;
            txtDescription.Text = part.Description;
            txtSku.Text = part.PartNumber;
            txtBarcode.Text = part.Barcode;
            cmbCategory.Text = part.CategoryName;
            cmbUom.Text = part.UnitOfMeasure;
            txtBatch.Text = part.BatchNumber;
            txtLocation.Text = part.Location;
            txtShelf.Text = part.Shelf;
            if (!string.IsNullOrEmpty(part.ExpiryDate) && DateTime.TryParse(part.ExpiryDate, out DateTime exp)) {
                dtpExpiry.Checked = true;
                dtpExpiry.Value = exp;
            } else { dtpExpiry.Checked = false; }

            if (part.ItemType == "Service") rbService.Checked = true; else rbProduct.Checked = true;
            chkSales.Checked = part.IsSalesItem;
            chkPurchase.Checked = part.IsPurchaseItem;
            chkInactive.Checked = part.IsInactive;

            for(int i=0; i<cmbTaxRate.Items.Count; i++) {
                dynamic item = cmbTaxRate.Items[i];
                if (item.Value == part.TaxRate) { cmbTaxRate.SelectedIndex = i; break; }
            }

            chkTrackStock.Checked = part.IsStockTracked;
            numStock.Value = part.QuantityInStock;
            numLowLevel.Value = part.MinimumStockLevel;

            if (part.SupplierId.HasValue) cmbSupplier.SelectedValue = part.SupplierId.Value;
            numCost.Value = part.PurchasePrice;

            numPrices[0].Value = part.SellingPrice;
            numPrices[1].Value = part.Price2;
            numPrices[2].Value = part.Price3;
            numPrices[3].Value = part.Price4;

            _currentImagePath = part.PartImage;
            UpdateImagePreview();
            CalculateMargins(null, null);
        }

        private void CalculateMargins(object sender, EventArgs e)
        {
            decimal cost = numCost.Value;
            for(int i=0; i<4; i++) {
                decimal price = numPrices[i].Value;
                decimal profit = price - cost;
                decimal gross = price > 0 ? (profit / price) * 100 : 0;
                
                txtProfits[i].Text = profit.ToString("N2");
                txtGrosses[i].Text = gross.ToString("N1") + "%";
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp" };
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                try {
                    string assetsDir = System.IO.Path.Combine(Application.StartupPath, "Assets", "Products");
                    if(!System.IO.Directory.Exists(assetsDir)) System.IO.Directory.CreateDirectory(assetsDir);
                    string newName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(ofd.FileName);
                    string destPath = System.IO.Path.Combine(assetsDir, newName);
                    System.IO.File.Copy(ofd.FileName, destPath, true);
                    _currentImagePath = "Assets/Products/" + newName;
                    UpdateImagePreview();
                } catch(Exception ex) { MessageHelper.ShowError("Error uploading: " + ex.Message); }
            }
        }

        private void UpdateImagePreview()
        {
            if (string.IsNullOrEmpty(_currentImagePath)) { pbImage.Image = null; return; }
            try {
                string fullPath = System.IO.Path.Combine(Application.StartupPath, _currentImagePath);
                if (System.IO.File.Exists(fullPath)) {
                    using (var ms = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(fullPath))) {
                        var old = pbImage.Image; pbImage.Image = null; if (old != null) old.Dispose();
                        pbImage.Image = System.Drawing.Image.FromStream(ms);
                    }
                }
            } catch { pbImage.Image = null; }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageHelper.ShowWarning("Please enter a name."); return; }

            PartData p = new PartData {
                Id = _editPartId ?? 0,
                PartName = txtName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Barcode = txtBarcode.Text.Trim(),
                PartNumber = txtSku.Text.Trim(),
                CategoryName = cmbCategory.Text,
                UnitOfMeasure = cmbUom.Text.Trim(),
                BatchNumber = txtBatch.Text.Trim(),
                Location = txtLocation.Text.Trim(),
                Shelf = txtShelf.Text.Trim(),
                ExpiryDate = dtpExpiry.Checked ? dtpExpiry.Value.Value.ToString("yyyy-MM-dd") : "",
                ItemType = rbService.Checked ? "Service" : "Product",
                IsSalesItem = chkSales.Checked,
                IsPurchaseItem = chkPurchase.Checked,
                IsInactive = chkInactive.Checked,
                TaxRate = (decimal)((dynamic)cmbTaxRate.SelectedItem).Value,
                IsStockTracked = chkTrackStock.Checked,
                QuantityInStock = (int)numStock.Value,
                MinimumStockLevel = (int)numLowLevel.Value,
                PurchasePrice = numCost.Value,
                SellingPrice = numPrices[0].Value,
                Price2 = numPrices[1].Value,
                Price3 = numPrices[2].Value,
                Price4 = numPrices[3].Value,
                PartImage = _currentImagePath,
                Status = chkInactive.Checked ? "Inactive" : "Active"
            };

            if (cmbSupplier.SelectedValue != null && (int)cmbSupplier.SelectedValue != -1)
                p.SupplierId = (int)cmbSupplier.SelectedValue;

            try {
                new InventoryService().SaveProductService(p);
                this.DialogResult = DialogResult.OK;
                this.Close();
            } catch (Exception ex) { MessageHelper.ShowError(ex.Message); }
        }

        private void btnCancel_Click(object sender, EventArgs e) { this.DialogResult = DialogResult.Cancel; this.Close(); }
    }
}
