# Al-Tayebat (الطيبات) Inventory Management System

## 📋 Overview

Complete inventory management system with POS, customer management, supplier tracking, and comprehensive reporting.

## 🚀 Quick Start

### Run published web app (WebView2)

```text
dist\app\AlTayebatInventorySystem.exe
```

This opens the Al-Tayebat web UI (dashboard, inventory, POS, reports, …). Hardware scale / sell-by-weight can be enabled by Softio Super Admin in Settings.

Refresh `dist` after code changes:

```powershell
.\publish.bat
```

Or:

```powershell
powershell -ExecutionPolicy Bypass -File .\installer\build.ps1
```

### Prerequisites
- Windows 10+ (win-x64) and WebView2 Runtime
- .NET 8 SDK only needed to build from source (`dist\app` is self-contained)

### Running from source

```powershell
dotnet run -c Release --project AlTayebatInventorySystem.csproj
```

Or Debug:

```text
bin\Debug\net8.0-windows\win-x64\AlTayebatInventorySystem.exe
```

### First-Time Setup

1. **License Activation** (if prompted)
2. **Default Login** — Softio.Admin / Softio@2026!
3. **Database** — created automatically under `%LocalAppData%\AlTayebatInventory\Data\inventory.db` (no SQL Server needed)

## ✨ Features

### Core Modules
- **Dashboard** - Real-time analytics and KPIs
- **Inventory Management** - Parts tracking with low-stock alerts
- **Point of Sale (POS)** - Quick sales processing
- **Customer Management** - Customer profiles and purchase history
- **Supplier Management** - Supplier tracking and orders
- **Reports** - Sales, inventory, and financial reports
- **User Management** - Role-based access control

### Import/Export
- **CSV Import/Export** - Parts, Customers, Suppliers
- **Excel Compatible** - TSV format for Excel compatibility
- **Templates** - Sample templates in `Templates\` folder
- **Duplicate Detection** - Automatic skip of existing records
- **Auto-Create Categories** - Categories created during import

### Licensing System
- **Yearly Subscriptions** - 1-10 year licenses
- **30-Day Trial** - Full feature access
- **Hardware-Locked** - Prevents casual copying
- **Offline Activation** - No internet required

## 📁 Project Structure

```
altayebat-dairy-Inventory-management-system/
├── wwwroot/                 ← Web UI (served in WebView2)
├── Forms/                   ← Host window + supporting WinForms
├── Helpers/                 ← DB, print, license, theme
├── Services/                ← Business logic
├── Plugins/                 ← Optional menu plugins
├── Data/                    ← C# data models (not the SQLite file)
├── Database/
│   └── pos_cert.pfx         ← Local HTTPS cert for the embedded API
├── Assets/                  ← Icons / branding
├── Templates/               ← CSV import templates
├── installer/               ← Inno Setup script + build.ps1
├── publish.bat              ← Publish app + AlTayebatSetup.exe
└── dist/                    ← Publish output (app + setup)
```

## 🔧 Configuration

### Database
SQLite file lives in `%LocalAppData%\AlTayebatInventory\Data\inventory.db` so the app can save data even when installed under Program Files.
Schema updates run automatically at startup. An old `Data\inventory.db` next to the exe is copied once on first launch after upgrade.

### Theme Customization
Edit `Helpers\ThemeConfig.cs`:
```csharp
public static Color PrimaryColor = Color.FromArgb(59, 130, 246);
public static Color AccentColor = Color.FromArgb(16, 185, 129);
```

## 📊 Import/Export Guide

### Exporting Data

1. Navigate to Parts/Customers/Suppliers tab
2. Click **📤 Export** button
3. Choose **Export to CSV** or **Export to Excel**
4. Select save location
5. File is created with timestamp

### Importing Data

1. Prepare CSV file with required columns:
   - **Parts**: PartNumber, PartName, Category, Quantity, MinimumStock, UnitPrice, Location, Status
   - **Customers**: CustomerName, Email, Phone, Address, City, PostalCode, Notes
   - **Suppliers**: SupplierName, ContactPerson, Email, Phone, Address, City, PostalCode, Website, Notes

2. Click **📥 Import** button
3. Choose **Import from CSV** or **Import from Excel**
4. Select file
5. Review import summary (imported/skipped counts)

**Sample CSV (Parts):**
```csv
PartNumber,PartName,Category,Quantity,MinimumStock,UnitPrice,Location,Status
P001,Brake Pad Set,Brakes,50,10,45.99,A1-Shelf2,Active
P002,Oil Filter,Filters,100,20,8.99,B2-Shelf1,Active
```

## 🔑 License Management

### Viewing License Info
1. Click user avatar (top-right)
2. Select "📄 License Info"
3. View license type, expiration, and status

### Renewing License
1. Obtain new license key from administrator
2. Click "License Info"
3. Click "Renew License"
4. Enter new key

### Trial Period
- 30 days full access
- All features unlocked
- Upgrade to full license anytime

## 🗄️ Database Management

Backups and the live database use `%LocalAppData%\AlTayebatInventory\`:

- **Database:** `Data\inventory.db`
- **Backups:** `Backups\` (also available in the web UI Backup tab)
- **Logs:** `Logs\`

Use the in-app **Backup & Restore** plugin or copy `inventory.db` while the app is closed.

## 🎨 Branding another client app

See **[BRANDING.md](BRANDING.md)** for the full checklist (rename exe, colors, logo, installer, quotation info). The Otargi repo is the reference branded fork.

## 🐛 Troubleshooting

### "Database connection failed"
- Ensure SQL Server LocalDB is installed
- Check database file exists: `bin\Debug\Data\carparts.mdf`
- Run `Database\SetupDatabase.bat`

### "License activation failed"
- Verify license key is copied correctly (no spaces)
- Check hardware hasn't changed significantly
- Try "Start 30-Day Trial" option

### "Import failed"
- Verify CSV format matches template
- Check for required columns (PartNumber, PartName)
- Ensure no duplicate part numbers

### Application won't start
- Check error logs: `bin\Debug\Logs\error_log_YYYYMMDD.txt`
- Verify .NET Framework 4.7.2+ is installed
- Run as Administrator if needed

## 📝 Development

### Building
```powershell
dotnet build CarPartsInventorySystem.csproj
```

### Running Tests
```powershell
# Test database connection
.\Database\CheckSchema.ps1

# Test data integrity
.\Database\CheckConstraints.ps1
```

### Adding New Features
1. Create feature branch
2. Add forms in `Forms\`
3. Add business logic in `Services\`
4. Update `MainForm.cs` navigation
5. Test thoroughly
6. Build and verify

## 📚 Documentation

- **User Guide**: See `README.md` (this file)
- **Database Schema**: `Database\CreateCarPartsDatabase.sql`
- **Import Templates**: `Templates\Parts_Import_Template.csv`
- **Error Logs**: `bin\Debug\Logs\`

## 🔐 Security

- Passwords hashed with SHA256
- License data encrypted (AES)
- Hardware-locked licenses
- Role-based access control
- Audit logging for critical operations

## 📞 Support

For issues or questions:
1. Check error logs: `bin\Debug\Logs\`
2. Review troubleshooting section above
3. Contact system administrator

## 📄 License

This software requires a valid license key for commercial use.
- Trial: 30 days
- Yearly: 1-10 years
- Contact administrator for license keys

---

**Version:** 2.0  
**Last Updated:** February 2026  
**Framework:** .NET Framework 4.7.2
