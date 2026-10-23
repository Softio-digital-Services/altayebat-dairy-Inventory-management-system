using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using InventorySystem.Helpers;

namespace InventorySystem
{
    /// <summary>
    /// Centralized database operations helper -- SQLite backend.
    /// Drop-in replacement for the previous SQL Server version.
    /// </summary>
    public static class DatabaseHelper
    {
        // - helpers -

        private static SqliteConnection OpenConnection()
        {
            var conn = new SqliteConnection(DatabaseConfig.ConnectionString);
            conn.Open();
            return conn;
        }

        private static void AddParams(SqliteCommand cmd, SqliteParameter[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
        }

        // - public API -

        public static List<T> ExecuteQuery<T>(string sql, Func<SqliteDataReader, T> map, params SqliteParameter[] parameters)
        {
            var results = new List<T>();
            try
            {
                using var conn = OpenConnection();
                using var cmd  = new SqliteCommand(sql, conn);
                AddParams(cmd, parameters);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    results.Add(map(reader));
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"ExecuteQuery: {sql}");
                throw;
            }
            return results;
        }

        public static bool ExecuteNonQuery(string sql, params SqliteParameter[] parameters)
        {
            try
            {
                using var conn = OpenConnection();
                using var cmd  = new SqliteCommand(sql, conn);
                AddParams(cmd, parameters);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"ExecuteNonQuery: {sql}");
                return false;
            }
        }

        public static T ExecuteScalar<T>(string sql, params SqliteParameter[] parameters)
        {
            try
            {
                using var conn = OpenConnection();
                using var cmd  = new SqliteCommand(sql, conn);
                AddParams(cmd, parameters);
                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                    return default(T);
                return (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"ExecuteScalar: {sql}");
                return default(T);
            }
        }

        public static bool RecordExists(string tableName, string columnName, string value, bool excludeDeleted = true)
        {
            string del = excludeDeleted ? " AND date_deleted IS NULL" : "";
            string sql = $"SELECT COUNT(*) FROM {tableName} WHERE {columnName} = @value{del}";
            return ExecuteScalar<int>(sql, new SqliteParameter("@value", value)) > 0;
        }

        public static int GetCount(string tableName, string whereClause = "", params SqliteParameter[] parameters)
        {
            string sql = $"SELECT COUNT(*) FROM {tableName}";
            if (!string.IsNullOrWhiteSpace(whereClause))
                sql += $" WHERE {whereClause}";
            return ExecuteScalar<int>(sql, parameters);
        }

        /// <summary>
        /// Fills a DataTable from a query -- replaces SqlDataAdapter for SQLite.
        /// </summary>
        public static DataTable ExecuteDataTable(string sql, params SqliteParameter[] parameters)
        {
            var dt = new DataTable();
            try
            {
                using var conn   = OpenConnection();
                using var cmd    = new SqliteCommand(sql, conn);
                AddParams(cmd, parameters);
                using var reader = cmd.ExecuteReader();
                
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Type t = reader.GetFieldType(i) ?? typeof(object);
                    dt.Columns.Add(reader.GetName(i), t);
                }

                while (reader.Read())
                {
                    DataRow row = dt.NewRow();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        object val = reader.GetValue(i);
                        if (val == DBNull.Value)
                        {
                            row[i] = DBNull.Value;
                        }
                        else
                        {
                            Type targetType = dt.Columns[i].DataType;
                            if (val.GetType() == targetType || targetType == typeof(object))
                            {
                                row[i] = val;
                            }
                            else
                            {
                                try
                                {
                                    if (val is string s && string.IsNullOrWhiteSpace(s))
                                    {
                                        row[i] = targetType.IsValueType ? Activator.CreateInstance(targetType) : DBNull.Value;
                                    }
                                    else
                                    {
                                        row[i] = Convert.ChangeType(val, targetType);
                                    }
                                }
                                catch
                                {
                                    row[i] = targetType.IsValueType ? Activator.CreateInstance(targetType) : DBNull.Value;
                                }
                            }
                        }
                    }
                    dt.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"ExecuteDataTable: {sql}");
                throw;
            }
            return dt;
        }

        public static void LogTransaction(string action, string partName, string description)
        {
            try
            {
                string sql = "INSERT INTO transactions (action_type, part_name, description, username, timestamp) " +
                             "VALUES (@action, @part, @desc, @user, datetime('now'))";
                ExecuteNonQuery(sql,
                    new SqliteParameter("@action", action),
                    new SqliteParameter("@part",   partName),
                    new SqliteParameter("@desc",   description),
                    new SqliteParameter("@user",   "Admin"));
            }
            catch { }
        }

        /// <summary>
        /// Ensures all required tables and columns exist (SQLite-compatible).
        /// Uses CREATE TABLE IF NOT EXISTS + PRAGMA table_info for column checks.
        /// </summary>
        public static void EnsureSchema()
        {
            try
            {
                // Tables -- SQLite CREATE IF NOT EXISTS is idempotent
                string sqlTables = @"
                    CREATE TABLE IF NOT EXISTS categories (
                        id              INTEGER PRIMARY KEY AUTOINCREMENT,
                        category_name   TEXT NOT NULL UNIQUE,
                        description     TEXT,
                        category_image  TEXT,
                        date_created    TEXT DEFAULT (datetime('now'))
                    );

                    CREATE TABLE IF NOT EXISTS suppliers (
                        id              INTEGER PRIMARY KEY AUTOINCREMENT,
                        supplier_code   TEXT,
                        supplier_name   TEXT NOT NULL,
                        contact_person  TEXT,
                        email           TEXT,
                        phone           TEXT,
                        address         TEXT,
                        type            TEXT,
                        balance_due     REAL DEFAULT 0,
                        payment_due_date TEXT,
                        reminder_days   INTEGER DEFAULT 0,
                        status          TEXT DEFAULT 'Active',
                        date_added      TEXT DEFAULT (datetime('now')),
                        date_deleted    TEXT
                    );

                    CREATE TABLE IF NOT EXISTS customers (
                        customer_id     INTEGER PRIMARY KEY AUTOINCREMENT,
                        full_name       TEXT NOT NULL,
                        phone           TEXT,
                        email           TEXT,
                        address         TEXT,
                        type            TEXT,
                        current_balance REAL DEFAULT 0,
                        credit_limit    REAL DEFAULT 1000,
                        payment_due_date TEXT,
                        reminder_days   INTEGER DEFAULT 0,
                        status          TEXT DEFAULT 'Active',
                        date_added      TEXT DEFAULT (datetime('now')),
                        date_deleted    TEXT
                    );

                    CREATE TABLE IF NOT EXISTS parts (
                        id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                        part_number         TEXT,
                        part_name           TEXT NOT NULL,
                        description         TEXT,
                        category_id         INTEGER,
                        supplier_id         INTEGER,
                        purchase_price      REAL DEFAULT 0,
                        selling_price       REAL DEFAULT 0,
                        quantity_in_stock   INTEGER DEFAULT 0,
                        minimum_stock_level INTEGER DEFAULT 5,
                        reorder_quantity    INTEGER DEFAULT 10,
                        location            TEXT,
                        shelf               TEXT,
                        part_image          TEXT,
                        barcode             TEXT,
                        status              TEXT DEFAULT 'Active',
                        date_added          TEXT DEFAULT (datetime('now')),
                        date_deleted        TEXT,
                        item_type           TEXT DEFAULT 'Product',
                        unit_of_measure     TEXT,
                        batch_number        TEXT,
                        expiry_date         TEXT,
                        is_sales_item       INTEGER DEFAULT 1,
                        is_purchase_item    INTEGER DEFAULT 0,
                        is_inactive         INTEGER DEFAULT 0,
                        tax_rate            REAL DEFAULT 0,
                        is_stock_tracked    INTEGER DEFAULT 1,
                        sell_by_weight      INTEGER DEFAULT 0,
                        price2              REAL DEFAULT 0,
                        price3              REAL DEFAULT 0,
                        price4              REAL DEFAULT 0
                    );

                    CREATE TABLE IF NOT EXISTS transactions (
                        id          INTEGER PRIMARY KEY AUTOINCREMENT,
                        action_type TEXT,
                        part_name   TEXT,
                        description TEXT,
                        username    TEXT,
                        timestamp   TEXT DEFAULT (datetime('now'))
                    );

                    CREATE TABLE IF NOT EXISTS users (
                        id           INTEGER PRIMARY KEY AUTOINCREMENT,
                        username     TEXT NOT NULL UNIQUE,
                        password     TEXT NOT NULL,
                        full_name    TEXT,
                        role         TEXT DEFAULT 'User',
                        is_active    INTEGER DEFAULT 1,
                        date_created TEXT DEFAULT (datetime('now'))
                    );

                    CREATE TABLE IF NOT EXISTS orders (
                        order_id        INTEGER PRIMARY KEY AUTOINCREMENT,
                        customer_id     INTEGER,
                        order_date      TEXT DEFAULT (datetime('now')),
                        total_amount    REAL DEFAULT 0,
                        status          TEXT DEFAULT 'Completed',
                        payment_status  TEXT DEFAULT 'Paid',
                        amount_paid     REAL DEFAULT 0,
                        payment_method  TEXT DEFAULT 'Cash'
                    );

                    CREATE TABLE IF NOT EXISTS order_items (
                        order_item_id   INTEGER PRIMARY KEY AUTOINCREMENT,
                        order_id        INTEGER,
                        part_id         INTEGER,
                        quantity        INTEGER,
                        price           REAL
                    );

                    CREATE TABLE IF NOT EXISTS payments (
                        payment_id      INTEGER PRIMARY KEY AUTOINCREMENT,
                        entity_type     TEXT,
                        entity_id       INTEGER,
                        amount          REAL,
                        payment_date    TEXT DEFAULT (datetime('now')),
                        notes           TEXT,
                        due_date        TEXT
                    );

                    CREATE TABLE IF NOT EXISTS purchase_orders (
                        po_id           INTEGER PRIMARY KEY AUTOINCREMENT,
                        supplier_id     INTEGER,
                        order_date      TEXT DEFAULT (datetime('now')),
                        total_amount    REAL DEFAULT 0,
                        status          TEXT DEFAULT 'Pending',
                        received_date   TEXT,
                        notes           TEXT
                    );

                    CREATE TABLE IF NOT EXISTS purchase_order_items (
                        po_item_id      INTEGER PRIMARY KEY AUTOINCREMENT,
                        po_id           INTEGER,
                        part_id         INTEGER,
                        quantity        INTEGER,
                        cost_price      REAL
                    );

                    CREATE TABLE IF NOT EXISTS returns (
                        return_id       INTEGER PRIMARY KEY AUTOINCREMENT,
                        order_id        INTEGER,
                        return_date     TEXT DEFAULT (datetime('now')),
                        total_refund    REAL DEFAULT 0,
                        reason          TEXT,
                        performed_by    TEXT
                    );

                    CREATE TABLE IF NOT EXISTS return_items (
                        return_item_id  INTEGER PRIMARY KEY AUTOINCREMENT,
                        return_id       INTEGER,
                        part_id         INTEGER,
                        quantity        INTEGER,
                        refund_amount   REAL
                    );

                    CREATE TABLE IF NOT EXISTS expenses (
                        expense_id          INTEGER PRIMARY KEY AUTOINCREMENT,
                        expense_date        TEXT DEFAULT (datetime('now')),
                        category            TEXT,
                        amount              REAL DEFAULT 0,
                        description         TEXT,
                        recorded_by         TEXT,
                        is_paid             INTEGER DEFAULT 1,
                        is_recurring        INTEGER DEFAULT 0,
                        last_processed_month TEXT,
                        date_deleted        TEXT
                    );

                    CREATE TABLE IF NOT EXISTS expense_categories (
                        category_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        category_name TEXT NOT NULL UNIQUE
                    );

                    CREATE TABLE IF NOT EXISTS units_of_measure (
                        unit_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        unit_name TEXT NOT NULL UNIQUE
                    );

                    CREATE TABLE IF NOT EXISTS app_settings (
                        key TEXT PRIMARY KEY NOT NULL,
                        value TEXT NOT NULL
                    );
                ";

                // SQLite doesn't support multiple statements in one call -- split them
                foreach (var stmt in sqlTables.Split(new[] { ";\r\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = stmt.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                        ExecuteNonQuery(trimmed + ";");
                }

                // --- MIGRATIONS ---
                // Add new unified product/service fields to parts
                if (!ColumnExists("parts", "item_type")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN item_type TEXT DEFAULT 'Product';");
                if (!ColumnExists("parts", "unit_of_measure")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN unit_of_measure TEXT;");
                if (!ColumnExists("parts", "batch_number")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN batch_number TEXT;");
                if (!ColumnExists("parts", "expiry_date")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN expiry_date TEXT;");
                if (!ColumnExists("parts", "is_sales_item")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN is_sales_item INTEGER DEFAULT 1;");
                if (!ColumnExists("parts", "is_purchase_item")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN is_purchase_item INTEGER DEFAULT 0;");
                if (!ColumnExists("parts", "is_inactive")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN is_inactive INTEGER DEFAULT 0;");
                if (!ColumnExists("parts", "tax_rate")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN tax_rate REAL DEFAULT 0;");
                if (!ColumnExists("parts", "is_stock_tracked")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN is_stock_tracked INTEGER DEFAULT 1;");
                if (!ColumnExists("parts", "sell_by_weight")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN sell_by_weight INTEGER DEFAULT 0;");
                if (!ColumnExists("parts", "price2")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN price2 REAL DEFAULT 0;");
                if (!ColumnExists("parts", "price3")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN price3 REAL DEFAULT 0;");
                if (!ColumnExists("parts", "price4")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN price4 REAL DEFAULT 0;");

                // Add due_date to payments if missing
                if (!ColumnExists("payments", "due_date"))
                {
                    ExecuteNonQuery("ALTER TABLE payments ADD COLUMN due_date TEXT;");
                }

                // Add shipping fields to orders
                if (!ColumnExists("orders", "shipping_address")) ExecuteNonQuery("ALTER TABLE orders ADD COLUMN shipping_address TEXT;");
                if (!ColumnExists("orders", "delivery_date")) ExecuteNonQuery("ALTER TABLE orders ADD COLUMN delivery_date TEXT;");
                if (!ColumnExists("orders", "due_date")) ExecuteNonQuery("ALTER TABLE orders ADD COLUMN due_date TEXT;");

                // Custom / quick-sale line label (when part_id is null or name overridden)
                if (!ColumnExists("order_items", "item_name")) ExecuteNonQuery("ALTER TABLE order_items ADD COLUMN item_name TEXT;");
                if (!ColumnExists("order_items", "amount_paid")) ExecuteNonQuery("ALTER TABLE order_items ADD COLUMN amount_paid REAL DEFAULT 0;");

                ExecuteNonQuery(@"
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
                if (!ColumnExists("supplier_purchase_items", "category"))
                    ExecuteNonQuery("ALTER TABLE supplier_purchase_items ADD COLUMN category TEXT;");

                // Milk intake tracking in expenses
                if (!ColumnExists("expenses", "supplier_id")) ExecuteNonQuery("ALTER TABLE expenses ADD COLUMN supplier_id INTEGER;");
                if (!ColumnExists("expenses", "liters")) ExecuteNonQuery("ALTER TABLE expenses ADD COLUMN liters REAL DEFAULT 0;");
                if (!ColumnExists("expenses", "unit_price")) ExecuteNonQuery("ALTER TABLE expenses ADD COLUMN unit_price REAL DEFAULT 0;");
                if (!ColumnExists("expenses", "milk_type")) ExecuteNonQuery("ALTER TABLE expenses ADD COLUMN milk_type TEXT;");
                if (!ColumnExists("expenses", "shift")) ExecuteNonQuery("ALTER TABLE expenses ADD COLUMN shift TEXT;");

                // What each milk intake was turned into (cheese, labneh, laban...)
                ExecuteNonQuery(@"
                    CREATE TABLE IF NOT EXISTS milk_production (
                        production_id   INTEGER PRIMARY KEY AUTOINCREMENT,
                        expense_id      INTEGER NOT NULL,
                        part_id         INTEGER NOT NULL,
                        liters_used     REAL NOT NULL,
                        quantity_made   REAL NOT NULL,
                        notes           TEXT,
                        production_date TEXT DEFAULT (datetime('now', 'localtime')),
                        recorded_by     TEXT,
                        date_deleted    TEXT
                    );");

                // Seed default dairy categories if empty
                int catCount = ExecuteScalar<int>("SELECT COUNT(*) FROM expense_categories");
                if (catCount == 0)
                {
                    string[] defaultCats = { "Raw Milk", "Electricity", "Rent", "Fuel & Transport", "Packaging", "Maintenance", "Salaries", "General" };
                    foreach (var c in defaultCats)
                    {
                        ExecuteNonQuery("INSERT OR IGNORE INTO expense_categories (category_name) VALUES (@c)",
                            new SqliteParameter("@c", c));
                    }
                }
                else
                {
                    // Ensure Raw Milk category is definitely present
                    ExecuteNonQuery("INSERT OR IGNORE INTO expense_categories (category_name) VALUES ('Raw Milk')");
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "EnsureSchema failed");
            }
        }

        /// <summary>
        /// Insert demo categories, suppliers, products, customers, and supplier purchase lines for testing.
        /// Skips when sentinel supplier already exists unless force=true.
        /// </summary>
        public static object SeedDemoData(bool force = false)
        {
            EnsureSchema();
            int exists = ExecuteScalar<int>(
                "SELECT COUNT(*) FROM suppliers WHERE supplier_name = @n AND date_deleted IS NULL",
                new SqliteParameter("@n", "Demo Auto Supply"));
            if (exists > 0 && !force)
                return new { seeded = false, reason = "Demo data already present" };

            void EnsureCategory(string name)
            {
                int c = ExecuteScalar<int>("SELECT COUNT(*) FROM categories WHERE category_name = @n",
                    new SqliteParameter("@n", name));
                if (c == 0)
                    ExecuteNonQuery("INSERT INTO categories (category_name, description) VALUES (@n, 'Demo')",
                        new SqliteParameter("@n", name));
            }

            EnsureCategory("Oils");
            EnsureCategory("Filters");
            EnsureCategory("Brakes");
            EnsureCategory("Electrical");
            EnsureCategory("General");

            int EnsureSupplier(string name, string phone, string contact)
            {
                object idObj = ExecuteScalar<object>(
                    "SELECT id FROM suppliers WHERE supplier_name = @n AND date_deleted IS NULL",
                    new SqliteParameter("@n", name));
                if (idObj != null && idObj != DBNull.Value)
                    return Convert.ToInt32(idObj);
                ExecuteNonQuery(
                    @"INSERT INTO suppliers (supplier_name, phone, email, address, contact_person, balance_due, type, date_added)
                      VALUES (@n, @p, '', '', @c, 0, 'Company', datetime('now'))",
                    new SqliteParameter("@n", name),
                    new SqliteParameter("@p", phone),
                    new SqliteParameter("@c", contact));
                return (int)ExecuteScalar<long>("SELECT last_insert_rowid()");
            }

            int sid1 = EnsureSupplier("Demo Auto Supply", "03111111", "Ahmad");
            int sid2 = EnsureSupplier("Demo Parts Hub", "03222222", "Sara");

            int EnsureCustomer(string name, string phone, decimal bal)
            {
                object idObj = ExecuteScalar<object>(
                    "SELECT customer_id FROM customers WHERE full_name = @n AND date_deleted IS NULL",
                    new SqliteParameter("@n", name));
                if (idObj != null && idObj != DBNull.Value)
                    return Convert.ToInt32(idObj);
                ExecuteNonQuery(
                    @"INSERT INTO customers (full_name, phone, email, address, type, credit_limit, current_balance, date_added)
                      VALUES (@n, @p, '', '', 'Retail', 2000, @b, datetime('now'))",
                    new SqliteParameter("@n", name),
                    new SqliteParameter("@p", phone),
                    new SqliteParameter("@b", bal));
                return (int)ExecuteScalar<long>("SELECT last_insert_rowid()");
            }

            EnsureCustomer("Demo Walk-in Ali", "03999901", 150);
            EnsureCustomer("Demo Garage Karim", "03999902", 420);

            int CatId(string name)
            {
                object o = ExecuteScalar<object>("SELECT id FROM categories WHERE category_name = @n",
                    new SqliteParameter("@n", name));
                return o == null || o == DBNull.Value ? 1 : Convert.ToInt32(o);
            }

            void EnsurePart(string name, string cat, int supplierId, decimal cost, decimal price, int stock)
            {
                int found = ExecuteScalar<int>(
                    @"SELECT COUNT(*) FROM parts WHERE LOWER(TRIM(part_name)) = LOWER(TRIM(@n))
                        AND supplier_id = @sid AND date_deleted IS NULL",
                    new SqliteParameter("@n", name),
                    new SqliteParameter("@sid", supplierId));
                if (found > 0) return;
                string sku = "DEMO-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
                string barcode = "BC" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpperInvariant();
                ExecuteNonQuery(
                    @"INSERT INTO parts (part_name, part_number, description, category_id, supplier_id,
                         purchase_price, selling_price, quantity_in_stock, minimum_stock_level,
                         status, date_added, item_type, is_sales_item, is_purchase_item, is_stock_tracked, barcode)
                      VALUES (@n, @sku, 'Demo', @cat, @sid, @cost, @price, @stock, 2,
                              'Active', datetime('now'), 'Product', 1, 1, 1, @bc)",
                    new SqliteParameter("@n", name),
                    new SqliteParameter("@sku", sku),
                    new SqliteParameter("@cat", CatId(cat)),
                    new SqliteParameter("@sid", supplierId),
                    new SqliteParameter("@cost", cost),
                    new SqliteParameter("@price", price),
                    new SqliteParameter("@stock", stock),
                    new SqliteParameter("@bc", barcode));
            }

            EnsurePart("Engine Oil 5W30 1L", "Oils", sid1, 4.50m, 7.00m, 24);
            EnsurePart("Oil Filter Standard", "Filters", sid1, 2.00m, 4.50m, 40);
            EnsurePart("Brake Pads Front", "Brakes", sid2, 12.00m, 22.00m, 10);
            EnsurePart("Battery 60Ah", "Electrical", sid2, 55.00m, 85.00m, 5);

            var purchaseSvc = new InventorySystem.Services.SupplierPurchaseService();
            purchaseSvc.EnsureTable();

            // Unadded lines (for import testing) + one debt / one paid
            void AddPurchase(int sid, string name, string cat, decimal qty, decimal price, bool paid)
            {
                int dup = ExecuteScalar<int>(
                    @"SELECT COUNT(*) FROM supplier_purchase_items
                      WHERE supplier_id = @sid AND item_name = @n AND date_deleted IS NULL AND part_id IS NULL",
                    new SqliteParameter("@sid", sid),
                    new SqliteParameter("@n", name));
                if (dup > 0 && !force) return;
                purchaseSvc.AddItem(sid, name, cat, qty, price, paid, "Demo seed");
            }

            AddPurchase(sid1, "Air Filter Cabin", "Filters", 15, 3.25m, false);
            AddPurchase(sid1, "Engine Oil 5W30 1L", "Oils", 12, 4.75m, true); // exists → import should add stock
            AddPurchase(sid2, "Spark Plug Iridium", "Electrical", 20, 6.00m, false);
            AddPurchase(sid2, "Brake Fluid DOT4", "Brakes", 8, 5.50m, false);
            AddPurchase(sid2, "Brake Pads Front", "Brakes", 4, 11.50m, false); // exists → add stock

            return new
            {
                seeded = true,
                suppliers = new[] { "Demo Auto Supply", "Demo Parts Hub" },
                categories = new[] { "Oils", "Filters", "Brakes", "Electrical" },
                note = "Open Suppliers → Add product, or Inventory → From supplier to import lines."
            };
        }

        private static bool ColumnExists(string tableName, string columnName)
        {
            try
            {
                DataTable dt = ExecuteDataTable($"PRAGMA table_info({tableName})");
                foreach (DataRow row in dt.Rows)
                {
                    if (row["name"].ToString().Equals(columnName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            catch { }
            return false;
        }
    }
}
