using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using InventorySystem.Helpers;
using InventorySystem;

namespace InventorySystem.Controls
{
    public class ModernComboBox : UserControl
    {
        private ComboBox cmbInput;
        private Label lblTitle;
        private Panel pnlContainer;

        public ComboBox InnerComboBox => cmbInput;

        public override string Text
        {
            get => cmbInput.Text;
            set => cmbInput.Text = value;
        }

        [Category("Appearance")]
        public string LabelText
        {
            get => lblTitle.Text;
            set { lblTitle.Text = value; UpdateLayout(); }
        }

        public object SelectedItem
        {
            get => cmbInput.SelectedItem;
            set => cmbInput.SelectedItem = value;
        }

        public int SelectedIndex
        {
            get => cmbInput.SelectedIndex;
            set => cmbInput.SelectedIndex = value;
        }

        public ComboBoxStyle DropDownStyle
        {
            get => cmbInput.DropDownStyle;
            set => cmbInput.DropDownStyle = value;
        }

        public object SelectedValue
        {
            get => cmbInput.SelectedValue;
            set => cmbInput.SelectedValue = value;
        }

        public ComboBox.ObjectCollection Items => cmbInput.Items;

        public object DataSource
        {
            get => cmbInput.DataSource;
            set => cmbInput.DataSource = value;
        }

        public string DisplayMember
        {
            get => cmbInput.DisplayMember;
            set => cmbInput.DisplayMember = value;
        }

        public string ValueMember
        {
            get => cmbInput.ValueMember;
            set => cmbInput.ValueMember = value;
        }

        private string _placeholderText = "";
        [Category("Appearance")]
        public string PlaceholderText 
        { 
            get => _placeholderText;
            set 
            { 
                _placeholderText = value; 
                if (lblTitle != null && _showLabel) lblTitle.Text = value;
            } 
        }

        private bool _showLabel = true;
        [Category("Appearance")]
        public bool ShowLabel
        {
            get => _showLabel;
            set
            {
                _showLabel = value;
                if (lblTitle != null) lblTitle.Visible = value;
                UpdateLayout();
            }
        }

        private bool _isFocused = false;

        public ModernComboBox()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true; 

            this.BackColor = Color.Transparent;
            this.Size = new Size(200, 70);
            this.Padding = new Padding(0);

            InitializeControls();
        }

        private void InitializeControls()
        {
            // Label
            lblTitle = new Label();
            lblTitle.Text = "";
            lblTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTitle.ForeColor = ThemeConfig.TextColorDark;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(5, 0);
            this.Controls.Add(lblTitle);

            // Container Panel
            pnlContainer = new Panel();
            pnlContainer.BackColor = Color.Transparent;
            pnlContainer.Location = new Point(0, 25);
            pnlContainer.Size = new Size(this.Width, 45); // Match ModernTextBox height
            pnlContainer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlContainer.Paint += PnlContainer_Paint;
            pnlContainer.Padding = new Padding(0); // We will manually center the combobox
            this.Controls.Add(pnlContainer);

            // ComboBox
            cmbInput = new ComboBox();
            cmbInput.FlatStyle = FlatStyle.Flat;
            cmbInput.Font = new Font("Segoe UI", 10F);
            cmbInput.ForeColor = ThemeConfig.TextColorDark;
            cmbInput.BackColor = ThemeConfig.SurfaceColor;
            cmbInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            if (LocalizationManager.IsArabic) cmbInput.RightToLeft = RightToLeft.Yes;
            
            pnlContainer.Resize += (s, e) => {
                if (cmbInput != null)
                {
                    cmbInput.Width = pnlContainer.Width - 24;
                    cmbInput.Location = new Point(12, (pnlContainer.Height - cmbInput.Height) / 2);
                }
            };

            cmbInput.Enter += (s, e) => { 
                _isFocused = true; 
                pnlContainer.Invalidate();
                // Prevent auto-selection of text
                if (cmbInput.IsHandleCreated) cmbInput.BeginInvoke(new Action(() => { cmbInput.Select(0, 0); cmbInput.SelectionLength = 0; }));
            };
            cmbInput.GotFocus += (s, e) => { 
                _isFocused = true; 
                pnlContainer.Invalidate(); 
                // Ensure selection is cleared on focus
                if (cmbInput.IsHandleCreated) cmbInput.BeginInvoke(new Action(() => { cmbInput.Select(0, 0); cmbInput.SelectionLength = 0; }));
            };
            cmbInput.LostFocus += (s, e) => { _isFocused = false; pnlContainer.Invalidate(); };

            // Clear selection after a choice is made to prevent blue highlight
            cmbInput.SelectedIndexChanged += (s, e) => {
                if (cmbInput.IsHandleCreated) cmbInput.BeginInvoke(new Action(() => { cmbInput.Select(0, 0); cmbInput.SelectionLength = 0; }));
            };
            
            // Handle DropDown events to clear selection after closing
            cmbInput.DropDownClosed += (s, e) => {
                if (cmbInput.IsHandleCreated) cmbInput.BeginInvoke(new Action(() => { cmbInput.Select(0, 0); cmbInput.SelectionLength = 0; }));
            };

            pnlContainer.Controls.Add(cmbInput);
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            if (pnlContainer == null) return;
            bool isAr = LocalizationManager.IsArabic;
            int labelHeight = _showLabel ? 25 : 0;
            
            this.Height = labelHeight + 35; // Enforce 35px input height
            
            pnlContainer.Location = new Point(0, labelHeight);
            pnlContainer.Size = new Size(this.Width, 35);

            if (lblTitle != null)
            {
                if (isAr)
                    lblTitle.Location = new Point(this.Width - lblTitle.Width - 5, 0);
                else
                    lblTitle.Location = new Point(5, 0);
            }
        }


        private void PnlContainer_Paint(object sender, PaintEventArgs e)
        {
            var pnl = sender as Panel;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // 1. Clear corners with PARENT color to solve "white corners bug"
            Color parentColor = ThemeConfig.GetParentColor(this);
            using (var brush = new SolidBrush(parentColor))
            {
                e.Graphics.FillRectangle(brush, -1, -1, pnl.Width + 2, pnl.Height + 2);
            }

            // 2. Draw Rounded Surface (White)
            Rectangle bounds = new Rectangle(0, 0, pnl.Width, pnl.Height);
            ThemeConfig.FillRoundedBackground(e.Graphics, bounds, 12f, ThemeConfig.SurfaceColor);

            // 3. Draw Border (Primary if focused, else BorderColor)
            Color borderColor = _isFocused ? ThemeConfig.PrimaryColor : ThemeConfig.BorderColor;
            ThemeConfig.DrawRoundedBorder(e.Graphics, bounds, 12f, borderColor, _isFocused ? 2f : 1.5f);
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            pnlContainer?.Invalidate();
        }
    }
}
