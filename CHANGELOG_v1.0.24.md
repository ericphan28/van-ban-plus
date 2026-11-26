# VanBanPlus v1.0.24

**Ngày phát hành:** 27/11/2026
**Loại bản cập nhật:** Sửa lỗi (không bắt buộc)

## 🎨 Hiển thị

- **Chấm màu mức độ** hiện đúng màu: Trang chủ «Cần xử lý», chú thích Lịch, độ ưu tiên cá nhân, kết quả soát văn bản, nhiệm vụ quá hạn, gợi ý của trợ lý. Trước đây emoji 🔴 🟡 🟢 🔵 🟠 ⚪ hiện thành vòng tròn gạch sọc đen trắng (WPF không vẽ được emoji màu); nay là dấu «●» tô màu.
- Hộp quét văn bản: ô Độ khẩn / Độ mật bỏ chấm tròn, chỉ còn chữ. Trạng thái họp «Đang diễn ra» dùng «▶».

## 🤖 Máy chủ (áp dụng cả bản cũ, không cần cập nhật)

- **Còn ít credit:** tác vụ AI chạy khi số dư ≥ mức thấp của ước tính. Trước đây bị chặn khi số dư dưới mức trần, dù chi phí thật thường chỉ khoảng một nửa mức trần. Khi số dư nằm giữa hai mức, máy chủ hạ trần độ dài kết quả cho vừa số dư (vanbanplus-api `a2ae467`).
- **AI soạn thảo:** không tự đặt số người tham dự, mốc giờ, thời hạn, kinh phí; chỗ thiếu ghi «[...]». Thêm luật soát chính tả. Địa danh của đơn vị đặt tên bằng chữ số / một chữ cái ghi đủ «Xã A», «Phường 1» (NĐ 30/2020 Phụ lục I).
- Bản web: soạn theo mẫu có sẵn ô người ký nay điền sẵn Người ký / Chức danh từ Cài đặt (vanbanplus-web `03287ea`).

## 🔧 Kỹ thuật

- `Views/ChamMau.cs`: chấm «●» + màu dùng chung (`GhiUuTien`, `Do`, `Vang`, `Xanh`…).
- Bump version: `1.0.23` → `1.0.24` (csproj, AssemblyVersion, FileVersion, Inno Setup). `update.xml`: `mandatory=false`.
