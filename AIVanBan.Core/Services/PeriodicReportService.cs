using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service AI soạn báo cáo định kỳ từ số liệu thô.
/// Tự tính % tăng/giảm, so sánh kỳ trước, viết đánh giá + kiến nghị.
/// </summary>
public class PeriodicReportService
{
    private readonly GeminiAIService _aiService;

    public PeriodicReportService(GeminiAIService? aiService = null)
    {
        _aiService = aiService ?? new GeminiAIService();
    }

    /// <summary>
    /// Tạo báo cáo định kỳ từ số liệu
    /// </summary>
    public async Task<string> GenerateReportAsync(
        string reportPeriodType,   // Tuần / Tháng / Quý / Năm
        string reportPeriod,       // VD: "Tháng 02/2026", "Quý I/2026"
        string field,              // Lĩnh vực: KT-XH, CCHC, Tài chính...
        string orgName,            // Tên đơn vị
        string rawData,            // Số liệu thô (paste)
        string? previousReport,    // Nội dung BC kỳ trước (nếu có)
        string signerName,         // Người ký
        string signerTitle)        // Chức danh
    {
        var systemInstruction = BuildSystemPrompt();
        var prompt = BuildUserPrompt(reportPeriodType, reportPeriod, field, orgName,
                                      rawData, previousReport, signerName, signerTitle);

        return await _aiService.GenerateContentAsync(prompt, systemInstruction);
    }

    private string BuildSystemPrompt()
    {
        return @"Bạn là CHUYÊN GIA SOẠN BÁO CÁO HÀNH CHÍNH tại UBND cấp xã/phường Việt Nam, 20 năm kinh nghiệm.

NHIỆM VỤ: Từ số liệu thô, soạn NỘI DUNG THÂN BÁO CÁO (body) — KHÔNG bao gồm phần thể thức.

⚠️ QUAN TRỌNG — CHỈ TẠO PHẦN NỘI DUNG:
KHÔNG được viết các phần sau (vì phần mềm sẽ tự thêm khi xuất Word):
- KHÔNG viết quốc hiệu (CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM...)
- KHÔNG viết tiêu ngữ (Độc lập - Tự do - Hạnh phúc)
- KHÔNG viết tên cơ quan ban hành
- KHÔNG viết số/ký hiệu văn bản
- KHÔNG viết địa danh, ngày tháng
- KHÔNG viết dòng ""BÁO CÁO"" và trích yếu
- KHÔNG viết ""Kính gửi:""
- KHÔNG viết phần ""Nơi nhận:""
- KHÔNG viết phần chữ ký, chức danh, tên người ký
- KHÔNG viết ""(Đã ký)""

CHỈ VIẾT NỘI DUNG BÁO CÁO, bắt đầu từ câu dẫn nhập và kết thúc bằng câu kết luận.

BỐ CỤC NỘI DUNG:

Câu dẫn nhập: ""Thực hiện kế hoạch..., [đơn vị] báo cáo kết quả... như sau:""

Phần I — KẾT QUẢ THỰC HIỆN
- Chia theo từng mục/lĩnh vực
- Mỗi mục: số liệu + so sánh kỳ trước (nếu có)
- TỰ TÍNH: % tăng/giảm, tỷ lệ hoàn thành, chênh lệch

Phần II — ĐÁNH GIÁ CHUNG
- Ưu điểm, kết quả nổi bật
- Tồn tại, hạn chế
- Nguyên nhân

Phần III — PHƯƠNG HƯỚNG, KIẾN NGHỊ
- Nhiệm vụ trọng tâm kỳ tới
- Kiến nghị cấp trên (nếu có)
- Đề xuất giải pháp

Câu kết: ""Trên đây là báo cáo... Kính đề nghị [cấp trên] xem xét, chỉ đạo.""

QUY TẮC VIẾT:
- Văn phong hành chính chuẩn, trang trọng
- Số liệu rõ ràng, có đơn vị
- Nếu có kỳ trước → so sánh tăng/giảm (tuyệt đối + %)
- KHÔNG dùng markdown (**, *, #, ```)
- PLAIN TEXT thuần — giống thân văn bản hành chính
- Xuống dòng bình thường, KHÔNG viết literal \n
- Gạch đầu dòng dùng dấu ""-""";
    }

    private string BuildUserPrompt(
        string reportPeriodType, string reportPeriod, string field, string orgName,
        string rawData, string? previousReport, string signerName, string signerTitle)
    {
        var prompt = $@"Hãy soạn BÁO CÁO ĐỊNH KỲ với thông tin sau:

ĐƠN VỊ: {orgName}
LOẠI BÁO CÁO: Báo cáo {reportPeriodType.ToLower()}
KỲ BÁO CÁO: {reportPeriod}
LĨNH VỰC: {field}
NGƯỜI KÝ: {signerName}
CHỨC DANH: {signerTitle}

===== SỐ LIỆU HIỆN TẠI =====
{rawData}";

        if (!string.IsNullOrWhiteSpace(previousReport))
        {
            prompt += $@"

===== BÁO CÁO KỲ TRƯỚC (để so sánh) =====
{previousReport}

LƯU Ý: Hãy SO SÁNH số liệu hiện tại với kỳ trước. Tính % tăng/giảm cho mỗi chỉ tiêu.";
        }
        else
        {
            prompt += @"

LƯU Ý: Không có số liệu kỳ trước. Chỉ trình bày số liệu hiện tại, không cần so sánh.";
        }

        prompt += @"

Hãy soạn NỘI DUNG THÂN BÁO CÁO (chỉ phần body, KHÔNG gồm header/quốc hiệu/chữ ký/nơi nhận).
Bắt đầu từ câu dẫn nhập, kết thúc bằng câu kết luận.
PLAIN TEXT thuần — KHÔNG dùng markdown — KHÔNG viết literal \n.";

        return prompt;
    }

    /// <summary>
    /// Danh sách lĩnh vực phổ biến cho báo cáo
    /// </summary>
    public static List<string> GetCommonFields()
    {
        return new List<string>
        {
            "Kinh tế - Xã hội",
            "Cải cách hành chính",
            "Tài chính - Ngân sách",
            "An ninh - Trật tự",
            "Giáo dục - Đào tạo",
            "Y tế - Dân số",
            "Văn hóa - Thông tin",
            "Nông nghiệp - Nông thôn",
            "Tài nguyên - Môi trường",
            "Lao động - TBXH",
            "Xây dựng - Hạ tầng",
            "Tư pháp - Hộ tịch",
            "Quốc phòng - Quân sự",
            "Phòng chống tham nhũng",
            "Công tác Đảng",
            "Nông thôn mới",
            "Chuyển đổi số",
            "Khác"
        };
    }

    /// <summary>
    /// Danh sách loại kỳ báo cáo
    /// </summary>
    public static List<string> GetPeriodTypes()
    {
        return new List<string> { "Tuần", "Tháng", "Quý", "6 tháng", "Năm" };
    }

    /// <summary>
    /// Gợi ý tên kỳ báo cáo dựa trên loại
    /// </summary>
    public static List<string> GetPeriodSuggestions(string periodType)
    {
        var now = DateTime.Now;
        return periodType switch
        {
            "Tuần" => Enumerable.Range(1, 5)
                .Select(i => $"Tuần {GetWeekOfMonth(now)}/{now.Month:00}/{now.Year}")
                .Distinct().Take(4)
                .Concat(new[] { $"Tuần {GetWeekOfMonth(now.AddDays(-7))}/{now.AddDays(-7).Month:00}/{now.AddDays(-7).Year}" })
                .Distinct().ToList(),

            "Tháng" => Enumerable.Range(0, 6)
                .Select(i => now.AddMonths(-i))
                .Select(d => $"Tháng {d.Month:00}/{d.Year}")
                .ToList(),

            "Quý" => Enumerable.Range(0, 4)
                .Select(i =>
                {
                    var quarter = (now.Month - 1) / 3 + 1 - i;
                    var year = now.Year;
                    while (quarter <= 0) { quarter += 4; year--; }
                    return $"Quý {ToRoman(quarter)}/{year}";
                })
                .ToList(),

            "6 tháng" => new List<string>
            {
                $"6 tháng đầu năm {now.Year}",
                $"6 tháng cuối năm {now.Year - 1}",
                $"6 tháng đầu năm {now.Year - 1}"
            },

            "Năm" => Enumerable.Range(0, 3)
                .Select(i => $"Năm {now.Year - i}")
                .ToList(),

            _ => new List<string>()
        };
    }

    private static int GetWeekOfMonth(DateTime date)
    {
        var firstDay = new DateTime(date.Year, date.Month, 1);
        return (date.Day + (int)firstDay.DayOfWeek - 1) / 7 + 1;
    }

    private static string ToRoman(int number)
    {
        return number switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            _ => number.ToString()
        };
    }
}
