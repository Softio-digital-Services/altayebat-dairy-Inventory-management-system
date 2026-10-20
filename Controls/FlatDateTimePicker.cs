using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace InventorySystem.Controls
{
    // Replaces the previous inheritance from DateTimePicker to a full Custom Control
    public class FlatDateTimePicker : UserControl
    {
        private Label lblDate;
        private Panel pnlContainer;
        private DateTime? _value = null;

        public event EventHandler ValueChanged;

        public DateTime? Value
        {
            get => _value;
            set
            {
                _value = value;
                UpdateLabel();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public DateTime MinDate { get; set; } = DateTime.MinValue;

        public bool ShowCheckBox { get; set; } = false;

        public bool Checked
        {
            get => _value.HasValue;
            set
            {
                if (value && !_value.HasValue) _value = DateTime.Today;
                else if (!value) _value = null;
                UpdateLabel();
            }
        }

        // Maintain compatibility with POsForm code
        public DateTimePickerFormat Format { get; set; } = DateTimePickerFormat.Short;
        
        // Hide standard properties we don't use but exist on UserControl
        // to prevent confusion
        
        public FlatDateTimePicker()
        {
            this.Size = new Size(200, 35); 
            this.Padding = new Padding(0);
            this.BackColor = Color.Transparent;
            if (Helpers.LocalizationManager.IsArabic) this.RightToLeft = RightToLeft.Yes;
            
            InitializeControls();
            Helpers.LocalizationManager.LanguageChanged += (s, e) =>
            {
                if (Helpers.LocalizationManager.IsArabic) this.RightToLeft = RightToLeft.Yes;
                else this.RightToLeft = RightToLeft.No;
                UpdateLabel();
            };
        }

        private void InitializeControls()
        {
            pnlContainer = new Panel();
            pnlContainer.Dock = DockStyle.Fill;
            pnlContainer.Padding = new Padding(10, 5, 5, 5);
            pnlContainer.Cursor = Cursors.Hand;
            pnlContainer.Click += OpenCalendar;
            
            lblDate = new Label();
            lblDate.Dock = DockStyle.Fill;
            lblDate.TextAlign = ContentAlignment.MiddleLeft;
            lblDate.Font = new Font("Segoe UI", 10F);
            lblDate.Text = _value.HasValue
                ? _value.Value.ToShortDateString()
                : Helpers.LocalizationManager.GetString("Common_NoDate", "No Date");
            lblDate.Click += OpenCalendar;
            
            Label lblIcon = new Label();
            lblIcon.Text = "📅";
            lblIcon.Dock = DockStyle.Right;
            lblIcon.Width = 30;
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblIcon.Font = new Font("Segoe UI Emoji", 10);
            lblIcon.ForeColor = ThemeConfig.SecondaryColor;
            lblIcon.Click += OpenCalendar;

            pnlContainer.Controls.Add(lblDate);
            pnlContainer.Controls.Add(lblIcon);
            pnlContainer.Paint += PnlContainer_Paint;
            
            this.Controls.Add(pnlContainer);
        }

        private void PnlContainer_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color parentColor = ThemeConfig.GetParentColor(this);
            using (var brush = new SolidBrush(parentColor))
                e.Graphics.FillRectangle(brush, -1, -1, pnlContainer.Width + 2, pnlContainer.Height + 2);

            Rectangle bounds = new Rectangle(0, 0, pnlContainer.Width, pnlContainer.Height);
            ThemeConfig.FillRoundedBackground(e.Graphics, bounds, 12f, ThemeConfig.SurfaceColor);
            ThemeConfig.DrawRoundedBorder(e.Graphics, bounds, 12f, ThemeConfig.BorderColor, 1.5f);
        }

        private void OpenCalendar(object sender, EventArgs e)
        {
            CustomCalendarForm calendar = new CustomCalendarForm(Value, MinDate);
            
            int xOffset = (this.Width - calendar.Width) / 2;
            Point screenPoint = this.PointToScreen(new Point(xOffset, this.Height));
            
            var screenBounds = Screen.FromPoint(screenPoint).WorkingArea;
            if (screenPoint.Y + calendar.Height > screenBounds.Bottom)
            {
                screenPoint.Y = this.PointToScreen(new Point(xOffset, -calendar.Height)).Y;
            }
            
            calendar.Location = screenPoint;
            calendar.DateSelected += (s, args) => 
            {
                this.Value = calendar.SelectedDate;
            };
            calendar.Show();
        }

        private void UpdateLabel()
        {
            if (lblDate != null)
                lblDate.Text = _value.HasValue
                    ? _value.Value.ToShortDateString()
                    : Helpers.LocalizationManager.GetString("Common_NoDate", "No Date");
        }
    }
}

