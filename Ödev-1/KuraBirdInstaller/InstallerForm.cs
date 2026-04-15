using System.IO;

namespace KuraBirdInstaller
{
    /// <summary>
    /// Adım adım kurulum sihirbazı formu.
    /// Adım 1: Karşılama + tekil kurulum kontrolü
    /// Adım 2: Lisans sözleşmesi
    /// Adım 3: Kurulum dizini seçimi
    /// Adım 4: Kurulum ilerlemesi
    /// Adım 5: Tamamlandı
    /// </summary>
    public partial class InstallerForm : Form
    {
        private int _step = 0;
        private string _installDir = @"C:\Program Files\KuraBird";
        private System.Windows.Forms.Timer _animTimer;
        private System.Windows.Forms.Timer _progressTimer;
        private float _time = 0f;
        private float _progress = 0f;
        private string _statusMsg = "";
        private bool _installDone = false;
        private CheckResult? _checkResult;

        // Kopyalanacak dosyalar (Inno Setup bunu yapar ama standalone modda biz yaparız)
        private string _sourceDir = Path.GetDirectoryName(Application.ExecutablePath) ?? ".";

        public InstallerForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Text = "KuraBird Kurulum Sihirbazı";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new Size(580, 440);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 15, 40);

            _animTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _animTimer.Tick += (s, e) => { _time += 0.016f; Invalidate(); };
            _animTimer.Start();

            this.MouseClick += InstallerForm_MouseClick;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape && _step < 4) Application.Exit(); };
            this.KeyPreview = true;

            // Başlangıçta kontrol et
            _checkResult = InstallGuard.CheckInstallStatus();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(580, 440);
            this.Name = "InstallerForm";
            this.ResumeLayout(false);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int w = ClientSize.Width, h = ClientSize.Height;

            // Arka plan
            using var bgBr = new LinearGradientBrush(new Rectangle(0, 0, w, h),
                Color.FromArgb(8, 12, 35), Color.FromArgb(12, 35, 70),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            g.FillRectangle(bgBr, 0, 0, w, h);

            // Üst başlık şeridi
            using var headerBr = new LinearGradientBrush(new Rectangle(0, 0, w, 70),
                Color.FromArgb(20, 60, 160), Color.FromArgb(10, 30, 80),
                System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            g.FillRectangle(headerBr, 0, 0, w, 70);

            // Logo & başlık
            using var logoFont = new Font("Segoe UI", 26f, FontStyle.Bold);
            using var logoBr = new SolidBrush(Color.White);
            float glow = 0.7f + 0.3f * (float)Math.Sin(_time * 2.5);
            g.DrawString("🐦 KuraBird", logoFont, new SolidBrush(Color.FromArgb((int)(glow * 255), Color.CornflowerBlue)), 24, 12);

            using var subFont = new Font("Segoe UI", 10f, FontStyle.Italic);
            g.DrawString("Kurulum Sihirbazı  v1.0", subFont, new SolidBrush(Color.FromArgb(160, Color.White)), 30, 46);

            // Adım göstergesi (sağ üst)
            DrawStepIndicator(g, w);

            // İçerik
            switch (_step)
            {
                case 0: DrawStepWelcome(g, w, h); break;
                case 1: DrawStepLicense(g, w, h); break;
                case 2: DrawStepDirectory(g, w, h); break;
                case 3: DrawStepInstalling(g, w, h); break;
                case 4: DrawStepDone(g, w, h); break;
            }

            // Alt buton şeridi
            DrawBottomBar(g, w, h);
        }

        private void DrawStepIndicator(Graphics g, int w)
        {
            string[] stepNames = { "Hoş Geldin", "Lisans", "Dizin", "Kurulum", "Bitti" };
            float totalW = stepNames.Length * 80f;
            float startX = w - totalW - 20;
            for (int i = 0; i < stepNames.Length; i++)
            {
                bool active = i == _step;
                bool done = i < _step;
                Color c = done ? Color.LimeGreen : active ? Color.CornflowerBlue : Color.FromArgb(60, Color.White);
                using var dotBr = new SolidBrush(c);
                g.FillEllipse(dotBr, startX + i * 80 + 28, 22, 12, 12);
                using var sf2 = new Font("Segoe UI", 7.5f);
                using var lBr = new SolidBrush(Color.FromArgb(active ? 220 : 120, Color.White));
                g.DrawString(stepNames[i], sf2, lBr, startX + i * 80 + 12, 36);
                if (i < stepNames.Length - 1)
                {
                    using var linePen = new Pen(Color.FromArgb(40, Color.White), 1f);
                    g.DrawLine(linePen, startX + i * 80 + 40, 28, startX + (i + 1) * 80 + 28, 28);
                }
            }
        }

        private void DrawStepWelcome(Graphics g, int w, int h)
        {
            var sf = new StringFormat { Alignment = StringAlignment.Center };
            using var titleFont = new Font("Segoe UI", 18f, FontStyle.Bold);
            g.DrawString("KuraBird'e Hoş Geldiniz!", titleFont, Brushes.White, new RectangleF(0, 84, w, 36), sf);

            if (_checkResult != null && _checkResult.IsInstalled)
            {
                // ENGELLEME DURUMU
                using var warnBr = new SolidBrush(Color.FromArgb(200, Color.FromArgb(100, 10, 10)));
                using var warnPen = new Pen(Color.OrangeRed, 2f);
                g.FillRectangle(warnBr, 40, 118, w - 80, 188);
                g.DrawRectangle(warnPen, 40, 118, w - 80, 188);

                using var warnFont = new Font("Segoe UI", 15f, FontStyle.Bold);
                g.DrawString("⛔  KURULUM ENGELLENDİ", warnFont,
                    Brushes.OrangeRed, new RectangleF(40, 126, w - 80, 34), sf);

                using var msgFont = new Font("Segoe UI", 11f);
                g.DrawString(
                    "KuraBird bu bilgisayara daha önce kurulmuştur.\n" +
                    "Oyun kaldırılmış olsa dahi yeniden kurulum yapılamaz.\n\n" +
                    "⚠  Bu kilit BİLGİSAYARA (donanıma) özgüdür.\n" +
                    "    Farklı bir bilgisayarda kurulum yapılabilir.\n" +
                    "    (Örn: hocanız kendi PC'sine kurabilir ✓)",
                    msgFont, Brushes.White, new RectangleF(55, 164, w - 110, 130), new StringFormat());

                DrawButton(g, "✖  Kapat", new RectangleF(w / 2f - 90, 322, 180, 44),
                    Color.FromArgb(160, Color.FromArgb(80, 15, 15)), Color.OrangeRed, Brushes.White);
            }
            else
            {
                using var msgFont = new Font("Segoe UI", 11.5f);
                string msg =
                    "KuraBird, Flappy Bird'den ilham alan gelişmiş bir\n" +
                    "nesne tabanlı programlama oyunudur.\n\n" +
                    "✔  Animasyonlu menüler ve 4 farklı karakter\n" +
                    "✔  Paralaks arka plan ve partikül efektleri\n" +
                    "✔  Power-up sistemi (Kalkan & Yavaşlatma)\n" +
                    "✔  Yüksek skor tablosu\n\n" +
                    "ℹ  TEKİL KURULUM: Bu paket her bilgisayara yalnızca\n" +
                    "   bir kez kurulabilir. Kilit donanıma bağlıdır —\n" +
                    "   farklı PC'lerde bağımsız olarak çalışır.";
                g.DrawString(msg, msgFont, Brushes.LightCyan, new RectangleF(55, 118, w - 110, 230), new StringFormat());
            }
        }

        private void DrawStepLicense(Graphics g, int w, int h)
        {
            using var titleFont = new Font("Segoe UI", 16f, FontStyle.Bold);
            g.DrawString("Lisans Sözleşmesi", titleFont, Brushes.White, new PointF(40, 84));

            string license =
                "KuraBird Kullanıcı Lisans Sözleşmesi\n" +
                "─────────────────────────────────\n\n" +
                "1. Bu yazılım yalnızca eğitim amaçlıdır.\n\n" +
                "2. Bu kurulum paketi TEK BİR BİLGİSAYARA yüklenebilir.\n" +
                "   Donanım parmak izi kaydedilir; oyun kaldırılsa dahi\n" +
                "   aynı paketi tekrar kullanmak mümkün değildir.\n\n" +
                "3. Yazılımın izinsiz kopyalanması yasaktır.\n\n" +
                "4. Geliştirici, yazılımın kullanımından doğan\n" +
                "   zararlardan sorumlu tutulamaz.\n\n" +
                "Devam ederek bu sözleşmeyi kabul etmiş sayılırsınız.";

            using var licBr = new SolidBrush(Color.FromArgb(80, Color.FromArgb(20, 40, 80)));
            g.FillRectangle(licBr, 40, 112, w - 80, 200);
            using var licPen = new Pen(Color.FromArgb(60, Color.CornflowerBlue), 1f);
            g.DrawRectangle(licPen, 40, 112, w - 80, 200);

            using var licFont = new Font("Segoe UI", 10.5f);
            g.DrawString(license, licFont, Brushes.LightCyan, new RectangleF(52, 120, w - 104, 188));
        }

        private void DrawStepDirectory(Graphics g, int w, int h)
        {
            using var titleFont = new Font("Segoe UI", 16f, FontStyle.Bold);
            g.DrawString("Kurulum Dizini", titleFont, Brushes.White, new PointF(40, 84));

            using var descFont = new Font("Segoe UI", 11f);
            g.DrawString("KuraBird aşağıdaki klasöre kurulacaktır:", descFont, Brushes.LightGray, new PointF(40, 120));

            // Dizin kutusu
            using var dirBr = new SolidBrush(Color.FromArgb(80, Color.FromArgb(20, 40, 80)));
            g.FillRectangle(dirBr, 40, 150, w - 80, 46);
            using var dirPen = new Pen(Color.CornflowerBlue, 1.5f);
            g.DrawRectangle(dirPen, 40, 150, w - 80, 46);
            using var dirFont = new Font("Consolas", 12f);
            g.DrawString(_installDir, dirFont, Brushes.White, new PointF(52, 163));

            using var noteFont = new Font("Segoe UI", 10f, FontStyle.Italic);
            g.DrawString("Not: Kurulum dizini bu sürümde değiştirilemez.", noteFont,
                new SolidBrush(Color.FromArgb(140, Color.White)), new PointF(40, 210));

            // Gerekli alan
            g.DrawString("Gerekli disk alanı: ~15 MB", descFont, Brushes.LightGray, new PointF(40, 240));
        }

        private void DrawStepInstalling(Graphics g, int w, int h)
        {
            var sf = new StringFormat { Alignment = StringAlignment.Center };

            if (!_installDone)
            {
                using var titleFont = new Font("Segoe UI", 16f, FontStyle.Bold);
                g.DrawString("Kuruluyor...", titleFont, Brushes.White, new RectangleF(0, 84, w, 34), sf);

                // Progress bar
                float barW = w - 100;
                float barX = 50, barY = 140;
                using var barBg = new SolidBrush(Color.FromArgb(50, Color.White));
                g.FillRectangle(barBg, barX, barY, barW, 28);
                using var barBr = new LinearGradientBrush(
                    new RectangleF(barX, barY, barW, 28),
                    Color.FromArgb(30, 100, 220), Color.CornflowerBlue,
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
                g.FillRectangle(barBr, barX, barY, barW * _progress, 28);
                using var barPen = new Pen(Color.CornflowerBlue, 1.5f);
                g.DrawRectangle(barPen, barX, barY, barW, 28);

                // Yüzde
                using var pctFont = new Font("Segoe UI", 13f, FontStyle.Bold);
                g.DrawString($"{(int)(_progress * 100)}%", pctFont, Brushes.White,
                    new RectangleF(0, 138, w, 32), sf);

                // Durum mesajı
                using var statusFont = new Font("Segoe UI", 11f, FontStyle.Italic);
                g.DrawString(_statusMsg, statusFont, Brushes.LightCyan, new RectangleF(0, 182, w, 26), sf);

                // Spinner
                float spinR = 20f;
                float spinX = w / 2f, spinY = 240f;
                for (int i = 0; i < 8; i++)
                {
                    double angle = _time * 4 + i * Math.PI / 4;
                    float ax = spinX + (float)(Math.Cos(angle) * spinR);
                    float ay = spinY + (float)(Math.Sin(angle) * spinR);
                    int alpha = (int)(255 * (i + 1) / 9.0);
                    using var dotBr = new SolidBrush(Color.FromArgb(alpha, Color.CornflowerBlue));
                    g.FillEllipse(dotBr, ax - 4, ay - 4, 8, 8);
                }
            }
        }

        private void DrawStepDone(Graphics g, int w, int h)
        {
            var sf = new StringFormat { Alignment = StringAlignment.Center };
            using var titleFont = new Font("Segoe UI", 22f, FontStyle.Bold);
            float glow = 0.7f + 0.3f * (float)Math.Sin(_time * 3);
            g.DrawString("✅  Kurulum Tamamlandı!", titleFont,
                new SolidBrush(Color.FromArgb((int)(glow * 255), Color.LimeGreen)),
                new RectangleF(0, 84, w, 44), sf);

            using var descFont = new Font("Segoe UI", 12f);
            string msg =
                "KuraBird başarıyla kuruldu!\n\n" +
                "📌  Donanım parmak iziniz kaydedildi.\n" +
                "    Bu paket aynı bilgisayara tekrar kurulamaz.\n\n" +
                "🎮  Oyunu başlangıç menüsünden veya masaüstü\n" +
                "    kısayolundan açabilirsiniz.\n\n" +
                "İyi oyunlar! 🐦";
            g.DrawString(msg, descFont, Brushes.LightCyan, new RectangleF(60, 140, w - 120, 200));

            DrawButton(g, "🚀  KuraBird'i Başlat", new RectangleF(w / 2f - 130, 300, 260, 48),
                Color.FromArgb(160, Color.FromArgb(20, 100, 30)), Color.LimeGreen, Brushes.White);
        }

        private void DrawBottomBar(Graphics g, int w, int h)
        {
            float barY = h - 60f;
            using var barBr = new SolidBrush(Color.FromArgb(80, Color.FromArgb(10, 20, 50)));
            g.FillRectangle(barBr, 0, barY, w, 60);
            using var barPen = new Pen(Color.FromArgb(40, Color.White), 1f);
            g.DrawLine(barPen, 0, barY, w, barY);

            if (_step == 0 && _checkResult != null && _checkResult.IsInstalled) return;
            if (_step == 4)
            {
                DrawButton(g, "✖  Kapat", new RectangleF(w - 140, barY + 10, 120, 40),
                    Color.FromArgb(120, Color.FromArgb(60, 15, 15)), Color.IndianRed, Brushes.White);
                return;
            }
            if (_step < 3)
            {
                if (_step > 0)
                    DrawButton(g, "‹ Geri", new RectangleF(20, barY + 10, 110, 40),
                        Color.FromArgb(100, Color.FromArgb(20, 30, 60)), Color.Gray, Brushes.LightGray);
                DrawButton(g, _step == 2 ? "Kurulumu Başlat ✓" : "İleri ›",
                    new RectangleF(w - 160, barY + 10, 140, 40),
                    Color.FromArgb(160, Color.FromArgb(20, 80, 180)), Color.CornflowerBlue, Brushes.White);
            }
            if (_step == 0 && (_checkResult == null || !_checkResult.IsInstalled))
            {
                DrawButton(g, "✖  İptal", new RectangleF(20, barY + 10, 110, 40),
                    Color.FromArgb(100, Color.FromArgb(60, 10, 10)), Color.IndianRed, Brushes.LightGray);
            }
        }

        private void DrawButton(Graphics g, string text, RectangleF rect, Color bgColor, Color borderColor, Brush textBr)
        {
            using var br = new SolidBrush(bgColor);
            using var pen = new Pen(borderColor, 1.5f);
            var path = RoundPath(rect, 10f);
            g.FillPath(br, path);
            g.DrawPath(pen, path);
            using var btnFont = new Font("Segoe UI", 11.5f, FontStyle.Bold);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(text, btnFont, textBr, rect, sf);
        }

        private System.Drawing.Drawing2D.GraphicsPath RoundPath(RectangleF r, float rad)
        {
            var p = new System.Drawing.Drawing2D.GraphicsPath();
            p.AddArc(r.X, r.Y, rad * 2, rad * 2, 180, 90);
            p.AddArc(r.Right - rad * 2, r.Y, rad * 2, rad * 2, 270, 90);
            p.AddArc(r.Right - rad * 2, r.Bottom - rad * 2, rad * 2, rad * 2, 0, 90);
            p.AddArc(r.X, r.Bottom - rad * 2, rad * 2, rad * 2, 90, 90);
            p.CloseFigure();
            return p;
        }

        private void InstallerForm_MouseClick(object? sender, MouseEventArgs e)
        {
            int w = ClientSize.Width, h = ClientSize.Height;
            float barY = h - 60f;

            // Engellendi → sadece kapat
            if (_step == 0 && _checkResult != null && _checkResult.IsInstalled)
            {
                if (new RectangleF(w / 2f - 90, 310, 180, 44).Contains(e.Location))
                    Application.Exit();
                return;
            }

            // Geri / İptal
            if (_step > 0 && _step < 3 && new RectangleF(20, barY + 10, 110, 40).Contains(e.Location))
            { _step--; return; }
            if (_step == 0 && new RectangleF(20, barY + 10, 110, 40).Contains(e.Location))
                Application.Exit();

            // İleri
            if (_step < 3 && new RectangleF(w - 160, barY + 10, 140, 40).Contains(e.Location))
            {
                if (_step == 2) StartInstallation();
                else _step++;
            }

            // Adım 4: Başlat
            if (_step == 4 && new RectangleF(w / 2f - 130, 300, 260, 48).Contains(e.Location))
                LaunchGame();

            // Kapat
            if (_step == 4 && new RectangleF(w - 140, barY + 10, 120, 40).Contains(e.Location))
                Application.Exit();
        }

        private void StartInstallation()
        {
            _step = 3;
            _progress = 0f;
            _progressTimer = new System.Windows.Forms.Timer { Interval = 80 };

            string[] messages = {
                "Dosyalar hazırlanıyor...",
                "KuraBird.exe kopyalanıyor...",
                "Kısayollar oluşturuluyor...",
                "Donanım parmak izi hesaplanıyor...",
                "Güvenlik kaydı yazılıyor...",
                "Registry güncelleniyor...",
                "Kurulum tamamlanıyor..."
            };
            int msgIdx = 0;

            _progressTimer.Tick += (s, e) =>
            {
                _progress += 0.022f;
                if (_progress > 1f) _progress = 1f;

                int newIdx = (int)(_progress * messages.Length);
                if (newIdx < messages.Length) _statusMsg = messages[newIdx];

                if (_progress >= 0.85f && msgIdx < 5) { msgIdx = 5; PerformInstall(); }
                if (_progress >= 1f)
                {
                    _progressTimer.Stop();
                    _installDone = true;
                    _step = 4;
                }
            };
            _progressTimer.Start();
        }

        private void PerformInstall()
        {
            try
            {
                Directory.CreateDirectory(_installDir);

                // Oyun EXE'sini kopyala (Inno Setup'lı versiyonda bu zaten yapılır)
                string exeSource = Path.Combine(_sourceDir, "KuraBird.exe");
                if (File.Exists(exeSource))
                    File.Copy(exeSource, Path.Combine(_installDir, "KuraBird.exe"), true);

                // Masaüstü kısayolu
                CreateShortcut(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "KuraBird.lnk"),
                    Path.Combine(_installDir, "KuraBird.exe"));

                // Başlangıç menüsü kısayolu
                string startMenuDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                    "Programs", "KuraBird");
                Directory.CreateDirectory(startMenuDir);
                CreateShortcut(Path.Combine(startMenuDir, "KuraBird.lnk"),
                    Path.Combine(_installDir, "KuraBird.exe"));

                // TEKİL KURULUM KAYDINI YAZ
                if (_checkResult?.Hash != null)
                    InstallGuard.MarkAsInstalled(_checkResult.Hash);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kurulum hatası: {ex.Message}\n\nYönetici olarak çalıştırıldığından emin olun.",
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void CreateShortcut(string shortcutPath, string targetPath)
        {
            // WScript.Shell ile .lnk oluştur
            try
            {
                var shell = new object();
                // PowerShell üzerinden kısayol oluştur
                var psCmd = $"$ws = New-Object -ComObject WScript.Shell; " +
                            $"$s = $ws.CreateShortcut('{shortcutPath}'); " +
                            $"$s.TargetPath = '{targetPath}'; " +
                            $"$s.Save()";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command \"{psCmd}\"",
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                })?.WaitForExit(5000);
            }
            catch { }
        }

        private void LaunchGame()
        {
            string gamePath = Path.Combine(_installDir, "KuraBird.exe");
            if (File.Exists(gamePath))
                System.Diagnostics.Process.Start(gamePath);
            Application.Exit();
        }
    }
}
