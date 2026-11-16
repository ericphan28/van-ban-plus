using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service kiểm tra, tư vấn nội dung văn bản bằng AI (Gemini)
/// Kiểm tra: chính tả, văn phong hành chính, xung đột nội dung, logic, đề xuất cải thiện
/// </summary>
public class DocumentReviewService
{
    private readonly TacVuClient _tacVu;

    public DocumentReviewService() : this(new TacVuClient()) { }

    public DocumentReviewService(TacVuClient tacVu)
    {
        _tacVu = tacVu;
    }

    /// <summary>Credit đã trừ, số dư, cờ bị cắt của lượt chạy gần nhất.</summary>
    public ThongTinLuot? LuotCuoi { get; private set; }

    /// <summary>Khoảng credit dự kiến cho đúng đầu vào sẽ gửi (không gọi AI, không giữ credit).</summary>
    public Task<UocTinhKetQua> UocTinhAsync(string content, string documentType = "", string title = "",
        string issuer = "", string? referenceContent = null)
        => _tacVu.UocTinhAsync("kiem-tra", DauVao(content, documentType, title, issuer, referenceContent));

    /// <summary>
    /// Kiểm tra nội dung văn bản qua tác vụ v2 kiem-tra. Kết quả bị cắt mà máy chủ trả chuỗi thô:
    /// ném <see cref="KetQuaBiCatException"/> (credit đã trừ, <see cref="LuotCuoi"/> vẫn được ghi).
    /// </summary>
    /// <param name="referenceContent">Nội dung văn bản mẫu để đối chiếu (tùy chọn).</param>
    public async Task<DocumentReviewResult> ReviewDocumentAsync(
        string content,
        string documentType = "",
        string title = "",
        string issuer = "",
        string? referenceContent = null)
    {
        var kq = await _tacVu.ChayAsync<DocumentReviewResult>("kiem-tra",
            DauVao(content, documentType, title, issuer, referenceContent));
        LuotCuoi = kq.Luot;
        if (kq.ChuoiTho != null || kq.KetQua is null)
            throw new KetQuaBiCatException("kiểm tra", kq.Luot);

        var result = kq.KetQua;
        // Hậu xử lý hiển thị (không phải prompt): văn bản hành chính là chữ thuần, bỏ ký hiệu markdown.
        if (!string.IsNullOrWhiteSpace(result.SuggestedContent))
            result.SuggestedContent = StripMarkdown(result.SuggestedContent);
        return result;
    }

    private static object DauVao(string content, string documentType, string title, string issuer, string? referenceContent)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung văn bản không được để trống.");
        return new
        {
            noi_dung = content,
            loai_van_ban = documentType,
            tieu_de = title,
            co_quan = issuer,
            van_ban_doi_chieu = string.IsNullOrWhiteSpace(referenceContent) ? null : referenceContent,
        };
    }

    internal static string BuildSystemPrompt()
    {
        return @"Bạn là CHUYÊN GIA SOÁT LỖI VĂN BẢN HÀNH CHÍNH NHÀ NƯỚC VIỆT NAM với kinh nghiệm 20 năm.
Thông thạo Nghị định 30/2020/NĐ-CP ngày 05/3/2020 về công tác văn thư.

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

8. THỂ THỨC VĂN BẢN THEO NĐ 30/2020 (category: ""format"")
   Kiểm tra thể thức theo Nghị định 30/2020/NĐ-CP:
   
   a) QUỐC HIỆU + TIÊU NGỮ:
      - Phải có ""CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM"" (in hoa, đậm)
      - Phải có ""Độc lập - Tự do - Hạnh phúc"" (có gạch nối, đậm)
      - Kiểm tra viết đúng, không thiếu dấu
   
   b) TÊN CƠ QUAN BAN HÀNH:
      - Phải ghi đúng tên cơ quan (in hoa, đậm)
      - Phải có cơ quan chủ quản phía trên (nếu có)
   
   c) SỐ VÀ KÝ HIỆU:
      - Format chuẩn: ""Số: XX/Loại VB-Chữ viết tắt đơn vị""
      - VD đúng: ""Số: 15/QĐ-UBND"", ""Số: 123/BC-VP""
      - Sai: ""CV số 01"", ""Số 15-QĐ/UBND"", thiếu loại VB
      - 29 loại VB theo NĐ 30: NQ, QĐ, CT (Chỉ thị), QC, QyĐ, TC, TB, HD, CTr (Chương trình), KH, PA, ĐA, DA, BC, BB, TTr, HĐ, CV, CĐ, BGN, BTT, GUQ, GM, GGT, GNP, PG, PC, PB, TC
   
   d) ĐỊA DANH VÀ NGÀY THÁNG:
      - Format: ""Địa danh, ngày ... tháng ... năm ..."" (viết đầy đủ, không viết tắt)
      - VD đúng: ""Gia Kiệm, ngày 15 tháng 3 năm 2026""
      - Sai: ""ngày 15/3/2026"", ""15-3-2026"", viết tắt
   
   e) TÊN LOẠI VĂN BẢN + TRÍCH YẾU:
      - Tên loại VB phải in hoa, đậm, đúng 1 trong 29 loại theo NĐ 30
      - Trích yếu phải ngắn gọn, phản ánh nội dung chính
   
   f) NƠI NHẬN:
      - Phải liệt kê đầy đủ cơ quan liên quan
      - PHẢI có ""Lưu: VT, ..."" ở cuối nơi nhận (bắt buộc theo NĐ 30)
      - Ghi đúng tên cơ quan, không viết tắt tùy tiện
   
   g) CHỮ KÝ + CHỨC DANH:
      - Chức danh người ký phải đúng thẩm quyền
      - Ký thay (KT.), ký thừa lệnh (TL.), ký thừa ủy quyền (TUQ.) phải đúng quy định

MỨC ĐỘ:
- ""critical"": 🔴 Nghiêm trọng — PHẢI sửa (xung đột, sai pháp luật, vượt thẩm quyền, sai thể thức nghiêm trọng)
- ""warning"": 🟡 Cần xem xét — NÊN sửa (thiếu thành phần, văn phong, logic, sai format nhẹ)
- ""suggestion"": 🟢 Gợi ý — TÙY CHỌN (cải thiện, bổ sung)

TRẢ VỀ JSON ĐÚNG FORMAT SAU (KHÔNG markdown, KHÔNG code block):
{
  ""overall_score"": <1-10>,
  ""summary"": ""<Nhận xét tổng thể 1-2 câu>"",
  ""strengths"": [""<Điểm mạnh 1>"", ""<Điểm mạnh 2>""],
  ""issues"": [
    {
      ""severity"": ""critical|warning|suggestion"",
      ""category"": ""spelling|style|conflict|logic|missing|ambiguous|enhancement|format"",
      ""location"": ""<Vị trí: Điều X / Khoản Y / Đoạn Z / Phần thể thức>"",
      ""original_text"": ""<Đoạn text gốc có vấn đề>"",
      ""description"": ""<Mô tả vấn đề>"",
      ""suggestion"": ""<Đề xuất sửa/nội dung thay thế>"",
      ""reason"": ""<Lý do / căn cứ (trích NĐ 30/2020 nếu liên quan thể thức)>""
    }
  ],
  ""suggested_content"": ""<Toàn bộ nội dung văn bản đã sửa và cải thiện, hoặc rỗng nếu không cần sửa nhiều>""
}

QUY TẮC:
- Phải tìm TẤT CẢ lỗi, kể cả lỗi nhỏ
- Mỗi lỗi phải có suggestion cụ thể (không nói chung chung)
- Với lỗi thể thức (format), PHẢI trích dẫn điều khoản NĐ 30/2020 trong reason
- Xếp issues theo mức độ: critical trước, suggestion sau
- overall_score phải phản ánh đúng chất lượng thực tế
- suggested_content: viết lại toàn bộ văn bản đã khắc phục tất cả issues
- TUYỆT ĐỐI KHÔNG dùng markdown trong suggested_content (không dùng **, *, #, ```, -, v.v.)
- suggested_content phải là PLAIN TEXT thuần, giống văn bản hành chính thật sự (in trên giấy)
- Dùng \n để xuống dòng trong suggested_content
- Chỉ trả JSON thuần, KHÔNG wrap trong ```json``` code block";
    }

    internal static string BuildUserPrompt(string content, string documentType, string title, string issuer, string? referenceContent = null)
    {
        var prompt = "KIỂM TRA VĂN BẢN SAU:\n\n";

        if (!string.IsNullOrWhiteSpace(documentType))
            prompt += $"📋 Loại văn bản: {documentType}\n";
        if (!string.IsNullOrWhiteSpace(title))
            prompt += $"📌 Tiêu đề: {title}\n";
        if (!string.IsNullOrWhiteSpace(issuer))
            prompt += $"🏛️ Cơ quan ban hành: {issuer}\n";

        prompt += $"\n--- NỘI DUNG ---\n{content}\n--- HẾT NỘI DUNG ---\n\n";

        // P6: Nếu có file mẫu đối chiếu, thêm vào prompt
        if (!string.IsNullOrWhiteSpace(referenceContent))
        {
            prompt += "\nĐỐI CHIẾU VỚI VĂN BẢN MẪU SAU:\n";
            prompt += "--- VĂN BẢN MẪU ---\n";
            prompt += referenceContent;
            prompt += "\n--- HẾT VĂN BẢN MẪU ---\n\n";
            prompt += "Hãy phân tích toàn diện văn bản cần kiểm tra, đồng thời SO SÁNH với văn bản mẫu.\n";
            prompt += "Chỉ ra những điểm KHÁC BIỆT, THIẾU SÓT so với mẫu, và gợi ý sửa theo mẫu.\n";
            prompt += "Trả về JSON.";
        }
        else
        {
            prompt += "Hãy phân tích toàn diện và trả về JSON.";
        }

        return prompt;
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
