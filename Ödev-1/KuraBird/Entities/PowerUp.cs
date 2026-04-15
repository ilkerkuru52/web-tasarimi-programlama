using KuraBird.Core;
using KuraBird.Entities;

namespace KuraBird.Entities
{
    /// <summary>
    /// Soyutlama + Çok Biçimlilik (Polymorphism): Tüm power-up'ların temel abstract sınıfı.
    /// Alt sınıflar ApplyEffect metodunu kendi davranışlarına göre override eder.
    /// </summary>
    public abstract class PowerUp : GameObject
    {
        protected float _lifetime;
        protected float _bobTimer;
        protected Color _color;
        protected string _label;

        public bool IsCollected { get; protected set; }

        protected PowerUp(PointF position, Color color, string label)
            : base(position, new SizeF(36, 36))
        {
            _color = color;
            _label = label;
            _lifetime = 8f;
        }

        // Çok biçimlilik: her power-up kendi etkisini uygular
        public abstract void ApplyEffect(Bird bird);

        public override void Update(float deltaTime)
        {
            _lifetime -= deltaTime;
            _bobTimer += deltaTime * 3f;
            // Hafif sallanma hareketi
            Position = new PointF(Position.X - 180f * deltaTime,
                                  Position.Y + (float)Math.Sin(_bobTimer) * 0.8f);
            IsActive = _lifetime > 0 && !IsCollected;
        }

        public override void Render(Graphics g)
        {
            if (!IsActive) return;
            float x = Position.X;
            float y = Position.Y;
            float r = Size.Width / 2;

            // Dış parıltı
            float pulse = 0.6f + 0.4f * (float)Math.Sin(_bobTimer * 2);
            using var glowBr = new SolidBrush(Color.FromArgb((int)(60 * pulse), _color));
            g.FillEllipse(glowBr, x - 8, y - 8, Size.Width + 16, Size.Height + 16);

            // Ana daire – PathGradientBrush ile radyal gradyan
            var circPath = new System.Drawing.Drawing2D.GraphicsPath();
            circPath.AddEllipse(x, y, Size.Width, Size.Height);
            using var pgBrush = new System.Drawing.Drawing2D.PathGradientBrush(circPath);
            pgBrush.CenterPoint = new PointF(x + r - 4, y + r - 4);
            pgBrush.CenterColor = Color.White;
            pgBrush.SurroundColors = new[] { _color };
            g.FillEllipse(pgBrush, x, y, Size.Width, Size.Height);

            // Kenar
            using var pen = new Pen(Color.FromArgb(180, _color.R, _color.G, _color.B), 2f);
            g.DrawEllipse(pen, x, y, Size.Width, Size.Height);

            // Etiket
            using var font = new Font("Segoe UI", 8f, FontStyle.Bold);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(_label, font, Brushes.White,
                new RectangleF(x, y, Size.Width, Size.Height), sf);
        }

        public override void OnCollision() { IsCollected = true; IsActive = false; }
    }

    /// <summary>
    /// Çok Biçimlilik (Polymorphism): Kalkan power-up'ı - ApplyEffect override.
    /// </summary>
    public class ShieldPowerUp : PowerUp
    {
        public ShieldPowerUp(PointF position)
            : base(position, Color.FromArgb(80, 160, 255), "🛡") { }

        public override void ApplyEffect(Bird bird)
        {
            bird.ApplyShield(6f);
            AudioManager.Instance.Play("powerup");
        }
    }

    /// <summary>
    /// Çok Biçimlilik (Polymorphism): Yavaşlatma power-up'ı - ApplyEffect override.
    /// </summary>
    public class SlowPowerUp : PowerUp
    {
        public SlowPowerUp(PointF position)
            : base(position, Color.FromArgb(80, 220, 80), "⏱") { }

        public override void ApplyEffect(Bird bird)
        {
            bird.ApplySlow(5f);
            AudioManager.Instance.Play("powerup");
        }
    }
}


