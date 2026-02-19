using System.Windows;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

/// <summary>
/// Dialog sao văn bản — Theo Điều 25-27, NĐ 30/2020/NĐ-CP
/// Hỗ trợ 3 hình thức: Sao y, Sao lục, Trích sao
/// </summary>
public partial class CopyDocumentDialog : Window
{
    private readonly DocumentService _documentService;
    private readonly Document _originalDocument;
    
    /// <summary>
    /// Bản sao đã tạo (null nếu hủy)
    /// </summary>
    public Document? CreatedCopy { get; private set; }
    
    private bool _cannotCopy = false;
    
    public CopyDocumentDialog(Document originalDocument, DocumentService documentService)
    {
        InitializeComponent();
        _documentService = documentService;
        _originalDocument = originalDocument;
        
        LoadOriginalInfo();
        LoadCopyTypes();
        
        if (_cannotCopy)
        {
            Loaded += (s, e) => Close();
        }
    }
    
    private void LoadOriginalInfo()
    {
        txtOriginalTitle.Text = _originalDocument.Title;
        txtOriginalNumber.Text = !string.IsNullOrEmpty(_originalDocument.Number) 
            ? _originalDocument.Number 
            : "(Chưa có số)";
        txtOriginalIssuer.Text = !string.IsNullOrEmpty(_originalDocument.Issuer) 
            ? _originalDocument.Issuer 
            : "(Chưa có)";
    }
    
    private void LoadCopyTypes()
    {
        var items = EnumDisplayHelper.GetCopyTypeItems();
        
        // Nếu VB gốc là bản sao y → chỉ cho phép sao lục (Điều 25 khoản 2)
        if (_originalDocument.CopyType == CopyType.SaoY)
        {
            items = items.Where(kv => kv.Key == CopyType.SaoLuc).ToList();
        }
        // Nếu VB gốc đã là sao lục hoặc trích sao → không cho sao tiếp
        else if (_originalDocument.CopyType == CopyType.SaoLuc || _originalDocument.CopyType == CopyType.TrichSao)
        {
            MessageBox.Show(
                "Không thể sao từ bản sao lục hoặc bản trích sao.\n" +
                "Sao lục chỉ thực hiện từ bản sao y (Điều 25 khoản 2, NĐ 30/2020).",
                "Không thể sao", MessageBoxButton.OK, MessageBoxImage.Warning);
            _cannotCopy = true;
            return;
        }
        
        cboCopyType.ItemsSource = items;
        if (items.Count > 0)
            cboCopyType.SelectedIndex = 0;
    }
    
    private void CboCopyType_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (cboCopyType.SelectedValue is not CopyType copyType) return;
        
        // Mô tả hình thức sao theo NĐ 30/2020
        txtCopyDescription.Text = copyType switch
        {
            CopyType.SaoY => "📌 Sao y: Sao đầy đủ, chính xác nội dung bản gốc/bản chính (Điều 25 khoản 1).",
            CopyType.SaoLuc => "📌 Sao lục: Sao đầy đủ, chính xác nội dung bản sao y (Điều 25 khoản 2).",
            CopyType.TrichSao => "📌 Trích sao: Sao chính xác phần nội dung cần trích (Điều 25 khoản 3).",
            _ => ""
        };
        
        // Hiện/ẩn ô nhập nội dung trích sao
        var isTrichSao = copyType == CopyType.TrichSao;
        lblExtractedContent.Visibility = isTrichSao ? Visibility.Visible : Visibility.Collapsed;
        txtExtractedContent.Visibility = isTrichSao ? Visibility.Visible : Visibility.Collapsed;
    }
    
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Validate
        if (cboCopyType.SelectedValue is not CopyType copyType)
        {
            MessageBox.Show("Vui lòng chọn hình thức sao.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (string.IsNullOrWhiteSpace(txtCopiedBy.Text))
        {
            MessageBox.Show("Vui lòng nhập họ tên người ký bản sao.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtCopiedBy.Focus();
            return;
        }
        
        if (string.IsNullOrWhiteSpace(txtRecipients.Text))
        {
            MessageBox.Show("Vui lòng nhập nơi nhận bản sao.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtRecipients.Focus();
            return;
        }
        
        if (copyType == CopyType.TrichSao && string.IsNullOrWhiteSpace(txtExtractedContent.Text))
        {
            MessageBox.Show("Trích sao phải nhập phần nội dung cần trích.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtExtractedContent.Focus();
            return;
        }
        
        try
        {
            // Lấy viết tắt cơ quan
            var config = _documentService.GetOrganizationConfig();
            var orgAbbr = !string.IsNullOrEmpty(config.Abbreviation) ? config.Abbreviation : "UBND";
            
            var recipients = txtRecipients.Text
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToArray();
            
            var extractedContent = copyType == CopyType.TrichSao ? txtExtractedContent.Text.Trim() : null;
            
            CreatedCopy = _documentService.CopyDocument(
                _originalDocument.Id,
                copyType,
                orgAbbr,
                txtCopiedBy.Text.Trim(),
                txtSigningTitle.Text.Trim(),
                recipients,
                extractedContent);
            
            MessageBox.Show(
                $"✅ Đã tạo bản sao thành công!\n\n" +
                $"📋 Hình thức: {copyType.GetDisplayName().ToUpper()}\n" +
                $"🔢 Ký hiệu: {CreatedCopy.CopySymbol}\n" +
                $"📄 VB gốc: {_originalDocument.Number}\n" +
                $"✍️ Người ký: {CreatedCopy.CopiedBy}\n" +
                $"📬 Nơi nhận: {string.Join(", ", recipients)}\n\n" +
                $"Bản sao có giá trị pháp lý như bản chính (Điều 26, NĐ 30/2020).",
                "Sao văn bản thành công",
                MessageBoxButton.OK, MessageBoxImage.Information);
            
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tạo bản sao: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
