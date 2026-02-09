using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class PhotoAlbumPage : Page
{
    private readonly DocumentService _documentService;
    private string _currentAlbumId = string.Empty;
    private List<Photo> _selectedPhotos = new();

    public PhotoAlbumPage(DocumentService documentService)
    {
        InitializeComponent();
        _documentService = documentService;
        LoadAlbums();
    }

    private void LoadAlbums()
    {
        try
        {
            var currentIndex = cboAlbum.SelectedIndex;
            
            cboAlbum.Items.Clear();
            cboAlbum.Items.Add("Tất cả ảnh");
        
            var albums = _documentService.GetAllAlbums();
            foreach (var album in albums)
            {
                cboAlbum.Items.Add($"{album.Name} ({album.PhotoCount})");
            }
        
            if (albums.Count == 0)
            {
                // Create default album
                var defaultAlbum = new Album 
                { 
                    Name = "Album chung",
                    Description = "Album mặc định",
                    CreatedDate = DateTime.Now
                };
                _documentService.AddAlbum(defaultAlbum);
                LoadAlbums();
                return;
            }
        
            // Restore previous selection nếu còn hợp lệ
            if (currentIndex >= 0 && currentIndex < cboAlbum.Items.Count)
            {
                cboAlbum.SelectedIndex = currentIndex;
            }
            else
            {
                cboAlbum.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            ErrorDialog.Show("Lỗi load albums", ex.Message, ex.StackTrace);
        }
    }

    private void Album_Changed(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            LoadPhotos();
        }
        catch (Exception ex)
        {
            ErrorDialog.Show("Lỗi chuyển album", ex.Message, ex.StackTrace);
        }
    }

    private void LoadPhotos()
    {
        try
        {
            photoPanel.Children.Clear();
            _selectedPhotos.Clear();

            List<Photo> photos;
            if (cboAlbum.SelectedIndex <= 0)
            {
                photos = _documentService.GetAllPhotos();
            }
            else
            {
                var albums = _documentService.GetAllAlbums();
                var albumIndex = cboAlbum.SelectedIndex - 1;
                
                if (albumIndex >= 0 && albumIndex < albums.Count)
                {
                    var album = albums[albumIndex];
                    _currentAlbumId = album.Id;
                    photos = _documentService.GetPhotosByAlbum(album.Id);
                }
                else
                {
                    // Index không hợp lệ, về "Tất cả ảnh"
                    cboAlbum.SelectedIndex = 0;
                    photos = _documentService.GetAllPhotos();
                }
            }

            if (photos.Count == 0)
            {
                emptyState.Visibility = Visibility.Visible;
                return;
            }

            emptyState.Visibility = Visibility.Collapsed;

            foreach (var photo in photos)
            {
                var card = CreatePhotoCard(photo);
                photoPanel.Children.Add(card);
            }
        }
        catch (Exception ex)
        {
            ErrorDialog.Show("Lỗi load photos", ex.Message, ex.StackTrace);
        }
    }

    private Border CreatePhotoCard(Photo photo)
    {
        var border = new Border
        {
            Width = 180,
            Height = 220,
            Margin = new Thickness(10),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Background = Brushes.White,
            Cursor = System.Windows.Input.Cursors.Hand
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(140) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Image
        var image = new Image
        {
            Stretch = Stretch.UniformToFill,
            Margin = new Thickness(5, 5, 5, 0)
        };

        if (File.Exists(photo.FilePath))
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 180;
                using (var stream = new System.IO.FileStream(photo.FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze(); // Important: freeze to release file handle
                }
                image.Source = bitmap;
            }
            catch
            {
                image.Source = null;
            }
        }

        Grid.SetRow(image, 0);

        // Title
        var title = new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(photo.Event) ? Path.GetFileName(photo.FilePath) : photo.Event,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(5),
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextAlignment = TextAlignment.Center
        };
        Grid.SetRow(title, 1);

        // Info
        var info = new TextBlock
        {
            Text = $"{photo.DateTaken:dd/MM/yyyy} • {FormatFileSize(photo.FileSize)}",
            FontSize = 11,
            Foreground = Brushes.Gray,
            Margin = new Thickness(5, 0, 5, 5),
            TextAlignment = TextAlignment.Center
        };
        Grid.SetRow(info, 2);

        grid.Children.Add(image);
        grid.Children.Add(title);
        grid.Children.Add(info);

        border.Child = grid;
        border.MouseLeftButtonDown += (s, e) =>
        {
            try
            {
                var viewer = new PhotoViewerDialog(photo);
                viewer.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xem ảnh: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };

        // Context menu
        var contextMenu = new ContextMenu();
        
        var viewItem = new MenuItem { Header = "🔍 Xem" };
        viewItem.Click += (s, e) =>
        {
            try
            {
                var viewer = new PhotoViewerDialog(photo);
                viewer.ShowDialog();
            }
            catch (Exception ex)
            {
                ErrorDialog.Show("Lỗi xem ảnh", ex.Message, ex.StackTrace);
            }
        };
        
        var editItem = new MenuItem { Header = "✏️ Sửa thông tin" };
        editItem.Click += (s, e) =>
        {
            try
            {
                var dialog = new PhotoEditDialog(photo, _documentService);
                if (dialog.ShowDialog() == true)
                {
                    LoadPhotos();
                }
            }
            catch (Exception ex)
            {
                ErrorDialog.Show("Lỗi sửa ảnh", ex.Message, ex.StackTrace);
            }
        };
        
        var deleteItem = new MenuItem { Header = "🗑️ Xóa" };
        deleteItem.Click += (s, e) =>
        {
            try
            {
                var result = MessageBox.Show($"Xóa ảnh '{photo.Event}'?", "Xác nhận",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _documentService.DeletePhoto(photo.Id);
                    LoadPhotos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa ảnh: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };

        contextMenu.Items.Add(viewItem);
        contextMenu.Items.Add(editItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(deleteItem);
        border.ContextMenu = contextMenu;

        return border;
    }

    private string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024} KB";
        return $"{bytes / (1024 * 1024)} MB";
    }

    private void UploadPhotos_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All files (*.*)|*.*",
                Multiselect = true
            };

            if (openDialog.ShowDialog() == true)
            {
            var photoDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "AIVanBan",
                "Photos"
            );
            Directory.CreateDirectory(photoDir);

            int count = 0;
            foreach (var file in openDialog.FileNames)
            {
                try
                {
                    var fileName = Path.GetFileName(file);
                    var newPath = Path.Combine(photoDir, $"{Guid.NewGuid()}_{fileName}");
                    File.Copy(file, newPath, true);

                    var photo = new Photo
                    {
                        FileName = fileName,
                        Event = Path.GetFileNameWithoutExtension(fileName),
                        FilePath = newPath,
                        FileSize = new FileInfo(file).Length,
                        DateTaken = DateTime.Now,
                        AlbumId = _currentAlbumId
                    };

                    _documentService.AddPhoto(photo);
                    count++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tải ảnh {Path.GetFileName(file)}: {ex.Message}", 
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (count > 0)
            {
                LoadPhotos(); // Refresh grid
                MessageBox.Show($"Đã tải lên {count} ảnh!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            }
        }
        catch (Exception ex)
        {
            ErrorDialog.Show("Lỗi upload ảnh", ex.Message, ex.StackTrace);
        }
    }

    private void AddAlbum_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new AlbumEditDialog(null, _documentService);
            if (dialog.ShowDialog() == true)
            {
                LoadAlbums();
            }
        }
        catch (Exception ex)
        {
            ErrorDialog.Show("Lỗi tạo album", ex.Message, ex.StackTrace);
        }
    }

    private void DeleteSelected_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            MessageBox.Show("Chức năng chọn nhiều ảnh đang phát triển!", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            LoadAlbums();
            LoadPhotos();
        }
        catch (Exception ex)
        {
            ErrorDialog.Show("Lỗi refresh", ex.Message, ex.StackTrace);
        }
    }
}
