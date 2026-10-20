using System;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Controls;
using InventorySystem.Helpers;

namespace InventorySystem.Forms
{
    public partial class ModernMessageBox : BaseModalForm
    {
        private Label lblMessage;
        private PictureBox picIcon;

        public ModernMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            InitializeModernUI();
            
            this.TitleText = caption;
            this.lblMessage.Text = text;
            
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            SetIcon(icon);
            SetButtons(buttons, isArabic);
            
            // Adjust size based on message length
            AdjustSize(text);
        }

        private void InitializeModernUI()
        {
            this.Size = new Size(380, 180);
            this.EnforceMinWidth = false;

            // Content Area
            TableLayoutPanel tlpContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(20),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            tlpContent.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F)); // Reduced from 60
            tlpContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            picIcon = new PictureBox
            {
                Size = new Size(32, 32), // Reduced from 48x48
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = new Padding(0, 0, 10, 0),
                Anchor = AnchorStyles.None // Center vertically in row
            };

            lblMessage = new Label
            {
                Text = "Message Text",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Regular), // Increased from StandardFont (9pt)
                ForeColor = ThemeConfig.TextColorDark,
                Dock = DockStyle.Fill,
                TextAlign = LocalizationManager.IsArabic ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft,
                AutoSize = false 
            };

            tlpContent.Controls.Add(picIcon, 0, 0);
            tlpContent.Controls.Add(lblMessage, 1, 0);

            this.ContentPanel.AddControl(tlpContent);
        }

        private void SetIcon(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Error:
                    picIcon.Image = ThemeConfig.GetNuricon("error");
                    this.BorderColor = ThemeConfig.DangerColor;
                    break;
                case MessageBoxIcon.Information:
                    picIcon.Image = ThemeConfig.GetNuricon("info");
                    this.BorderColor = ThemeConfig.PrimaryColor;
                    break;
                case MessageBoxIcon.Question:
                    picIcon.Image = ThemeConfig.GetNuricon("info"); // Using info for question as requested
                    this.BorderColor = ThemeConfig.PrimaryColor;
                    break;
                case MessageBoxIcon.Exclamation:
                    picIcon.Image = ThemeConfig.GetNuricon("warning");
                    this.BorderColor = ThemeConfig.WarningColor;
                    break;
                default:
                    picIcon.Visible = false;
                    this.BorderColor = ThemeConfig.PrimaryColor;
                    break;
            }
        }

        private void SetButtons(MessageBoxButtons buttons, bool isArabic)
        {
            string ok = LocalizationManager.GetString("Popup_OK", "OK");
            string cancel = LocalizationManager.GetString("Popup_Cancel", "Cancel");
            string yes = LocalizationManager.GetString("Popup_Yes", "Yes");
            string no = LocalizationManager.GetString("Popup_No", "No");

            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    SetFooterButtons(ok, "", (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); }, null);
                    break;

                case MessageBoxButtons.OKCancel:
                    SetFooterButtons(ok, cancel, 
                        (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); }, 
                        (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); });
                    break;

                case MessageBoxButtons.YesNo:
                    SetFooterButtons(no, yes, 
                        (s, e) => { this.DialogResult = DialogResult.No; this.Close(); }, 
                        (s, e) => { this.DialogResult = DialogResult.Yes; this.Close(); });
                    break;

                case MessageBoxButtons.YesNoCancel:
                    SetFooterButtons(no, yes, 
                        (s, e) => { this.DialogResult = DialogResult.No; this.Close(); }, 
                        (s, e) => { this.DialogResult = DialogResult.Yes; this.Close(); },
                        cancel, (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); });
                    break;
            }
        }

        private void AdjustSize(string text)
        {
            // Initial size estimate - smaller for simple alerts
            this.Width = 380;
            
            // Allow BaseModalForm.OnLoad to handle the final FitToContent
            // But we can trigger it early if we want immediate results
            FitToContent();
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            using (var msgBox = new ModernMessageBox(text, caption, buttons, icon))
            {
                return msgBox.ShowDialog();
            }
        }
    }
}
