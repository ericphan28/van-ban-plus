using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AIVanBan.Core.Models;

namespace AIVanBan.Desktop.Views;

public partial class PhotoViewerDialog : Window
{
    private readonly List<SimplePhoto> _photos;
    private readonly string _albumFolderPath;
    private int _currentIndex;

    public PhotoViewerDialog(string initialPhotoPath, List<SimplePhoto> photos, string albumFolderPath)
    {
        InitializeComponent();
        
        _photos = photos;
        _albumFolderPath = albumFolderPath;
        
        // Find initial photo index
        _currentIndex = _photos.FindIndex(p => p.FilePath == initialPhotoPath);
        if (_currentIndex < 0) _currentIndex = 0;

        ShowCurrentPhoto();
    }

    private void ShowCurrentPhoto()
    {
        if (_photos.Count == 0 || _currentIndex < 0 || _currentIndex >= _photos.Count)
            return;

        var photo = _photos[_currentIndex];

        try
        {
            // Load image
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(photo.FilePath);
            bitmap.EndInit();
            imgMain.Source = bitmap;

            // Update info
            txtFileName.Text = photo.FileName;
            txtPhotoIndex.Text = $"{_currentIndex + 1} / {_photos.Count}";

            // Get file info
            var fileInfo = new FileInfo(photo.FilePath);
            var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);
            txtPhotoInfo.Text = $"{bitmap.PixelWidth}x{bitmap.PixelHeight} • {fileSizeMB:F2} MB";

            // Update navigation buttons
            btnPrevious.IsEnabled = _currentIndex > 0;
            btnNext.IsEnabled = _currentIndex < _photos.Count - 1;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi load ảnh:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Previous_Click(object sender, RoutedEventArgs e)
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            ShowCurrentPhoto();
        }
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        if (_currentIndex < _photos.Count - 1)
        {
            _currentIndex++;
            ShowCurrentPhoto();
        }
    }

    private void DeletePhoto_Click(object sender, RoutedEventArgs e)
    {
        if (_currentIndex < 0 || _currentIndex >= _photos.Count)
            return;

        var photo = _photos[_currentIndex];
        var result = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa ảnh này?\n\n{photo.FileName}",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                File.Delete(photo.FilePath);
                _photos.RemoveAt(_currentIndex);

                if (_photos.Count == 0)
                {
                    MessageBox.Show("✅ Đã xóa ảnh. Album không còn ảnh nào.", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                    return;
                }

                // Show next photo or previous if this was the last
                if (_currentIndex >= _photos.Count)
                {
                    _currentIndex = _photos.Count - 1;
                }

                ShowCurrentPhoto();
                MessageBox.Show("✅ Đã xóa ảnh thành công!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa ảnh:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                if (btnPrevious.IsEnabled)
                    Previous_Click(sender, e);
                break;

            case Key.Right:
                if (btnNext.IsEnabled)
                    Next_Click(sender, e);
                break;

            case Key.Escape:
                Close_Click(sender, e);
                break;

            case Key.Delete:
                DeletePhoto_Click(sender, e);
                break;
        }
    }
}
