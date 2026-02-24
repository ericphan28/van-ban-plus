using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

/// <summary>
/// Unified Setup Wizard — thiết lập cơ quan lần đầu.
/// Tạo đồng thời: cấu trúc thư mục tài liệu + album ảnh + cấu hình cơ quan.
/// </summary>
public partial class UnifiedSetupWizard : Window
{
    private readonly DocumentService _documentService;
    private readonly AlbumStructureService _albumService;
    private readonly OrganizationSetupService _orgSetupService;
    private readonly MeetingService _meetingService;
    
    private int _currentStep = 1;
    private OrganizationType _selectedOrgType;
    
    /// <summary>
    /// True nếu user đã hoàn thành wizard (cả 2 cấu trúc đã được tạo)
    /// </summary>
    public bool SetupCompleted { get; private set; }

    public UnifiedSetupWizard(
        DocumentService documentService, 
        AlbumStructureService albumService)
    {
        InitializeComponent();
        
        _documentService = documentService;
        _albumService = albumService;
        _orgSetupService = new OrganizationSetupService(documentService);
        _meetingService = new MeetingService();
    }
    
    private void OrganizationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cboOrganizationType.SelectedItem is ComboBoxItem selected && selected.Tag != null)
        {
            var tag = selected.Tag.ToString() ?? "";
            if (!string.IsNullOrEmpty(tag) && Enum.TryParse<OrganizationType>(tag, out var orgType))
            {
                _selectedOrgType = orgType;
                btnNext.IsEnabled = true;
            }
            else
            {
                btnNext.IsEnabled = false;
            }
        }
        else
        {
            btnNext.IsEnabled = false;
        }
    }
    
    private void OrgName_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Enable Next only when name is entered
        if (_currentStep == 2)
        {
            btnNext.IsEnabled = !string.IsNullOrWhiteSpace(txtOrgFullName.Text);
        }
    }
    
    private void Next_Click(object sender, RoutedEventArgs e)
    {
        if (_currentStep == 1)
        {
            // Moving from step 1 → 2: auto-fill org info
            AutoFillOrgInfo();
        }
        else if (_currentStep == 2)
        {
            // Moving from step 2 → 3: build preview
            BuildPreview();
        }
        
        _currentStep++;
        UpdateStepUI();
    }
    
    private void Back_Click(object sender, RoutedEventArgs e)
    {
        _currentStep--;
        UpdateStepUI();
    }
    
    private void UpdateStepUI()
    {
        // Hide all panels
        step1Panel.Visibility = Visibility.Collapsed;
        step2Panel.Visibility = Visibility.Collapsed;
        step3Panel.Visibility = Visibility.Collapsed;
        
        // Step indicators - default gray
        stepIndicator1.Background = new SolidColorBrush(Color.FromRgb(0x90, 0xCA, 0xF9));
        stepIndicator2.Background = new SolidColorBrush(Color.FromRgb(0x90, 0xCA, 0xF9));
        stepIndicator3.Background = new SolidColorBrush(Color.FromRgb(0x90, 0xCA, 0xF9));
        
        switch (_currentStep)
        {
            case 1:
                step1Panel.Visibility = Visibility.Visible;
                stepIndicator1.Background = new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0));
                btnBack.Visibility = Visibility.Collapsed;
                btnNext.Visibility = Visibility.Visible;
                btnComplete.Visibility = Visibility.Collapsed;
                btnNext.IsEnabled = cboOrganizationType.SelectedItem is ComboBoxItem sel && sel.Tag != null;
                break;
                
            case 2:
                step2Panel.Visibility = Visibility.Visible;
                stepIndicator1.Background = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32)); // green = done
                stepIndicator2.Background = new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0));
                btnBack.Visibility = Visibility.Visible;
                btnNext.Visibility = Visibility.Visible;
                btnComplete.Visibility = Visibility.Collapsed;
                btnNext.IsEnabled = !string.IsNullOrWhiteSpace(txtOrgFullName.Text);
                break;
                
            case 3:
                step3Panel.Visibility = Visibility.Visible;
                stepIndicator1.Background = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
                stepIndicator2.Background = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
                stepIndicator3.Background = new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0));
                btnBack.Visibility = Visibility.Visible;
                btnNext.Visibility = Visibility.Collapsed;
                btnComplete.Visibility = Visibility.Visible;
                break;
        }
    }
    
    /// <summary>
    /// Auto-fill tên cơ quan và ký hiệu viết tắt dựa trên loại CQ đã chọn
    /// Theo Phụ lục VI, NĐ 30/2020/NĐ-CP
    /// </summary>
    private void AutoFillOrgInfo()
    {
        // Auto-fill ký hiệu viết tắt CQ
        txtOrgAbbreviation.Text = _selectedOrgType switch
        {
            OrganizationType.UbndXa or OrganizationType.UbndTinh => "UBND",
            OrganizationType.HdndXa or OrganizationType.HdndTinh => "HĐND",
            OrganizationType.VanPhong => "VP",
            OrganizationType.TrungTamHanhChinh => "TTHCC",
            OrganizationType.DangUyXa => "ĐU",
            OrganizationType.DangUyTinh => "TU",
            OrganizationType.ChiBoDang => "CB",
            OrganizationType.DangBo => "ĐB",
            OrganizationType.BanDanVan => "BDV",
            OrganizationType.BanToChuc => "BTC",
            OrganizationType.BanTuyenGiao => "BTG",
            OrganizationType.BanKiemTra => "UBKT",
            OrganizationType.BanNoiChinh => "BNC",
            OrganizationType.BanKinhTe => "BKT",
            OrganizationType.BanVanHoa => "BVHXH",
            OrganizationType.MatTran => "UBMTTQ",
            OrganizationType.HoiNongDan => "HND",
            OrganizationType.HoiPhuNu => "HPN",
            OrganizationType.DoanThanhNien => "ĐTN",
            OrganizationType.HoiCuuChienBinh => "HCCB",
            OrganizationType.CongDoan => "CĐ",
            OrganizationType.HoiChapThap => "HCTĐ",
            OrganizationType.HoiKhuyenHoc => "HKH",
            OrganizationType.CongAn => "CA",
            OrganizationType.TruongMamNon => "TMN",
            OrganizationType.TruongTieuHoc => "TTH",
            OrganizationType.TruongTHCS => "THCS",
            OrganizationType.TruongTHPT => "THPT",
            OrganizationType.TruongDaiHoc => "ĐH",
            OrganizationType.TramYTe => "TYT",
            OrganizationType.TrungTamYTe => "TTYT",
            OrganizationType.BenhVien => "BV",
            _ => "CQ"
        };
        
        // Auto-fill tên gợi ý
        if (string.IsNullOrWhiteSpace(txtOrgFullName.Text))
        {
            txtOrgFullName.Text = _selectedOrgType switch
            {
                OrganizationType.UbndXa => "ỦY BAN NHÂN DÂN XÃ ...",
                OrganizationType.UbndTinh => "ỦY BAN NHÂN DÂN TỈNH ...",
                OrganizationType.HdndXa => "HỘI ĐỒNG NHÂN DÂN XÃ ...",
                OrganizationType.HdndTinh => "HỘI ĐỒNG NHÂN DÂN TỈNH ...",
                OrganizationType.VanPhong => "VĂN PHÒNG UBND ...",
                OrganizationType.TrungTamHanhChinh => "TRUNG TÂM HÀNH CHÍNH CÔNG ...",
                OrganizationType.DangUyXa => "ĐẢNG ỦY XÃ ...",
                OrganizationType.DangUyTinh => "TỈNH ỦY ...",
                OrganizationType.ChiBoDang => "CHI BỘ ĐẢNG ...",
                OrganizationType.DangBo => "ĐẢNG BỘ ...",
                OrganizationType.MatTran => "ỦY BAN MẶT TRẬN TỔ QUỐC ...",
                OrganizationType.HoiNongDan => "HỘI NÔNG DÂN ...",
                OrganizationType.HoiPhuNu => "HỘI LIÊN HIỆP PHỤ NỮ ...",
                OrganizationType.DoanThanhNien => "ĐOÀN TNCS HỒ CHÍ MINH ...",
                OrganizationType.HoiCuuChienBinh => "HỘI CỰU CHIẾN BINH ...",
                OrganizationType.CongAn => "CÔNG AN ...",
                OrganizationType.TruongMamNon => "TRƯỜNG MẦM NON ...",
                OrganizationType.TruongTieuHoc => "TRƯỜNG TIỂU HỌC ...",
                OrganizationType.TruongTHCS => "TRƯỜNG THCS ...",
                OrganizationType.TruongTHPT => "TRƯỜNG THPT ...",
                OrganizationType.TruongDaiHoc => "TRƯỜNG ĐẠI HỌC ...",
                OrganizationType.TramYTe => "TRẠM Y TẾ ...",
                OrganizationType.TrungTamYTe => "TRUNG TÂM Y TẾ ...",
                OrganizationType.BenhVien => "BỆNH VIỆN ...",
                _ => ""
            };
        }
    }
    
    /// <summary>
    /// Build preview cho Step 3: hiển thị tóm tắt cả 2 cấu trúc sẽ tạo
    /// </summary>
    private void BuildPreview()
    {
        // Summary
        txtSummaryOrgName.Text = $"🏛️ Tên: {txtOrgFullName.Text}";
        txtSummaryOrgType.Text = $"📋 Loại: {GetOrgTypeDisplayName(_selectedOrgType)}";
        txtSummaryAbbrev.Text = $"✏️ Ký hiệu: {txtOrgAbbreviation.Text}";
        
        // Document structure preview (tùy theo org type)
        txtDocStructurePreview.Text = GetDocStructurePreviewText();
        
        // Album structure preview
        txtAlbumStructurePreview.Text = GetAlbumStructurePreviewText();
    }
    
    private string GetDocStructurePreviewText()
    {
        return "📂 01. VĂN BẢN ĐẾN (theo năm)\n" +
               "📂 02. VĂN BẢN ĐI (theo năm)\n" +
               "📂 03. HÀNH CHÍNH - TỔ CHỨC\n" +
               "📂 04. TÀI CHÍNH - KẾ TOÁN\n" +
               "📂 05. BIÊN BẢN - HỘI NGHỊ\n" +
               "📂 06. ĐẤT ĐAI / CHUYÊN MÔN\n" +
               "📂 07. MẪU VĂN BẢN\n" +
               "📂 08. BÁO CÁO - THỐNG KÊ\n" +
               "📂 09. TÀI LIỆU HỌC TẬP\n" +
               "📂 10. LƯU TRỮ\n" +
               "📂 11. CÁ NHÂN\n" +
               $"\n→ Tổng: ~100+ thư mục con";
    }
    
    private string GetAlbumStructurePreviewText()
    {
        var templateKey = AlbumStructureService.MapOrgTypeToTemplateKey(_selectedOrgType);
        var templates = _albumService.GetAllTemplates();
        var template = templates.FirstOrDefault(t => t.OrganizationType == templateKey);
        
        if (template != null)
        {
            var lines = template.Categories
                .OrderBy(c => c.SortOrder)
                .Select(c => $"{c.Icon} {c.Name} ({c.SubCategories.Count})")
                .ToList();
            
            var totalSub = template.Categories.Sum(c => c.SubCategories.Count);
            return string.Join("\n", lines) + $"\n\n→ Tổng: {template.Categories.Count} danh mục, {totalSub} phân loại";
        }
        
        return "🖼️ Sự kiện - Hội nghị\n" +
               "🏗️ Công trình - Dự án\n" +
               "📅 Hoạt động thường xuyên\n" +
               "🎊 Văn hóa - Lễ hội\n" +
               "... và nhiều danh mục khác";
    }
    
    private string GetOrgTypeDisplayName(OrganizationType orgType)
    {
        return orgType switch
        {
            OrganizationType.UbndXa => "UBND Xã/Phường",
            OrganizationType.UbndTinh => "UBND Tỉnh/TP",
            OrganizationType.HdndXa => "HĐND Xã/Phường",
            OrganizationType.HdndTinh => "HĐND Tỉnh/TP",
            OrganizationType.DangUyXa => "Đảng ủy Xã/Phường",
            OrganizationType.DangUyTinh => "Tỉnh ủy/Thành ủy",
            OrganizationType.MatTran => "Mặt trận Tổ quốc",
            OrganizationType.HoiNongDan => "Hội Nông dân",
            OrganizationType.HoiPhuNu => "Hội Phụ nữ",
            OrganizationType.DoanThanhNien => "Đoàn Thanh niên",
            OrganizationType.CongAn => "Công an",
            _ => orgType.ToString()
        };
    }
    
    private async void Complete_Click(object sender, RoutedEventArgs e)
    {
        ProgressDialog? progressDialog = null;
        
        try
        {
            // Capture UI values before background task
            var orgName = txtOrgFullName.Text.Trim();
            var orgType = _selectedOrgType;
            var abbreviation = txtOrgAbbreviation.Text?.Trim();
            var seedDemo = chkSeedDemoData.IsChecked == true;
            
            // Disable buttons
            btnComplete.IsEnabled = false;
            btnBack.IsEnabled = false;
            
            // Show progress
            progressDialog = new ProgressDialog("Đang tạo cấu trúc cơ quan...");
            progressDialog.Show();
            
            await Task.Run(() =>
            {
                // 1. Tạo cấu trúc thư mục tài liệu
                Console.WriteLine("📂 [UnifiedWizard] Creating document folder structure...");
                _orgSetupService.CreateDefaultStructure(orgName, orgType, abbreviation);
                
                // 2. Kích hoạt album template phù hợp
                Console.WriteLine("🖼️ [UnifiedWizard] Activating album template...");
                _albumService.ActivateTemplateByOrgType(orgType);
                
                // 3. Tạo dữ liệu demo nếu user chọn
                if (seedDemo)
                {
                    Console.WriteLine("📄 [UnifiedWizard] Seeding demo documents...");
                    var seedService = new SeedDataService(_documentService);
                    seedService.GenerateDemoDocuments();
                    
                    Console.WriteLine("📅 [UnifiedWizard] Seeding demo meetings...");
                    var meetingSeeder = new MeetingSeeder(_meetingService);
                    meetingSeeder.SeedDemoMeetings();
                }
            });
            
            progressDialog.Close();
            
            SetupCompleted = true;
            
            var demoText = seedDemo 
                ? "📄 Đã tạo 50 văn bản demo + 17 cuộc họp mẫu\n" 
                : "";
            
            MessageBox.Show(
                $"✅ THIẾT LẬP HOÀN TẤT!\n\n" +
                $"🏛️ {orgName}\n" +
                $"📂 Đã tạo cấu trúc thư mục tài liệu\n" +
                $"🖼️ Đã kích hoạt cấu trúc album ảnh\n" +
                $"✏️ Ký hiệu CQ: {abbreviation}\n" +
                $"{demoText}\n" +
                $"Bạn có thể bắt đầu sử dụng ngay!",
                "Thiết lập thành công",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            if (progressDialog != null && progressDialog.IsVisible)
                progressDialog.Close();
            
            Console.WriteLine($"❌ [UnifiedWizard] Error: {ex.Message}\n{ex.StackTrace}");
            
            MessageBox.Show(
                $"❌ Lỗi khi thiết lập cơ quan:\n\n{ex.Message}",
                "Lỗi",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            
            btnComplete.IsEnabled = true;
            btnBack.IsEnabled = true;
        }
    }
    
    private void Skip_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Bạn có chắc muốn bỏ qua thiết lập?\n\n" +
            "Bạn có thể thiết lập sau tại:\n" +
            "• Trang Tài liệu → nút \"Thiết lập cơ quan\"\n" +
            "• Trang Album → nút \"Cấu hình Album\"",
            "Bỏ qua thiết lập",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No);
        
        if (result == MessageBoxResult.Yes)
        {
            SetupCompleted = false;
            DialogResult = false;
            Close();
        }
    }
}
