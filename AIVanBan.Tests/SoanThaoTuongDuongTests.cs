using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using AIVanBan.Desktop.Views;

namespace AIVanBan.Tests;

/// <summary>
/// Bằng chứng tách prompt soạn thảo sang AIVanBan.Core không đổi hành vi (spec mục 10).
/// Lần chạy đầu (trên hộp thoại CŨ, Task 5 kế hoạch đợt 2b) chụp prompt vào MauCu/soan-thao-gia-kiem.json; từ đó mọi
/// lần chạy so hộp thoại (và lớp mới) với bản chụp. KHÔNG xóa / sinh lại bản chụp sau khi hộp thoại đã sửa.
/// </summary>
public class SoanThaoTuongDuongTests
{
    private const BindingFlags RIENG = BindingFlags.NonPublic | BindingFlags.Instance;

    private static readonly JsonSerializerOptions TUY_CHON = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    internal sealed record Chup(string ten, string he_thong, string nguoi_dung);

    internal static string TepChup()
    {
        for (var d = new DirectoryInfo(AppContext.BaseDirectory); d != null; d = d.Parent)
        {
            if (File.Exists(Path.Combine(d.FullName, "AIVanBan.sln")))
                return Path.Combine(d.FullName, "AIVanBan.Tests", "MauCu", "soan-thao-gia-kiem.json");
        }
        throw new DirectoryNotFoundException($"Không tìm thấy AIVanBan.sln khi đi ngược từ {AppContext.BaseDirectory}");
    }

    internal static string ChuanHoa(string s) => s.Replace("\r\n", "\n");

    internal static List<Chup> DocChup() =>
        JsonSerializer.Deserialize<List<Chup>>(File.ReadAllText(TepChup())) ?? throw new InvalidDataException("Bản chụp rỗng");

    /// <summary>
    /// Gọi hai hàm prompt riêng tư của hộp thoại trên một đối tượng CHƯA khởi tạo (không InitializeComponent, không cần
    /// giao diện). Hai hàm này chỉ đọc _selectedTemplate và tham số, nên đủ để tái tạo prompt. Chạy trên luồng STA cho chắc.
    /// </summary>
    private static (string HeThong, string NguoiDung) QuaHopThoai(MauSoanThao.Ca ca)
    {
        (string, string) kq = default;
        Exception? loi = null;
        var t = new Thread(() =>
        {
            try
            {
                var kieu = typeof(AIComposeDialog);
                var dlg = RuntimeHelpers.GetUninitializedObject(kieu);
                kieu.GetField("_selectedTemplate", RIENG)!
                    .SetValue(dlg, new DocumentTemplate { Type = ca.Loai, Name = ca.TenMau ?? "" });
                var heThong = (string)kieu.GetMethod("BuildSystemInstruction", RIENG, Type.EmptyTypes)!.Invoke(dlg, null)!;
                var nguoiDung = (string)kieu.GetMethod("BuildStructuredPrompt", RIENG)!.Invoke(dlg, new object[] { ca.Loai, ca.TuDien() })!;
                kq = (heThong, nguoiDung);
            }
            catch (Exception e)
            {
                loi = e;
            }
        });
        t.SetApartmentState(ApartmentState.STA);
        t.Start();
        t.Join();
        if (loi != null) throw new InvalidOperationException($"Không gọi được hàm prompt của hộp thoại (ca {ca.Ten})", loi);
        return kq;
    }

    [Fact]
    public void HopThoaiKhopBanChup()
    {
        var hienTai = MauSoanThao.GiaKiem.Select(ca =>
        {
            var (h, n) = QuaHopThoai(ca);
            return new Chup(ca.Ten, ChuanHoa(h), ChuanHoa(n));
        }).ToList();

        var duongDan = TepChup();
        if (!File.Exists(duongDan))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(duongDan)!);
            File.WriteAllText(duongDan, JsonSerializer.Serialize(hienTai, TUY_CHON).Replace("\r\n", "\n") + "\n");
            Assert.Fail($"Vừa chụp prompt của hộp thoại vào {duongDan}: kiểm tra, commit, rồi chạy lại.");
        }

        var chup = DocChup();
        Assert.Equal(chup.Select(c => c.ten), hienTai.Select(c => c.ten));
        for (var i = 0; i < chup.Count; i++)
        {
            Assert.True(chup[i].he_thong == hienTai[i].he_thong, $"{chup[i].ten}: he_thong lệch bản chụp");
            Assert.True(chup[i].nguoi_dung == hienTai[i].nguoi_dung, $"{chup[i].ten}: nguoi_dung lệch bản chụp");
        }
    }

    [Fact]
    public void LopMoiKhopBanChup()
    {
        var chup = DocChup();
        Assert.Equal(chup.Select(c => c.ten), MauSoanThao.GiaKiem.Select(c => c.Ten));
        foreach (var (ca, c) in MauSoanThao.GiaKiem.Zip(chup))
        {
            var h = ChuanHoa(ComposePromptBuilder.BuildSystemInstruction(ca.Loai, ca.DiaPhuong, 2026));
            var n = ChuanHoa(ComposePromptBuilder.BuildUserPrompt(ca.Loai, ca.TenMau, ca.TuDien(), ca.DiaPhuong));
            Assert.True(c.he_thong == h, $"{ca.Ten}: he_thong của lớp mới lệch hộp thoại cũ");
            Assert.True(c.nguoi_dung == n, $"{ca.Ten}: nguoi_dung của lớp mới lệch hộp thoại cũ");
        }
    }
}
