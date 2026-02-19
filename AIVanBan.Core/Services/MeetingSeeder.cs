using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service tạo dữ liệu demo cuộc họp sát thực tế
/// Mô phỏng lịch họp của UBND xã Hòa Bình, thành phố Tương Dương, tỉnh Nghệ An
/// Bao gồm đầy đủ: giấy mời, tài liệu họp, biên bản, kết luận, nhiệm vụ, thành phần tham dự
/// </summary>
public class MeetingSeeder
{
    private readonly MeetingService _meetingService;
    
    // === CƠ CẤU TỔ CHỨC UBND XÃ HÒA BÌNH ===
    private const string OrgName = "UBND xã Hòa Bình";
    private const string OrgFull = "Ủy ban nhân dân xã Hòa Bình";
    private const string ThanhPhoName = "thành phố Tương Dương";
    private const string TinhName = "tỉnh Nghệ An";
    private const string DiaDanh = "Hòa Bình";
    
    // Ban lãnh đạo xã
    private const string ChuTich = "Lê Văn Thắng";
    private const string ChucVuChuTich = "Chủ tịch UBND xã";
    private const string PctVhXh = "Nguyễn Thị Hương";
    private const string ChucVuPctVhXh = "Phó Chủ tịch UBND xã phụ trách VH-XH";
    private const string PctKtHt = "Trần Đình Lâm";
    private const string ChucVuPctKtHt = "Phó Chủ tịch UBND xã phụ trách KT-HT";
    private const string BiThuDang = "Hoàng Minh Đức";
    private const string ChucVuBiThu = "Bí thư Đảng ủy xã";
    private const string ChuTichHdnd = "Phạm Thị Lan";
    private const string ChucVuCtHdnd = "Chủ tịch HĐND xã";
    private const string ChuTichUbMttq = "Lương Văn Tùng";
    private const string ChucVuCtMttq = "Chủ tịch UB MTTQ Việt Nam xã";
    
    // Cán bộ chuyên môn
    private const string CbVpUbnd = "Vi Thị Ngọc";
    private const string CbDiaChinh = "Lò Văn Tuấn";
    private const string CbTuPhap = "Nguyễn Đình Trung";
    private const string CbTaiChinh = "Hà Thị Mai";
    private const string CbVhXh = "Trần Thị Hồng";
    private const string CbLdTbXh = "Lương Văn Hải";
    private const string CaTruongCa = "Thiếu tá Nguyễn Văn Cường";
    private const string XaDoiTruong = "Đại úy Trần Văn Sơn";
    private const string TramTruong = "BS. Nguyễn Thị Thảo";
    
    // Đoàn thể
    private const string ChuTichHoiND = "Lô Văn Minh";
    private const string ChuTichHoiPN = "Vi Thị Lan";
    private const string BiThuDoanTn = "Nguyễn Văn Hoàng";
    private const string ChuTichHoiCcb = "Trần Văn Đức";
    private const string ChuTichHoiNct = "Lương Thị Tâm";
    
    // Trưởng thôn/bản
    private static readonly string[] TruongThon = new[]
    {
        "Lô Văn Thanh - Trưởng bản Na Hang",
        "Vi Văn Hoa - Trưởng bản Khe Bố",
        "Lương Văn Đông - Trưởng bản Bản Vẽ",
        "Hà Văn Sáng - Trưởng bản Na Loi",
        "Nguyễn Văn Phúc - Trưởng thôn Hòa Phong"
    };
    
    public MeetingSeeder(MeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    /// <summary>
    /// Tạo dữ liệu demo đầy đủ: 18 cuộc họp đa dạng
    /// Bao gồm cả quá khứ (đã kết thúc), hiện tại (sắp tới) và tương lai
    /// </summary>
    public void SeedDemoMeetings()
    {
        var existing = _meetingService.GetAllMeetings();
        if (existing.Count > 0)
        {
            Console.WriteLine($"✅ Đã có {existing.Count} cuộc họp. Bỏ qua seed.");
            return;
        }
        
        Console.WriteLine("📅 Đang tạo dữ liệu demo cuộc họp...");
        
        var meetings = new List<Meeting>
        {
            // === QUÁ KHỨ (ĐÃ HOÀN THÀNH) ===
            Create_HopThuongKyThang1(),
            Create_HopGiaoBanTuan3Thang1(),
            Create_HopChiBoDinhKy(),
            Create_HoiNghiTongKetNam(),
            Create_HopBanChiDaoNTM(),
            Create_TiepCongDanDinhKy(),
            Create_HopHDND_KyHop(),
            
            // === TUẦN NÀY / GẦN ĐÂY ===
            Create_HopGiaoBanTuanHienTai(),
            Create_HopChuyenDeGiaiPhongMatBang(),
            Create_HopLienNganhPhongChongThienTai(),
            
            // === SẮP TỚI ===
            Create_HopThuongKyThang2(),
            Create_HopDangUyDinhKy(),
            Create_TapHuanChuyenDoiSo(),
            Create_HopXetKhenThuong(),
            Create_HopTrienKhaiKeHoach(),
            
            // === TƯƠNG LAI XA HƠN ===
            Create_HoiNghiNhanDan(),
            Create_HopSoKet6Thang(),
        };
        
        foreach (var meeting in meetings)
        {
            _meetingService.AddMeeting(meeting);
            Console.WriteLine($"  ✓ {meeting.StartTime:dd/MM/yyyy HH:mm} - {meeting.Title}");
        }
        
        Console.WriteLine($"✅ Đã tạo {meetings.Count} cuộc họp demo thành công!");
    }
    
    // ===========================================================================
    // QUÁ KHỨ - ĐÃ HOÀN THÀNH
    // ===========================================================================
    
    /// <summary>1. Họp UBND thường kỳ tháng 1/2026</summary>
    private Meeting Create_HopThuongKyThang1()
    {
        return new Meeting
        {
            Title = "Họp UBND xã thường kỳ tháng 01/2026",
            MeetingNumber = "03/GM-UBND",
            Type = MeetingType.HopThuongKy,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Completed,
            Priority = 4,
            StartTime = new DateTime(2026, 1, 15, 8, 0, 0),
            EndTime = new DateTime(2026, 1, 15, 11, 30, 0),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbVpUbnd,
            OrganizingUnit = OrgFull,
            Attendees = new List<MeetingAttendee>
            {
                new() { Name = ChuTich, Position = ChucVuChuTich, Unit = OrgName, Role = AttendeeRole.ChairPerson, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = PctVhXh, Position = ChucVuPctVhXh, Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = PctKtHt, Position = ChucVuPctKtHt, Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbVpUbnd, Position = "VP-TK UBND xã", Unit = OrgName, Role = AttendeeRole.Secretary, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbDiaChinh, Position = "CB Địa chính - Xây dựng", Unit = OrgName, Role = AttendeeRole.Presenter, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbTuPhap, Position = "CB Tư pháp - Hộ tịch", Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbTaiChinh, Position = "CB Tài chính - Kế toán", Unit = OrgName, Role = AttendeeRole.Presenter, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbVhXh, Position = "CB Văn hóa - Xã hội", Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbLdTbXh, Position = "CB LĐ-TB&XH", Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CaTruongCa, Position = "Trưởng Công an xã", Unit = "Công an xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = XaDoiTruong, Position = "Xã đội trưởng", Unit = "Ban CHQS xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.AbsentWithPermission, Note = "Ủy quyền Phó Xã đội trưởng dự thay" },
            },
            Agenda = @"1. Đánh giá tình hình thực hiện nhiệm vụ tháng 01/2026
2. Báo cáo thu-chi ngân sách tháng 01
3. Tình hình quản lý đất đai, trật tự xây dựng
4. Công tác an ninh trật tự, phòng chống ma túy
5. Triển khai kế hoạch Tết Nguyên đán Bính Ngọ 2026
6. Các kiến nghị, đề xuất",
            Content = @"1. Đ/c Lê Văn Thắng - Chủ tịch UBND xã khai mạc, nêu mục đích cuộc họp.

2. Đ/c Hà Thị Mai - CB TC-KT báo cáo:
- Thu ngân sách tháng 01: đạt 45/500 triệu (9% KH năm)
- Chi thường xuyên: 38 triệu, đảm bảo đúng dự toán
- Tồn quỹ: 12 triệu

3. Đ/c Lò Văn Tuấn - CB ĐC-XD báo cáo:
- 02 hồ sơ cấp GCNQSDĐ đang xử lý
- 01 trường hợp xây dựng trái phép tại bản Na Hang đã lập biên bản
- Hoàn thành kiểm kê đất đai theo KH

4. Đ/c Nguyễn Văn Cường - CA xã báo cáo:
- Tình hình ANTT ổn định, không có vụ việc nghiêm trọng
- Phát hiện 01 vụ tàng trữ trái phép chất ma túy, đã xử lý
- Triển khai kế hoạch bảo vệ Tết Nguyên đán

5. Các thành viên UBND thảo luận, đóng góp ý kiến.",
            Conclusion = @"Chủ tịch UBND xã kết luận:

1. Giao CB TC-KT tham mưu phương án thu ngân sách quý I, phấn đấu hoàn thành 25% KH năm.
2. Giao CB ĐC-XD xử lý dứt điểm trường hợp xây dựng trái phép trước 31/01/2026.
3. Công an xã phối hợp với Ban CHQS tăng cường tuần tra bảo vệ Tết.
4. VP-TK tham mưu kế hoạch tổ chức các hoạt động đón Tết.
5. Yêu cầu các bộ phận nộp báo cáo tổng kết tháng trước ngày 25 hàng tháng.",
            PersonalNotes = "Cuộc họp diễn ra đúng giờ, nội dung đầy đủ. Lưu ý: đ/c Tuấn cần đẩy nhanh tiến độ xử lý vi phạm xây dựng.",
            Tasks = new List<MeetingTask>
            {
                new() { Title = "Tham mưu phương án thu ngân sách quý I/2026", AssignedTo = CbTaiChinh, AssignedUnit = "TC-KT", Deadline = new DateTime(2026, 1, 25), TaskStatus = MeetingTaskStatus.Completed, CompletionDate = new DateTime(2026, 1, 24), Priority = 4 },
                new() { Title = "Xử lý dứt điểm xây dựng trái phép bản Na Hang", AssignedTo = CbDiaChinh, AssignedUnit = "ĐC-XD", Deadline = new DateTime(2026, 1, 31), TaskStatus = MeetingTaskStatus.Completed, CompletionDate = new DateTime(2026, 1, 29), Priority = 5 },
                new() { Title = "Kế hoạch tuần tra bảo vệ Tết Nguyên đán", AssignedTo = CaTruongCa, AssignedUnit = "Công an xã", Deadline = new DateTime(2026, 1, 20), TaskStatus = MeetingTaskStatus.Completed, CompletionDate = new DateTime(2026, 1, 19), Priority = 5 },
                new() { Title = "Tham mưu kế hoạch tổ chức đón Tết", AssignedTo = CbVpUbnd, AssignedUnit = "VP-TK", Deadline = new DateTime(2026, 1, 22), TaskStatus = MeetingTaskStatus.Completed, CompletionDate = new DateTime(2026, 1, 21), Priority = 3 },
            },
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Giấy mời họp UBND xã thường kỳ tháng 01/2026", DocumentNumber = "03/GM-UBND", IssuedDate = new DateTime(2026, 1, 12), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.ChuongTrinh, Title = "Chương trình cuộc họp UBND xã thường kỳ tháng 01", DocumentNumber = "", IssuedDate = new DateTime(2026, 1, 12), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Báo cáo thu-chi ngân sách tháng 01/2026", DocumentNumber = "05/BC-UBND", IssuedDate = new DateTime(2026, 1, 14), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Báo cáo tình hình quản lý đất đai, xây dựng tháng 01", DocumentNumber = "06/BC-UBND", IssuedDate = new DateTime(2026, 1, 14), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Báo cáo tình hình an ninh trật tự tháng 01", DocumentNumber = "02/BC-CA", IssuedDate = new DateTime(2026, 1, 14), Issuer = "Công an xã" },
                new() { DocumentType = MeetingDocumentType.BienBan, Title = "Biên bản họp UBND xã thường kỳ tháng 01/2026", DocumentNumber = "03/BB-UBND", IssuedDate = new DateTime(2026, 1, 15), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.ThongBaoKetLuan, Title = "Thông báo kết luận họp UBND xã thường kỳ tháng 01", DocumentNumber = "08/TB-UBND", IssuedDate = new DateTime(2026, 1, 16), Issuer = OrgName },
            },
            Tags = new[] { "thường kỳ", "UBND", "tháng 01", "2026" }
        };
    }
    
    /// <summary>2. Họp giao ban tuần 3 tháng 1</summary>
    private Meeting Create_HopGiaoBanTuan3Thang1()
    {
        return new Meeting
        {
            Title = "Họp giao ban tuần (12-16/01/2026)",
            MeetingNumber = "",
            Type = MeetingType.HopGiaoBan,
            Level = MeetingLevel.CapDonVi,
            Status = MeetingStatus.Completed,
            Priority = 3,
            StartTime = new DateTime(2026, 1, 19, 7, 30, 0),
            EndTime = new DateTime(2026, 1, 19, 8, 30, 0),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbVpUbnd,
            OrganizingUnit = OrgName,
            Attendees = new List<MeetingAttendee>
            {
                new() { Name = ChuTich, Position = ChucVuChuTich, Unit = OrgName, Role = AttendeeRole.ChairPerson, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = PctVhXh, Position = ChucVuPctVhXh, Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = PctKtHt, Position = ChucVuPctKtHt, Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbVpUbnd, Position = "VP-TK", Unit = OrgName, Role = AttendeeRole.Secretary, AttendanceStatus = AttendanceStatus.Attended },
            },
            Agenda = "1. Tổng hợp công việc tuần qua\n2. Phân công công việc tuần tới\n3. Vấn đề phát sinh",
            Content = "Các đ/c PCT báo cáo tình hình công việc thuộc lĩnh vực phụ trách. Không có vấn đề phát sinh lớn.",
            Conclusion = "Tuần tới tập trung chuẩn bị Tết Nguyên đán, rà soát danh sách hộ nghèo cận nghèo cần hỗ trợ.",
            Tags = new[] { "giao ban", "tuần" },
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Giấy mời giao ban sáng thứ Hai", DocumentNumber = "", Issuer = OrgName, Note = "Thông báo qua nhóm Zalo cơ quan" },
            },
        };
    }
    
    /// <summary>3. Sinh hoạt Chi bộ tháng 1</summary>
    private Meeting Create_HopChiBoDinhKy()
    {
        return new Meeting
        {
            Title = "Sinh hoạt Chi bộ Cơ quan UBND xã tháng 01/2026",
            MeetingNumber = "01/GM-CB",
            Type = MeetingType.HopChiBo,
            Level = MeetingLevel.CapDonVi,
            Status = MeetingStatus.Completed,
            Priority = 4,
            StartTime = new DateTime(2026, 1, 8, 14, 0, 0),
            EndTime = new DateTime(2026, 1, 8, 16, 30, 0),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = BiThuDang,
            ChairPersonTitle = "Bí thư Chi bộ",
            Secretary = CbVpUbnd,
            OrganizingUnit = "Chi bộ Cơ quan UBND xã Hòa Bình",
            Attendees = new List<MeetingAttendee>
            {
                new() { Name = BiThuDang, Position = "Bí thư Chi bộ", Unit = "Chi bộ CQ", Role = AttendeeRole.ChairPerson, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = ChuTich, Position = "Phó Bí thư Chi bộ", Unit = "Chi bộ CQ", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = PctVhXh, Position = "Đảng viên", Unit = "Chi bộ CQ", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = PctKtHt, Position = "Đảng viên", Unit = "Chi bộ CQ", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbVpUbnd, Position = "Đảng viên", Unit = "Chi bộ CQ", Role = AttendeeRole.Secretary, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbDiaChinh, Position = "Đảng viên", Unit = "Chi bộ CQ", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbTuPhap, Position = "Đảng viên", Unit = "Chi bộ CQ", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbTaiChinh, Position = "Đảng viên", Unit = "Chi bộ CQ", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
            },
            Agenda = @"1. Thông tin thời sự, chỉ thị nghị quyết mới của Đảng
2. Đánh giá kết quả thực hiện nghị quyết Chi bộ tháng 12/2025
3. Kiểm điểm đảng viên vi phạm (nếu có)
4. Bàn phương hướng nhiệm vụ tháng 01/2026
5. Đóng Đảng phí, thu nộp quỹ",
            Content = @"1. Đ/c Bí thư thông tin: Nghị quyết số 18-NQ/TW về tinh gọn bộ máy; Chỉ thị Tết Nguyên đán 2026.

2. Đánh giá tháng 12/2025:
- Chi bộ hoàn thành tốt các chỉ tiêu NQ đề ra
- 8/8 đảng viên hoàn thành nhiệm vụ
- Tham gia đầy đủ các phong trào của Đảng ủy xã

3. Không có đảng viên vi phạm.

4. Phương hướng tháng 01/2026:
- Tổ chức tốt các hoạt động Tết Nguyên đán
- Thăm hỏi gia đình chính sách, hộ nghèo
- Tuyên truyền người dân đón Tết an toàn, tiết kiệm

5. Đảng phí: Thu đủ 8/8 đảng viên.",
            Conclusion = "Chi bộ thống nhất phương hướng tháng 01. Giao đ/c Ngọc tổng hợp danh sách gia đình chính sách cần thăm hỏi dịp Tết.",
            PersonalNotes = "Chi bộ sinh hoạt đầy đủ, đúng quy định. Cần chuẩn bị nội dung chuyên đề cho tháng tới.",
            Tasks = new List<MeetingTask>
            {
                new() { Title = "Tổng hợp danh sách gia đình chính sách cần thăm Tết", AssignedTo = CbVpUbnd, AssignedUnit = "VP-TK", Deadline = new DateTime(2026, 1, 12), TaskStatus = MeetingTaskStatus.Completed, CompletionDate = new DateTime(2026, 1, 11), Priority = 4 },
                new() { Title = "Chuẩn bị nội dung sinh hoạt chuyên đề tháng 2", AssignedTo = BiThuDang, AssignedUnit = "Chi bộ", Deadline = new DateTime(2026, 2, 1), TaskStatus = MeetingTaskStatus.Completed, CompletionDate = new DateTime(2026, 1, 28), Priority = 3 },
            },
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Giấy mời sinh hoạt Chi bộ tháng 01/2026", DocumentNumber = "01/GM-CB", IssuedDate = new DateTime(2026, 1, 5), Issuer = "Chi bộ CQ UBND xã" },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Nghị quyết 18-NQ/TW ngày 15/12/2025 (trích)", DocumentNumber = "18-NQ/TW", Issuer = "Ban Chấp hành Trung ương" },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Chỉ thị về tổ chức Tết Nguyên đán 2026", DocumentNumber = "25/CT-TU", Issuer = "Tỉnh ủy Nghệ An" },
                new() { DocumentType = MeetingDocumentType.BienBan, Title = "Biên bản sinh hoạt Chi bộ tháng 01/2026", DocumentNumber = "01/BB-CB", IssuedDate = new DateTime(2026, 1, 8), Issuer = "Chi bộ CQ" },
            },
            Tags = new[] { "chi bộ", "đảng", "sinh hoạt", "tháng 01" }
        };
    }
    
    /// <summary>4. Hội nghị tổng kết năm 2025</summary>
    private Meeting Create_HoiNghiTongKetNam()
    {
        return new Meeting
        {
            Title = "Hội nghị tổng kết công tác năm 2025 và phương hướng nhiệm vụ năm 2026",
            MeetingNumber = "05/GM-UBND",
            Type = MeetingType.HopTongKet,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Completed,
            Priority = 5,
            StartTime = new DateTime(2026, 1, 10, 8, 0, 0),
            EndTime = new DateTime(2026, 1, 10, 17, 0, 0),
            IsAllDay = true,
            Location = "Hội trường UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbVpUbnd,
            OrganizingUnit = OrgFull,
            Attendees = new List<MeetingAttendee>
            {
                new() { Name = ChuTich, Position = ChucVuChuTich, Unit = OrgName, Role = AttendeeRole.ChairPerson, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = PctVhXh, Position = ChucVuPctVhXh, Unit = OrgName, Role = AttendeeRole.Presenter, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = PctKtHt, Position = ChucVuPctKtHt, Unit = OrgName, Role = AttendeeRole.Presenter, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = BiThuDang, Position = ChucVuBiThu, Unit = "Đảng ủy xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = ChuTichHdnd, Position = ChucVuCtHdnd, Unit = "HĐND xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = ChuTichUbMttq, Position = ChucVuCtMttq, Unit = "UB MTTQ xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = "Nguyễn Đức Hà", Position = "Phó Chủ tịch UBND thành phố", Unit = "UBND thành phố Tương Dương", Role = AttendeeRole.Observer, AttendanceStatus = AttendanceStatus.Attended, Note = "Phát biểu chỉ đạo" },
                new() { Name = CbVpUbnd, Position = "VP-TK", Unit = OrgName, Role = AttendeeRole.Secretary, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbTaiChinh, Position = "CB TC-KT", Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = ChuTichHoiND, Position = "CT Hội Nông dân xã", Unit = "Hội Nông dân", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = ChuTichHoiPN, Position = "CT Hội LHPN xã", Unit = "Hội Phụ nữ", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = BiThuDoanTn, Position = "Bí thư Đoàn xã", Unit = "Đoàn TN", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = ChuTichHoiCcb, Position = "CT Hội CCB xã", Unit = "Hội CCB", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
            },
            Agenda = @"BUỔI SÁNG (8h00 - 11h30):
1. Khai mạc, giới thiệu đại biểu
2. Báo cáo tổng kết tình hình KT-XH, AN-QP năm 2025
3. Báo cáo thu-chi ngân sách năm 2025
4. Tham luận các bộ phận

BUỔI CHIỀU (13h30 - 17h00):
5. Phương hướng, nhiệm vụ trọng tâm năm 2026
6. Phát biểu chỉ đạo của lãnh đạo thành phố
7. Trao Giấy khen cho tập thể, cá nhân tiên tiến
8. Bế mạc",
            Content = @"Báo cáo chính: Năm 2025, xã Hòa Bình đạt được nhiều kết quả tích cực:
- Thu ngân sách: 520/500 triệu (104% KH), trong đó thuế sử dụng đất phi nông nghiệp, phí lệ phí đạt 98%
- Hoàn thành 19/19 tiêu chí NTM nâng cao
- Tỷ lệ hộ nghèo giảm còn 3,2% (giảm 1,5% so với 2024)
- Xây dựng mới 02 tuyến đường bê tông liên bản (3,5km)
- Trạm y tế đạt chuẩn quốc gia
- An ninh trật tự ổn định, không có điểm nóng",
            Conclusion = @"1. Năm 2026 phấn đấu thu ngân sách 550 triệu, tăng 6% so với 2025.
2. Tiếp tục duy trì và nâng cao các tiêu chí NTM.
3. Giảm tỷ lệ hộ nghèo xuống dưới 2%.
4. Hoàn thành tuyến đường bản Bản Vẽ - Na Loi.
5. Đẩy mạnh chuyển đổi số, ứng dụng CNTT trong quản lý hành chính.",
            PersonalNotes = "Hội nghị thành công tốt đẹp. Đ/c PCT thành phố đánh giá cao nỗ lực xã Hòa Bình trong xây dựng NTM. Cần lưu ý chỉ tiêu thu ngân sách năm 2026 khá tham vọng.",
            Tasks = new List<MeetingTask>
            {
                new() { Title = "Hoàn thiện báo cáo tổng kết trình HĐND xã", AssignedTo = CbVpUbnd, AssignedUnit = "VP-TK", Deadline = new DateTime(2026, 1, 20), TaskStatus = MeetingTaskStatus.Completed, CompletionDate = new DateTime(2026, 1, 18), Priority = 4 },
                new() { Title = "Xây dựng kế hoạch phát triển KT-XH năm 2026", AssignedTo = PctKtHt, AssignedUnit = "UBND xã", Deadline = new DateTime(2026, 1, 30), TaskStatus = MeetingTaskStatus.Completed, CompletionDate = new DateTime(2026, 1, 28), Priority = 5 },
            },
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Giấy mời Hội nghị tổng kết năm 2025", DocumentNumber = "05/GM-UBND", IssuedDate = new DateTime(2026, 1, 5), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.ChuongTrinh, Title = "Chương trình Hội nghị tổng kết năm 2025", IssuedDate = new DateTime(2026, 1, 5), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Báo cáo tổng kết KT-XH, AN-QP năm 2025 và phương hướng 2026", DocumentNumber = "85/BC-UBND", IssuedDate = new DateTime(2026, 1, 8), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Báo cáo quyết toán ngân sách năm 2025", DocumentNumber = "86/BC-UBND", IssuedDate = new DateTime(2026, 1, 8), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Dự thảo KH phát triển KT-XH năm 2026", DocumentNumber = "01/KH-UBND", IssuedDate = new DateTime(2026, 1, 8), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.BienBan, Title = "Biên bản Hội nghị tổng kết năm 2025", DocumentNumber = "01/BB-UBND", IssuedDate = new DateTime(2026, 1, 10), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.ThongBaoKetLuan, Title = "Thông báo kết luận Hội nghị tổng kết", DocumentNumber = "02/TB-UBND", IssuedDate = new DateTime(2026, 1, 12), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.QuyetDinh, Title = "Quyết định khen thưởng tập thể, cá nhân năm 2025", DocumentNumber = "02/QĐ-UBND", IssuedDate = new DateTime(2026, 1, 10), Issuer = OrgName },
            },
            Tags = new[] { "tổng kết", "năm 2025", "phương hướng", "2026", "khen thưởng" }
        };
    }
    
    /// <summary>5. Họp Ban chỉ đạo NTM</summary>
    private Meeting Create_HopBanChiDaoNTM()
    {
        return new Meeting
        {
            Title = "Họp Ban chỉ đạo xây dựng Nông thôn mới xã Hòa Bình",
            MeetingNumber = "02/GM-BCĐ",
            Type = MeetingType.HopBanChiDao,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Completed,
            Priority = 4,
            StartTime = new DateTime(2026, 1, 22, 14, 0, 0),
            EndTime = new DateTime(2026, 1, 22, 16, 0, 0),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = "Trưởng BCĐ xây dựng NTM xã",
            Secretary = CbDiaChinh,
            OrganizingUnit = "Ban Chỉ đạo xây dựng NTM xã Hòa Bình",
            Attendees = new List<MeetingAttendee>
            {
                new() { Name = ChuTich, Position = "Trưởng BCĐ", Unit = "BCĐ NTM xã", Role = AttendeeRole.ChairPerson, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = PctKtHt, Position = "Phó Trưởng BCĐ", Unit = "BCĐ NTM xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbDiaChinh, Position = "Thành viên BCĐ", Unit = "BCĐ NTM xã", Role = AttendeeRole.Secretary, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = ChuTichUbMttq, Position = "Thành viên BCĐ", Unit = "UB MTTQ xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = ChuTichHoiND, Position = "Thành viên BCĐ", Unit = "Hội Nông dân", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
            },
            Agenda = "1. Rà soát 19 tiêu chí NTM nâng cao\n2. Tiến độ xây dựng đường liên bản\n3. Kế hoạch vận động nhân dân đóng góp năm 2026",
            Conclusion = "BCĐ thống nhất: tập trung hoàn thiện tiêu chí số 17 (Môi trường) và số 19 (Quốc phòng - An ninh).",
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Giấy mời họp BCĐ xây dựng NTM", DocumentNumber = "02/GM-BCĐ", IssuedDate = new DateTime(2026, 1, 19), Issuer = "BCĐ NTM xã" },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Báo cáo rà soát 19 tiêu chí NTM nâng cao", IssuedDate = new DateTime(2026, 1, 20), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.BienBan, Title = "Biên bản họp BCĐ NTM", DocumentNumber = "01/BB-BCĐ", IssuedDate = new DateTime(2026, 1, 22), Issuer = "BCĐ NTM xã" },
            },
            Tags = new[] { "NTM", "nông thôn mới", "BCĐ" }
        };
    }
    
    /// <summary>6. Tiếp công dân định kỳ</summary>
    private Meeting Create_TiepCongDanDinhKy()
    {
        return new Meeting
        {
            Title = "Tiếp công dân định kỳ tháng 01/2026",
            MeetingNumber = "01/TB-UBND",
            Type = MeetingType.TiepCongDan,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Completed,
            Priority = 4,
            StartTime = new DateTime(2026, 1, 15, 8, 0, 0),
            EndTime = new DateTime(2026, 1, 15, 11, 0, 0),
            Location = "Phòng tiếp dân UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbTuPhap,
            OrganizingUnit = OrgName,
            Attendees = new List<MeetingAttendee>
            {
                new() { Name = ChuTich, Position = ChucVuChuTich, Unit = OrgName, Role = AttendeeRole.ChairPerson, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbTuPhap, Position = "CB Tư pháp", Unit = OrgName, Role = AttendeeRole.Secretary, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbDiaChinh, Position = "CB Địa chính", Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
            },
            Agenda = "Tiếp và giải quyết đơn thư khiếu nại, tố cáo, kiến nghị, phản ánh của công dân.",
            Content = @"Tiếp 03 lượt công dân:
1. Ông Lô Văn Hùng (bản Na Hang): Kiến nghị về đường vào bản bị sạt lở → Chuyển CB ĐC-XD xử lý.
2. Bà Vi Thị Hoa (bản Khe Bố): Phản ánh hàng xóm lấn chiếm đất → Hẹn hòa giải tuần sau.
3. Ông Nguyễn Văn Bình (thôn Hòa Phong): Hỏi thủ tục cấp GCNQSDĐ → Hướng dẫn hồ sơ.",
            Conclusion = "Chủ tịch UBND xã chỉ đạo: 1) CB ĐC-XD kiểm tra hiện trường sạt lở bản Na Hang; 2) Tổ hòa giải tiến hành hòa giải vụ lấn chiếm đất; 3) Hướng dẫn ông Bình hoàn thiện hồ sơ.",
            Tasks = new List<MeetingTask>
            {
                new() { Title = "Kiểm tra hiện trường sạt lở đường vào bản Na Hang", AssignedTo = CbDiaChinh, AssignedUnit = "ĐC-XD", Deadline = new DateTime(2026, 1, 20), TaskStatus = MeetingTaskStatus.Completed, CompletionDate = new DateTime(2026, 1, 19), Priority = 4 },
                new() { Title = "Tổ chức hòa giải tranh chấp đất giữa 2 hộ tại Khe Bố", AssignedTo = CbTuPhap, AssignedUnit = "Tư pháp", Deadline = new DateTime(2026, 1, 25), TaskStatus = MeetingTaskStatus.Completed, CompletionDate = new DateTime(2026, 1, 23), Priority = 3 },
            },
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Thông báo lịch tiếp công dân định kỳ tháng 01", DocumentNumber = "01/TB-UBND", IssuedDate = new DateTime(2026, 1, 10), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.BienBan, Title = "Biên bản tiếp công dân ngày 15/01/2026", DocumentNumber = "02/BB-UBND", IssuedDate = new DateTime(2026, 1, 15), Issuer = OrgName },
            },
            Tags = new[] { "tiếp dân", "khiếu nại", "đất đai" }
        };
    }
    
    /// <summary>7. Kỳ họp HĐND xã cuối năm</summary>
    private Meeting Create_HopHDND_KyHop()
    {
        return new Meeting
        {
            Title = "Kỳ họp thứ 8, HĐND xã Hòa Bình khóa XXI",
            MeetingNumber = "10/GM-HĐND",
            Type = MeetingType.HopHDND,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Completed,
            Priority = 5,
            StartTime = new DateTime(2026, 1, 5, 8, 0, 0),
            EndTime = new DateTime(2026, 1, 5, 17, 0, 0),
            IsAllDay = true,
            Location = "Hội trường UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTichHdnd,
            ChairPersonTitle = ChucVuCtHdnd,
            Secretary = CbVpUbnd,
            OrganizingUnit = "HĐND xã Hòa Bình",
            Attendees = new List<MeetingAttendee>
            {
                new() { Name = ChuTichHdnd, Position = ChucVuCtHdnd, Unit = "HĐND xã", Role = AttendeeRole.ChairPerson, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = ChuTich, Position = ChucVuChuTich, Unit = OrgName, Role = AttendeeRole.Presenter, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = BiThuDang, Position = ChucVuBiThu, Unit = "Đảng ủy xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = ChuTichUbMttq, Position = ChucVuCtMttq, Unit = "UB MTTQ xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = PctVhXh, Position = ChucVuPctVhXh, Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = PctKtHt, Position = ChucVuPctKtHt, Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbTaiChinh, Position = "CB TC-KT", Unit = OrgName, Role = AttendeeRole.Presenter, AttendanceStatus = AttendanceStatus.Attended },
            },
            Agenda = @"1. Báo cáo KT-XH năm 2025 và KH 2026
2. Báo cáo quyết toán ngân sách 2025, dự toán 2026
3. Báo cáo của MTTQ về tổng hợp ý kiến, kiến nghị cử tri
4. Thảo luận, chất vấn
5. Biểu quyết thông qua các Nghị quyết",
            Conclusion = "HĐND xã thông qua 05 Nghị quyết: NQ về KT-XH 2026, NQ về dự toán ngân sách, NQ về kế hoạch đầu tư công, NQ về giám sát chuyên đề, NQ chất vấn.",
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Giấy mời Kỳ họp thứ 8 HĐND xã khóa XXI", DocumentNumber = "10/GM-HĐND", IssuedDate = new DateTime(2025, 12, 28), Issuer = "HĐND xã" },
                new() { DocumentType = MeetingDocumentType.ChuongTrinh, Title = "Chương trình Kỳ họp thứ 8 HĐND xã", IssuedDate = new DateTime(2025, 12, 28), Issuer = "HĐND xã" },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Tờ trình phê duyệt quyết toán ngân sách 2025", DocumentNumber = "01/TTr-UBND", IssuedDate = new DateTime(2025, 12, 30), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Tờ trình dự toán ngân sách năm 2026", DocumentNumber = "02/TTr-UBND", IssuedDate = new DateTime(2025, 12, 30), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Báo cáo tổng hợp ý kiến, kiến nghị cử tri", IssuedDate = new DateTime(2025, 12, 29), Issuer = "UB MTTQ xã" },
                new() { DocumentType = MeetingDocumentType.BienBan, Title = "Biên bản Kỳ họp thứ 8 HĐND xã", DocumentNumber = "08/BB-HĐND", IssuedDate = new DateTime(2026, 1, 5), Issuer = "HĐND xã" },
                new() { DocumentType = MeetingDocumentType.NghiQuyet, Title = "Nghị quyết về nhiệm vụ phát triển KT-XH năm 2026", DocumentNumber = "25/NQ-HĐND", IssuedDate = new DateTime(2026, 1, 5), Issuer = "HĐND xã" },
                new() { DocumentType = MeetingDocumentType.NghiQuyet, Title = "Nghị quyết về dự toán ngân sách xã năm 2026", DocumentNumber = "26/NQ-HĐND", IssuedDate = new DateTime(2026, 1, 5), Issuer = "HĐND xã" },
            },
            Tags = new[] { "HĐND", "kỳ họp", "nghị quyết", "ngân sách" }
        };
    }
    
    // ===========================================================================
    // TUẦN NÀY / GẦN ĐÂY
    // ===========================================================================
    
    /// <summary>8. Họp giao ban tuần hiện tại</summary>
    private Meeting Create_HopGiaoBanTuanHienTai()
    {
        var monday = DateTime.Today;
        while (monday.DayOfWeek != DayOfWeek.Monday) monday = monday.AddDays(-1);
        
        return new Meeting
        {
            Title = $"Họp giao ban sáng thứ Hai ({monday:dd/MM/yyyy})",
            Type = MeetingType.HopGiaoBan,
            Level = MeetingLevel.CapDonVi,
            Status = monday.Date <= DateTime.Today.Date ? MeetingStatus.Completed : MeetingStatus.Scheduled,
            Priority = 3,
            StartTime = monday.Date.AddHours(7).AddMinutes(30),
            EndTime = monday.Date.AddHours(8).AddMinutes(30),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbVpUbnd,
            OrganizingUnit = OrgName,
            Attendees = new List<MeetingAttendee>
            {
                new() { Name = ChuTich, Position = ChucVuChuTich, Unit = OrgName, Role = AttendeeRole.ChairPerson, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = PctVhXh, Position = ChucVuPctVhXh, Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = PctKtHt, Position = ChucVuPctKtHt, Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbVpUbnd, Position = "VP-TK", Unit = OrgName, Role = AttendeeRole.Secretary, AttendanceStatus = AttendanceStatus.Attended },
            },
            Agenda = "1. Đánh giá tuần trước\n2. Phân công tuần này\n3. Vấn đề phát sinh",
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Giao ban sáng thứ Hai (thông báo qua Zalo)", Issuer = OrgName, Note = "Lịch cố định hàng tuần" },
            },
            Tags = new[] { "giao ban", "tuần" }
        };
    }
    
    /// <summary>9. Họp chuyên đề giải phóng mặt bằng</summary>
    private Meeting Create_HopChuyenDeGiaiPhongMatBang()
    {
        var ngayHop = DateTime.Today.AddDays(-2);
        if (ngayHop.DayOfWeek == DayOfWeek.Sunday) ngayHop = ngayHop.AddDays(-1);
        if (ngayHop.DayOfWeek == DayOfWeek.Saturday) ngayHop = ngayHop.AddDays(-1);
        
        return new Meeting
        {
            Title = "Họp chuyên đề giải phóng mặt bằng dự án đường liên bản Bản Vẽ - Na Loi",
            MeetingNumber = $"08/GM-UBND",
            Type = MeetingType.HopChuyenDe,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Completed,
            Priority = 5,
            StartTime = ngayHop.Date.AddHours(14),
            EndTime = ngayHop.Date.AddHours(17),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbDiaChinh,
            OrganizingUnit = OrgName,
            Attendees = new List<MeetingAttendee>
            {
                new() { Name = ChuTich, Position = ChucVuChuTich, Unit = OrgName, Role = AttendeeRole.ChairPerson, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = PctKtHt, Position = ChucVuPctKtHt, Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbDiaChinh, Position = "CB ĐC-XD", Unit = OrgName, Role = AttendeeRole.Presenter, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CbTaiChinh, Position = "CB TC-KT", Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = ChuTichUbMttq, Position = ChucVuCtMttq, Unit = "UB MTTQ xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = TruongThon[2], Position = "Trưởng bản Bản Vẽ", Unit = "Bản Vẽ", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = TruongThon[3], Position = "Trưởng bản Na Loi", Unit = "Na Loi", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
            },
            Agenda = "1. Báo cáo tiến độ GPMB dự án\n2. Phương án bồi thường, hỗ trợ 07 hộ bị ảnh hưởng\n3. Lộ trình thi công",
            Content = "CB ĐC-XD báo cáo: 5/7 hộ đã đồng ý phương án bồi thường. 2 hộ còn lại (hộ ông Lô Văn Thanh, bà Hà Thị Ngân) chưa đồng ý vì cho rằng đơn giá thấp.",
            Conclusion = "Giao PCT KT-HT làm việc trực tiếp với 2 hộ, vận động MTTQ, trưởng bản hỗ trợ. Deadline hoàn tất GPMB: 28/02/2026.",
            PersonalNotes = "Vụ này khá phức tạp, 2 hộ kiên quyết đòi giá cao hơn. Cần xin ý kiến thành phố nếu không thỏa thuận được.",
            Tasks = new List<MeetingTask>
            {
                new() { Title = "Làm việc trực tiếp với hộ ông Lô Văn Thanh về phương án GPMB", AssignedTo = PctKtHt, AssignedUnit = "UBND xã", Deadline = new DateTime(2026, 2, 15), TaskStatus = MeetingTaskStatus.InProgress, Priority = 5 },
                new() { Title = "Vận động bà Hà Thị Ngân đồng ý phương án bồi thường", AssignedTo = ChuTichUbMttq, AssignedUnit = "MTTQ xã", Deadline = new DateTime(2026, 2, 15), TaskStatus = MeetingTaskStatus.InProgress, Priority = 5 },
                new() { Title = "Hoàn thiện hồ sơ GPMB trình thành phố phê duyệt", AssignedTo = CbDiaChinh, AssignedUnit = "ĐC-XD", Deadline = new DateTime(2026, 2, 28), TaskStatus = MeetingTaskStatus.NotStarted, Priority = 4 },
            },
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Giấy mời họp chuyên đề GPMB dự án đường liên bản", DocumentNumber = "08/GM-UBND", IssuedDate = ngayHop.AddDays(-3), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Báo cáo tiến độ GPMB dự án đường Bản Vẽ - Na Loi", IssuedDate = ngayHop.AddDays(-1), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Phương án bồi thường, hỗ trợ tái định cư", IssuedDate = ngayHop.AddDays(-1), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.VanBanChiDao, Title = "Quyết định phê duyệt dự án đường liên bản Bản Vẽ - Na Loi", DocumentNumber = "456/QĐ-UBND", Issuer = "UBND thành phố Tương Dương" },
                new() { DocumentType = MeetingDocumentType.BienBan, Title = "Biên bản họp chuyên đề GPMB", IssuedDate = ngayHop, Issuer = OrgName },
            },
            Tags = new[] { "GPMB", "đường liên bản", "bồi thường", "Bản Vẽ", "Na Loi" }
        };
    }
    
    /// <summary>10. Họp liên ngành phòng chống thiên tai</summary>
    private Meeting Create_HopLienNganhPhongChongThienTai()
    {
        var ngayHop = DateTime.Today.AddDays(-1);
        
        return new Meeting
        {
            Title = "Họp liên ngành triển khai kế hoạch PCTT&TKCN năm 2026",
            MeetingNumber = "07/GM-UBND",
            Type = MeetingType.HopLienNganh,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Completed,
            Priority = 4,
            StartTime = ngayHop.Date.AddHours(8),
            EndTime = ngayHop.Date.AddHours(11),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.KetHop,
            OnlineLink = "https://meet.google.com/abc-defg-hij",
            ChairPerson = PctKtHt,
            ChairPersonTitle = "Phó Trưởng BCĐ PCTT&TKCN xã",
            Secretary = CbDiaChinh,
            OrganizingUnit = "Ban Chỉ huy PCTT&TKCN xã Hòa Bình",
            Attendees = new List<MeetingAttendee>
            {
                new() { Name = PctKtHt, Position = "Phó Trưởng BCH", Unit = "BCH PCTT xã", Role = AttendeeRole.ChairPerson, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = CaTruongCa, Position = "Thành viên BCH", Unit = "Công an xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = XaDoiTruong, Position = "Thành viên BCH", Unit = "Ban CHQS xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = TramTruong, Position = "Trạm trưởng TYT", Unit = "Trạm y tế xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = TruongThon[0], Position = "", Unit = "Bản Na Hang", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
                new() { Name = TruongThon[1], Position = "", Unit = "Bản Khe Bố", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Attended },
            },
            Agenda = "1. Nhận định tình hình thời tiết mùa mưa 2026\n2. Rà soát vùng nguy cơ sạt lở, lũ quét\n3. Phương án sơ tán dân, phương tiện cứu hộ\n4. Phân công lực lượng ứng trực",
            Conclusion = "Giao CA xã + CHQS xã lập kế hoạch ứng trực chi tiết. Trạm y tế chuẩn bị thuốc, vật tư y tế dự phòng.",
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Giấy mời họp liên ngành PCTT&TKCN", DocumentNumber = "07/GM-UBND", IssuedDate = ngayHop.AddDays(-3), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Kế hoạch PCTT&TKCN năm 2026 (dự thảo)", IssuedDate = ngayHop.AddDays(-2), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.VanBanChiDao, Title = "Chỉ thị về công tác PCTT&TKCN năm 2026", DocumentNumber = "05/CT-UBND", Issuer = "UBND thành phố Tương Dương" },
                new() { DocumentType = MeetingDocumentType.BienBan, Title = "Biên bản họp liên ngành PCTT", IssuedDate = ngayHop, Issuer = OrgName },
            },
            Tags = new[] { "PCTT", "phòng chống thiên tai", "liên ngành", "sạt lở" }
        };
    }
    
    // ===========================================================================
    // SẮP TỚI
    // ===========================================================================
    
    /// <summary>11. Họp UBND thường kỳ tháng 2/2026</summary>
    private Meeting Create_HopThuongKyThang2()
    {
        return new Meeting
        {
            Title = "Họp UBND xã thường kỳ tháng 02/2026",
            MeetingNumber = "10/GM-UBND",
            Type = MeetingType.HopThuongKy,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Scheduled,
            Priority = 4,
            StartTime = new DateTime(2026, 2, 15, 8, 0, 0),
            EndTime = new DateTime(2026, 2, 15, 11, 30, 0),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbVpUbnd,
            OrganizingUnit = OrgFull,
            Attendees = new List<MeetingAttendee>
            {
                new() { Name = ChuTich, Position = ChucVuChuTich, Unit = OrgName, Role = AttendeeRole.ChairPerson, AttendanceStatus = AttendanceStatus.Confirmed },
                new() { Name = PctVhXh, Position = ChucVuPctVhXh, Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Confirmed },
                new() { Name = PctKtHt, Position = ChucVuPctKtHt, Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Confirmed },
                new() { Name = CbVpUbnd, Position = "VP-TK", Unit = OrgName, Role = AttendeeRole.Secretary, AttendanceStatus = AttendanceStatus.Confirmed },
                new() { Name = CbDiaChinh, Position = "CB ĐC-XD", Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Invited },
                new() { Name = CbTaiChinh, Position = "CB TC-KT", Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Invited },
                new() { Name = CaTruongCa, Position = "Trưởng CA xã", Unit = "Công an xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Invited },
            },
            Agenda = @"1. Đánh giá tình hình sau Tết Nguyên đán
2. Báo cáo thu-chi ngân sách tháng 02
3. Tiến độ GPMB dự án đường liên bản
4. Triển khai kế hoạch sản xuất vụ Xuân 2026
5. An ninh trật tự sau Tết",
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Giấy mời họp UBND xã thường kỳ tháng 02/2026", DocumentNumber = "10/GM-UBND", IssuedDate = new DateTime(2026, 2, 10), Issuer = OrgName },
            },
            Tags = new[] { "thường kỳ", "UBND", "tháng 02" }
        };
    }
    
    /// <summary>12. Họp Đảng ủy xã</summary>
    private Meeting Create_HopDangUyDinhKy()
    {
        return new Meeting
        {
            Title = "Họp Đảng ủy xã Hòa Bình tháng 02/2026",
            MeetingNumber = "03/GM-ĐU",
            Type = MeetingType.HopDangUy,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Scheduled,
            Priority = 5,
            StartTime = new DateTime(2026, 2, 12, 14, 0, 0),
            EndTime = new DateTime(2026, 2, 12, 17, 0, 0),
            Location = "Phòng họp Đảng ủy xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = BiThuDang,
            ChairPersonTitle = ChucVuBiThu,
            Secretary = ChuTich,
            OrganizingUnit = "Đảng ủy xã Hòa Bình",
            Attendees = new List<MeetingAttendee>
            {
                new() { Name = BiThuDang, Position = ChucVuBiThu, Unit = "Đảng ủy xã", Role = AttendeeRole.ChairPerson, AttendanceStatus = AttendanceStatus.Confirmed },
                new() { Name = ChuTich, Position = "Phó BT Đảng ủy", Unit = "Đảng ủy xã", Role = AttendeeRole.Secretary, AttendanceStatus = AttendanceStatus.Confirmed },
                new() { Name = ChuTichHdnd, Position = "Ủy viên BTV", Unit = "Đảng ủy xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Confirmed },
                new() { Name = PctVhXh, Position = "Đảng ủy viên", Unit = "Đảng ủy xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Confirmed },
                new() { Name = PctKtHt, Position = "Đảng ủy viên", Unit = "Đảng ủy xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Confirmed },
                new() { Name = ChuTichUbMttq, Position = "Đảng ủy viên", Unit = "Đảng ủy xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Invited },
            },
            Agenda = @"1. Đánh giá kết quả lãnh đạo tháng 01/2026
2. Công tác tổ chức, cán bộ
3. Chuẩn bị Đại hội các chi bộ trực thuộc
4. Bàn phương hướng tháng 02/2026",
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Giấy mời họp Đảng ủy xã tháng 02/2026", DocumentNumber = "03/GM-ĐU", IssuedDate = new DateTime(2026, 2, 8), Issuer = "Đảng ủy xã" },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Báo cáo kết quả lãnh đạo tháng 01/2026", IssuedDate = new DateTime(2026, 2, 10), Issuer = "Đảng ủy xã" },
            },
            Tags = new[] { "Đảng ủy", "lãnh đạo", "tháng 02" }
        };
    }
    
    /// <summary>13. Tập huấn chuyển đổi số</summary>
    private Meeting Create_TapHuanChuyenDoiSo()
    {
        return new Meeting
        {
            Title = "Tập huấn chuyển đổi số cho cán bộ xã Hòa Bình năm 2026",
            MeetingNumber = "12/GM-UBND",
            Type = MeetingType.TapHuan,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Scheduled,
            Priority = 3,
            StartTime = new DateTime(2026, 2, 20, 8, 0, 0),
            EndTime = new DateTime(2026, 2, 20, 16, 30, 0),
            IsAllDay = true,
            Location = "Hội trường UBND xã Hòa Bình",
            Format = MeetingFormat.KetHop,
            OnlineLink = "https://zoom.us/j/1234567890",
            ChairPerson = PctVhXh,
            ChairPersonTitle = ChucVuPctVhXh,
            Secretary = CbVpUbnd,
            OrganizingUnit = OrgName,
            Attendees = new List<MeetingAttendee>
            {
                new() { Name = PctVhXh, Position = ChucVuPctVhXh, Unit = OrgName, Role = AttendeeRole.ChairPerson, AttendanceStatus = AttendanceStatus.Confirmed },
                new() { Name = "ThS. Nguyễn Văn Hùng", Position = "Chuyên viên CNTT", Unit = "Sở TT&TT Nghệ An", Role = AttendeeRole.Presenter, AttendanceStatus = AttendanceStatus.Confirmed, Note = "Báo cáo viên chính" },
                new() { Name = CbVpUbnd, Position = "VP-TK", Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Invited },
                new() { Name = CbDiaChinh, Position = "CB ĐC-XD", Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Invited },
                new() { Name = CbTuPhap, Position = "CB Tư pháp", Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Invited },
            },
            Agenda = @"BUỔI SÁNG:
1. Tổng quan về Chuyển đổi số trong cơ quan nhà nước
2. Hướng dẫn sử dụng Cổng dịch vụ công trực tuyến
3. Thực hành: Xử lý hồ sơ trực tuyến

BUỔI CHIỀU:
4. Ký số, chữ ký điện tử trong văn bản
5. Quản lý văn bản điện tử
6. Hỏi đáp, trao đổi",
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Giấy mời tập huấn Chuyển đổi số năm 2026", DocumentNumber = "12/GM-UBND", IssuedDate = new DateTime(2026, 2, 12), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.ChuongTrinh, Title = "Chương trình tập huấn CĐS", IssuedDate = new DateTime(2026, 2, 12), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Tài liệu tập huấn: Cổng DVCTT cấp xã", Issuer = "Sở TT&TT Nghệ An" },
                new() { DocumentType = MeetingDocumentType.VanBanChiDao, Title = "Kế hoạch CĐS tỉnh Nghệ An năm 2026", DocumentNumber = "15/KH-UBND", Issuer = "UBND tỉnh Nghệ An" },
            },
            Tags = new[] { "chuyển đổi số", "tập huấn", "CNTT", "dịch vụ công" }
        };
    }
    
    /// <summary>14. Họp xét khen thưởng</summary>
    private Meeting Create_HopXetKhenThuong()
    {
        return new Meeting
        {
            Title = "Họp Hội đồng Thi đua - Khen thưởng xã quý I/2026",
            MeetingNumber = "11/GM-UBND",
            Type = MeetingType.HopCoQuan,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Scheduled,
            Priority = 3,
            StartTime = new DateTime(2026, 2, 25, 14, 0, 0),
            EndTime = new DateTime(2026, 2, 25, 16, 0, 0),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = "CT Hội đồng TĐ-KT xã",
            Secretary = CbVhXh,
            OrganizingUnit = "Hội đồng TĐ-KT xã Hòa Bình",
            Attendees = new List<MeetingAttendee>
            {
                new() { Name = ChuTich, Position = "CT Hội đồng TĐ-KT", Unit = OrgName, Role = AttendeeRole.ChairPerson, AttendanceStatus = AttendanceStatus.Invited },
                new() { Name = PctVhXh, Position = "Phó CT HĐ", Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Invited },
                new() { Name = CbVhXh, Position = "Thành viên HĐ", Unit = OrgName, Role = AttendeeRole.Secretary, AttendanceStatus = AttendanceStatus.Invited },
                new() { Name = ChuTichUbMttq, Position = "Thành viên HĐ", Unit = "MTTQ xã", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Invited },
            },
            Agenda = "1. Xét danh sách cá nhân, tập thể đề nghị khen thưởng quý I\n2. Bình xét danh hiệu thi đua\n3. Biểu quyết, thông qua",
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Giấy mời họp HĐ TĐ-KT xã quý I/2026", DocumentNumber = "11/GM-UBND", IssuedDate = new DateTime(2026, 2, 20), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Danh sách đề nghị khen thưởng quý I/2026", IssuedDate = new DateTime(2026, 2, 22), Issuer = OrgName },
            },
            Tags = new[] { "khen thưởng", "thi đua", "quý I" }
        };
    }
    
    /// <summary>15. Họp triển khai kế hoạch sản xuất</summary>
    private Meeting Create_HopTrienKhaiKeHoach()
    {
        return new Meeting
        {
            Title = "Họp triển khai kế hoạch sản xuất nông nghiệp vụ Xuân 2026",
            MeetingNumber = "09/GM-UBND",
            Type = MeetingType.HopTrienKhai,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Scheduled,
            Priority = 4,
            StartTime = new DateTime(2026, 2, 10, 8, 0, 0),
            EndTime = new DateTime(2026, 2, 10, 11, 0, 0),
            Location = "Hội trường UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = PctKtHt,
            ChairPersonTitle = ChucVuPctKtHt,
            Secretary = CbDiaChinh,
            OrganizingUnit = OrgName,
            Attendees = new List<MeetingAttendee>
            {
                new() { Name = PctKtHt, Position = ChucVuPctKtHt, Unit = OrgName, Role = AttendeeRole.ChairPerson, AttendanceStatus = AttendanceStatus.Confirmed },
                new() { Name = CbDiaChinh, Position = "CB ĐC-XD", Unit = OrgName, Role = AttendeeRole.Presenter, AttendanceStatus = AttendanceStatus.Confirmed },
                new() { Name = ChuTichHoiND, Position = "CT Hội ND xã", Unit = "Hội Nông dân", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Confirmed },
                new() { Name = TruongThon[0], Position = "", Unit = "Bản Na Hang", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Invited },
                new() { Name = TruongThon[1], Position = "", Unit = "Bản Khe Bố", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Invited },
                new() { Name = TruongThon[2], Position = "", Unit = "Bản Bản Vẽ", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Invited },
                new() { Name = TruongThon[3], Position = "", Unit = "Bản Na Loi", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Invited },
                new() { Name = TruongThon[4], Position = "", Unit = "Thôn Hòa Phong", Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Invited },
            },
            Agenda = "1. Kế hoạch sản xuất lúa vụ Xuân 2026 (diện tích, giống, lịch thời vụ)\n2. Phương án cung ứng vật tư nông nghiệp\n3. Kỹ thuật canh tác mới\n4. Phân công các bản thực hiện",
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Giấy mời họp triển khai SX vụ Xuân 2026", DocumentNumber = "09/GM-UBND", IssuedDate = new DateTime(2026, 2, 5), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Kế hoạch sản xuất nông nghiệp vụ Xuân 2026", DocumentNumber = "05/KH-UBND", IssuedDate = new DateTime(2026, 2, 3), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.VanBanChiDao, Title = "Hướng dẫn sản xuất vụ Xuân 2026", DocumentNumber = "15/HD-NNPTNT", Issuer = "Sở NN&PTNT tỉnh" },
            },
            Tags = new[] { "nông nghiệp", "vụ Xuân", "sản xuất", "2026" }
        };
    }
    
    // ===========================================================================
    // TƯƠNG LAI XA HƠN
    // ===========================================================================
    
    /// <summary>16. Hội nghị nhân dân bản</summary>
    private Meeting Create_HoiNghiNhanDan()
    {
        return new Meeting
        {
            Title = "Hội nghị nhân dân bản Na Hang về xây dựng đường giao thông nội bản",
            MeetingNumber = "15/GM-UBND",
            Type = MeetingType.HoiNghi,
            Level = MeetingLevel.CapDonVi,
            Status = MeetingStatus.Scheduled,
            Priority = 3,
            StartTime = new DateTime(2026, 3, 5, 19, 0, 0),
            EndTime = new DateTime(2026, 3, 5, 21, 0, 0),
            Location = "Nhà văn hóa bản Na Hang",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = TruongThon[0].Split(" - ")[0],
            ChairPersonTitle = "Trưởng bản Na Hang",
            Secretary = CbDiaChinh,
            OrganizingUnit = "UBND xã phối hợp bản Na Hang",
            Attendees = new List<MeetingAttendee>
            {
                new() { Name = TruongThon[0].Split(" - ")[0], Position = "Trưởng bản", Unit = "Bản Na Hang", Role = AttendeeRole.ChairPerson, AttendanceStatus = AttendanceStatus.Invited },
                new() { Name = PctKtHt, Position = ChucVuPctKtHt, Unit = OrgName, Role = AttendeeRole.Attendee, AttendanceStatus = AttendanceStatus.Invited },
                new() { Name = CbDiaChinh, Position = "CB ĐC-XD", Unit = OrgName, Role = AttendeeRole.Secretary, AttendanceStatus = AttendanceStatus.Invited },
            },
            Agenda = "1. Thông báo chủ trương xây dựng đường bê tông nội bản\n2. Lấy ý kiến nhân dân về phương án tuyến\n3. Vận động đóng góp ngày công, hiến đất\n4. Biểu quyết thống nhất",
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Giấy mời hội nghị nhân dân bản Na Hang", DocumentNumber = "15/GM-UBND", IssuedDate = new DateTime(2026, 2, 28), Issuer = OrgName },
                new() { DocumentType = MeetingDocumentType.TaiLieuHop, Title = "Phương án xây dựng đường bê tông nội bản Na Hang", IssuedDate = new DateTime(2026, 2, 25), Issuer = OrgName },
            },
            Tags = new[] { "nhân dân", "Na Hang", "đường bê tông", "NTM" }
        };
    }
    
    /// <summary>17. Họp sơ kết 6 tháng</summary>
    private Meeting Create_HopSoKet6Thang()
    {
        return new Meeting
        {
            Title = "Họp sơ kết công tác 6 tháng đầu năm 2026",
            MeetingNumber = "",
            Type = MeetingType.HopSoKet,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Scheduled,
            Priority = 4,
            StartTime = new DateTime(2026, 7, 10, 8, 0, 0),
            EndTime = new DateTime(2026, 7, 10, 11, 30, 0),
            Location = "Hội trường UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbVpUbnd,
            OrganizingUnit = OrgFull,
            Agenda = @"1. Báo cáo kết quả thực hiện nhiệm vụ 6 tháng đầu năm
2. Báo cáo thu-chi ngân sách 6 tháng
3. Đánh giá tiến độ các công trình, dự án
4. Phương hướng nhiệm vụ 6 tháng cuối năm",
            Documents = new List<MeetingDocument>
            {
                new() { DocumentType = MeetingDocumentType.GiayMoi, Title = "Giấy mời họp sơ kết 6 tháng đầu năm 2026", Issuer = OrgName, Note = "Sẽ phát hành sau" },
            },
            Tags = new[] { "sơ kết", "6 tháng", "2026" }
        };
    }
}
