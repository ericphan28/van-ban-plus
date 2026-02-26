using System.Windows;
using AIVanBan.Core.Models;

namespace AIVanBan.Desktop.Views;

public partial class TemplateViewDialog : Window
{
    public TemplateViewDialog(DocumentTemplate template)
    {
        Title = $"Chi tiết mẫu: {template.Name}";
        Width = 700;
        Height = 600;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        
        LoadTemplate(template);
    }

    private void LoadTemplate(DocumentTemplate template)
    {
        var content = $@"📝 TÊN MẪU: {template.Name}

📋 LOẠI VĂN BẢN: {template.Type}

📄 MÔ TẢ:
{template.Description}

🔤 NỘI DUNG MẪU:
{template.TemplateContent}

📊 THỐNG KÊ:
- Số lần sử dụng: {template.UsageCount}
- Ngày tạo: {template.CreatedDate:dd/MM/yyyy HH:mm}
- Người tạo: {template.CreatedBy}
";

        var textBox = new System.Windows.Controls.TextBox
        {
            Text = content,
            TextWrapping = TextWrapping.Wrap,
            IsReadOnly = true,
            VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
            FontSize = 13,
            Padding = new Thickness(15),
            BorderThickness = new Thickness(0)
        };

        var closeButton = new System.Windows.Controls.Button
        {
            Content = "Đóng",
            MinWidth = 100,
            Height = 36,
            Padding = new Thickness(16, 0, 16, 0),
            VerticalContentAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 10, 15, 10),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        closeButton.Click += (s, e) => Close();

        // Dùng DockPanel để button luôn ở dưới cùng, TextBox fill phần còn lại
        var dock = new System.Windows.Controls.DockPanel { Margin = new Thickness(10) };
        System.Windows.Controls.DockPanel.SetDock(closeButton, System.Windows.Controls.Dock.Bottom);
        dock.Children.Add(closeButton);
        dock.Children.Add(textBox); // LastChildFill = true (default)

        Content = dock;
    }
}
