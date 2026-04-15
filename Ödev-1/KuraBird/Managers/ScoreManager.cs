using Newtonsoft.Json;

namespace KuraBird.Managers
{
    /// <summary>
    /// Kapsülleme (Encapsulation): Skor verisi private tutulur, dışarıya sadece gerekli metotlar açılır.
    /// </summary>
    public class ScoreManager
    {
        private static ScoreManager? _instance;
        public static ScoreManager Instance => _instance ??= new ScoreManager();

        private int _currentScore;
        private int _highScore;
        private List<ScoreEntry> _leaderboard = new();
        private readonly string _savePath;

        public int CurrentScore => _currentScore;
        public int HighScore => _highScore;
        public IReadOnlyList<ScoreEntry> Leaderboard => _leaderboard.AsReadOnly();

        private ScoreManager()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dir = System.IO.Path.Combine(appData, "KuraBird");
            System.IO.Directory.CreateDirectory(dir);
            _savePath = System.IO.Path.Combine(dir, "scores.json");
            LoadScores();
        }

        public void ResetScore() => _currentScore = 0;

        public void AddScore(int points = 1)
        {
            _currentScore += points;
            if (_currentScore > _highScore) _highScore = _currentScore;
        }

        public void SaveScore(string playerName = "Oyuncu")
        {
            _leaderboard.Add(new ScoreEntry(playerName, _currentScore, DateTime.Now));
            _leaderboard = _leaderboard.OrderByDescending(e => e.Score).Take(10).ToList();
            SaveScores();
        }

        private void LoadScores()
        {
            try
            {
                if (System.IO.File.Exists(_savePath))
                {
                    string json = System.IO.File.ReadAllText(_savePath);
                    var data = JsonConvert.DeserializeObject<SaveData>(json);
                    if (data != null)
                    {
                        _highScore = data.HighScore;
                        _leaderboard = data.Entries ?? new List<ScoreEntry>();
                    }
                }
            }
            catch { /* bozuk dosya → sıfırla */ }
        }

        private void SaveScores()
        {
            try
            {
                var data = new SaveData { HighScore = _highScore, Entries = _leaderboard };
                System.IO.File.WriteAllText(_savePath, JsonConvert.SerializeObject(data, Formatting.Indented));
            }
            catch { }
        }

        private class SaveData
        {
            public int HighScore { get; set; }
            public List<ScoreEntry>? Entries { get; set; }
        }
    }

    public record ScoreEntry(string Name, int Score, DateTime Date);
}
