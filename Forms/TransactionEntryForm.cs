using System;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Controls;
using InventorySystem.Helpers;

namespace InventorySystem.Forms
{
    public partial class TransactionEntryForm : BaseModalForm
    {
        public decimal Amount { get; private set; }
        public string Notes { get; private set; }
        public DateTime? DueDate { get; private set; }

        private ModernTextBox txtAmount;
        private ModernTextBox txtNotes;
        private FlatDateTimePicker dtDueDate;
        private Label lblDueDate;
        private Label lblError;
        private Label lblPrompt;
        private bool _showDueDate;

        public TransactionEntryForm(string title, string prompt, string initialValue = "0.00", bool showDueDate = false)
        {
            _showDueDate = showDueDate;
            InitializeComponent();
            this.TitleText = title; // BaseModalForm Title
            lblPrompt.Text = prompt;
            txtAmount.Text = initialValue;
            ApplyTheme();
            InventorySystem.Helpers.LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            InventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            Func<string, string> L = InventorySystem.Helpers.LocalizationManager.GetString;

            if (txtAmount != null) txtAmount.LabelText = L("Tran_AmountLabel");
            if (txtNotes != null) txtNotes.LabelText = L("Tran_NotesLabel");
            if (lblDueDate != null) lblDueDate.Text = LocalizationManager.GetString("Tran_DueDateLabel", "Payment Due Date");

            SetFooterButtons(
                LocalizationManager.GetString("Tran_Confirm", "Confirm"),
                LocalizationManager.GetString("Tran_Cancel", "Cancel"),
                BtnSave_Click,
                (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); }
            );
        }

        private void InitializeComponent()
        {
            
            lblPrompt = new Label { AutoSize = true, Font = ThemeConfig.SubHeaderFont, ForeColor = ThemeConfig.SecondaryColor, Margin = new Padding(0, 0, 0, 10) };
            txtAmount = new ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 15) };
            txtNotes = new ModernTextBox { Dock = DockStyle.Fill, Multiline = true, Height = 120, Margin = new Padding(0, 0, 0, 15) };
            lblError = new Label { AutoSize = true, ForeColor = ThemeConfig.DangerColor, Visible = false, Font = ThemeConfig.StandardFont, Margin = new Padding(0, 5, 0, 5) };
            
            this.SuspendLayout();

            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = _showDueDate ? 7 : 5,
                Padding = new Padding(25),
                AutoSize = true
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            tlpMain.Controls.Add(lblPrompt, 0, 0);
            tlpMain.Controls.Add(txtAmount, 0, 1);

            int nextRow = 2;
            if (_showDueDate)
            {
                lblDueDate = new Label { AutoSize = true, Font = ThemeConfig.StandardFont, ForeColor = ThemeConfig.SecondaryColor, Margin = new Padding(0, 5, 0, 5) };
                dtDueDate = new FlatDateTimePicker { Dock = DockStyle.Fill, Height = 35, Margin = new Padding(0, 0, 0, 15), Value = DateTime.Today.AddDays(30) };
                
                tlpMain.Controls.Add(lblDueDate, 0, nextRow++);
                tlpMain.Controls.Add(dtDueDate, 0, nextRow++);
            }

            tlpMain.Controls.Add(txtNotes, 0, nextRow++);
            tlpMain.Controls.Add(lblError, 0, nextRow++);

            this.ContentPanel.Controls.Add(tlpMain);

            ApplyLocalization(); // To set initial button text

            this.ResumeLayout(false);
            this.PerformLayout();
            
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            // Background is White (BaseModalForm)
            // Footer buttons are styled automatically by SetFooterButtons
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string clean = txtAmount.Text.Replace("$", "").Replace(",", "").Trim();
            if (ValidationHelper.ValidateDecimal(clean, LocalizationManager.GetString("Tran_AmountLabel"), out decimal val))
            {
                if (val <= 0)
                {
                    string msg = LocalizationManager.GetString("Msg_AmountZero");
                    MessageHelper.ShowWarning(msg);
                    return;
                }

                this.Amount = val;
                this.Notes = txtNotes.Text.Trim();
                if (_showDueDate && dtDueDate != null)
                {
                    this.DueDate = dtDueDate.Value;
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}

