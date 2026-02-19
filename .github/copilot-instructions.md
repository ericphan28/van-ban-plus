# VanBanPlus — Copilot Instructions

## ⚠️ ĐỌC TRƯỚC KHI LÀM BẤT CỨ GÌ

1. **`PROJECT_STATUS.md`** — Trạng thái tất cả tính năng (đã có / chưa có / đang làm)
2. File này (`copilot-instructions.md`) — Quy tắc code
3. Sau khi hoàn thành feature → **Cập nhật `PROJECT_STATUS.md`** (đánh dấu `[x]` + ngày)
4. **KHÔNG tạo file .md mới** cho mỗi feature/phase

## Tổng quan dự án

VanBanPlus là phần mềm quản lý văn bản hành chính cho cán bộ, công chức Việt Nam.
Phần mềm phải tuân thủ các quy định pháp luật về công tác văn thư.

## 📚 Văn bản pháp quy tham chiếu

Thư mục `docs/van-ban-phap-quy/` chứa các văn bản pháp luật dưới dạng Markdown.
**Luôn tham chiếu các văn bản này** khi phát triển tính năng liên quan đến nghiệp vụ văn thư.

### Văn bản chính:
- **NĐ 30/2020/NĐ-CP** — Nghị định về công tác văn thư (quan trọng nhất)
  - Nội dung: `docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md`
  - 6 Phụ lục mẫu: `docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/`
- **TT 01/2011/TT-BNV** — Thể thức và kỹ thuật trình bày
- **TT 01/2019/TT-BNV** — Tài liệu điện tử
- **Luật Lưu trữ 2011 & 2024**

### Ánh xạ quy định → tính năng:
- Xem `docs/van-ban-phap-quy/_MAPPING.md` để biết điều khoản nào áp dụng cho tính năng nào.

## 🏗️ Kiến trúc dự án

- **AIVanBan.Core** — Business logic, models, services (không có UI)
- **AIVanBan.Desktop** — WPF Desktop app (.NET 9, code-behind pattern)
- **AIVanBan.API** — ASP.NET Core API (backup, ít sử dụng)
- **vanbanplus-api** — Next.js API (Vercel, chính)

## 🇻🇳 Ngôn ngữ

- Giao diện: **Tiếng Việt** (tất cả UI text bằng tiếng Việt)
- Code comments: Tiếng Việt hoặc tiếng Anh đều được
- Variable/method names: Tiếng Anh

## ⚖️ Quy tắc nghiệp vụ

Khi phát triển tính năng liên quan đến văn bản hành chính:

1. **Đọc quy định trước** — Tìm trong `docs/van-ban-phap-quy/` điều khoản liên quan
2. **Tuân thủ mẫu chuẩn** — Phụ lục I-VI của NĐ 30/2020 là "source of truth"
3. **Ghi chú trong code** — Comment reference: `// Theo Điều X, NĐ 30/2020/NĐ-CP`
4. **29 loại văn bản** — Điều 7, NĐ 30/2020 quy định đủ 29 loại VB hành chính
5. **Ký hiệu viết tắt** — Phải đúng theo Phụ lục VI (VD: QĐ, CV, BC, KH...)
6. **Quy tắc viết hoa** — Tuân thủ Phụ lục V khi AI soạn thảo
7. **Thể thức văn bản** — Điều 8 quy định 12 thành phần bắt buộc

## 🤖 AI Soạn thảo

Khi thay đổi AI prompt templates hoặc system instructions:
- AI phải sinh văn bản đúng thể thức NĐ 30/2020
- Quốc hiệu, tiêu ngữ phải đúng chuẩn
- Ký hiệu văn bản phải đúng format: `Số/Loại-CQ`
- Quy tắc viết hoa theo Phụ lục V

## 📁 Data & Settings

- User data: `Documents\AIVanBan\` (Data, Photos, Cache, Backups)
- Settings: `Documents\AIVanBan\settings.json`
- Database: LiteDB tại `Documents\AIVanBan\Data\documents.db`

## 🔧 LiteDB Lưu ý quan trọng

- Dùng `FindAll().Where()` thay vì `Find()` cho fields có thể null trong BSON cũ
- Khi thêm field mới vào Model → xử lý null cho documents đã tồn tại
- Dùng `DropCollection()` khi cần clear data

## 📋 Quy trình implement feature

1. Đọc `PROJECT_STATUS.md` → xác nhận feature chưa có
2. Đánh dấu `[~]` + ngày bắt đầu
3. Code theo thứ tự: **Model → Service → UI → Test → Build**
4. Build toàn bộ solution — 0 errors
5. Đánh dấu `[x]` + ngày hoàn thành trong `PROJECT_STATUS.md`
6. Cập nhật `CHANGELOG.md` nếu feature lớn
- Database: LiteDB tại `Documents\AIVanBan\Data\documents.db`
