using System.IO;
using System.Windows;
using System.Windows.Controls;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using Microsoft.Win32;

namespace AIVanBan.Desktop.Views;

public partial class DocumentEditDialog : Window
{
    public Document? Document { get; private set; }
    private DocumentService? _documentService;

    public DocumentEditDialog(Document? document = null, string? folderId = null, DocumentService? documentService = null)
    {
        InitializeComponent();
        _documentService = documentService;
        
        // Load loại văn bản (hiển thị tên tiếng Việt)
        cboType.DisplayMemberPath = "Value";
        cboType.SelectedValuePath = "Key";
        foreach (var item in EnumDisplayHelper.GetDocumentTypeItems())
        {
            cboType.Items.Add(item);
        }
        
        // Load hướng văn bản (hiển thị tên tiếng Việt)
        cboDirection.DisplayMemberPath = "Value";
        cboDirection.SelectedValuePath = "Key";
        foreach (var item in EnumDisplayHelper.GetDirectionItems())
        {
            cboDirection.Items.Add(item);
        }
        
        // Load mức độ khẩn — Điều 8 khoản 3b, NĐ 30/2020
        cboUrgency.DisplayMemberPath = "Value";
        cboUrgency.SelectedValuePath = "Key";
        foreach (var item in EnumDisplayHelper.GetUrgencyLevelItems())
        {
            cboUrgency.Items.Add(item);
        }
        
        // Load độ mật
        cboSecurity.DisplayMemberPath = "Value";
        cboSecurity.SelectedValuePath = "Key";
        foreach (var item in EnumDisplayHelper.GetSecurityLevelItems())
        {
            cboSecurity.Items.Add(item);
        }

        // Load thẩm quyền ký — Điều 13, NĐ 30/2020
        cboSigningAuthority.SelectedIndex = 0; // (Ký trực tiếp)

        if (document != null)
        {
            Document = document;
            Title = "Sửa văn bản";
            LoadDocument();
        }
        else
        {
            Document = new Document();
            if (!string.IsNullOrEmpty(folderId))
            {
                Document.FolderId = folderId;
            }
            cboType.SelectedIndex = 0;
            cboDirection.SelectedIndex = 0;
            cboUrgency.SelectedValue = UrgencyLevel.Thuong;
            cboSecurity.SelectedValue = SecurityLevel.Thuong;
        }
        
        UpdateDirectionPanels();
    }

    private void LoadDocument()
    {
        if (Document == null) return;

        txtNumber.Text = Document.Number;
        txtTitle.Text = Document.Title;
        txtIssuer.Text = Document.Issuer;
        txtSubject.Text = Document.Subject;
        txtRecipients.Text = string.Join(Environment.NewLine, Document.Recipients);
        txtBasedOn.Text = string.Join(Environment.NewLine, Document.BasedOn);
        txtContent.Text = Document.Content;
        txtFilePath.Text = Document.FilePath;
        txtSignedBy.Text = Document.SignedBy;
        txtSigningTitle.Text = Document.SigningTitle;
        
        // Thẩm quyền ký — Điều 13, NĐ 30/2020
        if (!string.IsNullOrEmpty(Document.SigningAuthority))
        {
            foreach (ComboBoxItem item in cboSigningAuthority.Items)
            {
                if (item.Tag?.ToString() == Document.SigningAuthority)
                {
                    cboSigningAuthority.SelectedItem = item;
                    break;
                }
            }
        }
        else
        {
            cboSigningAuthority.SelectedIndex = 0;
        }
        
        // Địa danh — Điều 8 khoản 4, NĐ 30/2020
        txtLocation.Text = Document.Location;
        
        // Lĩnh vực
        if (!string.IsNullOrEmpty(Document.Category))
            cboCategory.Text = Document.Category;
        
        // Tags
        if (Document.Tags != null && Document.Tags.Length > 0)
            txtTags.Text = string.Join(", ", Document.Tags);
        
        // Trạng thái hiệu lực
        foreach (ComboBoxItem item in cboStatus.Items)
        {
            if (item.Content?.ToString() == Document.Status)
            {
                cboStatus.SelectedItem = item;
                break;
            }
        }

        dpIssueDate.SelectedDate = Document.IssueDate;
        cboType.SelectedValue = Document.Type;
        cboDirection.SelectedValue = Document.Direction;
        cboUrgency.SelectedValue = Document.UrgencyLevel;
        cboSecurity.SelectedValue = Document.SecurityLevel;
        
        // VB đến fields
        if (Document.ArrivalNumber > 0)
            txtArrivalNumber.Text = Document.ArrivalNumber.ToString();
        dpArrivalDate.SelectedDate = Document.ArrivalDate;
        dpDueDate.SelectedDate = Document.DueDate;
        txtAssignedTo.Text = Document.AssignedTo;
        txtProcessingNotes.Text = Document.ProcessingNotes;
        
        UpdateDirectionPanels();
    }

    /// <summary>
    /// Hiển thị/ẩn panel VB đến khi đổi hướng — Điều 22, 24 NĐ 30/2020
    /// </summary>
    private void UpdateDirectionPanels()
    {
        var direction = cboDirection.SelectedValue is Direction d ? d : Direction.Di;
        
        if (direction == Direction.Den)
        {
            panelArrival.Visibility = Visibility.Visible;
            panelProcessing.Visibility = Visibility.Visible;
            txtProcessingNotes.Visibility = Visibility.Visible;
            
            // Auto-fill số đến nếu chưa có
            if (string.IsNullOrEmpty(txtArrivalNumber.Text) && _documentService != null)
            {
                var nextArrival = _documentService.GetNextArrivalNumber();
                txtArrivalNumber.Text = nextArrival.ToString();
            }
            if (!dpArrivalDate.SelectedDate.HasValue)
            {
                dpArrivalDate.SelectedDate = DateTime.Now;
            }
        }
        else
        {
            panelArrival.Visibility = Visibility.Collapsed;
            panelProcessing.Visibility = Visibility.Collapsed;
            txtProcessingNotes.Visibility = Visibility.Collapsed;
        }
    }

    private void CboDirection_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) UpdateDirectionPanels();
    }

    private void CboType_Changed(object sender, SelectionChangedEventArgs e)
    {
        // Có thể dùng để auto-suggest template
    }

    /// <summary>
    /// Tự động cấp số VB — Theo Điều 15, NĐ 30/2020/NĐ-CP
    /// </summary>
    private void AutoNumber_Click(object sender, RoutedEventArgs e)
    {
        if (_documentService == null)
        {
            MessageBox.Show("Chức năng cấp số tự động chưa sẵn sàng.", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var type = cboType.SelectedValue is DocumentType t ? t : DocumentType.CongVan;
        var direction = cboDirection.SelectedValue is Direction d ? d : Direction.Di;
        var isSecret = cboSecurity.SelectedValue is SecurityLevel s && s != SecurityLevel.Thuong;
        
        // Lấy viết tắt CQ từ cấu hình
        var config = _documentService.GetOrganizationConfig();
        var orgAbbr = !string.IsNullOrEmpty(config.Abbreviation) ? config.Abbreviation : "UBND";
        
        var symbol = _documentService.GenerateDocumentSymbol(type, orgAbbr, direction, isSecret: isSecret);
        txtNumber.Text = symbol;
        
        MessageBox.Show(
            $"✅ Đã cấp số: {symbol}\n\n" +
            $"📋 Loại VB: {type.GetDisplayName()} ({type.GetAbbreviation()})\n" +
            $"🏢 Cơ quan: {orgAbbr}\n" +
            $"📅 Năm: {DateTime.Now.Year}\n" +
            (isSecret ? "🔒 Hệ thống số mật riêng (Điều 15 khoản 2)" : ""),
            "Cấp số văn bản — Điều 15, NĐ 30/2020",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BrowseFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "All Files|*.*|Word|*.docx;*.doc|PDF|*.pdf|Excel|*.xlsx;*.xls",
            Title = "Chọn file văn bản"
        };

        if (dialog.ShowDialog() == true)
        {
            txtFilePath.Text = dialog.FileName;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Validation bắt buộc theo Điều 8, NĐ 30/2020
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(txtTitle.Text))
            errors.Add("• Tiêu đề văn bản (Trích yếu)");
        if (string.IsNullOrWhiteSpace(txtNumber.Text))
            errors.Add("• Số và ký hiệu văn bản");
        if (string.IsNullOrWhiteSpace(txtIssuer.Text))
            errors.Add("• Cơ quan ban hành");
        if (!dpIssueDate.SelectedDate.HasValue)
            errors.Add("• Ngày ban hành");
        
        if (errors.Count > 0)
        {
            MessageBox.Show(
                "Theo Điều 8, NĐ 30/2020, các thành phần sau là bắt buộc:\n\n" +
                string.Join("\n", errors) +
                "\n\nVui lòng điền đầy đủ để tiếp tục.",
                "Thiếu thông tin bắt buộc", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (Document == null) return;

        Document.Number = txtNumber.Text;
        Document.Title = txtTitle.Text;
        Document.Issuer = txtIssuer.Text;
        Document.Subject = txtSubject.Text;
        Document.Recipients = txtRecipients.Text
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
        Document.BasedOn = txtBasedOn.Text
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
        Document.Content = txtContent.Text;
        Document.FilePath = txtFilePath.Text;
        Document.SignedBy = txtSignedBy.Text;
        Document.SigningTitle = txtSigningTitle.Text;
        
        // Thẩm quyền ký — Điều 13, NĐ 30/2020
        if (cboSigningAuthority.SelectedItem is ComboBoxItem selectedAuth)
            Document.SigningAuthority = selectedAuth.Tag?.ToString() ?? "";
        
        // Địa danh — Điều 8 khoản 4, NĐ 30/2020
        Document.Location = txtLocation.Text;
        
        // Lĩnh vực
        Document.Category = cboCategory.Text;
        
        // Tags
        Document.Tags = txtTags.Text
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
        
        // Trạng thái hiệu lực
        if (cboStatus.SelectedItem is ComboBoxItem selectedStatus)
            Document.Status = selectedStatus.Content?.ToString() ?? "Còn hiệu lực";
        
        Document.IssueDate = dpIssueDate.SelectedDate ?? DateTime.Now;
        Document.Type = cboType.SelectedValue is DocumentType t ? t : DocumentType.CongVan;
        Document.Direction = cboDirection.SelectedValue is Direction d ? d : Direction.Di;
        
        // Mức độ khẩn, Độ mật — Điều 8 khoản 3b, NĐ 30/2020
        Document.UrgencyLevel = cboUrgency.SelectedValue is UrgencyLevel u ? u : UrgencyLevel.Thuong;
        Document.SecurityLevel = cboSecurity.SelectedValue is SecurityLevel s ? s : SecurityLevel.Thuong;
        
        // VB đến — Điều 22, 24, NĐ 30/2020
        if (Document.Direction == Direction.Den)
        {
            if (int.TryParse(txtArrivalNumber.Text, out var arrNum))
                Document.ArrivalNumber = arrNum;
            Document.ArrivalDate = dpArrivalDate.SelectedDate;
            Document.DueDate = dpDueDate.SelectedDate;
            Document.AssignedTo = txtAssignedTo.Text ?? "";
            Document.ProcessingNotes = txtProcessingNotes.Text ?? "";
        }

        if (!string.IsNullOrEmpty(Document.FilePath) && File.Exists(Document.FilePath))
        {
            var fileInfo = new FileInfo(Document.FilePath);
            Document.FileExtension = fileInfo.Extension;
            Document.FileSize = fileInfo.Length;
        }

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
