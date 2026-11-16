using System.Text.Encodings.Web;
using System.Text.Json;
using AIVanBan.Core.Services;

namespace AIVanBan.Tests;

/// <summary>
/// Sinh tệp mẫu prompt cho vanbanplus-api (tests/mau-prompt/*.json): đầu vào + system prompt + user prompt
/// do chính mã C# dựng. Bộ kiểm TypeScript so từng ký tự với prompt dựng ở máy chủ.
/// Xuống dòng chuẩn hóa về \n vì chuỗi @"..." mang CRLF của tệp nguồn khi build trên Windows.
/// </summary>
public class MauPromptTests
{
    /// <summary>Đi ngược từ thư mục build lên tới gốc repo (nơi có vanbanplus-api/package.json); không thấy thì báo lỗi.</summary>
    private static string TimThuMucMau()
    {
        for (var d = new DirectoryInfo(AppContext.BaseDirectory); d != null; d = d.Parent)
        {
            if (File.Exists(Path.Combine(d.FullName, "vanbanplus-api", "package.json")))
                return Path.Combine(d.FullName, "vanbanplus-api", "tests", "mau-prompt");
        }
        throw new DirectoryNotFoundException(
            $"Không tìm thấy vanbanplus-api/package.json khi đi ngược từ {AppContext.BaseDirectory}");
    }

    private static string ChuanHoa(string s) => s.Replace("\r\n", "\n");

    /// <summary>
    /// Ghi tệp mẫu. Nội dung khác bản đang có thì vẫn ghi bản mới nhưng làm hỏng bài kiểm,
    /// để nhớ commit tệp mẫu mới bên vanbanplus-api (bộ kiểm TypeScript đọc tệp đã commit).
    /// </summary>
    private static void Ghi(string ten, object duLieu)
    {
        var thuMuc = TimThuMucMau();
        Directory.CreateDirectory(thuMuc);
        var duongDan = Path.Combine(thuMuc, ten);
        var json = JsonSerializer.Serialize(duLieu, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        }).Replace("\r\n", "\n") + "\n";
        var cu = File.Exists(duongDan) ? File.ReadAllText(duongDan).Replace("\r\n", "\n") : null;
        if (cu == json) return;
        File.WriteAllText(duongDan, json);
        Assert.Fail($"Tệp mẫu {duongDan} vừa được sinh lại vì prompt C# đổi: commit tệp này bên vanbanplus-api.");
    }

    [Fact]
    public void GhiMauTomTat()
    {
        var cacMau = new[]
        {
            (NoiDung: "Điều 1. Phê duyệt kế hoạch.\nĐiều 2. Giao Văn phòng theo dõi.", Loai: "Quyết định", TieuDe: "Về việc phê duyệt kế hoạch năm 2026", CoQuan: "UBND xã Minh An"),
            (NoiDung: "Kính gửi: Các phòng chuyên môn.\nĐề nghị báo cáo trước ngày 20/11/2026.", Loai: "", TieuDe: "", CoQuan: ""),
            (NoiDung: "Nội dung có \"ngoặc kép\", dấu \\ và ký tự ```.", Loai: "Công văn", TieuDe: "", CoQuan: "Sở Nội vụ"),
        };

        var mau = cacMau.Select(m => new
        {
            dau_vao = new { noi_dung = m.NoiDung, loai_van_ban = m.Loai, tieu_de = m.TieuDe, co_quan = m.CoQuan },
            he_thong = ChuanHoa(DocumentSummaryService.BuildSystemPrompt()),
            nguoi_dung = ChuanHoa(DocumentSummaryService.BuildUserPrompt(m.NoiDung, m.Loai, m.TieuDe, m.CoQuan)),
        }).ToList();

        Ghi("tom-tat.json", mau);
    }

    [Fact]
    public void GhiMauKiemTra()
    {
        var cacMau = new (string NoiDung, string Loai, string TieuDe, string CoQuan, string? DoiChieu)[]
        {
            ("ỦY BAN NHÂN DÂN XÃ MINH AN\nSố: 15/QĐ-UBND\nQUYẾT ĐỊNH\nVề việc thành lập Tổ công tác chuyển đổi số\nĐiều 1. Thành lập Tổ công tác.\nĐiều 2. Giao Văn phòng theo dõi, xử lý nghiêm.",
             "Quyết định", "Về việc thành lập Tổ công tác chuyển đổi số", "UBND xã Minh An",
             "QUYẾT ĐỊNH\nVề việc thành lập Tổ công tác\nĐiều 1. Thành lập Tổ công tác gồm các ông (bà) có tên sau.\nĐiều 2. Nhiệm vụ của Tổ công tác.\nĐiều 3. Chánh Văn phòng và các cá nhân có tên tại Điều 1 chịu trách nhiệm thi hành Quyết định này.\nNơi nhận:\n- Như Điều 3;\n- Lưu: VT, VP."),
            ("Kính gửi: Các phòng chuyên môn.\nĐề nghị báo cáo trước ngày 20/11/2026.", "", "", "", null),
            ("Nội dung có \"ngoặc kép\", dấu \\ , ký tự ``` và ${bien}.", "  ", "Tiêu đề `lạ`", "Sở Nội vụ", "   "),
        };

        var mau = cacMau.Select(m => new
        {
            dau_vao = new { noi_dung = m.NoiDung, loai_van_ban = m.Loai, tieu_de = m.TieuDe, co_quan = m.CoQuan, van_ban_doi_chieu = m.DoiChieu },
            he_thong = ChuanHoa(DocumentReviewService.BuildSystemPrompt()),
            nguoi_dung = ChuanHoa(DocumentReviewService.BuildUserPrompt(m.NoiDung, m.Loai, m.TieuDe, m.CoQuan, m.DoiChieu)),
        }).ToList();

        Ghi("kiem-tra.json", mau);
    }

    [Fact]
    public void GhiMauThamMuu()
    {
        // Ép văn hóa vi-VN: C# định dạng {ngày:dd/MM/yyyy} theo CurrentCulture (dấu "/" là dấu ngăn ngày của văn hóa).
        var vanHoaCu = System.Globalization.CultureInfo.CurrentCulture;
        System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("vi-VN");
        try
        {
            // 09:30 ngày 16/11/2026 giờ máy người dùng (Việt Nam, UTC+7) = 02:30 UTC.
            var bayGio = new DateTime(2026, 11, 16, 9, 30, 0);
            const string bayGioIso = "2026-11-16T02:30:00.000Z";

            static string? Iso(DateTime? d) =>
                d?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            static object NguCanhJson(DocumentAdvisoryContext c) => new
            {
                huong = c.Direction,
                nhom = c.Category,
                so_van_ban = c.DocumentNumber,
                ngay_ban_hanh = Iso(c.IssuedDate),
                do_khan = c.UrgencyLevel,
                do_mat = c.SecurityLevel,
                han_xu_ly = Iso(c.DueDate),
                nguoi_ky = c.SignerName,
                chuc_danh_ky = c.SignerTitle,
                noi_nhan = c.Recipients,
                giao_cho = c.AssignedTo,
                ghi_chu_xu_ly = c.ProcessingNotes,
                can_cu = c.BasedOn,
                van_ban_lien_quan = c.RelatedDocumentsSummary,
            };

            // Đủ 14 trường; hạn còn 3,6 ngày → «3 ngày nữa».
            var day = new DocumentAdvisoryContext
            {
                Direction = "Đến",
                Category = "Đất đai",
                DocumentNumber = "123/UBND-KT",
                IssuedDate = new DateTime(2026, 11, 3),
                UrgencyLevel = "Khẩn",
                SecurityLevel = "Mật",
                DueDate = new DateTime(2026, 11, 20),
                SignerName = "Nguyễn Văn A",
                SignerTitle = "Phó Chủ tịch",
                Recipients = new[] { "UBND các xã, phường", "Phòng Kinh tế" },
                AssignedTo = "Địa chính - XD - MT",
                ProcessingNotes = "Giao Địa chính chủ trì, báo cáo trước hạn.",
                BasedOn = new[] { "Luật Đất đai 2024", "Nghị định 102/2024/NĐ-CP" },
                RelatedDocumentsSummary = "CV 98/UBND-KT ngày 10/10/2026 về rà soát đất công.\nĐã báo cáo lần 1.",
            };
            // «Thường» bị bỏ qua; chức danh không có người ký thì bỏ; mảng rỗng bỏ; hạn đã qua 6,4 ngày → «-6 ngày nữa».
            var mot = new DocumentAdvisoryContext
            {
                Direction = "Đến",
                UrgencyLevel = "Thường",
                SecurityLevel = "Thường",
                DueDate = new DateTime(2026, 11, 10),
                SignerTitle = "Chủ tịch",
                Recipients = Array.Empty<string>(),
                BasedOn = Array.Empty<string>(),
            };
            // Hạn đúng hôm nay (còn −0,4 ngày) → «0 ngày nữa»; người ký không chức danh.
            var homNay = new DocumentAdvisoryContext
            {
                SignerName = "Trần Thị B",
                DueDate = new DateTime(2026, 11, 16),
                IssuedDate = new DateTime(2026, 1, 5),
            };

            var cacMau = new (string NoiDung, string Loai, string TieuDe, string CoQuan, DocumentAdvisoryContext? NguCanh)[]
            {
                ("Đề nghị UBND các xã rà soát đất công, báo cáo trước ngày 20/11/2026.", "Công văn", "V/v rà soát đất công", "UBND tỉnh Đồng Nai", day),
                ("Thông báo lịch tiếp công dân tháng 12/2026.", "Thông báo", "", "Văn phòng UBND tỉnh", new DocumentAdvisoryContext()),
                ("Kính gửi: UBND xã.\nĐề nghị báo cáo số liệu dân số.", "", "", "", null),
                ("Nhắc lại hạn báo cáo; ký tự \"ngoặc kép\", \\ , ``` và ${bien}.", "Công văn", "Nhắc hạn", "Sở Nội vụ", mot),
                ("Mời họp giao ban.", "Giấy mời", "Mời họp", "UBND xã", homNay),
            };

            var mau = cacMau.Select(m => new
            {
                bay_gio = bayGioIso,
                dau_vao = new
                {
                    noi_dung = m.NoiDung,
                    loai_van_ban = m.Loai,
                    tieu_de = m.TieuDe,
                    co_quan = m.CoQuan,
                    ngu_canh = m.NguCanh == null ? null : NguCanhJson(m.NguCanh),
                },
                he_thong = ChuanHoa(DocumentAdvisoryService.BuildSystemPrompt()),
                nguoi_dung = ChuanHoa(DocumentAdvisoryService.BuildUserPrompt(m.NoiDung, m.Loai, m.TieuDe, m.CoQuan, m.NguCanh, bayGio)),
            }).ToList();

            Ghi("tham-muu.json", mau);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = vanHoaCu;
        }
    }
}
