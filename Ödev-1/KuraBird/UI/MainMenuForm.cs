using KuraBird.Managers;

namespace KuraBird.UI
{
    public partial class MainMenuForm : Form
    {
        private System.Windows.Forms.Timer _animTimer;
        private float _birdY = 300f;
        private float _birdVY = 0f;
        private float _birdAngle = 0f;
        private float _titleGlow = 0f;
        private float _bgScroll = 0f;
        private int _selectedButton = 0;
        private float _time = 0f;
        private float _wingAngle = 0f;
        private bool _wingUp = true;

        private readonly string[] _menuItems = {
            "▶  OYNA", "🐦  KARAKTER SEÇ", "🏆  YÜKSEK SKORLAR",
            "⚙  AYARLAR", "✖  ÇIKIŞ"
        };

        public MainMenuForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Text = "KuraBird";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black;

            _animTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _animTimer.Tick += AnimTimer_Tick;
            _animTimer.Start();

            this.KeyDown += MainMenuForm_KeyDown;
            this.MouseMove += MainMenuForm_MouseMove;
            this.MouseClick += MainMenuForm_MouseClick;
            this.KeyPreview = true;

            AudioManager.Instance.PlayLoopingMusic("menu_music");
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(900, 600);
            this.Name = "MainMenuForm";
            this.ResumeLayout(false);
        }

        private void AnimTimer_Tick(object? sender, EventArgs e)
        {
            float dt = 0.016f;
            _time += dt;
            _bgScroll -= 25f * dt;
            if (_bgScroll < -900) _bgScroll = 0;

            // Demo kuş hareketi
            _birdVY += 600f * dt;
            _birdY += _birdVY * dt;
            if (_birdY > 400) { _birdY = 400; _birdVY = -280f; }
            _birdAngle = Math.Clamp(_birdVY / 500f * 45f, -30f, 45f);

            // Kanat
            if (_wingUp) { _wingAngle += 250f * dt; if (_wingAngle >= 25f) _wingUp = false; }
            else { _wingAngle -= 250f * dt; if (_wingAngle <= -8f) _wingUp = true; }

            _titleGlow = (float)(0.5 + 0.5 * Math.Sin(_time * 2.5));
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int w = ClientSize.Width, h = ClientSize.Height;

            // Gökyüzü
            using var skyBr = new LinearGradientBrush(
                new Rectangle(0, 0, w, h),
                Color.FromArgb(5, 10, 40), Color.FromArgb(20, 80, 160),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            g.FillRectangle(skyBr, 0, 0, w, h);

            DrawStars(g, w, h);
            DrawScrollingClouds(g, w, h);
            DrawGround(g, w, h);

            // Demo kuş
            DrawDemoBird(g);

            // Başlık
            DrawTitle(g, w);

            // Menü butonları
            DrawMenu(g, w, h);

            // Alt bilgi
            using var infoFont = new Font("Segoe UI", 9f);
            g.DrawString("© 2025 KuraBird  |  OOP Projesi  |  Space/Tıkla ile oyna",
                infoFont, Brushes.DimGray, new PointF(10, h - 20));
        }

        private void DrawStars(Graphics g, int w, int h)
        {
            var rng = new Random(77);
            for (int i = 0; i < 80; i++)
            {
                float x = (float)rng.NextDouble() * w;
                float y = (float)rng.NextDouble() * h * 0.55f;
                float bri = (float)(0.6 + 0.4 * Math.Sin(_time * (rng.NextDouble() * 3 + 1) + rng.NextDouble() * 6));
                using var sb = new SolidBrush(Color.FromArgb((int)(bri * 220), Color.White));
                g.FillEllipse(sb, x, y, 2f, 2f);
            }
        }

        private void DrawScrollingClouds(Graphics g, int w, int h)
        {
            var rng = new Random(22);
            float[] cx = { 100, 350, 600, 820 };
            float[] cy = { 80, 50, 100, 70 };
            float[] cw = { 120, 90, 140, 100 };
            for (int i = 0; i < 4; i++)
            {
                float x = ((cx[i] + _bgScroll) % (w + 200)) - 100;
                using var br = new SolidBrush(Color.FromArgb(35, Color.White));
                g.FillEllipse(br, x, cy[i], cw[i], cw[i] * 0.4f);
                g.FillEllipse(br, x + cw[i] * 0.25f, cy[i] - 12, cw[i] * 0.6f, cw[i] * 0.35f);
            }
        }

        private void DrawGround(Graphics g, int w, int h)
        {
            float gy = h - 55;
            using var br = new LinearGradientBrush(
                new RectangleF(0, gy, w, 55),
                Color.FromArgb(40, 160, 40), Color.FromArgb(15, 80, 15),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            g.FillRectangle(br, 0, gy, w, 55);
        }

        private void DrawDemoBird(Graphics g)
        {
            float bx = 120, by = _birdY;
            float w = 56, h = 44;
            var st = g.Save();
            g.TranslateTransform(bx + w / 2, by + h / 2);
            g.RotateTransform(_birdAngle);
            g.TranslateTransform(-w / 2, -h / 2);

            // Kalkan parıltısı
            float pr = (float)(0.5 + 0.5 * Math.Sin(_time * 4));
            using var gBr = new SolidBrush(Color.FromArgb((int)(40 * pr), 100, 180, 255));
            g.FillEllipse(gBr, -10, -10, w + 20, h + 20);

            // Gövde
            float wingOff = (float)Math.Sin(_wingAngle * Math.PI / 180.0) * 9f;
            using var wingBr = new SolidBrush(Color.FromArgb(220, Color.FromArgb(255, 220, 50)));
            g.FillEllipse(wingBr, w * 0.15f, h * 0.28f + wingOff, w * 0.52f, h * 0.42f);
            using var bodyBr = new LinearGradientBrush(new RectangleF(0, 0, w, h),
                Color.FromArgb(255, 220, 50), Color.FromArgb(200, 180, 30),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            g.FillEllipse(bodyBr, 0, 0, w, h);
            using var bellyBr = new SolidBrush(Color.FromArgb(160, Color.FromArgb(200, 100, 20)));
            g.FillEllipse(bellyBr, w * 0.52f, h * 0.32f, w * 0.36f, h * 0.4f);
            g.FillEllipse(Brushes.White, w * 0.54f, h * 0.07f, w * 0.3f, h * 0.36f);
            g.FillEllipse(Brushes.Black, w * 0.62f, h * 0.14f, w * 0.14f, h * 0.2f);
            g.FillEllipse(Brushes.White, w * 0.66f, h * 0.16f, w * 0.05f, h * 0.06f);
            PointF[] beak = { new(w * 0.84f, h * 0.34f), new(w * 1.03f, h * 0.44f), new(w * 0.84f, h * 0.54f) };
            using var gagebr = new SolidBrush(Color.FromArgb(255, 200, 50));
            g.FillPolygon(gagebr, beak);
            g.Restore(st);
        }

        private void DrawTitle(Graphics g, int w)
        {
            // Parıltılı gölge katmanlar
            int a = (int)(80 + 60 * _titleGlow);
            using var glowFont = new Font("Segoe UI", 68f, FontStyle.Bold);
            var sf = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString("KuraBird", glowFont, new SolidBrush(Color.FromArgb(a, Color.DeepSkyBlue)),
                new RectangleF(4, 44, w, 120), sf);
            g.DrawString("KuraBird", glowFont, new SolidBrush(Color.FromArgb(a / 2, Color.White)),
                new RectangleF(-4, 36, w, 120), sf);
            g.DrawString("KuraBird", glowFont, Brushes.White, new RectangleF(0, 40, w, 120), sf);

            using var subFont = new Font("Segoe UI", 14f, FontStyle.Italic);
            g.DrawString("~ Nesne Tabanlı Uçuş Macerası ~", subFont,
                new SolidBrush(Color.FromArgb(180, Color.LightCyan)),
                new RectangleF(0, 118, w, 30), sf);
        }

        private void DrawMenu(Graphics g, int w, int h)
        {
            float startY = 175f;
            float btnH = 46f;
            float btnW = 280f;
            float btnX = (w - btnW) / 2f;

            for (int i = 0; i < _menuItems.Length; i++)
            {
                float y = startY + i * (btnH + 10);
                bool selected = i == _selectedButton;

                // Buton arka planı
                Color bgColor = selected
                    ? Color.FromArgb(200, Color.FromArgb(40, 100, 200))
                    : Color.FromArgb(100, Color.FromArgb(20, 30, 60));
                Color borderColor = selected ? Color.CornflowerBlue : Color.FromArgb(60, Color.White);

                float scale = selected ? 1.05f : 1f;
                float bw = btnW * scale;
                float bh = btnH * scale;
                float bx = btnX + (btnW - bw) / 2f;
                float by = y + (btnH - bh) / 2f;

                using var br = new SolidBrush(bgColor);
                using var pen = new Pen(borderColor, selected ? 2f : 1f);

                // Yuvarlak dikdörtgen
                DrawRoundRect(g, br, pen, new RectangleF(bx, by, bw, bh), 12f);

                // Seçili ok
                if (selected)
                {
                    float pulse = 0.7f + 0.3f * (float)Math.Sin(_time * 6);
                    using var arrBr = new SolidBrush(Color.FromArgb((int)(pulse * 255), Color.Gold));
                    using var arrFont = new Font("Segoe UI", 14f, FontStyle.Bold);
                    g.DrawString("►", arrFont, arrBr, bx - 22, by + 12);
                }

                // Buton yazısı
                using var btnFont = new Font("Segoe UI", selected ? 15f : 13f, FontStyle.Bold);
                using var txtBr = new SolidBrush(selected ? Color.White : Color.FromArgb(200, Color.White));
                var bsf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(_menuItems[i], btnFont, txtBr, new RectangleF(bx, by, bw, bh), bsf);
            }
        }

        private void DrawRoundRect(Graphics g, Brush br, Pen pen, RectangleF rect, float radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            g.FillPath(br, path);
            g.DrawPath(pen, path);
        }

        private RectangleF GetButtonRect(int i)
        {
            float startY = 175f, btnH = 46f, btnW = 280f;
            float btnX = (ClientSize.Width - btnW) / 2f;
            return new RectangleF(btnX - 20, startY + i * 56, btnW + 40, btnH);
        }

        private void MainMenuForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up) { _selectedButton = Math.Max(0, _selectedButton - 1); AudioManager.Instance.Play("click"); }
            else if (e.KeyCode == Keys.Down) { _selectedButton = Math.Min(_menuItems.Length - 1, _selectedButton + 1); AudioManager.Instance.Play("click"); }
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) SelectMenuItem();
        }

        private void MainMenuForm_MouseMove(object? sender, MouseEventArgs e)
        {
            for (int i = 0; i < _menuItems.Length; i++)
                if (GetButtonRect(i).Contains(e.Location)) _selectedButton = i;
        }

        private void MainMenuForm_MouseClick(object? sender, MouseEventArgs e)
        {
            for (int i = 0; i < _menuItems.Length; i++)
                if (GetButtonRect(i).Contains(e.Location)) { _selectedButton = i; SelectMenuItem(); }
        }

        private void SelectMenuItem()
        {
            AudioManager.Instance.Play("click");
            switch (_selectedButton)
            {
                case 0: OpenGame(); break;
                case 1: OpenCharacterSelect(); break;
                case 2: OpenHighScores(); break;
                case 3: OpenSettings(); break;
                case 4: Application.Exit(); break;
            }
        }

        private void OpenGame()
        {
            _animTimer.Stop();
            var gf = new GameForm(Program.SelectedCharacter, Program.Difficulty);
            gf.FormClosed += (s, e) => { this.Show(); _animTimer.Start(); AudioManager.Instance.PlayLoopingMusic("menu_music"); };
            this.Hide();
            gf.Show();
        }

        private void OpenCharacterSelect()
        {
            var f = new CharacterSelectForm();
            f.ShowDialog(this);
        }

        private void OpenHighScores()
        {
            var f = new HighScoreForm();
            f.ShowDialog(this);
        }

        private void OpenSettings()
        {
            var f = new SettingsForm();
            f.ShowDialog(this);
        }
    }
}
