using System;
using System.Windows.Forms;

namespace InventorySystem.Controls
{
    public class SidebarButton : Button
    {
        public SidebarButton()
        {
            this.SetStyle(ControlStyles.Selectable, false); // Optional: if focus preservation isn't critical for sidebar nav
            // Actually, keep Selectable=true for navigation accessibility, but suppress cues.
            this.SetStyle(ControlStyles.Selectable, true);
        }

        protected override bool ShowFocusCues => false;

        public override void NotifyDefault(bool value)
        {
            base.NotifyDefault(false);
        }
        
        // No OnPaint override. Let Windows handle FlatAppearance and Text/Image rendering perfectly.
    }
}
