using System;
using System.Data;
using Microsoft.Data.Sqlite;
using InventorySystem.Helpers;
using System.Collections.Generic;
using System.Globalization;

namespace InventorySystem.Services
{
    public class Notification
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // "LowStock", "Order", "Info"
        public string Target { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class DashboardService
    {
        // - Simple scalar queries -

        public decimal GetTotalInventoryValue()
        {
            return DatabaseHelper.ExecuteScalar<decimal>(
                "SELECT COALESCE(SUM(selling_price * quantity_in_stock), 0) FROM parts WHERE date_deleted IS NULL");
        }

        public int GetTotalItems()
        {
            return DatabaseHelper.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM parts WHERE date_deleted IS NULL");
        }

        public int GetLowStockCount()
        {
            return DatabaseHelper.GetCount("parts",
                "quantity_in_stock <= minimum_stock_level AND date_deleted IS NULL");
        }

        public int GetPendingOrdersCount()
        {
            return DatabaseHelper.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM orders WHERE payment_status = 'Unpaid'");
        }

        public int GetUnpaidExpensesCount()
        {
            return DatabaseHelper.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM expenses WHERE is_paid = 0 AND date_deleted IS NULL");
        }

        public decimal GetAverageOrderValue()
        {
            return DatabaseHelper.ExecuteScalar<decimal>(
                "SELECT COALESCE(AVG(total_amount), 0) FROM orders");
        }

        // - Payment reminders -- SQLite: julianday() for date arithmetic -

        public int GetPaymentRemindersCount()
        {
            int c = DatabaseHelper.ExecuteScalar<int>(@"
                SELECT COUNT(*) FROM customers
                WHERE date_deleted IS NULL
                  AND payment_due_date IS NOT NULL
                  AND current_balance > 0
                  AND (julianday(payment_due_date) - julianday('now')) <= reminder_days");

            int s = DatabaseHelper.ExecuteScalar<int>(@"
                SELECT COUNT(*) FROM suppliers
                WHERE date_deleted IS NULL
                  AND payment_due_date IS NOT NULL
                  AND balance_due > 0
                  AND (julianday(payment_due_date) - julianday('now')) <= reminder_days");

            return c + s;
        }

        // - Orders count -- SQLite: date() strips time portion -

        public int GetOrdersCount(string scope = "Today")
        {
            if (scope == "Today")
                return DatabaseHelper.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM orders WHERE date(order_date) = date('now')");
            return 0;
        }

        // - Revenue -- SQLite: date() for today comparison -

        public decimal GetSales(string scope = "Today")
        {
            if (scope == "Today")
            {
                decimal revenue = DatabaseHelper.ExecuteScalar<decimal>(
                    "SELECT COALESCE(SUM(total_amount), 0) FROM orders WHERE date(order_date) = date('now')");
                decimal expenses = DatabaseHelper.ExecuteScalar<decimal>(
                    "SELECT COALESCE(SUM(amount), 0) FROM expenses WHERE date(expense_date) = date('now') AND date_deleted IS NULL");
                return revenue - expenses;
            }
            return 0;
        }

        // - YTD -- SQLite: strftime('%Y') -

        public decimal GetTotalSalesYTD()
        {
            return DatabaseHelper.ExecuteScalar<decimal>(
                "SELECT COALESCE(SUM(total_amount), 0) FROM orders WHERE strftime('%Y', order_date) = strftime('%Y', 'now')");
        }

        // - Top selling items -- SQLite: LIMIT instead of TOP -

        public DataTable GetTopSellingItems(int limit = 5)
        {
            string sql = $@"
                SELECT p.part_name, SUM(oi.quantity) as total_sold
                FROM order_items oi
                INNER JOIN parts p ON oi.part_id = p.id
                GROUP BY p.part_name
                ORDER BY total_sold DESC
                LIMIT {limit}";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        // - Recent activity -- SQLite: LIMIT -

        public DataTable GetRecentActivity(int limit = 10)
        {
            return DatabaseHelper.ExecuteDataTable(
                $"SELECT action_type, description, timestamp FROM transactions ORDER BY timestamp DESC LIMIT {limit}");
        }

        // - Stock distribution -

        public Dictionary<string, int> GetStockDistribution()
        {
            string sql = @"
                SELECT
                    CASE
                        WHEN quantity_in_stock = 0                             THEN 'Out of Stock'
                        WHEN quantity_in_stock <= minimum_stock_level           THEN 'Low Stock'
                        WHEN quantity_in_stock <= minimum_stock_level * 2       THEN 'Moderate Stock'
                        ELSE 'Well Stocked'
                    END as stock_level,
                    COUNT(*) as item_count
                FROM parts
                WHERE date_deleted IS NULL
                GROUP BY 1";
            var dt   = DatabaseHelper.ExecuteDataTable(sql);
            var data = new Dictionary<string, int>();
            foreach (DataRow row in dt.Rows)
                data[row["stock_level"].ToString()] = Convert.ToInt32(row["item_count"]);
            return data;
        }

        // - Weekly revenue -- SQLite: date() and date('now','-7 days') -

        public Dictionary<string, decimal> GetWeeklyRevenue()
        {
            string sql = @"
                SELECT Date, SUM(Total) as NetTotal FROM (
                    SELECT date(order_date)   as Date, SUM(total_amount) as Total
                    FROM orders
                    WHERE date(order_date) >= date('now', '-7 days')
                    GROUP BY 1
                    UNION ALL
                    SELECT date(expense_date) as Date, -SUM(amount) as Total
                    FROM expenses
                    WHERE date(expense_date) >= date('now', '-7 days')
                      AND date_deleted IS NULL
                    GROUP BY 1
                ) t
                GROUP BY Date
                ORDER BY Date ASC";

            var dt   = DatabaseHelper.ExecuteDataTable(sql);
            var data = new Dictionary<string, decimal>();
            foreach (DataRow row in dt.Rows)
            {
                string rawDate = row["Date"].ToString();
                if (DateTime.TryParse(rawDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d))
                {
                    string key = d.ToString("dd/MM");
                    data[key] = Convert.ToDecimal(row["NetTotal"]);
                }
            }
            return data;
        }

        // - Monthly sales trend -- SQLite date arithmetic -

        public Dictionary<string, int> GetMonthlySalesTrend()
        {
            string sql = @"
                SELECT date(order_date) as Date, COUNT(*) as Count
                FROM orders
                WHERE date(order_date) >= date('now', '-30 days')
                GROUP BY 1
                ORDER BY Date ASC";

            var dt   = DatabaseHelper.ExecuteDataTable(sql);
            var data = new Dictionary<string, int>();
            foreach (DataRow row in dt.Rows)
            {
                string rawDate = row["Date"].ToString();
                if (DateTime.TryParse(rawDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d))
                {
                    string key = d.ToString("dd/MM");
                    data[key] = Convert.ToInt32(row["Count"]);
                }
            }
            return data;
        }

        // - Sales by category -

        public DataTable GetSalesByCategory()
        {
            string sql = @"
                SELECT c.category_name, SUM(oi.quantity * oi.price) as total_sales
                FROM order_items oi
                INNER JOIN parts p ON oi.part_id = p.id
                INNER JOIN categories c ON p.category_id = c.id
                GROUP BY c.category_name
                ORDER BY total_sales DESC";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        // - Monthly revenue (last 6 months) -- SQLite: strftime -

        public Dictionary<string, decimal> GetMonthlyRevenue()
        {
            string sql = @"
                SELECT MonthLabel, SUM(Total) as NetTotal FROM (
                    SELECT strftime('%m/%Y', order_date) as MonthLabel, 
                           strftime('%Y-%m', order_date) as SortKey,
                           SUM(total_amount) as Total
                    FROM orders
                    WHERE date(order_date) >= date('now', '-6 months')
                    GROUP BY 1, 2
                    UNION ALL
                    SELECT strftime('%m/%Y', expense_date) as MonthLabel,
                           strftime('%Y-%m', expense_date) as SortKey,
                           -SUM(amount) as Total
                    FROM expenses
                    WHERE date(expense_date) >= date('now', '-6 months')
                      AND date_deleted IS NULL
                    GROUP BY 1, 2
                ) t
                GROUP BY MonthLabel
                ORDER BY MIN(SortKey) ASC";

            var dt   = DatabaseHelper.ExecuteDataTable(sql);
            var data = new Dictionary<string, decimal>();
            foreach (DataRow row in dt.Rows)
            {
                data[row["MonthLabel"].ToString()] = Convert.ToDecimal(row["NetTotal"]);
            }
            return data;
        }

        // - Notifications -

        public List<Notification> GetNotifications(string lang = null)
        {
            var notifications = new List<Notification>();
            bool arabic = !string.IsNullOrEmpty(lang)
                ? lang.StartsWith("ar", StringComparison.OrdinalIgnoreCase)
                : LocalizationManager.IsArabic;
            string L(string key) => LocalizationManager.GetStringLang(key, arabic);

            void TryAdd(string section, Action action)
            {
                try { action(); }
                catch (Exception ex) { ErrorLogger.LogError(ex, "GetNotifications:" + section); }
            }

            // Low stock and out of stock only
            TryAdd("StockAlerts", () =>
            {
                var stockAlerts = DatabaseHelper.ExecuteDataTable(
                    "SELECT part_name, quantity_in_stock, minimum_stock_level FROM parts " +
                    "WHERE quantity_in_stock <= minimum_stock_level AND date_deleted IS NULL " +
                    "ORDER BY quantity_in_stock ASC, part_name ASC");
                foreach (DataRow row in stockAlerts.Rows)
                {
                    string partName = Convert.ToString(row["part_name"]);
                    int qty = Convert.ToInt32(row["quantity_in_stock"]);
                    bool outOfStock = qty <= 0;
                    string type = outOfStock ? "OutOfStock" : "LowStock";
                    notifications.Add(new Notification
                    {
                        Key       = $"{type}:{partName}",
                        Type      = type,
                        Title     = L(outOfStock ? "Notif_OutOfStock" : "Notif_LowStock"),
                        Message   = outOfStock
                            ? string.Format(L("Notif_OutOfStockMsg"), partName)
                            : string.Format(L("Notif_LowStockMsg"), partName, qty),
                        Target    = "btnInventory",
                        Timestamp = DateTime.Now
                    });
                }
            });

            // License expiration warning (3 days or fewer)
            TryAdd("LicenseAlert", () =>
            {
                var lic = LicenseManager.GetCurrentLicense();
                if (lic != null && lic.IsValid() && lic.DaysRemaining() <= 3 && lic.DaysRemaining() > 0)
                {
                    int days = lic.DaysRemaining();
                    notifications.Add(new Notification
                    {
                        Key       = $"LicenseExpiring:{days}",
                        Type      = "LicenseExpiring",
                        Title     = L("Notif_LicenseExpiringTitle"),
                        Message   = string.Format(L("Notif_LicenseExpiringMsg"), days),
                        Target    = "settings",
                        Timestamp = DateTime.Now
                    });
                }
            });

            return notifications;
        }
    }
}
