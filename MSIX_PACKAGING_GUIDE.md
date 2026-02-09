# Hướng dẫn tạo MSIX Package cho AIVanBan Desktop

## Yêu cầu
- Visual Studio 2022 với Windows Application Packaging Project
- Windows 10 SDK (10.0.19041.0 trở lên)

## Các bước thực hiện

### Bước 1: Thêm Windows Application Packaging Project
1. Mở AIVanBan.sln trong Visual Studio
2. Right-click Solution → Add → New Project
3. Tìm "Windows Application Packaging Project"
4. Đặt tên: AIVanBan.Package
5. Chọn Target/Minimum version: Windows 10, version 1809

### Bước 2: Thêm reference
1. Right-click "Applications" trong Package project
2. Add Reference → chọn AIVanBan.Desktop
3. Set làm Entry Point

### Bước 3: Cấu hình Package.appxmanifest
- Điền thông tin: App name, Publisher, Logo, Description
- Thiết lập Capabilities (quyền truy cập)
- Cấu hình Visual Assets (icons, splash screen)

### Bước 4: Build & Create Package
1. Right-click Package project → Publish → Create App Packages
2. Chọn "Sideloading" (phát hành độc lập)
3. HOẶC "Microsoft Store" (nếu muốn đưa lên Store)
4. Chọn architecture: x64, x86, ARM64
5. Configure signing certificate
6. Build → output sẽ có file .msix hoặc .msixbundle

## Lợi ích MSIX
- Tự động update
- Clean uninstall (không để rác)
- Sandbox security
- Hỗ trợ Microsoft Store

## Hạn chế
- Phức tạp hơn Inno Setup
- Yêu cầu Windows 10 1809+
