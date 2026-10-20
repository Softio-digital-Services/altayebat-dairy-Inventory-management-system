using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using InventorySystem; // Correct namespace for ThemeConfig

namespace InventorySystem.Controls
{
    public class ModernPanel : Panel
    {
        public int BorderRadius { get; set; } = 30;
        public float GradientAngle { get; set; } = 90F;
        public Color GradientTopColor { get; set; } = Color.FromArgb(239, 246, 255); // Slight Blue Tint
        public Color GradientBottomColor { get; set; } = Color.White;

        public ModernPanel()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.ForeColor = ThemeConfig.TextColorDark;
            this.Size = new Size(350, 200);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            
            // 1. Clear background with PARENT color to solve white corners
            Color parentColor = ThemeConfig.GetParentColor(this);
            using (var brush = new SolidBrush(parentColor))
            {
                e.Graphics.FillRectangle(brush, -1, -1, this.Width + 2, this.Height + 2);
            }

            // 2. Draw Rounded Surface (Gradient) over the full bounds
            Rectangle bounds = new Rectangle(0, 0, this.Width, this.Height);
            if (bounds.Width > 1 && bounds.Height > 1)
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(bounds, this.GradientTopColor, this.GradientBottomColor, this.GradientAngle))
                using (GraphicsPath graphicsPath = ThemeConfig.GetRoundedPathF(new RectangleF(0, 0, bounds.Width, bounds.Height), BorderRadius))
                {
                    e.Graphics.FillPath(brush, graphicsPath);
                }
            }

            // 3. Draw Rounded Border inside the bounds so it stays unbroken
            ThemeConfig.DrawRoundedBorder(e.Graphics, bounds, BorderRadius, ThemeConfig.BorderColor, 1.5f);
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

