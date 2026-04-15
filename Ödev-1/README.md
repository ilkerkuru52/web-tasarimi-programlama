# KuraBird 🐦
### OOP Tabanlı Flappy Bird Oyunu

---

## Kurulum Hakkında Önemli Bilgi

Bu kurulum paketi **her bilgisayara yalnızca bir kez** kurulabilir.

### Nasıl Çalışır?

Kurulum sırasında bilgisayarın donanım parmak izi hesaplanır:
- İşlemci (CPU) kimliği
- Anakart seri numarası
- Birincil ağ adaptörünün MAC adresi

Bu üç değer SHA-256 ile birleştirilip şifrelenerek iki yere kaydedilir:
1. `HKEY_LOCAL_MACHINE\SOFTWARE\KuraBird` (Windows Registry)
2. `C:\ProgramData\KuraBird\install.sig` (Gizli imza dosyası)

### Kural Şu:
| Senaryo | Sonuç |
|---------|-------|
| A kişisi kendi bilgisayarına kurar | ✅ Kurulur, A'nın hash'i kaydedilir |
| A kişisi oyunu kaldırıp tekrar kurmaya çalışır | ⛔ Engellenir |
| B kişisi (örn. hoca) kendi bilgisayarına kurar | ✅ Kurulur, farklı donanım = farklı hash |
| B kişisi oyunu kaldırıp tekrar kurmaya çalışır | ⛔ Engellenir |

### Önemli Notlar
- Mekanizma kişiye değil, **donanıma** bağlıdır.
- Oyun kaldırılsa bile `install.sig` dosyası `C:\ProgramData\KuraBird\` altında kalır.
- Registry kaydı silinse bile `.sig` dosyası koruyu sürdürür (çift güvence).

---

## Oyun Özellikleri
- 4 farklı kuş karakteri
- Paralaks arka plan (3 katman)
- Power-up sistemi (Kalkan & Yavaşlatma)
- Partikül efektleri ve ekran sarsıntısı
- Yüksek skor tablosu (JSON kayıt)
- Ses efektleri ve müzik

## OOP Yapısı
- **Soyutlama:** `IRenderable`, `IUpdatable`, `ICollidable`
- **Kalıtım:** `Bird`, `Pipe`, `Particle` → `GameObject`
- **Çok Biçimlilik:** `ShieldPowerUp`, `SlowPowerUp` → `PowerUp.ApplyEffect()`
- **Kapsülleme:** `AudioManager`, `ScoreManager` (Singleton)

---

*Geliştirici: İlker KURU | 2026*
