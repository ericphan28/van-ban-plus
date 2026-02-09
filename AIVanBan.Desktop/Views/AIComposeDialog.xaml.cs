using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Input;
using System.Printing;
using System.IO.Compression;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using MaterialDesignThemes.Wpf;
using System.Linq;

namespace AIVanBan.Desktop.Views;

public partial class AIComposeDialog : Window
{
    private readonly DocumentService _documentService;
    private readonly GeminiAIService _aiService;
    private DocumentTemplate? _selectedTemplate;
    private readonly Dictionary<string, TextBox> _fieldInputs = new();

    public Document? GeneratedDocument { get; private set; }

    public AIComposeDialog(DocumentService documentService, string? geminiApiKey = null)
    {
        InitializeComponent();
        _documentService = documentService;
        _aiService = string.IsNullOrEmpty(geminiApiKey) ? new GeminiAIService() : new GeminiAIService(geminiApiKey);
        
        LoadTemplates();
    }

    private void LoadTemplates()
    {
        var templates = _documentService.GetAllTemplates();
        TemplateComboBox.ItemsSource = templates.OrderBy(t => t.Type).ThenBy(t => t.Name);
        
        if (templates.Count > 0)
        {
            TemplateComboBox.SelectedIndex = 0;
        }
    }

    private void TemplateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedTemplate = TemplateComboBox.SelectedItem as DocumentTemplate;
        
        if (_selectedTemplate == null)
        {
            TemplateInfoPanel.Visibility = Visibility.Collapsed;
            InputFieldsPanel.Children.Clear();
            _fieldInputs.Clear();
            GenerateButton.IsEnabled = false;
            if (ViewTemplateButton != null)
                ViewTemplateButton.IsEnabled = false;
            return;
        }

        // Hiển thị thông tin template
        TemplateInfoPanel.Visibility = Visibility.Visible;
        TemplateDescription.Text = $"📋 {_selectedTemplate.Description}";
        
        // Enable view template button
        if (ViewTemplateButton != null)
            ViewTemplateButton.IsEnabled = true;
        
        if (_selectedTemplate.RequiredFields != null && _selectedTemplate.RequiredFields.Length > 0)
        {
            RequiredFieldsText.Text = $"✅ Các trường cần nhập: {string.Join(", ", _selectedTemplate.RequiredFields)}";
        }
        else
        {
            RequiredFieldsText.Text = "";
        }

        // Tạo input fields
        CreateInputFields();
        GenerateButton.IsEnabled = true;
    }

    private void SampleScenarioComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Enable load button when scenario is selected
        if (LoadSampleButton != null)
            LoadSampleButton.IsEnabled = SampleScenarioComboBox.SelectedItem != null;
    }

    private void CreateInputFields()
    {
        InputFieldsPanel.Children.Clear();
        _fieldInputs.Clear();

        if (_selectedTemplate?.RequiredFields == null) return;

        foreach (var field in _selectedTemplate.RequiredFields)
        {
            var card = new Card
            {
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var stackPanel = new StackPanel();

            // Label
            var label = new TextBlock
            {
                Text = GetFieldLabel(field),
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            };
            stackPanel.Children.Add(label);

            // TextBox
            var textBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                MinHeight = GetFieldHeight(field),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            
            // Enable Vietnamese input
            InputMethod.SetIsInputMethodEnabled(textBox, true);
            
            HintAssist.SetHint(textBox, GetFieldHint(field));
            textBox.Style = (Style)FindResource("MaterialDesignOutlinedTextBox");
            
            stackPanel.Children.Add(textBox);
            card.Content = stackPanel;
            
            InputFieldsPanel.Children.Add(card);
            _fieldInputs[field] = textBox;
        }
    }
    
    private void LoadSample_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTemplate == null || _selectedTemplate.RequiredFields == null) return;
        
        var selectedScenario = (SampleScenarioComboBox.SelectedItem as ComboBoxItem)?.Tag as string;
        if (string.IsNullOrEmpty(selectedScenario))
        {
            MessageBox.Show("Vui lòng chọn kịch bản mẫu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var samples = GetScenarioSamples(selectedScenario);
        
        foreach (var field in _selectedTemplate.RequiredFields)
        {
            if (_fieldInputs.TryGetValue(field, out var textBox))
            {
                if (samples.TryGetValue(field, out var value))
                {
                    textBox.Text = value;
                }
            }
        }
        
        MessageBox.Show($"✅ Đã tải dữ liệu mẫu cho kịch bản: {(SampleScenarioComboBox.SelectedItem as ComboBoxItem)?.Content}", 
            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private Dictionary<string, string> GetScenarioSamples(string scenario)
    {
        return scenario switch
        {
            // Công văn xin hỗ trợ kinh phí
            "cv_kinhi" => new Dictionary<string, string>
            {
                {"from_org", "UBND xã Tân Thành"},
                {"to_org", "UBND huyện Bình Chánh"},
                {"to_department", "Sở Tài chính TP.HCM"},
                {"subject", "Đề nghị hỗ trợ kinh phí xây dựng đường giao thông nông thôn"},
                {"content", "Hiện nay, tuyến đường liên xã Tân Thành - Long Phước dài 2,5km đang trong tình trạng xuống cấp nghiêm trọng, gây khó khăn cho việc đi lại của nhân dân. UBND xã Tân Thành kính đề nghị UBND huyện xem xét hỗ trợ kinh phí xây dựng, cải tạo tuyến đường theo dự toán đính kèm."},
                {"signer_name", "Nguyễn Văn Minh"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Công văn báo cáo tiến độ
            "cv_baocao" => new Dictionary<string, string>
            {
                {"from_org", "UBND xã Tân Thành"},
                {"to_org", "UBND huyện Bình Chánh"},
                {"subject", "Báo cáo tiến độ thực hiện Chương trình xây dựng nông thôn mới quý I/2026"},
                {"content", "Thực hiện Chương trình xây dựng nông thôn mới năm 2026, trong quý I, UBND xã Tân Thành đã hoàn thành 8/10 tiêu chí đề ra. Cụ thể: hoàn thành 100% công trình hạ tầng giao thông, 95% hộ dân có nhà tiêu hợp vệ sinh, 100% trẻ em được tiêm chủng đầy đủ. Hiện còn 2 tiêu chí về kinh tế hộ và môi trường đang trong quá trình triển khai."},
                {"proposal", "Đề nghị UBND huyện tiếp tục hỗ trợ về kinh phí và chuyên môn để xã hoàn thành các tiêu chí còn lại"},
                {"signer_name", "Trần Thị Mai"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Công văn trả lời
            "cv_traloi" => new Dictionary<string, string>
            {
                {"from_org", "UBND xã Tân Thành"},
                {"to_org", "Sở Nông nghiệp và Phát triển nông thôn"},
                {"reply_to_number", "145/SNN-PTNT ngày 25/01/2026"},
                {"subject", "Trả lời về việc báo cáo tình hình dịch bệnh gia súc"},
                {"content", "Trả lời Công văn số 145/SNN-PTNT ngày 25/01/2026 của Sở Nông nghiệp và Phát triển nông thôn về việc báo cáo tình hình dịch bệnh gia súc, UBND xã Tân Thành xin báo cáo như sau:\n\nTrên địa bàn xã hiện có 350 hộ chăn nuôi với tổng đàn 1.200 con lợn, 800 con gia cầm. Trong tháng qua, không phát hiện dịch bệnh nào trên đàn gia súc. 100% hộ chăn nuôi đã được tập huấn về phòng chống dịch bệnh và thực hiện tiêm phòng định kỳ."},
                {"signer_name", "Lê Văn Tâm"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Quyết định khen thưởng
            "qd_khenthuong" => new Dictionary<string, string>
            {
                {"award_type", "Bằng khen của UBND huyện"},
                {"recipient", "Tập thể Ban Văn hóa - Xã hội xã Tân Thành"},
                {"achievement", "Đã có thành tích xuất sắc trong công tác tuyên truyền, vận động nhân dân tham gia các phong trào văn hóa, thể thao năm 2025. Đạt danh hiệu Làng văn hóa tiêu biểu cấp huyện 3 năm liền (2023-2025)"},
                {"signer_name", "Phạm Văn Đức"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Quyết định điều động
            "qd_dieudonng" => new Dictionary<string, string>
            {
                {"person_name", "Võ Thị Hương"},
                {"current_position", "Công chức Văn phòng UBND"},
                {"from_unit", "Văn phòng UBND xã Tân Thành"},
                {"to_unit", "Phòng Tài chính - Kế hoạch xã Tân Thành"},
                {"new_position", "Công chức Phòng Tài chính - Kế hoạch"},
                {"effective_date", "01/03/2026"},
                {"signer_name", "Nguyễn Văn Minh"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Quyết định thành lập
            "qd_thanhlap" => new Dictionary<string, string>
            {
                {"org_name", "Ban Chỉ đạo phòng chống dịch Covid-19 xã Tân Thành"},
                {"members", "1. Ông Nguyễn Văn Minh - Chủ tịch UBND xã - Trưởng ban\n2. Bà Trần Thị Mai - Phó Chủ tịch UBND xã - Phó ban\n3. Ông Lê Văn Tâm - Trưởng Công an xã - Ủy viên\n4. Bà Võ Thị Hương - Trạm trưởng Y tế xã - Ủy viên\n5. Ông Phạm Văn Đức - Chủ tịch Hội Nông dân xã - Ủy viên"},
                {"tasks", "- Chỉ đạo, điều hành công tác phòng chống dịch Covid-19 trên địa bàn xã\n- Triển khai các biện pháp giám sát, cách ly, xét nghiệm\n- Tuyên truyền nâng cao ý thức người dân\n- Báo cáo định kỳ về UBND huyện"},
                {"signer_name", "Nguyễn Văn Minh"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Quyết định phê duyệt
            "qd_pheduyet" => new Dictionary<string, string>
            {
                {"project_name", "Dự án xây dựng trường mầm non Tân Thành B"},
                {"objectives", "Xây dựng trường mầm non 3 tầng, quy mô 6 phòng học, đáp ứng nhu cầu học tập cho 180 trẻ em trên địa bàn"},
                {"budget", "8 tỷ đồng từ nguồn ngân sách huyện và xã hội hóa"},
                {"implementing_unit", "Phòng Giáo dục và Đào tạo huyện Bình Chánh"},
                {"signer_name", "Nguyễn Văn Minh"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Báo cáo tổng kết
            "bc_tongket" => new Dictionary<string, string>
            {
                {"org_name", "UBND xã Tân Thành"},
                {"period", "Năm 2025"},
                {"achievements", "- Tốc độ tăng trưởng kinh tế đạt 12%, vượt 2% so với kế hoạch\n- Hoàn thành 18/19 tiêu chí xây dựng nông thôn mới\n- 100% trẻ em trong độ tuổi được đến trường\n- Thu nhập bình quân đầu người đạt 65 triệu đồng/năm\n- Tỷ lệ hộ nghèo giảm còn 1,2%\n- An ninh chính trị, trật tự an toàn xã hội ổn định"},
                {"challenges", "- Một số tuyến đường liên thôn chưa được bê tông hóa\n- Thiếu đất để xây dựng nhà văn hóa thôn\n- Nguồn vốn xã hội hóa huy động chưa đạt kế hoạch\n- Còn 15 hộ nghèo chưa có nhà ở kiên cố"},
                {"future_plans", "- Huy động nguồn lực hoàn thành tiêu chí nông thôn mới nâng cao\n- Tập trung phát triển kinh tế hộ, tăng thu nhập người dân\n- Đẩy mạnh xã hội hóa trong đầu tư hạ tầng\n- Hỗ trợ 100% hộ nghèo có nhà ở kiến cố trong năm 2026"},
                {"signer_name", "Nguyễn Văn Minh"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Báo cáo tình hình
            "bc_tinhhinh" => new Dictionary<string, string>
            {
                {"org_name", "UBND xã Tân Thành"},
                {"field", "An ninh trật tự tháng 1/2026"},
                {"situation", "Trong tháng 1/2026, tình hình an ninh trật tự trên địa bàn xã cơ bản ổn định. Không xảy ra các vụ việc nghiêm trọng. Công tác tuần tra, kiểm soát được duy trì thường xuyên."},
                {"results", "- Giải quyết 5 vụ việc tranh chấp đất đai, gia đình\n- Phát hiện và xử lý 2 trường hợp vi phạm TTATGT\n- Tuyên truyền phổ biến pháp luật cho 450 người dân\n- Tổ chức ký cam kết không vi phạm pháp luật cho 120 hộ dân"},
                {"proposals", "- Tăng cường tuần tra vào dịp Tết Nguyên đán\n- Đề nghị huyện hỗ trợ thêm thiết bị camera an ninh\n- Mở thêm lớp tuyên truyền phổ biến pháp luật cho thanh niên"},
                {"signer_name", "Lê Văn Tâm"},
                {"signer_title", "Trưởng Công an xã"}
            },
            
            // Tờ trình xin ý kiến
            "tt_yikien" => new Dictionary<string, string>
            {
                {"org_name", "UBND xã Tân Thành"},
                {"recipient", "UBND huyện Bình Chánh"},
                {"subject", "Xin ý kiến về phương án di dời chợ xã"},
                {"reason", "Chợ xã Tân Thành hiện đặt tại trung tâm, gây ùn tắc giao thông và mất vệ sinh môi trường. Nhân dân kiến nghị di dời để cải thiện diện mạo khu vực."},
                {"content", "UBND xã đề xuất 2 phương án:\n\nPhương án 1: Di dời chợ về khu đất 2.000m² tại thôn 3, cách trung tâm xã 500m. Ưu điểm: gần khu dân cư, thuận lợi giao thông. Nhược điểm: cần bồi thường giải phóng mặt bằng.\n\nPhương án 2: Nâng cấp chợ hiện tại, mở rộng thêm 500m². Ưu điểm: không phải di dời, tiết kiệm chi phí. Nhược điểm: vẫn còn tình trạng ùn tắc."},
                {"signer_name", "Nguyễn Văn Minh"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Tờ trình đề xuất
            "tt_dexuat" => new Dictionary<string, string>
            {
                {"org_name", "UBND xã Tân Thành"},
                {"recipient", "UBND huyện Bình Chánh"},
                {"proposal", "Đề xuất dự án xây dựng nhà văn hóa đa năng xã Tân Thành"},
                {"reason", "Nhà văn hóa xã hiện nay xuống cấp nghiêm trọng, không đáp ứng nhu cầu sinh hoạt văn hóa của nhân dân. Xã cần xây dựng nhà văn hóa mới quy mô 500m², 2 tầng để phục vụ các hoạt động văn hóa, thể thao, họp dân."},
                {"budget", "6 tỷ đồng (trong đó: ngân sách huyện 4 tỷ, ngân sách xã 1 tỷ, xã hội hóa 1 tỷ)"},
                {"signer_name", "Nguyễn Văn Minh"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Kế hoạch công tác
            "kh_congtac" => new Dictionary<string, string>
            {
                {"org_name", "UBND xã Tân Thành"},
                {"period", "Năm 2026"},
                {"objectives", "Phấn đấu hoàn thành 19/19 tiêu chí nông thôn mới, tốc độ tăng trưởng kinh tế đạt 13%, thu nhập bình quân 70 triệu đồng/người/năm"},
                {"tasks", "Quý I:\n- Hoàn thiện hồ sơ xét duyệt nông thôn mới nâng cao\n- Triển khai 3 công trình hạ tầng (đường, cầu, hệ thống thoát nước)\n\nQuý II:\n- Tổ chức hội nghị biểu dương điển hình tiên tiến\n- Kiểm tra giám sát thực hiện nhiệm vụ các thôn\n\nQuý III:\n- Tổng kết đánh giá 6 tháng đầu năm\n- Điều chỉnh kế hoạch nếu cần thiết\n\nQuý IV:\n- Hoàn thành các công trình còn lại\n- Tổng kết công tác năm 2026"},
                {"signer_name", "Nguyễn Văn Minh"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Kế hoạch tổ chức sự kiện
            "kh_sukien" => new Dictionary<string, string>
            {
                {"event_name", "Lễ hội Văn hóa - Thể thao xã Tân Thành năm 2026"},
                {"time_place", "Thời gian: Từ 8h00 ngày 15/02/2026 đến 17h00 ngày 16/02/2026\nĐịa điểm: Sân vận động xã Tân Thành"},
                {"purpose", "Chào mừng Đảng, chào mừng Xuân mới, tăng cường đoàn kết, phát huy truyền thống văn hóa dân tộc, động viên cán bộ và nhân dân phấn đấu hoàn thành nhiệm vụ năm 2026"},
                {"program", "Ngày 15/02:\n- 8h00: Lễ khai mạc, văn nghệ chào mừng\n- 9h00: Thi đấu bóng đá nam\n- 14h00: Thi đấu cầu lông, bóng chuyền\n- 19h00: Văn nghệ quần chúng\n\nNgày 16/02:\n- 8h00: Chung kết các môn thể thao\n- 14h00: Trao giải thưởng\n- 16h00: Lễ bế mạc"},
                {"budget", "50 triệu đồng (ngân sách xã 30 triệu, tài trợ doanh nghiệp 20 triệu)"},
                {"signer_name", "Phạm Văn Đức"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Thông báo họp
            "tb_hop" => new Dictionary<string, string>
            {
                {"meeting_name", "Hội nghị triển khai nhiệm vụ trọng tâm quý I/2026"},
                {"time", "8h00 thứ Hai, ngày 10/02/2026"},
                {"location", "Phòng họp UBND xã Tân Thành (tầng 2)"},
                {"participants", "- Ban Lãnh đạo UBND xã\n- Trưởng các ban ngành, đoàn thể xã\n- Bí thư Chi bộ các thôn\n- Trưởng thôn, Trưởng ấp"},
                {"agenda", "1. Đánh giá kết quả thực hiện nhiệm vụ năm 2025\n2. Triển khai nhiệm vụ trọng tâm quý I/2026\n3. Phân công nhiệm vụ cụ thể cho các ban ngành\n4. Thảo luận và thống nhất giải pháp"},
                {"signer_name", "Nguyễn Văn Minh"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Thông báo kết quả
            "tb_ketqua" => new Dictionary<string, string>
            {
                {"event_name", "Hội nghị Ban Chấp hành Đảng bộ xã lần thứ 5"},
                {"participants", "35 đồng chí ủy viên Ban Chấp hành, 5 đồng chí được mời dự"},
                {"content", "Hội nghị đã nghe và thảo luận các nội dung:\n1. Báo cáo tổng kết công tác năm 2025\n2. Đánh giá tình hình thực hiện nhiệm vụ 6 tháng đầu năm\n3. Phương hướng nhiệm vụ 6 tháng cuối năm 2026\n4. Một số vấn đề cấp bách khác"},
                {"conclusion", "Ban Chấp hành nhất trí cao với các nội dung Báo cáo. Đồng ý với phương hướng nhiệm vụ và các giải pháp đề ra. Yêu cầu các ban ngành tập trung triển khai quyết liệt, đảm bảo hoàn thành vượt mức các chỉ tiêu đề ra."},
                {"tasks", "1. Văn phòng UBND tổng hợp báo cáo chi tiết gửi cấp trên trước ngày 15/02\n2. Các ban ngành xây dựng kế hoạch cụ thể trình UBND trước ngày 20/02\n3. Thường trực UBND giám sát, đôn đốc việc thực hiện"},
                {"signer_name", "Trần Thị Mai"},
                {"signer_title", "Phó Chủ tịch UBND xã"}
            },
            
            // Nghị quyết HĐND
            "nq_hdnd" => new Dictionary<string, string>
            {
                {"level", "Xã"},
                {"subject", "Phê duyệt dự toán ngân sách xã năm 2026"},
                {"articles", "Điều 1. Phê duyệt tổng dự toán thu ngân sách xã năm 2026 là 15 tỷ đồng, bao gồm:\n- Thu từ đất: 8 tỷ đồng\n- Thu phí, lệ phí: 2 tỷ đồng\n- Hỗ trợ từ ngân sách cấp trên: 5 tỷ đồng\n\nĐiều 2. Phê duyệt tổng dự toán chi ngân sách xã năm 2026 là 15 tỷ đồng, trong đó:\n- Chi đầu tư phát triển: 8 tỷ đồng\n- Chi thường xuyên: 5 tỷ đồng\n- Chi dự phòng: 2 tỷ đồng"},
                {"effective_date", "01/01/2026"},
                {"chairman_name", "Võ Văn Hùng"}
            },
            
            // Nghị quyết UBND
            "nq_ubnd" => new Dictionary<string, string>
            {
                {"subject", "Ban hành Quy chế quản lý hoạt động kinh doanh karaoke trên địa bàn xã"},
                {"articles", "Điều 1. Phạm vi điều chỉnh\nQuy chế này quy định về điều kiện, thủ tục cấp phép và quản lý hoạt động kinh doanh karaoke trên địa bàn xã Tân Thành.\n\nĐiều 2. Điều kiện kinh doanh\n- Có đầy đủ giấy phép kinh doanh theo quy định\n- Cách xa trường học, bệnh viện tối thiểu 200m\n- Đảm bảo phòng cháy chữa cháy\n- Không hoạt động sau 23h00\n\nĐiều 3. Trách nhiệm của chủ cơ sở\n- Đăng ký kinh doanh với UBND xã\n- Nộp phí, lệ phí theo quy định\n- Chấp hành nghiêm các quy định về an ninh trật tự"},
                {"implementing_unit", "Công an xã, Văn phòng UBND xã"},
                {"chairman_name", "Nguyễn Văn Minh"}
            },
            
            _ => new Dictionary<string, string>()
        };
    }

    private string GetFieldLabel(string field)
    {
        return field switch
        {
            "from_org" => "🏢 Cơ quan gửi",
            "to_org" => "📨 Cơ quan nhận",
            "to_department" => "🏛️ Sở/Ban/Ngành nhận",
            "subject" => "📋 Vấn đề/Tiêu đề",
            "content" => "📝 Nội dung chính",
            "signer_name" => "✍️ Người ký",
            "signer_title" => "👔 Chức danh người ký",
            "recipient" => "📬 Đơn vị nhận",
            "reason" => "💡 Lý do",
            "proposal" => "📊 Đề xuất",
            "reply_to_number" => "🔢 Trả lời công văn số",
            "person_name" => "👤 Họ tên cán bộ",
            "current_position" => "💼 Chức vụ hiện tại",
            "from_unit" => "🏢 Đơn vị cũ",
            "to_unit" => "🏢 Đơn vị mới",
            "new_position" => "⭐ Chức vụ mới",
            "effective_date" => "📅 Ngày hiệu lực",
            "award_type" => "🏆 Hình thức khen thưởng",
            "achievement" => "✨ Thành tích",
            "org_name" => "🏛️ Tên tổ chức",
            "members" => "👥 Danh sách thành viên",
            "tasks" => "📋 Nhiệm vụ",
            "project_name" => "🎯 Tên đề án/dự án",
            "objectives" => "🎯 Mục tiêu",
            "budget" => "💰 Kinh phí",
            "implementing_unit" => "⚙️ Đơn vị thực hiện",
            "period" => "📆 Kỳ báo cáo/kế hoạch",
            "achievements" => "✅ Kết quả đạt được",
            "challenges" => "⚠️ Tồn tại, hạn chế",
            "future_plans" => "🚀 Phương hướng tiếp theo",
            "field" => "📂 Lĩnh vực",
            "situation" => "📊 Tình hình",
            "results" => "📈 Kết quả",
            "proposals" => "💡 Đề xuất, kiến nghị",
            "task_name" => "📌 Nhiệm vụ/Kế hoạch",
            "evaluation" => "⭐ Đánh giá",
            "time_place" => "⏰ Thời gian, địa điểm",
            "purpose" => "🎯 Mục đích",
            "program" => "📜 Chương trình",
            "event_name" => "🎉 Tên sự kiện",
            "meeting_name" => "🤝 Tên cuộc họp",
            "time" => "⏰ Thời gian",
            "location" => "📍 Địa điểm",
            "participants" => "👥 Thành phần tham dự",
            "agenda" => "📋 Nội dung họp",
            "conclusion" => "✅ Kết luận",
            "level" => "🏛️ Cấp (Tỉnh/Huyện/Xã)",
            "articles" => "📜 Các điều khoản",
            "chairman_name" => "👨‍💼 Chủ tịch",
            _ => field
        };
    }

    private string GetFieldHint(string field)
    {
        return field switch
        {
            "from_org" => "Ví dụ: UBND xã Tân Thành",
            "to_org" => "Ví dụ: UBND huyện Bình Chánh",
            "subject" => "Vấn đề văn bản cần soạn",
            "content" => "Nội dung chi tiết văn bản...",
            "signer_name" => "Ví dụ: Nguyễn Văn A",
            "signer_title" => "Ví dụ: Chủ tịch UBND",
            _ => $"Nhập {field}..."
        };
    }

    private double GetFieldHeight(string field)
    {
        return field switch
        {
            "content" or "achievements" or "challenges" or "tasks" or "proposals" 
                or "situation" or "results" or "members" or "articles" or "program" => 120,
            _ => 40
        };
    }

    private void ViewTemplate_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTemplate == null) return;

        var viewWindow = new Window
        {
            Title = $"📄 Xem mẫu: {_selectedTemplate.Name}",
            Width = 700,
            Height = 600,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Owner = this
        };

        var grid = new Grid { Margin = new Thickness(15) };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Template content
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(0, 0, 0, 10)
        };
        
        var contentTextBox = new TextBox
        {
            Text = _selectedTemplate.TemplateContent,
            IsReadOnly = true,
            TextWrapping = TextWrapping.Wrap,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10),
            FontFamily = new System.Windows.Media.FontFamily("Times New Roman"),
            FontSize = 14,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        
        scrollViewer.Content = contentTextBox;
        Grid.SetRow(scrollViewer, 0);
        grid.Children.Add(scrollViewer);

        // Close button
        var closeButton = new Button
        {
            Content = "Đóng",
            Width = 100,
            Height = 35,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        closeButton.Click += (s, args) => viewWindow.Close();
        
        Grid.SetRow(closeButton, 1);
        grid.Children.Add(closeButton);

        viewWindow.Content = grid;
        viewWindow.ShowDialog();
    }

    private async void Generate_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTemplate == null) return;

        // Validate inputs
        var missingFields = new List<string>();
        foreach (var field in _selectedTemplate.RequiredFields ?? Array.Empty<string>())
        {
            if (_fieldInputs.TryGetValue(field, out var textBox) && string.IsNullOrWhiteSpace(textBox.Text))
            {
                missingFields.Add(GetFieldLabel(field));
            }
        }

        if (missingFields.Any())
        {
            MessageBox.Show(
                $"Vui lòng nhập đầy đủ thông tin:\n\n{string.Join("\n", missingFields)}",
                "Thiếu thông tin",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }

        // Show loading
        LoadingProgress.Visibility = Visibility.Visible;
        GenerateButton.IsEnabled = false;

        try
        {
            // Build prompt từ template
            var prompt = BuildPrompt();

            // System instruction
            var systemInstruction = @"Bạn là chuyên gia soạn thảo văn bản hành chính Việt Nam.
Hãy tạo nội dung văn bản chính thức, đúng format, ngôn ngữ trang trọng, rõ ràng.
Chỉ trả về nội dung văn bản, KHÔNG thêm giải thích hay ghi chú.";

            // Gọi AI
            var content = await _aiService.GenerateContentAsync(prompt, systemInstruction);

            // Hiển thị kết quả trong RichTextBox
            SetRichTextContent(content);
            PreviewExpander.Visibility = Visibility.Visible;
            PreviewExpander.IsExpanded = true;
            
            // Enable save button when content is generated
            if (SaveDocumentButton != null)
                SaveDocumentButton.IsEnabled = !string.IsNullOrWhiteSpace(content);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Lỗi khi tạo nội dung với AI:\n\n{ex.Message}",
                "Lỗi AI",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
        finally
        {
            LoadingProgress.Visibility = Visibility.Collapsed;
            GenerateButton.IsEnabled = true;
        }
    }

    private string BuildPrompt()
    {
        if (_selectedTemplate == null) return "";

        var prompt = _selectedTemplate.AIPrompt ?? "";

        // Replace placeholders
        foreach (var kvp in _fieldInputs)
        {
            var placeholder = "{" + kvp.Key + "}";
            var value = kvp.Value.Text;
            prompt = prompt.Replace(placeholder, value);
        }

        return prompt;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var content = GetRichTextContent();
        if (string.IsNullOrWhiteSpace(content))
        {
            MessageBox.Show("Chưa có nội dung để lưu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Tạo document mới
        GeneratedDocument = new Document
        {
            Title = _fieldInputs.TryGetValue("subject", out var subjectBox) 
                ? subjectBox.Text 
                : $"{_selectedTemplate?.Name} - {DateTime.Now:dd/MM/yyyy}",
            Type = _selectedTemplate?.Type ?? DocumentType.CongVan,
            Content = content,
            CreatedDate = DateTime.Now,
            WorkflowStatus = DocumentStatus.Draft,
            Tags = new[] { "AI Generated", (_selectedTemplate?.Type.ToString() ?? "") }
        };

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    // ===== RICH TEXT EDITOR FUNCTIONS =====
    
    private string GetRichTextContent()
    {
        if (GeneratedContentRichTextBox?.Document == null) return "";
        
        var textRange = new TextRange(
            GeneratedContentRichTextBox.Document.ContentStart,
            GeneratedContentRichTextBox.Document.ContentEnd
        );
        
        return textRange.Text;
    }
    
    private void SetRichTextContent(string text)
    {
        var flowDoc = new FlowDocument();
        flowDoc.PagePadding = new Thickness(20);
        
        // Split by newlines and create paragraphs
        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        
        foreach (var line in lines)
        {
            var para = new Paragraph(new Run(line));
            
            // Format based on content
            if (line.Contains("**") && line.Trim().StartsWith("**"))
            {
                // Bold headers
                para.FontWeight = FontWeights.Bold;
                para.TextAlignment = TextAlignment.Center;
                para.FontSize = 16;
            }
            else if (line.Trim().StartsWith("Điều ") || line.Trim().StartsWith("Chương "))
            {
                // Bold articles
                para.FontWeight = FontWeights.Bold;
                para.FontSize = 14;
            }
            else
            {
                para.FontSize = 14;
                para.TextAlignment = TextAlignment.Justify;
            }
            
            flowDoc.Blocks.Add(para);
        }
        
        GeneratedContentRichTextBox.Document = flowDoc;
    }

    private void FontFamily_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (GeneratedContentRichTextBox?.Selection != null && FontFamilyComboBox.SelectedItem is ComboBoxItem item)
        {
            var fontFamily = new System.Windows.Media.FontFamily(item.Content.ToString());
            GeneratedContentRichTextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);
        }
    }

    private void FontSize_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (GeneratedContentRichTextBox?.Selection != null && FontSizeComboBox.SelectedItem is ComboBoxItem item)
        {
            if (double.TryParse(item.Content.ToString(), out var fontSize))
            {
                GeneratedContentRichTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
            }
        }
    }

    private void Bold_Click(object sender, RoutedEventArgs e)
    {
        if (GeneratedContentRichTextBox?.Selection != null)
        {
            var currentWeight = GeneratedContentRichTextBox.Selection.GetPropertyValue(TextElement.FontWeightProperty);
            var newWeight = (currentWeight as FontWeight?)?.Equals(FontWeights.Bold) == true 
                ? FontWeights.Normal 
                : FontWeights.Bold;
            GeneratedContentRichTextBox.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, newWeight);
        }
    }

    private void Italic_Click(object sender, RoutedEventArgs e)
    {
        if (GeneratedContentRichTextBox?.Selection != null)
        {
            var currentStyle = GeneratedContentRichTextBox.Selection.GetPropertyValue(TextElement.FontStyleProperty);
            var newStyle = (currentStyle as FontStyle?)?.Equals(FontStyles.Italic) == true 
                ? FontStyles.Normal 
                : FontStyles.Italic;
            GeneratedContentRichTextBox.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, newStyle);
        }
    }

    private void Underline_Click(object sender, RoutedEventArgs e)
    {
        if (GeneratedContentRichTextBox?.Selection != null)
        {
            var currentDeco = GeneratedContentRichTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            var newDeco = currentDeco == TextDecorations.Underline ? null : TextDecorations.Underline;
            GeneratedContentRichTextBox.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, newDeco);
        }
    }

    private void AlignLeft_Click(object sender, RoutedEventArgs e)
    {
        if (GeneratedContentRichTextBox?.Selection != null)
        {
            GeneratedContentRichTextBox.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Left);
        }
    }

    private void AlignCenter_Click(object sender, RoutedEventArgs e)
    {
        if (GeneratedContentRichTextBox?.Selection != null)
        {
            GeneratedContentRichTextBox.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Center);
        }
    }

    private void AlignRight_Click(object sender, RoutedEventArgs e)
    {
        if (GeneratedContentRichTextBox?.Selection != null)
        {
            GeneratedContentRichTextBox.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Right);
        }
    }

    private void AlignJustify_Click(object sender, RoutedEventArgs e)
    {
        if (GeneratedContentRichTextBox?.Selection != null)
        {
            GeneratedContentRichTextBox.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Justify);
        }
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        if (GeneratedContentRichTextBox != null)
        {
            var text = new TextRange(
                GeneratedContentRichTextBox.Document.ContentStart,
                GeneratedContentRichTextBox.Document.ContentEnd
            ).Text;
            
            Clipboard.SetText(text);
            MessageBox.Show("✅ Đã copy nội dung văn bản!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Print_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                var doc = GeneratedContentRichTextBox.Document;
                var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                printDialog.PrintDocument(paginator, "Văn bản hành chính");
                MessageBox.Show("✅ Đã gửi lệnh in!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportWord_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx|Rich Text Format (*.rtf)|*.rtf",
                DefaultExt = ".docx",
                FileName = $"VanBan_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var content = GetRichTextContent();
                
                if (saveDialog.FilterIndex == 1) // .docx
                {
                    ExportToDocx(saveDialog.FileName, content);
                }
                else // .rtf
                {
                    ExportToRtf(saveDialog.FileName);
                }
                
                MessageBox.Show($"✅ Đã xuất file thành công!\n\n{saveDialog.FileName}", 
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Mở file sau khi xuất
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = saveDialog.FileName,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportToDocx(string filePath, string content)
    {
        // Simple DOCX export using ZIP format
        using (var zip = System.IO.Compression.ZipFile.Open(filePath, System.IO.Compression.ZipArchiveMode.Create))
        {
            // Create basic Word document structure
            var contentTypesXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
    <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
    <Default Extension=""xml"" ContentType=""application/xml""/>
    <Override PartName=""/word/document.xml"" ContentType=""application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml""/>
</Types>";
            
            var relsXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
    <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""word/document.xml""/>
</Relationships>";

            var documentXml = $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">
    <w:body>
        <w:p>
            <w:pPr>
                <w:rPr>
                    <w:rFonts w:ascii=""Times New Roman"" w:hAnsi=""Times New Roman""/>
                    <w:sz w:val=""28""/>
                </w:rPr>
            </w:pPr>
            <w:r>
                <w:rPr>
                    <w:rFonts w:ascii=""Times New Roman"" w:hAnsi=""Times New Roman""/>
                    <w:sz w:val=""28""/>
                </w:rPr>
                <w:t xml:space=""preserve"">{System.Security.SecurityElement.Escape(content)}</w:t>
            </w:r>
        </w:p>
    </w:body>
</w:document>";

            var entry1 = zip.CreateEntry("[Content_Types].xml");
            using (var writer = new System.IO.StreamWriter(entry1.Open()))
                writer.Write(contentTypesXml);
            
            var entry2 = zip.CreateEntry("_rels/.rels");
            using (var writer = new System.IO.StreamWriter(entry2.Open()))
                writer.Write(relsXml);
            
            var entry3 = zip.CreateEntry("word/document.xml");
            using (var writer = new System.IO.StreamWriter(entry3.Open(), System.Text.Encoding.UTF8))
                writer.Write(documentXml);
        }
    }

    private void ExportToRtf(string filePath)
    {
        // RTF export is native to WPF RichTextBox
        var range = new TextRange(
            GeneratedContentRichTextBox.Document.ContentStart,
            GeneratedContentRichTextBox.Document.ContentEnd
        );
        
        using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
        {
            range.Save(stream, DataFormats.Rtf);
        }
    }
}

// Helper dialog
public class ContentDialog : Window
{
    public string Title { get; set; } = "";
    public object Content { get; set; } = new();
    public string PrimaryButtonText { get; set; } = "OK";

    public ContentDialog()
    {
        Width = 600;
        Height = 500;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        
        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        if (Content is UIElement element)
        {
            Grid.SetRow(element, 0);
            grid.Children.Add(element);
        }

        var button = new Button
        {
            Content = PrimaryButtonText,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0),
            Padding = new Thickness(20, 8, 20, 8)
        };
        button.Click += (s, e) => Close();
        Grid.SetRow(button, 1);
        grid.Children.Add(button);

        this.Content = grid;
        this.Title = Title;
    }

    public void ShowDialog()
    {
        ShowDialog();
    }
}
