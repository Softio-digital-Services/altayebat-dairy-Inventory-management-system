using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using InventorySystem.Helpers;
using InventorySystem.Helpers.Plugins;

namespace InventorySystem.Plugins
{
    /// <summary>
    /// Free built-in plugin -- adds a full-featured calculator to the sidebar.
    /// RequiresLicense = false -> always visible for all license types.
    /// </summary>
    public class CalculatorPlugin : ITabPlugin
    {
        public string Id          => "com.carparts.calculator";
        public string Name        => "Calculator";
        public string Version     => "1.0.0";
        public string Description => "Full-featured in-app calculator";
        public string Author      => "Car Parts Inventory System";

        public bool   RequiresLicense   => false;
        public string LicenseFeatureKey => "";

        public string TabId    => "btnCalculator";
        public string TabTitle => LocalizationManager.GetString("Plugins_CalcTitle", "Calculator");
        public string TabIcon  => "calculator";
        public int    TabOrder => 110;

        private PluginContext _ctx;

        public void Initialize(PluginContext context) => _ctx = context;
        public void Shutdown() { }

        public UserControl CreateTabContent() => new CalculatorPanel();
    }

    /// <summary>The calculator UI panel.</summary>
    public class CalculatorPanel : UserControl
    {
        private TextBox _display;
        private string  _current  = "0";
        private string  _operator = "";
        private double  _prev     = 0;
        private bool    _newEntry = true;

        public CalculatorPanel()
        {
            this.BackColor = ThemeConfig.SurfaceColor;
            this.Dock      = DockStyle.Fill;
            Build();

            // Enable Keyboard Support and fix modal size
            this.Load += (s, e) => {
                var form = this.FindForm();
                if (form != null) {
                    form.KeyPreview = true;
                    form.KeyDown += (sf, ef) => HandleKeyDown(ef);
                    
                    if (form is InventorySystem.Forms.BaseModalForm modal)
                    {
                        modal.EnforceMinWidth = false;
                        modal.Width = 400; // Small size for calculator
                        modal.FitToContent();
                    }
                }
            };
        }

        private void HandleKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) PressButton((e.KeyCode - Keys.D0).ToString());
            else if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9) PressButton((e.KeyCode - Keys.NumPad0).ToString());
            else if (e.KeyCode == Keys.Add || (e.Shift && e.KeyCode == Keys.Oemplus)) PressButton("+");
            else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus) PressButton("\u2212");
            else if (e.KeyCode == Keys.Multiply) PressButton("\u00d7");
            else if (e.KeyCode == Keys.Divide || e.KeyCode == Keys.OemQuestion) PressButton("\u00f7");
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Oemplus) PressButton("=");
            else if (e.KeyCode == Keys.Back) PressButton("\u232b");
            else if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Delete) PressButton("C");
            else if (e.KeyCode == Keys.Decimal || e.KeyCode == Keys.OemPeriod) PressButton(".");
        }

        private void Build()
        {
            this.BackColor = ThemeConfig.SurfaceColor;

            Panel container = new Panel();
            container.Size = new Size(340, 440);
            this.Resize += (s, e) => {
                container.Location = new Point((this.Width - container.Width) / 2, (this.Height - container.Height) / 2);
            };
            this.Controls.Add(container);

            // Display
            _display = new TextBox();
            _display.ReadOnly    = true;
            _display.TabStop     = false;
            _display.Text        = "0";
            _display.Font        = new Font("Segoe UI", 28, FontStyle.Bold);
            _display.TextAlign   = HorizontalAlignment.Right;
            _display.BorderStyle = BorderStyle.None;
            _display.BackColor   = ThemeConfig.SurfaceColor;
            _display.ForeColor   = ThemeConfig.TextColorDark;
            _display.Location    = new Point(10, 15);
            _display.Size        = new Size(320, 55);
            _display.GotFocus   += (s, e) => { _display.SelectionLength = 0; this.Focus(); };
            container.Controls.Add(_display);

            // Separator
            Panel sep = new Panel { BackColor = ThemeConfig.BorderColor, Location = new Point(10, 75), Size = new Size(320, 1) };
            container.Controls.Add(sep);

            // Button grid
            string[][] rows =
            {
                new[] { "C", "\u00b1", "%", "\u00f7" },
                new[] { "7", "8", "9", "\u00d7" },
                new[] { "4", "5", "6", "\u2212" },
                new[] { "1", "2", "3", "+" },
                new[] { "0", ".", "\u232b", "=" }
            };

            string[] operators = { "\u00f7", "\u00d7", "\u2212", "+" };

            int bw = 72, bh = 58, gap = 8;
            int startX = 14, startY = 88;

            for (int r = 0; r < rows.Length; r++)
            {
                for (int c = 0; c < rows[r].Length; c++)
                {
                    string label = rows[r][c];
                    Button btn = new Button();
                    btn.Text      = label;
                    btn.Size      = new Size(bw, bh);
                    btn.Location  = new Point(startX + c * (bw + gap), startY + r * (bh + gap));
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.TabStop   = false;
                    btn.Font      = new Font("Segoe UI", 14, FontStyle.Regular);
                    btn.Cursor    = Cursors.Hand;

                    if (label == "=")
                    {
                        btn.BackColor = ThemeConfig.PrimaryColor;
                        btn.ForeColor = Color.White;
                    }
                    else if (label == "C")
                    {
                        btn.BackColor = ThemeConfig.DangerColor;
                        btn.ForeColor = Color.White;
                    }
                    else if (operators.Contains(label))
                    {
                        btn.BackColor = ThemeConfig.ActiveBackColor;
                        btn.ForeColor = ThemeConfig.PrimaryColor;
                        btn.Font      = new Font("Segoe UI", 16, FontStyle.Bold);
                    }
                    else
                    {
                        btn.BackColor = ThemeConfig.SurfaceColor;
                        btn.ForeColor = ThemeConfig.TextColorDark;
                        btn.FlatAppearance.BorderColor = ThemeConfig.BorderColor;
                        btn.FlatAppearance.BorderSize  = 1;
                    }

                    DrawRounded(btn);

                    string captured = label;
                    btn.Click += (s, e) => PressButton(captured);
                    container.Controls.Add(btn);
                }
            }
        }

        private void PressButton(string key)
        {
            string[] operators = { "\u00f7", "\u00d7", "\u2212", "+" };
            switch (key)
            {
                case "C":
                    _current = "0"; _prev = 0; _operator = ""; _newEntry = true;
                    break;
                case "\u232b": // backspace
                    if (_current == "Error" || _current == "NaN") _current = "0";
                    else if (_current.Length > 1) {
                        _current = _current.Substring(0, _current.Length - 1);
                        if (_current == "-") _current = "0";
                    }
                    else _current = "0";
                    break;
                case "\u00b1": // plus-minus
                    if (double.TryParse(_current, out double neg))
                        _current = (-neg).ToString();
                    break;
                case "%":
                    if (double.TryParse(_current, out double pct))
                        _current = (pct / 100).ToString();
                    break;
                case "=":
                    Compute();
                    _operator = "";
                    _newEntry = true;
                    break;
                case ".":
                    if (_newEntry) { _current = "0."; _newEntry = false; }
                    else if (!_current.Contains(".")) _current += ".";
                    break;
                default:
                    if (operators.Contains(key))
                    {
                        if (double.TryParse(_current, out double val))
                        {
                            if (!_newEntry && !string.IsNullOrEmpty(_operator)) Compute();
                            _prev     = double.Parse(_current);
                            _operator = key;
                            _newEntry = true;
                        }
                    }
                    else // digit
                    {
                        if (_newEntry) { _current = key; _newEntry = false; }
                        else _current = (_current == "0") ? key : _current + key;
                    }
                    break;
            }
            _display.Text = _current;
        }

        private void Compute()
        {
            if (string.IsNullOrEmpty(_operator)) return;
            if (!double.TryParse(_current, out double b)) return;
            
            double result = _prev;
            try {
                if      (_operator == "\u00f7") result = b != 0 ? _prev / b : double.NaN;
                else if (_operator == "\u00d7") result = _prev * b;
                else if (_operator == "\u2212") result = _prev - b;
                else if (_operator == "+")      result = _prev + b;
                
                if (double.IsNaN(result)) _current = "Error";
                else _current = result % 1 == 0 ? ((long)result).ToString() : result.ToString("G10");
            } catch {
                _current = "Error";
            }
        }

        private void DrawRounded(Button btn)
        {
            btn.Paint += (s, e) =>
            {
                ThemeConfig.DrawRoundedButton(btn, e.Graphics);
            };
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundRect(Rectangle r, int rad)
        {
            var p = new System.Drawing.Drawing2D.GraphicsPath();
            p.AddArc(r.X, r.Y, rad, rad, 180, 90);
            p.AddArc(r.Right - rad, r.Y, rad, rad, 270, 90);
            p.AddArc(r.Right - rad, r.Bottom - rad, rad, rad, 0, 90);
            p.AddArc(r.X, r.Bottom - rad, rad, rad, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}

