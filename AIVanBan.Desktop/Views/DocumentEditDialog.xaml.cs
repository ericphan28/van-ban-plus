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
    /// <summary>True nếu user chọn "Lưu & Thêm mới" — caller sẽ mở lại dialog</summary>
    public bool SaveAndAddNew { get; private set; }
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

        // Load danh sách thư mục — cho phép chọn / đổi thư mục lưu
        LoadFolderCombo();

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

            // Set selected folder in combo (if any)
            cboFolder.SelectedValue = Document.FolderId ?? string.Empty;

            // ⭐ FORM MẪU SẴN — Auto-prefill defaults khi tạo VB mới
            // (Trước đây user phải nhập lại toàn bộ trường — tester feedback v1.0.14)
            ApplyNewDocumentDefaults();
        }
        
        UpdateDirectionPanels();
    }

    /// <summary>
    /// Load tất cả thư mục vào ComboBox để user chọn / đổi thư mục lưu.
    /// </summary>
    private void LoadFolderCombo()
    {
        try
        {
            var items = new List<Folder>
            {
                new Folder { Id = string.Empty, Name = "(Không thuộc thư mục nào)" }
            };

            if (_documentService != null)
            {
                var allFolders = _documentService.GetAllFolders()
                    .OrderBy(f => f.SortOrder)
                    .ThenBy(f => f.Name)
                    .ToList();

                // Build hierarchical display (thêm "→" indent cho thư mục con)
                var rootFolders = allFolders.Where(f => string.IsNullOrEmpty(f.ParentId)).ToList();
                foreach (var root in rootFolders)
                {
                    items.Add(new Folder { Id = root.Id, Name = $"📁 {root.Name}" });
                    AppendChildFolders(root.Id, allFolders, items, 1);
                }

                // Folders không có parent hợp lệ (mồ côi)
                var rootIds = rootFolders.Select(f => f.Id).ToHashSet();
                var orphans = allFolders.Where(f => !string.IsNullOrEmpty(f.ParentId) && !allFolders.Any(p => p.Id == f.ParentId)).ToList();
                foreach (var o in orphans)
                {
                    items.Add(new Folder { Id = o.Id, Name = $"📁 {o.Name}" });
                }
            }

            cboFolder.ItemsSource = items;
        }
        catch
        {
            // không chặn dialog nếu lỗi load thư mục
        }
    }

    private void AppendChildFolders(string parentId, List<Folder> allFolders, List<Folder> output, int level)
    {
        var children = allFolders.Where(f => f.ParentId == parentId).ToList();
        var indent = new string(' ', level * 4);
        foreach (var c in children)
        {
            output.Add(new Folder { Id = c.Id, Name = $"{indent}↳ {c.Name}" });
            AppendChildFolders(c.Id, allFolders, output, level + 1);
        }
    }

    /// <summary>
    /// Auto-prefill các trường thường dùng khi tạo VB mới:
    /// - Cơ quan ban hành (Issuer) ← OrganizationConfig.Name
    /// - Địa danh (Location) ← VB gần nhất hoặc settings
    /// - Người ký, chức danh ký ← VB gần nhất hoặc UserFullName
    /// - Số VB ← tự sinh theo loại VB mặc định + cơ quan
    /// - Ngày ban hành ← hôm nay
    /// </summary>
    private void ApplyNewDocumentDefaults()
    {
        try
        {
            // 1) Lấy cấu hình cơ quan
            string orgName = "";
            string orgAbbr = "UBND";
            try
            {
                if (_documentService != null)
                {
                    var org = _documentService.GetOrganizationConfig();
                    if (!string.IsNullOrWhiteSpace(org.Name)) orgName = org.Name;
                    if (!string.IsNullOrWhiteSpace(org.Abbreviation)) orgAbbr = org.Abbreviation;
                }
            }
            catch { /* ignore */ }

            // 2) Lấy VB gần nhất cùng hướng (Đi) để mượn Location, SignedBy, SigningTitle
            Document? lastDoc = null;
            try
            {
                if (_documentService != null)
                {
                    lastDoc = _documentService.GetAllDocuments()
                        .Where(d => !d.IsDeleted && d.Direction == Direction.Di)
                        .OrderByDescending(d => d.CreatedDate)
                        .FirstOrDefault();
                }
            }
            catch { /* ignore */ }

            // 3) Lấy thông tin user từ AppSettings (fallback signer)
            string userFullName = "";
            string defaultSigner = "";
            string defaultSigningTitle = "";
            string defaultLocation = "";
            try
            {
                var settings = AIVanBan.Core.Services.AppSettingsService.Load();
                userFullName = settings.UserFullName ?? "";
                defaultSigner = AIVanBan.Core.Services.AppSettingsService.GetRawSettingValue("Org.DefaultSigner") ?? "";
                defaultSigningTitle = AIVanBan.Core.Services.AppSettingsService.GetRawSettingValue("Org.DefaultSigningTitle") ?? "";
                defaultLocation = AIVanBan.Core.Services.AppSettingsService.GetRawSettingValue("Org.DefaultLocation") ?? "";
            }
            catch { /* ignore */ }

            // 4) Điền các giá trị mặc định
            // ƯU TIÊN: Cấu hình cơ quan (user vừa thiết lập) → VB gần nhất (fallback)
            // (Tránh trường hợp user đổi cơ quan trong Settings nhưng form vẫn hiện cơ quan cũ từ VB trước)
            txtIssuer.Text = !string.IsNullOrWhiteSpace(orgName)
                ? orgName
                : (lastDoc?.Issuer ?? "");

            txtLocation.Text = !string.IsNullOrWhiteSpace(defaultLocation)
                ? defaultLocation
                : (lastDoc?.Location ?? "");

            txtSignedBy.Text = !string.IsNullOrWhiteSpace(defaultSigner)
                ? defaultSigner
                : (!string.IsNullOrWhiteSpace(lastDoc?.SignedBy) ? lastDoc!.SignedBy : userFullName);

            txtSigningTitle.Text = !string.IsNullOrWhiteSpace(defaultSigningTitle)
                ? defaultSigningTitle
                : (lastDoc?.SigningTitle ?? "");

            dpIssueDate.SelectedDate = DateTime.Now;

            // 5) Auto-cấp số VB cho loại VB mặc định (Công văn / loại đầu tiên)
            try
            {
                if (_documentService != null && cboType.SelectedValue is DocumentType defType)
                {
                    var symbol = _documentService.GenerateDocumentSymbol(
                        defType, orgAbbr, Direction.Di, isSecret: false);
                    txtNumber.Text = symbol;
                }
            }
            catch { /* không chặn nếu lỗi cấp số */ }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ ApplyNewDocumentDefaults error: {ex.Message}");
        }
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

        // Thư mục lưu — chọn đúng FolderId hiện tại (hoặc rỗng = không thuộc thư mục)
        cboFolder.SelectedValue = Document.FolderId ?? string.Empty;
        
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
        // Khi tạo VB mới và Số VB còn rỗng/auto → tự động cập nhật ký hiệu theo loại mới
        // (chỉ áp dụng cho doc mới — Document.Id chưa có trong DB)
        if (!IsLoaded || Document == null || _documentService == null) return;
        if (!string.IsNullOrEmpty(Document.Number) && Document.Number != txtNumber.Text)
        {
            // Đang sửa VB đã lưu — không tự đổi số
            return;
        }
        try
        {
            if (cboType.SelectedValue is DocumentType newType)
            {
                var org = _documentService.GetOrganizationConfig();
                var orgAbbr = !string.IsNullOrWhiteSpace(org.Abbreviation) ? org.Abbreviation : "UBND";
                var direction = cboDirection.SelectedValue is Direction d ? d : Direction.Di;
                var isSecret = cboSecurity.SelectedValue is SecurityLevel s && s != SecurityLevel.Thuong;
                txtNumber.Text = _documentService.GenerateDocumentSymbol(newType, orgAbbr, direction, isSecret: isSecret);
            }
        }
        catch { /* ignore */ }
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

        // Thư mục lưu — cho phép chuyển vào thư mục mong muốn ngay khi soạn/sửa
        if (cboFolder.SelectedValue is string selectedFolderId)
        {
            Document.FolderId = selectedFolderId ?? string.Empty;
        }
        
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

    /// <summary>Lưu văn bản hiện tại rồi caller sẽ mở form thêm mới</summary>
    private void SaveAndAddNew_Click(object sender, RoutedEventArgs e)
    {
        SaveAndAddNew = true;
        Save_Click(sender, e); // Reuse validation + save logic
    }

    /// <summary>
    /// Pre-fill defaults từ VB trước (dùng cho "Lưu & Thêm mới").
    /// Giữ lại CQ ban hành, địa danh, hướng VB, loại VB và set ngày = hôm nay.
    /// </summary>
    public void PreFillDefaults(string issuer, string location, Direction direction, DocumentType type)
    {
        txtIssuer.Text = issuer ?? "";
        txtLocation.Text = location ?? "";
        dpIssueDate.SelectedDate = DateTime.Now;
        cboDirection.SelectedValue = direction;
        cboType.SelectedValue = type;
        UpdateDirectionPanels();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
