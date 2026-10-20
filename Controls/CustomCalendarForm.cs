using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventorySystem.Controls
{
    public class CustomCalendarForm : Form
    {
        public DateTime? SelectedDate { get; private set; }
        public DateTime MinDate { get; set; } = DateTime.MinValue;
        private DateTime _currentViewDate;
        
        private Label lblMonthYear;
        private Button btnPrev;
        private Button btnNext;
        private FlowLayoutPanel pnlDays;
        
        public event EventHandler DateSelected;

        public CustomCalendarForm(DateTime? initialDate, DateTime minDate)
        {
            this.SelectedDate = initialDate;
            this.MinDate = minDate;
            this._currentViewDate = initialDate ?? DateTime.Today;
            
            InitializeComponent();
            RenderCalendar();
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(280, 300);
            this.BackColor = ThemeConfig.SurfaceColor;
            this.Padding = new Padding(10);
            
            // Shadow / Border Paint
            this.Paint += (s, e) => 
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen p = new Pen(ThemeConfig.BorderColor, 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
                }
            };


            // Header Panel
            Panel pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 35;
            pnlHeader.Margin = new Padding(0, 0, 0, 10);

            btnPrev = new Button { Text = "<", Width = 30, Dock = DockStyle.Left, FlatStyle = FlatStyle.Flat };
            btnPrev.FlatAppearance.BorderSize = 0;
            btnPrev.Click += (s, e) => { _currentViewDate = _currentViewDate.AddMonths(-1); RenderCalendar(); };
            
            btnNext = new Button { Text = ">", Width = 30, Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat };
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.Click += (s, e) => { _currentViewDate = _currentViewDate.AddMonths(1); RenderCalendar(); };

            Button btnClear = new Button { Text = "Clear", Width = 45, Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8), ForeColor = ThemeConfig.DangerColor };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) => { SelectedDate = null; DateSelected?.Invoke(this, EventArgs.Empty); this.Close(); };

            lblMonthYear = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            
            pnlHeader.Controls.Add(lblMonthYear);
            pnlHeader.Controls.Add(btnClear);
            pnlHeader.Controls.Add(btnNext);
            pnlHeader.Controls.Add(btnPrev);
            this.Controls.Add(pnlHeader);

            // Weekday Headers
            TableLayoutPanel tlpWeek = new TableLayoutPanel();
            tlpWeek.Dock = DockStyle.Top;
            tlpWeek.Height = 30;
            tlpWeek.ColumnCount = 7;
            tlpWeek.RowCount = 1;
            string[] days = { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" };
            for(int i=0; i<7; i++)
            {
                tlpWeek.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28f));
                Label l = new Label { Text = days[i], TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill, ForeColor = ThemeConfig.SecondaryColor, Font = new Font("Segoe UI", 8) };
                tlpWeek.Controls.Add(l, i, 0);
            }

            this.Controls.Add(tlpWeek);

            // Days Canvas
            pnlDays = new FlowLayoutPanel();
            pnlDays.Dock = DockStyle.Fill;
            pnlDays.Padding = new Padding(1, 5, 0, 0); // 1px left padding for perfect 260px centering
            this.Controls.Add(pnlDays);
        }

        private void RenderCalendar()
        {
            lblMonthYear.Text = _currentViewDate.ToString("MMMM yyyy");
            pnlDays.Controls.Clear();
            
            DateTime firstDayOfMonth = new DateTime(_currentViewDate.Year, _currentViewDate.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(_currentViewDate.Year, _currentViewDate.Month);
            int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek; // 0 = Sunday
            
            int btnSize = 35; 
            int margin = 1;

            // Empty slots
            for(int i=0; i<startDayOfWeek; i++)
            {
                Panel placeholder = new Panel { Size = new Size(btnSize, btnSize), Margin = new Padding(margin) };
                pnlDays.Controls.Add(placeholder);
            }

            for(int day=1; day<=daysInMonth; day++)
            {
                int currentDay = day;
                Button btn = new Button();
                btn.Text = day.ToString();
                btn.Size = new Size(btnSize, btnSize);
                btn.Margin = new Padding(margin);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Font = new Font("Segoe UI", 9);
                btn.Cursor = Cursors.Hand;
                
                DateTime btnDate = new DateTime(_currentViewDate.Year, _currentViewDate.Month, day);
                
                if (btnDate.Date < MinDate.Date)
                {
                    btn.BackColor = Color.FromArgb(240, 240, 240);
                    btn.ForeColor = ThemeConfig.MutedTextColor;
                    btn.Cursor = Cursors.Default;
                    btn.Enabled = false;
                }
                else if (SelectedDate.HasValue && btnDate.Date == SelectedDate.Value.Date)
                {
                    btn.BackColor = ThemeConfig.PrimaryColor;
                    btn.ForeColor = Color.White;
                }
                else if (btnDate.Date == DateTime.Today)
                {
                    btn.ForeColor = ThemeConfig.PrimaryColor;
                    btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
                else
                {
                    btn.BackColor = ThemeConfig.SurfaceColor;
                }

                btn.Click += (s, e) => 
                {
                    SelectedDate = btnDate;
                    DateSelected?.Invoke(this, EventArgs.Empty);
                    this.Close();
                };

                pnlDays.Controls.Add(btn);
            }
        }
        
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            this.Close();
        }
    }
}
