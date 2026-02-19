using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using MaterialDesignThemes.Wpf;

namespace AIVanBan.Desktop.Views
{
    public partial class AlbumUploadDialog : Window
    {
        private readonly List<SimpleAlbum> _localAlbums;
        private AlbumUploadService? _uploadService;
        private readonly HashSet<string> _selectedAlbumIds = new();
        private List<UploadOrganization> _organizations = new();
        private CancellationTokenSource? _cts;
        private bool _isUploading;

        private const int MAX_ALBUMS = 5;
        private const string DEFAULT_URL = "";

        public int UploadedCount { get; private set; }

        public AlbumUploadDialog(List<SimpleAlbum> albums)
        {
            InitializeComponent();
            _localAlbums = albums.Where(a =>
                !string.IsNullOrEmpty(a.FolderPath) &&
                Directory.Exists(a.FolderPath)
            ).ToList();

            // Set URL mặc định
            txtServerUrl.Text = DEFAULT_URL;
        }

        #region Login

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var serverUrl = txtServerUrl.Text?.Trim();
            var email = txtEmail.Text?.Trim();
            var password = txtPassword.Password;

            if (string.IsNullOrEmpty(serverUrl))
            {
                ShowLoginError("Vui lòng nhập URL website (ví dụ: https://xagiakiem.gov.vn hoặc http://localhost:3010).");
                txtServerUrl.Focus();
                return;
            }

            if (!serverUrl.StartsWith("http://") && !serverUrl.StartsWith("https://"))
            {
                ShowLoginError("URL phải bắt đầu bằng http:// hoặc https://");
                txtServerUrl.Focus();
                return;
            }

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowLoginError("Vui lòng nhập email và mật khẩu.");
                return;
            }

            btnLogin.IsEnabled = false;
            btnLogin.Content = "Đang đăng nhập...";
            HideLoginError();

            try
            {
                _uploadService = new AlbumUploadService(serverUrl);

                var result = await _uploadService.LoginAsync(email, password);

                if (result.Success && result.User != null)
                {
                    // Đăng nhập thành công
                    txtUserInfo.Text = $"{result.User.FullName} ({result.User.RoleDisplayName}) — {result.User.Email}";

                    // Ẩn login panel, hiện upload panel
                    loginPanel.Visibility = Visibility.Collapsed;
                    userInfoPanel.Visibility = Visibility.Visible;
                    albumListPanel.Visibility = Visibility.Visible;
                    selectionInfoPanel.Visibility = Visibility.Visible;

                    // Load organizations + albums
                    await LoadOrganizations();
                    LoadLocalAlbums();
                }
                else
                {
                    ShowLoginError(result.Error ?? "Đăng nhập thất bại.");
                }
            }
            catch (Exception ex)
            {
                ShowLoginError($"Lỗi: {ex.Message}");
            }
            finally
            {
                btnLogin.IsEnabled = true;
                btnLogin.Content = "Đăng nhập";
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _uploadService?.Logout();
            _uploadService?.Dispose();
            _uploadService = null;
            _selectedAlbumIds.Clear();
            _organizations.Clear();

            // Reset UI
            loginPanel.Visibility = Visibility.Visible;
            userInfoPanel.Visibility = Visibility.Collapsed;
            albumListPanel.Visibility = Visibility.Collapsed;
            selectionInfoPanel.Visibility = Visibility.Collapsed;
            progressPanel.Visibility = Visibility.Collapsed;
            btnUpload.IsEnabled = false;

            albumListContainer.Children.Clear();
            cboOrganization.ItemsSource = null;
        }

        private void ShowLoginError(string message)
        {
            txtLoginError.Text = message;
            txtLoginError.Visibility = Visibility.Visible;
        }

        private void HideLoginError()
        {
            txtLoginError.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Organizations

        private async Task LoadOrganizations()
        {
            try
            {
                if (_uploadService == null) return;

                _organizations = await _uploadService.GetOrganizationsAsync();

                cboOrganization.ItemsSource = _organizations;
                cboOrganization.DisplayMemberPath = "Name";

                // Auto-select primary org nếu có
                if (_uploadService.CurrentUser?.OrganizationId != null)
                {
                    var primaryOrg = _organizations.FirstOrDefault(
                        o => o.Id == _uploadService.CurrentUser.OrganizationId);
                    if (primaryOrg != null)
                    {
                        cboOrganization.SelectedItem = primaryOrg;
                    }
                }
                else if (_organizations.Count == 1)
                {
                    cboOrganization.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading orgs: {ex.Message}");
            }
        }

        private void Organization_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUploadButtonState();
        }

        #endregion

        #region Album List

        private void LoadLocalAlbums()
        {
            albumListContainer.Children.Clear();

            if (_localAlbums.Count == 0)
            {
                txtNoAlbums.Visibility = Visibility.Visible;
                UpdateSelectionInfo();
                return;
            }

            txtNoAlbums.Visibility = Visibility.Collapsed;

            foreach (var album in _localAlbums)
            {
                var card = CreateAlbumCard(album);
                albumListContainer.Children.Add(card);
            }

            UpdateSelectionInfo();
        }

        private Border CreateAlbumCard(SimpleAlbum album)
        {
            // Đếm ảnh trong folder
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            int photoCount = 0;
            try
            {
                photoCount = Directory.GetFiles(album.FolderPath)
                    .Count(f => imageExtensions.Contains(Path.GetExtension(f).ToLower()));
            }
            catch { }

            var isSelected = _selectedAlbumIds.Contains(album.Id);

            // Main container
            var border = new Border
            {
                Background = isSelected ? new SolidColorBrush(Color.FromRgb(227, 242, 253)) : Brushes.White,
                BorderBrush = isSelected ? new SolidColorBrush(Color.FromRgb(21, 101, 192)) : new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                BorderThickness = new Thickness(isSelected ? 2 : 1),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 0, 0, 6),
                Padding = new Thickness(12, 10, 12, 10),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = album.Id
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) }); // Checkbox
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) }); // Thumbnail
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Info

            // Checkbox
            var checkbox = new CheckBox
            {
                IsChecked = isSelected,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = album.Id
            };
            checkbox.Checked += AlbumCheckbox_Changed;
            checkbox.Unchecked += AlbumCheckbox_Changed;
            Grid.SetColumn(checkbox, 0);
            grid.Children.Add(checkbox);

            // Thumbnail
            var thumbBorder = new Border
            {
                Width = 40, Height = 40,
                CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(Color.FromRgb(238, 238, 238)),
                ClipToBounds = true,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (!string.IsNullOrEmpty(album.CoverPhotoPath) && File.Exists(album.CoverPhotoPath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(album.CoverPhotoPath);
                    bitmap.DecodePixelWidth = 80;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    thumbBorder.Child = new Image
                    {
                        Source = bitmap,
                        Stretch = Stretch.UniformToFill
                    };
                }
                catch
                {
                    thumbBorder.Child = new PackIcon
                    {
                        Kind = PackIconKind.Image,
                        Width = 20, Height = 20,
                        Foreground = Brushes.Gray,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                }
            }
            else
            {
                thumbBorder.Child = new PackIcon
                {
                    Kind = PackIconKind.Image,
                    Width = 20, Height = 20,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }
            Grid.SetColumn(thumbBorder, 1);
            grid.Children.Add(thumbBorder);

            // Info panel
            var infoPanel = new StackPanel { Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };

            var titleText = new TextBlock
            {
                Text = album.Title,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            infoPanel.Children.Add(titleText);

            // Meta info
            var metaPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 3, 0, 0) };

            metaPanel.Children.Add(new TextBlock
            {
                Text = $"📷 {photoCount} ảnh",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            });

            if (album.EventDate.HasValue)
            {
                metaPanel.Children.Add(new TextBlock
                {
                    Text = $"  🗓️ {album.EventDate.Value:dd/MM/yyyy}",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(21, 101, 192))
                });
            }

            if (album.CreatedDate != default)
            {
                metaPanel.Children.Add(new TextBlock
                {
                    Text = $"  📅 {album.CreatedDate:dd/MM/yyyy}",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(158, 158, 158))
                });
            }

            infoPanel.Children.Add(metaPanel);

            Grid.SetColumn(infoPanel, 2);
            grid.Children.Add(infoPanel);

            border.Child = grid;

            // Click to toggle
            border.MouseLeftButtonDown += (s, e) =>
            {
                checkbox.IsChecked = !checkbox.IsChecked;
            };

            return border;
        }

        private void AlbumCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox cb || cb.Tag is not string albumId) return;

            if (cb.IsChecked == true)
            {
                if (_selectedAlbumIds.Count >= MAX_ALBUMS)
                {
                    cb.IsChecked = false;
                    return;
                }
                _selectedAlbumIds.Add(albumId);
            }
            else
            {
                _selectedAlbumIds.Remove(albumId);
            }

            // Update card visual
            UpdateCardVisual(albumId, cb.IsChecked == true);
            UpdateSelectionInfo();
            UpdateUploadButtonState();
        }

        private void UpdateCardVisual(string albumId, bool selected)
        {
            foreach (Border border in albumListContainer.Children)
            {
                if (border.Tag?.ToString() == albumId)
                {
                    border.Background = selected
                        ? new SolidColorBrush(Color.FromRgb(227, 242, 253))
                        : Brushes.White;
                    border.BorderBrush = selected
                        ? new SolidColorBrush(Color.FromRgb(21, 101, 192))
                        : new SolidColorBrush(Color.FromRgb(224, 224, 224));
                    border.BorderThickness = new Thickness(selected ? 2 : 1);
                    break;
                }
            }
        }

        private void UpdateSelectionInfo()
        {
            if (_selectedAlbumIds.Count == 0)
            {
                txtSelectionInfo.Text = $"Chưa chọn album nào. Có {_localAlbums.Count} album, tối đa {MAX_ALBUMS} album/lần.";
            }
            else
            {
                // Tính tổng ảnh
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                int totalPhotos = 0;
                foreach (var albumId in _selectedAlbumIds)
                {
                    var album = _localAlbums.FirstOrDefault(a => a.Id == albumId);
                    if (album != null && Directory.Exists(album.FolderPath))
                    {
                        try
                        {
                            totalPhotos += Directory.GetFiles(album.FolderPath)
                                .Count(f => imageExtensions.Contains(Path.GetExtension(f).ToLower()));
                        }
                        catch { }
                    }
                }

                txtSelectionInfo.Text = $"Đã chọn {_selectedAlbumIds.Count}/{MAX_ALBUMS} album ({totalPhotos} ảnh)";
            }
        }

        private void UpdateUploadButtonState()
        {
            btnUpload.IsEnabled =
                !_isUploading &&
                _selectedAlbumIds.Count > 0;
        }

        #endregion

        #region Upload

        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            if (_uploadService == null || !_uploadService.IsAuthenticated) return;

            var selectedOrg = cboOrganization.SelectedItem as UploadOrganization;
            var orgId = selectedOrg?.Id;
            var orgName = selectedOrg?.Name ?? "(không chọn tổ chức)";

            var selectedAlbums = _localAlbums
                .Where(a => _selectedAlbumIds.Contains(a.Id))
                .ToList();

            if (selectedAlbums.Count == 0) return;

            // Tính tổng ảnh để xác nhận
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            int totalPhotos = selectedAlbums.Sum(a =>
            {
                try { return Directory.GetFiles(a.FolderPath).Count(f => imageExtensions.Contains(Path.GetExtension(f).ToLower())); }
                catch { return 0; }
            });

            var confirmMsg = $"Upload {selectedAlbums.Count} album ({totalPhotos} ảnh) lên tổ chức \"{orgName}\"?\n\n" +
                             $"Album sẽ ở trạng thái Nháp (Draft) — quản trị viên sẽ duyệt sau.";

            if (MessageBox.Show(confirmMsg, "Xác nhận Upload",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            // Start uploading
            _isUploading = true;
            _cts = new CancellationTokenSource();

            btnUpload.IsEnabled = false;
            btnCancel.Content = "Hủy Upload";
            progressPanel.Visibility = Visibility.Visible;
            loginPanel.IsEnabled = false;
            userInfoPanel.IsEnabled = false;
            albumListPanel.IsEnabled = false;

            var progress = new Progress<AlbumUploadProgress>(p =>
            {
                txtProgress.Text = p.Status;
                progressBar.Value = p.Percentage;
            });

            try
            {
                var results = await _uploadService.UploadBatchAsync(
                    selectedAlbums, orgId, progress, _cts.Token);

                // Show results
                var successCount = results.Count(r => r.Success);
                var failCount = results.Count(r => !r.Success);
                UploadedCount = successCount;

                var resultMsg = $"Kết quả upload:\n" +
                                $"✅ Thành công: {successCount} album\n";

                if (failCount > 0)
                {
                    resultMsg += $"❌ Thất bại: {failCount} album\n\n";
                    foreach (var fail in results.Where(r => !r.Success))
                    {
                        resultMsg += $"  • {fail.AlbumTitle}: {fail.Error}\n";
                    }
                }

                // Hiện chi tiết lỗi từng ảnh
                foreach (var r in results.Where(x => x.ImageErrors.Count > 0))
                {
                    resultMsg += $"\n⚠️ Lỗi ảnh trong '{r.AlbumTitle}':\n";
                    foreach (var imgErr in r.ImageErrors.Take(5))
                    {
                        resultMsg += $"  • {imgErr}\n";
                    }
                    if (r.ImageErrors.Count > 5)
                    {
                        resultMsg += $"  ... và {r.ImageErrors.Count - 5} lỗi khác\n";
                    }
                }

                var totalPhotosUploaded = results.Where(r => r.Success).Sum(r => r.PhotoCount);
                var totalFailed = results.Sum(r => r.FailedCount);
                resultMsg += $"\nTổng cộng {totalPhotosUploaded} ảnh đã được upload.";
                if (totalFailed > 0)
                    resultMsg += $" ({totalFailed} ảnh lỗi)";
                resultMsg += $"\n\n💡 Album đang ở trạng thái Nháp. Truy cập trang quản trị web để duyệt và công bố.";

                // Ghi log ra file để debug
                try
                {
                    var logPath = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "AIVanBan", "upload-log.txt");
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath)!);
                    var logContent = $"=== Upload Log {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n{resultMsg}\n\n";
                    foreach (var r in results)
                    {
                        logContent += $"Album: {r.AlbumTitle} | Success: {r.Success} | Photos: {r.PhotoCount} | Failed: {r.FailedCount}\n";
                        foreach (var imgErr in r.ImageErrors)
                        {
                            logContent += $"  ERROR: {imgErr}\n";
                        }
                    }
                    File.AppendAllText(logPath, logContent + "\n");
                    resultMsg += $"\n\n📋 Log: {logPath}";
                }
                catch { }

                MessageBox.Show(resultMsg, "Hoàn tất Upload",
                    MessageBoxButton.OK,
                    failCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);

                if (successCount > 0)
                {
                    DialogResult = true;
                }
            }
            catch (OperationCanceledException)
            {
                txtProgress.Text = "Đã hủy upload.";
                MessageBox.Show("Upload đã bị hủy.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                txtProgress.Text = $"Lỗi: {ex.Message}";
                MessageBox.Show($"Lỗi trong quá trình upload:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isUploading = false;
                _cts?.Dispose();
                _cts = null;

                btnCancel.Content = "Đóng";
                loginPanel.IsEnabled = true;
                userInfoPanel.IsEnabled = true;
                albumListPanel.IsEnabled = true;
                UpdateUploadButtonState();
            }
        }

        #endregion

        #region Dialog Buttons

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (_isUploading && _cts != null)
            {
                if (MessageBox.Show("Bạn có chắc muốn hủy upload?", "Xác nhận",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _cts.Cancel();
                }
                return;
            }

            DialogResult = UploadedCount > 0;
            Close();
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _uploadService?.Dispose();
            base.OnClosed(e);
        }
    }
}
