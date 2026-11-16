namespace AIVanBan.Core.Services;

/// <summary>
/// Địa phương của cơ quan, gửi trong dia_phuong của tác vụ soan-thao. Ghi cả chữ «xã», «phường», «tỉnh»...
/// CoQuanCapTren bỏ trống: máy chủ coi cấp trên là tỉnh. DacDiem: tối đa 10 dòng, mỗi dòng thành một ý trong prompt.
/// </summary>
public sealed record DiaPhuongCoQuan(string Xa, string CoQuanCapTren, string Tinh, IReadOnlyList<string> DacDiem)
{
    public DiaPhuongCoQuan(string xa, string coQuanCapTren, string tinh) : this(xa, coQuanCapTren, tinh, Array.Empty<string>()) { }

    // Record mặc định so danh sách bằng tham chiếu; so từng phần tử để hai địa phương cùng nội dung thì bằng nhau.
    public bool Equals(DiaPhuongCoQuan? other) =>
        other is not null && Xa == other.Xa && CoQuanCapTren == other.CoQuanCapTren && Tinh == other.Tinh
        && DacDiem.SequenceEqual(other.DacDiem);

    public override int GetHashCode() => HashCode.Combine(Xa, CoQuanCapTren, Tinh, DacDiem.Count);
}

/// <summary>
/// Đọc/ghi địa phương ở ExtraSettings của settings.json (tab «🏛️ Cơ quan» trong Cài đặt).
/// Chưa từng lưu → chỉ máy đã đặt địa danh mặc định «Gia Kiệm» mới dùng mặc định Gia Kiệm; máy khác → null
/// (v1.0.20 trả Gia Kiệm cho MỌI máy chưa lưu, nên khách gõ «UBND xã A» vẫn ra «UBND XÃ GIA KIỆM» — sửa ở v1.0.21).
/// Đã lưu mà thiếu xã hoặc thiếu tỉnh → null (không gửi dia_phuong; hợp đồng đòi cả hai khi có dia_phuong).
/// </summary>
public static class ThietLapDiaPhuong
{
    public const string KhoaXa = "Org.DiaPhuong.Xa";
    public const string KhoaCoQuanCapTren = "Org.DiaPhuong.CoQuanCapTren";
    public const string KhoaTinh = "Org.DiaPhuong.Tinh";
    /// <summary>Các dòng đặc điểm, nối bằng dấu xuống dòng.</summary>
    public const string KhoaDacDiem = "Org.DiaPhuong.DacDiem";

    /// <summary>Giới hạn của hợp đồng: tối đa 10 dòng đặc điểm, mỗi chuỗi tối đa 300 ký tự.</summary>
    public const int DacDiemToiDa = 10;
    public const int DoDaiToiDa = 300;

    /// <summary>Địa danh ban hành mặc định (tab Cơ quan), ví dụ «Gia Kiệm».</summary>
    public const string KhoaDiaDanh = "Org.DefaultLocation";

    // Chính quyền hai cấp từ 01/7/2025: không còn cấp trung gian giữa xã và tỉnh.
    public static readonly DiaPhuongCoQuan MacDinhGiaKiem = new(
        "xã Gia Kiệm", "", "tỉnh Đồng Nai",
        new[]
        {
            "Dân số: 79.274 nhân khẩu, 19.818 hộ",
            "17 ấp, 5 giáo xứ Công giáo (96% dân theo đạo)",
            "Xã loại I, đang xây dựng nông thôn mới nâng cao",
        });

    public static DiaPhuongCoQuan? Doc(IReadOnlyDictionary<string, string> extra)
    {
        var daLuu = extra.ContainsKey(KhoaXa) || extra.ContainsKey(KhoaCoQuanCapTren)
            || extra.ContainsKey(KhoaTinh) || extra.ContainsKey(KhoaDacDiem);
        if (!daLuu)
        {
            var diaDanh = extra.TryGetValue(KhoaDiaDanh, out var d) ? (d ?? "").Normalize() : "";
            return diaDanh.Contains("Gia Kiệm".Normalize(), StringComparison.OrdinalIgnoreCase) ? MacDinhGiaKiem : null;
        }

        string Lay(string khoa) => Cat(extra.TryGetValue(khoa, out var v) ? v : null);
        var xa = Lay(KhoaXa);
        var tinh = Lay(KhoaTinh);
        if (xa.Length == 0 || tinh.Length == 0) return null;

        extra.TryGetValue(KhoaDacDiem, out var dacDiem);
        return new DiaPhuongCoQuan(xa, Lay(KhoaCoQuanCapTren), tinh, TachDacDiem(dacDiem));
    }

    public static DiaPhuongCoQuan? Doc() => Doc(AppSettingsService.Load().ExtraSettings);

    /// <summary>
    /// Địa danh ghi ở dòng «…, ngày … tháng … năm …» và ở trường Location khi lưu văn bản:
    /// địa danh mặc định của tab Cơ quan; không có thì tên xã/phường đã lưu (bỏ chữ «xã», «phường»…); không có nữa → "".
    /// </summary>
    public static string DiaDanh(IReadOnlyDictionary<string, string> extra)
    {
        var d = Cat(extra.TryGetValue(KhoaDiaDanh, out var v) ? v : null);
        if (d.Length > 0) return d;
        var dp = Doc(extra);
        return dp is null ? "" : BoTienToXa(dp.Xa);
    }

    public static string DiaDanh() => DiaDanh(AppSettingsService.Load().ExtraSettings);

    /// <summary>«xã Gia Kiệm» → «Gia Kiệm» (bỏ tiền tố xã, phường, thị trấn, đặc khu).</summary>
    public static string BoTienToXa(string xa)
    {
        foreach (var p in new[] { "xã ", "phường ", "thị trấn ", "đặc khu " })
            if (xa.StartsWith(p, StringComparison.OrdinalIgnoreCase)) return xa[p.Length..].Trim();
        return xa.Trim();
    }

    /// <summary>Ghi vào từ điển (tách riêng để kiểm thử mà không đụng settings.json thật). Luôn ghi đủ bốn khóa.</summary>
    public static void GhiVao(IDictionary<string, string> extra, string? xa, string? coQuanCapTren, string? tinh, string? dacDiem)
    {
        extra[KhoaXa] = Cat(xa);
        extra[KhoaCoQuanCapTren] = Cat(coQuanCapTren);
        extra[KhoaTinh] = Cat(tinh);
        extra[KhoaDacDiem] = string.Join("\n", TachDacDiem(dacDiem));
    }

    public static void Luu(string xa, string coQuanCapTren, string tinh, string dacDiem)
    {
        var s = AppSettingsService.Load();
        GhiVao(s.ExtraSettings, xa, coQuanCapTren, tinh, dacDiem);
        AppSettingsService.Save(s);
    }

    /// <summary>Tách theo dòng, cắt khoảng trắng, bỏ dòng trống, tối đa 10 dòng, mỗi dòng tối đa 300 ký tự.</summary>
    public static IReadOnlyList<string> TachDacDiem(string? text) =>
        (text ?? "").Split('\n')
            .Select(Cat)
            .Where(d => d.Length > 0)
            .Take(DacDiemToiDa)
            .ToArray();

    private static string Cat(string? v)
    {
        var t = (v ?? "").Trim();
        return t.Length > DoDaiToiDa ? t[..DoDaiToiDa].TrimEnd() : t;
    }
}
