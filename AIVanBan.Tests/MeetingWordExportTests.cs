using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using DocumentFormat.OpenXml.Packaging;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace AIVanBan.Tests;

/// <summary>Biên bản cuộc họp xuất Word: phần đầu theo NĐ 30/2020 Phụ lục I, khối ký không tách trang.</summary>
public class MeetingWordExportTests
{
    [Theory]
    [InlineData("Ủy ban nhân dân xã Hòa Bình", "Hòa Bình")]
    [InlineData("UBND xã A", "Xã A")]
    [InlineData("UBND phường 5", "Phường 5")]
    [InlineData("Ủy ban nhân dân thị trấn Gia Ray", "Gia Ray")]
    public void DiaDanh_suy_tu_ten_co_quan(string coQuan, string mong) =>
        Assert.Equal(mong, MeetingWordExportService.DiaDanh(coQuan));

    [Theory]
    [InlineData("Ủy ban nhân dân xã A", "UBND")]
    [InlineData("UBND xã A", "UBND")]
    [InlineData("Chi bộ CQ UBND xã", "")]
    public void Viet_tat_co_quan(string coQuan, string mong) =>
        Assert.Equal(mong, MeetingWordExportService.VietTatCoQuan(coQuan));

    [Fact]
    public void Bien_ban_co_phan_dau_hai_cot_so_ky_hieu_dia_danh_va_khoi_ky_khong_tach()
    {
        var m = new Meeting
        {
            Title = "Họp giao ban tháng 11",
            OrganizingUnit = "Ủy ban nhân dân xã A",
            StartTime = new DateTime(2026, 11, 20, 8, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 10, 30, 0),
            Location = "Phòng họp",
            ChairPerson = "Nguyễn Văn B",
            Secretary = "Trần Thị C",
            Attendees = new()
            {
                new MeetingAttendee { Name = "Nguyễn Văn B", Position = "Chủ tịch", Unit = "UBND xã A" },
                new MeetingAttendee { Name = "Trần Thị C", Position = "Văn phòng", Unit = "UBND xã A" },
                new MeetingAttendee { Name = "Lê Văn D", Position = "Phó Chủ tịch", Unit = "UBND xã A" },
            },
        };
        var tep = Path.Combine(Path.GetTempPath(), $"bb-{Guid.NewGuid():N}.docx");
        try
        {
            new MeetingWordExportService().ExportBienBan(m, tep);
            using var doc = WordprocessingDocument.Open(tep, false);
            var body = doc.MainDocumentPart!.Document.Body!;

            // Phần tử đầu là bảng phần đầu: trái tên cơ quan + số, phải Quốc hiệu + địa danh
            var dau = Assert.IsType<W.Table>(body.ChildElements.First());
            var hang = dau.Elements<W.TableRow>().ToList();
            var o = hang.Select(h => h.Elements<W.TableCell>().Select(c => c.InnerText).ToList()).ToList();
            Assert.Contains("ỦY BAN NHÂN DÂN XÃ A", o[0][0]);
            Assert.Contains("CỘNG HÒA XÃ HỘI CHỦ NGHĨA", o[0][1]);
            Assert.Contains("Độc lập - Tự do - Hạnh phúc", o[0][1]);
            Assert.Contains("/BB-UBND", o[1][0]);
            Assert.StartsWith("Số:", o[1][0]);
            Assert.Equal("Xã A, ngày 20 tháng 11 năm 2026", o[1][1]);

            // Bảng thành phần: vai trò suy từ ô Chủ trì / Thư ký của cuộc họp
            var tp = body.Elements<W.Table>().First(t => t.InnerText.Contains("Họ và tên"));
            var vai = tp.Elements<W.TableRow>().Skip(1).Select(h => h.Elements<W.TableCell>().Last().InnerText).ToList();
            Assert.Equal(new[] { "Chủ trì", "Thư ký", "Tham dự" }, vai);

            var chu = body.InnerText;
            Assert.Contains("Cuộc họp kết thúc vào 10 giờ 30, ngày 20 tháng 11 năm 2026.", chu);
            Assert.Contains("thống nhất nội dung trên./.", chu);

            // Khổ A4
            var kho = body.GetFirstChild<W.SectionProperties>()!.GetFirstChild<W.PageSize>()!;
            Assert.Equal(11906u, kho.Width!.Value);
            Assert.Equal(16838u, kho.Height!.Value);

            // Bảng cuối = khối ký: hàng không được tách trang
            var ky = body.Elements<W.Table>().Last();
            Assert.Contains("THƯ KÝ", ky.InnerText);
            Assert.All(ky.Elements<W.TableRow>(), h => Assert.NotNull(h.TableRowProperties?.GetFirstChild<W.CantSplit>()));
        }
        finally { File.Delete(tep); }
    }
}
