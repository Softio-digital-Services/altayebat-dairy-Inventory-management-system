using System;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Controls;
using InventorySystem.Helpers;

namespace InventorySystem.Forms
{
    public partial class LicenseActivationForm : BaseModalForm
    {
        private ModernTextBox txtLicenseKey;
        private Label lblHardwareId;
        private Label lblStatus;
        private LinkLabel lnkCopyHardwareId;

        // private Label lblTitle;
        private Label lblSubtitle;
        private Label lblHwIdTitle;
        public bool LicenseActivated { get; private set; }

        public LicenseActivationForm()
        {
            InitializeComponent();
            // Adaptive sizing handled by BaseModalForm.OnLoad

            ApplyLocalization();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(30),
                AutoSize = true
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Header title removed as it's already in the Modal Header

            lblSubtitle = new Label
            {
                Text = LocalizationManager.GetString("Msg_LicenseSubtitle"),
                Font = ThemeConfig.StandardFont,
                ForeColor = ThemeConfig.SecondaryColor,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 20)
            };
            tlpMain.Controls.Add(lblSubtitle, 0, 1);

            // License Key Input
            txtLicenseKey = new ModernTextBox
            {
                LabelText = "License Key",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 20)
            };
            txtLicenseKey.TextChanged += TxtLicenseKey_TextChanged;
            tlpMain.Controls.Add(txtLicenseKey, 0, 2);

            // Hardware ID Section
            TableLayoutPanel tlpMachineId = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, AutoSize = true };
            lblHwIdTitle = new Label { Text = LocalizationManager.GetString("Msg_MachineIdTitle"), Font = ThemeConfig.SmallBoldFont, ForeColor = ThemeConfig.TextColorDark, AutoSize = true };
            lblHardwareId = new Label { Text = HardwareInfo.GetShortHardwareId(), Font = ThemeConfig.StandardFont, ForeColor = ThemeConfig.SecondaryColor, AutoSize = true };
            lnkCopyHardwareId = new LinkLabel { Text = LocalizationManager.GetString("Msg_CopyClipboard"), Font = ThemeConfig.StandardFont, AutoSize = true, Margin = new Padding(0, 5, 0, 0) };
            lnkCopyHardwareId.LinkClicked += (s, e) =>
            {
                Clipboard.SetText(lblHardwareId.Text);
                MessageHelper.ShowSuccess(LocalizationManager.GetString("Msg_MachineIdCopied"));
            };

            tlpMachineId.Controls.Add(lblHwIdTitle, 0, 0);
            tlpMachineId.Controls.Add(lblHardwareId, 0, 1);
            tlpMachineId.Controls.Add(lnkCopyHardwareId, 0, 2);
            tlpMain.Controls.Add(tlpMachineId, 0, 3);

            // Status Label
            lblStatus = new Label
            {
                Text = "",
                Font = ThemeConfig.StandardFont,
                ForeColor = ThemeConfig.DangerColor,
                AutoSize = true,
                Margin = new Padding(0, 20, 0, 0)
            };
            tlpMain.Controls.Add(lblStatus, 0, 4);

            this.ContentPanel.Controls.Add(tlpMain);

            ApplyLocalization(); // To set initial button text

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            this.TitleText = LocalizationManager.GetString("Msg_LicenseActivation");
            lblSubtitle.Text = LocalizationManager.GetString("Msg_LicenseSubtitle");

            txtLicenseKey.LabelText = LocalizationManager.GetString("Msg_LicenseKey");
            lblHwIdTitle.Text = LocalizationManager.GetString("Msg_MachineIdTitle");
            lnkCopyHardwareId.Text = LocalizationManager.GetString("Msg_CopyClipboard");

            SetFooterButtons(
                LocalizationManager.GetString("Btn_Activate"),
                LocalizationManager.GetString("Btn_Exit"),
                BtnActivate_Click,
                (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); },
                LocalizationManager.GetString("Btn_StartTrial"),
                BtnStartTrial_Click
            );
        }

        private void TxtLicenseKey_TextChanged(object sender, EventArgs e)
        {
            // Enable activate button if key is provided (Format: CPIMS-XXXXX-XXXXX-XXXXX-XXXXX - 25+ chars with dashes)
            string key = txtLicenseKey.Text.Replace("-", "").Replace(" ", "");
            if (PrimaryButton != null) PrimaryButton.Enabled = key.Length >= 20;
            lblStatus.Text = "";
        }

        private void BtnActivate_Click(object sender, EventArgs e)
        {
            string licenseKey = txtLicenseKey.Text.Trim();

            if (!ValidationHelper.ValidateRequiredFields(this, new Control[] { txtLicenseKey }, new string[] { LocalizationManager.GetString("Msg_LicenseKey") }))
            {
                return;
            }

            // Validate and activate
            // User requested to remove Name field, so we use a default internal name for validation
            string customerName = "Licensed User";
            LicenseKey license = LicenseManager.ActivateLicense(licenseKey, customerName);

            if (license == null)
            {
                lblStatus.Text = LocalizationManager.GetString("Msg_LicenseInvalid");
                lblStatus.ForeColor = Color.Red;
                return;
            }

            // Success
            LicenseActivated = true;
            string successMsg = string.Format(
                LocalizationManager.GetString("Msg_LicenseActivateSuccess", "License activated successfully!\n\nExpires: {0:MMMM dd, yyyy}"),
                license.ExpirationDate
            );
            MessageHelper.ShowSuccess(successMsg);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnStartTrial_Click(object sender, EventArgs e)
        {
            // Check if trial already used
            LicenseKey existingLicense = LicenseManager.GetCurrentLicense();
            if (existingLicense != null && existingLicense.IsTrial())
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_TrialUsed"));
                return;
            }

            if (MessageHelper.ConfirmAction(LocalizationManager.GetString("Msg_StartTrial")))
            {
                LicenseKey trial = LicenseManager.StartTrial();
                if (trial != null)
                {
                    LicenseActivated = true;
                    string msg = string.Format(
                        LocalizationManager.GetString("Msg_TrialActivateSuccess", "Trial activated! You have {0} days remaining."),
                        trial.DaysRemaining()
                    );
                    MessageHelper.ShowSuccess(msg);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageHelper.ShowError(LocalizationManager.GetString("Msg_TrialFailed"));
                }
            }
        }
    }
}
