using System.Text.Json;
using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service quản lý album đơn giản (không dùng template)
/// </summary>
public class SimpleAlbumService : IDisposable
{
    private readonly string _photosBasePath;
    private const string AlbumMetadataFileName = "album.json";

    public SimpleAlbumService(string? photosBasePath = null)
    {
        _photosBasePath = photosBasePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "AIVanBan",
            "Photos"
        );
        
        if (!Directory.Exists(_photosBasePath))
        {
            Directory.CreateDirectory(_photosBasePath);
        }
    }
    
    public void Dispose()
    {
        // Nothing to dispose
    }

    #region Album CRUD

    /// <summary>
    /// Lấy tất cả albums
    /// </summary>
    public List<SimpleAlbum> GetAllAlbums()
    {
        var albums = new List<SimpleAlbum>();
        
        if (!Directory.Exists(_photosBasePath))
            return albums;

        var albumFolders = Directory.GetDirectories(_photosBasePath);
        
        foreach (var folder in albumFolders)
        {
            try
            {
                var album = LoadAlbumMetadata(folder);
                if (album != null && album.Status == "Active")
                {
                    // Update photo count
                    album.PhotoCount = CountPhotosInFolder(folder);
                    
                    // Load thumbnail photos (first 4 images, with cover photo priority)
                    album.ThumbnailPhotos = GetThumbnailPhotos(folder, album.CoverPhotoPath, 4);
                    
                    albums.Add(album);
                }
            }
            catch
            {
                // Skip invalid folders
            }
        }

        return albums.OrderByDescending(a => a.ModifiedDate).ToList();
    }

    /// <summary>
    /// Lấy album theo ID
    /// </summary>
    public SimpleAlbum? GetAlbumById(string albumId)
    {
        if (string.IsNullOrWhiteSpace(albumId))
        {
            return null;
        }

        var id = albumId.Trim();
        var albums = GetAllAlbums();
        var found = albums.FirstOrDefault(a => a.Id == id);
        if (found != null)
        {
            return found;
        }

        // Fallback: treat id as folder name under base path
        var folderPath = Path.Combine(_photosBasePath, id);
        if (Directory.Exists(folderPath))
        {
            return LoadAlbumMetadata(folderPath);
        }

        // Fallback: treat id as a full folder path
        if (Directory.Exists(id))
        {
            return LoadAlbumMetadata(id);
        }

        return null;
    }

    /// <summary>
    /// Tạo album mới
    /// </summary>
    public SimpleAlbum CreateAlbum(string title, string description, List<string>? tags = null)
    {
        var album = new SimpleAlbum
        {
            Id = Guid.NewGuid().ToString(),
            Title = title,
            Description = description,
            Tags = tags ?? new List<string>(),
            CreatedDate = DateTime.Now,
            ModifiedDate = DateTime.Now
        };

        // Create folder
        var folderName = $"{album.Id}"; // Chỉ dùng GUID cho đơn giản
        album.FolderPath = Path.Combine(_photosBasePath, folderName);
        Directory.CreateDirectory(album.FolderPath);

        // Save metadata
        SaveAlbumMetadata(album);

        return album;
    }

    /// <summary>
    /// Cập nhật album
    /// </summary>
    public void UpdateAlbum(SimpleAlbum album)
    {
        album.ModifiedDate = DateTime.Now;
        SaveAlbumMetadata(album);
    }

    /// <summary>
    /// Xóa album (soft delete)
    /// </summary>
    public void DeleteAlbum(string albumId)
    {
        var album = GetAlbumById(albumId);
        if (album != null)
        {
            album.Status = "Deleted";
            SaveAlbumMetadata(album);
        }
    }

    /// <summary>
    /// Xóa album vĩnh viễn (hard delete)
    /// </summary>
    public void DeleteAlbumPermanently(string albumId)
    {
        var album = GetAlbumById(albumId);
        if (album != null && Directory.Exists(album.FolderPath))
        {
            Directory.Delete(album.FolderPath, true);
            return;
        }

        // Fallback delete by folder name/path
        var folderPath = Path.Combine(_photosBasePath, albumId.Trim());
        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, true);
            return;
        }

        if (Directory.Exists(albumId))
        {
            Directory.Delete(albumId, true);
        }
    }

    #endregion

    #region Metadata Operations

    /// <summary>
    /// Load album metadata từ album.json
    /// </summary>
    private SimpleAlbum? LoadAlbumMetadata(string folderPath)
    {
        var metadataPath = Path.Combine(folderPath, AlbumMetadataFileName);
        
        if (!File.Exists(metadataPath))
        {
            // Tạo album metadata mặc định cho folder cũ
            var folderName = new DirectoryInfo(folderPath).Name;
            return new SimpleAlbum
            {
                Id = folderName,
                Title = folderName,
                Description = "",
                FolderPath = folderPath,
                CreatedDate = Directory.GetCreationTime(folderPath),
                ModifiedDate = Directory.GetLastWriteTime(folderPath)
            };
        }

        try
        {
            var json = File.ReadAllText(metadataPath);
            var album = JsonSerializer.Deserialize<SimpleAlbum>(json);
            if (album != null)
            {
                album.FolderPath = folderPath;
            }
            return album;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Lưu album metadata vào album.json
    /// </summary>
    private void SaveAlbumMetadata(SimpleAlbum album)
    {
        var metadataPath = Path.Combine(album.FolderPath, AlbumMetadataFileName);
        var json = JsonSerializer.Serialize(album, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        File.WriteAllText(metadataPath, json, System.Text.Encoding.UTF8);
    }

    #endregion

    #region Photo Operations

    /// <summary>
    /// Đếm số ảnh trong folder
    /// </summary>
    private int CountPhotosInFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return 0;

        var photoExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        return Directory.GetFiles(folderPath)
            .Count(f => photoExtensions.Contains(Path.GetExtension(f).ToLower()));
    }

    /// <summary>
    /// Lấy thumbnail photos từ folder (tối đa maxCount ảnh)
    /// Nếu có coverPhotoPath thì đặt nó làm ảnh đầu tiên
    /// </summary>
    private List<string> GetThumbnailPhotos(string folderPath, string? coverPhotoPath, int maxCount = 4)
    {
        var thumbnails = new List<string>();
        
        if (!Directory.Exists(folderPath))
            return thumbnails;

        var photoExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        var photoFiles = Directory.GetFiles(folderPath)
            .Where(f => photoExtensions.Contains(Path.GetExtension(f).ToLower()))
            .OrderBy(f => File.GetCreationTime(f))
            .ToList();

        // Nếu có cover photo và file tồn tại, đặt nó làm ảnh đầu tiên
        if (!string.IsNullOrEmpty(coverPhotoPath) && File.Exists(coverPhotoPath))
        {
            thumbnails.Add(coverPhotoPath);
            // Lấy các ảnh còn lại (trừ cover photo)
            var remainingPhotos = photoFiles
                .Where(f => !f.Equals(coverPhotoPath, StringComparison.OrdinalIgnoreCase))
                .Take(maxCount - 1);
            thumbnails.AddRange(remainingPhotos);
        }
        else
        {
            // Không có cover photo, lấy ảnh thường
            thumbnails.AddRange(photoFiles.Take(maxCount));
        }

        return thumbnails;
    }

    /// <summary>
    /// Lấy danh sách ảnh trong album
    /// </summary>
    public List<SimplePhoto> GetPhotosInAlbum(string albumId)
    {
        var album = GetAlbumById(albumId);
        if (album == null || !Directory.Exists(album.FolderPath))
            return new List<SimplePhoto>();

        var photos = new List<SimplePhoto>();
        var photoExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        var files = Directory.GetFiles(album.FolderPath)
            .Where(f => photoExtensions.Contains(Path.GetExtension(f).ToLower()))
            .OrderByDescending(f => new FileInfo(f).CreationTime);

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            photos.Add(new SimplePhoto
            {
                AlbumId = albumId,
                FilePath = file,
                FileName = fileInfo.Name,
                FileSize = fileInfo.Length,
                AddedDate = fileInfo.CreationTime
            });
        }

        return photos;
    }

    /// <summary>
    /// Lấy ảnh cover của album (ảnh đầu tiên)
    /// </summary>
    public string? GetAlbumCoverPhoto(string albumId)
    {
        var photos = GetPhotosInAlbum(albumId);
        return photos.FirstOrDefault()?.FilePath;
    }

    #endregion

    #region Search & Filter

    /// <summary>
    /// Loại bỏ dấu tiếng Việt để tìm kiếm
    /// </summary>
    private string RemoveVietnameseTones(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        // Bước 1: Normalize để tách các dấu thanh ra
        var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
        var result = new StringBuilder();

        foreach (char c in normalized)
        {
            var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                result.Append(c);
            }
        }

        var str = result.ToString().Normalize(System.Text.NormalizationForm.FormC);

        // Bước 2: Thay thế các ký tự có dấu mũ/dấu ngang còn lại
        str = str.Replace("đ", "d").Replace("Đ", "D")
                 .Replace("ă", "a").Replace("Ă", "A")
                 .Replace("â", "a").Replace("Â", "A")
                 .Replace("ê", "e").Replace("Ê", "E")
                 .Replace("ô", "o").Replace("Ô", "O")
                 .Replace("ơ", "o").Replace("Ơ", "O")
                 .Replace("ư", "u").Replace("Ư", "U");

        return str;
    }

    /// <summary>
    /// Tìm kiếm album theo tiêu đề hoặc mô tả (bỏ qua dấu tiếng Việt)
    /// </summary>
    public List<SimpleAlbum> SearchAlbums(string keyword)
    {
        var albums = GetAllAlbums();
        
        if (string.IsNullOrWhiteSpace(keyword))
            return albums;

        // Bỏ dấu keyword để tìm kiếm
        var normalizedKeyword = RemoveVietnameseTones(keyword).ToLower();
        
        return albums.Where(a => 
            RemoveVietnameseTones(a.Title).ToLower().Contains(normalizedKeyword) || 
            RemoveVietnameseTones(a.Description).ToLower().Contains(normalizedKeyword) ||
            a.Tags.Any(t => RemoveVietnameseTones(t).ToLower().Contains(normalizedKeyword))
        ).ToList();
    }

    /// <summary>
    /// Lấy album theo tag
    /// </summary>
    public List<SimpleAlbum> GetAlbumsByTag(string tag)
    {
        var albums = GetAllAlbums();
        return albums.Where(a => a.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)).ToList();
    }

    #endregion

    #region Album Folder Management

    /// <summary>
    /// Lấy tất cả album trong 1 folder
    /// </summary>
    public List<SimpleAlbum> GetAlbumsByFolderId(string folderId)
    {
        var albums = GetAllAlbums();
        return albums.Where(a => a.AlbumFolderId == folderId)
                     .OrderByDescending(a => a.CreatedDate)
                     .ToList();
    }

    /// <summary>
    /// Lấy tất cả album trong folder và tất cả subfolder (đệ quy)
    /// </summary>
    public List<SimpleAlbum> GetAlbumsRecursive(string folderId)
    {
        var albums = new List<SimpleAlbum>();
        
        // Lấy album trong folder hiện tại
        albums.AddRange(GetAlbumsByFolderId(folderId));
        
        // Lấy album trong tất cả subfolder
        using (var folderService = new AlbumFolderService())
        {
            var children = folderService.GetChildFolders(folderId);
            foreach (var child in children)
            {
                albums.AddRange(GetAlbumsRecursive(child.Id));
            }
        }
        
        return albums;
    }

    /// <summary>
    /// Di chuyển album sang folder khác
    /// </summary>
    public void MoveAlbumToFolder(string albumId, string targetFolderId)
    {
        var album = GetAlbumById(albumId);
        if (album == null) return;
        
        using (var folderService = new AlbumFolderService())
        {
            var targetFolder = folderService.GetFolderById(targetFolderId);
            if (targetFolder == null) return;
            
            // Lưu folder cũ để update stats sau
            var oldFolderId = album.AlbumFolderId;
            
            // Cập nhật folder của album
            album.AlbumFolderId = targetFolderId;
            album.AlbumFolderPath = targetFolder.Path;
            album.ModifiedDate = DateTime.Now;
            
            // Save metadata
            SaveAlbumMetadata(album);
            
            // Cập nhật AlbumCount của folder cũ và mới
            if (!string.IsNullOrEmpty(oldFolderId))
            {
                folderService.UpdateFolderStats(oldFolderId);
            }
            folderService.UpdateFolderStats(targetFolderId);
        }
    }

    /// <summary>
    /// Lấy tất cả album không thuộc folder nào (legacy albums)
    /// </summary>
    public List<SimpleAlbum> GetAlbumsWithoutFolder()
    {
        var albums = GetAllAlbums();
        return albums.Where(a => string.IsNullOrEmpty(a.AlbumFolderId))
                     .OrderByDescending(a => a.CreatedDate)
                     .ToList();
    }

    #endregion
}

