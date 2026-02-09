using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public enum AlbumViewMode
{
    Grid,    // Lưới 2x2 thumbnail lớn (300x300)
    Cards,   // Card ngang thumbnail vừa (180x140)
    List     // Danh sách thumbnail nhỏ (100x100)
}

/// <summary>
/// ViewModel cho TreeView Folder
/// </summary>
public class FolderTreeItem : INotifyPropertyChanged
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "📁";
    public int AlbumCount { get; set; }
    public ObservableCollection<FolderTreeItem> Children { get; set; } = new();
    
    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
            }
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
}

public partial class PhotoAlbumPageSimple : Page
{
    private readonly SimpleAlbumService _albumService;
    private readonly AlbumFolderService _folderService;
    private List<SimpleAlbum> _allAlbums = new();
    private string? _selectedAlbumId;
    private string? _currentFolderId = null;  // null = show all albums (no folder filter)
    private AlbumViewMode _currentViewMode = AlbumViewMode.Grid;

    public PhotoAlbumPageSimple()
    {
        InitializeComponent();
        
        var photosPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "AIVanBan", "Photos");
        
        _albumService = new SimpleAlbumService(photosPath);
        _folderService = new AlbumFolderService();
        
        Loaded += (s, e) =>
        {
            LoadFolderTree();
            LoadAlbums();  // Show all albums initially
            Focus();
            Keyboard.Focus(this);
        };
    }

    #region Folder Tree Management

    private void LoadFolderTree()
    {
        // Lưu trạng thái expanded và selected trước khi reload
        var expandedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string? selectedId = null;
        
        if (folderTree.ItemsSource is IEnumerable<FolderTreeItem> oldItems)
        {
            CollectExpandedIds(oldItems, expandedIds);
        }
        if (folderTree.SelectedItem is FolderTreeItem selectedItem)
        {
            selectedId = selectedItem.Id;
        }

        var rootFolders = _folderService.GetRootFolders();
        
        var treeItems = new List<FolderTreeItem>();
        
        // Add "All Albums" virtual folder
        var allAlbums = _albumService.GetAllAlbums();
        treeItems.Add(new FolderTreeItem
        {
            Id = "",  // Empty ID means "show all"
            Name = "Tất cả Album",
            Icon = "📂",
            AlbumCount = allAlbums.Count
        });
        
        // Add "Unclassified Albums" virtual folder (if any)
        var unclassifiedAlbums = _albumService.GetAlbumsWithoutFolder();
        if (unclassifiedAlbums.Count > 0)
        {
            treeItems.Add(new FolderTreeItem
            {
                Id = "UNCLASSIFIED",
                Name = "Album chưa phân loại",
                Icon = "📦",
                AlbumCount = unclassifiedAlbums.Count
            });
        }
        
        // Add real folders
        foreach (var folder in rootFolders)
        {
            treeItems.Add(BuildTreeItem(folder));
        }
        
        // Khôi phục trạng thái expanded
        RestoreExpandedState(treeItems, expandedIds);
        
        folderTree.ItemsSource = treeItems;
    }

    /// <summary>
    /// Thu thập các folder ID đang expanded
    /// </summary>
    private void CollectExpandedIds(IEnumerable<FolderTreeItem> items, HashSet<string> expandedIds)
    {
        foreach (var item in items)
        {
            if (item.IsExpanded && !string.IsNullOrEmpty(item.Id))
            {
                expandedIds.Add(item.Id);
            }
            if (item.Children.Count > 0)
            {
                CollectExpandedIds(item.Children, expandedIds);
            }
        }
    }

    /// <summary>
    /// Khôi phục trạng thái expanded cho tree items mới
    /// </summary>
    private void RestoreExpandedState(IEnumerable<FolderTreeItem> items, HashSet<string> expandedIds)
    {
        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.Id) && expandedIds.Contains(item.Id))
            {
                item.IsExpanded = true;
            }
            if (item.Children.Count > 0)
            {
                RestoreExpandedState(item.Children, expandedIds);
            }
        }
    }

    /// <summary>
    /// Expand folder trong tree hiện tại (trước khi reload) để lưu trạng thái
    /// </summary>
    private void ExpandFolderInCurrentTree(string folderId)
    {
        if (folderTree.ItemsSource is IEnumerable<FolderTreeItem> items)
        {
            SetExpandedById(items, folderId);
        }
    }

    private bool SetExpandedById(IEnumerable<FolderTreeItem> items, string folderId)
    {
        foreach (var item in items)
        {
            if (item.Id == folderId)
            {
                item.IsExpanded = true;
                return true;
            }
            if (item.Children.Count > 0 && SetExpandedById(item.Children, folderId))
            {
                item.IsExpanded = true; // Expand cha để thấy được folder con
                return true;
            }
        }
        return false;
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
        
        // Load children recursively
        var children = _folderService.GetChildFolders(folder.Id);
        foreach (var child in children)
        {
            item.Children.Add(BuildTreeItem(child));
        }
        
        return item;
    }

    private void FolderTree_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is FolderTreeItem selected)
        {
            // Nếu đang xem album chi tiết, quay lại danh sách trước
            if (albumDetailFrame.Visibility == Visibility.Visible)
            {
                albumDetailFrame.Visibility = Visibility.Collapsed;
                albumDetailFrame.Content = null;
                albumListView.Visibility = Visibility.Visible;
            }
            
            LoadFolderContent(selected);
        }
    }

    private void LoadAlbumsInFolder(string folderId)
    {
        try
        {
            var folder = _folderService.GetFolderById(folderId);
            var folderIds = GetDescendantFolderIds(folderId);

            var allAlbums = _albumService.GetAllAlbums();
            var albums = allAlbums.Where(a =>
                    (!string.IsNullOrEmpty(a.AlbumFolderId) && folderIds.Contains(a.AlbumFolderId)) ||
                    (!string.IsNullOrEmpty(folder?.Path) && !string.IsNullOrEmpty(a.AlbumFolderPath) &&
                     (a.AlbumFolderPath.Equals(folder.Path, StringComparison.OrdinalIgnoreCase) ||
                      a.AlbumFolderPath.StartsWith(folder.Path + "/", StringComparison.OrdinalIgnoreCase)))
                )
                .ToList();

            _allAlbums = albums;
            DisplayAlbums(albums);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error in LoadAlbumsInFolder: {ex.Message}");
            MessageBox.Show($"Lỗi khi load albums: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private HashSet<string> GetDescendantFolderIds(string folderId)
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(folderId)) return ids;

        var stack = new Stack<string>();
        stack.Push(folderId);
        ids.Add(folderId);

        while (stack.Count > 0)
        {
            var currentId = stack.Pop();
            var children = _folderService.GetChildFolders(currentId);
            foreach (var child in children)
            {
                if (ids.Add(child.Id))
                {
                    stack.Push(child.Id);
                }
            }
        }

        return ids;
    }

    private void LoadUnclassifiedAlbums()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔍 LoadUnclassifiedAlbums");
            
            // Get albums without folder assignment
            var albums = _albumService.GetAlbumsWithoutFolder();
            
            System.Diagnostics.Debug.WriteLine($"📦 Found {albums.Count} unclassified albums");
            foreach (var album in albums)
            {
                System.Diagnostics.Debug.WriteLine($"  - Album: {album.Title}, PhotoCount: {album.PhotoCount}");
            }
            
            _allAlbums = albums;
            DisplayAlbums(albums);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error in LoadUnclassifiedAlbums: {ex.Message}");
            MessageBox.Show($"Lỗi khi load albums chưa phân loại: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CreateFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = CreateFolderInputDialog("Tạo thư mục mới", "", "");
            dialog.Owner = Window.GetWindow(this);
            
            if (dialog.ShowDialog() == true && dialog.Tag is string folderName && !string.IsNullOrWhiteSpace(folderName))
            {
                var folder = new AlbumFolder
                {
                    Name = folderName.Trim(),
                    Icon = "📁",
                    ParentId = "" // Root folder
                };
                
                _folderService.CreateFolder(folder);
                LoadFolderTree();
                MessageBox.Show($"✅ Đã tạo thư mục '{folderName}'", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating folder: {ex.Message}");
            MessageBox.Show("Có lỗi khi tạo thư mục!", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Tạo thư mục con từ nút trên header hoặc empty state
    /// Khi không trong folder nào -> tạo root folder
    /// Khi trong folder -> tạo subfolder
    /// </summary>
    private void CreateSubFolderFromHeader_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string parentId;
            string dialogTitle;

            if (string.IsNullOrEmpty(_currentFolderId) || _currentFolderId == "UNCLASSIFIED")
            {
                // Tạo root folder
                parentId = "";
                dialogTitle = "Tạo thư mục mới";
            }
            else
            {
                // Tạo subfolder
                var parentFolder = _folderService.GetFolderById(_currentFolderId);
                var parentName = parentFolder?.Name ?? "thư mục hiện tại";
                parentId = _currentFolderId;
                dialogTitle = $"Tạo thư mục con trong '{parentName}'";
            }

            var dialog = CreateFolderInputDialog(dialogTitle, "", parentId);
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true && dialog.Tag is string folderName && !string.IsNullOrWhiteSpace(folderName))
            {
                var folder = new AlbumFolder
                {
                    Name = folderName.Trim(),
                    Icon = "📁",
                    ParentId = parentId
                };

                _folderService.CreateFolder(folder);
                
                // Expand parent trong tree hiện tại trước khi reload
                if (!string.IsNullOrEmpty(parentId))
                {
                    ExpandFolderInCurrentTree(parentId);
                }
                LoadFolderTree();

                // Refresh current folder view to show new child folder
                if (!string.IsNullOrEmpty(_currentFolderId) && _currentFolderId != "UNCLASSIFIED")
                {
                    var currentFolderItem = new FolderTreeItem 
                    { 
                        Id = _currentFolderId, 
                        Name = _folderService.GetFolderById(_currentFolderId)?.Name ?? "" 
                    };
                    LoadFolderContent(currentFolderItem);
                }

                MessageBox.Show($"✅ Đã tạo thư mục '{folderName}'", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating folder: {ex.Message}");
            MessageBox.Show("Có lỗi khi tạo thư mục!", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Tạo album trong thư mục - từ context menu của cây thư mục
    /// </summary>
    private void CreateAlbumInFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu menu 
                && menu.PlacementTarget is FrameworkElement element 
                && element.Tag is FolderTreeItem folder)
            {
                var dialog = new AlbumEditorDialog(null, _albumService)
                {
                    Owner = Window.GetWindow(this)
                };
                
                if (dialog.ShowDialog() == true && dialog.ResultAlbum != null)
                {
                    _albumService.MoveAlbumToFolder(dialog.ResultAlbum.Id, folder.Id);
                    
                    // Refresh tree and current view
                    LoadFolderTree();
                    if (_currentFolderId == folder.Id)
                    {
                        LoadAlbumsInFolder(folder.Id);
                    }

                    MessageBox.Show($"✅ Đã tạo album trong '{folder.Name}'", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating album in folder: {ex.Message}");
            MessageBox.Show("Có lỗi khi tạo album!", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RefreshFolders_Click(object sender, RoutedEventArgs e)
    {
        LoadFolderTree();
        LoadAlbums();
    }

    private void ExpandAll_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SetAllFoldersExpanded(folderTree.Items, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error expanding tree: {ex.Message}");
        }
    }
    
    private void CollapseAll_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SetAllFoldersExpanded(folderTree.Items, false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error collapsing tree: {ex.Message}");
        }
    }
    
    private void SetAllFoldersExpanded(System.Collections.IEnumerable items, bool isExpanded)
    {
        foreach (var item in items)
        {
            if (item is FolderTreeItem folder)
            {
                folder.IsExpanded = isExpanded;
                
                // Recursively expand/collapse all children
                if (folder.Children.Count > 0)
                {
                    SetAllFoldersExpanded(folder.Children, isExpanded);
                }
            }
        }
    }

    private void FolderTreeItem_RightClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is StackPanel panel && panel.Tag is FolderTreeItem folder)
        {
            // Context menu will show automatically
        }
    }

    private void LoadFolderContent(FolderTreeItem folder)
    {
        _currentFolderId = string.IsNullOrEmpty(folder.Id) ? null : folder.Id;

        if (_currentFolderId == null)
        {
            HideChildFolders();
            LoadAlbums();
            txtSubtitle.Text = "Tất cả album";
            txtEmptyTitle.Text = "Chưa có album nào";
            txtEmptyDesc.Text = "Hãy tạo album đầu tiên để bắt đầu quản lý ảnh của bạn";
        }
        else if (_currentFolderId == "UNCLASSIFIED")
        {
            HideChildFolders();
            LoadUnclassifiedAlbums();
            txtSubtitle.Text = "📦 Album chưa phân loại";
            txtEmptyTitle.Text = "Không có album chưa phân loại";
            txtEmptyDesc.Text = "Tất cả album đã được phân loại vào thư mục";
        }
        else
        {
            var childCount = DisplayChildFolders(folder.Id);
            LoadAlbumsInFolder(folder.Id);
            txtSubtitle.Text = $"📁 {folder.Name}";
            txtEmptyTitle.Text = "Thư mục trống";
            txtEmptyDesc.Text = "Tạo thư mục con hoặc album ảnh để tổ chức dữ liệu";
        }
    }

    private int DisplayChildFolders(string folderId)
    {
        try
        {
            var children = _folderService.GetChildFolders(folderId);
            if (children != null && children.Count > 0)
            {
                var folderItems = children.Select(f => new FolderTreeItem
                {
                    Id = f.Id,
                    Name = f.Name,
                    Icon = string.IsNullOrEmpty(f.Icon) ? "📁" : f.Icon,
                    AlbumCount = f.AlbumCount
                }).ToList();

                childFoldersPanel.ItemsSource = folderItems;
                childFoldersPanel.Visibility = Visibility.Visible;
                txtChildFoldersHeader.Visibility = Visibility.Visible;
                txtChildFoldersHeader.Text = $"📁 Thư mục con ({children.Count})";
                return children.Count;
            }
            else
            {
                HideChildFolders();
                return 0;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading child folders: {ex.Message}");
            HideChildFolders();
            return 0;
        }
    }

    private void HideChildFolders()
    {
        if (childFoldersPanel != null)
        {
            childFoldersPanel.ItemsSource = null;
            childFoldersPanel.Visibility = Visibility.Collapsed;
        }
        if (txtChildFoldersHeader != null)
        {
            txtChildFoldersHeader.Visibility = Visibility.Collapsed;
        }
    }

    private void ChildFolderCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is string folderId)
        {
            // Find and select the corresponding tree item
            SelectFolderInTree(folderId, folderTree.Items);
        }
    }

    private bool SelectFolderInTree(string folderId, System.Collections.IEnumerable items)
    {
        foreach (var item in items)
        {
            if (item is FolderTreeItem folderItem)
            {
                if (folderItem.Id == folderId)
                {
                    // Found it - need to select it in TreeView
                    // First expand parent, then select
                    var treeViewItem = FindTreeViewItem(folderTree, folderItem);
                    if (treeViewItem != null)
                    {
                        treeViewItem.IsSelected = true;
                        treeViewItem.BringIntoView();
                    }
                    else
                    {
                        // Fallback: directly load the folder content
                        LoadFolderContent(folderItem);
                    }
                    return true;
                }

                if (folderItem.Children.Count > 0)
                {
                    folderItem.IsExpanded = true;
                    if (SelectFolderInTree(folderId, folderItem.Children))
                        return true;
                }
            }
        }
        return false;
    }

    private TreeViewItem? FindTreeViewItem(ItemsControl container, object item)
    {
        if (container == null) return null;

        // Force generation of containers
        container.UpdateLayout();
        container.ApplyTemplate();

        for (int i = 0; i < container.Items.Count; i++)
        {
            var currentItem = container.Items[i];
            var treeViewItem = container.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
            
            if (treeViewItem == null) continue;

            if (currentItem == item)
                return treeViewItem;

            // Search children
            treeViewItem.IsExpanded = true;
            treeViewItem.UpdateLayout();
            var result = FindTreeViewItem(treeViewItem, item);
            if (result != null) return result;
        }
        return null;
    }

    private void OpenFolderMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu menu 
            && menu.PlacementTarget is FrameworkElement element 
            && element.Tag is FolderTreeItem folder)
        {
            LoadFolderContent(folder);
        }
    }

    private void CreateSubFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Get parent folder from context menu
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu menu 
                && menu.PlacementTarget is FrameworkElement element 
                && element.Tag is FolderTreeItem parentFolder)
            {
                var dialog = CreateFolderInputDialog("Thêm thư mục con", "", parentFolder.Id);
                dialog.Owner = Window.GetWindow(this);
                
                if (dialog.ShowDialog() == true && dialog.Tag is string folderName && !string.IsNullOrWhiteSpace(folderName))
                {
                    var folder = new AlbumFolder
                    {
                        Name = folderName.Trim(),
                        Icon = "📁",
                        ParentId = parentFolder.Id
                    };
                    
                    _folderService.CreateFolder(folder);
                    
                    // Expand parent để thấy folder con mới
                    parentFolder.IsExpanded = true;
                    LoadFolderTree();
                    
                    MessageBox.Show($"✅ Đã tạo thư mục con '{folderName}' trong '{parentFolder.Name}'", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating subfolder: {ex.Message}");
            MessageBox.Show("Có lỗi khi tạo thư mục con!", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RenameFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu menu 
                && menu.PlacementTarget is FrameworkElement element 
                && element.Tag is FolderTreeItem folder)
            {
                var dialog = CreateFolderInputDialog("Đổi tên thư mục", folder.Name, folder.Id);
                dialog.Owner = Window.GetWindow(this);
                
                if (dialog.ShowDialog() == true && dialog.Tag is string newName && !string.IsNullOrWhiteSpace(newName))
                {
                    _folderService.RenameFolder(folder.Id, newName.Trim());
                    LoadFolderTree();
                    MessageBox.Show($"✅ Đã đổi tên thành '{newName}'", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error renaming folder: {ex.Message}");
            MessageBox.Show("Có lỗi khi đổi tên thư mục!", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DeleteFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu menu 
                && menu.PlacementTarget is FrameworkElement element 
                && element.Tag is FolderTreeItem folder)
            {
                var result = MessageBox.Show(
                    $"Bạn có chắc muốn xóa thư mục '{folder.Name}'?\nTất cả thư mục con và album trong đó cũng sẽ bị xóa.",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _folderService.DeleteFolder(folder.Id);
                    LoadFolderTree();
                    LoadAlbums();
                    MessageBox.Show("✅ Đã xóa thư mục thành công!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting folder: {ex.Message}");
            MessageBox.Show("Có lỗi khi xóa thư mục!", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    private void LoadAlbums()
    {
        _allAlbums = _albumService.GetAllAlbums();
        System.Diagnostics.Debug.WriteLine($"LoadAlbums: Found {_allAlbums.Count} albums");
        DisplayAlbums(_allAlbums);
        UpdateStats();
    }

    private void DisplayAlbums(List<SimpleAlbum> albums)
    {
        if (albumsPanel == null || emptyState == null) return;

        var displayList = albums?.ToList() ?? new List<SimpleAlbum>();

        albumsPanel.ItemsSource = null;
        albumsPanel.ItemsSource = displayList;

        txtAlbumCount.Text = $"{displayList.Count} albums";
        var totalPhotos = displayList.Sum(a => a.PhotoCount);
        txtTotalPhotos.Text = $"{totalPhotos} ảnh";

        // Update folder count in stats bar
        var childFolderCount = 0;
        if (_currentFolderId != null && _currentFolderId != "UNCLASSIFIED")
        {
            var children = _folderService.GetChildFolders(_currentFolderId);
            childFolderCount = children?.Count ?? 0;
        }
        txtFolderCount.Text = $"{childFolderCount} thư mục";

        // Show/hide albums section header when child folders are visible
        bool hasChildFolders = childFoldersPanel.Visibility == Visibility.Visible;
        txtAlbumsHeader.Visibility = (hasChildFolders || displayList.Count > 0) && _currentFolderId != null && _currentFolderId != "UNCLASSIFIED" 
            ? Visibility.Visible : Visibility.Collapsed;
        if (txtAlbumsHeader.Visibility == Visibility.Visible)
            txtAlbumsHeader.Text = $"📸 Album ảnh ({displayList.Count})";

        // Only show empty state when BOTH no child folders AND no albums
        bool showEmpty = displayList.Count == 0 && !hasChildFolders;

        if (showEmpty)
        {
            emptyState.Visibility = Visibility.Visible;
            albumsPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            emptyState.Visibility = Visibility.Collapsed;
            albumsPanel.Visibility = Visibility.Visible;

            albumsPanel.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var album in displayList)
                {
                    LoadAlbumThumbnails(album);
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    private void LoadAlbumThumbnails(SimpleAlbum album)
    {
        if (album.ThumbnailPhotos == null || album.ThumbnailPhotos.Count == 0)
            return;

        // Find the card for this album - force container generation
        albumsPanel.UpdateLayout();
        var container = albumsPanel.ItemContainerGenerator.ContainerFromItem(album) as FrameworkElement;
        if (container == null) return;

        var thumbnailGrid = FindVisualChild<Grid>(container, "thumbnailGrid");
        var emptyPlaceholder = FindVisualChild<StackPanel>(container, "emptyPlaceholder");
        
        if (thumbnailGrid == null || emptyPlaceholder == null) return;

        // Show thumbnail grid, hide placeholder
        thumbnailGrid.Visibility = Visibility.Visible;
        emptyPlaceholder.Visibility = Visibility.Collapsed;

        // Load up to 4 thumbnails
        var thumbImages = new[] {
            FindVisualChild<Image>(container, "thumb1"),
            FindVisualChild<Image>(container, "thumb2"),
            FindVisualChild<Image>(container, "thumb3"),
            FindVisualChild<Image>(container, "thumb4")
        };

        Task.Run(() =>
        {
            for (int i = 0; i < Math.Min(4, album.ThumbnailPhotos.Count); i++)
            {
                var index = i;
                try
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelWidth = 200; // Thumbnail size
                    bitmap.UriSource = new Uri(album.ThumbnailPhotos[index], UriKind.Absolute);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    
                    Dispatcher.Invoke(() =>
                    {
                        if (thumbImages[index] != null)
                        {
                            thumbImages[index].Source = bitmap;
                        }
                    });
                }
                catch
                {
                    // Skip failed images
                }
            }
        });
    }

    private void UpdateStats()
    {
        txtAlbumCount.Text = $"{_allAlbums.Count} albums";
        var totalPhotos = _allAlbums.Sum(a => a.PhotoCount);
        txtTotalPhotos.Text = $"{totalPhotos} ảnh";
        
        if (txtSubtitle != null)
        {
            txtSubtitle.Text = _allAlbums.Count == 0 
                ? "Chưa có album nào - Hãy tạo album đầu tiên" 
                : $"Tổ chức và quản lý {_allAlbums.Count} album ảnh của bạn";
        }
    }

    private void Sort_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cboSort == null || cboSort.SelectedIndex < 0 || _allAlbums == null || _allAlbums.Count == 0) return;
        
        var sortedAlbums = cboSort.SelectedIndex switch
        {
            0 => _allAlbums.OrderByDescending(a => a.CreatedDate).ToList(), // Mới nhất
            1 => _allAlbums.OrderBy(a => a.CreatedDate).ToList(), // Cũ nhất
            2 => _allAlbums.OrderBy(a => a.Title).ToList(), // A-Z
            3 => _allAlbums.OrderByDescending(a => a.Title).ToList(), // Z-A
            4 => _allAlbums.OrderByDescending(a => a.PhotoCount).ToList(), // Nhiều ảnh nhất
            _ => _allAlbums
        };
        
        DisplayAlbums(sortedAlbums);
    }

    private void ViewMode_Click(object sender, RoutedEventArgs e)
    {
        // Cycle through 3 modes: Grid -> Cards -> List -> Grid
        _currentViewMode = _currentViewMode switch
        {
            AlbumViewMode.Grid => AlbumViewMode.Cards,
            AlbumViewMode.Cards => AlbumViewMode.List,
            AlbumViewMode.List => AlbumViewMode.Grid,
            _ => AlbumViewMode.Grid
        };

        UpdateViewMode();
    }

    private void UpdateViewMode()
    {
        if (albumsPanel == null) return;

        switch (_currentViewMode)
        {
            case AlbumViewMode.Grid:
                // Grid view - WrapPanel with large 300x300 cards
                albumsPanel.ItemsPanel = (ItemsPanelTemplate)Resources["GridViewPanel"];
                albumsPanel.ItemTemplate = (DataTemplate)Resources["GridViewTemplate"];
                if (btnViewMode != null) btnViewMode.ToolTip = "Chế độ xem: Lưới";
                if (iconViewMode != null) iconViewMode.Kind = MaterialDesignThemes.Wpf.PackIconKind.ViewGrid;
                break;

            case AlbumViewMode.Cards:
                // Cards view - StackPanel with medium 180x140 horizontal cards
                albumsPanel.ItemsPanel = (ItemsPanelTemplate)Resources["CardsViewPanel"];
                albumsPanel.ItemTemplate = (DataTemplate)Resources["CardsViewTemplate"];
                if (btnViewMode != null) btnViewMode.ToolTip = "Chế độ xem: Thẻ";
                if (iconViewMode != null) iconViewMode.Kind = MaterialDesignThemes.Wpf.PackIconKind.ViewModule;
                break;

            case AlbumViewMode.List:
                // List view - StackPanel with small 100x100 list items
                albumsPanel.ItemsPanel = (ItemsPanelTemplate)Resources["ListViewPanel"];
                albumsPanel.ItemTemplate = (DataTemplate)Resources["ListViewTemplate"];
                if (btnViewMode != null) btnViewMode.ToolTip = "Chế độ xem: Danh sách";
                if (iconViewMode != null) iconViewMode.Kind = MaterialDesignThemes.Wpf.PackIconKind.ViewList;
                break;
        }

        // Refresh data
        var currentAlbums = albumsPanel.ItemsSource as List<SimpleAlbum>;
        if (currentAlbums != null)
        {
            DisplayAlbums(currentAlbums);
        }
    }

    private void CreateAlbum_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AlbumEditorDialog(null, _albumService)
        {
            Owner = Window.GetWindow(this)
        };
        
        if (dialog.ShowDialog() == true)
        {
            if (dialog.ResultAlbum != null &&
                !string.IsNullOrEmpty(_currentFolderId) &&
                _currentFolderId != "UNCLASSIFIED")
            {
                _albumService.MoveAlbumToFolder(dialog.ResultAlbum.Id, _currentFolderId);
                LoadAlbumsInFolder(_currentFolderId);
            }
            else
            {
                LoadAlbums();
            }
        }
    }

    private void AlbumCard_Select(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag != null)
        {
            var albumId = element.Tag.ToString();
            if (string.IsNullOrWhiteSpace(albumId))
            {
                return;
            }

            _selectedAlbumId = albumId;
            UpdateAlbumSelection(element);

            if (e.ClickCount == 2)
            {
                // Show album detail in embedded frame (keep folder tree visible)
                ShowAlbumDetail(albumId);
            }
        }
    }

    private void UpdateAlbumSelection(FrameworkElement selectedCard)
    {
        // Force layout update để container được generate
        albumsPanel.UpdateLayout();
        
        // Clear all selections
        foreach (var item in albumsPanel.Items)
        {
            var container = albumsPanel.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
            if (container != null)
            {
                var border = FindVisualChild<Border>(container, "selectedBorder");
                if (border != null)
                {
                    border.Visibility = Visibility.Collapsed;
                }
            }
        }

        // Highlight selected
        var selectedBorder = FindVisualChild<Border>(selectedCard, "selectedBorder");
        if (selectedBorder != null)
        {
            selectedBorder.Visibility = Visibility.Visible;
        }
    }

    private void PhotoAlbumPage_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !string.IsNullOrEmpty(_selectedAlbumId))
        {
            ShowAlbumDetail(_selectedAlbumId);
            e.Handled = true;
        }
        // Escape key to go back from album detail to album list
        if (e.Key == Key.Escape && albumDetailFrame.Visibility == Visibility.Visible)
        {
            HideAlbumDetail();
            e.Handled = true;
        }
    }

    private T? FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T element && element.Name == name)
            {
                return element;
            }

            var result = FindVisualChild<T>(child, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    #region Embedded Album Detail

    /// <summary>
    /// Show album detail inside embedded frame (folder tree stays visible)
    /// </summary>
    private void ShowAlbumDetail(string albumId)
    {
        var detailPage = new AlbumDetailPage(albumId, _albumService);
        detailPage.BackRequested += AlbumDetail_BackRequested;
        
        albumDetailFrame.Navigate(detailPage);
        albumDetailFrame.Visibility = Visibility.Visible;
        albumListView.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Return from album detail to album list
    /// </summary>
    public void HideAlbumDetail()
    {
        albumDetailFrame.Visibility = Visibility.Collapsed;
        albumListView.Visibility = Visibility.Visible;
        albumDetailFrame.Content = null;
        
        // Refresh album list to reflect any changes made in detail view
        if (_currentFolderId == null)
            LoadAlbums();
        else if (_currentFolderId == "UNCLASSIFIED")
            LoadUnclassifiedAlbums();
        else
            LoadAlbumsInFolder(_currentFolderId);
    }

    private void AlbumDetail_BackRequested(object? sender, EventArgs e)
    {
        HideAlbumDetail();
    }

    #endregion

    private void AlbumCard_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.Cursor = Cursors.Hand;
            // Trigger hover animation would go here
        }
    }

    private void AlbumCard_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.Cursor = Cursors.Arrow;
        }
    }

    private void OpenAlbum_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && GetAlbumIdFromMenuItem(menuItem, out var albumId))
        {
            ShowAlbumDetail(albumId);
        }
    }

    private void AddPhotosToAlbum_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && GetAlbumIdFromMenuItem(menuItem, out var albumId))
        {
            var album = _allAlbums.FirstOrDefault(a => a.Id == albumId);
            if (album != null)
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = $"Chọn ảnh để thêm vào album '{album.Title}'",
                    Multiselect = true,
                    Filter = "Image files (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All files (*.*)|*.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        int successCount = 0;
                        int errorCount = 0;

                        foreach (var filePath in openFileDialog.FileNames)
                        {
                            try
                            {
                                // Copy file to album folder
                                var fileName = System.IO.Path.GetFileName(filePath);
                                var destPath = System.IO.Path.Combine(album.FolderPath, fileName);
                                
                                // Handle duplicate filenames
                                int counter = 1;
                                while (File.Exists(destPath))
                                {
                                    var nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(fileName);
                                    var ext = System.IO.Path.GetExtension(fileName);
                                    destPath = System.IO.Path.Combine(album.FolderPath, $"{nameWithoutExt}_{counter}{ext}");
                                    counter++;
                                }
                                
                                File.Copy(filePath, destPath, false);
                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error adding photo {filePath}: {ex.Message}");
                                errorCount++;
                            }
                        }

                        // Update album to refresh photo count
                        _albumService.GetAlbumById(albumId); // This will refresh the count
                        LoadAlbums();

                        var message = $"✅ Đã thêm {successCount} ảnh vào album '{album.Title}'";
                        if (errorCount > 0)
                        {
                            message += $"\n⚠️ {errorCount} ảnh gặp lỗi khi thêm";
                        }
                        
                        MessageBox.Show(message, "Thành công", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Có lỗi khi thêm ảnh:\n{ex.Message}", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }

    private void MoveAlbumToFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && GetAlbumIdFromMenuItem(menuItem, out var albumId))
        {
            var album = _allAlbums.FirstOrDefault(a => a.Id == albumId);
            if (album != null)
            {
                // Create folder selection dialog
                var dialog = CreateFolderSelectionDialog();
                dialog.Owner = Window.GetWindow(this);
                
                if (dialog.ShowDialog() == true && dialog.Tag is string targetFolderId)
                {
                    try
                    {
                        _albumService.MoveAlbumToFolder(albumId, targetFolderId);
                        LoadFolderTree();
                        LoadAlbums();
                        
                        var targetFolder = _folderService.GetFolderById(targetFolderId);
                        var folderName = targetFolder != null ? targetFolder.Name : "Thư mục gốc";
                        
                        MessageBox.Show($"✅ Đã di chuyển album '{album.Title}' vào '{folderName}'", 
                            "Thành công",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Có lỗi khi di chuyển album:\n{ex.Message}", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }

    private void EditAlbumFromMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && GetAlbumIdFromMenuItem(menuItem, out var albumId))
        {
            var album = _allAlbums.FirstOrDefault(a => a.Id == albumId);
            if (album != null)
            {
                var dialog = new AlbumEditorDialog(album, _albumService)
                {
                    Owner = Window.GetWindow(this)
                };
                
                if (dialog.ShowDialog() == true)
                {
                    LoadAlbums();
                }
            }
        }
    }

    private void CopyAlbumPath_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && GetAlbumIdFromMenuItem(menuItem, out var albumId))
        {
            var album = _allAlbums.FirstOrDefault(a => a.Id == albumId);
            if (album != null)
            {
                Clipboard.SetText(album.FolderPath);
                MessageBox.Show("✅ Đã copy đường dẫn vào clipboard!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private void OpenAlbumFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && GetAlbumIdFromMenuItem(menuItem, out var albumId))
        {
            var album = _allAlbums.FirstOrDefault(a => a.Id == albumId);
            if (album != null && Directory.Exists(album.FolderPath))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{album.FolderPath}\"")
                {
                    UseShellExecute = true
                });
            }
        }
    }

    private void DeleteAlbumFromMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && GetAlbumIdFromMenuItem(menuItem, out var albumId))
        {
            var album = _allAlbums.FirstOrDefault(a => a.Id == albumId);
            if (album != null)
            {
                var result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa album '{album.Title}'?\n\n" +
                    $"⚠️ Tất cả {album.PhotoCount} ảnh trong album sẽ bị xóa vĩnh viễn!",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _albumService.DeleteAlbumPermanently(albumId);
                        MessageBox.Show("✅ Đã xóa album thành công!", "Thành công",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadAlbums();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa album:\n{ex.Message}", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }

    private bool GetAlbumIdFromMenuItem(MenuItem menuItem, out string albumId)
    {
        albumId = string.Empty;

        if (menuItem.Parent is ContextMenu contextMenu &&
            contextMenu.PlacementTarget is FrameworkElement element &&
            element.Tag != null)
        {
            var id = element.Tag.ToString();
            if (!string.IsNullOrWhiteSpace(id))
            {
                albumId = id;
                return true;
            }
        }

        return false;
    }

    private void RenameAlbum_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && GetAlbumIdFromMenuItem(menuItem, out var albumId))
        {
            var album = _allAlbums.FirstOrDefault(a => a.Id == albumId);
            if (album != null)
            {
                var dialog = new SimpleInputDialog("Đổi tên album", "Nhập tên mới:", album.Title);
                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
                {
                    album.Title = dialog.InputText;
                    _albumService.UpdateAlbum(album);
                    MessageBox.Show("✅ Đã đổi tên album thành công!", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAlbums();
                }
            }
        }
    }

    private void ExportAlbum_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && GetAlbumIdFromMenuItem(menuItem, out var albumId))
        {
            var album = _allAlbums.FirstOrDefault(a => a.Id == albumId);
            if (album != null)
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Chọn vị trí export album",
                    FileName = album.Title,
                    DefaultExt = ".folder",
                    Filter = "Folder|*.folder",
                    CheckFileExists = false,
                    CheckPathExists = true,
                    ValidateNames = false
                };

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        var baseDir = Path.GetDirectoryName(dialog.FileName);
                        if (string.IsNullOrWhiteSpace(baseDir))
                        {
                            MessageBox.Show("Không lấy được đường dẫn thư mục.", "Lỗi",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var destFolder = Path.Combine(baseDir, album.Title);
                        Directory.CreateDirectory(destFolder);

                        var photos = _albumService.GetPhotosInAlbum(albumId);
                        int count = 0;

                        foreach (var photo in photos)
                        {
                            var destPath = Path.Combine(destFolder, Path.GetFileName(photo.FilePath));
                            File.Copy(photo.FilePath, destPath, true);
                            count++;
                        }

                        MessageBox.Show($"✅ Đã export {count} ảnh vào:\n{destFolder}", "Thành công",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi export:\n{ex.Message}", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }

    private void DuplicateAlbum_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && GetAlbumIdFromMenuItem(menuItem, out var albumId))
        {
            var album = _allAlbums.FirstOrDefault(a => a.Id == albumId);
            if (album != null)
            {
                try
                {
                    var newAlbum = _albumService.CreateAlbum(
                        $"{album.Title} (Copy)",
                        album.Description,
                        album.Tags);

                    // Copy all photos
                    var photos = _albumService.GetPhotosInAlbum(albumId);
                    foreach (var photo in photos)
                    {
                        var destPath = Path.Combine(newAlbum.FolderPath, Path.GetFileName(photo.FilePath));
                        File.Copy(photo.FilePath, destPath);
                    }

                    MessageBox.Show($"✅ Đã nhân bản album với {photos.Count} ảnh!", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAlbums();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi nhân bản:\n{ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private void Search_TextChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = txtSearch.Text;

        if (string.IsNullOrWhiteSpace(keyword))
        {
            DisplayAlbums(_allAlbums);
            return;
        }

        var filteredGlobal = _albumService.SearchAlbums(keyword);
        var currentIds = new HashSet<string>(_allAlbums.Select(a => a.Id));
        var filtered = filteredGlobal.Where(a => currentIds.Contains(a.Id)).ToList();
        DisplayAlbums(filtered);
    }
    
    private Window CreateFolderInputDialog(string title, string defaultValue, string parentId)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 450,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            Background = System.Windows.Media.Brushes.White
        };
        
        var grid = new Grid { Margin = new Thickness(25) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        
        var lblName = new TextBlock 
        { 
            Text = "Tên thư mục:", 
            FontSize = 13,
            Margin = new Thickness(0, 0, 0, 8) 
        };
        var txtName = new TextBox 
        { 
            Text = defaultValue, 
            FontSize = 14,
            Padding = new Thickness(10, 8, 10, 8),
            Margin = new Thickness(0, 0, 0, 20) 
        };
        
        Grid.SetRow(lblName, 0);
        Grid.SetRow(txtName, 1);
        
        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetRow(btnPanel, 3);
        
        var btnSave = new Button 
        { 
            Content = "✓ Lưu", 
            Width = 100,
            Height = 36,
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80)),
            Foreground = System.Windows.Media.Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Margin = new Thickness(0, 0, 10, 0) 
        };
        
        var btnCancel = new Button 
        { 
            Content = "✕ Hủy", 
            Width = 100,
            Height = 36,
            FontSize = 13,
            Background = System.Windows.Media.Brushes.White,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 100)),
            BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200)),
            BorderThickness = new Thickness(1),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        
        btnSave.Click += (s, args) =>
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên thư mục!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }
            dialog.Tag = txtName.Text.Trim();
            dialog.DialogResult = true;
            dialog.Close();
        };
        
        btnCancel.Click += (s, args) => dialog.Close();
        
        // Enter to save, Esc to cancel
        txtName.KeyDown += (s, args) =>
        {
            if (args.Key == Key.Enter)
            {
                btnSave.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            else if (args.Key == Key.Escape)
            {
                dialog.Close();
            }
        };
        
        btnPanel.Children.Add(btnSave);
        btnPanel.Children.Add(btnCancel);
        
        var stack = new StackPanel();
        stack.Children.Add(lblName);
        stack.Children.Add(txtName);
        
        grid.Children.Add(stack);
        grid.Children.Add(btnPanel);
        dialog.Content = grid;
        
        dialog.Loaded += (s, e) =>
        {
            txtName.Focus();
            txtName.SelectAll();
        };
        
        return dialog;
    }
    
    private Window CreateFolderSelectionDialog()
    {
        var dialog = new Window
        {
            Title = "Chọn thư mục đích",
            Width = 450,
            Height = 500,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            Background = System.Windows.Media.Brushes.White
        };
        
        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        
        var lblInfo = new TextBlock
        {
            Text = "Chọn thư mục để di chuyển album:",
            FontSize = 13,
            Margin = new Thickness(0, 0, 0, 10)
        };
        Grid.SetRow(lblInfo, 0);
        
        // TreeView to show folders
        var folderTreeView = new TreeView
        {
            Background = System.Windows.Media.Brushes.White,
            BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10)
        };
        Grid.SetRow(folderTreeView, 1);
        
        // Load folders into TreeView
        var rootItem = new TreeViewItem
        {
            Header = "📁 Thư mục gốc (không có thư mục cha)",
            Tag = "",
            IsExpanded = true,
            FontWeight = FontWeights.Bold
        };
        folderTreeView.Items.Add(rootItem);
        
        LoadFolderItemsRecursive(rootItem, "");
        
        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 15, 0, 0)
        };
        Grid.SetRow(btnPanel, 2);
        
        var btnSelect = new Button
        {
            Content = "✓ Chọn",
            Width = 100,
            Height = 36,
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243)),
            Foreground = System.Windows.Media.Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Margin = new Thickness(0, 0, 10, 0)
        };
        
        var btnCancel = new Button
        {
            Content = "✕ Hủy",
            Width = 100,
            Height = 36,
            FontSize = 13,
            Background = System.Windows.Media.Brushes.White,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 100)),
            BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200)),
            BorderThickness = new Thickness(1),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        
        btnSelect.Click += (s, args) =>
        {
            if (folderTreeView.SelectedItem is TreeViewItem selectedItem)
            {
                dialog.Tag = selectedItem.Tag?.ToString() ?? "";
                dialog.DialogResult = true;
                dialog.Close();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một thư mục!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        };
        
        btnCancel.Click += (s, args) => dialog.Close();
        
        btnPanel.Children.Add(btnSelect);
        btnPanel.Children.Add(btnCancel);
        
        grid.Children.Add(lblInfo);
        grid.Children.Add(folderTreeView);
        grid.Children.Add(btnPanel);
        dialog.Content = grid;
        
        return dialog;
    }
    
    private void LoadFolderItemsRecursive(TreeViewItem parentItem, string parentId)
    {
        // Get root folders or child folders based on parentId
        var folders = string.IsNullOrEmpty(parentId) 
            ? _folderService.GetRootFolders() 
            : _folderService.GetChildFolders(parentId);
            
        foreach (var folder in folders)
        {
            var item = new TreeViewItem
            {
                Header = $"{folder.Icon} {folder.Name}",
                Tag = folder.Id,
                IsExpanded = true
            };
            parentItem.Items.Add(item);
            
            // Recursively load children
            LoadFolderItemsRecursive(item, folder.Id);
        }
    }
}

// Simple Input Dialog
public class SimpleInputDialog : Window
{
    public string InputText { get; private set; } = "";
    private TextBox _textBox;

    public SimpleInputDialog(string title, string prompt, string defaultValue = "")
    {
        Title = title;
        Width = 400;
        Height = 180;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var promptText = new TextBlock
        {
            Text = prompt,
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 10)
        };
        Grid.SetRow(promptText, 0);

        _textBox = new TextBox
        {
            Text = defaultValue,
            FontSize = 14,
            Padding = new Thickness(8),
            Margin = new Thickness(0, 0, 0, 15)
        };
        Grid.SetRow(_textBox, 1);
        _textBox.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter)
            {
                InputText = _textBox.Text;
                DialogResult = true;
                Close();
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetRow(buttonPanel, 2);

        var btnOK = new Button
        {
            Content = "OK",
            Width = 80,
            Height = 32,
            Margin = new Thickness(0, 0, 10, 0)
        };
        btnOK.Click += (s, e) =>
        {
            InputText = _textBox.Text;
            DialogResult = true;
            Close();
        };

        var btnCancel = new Button
        {
            Content = "Hủy",
            Width = 80,
            Height = 32
        };
        btnCancel.Click += (s, e) =>
        {
            DialogResult = false;
            Close();
        };

        buttonPanel.Children.Add(btnOK);
        buttonPanel.Children.Add(btnCancel);

        grid.Children.Add(promptText);
        grid.Children.Add(_textBox);
        grid.Children.Add(buttonPanel);

        Content = grid;

        Loaded += (s, e) =>
        {
            _textBox.Focus();
            _textBox.SelectAll();
        };
    }
}
