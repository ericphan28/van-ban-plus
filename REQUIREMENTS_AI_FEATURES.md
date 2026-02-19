# 📋 YÊU CẦU TÍNH NĂNG AI — VanBanPlus

> **Version:** 1.0 · **Ngày:** 13/02/2026
> **Dự án:** Quản lý văn bản hành chính tích hợp AI
> **AI Engine:** Google Gemini 2.5 Flash (chính) + OpenAI, Claude, Grok (mở rộng)

---

## 1. Kiến Trúc AI

### 1.1 Dual-Mode Architecture

| ID     | Yêu cầu                                                                                                 | Độ ưu tiên |
| ------ | --------------------------------------------------------------------------------------------------------- | -------------- |
| ARC-01 | Hỗ trợ 2 chế độ kết nối:**Proxy API** (qua server) và **Direct** (gọi thẳng Gemini) | P0             |
| ARC-02 | Proxy API: xác thực bằng API Key, quản lý quota, ghi log usage                                       | P0             |
| ARC-03 | Direct Mode: ẩn sau Dev Mode, tự hết hạn sau 1 giờ                                                   | P1             |
| ARC-04 | Tự động fallback: nếu proxy lỗi → thông báo, không tự chuyển direct                            | P1             |

### 1.2 Multi-Provider Gateway (Server-side)

| ID     | Yêu cầu                                                               | Độ ưu tiên |
| ------ | ----------------------------------------------------------------------- | -------------- |
| ARC-05 | Hỗ trợ 4 provider: Gemini, OpenAI, Claude, Grok                       | P0             |
| ARC-06 | Mỗi provider có adapter riêng: format request/response thống nhất  | P0             |
| ARC-07 | Chọn API Key theo thứ tự: User key → System default → Env variable | P0             |
| ARC-08 | Retry tự động: 3 lần, exponential backoff (2s → 4s → 6s) khi 429  | P1             |

---

## 2. Tính Năng AI

### 2.1 Soạn Văn Bản Tự Động

> **Mô tả:** Tạo văn bản hành chính hoàn chỉnh từ template + input người dùng.

| ID    | Yêu cầu                                        | Chi tiết                                                                                              |
| ----- | ------------------------------------------------ | ------------------------------------------------------------------------------------------------------ |
| AI-01 | Hệ thống template có prompt AI                | Mỗi template gồm:`Name`, `Category`, `PromptTemplate`, `RequiredFields`, `SampleScenarios` |
| AI-02 | Tạo form nhập động từ `RequiredFields`    | Parse danh sách field → render TextBox/ComboBox tương ứng                                         |
| AI-03 | Thay thế placeholder trong prompt               | `{field_name}` → giá trị user nhập                                                               |
| AI-04 | Gọi AI với System Instruction cố định       | Role:*"Chuyên gia soạn thảo văn bản hành chính VN"*                                           |
| AI-05 | Kết quả dạng plain text (không markdown)     | Người dùng xem + chỉnh sửa trong RichTextBox                                                      |
| AI-06 | Tự động lưu vào CSDL sau khi xác nhận     | Lưu vào bảng Documents kèm metadata                                                                |
| AI-07 | Kho mẫu mặc định ≥ 18 loại                 | Theo NĐ 30/2020: CV, QĐ, BC, TT, KH, TB, NQ, CT...                                                   |
| AI-08 | CRUD template: Thêm/Sửa/Xóa/Reset mặc định | Admin tự tạo mẫu mới                                                                               |
| AI-09 | Template Store: tải mẫu từ server             | Hiển thị trạng thái: Chưa tải / Đã có / Có cập nhật                                        |

**Tham số AI:**

| Param           | Giá trị            |
| --------------- | -------------------- |
| Model           | `gemini-2.5-flash` |
| Temperature     | `0.7`              |
| MaxOutputTokens | `16,384`           |

---

### 2.2 OCR — Trích Xuất Văn Bản Từ Ảnh/PDF

> **Mô tả:** AI Vision đọc ảnh chụp/scan PDF → trích xuất 14 trường metadata có cấu trúc.

| ID    | Yêu cầu                                                         | Chi tiết                    |
| ----- | ----------------------------------------------------------------- | ---------------------------- |
| AI-10 | Hỗ trợ định dạng: JPG, PNG, BMP, WebP, TIFF, GIF, PDF        | Giới hạn 20MB              |
| AI-11 | Chuyển file → Base64 + MIME type → gửi Gemini Vision          | Dùng `inlineData` format  |
| AI-12 | Trích xuất 14 trường có cấu trúc (Structured Output)       | Xem bảng output bên dưới |
| AI-13 | Post-processing: format Chương/Điều/Khoản, xóa header thừa | Regex-based                  |
| AI-14 | Fallback: nếu JSON parse lỗi → regex salvage từng field       | Không mất dữ liệu        |
| AI-15 | Preview ảnh/PDF trước khi extract                              | Hiển thị trong dialog      |
| AI-16 | Cho phép user chỉnh sửa kết quả trước khi lưu             | Form editable                |

**14 trường output:**

| #  | Field                | Kiểu  | Ví dụ                                 |
| -- | -------------------- | ------ | --------------------------------------- |
| 1  | `so_van_ban`       | string | 123/QĐ-UBND                            |
| 2  | `trich_yeu`        | string | V/v khen thưởng...                    |
| 3  | `loai_van_ban`     | enum   | Quyết định, Công văn... (24 loại) |
| 4  | `ngay_ban_hanh`    | string | 15/01/2026                              |
| 5  | `co_quan_ban_hanh` | string | UBND xã ABC                            |
| 6  | `nguoi_ky`         | string | Nguyễn Văn A                          |
| 7  | `noi_dung`         | string | Toàn văn nội dung                    |
| 8  | `noi_nhan`         | string | Sở Nội vụ, UBND huyện...            |
| 9  | `can_cu_phap_ly`   | string | Căn cứ Luật..., NĐ...               |
| 10 | `huong_van_ban`    | enum   | di / den / noi_bo                       |
| 11 | `linh_vuc`         | string | Kinh tế, Tư pháp...                  |
| 12 | `dia_danh`         | string | Biên Hòa                              |
| 13 | `chuc_danh_ky`     | string | CHỦ TỊCH                              |
| 14 | `tham_quyen_ky`    | string | TM. UBND                                |

**Tham số AI:**

| Param            | Giá trị              | Lý do                       |
| ---------------- | ---------------------- | ---------------------------- |
| Temperature      | `0.1`                | Ưu tiên chính xác        |
| MaxOutputTokens  | `65,536`             | Văn bản dài               |
| ThinkingBudget   | `0`                  | Tắt suy luận → nhanh hơn |
| ResponseMimeType | `application/json`   | Structured Output            |
| Retry            | 2 lần, backoff 2s→4s | Xử lý timeout              |

---

### 2.3 Kiểm Tra / Soát Lỗi Văn Bản

> **Mô tả:** AI phân tích văn bản theo 8 khía cạnh, cho điểm chất lượng, đề xuất bản sửa.

| ID    | Yêu cầu                                                                   | Chi tiết                       |
| ----- | --------------------------------------------------------------------------- | ------------------------------- |
| AI-17 | Kiểm tra 8 khía cạnh (xem bảng)                                         | Mỗi khía cạnh là 1 category |
| AI-18 | Mỗi lỗi có: severity, vị trí, text gốc, mô tả, gợi ý sửa, lý do | Hiển thị dạng danh sách     |
| AI-19 | 3 mức severity: 🔴 Critical · 🟡 Warning · 🟢 Suggestion                 | Phân loại rõ ràng           |
| AI-20 | Điểm chất lượng tổng: 1–10                                           | Hiển thị kèm tóm tắt       |
| AI-21 | Output `suggested_content`: toàn bộ VB đã sửa                        | User có thể 1-click áp dụng |

**8 khía cạnh kiểm tra:**

| # | Category        | Kiểm tra gì                                            |
| - | --------------- | -------------------------------------------------------- |
| 1 | `spelling`    | Lỗi chính tả, viết hoa tiếng Việt                  |
| 2 | `style`       | Văn phong hành chính                                  |
| 3 | `conflict`    | Mâu thuẫn nội dung giữa các đoạn                  |
| 4 | `logic`       | Đánh số liên tục, tham chiếu hợp lệ              |
| 5 | `missing`     | Thiếu thành phần bắt buộc (căn cứ, nơi nhận...) |
| 6 | `ambiguous`   | Chủ thể, deadline, số liệu không rõ                |
| 7 | `enhancement` | Gợi ý viết tốt hơn                                  |
| 8 | `format`      | Thể thức theo NĐ 30/2020 (7 sub-check)                |

---

### 2.4 Tham Mưu Xử Lý Văn Bản Đến

> **Mô tả:** AI đóng vai chuyên viên tham mưu, phân tích VB đến → đề xuất hướng xử lý.

| ID    | Yêu cầu                                           | Chi tiết                                          |
| ----- | --------------------------------------------------- | -------------------------------------------------- |
| AI-22 | AI phân tích VB đến theo 15 chiều (xem bảng)  | Structured JSON output                             |
| AI-23 | Hiểu cơ cấu UBND xã/phường                    | CT, PCT-KT, PCT-VX, VP, Tư pháp, Địa chính... |
| AI-24 | Đề xuất người xử lý cụ thể theo lĩnh vực | Map field → position                              |
| AI-25 | Gợi ý draft phản hồi nếu cần trả lời        | Outline nội dung                                  |
| AI-26 | Cảnh báo rủi ro + căn cứ pháp lý liên quan  | Danh sách VB liên quan                           |

**15 chiều phân tích:**

| #  | Field                       | Output                                |
| -- | --------------------------- | ------------------------------------- |
| 1  | `summary`                 | Tóm tắt 3-5 câu                    |
| 2  | `urgency_level`           | thuong / khan / thuong_khan / hoa_toc |
| 3  | `action_items`            | Từng bước + người + timeline     |
| 4  | `deadlines`               | Thời hạn xử lý                    |
| 5  | `suggested_handler`       | Vị trí cụ thể                     |
| 6  | `coordination_units`      | Phòng ban phối hợp                 |
| 7  | `signing_authority`       | CT / PCT-KT / PCT-VX...               |
| 8  | `needs_response`          | true / false                          |
| 9  | `response_type`           | Loại VB trả lời                    |
| 10 | `draft_response`          | Outline phản hồi                    |
| 11 | `legal_references`        | Căn cứ pháp lý                    |
| 12 | `risk_warnings`           | Cảnh báo                            |
| 13 | `priority`                | high / medium / low                   |
| 14 | `related_field`           | Lĩnh vực (14+ loại)                |
| 15 | `document_classification` | Phân loại VB đến                  |

---

### 2.5 Tóm Tắt Văn Bản

> **Mô tả:** AI đọc văn bản → tóm tắt có cấu trúc 10 mục.

| ID    | Yêu cầu                                                                                                 | Chi tiết              |
| ----- | --------------------------------------------------------------------------------------------------------- | ---------------------- |
| AI-27 | Tóm tắt 10 trường: brief, type, authority, audience, key_points, legal, dates, figures, impact, notes | JSON output            |
| AI-28 | `key_points` là mảng object {heading, content}                                                        | Phân cấp rõ ràng   |
| AI-29 | Áp dụng cho cả VB nhập tay và VB đã OCR                                                            | Input = nội dung text |

---

### 2.6 Báo Cáo Định Kỳ

> **Mô tả:** AI soạn báo cáo từ số liệu thô, tự so sánh kỳ trước.

| ID    | Yêu cầu                                                             | Chi tiết                           |
| ----- | --------------------------------------------------------------------- | ----------------------------------- |
| AI-30 | Chọn kỳ: Tuần / Tháng / Quý / 6 tháng / Năm                    | ComboBox                            |
| AI-31 | Chọn lĩnh vực: 18 danh mục (KT-XH, CCHC, Tài chính...)          | ComboBox                            |
| AI-32 | Nhập số liệu thô (text tự do)                                    | TextBox multiline                   |
| AI-33 | (Tùy chọn) Dán báo cáo kỳ trước → AI tự tính % tăng/giảm | So sánh tự động                 |
| AI-34 | Output: chỉ phần body, không header/footer                         | Phần mềm tự thêm khi xuất Word |
| AI-35 | Cấu trúc 3 phần: Kết quả · Đánh giá · Phương hướng      | Chuẩn hành chính                 |
| AI-36 | Văn phong hành chính, không dùng markdown                        | Plain text                          |

---

### 2.7 Đọc Text Thuần (Simple OCR)

> **Mô tả:** OCR đơn giản — chỉ trả raw text, không metadata.

| ID    | Yêu cầu                                               | Chi tiết          |
| ----- | ------------------------------------------------------- | ------------------ |
| AI-37 | Input: ảnh/PDF → Output: raw text                     | Không JSON        |
| AI-38 | Giữ nguyên format gốc (xuống dòng, khoảng trắng) | Trung thực        |
| AI-39 | Dùng cho: copy nội dung, paste vào VB khác          | Use case phụ trợ |

---

## 3. Prompt Engineering

### 3.1 Nguyên Tắc Chung

| ID    | Yêu cầu                                                                             |
| ----- | ------------------------------------------------------------------------------------- |
| PE-01 | Mỗi tính năng có System Instruction riêng, định nghĩa role + rules            |
| PE-02 | Prompt bằng tiếng Việt, dùng thuật ngữ hành chính chuẩn                      |
| PE-03 | Quy định rõ format output (JSON schema hoặc plain text)                           |
| PE-04 | Liệt kê các "KHÔNG ĐƯỢC" (ví dụ: không dùng markdown, không thêm header) |
| PE-05 | Prompt OCR dùng `temperature: 0.1` để tối ưu chính xác                       |
| PE-06 | Prompt sáng tạo (soạn VB, báo cáo) dùng `temperature: 0.7`                    |

### 3.2 Template Prompt (Soạn VB)

| ID    | Yêu cầu                                                              |
| ----- | ---------------------------------------------------------------------- |
| PE-07 | Mỗi template có `PromptTemplate` chứa placeholder `{field}`     |
| PE-08 | Mỗi template có `SampleScenarios` để user thử nhanh             |
| PE-09 | Hỗ trợ kịch bản mẫu: fill sẵn tất cả field → 1-click generate |

---

## 4. Hạ Tầng & Phi Chức Năng

### 4.1 Quota & Usage Tracking

| ID    | Yêu cầu                                                                | Chi tiết              |
| ----- | ------------------------------------------------------------------------ | ---------------------- |
| NF-01 | Ghi log mỗi lượt gọi AI: user, action, provider, model, tokens, cost | Bảng `usage_logs`   |
| NF-02 | Ghi log chi phí: input 0.00759 VNĐ/token, output 0.06325 VNĐ/token | Gemini 2.5 Flash pricing |
| NF-03 | 4 gói: Free (20 req) · Starter (150) · Pro (500) · Business (2000) | Xem bảng subscription |
| NF-04 | Kiểm tra quota trước mỗi lần gọi AI                                | Reject nếu vượt     |
| NF-05 | Dashboard admin: thống kê usage theo ngày/user/action                 | Chart + bảng          |

### 4.2 Bảo Mật

| ID    | Yêu cầu                                                                 |
| ----- | ------------------------------------------------------------------------- |
| NF-06 | API Key mã hóa khi lưu client-side                                     |
| NF-07 | Server validate API Key mỗi request, check `is_active`                 |
| NF-08 | Không log nội dung văn bản lên server (chỉ metadata)                |
| NF-09 | Dev Mode ẩn, tự hết hạn, không phơi API key trong UI bình thường |

### 4.3 Hiệu Năng

| ID    | Yêu cầu                                                 |
| ----- | --------------------------------------------------------- |
| NF-10 | OCR: response ≤ 30s cho file ≤ 5MB                      |
| NF-11 | Soạn VB: response ≤ 20s cho văn bản thông thường   |
| NF-12 | Retry tự động với exponential backoff khi 429/timeout |
| NF-13 | Client hiển thị loading indicator + cho phép Cancel    |

### 4.4 Xuất Word

| ID    | Yêu cầu                                                                      |
| ----- | ------------------------------------------------------------------------------ |
| NF-14 | Xuất .docx chuẩn Thông tư 01/2011/TT-BNV                                   |
| NF-15 | Font: Times New Roman 14pt, giãn dòng 1.3                                    |
| NF-16 | Lề: Trên 2cm, Dưới 1.5cm, Trái 2cm, Phải 1cm                             |
| NF-17 | Tự động tạo: Quốc hiệu, Tiêu ngữ, Số VB, Ngày tháng, Khối chữ ký |
| NF-18 | Hỗ trợ đặc thù: QĐ (dòng thẩm quyền), NQ (nhãn), CT (nhãn)          |

---

## 5. Ma Trận Tính Năng AI

| Tính năng | Model            | Temp | MaxTokens | Vision | Structured | Streaming |
| ----------- | ---------------- | ---- | --------- | ------ | ---------- | --------- |
| Soạn VB    | gemini-2.5-flash | 0.7  | 16,384    | ❌     | ❌         | ❌        |
| OCR Extract | gemini-2.5-flash | 0.1  | 65,536    | ✅     | ✅ (JSON)  | ❌        |
| Soát lỗi  | gemini-2.5-flash | 0.7  | —        | ❌     | ✅ (JSON)  | ❌        |
| Tham mưu   | gemini-2.5-flash | 0.7  | —        | ❌     | ✅ (JSON)  | ❌        |
| Tóm tắt   | gemini-2.5-flash | 0.7  | —        | ❌     | ✅ (JSON)  | ❌        |
| Báo cáo   | gemini-2.5-flash | 0.7  | —        | ❌     | ❌         | ❌        |
| Read Text   | gemini-2.5-flash | 0.1  | 16,384    | ✅     | ❌         | ❌        |

---

## 6. Gói Dịch Vụ (Subscription)

| Gói                 | Requests/tháng   | Tokens/tháng     | File Size | Vision | Streaming | Giá      |
| -------------------- | ----------------- | ----------------- | --------- | ------ | --------- | --------- |
| **Free**       | 20                | 50K               | 5MB       | ✅     | ❌        | 0đ       |
| **Starter**    | 150               | 500K              | 10MB      | ✅     | ✅        | 79,000đ  |
| **Pro**        | 500               | 2M                | 20MB      | ✅     | ✅        | 199,000đ |
| **Business**   | 2,000             | 10M               | 50MB      | ✅     | ✅        | 499,000đ |

---

## 7. API Endpoints

| Method | Endpoint               | Mô tả                                               | Auth             |
| ------ | ---------------------- | ----------------------------------------------------- | ---------------- |
| POST   | `/api/ai/generate`   | Soạn VB, Soát lỗi, Tham mưu, Tóm tắt, Báo cáo | API Key          |
| POST   | `/api/ai/extract`    | OCR trích xuất có cấu trúc                       | API Key          |
| POST   | `/api/ai/read-text`  | OCR đọc text thuần                                 | API Key          |
| GET    | `/api/admin/stats`   | Thống kê hệ thống                                 | Admin Key        |
| CRUD   | `/api/admin/ai-keys` | Quản lý AI provider keys                            | Admin Key        |
| POST   | `/api/admin/auth`    | Đăng nhập admin                                    | Email + Password |

---

*Tài liệu này mô tả yêu cầu tính năng AI cho hệ thống VanBanPlus — dùng làm cơ sở phát triển, kiểm thử và nghiệm thu.*
