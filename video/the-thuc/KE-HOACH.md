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

## Tiêu chuẩn nội dung (chủ dự án chốt 10/11/2026 sau khi chê clip 07–10 bản đầu «nhàm, ít kiến thức»)

- Mỗi clip phải có **kiến thức ít người biết hoặc dễ làm sai** (lấy từ chính Nghị định), kèm **ví dụ minh họa trên hình** và **lỗi điển hình**;
  không chỉ đọc lại một điểm của Phụ lục I («cỡ 12–13, in hoa, đậm»).
- Khuôn: móc là câu hỏi / nghịch lý có đáp án đáng biết → minh họa biến đổi trực tiếp (sai → đúng) → thẻ nguyên văn mang một ý MỚI
  (đừng đọc lại điều vừa nói). Giữ cảnh phần mềm; «Tóm lại» tuỳ thời lượng. Mục tiêu 47–52 s.
- Ví dụ giữ chỗ («Sở A», «XÃ A», «VĂN PHÒNG») ghi «ví dụ minh họa» trên hình; không tên cơ quan thật.
- **BẮT BUỘC mỗi clip (chủ dự án 10/11/2026, «vô cùng quan trọng»): giữ cảnh giới thiệu phần mềm (thẻ VanBanPlus, `theApp("c0k0", true)` có dòng «Sản phẩm của Công ty Gia Kiệm Số», câu đọc nhắc Công ty Gia Kiệm Số + tải miễn phí). KHÔNG được bỏ.** Thời lượng 47–52 s như 01–06, không ngắn hơn.
- Bảng chữ viết tắt (Phụ lục III) và quy tắc viết hoa (Phụ lục II) còn nhiều chất liệu chưa dùng.

## Bảng clip

| NN | Đề tài | Nguồn | Trạng thái |
|---|---|---|---|
| 01 | Ngày 5 hay ngày 05? (ngày < 10, tháng 1–2 thêm 0) | II.4b | đăng 08/11 12:00 |
| 02 | Căn lề A4: trái 30–35, phải 15–20, trên dưới 20–25 mm | I.1–I.4 | hẹn 08/11 18:00 |
| 03 | Số trang: giữa lề trên, trang 1 không hiện số | I.7 | hẹn 09/11 05:00 |
| 04 | Tên loại IN HOA, trích yếu, đường kẻ 1/3–1/2 dòng | II.5b | hẹn 09/11 18:00 |
| 05 | Địa danh: chữ cái đầu viết hoa, sau địa danh có dấu phẩy, in nghiêng | II.4c | đăng 10/11 09:00 |
| 06 | Quốc hiệu cỡ 12–13 in hoa đậm; Tiêu ngữ 13–14, gạch nối có cách chữ | II.1a–b | hẹn 10/11 18:00 |
| 07 | Chữ viết tắt TTr · CTr · QyĐ (QĐ ≠ QyĐ) + dòng «Số:» đúng + số chạy theo năm (làm lại 10/11: bản cũ «5 hay 05?» nhàm) | Phụ lục III, II.3c, Điều 15 | hẹn 11/11 05:00 |
| 08 | Hai dòng tên cơ quan: dòng nào được viết tắt, dòng nào phải đầy đủ (làm lại 10/11) | II.2a, b | hẹn 11/11 18:00 |
| 09 | «Kính gửi» và «Nơi nhận»: loại nào có cả hai, cách ghi một nơi / nhiều nơi, «Như trên» (làm lại 10/11) | II.9b, c, d | hẹn 12/11 05:00 |
| 10 | Căn cứ và viện dẫn: Luật không ghi số, ký hiệu; Nghị định ghi đủ; lần sau ghi gọn; ; và . (làm lại 10/11) | II.6a, b | hẹn 12/11 18:00 |
| 11 | Phụ lục kèm văn bản: «Phụ lục» cỡ 14 đậm, đánh số La Mã | III.1 | chưa làm — đọc nguyên văn trước |
| 12 | Dấu mức độ khẩn: HỎA TỐC, THƯỢNG KHẨN, KHẨN | III.2b | chưa làm |
