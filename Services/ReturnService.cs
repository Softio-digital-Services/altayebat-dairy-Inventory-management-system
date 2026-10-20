using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using InventorySystem.Helpers;

namespace InventorySystem.Services
{
    public class ReturnService
    {
        public void ProcessReturn(int orderId, List<ReturnItemInfo> itemsToReturn, string reason)
        {
            try
            {
                // 1. Calculate total refund
                decimal totalRefund = 0;
                foreach (var item in itemsToReturn)
                {
                    totalRefund += item.RefundAmount;
                }

                // 2. Insert Return Record
                string sqlReturn = "INSERT INTO returns (order_id, return_date, total_refund, reason, performed_by) " +
                                  "VALUES (@oid, datetime('now'), @refund, @reason, @user); SELECT last_insert_rowid();";
                
                object returnIdObj = DatabaseHelper.ExecuteScalar<object>(sqlReturn,
                    new SqliteParameter("@oid", orderId),
                    new SqliteParameter("@refund", totalRefund),
                    new SqliteParameter("@reason", reason),
                    new SqliteParameter("@user", UserSession.Username));
                
                int returnId = Convert.ToInt32(returnIdObj);

                // 3. Process each item
                foreach (var item in itemsToReturn)
                {
                    // Insert return item record
                    string sqlItem = "INSERT INTO return_items (return_id, part_id, quantity, refund_amount) " +
                                     "VALUES (@rid, @pid, @qty, @refund)";
                    DatabaseHelper.ExecuteNonQuery(sqlItem,
                        new SqliteParameter("@rid", returnId),
                        new SqliteParameter("@pid", item.PartId),
                        new SqliteParameter("@qty", item.Quantity),
                        new SqliteParameter("@refund", item.RefundAmount));

                    // Update stock
                    string sqlStock = "UPDATE parts SET quantity_in_stock = quantity_in_stock + @qty WHERE id = @pid AND is_stock_tracked = 1 AND (item_type IS NULL OR item_type != 'Service')";
                    DatabaseHelper.ExecuteNonQuery(sqlStock,
                        new SqliteParameter("@qty", item.Quantity),
                        new SqliteParameter("@pid", item.PartId));

                    // Log stock movement
                    string sqlLog = "INSERT INTO stock_movements (part_id, movement_type, quantity, performed_by, notes, movement_date) " +
                                    "VALUES (@pid, 'RETURN', @qty, @user, @notes, datetime('now'))";
                    DatabaseHelper.ExecuteNonQuery(sqlLog,
                        new SqliteParameter("@pid", item.PartId),
                        new SqliteParameter("@qty", item.Quantity),
                        new SqliteParameter("@user", UserSession.Username),
                        new SqliteParameter("@notes", "Returned from Order #" + orderId));
                }

                // 4. Update Customer Balance if applicable
                // First get customer_id from order
                object cidObj = DatabaseHelper.ExecuteScalar<object>("SELECT customer_id FROM orders WHERE order_id = " + orderId);
                if (cidObj != null && cidObj != DBNull.Value)
                {
                    int customerId = Convert.ToInt32(cidObj);
                    if (customerId > 0)
                    {
                        // Reduce customer balance
                        string sqlBalance = "UPDATE customers SET current_balance = current_balance - @refund WHERE customer_id = @cid";
                        DatabaseHelper.ExecuteNonQuery(sqlBalance,
                            new SqliteParameter("@refund", totalRefund),
                            new SqliteParameter("@cid", customerId));

                        // Record payment record (negative payment/credit note)
                        string sqlPayRecord = "INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes) VALUES ('Customer', @cid, @amount, datetime('now'), @notes)";
                        DatabaseHelper.ExecuteNonQuery(sqlPayRecord,
                            new SqliteParameter("@cid", customerId),
                            new SqliteParameter("@amount", -totalRefund),
                            new SqliteParameter("@notes", "[Return] Refund for Order #" + orderId)
                        );
                    }
                }

                // 5. Update Order Status if fully returned (optional logic)
                // For now just log it
                DatabaseHelper.LogTransaction("RETURN", "Order #" + orderId, "Refund Total: " + totalRefund);
                
                GlobalEvents.RaiseOrdersUpdated();
                GlobalEvents.RaiseInventoryUpdated();
                GlobalEvents.RaiseCustomersUpdated();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ReturnService.ProcessReturn");
                throw;
            }
        }
        public void ProcessBlindReturn(List<ReturnItemInfo> itemsToReturn, string reason, int customerId = -1)
        {
            try
            {
                // 1. Calculate total refund
                decimal totalRefund = 0;
                foreach (var item in itemsToReturn)
                {
                    totalRefund += item.RefundAmount;
                }

                // 2. Insert Return Record (order_id is NULL)
                string sqlReturn = "INSERT INTO returns (return_date, total_refund, reason, performed_by) " +
                                  "VALUES (datetime('now'), @refund, @reason, @user); SELECT last_insert_rowid();";
                
                object returnIdObj = DatabaseHelper.ExecuteScalar<object>(sqlReturn,
                    new SqliteParameter("@refund", totalRefund),
                    new SqliteParameter("@reason", reason ?? (object)DBNull.Value),
                    new SqliteParameter("@user", UserSession.Username));
                
                int returnId = Convert.ToInt32(returnIdObj);

                // 3. Process each item
                foreach (var item in itemsToReturn)
                {
                    // Insert return item record
                    string sqlItem = "INSERT INTO return_items (return_id, part_id, quantity, refund_amount) " +
                                     "VALUES (@rid, @pid, @qty, @refund)";
                    DatabaseHelper.ExecuteNonQuery(sqlItem,
                        new SqliteParameter("@rid", returnId),
                        new SqliteParameter("@pid", item.PartId),
                        new SqliteParameter("@qty", item.Quantity),
                        new SqliteParameter("@refund", item.RefundAmount));

                    // Update stock
                    string sqlStock = "UPDATE parts SET quantity_in_stock = quantity_in_stock + @qty WHERE id = @pid AND is_stock_tracked = 1 AND (item_type IS NULL OR item_type != 'Service')";
                    DatabaseHelper.ExecuteNonQuery(sqlStock,
                        new SqliteParameter("@qty", item.Quantity),
                        new SqliteParameter("@pid", item.PartId));

                    // Log stock movement
                    string sqlLog = "INSERT INTO stock_movements (part_id, movement_type, quantity, performed_by, notes, movement_date) " +
                                    "VALUES (@pid, 'RETURN', @qty, @user, @notes, datetime('now'))";
                    DatabaseHelper.ExecuteNonQuery(sqlLog,
                        new SqliteParameter("@pid", item.PartId),
                        new SqliteParameter("@qty", item.Quantity),
                        new SqliteParameter("@user", UserSession.Username),
                        new SqliteParameter("@notes", "Unlinked Return (Blind Return)"));
                }

                // 4. Update Customer Balance if applicable
                if (customerId > 0)
                {
                    string sqlUpdateBalance = "UPDATE customers SET current_balance = current_balance - @refund WHERE id = @cid";
                    DatabaseHelper.ExecuteNonQuery(sqlUpdateBalance,
                        new SqliteParameter("@refund", totalRefund),
                        new SqliteParameter("@cid", customerId)
                    );

                    string sqlTrans = "INSERT INTO customer_transactions (customer_id, transaction_date, amount, transaction_type, reference, notes) " +
                                      "VALUES (@cid, datetime('now'), @amt, 'PAYMENT', @ref, @notes)";
                    DatabaseHelper.ExecuteNonQuery(sqlTrans,
                        new SqliteParameter("@cid", customerId),
                        new SqliteParameter("@amt", totalRefund),
                        new SqliteParameter("@ref", "Return #" + returnId),
                        new SqliteParameter("@notes", "[Return] Refund for Blind Return")
                    );
                }

                DatabaseHelper.LogTransaction("BLIND_RETURN", "Unlinked Items", "Refund Total: " + totalRefund);
                
                GlobalEvents.RaiseInventoryUpdated();
                if (customerId > 0) GlobalEvents.RaiseCustomersUpdated();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ReturnService.ProcessBlindReturn");
                throw;
            }
        }
    }

    public class ReturnItemInfo
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public decimal RefundAmount { get; set; }
    }
}
