# Branding a new app from Generic

Use **Generic Inventory** as the master template. Copy the repo, then change only the items below. All other code should stay identical so fixes merge cleanly.

## 1. Copy the repo

```powershell
robocopy "C:\path\to\Generic-Inventory-Management-System-Web-Based" "C:\path\to\MyClientInventory" /MIR /XD .git bin obj dist
```

Or clone Generic and rename the folder. Start a fresh git repo in the new folder.

## 2. Rename project / exe

| File | Change |
|------|--------|
| `GenericInventorySystem.csproj` | Rename to `MyClientInventorySystem.csproj` |
| `GenericInventorySystem.slnx` | Rename + update project path inside |
| Inside `.csproj` | `<AssemblyName>` and output exe name |

## 3. User data folder (one place)

`Helpers\DatabaseConfig.cs` — change the `UserDataRoot` folder name:

```csharp
"GenericInventory"  →  "MyClientInventory"
```

Logs, backups, and the SQLite DB all follow this path automatically.

## 3b. Unique HTTP port (required if apps run on the same PC)

Each branded exe hosts a local web server. **Ports must differ** or the second app will open the first app's UI.

Set these in `appsettings.json` → `SystemBranding`:

| App | `BrandId` | `HttpPort` | `HttpsPort` |
|-----|-----------|------------|-------------|
| Generic | `Generic` | 5000 | 5001 |
| Otargi | `Otargi` | 5010 | 5011 |
| Panache | `Panache` | 5020 | 5021 |
| a2z Tech | `A2Z` | 5030 | 5031 |
| New client | unique name | unused port (e.g. 5040) | +1 |

Do not reuse 5000/5010/5020/5030 for a new fork.

## 4. Theme and company name

| File | What to set |
|------|-------------|
| `appsettings.json` | `SystemBranding.CompanyName`, `AppName`, `BrandId`, `HttpPort`, `HttpsPort`, optional `CompanyInfo` |
| `Helpers/ThemeConfig.cs` | `PrimaryColor`, gradients, header colors (blue by default in Generic) |
| `Forms/WebServerHostForm.cs` | Splash title, logo path, status messages |
| `Assets/logo.png` | Client logo (optional; Generic uses text) |
| `Assets/icon.ico` | Window / taskbar / setup icon — run `installer/make-icon.ps1` after updating logo |

## 5. Web UI

| File | What to set |
|------|-------------|
| `wwwroot/index.html` | `<title>`, logo `<img>`, cache-bust `?v=` on `app.js` / `style.css` |
| `wwwroot/css/style.css` | CSS variables `--primary`, `--primary-rgb`, etc. |
| `wwwroot/manifest.json` | `name`, `short_name` |
| `wwwroot/app.js` | Top comment + `I18N.en` / `I18N.ar` strings: `about_app_name`, `about_blurb`, `scan_to_connect_hint`, fallback company name |

Search for `Generic` in `wwwroot/` after editing.

## 6. Quotation / receipt contact line

| File | What to set |
|------|-------------|
| `Properties/Resources.resx` | `QuotePreview_CompanyInfo` (address, phone) |
| `Properties/Resources_ar.resx` | Arabic version of the same |

The web quotation API reads this via `LocalizationManager.GetString("QuotePreview_CompanyInfo")`.

## 7. Installer

1. Copy `installer/Generic.iss` → `installer/MyClient.iss`
2. Update `AppName`, `AppExeName`, `DefaultDirName`, output `Setup.exe` name
3. Update `installer/build.ps1` and `publish.bat` to point at the new `.iss` and `.csproj`

## 8. Docs and IDE

- `README.md` — client name, exe path, `%LocalAppData%` folder
- `.vscode/tasks.json` and `launch.json` — project and dll names

## 9. Verify

```powershell
dotnet build -c Release
.\publish.bat
```

Smoke test: login, dashboard QR, add product (list updates immediately), quotation header shows company info, backups land under `%LocalAppData%\MyClientInventory\Backups`.

## Reference: Otargi

The **Otargi** repo is a fully branded fork (green theme, logo, Lebanon contact info). Diff these files against Generic when in doubt:

- `OtargiInventorySystem.csproj`
- `Helpers/DatabaseConfig.cs` (`OtargiInventory`)
- `Helpers/ThemeConfig.cs` (green `#10B981`)
- `installer/Otargi.iss`
- `Properties/Resources.resx` (`QuotePreview_CompanyInfo`)

Do **not** copy Otargi-only assets back into Generic unless you are updating the template for everyone.
