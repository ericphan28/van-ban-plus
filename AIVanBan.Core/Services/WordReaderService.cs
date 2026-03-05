using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service đọc file Word (.docx) — trích xuất text + bảng biểu thành plain text.
/// Dùng cho tính năng tổng hợp báo cáo tháng → quý/năm.
/// </summary>
public class WordReaderService
{
    /// <summary>
    /// Kết quả đọc file Word
    /// </summary>
    public class WordReadResult
    {
        public string FileName { get; set; } = string.Empty;
        public string FullText { get; set; } = string.Empty;
        public List<string> Tables { get; set; } = new();
        public int TableCount => Tables.Count;
        public int ParagraphCount { get; set; }
        public string? ErrorMessage { get; set; }
        public bool Success => string.IsNullOrEmpty(ErrorMessage);
    }

    /// <summary>
    /// Đọc 1 file Word, trích xuất toàn bộ text + bảng biểu
    /// </summary>
    public WordReadResult ReadDocx(string filePath)
    {
        var result = new WordReadResult
        {
            FileName = Path.GetFileName(filePath)
        };

        try
        {
            if (!File.Exists(filePath))
            {
                result.ErrorMessage = $"File không tồn tại: {filePath}";
                return result;
            }

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var wordDoc = WordprocessingDocument.Open(stream, false);

            var body = wordDoc.MainDocumentPart?.Document?.Body;
            if (body == null)
            {
                result.ErrorMessage = "File Word không có nội dung";
                return result;
            }

            var sb = new System.Text.StringBuilder();
            int paraCount = 0;

            // Duyệt từng element trong body theo thứ tự xuất hiện
            foreach (var element in body.ChildElements)
            {
                if (element is Paragraph para)
                {
                    var text = GetParagraphText(para);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        sb.AppendLine(text);
                        paraCount++;
                    }
                    else
                    {
                        sb.AppendLine(); // Giữ dòng trống
                    }
                }
                else if (element is Table table)
                {
                    var tableText = ExtractTableAsText(table);
                    sb.AppendLine();
                    sb.AppendLine(tableText);
                    sb.AppendLine();
                    result.Tables.Add(tableText);
                }
            }

            result.FullText = sb.ToString().Trim();
            result.ParagraphCount = paraCount;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Lỗi đọc file {result.FileName}: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Đọc nhiều file Word, gộp nội dung thành 1 chuỗi — dùng cho tổng hợp BC tháng → quý
    /// </summary>
    public string ReadAndMergeMultipleDocx(IEnumerable<string> filePaths, bool includeFileName = true)
    {
        var sb = new System.Text.StringBuilder();
        int fileIndex = 0;

        foreach (var filePath in filePaths)
        {
            fileIndex++;
            var result = ReadDocx(filePath);

            if (includeFileName)
            {
                sb.AppendLine($"===== FILE {fileIndex}: {result.FileName} =====");
            }

            if (result.Success)
            {
                sb.AppendLine(result.FullText);
            }
            else
            {
                sb.AppendLine($"(Lỗi: {result.ErrorMessage})");
            }

            sb.AppendLine();
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Trích xuất text từ 1 paragraph, bao gồm cả text trong hyperlinks
    /// </summary>
    private string GetParagraphText(Paragraph para)
    {
        var sb = new System.Text.StringBuilder();

        foreach (var child in para.ChildElements)
        {
            if (child is Run run)
            {
                foreach (var runChild in run.ChildElements)
                {
                    if (runChild is Text text)
                        sb.Append(text.Text);
                    else if (runChild is TabChar)
                        sb.Append('\t');
                    else if (runChild is Break br)
                        sb.Append('\n');
                }
            }
            else if (child is Hyperlink hyperlink)
            {
                foreach (var hlRun in hyperlink.Elements<Run>())
                {
                    foreach (var text in hlRun.Elements<Text>())
                        sb.Append(text.Text);
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Chuyển bảng Word thành dạng text có cấu trúc.
    /// Giữ nguyên cấu trúc hàng-cột cho AI hiểu được.
    /// 
    /// Ví dụ output:
    /// [BẢNG]
    /// | STT | Chỉ tiêu | Kế hoạch | Thực hiện | Tỷ lệ % |
    /// |-----|----------|----------|-----------|---------|
    /// | 1   | Thu NS   | 900 tr   | 850 tr    | 94,4%   |
    /// | 2   | Chi NS   | 700 tr   | 680 tr    | 97,1%   |
    /// [/BẢNG]
    /// </summary>
    private string ExtractTableAsText(Table table)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[BẢNG]");

        var rows = table.Elements<TableRow>().ToList();
        if (rows.Count == 0) return "[BẢNG]\n(Bảng trống)\n[/BẢNG]";

        // Thu thập tất cả dữ liệu cells để tính maxWidth
        var allRowsData = new List<List<string>>();
        int maxCols = 0;

        foreach (var row in rows)
        {
            var cells = row.Elements<TableCell>().ToList();
            var cellTexts = new List<string>();

            foreach (var cell in cells)
            {
                var cellText = GetCellText(cell).Trim();
                cellTexts.Add(cellText);
            }

            allRowsData.Add(cellTexts);
            if (cellTexts.Count > maxCols) maxCols = cellTexts.Count;
        }

        if (maxCols == 0) return "[BẢNG]\n(Bảng trống)\n[/BẢNG]";

        // Tính chiều rộng tối đa mỗi cột (giới hạn 30 ký tự)
        var colWidths = new int[maxCols];
        for (int c = 0; c < maxCols; c++)
        {
            colWidths[c] = 3; // min width
            foreach (var rowData in allRowsData)
            {
                if (c < rowData.Count)
                {
                    var len = rowData[c].Length;
                    if (len > colWidths[c]) colWidths[c] = Math.Min(len, 30);
                }
            }
        }

        // Render bảng dạng markdown-table
        for (int r = 0; r < allRowsData.Count; r++)
        {
            var rowData = allRowsData[r];
            sb.Append("| ");
            for (int c = 0; c < maxCols; c++)
            {
                var cellText = c < rowData.Count ? rowData[c] : "";
                if (cellText.Length > 30) cellText = cellText[..27] + "...";
                sb.Append(cellText.PadRight(colWidths[c]));
                sb.Append(" | ");
            }
            sb.AppendLine();

            // Dòng separator sau header row (dòng đầu tiên)
            if (r == 0)
            {
                sb.Append("| ");
                for (int c = 0; c < maxCols; c++)
                {
                    sb.Append(new string('-', colWidths[c]));
                    sb.Append(" | ");
                }
                sb.AppendLine();
            }
        }

        sb.AppendLine("[/BẢNG]");
        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Trích xuất text từ 1 cell trong bảng (có thể chứa nhiều paragraphs)
    /// </summary>
    private string GetCellText(TableCell cell)
    {
        var parts = new List<string>();

        foreach (var para in cell.Elements<Paragraph>())
        {
            var text = GetParagraphText(para);
            if (!string.IsNullOrWhiteSpace(text))
                parts.Add(text.Trim());
        }

        // Nối nhiều paragraph trong 1 cell bằng "; " để giữ trên 1 dòng
        return string.Join("; ", parts);
    }

    /// <summary>
    /// Chỉ đọc các bảng từ file Word (không đọc text thường)
    /// </summary>
    public List<string> ExtractTablesOnly(string filePath)
    {
        var result = ReadDocx(filePath);
        return result.Tables;
    }

    /// <summary>
    /// Trích xuất tóm tắt nhanh từ file Word: dòng đầu, số bảng, số paragraphs
    /// </summary>
    public string GetQuickSummary(string filePath)
    {
        var result = ReadDocx(filePath);
        if (!result.Success) return result.ErrorMessage!;

        var firstLine = result.FullText.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "(trống)";
        if (firstLine.Length > 80) firstLine = firstLine[..77] + "...";

        return $"📄 {result.FileName}: {result.ParagraphCount} đoạn, {result.TableCount} bảng biểu — \"{firstLine}\"";
    }
}
