; ============================================================
; VanBanPlus Installer Script - Inno Setup 6
; Professional Windows Installer
; ============================================================

#define MyAppName "VanBanPlus"
#define MyAppVersion "1.0.6"
#define MyAppPublisher "VanBanPlus Software"
#define MyAppURL "https://vanbanplus.vn"
#define MyAppExeName "AIVanBan.Desktop.exe"
#define MyAppDescription "Phan mem quan ly van ban thong minh"
#define PublishDir "D:\AIVanBanCaNhan\AIVanBan.Desktop\bin\Release\net9.0-windows\win-x64\publish"

[Setup]
AppId={{8A7B9C3D-4E5F-6A1B-2C3D-4E5F6A7B8C9D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=
InfoBeforeFile=
InfoAfterFile=
OutputDir=D:\AIVanBanCaNhan\Installer
OutputBaseFilename=VanBanPlus-Setup-{#MyAppVersion}-x64
SetupIconFile=D:\AIVanBanCaNhan\AIVanBan.Desktop\Assets\app.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
WizardSizePercent=120
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
VersionInfoVersion={#MyAppVersion}.0
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppDescription}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}
DisableWelcomePage=no
DisableDirPage=no
DisableProgramGroupPage=yes
ShowLanguageDialog=auto
WizardImageAlphaFormat=defined

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
english.WelcomeLabel2=This will install [name/ver] on your computer.%n%nVanBanPlus - Smart Document Management System for Government Agencies.%n%nIt is recommended that you close all other applications before continuing.
english.FinishedHeadingLabel=VanBanPlus Installation Complete
english.FinishedLabel=VanBanPlus has been successfully installed on your computer.%n%nClick Finish to close Setup.

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "quicklaunchicon"; Description: "Create a &Quick Launch icon"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startup"; Description: "Start VanBanPlus when Windows starts"; GroupDescription: "Other options:"; Flags: unchecked

[Files]
; Main application files
Source: "{#PublishDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishDir}\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishDir}\*.pdb"; DestDir: "{app}"; Flags: ignoreversion; Excludes: "*.Core.pdb"

; Localization folders (if any satellite assemblies)
Source: "{#PublishDir}\cs\*"; DestDir: "{app}\cs"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist
Source: "{#PublishDir}\de\*"; DestDir: "{app}\de"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist
Source: "{#PublishDir}\es\*"; DestDir: "{app}\es"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist
Source: "{#PublishDir}\fr\*"; DestDir: "{app}\fr"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist
Source: "{#PublishDir}\it\*"; DestDir: "{app}\it"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist
Source: "{#PublishDir}\ja\*"; DestDir: "{app}\ja"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist
Source: "{#PublishDir}\ko\*"; DestDir: "{app}\ko"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist
Source: "{#PublishDir}\pl\*"; DestDir: "{app}\pl"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist
Source: "{#PublishDir}\pt-BR\*"; DestDir: "{app}\pt-BR"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist
Source: "{#PublishDir}\ru\*"; DestDir: "{app}\ru"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist
Source: "{#PublishDir}\tr\*"; DestDir: "{app}\tr"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist
Source: "{#PublishDir}\zh-Hans\*"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist
Source: "{#PublishDir}\zh-Hant\*"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist

[Icons]
; Start Menu shortcuts
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "{#MyAppDescription}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"

; Desktop shortcut
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Comment: "{#MyAppDescription}"

; Quick Launch
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Registry]
; Auto startup
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: startup

; App registration
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "taskkill"; Parameters: "/F /IM {#MyAppExeName}"; Flags: runhidden; RunOnceId: "KillApp"

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
Type: dirifempty; Name: "{autopf}\{#MyAppName}"

[Code]
// Check if app is running before install/uninstall
function IsAppRunning(): Boolean;
var
  ResultCode: Integer;
begin
  Exec('tasklist', '/FI "IMAGENAME eq {#MyAppExeName}" /NH', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := (ResultCode = 0);
end;

function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
begin
  Result := True;
  
  // Kill running instance
  Exec('taskkill', '/F /IM {#MyAppExeName}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Sleep(500);
end;

function InitializeUninstall(): Boolean;
var
  ResultCode: Integer;
begin
  Result := True;
  
  // Kill running instance before uninstall
  Exec('taskkill', '/F /IM {#MyAppExeName}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Sleep(500);
end;
