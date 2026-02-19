using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AIVanBan.Core.Services;
using MaterialDesignThemes.Wpf;

namespace AIVanBan.Desktop.Views;

public partial class AlbumDownloadDialog : Window
{
    private AlbumDownloadService _downloadService;
    private readonly SimpleAlbumService _albumService;
    
    private List<RemoteAlbum> _currentAlbums = new();
    private readonly HashSet<string> _selectedAlbumIds = new();
    private RemotePagination _pagination = new();
    private int _currentPage = 1;
    private string _currentSearch = "";
    private CancellationTokenSource? _downloadCts;
    private bool _isDownloading;
    private string _currentBaseUrl;

    public int DownloadedCount { get; private set; }

    public AlbumDownloadDialog(SimpleAlbumService albumService, string? baseUrl = null)
    {
        InitializeComponent();
        _albumService = albumService;
        _currentBaseUrl = baseUrl ?? "https://xagiakiem.gov.vn";
        _downloadService = new AlbumDownloadService(albumService, _currentBaseUrl);
        txtServerUrl.Text = _currentBaseUrl;
        
        Loaded += AlbumDownloadDialog_Loaded;
    }

    private async void AlbumDownloadDialog_Loaded(object sender, RoutedEventArgs e)
    {
        await TestConnectionAndLoad();
    }

    private async Task TestConnectionAndLoad()
    {
        try
        {
            txtLoadingMessage.Text = "Đang kết nối đến xagiakiem.gov.vn...";
            loadingOverlay.Visibility = Visibility.Visible;

            var (isConnected, totalAlbums, error) = await _downloadService.TestConnectionAsync();
            
            if (isConnected)
            {
                iconConnection.Kind = PackIconKind.CloudCheck;
                iconConnection.Foreground = new SolidColorBrush(Color.FromRgb(0x81, 0xC7, 0x84));
                txtConnectionStatus.Text = $"Đã kết nối · {totalAlbums} album";
                
                await LoadAlbums();
            }
            else
            {
                iconConnection.Kind = PackIconKind.CloudOff;
                iconConnection.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x53, 0x50));
                txtConnectionStatus.Text = "Không kết nối được";
                
                loadingOverlay.Visibility = Visibility.Collapsed;
                emptyState.Visibility = Visibility.Visible;
                txtEmptyHint.Text = error ?? "Kiểm tra kết nối internet";
            }
        }
        catch (Exception ex)
        {
            iconConnection.Kind = PackIconKind.CloudOff;
            iconConnection.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x53, 0x50));
            txtConnectionStatus.Text = "Lỗi kết nối";
            
            loadingOverlay.Visibility = Visibility.Collapsed;
            emptyState.Visibility = Visibility.Visible;
            txtEmptyHint.Text = ex.Message;
        }
    }

    private async Task LoadAlbums()
    {
        try
        {
            loadingOverlay.Visibility = Visibility.Visible;
            txtLoadingMessage.Text = "Đang tải danh sách album...";
            emptyState.Visibility = Visibility.Collapsed;

            var result = await _downloadService.GetRemoteAlbumsAsync(
                page: _currentPage,
                limit: 12,
                search: string.IsNullOrWhiteSpace(_currentSearch) ? null : _currentSearch);

            _currentAlbums = result.Items;
            _pagination = result.Pagination;

            RenderAlbumList();
            UpdatePagination();
            UpdateSelectionInfo();

            loadingOverlay.Visibility = Visibility.Collapsed;

            if (_currentAlbums.Count == 0)
            {
                emptyState.Visibility = Visibility.Visible;
                txtEmptyHint.Text = string.IsNullOrWhiteSpace(_currentSearch)
                    ? "Chưa có album công khai nào trên website"
                    : $"Không tìm thấy album với từ khóa \"{_currentSearch}\"";
            }
        }
        catch (Exception ex)
        {
            loadingOverlay.Visibility = Visibility.Collapsed;
            emptyState.Visibility = Visibility.Visible;
            txtEmptyHint.Text = $"Lỗi: {ex.Message}";
        }
    }

    private void RenderAlbumList()
    {
        albumListPanel.Children.Clear();

        foreach (var album in _currentAlbums)
        {
            var isSelected = _selectedAlbumIds.Contains(album.Id);

            // Card border
            var card = new Border
            {
                Margin = new Thickness(0, 3, 0, 3),
                Padding = new Thickness(12, 10, 12, 10),
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(isSelected ? 2 : 1),
                BorderBrush = new SolidColorBrush(isSelected 
                    ? Color.FromRgb(0x15, 0x65, 0xC0) 
                    : Color.FromRgb(0xE0, 0xE0, 0xE0)),
                Background = new SolidColorBrush(isSelected 
                    ? Color.FromRgb(0xE3, 0xF2, 0xFD) 
                    : Colors.White),
                Cursor = Cursors.Hand
            };

            var mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Checkbox
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(72) }); // Thumbnail
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Info

            // Checkbox
            var checkbox = new CheckBox
            {
                IsChecked = isSelected,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            var capturedAlbumId = album.Id;
            checkbox.Checked += (s, ev) => ToggleAlbumSelection(capturedAlbumId, true);
            checkbox.Unchecked += (s, ev) => ToggleAlbumSelection(capturedAlbumId, false);
            Grid.SetColumn(checkbox, 0);

            // Thumbnail
            var thumbBorder = new Border
            {
                Width = 60, Height = 60,
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
                ClipToBounds = true,
                Margin = new Thickness(0, 0, 12, 0)
            };

            if (!string.IsNullOrEmpty(album.CoverUrl))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(album.CoverUrl);
                    bitmap.DecodePixelWidth = 100;
                    bitmap.CacheOption = BitmapCacheOption.OnDemand;
                    bitmap.EndInit();

                    thumbBorder.Background = new ImageBrush(bitmap)
                    {
                        Stretch = Stretch.UniformToFill
                    };
                }
                catch
                {
                    thumbBorder.Child = new PackIcon
                    {
                        Kind = PackIconKind.ImageOutline,
                        Width = 24, Height = 24,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E))
                    };
                }
            }
            else
            {
                thumbBorder.Child = new PackIcon
                {
                    Kind = PackIconKind.ImageMultiple,
                    Width = 24, Height = 24,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E))
                };
            }
            Grid.SetColumn(thumbBorder, 1);

            // Info panel
            var infoPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            
            infoPanel.Children.Add(new TextBlock
            {
                Text = album.Title,
                FontSize = 13.5,
                FontWeight = FontWeights.SemiBold,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 0, 0, 3)
            });

            // Meta info row
            var metaPanel = new WrapPanel();
            metaPanel.Children.Add(new TextBlock
            {
                Text = $"📷 {album.ImageCount} ảnh",
                FontSize = 11.5,
                Foreground = new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61)),
                Margin = new Thickness(0, 0, 14, 0)
            });

            if (!string.IsNullOrEmpty(album.OrganizationName))
            {
                metaPanel.Children.Add(new TextBlock
                {
                    Text = $"🏛️ {album.OrganizationName}",
                    FontSize = 11.5,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61)),
                    Margin = new Thickness(0, 0, 14, 0),
                    MaxWidth = 200,
                    TextTrimming = TextTrimming.CharacterEllipsis
                });
            }

            if (!string.IsNullOrEmpty(album.EventTitle))
            {
                metaPanel.Children.Add(new TextBlock
                {
                    Text = $"📅 {album.EventTitle}",
                    FontSize = 11.5,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0)),
                    Margin = new Thickness(0, 0, 14, 0),
                    MaxWidth = 250,
                    TextTrimming = TextTrimming.CharacterEllipsis
                });
            }

            // Event date
            if (!string.IsNullOrEmpty(album.EventDate) && DateTime.TryParse(album.EventDate, out var evtDate))
            {
                metaPanel.Children.Add(new TextBlock
                {
                    Text = $"🗓️ {evtDate:dd/MM/yyyy}",
                    FontSize = 11.5,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0)),
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 14, 0)
                });
            }

            if (!string.IsNullOrEmpty(album.CreatedAt) && DateTime.TryParse(album.CreatedAt, out var dt))
            {
                metaPanel.Children.Add(new TextBlock
                {
                    Text = dt.ToString("dd/MM/yyyy"),
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E))
                });
            }

            infoPanel.Children.Add(metaPanel);

            if (!string.IsNullOrEmpty(album.Description))
            {
                var desc = album.Description.Length > 100 ? album.Description[..100] + "..." : album.Description;
                infoPanel.Children.Add(new TextBlock
                {
                    Text = desc,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(0, 3, 0, 0),
                    TextTrimming = TextTrimming.CharacterEllipsis
                });
            }

            Grid.SetColumn(infoPanel, 2);

            mainGrid.Children.Add(checkbox);
            mainGrid.Children.Add(thumbBorder);
            mainGrid.Children.Add(infoPanel);
            card.Child = mainGrid;

            // Click anywhere on card to toggle selection
            card.MouseLeftButtonUp += (s, ev) =>
            {
                checkbox.IsChecked = !checkbox.IsChecked;
            };

            albumListPanel.Children.Add(card);
        }
    }

    private void ToggleAlbumSelection(string albumId, bool selected)
    {
        if (selected)
        {
            if (_selectedAlbumIds.Count >= AlbumDownloadService.MaxAlbumsPerDownload)
            {
                MessageBox.Show(
                    $"Tối đa {AlbumDownloadService.MaxAlbumsPerDownload} album mỗi lần tải!\n" +
                    $"Vui lòng bỏ chọn bớt album trước khi chọn thêm.",
                    "Đã đạt giới hạn", MessageBoxButton.OK, MessageBoxImage.Warning);
                
                // Re-render to uncheck the checkbox
                RenderAlbumList();
                return;
            }
            _selectedAlbumIds.Add(albumId);
        }
        else
        {
            _selectedAlbumIds.Remove(albumId);
        }

        UpdateSelectionInfo();
        RenderAlbumList(); // Re-render to update card visual
    }

    private void UpdateSelectionInfo()
    {
        var count = _selectedAlbumIds.Count;
        txtSelectionInfo.Text = count == 0 
            ? "Chưa chọn album nào" 
            : $"Đã chọn {count} album";
        
        btnDownload.IsEnabled = count > 0 && !_isDownloading;
        txtDownloadButton.Text = count > 0 
            ? $"Tải {count} album đã chọn" 
            : "Tải album đã chọn";
    }

    private void UpdatePagination()
    {
        txtTotalAlbums.Text = $"📋 {_pagination.Total} album công khai";
        txtPageInfo.Text = $"Trang {_pagination.Page}/{Math.Max(1, _pagination.TotalPages)}";
        txtPaginationInfo.Text = $"Hiển thị {_currentAlbums.Count} / {_pagination.Total} album";
        
        btnPrevPage.IsEnabled = _pagination.HasPrev;
        btnNextPage.IsEnabled = _pagination.HasNext;
    }

    #region Event handlers

    private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchAlbums_Click(sender, e);
        }
    }

    private async void SearchAlbums_Click(object sender, RoutedEventArgs e)
    {
        _currentSearch = txtSearch.Text?.Trim() ?? "";
        _currentPage = 1;
        await LoadAlbums();
    }

    private async void PrevPage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            await LoadAlbums();
        }
    }

    private async void NextPage_Click(object sender, RoutedEventArgs e)
    {
        if (_pagination.HasNext)
        {
            _currentPage++;
            await LoadAlbums();
        }
    }

    private void SelectAll_Click(object sender, RoutedEventArgs e)
    {
        var remaining = AlbumDownloadService.MaxAlbumsPerDownload - _selectedAlbumIds.Count;
        var toAdd = _currentAlbums.Where(a => !_selectedAlbumIds.Contains(a.Id)).Take(remaining);
        
        foreach (var album in toAdd)
        {
            _selectedAlbumIds.Add(album.Id);
        }

        if (_selectedAlbumIds.Count >= AlbumDownloadService.MaxAlbumsPerDownload)
        {
            MessageBox.Show(
                $"Đã chọn đủ {AlbumDownloadService.MaxAlbumsPerDownload} album (tối đa cho phép).",
                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        UpdateSelectionInfo();
        RenderAlbumList();
    }

    private void DeselectAll_Click(object sender, RoutedEventArgs e)
    {
        _selectedAlbumIds.Clear();
        UpdateSelectionInfo();
        RenderAlbumList();
    }

    private async void Download_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedAlbumIds.Count == 0) return;
        
        if (_selectedAlbumIds.Count > AlbumDownloadService.MaxAlbumsPerDownload)
        {
            MessageBox.Show(
                $"Tối đa {AlbumDownloadService.MaxAlbumsPerDownload} album mỗi lần tải!",
                "Quá giới hạn", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var totalImages = _currentAlbums
            .Where(a => _selectedAlbumIds.Contains(a.Id))
            .Sum(a => a.ImageCount);

        var confirmResult = MessageBox.Show(
            $"Tải {_selectedAlbumIds.Count} album ({totalImages} ảnh) về máy?\n\n" +
            $"Album sẽ được lưu vào: Documents\\AIVanBan\\Photos\\",
            "Xác nhận tải album",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmResult != MessageBoxResult.Yes) return;

        await StartDownload();
    }

    private async Task StartDownload()
    {
        _isDownloading = true;
        _downloadCts = new CancellationTokenSource();

        // Disable UI during download
        btnDownload.IsEnabled = false;
        btnSelectAll.IsEnabled = false;
        btnDeselectAll.IsEnabled = false;
        btnPrevPage.IsEnabled = false;
        btnNextPage.IsEnabled = false;
        btnCancel.Content = "Hủy tải";

        progressPanel.Visibility = Visibility.Visible;
        downloadProgressBar.Value = 0;

        var albumIds = _selectedAlbumIds.ToList();
        var completedAlbums = 0;
        var totalAlbums = albumIds.Count;
        DownloadedCount = 0;

        var progress = new Progress<AlbumDownloadProgress>(p =>
        {
            Dispatcher.Invoke(() =>
            {
                txtProgressTitle.Text = $"[{completedAlbums + 1}/{totalAlbums}] {p.AlbumTitle}";
                txtProgressPercent.Text = $"{p.ProgressPercent:F0}%";
                downloadProgressBar.Value = p.ProgressPercent;
                txtProgressDetail.Text = p.TotalImages > 0
                    ? $"Ảnh: {p.DownloadedImages}/{p.TotalImages} · Đang tải: {p.CurrentImageName}"
                    : "Đang lấy thông tin album...";

                if (p.IsCompleted)
                {
                    completedAlbums++;
                    if (p.ErrorMessage == null && !p.IsCancelled)
                        DownloadedCount++;
                }
            });
        });

        try
        {
            var results = await _downloadService.DownloadMultipleAlbumsAsync(
                albumIds, progress, _downloadCts.Token);

            var succeeded = results.Count(r => r.LocalAlbum != null);
            var failed = results.Count(r => r.Error != null);

            // Overall progress
            downloadProgressBar.Value = 100;
            txtProgressPercent.Text = "100%";
            txtProgressTitle.Text = "Hoàn tất!";
            txtProgressDetail.Text = $"✅ Thành công: {succeeded} album · ❌ Lỗi: {failed} album";

            if (succeeded > 0)
            {
                MessageBox.Show(
                    $"Đã tải thành công {succeeded}/{totalAlbums} album!\n\n" +
                    (failed > 0 ? $"⚠️ {failed} album bị lỗi.\n\n" : "") +
                    $"Album đã được lưu vào:\nDocuments\\AIVanBan\\Photos\\",
                    "Tải album hoàn tất",
                    MessageBoxButton.OK,
                    succeeded == totalAlbums ? MessageBoxImage.Information : MessageBoxImage.Warning);

                _selectedAlbumIds.Clear();
                UpdateSelectionInfo();
                RenderAlbumList();
            }
            else
            {
                MessageBox.Show(
                    "Không tải được album nào!\nVui lòng kiểm tra kết nối và thử lại.",
                    "Lỗi tải album", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (OperationCanceledException)
        {
            txtProgressTitle.Text = "Đã hủy tải";
            txtProgressDetail.Text = $"Đã tải được {DownloadedCount} album trước khi hủy.";
            
            MessageBox.Show(
                $"Đã hủy tải album.\nĐã tải được {DownloadedCount} album.",
                "Đã hủy", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải album:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isDownloading = false;
            btnDownload.IsEnabled = _selectedAlbumIds.Count > 0;
            btnSelectAll.IsEnabled = true;
            btnDeselectAll.IsEnabled = true;
            btnCancel.Content = "Đóng";
            UpdatePagination(); // Re-enable pagination buttons

            _downloadCts?.Dispose();
            _downloadCts = null;
        }
    }

    private void ToggleUrlConfig_Click(object sender, RoutedEventArgs e)
    {
        urlConfigPanel.Visibility = urlConfigPanel.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private async void ReconnectServer_Click(object sender, RoutedEventArgs e)
    {
        var newUrl = txtServerUrl.Text?.Trim();
        if (string.IsNullOrWhiteSpace(newUrl))
        {
            MessageBox.Show("Vui lòng nhập URL server!", "Thiếu URL", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Dispose old service, create new one with new URL
        _downloadService.Dispose();
        _currentBaseUrl = newUrl;
        _downloadService = new AlbumDownloadService(_albumService, _currentBaseUrl);
        
        // Reset state
        _selectedAlbumIds.Clear();
        _currentPage = 1;
        _currentSearch = "";
        txtSearch.Text = "";

        await TestConnectionAndLoad();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        if (_isDownloading)
        {
            var result = MessageBox.Show(
                "Đang tải album. Bạn muốn hủy?",
                "Xác nhận hủy", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _downloadCts?.Cancel();
            }
            return;
        }

        DialogResult = DownloadedCount > 0;
        Close();
    }

    #endregion

    protected override void OnClosed(EventArgs e)
    {
        _downloadCts?.Cancel();
        _downloadCts?.Dispose();
        _downloadService.Dispose();
        base.OnClosed(e);
    }
}
