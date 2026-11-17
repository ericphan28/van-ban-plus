# Bản web VanBanPlus — việc còn lại tới khi mở bán

Ngày 17/11/2026. Đọc cùng spec `docs/superpowers/specs/2026-11-17-ban-web-dot-a-design.md` và hợp đồng
`docs/superpowers/plans/2026-11-17-hop-dong-web-dot-a.md`. Tệp này là **danh sách việc chung** của bản web: phiên nào làm
xong một mục thì đánh dấu `[x]`, ghi ngày + commit ở cuối dòng, commit riêng tệp này. Không xóa mục; mục bỏ thì gạch và ghi
lý do.

## 1. Trạng thái

| Luồng | Việc | Trạng thái |
| --- | --- | --- |
| L1 | API phiên + đăng nhập | Prod 17/11 |
| L2 | os-kit lớp nối + khung `vanbanplus-web` (`https://app.vanbanplus.giakiemso.com`) | Prod 17/11 — còn mục 2.3 |
| L3 | API kho + dung lượng + phí | Prod 17/11 |
| L4 | Web văn bản, quét, 6 tác vụ AI, xuất Word | Đợt 2 — chưa bắt đầu |
| L5 | Mẫu văn bản + tra cứu pháp quy | Prod 17/11 (API `54cda8e` + migration 012, web `205ff6b`) |
| L6 | Lịch họp + album ảnh | Đợt 2 — chưa bắt đầu |

L4, L5, L6 chạy song song được (cả ba cùng sửa `vanbanplus-web`: worktree riêng, chỉ sửa dòng phân hệ của mình trong
`src/ung-dung.ts`, rebase lên `origin/main` + chạy lại kiểm trước khi đẩy; nguồn Storage trong CSP `public/serve.json` do L4
thêm, L6 dùng chung).

**Xong L4–L6 = đủ chức năng đợt A, CHƯA mở bán.** Mở bán khi mục 2–4 dưới đây xong hết.

## 2. Chờ chủ dự án (làm được ngay, không phải chờ L4–L6)

- [ ] 2.1 **Google OAuth Client ID** (loại Web; nguồn JS `https://app.vanbanplus.giakiemso.com`, `http://localhost:5173`).
  Có rồi: đặt biến build `VITE_GOOGLE_CLIENT_ID` cho app Coolify `csywi7ct…` (vanbanplus-web) + biến tương ứng phía API
  (L1), triển khai lại. Chưa có → nút Google ẩn.
- [ ] 2.2 **Hộp thư Gmail gửi email** + cấp quyền Gmail API (`VBP_GMAIL_*`, hợp đồng mục 4). Chưa có → đăng ký, đăng nhập
  bằng mã email, quên mật khẩu, nhắc gia hạn / vượt dung lượng đều chưa chạy thật (API ghi log thay vì gửi).
- [ ] 2.3 **Tài khoản thử cho kiểm production**: `~/.gikidi/vanbanplus-demo-clip.txt` có mật khẩu cũ (cả route cũ lẫn v2 trả
  `SAI_DANG_NHAP`). Cần mật khẩu đúng hoặc đồng ý đặt lại. Có rồi: chạy `node .tam/kiem-prod.mjs` trong `vanbanplus-web`
  (đăng nhập → tải lại giữ phiên → hai tab → thiết bị → đăng xuất, chụp `.tam/chup-prod/`) — bước 6 Task 11 kế hoạch L2.
  Đừng thử quá 5 lần sai (khóa 15 phút).

## 3. Điều kiện mở bán (spec §11, spec 09/11 §13)

- [ ] 3.1 **Đường trả phí thật cho Vertex AI trước 18/12/2026** — khóa hệ thống đang chạy bằng credit dùng thử, hết hạn là 6
  tác vụ AI dừng (bộ nhớ `vanbanplus-khoa-gemini`).
- [ ] 3.2 **Giới hạn 100 giây của Cloudflare** cho tác vụ AI dài (soạn thảo, báo cáo định kỳ) — chốt cách xử lý (chạy nền +
  hỏi kết quả, hoặc tên miền API riêng không qua proxy) trước khi bán.
- [ ] 3.3 **Việc treo của đợt credit**: theo dõi `tac_vu_chay.ma_loi` bản cài 1.0.20; hệ số ước tính `bao-cao-dinh-ky` hơi cao
  (trừ thật 5 < min 7) — bộ nhớ `vanbanplus-credit-dot-1`.

## 4. Chưa luồng nào nhận — phiên «Mở bán bản web» làm sau L4–L6

Đề xuất mở một phiên riêng gom mục 4 + 5 khi L4, L5, L6 đã lên `main`.

- [ ] 4.1 **Kiểm Playwright xuyên luồng** (spec §10): một mạch đăng nhập (4 cách) → quét ảnh tạo văn bản → soạn AI → xuất
  Word → tìm kiếm không dấu → vượt dung lượng chỉ đọc; chụp PC 1440 + điện thoại 390. Mỗi luồng chỉ kiểm phần của mình.
- [ ] 4.2 **Cảnh báo Storage VanBanPlus vượt 200 GB** (spec §9; máy chủ 1 TB theo hợp đồng Megacore) — gắn vào giám sát
  máy chủ (repo `D:/Thang/gikidi-server`), báo Telegram.
- [ ] 4.3 **Thanh toán ngay trong web**: trang mua credit / gói / dung lượng còn ở site API (`/bang-gia`, `/thanh-toan`) dùng
  đăng nhập kiểu cũ → người dùng web phải đăng nhập lại. Cần trang mua trong web dùng phiên Bearer (`/api/orders*` đã nhận
  Bearer từ L3) hoặc chuyển phiên sang site API.
- [ ] 4.4 **Điều khoản + Chính sách bảo mật**: thêm lưu văn bản trên cloud, phí dung lượng, chế độ chỉ đọc khi vượt, không
  mã hóa thêm, quản trị không xem văn bản khách (spec §2, §8).
- [ ] 4.5 Rà tiêu chí hoàn thành đợt A (spec §1, 6 mục) trên production, ghi kết quả vào tệp này.

## 5. Sửa còn tồn

- [x] 5.1 (L1, API) Đặt lại mật khẩu trên tài khoản bật 2 lớp: lần gửi đầu chưa có `ma_2_lop` vẫn tính vào trần đoán mã 2 lớp
  và ghi `hai_lop_sai`. **Xong 17/11/2026, API `a027cbf`:** thiếu `ma_2_lop` → `CAN_MA_2_LOP` không tính lượt, không ghi
  nhật ký; mã email vẫn phải đúng trước (không lộ trạng thái 2 lớp). Mã email vẫn bị dùng → web tự gửi mã mới như cũ.
- [x] 5.2 (L1, API) Mất phản hồi `lam-moi` giữa chừng → web đăng xuất / thu hồi phiên. **Xong 17/11/2026, migration
  `014_phien_cap_lai` (prod, trong giao dịch) + API `a027cbf`:** token vừa dùng < 30 giây mà chưa có token mới hơn được dùng thì
  cấp lại (tối đa 3 lần, nhật ký `token_cap_lai`); xoay bình thường gom các nhánh còn lại; > 30 giây vẫn thu hồi. Còn tồn
  (nhỏ, hiếm): trần 3 lần có thể vượt khi tải đồng thời vì `now()` là giờ bắt đầu giao dịch (dùng `clock_timestamp()`); đoạn
  hoàn tác nên `LOCK TABLE phien_token` trước. Web (`khach.ts`): sau `MAT_MANG` lúc làm mới nên thử lại ngay một lần để kịp
  cửa sổ 30 giây — việc của luồng web kế tiếp.
- [x] 5.3 (L1, API) Đăng nhập sai mật khẩu mất ~7 giây. **Đóng 17/11/2026, không tái hiện:** đo lại email thật và email giả
  đều ~1 giây (bằng nhau, không dò được email); 5 hash trong `users` đều bcrypt cost 10. Lần 7 giây là chậm nhất thời.
- [ ] 5.4 (L2 → L4 dọn khi đụng tới, ghi ở sổ SDD L2) — `vanbanplus-web`:
  - cờ tự tải lại `?lam-moi=1` hết hạn theo đồng hồ 10 giây thay vì khi trang tải thành công (`src/vo/lam-moi.ts`);
  - React Router không thấy việc gỡ cờ (`history.replaceState` trực tiếp);
  - lóe «Có lỗi khi hiển thị trang» trước khi tự tải lại;
  - timeout 15 giây áp cả lệnh ghi dữ liệu (`src/api/khach.ts`) — POST có thể đã chạy xong ở máy chủ mà web báo mất mạng;
  - nút `LoiTai` lệch trái; thông báo xanh tương phản 3,1:1 (dưới AA);
  - `api.dangXuat` lỗi máy chủ thì cookie làm mới còn sống (máy dùng chung) — sửa trước khi bán: giữ token khi đăng xuất lỗi
    hoặc cảnh báo ở màn đăng nhập.
- [ ] 5.5 (os-kit) Chế độ tối bên trong cửa sổ phân hệ (kit chưa có — web đang giữ nền sáng trong cửa sổ); thanh tab điện thoại
  cứng 5 cột (web đang đè tạm trong `goc.css`). Sửa ở `D:/Thang/os-kit` theo luật kit, rồi cài lại.
- [ ] 5.6 (os-kit) Chạy `next build` cho gikidi-truong qua `xay-thu` lần cài os-kit ≥ 0.4.0 kế tiếp (L2 mới kiểm `tsc` +
  `test:giao-dien`).

## 6. Sau đợt A (đã chốt làm sau, không chặn mở bán)

- Đợt B: tổ chức thuê trên cùng kho (`chu_loai = 'to_chuc'`).
- Bản cài máy nối kho chung (đồng bộ theo `phien_ban`, luật gộp khi sửa chồng).
- App iOS / Android (Capacitor): API trả `token_lam_moi` trong thân, CORS `capacitor://localhost` đã có.

## 7. Sổ cập nhật

| Ngày | Phiên | Đổi gì |
| --- | --- | --- |
| 17/11/2026 | L2 | Bản đầu |
| 17/11/2026 | L2 (sửa L1) | 5.1, 5.2 xong (014 prod + API `a027cbf`); 5.3 đóng — không tái hiện |
