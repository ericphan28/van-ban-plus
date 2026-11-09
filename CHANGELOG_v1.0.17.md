# VanBanPlus v1.0.17

**Ngày phát hành:** 09/11/2026
**Loại bản cập nhật:** Sửa lỗi tính năng AI (không bắt buộc, khuyến nghị cập nhật)

## 🤖 Tính năng AI ổn định hơn

- **Chuyển sang model AI mới:** khắc phục lỗi soạn thảo và trích xuất văn bản bị treo, không phản hồi hoặc báo lỗi.
- **Tự thử lại khi máy chủ AI quá tải tạm thời**, không cần thao tác lại.
- **Không chờ vô hạn:** soạn nội dung văn bản chờ tối đa 150 giây rồi báo rõ, thay vì 5 phút như trước.
- **Thông báo lỗi dễ hiểu:** máy chủ AI quá tải, phản hồi quá lâu, hoặc tài khoản cần đăng nhập lại.

## ⚠️ Lưu ý về tài khoản

Hệ thống đã chuyển máy chủ từ 07/11/2026. Tài khoản tạo trước ngày này cần đăng ký lại rồi đăng nhập trong ứng dụng để dùng AI qua VanBanPlus. Dữ liệu văn bản trên máy không bị ảnh hưởng.

## 🔧 Kỹ thuật

- Bump version: `1.0.16` → `1.0.17` (csproj, AssemblyVersion, FileVersion, Inno Setup).
- `GeminiAIService`: model `gemini-3.5-flash`; thử lại khi gặp 429, 502, 503, 504; streaming tạo request mới mỗi lần thử.
- Thêm 10 kiểm thử `GeminiAIServiceTests` (tổng 64).
