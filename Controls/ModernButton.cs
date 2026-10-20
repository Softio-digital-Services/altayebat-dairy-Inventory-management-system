using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventorySystem.Controls
{
    public class ModernButton : Button
    {
        public ModernButton()
        {
            // We set UserPaint to true to take full control
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Cursor = Cursors.Hand;
            this.BackColor = ThemeConfig.PrimaryColor;
            this.ForeColor = Color.White;
            this.Font = ThemeConfig.ButtonFont;
            this.Size = new Size(150, 35);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }

        protected override bool ShowFocusCues => false;

        protected override void OnPaint(PaintEventArgs pevent)
        {
            // Never call base.OnPaint! It triggers native Windows Forms drawing which 
            // draws a flat rectangle and overwrites our modern rounded corners.
            // ThemeConfig.DrawRoundedButton now handles all buttons, including special icon buttons.
            ThemeConfig.DrawRoundedButton(this, pevent.Graphics);
        }

        public override void NotifyDefault(bool value)
        {
            base.NotifyDefault(false);
        }
    }
}
