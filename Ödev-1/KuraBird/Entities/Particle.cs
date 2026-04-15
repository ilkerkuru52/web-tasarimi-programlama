using KuraBird.Core;

namespace KuraBird.Entities
{
    /// <summary>
    /// Kalıtım (Inheritance): GameObject'ten türetilmiş partikül efekti sınıfı.
    /// </summary>
    public class Particle : GameObject
    {
        private float _lifetime;
        private float _maxLifetime;
        private Color _color;
        private float _size;
        private float _rotationSpeed;

        public Particle(PointF position, PointF velocity, Color color, float lifetime, float size = 6f)
            : base(position, new SizeF(size, size))
        {
            Velocity = velocity;
            _color = color;
            _lifetime = lifetime;
            _maxLifetime = lifetime;
            _size = size;
            _rotationSpeed = (float)(new Random().NextDouble() * 400 - 200);
        }

        public override void Update(float deltaTime)
        {
            _lifetime -= deltaTime;
            IsActive = _lifetime > 0;
            // Yerçekimi etkisi
            Velocity = new PointF(Velocity.X * 0.98f, Velocity.Y + 500f * deltaTime);
            Position = new PointF(Position.X + Velocity.X * deltaTime,
                                  Position.Y + Velocity.Y * deltaTime);
            Rotation += _rotationSpeed * deltaTime;
            _size = Size.Width * (_lifetime / _maxLifetime);
        }

        public override void Render(Graphics g)
        {
            if (!IsActive || _size < 0.5f) return;
            float alpha = (_lifetime / _maxLifetime);
            int a = (int)(alpha * 220);
            if (a <= 0) return;

            using var br = new SolidBrush(Color.FromArgb(a, _color));
            float half = _size / 2f;
            g.FillEllipse(br, Position.X - half, Position.Y - half, _size, _size);
        }

        public static IEnumerable<Particle> CreateExplosion(PointF center, Color color, int count = 18)
        {
            var rng = new Random();
            var list = new List<Particle>();
            for (int i = 0; i < count; i++)
            {
                double angle = rng.NextDouble() * Math.PI * 2;
                float speed = (float)(rng.NextDouble() * 260 + 80);
                var vel = new PointF((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed);
                float lt = (float)(rng.NextDouble() * 0.6 + 0.3);
                float sz = (float)(rng.NextDouble() * 10 + 4);
                list.Add(new Particle(center, vel, color, lt, sz));
            }
            return list;
        }
    }
}
