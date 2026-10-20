using System;
using System.Data;
using System.Collections.Generic;

namespace InventorySystem.Data
{
    public static class DashboardAnalytics
    {
        public static decimal GetMonthlySales()
        {
            // SQLite: strftime extracts month/year from TEXT datetime column
            string sql = @"
                SELECT COALESCE(SUM(total_amount), 0)
                FROM orders
                WHERE strftime('%Y-%m', order_date) = strftime('%Y-%m', 'now')";
            return DatabaseHelper.ExecuteScalar<decimal>(sql);
        }

        public static DataTable GetLowStockItems()
        {
            string sql = @"
                SELECT part_name, quantity_in_stock
                FROM parts
                WHERE quantity_in_stock <= minimum_stock_level
                AND date_deleted IS NULL
                ORDER BY quantity_in_stock ASC
                LIMIT 10";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public static DataTable GetTopSellingItems()
        {
            string sql = @"
                SELECT p.part_name, SUM(oi.quantity) as total_sold
                FROM order_items oi
                JOIN parts p ON oi.part_id = p.id
                WHERE p.date_deleted IS NULL
                GROUP BY p.part_name
                ORDER BY total_sold DESC
                LIMIT 5";
            return DatabaseHelper.ExecuteDataTable(sql);
        }
    }
}
