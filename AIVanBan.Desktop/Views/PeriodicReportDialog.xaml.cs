using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class PeriodicReportDialog : Window
{
    private readonly DocumentService _documentService;
    private readonly PeriodicReportService _reportService;
    private string _generatedContent = string.Empty;

    public Document? GeneratedDocument { get; private set; }

    public PeriodicReportDialog(DocumentService documentService)
    {
        InitializeComponent();
        _documentService = documentService;
        _reportService = new PeriodicReportService();

        LoadComboBoxes();
        LoadPreviousReports();
    }

    private void LoadComboBoxes()
    {
        // Loại kỳ
        cboPeriodType.ItemsSource = PeriodicReportService.GetPeriodTypes();
        cboPeriodType.SelectedIndex = 1; // Default: Tháng

        // Lĩnh vực
        cboField.ItemsSource = PeriodicReportService.GetCommonFields();
        cboField.SelectedIndex = 0; // Default: KT-XH

        // Default signer
        txtSignerTitle.Text = "Chủ tịch UBND";

        // Kịch bản mẫu — đủ phòng ban/đoàn thể cấp xã
        cboSampleScenario.ItemsSource = new[]
        {
            "📊 Kinh tế - Xã hội (tháng)",
            "📋 Cải cách hành chính (quý)",
            "💰 Tài chính - Ngân sách (năm)",
            "🛡️ An ninh - Trật tự (tháng)",
            "🌾 Nông thôn mới (6 tháng)",
            "🏥 Y tế - Dân số (quý)",
            "⚖️ Tư pháp - Hộ tịch (quý)",
            "🏗️ Địa chính - Xây dựng (quý)",
            "🎭 Văn hóa - Thông tin (tháng)",
            "📚 Giáo dục - Đào tạo (năm học)",
            "👷 Lao động - TBXH (quý)",
            "🎖️ Quốc phòng - Quân sự (6 tháng)",
            "🤝 Mặt trận Tổ quốc (năm)",
            "👩 Hội Liên hiệp Phụ nữ (quý)",
            "🌱 Hội Nông dân (quý)",
            "🧑‍🤝‍🧑 Đoàn Thanh niên (quý)",
            "⭐ Hội Cựu chiến binh (6 tháng)",
            "💻 Chuyển đổi số (quý)",
            "🏛️ Phòng chống tham nhũng (năm)"
        };
        cboSampleScenario.SelectedIndex = 0;
    }

    private void LoadPreviousReports()
    {
        // Lấy các báo cáo từ DB để user chọn
        var reports = _documentService.GetDocumentsByType(DocumentType.BaoCao)
            .OrderByDescending(d => d.IssueDate)
            .Take(20)
            .ToList();
        
        cboPreviousReport.ItemsSource = reports;
    }

    private void PeriodType_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (cboPeriodType.SelectedItem is string periodType)
        {
            var suggestions = PeriodicReportService.GetPeriodSuggestions(periodType);
            cboPeriod.ItemsSource = suggestions;
            if (suggestions.Count > 0)
                cboPeriod.SelectedIndex = 0;
        }
    }

    private void PreviousReport_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (cboPreviousReport.SelectedItem is Document doc)
        {
            txtPreviousReport.Text = doc.Content;
        }
    }

    private void LoadSample_Click(object sender, RoutedEventArgs e)
    {
        var scenarioIndex = cboSampleScenario.SelectedIndex;
        if (scenarioIndex < 0) scenarioIndex = 0;

        switch (scenarioIndex)
        {
            case 0: LoadSample_KTXH(); break;
            case 1: LoadSample_CCHC(); break;
            case 2: LoadSample_TaiChinh(); break;
            case 3: LoadSample_ANTT(); break;
            case 4: LoadSample_NTM(); break;
            case 5: LoadSample_YTe(); break;
            case 6: LoadSample_TuPhap(); break;
            case 7: LoadSample_DiaChinh(); break;
            case 8: LoadSample_VanHoa(); break;
            case 9: LoadSample_GiaoDuc(); break;
            case 10: LoadSample_LaoDong(); break;
            case 11: LoadSample_QuanSu(); break;
            case 12: LoadSample_MatTran(); break;
            case 13: LoadSample_PhuNu(); break;
            case 14: LoadSample_NongDan(); break;
            case 15: LoadSample_DoanTN(); break;
            case 16: LoadSample_CuuChienBinh(); break;
            case 17: LoadSample_ChuyenDoiSo(); break;
            case 18: LoadSample_PhongChongThamNhung(); break;
            default: LoadSample_KTXH(); break;
        }

        var scenarioName = cboSampleScenario.SelectedItem as string ?? "Demo";
        MessageBox.Show($"✅ Đã tải dữ liệu mẫu: {scenarioName}\n\nBấm \"🤖 Tạo báo cáo\" để xem kết quả.",
            "Demo", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SetPeriod(string periodType, string? field = null)
    {
        cboPeriodType.SelectedItem = periodType;
        var suggestions = PeriodicReportService.GetPeriodSuggestions(periodType);
        cboPeriod.ItemsSource = suggestions;
        if (suggestions.Count > 0) cboPeriod.SelectedIndex = 0;
        if (field != null) cboField.SelectedItem = field;
    }

    // ===== 6 KỊCH BẢN MẪU =====

    private void LoadSample_KTXH()
    {
        txtOrgName.Text = "UBND xã Gia Kiệm";
        SetPeriod("Tháng", "Kinh tế - Xã hội");
        txtSignerName.Text = "Nguyễn Văn Minh";
        txtSignerTitle.Text = "Chủ tịch UBND";

        txtRawData.Text = @"Thu ngân sách: 850 triệu đồng (tháng trước: 720 triệu, KH tháng: 800 triệu)
Chi ngân sách: 680 triệu đồng (tháng trước: 650 triệu)
Hộ nghèo: 45 hộ (tháng trước: 47 hộ)
Hộ cận nghèo: 62 hộ (tháng trước: 65 hộ)
Giải quyết TTHC: 312 hồ sơ (tháng trước: 280)
Hồ sơ trễ hạn: 5 hồ sơ
DVC trực tuyến mức 3,4: 420 hồ sơ (tháng trước: 380)
Tai nạn giao thông: 0 vụ
Vi phạm ANTT: 2 vụ (tháng trước: 4 vụ)
Trẻ em đến trường: 100%
Tiêm chủng đầy đủ: 98,5%
Hộ dân có nước sạch: 95,2% (tháng trước: 94,8%)
Lao động có việc làm mới: 15 người (tháng trước: 12)";

        txtPreviousReport.Text = "";
    }

    private void LoadSample_CCHC()
    {
        txtOrgName.Text = "UBND xã Gia Kiệm";
        SetPeriod("Quý", "Cải cách hành chính");
        txtSignerName.Text = "Trần Thị Mai";
        txtSignerTitle.Text = "Phó Chủ tịch UBND";

        txtRawData.Text = @"TTHC tiếp nhận: 890 hồ sơ (quý trước: 820)
Giải quyết đúng hạn: 871 hồ sơ
Trễ hạn: 19 hồ sơ (quý trước: 25)
Tỷ lệ đúng hạn: 97,9%
DVC trực tuyến mức 3,4: 420 hồ sơ (quý trước: 350)
Tỷ lệ DVC trực tuyến: 47,2%
Bộ phận 1 cửa: 3 cán bộ
Khảo sát hài lòng: 95,2% (quý trước: 93,8%)
Số người khảo sát: 450 người
Tập huấn CBCC: 2 lớp, 45 lượt người
Sáng kiến CCHC: 1 (số hóa sổ hộ tịch)
Văn bản điện tử: 95% (quý trước: 90%)
Họp trực tuyến: 8 cuộc (quý trước: 5)
Phần mềm quản lý VB: 100% sử dụng
CBCC vi phạm kỷ luật: 0
Kiến nghị của dân chưa giải quyết: 3 vụ (quý trước: 5)";

        txtPreviousReport.Text = @"Quý IV/2025: Tổng hồ sơ 820, đúng hạn 795, trễ 25. DVC trực tuyến 350 hồ sơ. Hài lòng 93,8%. Tập huấn 1 lớp 30 người. Văn bản điện tử 90%.";
    }

    private void LoadSample_TaiChinh()
    {
        txtOrgName.Text = "UBND xã Gia Kiệm";
        SetPeriod("Năm", "Tài chính - Ngân sách");
        txtSignerName.Text = "Lê Văn Tâm";
        txtSignerTitle.Text = "Chủ tịch UBND";

        txtRawData.Text = @"TỔNG THU NGÂN SÁCH: 9,8 tỷ đồng (KH: 10,2 tỷ)
Thu thuế: 3,2 tỷ (KH: 3,5 tỷ)
Thu phí, lệ phí: 1,1 tỷ (KH: 1,0 tỷ)
Thu từ đất: 2,8 tỷ (KH: 3,0 tỷ)
Thu khác: 2,7 tỷ (KH: 2,7 tỷ)
TỔNG CHI NGÂN SÁCH: 9,5 tỷ đồng
Chi thường xuyên: 7,2 tỷ (KH: 7,0 tỷ)
  - Chi lương, phụ cấp: 4,5 tỷ
  - Chi hoạt động: 1,8 tỷ
  - Chi sự nghiệp GD, YT: 0,9 tỷ
Chi đầu tư phát triển: 2,3 tỷ (KH: 3,0 tỷ)
  - Đường giao thông nông thôn: 1,2 tỷ
  - Sửa chữa trường học: 0,8 tỷ
  - Hệ thống thoát nước: 0,3 tỷ
KẾT DƯ: 300 triệu đồng
Nợ xây dựng cơ bản: 520 triệu (năm trước: 780 triệu)
Số đơn vị nộp thuế đầy đủ: 125/130 đơn vị";

        txtPreviousReport.Text = @"Năm 2024: Tổng thu 8,9 tỷ (KH 9,5 tỷ, đạt 93,7%). Tổng chi 8,6 tỷ. Chi TX 6,5 tỷ, Chi ĐTPT 2,1 tỷ. Kết dư 300 triệu. Nợ XDCB 780 triệu.";
    }

    private void LoadSample_ANTT()
    {
        txtOrgName.Text = "UBND xã Gia Kiệm";
        SetPeriod("Tháng", "An ninh - Trật tự");
        txtSignerName.Text = "Phạm Văn Đức";
        txtSignerTitle.Text = "Trưởng Công an xã";

        txtRawData.Text = @"Vụ việc hình sự: 0 vụ (tháng trước: 1 vụ)
Vi phạm ANTT: 2 vụ (tháng trước: 4 vụ)
  - Gây rối trật tự: 1 vụ
  - Đánh nhau: 1 vụ
Tai nạn giao thông: 1 vụ, 0 chết, 1 bị thương (tháng trước: 2 vụ, 0 chết, 2 bị thương)
Vi phạm ATGT: xử lý 15 trường hợp (tháng trước: 12)
  - Không đội MBH: 8
  - Nồng độ cồn: 4
  - Khác: 3
Số tiền phạt ATGT: 22,5 triệu đồng
Tuần tra, kiểm soát: 45 lượt (tháng trước: 40)
Hòa giải mâu thuẫn: 6 vụ, thành công 5 vụ (tháng trước: 4 vụ)
Tuyên truyền PL: 3 buổi, 280 lượt người (tháng trước: 2 buổi, 200 người)
Quản lý tạm trú, tạm vắng: 35 người (tháng trước: 28)
Camera an ninh hoạt động: 12/12 cái
Tổ tự quản ANTT: 8 tổ, hoạt động tốt
Tin báo tội phạm qua đường dây nóng: 3 tin (xác minh 2, không có cơ sở 1)";

        txtPreviousReport.Text = "";
    }

    private void LoadSample_NTM()
    {
        txtOrgName.Text = "UBND xã Gia Kiệm";
        SetPeriod("6 tháng", "Nông thôn mới");
        txtSignerName.Text = "Nguyễn Văn Minh";
        txtSignerTitle.Text = "Chủ tịch UBND";

        txtRawData.Text = @"TIÊU CHÍ NÔNG THÔN MỚI ĐẠT: 17/19 tiêu chí (đầu năm: 15/19)
Tiêu chí mới đạt trong kỳ:
  - TC 6 (Cơ sở vật chất văn hóa): hoàn thành nhà văn hóa thôn 3, 4
  - TC 10 (Thu nhập): bình quân đạt 68 triệu/người/năm (KH: 65 triệu)
Tiêu chí chưa đạt:
  - TC 17 (Môi trường): tỷ lệ thu gom rác 85% (yêu cầu 90%)
  - TC 18 (Hệ thống chính trị): còn thiếu 1 CB đạt chuẩn
Hạ tầng giao thông: bê tông hóa 92% đường liên thôn (đầu năm: 85%)
Km đường mới: 2,5 km
Hộ dân có nước sạch: 95,2% (đầu năm: 90%)
Hộ dân có nhà tiêu HVS: 97% (đầu năm: 95%)
Thu nhập bình quân: 68 triệu/người/năm (năm trước: 62 triệu)
Hộ nghèo: 1,2% (đầu năm: 1,8%)
Hộ cận nghèo: 2,5% (đầu năm: 3,1%)
Kinh phí đầu tư NTM trong kỳ: 3,5 tỷ
  - Ngân sách nhà nước: 2,0 tỷ
  - Xã hội hóa: 1,0 tỷ
  - Nhân dân đóng góp: 0,5 tỷ
Mô hình kinh tế hiệu quả: 3 mô hình (nuôi gà thả vườn, trồng bưởi da xanh, du lịch sinh thái)";

        txtPreviousReport.Text = @"6 tháng cuối 2025: Đạt 15/19 tiêu chí. Đường bê tông 85%. Nước sạch 90%. Hộ nghèo 1,8%. Thu nhập BQ 62 triệu/người. Kinh phí NTM: 2,8 tỷ.";
    }

    private void LoadSample_YTe()
    {
        txtOrgName.Text = "UBND xã Gia Kiệm";
        SetPeriod("Quý", "Y tế - Dân số");
        txtSignerName.Text = "Võ Thị Hương";
        txtSignerTitle.Text = "Trạm trưởng Y tế xã";

        txtRawData.Text = @"Dân số: 12.450 người (quý trước: 12.380)
Sinh: 38 trẻ (quý trước: 42)
  - Sinh con thứ 3+: 2 trường hợp
Tử: 8 người
Tiêm chủng mở rộng: 98,5% (quý trước: 97,2%)
Khám chữa bệnh tại trạm: 1.250 lượt (quý trước: 1.180)
  - Khám BHYT: 980 lượt
  - Khám ngoài BHYT: 270 lượt
Chuyển tuyến trên: 45 ca (quý trước: 52)
BHYT toàn dân: 92,3% (quý trước: 91,5%)
Bệnh truyền nhiễm: sốt xuất huyết 3 ca (quý trước: 8 ca), tay chân miệng 5 ca (quý trước: 2 ca)
Phun thuốc diệt muỗi: 2 đợt, 100% hộ dân
Khám thai định kỳ: 85 lượt (quý trước: 80)
Suy dinh dưỡng trẻ < 5 tuổi: 3,2% (quý trước: 3,5%)
VSATTP: kiểm tra 25 cơ sở, vi phạm 3 cơ sở (quý trước: kiểm tra 20, vi phạm 5)
Thuốc cơ bản đảm bảo: 95% danh mục
Cán bộ trạm y tế: 6/6 người (đủ biên chế)";

        txtPreviousReport.Text = @"Quý IV/2025: Dân số 12.380. Sinh 42, tử 10. Tiêm chủng 97,2%. Khám 1.180 lượt. BHYT 91,5%. SXH 8 ca, TCM 2 ca. SDD 3,5%.";
    }

    private void LoadSample_TuPhap()
    {
        txtOrgName.Text = "UBND xã Gia Kiệm";
        SetPeriod("Quý", "Tư pháp - Hộ tịch");
        txtSignerName.Text = "Hoàng Thị Lan";
        txtSignerTitle.Text = "Công chức Tư pháp - Hộ tịch";

        txtRawData.Text = @"ĐĂNG KÝ KHAI SINH: 35 trường hợp (quý trước: 40)
  - Đúng hạn: 33
  - Quá hạn: 2 (do phụ huynh nộp trễ)
Đăng ký khai tử: 8 trường hợp (quý trước: 10)
Đăng ký kết hôn: 12 cặp (quý trước: 15)
Cấp bản sao trích lục: 85 bản (quý trước: 72)
CHỨNG THỰC:
  - Chứng thực bản sao: 420 bộ (quý trước: 380)
  - Chứng thực chữ ký: 35 trường hợp (quý trước: 28)
  - Chứng thực hợp đồng: 18 hợp đồng (quý trước: 15)
Phí chứng thực thu được: 12,5 triệu đồng
HÒA GIẢI Ở CƠ SỞ:
  - Tiếp nhận: 8 vụ (quý trước: 6)
  - Hòa giải thành: 6 vụ
  - Chuyển cơ quan có thẩm quyền: 2 vụ
Tuyên truyền phổ biến PL: 4 buổi, 350 lượt người
Tổ hòa giải hoạt động: 5/5 tổ
Rà soát văn bản QPPL: 3 văn bản (kiến nghị sửa 1)
Hỗ trợ trợ giúp pháp lý: 5 trường hợp";

        txtPreviousReport.Text = @"Quý IV/2025: Khai sinh 40, khai tử 10, kết hôn 15. Chứng thực bản sao 380 bộ, chữ ký 28, hợp đồng 15. Hòa giải 6 vụ (thành 5). Tuyên truyền 3 buổi, 280 người.";
    }

    private void LoadSample_DiaChinh()
    {
        txtOrgName.Text = "UBND xã Gia Kiệm";
        SetPeriod("Quý", "Tài nguyên - Môi trường");
        txtSignerName.Text = "Đặng Văn Hùng";
        txtSignerTitle.Text = "Công chức Địa chính - Xây dựng";

        txtRawData.Text = @"HỒ SƠ ĐẤT ĐAI:
  - Cấp mới GCNQSDĐ: 25 hồ sơ (quý trước: 30)
  - Chuyển nhượng: 18 hồ sơ (quý trước: 22)
  - Thừa kế, tặng cho: 8 hồ sơ
  - Thế chấp, xóa thế chấp: 35 hồ sơ
  - Tồn đọng chưa giải quyết: 12 hồ sơ (quý trước: 15)
Tranh chấp đất đai: 4 vụ (quý trước: 6), giải quyết 3 vụ
XÂY DỰNG:
  - Cấp phép xây dựng: 15 giấy phép (quý trước: 12)
  - Vi phạm xây dựng: 2 trường hợp (xây không phép), xử lý 2
  - Công trình hoàn thành nghiệm thu: 8 công trình
MÔI TRƯỜNG:
  - Thu gom rác thải: 85% hộ dân (quý trước: 82%)
  - Đơn vị thu gom: 1 HTX + 2 tổ tự quản
  - Xử lý vi phạm MT: 1 cơ sở (chăn nuôi gây ô nhiễm)
  - Cây xanh trồng mới: 120 cây
Phí bảo vệ MT thu được: 45 triệu đồng (quý trước: 40 triệu)
Diện tích đất nông nghiệp chuyển mục đích: 0,5 ha";

        txtPreviousReport.Text = @"Quý IV/2025: Cấp GCNQSDĐ 30, chuyển nhượng 22. Tranh chấp 6 vụ (giải quyết 4). Cấp phép XD 12. Thu gom rác 82%. Phí BVMT 40 triệu.";
    }

    private void LoadSample_VanHoa()
    {
        txtOrgName.Text = "UBND xã Gia Kiệm";
        SetPeriod("Tháng", "Văn hóa - Thông tin");
        txtSignerName.Text = "Lê Thị Hoa";
        txtSignerTitle.Text = "Công chức Văn hóa - Xã hội";

        txtRawData.Text = @"HOẠT ĐỘNG VĂN HÓA:
  - Buổi sinh hoạt văn hóa cộng đồng: 4 buổi (tháng trước: 3)
  - Lượt người tham gia: 480 lượt
  - Nhà văn hóa thôn hoạt động: 5/5
  - CLB văn nghệ: 3 CLB, 85 thành viên
  - Đội văn nghệ biểu diễn: 2 buổi
THỂ DỤC THỂ THAO:
  - Giải thể thao cấp xã: 1 (bóng chuyền)
  - Người tập TDTT thường xuyên: 35% dân số (tháng trước: 33%)
THÔNG TIN - TRUYỀN THÔNG:
  - Bản tin phát thanh: 20 buổi phát (tháng trước: 18)
  - Tin bài đăng trang web xã: 8 tin
  - Tuyên truyền trực quan: 5 băng rôn, 20 tờ rơi
GIA ĐÌNH:
  - Gia đình văn hóa: 2.450/2.600 hộ (94,2%)
  - Thôn văn hóa: 4/5 thôn
  - Bạo lực gia đình: 1 vụ (tháng trước: 0), đã hòa giải
QUẢN LÝ DI TÍCH:
  - Di tích được bảo vệ: 2 di tích
  - Khách tham quan: 150 lượt";

        txtPreviousReport.Text = "";
    }

    private void LoadSample_GiaoDuc()
    {
        txtOrgName.Text = "UBND xã Gia Kiệm";
        SetPeriod("Năm", "Giáo dục - Đào tạo");
        txtSignerName.Text = "Nguyễn Văn Minh";
        txtSignerTitle.Text = "Chủ tịch UBND";

        txtRawData.Text = @"TRƯỜNG MẦM NON:
  - Số lớp: 8, học sinh: 185 (năm trước: 178)
  - Giáo viên: 12 (đạt chuẩn: 12/12)
  - Tỷ lệ huy động trẻ 3-5 tuổi: 95% (năm trước: 92%)
  - Trẻ suy dinh dưỡng: 3 trẻ (năm trước: 5)
  - Đạt chuẩn quốc gia: Mức 1
TRƯỜNG TIỂU HỌC:
  - Số lớp: 15, học sinh: 380 (năm trước: 365)
  - Giáo viên: 18 (đạt chuẩn: 18/18)
  - Tỷ lệ hoàn thành chương trình: 100%
  - Học sinh giỏi cấp tỉnh/thành phố: 12 em (năm trước: 8)
  - Bỏ học: 0
  - Đạt chuẩn quốc gia: Mức 2
TRƯỜNG THCS:
  - Số lớp: 12, học sinh: 320 (năm trước: 310)
  - Giáo viên: 22 (đạt chuẩn: 21/22)
  - Tỷ lệ tốt nghiệp: 100%
  - Học sinh giỏi cấp tỉnh/thành phố: 8 em (năm trước: 6)
  - Bỏ học: 2 em (năm trước: 3)
  - Đạt chuẩn quốc gia: Mức 1
PHỔ CẬP GIÁO DỤC: duy trì PCGD tiểu học mức 3, THCS mức 2
Xã hội hóa GD: huy động 350 triệu đồng (năm trước: 280 triệu)
Xây dựng CSVC: sửa chữa 2 phòng học, lắp 1 phòng tin học";

        txtPreviousReport.Text = @"Năm 2024-2025: MN 178 HS, TH 365 HS, THCS 310 HS. HS giỏi tỉnh TH 8, THCS 6. Bỏ học THCS 3 em. XHH 280 triệu.";
    }

    private void LoadSample_LaoDong()
    {
        txtOrgName.Text = "UBND xã Gia Kiệm";
        SetPeriod("Quý", "Lao động - TBXH");
        txtSignerName.Text = "Lê Thị Hoa";
        txtSignerTitle.Text = "Công chức Văn hóa - Xã hội";

        txtRawData.Text = @"LAO ĐỘNG - VIỆC LÀM:
  - Giải quyết việc làm mới: 45 người (quý trước: 38)
  - Xuất khẩu lao động: 3 người (quý trước: 2)
  - Đào tạo nghề: 1 lớp may CN, 25 học viên
  - Tỷ lệ lao động qua đào tạo: 52% (quý trước: 50%)
GIẢM NGHÈO:
  - Hộ nghèo: 45 hộ, tỷ lệ 1,7% (quý trước: 47 hộ, 1,8%)
  - Hộ cận nghèo: 62 hộ (quý trước: 65)
  - Hộ thoát nghèo trong kỳ: 2 hộ
  - Hộ nghèo phát sinh: 0
BẢO TRỢ XÃ HỘI:
  - Đối tượng BTXH đang hưởng: 120 người
  - Chi trả trợ cấp: 180 triệu đồng/quý
  - Cấp thẻ BHYT cho hộ nghèo: 45 thẻ
  - Hỗ trợ xây/sửa nhà: 2 hộ (KP: 80 triệu)
NGƯỜI CÓ CÔNG:
  - Gia đình chính sách: 85 hộ
  - Thăm tặng quà: 85 suất x 500k = 42,5 triệu (dịp 27/7)
  - Hỗ trợ sửa nhà NCC: 1 hộ (KP: 50 triệu)
TRẺ EM:
  - Trẻ em có hoàn cảnh đặc biệt: 8 em
  - Hỗ trợ học bổng: 15 em x 1 triệu
  - Tổ chức sân chơi: 2 buổi, 120 lượt trẻ";

        txtPreviousReport.Text = @"Quý IV/2025: Việc làm mới 38, XKLĐ 2. Hộ nghèo 47 (1,8%), cận nghèo 65. BTXH 120 người, chi 180 triệu. NCC 85 hộ.";
    }

    private void LoadSample_QuanSu()
    {
        txtOrgName.Text = "UBND xã Gia Kiệm";
        SetPeriod("6 tháng", "Quốc phòng - Quân sự");
        txtSignerName.Text = "Trần Văn Hải";
        txtSignerTitle.Text = "Chỉ huy trưởng BCH Quân sự xã";

        txtRawData.Text = @"QUÂN SỰ ĐỊA PHƯƠNG:
  - Lực lượng DQTV: 120 người (KH: 120)
  - Dân quân cơ động: 30 người
  - Dân quân tại chỗ: 90 người
  - Huấn luyện DQTV: 2 đợt, 120 lượt (đạt 100% KH)
TUYỂN QUÂN:
  - Chỉ tiêu giao quân 2026: 5 thanh niên
  - Đã giao quân đợt 1: 5/5 (đạt 100%)
  - Thanh niên đăng ký NVQS bổ sung: 45 người
PHÒNG THỦ DÂN SỰ:
  - Diễn tập PTDS: 1 cuộc (cấp xã)
  - Tập huấn PCTT: 2 buổi, 80 lượt CB + dân
  - Phương tiện PCCC: 15 bình (đã kiểm tra 100%)
CHÍNH SÁCH HẬU PHƯƠNG QUÂN ĐỘI:
  - Gia đình quân nhân: 35 hộ
  - Thăm hỏi gia đình quân nhân: 35 lượt
  - Hỗ trợ gia đình quân nhân khó khăn: 3 hộ x 2 triệu
GIÁO DỤC QUỐC PHÒNG:
  - Tuyên truyền GDQP: 3 buổi, 250 lượt người
  - Đối tượng 4 (cán bộ thôn, đoàn thể): bồi dưỡng 1 lớp, 30 người";

        txtPreviousReport.Text = @"6 tháng cuối 2025: DQTV 120 người, huấn luyện 2 đợt. Giao quân 5/5 đạt 100%. Diễn tập PTDS 1 cuộc. GĐ quân nhân 35 hộ. GDQP 2 buổi 200 người.";
    }

    private void LoadSample_MatTran()
    {
        txtOrgName.Text = "UB MTTQ Việt Nam xã Gia Kiệm";
        SetPeriod("Năm", "Công tác Đảng");
        txtSignerName.Text = "Phạm Thị Nga";
        txtSignerTitle.Text = "Chủ tịch UB MTTQ xã";

        txtRawData.Text = @"CÔNG TÁC MẶT TRẬN:
  - Cuộc họp Ban Thường trực: 12 cuộc
  - Hội nghị hiệp thương: 2 cuộc
  - Tiếp xúc cử tri: 4 đợt, 680 lượt cử tri
  - Ý kiến, kiến nghị cử tri: 45 ý kiến (đã giải quyết 38, đang xử lý 7)
VẬN ĐỘNG QUẦN CHÚNG:
  - Phong trào ""Toàn dân ĐK xây dựng đời sống VH"": 94,2% hộ GĐVH
  - ""Ngày vì người nghèo"": vận động 85 triệu đồng
  - Quỹ ""Vì người nghèo"": 85 triệu (năm trước: 72 triệu)
  - Xây nhà Đại đoàn kết: 2 nhà x 50 triệu = 100 triệu
  - Tặng quà Tết hộ nghèo: 50 suất x 500k = 25 triệu
GIÁM SÁT, PHẢN BIỆN:
  - Giám sát chuyên đề: 3 cuộc (công trình, ATVSTP, môi trường)
  - Phản biện dự thảo VB: 2 văn bản
  - Kiến nghị sau giám sát: 8 kiến nghị (thực hiện 6)
BAN TTND, BAN GSĐTCĐ:
  - Ban TTND: 5 người, hoạt động thường xuyên
  - Giám sát đầu tư: 3 công trình (phát hiện 1 sai sót, đã khắc phục)
CÔNG TÁC TÔN GIÁO, DÂN TỘC:
  - Cơ sở tôn giáo: 3 (1 chùa, 1 nhà thờ, 1 thánh thất)
  - Gặp mặt chức sắc: 2 lần/năm
  - Tình hình ANTT tôn giáo: ổn định";

        txtPreviousReport.Text = @"Năm 2024: Tiếp xúc cử tri 4 đợt, 620 lượt. Quỹ vì người nghèo 72 triệu. Xây 1 nhà ĐĐK. GS chuyên đề 2 cuộc. GĐVH 93,5%.";
    }

    private void LoadSample_PhuNu()
    {
        txtOrgName.Text = "Hội LHPN xã Gia Kiệm";
        SetPeriod("Quý", "Khác");
        txtSignerName.Text = "Nguyễn Thị Thanh";
        txtSignerTitle.Text = "Chủ tịch Hội LHPN xã";

        txtRawData.Text = @"TỔ CHỨC HỘI:
  - Hội viên: 1.850 người (quý trước: 1.820)
  - Kết nạp mới: 30 hội viên
  - Chi hội: 5 chi hội, hoạt động tốt 5/5
  - Sinh hoạt chi hội: 100% đúng định kỳ
PHONG TRÀO:
  - ""Phụ nữ tích cực học tập, LĐ sáng tạo"": 75% hội viên đăng ký
  - ""Xây dựng gia đình 5 không, 3 sạch"": 1.200 hộ đăng ký (quý trước: 1.150)
  - Gia đình hội viên đạt ""5 không 3 sạch"": 85%
HỖ TRỢ PHỤ NỮ:
  - Vốn vay ủy thác (TW Hội): 3,2 tỷ đồng, 180 hộ vay
  - Vốn vay ngân hàng CSXH: 2,8 tỷ, 150 hộ
  - Nợ quá hạn: 0
  - Dạy nghề: 1 lớp (đan lát), 25 chị
  - Giới thiệu việc làm: 12 chị (quý trước: 8)
  - Mô hình kinh tế: 3 tổ hợp tác (rau sạch, may gia công, nấu ăn)
BẢO VỆ QUYỀN LỢI PN-TE:
  - Tư vấn pháp luật: 8 trường hợp
  - Can thiệp bạo lực GĐ: 1 vụ
  - Hỗ trợ PN khó khăn: 5 suất x 1 triệu
TỪ THIỆN:
  - Quỹ hội: thu 25 triệu (quý trước: 20 triệu)
  - Tặng quà hội viên khó khăn: 10 suất";

        txtPreviousReport.Text = @"Quý IV/2025: HV 1.820, kết nạp 25. 5K3S 1.150 hộ. Vốn vay TW Hội 3,0 tỷ/170 hộ, NHCSXH 2,5 tỷ/140 hộ. Dạy nghề 1 lớp 20 chị. Quỹ hội 20 triệu.";
    }

    private void LoadSample_NongDan()
    {
        txtOrgName.Text = "Hội Nông dân xã Gia Kiệm";
        SetPeriod("Quý", "Nông nghiệp - Nông thôn");
        txtSignerName.Text = "Võ Văn Thắng";
        txtSignerTitle.Text = "Chủ tịch Hội Nông dân xã";

        txtRawData.Text = @"TỔ CHỨC HỘI:
  - Hội viên: 1.450 người (quý trước: 1.420)
  - Kết nạp mới: 30 hội viên
  - Chi hội: 5, hoạt động tốt: 5/5
  - Nông dân SXKD giỏi cấp xã: 85 hộ (quý trước: 80)
  - Nông dân SXKD giỏi cấp tỉnh/thành phố: 12 hộ
SẢN XUẤT NÔNG NGHIỆP:
  - Diện tích gieo trồng: 450 ha (quý trước: 420 ha)
  - Năng suất lúa bình quân: 6,2 tấn/ha (quý trước: 5,8 tấn)
  - Cây ăn quả: 120 ha (bưởi, sầu riêng, xoài)
  - Chăn nuôi: 350 hộ, 1.200 con heo, 800 con bò, 15.000 gia cầm
  - Thủy sản: 25 ha mặt nước, sản lượng 45 tấn
HỖ TRỢ NÔNG DÂN:
  - Vốn vay Quỹ HTND: 850 triệu, 45 hộ (quý trước: 780 triệu, 40 hộ)
  - Vốn vay NHCSXH: 2,5 tỷ, 130 hộ
  - Tập huấn KHKT: 3 lớp, 90 lượt nông dân
  - Chuyển giao công nghệ: 2 mô hình (tưới nhỏ giọt, phân vi sinh)
  - THT, HTX: 3 THT, 1 HTX nông nghiệp
PHONG TRÀO:
  - ""Nông dân thi đua SXKD giỏi"": 450 hộ đăng ký
  - ""Nông dân tham gia BVMT"": thu gom bao bì thuốc BVTV 95%
  - Quỹ hội: thu 18 triệu (quý trước: 15 triệu)";

        txtPreviousReport.Text = @"Quý IV/2025: HV 1.420. ND SXKD giỏi xã 80. Gieo trồng 420 ha, năng suất lúa 5,8 tấn/ha. Quỹ HTND 780 triệu/40 hộ. Tập huấn 2 lớp 60 ND.";
    }

    private void LoadSample_DoanTN()
    {
        txtOrgName.Text = "Đoàn TNCS HCM xã Gia Kiệm";
        SetPeriod("Quý", "Khác");
        txtSignerName.Text = "Trần Minh Tuấn";
        txtSignerTitle.Text = "Bí thư Đoàn xã";

        txtRawData.Text = @"TỔ CHỨC ĐOÀN:
  - Đoàn viên: 520 người (quý trước: 500)
  - Kết nạp mới: 20 đoàn viên
  - Chi đoàn: 8 chi đoàn (5 thôn + 3 trực thuộc)
  - Sinh hoạt đúng kỳ: 8/8 chi đoàn (100%)
  - Giới thiệu ĐV ưu tú vào Đảng: 2 người
HỘI LHTN:
  - Hội viên: 850 người
  - CLB thanh niên: 4 CLB (tình nguyện, khởi nghiệp, TDTT, văn nghệ)
HOẠT ĐỘNG TÌNH NGUYỆN:
  - Ngày TNTN: 4 buổi, 180 lượt ĐV-TN
  - Dọn vệ sinh đường làng: 3 buổi
  - Hiến máu: 25 đơn vị máu (quý trước: 20)
  - Trồng cây xanh: 80 cây
KHỞI NGHIỆP - LẬP NGHIỆP:
  - Thanh niên khởi nghiệp: 5 mô hình (quý trước: 3)
  - Hỗ trợ vốn vay TN: 350 triệu, 15 hộ
  - Giới thiệu việc làm: 18 thanh niên (quý trước: 12)
  - Dạy nghề: 1 lớp điện dân dụng, 20 TN
VĂN HÓA - THỂ THAO:
  - Giải bóng đá TN: 1 giải, 6 đội
  - Văn nghệ: 2 buổi
  - TN lập gia đình trước 18 tuổi: 0
PHÒNG CHỐNG TỆ NẠN XH:
  - TN cai nghiện thành công: 1 người
  - Tuyên truyền PCTNXH: 3 buổi, 200 lượt TN
  - TN vi phạm PL: 0";

        txtPreviousReport.Text = @"Quý IV/2025: ĐV 500, kết nạp 15. TNTN 3 buổi 150 lượt. Hiến máu 20 đơn vị. Khởi nghiệp 3 MH. Vốn vay TN 280 triệu/12 hộ. Việc làm 12 TN.";
    }

    private void LoadSample_CuuChienBinh()
    {
        txtOrgName.Text = "Hội CCB xã Gia Kiệm";
        SetPeriod("6 tháng", "Khác");
        txtSignerName.Text = "Lê Văn Dũng";
        txtSignerTitle.Text = "Chủ tịch Hội CCB xã";

        txtRawData.Text = @"TỔ CHỨC HỘI:
  - Hội viên: 280 người (đầu năm: 275)
  - Kết nạp mới: 5 hội viên (cựu quân nhân xuất ngũ)
  - Chi hội: 5 chi hội, hoạt động tốt: 5/5
  - Sinh hoạt đúng kỳ: 100%
  - Hội viên có hoàn cảnh khó khăn: 12 (đầu năm: 15)
PHONG TRÀO:
  - ""CCB gương mẫu"": 250/280 HV đăng ký (89%)
  - CCB SXKD giỏi cấp xã: 35 hội viên
  - CCB SXKD giỏi cấp tỉnh/thành phố: 5 hội viên
  - ""CCB giúp nhau giảm nghèo"": 3 hộ thoát nghèo
HỖ TRỢ HỘI VIÊN:
  - Quỹ hội: thu 22 triệu (6T đầu năm trước: 18 triệu)
  - Hỗ trợ HV khó khăn: 8 lượt x 500k = 4 triệu
  - Vốn vay NHCSXH: 1,2 tỷ, 65 hộ (đầu năm: 1,0 tỷ, 58 hộ)
  - Sửa nhà cho HV: 1 hộ (KP: 40 triệu, nguồn XHH)
THAM GIA XÂY DỰNG ĐỊA PHƯƠNG:
  - Giữ gìn ANTT: 8 HV tham gia Tổ tự quản
  - Hòa giải cơ sở: tham gia 3 vụ, thành 3
  - Vận động hiến đất làm đường: 2 hộ (120 m²)
  - Tuyên truyền PL: phối hợp 2 buổi
CÔNG TÁC CHÍNH SÁCH:
  - Thăm HV ốm đau: 15 lượt
  - Tặng quà ngày TBL 27/7: 280 suất x 200k
  - Phối hợp tìm mộ liệt sĩ: 1 trường hợp (đang xác minh)";

        txtPreviousReport.Text = @"6 tháng cuối 2025: HV 275. CCB gương mẫu 88%. SXKD giỏi xã 32. Quỹ hội 18 triệu. Vốn NHCSXH 1,0 tỷ/58 hộ. Thoát nghèo 2 hộ.";
    }

    private void LoadSample_ChuyenDoiSo()
    {
        txtOrgName.Text = "UBND xã Gia Kiệm";
        SetPeriod("Quý", "Chuyển đổi số");
        txtSignerName.Text = "Trần Thị Mai";
        txtSignerTitle.Text = "Phó Chủ tịch UBND";

        txtRawData.Text = @"CHÍNH QUYỀN SỐ:
  - Văn bản điện tử: 95% (quý trước: 90%)
  - Chữ ký số: 100% lãnh đạo, 85% CBCC (quý trước: 80%)
  - Họp trực tuyến: 8 cuộc (quý trước: 5)
  - Phần mềm quản lý VB: 100% CBCC sử dụng
  - DVC trực tuyến mức 3,4: 47,2% hồ sơ (quý trước: 40%)
  - Thanh toán không dùng tiền mặt (phí, lệ phí): 35% (quý trước: 25%)
KINH TẾ SỐ:
  - Hộ kinh doanh có tài khoản ngân hàng: 85% (quý trước: 80%)
  - Hộ KD bán hàng online: 45 hộ (quý trước: 35)
  - Sản phẩm OCOP lên sàn TMĐT: 3 sản phẩm
  - Giao dịch QR Code tại chợ: 12 hộ tiểu thương (quý trước: 5)
XÃ HỘI SỐ:
  - Tài khoản định danh điện tử (VNeID): 8.500/10.200 người (83%, quý trước: 75%)
  - Cài đặt app DVC: 4.200 người (quý trước: 3.500)
  - Tổ công nghệ số cộng đồng: 5 tổ, 25 thành viên
  - Hướng dẫn người dân DVC online: 120 lượt (quý trước: 80)
HẠ TẦNG SỐ:
  - Wifi công cộng: 3 điểm (trụ sở, chợ, NVH)
  - Camera an ninh kết nối: 12/12
  - Hộ có internet: 88% (quý trước: 85%)
TẬP HUẤN:
  - CBCC: 1 lớp CĐS, 25 người
  - Người dân: 3 buổi hướng dẫn DVC, VNeID, thanh toán số";

        txtPreviousReport.Text = @"Quý IV/2025: VB điện tử 90%, CKS 80% CBCC. DVC TT 40%. VNeID 75%. App DVC 3.500 người. KD online 35 hộ. Internet 85%.";
    }

    private void LoadSample_PhongChongThamNhung()
    {
        txtOrgName.Text = "UBND xã Gia Kiệm";
        SetPeriod("Năm", "Phòng chống tham nhũng");
        txtSignerName.Text = "Nguyễn Văn Minh";
        txtSignerTitle.Text = "Chủ tịch UBND";

        txtRawData.Text = @"CÔNG KHAI, MINH BẠCH:
  - Công khai ngân sách xã: 4 lần/năm (đúng quy định)
  - Công khai quy hoạch, kế hoạch SDĐ: 2 lần
  - Công khai đầu tư công: 100% dự án
  - Niêm yết TTHC: 100% (153 TTHC)
  - Kê khai tài sản, thu nhập: 8/8 CB thuộc diện (100%)
KIỂM TRA, GIÁM SÁT:
  - Kiểm tra nội bộ: 2 cuộc (TC, XD)
  - Giám sát HĐND xã: 3 cuộc
  - Giám sát MTTQ + đoàn thể: 3 cuộc
  - Thanh tra nhân dân: 2 cuộc
  - Kiến nghị sau kiểm tra: 5 kiến nghị (thực hiện 5/5)
TIẾP CÔNG DÂN:
  - Lịch tiếp CD của CT UBND: 12 buổi/năm (đúng QĐ)
  - Lượt công dân tiếp: 45 lượt (năm trước: 52)
  - Đơn thư khiếu nại: 3 đơn (năm trước: 5)
  - Đơn tố cáo: 1 đơn (năm trước: 2)
  - Giải quyết đúng hạn: 4/4 đơn (100%)
PHÁT HIỆN THAM NHŨNG:
  - Vụ việc tham nhũng: 0 vụ (năm trước: 0)
  - Vi phạm về kê khai TS: 0
  - Chuyển cơ quan điều tra: 0
TUYÊN TRUYỀN PCTN:
  - Phổ biến Luật PCTN: 2 buổi, 120 lượt CBCC + dân
  - Lồng ghép trong họp thôn: 5 buổi
  - Đánh giá công tác PCTN cấp tỉnh/thành phố: Xếp loại TỐT";

        txtPreviousReport.Text = @"Năm 2024: Công khai NS 4 lần. Kê khai TS 8/8. KT nội bộ 2 cuộc. Tiếp CD 52 lượt. KNTC 7 đơn, giải quyết 7/7. Tham nhũng 0 vụ. Xếp loại TỐT.";
    }

    /// <summary>
    /// Tự động lấy số liệu từ sổ văn bản (LiteDB) để điền vào ô Số liệu
    /// </summary>
    private void AutoFillStats_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var periodType = cboPeriodType.SelectedItem as string ?? "Tháng";
            var reportPeriod = cboPeriod.Text;

            if (string.IsNullOrWhiteSpace(reportPeriod))
            {
                MessageBox.Show("Vui lòng chọn kỳ báo cáo trước!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var stats = PeriodicReportService.ExtractStatsFromDB(periodType, reportPeriod);

            if (string.IsNullOrWhiteSpace(stats))
            {
                MessageBox.Show("Không tìm thấy dữ liệu văn bản nào trong kỳ này.\n" +
                    "Hãy nhập số liệu thủ công hoặc chọn kỳ khác.", "Không có dữ liệu",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Nếu đã có nội dung, hỏi ghi đè hay nối thêm
            if (!string.IsNullOrWhiteSpace(txtRawData.Text))
            {
                var result = MessageBox.Show(
                    "Ô số liệu đã có nội dung.\n\n" +
                    "• Bấm YES để thay thế toàn bộ\n" +
                    "• Bấm NO để nối thêm vào cuối\n" +
                    "• Bấm Cancel để hủy",
                    "Đã có số liệu", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel) return;
                if (result == MessageBoxResult.No)
                {
                    txtRawData.Text = txtRawData.Text.TrimEnd() + "\n\n--- Số liệu từ sổ VB ---\n" + stats;
                    return;
                }
            }

            txtRawData.Text = stats;

            MessageBox.Show($"Đã lấy số liệu từ sổ văn bản cho kỳ: {reportPeriod}",
                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi lấy số liệu: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Generate_Click(object sender, RoutedEventArgs e)
    {
        if (!AiPromoHelper.CheckOrShowPromo(this)) return;

        // Validate
        if (string.IsNullOrWhiteSpace(txtRawData.Text))
        {
            MessageBox.Show("Vui lòng nhập số liệu!", "Thiếu thông tin", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtOrgName.Text))
        {
            MessageBox.Show("Vui lòng nhập tên đơn vị!", "Thiếu thông tin", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Confirm before calling AI
        var periodType = cboPeriodType.SelectedItem as string ?? "Tháng";
        var period = cboPeriod.Text;
        var field = cboField.Text;
        var hasPrevious = !string.IsNullOrWhiteSpace(txtPreviousReport.Text);

        var confirmMsg = $"📊 Tạo báo cáo {periodType.ToLower()} — {field}\n" +
                         $"📅 Kỳ: {period}\n" +
                         $"🏛️ Đơn vị: {txtOrgName.Text}\n" +
                         $"📋 So sánh kỳ trước: {(hasPrevious ? "Có" : "Không")}\n\n" +
                         "Bấm OK để gọi AI tạo báo cáo.";

        if (MessageBox.Show(confirmMsg, "Xác nhận tạo báo cáo", 
            MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            return;

        // Show loading
        LoadingPanel.Visibility = Visibility.Visible;
        PlaceholderPanel.Visibility = Visibility.Collapsed;
        ResultCard.Visibility = Visibility.Collapsed;
        ActionButtons.Visibility = Visibility.Collapsed;
        btnGenerate.IsEnabled = false;

        try
        {
            _generatedContent = await _reportService.GenerateReportAsync(
                periodType,
                period,
                field,
                txtOrgName.Text,
                txtRawData.Text,
                string.IsNullOrWhiteSpace(txtPreviousReport.Text) ? null : txtPreviousReport.Text,
                txtSignerName.Text,
                txtSignerTitle.Text
            );

            // Clean AI output: xóa header/footer nếu AI vẫn tạo, xử lý literal \n
            _generatedContent = CleanAIContent(_generatedContent);

            // Display result
            DisplayResult(_generatedContent);
            
            LoadingPanel.Visibility = Visibility.Collapsed;
            ResultCard.Visibility = Visibility.Visible;
            ActionButtons.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            LoadingPanel.Visibility = Visibility.Collapsed;
            PlaceholderPanel.Visibility = Visibility.Visible;

            MessageBox.Show($"Lỗi khi gọi AI:\n\n{ex.Message}", "Lỗi AI",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            btnGenerate.IsEnabled = true;
        }
    }

    /// <summary>
    /// Làm sạch nội dung AI trả về:
    /// 1. Xử lý literal \n thành newline thật
    /// 2. Xóa header thể thức nếu AI vẫn tạo (quốc hiệu, tên cơ quan, số VB, trích yếu)
    /// 3. Xóa footer (nơi nhận, chữ ký) nếu AI vẫn tạo
    /// </summary>
    private string CleanAIContent(string content)
    {
        if (string.IsNullOrEmpty(content)) return content;

        // 1. Xử lý literal \n thành newline thật
        content = content.Replace("\\n", "\n");

        // 2. Xóa các dòng header thể thức (nếu AI vẫn tạo)
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
        
        // Tìm và xóa các dòng header từ đầu
        var headerPatterns = new[]
        {
            "CỘNG HÒA XÃ HỘI", "Độc lập - Tự do", "ỦY BAN NHÂN DÂN", "UBND ",
            "Số: ", "Số:", "BÁO CÁO", "Kính gửi:",
            "───", "---"
        };

        // Xóa header lines từ đầu (tối đa 20 dòng đầu)
        int headerEnd = 0;
        for (int i = 0; i < Math.Min(lines.Count, 25); i++)
        {
            var trimmed = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                headerEnd = i + 1;
                continue;
            }
            
            bool isHeader = headerPatterns.Any(p => trimmed.StartsWith(p, StringComparison.OrdinalIgnoreCase)
                                                  || trimmed.Contains(p));
            
            // Dòng ngày tháng: "Gia Kiệm, ngày ... tháng ..."
            if (trimmed.Contains(", ngày") && trimmed.Contains("tháng") && trimmed.Contains("năm"))
                isHeader = true;
            
            // Trích yếu: "Về kết quả..." hoặc "V/v ..." ngay sau BÁO CÁO
            if (i > 0 && lines[i - 1].Trim().StartsWith("BÁO CÁO") && !string.IsNullOrWhiteSpace(trimmed))
                isHeader = true;

            if (isHeader)
            {
                headerEnd = i + 1;
            }
            else if (headerEnd > 0)
            {
                break; // Đã qua header, dừng
            }
        }

        // Xóa footer: "Nơi nhận:", chức danh, "(Đã ký)", tên người ký
        int footerStart = lines.Count;
        for (int i = lines.Count - 1; i >= Math.Max(0, lines.Count - 15); i--)
        {
            var trimmed = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                footerStart = i;
                continue;
            }

            bool isFooter = trimmed.StartsWith("Nơi nhận") ||
                           trimmed.StartsWith("- Như trên") ||
                           trimmed.StartsWith("- UBND") ||
                           trimmed.StartsWith("- Lưu:") ||
                           trimmed.StartsWith("- Thường trực") ||
                           trimmed.Contains("(Đã ký)") ||
                           trimmed.StartsWith("CÔNG CHỨC") ||
                           trimmed.StartsWith("CHỦ TỊCH") ||
                           trimmed.StartsWith("PHÓ CHỦ TỊCH") ||
                           trimmed.StartsWith("TRƯỞNG") ||
                           trimmed.StartsWith("BÍ THƯ") ||
                           trimmed.StartsWith("TRẠM TRƯỞNG") ||
                           trimmed.StartsWith("CHỈ HUY");

            if (isFooter)
            {
                footerStart = i;
            }
            else
            {
                break;
            }
        }

        // Lấy phần nội dung giữa header và footer
        var bodyLines = lines.Skip(headerEnd).Take(footerStart - headerEnd).ToList();

        // Trim empty lines ở đầu/cuối
        while (bodyLines.Count > 0 && string.IsNullOrWhiteSpace(bodyLines[0]))
            bodyLines.RemoveAt(0);
        while (bodyLines.Count > 0 && string.IsNullOrWhiteSpace(bodyLines[bodyLines.Count - 1]))
            bodyLines.RemoveAt(bodyLines.Count - 1);

        return string.Join("\n", bodyLines);
    }

    private void DisplayResult(string content)
    {
        var flowDoc = new FlowDocument();
        flowDoc.PagePadding = new Thickness(20);

        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            var para = new Paragraph(new Run(line));
            para.FontFamily = new System.Windows.Media.FontFamily("Times New Roman");

            var trimmed = line.Trim();

            // Heading: "Phần I", "Phần II", "Phần III" — in đậm, căn giữa
            if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^Phần\s+[IVX]+"))
            {
                para.FontWeight = FontWeights.Bold;
                para.TextAlignment = TextAlignment.Center;
                para.FontSize = 14;
            }
            // Sub-heading in hoa: "KẾT QUẢ THỰC HIỆN", "ĐÁNH GIÁ CHUNG"...
            else if (trimmed.Length > 5 && trimmed.Length < 80 && trimmed == trimmed.ToUpper() && !trimmed.StartsWith("-"))
            {
                para.FontWeight = FontWeights.Bold;
                para.TextAlignment = TextAlignment.Center;
                para.FontSize = 13;
            }
            // Numbered sections: "1. ", "2. ", "I. ", "II. "
            else if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^(\d+\.|[IVX]+\.)"))
            {
                para.FontWeight = FontWeights.Bold;
                para.FontSize = 13;
                para.TextAlignment = TextAlignment.Justify;
            }
            else
            {
                para.FontSize = 13;
                para.TextAlignment = TextAlignment.Justify;
            }

            flowDoc.Blocks.Add(para);
        }

        ResultRichTextBox.Document = flowDoc;
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_generatedContent))
        {
            Clipboard.SetText(_generatedContent);
            MessageBox.Show("✅ Đã copy nội dung báo cáo!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void ExportWord_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_generatedContent))
        {
            MessageBox.Show("Chưa có nội dung để xuất!", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var period = cboPeriod.Text.Replace("/", "-").Replace(" ", "_");
            var field = cboField.Text.Replace(" ", "_").Replace("-", "");

            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                DefaultExt = ".docx",
                FileName = $"BaoCao_{field}_{period}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                // Sử dụng WordExportService chuẩn TT01/2011 (reusable)
                var wordService = new WordExportService();
                wordService.ExportContent(saveDialog.FileName, _generatedContent,
                    new WordExportService.ExportContentOptions
                    {
                        OrgName = txtOrgName.Text,
                        DocumentTypeName = "BÁO CÁO",
                        Subject = $"Tình hình {cboField.Text.ToLower()} {cboPeriod.Text.ToLower()}",
                        SignerName = txtSignerName.Text,
                        SignerTitle = txtSignerTitle.Text,
                        IssueDate = DateTime.Now
                    });

                MessageBox.Show($"✅ Đã xuất file Word chuẩn TT01/2011!\n\n{saveDialog.FileName}",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = saveDialog.FileName,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SaveAsDocument_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_generatedContent))
        {
            MessageBox.Show("Chưa có nội dung để lưu!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var period = cboPeriod.Text;
        var field = cboField.Text;

        GeneratedDocument = new Document
        {
            Title = $"Báo cáo {field} {period}",
            Type = DocumentType.BaoCao,
            Content = _generatedContent,
            Issuer = txtOrgName.Text,
            Subject = $"Báo cáo tình hình {field.ToLower()} {period.ToLower()}",
            CreatedDate = DateTime.Now,
            IssueDate = DateTime.Now,
            WorkflowStatus = DocumentStatus.Draft,
            Tags = new[] { "AI Generated", "Báo cáo định kỳ", field }
        };

        _documentService.AddDocument(GeneratedDocument);

        MessageBox.Show($"✅ Đã lưu báo cáo vào kho văn bản!\n\n📄 {GeneratedDocument.Title}",
            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
