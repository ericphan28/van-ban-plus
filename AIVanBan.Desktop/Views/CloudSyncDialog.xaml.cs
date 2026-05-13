using System.Text.Json;
using System.Windows;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

/// <summary>
/// Dialog quản lý Cloud Sync — đồng bộ dữ liệu local ↔ cloud VanBanPlus.
/// </summary>
public partial class CloudSyncDialog : Window
{
    private CloudSyncService? _syncService;

    public CloudSyncDialog()
    {
        InitializeComponent();
        LoadSettings();
        LoadStatus();
    }

    // ==================== Load / Save Settings ====================

    private void LoadSettings()
    {
        try
        {
            var settings = AppSettingsService.Load();
            var sync = settings.CloudSync ?? new CloudSyncSettings();

            tglSyncEnabled.IsChecked = sync.Enabled;
            tglAutoSync.IsChecked = sync.AutoSyncEnabled;
            txtInterval.Text = sync.AutoSyncIntervalMinutes.ToString();
            chkSyncOnStartup.IsChecked = sync.SyncOnStartup;
            chkSyncOnExit.IsChecked = sync.SyncOnExit;

            chkSyncDocuments.IsChecked = sync.SyncDocuments;
            chkSyncMeetings.IsChecked = sync.SyncMeetings;
            chkSyncTemplates.IsChecked = sync.SyncTemplates;
            chkSyncFolders.IsChecked = sync.SyncFolders;
            chkSyncPhotos.IsChecked = sync.SyncPhotos;

            rbAutoResolve.IsChecked = sync.ConflictResolution == "auto";
            rbManualResolve.IsChecked = sync.ConflictResolution == "manual";

            txtDeviceName.Text = string.IsNullOrEmpty(sync.DeviceName)
                ? Environment.MachineName
                : sync.DeviceName;

            if (sync.LastSyncTimestamp.HasValue)
                txtLastSync.Text = sync.LastSyncTimestamp.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

            UpdateVisibility(sync.Enabled);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải cài đặt: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void LoadStatus()
    {
        try
        {
            var settings = AppSettingsService.Load();
            if (string.IsNullOrEmpty(settings.VanBanPlusApiUrl) || string.IsNullOrEmpty(settings.VanBanPlusApiKey))
            {
                txtStatus.Text = "⚠️ Chưa đăng nhập VanBanPlus";
                txtStatus.Foreground = System.Windows.Media.Brushes.OrangeRed;
                txtPlan.Text = "Vui lòng đăng nhập trước";
                return;
            }

            txtStatus.Text = "🔄 Đang kiểm tra...";

            _syncService = new CloudSyncService();
            _syncService.Initialize();

            // Gọi API lấy sync status
            var api = new CloudApiClient();
            api.Configure(settings.VanBanPlusApiUrl, settings.VanBanPlusApiKey);

            var statusResult = await api.GetSyncStatus();
            if (statusResult.Success && statusResult.Data is Dictionary<string, object?> statusData)
            {
                txtStatus.Text = "✅ Đã kết nối";
                txtStatus.Foreground = System.Windows.Media.Brushes.Green;

                // Parse storage quota — nested object arrives as JsonElement
                if (statusData.TryGetValue("storage", out var storageObj) && storageObj is JsonElement storageEl
                    && storageEl.ValueKind == JsonValueKind.Object)
                {
                    var usedBytes = storageEl.TryGetProperty("used_bytes", out var ub) ? ub.GetDouble() : 0;
                    var limitBytes = storageEl.TryGetProperty("limit_bytes", out var lb) ? lb.GetDouble() : 0;
                    var usedPercent = storageEl.TryGetProperty("used_percent", out var up) ? up.GetDouble() : 0;
                    var usedDisplay = storageEl.TryGetProperty("used_display", out var ud) ? ud.GetString() : null;
                    var isExceeded = storageEl.TryGetProperty("is_exceeded", out var ie) && ie.GetBoolean();

                    // Hiển thị dung lượng
                    txtStorage.Text = !string.IsNullOrEmpty(usedDisplay)
                        ? usedDisplay
                        : $"{FormatBytes(usedBytes)} / {FormatBytes(limitBytes)}";

                    prgStorage.Value = usedPercent;
                    txtStorageDetail.Text = $"{usedPercent:F1}% đã sử dụng";

                    // Đổi màu khi gần đầy
                    if (isExceeded || usedPercent > 90)
                    {
                        prgStorage.Foreground = System.Windows.Media.Brushes.Red;
                        txtStorageDetail.Text += " ⚠️ Sắp đầy!";
                    }
                    else if (usedPercent > 70)
                    {
                        prgStorage.Foreground = System.Windows.Media.Brushes.Orange;
                    }
                }

                // Parse plan name
                if (statusData.TryGetValue("plan_name", out var planObj) && planObj is JsonElement planEl
                    && planEl.ValueKind == JsonValueKind.String)
                {
                    var planName = planEl.GetString() ?? "Miễn phí";
                    var planId = "";
                    if (statusData.TryGetValue("plan_id", out var pidObj) && pidObj is JsonElement pidEl)
                        planId = pidEl.GetString() ?? "";

                    var planEmoji = planId switch
                    {
                        "starter" => "⭐",
                        "pro" => "💎",
                        "business" => "🏢",
                        _ => "🆓"
                    };
                    txtPlan.Text = $"{planEmoji} {planName}";
                }

                // Parse device count / limit
                var devicesCount = 0;
                var devicesLimit = 0;
                if (statusData.TryGetValue("devices_count", out var dcObj) && dcObj is JsonElement dcEl)
                    devicesCount = dcEl.TryGetInt32(out var dc) ? dc : 0;
                if (statusData.TryGetValue("devices_limit", out var dlObj) && dlObj is JsonElement dlEl)
                    devicesLimit = dlEl.TryGetInt32(out var dl) ? dl : 0;

                txtDeviceCount.Text = devicesLimit > 0
                    ? $"{devicesCount} / {devicesLimit} thiết bị"
                    : $"{devicesCount} thiết bị";

                if (devicesCount >= devicesLimit && devicesLimit > 0)
                    txtDeviceCount.Foreground = System.Windows.Media.Brushes.OrangeRed;

                // Parse last sync
                if (statusData.TryGetValue("last_sync", out var lsObj) && lsObj is JsonElement lsEl
                    && lsEl.ValueKind == JsonValueKind.Object)
                {
                    if (lsEl.TryGetProperty("created_at", out var caEl) && caEl.ValueKind == JsonValueKind.String)
                    {
                        if (DateTime.TryParse(caEl.GetString(), out var lastSyncDate))
                            txtLastSync.Text = lastSyncDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
                    }
                }
            }
            else
            {
                txtStatus.Text = "⚠️ Không thể kết nối";
                txtStatus.Foreground = System.Windows.Media.Brushes.OrangeRed;
                if (!string.IsNullOrEmpty(statusResult.Message))
                    txtStorageDetail.Text = statusResult.Message;
            }
        }
        catch (Exception ex)
        {
            txtStatus.Text = "❌ Lỗi kết nối";
            txtStatus.Foreground = System.Windows.Media.Brushes.Red;
            txtStorageDetail.Text = ex.Message;
        }
    }

    /// <summary>
    /// Format bytes thành đơn vị dễ đọc (KB, MB, GB).
    /// </summary>
    private static string FormatBytes(double bytes)
    {
        if (bytes < 1024) return $"{bytes:F0} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024 * 1024):F1} MB";
        return $"{bytes / (1024 * 1024 * 1024):F2} GB";
    }

    // ==================== UI Events ====================

    private void SyncToggle_Changed(object sender, RoutedEventArgs e)
    {
        var enabled = tglSyncEnabled.IsChecked == true;
        UpdateVisibility(enabled);
        txtSyncHint.Text = enabled
            ? "Đồng bộ đám mây đang BẬT"
            : "Tự động sao lưu và đồng bộ dữ liệu lên cloud";
    }

    private void UpdateVisibility(bool enabled)
    {
        var vis = enabled ? Visibility.Visible : Visibility.Collapsed;
        grpStatus.Visibility = vis;
        grpSyncConfig.Visibility = vis;
    }

    private async void SyncNow_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            btnSyncNow.IsEnabled = false;
            btnSyncNow.Content = "🔄 Đang đồng bộ...";

            if (_syncService == null)
            {
                _syncService = new CloudSyncService();
                _syncService.Initialize();
            }

            var result = await _syncService.RunSync();
            if (result == null)
            {
                MessageBox.Show("Đồng bộ đang chạy hoặc đã hủy.", "Đồng bộ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var msg = $"✅ Đồng bộ hoàn tất!\n\nĐã đẩy lên: {result.ItemsPushed} mục\nĐã kéo về: {result.ItemsPulled} mục";
            if (result.Conflicts?.Count > 0)
                msg += $"\n⚠️ Xung đột: {result.Conflicts.Count}";

            MessageBox.Show(msg, "Đồng bộ", MessageBoxButton.OK, MessageBoxImage.Information);

            // Reload status
            LoadStatus();
            var sync = AppSettingsService.Load().CloudSync ?? new CloudSyncSettings();
            sync.LastSyncTimestamp = DateTime.UtcNow;
            var settings = AppSettingsService.Load();
            settings.CloudSync = sync;
            AppSettingsService.Save(settings);
            txtLastSync.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi đồng bộ: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            btnSyncNow.IsEnabled = true;
            btnSyncNow.Content = "🔄 Đồng bộ ngay";
        }
    }

    private async void BackupNow_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            btnBackupNow.IsEnabled = false;
            btnBackupNow.Content = "💾 Đang sao lưu...";

            var settings = AppSettingsService.Load();
            var api = new CloudApiClient();
            api.Configure(settings.VanBanPlusApiUrl!, settings.VanBanPlusApiKey!);

            var result = await api.CreateBackup("Manual backup từ " + Environment.MachineName);
            if (result.Success)
            {
                MessageBox.Show("✅ Đã sao lưu thành công lên cloud!", "Sao lưu",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"⚠️ Không thể sao lưu: {result.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi sao lưu: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            btnBackupNow.IsEnabled = true;
            btnBackupNow.Content = "💾 Sao lưu ngay";
        }
    }

    private async void RestoreBackup_Click(object sender, RoutedEventArgs e)
    {
        var confirm = MessageBox.Show(
            "⚠️ Khôi phục từ cloud sẽ GHI ĐÈ dữ liệu local.\n\nBạn có chắc chắn?",
            "Xác nhận khôi phục",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            btnRestoreBackup.IsEnabled = false;
            btnRestoreBackup.Content = "📥 Đang khôi phục...";

            var settings = AppSettingsService.Load();
            var api = new CloudApiClient();
            api.Configure(settings.VanBanPlusApiUrl!, settings.VanBanPlusApiKey!);

            // List backups và lấy bản mới nhất
            var listResult = await api.ListBackups();
            if (!listResult.Success || listResult.Data == null)
            {
                MessageBox.Show("Không tìm thấy bản sao lưu nào trên cloud.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Parse backup list - Data is List<CloudBackupInfo>
            var backups = listResult.Data;
            if (backups.Count == 0)
            {
                MessageBox.Show("Không tìm thấy bản sao lưu nào trên cloud.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Lấy backup mới nhất
            var latestId = backups[0].Id;
            var restoreResult = await api.RestoreBackup(latestId);

            if (restoreResult.Success)
            {
                MessageBox.Show("✅ Đã khôi phục thành công!\n\nVui lòng khởi động lại ứng dụng.",
                    "Hoàn tất", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"⚠️ Lỗi khôi phục: {restoreResult.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khôi phục: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            btnRestoreBackup.IsEnabled = true;
            btnRestoreBackup.Content = "📥 Khôi phục từ cloud";
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var settings = AppSettingsService.Load();
            var sync = settings.CloudSync ?? new CloudSyncSettings();

            sync.Enabled = tglSyncEnabled.IsChecked == true;
            sync.AutoSyncEnabled = tglAutoSync.IsChecked == true;

            if (int.TryParse(txtInterval.Text, out var interval) && interval >= 1)
                sync.AutoSyncIntervalMinutes = interval;
            else
                sync.AutoSyncIntervalMinutes = 5;

            sync.SyncOnStartup = chkSyncOnStartup.IsChecked == true;
            sync.SyncOnExit = chkSyncOnExit.IsChecked == true;

            sync.SyncDocuments = chkSyncDocuments.IsChecked == true;
            sync.SyncMeetings = chkSyncMeetings.IsChecked == true;
            sync.SyncTemplates = chkSyncTemplates.IsChecked == true;
            sync.SyncFolders = chkSyncFolders.IsChecked == true;
            sync.SyncPhotos = chkSyncPhotos.IsChecked == true;

            sync.ConflictResolution = rbAutoResolve.IsChecked == true ? "auto" : "manual";

            settings.CloudSync = sync;
            AppSettingsService.Save(settings);

            // Invalidate SyncTracker cache
            SyncTracker.InvalidateCache();

            MessageBox.Show("✅ Đã lưu cài đặt Cloud Sync!", "Thành công",
                MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi lưu cài đặt: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
