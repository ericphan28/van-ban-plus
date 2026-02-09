using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AIVanBan.Core.Models;

namespace AIVanBan.Desktop.Views;

public partial class DocumentViewDialog : Window
{
    private Document _document;
    
    public DocumentViewDialog(Document document)
    {
        InitializeComponent();
        _document = document;
        LoadDocument(document);
    }

    private void LoadDocument(Document doc)
    {
        Title = $"📄 {doc.Number} — {doc.Title}";
        
        // === HEADER ===
        txtHeaderNumber.Text = !string.IsNullOrEmpty(doc.Number) ? $"Số: {doc.Number}" : "Chưa có số";
        txtHeaderTitle.Text = doc.Title;
        txtBadgeType.Text = GetDocumentTypeName(doc.Type);
        txtBadgeDirection.Text = doc.Direction switch
        {
            Direction.Di => "📤 Văn bản đi",
            Direction.Den => "📥 Văn bản đến",
            Direction.NoiBo => "🔄 Nội bộ",
            _ => "—"
        };
        
        // === CARD 1: THÔNG TIN CHUNG ===
        txtNumber.Text = !string.IsNullOrEmpty(doc.Number) ? doc.Number : "Chưa có";
        txtIssueDate.Text = doc.IssueDate.ToString("dd/MM/yyyy");
        txtIssuer.Text = !string.IsNullOrEmpty(doc.Issuer) ? doc.Issuer : "—";
        txtCategory.Text = !string.IsNullOrEmpty(doc.Category) ? doc.Category : "—";
        
        // Workflow status
        txtWorkflowStatus.Text = doc.WorkflowStatus switch
        {
            DocumentStatus.Draft => "📝 Nháp",
            DocumentStatus.PendingApproval => "⏳ Chờ duyệt",
            DocumentStatus.Approved => "✅ Đã duyệt",
            DocumentStatus.Signed => "✍️ Đã ký",
            DocumentStatus.Published => "📢 Đã phát hành",
            DocumentStatus.Sent => "📬 Đã gửi",
            DocumentStatus.Archived => "📦 Lưu trữ",
            _ => "—"
        };
        
        txtStatus.Text = doc.Status ?? "Còn hiệu lực";
        if (doc.Status == "Còn hiệu lực")
            txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
        else if (doc.Status == "Hết hiệu lực")
            txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));
        
        txtCreatedBy.Text = !string.IsNullOrEmpty(doc.CreatedBy) ? doc.CreatedBy : "—";
        txtCreatedDate.Text = doc.CreatedDate.ToString("dd/MM/yyyy HH:mm");
        
        // Tags
        if (doc.Tags != null && doc.Tags.Length > 0)
        {
            lblTags.Visibility = Visibility.Visible;
            foreach (var tag in doc.Tags)
            {
                if (string.IsNullOrWhiteSpace(tag)) continue;
                var badge = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(0xE3, 0xF2, 0xFD)),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(8, 2, 8, 2),
                    Margin = new Thickness(0, 0, 4, 4)
                };
                badge.Child = new TextBlock
                {
                    Text = $"🏷️ {tag}",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0))
                };
                tagPanel.Children.Add(badge);
            }
        }
        else
        {
            lblTags.Visibility = Visibility.Collapsed;
        }
        
        // === CARD 2: TRÍCH YẾU ===
        if (!string.IsNullOrWhiteSpace(doc.Subject))
        {
            txtSubject.Text = doc.Subject;
            cardSubject.Visibility = Visibility.Visible;
        }
        
        // === CARD 3: CĂN CỨ PHÁP LÝ ===
        if (doc.BasedOn != null && doc.BasedOn.Length > 0)
        {
            var basedOnText = string.Join("\n", doc.BasedOn.Select((b, i) => $"  {i + 1}. {b}"));
            txtBasedOn.Text = basedOnText;
            cardBasedOn.Visibility = Visibility.Visible;
        }
        
        // === CARD 4: NƠI NHẬN ===
        if (doc.Recipients != null && doc.Recipients.Length > 0)
        {
            txtRecipients.Text = string.Join("\n", doc.Recipients.Select(r => $"  • {r}"));
            cardRecipients.Visibility = Visibility.Visible;
        }
        
        // === CARD 5: NỘI DUNG ===
        if (!string.IsNullOrWhiteSpace(doc.Content))
        {
            txtContent.Text = doc.Content;
            txtContentLength.Text = $"({doc.Content.Length:N0} ký tự)";
            cardContent.Visibility = Visibility.Visible;
        }
        
        // === CARD 6: TÓM TẮT AI ===
        if (!string.IsNullOrWhiteSpace(doc.Summary))
        {
            txtSummary.Text = doc.Summary;
            cardSummary.Visibility = Visibility.Visible;
        }
        
        // === CARD 7: FILE ĐÍNH KÈM ===
        if (!string.IsNullOrEmpty(doc.FilePath))
        {
            cardFile.Visibility = Visibility.Visible;
            
            var fileExists = File.Exists(doc.FilePath);
            var ext = Path.GetExtension(doc.FilePath).ToLower();
            var (icon, color) = ext switch
            {
                ".docx" or ".doc" => ("📄", "#1565C0"),
                ".pdf" => ("📕", "#C62828"),
                ".xlsx" or ".xls" => ("📊", "#2E7D32"),
                ".pptx" or ".ppt" => ("📙", "#E65100"),
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => ("🖼️", "#6A1B9A"),
                ".zip" or ".rar" or ".7z" => ("📦", "#795548"),
                _ => ("📎", "#616161")
            };
            
            var fileRow = new StackPanel { Orientation = Orientation.Horizontal };
            fileRow.Children.Add(new TextBlock
            {
                Text = icon,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            });
            
            var fileName = Path.GetFileName(doc.FilePath);
            fileRow.Children.Add(new TextBlock
            {
                Text = fileName,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                VerticalAlignment = VerticalAlignment.Center
            });
            
            if (fileExists)
            {
                try
                {
                    var fi = new FileInfo(doc.FilePath);
                    var sizeText = fi.Length < 1024 * 1024
                        ? $"  ({fi.Length / 1024} KB)"
                        : $"  ({fi.Length / (1024 * 1024.0):F1} MB)";
                    fileRow.Children.Add(new TextBlock
                    {
                        Text = sizeText,
                        FontSize = 11,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
                        VerticalAlignment = VerticalAlignment.Center
                    });
                }
                catch { }
                
                // Show file action buttons in footer
                btnOpenFile.Visibility = Visibility.Visible;
                btnOpenFolder.Visibility = Visibility.Visible;
            }
            else
            {
                fileRow.Children.Add(new TextBlock
                {
                    Text = "  ⚠️ File không tồn tại",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35)),
                    FontStyle = FontStyles.Italic,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
            
            fileRow.Children.Add(new TextBlock
            {
                Text = $"\n📂 {doc.FilePath}",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
                Margin = new Thickness(0, 4, 0, 0)
            });
            
            filePanel.Children.Add(fileRow);
        }
        
        // === FOOTER: Audit info ===
        var auditParts = new List<string>();
        auditParts.Add($"Tạo: {doc.CreatedDate:dd/MM/yyyy HH:mm}");
        if (doc.ModifiedDate.HasValue)
            auditParts.Add($"Sửa: {doc.ModifiedDate:dd/MM/yyyy HH:mm}");
        if (!string.IsNullOrEmpty(doc.ApprovedBy))
            auditParts.Add($"Duyệt: {doc.ApprovedBy}");
        txtAuditInfo.Text = string.Join("  •  ", auditParts);
    }
    
    private string GetDocumentTypeName(DocumentType type)
    {
        return type switch
        {
            DocumentType.CongVan => "Công văn",
            DocumentType.QuyetDinh => "Quyết định",
            DocumentType.BaoCao => "Báo cáo",
            DocumentType.ToTrinh => "Tờ trình",
            DocumentType.KeHoach => "Kế hoạch",
            DocumentType.ThongBao => "Thông báo",
            DocumentType.NghiQuyet => "Nghị quyết",
            DocumentType.ChiThi => "Chỉ thị",
            DocumentType.HuongDan => "Hướng dẫn",
            DocumentType.QuyDinh => "Quy định",
            DocumentType.Luat => "Luật",
            DocumentType.NghiDinh => "Nghị định",
            DocumentType.ThongTu => "Thông tư",
            DocumentType.Khac => "Khác",
            _ => type.ToString()
        };
    }
    
    private void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(_document.FilePath) && File.Exists(_document.FilePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _document.FilePath,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể mở file:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(_document.FilePath) && File.Exists(_document.FilePath))
            {
                Process.Start("explorer.exe", $"/select,\"{_document.FilePath}\"");
            }
        }
        catch { }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
