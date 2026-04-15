using KuraBird.Core;
using KuraBird.Entities;
using KuraBird.Managers;

namespace KuraBird.Core
{
    /// <summary>
    /// Ana oyun döngüsü. Tüm oyun nesnelerini yönetir.
    /// OOP: Composition over inheritance; tüm entity'ler liste halinde yönetilir.
    /// </summary>
    public class GameEngine
    {
        private const float PipeSpawnInterval = 1.8f;
        private const float PipeSpeedBase = 220f;
        private const float PipeSpeedIncrease = 8f;
        private const float PowerUpSpawnInterval = 12f;

        private Bird _bird;
        private List<Pipe> _pipes = new();
        private List<Particle> _particles = new();
        private List<PowerUp> _powerUps = new();

        private float _pipeTimer;
        private float _powerUpTimer;
        private float _gameTime;
        private float _bgScrollX1, _bgScrollX2, _bgScrollX3;
        private bool _started;

        private GameState _state = GameState.Playing;
        private int _difficulty;   // 0=Kolay, 1=Normal, 2=Zor
        private int _selectedChar;

        // Ekran sarsıntısı
        private float _shakeTimer;
        private float _shakeIntensity;
        private Random _rng = new();

        public event Action? OnScoreChange;
        public event Action? OnDied;

        public GameState State => _state;
        public Bird Bird => _bird;
        public bool IsRunning => _state == GameState.Playing;

        public GameEngine(int selectedChar, int difficulty)
        {
            _selectedChar = selectedChar;
            _difficulty = difficulty;
        }

        public void Initialize(int screenW, int screenH)
        {
            _pipes.Clear();
            _particles.Clear();
            _powerUps.Clear();
            _bird = new Bird(new PointF(screenW * 0.25f, screenH * 0.45f), _selectedChar);
            _gameTime = 0;
            _pipeTimer = 0;
            _powerUpTimer = 0;
            _started = false;
            _state = GameState.Playing;
            ScoreManager.Instance.ResetScore();
            AudioManager.Instance.PlayLoopingMusic("menu_music");
        }

        public void Flap()
        {
            if (_state == GameState.GameOver) return;
            _started = true;
            _bird.Flap();
        }

        public void TogglePause()
        {
            if (_state == GameState.Playing) _state = GameState.Paused;
            else if (_state == GameState.Paused) _state = GameState.Playing;
        }

        public void Update(float deltaTime, int screenW, int screenH)
        {
            if (_state != GameState.Playing) return;
            if (!_started) return;

            _gameTime += deltaTime;

            // Arka plan kaydırma (paralaks: 3 hız)
            _bgScrollX1 = (_bgScrollX1 - 30f * deltaTime) % screenW;
            _bgScrollX2 = (_bgScrollX2 - 60f * deltaTime) % screenW;
            _bgScrollX3 = (_bgScrollX3 - 120f * deltaTime) % screenW;

            // Kuş güncelle
            _bird.Update(deltaTime);
            if (_bird.IsOffScreen(screenH) || _bird.IsDead)
                HandleDeath(screenH);

            // Ekran sarsıntısı
            if (_shakeTimer > 0) _shakeTimer -= deltaTime;

            float speed = GetPipeSpeed();

            // Boru üretimi
            _pipeTimer += deltaTime;
            float interval = _difficulty == 2 ? 1.4f : _difficulty == 1 ? 1.8f : 2.2f;
            if (_pipeTimer >= interval)
            {
                _pipeTimer = 0;
                float gapCenter = screenH * 0.25f + (float)_rng.NextDouble() * screenH * 0.5f;
                _pipes.Add(new Pipe(screenW + 10, gapCenter, true, speed));
                _pipes.Add(new Pipe(screenW + 10, gapCenter, false, speed));
            }

            // PowerUp üretimi
            _powerUpTimer += deltaTime;
            if (_powerUpTimer >= PowerUpSpawnInterval)
            {
                _powerUpTimer = 0;
                float py = screenH * 0.2f + (float)_rng.NextDouble() * screenH * 0.6f;
                PowerUp pu = _rng.Next(2) == 0
                    ? (PowerUp)new ShieldPowerUp(new PointF(screenW + 10, py))
                    : new SlowPowerUp(new PointF(screenW + 10, py));
                _powerUps.Add(pu);
            }

            // Boru güncellemesi + skor + çarpışma
            foreach (var pipe in _pipes)
            {
                pipe.Update(deltaTime);
                if (!pipe.IsTop && !pipe.IsPassed && pipe.Position.X + Pipe.PipeWidth < _bird.Position.X)
                {
                    pipe.IsPassed = true;
                    ScoreManager.Instance.AddScore();
                    AudioManager.Instance.Play("score");
                    OnScoreChange?.Invoke();
                }
                if (pipe.IsActive && CollisionCheck(_bird, pipe))
                    _bird.OnCollision();
            }

            // PowerUp çarpışma
            foreach (var pu in _powerUps)
            {
                pu.Update(deltaTime);
                if (pu.IsActive && !pu.IsCollected && CollisionCheck(_bird, pu))
                {
                    pu.ApplyEffect(_bird);
                    // Parıltı efekti
                    _particles.AddRange(
                        Particle.CreateExplosion(
                            new PointF(_bird.Position.X + 26, _bird.Position.Y + 20),
                            Color.Gold, 14));
                }
            }

            // Partiküller
            foreach (var p in _particles) p.Update(deltaTime);

            // Ölü kuş güncelleme
            if (_bird.IsDead) _bird.Update(deltaTime);

            // Temizlik
            _pipes.RemoveAll(p => p.IsOffScreen());
            _powerUps.RemoveAll(p => !p.IsActive);
            _particles.RemoveAll(p => !p.IsActive);
        }

        private bool CollisionCheck(GameObject a, GameObject b)
        {
            return a.GetBounds().IntersectsWith(b.GetBounds());
        }

        private void HandleDeath(int screenH)
        {
            if (_state == GameState.GameOver) return;
            _state = GameState.GameOver;
            _shakeTimer = 0.5f;
            _shakeIntensity = 12f;
            _particles.AddRange(
                Particle.CreateExplosion(
                    new PointF(_bird.Position.X + 26, _bird.Position.Y + 20),
                    Color.OrangeRed, 24));
            AudioManager.Instance.Play("gameover");
            ScoreManager.Instance.SaveScore();
            OnDied?.Invoke();
        }

        public void Render(Graphics g, int screenW, int screenH)
        {
            // Ekran sarsıntısı offseti
            float sx = 0, sy = 0;
            if (_shakeTimer > 0)
            {
                sx = (float)(_rng.NextDouble() * 2 - 1) * _shakeIntensity;
                sy = (float)(_rng.NextDouble() * 2 - 1) * _shakeIntensity;
            }
            var state = g.Save();
            g.TranslateTransform(sx, sy);

            DrawBackground(g, screenW, screenH);
            DrawGround(g, screenW, screenH);

            foreach (var p in _pipes) p.Render(g);
            foreach (var pu in _powerUps) pu.Render(g);
            foreach (var pt in _particles) pt.Render(g);
            _bird.Render(g);

            g.Restore(state);
            DrawHUD(g, screenW, screenH);
        }

        private void DrawBackground(Graphics g, int w, int h)
        {
            // Gökyüzü gradyanı
            using var skyBrush = new LinearGradientBrush(
                new Rectangle(0, 0, w, h),
                Color.FromArgb(10, 20, 60),
                Color.FromArgb(30, 100, 180),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            g.FillRectangle(skyBrush, 0, 0, w, h);

            // Yıldızlar (3. paralaks katman - en yavaş)
            g.TranslateTransform(_bgScrollX3, 0);
            DrawStars(g, w, h);
            g.TranslateTransform(-_bgScrollX3, 0);

            // Uzak bulutlar (2. katman - orta)
            g.TranslateTransform(_bgScrollX2, 0);
            DrawClouds(g, w, h, 0.5f, Color.FromArgb(30, 255, 255, 255), 3);
            g.TranslateTransform(-_bgScrollX2, 0);

            // Yakın bulutlar (1. katman - hızlı)
            g.TranslateTransform(_bgScrollX1, 0);
            DrawClouds(g, w, h, 1.0f, Color.FromArgb(50, 255, 255, 255), 2);
            g.TranslateTransform(-_bgScrollX1, 0);
        }

        private void DrawStars(Graphics g, int w, int h)
        {
            var rng = new Random(42); // sabit seed = sabit yıldız konumları
            for (int i = 0; i < 100; i++)
            {
                float x = (float)rng.NextDouble() * w * 2;
                float y = (float)rng.NextDouble() * h * 0.6f;
                float sz = (float)rng.NextDouble() * 2 + 0.5f;
                using var br = new SolidBrush(Color.FromArgb(
                    (int)(rng.NextDouble() * 100 + 155), Color.White));
                g.FillEllipse(br, x, y, sz, sz);
            }
        }

        private void DrawClouds(Graphics g, int w, int h, float scale, Color color, int count)
        {
            var rng = new Random(12);
            using var br = new SolidBrush(color);
            for (int i = 0; i < count; i++)
            {
                float cx = (float)rng.NextDouble() * w * 2;
                float cy = (float)rng.NextDouble() * h * 0.35f + 20;
                float cw = (float)(rng.NextDouble() * 120 + 60) * scale;
                float ch = cw * 0.4f * scale;
                g.FillEllipse(br, cx, cy, cw, ch);
                g.FillEllipse(br, cx + cw * 0.3f, cy - ch * 0.3f, cw * 0.7f, ch * 0.8f);
                g.FillEllipse(br, cx - cw * 0.1f, cy + ch * 0.1f, cw * 0.6f, ch * 0.6f);
            }
        }

        private void DrawGround(Graphics g, int w, int h)
        {
            float groundY = h - 60;
            using var grassBrush = new LinearGradientBrush(
                new RectangleF(0, groundY, w, 60),
                Color.FromArgb(40, 160, 40),
                Color.FromArgb(20, 100, 20),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            g.FillRectangle(grassBrush, 0, groundY, w, 60);

            // Zemin çizgisi
            using var pen = new Pen(Color.FromArgb(60, 200, 60), 2f);
            g.DrawLine(pen, 0, groundY, w, groundY);

            // Hareketli zemin dokusu
            float tileW = 40f;
            float offset = _bgScrollX1 % tileW;
            using var tilePen = new Pen(Color.FromArgb(25, 0, 0, 0), 1f);
            for (float x = offset - tileW; x < w + tileW; x += tileW)
                g.DrawLine(tilePen, x, groundY, x, groundY + 60);
        }

        private void DrawHUD(Graphics g, int w, int h)
        {
            // Skor
            using var bigFont = new Font("Segoe UI", 36f, FontStyle.Bold);
            using var shadowBrush = new SolidBrush(Color.FromArgb(120, Color.Black));
            using var scoreBrush = new SolidBrush(Color.White);

            string scoreText = ScoreManager.Instance.CurrentScore.ToString();
            var sf = new StringFormat { Alignment = StringAlignment.Center };

            g.DrawString(scoreText, bigFont, shadowBrush, new PointF(w / 2f + 2, 22));
            g.DrawString(scoreText, bigFont, scoreBrush, new PointF(w / 2f, 20));

            // En yüksek skor
            using var smallFont = new Font("Segoe UI", 12f, FontStyle.Bold);
            g.DrawString($"En İyi: {ScoreManager.Instance.HighScore}", smallFont, shadowBrush, 12, 12);
            g.DrawString($"En İyi: {ScoreManager.Instance.HighScore}", smallFont, scoreBrush, 11, 11);

            // Power-up göstergesi
            float py = 60f;
            if (_bird.HasShield)
            {
                using var shFont = new Font("Segoe UI", 11f, FontStyle.Bold);
                g.DrawString("🛡 KALKAN AKTİF", shFont, Brushes.CornflowerBlue, 10, py);
                py += 22;
            }
            if (_bird.IsSlowed)
            {
                using var slFont = new Font("Segoe UI", 11f, FontStyle.Bold);
                g.DrawString("⏱ YAVAŞLAMA AKTİF", slFont, Brushes.LightGreen, 10, py);
            }

            // Duraklatıldı
            if (_state == GameState.Paused)
            {
                using var overlay = new SolidBrush(Color.FromArgb(140, Color.Black));
                g.FillRectangle(overlay, 0, 0, w, h);
                using var pauseFont = new Font("Segoe UI", 44f, FontStyle.Bold);
                g.DrawString("⏸ DURAKLATILDI", pauseFont, Brushes.White, new PointF(w / 2f - 200, h / 2f - 40));
                using var resumeFont = new Font("Segoe UI", 18f);
                g.DrawString("Devam etmek için ESC/P tuşuna basın", resumeFont,
                    Brushes.LightGray, new PointF(w / 2f - 180, h / 2f + 20));
            }
        }

        private float GetPipeSpeed()
        {
            float t = _gameTime / 60f; // dakika
            float base_ = _difficulty == 0 ? 180f : _difficulty == 1 ? 240f : 300f;
            return base_ + t * PipeSpeedIncrease * 60f;
        }
    }
}
