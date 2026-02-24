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
    private readonly string? _preSelectedTemplateId;

    public Document? GeneratedDocument { get; private set; }

    public AIComposeDialog(DocumentService documentService, string? geminiApiKey = null, string? preSelectedTemplateId = null)
    {
        InitializeComponent();
        _documentService = documentService;
        _aiService = string.IsNullOrEmpty(geminiApiKey) ? new GeminiAIService() : new GeminiAIService(geminiApiKey);
        _preSelectedTemplateId = preSelectedTemplateId;
        
        LoadTemplates();
    }

    private void LoadTemplates()
    {
        var templates = _documentService.GetAllTemplates();
        TemplateComboBox.ItemsSource = templates.OrderBy(t => t.Type).ThenBy(t => t.Name);
        
        // Nếu có pre-selected template, auto-select nó
        if (!string.IsNullOrEmpty(_preSelectedTemplateId))
        {
            var preSelected = templates.FirstOrDefault(t => t.Id == _preSelectedTemplateId);
            if (preSelected != null)
            {
                TemplateComboBox.SelectedItem = preSelected;
                return;
            }
        }
        
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
            var fieldLabels = _selectedTemplate.RequiredFields.Select(f => {
                var label = GetFieldLabel(f);
                // Bỏ emoji ở đầu label (emoji là surrogate pair, không dùng TrimStart char được)
                var idx = 0;
                while (idx < label.Length && (char.IsHighSurrogate(label[idx]) || label[idx] > 0x2000))
                    idx += char.IsHighSurrogate(label[idx]) ? 2 : 1;
                return label.Substring(idx).Trim();
            });
            RequiredFieldsText.Text = $"✅ Các trường cần nhập: {string.Join(", ", fieldLabels)}";
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
        var selectedScenario = (SampleScenarioComboBox.SelectedItem as ComboBoxItem)?.Tag as string;
        if (string.IsNullOrEmpty(selectedScenario))
        {
            MessageBox.Show("Vui lòng chọn kịch bản mẫu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Auto-switch to the matching template for this scenario
        var templateName = GetTemplateNameForScenario(selectedScenario);
        if (!string.IsNullOrEmpty(templateName))
        {
            var templates = TemplateComboBox.ItemsSource as IEnumerable<DocumentTemplate>;
            var matchingTemplate = templates?.FirstOrDefault(t => t.Name == templateName);
            if (matchingTemplate != null && matchingTemplate != _selectedTemplate)
            {
                TemplateComboBox.SelectedItem = matchingTemplate;
                // This triggers TemplateComboBox_SelectionChanged synchronously
                // which calls CreateInputFields() and populates _fieldInputs
            }
        }

        if (_selectedTemplate == null || _selectedTemplate.RequiredFields == null) return;

        var samples = GetScenarioSamples(selectedScenario);
        int filledCount = 0;
        
        foreach (var field in _selectedTemplate.RequiredFields)
        {
            if (_fieldInputs.TryGetValue(field, out var textBox))
            {
                if (samples.TryGetValue(field, out var value))
                {
                    textBox.Text = value;
                    filledCount++;
                }
            }
        }
        
        var scenarioName = (SampleScenarioComboBox.SelectedItem as ComboBoxItem)?.Content;
        MessageBox.Show($"✅ Đã tải dữ liệu mẫu: {scenarioName}\n📝 Đã điền {filledCount}/{_selectedTemplate.RequiredFields.Length} trường.\n\n💡 Nhấn \"Tạo văn bản với AI\" để tạo văn bản.", 
            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private string GetTemplateNameForScenario(string scenario)
    {
        return scenario switch
        {
            // Công văn
            "cv_kinhi" or "cv_moihop" or "cv_dondoc" or "cv_chutruong" 
                or "cv_gioithieu" or "cv_phoihop" => "Công văn chung",
            "cv_baocao" => "Công văn báo cáo cấp trên",
            "cv_traloi" => "Công văn trả lời",
            
            // Quyết định
            "qd_khenthuong" => "Quyết định khen thưởng",
            "qd_dieudonng" => "Quyết định điều động cán bộ",
            "qd_thanhlap" => "Quyết định thành lập tổ chức",
            "qd_pheduyet" or "qd_xuphat" or "qd_capdat" => "Quyết định phê duyệt",
            "qd_quiche" => "Nghị quyết UBND",
            
            // Báo cáo
            "bc_tongket" or "bc_cchc" => "Báo cáo tổng kết",
            "bc_tinhhinh" or "bc_thientai" or "bc_danso" => "Báo cáo tình hình",
            
            // Tờ trình
            "tt_yikien" or "tt_bienche" => "Tờ trình xin ý kiến",
            "tt_dexuat" or "tt_kinhphi" or "tt_quyhoach" => "Tờ trình đề xuất",
            
            // Kế hoạch
            "kh_congtac" or "kh_pccc" or "kh_chuyendoiso" => "Kế hoạch công tác",
            "kh_sukien" or "kh_baucu" => "Kế hoạch tổ chức sự kiện",
            
            // Thông báo
            "tb_hop" or "tb_tiepcongdan" or "tb_nghile" => "Thông báo họp",
            "tb_ketqua" or "tb_tuyendung" => "Thông báo kết quả",
            
            // Nghị quyết
            "nq_hdnd" or "nq_chuyende" => "Nghị quyết HĐND",
            "nq_ubnd" => "Nghị quyết UBND",
            
            // Chỉ thị → dùng Công văn chung (chưa có mẫu Chỉ thị)
            "ct_antt" or "ct_phongdich" => "Công văn chung",
            
            _ => ""
        };
    }

    private Dictionary<string, string> GetScenarioSamples(string scenario)
    {
        return scenario switch
        {
            // Công văn xin hỗ trợ kinh phí
            "cv_kinhi" => new Dictionary<string, string>
            {
                {"from_org", "UBND xã Tân Thành"},
                {"to_org", "UBND thành phố Bình Chánh"},
                {"to_department", "Sở Tài chính TP.HCM"},
                {"subject", "Đề nghị hỗ trợ kinh phí xây dựng đường giao thông nông thôn"},
                {"content", "Hiện nay, tuyến đường liên xã Tân Thành - Long Phước dài 2,5km đang trong tình trạng xuống cấp nghiêm trọng, gây khó khăn cho việc đi lại của nhân dân. UBND xã Tân Thành kính đề nghị UBND thành phố xem xét hỗ trợ kinh phí xây dựng, cải tạo tuyến đường theo dự toán đính kèm."},
                {"signer_name", "Nguyễn Văn Minh"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Công văn báo cáo tiến độ
            "cv_baocao" => new Dictionary<string, string>
            {
                {"from_org", "UBND xã Tân Thành"},
                {"to_org", "UBND thành phố Bình Chánh"},
                {"subject", "Báo cáo tiến độ thực hiện Chương trình xây dựng nông thôn mới quý I/2026"},
                {"content", "Thực hiện Chương trình xây dựng nông thôn mới năm 2026, trong quý I, UBND xã Tân Thành đã hoàn thành 8/10 tiêu chí đề ra. Cụ thể: hoàn thành 100% công trình hạ tầng giao thông, 95% hộ dân có nhà tiêu hợp vệ sinh, 100% trẻ em được tiêm chủng đầy đủ. Hiện còn 2 tiêu chí về kinh tế hộ và môi trường đang trong quá trình triển khai."},
                {"proposal", "Đề nghị UBND thành phố tiếp tục hỗ trợ về kinh phí và chuyên môn để xã hoàn thành các tiêu chí còn lại"},
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
                {"award_type", "Bằng khen của UBND thành phố"},
                {"recipient", "Tập thể Ban Văn hóa - Xã hội xã Tân Thành"},
                {"achievement", "Đã có thành tích xuất sắc trong công tác tuyên truyền, vận động nhân dân tham gia các phong trào văn hóa, thể thao năm 2025. Đạt danh hiệu Làng văn hóa tiêu biểu cấp tỉnh/thành phố 3 năm liền (2023-2025)"},
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
                {"tasks", "- Chỉ đạo, điều hành công tác phòng chống dịch Covid-19 trên địa bàn xã\n- Triển khai các biện pháp giám sát, cách ly, xét nghiệm\n- Tuyên truyền nâng cao ý thức người dân\n- Báo cáo định kỳ về UBND thành phố"},
                {"signer_name", "Nguyễn Văn Minh"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Quyết định phê duyệt
            "qd_pheduyet" => new Dictionary<string, string>
            {
                {"project_name", "Dự án xây dựng trường mầm non Tân Thành B"},
                {"objectives", "Xây dựng trường mầm non 3 tầng, quy mô 6 phòng học, đáp ứng nhu cầu học tập cho 180 trẻ em trên địa bàn"},
                {"budget", "8 tỷ đồng từ nguồn ngân sách thành phố và xã hội hóa"},
                {"implementing_unit", "Phòng Giáo dục và Đào tạo thành phố Bình Chánh"},
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
                {"proposals", "- Tăng cường tuần tra vào dịp Tết Nguyên đán\n- Đề nghị thành phố hỗ trợ thêm thiết bị camera an ninh\n- Mở thêm lớp tuyên truyền phổ biến pháp luật cho thanh niên"},
                {"signer_name", "Lê Văn Tâm"},
                {"signer_title", "Trưởng Công an xã"}
            },
            
            // Tờ trình xin ý kiến
            "tt_yikien" => new Dictionary<string, string>
            {
                {"org_name", "UBND xã Tân Thành"},
                {"recipient", "UBND thành phố Bình Chánh"},
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
                {"recipient", "UBND thành phố Bình Chánh"},
                {"proposal", "Đề xuất dự án xây dựng nhà văn hóa đa năng xã Tân Thành"},
                {"reason", "Nhà văn hóa xã hiện nay xuống cấp nghiêm trọng, không đáp ứng nhu cầu sinh hoạt văn hóa của nhân dân. Xã cần xây dựng nhà văn hóa mới quy mô 500m², 2 tầng để phục vụ các hoạt động văn hóa, thể thao, họp dân."},
                {"budget", "6 tỷ đồng (trong đó: ngân sách thành phố 4 tỷ, ngân sách xã 1 tỷ, xã hội hóa 1 tỷ)"},
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
            
            // ═══════════════════════════════════════════════════
            // CÁC MẪU MỚI BỔ SUNG (23 mẫu)
            // ═══════════════════════════════════════════════════
            
            // --- CÔNG VĂN MỚI ---
            
            // Công văn mời họp liên ngành
            "cv_moihop" => new Dictionary<string, string>
            {
                {"from_org", "UBND xã Gia Kiệm"},
                {"to_org", "Các ban ngành, đoàn thể xã; Trưởng 17 ấp"},
                {"subject", "Mời dự Hội nghị triển khai công tác phòng chống lụt bão năm 2026"},
                {"content", "Thực hiện Chỉ thị số 05/CT-UBND ngày 20/01/2026 của UBND thành phố Thống Nhất về tăng cường công tác phòng chống thiên tai năm 2026, UBND xã Gia Kiệm tổ chức Hội nghị triển khai công tác phòng chống lụt bão với nội dung:\n\n1. Thời gian: 8h00, thứ Ba ngày 25/02/2026\n2. Địa điểm: Hội trường UBND xã Gia Kiệm\n3. Nội dung: Triển khai phương án 4 tại chỗ, phân công nhiệm vụ các ban ngành, thống nhất kịch bản ứng phó\n4. Thành phần: Trưởng các ban ngành, đoàn thể, Trưởng 17 ấp, đại diện 5 giáo xứ\n\nĐề nghị các đồng chí sắp xếp tham dự đầy đủ, đúng giờ."},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Công văn đôn đốc thu thuế
            "cv_dondoc" => new Dictionary<string, string>
            {
                {"from_org", "UBND xã Gia Kiệm"},
                {"to_org", "Chi cục Thuế thành phố Thống Nhất"},
                {"subject", "Đôn đốc thu nộp thuế sử dụng đất phi nông nghiệp năm 2026"},
                {"content", "Thực hiện kế hoạch thu ngân sách năm 2026, đến ngày 15/02/2026, tình hình thu thuế sử dụng đất phi nông nghiệp trên địa bàn xã như sau:\n\n- Tổng số hộ phải nộp: 4.850 hộ\n- Số hộ đã nộp: 2.120 hộ (đạt 43,7%)\n- Số tiền đã thu: 1,85 tỷ đồng / 4,2 tỷ đồng kế hoạch (đạt 44%)\n- Số hộ chưa nộp: 2.730 hộ\n\nUBND xã đã triển khai nhiều biện pháp: phát thông báo đến từng hộ, đôn đốc qua Trưởng ấp, niêm yết công khai tại trụ sở. Tuy nhiên tỷ lệ thu vẫn thấp do nhiều hộ vắng nhà, đi làm xa.\n\nKính đề nghị Chi cục Thuế hỗ trợ xã trong việc đôn đốc, xử lý các trường hợp cố tình chây ì."},
                {"signer_name", "Trần Văn Hải"},
                {"signer_title", "Phó Chủ tịch UBND xã"}
            },
            
            // Công văn xin chủ trương
            "cv_chutruong" => new Dictionary<string, string>
            {
                {"from_org", "UBND xã Gia Kiệm"},
                {"to_org", "UBND thành phố Thống Nhất"},
                {"subject", "Xin chủ trương đầu tư xây dựng hệ thống chiếu sáng công cộng tuyến đường liên ấp"},
                {"content", "Hiện nay, tuyến đường liên ấp 5 - ấp 7 dài 3,2km phục vụ đi lại cho khoảng 2.500 hộ dân chưa có hệ thống chiếu sáng công cộng, gây mất an toàn giao thông và an ninh trật tự vào ban đêm. Năm 2025, đã xảy ra 3 vụ tai nạn giao thông và 2 vụ trộm cắp trên tuyến đường này.\n\nUBND xã kính đề nghị UBND thành phố cho chủ trương đầu tư xây dựng hệ thống chiếu sáng với dự kiến:\n- Quy mô: 65 trụ đèn LED năng lượng mặt trời\n- Kinh phí dự kiến: 1,95 tỷ đồng\n- Nguồn vốn: Ngân sách thành phố 1,2 tỷ, ngân sách xã 0,5 tỷ, nhân dân đóng góp 0,25 tỷ\n- Thời gian thực hiện: Quý II-III/2026"},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Công văn giới thiệu công dân
            "cv_gioithieu" => new Dictionary<string, string>
            {
                {"from_org", "UBND xã Gia Kiệm"},
                {"to_org", "Sở Tư pháp tỉnh Đồng Nai"},
                {"subject", "Giới thiệu công dân liên hệ làm thủ tục cấp phiếu lý lịch tư pháp"},
                {"content", "UBND xã Gia Kiệm giới thiệu:\n\nHọ và tên: LÊ THỊ HỒNG NHUNG\nSinh ngày: 15/03/1990\nCMND/CCCD: 274195001234\nĐịa chỉ: Ấp 3, xã Gia Kiệm, thành phố Thống Nhất, tỉnh Đồng Nai\n\nNội dung: Đề nghị Sở Tư pháp tỉnh Đồng Nai xem xét cấp Phiếu lý lịch tư pháp cho công dân nêu trên để phục vụ mục đích xin việc làm.\n\nUBND xã xác nhận: Bà Lê Thị Hồng Nhung có hộ khẩu thường trú tại địa phương, chấp hành tốt pháp luật, không vi phạm gì trong thời gian cư trú."},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // Công văn phối hợp xử lý vi phạm
            "cv_phoihop" => new Dictionary<string, string>
            {
                {"from_org", "UBND xã Gia Kiệm"},
                {"to_org", "Phòng TN&MT thành phố Thống Nhất; Công an thành phố Thống Nhất"},
                {"subject", "Đề nghị phối hợp xử lý vi phạm xây dựng trái phép trên đất nông nghiệp"},
                {"content", "Ngày 10/02/2026, qua kiểm tra thực tế, UBND xã phát hiện hộ ông Trần Văn Bảy (CCCD: 274190005678, ấp 11) đang tự ý xây dựng nhà xưởng diện tích khoảng 500m² trên đất nông nghiệp (thửa đất số 125, tờ bản đồ số 8) mà không có giấy phép xây dựng và không được cơ quan có thẩm quyền cho phép chuyển mục đích sử dụng đất.\n\nUBND xã đã lập biên bản vi phạm và yêu cầu ngừng thi công, tuy nhiên hộ ông Bảy không chấp hành.\n\nKính đề nghị:\n1. Phòng TN&MT thành phố cử cán bộ xuống xác minh, xử lý theo thẩm quyền\n2. Công an thành phố hỗ trợ đảm bảo an ninh trật tự trong quá trình cưỡng chế (nếu có)"},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // --- QUYẾT ĐỊNH MỚI ---
            
            // QĐ xử phạt vi phạm hành chính (mapped to QĐ Phê duyệt)
            "qd_xuphat" => new Dictionary<string, string>
            {
                {"project_name", "Xử phạt VPHC ông Phạm Văn Thắng - Xây dựng trái phép trên đất nông nghiệp ấp 9"},
                {"objectives", "Căn cứ Luật Xử lý VPHC năm 2012 (sửa đổi 2020); NĐ 16/2022/NĐ-CP ngày 28/01/2022 về xử phạt VPHC xây dựng.\nHành vi: Xây dựng công trình không có giấy phép trên đất nông nghiệp tại ấp 9, xã Gia Kiệm.\nBiên bản VPHC số 05/BB-VPHC ngày 05/02/2026"},
                {"budget", "Phạt tiền 25.000.000 đồng (Hai mươi lăm triệu đồng). Buộc tháo dỡ công trình vi phạm trong 30 ngày kể từ ngày ra quyết định"},
                {"implementing_unit", "Công an xã Gia Kiệm, Ban Địa chính - Xây dựng xã"},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // QĐ cấp đất ở cho hộ nghèo (mapped to QĐ Phê duyệt)
            "qd_capdat" => new Dictionary<string, string>
            {
                {"project_name", "Giao 200m² đất ở cho bà Nguyễn Thị Lan - Hộ nghèo diện chính sách vợ liệt sĩ"},
                {"objectives", "Giao đất ở tại thửa số 45, tờ bản đồ số 3, ấp 6, xã Gia Kiệm cho bà Nguyễn Thị Lan (SN 1965, CCCD: 274190003456).\nMục đích: Xây dựng nhà ở. Thời hạn: Lâu dài.\nNguồn gốc đất: Quỹ đất công ích 5% của xã"},
                {"budget", "Miễn tiền sử dụng đất theo Nghị định 45/2014/NĐ-CP (hộ nghèo, gia đình chính sách liệt sĩ)"},
                {"implementing_unit", "Ban Địa chính xã Gia Kiệm, VP Đăng ký đất đai thành phố Thống Nhất"},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // QĐ ban hành quy chế (mapped to NQ UBND)
            "qd_quiche" => new Dictionary<string, string>
            {
                {"subject", "Ban hành Quy chế làm việc của UBND xã Gia Kiệm nhiệm kỳ 2021-2026"},
                {"articles", "Điều 1. Ban hành kèm theo Quyết định này Quy chế làm việc của UBND xã Gia Kiệm nhiệm kỳ 2021-2026.\n\nĐiều 2. Quyết định này có hiệu lực kể từ ngày ký và thay thế QĐ số 15/QĐ-UBND ngày 10/7/2021.\n\nĐiều 3. Văn phòng UBND xã, các ban ngành, đoàn thể xã chịu trách nhiệm thi hành.\n\nQuy chế gồm 6 chương, 32 điều: Quy định chung, Trách nhiệm quyền hạn CT/PCT/UV, Chế độ làm việc hội họp, Quan hệ công tác HĐND/Đảng ủy/MTTQ, Quản lý VB con dấu, Điều khoản thi hành."},
                {"implementing_unit", "Văn phòng UBND xã, các ban ngành, đoàn thể xã và cán bộ, công chức xã Gia Kiệm"},
                {"chairman_name", "Nguyễn Thanh Tùng"}
            },
            
            // --- BÁO CÁO MỚI ---
            
            // BC kết quả CCHC
            "bc_cchc" => new Dictionary<string, string>
            {
                {"org_name", "UBND xã Gia Kiệm"},
                {"period", "Quý I/2026"},
                {"achievements", "1. Cải cách thể chế:\n- Rà soát, bãi bỏ 3 văn bản không còn phù hợp\n- Ban hành 5 văn bản mới về quản lý đô thị, môi trường\n\n2. Cải cách thủ tục hành chính:\n- 100% TTHC được niêm yết công khai (156/156 thủ tục)\n- Tiếp nhận 1.245 hồ sơ, giải quyết đúng hạn 1.230 hồ sơ (98,8%)\n- 15 hồ sơ trễ hạn (1,2%) do thiếu giấy tờ bổ sung\n- Triển khai dịch vụ công trực tuyến mức 3, 4: 85/156 TTHC (54,5%)\n\n3. Cải cách tổ chức bộ máy:\n- Hoàn thành rà soát vị trí việc làm 22 CC-VC\n- Sắp xếp lại 2 ban ngành theo Nghị quyết 18-NQ/TW"},
                {"challenges", "- Tỷ lệ hồ sơ trực tuyến còn thấp (35%), người dân chưa quen sử dụng\n- Hạ tầng CNTT chưa đồng bộ, đường truyền internet thường xuyên chậm\n- Thiếu 2 biên chế so với quy định"},
                {"future_plans", "- Đẩy mạnh tuyên truyền dịch vụ công trực tuyến, phấn đấu 50% hồ sơ trực tuyến\n- Nâng cấp hạ tầng CNTT, lắp đặt thêm 2 máy tính phục vụ nhân dân\n- Tổ chức tập huấn kỹ năng số cho 100% cán bộ, công chức"},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // BC phòng chống thiên tai
            "bc_thientai" => new Dictionary<string, string>
            {
                {"org_name", "Ban Chỉ huy PCTT&TKCN xã Gia Kiệm"},
                {"field", "Phòng chống thiên tai và tìm kiếm cứu nạn tháng 1/2026"},
                {"situation", "Trong tháng 1/2026, trên địa bàn xã xảy ra 2 đợt mưa lớn (ngày 12/01 và 22/01), lượng mưa đo được 85mm và 120mm, gây ngập cục bộ tại ấp 3, ấp 7 và khu vực chợ Gia Kiệm."},
                {"results", "- Huy động 45 dân quân, 30 thanh niên tình nguyện ứng cứu\n- Di dời 12 hộ dân (52 nhân khẩu) tại vùng ngập đến nơi an toàn\n- Thiệt hại: 3 căn nhà tốc mái, 2ha hoa màu bị ngập, 500m đường bị sạt lở\n- Ước thiệt hại: 450 triệu đồng\n- Đã hỗ trợ khẩn cấp 15 triệu đồng cho 3 hộ bị tốc mái\n- Khơi thông 1.200m kênh mương thoát nước"},
                {"proposals", "- Đề nghị thành phố hỗ trợ 200 triệu đồng khắc phục hậu quả\n- Nạo vét suối Gia Kiệm đoạn qua ấp 3 (đã bồi lắng 50cm)\n- Xây dựng cống thoát nước tại ngã ba chợ\n- Cấp 100 bao cát dự phòng cho 5 điểm xung yếu"},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Trưởng Ban Chỉ huy PCTT&TKCN xã"}
            },
            
            // BC công tác dân số
            "bc_danso" => new Dictionary<string, string>
            {
                {"org_name", "UBND xã Gia Kiệm"},
                {"field", "Công tác Dân số - Kế hoạch hóa gia đình quý IV/2025"},
                {"situation", "Xã Gia Kiệm có 79.274 nhân khẩu, 19.818 hộ, phân bố trên 17 ấp. Cơ cấu dân số: 96% theo đạo Công giáo (thuộc 5 giáo xứ), đặc thù sinh đẻ nhiều con."},
                {"results", "- Trẻ sinh trong quý: 285 trẻ (nam 148, nữ 137), tỷ số giới tính: 108/100\n- Trẻ sinh là con thứ 3 trở lên: 42 trường hợp (14,7%) - giảm 2,1% so cùng kỳ\n- Phụ nữ 15-49 tuổi sử dụng BPTT: 8.450/12.200 (69,3%)\n- Tổ chức 8 buổi truyền thông tại 8 ấp, 2.400 lượt người tham dự\n- Khám sức khỏe tiền hôn nhân: 45 cặp (đạt 78% kế hoạch)\n- Tầm soát sơ sinh: 280/285 trẻ (98,2%)"},
                {"proposals", "- Tăng cường truyền thông tại 5 giáo xứ phối hợp với Linh mục chánh xứ\n- Mở thêm 2 điểm tư vấn SKSS tại ấp 9 và ấp 15\n- Đề nghị thành phố cấp thêm phương tiện tránh thai miễn phí cho 500 cặp vợ chồng"},
                {"signer_name", "Trần Văn Hải"},
                {"signer_title", "Phó Chủ tịch UBND xã"}
            },
            
            // --- TỜ TRÌNH MỚI ---
            
            // TT bổ sung biên chế
            "tt_bienche" => new Dictionary<string, string>
            {
                {"org_name", "UBND xã Gia Kiệm"},
                {"recipient", "UBND thành phố Thống Nhất; Phòng Nội vụ thành phố"},
                {"subject", "Đề nghị bổ sung biên chế công chức xã năm 2026"},
                {"reason", "Xã Gia Kiệm là xã loại I với dân số 79.274 người, 17 ấp, khối lượng công việc rất lớn. Hiện tại, xã có 22/24 biên chế theo quy định, thiếu 2 biên chế:\n- 01 công chức Địa chính - Xây dựng (vị trí 2): Do đồng chí Lê Văn Nam chuyển công tác từ 01/01/2026\n- 01 công chức Văn hóa - Xã hội (vị trí 2): Chưa được bố trí từ đầu nhiệm kỳ\n\nViệc thiếu biên chế gây quá tải cho cán bộ hiện có, ảnh hưởng đến chất lượng phục vụ nhân dân."},
                {"content", "Kính đề nghị UBND thành phố xem xét bổ sung 2 biên chế cho UBND xã Gia Kiệm:\n1. 01 công chức Địa chính - Xây dựng: Tốt nghiệp ĐH chuyên ngành Quản lý đất đai hoặc Xây dựng\n2. 01 công chức Văn hóa - Xã hội: Tốt nghiệp ĐH chuyên ngành CTXH hoặc Văn hóa\n\nĐiều kiện: Nam/Nữ, dưới 35 tuổi, có CCCD, lý lịch rõ ràng, sức khỏe tốt."},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // TT xin kinh phí sửa trường
            "tt_kinhphi" => new Dictionary<string, string>
            {
                {"org_name", "UBND xã Gia Kiệm"},
                {"recipient", "UBND thành phố Thống Nhất; Phòng GD&ĐT thành phố"},
                {"proposal", "Đề nghị cấp kinh phí sửa chữa Trường TH Gia Kiệm A"},
                {"reason", "Trường Tiểu học Gia Kiệm A (ấp 1) được xây dựng từ năm 2005, sau 21 năm sử dụng, nhiều hạng mục đã xuống cấp nghiêm trọng:\n- Mái ngói dãy phòng học A (8 phòng) bị dột, thấm nước mỗi khi mưa\n- Trần nhà 3 phòng học bị bong tróc, có nguy cơ sập\n- Hệ thống điện cũ, chập chờn, nguy cơ cháy nổ\n- Sân trường nứt vỡ, 2 cây phượng mục gốc nguy hiểm\n- Nhà vệ sinh hư hỏng 60%\n\nTrường đang phục vụ 856 học sinh, 32 giáo viên. Tình trạng xuống cấp gây mất an toàn và ảnh hưởng chất lượng giảng dạy."},
                {"budget", "Tổng dự toán sửa chữa: 2,8 tỷ đồng, gồm:\n- Lợp lại mái + sửa trần: 1,2 tỷ\n- Thay hệ thống điện: 350 triệu\n- Sửa sân trường + chặt cây nguy hiểm: 450 triệu\n- Xây mới nhà vệ sinh: 500 triệu\n- Chi phí khác (thiết kế, giám sát): 300 triệu"},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // TT phê duyệt đồ án quy hoạch (mapped to TT Đề xuất)
            "tt_quyhoach" => new Dictionary<string, string>
            {
                {"org_name", "UBND xã Gia Kiệm"},
                {"recipient", "UBND thành phố Thống Nhất"},
                {"proposal", "Phê duyệt đồ án quy hoạch chi tiết khu dân cư ấp 12"},
                {"reason", "Khu dân cư ấp 12 hiện có 380 hộ dân sinh sống tự phát, chưa có quy hoạch, thiếu hạ tầng kỹ thuật đồng bộ. Đường nội bộ nhỏ hẹp (2-3m), không có hệ thống thoát nước, điện chiếu sáng thiếu.\n\nQuy mô quy hoạch: 15ha, 450 lô đất ở, dân số 2.000 người.\nCơ cấu: Đất ở 8ha (53%), Giao thông 3ha (20%), Công cộng 1,5ha (10%), Hạ tầng KT 2,5ha (17%)"},
                {"budget", "350 triệu đồng lập quy hoạch (ngân sách thành phố). Tổng mức đầu tư hạ tầng dự kiến 25 tỷ đồng (giai đoạn 2026-2030)"},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // --- KẾ HOẠCH MỚI ---
            
            // KH phòng cháy chữa cháy
            "kh_pccc" => new Dictionary<string, string>
            {
                {"org_name", "UBND xã Gia Kiệm"},
                {"period", "Năm 2026"},
                {"objectives", "Không để xảy ra cháy lớn gây thiệt hại nghiêm trọng. 100% cơ sở kinh doanh có phương án PCCC. 100% khu dân cư có tổ PCCC tại chỗ."},
                {"tasks", "I. CÔNG TÁC TUYÊN TRUYỀN:\n- Tổ chức 17 buổi tuyên truyền tại 17 ấp (mỗi ấp 1 buổi/quý)\n- Phát 5.000 tờ rơi hướng dẫn PCCC tại nhà\n- Lắp 10 bảng tuyên truyền tại khu vực đông dân cư\n\nII. KIỂM TRA, XỬ LÝ:\n- Kiểm tra 100% cơ sở kinh doanh (85 cơ sở), nhà hàng, quán karaoke (12 cơ sở)\n- Kiểm tra hệ thống điện tại chợ Gia Kiệm, chợ ấp 6\n- Xử phạt nghiêm các trường hợp vi phạm\n\nIII. TỔ CHỨC LỰC LƯỢNG:\n- Thành lập/kiện toàn 17 tổ PCCC tại 17 ấp (mỗi tổ 10-15 người)\n- Tập huấn nghiệp vụ PCCC cho 250 người (2 đợt: tháng 3 và tháng 9)\n- Diễn tập PCCC tại chợ Gia Kiệm (tháng 5/2026)\n\nIV. PHƯƠNG TIỆN:\n- Mua sắm 34 bình chữa cháy (2 bình/ấp)\n- Lắp 5 trụ nước chữa cháy tại khu vực trọng điểm"},
                {"budget", "120 triệu đồng (ngân sách xã 80 triệu, hỗ trợ thành phố 40 triệu)"},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // KH tuyên truyền bầu cử
            "kh_baucu" => new Dictionary<string, string>
            {
                {"event_name", "Tuyên truyền cuộc bầu cử đại biểu Quốc hội khóa XVI và đại biểu HĐND các cấp nhiệm kỳ 2026-2031"},
                {"time_place", "Từ tháng 02/2026 đến ngày bầu cử (dự kiến 23/05/2026)\nĐịa bàn: 17 ấp, 5 giáo xứ, các trường học, cơ quan đoàn thể"},
                {"purpose", "Nâng cao nhận thức của cử tri về ý nghĩa, tầm quan trọng của cuộc bầu cử. Vận động 100% cử tri đi bỏ phiếu. Tạo không khí phấn khởi, tin tưởng trong nhân dân."},
                {"program", "GIAI ĐOẠN 1 (Tháng 2-3/2026):\n- Treo 50 băng-rôn, 200 cờ phướn trên các tuyến đường chính\n- Phát 10.000 tờ bướm giới thiệu Luật Bầu cử\n- Tuyên truyền trên loa phát thanh 17 ấp (3 lần/tuần)\n\nGIAI ĐOẠN 2 (Tháng 4-5/2026):\n- 17 buổi tiếp xúc cử tri với ứng cử viên tại 17 ấp\n- Phối hợp 5 giáo xứ tuyên truyền sau thánh lễ Chúa nhật\n- Hội nghị cử tri trẻ (thanh niên 18-30 tuổi)\n- Trang trí khánh tiết tại 25 điểm bỏ phiếu\n\nNGÀY BẦU CỬ:\n- Lễ khai mạc bỏ phiếu tại 25 khu vực bỏ phiếu\n- Xe loa tuyên truyền đi 17 ấp từ 5h00"},
                {"budget", "85 triệu đồng (ngân sách bầu cử cấp trên cấp)"},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // KH chuyển đổi số
            "kh_chuyendoiso" => new Dictionary<string, string>
            {
                {"org_name", "UBND xã Gia Kiệm"},
                {"period", "Năm 2026"},
                {"objectives", "1. Chính quyền số: 100% văn bản điện tử, 80% TTHC trực tuyến mức 3-4\n2. Kinh tế số: 50% hộ kinh doanh có tài khoản thanh toán điện tử\n3. Xã hội số: 70% người dân trưởng thành cài đặt app VNeID, 100% ấp có Zalo group"},
                {"tasks", "I. CHÍNH QUYỀN SỐ:\n- Triển khai phần mềm quản lý văn bản (VanBanPlus) cho 22 CC-VC\n- Số hóa 100% hồ sơ lưu trữ (ước tính 15.000 hồ sơ)\n- Lắp đặt wifi miễn phí tại bộ phận một cửa\n- Triển khai chữ ký số cho Chủ tịch, Phó CT, Văn phòng\n\nII. KINH TẾ SỐ:\n- Hỗ trợ 200 hộ kinh doanh tạo tài khoản QR thanh toán\n- Tập huấn bán hàng online cho 100 hộ nông dân (sản phẩm OCOP)\n- Thí điểm chợ không tiền mặt tại chợ Gia Kiệm\n\nIII. XÃ HỘI SỐ:\n- 17 buổi hướng dẫn cài VNeID tại 17 ấp\n- Tạo fanpage \"UBND xã Gia Kiệm\" trên Facebook/Zalo\n- Tập huấn an toàn thông tin, phòng chống lừa đảo online cho 1.000 người"},
                {"budget", "250 triệu đồng (ngân sách xã 100tr, thành phố hỗ trợ 100tr, xã hội hóa 50tr)"},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // --- THÔNG BÁO MỚI ---
            
            // TB lịch tiếp công dân
            "tb_tiepcongdan" => new Dictionary<string, string>
            {
                {"meeting_name", "Lịch tiếp công dân định kỳ của lãnh đạo UBND xã tháng 3/2026"},
                {"time", "Mỗi thứ Ba và thứ Năm hàng tuần, từ 7h30 - 11h30 và 13h30 - 16h30"},
                {"location", "Phòng Tiếp công dân UBND xã Gia Kiệm (Tầng 1, cạnh Bộ phận Một cửa)"},
                {"participants", "- Tuần 1 (03-07/03): Ông Nguyễn Thanh Tùng - Chủ tịch UBND xã\n- Tuần 2 (10-14/03): Ông Trần Văn Hải - Phó CT UBND phụ trách Kinh tế\n- Tuần 3 (17-21/03): Bà Lê Thị Hoa - Phó CT UBND phụ trách Văn xã\n- Tuần 4 (24-28/03): Ông Nguyễn Thanh Tùng - Chủ tịch UBND xã\n\nNgoài ra, Chủ tịch UBND xã tiếp công dân đột xuất khi có vụ việc phức tạp, đông người."},
                {"agenda", "Tiếp nhận và giải quyết khiếu nại, tố cáo, kiến nghị, phản ánh của công dân theo quy định của Luật Tiếp công dân năm 2013.\n\nCông dân khi đến tiếp mang theo: CCCD/CMND, đơn thư (nếu có), tài liệu liên quan."},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // TB tuyển dụng công chức
            "tb_tuyendung" => new Dictionary<string, string>
            {
                {"event_name", "Tuyển dụng công chức cấp xã năm 2026"},
                {"participants", "Công dân Việt Nam, đủ 18 tuổi trở lên, có đủ sức khỏe, phẩm chất đạo đức tốt, không trong thời gian bị truy cứu trách nhiệm hình sự"},
                {"content", "UBND xã Gia Kiệm thông báo tuyển dụng 02 công chức:\n\n1. VỊ TRÍ 1: Công chức Địa chính - Xây dựng\n- Số lượng: 01\n- Yêu cầu: Tốt nghiệp ĐH trở lên ngành Quản lý đất đai, Xây dựng, Kiến trúc\n- Ưu tiên: Có chứng chỉ tin học, ngoại ngữ, có kinh nghiệm\n\n2. VỊ TRÍ 2: Công chức Văn hóa - Xã hội\n- Số lượng: 01\n- Yêu cầu: Tốt nghiệp ĐH trở lên ngành CTXH, Văn hóa, Xã hội học\n- Ưu tiên: Người địa phương, có kinh nghiệm công tác đoàn thể"},
                {"conclusion", "Hồ sơ gồm: Đơn xin việc, Sơ yếu lý lịch, Bản sao bằng cấp, CCCD, Giấy khám sức khỏe.\nThời gian nhận hồ sơ: Từ 01/03/2026 đến 31/03/2026\nĐịa điểm: Văn phòng UBND xã Gia Kiệm\nHình thức tuyển: Thi tuyển (viết + phỏng vấn)\nDự kiến thi: Tháng 4/2026"},
                {"tasks", "- Văn phòng UBND xã tiếp nhận hồ sơ và trả giấy hẹn\n- Phòng Nội vụ thành phố tổ chức thi tuyển theo quy định\n- Kết quả được công bố trên website xã và niêm yết tại trụ sở"},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // TB nghỉ lễ
            "tb_nghile" => new Dictionary<string, string>
            {
                {"meeting_name", "Lịch nghỉ lễ Quốc khánh 2/9 năm 2026"},
                {"time", "Từ thứ Tư ngày 02/09/2026 đến hết thứ Năm ngày 03/09/2026 (nghỉ 02 ngày)"},
                {"location", "Áp dụng cho toàn thể cán bộ, công chức, viên chức, người lao động UBND xã Gia Kiệm"},
                {"participants", "- Toàn thể cán bộ, công chức, viên chức UBND xã (22 người)\n- Cán bộ không chuyên trách (35 người)\n- Trưởng 17 ấp"},
                {"agenda", "1. Trước khi nghỉ lễ:\n- Hoàn thành công việc đang giải quyết, không để tồn đọng\n- Tắt điện, nước, khóa cửa phòng làm việc\n- Bàn giao chìa khóa cho bảo vệ trực\n\n2. Trực lễ:\n- Ngày 02/9: Ông Trần Văn Hải (PCT) + 01 VP + 01 CA\n- Ngày 03/9: Bà Lê Thị Hoa (PCT) + 01 VP + 01 CA\n- SĐT trực: 0251.386.xxxx\n\n3. Lưu ý: Không uống rượu bia khi điều khiển phương tiện. Tuyên truyền nhân dân vui Tết an toàn."},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // --- NGHỊ QUYẾT MỚI ---
            
            // NQ chuyên đề xây dựng NTM
            "nq_chuyende" => new Dictionary<string, string>
            {
                {"level", "Xã"},
                {"subject", "Về tập trung nguồn lực xây dựng xã nông thôn mới nâng cao giai đoạn 2026-2030"},
                {"articles", "Điều 1. Mục tiêu\n- Đến năm 2028: Đạt chuẩn xã nông thôn mới nâng cao (19/19 tiêu chí nâng cao)\n- Đến năm 2030: Phấn đấu đạt xã nông thôn mới kiểu mẫu\n- Thu nhập bình quân đầu người đến 2030: 100 triệu đồng/năm\n- Tỷ lệ hộ nghèo dưới 0,5%\n\nĐiều 2. Nhiệm vụ trọng tâm\na) Phát triển kinh tế: Xây dựng 3 sản phẩm OCOP đạt 3 sao trở lên, hỗ trợ 100 hộ chuyển đổi số trong sản xuất nông nghiệp\nb) Hạ tầng: Bê tông hóa 100% đường nội đồng, xây mới 5 nhà văn hóa ấp\nc) Môi trường: 100% rác thải được thu gom xử lý, 80% hộ phân loại rác tại nguồn\nd) Văn hóa: 85% ấp đạt ấp văn hóa, 90% gia đình đạt gia đình văn hóa\n\nĐiều 3. Nguồn lực\nTổng kinh phí dự kiến: 120 tỷ đồng (2026-2030)\n- Ngân sách nhà nước: 60 tỷ (50%)\n- Vốn doanh nghiệp: 30 tỷ (25%)\n- Nhân dân đóng góp: 20 tỷ (17%)\n- Nguồn khác: 10 tỷ (8%)"},
                {"effective_date", "Kể từ ngày ký"},
                {"chairman_name", "Nguyễn Thanh Tùng"}
            },
            
            // --- CHỈ THỊ ---
            
            // CT tăng cường ANTT dịp Tết (mapped to Công văn chung)
            "ct_antt" => new Dictionary<string, string>
            {
                {"from_org", "UBND xã Gia Kiệm"},
                {"to_org", "Công an xã, BCH Quân sự xã, các ban ngành đoàn thể, Trưởng 17 ấp"},
                {"subject", "Tăng cường ANTT, ATXH dịp Tết Nguyên đán Bính Ngọ 2026"},
                {"content", "Để đảm bảo nhân dân đón Tết Nguyên đán vui tươi, lành mạnh, an toàn, UBND xã Gia Kiệm yêu cầu:\n\n1. Công an xã:\n- Tăng cường tuần tra 24/24, trọng tâm khu vực chợ, nhà thờ, trường học\n- Triển khai 3 tổ tuần tra cơ động (mỗi tổ 5 người), trực 100% quân số từ 28 Tết - Mùng 5\n- Kiểm tra xử lý nghiêm pháo nổ, đua xe, cờ bạc, ma túy\n- Phối hợp 17 ấp lập danh sách đối tượng cần quản lý\n\n2. Ban Chỉ huy Quân sự xã:\n- Duy trì chế độ trực sẵn sàng chiến đấu\n- Phối hợp Công an tuần tra vùng giáp ranh\n\n3. Các ban ngành, đoàn thể:\n- MTTQ, Hội PN, Đoàn TN: Vận động nhân dân không đốt pháo, không cờ bạc\n- Ban VH-XH: Tổ chức các hoạt động vui Tết lành mạnh\n- 5 giáo xứ: Phối hợp tuyên truyền sau thánh lễ\n\n4. Trưởng 17 ấp:\n- Nắm tình hình địa bàn, báo cáo hàng ngày về UBND xã\n- Hòa giải kịp thời tranh chấp, mâu thuẫn phát sinh\n- Báo cáo ngay khi có vụ việc bất thường (hotline: 0251.386.xxxx)"},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            // CT phòng chống dịch bệnh (mapped to Công văn chung)
            "ct_phongdich" => new Dictionary<string, string>
            {
                {"from_org", "UBND xã Gia Kiệm"},
                {"to_org", "Trạm Y tế xã, các trường học, Ban VH-XH, Trưởng 17 ấp"},
                {"subject", "Tăng cường phòng chống dịch sốt xuất huyết trên địa bàn xã Gia Kiệm"},
                {"content", "Trước tình hình dịch sốt xuất huyết đang diễn biến phức tạp trên địa bàn tỉnh Đồng Nai (tính đến 10/02/2026 đã ghi nhận 1.250 ca, tăng 35% so cùng kỳ), trong đó xã Gia Kiệm đã ghi nhận 8 ca (ấp 3: 3 ca, ấp 7: 2 ca, ấp 11: 2 ca, ấp 15: 1 ca), UBND xã yêu cầu:\n\n1. Trạm Y tế xã:\n- Giám sát chặt tình hình dịch bệnh, báo cáo hàng ngày\n- Tổ chức phun thuốc diệt muỗi tại 4 ấp có ca bệnh (ấp 3, 7, 11, 15) trong vòng 48h\n- Chuẩn bị đầy đủ thuốc, vật tư y tế, giường bệnh\n- Hướng dẫn người dân cách nhận biết triệu chứng và xử lý ban đầu\n\n2. Trưởng 17 ấp:\n- Phát động chiến dịch diệt lăng quăng hàng tuần (Thứ 7)\n- Rà soát 100% hộ dân, phát hiện và xử lý các ổ nước đọng\n- Báo cáo ngay khi phát hiện ca nghi ngờ\n\n3. Ban Văn hóa - Xã hội:\n- Phát 5.000 tờ rơi hướng dẫn phòng bệnh\n- Phát thanh tuyên truyền 3 lần/ngày trên loa 17 ấp\n\n4. Các trường học:\n- Vệ sinh trường lớp, diệt lăng quăng hàng tuần\n- Theo dõi sức khỏe học sinh, cho nghỉ khi có triệu chứng sốt"},
                {"signer_name", "Nguyễn Thanh Tùng"},
                {"signer_title", "Chủ tịch UBND xã"}
            },
            
            _ => new Dictionary<string, string>()
        };
    }

    private string GetFieldLabel(string field)
    {
        return field switch
        {
            // === Thông tin cơ quan ===
            "from_org" => "🏢 Cơ quan ban hành",
            "to_org" => "📨 Cơ quan nhận",
            "to_department" => "🏛️ Sở/Ban/Ngành nhận",
            "org_name" => "🏛️ Tên cơ quan/tổ chức",
            "from_unit" => "🏢 Đơn vị cũ",
            "to_unit" => "🏢 Đơn vị mới/nhận",
            "copy_org" => "🏢 Cơ quan sao lục",
            "implementing_unit" => "⚙️ Đơn vị thực hiện",
            
            // === Nội dung văn bản ===
            "subject" => "📋 Vấn đề/Tiêu đề",
            "content" => "📝 Nội dung chính",
            "reason" => "💡 Lý do",
            "purpose" => "🎯 Mục đích",
            "proposal" => "📊 Đề xuất",
            "proposals" => "💡 Đề xuất, kiến nghị",
            "objectives" => "🎯 Mục tiêu",
            "tasks" => "📋 Nhiệm vụ",
            "articles" => "📜 Các điều khoản",
            "program" => "📜 Chương trình",
            "agenda" => "📋 Nội dung cuộc họp",
            "conclusion" => "✅ Kết luận",
            "legal_basis" => "📖 Căn cứ pháp lý",
            
            // === Người ký / Người liên quan ===
            "signer_name" => "✍️ Người ký",
            "signer_title" => "👔 Chức danh người ký",
            "signer" => "✍️ Người ký",
            "chairman_name" => "👨‍💼 Chủ tịch",
            "principal_name" => "👨‍💼 Hiệu trưởng",
            "person_name" => "👤 Họ tên cán bộ",
            "person" => "👤 Họ tên",
            "citizen_name" => "👤 Họ tên công dân",
            "patient_name" => "🧑‍⚕️ Họ tên bệnh nhân",
            "student_name" => "🎓 Họ tên học sinh",
            "recipient" => "📬 Đơn vị/Người nhận",
            "recipients" => "📬 Nơi nhận",
            "grantor" => "👤 Người ủy quyền",
            "grantee" => "👤 Người được ủy quyền",
            "participants" => "👥 Thành phần tham dự",
            "members" => "👥 Danh sách thành viên",
            "students" => "🎓 Danh sách học sinh",
            "beneficiaries" => "👥 Đối tượng thụ hưởng",
            
            // === Chức vụ / Vị trí ===
            "current_position" => "💼 Chức vụ hiện tại",
            "new_position" => "⭐ Chức vụ mới",
            "level" => "🏛️ Cấp (Tỉnh/Huyện/Xã)",
            "ranking" => "🏅 Xếp loại",
            
            // === Thời gian / Địa điểm ===
            "time" => "⏰ Thời gian",
            "time_place" => "⏰ Thời gian, địa điểm",
            "location" => "📍 Địa điểm",
            "address" => "📍 Địa chỉ",
            "effective_date" => "📅 Ngày hiệu lực",
            "period" => "📆 Kỳ báo cáo/kế hoạch",
            "from_date" => "📅 Từ ngày",
            "to_date" => "📅 Đến ngày",
            "birth_date" => "📅 Ngày sinh",
            "exam_date" => "📅 Ngày khám",
            "meeting_time" => "⏰ Thời gian họp",
            "graduation_year" => "📅 Năm tốt nghiệp",
            "school_year" => "📅 Năm học",
            "year" => "📅 Năm",
            
            // === Khen thưởng / Kỷ luật ===
            "award_type" => "🏆 Hình thức khen thưởng",
            "achievement" => "✨ Thành tích",
            "achievements" => "✅ Kết quả đạt được",
            "reward_type" => "🏆 Hình thức khen thưởng",
            "reward_proposal" => "🏆 Đề nghị khen thưởng",
            "collective_achievements" => "✅ Thành tích tập thể",
            "violation" => "⚠️ Hành vi vi phạm",
            "penalty" => "⚖️ Hình thức xử phạt",
            "discipline_type" => "⚠️ Hình thức kỷ luật",
            
            // === Báo cáo / Đánh giá ===
            "situation" => "📊 Tình hình",
            "results" => "📈 Kết quả",
            "result" => "📈 Kết quả",
            "challenges" => "⚠️ Tồn tại, hạn chế",
            "future_plans" => "🚀 Phương hướng tiếp theo",
            "next_plan" => "🚀 Kế hoạch tiếp theo",
            "evaluation" => "⭐ Đánh giá",
            "task_name" => "📌 Nhiệm vụ/Kế hoạch",
            "field" => "📂 Lĩnh vực",
            "solutions" => "💡 Giải pháp thực hiện",
            "targets" => "🎯 Chỉ tiêu",
            "implementation" => "⚙️ Tổ chức thực hiện",
            "year_targets" => "🎯 Chỉ tiêu năm",
            "criteria_status" => "📊 Tình trạng các tiêu chí",
            
            // === Sự kiện / Hội nghị ===
            "event_name" => "🎉 Tên sự kiện",
            "meeting_name" => "🤝 Tên cuộc họp",
            "reply_to_number" => "🔢 Trả lời công văn số",
            
            // === Dự án / Tài chính ===
            "project_name" => "🎯 Tên đề án/dự án",
            "budget" => "💰 Kinh phí",
            "support_type" => "📋 Hình thức hỗ trợ",
            "support_amount" => "💰 Mức hỗ trợ",
            
            // === Trường học ===
            "school_name" => "🏫 Tên trường",
            "grade" => "📚 Khối/Lớp",
            "class_name" => "📚 Tên lớp",
            "curriculum_plan" => "📖 Chương trình dạy học",
            "student_count" => "👥 Số lượng học sinh",
            "quality_stats" => "📊 Thống kê chất lượng",
            
            // === Y tế ===
            "medical_unit" => "🏥 Cơ sở y tế",
            "hospital" => "🏥 Bệnh viện",
            "from_hospital" => "🏥 Bệnh viện chuyển",
            "to_hospital" => "🏥 Bệnh viện nhận",
            "disease_name" => "🩺 Tên bệnh/dịch",
            "diagnosis" => "🩺 Chẩn đoán",
            "transfer_reason" => "📋 Lý do chuyển viện",
            "prevention_measures" => "🛡️ Biện pháp phòng chống",
            "measures" => "📋 Biện pháp thực hiện",
            "statistics" => "📊 Số liệu thống kê",
            "patient_count" => "👥 Số lượng bệnh nhân",
            "clinical_results" => "📈 Kết quả lâm sàng",
            "treatment_plan" => "📋 Phác đồ điều trị",
            "procedure_name" => "📋 Tên quy trình",
            "test_type" => "🔬 Loại xét nghiệm",
            "test_result" => "📈 Kết quả xét nghiệm",
            "area" => "📍 Khu vực/Địa bàn",
            
            // === Hành chính xã/phường ===
            "ward_name" => "🏘️ Tên xã/phường",
            "marital_status" => "👪 Tình trạng hôn nhân",
            "population" => "👥 Dân số",
            "birth_death_rate" => "📊 Tỷ lệ sinh/tử",
            "economy" => "📈 Kinh tế",
            "social" => "🏘️ Xã hội",
            "disaster_type" => "⚠️ Loại thiên tai",
            "risk_areas" => "📍 Vùng có nguy cơ",
            "rescue_forces" => "🚑 Lực lượng cứu hộ",
            "evacuation_plan" => "🗺️ Phương án sơ tán",
            "reform_content" => "📋 Nội dung cải cách",
            "procedures" => "📋 Thủ tục",
            
            // === Sao lục / Phụ lục ===
            "original_document" => "📄 Văn bản gốc",
            "original_saoy" => "📄 Bản gốc sao y",
            "extract_section" => "📋 Phần trích sao",
            "document_ref" => "📎 Số hiệu văn bản",
            "documents" => "📎 Danh sách văn bản",
            "parent_document" => "📄 Văn bản chính",
            "appendix_title" => "📋 Tiêu đề phụ lục",
            
            _ => $"📝 {FormatFieldName(field)}"
        };
    }
    
    /// <summary>
    /// Chuyển field name kỹ thuật thành tên thân thiện (fallback)
    /// VD: "school_name" → "Tên trường", "from_date" → "Từ ngày"
    /// </summary>
    private static string FormatFieldName(string field)
    {
        // Thay _ thành khoảng trắng, viết hoa chữ đầu
        var words = field.Split('_');
        return string.Join(" ", words.Select(w => 
            w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w));
    }

    private string GetFieldHint(string field)
    {
        return field switch
        {
            // Cơ quan
            "from_org" => "Ví dụ: UBND xã Tân Thành",
            "to_org" => "Ví dụ: Sở Nội vụ tỉnh Bình Dương",
            "to_department" => "Ví dụ: Sở Giáo dục và Đào tạo",
            "org_name" => "Ví dụ: UBND xã Tân Phú",
            "copy_org" => "Ví dụ: Văn phòng UBND huyện",
            "implementing_unit" => "Ví dụ: Phòng Tài chính - Kế hoạch",
            "from_unit" => "Ví dụ: Phòng Nội vụ huyện ABC",
            "to_unit" => "Ví dụ: UBND xã XYZ",
            
            // Nội dung
            "subject" => "Vấn đề văn bản cần soạn",
            "content" => "Nội dung chi tiết văn bản...",
            "reason" => "Lý do ban hành văn bản",
            "purpose" => "Mục đích của sự kiện/hoạt động",
            "proposal" => "Nội dung đề xuất, kiến nghị",
            "proposals" => "Các đề xuất, kiến nghị cụ thể...",
            "objectives" => "Các mục tiêu cần đạt được...",
            "tasks" => "Danh sách nhiệm vụ cụ thể...",
            "articles" => "Nội dung các điều khoản...",
            "program" => "Chương trình, kịch bản chi tiết...",
            "agenda" => "Nội dung các phần trong cuộc họp...",
            "conclusion" => "Kết luận, quyết nghị...",
            "legal_basis" => "Căn cứ Luật, Nghị định, Thông tư...",
            "solutions" => "Các giải pháp cụ thể...",
            
            // Người ký
            "signer_name" => "Ví dụ: Nguyễn Văn A",
            "signer_title" => "Ví dụ: Chủ tịch UBND",
            "signer" => "Ví dụ: Nguyễn Văn A - Chủ tịch",
            "chairman_name" => "Ví dụ: Trần Văn B",
            "principal_name" => "Ví dụ: Lê Thị C",
            "person_name" => "Ví dụ: Nguyễn Văn A",
            "person" => "Ví dụ: Nguyễn Văn A",
            "citizen_name" => "Ví dụ: Trần Thị B",
            "patient_name" => "Ví dụ: Nguyễn Văn C",
            "student_name" => "Ví dụ: Lê Văn D",
            "recipient" => "Ví dụ: Sở Nội vụ tỉnh ABC",
            "recipients" => "Danh sách nơi nhận...",
            "grantor" => "Người ủy quyền",
            "grantee" => "Người được ủy quyền",
            "participants" => "Thành phần tham dự cuộc họp...",
            "members" => "Danh sách các thành viên...",
            "students" => "Danh sách học sinh...",
            "beneficiaries" => "Đối tượng được hỗ trợ...",
            
            // Chức vụ
            "current_position" => "Ví dụ: Trưởng phòng Nội vụ",
            "new_position" => "Ví dụ: Phó Chủ tịch UBND",
            "level" => "Ví dụ: Tỉnh, Huyện, hoặc Xã",
            "ranking" => "Ví dụ: Giỏi, Khá, Trung bình",
            
            // Thời gian
            "time" => "Ví dụ: 08h00 ngày 15/3/2026",
            "time_place" => "Ví dụ: 08h00, ngày 15/3/2026 tại Hội trường UBND",
            "location" => "Ví dụ: Hội trường UBND xã",
            "address" => "Ví dụ: 123 Nguyễn Huệ, phường 1, TP. HCM",
            "effective_date" => "Ví dụ: 01/01/2026",
            "period" => "Ví dụ: Quý I/2026 hoặc Năm 2025",
            "from_date" => "Ví dụ: 01/03/2026",
            "to_date" => "Ví dụ: 15/03/2026",
            "birth_date" => "Ví dụ: 15/05/1990",
            "exam_date" => "Ví dụ: 20/03/2026",
            "meeting_time" => "Ví dụ: 14h00, thứ Sáu ngày 21/3/2026",
            "graduation_year" => "Ví dụ: 2025",
            "school_year" => "Ví dụ: 2025-2026",
            "year" => "Ví dụ: 2026",
            
            // Khen thưởng / Kỷ luật
            "award_type" => "Ví dụ: Bằng khen, Giấy khen",
            "achievement" => "Mô tả thành tích cụ thể...",
            "achievements" => "Các kết quả đạt được...",
            "reward_type" => "Ví dụ: Bằng khen, Chiến sĩ thi đua",
            "reward_proposal" => "Đề nghị khen thưởng tập thể...",
            "collective_achievements" => "Thành tích tập thể trong năm...",
            "violation" => "Mô tả hành vi vi phạm...",
            "penalty" => "Hình thức xử phạt áp dụng...",
            "discipline_type" => "Ví dụ: Khiển trách, Cảnh cáo",
            
            // Báo cáo
            "situation" => "Mô tả tình hình hiện tại...",
            "results" or "result" => "Kết quả đạt được...",
            "challenges" => "Khó khăn, tồn tại, hạn chế...",
            "future_plans" or "next_plan" => "Phương hướng, nhiệm vụ tiếp theo...",
            "evaluation" => "Nhận xét, đánh giá...",
            "task_name" => "Ví dụ: Kiểm tra ATTP Quý I/2026",
            "field" => "Ví dụ: Giáo dục, Y tế, Nông nghiệp",
            "targets" or "year_targets" => "Các chỉ tiêu cần đạt...",
            "criteria_status" => "Tình trạng đạt/chưa đạt các tiêu chí...",
            "implementation" => "Cách tổ chức thực hiện...",
            
            // Sự kiện
            "event_name" => "Ví dụ: Lễ kỷ niệm 30/4",
            "meeting_name" => "Ví dụ: Họp UBND xã tháng 3/2026",
            "reply_to_number" => "Ví dụ: 123/UBND-VP ngày 01/3/2026",
            
            // Tài chính
            "project_name" => "Ví dụ: Xây dựng đường liên xã",
            "budget" => "Ví dụ: 500.000.000 đồng",
            "support_type" => "Ví dụ: Tiền mặt, Hiện vật",
            "support_amount" => "Ví dụ: 2.000.000 đồng/hộ",
            
            // Trường học
            "school_name" => "Ví dụ: Trường THCS Nguyễn Du",
            "grade" => "Ví dụ: Khối 9 hoặc Lớp 9A1",
            "class_name" => "Ví dụ: 9A1",
            "curriculum_plan" => "Nội dung chương trình giảng dạy...",
            "student_count" => "Ví dụ: 450 học sinh",
            "quality_stats" => "Thống kê tỷ lệ giỏi/khá/TB...",
            
            // Y tế
            "medical_unit" => "Ví dụ: Trạm Y tế xã Tân Phú",
            "hospital" => "Ví dụ: Bệnh viện Đa khoa tỉnh",
            "from_hospital" => "Ví dụ: BV Đa khoa huyện ABC",
            "to_hospital" => "Ví dụ: BV Chợ Rẫy TP.HCM",
            "disease_name" => "Ví dụ: Sốt xuất huyết, COVID-19",
            "diagnosis" => "Chẩn đoán bệnh...",
            "transfer_reason" => "Lý do cần chuyển viện...",
            "prevention_measures" or "measures" => "Các biện pháp phòng chống...",
            "statistics" => "Số liệu ca bệnh, tử vong...",
            "patient_count" => "Ví dụ: 1.200 lượt",
            "clinical_results" => "Kết quả điều trị lâm sàng...",
            "treatment_plan" => "Phác đồ, kế hoạch điều trị...",
            "procedure_name" => "Ví dụ: Quy trình khám sức khỏe",
            "test_type" => "Ví dụ: Xét nghiệm máu, PCR",
            "test_result" => "Kết quả xét nghiệm...",
            "area" => "Ví dụ: Xã Tân Phú, huyện ABC",
            
            // Hành chính xã/phường
            "ward_name" => "Ví dụ: Xã Tân Thành",
            "marital_status" => "Ví dụ: Độc thân, Đã kết hôn",
            "population" => "Ví dụ: 12.500 người",
            "birth_death_rate" => "Ví dụ: Sinh 1.2%, Tử 0.5%",
            "economy" => "Tình hình kinh tế địa phương...",
            "social" => "Tình hình xã hội, an ninh...",
            "disaster_type" => "Ví dụ: Bão, Lũ lụt, Sạt lở",
            "risk_areas" => "Khu vực có nguy cơ cao...",
            "rescue_forces" => "Lực lượng, phương tiện cứu hộ...",
            "evacuation_plan" => "Phương án di dời, sơ tán...",
            "reform_content" => "Nội dung cải cách hành chính...",
            "procedures" => "Các thủ tục hành chính...",
            
            // Sao lục / Phụ lục
            "original_document" or "original_saoy" => "Ví dụ: QĐ số 123/QĐ-UBND ngày 01/3/2026",
            "extract_section" => "Phần nội dung cần trích sao...",
            "document_ref" => "Ví dụ: Số 456/BC-UBND",
            "documents" => "Danh sách văn bản kèm theo...",
            "parent_document" => "Ví dụ: QĐ số 789/QĐ-UBND",
            "appendix_title" => "Ví dụ: Danh sách cán bộ",
            
            _ => $"Nhập thông tin {FormatFieldName(field).ToLower()}..."
        };
    }

    private double GetFieldHeight(string field)
    {
        return field switch
        {
            // Các trường cần nhập nhiều dòng
            "content" or "achievements" or "challenges" or "tasks" or "proposals" 
                or "situation" or "results" or "members" or "articles" or "program"
                or "future_plans" or "legal_basis" or "violation" or "penalty"
                or "objectives" or "conclusion" or "agenda" or "participants"
                or "students" or "measures" or "prevention_measures" or "statistics"
                or "clinical_results" or "treatment_plan" or "procedures"
                or "curriculum_plan" or "quality_stats" or "rescue_forces"
                or "evacuation_plan" or "reform_content" or "economy" or "social"
                or "collective_achievements" or "implementation" or "criteria_status"
                or "next_plan" or "solutions" or "beneficiaries" or "recipients"
                or "documents" or "extract_section" or "risk_areas" => 120,
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
        if (!AiPromoHelper.CheckOrShowPromo(this)) return;

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

            // System instruction chuyên nghiệp
            var systemInstruction = BuildSystemInstruction();

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

    private string BuildSystemInstruction()
    {
        var templateType = _selectedTemplate?.Type.ToString() ?? "CongVan";
        
        return $@"Bạn là CHUYÊN VIÊN VĂN THƯ CAO CẤP tại UBND cấp xã/phường Việt Nam với 20 năm kinh nghiệm soạn thảo văn bản hành chính. Bạn nắm vững:
- Luật Ban hành VBQPPL năm 2015 (sửa đổi 2020)
- Nghị định 30/2020/NĐ-CP ngày 05/3/2020 về công tác văn thư
- Nghị định 154/2020/NĐ-CP sửa đổi NĐ 34/2016 về VBQPPL
- Thông tư 01/2011/TT-BNV hướng dẫn thể thức và kỹ thuật trình bày văn bản hành chính
- Quy trình soạn thảo, trình ký, ban hành văn bản tại UBND cấp xã

NHIỆM VỤ: Soạn thảo văn bản hành chính HOÀN CHỈNH, ĐÚNG THỂ THỨC, sẵn sàng in ấn và ban hành.

═══════════════════════════════════════
QUY TẮC THỂ THỨC (Theo NĐ 30/2020):
═══════════════════════════════════════

1. QUỐC HIỆU VÀ TIÊU NGỮ (bắt buộc):
   CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
   Độc lập - Tự do - Hạnh phúc
   (Tiêu ngữ có gạch ngang ở giữa, có gạch liền phía dưới)

2. TÊN CƠ QUAN BAN HÀNH:
   - Cơ quan cấp trên (nếu có): UBND THÀNH PHỐ THỐNG NHẤT
   - Cơ quan ban hành: UBND XÃ GIA KIỆM (viết hoa, in đậm)
   - Giữa hai dòng có gạch ngang

3. SỐ VÀ KÝ HIỆU (Phụ lục III, NĐ 30/2020 — đầy đủ 29 loại VB hành chính):
   - Nghị quyết: Số:    /NQ-HĐND hoặc /NQ-UBND
   - Quyết định: Số:    /QĐ-UBND
   - Chỉ thị: Số:    /CT-UBND
   - Quy chế: Số:    /QC-UBND
   - Quy định: Số:    /QyĐ-UBND
   - Thông cáo: Số:    /TC-UBND
   - Thông báo: Số:    /TB-UBND
   - Hướng dẫn: Số:    /HD-UBND
   - Chương trình: Số:    /CTr-UBND
   - Kế hoạch: Số:    /KH-UBND
   - Phương án: Số:    /PA-UBND
   - Đề án: Số:    /ĐA-UBND
   - Dự án: Số:    /DA-UBND
   - Báo cáo: Số:    /BC-UBND
   - Biên bản: (không đánh số ký hiệu)
   - Tờ trình: Số:    /TTr-UBND
   - Hợp đồng: Số:    /HĐ-UBND
   - Công văn: Số:    /CV-UBND
   - Công điện: Số:    /CĐ-UBND
   - Bản ghi nhớ: (không đánh số ký hiệu)
   - Bản thỏa thuận: (không đánh số ký hiệu)
   - Giấy ủy quyền: Số:    /GUQ-UBND
   - Giấy mời: Số:    /GM-UBND
   - Giấy giới thiệu: Số:    /GGT-UBND
   - Giấy nghỉ phép: Số:    /GNP-UBND
   - Phiếu gửi: Số:    /PG-UBND
   - Phiếu chuyển: Số:    /PC-UBND
   - Phiếu báo: Số:    /PB-UBND
   - Thư công: (không đánh số ký hiệu)

4. ĐỊA DANH VÀ NGÀY THÁNG:
   ""Gia Kiệm, ngày ... tháng ... năm 2026""

5. TRÍCH YẾU NỘI DUNG (V/v):
   - Công văn: ""V/v [nội dung]""
   - QĐ/NQ: ""QUYẾT ĐỊNH / NGHỊ QUYẾT"" + ""Về việc [nội dung]""
   - Báo cáo: ""BÁO CÁO"" + ""[Về nội dung / Kết quả...]""

6. NỘI DUNG VĂN BẢN:
   - Công văn: Câu dẫn → Nội dung chính → Đề nghị → Kết
   - QĐ: Căn cứ → QUYẾT ĐỊNH: Điều 1, 2, 3...
   - Báo cáo: Phần I (Kết quả) → Phần II (Tồn tại) → Phần III (Phương hướng)
   - Tờ trình: Căn cứ → Lý do → Nội dung đề xuất → Kinh phí → Kiến nghị
   - Kế hoạch: Mục đích → Yêu cầu → Nội dung → Tổ chức thực hiện → Kinh phí
   - Thông báo: Nội dung thông báo → Thời gian → Địa điểm → Yêu cầu
   - Nghị quyết: Căn cứ → Điều khoản

7. NƠI NHẬN:
   ""Nơi nhận:"" (in nghiêng, in đậm)
   - Như trên;
   - [Các đơn vị liên quan];
   - Lưu: VT, [bộ phận].

8. CHỮ KÝ:
   [CHỨC DANH IN HOA]
   (Chữ ký, đóng dấu)
   [Họ và tên]

═══════════════════════════════════════
QUY TẮC VIẾT:
═══════════════════════════════════════

- VĂN PHONG: Hành chính chuẩn, trang trọng, mạch lạc, không dùng khẩu ngữ
- NGÔI THỨ: Ngôi thứ ba (""UBND xã"", ""Chủ tịch UBND""), không dùng ""tôi"", ""chúng tôi""
- CÂU CHỮ: Ngắn gọn, rõ ràng, chính xác, không mơ hồ
- SỐ LIỆU: Ghi kèm đơn vị, viết bằng số + chữ nếu là tiền/diện tích quan trọng
- VIỆN DẪN: Ghi đầy đủ số hiệu VB (VD: ""Theo Quyết định số 15/QĐ-UBND ngày 10/01/2026"")
- PLAIN TEXT: KHÔNG dùng markdown (**, *, #, ```), KHÔNG dùng emoji
- XUỐNG DÒNG: Bình thường, KHÔNG viết literal \\n
- Gạch đầu dòng dùng dấu ""-""
- TRÌNH BÀY: Sạch sẽ, có thể in trực tiếp lên giấy A4

═══════════════════════════════════════
BỐI CẢNH ĐỊA PHƯƠNG (xã Gia Kiệm):
═══════════════════════════════════════
- Xã Gia Kiệm, thành phố Thống Nhất, tỉnh Đồng Nai
- Dân số: 79.274 nhân khẩu, 19.818 hộ
- 17 ấp, 5 giáo xứ Công giáo (96% dân theo đạo)
- Xã loại I, đang xây dựng nông thôn mới nâng cao
- Cơ quan cấp trên: UBND thành phố Thống Nhất, UBND tỉnh Đồng Nai

Hãy soạn văn bản loại: {templateType}";
    }

    private string BuildPrompt()
    {
        if (_selectedTemplate == null) return "";

        var templateType = _selectedTemplate.Type;
        var fieldValues = new Dictionary<string, string>();
        foreach (var kvp in _fieldInputs)
        {
            fieldValues[kvp.Key] = kvp.Value.Text;
        }

        // Build structured prompt based on template type
        var prompt = BuildStructuredPrompt(templateType, fieldValues);
        
        return prompt;
    }

    private string BuildStructuredPrompt(DocumentType templateType, Dictionary<string, string> fields)
    {
        string GetField(string key) => fields.TryGetValue(key, out var val) ? val : "";

        return templateType switch
        {
            DocumentType.CongVan => BuildCongVanPrompt(fields, GetField),
            DocumentType.QuyetDinh => BuildQuyetDinhPrompt(fields, GetField),
            DocumentType.BaoCao => BuildBaoCaoPrompt(fields, GetField),
            DocumentType.ToTrinh => BuildToTrinhPrompt(fields, GetField),
            DocumentType.KeHoach => BuildKeHoachPrompt(fields, GetField),
            DocumentType.ThongBao => BuildThongBaoPrompt(fields, GetField),
            DocumentType.NghiQuyet => BuildNghiQuyetPrompt(fields, GetField),
            // === 22 loại VB bổ sung — NĐ 30/2020 ===
            DocumentType.ChiThi => BuildChiThiPrompt(fields, GetField),
            DocumentType.QuyChE => BuildQuyChEPrompt(fields, GetField),
            DocumentType.QuyDinh => BuildQuyDinhPrompt(fields, GetField),
            DocumentType.ThongCao => BuildThongCaoPrompt(fields, GetField),
            DocumentType.HuongDan => BuildHuongDanPrompt(fields, GetField),
            DocumentType.ChuongTrinh => BuildChuongTrinhPrompt(fields, GetField),
            DocumentType.PhuongAn or DocumentType.DeAn or DocumentType.DuAn => BuildDeAnDuAnPrompt(templateType, fields, GetField),
            DocumentType.BienBan => BuildBienBanPrompt(fields, GetField),
            DocumentType.HopDong => BuildHopDongPrompt(fields, GetField),
            DocumentType.CongDien => BuildCongDienPrompt(fields, GetField),
            DocumentType.BanGhiNho or DocumentType.BanThoaThuan => BuildThoaThuanPrompt(templateType, fields, GetField),
            DocumentType.GiayUyQuyen => BuildGiayUyQuyenPrompt(fields, GetField),
            DocumentType.GiayMoi => BuildGiayMoiPrompt(fields, GetField),
            DocumentType.GiayGioiThieu => BuildGiayGioiThieuPrompt(fields, GetField),
            DocumentType.GiayNghiPhep => BuildGiayNghiPhepPrompt(fields, GetField),
            DocumentType.PhieuGui or DocumentType.PhieuChuyen or DocumentType.PhieuBao => BuildPhieuPrompt(templateType, fields, GetField),
            DocumentType.ThuCong => BuildThuCongPrompt(fields, GetField),
            _ => BuildGenericPrompt(fields, GetField)
        };
    }

    private string BuildCongVanPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        var hasReplyTo = !string.IsNullOrEmpty(get("reply_to_number"));
        var hasProposal = !string.IsNullOrEmpty(get("proposal"));
        
        var prompt = $@"Soạn CÔNG VĂN hoàn chỉnh, đúng thể thức NĐ 30/2020:

THÔNG TIN:
- Cơ quan ban hành: {get("from_org")}
- Nơi nhận chính: {(string.IsNullOrEmpty(get("to_org")) ? get("to_department") : get("to_org"))}
- Vấn đề (V/v): {get("subject")}
- Người ký: {(string.IsNullOrEmpty(get("signer_name")) ? get("chairman_name") : get("signer_name"))}
- Chức danh: {get("signer_title")}

NỘI DUNG CHÍNH CẦN ĐƯA VÀO:
{get("content")}";

        if (hasReplyTo)
            prompt += $"\n\nĐÂY LÀ CÔNG VĂN TRẢ LỜI công văn số: {get("reply_to_number")}. Mở đầu bằng: \"Phúc đáp Công văn số... ngày... của... về việc..., UBND xã Gia Kiệm xin trả lời như sau:\"";

        if (hasProposal)
            prompt += $"\n\nĐỀ XUẤT/KIẾN NGHỊ:\n{get("proposal")}";

        prompt += @"

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ (đầy đủ)
2. Tên cơ quan cấp trên + cơ quan ban hành
3. Số/ký hiệu: Số:    /CV-UBND
4. Địa danh, ngày tháng
5. V/v: [trích yếu]
6. Kính gửi: [nơi nhận]
7. Thân văn bản:
   - Câu dẫn nhập (lý do, căn cứ)
   - Nội dung chính (diễn giải chi tiết, có số liệu nếu cần)
   - Đề nghị/kiến nghị (nêu rõ yêu cầu cụ thể)
   - Câu kết (""Kính đề nghị... xem xét, giải quyết./."")
8. Nơi nhận (liệt kê đầy đủ)
9. Chức danh + tên người ký

VĂN PHONG: Hành chính chuẩn, trang trọng, mạch lạc. Viện dẫn căn cứ pháp lý phù hợp nếu có.";

        return prompt;
    }

    private string BuildQuyetDinhPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        var templateName = _selectedTemplate?.Name ?? "";
        
        var prompt = $@"Soạn QUYẾT ĐỊNH hoàn chỉnh, đúng thể thức NĐ 30/2020:

LOẠI QUYẾT ĐỊNH: {templateName}

THÔNG TIN:";

        // Dynamic based on what fields are available
        if (!string.IsNullOrEmpty(get("person_name")))
            prompt += $"\n- Đối tượng: {get("person_name")}";
        if (!string.IsNullOrEmpty(get("current_position")))
            prompt += $"\n- Chức vụ hiện tại: {get("current_position")}";
        if (!string.IsNullOrEmpty(get("from_unit")))
            prompt += $"\n- Đơn vị cũ: {get("from_unit")}";
        if (!string.IsNullOrEmpty(get("to_unit")))
            prompt += $"\n- Đơn vị mới: {get("to_unit")}";
        if (!string.IsNullOrEmpty(get("new_position")))
            prompt += $"\n- Chức vụ mới: {get("new_position")}";
        if (!string.IsNullOrEmpty(get("effective_date")))
            prompt += $"\n- Ngày hiệu lực: {get("effective_date")}";
        if (!string.IsNullOrEmpty(get("award_type")))
            prompt += $"\n- Hình thức khen thưởng: {get("award_type")}";
        if (!string.IsNullOrEmpty(get("recipient")))
            prompt += $"\n- Đối tượng khen thưởng: {get("recipient")}";
        if (!string.IsNullOrEmpty(get("achievement")))
            prompt += $"\n- Thành tích: {get("achievement")}";
        if (!string.IsNullOrEmpty(get("org_name")))
            prompt += $"\n- Tên tổ chức: {get("org_name")}";
        if (!string.IsNullOrEmpty(get("members")))
            prompt += $"\n- Thành viên: {get("members")}";
        if (!string.IsNullOrEmpty(get("tasks")))
            prompt += $"\n- Nhiệm vụ: {get("tasks")}";
        if (!string.IsNullOrEmpty(get("project_name")))
            prompt += $"\n- Tên đề án/dự án: {get("project_name")}";
        if (!string.IsNullOrEmpty(get("objectives")))
            prompt += $"\n- Mục tiêu/Nội dung: {get("objectives")}";
        if (!string.IsNullOrEmpty(get("budget")))
            prompt += $"\n- Kinh phí: {get("budget")}";
        if (!string.IsNullOrEmpty(get("implementing_unit")))
            prompt += $"\n- Đơn vị thực hiện: {get("implementing_unit")}";

        var signerName = string.IsNullOrEmpty(get("signer_name")) ? get("chairman_name") : get("signer_name");
        var signerTitle = get("signer_title");
        if (!string.IsNullOrEmpty(signerName))
            prompt += $"\n- Người ký: {signerName}";
        if (!string.IsNullOrEmpty(signerTitle))
            prompt += $"\n- Chức danh: {signerTitle}";

        prompt += @"

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan ban hành (UBND XÃ GIA KIỆM)
3. Số/ký hiệu: Số:    /QĐ-UBND
4. Tiêu đề: QUYẾT ĐỊNH + ""Về việc [nội dung]""
5. Phần CĂN CỨ (bắt buộc):
   - Căn cứ Luật Tổ chức chính quyền địa phương 2015 (sửa đổi 2019)
   - Căn cứ các luật/nghị định chuyên ngành liên quan
   - Căn cứ tờ trình, đề nghị (nếu có)
   - Xét đề nghị của... (nếu có)
6. QUYẾT ĐỊNH:
   - Điều 1: Nội dung chính (chi tiết, cụ thể)
   - Điều 2: Trách nhiệm thi hành, hiệu lực
   - Điều 3: Tổ chức thực hiện, nơi gửi
7. Nơi nhận
8. Chức danh + tên người ký

LƯU Ý: Viện dẫn đúng căn cứ pháp lý. Mỗi Điều phải rõ ràng, cụ thể, có tính bắt buộc thi hành.";

        return prompt;
    }

    private string BuildBaoCaoPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        var templateName = _selectedTemplate?.Name ?? "";
        
        return $@"Soạn BÁO CÁO hoàn chỉnh, đúng thể thức NĐ 30/2020:

LOẠI BÁO CÁO: {templateName}

THÔNG TIN:
- Đơn vị báo cáo: {get("org_name")}
- Kỳ báo cáo/Lĩnh vực: {(string.IsNullOrEmpty(get("period")) ? get("field") : get("period"))}
- Người ký: {get("signer_name")}, {get("signer_title")}

DỮ LIỆU ĐẦU VÀO:
{(string.IsNullOrEmpty(get("achievements")) ? "" : $"KẾT QUẢ ĐẠT ĐƯỢC:\n{get("achievements")}\n")}
{(string.IsNullOrEmpty(get("situation")) ? "" : $"TÌNH HÌNH:\n{get("situation")}\n")}
{(string.IsNullOrEmpty(get("results")) ? "" : $"KẾT QUẢ CỤ THỂ:\n{get("results")}\n")}
{(string.IsNullOrEmpty(get("challenges")) ? "" : $"TỒN TẠI, HẠN CHẾ:\n{get("challenges")}\n")}
{(string.IsNullOrEmpty(get("future_plans")) ? "" : $"PHƯƠNG HƯỚNG:\n{get("future_plans")}\n")}
{(string.IsNullOrEmpty(get("proposals")) ? "" : $"ĐỀ XUẤT, KIẾN NGHỊ:\n{get("proposals")}\n")}

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan: {get("org_name")}
3. Số/ký hiệu: Số:    /BC-UBND
4. Tiêu đề: BÁO CÁO + trích yếu nội dung
5. Nơi gửi (Kính gửi)
6. Câu dẫn nhập: ""Thực hiện [căn cứ]..., {get("org_name")} báo cáo kết quả... như sau:""
7. Thân báo cáo:
   PHẦN I: KẾT QUẢ THỰC HIỆN
   - Chia theo mục, có đánh số I, II, III hoặc 1, 2, 3
   - Mỗi mục: trình bày cụ thể, có số liệu
   - Dùng gạch đầu dòng cho chi tiết

   PHẦN II: TỒN TẠI, HẠN CHẾ VÀ NGUYÊN NHÂN
   - Nêu rõ khó khăn, vướng mắc
   - Phân tích nguyên nhân (chủ quan/khách quan)

   PHẦN III: PHƯƠNG HƯỚNG, NHIỆM VỤ [kỳ tiếp]
   - Nhiệm vụ trọng tâm
   - Giải pháp thực hiện
   - Kiến nghị cấp trên (nếu cần)

8. Câu kết: ""Trên đây là báo cáo... Kính đề nghị [cấp trên] xem xét, chỉ đạo./.""
9. Nơi nhận
10. Chức danh + tên người ký

LƯU Ý: Số liệu phải cụ thể, rõ ràng. Đánh giá khách quan, trung thực. Kiến nghị phải khả thi, sát thực tế.";
    }

    private string BuildToTrinhPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn TỜ TRÌNH hoàn chỉnh, đúng thể thức NĐ 30/2020:

THÔNG TIN:
- Cơ quan trình: {get("org_name")}
- Nơi nhận: {get("recipient")}
- Vấn đề trình: {(string.IsNullOrEmpty(get("subject")) ? get("proposal") : get("subject"))}
- Lý do: {get("reason")}
- Nội dung đề xuất: {(string.IsNullOrEmpty(get("content")) ? get("proposal") : get("content"))}
{(string.IsNullOrEmpty(get("budget")) ? "" : $"- Kinh phí: {get("budget")}")}
- Người ký: {get("signer_name")}, {get("signer_title")}

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan: {get("org_name")}
3. Số/ký hiệu: Số:    /TTr-UBND
4. Tiêu đề: TỜ TRÌNH + ""Về việc [nội dung]""
5. Kính gửi: {get("recipient")}
6. Thân tờ trình:
   I. CĂN CỨ, LÝ DO:
   - Căn cứ pháp lý (Luật, NĐ, QĐ liên quan)
   - Tình hình thực tế, sự cần thiết

   II. NỘI DUNG ĐỀ XUẤT:
   - Nội dung cụ thể, chi tiết
   - Phương án thực hiện
   - Nguồn lực cần thiết
   {(string.IsNullOrEmpty(get("budget")) ? "" : "- Kinh phí dự kiến (ghi rõ nguồn vốn)")}

   III. TỔ CHỨC THỰC HIỆN:
   - Phân công trách nhiệm
   - Tiến độ dự kiến

7. Câu kết: ""Kính trình {get("recipient")} xem xét, phê duyệt./.""
8. Nơi nhận
9. Chức danh + tên người ký

LƯU Ý: Lập luận chặt chẽ, viện dẫn căn cứ đầy đủ. Nội dung phải thuyết phục, khả thi.";
    }

    private string BuildKeHoachPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        var isEvent = !string.IsNullOrEmpty(get("event_name")) || !string.IsNullOrEmpty(get("program"));
        var title = isEvent ? get("event_name") : $"Công tác {get("period")}";
        
        return $@"Soạn KẾ HOẠCH hoàn chỉnh, đúng thể thức NĐ 30/2020:

THÔNG TIN:
- Cơ quan ban hành: {(string.IsNullOrEmpty(get("org_name")) ? "UBND xã Gia Kiệm" : get("org_name"))}
- Nội dung: {title}
{(string.IsNullOrEmpty(get("period")) ? "" : $"- Thời kỳ: {get("period")}")}
{(string.IsNullOrEmpty(get("objectives")) ? "" : $"- Mục tiêu: {get("objectives")}")}
{(string.IsNullOrEmpty(get("tasks")) ? "" : $"- Nội dung nhiệm vụ: {get("tasks")}")}
{(string.IsNullOrEmpty(get("time_place")) ? "" : $"- Thời gian, địa điểm: {get("time_place")}")}
{(string.IsNullOrEmpty(get("purpose")) ? "" : $"- Mục đích: {get("purpose")}")}
{(string.IsNullOrEmpty(get("program")) ? "" : $"- Chương trình: {get("program")}")}
{(string.IsNullOrEmpty(get("budget")) ? "" : $"- Kinh phí: {get("budget")}")}
- Người ký: {get("signer_name")}, {get("signer_title")}

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan ban hành
3. Số/ký hiệu: Số:    /KH-UBND
4. Tiêu đề: KẾ HOẠCH + trích yếu
5. Thân kế hoạch:

   I. MỤC ĐÍCH, YÊU CẦU:
   1. Mục đích (rõ ràng, cụ thể)
   2. Yêu cầu (khả thi, sát thực tế)

   II. NỘI DUNG:
   - Chi tiết từng nhiệm vụ/hoạt động
   - Thời gian thực hiện cụ thể
   - Đơn vị/cá nhân chịu trách nhiệm

   III. TỔ CHỨC THỰC HIỆN:
   - Phân công nhiệm vụ cho từng bộ phận
   - Chế độ báo cáo, kiểm tra

   IV. KINH PHÍ THỰC HIỆN:
   - Tổng kinh phí, nguồn vốn
   - Phân bổ (nếu có)

6. Câu kết: ""Yêu cầu các ban ngành, đoàn thể nghiêm túc triển khai thực hiện./.""
7. Nơi nhận
8. Chức danh + tên người ký

LƯU Ý: Nội dung phải cụ thể, có mốc thời gian, có phân công rõ ràng. Tránh chung chung.";
    }

    private string BuildThongBaoPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        var templateName = _selectedTemplate?.Name ?? "";
        
        return $@"Soạn THÔNG BÁO hoàn chỉnh, đúng thể thức NĐ 30/2020:

LOẠI THÔNG BÁO: {templateName}

THÔNG TIN:
{(string.IsNullOrEmpty(get("meeting_name")) ? "" : $"- Nội dung: {get("meeting_name")}")}
{(string.IsNullOrEmpty(get("event_name")) ? "" : $"- Sự kiện: {get("event_name")}")}
{(string.IsNullOrEmpty(get("time")) ? "" : $"- Thời gian: {get("time")}")}
{(string.IsNullOrEmpty(get("location")) ? "" : $"- Địa điểm: {get("location")}")}
{(string.IsNullOrEmpty(get("participants")) ? "" : $"- Thành phần: {get("participants")}")}
{(string.IsNullOrEmpty(get("agenda")) ? "" : $"- Nội dung/Chương trình: {get("agenda")}")}
{(string.IsNullOrEmpty(get("content")) ? "" : $"- Nội dung: {get("content")}")}
{(string.IsNullOrEmpty(get("conclusion")) ? "" : $"- Kết luận: {get("conclusion")}")}
{(string.IsNullOrEmpty(get("tasks")) ? "" : $"- Nhiệm vụ: {get("tasks")}")}
- Người ký: {get("signer_name")}, {get("signer_title")}

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan: UBND XÃ GIA KIỆM
3. Số/ký hiệu: Số:    /TB-UBND
4. Tiêu đề: THÔNG BÁO + trích yếu
5. Thân thông báo:
   - Câu dẫn (lý do, căn cứ)
   - Nội dung chính (rõ ràng, cụ thể)
   - Thời gian, địa điểm (nếu có)
   - Thành phần, đối tượng
   - Yêu cầu (chuẩn bị, lưu ý)
6. Câu kết
7. Nơi nhận
8. Chức danh + tên người ký

LƯU Ý: Thông báo cần ngắn gọn, đầy đủ thông tin, dễ hiểu.";
    }

    private string BuildNghiQuyetPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        var level = get("level");
        var isHDND = _selectedTemplate?.Name?.Contains("HĐND") == true;
        
        return $@"Soạn NGHỊ QUYẾT hoàn chỉnh, đúng thể thức NĐ 30/2020:

LOẠI: Nghị quyết {(isHDND ? "HĐND" : "UBND")} cấp {level}

THÔNG TIN:
- Chủ đề: {get("subject")}
- Nội dung các điều: {get("articles")}
{(string.IsNullOrEmpty(get("effective_date")) ? "" : $"- Ngày hiệu lực: {get("effective_date")}")}
{(string.IsNullOrEmpty(get("implementing_unit")) ? "" : $"- Đơn vị thực hiện: {get("implementing_unit")}")}
- Chủ tịch: {get("chairman_name")}

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan: {(isHDND ? "HỘI ĐỒNG NHÂN DÂN" : "ỦY BAN NHÂN DÂN")} XÃ GIA KIỆM
3. Số/ký hiệu: Số:    /NQ-{(isHDND ? "HĐND" : "UBND")}
4. Tiêu đề: NGHỊ QUYẾT + ""Về việc [nội dung]""
5. Phần CĂN CỨ (bắt buộc, quan trọng):
   - Căn cứ Luật Tổ chức chính quyền địa phương 2015 (sửa đổi 2019)
   {(isHDND ? "- Căn cứ Luật Hoạt động giám sát của Quốc hội và HĐND 2015" : "")}
   - Căn cứ các luật/nghị định chuyên ngành liên quan
   - Căn cứ tờ trình của UBND (nếu là NQ HĐND)
   - Xét [tình hình thực tế/đề nghị...]
6. {(isHDND ? "HỘI ĐỒNG NHÂN DÂN XÃ GIA KIỆM QUYẾT NGHỊ:" : "QUYẾT NGHỊ:")}
   - Các điều khoản (Điều 1, 2, 3...)
   - Điều cuối: Giao trách nhiệm tổ chức thực hiện
7. Nơi nhận
8. CHỦ TỊCH + tên

LƯU Ý: Nghị quyết phải có tính pháp lý cao, viện dẫn căn cứ đầy đủ, nội dung chặt chẽ, từng điều khoản rõ ràng.";
    }

    // === 22 PROMPT BUILDERS BỔ SUNG — NĐ 30/2020 ===

    private string BuildChiThiPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn CHỈ THỊ hoàn chỉnh, đúng thể thức NĐ 30/2020:

THÔNG TIN:
- Cơ quan: {get("from_org")}
- Vấn đề: {get("subject")}
- Nội dung chỉ đạo: {get("content")}
- Người ký: {get("signer_name")}, Chức danh: {get("signer_title")}

CẤU TRÚC CHỈ THỊ:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan, Số/CT-UBND
3. Tiêu đề: CHỈ THỊ + ""Về việc...""
4. Phần mở đầu: nêu tình hình, lý do ban hành
5. Nội dung chỉ đạo (đánh số 1, 2, 3...)
6. Yêu cầu thực hiện
7. Nơi nhận + Chữ ký";
    }

    private string BuildQuyChEPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn QUY CHẾ hoàn chỉnh:
- Tên quy chế: {get("subject")}
- Nội dung: {get("content")}

CẤU TRÚC: Chương I (Quy định chung: phạm vi, đối tượng), Chương II (Nội dung cụ thể), Chương III (Tổ chức thực hiện).
Lưu ý: Quy chế thường ban hành kèm theo Quyết định.";
    }

    private string BuildQuyDinhPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn QUY ĐỊNH hoàn chỉnh:
- Tên quy định: {get("subject")}
- Nội dung: {get("content")}

CẤU TRÚC: Chương I (Quy định chung), Chương II (Quy định cụ thể), Chương III (Tổ chức thực hiện).";
    }

    private string BuildThongCaoPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn THÔNG CÁO hoàn chỉnh:
- Vấn đề: {get("subject")}
- Nội dung: {get("content")}

THÔNG CÁO thường ngắn gọn, thông tin chính thức đến công chúng. Văn phong rõ ràng, khách quan.";
    }

    private string BuildHuongDanPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn HƯỚNG DẪN hoàn chỉnh, đúng thể thức NĐ 30/2020:

- Vấn đề: {get("subject")}
- Nội dung: {get("content")}
- Cơ quan: {get("from_org")}

CẤU TRÚC: I. Mục đích, yêu cầu; II. Nội dung hướng dẫn (chi tiết từng bước); III. Tổ chức thực hiện.";
    }

    private string BuildChuongTrinhPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn CHƯƠNG TRÌNH hoàn chỉnh:
- Tên: {get("subject")}
- Nội dung: {get("content")}
- Thời gian: {get("timeline")}

CẤU TRÚC: I. Mục đích, yêu cầu; II. Nội dung (liệt kê hoạt động, thời gian, người chịu trách nhiệm); III. Tổ chức thực hiện.";
    }

    private string BuildDeAnDuAnPrompt(DocumentType type, Dictionary<string, string> fields, Func<string, string> get)
    {
        var typeName = type.GetDisplayName().ToUpper();
        return $@"Soạn {typeName} hoàn chỉnh:
- Tên: {get("subject")}
- Mục tiêu: {get("objectives")}
- Nội dung: {get("content")}
- Kinh phí: {get("budget")}

CẤU TRÚC: I. Sự cần thiết; II. Mục tiêu; III. Nội dung; IV. Giải pháp; V. Kinh phí; VI. Tổ chức thực hiện.";
    }

    private string BuildBienBanPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn BIÊN BẢN hoàn chỉnh:
- Cuộc họp/Làm việc: {get("subject")}
- Thời gian: {get("time")}
- Địa điểm: {get("location")}
- Thành phần: {get("attendees")}
- Nội dung: {get("content")}

CẤU TRÚC BIÊN BẢN: Thời gian, Địa điểm, Thành phần (Chủ trì, Tham dự, Thư ký), Nội dung, Kết luận.
Biên bản KHÔNG đánh số ký hiệu. Có CHỦ TRÌ và THƯ KÝ ký.";
    }

    private string BuildHopDongPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn HỢP ĐỒNG hoàn chỉnh:
- Tên: {get("subject")}
- Bên A: {get("party_a")}
- Bên B: {get("party_b")}
- Nội dung: {get("content")}

CẤU TRÚC: Căn cứ pháp lý, Thông tin các bên, Điều 1 (Nội dung), Điều 2 (Thời gian), Điều 3 (Giá trị), Điều 4 (Quyền/Nghĩa vụ), Điều 5 (Điều khoản chung).";
    }

    private string BuildCongDienPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn CÔNG ĐIỆN hoàn chỉnh, khẩn cấp:
- Vấn đề: {get("subject")}
- Nơi nhận: {get("to_org")}
- Nội dung: {get("content")}

CÔNG ĐIỆN phải ngắn gọn, khẩn trương. Mở đầu: ""[CƠ QUAN] ĐIỆN:"" + nơi nhận. Kết thúc yêu cầu khẩn trương thực hiện.";
    }

    private string BuildThoaThuanPrompt(DocumentType type, Dictionary<string, string> fields, Func<string, string> get)
    {
        var typeName = type.GetDisplayName().ToUpper();
        return $@"Soạn {typeName} hoàn chỉnh:
- Vấn đề: {get("subject")}
- Nội dung: {get("content")}
- Các bên: {get("parties")}

CẤU TRÚC: Thông tin các bên, Nội dung thỏa thuận, Cam kết thực hiện. {typeName} KHÔNG đánh số ký hiệu.";
    }

    private string BuildGiayUyQuyenPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn GIẤY ỦY QUYỀN hoàn chỉnh:
- Người ủy quyền: {get("grantor")}
- Người được ủy quyền: {get("grantee")}
- Nội dung: {get("content")}
- Thời hạn: {get("duration")}

Theo Điều 13 NĐ 30/2020: Người được ủy quyền KHÔNG được ủy quyền lại.";
    }

    private string BuildGiayMoiPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn GIẤY MỜI hoàn chỉnh:
- Nội dung mời: {get("subject")}
- Thời gian: {get("time")}
- Địa điểm: {get("location")}
- Người nhận: {get("to_org")}

Giấy mời ngắn gọn, lịch sự, đầy đủ thông tin (nội dung, thời gian, địa điểm, thành phần tham dự).";
    }

    private string BuildGiayGioiThieuPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn GIẤY GIỚI THIỆU hoàn chỉnh:
- Người được giới thiệu: {get("person")}
- Đến cơ quan: {get("to_org")}
- Nội dung: {get("content")}

Giấy giới thiệu phải ghi rõ: Họ tên, chức vụ, đơn vị; Nơi đến; Mục đích; Thời hạn giá trị.";
    }

    private string BuildGiayNghiPhepPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn GIẤY NGHỈ PHÉP hoàn chỉnh:
- Người xin nghỉ: {get("person")}
- Từ ngày: {get("from_date")}
- Đến ngày: {get("to_date")}
- Lý do: {get("reason")}

Giấy nghỉ phép ghi rõ: Họ tên, chức vụ, đơn vị; Thời gian nghỉ; Lý do; Địa chỉ liên lạc.";
    }

    private string BuildPhieuPrompt(DocumentType type, Dictionary<string, string> fields, Func<string, string> get)
    {
        var typeName = type.GetDisplayName().ToUpper();
        return $@"Soạn {typeName} hoàn chỉnh:
- Nơi nhận: {get("to_org")}
- Nội dung: {get("content")}
- Văn bản kèm theo: {get("documents")}

{typeName} là văn bản nghiệp vụ văn thư, ngắn gọn, rõ ràng.";
    }

    private string BuildThuCongPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn THƯ CÔNG hoàn chỉnh:
- Loại thư: {get("letter_type")} (chúc mừng / cảm ơn / chia buồn / thăm hỏi)
- Nơi nhận: {get("to_org")}
- Nội dung: {get("content")}

Thư công phải trang trọng, lịch sự, thể hiện tình cảm chân thành. Ký tên và đóng dấu.";
    }

    private string BuildGenericPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Soạn văn bản hành chính loại {_selectedTemplate?.Name ?? "Công văn"} hoàn chỉnh, đúng thể thức NĐ 30/2020:");
        sb.AppendLine();
        sb.AppendLine("THÔNG TIN:");
        foreach (var kvp in fields)
        {
            if (!string.IsNullOrWhiteSpace(kvp.Value))
                sb.AppendLine($"- {GetFieldLabel(kvp.Key)}: {kvp.Value}");
        }
        sb.AppendLine();
        sb.AppendLine("YÊU CẦU: Soạn văn bản đầy đủ thể thức (quốc hiệu, tiêu ngữ, số/ký hiệu, nội dung, nơi nhận, chữ ký). Văn phong hành chính chuẩn, trang trọng.");
        return sb.ToString();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var rawContent = GetRichTextContent();
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            MessageBox.Show("Chưa có nội dung để lưu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // === PARSE AI content → tách thành các trường cấu trúc ===
        var parsed = ParseAndCleanContent(rawContent, _selectedTemplate?.Type ?? DocumentType.CongVan);

        // === Lấy dữ liệu từ input fields người dùng đã nhập ===
        var subjectText = GetFieldValue("subject");
        var fromOrg = GetFieldValue("from_org", GetFieldValue("org_name"));
        var signerName = GetFieldValue("signer_name", GetFieldValue("chairman_name"));
        var signerTitle = GetFieldValue("signer_title");
        var recipientOrg = GetFieldValue("to_org", GetFieldValue("to_department", GetFieldValue("recipient")));

        // === Xác định Thẩm quyền ký (TM., KT.) dựa trên loại văn bản ===
        var docType = _selectedTemplate?.Type ?? DocumentType.CongVan;
        var signingAuthority = DetermineSigningAuthority(docType, signerTitle);

        // === Tạo document mới với ĐẦY ĐỦ các trường cho WordExportService ===
        GeneratedDocument = new Document
        {
            // Thông tin cơ bản
            Title = !string.IsNullOrWhiteSpace(subjectText) 
                ? subjectText 
                : $"{_selectedTemplate?.Name} - {DateTime.Now:dd/MM/yyyy}",
            Subject = subjectText,
            Type = docType,
            
            // Cơ quan ban hành (ưu tiên: input user > parsed từ AI)
            Issuer = !string.IsNullOrWhiteSpace(fromOrg) ? fromOrg : parsed.Issuer,
            
            // Nội dung (ĐÃ LỌC BỎ header/footer/căn cứ/nơi nhận - tránh trùng khi xuất Word)
            Content = parsed.CleanedContent,
            
            // Căn cứ pháp lý (tách riêng từ nội dung AI)
            BasedOn = parsed.BasedOn.ToArray(),
            
            // Nơi nhận (ưu tiên: parsed từ AI > mặc định)
            Recipients = parsed.Recipients.Count > 0 
                ? parsed.Recipients.ToArray() 
                : BuildDefaultRecipients(docType, recipientOrg),
            
            // Người ký (ưu tiên: input user > parsed từ AI)
            SignedBy = !string.IsNullOrWhiteSpace(signerName) ? signerName : parsed.SignerName,
            SigningTitle = !string.IsNullOrWhiteSpace(signerTitle) ? signerTitle : parsed.SignerTitle,
            SigningAuthority = signingAuthority,
            
            // Địa danh ban hành
            Location = "Gia Kiệm",
            
            // Ngày tháng & trạng thái
            IssueDate = DateTime.Now,
            CreatedDate = DateTime.Now,
            WorkflowStatus = DocumentStatus.Draft,
            Direction = Direction.Di,
            
            // Tags
            Tags = new[] { "AI Generated", (docType.ToString()) }
        };

        DialogResult = true;
        Close();
    }

    /// <summary>
    /// Lấy giá trị từ _fieldInputs, trả về fallback nếu rỗng
    /// </summary>
    private string GetFieldValue(string fieldName, string fallback = "")
    {
        if (_fieldInputs.TryGetValue(fieldName, out var textBox) && !string.IsNullOrWhiteSpace(textBox.Text))
            return textBox.Text.Trim();
        return fallback;
    }

    /// <summary>
    /// Xác định thẩm quyền ký dựa trên loại văn bản và chức danh người ký
    /// Theo Điều 13, NĐ 30/2020/NĐ-CP:
    /// - Ký trực tiếp: Người đứng đầu ký các VB thuộc thẩm quyền
    /// - KT. (Ký thay): Cấp phó ký thay cấp trưởng
    /// - TM. (Thay mặt): Người đứng đầu thay mặt tập thể ký
    /// - TL. (Thừa lệnh): Người được giao ký thừa lệnh
    /// - TUQ. (Thừa ủy quyền): Người được ủy quyền ký
    /// - Q. (Quyền): Người giữ quyền chức vụ
    /// </summary>
    private string DetermineSigningAuthority(DocumentType docType, string signerTitle)
    {
        var titleLower = (signerTitle ?? "").ToLower().Trim();
        
        // Q. (Quyền) — Người giữ quyền chức vụ
        if (titleLower.StartsWith("q.") || titleLower.Contains("quyền chủ tịch") || titleLower.Contains("quyền giám đốc"))
            return "Q.";
        
        // TUQ. (Thừa ủy quyền) — Điều 13 khoản 3
        if (titleLower.Contains("thừa ủy quyền") || titleLower.Contains("tuq"))
            return "TUQ.";
        
        // TL. (Thừa lệnh) — Điều 13 khoản 4
        if (titleLower.Contains("thừa lệnh") || titleLower.Contains("chánh văn phòng") 
            || titleLower.Contains("trưởng phòng"))
            return "TL.";
        
        // KT. (Ký thay) — Cấp phó ký thay cấp trưởng (Điều 13 khoản 1)
        if (titleLower.Contains("phó"))
            return "KT.";
        
        // TM. (Thay mặt) — Chế độ tập thể: QĐ, NQ, CT (Điều 13 khoản 2)
        if (docType is DocumentType.QuyetDinh or DocumentType.NghiQuyet or DocumentType.ChiThi
            or DocumentType.QuyChE or DocumentType.QuyDinh)
            return "TM.";
        
        return ""; // Công văn, báo cáo, tờ trình... ký trực tiếp
    }

    /// <summary>
    /// Tạo danh sách Nơi nhận mặc định theo loại văn bản
    /// </summary>
    private string[] BuildDefaultRecipients(DocumentType docType, string recipientOrg)
    {
        var recipients = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(recipientOrg))
            recipients.Add($"- Như trên;");
        
        if (docType is DocumentType.QuyetDinh or DocumentType.NghiQuyet or DocumentType.ChiThi)
        {
            recipients.Add("- Đảng ủy, HĐND, UBMTTQ xã (để báo cáo);");
        }
        
        recipients.Add("- Lưu: VT.");
        
        return recipients.ToArray();
    }

    /// <summary>
    /// Parse nội dung AI → tách thành các phần cấu trúc cho Document model
    /// Mục đích: WordExportService tạo header/footer riêng → Content chỉ giữ phần THÂN VĂN BẢN
    /// Tránh bị TRÙNG LẶP khi xuất Word
    /// </summary>
    private ParsedDocumentContent ParseAndCleanContent(string rawText, DocumentType docType)
    {
        var result = new ParsedDocumentContent();
        var lines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        
        var bodyLines = new List<string>();
        bool inHeader = true;       // Đang trong phần header (Quốc hiệu, tên CQ, số, loại VB, trích yếu)
        bool inCanCu = false;       // Đang trong phần căn cứ
        bool inNoiNhan = false;     // Đang trong phần nơi nhận
        bool inSignature = false;   // Đang trong phần chữ ký
        bool headerPassed = false;  // Đã qua hết phần header
        
        // Danh sách pattern cho header (sẽ bỏ qua)
        var headerPatterns = new[]
        {
            "CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM",
            "Độc lập - Tự do - Hạnh phúc",
            "───", "---", "___"
        };
        
        // Pattern cho tên loại văn bản (tiêu đề chính)
        var docTypeNames = new[]
        {
            "QUYẾT ĐỊNH", "NGHỊ QUYẾT", "BÁO CÁO", "KẾ HOẠCH", 
            "TỜ TRÌNH", "THÔNG BÁO", "CHỈ THỊ", "CÔNG VĂN",
            "HƯỚNG DẪN", "QUY ĐỊNH", "CHƯƠNG TRÌNH", "PHƯƠNG ÁN",
            "ĐỀ ÁN", "BIÊN BẢN"
        };
        
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.Trim();
            
            // === BỎ QUA HEADER ===
            if (inHeader && !headerPassed)
            {
                // Quốc hiệu, tiêu ngữ, gạch ngang
                if (headerPatterns.Any(p => trimmed.Contains(p)))
                    continue;
                
                // Tên cơ quan (chữ IN HOA, ngắn, không phải nội dung)
                if (trimmed == trimmed.ToUpper() && trimmed.Length > 3 && trimmed.Length <= 60 
                    && !trimmed.StartsWith("Điều") && !docTypeNames.Contains(trimmed)
                    && (trimmed.Contains("ỦY BAN") || trimmed.Contains("UBND") || trimmed.Contains("HỘI ĐỒNG") 
                        || trimmed.Contains("ĐẢNG ỦY") || trimmed.Contains("BAN") || trimmed.Contains("PHÒNG")
                        || trimmed.Contains("TRƯỜNG") || trimmed.Contains("TRẠM") || trimmed.Contains("CÔNG AN")
                        || trimmed.Contains("HUYỆN") || trimmed.Contains("XÃ") || trimmed.Contains("TỈNH")))
                {
                    // Lấy tên cơ quan cuối cùng làm issuer
                    result.Issuer = trimmed;
                    continue;
                }
                
                // Số văn bản: "Số: 123/QĐ-UBND"
                if (trimmed.StartsWith("Số:") || trimmed.StartsWith("Số "))
                    continue;
                
                // Ngày tháng: "Gia Kiệm, ngày 14 tháng 02 năm 2026"
                if (trimmed.Contains("ngày") && trimmed.Contains("tháng") && trimmed.Contains("năm"))
                    continue;
                
                // Tên loại văn bản (QUYẾT ĐỊNH, BÁO CÁO...)
                if (docTypeNames.Contains(trimmed) || trimmed == "QUYẾT ĐỊNH:" || trimmed == "QUYẾT NGHỊ:")
                {
                    continue;
                }
                
                // Trích yếu: "Về việc..." / "V/v ..."
                if (trimmed.StartsWith("Về việc") || trimmed.StartsWith("V/v"))
                    continue;
                
                // "Kính gửi:"
                if (trimmed.StartsWith("Kính gửi"))
                    continue;
                
                // Dòng trống trong header → bỏ qua
                if (string.IsNullOrWhiteSpace(trimmed))
                    continue;
                
                // Gặp dòng nội dung thật sự → hết header
                inHeader = false;
                headerPassed = true;
            }
            
            // === PHÁT HIỆN CĂN CỨ ===
            if (headerPassed && !inCanCu && !inNoiNhan && !inSignature)
            {
                if (trimmed.StartsWith("Căn cứ ") || trimmed.StartsWith("- Căn cứ ") || trimmed.StartsWith("Theo "))
                {
                    inCanCu = true;
                }
            }
            
            if (inCanCu)
            {
                if (trimmed.StartsWith("Căn cứ ") || trimmed.StartsWith("- Căn cứ ") || trimmed.StartsWith("Theo "))
                {
                    // Loại bỏ dấu "- " đầu dòng, dấu ";" cuối
                    var cancu = trimmed.TrimStart('-', ' ');
                    if (cancu.EndsWith(";")) cancu = cancu[..^1].Trim();
                    result.BasedOn.Add(cancu);
                    continue;
                }
                else if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    // Hết phần căn cứ → chuyển sang body
                    inCanCu = false;
                    // Kiểm tra nếu dòng này là nhãn "QUYẾT ĐỊNH:" thì bỏ qua luôn
                    if (trimmed == "QUYẾT ĐỊNH:" || trimmed == "QUYẾT NGHỊ:")
                        continue;
                }
                else
                {
                    continue; // Dòng trống trong phần căn cứ
                }
            }
            
            // === PHÁT HIỆN NƠI NHẬN ===
            if (trimmed.StartsWith("Nơi nhận:") || trimmed == "Nơi nhận:")
            {
                inNoiNhan = true;
                inSignature = true; // Nơi nhận thường đi kèm phần ký
                continue;
            }
            
            if (inNoiNhan)
            {
                if (trimmed.StartsWith("- ") || trimmed.StartsWith("+ "))
                {
                    result.Recipients.Add(trimmed);
                    continue;
                }
                else if (string.IsNullOrWhiteSpace(trimmed))
                {
                    continue;
                }
                else
                {
                    inNoiNhan = false;
                    // Dòng tiếp theo có thể là phần ký
                }
            }
            
            // === PHÁT HIỆN PHẦN CHỮ KÝ (cuối văn bản) ===
            if (inSignature || IsSignatureArea(trimmed))
            {
                inSignature = true;
                
                // Trích xuất thông tin ký
                if (trimmed.Contains("CHỦ TỊCH") || trimmed.Contains("PHÓ CHỦ TỊCH")
                    || trimmed.Contains("TRƯỞNG") || trimmed.Contains("GIÁM ĐỐC")
                    || trimmed.Contains("CHÁNH"))
                {
                    if (trimmed == trimmed.ToUpper())
                        result.SignerTitle = trimmed;
                }
                
                // Tên người ký (dòng cuối, có chữ hoa đầu, không phải chức danh)
                if (!string.IsNullOrWhiteSpace(trimmed) 
                    && trimmed != trimmed.ToUpper()
                    && !trimmed.StartsWith("(") && !trimmed.StartsWith("TM.")
                    && !trimmed.StartsWith("KT.") && !trimmed.StartsWith("Q.")
                    && !trimmed.Contains("ngày") && !trimmed.Contains("tháng")
                    && !trimmed.StartsWith("- ") && !trimmed.StartsWith("+ ")
                    && trimmed.Split(' ').Length >= 2 && trimmed.Split(' ').Length <= 5
                    && char.IsUpper(trimmed[0]))
                {
                    result.SignerName = trimmed;
                }
                
                continue; // Bỏ qua phần ký khỏi content
            }
            
            // === THU THẬP PHẦN THÂN VĂN BẢN ===
            if (headerPassed)
            {
                bodyLines.Add(line);
            }
        }
        
        // Clean up: bỏ dòng trống thừa đầu/cuối
        while (bodyLines.Count > 0 && string.IsNullOrWhiteSpace(bodyLines[0]))
            bodyLines.RemoveAt(0);
        while (bodyLines.Count > 0 && string.IsNullOrWhiteSpace(bodyLines[^1]))
            bodyLines.RemoveAt(bodyLines.Count - 1);
        
        result.CleanedContent = string.Join("\n", bodyLines);
        
        return result;
    }
    
    /// <summary>
    /// Kiểm tra dòng có thuộc vùng chữ ký không
    /// </summary>
    private bool IsSignatureArea(string trimmedLine)
    {
        if (string.IsNullOrWhiteSpace(trimmedLine)) return false;
        
        return trimmedLine.StartsWith("TM. ") 
            || trimmedLine.StartsWith("KT. ")
            || trimmedLine.StartsWith("Q. ")
            || trimmedLine == "(Ký, ghi rõ họ tên và đóng dấu)"
            || trimmedLine == "(Ký, ghi rõ họ tên)"
            || trimmedLine == "[Họ tên người ký]";
    }
    
    /// <summary>
    /// Kết quả parse nội dung AI
    /// </summary>
    private class ParsedDocumentContent
    {
        public string Issuer { get; set; } = "";
        public string CleanedContent { get; set; } = "";
        public List<string> BasedOn { get; set; } = new();
        public List<string> Recipients { get; set; } = new();
        public string SignerName { get; set; } = "";
        public string SignerTitle { get; set; } = "";
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
        flowDoc.PagePadding = new Thickness(30, 20, 30, 20);
        flowDoc.FontFamily = new FontFamily("Times New Roman");
        flowDoc.FontSize = 14;
        flowDoc.LineHeight = 1.5;
        
        // Clean up markdown artifacts from AI
        text = text.Replace("**", "").Replace("__", "");
        text = text.Replace("```", "").Replace("`", "");
        // Remove leading # markdown headers
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^#{1,6}\s*", "", System.Text.RegularExpressions.RegexOptions.Multiline);
        
        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            var para = new Paragraph(new Run(line));
            para.Margin = new Thickness(0, 2, 0, 2);
            
            // ═══ QUỐC HIỆU ═══
            if (trimmed.StartsWith("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM"))
            {
                para.FontWeight = FontWeights.Bold;
                para.FontSize = 14;
                para.TextAlignment = TextAlignment.Center;
                para.Margin = new Thickness(0, 0, 0, 0);
            }
            // ═══ TIÊU NGỮ ═══
            else if (trimmed.StartsWith("Độc lập") && trimmed.Contains("Tự do") && trimmed.Contains("Hạnh phúc"))
            {
                para.FontWeight = FontWeights.Bold;
                para.FontStyle = FontStyles.Italic;
                para.FontSize = 14;
                para.TextAlignment = TextAlignment.Center;
                para.Margin = new Thickness(0, 0, 0, 4);
            }
            // ═══ GẠCH NGANG DƯỚI TIÊU NGỮ ═══
            else if (trimmed.StartsWith("---") || trimmed.StartsWith("───") || trimmed.StartsWith("___"))
            {
                para = new Paragraph(new Run("─────────────────────────────────"));
                para.TextAlignment = TextAlignment.Center;
                para.FontSize = 10;
                para.Margin = new Thickness(0, 0, 0, 8);
            }
            // ═══ TÊN CƠ QUAN (chữ in hoa toàn bộ) ═══
            else if (trimmed == trimmed.ToUpper() && trimmed.Length > 5 && !trimmed.StartsWith("I.") && !trimmed.StartsWith("II.") && !trimmed.StartsWith("V/v") && !trimmed.StartsWith("Số:") && !trimmed.Contains("QUYẾT ĐỊNH") && !trimmed.Contains("NGHỊ QUYẾT") && !trimmed.Contains("BÁO CÁO") && !trimmed.Contains("KẾ HOẠCH") && !trimmed.Contains("TỜ TRÌNH") && !trimmed.Contains("THÔNG BÁO"))
            {
                para.FontWeight = FontWeights.Bold;
                para.FontSize = 13;
                para.TextAlignment = TextAlignment.Center;
            }
            // ═══ TIÊU ĐỀ VĂN BẢN (QUYẾT ĐỊNH, BÁO CÁO...) ═══
            else if (trimmed is "QUYẾT ĐỊNH" or "NGHỊ QUYẾT" or "BÁO CÁO" or "KẾ HOẠCH" or "TỜ TRÌNH" or "THÔNG BÁO" or "CHỈ THỊ")
            {
                para.FontWeight = FontWeights.Bold;
                para.FontSize = 16;
                para.TextAlignment = TextAlignment.Center;
                para.Margin = new Thickness(0, 12, 0, 4);
            }
            // ═══ TRÍCH YẾU (Về việc...) ═══
            else if (trimmed.StartsWith("Về việc") || trimmed.StartsWith("V/v:") || trimmed.StartsWith("V/v "))
            {
                para.FontWeight = FontWeights.Bold;
                para.FontStyle = FontStyles.Italic;
                para.TextAlignment = TextAlignment.Center;
                para.Margin = new Thickness(0, 0, 0, 8);
            }
            // ═══ SỐ/KÝ HIỆU ═══
            else if (trimmed.StartsWith("Số:") || trimmed.StartsWith("Số "))
            {
                para.FontSize = 13;
                para.Margin = new Thickness(0, 4, 0, 4);
            }
            // ═══ ĐỊA DANH NGÀY THÁNG ═══
            else if (trimmed.Contains("ngày") && trimmed.Contains("tháng") && trimmed.Contains("năm"))
            {
                para.FontStyle = FontStyles.Italic;
                para.TextAlignment = TextAlignment.Right;
                para.Margin = new Thickness(0, 4, 0, 8);
            }
            // ═══ KÍNH GỬI ═══
            else if (trimmed.StartsWith("Kính gửi:") || trimmed.StartsWith("Kính gửi "))
            {
                para.FontWeight = FontWeights.Bold;
                para.Margin = new Thickness(0, 8, 0, 8);
            }
            // ═══ CĂN CỨ ═══
            else if (trimmed.StartsWith("Căn cứ ") || trimmed.StartsWith("- Căn cứ "))
            {
                para.FontStyle = FontStyles.Italic;
                para.FontSize = 13;
            }
            // ═══ ĐIỀU KHOẢN ═══
            else if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^Điều\s+\d+"))
            {
                para.FontWeight = FontWeights.Bold;
                para.FontSize = 14;
                para.Margin = new Thickness(0, 8, 0, 4);
            }
            // ═══ CÁC PHẦN (I, II, III...) ═══
            else if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^(I{1,3}V?|VI{0,3}|Phần\s+[IVX]+)[\.\s]"))
            {
                para.FontWeight = FontWeights.Bold;
                para.FontSize = 14;
                para.Margin = new Thickness(0, 10, 0, 4);
            }
            // ═══ MỤC CON (1., 2., 3.,...) ═══
            else if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^\d+[\.\)]\s"))
            {
                para.FontWeight = FontWeights.SemiBold;
                para.Margin = new Thickness(0, 4, 0, 2);
            }
            // ═══ NƠI NHẬN ═══
            else if (trimmed.StartsWith("Nơi nhận:") || trimmed == "Nơi nhận:")
            {
                para.FontWeight = FontWeights.Bold;
                para.FontStyle = FontStyles.Italic;
                para.FontSize = 12;
                para.Margin = new Thickness(0, 16, 0, 2);
            }
            // ═══ CHỨC DANH KÝ (CHỦ TỊCH, PHÓ CHỦ TỊCH...) ═══
            else if ((trimmed.Contains("CHỦ TỊCH") || trimmed.Contains("TRƯỞNG BAN") || trimmed.Contains("CHÁNH VĂN PHÒNG")) && trimmed == trimmed.ToUpper())
            {
                para.FontWeight = FontWeights.Bold;
                para.TextAlignment = TextAlignment.Right;
                para.Margin = new Thickness(0, 12, 60, 2);
            }
            // ═══ QUYẾT ĐỊNH: / QUYẾT NGHỊ: ═══
            else if (trimmed is "QUYẾT ĐỊNH:" or "QUYẾT NGHỊ:" or "HỘI ĐỒNG NHÂN DÂN XÃ GIA KIỆM QUYẾT NGHỊ:")
            {
                para.FontWeight = FontWeights.Bold;
                para.TextAlignment = TextAlignment.Center;
                para.Margin = new Thickness(0, 8, 0, 8);
            }
            // ═══ GẠCH ĐẦU DÒNG ═══
            else if (trimmed.StartsWith("- ") || trimmed.StartsWith("+ "))
            {
                para.Margin = new Thickness(20, 1, 0, 1);
                para.TextAlignment = TextAlignment.Left;
            }
            // ═══ NỘI DUNG THƯỜNG ═══
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
