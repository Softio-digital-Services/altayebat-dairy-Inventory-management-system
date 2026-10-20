using System;
using System.Windows.Forms;
using System.Drawing;
using InventorySystem.Data;
using InventorySystem.Controls;
using InventorySystem.Helpers;

namespace InventorySystem.Forms
{
    public partial class AddCustomerForm : BaseModalForm
    {
        private ModernTextBox txtName;
        private ModernTextBox txtFirstName;
        private ModernTextBox txtLastName;
        private ModernTextBox txtPhone;
        private ModernTextBox txtEmail;
        private ModernTextBox txtAddress;
        private ModernTextBox txtCreditLimit;
        private FlatDateTimePicker dtDueDate;
        private ModernNumericUpDown numReminderDays;
        private CheckBox chkEnableReminder;
        private Label lblDueDate;
        
        public string CustomerName => rdoCompany.Checked ? txtName.Text.Trim() : $"{txtFirstName.Text.Trim()} {txtLastName.Text.Trim()}".Trim();
        public string Phone => txtPhone.Text.Trim();
        public string Email => txtEmail.Text.Trim();
        public string Address => txtAddress.Text.Trim();
        public decimal CreditLimit => decimal.TryParse(txtCreditLimit.Text, out decimal val) ? val : 0;
        public DateTime? DueDate => chkEnableReminder.Checked ? dtDueDate.Value : (DateTime?)null;
        public int ReminderDays => (int)numReminderDays.Value;
        public string CustomerType => rdoIndividual.Checked ? "Individual" : "Company";

        private RadioButton rdoCompany;
        private RadioButton rdoIndividual;

        public AddCustomerForm()
        {
            InitializeComponent();
            SetFooterButtons(
                InventorySystem.Helpers.LocalizationManager.GetString("AddCust_Save"),
                InventorySystem.Helpers.LocalizationManager.GetString("Popup_Cancel"),
                btnSave_Click,
                btnCancel_Click
            );
            
            ApplyTheme();
            InventorySystem.Helpers.LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (rdoCompany.Checked)
            {
                if (!ValidationHelper.ValidateRequiredFields(txtName, txtPhone)) return;
            }
            else
            {
                if (!ValidationHelper.ValidateRequiredFields(txtFirstName, txtLastName, txtPhone)) return;
            }

            if (!ValidationHelper.ValidatePhoneNumber(txtPhone.Text)) return;
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !ValidationHelper.ValidateEmail(txtEmail.Text)) return;
            
            if (!string.IsNullOrWhiteSpace(txtCreditLimit.Text))
            {
                if (!ValidationHelper.ValidateDecimal(txtCreditLimit.Text, LocalizationManager.GetString("AddCust_CreditLimit", "Credit Limit"), out _))
                    return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void ApplyLocalization()
        {
            InventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            Func<string, string> L = InventorySystem.Helpers.LocalizationManager.GetString;

            bool isEdit = this.TitleText != null && (this.TitleText.Contains("Edit") || this.TitleText.Contains(L("AddCust_TitleEdit")));
            this.TitleText = isEdit ? L("AddCust_TitleEdit") : L("AddCust_TitleNew");
            
            var lblSection = this.Controls.Find("lblSection", true);
            if(lblSection.Length > 0) lblSection[0].Text = L("AddCust_Section");

            if(txtName != null) txtName.LabelText = L("Add_CompanyName");
            if(txtFirstName != null) txtFirstName.LabelText = L("Add_FirstName");
            if(txtLastName != null) txtLastName.LabelText = L("Add_LastName");
            if(txtPhone != null) txtPhone.LabelText = L("Popup_Phone");
            if(txtEmail != null) txtEmail.LabelText = L("AddCust_Email");
            if(txtCreditLimit != null) txtCreditLimit.LabelText = LocalizationManager.GetString("AddCust_CreditLimit", "Credit Limit");
            if(txtAddress != null) txtAddress.LabelText = L("Popup_Address");
            
            if (lblDueDate != null) lblDueDate.Text = LocalizationManager.GetString("AddCust_DueDate", "Payment Due Date");
            if (numReminderDays != null) numReminderDays.LabelText = LocalizationManager.GetString("AddCust_ReminderDays", "Reminder (Days Before)");
            
            if(chkEnableReminder != null) chkEnableReminder.Text = LocalizationManager.GetString("AddCust_EnableReminder", "Enable Reminder");

            var lblType = this.Controls.Find("lblType", true);
            if(lblType.Length > 0) lblType[0].Text = L("AddCust_Type");

            if(rdoCompany != null) rdoCompany.Text = L("Popup_Company");
            if(rdoIndividual != null) rdoIndividual.Text = L("Popup_Individual");
            
            UpdateValidationUI();

            SetFooterButtons(
                isEdit ? L("AddCust_UpdateBtn") : L("AddCust_Save"),
                L("Popup_Cancel"),
                btnSave_Click,
                btnCancel_Click
            );
        }

        // Edit Mode Constructor
        public AddCustomerForm(int id, string name, string phone, string email, string address, string type, decimal creditLimit = 0, DateTime? dueDate = null, int reminderDays = 0) : this()
        {
            this.TitleText = InventorySystem.Helpers.LocalizationManager.GetString("AddCust_TitleEdit");
            SetFooterButtons(
                InventorySystem.Helpers.LocalizationManager.GetString("AddCust_UpdateBtn"),
                InventorySystem.Helpers.LocalizationManager.GetString("Popup_Cancel"),
                btnSave_Click,
                btnCancel_Click
            );
            
            // Split name
            if (type == "Company")
            {
                rdoCompany.Checked = true;
                txtName.Text = name;
            }
            else
            {
                rdoIndividual.Checked = true;
                if (!string.IsNullOrEmpty(name))
                {
                    string[] parts = name.Split(new char[] { ' ' }, 2);
                    if (parts.Length > 0) txtFirstName.Text = parts[0];
                    if (parts.Length > 1) txtLastName.Text = parts[1];
                }
            }
            // Combined constructor logic
            txtPhone.Text = phone;
            txtEmail.Text = email;
            txtCreditLimit.Text = creditLimit.ToString("F2");
            txtAddress.Text = address;
            
            if (dueDate.HasValue)
            {
                chkEnableReminder.Checked = true;
                dtDueDate.Value = dueDate.Value;
            }
            numReminderDays.Value = reminderDays;
            
            UpdateValidationUI();
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(550, 900);

            TableLayoutPanel tlpMain = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, RowCount = 10, AutoSize = true, Padding = new Padding(20) };
            for(int i=0; i<10; i++) tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Section Title
            Label lblSection = new Label { Name = "lblSection", Text = "Customer Information", Font = ThemeConfig.SubHeaderFont, AutoSize = true, ForeColor = ThemeConfig.SecondaryColor, Margin = new Padding(0, 0, 0, 15) };
            tlpMain.Controls.Add(lblSection, 0, 0);

            // Type Selection
            TableLayoutPanel pnlType = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Height = 35, Margin = new Padding(0, 0, 0, 15) };
            pnlType.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pnlType.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            pnlType.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));

            Label lblType = new Label { Name = "lblType", Text = "Customer Type", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeConfig.TextColorDark, AutoSize = true, Anchor = AnchorStyles.Left };
            rdoCompany = new RadioButton { Text = "Company", Font = ThemeConfig.StandardFont, AutoSize = true, Anchor = AnchorStyles.Left };
            rdoIndividual = new RadioButton { Text = "Individual", Font = ThemeConfig.StandardFont, AutoSize = true, Anchor = AnchorStyles.Left, Checked = true };
            
            pnlType.Controls.Add(lblType, 0, 0);
            pnlType.Controls.Add(rdoCompany, 1, 0);
            pnlType.Controls.Add(rdoIndividual, 2, 0);
            tlpMain.Controls.Add(pnlType, 0, 1);

            // Company Name
            txtName = new ModernTextBox { LabelText = "Company Name", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };
            tlpMain.Controls.Add(txtName, 0, 2);

            // Names Row
            TableLayoutPanel pnlNames = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, AutoSize = true, Margin = new Padding(0) };
            pnlNames.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlNames.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            txtFirstName = new ModernTextBox { LabelText = "First Name", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 5, 0) };
            txtLastName = new ModernTextBox { LabelText = "Last Name", Dock = DockStyle.Fill, Margin = new Padding(5, 0, 0, 0) };
            pnlNames.Controls.Add(txtFirstName, 0, 0);
            pnlNames.Controls.Add(txtLastName, 1, 0);
            tlpMain.Controls.Add(pnlNames, 0, 3);

            // Phone & Email
            txtPhone = new ModernTextBox { LabelText = "Phone Number", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };
            tlpMain.Controls.Add(txtPhone, 0, 4);

            txtEmail = new ModernTextBox { LabelText = "Email Address", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };
            tlpMain.Controls.Add(txtEmail, 0, 5);

            // Credit Limit
            txtCreditLimit = new ModernTextBox { LabelText = "Credit Limit", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 15), Text = "1000.00" };
            tlpMain.Controls.Add(txtCreditLimit, 0, 6);

            // Reminder Card
            Panel cardReminders = new Panel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(15), Margin = new Padding(0, 5, 0, 15) };
            cardReminders.Paint += (s, e) => {
                ThemeConfig.FillRoundedBackground(e.Graphics, cardReminders.ClientRectangle, 12f, Color.FromArgb(252, 253, 255));
                ThemeConfig.DrawRoundedBorder(e.Graphics, cardReminders.ClientRectangle, 12f, ThemeConfig.BorderColor, 1f);
            };

            TableLayoutPanel tlpRemContent = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 2, AutoSize = true };
            tlpRemContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpRemContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            chkEnableReminder = new CheckBox { Text = "Enable Payment Reminder", AutoSize = true, Font = ThemeConfig.StandardFont, Margin = new Padding(0, 0, 0, 15) };
            
            lblDueDate = new Label { Name = "lblDueDate", Text = "Payment Due Date", Font = ThemeConfig.SmallBoldFont, ForeColor = ThemeConfig.TextColorDark, AutoSize = true, Margin = new Padding(0, 0, 0, 5) };
            dtDueDate = new FlatDateTimePicker { Dock = DockStyle.Fill, Enabled = false, Height = 35 };
            
            numReminderDays = new ModernNumericUpDown { LabelText = "Reminder (Days Before)", Dock = DockStyle.Fill, Minimum = 0, Maximum = 365, Enabled = false, Increment = 1 };
            
            tlpRemContent.Controls.Add(lblDueDate, 0, 0);
            tlpRemContent.Controls.Add(dtDueDate, 0, 1);
            tlpRemContent.Controls.Add(numReminderDays, 1, 0);
            tlpRemContent.SetRowSpan(numReminderDays, 2);
            
            FlowLayoutPanel flpRemWrapper = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true };
            flpRemWrapper.Controls.Add(chkEnableReminder);
            flpRemWrapper.Controls.Add(tlpRemContent);
            
            cardReminders.Controls.Add(flpRemWrapper);
            tlpMain.Controls.Add(cardReminders, 0, 7);

            chkEnableReminder.CheckedChanged += (s, e) => { dtDueDate.Enabled = numReminderDays.Enabled = chkEnableReminder.Checked; };

            // Address
            txtAddress = new ModernTextBox { LabelText = "Address", Dock = DockStyle.Fill, Multiline = true, Height = 100, Margin = new Padding(0, 0, 0, 20) };
            tlpMain.Controls.Add(txtAddress, 0, 9);

            rdoCompany.CheckedChanged += (s, e) => UpdateValidationUI();
            rdoIndividual.CheckedChanged += (s, e) => UpdateValidationUI();

            this.ContentPanel.Controls.Add(tlpMain);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void UpdateValidationUI()
        {
            Func<string, string> L = InventorySystem.Helpers.LocalizationManager.GetString;
            bool isCompany = rdoCompany.Checked;

            txtName.Visible = isCompany;
            txtName.IsRequired = isCompany;
            txtName.LabelText = L("Add_CompanyName");
            
            txtFirstName.IsRequired = !isCompany;
            txtFirstName.LabelText = isCompany ? L("Add_ContactFirstName") : L("Add_FirstName");

            txtLastName.IsRequired = !isCompany;
            txtLastName.LabelText = isCompany ? L("Add_ContactLastName") : L("Add_LastName");

            txtPhone.IsRequired = true;
            txtPhone.LabelText = L("Popup_Phone");
        }

        private void ApplyTheme()
        {
            // Background is White (BaseModalForm)
            // Footer buttons are styled automatically by SetFooterButtons
        }
    }
}
