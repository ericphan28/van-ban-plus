# 📦 HƯỚNG DẪN TẠO VÀ CÀI ĐẶT MSIX PACKAGE

## ✅ ĐÃ HOÀN THÀNH

1. ✅ Tạo project `AIVanBan.Package` 
2. ✅ Tạo manifest `Package.appxmanifest`
3. ✅ Tạo tất cả assets (logo/icon các kích thước)
4. ✅ Thêm vào solution AIVanBan.sln
5. ✅ Script tự động build MSIX

---

## 🚀 CÁCH TẠO MSIX PACKAGE

### Bước 1: Build MSIX (chọn 1 trong 2 cách)

#### **Cách A: Dùng PowerShell Script (Khuyên dùng)**

```powershell
cd D:\AIVanBanCaNhan

# Build với cấu hình mặc định (Release, x64)
.\build-msix.ps1

# HOẶC tùy chỉnh
.\build-msix.ps1 -Configuration Release -Platform x64 -Version "1.0.5.0"
```

Script sẽ tự động:
- Restore NuGet packages
- Build solution
- Publish Desktop app
- Tạo certificate (nếu chưa có)
- Build MSIX package
- Hiển thị đường dẫn file .msix

#### **Cách B: Dùng Visual Studio**

1. Mở `AIVanBan.sln` trong Visual Studio 2022
2. Right-click project `AIVanBan.Package`
3. Chọn **Publish** → **Create App Packages**
4. Chọn **Sideloading** (không upload Store)
5. Chọn **Yes, use the current certificate** (hoặc tạo mới)
6. Chọn platform: **x64**
7. Click **Create**

Output: `D:\AIVanBanCaNhan\AIVanBan.Package\AppPackages\`

---

## 🔐 CÀI ĐẶT CERTIFICATE (Bắt buộc lần đầu)

File certificate: `AIVanBan.Package\VanBanPlus_TemporaryKey.pfx`
Password: `123456`

### Cách 1: Tự động (PowerShell as Admin)

```powershell
# Chạy PowerShell với quyền Administrator
$certPath = "D:\AIVanBanCaNhan\AIVanBan.Package\VanBanPlus_TemporaryKey.pfx"
$password = ConvertTo-SecureString -String "123456" -Force -AsPlainText

# Import vào Trusted Root
Import-PfxCertificate -FilePath $certPath -CertStoreLocation Cert:\LocalMachine\Root -Password $password

Write-Host "Certificate installed successfully!" -ForegroundColor Green
```

### Cách 2: Thủ công (GUI)

1. Tìm file `.msix` trong `AppPackages\`
2. **Right-click** file .msix → **Properties**
3. Tab **Digital Signatures** → chọn certificate → **Details**
4. Click **View Certificate**
5. Click **Install Certificate...**
6. Chọn **Local Machine** (cần quyền admin)
7. Click **Next**
8. Chọn **Place all certificates in the following store**
9. Click **Browse** → chọn **Trusted Root Certification Authorities**
10. Click **OK** → **Next** → **Finish**
11. Confirm UAC prompt

---

## 📥 CÀI ĐẶT ỨNG DỤNG

### Sau khi đã cài certificate:

1. **Double-click** file `.msix`
2. Click **Install**
3. Chờ vài giây
4. Ứng dụng sẽ xuất hiện trong Start Menu: **VanBanPlus**

### Nếu gặp lỗi "This app package is not trusted":

→ Chứng tỏ certificate chưa được cài đúng. Làm lại bước cài certificate ở trên.

---

## 🧪 TEST VÀ GỠ CÀI ĐẶT

### Kiểm tra app đã cài:

```powershell
# List tất cả MSIX apps
Get-AppxPackage -Name "*VanBan*"
```

### Gỡ cài đặt:

```powershell
# Qua PowerShell
Get-AppxPackage -Name "VanBanPlus" | Remove-AppxPackage

# HOẶC qua Settings
# Settings > Apps > Installed apps > VanBanPlus > Uninstall
```

### Cài đặt lại (khi update version mới):

```powershell
# Gỡ version cũ
Get-AppxPackage -Name "VanBanPlus" | Remove-AppxPackage

# Cài version mới
Add-AppxPackage -Path "D:\AIVanBanCaNhan\AIVanBan.Package\AppPackages\...\VanBanPlus_1.0.0.0_x64.msix"
```

---

## 🛠️ TÙY CHỈNH MANIFEST

File: [AIVanBan.Package\Package.appxmanifest](d:\AIVanBanCaNhan\AIVanBan.Package\Package.appxmanifest)

### Thay đổi thông tin:

```xml
<Identity
  Name="VanBanPlus"
  Publisher="CN=TenCongTyCuaBan"  <!-- ĐỔI TÊN CÔNG TY -->
  Version="1.0.0.0" />            <!-- ĐỔI VERSION -->

<Properties>
  <DisplayName>VanBanPlus</DisplayName>
  <PublisherDisplayName>Tên Công Ty</PublisherDisplayName>  <!-- ĐỔI -->
  <Description>Mô tả phần mềm...</Description>             <!-- ĐỔI -->
</Properties>
```

### Cập nhật version:

```powershell
# Version format: Major.Minor.Build.Revision
# Ví dụ: 1.0.5.0, 2.1.0.0

.\build-msix.ps1 -Version "1.0.5.0"
```

---

## 📊 SO SÁNH MSIX VS EXE INSTALLER

| Tính năng | MSIX | Inno Setup EXE |
|-----------|------|----------------|
| **Cài đặt** | Double-click | Wizard installer |
| **Gỡ cài đặt** | Clean 100% | Có thể để rác |
| **Auto-update** | ✅ Hỗ trợ sẵn | ❌ Phải code thêm |
| **Windows Store** | ✅ Có thể publish | ❌ Không |
| **Yêu cầu** | Win10 1809+ | Win7+ |
| **Signing** | Bắt buộc | Tùy chọn |
| **Sandbox** | ✅ Có | ❌ Không |
| **Complexity** | Trung bình | Dễ |

---

## 🚨 XỬ LÝ LỖI

### Lỗi: "Unable to find a manifest signing certificate"

```powershell
# Tạo lại certificate
$cert = New-SelfSignedCertificate `
    -Type Custom `
    -Subject "CN=VanBanPlus" `
    -KeyUsage DigitalSignature `
    -FriendlyName "VanBanPlus Certificate" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

$password = ConvertTo-SecureString -String "123456" -Force -AsPlainText
Export-PfxCertificate -Cert $cert `
    -FilePath "D:\AIVanBanCaNhan\AIVanBan.Package\VanBanPlus_TemporaryKey.pfx" `
    -Password $password
```

### Lỗi: "DEP0700: Registration of the app failed"

→ App đang chạy, tắt app trước khi cài version mới

```powershell
Get-Process AIVanBan* | Stop-Process -Force
```

### Lỗi: "The package could not be installed because resources it modifies are currently in use"

→ Gỡ version cũ trước:

```powershell
Get-AppxPackage VanBanPlus | Remove-AppxPackage
```

---

## 📝 CHECKLIST TRƯỚC KHI PHÁT HÀNH

- [ ] Đổi `Publisher` trong manifest thành tên công ty thật
- [ ] Đổi `PublisherDisplayName` 
- [ ] Cập nhật `Version` number
- [ ] Cập nhật `Description`
- [ ] Kiểm tra tất cả assets (icon/logo) hiển thị đẹp
- [ ] Test cài đặt trên máy sạch (chưa có .NET)
- [ ] Test gỡ cài đặt sạch sẽ
- [ ] Tạo certificate chính thức (nếu publish công khai)
- [ ] Viết release notes

---

## 🎯 BƯỚC TIẾP THEO

### Nếu muốn phát hành công khai:

1. **Microsoft Store** (Khuyên dùng):
   - Tạo tài khoản [Partner Center](https://partner.microsoft.com/)
   - Submit app qua Visual Studio
   - Review (~2-3 ngày)
   - Publish (miễn phí cài đặt & update tự động)

2. **Website riêng**:
   - Upload file .msix lên web
   - Hướng dẫn user cài certificate + .msix
   - Implement update checker trong app

3. **Inno Setup** (đơn giản hơn):
   - Dùng `setup-script.iss` đã tạo trước
   - Build EXE installer truyền thống
   - Dễ phát hành hơn MSIX

---

**Created:** February 9, 2026  
**For:** VanBanPlus - Vietnamese Government Document Management System
