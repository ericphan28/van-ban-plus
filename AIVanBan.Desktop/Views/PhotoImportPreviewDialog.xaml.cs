using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AIVanBan.Desktop.Views;

public partial class PhotoImportPreviewDialog : Window
{
    private List<PhotoPreviewItem> _photos = new();
    
    public List<string> SelectedPhotoPaths { get; private set; } = new();

    public PhotoImportPreviewDialog(string[] filePaths)
    {
        InitializeComponent();
        
        LoadPhotos(filePaths);
    }

    private void LoadPhotos(string[] filePaths)
    {
        _photos.Clear();
        
        foreach (var filePath in filePaths)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                
                // Load thumbnail
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 300; // Thumbnail size for performance
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();

                var photo = new PhotoPreviewItem
                {
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    FileSize = fileInfo.Length,
                    ImageSource = bitmap,
                    IsSelected = true // Select all by default
                };

                // Try to get image dimensions
                try
                {
                    using var stream = File.OpenRead(filePath);
                    var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.None);
                    if (decoder.Frames.Count > 0)
                    {
                        photo.Width = decoder.Frames[0].PixelWidth;
                        photo.Height = decoder.Frames[0].PixelHeight;
                    }
                }
                catch
                {
                    // Ignore dimension read errors
                }

                _photos.Add(photo);
            }
            catch
            {
                // Skip files that can't be loaded
            }
        }

        photosPanel.ItemsSource = _photos;
        UpdateStats();
    }

    private void UpdateStats()
    {
        var selectedCount = _photos.Count(p => p.IsSelected);
        var totalCount = _photos.Count;
        var totalSize = _photos.Where(p => p.IsSelected).Sum(p => p.FileSize);
        var sizeMB = totalSize / (1024.0 * 1024.0);

        txtStats.Text = $"Đã chọn: {selectedCount} / {totalCount} ảnh ({sizeMB:F1} MB)";
        btnImport.IsEnabled = selectedCount > 0;
    }

    private void Photo_CheckChanged(object sender, RoutedEventArgs e)
    {
        UpdateStats();
    }

    private void SelectAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var photo in _photos)
        {
            photo.IsSelected = true;
        }
        UpdateStats();
    }

    private void DeselectAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var photo in _photos)
        {
            photo.IsSelected = false;
        }
        UpdateStats();
    }

    private void Import_Click(object sender, RoutedEventArgs e)
    {
        SelectedPhotoPaths = _photos
            .Where(p => p.IsSelected)
            .Select(p => p.FilePath)
            .ToList();

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

public class PhotoPreviewItem : INotifyPropertyChanged
{
    private bool _isSelected;

    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public BitmapImage? ImageSource { get; set; }

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

    public string DisplayInfo
    {
        get
        {
            var sizeMB = FileSize / (1024.0 * 1024.0);
            var size = sizeMB > 1 ? $"{sizeMB:F1} MB" : $"{FileSize / 1024.0:F0} KB";
            
            if (Width > 0 && Height > 0)
            {
                return $"{Width}×{Height} • {size}";
            }
            return size;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
