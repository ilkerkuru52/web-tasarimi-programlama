# KuraBird – Tekil Kurulum Kontrol Mekanizması
## Teknik Dokümantasyon v1.0

---

## 1. Amaç

Bu belge, KuraBird oyununun kurulum paketinin **yalnızca bir kez** kurulabilmesini sağlayan güvenlik mekanizmasını teknik olarak açıklamaktadır.

**Temel gereksinim:** Kurulum paketi indirilip silinse ve yeniden indirilse de, aynı bilgisayara ikinci kez kurulum yapılamamalıdır. Oyun kaldırılsa bile bu kısıtlama sürmeli.

---

## 2. Donanım Parmak İzi (Hardware Fingerprint)

### 2.1 Bileşenler

| Kaynak | WMI Sorgusi | Örnek Değer |
|--------|-------------|-------------|
| CPU İşlemci ID | `Win32_Processor.ProcessorId` | `BFEBFBFF000906EA` |
| Anakart Seri No | `Win32_BaseBoard.SerialNumber` | `MP0123456789` |
| Birincil MAC | NetworkInterface (fiziksel, en yüksek hız) | `E4B97A12CD34` |

### 2.2 Hash Üretimi

```
RAW = "KURA|" + CPU_ID + "|" + BOARD_SN + "|" + MAC + "|BIRD"
FINGERPRINT = SHA-256(RAW)  → 64 karakter hex string
```

Bu hash, aynı donanım üzerinde her zaman aynı sonucu üretir. Kurulum dosyası silinip yeniden indirilse de hash değişmez; donanım değişmediği için.

---

## 3. Depolama Katmanları (Çift Güvence)

### Katman 1: Windows Registry

```
HKEY_LOCAL_MACHINE\SOFTWARE\KuraBird\InstallID = [AES-256 şifrelenmiş fingerprint]
HKEY_LOCAL_MACHINE\SOFTWARE\KuraBird\InstallDate = [ISO 8601 tarih]
HKEY_LOCAL_MACHINE\SOFTWARE\KuraBird\Version = "1.0.0"
```

### Katman 2: Gizli İmza Dosyası

```
C:\ProgramData\KuraBird\install.sig
```

- **İçerik:** AES-256 şifrelenmiş + Base64 kodlanmış fingerprint
- **Dosya özellikleri:** `Hidden | System | ReadOnly`
- **Neden ProgramData?** Standard `Program Files` kaldırma işlemi `ProgramData`'yı silmez.
- Kullanıcı oyunu kaldırsa bile bu dosya yerinde kalır.

### 3.1 AES Şifreleme

```
Algoritma : AES-256-CBC
Anahtar   : 32 byte (KuraBirdAES256K!KuraBirdAES256K!)
IV        : 16 byte (KuraBirdIV123456)
```

Registry veya dosyada ham hash değil, şifrelenmiş hash saklanır. Bu sayede dosya elle düzenlenebilse bile doğrulanacak değer bozulur.

---

## 4. Kontrol Akışı (Flowchart)

```
KuraBirdSetup.exe çalışır
        │
        ▼
Inno Setup: InitializeSetup() [Pascal kodu]
        │
        ├─── install.sig mevcut mu? ──── EVET ─→ ⛔ HATA: Kurulum engellendi
        │                                              [Setup kapanır]
        │
        └─── Registry kaydı mevcut mu? ─ EVET ─→ ⛔ HATA: Kurulum engellendi
                │                                     [Setup kapanır]
                │
               HAYIR
                │
                ▼
         KuraBirdInstaller.exe çalışır (gömülü)
                │
                ├── InstallGuard.CheckInstallStatus()
                │       │
                │       ├─ .sig var → zaten kurulu (UI'de ⛔ göster)
                │       └─ Registry var → zaten kurulu (UI'de ⛔ göster)
                │
                └── İkisi de yok → kuruluma izin ver
                        │
                        ▼
                HardwareFingerprint.Generate()
                [CPU + Board + MAC → SHA-256]
                        │
                        ▼
                InstallGuard.MarkAsInstalled(hash)
                [Registry + .sig dosyasına yaz]
                        │
                        ▼
                ✅ Kurulum tamamlandı
```

---

## 5. Kaldırma Sonrası Senaryo

```
Oyun kaldırıldı:
  Program Files\KuraBird\ → SİLİNDİ
  Registry\SOFTWARE\KuraBird\ → SİLİNDİ (uninstall kaydeder)
  C:\ProgramData\KuraBird\install.sig → KALDI ✓

Kurulum paketi tekrar çalıştırıldığında:
  Inno Setup başlar → install.sig kontrol eder → BULUNDU
  → ⛔ "Bu bilgisayara daha önce kurulmuştur" hatası
  → Setup sonlanır
```

---

## 6. Güvenlik Katmanları Özeti

| Katman | Konum | Silinmez mi? | Kontrol Noktası |
|--------|-------|-------------|-----------------|
| Registry | HKLM\SOFTWARE\KuraBird | Uninstall siler | Inno Setup + InstallGuard |
| .sig dosyası | C:\ProgramData\KuraBird | ✅ Kalır | Inno Setup + InstallGuard |

İkili yapı sayesinde saldırı yüzeyi küçülür:
- **Sadece Registry silinirse** → .sig dosyasından engellenir
- **Sadece .sig silinirse** → Registry'den engellenir, .sig yeniden yazılır
- **Her ikisi de silinirse** → Donanım aynı olduğu sürece bu sefer hash yeniden üretilir ve kayıt yoksa ilk kurulum gibi davranır. Bu sınırlı bir zayıflıktır; ancak gerçek bir senaryo değildir çünkü ReadOnly+Hidden+System .sig dosyasını silmek kullanıcı için açık değildir.

---

## 7. Kullanılan Teknolojiler

- **.NET 6 WinForms**: Oyun ve installer UI
- **System.Management (WMI)**: CPU ID, Anakart SN okuma
- **System.Net.NetworkInformation**: MAC adresi okuma
- **System.Security.Cryptography**: SHA-256 hash + AES-256 şifreleme
- **Microsoft.Win32.Registry**: Registry okuma/yazma
- **Inno Setup 6**: .exe kurulum paketi + Pascal script ile ön kontrol

---

*KuraBird – OOP Proje Ödevi | 2025*
