# App iOS / Android VanBanPlus (thiết kế)

Trạng thái triển khai: kế hoạch `docs/superpowers/plans/2026-11-19-app-di-dong.md`; APK thử gửi duyệt 19/11/2026 (Telegram; quét tài liệu + Google còn chờ thử trên điện thoại thật).

Ngày: 19/11/2026. Trạng thái: chủ dự án duyệt thiết kế trong phiên 19/11; **làm ở nhánh riêng, chưa phát hành, chưa hiện ở đâu cho
người dùng tới khi chủ dự án duyệt bản chạy thử**. Song song: tổ chức thuê — `2026-11-19-dot-b-to-chuc-design.md`.

## 1. Quyết định đã chốt (19/11/2026)

| Chủ đề | Chốt |
| --- | --- |
| Phạm vi đợt đầu | Bọc bản web (Capacitor) + camera quét văn bản; đăng nhập giữ phiên an toàn; nút Back Android. Chưa thông báo đẩy |
| Mua trong app | Ẩn hẳn (tránh In-App Purchase 15–30 %); không link dẫn ra web mua; khách mua trên web |
| Nguồn giao diện | Đóng gói sẵn bản build trong app (không tải trang web từ xa) |
| Duyệt | APK chạy thử + ảnh / video gửi Telegram |

Ngoài phạm vi: thông báo đẩy, ngoại tuyến, widget, phát hành lên kho (chờ tài khoản Apple Developer 99 USD/năm + Google Play
25 USD do chủ dự án tự mở), mua trong app.

## 2. Cấu trúc

- Capacitor (bản ổn định mới nhất lúc làm, ghim số bản) trong repo `vanbanplus-web`: `capacitor.config.ts`, `android/`, `ios/`.
  `appId` dự kiến `com.giakiemso.vanbanplus`, tên «VanBanPlus».
- Biến build `VITE_NEN_TANG=app`; module `src/nen-tang.ts` xuất `laApp()`, `coTheMua()`, `luuTep()`, `moTep()`, `moNgoai()` —
  mọi chỗ khác theo nền tảng đi qua đây (web: hành vi hiện tại, không đổi).
- Router giữ `BrowserRouter` (Capacitor phục vụ ở gốc). Không service worker.
- `X-VBP-Nguon: app` + `X-VBP-Phien-Ban: <bản app>` trên mọi lời gọi từ app.

## 3. Phiên đăng nhập (sửa API trước)

- Cookie `vbp_lam_moi` (`SameSite=Lax`, khác site với `capacitor://localhost`) không đi từ app → API: khi yêu cầu có
  `X-VBP-Nguon: app`, các route đăng nhập / làm mới / Google / 2 lớp trả `token_lam_moi` trong thân và **không** đặt cookie.
  Chiều nhận đã có (`tokenLamMoi()` đọc thân khi không có cookie). Web không gửi header này → không đổi.
- App lưu token làm mới bằng plugin lưu trữ an toàn (Keychain iOS / Keystore Android), gửi trong thân khi làm mới, xóa khi đăng xuất.
- Thêm `X-VBP-Nguon` vào danh sách header CORS nếu chưa có; nguồn `capacitor://localhost`, `https://localhost` đã có.

## 4. Đăng nhập Google

- Google chặn OAuth trong WebView → plugin đăng nhập Google native lấy `id_token` (dùng web client ID làm `serverClientId`).
- API cho nhiều `aud`: `VBP_GOOGLE_CLIENT_ID` nhận danh sách phân cách dấu phẩy (web, iOS, Android).
- Tạo client iOS + Android trong dự án GCP `vanbanplus` (Android cần SHA-1 khóa ký; bản thử dùng khóa debug, bản phát hành thêm sau).

## 5. Chỗ phải thay trong WebView

| Hiện tại (web) | App |
| --- | --- |
| Xuất Word, tải mã dự phòng: blob + `a[download]` | Ghi tệp (plugin Filesystem) → bảng Chia sẻ (plugin Share) |
| PDF trong `iframe` (`van-ban/DanhSachTep.tsx`), `window.open` (`lich-hop/KhoiTep.tsx`), link `_blank` URL ký | Tải về thư mục tạm → mở bằng trình xem của máy |
| Liên kết ngoài (`os-kit/dieu-huong.tsx`, điều khoản, chỉ đường lịch họp…) | Plugin Browser (trình duyệt trong app) |
| Cookie `osk_gd` lưu giao diện | Thử trên iOS; hỏng thì lưu bằng plugin Preferences qua `nen-tang.ts` |

## 6. Camera quét

- Plugin quét tài liệu (ML Kit Document Scanner trên Android, VisionKit trên iOS): tìm mép, cắt góc, nhiều trang, trả JPEG.
- Ở app, nút «Chụp ảnh» của Quét mở trình quét này; ảnh đưa vào `useTepQuet` như tệp chọn tay (luồng ước tính → đọc → soát giữ nguyên).
- Quyền camera: khai báo `NSCameraUsageDescription` (iOS) và quyền Android; từ chối quyền → nhắc chọn ảnh từ máy.

## 7. Ẩn mua

`coTheMua()` = `!laApp()`. Gom ~20 lối vào trang Mua về hàm này (phân hệ «Mua thêm» trong `src/ung-dung.ts`; báo thiếu credit
`ai/ThanhPhanTacVu.tsx`, `ai/thong-bao-loi.ts`; dải chỉ đọc `van-ban/DaiChiDoc.tsx`, `lich-hop/DaiChiDoc.tsx`; `van-ban/BieuMau.tsx`,
`van-ban/TaiLenTep.tsx`, `album/BangTaiLen.tsx`, `mau/loi-mau.tsx`, `tai-khoan/ThanhDungLuong.tsx`, `tai-khoan/TongQuan.tsx`). Ở app:
không link, câu báo trung tính («Hết credit», «Kho đã đầy — đang ở chế độ chỉ đọc»), không nhắc web hay giá. Route `/mua*` ở app
chuyển về trang chủ.

## 8. Build, kiểm, duyệt

- Nhánh `dot-b-app` (web + phần API mục 3–4; phần API chỉ thêm, không đổi hành vi web).
- Android: cài JDK + Android SDK trên PC, build APK debug; thử trên máy ảo Android hoặc máy thật cài tay.
- iOS: cần Mac hoặc build mây (Codemagic / GitHub Actions macOS) — làm khi chủ dự án có tài khoản Apple Developer; đợt này giữ
  cấu hình `ios/` đúng, chưa build ký.
- Kiểm:
  - API `node:test`: `X-VBP-Nguon: app` → token trong thân, không cookie; không header → như cũ; làm mới bằng token trong thân;
    nhiều `aud` Google, `aud` lạ bị từ chối.
  - Web vitest: `coTheMua()` ẩn mọi lối vào ở app, web không đổi; `luuTep` / `moTep` / `moNgoai` theo nền tảng.
  - Máy ảo / máy thật: đăng nhập mật khẩu + Google, tắt mở lại app vẫn còn phiên, quét 3 trang bằng camera, xuất Word chia sẻ,
    mở PDF đính kèm, nút Back, không thấy lối vào Mua. Quay video + ảnh gửi Telegram.
- Phát hành: chỉ khi chủ dự án duyệt + có tài khoản kho ứng dụng.
