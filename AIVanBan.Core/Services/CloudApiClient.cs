using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services
{
    /// <summary>
    /// HTTP client để gọi VanBanPlus Cloud API.
    /// Wrapper cho tất cả cloud endpoints.
    /// </summary>
    public class CloudApiClient : IDisposable
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        public CloudApiClient()
        {
            _http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(120) // Cloud operations có thể lâu
            };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
            };
        }

        /// <summary>
        /// Cấu hình base URL và API key.
        /// </summary>
        public void Configure(string baseUrl, string apiKey)
        {
            _http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // ==================== Sync API ====================

        /// <summary>
        /// Push changes lên cloud.
        /// </summary>
        public async Task<ApiResponse<SyncResult>> SyncPush(
            string deviceId, string? deviceName, List<SyncEntity> entities, CancellationToken ct = default)
        {
            var body = new
            {
                action = "push",
                device_id = deviceId,
                device_name = deviceName,
                entities
            };
            return await PostAsync<SyncResult>("api/cloud/sync", body, ct);
        }

        /// <summary>
        /// Pull changes từ cloud.
        /// </summary>
        public async Task<ApiResponse<SyncPullResponse>> SyncPull(
            string deviceId, DateTime since, string[]? entityTypes = null, CancellationToken ct = default)
        {
            var body = new
            {
                action = "pull",
                device_id = deviceId,
                since = since.ToUniversalTime().ToString("o"),
                entity_types = entityTypes
            };
            return await PostAsync<SyncPullResponse>("api/cloud/sync", body, ct);
        }

        /// <summary>
        /// Full sync (push + pull).
        /// </summary>
        public async Task<ApiResponse<SyncResult>> SyncFull(
            string deviceId, string? deviceName, List<SyncEntity> entities, DateTime since, CancellationToken ct = default)
        {
            var body = new
            {
                action = "full",
                device_id = deviceId,
                device_name = deviceName,
                entities,
                since = since.ToUniversalTime().ToString("o")
            };
            return await PostAsync<SyncResult>("api/cloud/sync", body, ct);
        }

        /// <summary>
        /// Lấy sync status.
        /// </summary>
        public async Task<ApiResponse<Dictionary<string, object?>>> GetSyncStatus(CancellationToken ct = default)
        {
            return await GetAsync<Dictionary<string, object?>>("api/cloud/sync", ct);
        }

        // ==================== Document API ====================

        public async Task<ApiResponse<Dictionary<string, object?>>> CloudDocumentAction(
            string action, Dictionary<string, object?>? body = null, CancellationToken ct = default)
        {
            var requestBody = body ?? new Dictionary<string, object?>();
            requestBody["action"] = action;
            return await PostAsync<Dictionary<string, object?>>("api/cloud/documents", requestBody, ct);
        }

        // ==================== Meeting API ====================

        public async Task<ApiResponse<Dictionary<string, object?>>> CloudMeetingAction(
            string action, Dictionary<string, object?>? body = null, CancellationToken ct = default)
        {
            var requestBody = body ?? new Dictionary<string, object?>();
            requestBody["action"] = action;
            return await PostAsync<Dictionary<string, object?>>("api/cloud/meetings", requestBody, ct);
        }

        // ==================== Backup API ====================

        public async Task<ApiResponse<CloudBackupInfo>> CreateBackup(string backupType = "manual", CancellationToken ct = default)
        {
            var body = new { action = "create", backup_type = backupType };
            return await PostAsync<CloudBackupInfo>("api/cloud/backup", body, ct);
        }

        public async Task<ApiResponse<List<CloudBackupInfo>>> ListBackups(int limit = 20, CancellationToken ct = default)
        {
            var body = new { action = "list", limit };
            return await PostAsync<List<CloudBackupInfo>>("api/cloud/backup", body, ct);
        }

        public async Task<ApiResponse<object>> RestoreBackup(string backupId, CancellationToken ct = default)
        {
            var body = new { action = "restore", backup_id = backupId };
            return await PostAsync<object>("api/cloud/backup", body, ct);
        }

        // ==================== Version API ====================

        public async Task<ApiResponse<List<Dictionary<string, object?>>>> GetDocumentVersions(
            string documentId, int limit = 50, CancellationToken ct = default)
        {
            var body = new { action = "list", document_id = documentId, limit };
            return await PostAsync<List<Dictionary<string, object?>>>("api/cloud/versions", body, ct);
        }

        public async Task<ApiResponse<Dictionary<string, object?>>> RestoreDocumentVersion(
            string documentId, int versionNumber, CancellationToken ct = default)
        {
            var body = new { action = "restore", document_id = documentId, version_number = versionNumber };
            return await PostAsync<Dictionary<string, object?>>("api/cloud/versions", body, ct);
        }

        // ==================== Storage API ====================

        public async Task<ApiResponse<StorageQuotaInfo>> GetStorageQuota(CancellationToken ct = default)
        {
            var body = new { action = "quota" };
            return await PostAsync<StorageQuotaInfo>("api/cloud/storage", body, ct);
        }

        public async Task<ApiResponse<Dictionary<string, object?>>> GetDownloadUrl(
            string bucket, string path, CancellationToken ct = default)
        {
            var body = new { action = "download", bucket, path };
            return await PostAsync<Dictionary<string, object?>>("api/cloud/storage", body, ct);
        }

        /// <summary>
        /// Upload file lên cloud storage.
        /// </summary>
        public async Task<ApiResponse<Dictionary<string, object?>>> UploadFile(
            byte[] fileData, string fileName, string bucket = "attachments",
            string? documentId = null, string? albumName = null, CancellationToken ct = default)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileData);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(fileName));
            content.Add(fileContent, "file", fileName);

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/cloud/storage");
            request.Content = content;
            request.Headers.Add("X-Bucket", bucket);
            if (documentId != null) request.Headers.Add("X-Document-Id", documentId);
            if (albumName != null) request.Headers.Add("X-Album-Name", albumName);

            // Copy API key header
            if (_http.DefaultRequestHeaders.Contains("X-API-Key"))
            {
                var apiKey = _http.DefaultRequestHeaders.GetValues("X-API-Key").FirstOrDefault();
                if (apiKey != null) request.Headers.Add("X-API-Key", apiKey);
            }

            var response = await _http.SendAsync(request, ct);
            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<ApiResponse<Dictionary<string, object?>>>(json, _jsonOptions)
                   ?? new ApiResponse<Dictionary<string, object?>> { Success = false, Message = "Lỗi deserialize" };
        }

        // ==================== Sharing API ====================

        public async Task<ApiResponse<T>> SharingAction<T>(
            string action, Dictionary<string, object?>? body = null, CancellationToken ct = default)
        {
            var requestBody = body ?? new Dictionary<string, object?>();
            requestBody["action"] = action;
            return await PostAsync<T>("api/cloud/sharing", requestBody, ct);
        }

        // ==================== HTTP Helpers ====================

        private async Task<ApiResponse<T>> PostAsync<T>(string url, object body, CancellationToken ct)
        {
            try
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PostAsync(url, content, ct);
                var responseJson = await response.Content.ReadAsStringAsync(ct);

                return JsonSerializer.Deserialize<ApiResponse<T>>(responseJson, _jsonOptions)
                       ?? new ApiResponse<T> { Success = false, Message = "Lỗi deserialize response" };
            }
            catch (TaskCanceledException)
            {
                return new ApiResponse<T> { Success = false, Message = "Request timeout." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T> { Success = false, Message = $"Lỗi kết nối: {ex.Message}" };
            }
        }

        private async Task<ApiResponse<T>> GetAsync<T>(string url, CancellationToken ct)
        {
            try
            {
                var response = await _http.GetAsync(url, ct);
                var json = await response.Content.ReadAsStringAsync(ct);

                return JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions)
                       ?? new ApiResponse<T> { Success = false, Message = "Lỗi deserialize response" };
            }
            catch (TaskCanceledException)
            {
                return new ApiResponse<T> { Success = false, Message = "Request timeout." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T> { Success = false, Message = $"Lỗi kết nối: {ex.Message}" };
            }
        }

        private static string GetMimeType(string fileName)
        {
            var ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }

        public void Dispose()
        {
            _http.Dispose();
        }
    }

    /// <summary>
    /// Generic API response (matching VanBanPlus API format).
    /// </summary>
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }
    }
}
