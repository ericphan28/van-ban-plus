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
}
