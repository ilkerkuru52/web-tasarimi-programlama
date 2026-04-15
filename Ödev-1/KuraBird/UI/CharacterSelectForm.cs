namespace KuraBird.UI
{
    public class CharacterSelectForm : Form
    {
        private int _selected = 0;
        private System.Windows.Forms.Timer _animTimer;
        private float _time = 0f;
        private float _wingAngle = 0f;
        private bool _wingUp = true;

        private static readonly (string Name, Color Body, Color Belly, Color Eye, string Desc)[] Characters = {
            ("Sarı Kuş",   Color.FromArgb(255,220,50),  Color.FromArgb(200,100,20), Color.White, "Klasik görünüm,\ndengeli uçuş"),
            ("Mavi Kuş",   Color.FromArgb(80, 180,255), Color.FromArgb(20,80,200),  Color.White, "Hızlı flap,\nhızlı düşüş"),
            ("Yeşil Kuş",  Color.FromArgb(80, 220,80),  Color.FromArgb(20,140,30),  Color.White, "Yavaş düşüş,\nkolay kontrol"),
            ("Kırmızı Kuş",Color.FromArgb(255,100,100), Color.FromArgb(180,20,20),  Color.White, "Güçlü flap,\nnormal düşüş"),
        };

        public CharacterSelectForm()
        {
            this.DoubleBuffered = true;
            this.Text = "Karakter Seç – KuraBird";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(700, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(10, 15, 40);
            this.KeyPreview = true;

            _selected = Program.SelectedCharacter;
            _animTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _animTimer.Tick += (s, e) => { _time += 0.016f; if (_wingUp) { _wingAngle += 220f * 0.016f; if (_wingAngle >= 25) _wingUp = false; } else { _wingAngle -= 220f * 0.016f; if (_wingAngle <= -8) _wingUp = true; } Invalidate(); };
            _animTimer.Start();

            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Left) { _selected = Math.Max(0, _selected - 1); }
                else if (e.KeyCode == Keys.Right) { _selected = Math.Min(Characters.Length - 1, _selected + 1); }
                else if (e.KeyCode == Keys.Enter) Confirm();
                else if (e.KeyCode == Keys.Escape) { _animTimer.Stop(); this.Close(); }
            };
            this.MouseClick += (s, e) =>
            {
                for (int i = 0; i < Characters.Length; i++)
                {
                    if (GetCardRect(i).Contains(e.Location)) { _selected = i; if (e.Clicks == 2) Confirm(); }
                }
                if (GetConfirmRect().Contains(e.Location)) Confirm();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int w = ClientSize.Width, h = ClientSize.Height;

            using var bgBr = new LinearGradientBrush(new Rectangle(0, 0, w, h),
                Color.FromArgb(8, 12, 35), Color.FromArgb(15, 40, 80), System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            g.FillRectangle(bgBr, 0, 0, w, h);

            using var titleFont = new Font("Segoe UI", 22f, FontStyle.Bold);
            var sf = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString("🐦  Karakter Seç", titleFont, Brushes.White, new RectangleF(0, 14, w, 40), sf);

            for (int i = 0; i < Characters.Length; i++) DrawCard(g, i);

            // Onayla butonu
            DrawConfirmButton(g, w, h);
        }

        private RectangleF GetCardRect(int i)
        {
            float cardW = 140f, cardH = 200f, startX = 30f, gap = 18f;
            float totalW = Characters.Length * (cardW + gap) - gap;
            float startXCentered = (ClientSize.Width - totalW) / 2f;
            return new RectangleF(startXCentered + i * (cardW + gap), 65f, cardW, cardH);
        }

        private RectangleF GetConfirmRect() => new RectangleF(ClientSize.Width / 2f - 110, 288f, 220f, 46f);

        private void DrawCard(Graphics g, int i)
        {
            var rect = GetCardRect(i);
            bool sel = i == _selected;
            var ch = Characters[i];

            Color bgColor = sel ? Color.FromArgb(180, Color.FromArgb(30, 80, 200)) : Color.FromArgb(80, Color.FromArgb(15, 25, 55));
            Color borderColor = sel ? Color.CornflowerBlue : Color.FromArgb(50, Color.White);
            float borderW = sel ? 2.5f : 1f;

            using var br = new SolidBrush(bgColor);
            using var pen = new Pen(borderColor, borderW);
            var path = RoundRectPath(rect, 14f);
            g.FillPath(br, path);
            g.DrawPath(pen, path);

            // Kuş çizimi
            DrawBirdPreview(g, ch.Body, ch.Belly, rect.X + rect.Width / 2 - 28, rect.Y + 22, sel);

            // İsim
            using var nameFont = new Font("Segoe UI", 11f, FontStyle.Bold);
            var nsf = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString(ch.Name, nameFont, sel ? Brushes.White : Brushes.LightGray,
                new RectangleF(rect.X, rect.Y + 106, rect.Width, 22), nsf);

            // Açıklama
            using var descFont = new Font("Segoe UI", 8.5f);
            g.DrawString(ch.Desc, descFont, new SolidBrush(Color.FromArgb(170, Color.White)),
                new RectangleF(rect.X + 4, rect.Y + 130, rect.Width - 8, 50), nsf);

            // Seçili işareti
            if (sel)
            {
                using var checkFont = new Font("Segoe UI", 18f);
                g.DrawString("✔", checkFont, Brushes.LimeGreen, rect.X + rect.Width - 30, rect.Y + 4);
            }
        }

        private void DrawBirdPreview(Graphics g, Color body, Color belly, float bx, float by, bool animate)
        {
            float w = 56f, h = 44f;
            float wingOff = animate ? (float)Math.Sin(_wingAngle * Math.PI / 180.0) * 9f : 0f;
            using var wingBr = new SolidBrush(Color.FromArgb(200, body));
            g.FillEllipse(wingBr, bx + w * 0.15f, by + h * 0.28f + wingOff, w * 0.52f, h * 0.42f);
            using var bodyBr = new LinearGradientBrush(new RectangleF(bx, by, w, h), body, Color.FromArgb(200, body.R, body.G, body.B), System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            g.FillEllipse(bodyBr, bx, by, w, h);
            using var bellyBr = new SolidBrush(Color.FromArgb(160, belly));
            g.FillEllipse(bellyBr, bx + w * 0.52f, by + h * 0.32f, w * 0.36f, h * 0.4f);
            g.FillEllipse(Brushes.White, bx + w * 0.54f, by + h * 0.07f, w * 0.3f, h * 0.36f);
            g.FillEllipse(Brushes.Black, bx + w * 0.62f, by + h * 0.14f, w * 0.14f, h * 0.2f);
            PointF[] beak = { new(bx + w * 0.84f, by + h * 0.34f), new(bx + w * 1.03f, by + h * 0.44f), new(bx + w * 0.84f, by + h * 0.54f) };
            using var beakBr = new SolidBrush(Color.FromArgb(255, 200, 50));
            g.FillPolygon(beakBr, beak);
        }

        private void DrawConfirmButton(Graphics g, int w, int h)
        {
            var rect = GetConfirmRect();
            float pulse = 0.85f + 0.15f * (float)Math.Sin(_time * 4);
            using var br = new SolidBrush(Color.FromArgb((int)(180 * pulse), Color.FromArgb(30, 100, 200)));
            using var pen = new Pen(Color.CornflowerBlue, 2f);
            g.FillPath(br, RoundRectPath(rect, 12f));
            g.DrawPath(pen, RoundRectPath(rect, 12f));
            using var btnFont = new Font("Segoe UI", 14f, FontStyle.Bold);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("✔  Seç ve Devam Et", btnFont, Brushes.White, rect, sf);
        }

        private System.Drawing.Drawing2D.GraphicsPath RoundRectPath(RectangleF r, float rad)
        {
            var p = new System.Drawing.Drawing2D.GraphicsPath();
            p.AddArc(r.X, r.Y, rad * 2, rad * 2, 180, 90);
            p.AddArc(r.Right - rad * 2, r.Y, rad * 2, rad * 2, 270, 90);
            p.AddArc(r.Right - rad * 2, r.Bottom - rad * 2, rad * 2, rad * 2, 0, 90);
            p.AddArc(r.X, r.Bottom - rad * 2, rad * 2, rad * 2, 90, 90);
            p.CloseFigure();
            return p;
        }

        private void Confirm()
        {
            Program.SelectedCharacter = _selected;
            _animTimer.Stop();
            this.Close();
        }
    }
}
