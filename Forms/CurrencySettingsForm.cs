using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using InventorySystem.Controls;
using InventorySystem.Helpers;
using InventorySystem.Services;

namespace InventorySystem.Forms
{
    /// <summary>
    /// Settings panel for managing supported currencies and their exchange rates.
    /// </summary>
    public class CurrencySettingsForm : BaseModalForm
    {
        private DataGridView dgvRates;
        private Label lblStatus;
        // private Button btnRefresh;

        public CurrencySettingsForm()
        {
            this.Width = 800;
            InitializeForm();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();
            LoadRates();
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            this.TitleText = LocalizationManager.GetString("Curr_SettingsTitle");
            SetFooterButtons(
                LocalizationManager.GetString("Curr_SaveBtn"),
                LocalizationManager.GetString("Popup_Cancel"),
                BtnSave_Click,
                (s, e) => this.Close()
            );
            if (dgvRates != null && dgvRates.Columns.Count > 0)
            {
                if (dgvRates.Columns.Contains("code")) dgvRates.Columns["code"].HeaderText = LocalizationManager.GetString("Curr_ColCode");
                if (dgvRates.Columns.Contains("name")) dgvRates.Columns["name"].HeaderText = LocalizationManager.GetString("Curr_ColName");
                if (dgvRates.Columns.Contains("symbol")) dgvRates.Columns["symbol"].HeaderText = LocalizationManager.GetString("Curr_ColSymbol");
                if (dgvRates.Columns.Contains("rate_vs_usd")) dgvRates.Columns["rate_vs_usd"].HeaderText = LocalizationManager.GetString("Curr_ColRate");
                if (dgvRates.Columns.Contains("last_updated")) dgvRates.Columns["last_updated"].HeaderText = LocalizationManager.GetString("Curr_ColUpdate");
                if (dgvRates.Columns.Contains("colAction")) dgvRates.Columns["colAction"].HeaderText = LocalizationManager.GetString("Parts_GridActions");
            }
            LoadRates();
        }

        private void InitializeForm()
        {
            LocalizationManager.ApplyRTL(this);
            
            // Adaptive sizing handled by BaseModalForm.OnLoad
            this.TitleText = LocalizationManager.GetString("Curr_SettingsTitle");

            this.SuspendLayout();

            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(25)
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F)); // Header
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); // Status
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Grid
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); // Info

            // ---------------------------------- Header ----------------------------------------------------------------------------
            FlowLayoutPanel flpHeader = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = LocalizationManager.IsArabic ? FlowDirection.LeftToRight : FlowDirection.RightToLeft,
                Padding = new Padding(0),
                BackColor = Color.Transparent
            };

            Button btnRefresh = new Button
            {
                Height = 45, Width = 150,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 10, 10, 0)
            };
            ThemeConfig.ApplyStandardRefreshButton(btnRefresh, "Curr_RefreshBtn");
            btnRefresh.Click += BtnRefresh_Click;
            flpHeader.Controls.Add(btnRefresh);

            Button btnAdd = new Button
            {
                Height = 45, Width = 160,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 10, 10, 0)
            };
            ThemeConfig.ApplyStandardAddButton(btnAdd, "Curr_AddBtn");
            btnAdd.Click += BtnAdd_Click;
            flpHeader.Controls.Add(btnAdd);

            tlpMain.Controls.Add(flpHeader, 0, 0);

            // ---------------------------------- Status label ------------------------------------------------------------------
            lblStatus = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                ForeColor = ThemeConfig.SecondaryColor,
                Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                TextAlign = LocalizationManager.IsArabic ? ContentAlignment.BottomRight : ContentAlignment.BottomLeft
            };
            tlpMain.Controls.Add(lblStatus, 0, 1);

            // ---------------------------------- Grid --------------------------------------------------------------------------------
            dgvRates = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvRates.DataError += (s, e) => { e.ThrowException = false; };
            ThemeConfig.ApplyGridTheme(dgvRates);

            dgvRates.Columns.Add(new DataGridViewTextBoxColumn { Name = "code", HeaderText = LocalizationManager.GetString("Curr_ColCode"), DataPropertyName = "code", Width = 60, ReadOnly = true });
            dgvRates.Columns.Add(new DataGridViewTextBoxColumn { Name = "name", HeaderText = LocalizationManager.GetString("Curr_ColName"), DataPropertyName = "name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = false });
            dgvRates.Columns.Add(new DataGridViewTextBoxColumn { Name = "symbol", HeaderText = LocalizationManager.GetString("Curr_ColSymbol"), DataPropertyName = "symbol", Width = 70, ReadOnly = false, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvRates.Columns.Add(new DataGridViewTextBoxColumn { Name = "rate_vs_usd", HeaderText = LocalizationManager.GetString("Curr_ColRate"), DataPropertyName = "rate_vs_usd", Width = 130, ReadOnly = false, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvRates.Columns.Add(new DataGridViewTextBoxColumn { Name = "last_updated", HeaderText = LocalizationManager.GetString("Curr_ColUpdate"), DataPropertyName = "last_updated", Width = 150, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });
            dgvRates.Columns.Add(new DataGridViewImageColumn { Name = "colAction", HeaderText = LocalizationManager.GetString("Parts_GridActions"), Width = 60 });

            dgvRates.CellPainting += DgvRates_CellPainting;
            dgvRates.CellMouseClick += DgvRates_CellMouseClick;

            // Context Menu removed in favor of Action column

            tlpMain.Controls.Add(dgvRates, 0, 2);

            // ---------------------------------- Info box --------------------------------------------------------------------------
            Label lblInfo = new Label
            {
                Text = LocalizationManager.GetString("Curr_InfoBox"),
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = ThemeConfig.SecondaryColor,
                TextAlign = LocalizationManager.IsArabic ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft
            };
            tlpMain.Controls.Add(lblInfo, 0, 3);

            this.ContentPanel.Controls.Add(tlpMain);

            SetFooterButtons(
                LocalizationManager.GetString("Curr_SaveBtn"),
                LocalizationManager.GetString("Popup_Cancel"),
                BtnSave_Click,
                (s, e) => this.Close()
            );

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new AddCurrencyForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadRates();
                    MessageHelper.ShowSuccess(LocalizationManager.GetString("Msg_Saved"));
                }
            }
        }

        private void LoadRates()
        {
            try
            {
                var dt = CurrencyService.GetAllCurrencies();
                dgvRates.DataSource = dt;

                // Find latest update
                string lastUpdate = "--";
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    if (row["last_updated"] != DBNull.Value)
                    {
                        lastUpdate = Convert.ToDateTime(row["last_updated"]).ToString("g");
                        break;
                    }
                }
                lblStatus.Text = string.Format(LocalizationManager.GetString("Curr_StatusLastUpdate"), lastUpdate);
            }
            catch (Exception ex)
            {
                lblStatus.Text = LocalizationManager.GetString("Msg_Error") + ": " + ex.Message;
            }
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (sender is Button btn) btn.Enabled = false;
            lblStatus.ForeColor = ThemeConfig.SecondaryColor;
            lblStatus.Text     = LocalizationManager.GetString("CurrSettings_Connecting", "Connecting to exchange rate service...");

            var rates = await CurrencyService.FetchLiveRatesAsync();

            if (rates != null && rates.Count > 1)
            {
                CurrencyService.SaveRatesToDb(rates);
                LoadRates();
                lblStatus.ForeColor = ThemeConfig.SuccessColor;
                lblStatus.Text      = string.Format(LocalizationManager.GetString("Curr_StatusLastUpdate"), DateTime.Now.ToString("g"));
            }
            else
            {
                lblStatus.ForeColor = ThemeConfig.DangerColor;
                lblStatus.Text      = LocalizationManager.GetString("CurrSettings_FetchError", "Could not reach server. Using cached rates.");
            }

            if (sender is Button btn2) btn2.Enabled = true;
        }

        private void DgvRates_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvRates.Columns[e.ColumnIndex].Name == "colAction")
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);
                
                Image imgDelete = ThemeConfig.GetNuricon("delete");
                if (imgDelete != null)
                {
                    int iconSize = 24;
                    Rectangle rect = new Rectangle(
                        e.CellBounds.X + (e.CellBounds.Width - iconSize) / 2,
                        e.CellBounds.Y + (e.CellBounds.Height - iconSize) / 2,
                        iconSize, iconSize);
                    e.Graphics.DrawImage(imgDelete, rect);
                }
            }
        }

        private void DgvRates_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvRates.Columns[e.ColumnIndex].Name == "colAction")
            {
                string code = dgvRates.Rows[e.RowIndex].Cells["code"].Value?.ToString();
                if (string.IsNullOrEmpty(code)) return;

                if (code == "USD")
                {
                    MessageHelper.ShowWarning(LocalizationManager.GetString("Curr_MsgBaseDelete"));
                    return;
                }

                if (MessageHelper.ShowConfirm(string.Format(LocalizationManager.GetString("Curr_MsgDeleteConfirm"), code)))
                {
                    DatabaseHelper.ExecuteNonQuery($"DELETE FROM currency_rates WHERE code = '{code}'");
                    CurrencyService.LoadRatesFromDb();
                    LoadRates();
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            dgvRates.EndEdit();
            foreach (DataGridViewRow row in dgvRates.Rows)
            {
                if (row.IsNewRow) continue;
                string code = row.Cells["code"].Value?.ToString();
                string name = row.Cells["name"].Value?.ToString();
                string symbol = row.Cells["symbol"].Value?.ToString();
                
                if (decimal.TryParse(row.Cells["rate_vs_usd"].Value?.ToString(),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal rate))
                {
                    CurrencyService.UpdateCurrency(code, name, symbol, rate);
                }
            }
            
            lblStatus.ForeColor = ThemeConfig.SuccessColor;
            lblStatus.Text = LocalizationManager.GetString("Msg_Saved");
            LoadRates();
        }
    }
}
