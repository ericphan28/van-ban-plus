; Script tao installer cho VanBanPlus
; Huong dan: Tai Inno Setup tai https://jrsoftware.org/isdl.php

#define MyAppName "VanBanPlus"
#define MyAppVersion "1.0.5"
#define MyAppPublisher "VanBanPlus Software"
#define MyAppExeName "AIVanBan.Desktop.exe"

[Setup]
; Thong tin co ban
AppId={{8A7B9C3D-4E5F-6A1B-2C3D-4E5F6A7B8C9D}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\VanBanPlus
DefaultGroupName={#MyAppName}
OutputDir=D:\AIVanBanCaNhan\Installer
OutputBaseFilename=VanBanPlus-Setup-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

; Yeu cau quyen admin
PrivilegesRequired=admin

; Icon va hinh anh
; SetupIconFile=D:\AIVanBanCaNhan\image\icon.ico
; WizardImageFile=D:\AIVanBanCaNhan\image\setup-banner.bmp

; Ho tro Windows 10/11
MinVersion=10.0

[Languages]
Name: "vietnamese"; MessagesFile: "compiler:Languages\Vietnamese.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Tao shortcut tren Desktop"; GroupDescription: "Shortcuts:"

[Files]
; Publish single file - chi can file exe chinh
Source: "D:\AIVanBanCaNhan\AIVanBan.Desktop\bin\Release\net9.0-windows\win-x64\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

; Neu can copy tat ca files (uncomment dong nay va comment dong tren)
; Source: "D:\AIVanBanCaNhan\AIVanBan.Desktop\bin\Release\net9.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Chay {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
