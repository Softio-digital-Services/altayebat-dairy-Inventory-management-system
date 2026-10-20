using System;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Controls;
using InventorySystem.Helpers;

namespace InventorySystem.Forms
{
    public partial class LicenseInfoForm : BaseModalForm
    {
        private LicenseKey _license;
        private TableLayoutPanel tlpInfo;

        private Label lblRenewal;

        public LicenseInfoForm()
        {
            this.Width = 600;
            InitializeComponent();
            // Adaptive sizing handled by BaseModalForm.OnLoad
            
            ApplyLocalization();
            LoadLicenseInfo();
            LocalizationManager.LanguageChanged += (s, e) => { ApplyLocalization(); LoadLicenseInfo(); };
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Header removed as it's already in the Modal Header

            tlpInfo = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 0,
                Padding = new Padding(25, 10, 25, 10),
                AutoSize = true,
                AutoScroll = false
            };
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.ContentPanel.Controls.Add(tlpInfo);

            ApplyLocalization(); // To set initial button text

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            this.TitleText = LocalizationManager.GetString("Msg_LicenseInfo");
            
            var currentLicense = LicenseManager.GetCurrentLicense();
            bool showActivate = currentLicense == null || currentLicense.IsTrial();

            SetFooterButtons(
                LocalizationManager.GetString("Popup_Cancel"),
                showActivate ? (LocalizationManager.GetString("Btn_ActivateLicense", "Activate License")) : "",
                (s, e) => this.Close(),
                showActivate ? new EventHandler((s, e) => {
                    this.Close();
                    new LicenseActivationForm().ShowDialog();
                }) : null
            );
        }

        private void LoadLicenseInfo()
        {
            tlpInfo.Controls.Clear();
            tlpInfo.RowCount = 0;
            tlpInfo.RowStyles.Clear();

            _license = LicenseManager.GetCurrentLicense();
            bool isArabic = LocalizationManager.IsArabic;

            if (_license == null)
            {
                AddInfoRow(LocalizationManager.GetString("Msg_Status"), LocalizationManager.GetString("Msg_NoLicense"), ThemeConfig.DangerColor);
                return;
            }

            // License Type
            string typeDisplay = _license.IsTrial() 
                ? LocalizationManager.GetString("Msg_Trial", "Trial Version") 
                : LocalizationManager.GetString("Msg_Licensed", "Licensed Version");
            AddInfoRow(LocalizationManager.GetString("Msg_LicenseType"), typeDisplay, ThemeConfig.TextColorDark);

            // Customer Name
            if (!string.IsNullOrEmpty(_license.CustomerName))
            {
                AddInfoRow(LocalizationManager.GetString("Msg_CustomerName"), _license.CustomerName, ThemeConfig.TextColorDark);
            }

            // License Key
            if (!_license.IsTrial())
            {
                AddInfoRow(LocalizationManager.GetString("Msg_LicenseKeyLabel"), _license.Key, ThemeConfig.SecondaryColor);
            }

            // Activation Date
            AddInfoRow(LocalizationManager.GetString("Msg_ActivatedOn"), _license.ActivationDate.ToString("MMMM dd, yyyy"), ThemeConfig.TextColorDark);

            // Expiration Date
            Color expiryColor = _license.IsExpiringSoon() ? Color.Orange : ThemeConfig.TextColorDark;
            AddInfoRow(LocalizationManager.GetString("Msg_ExpiresOn"), _license.ExpirationDate.ToString("MMMM dd, yyyy"), expiryColor);

            // Days Remaining
            int daysLeft = _license.DaysRemaining();
            Color daysColor = daysLeft <= 30 ? ThemeConfig.DangerColor : ThemeConfig.SuccessColor;
            string daysText = string.Format(LocalizationManager.GetString("Msg_Days", "{0} days"), daysLeft);
            AddInfoRow(LocalizationManager.GetString("Msg_DaysRemaining"), daysText, daysColor);

            // Status
            bool isValid = _license.IsValid();
            string statusText = isValid ? LocalizationManager.GetString("Msg_Active", "Active") : LocalizationManager.GetString("Msg_Expired", "Expired");
            Color statusColor = isValid ? ThemeConfig.SuccessColor : ThemeConfig.DangerColor;
            AddInfoRow(LocalizationManager.GetString("Msg_Status"), statusText, statusColor);

            // Machine Name
            AddInfoRow(LocalizationManager.GetString("Msg_Machine", "Machine:"), _license.MachineName, ThemeConfig.SecondaryColor);

            // Renewal Notice
            if (_license.IsExpiringSoon() && !_license.IsTrial())
            {
                int row = tlpInfo.RowCount++;
                tlpInfo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                lblRenewal = new Label
                {
                    Text = LocalizationManager.GetString("Msg_LicenseRenewNotice", "Your license is expiring soon. Please renew."),
                    Font = ThemeConfig.StandardFont,
                    ForeColor = ThemeConfig.WarningColor,
                    AutoSize = true,
                    Margin = new Padding(0, 20, 0, 0)
                };
                tlpInfo.Controls.Add(lblRenewal, 0, row);
                tlpInfo.SetColumnSpan(lblRenewal, 2);
            }
        }

        private void AddInfoRow(string label, string value, Color valueColor)
        {
            int row = tlpInfo.RowCount++;
            tlpInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));

            Label lblLabel = new Label { Text = label, Font = ThemeConfig.SmallBoldFont, ForeColor = ThemeConfig.SecondaryColor, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            Label lblValue = new Label { Text = value, Font = ThemeConfig.StandardFont, ForeColor = valueColor, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            
            tlpInfo.Controls.Add(lblLabel, 0, row);
            tlpInfo.Controls.Add(lblValue, 1, row);
        }
    }
}
