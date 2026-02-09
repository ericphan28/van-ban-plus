# ⚠️ HƯỚNG DẪN BUILD MSIX ĐƠN GIẢN

## Vấn đề hiện tại

Script tự động `build-msix.ps1` gặp lỗi do cấu trúc project phức tạp. 

**KHUYẾN NGHỊ: Dùng Visual Studio để build MSIX** (đơn giản và ổn định hơn)

---

## 🚀 CÁCH 1: BUILD TRONG VISUAL STUDIO (KHUYÊN DÙNG)

### Bước 1: Mở Visual Studio 2022
```
File > Open > Project/Solution
Chọn: D:\AIVanBanCaNhan\AIVanBan.sln
```

### Bước 2: Thêm Windows Application Packaging Project

1. **Right-click Solution** trong Solution Explorer
2. **Add > New Project**
3. Tìm: **"Windows Application Packaging Project"**
4. Tên: `AIVanBan.Package`
5. Location: `D:\AIVanBanCaNhan\`
6. **Next**
7. Target version: **Windows 10, version 1809 (10.0.17763.0)**
8. Minimum version: **Windows 10, version 1809**
9. **Create**

### Bước 3: Add Application Reference

1. Trong Solution Explorer, mở **AIVanBan.Package**
2. Right-click **Applications** folder
3. **Add Reference**
4. Chọn ✅ **AIVanBan.Desktop**
5. **OK**
6. Right-click **AIVanBan.Desktop** trong Applications folder
7. **Set as Entry Point**

### Bước 4: Copy Assets (Logo/Icon)

Copy tất cả file từ:
```
D:\AIVanBanCaNhan\AIVanBan.Package\Images\
```

Vào:
```
D:\AIVanBanCaNhan\AIVanBan.Package\Images\
```
(Folder tạo bởi Visual Studio)

### Bước 5: Cấu hình Manifest

1. Double-click **Package.appxmanifest** trong Solution Explorer
2. Tab **Application**:
   - Display name: `VanBanPlus`
   - Entry point: `AIVanBan.Desktop.App`
3. Tab **Visual Assets**:
   - Asset Generator > Source: chọn logo 1024x1024
   - Generate
   - HOẶC manually assign từ Images folder
4. Tab **Packaging**:
   - Package name: `VanBanPlus`
   - Publisher: `CN=Your Company`
   - Version: `1.0.5.0`

### Bước 6: Build MSIX

1. **Right-click AIVanBan.Package** project
2. **Publish > Create App Packages**
3. Chọn: **Sideloading** (không Microsoft Store)
4. **Next**
5. Signing method:
   - **Yes, select a certificate** (nếu đã có)
   - **Create...** (nếu tạo mới) → Password: `123456`
6. **Next**
7. Architecture: chỉ chọn ✅ **x64**
8. Version: `1.0.5.0`
9. **Create**

### Bước 7: Lấy file MSIX

Output location:
```
D:\AIVanBanCaNhan\AIVanBan.Package\AppPackages\
└── AIVanBan.Package_1.0.5.0_Test\
    └── AIVanBan.Package_1.0.5.0_x64.msix  ← FILE NÀY
```

---

## 🔧 CÁCH 2: MANUAL BUILD VỚI MAKEAPPX

Nếu không có Visual Studio, dùng Windows SDK tools:

### Bước 1: Publish Desktop app
```powershell
cd D:\AIVanBanCaNhan

dotnet publish AIVanBan.Desktop\AIVanBan.Desktop.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -o "publish\VanBanPlus"
```

### Bước 2: Copy Manifest và Assets
```powershell
# Copy manifest
Copy-Item "AIVanBan.Package\Package.appxmanifest" "publish\VanBanPlus\"

# Copy assets
Copy-Item "AIVanBan.Package\Images" "publish\VanBanPlus\Images" -Recurse
```

### Bước 3: Tạo mapping file

Tạo file `mapping.txt`:
```
[Files]
"D:\AIVanBanCaNhan\publish\VanBanPlus\Package.appxmanifest" "Package.appxmanifest"
"D:\AIVanBanCaNhan\publish\VanBanPlus\AIVanBan.Desktop.exe" "AIVanBan.Desktop.exe"
"D:\AIVanBanCaNhan\publish\VanBanPlus\Images" "Images"
; Add all other files...
```

### Bước 4: Build MSIX với makeappx

```powershell
# Find makeappx (in Windows SDK)
$makeappx = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\makeappx.exe"

# Create package
& $makeappx pack /d "publish\VanBanPlus" /p "VanBanPlus_1.0.5.0.msix"

# Sign package
$signtool = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe"
& $signtool sign /fd SHA256 /a /f "AIVanBan.Package\VanBanPlus_TemporaryKey.pfx" /p "123456" "VanBanPlus_1.0.5.0.msix"
```

---

## ⚡ CÁCH 3: DÙNG INNO SETUP (ĐƠN GIẢN HƠN MSIX)

Nếu MSIX quá phức tạp, dùng Inno Setup thay thế:

### Bước 1: Tải Inno Setup
https://jrsoftware.org/isdl.php

### Bước 2: Publish app
```powershell
dotnet publish AIVanBan.Desktop\AIVanBan.Desktop.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true
```

### Bước 3: Build installer
1. Mở Inno Setup Compiler
2. **File > Open**: `D:\AIVanBanCaNhan\setup-script.iss`
3. **Build > Compile**
4. File EXE sẽ ở `D:\AIVanBanCaNhan\Installer\`

**Lợi ích Inno Setup:**
- ✅ Không cần Visual Studio
- ✅ Dễ build
- ✅ Support Windows 7+
- ✅ File EXE truyền thống, dễ phân phối

---

## 📊 SO SÁNH

| | Visual Studio MSIX | Manual MSIX | Inno Setup |
|---|---|---|---|
| **Độ dễ** | ⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ |
| **Yêu cầu** | VS 2022 | Windows SDK | Inno Setup tool |
| **Thời gian** | 5 phút | 15 phút | 3 phút |
| **Kết quả** | .msix | .msix | .exe |

---

## 🎯 KHUYẾN NGHỊ

1. **Nếu có Visual Studio 2022**: Dùng **CÁCH 1** (Visual Studio)
2. **Nếu không có VS**: Dùng **CÁCH 3** (Inno Setup)
3. **Nếu bắt buộc MSIX không có VS**: Dùng **CÁCH 2** (Manual)

---

## 📝 GHI CHÚ

Script `build-msix.ps1` hiện tại có vấn đề với project structure. Cần refactor hoặc dùng Visual Studio manual build.

**Assets đã sẵn sàng:**
- ✅ Logo/icon 7 kích thước: `AIVanBan.Package\Images\`
- ✅ Manifest template: `AIVanBan.Package\Package.appxmanifest`
- ✅ Certificate: `AIVanBan.Package\VanBanPlus_TemporaryKey.pfx`

---

**Updated:** February 9, 2026  
**Status:** ⚠️ Script cần sửa - Khuyên dùng Visual Studio
