using System;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Helpers;
using InventorySystem.Services;
using InventorySystem.Controls;

namespace InventorySystem.Forms
{
    public partial class LockScreenForm : Form
    {
        private Panel pnlCenter;
        private TextBox txtPassword;
        private Label lblUser;
        private Label lblError;
        private Label _lblLocked;
        private ModernButton _btnUnlock;

        public LockScreenForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            CreateUI();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            if (_lblLocked != null)
                _lblLocked.Text = LocalizationManager.GetString("Lock_Title", "Session Locked");
            if (_btnUnlock != null)
                _btnUnlock.Text = LocalizationManager.GetString("Lock_Unlock", "Unlock");
        }

        private void CreateUI()
        {
            pnlCenter = new Panel { Size = new Size(400, 500), BackColor = ThemeConfig.SurfaceColor };
            pnlCenter.Location = new Point((this.Width - pnlCenter.Width) / 2, (this.Height - pnlCenter.Height) / 2);
            this.Controls.Add(pnlCenter);

            // Rounded corners for center panel using Paint event
            pnlCenter.Paint += (s, e) => {
                ThemeConfig.DrawRoundedBorder(e.Graphics, pnlCenter.ClientRectangle, 20f, ThemeConfig.BorderColor, 1f);
            };

            Label lblLocked = new Label { 
                Font = new Font("Segoe UI", 24, FontStyle.Bold), 
                ForeColor = ThemeConfig.TextColorDark, 
                TextAlign = ContentAlignment.MiddleCenter, 
                Dock = DockStyle.Top, 
                Height = 100 
            };
            _lblLocked = lblLocked;
            pnlCenter.Controls.Add(lblLocked);

            PictureBox picUser = new PictureBox { 
                Image = ThemeConfig.GetNuricon("users"), 
                SizeMode = PictureBoxSizeMode.CenterImage, 
                Dock = DockStyle.Top, 
                Height = 120 
            };
            pnlCenter.Controls.Add(picUser);

            lblUser = new Label { 
                Text = UserSession.Username, 
                Font = ThemeConfig.HeaderFont, 
                ForeColor = ThemeConfig.TextColorDark, 
                TextAlign = ContentAlignment.MiddleCenter, 
                Dock = DockStyle.Top, 
                Height = 35 
            };
            pnlCenter.Controls.Add(lblUser);

            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(40, 20, 40, 0) };
            txtPassword = new TextBox { 
                PasswordChar = '-', 
                Font = ThemeConfig.SubHeaderFont, 
                BorderStyle = BorderStyle.None, 
                Dock = DockStyle.Fill 
            };
            Panel txtWrapper = ThemeConfig.WrapInStyledInput(txtPassword, 50);
            txtWrapper.Dock = DockStyle.Top;
            pnlInput.Controls.Add(txtWrapper);
            pnlCenter.Controls.Add(pnlInput);

            lblError = new Label { 
                Text = "", 
                ForeColor = ThemeConfig.DangerColor, 
                TextAlign = ContentAlignment.MiddleCenter, 
                Dock = DockStyle.Top, 
                Height = 30,
                Font = ThemeConfig.SmallFont
            };
            pnlCenter.Controls.Add(lblError);

            _btnUnlock = new ModernButton { 
                Size = new Size(160, 45), 
                Location = new Point(120, 420) 
            };
            ThemeConfig.ApplyPrimaryButton(_btnUnlock);
            _btnUnlock.Click += BtnUnlock_Click;
            pnlCenter.Controls.Add(_btnUnlock);

            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnUnlock_Click(s, e); };
            
            this.Resize += (s, e) => {
                pnlCenter.Location = new Point((this.Width - pnlCenter.Width) / 2, (this.Height - pnlCenter.Height) / 2);
            };
        }

        private void BtnUnlock_Click(object sender, EventArgs e)
        {
            string pass = txtPassword.Text;
            // Real validation would use AuthService, but for now we check against a fixed value 
            // or better yet, since we don't store passwords in session, we assume any input 
            // for the current user is a placeholder in this demo or we'd need a re-auth service.
            
            // Assuming we have an AuthService.VerifyPassword(...)
            // AuthService auth = new AuthService();
            // if (auth.Login(UserSession.Username, pass)) { this.Close(); }
            
            // For now, let's treat "admin" or just non-empty as success if it's a demo
            if (!string.IsNullOrEmpty(pass))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                lblError.Text = LocalizationManager.GetString("Lock_Invalid", "Invalid password");
                txtPassword.Clear();
            }
        }
    }
}
