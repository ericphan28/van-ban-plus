using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Dữ liệu album từ API xa-gia-kiem (remote)
/// </summary>
public class RemoteAlbum
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }
    
    [JsonPropertyName("cover_url")]
    public string? CoverUrl { get; set; }
    
    [JsonPropertyName("image_count")]
    public int ImageCount { get; set; }
    
    [JsonPropertyName("event_title")]
    public string? EventTitle { get; set; }
    
    [JsonPropertyName("event_date")]
    public string? EventDate { get; set; }
    
    [JsonPropertyName("organization_name")]
    public string? OrganizationName { get; set; }
    
    [JsonPropertyName("organization_id")]
    public string? OrganizationId { get; set; }
}

/// <summary>
/// Dữ liệu ảnh trong album remote
/// </summary>
public class RemoteAlbumImage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    
    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; } = "";
    
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("display_order")]
    public int DisplayOrder { get; set; }
    
    [JsonPropertyName("is_cover")]
    public bool IsCover { get; set; }
    
    [JsonPropertyName("size_bytes")]
    public long SizeBytes { get; set; }
}

/// <summary>
/// Chi tiết album remote (bao gồm danh sách ảnh)
/// </summary>
public class RemoteAlbumDetail
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }
    
    [JsonPropertyName("cover_url")]
    public string? CoverUrl { get; set; }
    
    [JsonPropertyName("event_title")]
    public string? EventTitle { get; set; }
    
    [JsonPropertyName("event_date")]
    public string? EventDate { get; set; }
    
    [JsonPropertyName("organization_name")]
    public string? OrganizationName { get; set; }
    
    [JsonPropertyName("image_count")]
    public int ImageCount { get; set; }
    
    [JsonPropertyName("images")]
    public List<RemoteAlbumImage> Images { get; set; } = new();
}

/// <summary>
/// Phản hồi phân trang từ API
/// </summary>
public class RemoteAlbumListResponse
{
    [JsonPropertyName("items")]
    public List<RemoteAlbum> Items { get; set; } = new();
    
    [JsonPropertyName("pagination")]
    public RemotePagination Pagination { get; set; } = new();
}

public class RemotePagination
{
    [JsonPropertyName("page")]
    public int Page { get; set; }
    
    [JsonPropertyName("limit")]
    public int Limit { get; set; }
    
    [JsonPropertyName("total")]
    public int Total { get; set; }
    
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
    
    [JsonPropertyName("hasNext")]
    public bool HasNext { get; set; }
    
    [JsonPropertyName("hasPrev")]
    public bool HasPrev { get; set; }
}

/// <summary>
/// Tiến trình tải album
/// </summary>
public class AlbumDownloadProgress
{
    public string AlbumId { get; set; } = "";
    public string AlbumTitle { get; set; } = "";
    public int TotalImages { get; set; }
    public int DownloadedImages { get; set; }
    public int FailedImages { get; set; }
    public string CurrentImageName { get; set; } = "";
    public bool IsCompleted { get; set; }
    public bool IsCancelled { get; set; }
    public string? ErrorMessage { get; set; }
    
    public double ProgressPercent => TotalImages > 0 
        ? (double)(DownloadedImages + FailedImages) / TotalImages * 100 
        : 0;
}

/// <summary>
/// Service tải album ảnh từ website xã Gia Kiệm (xagiakiem.gov.vn)
/// Kết nối qua Public API, tải ảnh về lưu local dưới dạng SimpleAlbum
/// </summary>
public class AlbumDownloadService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly SimpleAlbumService _albumService;
    private readonly string _baseApiUrl;
    
    /// <summary>
    /// Số album tối đa mỗi lần tải
    /// </summary>
    public const int MaxAlbumsPerDownload = 10;

    public AlbumDownloadService(
        SimpleAlbumService albumService,
        string baseUrl = "https://xagiakiem.gov.vn")
    {
        _albumService = albumService;
        _baseApiUrl = baseUrl.TrimEnd('/') + "/api/albums";
        
        _httpClient = new HttpClient(new SocketsHttpHandler
        {
            ConnectTimeout = TimeSpan.FromSeconds(15),
            EnableMultipleHttp2Connections = true
        })
        {
            Timeout = TimeSpan.FromSeconds(60)
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "AIVanBan-Desktop/1.0");
    }

    /// <summary>
    /// Lấy danh sách album công khai từ server (có phân trang)
    /// </summary>
    public async Task<RemoteAlbumListResponse> GetRemoteAlbumsAsync(
        int page = 1, 
        int limit = 20, 
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_baseApiUrl}?page={page}&limit={limit}";
        
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<RemoteAlbumListResponse>(json);
        
        return result ?? new RemoteAlbumListResponse();
    }

    /// <summary>
    /// Lấy chi tiết album (bao gồm danh sách ảnh) từ server
    /// </summary>
    public async Task<RemoteAlbumDetail?> GetRemoteAlbumDetailAsync(
        string albumId, 
        CancellationToken cancellationToken = default)
    {
        var url = $"{_baseApiUrl}/{albumId}";
        
        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
            
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<RemoteAlbumDetail>(json);
    }

    /// <summary>
    /// Tải album về local (tạo SimpleAlbum + tải tất cả ảnh)
    /// </summary>
    public async Task<SimpleAlbum?> DownloadAlbumAsync(
        string remoteAlbumId,
        IProgress<AlbumDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var downloadProgress = new AlbumDownloadProgress { AlbumId = remoteAlbumId };

        try
        {
            // 1. Lấy chi tiết album từ server
            var detail = await GetRemoteAlbumDetailAsync(remoteAlbumId, cancellationToken);
            if (detail == null)
            {
                downloadProgress.ErrorMessage = "Không tìm thấy album trên server";
                downloadProgress.IsCompleted = true;
                progress?.Report(downloadProgress);
                return null;
            }

            downloadProgress.AlbumTitle = detail.Title;
            downloadProgress.TotalImages = detail.Images.Count;
            progress?.Report(downloadProgress);

            // 2. Tạo album local
            var tags = new List<string> { "Tải từ website", "xagiakiem.gov.vn" };
            if (!string.IsNullOrEmpty(detail.OrganizationName))
                tags.Add(detail.OrganizationName);
            if (!string.IsNullOrEmpty(detail.EventTitle))
                tags.Add(detail.EventTitle);

            var album = _albumService.CreateAlbum(
                title: detail.Title,
                description: BuildAlbumDescription(detail),
                tags: tags
            );

            // Set EventDate from remote event
            if (!string.IsNullOrEmpty(detail.EventDate) && DateTime.TryParse(detail.EventDate, out var eventDt))
            {
                album.EventDate = eventDt;
                _albumService.UpdateAlbum(album);
            }

            // 3. Tải từng ảnh về
            string? coverPhotoPath = null;
            
            foreach (var image in detail.Images)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    downloadProgress.IsCancelled = true;
                    downloadProgress.IsCompleted = true;
                    progress?.Report(downloadProgress);
                    return album; // Trả về album với ảnh đã tải được
                }

                try
                {
                    var fileName = GetSafeFileName(image);
                    downloadProgress.CurrentImageName = fileName;
                    progress?.Report(downloadProgress);

                    var savePath = Path.Combine(album.FolderPath, fileName);
                    
                    // Tải ảnh
                    var imageBytes = await _httpClient.GetByteArrayAsync(image.ImageUrl, cancellationToken);
                    await File.WriteAllBytesAsync(savePath, imageBytes, cancellationToken);

                    // Đánh dấu ảnh bìa
                    if (image.IsCover)
                        coverPhotoPath = savePath;

                    downloadProgress.DownloadedImages++;
                }
                catch (Exception ex)
                {
                    downloadProgress.FailedImages++;
                    System.Diagnostics.Debug.WriteLine($"[AlbumDownload] Lỗi tải ảnh {image.Id}: {ex.Message}");
                }

                progress?.Report(downloadProgress);
            }

            // 4. Cập nhật metadata album
            album.CoverPhotoPath = coverPhotoPath;
            album.PhotoCount = downloadProgress.DownloadedImages;
            album.ModifiedDate = DateTime.Now;
            _albumService.UpdateAlbum(album);

            downloadProgress.IsCompleted = true;
            progress?.Report(downloadProgress);

            return album;
        }
        catch (Exception ex)
        {
            downloadProgress.ErrorMessage = ex.Message;
            downloadProgress.IsCompleted = true;
            progress?.Report(downloadProgress);
            return null;
        }
    }

    /// <summary>
    /// Tải nhiều album (tối đa MaxAlbumsPerDownload)
    /// </summary>
    public async Task<List<(string RemoteId, SimpleAlbum? LocalAlbum, string? Error)>> DownloadMultipleAlbumsAsync(
        List<string> remoteAlbumIds,
        IProgress<AlbumDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (remoteAlbumIds.Count > MaxAlbumsPerDownload)
        {
            throw new InvalidOperationException(
                $"Tối đa {MaxAlbumsPerDownload} album mỗi lần tải. Đã chọn {remoteAlbumIds.Count} album.");
        }

        var results = new List<(string RemoteId, SimpleAlbum? LocalAlbum, string? Error)>();

        foreach (var id in remoteAlbumIds)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var album = await DownloadAlbumAsync(id, progress, cancellationToken);
                results.Add((id, album, album == null ? "Không tải được album" : null));
            }
            catch (Exception ex)
            {
                results.Add((id, null, ex.Message));
            }
        }

        return results;
    }

    /// <summary>
    /// Kiểm tra kết nối đến server
    /// </summary>
    public async Task<(bool IsConnected, int TotalAlbums, string? Error)> TestConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await GetRemoteAlbumsAsync(page: 1, limit: 1, cancellationToken: cancellationToken);
            return (true, result.Pagination.Total, null);
        }
        catch (HttpRequestException ex)
        {
            return (false, 0, $"Không kết nối được: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return (false, 0, "Hết thời gian kết nối (timeout)");
        }
        catch (Exception ex)
        {
            return (false, 0, ex.Message);
        }
    }

    private string BuildAlbumDescription(RemoteAlbumDetail detail)
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrEmpty(detail.Description))
            parts.Add(detail.Description);
        
        parts.Add($"--- Nguồn: xagiakiem.gov.vn ---");
        
        if (!string.IsNullOrEmpty(detail.OrganizationName))
            parts.Add($"Tổ chức: {detail.OrganizationName}");
        if (!string.IsNullOrEmpty(detail.EventTitle))
            parts.Add($"Sự kiện: {detail.EventTitle}");
        if (!string.IsNullOrEmpty(detail.EventDate))
        {
            if (DateTime.TryParse(detail.EventDate, out var evtDt))
                parts.Add($"Ngày diễn ra: {evtDt:dd/MM/yyyy}");
        }
        if (!string.IsNullOrEmpty(detail.CreatedAt))
        {
            if (DateTime.TryParse(detail.CreatedAt, out var dt))
                parts.Add($"Ngày đăng: {dt:dd/MM/yyyy}");
        }
        parts.Add($"ID gốc: {detail.Id}");
        
        return string.Join("\n", parts);
    }

    private string GetSafeFileName(RemoteAlbumImage image)
    {
        // Lấy extension từ URL
        var uri = new Uri(image.ImageUrl);
        var ext = Path.GetExtension(uri.AbsolutePath);
        if (string.IsNullOrEmpty(ext)) ext = ".jpg";

        // Tạo tên file: order_title hoặc order_id
        var order = image.DisplayOrder.ToString("D3");
        var baseName = !string.IsNullOrWhiteSpace(image.Title)
            ? SanitizeFileName(image.Title)
            : image.Id[..Math.Min(8, image.Id.Length)];

        return $"{order}_{baseName}{ext}";
    }

    private static string SanitizeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(name.Where(c => !invalidChars.Contains(c)).ToArray());
        
        // Truncate
        if (sanitized.Length > 60)
            sanitized = sanitized[..60];
            
        return sanitized.Trim();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
