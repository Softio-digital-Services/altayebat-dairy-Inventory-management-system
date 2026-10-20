using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Controls;
using InventorySystem.Helpers;
using InventorySystem.Services;
using System.Drawing.Printing;

namespace InventorySystem.Forms
{
    public class QuotationPreviewForm : BaseModalForm
    {
        private int _orderId;
        private OrderService _orderService;
        private List<Panel> _generatedPages = new List<Panel>();
        private int _currentPrintPageIndex = 0;

        public QuotationPreviewForm(int orderId)
        {
            _orderId = orderId;
            _orderService = new OrderService();

            this.Size = new Size(950, 950);
            this.ContentPanel.Padding = new Padding(20, 20, 20, 20);

            InitializeUI();
            LocalizationManager.LanguageChanged += OnLanguageChanged;
            this.FormClosed += (s, e) => LocalizationManager.LanguageChanged -= OnLanguageChanged;
            ApplyLocalization();
            LoadData();
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            if (IsDisposed) return;
            ApplyLocalization();
            LoadData();
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            string L(string key, string fb) => LocalizationManager.GetString(key, fb);
            this.TitleText = L("QuotePreview_Title", "Quotation Preview") + " - #" + _orderId;
            SetFooterButtons(
                L("Tran_Print", "Print"),
                L("Tran_Export", "Export"),
                (s, e) => HandlePrint(),
                (s, e) => HandleExport(),
                L("Popup_Cancel", "Close"),
                (s, e) => this.Close()
            );
        }

        private void InitializeUI()
        {
            this.ContentPanel.AutoScroll = true;

            this.ContentPanel.Resize += (s, e) =>
            {
                foreach (Control ctrl in this.ContentPanel.Controls)
                {
                    if (ctrl is Panel p) p.Left = Math.Max(0, (this.ContentPanel.Width - p.Width) / 2);
                }
            };
        }

        private void LoadData()
        {
            try
            {
                // Fetch Data
                var items = _orderService.GetOrderItems(_orderId);
                decimal total = 0;
                foreach (var item in items) total += (item.Quantity * item.UnitPrice);

                // Build UI on pnlContent
                RenderDocument(items, total);
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(string.Format(
                    LocalizationManager.GetString("QuotePreview_LoadError", "Could not load quotation details: {0}"),
                    ex.Message));
            }
        }

        private static string L(string key, string fallback) =>
            LocalizationManager.GetString(key, fallback);

        private Panel CreateA4PagePanel()
        {
            return new Panel
            {
                Width = 800,
                Height = 1131,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 40),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top
            };
        }

        private void RenderDocument(List<OrderItem> items, decimal total)
        {
            this.ContentPanel.Controls.Clear();
            _generatedPages.Clear();

            int currentItemIndex = 0;
            int pageNumber = 1;

            while (currentItemIndex < items.Count || pageNumber == 1)
            {
                Panel page = CreateA4PagePanel();
                _generatedPages.Add(page);

                int gridStartY = 40;

                if (pageNumber == 1)
                {
                    // Full Main Header
                    int hy = 40;

                    Label lblCompany = new Label
                    {
                        Text = ThemeConfig.CompanyName.ToUpper(),
                        Font = new Font("Segoe UI", 24, FontStyle.Bold),
                        ForeColor = ThemeConfig.PrimaryColor,
                        Location = new Point(40, hy),
                        AutoSize = true
                    };
                    page.Controls.Add(lblCompany);

                    Label lblQuoteTitle = new Label
                    {
                        Text = LocalizationManager.GetString("QuotePreview_Quote", "QUOTATION"),
                        Font = new Font("Segoe UI", 20, FontStyle.Bold),
                        ForeColor = Color.DimGray,
                        Location = new Point(page.Width - 350, hy + 5),
                        Size = new Size(310, 35),
                        TextAlign = ContentAlignment.TopRight
                    };
                    page.Controls.Add(lblQuoteTitle);

                    hy += 45;
                    Label lblCompInfo = new Label
                    {
                        Text = L("QuotePreview_CompanyInfo", ""),
                        Font = new Font("Segoe UI", 9),
                        Location = new Point(40, hy),
                        Size = new Size(500, 35),
                        ForeColor = Color.Gray
                    };
                    page.Controls.Add(lblCompInfo);

                    hy += 60;
                    Panel pnlDetails = new Panel { BackColor = Color.FromArgb(245, 247, 250), Location = new Point(40, hy), Size = new Size(page.Width - 80, 35) };
                    page.Controls.Add(pnlDetails);

                    string customerId = DatabaseHelper.ExecuteScalar<string>($"SELECT customer_id FROM orders WHERE order_id = {_orderId}");
                    Label lblQuoteInfo = new Label
                    {
                        Text = string.Format(
                            L("QuotePreview_MetaLine", "QUOTE #: {0}   |   DATE: {1}   |   CUST ID: {2}   |   VALIDITY: {3}"),
                            _orderId,
                            DateTime.Now.ToString("dd MMM yyyy"),
                            customerId ?? "N/A",
                            L("QuotePreview_ValidityDays", "15 Days")),
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        ForeColor = Color.FromArgb(64, 64, 64),
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    pnlDetails.Controls.Add(lblQuoteInfo);

                    hy += 60;
                    Label lblCustHeader = new Label { Text = LocalizationManager.GetString("QuotePreview_CustHeader"), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = ThemeConfig.PrimaryColor, Location = new Point(40, hy), AutoSize = true };
                    page.Controls.Add(lblCustHeader);
                    hy += 25;

                    string custQuery = $@"
                        SELECT COALESCE(c.full_name, 'Walk-in Customer') AS full_name, c.address, c.phone 
                        FROM orders o 
                        LEFT JOIN customers c ON o.customer_id = c.customer_id 
                        WHERE o.order_id = {_orderId}";
                    var custDt = DatabaseHelper.ExecuteDataTable(custQuery);

                    string custFullName = L("QuotePreview_WalkIn", "Walk-in Customer");
                    string custAddress = L("QuotePreview_NoAddress", "No Address Provided");
                    string custPhone = L("QuotePreview_NoPhone", "No Phone Provided");

                    if (custDt.Rows.Count > 0 && custDt.Rows[0]["full_name"] != DBNull.Value)
                    {
                        custFullName = custDt.Rows[0]["full_name"].ToString();
                        string addr = custDt.Rows[0]["address"]?.ToString();
                        string phone = custDt.Rows[0]["phone"]?.ToString();

                        if (!string.IsNullOrWhiteSpace(addr)) custAddress = addr;
                        if (!string.IsNullOrWhiteSpace(phone)) custPhone = phone;
                    }

                    Label lblCustInfo = new Label
                    {
                        Text = string.Format(
                            L("QuotePreview_CustInfoLine", "{0}\n{1}: {2} | {3}: {4}"),
                            custFullName,
                            L("Popup_Address", "Address"),
                            custAddress,
                            L("Popup_Phone", "Phone"),
                            custPhone),
                        Font = new Font("Segoe UI", 10),
                        Location = new Point(40, hy),
                        Size = new Size(600, 45),
                        ForeColor = Color.Black
                    };
                    page.Controls.Add(lblCustInfo);

                    gridStartY = hy + 60; // Starts at y = 310
                }
                else
                {
                    // Continued Header
                    Label lblCont = new Label
                    {
                        Text = string.Format(L("QuotePreview_ContinuedPage", "QUOTATION #{0} (Continued - Page {1})"), _orderId, pageNumber),
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = Color.Gray,
                        Location = new Point(40, 40),
                        AutoSize = true
                    };
                    page.Controls.Add(lblCont);
                    gridStartY = 80;
                }

                // Create Grid for this specific page
                DataGridView grid = new DataGridView
                {
                    Location = new Point(40, gridStartY),
                    Width = page.Width - 80,
                    AllowUserToAddRows = false,
                    ReadOnly = true,
                    RowHeadersVisible = false,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    ScrollBars = ScrollBars.None,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    EnableHeadersVisualStyles = false,
                    AllowUserToResizeRows = false
                };
                grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                grid.DefaultCellStyle.Padding = new Padding(5);
                ThemeConfig.ApplyGridTheme(grid);
                grid.ColumnHeadersHeight = 35;
                grid.RowTemplate.Height = 60;

                grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                grid.Columns.Add(new DataGridViewImageColumn { Name = "Photo", HeaderText = LocalizationManager.GetString("QuotePreview_ColPhoto"), Width = 60, ImageLayout = DataGridViewImageCellLayout.Zoom, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
                grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Desc", HeaderText = LocalizationManager.GetString("QuotePreview_ColDesc"), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
                grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = LocalizationManager.GetString("POS_GridQty"), Width = 60, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
                grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = LocalizationManager.GetString("POS_GridPrice"), Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
                grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = LocalizationManager.GetString("POS_GridTotal"), Width = 110, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) } });

                // Calculate available grid height depending on whether this can be the final page
                int remainingItems = items.Count - currentItemIndex;
                int estimatedHeightNeeded = 40 + (remainingItems * 60) + 10;

                // Final page footer block reserves Y bounds from 860 down. Max printable grid baseline on final page is 850.
                int maxGridHeightIfLastPage = 850 - gridStartY;

                bool isLastPage = (estimatedHeightNeeded <= maxGridHeightIfLastPage);
                int maxGridHeightForThisPage = isLastPage ? maxGridHeightIfLastPage : (1080 - gridStartY);
                int currentGridHeight = 35; // Starts with header height

                while (currentItemIndex < items.Count)
                {
                    var item = items[currentItemIndex];
                    if (currentGridHeight + 60 + 10 > maxGridHeightForThisPage && grid.Rows.Count > 0)
                    {
                        // Current page capacity reached, continue remaining rows onto subsequent page sheet
                        break;
                    }

                    Image partImg = ThemeConfig.GetNuricon("pos");
                    try { if (!string.IsNullOrEmpty(item.PartImage) && System.IO.File.Exists(item.PartImage)) partImg = Image.FromFile(item.PartImage); } catch { }
                    grid.Rows.Add(partImg, $"{item.PartName}\n{item.Description}", item.Quantity, CurrencyService.Format(item.UnitPrice), CurrencyService.Format(item.Quantity * item.UnitPrice));

                    currentGridHeight += 60;
                    currentItemIndex++;
                }

                grid.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);
                int actualRowsHeight = 0;
                foreach (DataGridViewRow r in grid.Rows) actualRowsHeight += r.Height;
                grid.Height = grid.ColumnHeadersHeight + actualRowsHeight + 10;
                page.Controls.Add(grid);

                // Attach complete summary footer exclusively to the final output page
                if (currentItemIndex >= items.Count)
                {
                    int footerStartY = 860;
                    Panel pnlSummaryWrap = new Panel { Location = new Point(40, footerStartY), Size = new Size(page.Width - 80, 180) };
                    page.Controls.Add(pnlSummaryWrap);

                    Label lblTermsHead = new Label
                    {
                        Text = LocalizationManager.GetString("QuotePreview_TermsHead", "TERMS AND CONDITIONS"),
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        ForeColor = ThemeConfig.PrimaryColor,
                        Location = new Point(0, 0),
                        AutoSize = true
                    };
                    pnlSummaryWrap.Controls.Add(lblTermsHead);

                    string termsText = LocalizationManager.GetString("QuotePreview_TermsBody", "• Validity: 15 days from issue.\n• Payment due prior to delivery.\n• Acceptance indicates billing confirmation.\n\nAccepted By: __________________________");

                    Label lblTerms = new Label
                    {
                        Text = termsText,
                        Font = new Font("Segoe UI", 8.5F),
                        Location = new Point(0, 25),
                        Size = new Size(400, 140),
                        ForeColor = Color.DimGray
                    };
                    pnlSummaryWrap.Controls.Add(lblTerms);

                    decimal dbTotal = DatabaseHelper.ExecuteScalar<decimal>($"SELECT total_amount FROM orders WHERE order_id = {_orderId}");
                    decimal taxAmount = dbTotal > total ? dbTotal - total : 0;
                    decimal grandTotal = dbTotal > total ? dbTotal : total;

                    int sx = pnlSummaryWrap.Width - 280;

                    List<string> lbls = new List<string>();
                    List<string> vls = new List<string>();

                    lbls.Add(LocalizationManager.GetString("POS_Subtotal", "Subtotal")); vls.Add(CurrencyService.Format(total));
                    if (taxAmount > 0) { lbls.Add(LocalizationManager.GetString("QuotePreview_Tax", "Tax / Extras")); vls.Add(CurrencyService.Format(taxAmount)); }
                    lbls.Add(LocalizationManager.GetString("POS_GrandTotal", "GRAND TOTAL")); vls.Add(CurrencyService.Format(grandTotal));

                    for (int i = 0; i < lbls.Count; i++)
                    {
                        bool isLast = (i == lbls.Count - 1);
                        Label lblL = new Label { Text = lbls[i], Font = new Font("Segoe UI", isLast ? 10 : 9, isLast ? FontStyle.Bold : FontStyle.Regular), Location = new Point(sx, i * 28), Size = new Size(130, 25), TextAlign = ContentAlignment.MiddleRight };
                        Label lblV = new Label
                        {
                            Text = vls[i],
                            Font = new Font("Segoe UI", isLast ? 12 : 10, isLast ? FontStyle.Bold : FontStyle.Regular),
                            Location = new Point(sx + 135, i * 28),
                            Size = new Size(140, 25),
                            TextAlign = ContentAlignment.MiddleRight,
                            ForeColor = isLast ? ThemeConfig.PrimaryColor : Color.Black
                        };
                        pnlSummaryWrap.Controls.Add(lblL);
                        pnlSummaryWrap.Controls.Add(lblV);
                    }

                    Label lblFinal = new Label
                    {
                        Text = LocalizationManager.GetString("QuotePreview_FinalMsg", "Thank you for your business! Please contact us if you have any questions."),
                        Font = new Font("Segoe UI", 10, FontStyle.Bold | FontStyle.Italic),
                        Location = new Point(0, 1050),
                        Size = new Size(page.Width, 30),
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = Color.Gray
                    };
                    page.Controls.Add(lblFinal);

                    Label lblContactFooter = new Label
                    {
                        Text = L("QuotePreview_ContactFooter", ""),
                        Font = new Font("Segoe UI", 8.5F),
                        Location = new Point(0, 1085),
                        Size = new Size(page.Width, 25),
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = Color.Silver
                    };
                    page.Controls.Add(lblContactFooter);

                    break;
                }
                else
                {
                    // Render minimalist continued marker on intermediate document footers
                    Label lblPageFooter = new Label
                    {
                        Text = string.Format(L("QuotePreview_PageN", "Page {0}"), pageNumber),
                        Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                        ForeColor = Color.Silver,
                        Location = new Point(0, 1100),
                        Size = new Size(page.Width, 20),
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    page.Controls.Add(lblPageFooter);
                }

                pageNumber++;
            }

            // Lay out physical sheet list into main scrollable preview viewport
            int pageTopOffset = 0;
            foreach (Panel p in _generatedPages)
            {
                p.Top = pageTopOffset;
                p.Left = Math.Max(0, (this.ContentPanel.Width - p.Width) / 2);
                this.ContentPanel.Controls.Add(p);
                pageTopOffset += p.Height + 30;
            }
        }

        private void HandlePrint()
        {
            if (_generatedPages.Count == 0) return;
            _currentPrintPageIndex = 0;
            try
            {
                using (PrintDocument pd = new PrintDocument())
                {
                    pd.DocumentName = $"Quotation_{_orderId}";
                    pd.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);

                    pd.PrintPage += (s, e) =>
                    {
                        if (_currentPrintPageIndex < _generatedPages.Count)
                        {
                            DrawPageToGraphics(_generatedPages[_currentPrintPageIndex], e.Graphics, e.MarginBounds);
                            _currentPrintPageIndex++;
                        }
                        e.HasMorePages = (_currentPrintPageIndex < _generatedPages.Count);
                    };

                    using (PrintDialog diag = new PrintDialog { Document = pd, UseEXDialog = true })
                    {
                        if (diag.ShowDialog() == DialogResult.OK)
                        {
                            pd.Print();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(string.Format(L("QuotePreview_PrintFailed", "Printing failed: {0}"), ex.Message));
            }
        }

        private void DrawPageToGraphics(Panel pageCanvas, Graphics g, Rectangle marginBounds)
        {
            using (Bitmap bmp = new Bitmap(pageCanvas.Width, pageCanvas.Height))
            {
                pageCanvas.DrawToBitmap(bmp, new Rectangle(0, 0, pageCanvas.Width, pageCanvas.Height));
                float scale = marginBounds.Width / (float)bmp.Width;
                int targetWidth = (int)(bmp.Width * scale);
                int targetHeight = (int)(bmp.Height * scale);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(bmp, new Rectangle(marginBounds.Left, marginBounds.Top, targetWidth, targetHeight));
            }
        }

        private void HandleExport()
        {
            if (_generatedPages.Count == 0) return;
            try
            {
                using (PrintDocument pd = new PrintDocument())
                {
                    string pdfPrinter = null;
                    foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                    {
                        if (printer.Contains("PDF"))
                        {
                            pdfPrinter = printer;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(pdfPrinter))
                    {
                        MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_PdfPrinterNotFound", "No PDF printer found (e.g. Microsoft Print to PDF). Please use the 'Print' button and select a PDF printer manually."));
                        return;
                    }

                    using (SaveFileDialog diag = new SaveFileDialog())
                    {
                        diag.Filter = "PDF Document|*.pdf";
                        diag.FileName = $"Quotation_{_orderId}";
                        diag.Title = L("QuotePreview_ExportPdfTitle", "Export to PDF");

                        if (diag.ShowDialog() == DialogResult.OK)
                        {
                            _currentPrintPageIndex = 0;
                            pd.PrinterSettings.PrinterName = pdfPrinter;
                            pd.PrinterSettings.PrintToFile = true;
                            pd.PrinterSettings.PrintFileName = diag.FileName;
                            pd.DocumentName = $"Quotation_{_orderId}";
                            pd.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);

                            pd.PrintPage += (s, e) =>
                            {
                                if (_currentPrintPageIndex < _generatedPages.Count)
                                {
                                    DrawPageToGraphics(_generatedPages[_currentPrintPageIndex], e.Graphics, e.MarginBounds);
                                    _currentPrintPageIndex++;
                                }
                                e.HasMorePages = (_currentPrintPageIndex < _generatedPages.Count);
                            };

                            pd.Print();
                            MessageHelper.ShowInfo(L("QuotePreview_ExportSuccess", "Quotation exported as PDF successfully!"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(string.Format(L("QuotePreview_ExportFailed", "Export failed: {0}"), ex.Message));
            }
        }
    }
}
