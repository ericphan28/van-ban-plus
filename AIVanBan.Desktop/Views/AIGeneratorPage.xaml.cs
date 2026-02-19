using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class AIGeneratorPage : Page
{
    private readonly DocumentService _documentService;

    public AIGeneratorPage(DocumentService documentService)
    {
        InitializeComponent();
        _documentService = documentService;
        LoadRecentDocuments();
    }
    
    /// <summary>
    /// Mở AI Compose Dialog để tạo văn bản mới
    /// </summary>
    private void NewDocument_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new AIComposeDialog(_documentService);
            dialog.Owner = Window.GetWindow(this);
            
            if (dialog.ShowDialog() == true && dialog.GeneratedDocument != null)
            {
                // Lưu document vào DB
                _documentService.AddDocument(dialog.GeneratedDocument);
                
                MessageBox.Show(
                    $"✅ Đã tạo và lưu văn bản:\n\n📋 {dialog.GeneratedDocument.Title}\n📁 Loại: {dialog.GeneratedDocument.Type.GetDisplayName()}\n🏢 Cơ quan: {dialog.GeneratedDocument.Issuer}",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                
                // Refresh danh sách
                LoadRecentDocuments();
            }
        }
        catch (Exception ex)
        {
            ShowErrorDialog(ex);
        }
    }

    /// <summary>
    /// Load danh sách văn bản AI đã tạo gần đây
    /// </summary>
    private void LoadRecentDocuments()
    {
        try
        {
            var allDocs = _documentService.GetAllDocuments();
            var aiDocs = allDocs
                .Where(d => d.Tags != null && d.Tags.Contains("AI Generated"))
                .OrderByDescending(d => d.CreatedDate)
                .Take(50)
                .Select(d => new DocumentListItem
                {
                    Id = d.Id,
                    Title = d.Title,
                    TypeDisplay = d.Type.GetDisplayName(),
                    Issuer = d.Issuer,
                    CreatedDate = d.CreatedDate
                })
                .ToList();

            dgRecentDocuments.ItemsSource = aiDocs;
            emptyState.Visibility = aiDocs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            dgRecentDocuments.Visibility = aiDocs.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading documents: {ex.Message}");
        }
    }

    /// <summary>
    /// Refresh danh sách
    /// </summary>
    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadRecentDocuments();
    }

    /// <summary>
    /// Double-click để xem chi tiết văn bản
    /// </summary>
    private void DocumentDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (dgRecentDocuments.SelectedItem is DocumentListItem item)
        {
            var doc = _documentService.GetDocument(item.Id);
            if (doc == null) return;

            // Hiển thị nội dung trong dialog
            var previewWindow = new Window
            {
                Title = $"📄 {doc.Title}",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Owner = Window.GetWindow(this)
            };

            var grid = new Grid { Margin = new Thickness(15) };
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var textBox = new TextBox
            {
                Text = doc.Content,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new System.Windows.Media.FontFamily("Times New Roman"),
                FontSize = 14,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(20)
            };
            Grid.SetRow(textBox, 0);
            grid.Children.Add(textBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var exportBtn = new Button
            {
                Content = "📝 Xuất Word",
                Padding = new Thickness(20, 8, 20, 8),
                Margin = new Thickness(0, 0, 10, 0)
            };
            exportBtn.Click += (s, args) =>
            {
                ExportDocumentToWord(doc);
            };

            var closeBtn = new Button
            {
                Content = "Đóng",
                Padding = new Thickness(20, 8, 20, 8),
                IsCancel = true
            };
            closeBtn.Click += (s, args) => previewWindow.Close();

            buttonPanel.Children.Add(exportBtn);
            buttonPanel.Children.Add(closeBtn);
            Grid.SetRow(buttonPanel, 1);
            grid.Children.Add(buttonPanel);

            previewWindow.Content = grid;
            previewWindow.ShowDialog();
        }
    }

    /// <summary>
    /// Xuất Word từ danh sách
    /// </summary>
    private void ExportWord_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string documentId)
        {
            var doc = _documentService.GetDocument(documentId);
            if (doc != null)
            {
                ExportDocumentToWord(doc);
            }
        }
    }

    /// <summary>
    /// Xuất văn bản ra file Word bằng WordExportService chuẩn TT01/2011
    /// </summary>
    private void ExportDocumentToWord(Document doc)
    {
        try
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Lưu file Word",
                FileName = $"{SanitizeFileName(doc.Title)}",
                DefaultExt = ".docx",
                Filter = "Word Document (*.docx)|*.docx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var wordService = new WordExportService();
                wordService.ExportDocument(doc, saveDialog.FileName);

                var result = MessageBox.Show(
                    $"✅ Đã xuất văn bản ra file:\n{saveDialog.FileName}\n\nBạn có muốn mở file không?",
                    "Xuất Word thành công",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xuất Word:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Xóa văn bản từ danh sách
    /// </summary>
    private void DeleteDocument_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string documentId)
        {
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa văn bản này?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _documentService.DeleteDocument(documentId);
                    LoadRecentDocuments();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private string SanitizeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "VanBan";
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }

    private void ShowErrorDialog(Exception ex)
    {
        var errorMessage = $"Lỗi khi mở AI Composer:\n\n{ex.Message}\n\nChi tiết:\n{ex}";
        
        var errorWindow = new Window
        {
            Title = "❌ Lỗi",
            Width = 600,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Owner = Window.GetWindow(this)
        };
        
        var grid = new Grid { Margin = new Thickness(15) };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        
        var errorTextBox = new TextBox
        {
            Text = errorMessage,
            IsReadOnly = true,
            TextWrapping = TextWrapping.Wrap,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10),
            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        Grid.SetRow(errorTextBox, 0);
        grid.Children.Add(errorTextBox);
        
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0)
        };
        
        var copyButton = new Button { Content = "📋 Copy Lỗi", Width = 100, Height = 35, Margin = new Thickness(0, 0, 10, 0) };
        copyButton.Click += (s, args) =>
        {
            try { Clipboard.SetText(errorMessage); } catch { }
        };
        
        var closeButton = new Button { Content = "Đóng", Width = 100, Height = 35, IsCancel = true };
        closeButton.Click += (s, args) => errorWindow.Close();
        
        buttonPanel.Children.Add(copyButton);
        buttonPanel.Children.Add(closeButton);
        Grid.SetRow(buttonPanel, 1);
        grid.Children.Add(buttonPanel);
        
        errorWindow.Content = grid;
        errorWindow.ShowDialog();
    }
}

/// <summary>
/// ViewModel cho danh sách văn bản AI
/// </summary>
public class DocumentListItem
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string TypeDisplay { get; set; } = "";
    public string Issuer { get; set; } = "";
    public DateTime CreatedDate { get; set; }
}
