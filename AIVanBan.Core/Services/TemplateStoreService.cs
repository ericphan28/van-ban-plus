using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Mô hình 1 template trên Store (JSON từ server)
/// </summary>
public class StoreTemplate
{
    [JsonPropertyName("store_id")]
    public string StoreId { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // "CongVan", "QuyetDinh", ...
    
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("template_content")]
    public string TemplateContent { get; set; } = string.Empty;
    
    [JsonPropertyName("ai_prompt")]
    public string AIPrompt { get; set; } = string.Empty;
    
    [JsonPropertyName("required_fields")]
    public string[] RequiredFields { get; set; } = Array.Empty<string>();
    
    [JsonPropertyName("tags")]
    public string[] Tags { get; set; } = Array.Empty<string>();
    
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;
    
    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;
    
    [JsonPropertyName("author")]
    public string Author { get; set; } = "VanBanPlus";
    
    [JsonPropertyName("is_new")]
    public bool IsNew { get; set; }
    
    [JsonPropertyName("is_popular")]
    public bool IsPopular { get; set; }
}

/// <summary>
/// Response wrapper từ store JSON
/// </summary>
public class TemplateStoreResponse
{
    [JsonPropertyName("store_version")]
    public int StoreVersion { get; set; }
    
    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;
    
    [JsonPropertyName("templates")]
    public List<StoreTemplate> Templates { get; set; } = new();
}

/// <summary>
/// Trạng thái template so với local
/// </summary>
public enum StoreTemplateStatus
{
    /// <summary>Chưa tải về</summary>
    NotDownloaded,
    /// <summary>Đã tải, đang dùng phiên bản mới nhất</summary>
    UpToDate,
    /// <summary>Có bản cập nhật mới trên store</summary>
    UpdateAvailable
}

/// <summary>
/// ViewModel cho hiển thị trên UI
/// </summary>
public class StoreTemplateViewModel
{
    public StoreTemplate Template { get; set; } = null!;
    public StoreTemplateStatus Status { get; set; }
    public int LocalVersion { get; set; }
    
    // Display helpers
    public string StatusText => Status switch
    {
        StoreTemplateStatus.NotDownloaded => "Tải về",
        StoreTemplateStatus.UpToDate => "✓ Đã có",
        StoreTemplateStatus.UpdateAvailable => "⬆ Cập nhật",
        _ => ""
    };
    
    public string StatusColor => Status switch
    {
        StoreTemplateStatus.NotDownloaded => "#1976D2",
        StoreTemplateStatus.UpToDate => "#4CAF50",
        StoreTemplateStatus.UpdateAvailable => "#FF9800",
        _ => "#757575"
    };
    
    public bool CanDownload => Status != StoreTemplateStatus.UpToDate;
    
    public string TypeDisplay => Template.Type;
    public string CategoryDisplay => Template.Category;
    public string VersionDisplay => $"v{Template.Version}";
    
    public string BadgeText
    {
        get
        {
            if (Template.IsNew) return "🆕 MỚI";
            if (Template.IsPopular) return "⭐ PHỔ BIẾN";
            return "";
        }
    }
}

/// <summary>
/// Service quản lý kho mẫu văn bản online (Template Store).
/// Tải danh sách từ VanBanPlus API, so sánh với local LiteDB, cho phép download/update.
/// </summary>
public class TemplateStoreService
{
    // URL chính tới JSON store trên VanBanPlus API (Vercel)
    private static string GetStoreUrl()
    {
        var settings = AppSettingsService.Load();
        var baseUrl = settings.VanBanPlusApiUrl?.TrimEnd('/') ?? "https://vanbanplus.giakiemso.com";
        return $"{baseUrl}/template-store.json";
    }
    
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };
    
    private readonly DocumentService _documentService;
    
    public TemplateStoreService(DocumentService documentService)
    {
        _documentService = documentService;
    }
    
    /// <summary>
    /// Tải danh sách template từ store online, fallback sang local nếu lỗi mạng
    /// </summary>
    public async Task<List<StoreTemplateViewModel>> FetchStoreTemplatesAsync()
    {
        string json;
        
        try
        {
            json = await _httpClient.GetStringAsync(GetStoreUrl());
        }
        catch (Exception)
        {
            // Fallback: đọc từ file local nếu không kết nối được server
            json = TryLoadLocalStoreJson();
            if (string.IsNullOrEmpty(json))
                throw new Exception("Không thể kết nối kho mẫu online và không tìm thấy dữ liệu local.");
        }
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
        
        var response = JsonSerializer.Deserialize<TemplateStoreResponse>(json, options)
            ?? throw new Exception("Không thể đọc dữ liệu từ kho mẫu.");
        
        // Lấy tất cả template local để so sánh
        var localTemplates = _documentService.GetAllTemplates();
        
        var result = new List<StoreTemplateViewModel>();
        
        foreach (var storeItem in response.Templates)
        {
            // Tìm trong local bằng StoreId
            var local = localTemplates.FirstOrDefault(t => t.StoreId == storeItem.StoreId);
            
            StoreTemplateStatus status;
            int localVersion = 0;
            
            if (local == null)
            {
                status = StoreTemplateStatus.NotDownloaded;
            }
            else
            {
                localVersion = local.StoreVersion;
                status = local.StoreVersion >= storeItem.Version
                    ? StoreTemplateStatus.UpToDate
                    : StoreTemplateStatus.UpdateAvailable;
            }
            
            result.Add(new StoreTemplateViewModel
            {
                Template = storeItem,
                Status = status,
                LocalVersion = localVersion
            });
        }
        
        return result;
    }
    
    /// <summary>
    /// Fallback: Đọc template-store.json từ thư mục cài đặt app
    /// </summary>
    private static string TryLoadLocalStoreJson()
    {
        try
        {
            // Tìm file template-store.json cạnh executable
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var localPath = Path.Combine(appDir, "template-store.json");
            
            if (File.Exists(localPath))
                return File.ReadAllText(localPath);
            
            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
    
    /// <summary>
    /// Tải 1 template từ store về local LiteDB
    /// </summary>
    public DocumentTemplate DownloadTemplate(StoreTemplate storeTemplate)
    {
        // Kiểm tra đã có chưa (bằng StoreId)
        var localTemplates = _documentService.GetAllTemplates();
        var existing = localTemplates.FirstOrDefault(t => t.StoreId == storeTemplate.StoreId);
        
        // Parse DocumentType
        if (!Enum.TryParse<DocumentType>(storeTemplate.Type, true, out var docType))
            docType = DocumentType.CongVan;
        
        if (existing != null)
        {
            // Update existing
            existing.Name = storeTemplate.Name;
            existing.Type = docType;
            existing.Category = storeTemplate.Category;
            existing.Description = storeTemplate.Description;
            existing.TemplateContent = storeTemplate.TemplateContent;
            existing.AIPrompt = storeTemplate.AIPrompt;
            existing.RequiredFields = storeTemplate.RequiredFields;
            existing.Tags = storeTemplate.Tags;
            existing.StoreVersion = storeTemplate.Version;
            existing.ModifiedDate = DateTime.Now;
            
            _documentService.UpdateTemplate(existing);
            return existing;
        }
        else
        {
            // Insert new
            var newTemplate = new DocumentTemplate
            {
                Name = storeTemplate.Name,
                Type = docType,
                Category = storeTemplate.Category,
                Description = storeTemplate.Description,
                TemplateContent = storeTemplate.TemplateContent,
                AIPrompt = storeTemplate.AIPrompt,
                RequiredFields = storeTemplate.RequiredFields,
                Tags = storeTemplate.Tags,
                StoreId = storeTemplate.StoreId,
                StoreVersion = storeTemplate.Version,
                CreatedBy = "Template Store"
            };
            
            _documentService.AddTemplate(newTemplate);
            return newTemplate;
        }
    }
    
    /// <summary>
    /// Tải tất cả template mới/cần update từ store
    /// </summary>
    public async Task<int> DownloadAllNewAsync()
    {
        var storeItems = await FetchStoreTemplatesAsync();
        int count = 0;
        
        foreach (var item in storeItems)
        {
            if (item.Status != StoreTemplateStatus.UpToDate)
            {
                DownloadTemplate(item.Template);
                count++;
            }
        }
        
        return count;
    }
}
