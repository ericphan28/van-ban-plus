using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class DocumentViewDialog : Window
{
    private Document _document;
    private readonly DocumentService? _documentService;
    
    /// <summary>
    /// Cho biết văn bản đã được chỉnh sửa từ dialog này chưa.
    /// Caller kiểm tra property này sau ShowDialog() để refresh danh sách.
    /// </summary>
    public bool IsEdited { get; private set; }
    
    public DocumentViewDialog(Document document, DocumentService? documentService = null)
    {
        InitializeComponent();
        _document = document;
        _documentService = documentService;
        
        // Ẩn nút Sửa nếu không có DocumentService
        if (_documentService == null && FindName("btnEdit") is UIElement btnEdit)
            btnEdit.Visibility = Visibility.Collapsed;
        
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
        
        // Mức độ khẩn — Điều 8 khoản 3b, NĐ 30/2020
        if (doc.UrgencyLevel != UrgencyLevel.Thuong)
        {
            lblUrgency.Visibility = Visibility.Visible;
            txtUrgency.Text = $"⚡ {doc.UrgencyLevel.GetDisplayName()}";
        }
        
        // Độ mật
        if (doc.SecurityLevel != SecurityLevel.Thuong)
        {
            lblSecurity.Visibility = Visibility.Visible;
            txtSecurity.Text = $"🔒 {doc.SecurityLevel.GetDisplayName()}";
        }
        
        // VB đến — Điều 22, 24, NĐ 30/2020
        if (doc.Direction == Direction.Den)
        {
            if (doc.ArrivalNumber > 0)
            {
                lblArrival.Visibility = Visibility.Visible;
                txtArrivalNumber.Text = doc.ArrivalNumber.ToString();
                if (doc.ArrivalDate.HasValue)
                    txtArrivalNumber.Text += $" (Ngày đến: {doc.ArrivalDate:dd/MM/yyyy})";
            }
            
            if (doc.DueDate.HasValue)
            {
                lblDueDate.Visibility = Visibility.Visible;
                var isOverdue = doc.DueDate < DateTime.Now;
                txtDueDate.Text = doc.DueDate.Value.ToString("dd/MM/yyyy");
                if (isOverdue)
                {
                    txtDueDate.Text += " ⚠️ QUÁ HẠN";
                    txtDueDate.Foreground = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));
                }
                else
                {
                    txtDueDate.Foreground = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
                }
            }
            
            if (!string.IsNullOrWhiteSpace(doc.AssignedTo))
            {
                lblAssignedTo.Visibility = Visibility.Visible;
                txtAssignedTo.Text = doc.AssignedTo;
            }
        }
        
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
        
        // === BẢN SAO — Điều 25-27, NĐ 30/2020 ===
        if (doc.CopyType != CopyType.None)
        {
            lblCopyType.Visibility = Visibility.Visible;
            txtCopyInfo.Visibility = Visibility.Visible;
            var copyLabel = doc.CopyType.GetDisplayName().ToUpper();
            var copyText = $"📋 {copyLabel} — {doc.CopySymbol}";
            if (!string.IsNullOrEmpty(doc.CopiedBy))
            {
                var sigTitle = !string.IsNullOrEmpty(doc.CopySigningTitle) ? doc.CopySigningTitle + " " : "";
                copyText += $"\n✍️ {sigTitle}{doc.CopiedBy}";
            }
            if (doc.CopyDate.HasValue)
                copyText += $"\n📅 {doc.CopyDate.Value:dd/MM/yyyy}";
            txtCopyInfo.Text = copyText;
            txtCopyInfo.Foreground = new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0));
            txtCopyInfo.FontWeight = FontWeights.SemiBold;
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
    
    /// <summary>
    /// Lấy tên hiển thị loại VB — delegate sang EnumDisplayHelper (đủ 29 loại, NĐ 30/2020)
    /// </summary>
    private string GetDocumentTypeName(DocumentType type) => type.GetDisplayName();
    
    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (_documentService == null) return;
        
        try
        {
            var editDialog = new DocumentEditDialog(_document, null, _documentService)
            {
                Owner = this
            };
            
            if (editDialog.ShowDialog() == true)
            {
                // Reload document mới nhất từ DB
                var updated = _documentService.GetDocument(_document.Id);
                if (updated != null)
                {
                    _document = updated;
                    LoadDocument(updated);
                }
                IsEdited = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi mở chỉnh sửa:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
