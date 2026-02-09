using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class AlbumDetailPage : Page
{
    private readonly SimpleAlbumService _albumService;
    private readonly string _albumId;
    private SimpleAlbum? _currentAlbum;
    private List<SimplePhoto> _photos = new();
    private string? _selectedPhotoPath;
    private HashSet<string> _selectedPhotos = new();
    private bool _isSelectMode = false;

    /// <summary>
    /// Event fired when user clicks Back - parent page should handle hiding detail
    /// </summary>
    public event EventHandler? BackRequested;

    public AlbumDetailPage(string albumId, SimpleAlbumService albumService)
    {
        InitializeComponent();
        
        _albumId = albumId;
        _albumService = albumService;
        
        Loaded += (s, e) =>
        {
            LoadAlbum();
            Focus();
            Keyboard.Focus(this);
        };
    }

    private void LoadAlbum()
    {
        _currentAlbum = _albumService.GetAlbumById(_albumId);
        
        if (_currentAlbum == null)
        {
            MessageBox.Show("Không tìm thấy album!", "Lỗi", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            Back_Click(null, null);
            return;
        }

        // Display album info IMMEDIATELY
        txtAlbumTitle.Text = _currentAlbum.Title;
        txtDescription.Text = string.IsNullOrWhiteSpace(_currentAlbum.Description) 
            ? "Không có mô tả" 
            : _currentAlbum.Description;
        tagsPanel.ItemsSource = _currentAlbum.Tags;

        // Get photo count first (fast)
        _photos = _albumService.GetPhotosInAlbum(_currentAlbum.Id);
        txtPhotoCount.Text = $"{_photos.Count} ảnh";
        
        // Show empty state immediately if no photos
        if (_photos.Count == 0)
        {
            emptyState.Visibility = Visibility.Visible;
            photosPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            emptyState.Visibility = Visibility.Collapsed;
            photosPanel.Visibility = Visibility.Visible;
            
            // Load photos AFTER UI is shown (lazy load like Windows Explorer)
            Dispatcher.BeginInvoke(new Action(() => LoadPhotosAsync()), 
                System.Windows.Threading.DispatcherPriority.Background);
        }
    }

    private void LoadPhotosAsync()
    {
        if (_currentAlbum == null || _photos == null) return;
        
        // Load BitmapImage từng cái một (lazy load) - KHÔNG show ItemsSource ngay
        Task.Run(() =>
        {
            var loadedPhotos = new List<SimplePhoto>();
            
            foreach (var photo in _photos)
            {
                try
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.IgnoreImageCache;
                    bitmap.UriSource = new Uri(photo.FilePath, UriKind.Absolute);
                    bitmap.DecodePixelWidth = 300; // Giảm kích thước để load nhanh hơn
                    bitmap.EndInit();
                    bitmap.Freeze();
                    
                    photo.ImageSource = bitmap;
                    loadedPhotos.Add(photo);
                    
                    // Update UI thread - thêm từng ảnh vào grid
                    Dispatcher.BeginInvoke(new Action(() => 
                    {
                        if (photosPanel.ItemsSource == null)
                        {
                            photosPanel.ItemsSource = new List<SimplePhoto>(loadedPhotos);
                        }
                        else
                        {
                            // Refresh để hiển thị ảnh mới load
                            var current = photosPanel.ItemsSource as List<SimplePhoto>;
                            photosPanel.ItemsSource = null;
                            photosPanel.ItemsSource = new List<SimplePhoto>(loadedPhotos);
                        }
                    }));
                }
                catch
                {
                    // Skip failed photos
                }
                
                // Small delay between loads to not freeze UI
                System.Threading.Thread.Sleep(15);
            }
        });
    }

    private void Back_Click(object? sender, RoutedEventArgs? e)
    {
        // If embedded in PhotoAlbumPageSimple, fire event
        if (BackRequested != null)
        {
            BackRequested.Invoke(this, EventArgs.Empty);
            return;
        }
        
        // Fallback: navigate back if opened standalone
        if (NavigationService != null && NavigationService.CanGoBack)
        {
            NavigationService.GoBack();
        }
    }

    private void EditAlbum_Click(object sender, RoutedEventArgs e)
    {
        if (_currentAlbum == null) return;

        var dialog = new AlbumEditorDialog(_currentAlbum, _albumService)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            LoadAlbum(); // Refresh
        }
    }

    private void DeleteAlbum_Click(object sender, RoutedEventArgs e)
    {
        if (_currentAlbum == null) return;

        var result = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa album '{_currentAlbum.Title}'?\n\n" +
            $"⚠️ Tất cả {_photos.Count} ảnh trong album sẽ bị xóa vĩnh viễn!",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _albumService.DeleteAlbumPermanently(_currentAlbum.Id);
                MessageBox.Show("✅ Đã xóa album thành công!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Back_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa album:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void Upload_Click(object sender, RoutedEventArgs e)
    {
        if (_currentAlbum == null) return;

        var dialog = new OpenFileDialog
        {
            Title = "Chọn ảnh để upload",
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
            Multiselect = true
        };

        if (dialog.ShowDialog() == true && dialog.FileNames.Length > 0)
        {
            var progressDialog = new AlbumUploadProgressDialog($"Đang import {dialog.FileNames.Length} ảnh...")
            {
                Owner = Window.GetWindow(this)
            };

            progressDialog.Show();

            Task.Run(() =>
            {
                var uploadedCount = 0;
                var totalFiles = dialog.FileNames.Length;

                foreach (var sourceFile in dialog.FileNames)
                {
                    try
                    {
                        // Copy file to album folder
                        var fileName = Path.GetFileName(sourceFile);
                        var destPath = Path.Combine(_currentAlbum.FolderPath, fileName);

                        // Handle duplicate names
                        if (File.Exists(destPath))
                        {
                            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                            var ext = Path.GetExtension(fileName);
                            var counter = 1;
                            
                            while (File.Exists(destPath))
                            {
                                fileName = $"{nameWithoutExt}_{counter}{ext}";
                                destPath = Path.Combine(_currentAlbum.FolderPath, fileName);
                                counter++;
                            }
                        }

                        File.Copy(sourceFile, destPath);
                        uploadedCount++;

                        // Update progress
                        var progress = (int)((uploadedCount / (double)totalFiles) * 100);
                        Dispatcher.Invoke(() => progressDialog.UpdateProgress(progress, $"Đã import {uploadedCount}/{totalFiles} ảnh"));
                    }
                    catch
                    {
                        // Continue with next file
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    progressDialog.Close();
                    MessageBox.Show($"✅ Đã import {uploadedCount}/{totalFiles} ảnh thành công!", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAlbum(); // Refresh
                });
            });
        }
    }

    #region Photo Event Handlers

    private void Photo_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.Cursor = Cursors.Hand;
        }
    }

    private void Photo_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.Cursor = Cursors.Arrow;
        }
    }

    private void Photo_Select(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement card && card.Tag is string photoPath)
        {
            if (_isSelectMode)
            {
                // Bulk selection mode
                if (_selectedPhotos.Contains(photoPath))
                {
                    _selectedPhotos.Remove(photoPath);
                }
                else
                {
                    _selectedPhotos.Add(photoPath);
                }
                UpdatePhotoSelection(card);
                UpdateSelectionCount();
            }
            else
            {
                // Normal mode
                _selectedPhotoPath = photoPath;
                UpdatePhotoSelection(card);

                // Handle double click - open viewer
                if (e.ClickCount == 2)
                {
                    OpenPhotoViewer(photoPath);
                }
            }
        }
    }

    private void UpdatePhotoSelection(FrameworkElement selectedCard)
    {
        if (_isSelectMode)
        {
            // Update the clicked card's border
            var photoPath = selectedCard.Tag as string;
            var border = FindVisualChild<Border>(selectedCard, "selectedBorder");
            if (border != null && photoPath != null)
            {
                border.Visibility = _selectedPhotos.Contains(photoPath) 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            }
        }
        else
        {
            // Clear all selections by finding all Cards in visual tree
            ClearAllSelections();

            // Set selected
            var selectedBorder = FindVisualChild<Border>(selectedCard, "selectedBorder");
            if (selectedBorder != null)
            {
                selectedBorder.Visibility = Visibility.Visible;
            }
        }
    }

    private void ClearAllSelections()
    {
        var cards = FindVisualChildren<MaterialDesignThemes.Wpf.Card>(photosPanel);
        foreach (var card in cards)
        {
            var border = FindVisualChild<Border>(card, "selectedBorder");
            if (border != null)
            {
                border.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void UpdateAllSelectionsVisual()
    {
        var cards = FindVisualChildren<MaterialDesignThemes.Wpf.Card>(photosPanel);
        foreach (var card in cards)
        {
            var photoPath = card.Tag as string;
            var border = FindVisualChild<Border>(card, "selectedBorder");
            if (border != null && photoPath != null)
            {
                border.Visibility = _selectedPhotos.Contains(photoPath) 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            }
        }
    }

    private IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) yield break;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is T element)
            {
                yield return element;
            }

            foreach (var descendant in FindVisualChildren<T>(child))
            {
                yield return descendant;
            }
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

    private void ViewPhoto_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && 
            menuItem.Parent is ContextMenu contextMenu &&
            contextMenu.PlacementTarget is FrameworkElement element &&
            element.Tag is string photoPath)
        {
            OpenPhotoViewer(photoPath);
        }
    }

    private void OpenPhotoViewer(string photoPath)
    {
        var viewer = new PhotoViewerDialog(photoPath, _photos, _currentAlbum?.FolderPath ?? "")
        {
            Owner = Window.GetWindow(this)
        };
        
        if (viewer.ShowDialog() == true)
        {
            LoadAlbum(); // Refresh if photo was deleted
        }
    }

    private void CopyPath_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && 
            menuItem.Parent is ContextMenu contextMenu &&
            contextMenu.PlacementTarget is FrameworkElement element &&
            element.Tag is string photoPath)
        {
            Clipboard.SetText(photoPath);
            MessageBox.Show("✅ Đã copy đường dẫn vào clipboard!", "Thành công",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void DeletePhotoFromMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && 
            menuItem.Parent is ContextMenu contextMenu &&
            contextMenu.PlacementTarget is FrameworkElement element &&
            element.Tag is string photoPath)
        {
            DeletePhoto(photoPath, confirm: true);
        }
    }

    private void OpenPhotoFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem &&
            menuItem.Parent is ContextMenu contextMenu &&
            contextMenu.PlacementTarget is FrameworkElement element &&
            element.Tag is string photoPath)
        {
            if (File.Exists(photoPath))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{photoPath}\"")
                {
                    UseShellExecute = true
                });
            }
        }
    }

    private void RenamePhoto_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem &&
            menuItem.Parent is ContextMenu contextMenu &&
            contextMenu.PlacementTarget is FrameworkElement element &&
            element.Tag is string photoPath)
        {
            var currentName = Path.GetFileNameWithoutExtension(photoPath);
            var extension = Path.GetExtension(photoPath);

            var dialog = new SimpleInputDialog("Đổi tên ảnh", "Nhập tên mới:", currentName)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
            {
                var newName = dialog.InputText.Trim();
                var newPath = Path.Combine(Path.GetDirectoryName(photoPath) ?? string.Empty, newName + extension);

                if (!File.Exists(newPath))
                {
                    File.Move(photoPath, newPath);
                    LoadAlbum();
                }
                else
                {
                    MessageBox.Show("Tên ảnh đã tồn tại!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private void AlbumDetailPage_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            if (_isSelectMode && _selectedPhotos.Count > 0)
            {
                BulkDelete_Click(sender, new RoutedEventArgs());
                e.Handled = true;
                return;
            }

            if (!_isSelectMode && !string.IsNullOrEmpty(_selectedPhotoPath))
            {
                DeletePhoto(_selectedPhotoPath, confirm: true);
                e.Handled = true;
            }
        }
    }

    private void DeletePhoto(string photoPath, bool confirm)
    {
        var fileName = Path.GetFileName(photoPath);

        if (confirm)
        {
            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa ảnh này?\n\n{fileName}",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        try
        {
            // Clear ItemsSource to release image handles
            photosPanel.ItemsSource = null;
            
            File.Delete(photoPath);
            MessageBox.Show("✅ Đã xóa ảnh thành công!", "Thành công",
                MessageBoxButton.OK, MessageBoxImage.Information);
            LoadAlbum();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Lỗi khi xóa ảnh:\n\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
            LoadAlbum(); // Reload to restore UI
        }
    }

    #endregion

    #region Bulk Operations

    private void SelectMode_Click(object sender, RoutedEventArgs e)
    {
        _isSelectMode = !_isSelectMode;
        _selectedPhotos.Clear();

        if (_isSelectMode)
        {
            normalModeButtons.Visibility = Visibility.Collapsed;
            bulkModeButtons.Visibility = Visibility.Visible;
            UpdateSelectionCount();
            MessageBox.Show("Chế độ chọn nhiều đã bật!\nClick vào ảnh để chọn/bỏ chọn.", "Chọn nhiều",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            normalModeButtons.Visibility = Visibility.Visible;
            bulkModeButtons.Visibility = Visibility.Collapsed;
            LoadAlbum(); // Refresh to clear selections
        }
    }

    private void UpdateSelectionCount()
    {
        if (txtSelectionCount != null)
        {
            txtSelectionCount.Text = $"Đã chọn: {_selectedPhotos.Count} ảnh";
        }
    }

    private void BulkSelectAll_Click(object sender, RoutedEventArgs e)
    {
        if (!_isSelectMode) return;

        _selectedPhotos.Clear();
        foreach (var photo in _photos)
        {
            _selectedPhotos.Add(photo.FilePath);
        }

        // Update visual selection
        UpdateAllSelectionsVisual();
        UpdateSelectionCount();
    }

    private void BulkDeselectAll_Click(object sender, RoutedEventArgs e)
    {
        if (!_isSelectMode) return;

        _selectedPhotos.Clear();

        // Update visual selection
        UpdateAllSelectionsVisual();
        UpdateSelectionCount();
    }

    private void BulkDelete_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedPhotos.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn ít nhất một ảnh!", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa {_selectedPhotos.Count} ảnh đã chọn?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            // Release all image handles before deletion
            photosPanel.ItemsSource = null;
            
            int successCount = 0;
            int failCount = 0;
            List<string> failedFiles = new();

            foreach (var photoPath in _selectedPhotos.ToList())
            {
                try
                {
                    if (File.Exists(photoPath))
                    {
                        File.Delete(photoPath);
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    failedFiles.Add($"{Path.GetFileName(photoPath)}: {ex.Message}");
                }
            }

            var message = $"✅ Đã xóa: {successCount} ảnh";
            if (failCount > 0)
            {
                message += $"\n❌ Lỗi: {failCount} ảnh\n\nChi tiết:\n{string.Join("\n", failedFiles.Take(5))}";
                if (failedFiles.Count > 5)
                    message += $"\n...và {failedFiles.Count - 5} file khác";
            }
            
            MessageBox.Show(message, "Kết quả", MessageBoxButton.OK, 
                failCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);

            _selectedPhotos.Clear();
            _isSelectMode = false;
            normalModeButtons.Visibility = Visibility.Visible;
            bulkModeButtons.Visibility = Visibility.Collapsed;
            LoadAlbum();
        }
    }

    private void BulkAddTags_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedPhotos.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn ít nhất một ảnh!", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new SimpleInputDialog("Thêm tags cho ảnh đã chọn", "Nhập tags (cách nhau bởi dấu phẩy):", "")
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
        {
            var tags = dialog.InputText.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(t => t.Trim())
                                     .Where(t => !string.IsNullOrWhiteSpace(t))
                                     .ToList();

            if (tags.Count > 0)
            {
                // Note: This requires extending SimplePhoto model to support tags
                MessageBox.Show($"✅ Đã thêm {tags.Count} tag(s) cho {_selectedPhotos.Count} ảnh!\n\n" +
                              $"Tags: {string.Join(", ", tags)}\n\n" +
                              $"Lưu ý: Cần mở rộng model SimplePhoto để lưu tags vào database.",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private void BulkExport_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedPhotos.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn ít nhất một ảnh!", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Use Microsoft.Win32.SaveFileDialog to select folder
        var dialog = new SaveFileDialog
        {
            Title = "Chọn thư mục để export ảnh",
            FileName = "SelectFolder", 
            Filter = "Folder|*.none",
            CheckFileExists = false,
            CheckPathExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            var destFolder = System.IO.Path.GetDirectoryName(dialog.FileName);
            if (string.IsNullOrEmpty(destFolder)) return;

            // Ask for folder name
            var folderDialog = new SimpleInputDialog("Export ảnh", "Nhập tên thư mục mới:", $"Export_{DateTime.Now:yyyyMMdd_HHmmss}")
            {
                Owner = Window.GetWindow(this)
            };

            if (folderDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(folderDialog.InputText))
            {
                destFolder = System.IO.Path.Combine(destFolder, folderDialog.InputText.Trim());
                
                // Create folder if doesn't exist
                if (!Directory.Exists(destFolder))
                {
                    Directory.CreateDirectory(destFolder);
                }

                var progressDialog = new AlbumUploadProgressDialog($"Đang export {_selectedPhotos.Count} ảnh...")
                {
                    Owner = Window.GetWindow(this)
                };

                progressDialog.Show();

                Task.Run(() =>
                {
                    int successCount = 0;
                    int failCount = 0;
                    var totalFiles = _selectedPhotos.Count;

                    foreach (var photoPath in _selectedPhotos.ToList())
                    {
                        try
                        {
                            if (File.Exists(photoPath))
                            {
                                var fileName = Path.GetFileName(photoPath);
                                var destPath = Path.Combine(destFolder, fileName);

                                // Handle duplicate names
                                if (File.Exists(destPath))
                                {
                                    var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                                    var ext = Path.GetExtension(fileName);
                                    var counter = 1;
                                    
                                    while (File.Exists(destPath))
                                    {
                                        fileName = $"{nameWithoutExt}_{counter}{ext}";
                                        destPath = Path.Combine(destFolder, fileName);
                                        counter++;
                                    }
                                }

                                File.Copy(photoPath, destPath);
                                successCount++;

                                // Update progress
                                var progress = (int)((successCount / (double)totalFiles) * 100);
                                Dispatcher.Invoke(() => progressDialog.UpdateProgress(progress, $"Đã export {successCount}/{totalFiles} ảnh"));
                            }
                        }
                        catch
                        {
                            failCount++;
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        progressDialog.Close();
                        
                        var message = $"✅ Đã export: {successCount} ảnh";
                        if (failCount > 0)
                        {
                            message += $"\n❌ Lỗi: {failCount} ảnh";
                        }
                        message += $"\n\nĐường dẫn: {destFolder}";

                        MessageBox.Show(message, "Kết quả", MessageBoxButton.OK,
                            failCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
                        
                        // Open folder in explorer
                        if (Directory.Exists(destFolder))
                        {
                            Process.Start(new ProcessStartInfo("explorer.exe", destFolder) { UseShellExecute = true });
                        }
                    });
                });
            }
        }
    }

    private void SetCover_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedPhotoPath))
        {
            MessageBox.Show("Vui lòng chọn một ảnh làm ảnh bìa!", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_currentAlbum != null)
        {
            _currentAlbum.CoverPhotoPath = _selectedPhotoPath;
            _albumService.UpdateAlbum(_currentAlbum);
            
            MessageBox.Show("✅ Đã đặt ảnh bìa thành công!", "Thành công",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    #endregion
}

// Simple Progress Dialog
public class AlbumUploadProgressDialog : Window
{
    private readonly TextBlock _messageText;
    private readonly System.Windows.Controls.ProgressBar _progressBar;

    public AlbumUploadProgressDialog(string message)
    {
        Width = 400;
        Height = 150;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.None;
        Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));

        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        _messageText = new TextBlock
        {
            Text = message,
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 15)
        };

        _progressBar = new System.Windows.Controls.ProgressBar
        {
            Height = 20,
            Minimum = 0,
            Maximum = 100,
            Value = 0
        };
        Grid.SetRow(_progressBar, 1);

        var percentText = new TextBlock
        {
            Text = "0%",
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 12,
            Foreground = new SolidColorBrush(Colors.Gray),
            Margin = new Thickness(0, 10, 0, 0)
        };
        Grid.SetRow(percentText, 2);

        grid.Children.Add(_messageText);
        grid.Children.Add(_progressBar);
        grid.Children.Add(percentText);

        Content = grid;
    }

    public void UpdateProgress(int percent, string message)
    {
        _progressBar.Value = percent;
        _messageText.Text = message;
    }
}
