# 🔗 Ánh xạ Văn bản Pháp quy → Tính năng Phần mềm

> File này giúp Copilot biết **điều khoản nào** áp dụng cho **tính năng nào** trong VanBanPlus.
> Khi phát triển hoặc sửa tính năng, hãy tham chiếu file này để đảm bảo đúng quy định.

---

## 1. Quản lý Văn bản Hành chính

### 1.1 Phân loại văn bản

| Điều khoản | Văn bản | Áp dụng vào | File code liên quan |
|-----------|---------|-------------|---------------------|
| Điều 7 — Các loại văn bản hành chính | NĐ 30/2020 | Danh sách loại văn bản trong app (Nghị quyết, Quyết định, Chỉ thị, Công văn...) | `AIVanBan.Core/Models/Document.cs` → enum `DocumentType` |
| Điều 8 — Thể thức văn bản | NĐ 30/2020 | Template soạn văn bản, AI soạn thảo | `AIVanBan.Core/Services/GeminiAIService.cs` |
| Phụ lục I — Mẫu trình bày | NĐ 30/2020 | Mẫu layout cho từng loại văn bản | Phụ lục: `docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md` |

### 1.2 Soạn thảo văn bản

| Điều khoản | Văn bản | Áp dụng vào | File code liên quan |
|-----------|---------|-------------|---------------------|
| Điều 8 — Quốc hiệu, Tiêu ngữ | NĐ 30/2020 | Header văn bản: "CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM" | AI prompt template |
| Điều 8 — Tên cơ quan ban hành | NĐ 30/2020 | Phần header bên trái | Template soạn thảo |
| Điều 8 — Số, ký hiệu văn bản | NĐ 30/2020 | Trường "Số văn bản" trong form | `Document.DocumentNumber` |
| Điều 8 — Địa danh, ngày tháng | NĐ 30/2020 | Format ngày trên văn bản | AI soạn thảo |
| Điều 8 — Trích yếu nội dung | NĐ 30/2020 | Trường "Trích yếu" | `Document.Summary` |
| Điều 9 — Ký hiệu viết tắt | NĐ 30/2020 | Hướng dẫn tạo ký hiệu văn bản | Tooltip hướng dẫn |

### 1.3 Đánh số và ký hiệu

| Điều khoản | Văn bản | Áp dụng vào | File code liên quan |
|-----------|---------|-------------|---------------------|
| Điều 12 — Số văn bản | NĐ 30/2020 | Auto-generate số thứ tự văn bản theo năm | `DocumentService` |
| Điều 13 — Ký hiệu văn bản | NĐ 30/2020 | Format: `Số/Loại-CQ` (VD: `01/QĐ-UBND`) | Validation logic |

---

## 2. Quản lý Hồ sơ & Lưu trữ

| Điều khoản | Văn bản | Áp dụng vào | File code liên quan |
|-----------|---------|-------------|---------------------|
| Điều 22-25 — Lập hồ sơ | NĐ 30/2020 | Tổ chức văn bản theo hồ sơ/danh mục | Tính năng quản lý hồ sơ |
| Điều 26-28 — Nộp lưu hồ sơ | NĐ 30/2020 | Chức năng lưu trữ, sao lưu | Backup/Export |
| Luật Lưu trữ 2011/2024 | Luật | Chính sách lưu trữ dài hạn | Backup strategy |

---

## 3. Album Ảnh Công việc

| Điều khoản | Văn bản | Áp dụng vào | File code liên quan |
|-----------|---------|-------------|---------------------|
| Điều 14 — Phụ lục kèm VB | NĐ 30/2020 | Đính kèm ảnh/file vào văn bản | Album feature |
| TT 01/2019/TT-BNV | Thông tư | Quy trình xử lý tài liệu điện tử | Photo management |

---

## 4. Bản sao & Sao y

| Điều khoản | Văn bản | Áp dụng vào | File code liên quan |
|-----------|---------|-------------|---------------------|
| Điều 19 — Bản sao | NĐ 30/2020 | Tính năng tạo bản sao y, sao lục | [TODO] |
| Phụ lục II — Mẫu sao y | NĐ 30/2020 | Template bản sao y bản chính | `phu-luc-II.md` |
| Phụ lục III — Mẫu sao lục | NĐ 30/2020 | Template sao lục | `phu-luc-III.md` |

---

## 5. Biên bản Cuộc họp

| Điều khoản | Văn bản | Áp dụng vào | File code liên quan |
|-----------|---------|-------------|---------------------|
| Điều 7 khoản 16 — Biên bản | NĐ 30/2020 | Template biên bản cuộc họp | `MeetingMinuteService` |
| Phụ lục I — Mẫu biên bản | NĐ 30/2020 | Layout chuẩn cho biên bản | AI soạn biên bản |

---

## 6. AI Soạn thảo

| Điều khoản | Văn bản | Áp dụng vào | File code liên quan |
|-----------|---------|-------------|---------------------|
| Toàn bộ Phụ lục I-VI | NĐ 30/2020 | System prompt cho AI — tuân thủ mẫu chuẩn | `GeminiAIService.cs` prompt templates |
| Điều 8 — Thể thức | NĐ 30/2020 | AI phải sinh văn bản đúng thể thức | AI prompt instructions |
| Điều 7 — Loại VB | NĐ 30/2020 | AI phải chọn đúng loại VB phù hợp ngữ cảnh | AI classification |

---

## 📌 Quy tắc cho Copilot

Khi thay đổi bất kỳ tính năng nào ở trên:

1. **Đọc điều khoản liên quan** trong `noi-dung.md` của văn bản tương ứng
2. **Kiểm tra phụ lục mẫu** nếu liên quan đến template/layout
3. **Đảm bảo tuân thủ** — nếu code trái với quy định, ưu tiên quy định
4. **Ghi chú trong code** — comment reference đến điều khoản (VD: `// Theo Điều 8, NĐ 30/2020`)
