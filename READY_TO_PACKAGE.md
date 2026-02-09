# ✅ PUBLISH THÀNH CÔNG - BƯỚC CUỐI: TẠO INSTALLER

## 📦 App đã được publish tại:
```
D:\AIVanBanCaNhan\AIVanBan.Desktop\bin\Release\net9.0-windows\win-x64\publish\
└── AIVanBan.Desktop.exe (file đơn, ~200MB, chứa toàn bộ .NET runtime)
```

## 🎯 BƯỚC TIẾP THEO: TẠO INSTALLER

### ✅ File setup-script.iss đã được cập nhật:
- Tên: **VanBanPlus**
- Version: **1.0.5**
- Output: `VanBanPlus-Setup-1.0.5.exe`

---

## 🚀 CÁCH 1: DÙNG INNO SETUP (KHUYÊN DÙNG)

### Bước 1: Tải và cài Inno Setup
**Link download:** https://jrsoftware.org/isdl.php
- Chọn: **Inno Setup 6.x** (stable version)
- Chạy installer
- Next > Next > Install

### Bước 2: Build Installer

#### Cách A: Dùng GUI (Đơn giản)
1. Mở **Inno Setup Compiler** (trong Start Menu)
2. **File > Open**: chọn `D:\AIVanBanCaNhan\setup-script.iss`
3. **Build > Compile** (hoặc nhấn F9)
4. Chờ ~5-10 giây
5. File installer sẽ xuất hiện: `D:\AIVanBanCaNhan\Installer\VanBanPlus-Setup-1.0.5.exe`

#### Cách B: Dùng Command Line
```powershell
# Sau khi cài Inno Setup, chạy:
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "D:\AIVanBanCaNhan\setup-script.iss"
```

### Bước 3: Test Installer
```powershell
# Chạy file installer để test
Start-Process "D:\AIVanBanCaNhan\Installer\VanBanPlus-Setup-1.0.5.exe"
```

Installer sẽ:
- ✅ Cài app vào `C:\Program Files\VanBanPlus\`
- ✅ Tạo shortcut trên Desktop
- ✅ Tạo Start Menu entry
- ✅ Đăng ký uninstaller trong Settings

---

## 🔧 CÁCH 2: PORTABLE VERSION (Không cần installer)

Nếu không muốn tạo installer, chỉ cần:

```powershell
# 1. Tạo thư mục portable
New-Item -ItemType Directory -Force -Path "D:\AIVanBanCaNhan\VanBanPlus-Portable"

# 2. Copy file exe
Copy-Item "D:\AIVanBanCaNhan\AIVanBan.Desktop\bin\Release\net9.0-windows\win-x64\publish\AIVanBan.Desktop.exe" `
          "D:\AIVanBanCaNhan\VanBanPlus-Portable\VanBanPlus.exe"

# 3. Tạo file README.txt
@"
VanBanPlus - Portable Version
Phần mềm quản lý văn bản thông minh

Cách chạy:
- Double-click VanBanPlus.exe

Yêu cầu:
- Windows 10 version 1809 trở lên
- Không cần cài .NET (đã tích hợp sẵn)

Version: 1.0.5
"@ | Out-File "D:\AIVanBanCaNhan\VanBanPlus-Portable\README.txt"

# 4. Nén thành ZIP để phân phối
Compress-Archive -Path "D:\AIVanBanCaNhan\VanBanPlus-Portable\*" `
                 -DestinationPath "D:\AIVanBanCaNhan\Installer\VanBanPlus-1.0.5-Portable.zip" `
                 -Force

Write-Host "✅ Portable version created: VanBanPlus-1.0.5-Portable.zip" -ForegroundColor Green
```

---

## 📊 SO SÁNH 2 PHƯƠNG PHÁP

| | Installer (EXE) | Portable (ZIP) |
|---|---|---|
| **Kích thước** | ~200MB | ~200MB |
| **Cài đặt** | Wizard + Start Menu | Giải nén + chạy |
| **Gỡ cài đặt** | Settings > Apps | Xóa folder |
| **Shortcuts** | ✅ Tự động tạo | ❌ Phải tạo thủ công |
| **Updates** | ✅ Có thể check | ❌ Manual |
| **Phù hợp** | Người dùng cuối | IT/Power users |

---

## 🎁 KẾT QUẢ CUỐI CÙNG

Sau khi build xong, bạn sẽ có:

### File phân phối:
```
D:\AIVanBanCaNhan\Installer\
├── VanBanPlus-Setup-1.0.5.exe        (~200MB - Installer chính thức)
└── VanBanPlus-1.0.5-Portable.zip     (~200MB - Portable version)
```

### Cách gửi cho người dùng:
1. **Upload lên Google Drive/OneDrive**
2. **Gửi link download**
3. **Hoặc copy vào USB**

### Hướng dẫn người dùng cài:
```
1. Download file VanBanPlus-Setup-1.0.5.exe
2. Double-click để chạy
3. Chọn "Có" khi Windows hỏi UAC
4. Next > Next > Install
5. Chờ cài đặt xong
6. Mở VanBanPlus từ Desktop hoặc Start Menu
```

---

## ⚠️ LƯU Ý

### File exe chưa được signed (ký số):
- Windows Defender có thể cảnh báo "Unknown publisher"
- Người dùng cần click "More info" > "Run anyway"

### Để ký số (signing):
1. Mua code signing certificate (~$100-300/năm)
2. Dùng `signtool.exe` để ký file exe
3. Hoặc tạo self-signed cert (chỉ dùng nội bộ)

Nhưng cho phát hành ban đầu, chưa ký cũng OK!

---

## 🎯 TƯƠNG LAI: AUTO-UPDATE

Sau này có thể thêm tính năng tự động update:
1. Host file version.json trên web server
2. App check version mới khi khởi động
3. Download installer mới nếu có
4. Hoặc dùng ClickOnce deployment

---

**Status:** ✅ Sẵn sàng tạo installer  
**Next:** Download Inno Setup và compile setup-script.iss
