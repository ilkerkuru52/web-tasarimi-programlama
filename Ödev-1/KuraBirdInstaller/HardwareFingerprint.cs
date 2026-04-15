using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace KuraBirdInstaller
{
    /// <summary>
    /// Donanım Parmak İzi Üretici.
    /// CPU ID + Anakart SN + MAC Adresi kombinasyonu → SHA-256 hash.
    /// Bu hash her zaman aynı makinede aynı sonucu verir.
    /// </summary>
    public static class HardwareFingerprint
    {
        public static string Generate()
        {
            string cpu = GetCpuId();
            string board = GetMotherboardSerial();
            string mac = GetMacAddress();

            // Ham metin: tüm donanım bilgileri birleştirildi
            string raw = $"KURA|{cpu}|{board}|{mac}|BIRD";

            // SHA-256 ile hash
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes); // 64 karakter hex string
        }

        private static string GetCpuId()
        {
            try
            {
                using var mos = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (ManagementObject mo in mos.Get())
                    if (mo["ProcessorId"] is string pid && !string.IsNullOrWhiteSpace(pid))
                        return pid.Trim();
            }
            catch { }
            return "UNKNOWN_CPU";
        }

        private static string GetMotherboardSerial()
        {
            try
            {
                using var mos = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                foreach (ManagementObject mo in mos.Get())
                    if (mo["SerialNumber"] is string sn && !string.IsNullOrWhiteSpace(sn))
                        return sn.Trim();
            }
            catch { }
            return "UNKNOWN_BOARD";
        }

        private static string GetMacAddress()
        {
            try
            {
                // Fiziksel NIC'leri filtrele, en büyük hız = birincil adaptör
                var nics = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback
                             && n.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                             && n.OperationalStatus == OperationalStatus.Up)
                    .OrderByDescending(n => n.Speed)
                    .ToList();

                if (nics.Count > 0)
                    return nics[0].GetPhysicalAddress().ToString();
            }
            catch { }
            return "UNKNOWN_MAC";
        }
    }
}
