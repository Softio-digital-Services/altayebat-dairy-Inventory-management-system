using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ClosedXML.Excel;
using InventorySystem.Services;

namespace InventorySystem.Helpers
{
    /// <summary>
    /// Helper class for importing and exporting data to CSV and Excel formats
    /// </summary>
    public static class ImportExportHelper
    {
        #region CSV Export/Import

        /// <summary>
        /// Exports a DataTable to CSV file
        /// </summary>
        public static bool ExportToCsv(DataTable dataTable, string filePath)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                // Write headers
                IEnumerable<string> columnNames = dataTable.Columns.Cast<DataColumn>()
                    .Select(column => EscapeCsvField(column.ColumnName));
                sb.AppendLine(string.Join(",", columnNames));

                // Write rows
                foreach (DataRow row in dataTable.Rows)
                {
                    IEnumerable<string> fields = row.ItemArray
                        .Select(field => EscapeCsvField(field?.ToString() ?? ""));
                    sb.AppendLine(string.Join(",", fields));
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ImportExportHelper.ExportToCsv");
                return false;
            }
        }

        /// <summary>
        /// Imports data from CSV file to DataTable
        /// </summary>
        public static DataTable ImportFromCsv(string filePath)
        {
            try
            {
                DataTable dt = new DataTable();
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

                if (lines.Length == 0)
                    return dt;

                // Parse headers
                string[] headers = ParseCsvLine(lines[0]);
                foreach (string header in headers)
                {
                    dt.Columns.Add(header.Trim());
                }

                // Parse data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    string[] fields = ParseCsvLine(lines[i]);

                    // Pad or trim so rows are not silently dropped
                    if (fields.Length < dt.Columns.Count)
                    {
                        var padded = new string[dt.Columns.Count];
                        Array.Copy(fields, padded, fields.Length);
                        for (int f = fields.Length; f < padded.Length; f++)
                            padded[f] = "";
                        fields = padded;
                    }
                    else if (fields.Length > dt.Columns.Count)
                    {
                        fields = fields.Take(dt.Columns.Count).ToArray();
                    }

                    dt.Rows.Add(fields);
                }

                return dt;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ImportExportHelper.ImportFromCsv");
                return new DataTable();
            }
        }

        #endregion

        #region Excel Export/Import (Simple TSV Format)

        /// <summary>
        /// Imports data from Excel-compatible TSV file to DataTable
        /// </summary>
        public static DataTable ImportFromExcel(string filePath, string sheetName = null)
        {
            try
            {
                DataTable dt = new DataTable();
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

                if (lines.Length == 0)
                    return dt;

                // Parse headers
                string[] headers = lines[0].Split('\t');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header.Trim());
                }

                // Parse data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    string[] fields = lines[i].Split('\t');

                    if (fields.Length < dt.Columns.Count)
                    {
                        var padded = new string[dt.Columns.Count];
                        Array.Copy(fields, padded, fields.Length);
                        for (int f = fields.Length; f < padded.Length; f++)
                            padded[f] = "";
                        fields = padded;
                    }
                    else if (fields.Length > dt.Columns.Count)
                    {
                        fields = fields.Take(dt.Columns.Count).ToArray();
                    }

                    dt.Rows.Add(fields);
                }

                return dt;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ImportExportHelper.ImportFromExcel");
                return new DataTable();
            }
        }

        /// <summary>
        /// Exports a full sales report (summary + sold products) to a real .xlsx workbook.
        /// </summary>
        public static bool ExportSalesReport(
            string filePath,
            SalesReportSummary summary,
            DataTable soldProducts,
            string periodLabel = null)
        {
            try
            {
                string L(string key, string fallback) => LocalizationManager.GetString(key, fallback);
                bool isAr = LocalizationManager.IsArabic;

                using var workbook = new XLWorkbook();
                string sheetName = L("Rep_ExportSheetName", "Sales Report");
                if (sheetName.Length > 31) sheetName = sheetName.Substring(0, 31);
                var ws = workbook.Worksheets.Add(sheetName);
                if (isAr)
                    ws.RightToLeft = true;

                int row = 1;
                ws.Cell(row, 1).Value = L("Rep_Title", "Sales Reports");
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Font.FontSize = 16;
                row += 2;

                if (!string.IsNullOrWhiteSpace(periodLabel))
                {
                    ws.Cell(row, 1).Value = L("Rep_ExportPeriodLabel", "Period");
                    ws.Cell(row, 2).Value = periodLabel;
                    row++;
                }

                ws.Cell(row, 1).Value = L("Rep_From", "From");
                ws.Cell(row, 2).Value = summary.FromDate.ToString("yyyy-MM-dd");
                row++;
                ws.Cell(row, 1).Value = L("Rep_To", "To");
                ws.Cell(row, 2).Value = summary.ToDate.ToString("yyyy-MM-dd");
                row += 2;

                ws.Cell(row, 1).Value = L("Rep_ExportSummary", "Summary");
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Font.FontSize = 13;
                row++;

                void AddMetric(string label, decimal value)
                {
                    ws.Cell(row, 1).Value = label;
                    ws.Cell(row, 2).Value = value;
                    ws.Cell(row, 2).Style.NumberFormat.Format = CurrencyFormat;
                    row++;
                }

                AddMetric(L("Rep_MonthlyExpenses", "Monthly Expenses"), summary.TotalExpenses);
                AddMetric(L("Rep_ExportTotalCostProducts", "Total Cost of Products"), summary.TotalCost);
                AddMetric(L("Rep_TotalSales", "Total Sales"), summary.TotalSales);
                AddMetric(L("Rep_TotalProfit", "Total Profit"), summary.TotalProfit);
                AddMetric(L("Rep_ProfitAfterExpenses", "Profit After Expenses"), summary.TotalProfitAfterExpenses);
                row++;

                ws.Cell(row, 1).Value = L("Rep_ExportSoldProducts", "Sold Products");
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Font.FontSize = 13;
                row++;

                string[] headers =
                {
                    L("Rep_ColProduct", "Product"),
                    L("Rep_ColQuantitySold", "Quantity Sold"),
                    L("Rep_ColUnitPrice", "Unit Price"),
                    L("Rep_ColTotalSales", "Total Sales"),
                    L("Rep_ColTotalCost", "Total Cost"),
                    L("Rep_ColProfit", "Profit")
                };
                for (int c = 0; c < headers.Length; c++)
                {
                    ws.Cell(row, c + 1).Value = headers[c];
                    ws.Cell(row, c + 1).Style.Font.Bold = true;
                    ws.Cell(row, c + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
                }
                row++;

                int dataStart = row;
                if (soldProducts != null)
                {
                    foreach (DataRow dr in soldProducts.Rows)
                    {
                        ws.Cell(row, 1).Value = dr["product_name"]?.ToString() ?? "";
                        ws.Cell(row, 2).Value = Convert.ToDecimal(dr["quantity_sold"] == DBNull.Value ? 0 : dr["quantity_sold"]);
                        ws.Cell(row, 3).Value = Convert.ToDecimal(dr["unit_price"] == DBNull.Value ? 0 : dr["unit_price"]);
                        ws.Cell(row, 4).Value = Convert.ToDecimal(dr["total_sales"] == DBNull.Value ? 0 : dr["total_sales"]);
                        ws.Cell(row, 5).Value = Convert.ToDecimal(dr["total_cost"] == DBNull.Value ? 0 : dr["total_cost"]);
                        ws.Cell(row, 6).Value = Convert.ToDecimal(dr["profit"] == DBNull.Value ? 0 : dr["profit"]);

                        ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.##";
                        for (int c = 3; c <= 6; c++)
                            ws.Cell(row, c).Style.NumberFormat.Format = CurrencyFormat;
                        row++;
                    }
                }

                int dataEnd = row - 1;
                row++;

                ws.Cell(row, 1).Value = L("Rep_ExportFinalTotals", "Final Totals");
                ws.Cell(row, 1).Style.Font.Bold = true;
                row++;

                if (dataEnd >= dataStart)
                {
                    ws.Cell(row, 1).Value = L("Rep_ColQuantitySold", "Quantity Sold");
                    ws.Cell(row, 2).FormulaA1 = $"SUM(B{dataStart}:B{dataEnd})";
                    ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.##";
                    row++;
                    ws.Cell(row, 1).Value = L("Rep_TotalSales", "Total Sales");
                    ws.Cell(row, 2).FormulaA1 = $"SUM(D{dataStart}:D{dataEnd})";
                    ws.Cell(row, 2).Style.NumberFormat.Format = CurrencyFormat;
                    row++;
                    ws.Cell(row, 1).Value = L("Rep_ColTotalCost", "Total Cost");
                    ws.Cell(row, 2).FormulaA1 = $"SUM(E{dataStart}:E{dataEnd})";
                    ws.Cell(row, 2).Style.NumberFormat.Format = CurrencyFormat;
                    row++;
                    ws.Cell(row, 1).Value = L("Rep_TotalProfit", "Total Profit");
                    ws.Cell(row, 2).FormulaA1 = $"SUM(F{dataStart}:F{dataEnd})";
                    ws.Cell(row, 2).Style.NumberFormat.Format = CurrencyFormat;
                    row++;
                    ws.Cell(row, 1).Value = L("Rep_MonthlyExpenses", "Monthly Expenses");
                    ws.Cell(row, 2).Value = summary.TotalExpenses;
                    ws.Cell(row, 2).Style.NumberFormat.Format = CurrencyFormat;
                    row++;
                    ws.Cell(row, 1).Value = L("Rep_ProfitAfterExpenses", "Profit After Expenses");
                    ws.Cell(row, 2).Value = summary.TotalProfitAfterExpenses;
                    ws.Cell(row, 2).Style.NumberFormat.Format = CurrencyFormat;
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    ws.Cell(row, 2).Style.Font.Bold = true;
                }
                else
                {
                    AddMetric(L("Rep_MonthlyExpenses", "Monthly Expenses"), summary.TotalExpenses);
                    AddMetric(L("Rep_TotalSales", "Total Sales"), summary.TotalSales);
                    AddMetric(L("Rep_TotalProfit", "Total Profit"), summary.TotalProfit);
                    AddMetric(L("Rep_ProfitAfterExpenses", "Profit After Expenses"), summary.TotalProfitAfterExpenses);
                }

                AutoFitUsedRange(ws);
                workbook.SaveAs(filePath);
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ImportExportHelper.ExportSalesReport");
                return false;
            }
        }

        /// <summary>
        /// Exports a DataTable to an .xlsx byte array with auto-fitted columns.
        /// </summary>
        public static byte[] ExportDataTableToXlsx(DataTable dataTable, string sheetName = "Export")
        {
            if (dataTable == null) return Array.Empty<byte>();
            return ExportTablesToXlsx(new[] { (sheetName, dataTable) });
        }

        /// <summary>
        /// Exports several DataTables to one workbook, one worksheet each, all columns auto-fitted.
        /// </summary>
        public static byte[] ExportTablesToXlsx(
            System.Collections.Generic.IEnumerable<(string SheetName, DataTable Table)> sheets)
        {
            if (sheets == null) return Array.Empty<byte>();
            using var workbook = new XLWorkbook();

            int added = 0;
            foreach (var (sheetName, table) in sheets)
            {
                if (table == null) continue;
                WriteSheet(workbook, sheetName, table, added);
                added++;
            }

            if (added == 0) workbook.Worksheets.Add("Export");

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Key used on DataColumn.ExtendedProperties to give a column an Excel number format,
        /// e.g. CurrencyFormat for money columns.
        /// </summary>
        public const string NumberFormatKey = "numberFormat";

        /// <summary>Excel/.NET number format for money columns. Values are stored in USD.</summary>
        public const string CurrencyFormat = "$#,##0.00";

        private static string ColumnNumberFormat(DataColumn column) =>
            column?.ExtendedProperties[NumberFormatKey] as string;

        /// <summary>
        /// Tags the named columns as money so the Excel writer renders them with a currency
        /// symbol and sizes them to the formatted text. Unknown column names are ignored.
        /// </summary>
        public static DataTable MarkCurrencyColumns(DataTable dataTable, params string[] columns)
        {
            if (dataTable == null || columns == null) return dataTable;
            foreach (string name in columns)
            {
                if (!string.IsNullOrEmpty(name) && dataTable.Columns.Contains(name))
                    dataTable.Columns[name].ExtendedProperties[NumberFormatKey] = CurrencyFormat;
            }
            return dataTable;
        }

        private static void WriteSheet(XLWorkbook workbook, string sheetName, DataTable table, int index)
        {
            string name = string.IsNullOrWhiteSpace(sheetName) ? $"Sheet{index + 1}" : sheetName.Trim();
            foreach (char bad in new[] { ':', '\\', '/', '?', '*', '[', ']' })
                name = name.Replace(bad, ' ');
            if (name.Length > 31) name = name.Substring(0, 31);
            while (workbook.Worksheets.Contains(name))
                name = (name.Length > 28 ? name.Substring(0, 28) : name) + "_" + (index + 1);

            var ws = workbook.Worksheets.Add(name);

            int colCount = table.Columns.Count;
            for (int c = 0; c < colCount; c++)
            {
                ws.Cell(1, c + 1).Value = table.Columns[c].ColumnName;
                ws.Cell(1, c + 1).Style.Font.Bold = true;
                ws.Cell(1, c + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
            }

            for (int r = 0; r < table.Rows.Count; r++)
            {
                for (int c = 0; c < colCount; c++)
                    SetExportCellValue(ws.Cell(r + 2, c + 1), table.Rows[r][c]);
            }

            // Applied to the data rows only so the bold header keeps its plain text style.
            if (table.Rows.Count > 0)
            {
                for (int c = 0; c < colCount; c++)
                {
                    string format = ColumnNumberFormat(table.Columns[c]);
                    if (string.IsNullOrEmpty(format)) continue;
                    ws.Range(2, c + 1, table.Rows.Count + 1, c + 1)
                      .Style.NumberFormat.Format = format;
                }
            }

            AutoFitWorksheetColumns(ws, table);
            ApplyExportCenterAlignment(ws, 1, Math.Max(1, table.Rows.Count + 1), 1, colCount);
        }

        /// <summary>Centers header + data horizontally and vertically in export sheets.</summary>
        private static void ApplyExportCenterAlignment(IXLWorksheet ws, int firstRow, int lastRow, int firstCol, int lastCol)
        {
            if (ws == null || lastRow < firstRow || lastCol < firstCol) return;
            var range = ws.Range(firstRow, firstCol, lastRow, lastCol);
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        private static void SetExportCellValue(IXLCell cell, object val)
        {
            if (val == null || val == DBNull.Value)
            {
                cell.Value = "";
                return;
            }

            switch (val)
            {
                case DateTime dt:
                    cell.Value = dt;
                    cell.Style.DateFormat.Format = ExportDateFormat(dt);
                    return;
                case bool b:
                    cell.Value = b;
                    return;
                case byte or sbyte or short or ushort or int or uint or long or ulong:
                    cell.Value = Convert.ToInt64(val);
                    return;
                case float or double or decimal:
                    cell.Value = Convert.ToDouble(val);
                    return;
            }

            string text = val.ToString()?.Trim() ?? "";
            if (DateTime.TryParse(text, out DateTime parsed))
            {
                cell.Value = parsed;
                cell.Style.DateFormat.Format = ExportDateFormat(parsed);
                return;
            }

            cell.Value = text;
        }

        /// <summary>
        /// Keeps the time component for timestamps (history, orders) but not for plain dates.
        /// </summary>
        private static string ExportDateFormat(DateTime value) =>
            value.TimeOfDay == TimeSpan.Zero ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm:ss";

        private const double BoldCharFactor = 1.18;
        private const double WidthPadding = 2.2;
        private const double MinColumnWidth = 8;
        private const double MaxColumnWidth = 80;

        /// <summary>
        /// Sizes every column so the longest header/value is fully visible without manual resizing.
        /// Header text is bold, so it needs slightly more room per character than body text.
        /// </summary>
        private static void AutoFitWorksheetColumns(IXLWorksheet ws, DataTable dataTable)
        {
            if (dataTable == null || dataTable.Columns.Count == 0) return;

            int lastRow = Math.Max(1, dataTable.Rows.Count + 1);

            for (int c = 1; c <= dataTable.Columns.Count; c++)
            {
                string header = dataTable.Columns[c - 1].ColumnName ?? "";
                string format = ColumnNumberFormat(dataTable.Columns[c - 1]);
                double target = (header.Length * BoldCharFactor) + WidthPadding;

                for (int r = 0; r < dataTable.Rows.Count; r++)
                {
                    double len = ExportCellDisplayLength(dataTable.Rows[r][c - 1], format) + WidthPadding;
                    if (len > target) target = len;
                }

                ws.Column(c).AdjustToContents(1, lastRow);
                double measured = ws.Column(c).Width;
                if (measured > target) target = measured;

                ws.Column(c).Width = Math.Clamp(target, MinColumnWidth, MaxColumnWidth);
                ws.Column(c).Style.Alignment.WrapText = false;
                ws.Column(c).Style.Alignment.ShrinkToFit = false;
            }
        }

        /// <summary>
        /// Auto-fits a hand-built sheet that has no backing DataTable, measuring each written cell.
        /// Bold and larger fonts are scaled up since they consume more width per character.
        /// </summary>
        private static void AutoFitUsedRange(IXLWorksheet ws)
        {
            var used = ws?.RangeUsed();
            if (used == null) return;

            int firstCol = used.RangeAddress.FirstAddress.ColumnNumber;
            int lastCol = used.RangeAddress.LastAddress.ColumnNumber;
            int firstRow = used.RangeAddress.FirstAddress.RowNumber;
            int lastRow = used.RangeAddress.LastAddress.RowNumber;

            for (int c = firstCol; c <= lastCol; c++)
            {
                double target = MinColumnWidth;

                for (int r = firstRow; r <= lastRow; r++)
                {
                    var cell = ws.Cell(r, c);
                    if (cell.IsEmpty()) continue;

                    string text;
                    try
                    {
                        // Formula cells would force evaluation; their results are short numbers.
                        text = cell.HasFormula ? "$0,000,000.00" : cell.GetFormattedString();
                    }
                    catch { continue; }

                    if (string.IsNullOrEmpty(text)) continue;

                    double scale = 1.0;
                    if (cell.Style.Font.Bold) scale *= BoldCharFactor;
                    double size = cell.Style.Font.FontSize;
                    if (size > 11) scale *= size / 11.0;

                    double len = (text.Length * scale) + WidthPadding;
                    if (len > target) target = len;
                }

                ws.Column(c).Width = Math.Clamp(target, MinColumnWidth, MaxColumnWidth);
                ws.Column(c).Style.Alignment.WrapText = false;
                ws.Column(c).Style.Alignment.ShrinkToFit = false;
            }

            ApplyExportCenterAlignment(ws, firstRow, lastRow, firstCol, lastCol);
        }

        private static int ExportCellDisplayLength(object val, string numberFormat = null)
        {
            if (val == null || val == DBNull.Value) return 0;
            if (val is DateTime dt) return ExportDateFormat(dt).Length;
            if (val is bool b) return b ? 4 : 5;
            if (val is byte or sbyte or short or ushort or int or uint or long or ulong
                or float or double or decimal)
            {
                // Currency/thousands formatting makes the rendered text longer than the raw number.
                if (!string.IsNullOrEmpty(numberFormat))
                {
                    try
                    {
                        return Convert.ToDecimal(val)
                            .ToString(numberFormat, CultureInfo.InvariantCulture).Length;
                    }
                    catch { /* fall through to the raw length */ }
                }
                return val.ToString().Length;
            }

            string text = val.ToString()?.Trim() ?? "";
            if (DateTime.TryParse(text, out DateTime parsed))
                return ExportDateFormat(parsed).Length;
            return text.Length;
        }

        /// <summary>
        /// Exports a DataTable to UTF-8 CSV bytes (with BOM for Excel).
        /// </summary>
        public static byte[] ExportDataTableToCsvBytes(DataTable dataTable)
        {
            if (dataTable == null) return Array.Empty<byte>();
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", dataTable.Columns.Cast<DataColumn>()
                .Select(column => EscapeCsvField(column.ColumnName))));
            foreach (DataRow row in dataTable.Rows)
            {
                sb.AppendLine(string.Join(",", row.ItemArray
                    .Select(field => EscapeCsvField(field?.ToString() ?? ""))));
            }
            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        }

        /// <summary>
        /// Exports a DataTable to an .xlsx file with auto-fitted columns.
        /// </summary>
        public static bool ExportToXlsx(DataTable dataTable, string filePath, string sheetName = "Export")
        {
            try
            {
                if (dataTable == null) return false;
                File.WriteAllBytes(filePath, ExportDataTableToXlsx(dataTable, sheetName));
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ImportExportHelper.ExportToXlsx");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Escapes a CSV field by wrapping in quotes if needed
        /// </summary>
        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // If field contains comma, quote, or newline, wrap in quotes
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                // Escape existing quotes by doubling them
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }

        /// <summary>
        /// Parses a CSV line handling quoted fields
        /// </summary>
        private static string[] ParseCsvLine(string line)
        {
            List<string> fields = new List<string>();
            bool inQuotes = false;
            StringBuilder currentField = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    // Check for escaped quote
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            // Add last field
            fields.Add(currentField.ToString());

            return fields.ToArray();
        }

        /// <summary>
        /// Creates a sample CSV template file
        /// </summary>
        public static void CreateCsvTemplate(string filePath, string[] headers)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Join(",", headers));
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ImportExportHelper.CreateCsvTemplate");
            }
        }

        #endregion
    }
}
