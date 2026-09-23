#define MyAppName "SnapImg"
#define MyAppVersion "0.9.2"
#define MyAppPublisher "SnapImg"
#define MyAppExeName "snapimg.exe"

[Setup]
AppId={{B1BCE4F2-1C74-4B9D-9AEF-0F1B0D0A7C96}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\SnapImg
DefaultGroupName={#MyAppName}
OutputDir=InstallerOutput
OutputBaseFilename=SnapImg-Setup-{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
SetupIconFile=SnapImg.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
PrivilegesRequired=admin

[Languages]
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "デスクトップにショートカットを作成"; GroupDescription: "追加のショートカット:"; Flags: unchecked

[Files]
Source: "publish-folder\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\SnapImg"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\SnapImg"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "SnapImg を起動"; Flags: nowait postinstall skipifsilent
