# VanBanPlus v1.0.18

**Ngày phát hành:** 11/11/2026
**Loại bản cập nhật:** Sửa lỗi tính năng AI (không bắt buộc, khuyến nghị cập nhật)

## 📑 Sửa lỗi trích xuất văn bản nhiều trang

- **Ghép trang chạy qua tài khoản VanBanPlus:** chế độ «Ghép trang» (nhiều ảnh scan → một văn bản) trước đây gọi thẳng Gemini bằng khóa riêng trên máy nên báo lỗi 403 khi máy chưa nhập khóa Gemini. Nay gửi mọi trang qua VanBanPlus trong một lần, dùng chung hạn mức như các tính năng AI khác.
- **Nhận tệp lớn hơn:** tối đa 20 MB mỗi lần gửi, báo rõ khi vượt.

## 🔧 Kỹ thuật

- Bump version: `1.0.17` → `1.0.18` (csproj, AssemblyVersion, FileVersion, Inno Setup).
- `GeminiAIService.ExtractDocumentFromMultipleFilesAsync`: chế độ VanBanPlus gửi `files[]` tới `/api/ai/extract`.
- API (`vanbanplus-api` 7eb7c84): `/api/ai/extract` nhận `files[]` (tối đa 30 trang, 20 MB); `proxyClientMaxBodySize` 30 MB.
- Thêm 2 kiểm thử `GeminiAIServiceTests` (tổng 66).
