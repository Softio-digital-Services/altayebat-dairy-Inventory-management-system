using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using InventorySystem.Controls;
using InventorySystem.Helpers;
using InventorySystem.Services;

namespace InventorySystem.Forms
{
    public class QuotationsForm : UserControl
    {
        private Label lblQuotationsTitle;
        private DataGridView dgvQuotes;
        private ModernTextBox txtSearch;
        private OrderService _orderService;
        private int _hoveredRow = -1;

        public QuotationsForm()
        {
            _orderService = new OrderService();
            InitializeComponent();
            LoadQuotations();

            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();

            // Currency Sync
            InventorySystem.Services.CurrencyService.CurrencyChanged += (s, e) => { dgvQuotes.Invalidate(); };

            GlobalEvents.OnOrdersUpdated += () => {
                if (!this.IsDisposed) LoadQuotations();
            };
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);

            if (dgvQuotes != null && dgvQuotes.Columns.Count > 0)
            {
                if (dgvQuotes.Columns.Contains("order_id"))    dgvQuotes.Columns["order_id"].HeaderText    = LocalizationManager.GetString("Users_GridID");
                if (dgvQuotes.Columns.Contains("order_date"))  dgvQuotes.Columns["order_date"].HeaderText  = LocalizationManager.GetString("Hist_ColDate");
                if (dgvQuotes.Columns.Contains("CustomerName")) dgvQuotes.Columns["CustomerName"].HeaderText = LocalizationManager.GetString("Cust_Title");
                if (dgvQuotes.Columns.Contains("total_amount")) dgvQuotes.Columns["total_amount"].HeaderText = LocalizationManager.GetString("Msg_Total");
                if (dgvQuotes.Columns.Contains("colActions"))   dgvQuotes.Columns["colActions"].HeaderText  = LocalizationManager.GetString("Parts_GridActions");
            }
            if (lblQuotationsTitle != null) lblQuotationsTitle.Text = LocalizationManager.GetString("Msg_CustomerQuotations");
            if (txtSearch != null) txtSearch.PlaceholderText = LocalizationManager.GetString("Msg_SearchQuotations");
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Dock = DockStyle.Fill;

            Panel pnlRoot = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            this.Controls.Add(pnlRoot);

            // Header
            lblQuotationsTitle = ThemeConfig.CreateStandardHeader(LocalizationManager.GetString("Msg_CustomerQuotations"));
            lblQuotationsTitle.Name = "lblQuotationsTitle";

            txtSearch = new ModernTextBox {
                IsSearch = true, ShowLabel = false,
                PlaceholderText = LocalizationManager.GetString("Msg_SearchQuotations"),
                Size = new Size(320, 35)
            };
            txtSearch.TextChanged += (s, e) => LoadQuotations(txtSearch.Text);

            TableLayoutPanel tlpHeader = ThemeConfig.CreateGlobalFormHeader(lblQuotationsTitle, txtSearch, null);

            // Grid
            dgvQuotes = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AutoGenerateColumns = false, BackgroundColor = ThemeConfig.SurfaceColor, BorderStyle = BorderStyle.None, RowHeadersVisible = false };
            ThemeConfig.ApplyGridTheme(dgvQuotes);

            dgvQuotes.Columns.Add(new DataGridViewTextBoxColumn { Name = "order_id", HeaderText = LocalizationManager.GetString("Users_GridID"), DataPropertyName = "order_id", Width = 60, ReadOnly = true });
            dgvQuotes.Columns.Add(new DataGridViewTextBoxColumn { Name = "order_date", HeaderText = LocalizationManager.GetString("Hist_ColDate"), DataPropertyName = "order_date", Width = 160, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });
            dgvQuotes.Columns.Add(new DataGridViewTextBoxColumn { Name = "CustomerName", HeaderText = LocalizationManager.GetString("POS_Customer"), DataPropertyName = "CustomerName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });

            var colTotal = new DataGridViewTextBoxColumn
            {
                Name = "total_amount", HeaderText = LocalizationManager.GetString("Msg_Total"), DataPropertyName = "total_amount",
                Width = 110, ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = ThemeConfig.PrimaryColor, Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            dgvQuotes.Columns.Add(colTotal);
            dgvQuotes.CellFormatting += DgvQuotes_CellFormatting;

            dgvQuotes.Columns.Add(new DataGridViewTextBoxColumn { Name = "customer_id", DataPropertyName = "customer_id", Visible = false });
            dgvQuotes.Columns.Add(new DataGridViewTextBoxColumn { Name = "colActions", HeaderText = LocalizationManager.GetString("Parts_GridActions"), Width = 200, ReadOnly = true });

            dgvQuotes.CellPainting  += DgvQuotes_CellPainting;
            dgvQuotes.CellClick     += DgvQuotes_CellClick;
            dgvQuotes.CellMouseMove += (s, e) =>
            {
                if (e.RowIndex != _hoveredRow && e.RowIndex >= 0 && dgvQuotes.Columns[e.ColumnIndex].Name == "colActions")
                { _hoveredRow = e.RowIndex; dgvQuotes.InvalidateRow(e.RowIndex); dgvQuotes.Cursor = Cursors.Hand; }
                else if (dgvQuotes.Columns[e.ColumnIndex].Name != "colActions") dgvQuotes.Cursor = Cursors.Default;
            };
            dgvQuotes.CellMouseLeave += (s, e) => { _hoveredRow = -1; dgvQuotes.Cursor = Cursors.Default; };

            Panel pnlCard = ThemeConfig.CreateCardPanel(dgvQuotes);

            // IMPORTANT: WinForms docks in reverse Z-order. Add Fill first, then Top controls bottom-to-top.
            pnlRoot.Controls.Add(pnlCard);      // Fill — added first
            pnlRoot.Controls.Add(tlpHeader);    // Top — docked last = appears at top

            this.ResumeLayout(false);
        }

        private void DgvQuotes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvQuotes.Columns[e.ColumnIndex].Name == "total_amount" && e.Value != null)
            {
                if (decimal.TryParse(e.Value.ToString(), out decimal usdTotal))
                {
                    e.Value = InventorySystem.Services.CurrencyService.Format(usdTotal);
                    e.FormattingApplied = true;
                }
            }
        }

        private void DgvQuotes_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // 3. Actions - Nuricon Icons
            if (dgvQuotes.Columns[e.ColumnIndex].Name == "colActions")
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                int btnW = 32, btnH = 32, gap = 15;
                int totalW = btnW * 3 + gap * 2;
                int startX = e.CellBounds.X + (e.CellBounds.Width - totalW) / 2;
                int startY = e.CellBounds.Y + (e.CellBounds.Height - btnH) / 2;

                Image imgView = ThemeConfig.GetNuricon("preview_doc");
                Image imgOk = ThemeConfig.GetNuricon("check");
                Image imgDel = ThemeConfig.GetNuricon("delete");
                int startX2 = e.CellBounds.X + (e.CellBounds.Width - (32 * 3 + 15 * 2)) / 2;
                int midY = e.CellBounds.Y + (e.CellBounds.Height - 32) / 2;
                if (imgView != null) e.Graphics.DrawImage(imgView, new Rectangle(startX2, midY, 32, 32));
                if (imgOk != null) e.Graphics.DrawImage(imgOk, new Rectangle(startX2 + 47, midY, 32, 32));
                if (imgDel != null) e.Graphics.DrawImage(imgDel, new Rectangle(startX2 + 94, midY, 32, 32));
            }
        }

        // Click handling for three painted buttons
        private void DgvQuotes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvQuotes.Columns[e.ColumnIndex].Name != "colActions") return;

            int orderId = Convert.ToInt32(dgvQuotes.Rows[e.RowIndex].Cells["order_id"].Value);

            // Figure out which sub-button was clicked using the mouse position
            Point cur      = dgvQuotes.PointToClient(Cursor.Position);
            Rectangle cell = dgvQuotes.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);

            int btnW = 62, gap = 5;
            int totalW = btnW * 3 + gap * 2;
            int startX = cell.X + (cell.Width - totalW) / 2;

            int relX = cur.X;

            if (relX >= startX && relX < startX + btnW)
            {
                // View / Print
                QuotationPreviewForm doc = new QuotationPreviewForm(orderId);
                doc.ShowDialog(this);
            }
            else if (relX >= startX + btnW + gap && relX < startX + btnW * 2 + gap)
            {
                // Checkout / Convert
                if (MessageHelper.ConfirmAction("Convert this quotation to a finalized Sales Order? This will reduce stock."))
                {
                    try
                    {
                        if (_orderService.ConvertToOrder(orderId))
                        {
                            MessageHelper.ShowSuccess("Quotation successfully converted to Order!");
                            LoadQuotations();
                        }
                    }
                    catch (Exception ex) { MessageHelper.ShowError(ex.Message); }
                }
            }
            else if (relX >= startX + (btnW + gap) * 2 && relX < startX + (btnW + gap) * 2 + btnW)
            {
                // Delete
                if (MessageHelper.ConfirmAction("Are you sure you want to delete this quotation?"))
                {
                    _orderService.DeleteOrder(orderId);
                    LoadQuotations();
                }
            }
        }

        // Data loading
        public void LoadQuotations(string search = "")
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => LoadQuotations(search)));
                return;
            }

            DataTable dt = _orderService.GetQuotations();
            if (!string.IsNullOrEmpty(search))
            {
                // Simple client-side filter for now
                DataView dv = dt.DefaultView;
                dv.RowFilter = string.Format("CustomerName LIKE '%{0}%' OR order_id = {1}", search, int.TryParse(search, out int id) ? id : -1);
                dt = dv.ToTable();
            }
            dgvQuotes.DataSource = dt;

            // Ensure hidden columns stay hidden even after DataSource rebind
            if (dgvQuotes.Columns.Contains("customer_id"))
                dgvQuotes.Columns["customer_id"].Visible = false;
            if (dgvQuotes.Columns.Contains("status"))
                dgvQuotes.Columns["status"].Visible = false;

            // Keep actions column last
            if (dgvQuotes.Columns.Contains("colActions"))
                dgvQuotes.Columns["colActions"].DisplayIndex = dgvQuotes.Columns.Count - 1;

            ApplyLocalization();
        }
    }
}
