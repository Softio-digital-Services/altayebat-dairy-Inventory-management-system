using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Data;
using InventorySystem.Controls;
using InventorySystem.Helpers;

namespace InventorySystem.Forms
{
    public partial class UsersForm : UserControl
    {
        private DataGridView dgvUsers;
        private Button btnAddUser;
        private Label lblUsersTitle;
        private ModernTextBox txtSearch;

        public UsersForm()
        {
            InitializeComponent();
            ApplyTheme();
            LoadData();
            ApplyLocalization();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Size = new Size(1100, 750);


            // Main Layout
            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(20) };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.Controls.Add(mainLayout);

            // Header
            lblUsersTitle = ThemeConfig.CreateStandardHeader(LocalizationManager.GetString("Msg_UserManagement"));
            lblUsersTitle.Name = "lblUsersTitle";

            txtSearch = new ModernTextBox { 
                IsSearch = true, 
                ShowLabel = false, 
                PlaceholderText = LocalizationManager.GetString("Msg_SearchUsers", "Search users..."), 
                Size = new Size(320, 35)
            };
            txtSearch.TextChanged += (s, e) => LoadData(txtSearch.Text);

            btnAddUser = new Button { Size = new Size(160, 35) };
            btnAddUser.Click += btnAddUser_Click;
            ThemeConfig.ApplyStandardAddButton(btnAddUser, "User_AddUser");

            var actionButtons = new Control[] { btnAddUser };
            TableLayoutPanel tlpHeader = ThemeConfig.CreateGlobalFormHeader(lblUsersTitle, txtSearch, actionButtons);
            mainLayout.Controls.Add(tlpHeader, 0, 0);

            // 2. Grid
            dgvUsers = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = ThemeConfig.SurfaceColor, BorderStyle = BorderStyle.None, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, RowHeadersVisible = false, AutoGenerateColumns = false, AllowUserToAddRows = false, ReadOnly = true };
            dgvUsers.DataError += (s, e) => e.ThrowException = false;
            dgvUsers.CellClick += DgvUsers_CellClick;
            dgvUsers.CellMouseMove += DgvUsers_CellMouseMove;
            dgvUsers.CellMouseLeave += DgvUsers_CellMouseLeave;
            dgvUsers.CellPainting += DgvUsers_CellPainting;

            dgvUsers.CellFormatting += (s, e) => {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                string col = dgvUsers.Columns[e.ColumnIndex].Name;
                if (col == "id")
                {
                    e.Value = (e.RowIndex + 1).ToString();
                    e.FormattingApplied = true;
                }
                else if (col == "role" && e.Value != null)
                {
                    string roleKey = "Role_" + e.Value.ToString().Replace(" ", "");
                    e.Value = LocalizationManager.GetString(roleKey, e.Value.ToString());
                    e.FormattingApplied = true;
                }
            };

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "db_id", DataPropertyName = "id", Visible = false });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "id", HeaderText = LocalizationManager.GetString("Users_GridID"), Width = 80 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "username", HeaderText = LocalizationManager.GetString("Users_GridUsername", "Username"), DataPropertyName = "username", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "role", HeaderText = LocalizationManager.GetString("Users_GridRole", "Role"), DataPropertyName = "role", Width = 150 });
            dgvUsers.Columns.Add(new DataGridViewButtonColumn { Name = "actions", HeaderText = LocalizationManager.GetString("Parts_GridActions"), Text = "", UseColumnTextForButtonValue = true, Width = 140, FlatStyle = FlatStyle.Flat });

            Panel pnlGridCard = ThemeConfig.CreateCardPanel(dgvUsers);
            mainLayout.Controls.Add(pnlGridCard, 0, 1);

            this.ResumeLayout(false);
        }

        private void ApplyTheme()
        {

            ThemeConfig.ApplyGridTheme(dgvUsers);
        }

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;
            lblUsersTitle.Text = LocalizationManager.GetString("Msg_UserManagement");
            ThemeConfig.ApplyStandardAddButton(btnAddUser, "User_AddUser");
            if (txtSearch != null) txtSearch.PlaceholderText = LocalizationManager.GetString("Msg_SearchUsers");
            if (dgvUsers.Columns["id"] != null) dgvUsers.Columns["id"].HeaderText = LocalizationManager.GetString("Users_GridID", "ID");
            if (dgvUsers.Columns["username"] != null) dgvUsers.Columns["username"].HeaderText = LocalizationManager.GetString("Users_GridUsername", "Username");
            if (dgvUsers.Columns["role"] != null) dgvUsers.Columns["role"].HeaderText = LocalizationManager.GetString("Users_GridRole", "Role");
            dgvUsers.Invalidate();
            if (dgvUsers.Columns["actions"] != null) dgvUsers.Columns["actions"].HeaderText = LocalizationManager.GetString("Parts_GridActions");
        }

        public void LoadData(string search = "")
        {
            try
            {
                string sql = "SELECT id, username, role FROM users";
                if (!string.IsNullOrEmpty(search))
                {
                    sql += $" WHERE username LIKE '%{search}%'";
                }
                DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
                dgvUsers.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(LocalizationManager.GetString("User_LoadError") + ": " + ex.Message);
            }
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            AddUserForm form = new AddUserForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void PerformEdit(int rowIndex)
        {
            int userId = Convert.ToInt32(dgvUsers.Rows[rowIndex].Cells["db_id"].Value);
            string username = dgvUsers.Rows[rowIndex].Cells["username"].Value.ToString();

            if (username.ToLower() == "softio.admin")
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("User_AdminEditBlock"));
                return;
            }

            AddUserForm form = new AddUserForm(userId);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void DgvUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            
            if (dgvUsers.Columns[e.ColumnIndex].Name == "actions")
            {
                var id = dgvUsers.Rows[e.RowIndex].Cells["db_id"].Value;
                var username = dgvUsers.Rows[e.RowIndex].Cells["username"].Value.ToString();

                var mousePos = dgvUsers.PointToClient(Cursor.Position);
                var cellRect = dgvUsers.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                int relativeX = mousePos.X - cellRect.X;
                int iconSize = 24;
                int gap = 12;
                int startX = (cellRect.Width - (iconSize * 2 + gap)) / 2;

                // Edit Click (centered range)
                if (relativeX >= startX && relativeX <= startX + iconSize)
                {
                    PerformEdit(e.RowIndex);
                }
                // Delete Click (centered range)
                else if (relativeX >= startX + iconSize + gap && relativeX <= startX + iconSize * 2 + gap)
                {
                    if (username.ToLower() == "softio.admin")
                    {
                        MessageHelper.ShowWarning(LocalizationManager.GetString("User_AdminDeleteBlock"));
                        return;
                    }

                    string confirmMsg = string.Format(LocalizationManager.GetString("User_DeleteConfirm"), username);

                    if (MessageHelper.ConfirmAction(confirmMsg))
                    {
                        try
                        {
                            DatabaseHelper.ExecuteNonQuery($"DELETE FROM users WHERE id = {id}");
                            LoadData();
                        }
                        catch (Exception ex)
                        {
                            MessageHelper.ShowError(LocalizationManager.GetString("User_DeleteError") + ": " + ex.Message);
                        }
                    }
                }
            }
        }
        
        private void DgvUsers_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
             if (e.RowIndex >= 0 && dgvUsers.Columns[e.ColumnIndex].Name == "actions")
             {
                 dgvUsers.Cursor = Cursors.Hand;
             }
             else
             {
                 dgvUsers.Cursor = Cursors.Default;
             }
        }

        private void DgvUsers_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
             dgvUsers.Cursor = Cursors.Default;
        }
        
        private void DgvUsers_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (dgvUsers.Columns[e.ColumnIndex].Name == "actions")
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                int iconSize = 24;
                int gap = 12;
                int startX = e.CellBounds.X + (e.CellBounds.Width - (iconSize * 2 + gap)) / 2;
                int centerY = e.CellBounds.Y + (e.CellBounds.Height - iconSize) / 2;

                // Edit Icon
                Rectangle editRect = new Rectangle(startX, centerY, iconSize, iconSize);
                Image imgEdit = ThemeConfig.GetNuricon("edit");
                if (imgEdit != null) e.Graphics.DrawImage(imgEdit, editRect);

                // Delete Icon
                Rectangle delRect = new Rectangle(startX + iconSize + gap, centerY, iconSize, iconSize);
                Image imgDelete = ThemeConfig.GetNuricon("delete");
                if (imgDelete != null) e.Graphics.DrawImage(imgDelete, delRect);
            }
        }
    }
}

