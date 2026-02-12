using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using AIVanBan.Core.Models;
using DocModel = AIVanBan.Core.Models.Document;
using DocType = AIVanBan.Core.Models.DocumentType;
using WordDoc = DocumentFormat.OpenXml.Wordprocessing.Document;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service xuất văn bản ra file Word (.docx) theo chuẩn Thông tư 01/2011/TT-BNV
/// Tiêu chuẩn: Times New Roman 14pt, line spacing 1.3, margins 2cm/1.5cm/2cm/1cm
/// </summary>
public class WordExportService
{
    private const string SingleLine = "240"; // 1.0
    private const string LineSpacing13 = "312"; // 1.3 lines (312/240 = 1.3)
    private const string SpacingSmall = "80"; // 4pt
    private const string SpacingMedium = "120"; // 6pt
    private const string SpacingLarge = "240"; // 12pt

    /// <summary>
    /// Xuất một văn bản ra file Word theo định dạng chuẩn hành chính nhà nước
    /// </summary>
    public void ExportDocument(DocModel document, string outputPath)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document), "Văn bản không được null");
            
        if (string.IsNullOrEmpty(outputPath))
            throw new ArgumentException("Đường dẫn file không được rỗng", nameof(outputPath));
        
        try
        {
            // Tạo file Word mới
            using var wordDoc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
            
            // Thêm main document part
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new WordDoc();
            var body = mainPart.Document.AppendChild(new Body());

            // Thiết lập margins theo chuẩn: Top 2cm, Bottom 1.5cm, Left 2cm, Right 1cm
            SetPageMargins(mainPart.Document);

            // Header - Logo và tiêu đề tổ chức (theo Thông tư 01/2011)
            AddHeader(body, document);

            // Thông tin văn bản (Số, ngày, tên loại VB, trích yếu)
            AddDocumentInfo(body, document);

            // Dòng thẩm quyền ban hành (cho QĐ, NQ, CT: VD: "CHỦ TỊCH UBND XÃ GIA KIỂM")
            if (IsDecisionType(document.Type))
            {
                AddAuthorityLine(body, document);
            }

            // Phần "Kính gửi" (CHỈ cho Công văn - không dùng cho QĐ, NQ, BC...)
            if (document.Type == DocType.CongVan)
            {
                AddSalutation(body, document);
            }

            // CĂN CỨ - Phần quan trọng trong văn bản hành chính VN
            if (document.BasedOn != null && document.BasedOn.Length > 0)
            {
                AddBasedOn(body, document);
            }

            // Nhãn loại văn bản trước nội dung (QUYẾT ĐỊNH: / NGHỊ QUYẾT:)
            if (IsDecisionType(document.Type))
            {
                AddDecisionLabel(body, document);
            }

            // Nội dung văn bản
            AddContent(body, document);

            // Footer - Chữ ký theo chuẩn
            AddSignature(body, document);

            mainPart.Document.Save();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi xuất văn bản ra Word: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Thiết lập margins theo chuẩn Thông tư 01/2011
    /// Top: 2cm (1134 twips), Bottom: 1.5cm (850 twips), Left: 2cm (1134 twips), Right: 1cm (567 twips)
    /// </summary>
    private void SetPageMargins(WordDoc document)
    {
        var body = document.Body;
        if (body == null) return;

        var sectionProps = body.GetFirstChild<SectionProperties>();
        if (sectionProps == null)
        {
            sectionProps = new SectionProperties();
            body.AppendChild(sectionProps);
        }

        var pageMargin = sectionProps.GetFirstChild<PageMargin>();
        if (pageMargin == null)
        {
            pageMargin = new PageMargin();
            sectionProps.AppendChild(pageMargin);
        }

        pageMargin.Top = 1134;      // 2cm
        pageMargin.Bottom = 850;    // 1.5cm
        pageMargin.Left = 1134;     // 2cm
        pageMargin.Right = 567;     // 1cm
        pageMargin.Header = 708;    // 1.25cm
        pageMargin.Footer = 708;    // 1.25cm
    }

    /// <summary>
    /// Phần "Kính gửi" đối với văn bản đi (theo Thông tư 01/2011)
    /// </summary>
    private void AddSalutation(Body body, DocModel document)
    {
        var salutationPara = body.AppendChild(new Paragraph());
        var salutationRun = salutationPara.AppendChild(new Run());
        salutationRun.AppendChild(new Text("Kính gửi: [Tên cơ quan nhận]"));
        
        var salutationProps = salutationRun.AppendChild(new RunProperties());
        salutationProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        salutationProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
        
        var salutationParaProps = salutationPara.AppendChild(new ParagraphProperties());
        salutationParaProps.AppendChild(new Justification() { Val = JustificationValues.Both });
        salutationParaProps.AppendChild(new Indentation() { FirstLine = "567" }); // Thụt đầu dòng 1cm
        salutationParaProps.AppendChild(new SpacingBetweenLines()
        {
            After = SpacingMedium, // 6pt spacing
            Line = LineSpacing13,
            LineRule = LineSpacingRuleValues.Auto
        });
    }

    /// <summary>
    /// Phần CĂN CỨ - Liệt kê các căn cứ pháp lý (theo chuẩn văn bản hành chính VN)
    /// Đây là phần bắt buộc trong văn bản hành chính nhà nước Việt Nam
    /// Format: Căn đầu dòng, mỗi căn cứ một dòng, font Times 14pt, không thụt đầu dòng
    /// </summary>
    private void AddBasedOn(Body body, DocModel document)
    {
        if (document.BasedOn == null || document.BasedOn.Length == 0)
            return;

        // Dòng trống trước phần căn cứ
        var spacerBefore = body.AppendChild(new Paragraph());
        var spacerBeforeProps = spacerBefore.AppendChild(new ParagraphProperties());
        spacerBeforeProps.AppendChild(new SpacingBetweenLines()
        {
            After = "0",
            Line = SingleLine,
            LineRule = LineSpacingRuleValues.Auto
        });

        // Mỗi căn cứ là một đoạn riêng
        foreach (var basedOnItem in document.BasedOn)
        {
            if (string.IsNullOrWhiteSpace(basedOnItem))
                continue;

            var para = body.AppendChild(new Paragraph());
            var run = para.AppendChild(new Run());
            
            // Đảm bảo text bắt đầu bằng "Căn cứ" (nếu chưa có)
            var text = basedOnItem.Trim();
            if (!text.StartsWith("Căn cứ", StringComparison.OrdinalIgnoreCase) &&
                !text.StartsWith("Theo", StringComparison.OrdinalIgnoreCase))
            {
                text = "Căn cứ " + text;
            }
            
            run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            
            // Font Times New Roman 14pt, IN NGHIÊNG
            var runProps = run.AppendChild(new RunProperties());
            runProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
            runProps.AppendChild(new Italic()); // IN NGHIÊNG
            runProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
            
            // Căn đều 2 bên, thụt đầu dòng 1cm (giống nội dung văn bản)
            var paraProps = para.AppendChild(new ParagraphProperties());
            paraProps.AppendChild(new Justification() { Val = JustificationValues.Both });
            paraProps.AppendChild(new Indentation() { FirstLine = "567" }); // Thụt đầu dòng 1cm
            
            // Line spacing 1.3 theo chuẩn
            paraProps.AppendChild(new SpacingBetweenLines() 
            { 
                After = "0",
                Line = LineSpacing13,
                LineRule = LineSpacingRuleValues.Auto
            });
        }

        // Dòng trống sau phần căn cứ, trước nội dung
        var spacerAfter = body.AppendChild(new Paragraph());
        var spacerAfterProps = spacerAfter.AppendChild(new ParagraphProperties());
        spacerAfterProps.AppendChild(new SpacingBetweenLines()
        {
            After = SpacingMedium, // 6pt spacing
            Line = SingleLine,
            LineRule = LineSpacingRuleValues.Auto
        });
    }

    /// <summary>
    /// Xuất nhiều văn bản vào một file Word
    /// </summary>
    public void ExportMultipleDocuments(List<DocModel> documents, string outputPath)
    {
        if (documents == null || documents.Count == 0)
            throw new ArgumentException("Danh sách văn bản không được rỗng", nameof(documents));
            
        using var wordDoc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
        var mainPart = wordDoc.AddMainDocumentPart();
        mainPart.Document = new WordDoc();
        var body = mainPart.Document.AppendChild(new Body());
        
        // Thiết lập margins chung cho toàn bộ document
        SetPageMargins(mainPart.Document);

        for (int i = 0; i < documents.Count; i++)
        {
            var doc = documents[i];
            
            // Header
            AddHeader(body, doc);
            AddDocumentInfo(body, doc);
            
            // Dòng thẩm quyền ban hành (cho QĐ, NQ, CT)
            if (IsDecisionType(doc.Type))
            {
                AddAuthorityLine(body, doc);
            }

            // Phần "Kính gửi" (CHỈ cho Công văn)
            if (doc.Type == DocType.CongVan)
            {
                AddSalutation(body, doc);
            }
            
            // CĂN CỨ - Phần quan trọng trong văn bản hành chính VN
            if (doc.BasedOn != null && doc.BasedOn.Length > 0)
            {
                AddBasedOn(body, doc);
            }

            // Nhãn loại văn bản (QUYẾT ĐỊNH: / NGHỊ QUYẾT:)
            if (IsDecisionType(doc.Type))
            {
                AddDecisionLabel(body, doc);
            }
            
            AddContent(body, doc);
            AddSignature(body, doc);

            // Thêm page break giữa các văn bản (trừ văn bản cuối)
            if (i < documents.Count - 1)
            {
                // Tạo paragraph với page break và spacing phù hợp
                var pageBreakPara = body.AppendChild(new Paragraph(
                    new ParagraphProperties(
                        new SpacingBetweenLines()
                        {
                            Before = "0",
                            After = "0",
                            Line = SingleLine,
                            LineRule = LineSpacingRuleValues.Auto
                        }
                    ),
                    new Run(
                        new Break() { Type = BreakValues.Page }
                    )
                ));
            }
        }

        mainPart.Document.Save();
    }

    /// <summary>
    /// Phần đầu văn bản theo Thông tư 01/2011: Layout 2 cột
    /// Trái: Tên cơ quan, đơn vị
    /// Phải: CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM / Độc lập - Tự do - Hạnh phúc
    /// </summary>
    private void AddHeader(Body body, DocModel document)
    {
        // Table 2 cột cho header
        var headerTable = body.AppendChild(new Table());
        var headerTableProps = headerTable.AppendChild(new TableProperties());
        headerTableProps.AppendChild(new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct });
        headerTableProps.AppendChild(new TableBorders(
            new TopBorder() { Val = BorderValues.None },
            new BottomBorder() { Val = BorderValues.None },
            new LeftBorder() { Val = BorderValues.None },
            new RightBorder() { Val = BorderValues.None },
            new InsideHorizontalBorder() { Val = BorderValues.None },
            new InsideVerticalBorder() { Val = BorderValues.None }
        ));

        // Row 1: Cơ quan cấp trên | CỘNG HÒA...
        var row1 = headerTable.AppendChild(new TableRow());
        
        // Cell trái: Tên cơ quan cấp trên (tự động tách từ Issuer)
        var leftCell1 = row1.AppendChild(new TableCell());
        var leftCellProps1 = leftCell1.AppendChild(new TableCellProperties());
        leftCellProps1.AppendChild(new TableCellWidth() { Width = "2500", Type = TableWidthUnitValues.Pct });
        
        var leftPara1 = leftCell1.AppendChild(new Paragraph());
        var leftRun1 = leftPara1.AppendChild(new Run());
        var parentOrg = ExtractParentOrg(document.Issuer);
        leftRun1.AppendChild(new Text(parentOrg));
        var leftRunProps1 = leftRun1.AppendChild(new RunProperties());
        leftRunProps1.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        leftRunProps1.AppendChild(new Bold());
        leftRunProps1.AppendChild(new FontSize() { Val = "28" });
        
        var leftParaProps1 = leftPara1.AppendChild(new ParagraphProperties());
        leftParaProps1.AppendChild(new Justification() { Val = JustificationValues.Center });
        leftParaProps1.AppendChild(new SpacingBetweenLines() { After = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });

        // Cell phải: CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
        var rightCell1 = row1.AppendChild(new TableCell());
        var rightCellProps1 = rightCell1.AppendChild(new TableCellProperties());
        rightCellProps1.AppendChild(new TableCellWidth() { Width = "2500", Type = TableWidthUnitValues.Pct });
        
        var rightPara1 = rightCell1.AppendChild(new Paragraph());
        var rightRun1 = rightPara1.AppendChild(new Run());
        rightRun1.AppendChild(new Text("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT\u00A0NAM")); // \u00A0 = non-breaking space
        var rightRunProps1 = rightRun1.AppendChild(new RunProperties());
        rightRunProps1.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        rightRunProps1.AppendChild(new Bold());
        rightRunProps1.AppendChild(new FontSize() { Val = "28" });
        
        var rightParaProps1 = rightPara1.AppendChild(new ParagraphProperties());
        rightParaProps1.AppendChild(new Justification() { Val = JustificationValues.Center });
        rightParaProps1.AppendChild(new SpacingBetweenLines() { After = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });

        // Row 2: Tên đơn vị | Độc lập - Tự do - Hạnh phúc
        var row2 = headerTable.AppendChild(new TableRow());
        
        // Cell trái: TÊN ĐƠN VỊ (gạch chân)
        var leftCell2 = row2.AppendChild(new TableCell());
        var leftCellProps2 = leftCell2.AppendChild(new TableCellProperties());
        leftCellProps2.AppendChild(new TableCellWidth() { Width = "2500", Type = TableWidthUnitValues.Pct });
        
        var leftPara2 = leftCell2.AppendChild(new Paragraph());
        var leftRun2 = leftPara2.AppendChild(new Run());
        var subOrg = ExtractSubOrg(document.Issuer);
        leftRun2.AppendChild(new Text(subOrg));
        var leftRunProps2 = leftRun2.AppendChild(new RunProperties());
        leftRunProps2.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        leftRunProps2.AppendChild(new Bold());
        leftRunProps2.AppendChild(new Underline() { Val = UnderlineValues.Single });
        leftRunProps2.AppendChild(new FontSize() { Val = "28" });
        
        var leftParaProps2 = leftPara2.AppendChild(new ParagraphProperties());
        leftParaProps2.AppendChild(new Justification() { Val = JustificationValues.Center });
        leftParaProps2.AppendChild(new SpacingBetweenLines() { After = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });

        // Cell phải: Độc lập - Tự do - Hạnh phúc
        var rightCell2 = row2.AppendChild(new TableCell());
        var rightCellProps2 = rightCell2.AppendChild(new TableCellProperties());
        rightCellProps2.AppendChild(new TableCellWidth() { Width = "2500", Type = TableWidthUnitValues.Pct });
        
        var rightPara2 = rightCell2.AppendChild(new Paragraph());
        var rightRun2 = rightPara2.AppendChild(new Run());
        rightRun2.AppendChild(new Text("Độc lập - Tự do - Hạnh phúc"));
        var rightRunProps2 = rightRun2.AppendChild(new RunProperties());
        rightRunProps2.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        rightRunProps2.AppendChild(new Bold());
        rightRunProps2.AppendChild(new FontSize() { Val = "28" });
        
        var rightParaProps2 = rightPara2.AppendChild(new ParagraphProperties());
        rightParaProps2.AppendChild(new Justification() { Val = JustificationValues.Center });
        rightParaProps2.AppendChild(new SpacingBetweenLines() { After = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });

        // Row 3: Khoảng trống | Gạch ngang
        var row3 = headerTable.AppendChild(new TableRow());
        
        var leftCell3 = row3.AppendChild(new TableCell());
        var leftCellProps3 = leftCell3.AppendChild(new TableCellProperties());
        leftCellProps3.AppendChild(new TableCellWidth() { Width = "2500", Type = TableWidthUnitValues.Pct });
        leftCell3.AppendChild(new Paragraph()); // Empty
        
        var rightCell3 = row3.AppendChild(new TableCell());
        var rightCellProps3 = rightCell3.AppendChild(new TableCellProperties());
        rightCellProps3.AppendChild(new TableCellWidth() { Width = "2500", Type = TableWidthUnitValues.Pct });
        
        var rightPara3 = rightCell3.AppendChild(new Paragraph());
        var rightRun3 = rightPara3.AppendChild(new Run());
        rightRun3.AppendChild(new Text("───────────────"));
        var rightRunProps3 = rightRun3.AppendChild(new RunProperties());
        rightRunProps3.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        rightRunProps3.AppendChild(new FontSize() { Val = "28" });
        
        var rightParaProps3 = rightPara3.AppendChild(new ParagraphProperties());
        rightParaProps3.AppendChild(new Justification() { Val = JustificationValues.Center });
        rightParaProps3.AppendChild(new SpacingBetweenLines() { After = SpacingLarge, Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });
    }

    /// <summary>
    /// Phần thông tin văn bản: Số, ngày, tiêu đề theo Thông tư 01/2011
    /// </summary>
    private void AddDocumentInfo(Body body, DocModel document)
    {
        // Số văn bản và Ngày tháng (2 cột) - Font thường 13pt
        var infoPara = body.AppendChild(new Paragraph());
        var infoProps = infoPara.AppendChild(new ParagraphProperties());
        infoProps.AppendChild(new Tabs(
            new TabStop
            {
                Val = TabStopValues.Right,
                Position = 9000 // Tab stop bên phải
            }
        ));
        infoProps.AppendChild(new SpacingBetweenLines()
        {
            After = SpacingLarge, // 12pt spacing sau số/ngày
            Line = SingleLine,
            LineRule = LineSpacingRuleValues.Auto
        });
        
        // Số văn bản (bên trái) - Font Times 13pt, IN NGHIÊNG
        var numberRun = infoPara.AppendChild(new Run());
        var numberText = !string.IsNullOrEmpty(document.Number) ? document.Number : "[Số]";
        numberRun.AppendChild(new Text($"Số: {numberText}"));
        var numberProps = numberRun.AppendChild(new RunProperties());
        numberProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        numberProps.AppendChild(new Italic()); // IN NGHIÊNG
        numberProps.AppendChild(new FontSize() { Val = "26" }); // 13pt
        
        numberRun.AppendChild(new TabChar());
        
        // Ngày tháng (bên phải) - in nghiêng 13pt
        var dateRun = infoPara.AppendChild(new Run());
        dateRun.AppendChild(new Text($"Ngày {document.IssueDate:dd} tháng {document.IssueDate:MM} năm {document.IssueDate:yyyy}"));
        
        var dateProps = dateRun.AppendChild(new RunProperties());
        dateProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        dateProps.AppendChild(new Italic());
        dateProps.AppendChild(new FontSize() { Val = "26" }); // 13pt

        // Tên LOẠI VĂN BẢN (căn giữa, in hoa, đậm, 16pt) - VD: QUYẾT ĐỊNH, CÔNG VĂN, BÁO CÁO
        var titlePara = body.AppendChild(new Paragraph());
        var titleRun = titlePara.AppendChild(new Run());
        var docTypeName = GetDocumentTypeName(document.Type);
        titleRun.AppendChild(new Text(docTypeName));
        
        var titleProps = titleRun.AppendChild(new RunProperties());
        titleProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        titleProps.AppendChild(new Bold());
        titleProps.AppendChild(new FontSize() { Val = "32" }); // 16pt
        
        var titleParaProps = titlePara.AppendChild(new ParagraphProperties());
        titleParaProps.AppendChild(new Justification() { Val = JustificationValues.Center });
        titleParaProps.AppendChild(new SpacingBetweenLines()
        {
            After = "0", // Không cách, trích yếu theo ngay dưới
            Line = SingleLine,
            LineRule = LineSpacingRuleValues.Auto
        });

        // Trích yếu (in nghiêng, căn giữa, 14pt) - luôn hiển thị
        var subjectPara = body.AppendChild(new Paragraph());
        var subjectRun = subjectPara.AppendChild(new Run());
        var subjectText = !string.IsNullOrEmpty(document.Subject) 
            ? document.Subject 
            : (!string.IsNullOrEmpty(document.Title) ? document.Title : "");
        subjectRun.AppendChild(new Text(subjectText));
        
        var subjectProps = subjectRun.AppendChild(new RunProperties());
        subjectProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        subjectProps.AppendChild(new Italic());
        subjectProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
        
        var subjectParaProps = subjectPara.AppendChild(new ParagraphProperties());
        subjectParaProps.AppendChild(new Justification() { Val = JustificationValues.Center });
        subjectParaProps.AppendChild(new SpacingBetweenLines()
        {
            After = SpacingLarge, // 12pt
            Line = SingleLine,
            LineRule = LineSpacingRuleValues.Auto
        });
    }

    /// <summary>
    /// Nội dung văn bản: Font Times 14pt, line spacing 1.3, căn đều 2 bên
    /// </summary>
    private void AddContent(Body body, DocModel document)
    {
        // Nội dung văn bản - chia thành các đoạn
        var contentText = !string.IsNullOrEmpty(document.Content) 
            ? document.Content 
            : "[Nội dung văn bản]";
        var lines = contentText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        
        foreach (var line in lines)
        {
            var para = body.AppendChild(new Paragraph());
            
            if (string.IsNullOrWhiteSpace(line))
            {
                // Đoạn trống - giữ line spacing 1.3
                var emptyProps = para.AppendChild(new ParagraphProperties());
                emptyProps.AppendChild(new SpacingBetweenLines()
                {
                    After = "0",
                    Line = LineSpacing13,
                    LineRule = LineSpacingRuleValues.Auto
                });
                continue;
            }

            var trimmedLine = line.Trim();
            
            // Phát hiện loại dòng để format phù hợp
            var lineType = DetectLineType(trimmedLine);
            
            var run = para.AppendChild(new Run());
            run.AppendChild(new Text(trimmedLine) { Space = SpaceProcessingModeValues.Preserve });
            
            // Font Times New Roman
            var runProps = run.AppendChild(new RunProperties());
            runProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
            
            // ParagraphProperties
            var paraProps = para.AppendChild(new ParagraphProperties());
            
            switch (lineType)
            {
                case ContentLineType.ChuongPhan: // Chương I, Phần thứ nhất...
                    runProps.AppendChild(new Bold());
                    runProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
                    paraProps.AppendChild(new Justification() { Val = JustificationValues.Center });
                    paraProps.AppendChild(new SpacingBetweenLines()
                    {
                        Before = SpacingLarge, After = SpacingSmall,
                        Line = LineSpacing13, LineRule = LineSpacingRuleValues.Auto
                    });
                    break;
                    
                case ContentLineType.Dieu: // Điều 1, Điều 2...
                    runProps.AppendChild(new Bold());
                    runProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
                    paraProps.AppendChild(new Justification() { Val = JustificationValues.Both });
                    paraProps.AppendChild(new Indentation() { FirstLine = "567" });
                    paraProps.AppendChild(new SpacingBetweenLines()
                    {
                        Before = SpacingMedium, After = "0",
                        Line = LineSpacing13, LineRule = LineSpacingRuleValues.Auto
                    });
                    break;
                    
                case ContentLineType.Khoan: // 1. ..., 2. ...
                    runProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
                    paraProps.AppendChild(new Justification() { Val = JustificationValues.Both });
                    paraProps.AppendChild(new Indentation() { FirstLine = "567" });
                    paraProps.AppendChild(new SpacingBetweenLines()
                    {
                        Before = SpacingSmall, After = "0",
                        Line = LineSpacing13, LineRule = LineSpacingRuleValues.Auto
                    });
                    break;
                    
                case ContentLineType.Diem: // a) ..., b) ...
                    runProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
                    paraProps.AppendChild(new Justification() { Val = JustificationValues.Both });
                    paraProps.AppendChild(new Indentation() { FirstLine = "851" }); // Thụt sâu hơn 1.5cm
                    paraProps.AppendChild(new SpacingBetweenLines()
                    {
                        After = "0",
                        Line = LineSpacing13, LineRule = LineSpacingRuleValues.Auto
                    });
                    break;
                    
                default: // Nội dung thường
                    runProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
                    paraProps.AppendChild(new Justification() { Val = JustificationValues.Both });
                    paraProps.AppendChild(new Indentation() { FirstLine = "567" });
                    paraProps.AppendChild(new SpacingBetweenLines()
                    {
                        After = "0",
                        Line = LineSpacing13, LineRule = LineSpacingRuleValues.Auto
                    });
                    break;
            }
        }

        // Khoảng cách trước chữ ký
        var spacer = body.AppendChild(new Paragraph());
        var spacerProps = spacer.AppendChild(new ParagraphProperties());
        spacerProps.AppendChild(new SpacingBetweenLines()
        {
            After = SpacingLarge,
            Line = LineSpacing13,
            LineRule = LineSpacingRuleValues.Auto
        });
    }

    /// <summary>
    /// Phân loại dòng nội dung để format phù hợp trong Word
    /// </summary>
    private enum ContentLineType { Normal, ChuongPhan, Dieu, Khoan, Diem }
    
    private ContentLineType DetectLineType(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return ContentLineType.Normal;
        
        var trimmed = line.TrimStart();
        
        // Chương I, CHƯƠNG II, Phần thứ nhất, PHẦN THỨ HAI, Mục 1, MỤC 2
        if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^(Chương|CHƯƠNG)\s+[IVXLCDM\d]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return ContentLineType.ChuongPhan;
        if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^(Phần|PHẦN)\s+(thứ\s+)?[IVXLCDM\d]", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return ContentLineType.ChuongPhan;
        if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^(Mục|MỤC)\s+\d+", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return ContentLineType.ChuongPhan;
        // QUY ĐỊNH CHUNG, QUY ĐỊNH CỤ THỂ (tiêu đề chương)
        if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^[A-ZÀÁẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬĐÈÉẺẼẸÊẾỀỂỄỆÌÍỈĨỊÒÓỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢÙÚỦŨỤƯỨỪỬỮỰỲÝỶỸỴ\s]+$") && trimmed.Length <= 80)
            return ContentLineType.ChuongPhan;

        // Điều 1. ..., Điều 12: ...
        if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^Điều\s+\d+"))
            return ContentLineType.Dieu;

        // Khoản: 1. ..., 2. ..., 10. ... (số + dấu chấm + khoảng trắng + chữ cái)
        if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^\d+\.\s+[A-ZÀÁẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬĐÈÉẺẼẸÊẾỀỂỄỆÌÍỈĨỊÒÓỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢÙÚỦŨỤƯỨỪỬỮỰỲÝỶỸỴa-zàáảãạăắằẳẵặâấầẩẫậđèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵ]"))
            return ContentLineType.Khoan;

        // Điểm: a) ..., b) ..., đ) ...
        if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^[a-zđ]\)\s"))
            return ContentLineType.Diem;

        return ContentLineType.Normal;
    }

    /// <summary>
    /// Phần chữ ký theo Thông tư 01/2011: Layout 2 cột với Nơi nhận bên trái và Chữ ký bên phải
    /// Có địa điểm, ngày tháng trước phần ký (theo chuẩn văn bản hành chính)
    /// </summary>
    private void AddSignature(Body body, DocModel document)
    {
        // Table 2 cột cho layout Nơi nhận (trái) và Chữ ký (phải)
        var table = body.AppendChild(new Table());
        
        // Table properties: No borders, full width
        var tableProps = table.AppendChild(new TableProperties());
        tableProps.AppendChild(new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct }); // 100% width
        tableProps.AppendChild(new TableBorders(
            new TopBorder() { Val = BorderValues.None },
            new BottomBorder() { Val = BorderValues.None },
            new LeftBorder() { Val = BorderValues.None },
            new RightBorder() { Val = BorderValues.None },
            new InsideHorizontalBorder() { Val = BorderValues.None },
            new InsideVerticalBorder() { Val = BorderValues.None }
        ));

        var tr = table.AppendChild(new TableRow());

        // Cột trái: Nơi nhận (hiện khi có danh sách nơi nhận hoặc là văn bản đi/QĐ/NQ)
        var leftCell = tr.AppendChild(new TableCell());
        var leftCellProps = leftCell.AppendChild(new TableCellProperties());
        leftCellProps.AppendChild(new TableCellWidth() { Width = "2500", Type = TableWidthUnitValues.Pct }); // 50%
        leftCellProps.AppendChild(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Top });

        var hasRecipients = document.Recipients != null && document.Recipients.Length > 0;
        if (hasRecipients || document.Direction == Direction.Di || IsDecisionType(document.Type))
        {
            // "Nơi nhận:"
            var receiverPara = leftCell.AppendChild(new Paragraph());
            var receiverRun = receiverPara.AppendChild(new Run());
            receiverRun.AppendChild(new Text("Nơi nhận:"));
            
            var receiverProps = receiverRun.AppendChild(new RunProperties());
            receiverProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
            receiverProps.AppendChild(new Bold());
            receiverProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
            
            var receiverParaProps = receiverPara.AppendChild(new ParagraphProperties());
            receiverParaProps.AppendChild(new SpacingBetweenLines()
            {
                After = SpacingSmall,
                Line = LineSpacing13,
                LineRule = LineSpacingRuleValues.Auto
            });

            // Danh sách nơi nhận (từ document.Recipients)
            var receiverListPara = leftCell.AppendChild(new Paragraph());
            var receiverListRun = receiverListPara.AppendChild(new Run());
            
            if (document.Recipients != null && document.Recipients.Length > 0)
            {
                // Sử dụng danh sách từ document
                for (int i = 0; i < document.Recipients.Length; i++)
                {
                    if (i > 0)
                    {
                        receiverListRun.AppendChild(new Break());
                    }
                    receiverListRun.AppendChild(new Text(document.Recipients[i]));
                }
            }
            else
            {
                // Mặc định nếu không có
                receiverListRun.AppendChild(new Text("- Như trên;"));
                receiverListRun.AppendChild(new Break());
                receiverListRun.AppendChild(new Text("- Lưu: VT."));
            }
            
            var listRunProps = receiverListRun.AppendChild(new RunProperties());
            listRunProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
            listRunProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
        }

        // Cột phải: Địa điểm, ngày + Chữ ký
        var rightCell = tr.AppendChild(new TableCell());
        var rightCellProps = rightCell.AppendChild(new TableCellProperties());
        rightCellProps.AppendChild(new TableCellWidth() { Width = "2500", Type = TableWidthUnitValues.Pct }); // 50%
        rightCellProps.AppendChild(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Top });

        // Địa điểm, ngày tháng (in nghiêng, căn phải) - dùng Location từ document
        var locationPara = rightCell.AppendChild(new Paragraph());
        var locationRun = locationPara.AppendChild(new Run());
        var locationName = !string.IsNullOrEmpty(document.Location) ? document.Location : "...";
        locationRun.AppendChild(new Text($"{locationName}, ngày {document.IssueDate:dd} tháng {document.IssueDate:MM} năm {document.IssueDate:yyyy}"));
        
        var locationProps = locationRun.AppendChild(new RunProperties());
        locationProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        locationProps.AppendChild(new Italic());
        locationProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
        
        var locationParaProps = locationPara.AppendChild(new ParagraphProperties());
        locationParaProps.AppendChild(new Justification() { Val = JustificationValues.Center });
        locationParaProps.AppendChild(new SpacingBetweenLines()
        {
            After = SpacingMedium,
            Line = LineSpacing13,
            LineRule = LineSpacingRuleValues.Auto
        });

        // Thẩm quyền ký (TM., KT., Q. - chỉ hiện nếu có)
        if (!string.IsNullOrEmpty(document.SigningAuthority))
        {
            var tmPara = rightCell.AppendChild(new Paragraph());
            var tmRun = tmPara.AppendChild(new Run());
            tmRun.AppendChild(new Text(document.SigningAuthority.ToUpper()));
            
            var tmRunProps = tmRun.AppendChild(new RunProperties());
            tmRunProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
            tmRunProps.AppendChild(new Bold());
            tmRunProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
            
            var tmParaProps = tmPara.AppendChild(new ParagraphProperties());
            tmParaProps.AppendChild(new Justification() { Val = JustificationValues.Center });
            tmParaProps.AppendChild(new SpacingBetweenLines()
            {
                After = "0",
                Line = LineSpacing13,
                LineRule = LineSpacingRuleValues.Auto
            });
        }

        // Chức danh ký (CHỦ TỊCH, GIÁM ĐỐC, TRƯỞNG PHÒNG...)
        var signingTitle = !string.IsNullOrEmpty(document.SigningTitle) ? document.SigningTitle.ToUpper() : "[CHỨC DANH]";
        var titlePara = rightCell.AppendChild(new Paragraph());
        var titleRun = titlePara.AppendChild(new Run());
        titleRun.AppendChild(new Text(signingTitle));
        
        var titleRunProps = titleRun.AppendChild(new RunProperties());
        titleRunProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        titleRunProps.AppendChild(new Bold());
        titleRunProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
        
        var titleParaProps = titlePara.AppendChild(new ParagraphProperties());
        titleParaProps.AppendChild(new Justification() { Val = JustificationValues.Center });
        titleParaProps.AppendChild(new SpacingBetweenLines()
        {
            After = "0",
            Line = LineSpacing13,
            LineRule = LineSpacingRuleValues.Auto
        });

        // "(Ký, ghi rõ họ tên và đóng dấu)" (in nghiêng, căn phải)
        var signNotePara = rightCell.AppendChild(new Paragraph());
        var signNoteRun = signNotePara.AppendChild(new Run());
        signNoteRun.AppendChild(new Text("(Ký, ghi rõ họ tên và đóng dấu)"));
        
        var noteRunProps = signNoteRun.AppendChild(new RunProperties());
        noteRunProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        noteRunProps.AppendChild(new Italic());
        noteRunProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
        
        var noteParaProps = signNotePara.AppendChild(new ParagraphProperties());
        noteParaProps.AppendChild(new Justification() { Val = JustificationValues.Center });
        noteParaProps.AppendChild(new SpacingBetweenLines()
        {
            After = SpacingMedium,
            Line = LineSpacing13,
            LineRule = LineSpacingRuleValues.Auto
        });

        // Khoảng trống cho chữ ký (3 dòng, trong rightCell)
        for (int i = 0; i < 3; i++)
        {
            var emptyPara = rightCell.AppendChild(new Paragraph());
            var emptyProps = emptyPara.AppendChild(new ParagraphProperties());
            emptyProps.AppendChild(new Justification() { Val = JustificationValues.Center });
            emptyProps.AppendChild(new SpacingBetweenLines()
            {
                Line = LineSpacing13,
                LineRule = LineSpacingRuleValues.Auto
            });
        }

        // Họ tên người ký (in đậm, căn phải, KHÔNG in hoa)
        var namePara = rightCell.AppendChild(new Paragraph());
        var nameRun = namePara.AppendChild(new Run());
        nameRun.AppendChild(new Text(!string.IsNullOrEmpty(document.SignedBy)
            ? document.SignedBy
            : "[Họ tên người ký]"));
        
        var nameRunProps = nameRun.AppendChild(new RunProperties());
        nameRunProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        nameRunProps.AppendChild(new Bold());
        nameRunProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
        
        var nameParaProps = namePara.AppendChild(new ParagraphProperties());
        nameParaProps.AppendChild(new Justification() { Val = JustificationValues.Center });
    }

    #region Helper Methods

    /// <summary>
    /// Kiểm tra loại VB có phải dạng quyết định/nghị quyết/chỉ thị (có phần "căn cứ" + "QUYẾT ĐỊNH:")
    /// </summary>
    private bool IsDecisionType(DocType type) => type switch
    {
        DocType.QuyetDinh => true,
        DocType.NghiQuyet => true,
        DocType.ChiThi => true,
        DocType.NghiDinh => true,
        DocType.Luat => true,
        _ => false
    };

    /// <summary>
    /// Lấy tên loại văn bản hiển thị in hoa
    /// </summary>
    private string GetDocumentTypeName(DocType type) => type switch
    {
        DocType.QuyetDinh => "QUYẾT ĐỊNH",
        DocType.CongVan => "CÔNG VĂN",
        DocType.BaoCao => "BÁO CÁO",
        DocType.ToTrinh => "TỞ TRÌNH",
        DocType.KeHoach => "KẾ HOẠCH",
        DocType.ThongBao => "THÔNG BÁO",
        DocType.NghiQuyet => "NGHỊ QUYẾT",
        DocType.ChiThi => "CHỈ THỊ",
        DocType.HuongDan => "HƯỚNG DẪN",
        DocType.Luat => "LUẬT",
        DocType.NghiDinh => "NGHỊ ĐỊNH",
        DocType.ThongTu => "THÔNG TƯ",
        DocType.QuyDinh => "QUY ĐỊNH",
        _ => "VĂN BẢN"
    };

    /// <summary>
    /// Lấy nhãn quyết định trước phần Điều (VD: "QUYẾT ĐỊNH:", "NGHỊ QUYẾT:")
    /// </summary>
    private string GetDecisionLabel(DocType type) => type switch
    {
        DocType.QuyetDinh => "QUYẾT ĐỊNH:",
        DocType.NghiQuyet => "NGHỊ QUYẾT:",
        DocType.ChiThi => "CHỈ THỊ:",
        DocType.NghiDinh => "NGHỊ ĐỊNH:",
        DocType.Luat => "LUẬT:",
        _ => ""
    };

    /// <summary>
    /// Tách tên cơ quan cấp trên từ Issuer
    /// VD: "ỦY BAN NHÂN DÂN XÃ GIA KIỂM" → "ỦY BAN NHÂN DÂN"
    ///     "SỞ GIÁO DỤC VÀ ĐÀO TẠO TỈNH ĐỒNG NAI" → "SỞ GIÁO DỤC VÀ ĐÀO TẠO"
    ///     "UBND HUYỆN THỐNG NHẤT" → "UBND"
    /// </summary>
    private string ExtractParentOrg(string issuer)
    {
        if (string.IsNullOrEmpty(issuer)) return "[CƠ QUAN CẤP TRÊN]";
        
        var upper = issuer.ToUpper().Trim();
        
        // Các pattern phổ biến: tách tên tổ chức khỏi tên địa phương
        // "ỦY BAN NHÂN DÂN XÃ/HUYỆN/TỈNH/TP..." → "ỦY BAN NHÂN DÂN"
        var locationPrefixes = new[] {
            " XÃ ", " HUYỆN ", " TỈNH ", " THÀNH PHỐ ", " TP. ", " TP ", " THỊ XÃ ", " THỊ TRẤN ",
            " PHƯỜNG ", " QUẬN ", " THÀNH PHỐ "
        };
        
        foreach (var prefix in locationPrefixes)
        {
            var idx = upper.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
            {
                return upper.Substring(0, idx).Trim();
            }
        }
        
        // Fallback: không tách được thì trả về nguyên bản
        return upper;
    }

    /// <summary>
    /// Tách tên đơn vị con (phần sau cơ quan cấp trên, gạch chân)
    /// VD: "ỦY BAN NHÂN DÂN XÃ GIA KIỂM" → "XÃ GIA KIỂM"
    ///     "SỞ GIÁO DỤC VÀ ĐÀO TẠO TỈNH ĐỒNG NAI" → "TỈNH ĐỒNG NAI"
    /// </summary>
    private string ExtractSubOrg(string issuer)
    {
        if (string.IsNullOrEmpty(issuer)) return "[TÊN ĐƠN VỊ]";
        
        var upper = issuer.ToUpper().Trim();
        
        var locationPrefixes = new[] {
            " XÃ ", " HUYỆN ", " TỈNH ", " THÀNH PHỐ ", " TP. ", " TP ", " THỊ XÃ ", " THỊ TRẤN ",
            " PHƯỜNG ", " QUẬN "
        };
        
        foreach (var prefix in locationPrefixes)
        {
            var idx = upper.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
            {
                return upper.Substring(idx).Trim();
            }
        }
        
        // Fallback: trả về toàn bộ (gạch chân)
        return upper;
    }

    /// <summary>
    /// Dòng thẩm quyền ban hành - xuất hiện sau tiêu đề, trước căn cứ
    /// VD: "CHỦ TỊCH ỦY BAN NHÂN DÂN XÃ GIA KIỂM"
    ///     "GIÁM ĐỐC SỞ GIÁO DỤC VÀ ĐÀO TẠO"
    /// </summary>
    private void AddAuthorityLine(Body body, DocModel document)
    {
        // Tạo dòng: [ChứcDanh] [CơQuanBanHành]
        var authorityText = "";
        
        if (!string.IsNullOrEmpty(document.SigningTitle) && !string.IsNullOrEmpty(document.Issuer))
        {
            authorityText = $"{document.SigningTitle.ToUpper()} {document.Issuer.ToUpper()}";
        }
        else if (!string.IsNullOrEmpty(document.SigningTitle))
        {
            authorityText = document.SigningTitle.ToUpper();
        }
        else if (!string.IsNullOrEmpty(document.Issuer))
        {
            authorityText = document.Issuer.ToUpper();
        }
        
        if (string.IsNullOrEmpty(authorityText)) return;

        var para = body.AppendChild(new Paragraph());
        var run = para.AppendChild(new Run());
        run.AppendChild(new Text(authorityText));
        
        var runProps = run.AppendChild(new RunProperties());
        runProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        runProps.AppendChild(new Bold());
        runProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
        
        var paraProps = para.AppendChild(new ParagraphProperties());
        paraProps.AppendChild(new Justification() { Val = JustificationValues.Center });
        paraProps.AppendChild(new SpacingBetweenLines()
        {
            After = SpacingLarge,
            Line = SingleLine,
            LineRule = LineSpacingRuleValues.Auto
        });
    }

    /// <summary>
    /// Nhãn loại văn bản trước phần nội dung Điều
    /// VD: "QUYẾT ĐỊNH:" in đậm, căn giữa
    /// </summary>
    private void AddDecisionLabel(Body body, DocModel document)
    {
        var label = GetDecisionLabel(document.Type);
        if (string.IsNullOrEmpty(label)) return;

        var para = body.AppendChild(new Paragraph());
        var run = para.AppendChild(new Run());
        run.AppendChild(new Text(label));
        
        var runProps = run.AppendChild(new RunProperties());
        runProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        runProps.AppendChild(new Bold());
        runProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
        
        var paraProps = para.AppendChild(new ParagraphProperties());
        paraProps.AppendChild(new Justification() { Val = JustificationValues.Center });
        paraProps.AppendChild(new SpacingBetweenLines()
        {
            After = SpacingMedium,
            Line = LineSpacing13,
            LineRule = LineSpacingRuleValues.Auto
        });
    }

    #endregion
}
