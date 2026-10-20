namespace InventorySystem.Forms
{
    partial class AddCategoryForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtName = new InventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10), IsRequired = true };
            this.txtDesc = new InventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };
            this.pbImage = new System.Windows.Forms.PictureBox { Width = 150, Height = 150, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.WhiteSmoke, Anchor = AnchorStyles.Top };
            this.btnUpload = new InventorySystem.Controls.ModernButton { Text = "Upload Image", Height = 35, Width = 150, Anchor = AnchorStyles.Top };
            
            this.SuspendLayout();

            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(25),
                AutoSize = true
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            tlpMain.Controls.Add(this.txtName, 0, 0);
            tlpMain.Controls.Add(this.txtDesc, 0, 1);
            
            FlowLayoutPanel flpImg = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, Anchor = AnchorStyles.Top };
            flpImg.Controls.Add(this.pbImage);
            flpImg.Controls.Add(this.btnUpload);
            tlpMain.Controls.Add(flpImg, 0, 2);

            this.ClientSize = new System.Drawing.Size(450, 520);
            this.ContentPanel.Controls.Add(tlpMain);

            this.Name = "AddCategoryForm";
            this.Text = "Add Category";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private InventorySystem.Controls.ModernTextBox txtName;
        private InventorySystem.Controls.ModernTextBox txtDesc;
        private System.Windows.Forms.PictureBox pbImage;
        private InventorySystem.Controls.ModernButton btnUpload;
    }
}
