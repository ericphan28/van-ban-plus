using System.Text.Json;
using AIVanBan.Core.Services;

namespace AIVanBan.Tests;

/// <summary>
/// settings.json của bản cũ còn các khóa của chế độ gọi thẳng Gemini (đã bỏ từ 1.0.20):
/// vẫn đọc được, và lần lưu sau không ghi lại các khóa đó.
/// (Dùng JsonSerializer như AppSettingsService; không đụng tệp settings thật của máy.)
/// </summary>
public class AppSettingsCuTests
{
    private const string JsonCu = """
    {
      "AiEnabled": true,
      "UseVanBanPlusApi": false,
      "VanBanPlusApiUrl": "https://vanbanplus.giakiemso.com",
      "VanBanPlusApiKey": "vbp_khoa_thu",
      "GeminiApiKey": "AIza-khoa-cu",
      "DevModeActivatedAt": "2026-10-01T08:00:00",
      "UserEmail": "a@b.vn"
    }
    """;

    [Fact]
    public void DocJsonCu_BoQuaKhoaDaBo_GiuCacTruongCon()
    {
        var s = JsonSerializer.Deserialize<AppSettings>(JsonCu);

        Assert.NotNull(s);
        Assert.True(s!.AiEnabled);
        Assert.Equal("vbp_khoa_thu", s.VanBanPlusApiKey);
        Assert.Equal("a@b.vn", s.UserEmail);
    }

    [Fact]
    public void LuuLai_KhongConKhoaDaBo()
    {
        var s = JsonSerializer.Deserialize<AppSettings>(JsonCu)!;

        var json = JsonSerializer.Serialize(s, new JsonSerializerOptions { WriteIndented = true });

        Assert.DoesNotContain("UseVanBanPlusApi", json);
        Assert.DoesNotContain("GeminiApiKey", json);
        Assert.DoesNotContain("DevModeActivatedAt", json);
        Assert.DoesNotContain("AIza-khoa-cu", json);
        Assert.Contains("vbp_khoa_thu", json);
    }

    [Theory]
    [InlineData("https://vanbanplus.vercel.app")]
    [InlineData("https://vanbanplus-git-main-gikidi.vercel.app/")]
    [InlineData("HTTPS://VanBanPlus.Vercel.App")]
    [InlineData("https://vercel.app")]
    public void DiaChiVercelCu_DoiSangDiaChiMacDinh(string url)
    {
        var s = new AppSettings { VanBanPlusApiUrl = url, VanBanPlusApiKey = "vbp_khoa_thu" };

        AppSettingsService.ChuanHoaDiaChiCu(s);

        Assert.Equal("https://vanbanplus.giakiemso.com", s.VanBanPlusApiUrl);
        Assert.Equal("vbp_khoa_thu", s.VanBanPlusApiKey);
    }

    [Theory]
    [InlineData("https://vanbanplus.giakiemso.com")]
    [InlineData("https://may-thu.example.vn")]
    [InlineData("http://localhost:3000")]
    [InlineData("https://notvercel.app.example.com")]
    [InlineData("https://vercel.app.evil.com")]
    [InlineData("khong-phai-url")]
    [InlineData("")]
    public void DiaChiKhac_GiuNguyen(string url)
    {
        var s = new AppSettings { VanBanPlusApiUrl = url };

        AppSettingsService.ChuanHoaDiaChiCu(s);

        Assert.Equal(url, s.VanBanPlusApiUrl);
    }
}
