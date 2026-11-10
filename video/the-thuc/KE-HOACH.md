# Loạt «Thể thức văn bản — Nghị định 30/2020» (video/the-thuc/)

Chủ dự án chốt 08/11/2026: kênh riêng «AI Tạo Văn Bản Hành Chính [GIKIDI]», **2 clip/ngày lúc 05:00 và 18:00** (riêng 08/11:
12:00 + 18:00), đăng YouTube + Trang Facebook + TikTok bằng `clip-dang AIVanBanCaNhan the-thuc/NN --luc "YYYY-MM-DD HH:MM" …`.
Hướng nội dung: mẹo thể thức NĐ 30 (mỗi clip MỘT quy định, đối chiếu nguyên văn) + cảnh giới thiệu VanBanPlus; xen clip
demo phần mềm khi đã chụp màn hình app.

## Đọc trước cho người dựng (ngắn)

- Nguồn duy nhất: `docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md` (Phụ lục I). Chép câu nguyên văn vào
  đầu tệp clip (khối SOÁT) trước khi viết lời; màn kết đọc lại câu nguyên văn + `nguonCau("Phụ lục một", "Phụ lục I · mục …")`.
- Khuôn tệp: copy `01-ngay-05-thang-02.mjs` (thẻ + dấu + nhãn) hoặc `02-can-le-a4.mjs` (tờ A4). Cảnh «app» dùng nguyên
  `theApp("c0k0")` + câu lời cố định (đệm giọng có sẵn, không tốn ký tự).
- Tên VanBanPlus chỉ trên hình. Không nói phần mềm tự làm đúng một quy định cụ thể khi chưa soát mã.
- Mốc chữ `wN` đếm từ 0 — kiểm số chữ của cụm (đã vấp 2 lần: c1k0w4, c1k1w3 không tồn tại).
- Dựng ở PC (`chay.mjs the-thuc/NN --thu` → MP4), commit `video/` riêng (đừng gộp tệp app đang sửa), đẩy `main` (repo không
  tự triển khai), máy chủ `git pull`, `scp` MP4 vào `ra/NN-…/`, rồi `clip-dang` (chạy `--thu` trước).

## Bảng clip

| NN | Đề tài | Nguồn | Trạng thái |
|---|---|---|---|
| 01 | Ngày 5 hay ngày 05? (ngày < 10, tháng 1–2 thêm 0) | II.4b | đăng 08/11 12:00 |
| 02 | Căn lề A4: trái 30–35, phải 15–20, trên dưới 20–25 mm | I.1–I.4 | hẹn 08/11 18:00 |
| 03 | Số trang: giữa lề trên, trang 1 không hiện số | I.7 | hẹn 09/11 05:00 |
| 04 | Tên loại IN HOA, trích yếu, đường kẻ 1/3–1/2 dòng | II.5b | hẹn 09/11 18:00 |
| 05 | Địa danh: chữ cái đầu viết hoa, sau địa danh có dấu phẩy, in nghiêng | II.4c | đăng 10/11 09:00 |
| 06 | Quốc hiệu cỡ 12–13 in hoa đậm; Tiêu ngữ 13–14, gạch nối có cách chữ | II.1a–b | hẹn 10/11 18:00 |
| 07 | Từ «Số» chữ thường, sau có dấu hai chấm; số dưới 10 ghi 0 | II.3a, c | chưa làm (11/11 05:00) |
| 08 | Tên cơ quan ban hành in hoa đậm, đường kẻ 1/3–1/2 | II.2b | chưa làm (11/11 18:00) |
| 09 | Nơi nhận: chữ nghiêng đậm cỡ 12, danh sách cỡ 11 | II.9d | chưa làm — đọc nguyên văn trước |
| 10 | Viện dẫn lần đầu phải ghi đủ tên loại, số, ngày, cơ quan, trích yếu | II.6b | chưa làm |
| 11 | Phụ lục kèm văn bản: «Phụ lục» cỡ 14 đậm, đánh số La Mã | III.1 | chưa làm — đọc nguyên văn trước |
| 12 | Dấu mức độ khẩn: HỎA TỐC, THƯỢNG KHẨN, KHẨN | III.2b | chưa làm |
