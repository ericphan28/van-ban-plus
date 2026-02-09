using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service tạo dữ liệu demo cho test
/// </summary>
public class SeedDataService
{
    private readonly DocumentService _documentService;
    private readonly Random _random = new Random();

    public SeedDataService(DocumentService documentService)
    {
        _documentService = documentService;
    }

    /// <summary>
    /// Tạo 20 văn bản demo
    /// </summary>
    public List<Document> GenerateDemoDocuments(int count = 20)
    {
        var documents = new List<Document>();
        var types = Enum.GetValues<DocumentType>();
        var directions = Enum.GetValues<Direction>();
        
        var titles = new[]
        {
            "Về việc triển khai kế hoạch năm 2026",
            "Thông báo tổ chức hội nghị tổng kết",
            "Quyết định bổ nhiệm cán bộ",
            "Kế hoạch đào tạo nâng cao năng lực",
            "Báo cáo tình hình hoạt động quý I",
            "Công văn xin ý kiến chỉ đạo",
            "Hướng dẫn thực hiện quy trình mới",
            "Nghị quyết phê duyệt dự án",
            "Quy định về chế độ làm việc",
            "Tờ trình đề xuất phương án",
            "Thông tư hướng dẫn thi hành",
            "Chỉ thị tăng cường công tác quản lý",
            "Báo cáo đánh giá kết quả thực hiện",
            "Công văn yêu cầu báo cáo",
            "Kế hoạch kiểm tra giám sát",
            "Quyết định phê duyệt kế hoạch",
            "Thông báo thay đổi quy trình",
            "Công văn đề nghị hỗ trợ",
            "Biên bản họp ban lãnh đạo",
            "Quyết định khen thưởng"
        };

        var subjects = new[]
        {
            "Triển khai nhiệm vụ trọng tâm năm 2026",
            "Tổ chức các hoạt động kỷ niệm ngày thành lập",
            "Bổ nhiệm Phó Giám đốc phụ trách kỹ thuật",
            "Nâng cao trình độ chuyên môn cho cán bộ",
            "Đánh giá hoạt động sản xuất kinh doanh quý I/2026",
            "Xin ý kiến về phương án cải cách hành chính",
            "Quy trình phê duyệt dự toán và thanh quyết toán",
            "Phê duyệt dự án đầu tư hạ tầng công nghệ thông tin",
            "Quy định giờ làm việc và nghỉ ngơi",
            "Đề xuất điều chỉnh cơ cấu tổ chức",
            "Quy định mới về quản lý tài chính",
            "Tăng cường công tác bảo vệ môi trường",
            "Kết quả triển khai chiến lược phát triển",
            "Báo cáo tình hình sử dụng ngân sách",
            "Kiểm tra việc chấp hành nội quy",
            "Kế hoạch tuyển dụng nhân sự năm 2026",
            "Cập nhật quy trình quản lý chất lượng",
            "Hỗ trợ kinh phí tổ chức sự kiện",
            "Thống nhất phương án hợp tác đối tác",
            "Khen thưởng tập thể và cá nhân xuất sắc"
        };

        var contents = new[]
        {
            "Căn cứ Luật Tổ chức Chính quyền địa phương năm 2015;\nCăn cứ Nghị định số 123/2016/NĐ-CP ngày 01/9/2016 của Chính phủ;\nXét đề nghị của Trưởng phòng Tổ chức - Hành chính,\n\nQUYẾT ĐỊNH:\n\nĐiều 1. Phê duyệt kế hoạch triển khai các nhiệm vụ trọng tâm trong năm 2026 với các nội dung chính sau:\n- Hoàn thiện hệ thống văn bản quy phạm pháp luật\n- Đẩy mạnh cải cách hành chính\n- Nâng cao chất lượng đội ngũ cán bộ công chức\n- Tăng cường ứng dụng công nghệ thông tin\n\nĐiều 2. Các phòng ban chức năng có trách nhiệm triển khai thực hiện theo đúng kế hoạch đã được phê duyệt.\n\nĐiều 3. Quyết định này có hiệu lực kể từ ngày ký.",
            
            "Kính gửi: Toàn thể cán bộ, công chức, viên chức\n\nĐơn vị trân trọng thông báo về việc tổ chức Hội nghị tổng kết công tác năm 2025 và triển khai nhiệm vụ năm 2026 như sau:\n\n1. Thời gian: 8h00, ngày 15/01/2026\n2. Địa điểm: Hội trường tầng 3, trụ sở cơ quan\n3. Thành phần tham dự: Toàn thể CBCCVC\n4. Nội dung chính:\n- Báo cáo tổng kết hoạt động năm 2025\n- Phương hướng nhiệm vụ năm 2026\n- Biểu dương khen thưởng tập thể, cá nhân\n\nĐề nghị các đồng chí sắp xếp công việc để tham dự đầy đủ.",
            
            "Căn cứ các quy định hiện hành về công tác cán bộ;\nXét đề nghị của Hội đồng tuyển dụng và bổ nhiệm cán bộ,\n\nQUYẾT ĐỊNH:\n\nĐiều 1. Bổ nhiệm ông Nguyễn Văn A giữ chức vụ Phó Giám đốc phụ trách mảng Kỹ thuật kể từ ngày 01/02/2026.\n\nĐiều 2. Ông Nguyễn Văn A có trách nhiệm:\n- Phụ trách chung các hoạt động kỹ thuật của đơn vị\n- Tham mưu cho Giám đốc về các vấn đề kỹ thuật\n- Điều hành các phòng ban thuộc khối kỹ thuật\n\nĐiều 3. Quyết định này có hiệu lực từ ngày ký.",
            
            "Nhằm nâng cao năng lực chuyên môn và kỹ năng nghiệp vụ cho đội ngũ cán bộ, Ban Giám đốc phê duyệt Kế hoạch đào tạo năm 2026 với các nội dung chính:\n\n1. Mục tiêu:\n- Đào tạo 100% CBCCVC về kỹ năng tin học văn phòng\n- Bồi dưỡng chuyên môn sâu cho 50% cán bộ chuyên môn\n- Đào tạo ngoại ngữ cho 30% cán bộ trẻ\n\n2. Hình thức:\n- Đào tạo tập trung tại cơ quan\n- Cử đi học các lớp ngắn hạn\n- Học trực tuyến qua internet\n\n3. Kinh phí: 500 triệu đồng từ ngân sách đơn vị.",
            
            "Kính gửi: Ban Giám đốc\n\nPhòng Kế hoạch - Tài chính báo cáo tình hình hoạt động sản xuất kinh doanh Quý I/2026:\n\n1. Doanh thu: 15,5 tỷ đồng (đạt 102% kế hoạch)\n2. Lợi nhuận: 2,3 tỷ đồng (đạt 115% kế hoạch)\n3. Các chỉ tiêu khác đều hoàn thành và vượt kế hoạch\n\n4. Đánh giá:\n- Ưu điểm: Sản xuất kinh doanh đạt kết quả tốt\n- Hạn chế: Chi phí quản lý còn cao\n- Nguyên nhân: Giá nguyên vật liệu tăng\n\n5. Giải pháp:\n- Tìm nguồn nguyên liệu thay thế\n- Tối ưu hóa quy trình sản xuất\n- Tiết giảm chi phí không cần thiết.",
            
            "Kính gửi: Lãnh đạo cấp trên\n\nĐơn vị kính đề nghị được chỉ đạo về phương án cải cách hành chính với các nội dung:\n\n1. Về cơ chế hoạt động:\n- Đề xuất thành lập bộ phận một cửa\n- Áp dụng hệ thống quản lý chất lượng ISO 9001\n\n2. Về nhân sự:\n- Sắp xếp lại cơ cấu tổ chức\n- Tuyển dụng bổ sung 5 biên chế\n\n3. Về cơ sở vật chất:\n- Đầu tư nâng cấp hệ thống công nghệ thông tin\n- Cải tạo trụ sở làm việc\n\nĐề nghị cấp trên xem xét và có ý kiến chỉ đạo."
        };

        var issuers = new[]
        {
            "UBND Tỉnh",
            "Sở Nội vụ",
            "Sở Tài chính",
            "Sở Kế hoạch và Đầu tư",
            "Sở Giáo dục và Đào tạo",
            "Sở Y tế",
            "Sở Công Thương",
            "Sở Nông nghiệp và PTNT",
            "Sở Xây dựng",
            "Sở Giao thông Vận tải",
            "Ban Giám đốc",
            "Phòng Hành chính - Tổng hợp",
            "Phòng Tài chính - Kế toán",
            "Phòng Kinh doanh",
            "Phòng Kỹ thuật"
        };

        var categories = new[]
        {
            "Hành chính",
            "Tài chính",
            "Nhân sự",
            "Kỹ thuật",
            "Kinh doanh",
            "Đào tạo",
            "Tổ chức",
            "Pháp chế",
            "Thanh tra"
        };

        for (int i = 0; i < count; i++)
        {
            var issueDate = DateTime.Now.AddDays(-_random.Next(1, 365));
            var doc = new Document
            {
                Number = $"{i + 1:000}/CV-{issuers[_random.Next(issuers.Length)].Split(' ')[0]}",
                Title = titles[i % titles.Length],
                Subject = subjects[i % subjects.Length],
                Content = contents[_random.Next(contents.Length)],
                Issuer = issuers[_random.Next(issuers.Length)],
                IssueDate = issueDate,
                Type = types[_random.Next(types.Length)],
                Direction = directions[_random.Next(directions.Length)],
                Category = categories[_random.Next(categories.Length)],
                Status = _random.Next(10) > 2 ? "Còn hiệu lực" : "Hết hiệu lực",
                Tags = GenerateRandomTags(),
                CreatedBy = "Admin",
                CreatedDate = issueDate,
                ModifiedBy = "Admin",
                ModifiedDate = issueDate
            };

            // Random workflow status for văn bản đi
            if (doc.Direction == Direction.Di)
            {
                var statuses = Enum.GetValues<DocumentStatus>();
                doc.WorkflowStatus = statuses[_random.Next(statuses.Length)];
                
                // Thêm Recipients cho văn bản đi (logic dựa trên loại văn bản)
                doc.Recipients = GenerateLogicalRecipients(doc.Type, doc.Title);
                
                if (doc.WorkflowStatus == DocumentStatus.Published)
                {
                    doc.SignedBy = "Giám đốc Nguyễn Văn A";
                    doc.SignedDate = doc.IssueDate.AddDays(-2);
                    doc.PublishedBy = "Admin";
                    doc.PublishedDate = doc.IssueDate;
                }
            }

            var created = _documentService.AddDocument(doc);
            documents.Add(created);
        }

        return documents;
    }

    /// <summary>
    /// Sinh Recipients logic dựa trên loại văn bản và nội dung
    /// </summary>
    private string[] GenerateLogicalRecipients(DocumentType type, string title)
    {
        // Quyết định bổ nhiệm/nhân sự -> gửi các phòng ban và cá nhân liên quan
        if (type == DocumentType.QuyetDinh && (title.Contains("bổ nhiệm") || title.Contains("nhân sự") || title.Contains("khen thưởng")))
        {
            return new[]
            {
                "- Như trên",
                "- Các phòng, ban trực thuộc",
                "- Phòng Tổ chức - Hành chính",
                "- Phòng Tài chính - Kế toán",
                "- Người được bổ nhiệm (để thực hiện)",
                "- Lưu: VT, HCTH"
            };
        }

        // Thông báo hội nghị -> gửi toàn thể
        if (type == DocumentType.ThongBao && title.Contains("hội nghị"))
        {
            return new[]
            {
                "- Ban Giám đốc",
                "- Toàn thể CBCCVC",
                "- Các đơn vị trực thuộc",
                "- Công đoàn cơ sở",
                "- Lưu: VT"
            };
        }

        // Kế hoạch -> gửi các đơn vị thực hiện
        if (type == DocumentType.KeHoach)
        {
            return new[]
            {
                "- Ban Giám đốc (để chỉ đạo)",
                "- Các phòng, ban chức năng (để thực hiện)",
                "- Phòng Kế hoạch - Tài chính (để theo dõi)",
                "- Lưu: VT"
            };
        }

        // Báo cáo -> gửi lên cấp trên
        if (type == DocumentType.BaoCao)
        {
            return new[]
            {
                "- UBND Tỉnh",
                "- Sở Nội vụ",
                "- Sở Kế hoạch và Đầu tư",
                "- Lưu: VT, Phòng KH-TC"
            };
        }

        // Công văn xin ý kiến -> gửi cấp có thẩm quyền
        if (type == DocumentType.CongVan && (title.Contains("xin ý kiến") || title.Contains("đề nghị")))
        {
            return new[]
            {
                "- Lãnh đạo UBND Tỉnh",
                "- Sở Nội vụ (để phối hợp)",
                "- Sở Tài chính (để tham mưu)",
                "- Lưu: VT"
            };
        }

        // Quy định/Hướng dẫn -> gửi toàn bộ đơn vị
        if (type == DocumentType.QuyDinh || type == DocumentType.HuongDan)
        {
            return new[]
            {
                "- Toàn thể CBCCVC",
                "- Các phòng, ban, đơn vị trực thuộc",
                "- Trưởng các phòng ban (để triển khai)",
                "- Lưu: VT, HCTH"
            };
        }

        // Chỉ thị -> gửi các cấp dưới
        if (type == DocumentType.ChiThi)
        {
            return new[]
            {
                "- Các phòng, ban chức năng",
                "- Các đơn vị trực thuộc",
                "- Trưởng các đơn vị (để chỉ đạo)",
                "- Lưu: VT, HCTH"
            };
        }

        // Nghị quyết -> gửi các đơn vị thực hiện
        if (type == DocumentType.NghiQuyet)
        {
            return new[]
            {
                "- Các thành viên Ban Giám đốc",
                "- Các phòng, ban chức năng",
                "- Các chi bộ trực thuộc",
                "- Lưu: VT, HCTH"
            };
        }

        // Tờ trình -> gửi cấp có thẩm quyền quyết định
        if (type == DocumentType.ToTrinh)
        {
            return new[]
            {
                "- Ban Giám đốc (để xem xét, quyết định)",
                "- Phòng Tài chính - Kế toán (để tham mưu)",
                "- Phòng Kế hoạch (để phối hợp)",
                "- Lưu: VT"
            };
        }

        // Mặc định - công văn thông thường
        return new[]
        {
            "- Như trên",
            "- Các đơn vị có liên quan",
            "- Lưu: VT"
        };
    }

    private string[] GenerateRandomTags()
    {
        var allTags = new[]
        {
            "Khẩn", "Quan trọng", "Mật", "Nội bộ",
            "Kế hoạch", "Báo cáo", "Quyết định", "Thông báo",
            "2026", "Q1", "Q2", "Năm 2026",
            "Tài chính", "Nhân sự", "Đào tạo", "Kỹ thuật"
        };

        var tagCount = _random.Next(2, 5);
        return allTags.OrderBy(x => _random.Next()).Take(tagCount).ToArray();
    }
}
