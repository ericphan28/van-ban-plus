using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using AutoUpdaterDotNET;

namespace AIVanBan.Desktop.Services;

/// <summary>
/// Service quản lý tự động cập nhật ứng dụng VanBanPlus.
/// Sử dụng AutoUpdater.NET để kiểm tra version mới,
/// và tự xử lý download bằng HttpClient (tránh lỗi WinForms ScaleHelper).
/// </summary>
public static class AppUpdateService
{
    // =====================================================
    // CẤU HÌNH - THAY ĐỔI KHI DEPLOY
    // =====================================================

    /// <summary>
    /// URL tới file XML chứa thông tin version mới nhất.
    /// </summary>
    private const string UpdateXmlUrl = "https://raw.githubusercontent.com/ericphan28/van-ban-plus-releases/main/update.xml";

    // Tên app hiển thị
    private const string AppTitle = "VanBanPlus";

    /// <summary>
    /// Khởi tạo và cấu hình AutoUpdater.
    /// Gọi 1 lần trong App.OnStartup hoặc MainWindow constructor.
    /// </summary>
    public static void Initialize()
    {
        // Tên app hiển thị trên dialog update
        AutoUpdater.AppTitle = AppTitle;

        // Cho phép user bỏ qua version này
        AutoUpdater.ShowSkipButton = true;

        // Cho phép nhắc lại sau
        AutoUpdater.ShowRemindLaterButton = true;

        // Chạy update installer với quyền admin
        AutoUpdater.RunUpdateAsAdmin = true;

        // Tự động report error khi không thể check update (set false cho production)
        AutoUpdater.ReportErrors = false;

        // Đăng ký event handlers
        AutoUpdater.CheckForUpdateEvent += OnCheckForUpdateEvent;

        Console.WriteLine($"[UpdateService] Initialized. Version: {GetCurrentVersion()}");
    }

    /// <summary>
    /// Kiểm tra update ngầm khi app khởi động (không hiện dialog nếu đã mới nhất).
    /// </summary>
    public static void CheckForUpdateSilent()
    {
        try
        {
            AutoUpdater.Start(UpdateXmlUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UpdateService] Silent check failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Kiểm tra update thủ công (từ menu Help > Check for Updates).
    /// Luôn hiện dialog kết quả.
    /// </summary>
    public static void CheckForUpdateManual()
    {
        AutoUpdater.ReportErrors = true; // Hiện thông báo nếu lỗi
        try
        {
            AutoUpdater.Start(UpdateXmlUrl);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Không thể kiểm tra cập nhật.\n\nLỗi: {ex.Message}",
                "Kiểm tra cập nhật",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        finally
        {
            AutoUpdater.ReportErrors = false; // Reset lại
        }
    }

    /// <summary>
    /// Lấy version hiện tại của ứng dụng.
    /// </summary>
    public static string GetCurrentVersion()
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
    }

    /// <summary>
    /// Tự download installer bằng HttpClient (tránh lỗi WinForms ScaleHelper).
    /// </summary>
    private static async Task DownloadAndRunInstallerAsync(UpdateInfoEventArgs args)
    {
        var downloadUrl = args.DownloadURL;
        var fileName = Path.GetFileName(new Uri(downloadUrl).LocalPath);
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);

        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(10);

            // Hiển thị thông báo đang tải
            MessageBox.Show(
                $"Bắt đầu tải bản cập nhật...\n\n" +
                $"File: {fileName}\n" +
                $"Vui lòng đợi trong giây lát.\n\n" +
                $"Nhấn OK để bắt đầu tải.",
                $"{AppTitle} - Đang tải cập nhật",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Console.WriteLine($"[UpdateService] Downloading: {downloadUrl}");
            Console.WriteLine($"[UpdateService] Save to: {tempPath}");

            // Download file
            using var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            Console.WriteLine($"[UpdateService] File size: {totalBytes / 1024.0 / 1024.0:F1} MB");

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[81920];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;

                if (totalBytes > 0)
                {
                    var percent = (int)(totalRead * 100 / totalBytes);
                    if (percent % 10 == 0)
                        Console.WriteLine($"[UpdateService] Download progress: {percent}%");
                }
            }

            Console.WriteLine($"[UpdateService] Download completed: {totalRead / 1024.0 / 1024.0:F1} MB");

            // Chạy installer
            var result = MessageBox.Show(
                $"Đã tải bản cập nhật thành công!\n\n" +
                $"File: {fileName}\n" +
                $"Kích thước: {totalRead / 1024.0 / 1024.0:F1} MB\n\n" +
                $"Nhấn OK để cài đặt. Ứng dụng sẽ đóng lại.",
                $"{AppTitle} - Cập nhật sẵn sàng",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.OK)
            {
                // Chạy installer
                var startInfo = new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true,
                    Verb = "runas" // Chạy với quyền admin
                };

                Process.Start(startInfo);

                // Thoát app
                Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UpdateService] Download failed: {ex.Message}");

            // Fallback: mở trình duyệt để download
            var fallbackResult = MessageBox.Show(
                $"Không thể tải tự động.\n\nLỗi: {ex.Message}\n\n" +
                $"Bạn có muốn mở trình duyệt để tải thủ công không?",
                $"{AppTitle} - Lỗi tải cập nhật",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (fallbackResult == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadUrl,
                        UseShellExecute = true
                    });
                }
                catch { }
            }
        }
    }

    /// <summary>
    /// Xử lý kết quả kiểm tra update.
    /// </summary>
    private static void OnCheckForUpdateEvent(UpdateInfoEventArgs args)
    {
        if (args.Error == null)
        {
            if (args.IsUpdateAvailable)
            {
                Console.WriteLine($"[UpdateService] New version available: {args.CurrentVersion}");

                var result = MessageBox.Show(
                    $"Đã có phiên bản mới!\n\n" +
                    $"Phiên bản hiện tại: {GetCurrentVersion()}\n" +
                    $"Phiên bản mới: {args.CurrentVersion}\n\n" +
                    $"{(string.IsNullOrEmpty(args.ChangelogURL) ? "" : "Xem chi tiết thay đổi sau khi cập nhật.\n\n")}" +
                    $"Bạn có muốn cập nhật ngay không?",
                    $"{AppTitle} - Cập nhật phần mềm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    // Sử dụng download thủ công thay vì AutoUpdater.DownloadUpdate
                    // để tránh lỗi WinForms ScaleHelper trong self-contained publish
                    _ = Task.Run(async () =>
                    {
                        await Application.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            await DownloadAndRunInstallerAsync(args);
                        });
                    });
                }
            }
            else
            {
                Console.WriteLine("[UpdateService] App is up to date.");

                // Chỉ hiện thông báo nếu user check thủ công
                if (AutoUpdater.ReportErrors)
                {
                    MessageBox.Show(
                        $"Bạn đang sử dụng phiên bản mới nhất ({GetCurrentVersion()}).",
                        $"{AppTitle} - Cập nhật",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }
        else
        {
            Console.WriteLine($"[UpdateService] Check failed: {args.Error.Message}");

            if (AutoUpdater.ReportErrors)
            {
                MessageBox.Show(
                    $"Không thể kiểm tra cập nhật.\nVui lòng kiểm tra kết nối mạng.\n\nChi tiết: {args.Error.Message}",
                    $"{AppTitle} - Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}
