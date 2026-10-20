using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using InventorySystem.Helpers;
using InventorySystem.Data;

namespace InventorySystem.Services
{
    /// <summary>
    /// Supplier purchase lines (name, category, qty, price, paid vs debt).
    /// Import into inventory: match by name + supplier → add stock; else create product.
    /// </summary>
    public class SupplierPurchaseService
    {
        public class ItemDto
        {
            public int Id { get; set; }
            public int SupplierId { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal LineTotal { get; set; }
            public string PaymentStatus { get; set; }
            public decimal AmountPaid { get; set; }
            public decimal Remaining { get; set; }
            public int? PartId { get; set; }
            public bool AddedToInventory { get; set; }
            public string CreatedAt { get; set; }
            public string Notes { get; set; }
        }

        public class ImportResult
        {
            public int PartId { get; set; }
            public bool Created { get; set; }
            public string Message { get; set; }
            public ItemDto Item { get; set; }
        }

        public void EnsureTable()
        {
            DatabaseHelper.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS supplier_purchase_items (
                    id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    supplier_id     INTEGER NOT NULL,
                    item_name       TEXT NOT NULL,
                    category        TEXT,
                    quantity        REAL NOT NULL DEFAULT 1,
                    unit_price      REAL NOT NULL DEFAULT 0,
                    payment_status  TEXT NOT NULL DEFAULT 'Unpaid',
                    amount_paid     REAL NOT NULL DEFAULT 0,
                    part_id         INTEGER,
                    notes           TEXT,
                    created_at      TEXT DEFAULT (datetime('now', 'localtime')),
                    date_deleted    TEXT
                );");
            try
            {
                var dt = DatabaseHelper.ExecuteDataTable("PRAGMA table_info(supplier_purchase_items)");
                bool hasCat = false;
                foreach (DataRow row in dt.Rows)
                {
                    if (string.Equals(row["name"]?.ToString(), "category", StringComparison.OrdinalIgnoreCase))
                    { hasCat = true; break; }
                }
                if (!hasCat)
                    DatabaseHelper.ExecuteNonQuery("ALTER TABLE supplier_purchase_items ADD COLUMN category TEXT;");
            }
            catch { }
        }

        public List<ItemDto> ListForSupplier(int supplierId, bool unaddedOnly = false)
        {
            EnsureTable();
            string sql = @"SELECT id, supplier_id, item_name, COALESCE(category, '') AS category,
                                  quantity, unit_price, payment_status,
                                  COALESCE(amount_paid, 0) AS amount_paid, part_id, notes, created_at
                           FROM supplier_purchase_items
                           WHERE supplier_id = @sid AND date_deleted IS NULL";
            if (unaddedOnly) sql += " AND part_id IS NULL";
            sql += " ORDER BY id DESC";

            var dt = DatabaseHelper.ExecuteDataTable(sql, new SqliteParameter("@sid", supplierId));
            return MapRows(dt);
        }

        public ItemDto AddItem(int supplierId, string name, string category, decimal qty, decimal unitPrice, bool isPaid, string notes)
        {
            EnsureTable();
            if (supplierId <= 0) throw new InvalidOperationException("Supplier required");
            if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Product name required");
            qty = qty <= 0 ? 1 : qty;
            unitPrice = Math.Max(0, unitPrice);
            decimal lineTotal = qty * unitPrice;
            string status = isPaid ? "Paid" : "Unpaid";
            decimal paid = isPaid ? lineTotal : 0;
            string cat = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim();

            DatabaseHelper.ExecuteNonQuery(
                @"INSERT INTO supplier_purchase_items
                    (supplier_id, item_name, category, quantity, unit_price, payment_status, amount_paid, notes, created_at)
                  VALUES (@sid, @name, @cat, @qty, @price, @status, @paid, @notes, datetime('now', 'localtime'))",
                new SqliteParameter("@sid", supplierId),
                new SqliteParameter("@name", name.Trim()),
                new SqliteParameter("@cat", cat),
                new SqliteParameter("@qty", qty),
                new SqliteParameter("@price", unitPrice),
                new SqliteParameter("@status", status),
                new SqliteParameter("@paid", paid),
                new SqliteParameter("@notes", notes ?? ""));

            int id = (int)DatabaseHelper.ExecuteScalar<long>("SELECT last_insert_rowid()");

            if (!isPaid && lineTotal > 0.004m)
            {
                AdjustSupplierBalance(supplierId, lineTotal);
                InsertPaymentLog(supplierId, lineTotal, $"[Debt] Purchase: {name.Trim()} x{qty}");
            }
            else if (isPaid && lineTotal > 0.004m)
            {
                InsertPaymentLog(supplierId, lineTotal, $"[Paid] Purchase: {name.Trim()} x{qty}");
            }

            return GetById(id);
        }

        /// <summary>
        /// Match inventory by name + supplier. If found: add qty and update cost.
        /// If not: create product with name, category, cost, stock.
        /// </summary>
        public ImportResult ImportToInventory(int itemId)
        {
            EnsureTable();
            var item = GetById(itemId);
            if (item == null) throw new InvalidOperationException("Purchase line not found");

            string name = (item.Name ?? "").Trim();
            string cat = string.IsNullOrWhiteSpace(item.Category) ? "General" : item.Category.Trim();
            int qtyInt = (int)Math.Max(1, Math.Round(item.Quantity));

            int? existingId = FindPartIdByNameAndSupplier(name, item.SupplierId);
            bool created;
            int partId;

            if (existingId.HasValue)
            {
                partId = existingId.Value;
                DatabaseHelper.ExecuteNonQuery(
                    @"UPDATE parts SET
                        quantity_in_stock = COALESCE(quantity_in_stock, 0) + @qty,
                        purchase_price = @cost,
                        supplier_id = @sid
                      WHERE id = @id",
                    new SqliteParameter("@qty", qtyInt),
                    new SqliteParameter("@cost", item.UnitPrice),
                    new SqliteParameter("@sid", item.SupplierId),
                    new SqliteParameter("@id", partId));
                created = false;
            }
            else
            {
                var part = new PartData
                {
                    Id = 0,
                    PartName = name,
                    CategoryName = cat,
                    PurchasePrice = item.UnitPrice,
                    SellingPrice = item.UnitPrice > 0 ? Math.Round(item.UnitPrice * 1.25m, 2) : 0,
                    QuantityInStock = qtyInt,
                    MinimumStockLevel = 0,
                    SupplierId = item.SupplierId,
                    ItemType = "Product",
                    IsSalesItem = true,
                    IsPurchaseItem = true,
                    IsStockTracked = true,
                    Status = "Active",
                    PartNumber = "",
                    Description = item.Notes ?? ""
                };
                new InventoryService().SaveProductService(part);
                partId = DatabaseHelper.ExecuteScalar<int>(
                    @"SELECT id FROM parts
                      WHERE part_name = @n AND date_deleted IS NULL
                        AND (supplier_id = @sid OR (@sid IS NULL AND supplier_id IS NULL))
                      ORDER BY id DESC LIMIT 1",
                    new SqliteParameter("@n", name),
                    new SqliteParameter("@sid", item.SupplierId));
                if (partId <= 0)
                {
                    partId = DatabaseHelper.ExecuteScalar<int>(
                        @"SELECT id FROM parts WHERE part_name = @n AND date_deleted IS NULL ORDER BY id DESC LIMIT 1",
                        new SqliteParameter("@n", name));
                }
                created = true;
            }

            LinkToPart(itemId, partId);
            GlobalEvents.RaiseInventoryUpdated();
            GlobalEvents.RaiseSuppliersUpdated();

            return new ImportResult
            {
                PartId = partId,
                Created = created,
                Message = created ? "Product created" : "Stock updated",
                Item = GetById(itemId)
            };
        }

        public ItemDto UpdatePayment(int itemId, bool markPaid, decimal? payAmount = null)
        {
            EnsureTable();
            var item = GetById(itemId);
            if (item == null) throw new InvalidOperationException("Item not found");

            decimal lineTotal = item.LineTotal;
            decimal already = item.AmountPaid;
            decimal remaining = Math.Max(0, lineTotal - already);

            if (markPaid)
            {
                decimal apply = payAmount.HasValue && payAmount.Value > 0
                    ? Math.Min(payAmount.Value, remaining)
                    : remaining;
                if (apply < 0.01m && remaining < 0.01m)
                    return item;
                if (apply < 0.01m)
                    throw new InvalidOperationException("Nothing remaining to pay");

                decimal newPaid = already + apply;
                string status = newPaid + 0.004m >= lineTotal ? "Paid" : "Partial";
                DatabaseHelper.ExecuteNonQuery(
                    @"UPDATE supplier_purchase_items
                      SET amount_paid = @p, payment_status = @s
                      WHERE id = @id",
                    new SqliteParameter("@p", newPaid),
                    new SqliteParameter("@s", status),
                    new SqliteParameter("@id", itemId));

                AdjustSupplierBalance(item.SupplierId, -apply);
                InsertPaymentLog(item.SupplierId, apply, $"[Payment] {item.Name} (line #{itemId})");
            }
            else
            {
                if (remaining > 0.004m)
                    return item;

                decimal reopen = already;
                if (reopen < 0.01m) reopen = lineTotal;
                DatabaseHelper.ExecuteNonQuery(
                    @"UPDATE supplier_purchase_items
                      SET amount_paid = 0, payment_status = 'Unpaid'
                      WHERE id = @id",
                    new SqliteParameter("@id", itemId));
                AdjustSupplierBalance(item.SupplierId, reopen);
                InsertPaymentLog(item.SupplierId, reopen, $"[Debt] Reopened: {item.Name}");
            }

            return GetById(itemId);
        }

        public void DeleteItem(int itemId)
        {
            EnsureTable();
            var item = GetById(itemId);
            if (item == null) return;

            if (item.Remaining > 0.004m)
                AdjustSupplierBalance(item.SupplierId, -item.Remaining);

            DatabaseHelper.ExecuteNonQuery(
                "UPDATE supplier_purchase_items SET date_deleted = datetime('now', 'localtime') WHERE id = @id",
                new SqliteParameter("@id", itemId));
        }

        public void LinkToPart(int itemId, int partId)
        {
            EnsureTable();
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE supplier_purchase_items SET part_id = @pid WHERE id = @id AND date_deleted IS NULL",
                new SqliteParameter("@pid", partId),
                new SqliteParameter("@id", itemId));
        }

        public ItemDto GetById(int id)
        {
            EnsureTable();
            var dt = DatabaseHelper.ExecuteDataTable(
                @"SELECT id, supplier_id, item_name, COALESCE(category, '') AS category,
                         quantity, unit_price, payment_status,
                         COALESCE(amount_paid, 0) AS amount_paid, part_id, notes, created_at
                  FROM supplier_purchase_items WHERE id = @id AND date_deleted IS NULL",
                new SqliteParameter("@id", id));
            var list = MapRows(dt);
            return list.Count > 0 ? list[0] : null;
        }

        private static int? FindPartIdByNameAndSupplier(string name, int supplierId)
        {
            object result = DatabaseHelper.ExecuteScalar<object>(
                @"SELECT id FROM parts
                  WHERE date_deleted IS NULL
                    AND LOWER(TRIM(part_name)) = LOWER(TRIM(@n))
                    AND supplier_id = @sid
                  ORDER BY id DESC LIMIT 1",
                new SqliteParameter("@n", name),
                new SqliteParameter("@sid", supplierId));
            if (result == null || result == DBNull.Value) return null;
            return Convert.ToInt32(result);
        }

        private static List<ItemDto> MapRows(DataTable dt)
        {
            var list = new List<ItemDto>();
            foreach (DataRow row in dt.Rows)
            {
                decimal qty = Convert.ToDecimal(row["quantity"]);
                decimal price = Convert.ToDecimal(row["unit_price"]);
                decimal line = qty * price;
                decimal paid = Convert.ToDecimal(row["amount_paid"]);
                int? partId = row["part_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["part_id"]);
                list.Add(new ItemDto
                {
                    Id = Convert.ToInt32(row["id"]),
                    SupplierId = Convert.ToInt32(row["supplier_id"]),
                    Name = row["item_name"]?.ToString() ?? "",
                    Category = row["category"]?.ToString() ?? "General",
                    Quantity = qty,
                    UnitPrice = price,
                    LineTotal = line,
                    PaymentStatus = row["payment_status"]?.ToString() ?? "Unpaid",
                    AmountPaid = paid,
                    Remaining = Math.Max(0, line - paid),
                    PartId = partId,
                    AddedToInventory = partId.HasValue,
                    CreatedAt = row["created_at"]?.ToString() ?? "",
                    Notes = row["notes"]?.ToString() ?? ""
                });
            }
            return list;
        }

        private static void AdjustSupplierBalance(int supplierId, decimal delta)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE suppliers SET balance_due = COALESCE(balance_due, 0) + @d WHERE id = @id",
                new SqliteParameter("@d", delta),
                new SqliteParameter("@id", supplierId));
        }

        private static void InsertPaymentLog(int supplierId, decimal amount, string notes)
        {
            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    @"INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes)
                      VALUES ('Supplier', @sid, @amt, datetime('now', 'localtime'), @notes)",
                    new SqliteParameter("@sid", supplierId),
                    new SqliteParameter("@amt", amount),
                    new SqliteParameter("@notes", notes ?? ""));
            }
            catch { }
        }
    }
}
