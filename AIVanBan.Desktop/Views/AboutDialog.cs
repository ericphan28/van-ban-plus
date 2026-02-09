using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AIVanBan.Desktop.Services;

namespace AIVanBan.Desktop.Views;

/// <summary>
/// Dialog hiển thị thông tin về phần mềm VanBanPlus.
/// </summary>
public class AboutDialog : Window
{
    public AboutDialog()
    {
        Title = "Giới thiệu VanBanPlus";
        Width = 520;
        Height = 580;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;

        Content = BuildContent();
    }

    private UIElement BuildContent()
    {
        // Main border with rounded corners and shadow
        var mainBorder = new Border
        {
            CornerRadius = new CornerRadius(12),
            Background = (Brush)FindResource("MaterialDesignPaper"),
            Margin = new Thickness(16),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 20,
                ShadowDepth = 4,
                Opacity = 0.3
            }
        };

        var mainStack = new StackPanel();
        mainBorder.Child = mainStack;

        // === Header with gradient ===
        var headerBorder = new Border
        {
            CornerRadius = new CornerRadius(12, 12, 0, 0),
            Background = new LinearGradientBrush(
                Color.FromRgb(33, 150, 243),   // Blue
                Color.FromRgb(30, 136, 229),   // Darker Blue
                45),
            Padding = new Thickness(0, 24, 0, 20)
        };

        var headerStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

        // App icon
        try
        {
            var iconPath = System.IO.Path.Combine(
                AppContext.BaseDirectory,
                "Assets", "app.ico");

            if (System.IO.File.Exists(iconPath))
            {
                var icon = new Image
                {
                    Source = new BitmapImage(new Uri(iconPath)),
                    Width = 72,
                    Height = 72,
                    Margin = new Thickness(0, 0, 0, 12)
                };
                headerStack.Children.Add(icon);
            }
        }
        catch { /* Skip icon if not found */ }

        // App name
        headerStack.Children.Add(new TextBlock
        {
            Text = "VanBanPlus",
            FontSize = 28,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        // Tagline
        headerStack.Children.Add(new TextBlock
        {
            Text = "Phần mềm quản lý văn bản thông minh",
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 4, 0, 0)
        });

        headerBorder.Child = headerStack;
        mainStack.Children.Add(headerBorder);

        // === Body content ===
        var bodyStack = new StackPanel { Margin = new Thickness(28, 20, 28, 8) };

        // Version info
        var version = AppUpdateService.GetCurrentVersion();
        AddInfoRow(bodyStack, "📦 Phiên bản:", $"v{version}");
        AddInfoRow(bodyStack, "🖥️ Nền tảng:", ".NET 9.0 / WPF");
        AddInfoRow(bodyStack, "👤 Tác giả:", "Eric Phan");
        AddInfoRow(bodyStack, "📅 Phát hành:", "02/2026");

        // Separator
        bodyStack.Children.Add(new Separator
        {
            Margin = new Thickness(0, 12, 0, 12),
            Background = (Brush)FindResource("MaterialDesignDivider")
        });

        // Description
        bodyStack.Children.Add(new TextBlock
        {
            Text = "📋 Giới thiệu",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8),
            Foreground = (Brush)FindResource("MaterialDesignBody")
        });

        bodyStack.Children.Add(new TextBlock
        {
            Text = "VanBanPlus là phần mềm hỗ trợ quản lý văn bản hành chính dành cho cán bộ, công chức cấp xã. " +
                   "Tích hợp AI (Gemini) giúp soạn thảo văn bản nhanh chóng, quản lý tài liệu, " +
                   "album ảnh công việc và biên bản cuộc họp.",
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12.5,
            LineHeight = 20,
            Foreground = (Brush)FindResource("MaterialDesignBodyLight")
        });

        // Features
        bodyStack.Children.Add(new TextBlock
        {
            Text = "✨ Tính năng chính",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 14, 0, 6),
            Foreground = (Brush)FindResource("MaterialDesignBody")
        });

        var features = new[]
        {
            "• Quản lý văn bản hành chính (Quyết định, Công văn, Báo cáo...)",
            "• Soạn thảo văn bản thông minh với AI Gemini",
            "• Quản lý album ảnh công việc theo cấu trúc",
            "• Quản lý biên bản cuộc họp, xuất Word",
            "• Tự động cập nhật phiên bản mới"
        };

        foreach (var feature in features)
        {
            bodyStack.Children.Add(new TextBlock
            {
                Text = feature,
                FontSize = 12,
                Margin = new Thickness(8, 2, 0, 2),
                Foreground = (Brush)FindResource("MaterialDesignBodyLight")
            });
        }

        mainStack.Children.Add(bodyStack);

        // === Footer buttons ===
        var footerBorder = new Border
        {
            Padding = new Thickness(28, 8, 28, 20)
        };

        var footerStack = new StackPanel();

        // GitHub link
        var linkPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 14)
        };

        var githubLink = new TextBlock
        {
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            Cursor = Cursors.Hand
        };
        var hyperlink = new Hyperlink(new Run("🔗 github.com/ericphan28/van-ban-plus"))
        {
            NavigateUri = new Uri("https://github.com/ericphan28/van-ban-plus")
        };
        hyperlink.RequestNavigate += (s, e) =>
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        };
        githubLink.Inlines.Add(hyperlink);
        linkPanel.Children.Add(githubLink);
        footerStack.Children.Add(linkPanel);

        // Buttons row
        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // Check update button
        var btnUpdate = new Button
        {
            Content = "Kiểm tra cập nhật",
            Padding = new Thickness(16, 6, 16, 6),
            Margin = new Thickness(0, 0, 8, 0),
            Style = (Style)FindResource("MaterialDesignOutlinedButton")
        };
        btnUpdate.Click += (s, e) => AppUpdateService.CheckForUpdateManual();
        btnPanel.Children.Add(btnUpdate);

        // Close button
        var btnClose = new Button
        {
            Content = "Đóng",
            Padding = new Thickness(24, 6, 24, 6),
            Style = (Style)FindResource("MaterialDesignFlatMidBgButton")
        };
        btnClose.Click += (s, e) => Close();
        btnPanel.Children.Add(btnClose);

        footerStack.Children.Add(btnPanel);

        // Copyright
        footerStack.Children.Add(new TextBlock
        {
            Text = "© 2026 VanBanPlus Software. All rights reserved.",
            FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 12, 0, 0),
            Foreground = (Brush)FindResource("MaterialDesignBodyLight")
        });

        footerBorder.Child = footerStack;
        mainStack.Children.Add(footerBorder);

        // Allow dragging the window
        mainBorder.MouseLeftButtonDown += (s, e) =>
        {
            if (e.ClickCount == 1) DragMove();
        };

        return mainBorder;
    }

    private void AddInfoRow(StackPanel parent, string label, string value)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 3, 0, 3)
        };

        row.Children.Add(new TextBlock
        {
            Text = label,
            FontSize = 13,
            FontWeight = FontWeights.Medium,
            Width = 120,
            Foreground = (Brush)FindResource("MaterialDesignBody")
        });

        row.Children.Add(new TextBlock
        {
            Text = value,
            FontSize = 13,
            Foreground = (Brush)FindResource("MaterialDesignBodyLight")
        });

        parent.Children.Add(row);
    }
}
