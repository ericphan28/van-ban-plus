# VanBanPlus v1.0.22

**Ngày phát hành:** 26/11/2026
**Loại bản cập nhật:** Sửa lỗi (không bắt buộc)

## 📝 Xuất Biên bản cuộc họp ra Word

- **Phần đầu văn bản** theo Nghị định 30/2020: tên cơ quan và «Số: .../BB-...» bên trái; Quốc hiệu, Tiêu ngữ và «<Địa danh>, ngày ... tháng ... năm ...» bên phải. Trước đây tên cơ quan nằm dưới Quốc hiệu, không có số, địa danh để trống «.........».
- **Địa danh** suy từ tên cơ quan («UBND xã A» → «Xã A»); không suy được thì lấy tại Cài đặt → 🏛️ Cơ quan.
- **Khối chữ ký** không còn bị tách, tên người ký không rơi sang trang sau.
- **Khổ giấy A4** đặt sẵn trong tệp Word (trước đây máy mở tự lấy khổ mặc định, thường là Letter).
- **Thành phần tham dự**: người trùng tên Chủ trì / Thư ký ghi đúng vai; dòng kết thúc ghi giờ, ngày kết thúc cuộc họp và «./.».
- Áp dụng cho cả Biên bản cuộc họp và Thông báo kết luận cuộc họp.

## 🔧 Kỹ thuật

- `MeetingWordExportService` (commit `c61f6c3`), bộ kiểm `MeetingWordExportTests`.
- Bump version: `1.0.21` → `1.0.22` (csproj, AssemblyVersion, FileVersion, Inno Setup). `update.xml`: `mandatory=false`.
