# VanBanPlus v1.0.15

**Ngày phát hành:** 14/05/2026
**Loại bản cập nhật:** Sửa lỗi & cải tiến trải nghiệm (không bắt buộc)

## 🐛 Sửa lỗi quan trọng

- **Lưu văn bản từ mẫu**: Khắc phục tình trạng nội dung từ mẫu bị xóa/thay sai khi nhấn "Lưu" sau khi soạn bằng AI.
- **Thay thế trường thông tin trong mẫu**: Áp dụng so khớp mờ (regex) cho các placeholder như `[TÊN ĐƠN VỊ]`, `[Vấn đề công văn]`, `[Nội dung công văn]`, v.v., giúp mẫu hiển thị đúng dữ liệu vừa nhập.

## ✨ Cải tiến

- **Hộp thoại chọn thư mục khi lưu**: Sau khi AI soạn xong, người dùng có thể chọn thư mục đích trước khi lưu (không còn phải kéo thả thủ công).
- **Nút "Di chuyển" cho từng văn bản**: Bổ sung trên danh sách văn bản; hỗ trợ cả di chuyển hàng loạt (bulk move) khi chọn nhiều mục.
- **ComboBox thư mục trong hộp thoại Sửa văn bản**: Cho phép đổi thư mục ngay khi chỉnh sửa thông tin văn bản.
- **Tăng tốc nạp mẫu (Templates)**: Dùng `DropCollection` + `Task.Run` + virtualization → mở trang Mẫu nhanh hơn rõ rệt, không còn treo UI.

## 📚 Trợ giúp

- Cập nhật trang **Trợ giúp** trong ứng dụng: thêm mục **"Có gì mới? (v1.0.15)"** ở đầu phần "Có gì mới", liệt kê toàn bộ thay đổi của bản này.

## 🔧 Kỹ thuật

- Bump version: `1.0.14` → `1.0.15` (csproj, AssemblyVersion, FileVersion).
- `update.xml`: `mandatory=false` (người dùng có thể tạm hoãn).
- Installer (Inno Setup) đã loại trừ `*.db, *.litedb, *.bak, *.log, settings.json, *.pdb, *.cache` để cài sạch trên máy mới.

## ⬇️ Tải về

- **Setup (x64)**: `VanBanPlus-Setup-1.0.15-x64.exe` (~54 MB)

## 🔄 Cập nhật tự động

Ứng dụng sẽ tự kiểm tra `update.xml` khi khởi động. Người dùng đang chạy v1.0.14 sẽ thấy thông báo cập nhật và có thể tải về cài đè (giữ nguyên dữ liệu).
