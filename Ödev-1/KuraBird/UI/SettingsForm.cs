using KuraBird.Managers;

namespace KuraBird.UI
{
    public class SettingsForm : Form
    {
        private System.Windows.Forms.Timer _animTimer;
        private float _time = 0f;

        public SettingsForm()
        {
            this.DoubleBuffered = true;
            this.Text = "Ayarlar – KuraBird";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(480, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(10, 15, 40);
            this.KeyPreview = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };

            _animTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _animTimer.Tick += (s, e) => { _time += 0.016f; Invalidate(); };
            _animTimer.Start();

            BuildControls();
        }

        private void BuildControls()
        {
            int cx = 60;
            int cy = 80;

            // Ses Açık/Kapalı
            var chkSound = new CheckBox
            {
                Text = "🔊  Ses Efektleri",
                Checked = AudioManager.Instance.SoundEnabled,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                Location = new Point(cx, cy),
                AutoSize = true
            };
            chkSound.CheckedChanged += (s, e) => AudioManager.Instance.SetSoundEnabled(chkSound.Checked);

            cy += 55;
            var chkMusic = new CheckBox
            {
                Text = "🎵  Müzik",
                Checked = AudioManager.Instance.MusicEnabled,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                Location = new Point(cx, cy),
                AutoSize = true
            };
            chkMusic.CheckedChanged += (s, e) => AudioManager.Instance.SetMusicEnabled(chkMusic.Checked);

            cy += 55;
            var lblDiff = new Label
            {
                Text = "Zorluk:",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                Location = new Point(cx, cy),
                AutoSize = true
            };

            cy += 34;
            var cmbDiff = new ComboBox
            {
                Location = new Point(cx, cy),
                Size = new Size(280, 30),
                Font = new Font("Segoe UI", 12f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(20, 30, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cmbDiff.Items.AddRange(new[] { "😊  Kolay", "😎  Normal", "💀  Zor" });
            cmbDiff.SelectedIndex = Program.Difficulty;
            cmbDiff.SelectedIndexChanged += (s, e) =>
            {
                Program.Difficulty = cmbDiff.SelectedIndex;
            };

            cy += 55;
            var btnClose = new Button
            {
                Text = "✔  Kaydet & Kapat",
                Location = new Point(cx, cy),
                Size = new Size(200, 46),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 80, 180),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderColor = Color.CornflowerBlue;
            btnClose.Click += (s, e) => { _animTimer.Stop(); this.Close(); };

            this.Controls.AddRange(new Control[] { chkSound, chkMusic, lblDiff, cmbDiff, btnClose });
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int w = ClientSize.Width, h = ClientSize.Height;
            using var bgBr = new LinearGradientBrush(new Rectangle(0, 0, w, h),
                Color.FromArgb(8, 12, 35), Color.FromArgb(15, 40, 80), System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            g.FillRectangle(bgBr, 0, 0, w, h);

            using var titleFont = new Font("Segoe UI", 20f, FontStyle.Bold);
            float glow = 0.7f + 0.3f * (float)Math.Sin(_time * 2.5);
            g.DrawString("⚙  Ayarlar", titleFont, new SolidBrush(Color.FromArgb((int)(glow * 255), Color.White)),
                new PointF(60, 20));
        }
    }
}
