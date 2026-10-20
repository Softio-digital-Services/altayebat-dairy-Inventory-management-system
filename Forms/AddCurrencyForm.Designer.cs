using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Helpers;

namespace InventorySystem.Forms
{
    partial class AddCurrencyForm
    {
        private void InitializeComponent()
        {
            this.txtCode = new InventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 15), IsRequired = true };
            this.txtName = new InventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 15), IsRequired = true };
            this.txtSymbol = new InventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 15) };
            this.numRate = new InventorySystem.Controls.ModernNumericUpDown { DecimalPlaces = 4, Maximum = 1000000, Dock = DockStyle.Fill, Margin = new Padding(0) };
            this.btnFetch = new InventorySystem.Controls.ModernButton { Text = "Fetch", Width = 110, Height = 42, Anchor = AnchorStyles.Bottom | AnchorStyles.Right, Margin = new Padding(0, 0, 0, 0) };
            ThemeConfig.ApplyPrimaryButton(btnFetch);
            
            this.SuspendLayout();

            TableLayoutPanel tlp = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, RowCount = 4, AutoSize = true, Padding = new Padding(25) };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            tlp.Controls.Add(txtCode, 0, 0);
            tlp.Controls.Add(txtName, 0, 1);
            tlp.Controls.Add(txtSymbol, 0, 2);
            
            TableLayoutPanel tlpRate = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 1, Height = 67, Margin = new Padding(0, 0, 0, 20) };
            tlpRate.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRate.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tlpRate.Controls.Add(numRate, 0, 0);
            
            // Center the Fetch button vertically with the NumericUpDown's input box
            // ModernNumericUpDown height is 67 (25 label + 42 box).
            // Fetch button height is 42. So we need a top margin of 25 to align with the box.
            btnFetch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFetch.Margin = new Padding(0, 25, 0, 0); 
            
            tlpRate.Controls.Add(btnFetch, 1, 0);
            tlp.Controls.Add(tlpRate, 0, 3);

            this.ClientSize = new System.Drawing.Size(420, 480);
            this.ContentPanel.Controls.Add(tlp);
            this.ContentPanel.Padding = new Padding(0, 0, 0, 10); // Extra bottom space

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private InventorySystem.Controls.ModernTextBox txtCode;
        private InventorySystem.Controls.ModernTextBox txtName;
        private InventorySystem.Controls.ModernTextBox txtSymbol;
        private InventorySystem.Controls.ModernNumericUpDown numRate;
        private InventorySystem.Controls.ModernButton btnFetch;
    }
}
