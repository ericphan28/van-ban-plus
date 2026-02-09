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
        var existingTemplates = _documentService.GetAllTemplates();
        
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
            Description = "Công văn từ UBND cấp huyện/xã gửi các Sở, Ban, Ngành cấp tỉnh",
            TemplateContent = @"
ỦY BAN NHÂN DÂN
[CẤP XÃ/HUYỆN]
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
[CẤP XÃ/HUYỆN/TỈNH]
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
[CẤP XÃ/HUYỆN/TỈNH]
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
}
