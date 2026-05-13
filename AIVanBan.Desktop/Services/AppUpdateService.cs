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

    // Cờ đánh dấu đang trong lượt "kiểm tra thủ công" — để hiện dialog cả khi đã mới nhất.
    // (Không dùng AutoUpdater.ReportErrors vì event chạy async, finally reset cờ trước khi event firing.)
    private static bool _isManualCheck = false;

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
    /// Luôn hiện dialog kết quả — kể cả khi đã là phiên bản mới nhất.
    /// </summary>
    public static void CheckForUpdateManual()
    {
        _isManualCheck = true; // Cờ sẽ được reset trong OnCheckForUpdateEvent
        AutoUpdater.ReportErrors = true;
        try
        {
            AutoUpdater.Start(UpdateXmlUrl);
        }
        catch (Exception ex)
        {
            _isManualCheck = false;
            MessageBox.Show(
                $"Không thể kiểm tra cập nhật.\n\nLỗi: {ex.Message}",
                "Kiểm tra cập nhật",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        // KHÔNG reset trong finally — để OnCheckForUpdateEvent tự reset sau khi xử lý xong
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
    /// Tự download installer bằng HttpClient + hiển thị cửa sổ tiến trình thân thiện.
    /// </summary>
    private static async Task DownloadAndRunInstallerAsync(UpdateInfoEventArgs args)
    {
        var downloadUrl = args.DownloadURL;
        var fileName = Path.GetFileName(new Uri(downloadUrl).LocalPath);
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);
        long totalRead = 0;

        // Mở cửa sổ tiến trình
        var progressWindow = new UpdateProgressWindow(fileName);
        progressWindow.Owner = Application.Current.MainWindow;
        progressWindow.Show();

        try
        {
            // Xóa file cũ nếu tồn tại
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { }
            }

            Console.WriteLine($"[UpdateService] Downloading: {downloadUrl}");
            Console.WriteLine($"[UpdateService] Save to: {tempPath}");

            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromMinutes(10);

                using (var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? -1;
                    Console.WriteLine($"[UpdateService] File size: {totalBytes / 1024.0 / 1024.0:F1} MB");

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[81920];
                        int bytesRead;
                        var lastUiUpdate = DateTime.UtcNow;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            // Kiểm tra hủy
                            if (progressWindow.IsCancelled)
                            {
                                throw new OperationCanceledException("Người dùng đã hủy tải.");
                            }

                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalRead += bytesRead;

                            // Cập nhật UI tối đa 10 lần/giây để mượt mà mà không lag
                            var now = DateTime.UtcNow;
                            if ((now - lastUiUpdate).TotalMilliseconds >= 100)
                            {
                                lastUiUpdate = now;
                                var captured = totalRead;
                                progressWindow.Dispatcher.Invoke(() =>
                                    progressWindow.ReportProgress(captured, totalBytes));
                            }
                        }
                    }
                }
            }

            // Update UI lần cuối
            progressWindow.Dispatcher.Invoke(() =>
            {
                progressWindow.ReportProgress(totalRead, totalRead);
                progressWindow.SetCompleted();
            });

            Console.WriteLine($"[UpdateService] Download completed: {totalRead / 1024.0 / 1024.0:F1} MB");

            // Đợi OS release file lock
            await Task.Delay(1000);

            // Đóng cửa sổ tiến trình trước khi hỏi cài đặt
            progressWindow.Dispatcher.Invoke(() => progressWindow.Close());

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
                var startInfo = new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(startInfo);
                Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[UpdateService] Download cancelled by user.");
            progressWindow.Dispatcher.Invoke(() => progressWindow.Close());
            try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
            MessageBox.Show(
                "Đã hủy tải bản cập nhật.\n\nBạn có thể cập nhật lại bất cứ lúc nào từ menu Trợ giúp → Kiểm tra cập nhật.",
                $"{AppTitle} - Đã hủy",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UpdateService] Download/Install failed: {ex.Message}");

            try
            {
                progressWindow.Dispatcher.Invoke(() => progressWindow.SetError(ex.Message));
                await Task.Delay(1500);
                progressWindow.Dispatcher.Invoke(() => progressWindow.Close());
            }
            catch { }

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
                Console.WriteLine($"[UpdateService] Mandatory update: {args.Mandatory?.Value}");

                // Nếu mandatory → không cho skip, chỉ có nút OK
                bool isMandatory = args.Mandatory?.Value == true;
                
                if (isMandatory)
                {
                    MessageBox.Show(
                        $"Phiên bản hiện tại ({GetCurrentVersion()}) đã cũ và cần cập nhật.\n\n" +
                        $"Phiên bản mới: {args.CurrentVersion}\n\n" +
                        $"Bản cập nhật này là BẮT BUỘC để sửa lỗi quan trọng.\n" +
                        $"Nhấn OK để tải và cài đặt ngay.",
                        $"{AppTitle} - Cập nhật bắt buộc",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    
                    // Tự động download
                    _ = Task.Run(async () =>
                    {
                        await Application.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            await DownloadAndRunInstallerAsync(args);
                        });
                    });
                }
                else
                {
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
                        _ = Task.Run(async () =>
                        {
                            await Application.Current.Dispatcher.InvokeAsync(async () =>
                            {
                                await DownloadAndRunInstallerAsync(args);
                            });
                        });
                    }
                }
            }
            else
            {
                Console.WriteLine("[UpdateService] App is up to date.");

                // Chỉ hiện thông báo nếu user check thủ công
                if (_isManualCheck)
                {
                    MessageBox.Show(
                        $"✅ Bạn đang sử dụng phiên bản mới nhất!\n\n" +
                        $"Phiên bản hiện tại: v{GetCurrentVersion()}\n" +
                        $"Không có bản cập nhật nào mới hơn.\n\n" +
                        $"Được kiểm tra từ: GitHub Releases",
                        $"{AppTitle} - Kiểm tra cập nhật",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }
        else
        {
            Console.WriteLine($"[UpdateService] Check failed: {args.Error.Message}");

            if (_isManualCheck)
            {
                MessageBox.Show(
                    $"⚠️ Không thể kiểm tra cập nhật.\n\n" +
                    $"Vui lòng kiểm tra kết nối Internet và thử lại.\n\n" +
                    $"Chi tiết lỗi: {args.Error.Message}",
                    $"{AppTitle} - Lỗi kiểm tra cập nhật",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        // Reset cờ manual check sau khi đã xử lý xong
        _isManualCheck = false;
        AutoUpdater.ReportErrors = false;
    }
}
