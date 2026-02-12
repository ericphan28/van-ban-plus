using System.Text.Json.Serialization;

namespace AIVanBan.Core.Models;

/// <summary>
/// Mức độ nghiêm trọng của vấn đề
/// </summary>
public enum IssueSeverity
{
    /// <summary>🔴 Nghiêm trọng - phải sửa trước khi ban hành</summary>
    Critical,
    /// <summary>🟡 Cần xem xét - nên sửa để hoàn thiện</summary>
    Warning,
    /// <summary>🟢 Gợi ý - tùy chọn cải thiện</summary>
    Suggestion
}

/// <summary>
/// Loại vấn đề
/// </summary>
public enum IssueCategory
{
    /// <summary>Lỗi chính tả</summary>
    Spelling,
    /// <summary>Văn phong hành chính</summary>
    Style,
    /// <summary>Xung đột nội dung</summary>
    Conflict,
    /// <summary>Logic không hợp lý</summary>
    Logic,
    /// <summary>Thiếu thành phần</summary>
    Missing,
    /// <summary>Nội dung mơ hồ</summary>
    Ambiguous,
    /// <summary>Đề xuất bổ sung</summary>
    Enhancement
}

/// <summary>
/// Một vấn đề cụ thể trong văn bản
/// </summary>
public class ReviewIssue
{
    [JsonPropertyName("severity")]
    public string Severity { get; set; } = "warning"; // critical, warning, suggestion

    [JsonPropertyName("category")]
    public string Category { get; set; } = ""; // spelling, style, conflict, logic, missing, ambiguous, enhancement

    [JsonPropertyName("location")]
    public string Location { get; set; } = ""; // Vị trí (VD: "Điều 2, Khoản 1")

    [JsonPropertyName("original_text")]
    public string OriginalText { get; set; } = ""; // Đoạn text gốc có vấn đề

    [JsonPropertyName("description")]
    public string Description { get; set; } = ""; // Mô tả vấn đề

    [JsonPropertyName("suggestion")]
    public string Suggestion { get; set; } = ""; // Đề xuất sửa/cải thiện

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = ""; // Lý do / căn cứ

    // Helper properties (không serialize)
    [JsonIgnore]
    public IssueSeverity SeverityEnum => Severity?.ToLower() switch
    {
        "critical" => IssueSeverity.Critical,
        "warning" => IssueSeverity.Warning,
        "suggestion" => IssueSeverity.Suggestion,
        _ => IssueSeverity.Warning
    };

    [JsonIgnore]
    public IssueCategory CategoryEnum => Category?.ToLower() switch
    {
        "spelling" => IssueCategory.Spelling,
        "style" => IssueCategory.Style,
        "conflict" => IssueCategory.Conflict,
        "logic" => IssueCategory.Logic,
        "missing" => IssueCategory.Missing,
        "ambiguous" => IssueCategory.Ambiguous,
        "enhancement" => IssueCategory.Enhancement,
        _ => IssueCategory.Enhancement
    };

    [JsonIgnore]
    public string SeverityIcon => SeverityEnum switch
    {
        IssueSeverity.Critical => "🔴",
        IssueSeverity.Warning => "🟡",
        IssueSeverity.Suggestion => "🟢",
        _ => "⚪"
    };

    [JsonIgnore]
    public string CategoryIcon => CategoryEnum switch
    {
        IssueCategory.Spelling => "🔤",
        IssueCategory.Style => "✍️",
        IssueCategory.Conflict => "⚡",
        IssueCategory.Logic => "🔗",
        IssueCategory.Missing => "📋",
        IssueCategory.Ambiguous => "❓",
        IssueCategory.Enhancement => "💡",
        _ => "📌"
    };

    [JsonIgnore]
    public string CategoryName => CategoryEnum switch
    {
        IssueCategory.Spelling => "Chính tả",
        IssueCategory.Style => "Văn phong",
        IssueCategory.Conflict => "Xung đột nội dung",
        IssueCategory.Logic => "Logic",
        IssueCategory.Missing => "Thiếu thành phần",
        IssueCategory.Ambiguous => "Nội dung mơ hồ",
        IssueCategory.Enhancement => "Đề xuất cải thiện",
        _ => "Khác"
    };

    [JsonIgnore]
    public string SeverityName => SeverityEnum switch
    {
        IssueSeverity.Critical => "Nghiêm trọng",
        IssueSeverity.Warning => "Cần xem xét",
        IssueSeverity.Suggestion => "Gợi ý",
        _ => "Khác"
    };
}

/// <summary>
/// Kết quả tổng thể kiểm tra văn bản
/// </summary>
public class DocumentReviewResult
{
    [JsonPropertyName("overall_score")]
    public int OverallScore { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = "";

    [JsonPropertyName("issues")]
    public List<ReviewIssue> Issues { get; set; } = new();

    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = new();

    [JsonPropertyName("suggested_content")]
    public string SuggestedContent { get; set; } = "";

    // Computed properties
    [JsonIgnore]
    public int CriticalCount => Issues.Count(i => i.SeverityEnum == IssueSeverity.Critical);
    
    [JsonIgnore]
    public int WarningCount => Issues.Count(i => i.SeverityEnum == IssueSeverity.Warning);
    
    [JsonIgnore]
    public int SuggestionCount => Issues.Count(i => i.SeverityEnum == IssueSeverity.Suggestion);

    [JsonIgnore]
    public string ScoreColor => OverallScore switch
    {
        >= 8 => "#4CAF50", // Xanh lá
        >= 6 => "#FF9800", // Cam
        >= 4 => "#FF5722", // Đỏ cam
        _ => "#D32F2F"     // Đỏ
    };

    [JsonIgnore]
    public string ScoreText => OverallScore switch
    {
        >= 9 => "Xuất sắc",
        >= 8 => "Tốt",
        >= 6 => "Khá — cần sửa một số lỗi",
        >= 4 => "Trung bình — cần chỉnh sửa nhiều",
        _ => "Yếu — cần soạn lại"
    };
}
