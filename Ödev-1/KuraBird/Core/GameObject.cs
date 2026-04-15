using KuraBird.Core.Abstractions;

namespace KuraBird.Core
{
    /// <summary>
    /// Kalıtım (Inheritance) ve Soyutlama (Abstraction): Tüm oyun nesnelerinin temel sınıfı.
    /// IRenderable, IUpdatable ve ICollidable arayüzlerini uygular.
    /// </summary>
    public abstract class GameObject : IRenderable, IUpdatable, ICollidable
    {
        // Kapsülleme (Encapsulation): Özellikler property ile korunmaktadır.
        private PointF _position;
        private PointF _velocity;
        private bool _isActive;
        private float _rotation;

        public PointF Position
        {
            get => _position;
            set => _position = value;
        }

        public PointF Velocity
        {
            get => _velocity;
            set => _velocity = value;
        }

        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }

        public float Rotation
        {
            get => _rotation;
            set => _rotation = Math.Clamp(value, -90f, 90f);
        }

        public SizeF Size { get; set; }
        public Color Tint { get; set; } = Color.White;

        protected GameObject(PointF position, SizeF size)
        {
            _position = position;
            _velocity = PointF.Empty;
            _isActive = true;
            _rotation = 0f;
            Size = size;
        }

        // Soyut metotlar - alt sınıflar kendi davranışlarını tanımlar
        public abstract void Render(Graphics g);
        public abstract void Update(float deltaTime);

        public virtual RectangleF GetBounds()
        {
            float margin = Size.Width * 0.1f;
            return new RectangleF(
                _position.X + margin,
                _position.Y + margin,
                Size.Width - margin * 2,
                Size.Height - margin * 2);
        }

        public virtual void OnCollision() { }

        protected void DrawRotated(Graphics g, Action<Graphics> drawAction)
        {
            var state = g.Save();
            g.TranslateTransform(
                _position.X + Size.Width / 2f,
                _position.Y + Size.Height / 2f);
            g.RotateTransform(_rotation);
            g.TranslateTransform(
                -Size.Width / 2f,
                -Size.Height / 2f);
            drawAction(g);
            g.Restore(state);
        }
    }
}
