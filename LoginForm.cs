using System;
using Microsoft.Data.Sqlite;
using System.Windows.Forms;
using System.Drawing;
using InventorySystem.Helpers;

namespace InventorySystem
{
    public partial class LoginForm : Form
    {
        private bool _isDragging = false;
        private Point _dragStartPoint = Point.Empty;
        private EventHandler _languageChangedHandler;

        public LoginForm()
        {
            InitializeComponent();
            ApplyTheme();
            ThemeConfig.ApplyFormIcon(this);
            _languageChangedHandler = (s, e) =>
            {
                if (!IsDisposed && Visible) ApplyLocalization();
            };
            LocalizationManager.LanguageChanged += _languageChangedHandler;
            ApplyLocalization();
            SetupDragging();
        }

        protected override void WndProc(ref Message m)
        {
            BorderlessFormHelper.HandleGetMinMaxInfo(this, ref m);
            base.WndProc(ref m);
        }

        private void SetupDragging()
        {
            // Attach dragging to form background and main panels
            this.MouseDown += OnDraggingMouseDown;
            this.MouseMove += OnDraggingMouseMove;
            this.MouseUp += OnDraggingMouseUp;

            if (tableLayoutPanel1 != null)
            {
                tableLayoutPanel1.MouseDown += OnDraggingMouseDown;
                tableLayoutPanel1.MouseMove += OnDraggingMouseMove;
                tableLayoutPanel1.MouseUp += OnDraggingMouseUp;
            }

            if (panelLoginCard != null)
            {
                panelLoginCard.MouseDown += OnDraggingMouseDown;
                panelLoginCard.MouseMove += OnDraggingMouseMove;
                panelLoginCard.MouseUp += OnDraggingMouseUp;

                // Also allow dragging from labels
                labelTitle.MouseDown += OnDraggingMouseDown;
                labelTitle.MouseMove += OnDraggingMouseMove;
                labelTitle.MouseUp += OnDraggingMouseUp;

                labelSubtitle.MouseDown += OnDraggingMouseDown;
                labelSubtitle.MouseMove += OnDraggingMouseMove;
                labelSubtitle.MouseUp += OnDraggingMouseUp;
            }
        }

        private void OnDraggingMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _dragStartPoint = new Point(e.X, e.Y);
            }
        }

        private void OnDraggingMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point p = PointToScreen(e.Location);
                this.Location = new Point(p.X - _dragStartPoint.X, p.Y - _dragStartPoint.Y);
            }
        }

        private void OnDraggingMouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            Func<string, string> L = LocalizationManager.GetString;
            bool isAr = LocalizationManager.IsArabic;

            labelTitle.Text    = L("Login_Title");
            labelSubtitle.Text = L("Login_Subtitle");
            txtUsername.LabelText = L("Login_Username");
            txtPassword.LabelText = L("Login_Password");
            chkShowPass.Text   = L("Login_ShowPassword");
            btnLogin.Text      = L("Login_Button");

            // RTL support
            this.RightToLeft        = isAr ? RightToLeft.Yes : RightToLeft.No;
            panelLoginCard.RightToLeft = isAr ? RightToLeft.Yes : RightToLeft.No;
            
            // Center Titles
            labelTitle.AutoSize = false;
            labelTitle.Width = panelLoginCard.Width;
            labelTitle.TextAlign = ContentAlignment.MiddleCenter;
            labelTitle.Left = 0;

            labelSubtitle.AutoSize = false;
            labelSubtitle.Width = panelLoginCard.Width;
            labelSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            labelSubtitle.Left = 0;
        }

        private void ApplyTheme()
        {
            // Background
            this.tableLayoutPanel1.BackColor = ThemeConfig.BackgroundColor; 
            
            // Brand name
            Label lblBrand = new Label
            {
                Text = ThemeConfig.CompanyName,
                AutoSize = false,
                Size = new Size(200, 48),
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = ThemeConfig.PrimaryColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top
            };
            panelLoginCard.Controls.Add(lblBrand);

            panelLoginCard.Resize += (s, e) => {
                lblBrand.Location = new Point((panelLoginCard.Width - lblBrand.Width) / 2, 36);
            };
            lblBrand.Location = new Point((panelLoginCard.Width - lblBrand.Width) / 2, 36);

            labelTitle.Top = 145;
            labelSubtitle.Top = 195;
            
            txtUsername.Top = 235;
            txtPassword.Top = 315;
            btnLogin.Top = 410;
            
            chkShowPass.Visible = false; // Hide old checkbox
            
            // Labels
            labelTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            
            // Window Controls
            ThemeConfig.ApplyWindowControl(btnClose, "Close");
            ThemeConfig.ApplyWindowControl(btnMinimize, "Minimize");
            labelTitle.ForeColor = ThemeConfig.PrimaryColor; 
            
            labelSubtitle.Font = ThemeConfig.StandardFont;
            labelSubtitle.ForeColor = ThemeConfig.SecondaryColor;

            // Keyboard Navigation
            txtUsername.KeyDown += txtUsername_KeyDown;
            txtPassword.KeyDown += txtPassword_KeyDown;

            // Close and Minimize Buttons
            btnClose.ForeColor = ThemeConfig.SecondaryColor;
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = ThemeConfig.DangerColor;
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = ThemeConfig.SecondaryColor;

            btnMinimize.ForeColor = ThemeConfig.SecondaryColor;
            btnMinimize.MouseEnter += (s, e) => btnMinimize.ForeColor = ThemeConfig.PrimaryColor;
            btnMinimize.MouseLeave += (s, e) => btnMinimize.ForeColor = ThemeConfig.SecondaryColor;

            // Rounded Corners for Card
            panelLoginCard.Resize += (s, e) => 
            {
                int radius = 20; 
                using (System.Drawing.Drawing2D.GraphicsPath path = GetRoundedPath(panelLoginCard.ClientRectangle, radius))
                {
                    panelLoginCard.Region = new Region(path);
                }
            };

            // Adjust card height
            if(tableLayoutPanel1.RowStyles.Count >= 2)
            {
                tableLayoutPanel1.RowStyles[1].Height = 550F;
            }

            panelLoginCard.BackColor = ThemeConfig.SurfaceColor;
            panelLoginCard.PerformLayout();
            using (System.Drawing.Drawing2D.GraphicsPath path = GetRoundedPath(panelLoginCard.ClientRectangle, 20))
            {
                panelLoginCard.Region = new Region(path);
            }
        }

        private System.Drawing.Drawing2D.GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            float r = radius;
            path.AddArc(rect.X, rect.Y, r, r, 180, 90);
            path.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
            path.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnLoginSucceeded(MainForm mainForm)
        {
            if (_languageChangedHandler != null)
                LocalizationManager.LanguageChanged -= _languageChangedHandler;
            mainForm.FormClosed += (s, e) => Application.Exit();
            mainForm.Show();
            Hide();
        }

        private void loginBtn_Click(object sender, EventArgs e)
        {
            if (!ValidationHelper.ValidateRequiredFields(txtUsername, txtPassword))
            {
                return;
            }

            try
            {
                if (txtUsername.Text.Trim() == "Softio.Admin" && txtPassword.Text.Trim() == "Softio@2026!")
                {
                    UserSession.Username = "Softio.Admin";
                    UserSession.FullName = "Softio Super Admin";
                    UserSession.Role = "Admin";

                    MainForm mForm = new MainForm();
                    OnLoginSucceeded(mForm);
                    return;
                }

                string sql = "SELECT username, full_name, role FROM users WHERE username = @username AND password = @password";
                var parameters = new SqliteParameter[]
                {
                    new SqliteParameter("@username", txtUsername.Text.Trim()),
                    new SqliteParameter("@password", txtPassword.Text.Trim())
                };

                using (var dt = DatabaseHelper.ExecuteDataTable(sql, parameters))
                {
                    if (dt.Rows.Count > 0)
                    {
                        var row = dt.Rows[0];
                        UserSession.Username = row["username"].ToString();
                        UserSession.FullName = row["full_name"].ToString();
                        UserSession.Role = row["role"].ToString();

                        MainForm mForm = new MainForm();
                        OnLoginSucceeded(mForm);
                    }
                    else
                    {
                        MessageHelper.ShowError(LocalizationManager.GetString("Login_Error"));
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "user login");
                MessageHelper.ShowDatabaseError(LocalizationManager.GetString("Msg_LoggingIn"));
            }
        }

        private void showPass_CheckedChanged(object sender, EventArgs e)
        {
            // chkShowPass is hidden, ModernTextBox handles it via eye icon now
        }

        private void txtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down)
            {
                txtPassword.Focus();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                loginBtn_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                txtUsername.Focus();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
    }
}
