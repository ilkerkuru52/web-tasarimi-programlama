using KuraBird.Managers;

namespace KuraBird.UI
{
    public class HighScoreForm : Form
    {
        private System.Windows.Forms.Timer _animTimer;
        private float _time = 0f;

        public HighScoreForm()
        {
            this.DoubleBuffered = true;
            this.Text = "Yüksek Skorlar – KuraBird";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(500, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(10, 15, 40);
            this.KeyPreview = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
            this.MouseClick += (s, e) => { if (GetCloseRect().Contains(e.Location)) this.Close(); };

            _animTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _animTimer.Tick += (s, e) => { _time += 0.016f; Invalidate(); };
            _animTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int w = ClientSize.Width, h = ClientSize.Height;

            using var bgBr = new LinearGradientBrush(new Rectangle(0, 0, w, h),
                Color.FromArgb(8, 12, 35), Color.FromArgb(15, 40, 80),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            g.FillRectangle(bgBr, 0, 0, w, h);

            // Başlık
            using var titleFont = new Font("Segoe UI", 22f, FontStyle.Bold);
            float glow = 0.7f + 0.3f * (float)Math.Sin(_time * 2.5);
            var sf = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString("🏆  En Yüksek Skorlar", titleFont,
                new SolidBrush(Color.FromArgb((int)(glow * 255), Color.Gold)),
                new RectangleF(0, 18, w, 44), sf);

            var scores = ScoreManager.Instance.Leaderboard;

            if (scores.Count == 0)
            {
                using var noFont = new Font("Segoe UI", 14f, FontStyle.Italic);
                g.DrawString("Henüz skor kaydedilmedi.\nOynu oyna ve ilk seni ol!", noFont,
                    Brushes.DimGray, new RectangleF(0, h / 2f - 40, w, 80), sf);
            }
            else
            {
                Color[] rowColors = {
                    Color.FromArgb(255, 215, 0),  // Altın
                    Color.FromArgb(192, 192, 192), // Gümüş
                    Color.FromArgb(205, 127, 50),  // Bronz
                };
                string[] medals = { "🥇", "🥈", "🥉" };

                float rowH = 38f, startY = 76f;
                for (int i = 0; i < scores.Count; i++)
                {
                    float rowY = startY + i * rowH;
                    bool isTop3 = i < 3;

                    // Satır arka planı
                    Color rowBg = isTop3
                        ? Color.FromArgb(60, rowColors[i])
                        : Color.FromArgb(30, Color.FromArgb(20, 40, 80));
                    using var rowBr = new SolidBrush(rowBg);
                    g.FillRectangle(rowBr, 20, rowY, w - 40, rowH - 4);

                    // Rank
                    using var rankFont = new Font("Segoe UI", 13f, FontStyle.Bold);
                    string rankStr = isTop3 ? medals[i] : $"#{i + 1}";
                    Color rankColor = isTop3 ? rowColors[i] : Color.Gray;
                    g.DrawString(rankStr, rankFont, new SolidBrush(rankColor), 28, rowY + 8);

                    // İsim
                    using var nameFont = new Font("Segoe UI", 12f);
                    Color nameColor = isTop3 ? rowColors[i] : Color.FromArgb(200, Color.White);
                    g.DrawString(scores[i].Name, nameFont, new SolidBrush(nameColor), 80, rowY + 9);

                    // Skor
                    using var scoreFont = new Font("Segoe UI", 14f, FontStyle.Bold);
                    var scoreSf = new StringFormat { Alignment = StringAlignment.Far };
                    g.DrawString(scores[i].Score.ToString(), scoreFont, new SolidBrush(nameColor),
                        new RectangleF(0, rowY + 7, w - 30, 24), scoreSf);
                }
            }

            // Kapat butonu
            DrawCloseButton(g, w, h);
        }

        private RectangleF GetCloseRect() => new RectangleF(ClientSize.Width / 2f - 90, ClientSize.Height - 58, 180f, 42f);

        private void DrawCloseButton(Graphics g, int w, int h)
        {
            var rect = GetCloseRect();
            using var br = new SolidBrush(Color.FromArgb(160, Color.FromArgb(60, 20, 20)));
            using var pen = new Pen(Color.IndianRed, 1.5f);
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            float r = 10f;
            path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
            path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
            path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            g.FillPath(br, path);
            g.DrawPath(pen, path);
            using var btnFont = new Font("Segoe UI", 12f, FontStyle.Bold);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("✖  Kapat", btnFont, Brushes.White, rect, sf);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _animTimer.Stop();
            base.OnFormClosed(e);
        }
    }
}
