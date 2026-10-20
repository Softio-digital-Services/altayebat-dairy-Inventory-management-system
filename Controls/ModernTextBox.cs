using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using InventorySystem.Helpers;

namespace InventorySystem.Controls
{
    public class ModernTextBox : UserControl
    {
        private TextBox txtInput;
        private Label lblTitle;
        private Panel pnlContainer;

        // Exposed Properties
        public override string Text
        {
            get => txtInput.Text;
            set => txtInput.Text = value;
        }

        [Category("Behavior")]
        public bool ReadOnly
        {
            get => txtInput.ReadOnly;
            set => txtInput.ReadOnly = value;
        }

        public new event KeyEventHandler KeyDown
        {
            add => txtInput.KeyDown += value;
            remove => txtInput.KeyDown -= value;
        }

        public void Clear()
        {
            this.Text = string.Empty;
        }

        private bool _isRequired = false;
        [Category("Behavior")]
        public bool IsRequired
        {
            get => _isRequired;
            set { _isRequired = value; UpdateLabel(); }
        }

        private bool _isError = false;
        [Category("Appearance")]
        public bool IsError
        {
            get => _isError;
            set { _isError = value; pnlContainer?.Invalidate(); }
        }

        private string _baseLabelText = "";
        [Category("Appearance")]
        public string LabelText
        {
            get => _baseLabelText;
            set { _baseLabelText = value; UpdateLabel(); }
        }

        private void UpdateLabel()
        {
            if (lblTitle == null) return;
            string text = _baseLabelText;
            if (_isRequired && !string.IsNullOrEmpty(text) && !text.EndsWith("*") && !text.EndsWith("* "))
                text += " *";
            lblTitle.Text = text;
            
            // Adjust label position for RTL
            if (LocalizationManager.IsArabic)
            {
                lblTitle.Location = new Point(this.Width - lblTitle.Width - 5, 0);
            }
            else
            {
                lblTitle.Location = new Point(5, 0);
            }
        }

        [Category("Behavior")]
        public bool UseSystemPasswordChar
        {
            get => txtInput.UseSystemPasswordChar;
            set => txtInput.UseSystemPasswordChar = value;
        }

        [Category("Behavior")]
        public bool Multiline
        {
            get => txtInput.Multiline;
            set 
            { 
                txtInput.Multiline = value;
                ResizeControls();
            }
        }

        [Category("Appearance")]
        public string PlaceholderText { get; set; } = "";

        [Category("Appearance")]
        public bool IsSearch { get; set; } = false;

        private bool _isPassword = false;
        [Category("Appearance")]
        public bool IsPassword
        {
            get => _isPassword;
            set
            {
                _isPassword = value;
                txtInput.UseSystemPasswordChar = value;
                ResizeControls();
                pnlContainer?.Invalidate();
            }
        }

        private bool _isFocused = false;

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

        public ModernTextBox()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true; 

            this.BackColor = Color.Transparent; // Host container
            this.Size = new Size(350, 60); // Default size (25px Label + 35px Input)
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
            lblTitle.Location = new Point(5, 0); // Top left
            this.Controls.Add(lblTitle);

            // Container Panel (Rounded White Box)
            pnlContainer = new Panel();
            pnlContainer.BackColor = Color.Transparent;
            pnlContainer.Location = new Point(0, 25);
            pnlContainer.Size = new Size(this.Width, this.Height - 25);
            pnlContainer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            pnlContainer.Paint += PnlContainer_Paint;
            pnlContainer.Padding = new Padding(10, 8, 10, 5);
            this.Controls.Add(pnlContainer);

            // TextBox
            txtInput = new TextBox();
            txtInput.BorderStyle = BorderStyle.None;
            txtInput.Font = new Font("Segoe UI", 10F); // premium size
            txtInput.ForeColor = ThemeConfig.TextColorDark;
            txtInput.Dock = DockStyle.Fill;
            txtInput.BackColor = ThemeConfig.SurfaceColor;
            if (LocalizationManager.IsArabic) txtInput.RightToLeft = RightToLeft.Yes;
            
            // Center vertically
            txtInput.Location = new Point(10, (pnlContainer.Height - txtInput.Height)/2);
            txtInput.TextChanged += (s, e) => {
                if (_isError) IsError = false; // Reset error on type
                this.OnTextChanged(EventArgs.Empty);
                UpdatePlaceholder();
            };
            
            txtInput.GotFocus += (s, e) => { 
                _isFocused = true; 
                pnlContainer.Invalidate(); 
                UpdatePlaceholder();
                // Prevent auto-selection of text
                if (this.IsHandleCreated) this.BeginInvoke(new Action(() => { txtInput.SelectionLength = 0; }));
            };
            txtInput.LostFocus += (s, e) => { _isFocused = false; pnlContainer.Invalidate(); UpdatePlaceholder(); };

            pnlContainer.Controls.Add(txtInput);
            
            pnlContainer.Click += (s, e) => {
                if (IsPassword)
                {
                    Point p = pnlContainer.PointToClient(Cursor.Position);
                    bool isAr = LocalizationManager.IsArabic;
                    bool isClickOnIcon = isAr ? p.X < 40 : p.X > pnlContainer.Width - 40;
                    
                    if (isClickOnIcon)
                    {
                        txtInput.UseSystemPasswordChar = !txtInput.UseSystemPasswordChar;
                        pnlContainer.Invalidate();
                    }
                }
            };

            UpdateLayout();
        }

        private void UpdateLayout()
        {
            if (pnlContainer == null) return;
            int labelHeight = _showLabel ? 25 : 0;
            pnlContainer.Location = new Point(0, labelHeight);
            
            // Standardize height for single-line inputs
            if (!Multiline)
            {
                this.Height = labelHeight + 35; // Enforce 35px input height
            }
            
            pnlContainer.Size = new Size(this.Width, this.Height - labelHeight);
            ResizeControls();
        }


        private void UpdatePlaceholder()
        {
            // Simple placeholder logic
            if (string.IsNullOrEmpty(txtInput.Text) && !_isFocused)
            {
                // Note: Real WinForms placeholder requires SendMessage EM_SETCUEBANNER
                // For now we rely on the Label or external logic, but we'll reserve the property.
            }
        }

        private void ResizeControls()
        {
            if (pnlContainer != null && txtInput != null)
            {
                if(Multiline)
                {
                     txtInput.Dock = DockStyle.Fill;
                }
                else
                {
                     // Center Vertically manually if dock behaves weirdly with single line in large panel
                     txtInput.Dock = DockStyle.None;
                     bool isAr = LocalizationManager.IsArabic;
                     int leftPadding = (isAr ? (IsPassword ? 40 : 10) : (IsSearch ? 40 : 10));
                     int rightPadding = (isAr ? (IsSearch ? 40 : 10) : (IsPassword ? 40 : 10));
                     
                     txtInput.Width = pnlContainer.Width - leftPadding - rightPadding;
                     txtInput.Location = new Point(leftPadding, (pnlContainer.Height - txtInput.Height) / 2);
                     txtInput.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ResizeControls();
            pnlContainer?.Invalidate(); // Redraw border
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            if (txtInput != null)
            {
                txtInput.RightToLeft = this.RightToLeft;
            }
            ResizeControls();
            pnlContainer?.Invalidate();
        }

        private void PnlContainer_Paint(object sender, PaintEventArgs e)
        {
            var pnl = sender as Panel;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // 1. Clear corners with PARENT color to solve "white corners bug"
            Color parentColor = ThemeConfig.GetParentColor(this);
            using (var brush = new SolidBrush(parentColor))
            {
                e.Graphics.FillRectangle(brush, -1, -1, pnl.Width + 2, pnl.Height + 2);
            }

            // 2. Draw Rounded Surface (White)
            const float radius = 12f;
            Rectangle bounds = new Rectangle(0, 0, pnl.Width, pnl.Height);
            ThemeConfig.FillRoundedBackground(e.Graphics, bounds, radius, ThemeConfig.SurfaceColor);

            // 3. Draw Border & Neon Glow — inset so no part of the stroke falls
            // outside the panel and gets cut off.
            Color borderColor = _isFocused ? ThemeConfig.PrimaryColor : ThemeConfig.BorderColor;

            if (_isFocused)
            {
                ThemeConfig.DrawRoundedBorder(e.Graphics, bounds, radius, Color.FromArgb(30, borderColor), 4f);
                ThemeConfig.DrawRoundedBorder(e.Graphics, bounds, radius, Color.FromArgb(60, borderColor), 2f);
                ThemeConfig.DrawRoundedBorder(e.Graphics, bounds, radius, borderColor, 2f);
            }
            else
            {
                ThemeConfig.DrawRoundedBorder(e.Graphics, bounds, radius, borderColor, 1.5f);
            }

            // Draw Search Icon
            bool isAr = LocalizationManager.IsArabic;
            if (IsSearch)
            {
                Image searchIcon = ThemeConfig.GetNuricon("search");
                if (searchIcon != null)
                {
                    int x = isAr ? pnl.Width - 32 : 12;
                    using (Image tinted = ThemeConfig.TintImage(searchIcon, ThemeConfig.PrimaryColor))
                    {
                        e.Graphics.DrawImage(tinted, new Rectangle(x, (pnl.Height - 20) / 2, 20, 20));
                    }
                }
            }

            // Draw Password Eye Icon
            if (IsPassword)
            {
                Image eyeIcon = ThemeConfig.GetNuricon("view");
                Color eyeColor = txtInput.UseSystemPasswordChar ? ThemeConfig.SecondaryColor : ThemeConfig.PrimaryColor;
                int x = isAr ? 12 : pnl.Width - 32;
                
                if (eyeIcon != null)
                {
                    using (Image tintedEye = ThemeConfig.TintImage(eyeIcon, eyeColor))
                    {
                        e.Graphics.DrawImage(tintedEye, new Rectangle(x, (pnl.Height - 20) / 2, 20, 20));
                    }
                }
                else
                {
                    // Fallback drawing if icon is missing
                    using (Pen pen = new Pen(eyeColor, 2f))
                    {
                        e.Graphics.DrawEllipse(pen, x, (pnl.Height - 12) / 2, 18, 10);
                        e.Graphics.FillEllipse(new SolidBrush(eyeColor), x + 6, (pnl.Height - 6) / 2, 6, 6);
                    }
                }
            }

        }

        private GraphicsPath GetRoundedPath(Rectangle rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = radius * 2;
            RectangleF r = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
