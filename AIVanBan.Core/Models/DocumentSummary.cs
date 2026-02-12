using System.Text.Json.Serialization;

namespace AIVanBan.Core.Models;

/// <summary>
/// Kết quả tóm tắt văn bản bằng AI
/// </summary>
public class DocumentSummary
{
    /// <summary>Tóm tắt ngắn gọn (1-2 câu)</summary>
    [JsonPropertyName("brief")]
    public string Brief { get; set; } = string.Empty;

    /// <summary>Loại văn bản (xác định bởi AI)</summary>
    [JsonPropertyName("document_type")]
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>Cơ quan ban hành / Người ký</summary>
    [JsonPropertyName("issuing_authority")]
    public string IssuingAuthority { get; set; } = string.Empty;

    /// <summary>Đối tượng áp dụng / Nơi nhận chính</summary>
    [JsonPropertyName("target_audience")]
    public string TargetAudience { get; set; } = string.Empty;

    /// <summary>Các nội dung chính (phân tích theo Điều/Khoản/Mục)</summary>
    [JsonPropertyName("key_points")]
    public List<SummaryKeyPoint> KeyPoints { get; set; } = new();

    /// <summary>Căn cứ pháp lý được viện dẫn</summary>
    [JsonPropertyName("legal_bases")]
    public List<string> LegalBases { get; set; } = new();

    /// <summary>Các mốc thời gian / Hiệu lực</summary>
    [JsonPropertyName("effective_dates")]
    public List<string> EffectiveDates { get; set; } = new();

    /// <summary>Các con số / Chỉ tiêu quan trọng</summary>
    [JsonPropertyName("key_figures")]
    public List<string> KeyFigures { get; set; } = new();

    /// <summary>Tác động / Ý nghĩa thực tiễn</summary>
    [JsonPropertyName("impact")]
    public string Impact { get; set; } = string.Empty;

    /// <summary>Ghi chú / Lưu ý đặc biệt</summary>
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Một nội dung chính trong văn bản
/// </summary>
public class SummaryKeyPoint
{
    /// <summary>Tiêu đề mục (VD: "Điều 1", "Phần I", "Mục tiêu")</summary>
    [JsonPropertyName("heading")]
    public string Heading { get; set; } = string.Empty;

    /// <summary>Nội dung tóm tắt của mục</summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}
