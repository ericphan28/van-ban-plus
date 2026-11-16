using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Địa phương đưa vào prompt soạn thảo (spec mục 10; hợp đồng tác vụ v2, trường «dia_phuong»).
/// CoQuanCapTren bỏ trống: cấp trên là tỉnh (chính quyền hai cấp). DacDiem: mỗi phần tử thành một dòng «- …».
/// </summary>
public sealed record ComposeLocality(string Xa, string? CoQuanCapTren, string Tinh, IReadOnlyList<string>? DacDiem = null)
{
    /// <summary>Giá trị trước đây ghi cứng trong AIComposeDialog. Hộp thoại dùng tới khi đợt 3 nối thiết lập cơ quan.</summary>
    public static ComposeLocality GiaKiem { get; } = new(
        "xã Gia Kiệm", "thành phố Thống Nhất", "tỉnh Đồng Nai",
        new[]
        {
            "Dân số: 79.274 nhân khẩu, 19.818 hộ",
            "17 ấp, 5 giáo xứ Công giáo (96% dân theo đạo)",
            "Xã loại I, đang xây dựng nông thôn mới nâng cao",
        });

    /// <summary>Địa danh ở dòng ngày tháng: bỏ tiền tố «xã», «phường», «thị trấn», «đặc khu».</summary>
    public static string TenDiaDanh(string xa)
    {
        foreach (var p in new[] { "xã ", "phường ", "thị trấn ", "đặc khu " })
            if (xa.StartsWith(p, StringComparison.OrdinalIgnoreCase)) return xa[p.Length..].Trim();
        return xa.Trim();
    }
}

/// <summary>
/// Prompt soạn thảo văn bản, tách NGUYÊN VĂN từ AIComposeDialog (đợt 2b). Đầu vào thuần: loại văn bản, tên mẫu,
/// các trường (khóa → giá trị, theo thứ tự ô nhập) và địa phương. Không có địa phương thì prompt không nêu địa danh nào.
/// Máy chủ (vanbanplus-api, tác vụ soan-thao) dựng lại y hệt; bộ kiểm so khớp tệp mẫu do lớp này sinh ra.
/// </summary>
public sealed class ComposePromptBuilder
{
    private readonly string? _templateName;
    private readonly ComposeLocality? _dp;
    private readonly int _nam;

    private ComposePromptBuilder(string? templateName, ComposeLocality? dp, int nam = 0)
    {
        if (dp != null && (string.IsNullOrWhiteSpace(dp.Xa) || string.IsNullOrWhiteSpace(dp.Tinh)))
            throw new ArgumentException("Địa phương phải có xã/phường và tỉnh", nameof(dp));
        _templateName = templateName;
        _dp = dp;
        _nam = nam;
    }

    /// <summary>System instruction. dp = null: không có bối cảnh địa phương.
    /// nam: năm hiện tại cho dòng «địa danh, ngày ... tháng ... năm …» (spec §5: thời điểm là tham số để kiểm so khớp).</summary>
    public static string BuildSystemInstruction(DocumentType type, ComposeLocality? dp, int nam) =>
        new ComposePromptBuilder(null, dp, nam).BuildSystemInstructionCore(type);

    /// <summary>User prompt. fields theo thứ tự ô nhập; templateName là tên mẫu đang chọn (null: chưa chọn mẫu).</summary>
    public static string BuildUserPrompt(DocumentType type, string? templateName, Dictionary<string, string> fields, ComposeLocality? dp) =>
        new ComposePromptBuilder(templateName, dp).BuildStructuredPrompt(type, fields);

    // ---- Địa phương. Có _dp: dựng đúng chữ cũ (Gia Kiệm khớp từng ký tự). Không có: chỗ trống trung tính. ----

    // Xuống dòng đúng kiểu của tệp nguồn này (CRLF khi checkout trên Windows), giống mọi chuỗi @"..." khác của lớp.
    private static readonly string NL = @"
";
    private const string VACH = "═══════════════════════════════════════";

    private string? CapTren => string.IsNullOrWhiteSpace(_dp?.CoQuanCapTren) ? null : _dp!.CoQuanCapTren;
    private string CoQuanThuong => _dp == null ? "[cơ quan ban hành]" : $"UBND {_dp.Xa}";
    private string CoQuanHoa => _dp == null ? "[CƠ QUAN BAN HÀNH]" : $"UBND {_dp.Xa.ToUpperInvariant()}";
    private string CapTrenHoa => _dp == null ? "[CƠ QUAN CẤP TRÊN]" : $"UBND {(CapTren ?? _dp.Tinh).ToUpperInvariant()}";
    private string TenXaHoa => _dp == null ? "[TÊN XÃ/PHƯỜNG]" : _dp.Xa.ToUpperInvariant();
    private string DiaDanh => _dp == null ? "[Địa danh]" : ComposeLocality.TenDiaDanh(_dp.Xa);
    private string TenCoQuanQuyetDinh => _dp == null ? "" : $" (UBND {_dp.Xa.ToUpperInvariant()})";

    private string KhoiDiaPhuong
    {
        get
        {
            if (_dp == null) return "";
            var sb = new System.Text.StringBuilder();
            sb.Append(VACH).Append(NL);
            sb.Append("BỐI CẢNH ĐỊA PHƯƠNG (").Append(_dp.Xa).Append("):").Append(NL);
            sb.Append(VACH).Append(NL);
            sb.Append("- ").Append(char.ToUpperInvariant(_dp.Xa[0])).Append(_dp.Xa, 1, _dp.Xa.Length - 1).Append(", ");
            if (CapTren != null) sb.Append(CapTren).Append(", ");
            sb.Append(_dp.Tinh).Append(NL);
            foreach (var d in _dp.DacDiem ?? Array.Empty<string>()) sb.Append("- ").Append(d).Append(NL);
            sb.Append("- Cơ quan cấp trên: ");
            if (CapTren != null) sb.Append("UBND ").Append(CapTren).Append(", ");
            sb.Append("UBND ").Append(_dp.Tinh).Append(NL).Append(NL);
            return sb.ToString();
        }
    }

    // ==== Dưới đây chép nguyên văn từ AIComposeDialog.xaml.cs (chỉ thay các chỗ địa phương / tên mẫu) ====

    private string BuildSystemInstructionCore(DocumentType type)
    {
        var templateType = type.ToString();
        
        return $@"Bạn là CHUYÊN VIÊN VĂN THƯ CAO CẤP tại UBND cấp xã/phường Việt Nam với 20 năm kinh nghiệm soạn thảo văn bản hành chính. Bạn nắm vững:
- Luật Ban hành VBQPPL năm 2015 (sửa đổi 2020)
- Nghị định 30/2020/NĐ-CP ngày 05/3/2020 về công tác văn thư
- Nghị định 154/2020/NĐ-CP sửa đổi NĐ 34/2016 về VBQPPL
- Thông tư 01/2011/TT-BNV hướng dẫn thể thức và kỹ thuật trình bày văn bản hành chính
- Quy trình soạn thảo, trình ký, ban hành văn bản tại UBND cấp xã

NHIỆM VỤ: Soạn thảo văn bản hành chính HOÀN CHỈNH, ĐÚNG THỂ THỨC, sẵn sàng in ấn và ban hành.

═══════════════════════════════════════
QUY TẮC THỂ THỨC (Theo NĐ 30/2020):
═══════════════════════════════════════

1. QUỐC HIỆU VÀ TIÊU NGỮ (bắt buộc):
   CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
   Độc lập - Tự do - Hạnh phúc
   (Tiêu ngữ có gạch ngang ở giữa, có gạch liền phía dưới)

2. TÊN CƠ QUAN BAN HÀNH:
   - Cơ quan cấp trên (nếu có): {CapTrenHoa}
   - Cơ quan ban hành: {CoQuanHoa} (viết hoa, in đậm)
   - Giữa hai dòng có gạch ngang

3. SỐ VÀ KÝ HIỆU (Phụ lục III, NĐ 30/2020 — đầy đủ 29 loại VB hành chính):
   - Nghị quyết: Số:    /NQ-HĐND hoặc /NQ-UBND
   - Quyết định: Số:    /QĐ-UBND
   - Chỉ thị: Số:    /CT-UBND
   - Quy chế: Số:    /QC-UBND
   - Quy định: Số:    /QyĐ-UBND
   - Thông cáo: Số:    /TC-UBND
   - Thông báo: Số:    /TB-UBND
   - Hướng dẫn: Số:    /HD-UBND
   - Chương trình: Số:    /CTr-UBND
   - Kế hoạch: Số:    /KH-UBND
   - Phương án: Số:    /PA-UBND
   - Đề án: Số:    /ĐA-UBND
   - Dự án: Số:    /DA-UBND
   - Báo cáo: Số:    /BC-UBND
   - Biên bản: (không đánh số ký hiệu)
   - Tờ trình: Số:    /TTr-UBND
   - Hợp đồng: Số:    /HĐ-UBND
   - Công văn: Số:    /CV-UBND
   - Công điện: Số:    /CĐ-UBND
   - Bản ghi nhớ: (không đánh số ký hiệu)
   - Bản thỏa thuận: (không đánh số ký hiệu)
   - Giấy ủy quyền: Số:    /GUQ-UBND
   - Giấy mời: Số:    /GM-UBND
   - Giấy giới thiệu: Số:    /GGT-UBND
   - Giấy nghỉ phép: Số:    /GNP-UBND
   - Phiếu gửi: Số:    /PG-UBND
   - Phiếu chuyển: Số:    /PC-UBND
   - Phiếu báo: Số:    /PB-UBND
   - Thư công: (không đánh số ký hiệu)

4. ĐỊA DANH VÀ NGÀY THÁNG:
   ""{DiaDanh}, ngày ... tháng ... năm {_nam}""

5. TRÍCH YẾU NỘI DUNG (V/v):
   - Công văn: ""V/v [nội dung]""
   - QĐ/NQ: ""QUYẾT ĐỊNH / NGHỊ QUYẾT"" + ""Về việc [nội dung]""
   - Báo cáo: ""BÁO CÁO"" + ""[Về nội dung / Kết quả...]""

6. NỘI DUNG VĂN BẢN:
   - Công văn: Câu dẫn → Nội dung chính → Đề nghị → Kết
   - QĐ: Căn cứ → QUYẾT ĐỊNH: Điều 1, 2, 3...
   - Báo cáo: Phần I (Kết quả) → Phần II (Tồn tại) → Phần III (Phương hướng)
   - Tờ trình: Căn cứ → Lý do → Nội dung đề xuất → Kinh phí → Kiến nghị
   - Kế hoạch: Mục đích → Yêu cầu → Nội dung → Tổ chức thực hiện → Kinh phí
   - Thông báo: Nội dung thông báo → Thời gian → Địa điểm → Yêu cầu
   - Nghị quyết: Căn cứ → Điều khoản

7. NƠI NHẬN:
   ""Nơi nhận:"" (in nghiêng, in đậm)
   - Như trên;
   - [Các đơn vị liên quan];
   - Lưu: VT, [bộ phận].

8. CHỮ KÝ:
   [CHỨC DANH IN HOA]
   (Chữ ký, đóng dấu)
   [Họ và tên]

═══════════════════════════════════════
QUY TẮC VIẾT:
═══════════════════════════════════════

- VĂN PHONG: Hành chính chuẩn, trang trọng, mạch lạc, không dùng khẩu ngữ
- NGÔI THỨ: Ngôi thứ ba (""UBND xã"", ""Chủ tịch UBND""), không dùng ""tôi"", ""chúng tôi""
- CÂU CHỮ: Ngắn gọn, rõ ràng, chính xác, không mơ hồ
- SỐ LIỆU: Ghi kèm đơn vị, viết bằng số + chữ nếu là tiền/diện tích quan trọng
- VIỆN DẪN: Ghi đầy đủ số hiệu VB (VD: ""Theo Quyết định số 15/QĐ-UBND ngày 10/01/2026"")
- PLAIN TEXT: KHÔNG dùng markdown (**, *, #, ```), KHÔNG dùng emoji
- XUỐNG DÒNG: Bình thường, KHÔNG viết literal \\n
- Gạch đầu dòng dùng dấu ""-""
- TRÌNH BÀY: Sạch sẽ, có thể in trực tiếp lên giấy A4

{KhoiDiaPhuong}Hãy soạn văn bản loại: {templateType}";
    }

    private string BuildStructuredPrompt(DocumentType templateType, Dictionary<string, string> fields)
    {
        string GetField(string key) => fields.TryGetValue(key, out var val) ? val : "";

        return templateType switch
        {
            DocumentType.CongVan => BuildCongVanPrompt(fields, GetField),
            DocumentType.QuyetDinh => BuildQuyetDinhPrompt(fields, GetField),
            DocumentType.BaoCao => BuildBaoCaoPrompt(fields, GetField),
            DocumentType.ToTrinh => BuildToTrinhPrompt(fields, GetField),
            DocumentType.KeHoach => BuildKeHoachPrompt(fields, GetField),
            DocumentType.ThongBao => BuildThongBaoPrompt(fields, GetField),
            DocumentType.NghiQuyet => BuildNghiQuyetPrompt(fields, GetField),
            // === 22 loại VB bổ sung — NĐ 30/2020 ===
            DocumentType.ChiThi => BuildChiThiPrompt(fields, GetField),
            DocumentType.QuyChE => BuildQuyChEPrompt(fields, GetField),
            DocumentType.QuyDinh => BuildQuyDinhPrompt(fields, GetField),
            DocumentType.ThongCao => BuildThongCaoPrompt(fields, GetField),
            DocumentType.HuongDan => BuildHuongDanPrompt(fields, GetField),
            DocumentType.ChuongTrinh => BuildChuongTrinhPrompt(fields, GetField),
            DocumentType.PhuongAn or DocumentType.DeAn or DocumentType.DuAn => BuildDeAnDuAnPrompt(templateType, fields, GetField),
            DocumentType.BienBan => BuildBienBanPrompt(fields, GetField),
            DocumentType.HopDong => BuildHopDongPrompt(fields, GetField),
            DocumentType.CongDien => BuildCongDienPrompt(fields, GetField),
            DocumentType.BanGhiNho or DocumentType.BanThoaThuan => BuildThoaThuanPrompt(templateType, fields, GetField),
            DocumentType.GiayUyQuyen => BuildGiayUyQuyenPrompt(fields, GetField),
            DocumentType.GiayMoi => BuildGiayMoiPrompt(fields, GetField),
            DocumentType.GiayGioiThieu => BuildGiayGioiThieuPrompt(fields, GetField),
            DocumentType.GiayNghiPhep => BuildGiayNghiPhepPrompt(fields, GetField),
            DocumentType.PhieuGui or DocumentType.PhieuChuyen or DocumentType.PhieuBao => BuildPhieuPrompt(templateType, fields, GetField),
            DocumentType.ThuCong => BuildThuCongPrompt(fields, GetField),
            _ => BuildGenericPrompt(fields, GetField)
        };
    }

    private string BuildCongVanPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        var hasReplyTo = !string.IsNullOrEmpty(get("reply_to_number"));
        var hasProposal = !string.IsNullOrEmpty(get("proposal"));
        
        var prompt = $@"Soạn CÔNG VĂN hoàn chỉnh, đúng thể thức NĐ 30/2020:

THÔNG TIN:
- Cơ quan ban hành: {get("from_org")}
- Nơi nhận chính: {(string.IsNullOrEmpty(get("to_org")) ? get("to_department") : get("to_org"))}
- Vấn đề (V/v): {get("subject")}
- Người ký: {(string.IsNullOrEmpty(get("signer_name")) ? get("chairman_name") : get("signer_name"))}
- Chức danh: {get("signer_title")}

NỘI DUNG CHÍNH CẦN ĐƯA VÀO:
{get("content")}";

        if (hasReplyTo)
            prompt += $"\n\nĐÂY LÀ CÔNG VĂN TRẢ LỜI công văn số: {get("reply_to_number")}. Mở đầu bằng: \"Phúc đáp Công văn số... ngày... của... về việc..., {CoQuanThuong} xin trả lời như sau:\"";

        if (hasProposal)
            prompt += $"\n\nĐỀ XUẤT/KIẾN NGHỊ:\n{get("proposal")}";

        prompt += @"

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ (đầy đủ)
2. Tên cơ quan cấp trên + cơ quan ban hành
3. Số/ký hiệu: Số:    /CV-UBND
4. Địa danh, ngày tháng
5. V/v: [trích yếu]
6. Kính gửi: [nơi nhận]
7. Thân văn bản:
   - Câu dẫn nhập (lý do, căn cứ)
   - Nội dung chính (diễn giải chi tiết, có số liệu nếu cần)
   - Đề nghị/kiến nghị (nêu rõ yêu cầu cụ thể)
   - Câu kết (""Kính đề nghị... xem xét, giải quyết./."")
8. Nơi nhận (liệt kê đầy đủ)
9. Chức danh + tên người ký

VĂN PHONG: Hành chính chuẩn, trang trọng, mạch lạc. Viện dẫn căn cứ pháp lý phù hợp nếu có.";

        return prompt;
    }

    private string BuildQuyetDinhPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        var templateName = _templateName ?? "";
        
        var prompt = $@"Soạn QUYẾT ĐỊNH hoàn chỉnh, đúng thể thức NĐ 30/2020:

LOẠI QUYẾT ĐỊNH: {templateName}

THÔNG TIN:";

        // Dynamic based on what fields are available
        if (!string.IsNullOrEmpty(get("person_name")))
            prompt += $"\n- Đối tượng: {get("person_name")}";
        if (!string.IsNullOrEmpty(get("current_position")))
            prompt += $"\n- Chức vụ hiện tại: {get("current_position")}";
        if (!string.IsNullOrEmpty(get("from_unit")))
            prompt += $"\n- Đơn vị cũ: {get("from_unit")}";
        if (!string.IsNullOrEmpty(get("to_unit")))
            prompt += $"\n- Đơn vị mới: {get("to_unit")}";
        if (!string.IsNullOrEmpty(get("new_position")))
            prompt += $"\n- Chức vụ mới: {get("new_position")}";
        if (!string.IsNullOrEmpty(get("effective_date")))
            prompt += $"\n- Ngày hiệu lực: {get("effective_date")}";
        if (!string.IsNullOrEmpty(get("award_type")))
            prompt += $"\n- Hình thức khen thưởng: {get("award_type")}";
        if (!string.IsNullOrEmpty(get("recipient")))
            prompt += $"\n- Đối tượng khen thưởng: {get("recipient")}";
        if (!string.IsNullOrEmpty(get("achievement")))
            prompt += $"\n- Thành tích: {get("achievement")}";
        if (!string.IsNullOrEmpty(get("org_name")))
            prompt += $"\n- Tên tổ chức: {get("org_name")}";
        if (!string.IsNullOrEmpty(get("members")))
            prompt += $"\n- Thành viên: {get("members")}";
        if (!string.IsNullOrEmpty(get("tasks")))
            prompt += $"\n- Nhiệm vụ: {get("tasks")}";
        if (!string.IsNullOrEmpty(get("project_name")))
            prompt += $"\n- Tên đề án/dự án: {get("project_name")}";
        if (!string.IsNullOrEmpty(get("objectives")))
            prompt += $"\n- Mục tiêu/Nội dung: {get("objectives")}";
        if (!string.IsNullOrEmpty(get("budget")))
            prompt += $"\n- Kinh phí: {get("budget")}";
        if (!string.IsNullOrEmpty(get("implementing_unit")))
            prompt += $"\n- Đơn vị thực hiện: {get("implementing_unit")}";

        var signerName = string.IsNullOrEmpty(get("signer_name")) ? get("chairman_name") : get("signer_name");
        var signerTitle = get("signer_title");
        if (!string.IsNullOrEmpty(signerName))
            prompt += $"\n- Người ký: {signerName}";
        if (!string.IsNullOrEmpty(signerTitle))
            prompt += $"\n- Chức danh: {signerTitle}";

        prompt += @"

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan ban hành" + TenCoQuanQuyetDinh + @"
3. Số/ký hiệu: Số:    /QĐ-UBND
4. Tiêu đề: QUYẾT ĐỊNH + ""Về việc [nội dung]""
5. Phần CĂN CỨ (bắt buộc):
   - Căn cứ Luật Tổ chức chính quyền địa phương 2015 (sửa đổi 2019)
   - Căn cứ các luật/nghị định chuyên ngành liên quan
   - Căn cứ tờ trình, đề nghị (nếu có)
   - Xét đề nghị của... (nếu có)
6. QUYẾT ĐỊNH:
   - Điều 1: Nội dung chính (chi tiết, cụ thể)
   - Điều 2: Trách nhiệm thi hành, hiệu lực
   - Điều 3: Tổ chức thực hiện, nơi gửi
7. Nơi nhận
8. Chức danh + tên người ký

LƯU Ý: Viện dẫn đúng căn cứ pháp lý. Mỗi Điều phải rõ ràng, cụ thể, có tính bắt buộc thi hành.";

        return prompt;
    }

    private string BuildBaoCaoPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        var templateName = _templateName ?? "";
        
        return $@"Soạn BÁO CÁO hoàn chỉnh, đúng thể thức NĐ 30/2020:

LOẠI BÁO CÁO: {templateName}

THÔNG TIN:
- Đơn vị báo cáo: {get("org_name")}
- Kỳ báo cáo/Lĩnh vực: {(string.IsNullOrEmpty(get("period")) ? get("field") : get("period"))}
- Người ký: {get("signer_name")}, {get("signer_title")}

DỮ LIỆU ĐẦU VÀO:
{(string.IsNullOrEmpty(get("achievements")) ? "" : $"KẾT QUẢ ĐẠT ĐƯỢC:\n{get("achievements")}\n")}
{(string.IsNullOrEmpty(get("situation")) ? "" : $"TÌNH HÌNH:\n{get("situation")}\n")}
{(string.IsNullOrEmpty(get("results")) ? "" : $"KẾT QUẢ CỤ THỂ:\n{get("results")}\n")}
{(string.IsNullOrEmpty(get("challenges")) ? "" : $"TỒN TẠI, HẠN CHẾ:\n{get("challenges")}\n")}
{(string.IsNullOrEmpty(get("future_plans")) ? "" : $"PHƯƠNG HƯỚNG:\n{get("future_plans")}\n")}
{(string.IsNullOrEmpty(get("proposals")) ? "" : $"ĐỀ XUẤT, KIẾN NGHỊ:\n{get("proposals")}\n")}

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan: {get("org_name")}
3. Số/ký hiệu: Số:    /BC-UBND
4. Tiêu đề: BÁO CÁO + trích yếu nội dung
5. Nơi gửi (Kính gửi)
6. Câu dẫn nhập: ""Thực hiện [căn cứ]..., {get("org_name")} báo cáo kết quả... như sau:""
7. Thân báo cáo:
   PHẦN I: KẾT QUẢ THỰC HIỆN
   - Chia theo mục, có đánh số I, II, III hoặc 1, 2, 3
   - Mỗi mục: trình bày cụ thể, có số liệu
   - Dùng gạch đầu dòng cho chi tiết

   PHẦN II: TỒN TẠI, HẠN CHẾ VÀ NGUYÊN NHÂN
   - Nêu rõ khó khăn, vướng mắc
   - Phân tích nguyên nhân (chủ quan/khách quan)

   PHẦN III: PHƯƠNG HƯỚNG, NHIỆM VỤ [kỳ tiếp]
   - Nhiệm vụ trọng tâm
   - Giải pháp thực hiện
   - Kiến nghị cấp trên (nếu cần)

8. Câu kết: ""Trên đây là báo cáo... Kính đề nghị [cấp trên] xem xét, chỉ đạo./.""
9. Nơi nhận
10. Chức danh + tên người ký

LƯU Ý: Số liệu phải cụ thể, rõ ràng. Đánh giá khách quan, trung thực. Kiến nghị phải khả thi, sát thực tế.";
    }

    private string BuildToTrinhPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn TỜ TRÌNH hoàn chỉnh, đúng thể thức NĐ 30/2020:

THÔNG TIN:
- Cơ quan trình: {get("org_name")}
- Nơi nhận: {get("recipient")}
- Vấn đề trình: {(string.IsNullOrEmpty(get("subject")) ? get("proposal") : get("subject"))}
- Lý do: {get("reason")}
- Nội dung đề xuất: {(string.IsNullOrEmpty(get("content")) ? get("proposal") : get("content"))}
{(string.IsNullOrEmpty(get("budget")) ? "" : $"- Kinh phí: {get("budget")}")}
- Người ký: {get("signer_name")}, {get("signer_title")}

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan: {get("org_name")}
3. Số/ký hiệu: Số:    /TTr-UBND
4. Tiêu đề: TỜ TRÌNH + ""Về việc [nội dung]""
5. Kính gửi: {get("recipient")}
6. Thân tờ trình:
   I. CĂN CỨ, LÝ DO:
   - Căn cứ pháp lý (Luật, NĐ, QĐ liên quan)
   - Tình hình thực tế, sự cần thiết

   II. NỘI DUNG ĐỀ XUẤT:
   - Nội dung cụ thể, chi tiết
   - Phương án thực hiện
   - Nguồn lực cần thiết
   {(string.IsNullOrEmpty(get("budget")) ? "" : "- Kinh phí dự kiến (ghi rõ nguồn vốn)")}

   III. TỔ CHỨC THỰC HIỆN:
   - Phân công trách nhiệm
   - Tiến độ dự kiến

7. Câu kết: ""Kính trình {get("recipient")} xem xét, phê duyệt./.""
8. Nơi nhận
9. Chức danh + tên người ký

LƯU Ý: Lập luận chặt chẽ, viện dẫn căn cứ đầy đủ. Nội dung phải thuyết phục, khả thi.";
    }

    private string BuildKeHoachPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        var isEvent = !string.IsNullOrEmpty(get("event_name")) || !string.IsNullOrEmpty(get("program"));
        var title = isEvent ? get("event_name") : $"Công tác {get("period")}";
        
        return $@"Soạn KẾ HOẠCH hoàn chỉnh, đúng thể thức NĐ 30/2020:

THÔNG TIN:
- Cơ quan ban hành: {(string.IsNullOrEmpty(get("org_name")) ? CoQuanThuong : get("org_name"))}
- Nội dung: {title}
{(string.IsNullOrEmpty(get("period")) ? "" : $"- Thời kỳ: {get("period")}")}
{(string.IsNullOrEmpty(get("objectives")) ? "" : $"- Mục tiêu: {get("objectives")}")}
{(string.IsNullOrEmpty(get("tasks")) ? "" : $"- Nội dung nhiệm vụ: {get("tasks")}")}
{(string.IsNullOrEmpty(get("time_place")) ? "" : $"- Thời gian, địa điểm: {get("time_place")}")}
{(string.IsNullOrEmpty(get("purpose")) ? "" : $"- Mục đích: {get("purpose")}")}
{(string.IsNullOrEmpty(get("program")) ? "" : $"- Chương trình: {get("program")}")}
{(string.IsNullOrEmpty(get("budget")) ? "" : $"- Kinh phí: {get("budget")}")}
- Người ký: {get("signer_name")}, {get("signer_title")}

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan ban hành
3. Số/ký hiệu: Số:    /KH-UBND
4. Tiêu đề: KẾ HOẠCH + trích yếu
5. Thân kế hoạch:

   I. MỤC ĐÍCH, YÊU CẦU:
   1. Mục đích (rõ ràng, cụ thể)
   2. Yêu cầu (khả thi, sát thực tế)

   II. NỘI DUNG:
   - Chi tiết từng nhiệm vụ/hoạt động
   - Thời gian thực hiện cụ thể
   - Đơn vị/cá nhân chịu trách nhiệm

   III. TỔ CHỨC THỰC HIỆN:
   - Phân công nhiệm vụ cho từng bộ phận
   - Chế độ báo cáo, kiểm tra

   IV. KINH PHÍ THỰC HIỆN:
   - Tổng kinh phí, nguồn vốn
   - Phân bổ (nếu có)

6. Câu kết: ""Yêu cầu các ban ngành, đoàn thể nghiêm túc triển khai thực hiện./.""
7. Nơi nhận
8. Chức danh + tên người ký

LƯU Ý: Nội dung phải cụ thể, có mốc thời gian, có phân công rõ ràng. Tránh chung chung.";
    }

    private string BuildThongBaoPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        var templateName = _templateName ?? "";
        
        return $@"Soạn THÔNG BÁO hoàn chỉnh, đúng thể thức NĐ 30/2020:

LOẠI THÔNG BÁO: {templateName}

THÔNG TIN:
{(string.IsNullOrEmpty(get("meeting_name")) ? "" : $"- Nội dung: {get("meeting_name")}")}
{(string.IsNullOrEmpty(get("event_name")) ? "" : $"- Sự kiện: {get("event_name")}")}
{(string.IsNullOrEmpty(get("time")) ? "" : $"- Thời gian: {get("time")}")}
{(string.IsNullOrEmpty(get("location")) ? "" : $"- Địa điểm: {get("location")}")}
{(string.IsNullOrEmpty(get("participants")) ? "" : $"- Thành phần: {get("participants")}")}
{(string.IsNullOrEmpty(get("agenda")) ? "" : $"- Nội dung/Chương trình: {get("agenda")}")}
{(string.IsNullOrEmpty(get("content")) ? "" : $"- Nội dung: {get("content")}")}
{(string.IsNullOrEmpty(get("conclusion")) ? "" : $"- Kết luận: {get("conclusion")}")}
{(string.IsNullOrEmpty(get("tasks")) ? "" : $"- Nhiệm vụ: {get("tasks")}")}
- Người ký: {get("signer_name")}, {get("signer_title")}

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan: {CoQuanHoa}
3. Số/ký hiệu: Số:    /TB-UBND
4. Tiêu đề: THÔNG BÁO + trích yếu
5. Thân thông báo:
   - Câu dẫn (lý do, căn cứ)
   - Nội dung chính (rõ ràng, cụ thể)
   - Thời gian, địa điểm (nếu có)
   - Thành phần, đối tượng
   - Yêu cầu (chuẩn bị, lưu ý)
6. Câu kết
7. Nơi nhận
8. Chức danh + tên người ký

LƯU Ý: Thông báo cần ngắn gọn, đầy đủ thông tin, dễ hiểu.";
    }

    private string BuildNghiQuyetPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        var level = get("level");
        var isHDND = _templateName?.Contains("HĐND") == true;
        
        return $@"Soạn NGHỊ QUYẾT hoàn chỉnh, đúng thể thức NĐ 30/2020:

LOẠI: Nghị quyết {(isHDND ? "HĐND" : "UBND")} cấp {level}

THÔNG TIN:
- Chủ đề: {get("subject")}
- Nội dung các điều: {get("articles")}
{(string.IsNullOrEmpty(get("effective_date")) ? "" : $"- Ngày hiệu lực: {get("effective_date")}")}
{(string.IsNullOrEmpty(get("implementing_unit")) ? "" : $"- Đơn vị thực hiện: {get("implementing_unit")}")}
- Chủ tịch: {get("chairman_name")}

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan: {(isHDND ? "HỘI ĐỒNG NHÂN DÂN" : "ỦY BAN NHÂN DÂN")} {TenXaHoa}
3. Số/ký hiệu: Số:    /NQ-{(isHDND ? "HĐND" : "UBND")}
4. Tiêu đề: NGHỊ QUYẾT + ""Về việc [nội dung]""
5. Phần CĂN CỨ (bắt buộc, quan trọng):
   - Căn cứ Luật Tổ chức chính quyền địa phương 2015 (sửa đổi 2019)
   {(isHDND ? "- Căn cứ Luật Hoạt động giám sát của Quốc hội và HĐND 2015" : "")}
   - Căn cứ các luật/nghị định chuyên ngành liên quan
   - Căn cứ tờ trình của UBND (nếu là NQ HĐND)
   - Xét [tình hình thực tế/đề nghị...]
6. {(isHDND ? $"HỘI ĐỒNG NHÂN DÂN {TenXaHoa} QUYẾT NGHỊ:" : "QUYẾT NGHỊ:")}
   - Các điều khoản (Điều 1, 2, 3...)
   - Điều cuối: Giao trách nhiệm tổ chức thực hiện
7. Nơi nhận
8. CHỦ TỊCH + tên

LƯU Ý: Nghị quyết phải có tính pháp lý cao, viện dẫn căn cứ đầy đủ, nội dung chặt chẽ, từng điều khoản rõ ràng.";
    }

    // === 22 PROMPT BUILDERS BỔ SUNG — NĐ 30/2020 ===

    private string BuildChiThiPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn CHỈ THỊ hoàn chỉnh, đúng thể thức NĐ 30/2020:

THÔNG TIN:
- Cơ quan: {get("from_org")}
- Vấn đề: {get("subject")}
- Nội dung chỉ đạo: {get("content")}
- Người ký: {get("signer_name")}, Chức danh: {get("signer_title")}

CẤU TRÚC CHỈ THỊ:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan, Số/CT-UBND
3. Tiêu đề: CHỈ THỊ + ""Về việc...""
4. Phần mở đầu: nêu tình hình, lý do ban hành
5. Nội dung chỉ đạo (đánh số 1, 2, 3...)
6. Yêu cầu thực hiện
7. Nơi nhận + Chữ ký";
    }

    private string BuildQuyChEPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn QUY CHẾ hoàn chỉnh:
- Tên quy chế: {get("subject")}
- Nội dung: {get("content")}

CẤU TRÚC: Chương I (Quy định chung: phạm vi, đối tượng), Chương II (Nội dung cụ thể), Chương III (Tổ chức thực hiện).
Lưu ý: Quy chế thường ban hành kèm theo Quyết định.";
    }

    private string BuildQuyDinhPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn QUY ĐỊNH hoàn chỉnh:
- Tên quy định: {get("subject")}
- Nội dung: {get("content")}

CẤU TRÚC: Chương I (Quy định chung), Chương II (Quy định cụ thể), Chương III (Tổ chức thực hiện).";
    }

    private string BuildThongCaoPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn THÔNG CÁO hoàn chỉnh:
- Vấn đề: {get("subject")}
- Nội dung: {get("content")}

THÔNG CÁO thường ngắn gọn, thông tin chính thức đến công chúng. Văn phong rõ ràng, khách quan.";
    }

    private string BuildHuongDanPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn HƯỚNG DẪN hoàn chỉnh, đúng thể thức NĐ 30/2020:

- Vấn đề: {get("subject")}
- Nội dung: {get("content")}
- Cơ quan: {get("from_org")}

CẤU TRÚC: I. Mục đích, yêu cầu; II. Nội dung hướng dẫn (chi tiết từng bước); III. Tổ chức thực hiện.";
    }

    private string BuildChuongTrinhPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn CHƯƠNG TRÌNH hoàn chỉnh:
- Tên: {get("subject")}
- Nội dung: {get("content")}
- Thời gian: {get("timeline")}

CẤU TRÚC: I. Mục đích, yêu cầu; II. Nội dung (liệt kê hoạt động, thời gian, người chịu trách nhiệm); III. Tổ chức thực hiện.";
    }

    private string BuildDeAnDuAnPrompt(DocumentType type, Dictionary<string, string> fields, Func<string, string> get)
    {
        var typeName = type.GetDisplayName().ToUpper();
        return $@"Soạn {typeName} hoàn chỉnh:
- Tên: {get("subject")}
- Mục tiêu: {get("objectives")}
- Nội dung: {get("content")}
- Kinh phí: {get("budget")}

CẤU TRÚC: I. Sự cần thiết; II. Mục tiêu; III. Nội dung; IV. Giải pháp; V. Kinh phí; VI. Tổ chức thực hiện.";
    }

    private string BuildBienBanPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn BIÊN BẢN hoàn chỉnh:
- Cuộc họp/Làm việc: {get("subject")}
- Thời gian: {get("time")}
- Địa điểm: {get("location")}
- Thành phần: {get("attendees")}
- Nội dung: {get("content")}

CẤU TRÚC BIÊN BẢN: Thời gian, Địa điểm, Thành phần (Chủ trì, Tham dự, Thư ký), Nội dung, Kết luận.
Biên bản KHÔNG đánh số ký hiệu. Có CHỦ TRÌ và THƯ KÝ ký.";
    }

    private string BuildHopDongPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn HỢP ĐỒNG hoàn chỉnh:
- Tên: {get("subject")}
- Bên A: {get("party_a")}
- Bên B: {get("party_b")}
- Nội dung: {get("content")}

CẤU TRÚC: Căn cứ pháp lý, Thông tin các bên, Điều 1 (Nội dung), Điều 2 (Thời gian), Điều 3 (Giá trị), Điều 4 (Quyền/Nghĩa vụ), Điều 5 (Điều khoản chung).";
    }

    private string BuildCongDienPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn CÔNG ĐIỆN hoàn chỉnh, khẩn cấp:
- Vấn đề: {get("subject")}
- Nơi nhận: {get("to_org")}
- Nội dung: {get("content")}

CÔNG ĐIỆN phải ngắn gọn, khẩn trương. Mở đầu: ""[CƠ QUAN] ĐIỆN:"" + nơi nhận. Kết thúc yêu cầu khẩn trương thực hiện.";
    }

    private string BuildThoaThuanPrompt(DocumentType type, Dictionary<string, string> fields, Func<string, string> get)
    {
        var typeName = type.GetDisplayName().ToUpper();
        return $@"Soạn {typeName} hoàn chỉnh:
- Vấn đề: {get("subject")}
- Nội dung: {get("content")}
- Các bên: {get("parties")}

CẤU TRÚC: Thông tin các bên, Nội dung thỏa thuận, Cam kết thực hiện. {typeName} KHÔNG đánh số ký hiệu.";
    }

    private string BuildGiayUyQuyenPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn GIẤY ỦY QUYỀN hoàn chỉnh:
- Người ủy quyền: {get("grantor")}
- Người được ủy quyền: {get("grantee")}
- Nội dung: {get("content")}
- Thời hạn: {get("duration")}

Theo Điều 13 NĐ 30/2020: Người được ủy quyền KHÔNG được ủy quyền lại.";
    }

    private string BuildGiayMoiPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn GIẤY MỜI hoàn chỉnh:
- Nội dung mời: {get("subject")}
- Thời gian: {get("time")}
- Địa điểm: {get("location")}
- Người nhận: {get("to_org")}

Giấy mời ngắn gọn, lịch sự, đầy đủ thông tin (nội dung, thời gian, địa điểm, thành phần tham dự).";
    }

    private string BuildGiayGioiThieuPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn GIẤY GIỚI THIỆU hoàn chỉnh:
- Người được giới thiệu: {get("person")}
- Đến cơ quan: {get("to_org")}
- Nội dung: {get("content")}

Giấy giới thiệu phải ghi rõ: Họ tên, chức vụ, đơn vị; Nơi đến; Mục đích; Thời hạn giá trị.";
    }

    private string BuildGiayNghiPhepPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn GIẤY NGHỈ PHÉP hoàn chỉnh:
- Người xin nghỉ: {get("person")}
- Từ ngày: {get("from_date")}
- Đến ngày: {get("to_date")}
- Lý do: {get("reason")}

Giấy nghỉ phép ghi rõ: Họ tên, chức vụ, đơn vị; Thời gian nghỉ; Lý do; Địa chỉ liên lạc.";
    }

    private string BuildPhieuPrompt(DocumentType type, Dictionary<string, string> fields, Func<string, string> get)
    {
        var typeName = type.GetDisplayName().ToUpper();
        return $@"Soạn {typeName} hoàn chỉnh:
- Nơi nhận: {get("to_org")}
- Nội dung: {get("content")}
- Văn bản kèm theo: {get("documents")}

{typeName} là văn bản nghiệp vụ văn thư, ngắn gọn, rõ ràng.";
    }

    private string BuildThuCongPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        return $@"Soạn THƯ CÔNG hoàn chỉnh:
- Loại thư: {get("letter_type")} (chúc mừng / cảm ơn / chia buồn / thăm hỏi)
- Nơi nhận: {get("to_org")}
- Nội dung: {get("content")}

Thư công phải trang trọng, lịch sự, thể hiện tình cảm chân thành. Ký tên và đóng dấu.";
    }

    private string BuildGenericPrompt(Dictionary<string, string> fields, Func<string, string> get)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Soạn văn bản hành chính loại {_templateName ?? "Công văn"} hoàn chỉnh, đúng thể thức NĐ 30/2020:");
        sb.AppendLine();
        sb.AppendLine("THÔNG TIN:");
        foreach (var kvp in fields)
        {
            if (!string.IsNullOrWhiteSpace(kvp.Value))
                sb.AppendLine($"- {GetFieldLabel(kvp.Key)}: {kvp.Value}");
        }
        sb.AppendLine();
        sb.AppendLine("YÊU CẦU: Soạn văn bản đầy đủ thể thức (quốc hiệu, tiêu ngữ, số/ký hiệu, nội dung, nơi nhận, chữ ký). Văn phong hành chính chuẩn, trang trọng.");
        return sb.ToString();
    }

    public static string GetFieldLabel(string field)
    {
        return field switch
        {
            // === Thông tin cơ quan ===
            "from_org" => "🏢 Cơ quan ban hành",
            "to_org" => "📨 Cơ quan nhận",
            "to_department" => "🏛️ Sở/Ban/Ngành nhận",
            "org_name" => "🏛️ Tên cơ quan/tổ chức",
            "from_unit" => "🏢 Đơn vị cũ",
            "to_unit" => "🏢 Đơn vị mới/nhận",
            "copy_org" => "🏢 Cơ quan sao lục",
            "implementing_unit" => "⚙️ Đơn vị thực hiện",
            
            // === Nội dung văn bản ===
            "subject" => "📋 Vấn đề/Tiêu đề",
            "content" => "📝 Nội dung chính",
            "reason" => "💡 Lý do",
            "purpose" => "🎯 Mục đích",
            "proposal" => "📊 Đề xuất",
            "proposals" => "💡 Đề xuất, kiến nghị",
            "objectives" => "🎯 Mục tiêu",
            "tasks" => "📋 Nhiệm vụ",
            "articles" => "📜 Các điều khoản",
            "program" => "📜 Chương trình",
            "agenda" => "📋 Nội dung cuộc họp",
            "conclusion" => "✅ Kết luận",
            "legal_basis" => "📖 Căn cứ pháp lý",
            
            // === Người ký / Người liên quan ===
            "signer_name" => "✍️ Người ký",
            "signer_title" => "👔 Chức danh người ký",
            "signer" => "✍️ Người ký",
            "chairman_name" => "👨‍💼 Chủ tịch",
            "principal_name" => "👨‍💼 Hiệu trưởng",
            "person_name" => "👤 Họ tên cán bộ",
            "person" => "👤 Họ tên",
            "citizen_name" => "👤 Họ tên công dân",
            "patient_name" => "🧑‍⚕️ Họ tên bệnh nhân",
            "student_name" => "🎓 Họ tên học sinh",
            "recipient" => "📬 Đơn vị/Người nhận",
            "recipients" => "📬 Nơi nhận",
            "grantor" => "👤 Người ủy quyền",
            "grantee" => "👤 Người được ủy quyền",
            "participants" => "👥 Thành phần tham dự",
            "members" => "👥 Danh sách thành viên",
            "students" => "🎓 Danh sách học sinh",
            "beneficiaries" => "👥 Đối tượng thụ hưởng",
            
            // === Chức vụ / Vị trí ===
            "current_position" => "💼 Chức vụ hiện tại",
            "new_position" => "⭐ Chức vụ mới",
            "level" => "🏛️ Cấp (Tỉnh/Huyện/Xã)",
            "ranking" => "🏅 Xếp loại",
            
            // === Thời gian / Địa điểm ===
            "time" => "⏰ Thời gian",
            "time_place" => "⏰ Thời gian, địa điểm",
            "location" => "📍 Địa điểm",
            "address" => "📍 Địa chỉ",
            "effective_date" => "📅 Ngày hiệu lực",
            "period" => "📆 Kỳ báo cáo/kế hoạch",
            "from_date" => "📅 Từ ngày",
            "to_date" => "📅 Đến ngày",
            "birth_date" => "📅 Ngày sinh",
            "exam_date" => "📅 Ngày khám",
            "meeting_time" => "⏰ Thời gian họp",
            "graduation_year" => "📅 Năm tốt nghiệp",
            "school_year" => "📅 Năm học",
            "year" => "📅 Năm",
            
            // === Khen thưởng / Kỷ luật ===
            "award_type" => "🏆 Hình thức khen thưởng",
            "achievement" => "✨ Thành tích",
            "achievements" => "✅ Kết quả đạt được",
            "reward_type" => "🏆 Hình thức khen thưởng",
            "reward_proposal" => "🏆 Đề nghị khen thưởng",
            "collective_achievements" => "✅ Thành tích tập thể",
            "violation" => "⚠️ Hành vi vi phạm",
            "penalty" => "⚖️ Hình thức xử phạt",
            "discipline_type" => "⚠️ Hình thức kỷ luật",
            
            // === Báo cáo / Đánh giá ===
            "situation" => "📊 Tình hình",
            "results" => "📈 Kết quả",
            "result" => "📈 Kết quả",
            "challenges" => "⚠️ Tồn tại, hạn chế",
            "future_plans" => "🚀 Phương hướng tiếp theo",
            "next_plan" => "🚀 Kế hoạch tiếp theo",
            "evaluation" => "⭐ Đánh giá",
            "task_name" => "📌 Nhiệm vụ/Kế hoạch",
            "field" => "📂 Lĩnh vực",
            "solutions" => "💡 Giải pháp thực hiện",
            "targets" => "🎯 Chỉ tiêu",
            "implementation" => "⚙️ Tổ chức thực hiện",
            "year_targets" => "🎯 Chỉ tiêu năm",
            "criteria_status" => "📊 Tình trạng các tiêu chí",
            
            // === Sự kiện / Hội nghị ===
            "event_name" => "🎉 Tên sự kiện",
            "meeting_name" => "🤝 Tên cuộc họp",
            "reply_to_number" => "🔢 Trả lời công văn số",
            
            // === Dự án / Tài chính ===
            "project_name" => "🎯 Tên đề án/dự án",
            "budget" => "💰 Kinh phí",
            "support_type" => "📋 Hình thức hỗ trợ",
            "support_amount" => "💰 Mức hỗ trợ",
            
            // === Trường học ===
            "school_name" => "🏫 Tên trường",
            "grade" => "📚 Khối/Lớp",
            "class_name" => "📚 Tên lớp",
            "curriculum_plan" => "📖 Chương trình dạy học",
            "student_count" => "👥 Số lượng học sinh",
            "quality_stats" => "📊 Thống kê chất lượng",
            
            // === Y tế ===
            "medical_unit" => "🏥 Cơ sở y tế",
            "hospital" => "🏥 Bệnh viện",
            "from_hospital" => "🏥 Bệnh viện chuyển",
            "to_hospital" => "🏥 Bệnh viện nhận",
            "disease_name" => "🩺 Tên bệnh/dịch",
            "diagnosis" => "🩺 Chẩn đoán",
            "transfer_reason" => "📋 Lý do chuyển viện",
            "prevention_measures" => "🛡️ Biện pháp phòng chống",
            "measures" => "📋 Biện pháp thực hiện",
            "statistics" => "📊 Số liệu thống kê",
            "patient_count" => "👥 Số lượng bệnh nhân",
            "clinical_results" => "📈 Kết quả lâm sàng",
            "treatment_plan" => "📋 Phác đồ điều trị",
            "procedure_name" => "📋 Tên quy trình",
            "test_type" => "🔬 Loại xét nghiệm",
            "test_result" => "📈 Kết quả xét nghiệm",
            "area" => "📍 Khu vực/Địa bàn",
            
            // === Hành chính xã/phường ===
            "ward_name" => "🏘️ Tên xã/phường",
            "marital_status" => "👪 Tình trạng hôn nhân",
            "population" => "👥 Dân số",
            "birth_death_rate" => "📊 Tỷ lệ sinh/tử",
            "economy" => "📈 Kinh tế",
            "social" => "🏘️ Xã hội",
            "disaster_type" => "⚠️ Loại thiên tai",
            "risk_areas" => "📍 Vùng có nguy cơ",
            "rescue_forces" => "🚑 Lực lượng cứu hộ",
            "evacuation_plan" => "🗺️ Phương án sơ tán",
            "reform_content" => "📋 Nội dung cải cách",
            "procedures" => "📋 Thủ tục",
            
            // === Sao lục / Phụ lục ===
            "original_document" => "📄 Văn bản gốc",
            "original_saoy" => "📄 Bản gốc sao y",
            "extract_section" => "📋 Phần trích sao",
            "document_ref" => "📎 Số hiệu văn bản",
            "documents" => "📎 Danh sách văn bản",
            "parent_document" => "📄 Văn bản chính",
            "appendix_title" => "📋 Tiêu đề phụ lục",
            "appendix_number" => "🔢 Số thứ tự phụ lục",
            
            // === Hợp đồng / Biên bản ===
            "party_a" => "🤝 Bên A",
            "party_b" => "🤝 Bên B",
            "attendees" => "👥 Thành phần tham dự",
            "timeline" => "⏰ Thời gian thực hiện",
            "instructions" => "📝 Ý kiến chỉ đạo",
            "letter_type" => "📋 Loại thư",
            "duration" => "⏱️ Thời hạn",
            
            // === Y tế bổ sung ===
            "patient_age" => "📅 Tuổi bệnh nhân",
            "patient_gender" => "⚧ Giới tính bệnh nhân",
            "medical_summary" => "📋 Tóm tắt bệnh án",
            "birth_year" => "📅 Năm sinh",
            "scope" => "📋 Phạm vi áp dụng",
            "discussion" => "💬 Ý kiến thảo luận",
            "recommendation" => "💡 Khuyến nghị",
            "recommendations" => "💡 Kiến nghị, đề xuất",
            
            // === Giáo dục bổ sung ===
            "assessment" => "📝 Kiểm tra đánh giá",
            "improvement_plan" => "📋 Kế hoạch cải thiện",
            "class_info" => "📚 Khối/Lớp liên quan",
            "notes" => "📝 Ghi chú, lưu ý",
            "teacher_achievements" => "👩‍🏫 Thành tích giáo viên",
            "student_achievements" => "🎓 Thành tích học sinh",
            
            // === UBND xã bổ sung ===
            "id_number" => "🆔 Số CCCD/CMND",
            "resources" => "📦 Phương tiện, vật tư",
            "funding_source" => "💰 Nguồn kinh phí",
            "funding" => "💰 Nguồn vốn",
            "assignment" => "👥 Phân công thực hiện",
            "receiving_point" => "📍 Nơi tiếp nhận",
            "contact" => "📞 Liên hệ",
            "family_planning_results" => "📊 Kết quả KHHGĐ",
            
            _ => $"📝 {FormatFieldName(field)}"
        };
    }
    
    /// <summary>
    /// Chuyển field name kỹ thuật thành tên thân thiện (fallback)
    /// VD: "school_name" → "Tên trường", "from_date" → "Từ ngày"
    /// </summary>
    public static string FormatFieldName(string field)
    {
        // Thay _ thành khoảng trắng, viết hoa chữ đầu
        var words = field.Split('_');
        return string.Join(" ", words.Select(w => 
            w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w));
    }
}
