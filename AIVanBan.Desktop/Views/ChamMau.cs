using System.Windows.Controls;
using System.Windows.Media;

namespace AIVanBan.Desktop.Views;

/// <summary>
/// WPF không vẽ được emoji màu: 🔴 🟡 🟢 🔵 🟠 ⚪ hiện thành vòng tròn đen trắng có gạch sọc. Thay bằng «●» (chữ thường của phông)
/// tô màu bằng Foreground.
/// </summary>
public static class ChamMau
{
    public const string Cham = "●";

    public static readonly SolidColorBrush Do = TaoMau(198, 40, 40);
    public static readonly SolidColorBrush Vang = TaoMau(249, 168, 37);
    public static readonly SolidColorBrush Cam = TaoMau(239, 108, 0);
    public static readonly SolidColorBrush Xanh = TaoMau(46, 125, 50);
    public static readonly SolidColorBrush XanhDuong = TaoMau(30, 136, 229);
    public static readonly SolidColorBrush Xam = TaoMau(158, 158, 158);

    private static SolidColorBrush TaoMau(byte r, byte g, byte b)
    {
        var m = new SolidColorBrush(Color.FromRgb(r, g, b));
        m.Freeze();
        return m;
    }

    /// <summary>Độ ưu tiên cá nhân 1–5: chữ «● Cao» và màu chấm.</summary>
    public static (string Chu, Brush Mau) UuTien(int mucDo) => mucDo switch
    {
        1 => ("Rất thấp", Xam),
        2 => ("Thấp", XanhDuong),
        4 => ("Cao", Cam),
        5 => ("Rất cao", Do),
        _ => ("Bình thường", Vang),
    };

    /// <summary>Ghi «● nhãn» vào TextBlock, chỉ chấm mang màu.</summary>
    public static void Ghi(TextBlock tb, string nhan, Brush mau)
    {
        tb.Inlines.Clear();
        tb.Inlines.Add(new System.Windows.Documents.Run(Cham) { Foreground = mau });
        tb.Inlines.Add(new System.Windows.Documents.Run(" " + nhan));
    }

    public static void GhiUuTien(TextBlock tb, int mucDo)
    {
        var (chu, mau) = UuTien(mucDo);
        Ghi(tb, chu, mau);
    }
}
