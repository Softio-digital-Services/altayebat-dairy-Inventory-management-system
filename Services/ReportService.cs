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
    }
}
