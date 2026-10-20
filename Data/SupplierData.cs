using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace InventorySystem.Data
{
    public class SupplierData
    {
        public int Id { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public int ReminderDays { get; set; }

        public static List<SupplierData> GetAllSuppliers()
        {
            return DatabaseHelper.ExecuteQuery(
                "SELECT * FROM suppliers WHERE date_deleted IS NULL ORDER BY supplier_name",
                MapFromReader);
        }

        public static List<SupplierData> GetActiveSuppliers()
        {
            return DatabaseHelper.ExecuteQuery(
                "SELECT * FROM suppliers WHERE status = 'Active' AND date_deleted IS NULL ORDER BY supplier_name",
                MapFromReader);
        }

        private static SupplierData MapFromReader(SqliteDataReader r)
        {
            DateTime? due = null;
            try {
                int ord = r.GetOrdinal("payment_due_date");
                if (!r.IsDBNull(ord) && DateTime.TryParse(r.GetString(ord), out DateTime d)) due = d;
            } catch { }

            return new SupplierData
            {
                Id            = r.GetInt32(r.GetOrdinal("id")),
                SupplierCode  = r.IsDBNull(r.GetOrdinal("supplier_code"))  ? "" : r.GetString(r.GetOrdinal("supplier_code")),
                SupplierName  = r.GetString(r.GetOrdinal("supplier_name")),
                ContactPerson = r.IsDBNull(r.GetOrdinal("contact_person")) ? "" : r.GetString(r.GetOrdinal("contact_person")),
                Email         = r.IsDBNull(r.GetOrdinal("email"))          ? "" : r.GetString(r.GetOrdinal("email")),
                Phone         = r.IsDBNull(r.GetOrdinal("phone"))          ? "" : r.GetString(r.GetOrdinal("phone")),
                Address       = r.IsDBNull(r.GetOrdinal("address"))        ? "" : r.GetString(r.GetOrdinal("address")),
                Status        = r.IsDBNull(r.GetOrdinal("status"))         ? "Active" : r.GetString(r.GetOrdinal("status")),
                DateAdded     = DateTime.TryParse(r.IsDBNull(r.GetOrdinal("date_added")) ? "" : r.GetString(r.GetOrdinal("date_added")), out DateTime da) ? da : DateTime.Now,
                PaymentDueDate = due,
                ReminderDays  = r.IsDBNull(r.GetOrdinal("reminder_days"))  ? 0 : r.GetInt32(r.GetOrdinal("reminder_days"))
            };
        }

        public static void AddSupplier(string name, string phone, string email, string address, string type, string contactPerson, DateTime? dueDate = null, int reminderDays = 0)
        {
            string code = "SUP-" + DateTime.Now.Ticks.ToString().Substring(10);
            string sql  = "INSERT INTO suppliers (supplier_name, phone, email, address, balance_due, type, supplier_code, contact_person, date_added, payment_due_date, reminder_days) " +
                          "VALUES (@name, @phone, @email, @addr, 0, @type, @code, @contact, datetime('now'), @due, @rem)";
            DatabaseHelper.ExecuteNonQuery(sql,
                new SqliteParameter("@name",    name),
                new SqliteParameter("@phone",   phone),
                new SqliteParameter("@email",   email),
                new SqliteParameter("@addr",    address),
                new SqliteParameter("@type",    type),
                new SqliteParameter("@code",    code),
                new SqliteParameter("@contact", contactPerson ?? ""),
                new SqliteParameter("@due",     dueDate.HasValue ? (object)dueDate.Value.ToString("s") : DBNull.Value),
                new SqliteParameter("@rem",     reminderDays));

            LogTransaction("SUPPLIER_ADD", $"Added Supplier: {name} ({code})");
        }

        public static void UpdateSupplier(int id, string name, string phone, string email, string address, string type, string contactPerson, DateTime? dueDate, int reminderDays)
        {
            string sql = "UPDATE suppliers SET supplier_name=@name, phone=@phone, email=@email, address=@addr, type=@type, contact_person=@contact, payment_due_date=@due, reminder_days=@rem WHERE id=@id";
            DatabaseHelper.ExecuteNonQuery(sql,
                new SqliteParameter("@name",    name),
                new SqliteParameter("@phone",   phone),
                new SqliteParameter("@email",   email),
                new SqliteParameter("@addr",    address),
                new SqliteParameter("@type",    type),
                new SqliteParameter("@contact", contactPerson ?? ""),
                new SqliteParameter("@due",     dueDate.HasValue ? (object)dueDate.Value.ToString("s") : DBNull.Value),
                new SqliteParameter("@rem",     reminderDays),
                new SqliteParameter("@id",      id));

            LogTransaction("SUPPLIER_UPDATE", $"Updated Supplier: {name} (ID: {id})");
        }

        public static void DeleteSupplier(int id)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE suppliers SET date_deleted = datetime('now') WHERE id = @id",
                new SqliteParameter("@id", id));
            LogTransaction("SUPPLIER_DELETE", $"Deleted Supplier ID: {id}");
        }

        private static void LogTransaction(string action, string description)
        {
            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO transactions (action_type, part_name, description, username) VALUES (@a, 'N/A', @d, 'System')",
                    new SqliteParameter("@a", action),
                    new SqliteParameter("@d", description));
            }
            catch { }
        }
    }
}
