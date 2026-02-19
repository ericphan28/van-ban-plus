using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service tạo dữ liệu demo cho test.
/// Mỗi văn bản là một "bộ" nhất quán: Title ↔ Subject ↔ Content ↔ Type ↔ Direction ↔ Issuer.
/// Phủ 25+ loại VB theo Điều 7, NĐ 30/2020, nhiều phòng ban và cơ quan thực tế.
/// </summary>
public class SeedDataService
{
    private readonly DocumentService _documentService;
    private readonly Random _rng = new();

    public SeedDataService(DocumentService documentService)
    {
        _documentService = documentService;
    }

    // ═══════════════════════════════════════
    // Mỗi record chứa đầy đủ thông tin nhất quán cho 1 VB
    // ═══════════════════════════════════════
    private record DemoTemplate(
        string Title,
        string Subject,
        string Content,
        DocumentType Type,
        Direction Direction,
        string Issuer,
        string Category,
        string NumberPrefix, // VD: QĐ, CV, BC, KH, TB...  — Phụ lục VI NĐ 30/2020
        string[] Tags,
        string[]? Recipients,       // Nơi nhận (VB đi + nội bộ)
        UrgencyLevel Urgency,
        SecurityLevel Security
    );

    /// <summary>
    /// 50 bộ template nhất quán — phủ 25+ loại VB, nhiều phòng ban, cơ quan
    /// </summary>
    private DemoTemplate[] GetTemplates() => new DemoTemplate[]
    {
        // ╔═══════════════════════════════════════════════════════╗
        // ║  PHẦN 1: VĂN BẢN ĐI  (18 mẫu)                      ║
        // ╚═══════════════════════════════════════════════════════╝

        // ── 1. Quyết định bổ nhiệm ──
        new(
            Title: "Quyết định bổ nhiệm Phó Giám đốc",
            Subject: "Bổ nhiệm ông Nguyễn Văn A giữ chức vụ Phó Giám đốc phụ trách kỹ thuật",
            Content: "Căn cứ Luật Tổ chức Chính quyền địa phương năm 2015;\n" +
                     "Căn cứ Nghị định số 138/2020/NĐ-CP về tuyển dụng, sử dụng và quản lý công chức;\n" +
                     "Xét đề nghị của Hội đồng tuyển dụng và bổ nhiệm cán bộ,\n\n" +
                     "QUYẾT ĐỊNH:\n\n" +
                     "Điều 1. Bổ nhiệm ông Nguyễn Văn A giữ chức vụ Phó Giám đốc phụ trách mảng Kỹ thuật kể từ ngày 01/02/2026.\n\n" +
                     "Điều 2. Ông Nguyễn Văn A có trách nhiệm:\n" +
                     "- Phụ trách chung các hoạt động kỹ thuật của đơn vị\n" +
                     "- Tham mưu cho Giám đốc về các vấn đề kỹ thuật\n" +
                     "- Điều hành các phòng ban thuộc khối kỹ thuật\n\n" +
                     "Điều 3. Quyết định này có hiệu lực từ ngày ký.",
            Type: DocumentType.QuyetDinh, Direction: Direction.Di,
            Issuer: "Giám đốc Sở", Category: "Nhân sự", NumberPrefix: "QĐ",
            Tags: new[] { "Nhân sự", "Bổ nhiệm", "Năm 2026" },
            Recipients: new[] { "- Như Điều 1", "- Phòng Tổ chức - Hành chính", "- Phòng Tài chính - Kế toán", "- Lưu: VT, TCCB" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 2. Quyết định khen thưởng ──
        new(
            Title: "Quyết định khen thưởng cá nhân xuất sắc năm 2025",
            Subject: "Khen thưởng các cá nhân có thành tích xuất sắc trong công tác năm 2025",
            Content: "Căn cứ Luật Thi đua, Khen thưởng năm 2022;\n" +
                     "Căn cứ kết quả bình xét thi đua năm 2025;\n" +
                     "Xét đề nghị của Hội đồng Thi đua - Khen thưởng,\n\n" +
                     "QUYẾT ĐỊNH:\n\n" +
                     "Điều 1. Tặng Giấy khen cho 15 cá nhân có thành tích xuất sắc (danh sách kèm theo).\n\n" +
                     "Điều 2. Mức thưởng: 1.500.000 đồng/cá nhân từ quỹ khen thưởng.\n\n" +
                     "Điều 3. Phòng TCHC, Phòng TCKT và các cá nhân có tên tại Điều 1 chịu trách nhiệm thi hành.",
            Type: DocumentType.QuyetDinh, Direction: Direction.Di,
            Issuer: "Giám đốc Sở", Category: "Tổ chức", NumberPrefix: "QĐ",
            Tags: new[] { "Khen thưởng", "Thi đua", "Năm 2025" },
            Recipients: new[] { "- Như Điều 1", "- Các phòng, ban", "- Phòng TCHC", "- Phòng TCKT", "- Lưu: VT, TCCB" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 3. Quyết định ban hành quy chế ──
        new(
            Title: "Quyết định ban hành Quy chế chi tiêu nội bộ",
            Subject: "Ban hành Quy chế chi tiêu nội bộ và quản lý, sử dụng tài sản công của Sở",
            Content: "Căn cứ Nghị định 130/2005/NĐ-CP về chế độ tự chủ;\n" +
                     "Căn cứ Thông tư 71/2014/TT-BTC hướng dẫn thực hiện;\n" +
                     "Theo đề nghị của Phòng Tài chính - Kế toán,\n\n" +
                     "QUYẾT ĐỊNH:\n\n" +
                     "Điều 1. Ban hành kèm theo Quyết định này Quy chế chi tiêu nội bộ " +
                     "và quản lý, sử dụng tài sản công của Sở (chi tiết tại Phụ lục đính kèm).\n\n" +
                     "Điều 2. Quy chế có hiệu lực từ ngày 01/01/2026 và thay thế Quy chế " +
                     "chi tiêu nội bộ ban hành ngày 15/01/2024.\n\n" +
                     "Điều 3. Chánh Văn phòng, Trưởng phòng TCKT, Trưởng các phòng ban " +
                     "chịu trách nhiệm thi hành.",
            Type: DocumentType.QuyetDinh, Direction: Direction.Di,
            Issuer: "Giám đốc Sở", Category: "Tài chính", NumberPrefix: "QĐ",
            Tags: new[] { "Quy chế", "Chi tiêu", "Tài chính" },
            Recipients: new[] { "- Như Điều 3", "- Toàn thể CBCCVC", "- Lưu: VT, TCKT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 4. Kế hoạch đào tạo ──
        new(
            Title: "Kế hoạch đào tạo nâng cao năng lực cán bộ năm 2026",
            Subject: "Đào tạo, bồi dưỡng nâng cao trình độ chuyên môn cho CBCCVC năm 2026",
            Content: "Nhằm nâng cao năng lực chuyên môn cho đội ngũ cán bộ, " +
                     "Ban Giám đốc phê duyệt Kế hoạch đào tạo năm 2026:\n\n" +
                     "I. MỤC TIÊU:\n" +
                     "- Đào tạo 100% CBCCVC về kỹ năng tin học văn phòng\n" +
                     "- Bồi dưỡng chuyên môn sâu cho 50% cán bộ chuyên môn\n" +
                     "- Đào tạo ngoại ngữ cho 30% cán bộ trẻ\n\n" +
                     "II. HÌNH THỨC:\n" +
                     "- Đào tạo tập trung tại cơ quan\n" +
                     "- Cử đi học các lớp ngắn hạn, dài hạn\n" +
                     "- Học trực tuyến qua nền tảng e-learning\n\n" +
                     "III. KINH PHÍ: 500 triệu đồng từ ngân sách đào tạo đơn vị.\n\n" +
                     "IV. TỔ CHỨC THỰC HIỆN:\n" +
                     "Phòng TCHC chủ trì, phối hợp các phòng ban lập danh sách và triển khai.",
            Type: DocumentType.KeHoach, Direction: Direction.Di,
            Issuer: "Ban Giám đốc", Category: "Đào tạo", NumberPrefix: "KH",
            Tags: new[] { "Đào tạo", "Nhân sự", "Năm 2026" },
            Recipients: new[] { "- Các phòng, ban (để thực hiện)", "- Phòng TCHC (chủ trì)", "- Phòng KHTC (bố trí kinh phí)", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 5. Kế hoạch kiểm tra CCHC ──
        new(
            Title: "Kế hoạch kiểm tra công tác cải cách hành chính năm 2026",
            Subject: "Kiểm tra tình hình thực hiện cải cách hành chính tại các phòng ban năm 2026",
            Content: "I. MỤC ĐÍCH, YÊU CẦU:\n" +
                     "- Đánh giá thực trạng CCHC tại các phòng ban\n" +
                     "- Phát hiện, chấn chỉnh những tồn tại, hạn chế\n" +
                     "- Nâng cao trách nhiệm của thủ trưởng đơn vị\n\n" +
                     "II. NỘI DUNG KIỂM TRA:\n" +
                     "1. Cải cách thể chế\n" +
                     "2. Cải cách thủ tục hành chính\n" +
                     "3. Cải cách tổ chức bộ máy\n" +
                     "4. Xây dựng và nâng cao chất lượng CBCCVC\n" +
                     "5. Ứng dụng CNTT trong quản lý\n\n" +
                     "III. THỜI GIAN: Quý II và Quý III/2026\n\n" +
                     "IV. ĐƠN VỊ CHỦ TRÌ: Thanh tra Sở phối hợp Văn phòng Sở.",
            Type: DocumentType.KeHoach, Direction: Direction.Di,
            Issuer: "Giám đốc Sở", Category: "Thanh tra", NumberPrefix: "KH",
            Tags: new[] { "CCHC", "Kiểm tra", "Năm 2026" },
            Recipients: new[] { "- Thanh tra Sở (chủ trì)", "- Các phòng, ban (để thực hiện)", "- Lưu: VT, TTr" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 6. Báo cáo quý ──
        new(
            Title: "Báo cáo kết quả hoạt động quý I/2026",
            Subject: "Báo cáo tình hình thực hiện nhiệm vụ công tác quý I năm 2026",
            Content: "Kính gửi: UBND Tỉnh\n\n" +
                     "Thực hiện chế độ báo cáo định kỳ, Sở báo cáo kết quả Quý I/2026:\n\n" +
                     "I. KẾT QUẢ THỰC HIỆN:\n" +
                     "1. Chỉ đạo điều hành: Hoàn thành 95% nhiệm vụ được giao\n" +
                     "2. Cải cách hành chính: Giải quyết 1.250/1.300 hồ sơ (96,2%)\n" +
                     "3. Thanh tra, kiểm tra: 12/15 cuộc theo kế hoạch\n\n" +
                     "II. TỒN TẠI, HẠN CHẾ:\n" +
                     "- Một số thủ tục hành chính còn chậm trễ\n" +
                     "- Ứng dụng CNTT chưa đồng bộ\n\n" +
                     "III. PHƯƠNG HƯỚNG QUÝ II/2026:\n" +
                     "- Hoàn thành 100% nhiệm vụ trọng tâm\n" +
                     "- Triển khai quản lý văn bản điện tử\n" +
                     "- Tổ chức đào tạo CBCCVC đợt 1",
            Type: DocumentType.BaoCao, Direction: Direction.Di,
            Issuer: "Giám đốc Sở", Category: "Hành chính", NumberPrefix: "BC",
            Tags: new[] { "Báo cáo", "Q1", "Năm 2026" },
            Recipients: new[] { "- UBND Tỉnh", "- Sở Nội vụ", "- Lưu: VT, THKH" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 7. Báo cáo quyết toán ──
        new(
            Title: "Báo cáo quyết toán ngân sách năm 2025",
            Subject: "Quyết toán thu, chi ngân sách nhà nước năm 2025 của Sở",
            Content: "Kính gửi: Sở Tài chính\n\n" +
                     "Thực hiện Thông tư 137/2017/TT-BTC, Sở báo cáo quyết toán NSNN năm 2025:\n\n" +
                     "I. TỔNG THU NGÂN SÁCH: 42.500.000.000 đồng\n" +
                     "- Thu phí, lệ phí: 8.200.000.000đ\n" +
                     "- Thu hoạt động SN: 34.300.000.000đ\n\n" +
                     "II. TỔNG CHI NGÂN SÁCH: 41.800.000.000 đồng\n" +
                     "- Chi thường xuyên: 35.600.000.000đ\n" +
                     "- Chi đầu tư phát triển: 4.200.000.000đ\n" +
                     "- Chi khác: 2.000.000.000đ\n\n" +
                     "III. KẾT DƯ: 700.000.000 đồng\n\n" +
                     "Sở kính đề nghị Sở Tài chính thẩm định, phê duyệt.",
            Type: DocumentType.BaoCao, Direction: Direction.Di,
            Issuer: "Phòng Tài chính - Kế toán", Category: "Tài chính", NumberPrefix: "BC",
            Tags: new[] { "Quyết toán", "Ngân sách", "Năm 2025" },
            Recipients: new[] { "- Sở Tài chính", "- Kho bạc Nhà nước tỉnh", "- Lưu: VT, TCKT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 8. Thông báo hội nghị ──
        new(
            Title: "Thông báo tổ chức hội nghị tổng kết năm 2025",
            Subject: "Tổ chức Hội nghị tổng kết công tác năm 2025 và triển khai nhiệm vụ năm 2026",
            Content: "Kính gửi: Toàn thể cán bộ, công chức, viên chức\n\n" +
                     "Đơn vị trân trọng thông báo tổ chức Hội nghị tổng kết:\n\n" +
                     "1. Thời gian: 8h00, ngày 15/01/2026\n" +
                     "2. Địa điểm: Hội trường tầng 3, trụ sở cơ quan\n" +
                     "3. Thành phần: Toàn thể CBCCVC\n" +
                     "4. Nội dung:\n" +
                     "   - Báo cáo tổng kết năm 2025\n" +
                     "   - Phương hướng nhiệm vụ năm 2026\n" +
                     "   - Biểu dương khen thưởng tập thể, cá nhân xuất sắc\n\n" +
                     "Đề nghị các đồng chí sắp xếp tham dự đầy đủ, đúng giờ.",
            Type: DocumentType.ThongBao, Direction: Direction.Di,
            Issuer: "Văn phòng Sở", Category: "Hành chính", NumberPrefix: "TB",
            Tags: new[] { "Hội nghị", "Tổng kết", "Năm 2025" },
            Recipients: new[] { "- Toàn thể CBCCVC", "- Các đơn vị trực thuộc", "- Công đoàn cơ sở", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 9. Công văn đề nghị biên chế ──
        new(
            Title: "Công văn đề nghị bổ sung biên chế",
            Subject: "Đề nghị bổ sung biên chế năm 2026 cho các phòng chuyên môn",
            Content: "Kính gửi: Sở Nội vụ\n\n" +
                     "Căn cứ khối lượng công việc tăng và nhu cầu thực tế, " +
                     "Sở đề nghị Sở Nội vụ xem xét bổ sung biên chế:\n\n" +
                     "1. Phòng Quản lý Quy hoạch: 02 biên chế (chuyên viên)\n" +
                     "2. Phòng CNTT: 03 biên chế (kỹ sư CNTT)\n" +
                     "3. Văn phòng Sở: 01 biên chế (văn thư - lưu trữ)\n\n" +
                     "Tổng cộng: 06 biên chế\n\n" +
                     "Lý do: Khối lượng hồ sơ tăng 35% so với năm 2025.\n\n" +
                     "Kính đề nghị Sở Nội vụ xem xét, tổng hợp trình UBND Tỉnh.",
            Type: DocumentType.CongVan, Direction: Direction.Di,
            Issuer: "Giám đốc Sở", Category: "Nhân sự", NumberPrefix: "CV",
            Tags: new[] { "Nhân sự", "Biên chế", "Năm 2026" },
            Recipients: new[] { "- Sở Nội vụ", "- Lưu: VT, TCCB" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 10. Tờ trình mua sắm CNTT ──
        new(
            Title: "Tờ trình đề xuất mua sắm trang thiết bị CNTT",
            Subject: "Đề xuất mua sắm máy tính, máy in phục vụ công tác chuyên môn năm 2026",
            Content: "Kính gửi: Ban Giám đốc\n\n" +
                     "Phòng CNTT kính trình phương án mua sắm trang thiết bị:\n\n" +
                     "I. SỰ CẦN THIẾT:\n" +
                     "- 12 máy tính đã sử dụng trên 7 năm, cấu hình không đáp ứng\n" +
                     "- 5 máy in thường xuyên hỏng, chi phí sửa chữa cao\n\n" +
                     "II. PHƯƠNG ÁN:\n" +
                     "1. Máy tính để bàn: 12 bộ × 18.000.000đ = 216.000.000đ\n" +
                     "2. Máy in laser: 05 cái × 8.500.000đ = 42.500.000đ\n" +
                     "3. Máy scan: 02 cái × 12.000.000đ = 24.000.000đ\n\n" +
                     "Tổng dự toán: 282.500.000đ\n\n" +
                     "III. NGUỒN KINH PHÍ: Ngân sách chi thường xuyên năm 2026.\n\n" +
                     "Kính trình Ban Giám đốc xem xét, quyết định.",
            Type: DocumentType.ToTrinh, Direction: Direction.Di,
            Issuer: "Phòng Công nghệ thông tin", Category: "Kỹ thuật", NumberPrefix: "TTr",
            Tags: new[] { "Mua sắm", "CNTT", "Năm 2026" },
            Recipients: new[] { "- Ban Giám đốc (xem xét)", "- Phòng KHTC (thẩm định)", "- Lưu: VT, CNTT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 11. Tờ trình sửa chữa trụ sở ──
        new(
            Title: "Tờ trình đề xuất sửa chữa, cải tạo trụ sở làm việc",
            Subject: "Sửa chữa mái tầng 4 và cải tạo nhà vệ sinh trụ sở Sở",
            Content: "Kính gửi: UBND Tỉnh\n\n" +
                     "Sở kính trình phương án sửa chữa, cải tạo trụ sở:\n\n" +
                     "I. THỰC TRẠNG:\n" +
                     "- Mái tầng 4 bị dột nghiêm trọng, ảnh hưởng đến kho lưu trữ\n" +
                     "- Hệ thống nhà vệ sinh xuống cấp, không đảm bảo vệ sinh\n\n" +
                     "II. PHƯƠNG ÁN:\n" +
                     "1. Chống dột mái tầng 4: 180.000.000đ\n" +
                     "2. Cải tạo 4 nhà vệ sinh: 320.000.000đ\n" +
                     "3. Sơn lại tường hành lang: 95.000.000đ\n\n" +
                     "Tổng dự toán: 595.000.000đ\n\n" +
                     "III. THỜI GIAN THI CÔNG: 45 ngày (dự kiến Quý III/2026)\n\n" +
                     "Kính trình UBND Tỉnh phê duyệt.",
            Type: DocumentType.ToTrinh, Direction: Direction.Di,
            Issuer: "Giám đốc Sở", Category: "Hành chính", NumberPrefix: "TTr",
            Tags: new[] { "Xây dựng", "Trụ sở", "Sửa chữa" },
            Recipients: new[] { "- UBND Tỉnh", "- Sở Tài chính (thẩm định)", "- Sở Xây dựng (ý kiến)", "- Lưu: VT, VP" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 12. Chương trình công tác ──
        new(
            Title: "Chương trình công tác năm 2026 của Sở",
            Subject: "Chương trình công tác trọng tâm và lịch hoạt động năm 2026",
            Content: "I. NHIỆM VỤ TRỌNG TÂM:\n" +
                     "1. Triển khai Kế hoạch phát triển KT-XH năm 2026\n" +
                     "2. Hoàn thiện chuyển đổi số, ứng dụng CNTT trong quản lý\n" +
                     "3. Cải cách hành chính, nâng cao chất lượng phục vụ\n" +
                     "4. Xây dựng đề án tái cơ cấu ngành\n\n" +
                     "II. LỊCH HOẠT ĐỘNG:\n" +
                     "- Quý I: Triển khai kế hoạch, tổ chức đào tạo đợt 1\n" +
                     "- Quý II: Kiểm tra giám sát, sơ kết 6 tháng\n" +
                     "- Quý III: Đánh giá giữa kỳ, điều chỉnh kế hoạch\n" +
                     "- Quý IV: Tổng kết, đánh giá CBCCVC, chuẩn bị kế hoạch 2027\n\n" +
                     "III. PHÂN CÔNG: Chi tiết tại Phụ lục đính kèm.",
            Type: DocumentType.ChuongTrinh, Direction: Direction.Di,
            Issuer: "Ban Giám đốc", Category: "Hành chính", NumberPrefix: "CTr",
            Tags: new[] { "Chương trình", "Năm 2026", "Công tác" },
            Recipients: new[] { "- UBND Tỉnh (báo cáo)", "- Các phòng, ban (thực hiện)", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 13. Công điện khẩn phòng chống bão ──
        new(
            Title: "Công điện khẩn về phòng chống bão số 3",
            Subject: "Triển khai khẩn cấp các biện pháp phòng chống bão số 3 năm 2026",
            Content: "GIÁM ĐỐC SỞ ĐIỆN:\n\n" +
                     "Trưởng các phòng, ban; Thủ trưởng các đơn vị trực thuộc\n\n" +
                     "Theo dự báo của Trung tâm Dự báo KTTV Quốc gia, bão số 3 " +
                     "đang di chuyển theo hướng Tây Bắc, dự kiến đổ bộ vào " +
                     "ven biển các tỉnh Trung Bộ trong 24-36 giờ tới.\n\n" +
                     "Giám đốc Sở yêu cầu:\n\n" +
                     "1. Các phòng ban chủ động cho CBCCVC nghỉ sớm để về gia cố nhà cửa\n" +
                     "2. Bộ phận văn thư bảo quản hồ sơ, tài liệu quan trọng\n" +
                     "3. Phòng CNTT sao lưu dữ liệu, tắt máy chủ nếu cần\n" +
                     "4. Chuẩn bị phương án trực 24/24 trong thời gian bão\n" +
                     "5. Số điện thoại trực ban: 0123.456.789\n\n" +
                     "Yêu cầu thực hiện ngay, báo cáo kết quả về Văn phòng Sở.",
            Type: DocumentType.CongDien, Direction: Direction.Di,
            Issuer: "Giám đốc Sở", Category: "Hành chính", NumberPrefix: "CĐ",
            Tags: new[] { "Khẩn cấp", "Phòng chống bão", "Năm 2026" },
            Recipients: new[] { "- Trưởng các phòng, ban", "- Các đơn vị trực thuộc", "- Lưu: VT" },
            Urgency: UrgencyLevel.HoaToc, Security: SecurityLevel.Thuong
        ),

        // ── 14. Hợp đồng bảo trì CNTT ──
        new(
            Title: "Hợp đồng bảo trì hệ thống CNTT năm 2026",
            Subject: "Hợp đồng bảo trì, bảo dưỡng hệ thống máy tính, máy chủ và mạng LAN",
            Content: "HỢP ĐỒNG BẢO TRÌ CNTT\n\n" +
                     "Bên A: Sở (Đại diện: Chánh Văn phòng Lê Văn E)\n" +
                     "Bên B: Công ty TNHH Giải pháp CNTT Miền Trung\n\n" +
                     "Điều 1. NỘI DUNG:\n" +
                     "- Bảo trì 50 máy tính, 3 máy chủ, hệ thống mạng LAN\n" +
                     "- Xử lý sự cố trong 4 giờ (ngày làm việc)\n" +
                     "- Bảo dưỡng định kỳ 3 tháng/lần\n\n" +
                     "Điều 2. GIÁ TRỊ: 96.000.000đ/năm (đã bao gồm VAT)\n\n" +
                     "Điều 3. THỜI HẠN: 01/01/2026 - 31/12/2026\n\n" +
                     "Điều 4. THANH TOÁN: Chuyển khoản theo quý (24 triệu/quý)",
            Type: DocumentType.HopDong, Direction: Direction.Di,
            Issuer: "Văn phòng Sở", Category: "Kỹ thuật", NumberPrefix: "HĐ",
            Tags: new[] { "Hợp đồng", "CNTT", "Bảo trì" },
            Recipients: new[] { "- Bên A (01 bản)", "- Bên B (01 bản)", "- Phòng TCKT (01 bản)", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 15. Giấy giới thiệu ──
        new(
            Title: "Giấy giới thiệu cán bộ đi công tác",
            Subject: "Giới thiệu ông Phạm Văn F đến liên hệ công tác tại Bộ Nội vụ",
            Content: "GIẤY GIỚI THIỆU\n\n" +
                     "Trân trọng giới thiệu:\n\n" +
                     "Ông: Phạm Văn F\n" +
                     "Chức vụ: Trưởng phòng Tổ chức - Hành chính\n" +
                     "Số CCCD: 045026001234\n\n" +
                     "Được cử đến: Vụ Công chức - Viên chức, Bộ Nội vụ\n\n" +
                     "Về việc: Làm việc về thủ tục nâng ngạch công chức cho CBCCVC " +
                     "của Sở theo Kế hoạch nâng ngạch năm 2026.\n\n" +
                     "Thời hạn giấy giới thiệu: Từ ngày 10/03/2026 đến ngày 12/03/2026\n\n" +
                     "Đề nghị Quý cơ quan tiếp và giúp đỡ.",
            Type: DocumentType.GiayGioiThieu, Direction: Direction.Di,
            Issuer: "Chánh Văn phòng Sở", Category: "Hành chính", NumberPrefix: "GGT",
            Tags: new[] { "Giới thiệu", "Công tác", "Bộ Nội vụ" },
            Recipients: new[] { "- Vụ CCVC, Bộ Nội vụ", "- Ông Phạm Văn F (để liên hệ)", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 16. Giấy mời dự họp ──
        new(
            Title: "Giấy mời dự họp sơ kết 6 tháng đầu năm 2026",
            Subject: "Mời dự Hội nghị sơ kết công tác 6 tháng đầu năm 2026 của Sở",
            Content: "Kính gửi: Đại diện các Sở, ban, ngành liên quan\n\n" +
                     "Sở trân trọng kính mời Quý cơ quan cử đại diện tham dự:\n\n" +
                     "Hội nghị sơ kết công tác 6 tháng đầu năm 2026\n\n" +
                     "1. Thời gian: 08h00, Thứ Năm ngày 02/07/2026\n" +
                     "2. Địa điểm: Hội trường lớn, tầng 5, trụ sở Sở\n" +
                     "3. Thành phần: Lãnh đạo hoặc đại diện được ủy quyền\n\n" +
                     "Nội dung chính:\n" +
                     "- Báo cáo kết quả 6 tháng đầu năm\n" +
                     "- Phương hướng 6 tháng cuối năm\n" +
                     "- Thảo luận khó khăn, vướng mắc\n\n" +
                     "Rất mong sự tham dự của Quý cơ quan.",
            Type: DocumentType.GiayMoi, Direction: Direction.Di,
            Issuer: "Giám đốc Sở", Category: "Hành chính", NumberPrefix: "GM",
            Tags: new[] { "Giấy mời", "Sơ kết", "6 tháng" },
            Recipients: new[] { "- Các Sở, ban, ngành liên quan", "- Các đơn vị trực thuộc", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 17. Phiếu chuyển văn bản ──
        new(
            Title: "Phiếu chuyển văn bản đến Phòng KHTC",
            Subject: "Chuyển công văn của Sở Tài chính về hướng dẫn lập dự toán 2027 đến Phòng KHTC xử lý",
            Content: "PHIẾU CHUYỂN\n\n" +
                     "Kính gửi: Trưởng phòng Kế hoạch - Tài chính\n\n" +
                     "Văn phòng Sở chuyển đến Phòng KHTC:\n\n" +
                     "Văn bản: Công văn số 2456/STC-QLNS ngày 15/08/2026 của Sở Tài chính\n" +
                     "V/v: Hướng dẫn xây dựng dự toán NSNN năm 2027\n\n" +
                     "Ý kiến chỉ đạo của Giám đốc:\n" +
                     "\"Giao Phòng KHTC chủ trì, phối hợp các phòng ban xây dựng dự toán " +
                     "theo hướng dẫn. Hoàn thành trước 15/09/2026.\"\n\n" +
                     "Đề nghị Phòng KHTC tiếp nhận và triển khai.",
            Type: DocumentType.PhieuChuyen, Direction: Direction.Di,
            Issuer: "Văn phòng Sở", Category: "Tài chính", NumberPrefix: "PC",
            Tags: new[] { "Phiếu chuyển", "Dự toán", "Tài chính" },
            Recipients: new[] { "- Phòng KHTC (để thực hiện)", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 18. Nghị quyết hội nghị CBCC ──
        new(
            Title: "Nghị quyết Hội nghị cán bộ, công chức năm 2026",
            Subject: "Nghị quyết Hội nghị CBCC năm 2026 về phương hướng, nhiệm vụ và cam kết thi đua",
            Content: "HỘI NGHỊ CÁN BỘ, CÔNG CHỨC NĂM 2026\n\n" +
                     "QUYẾT NGHỊ:\n\n" +
                     "1. Thống nhất mục tiêu: Hoàn thành xuất sắc nhiệm vụ năm 2026, " +
                     "giải quyết 100% hồ sơ đúng hạn.\n\n" +
                     "2. Phấn đấu:\n" +
                     "- 95% CBCCVC đạt hoàn thành tốt nhiệm vụ trở lên\n" +
                     "- Tỷ lệ hồ sơ trễ hẹn dưới 2%\n" +
                     "- 100% CBCCVC tham gia bồi dưỡng chuyên môn\n\n" +
                     "3. Cam kết thực hiện nghiêm:\n" +
                     "- Nội quy, kỷ luật công vụ\n" +
                     "- Quy chế dân chủ ở cơ sở\n" +
                     "- Phong trào thi đua yêu nước\n\n" +
                     "Nghị quyết này đã được toàn thể Hội nghị biểu quyết thông qua ngày 20/01/2026.",
            Type: DocumentType.NghiQuyet, Direction: Direction.Di,
            Issuer: "Hội nghị CBCC", Category: "Tổ chức", NumberPrefix: "NQ",
            Tags: new[] { "Nghị quyết", "Hội nghị", "Thi đua" },
            Recipients: new[] { "- Toàn thể CBCCVC", "- Công đoàn cơ sở", "- Đoàn Thanh niên", "- Lưu: VT, TCCB" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ╔═══════════════════════════════════════════════════════╗
        // ║  PHẦN 2: VĂN BẢN ĐẾN  (18 mẫu)                     ║
        // ╚═══════════════════════════════════════════════════════╝

        // ── 19. CV yêu cầu báo cáo CCHC ──
        new(
            Title: "Công văn yêu cầu báo cáo cải cách hành chính",
            Subject: "Yêu cầu báo cáo kết quả thực hiện CCHC 6 tháng đầu năm 2026",
            Content: "Kính gửi: Giám đốc các Sở, ban, ngành\n\n" +
                     "UBND Tỉnh yêu cầu:\n\n" +
                     "1. Các đơn vị báo cáo kết quả CCHC 6 tháng đầu năm 2026\n" +
                     "2. Nội dung theo đề cương đính kèm\n" +
                     "3. Thời hạn gửi: trước ngày 25/06/2026\n" +
                     "4. Gửi về: Sở Nội vụ (cơ quan thường trực)\n\n" +
                     "Yêu cầu các đơn vị nghiêm túc thực hiện, gửi đúng hạn.",
            Type: DocumentType.CongVan, Direction: Direction.Den,
            Issuer: "UBND Tỉnh", Category: "Hành chính", NumberPrefix: "CV",
            Tags: new[] { "CCHC", "Báo cáo", "Năm 2026" },
            Recipients: null,
            Urgency: UrgencyLevel.Khan, Security: SecurityLevel.Thuong
        ),

        // ── 20. Chỉ thị phòng chống tham nhũng ──
        new(
            Title: "Chỉ thị tăng cường phòng chống tham nhũng",
            Subject: "Tăng cường công tác phòng, chống tham nhũng, tiêu cực trong cơ quan nhà nước",
            Content: "Kính gửi: Thủ trưởng các Sở, ban, ngành; Chủ tịch UBND các huyện\n\n" +
                     "Chủ tịch UBND Tỉnh chỉ thị:\n\n" +
                     "1. Tăng cường công khai, minh bạch\n" +
                     "2. Thực hiện nghiêm kê khai tài sản, thu nhập\n" +
                     "3. Rà soát lĩnh vực dễ phát sinh tham nhũng\n" +
                     "4. Xử lý nghiêm các trường hợp vi phạm\n" +
                     "5. Bảo vệ người tố cáo theo quy định\n\n" +
                     "Yêu cầu thủ trưởng các đơn vị triển khai, báo cáo trước 30/06/2026.",
            Type: DocumentType.ChiThi, Direction: Direction.Den,
            Issuer: "UBND Tỉnh", Category: "Pháp chế", NumberPrefix: "CT",
            Tags: new[] { "Phòng chống tham nhũng", "Quan trọng", "Năm 2026" },
            Recipients: null,
            Urgency: UrgencyLevel.Khan, Security: SecurityLevel.Mat
        ),

        // ── 21. Thông tư hướng dẫn kinh phí ──
        new(
            Title: "Thông tư hướng dẫn quản lý kinh phí CCHC",
            Subject: "Hướng dẫn lập dự toán, quản lý, sử dụng và quyết toán kinh phí CCHC",
            Content: "Căn cứ Luật Ngân sách nhà nước năm 2015;\n" +
                     "Căn cứ Nghị định 163/2016/NĐ-CP;\n\n" +
                     "Bộ Tài chính hướng dẫn:\n\n" +
                     "Chương I. QUY ĐỊNH CHUNG\n" +
                     "Điều 1. Phạm vi điều chỉnh\n" +
                     "Điều 2. Đối tượng áp dụng\n\n" +
                     "Chương II. NỘI DUNG VÀ MỨC CHI\n" +
                     "Điều 3. Nội dung chi\n" +
                     "Điều 4. Mức chi theo quy định hiện hành\n\n" +
                     "Chương III. LẬP DỰ TOÁN, QUYẾT TOÁN\n" +
                     "Điều 5. Lập dự toán hằng năm\n" +
                     "Điều 6. Quản lý và sử dụng kinh phí\n\n" +
                     "Thông tư có hiệu lực từ 01/07/2026.",
            Type: DocumentType.ThongTu, Direction: Direction.Den,
            Issuer: "Bộ Tài chính", Category: "Tài chính", NumberPrefix: "TT",
            Tags: new[] { "Tài chính", "Ngân sách", "Thông tư" },
            Recipients: null,
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 22. Giấy mời họp liên ngành ──
        new(
            Title: "Giấy mời họp liên ngành về quy hoạch",
            Subject: "Mời dự họp liên ngành về phương án quy hoạch vùng 2026-2030",
            Content: "Kính gửi: Giám đốc Sở\n\n" +
                     "UBND Tỉnh trân trọng mời dự cuộc họp:\n\n" +
                     "1. Nội dung: Phương án quy hoạch vùng giai đoạn 2026-2030\n" +
                     "2. Thời gian: 14h00, ngày 20/03/2026\n" +
                     "3. Địa điểm: Phòng họp A, UBND Tỉnh\n" +
                     "4. Thành phần: Lãnh đạo các Sở: KHĐT, XD, TNMT, GTVT, NN&PTNT\n" +
                     "5. Yêu cầu: Chuẩn bị ý kiến bằng văn bản\n\n" +
                     "Đề nghị tham dự đúng thành phần.",
            Type: DocumentType.GiayMoi, Direction: Direction.Den,
            Issuer: "UBND Tỉnh", Category: "Hành chính", NumberPrefix: "GM",
            Tags: new[] { "Họp", "Quy hoạch", "Năm 2026" },
            Recipients: null,
            Urgency: UrgencyLevel.ThuongKhan, Security: SecurityLevel.Thuong
        ),

        // ── 23. CV đôn đốc báo cáo ──
        new(
            Title: "Công văn đôn đốc nộp báo cáo thống kê",
            Subject: "Đôn đốc gửi báo cáo thống kê tổng hợp năm 2025",
            Content: "Kính gửi: Giám đốc các Sở, ban, ngành\n\n" +
                     "Đến nay, Sở KHĐT vẫn chưa nhận được báo cáo thống kê " +
                     "năm 2025 của một số đơn vị (danh sách đính kèm).\n\n" +
                     "Đề nghị hoàn thiện và gửi trước ngày 15/02/2026.\n\n" +
                     "Sau thời hạn trên sẽ báo cáo Chủ tịch UBND Tỉnh.",
            Type: DocumentType.CongVan, Direction: Direction.Den,
            Issuer: "Sở Kế hoạch và Đầu tư", Category: "Hành chính", NumberPrefix: "CV",
            Tags: new[] { "Đôn đốc", "Báo cáo", "Thống kê" },
            Recipients: null,
            Urgency: UrgencyLevel.Khan, Security: SecurityLevel.Thuong
        ),

        // ── 24. Nghị định sửa đổi ──
        new(
            Title: "Nghị định sửa đổi về công tác văn thư",
            Subject: "Sửa đổi, bổ sung một số điều NĐ 30/2020/NĐ-CP về công tác văn thư",
            Content: "CHÍNH PHỦ\n\n" +
                     "Căn cứ Luật Tổ chức Chính phủ;\n" +
                     "Theo đề nghị của Bộ trưởng Bộ Nội vụ;\n\n" +
                     "NGHỊ ĐỊNH:\n\n" +
                     "Điều 1. Sửa đổi khoản 2 Điều 10: Bổ sung VB điện tử\n" +
                     "Điều 2. Bổ sung Điều 10a: Chữ ký số\n" +
                     "Điều 3. Sửa đổi Điều 15: Quản lý VB đi điện tử\n\n" +
                     "Có hiệu lực từ 01/01/2027.",
            Type: DocumentType.NghiDinh, Direction: Direction.Den,
            Issuer: "Chính phủ", Category: "Pháp chế", NumberPrefix: "NĐ",
            Tags: new[] { "Văn thư", "Nghị định", "VBQPPL" },
            Recipients: null,
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 25. CV đào tạo nước ngoài ──
        new(
            Title: "Công văn rà soát nhu cầu đào tạo ở nước ngoài",
            Subject: "Rà soát, tổng hợp nhu cầu đào tạo bồi dưỡng ở nước ngoài năm 2027",
            Content: "Kính gửi: Giám đốc Sở\n\n" +
                     "Bộ Nội vụ đề nghị:\n\n" +
                     "1. Rà soát CBCCVC có nhu cầu đào tạo nước ngoài năm 2027\n" +
                     "2. Lĩnh vực ưu tiên: Quản lý công, chuyển đổi số\n" +
                     "3. Tiêu chuẩn: IELTS 6.0 trở lên\n" +
                     "4. Hồ sơ: Đơn, lý lịch, chứng chỉ ngoại ngữ\n\n" +
                     "Thời hạn gửi: trước 30/09/2026\n" +
                     "Gửi về: Vụ Đào tạo, Bộ Nội vụ",
            Type: DocumentType.CongVan, Direction: Direction.Den,
            Issuer: "Bộ Nội vụ", Category: "Đào tạo", NumberPrefix: "CV",
            Tags: new[] { "Đào tạo", "Nước ngoài", "Quan trọng" },
            Recipients: null,
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Mat
        ),

        // ── 26. Luật Lưu trữ ──
        new(
            Title: "Luật Lưu trữ (sửa đổi) năm 2024",
            Subject: "Luật Lưu trữ (sửa đổi) được Quốc hội thông qua, có hiệu lực từ 01/07/2025",
            Content: "QUỐC HỘI NƯỚC CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM\n\n" +
                     "LUẬT LƯU TRỮ (SỬA ĐỔI)\n\n" +
                     "Chương I. QUY ĐỊNH CHUNG\n" +
                     "Điều 1. Phạm vi điều chỉnh\n" +
                     "Điều 2. Giải thích từ ngữ\n" +
                     "Điều 3. Nguyên tắc hoạt động lưu trữ\n\n" +
                     "Chương II. TÀI LIỆU LƯU TRỮ ĐIỆN TỬ\n" +
                     "Điều 15. Tài liệu lưu trữ điện tử\n" +
                     "Điều 16. Quản lý tài liệu điện tử\n\n" +
                     "Chương III. QUẢN LÝ TÀI LIỆU LƯU TRỮ\n" +
                     "Điều 20. Thu thập tài liệu\n" +
                     "Điều 21. Tiêu hủy tài liệu hết giá trị\n\n" +
                     "Luật này có hiệu lực thi hành từ 01/07/2025.",
            Type: DocumentType.Luat, Direction: Direction.Den,
            Issuer: "Quốc hội", Category: "Pháp chế", NumberPrefix: "Luật",
            Tags: new[] { "Lưu trữ", "Luật", "VBQPPL" },
            Recipients: null,
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 27. CV phối hợp phòng dịch ──
        new(
            Title: "Công văn đề nghị phối hợp phòng chống dịch bệnh",
            Subject: "Đề nghị phối hợp triển khai các biện pháp phòng, chống dịch cúm mùa",
            Content: "Kính gửi: Giám đốc Sở\n\n" +
                     "Trước diễn biến phức tạp của dịch cúm mùa, Sở Y tế đề nghị:\n\n" +
                     "1. Tuyên truyền CBCCVC thực hiện các biện pháp phòng dịch\n" +
                     "2. Chuẩn bị khẩu trang, nước sát khuẩn tại trụ sở\n" +
                     "3. Cho phép CBCCVC có triệu chứng nghỉ điều trị tại nhà\n" +
                     "4. Hạn chế tổ chức họp đông người trong giai đoạn cao điểm\n" +
                     "5. Báo cáo tình hình sức khỏe CBCCVC hằng tuần\n\n" +
                     "Gửi báo cáo về: Trung tâm Kiểm soát bệnh tật tỉnh (CDC)\n" +
                     "ĐT: 0256.3823.456",
            Type: DocumentType.CongVan, Direction: Direction.Den,
            Issuer: "Sở Y tế", Category: "Y tế", NumberPrefix: "CV",
            Tags: new[] { "Y tế", "Phòng dịch", "Khẩn" },
            Recipients: null,
            Urgency: UrgencyLevel.Khan, Security: SecurityLevel.Thuong
        ),

        // ── 28. TB cắt giảm ngân sách ──
        new(
            Title: "Thông báo điều chỉnh dự toán ngân sách năm 2026",
            Subject: "Điều chỉnh giảm 10% dự toán chi thường xuyên 6 tháng cuối năm 2026",
            Content: "Kính gửi: Thủ trưởng các Sở, ban, ngành\n\n" +
                     "Thực hiện chỉ đạo của Thủ tướng Chính phủ về tiết kiệm chi, " +
                     "Sở Tài chính thông báo:\n\n" +
                     "1. Cắt giảm 10% dự toán chi thường xuyên 6 tháng cuối năm 2026\n" +
                     "2. Không bổ sung kinh phí mua sắm ngoài kế hoạch\n" +
                     "3. Tiết kiệm tối thiểu 15% chi hội nghị, công tác phí\n" +
                     "4. Tạm dừng mua sắm ô tô công theo chỉ đạo của Thủ tướng\n\n" +
                     "Các đơn vị rà soát, điều chỉnh kế hoạch chi tiêu phù hợp.\n\n" +
                     "Gửi kế hoạch điều chỉnh về Sở Tài chính trước 15/07/2026.",
            Type: DocumentType.ThongBao, Direction: Direction.Den,
            Issuer: "Sở Tài chính", Category: "Tài chính", NumberPrefix: "TB",
            Tags: new[] { "Ngân sách", "Cắt giảm", "Tài chính" },
            Recipients: null,
            Urgency: UrgencyLevel.Khan, Security: SecurityLevel.Thuong
        ),

        // ── 29. CV phối hợp tuyển dụng GV ──
        new(
            Title: "Công văn đề nghị phối hợp tuyển dụng giáo viên",
            Subject: "Phối hợp tổ chức kỳ thi tuyển dụng viên chức ngành giáo dục năm 2026",
            Content: "Kính gửi: Giám đốc Sở Nội vụ\n\n" +
                     "Sở Giáo dục và Đào tạo đề nghị Sở Nội vụ phối hợp:\n\n" +
                     "1. Thẩm định kế hoạch tuyển dụng 250 giáo viên các cấp\n" +
                     "2. Cử cán bộ tham gia Hội đồng tuyển dụng\n" +
                     "3. Giám sát quy trình thi tuyển đảm bảo khách quan\n\n" +
                     "Nhu cầu tuyển dụng:\n" +
                     "- Mầm non: 45 giáo viên\n" +
                     "- Tiểu học: 80 giáo viên\n" +
                     "- THCS: 75 giáo viên\n" +
                     "- THPT: 50 giáo viên\n\n" +
                     "Dự kiến thi tuyển: Tháng 7/2026\n" +
                     "Kính đề nghị Sở Nội vụ xem xét, phối hợp.",
            Type: DocumentType.CongVan, Direction: Direction.Den,
            Issuer: "Sở Giáo dục và Đào tạo", Category: "Nhân sự", NumberPrefix: "CV",
            Tags: new[] { "Tuyển dụng", "Giáo dục", "Năm 2026" },
            Recipients: null,
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 30. Hướng dẫn đánh giá CBCCVC ──
        new(
            Title: "Hướng dẫn đánh giá, xếp loại CBCCVC năm 2025",
            Subject: "Hướng dẫn đánh giá, xếp loại chất lượng cán bộ, công chức, viên chức năm 2025",
            Content: "Kính gửi: Thủ trưởng các Sở, ban, ngành; Chủ tịch UBND huyện, TP\n\n" +
                     "Căn cứ Nghị định 90/2020/NĐ-CP, Sở Nội vụ hướng dẫn:\n\n" +
                     "I. ĐỐI TƯỢNG: Toàn bộ CBCCVC trong biên chế và hợp đồng\n\n" +
                     "II. TIÊU CHÍ ĐÁNH GIÁ:\n" +
                     "1. Chính trị, tư tưởng, đạo đức, lối sống\n" +
                     "2. Kết quả thực hiện nhiệm vụ được giao\n" +
                     "3. Ý thức tổ chức kỷ luật\n" +
                     "4. Tinh thần phối hợp, thái độ phục vụ nhân dân\n\n" +
                     "III. XẾP LOẠI:\n" +
                     "- Hoàn thành xuất sắc nhiệm vụ\n" +
                     "- Hoàn thành tốt nhiệm vụ\n" +
                     "- Hoàn thành nhiệm vụ\n" +
                     "- Không hoàn thành nhiệm vụ\n\n" +
                     "IV. THỜI HẠN: Gửi kết quả trước 20/12/2025",
            Type: DocumentType.HuongDan, Direction: Direction.Den,
            Issuer: "Sở Nội vụ", Category: "Nhân sự", NumberPrefix: "HD",
            Tags: new[] { "Đánh giá", "CBCCVC", "Năm 2025" },
            Recipients: null,
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 31. CV phân loại rác ──
        new(
            Title: "Công văn yêu cầu thực hiện phân loại rác thải",
            Subject: "Triển khai phân loại chất thải rắn tại nguồn tại các cơ quan nhà nước",
            Content: "Kính gửi: Thủ trưởng các Sở, ban, ngành\n\n" +
                     "Thực hiện Luật Bảo vệ môi trường 2020 và Nghị định 08/2022/NĐ-CP, " +
                     "Sở Tài nguyên và Môi trường đề nghị:\n\n" +
                     "1. Bố trí thùng rác phân loại tại trụ sở (3 loại: tái chế, hữu cơ, còn lại)\n" +
                     "2. Tuyên truyền CBCCVC thực hiện phân loại rác đúng quy định\n" +
                     "3. Ký hợp đồng với đơn vị thu gom có phân loại\n" +
                     "4. Giảm thiểu sử dụng túi nilon, đồ nhựa dùng 1 lần\n" +
                     "5. Báo cáo kết quả thực hiện hằng quý\n\n" +
                     "Thời hạn triển khai: trước 31/03/2026",
            Type: DocumentType.CongVan, Direction: Direction.Den,
            Issuer: "Sở Tài nguyên và Môi trường", Category: "Hành chính", NumberPrefix: "CV",
            Tags: new[] { "Môi trường", "Phân loại rác", "Năm 2026" },
            Recipients: null,
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 32. QĐ giao chỉ tiêu biên chế ──
        new(
            Title: "Quyết định giao chỉ tiêu biên chế công chức năm 2026",
            Subject: "Giao chỉ tiêu biên chế công chức trong cơ quan hành chính nhà nước năm 2026",
            Content: "Căn cứ Luật Tổ chức Chính quyền địa phương;\n" +
                     "Căn cứ Nghị quyết của HĐND Tỉnh về biên chế;\n" +
                     "Xét đề nghị của Giám đốc Sở Nội vụ,\n\n" +
                     "CHỦ TỊCH UBND TỈNH QUYẾT ĐỊNH:\n\n" +
                     "Điều 1. Giao chỉ tiêu biên chế cho Sở: 85 biên chế, trong đó:\n" +
                     "- Lãnh đạo: 04\n" +
                     "- Chuyên viên chính: 15\n" +
                     "- Chuyên viên: 52\n" +
                     "- Cán sự, nhân viên: 14\n\n" +
                     "Điều 2. Giám đốc Sở bố trí, sắp xếp CBCC phù hợp vị trí việc làm.\n\n" +
                     "Điều 3. Quyết định có hiệu lực từ 01/01/2026.",
            Type: DocumentType.QuyetDinh, Direction: Direction.Den,
            Issuer: "UBND Tỉnh", Category: "Nhân sự", NumberPrefix: "QĐ",
            Tags: new[] { "Biên chế", "Nhân sự", "Năm 2026" },
            Recipients: null,
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 33. Bản thỏa thuận hợp tác ──
        new(
            Title: "Bản thỏa thuận hợp tác với Sở Thông tin và Truyền thông",
            Subject: "Thỏa thuận hợp tác triển khai chuyển đổi số trong quản lý văn bản",
            Content: "BẢN THỎA THUẬN HỢP TÁC\n\n" +
                     "Bên A: Sở (Đại diện: Giám đốc Trần Văn B)\n" +
                     "Bên B: Sở Thông tin và Truyền thông (Đại diện: Giám đốc Nguyễn Thị G)\n\n" +
                     "HAI BÊN THỎA THUẬN:\n\n" +
                     "1. Bên B hỗ trợ Bên A triển khai hệ thống quản lý văn bản điện tử\n" +
                     "2. Bên B đào tạo CBCC của Bên A sử dụng chữ ký số\n" +
                     "3. Bên A chia sẻ kinh nghiệm ứng dụng CNTT trong lĩnh vực chuyên ngành\n" +
                     "4. Hai bên phối hợp xây dựng cơ sở dữ liệu dùng chung\n\n" +
                     "Thời hạn: 2 năm (2026-2027)\n\n" +
                     "Thỏa thuận này được lập thành 04 bản, mỗi bên giữ 02 bản.",
            Type: DocumentType.BanThoaThuan, Direction: Direction.Den,
            Issuer: "Sở Thông tin và Truyền thông", Category: "Kỹ thuật", NumberPrefix: "BTT",
            Tags: new[] { "Hợp tác", "Chuyển đổi số", "CNTT" },
            Recipients: null,
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 34. Thông cáo kỳ họp HĐND ──
        new(
            Title: "Thông cáo kết quả kỳ họp HĐND Tỉnh",
            Subject: "Thông cáo về kết quả kỳ họp thứ 8, HĐND Tỉnh khóa XII",
            Content: "THÔNG CÁO\n\n" +
                     "Kỳ họp thứ 8, HĐND Tỉnh khóa XII đã diễn ra từ ngày 10-12/12/2025 " +
                     "tại Hội trường HĐND Tỉnh.\n\n" +
                     "KẾT QUẢ KỲ HỌP:\n\n" +
                     "1. Thông qua 15 Nghị quyết, trong đó:\n" +
                     "   - NQ về dự toán NSNN năm 2026\n" +
                     "   - NQ về kế hoạch phát triển KT-XH 2026\n" +
                     "   - NQ về chỉ tiêu biên chế 2026\n\n" +
                     "2. Chất vấn và trả lời chất vấn: 5 nhóm vấn đề\n\n" +
                     "3. Bầu bổ sung Ủy viên UBND Tỉnh\n\n" +
                     "Chi tiết các Nghị quyết được đăng tải trên Cổng TTĐT của Tỉnh.",
            Type: DocumentType.ThongCao, Direction: Direction.Den,
            Issuer: "HĐND Tỉnh", Category: "Hành chính", NumberPrefix: "TC",
            Tags: new[] { "HĐND", "Kỳ họp", "Nghị quyết" },
            Recipients: null,
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 35. Đề án chuyển đổi số ──
        new(
            Title: "Đề án chuyển đổi số tỉnh giai đoạn 2025-2030",
            Subject: "Phê duyệt Đề án chuyển đổi số tỉnh giai đoạn 2025-2030, tầm nhìn 2035",
            Content: "I. SỰ CẦN THIẾT:\n" +
                     "- Thực hiện Chương trình chuyển đổi số quốc gia\n" +
                     "- Tỉnh đang ở mức trung bình về chỉ số DTI\n\n" +
                     "II. MỤC TIÊU ĐẾN NĂM 2030:\n" +
                     "1. Chính quyền số: 100% dịch vụ công trực tuyến toàn trình\n" +
                     "2. Kinh tế số: Chiếm 25% GRDP\n" +
                     "3. Xã hội số: 80% dân số có tài khoản số\n\n" +
                     "III. NHIỆM VỤ:\n" +
                     "1. Xây dựng hạ tầng số (LGSP, IOC, trung tâm dữ liệu)\n" +
                     "2. Phát triển dữ liệu số (CSDL dùng chung)\n" +
                     "3. Đào tạo nguồn nhân lực số\n" +
                     "4. An toàn thông tin\n\n" +
                     "IV. KINH PHÍ: 350 tỷ đồng (2025-2030)",
            Type: DocumentType.DeAn, Direction: Direction.Den,
            Issuer: "UBND Tỉnh", Category: "Kỹ thuật", NumberPrefix: "ĐA",
            Tags: new[] { "Chuyển đổi số", "Đề án", "CNTT" },
            Recipients: null,
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 36. Phương án phòng cháy ──
        new(
            Title: "Phương án chữa cháy của cơ sở",
            Subject: "Phương án chữa cháy và cứu nạn, cứu hộ tại trụ sở Sở năm 2026",
            Content: "I. ĐẶC ĐIỂM CƠ SỞ:\n" +
                     "- Tòa nhà 5 tầng, tổng diện tích 3.200 m²\n" +
                     "- Số người làm việc: 120 người\n" +
                     "- Có phòng máy chủ, kho lưu trữ hồ sơ (nguy cơ cháy cao)\n\n" +
                     "II. PHƯƠNG ÁN:\n" +
                     "1. Khi phát hiện cháy: Bấm chuông báo động, gọi 114\n" +
                     "2. Sơ tán: Theo cầu thang bộ, tập kết tại sân trước\n" +
                     "3. Chữa cháy ban đầu: Sử dụng bình chữa cháy xách tay\n" +
                     "4. Cứu nạn: Ưu tiên người bị thương, người cao tuổi\n\n" +
                     "III. LỰC LƯỢNG: Đội PCCC cơ sở 15 người\n\n" +
                     "IV. DIỄN TẬP: 2 lần/năm (Quý II và Quý IV)",
            Type: DocumentType.PhuongAn, Direction: Direction.Den,
            Issuer: "Cảnh sát PCCC Tỉnh", Category: "Hành chính", NumberPrefix: "PA",
            Tags: new[] { "PCCC", "An toàn", "Phương án" },
            Recipients: null,
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ╔═══════════════════════════════════════════════════════╗
        // ║  PHẦN 3: VĂN BẢN NỘI BỘ  (14 mẫu)                  ║
        // ╚═══════════════════════════════════════════════════════╝

        // ── 37. Quy chế làm việc ──
        new(
            Title: "Quy chế làm việc của cơ quan",
            Subject: "Ban hành Quy chế làm việc, quy định trách nhiệm và quyền hạn các phòng ban",
            Content: "Chương I. QUY ĐỊNH CHUNG\n\n" +
                     "Điều 1. Phạm vi, đối tượng\n" +
                     "Quy chế quy định nguyên tắc, chế độ trách nhiệm, lề lối làm việc.\n\n" +
                     "Điều 2. Nguyên tắc\n" +
                     "- Tuân thủ pháp luật, đúng thẩm quyền\n" +
                     "- Công khai minh bạch, đảm bảo kỷ cương\n\n" +
                     "Chương II. TRÁCH NHIỆM, QUYỀN HẠN\n" +
                     "Điều 3. Giám đốc Sở\n" +
                     "Điều 4. Phó Giám đốc\n" +
                     "Điều 5. Trưởng phòng chuyên môn\n\n" +
                     "Chương III. CHẾ ĐỘ LÀM VIỆC\n" +
                     "Điều 6. Giờ: Sáng 7h30-11h30, Chiều 13h30-17h00\n" +
                     "Điều 7. Chế độ hội họp",
            Type: DocumentType.QuyChE, Direction: Direction.NoiBo,
            Issuer: "Ban Giám đốc", Category: "Hành chính", NumberPrefix: "QC",
            Tags: new[] { "Quy chế", "Nội bộ", "Năm 2026" },
            Recipients: new[] { "- Toàn thể CBCCVC", "- Các phòng ban", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 38. Biên bản giao ban ──
        new(
            Title: "Biên bản họp giao ban tháng 01/2026",
            Subject: "Họp giao ban lãnh đạo Sở tháng 01/2026, đánh giá công tác và triển khai nhiệm vụ",
            Content: "BIÊN BẢN HỌP GIAO BAN LÃNH ĐẠO SỞ\n\n" +
                     "Thời gian: 8h30, ngày 05/01/2026\n" +
                     "Địa điểm: Phòng họp số 1\n" +
                     "Chủ trì: GĐ Trần Văn B\n" +
                     "Thành phần: Các PGĐ, Trưởng phòng ban\n\n" +
                     "I. NỘI DUNG:\n" +
                     "1. Đánh giá tháng 12/2025\n" +
                     "2. Triển khai nhiệm vụ tháng 01/2026\n\n" +
                     "II. KẾT LUẬN CỦA GIÁM ĐỐC:\n" +
                     "- Hoàn thành KH năm 2026 trước 15/01\n" +
                     "- Phòng CNTT triển khai phần mềm VanBanPlus trước 31/01\n" +
                     "- Phòng TCHC tổ chức hội nghị tổng kết trước 20/01\n\n" +
                     "Kết thúc lúc 11h00.",
            Type: DocumentType.BienBan, Direction: Direction.NoiBo,
            Issuer: "Văn phòng Sở", Category: "Hành chính", NumberPrefix: "BB",
            Tags: new[] { "Giao ban", "Nội bộ", "Tháng 01" },
            Recipients: new[] { "- Ban Giám đốc", "- Trưởng phòng ban", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 39. Biên bản nghiệm thu ──
        new(
            Title: "Biên bản nghiệm thu bàn giao thiết bị CNTT",
            Subject: "Nghiệm thu bàn giao 12 bộ máy tính và 5 máy in theo Hợp đồng số 15/HĐ",
            Content: "BIÊN BẢN NGHIỆM THU BÀN GIAO\n\n" +
                     "Thời gian: 14h00, ngày 15/03/2026\n" +
                     "Địa điểm: Phòng CNTT, tầng 2\n\n" +
                     "BÊN GIAO: Công ty TNHH Giải pháp CNTT Miền Trung\n" +
                     "BÊN NHẬN: Phòng CNTT, Sở\n\n" +
                     "NỘI DUNG NGHIỆM THU:\n" +
                     "1. Máy tính để bàn: 12/12 bộ - Đạt yêu cầu\n" +
                     "   - CPU: Intel Core i5-13400, RAM 16GB, SSD 512GB\n" +
                     "2. Máy in laser: 5/5 cái - Đạt yêu cầu\n" +
                     "   - Model: HP LaserJet Pro M404dn\n\n" +
                     "KẾT LUẬN: Đạt yêu cầu, đồng ý nghiệm thu và đưa vào sử dụng.\n\n" +
                     "Bảo hành: 36 tháng kể từ ngày nghiệm thu.",
            Type: DocumentType.BienBan, Direction: Direction.NoiBo,
            Issuer: "Phòng Công nghệ thông tin", Category: "Kỹ thuật", NumberPrefix: "BB",
            Tags: new[] { "Nghiệm thu", "CNTT", "Thiết bị" },
            Recipients: new[] { "- Bên giao (01 bản)", "- Phòng CNTT (01 bản)", "- Phòng TCKT (01 bản)", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 40. TB trực Tết ──
        new(
            Title: "Thông báo lịch trực Tết Nguyên đán 2026",
            Subject: "Phân công lịch trực cơ quan dịp Tết Nguyên đán Bính Ngọ 2026",
            Content: "Kính gửi: Toàn thể CBCCVC\n\n" +
                     "Ban Giám đốc phân công lịch trực Tết:\n\n" +
                     "28 Tết (27/01): Đ/c Nguyễn Văn A - PGĐ\n" +
                     "29 Tết (28/01): Đ/c Trần Văn B - GĐ\n" +
                     "30 Tết (29/01): Đ/c Lê Thị C - PGĐ\n" +
                     "Mùng 1 (30/01): Đ/c Trần Văn B - GĐ\n" +
                     "Mùng 2 (31/01): Đ/c Nguyễn Văn A - PGĐ\n" +
                     "Mùng 3 (01/02): Đ/c Lê Thị C - PGĐ\n\n" +
                     "Trực từ 8h00-17h00. ĐT: 0123.456.789",
            Type: DocumentType.ThongBao, Direction: Direction.NoiBo,
            Issuer: "Văn phòng Sở", Category: "Hành chính", NumberPrefix: "TB",
            Tags: new[] { "Trực Tết", "Nội bộ", "Năm 2026" },
            Recipients: new[] { "- Toàn thể CBCCVC", "- Bảo vệ", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 41. Hướng dẫn sử dụng phần mềm ──
        new(
            Title: "Hướng dẫn sử dụng phần mềm quản lý văn bản",
            Subject: "Hướng dẫn cài đặt và sử dụng phần mềm VanBanPlus cho CBCCVC",
            Content: "Kính gửi: Toàn thể CBCCVC\n\n" +
                     "Phòng CNTT hướng dẫn sử dụng VanBanPlus:\n\n" +
                     "1. CÀI ĐẶT:\n" +
                     "- Tải tại mục Chia sẻ nội bộ\n" +
                     "- Chạy VanBanPlus-Setup.exe\n" +
                     "- Đăng nhập bằng email cơ quan\n\n" +
                     "2. CHỨC NĂNG:\n" +
                     "- Quản lý VB đi/đến/nội bộ\n" +
                     "- Soạn thảo VB bằng AI\n" +
                     "- Theo dõi hạn xử lý\n" +
                     "- Thống kê, báo cáo\n\n" +
                     "3. HỖ TRỢ: Phòng CNTT (ext: 108) hoặc cntt@so.gov.vn\n\n" +
                     "Hoàn thành cài đặt trước 28/02/2026.",
            Type: DocumentType.HuongDan, Direction: Direction.NoiBo,
            Issuer: "Phòng Công nghệ thông tin", Category: "Kỹ thuật", NumberPrefix: "HD",
            Tags: new[] { "CNTT", "Phần mềm", "Hướng dẫn" },
            Recipients: new[] { "- Toàn thể CBCCVC", "- Trưởng phòng ban", "- Lưu: VT, CNTT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 42. Quy định xe công ──
        new(
            Title: "Quy định về sử dụng xe công",
            Subject: "Quản lý, sử dụng xe ô tô phục vụ công tác của cơ quan",
            Content: "Điều 1. Phạm vi: Quản lý, sử dụng xe ô tô công.\n\n" +
                     "Điều 2. Nguyên tắc:\n" +
                     "- Chỉ dùng cho công vụ\n" +
                     "- Đăng ký trước 01 ngày qua Văn phòng\n" +
                     "- Ưu tiên đi chung cùng hướng\n\n" +
                     "Điều 3. Quy trình:\n" +
                     "1. Gửi phiếu đăng ký\n" +
                     "2. Văn phòng sắp xếp\n" +
                     "3. Lái xe nhận lệnh\n\n" +
                     "Điều 4. Trách nhiệm:\n" +
                     "- Người dùng: Bảo quản, không sử dụng sai mục đích\n" +
                     "- Lái xe: Bảo trì, giữ nhật ký hành trình",
            Type: DocumentType.QuyDinh, Direction: Direction.NoiBo,
            Issuer: "Ban Giám đốc", Category: "Hành chính", NumberPrefix: "QyĐ",
            Tags: new[] { "Xe công", "Quy định", "Nội bộ" },
            Recipients: new[] { "- Toàn thể CBCCVC", "- Đội lái xe", "- Văn phòng Sở", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 43. Giấy nghỉ phép ──
        new(
            Title: "Giấy nghỉ phép của ông Hoàng Văn H",
            Subject: "Đơn xin nghỉ phép năm 05 ngày để về quê giải quyết việc gia đình",
            Content: "GIẤY NGHỈ PHÉP\n\n" +
                     "Kính gửi: Giám đốc Sở\n\n" +
                     "Họ tên: Hoàng Văn H\n" +
                     "Chức vụ: Chuyên viên, Phòng Quản lý Quy hoạch\n\n" +
                     "Xin nghỉ phép từ: 10/03/2026 đến 14/03/2026 (05 ngày)\n\n" +
                     "Lý do: Về quê giải quyết việc gia đình (bố ốm nặng)\n\n" +
                     "Số ngày phép còn lại: 12/15 ngày (năm 2026)\n\n" +
                     "Người bàn giao công việc: Chuyên viên Trần Thị I (cùng phòng)\n\n" +
                     "Kính đề nghị Giám đốc xem xét, cho phép.\n\n" +
                     "Ý kiến Trưởng phòng: Đồng ý, đã bố trí người thay thế.",
            Type: DocumentType.GiayNghiPhep, Direction: Direction.NoiBo,
            Issuer: "Phòng Quản lý Quy hoạch", Category: "Nhân sự", NumberPrefix: "GNP",
            Tags: new[] { "Nghỉ phép", "Nhân sự", "Nội bộ" },
            Recipients: new[] { "- Giám đốc Sở (phê duyệt)", "- Phòng TCHC (theo dõi)", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 44. Giấy ủy quyền ──
        new(
            Title: "Giấy ủy quyền ký văn bản",
            Subject: "Ủy quyền Phó Giám đốc ký thay văn bản trong thời gian Giám đốc đi công tác",
            Content: "GIẤY ỦY QUYỀN\n\n" +
                     "Tôi: Trần Văn B - Giám đốc Sở\n\n" +
                     "Ủy quyền cho: Ông Nguyễn Văn A - Phó Giám đốc\n\n" +
                     "Nội dung ủy quyền:\n" +
                     "- Ký thay các văn bản hành chính thông thường\n" +
                     "- Chủ trì các cuộc họp nội bộ\n" +
                     "- Giải quyết các công việc phát sinh thuộc thẩm quyền GĐ\n\n" +
                     "Thời hạn: Từ 15/04/2026 đến 22/04/2026\n\n" +
                     "Lý do: Giám đốc đi công tác tại Hà Nội theo giấy triệu tập " +
                     "của Bộ chủ quản.\n\n" +
                     "Ông Nguyễn Văn A chịu trách nhiệm về các quyết định trong phạm vi ủy quyền.",
            Type: DocumentType.GiayUyQuyen, Direction: Direction.NoiBo,
            Issuer: "Giám đốc Sở", Category: "Hành chính", NumberPrefix: "GUQ",
            Tags: new[] { "Ủy quyền", "Nội bộ", "Ký thay" },
            Recipients: new[] { "- PGĐ Nguyễn Văn A", "- Văn phòng Sở", "- Các phòng, ban", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 45. TB tuyển dụng ──
        new(
            Title: "Thông báo tuyển dụng công chức năm 2026",
            Subject: "Thông báo thi tuyển công chức vào làm việc tại Sở năm 2026",
            Content: "I. VỊ TRÍ TUYỂN DỤNG:\n" +
                     "1. Chuyên viên Phòng QLQH: 02 người (Kiến trúc sư/Kỹ sư XD)\n" +
                     "2. Chuyên viên Phòng CNTT: 01 người (Kỹ sư CNTT)\n" +
                     "3. Văn thư Văn phòng Sở: 01 người (CĐ/ĐH Văn thư - Lưu trữ)\n\n" +
                     "II. ĐIỀU KIỆN:\n" +
                     "- Tuổi: 18-35\n" +
                     "- Trình độ: Đại học trở lên (trừ vị trí văn thư)\n" +
                     "- Tin học: Chứng chỉ CNTT cơ bản trở lên\n" +
                     "- Ngoại ngữ: Chứng chỉ B1 trở lên\n\n" +
                     "III. HỒ SƠ:\n" +
                     "- Đơn dự tuyển, Sơ yếu lý lịch, Bằng cấp, CCCD\n" +
                     "- Nộp tại: Phòng TCHC, tầng 1\n\n" +
                     "IV. THỜI HẠN: Từ 01/04/2026 đến 30/04/2026",
            Type: DocumentType.ThongBao, Direction: Direction.NoiBo,
            Issuer: "Phòng Tổ chức - Hành chính", Category: "Nhân sự", NumberPrefix: "TB",
            Tags: new[] { "Tuyển dụng", "Nhân sự", "Năm 2026" },
            Recipients: new[] { "- Đăng công khai", "- Website cơ quan", "- Phòng TCHC (tiếp nhận HS)", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 46. Bản ghi nhớ ──
        new(
            Title: "Bản ghi nhớ cuộc họp với Sở Xây dựng",
            Subject: "Ghi nhớ nội dung thống nhất trong cuộc họp phối hợp với Sở Xây dựng ngày 25/02/2026",
            Content: "BẢN GHI NHỚ\n\n" +
                     "Cuộc họp phối hợp giữa Sở và Sở Xây dựng\n" +
                     "Thời gian: 14h00, ngày 25/02/2026\n\n" +
                     "HAI BÊN THỐNG NHẤT:\n\n" +
                     "1. Sở Xây dựng cung cấp dữ liệu quy hoạch theo định dạng GIS " +
                     "trước 15/03/2026\n\n" +
                     "2. Sở tích hợp dữ liệu vào hệ thống thông tin chuyên ngành " +
                     "và phản hồi trước 30/03/2026\n\n" +
                     "3. Hai bên cử đầu mối liên lạc:\n" +
                     "   - Sở: Ông Phạm Văn F - TP Phòng QLQH\n" +
                     "   - Sở XD: Bà Nguyễn Thị K - PTP Phòng QHKT\n\n" +
                     "4. Họp lại để đánh giá kết quả vào tháng 4/2026",
            Type: DocumentType.BanGhiNho, Direction: Direction.NoiBo,
            Issuer: "Văn phòng Sở", Category: "Hành chính", NumberPrefix: "BGN",
            Tags: new[] { "Ghi nhớ", "Phối hợp", "Sở Xây dựng" },
            Recipients: new[] { "- Sở Xây dựng (01 bản)", "- Phòng QLQH", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 47. Phiếu gửi hồ sơ ──
        new(
            Title: "Phiếu gửi hồ sơ sang Sở Nội vụ",
            Subject: "Gửi hồ sơ đề nghị nâng ngạch công chức năm 2026 sang Sở Nội vụ thẩm định",
            Content: "PHIẾU GỬI\n\n" +
                     "Kính gửi: Phòng CCVC, Sở Nội vụ\n\n" +
                     "Sở gửi kèm theo:\n\n" +
                     "1. Công văn đề nghị nâng ngạch công chức - 01 bản\n" +
                     "2. Danh sách công chức đủ điều kiện - 01 bản\n" +
                     "3. Hồ sơ cá nhân (05 người) - 05 bộ\n" +
                     "   Gồm: Bằng cấp, chứng chỉ, bản nhận xét, kết quả đánh giá 3 năm\n" +
                     "4. Bản sao Quyết định lương hiện hưởng - 05 bản\n\n" +
                     "Tổng cộng: 12 tài liệu\n\n" +
                     "Đề nghị Phòng CCVC tiếp nhận, thẩm định.",
            Type: DocumentType.PhieuGui, Direction: Direction.NoiBo,
            Issuer: "Phòng Tổ chức - Hành chính", Category: "Nhân sự", NumberPrefix: "PG",
            Tags: new[] { "Phiếu gửi", "Nâng ngạch", "Nhân sự" },
            Recipients: new[] { "- Phòng CCVC, Sở Nội vụ", "- Lưu: VT, TCCB" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 48. Thư công ──
        new(
            Title: "Thư cảm ơn đối tác hỗ trợ chuyển đổi số",
            Subject: "Thư cảm ơn Tập đoàn VNPT đã hỗ trợ triển khai hệ thống chính quyền điện tử",
            Content: "THƯ CẢM ƠN\n\n" +
                     "Kính gửi: Ban Giám đốc Tập đoàn VNPT\n\n" +
                     "Sở trân trọng cảm ơn Tập đoàn VNPT đã nhiệt tình hỗ trợ " +
                     "trong quá trình triển khai hệ thống chính quyền điện tử " +
                     "và chuyển đổi số tại Sở trong thời gian qua.\n\n" +
                     "Nhờ sự hỗ trợ của VNPT, Sở đã:\n" +
                     "- Triển khai thành công hệ thống quản lý văn bản điện tử\n" +
                     "- Ứng dụng chữ ký số cho 100% CBCCVC\n" +
                     "- Kết nối liên thông với Trục LGSP của tỉnh\n\n" +
                     "Hy vọng tiếp tục hợp tác chặt chẽ trong thời gian tới.\n\n" +
                     "Trân trọng.",
            Type: DocumentType.ThuCong, Direction: Direction.NoiBo,
            Issuer: "Giám đốc Sở", Category: "Hành chính", NumberPrefix: "ThC",
            Tags: new[] { "Thư cảm ơn", "CNTT", "Đối tác" },
            Recipients: new[] { "- Tập đoàn VNPT", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 49. Phiếu báo hỏng thiết bị ──
        new(
            Title: "Phiếu báo hỏng máy in Phòng Thanh tra",
            Subject: "Báo hỏng máy in HP LaserJet Pro M404dn tại Phòng Thanh tra, đề nghị sửa chữa",
            Content: "PHIẾU BÁO\n\n" +
                     "Đơn vị: Phòng Thanh tra\n" +
                     "Người báo: Đ/c Võ Thị L - Thanh tra viên\n\n" +
                     "THIẾT BỊ HỎNG:\n" +
                     "- Tên: Máy in HP LaserJet Pro M404dn\n" +
                     "- Mã tài sản: TS-2024-058\n" +
                     "- Vị trí: Phòng 305, tầng 3\n\n" +
                     "TÌNH TRẠNG:\n" +
                     "- Máy kéo giấy bị kẹt liên tục\n" +
                     "- In ra bị mờ, sọc đen dọc trang\n" +
                     "- Đã thử thay hộp mực, vẫn không khắc phục\n\n" +
                     "Ngày phát hiện: 20/02/2026\n" +
                     "Mức độ ảnh hưởng: Không in được biên bản thanh tra\n\n" +
                     "Đề nghị Phòng CNTT kiểm tra, sửa chữa hoặc thay thế.",
            Type: DocumentType.PhieuBao, Direction: Direction.NoiBo,
            Issuer: "Phòng Thanh tra", Category: "Kỹ thuật", NumberPrefix: "PB",
            Tags: new[] { "Phiếu báo", "Thiết bị", "Sửa chữa" },
            Recipients: new[] { "- Phòng CNTT (xử lý)", "- Phòng TCKT (theo dõi tài sản)", "- Lưu: VT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Thuong
        ),

        // ── 50. Quy định bảo mật thông tin ──
        new(
            Title: "Quy định về bảo mật thông tin và an toàn mạng",
            Subject: "Quy định bảo mật thông tin, an toàn hệ thống mạng và dữ liệu cơ quan",
            Content: "Điều 1. Phạm vi: Áp dụng cho toàn bộ CBCCVC sử dụng hệ thống CNTT.\n\n" +
                     "Điều 2. Quản lý tài khoản:\n" +
                     "- Mỗi người 01 tài khoản riêng, không chia sẻ mật khẩu\n" +
                     "- Đổi mật khẩu tối thiểu 90 ngày/lần\n" +
                     "- Mật khẩu tối thiểu 8 ký tự, có chữ hoa, số, ký tự đặc biệt\n\n" +
                     "Điều 3. Sử dụng email:\n" +
                     "- Chỉ gửi văn bản mật qua hệ thống mã hóa\n" +
                     "- Không mở file đính kèm từ email lạ\n" +
                     "- Không sử dụng email cá nhân cho công việc cơ quan\n\n" +
                     "Điều 4. Sao lưu dữ liệu:\n" +
                     "- Phòng CNTT sao lưu máy chủ hằng ngày\n" +
                     "- CBCCVC sao lưu dữ liệu cá nhân hằng tuần\n\n" +
                     "Điều 5. Xử lý vi phạm:\n" +
                     "- Vi phạm lần 1: Nhắc nhở\n" +
                     "- Vi phạm lần 2: Khiển trách\n" +
                     "- Gây hậu quả nghiêm trọng: Xử lý theo pháp luật",
            Type: DocumentType.QuyDinh, Direction: Direction.NoiBo,
            Issuer: "Ban Giám đốc", Category: "Kỹ thuật", NumberPrefix: "QyĐ",
            Tags: new[] { "Bảo mật", "An toàn mạng", "CNTT" },
            Recipients: new[] { "- Toàn thể CBCCVC", "- Phòng CNTT (giám sát)", "- Lưu: VT, CNTT" },
            Urgency: UrgencyLevel.Thuong, Security: SecurityLevel.Mat
        ),
    };

    /// <summary>
    /// Tạo văn bản demo nhất quán (Title ↔ Subject ↔ Content ↔ Type ↔ Direction ↔ Issuer).
    /// Mặc định tạo 50 mẫu — phủ 25+ loại VB, nhiều phòng ban.
    /// </summary>
    public List<Document> GenerateDemoDocuments(int count = 50)
    {
        var documents = new List<Document>();
        var templates = GetTemplates();

        for (int i = 0; i < count; i++)
        {
            var tmpl = templates[i % templates.Length];
            // Phân bố ngày ban hành đều trong 12 tháng qua, giảm dần (gần đây nhiều hơn)
            var daysAgo = _rng.Next(1, 30) + (i * 7 % 365);
            var issueDate = DateTime.Now.AddDays(-Math.Min(daysAgo, 365));
            var numberSeq = i + 1;

            // Tạo số VB đúng format: Số/KýHiệu-CơQuan — Theo Phụ lục VI, NĐ 30/2020
            var orgAbbr = GetOrgAbbreviation(tmpl.Issuer);
            var number = $"{numberSeq:000}/{tmpl.NumberPrefix}-{orgAbbr}";

            var doc = new Document
            {
                Number = number,
                Title = tmpl.Title,
                Subject = tmpl.Subject,
                Content = tmpl.Content,
                Issuer = tmpl.Issuer,
                IssueDate = issueDate,
                Type = tmpl.Type,
                Direction = tmpl.Direction,
                Category = tmpl.Category,
                UrgencyLevel = tmpl.Urgency,
                SecurityLevel = tmpl.Security,
                Status = "Còn hiệu lực",
                Tags = tmpl.Tags,
                Recipients = tmpl.Recipients ?? Array.Empty<string>(),
                CreatedBy = "Admin",
                CreatedDate = issueDate,
                ModifiedBy = "Admin",
                ModifiedDate = issueDate
            };

            // ── VĂN BẢN ĐI: workflow + ký duyệt ──
            if (doc.Direction == Direction.Di)
            {
                // VB đi thường ở trạng thái đã phát hành
                var statusPool = new[] { DocumentStatus.Published, DocumentStatus.Published,
                                          DocumentStatus.Signed, DocumentStatus.PendingApproval };
                doc.WorkflowStatus = statusPool[_rng.Next(statusPool.Length)];

                if (doc.WorkflowStatus >= DocumentStatus.Signed)
                {
                    doc.SignedBy = "Giám đốc Trần Văn B";
                    doc.SignedDate = doc.IssueDate.AddDays(-1);
                }
                if (doc.WorkflowStatus == DocumentStatus.Published)
                {
                    doc.PublishedBy = "Văn thư Nguyễn Thị D";
                    doc.PublishedDate = doc.IssueDate;
                }
            }

            // ── VĂN BẢN ĐẾN: ngày đến + hạn xử lý (test cảnh báo deadline) ──
            if (doc.Direction == Direction.Den)
            {
                doc.ArrivalDate = doc.IssueDate.AddDays(_rng.Next(1, 4));
                doc.WorkflowStatus = DocumentStatus.Draft; // Chưa xử lý

                // Phân bổ đa dạng kịch bản deadline
                switch (i % 7)
                {
                    case 0: // Quá hạn nặng (5-10 ngày)
                        doc.DueDate = DateTime.Today.AddDays(-_rng.Next(5, 11));
                        break;
                    case 1: // Quá hạn nhẹ (1-3 ngày)
                        doc.DueDate = DateTime.Today.AddDays(-_rng.Next(1, 4));
                        break;
                    case 2: // Hết hạn hôm nay
                        doc.DueDate = DateTime.Today;
                        break;
                    case 3: // Sắp hết hạn (1-2 ngày tới)
                        doc.DueDate = DateTime.Today.AddDays(_rng.Next(1, 3));
                        break;
                    case 4: // Sắp hết hạn (3-5 ngày tới)
                        doc.DueDate = DateTime.Today.AddDays(_rng.Next(3, 6));
                        break;
                    case 5: // Còn nhiều thời gian (7-20 ngày)
                        doc.DueDate = DateTime.Today.AddDays(_rng.Next(7, 21));
                        break;
                    case 6: // Rất nhiều thời gian (21-60 ngày)
                        doc.DueDate = DateTime.Today.AddDays(_rng.Next(21, 61));
                        break;
                }

                // 25% VB đến đã xử lý xong → không cảnh báo
                if (_rng.Next(4) == 0)
                {
                    doc.WorkflowStatus = DocumentStatus.Archived;
                    doc.Status = "Đã xử lý";
                }
            }

            // ── VĂN BẢN NỘI BỘ ──
            if (doc.Direction == Direction.NoiBo)
            {
                doc.WorkflowStatus = DocumentStatus.Published;
                doc.SignedBy = "Giám đốc Trần Văn B";
                doc.SignedDate = doc.IssueDate;
                doc.PublishedDate = doc.IssueDate;
            }

            var created = _documentService.AddDocument(doc);
            documents.Add(created);
        }

        return documents;
    }

    /// <summary>
    /// Tạo viết tắt tên cơ quan cho số hiệu VB — Theo Phụ lục VI, NĐ 30/2020
    /// </summary>
    private static string GetOrgAbbreviation(string issuer) => issuer switch
    {
        "UBND Tỉnh" => "UBND",
        "Chính phủ" => "CP",
        "Quốc hội" => "QH",
        "HĐND Tỉnh" => "HĐND",
        "Hội nghị CBCC" => "HNCC",
        "Bộ Tài chính" => "BTC",
        "Bộ Nội vụ" => "BNV",
        "Sở Nội vụ" => "SNV",
        "Sở Tài chính" => "STC",
        "Sở Kế hoạch và Đầu tư" => "SKHĐT",
        "Sở Y tế" => "SYT",
        "Sở Giáo dục và Đào tạo" => "SGDĐT",
        "Sở Tài nguyên và Môi trường" => "STNMT",
        "Sở Thông tin và Truyền thông" => "STTTT",
        "Sở Xây dựng" => "SXD",
        "Cảnh sát PCCC Tỉnh" => "PCCC",
        "Giám đốc Sở" => "Sở",
        "Ban Giám đốc" => "Sở",
        "Văn phòng Sở" => "VP",
        "Chánh Văn phòng Sở" => "VP",
        "Phòng Công nghệ thông tin" => "CNTT",
        "Phòng Tổ chức - Hành chính" => "TCHC",
        "Phòng Tài chính - Kế toán" => "TCKT",
        "Phòng Quản lý Quy hoạch" => "QLQH",
        "Phòng Thanh tra" => "TTr",
        _ => issuer.Split(' ')[0]
    };
}
