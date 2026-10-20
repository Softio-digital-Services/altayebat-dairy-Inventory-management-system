using System;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Helpers;
using InventorySystem.Controls;

namespace InventorySystem.Forms
{
    public class OrderIdPromptForm : BaseModalForm
    {
        private ModernTextBox txtOrderId;
        private Label _lblDesc;
        public int OrderId { get; private set; }

        public OrderIdPromptForm()
        {
            this.Size = new Size(420, 280);
            LocalizationManager.ApplyRTL(this);

            TableLayoutPanel tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(20)
            };
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            
            Label lblDesc = new Label
            {
                AutoSize = true,
                Font = ThemeConfig.StandardFont,
                ForeColor = ThemeConfig.SecondaryColor,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 15)
            };
            _lblDesc = lblDesc;
            
            txtOrderId = new ModernTextBox
            {
                Dock = DockStyle.Top,
                Width = 340,
                Height = 67 
            };
            txtOrderId.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Submit(); };
            
            tlp.Controls.Add(lblDesc, 0, 0);
            tlp.Controls.Add(txtOrderId, 0, 1);

            this.ContentPanel.Controls.Add(tlp);

            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();

            this.Shown += (s, e) => {
                txtOrderId.Focus();
            };
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            this.TitleText = LocalizationManager.GetString("Title_ReturnOrder");
            if (_lblDesc != null) _lblDesc.Text = LocalizationManager.GetString("Msg_EnterOrderId");
            if (txtOrderId != null) txtOrderId.LabelText = LocalizationManager.GetString("Msg_OrderId");
            SetFooterButtons(
                LocalizationManager.GetString("Tran_Continue"),
                LocalizationManager.GetString("Popup_Cancel"),
                (s, e) => Submit(),
                (s, e) => { DialogResult = DialogResult.Cancel; Close(); }
            );
        }


        private void Submit()
        {
            if (int.TryParse(txtOrderId.Text, out int id) && id > 0)
            {
                OrderId = id;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_InvalidOrderId"));
                txtOrderId.Focus();
            }
        }
    }
}
