using System;
using System.Data;
using Microsoft.Data.Sqlite;
using InventorySystem.Helpers;

namespace InventorySystem.Services
{
    /// <summary>
    /// Tracks raw milk received (expenses with liters > 0) and what it was processed into.
    /// Keeps a "Raw Milk" inventory item whose stock = liters received - liters used.
    /// </summary>
    public class MilkProductionService
    {
        private const string RawMilkSettingKey = "raw_milk_part_id";
        private const string RawMilkName = "Raw Milk";

        // - Raw milk inventory item -

        public int GetRawMilkPartId()
        {
            int id = DatabaseHelper.ExecuteScalar<int>(
                @"SELECT p.id FROM app_settings s JOIN parts p ON p.id = CAST(s.value AS INTEGER)
                  WHERE s.key = @k AND p.date_deleted IS NULL",
                new SqliteParameter("@k", RawMilkSettingKey));
            if (id > 0) return id;

            id = DatabaseHelper.ExecuteScalar<int>(
                "SELECT id FROM parts WHERE part_name = @n AND date_deleted IS NULL ORDER BY id LIMIT 1",
                new SqliteParameter("@n", RawMilkName));

            if (id <= 0)
            {
                DatabaseHelper.ExecuteNonQuery(
                    @"INSERT INTO parts (part_number, part_name, description, unit_of_measure, item_type, quantity_in_stock,
                                         minimum_stock_level, is_sales_item, is_purchase_item, is_stock_tracked, status)
                      VALUES ('RAW-MILK', @n, 'Raw milk received from suppliers (auto-managed)', 'L', 'Product', 0, 0, 0, 1, 1, 'Active')",
                    new SqliteParameter("@n", RawMilkName));
                id = DatabaseHelper.ExecuteScalar<int>(
                    "SELECT id FROM parts WHERE part_name = @n AND date_deleted IS NULL ORDER BY id DESC LIMIT 1",
                    new SqliteParameter("@n", RawMilkName));
            }

            if (id > 0)
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT OR REPLACE INTO app_settings (key, value) VALUES (@k, @v)",
                    new SqliteParameter("@k", RawMilkSettingKey),
                    new SqliteParameter("@v", id.ToString()));
            }
            return id;
        }

        public bool IsRawMilkPart(int partId)
        {
            return partId > 0 && partId == GetRawMilkPartId();
        }

        /// <summary>Recalculate Raw Milk stock from intakes and productions (safe to call any time).</summary>
        public void SyncRawMilkStock()
        {
            try
            {
                int id = GetRawMilkPartId();
                if (id <= 0) return;
                DatabaseHelper.ExecuteNonQuery(
                    @"UPDATE parts SET quantity_in_stock =
                        (SELECT IFNULL(SUM(liters), 0) FROM expenses WHERE date_deleted IS NULL AND liters > 0)
                      - (SELECT IFNULL(SUM(liters_used), 0) FROM milk_production WHERE date_deleted IS NULL)
                      WHERE id = @id",
                    new SqliteParameter("@id", id));
                GlobalEvents.RaiseInventoryUpdated();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "MilkProductionService.SyncRawMilkStock");
            }
        }

        // - Queries -

        public DataTable GetMilkIntakes(bool onlyRemaining, int? supplierId = null)
        {
            string sql = @"
                SELECT e.expense_id, e.expense_date, IFNULL(s.supplier_name, '') AS supplier_name,
                       IFNULL(e.milk_type, '') AS milk_type, e.description, e.liters,
                       IFNULL(p.used, 0) AS used, e.liters - IFNULL(p.used, 0) AS remaining
                FROM expenses e
                LEFT JOIN suppliers s ON s.id = e.supplier_id
                LEFT JOIN (SELECT expense_id, SUM(liters_used) AS used FROM milk_production
                           WHERE date_deleted IS NULL GROUP BY expense_id) p ON p.expense_id = e.expense_id
                WHERE e.date_deleted IS NULL AND e.liters > 0";
            if (onlyRemaining) sql += " AND e.liters - IFNULL(p.used, 0) > 0.0001";
            if (supplierId.HasValue) sql += " AND e.supplier_id = @sid";
            sql += " ORDER BY e.expense_date DESC";

            return supplierId.HasValue
                ? DatabaseHelper.ExecuteDataTable(sql, new SqliteParameter("@sid", supplierId.Value))
                : DatabaseHelper.ExecuteDataTable(sql);
        }

        public decimal GetRemainingLiters(int expenseId)
        {
            return DatabaseHelper.ExecuteScalar<decimal>(
                @"SELECT e.liters - IFNULL((SELECT SUM(liters_used) FROM milk_production
                                            WHERE expense_id = e.expense_id AND date_deleted IS NULL), 0)
                  FROM expenses e WHERE e.expense_id = @id AND e.date_deleted IS NULL",
                new SqliteParameter("@id", expenseId));
        }

        public bool HasProductions(int expenseId)
        {
            return DatabaseHelper.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM milk_production WHERE expense_id = @id AND date_deleted IS NULL",
                new SqliteParameter("@id", expenseId)) > 0;
        }

        public DataTable GetProductions(int? expenseId)
        {
            string sql = @"
                SELECT m.production_id, m.expense_id, m.production_date, p.part_name, IFNULL(p.unit_of_measure, '') AS unit,
                       m.liters_used, m.quantity_made, m.notes, m.recorded_by, IFNULL(s.supplier_name, '') AS supplier_name
                FROM milk_production m
                LEFT JOIN parts p ON p.id = m.part_id
                LEFT JOIN expenses e ON e.expense_id = m.expense_id
                LEFT JOIN suppliers s ON s.id = e.supplier_id
                WHERE m.date_deleted IS NULL";
            if (expenseId.HasValue) sql += " AND m.expense_id = @id";
            sql += " ORDER BY m.production_date DESC";

            return expenseId.HasValue
                ? DatabaseHelper.ExecuteDataTable(sql, new SqliteParameter("@id", expenseId.Value))
                : DatabaseHelper.ExecuteDataTable(sql);
        }

        /// <summary>Products that milk can be turned into (stock-tracked products, excluding Raw Milk itself).</summary>
        public DataTable GetProducibleProducts()
        {
            return DatabaseHelper.ExecuteDataTable(
                @"SELECT p.id, p.part_name, IFNULL(p.unit_of_measure, '') AS unit, IFNULL(c.category_name, '') AS category,
                         IFNULL(p.barcode, '') AS barcode, IFNULL(p.part_number, '') AS part_number, p.quantity_in_stock
                  FROM parts p
                  LEFT JOIN categories c ON c.id = p.category_id
                  WHERE p.date_deleted IS NULL AND IFNULL(p.is_inactive, 0) = 0
                    AND (p.item_type IS NULL OR p.item_type != 'Service') AND p.id != @raw
                  ORDER BY p.part_name",
                new SqliteParameter("@raw", GetRawMilkPartId()));
        }

        // - Commands -

        /// <summary>Record a production. Returns null on success, or an error message.</summary>
        public string AddProduction(int expenseId, int partId, decimal litersUsed, decimal quantityMade, string notes, string recordedBy = null)
        {
            if (litersUsed <= 0 || quantityMade <= 0) return LocalizationManager.GetString("Milk_ErrPositive", "Liters used and quantity made must be greater than zero.");
            decimal remaining = GetRemainingLiters(expenseId);
            if (litersUsed > remaining + 0.0001m)
                return string.Format(LocalizationManager.GetString("Milk_ErrTooMuch", "Only {0} L left in this milk intake."), remaining.ToString("0.##"));

            try
            {
                using (var conn = new SqliteConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        using (var cmd = new SqliteCommand(
                            @"INSERT INTO milk_production (expense_id, part_id, liters_used, quantity_made, notes, recorded_by)
                              VALUES (@e, @p, @l, @q, @n, @u)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@e", expenseId);
                            cmd.Parameters.AddWithValue("@p", partId);
                            cmd.Parameters.AddWithValue("@l", litersUsed);
                            cmd.Parameters.AddWithValue("@q", quantityMade);
                            cmd.Parameters.AddWithValue("@n", (object)notes ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@u", recordedBy ?? UserSession.Username ?? "");
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new SqliteCommand(
                            "UPDATE parts SET quantity_in_stock = quantity_in_stock + @q WHERE id = @p", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@q", quantityMade);
                            cmd.Parameters.AddWithValue("@p", partId);
                            cmd.ExecuteNonQuery();
                        }
                        tx.Commit();
                    }
                }

                string partName = DatabaseHelper.ExecuteScalar<string>("SELECT part_name FROM parts WHERE id = @id", new SqliteParameter("@id", partId)) ?? "Unknown";
                DatabaseHelper.LogTransaction("PRODUCTION_IN", partName,
                    $"Made {quantityMade:0.##} of {partName} from {litersUsed:0.##} L raw milk (intake #{expenseId})");
                SyncRawMilkStock();
                return null;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "MilkProductionService.AddProduction");
                return ex.Message;
            }
        }

        public bool DeleteProduction(int productionId)
        {
            try
            {
                var dt = DatabaseHelper.ExecuteDataTable(
                    "SELECT part_id, quantity_made FROM milk_production WHERE production_id = @id AND date_deleted IS NULL",
                    new SqliteParameter("@id", productionId));
                if (dt.Rows.Count == 0) return false;
                int partId = Convert.ToInt32(dt.Rows[0]["part_id"]);
                decimal qty = Convert.ToDecimal(dt.Rows[0]["quantity_made"]);

                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE milk_production SET date_deleted = datetime('now', 'localtime') WHERE production_id = @id",
                    new SqliteParameter("@id", productionId));
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE parts SET quantity_in_stock = quantity_in_stock - @q WHERE id = @p",
                    new SqliteParameter("@q", qty),
                    new SqliteParameter("@p", partId));

                string partName = DatabaseHelper.ExecuteScalar<string>("SELECT part_name FROM parts WHERE id = @id", new SqliteParameter("@id", partId)) ?? "Unknown";
                DatabaseHelper.LogTransaction("PRODUCTION_UNDO", partName, $"Removed production #{productionId} ({qty:0.##} of {partName})");
                SyncRawMilkStock();
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "MilkProductionService.DeleteProduction");
                return false;
            }
        }
    }
}
