namespace AIVanBan.Core.Models;

/// <summary>
/// Folder/Thư mục tùy chỉnh
/// </summary>
public class Folder
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty; // "Văn bản/Công văn đi/2024"
    public string ParentId { get; set; } = string.Empty;
    public string Icon { get; set; } = "📁";
    public string Color { get; set; } = "#1976D2"; // Color for UI
    public int SortOrder { get; set; } = 0; // Thứ tự hiển thị
    public int DocumentCount { get; set; }
    public string OrganizationName { get; set; } = string.Empty; // Tên cơ quan tạo folder này
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}

/// <summary>
/// Cấu hình cơ quan (cho tính năng setup ban đầu)
/// </summary>
public class OrganizationConfig
{
    public string Id { get; set; } = "default";
    public string Name { get; set; } = string.Empty; // "UBND Xã Hòa Bình"
    public OrganizationType Type { get; set; }
    public string[] Departments { get; set; } = Array.Empty<string>(); // Danh sách phòng ban
    public string FolderStructure { get; set; } = string.Empty; // JSON cấu trúc thư mục
    public DateTime SetupDate { get; set; } = DateTime.Now;
}

/// <summary>
/// Loại cơ quan
/// </summary>
public enum OrganizationType
{
    // === CƠ QUAN CHÍNH QUYỀN (2 CẤP: TỈNH - XÃ) ===
    UbndXa,             // UBND Xã/Phường/Thị trấn
    UbndTinh,           // UBND Tỉnh/Thành phố
    HdndXa,             // HĐND Xã/Phường/Thị trấn
    HdndTinh,           // HĐND Tỉnh/Thành phố
    VanPhong,           // Văn phòng UBND/HĐND
    TrungTamHanhChinh,  // Trung tâm Hành chính công
    
    // === CƠ QUAN ĐẢNG ===
    DangUyXa,           // Đảng ủy Xã/Phường/Thị trấn
    DangUyTinh,         // Tỉnh ủy/Thành ủy
    ChiBoDang,          // Chi bộ Đảng
    DangBo,             // Đảng bộ cơ quan
    
    // === MẶT TRẬN - ĐOÀN THỂ ===
    MatTran,            // Mặt trận Tổ quốc
    HoiNongDan,         // Hội Nông dân
    HoiPhuNu,           // Hội Liên hiệp Phụ nữ
    DoanThanhNien,      // Đoàn TNCS Hồ Chí Minh
    HoiCuuChienBinh,    // Hội Cựu chiến binh
    CongDoan,           // Công đoàn
    HoiChapThap,        // Hội Chữ thập đỏ
    HoiKhuyenHoc,       // Hội Khuyến học
    
    // === SỞ - BAN - NGÀNH CẤP TỈNH ===
    SoNoiVu,            // Sở Nội vụ
    SoTaiChinh,         // Sở Tài chính
    SoKhoHo,            // Sở Kế hoạch & Đầu tư
    SoGiaoDuc,          // Sở Giáo dục & Đào tạo
    SoYTe,              // Sở Y tế
    SoNongNghiep,       // Sở Nông nghiệp & PTNT
    SoCongThuong,       // Sở Công thương
    SoVanHoa,           // Sở Văn hóa, Thể thao & Du lịch
    SoTaiNguyen,        // Sở Tài nguyên & Môi trường
    SoXayDung,          // Sở Xây dựng
    SoGiaoThong,        // Sở Giao thông Vận tải
    SoTuPhap,           // Sở Tư pháp
    SoThongTin,         // Sở Thông tin & Truyền thông
    SoLaoDong,          // Sở Lao động TBXH
    SoKhoaHoc,          // Sở Khoa học & Công nghệ
    
    // === BAN ĐẢNG - BAN CỦA TỈNH ỦY ===
    BanDanVan,          // Ban Dân vận Tỉnh ủy
    BanToChuc,          // Ban Tổ chức Tỉnh ủy
    BanTuyenGiao,       // Ban Tuyên giáo Tỉnh ủy
    BanKiemTra,         // Ban Kiểm tra Tỉnh ủy
    BanNoiChinh,        // Ban Nội chính Tỉnh ủy
    BanKinhTe,          // Ban Kinh tế Tỉnh ủy
    BanVanHoa,          // Ban Văn hóa - Xã hội Tỉnh ủy
    
    // === CƠ SỞ GIÁO DỤC ===
    TruongMamNon,       // Trường Mầm non
    TruongTieuHoc,      // Trường Tiểu học
    TruongTHCS,         // Trường THCS
    TruongTHPT,         // Trường THPT
    TruongDaiHoc,       // Trường Đại học/Cao đẳng
    
    // === CƠ SỞ Y TẾ ===
    TramYTe,            // Trạm Y tế Xã
    TrungTamYTe,        // Trung tâm Y tế Huyện
    BenhVien,           // Bệnh viện
    
    // === CƠ QUAN KHÁC ===
    CongAn,             // Công an
    TrungTamVanHoa,     // Trung tâm Văn hóa
    ThuVien,            // Thư viện
    BaoTangVienDi,      // Bảo tàng/Viện/Di tích
    
    // === DOANH NGHIỆP NHÀ NƯỚC ===
    CongTyNhaNuoc,      // Công ty Nhà nước
    
    // === KHÁC ===
    CoQuanTuyChon       // Cơ quan tùy chọn
}
