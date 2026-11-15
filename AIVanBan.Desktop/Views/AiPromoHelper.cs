using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

/// <summary>
/// Helper hiển thị thông tin quảng bá AI khi chưa kích hoạt.
/// Dùng chung cho tất cả các entry point AI.
/// </summary>
public static class AiPromoHelper
{
    /// <summary>
    /// Kiểm tra AI đã sẵn sàng chưa. Nếu chưa → hiện dialog quảng bá rồi return false.
    /// </summary>
    public static bool CheckOrShowPromo(Window owner)
    {
        if (AppSettingsService.IsAiReady())
            return true;

        ShowPromoDialog(owner);
        return false;
    }

    /// <summary>
    /// Hiện dialog quảng bá AI thân thiện, khéo léo
    /// </summary>
    private static void ShowPromoDialog(Window owner)
    {
        var settings = AppSettingsService.Load();
        var hasKey = !string.IsNullOrWhiteSpace(AppSettingsService.GetEffectiveApiKey());

        var dialog = new Window
        {
            Title = "✨ Tính năng AI Nâng cao",
            Width = 520,
            Height = 640,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.SingleBorderWindow,
            Background = Brushes.White
        };

        var root = new StackPanel { Margin = new Thickness(0) };

        // ── Header gradient ──
        var headerBorder = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromRgb(25, 118, 210),   // Blue 700
                Color.FromRgb(21, 101, 192),   // Blue 800
                90),
            Padding = new Thickness(28, 24, 28, 24)
        };
        var headerStack = new StackPanel();
        headerStack.Children.Add(new TextBlock
        {
            Text = "✨ AI Văn Bản Thông Minh",
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White
        });
        headerStack.Children.Add(new TextBlock
        {
            Text = "Tiết kiệm 70% thời gian soạn thảo văn bản hành chính",
            FontSize = 13,
            Foreground = Brushes.White,
            Opacity = 0.9,
            Margin = new Thickness(0, 6, 0, 0)
        });
        headerBorder.Child = headerStack;
        root.Children.Add(headerBorder);

        // ── Body content ──
        var bodyStack = new StackPanel { Margin = new Thickness(28, 20, 28, 0) };

        // Feature list
        var features = new[]
        {
            ("📝", "Soạn văn bản tự động", "Công văn, quyết định, báo cáo, tờ trình... chuẩn TT01/2011"),
            ("🔍", "Kiểm tra văn bản", "Phát hiện lỗi chính tả, văn phong, thể thức tự động"),
            ("📸", "Scan & OCR", "Trích xuất nội dung từ ảnh, PDF thành văn bản"),
            ("💡", "Tham mưu & Tóm tắt", "AI phân tích và đề xuất hướng xử lý văn bản đến"),
            ("📊", "Báo cáo định kỳ", "Tự động tổng hợp dữ liệu, tạo báo cáo nhanh chóng")
        };

        foreach (var (icon, title, desc) in features)
        {
            var featureRow = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
            featureRow.Children.Add(new TextBlock
            {
                Text = icon,
                FontSize = 18,
                Width = 30,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 1, 0, 0)
            });
            var textStack = new StackPanel();
            textStack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33))
            });
            textStack.Children.Add(new TextBlock
            {
                Text = desc,
                FontSize = 11.5,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                TextWrapping = TextWrapping.Wrap
            });
            featureRow.Children.Add(textStack);
            bodyStack.Children.Add(featureRow);
        }

        root.Children.Add(bodyStack);

        // ── Pricing card ──
        var priceBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(255, 243, 224)), // Orange 50
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(18, 14, 18, 14),
            Margin = new Thickness(28, 6, 28, 0)
        };
        var priceStack = new StackPanel();
        priceStack.Children.Add(new TextBlock
        {
            Text = "💰 Chỉ từ 99.000đ/tháng",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(230, 81, 0)),
            TextAlignment = TextAlignment.Center
        });
        priceStack.Children.Add(new TextBlock
        {
            Text = "Đầu tư nhỏ — Hiệu quả lớn • Hỗ trợ kỹ thuật 24/7",
            FontSize = 11.5,
            Foreground = new SolidColorBrush(Color.FromRgb(191, 54, 12)),
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 4, 0, 0)
        });
        priceBorder.Child = priceStack;
        root.Children.Add(priceBorder);

        // ── Contact info ──
        var contactBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(227, 242, 253)), // Blue 50
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(18, 12, 18, 12),
            Margin = new Thickness(28, 12, 28, 0)
        };
        var contactStack = new StackPanel();
        contactStack.Children.Add(new TextBlock
        {
            Text = "📞 Liên hệ đăng ký:",
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(21, 101, 192))
        });
        contactStack.Children.Add(new TextBlock
        {
            Text = "Zalo: Thắng Phan — 0907136029",
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 4, 0, 0)
        });
        contactBorder.Child = contactStack;
        root.Children.Add(contactBorder);

        // ── Action buttons (cố định ở dưới, không cuộn) ──
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 0)
        };

        var btnRegister = new Button
        {
            Content = new TextBlock { Text = "🌐 Đăng ký ngay", VerticalAlignment = VerticalAlignment.Center },
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            MinHeight = 40,
            Padding = new Thickness(24, 0, 24, 0),
            VerticalContentAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Color.FromRgb(25, 118, 210)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            Margin = new Thickness(0, 0, 10, 0)
        };
        btnRegister.Click += (s, e) =>
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://vanbanplus.giakiemso.com",
                    UseShellExecute = true
                });
            }
            catch { }
        };

        var btnSettings = new Button
        {
            Content = new TextBlock { Text = "⚙ Cài đặt API", VerticalAlignment = VerticalAlignment.Center },
            FontSize = 13,
            MinHeight = 40,
            Padding = new Thickness(20, 0, 20, 0),
            VerticalContentAlignment = VerticalAlignment.Center,
            Background = Brushes.White,
            Foreground = new SolidColorBrush(Color.FromRgb(25, 118, 210)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(25, 118, 210)),
            BorderThickness = new Thickness(1),
            Cursor = Cursors.Hand,
            Margin = new Thickness(0, 0, 10, 0)
        };
        btnSettings.Click += (s, e) =>
        {
            dialog.Close();
            var settingsDialog = new ApiSettingsDialog { Owner = owner };
            settingsDialog.ShowDialog();
        };

        var btnClose = new Button
        {
            Content = new TextBlock { Text = "Đóng", VerticalAlignment = VerticalAlignment.Center },
            FontSize = 13,
            MinHeight = 40,
            Padding = new Thickness(20, 0, 20, 0),
            VerticalContentAlignment = VerticalAlignment.Center,
            Background = Brushes.White,
            Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(189, 189, 189)),
            BorderThickness = new Thickness(1),
            Cursor = Cursors.Hand,
            IsCancel = true
        };
        btnClose.Click += (s, e) => dialog.Close();

        buttonPanel.Children.Add(btnRegister);
        buttonPanel.Children.Add(btnSettings);
        buttonPanel.Children.Add(btnClose);

        // ── Layout: scroll cho nội dung, button cố định dưới cùng ──
        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = root
        };

        var buttonFooter = new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(0, 16, 0, 18),
            Background = Brushes.White,
            Child = buttonPanel
        };

        var outerGrid = new Grid();
        outerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        outerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        Grid.SetRow(scroll, 0);
        Grid.SetRow(buttonFooter, 1);
        outerGrid.Children.Add(scroll);
        outerGrid.Children.Add(buttonFooter);

        dialog.Content = outerGrid;
        dialog.ShowDialog();
    }
}
