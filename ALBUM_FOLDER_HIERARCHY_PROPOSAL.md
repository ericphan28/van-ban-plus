# ĐỀ XUẤT: HỆ THỐNG QUẢN LÝ ALBUM THEO CẤU TRÚC CÂY NHIỀU CẤP

## 1. VẤN ĐỀ HIỆN TẠI

### Hệ thống Văn bản (Hoạt động tốt) ✅
```csharp
public class Folder
{
    public string ParentId { get; set; }    // Trỏ đến folder cha
    public string Path { get; set; }        // "Văn bản/Công văn/2024"
    
    // Methods hỗ trợ cây:
    // - GetRootFolders() → ParentId = ""
    // - GetChildFolders(parentId)
}
```

**Kết quả:** Có thể tạo cây thư mục nhiều cấp như Windows Explorer:
```
📁 Văn bản đến
  📁 Công văn
    📁 2024
    📁 2025
  📁 Quyết định
    📁 2024
```

### Hệ thống Album (Chỉ có 1 cấp) ❌
```csharp
public class SimpleAlbum
{
    // KHÔNG có ParentId
    // KHÔNG có Path
    // → Chỉ là danh sách phẳng!
}
```

**Kết quả:** Album Setup Template có cấu trúc cây nhưng khi Apply chỉ tạo list phẳng:
```
📷 Album 1
📷 Album 2
📷 Album 3
```

**Không thể tạo được cấu trúc:**
```
📁 Trường Tiểu học (Organization Root)
  📁 ALBUM ẢNH (Category)
    📁 Hoạt động giảng dạy (SubCategory)
      📷 Lớp học 1A (Album)
      📷 Lớp học 2B (Album)
    📁 Đời - Thiếu nhi
      📷 Ngày 1/6 (Album)
  📁 Sự kiện năm học
```

---

## 2. GIẢI PHÁP: TẠO MODEL `AlbumFolder` GIỐNG `Folder`

### Option 1: Tạo model mới `AlbumFolder` (KHUYẾN NGHỊ) ⭐

**Ưu điểm:**
- Tách biệt rõ ràng: Document folders ≠ Photo folders
- Dễ mở rộng tính năng riêng cho album (cover photo, photo count...)
- Không ảnh hưởng code cũ
- Database collection riêng: "albumFolders" vs "folders"

**Model mới:**
```csharp
namespace AIVanBan.Core.Models;

/// <summary>
/// Folder quản lý Album ảnh theo cấu trúc cây nhiều cấp
/// Tương tự Folder của Document nhưng dành riêng cho Album
/// </summary>
public class AlbumFolder
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Tên folder (vd: "Trường Tiểu học", "ALBUM ẢNH", "Hoạt động giảng dạy")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// ID folder cha (rỗng = root folder)
    /// </summary>
    public string ParentId { get; set; } = string.Empty;
    
    /// <summary>
    /// Đường dẫn đầy đủ (vd: "Trường Tiểu học/ALBUM ẢNH/Hoạt động giảng dạy")
    /// Tự động build khi tạo folder
    /// </summary>
    public string Path { get; set; } = string.Empty;
    
    /// <summary>
    /// Icon hiển thị
    /// </summary>
    public string Icon { get; set; } = "📁";
    
    /// <summary>
    /// Màu sắc folder
    /// </summary>
    public string Color { get; set; } = "#FF9800"; // Orange cho album
    
    /// <summary>
    /// Thứ tự sắp xếp
    /// </summary>
    public int SortOrder { get; set; } = 0;
    
    /// <summary>
    /// Số lượng album trong folder này (không đệ quy)
    /// </summary>
    public int AlbumCount { get; set; }
    
    /// <summary>
    /// Tổng số ảnh trong tất cả album (bao gồm subfolder)
    /// </summary>
    public int TotalPhotoCount { get; set; }
    
    /// <summary>
    /// Đường dẫn ảnh cover của folder (lấy từ album đầu tiên)
    /// </summary>
    public string? CoverPhotoPath { get; set; }
    
    /// <summary>
    /// Loại folder từ template
    /// "Organization" | "Category" | "SubCategory" | "Custom"
    /// </summary>
    public string FolderType { get; set; } = "Custom";
    
    /// <summary>
    /// Link với AlbumStructureTemplate (nếu tạo từ template)
    /// </summary>
    public string? TemplateId { get; set; }
    public string? CategoryId { get; set; }
    public string? SubCategoryId { get; set; }
    
    /// <summary>
    /// Mô tả folder
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Tags
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// Audit
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public string CreatedBy { get; set; } = Environment.UserName;
    public DateTime ModifiedDate { get; set; } = DateTime.Now;
    public string ModifiedBy { get; set; } = Environment.UserName;
}
```

**Cập nhật SimpleAlbum:**
```csharp
public class SimpleAlbum
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string FolderPath { get; set; } = "";
    
    // ===== THÊM MỚI =====
    /// <summary>
    /// ID của AlbumFolder chứa album này
    /// </summary>
    public string AlbumFolderId { get; set; } = string.Empty;
    
    /// <summary>
    /// Đường dẫn đầy đủ trong cây folder
    /// Vd: "Trường Tiểu học/ALBUM ẢNH/Hoạt động giảng dạy"
    /// </summary>
    public string AlbumFolderPath { get; set; } = string.Empty;
    // ====================
    
    public string? CoverPhotoPath { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime ModifiedDate { get; set; } = DateTime.Now;
    public int PhotoCount { get; set; }
    public string Status { get; set; } = "Active";
    public List<string> ThumbnailPhotos { get; set; } = new();
}
```

### Option 2: Dùng chung model `Folder` (KHÔNG KHUYẾN NGHỊ)

**Nhược điểm:**
- Document folders và Album folders lẫn lộn trong 1 collection
- Khó phân biệt, dễ nhầm lẫn
- Cần thêm field `FolderCategory` = "Document" | "Album"

---

## 3. SERVICE METHODS (GIỐNG DocumentService)

```csharp
namespace AIVanBan.Core.Services;

public class AlbumFolderService : IDisposable
{
    private readonly LiteDatabase _db;

    public AlbumFolderService(string? databasePath = null)
    {
        // Khởi tạo giống DocumentService
        _db = new LiteDatabase($"Filename={dbPath};Connection=Shared");
        
        // Index
        var folders = _db.GetCollection<AlbumFolder>("albumFolders");
        folders.EnsureIndex(x => x.ParentId);
        folders.EnsureIndex(x => x.Path);
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
    /// Xóa folder (đệ quy xóa tất cả folder con)
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
        var albumService = new SimpleAlbumService();
        var albums = albumService.GetAlbumsByFolderId(folderId);
        foreach (var album in albums)
        {
            albumService.DeleteAlbum(album.Id);
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
    /// Rebuild Path cho tất cả folder con (sau khi move)
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
    /// Cập nhật số lượng album trong folder
    /// </summary>
    public void UpdateAlbumCount(string folderId)
    {
        var folder = GetFolderById(folderId);
        if (folder == null) return;

        var albumService = new SimpleAlbumService();
        var albums = albumService.GetAlbumsByFolderId(folderId);
        
        folder.AlbumCount = albums.Count;
        folder.TotalPhotoCount = albums.Sum(a => a.PhotoCount);
        
        // Lấy cover photo từ album đầu tiên
        var firstAlbum = albums.FirstOrDefault();
        if (firstAlbum != null)
        {
            folder.CoverPhotoPath = firstAlbum.CoverPhotoPath;
        }

        UpdateFolder(folder);
    }

    #endregion

    #region Apply Album Structure Template

    /// <summary>
    /// Áp dụng cấu trúc từ AlbumStructureTemplate
    /// Tạo cây folder theo Organization → Category → SubCategory
    /// </summary>
    public void ApplyTemplate(AlbumStructureTemplate template, string organizationName)
    {
        // 1. Tạo folder gốc (Organization)
        var orgFolder = new AlbumFolder
        {
            Name = organizationName,
            Icon = "🏢",
            FolderType = "Organization",
            TemplateId = template.Id,
            Description = $"Album ảnh của {organizationName}"
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
    }

    #endregion

    public void Dispose()
    {
        _db?.Dispose();
    }
}
```

---

## 4. CẬP NHẬT SimpleAlbumService

```csharp
// Thêm methods mới vào SimpleAlbumService.cs

/// <summary>
/// Lấy tất cả album trong 1 folder
/// </summary>
public List<SimpleAlbum> GetAlbumsByFolderId(string folderId)
{
    var collection = _db.GetCollection<SimpleAlbum>("simpleAlbums");
    return collection.Find(a => a.AlbumFolderId == folderId)
                     .OrderByDescending(a => a.CreatedDate)
                     .ToList();
}

/// <summary>
/// Lấy tất cả album trong folder và tất cả subfolder
/// </summary>
public List<SimpleAlbum> GetAlbumsRecursive(string folderId)
{
    var albums = new List<SimpleAlbum>();
    
    // Lấy album trong folder hiện tại
    albums.AddRange(GetAlbumsByFolderId(folderId));
    
    // Lấy album trong tất cả subfolder
    var folderService = new AlbumFolderService();
    var children = folderService.GetChildFolders(folderId);
    foreach (var child in children)
    {
        albums.AddRange(GetAlbumsRecursive(child.Id));
    }
    
    return albums;
}

/// <summary>
/// Di chuyển album sang folder khác
/// </summary>
public void MoveAlbumToFolder(string albumId, string targetFolderId)
{
    var album = GetAlbum(albumId);
    if (album == null) return;
    
    var folderService = new AlbumFolderService();
    var targetFolder = folderService.GetFolderById(targetFolderId);
    if (targetFolder == null) return;
    
    // Cập nhật folder của album
    album.AlbumFolderId = targetFolderId;
    album.AlbumFolderPath = targetFolder.Path;
    album.ModifiedDate = DateTime.Now;
    
    UpdateAlbum(album);
    
    // Cập nhật AlbumCount của folder cũ và mới
    var oldFolderId = album.AlbumFolderId;
    if (!string.IsNullOrEmpty(oldFolderId))
    {
        folderService.UpdateAlbumCount(oldFolderId);
    }
    folderService.UpdateAlbumCount(targetFolderId);
}
```

---

## 5. UI IMPLEMENTATION

### PhotoAlbumPageSimple.xaml - Thêm TreeView cho Folder

```xaml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="250"/> <!-- Folder tree -->
        <ColumnDefinition Width="*"/>   <!-- Album grid -->
    </Grid.ColumnDefinitions>

    <!-- Left: Folder Tree -->
    <Border Grid.Column="0" Background="White" 
            BorderBrush="#E0E0E0" BorderThickness="0,0,1,0">
        <DockPanel>
            <TextBlock DockPanel.Dock="Top" Text="📁 THƯ MỤC ALBUM"
                       FontSize="16" FontWeight="Bold"
                       Padding="16,12" Background="#F5F5F5"/>
            
            <TreeView x:Name="folderTree"
                      SelectionChanged="FolderTree_SelectionChanged">
                <TreeView.ItemTemplate>
                    <HierarchicalDataTemplate ItemsSource="{Binding Children}">
                        <StackPanel Orientation="Horizontal">
                            <TextBlock Text="{Binding Icon}" Margin="0,0,8,0"/>
                            <TextBlock Text="{Binding Name}"/>
                            <TextBlock Text="{Binding AlbumCount, StringFormat=' ({0})'}"
                                       Foreground="Gray" Margin="4,0,0,0"/>
                        </StackPanel>
                    </HierarchicalDataTemplate>
                </TreeView.ItemTemplate>
            </TreeView>
        </DockPanel>
    </Border>

    <!-- Right: Album Grid (existing) -->
    <DockPanel Grid.Column="1">
        <!-- Toolbar -->
        <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" Padding="16,12">
            <Button Content="📁 Thư mục mới" Click="CreateFolder_Click"/>
            <Button Content="➕ Album mới" Click="CreateAlbum_Click"/>
            <Button Content="📤 Import từ template" Click="ApplyTemplate_Click"/>
        </StackPanel>

        <!-- Album Grid -->
        <ScrollViewer>
            <ItemsControl x:Name="albumsPanel">
                <!-- Existing album cards -->
            </ItemsControl>
        </ScrollViewer>
    </DockPanel>
</Grid>
```

### Code-behind với TreeView

```csharp
// PhotoAlbumPageSimple.xaml.cs

private AlbumFolderService _folderService = new();
private string? _currentFolderId = null;

private void LoadFolderTree()
{
    var rootFolders = _folderService.GetRootFolders();
    
    var treeItems = new List<FolderTreeItem>();
    foreach (var folder in rootFolders)
    {
        treeItems.Add(BuildTreeItem(folder));
    }
    
    folderTree.ItemsSource = treeItems;
}

private FolderTreeItem BuildTreeItem(AlbumFolder folder)
{
    var item = new FolderTreeItem
    {
        Id = folder.Id,
        Name = folder.Name,
        Icon = folder.Icon,
        AlbumCount = folder.AlbumCount
    };
    
    // Load children
    var children = _folderService.GetChildFolders(folder.Id);
    foreach (var child in children)
    {
        item.Children.Add(BuildTreeItem(child));
    }
    
    return item;
}

private void FolderTree_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
{
    if (folderTree.SelectedItem is FolderTreeItem selected)
    {
        _currentFolderId = selected.Id;
        LoadAlbumsInFolder(selected.Id);
    }
}

private void LoadAlbumsInFolder(string folderId)
{
    var albums = _albumService.GetAlbumsByFolderId(folderId);
    
    // Convert to SimplePhotoAlbum for display
    var albumViewModels = albums.Select(a => new SimplePhotoAlbum
    {
        Id = a.Id,
        Title = a.Title,
        PhotoCount = a.PhotoCount,
        CoverPhoto = LoadBitmapImage(a.CoverPhotoPath),
        CreatedDate = a.CreatedDate
    }).ToList();
    
    albumsPanel.ItemsSource = albumViewModels;
}

// ViewModel cho TreeView
public class FolderTreeItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "📁";
    public int AlbumCount { get; set; }
    public ObservableCollection<FolderTreeItem> Children { get; set; } = new();
}
```

---

## 6. LUỒNG SỬ DỤNG

### Áp dụng Template (giống dialog hiện tại)

```
1. User mở "Thiết lập cấu trúc Album"
2. Chọn loại cơ quan: "Trường Tiểu học"
3. Nhập tên: "Trường Tiểu học Lê Quý Đôn"
4. Click "Áp dụng cấu trúc này"

→ AlbumFolderService.ApplyTemplate() tạo:

📁 Trường Tiểu học Lê Quý Đôn (Organization Root)
  📁 ALBUM ẢNH (4 danh mục chính)
    📁 Hoạt động giảng dạy (3 phân loại)
      [User tạo album ở đây]
    📁 Dự giờ - Kiểm tra
    📁 Ngoại khóa
  📁 Đời - Thiếu nhi (3 phân loại)
    📁 Sinh hoạt Đội
    📁 Kết nạp Đội viên
    📁 Hoạt động Đội
  📁 Sự kiện năm học
```

### Tạo Album trong Folder

```
1. User click vào folder "Hoạt động giảng dạy" trong TreeView
2. Click nút "➕ Album mới"
3. Dialog hiện ra với:
   - Folder hiện tại: "Trường.../ALBUM ẢNH/Hoạt động giảng dạy"
   - Title: "Lớp 1A - Môn Toán"
   - Description: ...
4. Lưu → Album được tạo với:
   - AlbumFolderId = ID của "Hoạt động giảng dạy"
   - AlbumFolderPath = "Trường.../ALBUM ẢNH/Hoạt động giảng dạy"
```

### Di chuyển Album

```
1. Right-click album → "Di chuyển"
2. Chọn folder đích trong TreeView
3. Album.AlbumFolderId = newFolderId
4. Cập nhật AlbumCount của cả 2 folder (cũ & mới)
```

---

## 7. LỘ TRÌNH TRIỂN KHAI

### Phase 1: Core Models & Services (1-2 giờ)
- [ ] Tạo `AlbumFolder.cs`
- [ ] Cập nhật `SimpleAlbum.cs` thêm `AlbumFolderId`, `AlbumFolderPath`
- [ ] Tạo `AlbumFolderService.cs` với tất cả methods
- [ ] Cập nhật `SimpleAlbumService.cs` thêm `GetAlbumsByFolderId()`
- [ ] Cập nhật `AlbumStructureService.cs` sử dụng `AlbumFolderService.ApplyTemplate()`

### Phase 2: UI - TreeView (2-3 giờ)
- [ ] Cập nhật `PhotoAlbumPageSimple.xaml` thêm TreeView
- [ ] Implement `LoadFolderTree()`, `BuildTreeItem()`
- [ ] Implement `FolderTree_SelectionChanged()`
- [ ] Implement `LoadAlbumsInFolder()`

### Phase 3: CRUD Operations (1-2 giờ)
- [ ] Dialog tạo folder mới
- [ ] Dialog đổi tên folder
- [ ] Right-click menu: Delete, Rename, Move
- [ ] Drag & drop album giữa các folder

### Phase 4: Template Apply (1 giờ)
- [ ] Cập nhật dialog "Thiết lập cấu trúc Album"
- [ ] Gọi `AlbumFolderService.ApplyTemplate()` khi Apply
- [ ] Hiển thị cây folder sau khi Apply

### Phase 5: Testing & Polish (1 giờ)
- [ ] Test tạo/xóa/move folder nhiều cấp
- [ ] Test AlbumCount tự động update
- [ ] Test load performance với nhiều folder
- [ ] Polish UI: icons, colors, animations

**Tổng thời gian ước tính: 6-9 giờ**

---

## 8. KẾT LUẬN

### Trước khi triển khai:
```
❌ Album chỉ là list phẳng
❌ Không quản lý được cấu trúc cây
❌ Template Apply không tạo được hierarchy
```

### Sau khi triển khai:
```
✅ Album có cấu trúc cây nhiều cấp giống Document
✅ TreeView quản lý folder trực quan
✅ Template Apply tạo đầy đủ Organization → Category → SubCategory
✅ Move, Rename, Delete folder đệ quy
✅ AlbumCount tự động update
✅ UI giống Windows Explorer
```

### So sánh với Document Management:
| Tính năng | Document | Album (Sau khi cải tiến) |
|-----------|----------|--------------------------|
| Cấu trúc cây | ✅ Folder với ParentId | ✅ AlbumFolder với ParentId |
| TreeView UI | ✅ Có | ✅ Có |
| Drag & Drop | ✅ Có | ✅ Sẽ có |
| Path hierarchy | ✅ "Văn bản/Công văn/2024" | ✅ "Org/Category/SubCategory" |
| Template support | ✅ OrganizationSetup | ✅ AlbumStructure |

---

## PHỤ LỤC: SO SÁNH CODE

### Document Management (Reference)
```csharp
// Folder.cs
public class Folder {
    public string ParentId { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

// DocumentService.cs
public List<Folder> GetRootFolders() {
    return folders.Find(f => string.IsNullOrEmpty(f.ParentId)).ToList();
}

public List<Folder> GetChildFolders(string parentId) {
    return folders.Find(f => f.ParentId == parentId).ToList();
}
```

### Album Management (Proposed)
```csharp
// AlbumFolder.cs (TẠO MỚI)
public class AlbumFolder {
    public string ParentId { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

// AlbumFolderService.cs (TẠO MỚI)
public List<AlbumFolder> GetRootFolders() {
    return folders.Find(f => string.IsNullOrEmpty(f.ParentId)).ToList();
}

public List<AlbumFolder> GetChildFolders(string parentId) {
    return folders.Find(f => f.ParentId == parentId).ToList();
}
```

**→ GIỐNG HỆT NHAU! Chỉ đổi tên từ `Folder` sang `AlbumFolder`**

