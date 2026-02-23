using LiteDB;
using AIVanBan.Core.Models;
using AIVanBan.Core.Data;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service quản lý AlbumFolder - cấu trúc cây thư mục cho Album
/// </summary>
public class AlbumFolderService : IDisposable
{
    private readonly LiteDatabase _db;
    private readonly string _dataPath;

    public AlbumFolderService(string? databasePath = null)
    {
        _dataPath = databasePath ?? DatabaseFactory.DataPath;

        Directory.CreateDirectory(_dataPath);

        // Dùng shared database instance — tránh file lock conflict
        _db = DatabaseFactory.GetDatabase(databasePath);

        // Indexes
        var folders = _db.GetCollection<AlbumFolder>("albumFolders");
        folders.EnsureIndex(x => x.ParentId);
        folders.EnsureIndex(x => x.Path);
        folders.EnsureIndex(x => x.FolderType);
    }

    #region Folder Tree Management

    /// <summary>
    /// Lấy tất cả folder gốc (ParentId rỗng)
    /// </summary>
    public List<AlbumFolder> GetRootFolders()
    {
        var folders = _db.GetCollection<AlbumFolder>("albumFolders");
        return folders.Find(f => string.IsNullOrEmpty(f.ParentId))
                      .OrderBy(f => f.SortOrder)
                      .ThenBy(f => f.Name)
                      .ToList();
    }

    /// <summary>
    /// Lấy các folder con của 1 folder
    /// </summary>
    public List<AlbumFolder> GetChildFolders(string parentId)
    {
        var folders = _db.GetCollection<AlbumFolder>("albumFolders");
        return folders.Find(f => f.ParentId == parentId)
                      .OrderBy(f => f.SortOrder)
                      .ThenBy(f => f.Name)
                      .ToList();
    }

    /// <summary>
    /// Lấy tất cả folder (dạng phẳng)
    /// </summary>
    public List<AlbumFolder> GetAllFolders()
    {
        var folders = _db.GetCollection<AlbumFolder>("albumFolders");
        return folders.FindAll().ToList();
    }

    /// <summary>
    /// Lấy folder theo ID
    /// </summary>
    public AlbumFolder? GetFolderById(string id)
    {
        var folders = _db.GetCollection<AlbumFolder>("albumFolders");
        return folders.FindById(id);
    }

    /// <summary>
    /// Tạo folder mới (tự động build Path)
    /// </summary>
    public AlbumFolder CreateFolder(AlbumFolder folder)
    {
        // Build Path từ ParentId
        if (!string.IsNullOrEmpty(folder.ParentId))
        {
            var parent = GetFolderById(folder.ParentId);
            if (parent != null)
            {
                folder.Path = string.IsNullOrEmpty(parent.Path)
                    ? folder.Name
                    : $"{parent.Path}/{folder.Name}";
            }
        }
        else
        {
            folder.Path = folder.Name; // Root folder
        }

        folder.CreatedDate = DateTime.Now;
        folder.CreatedBy = Environment.UserName;
        folder.ModifiedDate = DateTime.Now;
        folder.ModifiedBy = Environment.UserName;

        var folders = _db.GetCollection<AlbumFolder>("albumFolders");
        folders.Insert(folder);

        return folder;
    }

    /// <summary>
    /// Cập nhật folder
    /// </summary>
    public bool UpdateFolder(AlbumFolder folder)
    {
        folder.ModifiedDate = DateTime.Now;
        folder.ModifiedBy = Environment.UserName;

        var folders = _db.GetCollection<AlbumFolder>("albumFolders");
        return folders.Update(folder);
    }

    /// <summary>
    /// Xóa folder (đệ quy xóa tất cả folder con và album)
    /// </summary>
    public void DeleteFolder(string folderId)
    {
        // Xóa tất cả folder con trước
        var children = GetChildFolders(folderId);
        foreach (var child in children)
        {
            DeleteFolder(child.Id);
        }

        // Xóa tất cả album trong folder này
        using (var albumService = new SimpleAlbumService())
        {
            var albums = albumService.GetAlbumsByFolderId(folderId);
            foreach (var album in albums)
            {
                albumService.DeleteAlbum(album.Id);
            }
        }

        // Xóa folder
        var folders = _db.GetCollection<AlbumFolder>("albumFolders");
        folders.Delete(folderId);
    }

    /// <summary>
    /// Di chuyển folder sang folder cha mới
    /// </summary>
    public void MoveFolder(string folderId, string newParentId)
    {
        var folder = GetFolderById(folderId);
        if (folder == null) return;

        folder.ParentId = newParentId;

        // Rebuild Path
        if (!string.IsNullOrEmpty(newParentId))
        {
            var parent = GetFolderById(newParentId);
            folder.Path = parent != null
                ? $"{parent.Path}/{folder.Name}"
                : folder.Name;
        }
        else
        {
            folder.Path = folder.Name;
        }

        UpdateFolder(folder);

        // Rebuild Path cho tất cả children
        RebuildChildrenPaths(folderId);
    }

    /// <summary>
    /// Rebuild Path cho tất cả folder con (sau khi move hoặc rename)
    /// </summary>
    private void RebuildChildrenPaths(string parentId)
    {
        var children = GetChildFolders(parentId);
        foreach (var child in children)
        {
            var parent = GetFolderById(child.ParentId);
            if (parent != null)
            {
                child.Path = $"{parent.Path}/{child.Name}";
                UpdateFolder(child);
                RebuildChildrenPaths(child.Id); // Đệ quy
            }
        }
    }

    /// <summary>
    /// Đổi tên folder và rebuild Path cho tất cả children
    /// </summary>
    public void RenameFolder(string folderId, string newName)
    {
        var folder = GetFolderById(folderId);
        if (folder == null) return;

        folder.Name = newName;

        // Rebuild Path
        if (!string.IsNullOrEmpty(folder.ParentId))
        {
            var parent = GetFolderById(folder.ParentId);
            folder.Path = parent != null
                ? $"{parent.Path}/{newName}"
                : newName;
        }
        else
        {
            folder.Path = newName;
        }

        UpdateFolder(folder);

        // Rebuild Path cho tất cả children
        RebuildChildrenPaths(folderId);
    }

    /// <summary>
    /// Cập nhật số lượng album và photo trong folder
    /// </summary>
    public void UpdateFolderStats(string folderId)
    {
        var folder = GetFolderById(folderId);
        if (folder == null) return;

        using (var albumService = new SimpleAlbumService())
        {
            var albums = albumService.GetAlbumsByFolderId(folderId);

            folder.AlbumCount = albums.Count;
            folder.TotalPhotoCount = albums.Sum(a => a.PhotoCount);

            // Lấy cover photo từ album đầu tiên
            var firstAlbum = albums.FirstOrDefault();
            if (firstAlbum != null && !string.IsNullOrEmpty(firstAlbum.CoverPhotoPath))
            {
                folder.CoverPhotoPath = firstAlbum.CoverPhotoPath;
            }

            UpdateFolder(folder);
        }
    }

    #endregion

    #region Apply Album Structure Template

    /// <summary>
    /// Áp dụng cấu trúc từ AlbumStructureTemplate
    /// Tạo cây folder theo Organization → Category → SubCategory
    /// </summary>
    public AlbumFolder ApplyTemplate(AlbumStructureTemplate template, string organizationName)
    {
        // 1. Tạo folder gốc (Organization)
        var orgFolder = new AlbumFolder
        {
            Name = organizationName,
            Icon = "🏢",
            Color = "#2196F3",
            FolderType = "Organization",
            TemplateId = template.Id,
            Description = $"Album ảnh của {organizationName}",
            SortOrder = 0
        };
        CreateFolder(orgFolder);

        // 2. Tạo các Category folder
        foreach (var category in template.Categories.OrderBy(c => c.SortOrder))
        {
            var categoryFolder = new AlbumFolder
            {
                Name = category.Name,
                ParentId = orgFolder.Id,
                Icon = category.Icon,
                Color = "#FF9800",
                FolderType = "Category",
                TemplateId = template.Id,
                CategoryId = category.Id,
                Description = category.Description,
                SortOrder = category.SortOrder
            };
            CreateFolder(categoryFolder);

            // 3. Tạo các SubCategory folder
            foreach (var subCategory in category.SubCategories.OrderBy(s => s.SortOrder))
            {
                var subCategoryFolder = new AlbumFolder
                {
                    Name = subCategory.Name,
                    ParentId = categoryFolder.Id,
                    Icon = subCategory.Icon,
                    Color = "#4CAF50",
                    FolderType = "SubCategory",
                    TemplateId = template.Id,
                    CategoryId = category.Id,
                    SubCategoryId = subCategory.Id,
                    Description = subCategory.Description,
                    SortOrder = subCategory.SortOrder
                };
                CreateFolder(subCategoryFolder);
            }
        }

        return orgFolder;
    }

    /// <summary>
    /// Kiểm tra xem đã apply template chưa
    /// </summary>
    public bool HasAppliedTemplate()
    {
        var folders = _db.GetCollection<AlbumFolder>("albumFolders");
        return folders.Count() > 0;
    }

    /// <summary>
    /// Xóa tất cả folder (reset)
    /// </summary>
    public void ClearAllFolders()
    {
        var rootFolders = GetRootFolders();
        foreach (var folder in rootFolders)
        {
            DeleteFolder(folder.Id);
        }
    }

    #endregion

    public void Dispose()
    {
        // Không dispose _db — DatabaseFactory quản lý vòng đời shared instance
    }
}
