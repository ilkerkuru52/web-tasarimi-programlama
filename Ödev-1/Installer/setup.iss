; =============================================
; KuraBird - Inno Setup Script
; KuraBirdSetup.exe üretir
; =============================================

#define MyAppName "KuraBird"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "KuraBird Studio"
#define MyAppExeName "KuraBird.exe"
#define InstallerExeName "KuraBirdInstaller.exe"

[Setup]
AppId={{B4F7A1C2-3D8E-4F2A-9B1C-A5E7D3F2C8B9}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=no
OutputDir=.
OutputBaseFilename=KuraBirdSetup
SetupIconFile=KuraBird.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\{#MyAppExeName}
; Uninstall'da install.sig SİLİNMEZ → tekil kurulum korunur
UninstallFilesDir={app}

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "desktopicon"; Description: "Masaüstü kısayolu oluştur"; GroupDescription: "Ek görevler:"; Flags: unchecked

[Files]
; Ana oyun
Source: "publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
; Oyunun tüm bağımlılıkları
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "{#InstallerExeName}"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\KuraBird Kaldır"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Kurulum bitmeden önce tekil kurulum kontrolünü çalıştır
Filename: "{app}\{#InstallerExeName}"; Description: "Kurulum ayarlarını uygula"; \
  Flags: runhidden waituntilterminated; StatusMsg: "Güvenlik kaydı oluşturuluyor..."

; Oyunu başlatma seçeneği
Filename: "{app}\{#MyAppExeName}"; Description: "KuraBird'i Şimdi Başlat"; \
  Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Oyun dosyaları kaldırılır ama install.sig KALIR
; C:\ProgramData\KuraBird klasörü silinmez — bu bilinçli bir karardır!
Type: filesandordirs; Name: "{app}"

[Code]
// Grafik arayüz başlamadan önce tekil kurulum kontrolü
function InitializeSetup(): Boolean;
var
  SigFile: String;
  RegValue: String;
begin
  SigFile := 'C:\ProgramData\KuraBird\install.sig';

  // .sig dosyası var mı?
  if FileExists(SigFile) then
  begin
    MsgBox(
      '⛔ Kurulum Engellenmiştir'#13#10#13#10 +
      'KuraBird bu bilgisayara daha önce kurulmuştur.'#13#10 +
      'Bu kurulum paketi yalnızca bir kez kullanılabilir.'#13#10#13#10 +
      'Oyun kaldırılmış olsa dahi yeniden kurulum yapılamaz.'#13#10 +
      'Bu güvenlik donanım parmak izi ile sağlanmaktadır.',
      mbError,
      MB_OK
    );
    Result := False;
    Exit;
  end;

  // Registry kontrolü
  if RegQueryStringValue(HKLM, 'SOFTWARE\KuraBird', 'InstallID', RegValue) then
  begin
    if RegValue <> '' then
    begin
      MsgBox(
        '⛔ Kurulum Engellenmiştir'#13#10#13#10 +
        'KuraBird bu bilgisayara zaten kurulmuş.'#13#10 +
        'Registry kaydı mevcut. Kurulum sonlandırılıyor.',
        mbError,
        MB_OK
      );
      Result := False;
      Exit;
    end;
  end;

  Result := True;
end;
