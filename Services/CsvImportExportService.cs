using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using InventorySystem.Data;
using InventorySystem.Helpers;
using Microsoft.Data.Sqlite;

namespace InventorySystem.Services
{
    public class ImportBatchResult
    {
        public int Imported { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public int Total => Imported + Updated + Skipped;
    }

    /// <summary>
    /// Shared CSV import/export mapping with column aliases and upsert logic.
    /// Used by web API and WinForms import paths.
    /// </summary>
    public static class CsvImportExportService
    {
        #region Row helpers

        public static Dictionary<string, string> RowFromDataRow(DataRow row)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (row?.Table == null) return dict;
            foreach (DataColumn col in row.Table.Columns)
                dict[col.ColumnName] = row[col]?.ToString()?.Trim() ?? "";
            return dict;
        }

        public static string Cell(Dictionary<string, string> row, params string[] keys)
        {
            if (row == null) return "";
            foreach (var key in keys)
            {
                if (string.IsNullOrWhiteSpace(key)) continue;
                foreach (var kv in row)
                {
                    if (string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(kv.Value))
                        return kv.Value.Trim();
                }
            }
            return "";
        }

        public static bool ParseBool(string value, bool defaultValue = false)
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;
            var v = value.Trim().ToLowerInvariant();
            if (v is "1" or "true" or "yes" or "y") return true;
            if (v is "0" or "false" or "no" or "n") return false;
            return defaultValue;
        }

        public static int ParseInt(string value, int defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;
            return int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : defaultValue;
        }

        public static decimal ParseDecimal(string value, decimal defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;
            // Tolerate values copied out of a formatted export, e.g. "$1,250.00".
            var v = value.Trim().Replace(",", "").TrimStart('$', '\u20AC', '\u00A3', '\u00A5').Trim();
            return decimal.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : defaultValue;
        }

        /// <summary>True if the import file included this column (any alias).</summary>
        public static bool ColumnInRow(Dictionary<string, string> row, params string[] keys)
        {
            if (row == null || keys == null) return false;
            foreach (var key in keys)
            {
                if (string.IsNullOrWhiteSpace(key)) continue;
                foreach (var rk in row.Keys)
                    if (string.Equals(rk, key, StringComparison.OrdinalIgnoreCase))
                        return true;
            }
            return false;
        }

        private static string MergeString(string existing, Dictionary<string, string> row, params string[] keys)
        {
            if (!ColumnInRow(row, keys)) return existing;
            string v = Cell(row, keys);
            return string.IsNullOrWhiteSpace(v) ? existing : v;
        }

        private static int MergeInt(int existing, Dictionary<string, string> row, params string[] keys)
        {
            if (!ColumnInRow(row, keys)) return existing;
            string v = Cell(row, keys);
            return string.IsNullOrWhiteSpace(v) ? existing : ParseInt(v, existing);
        }

        private static decimal MergeDecimal(decimal existing, Dictionary<string, string> row, params string[] keys)
        {
            if (!ColumnInRow(row, keys)) return existing;
            string v = Cell(row, keys);
            return string.IsNullOrWhiteSpace(v) ? existing : ParseDecimal(v, existing);
        }

        private static bool MergeBool(bool existing, Dictionary<string, string> row, params string[] keys)
        {
            if (!ColumnInRow(row, keys)) return existing;
            string v = Cell(row, keys);
            return string.IsNullOrWhiteSpace(v) ? existing : ParseBool(v, existing);
        }

        private static PartData MergeProduct(PartData existing, PartData imported, Dictionary<string, string> row)
        {
            if (existing == null) return imported;
            var m = new PartData
            {
                Id = existing.Id,
                PartName = MergeString(existing.PartName, row, "part_name", "PartName", "name", "product_name", "product"),
                PartNumber = MergeString(existing.PartNumber, row, "part_number", "PartNumber", "sku", "SKU"),
                Description = MergeString(existing.Description, row, "description", "Description", "desc"),
                CategoryName = MergeString(existing.CategoryName, row, "category_name", "Category", "category"),
                SupplierName = MergeString(existing.SupplierName, row, "supplier_name", "supplier", "SupplierName"),
                QuantityInStock = MergeInt(existing.QuantityInStock, row, "quantity_in_stock", "Quantity", "stock", "qty"),
                MinimumStockLevel = MergeInt(existing.MinimumStockLevel, row, "minimum_stock_level", "MinimumStock", "min_stock", "minstock"),
                ReorderQuantity = MergeInt(existing.ReorderQuantity, row, "reorder_quantity", "ReorderQuantity", "reorder"),
                PurchasePrice = MergeDecimal(existing.PurchasePrice, row, "purchase_price", "cost", "Cost", "PurchasePrice"),
                SellingPrice = MergeDecimal(existing.SellingPrice, row, "selling_price", "UnitPrice", "price", "Price"),
                Location = MergeString(existing.Location, row, "location", "Location"),
                Shelf = MergeString(existing.Shelf, row, "shelf", "Shelf"),
                Barcode = MergeString(existing.Barcode, row, "barcode", "Barcode"),
                Status = MergeString(existing.Status, row, "status", "Status"),
                ItemType = MergeString(existing.ItemType, row, "item_type", "ItemType", "type"),
                UnitOfMeasure = MergeString(existing.UnitOfMeasure, row, "unit_of_measure", "uom", "Uom", "UOM"),
                BatchNumber = MergeString(existing.BatchNumber, row, "batch_number", "batch", "Batch", "BatchNumber"),
                ExpiryDate = MergeString(existing.ExpiryDate, row, "expiry_date", "expiry", "Expiry", "ExpiryDate"),
                IsSalesItem = MergeBool(existing.IsSalesItem, row, "is_sales_item", "sales_item", "IsSalesItem"),
                IsPurchaseItem = MergeBool(existing.IsPurchaseItem, row, "is_purchase_item", "purchase_item", "IsPurchaseItem"),
                IsInactive = MergeBool(existing.IsInactive, row, "is_inactive", "inactive", "IsInactive"),
                TaxRate = MergeDecimal(existing.TaxRate, row, "tax_rate", "tax", "TaxRate"),
                IsStockTracked = MergeBool(existing.IsStockTracked, row, "is_stock_tracked", "stock_tracked", "IsStockTracked"),
                SellByWeight = MergeBool(existing.SellByWeight, row, "sell_by_weight", "sellbyweight", "SellByWeight"),
                Price2 = MergeDecimal(existing.Price2, row, "price2", "Price2"),
                Price3 = MergeDecimal(existing.Price3, row, "price3", "Price3"),
                Price4 = MergeDecimal(existing.Price4, row, "price4", "Price4"),
                PartImage = existing.PartImage,
                SupplierId = existing.SupplierId
            };

            if (ColumnInRow(row, "part_image", "image", "Image", "PartImage", "imagepath"))
            {
                string img = Cell(row, "part_image", "image", "Image", "PartImage", "imagepath");
                if (!string.IsNullOrWhiteSpace(img))
                    m.PartImage = NormalizeImagePath(img);
            }

            if (ColumnInRow(row, "supplier_id", "supplierid"))
            {
                string supIdStr = Cell(row, "supplier_id", "supplierid");
                if (!string.IsNullOrWhiteSpace(supIdStr) && int.TryParse(supIdStr, out int supId) && supId > 0)
                    m.SupplierId = supId;
            }

            if (string.IsNullOrWhiteSpace(m.CategoryName)) m.CategoryName = existing.CategoryName ?? "General";
            if (m.IsInactive) m.Status = "Inactive";
            else if (!ColumnInRow(row, "status", "Status") && !ColumnInRow(row, "is_inactive", "inactive"))
                m.Status = existing.Status;

            return m;
        }

        #endregion

        #region Product mapping

        public static PartData MapProductRow(Dictionary<string, string> row)
        {
            if (row == null) return null;

            string name = Cell(row, "part_name", "PartName", "name", "product_name", "product");
            if (string.IsNullOrWhiteSpace(name)) return null;

            string status = Cell(row, "status", "Status");
            bool inactive = ParseBool(Cell(row, "is_inactive", "inactive", "IsInactive"));
            if (string.IsNullOrWhiteSpace(status))
                status = inactive ? "Inactive" : "Active";
            else if (inactive)
                status = "Inactive";

            bool sellByWeight = ParseBool(Cell(row, "sell_by_weight", "sellbyweight", "SellByWeight"));
            string uom = Cell(row, "unit_of_measure", "uom", "Uom", "UOM");
            string imageRaw = Cell(row, "part_image", "image", "Image", "PartImage", "imagepath");

            var part = new PartData
            {
                PartName = name,
                PartNumber = Cell(row, "part_number", "PartNumber", "sku", "SKU"),
                Description = Cell(row, "description", "Description", "desc"),
                CategoryName = Cell(row, "category_name", "Category", "category") is { Length: > 0 } cat ? cat : "General",
                SupplierName = Cell(row, "supplier_name", "supplier", "SupplierName"),
                QuantityInStock = ParseInt(Cell(row, "quantity_in_stock", "Quantity", "stock", "qty")),
                MinimumStockLevel = ParseInt(Cell(row, "minimum_stock_level", "MinimumStock", "min_stock", "minstock")),
                ReorderQuantity = ParseInt(Cell(row, "reorder_quantity", "ReorderQuantity", "reorder")),
                PurchasePrice = ParseDecimal(Cell(row, "purchase_price", "cost", "Cost", "PurchasePrice")),
                SellingPrice = ParseDecimal(Cell(row, "selling_price", "UnitPrice", "price", "Price")),
                Location = Cell(row, "location", "Location"),
                Shelf = Cell(row, "shelf", "Shelf"),
                Barcode = Cell(row, "barcode", "Barcode"),
                Status = status,
                ItemType = Cell(row, "item_type", "ItemType", "type") is { Length: > 0 } t ? t : "Product",
                UnitOfMeasure = uom,
                BatchNumber = Cell(row, "batch_number", "batch", "Batch", "BatchNumber"),
                ExpiryDate = Cell(row, "expiry_date", "expiry", "Expiry", "ExpiryDate"),
                IsSalesItem = ParseBool(Cell(row, "is_sales_item", "sales_item", "IsSalesItem"), true),
                IsPurchaseItem = ParseBool(Cell(row, "is_purchase_item", "purchase_item", "IsPurchaseItem")),
                IsInactive = inactive,
                TaxRate = ParseDecimal(Cell(row, "tax_rate", "tax", "TaxRate")),
                IsStockTracked = ParseBool(Cell(row, "is_stock_tracked", "stock_tracked", "IsStockTracked"), true),
                SellByWeight = sellByWeight,
                Price2 = ParseDecimal(Cell(row, "price2", "Price2")),
                Price3 = ParseDecimal(Cell(row, "price3", "Price3")),
                Price4 = ParseDecimal(Cell(row, "price4", "Price4"))
            };

            if (!string.IsNullOrWhiteSpace(imageRaw))
                part.PartImage = NormalizeImagePath(imageRaw);

            string supIdStr = Cell(row, "supplier_id", "supplierid");
            if (int.TryParse(supIdStr, out int supId) && supId > 0)
                part.SupplierId = supId;

            return part;
        }

        private static string NormalizeImagePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            path = path.Trim();
            if (path.StartsWith("/")) path = path.Substring(1);
            return path;
        }

        private static DateTime? ParseDateNullable(string raw)
        {
            string normalized = NormalizeDate(raw);
            if (string.IsNullOrWhiteSpace(normalized)) return null;
            if (DateTime.TryParse(normalized, out var dt)) return dt.Date;
            return null;
        }

        public static int? FindProductId(string partNumber, string barcode, string partName)
        {
            if (!string.IsNullOrWhiteSpace(partNumber))
            {
                var id = DatabaseHelper.ExecuteScalar<object>(
                    "SELECT id FROM parts WHERE part_number = @pn AND date_deleted IS NULL LIMIT 1",
                    new SqliteParameter("@pn", partNumber.Trim()));
                if (id != null && id != DBNull.Value) return Convert.ToInt32(id);
            }
            if (!string.IsNullOrWhiteSpace(barcode))
            {
                var id = DatabaseHelper.ExecuteScalar<object>(
                    "SELECT id FROM parts WHERE barcode = @bc AND date_deleted IS NULL LIMIT 1",
                    new SqliteParameter("@bc", barcode.Trim()));
                if (id != null && id != DBNull.Value) return Convert.ToInt32(id);
            }
            if (!string.IsNullOrWhiteSpace(partName))
            {
                var id = DatabaseHelper.ExecuteScalar<object>(
                    "SELECT id FROM parts WHERE part_name = @n AND date_deleted IS NULL LIMIT 1",
                    new SqliteParameter("@n", partName.Trim()));
                if (id != null && id != DBNull.Value) return Convert.ToInt32(id);
            }
            return null;
        }

        public static ImportBatchResult ImportProducts(IEnumerable<Dictionary<string, string>> rows)
        {
            var result = new ImportBatchResult();
            var inventory = new InventoryService();

            foreach (var row in rows ?? Enumerable.Empty<Dictionary<string, string>>())
            {
                try
                {
                    var part = MapProductRow(row);
                    if (part == null) { result.Skipped++; continue; }

                    if (!part.SupplierId.HasValue && !string.IsNullOrWhiteSpace(part.SupplierName))
                    {
                        var supId = DatabaseHelper.ExecuteScalar<object>(
                            "SELECT id FROM suppliers WHERE supplier_name = @n AND date_deleted IS NULL LIMIT 1",
                            new SqliteParameter("@n", part.SupplierName));
                        if (supId != null && supId != DBNull.Value)
                            part.SupplierId = Convert.ToInt32(supId);
                    }

                    int? existingId = FindProductId(part.PartNumber, part.Barcode, part.PartName);
                    if (existingId.HasValue)
                    {
                        part.Id = existingId.Value;
                        var existing = PartData.GetById(existingId.Value);
                        if (existing != null)
                            part = MergeProduct(existing, part, row);

                        if (!part.SupplierId.HasValue && !string.IsNullOrWhiteSpace(part.SupplierName))
                        {
                            var supId = DatabaseHelper.ExecuteScalar<object>(
                                "SELECT id FROM suppliers WHERE supplier_name = @n AND date_deleted IS NULL LIMIT 1",
                                new SqliteParameter("@n", part.SupplierName));
                            if (supId != null && supId != DBNull.Value)
                                part.SupplierId = Convert.ToInt32(supId);
                        }

                        inventory.SaveProductService(part);
                        result.Updated++;
                    }
                    else
                    {
                        part.Id = 0;
                        inventory.SaveProductService(part);
                        result.Imported++;
                    }
                }
                catch (Exception ex)
                {
                    result.Skipped++;
                    result.Errors.Add(ex.Message);
                    ErrorLogger.LogError(ex, "CsvImportExportService.ImportProducts");
                }
            }

            return result;
        }

        #endregion

        #region Customer mapping

        public static ImportBatchResult ImportCustomers(IEnumerable<Dictionary<string, string>> rows)
        {
            var result = new ImportBatchResult();
            var service = new CustomerService();

            foreach (var row in rows ?? Enumerable.Empty<Dictionary<string, string>>())
            {
                try
                {
                    string name = Cell(row, "CustomerName", "customername", "name", "full_name", "customer");
                    if (string.IsNullOrWhiteSpace(name)) { result.Skipped++; continue; }

                    string phone = Cell(row, "Phone", "phone");
                    string email = Cell(row, "Email", "email");
                    string address = Cell(row, "Address", "address");
                    string city = Cell(row, "City", "city");
                    string postal = Cell(row, "PostalCode", "postalcode", "postal_code", "zip");
                    if (!string.IsNullOrWhiteSpace(city) || !string.IsNullOrWhiteSpace(postal))
                    {
                        address = (address ?? "") +
                            (string.IsNullOrWhiteSpace(city) ? "" : ", " + city) +
                            (string.IsNullOrWhiteSpace(postal) ? "" : " " + postal);
                    }
                    string type = Cell(row, "CustomerType", "customertype", "type") is { Length: > 0 } t ? t : "Regular";
                    decimal creditLimit = ParseDecimal(Cell(row, "CreditLimit", "creditlimit", "credit_limit"), 1000);
                    DateTime? dueDate = ParseDateNullable(Cell(row, "PaymentDueDate", "payment_due_date", "duedate", "due_date"));
                    int reminderDays = ParseInt(Cell(row, "ReminderDays", "reminder_days", "reminderdays"));
                    string balanceRaw = Cell(row, "Balance", "balance", "current_balance");
                    bool hasBalance = !string.IsNullOrWhiteSpace(balanceRaw);

                    var existingId = DatabaseHelper.ExecuteScalar<object>(
                        "SELECT customer_id FROM customers WHERE full_name = @n COLLATE NOCASE AND date_deleted IS NULL LIMIT 1",
                        new SqliteParameter("@n", name));

                    int customerId;
                    if (existingId != null && existingId != DBNull.Value)
                    {
                        customerId = Convert.ToInt32(existingId);
                        var exDt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT full_name, phone, email, address, type, credit_limit, payment_due_date, reminder_days, current_balance
                              FROM customers WHERE customer_id = @id",
                            new SqliteParameter("@id", customerId));
                        var ex = exDt.Rows.Count > 0 ? exDt.Rows[0] : null;

                        string finalName = ex != null ? MergeString(ex["full_name"]?.ToString(), row, "CustomerName", "customername", "name", "full_name", "customer") : name;
                        string finalPhone = ex != null ? MergeString(ex["phone"]?.ToString(), row, "Phone", "phone") : phone;
                        string finalEmail = ex != null ? MergeString(ex["email"]?.ToString(), row, "Email", "email") : email;
                        string finalAddress = ex != null ? MergeString(ex["address"]?.ToString(), row, "Address", "address") : address;
                        if (ColumnInRow(row, "City", "city") || ColumnInRow(row, "PostalCode", "postalcode", "postal_code", "zip"))
                        {
                            string cityPart = ColumnInRow(row, "City", "city") ? Cell(row, "City", "city") : "";
                            string postalPart = ColumnInRow(row, "PostalCode", "postalcode", "postal_code", "zip") ? Cell(row, "PostalCode", "postalcode", "postal_code", "zip") : "";
                            if (!string.IsNullOrWhiteSpace(cityPart) || !string.IsNullOrWhiteSpace(postalPart))
                            {
                                finalAddress = (finalAddress ?? "") +
                                    (string.IsNullOrWhiteSpace(cityPart) ? "" : ", " + cityPart) +
                                    (string.IsNullOrWhiteSpace(postalPart) ? "" : " " + postalPart);
                            }
                        }
                        else if (!ColumnInRow(row, "Address", "address") && ex != null)
                        {
                            finalAddress = ex["address"]?.ToString();
                        }

                        string finalType = ex != null
                            ? (ColumnInRow(row, "CustomerType", "customertype", "type") && !string.IsNullOrWhiteSpace(Cell(row, "CustomerType", "customertype", "type"))
                                ? Cell(row, "CustomerType", "customertype", "type") : ex["type"]?.ToString())
                            : type;
                        decimal finalCredit = ex != null
                            ? MergeDecimal(ex["credit_limit"] == DBNull.Value ? 1000m : Convert.ToDecimal(ex["credit_limit"]), row, "CreditLimit", "creditlimit", "credit_limit")
                            : creditLimit;
                        DateTime? finalDue = dueDate;
                        if (ex != null)
                        {
                            if (ColumnInRow(row, "PaymentDueDate", "payment_due_date", "duedate", "due_date"))
                            {
                                string dueRaw = Cell(row, "PaymentDueDate", "payment_due_date", "duedate", "due_date");
                                finalDue = string.IsNullOrWhiteSpace(dueRaw)
                                    ? (ex["payment_due_date"] == DBNull.Value ? null : ParseDateNullable(ex["payment_due_date"]?.ToString()))
                                    : ParseDateNullable(dueRaw);
                            }
                            else
                                finalDue = ex["payment_due_date"] == DBNull.Value ? null : ParseDateNullable(ex["payment_due_date"]?.ToString());
                        }
                        int finalReminder = ex != null
                            ? MergeInt(ex["reminder_days"] == DBNull.Value ? 0 : Convert.ToInt32(ex["reminder_days"]), row, "ReminderDays", "reminder_days", "reminderdays")
                            : reminderDays;

                        service.UpdateCustomer(customerId, finalName, finalPhone, finalEmail, finalAddress, finalType ?? "Regular", finalCredit, finalDue, finalReminder);
                        result.Updated++;
                    }
                    else
                    {
                        customerId = service.AddCustomer(name, phone, email, address, type, creditLimit, dueDate, reminderDays);
                        result.Imported++;
                    }

                    if (ColumnInRow(row, "Balance", "balance", "current_balance") && hasBalance)
                    {
                        DatabaseHelper.ExecuteNonQuery(
                            "UPDATE customers SET current_balance = @b WHERE customer_id = @id",
                            new SqliteParameter("@b", ParseDecimal(balanceRaw)),
                            new SqliteParameter("@id", customerId));
                    }
                }
                catch (Exception ex)
                {
                    result.Skipped++;
                    result.Errors.Add(ex.Message);
                    ErrorLogger.LogError(ex, "CsvImportExportService.ImportCustomers");
                }
            }

            return result;
        }

        #endregion

        #region Supplier mapping

        public static ImportBatchResult ImportSuppliers(IEnumerable<Dictionary<string, string>> rows)
        {
            var result = new ImportBatchResult();
            var service = new SupplierService();

            foreach (var row in rows ?? Enumerable.Empty<Dictionary<string, string>>())
            {
                try
                {
                    string name = Cell(row, "SupplierName", "suppliername", "name", "supplier");
                    if (string.IsNullOrWhiteSpace(name)) { result.Skipped++; continue; }

                    string contact = Cell(row, "ContactPerson", "contactperson", "contact");
                    string email = Cell(row, "Email", "email");
                    string phone = Cell(row, "Phone", "phone");
                    string address = Cell(row, "Address", "address");
                    string city = Cell(row, "City", "city");
                    string postal = Cell(row, "PostalCode", "postalcode", "postal_code", "zip");
                    string website = Cell(row, "Website", "website");
                    string notes = Cell(row, "Notes", "notes");
                    string type = Cell(row, "type", "Type", "SupplierType", "suppliertype") is { Length: > 0 } t ? t : "Regular";
                    DateTime? dueDate = ParseDateNullable(Cell(row, "PaymentDueDate", "payment_due_date", "duedate", "due_date"));
                    int reminderDays = ParseInt(Cell(row, "ReminderDays", "reminder_days", "reminderdays"));
                    string balanceRaw = Cell(row, "Balance", "balance", "balance_due");
                    bool hasBalance = !string.IsNullOrWhiteSpace(balanceRaw);

                    string fullAddress = address ?? "";
                    if (!string.IsNullOrWhiteSpace(city)) fullAddress += (fullAddress.Length > 0 ? ", " : "") + city;
                    if (!string.IsNullOrWhiteSpace(postal)) fullAddress += (fullAddress.Length > 0 ? " " : "") + postal;
                    if (!string.IsNullOrWhiteSpace(website)) fullAddress += (fullAddress.Length > 0 ? " | " : "") + website;
                    if (!string.IsNullOrWhiteSpace(notes)) fullAddress += (fullAddress.Length > 0 ? " | " : "") + notes;

                    var existingId = DatabaseHelper.ExecuteScalar<object>(
                        "SELECT id FROM suppliers WHERE supplier_name = @n COLLATE NOCASE AND date_deleted IS NULL LIMIT 1",
                        new SqliteParameter("@n", name));

                    int supplierId;
                    if (existingId != null && existingId != DBNull.Value)
                    {
                        supplierId = Convert.ToInt32(existingId);
                        var exDt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT supplier_name, contact_person, phone, email, address, type, payment_due_date, reminder_days, balance_due
                              FROM suppliers WHERE id = @id",
                            new SqliteParameter("@id", supplierId));
                        var ex = exDt.Rows.Count > 0 ? exDt.Rows[0] : null;

                        string finalName = ex != null ? MergeString(ex["supplier_name"]?.ToString(), row, "SupplierName", "suppliername", "name", "supplier") : name;
                        string finalPhone = ex != null ? MergeString(ex["phone"]?.ToString(), row, "Phone", "phone") : phone;
                        string finalEmail = ex != null ? MergeString(ex["email"]?.ToString(), row, "Email", "email") : email;
                        string finalContact = ex != null ? MergeString(ex["contact_person"]?.ToString(), row, "ContactPerson", "contactperson", "contact") : contact;

                        string finalAddress = fullAddress;
                        if (ex != null && !ColumnInRow(row, "Address", "address", "City", "city", "PostalCode", "postalcode", "Website", "website", "Notes", "notes"))
                            finalAddress = ex["address"]?.ToString() ?? fullAddress;
                        else if (ex != null && ColumnInRow(row, "Address", "address"))
                            finalAddress = MergeString(ex["address"]?.ToString(), row, "Address", "address");

                        string finalType = ex != null
                            ? MergeString(ex["type"]?.ToString() ?? "Regular", row, "type", "Type", "SupplierType", "suppliertype")
                            : type;
                        if (string.IsNullOrWhiteSpace(finalType)) finalType = "Regular";

                        DateTime? finalDue = dueDate;
                        if (ex != null)
                        {
                            if (ColumnInRow(row, "PaymentDueDate", "payment_due_date", "duedate", "due_date"))
                            {
                                string dueRaw = Cell(row, "PaymentDueDate", "payment_due_date", "duedate", "due_date");
                                finalDue = string.IsNullOrWhiteSpace(dueRaw)
                                    ? (ex["payment_due_date"] == DBNull.Value ? null : ParseDateNullable(ex["payment_due_date"]?.ToString()))
                                    : ParseDateNullable(dueRaw);
                            }
                            else
                                finalDue = ex["payment_due_date"] == DBNull.Value ? null : ParseDateNullable(ex["payment_due_date"]?.ToString());
                        }

                        int finalReminder = ex != null
                            ? MergeInt(ex["reminder_days"] == DBNull.Value ? 0 : Convert.ToInt32(ex["reminder_days"]), row, "ReminderDays", "reminder_days", "reminderdays")
                            : reminderDays;

                        service.UpdateSupplier(supplierId, finalName, finalPhone, finalEmail, finalAddress ?? "", finalType, finalDue, finalReminder);
                        if (!string.IsNullOrWhiteSpace(finalContact))
                        {
                            DatabaseHelper.ExecuteNonQuery(
                                "UPDATE suppliers SET contact_person = @c WHERE id = @id",
                                new SqliteParameter("@c", finalContact),
                                new SqliteParameter("@id", supplierId));
                        }
                        result.Updated++;
                    }
                    else
                    {
                        service.AddSupplier(name, phone, email, fullAddress, type, dueDate, reminderDays);
                        supplierId = DatabaseHelper.ExecuteScalar<int>(
                            "SELECT id FROM suppliers WHERE supplier_name = @n AND date_deleted IS NULL ORDER BY id DESC LIMIT 1",
                            new SqliteParameter("@n", name));
                        if (!string.IsNullOrWhiteSpace(contact))
                        {
                            DatabaseHelper.ExecuteNonQuery(
                                "UPDATE suppliers SET contact_person = @c WHERE id = @id",
                                new SqliteParameter("@c", contact),
                                new SqliteParameter("@id", supplierId));
                        }
                        result.Imported++;
                    }

                    if (ColumnInRow(row, "Balance", "balance", "balance_due") && hasBalance)
                    {
                        DatabaseHelper.ExecuteNonQuery(
                            "UPDATE suppliers SET balance_due = @b WHERE id = @id",
                            new SqliteParameter("@b", ParseDecimal(balanceRaw)),
                            new SqliteParameter("@id", supplierId));
                    }
                }
                catch (Exception ex)
                {
                    result.Skipped++;
                    result.Errors.Add(ex.Message);
                    ErrorLogger.LogError(ex, "CsvImportExportService.ImportSuppliers");
                }
            }

            return result;
        }

        #endregion

        #region Expense mapping

        public static ImportBatchResult ImportExpenses(IEnumerable<Dictionary<string, string>> rows, string recordedBy = "Import")
        {
            var result = new ImportBatchResult();

            foreach (var row in rows ?? Enumerable.Empty<Dictionary<string, string>>())
            {
                try
                {
                    string category = Cell(row, "category", "Category");
                    decimal amount = ParseDecimal(Cell(row, "amount", "Amount"));
                    if (string.IsNullOrWhiteSpace(category) || amount <= 0) { result.Skipped++; continue; }

                    string dateRaw = Cell(row, "date", "expensedate", "expense_date", "ExpenseDate");
                    string expenseDate = NormalizeDate(dateRaw) ?? DateTime.Now.ToString("yyyy-MM-dd");
                    string description = Cell(row, "description", "desc", "Description");
                    bool isPaid = ParseBool(Cell(row, "paid", "is_paid", "ispaid"), false);
                    bool isRecurring = ParseBool(Cell(row, "recurring", "is_recurring", "isrecurring"), false);

                    // Date + category + amount + description is the natural key, so re-importing
                    // an exported file updates the existing rows instead of duplicating them.
                    int existingId = DatabaseHelper.ExecuteScalar<int>(
                        @"SELECT COALESCE(MAX(expense_id), 0) FROM expenses
                          WHERE date_deleted IS NULL
                            AND date(expense_date) = date(@d)
                            AND category = @c
                            AND amount = @a
                            AND COALESCE(description, '') = @desc",
                        new SqliteParameter("@c", category),
                        new SqliteParameter("@d", expenseDate),
                        new SqliteParameter("@a", amount),
                        new SqliteParameter("@desc", description ?? ""));

                    if (existingId > 0)
                    {
                        DatabaseHelper.ExecuteNonQuery(
                            @"UPDATE expenses SET is_paid = @paid, is_recurring = @rec WHERE expense_id = @id",
                            new SqliteParameter("@paid", isPaid ? 1 : 0),
                            new SqliteParameter("@rec", isRecurring ? 1 : 0),
                            new SqliteParameter("@id", existingId));
                        result.Updated++;
                    }
                    else
                    {
                        DatabaseHelper.ExecuteNonQuery(
                            @"INSERT INTO expenses (category, expense_date, amount, description, recorded_by, is_paid, is_recurring)
                              VALUES (@c, @d, @a, @desc, @u, @paid, @rec)",
                            new SqliteParameter("@c", category),
                            new SqliteParameter("@d", expenseDate),
                            new SqliteParameter("@a", amount),
                            new SqliteParameter("@desc", description),
                            new SqliteParameter("@u", recordedBy),
                            new SqliteParameter("@paid", isPaid ? 1 : 0),
                            new SqliteParameter("@rec", isRecurring ? 1 : 0));
                        result.Imported++;
                    }
                }
                catch (Exception ex)
                {
                    result.Skipped++;
                    result.Errors.Add(ex.Message);
                    ErrorLogger.LogError(ex, "CsvImportExportService.ImportExpenses");
                }
            }

            return result;
        }

        private static string NormalizeDate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (raw.Contains('T')) raw = raw.Split('T')[0];
            if (DateTime.TryParse(raw, out var dt))
                return dt.ToString("yyyy-MM-dd");
            return null;
        }

        #endregion

        #region Export tables

        private static DataTable MarkCurrency(DataTable dt, params string[] columns) =>
            ImportExportHelper.MarkCurrencyColumns(dt, columns);

        /// <summary>SQL fragment rendering a 0/1 flag as Yes/No for export readability.</summary>
        private static string YesNo(string expression, string alias) =>
            $"CASE WHEN COALESCE({expression}, 0) IN (1, '1') THEN 'Yes' ELSE 'No' END AS {alias}";

        public static DataTable BuildProductsExportTable(bool includeInactive = true)
        {
            string statusFilter = includeInactive
                ? "WHERE p.date_deleted IS NULL"
                : "WHERE p.date_deleted IS NULL AND (p.status = 'Active' OR p.status IS NULL) AND COALESCE(p.is_inactive, 0) = 0";

            return MarkCurrency(DatabaseHelper.ExecuteDataTable($@"
                SELECT p.part_number, p.part_name, p.description,
                       COALESCE(c.category_name, 'General') AS category_name,
                       p.supplier_id, COALESCE(s.supplier_name, '') AS supplier_name,
                       p.quantity_in_stock, p.selling_price, p.purchase_price,
                       p.minimum_stock_level, p.reorder_quantity, p.location, p.shelf, p.barcode,
                       p.unit_of_measure, p.batch_number, p.expiry_date, p.item_type,
                       CASE WHEN COALESCE(p.is_sales_item, 1) IN (1, '1') THEN 'Yes' ELSE 'No' END AS is_sales_item,
                       {YesNo("p.is_purchase_item", "is_purchase_item")},
                       {YesNo("p.is_inactive", "is_inactive")},
                       COALESCE(p.tax_rate, 0) AS tax_rate,
                       CASE WHEN COALESCE(p.is_stock_tracked, 1) IN (1, '1') THEN 'Yes' ELSE 'No' END AS is_stock_tracked,
                       {YesNo("p.sell_by_weight", "sell_by_weight")},
                       COALESCE(p.price2, 0) AS price2,
                       COALESCE(p.price3, 0) AS price3,
                       COALESCE(p.price4, 0) AS price4,
                       COALESCE(p.part_image, '') AS part_image,
                       COALESCE(p.status, 'Active') AS status
                FROM parts p
                LEFT JOIN categories c ON p.category_id = c.id
                LEFT JOIN suppliers s ON p.supplier_id = s.id
                {statusFilter}
                ORDER BY c.category_name, p.part_name"),
                "selling_price", "purchase_price", "price2", "price3", "price4");
        }

        public static DataTable BuildCustomersExportTable()
        {
            return MarkCurrency(DatabaseHelper.ExecuteDataTable(@"
                SELECT full_name AS CustomerName, phone AS Phone, email AS Email, address AS Address,
                       COALESCE(type, 'Regular') AS CustomerType, COALESCE(current_balance, 0) AS Balance,
                       COALESCE(credit_limit, 1000) AS CreditLimit,
                       payment_due_date AS PaymentDueDate, COALESCE(reminder_days, 0) AS ReminderDays
                FROM customers
                WHERE date_deleted IS NULL
                ORDER BY full_name"),
                "Balance", "CreditLimit");
        }

        public static DataTable BuildSuppliersExportTable()
        {
            return MarkCurrency(DatabaseHelper.ExecuteDataTable(@"
                SELECT supplier_name AS SupplierName, contact_person AS ContactPerson,
                       phone AS Phone, email AS Email, address AS Address,
                       COALESCE(type, 'Regular') AS Type, COALESCE(balance_due, 0) AS Balance,
                       payment_due_date AS PaymentDueDate, COALESCE(reminder_days, 0) AS ReminderDays,
                       '' AS City, '' AS PostalCode, '' AS Website, '' AS Notes
                FROM suppliers
                WHERE date_deleted IS NULL
                ORDER BY supplier_name"),
                "Balance");
        }

        public static DataTable BuildExpensesExportTable()
        {
            return MarkCurrency(DatabaseHelper.ExecuteDataTable($@"
                SELECT expense_date AS date, category, amount, description,
                       {YesNo("is_paid", "paid")},
                       {YesNo("is_recurring", "recurring")}
                FROM expenses
                WHERE date_deleted IS NULL AND category != 'System'
                ORDER BY expense_date DESC"),
                "amount");
        }

        /// <summary>
        /// Completed sales orders. Column names match SaleImportRow so an export can be re-imported.
        /// </summary>
        public static DataTable BuildSalesExportTable()
        {
            return MarkCurrency(DatabaseHelper.ExecuteDataTable(@"
                SELECT o.order_id AS orderId,
                       o.order_date AS date,
                       COALESCE(c.full_name, 'Walk-in') AS customer,
                       COALESCE(o.total_amount, 0) AS total,
                       COALESCE(o.amount_paid, 0) AS amountPaid,
                       COALESCE(o.payment_status, '') AS payment,
                       COALESCE(o.payment_method, '') AS paymentMethod,
                       COALESCE(o.status, '') AS status,
                       (SELECT COALESCE(SUM(oi.quantity), 0) FROM order_items oi
                         WHERE oi.order_id = o.order_id) AS itemCount
                FROM orders o
                LEFT JOIN customers c ON o.customer_id = c.customer_id
                WHERE o.status IS NOT NULL AND o.status NOT IN ('Draft', 'Quotation')
                ORDER BY o.order_date DESC, o.order_id DESC"),
                "total", "amountPaid");
        }

        /// <summary>
        /// Activity log. Column names match HistoryImportRow so an export can be re-imported.
        /// </summary>
        public static DataTable BuildHistoryExportTable()
        {
            return DatabaseHelper.ExecuteDataTable(@"
                SELECT timestamp AS date,
                       COALESCE(action_type, '') AS action,
                       COALESCE(part_name, '') AS item,
                       COALESCE(description, '') AS details,
                       COALESCE(username, '') AS user
                FROM transactions
                ORDER BY timestamp DESC");
        }

        /// <summary>
        /// Report summary as a two-column metric/value sheet.
        /// </summary>
        public static DataTable BuildReportSummaryExportTable(SalesReportSummary summary)
        {
            var dt = new DataTable("Summary");
            dt.Columns.Add("metric", typeof(string));
            dt.Columns.Add("value", typeof(string));
            if (summary == null) return dt;

            string Money(decimal v) =>
                v.ToString(ImportExportHelper.CurrencyFormat, CultureInfo.InvariantCulture);

            dt.Rows.Add("Date From", summary.FromDate.ToString("yyyy-MM-dd"));
            dt.Rows.Add("Date To", summary.ToDate.ToString("yyyy-MM-dd"));
            dt.Rows.Add("Total Sales", Money(summary.TotalSales));
            dt.Rows.Add("Total Cost", Money(summary.TotalCost));
            dt.Rows.Add("Total Expenses", Money(summary.TotalExpenses));
            dt.Rows.Add("Profit Before Expenses", Money(summary.TotalProfit));
            dt.Rows.Add("Profit After Expenses", Money(summary.TotalProfitAfterExpenses));
            return dt;
        }

        /// <summary>
        /// Sold-products detail for the given range, with export-friendly column names.
        /// </summary>
        public static DataTable BuildReportProductsExportTable(DateTime fromDate, DateTime toDate)
        {
            var dt = new ReportService().GetSoldProductsDetail(fromDate, toDate);
            RenameColumn(dt, "product_name", "Product");
            RenameColumn(dt, "quantity_sold", "Quantity Sold");
            RenameColumn(dt, "unit_price", "Unit Price");
            RenameColumn(dt, "total_sales", "Total Sales");
            RenameColumn(dt, "total_cost", "Total Cost");
            RenameColumn(dt, "profit", "Profit");
            return MarkCurrency(dt, "Unit Price", "Total Sales", "Total Cost", "Profit");
        }

        private static void RenameColumn(DataTable dt, string from, string to)
        {
            if (dt != null && dt.Columns.Contains(from))
                dt.Columns[from].ColumnName = to;
        }

        #endregion
    }
}
