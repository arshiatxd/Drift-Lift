[Setup]
AppName=Drift Lift
AppVersion=1.0.5
AppPublisher=DriftTeam
AppCopyright=© 2026 DriftTeam. All rights reserved.
DefaultDirName={autopf}\DriftLift
DefaultGroupName=Drift Lift
UninstallDisplayIcon={app}\DriftliftApp.exe
Compression=lzma2/ultra64
SolidCompression=yes
OutputDir=C:\Users\Parsian\Desktop
OutputBaseFilename=DriftLift_Setup
SetupIconFile=C:\Users\Parsian\Desktop\prj\312321\DriftLift\icon.ico
DisableProgramGroupPage=yes
DisableDirPage=no
PrivilegesRequired=admin

[Files]
Source: "C:\Users\Parsian\Desktop\prj\312321\net10. released\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb,crash.log"

[Icons]
Name: "{autoprograms}\Drift Lift"; Filename: "{app}\DriftliftApp.exe"; IconFilename: "{app}\DriftliftApp.exe"
Name: "{autodesktop}\Drift Lift"; Filename: "{app}\DriftliftApp.exe"; IconFilename: "{app}\DriftliftApp.exe"; Tasks: desktopicon

[InstallDelete]
Type: filesandordirs; Name: "{userappdata}\DriftLock"

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\DriftLock"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Run]
Filename: "{app}\DriftliftApp.exe"; Description: "{cm:LaunchProgram,Drift Lift}"; Flags: nowait postinstall skipifsilent
