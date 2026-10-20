using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using InventorySystem.Helpers;

namespace InventorySystem.Controls
{
    public class ModernNumericUpDown : UserControl
    {
        private TextBox txtInput;
        private Label lblTitle;
        private Panel pnlContainer;
        // private Button btnUp;
        // private Button btnDown;

        private decimal _value = 0;
        [Category("Appearance")]
        public decimal Value
        {
            get => _value;
            set 
            { 
                _value = Math.Max(_minimum, Math.Min(_maximum, value));
                UpdateText();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private decimal _minimum = 0;
        [Category("Appearance")]
        public decimal Minimum 
        { 
            get => _minimum; 
            set { _minimum = value; if (_value < value) Value = value; } 
        }

        private decimal _maximum = 100;
        [Category("Appearance")]
        public decimal Maximum 
        { 
            get => _maximum; 
            set { _maximum = value; if (_value > value) Value = value; } 
        }

        private int _decimalPlaces = 0;
        [Category("Appearance")]
        public int DecimalPlaces 
        { 
            get => _decimalPlaces; 
            set { _decimalPlaces = value; UpdateText(); } 
        }

        private decimal _increment = 1;
        [Category("Appearance")]
        public decimal Increment 
        { 
            get => _increment; 
            set => _increment = value; 
        }

        [Category("Appearance")]
        public string LabelText 
        { 
            get => lblTitle.Text; 
            set { lblTitle.Text = value; UpdateLayout(); }
        }

        private bool _showLabel = true;
        [Category("Appearance")]
        public bool ShowLabel
        {
            get => _showLabel;
            set
            {
                _showLabel = value;
                lblTitle.Visible = value;
                UpdateLayout();
            }
        }

        public event EventHandler ValueChanged;

        public ModernNumericUpDown()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;
            this.Size = new Size(150, 67);
            
            InitializeControls();
        }

        private void InitializeControls()
        {
            // Label
            lblTitle = new Label { 
                Font = new Font("Segoe UI", 9F, FontStyle.Bold), 
                ForeColor = ThemeConfig.TextColorDark, 
                AutoSize = true, 
                Location = new Point(5, 0) 
            };
            this.Controls.Add(lblTitle);

            // Container Panel
            bool isAr = LocalizationManager.IsArabic;
            pnlContainer = new Panel { 
                BackColor = Color.Transparent,
                Padding = isAr ? new Padding(26, 6, 10, 5) : new Padding(10, 6, 26, 5)
            };
            pnlContainer.Paint += PnlContainer_Paint;
            this.Controls.Add(pnlContainer);

            // Input TextBox
            txtInput = new TextBox { 
                BorderStyle = BorderStyle.None, 
                Font = new Font("Segoe UI", 10F), 
                ForeColor = ThemeConfig.TextColorDark, 
                BackColor = ThemeConfig.SurfaceColor,
                Dock = DockStyle.Fill,
                RightToLeft = isAr ? RightToLeft.Yes : RightToLeft.No
            };
            txtInput.KeyPress += TxtInput_KeyPress;
            txtInput.Enter += (s, e) => {
                if (txtInput.IsHandleCreated) txtInput.BeginInvoke(new Action(() => { txtInput.Select(0, 0); txtInput.SelectionLength = 0; }));
            };
            txtInput.GotFocus += (s, e) => {
                if (txtInput.IsHandleCreated) txtInput.BeginInvoke(new Action(() => { txtInput.Select(0, 0); txtInput.SelectionLength = 0; }));
            };
            txtInput.LostFocus += (s, e) => {
                if (decimal.TryParse(txtInput.Text, out decimal val)) Value = val;
                else UpdateText();
            };
            pnlContainer.Controls.Add(txtInput);

            pnlContainer.MouseClick += (s, e) => {
                bool isAr = LocalizationManager.IsArabic;
                int btnZoneX = isAr ? 0 : pnlContainer.Width - 25;
                if (e.X >= btnZoneX && e.X <= btnZoneX + 25)
                {
                    if (e.Y < pnlContainer.Height / 2) Value += _increment;
                    else Value -= _increment;
                }
            };

            pnlContainer.MouseMove += (s, e) => {
                bool isAr = LocalizationManager.IsArabic;
                int btnZoneX = isAr ? 0 : pnlContainer.Width - 25;
                if (e.X >= btnZoneX && e.X <= btnZoneX + 25)
                    pnlContainer.Cursor = Cursors.Hand;
                else
                    pnlContainer.Cursor = Cursors.Default;
            };

            UpdateLayout();
            UpdateText();
        }

        private void UpdateLayout()
        {
            if (pnlContainer == null) return;

            int labelHeight = _showLabel ? 25 : 0;
            int totalHeight = labelHeight + 35;

            // Pin our own height — never let a parent stretch us beyond our natural size
            this.Height = totalHeight;

            pnlContainer.Location = new Point(0, labelHeight);
            pnlContainer.Size = new Size(this.Width, 35); // always exactly 35px tall

            if (lblTitle != null)
            {
                if (LocalizationManager.IsArabic)
                    lblTitle.Location = new Point(this.Width - lblTitle.Width - 5, 0);
                else
                    lblTitle.Location = new Point(5, 0);
            }

            // Buttons are now drawn manually in paint event
        }

        private void UpdateText()
        {
            if (txtInput != null)
                txtInput.Text = _value.ToString("F" + _decimalPlaces);
        }

        private void TxtInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.') && (e.KeyChar != '-'))
                e.Handled = true;
            if ((e.KeyChar == '.') && (txtInput.Text.IndexOf('.') > -1))
                e.Handled = true;
            if ((e.KeyChar == '-') && (txtInput.Text.Length > 0 && txtInput.SelectionStart != 0))
                e.Handled = true;
        }

        private void DrawChevron(Graphics g, bool up)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Image icon = ThemeConfig.GetNuricon(up ? "chevron_up" : "chevron_down");
            if (icon != null)
            {
                using (var tinted = ThemeConfig.TintImage(icon, ThemeConfig.SecondaryColor))
                {
                    g.DrawImage(tinted, new Rectangle(7, 4, 10, 9));
                }
            }
        }

        private void PnlContainer_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Clear corners
            Color parentColor = ThemeConfig.GetParentColor(this);
            using (var brush = new SolidBrush(parentColor))
                e.Graphics.FillRectangle(brush, -1, -1, pnlContainer.Width + 2, pnlContainer.Height + 2);

            Rectangle bounds = new Rectangle(0, 0, pnlContainer.Width, pnlContainer.Height);
            ThemeConfig.FillRoundedBackground(e.Graphics, bounds, 12f, ThemeConfig.SurfaceColor);
            ThemeConfig.DrawRoundedBorder(e.Graphics, bounds, 12f, ThemeConfig.BorderColor, 1.5f);

            // Separator for buttons
            bool isAr = LocalizationManager.IsArabic;
            int btnZoneX = isAr ? 25 : pnlContainer.Width - 25;
            
            // Draw lines for button area
            e.Graphics.DrawLine(new Pen(ThemeConfig.BorderColor, 1f), btnZoneX, 5, btnZoneX, pnlContainer.Height - 5);
            e.Graphics.DrawLine(new Pen(ThemeConfig.BorderColor, 1f), isAr ? 1 : btnZoneX, pnlContainer.Height / 2, isAr ? 25 : pnlContainer.Width - 2, pnlContainer.Height / 2);

            // Draw chevrons
            int chevronX = isAr ? 7 : pnlContainer.Width - 18;
            Image iconUp = ThemeConfig.GetNuricon("chevron_up");
            Image iconDown = ThemeConfig.GetNuricon("chevron_down");
            if (iconUp != null)
            {
                using (var tinted = ThemeConfig.TintImage(iconUp, ThemeConfig.SecondaryColor))
                    e.Graphics.DrawImage(tinted, new Rectangle(chevronX, 5, 10, 9));
            }
            if (iconDown != null)
            {
                using (var tinted = ThemeConfig.TintImage(iconDown, ThemeConfig.SecondaryColor))
                    e.Graphics.DrawImage(tinted, new Rectangle(chevronX, (pnlContainer.Height / 2) + 4, 10, 9));
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateLayout();
            pnlContainer?.Invalidate();
        }
    }
}
