using ClosedXML.Excel;
using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service xuất dữ liệu văn bản ra Excel (.xlsx) sử dụng ClosedXML.
/// Hỗ trợ: danh sách VB (lọc), báo cáo thống kê tổng hợp.
/// </summary>
public class ExcelExportService
{
    #region 1. Xuất danh sách văn bản

    /// <summary>
    /// Xuất danh sách văn bản ra file Excel.
    /// Gồm: Số VB, Tiêu đề, Loại, Hướng, Ngày BH, Cơ quan, Mức khẩn, Độ mật, Hạn XL, Trạng thái, Người XL.
    /// </summary>
    public void ExportDocumentList(List<Document> documents, string filePath, string sheetTitle = "Danh sách văn bản")
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(sheetTitle);

        // === TIÊU ĐỀ BÁO CÁO ===
        ws.Cell(1, 1).Value = "DANH SÁCH VĂN BẢN";
        ws.Range(1, 1, 1, 12).Merge().Style
            .Font.SetBold(true).Font.SetFontSize(16)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell(2, 1).Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm} — Tổng: {documents.Count} văn bản";
        ws.Range(2, 1, 2, 12).Merge().Style
            .Font.SetItalic(true).Font.SetFontSize(10)
            .Font.SetFontColor(XLColor.Gray)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        // === HEADER ROW ===
        var headers = new[]
        {
            "STT", "Số văn bản", "Tiêu đề", "Loại VB", "Hướng",
            "Ngày ban hành", "Cơ quan BH", "Mức khẩn", "Độ mật",
            "Hạn xử lý", "Trạng thái", "Người xử lý"
        };

        int headerRow = 4;
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(headerRow, i + 1);
            cell.Value = headers[i];
        }

        var headerRange = ws.Range(headerRow, 1, headerRow, headers.Length);
        StyleHeaderRow(headerRange);

        // === DATA ROWS ===
        int row = headerRow + 1;
        int stt = 1;
        foreach (var doc in documents.OrderBy(d => d.IssueDate))
        {
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = doc.Number ?? "";
            ws.Cell(row, 3).Value = doc.Title ?? "";
            ws.Cell(row, 4).Value = doc.Type.GetDisplayName();
            ws.Cell(row, 5).Value = doc.Direction.GetDisplayName();
            ws.Cell(row, 6).Value = doc.IssueDate.ToString("dd/MM/yyyy");
            ws.Cell(row, 7).Value = doc.Issuer ?? "";
            ws.Cell(row, 8).Value = doc.UrgencyLevel.GetDisplayName();
            ws.Cell(row, 9).Value = doc.SecurityLevel.GetDisplayName();
            ws.Cell(row, 10).Value = doc.DueDate?.ToString("dd/MM/yyyy") ?? "";
            ws.Cell(row, 11).Value = doc.WorkflowStatus.GetDisplayName();
            ws.Cell(row, 12).Value = doc.AssignedTo ?? "";

            // Highlight quá hạn
            if (doc.DueDate.HasValue && doc.DueDate.Value.Date < DateTime.Today
                && doc.WorkflowStatus != DocumentStatus.Archived
                && doc.WorkflowStatus != DocumentStatus.Published)
            {
                ws.Range(row, 1, row, headers.Length).Style
                    .Fill.SetBackgroundColor(XLColor.FromArgb(255, 235, 238)); // Light red
            }

            // Alternate row color
            if (stt % 2 == 0)
            {
                ws.Range(row, 1, row, headers.Length).Style
                    .Fill.SetBackgroundColor(XLColor.FromArgb(250, 250, 250));
            }

            row++;
        }

        // === FOOTER: TỔNG ===
        ws.Cell(row + 1, 1).Value = $"Tổng cộng: {documents.Count} văn bản";
        ws.Range(row + 1, 1, row + 1, 4).Merge().Style
            .Font.SetBold(true).Font.SetItalic(true);

        // === FORMAT ===
        StyleDataArea(ws, headerRow, row - 1, headers.Length);
        ws.Column(1).Width = 5;   // STT
        ws.Column(2).Width = 18;  // Số VB
        ws.Column(3).Width = 40;  // Tiêu đề
        ws.Column(4).Width = 18;  // Loại
        ws.Column(5).Width = 10;  // Hướng
        ws.Column(6).Width = 14;  // Ngày BH
        ws.Column(7).Width = 25;  // Cơ quan
        ws.Column(8).Width = 14;  // Khẩn
        ws.Column(9).Width = 14;  // Mật
        ws.Column(10).Width = 14; // Hạn XL
        ws.Column(11).Width = 16; // Trạng thái
        ws.Column(12).Width = 20; // Người XL

        workbook.SaveAs(filePath);
    }

    #endregion

    #region 2. Xuất báo cáo thống kê tổng hợp (nhiều sheet)

    /// <summary>
    /// Xuất báo cáo thống kê tổng hợp gồm nhiều sheet:
    /// Sheet 1: Tổng quan kỳ (so sánh kỳ trước)
    /// Sheet 2: Phân loại VB theo loại
    /// Sheet 3: Mức khẩn & Độ mật
    /// Sheet 4: Xu hướng 12 tháng
    /// Sheet 5: Danh sách VB chi tiết
    /// </summary>
    public void ExportStatisticsReport(
        List<Document> periodDocs,
        List<Document> prevPeriodDocs,
        List<Document> allDocs,
        string periodLabel,
        string filePath)
    {
        using var workbook = new XLWorkbook();

        // Sheet 1: Tổng quan
        BuildOverviewSheet(workbook, periodDocs, prevPeriodDocs, periodLabel);

        // Sheet 2: Phân loại theo loại VB
        BuildTypeBreakdownSheet(workbook, periodDocs, periodLabel);

        // Sheet 3: Mức khẩn & Độ mật
        BuildUrgencySecuritySheet(workbook, periodDocs, periodLabel);

        // Sheet 4: Xu hướng 12 tháng
        BuildMonthlyTrendSheet(workbook, allDocs);

        // Sheet 5: Danh sách chi tiết
        ExportDocumentListToSheet(workbook, periodDocs, $"DS VB ({periodLabel})");

        workbook.SaveAs(filePath);
    }

    private void BuildOverviewSheet(XLWorkbook wb, List<Document> current, List<Document> previous, string periodLabel)
    {
        var ws = wb.Worksheets.Add("Tổng quan");

        // Title
        ws.Cell(1, 1).Value = $"BÁO CÁO THỐNG KÊ VĂN BẢN — {periodLabel.ToUpper()}";
        ws.Range(1, 1, 1, 6).Merge().Style
            .Font.SetBold(true).Font.SetFontSize(16)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell(2, 1).Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Range(2, 1, 2, 6).Merge().Style
            .Font.SetItalic(true).Font.SetFontSize(10)
            .Font.SetFontColor(XLColor.Gray)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        // === SO SÁNH KỲ ===
        int row = 4;
        ws.Cell(row, 1).Value = "CHỈ TIÊU";
        ws.Cell(row, 2).Value = "KỲ NÀY";
        ws.Cell(row, 3).Value = "KỲ TRƯỚC";
        ws.Cell(row, 4).Value = "CHÊNH LỆCH";
        ws.Cell(row, 5).Value = "% THAY ĐỔI";
        StyleHeaderRow(ws.Range(row, 1, row, 5));

        var categories = new (string label, Func<Document, bool> filter)[]
        {
            ("VB Đến", d => d.Direction == Direction.Den),
            ("VB Đi", d => d.Direction == Direction.Di),
            ("Nội bộ", d => d.Direction == Direction.NoiBo),
            ("TỔNG CỘNG", _ => true)
        };

        foreach (var (label, filter) in categories)
        {
            row++;
            int cur = current.Count(filter);
            int prev = previous.Count(filter);
            int diff = cur - prev;
            double pct = prev > 0 ? (double)diff / prev * 100 : (cur > 0 ? 100 : 0);

            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 2).Value = cur;
            ws.Cell(row, 3).Value = prev;
            ws.Cell(row, 4).Value = diff;
            ws.Cell(row, 5).Value = $"{pct:+0.0;-0.0;0}%";

            if (label == "TỔNG CỘNG")
                ws.Range(row, 1, row, 5).Style.Font.SetBold(true);

            // Color diff
            if (diff > 0)
                ws.Cell(row, 4).Style.Font.SetFontColor(XLColor.Green);
            else if (diff < 0)
                ws.Cell(row, 4).Style.Font.SetFontColor(XLColor.Red);
        }

        // === HIỆU SUẤT XỬ LÝ ===
        row += 2;
        ws.Cell(row, 1).Value = "HIỆU SUẤT XỬ LÝ VĂN BẢN ĐẾN";
        ws.Range(row, 1, row, 5).Merge().Style.Font.SetBold(true).Font.SetFontSize(13);

        row++;
        var docsWithDue = current.Where(d => d.Direction == Direction.Den && d.DueDate.HasValue).ToList();
        int onTime = docsWithDue.Count(d =>
            d.WorkflowStatus == DocumentStatus.Archived ||
            d.WorkflowStatus == DocumentStatus.Published ||
            d.DueDate!.Value.Date >= DateTime.Today);
        int overdue = docsWithDue.Count - onTime;

        ws.Cell(row, 1).Value = "Chỉ tiêu";
        ws.Cell(row, 2).Value = "Giá trị";
        StyleHeaderRow(ws.Range(row, 1, row, 2));

        ws.Cell(++row, 1).Value = "Tổng VB có hạn XL";
        ws.Cell(row, 2).Value = docsWithDue.Count;

        ws.Cell(++row, 1).Value = "Đúng hạn";
        ws.Cell(row, 2).Value = onTime;
        if (docsWithDue.Count > 0)
        {
            ws.Cell(row, 3).Value = $"{(double)onTime / docsWithDue.Count * 100:F0}%";
            ws.Cell(row, 3).Style.Font.SetFontColor(XLColor.Green);
        }

        ws.Cell(++row, 1).Value = "Quá hạn";
        ws.Cell(row, 2).Value = overdue;
        if (overdue > 0)
            ws.Cell(row, 2).Style.Font.SetFontColor(XLColor.Red);

        // Auto fit
        ws.Column(1).Width = 25;
        ws.Column(2).Width = 15;
        ws.Column(3).Width = 15;
        ws.Column(4).Width = 15;
        ws.Column(5).Width = 15;
    }

    private void BuildTypeBreakdownSheet(XLWorkbook wb, List<Document> docs, string periodLabel)
    {
        var ws = wb.Worksheets.Add("Phân loại VB");

        ws.Cell(1, 1).Value = $"PHÂN LOẠI VĂN BẢN — {periodLabel.ToUpper()}";
        ws.Range(1, 1, 1, 5).Merge().Style
            .Font.SetBold(true).Font.SetFontSize(14)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        // Headers
        int headerRow = 3;
        ws.Cell(headerRow, 1).Value = "Loại văn bản";
        ws.Cell(headerRow, 2).Value = "VB Đến";
        ws.Cell(headerRow, 3).Value = "VB Đi";
        ws.Cell(headerRow, 4).Value = "Nội bộ";
        ws.Cell(headerRow, 5).Value = "Tổng";
        StyleHeaderRow(ws.Range(headerRow, 1, headerRow, 5));

        // Data
        var grouped = docs.GroupBy(d => d.Type).OrderByDescending(g => g.Count()).ToList();
        int row = headerRow + 1;
        int totalDen = 0, totalDi = 0, totalNB = 0;

        foreach (var g in grouped)
        {
            int den = g.Count(d => d.Direction == Direction.Den);
            int di = g.Count(d => d.Direction == Direction.Di);
            int nb = g.Count(d => d.Direction == Direction.NoiBo);

            ws.Cell(row, 1).Value = g.Key.GetDisplayName();
            ws.Cell(row, 2).Value = den;
            ws.Cell(row, 3).Value = di;
            ws.Cell(row, 4).Value = nb;
            ws.Cell(row, 5).Value = den + di + nb;

            totalDen += den;
            totalDi += di;
            totalNB += nb;
            row++;
        }

        // Total row
        ws.Cell(row, 1).Value = "TỔNG CỘNG";
        ws.Cell(row, 2).Value = totalDen;
        ws.Cell(row, 3).Value = totalDi;
        ws.Cell(row, 4).Value = totalNB;
        ws.Cell(row, 5).Value = totalDen + totalDi + totalNB;
        ws.Range(row, 1, row, 5).Style.Font.SetBold(true)
            .Fill.SetBackgroundColor(XLColor.FromArgb(232, 245, 233));

        StyleDataArea(ws, headerRow, row, 5);
        ws.Column(1).Width = 30;
        ws.Columns(2, 5).Width = 12;
    }

    private void BuildUrgencySecuritySheet(XLWorkbook wb, List<Document> docs, string periodLabel)
    {
        var ws = wb.Worksheets.Add("Khẩn & Mật");
        int total = Math.Max(1, docs.Count);

        // === MỨC ĐỘ KHẨN ===
        ws.Cell(1, 1).Value = $"MỨC ĐỘ KHẨN — {periodLabel.ToUpper()}";
        ws.Range(1, 1, 1, 3).Merge().Style.Font.SetBold(true).Font.SetFontSize(14);

        int headerRow = 3;
        ws.Cell(headerRow, 1).Value = "Mức độ";
        ws.Cell(headerRow, 2).Value = "Số lượng";
        ws.Cell(headerRow, 3).Value = "Tỷ lệ";
        StyleHeaderRow(ws.Range(headerRow, 1, headerRow, 3));

        int row = headerRow + 1;
        foreach (var level in Enum.GetValues<UrgencyLevel>())
        {
            int count = docs.Count(d => d.UrgencyLevel == level);
            ws.Cell(row, 1).Value = level.GetDisplayName();
            ws.Cell(row, 2).Value = count;
            ws.Cell(row, 3).Value = $"{(double)count / total * 100:F0}%";
            row++;
        }

        // === ĐỘ MẬT ===
        row += 2;
        ws.Cell(row, 1).Value = $"ĐỘ MẬT — {periodLabel.ToUpper()}";
        ws.Range(row, 1, row, 3).Merge().Style.Font.SetBold(true).Font.SetFontSize(14);

        row += 2;
        ws.Cell(row, 1).Value = "Độ mật";
        ws.Cell(row, 2).Value = "Số lượng";
        ws.Cell(row, 3).Value = "Tỷ lệ";
        StyleHeaderRow(ws.Range(row, 1, row, 3));

        row++;
        foreach (var level in Enum.GetValues<SecurityLevel>())
        {
            int count = docs.Count(d => d.SecurityLevel == level);
            ws.Cell(row, 1).Value = level.GetDisplayName();
            ws.Cell(row, 2).Value = count;
            ws.Cell(row, 3).Value = $"{(double)count / total * 100:F0}%";
            row++;
        }

        ws.Column(1).Width = 20;
        ws.Column(2).Width = 12;
        ws.Column(3).Width = 10;
    }

    private void BuildMonthlyTrendSheet(XLWorkbook wb, List<Document> allDocs)
    {
        var ws = wb.Worksheets.Add("Xu hướng 12 tháng");

        ws.Cell(1, 1).Value = "DIỄN BIẾN 12 THÁNG GẦN NHẤT";
        ws.Range(1, 1, 1, 6).Merge().Style
            .Font.SetBold(true).Font.SetFontSize(14)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        int headerRow = 3;
        ws.Cell(headerRow, 1).Value = "Tháng";
        ws.Cell(headerRow, 2).Value = "VB Đến";
        ws.Cell(headerRow, 3).Value = "VB Đi";
        ws.Cell(headerRow, 4).Value = "Nội bộ";
        ws.Cell(headerRow, 5).Value = "Tổng";
        ws.Cell(headerRow, 6).Value = "± Kỳ trước";
        StyleHeaderRow(ws.Range(headerRow, 1, headerRow, 6));

        var now = DateTime.Now;
        int row = headerRow + 1;

        for (int i = 11; i >= 0; i--)
        {
            var target = now.AddMonths(-i);
            var mStart = new DateTime(target.Year, target.Month, 1);
            var mEnd = mStart.AddMonths(1).AddTicks(-1);

            var monthDocs = allDocs.Where(d => d.IssueDate >= mStart && d.IssueDate <= mEnd).ToList();
            int curTotal = monthDocs.Count;

            var prevTarget = target.AddMonths(-1);
            var prevStart = new DateTime(prevTarget.Year, prevTarget.Month, 1);
            var prevEnd = prevStart.AddMonths(1).AddTicks(-1);
            int prevTotal = allDocs.Count(d => d.IssueDate >= prevStart && d.IssueDate <= prevEnd);

            int diff = curTotal - prevTotal;

            ws.Cell(row, 1).Value = $"T{target.Month:D2}/{target.Year}";
            ws.Cell(row, 2).Value = monthDocs.Count(d => d.Direction == Direction.Den);
            ws.Cell(row, 3).Value = monthDocs.Count(d => d.Direction == Direction.Di);
            ws.Cell(row, 4).Value = monthDocs.Count(d => d.Direction == Direction.NoiBo);
            ws.Cell(row, 5).Value = curTotal;
            ws.Cell(row, 6).Value = diff;

            if (diff > 0)
                ws.Cell(row, 6).Style.Font.SetFontColor(XLColor.Green);
            else if (diff < 0)
                ws.Cell(row, 6).Style.Font.SetFontColor(XLColor.Red);

            row++;
        }

        StyleDataArea(ws, headerRow, row - 1, 6);
        ws.Column(1).Width = 14;
        ws.Columns(2, 6).Width = 12;
    }

    /// <summary>
    /// Thêm sheet danh sách VB vào workbook đã có (dùng bởi báo cáo thống kê).
    /// </summary>
    private void ExportDocumentListToSheet(XLWorkbook wb, List<Document> documents, string sheetName)
    {
        var ws = wb.Worksheets.Add(sheetName);

        var headers = new[]
        {
            "STT", "Số VB", "Tiêu đề", "Loại", "Hướng",
            "Ngày BH", "Cơ quan", "Khẩn", "Mật", "Hạn XL", "Trạng thái"
        };

        int headerRow = 1;
        for (int i = 0; i < headers.Length; i++)
            ws.Cell(headerRow, i + 1).Value = headers[i];

        StyleHeaderRow(ws.Range(headerRow, 1, headerRow, headers.Length));

        int row = 2;
        int stt = 1;
        foreach (var doc in documents.OrderBy(d => d.IssueDate))
        {
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = doc.Number ?? "";
            ws.Cell(row, 3).Value = doc.Title ?? "";
            ws.Cell(row, 4).Value = doc.Type.GetDisplayName();
            ws.Cell(row, 5).Value = doc.Direction.GetDisplayName();
            ws.Cell(row, 6).Value = doc.IssueDate.ToString("dd/MM/yyyy");
            ws.Cell(row, 7).Value = doc.Issuer ?? "";
            ws.Cell(row, 8).Value = doc.UrgencyLevel.GetDisplayName();
            ws.Cell(row, 9).Value = doc.SecurityLevel.GetDisplayName();
            ws.Cell(row, 10).Value = doc.DueDate?.ToString("dd/MM/yyyy") ?? "";
            ws.Cell(row, 11).Value = doc.WorkflowStatus.GetDisplayName();
            row++;
        }

        StyleDataArea(ws, headerRow, row - 1, headers.Length);
        ws.Column(1).Width = 5;
        ws.Column(2).Width = 16;
        ws.Column(3).Width = 35;
        ws.Column(4).Width = 16;
        ws.Column(5).Width = 10;
        ws.Column(6).Width = 13;
        ws.Column(7).Width = 22;
        ws.Column(8).Width = 12;
        ws.Column(9).Width = 12;
        ws.Column(10).Width = 13;
        ws.Column(11).Width = 15;
    }

    #endregion

    #region Styling Helpers

    private static void StyleHeaderRow(IXLRange range)
    {
        range.Style
            .Font.SetBold(true)
            .Font.SetFontColor(XLColor.White)
            .Font.SetFontSize(11)
            .Fill.SetBackgroundColor(XLColor.FromArgb(55, 71, 79)) // #37474F
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
            .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
            .Border.SetOutsideBorderColor(XLColor.FromArgb(55, 71, 79));
    }

    private static void StyleDataArea(IXLWorksheet ws, int headerRow, int lastDataRow, int colCount)
    {
        if (lastDataRow < headerRow) return;

        var dataRange = ws.Range(headerRow + 1, 1, lastDataRow, colCount);
        dataRange.Style
            .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
            .Border.SetInsideBorder(XLBorderStyleValues.Thin)
            .Border.SetOutsideBorderColor(XLColor.FromArgb(224, 224, 224))
            .Border.SetInsideBorderColor(XLColor.FromArgb(224, 224, 224))
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        // Numeric columns center align
        for (int r = headerRow + 1; r <= lastDataRow; r++)
        {
            ws.Cell(r, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center); // STT
        }
    }

    #endregion
}
