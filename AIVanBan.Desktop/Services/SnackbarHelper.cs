using System.Windows;
using MaterialDesignThemes.Wpf;

namespace AIVanBan.Desktop.Services;

/// <summary>
/// Helper tĩnh để hiện Snackbar thông báo thay vì MessageBox cho info/success.
/// MessageBox chỉ nên dùng cho xác nhận xóa hoặc cảnh báo lỗi nghiêm trọng.
/// </summary>
public static class SnackbarHelper
{
    private static Snackbar? _mainSnackbar;

    /// <summary>
    /// Khởi tạo với MainSnackbar từ MainWindow. Gọi 1 lần khi app start.
    /// </summary>
    public static void Initialize(Snackbar snackbar)
    {
        _mainSnackbar = snackbar;
    }

    /// <summary>
    /// Hiện thông báo thành công (icon ✓, 3 giây tự đóng)
    /// </summary>
    public static void ShowSuccess(string message, int durationSeconds = 3)
    {
        Show($"✅ {message}", durationSeconds);
    }

    /// <summary>
    /// Hiện thông báo thông tin (3 giây)
    /// </summary>
    public static void ShowInfo(string message, int durationSeconds = 3)
    {
        Show($"ℹ️ {message}", durationSeconds);
    }

    /// <summary>
    /// Hiện thông báo cảnh báo (4 giây)
    /// </summary>
    public static void ShowWarning(string message, int durationSeconds = 4)
    {
        Show($"⚠️ {message}", durationSeconds);
    }

    /// <summary>
    /// Hiện Snackbar với nút hành động
    /// </summary>
    public static void ShowWithAction(string message, string actionText, Action action, int durationSeconds = 5)
    {
        if (_mainSnackbar?.MessageQueue == null) return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            _mainSnackbar.MessageQueue.Enqueue(
                message,
                actionText,
                _ => action(),
                null,
                false,
                true,
                TimeSpan.FromSeconds(durationSeconds));
        });
    }

    private static void Show(string message, int durationSeconds)
    {
        if (_mainSnackbar?.MessageQueue == null)
        {
            // Fallback nếu Snackbar chưa được khởi tạo
            return;
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            _mainSnackbar.MessageQueue.Enqueue(
                message,
                null,
                null,
                null,
                false,
                true,
                TimeSpan.FromSeconds(durationSeconds));
        });
    }
}
