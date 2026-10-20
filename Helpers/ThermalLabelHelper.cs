using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;
using InventorySystem.Services;

namespace InventorySystem.Helpers
{
    public class LabelPrintItem
    {
        public string Name { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class LabelPrintOptions
    {
        public string PrinterName { get; set; }
        public int Copies { get; set; } = 1;
        public bool Landscape { get; set; }
        public bool Color { get; set; } = true;
        public string PageRange { get; set; } = "all";
        public double LabelWidthMm { get; set; }
        public double LabelHeightMm { get; set; }
        public double LabelGapMm { get; set; } = -1;
        public double MarginMm { get; set; } = -1;
        public double MarginTopMm { get; set; } = -1;
        public double MarginRightMm { get; set; } = -1;
        public double MarginBottomMm { get; set; } = -1;
        public double MarginLeftMm { get; set; } = -1;
        public int Columns { get; set; } = -1; // -1 = use saved / auto
        public string PaperMode { get; set; }
        public double PageWidthMm { get; set; }
        public double PageHeightMm { get; set; }
    }

    public class ThermalLabelHelper
    {
        private static List<LabelPrintItem> _printQueue;
        private static int _queueIndex;
        private static int _pageNumber;
        private static HashSet<int> _allowedPages;
        private static bool _grayscale;

        // Active layout for the current print job (hundredths of an inch)
        private static int _labelWidth;
        private static int _labelHeight;
        private static int _gapX;
        private static int _gapY;
        private static int _maxCols;
        private static bool _rollMode;

        /// <summary>
        /// Opens a compact themed PrintPreviewDialog (legacy path).
        /// </summary>
        public static void GenerateLabelPDF(List<LabelPrintItem> items, IWin32Window owner = null)
        {
            if (items == null || items.Count == 0) return;

            PrintDocument pd = BuildDocument(items, new LabelPrintOptions());

            using (PrintPreviewDialog preview = new PrintPreviewDialog())
            {
                preview.Document = pd;
                preview.Text = LocalizationManager.GetString("ThermalLabel_PreviewTitle", "Label Preview");
                if (owner is Form ownerForm)
                    preview.Owner = ownerForm;

                ThemeConfig.ApplyPrintPreviewTheme(preview);

                if (owner != null)
                    preview.ShowDialog(owner);
                else
                    preview.ShowDialog();
            }
        }

        /// <summary>
        /// Prints labels directly using the chosen printer and options (no Chromium dialog).
        /// </summary>
        public static void PrintLabels(List<LabelPrintItem> items, LabelPrintOptions options, IWin32Window owner = null)
        {
            if (items == null || items.Count == 0) return;
            options ??= new LabelPrintOptions();

            PrintDocument pd = BuildDocument(items, options);
            try
            {
                pd.Print();
            }
            finally
            {
                pd.Dispose();
            }
        }

        private static PrintDocument BuildDocument(List<LabelPrintItem> items, LabelPrintOptions options)
        {
            _printQueue = ExpandQueue(items);
            _queueIndex = 0;
            _pageNumber = 0;
            _allowedPages = ParsePageRange(options.PageRange);
            _grayscale = !options.Color;

            var defaults = PrintSettings.GetSnapshot();
            var resolved = PrintSettings.ResolveForPrinter(options.PrinterName, "label", defaults);

            double wMm = options.LabelWidthMm > 0 ? options.LabelWidthMm : resolved.WidthMm;
            double hMm = options.LabelHeightMm > 0 ? options.LabelHeightMm : resolved.HeightMm;
            double gapMm = options.LabelGapMm >= 0 ? options.LabelGapMm : (resolved.GapMm >= 0 ? resolved.GapMm : defaults.LabelGapMm);
            double uniMargin = options.MarginMm >= 0 ? options.MarginMm
                : (resolved.MarginMm >= 0 ? resolved.MarginMm : defaults.LabelMarginMm);
            double mTop = options.MarginTopMm >= 0 ? options.MarginTopMm
                : (resolved.MarginTopMm >= 0 ? resolved.MarginTopMm : uniMargin);
            double mRight = options.MarginRightMm >= 0 ? options.MarginRightMm
                : (resolved.MarginRightMm >= 0 ? resolved.MarginRightMm : uniMargin);
            double mBottom = options.MarginBottomMm >= 0 ? options.MarginBottomMm
                : (resolved.MarginBottomMm >= 0 ? resolved.MarginBottomMm : uniMargin);
            double mLeft = options.MarginLeftMm >= 0 ? options.MarginLeftMm
                : (resolved.MarginLeftMm >= 0 ? resolved.MarginLeftMm : uniMargin);
            int columns = options.Columns >= 0 ? options.Columns
                : (resolved.Columns > 0 ? resolved.Columns : defaults.LabelColumns);
            string mode = !string.IsNullOrWhiteSpace(options.PaperMode)
                ? options.PaperMode
                : (!string.IsNullOrWhiteSpace(resolved.PaperMode) ? resolved.PaperMode : defaults.LabelPaperMode);
            double pageWMm = options.PageWidthMm > 0 ? options.PageWidthMm
                : (resolved.PageWidthMm > 0 ? resolved.PageWidthMm : defaults.LabelPageWidthMm);
            double pageHMm = options.PageHeightMm > 0 ? options.PageHeightMm
                : (resolved.PageHeightMm > 0 ? resolved.PageHeightMm : defaults.LabelPageHeightMm);

            _rollMode = string.Equals(mode, "roll", StringComparison.OrdinalIgnoreCase);

            _labelWidth = Math.Max(1, PrintSettings.MmToHundredths(wMm));
            _labelHeight = Math.Max(1, PrintSettings.MmToHundredths(hMm));
            _gapX = PrintSettings.MmToHundredths(gapMm);
            _gapY = _gapX;
            _maxCols = _rollMode ? 1 : (columns > 0 ? Math.Min(4, columns) : 4);
            int mt = PrintSettings.MmToHundredths(mTop);
            int mr = PrintSettings.MmToHundredths(mRight);
            int mb = PrintSettings.MmToHundredths(mBottom);
            int ml = PrintSettings.MmToHundredths(mLeft);

            PrintDocument pd = new PrintDocument();
            if (_rollMode)
            {
                int pw = Math.Max(_labelWidth + ml + mr, PrintSettings.MmToHundredths(wMm));
                int ph = Math.Max(_labelHeight + mt + mb, PrintSettings.MmToHundredths(hMm));
                try { pd.DefaultPageSettings.PaperSize = new PaperSize("Label", pw, ph); }
                catch { pd.DefaultPageSettings.PaperSize = new PaperSize("Label", pw, ph); }
                pd.DefaultPageSettings.Margins = new Margins(ml, mr, mt, mb);
            }
            else
            {
                int pw = Math.Max(1, PrintSettings.MmToHundredths(pageWMm));
                int ph = Math.Max(1, PrintSettings.MmToHundredths(pageHMm));
                try { pd.DefaultPageSettings.PaperSize = new PaperSize("LabelSheet", pw, ph); }
                catch { pd.DefaultPageSettings.PaperSize = new PaperSize("LabelSheet", pw, ph); }
                pd.DefaultPageSettings.Margins = new Margins(ml, mr, mt, mb);
            }
            pd.DefaultPageSettings.Landscape = options.Landscape;
            pd.DefaultPageSettings.Color = options.Color;

            if (!string.IsNullOrWhiteSpace(options.PrinterName))
            {
                try { pd.PrinterSettings.PrinterName = options.PrinterName; } catch { }
            }

            int copies = Math.Max(1, Math.Min(99, options.Copies));
            try { pd.PrinterSettings.Copies = (short)copies; } catch { }

            try
            {
                PrintSettings.SavePrinterProfile(options.PrinterName, "label", new PrinterJobProfile
                {
                    WidthMm = wMm,
                    HeightMm = hMm,
                    GapMm = gapMm,
                    MarginMm = uniMargin,
                    MarginTopMm = mTop,
                    MarginRightMm = mRight,
                    MarginBottomMm = mBottom,
                    MarginLeftMm = mLeft,
                    Columns = columns,
                    PaperMode = mode,
                    PageWidthMm = pageWMm,
                    PageHeightMm = pageHMm
                });
            }
            catch { }

            pd.PrintPage += Pd_PrintPage;
            return pd;
        }

        private static HashSet<int> ParsePageRange(string range)
        {
            if (string.IsNullOrWhiteSpace(range) ||
                range.Trim().Equals("all", StringComparison.OrdinalIgnoreCase))
                return null;

            var set = new HashSet<int>();
            foreach (var part in range.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var token = part.Trim();
                if (token.Contains('-'))
                {
                    var ends = token.Split('-');
                    if (ends.Length == 2 &&
                        int.TryParse(ends[0].Trim(), out int a) &&
                        int.TryParse(ends[1].Trim(), out int b))
                    {
                        int lo = Math.Min(a, b);
                        int hi = Math.Max(a, b);
                        for (int i = lo; i <= hi; i++)
                            if (i >= 1) set.Add(i);
                    }
                }
                else if (int.TryParse(token, out int page) && page >= 1)
                {
                    set.Add(page);
                }
            }
            return set.Count > 0 ? set : null;
        }

        private static List<LabelPrintItem> ExpandQueue(List<LabelPrintItem> items)
        {
            var queue = new List<LabelPrintItem>();
            foreach (var item in items)
            {
                int copies = Math.Max(1, item.Quantity);
                for (int i = 0; i < copies; i++)
                {
                    queue.Add(new LabelPrintItem
                    {
                        Name = item.Name,
                        SKU = item.SKU,
                        Price = item.Price,
                        Quantity = 1
                    });
                }
            }
            return queue;
        }

        private static void Pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (_printQueue == null || _queueIndex >= _printQueue.Count)
            {
                e.HasMorePages = false;
                return;
            }

            _pageNumber++;
            bool include = _allowedPages == null || _allowedPages.Contains(_pageNumber);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            Rectangle area = e.MarginBounds;
            int lw = Math.Max(40, _labelWidth);
            int lh = Math.Max(30, _labelHeight);
            int gx = Math.Max(0, _gapX);
            int gy = Math.Max(0, _gapY);
            int maxCols = Math.Max(1, _maxCols);
            int cols = _rollMode
                ? 1
                : Math.Max(1, Math.Min(maxCols, (area.Width + gx) / (lw + gx)));
            int rows = _rollMode
                ? 1
                : Math.Max(1, (area.Height + gy) / (lh + gy));
            int perPage = cols * rows;

            int drawn = 0;
            while (_queueIndex < _printQueue.Count && drawn < perPage)
            {
                if (include)
                {
                    int col = drawn % cols;
                    int row = drawn / cols;
                    int x = area.Left + col * (lw + gx);
                    int y = area.Top + row * (lh + gy);
                    DrawLabel(g, _printQueue[_queueIndex], new Rectangle(x, y, lw, lh));
                }
                _queueIndex++;
                drawn++;
            }

            // If this page was skipped by range, keep going until an included page or end
            if (!include && _queueIndex < _printQueue.Count)
            {
                e.HasMorePages = true;
                return;
            }

            if (_allowedPages != null && _queueIndex < _printQueue.Count)
            {
                // More labels remain — only continue if any later page is allowed
                int nextPage = _pageNumber + 1;
                int remaining = _printQueue.Count - _queueIndex;
                int maxPage = nextPage + Math.Max(0, (remaining + perPage - 1) / perPage);
                bool anyLeft = false;
                for (int p = nextPage; p <= maxPage; p++)
                {
                    if (_allowedPages.Contains(p)) { anyLeft = true; break; }
                }
                e.HasMorePages = anyLeft;
                return;
            }

            e.HasMorePages = _queueIndex < _printQueue.Count;
        }

        private static void DrawLabel(Graphics g, LabelPrintItem item, Rectangle bounds)
        {
            Color borderColor = _grayscale ? Color.FromArgb(100, 100, 100) : Color.FromArgb(148, 163, 184);
            Color textColor = Color.Black;
            Color accent = _grayscale ? Color.FromArgb(40, 40, 40) : Color.FromArgb(5, 150, 105);

            using (var border = new Pen(borderColor, Math.Max(1f, bounds.Height / 80f)))
                g.DrawRectangle(border, bounds);

            var center = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            };

            string name = item.Name ?? "";
            int maxName = Math.Max(12, Math.Min(40, bounds.Width / 7));
            if (name.Length > maxName) name = name.Substring(0, Math.Max(1, maxName - 3)) + "...";
            string code = string.IsNullOrWhiteSpace(item.SKU) ? "—" : item.SKU;
            string priceStr = CurrencyService.Format(item.Price);

            float pad = Math.Max(2f, bounds.Width * 0.03f);
            float nameH = Math.Max(10f, bounds.Height * 0.16f);
            float barcodeH = Math.Max(18f, bounds.Height * 0.38f);
            float codeH = Math.Max(9f, bounds.Height * 0.13f);
            float priceH = Math.Max(11f, bounds.Height * 0.16f);
            float namePt = Math.Max(5f, Math.Min(12f, bounds.Height * 0.07f));
            float skuPt = Math.Max(4.5f, Math.Min(10f, bounds.Height * 0.055f));
            float pricePt = Math.Max(5.5f, Math.Min(13f, bounds.Height * 0.075f));

            using (var nameFont = new Font("Segoe UI", namePt, FontStyle.Bold))
            using (var skuFont = new Font("Consolas", skuPt))
            using (var priceFont = new Font("Segoe UI", pricePt, FontStyle.Bold))
            using (var textBrush = new SolidBrush(textColor))
            using (var accentBrush = new SolidBrush(accent))
            {
                float y = bounds.Y + pad;
                var nameRect = new RectangleF(bounds.X + pad, y, bounds.Width - pad * 2, nameH);
                g.DrawString(name, nameFont, textBrush, nameRect, center);
                y += nameH + pad * 0.4f;

                try
                {
                    var bs = new BarcodeService();
                    int targetW = Math.Max(40, (int)(bounds.Width - pad * 2));
                    int targetH = Math.Max(16, (int)barcodeH);
                    using (Bitmap barcode = bs.RenderCode128(code == "—" ? "0" : code, targetW, targetH))
                    {
                        int bw = Math.Min(barcode.Width, targetW);
                        int bh = Math.Min(barcode.Height, targetH);
                        int bx = bounds.X + (bounds.Width - bw) / 2;
                        g.DrawImage(barcode, new Rectangle(bx, (int)y, bw, bh));
                    }
                }
                catch
                {
                    // If barcode render fails, still show code text
                }
                y += barcodeH + pad * 0.3f;

                var codeRect = new RectangleF(bounds.X + pad, y, bounds.Width - pad * 2, codeH);
                g.DrawString(code, skuFont, textBrush, codeRect, center);
                y += codeH;

                var priceRect = new RectangleF(bounds.X + pad, y, bounds.Width - pad * 2, priceH);
                g.DrawString(priceStr, priceFont, accentBrush, priceRect, center);
            }
        }
    }
}
