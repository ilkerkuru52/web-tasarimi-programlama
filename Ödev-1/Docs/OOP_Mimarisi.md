# KuraBird – OOP Mimarisi Dokümantasyonu

---

## 1. Sınıf Hiyerarşisi

```
IRenderable              IUpdatable              ICollidable
  │ Render(g)              │ Update(dt)             │ GetBounds()
  │                        │                        │ OnCollision()
  └──────────────┬─────────┘─────────────┬──────────┘
                 │                       │
          [abstract] GameObject ─────────┘
               / │ \  \
              /  │  \  \
           Bird Pipe Particle [abstract] PowerUp
                                           /   \
                                    ShieldPowerUp SlowPowerUp

Manager Sınıfları (Singleton + Kapsülleme):
  AudioManager   – ses çalma, WAV üretimi
  ScoreManager   – skor takip, JSON kayıt

Engine:
  GameEngine     – oyun döngüsü, state machine
```

---

## 2. OOP Prensipleri

### 2.1 Soyutlama (Abstraction)

Üç arayüz tanımlandı:

| Arayüz | Metot | Açıklama |
|--------|-------|----------|
| `IRenderable` | `Render(Graphics g)` | Her çizilebilir nesne bu metodu uygular |
| `IUpdatable` | `Update(float deltaTime)` | Her güncellenebilir nesne bu metodu uygular |
| `ICollidable` | `GetBounds()`, `OnCollision()` | Çarpışma algılama mantığı |

`GameObject` abstract sınıfı bu üç arayüzü birleştirir ve tüm oyun nesnelerinin ortak tabanını oluşturur.

### 2.2 Kalıtım (Inheritance)

```
GameObject (abstract)
├── Bird       — Kuş varlığı, fizik, animasyon
├── Pipe       — Boru engeli, renk gradyanı
├── Particle   — Partikül efekti, ölüm animasyonu
└── PowerUp (abstract)
    ├── ShieldPowerUp  — Kalkan efekti
    └── SlowPowerUp    — Yavaşlatma efekti
```

Her alt sınıf `Render()` ve `Update()` metodlarını kendi davranışına göre `override` eder.

### 2.3 Kapsülleme (Encapsulation)

**`GameObject`:**
```csharp
private PointF _position;
public PointF Position { get => _position; set => _position = value; }

private float _rotation;
public float Rotation { get => _rotation; set => _rotation = Math.Clamp(value, -90f, 90f); }
```

Rotation değeri setter'da kısıtlama uygulanarak güvenli tutulur.

**`AudioManager` (Singleton):**
```csharp
private static AudioManager? _instance;
public static AudioManager Instance => _instance ??= new AudioManager();
// private yapıcı → dışarıdan new AudioManager() yazılamaz
```

**`ScoreManager` (Singleton):**
Skor verisi `private`, dışarıya sadece `AddScore()`, `ResetScore()`, `SaveScore()` açık.

### 2.4 Çok Biçimlilik (Polymorphism)

**PowerUp hiyerarşisi:**
```csharp
// Abstract
public abstract void ApplyEffect(Bird bird);

// ShieldPowerUp override
public override void ApplyEffect(Bird bird) { bird.ApplyShield(6f); }

// SlowPowerUp override
public override void ApplyEffect(Bird bird) { bird.ApplySlow(5f); }
```

`GameEngine` şunu çağırdığında hangi power-up olduğunu bilmez:
```csharp
powerUp.ApplyEffect(_bird);  // Çok biçimlilik!
```

---

## 3. Tasarım Desenleri

| Desen | Kullanıldığı Yer | Amaç |
|-------|-----------------|------|
| Singleton | `AudioManager`, `ScoreManager` | Tek örnek garantisi |
| State Machine | `GameEngine.GameState` | Oyun durumu yönetimi |
| Observer | `OnDied`, `OnScoreChange` event'leri | Bileşenler arası bağlantı |
| Template Method | `GameObject.DrawRotated()` | Ortak render mantığı |

---

## 4. Bileşen Diyagramı

```
[MainMenuForm] ──────► [GameForm]
      │                    │
      │              [GameEngine]
      │               /    |    \
      ▼              ▼     ▼     ▼
[CharacterSelectForm] [Bird] [Pipe] [PowerUp]
[SettingsForm]          \     \      /
[HighScoreForm]          ▼     ▼    ▼
                       [AudioManager] [ScoreManager]
                       (Singleton)    (Singleton + JSON)
```

---

*KuraBird – OOP Proje Ödevi | 2025*
