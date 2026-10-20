# Import/Export Templates

This folder contains CSV templates for importing data into the Car Parts Inventory System.

## 📋 Available Templates

### 1. Parts_Import_Template.csv
Import parts/inventory items into the system.

**Columns:**
- `PartNumber` - Unique identifier (required)
- `PartName` - Name of the part (required)
- `Category` - Category name (auto-created if doesn't exist)
- `Quantity` - Stock quantity
- `MinimumStock` - Low stock threshold
- `UnitPrice` - Selling price
- `Location` - Warehouse location (e.g., A1-Shelf2)
- `Status` - Active/Inactive

**Example:**
```csv
PartNumber,PartName,Category,Quantity,MinimumStock,UnitPrice,Location,Status
P001,Brake Pad Set,Brakes,50,10,45.99,A1-Shelf2,Active
```

### 2. Customers_Import_Template.csv
Import customer records.

**Columns:**
- `CustomerName` - Full name or company name (required)
- `Email` - Email address
- `Phone` - Contact number
- `Address` - Street address
- `City` - City name
- `PostalCode` - ZIP/Postal code
- `Notes` - Additional information

**Example:**
```csv
CustomerName,Email,Phone,Address,City,PostalCode,Notes
John Smith,john.smith@email.com,555-0101,123 Main St,Springfield,12345,Regular customer
```

### 3. Suppliers_Import_Template.csv
Import supplier information.

**Columns:**
- `SupplierName` - Company name (required)
- `ContactPerson` - Primary contact
- `Email` - Email address
- `Phone` - Contact number
- `Address` - Street address
- `City` - City name
- `PostalCode` - ZIP/Postal code
- `Website` - Company website
- `Notes` - Additional information

**Example:**
```csv
SupplierName,ContactPerson,Email,Phone,Address,City,PostalCode,Website,Notes
AutoParts Inc,David Lee,sales@autoparts.com,555-0201,100 Commerce Dr,City,10001,www.autoparts.com,Primary supplier
```

## 📥 How to Use Templates

### Step 1: Download Template
1. Open the template file for the data you want to import
2. Save a copy with a new name (e.g., `my_parts_import.csv`)

### Step 2: Fill in Your Data
1. Open the file in Excel or a text editor
2. Keep the header row (first line) unchanged
3. Add your data rows below the header
4. Save the file

### Step 3: Import into Application
1. Open the Car Parts Inventory System
2. Navigate to the appropriate tab (Parts/Customers/Suppliers)
3. Click the **📥 Import** button
4. Select **Import from CSV**
5. Choose your filled template file
6. Review the import summary

## ⚠️ Important Notes

### Data Validation
- **Required fields** must not be empty
- **Duplicate detection**: Existing records (by PartNumber/Name) will be skipped
- **Categories**: For parts, categories are auto-created if they don't exist
- **Format**: Save as CSV (Comma-Separated Values)

### Best Practices
- ✅ Test with a small file first (5-10 records)
- ✅ Keep a backup of your original data
- ✅ Use consistent formatting (dates, numbers)
- ✅ Avoid special characters in IDs
- ❌ Don't modify the header row
- ❌ Don't include empty rows

### Excel Compatibility
The application also supports Excel-compatible TSV (Tab-Separated Values) format:
- Use the **Import from Excel** option
- File will be read as tab-delimited
- Works with `.xls` and `.txt` files

## 🔄 Export Format

When you export data, the CSV files will have the same format as these templates, making it easy to:
- Edit exported data
- Re-import modified data
- Share data between systems
- Create backups

## 📞 Support

If you encounter issues with importing:
1. Verify CSV format matches template
2. Check for required fields (PartNumber, PartName, etc.)
3. Ensure no duplicate IDs in your file
4. Review error logs: `bin\Debug\Logs\error_log_YYYYMMDD.txt`

---

**Template Version:** 1.0  
**Last Updated:** February 2026
