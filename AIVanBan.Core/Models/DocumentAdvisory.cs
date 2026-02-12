using System.Text.Json.Serialization;

namespace AIVanBan.Core.Models;

/// <summary>
/// Kết quả tham mưu xử lý văn bản đến từ AI
/// </summary>
public class DocumentAdvisory
{
    /// <summary>Tóm tắt nội dung VB (3-5 câu)</summary>
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = "";

    /// <summary>Mức độ khẩn: thuong, khan, thuong_khan, hoa_toc</summary>
    [JsonPropertyName("urgency_level")]
    public string UrgencyLevel { get; set; } = "thuong";

    /// <summary>Các việc cần thực hiện — mỗi item ghi rõ bước + người thực hiện</summary>
    [JsonPropertyName("action_items")]
    public List<string> ActionItems { get; set; } = new();

    /// <summary>Các mốc deadline quan trọng</summary>
    [JsonPropertyName("deadlines")]
    public List<DeadlineItem> Deadlines { get; set; } = new();

    /// <summary>Đề xuất bộ phận/người xử lý chính</summary>
    [JsonPropertyName("suggested_handler")]
    public string SuggestedHandler { get; set; } = "";

    /// <summary>Các đơn vị cần phối hợp</summary>
    [JsonPropertyName("coordination")]
    public List<string> Coordination { get; set; } = new();

    /// <summary>Thẩm quyền ký: ai ký nháy, ai ký chính</summary>
    [JsonPropertyName("signing_authority")]
    public string SigningAuthority { get; set; } = "";

    /// <summary>Cần trả lời/phản hồi không?</summary>
    [JsonPropertyName("response_needed")]
    public bool ResponseNeeded { get; set; }

    /// <summary>Nếu cần trả lời: loại VB trả lời (Công văn, Báo cáo, Tờ trình...)</summary>
    [JsonPropertyName("response_type")]
    public string ResponseType { get; set; } = "";

    /// <summary>Dự thảo trả lời ngắn gọn (nếu có)</summary>
    [JsonPropertyName("draft_response_outline")]
    public string DraftResponseOutline { get; set; } = "";

    /// <summary>Các văn bản pháp lý được viện dẫn trong VB</summary>
    [JsonPropertyName("legal_references")]
    public List<string> LegalReferences { get; set; } = new();

    /// <summary>Cảnh báo rủi ro nếu chậm xử lý</summary>
    [JsonPropertyName("risk_warning")]
    public string RiskWarning { get; set; } = "";

    /// <summary>Mức độ ưu tiên: high, medium, low</summary>
    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "medium";

    /// <summary>Lĩnh vực liên quan</summary>
    [JsonPropertyName("related_field")]
    public string RelatedField { get; set; } = "";

    /// <summary>Phân loại VB đến (Chỉ đạo, Đề nghị, Thông báo, Mời họp...)</summary>
    [JsonPropertyName("incoming_type")]
    public string IncomingType { get; set; } = "";
}

/// <summary>
/// Một mốc deadline trong VB
/// </summary>
public class DeadlineItem
{
    [JsonPropertyName("task")]
    public string Task { get; set; } = "";

    [JsonPropertyName("date")]
    public string Date { get; set; } = "";
}
