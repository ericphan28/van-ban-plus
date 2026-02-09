using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using AIVanBan.Core.Models;
using WordDoc = DocumentFormat.OpenXml.Wordprocessing.Document;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service xuất nội dung cuộc họp ra file Word (.docx)
/// Hỗ trợ 4 loại xuất:
/// 1. Biên bản cuộc họp (đầy đủ theo chuẩn hành chính VN)
/// 2. Thông báo kết luận cuộc họp
/// 3. Báo cáo tổng hợp cuộc họp (nội bộ)
/// 4. Tổng hợp nhiều cuộc họp (danh sách)
/// Định dạng: Times New Roman, line spacing 1.3, margins theo Thông tư 01/2011/TT-BNV
/// </summary>
public class MeetingWordExportService
{
    // ======= FONT & SPACING CONSTANTS =======
    private const string FontFamily = "Times New Roman";
    private const string FontSize16 = "32";  // 16pt = 32 half-points (tiêu đề lớn)
    private const string FontSize14 = "28";  // 14pt (body text chuẩn)
    private const string FontSize13 = "26";  // 13pt (tiêu đề phụ, table)
    private const string FontSize12 = "24";  // 12pt (nơi nhận)
    private const string FontSize11 = "22";  // 11pt
    private const string LineSpacing13 = "312"; // 1.3 lines
    private const string SingleLine = "240";
    private const string SpacingSmall = "80";
    
    // ======= TABLE LAYOUT CONSTANTS =======
    // A4 = 11906 twips width. Margins: Left 1134 (2cm) + Right 567 (1cm) = 1701
    // Available width = 11906 - 1701 = 10205 twips ≈ 10200
    private const int PageAvailableWidth = 10200;
    
    // Color cho header row
    private const string HeaderBgColor = "D9E2F3"; // Xanh nhạt chuyên nghiệp
    private const string BorderColorBlack = "000000"; // Viền đen
    
    // ========================================================================
    // PUBLIC: 1. Xuất Biên bản cuộc họp
    // ========================================================================
    
    public void ExportBienBan(Meeting meeting, string outputPath)
    {
        using var wordDoc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
        var mainPart = wordDoc.AddMainDocumentPart();
        mainPart.Document = new WordDoc();
        var body = mainPart.Document.AppendChild(new Body());
        SetPageMargins(body);
        
        // === QUỐC HIỆU + TIÊU NGỮ ===
        AddQuocHieu(body);
        AddOrgHeader(body, meeting.OrganizingUnit);
        AddDateLine(body, meeting.StartTime);
        
        // === BIÊN BẢN ===
        AddCenteredBold(body, "BIÊN BẢN", FontSize16);
        AddCenteredText(body, meeting.Title, FontSize14, true);
        AddEmptyLine(body);
        
        // === Thông tin chung ===
        AddParagraph(body, $"Thời gian: {MeetingHelper.FormatTimeRange(meeting.StartTime, meeting.EndTime)}, " +
            $"{MeetingHelper.GetVietnameseDayOfWeek(meeting.StartTime)}, ngày {meeting.StartTime:dd} tháng {meeting.StartTime:MM} năm {meeting.StartTime:yyyy}.", false);
        AddParagraph(body, $"Địa điểm: {meeting.Location}.", false);
        AddParagraph(body, $"Chủ trì: {meeting.ChairPerson}" +
            (string.IsNullOrEmpty(meeting.ChairPersonTitle) ? "" : $" - {meeting.ChairPersonTitle}") + ".", false);
        AddParagraph(body, $"Thư ký: {meeting.Secretary}.", false);
        AddEmptyLine(body);
        
        // === I. Thành phần tham dự ===
        AddBoldTitle(body, "I. THÀNH PHẦN THAM DỰ");
        if (meeting.Attendees != null && meeting.Attendees.Count > 0)
        {
            var attended = meeting.Attendees.Where(a =>
                a.AttendanceStatus == AttendanceStatus.Attended ||
                a.AttendanceStatus == AttendanceStatus.Confirmed ||
                a.AttendanceStatus == AttendanceStatus.Invited).ToList();
            var absent = meeting.Attendees.Where(a =>
                a.AttendanceStatus == AttendanceStatus.Absent ||
                a.AttendanceStatus == AttendanceStatus.AbsentWithPermission ||
                a.AttendanceStatus == AttendanceStatus.Delegated).ToList();
            
            AddParagraph(body, $"Tổng số: {meeting.Attendees.Count} người; Có mặt: {attended.Count}; Vắng mặt: {absent.Count}.", false);
            AddEmptyLine(body);
            AddAttendeeTable(body, meeting.Attendees);
            AddEmptyLine(body);
            
            if (absent.Count > 0)
            {
                AddParagraph(body, "Vắng mặt:", true);
                foreach (var a in absent)
                {
                    var reason = a.AttendanceStatus == AttendanceStatus.AbsentWithPermission ? " (có phép)" :
                                 a.AttendanceStatus == AttendanceStatus.Delegated ? " (ủy quyền)" : "";
                    var note = string.IsNullOrEmpty(a.Note) ? "" : $". {a.Note}";
                    AddParagraph(body, $"- {a.Name} - {a.Position}, {a.Unit}{reason}{note}", false);
                }
                AddEmptyLine(body);
            }
        }
        
        // === II. Nội dung ===
        AddBoldTitle(body, "II. NỘI DUNG CUỘC HỌP");
        if (!string.IsNullOrEmpty(meeting.Agenda))
        {
            AddParagraph(body, "Chương trình cuộc họp:", true);
            AddMultiLineParagraph(body, meeting.Agenda);
            AddEmptyLine(body);
        }
        if (!string.IsNullOrEmpty(meeting.Content))
        {
            AddParagraph(body, "Diễn biến cuộc họp:", true);
            AddMultiLineParagraph(body, meeting.Content);
            AddEmptyLine(body);
        }
        
        // === III. Kết luận ===
        AddBoldTitle(body, "III. KẾT LUẬN CUỘC HỌP");
        if (!string.IsNullOrEmpty(meeting.Conclusion))
        {
            AddMultiLineParagraph(body, meeting.Conclusion);
            AddEmptyLine(body);
        }
        
        // === IV. Nhiệm vụ ===
        if (meeting.Tasks != null && meeting.Tasks.Count > 0)
        {
            AddBoldTitle(body, "IV. NHIỆM VỤ ĐƯỢC GIAO");
            AddTaskTable(body, meeting.Tasks);
            AddEmptyLine(body);
        }
        
        // === Kết thúc ===
        var endTime = meeting.EndTime?.ToString("HH:mm") ?? "...";
        AddParagraph(body, $"Cuộc họp kết thúc vào lúc {endTime} cùng ngày.", false);
        AddParagraph(body, "Biên bản được lập thành 02 bản, các thành viên dự họp đã thống nhất nội dung trên.", false);
        AddEmptyLine(body);
        AddEmptyLine(body);
        
        AddSignatureBlock(body, "THƯ KÝ", meeting.Secretary, "CHỦ TRÌ", meeting.ChairPerson);
        
        mainPart.Document.Save();
    }
    
    // ========================================================================
    // PUBLIC: 2. Xuất Thông báo kết luận cuộc họp
    // ========================================================================
    
    public void ExportKetLuan(Meeting meeting, string outputPath)
    {
        using var wordDoc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
        var mainPart = wordDoc.AddMainDocumentPart();
        mainPart.Document = new WordDoc();
        var body = mainPart.Document.AppendChild(new Body());
        SetPageMargins(body);
        
        AddQuocHieu(body);
        AddOrgHeader(body, meeting.OrganizingUnit);
        AddDateLine(body, meeting.StartTime);
        
        AddCenteredBold(body, "THÔNG BÁO", FontSize16);
        AddCenteredText(body, $"Kết luận cuộc họp {MeetingHelper.GetTypeName(meeting.Type).ToLower()}", FontSize14, true);
        AddCenteredText(body, meeting.Title, FontSize14, false);
        AddEmptyLine(body);
        
        AddParagraph(body, $"Ngày {meeting.StartTime:dd} tháng {meeting.StartTime:MM} năm {meeting.StartTime:yyyy}, " +
            $"tại {meeting.Location}, {meeting.OrganizingUnit} đã tổ chức cuộc họp " +
            $"{MeetingHelper.GetTypeName(meeting.Type).ToLower()}: \"{meeting.Title}\".", false);
        AddParagraph(body, $"Chủ trì cuộc họp: {meeting.ChairPerson}" +
            (string.IsNullOrEmpty(meeting.ChairPersonTitle) ? "" : $" - {meeting.ChairPersonTitle}") + ".", false);
        AddEmptyLine(body);
        
        AddParagraph(body, $"Sau khi nghe báo cáo và ý kiến thảo luận của các thành viên dự họp, " +
            $"{(string.IsNullOrEmpty(meeting.ChairPersonTitle) ? "Chủ trì" : meeting.ChairPersonTitle)} kết luận:", false);
        AddEmptyLine(body);
        
        if (!string.IsNullOrEmpty(meeting.Conclusion))
        {
            AddMultiLineParagraph(body, meeting.Conclusion);
            AddEmptyLine(body);
        }
        
        if (meeting.Tasks != null && meeting.Tasks.Count > 0)
        {
            AddParagraph(body, "Các nhiệm vụ cụ thể:", true);
            AddEmptyLine(body);
            AddTaskTable(body, meeting.Tasks);
            AddEmptyLine(body);
        }
        
        AddParagraph(body, "Yêu cầu các đơn vị, cá nhân được phân công nghiêm túc triển khai thực hiện; " +
            "báo cáo kết quả về Văn phòng tổng hợp theo đúng thời hạn quy định./.", false);
        AddEmptyLine(body);
        AddEmptyLine(body);
        
        AddNoiNhanAndSignature(body, meeting);
        
        mainPart.Document.Save();
    }
    
    // ========================================================================
    // PUBLIC: 3. Xuất Báo cáo tổng hợp (nội bộ, đầy đủ mọi thông tin)
    // ========================================================================
    
    public void ExportBaoCaoTongHop(Meeting meeting, string outputPath)
    {
        using var wordDoc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
        var mainPart = wordDoc.AddMainDocumentPart();
        mainPart.Document = new WordDoc();
        var body = mainPart.Document.AppendChild(new Body());
        SetPageMargins(body);
        
        // === HEADER ===
        AddCenteredBold(body, "BÁO CÁO TỔNG HỢP CUỘC HỌP", FontSize16);
        AddEmptyLine(body);
        
        // === I. THÔNG TIN CHUNG (dùng table 2 cột, chuyên nghiệp) ===
        AddBoldTitle(body, "I. THÔNG TIN CHUNG");
        AddInfoTable(body, meeting);
        AddEmptyLine(body);
        
        // === II. THÀNH PHẦN THAM DỰ ===
        if (meeting.Attendees != null && meeting.Attendees.Count > 0)
        {
            AddBoldTitle(body, "II. THÀNH PHẦN THAM DỰ");
            
            var attended = meeting.Attendees.Where(a =>
                a.AttendanceStatus == AttendanceStatus.Attended ||
                a.AttendanceStatus == AttendanceStatus.Confirmed ||
                a.AttendanceStatus == AttendanceStatus.Invited).Count();
            var absent = meeting.Attendees.Count - attended;
            AddParagraph(body, $"Tổng số: {meeting.Attendees.Count} người — Có mặt: {attended} — Vắng: {absent}", false);
            AddEmptyLine(body);
            AddAttendeeTable(body, meeting.Attendees);
            AddEmptyLine(body);
        }
        
        // === III. CHƯƠNG TRÌNH ===
        if (!string.IsNullOrEmpty(meeting.Agenda))
        {
            AddBoldTitle(body, "III. CHƯƠNG TRÌNH CUỘC HỌP");
            AddMultiLineParagraph(body, meeting.Agenda);
            AddEmptyLine(body);
        }
        
        // === IV. NỘI DUNG ===
        if (!string.IsNullOrEmpty(meeting.Content))
        {
            AddBoldTitle(body, "IV. NỘI DUNG CHI TIẾT");
            AddMultiLineParagraph(body, meeting.Content);
            AddEmptyLine(body);
        }
        
        // === V. KẾT LUẬN ===
        if (!string.IsNullOrEmpty(meeting.Conclusion))
        {
            AddBoldTitle(body, "V. KẾT LUẬN");
            AddMultiLineParagraph(body, meeting.Conclusion);
            AddEmptyLine(body);
        }
        
        // === VI. NHIỆM VỤ ===
        if (meeting.Tasks != null && meeting.Tasks.Count > 0)
        {
            AddBoldTitle(body, "VI. NHIỆM VỤ ĐƯỢC GIAO");
            AddTaskTable(body, meeting.Tasks);
            AddEmptyLine(body);
            
            var total = meeting.Tasks.Count;
            var completed = meeting.Tasks.Count(t => t.TaskStatus == MeetingTaskStatus.Completed);
            var inProgress = meeting.Tasks.Count(t => t.TaskStatus == MeetingTaskStatus.InProgress);
            var overdue = meeting.Tasks.Count(t => t.TaskStatus == MeetingTaskStatus.Overdue);
            AddParagraph(body,
                $"Tổng kết: {total} nhiệm vụ — Hoàn thành: {completed}, Đang thực hiện: {inProgress}, Quá hạn: {overdue}",
                true);
            AddEmptyLine(body);
        }
        
        // === VII. TÀI LIỆU ===
        if (meeting.Documents != null && meeting.Documents.Count > 0)
        {
            AddBoldTitle(body, "VII. DANH MỤC TÀI LIỆU");
            AddDocumentTable(body, meeting.Documents);
            AddEmptyLine(body);
        }
        
        // === VIII. GHI CHÚ ===
        if (!string.IsNullOrEmpty(meeting.PersonalNotes))
        {
            AddBoldTitle(body, "VIII. GHI CHÚ CÁ NHÂN");
            AddMultiLineParagraph(body, meeting.PersonalNotes);
            AddEmptyLine(body);
        }
        
        // === FOOTER ===
        if (meeting.Tags != null && meeting.Tags.Length > 0)
            AddParagraph(body, $"Từ khóa: {string.Join(", ", meeting.Tags)}", false);
        
        AddEmptyLine(body);
        AddFooterInfo(body);
        
        mainPart.Document.Save();
    }
    
    // ========================================================================
    // PUBLIC: 4. Xuất Tổng hợp nhiều cuộc họp (dạng bảng tổng hợp)
    // ========================================================================
    
    public void ExportTongHopNhieuCuocHop(List<Meeting> meetings, string outputPath)
    {
        using var wordDoc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
        var mainPart = wordDoc.AddMainDocumentPart();
        mainPart.Document = new WordDoc();
        var body = mainPart.Document.AppendChild(new Body());
        SetPageMargins(body, landscape: true);
        
        // === HEADER ===
        AddCenteredBold(body, "BẢNG TỔNG HỢP CÁC CUỘC HỌP", FontSize16);
        AddCenteredText(body,
            $"Từ ngày {meetings.Min(m => m.StartTime):dd/MM/yyyy} đến ngày {meetings.Max(m => m.StartTime):dd/MM/yyyy}",
            FontSize13, false);
        AddCenteredText(body,
            $"Tổng số: {meetings.Count} cuộc họp",
            FontSize13, true);
        AddEmptyLine(body);
        
        // === BẢNG TỔNG HỢP CHÍNH ===
        // Landscape A4: 16838 - margins (1134+567) = 15137 twips
        var summaryTable = CreateStyledTable(15100,
            700,   // STT
            3500,  // Tên cuộc họp
            1200,  // Loại
            1800,  // Thời gian
            2000,  // Địa điểm
            2000,  // Chủ trì
            1200,  // Hình thức
            1200,  // Trạng thái
            1500); // Nhiệm vụ
        
        // Header row
        var headerRow = CreateHeaderRow(
            "STT", "Tên cuộc họp", "Loại", "Thời gian",
            "Địa điểm", "Chủ trì", "Hình thức", "Trạng thái", "Nhiệm vụ");
        summaryTable.AppendChild(headerRow);
        
        // Data rows
        int stt = 1;
        foreach (var m in meetings.OrderBy(x => x.StartTime))
        {
            var taskSummary = "";
            if (m.Tasks?.Count > 0)
            {
                var done = m.Tasks.Count(t => t.TaskStatus == MeetingTaskStatus.Completed);
                taskSummary = $"{done}/{m.Tasks.Count} xong";
            }
            
            var dataRow = CreateDataRow(
                stt.ToString(),
                m.Title,
                MeetingHelper.GetTypeName(m.Type),
                $"{m.StartTime:dd/MM/yyyy}\n{MeetingHelper.FormatTimeRange(m.StartTime, m.EndTime)}",
                m.Location ?? "",
                m.ChairPerson ?? "",
                MeetingHelper.GetFormatName(m.Format),
                MeetingHelper.GetStatusName(m.Status),
                taskSummary);
            summaryTable.AppendChild(dataRow);
            stt++;
        }
        
        body.AppendChild(summaryTable);
        AddEmptyLine(body);
        
        // === THỐNG KÊ TỔNG HỢP ===
        AddBoldTitle(body, "THỐNG KÊ TỔNG HỢP");
        AddEmptyLine(body);
        
        var statTable = CreateStyledTable(8000, 4500, 3500);
        statTable.AppendChild(CreateHeaderRow("Chỉ tiêu", "Số lượng"));
        
        var scheduled = meetings.Count(m => m.Status == MeetingStatus.Scheduled);
        var inProgress = meetings.Count(m => m.Status == MeetingStatus.InProgress);
        var completed = meetings.Count(m => m.Status == MeetingStatus.Completed);
        var cancelled = meetings.Count(m => m.Status == MeetingStatus.Cancelled);
        var totalTasks = meetings.Sum(m => m.Tasks?.Count ?? 0);
        var completedTasks = meetings.Sum(m => m.Tasks?.Count(t => t.TaskStatus == MeetingTaskStatus.Completed) ?? 0);
        var overdueTasks = meetings.Sum(m => m.Tasks?.Count(t => t.TaskStatus == MeetingTaskStatus.Overdue) ?? 0);
        
        statTable.AppendChild(CreateDataRow("Tổng số cuộc họp", meetings.Count.ToString()));
        statTable.AppendChild(CreateDataRow("Đã lên lịch", scheduled.ToString()));
        statTable.AppendChild(CreateDataRow("Đang diễn ra", inProgress.ToString()));
        statTable.AppendChild(CreateDataRow("Đã hoàn thành", completed.ToString()));
        if (cancelled > 0)
            statTable.AppendChild(CreateDataRow("Đã hủy", cancelled.ToString()));
        statTable.AppendChild(CreateDataRow("Tổng nhiệm vụ", totalTasks.ToString()));
        statTable.AppendChild(CreateDataRow("Nhiệm vụ hoàn thành", completedTasks.ToString()));
        if (overdueTasks > 0)
            statTable.AppendChild(CreateDataRow("Nhiệm vụ quá hạn", overdueTasks.ToString()));
        
        body.AppendChild(statTable);
        AddEmptyLine(body);
        
        // === CHI TIẾT TỪNG CUỘC HỌP (nếu có kết luận/nhiệm vụ) ===
        var meetingsWithContent = meetings
            .Where(m => !string.IsNullOrEmpty(m.Conclusion) || (m.Tasks?.Count > 0))
            .OrderBy(m => m.StartTime)
            .ToList();
        
        if (meetingsWithContent.Count > 0)
        {
            AddBoldTitle(body, "CHI TIẾT KẾT LUẬN VÀ NHIỆM VỤ");
            AddEmptyLine(body);
            
            int idx = 1;
            foreach (var m in meetingsWithContent)
            {
                AddParagraphNoIndent(body, $"{idx}. {m.Title}", true, FontSize14);
                AddParagraphNoIndent(body,
                    $"   Thời gian: {m.StartTime:dd/MM/yyyy} {MeetingHelper.FormatTimeRange(m.StartTime, m.EndTime)} — " +
                    $"Chủ trì: {m.ChairPerson} — Trạng thái: {MeetingHelper.GetStatusName(m.Status)}",
                    false, FontSize13);
                
                if (!string.IsNullOrEmpty(m.Conclusion))
                {
                    AddParagraph(body, "Kết luận:", true);
                    AddMultiLineParagraph(body, m.Conclusion);
                }
                
                if (m.Tasks?.Count > 0)
                {
                    AddParagraph(body, $"Nhiệm vụ ({m.Tasks.Count}):", true);
                    // Dùng bảng nhiệm vụ đầy đủ (landscape có đủ chỗ)
                    AddTaskTable(body, m.Tasks);
                }
                
                AddEmptyLine(body);
                idx++;
            }
        }
        
        // === FOOTER ===
        AddFooterInfo(body);
        
        mainPart.Document.Save();
    }
    
    // ========================================================================
    // PRIVATE: Bảng thông tin chung (2 cột, viền mờ) cho BaoCaoTongHop
    // ========================================================================
    
    private void AddInfoTable(Body body, Meeting meeting)
    {
        // Table 2 cột: Label (3400) | Value (6800) = 10200
        var table = new Table();
        var tblProps = new TableProperties();
        
        tblProps.AppendChild(new TableBorders(
            new TopBorder { Val = BorderValues.Single, Size = 4, Color = "CCCCCC" },
            new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "CCCCCC" },
            new LeftBorder { Val = BorderValues.Single, Size = 4, Color = "CCCCCC" },
            new RightBorder { Val = BorderValues.Single, Size = 4, Color = "CCCCCC" },
            new InsideHorizontalBorder { Val = BorderValues.Dotted, Size = 4, Color = "CCCCCC" },
            new InsideVerticalBorder { Val = BorderValues.Single, Size = 4, Color = "CCCCCC" }
        ));
        tblProps.AppendChild(new TableWidth { Width = PageAvailableWidth.ToString(), Type = TableWidthUnitValues.Dxa });
        tblProps.AppendChild(new TableLayout { Type = TableLayoutValues.Fixed });
        tblProps.AppendChild(new TableCellMarginDefault(
            new TopMargin { Width = "30", Type = TableWidthUnitValues.Dxa },
            new StartMargin { Width = "80", Type = TableWidthUnitValues.Dxa },
            new BottomMargin { Width = "30", Type = TableWidthUnitValues.Dxa },
            new EndMargin { Width = "80", Type = TableWidthUnitValues.Dxa }
        ));
        table.AppendChild(tblProps);
        
        var grid = new TableGrid();
        grid.AppendChild(new GridColumn { Width = "3400" });
        grid.AppendChild(new GridColumn { Width = "6800" });
        table.AppendChild(grid);
        
        // Rows
        AddInfoRow(table, "Tên cuộc họp", meeting.Title, true);
        if (!string.IsNullOrEmpty(meeting.MeetingNumber))
            AddInfoRow(table, "Số giấy mời", meeting.MeetingNumber);
        AddInfoRow(table, "Loại cuộc họp", MeetingHelper.GetTypeName(meeting.Type));
        AddInfoRow(table, "Cấp cuộc họp", MeetingHelper.GetLevelName(meeting.Level));
        AddInfoRow(table, "Trạng thái", MeetingHelper.GetStatusName(meeting.Status));
        AddInfoRow(table, "Mức ưu tiên", MeetingHelper.GetPriorityText(meeting.Priority));
        AddInfoRow(table, "Thời gian",
            $"{MeetingHelper.FormatTimeRange(meeting.StartTime, meeting.EndTime)}, " +
            $"{MeetingHelper.GetVietnameseDayOfWeek(meeting.StartTime)}, ngày {meeting.StartTime:dd}/{meeting.StartTime:MM}/{meeting.StartTime:yyyy}");
        AddInfoRow(table, "Địa điểm", meeting.Location ?? "");
        AddInfoRow(table, "Hình thức", MeetingHelper.GetFormatName(meeting.Format));
        if (!string.IsNullOrEmpty(meeting.OnlineLink))
            AddInfoRow(table, "Link trực tuyến", meeting.OnlineLink);
        AddInfoRow(table, "Chủ trì",
            meeting.ChairPerson + (string.IsNullOrEmpty(meeting.ChairPersonTitle) ? "" : $" - {meeting.ChairPersonTitle}"));
        if (!string.IsNullOrEmpty(meeting.Secretary))
            AddInfoRow(table, "Thư ký", meeting.Secretary);
        AddInfoRow(table, "Đơn vị tổ chức", meeting.OrganizingUnit ?? "");
        
        body.AppendChild(table);
    }
    
    private void AddInfoRow(Table table, string label, string value, bool valueBold = false)
    {
        var row = new TableRow();
        
        // Label cell (nền xám nhạt)
        var labelCell = new TableCell();
        var labelCellProps = new TableCellProperties();
        labelCellProps.AppendChild(new TableCellWidth { Width = "3400", Type = TableWidthUnitValues.Dxa });
        labelCellProps.AppendChild(new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center });
        labelCellProps.AppendChild(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "F2F2F2" });
        labelCell.AppendChild(labelCellProps);
        
        var labelPara = new Paragraph();
        var labelRun = labelPara.AppendChild(new Run());
        labelRun.AppendChild(new Text(label) { Space = SpaceProcessingModeValues.Preserve });
        SetRunProps(labelRun, FontSize13, true);
        var labelProps = labelPara.PrependChild(new ParagraphProperties());
        labelProps.AppendChild(new SpacingBetweenLines { After = "0", Before = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });
        labelCell.AppendChild(labelPara);
        row.AppendChild(labelCell);
        
        // Value cell
        var valueCell = new TableCell();
        var valueCellProps = new TableCellProperties();
        valueCellProps.AppendChild(new TableCellWidth { Width = "6800", Type = TableWidthUnitValues.Dxa });
        valueCellProps.AppendChild(new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center });
        valueCell.AppendChild(valueCellProps);
        
        var valuePara = new Paragraph();
        var valueRun = valuePara.AppendChild(new Run());
        valueRun.AppendChild(new Text(value) { Space = SpaceProcessingModeValues.Preserve });
        SetRunProps(valueRun, FontSize14, valueBold);
        var valueProps = valuePara.PrependChild(new ParagraphProperties());
        valueProps.AppendChild(new SpacingBetweenLines { After = "0", Before = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });
        valueCell.AppendChild(valuePara);
        row.AppendChild(valueCell);
        
        table.AppendChild(row);
    }
    
    // ========================================================================
    // PRIVATE: Bảng thành phần tham dự (10200 twips)
    // ========================================================================
    
    private void AddAttendeeTable(Body body, List<MeetingAttendee> attendees)
    {
        // STT(700) + Họ tên(2800) + Chức vụ(2200) + Đơn vị(2500) + Vai trò(2000) = 10200
        var table = CreateStyledTable(PageAvailableWidth, 700, 2800, 2200, 2500, 2000);
        table.AppendChild(CreateHeaderRow("STT", "Họ và tên", "Chức vụ", "Đơn vị", "Vai trò"));
        
        int stt = 1;
        foreach (var a in attendees)
        {
            var roleName = a.Role switch
            {
                AttendeeRole.ChairPerson => "Chủ trì",
                AttendeeRole.Secretary => "Thư ký",
                AttendeeRole.Presenter => "Báo cáo viên",
                AttendeeRole.Observer => "Dự thính",
                _ => "Tham dự"
            };
            table.AppendChild(CreateDataRow(stt.ToString(), a.Name, a.Position, a.Unit, roleName));
            stt++;
        }
        
        body.AppendChild(table);
    }
    
    // ========================================================================
    // PRIVATE: Bảng nhiệm vụ được giao (10200 twips)
    // ========================================================================
    
    private void AddTaskTable(Body body, List<MeetingTask> tasks)
    {
        // STT(600) + Nội dung(3400) + Người TH(2200) + Hạn(1800) + Trạng thái(2200) = 10200
        var table = CreateStyledTable(PageAvailableWidth, 600, 3400, 2200, 1800, 2200);
        table.AppendChild(CreateHeaderRow("STT", "Nội dung nhiệm vụ", "Người/ĐV thực hiện", "Hạn hoàn thành", "Trạng thái"));
        
        int stt = 1;
        foreach (var task in tasks)
        {
            var assignee = task.AssignedTo ?? "";
            if (!string.IsNullOrEmpty(task.AssignedUnit))
                assignee += $"\n({task.AssignedUnit})";
            
            table.AppendChild(CreateDataRow(
                stt.ToString(),
                task.Title ?? "",
                assignee,
                task.Deadline?.ToString("dd/MM/yyyy") ?? "",
                MeetingHelper.GetTaskStatusName(task.TaskStatus)));
            stt++;
        }
        
        body.AppendChild(table);
    }
    
    // ========================================================================
    // PRIVATE: Bảng danh mục tài liệu (10200 twips)
    // ========================================================================
    
    private void AddDocumentTable(Body body, List<MeetingDocument> docs)
    {
        // STT(600) + Loại(1800) + Trích yếu(3600) + Số hiệu(1800) + Cơ quan BH(2400) = 10200
        var table = CreateStyledTable(PageAvailableWidth, 600, 1800, 3600, 1800, 2400);
        table.AppendChild(CreateHeaderRow("STT", "Loại tài liệu", "Trích yếu nội dung", "Số hiệu", "Cơ quan ban hành"));
        
        int stt = 1;
        foreach (var doc in docs)
        {
            table.AppendChild(CreateDataRow(
                stt.ToString(),
                MeetingHelper.GetDocumentTypeName(doc.DocumentType),
                doc.Title ?? "",
                doc.DocumentNumber ?? "",
                doc.Issuer ?? ""));
            stt++;
        }
        
        body.AppendChild(table);
    }
    
    // ========================================================================
    // TABLE BUILDER: Tạo table chuẩn với borders + cell padding + fixed layout
    // ========================================================================
    
    private Table CreateStyledTable(int totalWidth, params int[] columnWidths)
    {
        var table = new Table();
        var tblProps = new TableProperties();
        
        // Borders đen, nét liền
        tblProps.AppendChild(new TableBorders(
            new TopBorder { Val = BorderValues.Single, Size = 6, Color = BorderColorBlack },
            new BottomBorder { Val = BorderValues.Single, Size = 6, Color = BorderColorBlack },
            new LeftBorder { Val = BorderValues.Single, Size = 6, Color = BorderColorBlack },
            new RightBorder { Val = BorderValues.Single, Size = 6, Color = BorderColorBlack },
            new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4, Color = BorderColorBlack },
            new InsideVerticalBorder { Val = BorderValues.Single, Size = 4, Color = BorderColorBlack }
        ));
        
        // Width chính xác = available page width
        tblProps.AppendChild(new TableWidth { Width = totalWidth.ToString(), Type = TableWidthUnitValues.Dxa });
        
        // Fixed layout → column widths được tôn trọng
        tblProps.AppendChild(new TableLayout { Type = TableLayoutValues.Fixed });
        
        // Cell padding mặc định
        tblProps.AppendChild(new TableCellMarginDefault(
            new TopMargin { Width = "40", Type = TableWidthUnitValues.Dxa },
            new StartMargin { Width = "80", Type = TableWidthUnitValues.Dxa },
            new BottomMargin { Width = "40", Type = TableWidthUnitValues.Dxa },
            new EndMargin { Width = "80", Type = TableWidthUnitValues.Dxa }
        ));
        
        // Căn giữa table trên trang
        tblProps.AppendChild(new TableJustification { Val = TableRowAlignmentValues.Center });
        
        table.AppendChild(tblProps);
        
        // Column grid
        var grid = new TableGrid();
        foreach (var w in columnWidths)
            grid.AppendChild(new GridColumn { Width = w.ToString() });
        table.AppendChild(grid);
        
        return table;
    }
    
    /// <summary>
    /// Header row: nền xanh nhạt, chữ đậm 13pt, căn giữa, chiều cao tối thiểu
    /// </summary>
    private TableRow CreateHeaderRow(params string[] cellTexts)
    {
        var row = new TableRow();
        row.AppendChild(new TableRowProperties(
            new TableRowHeight { Val = 420, HeightType = HeightRuleValues.AtLeast }
        ));
        
        foreach (var text in cellTexts)
        {
            var cell = new TableCell();
            
            var cellProps = new TableCellProperties();
            cellProps.AppendChild(new Shading
            {
                Val = ShadingPatternValues.Clear, Color = "auto", Fill = HeaderBgColor
            });
            cellProps.AppendChild(new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center });
            cell.AppendChild(cellProps);
            
            var para = new Paragraph();
            var run = para.AppendChild(new Run());
            run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            SetRunProps(run, FontSize13, true);
            
            var paraProps = para.PrependChild(new ParagraphProperties());
            paraProps.AppendChild(new Justification { Val = JustificationValues.Center });
            paraProps.AppendChild(new SpacingBetweenLines
            {
                After = "0", Before = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto
            });
            
            cell.AppendChild(para);
            row.AppendChild(cell);
        }
        
        return row;
    }
    
    /// <summary>
    /// Data row: cột đầu (STT) căn giữa, các cột còn lại căn trái.
    /// Hỗ trợ \n trong text → tạo nhiều paragraph trong 1 cell.
    /// </summary>
    private TableRow CreateDataRow(params string[] cellTexts)
    {
        var row = new TableRow();
        row.AppendChild(new TableRowProperties(
            new TableRowHeight { Val = 340, HeightType = HeightRuleValues.AtLeast }
        ));
        
        for (int i = 0; i < cellTexts.Length; i++)
        {
            var cell = new TableCell();
            
            var cellProps = new TableCellProperties();
            cellProps.AppendChild(new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center });
            cell.AppendChild(cellProps);
            
            var text = cellTexts[i] ?? "";
            bool isFirstCol = (i == 0);
            
            var lines = text.Split('\n');
            foreach (var line in lines)
            {
                var para = new Paragraph();
                var run = para.AppendChild(new Run());
                run.AppendChild(new Text(line.Trim()) { Space = SpaceProcessingModeValues.Preserve });
                SetRunProps(run, FontSize13, false);
                
                var paraProps = para.PrependChild(new ParagraphProperties());
                paraProps.AppendChild(new Justification
                {
                    Val = isFirstCol ? JustificationValues.Center : JustificationValues.Left
                });
                paraProps.AppendChild(new SpacingBetweenLines
                {
                    After = "0", Before = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto
                });
                
                cell.AppendChild(para);
            }
            
            row.AppendChild(cell);
        }
        
        return row;
    }
    
    // ========================================================================
    // PRIVATE: Nơi nhận + Chữ ký (cho thông báo kết luận)
    // ========================================================================
    
    private void AddNoiNhanAndSignature(Body body, Meeting meeting)
    {
        var table = new Table();
        var tblProps = new TableProperties(
            new TableBorders(),
            new TableWidth { Width = PageAvailableWidth.ToString(), Type = TableWidthUnitValues.Dxa },
            new TableLayout { Type = TableLayoutValues.Fixed }
        );
        table.AppendChild(tblProps);
        
        var grid = new TableGrid();
        grid.AppendChild(new GridColumn { Width = "5100" });
        grid.AppendChild(new GridColumn { Width = "5100" });
        table.AppendChild(grid);
        
        var row = new TableRow();
        
        // Cột trái: Nơi nhận
        var leftCell = new TableCell();
        leftCell.AppendChild(new TableCellProperties
        {
            TableCellWidth = new TableCellWidth { Width = "5100", Type = TableWidthUnitValues.Dxa },
            TableCellVerticalAlignment = new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Top }
        });
        AddCellParagraph(leftCell, "Nơi nhận:", FontSize12, true, false);
        AddCellParagraph(leftCell, "- Như thành phần dự họp;", FontSize11, false, false);
        AddCellParagraph(leftCell, "- Lưu: VT.", FontSize11, false, false);
        row.AppendChild(leftCell);
        
        // Cột phải: Chữ ký
        var rightCell = new TableCell();
        rightCell.AppendChild(new TableCellProperties
        {
            TableCellWidth = new TableCellWidth { Width = "5100", Type = TableWidthUnitValues.Dxa },
            TableCellVerticalAlignment = new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Top }
        });
        var titleText = string.IsNullOrEmpty(meeting.ChairPersonTitle) ? "CHỦ TRÌ" : meeting.ChairPersonTitle.ToUpper();
        AddCellParagraph(rightCell, titleText, FontSize13, true, true);
        for (int i = 0; i < 4; i++)
        {
            var empty = new Paragraph();
            empty.PrependChild(new ParagraphProperties()).AppendChild(
                new SpacingBetweenLines { After = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });
            rightCell.AppendChild(empty);
        }
        AddCellParagraph(rightCell, meeting.ChairPerson, FontSize14, true, true);
        row.AppendChild(rightCell);
        
        table.AppendChild(row);
        body.AppendChild(table);
    }
    
    // ========================================================================
    // PRIVATE: Khối chữ ký 2 bên (Thư ký + Chủ trì)
    // ========================================================================
    
    private void AddSignatureBlock(Body body, string leftTitle, string leftName, string rightTitle, string rightName)
    {
        var table = new Table();
        var tblProps = new TableProperties(
            new TableBorders(),
            new TableWidth { Width = PageAvailableWidth.ToString(), Type = TableWidthUnitValues.Dxa },
            new TableLayout { Type = TableLayoutValues.Fixed }
        );
        table.AppendChild(tblProps);
        
        var grid = new TableGrid();
        grid.AppendChild(new GridColumn { Width = "5100" });
        grid.AppendChild(new GridColumn { Width = "5100" });
        table.AppendChild(grid);
        
        var row = new TableRow();
        
        var leftCell = new TableCell();
        leftCell.AppendChild(new TableCellProperties
        {
            TableCellWidth = new TableCellWidth { Width = "5100", Type = TableWidthUnitValues.Dxa }
        });
        AddSignatureToCellContent(leftCell, leftTitle, leftName);
        row.AppendChild(leftCell);
        
        var rightCell = new TableCell();
        rightCell.AppendChild(new TableCellProperties
        {
            TableCellWidth = new TableCellWidth { Width = "5100", Type = TableWidthUnitValues.Dxa }
        });
        AddSignatureToCellContent(rightCell, rightTitle, rightName);
        row.AppendChild(rightCell);
        
        table.AppendChild(row);
        body.AppendChild(table);
    }
    
    private void AddSignatureToCellContent(TableCell cell, string title, string name)
    {
        AddCellParagraph(cell, title, FontSize13, true, true);
        for (int i = 0; i < 4; i++)
        {
            var empty = new Paragraph();
            empty.PrependChild(new ParagraphProperties()).AppendChild(
                new SpacingBetweenLines { After = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });
            cell.AppendChild(empty);
        }
        AddCellParagraph(cell, name, FontSize14, true, true);
    }
    
    // ========================================================================
    // PRIVATE: Paragraph helpers
    // ========================================================================
    
    private void SetPageMargins(Body body, bool landscape = false)
    {
        var sectionProps = new SectionProperties();
        if (landscape)
        {
            sectionProps.AppendChild(new PageSize { Width = 16838, Height = 11906, Orient = PageOrientationValues.Landscape });
            sectionProps.AppendChild(new PageMargin { Top = 850, Bottom = 850, Left = 1134, Right = 567, Header = 708, Footer = 708 });
        }
        else
        {
            sectionProps.AppendChild(new PageMargin { Top = 1134, Bottom = 850, Left = 1134, Right = 567, Header = 708, Footer = 708 });
        }
        body.AppendChild(sectionProps);
    }
    
    private void AddQuocHieu(Body body)
    {
        AddCenteredBold(body, "CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM", FontSize14);
        AddCenteredBoldUnderline(body, "Độc lập - Tự do - Hạnh phúc", FontSize13);
        AddEmptyLine(body);
    }
    
    private void AddOrgHeader(Body body, string orgName)
    {
        if (string.IsNullOrEmpty(orgName)) return;
        var para = body.AppendChild(new Paragraph());
        var run = para.AppendChild(new Run());
        run.AppendChild(new Text(orgName.ToUpper()));
        SetRunProps(run, FontSize13, true);
        var paraProps = para.PrependChild(new ParagraphProperties());
        paraProps.AppendChild(new Justification { Val = JustificationValues.Center });
        SetSpacing(paraProps);
    }
    
    private void AddDateLine(Body body, DateTime date)
    {
        var text = $"........., ngày {date:dd} tháng {date:MM} năm {date:yyyy}";
        var para = body.AppendChild(new Paragraph());
        var run = para.AppendChild(new Run());
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        SetRunProps(run, FontSize13, false, true);
        var paraProps = para.PrependChild(new ParagraphProperties());
        paraProps.AppendChild(new Justification { Val = JustificationValues.Right });
        SetSpacing(paraProps);
        AddEmptyLine(body);
    }
    
    private void AddCenteredBold(Body body, string text, string fontSize)
    {
        var para = body.AppendChild(new Paragraph());
        var run = para.AppendChild(new Run());
        run.AppendChild(new Text(text));
        SetRunProps(run, fontSize, true);
        var paraProps = para.PrependChild(new ParagraphProperties());
        paraProps.AppendChild(new Justification { Val = JustificationValues.Center });
        SetSpacing(paraProps, "0");
    }
    
    private void AddCenteredBoldUnderline(Body body, string text, string fontSize)
    {
        var para = body.AppendChild(new Paragraph());
        var run = para.AppendChild(new Run());
        run.AppendChild(new Text(text));
        var runProps = run.PrependChild(new RunProperties());
        runProps.AppendChild(new RunFonts { Ascii = FontFamily, HighAnsi = FontFamily, EastAsia = FontFamily, ComplexScript = FontFamily });
        runProps.AppendChild(new FontSize { Val = fontSize });
        runProps.AppendChild(new FontSizeComplexScript { Val = fontSize });
        runProps.AppendChild(new Bold());
        runProps.AppendChild(new Underline { Val = UnderlineValues.Single });
        var paraProps = para.PrependChild(new ParagraphProperties());
        paraProps.AppendChild(new Justification { Val = JustificationValues.Center });
        SetSpacing(paraProps, "0");
    }
    
    private void AddCenteredText(Body body, string text, string fontSize, bool bold)
    {
        var para = body.AppendChild(new Paragraph());
        var run = para.AppendChild(new Run());
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        SetRunProps(run, fontSize, bold);
        var paraProps = para.PrependChild(new ParagraphProperties());
        paraProps.AppendChild(new Justification { Val = JustificationValues.Center });
        SetSpacing(paraProps, "0");
    }
    
    private void AddBoldTitle(Body body, string text)
    {
        var para = body.AppendChild(new Paragraph());
        var run = para.AppendChild(new Run());
        run.AppendChild(new Text(text));
        SetRunProps(run, FontSize14, true);
        var paraProps = para.PrependChild(new ParagraphProperties());
        paraProps.AppendChild(new Justification { Val = JustificationValues.Both });
        SetSpacing(paraProps);
    }
    
    private void AddParagraph(Body body, string text, bool bold)
    {
        var para = body.AppendChild(new Paragraph());
        var run = para.AppendChild(new Run());
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        SetRunProps(run, FontSize14, bold);
        var paraProps = para.PrependChild(new ParagraphProperties());
        paraProps.AppendChild(new Justification { Val = JustificationValues.Both });
        paraProps.AppendChild(new Indentation { FirstLine = "567" });
        SetSpacing(paraProps, "0");
    }
    
    private void AddParagraphNoIndent(Body body, string text, bool bold, string fontSize)
    {
        var para = body.AppendChild(new Paragraph());
        var run = para.AppendChild(new Run());
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        SetRunProps(run, fontSize, bold);
        var paraProps = para.PrependChild(new ParagraphProperties());
        paraProps.AppendChild(new Justification { Val = JustificationValues.Both });
        SetSpacing(paraProps, "0");
    }
    
    private void AddMultiLineParagraph(Body body, string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        foreach (var line in text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) { AddEmptyLine(body); continue; }
            var para = body.AppendChild(new Paragraph());
            var run = para.AppendChild(new Run());
            run.AppendChild(new Text(line.TrimEnd()) { Space = SpaceProcessingModeValues.Preserve });
            SetRunProps(run, FontSize14, false);
            var paraProps = para.PrependChild(new ParagraphProperties());
            paraProps.AppendChild(new Justification { Val = JustificationValues.Both });
            paraProps.AppendChild(new Indentation { FirstLine = "567" });
            SetSpacing(paraProps, "0");
        }
    }
    
    private void AddEmptyLine(Body body)
    {
        var para = body.AppendChild(new Paragraph());
        para.PrependChild(new ParagraphProperties()).AppendChild(
            new SpacingBetweenLines { After = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });
    }
    
    private void AddCellParagraph(TableCell cell, string text, string fontSize, bool bold, bool center)
    {
        var para = new Paragraph();
        var run = para.AppendChild(new Run());
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        SetRunProps(run, fontSize, bold);
        var paraProps = para.PrependChild(new ParagraphProperties());
        if (center) paraProps.AppendChild(new Justification { Val = JustificationValues.Center });
        paraProps.AppendChild(new SpacingBetweenLines { After = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });
        cell.AppendChild(para);
    }
    
    private void AddFooterInfo(Body body)
    {
        // Đường kẻ phân cách
        var borderPara = body.AppendChild(new Paragraph());
        borderPara.AppendChild(new Run()).AppendChild(
            new Text("") { Space = SpaceProcessingModeValues.Preserve });
        var borderParaProps = borderPara.PrependChild(new ParagraphProperties());
        borderParaProps.AppendChild(new ParagraphBorders(
            new TopBorder { Val = BorderValues.Single, Size = 4, Color = "999999", Space = 1 }
        ));
        borderParaProps.AppendChild(new SpacingBetweenLines { After = "60", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });
        
        // Footer info
        var footerPara = body.AppendChild(new Paragraph());
        var footerRun = footerPara.AppendChild(new Run());
        footerRun.AppendChild(new Text($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}  —  Người xuất: {Environment.UserName}  —  Phần mềm: AI Văn Bản Cá Nhân")
            { Space = SpaceProcessingModeValues.Preserve });
        SetRunProps(footerRun, FontSize11, false, true);
        var footerProps = footerPara.PrependChild(new ParagraphProperties());
        footerProps.AppendChild(new Justification { Val = JustificationValues.Right });
        footerProps.AppendChild(new SpacingBetweenLines { After = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });
    }
    
    // ========================================================================
    // PRIVATE: Run & Spacing helpers
    // ========================================================================
    
    private void SetRunProps(Run run, string fontSize, bool bold, bool italic = false)
    {
        var runProps = run.PrependChild(new RunProperties());
        runProps.AppendChild(new RunFonts { Ascii = FontFamily, HighAnsi = FontFamily, EastAsia = FontFamily, ComplexScript = FontFamily });
        runProps.AppendChild(new FontSize { Val = fontSize });
        runProps.AppendChild(new FontSizeComplexScript { Val = fontSize });
        if (bold) runProps.AppendChild(new Bold());
        if (italic) runProps.AppendChild(new Italic());
    }
    
    private void SetSpacing(ParagraphProperties paraProps, string? afterSpacing = null)
    {
        paraProps.AppendChild(new SpacingBetweenLines
        {
            After = afterSpacing ?? SpacingSmall,
            Line = LineSpacing13,
            LineRule = LineSpacingRuleValues.Auto
        });
    }
}
