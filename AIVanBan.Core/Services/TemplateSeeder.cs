using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service tạo dữ liệu mẫu template văn bản
/// </summary>
public class TemplateSeeder
{
    private readonly DocumentService _documentService;

    public TemplateSeeder(DocumentService documentService)
    {
        _documentService = documentService;
    }

    /// <summary>
    /// Khởi tạo các template mẫu mặc định
    /// </summary>
    public void SeedDefaultTemplates()
    {
        List<DocumentTemplate> existingTemplates;
        try
        {
            existingTemplates = _documentService.GetAllTemplates();
        }
        catch (Exception ex)
        {
            // Nếu LiteDB không deserialize được (VD: enum value cũ không tồn tại),
            // xóa collection templates cũ và seed lại từ đầu
            Console.WriteLine($"⚠️ Error loading existing templates: {ex.Message}");
            Console.WriteLine("🔄 Dropping corrupted templates collection and re-seeding...");
            _documentService.DropTemplatesCollection();
            existingTemplates = new List<DocumentTemplate>();
        }
        
        // Nếu đã có template rồi thì không seed nữa
        if (existingTemplates.Count > 0)
        {
            Console.WriteLine($"✅ Found {existingTemplates.Count} existing templates. Skip seeding.");
            return;
        }

        Console.WriteLine("📝 Seeding default document templates...");

        var templates = new List<DocumentTemplate>
        {
            // === CÔNG VĂN ===
            CreateCongVanTemplate(),
            CreateCongVanGuiSoBanNganhTemplate(),
            CreateCongVanGuiCapTrenTemplate(),
            CreateCongVanTraLoiTemplate(),
            
            // === QUYẾT ĐỊNH ===
            CreateQuyetDinhDieuDongTemplate(),
            CreateQuyetDinhKhenThuongTemplate(),
            CreateQuyetDinhThanhLapTemplate(),
            CreateQuyetDinhPheTemplate(),
            
            // === BÁO CÁO ===
            CreateBaoCaoTongKetTemplate(),
            CreateBaoCaoTinhHinhTemplate(),
            CreateBaoCaoKetQuaTemplate(),
            
            // === TỜ TRÌNH ===
            CreateToTrinhXinYKienTemplate(),
            CreateToTrinhDeXuatTemplate(),
            
            // === KẾ HOẠCH ===
            CreateKeHoachCongTacTemplate(),
            CreateKeHoachToChucTemplate(),
            
            // === THÔNG BÁO ===
            CreateThongBaoHoiNghiTemplate(),
            CreateThongBaoKetQuaTemplate(),
            
            // === NGHỊ QUYẾT ===
            CreateNghiQuyetHDNDTemplate(),
            CreateNghiQuyetUBNDTemplate(),
            
            // === 15 LOẠI VB BỔ SUNG — NĐ 30/2020 ===
            CreateChiThiTemplate(),
            CreateQuyChETemplate(),
            CreateQuyDinhTemplate(),
            CreateThongCaoTemplate(),
            CreateHuongDanTemplate(),
            CreateChuongTrinhTemplate(),
            CreatePhuongAnTemplate(),
            CreateDeAnTemplate(),
            CreateDuAnTemplate(),
            CreateBienBanTemplate(),
            CreateHopDongTemplate(),
            CreateCongDienTemplate(),
            CreateBanGhiNhoTemplate(),
            CreateBanThoaThuanTemplate(),
            CreateGiayUyQuyenTemplate(),
            CreateGiayMoiTemplate(),
            CreateGiayGioiThieuTemplate(),
            CreateGiayNghiPhepTemplate(),
            CreatePhieuGuiTemplate(),
            CreatePhieuChuyenTemplate(),
            CreatePhieuBaoTemplate(),
            CreateThuCongTemplate(),
            
            // === BẢN SAO VĂN BẢN — Mẫu 3.1, Phụ lục III, NĐ 30/2020 ===
            CreateSaoYTemplate(),
            CreateSaoLucTemplate(),
            CreateTrichSaoTemplate(),
            
            // === PHỤ LỤC VĂN BẢN — Mẫu 2.1, Phụ lục III, NĐ 30/2020 ===
            CreatePhuLucVanBanTemplate(),
        };

        foreach (var template in templates)
        {
            _documentService.AddTemplate(template);
            Console.WriteLine($"  ✓ Added: {template.Name}");
        }

        Console.WriteLine($"✅ Seeded {templates.Count} default templates successfully!");
    }

    #region Công văn Templates

    private DocumentTemplate CreateCongVanTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Công văn chung",
            Type = DocumentType.CongVan,
            Category = "Hành chính",
            Description = "Mẫu công văn chung để gửi đi các cơ quan, đơn vị",
            TemplateContent = @"
[TÊN CƠ QUAN CẤP TRÊN]
[TÊN ĐƠN VỊ]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /CV-[Viết tắt]
V/v: [Vấn đề công văn]

[Địa danh], ngày [  ] tháng [  ] năm 202[  ]

Kính gửi: [Cơ quan nhận]

[Nội dung công văn]

Nơi nhận:                                      [CHỨC DANH]
- Như trên;
- Lưu VT.                                      [Chữ ký, đóng dấu]

                                               [Họ và tên]
",
            AIPrompt = @"Viết công văn:
- Cơ quan gửi: {from_org}
- Cơ quan nhận: {to_org}
- Vấn đề: {subject}
- Nội dung: {content}
- Người ký: {signer_name}, {signer_title}",
            RequiredFields = new[] { "from_org", "to_org", "subject", "content", "signer_name", "signer_title" },
            Tags = new[] { "công văn", "hành chính" }
        };
    }

    private DocumentTemplate CreateCongVanGuiSoBanNganhTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Công văn gửi Sở/Ban/Ngành",
            Type = DocumentType.CongVan,
            Category = "Hành chính",
            Description = "Công văn từ UBND cấp xã/phường gửi các Sở, Ban, Ngành cấp tỉnh",
            TemplateContent = @"
ỦY BAN NHÂN DÂN
[CẤP XÃ/PHƯỜNG]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /CV-UBND
V/v: [Vấn đề]

[Địa danh], ngày [  ] tháng [  ] năm 202[  ]

Kính gửi: [Sở/Ban/Ngành]

[Nội dung công văn]

Nơi nhận:                                      CHỦ TỊCH UBND
- Như trên;
- Lưu VT.                                      [Chữ ký, đóng dấu]

                                               [Họ và tên]
",
            AIPrompt = @"Viết công văn gửi Sở/Ban/Ngành:
- Tên đơn vị gửi: {from_org}
- Sở/Ban/Ngành nhận: {to_department}
- Vấn đề: {subject}
- Nội dung: {content}
- Chủ tịch: {chairman_name}",
            RequiredFields = new[] { "from_org", "to_department", "subject", "content", "chairman_name" },
            Tags = new[] { "công văn", "UBND", "Sở Ban Ngành" }
        };
    }

    private DocumentTemplate CreateCongVanGuiCapTrenTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Công văn báo cáo cấp trên",
            Type = DocumentType.CongVan,
            Category = "Hành chính",
            Description = "Công văn báo cáo, đề xuất với cấp trên",
            TemplateContent = @"
[TÊN ĐƠN VỊ]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /CV-[Viết tắt]
V/v: Báo cáo [vấn đề]

[Địa danh], ngày [  ] tháng [  ] năm 202[  ]

Kính gửi: [Cơ quan cấp trên]

[Nội dung báo cáo]

Đề nghị [cơ quan cấp trên] xem xét, chỉ đạo./.

Nơi nhận:                                      [CHỨC DANH]
- Như trên;
- Lưu VT.                                      [Chữ ký, đóng dấu]

                                               [Họ và tên]
",
            AIPrompt = @"Viết công văn báo cáo cấp trên:
- Đơn vị báo cáo: {from_org}
- Cấp trên: {to_org}
- Vấn đề: {subject}
- Nội dung báo cáo: {content}
- Đề xuất: {proposal}
- Người ký: {signer_name}, {signer_title}",
            RequiredFields = new[] { "from_org", "to_org", "subject", "content", "proposal", "signer_name", "signer_title" },
            Tags = new[] { "công văn", "báo cáo", "cấp trên" }
        };
    }

    private DocumentTemplate CreateCongVanTraLoiTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Công văn trả lời",
            Type = DocumentType.CongVan,
            Category = "Hành chính",
            Description = "Công văn trả lời, phản hồi công văn khác",
            TemplateContent = @"
[TÊN ĐƠN VỊ]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /CV-[Viết tắt]
V/v: Trả lời Công văn số [số CV]

[Địa danh], ngày [  ] tháng [  ] năm 202[  ]

Kính gửi: [Cơ quan nhận]

Trả lời Công văn số [số] ngày [  ] của [cơ quan], về vấn đề [vấn đề], [tên đơn vị] xin trả lời như sau:

[Nội dung trả lời]

Nơi nhận:                                      [CHỨC DANH]
- Như trên;
- Lưu VT.                                      [Chữ ký, đóng dấu]

                                               [Họ và tên]
",
            AIPrompt = @"Viết công văn trả lời:
- Đơn vị gửi: {from_org}
- Đơn vị nhận: {to_org}
- Trả lời công văn số: {reply_to_number}
- Vấn đề: {subject}
- Nội dung trả lời: {content}
- Người ký: {signer_name}, {signer_title}",
            RequiredFields = new[] { "from_org", "to_org", "reply_to_number", "subject", "content", "signer_name", "signer_title" },
            Tags = new[] { "công văn", "trả lời" }
        };
    }

    #endregion

    #region Quyết định Templates

    private DocumentTemplate CreateQuyetDinhDieuDongTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Quyết định điều động cán bộ",
            Type = DocumentType.QuyetDinh,
            Category = "Tổ chức - Cán bộ",
            Description = "Quyết định điều động, luân chuyển cán bộ",
            TemplateContent = @"
[TÊN CƠ QUAN]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /QĐ-[Viết tắt]

QUYẾT ĐỊNH
Về việc điều động cán bộ

[CHỨC DANH NGƯỜI KÝ]

Căn cứ [Luật, Nghị định liên quan];
Xét đề nghị của [đơn vị đề xuất];

QUYẾT ĐỊNH:

Điều 1. Điều động Ông/Bà [Họ tên], sinh năm [  ], chức vụ [  ] tại [đơn vị cũ], về công tác tại [đơn vị mới], giữ chức vụ [chức vụ mới], kể từ ngày [  ] tháng [  ] năm [  ].

Điều 2. [Đơn vị cũ] và [Đơn vị mới] có trách nhiệm thi hành Quyết định này.

Điều 3. Quyết định này có hiệu lực kể từ ngày ký.

Nơi nhận:                                      [CHỨC DANH]
- Như Điều 2;
- Lưu VT.                                      [Chữ ký, đóng dấu]

                                               [Họ và tên]
",
            AIPrompt = @"Tạo quyết định điều động:
- Họ tên cán bộ: {person_name}
- Chức vụ hiện tại: {current_position}
- Đơn vị cũ: {from_unit}
- Đơn vị mới: {to_unit}
- Chức vụ mới: {new_position}
- Ngày hiệu lực: {effective_date}
- Người ký: {signer_name}, {signer_title}",
            RequiredFields = new[] { "person_name", "current_position", "from_unit", "to_unit", "new_position", "effective_date", "signer_name", "signer_title" },
            Tags = new[] { "quyết định", "điều động", "cán bộ" }
        };
    }

    private DocumentTemplate CreateQuyetDinhKhenThuongTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Quyết định khen thưởng",
            Type = DocumentType.QuyetDinh,
            Category = "Thi đua - Khen thưởng",
            Description = "Quyết định khen thưởng cá nhân, tập thể",
            TemplateContent = @"
[TÊN CƠ QUAN]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /QĐ-[Viết tắt]

QUYẾT ĐỊNH
Về việc khen thưởng

[CHỨC DANH NGƯỜI KÝ]

Căn cứ Luật Thi đua, Khen thưởng;
Xét thành tích của [cá nhân/tập thể];

QUYẾT ĐỊNH:

Điều 1. Tặng [Hình thức khen thưởng] cho [Cá nhân/Tập thể]:

[Danh sách khen thưởng]

Vì đã có thành tích [nội dung thành tích].

Điều 2. Quyết định này có hiệu lực kể từ ngày ký.

Nơi nhận:                                      [CHỨC DANH]
- Như Điều 1;
- Lưu VT.                                      [Chữ ký, đóng dấu]

                                               [Họ và tên]
",
            AIPrompt = @"Tạo quyết định khen thưởng:
- Hình thức khen thưởng: {award_type}
- Đối tượng: {recipient}
- Thành tích: {achievement}
- Người ký: {signer_name}, {signer_title}",
            RequiredFields = new[] { "award_type", "recipient", "achievement", "signer_name", "signer_title" },
            Tags = new[] { "quyết định", "khen thưởng" }
        };
    }

    private DocumentTemplate CreateQuyetDinhThanhLapTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Quyết định thành lập tổ chức",
            Type = DocumentType.QuyetDinh,
            Category = "Tổ chức",
            Description = "Quyết định thành lập Ban, Hội đồng, Tổ công tác",
            TemplateContent = @"
[TÊN CƠ QUAN]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /QĐ-[Viết tắt]

QUYẾT ĐỊNH
Về việc thành lập [Tên tổ chức]

[CHỨC DANH NGƯỜI KÝ]

Căn cứ [Luật, Nghị định liên quan];
Xét sự cần thiết thành lập [tổ chức];

QUYẾT ĐỊNH:

Điều 1. Thành lập [Tên tổ chức đầy đủ] gồm các thành viên sau:

1. [Họ tên] - [Chức vụ] - [Vai trò trong tổ chức]
2. [Họ tên] - [Chức vụ] - [Vai trò trong tổ chức]
[...]

Điều 2. [Tên tổ chức] có nhiệm vụ:
- [Nhiệm vụ 1]
- [Nhiệm vụ 2]
[...]

Điều 3. Quyết định này có hiệu lực kể từ ngày ký.

Nơi nhận:                                      [CHỨC DANH]
- Như Điều 1;
- Lưu VT.                                      [Chữ ký, đóng dấu]

                                               [Họ và tên]
",
            AIPrompt = @"Tạo quyết định thành lập tổ chức:
- Tên tổ chức: {org_name}
- Danh sách thành viên: {members}
- Nhiệm vụ: {tasks}
- Người ký: {signer_name}, {signer_title}",
            RequiredFields = new[] { "org_name", "members", "tasks", "signer_name", "signer_title" },
            Tags = new[] { "quyết định", "thành lập" }
        };
    }

    private DocumentTemplate CreateQuyetDinhPheTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Quyết định phê duyệt",
            Type = DocumentType.QuyetDinh,
            Category = "Hành chính",
            Description = "Quyết định phê duyệt đề án, dự án, kế hoạch",
            TemplateContent = @"
[TÊN CƠ QUAN]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /QĐ-[Viết tắt]

QUYẾT ĐỊNH
Về việc phê duyệt [Tên đề án/dự án]

[CHỨC DANH NGƯỜI KÝ]

Căn cứ [Luật, Nghị định liên quan];
Xét đề nghị của [đơn vị trình];

QUYẾT ĐỊNH:

Điều 1. Phê duyệt [Tên đề án/dự án đầy đủ] với các nội dung chính sau:

1. Mục tiêu: [Mục tiêu]
2. Phạm vi: [Phạm vi]
3. Kinh phí: [Kinh phí] từ nguồn [nguồn]
4. Thời gian thực hiện: [Thời gian]

Điều 2. Giao [đơn vị] chủ trì tổ chức thực hiện.

Điều 3. Quyết định này có hiệu lực kể từ ngày ký.

Nơi nhận:                                      [CHỨC DANH]
- Như Điều 2;
- Lưu VT.                                      [Chữ ký, đóng dấu]

                                               [Họ và tên]
",
            AIPrompt = @"Tạo quyết định phê duyệt:
- Tên đề án/dự án: {project_name}
- Mục tiêu: {objectives}
- Kinh phí: {budget}
- Đơn vị thực hiện: {implementing_unit}
- Người ký: {signer_name}, {signer_title}",
            RequiredFields = new[] { "project_name", "objectives", "budget", "implementing_unit", "signer_name", "signer_title" },
            Tags = new[] { "quyết định", "phê duyệt" }
        };
    }

    #endregion

    #region Báo cáo Templates

    private DocumentTemplate CreateBaoCaoTongKetTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Báo cáo tổng kết",
            Type = DocumentType.BaoCao,
            Category = "Hành chính",
            Description = "Báo cáo tổng kết công tác năm, quý",
            TemplateContent = @"
[TÊN ĐƠN VỊ]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

BÁO CÁO
Tổng kết công tác [năm/quý]

Kính gửi: [Cơ quan cấp trên]

I. KẾT QUẢ ĐẠT ĐƯỢC

[Nội dung kết quả]

II. TỒN TẠI, HẠN CHẾ

[Nội dung hạn chế]

III. NGUYÊN NHÂN

[Phân tích nguyên nhân]

IV. PHƯƠNG HƯỚNG, NHIỆM VỤ TIẾP THEO

[Kế hoạch tiếp theo]

Trên đây là báo cáo tổng kết của [đơn vị], kính trình [cơ quan cấp trên] xem xét./.


                                               [CHỨC DANH]

                                               [Chữ ký]

                                               [Họ và tên]
",
            AIPrompt = @"Tạo báo cáo tổng kết:
- Đơn vị báo cáo: {org_name}
- Kỳ báo cáo: {period}
- Kết quả đạt được: {achievements}
- Tồn tại: {challenges}
- Phương hướng tiếp theo: {future_plans}
- Người ký: {signer_name}, {signer_title}",
            RequiredFields = new[] { "org_name", "period", "achievements", "challenges", "future_plans", "signer_name", "signer_title" },
            Tags = new[] { "báo cáo", "tổng kết" }
        };
    }

    private DocumentTemplate CreateBaoCaoTinhHinhTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Báo cáo tình hình",
            Type = DocumentType.BaoCao,
            Category = "Hành chính",
            Description = "Báo cáo tình hình công tác định kỳ",
            TemplateContent = @"
[TÊN ĐƠN VỊ]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /BC-[Viết tắt]
V/v: Báo cáo tình hình [lĩnh vực]

[Địa danh], ngày [  ] tháng [  ] năm 202[  ]

Kính gửi: [Cơ quan cấp trên]

[Tên đơn vị] báo cáo tình hình [lĩnh vực] như sau:

I. TÌNH HÌNH CHUNG

[Mô tả tình hình]

II. CÔNG VIỆC ĐÃ TRIỂN KHAI

[Các hoạt động đã thực hiện]

III. KẾT QUẢ ĐẠT ĐƯỢC

[Kết quả cụ thể]

IV. KHÓ KHĂN, VẬN ĐỀ

[Những khó khăn]

V. ĐỀ XUẤT, KIẾN NGHỊ

[Đề xuất giải pháp]

Trên đây là báo cáo của [đơn vị], kính trình [cơ quan] xem xét./.


                                               [CHỨC DANH]

                                               [Chữ ký]

                                               [Họ và tên]
",
            AIPrompt = @"Tạo báo cáo tình hình:
- Đơn vị báo cáo: {org_name}
- Lĩnh vực: {field}
- Tình hình: {situation}
- Kết quả: {results}
- Đề xuất: {proposals}
- Người ký: {signer_name}, {signer_title}",
            RequiredFields = new[] { "org_name", "field", "situation", "results", "proposals", "signer_name", "signer_title" },
            Tags = new[] { "báo cáo", "tình hình" }
        };
    }

    private DocumentTemplate CreateBaoCaoKetQuaTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Báo cáo kết quả thực hiện",
            Type = DocumentType.BaoCao,
            Category = "Hành chính",
            Description = "Báo cáo kết quả thực hiện nhiệm vụ, kế hoạch",
            TemplateContent = @"
[TÊN ĐƠN VỊ]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

BÁO CÁO
Kết quả thực hiện [nhiệm vụ/kế hoạch]

Kính gửi: [Cơ quan cấp trên]

Thực hiện [Kế hoạch/Chỉ thị số...], [Tên đơn vị] báo cáo kết quả như sau:

I. TRIỂN KHAI THỰC HIỆN

[Các bước đã thực hiện]

II. KẾT QUẢ ĐẠT ĐƯỢC

1. Về [lĩnh vực 1]: [kết quả]
2. Về [lĩnh vực 2]: [kết quả]

III. ĐÁNH GIÁ

[Đánh giá chung]

IV. ĐỀ XUẤT

[Kiến nghị, đề xuất]

Trên đây là báo cáo của [đơn vị], kính trình [cơ quan] biết./.


                                               [CHỨC DANH]

                                               [Chữ ký]

                                               [Họ và tên]
",
            AIPrompt = @"Tạo báo cáo kết quả:
- Đơn vị báo cáo: {org_name}
- Nhiệm vụ/Kế hoạch: {task_name}
- Kết quả: {results}
- Đánh giá: {evaluation}
- Đề xuất: {proposals}
- Người ký: {signer_name}, {signer_title}",
            RequiredFields = new[] { "org_name", "task_name", "results", "evaluation", "proposals", "signer_name", "signer_title" },
            Tags = new[] { "báo cáo", "kết quả" }
        };
    }

    #endregion

    #region Tờ trình Templates

    private DocumentTemplate CreateToTrinhXinYKienTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Tờ trình xin ý kiến",
            Type = DocumentType.ToTrinh,
            Category = "Hành chính",
            Description = "Tờ trình xin ý kiến chỉ đạo",
            TemplateContent = @"
[TÊN ĐƠN VỊ]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /TTr-[Viết tắt]

TỜ TRÌNH
Xin ý kiến về [vấn đề]

Kính gửi: [Cấp trên]

Căn cứ [văn bản liên quan];
[Tên đơn vị] kính trình [cấp trên] xem xét, cho ý kiến về nội dung sau:

I. SỰ CẦN THIẾT

[Lý do cần xin ý kiến]

II. NỘI DUNG CẦN XIN Ý KIẾN

[Nội dung cụ thể]

III. ĐỀ XUẤT

[Tên đơn vị] kính đề nghị [cấp trên] xem xét, cho ý kiến về vấn đề trên./.


Nơi nhận:                                      [CHỨC DANH]
- Như trên;
- Lưu VT.                                      [Chữ ký]

                                               [Họ và tên]
",
            AIPrompt = @"Tạo tờ trình xin ý kiến:
- Đơn vị trình: {org_name}
- Cấp trên: {recipient}
- Vấn đề: {subject}
- Lý do: {reason}
- Nội dung: {content}
- Người ký: {signer_name}, {signer_title}",
            RequiredFields = new[] { "org_name", "recipient", "subject", "reason", "content", "signer_name", "signer_title" },
            Tags = new[] { "tờ trình", "xin ý kiến" }
        };
    }

    private DocumentTemplate CreateToTrinhDeXuatTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Tờ trình đề xuất",
            Type = DocumentType.ToTrinh,
            Category = "Hành chính",
            Description = "Tờ trình đề xuất phương án, giải pháp",
            TemplateContent = @"
[TÊN ĐƠN VỊ]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /TTr-[Viết tắt]

TỜ TRÌNH
Về việc [nội dung đề xuất]

Kính gửi: [Cấp trên]

Căn cứ [văn bản liên quan];
[Tên đơn vị] kính trình [cấp trên] xem xét, phê duyệt nội dung sau:

I. CĂN CỨ, LÝ DO

[Lý do đề xuất]

II. NỘI DUNG ĐỀ XUẤT

1. [Nội dung 1]
2. [Nội dung 2]

III. Dự TOÁN KINH PHÍ

Tổng kinh phí: [số tiền]
Nguồn kinh phí: [nguồn]

IV. ĐỀ NGHỊ

[Tên đơn vị] kính đề nghị [cấp trên] xem xét, phê duyệt./.


Nơi nhận:                                      [CHỨC DANH]
- Như trên;
- Lưu VT.                                      [Chữ ký]

                                               [Họ và tên]
",
            AIPrompt = @"Tạo tờ trình đề xuất:
- Đơn vị trình: {org_name}
- Cấp trên: {recipient}
- Nội dung đề xuất: {proposal}
- Lý do: {reason}
- Kinh phí: {budget}
- Người ký: {signer_name}, {signer_title}",
            RequiredFields = new[] { "org_name", "recipient", "proposal", "reason", "budget", "signer_name", "signer_title" },
            Tags = new[] { "tờ trình", "đề xuất" }
        };
    }

    #endregion

    #region Kế hoạch Templates

    private DocumentTemplate CreateKeHoachCongTacTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Kế hoạch công tác",
            Type = DocumentType.KeHoach,
            Category = "Hành chính",
            Description = "Kế hoạch công tác năm, quý, tháng",
            TemplateContent = @"
[TÊN ĐƠN VỊ]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /KH-[Viết tắt]

KẾ HOẠCH
Công tác [năm/quý/tháng]

I. MỤC ĐÍCH, YÊU CẦU

[Mục đích của kế hoạch]

II. NỘI DUNG CÔNG VIỆC

1. [Công việc 1]
   - Thời gian: [thời gian]
   - Đơn vị thực hiện: [đơn vị]
   - Kết quả: [kết quả mong đợi]

2. [Công việc 2]
   [...]

III. TỔ CHỨC THỰC HIỆN

[Phân công cụ thể]


                                               [CHỨC DANH]

                                               [Chữ ký, đóng dấu]

                                               [Họ và tên]
",
            AIPrompt = @"Tạo kế hoạch công tác:
- Đơn vị: {org_name}
- Kỳ kế hoạch: {period}
- Mục đích: {objectives}
- Các công việc: {tasks}
- Người ký: {signer_name}, {signer_title}",
            RequiredFields = new[] { "org_name", "period", "objectives", "tasks", "signer_name", "signer_title" },
            Tags = new[] { "kế hoạch", "công tác" }
        };
    }

    private DocumentTemplate CreateKeHoachToChucTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Kế hoạch tổ chức sự kiện",
            Type = DocumentType.KeHoach,
            Category = "Hành chính",
            Description = "Kế hoạch tổ chức hội nghị, lễ hội, sự kiện",
            TemplateContent = @"
[TÊN ĐƠN VỊ]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /KH-[Viết tắt]

KẾ HOẠCH
Tổ chức [tên sự kiện]

I. MỤC ĐÍCH

[Mục đích tổ chức]

II. THỜI GIAN, ĐỊA ĐIỂM

- Thời gian: [ngày, giờ]
- Địa điểm: [địa điểm]
- Thành phần: [người tham dự]

III. NỘI DUNG CHƯƠNG TRÌNH

[Nội dung chi tiết]

IV. PHÂN CÔNG NHIỆM VỤ

[Phân công cụ thể]

V. KINH PHÍ

Tổng kinh phí: [số tiền]
Nguồn: [nguồn kinh phí]


                                               [CHỨC DANH]

                                               [Chữ ký, đóng dấu]

                                               [Họ và tên]
",
            AIPrompt = @"Tạo kế hoạch tổ chức sự kiện:
- Tên sự kiện: {event_name}
- Thời gian, địa điểm: {time_place}
- Mục đích: {purpose}
- Nội dung chương trình: {program}
- Kinh phí: {budget}
- Người ký: {signer_name}, {signer_title}",
            RequiredFields = new[] { "event_name", "time_place", "purpose", "program", "budget", "signer_name", "signer_title" },
            Tags = new[] { "kế hoạch", "sự kiện" }
        };
    }

    #endregion

    #region Thông báo Templates

    private DocumentTemplate CreateThongBaoHoiNghiTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Thông báo họp",
            Type = DocumentType.ThongBao,
            Category = "Hành chính",
            Description = "Thông báo tổ chức cuộc họp",
            TemplateContent = @"
[TÊN ĐƠN VỊ]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /TB-[Viết tắt]

THÔNG BÁO
Về việc tổ chức [cuộc họp]

Kính gửi: [Thành phần tham dự]

[Tên đơn vị] thông báo về việc tổ chức [cuộc họp] như sau:

1. Thời gian: [giờ], ngày [  ] tháng [  ] năm [  ]
2. Địa điểm: [địa điểm họp]
3. Thành phần: [người tham dự]
4. Nội dung: [nội dung cuộc họp]
5. Yêu cầu: [chuẩn bị tài liệu, v.v...]

Đề nghị các đơn vị, cá nhân có liên quan tham dự đúng giờ./.


Nơi nhận:                                      [CHỨC DANH]
- Như trên;
- Lưu VT.                                      [Chữ ký, đóng dấu]

                                               [Họ và tên]
",
            AIPrompt = @"Tạo thông báo họp:
- Tên cuộc họp: {meeting_name}
- Thời gian: {time}
- Địa điểm: {location}
- Thành phần: {participants}
- Nội dung: {agenda}
- Người ký: {signer_name}, {signer_title}",
            RequiredFields = new[] { "meeting_name", "time", "location", "participants", "agenda", "signer_name", "signer_title" },
            Tags = new[] { "thông báo", "họp" }
        };
    }

    private DocumentTemplate CreateThongBaoKetQuaTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Thông báo kết quả",
            Type = DocumentType.ThongBao,
            Category = "Hành chính",
            Description = "Thông báo kết quả cuộc họp, sự kiện",
            TemplateContent = @"
[TÊN ĐƠN VỊ]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /TB-[Viết tắt]

THÔNG BÁO
Kết quả [cuộc họp/sự kiện]

Kính gửi: [Các đơn vị, cá nhân]

Ngày [  ] tháng [  ] năm [  ], [Tên đơn vị] đã tổ chức [cuộc họp/sự kiện], với các kết quả chính như sau:

I. THÀNH PHẦN THAM DỰ

[Danh sách tham dự]

II. NỘI DUNG CUỘC HỌP

[Các nội dung đã thảo luận]

III. KẾT LUẬN

[Kết luận của cuộc họp]

IV. NHIỆM VỤ TRIỂN KHAI

[Phân công nhiệm vụ]

[Tên đơn vị] thông báo để các đơn vị, cá nhân biết và thực hiện./.


Nơi nhận:                                      [CHỨC DANH]
- Như trên;
- Lưu VT.                                      [Chữ ký, đóng dấu]

                                               [Họ và tên]
",
            AIPrompt = @"Tạo thông báo kết quả:
- Cuộc họp/Sự kiện: {event_name}
- Thành phần: {participants}
- Nội dung: {content}
- Kết luận: {conclusion}
- Nhiệm vụ: {tasks}
- Người ký: {signer_name}, {signer_title}",
            RequiredFields = new[] { "event_name", "participants", "content", "conclusion", "tasks", "signer_name", "signer_title" },
            Tags = new[] { "thông báo", "kết quả" }
        };
    }

    #endregion

    #region Nghị quyết Templates

    private DocumentTemplate CreateNghiQuyetHDNDTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Nghị quyết HĐND",
            Type = DocumentType.NghiQuyet,
            Category = "Hành chính",
            Description = "Nghị quyết của Hội đồng nhân dân",
            TemplateContent = @"
HỘI ĐỒNG NHÂN DÂN
[CẤP XÃ/PHƯỜNG/TỈNH]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /NQ-HĐND

NGHỊ QUYẾT
Về việc [nội dung]

HỘI ĐỒNG NHÂN DÂN [CẤP]

Căn cứ Luật Tổ chức chính quyền địa phương;
Căn cứ [văn bản pháp luật liên quan];
Xét Tờ trình số [  ] của UBND [cấp];

QUYẾT NGHỊ:

Điều 1. [Nội dung nghị quyết]

Điều 2. Giao UBND [cấp] tổ chức thực hiện Nghị quyết.

Điều 3. Nghị quyết này có hiệu lực kể từ ngày [  ] tháng [  ] năm [  ].

Thường trực HĐND, các Ban HĐND, các đại biểu HĐND và UBND [cấp] chịu trách nhiệm thi hành Nghị quyết này./.


                                               CHỦ TỊCH

                                               [Chữ ký, đóng dấu]

                                               [Họ và tên]
",
            AIPrompt = @"Viết nghị quyết HĐND:
- Cấp: {level}
- Nội dung: {subject}
- Các điều khoản: {articles}
- Ngày hiệu lực: {effective_date}
- Chủ tịch: {chairman_name}",
            RequiredFields = new[] { "level", "subject", "articles", "effective_date", "chairman_name" },
            Tags = new[] { "nghị quyết", "HĐND" }
        };
    }

    private DocumentTemplate CreateNghiQuyetUBNDTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Nghị quyết UBND",
            Type = DocumentType.NghiQuyet,
            Category = "Hành chính",
            Description = "Nghị quyết của UBND",
            TemplateContent = @"
ỦY BAN NHÂN DÂN
[CẤP XÃ/PHƯỜNG/TỈNH]
-------

CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
---------------

Số:     /NQ-UBND

NGHỊ QUYẾT
Về việc [nội dung]

ỦY BAN NHÂN DÂN [CẤP]

Căn cứ Luật Tổ chức chính quyền địa phương;
Căn cứ [văn bản pháp luật liên quan];
Xét đề nghị của [đơn vị trình];

QUYẾT NGHỊ:

Điều 1. [Nội dung nghị quyết]

Điều 2. Tổ chức thực hiện
Giao [đơn vị] chủ trì, phối hợp với các đơn vị liên quan triển khai thực hiện.

Điều 3. Hiệu lực thi hành
Nghị quyết này có hiệu lực kể từ ngày ký.

Chánh Văn phòng UBND, Trưởng các phòng ban và các cá nhân, tổ chức có liên quan chịu trách nhiệm thi hành Nghị quyết này./.


                                               CHỦ TỊCH

                                               [Chữ ký, đóng dấu]

                                               [Họ và tên]
",
            AIPrompt = @"Viết nghị quyết UBND:
- Nội dung: {subject}
- Các điều khoản: {articles}
- Đơn vị thực hiện: {implementing_unit}
- Chủ tịch: {chairman_name}",
            RequiredFields = new[] { "subject", "articles", "implementing_unit", "chairman_name" },
            Tags = new[] { "nghị quyết", "UBND" }
        };
    }

    #endregion

    #region Các loại VB bổ sung — NĐ 30/2020 (22 loại còn lại)

    // === CHỈ THỊ ===
    private DocumentTemplate CreateChiThiTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Chỉ thị",
            Type = DocumentType.ChiThi,
            Category = "Hành chính",
            Description = "Mẫu chỉ thị của UBND",
            TemplateContent = @"
ỦY BAN NHÂN DÂN                    CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
[CẤP XÃ/PHƯỜNG]                         Độc lập - Tự do - Hạnh phúc
Số:     /CT-UBND                    [Địa danh], ngày    tháng    năm 202

                            CHỈ THỊ
                    Về việc [nội dung]

[Nội dung chỉ thị, gồm: phần mở đầu nêu lý do, phần nội dung chỉ đạo cụ thể]

1. [Nhiệm vụ/yêu cầu thứ nhất]
2. [Nhiệm vụ/yêu cầu thứ hai]
...

Chỉ thị này có hiệu lực kể từ ngày ký./.

                                               CHỦ TỊCH
                                               [Họ và tên]
",
            AIPrompt = "Viết chỉ thị: {subject}, nội dung: {content}, người ký: {signer_name}",
            RequiredFields = new[] { "subject", "content", "signer_name" },
            Tags = new[] { "chỉ thị" }
        };
    }

    // === QUY CHẾ ===
    private DocumentTemplate CreateQuyChETemplate()
    {
        return new DocumentTemplate
        {
            Name = "Quy chế",
            Type = DocumentType.QuyChE,
            Category = "Hành chính",
            Description = "Mẫu quy chế làm việc / quy chế nội bộ",
            TemplateContent = @"
Số:     /QC-UBND

                            QUY CHẾ
                    [Tên quy chế]
(Ban hành kèm theo Quyết định số    /QĐ-UBND ngày    tháng    năm 202   )

Chương I. QUY ĐỊNH CHUNG
Điều 1. Phạm vi điều chỉnh
Điều 2. Đối tượng áp dụng

Chương II. NỘI DUNG
Điều 3...

Chương III. ĐIỀU KHOẢN THI HÀNH
",
            AIPrompt = "Viết quy chế: {subject}, nội dung: {content}",
            RequiredFields = new[] { "subject", "content" },
            Tags = new[] { "quy chế" }
        };
    }

    // === QUY ĐỊNH ===
    private DocumentTemplate CreateQuyDinhTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Quy định",
            Type = DocumentType.QuyDinh,
            Category = "Hành chính",
            Description = "Mẫu quy định nội bộ",
            TemplateContent = @"
Số:     /QyĐ-UBND

                            QUY ĐỊNH
                    [Tên quy định]

Chương I. QUY ĐỊNH CHUNG
Chương II. QUY ĐỊNH CỤ THỂ
Chương III. TỔ CHỨC THỰC HIỆN
",
            AIPrompt = "Viết quy định: {subject}, nội dung: {content}",
            RequiredFields = new[] { "subject", "content" },
            Tags = new[] { "quy định" }
        };
    }

    // === THÔNG CÁO ===
    private DocumentTemplate CreateThongCaoTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Thông cáo",
            Type = DocumentType.ThongCao,
            Category = "Hành chính",
            Description = "Mẫu thông cáo báo chí / thông cáo chung",
            TemplateContent = @"
Số:     /TC-UBND

                            THÔNG CÁO
                    [Tên thông cáo]

[Nội dung thông cáo]
",
            AIPrompt = "Viết thông cáo: {subject}, nội dung: {content}",
            RequiredFields = new[] { "subject", "content" },
            Tags = new[] { "thông cáo" }
        };
    }

    // === HƯỚNG DẪN ===
    private DocumentTemplate CreateHuongDanTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Hướng dẫn",
            Type = DocumentType.HuongDan,
            Category = "Hành chính",
            Description = "Mẫu hướng dẫn thực hiện công việc",
            TemplateContent = @"
Số:     /HD-UBND

                            HƯỚNG DẪN
                    [Tên hướng dẫn]

I. MỤC ĐÍCH, YÊU CẦU
II. NỘI DUNG HƯỚNG DẪN
III. TỔ CHỨC THỰC HIỆN
",
            AIPrompt = "Viết hướng dẫn: {subject}, nội dung: {content}",
            RequiredFields = new[] { "subject", "content" },
            Tags = new[] { "hướng dẫn" }
        };
    }

    // === CHƯƠNG TRÌNH ===
    private DocumentTemplate CreateChuongTrinhTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Chương trình công tác",
            Type = DocumentType.ChuongTrinh,
            Category = "Hành chính",
            Description = "Mẫu chương trình công tác",
            TemplateContent = @"
Số:     /CTr-UBND

                        CHƯƠNG TRÌNH
                [Tên chương trình]

I. MỤC ĐÍCH, YÊU CẦU
II. NỘI DUNG CHƯƠNG TRÌNH
III. THỜI GIAN, ĐỊA ĐIỂM
IV. TỔ CHỨC THỰC HIỆN
",
            AIPrompt = "Viết chương trình: {subject}, nội dung: {content}, thời gian: {timeline}",
            RequiredFields = new[] { "subject", "content" },
            Tags = new[] { "chương trình" }
        };
    }

    // === PHƯƠNG ÁN ===
    private DocumentTemplate CreatePhuongAnTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Phương án",
            Type = DocumentType.PhuongAn,
            Category = "Hành chính",
            Description = "Mẫu phương án thực hiện",
            TemplateContent = @"
Số:     /PA-UBND

                        PHƯƠNG ÁN
                [Tên phương án]

I. SỰ CẦN THIẾT VÀ CĂN CỨ XÂY DỰNG
II. NỘI DUNG PHƯƠNG ÁN
III. KINH PHÍ THỰC HIỆN
IV. TỔ CHỨC THỰC HIỆN
",
            AIPrompt = "Viết phương án: {subject}, nội dung: {content}",
            RequiredFields = new[] { "subject", "content" },
            Tags = new[] { "phương án" }
        };
    }

    // === ĐỀ ÁN ===
    private DocumentTemplate CreateDeAnTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Đề án",
            Type = DocumentType.DeAn,
            Category = "Hành chính",
            Description = "Mẫu đề án",
            TemplateContent = @"
Số:     /ĐA-UBND

                            ĐỀ ÁN
                    [Tên đề án]

I. SỰ CẦN THIẾT VÀ CĂN CỨ
II. MỤC TIÊU
III. NỘI DUNG ĐỀ ÁN
IV. GIẢI PHÁP THỰC HIỆN
V. KINH PHÍ
VI. TỔ CHỨC THỰC HIỆN
",
            AIPrompt = "Viết đề án: {subject}, mục tiêu: {objectives}, nội dung: {content}",
            RequiredFields = new[] { "subject", "content" },
            Tags = new[] { "đề án" }
        };
    }

    // === DỰ ÁN ===
    private DocumentTemplate CreateDuAnTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Dự án",
            Type = DocumentType.DuAn,
            Category = "Hành chính",
            Description = "Mẫu dự án",
            TemplateContent = @"
Số:     /DA-UBND

                            DỰ ÁN
                    [Tên dự án]

I. THÔNG TIN CHUNG
II. MỤC TIÊU DỰ ÁN
III. NỘI DUNG VÀ QUY MÔ
IV. TỔNG MỨC ĐẦU TƯ
V. TIẾN ĐỘ THỰC HIỆN
VI. TỔ CHỨC THỰC HIỆN
",
            AIPrompt = "Viết dự án: {subject}, mục tiêu: {objectives}, nội dung: {content}",
            RequiredFields = new[] { "subject", "content" },
            Tags = new[] { "dự án" }
        };
    }

    // === BIÊN BẢN ===
    private DocumentTemplate CreateBienBanTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Biên bản họp / làm việc",
            Type = DocumentType.BienBan,
            Category = "Hành chính",
            Description = "Mẫu biên bản cuộc họp, làm việc",
            TemplateContent = @"
                            BIÊN BẢN
                    [Tên cuộc họp/làm việc]

Thời gian: [   ]
Địa điểm: [   ]
Thành phần tham dự:
- Chủ trì: [   ]
- Tham dự: [   ]
- Thư ký: [   ]

NỘI DUNG:
1. [Nội dung thứ nhất]
2. [Nội dung thứ hai]

KẾT LUẬN:
[Kết luận cuộc họp]

THƯ KÝ                                CHỦ TRÌ
[Họ và tên]                            [Họ và tên]
",
            AIPrompt = "Viết biên bản họp: {subject}, thời gian: {time}, địa điểm: {location}, thành phần: {attendees}, nội dung: {content}",
            RequiredFields = new[] { "subject", "content" },
            Tags = new[] { "biên bản", "cuộc họp" }
        };
    }

    // === HỢP ĐỒNG ===
    private DocumentTemplate CreateHopDongTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Hợp đồng",
            Type = DocumentType.HopDong,
            Category = "Hành chính",
            Description = "Mẫu hợp đồng",
            TemplateContent = @"
Số:     /HĐ-UBND

                        HỢP ĐỒNG
                    [Tên hợp đồng]

Căn cứ Bộ luật Dân sự 2015;
Căn cứ [văn bản liên quan];

Hôm nay, ngày    tháng    năm 202   , tại [địa điểm]

BÊN A: [Thông tin bên A]
BÊN B: [Thông tin bên B]

Hai bên thống nhất ký kết hợp đồng với các điều khoản sau:

Điều 1. Nội dung công việc
Điều 2. Thời gian thực hiện  
Điều 3. Giá trị hợp đồng
Điều 4. Quyền và nghĩa vụ
Điều 5. Điều khoản chung

ĐẠI DIỆN BÊN A                     ĐẠI DIỆN BÊN B
",
            AIPrompt = "Viết hợp đồng: {subject}, bên A: {party_a}, bên B: {party_b}, nội dung: {content}",
            RequiredFields = new[] { "subject", "content" },
            Tags = new[] { "hợp đồng" }
        };
    }

    // === CÔNG ĐIỆN ===
    private DocumentTemplate CreateCongDienTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Công điện",
            Type = DocumentType.CongDien,
            Category = "Hành chính",
            Description = "Mẫu công điện khẩn",
            TemplateContent = @"
Số:     /CĐ-UBND

                        CÔNG ĐIỆN
            [Về việc nội dung công điện]

[CƠ QUAN BAN HÀNH] ĐIỆN:
[Cơ quan nhận]

[Nội dung công điện — ngắn gọn, khẩn cấp]

Yêu cầu [đơn vị] khẩn trương thực hiện./.
",
            AIPrompt = "Viết công điện khẩn: {subject}, nơi nhận: {to_org}, nội dung: {content}",
            RequiredFields = new[] { "subject", "to_org", "content" },
            Tags = new[] { "công điện", "khẩn" }
        };
    }

    // === BẢN GHI NHỚ ===
    private DocumentTemplate CreateBanGhiNhoTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Bản ghi nhớ",
            Type = DocumentType.BanGhiNho,
            Category = "Hành chính",
            Description = "Mẫu bản ghi nhớ hợp tác",
            TemplateContent = @"
                        BẢN GHI NHỚ
            [Về việc hợp tác / thỏa thuận]

Bên A: [   ]
Bên B: [   ]

Hai bên thống nhất ghi nhớ các nội dung sau:
1. [Nội dung thứ nhất]
2. [Nội dung thứ hai]

ĐẠI DIỆN BÊN A                     ĐẠI DIỆN BÊN B
",
            AIPrompt = "Viết bản ghi nhớ: {subject}, nội dung: {content}",
            RequiredFields = new[] { "subject", "content" },
            Tags = new[] { "bản ghi nhớ" }
        };
    }

    // === BẢN THỎA THUẬN ===
    private DocumentTemplate CreateBanThoaThuanTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Bản thỏa thuận",
            Type = DocumentType.BanThoaThuan,
            Category = "Hành chính",
            Description = "Mẫu bản thỏa thuận",
            TemplateContent = @"
                        BẢN THỎA THUẬN
                [Về việc ...]

Các bên tham gia:
- Bên A: [   ]
- Bên B: [   ]

Nội dung thỏa thuận:
[...]

ĐẠI DIỆN BÊN A                     ĐẠI DIỆN BÊN B
",
            AIPrompt = "Viết bản thỏa thuận: {subject}, nội dung: {content}",
            RequiredFields = new[] { "subject", "content" },
            Tags = new[] { "bản thỏa thuận" }
        };
    }

    // === GIẤY ỦY QUYỀN ===
    private DocumentTemplate CreateGiayUyQuyenTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Giấy ủy quyền",
            Type = DocumentType.GiayUyQuyen,
            Category = "Hành chính",
            Description = "Mẫu giấy ủy quyền",
            TemplateContent = @"
Số:     /GUQ-UBND

                        GIẤY ỦY QUYỀN

Căn cứ [văn bản pháp luật];

Tôi, [họ tên người ủy quyền], chức vụ: [chức vụ]
Ủy quyền cho: [họ tên người được ủy quyền], chức vụ: [chức vụ]

Nội dung ủy quyền: [   ]
Thời hạn ủy quyền: Từ ngày    đến ngày   

Người được ủy quyền không được ủy quyền lại cho người khác./.

                                               [CHỨC DANH]
                                               [Họ và tên]
",
            AIPrompt = "Viết giấy ủy quyền: người ủy quyền: {grantor}, người được ủy quyền: {grantee}, nội dung: {content}",
            RequiredFields = new[] { "grantor", "grantee", "content" },
            Tags = new[] { "giấy ủy quyền" }
        };
    }

    // === GIẤY MỜI ===
    private DocumentTemplate CreateGiayMoiTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Giấy mời họp",
            Type = DocumentType.GiayMoi,
            Category = "Hành chính",
            Description = "Mẫu giấy mời họp / hội nghị",
            TemplateContent = @"
Số:     /GM-UBND

                            GIẤY MỜI

Kính gửi: [   ]

[Cơ quan] trân trọng kính mời [đại diện cơ quan / ông bà] đến dự:

Nội dung: [   ]
Thời gian: [   ]
Địa điểm: [   ]

Rất mong [quý cơ quan / ông bà] thu xếp thời gian tham dự./.
",
            AIPrompt = "Viết giấy mời: nội dung: {subject}, thời gian: {time}, địa điểm: {location}, người nhận: {to_org}",
            RequiredFields = new[] { "subject", "time", "location" },
            Tags = new[] { "giấy mời" }
        };
    }

    // === GIẤY GIỚI THIỆU ===
    private DocumentTemplate CreateGiayGioiThieuTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Giấy giới thiệu",
            Type = DocumentType.GiayGioiThieu,
            Category = "Hành chính",
            Description = "Mẫu giấy giới thiệu cán bộ",
            TemplateContent = @"
Số:     /GGT-UBND

                        GIẤY GIỚI THIỆU

Kính gửi: [   ]

[Cơ quan] giới thiệu:
Ông/Bà: [   ], Chức vụ: [   ]
Được cử đến: [   ]
Về việc: [   ]

Mong [quý cơ quan] tiếp và giải quyết./.

Giấy này có giá trị đến ngày [   ].
",
            AIPrompt = "Viết giấy giới thiệu: người được giới thiệu: {person}, đến: {to_org}, nội dung: {content}",
            RequiredFields = new[] { "person", "to_org", "content" },
            Tags = new[] { "giấy giới thiệu" }
        };
    }

    // === GIẤY NGHỈ PHÉP ===
    private DocumentTemplate CreateGiayNghiPhepTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Giấy nghỉ phép",
            Type = DocumentType.GiayNghiPhep,
            Category = "Nội vụ",
            Description = "Mẫu giấy nghỉ phép cán bộ, công chức",
            TemplateContent = @"
Số:     /GNP-UBND

                        GIẤY NGHỈ PHÉP

Họ và tên: [   ]
Chức vụ: [   ]
Đơn vị công tác: [   ]

Xin nghỉ phép từ ngày    đến ngày    (    ngày).
Lý do: [   ]
Địa chỉ trong thời gian nghỉ: [   ]

Người xin nghỉ phép             Thủ trưởng đơn vị
[Ký, ghi rõ họ tên]             [Ký, ghi rõ họ tên]
",
            AIPrompt = "Viết giấy nghỉ phép: người xin: {person}, từ ngày: {from_date}, đến ngày: {to_date}, lý do: {reason}",
            RequiredFields = new[] { "person", "from_date", "to_date", "reason" },
            Tags = new[] { "giấy nghỉ phép", "nội vụ" }
        };
    }

    // === PHIẾU GỬI ===
    private DocumentTemplate CreatePhieuGuiTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Phiếu gửi",
            Type = DocumentType.PhieuGui,
            Category = "Văn thư",
            Description = "Mẫu phiếu gửi văn bản, tài liệu",
            TemplateContent = @"
Số:     /PG-VP

                        PHIẾU GỬI

Kính gửi: [   ]

[Cơ quan] gửi kèm theo phiếu này [số lượng] văn bản/tài liệu:
1. [Tên VB, số, ngày]
2. [Tên VB, số, ngày]

Đề nghị [quý cơ quan] xác nhận đã nhận đủ./.
",
            AIPrompt = "Viết phiếu gửi: nơi nhận: {to_org}, danh sách VB: {documents}",
            RequiredFields = new[] { "to_org", "documents" },
            Tags = new[] { "phiếu gửi", "văn thư" }
        };
    }

    // === PHIẾU CHUYỂN ===
    private DocumentTemplate CreatePhieuChuyenTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Phiếu chuyển",
            Type = DocumentType.PhieuChuyen,
            Category = "Văn thư",
            Description = "Mẫu phiếu chuyển văn bản nội bộ",
            TemplateContent = @"
Số:     /PC-VP

                        PHIẾU CHUYỂN

Kính chuyển: [Đơn vị/Cá nhân nhận]

Văn bản: [Số, ký hiệu, ngày tháng, cơ quan ban hành]
Trích yếu: [   ]

Ý kiến chỉ đạo: [   ]
Hạn giải quyết: [   ]
",
            AIPrompt = "Viết phiếu chuyển: đơn vị nhận: {to_unit}, văn bản: {document_ref}, ý kiến: {instructions}",
            RequiredFields = new[] { "to_unit", "document_ref" },
            Tags = new[] { "phiếu chuyển", "văn thư" }
        };
    }

    // === PHIẾU BÁO ===
    private DocumentTemplate CreatePhieuBaoTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Phiếu báo",
            Type = DocumentType.PhieuBao,
            Category = "Văn thư",
            Description = "Mẫu phiếu báo (thông báo nội bộ)",
            TemplateContent = @"
Số:     /PB-VP

                        PHIẾU BÁO

Kính gửi: [   ]

[Nội dung thông báo]

Đề nghị [đơn vị/cá nhân] lưu ý và thực hiện./.
",
            AIPrompt = "Viết phiếu báo: nội dung: {content}, nơi nhận: {to_org}",
            RequiredFields = new[] { "content" },
            Tags = new[] { "phiếu báo" }
        };
    }

    // === THƯ CÔNG ===
    private DocumentTemplate CreateThuCongTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Thư công",
            Type = DocumentType.ThuCong,
            Category = "Hành chính",
            Description = "Mẫu thư công (thư chúc mừng, cảm ơn, chia buồn...)",
            TemplateContent = @"
Số:     /TC-UBND

                            THƯ [CHÚC MỪNG/CẢM ƠN]

Kính gửi: [   ]

[Nội dung thư]

Trân trọng./.

                                               [CHỨC DANH]
                                               [Họ và tên]
",
            AIPrompt = "Viết thư công: loại: {letter_type}, nơi nhận: {to_org}, nội dung: {content}",
            RequiredFields = new[] { "to_org", "content" },
            Tags = new[] { "thư công" }
        };
    }

    // === BẢN SAO Y — Mẫu 3.1, Phụ lục III, NĐ 30/2020 ===
    // Theo Điều 25-27, NĐ 30/2020/NĐ-CP
    private DocumentTemplate CreateSaoYTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Bản sao y",
            Type = DocumentType.Khac,
            Category = "Bản sao",
            Description = "Mẫu bản sao y văn bản — Mẫu 3.1, Phụ lục III, NĐ 30/2020. Sao đầy đủ, chính xác nội dung bản gốc hoặc bản chính (Điều 25).",
            TemplateContent = @"
TÊN CƠ QUAN, TỔ CHỨC             CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
____________                       Độc lập - Tự do - Hạnh phúc
                                    _________________________________________

                        [NỘI DUNG VĂN BẢN GỐC ĐƯỢC SAO Y]

                                ./.                

                                            SAO Y

TÊN CƠ QUAN, TỔ CHỨC                               
Số: ....../SY-[Viết tắt CQ]              [Địa danh], ngày ... tháng ... năm ...
____________

Nơi nhận:                                 QUYỀN HẠN, CHỨC VỤ CỦA NGƯỜI KÝ
- ...............;                         (Chữ ký, dấu của cơ quan, tổ chức
- ...............;                          thực hiện sao văn bản)
- Lưu: VT.                                Họ và tên
",
            AIPrompt = @"Soạn bản sao y theo đúng thể thức Mẫu 3.1, Phụ lục III, NĐ 30/2020/NĐ-CP.
Văn bản gốc: {original_document}
Cơ quan sao: {copy_org}
Người ký sao: {signer}
Chức vụ: {signer_title}
Nơi nhận: {recipients}
Địa danh: {location}",
            RequiredFields = new[] { "original_document", "copy_org", "signer", "recipients" },
            Tags = new[] { "sao y", "bản sao", "Điều 25", "NĐ 30/2020" }
        };
    }

    // === BẢN SAO LỤC — Mẫu 3.1, Phụ lục III, NĐ 30/2020 ===
    private DocumentTemplate CreateSaoLucTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Bản sao lục",
            Type = DocumentType.Khac,
            Category = "Bản sao",
            Description = "Mẫu bản sao lục — Mẫu 3.1, Phụ lục III, NĐ 30/2020. Sao đầy đủ, chính xác nội dung của bản sao y (Điều 25 khoản 2).",
            TemplateContent = @"
TÊN CƠ QUAN, TỔ CHỨC             CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
____________                       Độc lập - Tự do - Hạnh phúc
                                    _________________________________________

                        [NỘI DUNG BẢN SAO Y ĐƯỢC SAO LỤC]

                                ./.                

                                            SAO LỤC

TÊN CƠ QUAN, TỔ CHỨC                               
Số: ....../SL-[Viết tắt CQ]              [Địa danh], ngày ... tháng ... năm ...
____________

Nơi nhận:                                 QUYỀN HẠN, CHỨC VỤ CỦA NGƯỜI KÝ
- ...............;                         (Chữ ký, dấu của cơ quan, tổ chức
- ...............;                          thực hiện sao văn bản)
- Lưu: VT.                                Họ và tên
",
            AIPrompt = @"Soạn bản sao lục theo đúng thể thức Mẫu 3.1, Phụ lục III, NĐ 30/2020/NĐ-CP.
Bản sao y gốc: {original_saoy}
Cơ quan sao: {copy_org}
Người ký sao: {signer}
Chức vụ: {signer_title}
Nơi nhận: {recipients}
Địa danh: {location}",
            RequiredFields = new[] { "original_saoy", "copy_org", "signer", "recipients" },
            Tags = new[] { "sao lục", "bản sao", "Điều 25", "NĐ 30/2020" }
        };
    }

    // === BẢN TRÍCH SAO — Mẫu 3.1, Phụ lục III, NĐ 30/2020 ===
    private DocumentTemplate CreateTrichSaoTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Bản trích sao",
            Type = DocumentType.Khac,
            Category = "Bản sao",
            Description = "Mẫu bản trích sao — Mẫu 3.1, Phụ lục III, NĐ 30/2020. Sao chính xác phần nội dung cần trích từ bản gốc hoặc bản chính (Điều 25 khoản 3).",
            TemplateContent = @"
TÊN CƠ QUAN, TỔ CHỨC             CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
____________                       Độc lập - Tự do - Hạnh phúc
                                    _________________________________________

                        [PHẦN NỘI DUNG TRÍCH SAO TỪ VĂN BẢN GỐC]

                                ./.                

                                            TRÍCH SAO

TÊN CƠ QUAN, TỔ CHỨC                               
Số: ....../TrS-[Viết tắt CQ]             [Địa danh], ngày ... tháng ... năm ...
____________

Nơi nhận:                                 QUYỀN HẠN, CHỨC VỤ CỦA NGƯỜI KÝ
- ...............;                         (Chữ ký, dấu của cơ quan, tổ chức
- ...............;                          thực hiện sao văn bản)
- Lưu: VT.                                Họ và tên
",
            AIPrompt = @"Soạn bản trích sao theo đúng thể thức Mẫu 3.1, Phụ lục III, NĐ 30/2020/NĐ-CP.
Văn bản gốc: {original_document}
Phần cần trích: {extract_section}
Cơ quan sao: {copy_org}
Người ký sao: {signer}
Chức vụ: {signer_title}
Nơi nhận: {recipients}
Địa danh: {location}",
            RequiredFields = new[] { "original_document", "extract_section", "copy_org", "signer", "recipients" },
            Tags = new[] { "trích sao", "bản sao", "Điều 25", "NĐ 30/2020" }
        };
    }

    // === PHỤ LỤC VĂN BẢN — Mẫu 2.1, Phụ lục III, NĐ 30/2020 ===
    private DocumentTemplate CreatePhuLucVanBanTemplate()
    {
        return new DocumentTemplate
        {
            Name = "Phụ lục văn bản hành chính",
            Type = DocumentType.Khac,
            Category = "Phụ lục",
            Description = "Mẫu phụ lục kèm theo văn bản hành chính — Mẫu 2.1, Phụ lục III, NĐ 30/2020. Phụ lục được đánh số La Mã (I, II, III...) nếu có từ 2 phụ lục trở lên.",
            TemplateContent = @"
                                Phụ lục [số La Mã]
                            [TÊN PHỤ LỤC]
       (Kèm theo [Tên loại VB] số .../[Ký hiệu]-[CQ] ngày ... tháng ... năm ... của [Cơ quan])
                            ___________

[Nội dung phụ lục: bảng biểu, danh sách, quy trình...]

                                ./.                
",
            AIPrompt = @"Soạn phụ lục văn bản hành chính theo đúng Mẫu 2.1, Phụ lục III, NĐ 30/2020/NĐ-CP.
Số thứ tự phụ lục: {appendix_number}
Tên phụ lục: {appendix_title}
Văn bản kèm theo (loại, số, ký hiệu, ngày, cơ quan): {parent_document}
Nội dung phụ lục: {content}",
            RequiredFields = new[] { "appendix_title", "parent_document", "content" },
            Tags = new[] { "phụ lục", "kèm theo", "Mẫu 2.1", "NĐ 30/2020" }
        };
    }

    #endregion
}
