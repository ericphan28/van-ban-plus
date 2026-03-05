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

        // Phát hiện nếu dữ liệu được gộp từ nhiều file Word (tổng hợp tháng → quý)
        bool isMergedFromFiles = rawData.Contains("===== FILE ") && rawData.Contains("=====");
        bool hasTables = rawData.Contains("[BẢNG]");

        if (isMergedFromFiles)
        {
            prompt += @"

⚠️ DỮ LIỆU TRÊN ĐƯỢC GỘP TỪ NHIỀU FILE BÁO CÁO THÁNG.
Mỗi phần '===== FILE X: ... =====' là nội dung từ 1 file báo cáo tháng riêng.
NHIỆM VỤ QUAN TRỌNG:
1. Đọc hiểu NỘI DUNG từng báo cáo tháng
2. TỔNG HỢP số liệu cả kỳ (cộng gộp hoặc lấy trung bình tùy chỉ tiêu)
3. SO SÁNH xu hướng giữa các tháng (tăng/giảm)
4. Viết thành 1 báo cáo tổng hợp hoàn chỉnh cho cả kỳ";
        }

        if (hasTables)
        {
            prompt += @"

⚠️ DỮ LIỆU CÓ CHỨA BẢNG BIỂU (giữa [BẢNG] và [/BẢNG]).
- Đọc dữ liệu từ bảng theo cấu trúc cột: dòng đầu là tiêu đề, các dòng sau là dữ liệu.
- Trích xuất số liệu từ bảng để tính toán và tổng hợp.
- KHÔNG copy nguyên bảng vào báo cáo — hãy VIẾT LẠI thành câu văn có số liệu.";
        }

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

    /// <summary>
    /// Tự động trích xuất số liệu thống kê từ DB cho báo cáo định kỳ.
    /// Giúp user không phải tự paste số liệu — auto-fill từ sổ văn bản.
    /// </summary>
    public static string ExtractStatsFromDB(string periodType, string reportPeriod)
    {
        try
        {
            var docService = new DocumentService();
            var allDocs = docService.GetAllDocuments();
            if (allDocs.Count == 0)
                return "(Chưa có dữ liệu văn bản trong hệ thống)";

            // Xác định khoảng thời gian
            var (startDate, endDate) = ParsePeriodRange(periodType, reportPeriod);
            var periodDocs = allDocs.Where(d => d.IssueDate >= startDate && d.IssueDate <= endDate).ToList();

            // Khoảng thời gian kỳ trước (để so sánh)
            var periodLength = endDate - startDate;
            var prevStart = startDate - periodLength;
            var prevEnd = startDate.AddDays(-1);
            var prevDocs = allDocs.Where(d => d.IssueDate >= prevStart && d.IssueDate <= prevEnd).ToList();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"📊 SỐ LIỆU TỰ ĐỘNG TỪ SỔ VĂN BẢN (Kỳ: {reportPeriod})");
            sb.AppendLine($"Từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}");
            sb.AppendLine();

            // 1. Tổng quan
            var docsDen = periodDocs.Where(d => d.Direction == Direction.Den).ToList();
            var docsDi = periodDocs.Where(d => d.Direction == Direction.Di).ToList();
            sb.AppendLine("1. TỔNG QUAN VĂN BẢN:");
            sb.AppendLine($"   - Tổng VB trong kỳ: {periodDocs.Count}");
            sb.AppendLine($"   - VB đến: {docsDen.Count}");
            sb.AppendLine($"   - VB đi: {docsDi.Count}");
            if (prevDocs.Count > 0)
            {
                var prevDen = prevDocs.Count(d => d.Direction == Direction.Den);
                var prevDi = prevDocs.Count(d => d.Direction == Direction.Di);
                sb.AppendLine($"   - So kỳ trước: {prevDocs.Count} VB (đến: {prevDen}, đi: {prevDi})");
            }
            sb.AppendLine();

            // 2. Phân loại theo loại VB
            sb.AppendLine("2. PHÂN LOẠI THEO LOẠI VĂN BẢN:");
            var byType = periodDocs.GroupBy(d => d.Type.GetDisplayName())
                .OrderByDescending(g => g.Count())
                .Take(10);
            foreach (var g in byType)
                sb.AppendLine($"   - {g.Key}: {g.Count()} VB");
            sb.AppendLine();

            // 3. Mức độ khẩn
            var khacThuong = periodDocs.Where(d => d.UrgencyLevel != UrgencyLevel.Thuong).ToList();
            if (khacThuong.Count > 0)
            {
                sb.AppendLine("3. VĂN BẢN KHẨN/MẬT:");
                foreach (var g in khacThuong.GroupBy(d => d.UrgencyLevel.GetDisplayName()))
                    sb.AppendLine($"   - {g.Key}: {g.Count()} VB");
                sb.AppendLine();
            }

            // 4. Tình hình xử lý VB đến
            var processed = docsDen.Where(d =>
                !string.IsNullOrWhiteSpace(d.ProcessingNotes) || !string.IsNullOrWhiteSpace(d.AssignedTo)).ToList();
            var overdue = docsDen.Where(d => d.DueDate.HasValue && d.DueDate.Value < DateTime.Now &&
                string.IsNullOrWhiteSpace(d.ProcessingNotes)).ToList();
            sb.AppendLine("4. TÌNH HÌNH XỬ LÝ VB ĐẾN:");
            sb.AppendLine($"   - Đã phân công/xử lý: {processed.Count}/{docsDen.Count}");
            if (docsDen.Count > 0)
                sb.AppendLine($"   - Tỷ lệ xử lý: {processed.Count * 100 / docsDen.Count}%");
            sb.AppendLine($"   - Quá hạn chưa xử lý: {overdue.Count}");
            sb.AppendLine();

            // 5. Phân loại theo lĩnh vực
            var byCategory = periodDocs.Where(d => !string.IsNullOrWhiteSpace(d.Category))
                .GroupBy(d => d.Category)
                .OrderByDescending(g => g.Count())
                .Take(8);
            if (byCategory.Any())
            {
                sb.AppendLine("5. PHÂN LOẠI THEO LĨNH VỰC:");
                foreach (var g in byCategory)
                    sb.AppendLine($"   - {g.Key}: {g.Count()} VB");
                sb.AppendLine();
            }

            // 6. Top cơ quan gửi VB đến
            var topIssuers = docsDen.Where(d => !string.IsNullOrWhiteSpace(d.Issuer))
                .GroupBy(d => d.Issuer)
                .OrderByDescending(g => g.Count())
                .Take(5);
            if (topIssuers.Any())
            {
                sb.AppendLine("6. CƠ QUAN GỬI VB ĐẾN NHIỀU NHẤT:");
                foreach (var g in topIssuers)
                    sb.AppendLine($"   - {g.Key}: {g.Count()} VB");
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"(Lỗi trích xuất số liệu: {ex.Message})";
        }
    }

    /// <summary>
    /// Parse kỳ báo cáo thành khoảng ngày (startDate, endDate)
    /// </summary>
    private static (DateTime start, DateTime end) ParsePeriodRange(string periodType, string reportPeriod)
    {
        var now = DateTime.Now;
        try
        {
            switch (periodType)
            {
                case "Tháng":
                    // "Tháng 02/2026" → parse month/year
                    var parts = reportPeriod.Replace("Tháng ", "").Split('/');
                    if (parts.Length == 2 && int.TryParse(parts[0], out var month) && int.TryParse(parts[1], out var year))
                    {
                        var start = new DateTime(year, month, 1);
                        return (start, start.AddMonths(1).AddDays(-1));
                    }
                    break;

                case "Quý":
                    // "Quý I/2026" → parse quarter/year
                    var qParts = reportPeriod.Replace("Quý ", "").Split('/');
                    if (qParts.Length == 2 && int.TryParse(qParts[1], out var qYear))
                    {
                        var quarter = qParts[0] switch { "I" => 1, "II" => 2, "III" => 3, "IV" => 4, _ => 1 };
                        var qStart = new DateTime(qYear, (quarter - 1) * 3 + 1, 1);
                        return (qStart, qStart.AddMonths(3).AddDays(-1));
                    }
                    break;

                case "Năm":
                    // "Năm 2026"
                    var yParts = reportPeriod.Replace("Năm ", "");
                    if (int.TryParse(yParts, out var y))
                        return (new DateTime(y, 1, 1), new DateTime(y, 12, 31));
                    break;

                case "Tuần":
                    // Tuần hiện tại fallback
                    var weekStart = now.AddDays(-(int)now.DayOfWeek + 1);
                    return (weekStart, weekStart.AddDays(6));

                case "6 tháng":
                    if (reportPeriod.Contains("đầu"))
                    {
                        var yy = int.TryParse(reportPeriod.Split(' ').Last(), out var hy) ? hy : now.Year;
                        return (new DateTime(yy, 1, 1), new DateTime(yy, 6, 30));
                    }
                    else
                    {
                        var yy = int.TryParse(reportPeriod.Split(' ').Last(), out var hy) ? hy : now.Year;
                        return (new DateTime(yy, 7, 1), new DateTime(yy, 12, 31));
                    }
            }
        }
        catch { }

        // Fallback: tháng hiện tại
        return (new DateTime(now.Year, now.Month, 1), now);
    }
}
