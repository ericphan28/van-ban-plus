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
