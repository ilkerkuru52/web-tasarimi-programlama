using System.Media;

namespace KuraBird.Managers
{
    /// <summary>
    /// Kapsülleme (Encapsulation) + Singleton Pattern: Ses yönetim sınıfı.
    /// Tüm ses işlemleri bu sınıf üzerinden yönetilir, dışarıya sadece gerekli API açılır.
    /// </summary>
    public class AudioManager
    {
        // Singleton implementasyonu
        private static AudioManager? _instance;
        public static AudioManager Instance => _instance ??= new AudioManager();

        private Dictionary<string, byte[]> _sounds = new();
        private bool _soundEnabled = true;
        private bool _musicEnabled = true;
        private float _volume = 1.0f;

        private System.Threading.Thread? _musicThread;
        private bool _musicRunning = false;
        private string _currentMusic = "";

        // Kapsülleme: dışarıdan salt okunur
        public bool SoundEnabled => _soundEnabled;
        public bool MusicEnabled => _musicEnabled;
        public float Volume => _volume;

        private AudioManager()
        {
            GenerateAllSounds();
        }

        public void SetSoundEnabled(bool enabled) => _soundEnabled = enabled;
        public void SetMusicEnabled(bool enabled)
        {
            _musicEnabled = enabled;
            if (!enabled) StopMusic();
        }
        public void SetVolume(float v) => _volume = Math.Clamp(v, 0f, 1f);

        /// <summary>
        /// Belirtilen sesi çalar.
        /// </summary>
        public void Play(string name)
        {
            if (!_soundEnabled) return;
            if (!_sounds.TryGetValue(name, out var data)) return;
            Task.Run(() =>
            {
                try
                {
                    using var ms = new System.IO.MemoryStream(data);
                    using var player = new SoundPlayer(ms);
                    player.PlaySync();
                }
                catch { /* ses hatalarını sessizce geç */ }
            });
        }

        public void StopMusic()
        {
            _musicRunning = false;
        }

        public void PlayLoopingMusic(string name)
        {
            if (!_musicEnabled) return;
            if (_currentMusic == name) return;
            _musicRunning = false;
            _currentMusic = name;
            _musicThread = new System.Threading.Thread(() =>
            {
                _musicRunning = true;
                while (_musicRunning && _musicEnabled)
                {
                    if (_sounds.TryGetValue(name, out var data))
                    {
                        using var ms = new System.IO.MemoryStream(data);
                        using var player = new SoundPlayer(ms);
                        player.PlaySync();
                    }
                    System.Threading.Thread.Sleep(100);
                }
            }) { IsBackground = true };
            _musicThread.Start();
        }

        /// <summary>
        /// PCM WAV ses verilerini programatik olarak üretir.
        /// </summary>
        private void GenerateAllSounds()
        {
            _sounds["flap"] = GenerateBeepWav(660, 0.06, 0.5);
            _sounds["hit"] = GenerateNoiseWav(0.15, 0.8);
            _sounds["score"] = GenerateBeepWav(880, 0.08, 0.4);
            _sounds["powerup"] = GenerateSweepWav(400, 900, 0.2, 0.5);
            _sounds["gameover"] = GenerateSweepWav(440, 200, 0.5, 0.6);
            _sounds["click"] = GenerateBeepWav(440, 0.04, 0.3);
            _sounds["menu_music"] = GenerateMelodyWav();
        }

        private static byte[] GenerateBeepWav(double freq, double duration, double amplitude)
        {
            int sampleRate = 22050;
            int numSamples = (int)(sampleRate * duration);
            using var ms = new System.IO.MemoryStream();
            using var bw = new System.IO.BinaryWriter(ms);
            WriteWavHeader(bw, numSamples, sampleRate);
            for (int i = 0; i < numSamples; i++)
            {
                double t = i / (double)sampleRate;
                double env = Math.Exp(-t * 12.0);
                double val = Math.Sin(2 * Math.PI * freq * t) * amplitude * env;
                bw.Write((short)(val * short.MaxValue));
            }
            return ms.ToArray();
        }

        private static byte[] GenerateNoiseWav(double duration, double amplitude)
        {
            int sampleRate = 22050;
            int numSamples = (int)(sampleRate * duration);
            var rng = new Random();
            using var ms = new System.IO.MemoryStream();
            using var bw = new System.IO.BinaryWriter(ms);
            WriteWavHeader(bw, numSamples, sampleRate);
            for (int i = 0; i < numSamples; i++)
            {
                double t = i / (double)sampleRate;
                double env = Math.Exp(-t * 8.0);
                double val = (rng.NextDouble() * 2 - 1) * amplitude * env;
                bw.Write((short)(val * short.MaxValue));
            }
            return ms.ToArray();
        }

        private static byte[] GenerateSweepWav(double startFreq, double endFreq, double duration, double amplitude)
        {
            int sampleRate = 22050;
            int numSamples = (int)(sampleRate * duration);
            using var ms = new System.IO.MemoryStream();
            using var bw = new System.IO.BinaryWriter(ms);
            WriteWavHeader(bw, numSamples, sampleRate);
            double phase = 0;
            for (int i = 0; i < numSamples; i++)
            {
                double t = i / (double)numSamples;
                double freq = startFreq + (endFreq - startFreq) * t;
                double env = Math.Sin(t * Math.PI);
                phase += 2 * Math.PI * freq / sampleRate;
                double val = Math.Sin(phase) * amplitude * env;
                bw.Write((short)(val * short.MaxValue));
            }
            return ms.ToArray();
        }

        private static byte[] GenerateMelodyWav()
        {
            double[] notes = { 523, 587, 659, 698, 784, 659, 523, 0, 523, 659, 784, 880 };
            double noteDur = 0.2;
            int sampleRate = 22050;
            using var ms = new System.IO.MemoryStream();
            using var bw = new System.IO.BinaryWriter(ms);
            int total = (int)(sampleRate * noteDur * notes.Length);
            WriteWavHeader(bw, total, sampleRate);
            foreach (var freq in notes)
            {
                int n = (int)(sampleRate * noteDur);
                for (int i = 0; i < n; i++)
                {
                    double t = i / (double)sampleRate;
                    double env = Math.Sin(t / noteDur * Math.PI);
                    double val = freq > 0 ? Math.Sin(2 * Math.PI * freq * t) * 0.4 * env : 0;
                    bw.Write((short)(val * short.MaxValue));
                }
            }
            return ms.ToArray();
        }

        private static void WriteWavHeader(System.IO.BinaryWriter bw, int numSamples, int sampleRate)
        {
            int byteRate = sampleRate * 2;
            int blockAlign = 2;
            int subChunkSize = numSamples * 2;
            bw.Write(new char[] { 'R', 'I', 'F', 'F' });
            bw.Write(36 + subChunkSize);
            bw.Write(new char[] { 'W', 'A', 'V', 'E' });
            bw.Write(new char[] { 'f', 'm', 't', ' ' });
            bw.Write(16); bw.Write((short)1); bw.Write((short)1);
            bw.Write(sampleRate); bw.Write(byteRate); bw.Write((short)blockAlign);
            bw.Write((short)16);
            bw.Write(new char[] { 'd', 'a', 't', 'a' });
            bw.Write(subChunkSize);
        }
    }
}
