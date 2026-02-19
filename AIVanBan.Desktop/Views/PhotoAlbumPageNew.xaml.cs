using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;

namespace AIVanBan.Desktop.Views
{
    public partial class PhotoAlbumPageNew : Page
    {
        private readonly DocumentService _documentService;
        private readonly AlbumStructureService _albumService;
        private readonly string _photosBasePath;
        private AlbumNode? _selectedAlbum;
        private ObservableCollection<AlbumNode> _albumTree = new();
        private AlbumStructureTemplate? _activeTemplate;

        public PhotoAlbumPageNew(DocumentService documentService)
        {
            InitializeComponent();
            _documentService = documentService;
            _albumService = new AlbumStructureService();
            
            // Photos base path with folder structure
            _photosBasePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "AIVanBan",
                "Photos"
            );
            
            Directory.CreateDirectory(_photosBasePath);
            
            // Load active template
            _activeTemplate = _albumService.GetActiveTemplate();
            
            LoadAlbumTree();
        }

        private void LoadAlbumTree()
        {
            _albumTree.Clear();
            
            // Root: All Photos
            var root = new AlbumNode
            {
                Id = "",
                DisplayName = "Tất cả ảnh",
                Icon = "🖼️",
                FolderPath = _photosBasePath,
                Children = new ObservableCollection<AlbumNode>()
            };
            
            // Load from template structure if available
            if (_activeTemplate != null)
            {
                LoadFromTemplate(root, _activeTemplate);
            }
            else
            {
                // Fallback: Load physical folders (old way)
                LoadFolderStructure(root, _photosBasePath);
            }
            
            // Count photos
            root.PhotoCount = CountPhotosRecursive(root);
            
            _albumTree.Add(root);
            tvAlbums.ItemsSource = _albumTree;
        }

        private void LoadFromTemplate(AlbumNode root, AlbumStructureTemplate template)
        {
            try
            {
                foreach (var category in template.Categories.OrderBy(c => c.SortOrder))
                {
                    var categoryPath = Path.Combine(_photosBasePath, category.Name);
                    
                    // Create category node
                    var categoryNode = new AlbumNode
                    {
                        Id = category.Id,
                        DisplayName = $"{category.Icon} {category.Name}",
                        Icon = category.Icon,
                        FolderPath = categoryPath,
                        Children = new ObservableCollection<AlbumNode>()
                    };
                    
                    // Load subcategories
                    foreach (var subCategory in category.SubCategories.OrderBy(s => s.SortOrder))
                    {
                        var subCategoryPath = Path.Combine(categoryPath, subCategory.Name);
                        
                        var subNode = new AlbumNode
                        {
                            Id = subCategory.Id,
                            DisplayName = $"{subCategory.Icon} {subCategory.Name}",
                            Icon = subCategory.Icon,
                            FolderPath = subCategoryPath,
                            Children = new ObservableCollection<AlbumNode>()
                        };
                        
                        // Load year folders if exists
                        if (Directory.Exists(subCategoryPath))
                        {
                            LoadYearFolders(subNode, subCategoryPath);
                        }
                        
                        // Count photos
                        if (Directory.Exists(subCategoryPath))
                        {
                            subNode.PhotoCount = CountPhotosInFolder(subCategoryPath, true);
                        }
                        
                        categoryNode.Children.Add(subNode);
                    }
                    
                    // Count total photos in category
                    categoryNode.PhotoCount = categoryNode.Children.Sum(c => c.PhotoCount);
                    
                    root.Children.Add(categoryNode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi load từ template: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadYearFolders(AlbumNode parent, string folderPath)
        {
            try
            {
                var yearDirs = Directory.GetDirectories(folderPath)
                    .Select(d => new DirectoryInfo(d))
                    .Where(d => int.TryParse(d.Name, out _)) // Only year folders
                    .OrderByDescending(d => d.Name);
                
                foreach (var yearDir in yearDirs)
                {
                    var yearNode = new AlbumNode
                    {
                        Id = yearDir.Name,
                        DisplayName = $"📅 {yearDir.Name}",
                        Icon = "📅",
                        FolderPath = yearDir.FullName,
                        Children = new ObservableCollection<AlbumNode>(),
                        PhotoCount = CountPhotosInFolder(yearDir.FullName, false)
                    };
                    
                    parent.Children.Add(yearNode);
                }
            }
            catch { }
        }

        private void LoadFolderStructure(AlbumNode parent, string folderPath)
        {
            try
            {
                var directories = Directory.GetDirectories(folderPath);
                foreach (var dir in directories)
                {
                    var dirInfo = new DirectoryInfo(dir);
                    var node = new AlbumNode
                    {
                        Id = dirInfo.Name,
                        DisplayName = dirInfo.Name,
                        Icon = "📁",
                        FolderPath = dir,
                        Children = new ObservableCollection<AlbumNode>()
                    };
                    
                    // Recursive load subfolders
                    LoadFolderStructure(node, dir);
                    
                    // Count photos in this folder
                    node.PhotoCount = CountPhotosInFolder(dir, false);
                    
                    parent.Children.Add(node);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi load folder: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int CountPhotosInFolder(string folderPath, bool recursive)
        {
            try
            {
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var extensions = new[] { "*.jpg", "*.jpeg", "*.png", "*.gif", "*.bmp" };
                return extensions.Sum(ext => Directory.GetFiles(folderPath, ext, searchOption).Length);
            }
            catch
            {
                return 0;
            }
        }

        private int CountPhotosRecursive(AlbumNode node)
        {
            int count = CountPhotosInFolder(node.FolderPath, false);
            foreach (var child in node.Children)
            {
                count += CountPhotosRecursive(child);
            }
            return count;
        }
        
        /// <summary>
        /// Đợi TreeView generate xong tất cả containers trước khi thực hiện action
        /// </summary>
        private void WaitForTreeViewReady(Action action)
        {
            if (tvAlbums.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                // Containers đã ready, thực hiện ngay
                Dispatcher.BeginInvoke(action, System.Windows.Threading.DispatcherPriority.Background);
            }
            else
            {
                // Đợi event StatusChanged
                EventHandler? handler = null;
                handler = (s, e) =>
                {
                    if (tvAlbums.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                    {
                        tvAlbums.ItemContainerGenerator.StatusChanged -= handler;
                        Dispatcher.BeginInvoke(action, System.Windows.Threading.DispatcherPriority.Background);
                    }
                };
                tvAlbums.ItemContainerGenerator.StatusChanged += handler;
            }
        }
        
        /// <summary>
        /// Tự động select và scroll đến album theo đường dẫn
        /// </summary>
        private void SelectAndScrollToAlbum(string albumPath)
        {
            // Normalize path
            albumPath = albumPath.Replace("/", "\\").TrimEnd('\\');
            
            // Tìm node trong tree
            var node = FindNodeByPath(_albumTree, albumPath);
            if (node != null)
            {
                // Clear previous selection
                ClearSelection(_albumTree);
                
                // Expand all parent nodes
                ExpandNodePath(node);
                
                // Update layout to ensure expanded nodes generate their containers
                tvAlbums.UpdateLayout();
                
                // Select node (data binding sẽ update TreeViewItem)
                node.IsSelected = true;
                
                // Scroll to visible
                var treeViewItem = FindTreeViewItemByNode(tvAlbums, node);
                if (treeViewItem != null)
                {
                    treeViewItem.BringIntoView();
                    treeViewItem.Focus();
                }
            }
        }
        
        private void ClearSelection(IEnumerable<AlbumNode> nodes)
        {
            foreach (var node in nodes)
            {
                node.IsSelected = false;
                ClearSelection(node.Children);
            }
        }
        
        private void ExpandNodePath(AlbumNode node)
        {
            // Expand all parent nodes
            var parents = new List<AlbumNode>();
            var current = FindParentNode(_albumTree, node, null);
            while (current != null)
            {
                parents.Add(current);
                current = FindParentNode(_albumTree, current, null);
            }
            
            // Expand from root to leaf
            parents.Reverse();
            foreach (var parent in parents)
            {
                parent.IsExpanded = true;
            }
            
            // Expand target node itself
            node.IsExpanded = true;
        }
        
        private AlbumNode? FindParentNode(IEnumerable<AlbumNode> nodes, AlbumNode target, AlbumNode? parent)
        {
            foreach (var node in nodes)
            {
                if (node.Children.Contains(target))
                {
                    return node;
                }
                
                var result = FindParentNode(node.Children, target, node);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
        
        private AlbumNode? FindNodeByPath(IEnumerable<AlbumNode> nodes, string path)
        {
            foreach (var node in nodes)
            {
                var nodePath = node.FolderPath.Replace("/", "\\").TrimEnd('\\');
                if (nodePath.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    return node;
                }
                
                // Search in children recursively
                var childResult = FindNodeByPath(node.Children, path);
                if (childResult != null)
                {
                    return childResult;
                }
            }
            return null;
        }
        
        private TreeViewItem? FindTreeViewItemByNode(ItemsControl container, AlbumNode node)
        {
            if (container == null) return null;
            
            // Update layout to ensure containers are generated
            container.UpdateLayout();
            
            var itemContainer = container.ItemContainerGenerator.ContainerFromItem(node) as TreeViewItem;
            if (itemContainer != null)
            {
                return itemContainer;
            }
            
            // Search in children
            foreach (var childItem in container.Items)
            {
                var childContainer = container.ItemContainerGenerator.ContainerFromItem(childItem) as TreeViewItem;
                if (childContainer != null)
                {
                    var result = FindTreeViewItemByNode(childContainer, node);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            
            return null;
        }

        private void AlbumTree_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (tvAlbums.SelectedItem is AlbumNode node)
            {
                _selectedAlbum = node;
                LoadPhotos(node);
                ShowAlbumDetails(node);
            }
        }

        private void LoadPhotos(AlbumNode album)
        {
            photoPanel.Children.Clear();
            
            txtCurrentAlbum.Text = album.DisplayName;
            
            var photoFiles = new List<string>();
            var extensions = new[] { "*.jpg", "*.jpeg", "*.png", "*.gif", "*.bmp" };
            
            foreach (var ext in extensions)
            {
                photoFiles.AddRange(Directory.GetFiles(album.FolderPath, ext, SearchOption.TopDirectoryOnly));
            }
            
            txtPhotoCount.Text = $"{photoFiles.Count} ảnh";
            
            if (photoFiles.Count == 0)
            {
                emptyState.Visibility = Visibility.Visible;
                return;
            }
            
            emptyState.Visibility = Visibility.Collapsed;
            
            foreach (var photoPath in photoFiles)
            {
                CreatePhotoCard(photoPath);
            }
        }

        private void CreatePhotoCard(string photoPath)
        {
            var card = new MaterialDesignThemes.Wpf.Card
            {
                Width = 150,
                Height = 180,
                Margin = new Thickness(5),
                Cursor = Cursors.Hand
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Image
            try
            {
                var img = new Image
                {
                    Source = new BitmapImage(new Uri(photoPath)),
                    Stretch = Stretch.UniformToFill
                };
                Grid.SetRow(img, 0);
                grid.Children.Add(img);
            }
            catch { }

            // File name
            var fileName = Path.GetFileName(photoPath);
            var textBlock = new TextBlock
            {
                Text = fileName,
                FontSize = 11,
                Margin = new Thickness(5),
                TextTrimming = TextTrimming.CharacterEllipsis,
                ToolTip = fileName
            };
            Grid.SetRow(textBlock, 1);
            grid.Children.Add(textBlock);

            card.Content = grid;
            card.Tag = photoPath;
            card.MouseLeftButtonUp += (s, e) =>
            {
                // Open photo viewer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = photoPath,
                    UseShellExecute = true
                });
            };

            photoPanel.Children.Add(card);
        }

        private void ShowAlbumDetails(AlbumNode album)
        {
            if (album.Id == "")
            {
                // Root - hide details
                albumInfoPanel.Visibility = Visibility.Collapsed;
                emptySelectionPanel.Visibility = Visibility.Visible;
                return;
            }

            emptySelectionPanel.Visibility = Visibility.Collapsed;
            albumInfoPanel.Visibility = Visibility.Visible;

            // Load metadata
            var metadata = LoadAlbumMetadata(album.FolderPath);
            
            txtAlbumTitle.Text = metadata.Title;
            txtAlbumDescription.Text = metadata.Description;
            txtDetailPhotoCount.Text = album.PhotoCount.ToString();
            
            var dirInfo = new DirectoryInfo(album.FolderPath);
            txtCreatedDate.Text = dirInfo.CreationTime.ToString("dd/MM/yyyy HH:mm");
        }

        private void SaveAlbumMetadata(string albumPath, string title, string description)
        {
            try
            {
                var metadata = new AlbumMetadata
                {
                    Title = title,
                    Description = description,
                    CreatedDate = DateTime.Now
                };
                
                var jsonPath = Path.Combine(albumPath, "album.json");
                var json = System.Text.Json.JsonSerializer.Serialize(metadata, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                File.WriteAllText(jsonPath, json, System.Text.Encoding.UTF8);
            }
            catch { }
        }

        private AlbumMetadata LoadAlbumMetadata(string albumPath)
        {
            try
            {
                var jsonPath = Path.Combine(albumPath, "album.json");
                if (File.Exists(jsonPath))
                {
                    var json = File.ReadAllText(jsonPath, System.Text.Encoding.UTF8);
                    return System.Text.Json.JsonSerializer.Deserialize<AlbumMetadata>(json) ?? new AlbumMetadata();
                }
            }
            catch { }
            
            // Return default with folder name
            var dirName = new DirectoryInfo(albumPath).Name;
            return new AlbumMetadata { Title = dirName, Description = "" };
        }

        private void CreateAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTemplate == null)
            {
                // No template - use old simple dialog
                CreateAlbumSimple();
                return;
            }

            // Show template-based album creation dialog
            var dialog = new CreateAlbumFromTemplateDialog(_activeTemplate, _albumService)
            {
                Owner = Window.GetWindow(this)
            };
            if (dialog.ShowDialog() == true && dialog.CreatedAlbum != null)
            {
                LoadAlbumTree();
                
                // Đợi TreeView generate containers xong rồi mới select
                WaitForTreeViewReady(() => 
                {
                    SelectAndScrollToAlbum(dialog.CreatedAlbum.FullPath);
                });
                
                MessageBox.Show(
                    $"✅ Đã tạo album:\n{dialog.CreatedAlbum.Name}\n\nĐường dẫn: {dialog.CreatedAlbum.FullPath}",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void CreateAlbumSimple()
        {
            var dialog = new PhotoAlbumInputDialog("Tạo album mới")
            {
                Owner = Window.GetWindow(this)
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var albumName = dialog.AlbumName.Trim();
                    var albumDescription = dialog.AlbumDescription.Trim();
                    
                    if (string.IsNullOrWhiteSpace(albumName))
                    {
                        MessageBox.Show("Vui lòng nhập tên album!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    var albumPath = Path.Combine(_photosBasePath, albumName);
                    
                    if (Directory.Exists(albumPath))
                    {
                        MessageBox.Show("Album đã tồn tại!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    Directory.CreateDirectory(albumPath);
                    
                    // Save metadata
                    SaveAlbumMetadata(albumPath, albumName, albumDescription);
                    
                    LoadAlbumTree();
                    
                    MessageBox.Show($"✅ Đã tạo album '{albumName}'!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tạo album:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UploadPhotos_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAlbum == null || _selectedAlbum.Id == "") return;

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Chọn ảnh để upload",
                Filter = "Hình ảnh|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                int successCount = 0;
                foreach (var file in dialog.FileNames)
                {
                    try
                    {
                        var fileName = Path.GetFileName(file);
                        var destPath = Path.Combine(_selectedAlbum.FolderPath, fileName);
                        File.Copy(file, destPath, true);
                        successCount++;
                    }
                    catch { }
                }

                LoadPhotos(_selectedAlbum);
                LoadAlbumTree(); // Refresh counts
                
                MessageBox.Show($"✅ Đã upload {successCount}/{dialog.FileNames.Length} ảnh!", 
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DownloadFromWeb_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var simpleAlbumService = new SimpleAlbumService();
                var dialog = new AlbumDownloadDialog(simpleAlbumService)
                {
                    Owner = Window.GetWindow(this)
                };

                if (dialog.ShowDialog() == true && dialog.DownloadedCount > 0)
                {
                    // Refresh album list after download
                    LoadAlbumTree();
                    
                    MessageBox.Show(
                        $"✅ Đã tải {dialog.DownloadedCount} album từ website!\nDanh sách album đã được cập nhật.",
                        "Tải thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở trang tải album:\n{ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAlbum == null || _selectedAlbum.Id == "") return;

            var metadata = LoadAlbumMetadata(_selectedAlbum.FolderPath);
            var dialog = new PhotoAlbumInputDialog("Sửa thông tin album", metadata.Title, metadata.Description)
            {
                Owner = Window.GetWindow(this)
            };
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var newTitle = dialog.AlbumName.Trim();
                    var newDescription = dialog.AlbumDescription.Trim();
                    
                    if (string.IsNullOrWhiteSpace(newTitle))
                    {
                        MessageBox.Show("Vui lòng nhập tiêu đề!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    // Update metadata
                    SaveAlbumMetadata(_selectedAlbum.FolderPath, newTitle, newDescription);
                    
                    // Refresh display
                    ShowAlbumDetails(_selectedAlbum);
                    
                    MessageBox.Show("✅ Đã cập nhật thông tin album!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAlbum == null || _selectedAlbum.Id == "") return;

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa album '{_selectedAlbum.DisplayName}'?\nTất cả ảnh trong album sẽ bị xóa!",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Directory.Delete(_selectedAlbum.FolderPath, true);
                    LoadAlbumTree();
                    MessageBox.Show("✅ Đã xóa album!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Search_KeyUp(object sender, KeyEventArgs e)
        {
            // TODO: Implement search
        }
    }

    public class AlbumNode : INotifyPropertyChanged
    {
        private bool _isExpanded = true; // Default expanded
        private bool _isSelected;
        
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Icon { get; set; } = "📁";
        public string FolderPath { get; set; } = "";
        public int PhotoCount { get; set; }
        public string CountText => PhotoCount > 0 ? $"({PhotoCount})" : "";
        public ObservableCollection<AlbumNode> Children { get; set; } = new();
        
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }
        
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AlbumMetadata
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    // Simple text input dialog
    public class TextInputDialog : Window
    {
        public string ResultText { get; private set; } = "";

        public TextInputDialog(string title, string prompt, string defaultText = "")
        {
            Title = title;
            Width = 400;
            Height = 180;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            var stack = new StackPanel { Margin = new Thickness(20) };
            
            stack.Children.Add(new TextBlock { Text = prompt, Margin = new Thickness(0, 0, 0, 10) });
            
            var textBox = new TextBox { Text = defaultText };
            stack.Children.Add(textBox);
            
            var buttonPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };
            
            var okButton = new Button { Content = "OK", Width = 80, Margin = new Thickness(0, 0, 10, 0) };
            okButton.Click += (s, e) => { ResultText = textBox.Text; DialogResult = true; };
            
            var cancelButton = new Button { Content = "Hủy", Width = 80 };
            cancelButton.Click += (s, e) => { DialogResult = false; };
            
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stack.Children.Add(buttonPanel);
            
            Content = stack;
        }
    }
}
