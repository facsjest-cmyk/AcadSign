#define MyAppName "AcadSign"
#define MyAppExeName "AcadSign.Desktop.exe"

#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif

[Setup]
AppId={{F7A2C4F7-EC55-4B2D-BA4F-8D6C3A44E1E7}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
PrivilegesRequired=lowest
DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
SetupIconFile=..\..\AcadSign.Desktop\Assets\AcadSign.ico
UninstallDisplayIcon={app}\AcadSign.ico
OutputDir=output
OutputBaseFilename={#MyAppName}-Setup
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "externals\*"; DestDir: "{tmp}"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist deleteafterinstall
Source: "..\..\AcadSign.Desktop\Assets\AcadSign.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\AcadSign.ico"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; IconFilename: "{app}\AcadSign.ico"

[Run]
Filename: "{tmp}\MicrosoftEdgeWebview2Setup.exe"; Parameters: "/silent /install"; StatusMsg: "Installing WebView2 Runtime..."; Flags: waituntilterminated skipifsilent; Check: FileExists(ExpandConstant('{tmp}\\MicrosoftEdgeWebview2Setup.exe'))
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
