using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service để setup cơ quan lần đầu - tự động tạo cấu trúc thư mục chuẩn
/// </summary>
public class OrganizationSetupService
{
    private readonly DocumentService _documentService;
    
    public OrganizationSetupService(DocumentService documentService)
    {
        _documentService = documentService;
    }
    
    /// <summary>
    /// Tạo cấu trúc thư mục chuẩn cho cơ quan - theo loại cơ quan cụ thể
    /// </summary>
    public void CreateDefaultStructure(string orgName, OrganizationType orgType)
    {
        try
        {
            Console.WriteLine($"📁 Creating organization-specific folder structure for: {orgName} ({orgType})");
            
            // Xóa tất cả folders cũ nếu có
            var existingFolders = _documentService.GetAllFolders();
            Console.WriteLine($"  Found {existingFolders.Count} existing folders to delete");
            
            foreach (var folder in existingFolders)
            {
                try
                {
                    _documentService.DeleteFolder(folder.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Warning: Could not delete folder {folder.Name}: {ex.Message}");
                }
            }
            
            // Tạo cấu trúc theo từng loại cơ quan
            switch (orgType)
            {
                // === CƠ QUAN CHÍNH QUYỀN ===
                case OrganizationType.UbndXa:
                    CreateStructure_UbndXa(orgName);
                    break;
                    
                case OrganizationType.UbndTinh:
                    CreateStructure_UbndTinh(orgName);
                    break;
                    
                case OrganizationType.HdndXa:
                case OrganizationType.HdndTinh:
                    CreateStructure_HDND(orgName);
                    break;
                    
                case OrganizationType.VanPhong:
                    CreateStructure_VanPhong(orgName);
                    break;
                    
                case OrganizationType.TrungTamHanhChinh:
                    CreateStructure_TrungTamHanhChinh(orgName);
                    break;
                    
                // === CƠ QUAN ĐẢNG ===
                case OrganizationType.DangUyXa:
                case OrganizationType.DangUyTinh:
                case OrganizationType.ChiBoDang:
                case OrganizationType.DangBo:
                    CreateStructure_Dang(orgName);
                    break;
                    
                // === BAN CỦA ĐẢNG ===
                case OrganizationType.BanDanVan:
                case OrganizationType.BanToChuc:
                case OrganizationType.BanTuyenGiao:
                case OrganizationType.BanKiemTra:
                case OrganizationType.BanNoiChinh:
                case OrganizationType.BanKinhTe:
                case OrganizationType.BanVanHoa:
                    CreateStructure_BanCuaDang(orgName);
                    break;
                    
                // === MẶT TRẬN - ĐOÀN THỂ ===
                case OrganizationType.MatTran:
                    CreateStructure_MatTran(orgName);
                    break;
                    
                case OrganizationType.HoiNongDan:
                    CreateStructure_HoiNongDan(orgName);
                    break;
                    
                case OrganizationType.HoiPhuNu:
                    CreateStructure_HoiPhuNu(orgName);
                    break;
                    
                case OrganizationType.DoanThanhNien:
                    CreateStructure_DoanThanhNien(orgName);
                    break;
                    
                case OrganizationType.HoiCuuChienBinh:
                case OrganizationType.CongDoan:
                case OrganizationType.HoiChapThap:
                case OrganizationType.HoiKhuyenHoc:
                    CreateStructure_DoanTheKhac(orgName);
                    break;
                    
                // === SỞ - BAN - NGÀNH ===
                case OrganizationType.SoNoiVu:
                case OrganizationType.SoTaiChinh:
                case OrganizationType.SoKhoHo:
                case OrganizationType.SoGiaoDuc:
                case OrganizationType.SoYTe:
                case OrganizationType.SoNongNghiep:
                case OrganizationType.SoCongThuong:
                case OrganizationType.SoVanHoa:
                case OrganizationType.SoTaiNguyen:
                case OrganizationType.SoXayDung:
                case OrganizationType.SoGiaoThong:
                case OrganizationType.SoTuPhap:
                case OrganizationType.SoThongTin:
                case OrganizationType.SoLaoDong:
                case OrganizationType.SoKhoaHoc:
                    CreateStructure_SoBanNganh(orgName);
                    break;
                    
                // === GIÁO DỤC & Y TẾ ===
                case OrganizationType.TruongMamNon:
                case OrganizationType.TruongTieuHoc:
                case OrganizationType.TruongTHCS:
                case OrganizationType.TruongTHPT:
                case OrganizationType.TruongDaiHoc:
                    CreateStructure_TruongHoc(orgName);
                    break;
                    
                case OrganizationType.TramYTe:
                case OrganizationType.TrungTamYTe:
                case OrganizationType.BenhVien:
                    CreateStructure_YTe(orgName);
                    break;
                    
                // === KHÁC ===
                case OrganizationType.CongAn:
                    CreateStructure_CongAn(orgName);
                    break;
                    
                case OrganizationType.TrungTamVanHoa:
                case OrganizationType.ThuVien:
                case OrganizationType.BaoTangVienDi:
                case OrganizationType.CongTyNhaNuoc:
                    CreateStructure_Generic(orgName);
                    break;
                    
                default:
                    CreateStructure_Generic(orgName);
                    break;
            }
            
            Console.WriteLine("✅ Folder structure created successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR creating folder structure: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw new Exception($"Không thể tạo cấu trúc thư mục: {ex.Message}", ex);
        }
    }
    
    private void Create01_VanBanPhapLuat(string orgName)
    {
        var root = CreateFolder("01. VĂN BẢN PHÁP LUẬT", null, "⚖️", orgName, 1);
        
        CreateSubFolders(root.Id, orgName, new[]
        {
            ("Hiến pháp", "📜"),
            ("Luật", "📕"),
            ("Pháp lệnh", "📘"),
            ("Nghị quyết (Quốc hội, HĐND)", "📗"),
            ("Nghị định (Chính phủ)", "📙"),
            ("Thông tư (Bộ, ngành)", "📑"),
            ("Quyết định (UBND các cấp)", "📋"),
            ("Chỉ thị", "📌"),
            ("Hướng dẫn, Quy định", "📝")
        });
    }
    
    private void Create02_VanBanDi(string orgName)
    {
        var root = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        
        // Tạo folders theo năm (2024 đến hiện tại)
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            var yearFolder = CreateFolder($"[Năm {year}]", root.Id, "📅", orgName);
            
            CreateSubFolders(yearFolder.Id, orgName, new[]
            {
                ("Công văn đi", "📄"),
                ("Quyết định", "📋"),
                ("Thông báo", "📢"),
                ("Báo cáo (gửi cấp trên)", "📊"),
                ("Tờ trình", "📝"),
                ("Kế hoạch", "📅")
            });
        }
    }
    
    private void Create03_VanBanDen(string orgName)
    {
        var root = CreateFolder("03. VĂN BẢN ĐẾN", null, "📥", orgName, 3);
        
        // Tạo folders theo năm + nguồn
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            var yearFolder = CreateFolder($"[Năm {year}]", root.Id, "📅", orgName);
            
            CreateSubFolders(yearFolder.Id, orgName, new[]
            {
                ("Từ Trung ương (Chính phủ, Bộ)", "🏛️"),
                ("Từ cấp Tỉnh (UBND, Sở)", "🏢"),
                ("Từ cấp Huyện (UBND, Phòng)", "🏫"),
                ("Từ các xã/phường", "🏘️"),
                ("Từ tổ chức, cá nhân", "👥")
            });
        }
    }
    
    private void Create04_HoSoCongViec(string orgName, OrganizationType orgType)
    {
        var root = CreateFolder("04. HỒ SƠ CÔNG VIỆC", null, "💼", orgName, 4);
        
        // 1. Nội vụ - Tổ chức
        var nvFolder = CreateFolder("Nội vụ - Tổ chức", root.Id, "👔", orgName);
        CreateSubFolders(nvFolder.Id, orgName, new[]
        {
            ("Biên chế, tuyển dụng", "📋"),
            ("Đào tạo, bồi dưỡng", "🎓"),
            ("Khen thưởng, kỷ luật", "🏆")
        });
        
        // 2. Tài chính - Ngân sách
        var tcFolder = CreateFolder("Tài chính - Ngân sách", root.Id, "💰", orgName);
        CreateSubFolders(tcFolder.Id, orgName, new[]
        {
            ("Dự toán", "📊"),
            ("Quyết toán", "📈"),
            ("Thu chi", "💵")
        });
        
        // 3. Đất đai - Xây dựng
        var ddFolder = CreateFolder("Đất đai - Xây dựng", root.Id, "🏗️", orgName);
        CreateSubFolders(ddFolder.Id, orgName, new[]
        {
            ("Cấp giấy CNQSD đất", "📜"),
            ("Giấy phép xây dựng", "🏠"),
            ("Quy hoạch", "🗺️")
        });
        
        // 4. Văn hóa - Xã hội
        var vhFolder = CreateFolder("Văn hóa - Xã hội", root.Id, "🎭", orgName);
        CreateSubFolders(vhFolder.Id, orgName, new[]
        {
            ("Giáo dục", "🎓"),
            ("Y tế", "🏥"),
            ("Thể thao, văn nghệ", "⚽")
        });
        
        // 5. Kinh tế - Phát triển
        var ktFolder = CreateFolder("Kinh tế - Phát triển", root.Id, "📈", orgName);
        CreateSubFolders(ktFolder.Id, orgName, new[]
        {
            ("Nông nghiệp", "🌾"),
            ("Công nghiệp, thương mại", "🏭"),
            ("Du lịch", "✈️")
        });
        
        // 6. An ninh - Trật tự
        CreateFolder("An ninh - Trật tự", root.Id, "🚔", orgName);
    }
    
    private void Create05_HoSoDuAn(string orgName)
    {
        var root = CreateFolder("05. HỒ SƠ DỰ ÁN - CÔNG TRÌNH", null, "🏗️", orgName, 5);
        
        // Tạo template folder cho dự án mẫu
        var exampleProject = CreateFolder("[Mẫu] Tên dự án", root.Id, "📁", orgName);
        CreateSubFolders(exampleProject.Id, orgName, new[]
        {
            ("Văn bản phê duyệt", "✅"),
            ("Hồ sơ thiết kế", "📐"),
            ("Hợp đồng, thầu", "📝"),
            ("Tiến độ thi công", "⏱️"),
            ("Nghiệm thu", "✔️"),
            ("Album ảnh công trình", "📷")
        });
    }
    
    private void Create06_AlbumAnh(string orgName)
    {
        var root = CreateFolder("06. ALBUM ẢNH - HÌNH ẢNH", null, "📷", orgName, 6);
        
        // Sự kiện - Hội nghị
        var sukienFolder = CreateFolder("Sự kiện - Hội nghị", root.Id, "🎉", orgName);
        CreateSubFolders(sukienFolder.Id, orgName, new[]
        {
            ($"[{DateTime.Now.Year}] Đại hội Đảng bộ", "🎊"),
            ($"[{DateTime.Now.Year}] Lễ khánh thành", "🎗️"),
            ($"[{DateTime.Now.Year}] Hội nghị cán bộ", "👥")
        });
        
        // Hoạt động thường xuyên
        var hoatdongFolder = CreateFolder("Hoạt động thường xuyên", root.Id, "📅", orgName);
        CreateSubFolders(hoatdongFolder.Id, orgName, new[]
        {
            ("Lễ chào cờ", "🇻🇳"),
            ("Sinh hoạt Đảng, Đoàn", "🏛️"),
            ("Họp giao ban", "💼")
        });
        
        // Công trình - Dự án
        var congtrinh = CreateFolder("Công trình - Dự án", root.Id, "🏗️", orgName);
        CreateSubFolders(congtrinh.Id, orgName, new[]
        {
            ("Trước thi công", "📸"),
            ("Trong thi công", "🏗️"),
            ("Sau hoàn thành", "✅")
        });
        
        // Khảo sát - Thực địa
        var khaosat = CreateFolder("Khảo sát - Thực địa", root.Id, "🔍", orgName);
        CreateSubFolders(khaosat.Id, orgName, new[]
        {
            ("Khảo sát đất đai", "🗺️"),
            ("Kiểm tra hiện trường", "📋"),
            ("Làm việc với dân", "👥")
        });
        
        // Văn hóa - Lễ hội
        var vanhoa = CreateFolder("Văn hóa - Lễ hội", root.Id, "🎭", orgName);
        CreateSubFolders(vanhoa.Id, orgName, new[]
        {
            ("Tết Nguyên Đán", "🧧"),
            ("Ngày lễ lớn", "🎊"),
            ("Lễ hội địa phương", "🎉")
        });
        
        // Tập thể - Cá nhân
        var taphte = CreateFolder("Tập thể - Cá nhân", root.Id, "👥", orgName);
        CreateSubFolders(taphte.Id, orgName, new[]
        {
            ("Ảnh tập thể lãnh đạo", "📸"),
            ("Hoạt động CBCC", "👔")
        });
    }
    
    private void Create07_MauVanBan(string orgName)
    {
        var root = CreateFolder("07. MẪU VĂN BẢN - TEMPLATE", null, "📋", orgName, 7);
        
        // Mẫu theo loại
        var mauTheoLoai = CreateFolder("Mẫu theo loại", root.Id, "📄", orgName);
        CreateSubFolders(mauTheoLoai.Id, orgName, new[]
        {
            ("Công văn.docx", "📄"),
            ("Báo cáo.docx", "📊"),
            ("Tờ trình.docx", "📝"),
            ("Quyết định.docx", "📋"),
            ("Kế hoạch.docx", "📅")
        });
        
        // Mẫu theo lĩnh vực
        var mauTheoLinhVuc = CreateFolder("Mẫu theo lĩnh vực", root.Id, "📂", orgName);
        CreateSubFolders(mauTheoLinhVuc.Id, orgName, new[]
        {
            ("Nội vụ", "👔"),
            ("Tài chính", "💰"),
            ("Đất đai", "🏗️"),
            ("Văn hóa - Xã hội", "🎭")
        });
    }
    
    private void Create08_BaoCaoThongKe(string orgName)
    {
        var root = CreateFolder("08. BÁO CÁO - THỐNG KÊ", null, "📊", orgName, 8);
        
        // Báo cáo định kỳ
        var dinhky = CreateFolder("Báo cáo định kỳ", root.Id, "📅", orgName);
        CreateSubFolders(dinhky.Id, orgName, new[]
        {
            ("Tuần", "📆"),
            ("Tháng", "📅"),
            ("Quý", "📊"),
            ("Năm", "📈")
        });
        
        // Báo cáo chuyên đề
        CreateFolder("Báo cáo chuyên đề", root.Id, "📋", orgName);
    }
    
    private void Create09_TaiLieuHocTap(string orgName)
    {
        var root = CreateFolder("09. TÀI LIỆU HỌC TẬP - NGHIỆP VỤ", null, "📚", orgName, 9);
        
        CreateSubFolders(root.Id, orgName, new[]
        {
            ("Tài liệu đào tạo", "🎓"),
            ("Hướng dẫn nghiệp vụ", "📖"),
            ("Sách chuyên ngành", "📕"),
            ("Bài giảng, slide", "📊")
        });
    }
    
    private void Create10_LuuTru(string orgName)
    {
        var root = CreateFolder("10. LƯU TRỮ - ĐÃ HẾT HIỆU LỰC", null, "📦", orgName, 10);
        
        CreateSubFolders(root.Id, orgName, new[]
        {
            ("Văn bản cũ (trước 2020)", "📜"),
            ("Văn bản đã thay thế", "🔄"),
            ("Hồ sơ đã đóng", "📁")
        });
    }
    
    private void Create11_CaNhan(string orgName)
    {
        var root = CreateFolder("11. CÁ NHÂN (Workspace riêng)", null, "👤", orgName, 11);
        
        CreateSubFolders(root.Id, orgName, new[]
        {
            ("Văn bản nháp", "📝"),
            ("Ghi chú công việc", "📋"),
            ("Tài liệu cá nhân", "📄")
        });
    }
    
    // ===============================================
    // CẤU TRÚC THEO TỪNG LOẠI CƠ QUAN
    // ===============================================
    
    private void CreateStructure_UbndXa(string orgName)
    {
        Console.WriteLine("  Creating UBND XÃ/PHƯỜNG structure...");
        
        // 01. VĂN BẢN ĐẾN (theo năm)
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        // 02. VĂN BẢN ĐI (theo năm)
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03. HÀNH CHÍNH - TỔ CHỨC
        var hanhChinh = CreateFolder("03. HÀNH CHÍNH - TỔ CHỨC", null, "🏛️", orgName, 3);
        CreateSubFolders(hanhChinh.Id, orgName, new[]
        {
            ("Tổ chức bộ máy", "⚙️"),
            ("Quản lý cán bộ", "👥"),
            ("Thi đua khen thưởng", "🏆"),
            ("Quản lý con dấu", "🔐")
        });
        
        // 04. TÀI CHÍNH - KẾ TOÁN
        var taiChinh = CreateFolder("04. TÀI CHÍNH - KẾ TOÁN", null, "💰", orgName, 4);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán ngân sách", "📊"),
            ("Quyết toán", "📝"),
            ("Thu chi tài chính", "💵")
        });
        
        // 05. BIÊN BẢN - HỘI NGHỊ
        var bienBan = CreateFolder("05. BIÊN BẢN - HỘI NGHỊ", null, "📋", orgName, 5);
        CreateSubFolders(bienBan.Id, orgName, new[]
        {
            ("HĐND", "🏛️"),
            ("UBND", "⚖️"),
            ("Hội nghị cán bộ", "👥")
        });
        
        // 06. ĐẤT ĐAI - XÂY DỰNG
        var datDai = CreateFolder("06. ĐẤT ĐAI - XÂY DỰNG", null, "🏗️", orgName, 6);
        CreateSubFolders(datDai.Id, orgName, new[]
        {
            ("Quản lý đất đai", "🗺️"),
            ("Giải phóng mặt bằng", "🚜"),
            ("Cấp giấy phép xây dựng", "📄")
        });
        
        // 07. VĂN HÓA - XÃ HỘI
        var vanHoa = CreateFolder("07. VĂN HÓA - XÃ HỘI", null, "🎭", orgName, 7);
        CreateSubFolders(vanHoa.Id, orgName, new[]
        {
            ("Giáo dục đào tạo", "🎓"),
            ("Y tế dân số", "🏥"),
            ("Văn hóa thể thao", "⚽"),
            ("Lao động TBXH", "🤝")
        });
        
        // 08. KINH TẾ
        var kinhTe = CreateFolder("08. KINH TẾ", null, "💼", orgName, 8);
        CreateSubFolders(kinhTe.Id, orgName, new[]
        {
            ("Phát triển kinh tế", "📈"),
            ("Nông nghiệp lâm nghiệp", "🌾"),
            ("Tiểu thương dịch vụ", "🏪")
        });
        
        // 09. QUỐC PHÒNG - AN NINH
        var quocPhong = CreateFolder("09. QUỐC PHÒNG - AN NINH", null, "🛡️", orgName, 9);
        CreateSubFolders(quocPhong.Id, orgName, new[]
        {
            ("Quốc phòng địa phương", "⚔️"),
            ("Công an trật tự", "👮"),
            ("Phòng cháy chữa cháy", "🚒")
        });
        
        // 10. TƯ PHÁP
        var tuPhap = CreateFolder("10. TƯ PHÁP", null, "⚖️", orgName, 10);
        CreateSubFolders(tuPhap.Id, orgName, new[]
        {
            ("Pháp chế", "📜"),
            ("Hộ tịch", "👶"),
            ("Công chứng", "✍️")
        });
        
        // 11. TÀI LIỆU KHÁC
        var taiLieu = CreateFolder("11. TÀI LIỆU KHÁC", null, "📚", orgName, 11);
        CreateSubFolders(taiLieu.Id, orgName, new[]
        {
            ("Báo cáo thống kê", "📊"),
            ("Kế hoạch nhật ký công tác", "📅"),
            ("Lưu trữ lịch sử", "🗄️")
        });
    }
    
    private void CreateStructure_UbndTinh(string orgName)
    {
        Console.WriteLine("  Creating UBND TỈNH structure...");
        
        // 01. VĂN BẢN ĐẾN
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        // 02. VĂN BẢN ĐI
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03. HÀNH CHÍNH - TỔ CHỨC
        var hanhChinh = CreateFolder("03. HÀNH CHÍNH - TỔ CHỨC", null, "🏛️", orgName, 3);
        CreateSubFolders(hanhChinh.Id, orgName, new[]
        {
            ("Tổ chức bộ máy", "⚙️"),
            ("Biên chế cán bộ", "👥"),
            ("Quy hoạch cán bộ", "📋"),
            ("Thi đua khen thưởng", "🏆"),
            ("Quản lý con dấu", "🔐")
        });
        
        // 04. TÀI CHÍNH - KẾ TOÁN - DỰ TOÁN
        var taiChinh = CreateFolder("04. TÀI CHÍNH - KẾ TOÁN - DỰ TOÁN", null, "💰", orgName, 4);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán ngân sách", "📊"),
            ("Phân bổ ngân sách", "💵"),
            ("Quyết toán", "📝"),
            ("Báo cáo tài chính", "📈")
        });
        
        // 05. BIÊN BẢN - HỘI NGHỊ - QUYẾT ĐỊNH
        var bienBan = CreateFolder("05. BIÊN BẢN - HỘI NGHỊ - QUYẾT ĐỊNH", null, "📋", orgName, 5);
        CreateSubFolders(bienBan.Id, orgName, new[]
        {
            ("HĐND huyện", "🏛️"),
            ("UBND huyện", "⚖️"),
            ("Ban thường vụ", "👔"),
            ("Hội nghị CB-VC", "👥")
        });
        
        // 06. QUY HOẠCH - KẾ HOẠCH
        var quyHoach = CreateFolder("06. QUY HOẠCH - KẾ HOẠCH", null, "🗺️", orgName, 6);
        CreateSubFolders(quyHoach.Id, orgName, new[]
        {
            ("Quy hoạch phát triển", "📍"),
            ("Kế hoạch 5 năm", "📅"),
            ("Kế hoạch hàng năm", "📆")
        });
        
        // 07. ĐẤT ĐAI - XÂY DỰNG - ĐÔ THỊ
        var datDai = CreateFolder("07. ĐẤT ĐAI - XÂY DỰNG - ĐÔ THỊ", null, "🏗️", orgName, 7);
        CreateSubFolders(datDai.Id, orgName, new[]
        {
            ("Quản lý đất đai", "🗺️"),
            ("Quy hoạch xây dựng", "📐"),
            ("Cấp GCN quyền sử dụng đất", "📄"),
            ("Quản lý đô thị", "🏙️")
        });
        
        // 08. KINH TẾ
        var kinhTe = CreateFolder("08. KINH TẾ", null, "💼", orgName, 8);
        CreateSubFolders(kinhTe.Id, orgName, new[]
        {
            ("Phát triển kinh tế", "📈"),
            ("Nông nghiệp", "🌾"),
            ("Công nghiệp tiểu thủ công", "🏭"),
            ("Thương mại dịch vụ", "🏪"),
            ("Du lịch", "✈️")
        });
        
        // 09. VĂN HÓA - XÃ HỘI
        var vanHoa = CreateFolder("09. VĂN HÓA - XÃ HỘI", null, "🎭", orgName, 9);
        CreateSubFolders(vanHoa.Id, orgName, new[]
        {
            ("Giáo dục đào tạo", "🎓"),
            ("Y tế", "🏥"),
            ("Văn hóa thể thao", "⚽"),
            ("LĐTBXH", "🤝"),
            ("Dân số dân tộc", "👪")
        });
        
        // 10. QUỐC PHÒNG - AN NINH - TƯ PHÁP
        var quocPhong = CreateFolder("10. QUỐC PHÒNG - AN NINH - TƯ PHÁP", null, "🛡️", orgName, 10);
        CreateSubFolders(quocPhong.Id, orgName, new[]
        {
            ("Quốc phòng", "⚔️"),
            ("Công an", "👮"),
            ("Tư pháp", "⚖️"),
            ("Phòng chống tội phạm", "🚨")
        });
        
        // 11. TÀI NGUYÊN - MÔI TRƯỜNG
        var taiNguyen = CreateFolder("11. TÀI NGUYÊN - MÔI TRƯỜNG", null, "🌳", orgName, 11);
        CreateSubFolders(taiNguyen.Id, orgName, new[]
        {
            ("Tài nguyên khoáng sản", "⛏️"),
            ("Quản lý nước", "💧"),
            ("Môi trường", "♻️")
        });
        
        // 12. TÀI LIỆU KHÁC
        var taiLieu = CreateFolder("12. TÀI LIỆU KHÁC", null, "📚", orgName, 12);
        CreateSubFolders(taiLieu.Id, orgName, new[]
        {
            ("Báo cáo định kỳ", "📊"),
            ("Thống kê", "📈"),
            ("Lưu trữ lịch sử", "🗄️")
        });
    }
    
    private void CreateStructure_TruongHoc(string orgName)
    {
        Console.WriteLine("  Creating TRƯỜNG HỌC structure...");
        
        // 01. VĂN BẢN ĐẾN
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        // 02. VĂN BẢN ĐI
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03. TỔ CHỨC - HÀNH CHÍNH
        var toChuc = CreateFolder("03. TỔ CHỨC - HÀNH CHÍNH", null, "🏛️", orgName, 3);
        CreateSubFolders(toChuc.Id, orgName, new[]
        {
            ("Tổ chức nhà trường", "⚙️"),
            ("Quản lý cán bộ giáo viên", "👥"),
            ("Biên chế lao động", "📋"),
            ("Thi đua khen thưởng", "🏆"),
            ("Quản lý con dấu", "🔐")
        });
        
        // 04. TÀI CHÍNH - TÀI SẢN
        var taiChinh = CreateFolder("04. TÀI CHÍNH - TÀI SẢN", null, "💰", orgName, 4);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán thu chi", "📊"),
            ("Quyết toán", "📝"),
            ("Quản lý tài sản", "🏢"),
            ("Thu học phí", "💵")
        });
        
        // 05. CHƯƠNG TRÌNH - GIẢNG DẠY
        var giangDay = CreateFolder("05. CHƯƠNG TRÌNH - GIẢNG DẠY", null, "📚", orgName, 5);
        CreateSubFolders(giangDay.Id, orgName, new[]
        {
            ("Kế hoạch giảng dạy", "📅"),
            ("Chương trình đào tạo", "🎯"),
            ("Sách giáo khoa", "📖"),
            ("Giáo án điện tử", "💻")
        });
        
        // 06. QUẢN LÝ HỌC SINH
        var hocSinh = CreateFolder("06. QUẢN LÝ HỌC SINH", null, "👨‍🎓", orgName, 6);
        CreateSubFolders(hocSinh.Id, orgName, new[]
        {
            ("Hồ sơ học sinh", "📁"),
            ("Tuyển sinh", "📝"),
            ("Điểm danh điểm số", "📊"),
            ("Khen thưởng kỷ luật", "🏆"),
            ("Tốt nghiệp lên lớp", "🎓")
        });
        
        // 07. CÔNG TÁC CHUYÊN MÔN
        var chuyenMon = CreateFolder("07. CÔNG TÁC CHUYÊN MÔN", null, "🎓", orgName, 7);
        CreateSubFolders(chuyenMon.Id, orgName, new[]
        {
            ("Hội đồng sư phạm", "👥"),
            ("Tổ chuyên môn", "📚"),
            ("Bồi dưỡng nghiệp vụ", "📖"),
            ("Kiểm tra đánh giá", "✅")
        });
        
        // 08. HỘI ĐỒNG - THI ĐUA
        var hoiDong = CreateFolder("08. HỘI ĐỒNG - THI ĐUA", null, "🏆", orgName, 8);
        CreateSubFolders(hoiDong.Id, orgName, new[]
        {
            ("Hội nghị CBVC", "👥"),
            ("Hội nghị cha mẹ học sinh", "👪"),
            ("Biên bản họp", "📋")
        });
        
        // 09. KỸ THUẬT - CƠ SỞ VẬT CHẤT
        var kyThuat = CreateFolder("09. KỸ THUẬT - CƠ SỞ VẬT CHẤT", null, "🏗️", orgName, 9);
        CreateSubFolders(kyThuat.Id, orgName, new[]
        {
            ("Quản lý phòng học", "🏫"),
            ("Thiết bị dạy học", "💻"),
            ("Sửa chữa bảo dưỡng", "🔧")
        });
        
        // 10. CÔNG TÁC KHÁC
        var congTac = CreateFolder("10. CÔNG TÁC KHÁC", null, "📚", orgName, 10);
        CreateSubFolders(congTac.Id, orgName, new[]
        {
            ("Y tế học đường", "🏥"),
            ("Bảo đảm chất lượng", "✅"),
            ("Báo cáo thống kê", "📊"),
            ("Lưu trữ", "🗄️")
        });
    }
    
    private void CreateStructure_HoiNongDan(string orgName)
    {
        Console.WriteLine("  Creating HỘI NÔNG DÂN structure...");
        
        // 01. VĂN BẢN ĐẾN
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        // 02. VĂN BẢN ĐI
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03. TỔ CHỨC - XÂY DỰNG HỘI
        var toChuc = CreateFolder("03. TỔ CHỨC - XÂY DỰNG HỘI", null, "🏛️", orgName, 3);
        CreateSubFolders(toChuc.Id, orgName, new[]
        {
            ("Tổ chức bộ máy", "⚙️"),
            ("Phát triển hội viên", "👥"),
            ("Quản lý cán bộ hội", "📋"),
            ("Quản lý con dấu", "🔐")
        });
        
        // 04. TÀI CHÍNH - KINH PHÍ
        var taiChinh = CreateFolder("04. TÀI CHÍNH - KINH PHÍ", null, "💰", orgName, 4);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán kinh phí", "📊"),
            ("Quyết toán", "📝"),
            ("Hội phí", "💵")
        });
        
        // 05. ĐẠI HỘI - HỘI NGHỊ
        var daiHoi = CreateFolder("05. ĐẠI HỘI - HỘI NGHỊ", null, "🎭", orgName, 5);
        CreateSubFolders(daiHoi.Id, orgName, new[]
        {
            ("Đại hội hội", "🎪"),
            ("Hội nghị BCH", "👥"),
            ("Hội nghị toàn thể cán bộ", "🏛️"),
            ("Biên bản nghị quyết", "📋")
        });
        
        // 06. CHƯƠNG TRÌNH - HOẠT ĐỘNG
        var chuongTrinh = CreateFolder("06. CHƯƠNG TRÌNH - HOẠT ĐỘNG", null, "🎯", orgName, 6);
        CreateSubFolders(chuongTrinh.Id, orgName, new[]
        {
            ("Chương trình năm", "📅"),
            ("Các phong trào", "🚩"),
            ("Hội thi hội diễn", "🎪"),
            ("Tuyên truyền vận động", "📢")
        });
        
        // 07. QUẢN LÝ HỘI VIÊN
        var hoiVien = CreateFolder("07. QUẢN LÝ HỘI VIÊN", null, "👨‍🌾", orgName, 7);
        CreateSubFolders(hoiVien.Id, orgName, new[]
        {
            ("Danh sách hội viên", "📜"),
            ("Thẻ hội viên", "🎫"),
            ("Khen thưởng kỷ luật", "🏆")
        });
        
        // 08. SẢN XUẤT - KINH TẾ
        var sanXuat = CreateFolder("08. SẢN XUẤT - KINH TẾ", null, "🌾", orgName, 8);
        CreateSubFolders(sanXuat.Id, orgName, new[]
        {
            ("Xây dựng nông thôn mới", "🏡"),
            ("Phát triển kinh tế HTX", "🤝"),
            ("Ứng dụng khoa học kỹ thuật", "🔬"),
            ("Liên kết tiêu thụ sản phẩm", "🛒")
        });
        
        // 09. TÀI LIỆU KHÁC
        var taiLieu = CreateFolder("09. TÀI LIỆU KHÁC", null, "📚", orgName, 9);
        CreateSubFolders(taiLieu.Id, orgName, new[]
        {
            ("Báo cáo tổng kết", "📊"),
            ("Thống kê", "📈"),
            ("Lưu trữ", "🗄️")
        });
    }
    
    private void CreateStructure_MatTran(string orgName)
    {
        Console.WriteLine("  Creating MẶT TRẬN TỔ QUỐC structure...");
        
        // 01-02: VĂN BẢN ĐẾN/ĐI
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03. TỔ CHỨC - XÂY DỰNG MẶT TRẬN
        var toChuc = CreateFolder("03. TỔ CHỨC - XÂY DỰNG MẶT TRẬN", null, "🏛️", orgName, 3);
        CreateSubFolders(toChuc.Id, orgName, new[]
        {
            ("Tổ chức bộ máy", "⚙️"),
            ("Quản lý cán bộ", "👥"),
            ("Xây dựng khối đại đoàn kết", "🤝"),
            ("Quản lý con dấu", "🔐")
        });
        
        // 04. TÀI CHÍNH
        var taiChinh = CreateFolder("04. TÀI CHÍNH", null, "💰", orgName, 4);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán thu chi", "📊"),
            ("Quyết toán", "📝"),
            ("Nguồn đóng góp", "💵")
        });
        
        // 05. ĐẠI HỘI - HỘI NGHỊ
        var daiHoi = CreateFolder("05. ĐẠI HỘI - HỘI NGHỊ", null, "🎭", orgName, 5);
        CreateSubFolders(daiHoi.Id, orgName, new[]
        {
            ("Đại hội MTTQ", "🎪"),
            ("Hội nghị ủy ban", "👥"),
            ("Hội nghị thường trực", "🏛️"),
            ("Biên bản nghị quyết", "📋")
        });
        
        // 06. GIÁM SÁT - PHẢN BIỆN
        var giamSat = CreateFolder("06. GIÁM SÁT - PHẢN BIỆN", null, "🔍", orgName, 6);
        CreateSubFolders(giamSat.Id, orgName, new[]
        {
            ("Giám sát chính quyền", "👁️"),
            ("Góp ý văn bản QPPL", "📜"),
            ("Tiếp dân kiến nghị", "📢")
        });
        
        // 07. DÂN VẬN - TƯ VẤN PHÁP LUẬT
        var danVan = CreateFolder("07. DÂN VẬN - TƯ VẤN PHÁP LUẬT", null, "⚖️", orgName, 7);
        CreateSubFolders(danVan.Id, orgName, new[]
        {
            ("Tiếp dân định kỳ", "👥"),
            ("Giải quyết đơn thư", "✉️"),
            ("Hòa giải đối thoại", "🤝"),
            ("Truyền thông pháp luật", "📢")
        });
        
        // 08. DÂN CHỦ Ở CƠ SỞ
        var danChu = CreateFolder("08. DÂN CHỦ Ở CƠ SỞ", null, "🏘️", orgName, 8);
        CreateSubFolders(danChu.Id, orgName, new[]
        {
            ("Quy ước hương ước", "📜"),
            ("Sinh hoạt cộng đồng", "👪"),
            ("Ban công tác MTTT/KP", "🏠")
        });
        
        // 09. ĐOÀN THỂ THÀNH VIÊN
        var doanThe = CreateFolder("09. ĐOÀN THỂ THÀNH VIÊN", null, "🤝", orgName, 9);
        CreateSubFolders(doanThe.Id, orgName, new[]
        {
            ("Công đoàn", "👷"),
            ("Đoàn TNCS HCM", "🎓"),
            ("Hội LHPN", "👩"),
            ("Hội Nông dân", "👨‍🌾"),
            ("Hội CCB", "🎖️"),
            ("Các hội đoàn thể khác", "🏛️")
        });
        
        // 10. TÀI LIỆU KHÁC
        var taiLieu = CreateFolder("10. TÀI LIỆU KHÁC", null, "📚", orgName, 10);
        CreateSubFolders(taiLieu.Id, orgName, new[]
        {
            ("Báo cáo tổng kết", "📊"),
            ("Thống kê", "📈"),
            ("Lưu trữ", "🗄️")
        });
    }
    
    private void CreateStructure_HoiPhuNu(string orgName)
    {
        Console.WriteLine("  Creating HỘI PHỤ NỮ structure...");
        
        // 01-02: VĂN BẢN ĐẾN/ĐI
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03-10: Các folder chuyên môn
        var toChuc = CreateFolder("03. TỔ CHỨC - XÂY DỰNG HỘI", null, "🏛️", orgName, 3);
        CreateSubFolders(toChuc.Id, orgName, new[]
        {
            ("Tổ chức bộ máy", "⚙️"),
            ("Phát triển hội viên", "👥"),
            ("Quản lý cán bộ hội", "📋"),
            ("Quản lý con dấu", "🔐")
        });
        
        var taiChinh = CreateFolder("04. TÀI CHÍNH", null, "💰", orgName, 4);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán kinh phí", "📊"),
            ("Quyết toán", "📝"),
            ("Hội phí", "💵")
        });
        
        var daiHoi = CreateFolder("05. ĐẠI HỘI - HỘI NGHỊ", null, "🎭", orgName, 5);
        CreateSubFolders(daiHoi.Id, orgName, new[]
        {
            ("Đại hội phụ nữ", "🎪"),
            ("Hội nghị BCH", "👥"),
            ("Hội nghị toàn thể cán bộ", "🏛️"),
            ("Biên bản nghị quyết", "📋")
        });
        
        var chuongTrinh = CreateFolder("06. CHƯƠNG TRÌNH - HOẠT ĐỘNG", null, "🎯", orgName, 6);
        CreateSubFolders(chuongTrinh.Id, orgName, new[]
        {
            ("Chương trình năm", "📅"),
            ("Phong trào thi đua", "🚩"),
            ("Cuộc vận động", "📢"),
            ("Tuyên truyền", "📣")
        });
        
        var hoiVien = CreateFolder("07. QUẢN LÝ HỘI VIÊN", null, "👩", orgName, 7);
        CreateSubFolders(hoiVien.Id, orgName, new[]
        {
            ("Danh sách hội viên", "📜"),
            ("Thẻ hội viên", "🎫"),
            ("Khen thưởng kỷ luật", "🏆")
        });
        
        var quyenLoi = CreateFolder("08. VÌ QUYỀN LỢI PHỤ NỮ", null, "⚖️", orgName, 8);
        CreateSubFolders(quyenLoi.Id, orgName, new[]
        {
            ("Pháp luật quyền lợi phụ nữ", "📜"),
            ("Bình đẳng giới", "⚖️"),
            ("Phòng chống BLGĐ", "🛡️"),
            ("Bảo vệ trẻ em", "👶")
        });
        
        var kinhTe = CreateFolder("09. PHÁT TRIỂN KINH TẾ", null, "💼", orgName, 9);
        CreateSubFolders(kinhTe.Id, orgName, new[]
        {
            ("Dạy nghề tạo việc làm", "🎓"),
            ("Tiết kiệm và vay vốn", "💰"),
            ("Phát triển kinh tế gia đình", "🏠"),
            ("Liên kết sản xuất", "🤝")
        });
        
        var taiLieu = CreateFolder("10. TÀI LIỆU KHÁC", null, "📚", orgName, 10);
        CreateSubFolders(taiLieu.Id, orgName, new[]
        {
            ("Báo cáo tổng kết", "📊"),
            ("Thống kê", "📈"),
            ("Lưu trữ", "🗄️")
        });
    }
    
    private void CreateStructure_DoanThanhNien(string orgName)
    {
        Console.WriteLine("  Creating ĐOÀN THANH NIÊN structure...");
        
        // 01-02: VĂN BẢN ĐẾN/ĐI
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03-10: Các folder chuyên môn
        var toChuc = CreateFolder("03. TỔ CHỨC - XÂY DỰNG ĐOÀN", null, "🏛️", orgName, 3);
        CreateSubFolders(toChuc.Id, orgName, new[]
        {
            ("Tổ chức bộ máy", "⚙️"),
            ("Phát triển đoàn viên", "👥"),
            ("Quản lý cán bộ đoàn", "📋"),
            ("Quản lý con dấu", "🔐")
        });
        
        var taiChinh = CreateFolder("04. TÀI CHÍNH", null, "💰", orgName, 4);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán kinh phí", "📊"),
            ("Quyết toán", "📝"),
            ("Đoàn phí", "💵")
        });
        
        var daiHoi = CreateFolder("05. ĐẠI HỘI - HỘI NGHỊ", null, "🎭", orgName, 5);
        CreateSubFolders(daiHoi.Id, orgName, new[]
        {
            ("Đại hội đoàn", "🎪"),
            ("Hội nghị BCH", "👥"),
            ("Hội nghị BCH mở rộng", "🏛️"),
            ("Biên bản nghị quyết", "📋")
        });
        
        var chuongTrinh = CreateFolder("06. CHƯƠNG TRÌNH - HOẠT ĐỘNG", null, "🎯", orgName, 6);
        CreateSubFolders(chuongTrinh.Id, orgName, new[]
        {
            ("Chương trình năm", "📅"),
            ("Phong trào thanh niên", "🚩"),
            ("Tình nguyện", "❤️"),
            ("Tuyên truyền vận động", "📢")
        });
        
        var doanVien = CreateFolder("07. QUẢN LÝ ĐOÀN VIÊN", null, "👨‍🎓", orgName, 7);
        CreateSubFolders(doanVien.Id, orgName, new[]
        {
            ("Danh sách đoàn viên", "📜"),
            ("Thẻ đoàn viên", "🎫"),
            ("Khen thưởng kỷ luật", "🏆"),
            ("Truy tặng tuyên dương", "🏅")
        });
        
        var lyTuong = CreateFolder("08. GIÁO DỤC LÝ TƯỞNG", null, "🎓", orgName, 8);
        CreateSubFolders(lyTuong.Id, orgName, new[]
        {
            ("Học tập chính trị", "📚"),
            ("Đạo đức lối sống", "💫"),
            ("Giáo dục truyền thống", "🇻🇳"),
            ("Bồi dưỡng lý luận", "📖")
        });
        
        var phongTrao = CreateFolder("09. PHONG TRÀO - HÀNH ĐỘNG", null, "🚀", orgName, 9);
        CreateSubFolders(phongTrao.Id, orgName, new[]
        {
            ("Tình nguyện cộng đồng", "❤️"),
            ("Khởi nghiệp làm kinh tế", "💼"),
            ("Xung kích thanh niên", "⚡"),
            ("Hiếu sinh hiếu nuôi", "👨‍👩‍👧")
        });
        
        var taiLieu = CreateFolder("10. TÀI LIỆU KHÁC", null, "📚", orgName, 10);
        CreateSubFolders(taiLieu.Id, orgName, new[]
        {
            ("Báo cáo tổng kết", "📊"),
            ("Thống kê", "📈"),
            ("Lưu trữ", "🗄️")
        });
    }
    
    // === HĐND (HỘI ĐỒNG NHÂN DÂN) ===
    private void CreateStructure_HDND(string orgName)
    {
        Console.WriteLine("  Creating HỘI ĐỒNG NHÂN DÂN structure...");
        
        // 01-02: VĂN BẢN ĐẾN/ĐI
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03. TỔ CHỨC - HOẠT ĐỘNG
        var toChuc = CreateFolder("03. TỔ CHỨC - HOẠT ĐỘNG", null, "🏛️", orgName, 3);
        CreateSubFolders(toChuc.Id, orgName, new[]
        {
            ("Tổ chức HĐND", "⚙️"),
            ("Đại biểu HĐND", "👥"),
            ("Thường trực HĐND", "📋"),
            ("Ủy ban HĐND", "🏛️"),
            ("Tổ đại biểu", "👔")
        });
        
        // 04. KỲ HỌP - PHIÊN HỌP
        var kyHop = CreateFolder("04. KỲ HỌP - PHIÊN HỌP", null, "🎭", orgName, 4);
        CreateSubFolders(kyHop.Id, orgName, new[]
        {
            ("Kỳ họp thường kỳ", "📅"),
            ("Kỳ họp bất thường", "⚡"),
            ("Biên bản kỳ họp", "📋"),
            ("Chất vấn trả lời", "❓"),
            ("Thảo luận tổ", "👥")
        });
        
        // 05. NGHỊ QUYẾT - QUYẾT ĐỊNH
        var nghiQuyet = CreateFolder("05. NGHỊ QUYẾT - QUYẾT ĐỊNH", null, "📜", orgName, 5);
        CreateSubFolders(nghiQuyet.Id, orgName, new[]
        {
            ("Nghị quyết HĐND", "📕"),
            ("Quyết định HĐND", "📘"),
            ("Nghị quyết Thường trực", "📗"),
            ("Quyết định Thường trực", "📙")
        });
        
        // 06. GIÁM SÁT
        var giamSat = CreateFolder("06. GIÁM SÁT", null, "🔍", orgName, 6);
        CreateSubFolders(giamSat.Id, orgName, new[]
        {
            ("Chương trình giám sát", "📅"),
            ("Đoàn giám sát", "👥"),
            ("Báo cáo giám sát", "📊"),
            ("Kiến nghị sau giám sát", "📝")
        });
        
        // 07. TÀI CHÍNH - NGÂN SÁCH
        var taiChinh = CreateFolder("07. TÀI CHÍNH - NGÂN SÁCH", null, "💰", orgName, 7);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán ngân sách", "📊"),
            ("Quyết toán ngân sách", "📈"),
            ("Phân bổ ngân sách", "💵"),
            ("Báo cáo tài chính", "📝")
        });
        
        // 08. TIẾP DÂN - ĐƠN THƯ
        var tiepDan = CreateFolder("08. TIẾP DÂN - ĐƠN THƯ", null, "👥", orgName, 8);
        CreateSubFolders(tiepDan.Id, orgName, new[]
        {
            ("Tiếp dân định kỳ", "📅"),
            ("Đơn thư khiếu nại", "✉️"),
            ("Giải quyết kiến nghị", "📝"),
            ("Phúc đáp cử tri", "📢")
        });
        
        // 09. TÀI LIỆU KHÁC
        var taiLieu = CreateFolder("09. TÀI LIỆU KHÁC", null, "📚", orgName, 9);
        CreateSubFolders(taiLieu.Id, orgName, new[]
        {
            ("Báo cáo hoạt động", "📊"),
            ("Thống kê", "📈"),
            ("Lưu trữ", "🗄️")
        });
    }
    
    // === ĐẢNG (ĐẢNG UỶ, CHI BỘ) ===
    private void CreateStructure_Dang(string orgName)
    {
        Console.WriteLine("  Creating CƠ QUAN ĐẢNG structure...");
        
        // 01-02: VĂN BẢN ĐẾN/ĐI
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03. TỔ CHỨC - XÂY DỰNG ĐẢNG
        var toChuc = CreateFolder("03. TỔ CHỨC - XÂY DỰNG ĐẢNG", null, "🏛️", orgName, 3);
        CreateSubFolders(toChuc.Id, orgName, new[]
        {
            ("Tổ chức Đảng bộ", "⚙️"),
            ("Chi bộ trực thuộc", "🏢"),
            ("Đảng viên", "👥"),
            ("Kết nạp Đảng", "📝"),
            ("Chuyển sinh hoạt Đảng", "🔄"),
            ("Kiểm điểm Đảng viên", "📋")
        });
        
        // 04. ĐẠI HỘI - HỘI NGHỊ
        var daiHoi = CreateFolder("04. ĐẠI HỘI - HỘI NGHỊ", null, "🎭", orgName, 4);
        CreateSubFolders(daiHoi.Id, orgName, new[]
        {
            ("Đại hội Đảng bộ", "🎪"),
            ("Hội nghị Ban chấp hành", "👥"),
            ("Hội nghị Ban thường vụ", "👔"),
            ("Sinh hoạt chi bộ", "🏛️"),
            ("Biên bản nghị quyết", "📋")
        });
        
        // 05. TUYÊN GIÁO - ĐÀO TẠO
        var tuyenGiao = CreateFolder("05. TUYÊN GIÁO - ĐÀO TẠO", null, "📢", orgName, 5);
        CreateSubFolders(tuyenGiao.Id, orgName, new[]
        {
            ("Học tập nghị quyết", "📚"),
            ("Bồi dưỡng lý luận chính trị", "🎓"),
            ("Tuyên truyền vận động", "📣"),
            ("Giáo dục chính trị tư tưởng", "💭")
        });
        
        // 06. KIỂM TRA - KỶ LUẬT
        var kiemTra = CreateFolder("06. KIỂM TRA - KỶ LUẬT", null, "🔍", orgName, 6);
        CreateSubFolders(kiemTra.Id, orgName, new[]
        {
            ("Kiểm tra tổ chức Đảng", "👁️"),
            ("Kiểm tra Đảng viên", "📋"),
            ("Kỷ luật Đảng", "⚖️"),
            ("Thi hành kỷ luật", "📜")
        });
        
        // 07. DÂN VẬN - MẶT TRẬN
        var danVan = CreateFolder("07. DÂN VẬN - MẶT TRẬN", null, "👥", orgName, 7);
        CreateSubFolders(danVan.Id, orgName, new[]
        {
            ("Công tác dân vận", "🤝"),
            ("Mặt trận tổ quốc", "🏛️"),
            ("Đoàn thể chính trị", "🎗️"),
            ("Đại đoàn kết", "🤝")
        });
        
        // 08. NỘI CHÍNH - PHÒNG CHỐNG THAM NHŨNG
        var noiChinh = CreateFolder("08. NỘI CHÍNH - PCTN", null, "🛡️", orgName, 8);
        CreateSubFolders(noiChinh.Id, orgName, new[]
        {
            ("Nội chính - Quốc phòng", "⚔️"),
            ("Phòng chống tham nhũng", "🚫"),
            ("Cải cách hành chính", "⚙️"),
            ("Công tác bảo vệ", "🔒")
        });
        
        // 09. TÀI CHÍNH
        var taiChinh = CreateFolder("09. TÀI CHÍNH", null, "💰", orgName, 9);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán kinh phí", "📊"),
            ("Quyết toán", "📝"),
            ("Đảng phí", "💵")
        });
        
        // 10. TÀI LIỆU KHÁC
        var taiLieu = CreateFolder("10. TÀI LIỆU KHÁC", null, "📚", orgName, 10);
        CreateSubFolders(taiLieu.Id, orgName, new[]
        {
            ("Báo cáo định kỳ", "📊"),
            ("Thống kê Đảng", "📈"),
            ("Lưu trữ", "🗄️")
        });
    }
    
    // === BAN CỦA ĐẢNG (Ban Dân vận, Ban Tổ chức, Ban Tuyên giáo...) ===
    private void CreateStructure_BanCuaDang(string orgName)
    {
        Console.WriteLine("  Creating BAN CỦA ĐẢNG structure...");
        
        // 01-02: VĂN BẢN ĐẾN/ĐI
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03. TỔ CHỨC - HÀNH CHÍNH
        var toChuc = CreateFolder("03. TỔ CHỨC - HÀNH CHÍNH", null, "🏛️", orgName, 3);
        CreateSubFolders(toChuc.Id, orgName, new[]
        {
            ("Tổ chức bộ máy", "⚙️"),
            ("Quản lý cán bộ", "👥"),
            ("Quản lý con dấu", "🔐")
        });
        
        // 04. CHƯƠNG TRÌNH - KẾ HOẠCH
        var chuongTrinh = CreateFolder("04. CHƯƠNG TRÌNH - KẾ HOẠCH", null, "📅", orgName, 4);
        CreateSubFolders(chuongTrinh.Id, orgName, new[]
        {
            ("Chương trình năm", "📆"),
            ("Kế hoạch tháng", "📋"),
            ("Nghị quyết chuyên đề", "📜")
        });
        
        // 05. CÔNG TÁC CHUYÊN MÔN
        var chuyenMon = CreateFolder("05. CÔNG TÁC CHUYÊN MÔN", null, "💼", orgName, 5);
        CreateSubFolders(chuyenMon.Id, orgName, new[]
        {
            ("Công tác theo lĩnh vực", "📁"),
            ("Hướng dẫn nghiệp vụ", "📖"),
            ("Báo cáo chuyên đề", "📊"),
            ("Tổng kết kinh nghiệm", "📝")
        });
        
        // 06. HỘI NGHỊ - HỘI THẢO
        var hoiNghi = CreateFolder("06. HỘI NGHỊ - HỘI THẢO", null, "🎭", orgName, 6);
        CreateSubFolders(hoiNghi.Id, orgName, new[]
        {
            ("Hội nghị cán bộ", "👥"),
            ("Hội thảo chuyên đề", "🎓"),
            ("Biên bản họp", "📋")
        });
        
        // 07. KIỂM TRA - GIÁM SÁT
        var kiemTra = CreateFolder("07. KIỂM TRA - GIÁM SÁT", null, "🔍", orgName, 7);
        CreateSubFolders(kiemTra.Id, orgName, new[]
        {
            ("Kế hoạch kiểm tra", "📅"),
            ("Đoàn kiểm tra", "👥"),
            ("Báo cáo kiểm tra", "📊"),
            ("Kiến nghị xử lý", "📝")
        });
        
        // 08. TÀI CHÍNH
        var taiChinh = CreateFolder("08. TÀI CHÍNH", null, "💰", orgName, 8);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán kinh phí", "📊"),
            ("Quyết toán", "📝")
        });
        
        // 09. TÀI LIỆU KHÁC
        var taiLieu = CreateFolder("09. TÀI LIỆU KHÁC", null, "📚", orgName, 9);
        CreateSubFolders(taiLieu.Id, orgName, new[]
        {
            ("Báo cáo định kỳ", "📊"),
            ("Thống kê", "📈"),
            ("Lưu trữ", "🗄️")
        });
    }
    
    // === SỞ - BAN - NGÀNH (CẤP TỈNH) ===
    private void CreateStructure_SoBanNganh(string orgName)
    {
        Console.WriteLine("  Creating SỞ - BAN - NGÀNH structure...");
        
        // 01-02: VĂN BẢN ĐẾN/ĐI
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03. TỔ CHỨC - HÀNH CHÍNH
        var toChuc = CreateFolder("03. TỔ CHỨC - HÀNH CHÍNH", null, "🏛️", orgName, 3);
        CreateSubFolders(toChuc.Id, orgName, new[]
        {
            ("Tổ chức bộ máy", "⚙️"),
            ("Biên chế cán bộ", "👥"),
            ("Quản lý lãnh đạo", "👔"),
            ("Thi đua khen thưởng", "🏆"),
            ("Quản lý con dấu", "🔐")
        });
        
        // 04. TÀI CHÍNH - KẾ TOÁN
        var taiChinh = CreateFolder("04. TÀI CHÍNH - KẾ TOÁN", null, "💰", orgName, 4);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán ngân sách", "📊"),
            ("Quyết toán", "📝"),
            ("Thu chi tài chính", "💵"),
            ("Quản lý tài sản", "🏢")
        });
        
        // 05. QUY HOẠCH - KẾ HOẠCH
        var quyHoach = CreateFolder("05. QUY HOẠCH - KẾ HOẠCH", null, "🗺️", orgName, 5);
        CreateSubFolders(quyHoach.Id, orgName, new[]
        {
            ("Quy hoạch ngành", "📍"),
            ("Kế hoạch 5 năm", "📅"),
            ("Kế hoạch hàng năm", "📆"),
            ("Chương trình mục tiêu", "🎯")
        });
        
        // 06. CÔNG TÁC CHUYÊN MÔN
        var chuyenMon = CreateFolder("06. CÔNG TÁC CHUYÊN MÔN", null, "💼", orgName, 6);
        CreateSubFolders(chuyenMon.Id, orgName, new[]
        {
            ("Quản lý nhà nước về ngành", "🏛️"),
            ("Hướng dẫn nghiệp vụ", "📖"),
            ("Thẩm định dự án", "📋"),
            ("Cấp phép giấy tờ", "📄"),
            ("Thanh tra kiểm tra", "🔍")
        });
        
        // 07. HỘI NGHỊ - HỘI THẢO
        var hoiNghi = CreateFolder("07. HỘI NGHỊ - HỘI THẢO", null, "🎭", orgName, 7);
        CreateSubFolders(hoiNghi.Id, orgName, new[]
        {
            ("Hội nghị cán bộ", "👥"),
            ("Hội nghị chuyên đề", "📋"),
            ("Biên bản họp", "📝")
        });
        
        // 08. ĐƠN VỊ TRỰC THUỘC
        var donVi = CreateFolder("08. ĐƠN VỊ TRỰC THUỘC", null, "🏢", orgName, 8);
        CreateSubFolders(donVi.Id, orgName, new[]
        {
            ("Phòng chức năng", "📁"),
            ("Đơn vị sự nghiệp", "🏛️"),
            ("Trung tâm trực thuộc", "🏫")
        });
        
        // 09. BÁO CÁO - THỐNG KÊ
        var baoCao = CreateFolder("09. BÁO CÁO - THỐNG KÊ", null, "📊", orgName, 9);
        CreateSubFolders(baoCao.Id, orgName, new[]
        {
            ("Báo cáo định kỳ", "📅"),
            ("Báo cáo đột xuất", "⚡"),
            ("Thống kê ngành", "📈"),
            ("Tổng kết", "📝")
        });
        
        // 10. TÀI LIỆU KHÁC
        var taiLieu = CreateFolder("10. TÀI LIỆU KHÁC", null, "📚", orgName, 10);
        CreateSubFolders(taiLieu.Id, orgName, new[]
        {
            ("Văn bản pháp quy", "📜"),
            ("Tài liệu nghiệp vụ", "📖"),
            ("Lưu trữ", "🗄️")
        });
    }
    
    // === PHÒNG CẤP HUYỆN ===
    private void CreateStructure_PhongCapHuyen(string orgName)
    {
        Console.WriteLine("  Creating PHÒNG CẤP HUYỆN structure...");
        
        // 01-02: VĂN BẢN ĐẾN/ĐI
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03. TỔ CHỨC - HÀNH CHÍNH
        var toChuc = CreateFolder("03. TỔ CHỨC - HÀNH CHÍNH", null, "🏛️", orgName, 3);
        CreateSubFolders(toChuc.Id, orgName, new[]
        {
            ("Tổ chức bộ máy", "⚙️"),
            ("Biên chế cán bộ", "👥"),
            ("Thi đua khen thưởng", "🏆"),
            ("Quản lý con dấu", "🔐")
        });
        
        // 04. TÀI CHÍNH - KẾ TOÁN
        var taiChinh = CreateFolder("04. TÀI CHÍNH - KẾ TOÁN", null, "💰", orgName, 4);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán", "📊"),
            ("Quyết toán", "📝"),
            ("Thu chi", "💵")
        });
        
        // 05. KẾ HOẠCH - CHƯƠNG TRÌNH
        var keHoach = CreateFolder("05. KẾ HOẠCH - CHƯƠNG TRÌNH", null, "📅", orgName, 5);
        CreateSubFolders(keHoach.Id, orgName, new[]
        {
            ("Kế hoạch năm", "📆"),
            ("Kế hoạch tháng", "📋"),
            ("Chương trình công tác", "🎯")
        });
        
        // 06. CÔNG TÁC CHUYÊN MÔN
        var chuyenMon = CreateFolder("06. CÔNG TÁC CHUYÊN MÔN", null, "💼", orgName, 6);
        CreateSubFolders(chuyenMon.Id, orgName, new[]
        {
            ("Quản lý nhà nước", "🏛️"),
            ("Hướng dẫn nghiệp vụ", "📖"),
            ("Thẩm định hồ sơ", "📋"),
            ("Cấp phép", "📄"),
            ("Thanh tra kiểm tra", "🔍")
        });
        
        // 07. HỘI NGHỊ - BIÊN BẢN
        var hoiNghi = CreateFolder("07. HỘI NGHỊ - BIÊN BẢN", null, "🎭", orgName, 7);
        CreateSubFolders(hoiNghi.Id, orgName, new[]
        {
            ("Hội nghị cán bộ", "👥"),
            ("Biên bản họp", "📋")
        });
        
        // 08. ĐƠN VỊ TRỰC THUỘC
        var donVi = CreateFolder("08. ĐƠN VỊ TRỰC THUỘC", null, "🏢", orgName, 8);
        CreateSubFolders(donVi.Id, orgName, new[]
        {
            ("Đơn vị cấp xã", "🏘️"),
            ("Cơ sở trực thuộc", "🏫")
        });
        
        // 09. BÁO CÁO - THỐNG KÊ
        var baoCao = CreateFolder("09. BÁO CÁO - THỐNG KÊ", null, "📊", orgName, 9);
        CreateSubFolders(baoCao.Id, orgName, new[]
        {
            ("Báo cáo định kỳ", "📅"),
            ("Thống kê", "📈"),
            ("Tổng kết", "📝")
        });
        
        // 10. TÀI LIỆU KHÁC
        var taiLieu = CreateFolder("10. TÀI LIỆU KHÁC", null, "📚", orgName, 10);
        CreateSubFolders(taiLieu.Id, orgName, new[]
        {
            ("Văn bản hướng dẫn", "📜"),
            ("Tài liệu nghiệp vụ", "📖"),
            ("Lưu trữ", "🗄️")
        });
    }
    
    // === Y TẾ (TRẠM/TRUNG TÂM/BỆNH VIỆN) ===
    private void CreateStructure_YTe(string orgName)
    {
        Console.WriteLine("  Creating CƠ SỞ Y TẾ structure...");
        
        // 01-02: VĂN BẢN ĐẾN/ĐI
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03. TỔ CHỨC - HÀNH CHÍNH
        var toChuc = CreateFolder("03. TỔ CHỨC - HÀNH CHÍNH", null, "🏛️", orgName, 3);
        CreateSubFolders(toChuc.Id, orgName, new[]
        {
            ("Tổ chức bộ máy", "⚙️"),
            ("Quản lý cán bộ y tế", "👥"),
            ("Thi đua khen thưởng", "🏆"),
            ("Quản lý con dấu", "🔐")
        });
        
        // 04. TÀI CHÍNH - KẾ TOÁN
        var taiChinh = CreateFolder("04. TÀI CHÍNH - KẾ TOÁN", null, "💰", orgName, 4);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán", "📊"),
            ("Quyết toán", "📝"),
            ("Thu viện phí", "💵"),
            ("Bảo hiểm y tế", "🏥")
        });
        
        // 05. KHÁM CHỮA BỆNH
        var khamBenh = CreateFolder("05. KHÁM CHỮA BỆNH", null, "🏥", orgName, 5);
        CreateSubFolders(khamBenh.Id, orgName, new[]
        {
            ("Khám bệnh ngoại trú", "👨‍⚕️"),
            ("Điều trị nội trú", "🛏️"),
            ("Cấp cứu", "🚑"),
            ("Hồ sơ bệnh án", "📋"),
            ("Chuyển viện", "🔄")
        });
        
        // 06. PHÒNG CHỐNG DỊCH BỆNH
        var phongDich = CreateFolder("06. PHÒNG CHỐNG DỊCH BỆNH", null, "💉", orgName, 6);
        CreateSubFolders(phongDich.Id, orgName, new[]
        {
            ("Tiêm chủng", "💉"),
            ("Giám sát dịch bệnh", "🔍"),
            ("Phòng chống dịch", "🛡️"),
            ("Y tế dự phòng", "🏥")
        });
        
        // 07. DÂN SỐ - KẾ HOẠCH HÓA GIA ĐÌNH
        var danSo = CreateFolder("07. DÂN SỐ - KHHGĐ", null, "👶", orgName, 7);
        CreateSubFolders(danSo.Id, orgName, new[]
        {
            ("Kế hoạch hóa gia đình", "👪"),
            ("Sức khỏe sinh sản", "🤰"),
            ("Dinh dưỡng", "🍼")
        });
        
        // 08. DƯỢC - VẬT TƯ Y TẾ
        var duoc = CreateFolder("08. DƯỢC - VẬT TƯ Y TẾ", null, "💊", orgName, 8);
        CreateSubFolders(duoc.Id, orgName, new[]
        {
            ("Quản lý thuốc", "💊"),
            ("Vật tư y tế", "🩺"),
            ("Thiết bị y tế", "🔬")
        });
        
        // 09. BÁO CÁO - THỐNG KÊ
        var baoCao = CreateFolder("09. BÁO CÁO - THỐNG KÊ", null, "📊", orgName, 9);
        CreateSubFolders(baoCao.Id, orgName, new[]
        {
            ("Báo cáo hoạt động", "📅"),
            ("Thống kê y tế", "📈"),
            ("Chất lượng khám chữa bệnh", "✅")
        });
        
        // 10. TÀI LIỆU KHÁC
        var taiLieu = CreateFolder("10. TÀI LIỆU KHÁC", null, "📚", orgName, 10);
        CreateSubFolders(taiLieu.Id, orgName, new[]
        {
            ("Tài liệu chuyên môn", "📖"),
            ("Hội nghị y khoa", "🎓"),
            ("Lưu trữ", "🗄️")
        });
    }
    
    // === CÔNG AN ===
    private void CreateStructure_CongAn(string orgName)
    {
        Console.WriteLine("  Creating CÔNG AN structure...");
        
        // 01-02: VĂN BẢN ĐẾN/ĐI
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03. TỔ CHỨC - HÀNH CHÍNH
        var toChuc = CreateFolder("03. TỔ CHỨC - HÀNH CHÍNH", null, "🏛️", orgName, 3);
        CreateSubFolders(toChuc.Id, orgName, new[]
        {
            ("Tổ chức bộ máy", "⚙️"),
            ("Quản lý cán bộ chiến sĩ", "👥"),
            ("Thi đua khen thưởng", "🏆"),
            ("Quản lý con dấu", "🔐")
        });
        
        // 04. TÀI CHÍNH - HẬU CẦN
        var taiChinh = CreateFolder("04. TÀI CHÍNH - HẬU CẦN", null, "💰", orgName, 4);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán kinh phí", "📊"),
            ("Quyết toán", "📝"),
            ("Quản lý vũ khí trang bị", "🔫")
        });
        
        // 05. AN NINH - TRẬT TỰ
        var anNinh = CreateFolder("05. AN NINH - TRẬT TỰ", null, "🛡️", orgName, 5);
        CreateSubFolders(anNinh.Id, orgName, new[]
        {
            ("An ninh chính trị", "🏛️"),
            ("Trật tự an toàn xã hội", "👮"),
            ("Tuần tra kiểm soát", "🚔"),
            ("Giữ gìn trật tự", "⚖️")
        });
        
        // 06. PHÒNG CHỐNG TỘI PHẠM
        var pctp = CreateFolder("06. PHÒNG CHỐNG TỘI PHẠM", null, "🚨", orgName, 6);
        CreateSubFolders(pctp.Id, orgName, new[]
        {
            ("Điều tra hình sự", "🔍"),
            ("Đấu tranh tội phạm", "⚔️"),
            ("Phòng chống ma túy", "🚫"),
            ("Hồ sơ vụ án", "📁")
        });
        
        // 07. QUẢN LÝ HÀNH CHÍNH
        var qlhc = CreateFolder("07. QUẢN LÝ HÀNH CHÍNH", null, "📋", orgName, 7);
        CreateSubFolders(qlhc.Id, orgName, new[]
        {
            ("Cấp CCCD", "🪪"),
            ("Quản lý cư trú", "🏘️"),
            ("Hộ khẩu tạm trú", "📝"),
            ("Quản lý vũ khí vật liệu nổ", "💣")
        });
        
        // 08. PHÒNG CHÁY CHỮA CHÁY
        var pccc = CreateFolder("08. PHÒNG CHÁY CHỮA CHÁY", null, "🚒", orgName, 8);
        CreateSubFolders(pccc.Id, orgName, new[]
        {
            ("Tuyên truyền PCCC", "📢"),
            ("Kiểm tra PCCC", "🔍"),
            ("Chữa cháy cứu nạn", "🚨"),
            ("Cấp phép PCCC", "📄")
        });
        
        // 09. BÁO CÁO - THỐNG KÊ
        var baoCao = CreateFolder("09. BÁO CÁO - THỐNG KÊ", null, "📊", orgName, 9);
        CreateSubFolders(baoCao.Id, orgName, new[]
        {
            ("Báo cáo định kỳ", "📅"),
            ("Thống kê tội phạm", "📈"),
            ("Tổng kết", "📝")
        });
        
        // 10. TÀI LIỆU KHÁC
        var taiLieu = CreateFolder("10. TÀI LIỆU KHÁC", null, "📚", orgName, 10);
        CreateSubFolders(taiLieu.Id, orgName, new[]
        {
            ("Văn bản pháp luật", "📜"),
            ("Tài liệu nghiệp vụ", "📖"),
            ("Lưu trữ", "🗄️")
        });
    }
    
    // === VĂN PHÒNG (Văn phòng UBND, Văn phòng cấp ủy...) ===
    private void CreateStructure_VanPhong(string orgName)
    {
        Console.WriteLine("  Creating VĂN PHÒNG structure...");
        
        // 01-02: VĂN BẢN ĐẾN/ĐI
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03. VĂN THƯ - LƯU TRỮ
        var vanThu = CreateFolder("03. VĂN THƯ - LƯU TRỮ", null, "📚", orgName, 3);
        CreateSubFolders(vanThu.Id, orgName, new[]
        {
            ("Quản lý văn bản", "📋"),
            ("Lưu trữ hồ sơ", "🗄️"),
            ("Thống kê văn bản", "📊"),
            ("Sổ văn bản", "📖")
        });
        
        // 04. HÀNH CHÍNH - TỔ CHỨC
        var hanhChinh = CreateFolder("04. HÀNH CHÍNH - TỔ CHỨC", null, "🏛️", orgName, 4);
        CreateSubFolders(hanhChinh.Id, orgName, new[]
        {
            ("Tổ chức bộ máy", "⚙️"),
            ("Quản lý cán bộ", "👥"),
            ("Quản lý con dấu", "🔐"),
            ("Thi đua khen thưởng", "🏆")
        });
        
        // 05. TÀI CHÍNH - TÀI SẢN
        var taiChinh = CreateFolder("05. TÀI CHÍNH - TÀI SẢN", null, "💰", orgName, 5);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán kinh phí", "📊"),
            ("Quyết toán", "📝"),
            ("Quản lý tài sản", "🏢")
        });
        
        // 06. HỘI NGHỊ - LỄ TÂN
        var hoiNghi = CreateFolder("06. HỘI NGHỊ - LỄ TÂN", null, "🎭", orgName, 6);
        CreateSubFolders(hoiNghi.Id, orgName, new[]
        {
            ("Chuẩn bị hội nghị", "📅"),
            ("Biên bản họp", "📋"),
            ("Lễ tân tiếp khách", "👥"),
            ("Sự kiện quan trọng", "🎉")
        });
        
        // 07. CÔNG NGHỆ THÔNG TIN
        var congNghe = CreateFolder("07. CÔNG NGHỆ THÔNG TIN", null, "💻", orgName, 7);
        CreateSubFolders(congNghe.Id, orgName, new[]
        {
            ("Quản trị hệ thống", "🖥️"),
            ("Bảo mật thông tin", "🔒"),
            ("Ứng dụng CNTT", "📱")
        });
        
        // 08. TIẾP DÂN - ĐƠN THƯ
        var tiepDan = CreateFolder("08. TIẾP DÂN - ĐƠN THƯ", null, "👥", orgName, 8);
        CreateSubFolders(tiepDan.Id, orgName, new[]
        {
            ("Tiếp dân định kỳ", "📅"),
            ("Đơn thư khiếu nại", "✉️"),
            ("Giải quyết kiến nghị", "📝")
        });
        
        // 09. BÁO CÁO - THỐNG KÊ
        var baoCao = CreateFolder("09. BÁO CÁO - THỐNG KÊ", null, "📊", orgName, 9);
        CreateSubFolders(baoCao.Id, orgName, new[]
        {
            ("Báo cáo định kỳ", "📅"),
            ("Thống kê", "📈"),
            ("Tổng hợp báo cáo", "📝")
        });
        
        // 10. TÀI LIỆU KHÁC
        var taiLieu = CreateFolder("10. TÀI LIỆU KHÁC", null, "📚", orgName, 10);
        CreateSubFolders(taiLieu.Id, orgName, new[]
        {
            ("Văn bản hướng dẫn", "📜"),
            ("Mẫu biểu", "📋"),
            ("Lưu trữ", "🗄️")
        });
    }
    
    // === TRUNG TÂM HÀNH CHÍNH CÔNG ===
    private void CreateStructure_TrungTamHanhChinh(string orgName)
    {
        Console.WriteLine("  Creating TRUNG TÂM HÀNH CHÍNH CÔNG structure...");
        
        // 01-02: VĂN BẢN ĐẾN/ĐI
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03. TỔ CHỨC - HÀNH CHÍNH
        var toChuc = CreateFolder("03. TỔ CHỨC - HÀNH CHÍNH", null, "🏛️", orgName, 3);
        CreateSubFolders(toChuc.Id, orgName, new[]
        {
            ("Tổ chức bộ máy", "⚙️"),
            ("Quản lý cán bộ", "👥"),
            ("Quản lý con dấu", "🔐")
        });
        
        // 04. TÀI CHÍNH
        var taiChinh = CreateFolder("04. TÀI CHÍNH", null, "💰", orgName, 4);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán", "📊"),
            ("Quyết toán", "📝")
        });
        
        // 05. TIẾP NHẬN - TRẢ KẾT QUẢ
        var tiepNhan = CreateFolder("05. TIẾP NHẬN - TRẢ KẾT QUẢ", null, "📋", orgName, 5);
        CreateSubFolders(tiepNhan.Id, orgName, new[]
        {
            ("Tiếp nhận hồ sơ", "📥"),
            ("Trả kết quả", "📤"),
            ("Hồ sơ đang xử lý", "⏳"),
            ("Hồ sơ hoàn thành", "✅")
        });
        
        // 06. THỦ TỤC HÀNH CHÍNH
        var thuTuc = CreateFolder("06. THỦ TỤC HÀNH CHÍNH", null, "📄", orgName, 6);
        CreateSubFolders(thuTuc.Id, orgName, new[]
        {
            ("Đất đai", "🗺️"),
            ("Xây dựng", "🏗️"),
            ("Đầu tư kinh doanh", "💼"),
            ("Hộ tịch - CCCD", "🪪"),
            ("Các TTHC khác", "📁")
        });
        
        // 07. CSDL - CÔNG NGHỆ
        var csdl = CreateFolder("07. CSDL - CÔNG NGHỆ", null, "💻", orgName, 7);
        CreateSubFolders(csdl.Id, orgName, new[]
        {
            ("Cơ sở dữ liệu TTHC", "🗄️"),
            ("Phần mềm 1 cửa", "🖥️"),
            ("Dịch vụ công trực tuyến", "🌐")
        });
        
        // 08. BÁO CÁO - THỐNG KÊ
        var baoCao = CreateFolder("08. BÁO CÁO - THỐNG KÊ", null, "📊", orgName, 8);
        CreateSubFolders(baoCao.Id, orgName, new[]
        {
            ("Báo cáo định kỳ", "📅"),
            ("Thống kê hồ sơ", "📈"),
            ("Đánh giá chất lượng", "✅")
        });
        
        // 09. TÀI LIỆU KHÁC
        var taiLieu = CreateFolder("09. TÀI LIỆU KHÁC", null, "📚", orgName, 9);
        CreateSubFolders(taiLieu.Id, orgName, new[]
        {
            ("Hướng dẫn TTHC", "📖"),
            ("Quy trình nghiệp vụ", "📋"),
            ("Lưu trữ", "🗄️")
        });
    }
    
    // === ĐOÀN THỂ KHÁC (Hội CCB, Công đoàn, Hội Chữ thập đỏ, Hội Khuyến học) ===
    private void CreateStructure_DoanTheKhac(string orgName)
    {
        Console.WriteLine("  Creating ĐOÀN THỂ KHÁC structure...");
        
        // 01-02: VĂN BẢN ĐẾN/ĐI
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        // 03. TỔ CHỨC - XÂY DỰNG
        var toChuc = CreateFolder("03. TỔ CHỨC - XÂY DỰNG", null, "🏛️", orgName, 3);
        CreateSubFolders(toChuc.Id, orgName, new[]
        {
            ("Tổ chức bộ máy", "⚙️"),
            ("Phát triển hội viên", "👥"),
            ("Quản lý cán bộ", "📋"),
            ("Quản lý con dấu", "🔐")
        });
        
        // 04. TÀI CHÍNH
        var taiChinh = CreateFolder("04. TÀI CHÍNH", null, "💰", orgName, 4);
        CreateSubFolders(taiChinh.Id, orgName, new[]
        {
            ("Dự toán kinh phí", "📊"),
            ("Quyết toán", "📝"),
            ("Hội phí", "💵")
        });
        
        // 05. ĐẠI HỘI - HỘI NGHỊ
        var daiHoi = CreateFolder("05. ĐẠI HỘI - HỘI NGHỊ", null, "🎭", orgName, 5);
        CreateSubFolders(daiHoi.Id, orgName, new[]
        {
            ("Đại hội", "🎪"),
            ("Hội nghị BCH", "👥"),
            ("Biên bản nghị quyết", "📋")
        });
        
        // 06. CHƯƠNG TRÌNH - HOẠT ĐỘNG
        var chuongTrinh = CreateFolder("06. CHƯƠNG TRÌNH - HOẠT ĐỘNG", null, "🎯", orgName, 6);
        CreateSubFolders(chuongTrinh.Id, orgName, new[]
        {
            ("Chương trình năm", "📅"),
            ("Phong trào", "🚩"),
            ("Tuyên truyền", "📢")
        });
        
        // 07. QUẢN LÝ HỘI VIÊN
        var hoiVien = CreateFolder("07. QUẢN LÝ HỘI VIÊN", null, "👥", orgName, 7);
        CreateSubFolders(hoiVien.Id, orgName, new[]
        {
            ("Danh sách hội viên", "📜"),
            ("Thẻ hội viên", "🎫"),
            ("Khen thưởng kỷ luật", "🏆")
        });
        
        // 08. BÁO CÁO - THỐNG KÊ
        var baoCao = CreateFolder("08. BÁO CÁO - THỐNG KÊ", null, "📊", orgName, 8);
        CreateSubFolders(baoCao.Id, orgName, new[]
        {
            ("Báo cáo tổng kết", "📅"),
            ("Thống kê", "📈")
        });
        
        // 09. TÀI LIỆU KHÁC
        var taiLieu = CreateFolder("09. TÀI LIỆU KHÁC", null, "📚", orgName, 9);
        CreateSubFolders(taiLieu.Id, orgName, new[]
        {
            ("Tài liệu chuyên môn", "📖"),
            ("Lưu trữ", "🗄️")
        });
    }
    
    private void CreateStructure_Generic(string orgName)
    {
        Console.WriteLine("  Creating GENERIC (default) structure...");
        
        // Cấu trúc chung cho các loại còn lại
        var vbDen = CreateFolder("01. VĂN BẢN ĐẾN", null, "📥", orgName, 1);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDen.Id, "📅", orgName);
        }
        
        var vbDi = CreateFolder("02. VĂN BẢN ĐI", null, "📤", orgName, 2);
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            CreateFolder($"Năm {year}", vbDi.Id, "📅", orgName);
        }
        
        CreateFolder("03. HÀNH CHÍNH - TỔ CHỨC", null, "🏛️", orgName, 3);
        CreateFolder("04. TÀI CHÍNH", null, "💰", orgName, 4);
        CreateFolder("05. HỘI NGHỊ - BIÊN BẢN", null, "📋", orgName, 5);
        CreateFolder("06. HOẠT ĐỘNG CHUYÊN MÔN", null, "💼", orgName, 6);
        CreateFolder("07. BÁO CÁO - THỐNG KÊ", null, "📊", orgName, 7);
        CreateFolder("08. TÀI LIỆU KHÁC", null, "📚", orgName, 8);
    }
    
    // Helper methods
    private Folder CreateFolder(string name, string? parentId, string icon, string orgName, int sortOrder = 0)
    {
        try
        {
            var folder = new Folder
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                ParentId = parentId ?? string.Empty,
                Icon = icon,
                OrganizationName = orgName,
                SortOrder = sortOrder,
                CreatedDate = DateTime.Now
            };
            
            _documentService.CreateFolder(folder);
            Console.WriteLine($"  ✓ Created: {name}");
            return folder;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ✗ Failed to create folder '{name}': {ex.Message}");
            throw new Exception($"Lỗi tạo folder '{name}': {ex.Message}", ex);
        }
    }
    
    private void CreateSubFolders(string parentId, string orgName, (string name, string icon)[] folders)
    {
        foreach (var (name, icon) in folders)
        {
            CreateFolder(name, parentId, icon, orgName);
        }
    }
}
