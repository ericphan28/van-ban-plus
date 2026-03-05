using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIVanBan.Core.Services;

/// <summary>
/// Manifest chứa danh sách văn bản pháp quy trên server
/// </summary>
public class LegalManifest
{
    [JsonPropertyName("version")]
    public int Version { get; set; }
    
    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; } = "";
    
    [JsonPropertyName("documents")]
    public List<LegalDocumentInfo> Documents { get; set; } = new();
    
    [JsonPropertyName("notice")]
    public string Notice { get; set; } = "";
}

/// <summary>
/// Thông tin 1 văn bản pháp quy trên server
/// </summary>
public class LegalDocumentInfo
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";
    
    [JsonPropertyName("version")]
    public int Version { get; set; }
    
    [JsonPropertyName("effective_date")]
    public string EffectiveDate { get; set; } = "";
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = "active";
    
    [JsonPropertyName("data_file")]
    public string DataFile { get; set; } = "";
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
    
    [JsonPropertyName("chapters")]
    public int Chapters { get; set; }
    
    [JsonPropertyName("articles")]
    public int Articles { get; set; }
    
    [JsonPropertyName("appendices")]
    public int Appendices { get; set; }
}

/// <summary>
/// Trạng thái so sánh văn bản pháp quy local vs server
/// </summary>
public class LegalUpdateStatus
{
    public int ServerManifestVersion { get; set; }
    public int LocalManifestVersion { get; set; }
    public string ServerUpdatedAt { get; set; } = "";
    public bool HasUpdate { get; set; }
    public string Notice { get; set; } = "";
    public List<LegalDocumentInfo> AvailableDocuments { get; set; } = new();
    public DateTime LastChecked { get; set; } = DateTime.Now;
}

/// <summary>
/// Service kiểm tra và tải cập nhật pháp quy từ VanBanPlus API.
/// Tương tự TemplateStoreService — lưu manifest version vào local file.
/// Theo NĐ 30/2020/NĐ-CP
/// </summary>
public class LegalUpdateService
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };
    
    private static readonly string _localDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "AIVanBan", "Data", "Legal");
    
    private static readonly string _localManifestPath = Path.Combine(_localDir, "manifest.json");
    
    /// <summary>
    /// URL đến thư mục legal trên VanBanPlus API
    /// </summary>
    private static string GetBaseUrl()
    {
        var settings = AppSettingsService.Load();
        var baseUrl = settings.VanBanPlusApiUrl?.TrimEnd('/') ?? "https://vanbanplus.giakiemso.com";
        return $"{baseUrl}/legal";
    }
    
    /// <summary>
    /// Kiểm tra xem có bản cập nhật pháp quy mới không.
    /// So sánh manifest version trên server vs local.
    /// </summary>
    public async Task<LegalUpdateStatus> CheckForUpdatesAsync()
    {
        var status = new LegalUpdateStatus();
        
        try
        {
            // 1. Tải manifest từ server
            var manifestUrl = $"{GetBaseUrl()}/manifest.json";
            var json = await _httpClient.GetStringAsync(manifestUrl);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            
            var serverManifest = JsonSerializer.Deserialize<LegalManifest>(json, options);
            if (serverManifest == null)
                throw new Exception("Không thể đọc manifest từ server.");
            
            status.ServerManifestVersion = serverManifest.Version;
            status.ServerUpdatedAt = serverManifest.UpdatedAt;
            status.Notice = serverManifest.Notice;
            status.AvailableDocuments = serverManifest.Documents;
            
            // 2. Đọc manifest local (nếu có)
            status.LocalManifestVersion = GetLocalManifestVersion();
            
            // 3. So sánh
            status.HasUpdate = serverManifest.Version > status.LocalManifestVersion;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ LegalUpdateService.CheckForUpdates error: {ex.Message}");
            throw new Exception($"Không thể kết nối server: {ex.Message}");
        }
        
        return status;
    }
    
    /// <summary>
    /// Tải và lưu manifest mới nhất từ server vào local
    /// </summary>
    public async Task<bool> DownloadLatestManifestAsync()
    {
        try
        {
            var manifestUrl = $"{GetBaseUrl()}/manifest.json";
            var json = await _httpClient.GetStringAsync(manifestUrl);
            
            Directory.CreateDirectory(_localDir);
            await File.WriteAllTextAsync(_localManifestPath, json);
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ LegalUpdateService.DownloadManifest error: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Đọc version manifest local (0 nếu chưa tải lần nào)
    /// </summary>
    public static int GetLocalManifestVersion()
    {
        try
        {
            if (!File.Exists(_localManifestPath)) return 0;
            
            var json = File.ReadAllText(_localManifestPath);
            var manifest = JsonSerializer.Deserialize<LegalManifest>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return manifest?.Version ?? 0;
        }
        catch
        {
            return 0;
        }
    }
    
    /// <summary>
    /// Lấy thông tin ngày kiểm tra lần cuối
    /// </summary>
    public static string GetLastCheckedText()
    {
        try
        {
            if (!File.Exists(_localManifestPath)) return "Chưa kiểm tra";
            var lastWrite = File.GetLastWriteTime(_localManifestPath);
            return $"Lần cuối: {lastWrite:dd/MM/yyyy HH:mm}";
        }
        catch
        {
            return "Chưa kiểm tra";
        }
    }
}
