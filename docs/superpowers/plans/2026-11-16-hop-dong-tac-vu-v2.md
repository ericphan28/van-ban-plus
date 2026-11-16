# Hợp đồng tác vụ v2 — dùng chung cho kế hoạch đợt 2 (máy chủ) và đợt 3 (bản cài máy)

Ngày: 16/11/2026. Spec gốc: `docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md` (§5, §6, §10).
Đợt 1 đã lên production 12/11/2026: khung `chayTacVu`, `tom-tat`, `/api/v2/credit`, migration 004.
Mọi kế hoạch đợt 2/3 PHẢI dùng đúng tên dưới đây; đổi tên thì sửa tệp này trước.

## Chung

- `POST /api/v2/tac-vu/<ten>` và `POST /api/v2/tac-vu/<ten>/uoc-tinh`, header `X-API-Key`, `X-VBP-Nguon` (`desktop` | `web`),
  `X-VBP-Phien-Ban` (phiên bản ứng dụng). Thân là JSON. Tên trường đầu vào: snake_case tiếng Việt không dấu như dưới.
- Thành công: `{ success: true, data: { ket_qua, credit_da_tru, so_du, ma_luot, bi_cat } }`.
  Ước tính: `{ success: true, data: { credit_min, credit_max, so_du, du_credit } }`.
- Lỗi: `{ success: false, message, code }`, mã và HTTP theo spec §6, thêm `NGAT_KET_NOI` 499. 503 có `Retry-After`.
  401 (thiếu/sai khóa), 404 (tác vụ không có), 500 (lỗi máy chủ) không có `code`.
- Tác vụ trả JSON: `ket_qua` là đối tượng đúng tên trường JSON của model C# hiện có (giữ nguyên như prompt hiện nay).
  Tác vụ trả văn bản: `ket_qua` là chuỗi.
- Ngày tháng trong đầu vào: chuỗi ISO `yyyy-MM-dd`; máy chủ tự định dạng giống hệt C# khi dựng prompt.
- Trường chuỗi tùy chọn: thiếu, `null` hoặc `""` đều coi là không có.

## Từng tác vụ

| Tác vụ | Đầu vào | `ket_qua` | Nguồn prompt C# |
| --- | --- | --- | --- |
| `tom-tat` (đã có) | `noi_dung`*, `loai_van_ban`, `tieu_de`, `co_quan` | JSON `DocumentSummary` | `DocumentSummaryService` |
| `kiem-tra` | `noi_dung`*, `loai_van_ban`, `tieu_de`, `co_quan`, `van_ban_doi_chieu` | JSON `DocumentReviewResult` | `DocumentReviewService` |
| `tham-muu` | `noi_dung`*, `loai_van_ban`, `tieu_de`, `co_quan`, `ngu_canh` (đối tượng, dưới) | JSON `DocumentAdvisory` | `DocumentAdvisoryService` |
| `bao-cao-dinh-ky` | `loai_ky`*, `ky`*, `linh_vuc`*, `don_vi`*, `so_lieu`*, `bao_cao_ky_truoc`, `nguoi_ky`, `chuc_danh` | chuỗi | `PeriodicReportService` |
| `trich-xuat` | `tep`* = mảng `{ base64, mime }` (1..20), `che_do` = `cau-truc` (mặc định) \| `chi-chu` | `cau-truc`: JSON `ExtractedDocumentData`; `chi-chu`: chuỗi | `/api/ai/extract` + `/api/ai/read-text` của máy chủ, `GeminiAIService` |
| `soan-thao` | `loai_van_ban`* (tên enum `DocumentType` C#, vd `CongVan`), `truong`* (đối tượng khóa → chuỗi), `dia_phuong` (đối tượng, dưới) | chuỗi | `AIComposeDialog` (tách sang `AIVanBan.Core`) |

`*` = bắt buộc.

**`ngu_canh` của `tham-muu`** — ánh xạ `DocumentAdvisoryContext`:
`huong` (Direction), `nhom` (Category), `so_van_ban` (DocumentNumber), `ngay_ban_hanh` (IssuedDate, ISO),
`do_khan` (UrgencyLevel), `do_mat` (SecurityLevel), `han_xu_ly` (DueDate, ISO), `nguoi_ky` (SignerName),
`chuc_danh_ky` (SignerTitle), `noi_nhan` (Recipients, mảng chuỗi), `giao_cho` (AssignedTo),
`ghi_chu_xu_ly` (ProcessingNotes), `can_cu` (BasedOn, mảng chuỗi), `van_ban_lien_quan` (RelatedDocumentsSummary).

**`dia_phuong` của `soan-thao`**: `{ xa, co_quan_cap_tren, tinh }` (chuỗi). Không gửi thì prompt không có bối cảnh địa phương
(spec §10). Bộ kiểm so khớp dùng giá trị Gia Kiệm (`xa` = «xã Gia Kiệm», `co_quan_cap_tren` = «thành phố Thống Nhất»,
`tinh` = «tỉnh Đồng Nai») để so được từng ký tự với prompt cũ.

**`trich-xuat`**: tổng dung lượng tệp (sau giải base64) vượt `max_file_size_mb` của gói hiện hành → `QUA_DAI` 413.
Ước tính theo số tệp và dung lượng (spec §7).

## Tệp mẫu prompt

- C# `AIVanBan.Tests/MauPromptTests.cs` sinh `vanbanplus-api/tests/mau-prompt/<ten>.json` (đã có `tom-tat.json`);
  mỗi tác vụ thêm một hàm sinh, hàm prompt C# đổi sang `internal static` (hoặc tách lớp thuần) mà không đổi hành vi.
- Sau khi đợt 3 xóa prompt khỏi C#, các tệp mẫu đã commit ở `vanbanplus-api` trở thành mốc cố định; bộ sinh C# bị gỡ
  cùng prompt.

## Số migration

- `005_goi_nap.sql`: `orders.loai`, `orders.goi_nap_ma`, `plan_id`/`cycle` nullable + check, cấp lô `nap` 90 ngày.
- `006_luu_ho_tro.sql`: `tac_vu_luu`, `users.cho_phep_luu_ho_tro`, nhật ký mở xem.
