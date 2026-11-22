# Loạt «Việc thật» (video/viec-that/)

Chủ dự án chốt 22/11/2026: bỏ hướng mẹo NĐ 30 (loạt `the-thuc/` dừng ở clip 46), chuyển sang **trải nghiệm thật** —
AI Văn Bản làm được gì cho khách, quay cả **bản web** lẫn **app desktop**, xoáy vào **giá trị khách sẵn sàng trả tiền**.
Loạt này thay `lam-that/` và `huong-dan-web/` (dùng lại công cụ quay / chụp và ảnh đã có của hai loạt đó).

## Khuôn một clip (45–50 s)

1. **Móc (0–4 s):** một việc cụ thể đang đè lên cán bộ — có giờ, có hạn, có người chờ
   («Ba giờ chiều nhận công văn, năm giờ phải trả lời»). Không mở bằng điều luật.
2. **Làm thật:** quay chính phần mềm giải xong việc đó; AI chạy thật, đoạn chờ tua nhanh có nhãn «tua nhanh».
3. **Kết quả cầm về:** tệp Word, bản tóm tắt, sổ văn bản… đặt to trên hình, nói rõ dùng ngay được gì.
4. **Màn kết:** địa chỉ bản web + giá đọc từ prod ngay trước khi viết + «dùng thử miễn phí»
   (bản cài máy vẫn không nói giá — `NOI-DUNG.md` mục 5).

Luật giữ nguyên: xưng «anh chị», không đọc tên VanBanPlus, không chữ Latin trong lời đọc, không hứa «tiết kiệm X giờ»,
mỗi câu soát với mã / prod, ghi ✔ đầu tệp clip. NĐ 30 chỉ được nhắc như chi tiết phụ («ra đúng thể thức»).

## Bảng clip (ý — soát mã / prod trước khi viết lời)

| NN | Việc thật | Quay trên | Nguồn có sẵn |
|---|---|---|---|
| 01 | Nhận công văn lúc 15:00, cần văn bản trả lời trước giờ về → AI soạn → xuất Word | web | `huong-dan-web/02` (ảnh `video/anh/web-1/`) |
| 02 | Văn bản đến bản giấy dài → quét → AI tóm tắt để báo lãnh đạo | web + desktop | `lam-that` 03, 04; quét web đã sửa chiều 80b1cb0 |
| 03 | Trước khi trình ký, AI kiểm lỗi → bắt 4 lỗi cài sẵn, ra bản đã sửa | desktop | `lam-that/02` (quay lại bằng app ≥ 1.0.20) |
| 04 | Họp xong → xuất biên bản cuộc họp ra Word | desktop | `lam-that/05` |
| 05 | Tìm lại văn bản năm ngoái: gõ không dấu, Ctrl+K, ra ngay | web | `huong-dan-web/06` |
| 06 | Sổ văn bản đến tự đánh số, tự cảnh báo quá hạn | desktop | `lam-that/06` |
| 07 | Kế hoạch, báo cáo dài → AI dựng khung từ mẫu có sẵn | web | L5 mẫu `/soan-thao?mau=` |
| 08 | Lịch họp + album ảnh hoạt động lưu một chỗ | web | `huong-dan-web/05` |

**Trạng thái:** 01 dựng PC 22/11/2026, 47,8 s · 02 dựng PC 22/11/2026, 50,5 s · 03 dựng PC 22/11/2026, 48,1 s — chờ duyệt, chưa hẹn đăng.
03: AI có thẻ riêng cho 3/4 lỗi cài (lỗi «Kính gửi» chỉ sửa trong bản đã sửa) → lời không nói «AI bắt 4 lỗi»; bản đã sửa tự thêm ý
không có trong bản gốc → không phóng phần thân; «Xuất Word» của hộp AI Kiểm tra bọc nhầm vào khuôn BÁO CÁO → không quay.
04 dựng PC 23/11/2026 — chờ duyệt. Xuất thử bằng bản 1.0.21 thấy sai thể thức (tên cơ quan dưới Quốc hiệu, không số/ký hiệu,
địa danh «.........», tên người ký rơi sang trang sau, không đặt khổ A4) → đã sửa `MeetingWordExportService` + bộ kiểm
`MeetingWordExportTests`, quay bằng app dựng từ mã đã sửa. **Phải phát hành bản cài mới trước khi đăng 04.** Lời không nói
«đúng thể thức» (lề trang bản xuất chưa theo NĐ 30). Cuộc họp hư cấu «Họp giao ban tháng 11 năm 2026» (UBND xã A) tạo qua giao diện.

**Công cụ quay của loạt (22/11/2026):**
- Web: `node video/cong-cu/quay-web/quay.mjs xem | dang-ky | 01 | 02` — CDP screencast khung 390×844 ×3 → `video/anh/viec-that/web-NN.mp4`
  + `.moc.json` (giây + hộp chạm). `VBP_TK=2` dùng tài khoản `demo-web-clip-2` (tài khoản 1 còn 18 credit tới 18/12).
- Giấy giả lập (cơ quan hư cấu tỉnh B / xã A, không con dấu): `node video/cong-cu/quay-web/giay.mjs`.
- Desktop: app dựng từ mã (`dotnet build … -o <scratchpad>`), ghi nền `quay-app/quay-nen.ps1` (MKV, dừng an toàn) + kịch bản
  `quay-viec-that-02a/b.ps1`. Hộp chọn tệp / hộp xác nhận chỉ lái được bằng `WScript.Shell.AppActivate` + `SendKeys`
  (`quay-app/chon-tep.ps1`). Bản quay dài phải cắt gọn trước khi đưa vào clip (tệp 556 s làm trang dựng ra khung đen).
- Tệp Word → ảnh trang: LibreOffice `--convert-to pdf` rồi PyMuPDF.
- Menu chuột phải / hộp báo tràn ra ngoài vùng ghi khi cửa sổ ở x 0 (màn 2560) → đặt cửa sổ app sát mép phải (`$X = 1120`,
  `quay-viec-that-04.ps1`). Đường dẫn không gõ bằng SendKeys (bộ gõ telex đổi «ys» → «ý») — dán qua bộ nhớ tạm hoặc dùng tên mặc định.
- gdigrab có lúc rớt khung (bản 04: 26 s ghi ra 17,6 s) → luôn dò mốc bằng tờ khung, tua bằng `khoa`.

Cách quay: desktop theo `video/lam-that/KE-HOACH.md` mục «Cách quay»; web theo `video/huong-dan-web/KE-HOACH.md` mục
«Cách chụp» (tài khoản demo web, QR thật luôn làm mờ).
