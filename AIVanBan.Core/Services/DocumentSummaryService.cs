using System.Text.Json;
using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service tóm tắt văn bản nhà nước bằng AI.
/// Prompt được thiết kế khoa học, logic, phù hợp với cấu trúc văn bản hành chính Việt Nam.
/// </summary>
public class DocumentSummaryService
{
    private readonly GeminiAIService _aiService;

    public DocumentSummaryService()
    {
        _aiService = new GeminiAIService();
    }

    public DocumentSummaryService(GeminiAIService aiService)
    {
        _aiService = aiService;
    }

    /// <summary>
    /// Tóm tắt văn bản
    /// </summary>
    public async Task<DocumentSummary> SummarizeAsync(
        string content,
        string documentType = "",
        string title = "",
        string issuer = "")
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung văn bản không được để trống.");

        var systemPrompt = BuildSystemPrompt();
        var userPrompt = BuildUserPrompt(content, documentType, title, issuer);

        var responseText = await _aiService.GenerateContentAsync(userPrompt, systemPrompt);
        return ParseResult(responseText);
    }

    private string BuildSystemPrompt()
    {
        return @"Bạn là CHUYÊN GIA PHÂN TÍCH VĂN BẢN NHÀ NƯỚC Việt Nam, có kinh nghiệm 20 năm trong lĩnh vực pháp chế và hành chính công.

NHIỆM VỤ: Tóm tắt văn bản hành chính nhà nước một cách KHOA HỌC, LOGIC, CHÍNH XÁC.

NGUYÊN TẮC TÓM TẮT:
1. TRUNG THÀNH với nội dung gốc — KHÔNG thêm thông tin, KHÔNG suy diễn
2. SẮP XẾP LOGIC theo cấu trúc văn bản: mục đích → căn cứ → nội dung chính → thực hiện → hiệu lực
3. TRÍCH XUẤT CHÍNH XÁC: số liệu, ngày tháng, tên cơ quan, căn cứ pháp lý — không làm tròn, không ước lượng
4. SỬ DỤNG THUẬT NGỮ hành chính chuẩn mực, phù hợp Nghị định 30/2020/NĐ-CP về công tác văn thư
5. PHÂN LOẠI nội dung theo Điều/Khoản/Mục nếu văn bản có cấu trúc rõ ràng
6. NẾU văn bản không rõ cấu trúc (biên bản, công văn thường), phân chia theo chủ đề logic

CẤU TRÚC PHÂN TÍCH theo 9 mục:

1. brief: Tóm tắt ngắn gọn 1-2 câu — trả lời được: ""Văn bản này nói về gì? Ai ban hành? Để làm gì?""
2. document_type: Xác định chính xác loại văn bản (Công văn, Quyết định, Nghị quyết, Báo cáo, Kế hoạch, Tờ trình, Thông báo, Biên bản, Chỉ thị, Nghị định...)
3. issuing_authority: Cơ quan ban hành + người ký (nếu có)
4. target_audience: Đối tượng áp dụng hoặc nơi nhận chính
5. key_points: Các nội dung chính, mỗi mục gồm {heading, content}:
   - Với VB có Điều/Khoản: heading = ""Điều 1"", ""Điều 2""...
   - Với VB không có Điều: heading = ""Mục tiêu"", ""Nội dung"", ""Tổ chức thực hiện""...
   - Với Báo cáo: heading = ""Kết quả"", ""Tồn tại"", ""Phương hướng""...
   - Với Biên bản: heading = ""Nội dung 1"", ""Nội dung 2"", ""Kết luận""...
   - content: tóm gọn nhưng ĐẦY ĐỦ ý chính, giữ nguyên số liệu và tên riêng
6. legal_bases: Danh sách căn cứ pháp lý được viện dẫn (giữ nguyên số hiệu, ngày ban hành)
7. effective_dates: Các mốc thời gian quan trọng (ngày hiệu lực, thời hạn, deadline)
8. key_figures: Các con số/chỉ tiêu quan trọng (kinh phí, số lượng, tỷ lệ, diện tích...)
9. impact: Tác động/ý nghĩa thực tiễn 1-2 câu
10. notes: Lưu ý đặc biệt (nếu có: thay thế VB cũ, hiệu lực trở về trước, áp dụng thí điểm...)

TRẢ VỀ JSON THUẦN TÚY, KHÔNG dùng markdown code fence (```json```).

Ví dụ output:
{
  ""brief"": ""Quyết định phê duyệt kế hoạch phát triển kinh tế - xã hội năm 2026 của UBND thành phố X"",
  ""document_type"": ""Quyết định"",
  ""issuing_authority"": ""Chủ tịch UBND thành phố X — Nguyễn Văn A"",
  ""target_audience"": ""Các sở ban ngành thuộc UBND thành phố, UBND các xã/phường"",
  ""key_points"": [
    {""heading"": ""Điều 1: Mục tiêu tổng quát"", ""content"": ""Tốc độ tăng trưởng kinh tế đạt 8,5%; thu ngân sách 120 tỷ đồng""},
    {""heading"": ""Điều 2: Nhiệm vụ trọng tâm"", ""content"": ""5 nhiệm vụ: (1) Phát triển nông nghiệp công nghệ cao, (2) Thu hút đầu tư, (3) Đẩy mạnh cải cách hành chính, (4) Xây dựng NTM, (5) An sinh xã hội""},
    {""heading"": ""Điều 3: Tổ chức thực hiện"", ""content"": ""Giao Phòng TC-KH chủ trì, các phòng ban phối hợp, báo cáo hàng quý""}
  ],
  ""legal_bases"": [
    ""Luật Tổ chức chính quyền địa phương 2015"",
    ""Nghị quyết số 01/NQ-HĐND ngày 15/01/2026 của HĐND thành phố""
  ],
  ""effective_dates"": [""Có hiệu lực kể từ ngày ký (20/01/2026)"", ""Báo cáo kết quả trước 30/6/2026""],
  ""key_figures"": [""Tăng trưởng 8,5%"", ""Thu ngân sách 120 tỷ đồng"", ""Xây dựng 3 xã NTM nâng cao""],
  ""impact"": ""Là cơ sở pháp lý để các đơn vị triển khai kế hoạch phát triển KT-XH năm 2026, gắn trách nhiệm cụ thể cho từng đơn vị."",
  ""notes"": ""Thay thế Quyết định số 15/QĐ-UBND ngày 18/01/2025.""
}

QUY TẮC QUAN TRỌNG:
- KHÔNG bịa số liệu, ngày tháng — nếu VB không đề cập thì bỏ trống mảng []
- key_points: tối thiểu 2 mục, tối đa 8 mục — ưu tiên nội dung quan trọng nhất
- legal_bases: giữ nguyên số hiệu VB, không rút gọn
- brief: PHẢI trả lời được 3 câu hỏi: Ai? Về gì? Để làm gì?
- KHÔNG dùng markdown ```json``` bao quanh kết quả";
    }

    private string BuildUserPrompt(string content, string documentType, string title, string issuer)
    {
        var prompt = "HÃY TÓM TẮT VĂN BẢN SAU:\n\n";

        if (!string.IsNullOrWhiteSpace(documentType))
            prompt += $"Loại văn bản: {documentType}\n";
        if (!string.IsNullOrWhiteSpace(title))
            prompt += $"Tiêu đề: {title}\n";
        if (!string.IsNullOrWhiteSpace(issuer))
            prompt += $"Cơ quan ban hành: {issuer}\n";

        prompt += $"\n--- NỘI DUNG VĂN BẢN ---\n{content}\n--- HẾT ---";
        return prompt;
    }

    private DocumentSummary ParseResult(string responseText)
    {
        if (string.IsNullOrWhiteSpace(responseText))
            throw new Exception("AI không trả về kết quả.");

        var text = responseText.Trim();

        // 1) Strip markdown code fences ```json ... ```
        if (text.StartsWith("```"))
        {
            var firstNewline = text.IndexOf('\n');
            if (firstNewline > 0) text = text.Substring(firstNewline + 1);
            if (text.EndsWith("```")) text = text.Substring(0, text.Length - 3);
            text = text.Trim();
        }

        // 2) Bóc tách phần JSON: tìm '{' đầu tiên và '}' cuối cùng
        var firstBrace = text.IndexOf('{');
        var lastBrace = text.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            text = text.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        // 3) Thử parse trực tiếp
        try
        {
            var parsed = JsonSerializer.Deserialize<DocumentSummary>(text, options);
            if (parsed != null && !string.IsNullOrWhiteSpace(parsed.Brief))
                return parsed;
        }
        catch (JsonException) { /* fallthrough */ }

        // 4) Thử sửa JSON bị cắt cụt — đóng các ngoặc còn dở
        try
        {
            var repaired = TryRepairTruncatedJson(text);
            var parsed = JsonSerializer.Deserialize<DocumentSummary>(repaired, options);
            if (parsed != null && !string.IsNullOrWhiteSpace(parsed.Brief))
                return parsed;
        }
        catch (JsonException) { /* fallthrough */ }

        // 5) Cuối cùng: trích các trường chính bằng regex từ JSON lỗi
        // — KHÔNG hiển thị raw JSON cho user nữa
        return ExtractFieldsByRegex(text);
    }

    /// <summary>
    /// Thử "vá" JSON bị cắt cụt: đếm ngoặc { } [ ] và bổ sung phần thiếu
    /// </summary>
    private static string TryRepairTruncatedJson(string json)
    {
        int braceOpen = 0, braceClose = 0, bracketOpen = 0, bracketClose = 0;
        bool inString = false; char prev = '\0';
        foreach (var c in json)
        {
            if (c == '"' && prev != '\\') inString = !inString;
            if (!inString)
            {
                if (c == '{') braceOpen++;
                else if (c == '}') braceClose++;
                else if (c == '[') bracketOpen++;
                else if (c == ']') bracketClose++;
            }
            prev = c;
        }

        var s = json.TrimEnd().TrimEnd(',');
        // Đóng string đang dở
        if (inString) s += "\"";
        // Đóng các array thiếu
        for (int i = 0; i < bracketOpen - bracketClose; i++) s += "]";
        // Đóng các object thiếu
        for (int i = 0; i < braceOpen - braceClose; i++) s += "}";
        return s;
    }

    /// <summary>
    /// Khi JSON không parse được, trích các trường chính bằng regex để hiển thị
    /// </summary>
    private static DocumentSummary ExtractFieldsByRegex(string text)
    {
        string Pick(string key)
        {
            var m = System.Text.RegularExpressions.Regex.Match(
                text,
                "\"" + key + "\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\"",
                System.Text.RegularExpressions.RegexOptions.Singleline);
            return m.Success ? m.Groups[1].Value.Replace("\\\"", "\"").Replace("\\n", "\n") : "";
        }

        var result = new DocumentSummary
        {
            Brief = Pick("brief"),
            DocumentType = Pick("document_type"),
            IssuingAuthority = Pick("issuing_authority"),
            TargetAudience = Pick("target_audience"),
            Impact = Pick("impact"),
            Notes = Pick("notes")
        };

        if (string.IsNullOrWhiteSpace(result.Brief))
            result.Brief = "(AI không trả về tóm tắt hợp lệ — vui lòng thử lại)";

        // Trích key_points đơn giản: tìm các cặp heading/content
        var kpMatches = System.Text.RegularExpressions.Regex.Matches(
            text,
            "\"heading\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\"\\s*,\\s*\"content\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\"",
            System.Text.RegularExpressions.RegexOptions.Singleline);
        foreach (System.Text.RegularExpressions.Match m in kpMatches)
        {
            result.KeyPoints.Add(new SummaryKeyPoint
            {
                Heading = m.Groups[1].Value,
                Content = m.Groups[2].Value
            });
        }

        // Trích các array string đơn giản: legal_bases, effective_dates, key_figures
        result.LegalBases = ExtractStringArray(text, "legal_bases");
        result.EffectiveDates = ExtractStringArray(text, "effective_dates");
        result.KeyFigures = ExtractStringArray(text, "key_figures");

        if (string.IsNullOrWhiteSpace(result.Notes))
            result.Notes = "⚠️ AI trả về định dạng không chuẩn — kết quả có thể chưa đầy đủ. Nhấn 🔄 Thử lại để tóm tắt lại.";

        return result;
    }

    private static List<string> ExtractStringArray(string text, string key)
    {
        var list = new List<string>();
        var arrMatch = System.Text.RegularExpressions.Regex.Match(
            text,
            "\"" + key + "\"\\s*:\\s*\\[(.*?)\\]",
            System.Text.RegularExpressions.RegexOptions.Singleline);
        if (!arrMatch.Success) return list;

        var items = System.Text.RegularExpressions.Regex.Matches(
            arrMatch.Groups[1].Value,
            "\"((?:[^\"\\\\]|\\\\.)*)\"",
            System.Text.RegularExpressions.RegexOptions.Singleline);
        foreach (System.Text.RegularExpressions.Match m in items)
        {
            list.Add(m.Groups[1].Value);
        }
        return list;
    }
}
