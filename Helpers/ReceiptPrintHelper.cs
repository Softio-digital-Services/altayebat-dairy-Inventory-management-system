using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace InventorySystem.Helpers
{
    public class ReceiptPrintItem
    {
        public string Name { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
    }

    public class ReceiptPrintOptions
    {
        public string PrinterName { get; set; }
        public int Copies { get; set; } = 1;
        public bool Landscape { get; set; }
        public bool Color { get; set; } = true;
        public string CustomerName { get; set; }
        public string ShippingTo { get; set; }
        public string CurrencySymbol { get; set; } = "$";
        public List<ReceiptPrintItem> Items { get; set; } = new List<ReceiptPrintItem>();
        public decimal Subtotal { get; set; }
        public decimal Vat { get; set; }
        public decimal Shipping { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        /// <summary>Receipt paper width in mm. 0 = use saved default.</summary>
        public double PaperWidthMm { get; set; }
        /// <summary>Receipt paper height in mm. 0 = auto tall roll.</summary>
        public double PaperHeightMm { get; set; }
        public double MarginMm { get; set; } = -1;
        public double MarginTopMm { get; set; } = -1;
        public double MarginRightMm { get; set; } = -1;
        public double MarginBottomMm { get; set; } = -1;
        public double MarginLeftMm { get; set; } = -1;
    }

    public static class ReceiptPrintHelper
    {
        private static ReceiptPrintOptions _opts;

        public static void Print(ReceiptPrintOptions options)
        {
            if (options == null) return;
            _opts = options;

            var defaults = PrintSettings.GetSnapshot();
            var resolved = PrintSettings.ResolveForPrinter(options.PrinterName, "receipt", defaults);
            double widthMm = options.PaperWidthMm > 0 ? options.PaperWidthMm : resolved.WidthMm;
            double heightMm = options.PaperHeightMm > 0 ? options.PaperHeightMm
                : (resolved.HeightMm > 0 ? resolved.HeightMm : 0);
            double uni = options.MarginMm >= 0 ? options.MarginMm
                : (resolved.MarginMm >= 0 ? resolved.MarginMm : defaults.ReceiptMarginMm);
            double mTop = options.MarginTopMm >= 0 ? options.MarginTopMm
                : (resolved.MarginTopMm >= 0 ? resolved.MarginTopMm : uni);
            double mRight = options.MarginRightMm >= 0 ? options.MarginRightMm
                : (resolved.MarginRightMm >= 0 ? resolved.MarginRightMm : uni);
            double mBottom = options.MarginBottomMm >= 0 ? options.MarginBottomMm
                : (resolved.MarginBottomMm >= 0 ? resolved.MarginBottomMm : uni);
            double mLeft = options.MarginLeftMm >= 0 ? options.MarginLeftMm
                : (resolved.MarginLeftMm >= 0 ? resolved.MarginLeftMm : uni);
            int paperW = Math.Max(1, PrintSettings.MmToHundredths(widthMm));
            int paperH = heightMm > 0
                ? Math.Max(1, PrintSettings.MmToHundredths(heightMm))
                : Math.Max(1, PrintSettings.MmToHundredths(Math.Max(200, widthMm * 3.5)));

            using (PrintDocument pd = new PrintDocument())
            {
                try { pd.DefaultPageSettings.PaperSize = new PaperSize("Receipt", paperW, paperH); }
                catch { pd.DefaultPageSettings.PaperSize = new PaperSize("Receipt", paperW, paperH); }
                pd.DefaultPageSettings.Margins = new Margins(
                    PrintSettings.MmToHundredths(mLeft),
                    PrintSettings.MmToHundredths(mRight),
                    PrintSettings.MmToHundredths(mTop),
                    PrintSettings.MmToHundredths(mBottom));
                pd.DefaultPageSettings.Landscape = options.Landscape;
                pd.DefaultPageSettings.Color = options.Color;

                if (!string.IsNullOrWhiteSpace(options.PrinterName))
                {
                    try { pd.PrinterSettings.PrinterName = options.PrinterName; } catch { }
                }
                try
                {
                    pd.PrinterSettings.Copies = (short)Math.Max(1, Math.Min(99, options.Copies));
                }
                catch { }

                try
                {
                    PrintSettings.SavePrinterProfile(options.PrinterName, "receipt", new PrinterJobProfile
                    {
                        WidthMm = widthMm,
                        HeightMm = heightMm,
                        MarginMm = uni,
                        MarginTopMm = mTop,
                        MarginRightMm = mRight,
                        MarginBottomMm = mBottom,
                        MarginLeftMm = mLeft,
                        PaperMode = "roll"
                    });
                }
                catch { }

                pd.PrintPage += PrintPage;
                pd.Print();
            }
        }

        private static void PrintPage(object sender, PrintPageEventArgs e)
        {
            var o = _opts ?? new ReceiptPrintOptions();
            Graphics g = e.Graphics;
            int w = Math.Max(40, e.MarginBounds.Width);
            // Scale fonts slightly for narrow (58mm) vs wide (80mm+) rolls
            float scale = Math.Max(0.85f, Math.Min(1.15f, w / 280f));
            Font fH = new Font("Segoe UI", 11f * scale, FontStyle.Bold);
            Font fS = new Font("Segoe UI", 8.5f * scale);
            Font fI = new Font("Consolas", 8f * scale);
            Font fB = new Font("Segoe UI", 9.5f * scale, FontStyle.Bold);

            int m = e.MarginBounds.Left;
            int y = e.MarginBounds.Top;
            var center = new StringFormat { Alignment = StringAlignment.Center };
            var right = new StringFormat { Alignment = StringAlignment.Far };
            string sym = string.IsNullOrWhiteSpace(o.CurrencySymbol) ? "$" : o.CurrencySymbol;
            int nameMax = Math.Max(10, Math.Min(28, w / 10));

            string company = ThemeConfig.CompanyName;
            if (string.IsNullOrWhiteSpace(company)) company = ThemeConfig.CompanyName;
            g.DrawString(company.ToUpperInvariant(), fH, Brushes.Black, new RectangleF(m, y, w, 22 * scale), center);
            y += (int)(24 * scale);
            g.DrawString("SALES RECEIPT", fS, Brushes.Black, new RectangleF(m, y, w, 18 * scale), center);
            y += (int)(18 * scale);
            g.DrawString(DateTime.Now.ToString("g"), fS, Brushes.Black, new RectangleF(m, y, w, 18 * scale), center);
            y += (int)(22 * scale);
            g.DrawLine(Pens.Black, m, y, m + w, y);
            y += 8;

            if (!string.IsNullOrWhiteSpace(o.CustomerName))
            {
                g.DrawString("Customer: " + o.CustomerName, fS, Brushes.Black, m, y);
                y += (int)(18 * scale);
            }
            if (!string.IsNullOrWhiteSpace(o.ShippingTo))
            {
                g.DrawString("Ship to: " + o.ShippingTo.Replace("\r\n", ", ").Replace("\n", ", "), fS, Brushes.Black,
                    new RectangleF(m, y, w, 36 * scale));
                y += (int)(36 * scale);
            }

            g.DrawString("QTY", fI, Brushes.Black, m, y);
            g.DrawString("ITEM", fI, Brushes.Black, m + (int)(36 * scale), y);
            g.DrawString("TOTAL", fI, Brushes.Black, new RectangleF(m, y, w, 16 * scale), right);
            y += (int)(16 * scale);
            g.DrawLine(Pens.Black, m, y, m + w, y);
            y += 6;

            if (o.Items != null)
            {
                foreach (var it in o.Items)
                {
                    string name = it.Name ?? "";
                    if (name.Length > nameMax) name = name.Substring(0, Math.Max(1, nameMax - 3)) + "...";
                    g.DrawString(Math.Max(1, it.Qty).ToString(), fI, Brushes.Black, m, y);
                    g.DrawString(name, fI, Brushes.Black, m + (int)(36 * scale), y);
                    g.DrawString(FormatMoney(sym, it.Total), fI, Brushes.Black, new RectangleF(m, y, w, 16 * scale), right);
                    y += (int)(16 * scale);
                    if (y > e.MarginBounds.Bottom - 120)
                    {
                        e.HasMorePages = true;
                        return;
                    }
                }
            }

            y += 6;
            g.DrawLine(Pens.Black, m, y, m + w, y);
            y += 8;

            void Row(string label, decimal amount, bool bold = false)
            {
                var font = bold ? fB : fS;
                g.DrawString(label, font, Brushes.Black, m, y);
                g.DrawString(FormatMoney(sym, amount), font, Brushes.Black, new RectangleF(m, y, w, 18 * scale), right);
                y += (int)(18 * scale);
            }

            Row("Subtotal", o.Subtotal);
            if (o.Vat > 0.009m) Row("VAT", o.Vat);
            if (o.Shipping > 0.009m) Row("Shipping", o.Shipping);
            if (o.Discount > 0.009m) Row("Discount", -o.Discount);
            y += 4;
            g.DrawLine(Pens.Black, m, y, m + w, y);
            y += 8;
            Row("TOTAL", o.Total, true);
            y += (int)(16 * scale);
            g.DrawString("Thank you!", fS, Brushes.Black, new RectangleF(m, y, w, 18 * scale), center);
            e.HasMorePages = false;
        }

        private static string FormatMoney(string symbol, decimal amount)
        {
            return $"{symbol}{amount:0.00}";
        }
    }
}
