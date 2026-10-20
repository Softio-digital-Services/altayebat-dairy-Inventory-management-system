using System;
using System.Data;
using InventorySystem.Helpers;

namespace InventorySystem.Services
{
    public class HistoryService
    {
        public DataTable GetInventoryLogs()
        {
            return DatabaseHelper.ExecuteDataTable("SELECT timestamp as 'Date', action_type as 'Action', part_name as 'Item', description as 'Details', username as 'User' FROM transactions WHERE action_type IN ('ADD', 'EDIT', 'DELETE', 'QUICK_ADD', 'QUICK_SUB', 'STOCK_UPDATE') ORDER BY timestamp DESC");
        }

        public DataTable GetCustomerHistory()
        {
            string sql = @"
                SELECT order_date as 'Date', 'Order' as 'Type', c.full_name as 'Customer', CAST(total_amount AS DECIMAL(18,2)) as 'Amount', 'Order #' || CAST(order_id AS TEXT) as 'Details' 
                FROM orders o
                JOIN customers c ON o.customer_id = c.customer_id
                UNION ALL
                SELECT payment_date as 'Date', 'Payment' as 'Type', c.full_name as 'Customer', CAST(amount AS DECIMAL(18,2)) as 'Amount', COALESCE(notes, '') as 'Details' 
                FROM payments p
                JOIN customers c ON p.entity_id = c.customer_id
                UNION ALL
                SELECT timestamp as 'Date', CAST(REPLACE(action_type, 'CUSTOMER_', '') AS TEXT) as 'Type', 
                       CASE WHEN part_name = 'N/A' THEN 'System' ELSE part_name END as 'Customer', 
                       CAST(0 AS DECIMAL(18,2)) as 'Amount', COALESCE(description, '') as 'Details'
                FROM transactions
                WHERE action_type LIKE 'CUSTOMER_%'
                ORDER BY 1 DESC
            ";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public DataTable GetOrderHistory(bool unpaidOnly = false)
        {
             string payFilter = unpaidOnly
                ? " AND LOWER(COALESCE(o.payment_status,'')) IN ('unpaid','pending') AND o.status != 'Draft' "
                : "";
             string sql = $@"
                SELECT o.order_id as 'Order ID', o.order_date as 'Date', 
                       CAST(COALESCE(c.full_name, 'Walk-in') AS TEXT) as 'Customer', 
                       o.total_amount as 'Total', o.status as 'Status',
                       CAST(COALESCE(o.payment_status, 'Paid') AS TEXT) as 'Payment',
                       CAST((SELECT COUNT(*) FROM order_items i WHERE i.order_id = o.order_id) AS INTEGER) as 'Items'
                FROM orders o
                LEFT JOIN customers c ON o.customer_id = c.customer_id
                WHERE o.status != 'Quotation' AND o.status != 'Draft'
                {payFilter}
                ORDER BY o.order_date DESC";
             return DatabaseHelper.ExecuteDataTable(sql);
        }

        public DataTable GetQuotationHistory()
        {
             string sql = @"
                SELECT o.order_id as 'ID', o.order_date as 'Date', 
                       CAST(COALESCE(c.full_name, 'Walk-in') AS TEXT) as 'Customer', 
                       o.total_amount as 'Total', 
                       CAST((SELECT COUNT(*) FROM order_items i WHERE i.order_id = o.order_id) AS INTEGER) as 'Items'
                FROM orders o
                LEFT JOIN customers c ON o.customer_id = c.customer_id
                WHERE o.status = 'Quotation'
                ORDER BY o.order_date DESC";
             return DatabaseHelper.ExecuteDataTable(sql);
        }

        public DataTable GetSupplierHistory()
        {
            string sql = @"
                SELECT payment_date as 'Date', 
                       CAST(CASE WHEN notes LIKE '%Bill%' THEN 'Bill (Owing)' ELSE 'Payment (Paid)' END AS TEXT) as 'Type', 
                       s.supplier_name as 'Supplier', 
                       CAST(amount AS DECIMAL(18,2)) as 'Amount', 
                       COALESCE(notes, '') as 'Details'
                FROM payments p
                JOIN suppliers s ON p.entity_id = s.id
                WHERE p.entity_type = 'Supplier'
                
                UNION ALL
                
                SELECT timestamp as 'Date',
                       CAST(REPLACE(action_type, 'SUPPLIER_', '') AS TEXT) as 'Type',
                       CASE WHEN part_name = 'N/A' THEN 'System' ELSE part_name END as 'Supplier',
                       CAST(0 AS DECIMAL(18,2)) as 'Amount',
                       COALESCE(description, '') as 'Details'
                FROM transactions
                WHERE action_type LIKE 'SUPPLIER_%'
                
                ORDER BY 1 DESC
            ";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public (int actions, int orders, int payments) GetTodayStats()
        {
            int actions = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM transactions WHERE CAST(timestamp as DATE) = CAST(datetime('now') as DATE)");
            int orders = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM orders WHERE CAST(order_date as DATE) = CAST(datetime('now') as DATE)");
            int payments = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM payments WHERE CAST(payment_date as DATE) = CAST(datetime('now') as DATE)");
            return (actions, orders, payments);
        }
    }
}
