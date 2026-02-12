using System.Text.Json;
using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service kiểm tra, tư vấn nội dung văn bản bằng AI (Gemini)
/// Kiểm tra: chính tả, văn phong hành chính, xung đột nội dung, logic, đề xuất cải thiện
/// </summary>
public class DocumentReviewService
{
    private readonly GeminiAIService _aiService;

    public DocumentReviewService()
    {
        _aiService = new GeminiAIService();
    }

    public DocumentReviewService(GeminiAIService aiService)
    {
        _aiService = aiService;
    }

    /// <summary>
    /// Kiểm tra nội dung văn bản bằng AI
    /// </summary>
    /// <param name="content">Nội dung văn bản cần kiểm tra</param>
    /// <param name="documentType">Loại văn bản (Quyết định, Công văn...)</param>
    /// <param name="title">Tiêu đề/trích yếu</param>
    /// <param name="issuer">Cơ quan ban hành</param>
    public async Task<DocumentReviewResult> ReviewDocumentAsync(
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

        return ParseReviewResult(responseText);
    }

    private string BuildSystemPrompt()
    {
        return @"Bạn là CHUYÊN GIA SOÁT LỖI VĂN BẢN HÀNH CHÍNH NHÀ NƯỚC VIỆT NAM với kinh nghiệm 20 năm.

NHIỆM VỤ: Phân tích toàn diện văn bản và trả về kết quả dạng JSON.

BẠN PHẢI KIỂM TRA CÁC KHÍA CẠNH SAU:

1. CHÍNH TẢ (category: ""spelling"")
   - Lỗi đánh máy, sai chính tả tiếng Việt
   - Viết hoa sai quy tắc (tên cơ quan, chức danh, địa danh)
   - Dấu câu thiếu hoặc sai

2. VĂN PHONG HÀNH CHÍNH (category: ""style"")
   - Dùng khẩu ngữ, ngôn ngữ không phù hợp văn bản hành chính
   - Câu dài dòng, khó hiểu
   - Thiếu tính trang trọng, chính xác
   - Phải dùng đúng thuật ngữ hành chính

3. XUNG ĐỘT NỘI DUNG (category: ""conflict"")
   - Các điều/khoản mâu thuẫn nhau
   - Nội dung phần trước trái ngược phần sau
   - Quy định chồng chéo

4. LOGIC VÀ CẤU TRÚC (category: ""logic"")
   - Điều/Khoản/Điểm đánh số không liên tục
   - Tham chiếu sai (nhắc Điều X nhưng không tồn tại)
   - Bố cục không hợp lý

5. THIẾU THÀNH PHẦN (category: ""missing"")
   - Thiếu các phần bắt buộc theo loại văn bản
   - QĐ xử phạt thiếu quyền khiếu nại
   - Công văn thiếu thời hạn trả lời
   - Thiếu căn cứ pháp lý cần thiết

6. NỘI DUNG MƠ HỒ (category: ""ambiguous"")
   - Không rõ đối tượng áp dụng
   - Không rõ thời hạn, mức độ
   - ""Xử lý nghiêm"" mà không nói cụ thể

7. ĐỀ XUẤT CẢI THIỆN (category: ""enhancement"")
   - Bổ sung nội dung thường có trong loại VB này
   - Cải thiện cách diễn đạt
   - Bổ sung điều khoản thi hành

MỨC ĐỘ:
- ""critical"": 🔴 Nghiêm trọng — PHẢI sửa (xung đột, sai pháp luật, vượt thẩm quyền)
- ""warning"": 🟡 Cần xem xét — NÊN sửa (thiếu thành phần, văn phong, logic)
- ""suggestion"": 🟢 Gợi ý — TÙY CHỌN (cải thiện, bổ sung)

TRẢ VỀ JSON ĐÚNG FORMAT SAU (KHÔNG markdown, KHÔNG code block):
{
  ""overall_score"": <1-10>,
  ""summary"": ""<Nhận xét tổng thể 1-2 câu>"",
  ""strengths"": [""<Điểm mạnh 1>"", ""<Điểm mạnh 2>""],
  ""issues"": [
    {
      ""severity"": ""critical|warning|suggestion"",
      ""category"": ""spelling|style|conflict|logic|missing|ambiguous|enhancement"",
      ""location"": ""<Vị trí: Điều X / Khoản Y / Đoạn Z>"",
      ""original_text"": ""<Đoạn text gốc có vấn đề>"",
      ""description"": ""<Mô tả vấn đề>"",
      ""suggestion"": ""<Đề xuất sửa/nội dung thay thế>"",
      ""reason"": ""<Lý do / căn cứ>""
    }
  ],
  ""suggested_content"": ""<Toàn bộ nội dung văn bản đã sửa và cải thiện, hoặc rỗng nếu không cần sửa nhiều>""
}

QUY TẮC:
- Phải tìm TẤT CẢ lỗi, kể cả lỗi nhỏ
- Mỗi lỗi phải có suggestion cụ thể (không nói chung chung)
- Xếp issues theo mức độ: critical trước, suggestion sau
- overall_score phải phản ánh đúng chất lượng thực tế
- suggested_content: viết lại toàn bộ văn bản đã khắc phục tất cả issues
- TUYỆT ĐỐI KHÔNG dùng markdown trong suggested_content (không dùng **, *, #, ```, -, v.v.)
- suggested_content phải là PLAIN TEXT thuần, giống văn bản hành chính thật sự (in trên giấy)
- Dùng \n để xuống dòng trong suggested_content
- Chỉ trả JSON thuần, KHÔNG wrap trong ```json``` code block";
    }

    private string BuildUserPrompt(string content, string documentType, string title, string issuer)
    {
        var prompt = "KIỂM TRA VĂN BẢN SAU:\n\n";

        if (!string.IsNullOrWhiteSpace(documentType))
            prompt += $"📋 Loại văn bản: {documentType}\n";
        if (!string.IsNullOrWhiteSpace(title))
            prompt += $"📌 Tiêu đề: {title}\n";
        if (!string.IsNullOrWhiteSpace(issuer))
            prompt += $"🏛️ Cơ quan ban hành: {issuer}\n";

        prompt += $"\n--- NỘI DUNG ---\n{content}\n--- HẾT NỘI DUNG ---\n\n";
        prompt += "Hãy phân tích toàn diện và trả về JSON.";

        return prompt;
    }

    private DocumentReviewResult ParseReviewResult(string responseText)
    {
        try
        {
            // Loại bỏ markdown code block nếu AI wrap trong ```json...```
            var json = responseText.Trim();
            if (json.StartsWith("```"))
            {
                var firstNewLine = json.IndexOf('\n');
                if (firstNewLine > 0)
                    json = json[(firstNewLine + 1)..];
                if (json.EndsWith("```"))
                    json = json[..^3];
                json = json.Trim();
            }

            var result = JsonSerializer.Deserialize<DocumentReviewResult>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                return new DocumentReviewResult
                {
                    OverallScore = 0,
                    Summary = "Không thể phân tích kết quả từ AI."
                };
            }

            // Loại bỏ markdown artifacts khỏi suggested_content
            if (!string.IsNullOrWhiteSpace(result.SuggestedContent))
            {
                result.SuggestedContent = StripMarkdown(result.SuggestedContent);
            }

            return result;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"⚠️ Lỗi parse JSON review: {ex.Message}");
            Console.WriteLine($"Response: {responseText}");

            // Trả về kết quả lỗi
            return new DocumentReviewResult
            {
                OverallScore = 0,
                Summary = $"AI đã phân tích nhưng không thể đọc kết quả. Vui lòng thử lại.\n\nChi tiết: {responseText}"
            };
        }
    }

    /// <summary>
    /// Loại bỏ TOÀN BỘ ký hiệu markdown khỏi văn bản
    /// để đảm bảo suggested_content là plain text phù hợp văn bản hành chính
    /// </summary>
    private static string StripMarkdown(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        var M = System.Text.RegularExpressions.RegexOptions.Multiline;
        var S = System.Text.RegularExpressions.RegexOptions.Singleline;

        // === CODE BLOCKS (xử lý trước để không ảnh hưởng nội dung bên trong) ===
        // ```lang\ncode\n``` → giữ nội dung code
        text = System.Text.RegularExpressions.Regex.Replace(text, @"```\w*\r?\n([\s\S]*?)```", "$1", S);
        // ```inline``` 
        text = System.Text.RegularExpressions.Regex.Replace(text, @"```(.+?)```", "$1");
        // `inline code` → inline code
        text = System.Text.RegularExpressions.Regex.Replace(text, @"`(.+?)`", "$1");

        // === BOLD & ITALIC (thứ tự quan trọng: ***bolditalic*** trước **bold** trước *italic*) ===
        // ***bold italic*** hoặc ___bold italic___
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\*{3}(.+?)\*{3}", "$1");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"_{3}(.+?)_{3}", "$1");
        // **bold** 
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\*{2}(.+?)\*{2}", "$1");
        // __bold__
        text = System.Text.RegularExpressions.Regex.Replace(text, @"_{2}(.+?)_{2}", "$1");
        // *italic* (cẩn thận không bắt * đầu dòng bullet)
        text = System.Text.RegularExpressions.Regex.Replace(text, @"(?<!\s)\*(.+?)\*(?!\s)", "$1");
        // _italic_ (cẩn thận không bắt tên_biến)
        text = System.Text.RegularExpressions.Regex.Replace(text, @"(?<=\s|^)_(.+?)_(?=\s|$|[.,;:!?])", "$1", M);

        // === STRIKETHROUGH ===
        // ~~strikethrough~~
        text = System.Text.RegularExpressions.Regex.Replace(text, @"~~(.+?)~~", "$1");

        // === HIGHLIGHT ===
        // ==highlight==
        text = System.Text.RegularExpressions.Regex.Replace(text, @"==(.+?)==", "$1");

        // === HEADINGS ===
        // # Heading 1 ... ###### Heading 6
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^#{1,6}\s*", "", M);

        // === BLOCKQUOTES ===
        // > quote  hoặc >> nested quote
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^(\s*)>{1,3}\s?", "$1", M);

        // === HORIZONTAL RULES ===
        // --- hoặc *** hoặc ___ (dòng chỉ có ký hiệu)
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^[\s]*([-*_])\1{2,}[\s]*$", "", M);

        // === LISTS ===
        // - bullet hoặc * bullet → giữ nội dung, thêm indent
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^(\s*)[\*\-\+]\s+", "$1", M);
        // 1. ordered list → giữ nội dung (giữ số thứ tự vì có thể là Khoản 1, 2, 3)
        // Không strip số thứ tự vì văn bản HC dùng "1.", "2." là bình thường

        // === CHECKBOXES ===
        // - [ ] unchecked → nội dung
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^(\s*)\[[ ]\]\s*", "$1☐ ", M);
        // - [x] checked → nội dung
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^(\s*)\[[xX]\]\s*", "$1☑ ", M);

        // === LINKS ===
        // [text](url) → text
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\[([^\]]+)\]\([^\)]+\)", "$1");
        // [text][ref] → text
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\[([^\]]+)\]\[[^\]]*\]", "$1");
        // [ref]: url (reference link definition) → bỏ
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^\s*\[[^\]]+\]:\s+\S+.*$", "", M);

        // === IMAGES ===
        // ![alt](url) → alt text
        text = System.Text.RegularExpressions.Regex.Replace(text, @"!\[([^\]]*)\]\([^\)]+\)", "$1");

        // === FOOTNOTES ===
        // [^1] → bỏ
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\[\^\w+\]", "");

        // === HTML TAGS phổ biến ===
        // <br>, <br/>, <br /> → xuống dòng
        text = System.Text.RegularExpressions.Regex.Replace(text, @"<br\s*/?>", "\n", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        // <b>text</b>, <strong>text</strong> → text
        text = System.Text.RegularExpressions.Regex.Replace(text, @"</?(?:b|strong|i|em|u|s|del|ins|mark|sub|sup|small|big)>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        // <p>, </p>, <div>, </div> → xuống dòng hoặc bỏ
        text = System.Text.RegularExpressions.Regex.Replace(text, @"</(?:p|div)>", "\n", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        text = System.Text.RegularExpressions.Regex.Replace(text, @"<(?:p|div)[^>]*>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        // Mọi HTML tag còn lại → bỏ
        text = System.Text.RegularExpressions.Regex.Replace(text, @"<[^>]+>", "");

        // === HTML ENTITIES ===
        text = text.Replace("&nbsp;", " ");
        text = text.Replace("&amp;", "&");
        text = text.Replace("&lt;", "<");
        text = text.Replace("&gt;", ">");
        text = text.Replace("&quot;", "\"");
        text = text.Replace("&#39;", "'");

        // === TABLE SYNTAX ===
        // | col | col | → giữ nội dung, bỏ ký hiệu |
        // Dòng separator: |---|---| → bỏ hoàn toàn
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^\|[-:\s|]+\|$", "", M);
        // Bỏ | đầu và cuối dòng table
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^\|\s*(.+?)\s*\|$", "$1", M);
        // Bỏ | giữa các cột → thay bằng tab
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s*\|\s*", "   ", System.Text.RegularExpressions.RegexOptions.None);

        // === CLEANUP: dọn dẹp dòng trống thừa ===
        // 3+ dòng trống liên tiếp → 2 dòng trống
        text = System.Text.RegularExpressions.Regex.Replace(text, @"(\r?\n){3,}", "\n\n");

        return text.Trim();
    }
}
