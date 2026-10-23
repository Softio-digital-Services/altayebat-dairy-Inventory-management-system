# Al-Tayebat Inventory - Release + Installer build
# Usage: powershell -ExecutionPolicy Bypass -File .\installer\build.ps1

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $root "dist\app"
$distDir = Join-Path $root "dist"
$iss = Join-Path $PSScriptRoot "AlTayebat.iss"
$csproj = Join-Path $root "AlTayebatInventorySystem.csproj"

# Find a dotnet.exe that actually has an SDK installed (PATH may point to a runtime-only install).
$dotnetCandidates = @(
    (Join-Path $env:LocalAppData "Microsoft\dotnet\dotnet.exe"),
    (Join-Path $env:USERPROFILE ".dotnet\dotnet.exe"),
    (Join-Path $env:ProgramFiles "dotnet\dotnet.exe"),
    (Join-Path ${env:ProgramFiles(x86)} "dotnet\dotnet.exe")
)
$pathDotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($pathDotnet) { $dotnetCandidates = @($pathDotnet.Source) + $dotnetCandidates }
$dotnet = $null
foreach ($c in $dotnetCandidates) {
    if (-not $c -or -not (Test-Path $c)) { continue }
    try { $sdks = & $c --list-sdks 2>$null } catch { continue }
    if ($LASTEXITCODE -eq 0 -and ($sdks | Where-Object { $_ -match '^\d+\.' })) { $dotnet = $c; break }
}
if (-not $dotnet) { throw "No .NET SDK found. Install the .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0" }
Write-Host "Using .NET SDK: $dotnet" -ForegroundColor DarkGray

# A running copy from dist\ locks its files and makes publish/copy fail -- close it first.
$running = Get-Process -ErrorAction SilentlyContinue | Where-Object {
    $_.Path -and $_.Path.StartsWith($distDir, [System.StringComparison]::OrdinalIgnoreCase)
}
if ($running) {
    Write-Host "==> Closing running app from dist\ so it can be replaced..." -ForegroundColor Yellow
    foreach ($p in $running) {
        Write-Host "    $($p.Path) (PID $($p.Id))" -ForegroundColor DarkGray
        $null = $p.CloseMainWindow()
    }
    $running | ForEach-Object { $_.WaitForExit(10000) | Out-Null }
    $running | Where-Object { -not $_.HasExited } | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1
}

Write-Host "==> Publishing self-contained Release (win-x64)..." -ForegroundColor Cyan
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

& $dotnet build-server shutdown | Out-Null
& $dotnet publish $csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:UseSharedCompilation=false -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

Write-Host "==> Publishing portable single-file exe..." -ForegroundColor Cyan
$singleDir = Join-Path $root "dist\_single"
$portableExe = Join-Path $distDir "AlTayebatInventory.exe"
if (Test-Path $singleDir) { Remove-Item $singleDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $distDir | Out-Null
New-Item -ItemType Directory -Force -Path $singleDir | Out-Null
& $dotnet build-server shutdown | Out-Null
Start-Sleep -Seconds 2
& $dotnet publish $csproj -c Release -r win-x64 --self-contained true `
    -p:UseSharedCompilation=false `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:IncludeAllContentForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o $singleDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish (single-file) failed" }
$publishedExe = Join-Path $singleDir "AlTayebatInventorySystem.exe"
if (-not (Test-Path $publishedExe)) { throw "Single-file exe not found: $publishedExe" }
Copy-Item $publishedExe $portableExe -Force
Remove-Item $singleDir -Recurse -Force
Write-Host "Portable app: $portableExe" -ForegroundColor Green

# Offline WebView2 runtime installer, bundled into setup so it installs on laptops without internet.
# Downloaded once from Microsoft and kept in installer\redist (ignored by git: ~180 MB).
$wv2 = Join-Path $PSScriptRoot "redist\MicrosoftEdgeWebView2RuntimeInstallerX64.exe"
if (-not (Test-Path $wv2)) {
    Write-Host "==> Downloading offline WebView2 runtime installer (one time)..." -ForegroundColor Cyan
    New-Item -ItemType Directory -Force -Path (Split-Path $wv2) | Out-Null
    & curl.exe -sSL --fail -o "$wv2.part" "https://go.microsoft.com/fwlink/?linkid=2124701"
    $sig = if (Test-Path "$wv2.part") { Get-AuthenticodeSignature "$wv2.part" } else { $null }
    if ($LASTEXITCODE -eq 0 -and $sig -and $sig.Status -eq 'Valid' -and $sig.SignerCertificate.Subject -like '*Microsoft*') {
        Move-Item "$wv2.part" $wv2 -Force
    } else {
        Remove-Item "$wv2.part" -ErrorAction SilentlyContinue
        Write-Host "WARNING: could not get the offline WebView2 installer. Setup will download WebView2 online instead (only on PCs that lack it)." -ForegroundColor Yellow
    }
}
if (Test-Path $wv2) { Write-Host "Offline WebView2 runtime will be bundled into setup." -ForegroundColor DarkGray }

Write-Host "==> Looking for Inno Setup 6.4+ (ISCC)..." -ForegroundColor Cyan
$pf86 = ${env:ProgramFiles(x86)}
$pf = $env:ProgramFiles
$lad = $env:LocalAppData
$isccCandidates = @(
    (Join-Path $pf86 "Inno Setup 6\ISCC.exe"),
    (Join-Path $pf "Inno Setup 6\ISCC.exe"),
    (Join-Path $lad "Programs\Inno Setup 6\ISCC.exe"),
    (Join-Path $pf86 "Inno Setup 7\ISCC.exe"),
    (Join-Path $pf "Inno Setup 7\ISCC.exe")
)
$iscc = $isccCandidates | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1

New-Item -ItemType Directory -Force -Path $distDir | Out-Null

if ($iscc) {
    Write-Host "==> Building Setup.exe with $iscc" -ForegroundColor Cyan
    Write-Host "    (Setup will auto-install WebView2 on PCs that need it)" -ForegroundColor DarkGray
    & $iscc $iss
    if ($LASTEXITCODE -ne 0) { throw "ISCC failed" }
    $setup = Get-ChildItem $distDir -Filter "AlTayebatSetup.exe" | Select-Object -First 1
    if ($setup) {
        Write-Host ""
        Write-Host "SUCCESS - give buyers this file:" -ForegroundColor Green
        Write-Host $setup.FullName -ForegroundColor Green
        Write-Host ""
        Write-Host "Target PC needs: Windows 10/11 64-bit. WebView2 is installed by setup if missing." -ForegroundColor Cyan
    }
}
else {
    $zip = Join-Path $distDir "AlTayebatInventory-Portable.zip"
    if (Test-Path $zip) { Remove-Item $zip -Force }
    Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zip
    Write-Host ""
    Write-Host "Inno Setup not available - created portable zip instead:" -ForegroundColor Yellow
    Write-Host $zip
}

Write-Host ""
Write-Host "Published app folder: $publishDir"
Write-Host "Portable exe: $(Join-Path $distDir 'AlTayebatInventory.exe')"
Write-Host "Run locally: $(Join-Path $publishDir 'AlTayebatInventorySystem.exe')"
