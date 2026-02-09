using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using AIVanBan.Core.Services;
using Microsoft.Win32;

namespace AIVanBan.Desktop.Views;

public partial class BackupRestorePage : Page
{
    private readonly BackupService _backupService;

    public BackupRestorePage()
    {
        InitializeComponent();
        _backupService = new BackupService();
        LoadData();
    }

    private void LoadData()
    {
        // Hiển thị dung lượng dữ liệu (tổng + chi tiết)
        var (dataSize, photosSize) = _backupService.GetDataSizeDetails();
        var totalSize = dataSize + photosSize;
        txtDataSize.Text = BackupService.FormatFileSize(totalSize);
        txtDataDetails.Text = $"📄 Văn bản & DB: {BackupService.FormatFileSize(dataSize)}  |  📷 Album ảnh: {BackupService.FormatFileSize(photosSize)}";
        txtDataPath.Text = _backupService.DataPath;

        // Load danh sách backup
        LoadBackupList();

        // Hiển thị thông tin auto backup
        UpdateAutoBackupStatus();
    }

    private void LoadBackupList()
    {
        var backups = _backupService.GetBackupList();
        dgBackups.ItemsSource = backups;
    }

    private void UpdateAutoBackupStatus()
    {
        var autoDir = System.IO.Path.Combine(_backupService.BackupPath, "Auto");
        var autoBackups = _backupService.GetBackupList(autoDir);

        if (autoBackups.Any())
        {
            var latest = autoBackups.First();
            txtAutoBackupStatus.Text = $"✅ Tự động sao lưu: Bản gần nhất lúc {latest.CreatedDate:dd/MM/yyyy HH:mm} " +
                                       $"({latest.FileSizeFormatted}) — Tổng {autoBackups.Count} bản";
        }
        else
        {
            txtAutoBackupStatus.Text = "⚠️ Tự động sao lưu: Chưa có bản nào. Hệ thống sẽ tự backup khi mở app.";
        }
    }

    /// <summary>
    /// Sao lưu thủ công — chọn nơi lưu hoặc lưu mặc định.
    /// </summary>
    private void BtnBackup_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var choice = MessageBox.Show(
                "Bạn muốn lưu bản sao lưu ở đâu?\n\n" +
                "• Bấm [Yes] → Chọn thư mục tùy ý\n" +
                "• Bấm [No] → Lưu vào thư mục mặc định\n" +
                "• Bấm [Cancel] → Hủy",
                "Sao lưu dữ liệu",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (choice == MessageBoxResult.Cancel) return;

            string? targetPath = null;

            if (choice == MessageBoxResult.Yes)
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Chọn thư mục lưu bản sao lưu",
                    ShowNewFolderButton = true
                };

                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                targetPath = dialog.SelectedPath;
            }

            btnBackup.IsEnabled = false;

            var result = _backupService.Backup(targetPath);

            if (result.Success)
            {
                MessageBox.Show(
                    $"✅ Sao lưu thành công!\n\n" +
                    $"📁 File: {result.FilePath}\n" +
                    $"💾 Dung lượng: {BackupService.FormatFileSize(result.FileSize)}",
                    "Sao lưu thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadBackupList();
                UpdateAutoBackupStatus();
            }
            else
            {
                MessageBox.Show(
                    $"❌ {result.ErrorMessage}",
                    "Lỗi sao lưu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            btnBackup.IsEnabled = true;
        }
    }

    /// <summary>
    /// Khôi phục — chọn file .zip backup.
    /// </summary>
    private void BtnRestore_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var openDialog = new OpenFileDialog
            {
                Title = "Chọn file sao lưu để khôi phục",
                Filter = "Backup files (*.zip)|*.zip",
                InitialDirectory = _backupService.BackupPath
            };

            if (openDialog.ShowDialog() != true) return;

            DoRestore(openDialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Khôi phục từ 1 file trong danh sách.
    /// </summary>
    private void BtnRestoreFromList_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string filePath)
        {
            DoRestore(filePath);
        }
    }

    private void DoRestore(string filePath)
    {
        var confirm = MessageBox.Show(
            $"⚠️ CẢNH BÁO: Khôi phục dữ liệu sẽ GHI ĐÈ toàn bộ dữ liệu hiện tại!\n\n" +
            $"File: {System.IO.Path.GetFileName(filePath)}\n\n" +
            $"Hệ thống sẽ tự động sao lưu dữ liệu hiện tại trước khi khôi phục.\n\n" +
            $"Sau khi khôi phục, ứng dụng cần được KHỞI ĐỘNG LẠI.\n\n" +
            $"Bạn có chắc chắn muốn khôi phục?",
            "Xác nhận khôi phục",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        var result = _backupService.Restore(filePath);

        if (result.Success)
        {
            MessageBox.Show(
                $"✅ Khôi phục thành công!\n\n" +
                $"Bản sao lưu an toàn đã được tạo tại:\n{result.SafetyBackupPath}\n\n" +
                $"Ứng dụng sẽ đóng lại. Vui lòng mở lại để sử dụng dữ liệu đã khôi phục.",
                "Khôi phục thành công",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Đóng app để reload data
            Application.Current.Shutdown();
        }
        else
        {
            MessageBox.Show(
                $"❌ {result.ErrorMessage}",
                "Lỗi khôi phục",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void BtnDeleteBackup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string filePath)
        {
            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn xóa bản backup này?\n\n{System.IO.Path.GetFileName(filePath)}",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                _backupService.DeleteBackup(filePath);
                LoadBackupList();
                UpdateAutoBackupStatus();
            }
        }
    }

    private void BtnOpenFileLocation_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string filePath)
        {
            if (System.IO.File.Exists(filePath))
                Process.Start("explorer.exe", $"/select,\"{filePath}\"");
        }
    }

    private void BtnOpenBackupFolder_Click(object sender, RoutedEventArgs e)
    {
        var path = _backupService.BackupPath;
        if (System.IO.Directory.Exists(path))
            Process.Start("explorer.exe", path);
        else
            MessageBox.Show("Thư mục backup chưa tồn tại.", "Thông báo");
    }

    private void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        LoadData();
    }
}
