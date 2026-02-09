using System;
using System.Reflection;
using System.Windows;
using AutoUpdaterDotNET;

namespace AIVanBan.Desktop.Services;

/// <summary>
/// Service quản lý tự động cập nhật ứng dụng VanBanPlus.
/// Sử dụng AutoUpdater.NET để kiểm tra và tải bản cập nhật mới.
/// </summary>
public static class AppUpdateService
{
    // =====================================================
    // CẤU HÌNH - THAY ĐỔI KHI DEPLOY
    // =====================================================

    /// <summary>
    /// URL tới file XML chứa thông tin version mới nhất.
    /// Có thể host trên: GitHub Pages, Google Drive, Web server, v.v.
    /// </summary>
    private const string UpdateXmlUrl = "https://raw.githubusercontent.com/YOUR_GITHUB_USERNAME/vanbanplus-updates/main/update.xml";

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
                    try
                    {
                        if (AutoUpdater.DownloadUpdate(args))
                        {
                            // Thoát app để installer chạy
                            Application.Current.Shutdown();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Lỗi khi tải bản cập nhật:\n{ex.Message}",
                            "Lỗi cập nhật",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
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
