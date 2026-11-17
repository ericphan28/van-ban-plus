// Xuất dữ liệu chung của bản cài máy cho bản web (L5): 69 mẫu cài sẵn (đúng thứ tự TemplateSeeder.SeedDefaultTemplates)
// + cây pháp quy NĐ 30/2020. Chạy từ gốc repo:
//   dotnet run --project tools/xuat-du-lieu-web -- D:/AIVanBanCaNhan/_wt-web-mau/supabase/du-lieu
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

if (args.Length != 1)
{
    Console.Error.WriteLine("Dùng: dotnet run --project tools/xuat-du-lieu-web -- <thu-muc-ra>");
    return 2;
}
var ra = args[0];
Directory.CreateDirectory(ra);

string ChuanHoa(string? s) => (s ?? "").Replace("\r\n", "\n").Trim('\n', '\r', ' ');

var goc = Directory.GetCurrentDirectory();
while (!File.Exists(Path.Combine(goc, "AIVanBan.sln")))
    goc = Path.GetDirectoryName(goc) ?? throw new Exception("Chạy lệnh từ trong repo AIVanBanCaNhan.");

// Thứ tự + danh sách lấy từ chính mã SeedDefaultTemplates: chỉ xuất mẫu thật sự được seed.
var nguon = File.ReadAllText(Path.Combine(goc, "AIVanBan.Core", "Services", "TemplateSeeder.cs"));
var khoi = Regex.Match(nguon, @"new List<DocumentTemplate>\s*\{(.*?)\};", RegexOptions.Singleline);
if (!khoi.Success) throw new Exception("Không thấy danh sách mẫu trong SeedDefaultTemplates.");
var tenHam = Regex.Matches(khoi.Groups[1].Value, @"(Create\w+Template)\(\)").Select(m => m.Groups[1].Value).ToList();

var seeder = (TemplateSeeder)RuntimeHelpers.GetUninitializedObject(typeof(TemplateSeeder));
var mau = new List<object>();
foreach (var ham in tenHam)
{
    var m = typeof(TemplateSeeder).GetMethod(ham,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes)
            ?? throw new Exception($"Không thấy hàm {ham}.");
    var d = (DocumentTemplate)m.Invoke(m.IsStatic ? null : seeder, null)!;
    mau.Add(new
    {
        ham,
        ten = d.Name.Trim(),
        loai = d.Type.ToString(),
        nhom = d.Category,
        mo_ta = d.Description,
        noi_dung_mau = ChuanHoa(d.TemplateContent),
        goi_y_ai = ChuanHoa(d.AIPrompt),
        truong = d.RequiredFields,
        the = d.Tags,
    });
}

var nut = new List<object>();
void Di(IEnumerable<LegalNode> ds, string? cha, string? vanBan)
{
    var i = 0;
    foreach (var n in ds)
    {
        var vb = vanBan ?? n.Id;
        var ma = vanBan == null ? n.Id : $"{vb}:{n.Id}";
        nut.Add(new { ma, cha, loai = n.NodeType.ToString(), thu_tu = i++, tieu_de = n.Title.Trim(), noi_dung = ChuanHoa(n.Content), the = n.FeatureTags });
        Di(n.Children, ma, vb);
    }
}
Di(LegalReferenceData.GetLegalTree(), null, null);

var tuyChon = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
File.WriteAllText(Path.Combine(ra, "mau-cai-san.json"), JsonSerializer.Serialize(mau, tuyChon) + "\n");
File.WriteAllText(Path.Combine(ra, "phap-quy.json"), JsonSerializer.Serialize(nut, tuyChon) + "\n");
Console.WriteLine($"Đã xuất {mau.Count} mẫu, {nut.Count} mục pháp quy vào {ra}");
return 0;
