using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventorySystem.Controls
{
    public class StatCard : Panel
    {
        private Label _lblTitle;
        private Label _lblValue;
        private Label _lblSubtitle;
        private Panel _iconPanel;
        private string _iconText;
        private Color _themeColor = Color.Blue;

        public string Title 
        { 
            get => _lblTitle.Text; 
            set
            {
                _lblTitle.Text = value;
                RepositionForRTL();
            }
        }

        public string Value 
        { 
            get => _lblValue.Text; 
            set
            {
                _lblValue.Text = value;
                if (_compact) RepositionForRTL();
            }
        }

        public string Subtitle 
        { 
            get => _lblSubtitle.Text; 
            set => _lblSubtitle.Text = value; 
        }

        private Image _iconImage;

        public Image IconImage
        {
            get => _iconImage;
            set
            {
                _iconImage = value;
                if (_iconPanel != null) _iconPanel.Invalidate();
            }
        }

        public string Icon 
        { 
            get => _iconText; 
            set 
            {
                _iconText = value;
                if (_iconPanel != null) _iconPanel.Invalidate();
            } 
        }

        public Color ThemeColor
        {
            get => _themeColor;
            set 
            {
                _themeColor = value;
                UpdateTheme();
            }
        }

        private int _iconPadding = 8;
        public int IconPadding
        {
            get => _iconPadding;
            set
            {
                _iconPadding = value;
                if (_iconPanel != null) _iconPanel.Invalidate();
            }
        }

        private int _cornerRadius = 15;
        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = Math.Max(4, value); ApplyRoundedRegion(); Invalidate(); }
        }

        private bool _compact;
        /// <summary>
        /// Tighter typography and a vertically centred title/value block, for rails
        /// and toolbars where the card is short and has no subtitle.
        /// </summary>
        public bool Compact
        {
            get => _compact;
            set
            {
                _compact = value;
                if (_lblTitle == null) return;
                _lblTitle.Font = ValueFont(value ? 9.5F : 12F);
                if (value)
                    _fittedText = null; // force FitValueFont to pick a size
                else
                    _lblValue.Font = ValueFont(22F);
                _lblSubtitle.Visible = !value;
                _iconPadding = value ? 4 : 8;
                _cornerRadius = value ? 18 : 16;
                MinimumSize = value ? new Size(100, 72) : new Size(120, 90);
                RepositionForRTL();
                ApplyRoundedRegion();
            }
        }

        public StatCard()
        {
            InitializeControls();
        }

        private void InitializeControls()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            this.Size = new Size(240, 110);
            this.Padding = new Padding(20);
            this.BackColor = Color.Transparent;
            
            // Container for Icon
            _iconPanel = new Panel
            {
                Size = new Size(50, 50),
                BackColor = Color.Transparent
            };
            _iconPanel.Location = new Point(this.Width - 65, 20); // Top Right
            _iconPanel.Paint += IconPanel_Paint;
            



            // Labels
            _lblTitle = new Label
            {
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ThemeConfig.MutedTextColor,
                AutoSize = false,
                AutoEllipsis = false,
                Location = new Point(20, 15),
                BackColor = Color.Transparent
            };

            _lblValue = new Label
            {
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                AutoSize = true,
                Location = new Point(18, 40),
                BackColor = Color.Transparent
            };

            _lblSubtitle = new Label
            {
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeConfig.MutedTextColor,
                AutoSize = true,
                Location = new Point(22, 85),
                BackColor = Color.Transparent
            };


            this.Controls.Add(_iconPanel);
            this.Controls.Add(_lblSubtitle);
            this.Controls.Add(_lblValue);
            this.Controls.Add(_lblTitle);

            // Initial Theme
            this.ThemeColor = Color.FromArgb(67, 24, 255); // Default Blue
            
            this.RightToLeftChanged += (s, e) => RepositionForRTL();
            RepositionForRTL();
            ApplyRoundedRegion();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RepositionForRTL();
            ApplyRoundedRegion();
        }

        private void ApplyRoundedRegion()
        {
            // A clipping Region cuts pixels on whole-pixel boundaries, which sanded the
            // antialiased corners back into hard steps. OnPaint already paints the
            // parent colour behind an antialiased rounded card, so no clipping is needed.
            if (Region == null) return;
            var old = Region;
            Region = null;
            old.Dispose();
        }

        private void RepositionForRTL()
        {
            // Guard: may be called before InitializeControls finishes
            if (_iconPanel == null || _lblTitle == null || _lblValue == null || _lblSubtitle == null) return;

            // Use IsArabic as the source of truth — the inherited RightToLeft property
            // may not have propagated yet when OnResize fires during initialization.
            bool isRtl = Helpers.LocalizationManager.IsArabic;

            if (_compact)
            {
                RepositionCompact(isRtl);
                return;
            }

            // Resizing labels below raises layout events that come back here; without
            // this guard a language switch can recurse until the fonts are disposed.
            if (_repositioning) return;
            _repositioning = true;
            try { LayoutRegular(isRtl); }
            finally { _repositioning = false; }
        }

        private void LayoutRegular(bool isRtl)
        {
            // Icon sits bottom-corner so titles can use the full card width.
            int iconY = Math.Max(15, this.Height - 65);
            _iconPanel.Location = isRtl ? new Point(15, iconY) : new Point(this.Width - 70, iconY);

            int labelX = isRtl ? 20 : 20;
            // Title spans nearly full width; value stays clear of the bottom icon.
            int titleWidth = Math.Max(40, this.Width - 40);
            int valueWidth = Math.Max(40, this.Width - 90);

            _lblTitle.AutoSize = false;
            _lblValue.AutoSize = false;
            _lblSubtitle.AutoSize = false;
            _lblTitle.AutoEllipsis = false;

            int titleH = TextRenderer.MeasureText(
                string.IsNullOrEmpty(_lblTitle.Text) ? "Ag" : _lblTitle.Text,
                _lblTitle.Font,
                new Size(titleWidth, 0),
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height;
            titleH = Math.Clamp(titleH, 18, 44);

            _lblTitle.Size = new Size(titleWidth, titleH);
            _lblValue.Size = new Size(valueWidth, 45);
            _lblSubtitle.Size = new Size(valueWidth, 20);

            int titleY = 12;
            int valueY = titleY + titleH + 2;
            _lblTitle.Location = new Point(labelX, titleY);
            _lblValue.Location = new Point(labelX - 2, valueY);
            _lblSubtitle.Location = new Point(labelX + 2, Math.Max(valueY + 40, this.Height - 28));

            // ── TEXT ALIGNMENT ─────────────────────────────────────────────────────
            // WinForms mirrors ContentAlignment when a Label's RightToLeft == Yes.
            //   MiddleRight + RightToLeft.Yes → renders on the LEFT  (wrong)
            //   MiddleRight + RightToLeft.No  → renders on the RIGHT (correct)
            // We pin each label to RightToLeft.No so ContentAlignment.MiddleRight
            // always means "right edge of the label box" regardless of the parent
            // form's RTL setting.
            _lblTitle.RightToLeft    = RightToLeft.No;
            _lblValue.RightToLeft    = RightToLeft.No;
            _lblSubtitle.RightToLeft = RightToLeft.No;

            _lblTitle.TextAlign    = isRtl ? ContentAlignment.TopRight : ContentAlignment.TopLeft;
            _lblValue.TextAlign    = isRtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
            _lblSubtitle.TextAlign = isRtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
        }

        private bool _repositioning;

        private void RepositionCompact(bool isRtl)
        {
            if (this.Width < 8 || this.Height < 8) return;
            if (_repositioning) return;
            _repositioning = true;
            try { LayoutCompact(isRtl); }
            finally { _repositioning = false; }
        }

        private void LayoutCompact(bool isRtl)
        {
            const int edge = 14;
            const int iconSize = 36;
            const int gap = 10;

            _iconPanel.Size = new Size(iconSize, iconSize);
            int iconY = (Height - iconSize) / 2;
            _iconPanel.Location = isRtl
                ? new Point(edge, iconY)
                : new Point(Math.Max(edge, Width - iconSize - edge), iconY);

            int labelX = isRtl ? edge + iconSize + gap : edge;
            int labelWidth = Math.Max(28, Width - iconSize - edge * 2 - gap);

            FitValueFont(labelWidth);

            int titleH = TextRenderer.MeasureText("Ag", _lblTitle.Font).Height;
            int valueH = TextRenderer.MeasureText("0", _lblValue.Font).Height + 2;
            int top = Math.Max(4, (Height - (titleH + valueH)) / 2);

            _lblTitle.AutoSize = false;
            _lblValue.AutoSize = false;
            _lblTitle.Size = new Size(labelWidth, titleH);
            _lblValue.Size = new Size(labelWidth, valueH);
            _lblTitle.Location = new Point(labelX, top);
            _lblValue.Location = new Point(labelX, top + titleH);

            _lblTitle.RightToLeft = RightToLeft.No;
            _lblValue.RightToLeft = RightToLeft.No;
            _lblTitle.TextAlign = isRtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
            _lblValue.TextAlign = isRtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
            _lblTitle.AutoEllipsis = true;
        }

        // Shared across every card and never disposed: assigning a Label.Font raises
        // a resize, which re-enters the layout, so a per-instance font could be
        // disposed while still in use.
        private static readonly System.Collections.Generic.Dictionary<float, Font> _valueFonts = new();

        private static Font ValueFont(float size)
        {
            if (!_valueFonts.TryGetValue(size, out Font f))
            {
                f = new Font("Segoe UI", size, FontStyle.Bold);
                _valueFonts[size] = f;
            }
            return f;
        }

        private string _fittedText;
        private int _fittedWidth;

        /// <summary>
        /// Steps the value font down until the figure fits, so long amounts like
        /// "$12,345.67" shrink instead of being cut off.
        /// </summary>
        private void FitValueFont(int maxWidth)
        {
            string text = _lblValue.Text ?? "";
            if (maxWidth <= 0) return;
            if (text == _fittedText && maxWidth == _fittedWidth) return;

            _fittedText = text;
            _fittedWidth = maxWidth;

            Font chosen = ValueFont(11F);
            for (float size = 19F; size >= 11F; size -= 1F)
            {
                Font candidate = ValueFont(size);
                if (TextRenderer.MeasureText(text, candidate).Width <= maxWidth)
                {
                    chosen = candidate;
                    break;
                }
            }

            if (!ReferenceEquals(_lblValue.Font, chosen))
                _lblValue.Font = chosen;
        }

        // Call this after setting Value/Title to ensure labels move if their width changed
        public void FinalizeLayout()
        {
            RepositionForRTL();
        }

        private void UpdateTheme()
        {
            // if (_lblIcon != null) _lblIcon.ForeColor = _themeColor; // No longer needed
            if (_iconPanel != null) _iconPanel.Invalidate(); // Repaint background and icon
        }

        private void IconPanel_Paint(object sender, PaintEventArgs e)
        {
            // Draw Circle Background with High Quality
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            /* Circle background removed per user request
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(40, _themeColor))) // 40/255 Opacity
            {
                e.Graphics.FillEllipse(brush, 0, 0, _iconPanel.Width - 1, _iconPanel.Height - 1);
            }
            */

            // Draw Icon Image
            if (this.IconImage != null)
            {
                Rectangle imgRect = new Rectangle(IconPadding, IconPadding, _iconPanel.Width - (IconPadding * 2), _iconPanel.Height - (IconPadding * 2));
                e.Graphics.DrawImage(this.IconImage, imgRect);
            }
            // Fallback to text icon (emoji)
            else if (!string.IsNullOrEmpty(this.Icon))
            {
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    using (Brush brush = new SolidBrush(_themeColor))
                    {
                        RectangleF drawingRect = _iconPanel.ClientRectangle;
                        
                        // Targeted fix for Alert Icon which often visually renders lower than other emojis
                        if (this.Icon == "⚠️") 
                        {
                            drawingRect.Offset(0, -2); // Nudge up
                        }

                        // DrawString (GDI+)
                        e.Graphics.DrawString(this.Icon, new Font("Segoe UI Emoji", 18), brush, drawingRect, sf);
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            
            // 1. Clear background with parent color to solve "white corners bug"
            Color parentColor = ThemeConfig.GetParentColor(this);
            using (var brush = new SolidBrush(parentColor))
            {
                // Slightly larger rectangle to ensure no artifacts at edges/corners
                e.Graphics.FillRectangle(brush, -1, -1, this.Width + 2, this.Height + 2);
            }

            // Half-pixel inset keeps the 1px stroke centred on the path instead of
            // straddling the card edge, which is what made the corners look chipped.
            var r = new RectangleF(0.5f, 0.5f, this.Width - 1.5f, this.Height - 1.5f);
            using (var path = GetRoundedRect(r, _cornerRadius))
            {
                // 2. Fill rounded card with surface color
                using (var brush = new SolidBrush(ThemeConfig.SurfaceColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // 3. Draw border
                using (var pen = new Pen(ThemeConfig.BorderColor, 1))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        private GraphicsPath GetRoundedRect(RectangleF rect, int radius)
        {
            var path = new GraphicsPath();
            float d = Math.Min(radius * 2f, Math.Min(rect.Width, rect.Height));
            if (d <= 0f)
            {
                path.AddRectangle(rect);
                return path;
            }
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}


