using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AIVanBan.Core.Services;
using AIVanBan.Tests.HoTro;

namespace AIVanBan.Tests;

public class TrichXuatServiceTests
{
    private static TrichXuatService TaoDichVu(HandlerTacVuGia h) =>
        new(new TacVuClient(h, "https://vbp.thu", "khoa-thu", "1.0.20"));

    private static JsonElement Than(HandlerTacVuGia h, int i = 0)
    {
        using var doc = JsonDocument.Parse(h.DaNhan[i].Body);
        return doc.RootElement.Clone();
    }

    /// <summary>Tạo ảnh BMP 2x2 thật bằng bộ mã hóa WPF.</summary>
    private static byte[] TaoBmpNho()
    {
        var pixels = new byte[] { 255, 0, 0, 255, 0, 255, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255 };
        var nguon = BitmapSource.Create(2, 2, 96, 96, PixelFormats.Bgra32, null, pixels, 8);
        var encoder = new BmpBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(nguon));
        using var ms = new MemoryStream();
        encoder.Save(ms);
        return ms.ToArray();
    }

    private static readonly byte[] ChuKyPng = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    [Fact]
    public async Task GhepTrang_GuiMotYeuCauDuCacTep_VaDocKetQuaCauTruc()
    {
        var h = new HandlerTacVuGia().ThanhCong(
            """{"so_van_ban":"15/GM-UBND","loai_van_ban":"GiayMoi","noi_dung":"Điều 1. A.\nĐiều 2. B.\nĐiều 3. C.","noi_nhan":["Như trên","Lưu: VT"],"can_cu":[]}""");
        var trang = new List<(string, string)> { ("AQID", "image/png"), ("BAUG", "image/jpeg") };

        var kq = await TaoDichVu(h).ExtractDocumentFromMultipleFilesAsync(trang);

        Assert.Equal("15/GM-UBND", kq.SoVanBan);
        Assert.Equal("GiayMoi", kq.LoaiVanBan);
        Assert.Equal("Điều 1. A.\nĐiều 2. B.\nĐiều 3. C.", kq.NoiDung);
        Assert.Equal(new[] { "Như trên", "Lưu: VT" }, kq.NoiNhan);
        Assert.Equal("Thuong", kq.DoKhan);
        var y = Assert.Single(h.DaNhan);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/trich-xuat", y.Url);
        var b = Than(h);
        Assert.Equal("cau-truc", b.GetProperty("che_do").GetString());
        var tep = b.GetProperty("tep");
        Assert.Equal(2, tep.GetArrayLength());
        Assert.Equal("BAUG", tep[1].GetProperty("base64").GetString());
        Assert.Equal("image/jpeg", tep[1].GetProperty("mime").GetString());
    }

    [Fact]
    public async Task DocChu_GuiCheDoChiChuVaTraChuoi()
    {
        var h = new HandlerTacVuGia().ThanhCong("\"GIẤY MỜI\\nHọp giao ban\"");

        var kq = await TaoDichVu(h).ReadTextAsync("JVBERi0=", "application/pdf");

        Assert.Equal("GIẤY MỜI\nHọp giao ban", kq);
        var b = Than(h);
        Assert.Equal("chi-chu", b.GetProperty("che_do").GetString());
        Assert.Equal(1, b.GetProperty("tep").GetArrayLength());
        Assert.Equal("application/pdf", b.GetProperty("tep")[0].GetProperty("mime").GetString());
    }

    [Fact]
    public async Task QuaHaiMuoiTep_BaoLoiTruocKhiGui()
    {
        var h = new HandlerTacVuGia();
        var trang = Enumerable.Range(0, 21).Select(_ => ("AQID", "image/png")).ToList();

        await Assert.ThrowsAsync<ArgumentException>(() => TaoDichVu(h).ExtractDocumentFromMultipleFilesAsync(trang));

        Assert.Empty(h.DaNhan);
    }

    [Fact]
    public async Task UocTinh_CungThanVoiLuotChay()
    {
        var h = new HandlerTacVuGia().TraVe(System.Net.HttpStatusCode.OK,
            """{"success":true,"data":{"credit_min":3,"credit_max":9,"so_du":50,"du_credit":true}}""");

        var u = await TaoDichVu(h).UocTinhAsync(new[] { ("AQID", "image/png") }, chiChu: true);

        Assert.Equal(9, u.CreditMax);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/trich-xuat/uoc-tinh", h.DaNhan[0].Url);
        Assert.Equal("chi-chu", Than(h).GetProperty("che_do").GetString());
    }

    [Fact]
    public void DocTep_DoanMimeTheoDuoi()
    {
        var duongDan = Path.Combine(Path.GetTempPath(), $"thu-trich-xuat-{Guid.NewGuid():N}.pdf");
        File.WriteAllBytes(duongDan, new byte[] { 1, 2, 3 });
        try
        {
            var (base64, mime) = TrichXuatService.DocTep(duongDan);
            Assert.Equal("AQID", base64);
            Assert.Equal("application/pdf", mime);
        }
        finally
        {
            File.Delete(duongDan);
        }
    }

    [Fact]
    public void DocTep_BmpDoiSangPng()
    {
        var duongDan = Path.Combine(Path.GetTempPath(), $"thu-trich-xuat-{Guid.NewGuid():N}.bmp");
        File.WriteAllBytes(duongDan, TaoBmpNho());
        try
        {
            var (base64, mime) = TrichXuatService.DocTep(duongDan);

            Assert.Equal("image/png", mime);
            Assert.Equal(ChuKyPng, Convert.FromBase64String(base64).Take(8).ToArray());
        }
        finally
        {
            File.Delete(duongDan);
        }
    }

    [Fact]
    public void ChuanBiTep_BmpBangMime_DoiSangPng_PngGiuNguyen()
    {
        var (b64, mime) = TrichXuatService.ChuanBiTep(TaoBmpNho(), "image/bmp");
        Assert.Equal("image/png", mime);
        Assert.Equal(ChuKyPng, Convert.FromBase64String(b64).Take(8).ToArray());

        var goc = new byte[] { 1, 2, 3 };
        var (b64Png, mimePng) = TrichXuatService.ChuanBiTep(goc, "image/png");
        Assert.Equal("image/png", mimePng);
        Assert.Equal("AQID", b64Png);
    }

    [Fact]
    public void ChuanBiTep_AnhHong_VaDinhDangLa_NemArgumentException()
    {
        Assert.Throws<ArgumentException>(() => TrichXuatService.ChuanBiTep(new byte[] { 1, 2, 3 }, "image/bmp"));
        Assert.Throws<ArgumentException>(() => TrichXuatService.ChuanBiTep(new byte[] { 1, 2, 3 }, "application/octet-stream"));
    }

    [Fact]
    public async Task MimeNgoaiDanhSach_BaoLoiTruocKhiGui()
    {
        var h = new HandlerTacVuGia();

        await Assert.ThrowsAsync<ArgumentException>(
            () => TaoDichVu(h).ExtractDocumentFromMultipleFilesAsync(new[] { ("AQID", "image/gif") }));

        Assert.Empty(h.DaNhan);
    }

    [Fact]
    public async Task TongDungLuongQua20MB_BaoLoiRoTruocKhiGui_CaUocTinhVaChay()
    {
        var h = new HandlerTacVuGia();
        var lon = new string('A', 28 * 1024 * 1024); // 21 MB sau giải base64
        var tep = new[] { (lon, "application/pdf") };

        var loi1 = await Assert.ThrowsAsync<ArgumentException>(() => TaoDichVu(h).UocTinhAsync(tep));
        var loi2 = await Assert.ThrowsAsync<ArgumentException>(() => TaoDichVu(h).ExtractDocumentFromMultipleFilesAsync(tep));

        Assert.Contains("20 MB", loi1.Message);
        Assert.Contains("20 MB", loi2.Message);
        Assert.Empty(h.DaNhan);
    }

    [Fact]
    public async Task DungSat20MB_VanGui()
    {
        var h = new HandlerTacVuGia().ThanhCong("""{"noi_dung":"x"}""");
        // 27.962.024 ký tự base64 -> 20.971.518 byte, dưới trần 20 MB
        var vuaDu = new string('A', 27962024);

        var kq = await TaoDichVu(h).ExtractDocumentFromMultipleFilesAsync(new[] { (vuaDu, "application/pdf") });

        Assert.Equal("x", kq.NoiDung);
        Assert.Single(h.DaNhan);
    }

    [Fact]
    public async Task ChiChuKhacMotTep_BaoLoiTruocKhiGui()
    {
        var h = new HandlerTacVuGia();

        await Assert.ThrowsAsync<ArgumentException>(
            () => TaoDichVu(h).UocTinhAsync(new[] { ("AQID", "image/png"), ("AQID", "image/png") }, chiChu: true));

        Assert.Empty(h.DaNhan);
    }

    /// <summary>ket_qua là chuỗi thô (hợp đồng): bọc văn bản đã cắt thành một chuỗi JSON hợp lệ.</summary>
    private static HandlerTacVuGia TraChuoiTho(string thoBiCat) =>
        new HandlerTacVuGia().ThanhCong(JsonSerializer.Serialize(thoBiCat), biCat: true);

    [Fact]
    public async Task KetQuaBiCat_CatTrongNoiDung_LayPhanNoiDungDaGiaiMa_KhongDoRaJson()
    {
        // Cắt giữa noi_dung, không có dấu nháy đóng; có \n, \", \\, \t, á và escape \u00 dở dang ở cuối.
        var tho = """{"so_van_ban":"15/GM-UBND","trich_yeu":"V/v họp","noi_dung":"Điều 1. Một.\nĐiều 2. Hai \"trích\" \\ giá\tx.\nĐiều 3. Ba cắt dở \u00""";
        var dichVu = TaoDichVu(TraChuoiTho(tho));

        var kq = await dichVu.ExtractDocumentFromBytesAsync("AQID", "image/png");

        Assert.Equal("15/GM-UBND", kq.SoVanBan);
        Assert.Equal("V/v họp", kq.TrichYeu);
        Assert.Equal("Điều 1. Một.\nĐiều 2. Hai \"trích\" \\ giá\tx.\nĐiều 3. Ba cắt dở", kq.NoiDung.TrimEnd());
        Assert.DoesNotContain("so_van_ban", kq.NoiDung);
        Assert.Equal("Thuong", kq.DoKhan);
        Assert.Equal("Thuong", kq.DoMat);
        Assert.True(dichVu.LuotCuoi!.BiCat);
        Assert.Equal(7, dichVu.LuotCuoi.CreditDaTru);
    }

    [Fact]
    public async Task KetQuaBiCat_CatSauDoKhanDoMat_GiuGiaTriDaVot_BoDauGachCheoCuoi()
    {
        var tho = """{"do_khan":"HoaToc","do_mat":"Mat","noi_dung":"Điều 1. A.\nĐiều 2. B.\nĐiều 3. C\""";
        var kq = await TaoDichVu(TraChuoiTho(tho)).ExtractDocumentFromBytesAsync("AQID", "image/png");

        Assert.Equal("HoaToc", kq.DoKhan);
        Assert.Equal("Mat", kq.DoMat);
        Assert.Equal("Điều 1. A.\nĐiều 2. B.\nĐiều 3. C", kq.NoiDung.TrimEnd());
    }

    [Fact]
    public async Task KetQuaBiCat_CatTruocNoiDung_KhongCoKhoa_DungNguyenVan()
    {
        var tho = """{"so_van_ban":"15/GM-UBND","trich_yeu":"V/v họp""";
        var kq = await TaoDichVu(TraChuoiTho(tho)).ExtractDocumentFromBytesAsync("AQID", "image/png");

        Assert.Equal("15/GM-UBND", kq.SoVanBan);
        Assert.Equal(tho, kq.NoiDung);
        Assert.Equal("Thuong", kq.DoKhan);
    }

    [Fact]
    public async Task KetQuaCauTruc_GhiLuotCuoi()
    {
        var h = new HandlerTacVuGia().ThanhCong("""{"so_van_ban":"1/QĐ"}""", creditDaTru: 4, soDu: 96);
        var dichVu = TaoDichVu(h);

        await dichVu.ExtractDocumentFromBytesAsync("AQID", "image/png");

        Assert.Equal(4, dichVu.LuotCuoi!.CreditDaTru);
        Assert.Equal(96, dichVu.LuotCuoi.SoDu);
        Assert.False(dichVu.LuotCuoi.BiCat);
    }

    [Fact]
    public void DocTep_TepQua20MB_BaoLoiTruocKhiDoc()
    {
        var duongDan = Path.Combine(Path.GetTempPath(), $"thu-trich-xuat-{Guid.NewGuid():N}.pdf");
        using (var f = new FileStream(duongDan, FileMode.Create))
            f.SetLength(TrichXuatService.TongByteToiDa + 1);
        try
        {
            var loi = Assert.Throws<ArgumentException>(() => TrichXuatService.DocTep(duongDan));
            Assert.Contains("20 MB", loi.Message);
        }
        finally
        {
            File.Delete(duongDan);
        }
    }
}
