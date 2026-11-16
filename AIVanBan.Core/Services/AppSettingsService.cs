using System.IO;
using System.Text.Json;
using AIVanBan.Core.Models;

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
    /// Kiểm tra xem AI đã sẵn sàng sử dụng chưa (đã bật + có API key)
    /// </summary>
    public static bool IsAiReady()
    {
        var s = Load();
        if (!s.AiEnabled) return false;
        var key = GetEffectiveApiKey();
        return !string.IsNullOrWhiteSpace(key);
    }

    /// <summary>Khóa API VanBanPlus hiện tại (bản cài máy chỉ gọi AI qua máy chủ VanBanPlus).</summary>
    public static string GetEffectiveApiKey() => Load().VanBanPlusApiKey;

    /// <summary>Alias cho Load()</summary>
    public static AppSettings GetSettings() => Load();

    /// <summary>
    /// Đọc giá trị raw setting theo key (dùng cho nested JSON settings như CloudSync).
    /// Internally đọc từ ExtraSettings dictionary.
    /// </summary>
    public static string? GetRawSettingValue(string key)
    {
        var settings = Load();
        return settings.ExtraSettings.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Ghi giá trị raw setting theo key.
    /// </summary>
    public static void SetRawSettingValue(string key, string value)
    {
        var settings = Load();
        settings.ExtraSettings[key] = value;
        Save(settings);
    }
}

public class AppSettings
{
    // ===== Bật/tắt tính năng AI =====
    /// <summary>
    /// Mặc định false = tính năng AI chưa kích hoạt. Bật khi người dùng đăng ký và cấu hình API.
    /// </summary>
    public bool AiEnabled { get; set; } = false;

    // ===== VanBanPlus API (cloud) =====
    public string VanBanPlusApiUrl { get; set; } = "https://vanbanplus.giakiemso.com";
    public string VanBanPlusApiKey { get; set; } = "";
    /// <summary>
    /// Token bypass Vercel Deployment Protection (Hobby plan không tắt được)
    /// </summary>
    public string VercelBypassToken { get; set; } = "O6ZwqggP5r8buGm985jUItBVbT8qIZQC";

    // ===== Thông tin người dùng (cache từ API) =====
    public string UserEmail { get; set; } = "";
    public string UserFullName { get; set; } = "";
    public string UserPlan { get; set; } = "";
    public string UserRole { get; set; } = "user";

    // ===== Cloud Sync =====
    /// <summary>
    /// Cấu hình Cloud Sync — đồng bộ local ↔ Supabase.
    /// </summary>
    public CloudSyncSettings CloudSync { get; set; } = new();

    // ===== Extra Settings (extensible) =====
    /// <summary>
    /// Dictionary lưu trữ settings mở rộng (JSON string values).
    /// Dùng cho các tính năng mới mà không cần thay đổi schema.
    /// </summary>
    public Dictionary<string, string> ExtraSettings { get; set; } = new();
}
