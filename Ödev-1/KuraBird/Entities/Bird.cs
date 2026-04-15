using KuraBird.Core;
using KuraBird.Managers;

namespace KuraBird.Entities
{
    /// <summary>
    /// Kalıtım (Inheritance): GameObject'ten türetilmiş kuş sınıfı.
    /// Kapsülleme (Encapsulation): Kuşa ait tüm özellikler private/property ile korunmuştur.
    /// </summary>
    public class Bird : GameObject
    {
        private const float Gravity = 1400f;
        private const float FlapForce = -480f;
        private const float MaxFallSpeed = 700f;
        private const float MaxRiseSpeed = -480f;

        private float _flapAnimTimer;
        private int _flapFrame;
        private bool _isDead;
        private bool _hasShield;
        private float _shieldTimer;
        private bool _isSlowed;
        private float _slowTimer;
        private int _characterIndex;
        private float _deathTimer;
        private float _wingAngle;
        private bool _wingGoingUp = true;

        // Kuş renk temaları (4 farklı karakter)
        private static readonly Color[][] CharacterColors = new[]
        {
            new[] { Color.FromArgb(255, 220, 50),  Color.FromArgb(200, 100, 20), Color.White },  // Sarı
            new[] { Color.FromArgb(80,  180, 255), Color.FromArgb(20,  80, 200), Color.White },  // Mavi
            new[] { Color.FromArgb(80,  220, 80),  Color.FromArgb(20,  140, 30), Color.White },  // Yeşil
            new[] { Color.FromArgb(255, 100, 100), Color.FromArgb(180, 20,  20), Color.White },  // Kırmızı
        };

        public bool IsDead => _isDead;
        public bool HasShield => _hasShield;
        public bool IsSlowed => _isSlowed;
        public int CharacterIndex
        {
            get => _characterIndex;
            set => _characterIndex = Math.Clamp(value, 0, CharacterColors.Length - 1);
        }

        public Bird(PointF position, int characterIndex = 0)
            : base(position, new SizeF(52, 40))
        {
            CharacterIndex = characterIndex;
        }

        public void Flap()
        {
            if (_isDead) return;
            float force = _isSlowed ? FlapForce * 0.7f : FlapForce;
            Velocity = new PointF(Velocity.X, force);
            AudioManager.Instance.Play("flap");
        }

        public void ApplyShield(float duration = 5f)
        {
            _hasShield = true;
            _shieldTimer = duration;
        }

        public void ApplySlow(float duration = 5f)
        {
            _isSlowed = true;
            _slowTimer = duration;
        }

        public override void Update(float deltaTime)
        {
            if (_isDead)
            {
                _deathTimer += deltaTime;
                float vy = Velocity.Y + Gravity * deltaTime;
                vy = Math.Min(vy, MaxFallSpeed * 2);
                Velocity = new PointF(Velocity.X, vy);
                Position = new PointF(Position.X, Position.Y + Velocity.Y * deltaTime);
                Rotation = 90f;
                return;
            }

            // Yerçekimi uygula
            float gravity = _isSlowed ? Gravity * 0.6f : Gravity;
            float newVy = Velocity.Y + gravity * deltaTime;
            newVy = Math.Clamp(newVy, MaxRiseSpeed, MaxFallSpeed);
            Velocity = new PointF(Velocity.X, newVy);
            Position = new PointF(Position.X, Position.Y + Velocity.Y * deltaTime);

            // Rotasyon: düşüşe göre eğilme
            float targetRot = Velocity.Y / MaxFallSpeed * 90f;
            Rotation += (targetRot - Rotation) * 10f * deltaTime;

            // Kanat animasyonu
            _flapAnimTimer += deltaTime;
            if (_flapAnimTimer > 0.08f)
            {
                _flapFrame = (_flapFrame + 1) % 3;
                _flapAnimTimer = 0;
            }

            // Kanat sallanma açısı animasyonu
            if (_wingGoingUp) { _wingAngle += 300f * deltaTime; if (_wingAngle >= 30f) _wingGoingUp = false; }
            else { _wingAngle -= 300f * deltaTime; if (_wingAngle <= -10f) _wingGoingUp = true; }

            // Kalkan süresi
            if (_hasShield) { _shieldTimer -= deltaTime; if (_shieldTimer <= 0) _hasShield = false; }
            if (_isSlowed) { _slowTimer -= deltaTime; if (_slowTimer <= 0) _isSlowed = false; }
        }

        public override void Render(Graphics g)
        {
            var colors = CharacterColors[_characterIndex];
            Color body = colors[0];
            Color belly = colors[1];
            Color eye = colors[2];

            DrawRotated(g, gfx =>
            {
                float w = Size.Width;
                float h = Size.Height;

                // Kalkan efekti
                if (_hasShield)
                {
                    float pulse = (float)(0.5 + 0.5 * Math.Sin(DateTime.Now.Ticks / 1e6));
                    Color shieldColor = Color.FromArgb((int)(80 + 80 * pulse), 100, 180, 255);
                    using var sb = new SolidBrush(shieldColor);
                    gfx.FillEllipse(sb, -8, -8, w + 16, h + 16);
                }

                // Yavaşlama efekti
                if (_isSlowed)
                {
                    using var sb = new SolidBrush(Color.FromArgb(60, 180, 255, 100));
                    gfx.FillEllipse(sb, -4, -4, w + 8, h + 8);
                }

                // Kanatlı gövde
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    // Alt kanat
                    var wingTransform = gfx.Transform.Clone();
                    float wingOffY = (float)(Math.Sin(_wingAngle * Math.PI / 180.0) * 10);
                    using var wingBrush = new SolidBrush(Color.FromArgb(220, body));
                    gfx.FillEllipse(wingBrush, w * 0.15f, h * 0.3f + wingOffY, w * 0.55f, h * 0.45f);

                    // Ana gövde (oval)
                    using var bodyBrush = new LinearGradientBrush(
                        new RectangleF(0, 0, w, h),
                        body, Color.FromArgb(200, body.R, body.G, body.B),
                        System.Drawing.Drawing2D.LinearGradientMode.Vertical);
                    gfx.FillEllipse(bodyBrush, 0, 0, w, h);

                    // Karın kısmı
                    using var bellyBrush = new SolidBrush(Color.FromArgb(180, belly));
                    gfx.FillEllipse(bellyBrush, w * 0.55f, h * 0.35f, w * 0.36f, h * 0.4f);

                    // Göz beyazı
                    gfx.FillEllipse(Brushes.White, w * 0.55f, h * 0.08f, w * 0.32f, h * 0.38f);
                    // Göz bebeği
                    gfx.FillEllipse(Brushes.Black, w * 0.64f, h * 0.16f, w * 0.15f, h * 0.22f);
                    // Göz parlaması
                    gfx.FillEllipse(Brushes.White, w * 0.68f, h * 0.18f, w * 0.05f, h * 0.07f);

                    // Gaga
                    PointF[] beak = {
                        new PointF(w * 0.86f, h * 0.35f),
                        new PointF(w * 1.05f, h * 0.45f),
                        new PointF(w * 0.86f, h * 0.55f)
                    };
                    using var beakBrush = new SolidBrush(Color.FromArgb(255, 200, 50));
                    gfx.FillPolygon(beakBrush, beak);
                }
            });
        }

        public override void OnCollision()
        {
            if (_hasShield) { _hasShield = false; return; }
            _isDead = true;
            AudioManager.Instance.Play("hit");
        }

        public bool IsOffScreen(int screenHeight) =>
            Position.Y > screenHeight + 50 || Position.Y < -100;
    }
}
