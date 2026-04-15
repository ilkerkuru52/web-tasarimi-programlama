using KuraBird.Core;
using KuraBird.Managers;

namespace KuraBird.Entities
{
    /// <summary>
    /// Kalıtım (Inheritance): GameObject'ten türetilmiş boru engeli sınıfı.
    /// </summary>
    public class Pipe : GameObject
    {
        public const float PipeWidth = 80f;
        public const float Gap = 190f;    // Kuşun geçeceği boşluk

        private bool _isTop;             // Üst boru mu?
        private float _speed;
        private bool _passed;
        private Color _pipeColor;
        private Color _pipeBorderColor;

        public bool IsPassed
        {
            get => _passed;
            set => _passed = value;
        }

        public bool IsTop => _isTop;

        public Pipe(float x, float gapCenterY, bool isTop, float speed)
            : base(new PointF(x, 0), new SizeF(PipeWidth, 1200f))
        {
            _isTop = isTop;
            _speed = speed;

            // Renkler: zorluk arttıkça kırmızımsı
            float hue = Math.Clamp(1f - (speed - 200f) / 300f, 0, 1);
            _pipeColor = ColorFromHSV(120 * hue, 0.6, 0.5);
            _pipeBorderColor = ColorFromHSV(120 * hue, 0.8, 0.3);

            if (isTop)
                Position = new PointF(x, gapCenterY - Gap / 2 - 1200f);
            else
                Position = new PointF(x, gapCenterY + Gap / 2);
        }

        public override void Update(float deltaTime)
        {
            Position = new PointF(Position.X - _speed * deltaTime, Position.Y);
            IsActive = Position.X + PipeWidth > -20;
        }

        public override void Render(Graphics g)
        {
            float x = Position.X;
            float y = Position.Y;
            float w = Size.Width;
            float h = Size.Height;

            // Boru gövdesi gradyan
            using (var br = new LinearGradientBrush(
                new RectangleF(x, y, w, h),
                _pipeBorderColor, _pipeColor,
                System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
            {
                g.FillRectangle(br, x, y, w, h);
            }

            // Kenar çizgileri
            using var pen = new Pen(_pipeBorderColor, 2f);
            g.DrawRectangle(pen, x, y, w, h);

            // Highlight şeridi
            using var highlightBr = new SolidBrush(Color.FromArgb(50, Color.White));
            g.FillRectangle(highlightBr, x + 4, y, w * 0.25f, h);

            // Şapka (kapak)
            float capH = 28f;
            float capX = x - 8;
            float capY = _isTop ? y + h - capH : y;
            using (var capBr = new LinearGradientBrush(
                new RectangleF(capX, capY, w + 16, capH),
                Color.FromArgb(200, _pipeColor), _pipeBorderColor,
                System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
            {
                g.FillRectangle(capBr, capX, capY, w + 16, capH);
            }
            using var capPen = new Pen(_pipeBorderColor, 2f);
            g.DrawRectangle(capPen, capX, capY, w + 16 - 1, capH - 1);
        }

        public override RectangleF GetBounds()
        {
            float x = Position.X;
            float y = Position.Y;
            float w = Size.Width;
            if (_isTop)
                return new RectangleF(x, y, w, Size.Height);
            else
                return new RectangleF(x, y, w, Size.Height);
        }

        public bool IsOffScreen() => Position.X + Size.Width < -10;

        private static Color ColorFromHSV(double h, double s, double v)
        {
            int hi = (int)(h / 60) % 6;
            double f = h / 60 - Math.Floor(h / 60);
            double p = v * (1 - s); double q = v * (1 - f * s); double t = v * (1 - (1 - f) * s);
            (double r, double g2, double b) = hi switch
            {
                0 => (v, t, p), 1 => (q, v, p), 2 => (p, v, t),
                3 => (p, q, v), 4 => (t, p, v), _ => (v, p, q)
            };
            return Color.FromArgb((int)(r * 255), (int)(g2 * 255), (int)(b * 255));
        }
    }
}
