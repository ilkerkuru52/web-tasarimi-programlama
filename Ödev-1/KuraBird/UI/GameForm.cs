using KuraBird.Core;
using KuraBird.Managers;

namespace KuraBird.UI
{
    public class GameForm : Form
    {
        private GameEngine _engine;
        private System.Windows.Forms.Timer _gameTimer;
        private DateTime _lastTick;
        private bool _gameOverHandled;

        public GameForm(int charIndex, int difficulty)
        {
            this.DoubleBuffered = true;
            this.Text = "KuraBird";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black;
            this.KeyPreview = true;

            _engine = new GameEngine(charIndex, difficulty);
            _engine.OnDied += Engine_OnDied;

            _gameTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _gameTimer.Tick += GameTimer_Tick;

            this.KeyDown += GameForm_KeyDown;
            this.MouseClick += GameForm_MouseClick;
            this.Load += (s, e) =>
            {
                _engine.Initialize(ClientSize.Width, ClientSize.Height);
                _lastTick = DateTime.Now;
                _gameTimer.Start();
            };
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            float dt = (float)(now - _lastTick).TotalSeconds;
            _lastTick = now;
            dt = Math.Min(dt, 0.05f);
            _engine.Update(dt, ClientSize.Width, ClientSize.Height);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            _engine.Render(g, ClientSize.Width, ClientSize.Height);

            if (_engine.State == GameState.GameOver && _gameOverHandled)
                DrawGameOverOverlay(g);
        }

        private void DrawGameOverOverlay(Graphics g)
        {
            int w = ClientSize.Width, h = ClientSize.Height;
            // Karartma
            using var ov = new SolidBrush(Color.FromArgb(170, Color.Black));
            g.FillRectangle(ov, 0, 0, w, h);

            // Kutu
            float bw = 380, bh = 240;
            float bx = (w - bw) / 2, by = (h - bh) / 2;
            DrawRoundRect(g, new SolidBrush(Color.FromArgb(220, Color.FromArgb(15, 25, 55))),
                new Pen(Color.CornflowerBlue, 2f), new RectangleF(bx, by, bw, bh), 16f);

            var sf = new StringFormat { Alignment = StringAlignment.Center };

            using var bigFont = new Font("Segoe UI", 36f, FontStyle.Bold);
            g.DrawString("OYUN BİTTİ!", bigFont, Brushes.OrangeRed, new RectangleF(bx, by + 16, bw, 60), sf);

            using var scoreFont = new Font("Segoe UI", 22f, FontStyle.Bold);
            g.DrawString($"Skor: {ScoreManager.Instance.CurrentScore}", scoreFont, Brushes.White,
                new RectangleF(bx, by + 76, bw, 40), sf);

            using var bestFont = new Font("Segoe UI", 16f);
            g.DrawString($"En Yüksek: {ScoreManager.Instance.HighScore}", bestFont, Brushes.Gold,
                new RectangleF(bx, by + 116, bw, 30), sf);

            // Butonlar
            using var btnFont = new Font("Segoe UI", 14f, FontStyle.Bold);
            DrawRoundRect(g, new SolidBrush(Color.FromArgb(180, Color.FromArgb(30, 80, 180))),
                new Pen(Color.CornflowerBlue, 1.5f), new RectangleF(bx + 20, by + 160, 160, 44), 10f);
            g.DrawString("🔄 Tekrar", btnFont, Brushes.White,
                new RectangleF(bx + 20, by + 160, 160, 44), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

            DrawRoundRect(g, new SolidBrush(Color.FromArgb(180, Color.FromArgb(60, 20, 20))),
                new Pen(Color.IndianRed, 1.5f), new RectangleF(bx + 200, by + 160, 160, 44), 10f);
            g.DrawString("🏠 Ana Menü", btnFont, Brushes.White,
                new RectangleF(bx + 200, by + 160, 160, 44), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        private void DrawRoundRect(Graphics g, Brush br, Pen pen, RectangleF rect, float r)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
            path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
            path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            g.FillPath(br, path);
            g.DrawPath(pen, path);
        }

        private void Engine_OnDied()
        {
            _gameOverHandled = true;
        }

        private void GameForm_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                case Keys.Up:
                case Keys.W:
                    if (_engine.State == GameState.Playing) _engine.Flap();
                    else if (_engine.State == GameState.GameOver) RestartGame();
                    break;
                case Keys.Escape:
                case Keys.P:
                    if (_engine.State == GameState.Playing || _engine.State == GameState.Paused)
                        _engine.TogglePause();
                    break;
                case Keys.R:
                    if (_engine.State == GameState.GameOver) RestartGame();
                    break;
            }
        }

        private void GameForm_MouseClick(object? sender, MouseEventArgs e)
        {
            if (_engine.State == GameState.Playing)
            {
                _engine.Flap();
                return;
            }
            if (_engine.State == GameState.GameOver && _gameOverHandled)
            {
                int w = ClientSize.Width, h = ClientSize.Height;
                float bw = 380, bh = 240;
                float bx = (w - bw) / 2, by = (h - bh) / 2;
                var retryRect = new RectangleF(bx + 20, by + 160, 160, 44);
                var menuRect = new RectangleF(bx + 200, by + 160, 160, 44);
                if (retryRect.Contains(e.Location)) RestartGame();
                else if (menuRect.Contains(e.Location)) GoToMenu();
            }
        }

        private void RestartGame()
        {
            _gameOverHandled = false;
            _engine = new GameEngine(Program.SelectedCharacter, Program.Difficulty);
            _engine.OnDied += Engine_OnDied;
            _engine.Initialize(ClientSize.Width, ClientSize.Height);
            _lastTick = DateTime.Now;
        }

        private void GoToMenu()
        {
            _gameTimer.Stop();
            this.Close();
        }
    }
}
