using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace KuraBirdInstaller
{
    /// <summary>
    /// Tekil Kurulum Kontrol Mekanizması.
    /// 
    /// ÇALIŞMA PRENSİBİ:
    /// 1. Kurulum sırasında makinenin donanım parmak izi (SHA-256 hash) hesaplanır.
    /// 2. Bu hash şifrelenmiş olarak iki farklı yere kaydedilir:
    ///    a) Windows Registry → HKLM\SOFTWARE\KuraBird\InstallID
    ///    b) Gizli dosya    → C:\ProgramData\KuraBird\install.sig
    /// 3. Kurulum paketi tekrar çalışacak olursa, o anki donanım hash'i
    ///    kayıtlı hash ile karşılaştırılır.
    /// 4. Eşleşirse → KURULUM ENGELLENİR (oyun silinmiş olsa bile).
    /// 5. Registry silinse bile .sig dosyası kontrolü devam eder (çift güvence).
    /// </summary>
    public static class InstallGuard
    {
        private const string RegistryPath = @"SOFTWARE\KuraBird";
        private const string RegistryKey   = "InstallID";
        private const string SigDir        = @"C:\ProgramData\KuraBird";
        private const string SigFile       = @"C:\ProgramData\KuraBird\install.sig";

        // AES şifreleme için sabit anahtar (gerçek projede gizlenmiş/karıştırılmış olur)
        private static readonly byte[] _aesKey = Encoding.UTF8.GetBytes("KuraBirdAES256K!KuraBirdAES256K!"); // 32 byte
        private static readonly byte[] _aesIV  = Encoding.UTF8.GetBytes("KuraBirdIV123456"); // 16 byte

        /// <summary>
        /// Bu bilgisayarda daha önce kurulum yapılmış mı kontrol eder.
        /// </summary>
        public static CheckResult CheckInstallStatus()
        {
            string currentHash = HardwareFingerprint.Generate();

            // — Kontrol 1: .sig dosyası —
            if (File.Exists(SigFile))
            {
                try
                {
                    string storedHash = DecryptFromFile(SigFile);
                    if (storedHash == currentHash)
                        return CheckResult.AlreadyInstalled("Dosya kaydı eşleşti.");
                }
                catch
                {
                    // Dosya bozuk → registry'e bak
                }
            }

            // — Kontrol 2: Registry —
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(RegistryPath);
                if (key != null)
                {
                    string? storedEncrypted = key.GetValue(RegistryKey) as string;
                    if (!string.IsNullOrEmpty(storedEncrypted))
                    {
                        string storedHash = Decrypt(storedEncrypted);
                        if (storedHash == currentHash)
                        {
                            // Registry eşleşti, .sig'i yeniden yaz (onarım)
                            WriteSignatureFile(currentHash);
                            return CheckResult.AlreadyInstalled("Registry kaydı eşleşti.");
                        }
                    }
                }
            }
            catch { }

            return CheckResult.NotInstalled(currentHash);
        }

        /// <summary>
        /// Kurulum başarılı olunca çağrılır. Hash'i her iki konuma yazar.
        /// </summary>
        public static void MarkAsInstalled(string hash)
        {
            WriteRegistryEntry(hash);
            WriteSignatureFile(hash);
        }

        private static void WriteRegistryEntry(string hash)
        {
            using var key = Registry.LocalMachine.CreateSubKey(RegistryPath, writable: true);
            if (key != null)
            {
                key.SetValue(RegistryKey, Encrypt(hash));
                key.SetValue("InstallDate", DateTime.UtcNow.ToString("o"));
                key.SetValue("Version", "1.0.0");
            }
        }

        private static void WriteSignatureFile(string hash)
        {
            Directory.CreateDirectory(SigDir);
            EncryptToFile(hash, SigFile);

            // Gizli + sistem dosyası → normal kullanıcı göremez/silemez (kolayca)
            File.SetAttributes(SigFile,
                FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly);
        }

        // ── Şifreleme / Şifre Çözme ──────────────────────────────────────────

        private static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _aesKey; aes.IV = _aesIV;
            using var encryptor = aes.CreateEncryptor();
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
            return Convert.ToBase64String(encrypted);
        }

        private static string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = _aesKey; aes.IV = _aesIV;
            using var decryptor = aes.CreateDecryptor();
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] decrypted = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(decrypted);
        }

        private static void EncryptToFile(string plainText, string path)
        {
            // ReadOnly var ise kaldır, yaz, tekrar ReadOnly yap
            if (File.Exists(path))
                File.SetAttributes(path, FileAttributes.Normal);
            File.WriteAllText(path, Encrypt(plainText), Encoding.UTF8);
        }

        private static string DecryptFromFile(string path)
        {
            // Geçici olarak normal yap, oku, geri koy
            var attrs = File.GetAttributes(path);
            if ((attrs & FileAttributes.ReadOnly) != 0)
                File.SetAttributes(path, attrs & ~FileAttributes.ReadOnly);
            string content = File.ReadAllText(path, Encoding.UTF8).Trim();
            File.SetAttributes(path, attrs); // eski attribute'ları geri koy
            return Decrypt(content);
        }
    }

    public class CheckResult
    {
        public bool IsInstalled { get; }
        public string Reason { get; }
        public string? Hash { get; }

        private CheckResult(bool installed, string reason, string? hash)
        {
            IsInstalled = installed; Reason = reason; Hash = hash;
        }

        public static CheckResult AlreadyInstalled(string reason) =>
            new(true, reason, null);
        public static CheckResult NotInstalled(string hash) =>
            new(false, "İlk kurulum.", hash);
    }
}
