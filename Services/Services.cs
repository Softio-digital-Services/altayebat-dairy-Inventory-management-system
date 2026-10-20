using System;
using System.Data;
using Microsoft.Data.Sqlite;
using InventorySystem.Helpers;

namespace InventorySystem.Services
{
    public class InventoryService
    {
        public DataTable GetAllParts(string search = "", bool lowStockOnly = false, bool activeOnly = false, string category = null, int limit = 0, int offset = 0)
        {
            string sql = @"SELECT p.id as part_id, p.part_number, p.part_name, p.description,
                           COALESCE(c.category_name, 'Category') as category_name,
                           COALESCE(s.supplier_name, '') as supplier_name,
                           p.quantity_in_stock, p.selling_price, p.purchase_price, p.status, 
                           COALESCE(NULLIF(p.part_image, ''), NULLIF(c.category_image, '')) as part_image,
                           p.minimum_stock_level, p.reorder_quantity, p.location, p.barcode, p.shelf,
                           p.item_type, p.unit_of_measure, p.batch_number, p.expiry_date,
                           p.is_sales_item, p.is_purchase_item, p.is_inactive, p.tax_rate,
                           p.is_stock_tracked, p.sell_by_weight, p.price2, p.price3, p.price4
                           FROM parts p
                           LEFT JOIN categories c ON p.category_id = c.id
                           LEFT JOIN suppliers s ON p.supplier_id = s.id
                           WHERE p.date_deleted IS NULL";

            if (!string.IsNullOrEmpty(search))
                sql += $" AND (p.part_name LIKE '%{search}%' OR p.part_number LIKE '%{search}%')";
            if (lowStockOnly)
                sql += " AND p.quantity_in_stock <= p.minimum_stock_level";
            if (activeOnly)
                sql += " AND p.status = 'Active'";
            if (!string.IsNullOrEmpty(category))
            {
                if (category == "Others")
                    sql += " AND (c.category_name IS NULL OR p.category_id = 0 OR c.category_name = '')";
                else
                    sql += $" AND c.category_name = '{category}'";
            }

            sql += " ORDER BY p.id";
            if (limit > 0)
            {
                sql += $" LIMIT {limit} OFFSET {offset}";
            }
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public int GetPartsCount(string search = "", bool lowStockOnly = false, bool activeOnly = false, string category = null)
        {
            string sql = @"SELECT COUNT(*)
                           FROM parts p
                           LEFT JOIN categories c ON p.category_id = c.id
                           WHERE p.date_deleted IS NULL";

            if (!string.IsNullOrEmpty(search))
                sql += $" AND (p.part_name LIKE '%{search}%' OR p.part_number LIKE '%{search}%')";
            if (lowStockOnly)
                sql += " AND p.quantity_in_stock <= p.minimum_stock_level";
            if (activeOnly)
                sql += " AND p.status = 'Active'";
            if (!string.IsNullOrEmpty(category))
            {
                if (category == "Others")
                    sql += " AND (c.category_name IS NULL OR p.category_id = 0 OR c.category_name = '')";
                else
                    sql += $" AND c.category_name = '{category}'";
            }

            return Convert.ToInt32(DatabaseHelper.ExecuteScalar<object>(sql) ?? 0);
        }

        public void AddPart(string name, string number, string categoryName, int stock, decimal price, int minStock, string imagePath, string barcode, string location, string shelf, string status)
        {
            int categoryId = GetCategoryId(categoryName);
            string sql = "INSERT INTO parts (part_name, part_number, category_id, quantity_in_stock, selling_price, minimum_stock_level, part_image, status, barcode, location, shelf, date_added) " +
                         "VALUES (@name, @num, @cat, @stock, @price, @min, @img, @status, @barcode, @loc, @shelf, datetime('now'))";
            if (!DatabaseHelper.ExecuteNonQuery(sql,
                new SqliteParameter("@name",   name),
                new SqliteParameter("@num",    number),
                new SqliteParameter("@cat",    categoryId),
                new SqliteParameter("@stock",  stock),
                new SqliteParameter("@price",  price),
                new SqliteParameter("@min",    minStock),
                new SqliteParameter("@img",    imagePath ?? (object)DBNull.Value),
                new SqliteParameter("@status", status ?? "Active"),
                new SqliteParameter("@barcode",barcode ?? ""),
                new SqliteParameter("@loc",    location ?? ""),
                new SqliteParameter("@shelf",  shelf ?? "")))
            {
                throw new Exception("Failed to add part. Database operation failed.");
            }
            LogTransaction("ADD", $"Added Part: {name} ({number})", name);
            GlobalEvents.RaiseInventoryUpdated();
        }

        public void UpdatePart(int id, string partName, string partNumber, string categoryName, decimal sellingPrice, int quantityInStock, int minStockLevel, string imagePath, string barcode, string location, string shelf, string status)
        {
            int categoryId = GetCategoryId(categoryName);
            string sql = "UPDATE parts SET part_name=@name, part_number=@num, category_id=@cat, selling_price=@price, " +
                         "quantity_in_stock=@stock, minimum_stock_level=@min, status=@status, barcode=@barcode, location=@loc, shelf=@shelf ";
            if (imagePath != null) sql += ", part_image=@img ";
            sql += "WHERE id=@id";

            var p = new System.Collections.Generic.List<SqliteParameter>
            {
                new SqliteParameter("@name",    partName),
                new SqliteParameter("@num",     partNumber),
                new SqliteParameter("@cat",     categoryId),
                new SqliteParameter("@price",   sellingPrice),
                new SqliteParameter("@stock",   quantityInStock),
                new SqliteParameter("@min",     minStockLevel),
                new SqliteParameter("@status",  status),
                new SqliteParameter("@barcode", barcode ?? ""),
                new SqliteParameter("@loc",     location ?? ""),
                new SqliteParameter("@shelf",   shelf ?? ""),
                new SqliteParameter("@id",      id)
            };
            if (imagePath != null) p.Add(new SqliteParameter("@img", imagePath));

            if (!DatabaseHelper.ExecuteNonQuery(sql, p.ToArray()))
                throw new Exception("Failed to update part. Database operation failed.");

            LogTransaction("EDIT", $"Updated Part ID: {id}", partName);
            GlobalEvents.RaiseInventoryUpdated();
        }

        public void SaveProductService(InventorySystem.Data.PartData p)
        {
            int? excludeId = p.Id > 0 ? p.Id : (int?)null;
            p.Barcode = EnsureUniqueBarcode(p.Barcode, excludeId);

            int categoryId = GetCategoryId(p.CategoryName);
            bool isNew = p.Id == 0;
            
            string sql;
            if (isNew)
            {
                sql = @"INSERT INTO parts (part_name, part_number, description, category_id, supplier_id, purchase_price, selling_price, quantity_in_stock, minimum_stock_level, reorder_quantity, location, shelf, part_image, barcode, status, date_added,
                                          item_type, unit_of_measure, batch_number, expiry_date, is_sales_item, is_purchase_item, is_inactive, tax_rate, is_stock_tracked, sell_by_weight, price2, price3, price4) 
                        VALUES (@name, @num, @desc, @cat, @sup, @cost, @price1, @stock, @min, @reorder, @loc, @shelf, @img, @barcode, @status, datetime('now'),
                                @type, @uom, @batch, @expiry, @sales, @purchase, @inactive, @tax, @tracked, @sellByWeight, @price2, @price3, @price4)";
            }
            else
            {
                sql = @"UPDATE parts SET part_name=@name, part_number=@num, description=@desc, category_id=@cat, supplier_id=@sup, purchase_price=@cost, selling_price=@price1, 
                                         quantity_in_stock=@stock, minimum_stock_level=@min, reorder_quantity=@reorder, location=@loc, shelf=@shelf, barcode=@barcode, status=@status,
                                         item_type=@type, unit_of_measure=@uom, batch_number=@batch, expiry_date=@expiry, is_sales_item=@sales, is_purchase_item=@purchase, 
                                         is_inactive=@inactive, tax_rate=@tax, is_stock_tracked=@tracked, sell_by_weight=@sellByWeight, price2=@price2, price3=@price3, price4=@price4";
                if (p.PartImage != null) sql += ", part_image=@img";
                sql += " WHERE id=@id";
            }

            var parms = new System.Collections.Generic.List<SqliteParameter>
            {
                new SqliteParameter("@name",     p.PartName),
                new SqliteParameter("@num",      p.PartNumber ?? ""),
                new SqliteParameter("@desc",     p.Description ?? ""),
                new SqliteParameter("@cat",      categoryId),
                new SqliteParameter("@sup",      p.SupplierId.HasValue ? (object)p.SupplierId.Value : DBNull.Value),
                new SqliteParameter("@cost",     p.PurchasePrice),
                new SqliteParameter("@price1",   p.SellingPrice),
                new SqliteParameter("@stock",    p.QuantityInStock),
                new SqliteParameter("@min",      p.MinimumStockLevel),
                new SqliteParameter("@reorder",  p.ReorderQuantity),
                new SqliteParameter("@loc",      p.Location ?? ""),
                new SqliteParameter("@shelf",    p.Shelf ?? ""),
                new SqliteParameter("@barcode",  p.Barcode ?? ""),
                new SqliteParameter("@status",   p.Status ?? "Active"),
                new SqliteParameter("@type",     p.ItemType ?? "Product"),
                new SqliteParameter("@uom",      p.UnitOfMeasure ?? ""),
                new SqliteParameter("@batch",    p.BatchNumber ?? ""),
                new SqliteParameter("@expiry",   p.ExpiryDate ?? ""),
                new SqliteParameter("@sales",    p.IsSalesItem ? 1 : 0),
                new SqliteParameter("@purchase", p.IsPurchaseItem ? 1 : 0),
                new SqliteParameter("@inactive", p.IsInactive ? 1 : 0),
                new SqliteParameter("@tax",      p.TaxRate),
                new SqliteParameter("@tracked",  p.IsStockTracked ? 1 : 0),
                new SqliteParameter("@sellByWeight", p.SellByWeight ? 1 : 0),
                new SqliteParameter("@price2",   p.Price2),
                new SqliteParameter("@price3",   p.Price3),
                new SqliteParameter("@price4",   p.Price4)
            };
            if (isNew || p.PartImage != null) parms.Add(new SqliteParameter("@img", p.PartImage ?? (object)DBNull.Value));
            if (!isNew) parms.Add(new SqliteParameter("@id", p.Id));

            if (!DatabaseHelper.ExecuteNonQuery(sql, parms.ToArray()))
                throw new Exception("Failed to save product/service. Database operation failed.");

            LogTransaction(isNew ? "ADD" : "EDIT", $"{(isNew ? "Added" : "Updated")} {p.ItemType}: {p.PartName} ({p.PartNumber})", p.PartName);
            GlobalEvents.RaiseInventoryUpdated();
        }

        private int GetCategoryId(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName)) return 1;
            object result = DatabaseHelper.ExecuteScalar<object>("SELECT id FROM categories WHERE category_name = @name",
                new SqliteParameter("@name", categoryName));
            if (result != null) return Convert.ToInt32(result);

            // Create if not exists, return new id
            DatabaseHelper.ExecuteNonQuery("INSERT INTO categories (category_name, description) VALUES (@name, '')",
                new SqliteParameter("@name", categoryName));
            return (int)DatabaseHelper.ExecuteScalar<long>("SELECT last_insert_rowid()");
        }

        private void LogTransaction(string action, string description, string partName = "N/A")
        {
            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO transactions (action_type, part_name, description, username, timestamp) VALUES (@action, @part, @desc, 'System', datetime('now'))",
                    new SqliteParameter("@action", action),
                    new SqliteParameter("@part",   partName),
                    new SqliteParameter("@desc",   description));
            }
            catch { }
        }

        public void DeletePart(int partId)
        {
            string partName = DatabaseHelper.ExecuteScalar<string>($"SELECT part_name FROM parts WHERE id = {partId}") ?? "N/A";
            DatabaseHelper.ExecuteNonQuery($"UPDATE parts SET date_deleted = datetime('now') WHERE id = {partId}");
            LogTransaction("DELETE", $"Deleted Part: {partName} (ID: {partId})", partName);
            GlobalEvents.RaiseInventoryUpdated();
        }

        public DataTable GetCategories()
        {
            return DatabaseHelper.ExecuteDataTable("SELECT id, category_name FROM categories ORDER BY category_name");
        }

        public bool PartExists(string partNumber)
        {
            return DatabaseHelper.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM parts WHERE part_number = @pn AND date_deleted IS NULL",
                new SqliteParameter("@pn", partNumber)) > 0;
        }

        public bool BarcodeExists(string barcode, int? excludePartId = null)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return false;
            string sql = "SELECT COUNT(*) FROM parts WHERE barcode = @barcode AND date_deleted IS NULL";
            var parameters = new System.Collections.Generic.List<SqliteParameter> { new SqliteParameter("@barcode", barcode) };
            if (excludePartId.HasValue)
            {
                sql += " AND id != @id";
                parameters.Add(new SqliteParameter("@id", excludePartId.Value));
            }
            return DatabaseHelper.ExecuteScalar<int>(sql, parameters.ToArray()) > 0;
        }

        /// <summary>Returns existing barcode, or generates a unique CODE128-friendly code.</summary>
        public string EnsureUniqueBarcode(string barcode, int? excludePartId = null)
        {
            if (!string.IsNullOrWhiteSpace(barcode))
                return barcode.Trim();

            for (int attempt = 0; attempt < 25; attempt++)
            {
                string code = "BC" + DateTime.Now.ToString("yyMMddHHmmss") + Random.Shared.Next(100, 999);
                if (!BarcodeExists(code, excludePartId))
                    return code;
                System.Threading.Thread.Sleep(2);
            }
            return "BC" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant();
        }

        public DataRow GetPartByBarcodeOrNumber(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;
            string sql = "SELECT id, part_name, part_number, barcode, selling_price FROM parts " +
                         "WHERE (barcode = @q OR part_number = @q OR part_name = @q) AND date_deleted IS NULL LIMIT 1";
            DataTable dt = DatabaseHelper.ExecuteDataTable(sql, new SqliteParameter("@q", query));
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public void ImportPart(string partNumber, string partName, string categoryName, int quantity, int minStock, decimal unitPrice, string location, string status)
        {
            try
            {
                int categoryId = GetCategoryId(categoryName);
                string sql = "INSERT INTO parts (part_number, part_name, category_id, quantity_in_stock, minimum_stock_level, selling_price, location, status, date_added) " +
                             "VALUES (@pn, @name, @cat, @qty, @min, @price, @loc, @status, datetime('now'))";
                DatabaseHelper.ExecuteNonQuery(sql,
                    new SqliteParameter("@pn",     partNumber),
                    new SqliteParameter("@name",   partName),
                    new SqliteParameter("@cat",    categoryId),
                    new SqliteParameter("@qty",    quantity),
                    new SqliteParameter("@min",    minStock),
                    new SqliteParameter("@price",  unitPrice),
                    new SqliteParameter("@loc",    location ?? ""),
                    new SqliteParameter("@status", status ?? "Active"));
                GlobalEvents.RaiseInventoryUpdated();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "InventoryService.ImportPart");
                throw;
            }
        }

        public void AdjustStock(int partId, int change, string reason)
        {
            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE parts SET quantity_in_stock = quantity_in_stock + @change WHERE id = @id",
                    new SqliteParameter("@change", change),
                    new SqliteParameter("@id",     partId));

                string partNameResult = DatabaseHelper.ExecuteScalar<string>($"SELECT part_name FROM parts WHERE id = {partId}") ?? "Unknown";
                string action = change > 0 ? "ADJUST_IN" : "ADJUST_OUT";
                LogTransaction(action, $"Adjusted stock of {partNameResult} by {change}. Reason: {reason}", partNameResult);
                GlobalEvents.RaiseInventoryUpdated();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "InventoryService.AdjustStock");
                throw;
            }
        }
    }

    // - Customer Service -

    public class CustomerService
    {
        public DataTable GetAllCustomers(string search = "")
        {
            string sql = "SELECT customer_id as ID, full_name as Name, phone as Phone, email as Email, address as Address, " +
                         "current_balance as 'Balance Due', credit_limit, payment_due_date, reminder_days " +
                         "FROM customers WHERE date_deleted IS NULL";
            if (!string.IsNullOrEmpty(search))
                sql += $" AND (full_name LIKE '%{search}%' OR phone LIKE '%{search}%' OR email LIKE '%{search}%')";
            sql += " ORDER BY current_balance DESC";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public int AddCustomer(string name, string phone, string email, string address, string type, decimal creditLimit = 1000, DateTime? dueDate = null, int reminderDays = 0)
        {
            string sql = "INSERT INTO customers (full_name, phone, email, address, current_balance, type, credit_limit, payment_due_date, reminder_days) " +
                         "VALUES (@name, @phone, @email, @addr, 0, @type, @credit, @due, @rem); SELECT last_insert_rowid();";
            long id = DatabaseHelper.ExecuteScalar<long>(sql,
                new SqliteParameter("@name",   name),
                new SqliteParameter("@phone",  phone),
                new SqliteParameter("@email",  email),
                new SqliteParameter("@addr",   address),
                new SqliteParameter("@type",   type),
                new SqliteParameter("@credit", creditLimit),
                new SqliteParameter("@due",    dueDate.HasValue ? (object)dueDate.Value.ToString("s") : DBNull.Value),
                new SqliteParameter("@rem",    reminderDays));
            LogTransaction("CUSTOMER_ADD", $"Added Customer: {name} (Limit: {creditLimit})", name);
            GlobalEvents.RaiseCustomersUpdated();
            return (int)id;
        }

        public void UpdateBalance(int customerId, decimal amount)
        {
            DatabaseHelper.ExecuteNonQuery(
                $"UPDATE customers SET current_balance = current_balance + {amount} WHERE customer_id = {customerId}");
        }

        public void UpdateCustomer(int id, string name, string phone, string email, string address, string type, decimal creditLimit, DateTime? dueDate, int reminderDays)
        {
            string sql = "UPDATE customers SET full_name=@name, phone=@phone, email=@email, address=@addr, type=@type, " +
                         "credit_limit=@credit, payment_due_date=@due, reminder_days=@rem WHERE customer_id=@id";
            DatabaseHelper.ExecuteNonQuery(sql,
                new SqliteParameter("@name",   name),
                new SqliteParameter("@phone",  phone),
                new SqliteParameter("@email",  email),
                new SqliteParameter("@addr",   address),
                new SqliteParameter("@type",   type),
                new SqliteParameter("@credit", creditLimit),
                new SqliteParameter("@due",    dueDate.HasValue ? (object)dueDate.Value.ToString("s") : DBNull.Value),
                new SqliteParameter("@rem",    reminderDays),
                new SqliteParameter("@id",     id));
            LogTransaction("CUSTOMER_UPDATE", $"Updated Customer: {name} (ID: {id})", name);
            GlobalEvents.RaiseCustomersUpdated();
        }

        public void DeleteCustomer(int id)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE customers SET date_deleted = datetime('now') WHERE customer_id = @id",
                new SqliteParameter("@id", id));
            LogTransaction("CUSTOMER_DELETE", $"Deleted Customer ID: {id}", "N/A");
            GlobalEvents.RaiseCustomersUpdated();
        }

        public CustomerStats GetStats()
        {
            var stats = new CustomerStats();
            try
            {
                stats.TotalCustomers = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM customers");
                stats.TotalDebt      = DatabaseHelper.ExecuteScalar<decimal>("SELECT COALESCE(SUM(current_balance), 0) FROM customers");
            }
            catch { }
            return stats;
        }

        public bool CustomerExists(string email)
        {
            return DatabaseHelper.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM customers WHERE email = @email AND date_deleted IS NULL",
                new SqliteParameter("@email", email)) > 0;
        }

        public void ImportCustomer(string customerName, string email, string phone, string address, string city, string postalCode, string notes)
        {
            try
            {
                string fullAddress = (address ?? "") +
                    (string.IsNullOrEmpty(city)       ? "" : ", " + city) +
                    (string.IsNullOrEmpty(postalCode) ? "" : " " + postalCode);
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO customers (full_name, email, phone, address, current_balance, type, date_added) " +
                    "VALUES (@name, @email, @phone, @address, 0, 'Regular', datetime('now'))",
                    new SqliteParameter("@name",    customerName),
                    new SqliteParameter("@email",   email ?? ""),
                    new SqliteParameter("@phone",   phone ?? ""),
                    new SqliteParameter("@address", fullAddress));
                GlobalEvents.RaiseCustomersUpdated();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "CustomerService.ImportCustomer");
                throw;
            }
        }

        private void LogTransaction(string action, string description, string entityName = "N/A")
        {
            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO transactions (action_type, part_name, description, username, timestamp) VALUES (@a, @e, @d, 'System', datetime('now'))",
                    new SqliteParameter("@a", action),
                    new SqliteParameter("@e", entityName),
                    new SqliteParameter("@d", description));
            }
            catch { }
        }
    }

    public class CustomerStats
    {
        public int TotalCustomers { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal TotalPaid { get; set; }
        public int ActiveCustomers { get; set; }
    }

    // - Supplier Service -

    public class SupplierService
    {
        public DataTable GetAllSuppliers(string search = "")
        {
            string sql = "SELECT id as ID, supplier_name, phone, email, address, type, balance_due, payment_due_date, reminder_days " +
                         "FROM suppliers WHERE date_deleted IS NULL";
            if (!string.IsNullOrEmpty(search))
                sql += $" AND (supplier_name LIKE '%{search}%' OR phone LIKE '%{search}%' OR email LIKE '%{search}%')";
            sql += " ORDER BY supplier_name";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public bool SupplierExists(string supplierName)
        {
            return DatabaseHelper.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM suppliers WHERE supplier_name = @name AND date_deleted IS NULL",
                new SqliteParameter("@name", supplierName)) > 0;
        }

        public void ImportSupplier(string supplierName, string contactPerson, string email, string phone, string address, string city, string postalCode, string website, string notes)
        {
            try
            {
                string full = (address ?? "") +
                    (string.IsNullOrEmpty(city)       ? "" : ", " + city) +
                    (string.IsNullOrEmpty(postalCode) ? "" : " " + postalCode);
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO suppliers (supplier_name, phone, email, address, balance_due, date_added) " +
                    "VALUES (@name, @phone, @email, @address, 0, datetime('now'))",
                    new SqliteParameter("@name",    supplierName),
                    new SqliteParameter("@phone",   phone ?? ""),
                    new SqliteParameter("@email",   email ?? ""),
                    new SqliteParameter("@address", full));
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "SupplierService.ImportSupplier");
                throw;
            }
        }

        public void AddSupplier(string name, string phone, string email, string address, string type, DateTime? dueDate = null, int reminderDays = 0)
        {
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO suppliers (supplier_name, phone, email, address, type, balance_due, payment_due_date, reminder_days) " +
                "VALUES (@name, @phone, @email, @addr, @type, 0, @due, @rem)",
                new SqliteParameter("@name",  name),
                new SqliteParameter("@phone", phone),
                new SqliteParameter("@email", email),
                new SqliteParameter("@addr",  address),
                new SqliteParameter("@type",  type),
                new SqliteParameter("@due",   dueDate.HasValue ? (object)dueDate.Value.ToString("s") : DBNull.Value),
                new SqliteParameter("@rem",   reminderDays));
            GlobalEvents.RaiseSuppliersUpdated();
        }

        public void UpdateSupplier(int id, string name, string phone, string email, string address, string type, DateTime? dueDate, int reminderDays)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE suppliers SET supplier_name=@name, phone=@phone, email=@email, address=@addr, type=@type, payment_due_date=@due, reminder_days=@rem WHERE id=@id",
                new SqliteParameter("@name",  name),
                new SqliteParameter("@phone", phone),
                new SqliteParameter("@email", email),
                new SqliteParameter("@addr",  address),
                new SqliteParameter("@type",  type),
                new SqliteParameter("@due",   dueDate.HasValue ? (object)dueDate.Value.ToString("s") : DBNull.Value),
                new SqliteParameter("@rem",   reminderDays),
                new SqliteParameter("@id",    id));
            GlobalEvents.RaiseSuppliersUpdated();
        }

        public void DeleteSupplier(int id)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE suppliers SET date_deleted = datetime('now') WHERE id = @id",
                new SqliteParameter("@id", id));
            GlobalEvents.RaiseSuppliersUpdated();
        }
    }
}
