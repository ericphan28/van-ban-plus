using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Tests;

/// <summary>
/// Đầu vào mẫu cho prompt soạn thảo: phủ mọi DocumentType và mọi nhánh của từng hàm dựng trong AIComposeDialog.
/// Thứ tự trường = thứ tự ô nhập (nhánh soạn chung liệt kê theo thứ tự này).
/// </summary>
internal static class MauSoanThao
{
    internal sealed record Ca(string Ten, DocumentType Loai, string? TenMau, (string Khoa, string GiaTri)[] Truong, ComposeLocality? DiaPhuong)
    {
        public Dictionary<string, string> TuDien()
        {
            var d = new Dictionary<string, string>();
            foreach (var (k, v) in Truong) d[k] = v;
            return d;
        }
    }

    private static (string Khoa, string GiaTri)[] T(params string[] kv)
    {
        if (kv.Length % 2 != 0) throw new ArgumentException("Cần cặp khóa, giá trị");
        return Enumerable.Range(0, kv.Length / 2).Select(i => (kv[2 * i], kv[2 * i + 1])).ToArray();
    }

    private static Ca G(string ten, DocumentType loai, string tenMau, params string[] kv) => new(ten, loai, tenMau, T(kv), ComposeLocality.GiaKiem);

    /// <summary>Ca chạy được trên hộp thoại cũ: địa phương Gia Kiệm, luôn có tên mẫu (hộp thoại luôn có mẫu khi soạn).</summary>
    public static IReadOnlyList<Ca> GiaKiem { get; } = new[]
    {
        G("cong-van-day-du", DocumentType.CongVan, "Công văn đề nghị",
            "from_org", "UBND xã Minh An", "to_org", "Sở Tài chính tỉnh Lâm Đồng", "subject", "Đề nghị bổ sung kinh phí sửa chữa trường học",
            "signer_name", "Trần Văn Bình", "signer_title", "Chủ tịch", "content", "Trường Tiểu học Minh An xuống cấp.\nMái dột 8 phòng học.",
            "reply_to_number", "15/STC-NS ngày 02/11/2026", "proposal", "Bổ sung 500 triệu đồng trong quý IV/2026"),
        G("cong-van-du-phong", DocumentType.CongVan, "",
            "to_department", "Phòng Kinh tế", "chairman_name", "Lê Thị Hoa", "signer_title", "Phó Chủ tịch", "content", "Báo cáo tiến độ giải ngân."),
        G("quyet-dinh-day-du", DocumentType.QuyetDinh, "Quyết định khen thưởng",
            "person_name", "Phạm Văn Cường", "current_position", "Công chức Văn phòng", "from_unit", "Văn phòng UBND", "to_unit", "Ban Địa chính",
            "new_position", "Phó ban", "effective_date", "01/12/2026", "award_type", "Giấy khen", "recipient", "Thôn 3",
            "achievement", "Dẫn đầu phong trào xây dựng nông thôn mới", "org_name", "Tổ công tác chuyển đổi số", "members", "5 thành viên",
            "tasks", "Hướng dẫn người dân dùng dịch vụ công", "project_name", "Đề án chuyển đổi số", "objectives", "100% hồ sơ trực tuyến",
            "budget", "200 triệu đồng", "implementing_unit", "Văn phòng UBND xã", "signer_name", "Trần Văn Bình", "signer_title", "Chủ tịch"),
        G("quyet-dinh-chu-tich", DocumentType.QuyetDinh, "Quyết định điều động",
            "person_name", "Phạm Văn Cường", "chairman_name", "Lê Thị Hoa"),
        G("quyet-dinh-rong", DocumentType.QuyetDinh, ""),
        G("bao-cao-day-du", DocumentType.BaoCao, "Báo cáo tình hình tháng",
            "org_name", "UBND xã Minh An", "period", "Tháng 10/2026", "signer_name", "Trần Văn Bình", "signer_title", "Chủ tịch",
            "achievements", "Thu ngân sách đạt 92%.", "situation", "Mưa lớn kéo dài.", "results", "Sửa 3 km đường.",
            "challenges", "Thiếu biên chế.", "future_plans", "Hoàn thành giải ngân.", "proposals", "Hỗ trợ 200 triệu đồng."),
        G("bao-cao-linh-vuc", DocumentType.BaoCao, "Báo cáo chuyên đề",
            "org_name", "UBND xã Minh An", "field", "Phòng chống thiên tai", "situation", "Ngập cục bộ 2 thôn."),
        G("to-trinh-day-du", DocumentType.ToTrinh, "Tờ trình",
            "org_name", "UBND xã Minh An", "recipient", "UBND tỉnh Lâm Đồng", "subject", "Xin chủ trương xây nhà văn hóa",
            "reason", "Nhà văn hóa cũ xuống cấp.", "content", "Xây mới nhà văn hóa thôn 3.", "budget", "1,2 tỷ đồng",
            "signer_name", "Trần Văn Bình", "signer_title", "Chủ tịch"),
        G("to-trinh-de-xuat", DocumentType.ToTrinh, "Tờ trình đề xuất",
            "org_name", "UBND xã Minh An", "recipient", "Sở Nội vụ", "proposal", "Bổ sung 2 biên chế công chức", "reason", "Khối lượng công việc tăng."),
        G("ke-hoach-cong-tac", DocumentType.KeHoach, "Kế hoạch công tác",
            "org_name", "UBND xã Minh An", "period", "năm 2027", "objectives", "Hoàn thành 100% chỉ tiêu.",
            "tasks", "1. Chuyển đổi số.\n2. Cải cách hành chính.", "budget", "300 triệu đồng", "signer_name", "Trần Văn Bình", "signer_title", "Chủ tịch"),
        G("ke-hoach-su-kien", DocumentType.KeHoach, "Kế hoạch tổ chức sự kiện",
            "event_name", "Ngày hội Đại đoàn kết", "time_place", "8h00 ngày 18/11/2026, sân văn hóa thôn 2", "purpose", "Tăng cường đoàn kết.",
            "program", "Văn nghệ, thể thao."),
        G("ke-hoach-chuong-trinh", DocumentType.KeHoach, "Kế hoạch",
            "program", "Chương trình tập huấn", "period", "quý I/2027"),
        G("thong-bao-day-du", DocumentType.ThongBao, "Thông báo kết luận cuộc họp",
            "meeting_name", "Họp giao ban tháng 11", "event_name", "Giao ban", "time", "14h00 ngày 20/11/2026", "location", "Hội trường UBND xã",
            "participants", "Lãnh đạo UBND, công chức", "agenda", "Đánh giá tháng 11", "content", "Kết luận của Chủ tịch",
            "conclusion", "Hoàn thành giải ngân trước 15/12.", "tasks", "Văn phòng tổng hợp.", "signer_name", "Trần Văn Bình", "signer_title", "Chủ tịch"),
        G("thong-bao-rong", DocumentType.ThongBao, "Thông báo"),
        G("nghi-quyet-hdnd", DocumentType.NghiQuyet, "Nghị quyết HĐND về dự toán",
            "level", "xã", "subject", "Dự toán ngân sách 2027", "articles", "Điều 1. Thông qua dự toán.\nĐiều 2. Giao UBND tổ chức thực hiện.",
            "effective_date", "01/01/2027", "implementing_unit", "UBND xã Minh An", "chairman_name", "Nguyễn Thị Lan"),
        G("nghi-quyet-ubnd", DocumentType.NghiQuyet, "Nghị quyết UBND",
            "level", "xã", "subject", "Quy chế làm việc", "articles", "Điều 1. Ban hành quy chế.", "chairman_name", "Trần Văn Bình"),
        G("chi-thi", DocumentType.ChiThi, "Chỉ thị",
            "from_org", "UBND xã Minh An", "subject", "Tăng cường phòng cháy", "content", "Kiểm tra 100% cơ sở kinh doanh.",
            "signer_name", "Trần Văn Bình", "signer_title", "Chủ tịch"),
        G("quy-che", DocumentType.QuyChE, "Quy chế", "subject", "Quy chế làm việc", "content", "Phạm vi, đối tượng, chế độ họp."),
        G("quy-dinh", DocumentType.QuyDinh, "Quy định", "subject", "Quy định tiếp công dân", "content", "Lịch tiếp thứ Ba hằng tuần."),
        G("thong-cao", DocumentType.ThongCao, "Thông cáo", "subject", "Kết quả bầu cử", "content", "Tỷ lệ cử tri đi bầu 99%."),
        G("huong-dan", DocumentType.HuongDan, "Hướng dẫn", "subject", "Đăng ký tạm trú", "content", "Nộp hồ sơ trực tuyến.", "from_org", "Công an xã"),
        G("chuong-trinh", DocumentType.ChuongTrinh, "Chương trình", "subject", "Chương trình công tác năm", "content", "12 nhiệm vụ trọng tâm.", "timeline", "Năm 2027"),
        G("phuong-an", DocumentType.PhuongAn, "Phương án", "subject", "Phương án phòng chống bão", "objectives", "Không thiệt hại người",
            "content", "Sơ tán 50 hộ.", "budget", "50 triệu đồng"),
        G("de-an", DocumentType.DeAn, "Đề án", "subject", "Đề án chuyển đổi số", "objectives", "Chính quyền số", "content", "3 trụ cột.", "budget", "1 tỷ đồng"),
        G("du-an", DocumentType.DuAn, "Dự án", "subject", "Dự án đường liên thôn", "objectives", "Nhựa hóa 5 km", "content", "Làm mới mặt đường."),
        G("bien-ban", DocumentType.BienBan, "Biên bản", "subject", "Họp dân thôn 3", "time", "19h00 ngày 10/11/2026", "location", "Nhà văn hóa thôn 3",
            "attendees", "45 hộ dân", "content", "Thống nhất góp đất làm đường."),
        G("hop-dong", DocumentType.HopDong, "Hợp đồng", "subject", "Thuê bảo vệ", "party_a", "UBND xã Minh An", "party_b", "Công ty Bảo vệ An Toàn",
            "content", "Bảo vệ trụ sở 24/24."),
        G("cong-dien", DocumentType.CongDien, "Công điện", "subject", "Ứng phó bão số 9", "to_org", "Trưởng các thôn", "content", "Sơ tán dân vùng trũng."),
        G("ban-ghi-nho", DocumentType.BanGhiNho, "Bản ghi nhớ", "subject", "Hợp tác đào tạo", "content", "Đào tạo kỹ năng số.", "parties", "UBND xã và Trường Cao đẳng"),
        G("ban-thoa-thuan", DocumentType.BanThoaThuan, "Bản thỏa thuận", "subject", "Dùng chung sân bóng", "content", "Luân phiên theo tuần.", "parties", "Thôn 1 và Thôn 2"),
        G("giay-uy-quyen", DocumentType.GiayUyQuyen, "Giấy ủy quyền", "grantor", "Trần Văn Bình", "grantee", "Lê Thị Hoa", "content", "Ký văn bản thường xuyên.",
            "duration", "Từ 01/12 đến 31/12/2026"),
        G("giay-moi", DocumentType.GiayMoi, "Giấy mời", "subject", "Dự hội nghị tổng kết", "time", "8h00 ngày 28/12/2026", "location", "Hội trường UBND xã",
            "to_org", "Trưởng các thôn"),
        G("giay-gioi-thieu", DocumentType.GiayGioiThieu, "Giấy giới thiệu", "person", "Phạm Văn Cường", "to_org", "Sở Nội vụ", "content", "Liên hệ công tác biên chế."),
        G("giay-nghi-phep", DocumentType.GiayNghiPhep, "Giấy nghỉ phép", "person", "Lê Thị Hoa", "from_date", "01/12/2026", "to_date", "05/12/2026", "reason", "Nghỉ phép năm."),
        G("phieu-gui", DocumentType.PhieuGui, "Phiếu gửi", "to_org", "Văn phòng UBND tỉnh", "content", "Gửi báo cáo tháng.", "documents", "Báo cáo số 12/BC-UBND"),
        G("phieu-chuyen", DocumentType.PhieuChuyen, "Phiếu chuyển", "to_org", "Ban Địa chính", "content", "Chuyển đơn của công dân.", "documents", "Đơn ngày 03/11/2026"),
        G("phieu-bao", DocumentType.PhieuBao, "Phiếu báo", "to_org", "Các thôn", "content", "Báo lịch cắt điện.", "documents", "Thông báo của Điện lực"),
        G("thu-cong", DocumentType.ThuCong, "Thư công", "letter_type", "cảm ơn", "to_org", "Công ty Bảo vệ An Toàn", "content", "Cảm ơn hỗ trợ trong mùa mưa bão."),
        G("khac-chung", DocumentType.Khac, "Mẫu văn bản khác",
            "subject", "Xác nhận hộ nghèo", "so_ho_dan", "125", "ghi_chu_rieng", "   ", "signer_name", "", "location", "Thôn 4",
            "content", "Nội dung có \"ngoặc kép\", dấu \\ và ký tự ``` ."),
        G("luat", DocumentType.Luat, "Luật", "subject", "Luật mẫu", "content", "Nội dung luật."),
        G("nghi-dinh", DocumentType.NghiDinh, "Nghị định", "subject", "Nghị định mẫu", "content", "Nội dung nghị định."),
        G("thong-tu", DocumentType.ThongTu, "Thông tư", "subject", "Thông tư mẫu", "content", "Nội dung thông tư."),
    };
}
