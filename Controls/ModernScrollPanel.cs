using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventorySystem.Controls
{
    /// <summary>
    /// A clean, themed scrollable panel using standard WinForms scrolling for maximum stability.
    /// Drop-in replacement for Panel with AutoScroll.
    /// </summary>
    public class ModernScrollPanel : Panel
    {
        public ModernScrollPanel()
        {
            this.AutoScroll = true;
            this.DoubleBuffered = true;
            // Opaque: a transparent scroll surface cannot be erased by the scroll blit,
            // so every notch smeared the previous frame across the viewport.
            this.BackColor = InventorySystem.ThemeConfig.SurfaceColor;
        }

        // Composites the whole child tree off-screen, so the many transparent children
        // no longer repaint one-by-one over a scrolled surface.
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!AutoScroll || !VerticalScroll.Visible)
            {
                base.OnMouseWheel(e);
                return;
            }

            // Fixed-step scrolling: the default handler emits one scroll per wheel
            // detent multiplied by SystemInformation.MouseWheelScrollLines, which on
            // dense forms produced dozens of full-surface repaints per gesture.
            int step = SystemInformation.MouseWheelScrollDelta;
            int lines = e.Delta / step;
            int target = VerticalScroll.Value - lines * 60;
            target = Math.Max(VerticalScroll.Minimum, Math.Min(VerticalScroll.Maximum, target));

            var pos = AutoScrollPosition;
            AutoScrollPosition = new Point(-pos.X, target);

            if (e is HandledMouseEventArgs h) h.Handled = true;
        }

        // Maintain compatibility with legacy calls
        public void AddControl(Control c)
        {
            this.Controls.Add(c);
            c.BringToFront();
        }
    }
}
