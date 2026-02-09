# 🗂️ HỆ THỐNG QUẢN LÝ CẤU TRÚC ALBUM THEO NGHIỆP VỤ

## 📋 TỔNG QUAN

Hệ thống quản lý Album ảnh theo cấu trúc chuẩn của các loại cơ quan hành chính tại Việt Nam.

### ✨ Tính năng chính:

1. **Cấu trúc Album theo nghiệp vụ** - Tự động tạo folder theo từng loại cơ quan
2. **Lưu trữ Localhost** - Database LiteDB tại `My Documents\AIVanBan\Data\documents.db`
3. **Đồng bộ từ Web** - Có thể tải cấu trúc mới từ server API
4. **Tự động tạo folder năm** - Các danh mục như "Sự kiện" tự động tạo folder theo năm
5. **Tags gợi ý** - Mỗi phân loại có sẵn tags để dễ tìm kiếm

---

## 🗄️ LƯU TRỮ DỮ LIỆU

### **1. Database (LiteDB)**

```
📂 C:\Users\[YourName]\Documents\AIVanBan\
├─ 📂 Data\
│  └─ 📄 documents.db           # Database LiteDB chứa tất cả dữ liệu
└─ 📂 Photos\                   # Folder chứa ảnh vật lý
   ├─ 📂 Sự kiện - Hội nghị\
   ├─ 📂 Công trình - Dự án\
   └─ ...
```

### **2. Collections trong Database**

#### **Collection: `albumTemplates`**
Lưu các template cấu trúc album theo loại cơ quan

```json
{
    "Id": "abc-123",
    "Name": "Cấu trúc Album - UBND Xã/Phường",
    "OrganizationType": "XaPhuong",
    "Version": "1.0",
    "Source": "local",  // hoặc "web-sync"
    "SyncUrl": "",      // URL để đồng bộ
    "LastSyncDate": "2024-01-15T10:30:00",
    "IsActive": true,
    "Categories": [...]
}
```

#### **Collection: `albumInstances`**
Lưu các album thực tế được tạo ra

```json
{
    "Id": "xyz-456",
    "Name": "[2024] Lễ khánh thành TH Hòa Bình",
    "FullPath": "Sự kiện - Hội nghị/Lễ khánh thành/[2024] Lễ khánh thành TH Hòa Bình",
    "PhysicalPath": "C:\\Users\\...\\Photos\\Sự kiện - Hội nghị\\...",
    "TemplateId": "abc-123",
    "CategoryId": "cat-001",
    "SubCategoryId": "sub-005",
    "PhotoCount": 120,
    "Tags": ["khánh thành", "trường học", "giáo dục"],
    "RelatedDocumentIds": ["doc-123", "doc-456"]
}
```

#### **Collection: `photos`**
Lưu metadata của từng ảnh

```json
{
    "Id": "photo-789",
    "FileName": "IMG_20240115_100530.jpg",
    "FilePath": "C:\\Users\\...\\Photos\\...",
    "ThumbnailPath": "C:\\Users\\...\\Thumbnails\\...",
    "AlbumId": "xyz-456",
    "DateTaken": "2024-01-15T10:05:30",
    "Event": "Lễ khánh thành Trường TH Hòa Bình",
    "Location": "Xã Hòa Bình, Huyện X",
    "Tags": ["khánh thành", "trường học"],
    "People": ["Chủ tịch UBND", "Hiệu trưởng"],
    "RelatedDocumentIds": ["doc-123"]
}
```

---

## 🌐 ĐỒNG BỘ TỪ WEB

### **Luồng hoạt động:**

```
┌─────────────┐         ┌─────────────┐         ┌──────────────┐
│   Desktop   │  HTTP   │  Web API    │  Query  │   Database   │
│   Client    │ ◄─────► │   Server    │ ◄─────► │  (SQL/NoSQL) │
└─────────────┘         └─────────────┘         └──────────────┘
      │
      │ Lưu local
      ▼
┌─────────────┐
│   LiteDB    │
│  localhost  │
└─────────────┘
```

### **API Endpoint (Dự kiến):**

#### **1. Lấy danh sách templates**
```http
GET https://api.example.com/album-templates
Response:
[
    {
        "OrganizationType": "XaPhuong",
        "Name": "Cấu trúc UBND Xã/Phường",
        "Version": "1.2",
        "DownloadUrl": "https://api.example.com/album-templates/xaphuong/v1.2"
    },
    ...
]
```

#### **2. Tải template cụ thể**
```http
GET https://api.example.com/album-templates/xaphuong/v1.2
Response: {AlbumStructureTemplate JSON}
```

#### **3. Kiểm tra version mới**
```http
GET https://api.example.com/album-templates/xaphuong/version
Response:
{
    "Version": "1.2",
    "ReleaseDate": "2024-02-01",
    "ChangeLog": "Thêm danh mục mới..."
}
```

### **Cách sử dụng trong code:**

```csharp
// 1. Đồng bộ từ web
var template = await _albumService.SyncTemplateFromWeb(
    "https://api.example.com/album-templates/xaphuong/latest",
    "XaPhuong"
);

// 2. Kiểm tra update
bool hasUpdate = await _albumService.CheckForUpdates(
    "https://api.example.com/album-templates/xaphuong"
);

// 3. Áp dụng template
_albumService.SetActiveTemplate(template.Id);
_albumService.CreatePhysicalStructure(template);
```

---

## 📂 CẤU TRÚC ALBUM - UBND XÃ/PHƯỜNG (ĐẦY ĐỦ)

### **12 Danh mục chính - 70+ Phân loại**

```
🖼️ ALBUM ẢNH
│
├─ 🎉 1. SỰ KIỆN - HỘI NGHỊ (9 phân loại)
│  ├─ 🏛️ Đại hội Đảng bộ
│  ├─ 🏢 Đại hội Hội đồng nhân dân
│  ├─ 👔 Hội nghị cán bộ công chức
│  ├─ 📋 Hội nghị triển khai nhiệm vụ
│  ├─ 🏗️ Lễ khánh thành công trình
│  ├─ 🚧 Lễ khởi công dự án
│  ├─ 🤝 Lễ ký kết hợp tác
│  ├─ 🏆 Lễ trao giải thưởng
│  └─ 💬 Hội thảo - Tọa đàm
│
├─ 🏗️ 2. CÔNG TRÌNH - DỰ ÁN (10 phân loại)
│  ├─ 🛣️ Giao thông - Đường giao thông
│  ├─ 🌊 Thủy lợi - Kênh mương
│  ├─ 🏫 Trường học - Giáo dục
│  ├─ 🏥 Trạm y tế
│  ├─ 🏟️ Nhà văn hóa - Khu thể thao
│  ├─ 💡 Điện - Nước sinh hoạt
│  ├─ 🏠 Nhà ở - Nhà tình nghĩa
│  ├─ 🏘️ Khu tái định cư
│  ├─ 🌉 Cầu - Cống
│  └─ 🏢 Công trình khác
│
├─ 📅 3. HOẠT ĐỘNG THƯỜNG XUYÊN (6 phân loại)
│  ├─ 🚩 Lễ chào cờ đầu tuần
│  ├─ 👥 Họp giao ban
│  ├─ 🔴 Sinh hoạt Chi bộ
│  ├─ ⭐ Sinh hoạt Đoàn - Hội
│  ├─ 📝 Tiếp dân - Giải quyết thủ tục
│  └─ 👮 Công tác tuần tra
│
├─ 🔍 4. KHẢO SÁT - THỰC ĐỊA (6 phân loại)
│  ├─ 📏 Khảo sát đất đai
│  ├─ 🔧 Kiểm tra công trình
│  ├─ 👨‍👩‍👧 Làm việc với hộ dân
│  ├─ 🌳 Kiểm tra môi trường
│  ├─ 🍎 Kiểm tra an toàn thực phẩm
│  └─ 📊 Khảo sát dân sinh
│
├─ 🎊 5. VĂN HÓA - LỄ HỘI (10 phân loại)
│  ├─ 🧧 Tết Nguyên Đán
│  ├─ 🥮 Tết Trung thu
│  ├─ 🎆 Ngày lễ lớn
│  ├─ 🎭 Lễ hội địa phương
│  ├─ 📚 Ngày Nhà giáo 20/11
│  ├─ 💐 Ngày Phụ nữ 8/3
│  ├─ 🎈 Ngày Quốc tế Thiếu nhi 1/6
│  ├─ 🚩 Ngày thành lập Đảng 3/2
│  ├─ 🎉 Ngày Giải phóng 30/4
│  └─ 🇻🇳 Ngày Quốc khánh 2/9
│
├─ 🎓 6. GIÁO DỤC - ĐÀO TẠO (5 phân loại)
│  ├─ 📖 Khai giảng năm học
│  ├─ 🎓 Lễ bế giảng
│  ├─ 🥇 Thi học sinh giỏi
│  ├─ 📚 Bồi dưỡng cán bộ
│  └─ 💼 Tập huấn nghiệp vụ
│
├─ ⚕️ 7. Y TẾ - SỨC KHỎE (5 phân loại)
│  ├─ 🩺 Khám sức khỏe định kỳ
│  ├─ 💉 Tiêm chủng - Phòng bệnh
│  ├─ 📢 Truyền thông sức khỏe
│  ├─ ❤️ Khám chữa bệnh miễn phí
│  └─ 🦠 Phòng chống dịch bệnh
│
├─ ❤️ 8. AN SINH - TỪ THIỆN (6 phân loại)
│  ├─ 🎁 Trao quà Tết
│  ├─ 🏠 Trao nhà tình thương
│  ├─ 🎒 Hỗ trợ học sinh nghèo
│  ├─ 🏅 Thăm hỏi gia đình chính sách
│  ├─ 👴 Hỗ trợ người già neo đơn
│  └─ ♿ Hỗ trợ người khuyết tật
│
├─ 🌾 9. NÔNG NGHIỆP - KINH TẾ (5 phân loại)
│  ├─ 🚜 Mô hình sản xuất
│  ├─ 🛒 Hội chợ nông sản
│  ├─ 👨‍🌾 Tập huấn kỹ thuật
│  ├─ 🌱 Công tác khuyến nông
│  └─ 🤝 Hợp tác xã
│
├─ 🛡️ 10. AN NINH - TRẬT TỰ (5 phân loại)
│  ├─ 👮 Tuần tra đảm bảo ANTT
│  ├─ 🚒 Tuyên truyền phòng cháy chữa cháy
│  ├─ 🎯 Diễn tập phòng thủ dân sự
│  ├─ ⚖️ Tuyên truyền pháp luật
│  └─ 🚦 An toàn giao thông
│
├─ 👥 11. TẬP THỂ - CÁ NHÂN (5 phân loại)
│  ├─ 📸 Ảnh tập thể lãnh đạo
│  ├─ 🎭 Ảnh cá nhân cán bộ
│  ├─ 🎤 Hoạt động văn nghệ
│  ├─ ⚽ Hoạt động thể thao
│  └─ 🏖️ Du lịch - Team building
│
└─ 📂 12. KHÁC (3 phân loại)
   ├─ 📚 Ảnh tài liệu lưu trữ
   ├─ 📄 Ảnh quét văn bản
   └─ 📁 Ảnh tự do
```

### **Tính năng đặc biệt:**

- ✅ **Auto-create year folder**: Các danh mục có tính chu kỳ sẽ tự động tạo folder theo năm
- ✅ **Suggested tags**: Mỗi phân loại có sẵn tags gợi ý để dễ tìm kiếm
- ✅ **Icon system**: Mỗi folder có icon riêng để dễ nhận biết
- ✅ **Metadata JSON**: Mỗi folder có file `album-info.json` chứa thông tin

---

## 💻 SỬ DỤNG TRONG CODE

### **1. Khởi tạo Service**

```csharp
var albumService = new AlbumStructureService();
```

### **2. Hiển thị Dialog thiết lập**

```csharp
var dialog = new AlbumStructureSetupDialog(albumService);
if (dialog.ShowDialog() == true)
{
    // User đã chọn và áp dụng template
    LoadAlbums(); // Reload UI
}
```

### **3. Lấy template đang active**

```csharp
var activeTemplate = albumService.GetActiveTemplate();
if (activeTemplate != null)
{
    Console.WriteLine($"Đang dùng: {activeTemplate.Name}");
}
```

### **4. Tạo album mới**

```csharp
var album = albumService.CreateAlbum(
    categoryId: "cat-001",
    subCategoryId: "sub-005",
    name: "[2024] Lễ khánh thành TH Hòa Bình",
    description: "Lễ khánh thành và đưa vào sử dụng trường..."
);
```

### **5. Thêm ảnh vào album**

```csharp
var photo = new PhotoExtended
{
    FileName = "IMG_001.jpg",
    FilePath = sourcePath,
    AlbumId = album.Id,
    Event = "Lễ khánh thành",
    Location = "Trường TH Hòa Bình",
    Tags = new[] { "khánh thành", "trường học", "giáo dục" }
};

albumService.AddPhoto(photo);
```

### **6. Tìm kiếm ảnh**

```csharp
var photos = albumService.SearchPhotos("khánh thành");
```

---

## 🚀 MỞ RỘNG SAU NÀY

### **1. Web API Server (PHP/Node.js/ASP.NET)**

Tạo server để cung cấp templates mới:

```
📂 album-templates-api/
├─ GET  /templates                    # Danh sách tất cả templates
├─ GET  /templates/{type}/latest      # Template mới nhất theo loại
├─ GET  /templates/{type}/v{version}  # Template cụ thể
├─ POST /templates                    # Upload template mới (admin)
└─ GET  /templates/{type}/version     # Kiểm tra version
```

### **2. Template Store - Marketplace**

```
- Cộng đồng chia sẻ templates
- Voting & rating
- Template cho từng ngành nghề đặc thù
- Tùy biến theo địa phương
```

### **3. Cloud Sync**

```
- Backup album lên cloud
- Sync giữa nhiều máy
- Team collaboration
```

---

## 📝 GHI CHÚ

- **Dung lượng database**: Rất nhỏ (~1-5 MB cho metadata, ảnh thực tế lưu ở folder)
- **Performance**: LiteDB hỗ trợ hàng triệu records, phù hợp với nhu cầu cơ quan
- **Backup**: Chỉ cần backup folder `AIVanBan` là đủ (bao gồm cả DB và ảnh)
- **Migration**: Dễ dàng export sang SQL Server hoặc PostgreSQL sau này

---

**Created**: 2026-02-05  
**Version**: 1.0  
**Author**: AIVanBan Development Team
