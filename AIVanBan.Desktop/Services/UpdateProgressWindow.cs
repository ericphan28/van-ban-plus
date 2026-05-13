using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace AIVanBan.Desktop.Services;

/// <summary>
/// Cửa sổ hiển thị tiến trình download bản cập nhật.
/// Có ProgressBar, %, dung lượng đã tải, tốc độ, nút Hủy.
/// </summary>
internal sealed class UpdateProgressWindow : Window
{
    private readonly ProgressBar _progressBar;
    private readonly TextBlock _txtPercent;
    private readonly TextBlock _txtDownloaded;
    private readonly TextBlock _txtSpeed;
    private readonly TextBlock _txtStatus;
    private readonly Button _btnCancel;

    private DateTime _startTime;
    private long _lastBytes;
    private DateTime _lastTick;
    private readonly DispatcherTimer _speedTimer;

    public bool IsCancelled { get; private set; }

    public UpdateProgressWindow(string fileName)
    {
        Title = "VanBanPlus - Đang tải bản cập nhật";
        Width = 480;
        Height = 240;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.SingleBorderWindow;
        Background = new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5));
        ShowInTaskbar = true;
        Topmost = false;

        var root = new Grid { Margin = new Thickness(20) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // File name
        var titleText = new TextBlock
        {
            Text = $"📥 Đang tải: {fileName}",
            FontWeight = FontWeights.SemiBold,
            FontSize = 14,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        Grid.SetRow(titleText, 0);
        root.Children.Add(titleText);

        // Status
        _txtStatus = new TextBlock
        {
            Text = "Đang kết nối tới máy chủ...",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x60))
        };
        Grid.SetRow(_txtStatus, 2);
        root.Children.Add(_txtStatus);

        // Progress bar
        _progressBar = new ProgressBar
        {
            Height = 22,
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            IsIndeterminate = true,
            Foreground = new SolidColorBrush(Color.FromRgb(0x19, 0x76, 0xD2))
        };
        Grid.SetRow(_progressBar, 4);
        root.Children.Add(_progressBar);

        // Stats row
        var statsPanel = new Grid();
        statsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        statsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        statsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        _txtPercent = new TextBlock
        {
            Text = "0%",
            FontWeight = FontWeights.Bold,
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(0x19, 0x76, 0xD2))
        };
        Grid.SetColumn(_txtPercent, 0);
        statsPanel.Children.Add(_txtPercent);

        _txtDownloaded = new TextBlock
        {
            Text = "",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromRgb(0x40, 0x40, 0x40))
        };
        Grid.SetColumn(_txtDownloaded, 1);
        statsPanel.Children.Add(_txtDownloaded);

        _txtSpeed = new TextBlock
        {
            Text = "",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Right,
            Foreground = new SolidColorBrush(Color.FromRgb(0x40, 0x40, 0x40))
        };
        Grid.SetColumn(_txtSpeed, 2);
        statsPanel.Children.Add(_txtSpeed);

        Grid.SetRow(statsPanel, 6);
        root.Children.Add(statsPanel);

        // Cancel button
        _btnCancel = new Button
        {
            Content = "Hủy",
            Width = 90,
            Height = 32,
            HorizontalAlignment = HorizontalAlignment.Right,
            Background = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        _btnCancel.Click += (_, _) =>
        {
            IsCancelled = true;
            _btnCancel.IsEnabled = false;
            _btnCancel.Content = "Đang hủy...";
            _txtStatus.Text = "Đang hủy tải...";
        };
        Grid.SetRow(_btnCancel, 8);
        root.Children.Add(_btnCancel);

        Content = root;

        _startTime = DateTime.UtcNow;
        _lastTick = _startTime;

        // Timer cập nhật speed mỗi 500ms
        _speedTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _speedTimer.Tick += UpdateSpeed;
        _speedTimer.Start();

        Closed += (_, _) => _speedTimer.Stop();
    }

    private long _currentBytes;
    private long _totalBytes;

    public void ReportProgress(long bytesRead, long totalBytes)
    {
        _currentBytes = bytesRead;
        _totalBytes = totalBytes;

        if (totalBytes > 0)
        {
            _progressBar.IsIndeterminate = false;
            var percent = (double)bytesRead * 100.0 / totalBytes;
            _progressBar.Value = percent;
            _txtPercent.Text = $"{percent:F1}%";
            _txtDownloaded.Text = $"{FormatMB(bytesRead)} / {FormatMB(totalBytes)}";
            _txtStatus.Text = "Đang tải dữ liệu từ GitHub...";
        }
        else
        {
            _txtDownloaded.Text = FormatMB(bytesRead);
        }
    }

    public void SetCompleted()
    {
        _progressBar.IsIndeterminate = false;
        _progressBar.Value = 100;
        _txtPercent.Text = "100%";
        _txtStatus.Text = "✅ Hoàn tất! Chuẩn bị cài đặt...";
        _btnCancel.IsEnabled = false;
        _speedTimer.Stop();
    }

    public void SetError(string message)
    {
        _progressBar.IsIndeterminate = false;
        _progressBar.Foreground = new SolidColorBrush(Color.FromRgb(0xD3, 0x2F, 0x2F));
        _txtStatus.Text = $"❌ Lỗi: {message}";
        _btnCancel.Content = "Đóng";
        _btnCancel.IsEnabled = true;
        _speedTimer.Stop();
    }

    private void UpdateSpeed(object? sender, EventArgs e)
    {
        var now = DateTime.UtcNow;
        var elapsed = (now - _lastTick).TotalSeconds;
        if (elapsed <= 0) return;

        var deltaBytes = _currentBytes - _lastBytes;
        var bytesPerSec = deltaBytes / elapsed;
        _lastBytes = _currentBytes;
        _lastTick = now;

        if (bytesPerSec > 0)
        {
            string speedText = bytesPerSec >= 1024 * 1024
                ? $"⬇ {bytesPerSec / 1024.0 / 1024.0:F2} MB/s"
                : $"⬇ {bytesPerSec / 1024.0:F0} KB/s";

            // ETA
            if (_totalBytes > 0 && bytesPerSec > 0)
            {
                var remainingBytes = _totalBytes - _currentBytes;
                var etaSec = remainingBytes / bytesPerSec;
                if (etaSec > 0 && etaSec < 3600)
                {
                    speedText += etaSec >= 60
                        ? $"  •  còn ~{(int)(etaSec / 60)}p {(int)(etaSec % 60)}s"
                        : $"  •  còn ~{(int)etaSec}s";
                }
            }
            _txtSpeed.Text = speedText;
        }
    }

    private static string FormatMB(long bytes) => $"{bytes / 1024.0 / 1024.0:F2} MB";
}
