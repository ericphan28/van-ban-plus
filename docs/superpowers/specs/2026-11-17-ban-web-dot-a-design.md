# Bản web VanBanPlus — đợt A (cá nhân): thiết kế

Ngày: 17/11/2026. Trạng thái: chờ chủ dự án duyệt.

Spec trước: `2026-11-09-api-tac-vu-ai-credit-design.md` (API tác vụ AI + credit, đã lên production 12–17/11/2026).
Bộ nhớ dự án: «bản web cho cá nhân + tổ chức, lâu dài app iOS/Android» (chốt 08/11/2026).

## 1. Mục tiêu

Ra bản web VanBanPlus cho **cá nhân thuê**, dùng chung tài khoản, gói và credit với bản cài máy, có **kho văn bản trên
cloud tính phí theo dung lượng**. Đợt B (sau) thêm **tổ chức thuê** trên cùng kho dữ liệu.

**Tiêu chí hoàn thành đợt A**

1. Đăng nhập web bằng email + mật khẩu, Google, OTP email; bật được xác thực 2 lớp.
2. Quản lý văn bản đến / đi / nội bộ trên cloud: thư mục, tìm kiếm không dấu, tệp đính kèm, xuất Word.
3. Đủ 6 tác vụ AI v2 trên web, báo trước khoảng credit, xử lý đủ mã lỗi.
4. Mẫu văn bản, tra cứu pháp quy, lịch họp, album ảnh chạy trên web.
5. Dung lượng theo gói + mua thêm; vượt mức thì chỉ đọc, không mất dữ liệu.
6. Giao diện hiện đại kiểu os-kit (như gikidi-truonghoc), dùng tốt trên PC và điện thoại.

**Ngoài phạm vi đợt A:** tổ chức (đợt B); bản cài máy nối kho chung (bản phát hành sau); app iOS/Android; ngoại tuyến;
soạn chung thời gian thực; mã hóa thêm dữ liệu khi lưu.

## 2. Quyết định đã chốt (17/11/2026)

| Vấn đề | Quyết định |
| --- | --- |
| Thứ tự | Cá nhân (đợt A) trước, tổ chức (đợt B) sau; kho có chỗ cho chủ sở hữu tổ chức ngay từ đầu |
| Phí lưu trữ | Gói tháng kèm dung lượng + gói dung lượng mua thêm theo tháng |
| Web ↔ bản cài máy | Một kho chung trên cloud; bản cài máy nối vào ở bản phát hành sau |
| Phạm vi đợt A | Văn bản + 6 tác vụ AI, mẫu văn bản, tra cứu pháp quy, lịch họp + album ảnh |
| Nơi đặt giao diện | Repo riêng `vanbanplus-web`, **React + Vite + TypeScript**, chỉ gọi API |
| Đăng nhập | Email + mật khẩu, Google, OTP email, xác thực 2 lớp TOTP |
| Dung lượng kèm gói | Free 200 MB · Starter 2 GB · Pro 10 GB · Business 50 GB |
| Mua thêm | +10 GB 29.000đ/tháng · +50 GB 99.000đ/tháng (bảng giá chỉnh được) |
| Vượt mức | Chỉ đọc, giữ dữ liệu, nhắc email; không tự xóa |
| Bảo vệ dữ liệu | Không mã hóa thêm; quản trị không có chức năng xem văn bản khách |
| Kiến trúc | API tập trung (`vanbanplus-api`), tệp trong Supabase Storage có sẵn |
| Gửi email | Gmail API (hộp thư công ty) |
| Giao diện | os-kit, sửa kit để chạy ngoài Next.js qua lớp nối bộ định tuyến |

## 3. Kiến trúc

```
vanbanplus-web (React + Vite, app.vanbanplus.giakiemso.com, Coolify tĩnh) ──┐
App iOS/Android (Capacitor, sau) ──────────────────────────────────────────┤  Bearer token phiên
Bản cài máy (X-API-Key) ────────────────────────────────────────────────────┤
                                                                             ▼
                vanbanplus-api (Next.js) — /api/v2/phien, /api/v2/kho, /api/v2/tac-vu, /api/v2/credit …
                    │  luật: quyền, hạn mức, phí, credit
                    ├── Postgres VanBanPlus (supabase-db-q323…) — bảng kho_*, phien, …
                    └── Supabase Storage (bucket kho) — tệp; tải lên / tải về qua đường ký có hạn
```

- Web không có luật nghiệp vụ. Mọi kiểm quyền, hạn mức, phí ở API.
- Hàm `xacThuc` nhận **Bearer token phiên** (web/app) hoặc **X-API-Key** (bản cài máy) → route v2 hiện có dùng được
  ngay từ web.
- CORS: chỉ `https://app.vanbanplus.giakiemso.com` và nguồn Capacitor (`capacitor://localhost`, `https://localhost`).
- Đường cũ `/api/ai/*`, `/api/auth/*` giữ nguyên cho bản cài máy.

## 4. Đăng nhập và phiên

**Phiên**

- Token truy cập: JWT HS256 15 phút, khóa `VBP_PHIEN_KHOA` (biến Coolify chỉ lúc chạy, bản gốc ở `~/.gikidi`).
- Token làm mới: 256 bit ngẫu nhiên, 30 ngày, cookie `httpOnly; Secure; SameSite=Lax` trên miền API; CSDL chỉ giữ băm
  SHA-256. Mỗi lần làm mới **xoay token**; token cũ bị dùng lại → thu hồi cả chuỗi phiên (`phien.chuoi_id`).
- Bảng `phien(id, user_id, chuoi_id, bam_token, thiet_bi, ip, tao_luc, dung_luc, het_han, thu_hoi_luc)`.
- Web và API cùng miền gốc `giakiemso.com` nên cookie `SameSite=Lax` đi kèm yêu cầu `credentials: "include"`. App di
  động (sau) không dùng cookie: token làm mới giữ trong kho bảo mật của máy, gửi trong thân yêu cầu làm mới.
- Trang tài khoản: danh sách thiết bị đang đăng nhập, thu hồi từng cái / tất cả.

**Cách đăng nhập** (đều ra cùng một kiểu phiên)

1. **Email + mật khẩu** — bảng `users` + bcrypt đang có; mật khẩu mới tối thiểu 8 ký tự; sai 5 lần → khóa 15 phút
   (theo email và theo IP).
2. **Google** — Google Identity Services trên web; máy chủ kiểm ID token (khóa công khai Google, `aud` = client id,
   `email_verified`). Email trùng tài khoản có sẵn → gắn vào tài khoản đó (`dang_nhap_ngoai(nha_cung_cap, sub, user_id)`);
   email mới → tạo tài khoản gói Free. Cần OAuth client trên Google Cloud.
3. **OTP email** — mã 6 số, 10 phút, sai quá 5 lần thì hủy; tối đa 3 mã / 15 phút / email; bảng `otp_email` giữ băm.
   Thay luôn «quên mật khẩu» kiểu cũ (phải hỏi Admin).
4. **Xác thực 2 lớp TOTP** — bật ở tài khoản (QR + 10 mã dự phòng dùng một lần); bật rồi thì mọi cách trên phải qua
   bước mã, nối bằng mã tạm 5 phút. Bí mật TOTP mã hóa AES-256-GCM (cùng module `luu-ho-tro/ma-hoa`, khóa riêng
   `VBP_TOTP_KHOA`).

**Gửi email:** Gmail API từ hộp thư công ty (giới hạn khoảng 500/ngày Gmail, 2.000/ngày Workspace). Module gửi email là
một giao diện nhỏ để đổi sang dịch vụ khác khi lượng gửi vượt.

**Bản cài máy:** giữ X-API-Key; chuyển sang phiên (để dùng chung 2 lớp) là bản phát hành riêng.

## 5. Kho dữ liệu

**Chủ sở hữu:** mọi bản ghi kho có `chu_loai` (`ca_nhan` | `to_chuc`) + `chu_id`; đợt A chỉ `ca_nhan` (= `users.id`).
Kèm `tao_boi`, `tao_luc`, `sua_luc`, `phien_ban` (tăng mỗi lần sửa — để bản cài máy đồng bộ và phát hiện sửa chồng).

**Bảng** (thay các bảng `cloud_*` của 001 — đang trống trên prod, bỏ trong migration kho):

| Bảng | Nội dung |
| --- | --- |
| `kho_thu_muc` | Cây thư mục (`cha_id`); gốc theo hướng Đến / Đi / Nội bộ |
| `kho_van_ban` | Trường của `Document` C#: số, ký hiệu, ngày ban hành, cơ quan, trích yếu, loại, hướng, độ khẩn, độ mật, số đến, ngày đến, hạn xử lý, giao cho, căn cứ, nơi nhận, người ký, chức danh, nội dung, tóm tắt, thẻ, văn bản liên quan, bản sao, gắn sao, ghi chú riêng; xóa mềm (`xoa_luc`, thùng rác 30 ngày) |
| `kho_tep` | Tệp gắn văn bản / cuộc họp / album: đường dẫn Storage, tên, mime, byte, băm SHA-256, số trang, ảnh thu nhỏ |
| `kho_cuoc_hop` | Cuộc họp: thời gian, địa điểm, chủ trì, thư ký; thành phần + nhiệm vụ (JSONB); văn bản giấy mời / biên bản / kết luận |
| `kho_album`, `kho_anh` | Album theo sự kiện; ảnh là `kho_tep` loại ảnh + kích thước, ngày chụp |
| `kho_mau` | Mẫu văn bản riêng của chủ sở hữu |
| `mau_chung`, `phap_quy` | Dữ liệu chung chỉ đọc: 62 mẫu sẵn + cửa hàng mẫu; thư viện pháp quy — nạp từ dữ liệu của bản cài máy |
| `kho_dung_luong` | `(chu_loai, chu_id, byte_da_dung)` cập nhật trong cùng giao dịch thêm / xóa tệp. Chỉ tính byte tệp trong Storage; chữ của bản ghi không tính. Tệp trong thùng rác vẫn tính đến khi xóa vĩnh viễn |

**Tìm kiếm:** Postgres `tsvector` trên trích yếu + số + cơ quan + nội dung, cấu hình `simple` + `unaccent` (gõ không dấu
vẫn ra); lọc theo loại, hướng, khoảng ngày, thư mục, thẻ.

**Tải tệp lên (hai bước):**

1. `POST /api/v2/kho/tep/xin-tai-len {ten, mime, byte}` → kiểm hạn mức + mime + ≤ 50 MB/tệp → ghi hàng chờ, trả đường ký
   tải lên Storage (10 phút).
2. `POST /api/v2/kho/tep/xac-nhan {id}` → API đọc kích thước thật trong Storage, cộng `byte_da_dung`; sai khai báo hoặc
   vượt mức → xóa tệp. Hàng chờ quá 1 giờ chưa xác nhận được dọn (cả tệp nếu đã lên Storage) bằng pg_cron hằng ngày.

Tải về: đường ký 10 phút; Storage trả `Content-Disposition: attachment` trừ PDF và ảnh (xem tại chỗ).

**Quyền:** RLS bật, không policy, chỉ `service_role` (như các bảng credit); mọi kiểm quyền ở API. Truy cập bản ghi không
thuộc mình → 404.

## 6. Gói và phí lưu trữ

- **Dung lượng kèm gói:** dùng cột có sẵn `subscription_plans.cloud_storage_mb`: free 200, starter 2048, pro 10240,
  business 51200. Mô tả gói trên prod (đang ghi «500MB cloud…») đổi đúng cụm dung lượng như cách migration 004 làm.
- **Gói dung lượng mua thêm:** dòng bảng giá `loai = 'goi_dung_luong'`: `dl-10` {them_mb 10240, gia_vnd_thang 29000},
  `dl-50` {them_mb 51200, gia_vnd_thang 99000}; mua 1–12 tháng, giá = giá tháng × số tháng.
- **Đơn hàng** thêm `loai = 'dung_luong'` (+ `goi_dung_luong_ma`, `so_thang`); `pay_order` thêm nhánh ghi
  `kho_dung_luong_mua(chu_loai, chu_id, them_mb, bat_dau, het_han, order_id)` — gọi lại không ghi đôi (unique
  `order_id`); mua tiếp cùng mã khi còn hạn thì nối từ ngày hết hạn cũ. Thanh toán VietQR + webhook SePay + xác nhận tay
  như hiện nay; `/thanh-toan?dung-luong=dl-10&thang=3`.
- **Hạn mức hiệu lực** = dung lượng gói hiệu lực (gói trả phí hết hạn rơi về Free theo `effectivePlanId`) + tổng gói
  dung lượng còn hạn.
- **Chỉ đọc khi vượt:** chặn tải tệp lên và tạo bản ghi mới (mã lỗi `HET_DUNG_LUONG`, HTTP 403); vẫn xem, tìm, tải về,
  xóa, sửa thông tin bản ghi có sẵn, chạy tác vụ AI không lưu kết quả thành bản ghi mới. Dải cảnh báo đỏ trên web kèm nút
  «Mua thêm dung lượng».
- **Nhắc email:** `pg_cron` hằng ngày gọi đường nội bộ API qua `pg_net` (khóa bí mật); API gửi: gói / gói dung lượng còn
  7 ngày, vừa vào chỉ đọc, còn chỉ đọc sau 30 ngày. Bảng `nhac_email` chống gửi lặp.

## 7. Giao diện web

**Nền:** React 18 + Vite + TypeScript + React Router; os-kit được sửa để không phụ thuộc Next.js (lớp nối: thành phần
Link + hàm đọc đường dẫn truyền vào; Next dùng lớp nối mặc định nên gikidi-truonghoc / gikidi-giaoduc không đổi gì). Sửa
kit theo `os-kit/CLAUDE.md` (ghi `THAY-DOI.md`, soát các project đang dùng).

**Màn hình chính (Launchpad):** lưới phân hệ — Văn bản, Soạn thảo AI, Nhập từ ảnh/PDF, Kiểm tra, Tóm tắt, Tham mưu, Báo
cáo định kỳ, Mẫu văn bản, Tra cứu pháp quy, Lịch họp, Album ảnh, Thùng rác, Tài khoản, Cài đặt. Cột thẻ: số dư credit +
thanh dung lượng; văn bản sắp đến hạn; lịch họp hôm nay; văn bản mới; nút nhanh «Soạn văn bản», «Quét văn bản». Tìm toàn
cục `Ctrl+K` (không dấu) qua văn bản, cuộc họp, mẫu, pháp quy. Điện thoại: màn hình chính lưới icon + thanh tab kính mờ.

**Văn bản:** PC ba cột (thư mục | danh sách | chi tiết); điện thoại danh sách → trượt sang chi tiết, vuốt để gắn sao /
xóa. Lọc bằng thẻ chọn nhanh (Đến / Đi / Nội bộ, loại, khoảng ngày, độ khẩn, thẻ). Chi tiết: thông tin, xem tệp tại chỗ
(PDF, ảnh), ngăn «AI» (Tóm tắt, Kiểm tra, Tham mưu) hiện kết quả bên cạnh.

**Tạo văn bản:** nhập tay, hoặc chụp / tải ảnh-PDF (điện thoại mở máy ảnh) → `trich-xuat` điền sẵn biểu mẫu → soát → lưu,
tệp gốc tự đính kèm.

**Soạn thảo AI:** chọn loại / mẫu → điền trường → kết quả trong trình soạn thảo chữ có định dạng → «Lưu thành văn bản đi»
hoặc «Xuất Word» đúng thể thức NĐ 30 (tạo tệp .docx ngay trên trình duyệt). Địa phương lấy từ Cài đặt → Cơ quan (lưu ở
máy chủ theo tài khoản).

**Credit trên mọi tác vụ AI:** nút ghi sẵn khoảng («Tóm tắt · 4–13 credit», lấy từ `/uoc-tinh`); chạy có thanh tiến trình
đếm giây + hủy; xong báo credit trừ thật + số dư; lỗi hiện đúng thông báo theo mã (hết credit có nút «Mua thêm»).

**Lịch họp:** xem tuần / tháng; cuộc họp có thành phần, nhiệm vụ, văn bản gắn kèm. **Album:** lưới ảnh, tải nhiều ảnh từ
điện thoại.

**Chất lượng:** chữ có dấu, không đại từ nhân xưng; chế độ tối; dải báo mất mạng; mỗi phần giao xong chụp màn hình
Playwright ở 1440 px và 390 px để soát trước khi đẩy.

## 8. Bảo mật

- Kiểm chủ sở hữu ở mọi route kho; trả 404 cho bản ghi không thuộc mình.
- Đường ký Storage 10 phút, gắn một tệp. Quản trị không có chức năng xem văn bản khách.
- Giới hạn tần suất đăng nhập, OTP, Google (theo IP + email).
- Nhật ký bảo mật: đăng nhập, đổi mật khẩu, bật / tắt 2 lớp, thu hồi phiên, xóa vĩnh viễn — không chứa nội dung văn bản.
- Tệp: danh sách mime cho phép, ≤ 50 MB/tệp, tên tệp làm sạch.
- Khóa mới (`VBP_PHIEN_KHOA`, `VBP_TOTP_KHOA`, OAuth Google, Gmail API) chỉ ở Coolify (lúc chạy) và `~/.gikidi`; không in,
  không commit.

## 9. Sao lưu và vận hành

- Bảng mới trong `supabase-db-*` → đã thuộc đường `sao-luu`.
- **Tệp Storage của VanBanPlus:** bước đầu kế hoạch kho kiểm thư mục volume Storage đã nằm trong `sao-luu` chưa; chưa thì
  thêm (luật máy chủ: thêm nơi lưu dữ liệu → tự kiểm đã vào sao lưu).
- Cảnh báo khi Storage VanBanPlus vượt 200 GB (máy 1 TB theo hợp đồng).

## 10. Kiểm thử

- **API:** bài kiểm CSDL thử (phiên, xoay token, thu hồi; kho, dung lượng, chỉ đọc; đơn dung lượng, gọi lại không ghi đôi,
  nối hạn) + kiểm đơn vị hàm thuần (JWT, TOTP, băm, tính giá, tìm kiếm không dấu).
- **Web:** Vitest cho logic; Playwright các luồng: đăng nhập (4 cách) → quét ảnh tạo văn bản → soạn AI → xuất Word → tìm
  kiếm → vượt mức chỉ đọc; chụp màn hình PC + điện thoại.

## 11. Triển khai

Đẩy `main` = production (cả hai repo). Migrate CSDL trước khi đẩy mã cần lược đồ mới; thử migration trên bản sao prod
trước. Thứ tự, mỗi bước một kế hoạch, lên production độc lập:

1. Phiên + đăng nhập (API) + khung `vanbanplus-web` (os-kit, đăng nhập, tài khoản) + DNS `app.vanbanplus.giakiemso.com`.
2. Kho dữ liệu + dung lượng + phí (API) + trang tài khoản / bảng giá.
3. Web văn bản + 6 tác vụ AI + xuất Word.
4. Mẫu văn bản + tra cứu pháp quy.
5. Lịch họp + album ảnh.

Bản web chỉ **mở bán** sau bước 5 và sau các điều kiện còn treo của spec trước §13 (Vertex trả phí trước 18/12/2026;
quyết định với giới hạn 100 giây của Cloudflare cho tác vụ dài).

## 12. Rủi ro

| Rủi ro | Cách giảm |
| --- | --- |
| Gmail API giới hạn số email/ngày | Module gửi email đổi được; theo dõi số gửi; đổi dịch vụ khi gần trần |
| Dung lượng máy chủ 1 TB | Cảnh báo 200 GB; xem lại giá dung lượng khi gần mức |
| Sửa os-kit làm hỏng project đang dùng | Lớp nối mặc định giữ hành vi Next; soát gikidi-truonghoc / giaoduc sau khi cài lại |
| Tệp Storage chưa vào sao lưu | Kiểm và thêm ở bước 2 trước khi nhận tệp thật |
| Bản cài máy và web sửa chồng một văn bản | `phien_ban` trên mọi bản ghi; luật gộp bàn ở bản cài máy nối kho |
| Phiên khác đang sửa cùng `vanbanplus-api` | Kiểm `git status` / `git log` trước mỗi đợt; nhánh riêng |
