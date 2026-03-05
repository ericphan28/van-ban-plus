namespace AIVanBan.Core.Models;

/// <summary>
/// Cuộc họp - Quản lý toàn bộ dữ liệu liên quan đến cuộc họp hành chính
/// Thiết kế dành cho cán bộ nhà nước Việt Nam
/// </summary>
public class Meeting
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // === THÔNG TIN CƠ BẢN ===
    
    /// <summary>Tên cuộc họp (VD: "Họp UBND thường kỳ tháng 2/2026")</summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>Số giấy mời / Số cuộc họp (VD: "15/GM-UBND")</summary>
    public string MeetingNumber { get; set; } = string.Empty;
    
    /// <summary>Loại cuộc họp</summary>
    public MeetingType Type { get; set; } = MeetingType.HopCoQuan;
    
    /// <summary>Cấp cuộc họp</summary>
    public MeetingLevel Level { get; set; } = MeetingLevel.CapDonVi;
    
    /// <summary>Trạng thái cuộc họp</summary>
    public MeetingStatus Status { get; set; } = MeetingStatus.Scheduled;
    
    /// <summary>Mức độ ưu tiên (1=Thấp → 5=Rất cao)</summary>
    public int Priority { get; set; } = 3;
    
    // === THỜI GIAN ===
    
    /// <summary>Thời gian bắt đầu</summary>
    public DateTime StartTime { get; set; } = DateTime.Today.AddHours(8);
    
    /// <summary>Thời gian kết thúc (null nếu chưa xác định)</summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>Cuộc họp cả ngày (không cần giờ cụ thể)</summary>
    public bool IsAllDay { get; set; } = false;
    
    // === ĐỊA ĐIỂM ===
    
    /// <summary>Địa điểm họp (VD: "Phòng họp số 1, UBND xã Hòa Bình")</summary>
    public string Location { get; set; } = string.Empty;
    
    /// <summary>Hình thức họp</summary>
    public MeetingFormat Format { get; set; } = MeetingFormat.TrucTiep;
    
    /// <summary>Link họp trực tuyến (Zoom, Teams, Google Meet...)</summary>
    public string OnlineLink { get; set; } = string.Empty;
    
    // === NGƯỜI THAM DỰ ===
    
    /// <summary>Người chủ trì (họ tên)</summary>
    public string ChairPerson { get; set; } = string.Empty;
    
    /// <summary>Chức vụ người chủ trì</summary>
    public string ChairPersonTitle { get; set; } = string.Empty;
    
    /// <summary>Thư ký cuộc họp</summary>
    public string Secretary { get; set; } = string.Empty;
    
    /// <summary>Cơ quan/đơn vị tổ chức</summary>
    public string OrganizingUnit { get; set; } = string.Empty;
    
    /// <summary>Danh sách thành phần tham dự</summary>
    public List<MeetingAttendee> Attendees { get; set; } = new();
    
    // === NỘI DUNG ===
    
    /// <summary>Chương trình / Nội dung dự kiến</summary>
    public string Agenda { get; set; } = string.Empty;
    
    /// <summary>Nội dung chi tiết cuộc họp (ghi chép)</summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>Kết luận cuộc họp</summary>
    public string Conclusion { get; set; } = string.Empty;
    
    /// <summary>Ghi chú cá nhân (chỉ mình xem)</summary>
    public string PersonalNotes { get; set; } = string.Empty;
    
    // === NHIỆM VỤ ĐƯỢC GIAO TỪ CUỘC HỌP ===
    
    /// <summary>Danh sách nhiệm vụ/công việc được giao</summary>
    public List<MeetingTask> Tasks { get; set; } = new();
    
    // === TÀI LIỆU CUỘC HỌP ===
    
    /// <summary>Danh sách tài liệu/văn bản liên quan đến cuộc họp (giấy mời, tài liệu họp, biên bản, kết luận...)</summary>
    public List<MeetingDocument> Documents { get; set; } = new();
    
    /// <summary>ID album ảnh liên quan (link đến module Album Ảnh)</summary>
    public string[] RelatedAlbumIds { get; set; } = Array.Empty<string>();
    
    /// <summary>Đường dẫn file đính kèm bổ sung (slide, tài liệu rời...)</summary>
    public string[] AttachmentPaths { get; set; } = Array.Empty<string>();
    
    // Legacy fields - backward compatible
    /// <summary>[Legacy] ID Giấy mời họp (trong module Văn bản)</summary>
    public string InvitationDocId { get; set; } = string.Empty;
    /// <summary>[Legacy] ID Biên bản họp</summary>
    public string MinutesDocId { get; set; } = string.Empty;
    /// <summary>[Legacy] ID Thông báo kết luận</summary>
    public string ConclusionDocId { get; set; } = string.Empty;
    /// <summary>[Legacy] ID văn bản liên quan</summary>
    public string[] RelatedDocumentIds { get; set; } = Array.Empty<string>();
    
    // === PHÂN LOẠI ===
    
    /// <summary>Tags / Nhãn phân loại</summary>
    public string[] Tags { get; set; } = Array.Empty<string>();
    
    // === AUDIT ===
    
    public string CreatedBy { get; set; } = Environment.UserName;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    
    // === MẪU CUỘC HỌP (Meeting Template) ===
    
    /// <summary>Đánh dấu đây là mẫu cuộc họp (không phải cuộc họp thực)</summary>
    public bool IsTemplate { get; set; } = false;
    
    /// <summary>Tên mẫu (VD: "Họp giao ban tuần", "Họp UBND thường kỳ")</summary>
    public string TemplateName { get; set; } = string.Empty;
    
    // === NHẮC NHỞ ===
    
    /// <summary>Nhắc nhở trước bao nhiêu phút (0 = không nhắc, mặc định 15 phút)</summary>
    public int ReminderMinutesBefore { get; set; } = 15;
    
    /// <summary>Đã hiển thị nhắc nhở cho lần gần nhất chưa (tránh nhắc lặp)</summary>
    public bool ReminderShown { get; set; } = false;
}

/// <summary>
/// Thành phần tham dự cuộc họp
/// </summary>
public class MeetingAttendee
{
    /// <summary>Họ và tên</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Chức vụ</summary>
    public string Position { get; set; } = string.Empty;
    
    /// <summary>Đơn vị / Phòng ban</summary>
    public string Unit { get; set; } = string.Empty;
    
    /// <summary>Số điện thoại</summary>
    public string Phone { get; set; } = string.Empty;
    
    /// <summary>Vai trò trong cuộc họp</summary>
    public AttendeeRole Role { get; set; } = AttendeeRole.Attendee;
    
    /// <summary>Tình trạng tham dự</summary>
    public AttendanceStatus AttendanceStatus { get; set; } = AttendanceStatus.Invited;
    
    /// <summary>Ghi chú (lý do vắng, người thay thế...)</summary>
    public string Note { get; set; } = string.Empty;
}

/// <summary>
/// Nhiệm vụ được giao từ cuộc họp (kết luận cuộc họp thường giao việc)
/// </summary>
public class MeetingTask
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>Nội dung nhiệm vụ</summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>Mô tả chi tiết</summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>Người được giao (họ tên)</summary>
    public string AssignedTo { get; set; } = string.Empty;
    
    /// <summary>Đơn vị thực hiện</summary>
    public string AssignedUnit { get; set; } = string.Empty;
    
    /// <summary>Hạn hoàn thành</summary>
    public DateTime? Deadline { get; set; }
    
    /// <summary>Trạng thái thực hiện</summary>
    public MeetingTaskStatus TaskStatus { get; set; } = MeetingTaskStatus.NotStarted;
    
    /// <summary>Ngày hoàn thành thực tế</summary>
    public DateTime? CompletionDate { get; set; }
    
    /// <summary>Ghi chú / Kết quả thực hiện</summary>
    public string Notes { get; set; } = string.Empty;
    
    /// <summary>Mức độ ưu tiên (1-5)</summary>
    public int Priority { get; set; } = 3;
}

/// <summary>
/// Tài liệu/Văn bản liên quan đến cuộc họp
/// Mỗi cuộc họp có nhiều loại tài liệu: giấy mời, tài liệu họp, biên bản, kết luận...
/// </summary>
public class MeetingDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>Loại tài liệu</summary>
    public MeetingDocumentType DocumentType { get; set; }
    
    /// <summary>Tên/Trích yếu tài liệu</summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>Số hiệu văn bản (VD: 15/GM-UBND, 20/BB-UBND)</summary>
    public string DocumentNumber { get; set; } = string.Empty;
    
    /// <summary>Ngày ban hành / Ngày ký</summary>
    public DateTime? IssuedDate { get; set; }
    
    /// <summary>Cơ quan ban hành</summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>Đường dẫn file (nếu có file trên máy)</summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>ID liên kết đến văn bản trong module Quản lý Văn bản (nếu đã nhập vào hệ thống)</summary>
    public string LinkedDocumentId { get; set; } = string.Empty;
    
    /// <summary>Ghi chú</summary>
    public string Note { get; set; } = string.Empty;
}

// ============================================================
// ENUMS
// ============================================================

/// <summary>
/// Loại cuộc họp - bao quát tất cả loại họp phổ biến tại Việt Nam
/// </summary>
public enum MeetingType
{
    // --- Họp định kỳ ---
    HopThuongKy,        // Họp thường kỳ (UBND, Đảng ủy, cơ quan...)
    HopGiaoBan,         // Họp giao ban (tuần, tháng)
    HopChuyenDe,        // Họp chuyên đề
    
    // --- Họp đánh giá ---
    HopSoKet,           // Họp sơ kết (6 tháng, quý)
    HopTongKet,         // Họp tổng kết (năm)
    HopKiemDiem,        // Họp kiểm điểm, đánh giá
    
    // --- Họp triển khai ---
    HopTrienKhai,       // Họp triển khai (nghị quyết, kế hoạch, dự án)
    HopBanChiDao,       // Họp Ban chỉ đạo
    
    // --- Hội nghị / Hội thảo ---
    HoiNghi,            // Hội nghị
    HoiThao,            // Hội thảo / Tọa đàm
    TapHuan,            // Tập huấn / Bồi dưỡng nghiệp vụ
    
    // --- Họp Đảng ---
    HopChiBo,           // Họp Chi bộ Đảng (hàng tháng)
    HopDangUy,          // Họp Đảng ủy / Ban Thường vụ
    
    // --- Họp cơ quan ---
    HopHDND,            // Họp HĐND (kỳ họp)
    HopCoQuan,          // Họp cơ quan / Đơn vị
    HopLienNganh,       // Họp liên ngành / Liên cơ quan
    HopDotXuat,         // Họp đột xuất / Khẩn cấp
    
    // --- Tiếp dân ---
    TiepCongDan,        // Tiếp công dân / Tiếp dân định kỳ
    
    // --- Sự kiện ---
    LeTruyenThong,      // Lễ kỷ niệm / Ngày truyền thống
    GiaoLuu,            // Giao lưu / Gặp mặt
    
    // --- Khác ---
    Khac                // Loại khác
}

/// <summary>
/// Trạng thái cuộc họp
/// </summary>
public enum MeetingStatus
{
    Scheduled,      // Đã lên lịch (chưa họp)
    InProgress,     // Đang diễn ra
    Completed,      // Đã kết thúc
    Postponed,      // Hoãn
    Cancelled       // Hủy
}

/// <summary>
/// Hình thức họp
/// </summary>
public enum MeetingFormat
{
    TrucTiep,       // Trực tiếp (tại phòng họp)
    TrucTuyen,      // Trực tuyến (Zoom, Teams...)
    KetHop          // Kết hợp (hybrid)
}

/// <summary>
/// Cấp cuộc họp
/// </summary>
public enum MeetingLevel
{
    CapDonVi,       // Cấp đơn vị / Cơ quan
    CapXa,          // Cấp xã / Phường / Thị trấn
    CapTinh,        // Cấp tỉnh / Thành phố
    CapTrungUong,   // Cấp trung ương
    LienNganh       // Liên ngành / Liên cơ quan
}

/// <summary>
/// Vai trò trong cuộc họp
/// </summary>
public enum AttendeeRole
{
    ChairPerson,    // Chủ trì
    Secretary,      // Thư ký
    Presenter,      // Báo cáo viên / Trình bày
    Attendee,       // Thành viên tham dự
    Observer,       // Dự thính / Quan sát
    Invitee         // Được mời (chưa xác nhận)
}

/// <summary>
/// Tình trạng tham dự
/// </summary>
public enum AttendanceStatus
{
    Invited,                // Đã mời (chưa xác nhận)
    Confirmed,              // Đã xác nhận tham dự
    Attended,               // Có mặt
    Absent,                 // Vắng mặt (không phép)
    AbsentWithPermission,   // Vắng mặt có phép
    Delegated               // Ủy quyền người khác dự
}

/// <summary>
/// Trạng thái nhiệm vụ từ cuộc họp
/// </summary>
public enum MeetingTaskStatus
{
    NotStarted,     // Chưa thực hiện
    InProgress,     // Đang thực hiện
    Completed,      // Đã hoàn thành
    Overdue,        // Quá hạn
    Cancelled       // Hủy
}

/// <summary>
/// Loại tài liệu/văn bản cuộc họp - theo quy trình họp tại Việt Nam
/// </summary>
public enum MeetingDocumentType
{
    GiayMoi,            // 📋 Giấy mời họp (bắt buộc)
    ChuongTrinh,        // 📑 Chương trình / Lịch trình cuộc họp
    TaiLieuHop,         // 📄 Tài liệu họp (báo cáo, tờ trình, đề án, dự thảo, kế hoạch...)
    BienBan,            // 📝 Biên bản cuộc họp
    ThongBaoKetLuan,    // 📌 Thông báo kết luận cuộc họp
    NghiQuyet,          // 📜 Nghị quyết
    VanBanChiDao,       // 📂 Văn bản chỉ đạo liên quan
    QuyetDinh,          // ⚖️ Quyết định liên quan
    CongVan,            // ✉️ Công văn liên quan
    Khac                // 📎 Tài liệu khác
}

/// <summary>
/// Helper class chuyển đổi enum sang tên hiển thị tiếng Việt
/// </summary>
public static class MeetingHelper
{
    public static string GetTypeName(MeetingType type) => type switch
    {
        MeetingType.HopThuongKy => "Họp thường kỳ",
        MeetingType.HopGiaoBan => "Họp giao ban",
        MeetingType.HopChuyenDe => "Họp chuyên đề",
        MeetingType.HopSoKet => "Họp sơ kết",
        MeetingType.HopTongKet => "Họp tổng kết",
        MeetingType.HopKiemDiem => "Họp kiểm điểm",
        MeetingType.HopTrienKhai => "Họp triển khai",
        MeetingType.HopBanChiDao => "Họp Ban chỉ đạo",
        MeetingType.HoiNghi => "Hội nghị",
        MeetingType.HoiThao => "Hội thảo / Tọa đàm",
        MeetingType.TapHuan => "Tập huấn",
        MeetingType.HopChiBo => "Họp Chi bộ",
        MeetingType.HopDangUy => "Họp Đảng ủy",
        MeetingType.HopHDND => "Họp HĐND",
        MeetingType.HopCoQuan => "Họp cơ quan",
        MeetingType.HopLienNganh => "Họp liên ngành",
        MeetingType.HopDotXuat => "Họp đột xuất",
        MeetingType.TiepCongDan => "Tiếp công dân",
        MeetingType.LeTruyenThong => "Lễ kỷ niệm",
        MeetingType.GiaoLuu => "Giao lưu / Gặp mặt",
        MeetingType.Khac => "Khác",
        _ => type.ToString()
    };

    public static string GetStatusName(MeetingStatus status) => status switch
    {
        MeetingStatus.Scheduled => "Đã lên lịch",
        MeetingStatus.InProgress => "Đang diễn ra",
        MeetingStatus.Completed => "Đã kết thúc",
        MeetingStatus.Postponed => "Hoãn",
        MeetingStatus.Cancelled => "Hủy",
        _ => status.ToString()
    };

    public static string GetStatusIcon(MeetingStatus status) => status switch
    {
        MeetingStatus.Scheduled => "📅",
        MeetingStatus.InProgress => "🔴",
        MeetingStatus.Completed => "✅",
        MeetingStatus.Postponed => "⏸️",
        MeetingStatus.Cancelled => "❌",
        _ => "📋"
    };
    
    public static string GetStatusColor(MeetingStatus status) => status switch
    {
        MeetingStatus.Scheduled => "#1976D2",    // Blue
        MeetingStatus.InProgress => "#E53935",   // Red
        MeetingStatus.Completed => "#43A047",    // Green
        MeetingStatus.Postponed => "#FB8C00",    // Orange
        MeetingStatus.Cancelled => "#757575",    // Gray
        _ => "#1976D2"
    };

    public static string GetFormatName(MeetingFormat format) => format switch
    {
        MeetingFormat.TrucTiep => "Trực tiếp",
        MeetingFormat.TrucTuyen => "Trực tuyến",
        MeetingFormat.KetHop => "Kết hợp (hybrid)",
        _ => format.ToString()
    };

    public static string GetFormatIcon(MeetingFormat format) => format switch
    {
        MeetingFormat.TrucTiep => "🏢",
        MeetingFormat.TrucTuyen => "💻",
        MeetingFormat.KetHop => "🔄",
        _ => "📋"
    };

    public static string GetLevelName(MeetingLevel level) => level switch
    {
        MeetingLevel.CapDonVi => "Cấp đơn vị",
        MeetingLevel.CapXa => "Cấp xã/phường",
        MeetingLevel.CapTinh => "Cấp tỉnh/TP",
        MeetingLevel.CapTrungUong => "Cấp trung ương",
        MeetingLevel.LienNganh => "Liên ngành",
        _ => level.ToString()
    };

    public static string GetTaskStatusName(MeetingTaskStatus status) => status switch
    {
        MeetingTaskStatus.NotStarted => "Chưa thực hiện",
        MeetingTaskStatus.InProgress => "Đang thực hiện",
        MeetingTaskStatus.Completed => "Đã hoàn thành",
        MeetingTaskStatus.Overdue => "Quá hạn",
        MeetingTaskStatus.Cancelled => "Hủy",
        _ => status.ToString()
    };
    
    public static string GetTaskStatusColor(MeetingTaskStatus status) => status switch
    {
        MeetingTaskStatus.NotStarted => "#757575",   // Gray
        MeetingTaskStatus.InProgress => "#1976D2",   // Blue
        MeetingTaskStatus.Completed => "#43A047",    // Green
        MeetingTaskStatus.Overdue => "#E53935",      // Red
        MeetingTaskStatus.Cancelled => "#9E9E9E",    // Light gray
        _ => "#757575"
    };

    public static string GetAttendanceStatusName(AttendanceStatus status) => status switch
    {
        AttendanceStatus.Invited => "Đã mời",
        AttendanceStatus.Confirmed => "Xác nhận",
        AttendanceStatus.Attended => "Có mặt",
        AttendanceStatus.Absent => "Vắng mặt",
        AttendanceStatus.AbsentWithPermission => "Vắng có phép",
        AttendanceStatus.Delegated => "Ủy quyền",
        _ => status.ToString()
    };

    // === Meeting Document Type Helpers ===
    
    public static string GetDocumentTypeName(MeetingDocumentType type) => type switch
    {
        MeetingDocumentType.GiayMoi => "Giấy mời họp",
        MeetingDocumentType.ChuongTrinh => "Chương trình họp",
        MeetingDocumentType.TaiLieuHop => "Tài liệu họp",
        MeetingDocumentType.BienBan => "Biên bản cuộc họp",
        MeetingDocumentType.ThongBaoKetLuan => "Thông báo kết luận",
        MeetingDocumentType.NghiQuyet => "Nghị quyết",
        MeetingDocumentType.VanBanChiDao => "Văn bản chỉ đạo",
        MeetingDocumentType.QuyetDinh => "Quyết định",
        MeetingDocumentType.CongVan => "Công văn",
        MeetingDocumentType.Khac => "Tài liệu khác",
        _ => type.ToString()
    };
    
    public static string GetDocumentTypeIcon(MeetingDocumentType type) => type switch
    {
        MeetingDocumentType.GiayMoi => "📋",
        MeetingDocumentType.ChuongTrinh => "📑",
        MeetingDocumentType.TaiLieuHop => "📄",
        MeetingDocumentType.BienBan => "📝",
        MeetingDocumentType.ThongBaoKetLuan => "📌",
        MeetingDocumentType.NghiQuyet => "📜",
        MeetingDocumentType.VanBanChiDao => "📂",
        MeetingDocumentType.QuyetDinh => "⚖️",
        MeetingDocumentType.CongVan => "✉️",
        MeetingDocumentType.Khac => "📎",
        _ => "📄"
    };
    
    public static string GetDocumentTypeColor(MeetingDocumentType type) => type switch
    {
        MeetingDocumentType.GiayMoi => "#E53935",        // Red - bắt buộc
        MeetingDocumentType.ChuongTrinh => "#1976D2",    // Blue
        MeetingDocumentType.TaiLieuHop => "#7B1FA2",     // Purple
        MeetingDocumentType.BienBan => "#00695C",         // Teal
        MeetingDocumentType.ThongBaoKetLuan => "#E65100", // Orange
        MeetingDocumentType.NghiQuyet => "#283593",       // Indigo
        MeetingDocumentType.VanBanChiDao => "#4527A0",    // Deep Purple
        MeetingDocumentType.QuyetDinh => "#1565C0",       // Blue
        MeetingDocumentType.CongVan => "#2E7D32",         // Green
        MeetingDocumentType.Khac => "#757575",             // Gray
        _ => "#757575"
    };

    public static string GetPriorityText(int priority) => priority switch
    {
        1 => "⬜ Thấp",
        2 => "🟦 Bình thường",
        3 => "🟨 Trung bình",
        4 => "🟧 Cao",
        5 => "🟥 Rất cao",
        _ => "🟨 Trung bình"
    };

    public static string GetPriorityColor(int priority) => priority switch
    {
        1 => "#9E9E9E",
        2 => "#42A5F5",
        3 => "#FFA726",
        4 => "#EF5350",
        5 => "#C62828",
        _ => "#FFA726"
    };

    /// <summary>
    /// Lấy tên ngày trong tuần bằng tiếng Việt
    /// </summary>
    public static string GetVietnameseDayOfWeek(DateTime date) => date.DayOfWeek switch
    {
        DayOfWeek.Monday => "Thứ Hai",
        DayOfWeek.Tuesday => "Thứ Ba",
        DayOfWeek.Wednesday => "Thứ Tư",
        DayOfWeek.Thursday => "Thứ Năm",
        DayOfWeek.Friday => "Thứ Sáu",
        DayOfWeek.Saturday => "Thứ Bảy",
        DayOfWeek.Sunday => "Chủ Nhật",
        _ => date.DayOfWeek.ToString()
    };

    /// <summary>
    /// Format khoảng thời gian họp
    /// </summary>
    public static string FormatTimeRange(DateTime start, DateTime? end)
    {
        var startStr = start.ToString("HH:mm");
        if (end.HasValue)
            return $"{startStr} - {end.Value.ToString("HH:mm")}";
        return startStr;
    }

    /// <summary>
    /// Format ngày họp với thứ trong tuần
    /// </summary>
    public static string FormatMeetingDate(DateTime date)
    {
        return $"{GetVietnameseDayOfWeek(date)}, {date:dd/MM/yyyy}";
    }
}
