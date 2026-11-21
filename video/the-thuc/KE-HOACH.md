# Loạt «Thể thức văn bản — Nghị định 30/2020» (video/the-thuc/)

Chủ dự án chốt 08/11/2026: kênh riêng «AI Tạo Văn Bản Hành Chính [GIKIDI]», **từ 11/11/2026: 3 clip/ngày lúc 05:00, 10:00, 17:00** (trước đó 2 clip/ngày 05:00 + 18:00) (riêng 08/11:
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

## Khuôn tranh luận + bản web (từ clip 35, chủ dự án chốt 22/11/2026)

- Chọn điểm **dễ cãi nhau** (thực tế làm khác quy định, hoặc chính Nghị định viết không thống nhất) để kéo bình luận.
- Móc: hai phương án A/B (hoặc đố nhiều câu) + câu đọc cố định «Anh chị chọn bên nào, bình luận trước, rồi xem tiếp.» +
  `nhanBinhLuan("c1k0")`. Màn kết có câu 3 mời bình luận: `binhLuanCau(…)` + `theBinhLuan(…)` (ket.the có 3 đoạn).
- Cảnh phần mềm: `theWeb("c0k0")` + `CAU_WEB` (đăng ký miễn phí trên trình duyệt, app.vanbanplus.giakiemso.com, gói tháng
  từ 99.000đ — giá đọc prod `/api/plans` 22/11/2026); thay `theApp` (bản cài Windows). Mô tả bài đăng kết bằng 💬 câu hỏi
  + dòng đăng ký bản web.
- Soát với bản gốc `nghi-dinh/30-2020-ND-CP/van-ban-goc/30.2020.NĐ-CP/*.md` (bản chuyển `phu-luc/*.md` từng bị cụt câu
  Phụ lục II mục V.7 — đã sửa 22/11/2026).

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
| 08 | Hai dòng tên cơ quan: dòng nào được viết tắt, dòng nào phải đầy đủ (làm lại 10/11) | II.2a, b | hẹn 11/11 10:00 |
| 09 | «Kính gửi» và «Nơi nhận»: loại nào có cả hai, cách ghi một nơi / nhiều nơi, «Như trên» (làm lại 10/11) | II.9b, c, d | hẹn 11/11 17:00 |
| 10 | Căn cứ và viện dẫn: Luật không ghi số, ký hiệu; Nghị định ghi đủ; lần sau ghi gọn; ; và . (làm lại 10/11) | II.6a, b | hẹn 12/11 05:00 |
| 11 | Ký thay: TM., Q., KT., TL., TUQ. khác nhau thế nào | II.7b, đ | hẹn 12/11 10:00 |
| 12 | Ký tên có được ghi «TS.», «ThS.» trước họ tên không (ngoại lệ giáo dục, y tế); chức vụ chính thức; tổ chức tư vấn | II.7c | hẹn 12/11 17:00 |
| 13 | Viết hoa: tháng Năm, thứ Hai, tết Nguyên đán / Tết, Nhân dân, Nhà nước, chức vụ đi liền tên; Quận 1 | Phụ lục II mục V, III.1b | hẹn 13/11 05:00 |
| 14 | Văn bản khẩn: ai quyết định, ba con dấu 30:40:20 × 8 mm, mực đỏ tươi, phạm vi lưu hành, độ mật | III.2 | hẹn 13/11 10:00 |
| 15 | Phụ lục kèm theo: số La Mã, số trang riêng, dòng «Kèm theo…», giấy / điện tử | III.1 | hẹn 13/11 17:00 |
| 16 | Chữ ký số: chữ ký xanh, dấu đỏ trùm 1/3 về bên trái, văn bản kèm theo không cùng tệp | II.7d, II.8 | hẹn 14/11 05:00 |
| 17 | Ký thừa ủy quyền: không được ủy quyền lại; thừa lệnh giao lại cho cấp phó; mực xanh | Điều 13 khoản 3, 4, 6 | hẹn 14/11 10:00 |
| 18 | Văn bản đã phát hành mà sai sót: sửa đổi / thay thế (nội dung) và đính chính bằng công văn (thể thức); phát hành trong ngày | Điều 18, Điều 15 | hẹn 14/11 17:00 |
| 19 | Văn bản đến: chưa đăng ký tại Văn thư thì không phải giải quyết; trình trong ngày; Hệ thống phải in giấy, ký nhận, đóng sổ | Điều 4, 22, 23 | hẹn 15/11 05:00 |
| 20 | Sao y, sao lục, trích sao khác nhau; giá trị pháp lý như bản chính; thẩm quyền sao | Điều 3, 25, 26, 27 | hẹn 15/11 10:00 |
| 21 | Thụt đầu dòng 1 / 1,27 cm, giãn đoạn ≥ 6pt, giãn dòng đơn → 1,5 lines, điểm a) b) c) d) đ) | Phụ lục I II.6 | hẹn 15/11 17:00 |
| 22 | Viết hoa tên cơ quan, địa danh (Sở Tài chính, thành phố Thái Nguyên, sông Vàm Cỏ…), tên nước ngoài viết tắt | Phụ lục II III, IV | hẹn 16/11 05:00 |
| 23 | Soạn, duyệt, kiểm tra bản thảo: ai chịu trách nhiệm nội dung, ai thể thức | Điều 10–12 | hẹn 16/11 10:00 |
| 24 | Con dấu: Văn thư giữ, giao dấu phải có phép + biên bản, không đóng dấu trước ký sau, tự bảo quản khóa bí mật | Điều 32 | hẹn 16/11 17:00 |
| 25 | Dấu giáp lai giữa mép phải, tối đa 5 tờ/dấu; dấu lên phụ lục; dấu treo/nổi do người đứng đầu quy định | Điều 33 | hẹn 17/11 05:00 |
| 26 | Kết thúc hồ sơ: loại bản trùng, bản nháp; đánh số tờ (từ 5 năm); mục lục (vĩnh viễn); biên mục điện tử | Điều 29 khoản 4 | hẹn 17/11 10:00 |
| 27 | Nộp lưu: 3 tháng (XDCB) / 1 năm; giữ lại không quá 2 năm; nghỉ hưu, chuyển công tác phải bàn giao hồ sơ | Điều 30, 31 | hẹn 17/11 17:00 |
| 28 | Danh mục hồ sơ: hồ sơ ngoài Danh mục tự xác định 5 thông tin; mở hồ sơ; cập nhật phim, ảnh, ghi âm | Điều 28, 29 | hẹn 18/11 05:00 |
| 29 | Văn bản hành chính có 29 loại (chia khúc 5·6·7·6·5 theo thứ tự liệt kê); công văn chỉ là 1 loại; giấy nghỉ phép, thư công cũng nằm trong danh sách | Điều 7 | hẹn 18/11 10:00 |
| 30 | Thể thức: 9 thành phần chính (a–i) + 4 nhóm bổ sung | Điều 8 | hẹn 18/11 17:00 |
| 31 | Văn bản đi: 5 bước cấp số → đăng ký → nhân bản/đóng dấu/ký số → phát hành → lưu; số duy nhất trong năm | Điều 14–19 | hẹn 19/11 05:00 |
| 32 | Bản gốc ≠ bản chính; lưu giấy (Văn thư / hồ sơ), lưu điện tử trên Hệ thống; Hệ thống chưa đáp ứng thì in ký số ra giấy đóng dấu | Điều 3, 18, 19 | hẹn 19/11 10:00 |
| 33 | Nhận văn bản đến: kiểm tra bì, bóc bì đóng dấu ĐẾN, gửi đích danh không bóc, việc chung chuyển lại Văn thư; điện tử: xác thực toàn vẹn, báo nhận trong ngày | Điều 21 | hẹn 19/11 17:00 |
| 34 | Chỉ đạo giải quyết văn bản đến: trình trong ngày, ý kiến ghi mục «Chuyển», chủ trì / phối hợp / thời hạn, ký nhận, khẩn giải quyết ngay | Điều 23, 24 | hẹn 20/11 05:00 |
| 35 | «Hòa» hay «hoà», «ủy» hay «uỷ»: chính NĐ 30 viết cả hai kiểu; không quy định chỗ đặt dấu thanh (tranh luận) | Điều 4, 13, Phụ lục I | hẹn 22/11 10:00 |
| 36 | Viện dẫn: viết hoa Phần, Chương, Mục, Tiểu mục, Điều; khoản, điểm viết thường; đầu câu vẫn hoa | Phụ lục II V.7, I | hẹn 22/11 17:00 |
| 37 | Cỡ chữ 13 hay 14 (cả hai đúng), phông Times New Roman; chỗ cỡ cố định: Số 13, Nơi nhận 12, Phụ lục 14 | Phụ lục I I.4, II.6e, II.3c, II.9, III.1b | hẹn 23/11 05:00 |
| 38 | Ký bút bi xanh có sai không: chỉ hai điều kiện mực xanh + không dễ phai; dấu mực đỏ; chữ ký số xanh, hình dấu đỏ | Điều 13.6, 33.1a, Phụ lục I II.7d, II.8a | hẹn 23/11 10:00 |
| 39 | «Số: 15/CV-UBND» sai: công văn không có chữ viết tắt tên loại; trích yếu sau «V/v» | Phụ lục I II.3b, II.5b, Phụ lục III | hẹn 23/11 17:00 |
| 40 | Đố chữ viết tắt: HD/HĐ, ĐA/DA, TrS, BGN | Phụ lục III mục I | hẹn 24/11 05:00 |
