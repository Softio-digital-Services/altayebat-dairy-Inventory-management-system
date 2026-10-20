
namespace InventorySystem
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            // Main Container (Grid for Centering)
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panelLoginCard = new System.Windows.Forms.Panel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelSubtitle = new System.Windows.Forms.Label();
            this.txtUsername = new InventorySystem.Controls.ModernTextBox();
            this.txtPassword = new InventorySystem.Controls.ModernTextBox();
            this.btnLogin = new InventorySystem.Controls.ModernButton();
            this.chkShowPass = new System.Windows.Forms.CheckBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnMinimize = new System.Windows.Forms.Button();


            this.tableLayoutPanel1.SuspendLayout();
            this.panelLoginCard.SuspendLayout();
            this.SuspendLayout();

            // 
            // tableLayoutPanel1 (Background Container)
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 350F)); // Card Width
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.panelLoginCard, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 450F)); // Card Height
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.WhiteSmoke; // Default, overriden by Theme

            // 
            // panelLoginCard (The White Box)
            // 
            this.panelLoginCard.BackColor = System.Drawing.Color.White;
            this.panelLoginCard.Controls.Add(this.btnMinimize);
            this.panelLoginCard.Controls.Add(this.btnClose);

            this.panelLoginCard.Controls.Add(this.labelTitle);
            this.panelLoginCard.Controls.Add(this.labelSubtitle);
            this.panelLoginCard.Controls.Add(this.txtUsername);
            this.panelLoginCard.Controls.Add(this.txtPassword);
            this.panelLoginCard.Controls.Add(this.chkShowPass);
            this.panelLoginCard.Controls.Add(this.btnLogin);
            this.panelLoginCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLoginCard.Name = "panelLoginCard";
            // Optional: Add Padding/Margin if needed, but Grid handles it

            // 
            // btnClose (X)
            // 
            this.btnClose.Location = new System.Drawing.Point(300, 5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(45, 38);
            this.btnClose.Click += new System.EventHandler(this.label1_Click);
            this.btnClose.BringToFront();
            // 
            // btnMinimize (--)
            // 
            this.btnMinimize.Location = new System.Drawing.Point(255, 5);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(45, 38);
            this.btnMinimize.Click += (s, e) => this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.btnMinimize.BringToFront();


            // 
            // labelTitle (Sign In)
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.labelTitle.Location = new System.Drawing.Point(35, 40);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Text = "Sign In";

            // 
            // labelSubtitle
            // 
            this.labelSubtitle.AutoSize = true;
            this.labelSubtitle.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.labelSubtitle.ForeColor = System.Drawing.Color.Gray;
            this.labelSubtitle.Location = new System.Drawing.Point(35, 95); // Increased from 80
            this.labelSubtitle.Name = "labelSubtitle";
            this.labelSubtitle.Text = "Enter your credentials to access the system.";

            this.txtUsername.Location = new System.Drawing.Point(35, 140);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(280, 67);
            this.txtUsername.LabelText = "Username";
            this.txtUsername.IsRequired = true;


            this.txtPassword.Location = new System.Drawing.Point(35, 240);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(280, 67);
            this.txtPassword.LabelText = "Password";
            this.txtPassword.IsPassword = true;
            this.txtPassword.IsRequired = true;

            // 
            // chkShowPass
            // 
            this.chkShowPass.AutoSize = true;
            this.chkShowPass.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkShowPass.Location = new System.Drawing.Point(35, 285);
            this.chkShowPass.Name = "showPass";
            this.chkShowPass.Text = "Show Password";
            this.chkShowPass.CheckedChanged += new System.EventHandler(this.showPass_CheckedChanged);

            // 
            // btnLogin
            // 
            this.btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnLogin.ForeColor = System.Drawing.Color.White;
            this.btnLogin.Location = new System.Drawing.Point(35, 340);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(280, 45);
            this.btnLogin.Text = "LOGIN";
            this.btnLogin.Click += new System.EventHandler(this.loginBtn_Click);

            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panelLoginCard.ResumeLayout(false);
            this.panelLoginCard.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panelLoginCard;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelSubtitle;
        private InventorySystem.Controls.ModernTextBox txtUsername;
        private InventorySystem.Controls.ModernTextBox txtPassword;
        private InventorySystem.Controls.ModernButton btnLogin;
        private System.Windows.Forms.CheckBox chkShowPass;
        private System.Windows.Forms.Button btnMinimize;


    }
}

