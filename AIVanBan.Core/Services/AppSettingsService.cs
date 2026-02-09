using System.IO;
using System.Text.Json;

namespace AIVanBan.Core.Services;

/// <summary>
/// Quản lý cấu hình ứng dụng — lưu trữ API settings vào file JSON.
/// </summary>
public class AppSettingsService
{
    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AIVanBan");
    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    private static AppSettings? _cached;
    private static readonly object _lock = new();

    /// <summary>
    /// Đọc settings (có cache)
    /// </summary>
    public static AppSettings Load()
    {
        lock (_lock)
        {
            if (_cached != null) return _cached;

            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    _cached = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    _cached = new AppSettings();
                }
            }
            catch
            {
                _cached = new AppSettings();
            }

            return _cached;
        }
    }

    /// <summary>
    /// Lưu settings
    /// </summary>
    public static void Save(AppSettings settings)
    {
        lock (_lock)
        {
            _cached = settings;
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(SettingsPath, json);
        }
    }

    /// <summary>
    /// Lấy API Key hiện tại (ưu tiên VanBanPlus, fallback Gemini trực tiếp)
    /// </summary>
    public static string GetEffectiveApiKey()
    {
        var s = Load();
        return s.UseVanBanPlusApi && !string.IsNullOrEmpty(s.VanBanPlusApiKey)
            ? s.VanBanPlusApiKey
            : s.GeminiApiKey;
    }
}

/// <summary>
/// Cấu hình ứng dụng
/// </summary>
public class AppSettings
{
    // ===== Chế độ API =====
    /// <summary>
    /// true = Gọi qua VanBanPlus API (khuyến nghị), false = Gọi Gemini trực tiếp
    /// </summary>
    public bool UseVanBanPlusApi { get; set; } = true;

    // ===== VanBanPlus API (cloud) =====
    public string VanBanPlusApiUrl { get; set; } = "https://vanbanplus-api-git-main-ericphan28s-projects.vercel.app";
    public string VanBanPlusApiKey { get; set; } = "";
    /// <summary>
    /// Token bypass Vercel Deployment Protection (Hobby plan không tắt được)
    /// </summary>
    public string VercelBypassToken { get; set; } = "O6ZwqggP5r8buGm985jUItBVbT8qIZQC";

    // ===== Gemini trực tiếp (legacy) =====
    public string GeminiApiKey { get; set; } = "";

    // ===== Thông tin người dùng (cache từ API) =====
    public string UserEmail { get; set; } = "";
    public string UserFullName { get; set; } = "";
    public string UserPlan { get; set; } = "";
    public string UserRole { get; set; } = "user";
}
