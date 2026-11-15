; Installer für PhoMiniPrint (F-12, NFR-006).
; Wird von tools/installer_bauen.ps1 aufgerufen, das Version und Quellordner übergibt:
;   ISCC /DVersion=1.0.0 /DQuelle=<publish-Ordner> installer\PhoMiniPrint.iss

#ifndef Version
  #error Version fehlt: bitte tools/installer_bauen.ps1 verwenden.
#endif
#ifndef Quelle
  #error Quelle fehlt: bitte tools/installer_bauen.ps1 verwenden.
#endif

[Setup]
; Feste AppId: neue Versionen ersetzen die alte, Einstellungen in %APPDATA% bleiben erhalten.
AppId={{6B1E3C2A-9F4D-4E8B-A7C5-2D0F8E1B4A93}
AppName=PhoMiniPrint
AppVersion={#Version}
AppVerName=PhoMiniPrint {#Version}
AppPublisher=wikischwemler-ops
AppPublisherURL=https://github.com/wikischwemler-ops/PhoMiniPrint
AppSupportURL=https://github.com/wikischwemler-ops/PhoMiniPrint/issues
VersionInfoVersion={#Version}
VersionInfoProductVersion={#Version}
VersionInfoDescription=PhoMiniPrint Setup
DefaultDirName={autopf}\PhoMiniPrint
DefaultGroupName=PhoMiniPrint
DisableProgramGroupPage=yes
; Ohne Administratorrechte: Installation nur für den aktuellen Benutzer anbieten.
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
SetupIconFile=..\src\PhoMiniPrint.App\PhoMiniPrint.ico
UninstallDisplayIcon={app}\PhoMiniPrint.exe
UninstallDisplayName=PhoMiniPrint {#Version}
LicenseFile=..\NOTICE
OutputDir=..\artefakte
OutputBaseFilename=PhoMiniPrint-Setup-{#Version}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes

[Languages]
Name: "deutsch"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#Quelle}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\PhoMiniPrint"; Filename: "{app}\PhoMiniPrint.exe"; Comment: "Exakt drucken auf dem Phomemo T02"
Name: "{autodesktop}\PhoMiniPrint"; Filename: "{app}\PhoMiniPrint.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\PhoMiniPrint.exe"; Description: "{cm:LaunchProgram,PhoMiniPrint}"; Flags: nowait postinstall skipifsilent
