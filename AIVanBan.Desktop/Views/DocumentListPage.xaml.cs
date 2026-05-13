using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Text;
using System.Globalization;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using MaterialDesignThemes.Wpf;

namespace AIVanBan.Desktop.Views;

public class FolderNode
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PackIconKind IconKind { get; set; } = PackIconKind.Folder;
    public string IconColor { get; set; } = "#FFA726";
    public int DocumentCount { get; set; }
    public ObservableCollection<FolderNode> Children { get; set; } = new();
}

public class DocumentViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    /// <summary>Khóa sort numeric cho cột Số VB (tách phần số trước dấu "/"). VD: "15/CV-UBND" → 15</summary>
    public long NumberSortKey { get; set; } = 0;
    public string Title { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public string TypeText { get; set; } = string.Empty;
    public string TypeColor { get; set; } = "#999999";
    public DateTime IssueDate { get; set; }
    public string Issuer { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int AttachmentCount { get; set; } = 0;
    public string AttachmentText { get; set; } = string.Empty;
    public bool HasAttachments => AttachmentCount > 0;
    public bool HasNoAttachments => AttachmentCount == 0;
    public string CopyIndicator { get; set; } = string.Empty; // Bản sao indicator: "📋 SAO Y (05/SY-UBND)"
    public bool IsCopy => !string.IsNullOrEmpty(CopyIndicator);
    public Direction Direction { get; set; }
    public string DirectionText { get; set; } = string.Empty;
    public string DirectionColor { get; set; } = "#999";
    
    // Deadline status — Theo dõi hạn xử lý (Điều 24, NĐ 30/2020)
    public DateTime? DueDate { get; set; }
    public string DeadlineText { get; set; } = string.Empty; // "⚠ Quá hạn" / "⏰ Còn 2 ngày" / ""
    public string DeadlineColor { get; set; } = "#999";
    public string RowBackground { get; set; } = "Transparent"; // Tô màu dòng
    
    // Thùng rác — hiện nút Phục hồi, ẩn nút Sửa/Word
    public Visibility RestoreVisibility { get; set; } = Visibility.Collapsed;
    public Visibility EditVisibility { get; set; } = Visibility.Visible;
    
    // Trạng thái workflow — hiển thị badge trong DataGrid
    public DocumentStatus WorkflowStatus { get; set; } = DocumentStatus.Draft;
    public string StatusText { get; set; } = "📝 Đang soạn";
    public string StatusColor { get; set; } = "#757575";
    public string StatusTooltip { get; set; } = string.Empty;
    
    // Sổ theo dõi cá nhân
    public bool IsStarred { get; set; } = false;
    public string StarIcon { get; set; } = "☆";
    public string StarColor { get; set; } = "#BDBDBD";
    public PersonalStatus MyStatus { get; set; } = PersonalStatus.ChuaXuLy;
    public string MyStatusText { get; set; } = "Chưa XL";
    public string MyStatusColor { get; set; } = "#9E9E9E";
    public DateTime? PersonalDeadline { get; set; }
    public int NoteCount { get; set; } = 0;
    
    /// <summary>
    /// Trích phần số đầu của ký hiệu VB ("123/CV-UBND" → 123) để sort numeric.
    /// Trả về 0 nếu không parse được.
    /// </summary>
    public static long ParseNumberSortKey(string? number)
    {
        if (string.IsNullOrWhiteSpace(number)) return 0;
        var head = number.Split('/', '-', ' ').FirstOrDefault()?.Trim();
        if (string.IsNullOrEmpty(head)) return 0;
        // Lấy các ký tự số ở đầu chuỗi
        int i = 0;
        while (i < head.Length && char.IsDigit(head[i])) i++;
        if (i == 0) return 0;
        return long.TryParse(head.Substring(0, i), out var n) ? n : 0;
    }

    public static DocumentViewModel FromDocument(Document doc, DocumentService? service = null)
    {
        var vm = new DocumentViewModel
        {
            Id = doc.Id,
            Number = doc.Number,
            NumberSortKey = ParseNumberSortKey(doc.Number),
            Title = doc.Title,
            Type = doc.Type,
            IssueDate = doc.IssueDate,
            Issuer = doc.Issuer,
            Subject = doc.Subject,
            Content = doc.Content,
            Direction = doc.Direction
        };
        
        // Badge hướng VB
        (vm.DirectionText, vm.DirectionColor) = doc.Direction switch
        {
            Direction.Di => ("Đi", "#1B5E20"),
            Direction.Den => ("Đến", "#E65100"),
            Direction.NoiBo => ("NB", "#1565C0"),
            _ => ("?", "#999")
        };
        
        // Get attachment count
        if (service != null && doc.AttachmentIds != null && doc.AttachmentIds.Length > 0)
        {
            try
            {
                var attachments = service.GetAttachmentsByDocument(doc.Id);
                vm.AttachmentCount = attachments.Count;
                vm.AttachmentText = vm.AttachmentCount > 0 ? $"📎 {vm.AttachmentCount}" : "";
            }
            catch
            {
                vm.AttachmentCount = doc.AttachmentIds.Length;
                vm.AttachmentText = vm.AttachmentCount > 0 ? $"📎 {vm.AttachmentCount}" : "";
            }
        }
        
        // Set type text and color
        // Tên hiển thị: delegate sang EnumDisplayHelper (đủ 29 loại VB, NĐ 30/2020)
        vm.TypeText = doc.Type.GetDisplayName();
        
        // Màu badge theo loại VB
        vm.TypeColor = doc.Type switch
        {
            // VBQPPL
            DocumentType.Luat => "#D32F2F",
            DocumentType.NghiDinh => "#C2185B",
            DocumentType.ThongTu => "#7B1FA2",
            // VB hành chính — Điều 7, NĐ 30/2020
            DocumentType.NghiQuyet => "#F44336",
            DocumentType.QuyetDinh => "#4CAF50",
            DocumentType.ChiThi => "#E91E63",
            DocumentType.QuyChE => "#009688",
            DocumentType.QuyDinh => "#009688",
            DocumentType.ThongCao => "#795548",
            DocumentType.ThongBao => "#FF5722",
            DocumentType.HuongDan => "#3F51B5",
            DocumentType.ChuongTrinh => "#00897B",
            DocumentType.KeHoach => "#00BCD4",
            DocumentType.PhuongAn => "#26A69A",
            DocumentType.DeAn => "#5C6BC0",
            DocumentType.DuAn => "#42A5F5",
            DocumentType.BaoCao => "#FF9800",
            DocumentType.BienBan => "#8D6E63",
            DocumentType.ToTrinh => "#9C27B0",
            DocumentType.HopDong => "#607D8B",
            DocumentType.CongVan => "#2196F3",
            DocumentType.CongDien => "#EF5350",
            DocumentType.BanGhiNho => "#78909C",
            DocumentType.BanThoaThuan => "#66BB6A",
            DocumentType.GiayUyQuyen => "#AB47BC",
            DocumentType.GiayMoi => "#29B6F6",
            DocumentType.GiayGioiThieu => "#26C6DA",
            DocumentType.GiayNghiPhep => "#FFA726",
            DocumentType.PhieuGui => "#BDBDBD",
            DocumentType.PhieuChuyen => "#90A4AE",
            DocumentType.PhieuBao => "#A1887F",
            DocumentType.ThuCong => "#7E57C2",
            DocumentType.Khac => "#757575",
            _ => "#999999"
        };
        
        // Hiển thị chỉ báo bản sao — Điều 25, NĐ 30/2020
        if (doc.CopyType != CopyType.None)
        {
            vm.CopyIndicator = $"📋 {doc.CopyType.GetDisplayName().ToUpper()} ({doc.CopySymbol})";
        }
        
        // Trạng thái workflow — hiển thị badge
        vm.WorkflowStatus = doc.WorkflowStatus;
        vm.StatusText = doc.WorkflowStatus.GetDisplayName();
        vm.StatusColor = doc.WorkflowStatus switch
        {
            DocumentStatus.Draft => "#757575",           // Xám
            DocumentStatus.PendingApproval => "#E65100", // Cam
            DocumentStatus.Approved => "#1565C0",        // Xanh dương
            DocumentStatus.Signed => "#2E7D32",          // Xanh lá
            DocumentStatus.Published => "#6A1B9A",       // Tím
            DocumentStatus.Sent => "#00838F",            // Teal
            DocumentStatus.Archived => "#37474F",        // Xám đậm
            _ => "#757575"
        };
        // Context-aware tooltip: hiển thị trạng thái hiện tại + gợi ý bước tiếp theo
        var nextStatus = doc.WorkflowStatus.GetNextStatus();
        var nextHint = nextStatus.HasValue ? $"\n💡 Click để chuyển → {nextStatus.Value.GetDisplayName()}" : "\n✅ Đã hoàn thành quy trình";
        vm.StatusTooltip = doc.WorkflowStatus switch
        {
            DocumentStatus.Draft => $"📝 Đang soạn — Tôi đang soạn thảo VB này{nextHint}",
            DocumentStatus.PendingApproval => $"📤 Đã trình sếp — Đã đưa lãnh đạo xem/ký{nextHint}",
            DocumentStatus.Approved => $"✅ Sếp đã duyệt — Lãnh đạo OK, chờ ký chính thức{nextHint}",
            DocumentStatus.Signed => $"✍️ Đã ký — Chờ phát hành, đăng ký số{nextHint}",
            DocumentStatus.Published => $"📢 Đã phát hành — Có số văn bản chính thức{nextHint}",
            DocumentStatus.Sent => $"📨 Đã gửi — Đã gửi đến nơi nhận{nextHint}",
            DocumentStatus.Archived => $"🗄️ Xong — Đã hoàn thành, lưu hồ sơ{nextHint}",
            _ => ""
        };
        
        // Cảnh báo hạn xử lý — Điều 24, NĐ 30/2020
        vm.DueDate = doc.DueDate;
        vm.PersonalDeadline = doc.PersonalDeadline;
        
        // Sổ theo dõi cá nhân
        vm.IsStarred = doc.IsStarred;
        vm.StarIcon = doc.IsStarred ? "★" : "☆";
        vm.StarColor = doc.IsStarred ? "#FFC107" : "#BDBDBD";
        vm.MyStatus = doc.MyStatus;
        (vm.MyStatusText, vm.MyStatusColor) = doc.MyStatus switch
        {
            PersonalStatus.ChuaXuLy => ("Chưa XL", "#9E9E9E"),
            PersonalStatus.DangXuLy => ("Đang XL", "#FB8C00"),
            PersonalStatus.DaXuLy => ("Đã XL", "#43A047"),
            PersonalStatus.ChuyenTiep => ("Chuyển", "#1E88E5"),
            _ => ("Chưa XL", "#9E9E9E")
        };
        vm.NoteCount = doc.Notes?.Count ?? 0;
        if (doc.DueDate.HasValue && doc.Direction == Direction.Den
            && doc.WorkflowStatus != DocumentStatus.Archived
            && doc.WorkflowStatus != DocumentStatus.Published)
        {
            var daysLeft = (doc.DueDate.Value.Date - DateTime.Today).Days;
            if (daysLeft < 0)
            {
                vm.DeadlineText = $"⚠ Quá hạn {-daysLeft} ngày";
                vm.DeadlineColor = "#C62828"; // Đỏ
                vm.RowBackground = "#FFEBEE"; // Đỏ nhạt
            }
            else if (daysLeft <= 3)
            {
                vm.DeadlineText = daysLeft == 0 ? "⏰ Hết hạn hôm nay" : $"⏰ Còn {daysLeft} ngày";
                vm.DeadlineColor = "#E65100"; // Cam
                vm.RowBackground = "#FFF3E0"; // Vàng nhạt
            }
        }
        
        return vm;
    }
}

public partial class DocumentListPage : Page
{
    private readonly DocumentService _documentService;
    private List<Document> _allDocuments = new();
    private string _selectedFolderId = string.Empty;
    private DateTime? _quickFilterStart = null;
    private DateTime? _quickFilterEnd = null;
    private bool _isTrashView = false; // Thùng rác mode
    private string _personalFilter = ""; // starred, unprocessed, overdue
    
    // Debounce timer cho search — tránh ApplyFilters mỗi keystroke
    private readonly System.Windows.Threading.DispatcherTimer _searchDebounceTimer;

    public DocumentListPage(DocumentService documentService)
    {
        InitializeComponent();
        _documentService = documentService;
        
        // Init search debounce: chờ 300ms sau keystroke cuối mới search
        _searchDebounceTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _searchDebounceTimer.Tick += (s, e) =>
        {
            _searchDebounceTimer.Stop();
            ApplyFilters();
        };
        
        // Ẩn banner hướng dẫn nếu user đã đóng trước đó
        try
        {
            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "AIVanBan", "settings.json");
            if (File.Exists(settingsPath))
            {
                var json = File.ReadAllText(settingsPath);
                if (json.Contains("\"statusGuideHidden\":true"))
                    pnlStatusGuide.Visibility = Visibility.Collapsed;
            }
        }
        catch { /* Bỏ qua */ }
        
        // Check if first-time setup needed
        CheckAndRunSetup();
        
        InitializeFilters();
        LoadFolders();
        LoadDocuments();
    }
    
    // Helper method to remove Vietnamese diacritics for search
    private string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
            
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
    
    private void CheckAndRunSetup()
    {
        var folders = _documentService.GetAllFolders();
        
        // Nếu chưa có folder hoặc ít hơn 5 folders -> chạy setup
        // (Unified Wizard ở MainWindow đã xử lý first-run, đây là fallback)
        if (folders.Count < 5)
        {
            var orgConfig = _documentService.GetOrganizationConfig();
            if (!string.IsNullOrEmpty(orgConfig.Name))
            {
                // Đã có org config (từ Unified Wizard) nhưng folders bị mất → tạo lại
                var setupService = new OrganizationSetupService(_documentService);
                setupService.CreateDefaultStructure(orgConfig.Name, orgConfig.Type, orgConfig.Abbreviation);
                LoadFolders();
                return;
            }
            
            var result = MessageBox.Show(
                "🏛️ CẤU TRÚC THƯ MỤC CHƯA ĐƯỢC THIẾT LẬP\n\n" +
                "Bạn có muốn thiết lập cấu trúc thư mục chuẩn cho cơ quan không?\n\n" +
                "Hệ thống sẽ tự động tạo cấu trúc phù hợp với loại cơ quan.",
                "Thiết lập thư mục",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                var setupService = new OrganizationSetupService(_documentService);
                var setupDialog = new OrganizationSetupDialog(setupService);
                
                if (setupDialog.ShowDialog() == true)
                {
                    LoadFolders();
                }
            }
            else
            {
                _documentService.InitializeDefaultFolders();
            }
        }
    }

    private void InitializeFilters()
    {
        // Load Document Types (hiển thị tên tiếng Việt)
        cboType.Items.Add("Tất cả");
        foreach (DocumentType type in Enum.GetValues(typeof(DocumentType)))
        {
            cboType.Items.Add(type.GetDisplayName());
        }
        cboType.SelectedIndex = 0;

        // Load Years
        cboYear.Items.Add("Tất cả");
        for (int year = DateTime.Now.Year; year >= 2020; year--)
        {
            cboYear.Items.Add(year);
        }
        cboYear.SelectedIndex = 0;
        
        // Load Direction (for advanced search)
        if (cboDirection != null)
        {
            cboDirection.Items.Add("Tất cả");
            cboDirection.Items.Add("Đi");
            cboDirection.Items.Add("Đến");
            cboDirection.Items.Add("Nội bộ");
            cboDirection.SelectedIndex = 0;
        }
        
        // Load Workflow Status (for advanced search)
        if (cboWorkflowStatus != null)
        {
            cboWorkflowStatus.Items.Add("Tất cả");
            cboWorkflowStatus.Items.Add("📝 Đang soạn");
            cboWorkflowStatus.Items.Add("📤 Đã trình sếp");
            cboWorkflowStatus.Items.Add("✅ Sếp đã duyệt");
            cboWorkflowStatus.Items.Add("✍️ Đã ký");
            cboWorkflowStatus.Items.Add("📢 Đã phát hành");
            cboWorkflowStatus.Items.Add("📨 Đã gửi");
            cboWorkflowStatus.Items.Add("🗄️ Xong — Lưu hồ sơ");
            cboWorkflowStatus.SelectedIndex = 0;
        }
    }

    private async void LoadDocuments()
    {
        // Show loading state
        if (dgDocuments != null) dgDocuments.IsEnabled = false;
        if (txtTotalDocs != null) txtTotalDocs.Text = "Đang tải...";
        
        try
        {
            // Load on background thread to avoid UI freeze
            var folderId = _selectedFolderId;
            var isTrash = _isTrashView;
            var docs = await Task.Run(() =>
            {
                if (isTrash)
                    return _documentService.GetDeletedDocuments();
                else if (!string.IsNullOrEmpty(folderId))
                    return _documentService.GetDocumentsByFolder(folderId);
                else
                    return _documentService.GetAllDocuments();
            });
            
            _allDocuments = docs;
            ApplyFilters();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error loading documents: {ex.Message}");
            MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            if (dgDocuments != null) dgDocuments.IsEnabled = true;
        }
    }

    private void ApplyFilters()
    {
        try
        {
            Console.WriteLine($"📋 ApplyFilters START: _allDocuments.Count={_allDocuments?.Count ?? 0}");
            
            if (_allDocuments == null)
            {
                Console.WriteLine("⚠️ _allDocuments is NULL!");
                _allDocuments = new List<Document>();
                return;
            }
            
            var filtered = _allDocuments.AsEnumerable();

            // Filter by search (without Vietnamese diacritics)
            if (!string.IsNullOrWhiteSpace(txtSearch?.Text))
            {
                var keyword = txtSearch.Text.ToLower();
                var keywordNoAccent = RemoveDiacritics(keyword);
                Console.WriteLine($"🔍 Search filter: keyword='{keyword}' (no accent: '{keywordNoAccent}')");
                
                filtered = filtered.Where(d =>
                {
                    var title = RemoveDiacritics(d.Title ?? "").ToLower();
                    var number = RemoveDiacritics(d.Number ?? "").ToLower();
                    var subject = RemoveDiacritics(d.Subject ?? "").ToLower();
                    var content = RemoveDiacritics(d.Content ?? "").ToLower();
                    
                    return title.Contains(keywordNoAccent) ||
                           number.Contains(keywordNoAccent) ||
                           subject.Contains(keywordNoAccent) ||
                           content.Contains(keywordNoAccent);
                });
            }

            // Filter by quick date range
            if (_quickFilterStart.HasValue && _quickFilterEnd.HasValue)
            {
                Console.WriteLine($"📅 Date filter: {_quickFilterStart.Value:yyyy-MM-dd} to {_quickFilterEnd.Value:yyyy-MM-dd}");
                filtered = filtered.Where(d => 
                    d.IssueDate >= _quickFilterStart.Value && 
                    d.IssueDate <= _quickFilterEnd.Value);
            }

            // Filter by type
            if (cboType != null && cboType.SelectedIndex > 0 && cboType.SelectedItem is string selectedTypeName)
            {
                var matchedType = Enum.GetValues(typeof(DocumentType)).Cast<DocumentType>()
                    .FirstOrDefault(t => t.GetDisplayName() == selectedTypeName);
                Console.WriteLine($"📂 Type filter: {matchedType}");
                filtered = filtered.Where(d => d.Type == matchedType);
            }

            // Filter by year
            if (cboYear != null && cboYear.SelectedIndex > 0 && cboYear.SelectedItem != null)
            {
                var selectedYear = (int)cboYear.SelectedItem;
                Console.WriteLine($"📆 Year filter: {selectedYear}");
                filtered = filtered.Where(d => d.IssueDate.Year == selectedYear);
            }
            
            // ADVANCED FILTERS
            
            // Filter by document number
            if (!string.IsNullOrWhiteSpace(txtSearchNumber?.Text))
            {
                var number = RemoveDiacritics(txtSearchNumber.Text.ToLower());
                filtered = filtered.Where(d => 
                    RemoveDiacritics(d.Number ?? "").ToLower().Contains(number));
            }
            
            // Filter by signer
            if (!string.IsNullOrWhiteSpace(txtSearchSigner?.Text))
            {
                var signer = RemoveDiacritics(txtSearchSigner.Text.ToLower());
                filtered = filtered.Where(d => 
                    RemoveDiacritics(d.SignedBy ?? "").ToLower().Contains(signer));
            }
            
            // Filter by direction
            if (cboDirection != null && cboDirection.SelectedIndex > 0)
            {
                var direction = cboDirection.SelectedIndex switch
                {
                    1 => Direction.Di,
                    2 => Direction.Den,
                    3 => Direction.NoiBo,
                    _ => Direction.Den
                };
                filtered = filtered.Where(d => d.Direction == direction);
            }
            
            // Filter by date range
            if (dpFromDate?.SelectedDate != null)
            {
                filtered = filtered.Where(d => d.IssueDate >= dpFromDate.SelectedDate.Value);
            }
            if (dpToDate?.SelectedDate != null)
            {
                filtered = filtered.Where(d => d.IssueDate <= dpToDate.SelectedDate.Value.AddDays(1).AddSeconds(-1));
            }
            
            // Filter by workflow status
            if (cboWorkflowStatus != null && cboWorkflowStatus.SelectedIndex > 0)
            {
                var status = cboWorkflowStatus.SelectedIndex switch
                {
                    1 => DocumentStatus.Draft,
                    2 => DocumentStatus.PendingApproval,
                    3 => DocumentStatus.Approved,
                    4 => DocumentStatus.Signed,
                    5 => DocumentStatus.Published,
                    6 => DocumentStatus.Sent,
                    7 => DocumentStatus.Archived,
                    _ => DocumentStatus.Draft
                };
                filtered = filtered.Where(d => d.WorkflowStatus == status);
            }
            
            // Personal tracking filters — Sổ theo dõi cá nhân
            if (_personalFilter == "starred")
                filtered = filtered.Where(d => d.IsStarred);
            else if (_personalFilter == "unprocessed")
                filtered = filtered.Where(d => d.MyStatus == PersonalStatus.ChuaXuLy);
            else if (_personalFilter == "overdue")
            {
                var now = DateTime.Now;
                filtered = filtered.Where(d => 
                    d.MyStatus != PersonalStatus.DaXuLy 
                    && d.MyStatus != PersonalStatus.ChuyenTiep
                    && ((d.PersonalDeadline.HasValue && d.PersonalDeadline.Value < now)
                        || (d.DueDate.HasValue && d.DueDate.Value < now)));
            }

            // Sắp xếp mặc định: Năm DESC → Số VB ASC (numeric-aware)
            // Trước đây chỉ sort theo IssueDate, làm số VB "10" hiển thị trước "2" do so sánh chuỗi.
            var result = filtered
                .Select(d => DocumentViewModel.FromDocument(d, _documentService))
                .OrderByDescending(vm => vm.IssueDate.Year)
                .ThenBy(vm => vm.Type)
                .ThenBy(vm => vm.NumberSortKey)
                .ThenByDescending(vm => vm.IssueDate)
                .ToList();
            
            // Hiện nút Phục hồi khi ở chế độ thùng rác
            if (_isTrashView)
            {
                foreach (var vm in result)
                {
                    vm.RestoreVisibility = Visibility.Visible;
                    vm.EditVisibility = Visibility.Collapsed;
                }
            }
            
            Console.WriteLine($"✅ Filtered result: {result.Count} documents");
            
            if (dgDocuments != null)
            {
                dgDocuments.ItemsSource = result;
            }
            
            // Update stats
            if (txtTotalDocs != null)
            {
                txtTotalDocs.Text = $"Tổng: {_allDocuments.Count} văn bản";
            }
            if (txtFilteredDocs != null)
            {
                txtFilteredDocs.Text = $"Hiển thị: {result.Count}";
            }
            
            // Show/Hide empty state
            if (result.Count == 0 && _allDocuments.Count == 0)
            {
                // Completely empty - show empty state
                if (emptyStatePanel != null)
                    emptyStatePanel.Visibility = Visibility.Visible;
                if (dgDocuments != null)
                    dgDocuments.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Has documents or filtered results
                if (emptyStatePanel != null)
                    emptyStatePanel.Visibility = Visibility.Collapsed;
                if (dgDocuments != null)
                    dgDocuments.Visibility = Visibility.Visible;
            }
            
            // Update quick filter button styles
            ResetQuickFilterStyles();
            
            Console.WriteLine($"📋 ApplyFilters COMPLETE");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR in ApplyFilters: {ex.Message}");
            Console.WriteLine($"Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"InnerException: {ex.InnerException.Message}");
            }
            
            // Show error with copy button
            ShowErrorDialog("Lỗi ApplyFilters", ex);
        }
    }
    
    private void ShowErrorDialog(string title, Exception ex)
    {
        var errorMessage = $"Type: {ex.GetType().Name}\n" +
                          $"Message: {ex.Message}\n\n" +
                          $"StackTrace:\n{ex.StackTrace}";
        
        var dialog = new Window
        {
            Title = title,
            Width = 600,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.CanResize
        };
        
        var grid = new Grid { Margin = new Thickness(10) };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        
        var txtError = new TextBox
        {
            Text = errorMessage,
            IsReadOnly = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
            FontSize = 12,
            Padding = new Thickness(10)
        };
        Grid.SetRow(txtError, 0);
        
        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0)
        };
        Grid.SetRow(btnPanel, 1);
        
        var btnCopy = new Button
        {
            Content = "📋 Copy",
            Width = 100,
            Height = 32,
            Margin = new Thickness(0, 0, 10, 0)
        };
        btnCopy.Click += (s, e) =>
        {
            try
            {
                System.Windows.Clipboard.SetText(errorMessage);
                btnCopy.Content = "✅ Copied!";
                Task.Delay(2000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => btnCopy.Content = "📋 Copy");
                });
            }
            catch { }
        };
        
        var btnClose = new Button
        {
            Content = "Đóng",
            Width = 100,
            Height = 32
        };
        btnClose.Click += (s, e) => dialog.Close();
        
        btnPanel.Children.Add(btnCopy);
        btnPanel.Children.Add(btnClose);
        
        grid.Children.Add(txtError);
        grid.Children.Add(btnPanel);
        dialog.Content = grid;
        
        dialog.ShowDialog();
    }
    
    // Quick Filter Handlers
    private void FilterToday_Click(object sender, RoutedEventArgs e)
    {
        _quickFilterStart = DateTime.Today;
        _quickFilterEnd = DateTime.Today.AddDays(1).AddSeconds(-1);
        ApplyFilters();
        HighlightQuickFilter(btnFilterToday);
    }
    
    private void FilterWeek_Click(object sender, RoutedEventArgs e)
    {
        var today = DateTime.Today;
        var dayOfWeek = (int)today.DayOfWeek;
        var startOfWeek = today.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1)); // Monday
        _quickFilterStart = startOfWeek;
        _quickFilterEnd = startOfWeek.AddDays(7).AddSeconds(-1);
        ApplyFilters();
        HighlightQuickFilter(btnFilterWeek);
    }
    
    private void FilterMonth_Click(object sender, RoutedEventArgs e)
    {
        var today = DateTime.Today;
        _quickFilterStart = new DateTime(today.Year, today.Month, 1);
        _quickFilterEnd = _quickFilterStart.Value.AddMonths(1).AddSeconds(-1);
        ApplyFilters();
        HighlightQuickFilter(btnFilterMonth);
    }
    
    private void ClearFilters_Click(object sender, RoutedEventArgs e)
    {
        _quickFilterStart = null;
        _quickFilterEnd = null;
        _personalFilter = "";
        txtSearch.Text = string.Empty;
        cboType.SelectedIndex = 0;
        cboYear.SelectedIndex = 0;
        
        // Clear advanced filters
        if (txtSearchNumber != null) txtSearchNumber.Text = string.Empty;
        if (txtSearchSigner != null) txtSearchSigner.Text = string.Empty;
        if (cboDirection != null) cboDirection.SelectedIndex = 0;
        if (dpFromDate != null) dpFromDate.SelectedDate = null;
        if (dpToDate != null) dpToDate.SelectedDate = null;
        if (cboWorkflowStatus != null) cboWorkflowStatus.SelectedIndex = 0;
        
        ResetPersonalFilterStyles();
        ApplyFilters();
    }

    /// <summary>Lọc VB đánh dấu sao</summary>
    private void FilterStarred_Click(object sender, RoutedEventArgs e)
    {
        _personalFilter = _personalFilter == "starred" ? "" : "starred";
        ResetPersonalFilterStyles();
        if (_personalFilter == "starred")
            btnFilterStarred.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(40, 255, 193, 7));
        ApplyFilters();
    }

    /// <summary>Lọc VB chưa xử lý</summary>
    private void FilterUnprocessed_Click(object sender, RoutedEventArgs e)
    {
        _personalFilter = _personalFilter == "unprocessed" ? "" : "unprocessed";
        ResetPersonalFilterStyles();
        if (_personalFilter == "unprocessed")
            btnFilterUnprocessed.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(40, 251, 140, 0));
        ApplyFilters();
    }

    /// <summary>Lọc VB quá hạn</summary>
    private void FilterOverdue_Click(object sender, RoutedEventArgs e)
    {
        _personalFilter = _personalFilter == "overdue" ? "" : "overdue";
        ResetPersonalFilterStyles();
        if (_personalFilter == "overdue")
            btnFilterOverdue.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(40, 198, 40, 40));
        ApplyFilters();
    }

    private void ResetPersonalFilterStyles()
    {
        btnFilterStarred.Background = System.Windows.Media.Brushes.Transparent;
        btnFilterUnprocessed.Background = System.Windows.Media.Brushes.Transparent;
        btnFilterOverdue.Background = System.Windows.Media.Brushes.Transparent;
    }
    
    private void ResetQuickFilterStyles()
    {
        if (_quickFilterStart.HasValue)
        {
            // Keep highlight if filter is active
            return;
        }
        
        btnFilterToday.Style = (Style)FindResource("MaterialDesignOutlinedButton");
        btnFilterWeek.Style = (Style)FindResource("MaterialDesignOutlinedButton");
        btnFilterMonth.Style = (Style)FindResource("MaterialDesignOutlinedButton");
    }
    
    private void HighlightQuickFilter(Button activeButton)
    {
        // Reset all
        btnFilterToday.Style = (Style)FindResource("MaterialDesignOutlinedButton");
        btnFilterWeek.Style = (Style)FindResource("MaterialDesignOutlinedButton");
        btnFilterMonth.Style = (Style)FindResource("MaterialDesignOutlinedButton");
        
        // Highlight active
        activeButton.Style = (Style)FindResource("MaterialDesignRaisedButton");
    }
    
    // Keyboard Shortcuts
    private void Page_KeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            // Ctrl+F -> Focus search box
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                txtSearch.Focus();
                txtSearch.SelectAll();
                e.Handled = true;
            }
            // Ctrl+N -> Add new document
            else if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                AddDocument_Click(sender, e);
                e.Handled = true;
            }
            // F5 -> Refresh
            else if (e.Key == Key.F5)
            {
                LoadDocuments();
                e.Handled = true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in Page_KeyDown: {ex.Message}");
        }
    }
    
    private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if (dgDocuments.SelectedItem is DocumentViewModel docVm)
            {
                // Enter -> Open document
                if (e.Key == Key.Enter)
                {
                    var doc = _documentService.GetDocument(docVm.Id);
                    if (doc != null)
                    {
                        var viewer = new DocumentViewDialog(doc, _documentService);
                        viewer.ShowDialog();
                        if (viewer.IsEdited) LoadDocuments();
                    }
                    e.Handled = true;
                }
                // Delete -> Delete document
                else if (e.Key == Key.Delete)
                {
                    var doc = _documentService.GetDocument(docVm.Id);
                    if (doc != null)
                    {
                        if (_isTrashView)
                        {
                            var result = MessageBox.Show(
                                $"Xóa vĩnh viễn '{doc.Title}'?\nKhông thể hoàn tác!",
                                "Xóa vĩnh viễn",
                                MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (result == MessageBoxResult.Yes)
                            {
                                _documentService.PermanentDeleteDocument(docVm.Id);
                                LoadDocuments();
                            }
                        }
                        else
                        {
                            var result = MessageBox.Show(
                                $"Chuyển '{doc.Title}' vào thùng rác?",
                                "Xóa văn bản",
                                MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                _documentService.SoftDeleteDocument(docVm.Id);
                                LoadDocuments();
                            }
                        }
                    }
                    e.Handled = true;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in DataGrid_PreviewKeyDown: {ex.Message}");
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Search_KeyUp(object sender, KeyEventArgs e)
    {
        try
        {
            // Escape key: clear search text
            if (e.Key == Key.Escape)
            {
                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    txtSearch.Text = string.Empty;
                    ApplyFilters();
                    e.Handled = true;
                    return;
                }
                // Nếu search đã trống, bỏ chọn DataGrid
                if (dgDocuments != null)
                    dgDocuments.SelectedItem = null;
                e.Handled = true;
                return;
            }
            
            // Enter: search ngay lập tức
            if (e.Key == Key.Enter)
            {
                _searchDebounceTimer.Stop();
                ApplyFilters();
                return;
            }
            
            // Debounce: chờ 300ms rồi mới search
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR in Search_KeyUp: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            ShowErrorDialog("Lỗi Search", ex);
        }
    }

    private void FilterChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            Console.WriteLine($"🔄 FilterChanged: cboType={cboType?.SelectedIndex}, cboYear={cboYear?.SelectedIndex}");
            ApplyFilters();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR in FilterChanged: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            ShowErrorDialog("Lỗi Filter", ex);
        }
    }
    
    // Advanced Search handlers
    private void AdvancedSearch_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (advancedPanel.Visibility == Visibility.Collapsed)
            {
                // Mở panel
                advancedPanel.Visibility = Visibility.Visible;
                btnAdvancedSearch.Style = (Style)FindResource("MaterialDesignRaisedButton");
                
                // Đổi text và icon
                if (txtAdvancedSearch != null)
                    txtAdvancedSearch.Text = "Thu gọn";
                if (iconAdvancedSearch != null)
                    iconAdvancedSearch.Kind = MaterialDesignThemes.Wpf.PackIconKind.ChevronUp;
            }
            else
            {
                // Đóng panel
                advancedPanel.Visibility = Visibility.Collapsed;
                btnAdvancedSearch.Style = (Style)FindResource("MaterialDesignOutlinedButton");
                
                // Đổi text và icon
                if (txtAdvancedSearch != null)
                    txtAdvancedSearch.Text = "Tìm kiếm nâng cao";
                if (iconAdvancedSearch != null)
                    iconAdvancedSearch.Kind = MaterialDesignThemes.Wpf.PackIconKind.ChevronDown;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR in AdvancedSearch_Click: {ex.Message}");
        }
    }
    
    private void AdvancedFilter_Changed(object sender, EventArgs e)
    {
        try
        {
            ApplyFilters();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR in AdvancedFilter_Changed: {ex.Message}");
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadDocuments();
    }
    
    // ═══════════════════════════════════════
    // THÙNG RÁC — Soft Delete
    // ═══════════════════════════════════════
    private void TrashToggle_Click(object sender, RoutedEventArgs e)
    {
        _isTrashView = !_isTrashView;
        
        if (_isTrashView)
        {
            txtTrashToggle.Text = "← Quay lại";
            btnTrashToggle.BorderBrush = new SolidColorBrush(Color.FromRgb(198, 40, 40));
            btnTrashToggle.Foreground = new SolidColorBrush(Color.FromRgb(198, 40, 40));
            btnEmptyTrash.Visibility = Visibility.Visible;
            
            // Update status text
            var trashCount = _documentService.GetTrashCount();
            if (txtTotalDocs != null)
                txtTotalDocs.Text = $"🗑️ Thùng rác: {trashCount} văn bản";
        }
        else
        {
            txtTrashToggle.Text = "Thùng rác";
            btnTrashToggle.BorderBrush = new SolidColorBrush(Color.FromRgb(158, 158, 158));
            btnTrashToggle.Foreground = (Brush)FindResource("MaterialDesignBody");
            btnEmptyTrash.Visibility = Visibility.Collapsed;
        }
        
        LoadDocuments();
    }
    
    private void RestoreDocument_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string id)
        {
            _documentService.RestoreDocument(id);
            LoadDocuments();
            Services.SnackbarHelper.ShowSuccess("Đã khôi phục văn bản!");
        }
    }
    
    private void EmptyTrash_Click(object sender, RoutedEventArgs e)
    {
        var count = _documentService.GetTrashCount();
        if (count == 0)
        {
            MessageBox.Show("Thùng rác đã trống.", "Thùng rác", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var result = MessageBox.Show(
            $"Xóa vĩnh viễn {count} văn bản trong thùng rác?\nHành động này KHÔNG thể hoàn tác!",
            "Dọn sạch thùng rác",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        
        if (result == MessageBoxResult.Yes)
        {
            var deleted = _documentService.EmptyTrash();
            LoadDocuments();
            Services.SnackbarHelper.ShowSuccess($"Đã xóa vĩnh viễn {deleted} văn bản.");
        }
    }

    private void AddDocument_Click(object sender, RoutedEventArgs e)
    {
        // Pass selectedFolderId để văn bản mới được gán vào đúng thư mục
        var dialog = new DocumentEditDialog(null, _selectedFolderId, _documentService);
        if (dialog.ShowDialog() == true && dialog.Document != null)
        {
            _documentService.AddDocument(dialog.Document);
            
            // Reload folders to update document count
            LoadFolders();
            
            // Reload documents in current folder
            LoadDocuments();
            
            Services.SnackbarHelper.ShowSuccess("Đã thêm văn bản thành công!");
            
            // Nếu chọn "Lưu & Thêm mới" → mở lại dialog với defaults giữ lại
            if (dialog.SaveAndAddNew)
            {
                var lastDoc = dialog.Document;
                var newDialog = new DocumentEditDialog(null, _selectedFolderId, _documentService);
                // Auto-fill defaults từ VB vừa lưu để tăng tốc nhập liệu
                newDialog.Loaded += (s, args) =>
                {
                    newDialog.PreFillDefaults(lastDoc.Issuer, lastDoc.Location, lastDoc.Direction, lastDoc.Type);
                };
                if (newDialog.ShowDialog() == true && newDialog.Document != null)
                {
                    _documentService.AddDocument(newDialog.Document);
                    LoadFolders();
                    LoadDocuments();
                    Services.SnackbarHelper.ShowSuccess("Đã thêm văn bản thành công!");
                }
            }
        }
    }

    private void ScanImport_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new ScanImportDialog(_documentService);
            dialog.Owner = Window.GetWindow(this);
            
            if (dialog.ShowDialog() == true && dialog.CreatedDocuments.Count > 0)
            {
                // Gán folder hiện tại nếu có
                foreach (var doc in dialog.CreatedDocuments)
                {
                    if (!string.IsNullOrEmpty(_selectedFolderId))
                        doc.FolderId = _selectedFolderId;
                    
                    _documentService.AddDocument(doc);
                }
                
                LoadFolders();
                LoadDocuments();
                
                if (dialog.CreatedDocuments.Count == 1)
                {
                    var doc = dialog.CreatedDocuments[0];
                    Services.SnackbarHelper.ShowSuccess($"Đã nhập VB: {doc.Number} — {doc.Title}");
                }
                else
                {
                    Services.SnackbarHelper.ShowSuccess($"Đã nhập {dialog.CreatedDocuments.Count} văn bản từ scan.");
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GenerateDemo_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = MessageBox.Show(
                "⚠️ Tạo 50 văn bản demo mẫu?\n\n" +
                "Toàn bộ dữ liệu hiện có sẽ bị XÓA SẠCH\n" +
                "và thay bằng 50 văn bản demo nhất quán.\n\n" +
                "Phủ 25+ loại VB, nhiều phòng ban, cơ quan.\n" +
                "Dữ liệu demo giúp kiểm tra:\n" +
                "• Cảnh báo hạn xử lý (VB đến quá hạn/sắp hạn)\n" +
                "• Export Word, tìm kiếm, lọc, sắp xếp\n" +
                "• Thùng rác, thống kê",
                "Tạo dữ liệu Demo",
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            // Xóa sạch toàn bộ (bao gồm thùng rác)
            var deletedCount = _documentService.DeleteAllDocuments();

            // Tạo mới 50 VB demo nhất quán — phủ 25+ loại VB theo Điều 7, NĐ 30/2020
            var seedService = new AIVanBan.Core.Services.SeedDataService(_documentService);
            // Lấy tên cơ quan từ OrganizationConfig (nếu có)
            var orgConfig = _documentService.GetOrganizationConfig();
            var orgName = !string.IsNullOrEmpty(orgConfig?.Name) ? orgConfig.Name : "Sở Nội vụ";
            var docs = seedService.GenerateDemoDocuments(orgName: orgName);

            LoadDocuments();
            
            var clearText = deletedCount > 0 ? $"(đã xóa {deletedCount} văn bản cũ)\n" : "";
            
            MessageBox.Show(
                $"✅ Đã tạo thành công {docs.Count} văn bản demo!\n" +
                clearText +
                $"\n• {docs.Count(d => d.Direction == Direction.Di)} văn bản đi\n" +
                $"• {docs.Count(d => d.Direction == Direction.Den)} văn bản đến (có hạn xử lý)\n" +
                $"• {docs.Count(d => d.Direction == Direction.NoiBo)} văn bản nội bộ", 
                "Thành công",
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in GenerateDemo_Click: {ex.Message}");
            MessageBox.Show($"Lỗi khi tạo dữ liệu demo:\n{ex.Message}", "Lỗi", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Click vào badge trạng thái → hiện ContextMenu chuyển nhanh
    /// </summary>
    private void StatusBadge_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not Border badge || badge.Tag is not string docId) return;

        var doc = _documentService.GetDocument(docId);
        if (doc == null) return;

        var menu = new ContextMenu();
        
        // Header: hướng dẫn
        var header = new MenuItem
        {
            Header = "📋 Chuyển trạng thái văn bản",
            IsEnabled = false,
            FontWeight = FontWeights.Bold,
            FontSize = 12
        };
        menu.Items.Add(header);
        menu.Items.Add(new Separator());
        
        var statuses = new[]
        {
            (DocumentStatus.Draft,            "📝 Đang soạn",         "Tôi đang soạn thảo VB này"),
            (DocumentStatus.PendingApproval,  "📤 Đã trình sếp",      "Đã đưa lãnh đạo xem/ký"),
            (DocumentStatus.Approved,         "✅ Sếp đã duyệt",      "Lãnh đạo OK, chờ ký chính thức"),
            (DocumentStatus.Signed,           "✍️ Đã ký",             "Đã ký xong, chờ phát hành"),
            (DocumentStatus.Published,        "📢 Đã phát hành",      "Có số VB chính thức"),
            (DocumentStatus.Sent,             "📨 Đã gửi",            "Đã gửi đến nơi nhận"),
            (DocumentStatus.Archived,         "🗄️ Xong — Lưu hồ sơ", "Đã hoàn thành, lưu hồ sơ")
        };
        
        // Gợi ý bước tiếp theo
        var nextStatus = doc.WorkflowStatus.GetNextStatus();

        foreach (var (status, label, tip) in statuses)
        {
            var item = new MenuItem
            {
                Header = label,
                ToolTip = tip,
                IsCheckable = false,
                FontWeight = doc.WorkflowStatus == status ? FontWeights.Bold : FontWeights.Normal,
                IsEnabled = doc.WorkflowStatus != status
            };
            
            // Đánh dấu trạng thái hiện tại + gợi ý bước tiếp
            if (doc.WorkflowStatus == status)
            {
                item.Icon = new PackIcon { Kind = PackIconKind.CheckCircle, Foreground = Brushes.Green };
            }
            else if (nextStatus.HasValue && status == nextStatus.Value)
            {
                item.Icon = new PackIcon { Kind = PackIconKind.ArrowRight, Foreground = Brushes.Orange };
                item.FontWeight = FontWeights.SemiBold;
                item.Header = $"{label}  ← Bước tiếp";
            }

            var capturedStatus = status;
            item.Click += (s, args) =>
            {
                doc.WorkflowStatus = capturedStatus;
                _documentService.UpdateDocument(doc);
                LoadDocuments();
                Services.SnackbarHelper.ShowSuccess($"Đã chuyển trạng thái → {capturedStatus.GetDisplayName()}");
            };
            menu.Items.Add(item);
        }

        badge.ContextMenu = menu;
        menu.IsOpen = true;
        e.Handled = true;
    }

    /// <summary>Toggle đánh dấu sao — Sổ theo dõi cá nhân</summary>
    private void StarToggle_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not TextBlock star || star.Tag is not string docId) return;
        _documentService.ToggleStar(docId);
        LoadDocuments();
        e.Handled = true;
    }

    /// <summary>Đóng banner hướng dẫn trạng thái + lưu setting để không hiện lại</summary>
    private void CloseStatusGuide_Click(object sender, RoutedEventArgs e)
    {
        pnlStatusGuide.Visibility = Visibility.Collapsed;
        try
        {
            var settingsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AIVanBan");
            var settingsPath = Path.Combine(settingsDir, "settings.json");
            
            var json = "{}";
            if (File.Exists(settingsPath))
                json = File.ReadAllText(settingsPath);
            
            // Thêm flag statusGuideHidden vào settings
            if (!json.Contains("statusGuideHidden"))
            {
                json = json.TrimEnd().TrimEnd('}') + (json.Contains(":") ? "," : "") 
                    + "\"statusGuideHidden\":true}";
                Directory.CreateDirectory(settingsDir);
                File.WriteAllText(settingsPath, json);
            }
        }
        catch { /* Bỏ qua nếu không ghi được */ }
    }

    /// <summary>Đổi trạng thái xử lý cá nhân — Sổ theo dõi cá nhân</summary>
    private void PersonalStatusBadge_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not Border badge || badge.Tag is not string docId) return;

        var doc = _documentService.GetDocument(docId);
        if (doc == null) return;

        var menu = new ContextMenu();
        var statuses = new[]
        {
            (PersonalStatus.ChuaXuLy,   "⬜ Chưa xử lý"),
            (PersonalStatus.DangXuLy,   "🟠 Đang xử lý"),
            (PersonalStatus.DaXuLy,     "✅ Đã xử lý"),
            (PersonalStatus.ChuyenTiep,  "➡️ Chuyển tiếp")
        };

        foreach (var (status, label) in statuses)
        {
            var item = new MenuItem
            {
                Header = label,
                FontWeight = doc.MyStatus == status ? FontWeights.Bold : FontWeights.Normal,
                IsEnabled = doc.MyStatus != status
            };
            if (doc.MyStatus == status)
                item.Icon = new PackIcon { Kind = PackIconKind.CheckCircle, Foreground = Brushes.Green };

            var capturedStatus = status;
            item.Click += (s, args) =>
            {
                _documentService.UpdatePersonalStatus(docId, capturedStatus);
                LoadDocuments();
                Services.SnackbarHelper.ShowSuccess($"Đã đổi trạng thái → {capturedStatus.GetDisplayName()}");
            };
            menu.Items.Add(item);
        }

        badge.ContextMenu = menu;
        menu.IsOpen = true;
        e.Handled = true;
    }

    private void ViewDocument_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button && button.Tag is string id)
            {
                var doc = _documentService.GetDocument(id);
                if (doc != null)
                {
                    var viewer = new DocumentViewDialog(doc, _documentService);
                    viewer.ShowDialog();
                    if (viewer.IsEdited) LoadDocuments();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in ViewDocument_Click: {ex.Message}");
            MessageBox.Show($"Lỗi khi xem văn bản:\n{ex.Message}", "Lỗi", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void EditDocument_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button && button.Tag is string id)
            {
                var doc = _documentService.GetDocument(id);
                if (doc != null)
                {
                    var dialog = new DocumentEditDialog(doc, documentService: _documentService);
                    if (dialog.ShowDialog() == true && dialog.Document != null)
                    {
                        _documentService.UpdateDocument(dialog.Document);
                        LoadDocuments();
                        Services.SnackbarHelper.ShowSuccess("Đã cập nhật văn bản!");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in EditDocument_Click: {ex.Message}");
            MessageBox.Show($"Lỗi khi sửa văn bản:\n{ex.Message}", "Lỗi", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ReviewDocument_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string? docId = null;
            if (sender is Button btn && btn.Tag is string tagId)
                docId = tagId;
            else if (dgDocuments.SelectedItem is DocumentViewModel vm)
                docId = vm.Id;

            if (string.IsNullOrEmpty(docId)) return;

            var doc = _documentService.GetDocument(docId);
            if (doc == null)
            {
                MessageBox.Show("Không tìm thấy văn bản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var typeName = doc.Type.GetDisplayName();
            var dialog = new DocumentReviewDialog(doc.Content ?? "", typeName, doc.Title, doc.Issuer);
            
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.AppliedContent))
            {
                doc.Content = dialog.AppliedContent;
                _documentService.UpdateDocument(doc);
                LoadDocuments();
                Services.SnackbarHelper.ShowSuccess("Đã áp dụng nội dung đã sửa vào văn bản!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in ReviewDocument_Click: {ex.Message}");
            MessageBox.Show($"Lỗi khi kiểm tra văn bản:\n{ex.Message}", "Lỗi", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #region AI Tham mưu xử lý

    private void AdvisoryDocument_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string? docId = null;
            if (sender is Button btn && btn.Tag is string tagId)
                docId = tagId;
            else if (dgDocuments.SelectedItem is DocumentViewModel vm)
                docId = vm.Id;

            if (string.IsNullOrEmpty(docId))
            {
                MessageBox.Show("Vui lòng chọn một văn bản trước.", "Chưa chọn văn bản",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var doc = _documentService.GetDocument(docId);
            if (doc == null)
            {
                MessageBox.Show("Không tìm thấy văn bản trong cơ sở dữ liệu.", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Dùng Content hoặc fallback về Title/Subject nếu nội dung trống
            var contentToAnalyze = doc.Content;
            if (string.IsNullOrWhiteSpace(contentToAnalyze) || contentToAnalyze.Length < 10)
            {
                var fallbackParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(doc.Title)) fallbackParts.Add($"Tiêu đề: {doc.Title}");
                if (!string.IsNullOrWhiteSpace(doc.Subject)) fallbackParts.Add($"Trích yếu: {doc.Subject}");
                if (!string.IsNullOrWhiteSpace(doc.Issuer)) fallbackParts.Add($"Cơ quan ban hành: {doc.Issuer}");
                if (!string.IsNullOrWhiteSpace(doc.Number)) fallbackParts.Add($"Số hiệu: {doc.Number}");
                
                if (fallbackParts.Count == 0)
                {
                    MessageBox.Show("Văn bản chưa có nội dung hoặc thông tin để phân tích.\nVui lòng nhập nội dung văn bản trước.",
                        "Thiếu nội dung", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                contentToAnalyze = string.Join("\n", fallbackParts);
            }

            // Mở popup dialog — giống AI Kiểm tra
            var typeName = doc.Type.GetDisplayName();

            // Tạo context đầy đủ từ Document metadata
            var advisoryContext = DocumentAdvisoryContext.FromDocument(doc);

            // Load tóm tắt VB liên quan (nếu có RelatedDocumentIds)
            if (doc.RelatedDocumentIds?.Length > 0)
            {
                var docService = new DocumentService();
                var relatedSummaries = new List<string>();
                foreach (var relId in doc.RelatedDocumentIds.Take(5)) // Tối đa 5 VB liên quan
                {
                    var relDoc = docService.GetDocument(relId);
                    if (relDoc != null)
                    {
                        relatedSummaries.Add($"- [{relDoc.Type.GetDisplayName()}] {relDoc.Number} — {relDoc.Title} ({relDoc.Issuer}, {relDoc.IssueDate:dd/MM/yyyy})");
                    }
                }
                if (relatedSummaries.Count > 0)
                    advisoryContext.RelatedDocumentsSummary = string.Join("\n", relatedSummaries);
            }

            var dialog = new DocumentAdvisoryDialog(contentToAnalyze, typeName, doc.Title, doc.Issuer, advisoryContext);
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi mở AI Tham mưu:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SummaryDocument_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string? docId = null;
            if (sender is Button btn && btn.Tag is string tagId)
                docId = tagId;
            else if (dgDocuments.SelectedItem is DocumentViewModel vm)
                docId = vm.Id;

            if (string.IsNullOrEmpty(docId))
            {
                MessageBox.Show("Vui lòng chọn một văn bản trước.", "Chưa chọn văn bản",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var doc = _documentService.GetDocument(docId);
            if (doc == null)
            {
                MessageBox.Show("Không tìm thấy văn bản trong cơ sở dữ liệu.", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Dùng Content hoặc fallback về Title/Subject nếu nội dung trống
            var contentToAnalyze = doc.Content;
            if (string.IsNullOrWhiteSpace(contentToAnalyze) || contentToAnalyze.Length < 10)
            {
                var fallbackParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(doc.Title)) fallbackParts.Add($"Tiêu đề: {doc.Title}");
                if (!string.IsNullOrWhiteSpace(doc.Subject)) fallbackParts.Add($"Trích yếu: {doc.Subject}");
                if (!string.IsNullOrWhiteSpace(doc.Issuer)) fallbackParts.Add($"Cơ quan ban hành: {doc.Issuer}");
                if (!string.IsNullOrWhiteSpace(doc.Number)) fallbackParts.Add($"Số hiệu: {doc.Number}");
                
                if (fallbackParts.Count == 0)
                {
                    MessageBox.Show("Văn bản chưa có nội dung hoặc thông tin để tóm tắt.\nVui lòng nhập nội dung văn bản trước.",
                        "Thiếu nội dung", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                contentToAnalyze = string.Join("\n", fallbackParts);
            }

            // Mở popup dialog AI Tóm tắt
            var typeName = doc.Type.GetDisplayName();
            var dialog = new DocumentSummaryDialog(contentToAnalyze, typeName, doc.Title, doc.Issuer);
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi mở AI Tóm tắt:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    private void DeleteDocument_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string id)
        {
            var doc = _documentService.GetDocument(id);
            if (doc != null)
            {
                if (_isTrashView)
                {
                    // Trong thùng rác: xóa vĩnh viễn
                    var result = MessageBox.Show(
                        $"Xóa vĩnh viễn '{doc.Title}'?\nHành động này không thể hoàn tác!",
                        "Xóa vĩnh viễn",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        _documentService.PermanentDeleteDocument(id);
                        LoadDocuments();
                        Services.SnackbarHelper.ShowSuccess("Đã xóa vĩnh viễn!");
                    }
                }
                else
                {
                    // Bình thường: chuyển vào thùng rác (soft delete)
                    var result = MessageBox.Show(
                        $"Chuyển văn bản '{doc.Title}' vào thùng rác?",
                        "Xóa văn bản",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        _documentService.SoftDeleteDocument(id);
                        LoadDocuments();
                        Services.SnackbarHelper.ShowSuccess("Đã chuyển vào thùng rác! Bạn có thể khôi phục trong mục Thùng rác.");
                    }
                }
            }
        }
    }

    private void ManageAttachments_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button && button.Tag is string documentId)
            {
                var doc = _documentService.GetDocument(documentId);
                if (doc != null)
                {
                    var dialog = new AttachmentManagerDialog(_documentService, documentId, doc.Title);
                    dialog.ShowDialog();
                    
                    // Reload documents to update attachment count
                    LoadDocuments();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in ManageAttachments_Click: {ex.Message}");
            MessageBox.Show($"Lỗi khi mở quản lý file đính kèm:\n{ex.Message}", "Lỗi", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportWord_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button && button.Tag is string documentId)
            {
                var doc = _documentService.GetDocument(documentId);
                if (doc == null)
                {
                    MessageBox.Show("Không tìm thấy văn bản!", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Mở SaveFileDialog
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Lưu file Word",
                    FileName = $"{SanitizeFileName(doc.Number)}_{SanitizeFileName(doc.Title)}",
                    DefaultExt = ".docx",
                    Filter = "Word Document (*.docx)|*.docx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // Export văn bản ra Word
                    var wordService = new AIVanBan.Core.Services.WordExportService();
                    wordService.ExportDocument(doc, saveDialog.FileName);

                    // ⭐ Auto-update workflow status sau khi xuất Word thành công
                    // (Tester feedback v1.0.14: trạng thái VB không tự cập nhật khi xuất → user không biết)
                    SuggestStatusUpdateAfterExport(doc);

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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in ExportWord_Click: {ex.Message}");
            MessageBox.Show($"Lỗi khi xuất Word:\n{ex.Message}", "Lỗi", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Sau khi xuất Word thành công, gợi ý cập nhật trạng thái workflow.
    /// Chỉ gợi ý nếu trạng thái hiện tại là pre-published (Draft/PendingApproval/Approved/Signed).
    /// (Tester feedback v1.0.14: trạng thái VB không tự cập nhật khi xuất Word)
    /// </summary>
    private void SuggestStatusUpdateAfterExport(Document doc)
    {
        try
        {
            if (doc.WorkflowStatus == DocumentStatus.Published
                || doc.WorkflowStatus == DocumentStatus.Sent
                || doc.WorkflowStatus == DocumentStatus.Archived)
            {
                return;
            }

            var currentLabel = doc.WorkflowStatus switch
            {
                DocumentStatus.Draft => "📝 Đang soạn (Nháp)",
                DocumentStatus.PendingApproval => "⏳ Chờ duyệt",
                DocumentStatus.Approved => "✅ Đã duyệt",
                DocumentStatus.Signed => "✍️ Đã ký",
                _ => doc.WorkflowStatus.ToString()
            };

            var msg = $"📄 Văn bản đã được xuất ra Word.\n\n" +
                      $"Trạng thái hiện tại: {currentLabel}\n\n" +
                      $"Bạn có muốn cập nhật trạng thái sang \"📢 Đã phát hành\" để đánh dấu VB đã hoàn tất?\n\n" +
                      $"• [Yes] → Chuyển sang \"Đã phát hành\"\n" +
                      $"• [No] → Giữ nguyên trạng thái";

            var ans = MessageBox.Show(msg, "Cập nhật trạng thái văn bản",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (ans == MessageBoxResult.Yes)
            {
                doc.WorkflowStatus = DocumentStatus.Published;
                doc.PublishedDate = DateTime.Now;
                doc.PublishedBy = Environment.UserName;
                doc.ModifiedDate = DateTime.Now;
                doc.ModifiedBy = Environment.UserName;
                _documentService.UpdateDocument(doc);
                LoadDocuments(); // refresh DataGrid để badge hiển thị màu mới
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ SuggestStatusUpdateAfterExport error: {ex.Message}");
        }
    }

    private void DocumentDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (dgDocuments.SelectedItem is DocumentViewModel docVm)
            {
                var doc = _documentService.GetDocument(docVm.Id);
                if (doc != null)
                {
                    var viewer = new DocumentViewDialog(doc, _documentService);
                    viewer.ShowDialog();
                    if (viewer.IsEdited) LoadDocuments();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in DocumentDoubleClick: {ex.Message}");
            MessageBox.Show($"Lỗi khi mở văn bản:\n{ex.Message}", "Lỗi", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static string SanitizeFileName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "VanBan";
        }

        var invalidChars = System.IO.Path.GetInvalidFileNameChars();
        var sanitized = new string(name
            .Select(ch => invalidChars.Contains(ch) ? '-' : ch)
            .ToArray());

        return sanitized.Trim();
    }

    // Folder methods
    private void LoadFolders()
    {
        var allFolders = _documentService.GetAllFolders();
        var rootFolders = allFolders.Where(f => string.IsNullOrEmpty(f.ParentId)).ToList();

        var folderNodes = new ObservableCollection<FolderNode>();
        
        // Add "All Documents" node
        folderNodes.Add(new FolderNode 
        { 
            Id = "", 
            Name = "Tất cả văn bản", 
            IconKind = PackIconKind.FileMultiple,
            IconColor = "#1976D2",
            DocumentCount = _documentService.GetTotalDocuments()
        });

        foreach (var folder in rootFolders)
        {
            folderNodes.Add(BuildFolderTree(folder, allFolders));
        }

        tvFolders.ItemsSource = folderNodes;
    }

    private FolderNode BuildFolderTree(Folder folder, List<Folder> allFolders)
    {
        var node = new FolderNode
        {
            Id = folder.Id,
            Name = folder.Name,
            IconKind = GetFolderIcon(folder.Icon),
            IconColor = GetFolderColor(folder.Icon),
            DocumentCount = _documentService.GetDocumentsByFolder(folder.Id).Count
        };

        var children = allFolders.Where(f => f.ParentId == folder.Id);
        foreach (var child in children)
        {
            node.Children.Add(BuildFolderTree(child, allFolders));
        }

        return node;
    }
    
    private PackIconKind GetFolderIcon(string icon)
    {
        return icon switch
        {
            "📁" => PackIconKind.Folder,
            "📂" => PackIconKind.FolderOpen,
            "📋" => PackIconKind.ClipboardText,
            "📝" => PackIconKind.FileDocument,
            "📊" => PackIconKind.ChartBox,
            "📅" => PackIconKind.Calendar,
            "⚖️" => PackIconKind.Gavel,
            "👥" => PackIconKind.AccountMultiple,
            "💼" => PackIconKind.Briefcase,
            "🏛️" => PackIconKind.Domain,
            "📜" => PackIconKind.Script,
            "📄" => PackIconKind.FileOutline,
            _ => PackIconKind.Folder
        };
    }
    
    private string GetFolderColor(string icon)
    {
        return icon switch
        {
            "📁" => "#FFA726",
            "📂" => "#FF9800",
            "📋" => "#42A5F5",
            "📝" => "#66BB6A",
            "📊" => "#AB47BC",
            "📅" => "#26C6DA",
            "⚖️" => "#EF5350",
            "👥" => "#5C6BC0",
            "💼" => "#8D6E63",
            "🏛️" => "#78909C",
            "📜" => "#9575CD",
            "📄" => "#90A4AE",
            _ => "#FFA726"
        };
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is FolderNode node)
        {
            _selectedFolderId = node.Id;
            LoadDocuments();
        }
    }
    
    // NEW: Preview Panel handlers
    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            // Update bulk actions UI based on selection
            UpdateBulkActionsUI();
            
            // Show preview for single selection only
            if (dgDocuments.SelectedItems.Count == 1 && dgDocuments.SelectedItem is DocumentViewModel docVm)
            {
                ShowDocumentPreview(docVm);
            }
            else
            {
                HideDocumentPreview();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in DataGrid_SelectionChanged: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            MessageBox.Show($"Lỗi khi chọn văn bản:\n{ex.Message}\n\nType: {ex.GetType().Name}",
                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void ShowDocumentPreview(DocumentViewModel docVm)
    {
        try
        {
            if (docVm == null)
            {
                HideDocumentPreview();
                return;
            }
            
            // Get full document from database
            var doc = _documentService.GetDocument(docVm.Id);
            if (doc == null)
            {
                HideDocumentPreview();
                return;
            }
            
            // STEP 1: Hide everything and clear content
            if (emptyState != null) 
                emptyState.Visibility = Visibility.Collapsed;
            
            if (docContentCard != null)
                docContentCard.Visibility = Visibility.Collapsed;
            
            if (recipientsCard != null)
                recipientsCard.Visibility = Visibility.Collapsed;
            
            if (basedOnCard != null)
                basedOnCard.Visibility = Visibility.Collapsed;
            
            if (txtPreviewContent != null)
                txtPreviewContent.Text = string.Empty;
            
            if (txtPreviewRecipients != null)
                txtPreviewRecipients.Text = string.Empty;
            
            if (txtPreviewBasedOn != null)
                txtPreviewBasedOn.Text = string.Empty;
            
            // STEP 2: Show and populate info cards immediately
            if (docInfoCard != null) 
                docInfoCard.Visibility = Visibility.Visible;
            
            if (previewActions != null) 
                previewActions.Visibility = Visibility.Visible;
            
            // Update header and basic info
            if (txtPreviewHint != null) 
                txtPreviewHint.Text = $"Đang xem: {doc.Number ?? "N/A"}";
            
            if (txtPreviewNumber != null) 
                txtPreviewNumber.Text = $"Số: {doc.Number ?? "Chưa có"}";
            
            if (txtPreviewTitle != null) 
                txtPreviewTitle.Text = doc.Title ?? "Chưa có tiêu đề";
            
            if (txtPreviewType != null) 
                txtPreviewType.Text = GetDocumentTypeText(doc.Type);
            
            if (txtPreviewDate != null) 
                txtPreviewDate.Text = doc.IssueDate.ToString("dd/MM/yyyy");
            
            if (txtPreviewIssuer != null) 
                txtPreviewIssuer.Text = doc.Issuer ?? "Chưa có thông tin";
            
            if (txtPreviewStatus != null) 
                txtPreviewStatus.Text = $"{GetWorkflowStatusText(doc.WorkflowStatus)} · {GetDocumentStatusText(doc.IssueDate)}";
            
            // ═══════ Sổ theo dõi cá nhân ═══════
            if (personalTrackingCard != null)
            {
                personalTrackingCard.Visibility = Visibility.Visible;
                personalTrackingCard.Tag = doc.Id; // Lưu ID để dùng khi thêm note
                
                if (txtPreviewMyStatus != null)
                {
                    txtPreviewMyStatus.Text = doc.MyStatus.GetDisplayName();
                    txtPreviewMyStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                        doc.MyStatus switch
                        {
                            PersonalStatus.ChuaXuLy => System.Windows.Media.Color.FromRgb(158, 158, 158),
                            PersonalStatus.DangXuLy => System.Windows.Media.Color.FromRgb(251, 140, 0),
                            PersonalStatus.DaXuLy => System.Windows.Media.Color.FromRgb(67, 160, 71),
                            PersonalStatus.ChuyenTiep => System.Windows.Media.Color.FromRgb(30, 136, 229),
                            _ => System.Windows.Media.Color.FromRgb(158, 158, 158)
                        });
                }
                
                if (txtPreviewPriority != null)
                    txtPreviewPriority.Text = doc.PersonalPriority switch
                    {
                        1 => "⚪ Rất thấp",
                        2 => "🔵 Thấp",
                        3 => "🟡 Bình thường",
                        4 => "🟠 Cao",
                        5 => "🔴 Rất cao",
                        _ => "🟡 Bình thường"
                    };
                
                if (txtPreviewPersonalDeadline != null)
                    txtPreviewPersonalDeadline.Text = doc.PersonalDeadline?.ToString("dd/MM/yyyy") ?? "—";
                
                // Hiện ghi chú bút phê
                if (icPreviewNotes != null)
                {
                    var notes = (doc.Notes ?? new List<PersonalNoteEntry>())
                        .OrderByDescending(n => n.CreatedDate)
                        .Take(5)
                        .Select(n => new { n.Content, n.CreatedDate, TypeDisplay = n.Type.GetDisplayName() })
                        .ToList();
                    icPreviewNotes.ItemsSource = notes;
                }
                
                if (txtQuickNote != null) txtQuickNote.Text = string.Empty;
            }
            
            // Set button tags
            if (btnPreviewEdit != null) btnPreviewEdit.Tag = doc.Id;
            if (btnPreviewView != null) btnPreviewView.Tag = doc.Id;
            if (btnPreviewDelete != null) btnPreviewDelete.Tag = doc.Id;
            if (btnPreviewReview != null) btnPreviewReview.Tag = doc.Id;
            if (btnPreviewAdvisory != null) btnPreviewAdvisory.Tag = doc.Id;
            
            // STEP 3: Prepare content data
            var content = doc.Content ?? "Chưa có nội dung";
            if (content.Length > 1000)
            {
                content = content.Substring(0, 1000) + "\n\n... (xem đầy đủ nội dung bằng nút Mở)";
            }
            
            var hasRecipients = doc.Recipients != null && doc.Recipients.Length > 0;
            var recipientsText = hasRecipients ? string.Join("\n", doc.Recipients!) : string.Empty;
            
            var hasBasedOn = doc.BasedOn != null && doc.BasedOn.Length > 0;
            var basedOnText = hasBasedOn ? string.Join("\n", doc.BasedOn!) : string.Empty;
            
            // STEP 4: Use Dispatcher to show content AFTER UI has updated
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    // Show Recipients
                    if (hasRecipients)
                    {
                        if (txtPreviewRecipients != null)
                            txtPreviewRecipients.Text = recipientsText;
                        
                        if (recipientsCard != null)
                            recipientsCard.Visibility = Visibility.Visible;
                    }
                    
                    // Show Căn cứ
                    if (hasBasedOn)
                    {
                        if (txtPreviewBasedOn != null)
                            txtPreviewBasedOn.Text = basedOnText;
                        
                        if (basedOnCard != null)
                            basedOnCard.Visibility = Visibility.Visible;
                    }
                    
                    // Show Content - CRITICAL SECTION
                    if (txtPreviewContent != null)
                    {
                        txtPreviewContent.Text = content;
                    }
                    
                    if (docContentCard != null)
                    {
                        docContentCard.Visibility = Visibility.Visible;
                        docContentCard.InvalidateVisual();
                    }
                    
                    // Force complete layout update
                    UpdateLayout();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error in Dispatcher action: {ex.Message}");
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in ShowDocumentPreview: {ex.Message}");
            HideDocumentPreview();
        }
    }
    
    private void HideDocumentPreview()
    {
        try
        {
            if (docInfoCard != null) docInfoCard.Visibility = Visibility.Collapsed;
            if (personalTrackingCard != null) personalTrackingCard.Visibility = Visibility.Collapsed;
            if (docContentCard != null) docContentCard.Visibility = Visibility.Collapsed;
            if (recipientsCard != null) recipientsCard.Visibility = Visibility.Collapsed;
            if (previewActions != null) previewActions.Visibility = Visibility.Collapsed;
            if (emptyState != null) emptyState.Visibility = Visibility.Visible;
            if (txtPreviewHint != null) txtPreviewHint.Text = "Chọn văn bản để xem nội dung";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in HideDocumentPreview: {ex.Message}");
        }
    }

    /// <summary>Thêm ghi chú nhanh từ preview panel</summary>
    private void AddQuickNote_Click(object sender, RoutedEventArgs e)
    {
        AddQuickNoteFromPreview();
    }

    private void QuickNote_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            AddQuickNoteFromPreview();
            e.Handled = true;
        }
    }

    private void AddQuickNoteFromPreview()
    {
        var noteText = txtQuickNote?.Text?.Trim();
        if (string.IsNullOrEmpty(noteText)) return;
        
        var docId = personalTrackingCard?.Tag as string;
        if (string.IsNullOrEmpty(docId)) return;
        
        _documentService.AddNote(docId, noteText);
        txtQuickNote!.Text = string.Empty;
        
        // Refresh preview
        if (dgDocuments.SelectedItem is DocumentViewModel docVm)
            ShowDocumentPreview(docVm);
        
        Services.SnackbarHelper.ShowSuccess("Đã thêm ghi chú!");
    }
    
    /// <summary>
    /// Lấy tên hiển thị loại VB — delegate sang EnumDisplayHelper (đủ 29 loại, NĐ 30/2020)
    /// </summary>
    private string GetDocumentTypeText(DocumentType type) => type.GetDisplayName();
    
    private string GetDocumentStatusText(DateTime issueDate)
    {
        // Hiện thời gian tương đối thay vì status mâu thuẫn với workflow
        var daysSinceIssue = (DateTime.Now - issueDate).Days;
        if (daysSinceIssue == 0)
            return "Hôm nay";
        else if (daysSinceIssue == 1)
            return "Hôm qua";
        else if (daysSinceIssue <= 7)
            return $"{daysSinceIssue} ngày trước";
        else if (daysSinceIssue <= 30)
            return $"{daysSinceIssue / 7} tuần trước";
        else if (daysSinceIssue <= 365)
            return $"{daysSinceIssue / 30} tháng trước";
        else
            return issueDate.ToString("dd/MM/yyyy");
    }
    
    /// <summary>
    /// Lấy text trạng thái workflow để hiện trong Preview panel
    /// </summary>
    private string GetWorkflowStatusText(DocumentStatus status)
    {
        return status switch
        {
            DocumentStatus.Draft => "📝 Nháp",
            DocumentStatus.PendingApproval => "📤 Trình ký",
            DocumentStatus.Approved => "✅ Đã duyệt",
            DocumentStatus.Signed => "🖊️ Đã ký",
            DocumentStatus.Published => "📢 Đã phát hành",
            DocumentStatus.Sent => "📨 Đã gửi",
            DocumentStatus.Archived => "📁 Lưu trữ",
            _ => "—"
        };
    }
    
    private void SetupOrganization_Click(object sender, RoutedEventArgs e)
    {
        var setupService = new OrganizationSetupService(_documentService);
        var setupDialog = new OrganizationSetupDialog(setupService);
        
        if (setupDialog.ShowDialog() == true)
        {
            LoadFolders();
            Services.SnackbarHelper.ShowSuccess("Đã tạo lại cấu trúc thư mục!");
        }
    }

    private void AddFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Window
        {
            Title = "Thêm thư mục mới",
            Width = 400,
            Height = 250,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize
        };

        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var txtName = new TextBox { Margin = new Thickness(0, 5, 0, 15) };
        var txtIcon = new TextBox { Margin = new Thickness(0, 5, 0, 0), Text = "📁" };

        var lblName = new TextBlock { Text = "Tên thư mục:", FontWeight = FontWeights.SemiBold };
        var lblIcon = new TextBlock { Text = "Icon (emoji):", Margin = new Thickness(0, 10, 0, 0) };

        var inputStack = new StackPanel();
        inputStack.Children.Add(lblName);
        inputStack.Children.Add(txtName);
        inputStack.Children.Add(lblIcon);
        inputStack.Children.Add(txtIcon);
        Grid.SetRow(inputStack, 0);

        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 20, 0, 0)
        };
        Grid.SetRow(btnPanel, 1);

        var btnSave = new Button { Content = "Lưu", MinWidth = 80, Height = 36, Padding = new Thickness(16, 0, 16, 0), VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
        var btnCancel = new Button { Content = "Hủy", MinWidth = 80, Height = 36, Padding = new Thickness(16, 0, 16, 0), VerticalContentAlignment = VerticalAlignment.Center };

        btnSave.Click += (s, args) =>
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên thư mục!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var folder = new Folder
            {
                Name = txtName.Text.Trim(),
                Icon = string.IsNullOrWhiteSpace(txtIcon.Text) ? "📁" : txtIcon.Text.Trim(),
                ParentId = _selectedFolderId
            };

            _documentService.CreateFolder(folder);
            LoadFolders();
            dialog.DialogResult = true;
            dialog.Close();
        };

        btnCancel.Click += (s, args) => dialog.Close();

        btnPanel.Children.Add(btnSave);
        btnPanel.Children.Add(btnCancel);

        grid.Children.Add(inputStack);
        grid.Children.Add(btnPanel);
        dialog.Content = grid;

        dialog.ShowDialog();
    }
    
    // TreeView Expand/Collapse handlers
    private void ExpandAll_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SetTreeViewItemsExpandedState(tvFolders.Items, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error expanding tree: {ex.Message}");
        }
    }
    
    private void CollapseAll_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SetTreeViewItemsExpandedState(tvFolders.Items, false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error collapsing tree: {ex.Message}");
        }
    }
    
    private void SetTreeViewItemsExpandedState(System.Collections.IEnumerable items, bool isExpanded)
    {
        foreach (var item in items)
        {
            var treeViewItem = tvFolders.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.IsExpanded = isExpanded;
                
                // Recursively expand/collapse children
                if (item is FolderNode folderNode && folderNode.Children.Count > 0)
                {
                    SetTreeViewItemsExpandedState(folderNode.Children, isExpanded);
                }
            }
        }
    }
    
    // TreeView Context Menu handlers
    private void AddSubFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedFolder = tvFolders.SelectedItem as FolderNode;
            if (selectedFolder == null)
            {
                MessageBox.Show("Vui lòng chọn thư mục cha trước!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var dialog = CreateFolderInputDialog("Thêm thư mục con", "");
            if (dialog.ShowDialog() == true && dialog.Tag is string folderName && !string.IsNullOrWhiteSpace(folderName))
            {
                var folder = new Folder
                {
                    Name = folderName.Trim(),
                    Icon = "📁",
                    ParentId = selectedFolder.Id
                };
                
                _documentService.CreateFolder(folder);
                LoadFolders();
                MessageBox.Show($"✅ Đã tạo thư mục con '{folderName}'", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding subfolder: {ex.Message}");
            MessageBox.Show("Có lỗi khi tạo thư mục con!", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void RenameFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedFolder = tvFolders.SelectedItem as FolderNode;
            if (selectedFolder == null)
            {
                MessageBox.Show("Vui lòng chọn thư mục cần đổi tên!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var dialog = CreateFolderInputDialog("Đổi tên thư mục", selectedFolder.Name);
            if (dialog.ShowDialog() == true && dialog.Tag is string newName && !string.IsNullOrWhiteSpace(newName))
            {
                var folder = _documentService.GetFolderById(selectedFolder.Id);
                if (folder != null)
                {
                    folder.Name = newName.Trim();
                    _documentService.UpdateFolder(folder);
                    LoadFolders();
                    MessageBox.Show($"✅ Đã đổi tên thành '{newName}'", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error renaming folder: {ex.Message}");
            MessageBox.Show("Có lỗi khi đổi tên thư mục!", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void DeleteFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedFolder = tvFolders.SelectedItem as FolderNode;
            if (selectedFolder == null)
            {
                MessageBox.Show("Vui lòng chọn thư mục cần xóa!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var result = MessageBox.Show(
                $"⚠️ Bạn có chắc muốn xóa thư mục '{selectedFolder.Name}'?\n\n" +
                $"Thư mục có {selectedFolder.DocumentCount} văn bản.\n" +
                "Các văn bản sẽ được chuyển về thư mục gốc.",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                _documentService.DeleteFolder(selectedFolder.Id);
                LoadFolders();
                LoadDocuments();
                Services.SnackbarHelper.ShowSuccess("Đã xóa thư mục!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting folder: {ex.Message}");
            MessageBox.Show("Có lỗi khi xóa thư mục!", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private Window CreateFolderInputDialog(string title, string defaultValue)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 220,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize
        };
        
        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        
        var lblName = new TextBlock { Text = "Tên thư mục:", Margin = new Thickness(0, 0, 0, 5) };
        var txtName = new TextBox { Text = defaultValue, Margin = new Thickness(0, 0, 0, 15) };
        
        Grid.SetRow(lblName, 0);
        Grid.SetRow(txtName, 1);
        
        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 20, 0, 0)
        };
        Grid.SetRow(btnPanel, 3);
        
        var btnSave = new Button { Content = "Lưu", MinWidth = 80, Height = 36, Padding = new Thickness(16, 0, 16, 0), VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
        var btnCancel = new Button { Content = "Hủy", MinWidth = 80, Height = 36, Padding = new Thickness(16, 0, 16, 0), VerticalContentAlignment = VerticalAlignment.Center };
        
        btnSave.Click += (s, args) =>
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên thư mục!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            dialog.Tag = txtName.Text.Trim();
            dialog.DialogResult = true;
            dialog.Close();
        };
        
        btnCancel.Click += (s, args) => dialog.Close();
        
        btnPanel.Children.Add(btnSave);
        btnPanel.Children.Add(btnCancel);
        
        var stack = new StackPanel();
        stack.Children.Add(lblName);
        stack.Children.Add(txtName);
        
        grid.Children.Add(stack);
        grid.Children.Add(btnPanel);
        dialog.Content = grid;
        
        return dialog;
    }
    
    // DataGrid Sorting handler — dùng ICollectionView để sort đúng
    private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        try
        {
            e.Handled = true; // Ta tự xử lý sorting
            
            var column = e.Column;
            var sortPath = column.SortMemberPath;
            if (string.IsNullOrEmpty(sortPath)) return;
            
            // Toggle sort direction
            var direction = (column.SortDirection != System.ComponentModel.ListSortDirection.Ascending)
                ? System.ComponentModel.ListSortDirection.Ascending
                : System.ComponentModel.ListSortDirection.Descending;
            column.SortDirection = direction;
            
            // Clear other column sorts
            foreach (var col in dgDocuments.Columns)
            {
                if (col != column) col.SortDirection = null;
            }
            
            // Sort data using ICollectionView
            var view = System.Windows.Data.CollectionViewSource.GetDefaultView(dgDocuments.ItemsSource);
            if (view != null)
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new System.ComponentModel.SortDescription(sortPath, direction));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sorting: {ex.Message}");
        }
    }
    
    #region Sao văn bản — Điều 25-27, NĐ 30/2020
    
    /// <summary>
    /// Sao văn bản — Theo Điều 25-27, NĐ 30/2020/NĐ-CP
    /// </summary>
    private void CopyDocument_Click(object sender, RoutedEventArgs e)
    {
        var selected = dgDocuments.SelectedItems.Cast<DocumentViewModel>().ToList();
        if (selected.Count != 1)
        {
            MessageBox.Show("Vui lòng chọn đúng 1 văn bản để sao.", "Sao văn bản", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var doc = _documentService.GetDocument(selected[0].Id);
        if (doc == null) return;
        
        var dialog = new CopyDocumentDialog(doc, _documentService) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.CreatedCopy != null)
        {
            LoadDocuments();
            
            // Chọn bản sao vừa tạo
            var newCopy = dgDocuments.Items.Cast<DocumentViewModel>()
                .FirstOrDefault(vm => vm.Id == dialog.CreatedCopy.Id);
            if (newCopy != null) dgDocuments.SelectedItem = newCopy;
        }
    }
    
    #endregion
}