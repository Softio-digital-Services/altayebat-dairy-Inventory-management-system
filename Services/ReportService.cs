using System;
using System.Data;
using Microsoft.Data.Sqlite;
using InventorySystem.Helpers;

namespace InventorySystem.Services
{
    public class SalesReportSummary
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalProfitAfterExpenses { get; set; }
        public decimal TotalMilkExpenses { get; set; }
        public decimal TotalMilkLiters { get; set; }
        public decimal TotalGeneralExpenses { get; set; }
        public decimal TotalUnpaidMilkExpenses { get; set; }
    }

    public class ReportService
    {
        private const string CompletedOrdersFilter =
            "o.status IS NOT NULL AND o.status NOT IN ('Draft', 'Quotation')";

        public (DateTime from, DateTime to) GetPresetRange(string preset)
        {
            DateTime today = DateTime.Today;
            return (preset ?? "Daily") switch
            {
                "Weekly" => (today.AddDays(-(int)today.DayOfWeek), today),
                "Monthly" => (new DateTime(today.Year, today.Month, 1), today),
                "Yearly" => (new DateTime(today.Year, 1, 1), today),
                "Custom" => (today, today),
                _ => (today, today) // Daily
            };
        }

        public SalesReportSummary GetSummary(DateTime fromDate, DateTime toDate)
        {
            string from = fromDate.Date.ToString("yyyy-MM-dd");
            string to = toDate.Date.ToString("yyyy-MM-dd");

            decimal totalSales = DatabaseHelper.ExecuteScalar<decimal>($@"
                SELECT COALESCE(SUM(oi.quantity * oi.price), 0)
                FROM order_items oi
                INNER JOIN orders o ON oi.order_id = o.order_id
                WHERE date(o.order_date) BETWEEN date(@from) AND date(@to)
                  AND {CompletedOrdersFilter}",
                new SqliteParameter("@from", from),
                new SqliteParameter("@to", to));

            decimal totalCost = DatabaseHelper.ExecuteScalar<decimal>($@"
                SELECT COALESCE(SUM(oi.quantity * COALESCE(p.purchase_price, 0)), 0)
                FROM order_items oi
                INNER JOIN orders o ON oi.order_id = o.order_id
                INNER JOIN parts p ON oi.part_id = p.id
                WHERE date(o.order_date) BETWEEN date(@from) AND date(@to)
                  AND {CompletedOrdersFilter}",
                new SqliteParameter("@from", from),
                new SqliteParameter("@to", to));

            decimal totalExpenses = DatabaseHelper.ExecuteScalar<decimal>(@"
                SELECT COALESCE(SUM(amount), 0)
                FROM expenses
                WHERE date(expense_date) BETWEEN date(@from) AND date(@to)
                  AND date_deleted IS NULL",
                new SqliteParameter("@from", from),
                new SqliteParameter("@to", to));

            decimal totalMilkExpenses = DatabaseHelper.ExecuteScalar<decimal>(@"
                SELECT COALESCE(SUM(amount), 0)
                FROM expenses
                WHERE date(expense_date) BETWEEN date(@from) AND date(@to)
                  AND date_deleted IS NULL
                  AND (category = 'Raw Milk' OR (liters IS NOT NULL AND liters > 0))",
                new SqliteParameter("@from", from),
                new SqliteParameter("@to", to));

            decimal totalMilkLiters = DatabaseHelper.ExecuteScalar<decimal>(@"
                SELECT COALESCE(SUM(liters), 0)
                FROM expenses
                WHERE date(expense_date) BETWEEN date(@from) AND date(@to)
                  AND date_deleted IS NULL
                  AND (category = 'Raw Milk' OR (liters IS NOT NULL AND liters > 0))",
                new SqliteParameter("@from", from),
                new SqliteParameter("@to", to));

            decimal totalUnpaidMilk = DatabaseHelper.ExecuteScalar<decimal>(@"
                SELECT COALESCE(SUM(amount), 0)
                FROM expenses
                WHERE date(expense_date) BETWEEN date(@from) AND date(@to)
                  AND date_deleted IS NULL
                  AND is_paid = 0
                  AND (category = 'Raw Milk' OR (liters IS NOT NULL AND liters > 0))",
                new SqliteParameter("@from", from),
                new SqliteParameter("@to", to));

            decimal profit = totalSales - totalCost;
            decimal profitAfterExpenses = profit - totalExpenses;
            decimal totalGeneralExpenses = Math.Max(0, totalExpenses - totalMilkExpenses);

            return new SalesReportSummary
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date,
                TotalExpenses = totalExpenses,
                TotalCost = totalCost,
                TotalSales = totalSales,
                TotalProfit = profit,
                TotalProfitAfterExpenses = profitAfterExpenses,
                TotalMilkExpenses = totalMilkExpenses,
                TotalMilkLiters = totalMilkLiters,
                TotalGeneralExpenses = totalGeneralExpenses,
                TotalUnpaidMilkExpenses = totalUnpaidMilk
            };
        }

        public DataTable GetTopSellingProducts(DateTime fromDate, DateTime toDate, int limit = 20)
        {
            string from = fromDate.Date.ToString("yyyy-MM-dd");
            string to = toDate.Date.ToString("yyyy-MM-dd");

            string sql = $@"
                SELECT
                    p.part_name AS product_name,
                    SUM(oi.quantity) AS quantity_sold,
                    AVG(oi.price) AS unit_price,
                    SUM(oi.quantity * oi.price) AS total_sales,
                    SUM(oi.quantity * (oi.price - COALESCE(p.purchase_price, 0))) AS profit
                FROM order_items oi
                INNER JOIN orders o ON oi.order_id = o.order_id
                INNER JOIN parts p ON oi.part_id = p.id
                WHERE date(o.order_date) BETWEEN date(@from) AND date(@to)
                  AND {CompletedOrdersFilter}
                GROUP BY p.part_name
                ORDER BY quantity_sold DESC
                LIMIT {Math.Max(1, limit)}";

            return DatabaseHelper.ExecuteDataTable(sql,
                new SqliteParameter("@from", from),
                new SqliteParameter("@to", to));
        }

        public DataTable GetTopSellingCategories(DateTime fromDate, DateTime toDate, int limit = 20)
        {
            string from = fromDate.Date.ToString("yyyy-MM-dd");
            string to = toDate.Date.ToString("yyyy-MM-dd");

            string sql = $@"
                SELECT
                    COALESCE(c.category_name, 'Uncategorized') AS category_name,
                    SUM(oi.quantity) AS quantity_sold,
                    SUM(oi.quantity * oi.price) AS total_sales,
                    SUM(oi.quantity * (oi.price - COALESCE(p.purchase_price, 0))) AS profit
                FROM order_items oi
                INNER JOIN orders o ON oi.order_id = o.order_id
                INNER JOIN parts p ON oi.part_id = p.id
                LEFT JOIN categories c ON p.category_id = c.id
                WHERE date(o.order_date) BETWEEN date(@from) AND date(@to)
                  AND {CompletedOrdersFilter}
                GROUP BY COALESCE(c.category_name, 'Uncategorized')
                ORDER BY quantity_sold DESC
                LIMIT {Math.Max(1, limit)}";

            return DatabaseHelper.ExecuteDataTable(sql,
                new SqliteParameter("@from", from),
                new SqliteParameter("@to", to));
        }

        /// <summary>
        /// Full sold-products list for Excel export (no limit).
        /// </summary>
        public DataTable GetSoldProductsDetail(DateTime fromDate, DateTime toDate)
        {
            string from = fromDate.Date.ToString("yyyy-MM-dd");
            string to = toDate.Date.ToString("yyyy-MM-dd");

            string sql = $@"
                SELECT
                    p.part_name AS product_name,
                    SUM(oi.quantity) AS quantity_sold,
                    AVG(oi.price) AS unit_price,
                    SUM(oi.quantity * oi.price) AS total_sales,
                    SUM(oi.quantity * COALESCE(p.purchase_price, 0)) AS total_cost,
                    SUM(oi.quantity * (oi.price - COALESCE(p.purchase_price, 0))) AS profit
                FROM order_items oi
                INNER JOIN orders o ON oi.order_id = o.order_id
                INNER JOIN parts p ON oi.part_id = p.id
                WHERE date(o.order_date) BETWEEN date(@from) AND date(@to)
                  AND {CompletedOrdersFilter}
                GROUP BY p.part_name
                ORDER BY total_sales DESC";

            return DatabaseHelper.ExecuteDataTable(sql,
                new SqliteParameter("@from", from),
                new SqliteParameter("@to", to));
        }

        /// <summary>Range from explicit from/to (swapped if reversed), else from the preset.</summary>
        public (DateTime from, DateTime to) ResolveRange(string preset, string from, string to)
        {
            if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to)
                && DateTime.TryParse(from, out var f) && DateTime.TryParse(to, out var t))
                return t < f ? (t, f) : (f, t);
            return GetPresetRange(string.IsNullOrWhiteSpace(preset) ? "Monthly" : preset);
        }

        /// <summary>Order / item / unpaid / returns counts for the period.</summary>
        public DataRow GetActivity(DateTime fromDate, DateTime toDate)
        {
            var dt = DatabaseHelper.ExecuteDataTable($@"
                SELECT
                    (SELECT COUNT(*) FROM orders o
                      WHERE date(o.order_date) BETWEEN date(@from) AND date(@to) AND {CompletedOrdersFilter}) AS order_count,
                    (SELECT COALESCE(SUM(oi.quantity), 0) FROM order_items oi INNER JOIN orders o ON oi.order_id = o.order_id
                      WHERE date(o.order_date) BETWEEN date(@from) AND date(@to) AND {CompletedOrdersFilter}) AS items_sold,
                    (SELECT COUNT(*) FROM orders o
                      WHERE date(o.order_date) BETWEEN date(@from) AND date(@to) AND {CompletedOrdersFilter}
                        AND LOWER(COALESCE(o.payment_status, 'Paid')) <> 'paid') AS unpaid_count,
                    (SELECT COALESCE(SUM(o.total_amount), 0) FROM orders o
                      WHERE date(o.order_date) BETWEEN date(@from) AND date(@to) AND {CompletedOrdersFilter}
                        AND LOWER(COALESCE(o.payment_status, 'Paid')) <> 'paid') AS unpaid_total,
                    (SELECT COUNT(*) FROM returns r WHERE date(r.return_date) BETWEEN date(@from) AND date(@to)) AS returns_count,
                    (SELECT COALESCE(SUM(r.total_refund), 0) FROM returns r WHERE date(r.return_date) BETWEEN date(@from) AND date(@to)) AS returns_total,
                    (SELECT COUNT(*) FROM expenses e
                      WHERE date(e.expense_date) BETWEEN date(@from) AND date(@to) AND e.date_deleted IS NULL) AS expense_count",
                new SqliteParameter("@from", fromDate.Date.ToString("yyyy-MM-dd")),
                new SqliteParameter("@to", toDate.Date.ToString("yyyy-MM-dd")));
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>Every product sold in the period, with category, cost, profit and weight flag.</summary>
        public DataTable GetProductsReport(DateTime fromDate, DateTime toDate)
        {
            return DatabaseHelper.ExecuteDataTable($@"
                SELECT
                    p.part_name AS product_name,
                    COALESCE(c.category_name, 'Uncategorized') AS category_name,
                    COALESCE(p.sell_by_weight, 0) AS sell_by_weight,
                    COALESCE(p.unit_of_measure, '') AS unit,
                    SUM(oi.quantity) AS quantity_sold,
                    AVG(oi.price) AS unit_price,
                    SUM(oi.quantity * oi.price) AS total_sales,
                    SUM(oi.quantity * COALESCE(p.purchase_price, 0)) AS total_cost,
                    SUM(oi.quantity * (oi.price - COALESCE(p.purchase_price, 0))) AS profit
                FROM order_items oi
                INNER JOIN orders o ON oi.order_id = o.order_id
                INNER JOIN parts p ON oi.part_id = p.id
                LEFT JOIN categories c ON p.category_id = c.id
                WHERE date(o.order_date) BETWEEN date(@from) AND date(@to)
                  AND {CompletedOrdersFilter}
                GROUP BY p.id
                ORDER BY total_sales DESC",
                new SqliteParameter("@from", fromDate.Date.ToString("yyyy-MM-dd")),
                new SqliteParameter("@to", toDate.Date.ToString("yyyy-MM-dd")));
        }

        /// <summary>Every expense in the period, one row each.</summary>
        public DataTable GetExpensesList(DateTime fromDate, DateTime toDate)
        {
            return DatabaseHelper.ExecuteDataTable(@"
                SELECT e.expense_id, e.expense_date, COALESCE(e.category, '') AS category,
                       COALESCE(s.supplier_name, '') AS supplier_name, COALESCE(e.liters, 0) AS liters,
                       e.amount, COALESCE(e.is_paid, 1) AS is_paid, COALESCE(e.description, '') AS description
                FROM expenses e
                LEFT JOIN suppliers s ON s.id = e.supplier_id
                WHERE date(e.expense_date) BETWEEN date(@from) AND date(@to)
                  AND e.date_deleted IS NULL
                ORDER BY e.expense_date DESC, e.expense_id DESC",
                new SqliteParameter("@from", fromDate.Date.ToString("yyyy-MM-dd")),
                new SqliteParameter("@to", toDate.Date.ToString("yyyy-MM-dd")));
        }

        /// <summary>Sales and expenses per day (or per month for ranges longer than 62 days), empty periods filled with 0.</summary>
        public System.Collections.Generic.List<object> GetTrend(DateTime fromDate, DateTime toDate)
        {
            bool monthly = (toDate.Date - fromDate.Date).TotalDays > 62;
            string fmt = monthly ? "%Y-%m" : "%Y-%m-%d";
            var p1 = new SqliteParameter("@from", fromDate.Date.ToString("yyyy-MM-dd"));
            var p2 = new SqliteParameter("@to", toDate.Date.ToString("yyyy-MM-dd"));
            var sales = DatabaseHelper.ExecuteDataTable($@"
                SELECT strftime('{fmt}', o.order_date) AS k, COALESCE(SUM(oi.quantity * oi.price), 0) AS v
                FROM order_items oi INNER JOIN orders o ON oi.order_id = o.order_id
                WHERE date(o.order_date) BETWEEN date(@from) AND date(@to) AND {CompletedOrdersFilter}
                GROUP BY k", p1, p2);
            var expenses = DatabaseHelper.ExecuteDataTable($@"
                SELECT strftime('{fmt}', e.expense_date) AS k, COALESCE(SUM(e.amount), 0) AS v
                FROM expenses e
                WHERE date(e.expense_date) BETWEEN date(@from) AND date(@to) AND e.date_deleted IS NULL
                GROUP BY k",
                new SqliteParameter("@from", fromDate.Date.ToString("yyyy-MM-dd")),
                new SqliteParameter("@to", toDate.Date.ToString("yyyy-MM-dd")));

            var s = new System.Collections.Generic.Dictionary<string, decimal>();
            foreach (DataRow r in sales.Rows) if (r["k"] != DBNull.Value) s[r["k"].ToString()] = Convert.ToDecimal(r["v"]);
            var x = new System.Collections.Generic.Dictionary<string, decimal>();
            foreach (DataRow r in expenses.Rows) if (r["k"] != DBNull.Value) x[r["k"].ToString()] = Convert.ToDecimal(r["v"]);

            var list = new System.Collections.Generic.List<object>();
            var cur = monthly ? new DateTime(fromDate.Year, fromDate.Month, 1) : fromDate.Date;
            while (cur <= toDate.Date)
            {
                string key = monthly ? cur.ToString("yyyy-MM") : cur.ToString("yyyy-MM-dd");
                list.Add(new { period = key, sales = s.TryGetValue(key, out var sv) ? sv : 0m, expenses = x.TryGetValue(key, out var ev) ? ev : 0m });
                cur = monthly ? cur.AddMonths(1) : cur.AddDays(1);
            }
            return list;
        }
    }
}
