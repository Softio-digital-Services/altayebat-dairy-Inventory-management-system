using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using InventorySystem.Helpers;

namespace InventorySystem.Services
{
    public class OrderItem
    {
        public int PartId { get; set; }
        public string PartName { get; set; }
        public string Description { get; set; }
        public string PartImage { get; set; }
        public int Quantity { get; set; }
        /// <summary>When &gt; 0, stock is reduced by this amount instead of Quantity (e.g. grams for weighed items).</summary>
        public int StockDeductQty { get; set; }
        /// <summary>When true, sale is recorded but inventory is not reduced (car stock / quick sale).</summary>
        public bool SkipStock { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
    }

    public class OrderService
    {
        /// <summary>
        /// Places a new order, updates stock, and manages customer balance.
        /// </summary>
        /// <param name="customerId">Customer ID (-1 for Walk-in)</param>
        /// <param name="items">List of items in the cart</param>
        /// <param name="totalAmount">Total amount of the order</param>
        /// <param name="isPaid">Whether the order is fully paid</param>
        /// <returns>The ID of the created order</returns>
        public int PlaceOrder(int customerId, List<OrderItem> items, decimal totalAmount, bool isPaid, string orderStatus = "Completed", DateTime? dueDate = null, string shippingAddress = null, DateTime? deliveryDate = null)
        {
            // 1. Determine Status
            // Walk-in is always paid (enforced by UI, but logic here: if walk-in, force paid?)
            bool isWalkIn = customerId == -1;
            string paymentStatus = isPaid ? "Paid" : "Unpaid";
            if (orderStatus == "Draft") paymentStatus = "Pending"; // Drafts are pending payment automatically
            
            decimal amountPaid = isPaid ? totalAmount : 0;
            
            // 2. Insert Order
            string sqlOrder;
            if (isWalkIn)
            {
                sqlOrder = "INSERT INTO orders (order_date, total_amount, payment_status, amount_paid, status, shipping_address, delivery_date, due_date) " +
                           "VALUES (datetime('now'), @total, @status, @paid, @ostatus, @shipAddr, @delDate, @dueDate); SELECT last_insert_rowid();";
            }
            else
            {
                sqlOrder = "INSERT INTO orders (order_date, total_amount, payment_status, amount_paid, customer_id, status, shipping_address, delivery_date, due_date) " +
                           "VALUES (datetime('now'), @total, @status, @paid, @cid, @ostatus, @shipAddr, @delDate, @dueDate); SELECT last_insert_rowid();";
            }

            long orderIdLong = DatabaseHelper.ExecuteScalar<long>(sqlOrder,
                new SqliteParameter("@total", totalAmount),
                new SqliteParameter("@status", paymentStatus),
                new SqliteParameter("@paid", amountPaid),
                new SqliteParameter("@cid", customerId),
                new SqliteParameter("@ostatus", orderStatus),
                new SqliteParameter("@shipAddr", string.IsNullOrEmpty(shippingAddress) ? (object)DBNull.Value : shippingAddress),
                new SqliteParameter("@delDate", deliveryDate.HasValue ? (object)deliveryDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : DBNull.Value),
                new SqliteParameter("@dueDate", dueDate.HasValue ? (object)dueDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : DBNull.Value)
            );
            int orderId = Convert.ToInt32(orderIdLong);

            // If Draft or Quotation, skip stock updates and balance updates
            if (orderStatus == "Draft" || orderStatus == "Quotation")
            {
                 // Still insert items so we can retrieve them, but DON'T update stock
                 foreach (var item in items)
                 {
                    InsertOrderItem(orderId, item);
                 }
                 GlobalEvents.RaiseOrdersUpdated();
                 return orderId;
            }

            // 3. Process Items & Update Stock (Normally)
            foreach (var item in items)
            {
                InsertOrderItem(orderId, item);

                // Skip stock for quick-sale / car-stock lines, custom items, or untracked parts
                if (item.SkipStock || item.PartId <= 0)
                    continue;

                int stockQty = item.StockDeductQty > 0 ? item.StockDeductQty : item.Quantity;
                string sqlStock = "UPDATE parts SET quantity_in_stock = quantity_in_stock - @qty WHERE id = @pid AND is_stock_tracked = 1 AND (item_type IS NULL OR item_type != 'Service')";
                DatabaseHelper.ExecuteNonQuery(sqlStock,
                    new SqliteParameter("@qty", stockQty),
                    new SqliteParameter("@pid", item.PartId)
                );
            }

            // 4. Update Customer Balance (if unpaid)
            if (!isPaid && !isWalkIn)
            {
                string sqlBalance = "UPDATE customers SET current_balance = current_balance + @total";
                List<SqliteParameter> parameters = new List<SqliteParameter> {
                    new SqliteParameter("@total", totalAmount),
                    new SqliteParameter("@cid", customerId)
                };

                if (dueDate.HasValue)
                {
                    sqlBalance += ", payment_due_date = @dueDate";
                    parameters.Add(new SqliteParameter("@dueDate", dueDate.Value));
                }

                sqlBalance += " WHERE customer_id = @cid";
                DatabaseHelper.ExecuteNonQuery(sqlBalance, parameters.ToArray());
            }

            // 5. Record in Customer History (if not Walk-in)
            if (!isWalkIn)
            {
                // Record the Sale (Payment Due)
                string sqlSaleRecord = "INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes, due_date) VALUES ('Customer', @cid, @amount, datetime('now'), @notes, @ddate)";
                DatabaseHelper.ExecuteNonQuery(sqlSaleRecord,
                    new SqliteParameter("@cid", customerId),
                    new SqliteParameter("@amount", totalAmount),
                    new SqliteParameter("@notes", "[Sale] Order #" + orderId),
                    new SqliteParameter("@ddate", (object)dueDate ?? DBNull.Value)
                );

                if (isPaid)
                {
                    // Record the Payment (Payment Received) - POS auto-payment
                    string sqlPayRecord = "INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes) VALUES ('Customer', @cid, @amount, datetime('now'), @notes)";
                    DatabaseHelper.ExecuteNonQuery(sqlPayRecord,
                        new SqliteParameter("@cid", customerId),
                        new SqliteParameter("@amount", totalAmount),
                        new SqliteParameter("@notes", "[Payment] Order #" + orderId)
                    );
                }
            }

            // 6. Log
            DatabaseHelper.LogTransaction("SALE", "Order #" + orderId, $"Total: ${totalAmount:N2}");
            
            // 7. Global Synchronization
            GlobalEvents.RaiseOrdersUpdated();     // New Order created
            GlobalEvents.RaiseInventoryUpdated(); // Stock reduced
            if (!isPaid && !isWalkIn) GlobalEvents.RaiseCustomersUpdated(); // Balance updated

            return orderId;
        }

        public System.Data.DataTable GetDrafts()
        {
             // Get orders with status 'Draft'
             string sql = @"
                SELECT o.order_id, o.order_date, COALESCE(c.full_name, 'Walk-in') as CustomerName, o.total_amount, o.customer_id
                FROM orders o
                LEFT JOIN customers c ON o.customer_id = c.customer_id
                WHERE o.status = 'Draft'
                ORDER BY o.order_date DESC";
             return DatabaseHelper.ExecuteDataTable(sql);
        }

        public System.Data.DataTable GetQuotations()
        {
             // Get orders with status 'Quotation'
             string sql = @"
                SELECT o.order_id, o.order_date, COALESCE(c.full_name, 'Walk-in') as CustomerName, o.total_amount, o.customer_id
                FROM orders o
                LEFT JOIN customers c ON o.customer_id = c.customer_id
                WHERE o.status = 'Quotation'
                ORDER BY o.order_date DESC";
             return DatabaseHelper.ExecuteDataTable(sql);
        }

        public bool ConvertToOrder(int orderId)
        {
            try
            {
                // 1. Get current order details
                var items = GetOrderItems(orderId);
                
                // 2. Perform Stock Validation
                foreach (var item in items)
                {
                    var partInfo = DatabaseHelper.ExecuteDataTable($"SELECT quantity_in_stock, is_stock_tracked, item_type FROM parts WHERE id = {item.PartId}");
                    if (partInfo.Rows.Count > 0)
                    {
                        var row = partInfo.Rows[0];
                        bool isTracked = Convert.ToInt32(row["is_stock_tracked"] == DBNull.Value ? 1 : row["is_stock_tracked"]) == 1;
                        string itemType = row["item_type"]?.ToString();
                        bool isService = string.Equals(itemType, "Service", StringComparison.OrdinalIgnoreCase);

                        if (isTracked && !isService)
                        {
                            int currentStock = Convert.ToInt32(row["quantity_in_stock"]);
                            if (currentStock < item.Quantity)
                            {
                                string partName = DatabaseHelper.ExecuteScalar<string>($"SELECT part_name FROM parts WHERE id = {item.PartId}") ?? "Unknown Item";
                                throw new Exception($"Insufficient stock for {partName}. Available: {currentStock}, Required: {item.Quantity}");
                            }
                        }
                    }
                }

                // 3. Update Status to Completed
                DatabaseHelper.ExecuteNonQuery("UPDATE orders SET status = 'Completed', order_date = datetime('now') WHERE order_id = @oid", 
                    new SqliteParameter("@oid", orderId));

                // 4. Update Stock
                foreach (var item in items)
                {
                    DatabaseHelper.ExecuteNonQuery("UPDATE parts SET quantity_in_stock = quantity_in_stock - @qty WHERE id = @pid AND is_stock_tracked = 1 AND (item_type IS NULL OR item_type != 'Service')", 
                        new SqliteParameter("@qty", item.Quantity), 
                        new SqliteParameter("@pid", item.PartId));
                }

                // 5. Log & Notify
                DatabaseHelper.LogTransaction("SALE", "Quote #" + orderId + " -> Order", "Converted quotation to order");
                GlobalEvents.RaiseOrdersUpdated();
                GlobalEvents.RaiseInventoryUpdated();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Conversion failed: " + ex.Message);
            }
        }

        public System.Data.DataRow GetOrderHeader(int orderId)
        {
            string sql = @"SELECT o.order_id, o.order_date, o.total_amount, o.status, o.payment_status, o.shipping_address, o.delivery_date, o.due_date, c.full_name as customer_name
                           FROM orders o
                           LEFT JOIN customers c ON o.customer_id = c.customer_id
                           WHERE o.order_id = @id";
            var dt = DatabaseHelper.ExecuteDataTable(sql, new SqliteParameter("@id", orderId));
            if (dt != null && dt.Rows.Count > 0) return dt.Rows[0];
            return null;
        }

        public List<OrderItem> GetOrderItems(int orderId)
        {
             string sql = @"
                SELECT COALESCE(i.part_id, 0),
                       COALESCE(NULLIF(TRIM(i.item_name), ''), p.part_name, 'Quick Sale'),
                       i.quantity, i.price,
                       COALESCE(p.description, ''),
                       COALESCE(p.part_image, '')
                FROM order_items i
                LEFT JOIN parts p ON i.part_id = p.id
                WHERE i.order_id = @oid";
             
             return DatabaseHelper.ExecuteQuery(sql, reader => new OrderItem 
             {
                 PartId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                 PartName = reader.IsDBNull(1) ? "Quick Sale" : reader.GetString(1),
                 Quantity = reader.GetInt32(2),
                 UnitPrice = reader.GetDecimal(3),
                 Description = reader.IsDBNull(4) ? "" : reader.GetString(4),
                 PartImage = reader.IsDBNull(5) ? "" : reader.GetString(5)
             }, new SqliteParameter("@oid", orderId));
        }

        public void DeleteOrder(int orderId)
        {
             // Delete items first
             DatabaseHelper.ExecuteNonQuery($"DELETE FROM order_items WHERE order_id = {orderId}");
             DatabaseHelper.ExecuteNonQuery($"DELETE FROM orders WHERE order_id = {orderId}");
        }

        private static void InsertOrderItem(int orderId, OrderItem item)
        {
            object partId = item.PartId > 0 ? (object)item.PartId : DBNull.Value;
            string itemName = string.IsNullOrWhiteSpace(item.PartName) ? null : item.PartName.Trim();
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO order_items (order_id, part_id, quantity, price, item_name) VALUES (@oid, @pid, @qty, @price, @iname)",
                new SqliteParameter("@oid", orderId),
                new SqliteParameter("@pid", partId),
                new SqliteParameter("@qty", item.Quantity),
                new SqliteParameter("@price", item.UnitPrice),
                new SqliteParameter("@iname", (object)itemName ?? DBNull.Value));
        }
    }
}
