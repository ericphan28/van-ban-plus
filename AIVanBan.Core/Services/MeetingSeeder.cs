using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service tạo dữ liệu demo cuộc họp sát thực tế — UBND xã Hòa Bình, TP Tương Dương, Nghệ An.
/// TẤT CẢ NGÀY đều tương đối so với DateTime.Today → dữ liệu luôn tươi mới bất kể ngày cài app.
/// Bao gồm đầy đủ: tất cả trạng thái, đa dạng loại họp, tasks đa dạng, nhiều format.
/// </summary>
public class MeetingSeeder
{
    private readonly MeetingService _meetingService;
    
    // Ngày gốc — tất cả tính tương đối từ đây
    private static DateTime Today => DateTime.Today;
    
    // === CƠ CẤU TỔ CHỨC UBND XÃ HÒA BÌNH ===
    private const string OrgName = "UBND xã Hòa Bình";
    private const string OrgFull = "Ủy ban nhân dân xã Hòa Bình";
    
    // Ban lãnh đạo
    private const string ChuTich = "Lê Văn Thắng";
    private const string ChucVuChuTich = "Chủ tịch UBND xã";
    private const string PctVhXh = "Nguyễn Thị Hương";
    private const string ChucVuPctVhXh = "Phó CT UBND xã phụ trách VH-XH";
    private const string PctKtHt = "Trần Đình Lâm";
    private const string ChucVuPctKtHt = "Phó CT UBND xã phụ trách KT-HT";
    private const string BiThuDang = "Hoàng Minh Đức";
    private const string ChucVuBiThu = "Bí thư Đảng ủy xã";
    private const string ChuTichHdnd = "Phạm Thị Lan";
    private const string ChucVuCtHdnd = "Chủ tịch HĐND xã";
    private const string ChuTichUbMttq = "Lương Văn Tùng";
    private const string ChucVuCtMttq = "Chủ tịch UB MTTQ VN xã";
    
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
    
    // Trưởng thôn/bản
    private static readonly string[] TruongThon = new[]
    {
        "Lô Văn Thanh", "Vi Văn Hoa", "Lương Văn Đông", "Hà Văn Sáng", "Nguyễn Văn Phúc"
    };
    private static readonly string[] TenBan = new[]
    {
        "bản Na Hang", "bản Khe Bố", "bản Bản Vẽ", "bản Na Loi", "thôn Hòa Phong"
    };
    
    public MeetingSeeder(MeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    /// <summary>
    /// Tạo ~25 cuộc họp demo bao quát mọi trường hợp.
    /// Tất cả ngày tương đối → luôn tươi mới.
    /// </summary>
    public void SeedDemoMeetings()
    {
        var existing = _meetingService.GetAllMeetings();
        if (existing.Count > 0)
        {
            Console.WriteLine($"✅ Đã có {existing.Count} cuộc họp. Bỏ qua seed.");
            return;
        }
        
        Console.WriteLine("📅 Đang tạo dữ liệu demo cuộc họp (relative dates)...");
        
        var meetings = new List<Meeting>
        {
            // ══════════ QUÁ KHỨ — ĐÃ HOÀN THÀNH (7) ══════════
            Past_HoiNghiTongKetNam(),          // -28 ngày: Tổng kết năm (cả ngày, priority 5)
            Past_HopHDND_KyHop(),              // -25 ngày: Kỳ họp HĐND (cả ngày)
            Past_HopThuongKyThangTruoc(),      // -21 ngày: UBND thường kỳ (tasks hoàn thành)
            Past_HopChiBoDinhKy(),             // -18 ngày: Sinh hoạt Chi bộ
            Past_HopBanChiDaoNTM(),            // -14 ngày: BCĐ Nông thôn mới
            Past_TiepCongDanDinhKy(),          // -10 ngày: Tiếp công dân
            Past_HopGiaoBanTuanTruoc(),        // -7 ngày: Giao ban tuần trước
            
            // ══════════ GẦN ĐÂY — HÔM QUA / HÔM NAY (4) ══════════
            Recent_HopChuyenDeGPMB(),          // -3 ngày: Chuyên đề GPMB (tasks đang làm + quá hạn)
            Recent_HopLienNganhPCTT(),         // -1 ngày: Liên ngành PCTT (kết hợp online)
            Today_HopGiaoBanSangNay(),         // Hôm nay 7:30: Giao ban (đã xong hoặc đang diễn ra)
            Today_HopChuyenDeChieuNay(),       // Hôm nay 14:00: Chuyên đề buổi chiều
            
            // ══════════ ĐÃ HOÃN / ĐÃ HỦY (2) ══════════
            Special_HopHoanLai(),              // +2 ngày → Postponed
            Special_HopDaHuy(),                // +4 ngày → Cancelled
            
            // ══════════ SẮP TỚI — TUẦN NÀY / TUẦN SAU (5) ══════════
            Soon_HopDangUy(),                  // +1 ngày: Đảng ủy
            Soon_TapHuanChuyenDoiSo(),         // +3 ngày: Tập huấn CĐS (cả ngày, kết hợp)
            Soon_HopXetKhenThuong(),           // +5 ngày: Khen thưởng quý
            Soon_HopThuongKyThangNay(),        // +7 ngày: UBND thường kỳ tháng này
            Soon_HopTrienKhaiSanXuat(),        // +10 ngày: Triển khai sản xuất
            
            // ══════════ TƯƠNG LAI — 2-5 TUẦN TỚI (5) ══════════
            Future_TiepCongDanThangSau(),      // +15 ngày: Tiếp công dân
            Future_HoiNghiNhanDanBan(),        // +19 ngày: Hội nghị nhân dân bản (tối)
            Future_HopSoKetQuy(),              // +24 ngày: Sơ kết quý (trực tuyến)
            Future_HopChiBoDinhKy(),           // +28 ngày: Sinh hoạt Chi bộ tháng sau
            Future_HopLienNganhGiaoDuc(),      // +35 ngày: Liên ngành giáo dục
        };
        
        foreach (var meeting in meetings)
        {
            _meetingService.AddMeeting(meeting);
            Console.WriteLine($"  ✓ {meeting.StartTime:dd/MM HH:mm} [{meeting.Status}] {meeting.Title}");
        }
        
        Console.WriteLine($"✅ Đã tạo {meetings.Count} cuộc họp demo thành công!");
    }
    
    // ═══════════════════════════════════════════════════════════════
    //  HELPER: Tính ngày làm việc (bỏ T7/CN)
    // ═══════════════════════════════════════════════════════════════
    
    private static DateTime WorkDay(int daysFromToday)
    {
        var date = Today.AddDays(daysFromToday);
        if (date.DayOfWeek == DayOfWeek.Saturday) date = date.AddDays(daysFromToday > 0 ? 2 : -1);
        if (date.DayOfWeek == DayOfWeek.Sunday) date = date.AddDays(daysFromToday > 0 ? 1 : -2);
        return date;
    }
    
    // ═══════════════════════════════════════════════════════════════
    //  QUÁ KHỨ — ĐÃ HOÀN THÀNH
    // ═══════════════════════════════════════════════════════════════
    
    private Meeting Past_HoiNghiTongKetNam()
    {
        var d = WorkDay(-28);
        return new Meeting
        {
            Title = "Hội nghị tổng kết công tác năm và phương hướng nhiệm vụ năm mới",
            MeetingNumber = "05/GM-UBND",
            Type = MeetingType.HopTongKet,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Completed,
            Priority = 5,
            StartTime = d.AddHours(8),
            EndTime = d.AddHours(17),
            IsAllDay = true,
            Location = "Hội trường UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbVpUbnd,
            OrganizingUnit = OrgFull,
            Attendees = new List<MeetingAttendee>
            {
                Att(ChuTich, ChucVuChuTich, OrgName, AttendeeRole.ChairPerson, AttendanceStatus.Attended),
                Att(PctVhXh, ChucVuPctVhXh, OrgName, AttendeeRole.Presenter, AttendanceStatus.Attended),
                Att(PctKtHt, ChucVuPctKtHt, OrgName, AttendeeRole.Presenter, AttendanceStatus.Attended),
                Att(BiThuDang, ChucVuBiThu, "Đảng ủy xã", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(ChuTichHdnd, ChucVuCtHdnd, "HĐND xã", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(ChuTichUbMttq, ChucVuCtMttq, "UB MTTQ xã", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att("Nguyễn Đức Hà", "PCT UBND thành phố", "UBND TP Tương Dương", AttendeeRole.Observer, AttendanceStatus.Attended, "Phát biểu chỉ đạo"),
                Att(CbVpUbnd, "VP-TK", OrgName, AttendeeRole.Secretary, AttendanceStatus.Attended),
                Att(CbTaiChinh, "CB TC-KT", OrgName, AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(ChuTichHoiND, "CT Hội Nông dân", "Hội Nông dân", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(ChuTichHoiPN, "CT Hội LHPN", "Hội Phụ nữ", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(BiThuDoanTn, "BT Đoàn xã", "Đoàn TN", AttendeeRole.Attendee, AttendanceStatus.Attended),
            },
            Agenda = "SÁNG: Báo cáo tổng kết KT-XH, AN-QP; Báo cáo ngân sách; Tham luận.\nCHIỀU: Phương hướng năm mới; Phát biểu chỉ đạo; Khen thưởng; Bế mạc.",
            Content = "Năm qua xã đạt nhiều kết quả: thu NS 520/500tr (104%); 19/19 tiêu chí NTM nâng cao; hộ nghèo giảm còn 3,2%.",
            Conclusion = "Năm mới phấn đấu thu NS 550tr; duy trì NTM; giảm hộ nghèo xuống <2%; đẩy mạnh chuyển đổi số.",
            PersonalNotes = "Hội nghị thành công, PCT thành phố đánh giá cao. Chỉ tiêu NS khá tham vọng.",
            Tasks = new List<MeetingTask>
            {
                Task("Hoàn thiện báo cáo tổng kết trình HĐND xã", CbVpUbnd, "VP-TK", d.AddDays(7), MeetingTaskStatus.Completed, d.AddDays(5), 4),
                Task("Xây dựng KH phát triển KT-XH năm mới", PctKtHt, "UBND xã", d.AddDays(14), MeetingTaskStatus.Completed, d.AddDays(12), 5),
            },
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "Giấy mời Hội nghị tổng kết năm", "05/GM-UBND", d.AddDays(-5)),
                Doc(MeetingDocumentType.ChuongTrinh, "Chương trình Hội nghị tổng kết", "", d.AddDays(-5)),
                Doc(MeetingDocumentType.TaiLieuHop, "Báo cáo tổng kết KT-XH, AN-QP", "85/BC-UBND", d.AddDays(-3)),
                Doc(MeetingDocumentType.TaiLieuHop, "Báo cáo quyết toán ngân sách", "86/BC-UBND", d.AddDays(-3)),
                Doc(MeetingDocumentType.BienBan, "Biên bản Hội nghị tổng kết", "01/BB-UBND", d),
                Doc(MeetingDocumentType.ThongBaoKetLuan, "Thông báo kết luận Hội nghị", "02/TB-UBND", d.AddDays(2)),
                Doc(MeetingDocumentType.QuyetDinh, "QĐ khen thưởng tập thể, cá nhân tiên tiến", "02/QĐ-UBND", d),
            },
            Tags = new[] { "tổng kết", "khen thưởng", "phương hướng" }
        };
    }
    
    private Meeting Past_HopHDND_KyHop()
    {
        var d = WorkDay(-25);
        return new Meeting
        {
            Title = "Kỳ họp thứ 8, HĐND xã Hòa Bình khóa XXI",
            MeetingNumber = "10/GM-HĐND",
            Type = MeetingType.HopHDND,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Completed,
            Priority = 5,
            StartTime = d.AddHours(8),
            EndTime = d.AddHours(17),
            IsAllDay = true,
            Location = "Hội trường UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTichHdnd,
            ChairPersonTitle = ChucVuCtHdnd,
            Secretary = CbVpUbnd,
            OrganizingUnit = "HĐND xã Hòa Bình",
            Attendees = new List<MeetingAttendee>
            {
                Att(ChuTichHdnd, ChucVuCtHdnd, "HĐND xã", AttendeeRole.ChairPerson, AttendanceStatus.Attended),
                Att(ChuTich, ChucVuChuTich, OrgName, AttendeeRole.Presenter, AttendanceStatus.Attended),
                Att(BiThuDang, ChucVuBiThu, "Đảng ủy xã", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(ChuTichUbMttq, ChucVuCtMttq, "UB MTTQ xã", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(PctVhXh, ChucVuPctVhXh, OrgName, AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(PctKtHt, ChucVuPctKtHt, OrgName, AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(CbTaiChinh, "CB TC-KT", OrgName, AttendeeRole.Presenter, AttendanceStatus.Attended),
                Att(CbVpUbnd, "VP-TK", OrgName, AttendeeRole.Secretary, AttendanceStatus.Attended),
            },
            Agenda = "1. BC KT-XH + KH năm mới\n2. BC quyết toán NS, dự toán NS mới\n3. BC MTTQ tổng hợp ý kiến cử tri\n4. Thảo luận, chất vấn\n5. Biểu quyết thông qua NQ",
            Conclusion = "HĐND thông qua 5 Nghị quyết: NQ KT-XH, NQ dự toán NS, NQ đầu tư công, NQ giám sát, NQ chất vấn.",
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM Kỳ họp thứ 8 HĐND xã", "10/GM-HĐND", d.AddDays(-7)),
                Doc(MeetingDocumentType.TaiLieuHop, "Tờ trình phê duyệt quyết toán NS", "01/TTr-UBND", d.AddDays(-5)),
                Doc(MeetingDocumentType.TaiLieuHop, "Tờ trình dự toán NS năm mới", "02/TTr-UBND", d.AddDays(-5)),
                Doc(MeetingDocumentType.BienBan, "Biên bản Kỳ họp thứ 8", "08/BB-HĐND", d),
                Doc(MeetingDocumentType.NghiQuyet, "NQ về nhiệm vụ phát triển KT-XH", "25/NQ-HĐND", d),
                Doc(MeetingDocumentType.NghiQuyet, "NQ về dự toán NS xã", "26/NQ-HĐND", d),
            },
            Tags = new[] { "HĐND", "kỳ họp", "nghị quyết", "ngân sách" }
        };
    }
    
    private Meeting Past_HopThuongKyThangTruoc()
    {
        var d = WorkDay(-21);
        return new Meeting
        {
            Title = "Họp UBND xã thường kỳ tháng trước",
            MeetingNumber = "03/GM-UBND",
            Type = MeetingType.HopThuongKy,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Completed,
            Priority = 4,
            StartTime = d.AddHours(8),
            EndTime = d.AddHours(11).AddMinutes(30),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbVpUbnd,
            OrganizingUnit = OrgFull,
            Attendees = new List<MeetingAttendee>
            {
                Att(ChuTich, ChucVuChuTich, OrgName, AttendeeRole.ChairPerson, AttendanceStatus.Attended),
                Att(PctVhXh, ChucVuPctVhXh, OrgName, AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(PctKtHt, ChucVuPctKtHt, OrgName, AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(CbVpUbnd, "VP-TK", OrgName, AttendeeRole.Secretary, AttendanceStatus.Attended),
                Att(CbDiaChinh, "CB ĐC-XD", OrgName, AttendeeRole.Presenter, AttendanceStatus.Attended),
                Att(CbTuPhap, "CB Tư pháp", OrgName, AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(CbTaiChinh, "CB TC-KT", OrgName, AttendeeRole.Presenter, AttendanceStatus.Attended),
                Att(CbVhXh, "CB VH-XH", OrgName, AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(CaTruongCa, "Trưởng CA xã", "Công an xã", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(XaDoiTruong, "Xã đội trưởng", "Ban CHQS xã", AttendeeRole.Attendee, AttendanceStatus.AbsentWithPermission, "Ủy quyền Phó dự thay"),
            },
            Agenda = "1. Đánh giá nhiệm vụ tháng\n2. BC thu-chi ngân sách\n3. Quản lý đất đai, trật tự xây dựng\n4. An ninh trật tự\n5. Kiến nghị, đề xuất",
            Content = "Thu NS tháng đạt 45/550tr (8,2% KH năm). 2 hồ sơ cấp GCN đang xử lý. ANTT ổn định.",
            Conclusion = "1. Giao TC-KT tham mưu phương án thu NS quý I\n2. Giao ĐC-XD xử lý vi phạm XD bản Na Hang\n3. CA xã tăng cường tuần tra",
            PersonalNotes = "Cuộc họp đúng giờ, đầy đủ. Đ/c Tuấn cần đẩy nhanh tiến độ.",
            Tasks = new List<MeetingTask>
            {
                Task("Tham mưu phương án thu NS quý I", CbTaiChinh, "TC-KT", d.AddDays(7), MeetingTaskStatus.Completed, d.AddDays(5), 4),
                Task("Xử lý vi phạm xây dựng bản Na Hang", CbDiaChinh, "ĐC-XD", d.AddDays(14), MeetingTaskStatus.Completed, d.AddDays(12), 5),
                Task("KH tuần tra bảo vệ ANTT", CaTruongCa, "Công an xã", d.AddDays(5), MeetingTaskStatus.Completed, d.AddDays(4), 4),
                Task("Các bộ phận nộp BC tháng", CbVpUbnd, "VP-TK", d.AddDays(3), MeetingTaskStatus.Completed, d.AddDays(3), 3),
            },
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM họp UBND thường kỳ", "03/GM-UBND", d.AddDays(-3)),
                Doc(MeetingDocumentType.TaiLieuHop, "BC thu-chi ngân sách tháng", "05/BC-UBND", d.AddDays(-1)),
                Doc(MeetingDocumentType.TaiLieuHop, "BC quản lý đất đai, xây dựng", "06/BC-UBND", d.AddDays(-1)),
                Doc(MeetingDocumentType.BienBan, "Biên bản họp UBND thường kỳ", "03/BB-UBND", d),
                Doc(MeetingDocumentType.ThongBaoKetLuan, "TB kết luận họp UBND", "08/TB-UBND", d.AddDays(1)),
            },
            Tags = new[] { "thường kỳ", "UBND" }
        };
    }
    
    private Meeting Past_HopChiBoDinhKy()
    {
        var d = WorkDay(-18);
        return new Meeting
        {
            Title = "Sinh hoạt Chi bộ Cơ quan UBND xã",
            MeetingNumber = "01/GM-CB",
            Type = MeetingType.HopChiBo,
            Level = MeetingLevel.CapDonVi,
            Status = MeetingStatus.Completed,
            Priority = 4,
            StartTime = d.AddHours(14),
            EndTime = d.AddHours(16).AddMinutes(30),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = BiThuDang,
            ChairPersonTitle = "Bí thư Chi bộ",
            Secretary = CbVpUbnd,
            OrganizingUnit = "Chi bộ CQ UBND xã Hòa Bình",
            Attendees = new List<MeetingAttendee>
            {
                Att(BiThuDang, "Bí thư Chi bộ", "Chi bộ CQ", AttendeeRole.ChairPerson, AttendanceStatus.Attended),
                Att(ChuTich, "Phó BT Chi bộ", "Chi bộ CQ", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(PctVhXh, "Đảng viên", "Chi bộ CQ", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(PctKtHt, "Đảng viên", "Chi bộ CQ", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(CbVpUbnd, "Đảng viên", "Chi bộ CQ", AttendeeRole.Secretary, AttendanceStatus.Attended),
                Att(CbDiaChinh, "Đảng viên", "Chi bộ CQ", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(CbTuPhap, "Đảng viên", "Chi bộ CQ", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(CbTaiChinh, "Đảng viên", "Chi bộ CQ", AttendeeRole.Attendee, AttendanceStatus.Attended),
            },
            Agenda = "1. Thông tin thời sự, chỉ thị mới\n2. Đánh giá NQ tháng trước\n3. Kiểm điểm (nếu có)\n4. Phương hướng tháng tới\n5. Đảng phí",
            Content = "8/8 ĐV hoàn thành nhiệm vụ. Thông tin NQ 18-NQ/TW tinh gọn bộ máy. Đảng phí thu đủ.",
            Conclusion = "Giao đ/c Ngọc tổng hợp DS gia đình chính sách. Chuẩn bị nội dung chuyên đề tháng tới.",
            PersonalNotes = "Sinh hoạt đầy đủ, đúng quy định.",
            Tasks = new List<MeetingTask>
            {
                Task("Tổng hợp DS gia đình chính sách", CbVpUbnd, "VP-TK", d.AddDays(5), MeetingTaskStatus.Completed, d.AddDays(4), 4),
                Task("Chuẩn bị nội dung sinh hoạt chuyên đề", BiThuDang, "Chi bộ", d.AddDays(25), MeetingTaskStatus.InProgress, null, 3),
            },
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM sinh hoạt Chi bộ", "01/GM-CB", d.AddDays(-3)),
                Doc(MeetingDocumentType.TaiLieuHop, "NQ 18-NQ/TW (trích)", "18-NQ/TW", null, "BCH Trung ương"),
                Doc(MeetingDocumentType.BienBan, "BB sinh hoạt Chi bộ", "01/BB-CB", d),
            },
            Tags = new[] { "chi bộ", "đảng", "sinh hoạt" }
        };
    }
    
    private Meeting Past_HopBanChiDaoNTM()
    {
        var d = WorkDay(-14);
        return new Meeting
        {
            Title = "Họp BCĐ xây dựng Nông thôn mới xã Hòa Bình",
            MeetingNumber = "02/GM-BCĐ",
            Type = MeetingType.HopBanChiDao,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Completed,
            Priority = 4,
            StartTime = d.AddHours(14),
            EndTime = d.AddHours(16),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = "Trưởng BCĐ NTM xã",
            Secretary = CbDiaChinh,
            OrganizingUnit = "BCĐ xây dựng NTM xã Hòa Bình",
            Attendees = new List<MeetingAttendee>
            {
                Att(ChuTich, "Trưởng BCĐ", "BCĐ NTM", AttendeeRole.ChairPerson, AttendanceStatus.Attended),
                Att(PctKtHt, "Phó Trưởng BCĐ", "BCĐ NTM", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(CbDiaChinh, "Thành viên BCĐ", "BCĐ NTM", AttendeeRole.Secretary, AttendanceStatus.Attended),
                Att(ChuTichUbMttq, "Thành viên BCĐ", "MTTQ xã", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(ChuTichHoiND, "Thành viên BCĐ", "Hội ND", AttendeeRole.Attendee, AttendanceStatus.Attended),
            },
            Agenda = "1. Rà soát 19 tiêu chí NTM nâng cao\n2. Tiến độ đường liên bản\n3. KH vận động nhân dân đóng góp",
            Conclusion = "Tập trung hoàn thiện tiêu chí 17 (Môi trường) và 19 (QP-AN).",
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM họp BCĐ NTM", "02/GM-BCĐ", d.AddDays(-3)),
                Doc(MeetingDocumentType.TaiLieuHop, "BC rà soát 19 tiêu chí NTM", "", d.AddDays(-2)),
                Doc(MeetingDocumentType.BienBan, "BB họp BCĐ NTM", "01/BB-BCĐ", d),
            },
            Tags = new[] { "NTM", "nông thôn mới", "BCĐ" }
        };
    }
    
    private Meeting Past_TiepCongDanDinhKy()
    {
        var d = WorkDay(-10);
        return new Meeting
        {
            Title = "Tiếp công dân định kỳ",
            MeetingNumber = "01/TB-UBND",
            Type = MeetingType.TiepCongDan,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Completed,
            Priority = 4,
            StartTime = d.AddHours(8),
            EndTime = d.AddHours(11),
            Location = "Phòng tiếp dân UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbTuPhap,
            OrganizingUnit = OrgName,
            Attendees = new List<MeetingAttendee>
            {
                Att(ChuTich, ChucVuChuTich, OrgName, AttendeeRole.ChairPerson, AttendanceStatus.Attended),
                Att(CbTuPhap, "CB Tư pháp", OrgName, AttendeeRole.Secretary, AttendanceStatus.Attended),
                Att(CbDiaChinh, "CB ĐC-XD", OrgName, AttendeeRole.Attendee, AttendanceStatus.Attended),
            },
            Content = "Tiếp 3 lượt: ông Lô Văn Hùng (sạt lở đường bản Na Hang), bà Vi Thị Hoa (lấn chiếm đất Khe Bố), ông Nguyễn Văn Bình (thủ tục GCN).",
            Conclusion = "1) Kiểm tra sạt lở Na Hang; 2) Hòa giải vụ lấn chiếm; 3) Hướng dẫn hồ sơ GCN.",
            Tasks = new List<MeetingTask>
            {
                Task("Kiểm tra hiện trường sạt lở bản Na Hang", CbDiaChinh, "ĐC-XD", d.AddDays(5), MeetingTaskStatus.Completed, d.AddDays(4), 4),
                Task("Hòa giải tranh chấp đất Khe Bố", CbTuPhap, "Tư pháp", d.AddDays(10), MeetingTaskStatus.Completed, d.AddDays(8), 3),
            },
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "TB lịch tiếp công dân", "01/TB-UBND", d.AddDays(-5)),
                Doc(MeetingDocumentType.BienBan, "BB tiếp công dân", "02/BB-UBND", d),
            },
            Tags = new[] { "tiếp dân", "khiếu nại", "đất đai" }
        };
    }
    
    private Meeting Past_HopGiaoBanTuanTruoc()
    {
        var d = WorkDay(-7);
        return new Meeting
        {
            Title = $"Họp giao ban sáng thứ Hai ({d:dd/MM})",
            Type = MeetingType.HopGiaoBan,
            Level = MeetingLevel.CapDonVi,
            Status = MeetingStatus.Completed,
            Priority = 3,
            StartTime = d.AddHours(7).AddMinutes(30),
            EndTime = d.AddHours(8).AddMinutes(30),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbVpUbnd,
            OrganizingUnit = OrgName,
            Attendees = new List<MeetingAttendee>
            {
                Att(ChuTich, ChucVuChuTich, OrgName, AttendeeRole.ChairPerson, AttendanceStatus.Attended),
                Att(PctVhXh, ChucVuPctVhXh, OrgName, AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(PctKtHt, ChucVuPctKtHt, OrgName, AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(CbVpUbnd, "VP-TK", OrgName, AttendeeRole.Secretary, AttendanceStatus.Attended),
            },
            Agenda = "1. Tổng hợp tuần qua\n2. Phân công tuần này\n3. Phát sinh",
            Conclusion = "Tuần này tập trung xử lý hồ sơ GPMB, chuẩn bị tập huấn CĐS.",
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GB sáng thứ Hai (Zalo)", "", null, OrgName, "Lịch cố định hàng tuần"),
            },
            Tags = new[] { "giao ban", "tuần" }
        };
    }
    
    // ═══════════════════════════════════════════════════════════════
    //  GẦN ĐÂY — 3 NGÀY QUA + HÔM NAY
    // ═══════════════════════════════════════════════════════════════
    
    private Meeting Recent_HopChuyenDeGPMB()
    {
        var d = WorkDay(-3);
        return new Meeting
        {
            Title = "Họp chuyên đề GPMB dự án đường liên bản Bản Vẽ - Na Loi",
            MeetingNumber = "08/GM-UBND",
            Type = MeetingType.HopChuyenDe,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Completed,
            Priority = 5,
            StartTime = d.AddHours(14),
            EndTime = d.AddHours(17),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbDiaChinh,
            OrganizingUnit = OrgName,
            Attendees = new List<MeetingAttendee>
            {
                Att(ChuTich, ChucVuChuTich, OrgName, AttendeeRole.ChairPerson, AttendanceStatus.Attended),
                Att(PctKtHt, ChucVuPctKtHt, OrgName, AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(CbDiaChinh, "CB ĐC-XD", OrgName, AttendeeRole.Presenter, AttendanceStatus.Attended),
                Att(CbTaiChinh, "CB TC-KT", OrgName, AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(ChuTichUbMttq, ChucVuCtMttq, "MTTQ xã", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(TruongThon[2], "Trưởng bản", TenBan[2], AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(TruongThon[3], "Trưởng bản", TenBan[3], AttendeeRole.Attendee, AttendanceStatus.Attended),
            },
            Content = "5/7 hộ đã đồng ý bồi thường. 2 hộ (ông Lô Văn Thanh, bà Hà Thị Ngân) chưa đồng ý vì đơn giá thấp.",
            Conclusion = "PCT KT-HT làm việc trực tiếp với 2 hộ. MTTQ, trưởng bản vận động. Deadline GPMB: +30 ngày.",
            PersonalNotes = "Vụ khá phức tạp, 2 hộ kiên quyết. Có thể cần xin ý kiến thành phố.",
            Tasks = new List<MeetingTask>
            {
                // Task đang làm — chưa đến hạn
                Task("Làm việc với hộ ông Lô Văn Thanh", PctKtHt, "UBND xã", Today.AddDays(12), MeetingTaskStatus.InProgress, null, 5),
                // Task đang làm — chưa đến hạn
                Task("Vận động bà Hà Thị Ngân", ChuTichUbMttq, "MTTQ", Today.AddDays(12), MeetingTaskStatus.InProgress, null, 5),
                // Task QUÁ HẠN — tạo cảnh báo đỏ!
                Task("Hoàn thiện hồ sơ GPMB trình TP phê duyệt", CbDiaChinh, "ĐC-XD", Today.AddDays(-2), MeetingTaskStatus.InProgress, null, 4),
            },
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM họp chuyên đề GPMB", "08/GM-UBND", d.AddDays(-3)),
                Doc(MeetingDocumentType.TaiLieuHop, "BC tiến độ GPMB", "", d.AddDays(-1)),
                Doc(MeetingDocumentType.TaiLieuHop, "Phương án bồi thường, hỗ trợ TĐC", "", d.AddDays(-1)),
                Doc(MeetingDocumentType.VanBanChiDao, "QĐ phê duyệt DA đường Bản Vẽ - Na Loi", "456/QĐ-UBND", null, "UBND TP Tương Dương"),
                Doc(MeetingDocumentType.BienBan, "BB họp chuyên đề GPMB", "", d),
            },
            Tags = new[] { "GPMB", "đường liên bản", "bồi thường", "Bản Vẽ" }
        };
    }
    
    private Meeting Recent_HopLienNganhPCTT()
    {
        var d = WorkDay(-1);
        return new Meeting
        {
            Title = "Họp liên ngành triển khai KH PCTT&TKCN năm nay",
            MeetingNumber = "07/GM-UBND",
            Type = MeetingType.HopLienNganh,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Completed,
            Priority = 4,
            StartTime = d.AddHours(8),
            EndTime = d.AddHours(11),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.KetHop,
            OnlineLink = "https://meet.google.com/abc-defg-hij",
            ChairPerson = PctKtHt,
            ChairPersonTitle = "Phó Trưởng BCH PCTT xã",
            Secretary = CbDiaChinh,
            OrganizingUnit = "BCH PCTT&TKCN xã Hòa Bình",
            Attendees = new List<MeetingAttendee>
            {
                Att(PctKtHt, "Phó Trưởng BCH", "BCH PCTT", AttendeeRole.ChairPerson, AttendanceStatus.Attended),
                Att(CaTruongCa, "Thành viên BCH", "CA xã", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(XaDoiTruong, "Thành viên BCH", "Ban CHQS xã", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(TramTruong, "Trạm trưởng TYT", "Trạm y tế xã", AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(TruongThon[0], "Trưởng bản", TenBan[0], AttendeeRole.Attendee, AttendanceStatus.Attended),
                Att(TruongThon[1], "Trưởng bản", TenBan[1], AttendeeRole.Attendee, AttendanceStatus.Attended),
            },
            Conclusion = "Giao CA + CHQS lập KH ứng trực. Trạm y tế chuẩn bị thuốc, vật tư dự phòng.",
            Tasks = new List<MeetingTask>
            {
                Task("Lập KH ứng trực mùa mưa bão", CaTruongCa, "CA xã", Today.AddDays(7), MeetingTaskStatus.NotStarted, null, 4),
                Task("Chuẩn bị thuốc, vật tư y tế dự phòng", TramTruong, "TYT xã", Today.AddDays(14), MeetingTaskStatus.NotStarted, null, 3),
            },
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM họp liên ngành PCTT", "07/GM-UBND", d.AddDays(-3)),
                Doc(MeetingDocumentType.TaiLieuHop, "Dự thảo KH PCTT&TKCN", "", d.AddDays(-2)),
                Doc(MeetingDocumentType.VanBanChiDao, "CT về công tác PCTT", "05/CT-UBND", null, "UBND TP Tương Dương"),
                Doc(MeetingDocumentType.BienBan, "BB họp liên ngành PCTT", "", d),
            },
            Tags = new[] { "PCTT", "phòng chống thiên tai", "liên ngành" }
        };
    }
    
    private Meeting Today_HopGiaoBanSangNay()
    {
        // Nếu hôm nay là T7/CN → dời sang thứ 2
        var d = Today;
        if (d.DayOfWeek == DayOfWeek.Saturday) d = d.AddDays(2);
        if (d.DayOfWeek == DayOfWeek.Sunday) d = d.AddDays(1);
        
        var now = DateTime.Now;
        bool daDienRa = now.Hour >= 9; // Sau 9h = đã xong
        
        return new Meeting
        {
            Title = $"Họp giao ban sáng thứ Hai ({d:dd/MM})",
            Type = MeetingType.HopGiaoBan,
            Level = MeetingLevel.CapDonVi,
            Status = daDienRa ? MeetingStatus.Completed : MeetingStatus.InProgress,
            Priority = 3,
            StartTime = d.AddHours(7).AddMinutes(30),
            EndTime = d.AddHours(8).AddMinutes(30),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbVpUbnd,
            OrganizingUnit = OrgName,
            Attendees = new List<MeetingAttendee>
            {
                Att(ChuTich, ChucVuChuTich, OrgName, AttendeeRole.ChairPerson, daDienRa ? AttendanceStatus.Attended : AttendanceStatus.Confirmed),
                Att(PctVhXh, ChucVuPctVhXh, OrgName, AttendeeRole.Attendee, daDienRa ? AttendanceStatus.Attended : AttendanceStatus.Confirmed),
                Att(PctKtHt, ChucVuPctKtHt, OrgName, AttendeeRole.Attendee, daDienRa ? AttendanceStatus.Attended : AttendanceStatus.Confirmed),
                Att(CbVpUbnd, "VP-TK", OrgName, AttendeeRole.Secretary, daDienRa ? AttendanceStatus.Attended : AttendanceStatus.Confirmed),
            },
            Agenda = "1. Đánh giá tuần qua\n2. Phân công tuần này\n3. Phát sinh",
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GB sáng thứ Hai (Zalo)", "", null, OrgName, "Lịch cố định"),
            },
            Tags = new[] { "giao ban", "tuần", "hôm nay" }
        };
    }
    
    private Meeting Today_HopChuyenDeChieuNay()
    {
        var d = Today;
        var now = DateTime.Now;
        bool dangDienRa = now.Hour >= 14 && now.Hour < 16;
        bool daDienRa = now.Hour >= 16;
        
        return new Meeting
        {
            Title = "Họp chuyên đề rà soát hộ nghèo, cận nghèo",
            MeetingNumber = "14/GM-UBND",
            Type = MeetingType.HopChuyenDe,
            Level = MeetingLevel.CapXa,
            Status = daDienRa ? MeetingStatus.Completed : dangDienRa ? MeetingStatus.InProgress : MeetingStatus.Scheduled,
            Priority = 4,
            StartTime = d.AddHours(14),
            EndTime = d.AddHours(16),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = PctVhXh,
            ChairPersonTitle = ChucVuPctVhXh,
            Secretary = CbLdTbXh,
            OrganizingUnit = OrgName,
            Attendees = new List<MeetingAttendee>
            {
                Att(PctVhXh, ChucVuPctVhXh, OrgName, AttendeeRole.ChairPerson, AttendanceStatus.Confirmed),
                Att(CbLdTbXh, "CB LĐ-TB&XH", OrgName, AttendeeRole.Presenter, AttendanceStatus.Confirmed),
                Att(CbTaiChinh, "CB TC-KT", OrgName, AttendeeRole.Attendee, AttendanceStatus.Confirmed),
                Att(ChuTichHoiPN, "CT Hội LHPN", "Hội Phụ nữ", AttendeeRole.Attendee, AttendanceStatus.Confirmed),
                Att(TruongThon[0], "Trưởng bản", TenBan[0], AttendeeRole.Attendee, AttendanceStatus.Invited),
                Att(TruongThon[1], "Trưởng bản", TenBan[1], AttendeeRole.Attendee, AttendanceStatus.Invited),
                Att(TruongThon[4], "Trưởng thôn", TenBan[4], AttendeeRole.Attendee, AttendanceStatus.Invited),
            },
            Agenda = "1. Rà soát danh sách hộ nghèo, cận nghèo\n2. Bình xét bổ sung/đưa ra\n3. Chính sách hỗ trợ đợt 1",
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM họp chuyên đề hộ nghèo", "14/GM-UBND", d.AddDays(-3)),
                Doc(MeetingDocumentType.TaiLieuHop, "DS hộ nghèo, cận nghèo hiện hành", "", d.AddDays(-1)),
            },
            Tags = new[] { "hộ nghèo", "cận nghèo", "LĐ-TB&XH", "hôm nay" }
        };
    }
    
    // ═══════════════════════════════════════════════════════════════
    //  ĐẶC BIỆT — HOÃN / HỦY
    // ═══════════════════════════════════════════════════════════════
    
    private Meeting Special_HopHoanLai()
    {
        var d = WorkDay(2);
        return new Meeting
        {
            Title = "Họp Ban chỉ đạo phòng chống dịch bệnh gia súc",
            MeetingNumber = "16/GM-UBND",
            Type = MeetingType.HopBanChiDao,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Postponed,
            Priority = 4,
            StartTime = d.AddHours(8),
            EndTime = d.AddHours(11),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = PctKtHt,
            ChairPersonTitle = ChucVuPctKtHt,
            Secretary = CbDiaChinh,
            OrganizingUnit = OrgName,
            PersonalNotes = "Hoãn do đ/c PCT KT-HT đi công tác thành phố đột xuất. Dự kiến dời sang tuần sau.",
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM họp BCĐ phòng chống dịch gia súc", "16/GM-UBND", d.AddDays(-3)),
            },
            Tags = new[] { "hoãn", "dịch bệnh", "gia súc", "BCĐ" }
        };
    }
    
    private Meeting Special_HopDaHuy()
    {
        var d = WorkDay(4);
        return new Meeting
        {
            Title = "Họp phối hợp với Phòng TN&MT thành phố về cấp GCN",
            MeetingNumber = "17/GM-UBND",
            Type = MeetingType.HopLienNganh,
            Level = MeetingLevel.LienNganh,
            Status = MeetingStatus.Cancelled,
            Priority = 3,
            StartTime = d.AddHours(14),
            EndTime = d.AddHours(16),
            Location = "Phòng họp Phòng TN&MT TP Tương Dương",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = "Trần Văn Nam",
            ChairPersonTitle = "Trưởng phòng TN&MT",
            OrganizingUnit = "Phòng TN&MT TP Tương Dương",
            PersonalNotes = "Đã hủy do Phòng TN&MT có lịch họp tỉnh. Chưa có lịch họp lại.",
            Attendees = new List<MeetingAttendee>
            {
                Att(PctKtHt, ChucVuPctKtHt, OrgName, AttendeeRole.Attendee, AttendanceStatus.Invited),
                Att(CbDiaChinh, "CB ĐC-XD", OrgName, AttendeeRole.Attendee, AttendanceStatus.Invited),
            },
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM họp phối hợp cấp GCN", "17/GM-UBND", d.AddDays(-5)),
            },
            Tags = new[] { "đã hủy", "TN&MT", "GCN", "cấp huyện" }
        };
    }
    
    // ═══════════════════════════════════════════════════════════════
    //  SẮP TỚI — 1-10 NGÀY TỚI
    // ═══════════════════════════════════════════════════════════════
    
    private Meeting Soon_HopDangUy()
    {
        var d = WorkDay(1);
        return new Meeting
        {
            Title = "Họp Đảng ủy xã Hòa Bình định kỳ",
            MeetingNumber = "03/GM-ĐU",
            Type = MeetingType.HopDangUy,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Scheduled,
            Priority = 5,
            StartTime = d.AddHours(14),
            EndTime = d.AddHours(17),
            Location = "Phòng họp Đảng ủy xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = BiThuDang,
            ChairPersonTitle = ChucVuBiThu,
            Secretary = ChuTich,
            OrganizingUnit = "Đảng ủy xã Hòa Bình",
            Attendees = new List<MeetingAttendee>
            {
                Att(BiThuDang, ChucVuBiThu, "Đảng ủy xã", AttendeeRole.ChairPerson, AttendanceStatus.Confirmed),
                Att(ChuTich, "Phó BT Đảng ủy", "Đảng ủy xã", AttendeeRole.Secretary, AttendanceStatus.Confirmed),
                Att(ChuTichHdnd, "UV BTV", "Đảng ủy xã", AttendeeRole.Attendee, AttendanceStatus.Confirmed),
                Att(PctVhXh, "Đảng ủy viên", "Đảng ủy xã", AttendeeRole.Attendee, AttendanceStatus.Confirmed),
                Att(PctKtHt, "Đảng ủy viên", "Đảng ủy xã", AttendeeRole.Attendee, AttendanceStatus.Confirmed),
                Att(ChuTichUbMttq, "Đảng ủy viên", "Đảng ủy xã", AttendeeRole.Attendee, AttendanceStatus.Invited),
            },
            Agenda = "1. Đánh giá kết quả lãnh đạo tháng qua\n2. Công tác tổ chức, cán bộ\n3. Chuẩn bị ĐH các chi bộ\n4. Phương hướng tháng tới",
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM họp Đảng ủy xã", "03/GM-ĐU", d.AddDays(-4)),
                Doc(MeetingDocumentType.TaiLieuHop, "BC kết quả lãnh đạo tháng", "", d.AddDays(-2)),
            },
            Tags = new[] { "Đảng ủy", "lãnh đạo" }
        };
    }
    
    private Meeting Soon_TapHuanChuyenDoiSo()
    {
        var d = WorkDay(3);
        return new Meeting
        {
            Title = "Tập huấn chuyển đổi số cho cán bộ xã Hòa Bình",
            MeetingNumber = "12/GM-UBND",
            Type = MeetingType.TapHuan,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Scheduled,
            Priority = 3,
            StartTime = d.AddHours(8),
            EndTime = d.AddHours(16).AddMinutes(30),
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
                Att(PctVhXh, ChucVuPctVhXh, OrgName, AttendeeRole.ChairPerson, AttendanceStatus.Confirmed),
                Att("ThS. Nguyễn Văn Hùng", "Chuyên viên CNTT", "Sở TT&TT Nghệ An", AttendeeRole.Presenter, AttendanceStatus.Confirmed, "Báo cáo viên chính"),
                Att(CbVpUbnd, "VP-TK", OrgName, AttendeeRole.Attendee, AttendanceStatus.Invited),
                Att(CbDiaChinh, "CB ĐC-XD", OrgName, AttendeeRole.Attendee, AttendanceStatus.Invited),
                Att(CbTuPhap, "CB Tư pháp", OrgName, AttendeeRole.Attendee, AttendanceStatus.Invited),
                Att(CbTaiChinh, "CB TC-KT", OrgName, AttendeeRole.Attendee, AttendanceStatus.Invited),
            },
            Agenda = "SÁNG: Tổng quan CĐS; DVC trực tuyến; Thực hành.\nCHIỀU: Ký số, chữ ký ĐT; QLVB điện tử; Hỏi đáp.",
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM tập huấn CĐS", "12/GM-UBND", d.AddDays(-7)),
                Doc(MeetingDocumentType.ChuongTrinh, "Chương trình tập huấn CĐS", "", d.AddDays(-7)),
                Doc(MeetingDocumentType.TaiLieuHop, "Tài liệu: Cổng DVCTT cấp xã", "", null, "Sở TT&TT"),
                Doc(MeetingDocumentType.VanBanChiDao, "KH CĐS tỉnh Nghệ An", "15/KH-UBND", null, "UBND tỉnh"),
            },
            Tags = new[] { "chuyển đổi số", "tập huấn", "CNTT" }
        };
    }
    
    private Meeting Soon_HopXetKhenThuong()
    {
        var d = WorkDay(5);
        return new Meeting
        {
            Title = "Họp Hội đồng Thi đua - Khen thưởng xã quý I",
            MeetingNumber = "11/GM-UBND",
            Type = MeetingType.HopCoQuan,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Scheduled,
            Priority = 3,
            StartTime = d.AddHours(14),
            EndTime = d.AddHours(16),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = "CT HĐ TĐ-KT xã",
            Secretary = CbVhXh,
            OrganizingUnit = "HĐ TĐ-KT xã Hòa Bình",
            Attendees = new List<MeetingAttendee>
            {
                Att(ChuTich, "CT HĐ TĐ-KT", OrgName, AttendeeRole.ChairPerson, AttendanceStatus.Invited),
                Att(PctVhXh, "Phó CT HĐ", OrgName, AttendeeRole.Attendee, AttendanceStatus.Invited),
                Att(CbVhXh, "Thành viên HĐ", OrgName, AttendeeRole.Secretary, AttendanceStatus.Invited),
                Att(ChuTichUbMttq, "Thành viên HĐ", "MTTQ xã", AttendeeRole.Attendee, AttendanceStatus.Invited),
            },
            Agenda = "1. Xét DS đề nghị khen thưởng quý I\n2. Bình xét danh hiệu thi đua\n3. Biểu quyết",
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM họp HĐ TĐ-KT quý I", "11/GM-UBND", d.AddDays(-5)),
                Doc(MeetingDocumentType.TaiLieuHop, "DS đề nghị khen thưởng quý I", "", d.AddDays(-3)),
            },
            Tags = new[] { "khen thưởng", "thi đua", "quý I" }
        };
    }
    
    private Meeting Soon_HopThuongKyThangNay()
    {
        var d = WorkDay(7);
        return new Meeting
        {
            Title = "Họp UBND xã thường kỳ tháng này",
            MeetingNumber = "20/GM-UBND",
            Type = MeetingType.HopThuongKy,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Scheduled,
            Priority = 4,
            StartTime = d.AddHours(8),
            EndTime = d.AddHours(11).AddMinutes(30),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbVpUbnd,
            OrganizingUnit = OrgFull,
            Attendees = new List<MeetingAttendee>
            {
                Att(ChuTich, ChucVuChuTich, OrgName, AttendeeRole.ChairPerson, AttendanceStatus.Confirmed),
                Att(PctVhXh, ChucVuPctVhXh, OrgName, AttendeeRole.Attendee, AttendanceStatus.Confirmed),
                Att(PctKtHt, ChucVuPctKtHt, OrgName, AttendeeRole.Attendee, AttendanceStatus.Confirmed),
                Att(CbVpUbnd, "VP-TK", OrgName, AttendeeRole.Secretary, AttendanceStatus.Confirmed),
                Att(CbDiaChinh, "CB ĐC-XD", OrgName, AttendeeRole.Attendee, AttendanceStatus.Invited),
                Att(CbTaiChinh, "CB TC-KT", OrgName, AttendeeRole.Attendee, AttendanceStatus.Invited),
                Att(CaTruongCa, "Trưởng CA xã", "CA xã", AttendeeRole.Attendee, AttendanceStatus.Invited),
            },
            Agenda = "1. Đánh giá nhiệm vụ tháng\n2. BC thu-chi NS\n3. Tiến độ GPMB\n4. SX vụ Xuân\n5. ANTT",
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM họp UBND thường kỳ tháng này", "20/GM-UBND", d.AddDays(-3)),
            },
            Tags = new[] { "thường kỳ", "UBND" }
        };
    }
    
    private Meeting Soon_HopTrienKhaiSanXuat()
    {
        var d = WorkDay(10);
        return new Meeting
        {
            Title = "Họp triển khai kế hoạch sản xuất nông nghiệp vụ Xuân",
            MeetingNumber = "09/GM-UBND",
            Type = MeetingType.HopTrienKhai,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Scheduled,
            Priority = 4,
            StartTime = d.AddHours(8),
            EndTime = d.AddHours(11),
            Location = "Hội trường UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = PctKtHt,
            ChairPersonTitle = ChucVuPctKtHt,
            Secretary = CbDiaChinh,
            OrganizingUnit = OrgName,
            Attendees = new List<MeetingAttendee>
            {
                Att(PctKtHt, ChucVuPctKtHt, OrgName, AttendeeRole.ChairPerson, AttendanceStatus.Confirmed),
                Att(CbDiaChinh, "CB ĐC-XD", OrgName, AttendeeRole.Presenter, AttendanceStatus.Confirmed),
                Att(ChuTichHoiND, "CT Hội ND", "Hội Nông dân", AttendeeRole.Attendee, AttendanceStatus.Confirmed),
                Att(TruongThon[0], "Trưởng bản", TenBan[0], AttendeeRole.Attendee, AttendanceStatus.Invited),
                Att(TruongThon[1], "Trưởng bản", TenBan[1], AttendeeRole.Attendee, AttendanceStatus.Invited),
                Att(TruongThon[2], "Trưởng bản", TenBan[2], AttendeeRole.Attendee, AttendanceStatus.Invited),
                Att(TruongThon[3], "Trưởng bản", TenBan[3], AttendeeRole.Attendee, AttendanceStatus.Invited),
                Att(TruongThon[4], "Trưởng thôn", TenBan[4], AttendeeRole.Attendee, AttendanceStatus.Invited),
            },
            Agenda = "1. KH lúa vụ Xuân (diện tích, giống, lịch thời vụ)\n2. Vật tư nông nghiệp\n3. Kỹ thuật mới\n4. Phân công các bản",
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM họp triển khai SX vụ Xuân", "09/GM-UBND", d.AddDays(-5)),
                Doc(MeetingDocumentType.TaiLieuHop, "KH sản xuất NN vụ Xuân", "05/KH-UBND", d.AddDays(-3)),
                Doc(MeetingDocumentType.VanBanChiDao, "HD sản xuất vụ Xuân", "15/HD-NNPTNT", null, "Sở NN&PTNT"),
            },
            Tags = new[] { "nông nghiệp", "vụ Xuân", "sản xuất" }
        };
    }
    
    // ═══════════════════════════════════════════════════════════════
    //  TƯƠNG LAI — 2-5 TUẦN TỚI
    // ═══════════════════════════════════════════════════════════════
    
    private Meeting Future_TiepCongDanThangSau()
    {
        var d = WorkDay(15);
        return new Meeting
        {
            Title = "Tiếp công dân định kỳ tháng tới",
            MeetingNumber = "25/TB-UBND",
            Type = MeetingType.TiepCongDan,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Scheduled,
            Priority = 4,
            StartTime = d.AddHours(8),
            EndTime = d.AddHours(11),
            Location = "Phòng tiếp dân UBND xã",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbTuPhap,
            OrganizingUnit = OrgName,
            Attendees = new List<MeetingAttendee>
            {
                Att(ChuTich, ChucVuChuTich, OrgName, AttendeeRole.ChairPerson, AttendanceStatus.Invited),
                Att(CbTuPhap, "CB Tư pháp", OrgName, AttendeeRole.Secretary, AttendanceStatus.Invited),
                Att(CbDiaChinh, "CB ĐC-XD", OrgName, AttendeeRole.Attendee, AttendanceStatus.Invited),
            },
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "TB lịch tiếp công dân", "25/TB-UBND", d.AddDays(-5)),
            },
            Tags = new[] { "tiếp dân" }
        };
    }
    
    private Meeting Future_HoiNghiNhanDanBan()
    {
        var d = WorkDay(19);
        return new Meeting
        {
            Title = "Hội nghị nhân dân bản Na Hang về đường giao thông nội bản",
            MeetingNumber = "15/GM-UBND",
            Type = MeetingType.HoiNghi,
            Level = MeetingLevel.CapDonVi,
            Status = MeetingStatus.Scheduled,
            Priority = 3,
            StartTime = d.AddHours(19), // Tối — vì nhân dân đi làm ban ngày
            EndTime = d.AddHours(21),
            Location = "Nhà văn hóa bản Na Hang",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = TruongThon[0],
            ChairPersonTitle = "Trưởng bản Na Hang",
            Secretary = CbDiaChinh,
            OrganizingUnit = "UBND xã phối hợp bản Na Hang",
            Attendees = new List<MeetingAttendee>
            {
                Att(TruongThon[0], "Trưởng bản", TenBan[0], AttendeeRole.ChairPerson, AttendanceStatus.Invited),
                Att(PctKtHt, ChucVuPctKtHt, OrgName, AttendeeRole.Attendee, AttendanceStatus.Invited),
                Att(CbDiaChinh, "CB ĐC-XD", OrgName, AttendeeRole.Secretary, AttendanceStatus.Invited),
            },
            Agenda = "1. Thông báo chủ trương XD đường bê tông\n2. Lấy ý kiến nhân dân\n3. Vận động đóng góp, hiến đất\n4. Biểu quyết",
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM hội nghị nhân dân Na Hang", "15/GM-UBND", d.AddDays(-7)),
                Doc(MeetingDocumentType.TaiLieuHop, "Phương án XD đường bê tông nội bản", "", d.AddDays(-5)),
            },
            Tags = new[] { "nhân dân", "Na Hang", "đường bê tông", "NTM" }
        };
    }
    
    private Meeting Future_HopSoKetQuy()
    {
        var d = WorkDay(24);
        return new Meeting
        {
            Title = "Họp sơ kết công tác quý I và triển khai nhiệm vụ quý II",
            MeetingNumber = "",
            Type = MeetingType.HopSoKet,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Scheduled,
            Priority = 4,
            StartTime = d.AddHours(8),
            EndTime = d.AddHours(11).AddMinutes(30),
            Location = "Trực tuyến qua Google Meet",
            Format = MeetingFormat.TrucTuyen,
            OnlineLink = "https://meet.google.com/xyz-uvw-rst",
            ChairPerson = ChuTich,
            ChairPersonTitle = ChucVuChuTich,
            Secretary = CbVpUbnd,
            OrganizingUnit = OrgFull,
            Attendees = new List<MeetingAttendee>
            {
                Att(ChuTich, ChucVuChuTich, OrgName, AttendeeRole.ChairPerson, AttendanceStatus.Invited),
                Att(PctVhXh, ChucVuPctVhXh, OrgName, AttendeeRole.Attendee, AttendanceStatus.Invited),
                Att(PctKtHt, ChucVuPctKtHt, OrgName, AttendeeRole.Attendee, AttendanceStatus.Invited),
                Att(CbVpUbnd, "VP-TK", OrgName, AttendeeRole.Secretary, AttendanceStatus.Invited),
                Att(CbTaiChinh, "CB TC-KT", OrgName, AttendeeRole.Presenter, AttendanceStatus.Invited),
            },
            Agenda = "1. BC kết quả quý I\n2. BC thu-chi NS quý I\n3. Tiến độ công trình\n4. Nhiệm vụ trọng tâm quý II",
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM họp sơ kết quý I", "", d.AddDays(-5), OrgName, "Sẽ phát hành"),
            },
            Tags = new[] { "sơ kết", "quý I", "trực tuyến" }
        };
    }
    
    private Meeting Future_HopChiBoDinhKy()
    {
        var d = WorkDay(28);
        return new Meeting
        {
            Title = "Sinh hoạt Chi bộ CQ UBND xã tháng tới",
            MeetingNumber = "",
            Type = MeetingType.HopChiBo,
            Level = MeetingLevel.CapDonVi,
            Status = MeetingStatus.Scheduled,
            Priority = 4,
            StartTime = d.AddHours(14),
            EndTime = d.AddHours(16).AddMinutes(30),
            Location = "Phòng họp UBND xã Hòa Bình",
            Format = MeetingFormat.TrucTiep,
            ChairPerson = BiThuDang,
            ChairPersonTitle = "Bí thư Chi bộ",
            Secretary = CbVpUbnd,
            OrganizingUnit = "Chi bộ CQ UBND xã",
            Attendees = new List<MeetingAttendee>
            {
                Att(BiThuDang, "Bí thư Chi bộ", "Chi bộ CQ", AttendeeRole.ChairPerson, AttendanceStatus.Invited),
                Att(ChuTich, "Phó BT Chi bộ", "Chi bộ CQ", AttendeeRole.Attendee, AttendanceStatus.Invited),
            },
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM sinh hoạt Chi bộ tháng tới", "", d.AddDays(-3)),
            },
            Tags = new[] { "chi bộ", "đảng", "sinh hoạt" }
        };
    }
    
    private Meeting Future_HopLienNganhGiaoDuc()
    {
        var d = WorkDay(35);
        return new Meeting
        {
            Title = "Họp liên ngành rà soát cơ sở vật chất trường học trước năm học mới",
            MeetingNumber = "",
            Type = MeetingType.HopLienNganh,
            Level = MeetingLevel.CapXa,
            Status = MeetingStatus.Scheduled,
            Priority = 3,
            StartTime = d.AddHours(8),
            EndTime = d.AddHours(11),
            Location = "Hội trường UBND xã Hòa Bình",
            Format = MeetingFormat.KetHop,
            OnlineLink = "https://meet.google.com/gd-abc-123",
            ChairPerson = PctVhXh,
            ChairPersonTitle = ChucVuPctVhXh,
            Secretary = CbVhXh,
            OrganizingUnit = OrgName,
            Attendees = new List<MeetingAttendee>
            {
                Att(PctVhXh, ChucVuPctVhXh, OrgName, AttendeeRole.ChairPerson, AttendanceStatus.Invited),
                Att(CbVhXh, "CB VH-XH", OrgName, AttendeeRole.Secretary, AttendanceStatus.Invited),
                Att("Nguyễn Thị Hồng", "Hiệu trưởng TH", "Trường TH Hòa Bình", AttendeeRole.Presenter, AttendanceStatus.Invited),
                Att("Lê Đức Anh", "Hiệu trưởng THCS", "Trường THCS Hòa Bình", AttendeeRole.Presenter, AttendanceStatus.Invited),
            },
            Agenda = "1. Rà soát CSVC trường TH, THCS\n2. Nhu cầu sửa chữa, mua sắm\n3. Nguồn kinh phí, XHH GD\n4. KH chuẩn bị năm học mới",
            Documents = new List<MeetingDocument>
            {
                Doc(MeetingDocumentType.GiayMoi, "GM họp liên ngành giáo dục", "", d.AddDays(-7)),
            },
            Tags = new[] { "giáo dục", "trường học", "CSVC", "liên ngành" }
        };
    }
    
    // ═══════════════════════════════════════════════════════════════
    //  FACTORY HELPERS — Rút gọn code tạo Attendee, Task, Document
    // ═══════════════════════════════════════════════════════════════
    
    private static MeetingAttendee Att(string name, string pos, string unit, AttendeeRole role, AttendanceStatus status, string? note = null)
        => new() { Name = name, Position = pos, Unit = unit, Role = role, AttendanceStatus = status, Note = note ?? "" };
    
    private static MeetingTask Task(string title, string assignedTo, string unit, DateTime deadline, MeetingTaskStatus status, DateTime? completionDate, int priority)
        => new() { Title = title, AssignedTo = assignedTo, AssignedUnit = unit, Deadline = deadline, TaskStatus = status, CompletionDate = completionDate, Priority = priority };
    
    private static MeetingDocument Doc(MeetingDocumentType type, string title, string number, DateTime? issuedDate, string? issuer = null, string? note = null)
        => new() { DocumentType = type, Title = title, DocumentNumber = number, IssuedDate = issuedDate, Issuer = issuer ?? OrgName, Note = note ?? "" };
}
