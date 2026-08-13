#define MyAppName      "Drift Lift"
#define MyAppVersion   "1.0.3"
#define MyAppPublisher "arshiatxd"
#define MyAppURL       "https://github.com/arshiatxd/Drift-Lift"
#define MyAppExeName   "DriftliftApp.exe"
#define SourceDir      "C:\Users\Parsian\Desktop\prj\312321\net10. released"
#define IconFile       "C:\Users\Parsian\Desktop\prj\312321\DriftLift\icon.ico"

[Setup]
AppId={{D37F20B6-7E1A-4D3B-98F1-4A5C10852F9E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases

DefaultDirName={autopf}\DriftLift
DisableDirPage=no
DirExistsWarning=no
DisableProgramGroupPage=yes
UsePreviousAppDir=no

OutputBaseFilename=DriftLift_Setup
OutputDir=C:\Users\Parsian\Desktop
Compression=lzma2/ultra64
SolidCompression=yes

WizardStyle=modern
SetupIconFile={#IconFile}
UninstallDisplayIcon={app}\{#MyAppExeName}
WizardImageFile=C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\WizImage.bmp
WizardSmallImageFile=C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\WizSmallImage.bmp

PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible

UninstallDisplayName={#MyAppName}
CreateUninstallRegKey=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon";    Description: "{cm:CreateDesktopIcon}";         GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startupicon";    Description: "Launch {#MyAppName} on startup"; GroupDescription: "Startup Options";     Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}";  Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKLM; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: startupicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "taskkill.exe"; Parameters: "/f /im {#MyAppExeName}"; Flags: runhidden; RunOnceId: "KillApp"

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
end;
