using KuraBird.UI;

namespace KuraBird
{
    internal static class Program
    {
        // Global oyun ayarları (kapsülleme)
        public static int SelectedCharacter { get; set; } = 0;
        public static int Difficulty { get; set; } = 1; // 0=Kolay, 1=Normal, 2=Zor

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainMenuForm());
        }
    }
}
