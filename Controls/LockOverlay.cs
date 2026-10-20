using System;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Helpers;
using InventorySystem.Controls;

namespace InventorySystem.Controls
{
    public class LockOverlay : UserControl
    {
        private Panel pnlCard;
        private TextBox txtPassword;
        private Label lblError;
        public event EventHandler Unlocked;

        public LockOverlay()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(180, ThemeConfig.BackgroundColor); // Semi-transparent dark overlay
            this.DoubleBuffered = true;
            
            InitializeUI();
        }

        private void InitializeUI()
        {
            pnlCard = new Panel { Size = new Size(380, 420), BackColor = ThemeConfig.SurfaceColor };
            this.Controls.Add(pnlCard);

            // Center the card
            CenterCard();
            this.Resize += (s, e) => CenterCard();

            // Rounded Corners & Shadow Effect
            pnlCard.Paint += (s, e) => {
                ThemeConfig.DrawRoundedBorder(e.Graphics, pnlCard.ClientRectangle, 24f, ThemeConfig.BorderColor, 1f);
            };

            // Icon
            PictureBox picLock = new PictureBox {
                Image = ThemeConfig.GetNuricon("lock"),
                Size = new Size(64, 64),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point((pnlCard.Width - 64) / 2, 40)
            };
            pnlCard.Controls.Add(picLock);

            // Title
            Label lblTitle = new Label {
                Text = LocalizationManager.GetString("Lock_Title", "Session Locked"),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 120),
                Size = new Size(pnlCard.Width - 40, 35)
            };
            pnlCard.Controls.Add(lblTitle);

            // Username
            Label lblUser = new Label {
                Text = UserSession.FullName ?? UserSession.Username,
                Font = ThemeConfig.SubHeaderFont,
                ForeColor = ThemeConfig.SecondaryColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 165),
                Size = new Size(pnlCard.Width - 40, 25)
            };
            pnlCard.Controls.Add(lblUser);

            // Password Field Label
            Label lblPassLabel = new Label {
                Text = "Enter Password",
                Font = ThemeConfig.SmallBoldFont,
                ForeColor = ThemeConfig.TextColorDark,
                Location = new Point(45, 210),
                AutoSize = true
            };
            pnlCard.Controls.Add(lblPassLabel);

            // Password Input
            txtPassword = new TextBox {
                PasswordChar = '\u25CF',
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.None,
                Width = pnlCard.Width - 90
            };
            Panel txtWrapper = ThemeConfig.WrapInStyledInput(txtPassword, 45);
            txtWrapper.Location = new Point(40, 235);
            txtWrapper.Width = pnlCard.Width - 80;
            pnlCard.Controls.Add(txtWrapper);

            // Error Label
            lblError = new Label {
                Text = "",
                ForeColor = ThemeConfig.DangerColor,
                Font = ThemeConfig.SmallFont,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(40, 285),
                Size = new Size(pnlCard.Width - 80, 20)
            };
            pnlCard.Controls.Add(lblError);

            // Unlock Button
            Button btnUnlock = new ModernButton {
                Text = LocalizationManager.GetString("Lock_Unlock", "Unlock Session"),
                Size = new Size(pnlCard.Width - 80, 50),
                Location = new Point(40, 320)
            };
            ThemeConfig.ApplyPrimaryButton(btnUnlock);
            btnUnlock.Click += (s, e) => AttemptUnlock();
            pnlCard.Controls.Add(btnUnlock);

            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) AttemptUnlock(); };
            
            // Focus password box when shown
            this.VisibleChanged += (s, e) => { if (this.Visible) txtPassword.Focus(); };
        }

        private void CenterCard()
        {
            if (pnlCard != null)
                pnlCard.Location = new Point((this.Width - pnlCard.Width) / 2, (this.Height - pnlCard.Height) / 2);
        }

        private void AttemptUnlock()
        {
            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                lblError.Text = "Please enter your password.";
                return;
            }

            // In a real app, we'd verify against the DB. 
            // For now, any non-empty password "unlocks" as per previous LockScreenForm logic.
            // But let's add a small delay for "premium" feel.
            lblError.Text = "";
            txtPassword.Enabled = false;
            
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer { Interval = 300 };
            t.Tick += (s, e) => {
                t.Stop();
                t.Dispose();
                Unlocked?.Invoke(this, EventArgs.Empty);
            };
            t.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Draw semi-transparent background
            using (var b = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillRectangle(b, this.ClientRectangle);
            }
            base.OnPaint(e);
        }
    }
}
