; Al-Tayebat Inventory — Inno Setup installer
; Compile with: ISCC.exe AlTayebat.iss  (after publish)
; Requires Inno Setup 6.4+ for WebView2 download helper

#define MyAppName "Al-Tayebat Inventory"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "Al-Tayebat"
#define MyAppExeName "AlTayebatInventorySystem.exe"
#define PublishDir "..\dist\app"
; Evergreen Bootstrapper (needs internet) - only used if the offline installer below is not bundled
#define WebView2Url "https://go.microsoft.com/fwlink/p/?LinkId=2124703"
; Evergreen Standalone (offline) installer, bundled so setup works on laptops without internet.
; build.ps1 downloads it into the installer/redist folder (not committed to git: ~200 MB).
#define WebView2Offline AddBackslash(SourcePath) + "redist\MicrosoftEdgeWebView2RuntimeInstallerX64.exe"

[Setup]
AppId={{B81E2E01-GENR-4200-B001-GENERICINV}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Al-Tayebat Inventory
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=AlTayebatSetup
SetupIconFile=..\Assets\icon.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"; Flags: checkedonce

[Dirs]
; Non-admin users must write product images, language.txt, logs, etc.
Name: "{app}"; Permissions: users-modify
Name: "{app}\Data"; Permissions: users-modify
Name: "{app}\Assets"; Permissions: users-modify
Name: "{app}\Assets\Products"; Permissions: users-modify
Name: "{app}\Assets\Categories"; Permissions: users-modify
Name: "{app}\Logs"; Permissions: users-modify
Name: "{app}\Backups"; Permissions: users-modify
Name: "{app}\Plugins"; Permissions: users-modify
Name: "{app}\Templates"; Permissions: users-modify

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
#ifexist WebView2Offline
Source: "{#WebView2Offline}"; Flags: dontcopy
#endif

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Al-Tayebat Inventory"; Flags: nowait postinstall skipifsilent

[Code]
function IsWebView2RuntimeInstalled: Boolean;
var
  Ver: String;
begin
  { Evergreen WebView2 runtime client id }
  if RegQueryStringValue(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Ver) then
  begin
    Result := (Ver <> '') and (Ver <> '0.0.0.0');
    exit;
  end;
  if RegQueryStringValue(HKLM, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Ver) then
  begin
    Result := (Ver <> '') and (Ver <> '0.0.0.0');
    exit;
  end;
  if RegQueryStringValue(HKCU, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Ver) then
  begin
    Result := (Ver <> '') and (Ver <> '0.0.0.0');
    exit;
  end;
  Result := False;
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
  SetupPath: String;
begin
  Result := '';
  NeedsRestart := False;

  if IsWebView2RuntimeInstalled then
    exit;

#ifexist WebView2Offline
  { Offline: use the bundled standalone installer (no internet needed) }
  WizardForm.StatusLabel.Caption := 'Preparing Microsoft Edge WebView2 Runtime...';
  ExtractTemporaryFile('MicrosoftEdgeWebView2RuntimeInstallerX64.exe');
  SetupPath := ExpandConstant('{tmp}\MicrosoftEdgeWebView2RuntimeInstallerX64.exe');
#else
  WizardForm.StatusLabel.Caption := 'Downloading Microsoft Edge WebView2 Runtime...';
  try
    DownloadTemporaryFile('{#WebView2Url}', 'MicrosoftEdgeWebview2Setup.exe', '', nil);
  except
    Result := 'Could not download WebView2 Runtime. Install it manually from:'#13#10
      + 'https://developer.microsoft.com/microsoft-edge/webview2/'#13#10
      + 'Then run this setup again.';
    exit;
  end;
  SetupPath := ExpandConstant('{tmp}\MicrosoftEdgeWebview2Setup.exe');
#endif
  WizardForm.StatusLabel.Caption := 'Installing Microsoft Edge WebView2 Runtime...';
  if not Exec(SetupPath, '/silent /install', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
  begin
    Result := 'WebView2 Runtime installer failed to start. Install it manually, then re-run setup.';
    exit;
  end;

  if (ResultCode <> 0) and (not IsWebView2RuntimeInstalled) then
  begin
    Result := 'WebView2 Runtime install returned code '
      + IntToStr(ResultCode)
      + '. Install it manually from https://developer.microsoft.com/microsoft-edge/webview2/';
  end;
end;
