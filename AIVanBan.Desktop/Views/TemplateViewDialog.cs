using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AIVanBan.Core.Models;
using MaterialDesignThemes.Wpf;

namespace AIVanBan.Desktop.Views;

/// <summary>
/// Dialog xem chi tiết mẫu văn bản — hiển thị trực quan, font Times New Roman, 
/// có nút "Sử dụng mẫu này" để chuyển thẳng sang AI Soạn thảo.
/// </summary>
public partial class TemplateViewDialog : Window
{
    private readonly DocumentTemplate _template;

    /// <summary>
    /// true nếu user chọn "Sử dụng mẫu này"
    /// </summary>
    public bool WantsUseTemplate { get; private set; }

    public TemplateViewDialog(DocumentTemplate template)
    {
        _template = template;
        Title = $"Chi tiết mẫu: {template.Name}";
        Width = 780;
        Height = 650;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ResizeMode = ResizeMode.CanResizeWithGrip;
        Background = new SolidColorBrush(Color.FromRgb(250, 250, 252));

        BuildUI();
    }

    private void BuildUI()
    {
        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // Header
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // Buttons

        // ===== HEADER =====
        var header = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(21, 101, 192)),
            Padding = new Thickness(20, 14, 20, 14)
        };
        var headerStack = new StackPanel { Orientation = Orientation.Horizontal };
        headerStack.Children.Add(new PackIcon
        {
            Kind = PackIconKind.FileDocumentOutline,
            Width = 24, Height = 24,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        });
        headerStack.Children.Add(new TextBlock
        {
            Text = _template.Name,
            FontSize = 17,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        });
        header.Child = headerStack;
        Grid.SetRow(header, 0);
        root.Children.Add(header);

        // ===== CONTENT AREA =====
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Padding = new Thickness(24, 16, 24, 16)
        };
        var contentStack = new StackPanel();

        // --- Info badges ---
        var badgePanel = new WrapPanel { Margin = new Thickness(0, 0, 0, 16) };
        AddInfoBadge(badgePanel, $"📋 {_template.Type}", "#E3F2FD", "#1565C0");
        if (!string.IsNullOrWhiteSpace(_template.Category))
            AddInfoBadge(badgePanel, $"📂 {_template.Category}", "#F3E5F5", "#7B1FA2");
        AddInfoBadge(badgePanel, $"📊 Đã dùng {_template.UsageCount} lần", "#E8F5E9", "#2E7D32");
        AddInfoBadge(badgePanel, $"📅 {_template.CreatedDate:dd/MM/yyyy}", "#FFF3E0", "#E65100");
        if (!string.IsNullOrWhiteSpace(_template.CreatedBy))
            AddInfoBadge(badgePanel, $"👤 {_template.CreatedBy}", "#ECEFF1", "#37474F");
        contentStack.Children.Add(badgePanel);

        // --- Mô tả ---
        if (!string.IsNullOrWhiteSpace(_template.Description))
        {
            contentStack.Children.Add(CreateSectionHeader("📄 MÔ TẢ"));
            contentStack.Children.Add(new TextBlock
            {
                Text = _template.Description,
                FontFamily = new FontFamily("Times New Roman"),
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(66, 66, 66)),
                Margin = new Thickness(0, 0, 0, 16),
                LineHeight = 22
            });
        }

        // --- Nội dung mẫu (phần chính) ---
        contentStack.Children.Add(CreateSectionHeader("🔤 NỘI DUNG MẪU"));
        var templateBorder = new Border
        {
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(20, 16, 20, 16),
            Margin = new Thickness(0, 0, 0, 12)
        };
        templateBorder.Child = new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(_template.TemplateContent)
                ? "(Chưa có nội dung mẫu)"
                : _template.TemplateContent,
            FontFamily = new FontFamily("Times New Roman"),
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            Foreground = new SolidColorBrush(
                string.IsNullOrWhiteSpace(_template.TemplateContent)
                    ? Color.FromRgb(158, 158, 158)
                    : Color.FromRgb(33, 33, 33)),
            LineHeight = 24
        };
        contentStack.Children.Add(templateBorder);

        // --- Tags ---
        if (_template.Tags != null && _template.Tags.Length > 0)
        {
            contentStack.Children.Add(CreateSectionHeader("🏷️ TAGS"));
            var tagPanel = new WrapPanel { Margin = new Thickness(0, 0, 0, 12) };
            foreach (var tag in _template.Tags)
            {
                var tagBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(232, 234, 246)),
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(10, 4, 10, 4),
                    Margin = new Thickness(0, 0, 6, 6)
                };
                tagBorder.Child = new TextBlock
                {
                    Text = $"#{tag}",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(63, 81, 181))
                };
                tagPanel.Children.Add(tagBorder);
            }
            contentStack.Children.Add(tagPanel);
        }

        scrollViewer.Content = contentStack;
        Grid.SetRow(scrollViewer, 1);
        root.Children.Add(scrollViewer);

        // ===== BUTTON BAR =====
        var buttonBar = new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(20, 12, 20, 12),
            Background = Brushes.White
        };
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        // Nút Đóng
        var closeBtn = new Button
        {
            Content = "Đóng",
            MinWidth = 100,
            Height = 38,
            Padding = new Thickness(20, 0, 20, 0),
            Margin = new Thickness(0, 0, 10, 0),
            Style = (Style)FindResource("MaterialDesignOutlinedButton")
        };
        closeBtn.Click += (s, e) => Close();
        buttonPanel.Children.Add(closeBtn);

        // Nút "Sử dụng mẫu này" — nổi bật
        var useBtn = new Button
        {
            Height = 38,
            Padding = new Thickness(20, 0, 20, 0),
            Style = (Style)FindResource("MaterialDesignRaisedButton"),
            Background = new SolidColorBrush(Color.FromRgb(46, 125, 50)),
            Foreground = Brushes.White,
            ToolTip = "Mở AI Soạn thảo với mẫu này"
        };
        var useBtnContent = new StackPanel { Orientation = Orientation.Horizontal };
        useBtnContent.Children.Add(new PackIcon
        {
            Kind = PackIconKind.FileEditOutline,
            Width = 18, Height = 18,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        });
        useBtnContent.Children.Add(new TextBlock
        {
            Text = "Sử dụng mẫu này",
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeights.SemiBold,
            FontSize = 14
        });
        useBtn.Content = useBtnContent;
        useBtn.Click += (s, e) =>
        {
            WantsUseTemplate = true;
            DialogResult = true;
            Close();
        };
        buttonPanel.Children.Add(useBtn);

        buttonBar.Child = buttonPanel;
        Grid.SetRow(buttonBar, 2);
        root.Children.Add(buttonBar);

        Content = root;
    }

    private void AddInfoBadge(WrapPanel panel, string text, string bgHex, string fgHex)
    {
        var bg = (Color)ColorConverter.ConvertFromString(bgHex);
        var fg = (Color)ColorConverter.ConvertFromString(fgHex);
        var badge = new Border
        {
            Background = new SolidColorBrush(bg),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10, 5, 10, 5),
            Margin = new Thickness(0, 0, 8, 8)
        };
        badge.Child = new TextBlock
        {
            Text = text,
            FontSize = 12,
            FontWeight = FontWeights.Medium,
            Foreground = new SolidColorBrush(fg)
        };
        panel.Children.Add(badge);
    }

    private TextBlock CreateSectionHeader(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(55, 71, 79)),
            Margin = new Thickness(0, 0, 0, 8)
        };
    }
}
