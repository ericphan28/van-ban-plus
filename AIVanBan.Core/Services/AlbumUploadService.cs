using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AIVanBan.Core.Services
{
    /// <summary>
    /// Service upload album từ Desktop lên website xa-gia-kiem
    /// Xác thực bằng Supabase JWT qua hệ thống phân quyền RBAC
    /// </summary>
    public class AlbumUploadService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private string? _accessToken;
        private string? _refreshToken;
        private UploadUserProfile? _currentUser;

        public string BaseUrl { get; private set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken) && _currentUser != null;
        public UploadUserProfile? CurrentUser => _currentUser;

        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        public AlbumUploadService(string baseUrl)
        {
            BaseUrl = baseUrl.TrimEnd('/');
            var handler = new SocketsHttpHandler
            {
                ConnectTimeout = TimeSpan.FromSeconds(15),
                EnableMultipleHttp2Connections = true
            };
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMinutes(10)
            };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "AIVanBan-Desktop/1.0");
        }

        /// <summary>
        /// Đổi server URL (cho testing)
        /// </summary>
        public void ChangeBaseUrl(string newUrl)
        {
            BaseUrl = newUrl.TrimEnd('/');
        }

        #region Authentication

        /// <summary>
        /// Đăng nhập bằng tài khoản xa-gia-kiem
        /// </summary>
        public async Task<UploadLoginResult> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"{BaseUrl}/api/upload/auth",
                    new { email, password }
                );

                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Xử lý trường hợp server trả về HTML hoặc non-JSON
                    string errorMsg = $"Lỗi HTTP {(int)response.StatusCode}";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(json) && json.TrimStart().StartsWith("{"))
                        {
                            var error = JsonSerializer.Deserialize<ErrorResponse>(json);
                            if (!string.IsNullOrEmpty(error?.Error))
                                errorMsg = error.Error;
                        }
                        else if ((int)response.StatusCode == 404)
                        {
                            errorMsg = "Không tìm thấy API upload trên server. Kiểm tra lại URL (bấm ⚙️).";
                        }
                    }
                    catch { /* Giữ errorMsg mặc định */ }

                    return new UploadLoginResult
                    {
                        Success = false,
                        Error = errorMsg
                    };
                }

                // Kiểm tra response có phải JSON không
                if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
                {
                    return new UploadLoginResult
                    {
                        Success = false,
                        Error = "Server trả về dữ liệu không hợp lệ. Kiểm tra lại URL server (bấm ⚙️)."
                    };
                }

                var result = JsonSerializer.Deserialize<UploadAuthResponse>(json);
                if (result?.AccessToken == null)
                {
                    return new UploadLoginResult { Success = false, Error = "Phản hồi không hợp lệ từ server. Thiếu access_token." };
                }

                _accessToken = result.AccessToken;
                _refreshToken = result.RefreshToken;
                _currentUser = result.User;
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);

                return new UploadLoginResult
                {
                    Success = true,
                    User = result.User
                };
            }
            catch (HttpRequestException ex)
            {
                return new UploadLoginResult
                {
                    Success = false,
                    Error = $"Không kết nối được server ({BaseUrl}). Kiểm tra URL và đảm bảo server đang chạy. Bấm ⚙️ để sửa URL."
                };
            }
            catch (TaskCanceledException)
            {
                return new UploadLoginResult
                {
                    Success = false,
                    Error = "Kết nối bị timeout. Vui lòng kiểm tra lại server."
                };
            }
            catch (Exception ex)
            {
                return new UploadLoginResult
                {
                    Success = false,
                    Error = $"Lỗi đăng nhập: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Đăng xuất - xóa token
        /// </summary>
        public void Logout()
        {
            _accessToken = null;
            _refreshToken = null;
            _currentUser = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        #endregion

        #region Organizations

        /// <summary>
        /// Lấy danh sách tổ chức user có quyền upload
        /// </summary>
        public async Task<List<UploadOrganization>> GetOrganizationsAsync()
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/api/upload/organizations");

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception($"Không tải được tổ chức: {err}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<UploadOrganization>>(json)
                ?? new List<UploadOrganization>();
        }

        #endregion

        #region Upload Album

        /// <summary>
        /// Upload 1 album (tạo album + upload ảnh + finalize)
        /// </summary>
        public async Task<UploadAlbumResult> UploadAlbumAsync(
            Models.SimpleAlbum album,
            string? organizationId,
            IProgress<AlbumUploadProgress>? progress = null,
            CancellationToken ct = default)
        {
            var result = new UploadAlbumResult { AlbumTitle = album.Title };

            try
            {
                // Step 1: Lấy danh sách ảnh
                var photos = GetAlbumImageFiles(album.FolderPath);
                if (photos.Count == 0)
                {
                    result.Error = "Album không có ảnh nào để upload.";
                    return result;
                }

                // Step 2: Tạo album trên server
                progress?.Report(new AlbumUploadProgress
                {
                    Status = $"Tạo album '{album.Title}'...",
                    Percentage = 2
                });

                var createBody = new Dictionary<string, object?>
                {
                    ["title"] = album.Title,
                    ["description"] = album.Description
                };

                if (!string.IsNullOrEmpty(organizationId))
                {
                    createBody["organization_id"] = organizationId;
                }

                if (album.EventDate.HasValue)
                {
                    createBody["event_date"] = album.EventDate.Value.ToString("yyyy-MM-dd");
                }

                var createResponse = await _httpClient.PostAsJsonAsync(
                    $"{BaseUrl}/api/upload/albums",
                    createBody, ct
                );

                if (!createResponse.IsSuccessStatusCode)
                {
                    var err = await createResponse.Content.ReadAsStringAsync(ct);
                    var errObj = JsonSerializer.Deserialize<ErrorResponse>(err);
                    result.Error = $"Tạo album thất bại: {errObj?.Error ?? err}";
                    return result;
                }

                var albumJson = await createResponse.Content.ReadAsStringAsync(ct);
                var remoteAlbum = JsonSerializer.Deserialize<JsonElement>(albumJson);
                var albumId = remoteAlbum.GetProperty("id").GetString()!;
                result.RemoteAlbumId = albumId;

                // Step 3: Upload từng ảnh
                int uploaded = 0;
                int failed = 0;

                foreach (var photoPath in photos)
                {
                    ct.ThrowIfCancellationRequested();

                    var fileName = Path.GetFileName(photoPath);
                    var pct = 5 + (int)(85.0 * uploaded / photos.Count);
                    progress?.Report(new AlbumUploadProgress
                    {
                        Status = $"Upload ảnh {uploaded + 1}/{photos.Count}: {fileName}",
                        Percentage = pct,
                        CurrentImage = uploaded + 1,
                        TotalImages = photos.Count
                    });

                    // Xác định cover: dùng CoverPhotoPath hoặc ảnh đầu tiên
                    var isCover = !string.IsNullOrEmpty(album.CoverPhotoPath)
                        ? string.Equals(photoPath, album.CoverPhotoPath, StringComparison.OrdinalIgnoreCase)
                        : uploaded == 0;

                    try
                    {
                        await UploadSingleImageAsync(albumId, photoPath, null, isCover, ct);
                        uploaded++;
                    }
                    catch (Exception ex)
                    {
                        var errMsg = $"{fileName}: {ex.Message}";
                        System.Diagnostics.Debug.WriteLine($"⚠️ Upload ảnh thất bại: {errMsg}");
                        result.ImageErrors.Add(errMsg);
                        failed++;

                        progress?.Report(new AlbumUploadProgress
                        {
                            Status = $"❌ Lỗi ảnh {uploaded + failed}/{photos.Count}: {fileName}",
                            Percentage = pct
                        });
                    }
                }

                // Step 4: Finalize album
                progress?.Report(new AlbumUploadProgress
                {
                    Status = "Hoàn tất album...",
                    Percentage = 95
                });

                await FinalizeAlbumAsync(albumId, ct);

                result.Success = true;
                result.PhotoCount = uploaded;
                result.FailedCount = failed;

                var statusMsg = failed > 0
                    ? $"✓ '{album.Title}' ({uploaded} ảnh, {failed} lỗi)"
                    : $"✓ '{album.Title}' ({uploaded} ảnh)";

                progress?.Report(new AlbumUploadProgress
                {
                    Status = statusMsg,
                    Percentage = 100
                });
            }
            catch (OperationCanceledException)
            {
                result.Error = "Đã hủy upload.";
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Upload batch - nhiều album cùng lúc
        /// </summary>
        public async Task<List<UploadAlbumResult>> UploadBatchAsync(
            List<Models.SimpleAlbum> albums,
            string? organizationId,
            IProgress<AlbumUploadProgress>? progress = null,
            CancellationToken ct = default)
        {
            var results = new List<UploadAlbumResult>();

            for (int i = 0; i < albums.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                var album = albums[i];
                progress?.Report(new AlbumUploadProgress
                {
                    Status = $"Album {i + 1}/{albums.Count}: {album.Title}",
                    Percentage = (int)(100.0 * i / albums.Count),
                    CurrentAlbum = i + 1,
                    TotalAlbums = albums.Count
                });

                var albumProgress = new Progress<AlbumUploadProgress>(p =>
                {
                    // Scale progress within this album's portion
                    var albumPortion = 100.0 / albums.Count;
                    var overallPct = (int)(albumPortion * i + albumPortion * p.Percentage / 100);
                    progress?.Report(new AlbumUploadProgress
                    {
                        Status = $"[{i + 1}/{albums.Count}] {p.Status}",
                        Percentage = overallPct,
                        CurrentImage = p.CurrentImage,
                        TotalImages = p.TotalImages,
                        CurrentAlbum = i + 1,
                        TotalAlbums = albums.Count
                    });
                });

                var result = await UploadAlbumAsync(album, organizationId, albumProgress, ct);
                results.Add(result);
            }

            return results;
        }

        #endregion

        #region Private Helpers

        private async Task UploadSingleImageAsync(
            string albumId, string imagePath, string? caption, bool isCover, CancellationToken ct)
        {
            // Resize ảnh giống admin web (max 1920x1080, quality 85%)
            var (imageBytes, mimeType, fileName) = await ResizeImageAsync(imagePath, ct);

            // Xây dựng multipart body THỦ CÔNG — .NET MultipartFormDataContent
            // không tương thích với Node.js/undici request.formData() parser.
            // Cách này tạo body giống hệt curl -F gửi.
            var boundary = $"----Upload{Guid.NewGuid():N}";
            var crlf = "\r\n";
            var encoding = new System.Text.UTF8Encoding(false);

            using var bodyStream = new MemoryStream();

            // --- File part ---
            var fileHeader = $"--{boundary}{crlf}" +
                             $"Content-Disposition: form-data; name=\"file\"; filename=\"{fileName}\"{crlf}" +
                             $"Content-Type: {mimeType}{crlf}{crlf}";
            var headerBytes = encoding.GetBytes(fileHeader);
            bodyStream.Write(headerBytes, 0, headerBytes.Length);
            bodyStream.Write(imageBytes, 0, imageBytes.Length);
            bodyStream.Write(encoding.GetBytes(crlf), 0, encoding.GetBytes(crlf).Length);

            // --- Caption part (optional) ---
            if (!string.IsNullOrEmpty(caption))
            {
                var captionPart = $"--{boundary}{crlf}" +
                                  $"Content-Disposition: form-data; name=\"caption\"{crlf}{crlf}" +
                                  $"{caption}{crlf}";
                var captionBytes = encoding.GetBytes(captionPart);
                bodyStream.Write(captionBytes, 0, captionBytes.Length);
            }

            // --- is_cover part ---
            var coverPart = $"--{boundary}{crlf}" +
                            $"Content-Disposition: form-data; name=\"is_cover\"{crlf}{crlf}" +
                            $"{isCover.ToString().ToLower()}{crlf}";
            var coverBytes = encoding.GetBytes(coverPart);
            bodyStream.Write(coverBytes, 0, coverBytes.Length);

            // --- End boundary ---
            var endBoundary = $"--{boundary}--{crlf}";
            var endBytes = encoding.GetBytes(endBoundary);
            bodyStream.Write(endBytes, 0, endBytes.Length);

            var bodyContent = new ByteArrayContent(bodyStream.ToArray());
            bodyContent.Headers.ContentType = MediaTypeHeaderValue.Parse(
                $"multipart/form-data; boundary={boundary}");

            System.Diagnostics.Debug.WriteLine(
                $"📤 Upload: {fileName} ({imageBytes.Length / 1024}KB) → {BaseUrl}/api/upload/albums/{albumId}/images");
            System.Diagnostics.Debug.WriteLine(
                $"   Content-Type: multipart/form-data; boundary={boundary}");
            System.Diagnostics.Debug.WriteLine(
                $"   Body size: {bodyStream.Length} bytes");

            var response = await _httpClient.PostAsync(
                $"{BaseUrl}/api/upload/albums/{albumId}/images",
                bodyContent, ct
            );

            var responseBody = await response.Content.ReadAsStringAsync(ct);
            System.Diagnostics.Debug.WriteLine(
                $"   Response: {(int)response.StatusCode} {response.StatusCode} - {responseBody.Substring(0, Math.Min(200, responseBody.Length))}");

            if (!response.IsSuccessStatusCode)
            {
                // Parse error message nếu có
                string errorDetail;
                try
                {
                    var errObj = JsonSerializer.Deserialize<ErrorResponse>(responseBody);
                    errorDetail = errObj?.Error ?? responseBody;
                }
                catch
                {
                    errorDetail = $"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(300, responseBody.Length))}";
                }
                throw new Exception(errorDetail);
            }
        }

        /// <summary>
        /// Resize ảnh trước khi upload (giống admin web: max 1920x1080, quality 85%)
        /// </summary>
        private static async Task<(byte[] Bytes, string MimeType, string FileName)> ResizeImageAsync(
            string imagePath, CancellationToken ct)
        {
            const int MAX_WIDTH = 1920;
            const int MAX_HEIGHT = 1080;
            const long QUALITY = 85L;

            var originalBytes = await File.ReadAllBytesAsync(imagePath, ct);
            var fileName = Path.GetFileName(imagePath);
            var ext = Path.GetExtension(imagePath).ToLower();

            // Xác định mime type
            var mimeType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                _ => "image/jpeg"
            };

            // GIF không resize (giữ animation)
            if (ext == ".gif")
            {
                return (originalBytes, mimeType, fileName);
            }

            try
            {
                using var ms = new MemoryStream(originalBytes);
                using var original = Image.FromStream(ms);

                // Kiểm tra cần resize không
                if (original.Width <= MAX_WIDTH && original.Height <= MAX_HEIGHT)
                {
                    // Không cần resize nhưng vẫn nén JPEG quality 85%
                    if (ext is ".jpg" or ".jpeg")
                    {
                        using var outputMs = new MemoryStream();
                        var jpegCodec = GetJpegEncoder();
                        var encoderParams = new EncoderParameters(1);
                        encoderParams.Param[0] = new EncoderParameter(
                            System.Drawing.Imaging.Encoder.Quality, QUALITY);
                        original.Save(outputMs, jpegCodec, encoderParams);

                        var compressed = outputMs.ToArray();
                        // Chỉ dùng bản nén nếu nhỏ hơn
                        if (compressed.Length < originalBytes.Length)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"🗜️ Nén: {fileName} {originalBytes.Length / 1024}KB → {compressed.Length / 1024}KB");
                            return (compressed, mimeType, fileName);
                        }
                    }

                    return (originalBytes, mimeType, fileName);
                }

                // Tính kích thước mới giữ tỷ lệ
                double ratioW = (double)MAX_WIDTH / original.Width;
                double ratioH = (double)MAX_HEIGHT / original.Height;
                double ratio = Math.Min(ratioW, ratioH);

                int newWidth = (int)(original.Width * ratio);
                int newHeight = (int)(original.Height * ratio);

                System.Diagnostics.Debug.WriteLine(
                    $"📐 Resize: {fileName} {original.Width}x{original.Height} → {newWidth}x{newHeight}");

                // Resize
                using var resized = new Bitmap(newWidth, newHeight);
                using (var g = Graphics.FromImage(resized))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.DrawImage(original, 0, 0, newWidth, newHeight);
                }

                // Encode output
                using var outMs = new MemoryStream();

                if (ext is ".png")
                {
                    resized.Save(outMs, ImageFormat.Png);
                    mimeType = "image/png";
                }
                else
                {
                    // JPEG với quality 85%
                    var jpegCodec = GetJpegEncoder();
                    var encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(
                        System.Drawing.Imaging.Encoder.Quality, QUALITY);
                    resized.Save(outMs, jpegCodec, encoderParams);
                    mimeType = "image/jpeg";

                    // Đổi extension nếu cần
                    if (ext != ".jpg" && ext != ".jpeg")
                    {
                        fileName = Path.ChangeExtension(fileName, ".jpg");
                    }
                }

                var resultBytes = outMs.ToArray();
                System.Diagnostics.Debug.WriteLine(
                    $"🗜️ {fileName}: {originalBytes.Length / 1024}KB → {resultBytes.Length / 1024}KB " +
                    $"({(100 - 100.0 * resultBytes.Length / originalBytes.Length):N0}% nhỏ hơn)");

                return (resultBytes, mimeType, fileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Resize failed ({fileName}), gửi nguyên gốc: {ex.Message}");
                return (originalBytes, mimeType, fileName);
            }
        }

        private static ImageCodecInfo GetJpegEncoder()
        {
            return ImageCodecInfo.GetImageEncoders()
                .First(c => c.FormatID == ImageFormat.Jpeg.Guid);
        }

        private async Task FinalizeAlbumAsync(string albumId, CancellationToken ct)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Patch,
                $"{BaseUrl}/api/upload/albums/{albumId}/finalize"
            );

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(ct);
                System.Diagnostics.Debug.WriteLine($"⚠️ Finalize warning: {err}");
            }
        }

        private static List<string> GetAlbumImageFiles(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                return new List<string>();

            return Directory.GetFiles(folderPath)
                .Where(f => ImageExtensions.Contains(Path.GetExtension(f).ToLower()))
                .OrderBy(f => f)
                .ToList();
        }

        #endregion

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    #region Models

    public class UploadLoginResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public UploadUserProfile? User { get; set; }
    }

    public class UploadAuthResponse
    {
        [JsonPropertyName("user")] public UploadUserProfile? User { get; set; }
        [JsonPropertyName("access_token")] public string? AccessToken { get; set; }
        [JsonPropertyName("refresh_token")] public string? RefreshToken { get; set; }
        [JsonPropertyName("expires_at")] public long? ExpiresAt { get; set; }
    }

    public class UploadUserProfile
    {
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        [JsonPropertyName("full_name")] public string FullName { get; set; } = "";
        [JsonPropertyName("email")] public string Email { get; set; } = "";
        [JsonPropertyName("system_role")] public string SystemRole { get; set; } = "";
        [JsonPropertyName("organization_id")] public string? OrganizationId { get; set; }

        public string RoleDisplayName => SystemRole switch
        {
            "super_admin" => "Quản trị hệ thống",
            "admin" => "Quản trị viên",
            "org_admin" => "Quản trị tổ chức",
            "editor" => "Biên tập viên",
            "contributor" => "Cộng tác viên",
            "viewer" => "Người xem",
            _ => SystemRole
        };
    }

    public class UploadOrganization
    {
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("short_name")] public string? ShortName { get; set; }
        [JsonPropertyName("org_type")] public string? OrgType { get; set; }

        public override string ToString() => Name;
    }

    public class UploadAlbumResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string AlbumTitle { get; set; } = "";
        public string? RemoteAlbumId { get; set; }
        public int PhotoCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> ImageErrors { get; set; } = new();
    }

    public class AlbumUploadProgress
    {
        public string Status { get; set; } = "";
        public int Percentage { get; set; }
        public int CurrentImage { get; set; }
        public int TotalImages { get; set; }
        public int CurrentAlbum { get; set; }
        public int TotalAlbums { get; set; }
    }

    internal class ErrorResponse
    {
        [JsonPropertyName("error")] public string? Error { get; set; }
    }

    #endregion
}
