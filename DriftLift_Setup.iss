#define MyAppName      "Drift Lift"
#define MyAppVersion   "1.0.6"
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
AppComments=Gamepad drift correction, button remapping, and calibration tool for Windows.
AppCopyright=Copyright (C) 2024 arshiatxd

DefaultDirName={autopf}\DriftLift
DefaultGroupName={#MyAppName}
DisableDirPage=no
DirExistsWarning=no
DisableProgramGroupPage=yes
UsePreviousAppDir=no
AllowNoIcons=yes

OutputBaseFilename=DriftLift_Setup
OutputDir=C:\Users\Parsian\Desktop
Compression=lzma2/ultra64
SolidCompression=yes
InternalCompressLevel=ultra64

WizardStyle=modern
SetupIconFile={#IconFile}
UninstallDisplayIcon={app}\{#MyAppExeName}
WizardImageFile=C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\WizImage.bmp
WizardSmallImageFile=C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\WizSmallImage.bmp
WizardSizePercent=100

PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible

MinVersion=10.0
VersionInfoVersion={#MyAppVersion}
VersionInfoDescription={#MyAppName} Setup
VersionInfoProductName={#MyAppName}
VersionInfoCopyright=Copyright (C) 2024 arshiatxd

UninstallDisplayName={#MyAppName}
CreateUninstallRegKey=yes
CloseApplications=yes
CloseApplicationsFilter=*{#MyAppExeName}
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
WelcomeLabel1=Welcome to the [name] Setup Wizard
WelcomeLabel2=This will install [name/ver] on your computer.%n%nDrift Lift fixes analog stick drift, provides button remapping, and calibration tools for PlayStation and Xbox controllers.%n%nIt is recommended that you close all other applications before continuing.
FinishedHeadingLabel=Setup Complete
FinishedLabel=Drift Lift has been installed on your computer.%n%nLaunch the app and connect your controller via USB or Bluetooth to get started.

[Tasks]
Name: "desktopicon";  Description: "Create a &desktop shortcut";        GroupDescription: "Additional Shortcuts:"; Flags: unchecked
Name: "startupicon";  Description: "Launch Drift Lift on &Windows startup"; GroupDescription: "Startup Options:";     Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb"

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; Comment: "Open Drift Lift controller calibration tool"
Name: "{autodesktop}\{#MyAppName}";  Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; Comment: "Open Drift Lift controller calibration tool"; Tasks: desktopicon

[Registry]
Root: HKLM; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: startupicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName} now"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "taskkill.exe"; Parameters: "/f /im {#MyAppExeName}"; Flags: runhidden; RunOnceId: "KillApp"

[Code]
function IsDotNet10Installed(): Boolean;
var
  ResultCode: Integer;
begin
  Result := False;
  if Exec('dotnet', '--list-runtimes', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Result := True;
  end;
end;

function InitializeSetup(): Boolean;
begin
  Result := True;

  if not IsDotNet10Installed() then
  begin
    if MsgBox(
      'Drift Lift requires the .NET 10.0 Runtime to run.' + #13#10 + #13#10 +
      'Please download and install .NET 10.0 from:' + #13#10 +
      'https://dotnet.microsoft.com/download/dotnet/10.0' + #13#10 + #13#10 +
      'Continue installing anyway?',
      mbConfirmation, MB_YESNO) = IDNO then
    begin
      Result := False;
    end;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Clean up old crash logs from previous installs
  end;
end;
