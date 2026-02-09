using LiteDB;
using AIVanBan.Core.Models;
using SystemJsonSerializer = System.Text.Json.JsonSerializer;
using System.Text.Json;
using System.Net.Http;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service quản lý cấu trúc Album theo nghiệp vụ cơ quan
/// Hỗ trợ đồng bộ từ web server
/// </summary>
public class AlbumStructureService : IDisposable
{
    private readonly LiteDatabase _db;
    private readonly string _dataPath;
    private readonly string _photosBasePath;
    private readonly HttpClient _httpClient;

    public AlbumStructureService(string? databasePath = null)
    {
        _dataPath = databasePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "AIVanBan",
            "Data"
        );

        _photosBasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "AIVanBan",
            "Photos"
        );

        Directory.CreateDirectory(_dataPath);
        Directory.CreateDirectory(_photosBasePath);

        var dbPath = Path.Combine(_dataPath, "documents.db");
        _db = new LiteDatabase($"Filename={dbPath};Connection=Shared");

        // Indexes
        var templates = _db.GetCollection<AlbumStructureTemplate>("albumTemplates");
        templates.EnsureIndex(x => x.OrganizationType);
        templates.EnsureIndex(x => x.IsActive);

        var albums = _db.GetCollection<AlbumInstance>("albumInstances");
        albums.EnsureIndex(x => x.CategoryId);
        albums.EnsureIndex(x => x.FullPath);

        var photos = _db.GetCollection<PhotoExtended>("photos");
        photos.EnsureIndex(x => x.AlbumId);
        photos.EnsureIndex(x => x.DateTaken);
        photos.EnsureIndex(x => x.Tags);

        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        // Note: InitializeDefaultTemplates() should be called explicitly after construction
        // to avoid database locking issues when multiple services access the same DB
    }

    #region Template Management

    /// <summary>
    /// Tạo các template mặc định cho các loại cơ quan
    /// Call this method explicitly after creating the service
    /// </summary>
    public void InitializeDefaultTemplates()
    {
        var collection = _db.GetCollection<AlbumStructureTemplate>("albumTemplates");
        
        if (collection.Count() == 0)
        {
            // CẤP XÃ/PHƯỜNG - CHÍNH QUYỀN
            var xaPhuongTemplate = CreateXaPhuongTemplate();
            collection.Insert(xaPhuongTemplate);

            var dangUyXaTemplate = CreateDangUyXaTemplate();
            collection.Insert(dangUyXaTemplate);

            var hdndXaTemplate = CreateHDNDXaTemplate();
            collection.Insert(hdndXaTemplate);

            var congAnXaTemplate = CreateCongAnXaTemplate();
            collection.Insert(congAnXaTemplate);

            var quanSuXaTemplate = CreateQuanSuXaTemplate();
            collection.Insert(quanSuXaTemplate);

            var tramYTeTemplate = CreateTramYTeTemplate();
            collection.Insert(tramYTeTemplate);

            // CẤP XÃ/PHƯỜNG - ĐOÀN THỂ
            var hoiNongDanTemplate = CreateHoiNongDanTemplate();
            collection.Insert(hoiNongDanTemplate);

            var hoiPhuNuTemplate = CreateHoiPhuNuTemplate();
            collection.Insert(hoiPhuNuTemplate);

            var doanTNTemplate = CreateDoanThanhNienTemplate();
            collection.Insert(doanTNTemplate);

            var hoiCCBTemplate = CreateHoiCuuChienBinhTemplate();
            collection.Insert(hoiCCBTemplate);

            var hoiNCTTemplate = CreateHoiNguoiCaoTuoiTemplate();
            collection.Insert(hoiNCTTemplate);

            var mttqTemplate = CreateMTTQTemplate();
            collection.Insert(mttqTemplate);

            // CẤP XÃ/PHƯỜNG - GIÁO DỤC
            var truongMNTemplate = CreateTruongMamNonTemplate();
            collection.Insert(truongMNTemplate);

            var truongTHTemplate = CreateTruongTieuHocTemplate();
            collection.Insert(truongTHTemplate);

            var truongTHCSTemplate = CreateTruongTHCSTemplate();
            collection.Insert(truongTHCSTemplate);

            // CẤP HUYỆN
            var huyenTemplate = CreateHuyenTemplate();
            collection.Insert(huyenTemplate);
        }
    }

    /// <summary>
    /// Cấu trúc album cho Xã/Phường - ĐẦY ĐỦ NHẤT
    /// </summary>
    private AlbumStructureTemplate CreateXaPhuongTemplate()
    {
        return new AlbumStructureTemplate
        {
            Name = "Cấu trúc Album - UBND Xã/Phường/Thị trấn",
            OrganizationType = "XaPhuong",
            Version = "1.0",
            Description = "Cấu trúc album chuẩn cho cơ quan UBND cấp xã",
            Source = "local",
            IsActive = true,
            Categories = new List<AlbumCategory>
            {
                // 1. SỰ KIỆN - HỘI NGHỊ
                new AlbumCategory
                {
                    Name = "Sự kiện - Hội nghị",
                    Icon = "🎉",
                    SortOrder = 1,
                    Description = "Các sự kiện quan trọng, hội nghị, lễ kỷ niệm",
                    SubCategories = new List<AlbumSubCategory>
                    {
                        new() { Name = "Đại hội Đảng bộ", Icon = "🏛️", SortOrder = 1, AutoCreateYearFolder = true, 
                            SuggestedTags = new[] { "đại hội", "đảng bộ", "nghị quyết" } },
                        new() { Name = "Đại hội Hội đồng nhân dân", Icon = "🏢", SortOrder = 2, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "hđnd", "nghị quyết", "kỳ họp" } },
                        new() { Name = "Hội nghị cán bộ công chức", Icon = "👔", SortOrder = 3, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "hội nghị", "cán bộ", "triển khai" } },
                        new() { Name = "Hội nghị triển khai nhiệm vụ", Icon = "📋", SortOrder = 4, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "triển khai", "nhiệm vụ", "kế hoạch" } },
                        new() { Name = "Lễ khánh thành công trình", Icon = "🏗️", SortOrder = 5, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "khánh thành", "công trình", "đưa vào sử dụng" } },
                        new() { Name = "Lễ khởi công dự án", Icon = "🚧", SortOrder = 6, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "khởi công", "dự án", "đầu tư" } },
                        new() { Name = "Lễ ký kết hợp tác", Icon = "🤝", SortOrder = 7, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "ký kết", "hợp tác", "biên bản" } },
                        new() { Name = "Lễ trao giải thưởng", Icon = "🏆", SortOrder = 8, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "trao giải", "khen thưởng", "danh hiệu" } },
                        new() { Name = "Hội thảo - Tọa đàm", Icon = "💬", SortOrder = 9, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "hội thảo", "tọa đàm", "chia sẻ" } },
                    }
                },

                // 2. CÔNG TRÌNH - DỰ ÁN
                new AlbumCategory
                {
                    Name = "Công trình - Dự án",
                    Icon = "🏗️",
                    SortOrder = 2,
                    Description = "Ảnh theo dõi tiến độ các công trình, dự án đầu tư",
                    SubCategories = new List<AlbumSubCategory>
                    {
                        new() { Name = "Giao thông - Đường giao thông", Icon = "🛣️", SortOrder = 1,
                            SuggestedTags = new[] { "giao thông", "đường", "bê tông" } },
                        new() { Name = "Thủy lợi - Kênh mương", Icon = "🌊", SortOrder = 2,
                            SuggestedTags = new[] { "thủy lợi", "kênh", "mương" } },
                        new() { Name = "Trường học - Giáo dục", Icon = "🏫", SortOrder = 3,
                            SuggestedTags = new[] { "trường học", "giáo dục", "xây mới" } },
                        new() { Name = "Trạm y tế", Icon = "🏥", SortOrder = 4,
                            SuggestedTags = new[] { "y tế", "trạm xá", "sức khỏe" } },
                        new() { Name = "Nhà văn hóa - Khu thể thao", Icon = "🏟️", SortOrder = 5,
                            SuggestedTags = new[] { "văn hóa", "thể thao", "cộng đồng" } },
                        new() { Name = "Điện - Nước sinh hoạt", Icon = "💡", SortOrder = 6,
                            SuggestedTags = new[] { "điện", "nước", "hạ tầng" } },
                        new() { Name = "Nhà ở - Nhà tình nghĩa", Icon = "🏠", SortOrder = 7,
                            SuggestedTags = new[] { "nhà ở", "tình nghĩa", "xã hội" } },
                        new() { Name = "Khu tái định cư", Icon = "🏘️", SortOrder = 8,
                            SuggestedTags = new[] { "tái định cư", "giải tỏa", "bồi thường" } },
                        new() { Name = "Cầu - Cống", Icon = "🌉", SortOrder = 9,
                            SuggestedTags = new[] { "cầu", "cống", "giao thông" } },
                        new() { Name = "Công trình khác", Icon = "🏢", SortOrder = 10,
                            SuggestedTags = new[] { "công trình", "xây dựng" } },
                    }
                },

                // 3. HOẠT ĐỘNG THƯỜNG XUYÊN
                new AlbumCategory
                {
                    Name = "Hoạt động thường xuyên",
                    Icon = "📅",
                    SortOrder = 3,
                    Description = "Các hoạt động diễn ra định kỳ, thường xuyên",
                    SubCategories = new List<AlbumSubCategory>
                    {
                        new() { Name = "Lễ chào cờ đầu tuần", Icon = "🚩", SortOrder = 1, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "chào cờ", "thứ hai", "lễ" } },
                        new() { Name = "Họp giao ban", Icon = "👥", SortOrder = 2, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "giao ban", "họp", "tuần" } },
                        new() { Name = "Sinh hoạt Chi bộ", Icon = "🔴", SortOrder = 3, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "chi bộ", "sinh hoạt", "đảng" } },
                        new() { Name = "Sinh hoạt Đoàn - Hội", Icon = "⭐", SortOrder = 4, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "đoàn", "hội", "thanh niên" } },
                        new() { Name = "Tiếp dân - Giải quyết thủ tục", Icon = "📝", SortOrder = 5, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "tiếp dân", "thủ tục", "hành chính" } },
                        new() { Name = "Công tác tuần tra", Icon = "👮", SortOrder = 6, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "tuần tra", "an ninh", "trật tự" } },
                    }
                },

                // 4. KHẢO SÁT - THỰC ĐỊA
                new AlbumCategory
                {
                    Name = "Khảo sát - Thực địa",
                    Icon = "🔍",
                    SortOrder = 4,
                    Description = "Ảnh khảo sát, kiểm tra hiện trường, làm việc với dân",
                    SubCategories = new List<AlbumSubCategory>
                    {
                        new() { Name = "Khảo sát đất đai", Icon = "📏", SortOrder = 1, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "khảo sát", "đất đai", "đo đạc" } },
                        new() { Name = "Kiểm tra công trình", Icon = "🔧", SortOrder = 2, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "kiểm tra", "công trình", "chất lượng" } },
                        new() { Name = "Làm việc với hộ dân", Icon = "👨‍👩‍👧", SortOrder = 3, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "dân", "hộ gia đình", "trao đổi" } },
                        new() { Name = "Kiểm tra môi trường", Icon = "🌳", SortOrder = 4, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "môi trường", "kiểm tra", "vệ sinh" } },
                        new() { Name = "Kiểm tra an toàn thực phẩm", Icon = "🍎", SortOrder = 5, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "vệ sinh", "thực phẩm", "an toàn" } },
                        new() { Name = "Khảo sát dân sinh", Icon = "📊", SortOrder = 6, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "khảo sát", "dân sinh", "thống kê" } },
                    }
                },

                // 5. VĂN HÓA - LỄ HỘI
                new AlbumCategory
                {
                    Name = "Văn hóa - Lễ hội",
                    Icon = "🎊",
                    SortOrder = 5,
                    Description = "Các hoạt động văn hóa, lễ hội truyền thống",
                    SubCategories = new List<AlbumSubCategory>
                    {
                        new() { Name = "Tết Nguyên Đán", Icon = "🧧", SortOrder = 1, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "tết", "nguyên đán", "xuân" } },
                        new() { Name = "Tết Trung thu", Icon = "🥮", SortOrder = 2, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "trung thu", "thiếu nhi", "lễ hội" } },
                        new() { Name = "Ngày lễ lớn", Icon = "🎆", SortOrder = 3, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "lễ", "kỷ niệm", "quốc gia" } },
                        new() { Name = "Lễ hội địa phương", Icon = "🎭", SortOrder = 4, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "lễ hội", "truyền thống", "địa phương" } },
                        new() { Name = "Ngày Nhà giáo 20/11", Icon = "📚", SortOrder = 5, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "nhà giáo", "20/11", "giáo viên" } },
                        new() { Name = "Ngày Phụ nữ 8/3", Icon = "💐", SortOrder = 6, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "phụ nữ", "8/3", "quốc tế" } },
                        new() { Name = "Ngày Quốc tế Thiếu nhi 1/6", Icon = "🎈", SortOrder = 7, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "thiếu nhi", "1/6", "trẻ em" } },
                        new() { Name = "Ngày thành lập Đảng 3/2", Icon = "🚩", SortOrder = 8, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "đảng", "3/2", "kỷ niệm" } },
                        new() { Name = "Ngày Giải phóng 30/4", Icon = "🎉", SortOrder = 9, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "30/4", "giải phóng", "thống nhất" } },
                        new() { Name = "Ngày Quốc khánh 2/9", Icon = "🇻🇳", SortOrder = 10, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "quốc khánh", "2/9", "độc lập" } },
                    }
                },

                // 6. GIÁO DỤC - ĐÀO TẠO
                new AlbumCategory
                {
                    Name = "Giáo dục - Đào tạo",
                    Icon = "🎓",
                    SortOrder = 6,
                    Description = "Hoạt động giáo dục, đào tạo, bồi dưỡng",
                    SubCategories = new List<AlbumSubCategory>
                    {
                        new() { Name = "Khai giảng năm học", Icon = "📖", SortOrder = 1, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "khai giảng", "năm học", "học sinh" } },
                        new() { Name = "Lễ bế giảng", Icon = "🎓", SortOrder = 2, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "bế giảng", "tốt nghiệp", "học sinh" } },
                        new() { Name = "Thi học sinh giỏi", Icon = "🥇", SortOrder = 3, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "thi", "học sinh giỏi", "khen thưởng" } },
                        new() { Name = "Bồi dưỡng cán bộ", Icon = "📚", SortOrder = 4, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "bồi dưỡng", "cán bộ", "đào tạo" } },
                        new() { Name = "Tập huấn nghiệp vụ", Icon = "💼", SortOrder = 5, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "tập huấn", "nghiệp vụ", "kỹ năng" } },
                    }
                },

                // 7. Y TẾ - SỨC KHỎE
                new AlbumCategory
                {
                    Name = "Y tế - Sức khỏe",
                    Icon = "⚕️",
                    SortOrder = 7,
                    Description = "Hoạt động y tế, chăm sóc sức khỏe cộng đồng",
                    SubCategories = new List<AlbumSubCategory>
                    {
                        new() { Name = "Khám sức khỏe định kỳ", Icon = "🩺", SortOrder = 1, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "khám", "sức khỏe", "định kỳ" } },
                        new() { Name = "Tiêm chủng - Phòng bệnh", Icon = "💉", SortOrder = 2, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "tiêm chủng", "vắc xin", "phòng bệnh" } },
                        new() { Name = "Truyền thông sức khỏe", Icon = "📢", SortOrder = 3, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "truyền thông", "y tế", "tuyên truyền" } },
                        new() { Name = "Khám chữa bệnh miễn phí", Icon = "❤️", SortOrder = 4, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "khám", "miễn phí", "từ thiện" } },
                        new() { Name = "Phòng chống dịch bệnh", Icon = "🦠", SortOrder = 5, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "phòng chống", "dịch bệnh", "y tế" } },
                    }
                },

                // 8. AN SINH - TỪ THIỆN
                new AlbumCategory
                {
                    Name = "An sinh - Từ thiện",
                    Icon = "❤️",
                    SortOrder = 8,
                    Description = "Hoạt động an sinh xã hội, từ thiện, hỗ trợ người nghèo",
                    SubCategories = new List<AlbumSubCategory>
                    {
                        new() { Name = "Trao quà Tết", Icon = "🎁", SortOrder = 1, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "tặng quà", "tết", "hộ nghèo" } },
                        new() { Name = "Trao nhà tình thương", Icon = "🏠", SortOrder = 2, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "nhà", "tình thương", "từ thiện" } },
                        new() { Name = "Hỗ trợ học sinh nghèo", Icon = "🎒", SortOrder = 3, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "học sinh", "nghèo", "học bổng" } },
                        new() { Name = "Thăm hỏi gia đình chính sách", Icon = "🏅", SortOrder = 4, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "chính sách", "thương binh", "liệt sĩ" } },
                        new() { Name = "Hỗ trợ người già neo đơn", Icon = "👴", SortOrder = 5, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "người già", "neo đơn", "trợ giúp" } },
                        new() { Name = "Hỗ trợ người khuyết tật", Icon = "♿", SortOrder = 6, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "khuyết tật", "trợ giúp", "xã hội" } },
                    }
                },

                // 9. NÔNG NGHIỆP - KINH TẾ
                new AlbumCategory
                {
                    Name = "Nông nghiệp - Kinh tế",
                    Icon = "🌾",
                    SortOrder = 9,
                    Description = "Hoạt động sản xuất nông nghiệp, phát triển kinh tế",
                    SubCategories = new List<AlbumSubCategory>
                    {
                        new() { Name = "Mô hình sản xuất", Icon = "🚜", SortOrder = 1, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "mô hình", "sản xuất", "nông nghiệp" } },
                        new() { Name = "Hội chợ nông sản", Icon = "🛒", SortOrder = 2, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "hội chợ", "nông sản", "tiêu thụ" } },
                        new() { Name = "Tập huấn kỹ thuật", Icon = "👨‍🌾", SortOrder = 3, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "tập huấn", "kỹ thuật", "nông dân" } },
                        new() { Name = "Công tác khuyến nông", Icon = "🌱", SortOrder = 4, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "khuyến nông", "tư vấn", "kỹ thuật" } },
                        new() { Name = "Hợp tác xã", Icon = "🤝", SortOrder = 5, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "hợp tác xã", "liên kết", "sản xuất" } },
                    }
                },

                // 10. AN NINH - TRẬT TỰ
                new AlbumCategory
                {
                    Name = "An ninh - Trật tự",
                    Icon = "🛡️",
                    SortOrder = 10,
                    Description = "Hoạt động đảm bảo an ninh trật tự, an toàn xã hội",
                    SubCategories = new List<AlbumSubCategory>
                    {
                        new() { Name = "Tuần tra đảm bảo ANTT", Icon = "👮", SortOrder = 1, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "tuần tra", "an ninh", "công an" } },
                        new() { Name = "Tuyên truyền phòng cháy chữa cháy", Icon = "🚒", SortOrder = 2, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "phòng cháy", "chữa cháy", "tuyên truyền" } },
                        new() { Name = "Diễn tập phòng thủ dân sự", Icon = "🎯", SortOrder = 3, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "diễn tập", "phòng thủ", "dân sự" } },
                        new() { Name = "Tuyên truyền pháp luật", Icon = "⚖️", SortOrder = 4, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "pháp luật", "tuyên truyền", "phổ biến" } },
                        new() { Name = "An toàn giao thông", Icon = "🚦", SortOrder = 5, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "giao thông", "an toàn", "trật tự" } },
                    }
                },

                // 11. TẬP THỂ - CÁ NHÂN
                new AlbumCategory
                {
                    Name = "Tập thể - Cá nhân",
                    Icon = "👥",
                    SortOrder = 11,
                    Description = "Ảnh tập thể, cá nhân cán bộ công chức",
                    SubCategories = new List<AlbumSubCategory>
                    {
                        new() { Name = "Ảnh tập thể lãnh đạo", Icon = "📸", SortOrder = 1, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "tập thể", "lãnh đạo", "chính thức" } },
                        new() { Name = "Ảnh cá nhân cán bộ", Icon = "🎭", SortOrder = 2, AutoCreateYearFolder = false,
                            SuggestedTags = new[] { "cá nhân", "cán bộ", "hồ sơ" } },
                        new() { Name = "Hoạt động văn nghệ", Icon = "🎤", SortOrder = 3, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "văn nghệ", "biểu diễn", "giải trí" } },
                        new() { Name = "Hoạt động thể thao", Icon = "⚽", SortOrder = 4, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "thể thao", "thi đấu", "giải" } },
                        new() { Name = "Du lịch - Team building", Icon = "🏖️", SortOrder = 5, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "du lịch", "team building", "gắn kết" } },
                    }
                },

                // 12. KHÁC
                new AlbumCategory
                {
                    Name = "Khác",
                    Icon = "📂",
                    SortOrder = 12,
                    Description = "Các album khác không thuộc danh mục trên",
                    SubCategories = new List<AlbumSubCategory>
                    {
                        new() { Name = "Ảnh tài liệu lưu trữ", Icon = "📚", SortOrder = 1, AutoCreateYearFolder = true,
                            SuggestedTags = new[] { "lưu trữ", "tài liệu", "tham khảo" } },
                        new() { Name = "Ảnh quét văn bản", Icon = "📄", SortOrder = 2, AutoCreateYearFolder = false,
                            SuggestedTags = new[] { "scan", "văn bản", "số hóa" } },
                        new() { Name = "Ảnh tự do", Icon = "📁", SortOrder = 3, AutoCreateYearFolder = false,
                            SuggestedTags = new[] { "khác", "tự do" } },
                    }
                }
            }
        };
    }

    /// <summary>
    /// Cấu trúc album cho Huyện
    /// </summary>
    private AlbumStructureTemplate CreateHuyenTemplate()
    {
        var template = CreateXaPhuongTemplate(); // Kế thừa từ xã
        template.Name = "Cấu trúc Album - UBND Huyện/Quận/Thị xã";
        template.OrganizationType = "Huyen";
        template.Description = "Cấu trúc album chuẩn cho cơ quan UBND cấp huyện";
        
        // Thêm các subcategory đặc thù cho cấp huyện
        var sukienCategory = template.Categories.First(c => c.Name == "Sự kiện - Hội nghị");
        sukienCategory.SubCategories.Add(new AlbumSubCategory 
        { 
            Name = "Hội nghị liên xã", 
            Icon = "🏛️", 
            SortOrder = 100,
            AutoCreateYearFolder = true,
            SuggestedTags = new[] { "liên xã", "cấp huyện", "điều hành" }
        });

        return template;
    }

    /// <summary>
    /// Cấu trúc album cho Hội Nông dân
    /// </summary>
    private AlbumStructureTemplate CreateHoiNongDanTemplate()
    {
        return new AlbumStructureTemplate
        {
            Name = "Cấu trúc Album - Hội Nông dân",
            OrganizationType = "HoiNongDan",
            Version = "1.0",
            Description = "Cấu trúc album cho tổ chức Hội Nông dân",
            Source = "local",
            IsActive = true,
            Categories = new List<AlbumCategory>
            {
                new AlbumCategory
                {
                    Name = "Đại hội - Hội nghị",
                    Icon = "🎉",
                    SortOrder = 1,
                    SubCategories = new List<AlbumSubCategory>
                    {
                        new() { Name = "Đại hội Hội Nông dân", Icon = "🏛️", SortOrder = 1, AutoCreateYearFolder = true },
                        new() { Name = "Hội nghị Ban chấp hành", Icon = "📋", SortOrder = 2, AutoCreateYearFolder = true },
                    }
                },
                new AlbumCategory
                {
                    Name = "Hoạt động sản xuất",
                    Icon = "🌾",
                    SortOrder = 2,
                    SubCategories = new List<AlbumSubCategory>
                    {
                        new() { Name = "Mô hình nông nghiệp", Icon = "🚜", SortOrder = 1, AutoCreateYearFolder = true },
                        new() { Name = "Tập huấn kỹ thuật", Icon = "👨‍🌾", SortOrder = 2, AutoCreateYearFolder = true },
                        new() { Name = "Chợ phiên - Hội chợ", Icon = "🛒", SortOrder = 3, AutoCreateYearFolder = true },
                    }
                },
                new AlbumCategory
                {
                    Name = "Hoạt động xã hội",
                    Icon = "❤️",
                    SortOrder = 3,
                    SubCategories = new List<AlbumSubCategory>
                    {
                        new() { Name = "Từ thiện - An sinh", Icon = "🎁", SortOrder = 1, AutoCreateYearFolder = true },
                        new() { Name = "Văn nghệ - Thể thao", Icon = "🎤", SortOrder = 2, AutoCreateYearFolder = true },
                    }
                }
            }
        };
    }

    private AlbumStructureTemplate CreateDangUyXaTemplate()
    {
        return new AlbumStructureTemplate
        {
            Name = "Đảng ủy Xã/Phường",
            OrganizationType = "DangUyXa",
            Version = "1.0",
            Description = "Cấu trúc album cho Đảng ủy cấp xã",
            Source = "local",
            IsActive = false,
            Categories = new List<AlbumCategory>
            {
                new AlbumCategory { Name = "Đại hội Đảng", Icon = "🏛️", SortOrder = 1, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Đại hội nhiệm kỳ", Icon = "🎉", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Đại hội điểm", Icon = "📍", SortOrder = 2, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Sinh hoạt Đảng", Icon = "📋", SortOrder = 2, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Hội nghị Ban chấp hành", Icon = "👥", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Sinh hoạt Chi bộ", Icon = "🏢", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Học tập lý luận", Icon = "📚", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Kiểm tra - Giám sát", Icon = "🔍", SortOrder = 3, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Kiểm tra nội bộ", Icon = "✅", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Giám sát cơ sở", Icon = "👀", SortOrder = 2, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Tuyên giáo", Icon = "📢", SortOrder = 4, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Tuyên truyền", Icon = "📣", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Văn hóa - Văn nghệ", Icon = "🎭", SortOrder = 2, AutoCreateYearFolder = true }
                }}
            }
        };
    }

    private AlbumStructureTemplate CreateHDNDXaTemplate()
    {
        return new AlbumStructureTemplate
        {
            Name = "HĐND Xã/Phường",
            OrganizationType = "HDNDXa",
            Version = "1.0",
            Description = "Cấu trúc album cho Hội đồng nhân dân cấp xã",
            Source = "local",
            IsActive = false,
            Categories = new List<AlbumCategory>
            {
                new AlbumCategory { Name = "Kỳ họp HĐND", Icon = "🏛️", SortOrder = 1, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Kỳ họp thường lệ", Icon = "📋", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Kỳ họp bất thường", Icon = "⚡", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Tiếp xúc cử tri", Icon = "👥", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Giám sát", Icon = "🔍", SortOrder = 2, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Giám sát chuyên đề", Icon = "📊", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Giám sát định kỳ", Icon = "📅", SortOrder = 2, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Đại biểu HĐND", Icon = "👤", SortOrder = 3, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Hoạt động đại biểu", Icon = "💼", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Bầu cử", Icon = "🗳️", SortOrder = 2, AutoCreateYearFolder = true }
                }}
            }
        };
    }

    private AlbumStructureTemplate CreateCongAnXaTemplate()
    {
        return new AlbumStructureTemplate
        {
            Name = "Công an Xã/Phường",
            OrganizationType = "CongAnXa",
            Version = "1.0",
            Description = "Cấu trúc album cho Công an cấp xã",
            Source = "local",
            IsActive = false,
            Categories = new List<AlbumCategory>
            {
                new AlbumCategory { Name = "An ninh - Trật tự", Icon = "🚔", SortOrder = 1, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Tuần tra kiểm soát", Icon = "👮", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Bảo vệ sự kiện", Icon = "🛡️", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Phòng cháy chữa cháy", Icon = "🚒", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Hành chính", Icon = "📋", SortOrder = 2, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Cấp CCCD", Icon = "🪪", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Đăng ký tạm trú", Icon = "🏠", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Quản lý cư trú", Icon = "📝", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Tuyên truyền", Icon = "📢", SortOrder = 3, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Tuyên truyền pháp luật", Icon = "⚖️", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Phổ biến kiến thức", Icon = "📚", SortOrder = 2, AutoCreateYearFolder = true }
                }}
            }
        };
    }

    private AlbumStructureTemplate CreateQuanSuXaTemplate()
    {
        return new AlbumStructureTemplate
        {
            Name = "Ban CHQS Xã/Phường",
            OrganizationType = "QuanSuXa",
            Version = "1.0",
            Description = "Cấu trúc album cho Ban Chỉ huy Quân sự cấp xã",
            Source = "local",
            IsActive = false,
            Categories = new List<AlbumCategory>
            {
                new AlbumCategory { Name = "Huấn luyện - Diễn tập", Icon = "⚔️", SortOrder = 1, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Huấn luyện dân quân", Icon = "🎖️", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Diễn tập chiến đấu", Icon = "💪", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Hội thao - Hội thi", Icon = "🏅", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Tuyển quân", Icon = "🪖", SortOrder = 2, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Tuyển chọn", Icon = "✅", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Giao quân", Icon = "🚌", SortOrder = 2, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Phòng thủ dân sự", Icon = "🛡️", SortOrder = 3, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Phòng chống thiên tai", Icon = "🌊", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Cứu nạn cứu hộ", Icon = "🚑", SortOrder = 2, AutoCreateYearFolder = true }
                }}
            }
        };
    }

    private AlbumStructureTemplate CreateTramYTeTemplate()
    {
        return new AlbumStructureTemplate
        {
            Name = "Trạm Y tế Xã/Phường",
            OrganizationType = "TramYTe",
            Version = "1.0",
            Description = "Cấu trúc album cho Trạm Y tế cấp xã",
            Source = "local",
            IsActive = false,
            Categories = new List<AlbumCategory>
            {
                new AlbumCategory { Name = "Khám chữa bệnh", Icon = "🏥", SortOrder = 1, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Khám bệnh", Icon = "👨‍⚕️", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Cấp cứu", Icon = "🚑", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Trang thiết bị y tế", Icon = "💉", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Y tế dự phòng", Icon = "💊", SortOrder = 2, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Tiêm chủng mở rộng", Icon = "💉", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Phòng dịch", Icon = "😷", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Dinh dưỡng", Icon = "🥗", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Chăm sóc sức khỏe", Icon = "❤️", SortOrder = 3, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Chăm sóc bà mẹ trẻ em", Icon = "👶", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Sức khỏe sinh sản", Icon = "🤰", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Người cao tuổi", Icon = "👴", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Vệ sinh môi trường", Icon = "🌱", SortOrder = 4, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "An toàn vệ sinh thực phẩm", Icon = "🍽️", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Nước sạch", Icon = "💧", SortOrder = 2, AutoCreateYearFolder = true }
                }}
            }
        };
    }

    private AlbumStructureTemplate CreateHoiPhuNuTemplate()
    {
        return new AlbumStructureTemplate
        {
            Name = "Hội Liên hiệp Phụ nữ",
            OrganizationType = "HoiPhuNu",
            Version = "1.0",
            Description = "Cấu trúc album cho Hội Phụ nữ cấp xã",
            Source = "local",
            IsActive = false,
            Categories = new List<AlbumCategory>
            {
                new AlbumCategory { Name = "Đại hội - Hội nghị", Icon = "👩", SortOrder = 1, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Đại hội phụ nữ", Icon = "🎉", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Hội nghị Ban chấp hành", Icon = "📋", SortOrder = 2, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Phát triển kinh tế", Icon = "💰", SortOrder = 2, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Mô hình kinh tế", Icon = "🏪", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Tập huấn nghề", Icon = "👩‍💼", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Hội chợ - Phiên chợ", Icon = "🛒", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Chăm sóc gia đình", Icon = "❤️", SortOrder = 3, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Gia đình hạnh phúc", Icon = "🏠", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Chăm sóc trẻ em", Icon = "👶", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Chăm sóc người già", Icon = "👵", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Hoạt động xã hội", Icon = "🎭", SortOrder = 4, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Văn nghệ", Icon = "🎤", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Từ thiện", Icon = "🎁", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Ngày lễ 8/3 - 20/10", Icon = "🌹", SortOrder = 3, AutoCreateYearFolder = true }
                }}
            }
        };
    }

    private AlbumStructureTemplate CreateDoanThanhNienTemplate()
    {
        return new AlbumStructureTemplate
        {
            Name = "Đoàn Thanh niên",
            OrganizationType = "DoanTN",
            Version = "1.0",
            Description = "Cấu trúc album cho Đoàn Thanh niên cấp xã",
            Source = "local",
            IsActive = false,
            Categories = new List<AlbumCategory>
            {
                new AlbumCategory { Name = "Đại hội - Hội nghị", Icon = "🎉", SortOrder = 1, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Đại hội Đoàn", Icon = "🏛️", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Hội nghị Ban chấp hành", Icon = "📋", SortOrder = 2, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Sinh hoạt - Học tập", Icon = "📚", SortOrder = 2, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Sinh hoạt chi đoàn", Icon = "👥", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Học tập lý luận", Icon = "📖", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Rèn luyện thanh niên", Icon = "💪", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Tình nguyện - Xã hội", Icon = "❤️", SortOrder = 3, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Mùa hè xanh", Icon = "☀️", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Xuân tình nguyện", Icon = "🌸", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Hiến máu nhân đạo", Icon = "🩸", SortOrder = 3, AutoCreateYearFolder = true },
                    new() { Name = "Xây dựng nông thôn mới", Icon = "🏘️", SortOrder = 4, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Văn hóa - Thể thao", Icon = "🎭", SortOrder = 4, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Văn nghệ", Icon = "🎤", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Thể thao", Icon = "⚽", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Ngày lễ 26/3", Icon = "🎊", SortOrder = 3, AutoCreateYearFolder = true }
                }}
            }
        };
    }

    private AlbumStructureTemplate CreateHoiCuuChienBinhTemplate()
    {
        return new AlbumStructureTemplate
        {
            Name = "Hội Cựu chiến binh",
            OrganizationType = "HoiCCB",
            Version = "1.0",
            Description = "Cấu trúc album cho Hội Cựu chiến binh cấp xã",
            Source = "local",
            IsActive = false,
            Categories = new List<AlbumCategory>
            {
                new AlbumCategory { Name = "Đại hội - Hội nghị", Icon = "🎖️", SortOrder = 1, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Đại hội Hội CCB", Icon = "🏛️", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Hội nghị Ban chấp hành", Icon = "📋", SortOrder = 2, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Hoạt động Hội", Icon = "👥", SortOrder = 2, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Sinh hoạt Hội", Icon = "🏢", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Học tập chính trị", Icon = "📚", SortOrder = 2, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Hoạt động xã hội", Icon = "❤️", SortOrder = 3, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Uống nước nhớ nguồn", Icon = "🙏", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Từ thiện - Tương trợ", Icon = "🎁", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Ngày thương binh liệt sĩ", Icon = "🕯️", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Phát triển kinh tế", Icon = "💰", SortOrder = 4, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Mô hình kinh tế", Icon = "🏪", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Giúp đỡ hội viên", Icon = "🤝", SortOrder = 2, AutoCreateYearFolder = true }
                }}
            }
        };
    }

    private AlbumStructureTemplate CreateHoiNguoiCaoTuoiTemplate()
    {
        return new AlbumStructureTemplate
        {
            Name = "Hội Người cao tuổi",
            OrganizationType = "HoiNCT",
            Version = "1.0",
            Description = "Cấu trúc album cho Hội Người cao tuổi cấp xã",
            Source = "local",
            IsActive = false,
            Categories = new List<AlbumCategory>
            {
                new AlbumCategory { Name = "Đại hội - Hội nghị", Icon = "👴", SortOrder = 1, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Đại hội Hội NCT", Icon = "🎉", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Hội nghị Ban chấp hành", Icon = "📋", SortOrder = 2, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Chăm sóc sức khỏe", Icon = "❤️", SortOrder = 2, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Khám bệnh từ thiện", Icon = "🏥", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Tư vấn sức khỏe", Icon = "👨‍⚕️", SortOrder = 2, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Hoạt động vui chơi", Icon = "🎭", SortOrder = 3, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Văn nghệ", Icon = "🎤", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Thể dục - Thể thao", Icon = "🧘", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Du lịch", Icon = "🚌", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Hoạt động xã hội", Icon = "🎁", SortOrder = 4, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Tết Sum vầy", Icon = "🏮", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Mừng thọ", Icon = "🎂", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Tặng quà", Icon = "🎁", SortOrder = 3, AutoCreateYearFolder = true }
                }}
            }
        };
    }

    private AlbumStructureTemplate CreateMTTQTemplate()
    {
        return new AlbumStructureTemplate
        {
            Name = "Ủy ban MTTQ Việt Nam",
            OrganizationType = "MTTQ",
            Version = "1.0",
            Description = "Cấu trúc album cho Mặt trận Tổ quốc cấp xã",
            Source = "local",
            IsActive = false,
            Categories = new List<AlbumCategory>
            {
                new AlbumCategory { Name = "Đại hội - Hội nghị", Icon = "🏛️", SortOrder = 1, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Đại hội MTTQ", Icon = "🎉", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Hội nghị Ủy ban", Icon = "📋", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Hội nghị đoàn thể", Icon = "👥", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Giám sát - Phản biện", Icon = "🔍", SortOrder = 2, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Giám sát xã hội", Icon = "👀", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Phản biện xã hội", Icon = "💬", SortOrder = 2, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Đại đoàn kết", Icon = "🤝", SortOrder = 3, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Ngày hội đại đoàn kết", Icon = "🎊", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Tôn giáo", Icon = "⛪", SortOrder = 2, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Vận động - Từ thiện", Icon = "❤️", SortOrder = 4, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Vận động nguồn lực", Icon = "💰", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Từ thiện - Tương trợ", Icon = "🎁", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Nhà đại đoàn kết", Icon = "🏠", SortOrder = 3, AutoCreateYearFolder = true }
                }}
            }
        };
    }

    private AlbumStructureTemplate CreateTruongMamNonTemplate()
    {
        return new AlbumStructureTemplate
        {
            Name = "Trường Mầm non",
            OrganizationType = "TruongMN",
            Version = "1.0",
            Description = "Cấu trúc album cho Trường Mầm non",
            Source = "local",
            IsActive = false,
            Categories = new List<AlbumCategory>
            {
                new AlbumCategory { Name = "Hoạt động giảng dạy", Icon = "👶", SortOrder = 1, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Lớp học", Icon = "🎨", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Chăm sóc trẻ", Icon = "❤️", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Vui chơi", Icon = "🎡", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Sự kiện - Lễ hội", Icon = "🎉", SortOrder = 2, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Ngày khai giảng", Icon = "🏫", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Ngày nhà giáo", Icon = "👩‍🏫", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Trung thu", Icon = "🏮", SortOrder = 3, AutoCreateYearFolder = true },
                    new() { Name = "Tết thiếu nhi 1/6", Icon = "🎈", SortOrder = 4, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Cơ sở vật chất", Icon = "🏢", SortOrder = 3, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Phòng học", Icon = "🚪", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Sân chơi", Icon = "🎠", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Bếp ăn", Icon = "🍽️", SortOrder = 3, AutoCreateYearFolder = true }
                }}
            }
        };
    }

    private AlbumStructureTemplate CreateTruongTieuHocTemplate()
    {
        return new AlbumStructureTemplate
        {
            Name = "Trường Tiểu học",
            OrganizationType = "TruongTH",
            Version = "1.0",
            Description = "Cấu trúc album cho Trường Tiểu học",
            Source = "local",
            IsActive = false,
            Categories = new List<AlbumCategory>
            {
                new AlbumCategory { Name = "Hoạt động giảng dạy", Icon = "📚", SortOrder = 1, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Lớp học", Icon = "✏️", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Dự giờ - Kiểm tra", Icon = "📋", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Ngoại khóa", Icon = "🎨", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Đội - Thiếu nhi", Icon = "🎗️", SortOrder = 2, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Sinh hoạt Đội", Icon = "👥", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Kết nạp Đội viên", Icon = "🎊", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Hoạt động Đội", Icon = "🚩", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Sự kiện - Lễ hội", Icon = "🎉", SortOrder = 3, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Khai giảng - Bế giảng", Icon = "🏫", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Ngày nhà giáo 20/11", Icon = "👨‍🏫", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Ngày thành lập Đội 15/5", Icon = "🎗️", SortOrder = 3, AutoCreateYearFolder = true },
                    new() { Name = "Ngày thiếu nhi 1/6", Icon = "🎈", SortOrder = 4, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Thi đua - Tuyên dương", Icon = "🏆", SortOrder = 4, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Học sinh giỏi", Icon = "🌟", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Hội thi", Icon = "🎯", SortOrder = 2, AutoCreateYearFolder = true }
                }}
            }
        };
    }

    private AlbumStructureTemplate CreateTruongTHCSTemplate()
    {
        return new AlbumStructureTemplate
        {
            Name = "Trường THCS",
            OrganizationType = "TruongTHCS",
            Version = "1.0",
            Description = "Cấu trúc album cho Trường Trung học cơ sở",
            Source = "local",
            IsActive = false,
            Categories = new List<AlbumCategory>
            {
                new AlbumCategory { Name = "Hoạt động giảng dạy", Icon = "📖", SortOrder = 1, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Lớp học", Icon = "✍️", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Dự giờ - Kiểm tra", Icon = "📋", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Ngoại khóa", Icon = "🎨", SortOrder = 3, AutoCreateYearFolder = true },
                    new() { Name = "Thí nghiệm", Icon = "🔬", SortOrder = 4, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Đoàn - Học sinh", Icon = "🎗️", SortOrder = 2, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Sinh hoạt Đoàn", Icon = "👥", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Kết nạp Đoàn viên", Icon = "🎊", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Hoạt động ngoại khóa", Icon = "🎭", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Sự kiện - Lễ hội", Icon = "🎉", SortOrder = 3, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Khai giảng - Bế giảng", Icon = "🏫", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Ngày nhà giáo 20/11", Icon = "👩‍🏫", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Ngày thành lập Đoàn 26/3", Icon = "🎗️", SortOrder = 3, AutoCreateYearFolder = true }
                }},
                new AlbumCategory { Name = "Thi đua - Thi HSG", Icon = "🏆", SortOrder = 4, SubCategories = new List<AlbumSubCategory> {
                    new() { Name = "Học sinh giỏi", Icon = "🌟", SortOrder = 1, AutoCreateYearFolder = true },
                    new() { Name = "Hội thi - Cuộc thi", Icon = "🎯", SortOrder = 2, AutoCreateYearFolder = true },
                    new() { Name = "Thi vào lớp 10", Icon = "📝", SortOrder = 3, AutoCreateYearFolder = true }
                }}
            }
        };
    }

    public List<AlbumStructureTemplate> GetAllTemplates()
    {
        var collection = _db.GetCollection<AlbumStructureTemplate>("albumTemplates");
        return collection.FindAll().ToList();
    }

    public AlbumStructureTemplate? GetActiveTemplate()
    {
        var collection = _db.GetCollection<AlbumStructureTemplate>("albumTemplates");
        return collection.FindOne(t => t.IsActive);
    }

    public bool SetActiveTemplate(string templateId)
    {
        var collection = _db.GetCollection<AlbumStructureTemplate>("albumTemplates");
        
        // Deactivate all
        var all = collection.FindAll().ToList();
        foreach (var t in all)
        {
            t.IsActive = false;
            collection.Update(t);
        }

        // Activate selected
        var template = collection.FindById(templateId);
        if (template != null)
        {
            template.IsActive = true;
            collection.Update(template);
            return true;
        }
        return false;
    }

    #endregion

    #region Sync from Web

    /// <summary>
    /// Đồng bộ template từ web server
    /// URL example: https://api.example.com/album-templates/xaphuong/latest
    /// </summary>
    public async Task<AlbumStructureTemplate?> SyncTemplateFromWeb(string syncUrl, string organizationType)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(syncUrl);
            var template = SystemJsonSerializer.Deserialize<AlbumStructureTemplate>(response);
            
            if (template != null)
            {
                template.Source = "web-sync";
                template.SyncUrl = syncUrl;
                template.LastSyncDate = DateTime.Now;
                template.OrganizationType = organizationType;

                var collection = _db.GetCollection<AlbumStructureTemplate>("albumTemplates");
                
                // Check if exists
                var existing = collection.FindOne(t => 
                    t.OrganizationType == organizationType && 
                    t.Source == "web-sync");

                if (existing != null)
                {
                    // Update
                    template.Id = existing.Id;
                    collection.Update(template);
                }
                else
                {
                    // Insert new
                    collection.Insert(template);
                }

                return template;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sync error: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Check xem có version mới trên server không
    /// </summary>
    public async Task<bool> CheckForUpdates(string syncUrl)
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{syncUrl}/version");
            var serverVersion = SystemJsonSerializer.Deserialize<VersionInfo>(response);
            
            if (serverVersion != null)
            {
                var localTemplate = GetActiveTemplate();
                if (localTemplate != null && !string.IsNullOrEmpty(localTemplate.SyncUrl))
                {
                    return CompareVersions(serverVersion.Version, localTemplate.Version) > 0;
                }
            }
        }
        catch { }

        return false;
    }

    private int CompareVersions(string v1, string v2)
    {
        var parts1 = v1.Split('.').Select(int.Parse).ToArray();
        var parts2 = v2.Split('.').Select(int.Parse).ToArray();
        
        for (int i = 0; i < Math.Min(parts1.Length, parts2.Length); i++)
        {
            if (parts1[i] != parts2[i])
                return parts1[i].CompareTo(parts2[i]);
        }
        
        return parts1.Length.CompareTo(parts2.Length);
    }

    #endregion

    #region Album Instance Management

    /// <summary>
    /// Tạo cấu trúc folder vật lý theo template
    /// </summary>
    public void CreatePhysicalStructure(AlbumStructureTemplate template)
    {
        foreach (var category in template.Categories)
        {
            var categoryPath = Path.Combine(_photosBasePath, category.Name);
            Directory.CreateDirectory(categoryPath);

            foreach (var subCategory in category.SubCategories)
            {
                var subCategoryPath = Path.Combine(categoryPath, subCategory.Name);
                Directory.CreateDirectory(subCategoryPath);

                // Auto create year folders if needed
                if (subCategory.AutoCreateYearFolder)
                {
                    var currentYear = DateTime.Now.Year;
                    for (int year = currentYear - 2; year <= currentYear + 1; year++)
                    {
                        var yearPath = Path.Combine(subCategoryPath, year.ToString());
                        Directory.CreateDirectory(yearPath);
                    }
                }

                // Save metadata
                SaveAlbumMetadata(subCategoryPath, subCategory);
            }
        }
    }

    private void SaveAlbumMetadata(string path, AlbumSubCategory subCategory)
    {
        try
        {
            var metadata = new
            {
                subCategory.Name,
                subCategory.Description,
                subCategory.SuggestedTags,
                CreatedDate = DateTime.Now
            };

            var jsonPath = Path.Combine(path, "album-info.json");
            var json = SystemJsonSerializer.Serialize(metadata, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            File.WriteAllText(jsonPath, json, System.Text.Encoding.UTF8);
        }
        catch { }
    }

    /// <summary>
    /// Tạo album instance mới
    /// </summary>
    public AlbumInstance CreateAlbum(string categoryId, string subCategoryId, string name, string description = "")
    {
        var template = GetActiveTemplate();
        if (template == null) throw new Exception("No active template found");

        var category = template.Categories.FirstOrDefault(c => c.Id == categoryId);
        var subCategory = category?.SubCategories.FirstOrDefault(s => s.Id == subCategoryId);
        
        if (category == null || subCategory == null)
            throw new Exception("Category or SubCategory not found");

        var fullPath = $"{category.Name}/{subCategory.Name}/{name}";
        var physicalPath = Path.Combine(_photosBasePath, fullPath);
        Directory.CreateDirectory(physicalPath);

        var album = new AlbumInstance
        {
            Name = name,
            FullPath = fullPath,
            PhysicalPath = physicalPath,
            TemplateId = template.Id,
            CategoryId = categoryId,
            SubCategoryId = subCategoryId,
            Description = description,
            Tags = subCategory.SuggestedTags,
            Icon = subCategory.Icon
        };

        var collection = _db.GetCollection<AlbumInstance>("albumInstances");
        collection.Insert(album);

        return album;
    }

    public List<AlbumInstance> GetAllAlbums()
    {
        var collection = _db.GetCollection<AlbumInstance>("albumInstances");
        return collection.FindAll().ToList();
    }

    public List<AlbumInstance> GetAlbumsByCategory(string categoryId)
    {
        var collection = _db.GetCollection<AlbumInstance>("albumInstances");
        return collection.Find(a => a.CategoryId == categoryId).ToList();
    }

    #endregion

    #region Photos Management

    public PhotoExtended AddPhoto(PhotoExtended photo)
    {
        var collection = _db.GetCollection<PhotoExtended>("photos");
        collection.Insert(photo);

        // Update album photo count
        UpdateAlbumPhotoCount(photo.AlbumId);

        return photo;
    }

    public List<PhotoExtended> GetPhotosByAlbum(string albumId)
    {
        var collection = _db.GetCollection<PhotoExtended>("photos");
        return collection.Find(p => p.AlbumId == albumId).ToList();
    }

    public List<PhotoExtended> SearchPhotos(string keyword)
    {
        var collection = _db.GetCollection<PhotoExtended>("photos");
        return collection.Find(p => 
            p.FileName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            p.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            p.Event.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            p.Location.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            p.Tags.Any(t => t.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        ).ToList();
    }

    private void UpdateAlbumPhotoCount(string albumId)
    {
        var albumCollection = _db.GetCollection<AlbumInstance>("albumInstances");
        var album = albumCollection.FindById(albumId);
        if (album != null)
        {
            album.PhotoCount = GetPhotosByAlbum(albumId).Count;
            albumCollection.Update(album);
        }
    }

    #endregion

    public void Dispose()
    {
        _db?.Dispose();
        _httpClient?.Dispose();
    }
}

/// <summary>
/// Version info từ server
/// </summary>
public class VersionInfo
{
    public string Version { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string ChangeLog { get; set; } = string.Empty;
}
