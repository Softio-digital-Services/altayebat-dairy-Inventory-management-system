using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Data.Sqlite;
using InventorySystem.Helpers;

namespace InventorySystem.Services
{
    /// <summary>
    /// Customer debt settlement by unpaid order and/or order line,
    /// with date/time on each sale.
    /// </summary>
    public class CustomerDebtService
    {
        public class DebtItemDto
        {
            public int OrderItemId { get; set; }
            public int PartId { get; set; }
            public string Name { get; set; }
            public int Qty { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal LineTotal { get; set; }
            public decimal AmountPaid { get; set; }
            public decimal Remaining { get; set; }
        }

        public class DebtOrderDto
        {
            public int OrderId { get; set; }
            public string OrderDate { get; set; }
            public decimal Total { get; set; }
            public decimal AmountPaid { get; set; }
            public decimal Remaining { get; set; }
            public string PaymentStatus { get; set; }
            public List<DebtItemDto> Items { get; set; } = new List<DebtItemDto>();
        }

        public class DebtSummaryDto
        {
            public int CustomerId { get; set; }
            public string CustomerName { get; set; }
            public decimal Balance { get; set; }
            public decimal OrdersRemaining { get; set; }
            public List<DebtOrderDto> Orders { get; set; } = new List<DebtOrderDto>();
            public List<object> RecentPayments { get; set; } = new List<object>();
        }

        public class AllocationDto
        {
            public int? OrderId { get; set; }
            public int? OrderItemId { get; set; }
            public decimal Amount { get; set; }
        }

        public DebtSummaryDto GetDebtDetails(int customerId)
        {
            EnsureItemPaidColumn();

            var cust = DatabaseHelper.ExecuteDataTable(
                "SELECT full_name, COALESCE(current_balance, 0) AS bal FROM customers WHERE customer_id = @id AND date_deleted IS NULL",
                new SqliteParameter("@id", customerId));
            if (cust.Rows.Count == 0)
                throw new InvalidOperationException("Customer not found");

            string name = cust.Rows[0]["full_name"]?.ToString() ?? "";
            decimal balance = Convert.ToDecimal(cust.Rows[0]["bal"]);

            var ordersDt = DatabaseHelper.ExecuteDataTable(
                @"SELECT order_id, order_date, total_amount,
                         COALESCE(amount_paid, 0) AS amount_paid,
                         COALESCE(payment_status, 'Unpaid') AS payment_status
                  FROM orders
                  WHERE customer_id = @cid
                    AND status = 'Completed'
                    AND (
                         LOWER(COALESCE(payment_status, 'unpaid')) IN ('unpaid', 'pending', 'partial')
                         OR COALESCE(amount_paid, 0) + 0.004 < COALESCE(total_amount, 0)
                    )
                  ORDER BY order_date ASC, order_id ASC",
                new SqliteParameter("@cid", customerId));

            var orders = new List<DebtOrderDto>();
            foreach (DataRow row in ordersDt.Rows)
            {
                int oid = Convert.ToInt32(row["order_id"]);
                decimal total = Convert.ToDecimal(row["total_amount"]);
                decimal paid = Convert.ToDecimal(row["amount_paid"]);
                var items = LoadOrderItems(oid);
                decimal lineRemaining = items.Sum(i => i.Remaining);
                decimal remaining = items.Count > 0
                    ? lineRemaining
                    : Math.Max(0, total - paid);
                // Keep order visible while any line (or order) still has debt
                if (remaining <= 0.004m) continue;

                decimal orderPaid = items.Count > 0
                    ? Math.Max(0, items.Sum(i => i.LineTotal) - lineRemaining)
                    : paid;

                // Repair stale Paid status if lines still owe
                string status = row["payment_status"]?.ToString() ?? "Unpaid";
                if (remaining > 0.004m &&
                    string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase))
                {
                    status = orderPaid <= 0.004m ? "Unpaid" : "Partial";
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE orders SET payment_status = @s, amount_paid = @p WHERE order_id = @id",
                        new SqliteParameter("@s", status),
                        new SqliteParameter("@p", orderPaid),
                        new SqliteParameter("@id", oid));
                }

                orders.Add(new DebtOrderDto
                {
                    OrderId = oid,
                    OrderDate = FormatDateTime(row["order_date"]),
                    Total = total,
                    AmountPaid = orderPaid,
                    Remaining = remaining,
                    PaymentStatus = status,
                    Items = items.Where(i => i.Remaining > 0.004m).ToList()
                });
            }

            var payments = new List<object>();
            try
            {
                var payDt = DatabaseHelper.ExecuteDataTable(
                    @"SELECT payment_date, amount, COALESCE(notes, '') AS notes
                      FROM payments
                      WHERE entity_type = 'Customer' AND entity_id = @cid
                        AND (notes LIKE '%[Payment]%' OR notes LIKE '%[Debt]%')
                      ORDER BY payment_date DESC
                      LIMIT 20",
                    new SqliteParameter("@cid", customerId));
                foreach (DataRow r in payDt.Rows)
                {
                    payments.Add(new
                    {
                        date = FormatDateTime(r["payment_date"]),
                        amount = Convert.ToDecimal(r["amount"]),
                        notes = r["notes"]?.ToString() ?? ""
                    });
                }
            }
            catch { }

            return new DebtSummaryDto
            {
                CustomerId = customerId,
                CustomerName = name,
                Balance = balance,
                OrdersRemaining = orders.Sum(o => o.Remaining),
                Orders = orders,
                RecentPayments = payments
            };
        }

        /// <summary>
        /// Apply a payment. If allocations are empty, reduce balance only (legacy).
        /// With allocations: apply to order and/or order items (FIFO within order when order-only).
        /// </summary>
        public decimal ApplyPayment(int customerId, decimal amount, string note, List<AllocationDto> allocations)
        {
            EnsureItemPaidColumn();
            amount = Math.Abs(amount);
            if (amount < 0.01m)
                throw new InvalidOperationException("Amount required");

            var cust = DatabaseHelper.ExecuteDataTable(
                "SELECT customer_id FROM customers WHERE customer_id = @id AND date_deleted IS NULL",
                new SqliteParameter("@id", customerId));
            if (cust.Rows.Count == 0)
                throw new InvalidOperationException("Customer not found");

            var allocs = (allocations ?? new List<AllocationDto>())
                .Where(a => a != null && a.Amount > 0.004m)
                .ToList();

            var detail = new StringBuilder();
            decimal applied = 0;

            if (allocs.Count == 0)
            {
                // No explicit lines selected — FIFO across unpaid orders, then leftover on balance only
                var debt = GetDebtDetails(customerId);
                decimal left = amount;
                foreach (var o in debt.Orders)
                {
                    if (left < 0.01m) break;
                    decimal got = ApplyToOrderFifo(o.OrderId, Math.Min(left, o.Remaining));
                    if (got > 0)
                    {
                        applied += got;
                        left -= got;
                        detail.Append($"Order#{o.OrderId} {got:0.00}; ");
                        RefreshOrderPaidStatus(o.OrderId);
                    }
                }
                if (applied < 0.01m)
                {
                    // No open orders — legacy balance-only credit
                    applied = amount;
                    new CustomerService().UpdateBalance(customerId, -applied);
                    string n = string.IsNullOrWhiteSpace(note) ? "[Payment] Open payment" : "[Debt] " + note.Trim();
                    InsertPayment(customerId, applied, n);
                    return applied;
                }
                new CustomerService().UpdateBalance(customerId, -applied);
                string notesFifo = "[Debt] " + (string.IsNullOrWhiteSpace(note) ? detail.ToString().Trim() : note.Trim() + " | " + detail);
                if (notesFifo.Length > 400) notesFifo = notesFifo.Substring(0, 400);
                InsertPayment(customerId, applied, notesFifo);
                return applied;
            }

            // Amount field is the payment cap. Allocations say *where* to apply.
            // If amount < sum(allocations), pay only up to amount (partial on first lines).
            // If amount is 0/missing but allocations exist, use allocation sum.
            decimal requested = allocs.Sum(a => a.Amount);
            if (amount < 0.01m)
                amount = requested;
            else if (requested > 0.01m && amount > requested + 0.02m)
                amount = requested; // don't overpay beyond selected remainings

            foreach (var a in allocs)
            {
                if (applied >= amount - 0.004m) break;
                decimal want = Math.Min(a.Amount, amount - applied);
                if (want < 0.01m) continue;

                if (a.OrderItemId.HasValue && a.OrderItemId.Value > 0)
                {
                    decimal got = ApplyToItem(a.OrderItemId.Value, want, out int oid);
                    if (got > 0)
                    {
                        applied += got;
                        detail.Append($"Item#{a.OrderItemId} Order#{oid} {got:0.00}; ");
                        RefreshOrderPaidStatus(oid);
                    }
                }
                else if (a.OrderId.HasValue && a.OrderId.Value > 0)
                {
                    decimal got = ApplyToOrderFifo(a.OrderId.Value, want);
                    if (got > 0)
                    {
                        applied += got;
                        detail.Append($"Order#{a.OrderId} {got:0.00}; ");
                        RefreshOrderPaidStatus(a.OrderId.Value);
                    }
                }
            }

            if (applied < 0.01m)
                throw new InvalidOperationException("Nothing to apply — check remaining balances");

            new CustomerService().UpdateBalance(customerId, -applied);
            string notes = "[Debt] " + (string.IsNullOrWhiteSpace(note) ? detail.ToString().Trim() : note.Trim() + " | " + detail);
            if (notes.Length > 400) notes = notes.Substring(0, 400);
            InsertPayment(customerId, applied, notes);
            return applied;
        }

        private List<DebtItemDto> LoadOrderItems(int orderId)
        {
            bool hasItemName = ColumnExists("order_items", "item_name");
            string nameExpr = hasItemName
                ? "COALESCE(NULLIF(TRIM(oi.item_name), ''), p.part_name, 'Item')"
                : "COALESCE(p.part_name, 'Item')";

            var dt = DatabaseHelper.ExecuteDataTable(
                $@"SELECT oi.order_item_id,
                         COALESCE(oi.part_id, 0) AS part_id,
                         {nameExpr} AS name,
                         oi.quantity, oi.price,
                         COALESCE(oi.amount_paid, 0) AS amount_paid
                  FROM order_items oi
                  LEFT JOIN parts p ON oi.part_id = p.id
                  WHERE oi.order_id = @oid
                  ORDER BY oi.order_item_id",
                new SqliteParameter("@oid", orderId));

            var list = new List<DebtItemDto>();
            foreach (DataRow row in dt.Rows)
            {
                int qty = Convert.ToInt32(row["quantity"]);
                decimal price = Convert.ToDecimal(row["price"]);
                decimal lineTotal = qty * price;
                decimal paid = Convert.ToDecimal(row["amount_paid"]);
                // Legacy unpaid orders: amount_paid may be 0 on lines while order is Unpaid — correct
                // If order was fully paid historically, those won't appear in unpaid query
                list.Add(new DebtItemDto
                {
                    OrderItemId = Convert.ToInt32(row["order_item_id"]),
                    PartId = Convert.ToInt32(row["part_id"]),
                    Name = row["name"]?.ToString() ?? "Item",
                    Qty = qty,
                    UnitPrice = price,
                    LineTotal = lineTotal,
                    AmountPaid = paid,
                    Remaining = Math.Max(0, lineTotal - paid)
                });
            }
            return list;
        }

        private decimal ApplyToItem(int orderItemId, decimal amount, out int orderId)
        {
            orderId = 0;
            var dt = DatabaseHelper.ExecuteDataTable(
                @"SELECT order_item_id, order_id, quantity, price, COALESCE(amount_paid, 0) AS amount_paid
                  FROM order_items WHERE order_item_id = @id",
                new SqliteParameter("@id", orderItemId));
            if (dt.Rows.Count == 0) return 0;

            var row = dt.Rows[0];
            orderId = Convert.ToInt32(row["order_id"]);
            decimal lineTotal = Convert.ToInt32(row["quantity"]) * Convert.ToDecimal(row["price"]);
            decimal paid = Convert.ToDecimal(row["amount_paid"]);
            decimal remaining = Math.Max(0, lineTotal - paid);
            decimal apply = Math.Min(amount, remaining);
            if (apply < 0.01m) return 0;

            DatabaseHelper.ExecuteNonQuery(
                "UPDATE order_items SET amount_paid = COALESCE(amount_paid, 0) + @a WHERE order_item_id = @id",
                new SqliteParameter("@a", apply),
                new SqliteParameter("@id", orderItemId));
            return apply;
        }

        private decimal ApplyToOrderFifo(int orderId, decimal amount)
        {
            var items = LoadOrderItems(orderId).Where(i => i.Remaining > 0.004m).ToList();
            decimal left = amount;
            decimal applied = 0;
            foreach (var item in items)
            {
                if (left < 0.01m) break;
                decimal got = ApplyToItem(item.OrderItemId, Math.Min(left, item.Remaining), out _);
                applied += got;
                left -= got;
            }

            if (items.Count == 0)
            {
                // No lines — fall back to order.amount_paid only
                var ord = DatabaseHelper.ExecuteDataTable(
                    "SELECT total_amount, COALESCE(amount_paid, 0) AS amount_paid FROM orders WHERE order_id = @id",
                    new SqliteParameter("@id", orderId));
                if (ord.Rows.Count == 0) return 0;
                decimal total = Convert.ToDecimal(ord.Rows[0]["total_amount"]);
                decimal paid = Convert.ToDecimal(ord.Rows[0]["amount_paid"]);
                decimal rem = Math.Max(0, total - paid);
                decimal apply = Math.Min(amount, rem);
                if (apply < 0.01m) return 0;
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE orders SET amount_paid = COALESCE(amount_paid, 0) + @a WHERE order_id = @id",
                    new SqliteParameter("@a", apply),
                    new SqliteParameter("@id", orderId));
                RefreshOrderPaidStatus(orderId);
                return apply;
            }

            return applied;
        }

        private void RefreshOrderPaidStatus(int orderId)
        {
            var items = LoadOrderItems(orderId);
            decimal lineTotal = items.Sum(i => i.LineTotal);
            decimal linePaid = items.Sum(i => i.AmountPaid);

            if (items.Count == 0)
            {
                var ord = DatabaseHelper.ExecuteDataTable(
                    "SELECT total_amount, COALESCE(amount_paid, 0) AS amount_paid FROM orders WHERE order_id = @id",
                    new SqliteParameter("@id", orderId));
                if (ord.Rows.Count == 0) return;
                lineTotal = Convert.ToDecimal(ord.Rows[0]["total_amount"]);
                linePaid = Convert.ToDecimal(ord.Rows[0]["amount_paid"]);
            }
            else
            {
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE orders SET amount_paid = @p WHERE order_id = @id",
                    new SqliteParameter("@p", linePaid),
                    new SqliteParameter("@id", orderId));
            }

            string status = linePaid <= 0.004m ? "Unpaid"
                : linePaid + 0.004m >= lineTotal ? "Paid"
                : "Partial";

            DatabaseHelper.ExecuteNonQuery(
                "UPDATE orders SET payment_status = @s WHERE order_id = @id",
                new SqliteParameter("@s", status),
                new SqliteParameter("@id", orderId));
        }

        private static void InsertPayment(int customerId, decimal amount, string notes)
        {
            DatabaseHelper.ExecuteNonQuery(
                @"INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes)
                  VALUES ('Customer', @cid, @amt, datetime('now', 'localtime'), @notes)",
                new SqliteParameter("@cid", customerId),
                new SqliteParameter("@amt", amount),
                new SqliteParameter("@notes", notes ?? ""));
        }

        private static string FormatDateTime(object value)
        {
            if (value == null || value == DBNull.Value) return "—";
            string raw = value.ToString();
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt)
                || DateTime.TryParse(raw, out dt))
                return dt.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            return raw;
        }

        private static void EnsureItemPaidColumn()
        {
            try
            {
                if (!ColumnExists("order_items", "amount_paid"))
                    DatabaseHelper.ExecuteNonQuery("ALTER TABLE order_items ADD COLUMN amount_paid REAL DEFAULT 0;");
            }
            catch { }
        }

        private static bool ColumnExists(string table, string column)
        {
            try
            {
                var dt = DatabaseHelper.ExecuteDataTable($"PRAGMA table_info({table})");
                foreach (DataRow row in dt.Rows)
                {
                    if (string.Equals(row["name"]?.ToString(), column, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            catch { }
            return false;
        }
    }
}
