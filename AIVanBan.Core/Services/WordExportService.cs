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
///
/// OpenXML ordering rules (bắt buộc để formatting hiển thị đúng):
/// - RunProperties PHẢI là child ĐẦU TIÊN của Run (trước Text)
/// - ParagraphProperties PHẢI là child ĐẦU TIÊN của Paragraph (trước Run)
/// - SectionProperties PHẢI là child CUỐI CÙNG của Body
/// - RunFonts cần đủ 4 slot: Ascii, HighAnsi, EastAsia, ComplexScript
/// </summary>
public class WordExportService
{
    private const string SingleLine = "240"; // 1.0
    private const string LineSpacing13 = "312"; // 1.3 lines (312/240 = 1.3)
    private const string SpacingSmall = "80"; // 4pt
    private const string SpacingMedium = "120"; // 6pt
    private const string SpacingLarge = "240"; // 12pt

    #region OpenXML Helper — Tạo Run đúng thứ tự (RunProperties trước Text)

    /// <summary>
    /// Tạo Run với RunProperties ĐẶT TRƯỚC Text (bắt buộc theo OpenXML spec).
    /// Font: Times New Roman đủ 4 slot (Ascii + HighAnsi + EastAsia + ComplexScript).
    /// FontSize kèm FontSizeComplexScript để đảm bảo cỡ chữ đúng cho mọi ngôn ngữ.
    /// </summary>
    private static Run CreateStyledRun(string text, bool bold = false, bool italic = false,
        string fontSize = "28", bool underline = false)
    {
        var run = new Run();
        var rp = new RunProperties();
        rp.AppendChild(new RunFonts()
        {
            Ascii = "Times New Roman",
            HighAnsi = "Times New Roman",
            EastAsia = "Times New Roman",
            ComplexScript = "Times New Roman"
        });
        if (bold) rp.AppendChild(new Bold());
        if (italic) rp.AppendChild(new Italic());
        rp.AppendChild(new FontSize() { Val = fontSize });
        rp.AppendChild(new FontSizeComplexScript() { Val = fontSize });
        if (underline) rp.AppendChild(new Underline() { Val = UnderlineValues.Single });
        run.AppendChild(rp); // RunProperties FIRST
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        return run;
    }

    #endregion

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

            // ═══ TIỀN XỬ LÝ: Tách căn cứ + làm sạch nội dung trước khi xuất ═══
            // Content từ AI hoặc seed data có thể chứa sẵn căn cứ, nhãn QĐ, Kính gửi...
            // → Tách ra các field riêng để tránh trùng lặp khi xuất
            CleanupDocumentContent(document);

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

            // SectionProperties PHẢI là child CUỐI CÙNG của Body (OpenXML spec)
            // Theo Thông tư 01/2011: Top 2cm, Bottom 1.5cm, Left 2cm, Right 1cm
            SetPageMargins(body);

            mainPart.Document.Save();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi xuất văn bản ra Word: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Thiết lập margins theo chuẩn Thông tư 01/2011.
    /// SectionProperties được thêm/di chuyển về cuối Body (bắt buộc theo OpenXML).
    /// Top: 2cm (1134 twips), Bottom: 1.5cm (850 twips), Left: 2cm (1134 twips), Right: 1cm (567 twips)
    /// </summary>
    private void SetPageMargins(Body body)
    {
        if (body == null) return;

        // Xóa SectionProperties cũ nếu có (đảm bảo luôn nằm cuối)
        var existing = body.GetFirstChild<SectionProperties>();
        existing?.Remove();

        var sectionProps = new SectionProperties();
        sectionProps.AppendChild(new PageMargin()
        {
            Top = 1134,      // 2cm
            Bottom = 850,    // 1.5cm
            Left = 1134,     // 2cm
            Right = 567,     // 1cm
            Header = 708,    // 1.25cm
            Footer = 708     // 1.25cm
        });

        body.AppendChild(sectionProps); // MUST be last child of Body
    }

    /// <summary>
    /// Phần "Kính gửi" đối với văn bản đi (theo Thông tư 01/2011)
    /// </summary>
    private void AddSalutation(Body body, DocModel document)
    {
        var para = body.AppendChild(new Paragraph());

        // ParagraphProperties PHẢI là child ĐẦU TIÊN của Paragraph
        var paraProps = para.AppendChild(new ParagraphProperties());
        paraProps.AppendChild(new Justification() { Val = JustificationValues.Both });
        paraProps.AppendChild(new Indentation() { FirstLine = "567" }); // Thụt đầu dòng 1cm
        paraProps.AppendChild(new SpacingBetweenLines()
        {
            After = SpacingMedium, // 6pt spacing
            Line = LineSpacing13,
            LineRule = LineSpacingRuleValues.Auto
        });

        // Run (với RunProperties trước Text) — sau ParagraphProperties
        para.AppendChild(CreateStyledRun("Kính gửi: [Tên cơ quan nhận]"));
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

            // ParagraphProperties FIRST — căn đều 2 bên, thụt đầu dòng 1cm
            var paraProps = para.AppendChild(new ParagraphProperties());
            paraProps.AppendChild(new Justification() { Val = JustificationValues.Both });
            paraProps.AppendChild(new Indentation() { FirstLine = "567" }); // Thụt đầu dòng 1cm
            paraProps.AppendChild(new SpacingBetweenLines()
            {
                After = "0",
                Line = LineSpacing13,
                LineRule = LineSpacingRuleValues.Auto
            });

            // Đảm bảo text bắt đầu bằng "Căn cứ" (nếu chưa có)
            var text = basedOnItem.Trim();
            if (!text.StartsWith("Căn cứ", StringComparison.OrdinalIgnoreCase) &&
                !text.StartsWith("Theo", StringComparison.OrdinalIgnoreCase))
            {
                text = "Căn cứ " + text;
            }

            // Run AFTER ParagraphProperties — font Times New Roman 14pt, IN NGHIÊNG
            para.AppendChild(CreateStyledRun(text, italic: true));
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

        for (int i = 0; i < documents.Count; i++)
        {
            var doc = documents[i];

            // Tiền xử lý: tách căn cứ + làm sạch nội dung
            CleanupDocumentContent(doc);

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
                // Tạo paragraph với page break (constructor pattern — ordering đúng sẵn)
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

        // SectionProperties PHẢI là child CUỐI CÙNG của Body
        SetPageMargins(body);

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
        // ParagraphProperties FIRST
        var leftParaProps1 = leftPara1.AppendChild(new ParagraphProperties());
        leftParaProps1.AppendChild(new Justification() { Val = JustificationValues.Center });
        leftParaProps1.AppendChild(new SpacingBetweenLines() { After = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });
        // Run AFTER ParagraphProperties — nếu không có cơ quan cấp trên thì để trống
        var parentOrg = ExtractParentOrg(document.Issuer);
        if (!string.IsNullOrEmpty(parentOrg))
        {
            leftPara1.AppendChild(CreateStyledRun(parentOrg, bold: true));
        }

        // Cell phải: CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
        var rightCell1 = row1.AppendChild(new TableCell());
        var rightCellProps1 = rightCell1.AppendChild(new TableCellProperties());
        rightCellProps1.AppendChild(new TableCellWidth() { Width = "2500", Type = TableWidthUnitValues.Pct });

        var rightPara1 = rightCell1.AppendChild(new Paragraph());
        var rightParaProps1 = rightPara1.AppendChild(new ParagraphProperties());
        rightParaProps1.AppendChild(new Justification() { Val = JustificationValues.Center });
        rightParaProps1.AppendChild(new SpacingBetweenLines() { After = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });
        rightPara1.AppendChild(CreateStyledRun("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT\u00A0NAM", bold: true)); // \u00A0 = non-breaking space

        // Row 2: Tên đơn vị | Độc lập - Tự do - Hạnh phúc
        var row2 = headerTable.AppendChild(new TableRow());

        // Cell trái: TÊN ĐƠN VỊ (gạch chân)
        var leftCell2 = row2.AppendChild(new TableCell());
        var leftCellProps2 = leftCell2.AppendChild(new TableCellProperties());
        leftCellProps2.AppendChild(new TableCellWidth() { Width = "2500", Type = TableWidthUnitValues.Pct });

        var leftPara2 = leftCell2.AppendChild(new Paragraph());
        var leftParaProps2 = leftPara2.AppendChild(new ParagraphProperties());
        leftParaProps2.AppendChild(new Justification() { Val = JustificationValues.Center });
        leftParaProps2.AppendChild(new SpacingBetweenLines() { After = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });
        var subOrg = ExtractSubOrg(document.Issuer);
        if (!string.IsNullOrEmpty(subOrg))
        {
            leftPara2.AppendChild(CreateStyledRun(subOrg, bold: true, underline: true));
        }

        // Cell phải: Độc lập - Tự do - Hạnh phúc
        var rightCell2 = row2.AppendChild(new TableCell());
        var rightCellProps2 = rightCell2.AppendChild(new TableCellProperties());
        rightCellProps2.AppendChild(new TableCellWidth() { Width = "2500", Type = TableWidthUnitValues.Pct });

        var rightPara2 = rightCell2.AppendChild(new Paragraph());
        var rightParaProps2 = rightPara2.AppendChild(new ParagraphProperties());
        rightParaProps2.AppendChild(new Justification() { Val = JustificationValues.Center });
        rightParaProps2.AppendChild(new SpacingBetweenLines() { After = "0", Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });
        rightPara2.AppendChild(CreateStyledRun("Độc lập - Tự do - Hạnh phúc", bold: true));

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
        var rightParaProps3 = rightPara3.AppendChild(new ParagraphProperties());
        rightParaProps3.AppendChild(new Justification() { Val = JustificationValues.Center });
        rightParaProps3.AppendChild(new SpacingBetweenLines() { After = SpacingLarge, Line = SingleLine, LineRule = LineSpacingRuleValues.Auto });
        rightPara3.AppendChild(CreateStyledRun("───────────────"));
    }

    /// <summary>
    /// Phần thông tin văn bản: Số, ngày, tiêu đề theo Thông tư 01/2011
    /// </summary>
    private void AddDocumentInfo(Body body, DocModel document)
    {
        // Số văn bản và Ngày tháng (2 cột) - Font thường 13pt
        var infoPara = body.AppendChild(new Paragraph());
        // ParagraphProperties FIRST
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

        // Số văn bản (bên trái) - Font Times 13pt, IN NGHIÊNG + TabChar
        var numberText = !string.IsNullOrEmpty(document.Number) ? document.Number : "[Số]";
        var numberRun = new Run();
        // RunProperties FIRST
        var numberRunProps = numberRun.AppendChild(new RunProperties());
        numberRunProps.AppendChild(new RunFonts()
        {
            Ascii = "Times New Roman",
            HighAnsi = "Times New Roman",
            EastAsia = "Times New Roman",
            ComplexScript = "Times New Roman"
        });
        numberRunProps.AppendChild(new Italic());
        numberRunProps.AppendChild(new FontSize() { Val = "26" }); // 13pt
        numberRunProps.AppendChild(new FontSizeComplexScript() { Val = "26" });
        // Text + TabChar AFTER RunProperties
        numberRun.AppendChild(new Text($"Số: {numberText}") { Space = SpaceProcessingModeValues.Preserve });
        numberRun.AppendChild(new TabChar());
        infoPara.AppendChild(numberRun);

        // Ngày tháng (bên phải) - in nghiêng 13pt
        infoPara.AppendChild(CreateStyledRun(
            $"Ngày {document.IssueDate:dd} tháng {document.IssueDate:MM} năm {document.IssueDate:yyyy}",
            italic: true, fontSize: "26"));

        // Tên LOẠI VĂN BẢN (căn giữa, in hoa, đậm, 16pt) - VD: QUYẾT ĐỊNH, CÔNG VĂN, BÁO CÁO
        var titlePara = body.AppendChild(new Paragraph());
        // ParagraphProperties FIRST
        var titleParaProps = titlePara.AppendChild(new ParagraphProperties());
        titleParaProps.AppendChild(new Justification() { Val = JustificationValues.Center });
        titleParaProps.AppendChild(new SpacingBetweenLines()
        {
            After = "0", // Không cách, trích yếu theo ngay dưới
            Line = SingleLine,
            LineRule = LineSpacingRuleValues.Auto
        });
        // Run AFTER ParagraphProperties
        var docTypeName = GetDocumentTypeName(document.Type);
        titlePara.AppendChild(CreateStyledRun(docTypeName, bold: true, fontSize: "32")); // 16pt

        // Trích yếu (in nghiêng, căn giữa, 14pt) - luôn hiển thị
        var subjectPara = body.AppendChild(new Paragraph());
        var subjectParaProps = subjectPara.AppendChild(new ParagraphProperties());
        subjectParaProps.AppendChild(new Justification() { Val = JustificationValues.Center });
        subjectParaProps.AppendChild(new SpacingBetweenLines()
        {
            After = SpacingLarge, // 12pt
            Line = SingleLine,
            LineRule = LineSpacingRuleValues.Auto
        });
        var subjectText = !string.IsNullOrEmpty(document.Subject)
            ? document.Subject
            : (!string.IsNullOrEmpty(document.Title) ? document.Title : "");
        subjectPara.AppendChild(CreateStyledRun(subjectText, italic: true));
    }

    /// <summary>
    /// Nội dung văn bản: Font Times 14pt, line spacing 1.3, căn đều 2 bên
    /// </summary>
    /// <summary>
    /// Tiền xử lý: Tách căn cứ pháp lý, loại bỏ nhãn QĐ, Kính gửi... khỏi Content
    /// để tránh trùng lặp khi xuất Word (vì WordExportService tự thêm các phần này)
    /// </summary>
    private void CleanupDocumentContent(DocModel document)
    {
        if (string.IsNullOrEmpty(document.Content)) return;

        var lines = document.Content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var cleanedLines = new List<string>();
        var extractedBasedOn = new List<string>();
        var extractedRecipients = new List<string>();
        bool passedLeadingSection = false;

        for (int i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].Trim();
            var upper = trimmed.ToUpper();

            // Bỏ qua dòng trống ở đầu
            if (!passedLeadingSection && string.IsNullOrWhiteSpace(trimmed))
                continue;

            // ── Bỏ Quốc hiệu: "CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM" ──
            if (!passedLeadingSection && upper.Contains("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM"))
                continue;

            // ── Bỏ Tiêu ngữ: "Độc lập - Tự do - Hạnh phúc" ──
            if (!passedLeadingSection && upper.Contains("ĐỘC LẬP") && upper.Contains("TỰ DO") && upper.Contains("HẠNH PHÚC"))
                continue;

            // ── Bỏ dòng gạch ngang trang trí (────, ═══, ---) ──
            if (!passedLeadingSection && IsDecorativeLine(trimmed))
                continue;

            // ── Bỏ tên cơ quan/tổ chức in hoa ở đầu (ỦY BAN NHÂN DÂN..., PHÒNG TÀI CHÍNH...) ──
            if (!passedLeadingSection && IsOrgNameLine(upper))
                continue;

            // ── Bỏ dòng "Số: ..." (AddDocumentInfo đã thêm) ──
            if (!passedLeadingSection && IsDocumentNumberLine(trimmed))
                continue;

            // ── Bỏ dòng ngày tháng ("..., ngày ... tháng ... năm ...") ──
            if (!passedLeadingSection && IsDateLine(trimmed))
                continue;

            // ── Bỏ tên loại văn bản in hoa (BÁO CÁO, CÔNG VĂN, QUYẾT ĐỊNH...) ──
            if (!passedLeadingSection && IsDocumentTypeLine(upper))
                continue;

            // ── Bỏ dòng trích yếu "Về việc ..." / "V/v ..." ──
            if (!passedLeadingSection && IsSubjectLine(trimmed))
                continue;

            // ── Bỏ dòng "Kính gửi:..." (AddSalutation đã xử lý) ──
            if (!passedLeadingSection && trimmed.StartsWith("Kính gửi", StringComparison.OrdinalIgnoreCase))
                continue;

            // ── Bỏ dòng thẩm quyền trùng: "GIÁM ĐỐC SỞ...", "CHỦ TỊCH..." ──
            if (!passedLeadingSection && IsAuthorityLine(upper))
                continue;

            // ── Tách dòng Căn cứ / Theo vào BasedOn[] ──
            if (!passedLeadingSection && 
                (trimmed.StartsWith("Căn cứ", StringComparison.OrdinalIgnoreCase) ||
                 trimmed.StartsWith("Theo ", StringComparison.OrdinalIgnoreCase) ||
                 trimmed.StartsWith("Xét ", StringComparison.OrdinalIgnoreCase)))
            {
                extractedBasedOn.Add(trimmed);
                continue;
            }

            // ── Bỏ nhãn "QUYẾT ĐỊNH:", "NGHỊ QUYẾT:" (AddDecisionLabel đã thêm) ──
            if (!passedLeadingSection && IsDecisionLabelLine(upper))
            {
                passedLeadingSection = true; // Mọi thứ sau nhãn QĐ là nội dung thật
                continue;
            }

            // Dòng trống sau phần căn cứ, trước nội dung → bỏ
            if (!passedLeadingSection && string.IsNullOrWhiteSpace(trimmed))
                continue;

            // Đánh dấu bắt đầu nội dung thật
            passedLeadingSection = true;
            cleanedLines.Add(lines[i]); // Giữ nguyên indentation gốc
        }

        // ═══ Phase 2: Loại bỏ phần "Nơi nhận:" + chữ ký ở cuối nội dung ═══
        // (AddSignature đã tự thêm phần này)
        RemoveTrailingSignatureAndRecipients(cleanedLines, extractedRecipients);

        // Cập nhật Content đã làm sạch
        document.Content = string.Join("\n", cleanedLines);

        // Nếu BasedOn trống và tách được căn cứ từ Content → gán vào
        if ((document.BasedOn == null || document.BasedOn.Length == 0) && extractedBasedOn.Count > 0)
        {
            document.BasedOn = extractedBasedOn.ToArray();
        }

        // Nếu Recipients trống và tách được từ Content → gán vào
        if ((document.Recipients == null || document.Recipients.Length == 0) && extractedRecipients.Count > 0)
        {
            document.Recipients = extractedRecipients.ToArray();
        }
    }

    /// <summary>Kiểm tra dòng có phải đường kẻ trang trí (─── , ═══, ---)</summary>
    private static bool IsDecorativeLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return false;
        var clean = line.Trim();
        // Dòng chỉ gồm các ký tự trang trí
        return clean.Length >= 3 && clean.All(c => c == '─' || c == '═' || c == '-' || c == '—' || c == '━' || c == '_');
    }

    /// <summary>Kiểm tra dòng có phải tên cơ quan/tổ chức in hoa</summary>
    private static bool IsOrgNameLine(string upperLine)
    {
        if (string.IsNullOrWhiteSpace(upperLine) || upperLine.Length < 5 || upperLine.Length > 80) return false;
        // Các tiền tố phổ biến của tên cơ quan hành chính VN
        var orgPrefixes = new[] {
            "ỦY BAN NHÂN DÂN", "UBND", "HỘI ĐỒNG NHÂN DÂN", "HĐND",
            "SỞ ", "PHÒNG ", "BAN ", "CHI CỤC ", "CỤC ",
            "BỘ ", "VĂN PHÒNG ", "TRUNG TÂM ", "HỘI ", "ĐOÀN ",
            "CÔNG AN ", "BỘ CHỈ HUY ", "VIỆN ", "TRƯỜNG ",
            "ĐẢNG ỦY", "ĐẢNG BỘ", "CHI BỘ"
        };
        foreach (var prefix in orgPrefixes)
        {
            if (upperLine.StartsWith(prefix))
                return true;
        }
        return false;
    }

    /// <summary>Kiểm tra dòng có phải dòng số văn bản: "Số: ...", "Số:..."</summary>
    private static bool IsDocumentNumberLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return false;
        var trimmed = line.Trim();
        return trimmed.StartsWith("Số:", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("Số :", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Kiểm tra dòng có phải dòng ngày tháng ("..., ngày ... tháng ... năm ...")</summary>
    private static bool IsDateLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return false;
        var lower = line.ToLower().Trim();
        return lower.Contains("ngày") && lower.Contains("tháng") && lower.Contains("năm") && lower.Length < 80;
    }

    /// <summary>Kiểm tra dòng có phải tên loại văn bản in hoa đứng riêng (BÁO CÁO, CÔNG VĂN...)</summary>
    private static bool IsDocumentTypeLine(string upperLine)
    {
        if (string.IsNullOrWhiteSpace(upperLine)) return false;
        var clean = upperLine.Trim();
        var docTypeNames = new[] {
            "BÁO CÁO", "CÔNG VĂN", "QUYẾT ĐỊNH", "TỜ TRÌNH", "KẾ HOẠCH",
            "THÔNG BÁO", "NGHỊ QUYẾT", "CHỈ THỊ", "HƯỚNG DẪN", "THÔNG TƯ",
            "QUY ĐỊNH", "QUY CHẾ", "NGHỊ ĐỊNH", "LUẬT", "VĂN BẢN",
            "BIÊN BẢN", "HỢP ĐỒNG", "GIẤY MỜI", "GIẤY GIỚI THIỆU",
            "PHIẾU CHUYỂN", "PHIẾU GỬI", "CÔNG ĐIỆN", "ĐỀ ÁN"
        };
        foreach (var name in docTypeNames)
        {
            if (clean == name) return true;
        }
        return false;
    }

    /// <summary>Kiểm tra dòng có phải trích yếu: "Về việc ...", "V/v ..."</summary>
    private static bool IsSubjectLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return false;
        var trimmed = line.Trim();
        return trimmed.StartsWith("Về việc", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("V/v", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("V/V", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Loại bỏ phần "Nơi nhận:" và khối chữ ký ở cuối nội dung.
    /// Tìm từ cuối lên trên để phát hiện "Nơi nhận:" hoặc dòng chức danh ký.
    /// </summary>
    private static void RemoveTrailingSignatureAndRecipients(List<string> lines, List<string> extractedRecipients)
    {
        if (lines.Count == 0) return;

        // Tìm vị trí "Nơi nhận:" từ cuối lên (chỉ tìm trong 25 dòng cuối)
        int noiNhanIndex = -1;
        int searchStart = Math.Max(0, lines.Count - 25);
        for (int i = lines.Count - 1; i >= searchStart; i--)
        {
            var trimmed = lines[i].Trim();
            if (trimmed.StartsWith("Nơi nhận", StringComparison.OrdinalIgnoreCase))
            {
                noiNhanIndex = i;
                break;
            }
        }

        if (noiNhanIndex >= 0)
        {
            // Tách danh sách nơi nhận để lưu vào document.Recipients
            for (int i = noiNhanIndex + 1; i < lines.Count; i++)
            {
                var recipientLine = lines[i].Trim();
                if (!string.IsNullOrWhiteSpace(recipientLine) && !IsSignaturePatternLine(recipientLine))
                {
                    // Bỏ dấu "- " ở đầu nếu có
                    if (recipientLine.StartsWith("- "))
                        recipientLine = recipientLine.Substring(2).Trim();
                    extractedRecipients.Add(recipientLine);
                }
            }

            // Xóa từ "Nơi nhận:" đến hết — nhưng cũng check phía trước nếu có
            // dòng kết luận trống dư thừa
            while (noiNhanIndex > 0 && string.IsNullOrWhiteSpace(lines[noiNhanIndex - 1].Trim()))
                noiNhanIndex--;
            lines.RemoveRange(noiNhanIndex, lines.Count - noiNhanIndex);
            return;
        }

        // Nếu không có "Nơi nhận:", tìm khối chữ ký ở cuối (TRƯỞNG PHÒNG, CHỦ TỊCH...)
        // Tìm từ cuối lên, gặp dòng chức danh in hoa → xóa từ đó đến hết
        int signatureStart = -1;
        for (int i = lines.Count - 1; i >= searchStart; i--)
        {
            var trimmed = lines[i].Trim();
            var upper = trimmed.ToUpper();
            if (IsAuthorityLine(upper) || IsSignaturePatternLine(trimmed))
            {
                signatureStart = i;
                // Tiếp tục lên trên để bắt toàn bộ block chữ ký
            }
            else if (!string.IsNullOrWhiteSpace(trimmed) && signatureStart >= 0)
            {
                // Gặp nội dung thật → dừng
                break;
            }
        }

        if (signatureStart >= 0)
        {
            // Xóa dòng trống trước block chữ ký
            while (signatureStart > 0 && string.IsNullOrWhiteSpace(lines[signatureStart - 1].Trim()))
                signatureStart--;
            lines.RemoveRange(signatureStart, lines.Count - signatureStart);
        }
    }

    /// <summary>Kiểm tra dòng có phải pattern chữ ký: "(Chữ ký, dấu)", "(Ký, ghi rõ họ tên...)", "(Họ và tên)"</summary>
    private static bool IsSignaturePatternLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return false;
        var trimmed = line.Trim().ToLower();
        return trimmed.Contains("(chữ ký") ||
               trimmed.Contains("(ký,") ||
               trimmed.Contains("(ký tên") ||
               trimmed.Contains("(họ và tên") ||
               trimmed.Contains("(họ tên") ||
               trimmed.Contains("ghi rõ họ tên") ||
               trimmed.Contains("đóng dấu");
    }

    /// <summary>Kiểm tra dòng có phải nhãn quyết định (QUYẾT ĐỊNH:, NGHỊ QUYẾT:...)</summary>
    private static bool IsDecisionLabelLine(string upperLine)
    {
        var labels = new[] { "QUYẾT ĐỊNH:", "NGHỊ QUYẾT:", "CHỈ THỊ:", "QUYẾT NGHỊ:" };
        foreach (var label in labels)
        {
            if (upperLine == label || upperLine == label.TrimEnd(':'))
                return true;
        }
        return false;
    }

    /// <summary>Kiểm tra dòng có phải dòng thẩm quyền (GIÁM ĐỐC, CHỦ TỊCH...)</summary>
    private static bool IsAuthorityLine(string upperLine)
    {
        if (string.IsNullOrWhiteSpace(upperLine)) return false;
        var authorityPrefixes = new[] {
            "GIÁM ĐỐC", "PHÓ GIÁM ĐỐC", "CHỦ TỊCH", "PHÓ CHỦ TỊCH",
            "CHÁNH VĂN PHÒNG", "TRƯỞNG BAN", "TRƯỞNG PHÒNG",
            "BỘ TRƯỞNG", "THỨ TRƯỞNG", "TỔNG GIÁM ĐỐC"
        };
        foreach (var prefix in authorityPrefixes)
        {
            if (upperLine.StartsWith(prefix) && !upperLine.Contains("ĐIỆN")) // "GIÁM ĐỐC SỞ ĐIỆN:" là nội dung
                return true;
        }
        return false;
    }

    private void AddContent(Body body, DocModel document)
    {
        // Nội dung văn bản - chia thành các đoạn
        var contentText = !string.IsNullOrEmpty(document.Content)
            ? document.Content
            : "[Nội dung văn bản]";

        // Loại bỏ markdown artifacts từ AI
        contentText = contentText.Replace("**", "").Replace("__", "");
        contentText = contentText.Replace("```", "").Replace("`", "");
        contentText = System.Text.RegularExpressions.Regex.Replace(
            contentText, @"^#{1,6}\s*", "", System.Text.RegularExpressions.RegexOptions.Multiline);

        var lines = contentText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            var para = body.AppendChild(new Paragraph());

            if (string.IsNullOrWhiteSpace(line))
            {
                // Đoạn trống - giữ line spacing 1.3 (ParagraphProperties là child duy nhất → OK)
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

            // Xác định formatting dựa trên loại dòng
            bool isBold = false;
            string spacingBefore = "0";
            string spacingAfter = "0";
            var justification = JustificationValues.Both;
            string? indent = "567"; // Mặc định 1cm

            switch (lineType)
            {
                case ContentLineType.ChuongPhan: // Chương I, Phần thứ nhất...
                    isBold = true;
                    justification = JustificationValues.Center;
                    indent = null; // Không thụt cho căn giữa
                    spacingBefore = SpacingLarge;
                    spacingAfter = SpacingSmall;
                    break;

                case ContentLineType.Dieu: // Điều 1, Điều 2...
                    isBold = true;
                    spacingBefore = SpacingMedium;
                    break;

                case ContentLineType.Khoan: // 1. ..., 2. ...
                    spacingBefore = SpacingSmall;
                    break;

                case ContentLineType.Diem: // a) ..., b) ...
                    indent = "851"; // Thụt sâu hơn 1.5cm
                    break;

                default: // Nội dung thường
                    break;
            }

            // ParagraphProperties FIRST
            var paraProps = para.AppendChild(new ParagraphProperties());
            paraProps.AppendChild(new Justification() { Val = justification });
            if (indent != null)
                paraProps.AppendChild(new Indentation() { FirstLine = indent });
            paraProps.AppendChild(new SpacingBetweenLines()
            {
                Before = spacingBefore,
                After = spacingAfter,
                Line = LineSpacing13,
                LineRule = LineSpacingRuleValues.Auto
            });

            // Run AFTER ParagraphProperties (với RunProperties trước Text)
            if (lineType == ContentLineType.Dieu)
            {
                // Điều X. — chỉ in đậm phần "Điều X." theo NĐ 30/2020
                var dieuMatch = System.Text.RegularExpressions.Regex.Match(trimmedLine, @"^(Điều\s+\d+[\.:]\s*)(.*)$");
                if (dieuMatch.Success)
                {
                    para.AppendChild(CreateStyledRun(dieuMatch.Groups[1].Value, bold: true));
                    if (!string.IsNullOrEmpty(dieuMatch.Groups[2].Value))
                        para.AppendChild(CreateStyledRun(dieuMatch.Groups[2].Value, bold: false));
                }
                else
                {
                    para.AppendChild(CreateStyledRun(trimmedLine, bold: true));
                }
            }
            else
            {
                para.AppendChild(CreateStyledRun(trimmedLine, bold: isBold));
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

        // Tiêu đề La Mã + chữ hoa: "I. MỤC TIÊU:", "II. NỘI DUNG:", "III. KINH PHÍ:"
        // Chỉ match khi bắt đầu bằng số La Mã + dấu chấm + toàn chữ hoa
        if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^[IVXLCDM]+\.\s+[A-ZÀÁẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬĐÈÉẺẼẸÊẾỀỂỄỆÌÍỈĨỊÒÓỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢÙÚỦŨỤƯỨỪỬỮỰỲÝỶỸỴ\s,\-:]+$") && trimmed.Length <= 60)
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
            // "Nơi nhận:" header
            var receiverPara = leftCell.AppendChild(new Paragraph());
            // ParagraphProperties FIRST
            var receiverParaProps = receiverPara.AppendChild(new ParagraphProperties());
            receiverParaProps.AppendChild(new SpacingBetweenLines()
            {
                After = SpacingSmall,
                Line = LineSpacing13,
                LineRule = LineSpacingRuleValues.Auto
            });
            receiverPara.AppendChild(CreateStyledRun("Nơi nhận:", bold: true));

            // Danh sách nơi nhận (từ document.Recipients)
            var receiverListPara = leftCell.AppendChild(new Paragraph());
            var receiverListRun = new Run();
            // RunProperties FIRST
            var listRunProps = receiverListRun.AppendChild(new RunProperties());
            listRunProps.AppendChild(new RunFonts()
            {
                Ascii = "Times New Roman",
                HighAnsi = "Times New Roman",
                EastAsia = "Times New Roman",
                ComplexScript = "Times New Roman"
            });
            listRunProps.AppendChild(new FontSize() { Val = "28" }); // 14pt
            listRunProps.AppendChild(new FontSizeComplexScript() { Val = "28" });

            // Text + Break elements AFTER RunProperties
            if (document.Recipients != null && document.Recipients.Length > 0)
            {
                // Sử dụng danh sách từ document
                for (int i = 0; i < document.Recipients.Length; i++)
                {
                    if (i > 0)
                    {
                        receiverListRun.AppendChild(new Break());
                    }
                    receiverListRun.AppendChild(new Text(document.Recipients[i])
                        { Space = SpaceProcessingModeValues.Preserve });
                }
            }
            else
            {
                // Mặc định nếu không có
                receiverListRun.AppendChild(new Text("- Như trên;")
                    { Space = SpaceProcessingModeValues.Preserve });
                receiverListRun.AppendChild(new Break());
                receiverListRun.AppendChild(new Text("- Lưu: VT.")
                    { Space = SpaceProcessingModeValues.Preserve });
            }

            receiverListPara.AppendChild(receiverListRun);
        }
        else
        {
            // TableCell bắt buộc phải có ít nhất 1 Paragraph (OpenXML spec)
            leftCell.AppendChild(new Paragraph());
        }

        // Cột phải: Địa điểm, ngày + Chữ ký
        var rightCell = tr.AppendChild(new TableCell());
        var rightCellProps = rightCell.AppendChild(new TableCellProperties());
        rightCellProps.AppendChild(new TableCellWidth() { Width = "2500", Type = TableWidthUnitValues.Pct }); // 50%
        rightCellProps.AppendChild(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Top });

        // Địa điểm, ngày tháng (in nghiêng, căn giữa) - dùng Location từ document
        var locationName = !string.IsNullOrEmpty(document.Location) ? document.Location : "...";
        var locationPara = rightCell.AppendChild(new Paragraph());
        // ParagraphProperties FIRST
        var locationParaProps = locationPara.AppendChild(new ParagraphProperties());
        locationParaProps.AppendChild(new Justification() { Val = JustificationValues.Center });
        locationParaProps.AppendChild(new SpacingBetweenLines()
        {
            After = SpacingMedium,
            Line = LineSpacing13,
            LineRule = LineSpacingRuleValues.Auto
        });
        locationPara.AppendChild(CreateStyledRun(
            $"{locationName}, ngày {document.IssueDate:dd} tháng {document.IssueDate:MM} năm {document.IssueDate:yyyy}",
            italic: true));

        // Thẩm quyền ký (TM., KT., Q. - chỉ hiện nếu có)
        if (!string.IsNullOrEmpty(document.SigningAuthority))
        {
            var tmPara = rightCell.AppendChild(new Paragraph());
            var tmParaProps = tmPara.AppendChild(new ParagraphProperties());
            tmParaProps.AppendChild(new Justification() { Val = JustificationValues.Center });
            tmParaProps.AppendChild(new SpacingBetweenLines()
            {
                After = "0",
                Line = LineSpacing13,
                LineRule = LineSpacingRuleValues.Auto
            });
            tmPara.AppendChild(CreateStyledRun(document.SigningAuthority.ToUpper(), bold: true));
        }

        // Chức danh ký (CHỦ TỊCH, GIÁM ĐỐC, TRƯỞNG PHÒNG...)
        var signingTitle = !string.IsNullOrEmpty(document.SigningTitle) ? document.SigningTitle.ToUpper() : "[CHỨC DANH]";
        var titlePara = rightCell.AppendChild(new Paragraph());
        var titleParaProps = titlePara.AppendChild(new ParagraphProperties());
        titleParaProps.AppendChild(new Justification() { Val = JustificationValues.Center });
        titleParaProps.AppendChild(new SpacingBetweenLines()
        {
            After = "0",
            Line = LineSpacing13,
            LineRule = LineSpacingRuleValues.Auto
        });
        titlePara.AppendChild(CreateStyledRun(signingTitle, bold: true));

        // "(Ký, ghi rõ họ tên và đóng dấu)" (in nghiêng, căn giữa)
        var signNotePara = rightCell.AppendChild(new Paragraph());
        var noteParaProps = signNotePara.AppendChild(new ParagraphProperties());
        noteParaProps.AppendChild(new Justification() { Val = JustificationValues.Center });
        noteParaProps.AppendChild(new SpacingBetweenLines()
        {
            After = SpacingMedium,
            Line = LineSpacing13,
            LineRule = LineSpacingRuleValues.Auto
        });
        signNotePara.AppendChild(CreateStyledRun("(Ký, ghi rõ họ tên và đóng dấu)", italic: true));

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

        // Họ tên người ký (in đậm, căn giữa, KHÔNG in hoa)
        var namePara = rightCell.AppendChild(new Paragraph());
        var nameParaProps = namePara.AppendChild(new ParagraphProperties());
        nameParaProps.AppendChild(new Justification() { Val = JustificationValues.Center });
        namePara.AppendChild(CreateStyledRun(
            !string.IsNullOrEmpty(document.SignedBy) ? document.SignedBy : "[Họ tên người ký]",
            bold: true));
    }

    #region Export Content (Reusable - Xuất nội dung text bất kỳ ra Word chuẩn)

    /// <summary>
    /// Tùy chọn xuất nội dung ra Word — dùng chung cho tất cả các trang (AI Báo cáo, AI Soạn, v.v.)
    /// </summary>
    public class ExportContentOptions
    {
        /// <summary>Tên đơn vị ban hành (VD: "UBND xã Gia Kiệm")</summary>
        public string OrgName { get; set; } = "";

        /// <summary>Tên loại văn bản in hoa (VD: "BÁO CÁO")</summary>
        public string DocumentTypeName { get; set; } = "BÁO CÁO";

        /// <summary>Trích yếu (VD: "Tình hình kinh tế - xã hội tháng 01/2026")</summary>
        public string Subject { get; set; } = "";

        /// <summary>Họ tên người ký</summary>
        public string SignerName { get; set; } = "";

        /// <summary>Chức danh người ký (VD: "Chủ tịch UBND", "Trưởng Công an xã")</summary>
        public string SignerTitle { get; set; } = "";

        /// <summary>Địa danh (VD: "Gia Kiệm"). Nếu rỗng sẽ tự trích từ OrgName</summary>
        public string Location { get; set; } = "";

        /// <summary>Ngày ký. Mặc định = hôm nay</summary>
        public DateTime IssueDate { get; set; } = DateTime.Now;

        /// <summary>Danh sách nơi nhận (tùy chọn). VD: ["Như trên;", "Lưu: VT."]</summary>
        public string[]? Recipients { get; set; }
    }

    /// <summary>
    /// Xuất nội dung text (VD: AI-generated content) ra file Word chuẩn TT01/2011.
    /// Có thể reuse từ bất kỳ dialog/page nào trong ứng dụng.
    /// Format: Header 2 cột (cơ quan | quốc hiệu) → Tên loại VB → Trích yếu → Nội dung → Chữ ký.
    /// </summary>
    public void ExportContent(string outputPath, string content, ExportContentOptions options)
    {
        if (string.IsNullOrEmpty(outputPath))
            throw new ArgumentException("Đường dẫn file không được rỗng", nameof(outputPath));
        if (string.IsNullOrEmpty(content))
            throw new ArgumentException("Nội dung không được rỗng", nameof(content));

        options ??= new ExportContentOptions();

        // Tạo Document model tạm để tái sử dụng các method AddHeader, AddContent, AddSignature
        var tempDoc = new DocModel
        {
            Issuer = options.OrgName,
            Type = DocType.BaoCao,
            Title = options.DocumentTypeName,
            Subject = options.Subject,
            Content = content,
            SignedBy = options.SignerName,
            SigningTitle = options.SignerTitle,
            Location = !string.IsNullOrEmpty(options.Location)
                ? options.Location
                : ExtractLocationFromOrg(options.OrgName),
            IssueDate = options.IssueDate,
            Recipients = options.Recipients ?? new[] { "- Như trên;", "- Lưu: VT." },
            Direction = Direction.Di
        };

        try
        {
            using var wordDoc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new WordDoc();
            var body = mainPart.Document.AppendChild(new Body());

            // Header: Cơ quan | Quốc hiệu
            CleanupDocumentContent(tempDoc);
            AddHeader(body, tempDoc);

            // Số VB + Ngày + Tên loại + Trích yếu
            AddDocumentInfo(body, tempDoc);

            // Nội dung
            AddContent(body, tempDoc);

            // Chữ ký (Nơi nhận | Chức danh + Tên)
            AddSignature(body, tempDoc);

            // SectionProperties PHẢI là child CUỐI CÙNG của Body — Margins theo TT01/2011
            SetPageMargins(body);

            mainPart.Document.Save();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi xuất nội dung ra Word: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Trích địa danh từ tên cơ quan.
    /// VD: "UBND xã Gia Kiệm" → "Gia Kiệm", "Hội LHPN xã Gia Kiệm" → "Gia Kiệm"
    /// </summary>
    private string ExtractLocationFromOrg(string orgName)
    {
        if (string.IsNullOrEmpty(orgName)) return "...";

        var locationPrefixes = new[] { " xã ", " huyện ", " tỉnh ", " thành phố ", " TP. ", " TP ",
            " thị xã ", " thị trấn ", " phường ", " quận " };

        foreach (var prefix in locationPrefixes)
        {
            var idx = orgName.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                return orgName.Substring(idx + prefix.Length).Trim();
            }
        }

        return "...";
    }

    #endregion

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
        if (string.IsNullOrEmpty(issuer)) return "";

        var upper = StripTitleKeywords(issuer.ToUpper().Trim());

        // Các pattern phổ biến: tách tên tổ chức khỏi tên địa phương
        // "ỦY BAN NHÂN DÂN XÃ/HUYỆN/TỈNH/TP..." → "ỦY BAN NHÂN DÂN"
        var locationPrefixes = new[] {
            " XÃ ", " HUYỆN ", " TỈNH ", " THÀNH PHỐ ", " TP. ", " TP ", " THỊ XÃ ", " THỊ TRẤN ",
            " PHƯỜNG ", " QUẬN "
        };

        foreach (var prefix in locationPrefixes)
        {
            var idx = upper.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
            {
                return upper.Substring(0, idx).Trim();
            }
        }

        // Sở, Ban, Ngành cấp tỉnh hoặc single-level org → không có cơ quan cấp trên
        return "";
    }

    /// <summary>
    /// Tách tên đơn vị con (phần sau cơ quan cấp trên, gạch chân)
    /// VD: "ỦY BAN NHÂN DÂN XÃ GIA KIỂM" → "XÃ GIA KIỂM"
    ///     "SỞ GIÁO DỤC VÀ ĐÀO TẠO TỈNH ĐỒNG NAI" → "TỈNH ĐỒNG NAI"
    /// </summary>
    private string ExtractSubOrg(string issuer)
    {
        if (string.IsNullOrEmpty(issuer)) return "";

        var upper = StripTitleKeywords(issuer.ToUpper().Trim());

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

        // Single-level org (Sở, Ban, Ngành...) → trả về toàn bộ tên (gạch chân)
        return upper;
    }

    /// <summary>
    /// Loại bỏ chức danh khỏi tên cơ quan ("GIÁM ĐỐC SỞ NỘI VỤ" → "SỞ NỘI VỤ")
    /// Tránh lẫn lộn giữa Issuer (cơ quan) và SigningTitle (chức danh)
    /// </summary>
    private string StripTitleKeywords(string orgName)
    {
        if (string.IsNullOrEmpty(orgName)) return orgName;

        var titlePrefixes = new[] {
            "GIÁM ĐỐC ", "PHÓ GIÁM ĐỐC ",
            "CHỦ TỊCH ", "PHÓ CHỦ TỊCH ",
            "TRƯỞNG BAN ", "PHÓ TRƯỞNG BAN ",
            "TRƯỞNG PHÒNG ", "PHÓ TRƯỞNG PHÒNG ",
            "CHÁNH VĂN PHÒNG ", "PHÓ CHÁNH VĂN PHÒNG ",
            "TỔNG GIÁM ĐỐC ", "PHÓ TỔNG GIÁM ĐỐC ",
            "BÍ THƯ ", "PHÓ BÍ THƯ ",
            "CHÁNH ÁN ", "VIỆN TRƯỞNG "
        };

        foreach (var prefix in titlePrefixes)
        {
            if (orgName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var stripped = orgName.Substring(prefix.Length).Trim();
                // Chỉ strip nếu phần còn lại đủ dài để là tên cơ quan có nghĩa
                // VD: "GIÁM ĐỐC SỞ NỘI VỤ" → "SỞ NỘI VỤ" (OK, 10 chars)
                // VD: "GIÁM ĐỐC SỞ" → "SỞ" (3 chars, quá ngắn → không strip)
                if (!string.IsNullOrEmpty(stripped) && stripped.Length >= 5)
                    return stripped;
            }
        }

        return orgName;
    }

    /// <summary>
    /// Dòng thẩm quyền ban hành - xuất hiện sau tiêu đề, trước căn cứ
    /// VD: "CHỦ TỊCH ỦY BAN NHÂN DÂN XÃ GIA KIỂM"
    ///     "GIÁM ĐỐC SỞ GIÁO DỤC VÀ ĐÀO TẠO"
    /// </summary>
    private void AddAuthorityLine(Body body, DocModel document)
    {
        // Tạo dòng thẩm quyền: [ChứcDanh] [CơQuanBanHành]
        // VD: "GIÁM ĐỐC SỞ NỘI VỤ", "CHỦ TỊCH UBND XÃ GIA KIỆM"
        var authorityText = "";

        var signingTitle = document.SigningTitle?.Trim().ToUpper() ?? "";
        var issuer = document.Issuer?.Trim().ToUpper() ?? "";
        var cleanIssuer = StripTitleKeywords(issuer); // Bỏ chức danh khỏi tên CQ nếu bị lẫn

        if (!string.IsNullOrEmpty(signingTitle) && !string.IsNullOrEmpty(cleanIssuer))
        {
            // Tránh trùng: nếu SigningTitle đã chứa trong Issuer thì dùng Issuer trực tiếp
            if (issuer.Contains(signingTitle, StringComparison.OrdinalIgnoreCase))
            {
                authorityText = issuer;
            }
            else
            {
                authorityText = $"{signingTitle} {cleanIssuer}";
            }
        }
        else if (!string.IsNullOrEmpty(signingTitle))
        {
            authorityText = signingTitle;
        }
        else if (!string.IsNullOrEmpty(issuer))
        {
            // Issuer có thể chứa cả chức danh (VD: "Giám đốc Sở") — dùng nguyên
            authorityText = issuer;
        }

        if (string.IsNullOrEmpty(authorityText)) return;

        var para = body.AppendChild(new Paragraph());
        // ParagraphProperties FIRST
        var paraProps = para.AppendChild(new ParagraphProperties());
        paraProps.AppendChild(new Justification() { Val = JustificationValues.Center });
        paraProps.AppendChild(new SpacingBetweenLines()
        {
            After = SpacingLarge,
            Line = SingleLine,
            LineRule = LineSpacingRuleValues.Auto
        });
        // Run AFTER ParagraphProperties
        para.AppendChild(CreateStyledRun(authorityText, bold: true));
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
        // ParagraphProperties FIRST
        var paraProps = para.AppendChild(new ParagraphProperties());
        paraProps.AppendChild(new Justification() { Val = JustificationValues.Center });
        paraProps.AppendChild(new SpacingBetweenLines()
        {
            After = SpacingMedium,
            Line = LineSpacing13,
            LineRule = LineSpacingRuleValues.Auto
        });
        // Run AFTER ParagraphProperties
        para.AppendChild(CreateStyledRun(label, bold: true));
    }

    #endregion
}
