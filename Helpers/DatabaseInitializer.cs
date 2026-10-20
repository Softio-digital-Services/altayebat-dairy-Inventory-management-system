using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace InventorySystem.Helpers
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            // SQLite creates the file automatically -- no CreateDatabase() needed
            DatabaseHelper.EnsureSchema();
            UpdateSchema();
            // SeedIfEmpty(); // Disabled to provide a fresh start for clients
        }

        /// <summary>
        /// Idempotent patches applied on every startup.
        /// Safe to re-run -- all guards use IF NOT EXISTS / INSERT OR IGNORE.
        /// </summary>
        private static void UpdateSchema()
        {
            // Ensure category_image column exists for existing databases
            DatabaseHelper.ExecuteNonQuery("ALTER TABLE categories ADD COLUMN category_image TEXT;");

            // Ensure Softio Super Admin always exists
            EnsureSoftioSuperAdmin();

            FeatureFlags.EnsureTable();

            // Repair: Standardise status values (fix Arabic UI bug)
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE parts SET status = 'Active' WHERE status NOT IN ('Active', 'Inactive') AND date_deleted IS NULL;"
            );
        }

        /// <summary>
        /// Ensures the built-in Softio.Admin account exists. Safe to call after
        /// factory reset or any schema rebuild.
        /// </summary>
        public static void EnsureSoftioSuperAdmin()
        {
            DatabaseHelper.ExecuteNonQuery(
                "INSERT OR IGNORE INTO users (username, password, full_name, role) VALUES ('Softio.Admin', 'Softio@2026!', 'Softio Super Admin', 'Admin');"
            );
            // If the row somehow exists with a blank password, restore defaults (never wipe a custom password if set).
            DatabaseHelper.ExecuteNonQuery(
                @"UPDATE users SET
                    full_name = COALESCE(NULLIF(TRIM(full_name), ''), 'Softio Super Admin'),
                    role = CASE WHEN role IS NULL OR TRIM(role) = '' THEN 'Admin' ELSE role END,
                    password = CASE WHEN password IS NULL OR TRIM(password) = '' THEN 'Softio@2026!' ELSE password END,
                    is_active = 1
                  WHERE LOWER(username) = 'softio.admin';"
            );
        }

        /// <summary>
        /// True for the built-in Softio Super Admin (must never be deleted).
        /// </summary>
        public static bool IsProtectedSuperAdmin(string username)
        {
            return !string.IsNullOrWhiteSpace(username)
                && username.Trim().Equals("Softio.Admin", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Seeds demo data only when the database is brand new (no suppliers yet).
        /// </summary>
        private static void SeedIfEmpty()
        {
            int count = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM suppliers;");
            if (count > 0) return; // Already seeded

            // Suppliers
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO suppliers (supplier_name, phone, email, address, type, supplier_code)
                VALUES
                  ('Global Auto Parts', '555-0101', 'contact@globalautoparts.com', '123 Industrial Way', 'Wholesaler', 'SUP-001'),
                  ('Brake Systems Inc',  '555-0102', 'sales@brakesystems.com',     '456 Safety Blvd',    'Specialist',  'SUP-002');
            ");

            // Customers
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO customers (full_name, phone, email, address, type, current_balance)
                VALUES
                  ('John Smith',    '555-0201', 'john.smith@email.com',       '789 Maple Ave',   'Retail',    0),
                  ('Speedy Garage', '555-0202', 'manager@speedygarage.com', '321 Mechanic Ln', 'Corporate', 150.00);
            ");

            // Demo parts (category_id 1 = Engine, 2 = Brakes)
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO parts (part_name, part_number, description, category_id, supplier_id, purchase_price, selling_price, quantity_in_stock, minimum_stock_level, location, status)
                VALUES
                  ('Oil Filter',     'OIL-001', 'Standard oil filter',   1, 1,  5.00, 12.00, 50, 10, 'Shelf A', 'Active'),
                  ('Brake Pads',     'BRK-002', 'Front brake pads set',  2, 2, 20.00, 45.00, 30,  8, 'Shelf B', 'Active'),
                  ('Air Filter',     'AIR-003', 'Engine air filter',     1, 1,  8.00, 18.00, 40, 10, 'Shelf A', 'Active'),
                  ('Spark Plug',     'SPK-004', 'NGK spark plug',        1, 1,  3.00,  8.00,100, 20, 'Shelf C', 'Active'),
                  ('Timing Belt',    'TIM-005', 'Heavy duty timing belt',1, 1, 25.00, 60.00, 15,  5, 'Shelf B', 'Active');
            ");

            // Demo orders
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO orders (order_date, customer_id, total_amount, status, payment_status, payment_method)
                VALUES
                  (datetime('now','-1 day'), 1, 150.00, 'Completed', 'Paid', 'Cash'),
                  (datetime('now'),          2, 250.00, 'Completed', 'Paid', 'Card'),
                  (datetime('now'),          1,  85.00, 'Completed', 'Paid', 'Cash');
            ");

            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO order_items (order_id, part_id, quantity, price)
                VALUES (1,1,5,12.00),(1,2,2,45.00),(2,3,10,18.00),(2,4,2,8.00),(3,1,3,12.00),(3,5,1,60.00);
            ");
        }
    }
}
