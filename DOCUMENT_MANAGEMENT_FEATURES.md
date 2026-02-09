# 🏛️ HỆ THỐNG QUẢN LÝ TÀI LIỆU CƠ QUAN - DMS (Document Management System)

## 🎯 MỤC ĐÍCH & ĐỐI TƯỢNG SỬ DỤNG

**Phần mềm dành cho:** Cán bộ, công chức cấp xã/phường/thị trấn ở Việt Nam

**Đặc thù nghiệp vụ:**
- ✅ Quản lý văn bản đi/đến theo quy định văn thư Nhà nước
- ✅ Phân loại theo nguồn: Trung ương → Tỉnh → Huyện → Xã
- ✅ Quản lý hồ sơ công việc theo 6 lĩnh vực chính (Nội vụ, Tài chính, Đất đai, Văn hóa, Kinh tế, An ninh)
- ✅ Workflow phê duyệt văn bản đi: Nháp → Trình ký → Đã ký → Phát hành
- ✅ Phân quyền theo phòng ban (VP-TH, HC-QP, TC-KH, TP-HT, Album...)
- ✅ Lưu trữ theo quy định (văn bản vĩnh viễn, thời hạn 70 năm, 10 năm...)

---

## 📊 HIỆN TRẠNG HỆ THỐNG

### ✅ ĐÃ CÓ (Current Implementation)

#### 1. **Giao diện cơ bản**
- ✅ Split view: TreeView (250px) + DataGrid
- ✅ Material Design UI
- ✅ Tìm kiếm, lọc cơ bản
- ✅ CRUD văn bản

#### 2. **Thư mục đơn giản**
- ✅ TreeView hierarchy
- ✅ Document count
- ⚠️ **CHƯA ĐÚNG NGHIỆP VỤ** - Thiếu cấu trúc chuẩn cơ quan hành chính

---

## 🏗️ CẤU TRÚC THƯ MỤC CHUẨN CƠ QUAN NHÀ NƯỚC

### 📋 11 PHẦN CHÍNH (Standard Folder Structure)

### 📋 11 PHẦN CHÍNH (Standard Folder Structure)

```
📁 KHO TÀI LIỆU CƠ QUAN
│
├─ 📂 01. VĂN BẢN PHÁP LUẬT
│  ├─ 📂 Hiến pháp
│  ├─ 📂 Luật
│  ├─ 📂 Pháp lệnh
│  ├─ 📂 Nghị quyết (Quốc hội, HĐND)
│  ├─ 📂 Nghị định (Chính phủ)
│  ├─ 📂 Thông tư (Bộ, ngành)
│  ├─ 📂 Quyết định (UBND các cấp)
│  ├─ 📂 Chỉ thị
│  └─ 📂 Hướng dẫn, Quy định
│
├─ 📂 02. VĂN BẢN ĐI (Phát hành)
│  ├─ 📂 [Năm 2024]
│  │  ├─ 📂 Công văn đi
│  │  ├─ 📂 Quyết định
│  │  ├─ 📂 Thông báo
│  │  ├─ 📂 Báo cáo (gửi cấp trên)
│  │  ├─ 📂 Tờ trình
│  │  └─ 📂 Kế hoạch
│  ├─ 📂 [Năm 2025]
│  └─ 📂 [Năm 2026]
│
├─ 📂 03. VĂN BẢN ĐẾN (Tiếp nhận)
│  ├─ 📂 [Năm 2024]
│  │  ├─ 📂 Từ Trung ương (Chính phủ, Bộ)
│  │  ├─ 📂 Từ cấp Tỉnh (UBND, Sở)
│  │  ├─ 📂 Từ cấp Huyện (UBND, Phòng)
│  │  ├─ 📂 Từ các xã/phường
│  │  └─ 📂 Từ tổ chức, cá nhân
│  ├─ 📂 [Năm 2025]
│  └─ 📂 [Năm 2026]
│
├─ 📂 04. HỒ SƠ CÔNG VIỆC (Theo lĩnh vực)
│  ├─ 📂 Nội vụ - Tổ chức
│  │  ├─ 📂 Biên chế, tuyển dụng
│  │  ├─ 📂 Đào tạo, bồi dưỡng
│  │  └─ 📂 Khen thưởng, kỷ luật
│  ├─ 📂 Tài chính - Ngân sách
│  │  ├─ 📂 Dự toán
│  │  ├─ 📂 Quyết toán
│  │  └─ 📂 Thu chi
│  ├─ 📂 Đất đai - Xây dựng
│  │  ├─ 📂 Cấp giấy CNQSD đất
│  │  ├─ 📂 Giấy phép xây dựng
│  │  └─ 📂 Quy hoạch
│  ├─ 📂 Văn hóa - Xã hội
│  │  ├─ 📂 Giáo dục
│  │  ├─ 📂 Y tế
│  │  └─ 📂 Thể thao, văn nghệ
│  ├─ 📂 Kinh tế - Phát triển
│  │  ├─ 📂 Nông nghiệp
│  │  ├─ 📂 Công nghiệp, thương mại
│  │  └─ 📂 Du lịch
│  └─ 📂 An ninh - Trật tự
│
├─ 📂 05. HỒ SƠ DỰ ÁN - CÔNG TRÌNH
│  ├─ 📂 [Tên dự án 1]
│  │  ├─ 📂 Văn bản phê duyệt
│  │  ├─ 📂 Hồ sơ thiết kế
│  │  ├─ 📂 Hợp đồng, thầu
│  │  ├─ 📂 Tiến độ thi công
│  │  ├─ 📂 Nghiệm thu
│  │  └─ 🖼️ Album ảnh công trình
│  └─ 📂 [Tên dự án 2]
│
├─ 🖼️ 06. ALBUM ẢNH - HÌNH ẢNH
│  ├─ 📂 Sự kiện - Hội nghị
│  │  ├─ 📂 [2024] Đại hội Đảng bộ
│  │  ├─ 📂 [2024] Lễ khánh thành
│  │  └─ 📂 [2025] Hội nghị cán bộ
│  ├─ 📂 Hoạt động thường xuyên
│  │  ├─ 📂 Lễ chào cờ
│  │  ├─ 📂 Sinh hoạt Đảng, Đoàn
│  │  └─ 📂 Họp giao ban
│  ├─ 📂 Công trình - Dự án
│  │  ├─ 📂 Trước thi công
│  │  ├─ 📂 Trong thi công
│  │  └─ 📂 Sau hoàn thành
│  ├─ 📂 Khảo sát - Thực địa
│  │  ├─ 📂 Khảo sát đất đai
│  │  ├─ 📂 Kiểm tra hiện trường
│  │  └─ 📂 Làm việc với dân
│  ├─ 📂 Văn hóa - Lễ hội
│  │  ├─ 📂 Tết Nguyên Đán
│  │  ├─ 📂 Ngày lễ lớn
│  │  └─ 📂 Lễ hội địa phương
│  └─ 📂 Tập thể - Cá nhân
│      ├─ 📂 Ảnh tập thể lãnh đạo
│      └─ 📂 Hoạt động CBCC
│
├─ 📂 07. MẪU VĂN BẢN - TEMPLATE
│  ├─ 📂 Mẫu theo loại
│  │  ├─ 📄 Công văn.docx
│  │  ├─ 📄 Báo cáo.docx
│  │  ├─ 📄 Tờ trình.docx
│  │  ├─ 📄 Quyết định.docx
│  │  └─ 📄 Kế hoạch.docx
│  └─ 📂 Mẫu theo lĩnh vực
│      ├─ 📂 Nội vụ
│      ├─ 📂 Tài chính
│      ├─ 📂 Đất đai
│      └─ 📂 Văn hóa - Xã hội
│
├─ 📂 08. BÁO CÁO - THỐNG KÊ
│  ├─ 📂 Báo cáo định kỳ
│  │  ├─ 📂 Tuần
│  │  ├─ 📂 Tháng
│  │  ├─ 📂 Quý
│  │  └─ 📂 Năm
│  └─ 📂 Báo cáo chuyên đề
│
├─ 📂 09. TÀI LIỆU HỌC TẬP - NGHIỆP VỤ
│  ├─ 📂 Tài liệu đào tạo
│  ├─ 📂 Hướng dẫn nghiệp vụ
│  ├─ 📂 Sách chuyên ngành
│  └─ 📂 Bài giảng, slide
│
├─ 📂 10. LƯU TRỮ - ĐÃ HẾT HIỆU LỰC
│  ├─ 📂 Văn bản cũ (trước 2020)
│  ├─ 📂 Văn bản đã thay thế
│  └─ 📂 Hồ sơ đã đóng
│
└─ 📂 11. CÁ NHÂN (Workspace riêng)
   ├─ 📂 Văn bản nháp
   ├─ 📂 Ghi chú công việc
   └─ 📂 Tài liệu cá nhân
```

---

## 🚀 TÍNH NĂNG THEO NGHIỆP VỤ THỰC TẾ

### 1. **SETUP BAN ĐẦU - Chọn loại cơ quan** 🌟 ƯU TIÊN CAO

**Bước 1: First-time Setup Wizard**

Khi chạy lần đầu, hiển thị wizard để chọn loại cơ quan:

```xml
<Window Title="🏛️ THIẾT LẬP CƠ QUAN" Width="600" Height="500">
    <StackPanel Margin="30">
        <TextBlock Text="🏛️ THIẾT LẬP CƠ QUAN" 
                   FontSize="24" FontWeight="Bold" 
                   Margin="0,0,0,20"/>
        
        <TextBlock Text="Chọn loại cơ quan của bạn:" 
                   FontSize="14" Margin="0,0,0,10"/>
        
        <!-- Radio buttons -->
        <RadioButton Content="○ UBND Xã/Phường/Thị trấn" 
                     FontSize="14" Margin="0,10"/>
        <RadioButton Content="○ UBND Huyện/Quận/Thị xã" 
                     FontSize="14" Margin="0,10"/>
        <RadioButton Content="○ UBND Tỉnh/Thành phố trực thuộc TW" 
                     FontSize="14" Margin="0,10"/>
        <RadioButton Content="○ Hội Nông dân" 
                     FontSize="14" Margin="0,10"/>
        <RadioButton Content="○ Hội Phụ nữ" 
                     FontSize="14" Margin="0,10"/>
        <RadioButton Content="○ Đoàn Thanh niên" 
                     FontSize="14" Margin="0,10"/>
        <RadioButton Content="○ Hội Cựu chiến binh" 
                     FontSize="14" Margin="0,10"/>
        <RadioButton Content="○ Công đoàn" 
                     FontSize="14" Margin="0,10"/>
        <RadioButton Content="○ Sở/Ban/Ngành (cấp tỉnh)" 
                     FontSize="14" Margin="0,10"/>
        <RadioButton Content="○ Phòng (cấp huyện)" 
                     FontSize="14" Margin="0,10"/>
        <RadioButton Content="○ Trường học (MN/TH/THCS/THPT/ĐH)" 
                     FontSize="14" Margin="0,10"/>
        <RadioButton Content="○ Trạm y tế/Bệnh viện" 
                     FontSize="14" Margin="0,10"/>
        <RadioButton Content="○ Tùy chỉnh (Tự thiết kế)" 
                     FontSize="14" Margin="0,10"/>
        
        <StackPanel Orientation="Horizontal" 
                    HorizontalAlignment="Right" 
                    Margin="0,30,0,0">
            <Button Content="[Tiếp theo →]" 
                    Width="120" Height="40"
                    Click="NextStep_Click"/>
        </StackPanel>
    </StackPanel>
</Window>
```

**Bước 2: Tên cơ quan**

```xml
<StackPanel>
    <TextBlock Text="Tên đầy đủ cơ quan:" Margin="0,0,0,10"/>
    <TextBox Text="ỦY BAN NHÂN DÂN XÃ HÒA BÌNH" 
             FontSize="16" Padding="10"/>
    
    <TextBlock Text="Tên viết tắt:" Margin="0,20,0,10"/>
    <TextBox Text="UBND xã Hòa Bình" 
             FontSize="16" Padding="10"/>
</StackPanel>
```

**Bước 3: Tự động tạo cấu trúc**

Code tự động tạo 11 folder chính + sub-folders theo loại cơ quan:

```csharp
public class OrganizationSetupService
{
    public void CreateFolderStructure(OrganizationType type, string orgName)
    {
        switch (type)
        {
            case OrganizationType.UBNDXa:
                CreateStandardStructure(orgName);
                AddSpecificFolders_UBNDXa();
                break;
            
            case OrganizationType.TruongHoc:
                CreateStandardStructure(orgName);
                AddSpecificFolders_School();
                break;
                
            // ... other types
        }
    }
    
    private void CreateStandardStructure(string orgName)
    {
        // Tạo 11 folder chính
        var folders = new[]
        {
            "01. VĂN BẢN PHÁP LUẬT",
            "02. VĂN BẢN ĐI",
            "03. VĂN BẢN ĐẾN",
            "04. HỒ SƠ CÔNG VIỆC",
            "05. HỒ SƠ DỰ ÁN - CÔNG TRÌNH",
            "06. ALBUM ẢNH",
            "07. MẪU VĂN BẢN",
            "08. BÁO CÁO - THỐNG KÊ",
            "09. TÀI LIỆU HỌC TẬP",
            "10. LƯU TRỮ",
            "11. CÁ NHÂN"
        };
        
        foreach (var folder in folders)
        {
            _documentService.CreateFolder(new Folder
            {
                Name = folder,
                ParentId = null,
                Icon = "📂",
                OrganizationName = orgName
            });
        }
    }
    
    private void AddSpecificFolders_UBNDXa()
    {
        // Văn bản pháp luật
        var plFolder = FindFolder("01. VĂN BẢN PHÁP LUẬT");
        CreateSubFolders(plFolder.Id, new[]
        {
            "Hiến pháp",
            "Luật",
            "Pháp lệnh",
            "Nghị quyết (Quốc hội, HĐND)",
            "Nghị định (Chính phủ)",
            "Thông tư (Bộ, ngành)",
            "Quyết định (UBND các cấp)",
            "Chỉ thị",
            "Hướng dẫn, Quy định"
        });
        
        // Văn bản đi - Tạo folders theo năm
        var vbDiFolder = FindFolder("02. VĂN BẢN ĐI");
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            var yearFolder = CreateFolder($"[Năm {year}]", vbDiFolder.Id);
            CreateSubFolders(yearFolder.Id, new[]
            {
                "Công văn đi",
                "Quyết định",
                "Thông báo",
                "Báo cáo (gửi cấp trên)",
                "Tờ trình",
                "Kế hoạch"
            });
        }
        
        // Văn bản đến - Tạo folders theo năm + nguồn
        var vbDenFolder = FindFolder("03. VĂN BẢN ĐẾN");
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            var yearFolder = CreateFolder($"[Năm {year}]", vbDenFolder.Id);
            CreateSubFolders(yearFolder.Id, new[]
            {
                "Từ Trung ương (Chính phủ, Bộ)",
                "Từ cấp Tỉnh (UBND, Sở)",
                "Từ cấp Huyện (UBND, Phòng)",
                "Từ các xã/phường",
                "Từ tổ chức, cá nhân"
            });
        }
        
        // Hồ sơ công việc - 6 lĩnh vực chính
        var hscvFolder = FindFolder("04. HỒ SƠ CÔNG VIỆC");
        
        // 1. Nội vụ - Tổ chức
        var nvFolder = CreateFolder("Nội vụ - Tổ chức", hscvFolder.Id);
        CreateSubFolders(nvFolder.Id, new[]
        {
            "Biên chế, tuyển dụng",
            "Đào tạo, bồi dưỡng",
            "Khen thưởng, kỷ luật"
        });
        
        // 2. Tài chính - Ngân sách
        var tcFolder = CreateFolder("Tài chính - Ngân sách", hscvFolder.Id);
        CreateSubFolders(tcFolder.Id, new[]
        {
            "Dự toán",
            "Quyết toán",
            "Thu chi"
        });
        
        // 3. Đất đai - Xây dựng
        var ddFolder = CreateFolder("Đất đai - Xây dựng", hscvFolder.Id);
        CreateSubFolders(ddFolder.Id, new[]
        {
            "Cấp giấy CNQSD đất",
            "Giấy phép xây dựng",
            "Quy hoạch"
        });
        
        // 4. Văn hóa - Xã hội
        var vhFolder = CreateFolder("Văn hóa - Xã hội", hscvFolder.Id);
        CreateSubFolders(vhFolder.Id, new[]
        {
            "Giáo dục",
            "Y tế",
            "Thể thao, văn nghệ"
        });
        
        // 5. Kinh tế - Phát triển
        var ktFolder = CreateFolder("Kinh tế - Phát triển", hscvFolder.Id);
        CreateSubFolders(ktFolder.Id, new[]
        {
            "Nông nghiệp",
            "Công nghiệp, thương mại",
            "Du lịch"
        });
        
        // 6. An ninh - Trật tự
        CreateFolder("An ninh - Trật tự", hscvFolder.Id);
        
        // Album ảnh - Tích hợp với PhotoAlbumPageNew
        var albumFolder = FindFolder("06. ALBUM ẢNH");
        CreateSubFolders(albumFolder.Id, new[]
        {
            "Sự kiện - Hội nghị",
            "Hoạt động thường xuyên",
            "Công trình - Dự án",
            "Khảo sát - Thực địa",
            "Văn hóa - Lễ hội",
            "Tập thể - Cá nhân"
        });
        
        // Mẫu văn bản
        var mauVBFolder = FindFolder("07. MẪU VĂN BẢN");
        var mauTheoLoai = CreateFolder("Mẫu theo loại", mauVBFolder.Id);
        var mauTheoLinhVuc = CreateFolder("Mẫu theo lĩnh vực", mauVBFolder.Id);
        
        // Báo cáo - Thống kê
        var baoCaoFolder = FindFolder("08. BÁO CÁO - THỐNG KÊ");
        var bcDinhKy = CreateFolder("Báo cáo định kỳ", baoCaoFolder.Id);
        CreateSubFolders(bcDinhKy.Id, new[] { "Tuần", "Tháng", "Quý", "Năm" });
        CreateFolder("Báo cáo chuyên đề", baoCaoFolder.Id);
        
        // Tài liệu học tập
        var tlhtFolder = FindFolder("09. TÀI LIỆU HỌC TẬP");
        CreateSubFolders(tlhtFolder.Id, new[]
        {
            "Tài liệu đào tạo",
            "Hướng dẫn nghiệp vụ",
            "Sách chuyên ngành",
            "Bài giảng, slide"
        });
        
        // Lưu trữ
        var luuTruFolder = FindFolder("10. LƯU TRỮ");
        CreateSubFolders(luuTruFolder.Id, new[]
        {
            "Văn bản cũ (trước 2020)",
            "Văn bản đã thay thế",
            "Hồ sơ đã đóng"
        });
        
        // Cá nhân
        var caNhanFolder = FindFolder("11. CÁ NHÂN");
        CreateSubFolders(caNhanFolder.Id, new[]
        {
            "Văn bản nháp",
            "Ghi chú công việc",
            "Tài liệu cá nhân"
        });
    }
}
```

---

### 2. **PHÂN QUYỀN PHÒNG BAN** 🌟 ƯU TIÊN CAO

**Cấu trúc phòng ban điển hình UBND xã:**

```csharp
public enum Department
{
    VP_TH,      // Văn phòng - Tổng hợp
    HC_QP,      // Hành chính - Quốc phòng
    TC_KH,      // Tài chính - Kế hoạch
    TP_HT,      // Tư pháp - Hộ tịch
    NN_TN,      // Nông nghiệp - Tài nguyên
    VH_XH,      // Văn hóa - Xã hội
    ALBUM       // Quản lý Album ảnh
}

public class User
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Department Department { get; set; }
    public UserRole Role { get; set; } // Admin, Leader, Staff, Viewer
}

public enum UserRole
{
    Admin,      // Toàn quyền (Chủ tịch, Phó Chủ tịch)
    Leader,     // Trưởng/Phó phòng - Xem/Sửa văn bản của phòng mình
    Staff,      // Cán bộ - Tạo/Sửa văn bản nháp, không xóa
    Viewer      // Chỉ xem - Không sửa xóa
}
```

**UI: Filter theo phòng ban**

```xml
<!-- Thêm vào toolbar -->
<ComboBox x:Name="cboDepartment" 
          materialDesign:HintAssist.Hint="[Phòng ban ▼]"
          Width="200" Margin="0,0,10,0"
          SelectionChanged="DepartmentFilter_Changed">
    <ComboBoxItem Content="Tất cả" IsSelected="True"/>
    <ComboBoxItem Content="▶ Văn phòng - Tổng hợp"/>
    <ComboBoxItem Content="▶ Hành chính - Quốc phòng"/>
    <ComboBoxItem Content="▶ Tài chính - Kế hoạch"/>
    <ComboBoxItem Content="▶ Tư pháp - Hộ tịch"/>
    <ComboBoxItem Content="▶ Nông nghiệp - Tài nguyên"/>
    <ComboBoxItem Content="▶ Văn hóa - Xã hội"/>
    <ComboBoxItem Content="▶ Album"/>
</ComboBox>
```

**Phân quyền logic:**

```csharp
public class PermissionService
{
    private User _currentUser;
    
    public bool CanView(Document doc)
    {
        // Admin xem tất cả
        if (_currentUser.Role == UserRole.Admin) return true;
        
        // Văn bản công khai - ai cũng xem được
        if (doc.IsPublic) return true;
        
        // Chỉ xem văn bản của phòng mình
        return doc.Department == _currentUser.Department;
    }
    
    public bool CanEdit(Document doc)
    {
        // Admin sửa tất cả
        if (_currentUser.Role == UserRole.Admin) return true;
        
        // Viewer không sửa
        if (_currentUser.Role == UserRole.Viewer) return false;
        
        // Leader sửa văn bản của phòng mình
        if (_currentUser.Role == UserRole.Leader)
            return doc.Department == _currentUser.Department;
        
        // Staff chỉ sửa văn bản nháp của mình
        if (_currentUser.Role == UserRole.Staff)
            return doc.CreatedBy == _currentUser.Id && 
                   doc.Status == DocumentStatus.Draft;
        
        return false;
    }
    
    public bool CanDelete(Document doc)
    {
        // Chỉ Admin và Leader được xóa
        if (_currentUser.Role == UserRole.Admin) return true;
        
        if (_currentUser.Role == UserRole.Leader)
            return doc.Department == _currentUser.Department &&
                   doc.Status == DocumentStatus.Draft;
        
        return false;
    }
}
```

---

### 3. **WORKFLOW VĂN BẢN ĐI** 🌟 ƯU TIÊN CAO

**Quy trình phát hành văn bản chính thức:**

```
1. NHÁP (Draft)
   ↓ [Cán bộ soạn thảo]
   
2. TRÌNH KÝ (Pending Approval)
   ↓ [Gửi lên Trưởng phòng]
   
3. PHÊ DUYỆT (Approved)
   ↓ [Trưởng phòng duyệt]
   ↓ [Gửi lên Chủ tịch/Phó Chủ tịch]
   
4. ĐÃ KÝ (Signed)
   ↓ [Lãnh đạo ký]
   
5. PHÁT HÀNH (Published)
   ↓ [Cấp số văn bản, ban hành]
   
6. ĐÃ GỬI (Sent)
```

**Database Model:**

```csharp
public enum DocumentStatus
{
    Draft,              // Nháp - đang soạn
    PendingApproval,    // Trình ký - chờ duyệt
    Approved,           // Đã duyệt - chờ ký
    Signed,             // Đã ký - chờ phát hành
    Published,          // Đã phát hành - có số VB
    Sent,               // Đã gửi đi
    Archived            // Đã lưu trữ
}

public class Document
{
    // ... existing fields ...
    
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public string CreatedBy { get; set; }        // User ID người tạo
    public string ApprovedBy { get; set; }       // User ID người duyệt
    public DateTime? ApprovedDate { get; set; }
    public string SignedBy { get; set; }         // User ID người ký
    public DateTime? SignedDate { get; set; }
    public string PublishedBy { get; set; }      // User ID người phát hành
    public DateTime? PublishedDate { get; set; }
    
    public Department Department { get; set; }
    public bool IsPublic { get; set; } = false;
    
    // Workflow comments
    public List<WorkflowComment> Comments { get; set; } = new();
}

public class WorkflowComment
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public DateTime Timestamp { get; set; }
    public string Action { get; set; }  // "Trình ký", "Phê duyệt", "Từ chối", "Ký"
    public string Comment { get; set; }
}
```

**UI: Status badges trong DataGrid:**

```xml
<DataGridTemplateColumn Header="Trạng thái" Width="150">
    <DataGridTemplateColumn.CellTemplate>
        <DataTemplate>
            <!-- Draft -->
            <Border Background="#FFF3E5" CornerRadius="3" Padding="8,4"
                    Visibility="{Binding Status, Converter={StaticResource StatusToVisibility}, ConverterParameter=Draft}">
                <TextBlock Text="📝 Nháp" Foreground="#F57C00"/>
            </Border>
            
            <!-- Pending Approval -->
            <Border Background="#E3F2FD" CornerRadius="3" Padding="8,4"
                    Visibility="{Binding Status, Converter={StaticResource StatusToVisibility}, ConverterParameter=PendingApproval}">
                <TextBlock Text="⏳ Trình ký" Foreground="#1976D2"/>
            </Border>
            
            <!-- Approved -->
            <Border Background="#E8F5E9" CornerRadius="3" Padding="8,4"
                    Visibility="{Binding Status, Converter={StaticResource StatusToVisibility}, ConverterParameter=Approved}">
                <TextBlock Text="✅ Đã duyệt" Foreground="#388E3C"/>
            </Border>
            
            <!-- Signed -->
            <Border Background="#F3E5F5" CornerRadius="3" Padding="8,4"
                    Visibility="{Binding Status, Converter={StaticResource StatusToVisibility}, ConverterParameter=Signed}">
                <TextBlock Text="🖊️ Đã ký" Foreground="#7B1FA2"/>
            </Border>
            
            <!-- Published -->
            <Border Background="#E8EAF6" CornerRadius="3" Padding="8,4"
                    Visibility="{Binding Status, Converter={StaticResource StatusToVisibility}, ConverterParameter=Published}">
                <TextBlock Text="📢 Đã phát hành" Foreground="#303F9F"/>
            </Border>
        </DataTemplate>
    </DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>
```

**Action buttons theo status:**

```csharp
private void UpdateActionButtons(Document doc)
{
    // Ẩn tất cả buttons trước
    btnEdit.Visibility = Visibility.Collapsed;
    btnDelete.Visibility = Visibility.Collapsed;
    btnSubmitApproval.Visibility = Visibility.Collapsed;
    btnApprove.Visibility = Visibility.Collapsed;
    btnReject.Visibility = Visibility.Collapsed;
    btnSign.Visibility = Visibility.Collapsed;
    btnPublish.Visibility = Visibility.Collapsed;
    
    switch (doc.Status)
    {
        case DocumentStatus.Draft:
            // Người tạo: Sửa, Xóa, Trình ký
            if (doc.CreatedBy == _currentUser.Id)
            {
                btnEdit.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Visible;
                btnSubmitApproval.Visibility = Visibility.Visible;
            }
            break;
            
        case DocumentStatus.PendingApproval:
            // Trưởng phòng: Duyệt, Từ chối
            if (_currentUser.Role == UserRole.Leader || 
                _currentUser.Role == UserRole.Admin)
            {
                btnApprove.Visibility = Visibility.Visible;
                btnReject.Visibility = Visibility.Visible;
            }
            break;
            
        case DocumentStatus.Approved:
            // Lãnh đạo: Ký, Từ chối
            if (_currentUser.Role == UserRole.Admin)
            {
                btnSign.Visibility = Visibility.Visible;
                btnReject.Visibility = Visibility.Visible;
            }
            break;
            
        case DocumentStatus.Signed:
            // Văn thư: Phát hành (cấp số)
            if (_currentUser.Department == Department.VP_TH)
            {
                btnPublish.Visibility = Visibility.Visible;
            }
            break;
            
        case DocumentStatus.Published:
            // Chỉ xem, không sửa xóa
            break;
    }
}
```

---

### 4. **SỔ VĂN BẢN ĐI/ĐẾN** 📋 ƯU TIÊN CAO

**Tính năng sổ văn bản giống sổ tay truyền thống:**

```xml
<TabControl>
    <TabItem Header="📤 Sổ văn bản đi">
        <DataGrid ItemsSource="{Binding OutgoingDocuments}">
            <DataGrid.Columns>
                <DataGridTextColumn Header="STT" Binding="{Binding Index}"/>
                <DataGridTextColumn Header="Số ký hiệu" Binding="{Binding Number}"/>
                <DataGridTextColumn Header="Ngày tháng" Binding="{Binding IssueDate, StringFormat='dd/MM/yyyy'}"/>
                <DataGridTextColumn Header="Trích yếu" Binding="{Binding Subject}"/>
                <DataGridTextColumn Header="Nơi gửi" Binding="{Binding Recipient}"/>
                <DataGridTextColumn Header="Người ký" Binding="{Binding SignedBy}"/>
                <DataGridTextColumn Header="Ghi chú" Binding="{Binding Notes}"/>
            </DataGrid.Columns>
        </DataGrid>
    </TabItem>
    
    <TabItem Header="📥 Sổ văn bản đến">
        <DataGrid ItemsSource="{Binding IncomingDocuments}">
            <DataGrid.Columns>
                <DataGridTextColumn Header="STT" Binding="{Binding Index}"/>
                <DataGridTextColumn Header="Số ký hiệu" Binding="{Binding Number}"/>
                <DataGridTextColumn Header="Ngày đến" Binding="{Binding ReceivedDate, StringFormat='dd/MM/yyyy'}"/>
                <DataGridTextColumn Header="Nơi gửi" Binding="{Binding Issuer}"/>
                <DataGridTextColumn Header="Trích yếu" Binding="{Binding Subject}"/>
                <DataGridTextColumn Header="Ngày VB" Binding="{Binding IssueDate, StringFormat='dd/MM/yyyy'}"/>
                <DataGridTextColumn Header="Người nhận" Binding="{Binding ReceivedBy}"/>
                <DataGridTextColumn Header="Chuyển đến" Binding="{Binding ForwardedTo}"/>
                <DataGridTextColumn Header="Ghi chú" Binding="{Binding Notes}"/>
            </DataGrid.Columns>
        </DataGrid>
    </TabItem>
</TabControl>
```

**Export to Excel:**

```csharp
public void ExportToExcel(List<Document> documents, DocumentDirection direction)
{
    var workbook = new XLWorkbook();
    var worksheet = workbook.Worksheets.Add(
        direction == DocumentDirection.Outgoing ? "Sổ văn bản đi" : "Sổ văn bản đến"
    );
    
    // Header
    worksheet.Cell(1, 1).Value = "STT";
    worksheet.Cell(1, 2).Value = "Số ký hiệu";
    worksheet.Cell(1, 3).Value = "Ngày tháng";
    worksheet.Cell(1, 4).Value = "Trích yếu";
    // ... more columns
    
    // Data
    int row = 2;
    foreach (var doc in documents)
    {
        worksheet.Cell(row, 1).Value = row - 1;
        worksheet.Cell(row, 2).Value = doc.Number;
        worksheet.Cell(row, 3).Value = doc.IssueDate.ToString("dd/MM/yyyy");
        worksheet.Cell(row, 4).Value = doc.Subject;
        // ... more data
        row++;
    }
    
    workbook.SaveAs($"So_van_ban_{direction}_{DateTime.Now:yyyyMMdd}.xlsx");
}
```

---

### 5. **TÌM KIẾM NÂNG CAO** 🔍

**Multi-field search theo nghiệp vụ:**

```xml
<Expander Header="🔍 TÌM KIẾM NÂNG CAO" IsExpanded="False" Margin="0,0,0,15">
    <materialDesign:Card Padding="15">
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>
            
            <!-- Row 1 -->
            <TextBox Grid.Row="0" Grid.Column="0" Margin="0,0,10,10"
                     x:Name="txtSearchNumber"
                     materialDesign:HintAssist.Hint="📄 Số văn bản"/>
            <TextBox Grid.Row="0" Grid.Column="1" Margin="0,0,0,10"
                     x:Name="txtSearchTitle"
                     materialDesign:HintAssist.Hint="📝 Tiêu đề"/>
            
            <!-- Row 2 -->
            <TextBox Grid.Row="1" Grid.Column="0" Margin="0,0,10,10"
                     x:Name="txtSearchIssuer"
                     materialDesign:HintAssist.Hint="🏢 Cơ quan ban hành"/>
            <ComboBox Grid.Row="1" Grid.Column="1" Margin="0,0,0,10"
                      x:Name="cboSearchType"
                      materialDesign:HintAssist.Hint="📑 Loại văn bản"/>
            
            <!-- Row 3 -->
            <DatePicker Grid.Row="2" Grid.Column="0" Margin="0,0,10,10"
                        x:Name="dpFromDate"
                        materialDesign:HintAssist.Hint="📅 Từ ngày"/>
            <DatePicker Grid.Row="2" Grid.Column="1" Margin="0,0,0,10"
                        x:Name="dpToDate"
                        materialDesign:HintAssist.Hint="📅 Đến ngày"/>
            
            <!-- Row 4 -->
            <ComboBox Grid.Row="3" Grid.Column="0" Margin="0,0,10,10"
                      x:Name="cboSearchDepartment"
                      materialDesign:HintAssist.Hint="🏛️ Phòng ban"/>
            <ComboBox Grid.Row="3" Grid.Column="1" Margin="0,0,0,10"
                      x:Name="cboSearchStatus"
                      materialDesign:HintAssist.Hint="📊 Trạng thái"/>
            
            <!-- Row 5: Tags -->
            <TextBox Grid.Row="4" Grid.ColumnSpan="2" Margin="0,0,0,10"
                     x:Name="txtSearchTags"
                     materialDesign:HintAssist.Hint="🏷️ Tags (cách nhau bởi dấu phẩy)"/>
            
            <!-- Row 6: Buttons -->
            <StackPanel Grid.Row="5" Grid.ColumnSpan="2" Orientation="Horizontal" HorizontalAlignment="Right">
                <Button Content="🔍 Tìm kiếm" 
                        Style="{StaticResource MaterialDesignRaisedButton}"
                        Click="AdvancedSearch_Click"
                        Margin="0,0,10,0"/>
                <Button Content="🔄 Reset" 
                        Style="{StaticResource MaterialDesignOutlinedButton}"
                        Click="ResetSearch_Click"/>
            </StackPanel>
        </Grid>
    </materialDesign:Card>
</Expander>
```

```csharp
private void AdvancedSearch_Click(object sender, RoutedEventArgs e)
{
    var query = _allDocuments.AsQueryable();
    
    // Số văn bản
    if (!string.IsNullOrWhiteSpace(txtSearchNumber.Text))
        query = query.Where(d => d.Number.Contains(txtSearchNumber.Text, StringComparison.OrdinalIgnoreCase));
    
    // Tiêu đề
    if (!string.IsNullOrWhiteSpace(txtSearchTitle.Text))
        query = query.Where(d => d.Title.Contains(txtSearchTitle.Text, StringComparison.OrdinalIgnoreCase));
    
    // Cơ quan
    if (!string.IsNullOrWhiteSpace(txtSearchIssuer.Text))
        query = query.Where(d => d.Issuer.Contains(txtSearchIssuer.Text, StringComparison.OrdinalIgnoreCase));
    
    // Loại văn bản
    if (cboSearchType.SelectedIndex > 0)
        query = query.Where(d => d.Type == (DocumentType)cboSearchType.SelectedItem);
    
    // Khoảng thời gian
    if (dpFromDate.SelectedDate.HasValue)
        query = query.Where(d => d.IssueDate >= dpFromDate.SelectedDate.Value);
    if (dpToDate.SelectedDate.HasValue)
        query = query.Where(d => d.IssueDate <= dpToDate.SelectedDate.Value);
    
    // Phòng ban
    if (cboSearchDepartment.SelectedIndex > 0)
        query = query.Where(d => d.Department == (Department)cboSearchDepartment.SelectedItem);
    
    // Trạng thái
    if (cboSearchStatus.SelectedIndex > 0)
        query = query.Where(d => d.Status == (DocumentStatus)cboSearchStatus.SelectedItem);
    
    // Tags
    if (!string.IsNullOrWhiteSpace(txtSearchTags.Text))
    {
        var tags = txtSearchTags.Text.Split(',').Select(t => t.Trim()).ToList();
        query = query.Where(d => d.Tags.Any(t => tags.Contains(t)));
    }
    
    dgDocuments.ItemsSource = query.OrderByDescending(d => d.IssueDate).ToList();
}
```

---

### 6. **BATCH OPERATIONS - Thao tác hàng loạt** ⚡

**UI với selection mode:**

```xml
<!-- Batch Actions Bar -->
- Chọn nhiều văn bản cùng lúc (Ctrl+Click, Shift+Click)
- Di chuyển hàng loạt sang thư mục khác
- Xóa hàng loạt
- Gắn tag hàng loạt
- Export hàng loạt

**Giao diện đề xuất:**

```xml
<!-- Batch Actions Toolbar (ẩn/hiện khi chọn nhiều) -->
<materialDesign:Card x:Name="batchActionsBar" 
                     Visibility="Collapsed"
                     Background="#FFF3E5"
                     Padding="10" Margin="0,0,0,10">
    <StackPanel Orientation="Horizontal">
        <TextBlock x:Name="txtSelectedCount" 
                   Text="Đã chọn: 0 văn bản" 
                   VerticalAlignment="Center" 
                   FontWeight="Bold" 
                   Margin="0,0,20,0"/>
        
        <Button Content="📁 Di chuyển" Click="BatchMove_Click"/>
        <Button Content="🏷️ Gắn tag" Click="BatchTag_Click"/>
        <Button Content="📥 Export" Click="BatchExport_Click"/>
        <Button Content="🗑️ Xóa" Click="BatchDelete_Click" Foreground="Red"/>
        
        <Button Content="✖ Bỏ chọn" 
                Click="BatchClear_Click" 
                Margin="20,0,0,0"/>
    </StackPanel>
</materialDesign:Card>
```

---

### 3. **Smart Tags - Tag thông minh** 🎯

**Tính năng:**
- Gắn nhiều tag cho mỗi văn bản (giống Photo Album)
- Tìm kiếm theo tag
- Tag suggestions khi nhập
- Tag colors để phân loại

**Ví dụ tags:**
- `#cấp-bách` (đỏ)
- `#mật` (vàng)
- `#quan-trọng` (cam)
- `#nội-bộ` (xanh)
- `#đã-xử-lý` (xám)

**Database Model:**

```csharp
public class DocumentTag
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
    public int UseCount { get; set; } // Số lần sử dụng
}

public class Document
{
    // ... existing fields ...
    public List<string> Tags { get; set; } = new();
}
```

---

### 4. **Advanced Search - Tìm kiếm nâng cao** 🔍

**Giao diện đề xuất:**

```xml
<Expander Header="🔍 Tìm kiếm nâng cao" Margin="0,0,0,10">
    <StackPanel Margin="10">
        <!-- Tìm theo số văn bản -->
        <TextBox materialDesign:HintAssist.Hint="Số văn bản"/>
        
        <!-- Tìm theo tiêu đề -->
        <TextBox materialDesign:HintAssist.Hint="Tiêu đề"/>
        
        <!-- Tìm theo cơ quan -->
        <TextBox materialDesign:HintAssist.Hint="Cơ quan ban hành"/>
        
        <!-- Khoảng thời gian -->
        <DatePicker materialDesign:HintAssist.Hint="Từ ngày"/>
        <DatePicker materialDesign:HintAssist.Hint="Đến ngày"/>
        
        <!-- Tags -->
        <ComboBox materialDesign:HintAssist.Hint="Tags" 
                  IsEditable="True" 
                  SelectionMode="Multiple"/>
        
        <!-- Buttons -->
        <StackPanel Orientation="Horizontal" Margin="0,10,0,0">
            <Button Content="🔍 Tìm kiếm"/>
            <Button Content="🔄 Reset"/>
        </StackPanel>
    </StackPanel>
</Expander>
```

---

### 5. **Document Templates - Mẫu văn bản** 📋

**Đã có Model nhưng chưa integrate vào UI**

**Tính năng cần thêm:**
- Tạo văn bản từ template nhanh
- Quản lý templates (thêm/sửa/xóa)
- Templates có sẵn cho các loại văn bản phổ biến
- Preview template trước khi tạo

**UI đề xuất:**

```xml
<!-- Thêm nút "Tạo từ mẫu" vào toolbar -->
<Button Style="{StaticResource MaterialDesignRaisedButton}"
        Click="CreateFromTemplate_Click">
    <StackPanel Orientation="Horizontal">
        <materialDesign:PackIcon Kind="FileDocument" Margin="0,0,5,0"/>
        <TextBlock Text="Tạo từ mẫu"/>
    </StackPanel>
</Button>
```

---

### 6. **Quick Stats - Thống kê nhanh** 📊

**Dashboard mini phía trên DataGrid:**

```xml
<StackPanel Orientation="Horizontal" Margin="0,0,0,10">
    <!-- Tổng số văn bản -->
    <materialDesign:Card Padding="15" Margin="0,0,10,0">
        <StackPanel>
            <TextBlock Text="Tổng số" FontSize="11" Foreground="Gray"/>
            <TextBlock Text="248" FontSize="24" FontWeight="Bold"/>
        </StackPanel>
    </materialDesign:Card>
    
    <!-- Tháng này -->
    <materialDesign:Card Padding="15" Margin="0,0,10,0">
        <StackPanel>
            <TextBlock Text="Tháng này" FontSize="11" Foreground="Gray"/>
            <TextBlock Text="12" FontSize="24" FontWeight="Bold" Foreground="Green"/>
        </StackPanel>
    </materialDesign:Card>
    
    <!-- Chưa xử lý -->
    <materialDesign:Card Padding="15" Margin="0,0,10,0">
        <StackPanel>
            <TextBlock Text="Chưa xử lý" FontSize="11" Foreground="Gray"/>
            <TextBlock Text="5" FontSize="24" FontWeight="Bold" Foreground="Orange"/>
        </StackPanel>
    </materialDesign:Card>
    
    <!-- Quá hạn -->
    <materialDesign:Card Padding="15">
        <StackPanel>
            <TextBlock Text="Quá hạn" FontSize="11" Foreground="Gray"/>
            <TextBlock Text="2" FontSize="24" FontWeight="Bold" Foreground="Red"/>
        </StackPanel>
    </materialDesign:Card>
</StackPanel>
```

---

### 7. **Document Attachments - File đính kèm** 📎

**Model cần update:**

```csharp
public class Document
{
    // ... existing fields ...
    
    public List<DocumentAttachment> Attachments { get; set; } = new();
}

public class DocumentAttachment
{
    public string Id { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; }
}
```

**UI trong DocumentEditDialog:**

```xml
<!-- Attachments Section -->
<TextBlock Text="📎 File đính kèm" FontWeight="Bold" Margin="0,10,0,5"/>
<ListView x:Name="lvAttachments" Height="150">
    <ListView.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal">
                <materialDesign:PackIcon Kind="FileDocument" VerticalAlignment="Center"/>
                <TextBlock Text="{Binding FileName}" Margin="10,0"/>
                <TextBlock Text="{Binding FileSize, StringFormat='{0:N0} KB'}" 
                          Foreground="Gray" Margin="10,0"/>
                <Button ToolTip="Xóa" Click="RemoveAttachment_Click">
                    <materialDesign:PackIcon Kind="Delete" Foreground="Red"/>
                </Button>
            </StackPanel>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>

<Button Content="➕ Thêm file" Click="AddAttachment_Click"/>
```

---

### 8. **Document History - Lịch sử thay đổi** 📜

**Tracking mọi thay đổi:**

```csharp
public class DocumentHistory
{
    public string Id { get; set; }
    public string DocumentId { get; set; }
    public string Action { get; set; } // Created, Updated, Moved, Tagged
    public string User { get; set; }
    public DateTime Timestamp { get; set; }
    public string Details { get; set; } // JSON of changes
}
```

**UI trong DocumentViewDialog:**

```xml
<TabControl>
    <TabItem Header="Chi tiết">
        <!-- Document details -->
    </TabItem>
    
    <TabItem Header="Lịch sử">
        <ListView ItemsSource="{Binding History}">
            <ListView.ItemTemplate>
                <DataTemplate>
                    <StackPanel Margin="0,5">
                        <TextBlock FontWeight="Bold">
                            <Run Text="{Binding User}"/>
                            <Run Text="{Binding Action}"/>
                        </TextBlock>
                        <TextBlock Text="{Binding Timestamp, StringFormat='dd/MM/yyyy HH:mm'}" 
                                  FontSize="11" Foreground="Gray"/>
                        <TextBlock Text="{Binding Details}" 
                                  Margin="10,0,0,0"/>
                    </StackPanel>
                </DataTemplate>
            </ListView.ItemTemplate>
        </ListView>
    </TabItem>
</TabControl>
```

---

### 9. **Export/Import - Xuất nhập dữ liệu** 💾

**Export:**
- Export to Excel (toàn bộ hoặc filtered)
- Export to PDF (báo cáo danh sách)
- Export metadata (JSON/XML)

**Import:**
- Import từ Excel (bulk create)
- Import từ file system (scan folder)
- Import từ email (drag PDF từ Outlook)

---

### 10. **Recent Documents - Văn bản gần đây** 🕒

**Quick access panel:**

```xml
<Expander Header="⏰ Gần đây" IsExpanded="True">
    <ListView x:Name="lvRecent" Height="200">
        <ListView.ItemTemplate>
            <DataTemplate>
                <StackPanel Orientation="Horizontal">
                    <TextBlock Text="{Binding Title}" FontWeight="Bold"/>
                    <TextBlock Text="{Binding Number}" 
                              Foreground="Gray" 
                              Margin="10,0"/>
                </StackPanel>
            </DataTemplate>
        </ListView.ItemTemplate>
    </ListView>
</Expander>
```

---

## 🎯 ROADMAP IMPLEMENTATION - KẾ HOẠCH TRIỂN KHAI

### 📅 PHASE 1: CORE FOUNDATION (Tuần 1-2) - CỐT LÕI

**Mục tiêu:** Tạo nền tảng vững chắc với cấu trúc thư mục chuẩn

#### Task 1.1: Organization Setup Wizard
- [ ] Tạo `OrganizationSetupDialog.xaml` với wizard 3 bước
- [ ] Implement `OrganizationSetupService` với method `CreateFolderStructure()`
- [ ] Tự động tạo 11 folder chính + sub-folders
- [ ] Lưu Organization info vào database

#### Task 1.2: Folder Management
- [ ] Update `Folder` model với `OrganizationName`, `Icon`, `Color`, `SortOrder`
- [ ] Implement `LoadFolders()` với hierarchical structure
- [ ] Right-click menu trên TreeView: Tạo mới, Đổi tên, Xóa, Thuộc tính
- [ ] Drag-drop văn bản vào folder

#### Task 1.3: Database Schema Updates
```sql
-- Thêm vào Document table
ALTER TABLE Document ADD COLUMN Status TEXT DEFAULT 'Draft';
ALTER TABLE Document ADD COLUMN CreatedBy TEXT;
ALTER TABLE Document ADD COLUMN ApprovedBy TEXT;
ALTER TABLE Document ADD COLUMN ApprovedDate TEXT;
ALTER TABLE Document ADD COLUMN SignedBy TEXT;
ALTER TABLE Document ADD COLUMN SignedDate TEXT;
ALTER TABLE Document ADD COLUMN PublishedBy TEXT;
ALTER TABLE Document ADD COLUMN PublishedDate TEXT;
ALTER TABLE Document ADD COLUMN Department TEXT;
ALTER TABLE Document ADD COLUMN IsPublic INTEGER DEFAULT 0;
ALTER TABLE Document ADD COLUMN Tags TEXT; -- JSON array

-- Thêm vào Folder table
ALTER TABLE Folder ADD COLUMN OrganizationName TEXT;
ALTER TABLE Folder ADD COLUMN Icon TEXT DEFAULT '📂';
ALTER TABLE Folder ADD COLUMN Color TEXT DEFAULT '#1976D2';
ALTER TABLE Folder ADD COLUMN SortOrder INTEGER DEFAULT 0;

-- Tạo WorkflowComment table
CREATE TABLE WorkflowComment (
    Id TEXT PRIMARY KEY,
    DocumentId TEXT NOT NULL,
    UserId TEXT NOT NULL,
    UserName TEXT NOT NULL,
    Timestamp TEXT NOT NULL,
    Action TEXT NOT NULL,
    Comment TEXT,
    FOREIGN KEY(DocumentId) REFERENCES Document(Id)
);

-- Tạo User table (nếu chưa có)
CREATE TABLE User (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Email TEXT,
    Department TEXT NOT NULL,
    Role TEXT NOT NULL,
    CreatedDate TEXT NOT NULL
);
```

**Deliverables:**
- ✅ Cấu trúc thư mục chuẩn tự động
- ✅ TreeView hoàn chỉnh với icons, colors
- ✅ Database schema đầy đủ
- ✅ Basic folder operations

---

### 📅 PHASE 2: WORKFLOW & PERMISSIONS (Tuần 3-4) - PHÂN QUYỀN

**Mục tiêu:** Workflow văn bản đi + Phân quyền phòng ban

#### Task 2.1: User & Permission System
- [ ] Tạo `UserManagementDialog.xaml` - Quản lý user
- [ ] Implement `PermissionService` với methods: `CanView()`, `CanEdit()`, `CanDelete()`
- [ ] Login dialog (simple - chỉ chọn user từ list)
- [ ] Current user indicator trên UI

#### Task 2.2: Document Workflow
- [ ] Update `DocumentEditDialog` với status dropdown
- [ ] Action buttons theo status: Trình ký, Phê duyệt, Từ chối, Ký, Phát hành
- [ ] `WorkflowHistoryDialog` - Hiển thị lịch sử workflow
- [ ] Notification khi văn bản chuyển status (optional)

#### Task 2.3: Department Filter
- [ ] Thêm Department ComboBox vào toolbar
- [ ] Filter documents theo department của current user
- [ ] Color-code văn bản theo department
- [ ] Department badge trong DataGrid

**Deliverables:**
- ✅ Workflow văn bản đi hoàn chỉnh (7 status)
- ✅ Phân quyền theo User Role
- ✅ Department management
- ✅ Workflow history tracking

---

### 📅 PHASE 3: ADVANCED FEATURES (Tuần 5-6) - TÍNH NĂNG NÂNG CAO

**Mục tiêu:** Tìm kiếm nâng cao + Batch operations + Sổ văn bản

#### Task 3.1: Advanced Search
- [ ] Tạo `AdvancedSearchPanel` với Expander
- [ ] Multi-field search: Number, Title, Issuer, Type, Date range, Department, Status, Tags
- [ ] Saved searches (optional)
- [ ] Export search results to Excel

#### Task 3.2: Batch Operations
- [ ] DataGrid SelectionMode = Extended
- [ ] Batch actions bar: Move, Tag, Delete, Export
- [ ] Selected count indicator
- [ ] Confirm dialog trước khi batch delete

#### Task 3.3: Document Registry (Sổ văn bản)
- [ ] Tạo `DocumentRegistryPage.xaml` với TabControl
- [ ] Tab "Sổ văn bản đi" - Outgoing documents
- [ ] Tab "Sổ văn bản đến" - Incoming documents
- [ ] Export to Excel với format chuẩn
- [ ] Print preview (optional)

**Deliverables:**
- ✅ Advanced search với 8+ fields
- ✅ Batch operations (move, tag, delete)
- ✅ Sổ văn bản đi/đến
- ✅ Excel export

---

### 📅 PHASE 4: TEMPLATES & INTEGRATION (Tuần 7-8) - TÍCH HỢP

**Mục tiêu:** Mẫu văn bản + Tích hợp với Album ảnh

#### Task 4.1: Document Templates
- [ ] Tạo `TemplateManagementPage.xaml`
- [ ] Upload .docx templates
- [ ] Create document from template (open in Word)
- [ ] Template variables: `{{SO_VAN_BAN}}`, `{{NGAY_THANG}}`, etc.
- [ ] Pre-populated templates cho các loại văn bản phổ biến

#### Task 4.2: Album Ảnh Integration
- [ ] Link Album ảnh vào folder "06. ALBUM ẢNH"
- [ ] Thêm Album tab trong `DocumentViewDialog`
- [ ] Attach photos to documents
- [ ] Photo gallery trong document view

#### Task 4.3: Attachments
- [ ] Upload files đính kèm (PDF, Word, Excel, Image)
- [ ] File preview trong dialog
- [ ] Download attachments
- [ ] File size limit & validation

**Deliverables:**
- ✅ Template system hoàn chỉnh
- ✅ Tích hợp Album ảnh
- ✅ File attachments
- ✅ Template variables

---

### 📅 PHASE 5: REPORTING & POLISH (Tuần 9-10) - BÁO CÁO & HOÀN THIỆN

**Mục tiêu:** Dashboard + Reports + UI/UX polish

#### Task 5.1: Dashboard & Statistics
- [ ] Quick stats cards: Tổng số, Tháng này, Chưa xử lý, Quá hạn
- [ ] Chart: Văn bản theo tháng (Line chart)
- [ ] Chart: Văn bản theo loại (Pie chart)
- [ ] Chart: Văn bản theo phòng ban (Bar chart)

#### Task 5.2: Reports
- [ ] Báo cáo định kỳ: Tuần, Tháng, Quý, Năm
- [ ] Báo cáo theo lĩnh vực
- [ ] Báo cáo workflow (bao nhiêu văn bản pending)
- [ ] Export reports to PDF/Excel

#### Task 5.3: UI/UX Polish
- [ ] Animations (fade in/out, slide)
- [ ] Loading indicators
- [ ] Empty states với friendly messages
- [ ] Keyboard shortcuts (Ctrl+F = search, Ctrl+N = new doc)
- [ ] Tooltips với hướng dẫn
- [ ] Theme switcher (Light/Dark mode - optional)

#### Task 5.4: Testing & Bug Fixes
- [ ] Test all workflows
- [ ] Test permissions với different users
- [ ] Test large datasets (1000+ documents)
- [ ] Performance optimization
- [ ] Bug fixes from user feedback

**Deliverables:**
- ✅ Dashboard với charts
- ✅ Reports module
- ✅ Polished UI/UX
- ✅ Production-ready app

---

## 🛠️ TECHNICAL STACK & TOOLS

### Core Technologies
- **Framework:** .NET 9.0, WPF
- **UI:** Material Design 5.3.0
- **Database:** LiteDB 5.0.21 (embedded)
- **Charts:** LiveCharts2 hoặc OxyPlot
- **Excel:** ClosedXML
- **PDF:** iTextSharp hoặc QuestPDF
- **Word:** DocX (Xceed)

### Recommended NuGet Packages
```xml
<PackageReference Include="MaterialDesignThemes" Version="5.3.0" />
<PackageReference Include="MaterialDesignColors" Version="3.3.0" />
<PackageReference Include="LiteDB" Version="5.0.21" />
<PackageReference Include="ClosedXML" Version="0.104.1" />
<PackageReference Include="LiveChartsCore.SkiaSharpView.WPF" Version="2.0.0-rc3.3" />
<PackageReference Include="Xceed.Words.NET" Version="1.11.0" />
<PackageReference Include="QuestPDF" Version="2024.12.3" />
```

---

## 📝 CODE SAMPLES - MẪU CODE QUAN TRỌNG

### 1. Auto-Create Folder Structure

```csharp
public class OrganizationSetupService
{
    private readonly DocumentService _documentService;
    
    public OrganizationSetupService(DocumentService documentService)
    {
        _documentService = documentService;
    }
    
    public void CreateDefaultStructure(string orgName, OrganizationType orgType)
    {
        Console.WriteLine($"Creating folder structure for: {orgName} ({orgType})");
        
        // 1. Văn bản pháp luật
        var plFolder = CreateFolder("01. VĂN BẢN PHÁP LUẬT", null, "⚖️");
        CreateSubFolders(plFolder.Id, new[]
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
        
        // 2. Văn bản đi
        var vbDiFolder = CreateFolder("02. VĂN BẢN ĐI", null, "📤");
        CreateYearFolders(vbDiFolder.Id, new[]
        {
            "Công văn đi",
            "Quyết định",
            "Thông báo",
            "Báo cáo (gửi cấp trên)",
            "Tờ trình",
            "Kế hoạch"
        });
        
        // 3. Văn bản đến
        var vbDenFolder = CreateFolder("03. VĂN BẢN ĐẾN", null, "📥");
        CreateYearFolders(vbDenFolder.Id, new[]
        {
            "Từ Trung ương (Chính phủ, Bộ)",
            "Từ cấp Tỉnh (UBND, Sở)",
            "Từ cấp Huyện (UBND, Phòng)",
            "Từ các xã/phường",
            "Từ tổ chức, cá nhân"
        });
        
        // ... Continue for all 11 main folders
        
        Console.WriteLine("✅ Folder structure created successfully!");
    }
    
    private Folder CreateFolder(string name, string? parentId, string icon = "📂")
    {
        var folder = new Folder
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            ParentId = parentId,
            Icon = icon,
            CreatedDate = DateTime.Now
        };
        
        _documentService.CreateFolder(folder);
        return folder;
    }
    
    private void CreateSubFolders(string parentId, (string name, string icon)[] folders)
    {
        foreach (var (name, icon) in folders)
        {
            CreateFolder(name, parentId, icon);
        }
    }
    
    private void CreateYearFolders(string parentId, string[] subFolders)
    {
        for (int year = 2024; year <= DateTime.Now.Year; year++)
        {
            var yearFolder = CreateFolder($"[Năm {year}]", parentId, "📅");
            CreateSubFolders(yearFolder.Id, 
                subFolders.Select(f => (f, "📂")).ToArray());
        }
    }
}
```

### 2. Workflow State Machine

```csharp
public class DocumentWorkflowService
{
    private readonly DocumentService _documentService;
    private readonly User _currentUser;
    
    public async Task<bool> SubmitForApproval(string documentId, string comment)
    {
        var doc = _documentService.GetDocument(documentId);
        if (doc == null) return false;
        
        // Validate transition
        if (doc.Status != DocumentStatus.Draft)
        {
            throw new InvalidOperationException("Chỉ văn bản nháp mới có thể trình ký!");
        }
        
        if (doc.CreatedBy != _currentUser.Id)
        {
            throw new UnauthorizedAccessException("Bạn không phải người tạo văn bản này!");
        }
        
        // Update status
        doc.Status = DocumentStatus.PendingApproval;
        doc.Comments.Add(new WorkflowComment
        {
            Id = Guid.NewGuid().ToString(),
            UserId = _currentUser.Id,
            UserName = _currentUser.Name,
            Timestamp = DateTime.Now,
            Action = "Trình ký",
            Comment = comment
        });
        
        _documentService.UpdateDocument(doc);
        
        // TODO: Send notification to leader
        await NotifyLeader(doc);
        
        return true;
    }
    
    public async Task<bool> Approve(string documentId, string comment)
    {
        var doc = _documentService.GetDocument(documentId);
        if (doc == null) return false;
        
        // Validate permission
        if (_currentUser.Role != UserRole.Leader && _currentUser.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền phê duyệt!");
        }
        
        // Validate transition
        if (doc.Status != DocumentStatus.PendingApproval)
        {
            throw new InvalidOperationException("Văn bản không ở trạng thái chờ duyệt!");
        }
        
        // Update status
        doc.Status = DocumentStatus.Approved;
        doc.ApprovedBy = _currentUser.Id;
        doc.ApprovedDate = DateTime.Now;
        doc.Comments.Add(new WorkflowComment
        {
            Id = Guid.NewGuid().ToString(),
            UserId = _currentUser.Id,
            UserName = _currentUser.Name,
            Timestamp = DateTime.Now,
            Action = "Phê duyệt",
            Comment = comment
        });
        
        _documentService.UpdateDocument(doc);
        
        // Notify admin for signature
        await NotifyAdmin(doc);
        
        return true;
    }
    
    public async Task<bool> Reject(string documentId, string reason)
    {
        var doc = _documentService.GetDocument(documentId);
        if (doc == null) return false;
        
        // Validate permission
        if (_currentUser.Role != UserRole.Leader && _currentUser.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền từ chối!");
        }
        
        // Revert to Draft
        doc.Status = DocumentStatus.Draft;
        doc.Comments.Add(new WorkflowComment
        {
            Id = Guid.NewGuid().ToString(),
            UserId = _currentUser.Id,
            UserName = _currentUser.Name,
            Timestamp = DateTime.Now,
            Action = "Từ chối",
            Comment = reason
        });
        
        _documentService.UpdateDocument(doc);
        
        // Notify creator
        await NotifyCreator(doc, reason);
        
        return true;
    }
    
    // Similar methods for Sign(), Publish(), etc...
}
```

---

## 🎓 HỌC TỪ PHOTO ALBUM

**Patterns đã implement thành công trong PhotoAlbumPageNew:**

### 1. TreeView Management
```csharp
// ✅ Đã có trong PhotoAlbumPageNew - Áp dụng cho DocumentListPage
private void LoadAlbumTree(bool preserveSelection = false)
{
    var currentAlbumId = preserveSelection ? _selectedAlbumNode?.Id : null;
    
    var rootNode = BuildAlbumTree();
    albumTree.Items.Clear();
    albumTree.Items.Add(rootNode);
    
    if (preserveSelection && !string.IsNullOrEmpty(currentAlbumId))
    {
        Dispatcher.BeginInvoke(() =>
        {
            try
            {
                var nodeToSelect = FindNodeById(rootNode, currentAlbumId);
                if (nodeToSelect != null)
                {
                    ExpandToNode(rootNode, currentAlbumId);
                    _selectedAlbumNode = nodeToSelect;
                    UpdateButtonStates();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring selection: {ex.Message}");
            }
        }, DispatcherPriority.Loaded);
    }
}
```

### 2. Actual Count from Database
```csharp
// ✅ Get real-time count - Không dùng cached value
PhotoCount = _documentService.GetPhotosByAlbum(album.Id).Count
```

### 3. ErrorDialog Pattern
```csharp
// ✅ User-friendly error handling
try
{
    // ... operation
}
catch (Exception ex)
{
    ErrorDialog.Show(this, "Lỗi thao tác", ex);
}
```

### 4. Batch Operations UI
```csharp
// ✅ Selection mode toggle
private void ToggleSelectionMode()
{
    _isSelectionMode = !_isSelectionMode;
    batchActionsBar.Visibility = _isSelectionMode ? Visibility.Visible : Visibility.Collapsed;
    btnSelectMode.Content = _isSelectionMode ? "✖ Thoát chế độ chọn" : "☑️ Chọn nhiều";
}
```

---

## ✅ DELIVERABLES - SẢN PHẨM CUỐI CÙNG

### Chức năng hoàn chỉnh:
1. ✅ **11 thư mục chuẩn** - Tự động tạo theo loại cơ quan
2. ✅ **Workflow văn bản đi** - 7 trạng thái từ nháp → phát hành
3. ✅ **Phân quyền phòng ban** - 4 user roles, 7 departments
4. ✅ **Sổ văn bản đi/đến** - Export Excel
5. ✅ **Tìm kiếm nâng cao** - 8+ search fields
6. ✅ **Batch operations** - Move, tag, delete nhiều văn bản
7. ✅ **Templates** - Tạo văn bản từ mẫu
8. ✅ **Album ảnh tích hợp** - Link với PhotoAlbumPageNew
9. ✅ **Dashboard** - Thống kê, biểu đồ
10. ✅ **Reports** - Báo cáo định kỳ

### UI/UX:
- ✅ Material Design consistent
- ✅ Responsive layout
- ✅ Keyboard shortcuts
- ✅ Empty states
- ✅ Loading indicators
- ✅ Error handling với ErrorDialog

### Performance:
- ✅ Handle 1000+ documents
- ✅ Fast search với indexing
- ✅ Lazy loading cho TreeView
- ✅ Optimized database queries

---

**Last Updated:** 2026-02-05  
**Status:** 📋 Document complete - Ready for Phase 1 implementation  
**Next Action:** Bạn có muốn tôi bắt đầu implement Phase 1 không?
