# Đợt B — Tổ chức thuê VanBanPlus (thiết kế)

Ngày: 19/11/2026. Trạng thái: chủ dự án duyệt thiết kế trong phiên 19/11; **làm ở nhánh riêng, chưa lên prod, chưa hiện trên
giao diện cho tới khi chủ dự án duyệt bản chạy thử** (duyệt qua ảnh chụp Playwright + video gửi Telegram, không dựng môi trường thử).

Nền: spec đợt A `2026-11-17-ban-web-dot-a-design.md` (kho có `chu_loai`/`chu_id` từ đầu), hợp đồng `2026-11-17-hop-dong-web-dot-a.md`.
Song song: app iOS/Android — `2026-11-19-app-di-dong-design.md`.

## 1. Quyết định đã chốt (19/11/2026)

| Chủ đề | Chốt |
| --- | --- |
| Kiểu dùng chung | Sổ văn thư + giao việc: một kho chung của cơ quan, vai trò Quản trị / Văn thư / Cán bộ; mỗi người vẫn giữ kho cá nhân |
| Thành viên | Mời qua email; một tài khoản thuộc được nhiều tổ chức; giao diện chuyển ngữ cảnh «Cá nhân / Tổ chức X» |
| Giao việc | Chủ trì + phối hợp, hạn xử lý, ý kiến chỉ đạo, trạng thái chờ / đang / xong, ghi chú, nhật ký, «Việc của tôi». Chưa có luồng duyệt văn bản đi, chưa ký số |
| Tiền | Gói theo cỡ, trả năm; quản trị mua bằng chuyển khoản SePay; HĐĐT chủ dự án xuất tay qua skill PA (đợt này) |
| Ví credit | Phương án A: tổng quát hóa sổ credit theo ví (`vi_loai`, `vi_id`), `user_id` giữ là người chạy |
| Duyệt | Ảnh chụp + video; không môi trường thử trên máy chủ |

Ngoài phạm vi: luồng trình ký / duyệt văn bản đi, ký số, thông báo đẩy, HĐĐT tự động, liên thông văn bản giữa các tổ chức,
tổ chức cha – con (phòng ban).

## 2. Dữ liệu

Mọi migration **chỉ thêm** (bảng mới; cột mới có mặc định = hành vi cá nhân hiện nay), chạy lại được, theo luật của hợp đồng đợt A
(REVOKE bảng khỏi mọi role, chỉ hàm `SECURITY DEFINER` cấp cho `service_role`, `SET lock_timeout = '5s'`). Số dự kiến 017–020.

### 2.1 Tổ chức + thành viên + lời mời

- `to_chuc(id, ten, mst, dia_chi, email_hoa_don, goi_ma, goi_bat_dau, goi_het_han, tao_boi, tao_luc, sua_luc, phien_ban)`.
- `to_chuc_thanh_vien(to_chuc_id, user_id, vai_tro CHECK IN ('quan_tri','van_thu','can_bo'), trang_thai CHECK IN ('hoat_dong','da_roi'), them_luc, them_boi)`,
  PK `(to_chuc_id, user_id)`. Ràng buộc trong hàm: tổ chức luôn còn ≥ 1 quản trị hoạt động; quản trị cuối không tự rời / tự hạ vai trò.
- `to_chuc_loi_moi(id, to_chuc_id, email_thuong, vai_tro, ma_bam, het_han (7 ngày), moi_boi, tao_luc, dung_luc, dung_boi, huy_luc)`.
  Mã mời 32 byte ngẫu nhiên, chỉ lưu SHA-256. Nhận lời mời: đăng nhập đúng email được mời (so chữ thường), mã còn hạn, chưa dùng,
  tổ chức chưa đủ số thành viên tối đa của gói.
- `to_chuc_co_quan`: hồ sơ Cơ quan của tổ chức (cùng dạng `nguoi_dung_co_quan`: tên cơ quan, xã, tỉnh, đặc điểm, người ký, chức danh).
  Ở ngữ cảnh tổ chức, soạn thảo / Quét / AI dùng hồ sơ này.

### 2.2 Giao việc

- `kho_giao_viec(id, chu_loai='to_chuc', chu_id, van_ban_id, nguoi_nhan, vai CHECK IN ('chu_tri','phoi_hop'), han_xu_ly DATE NULL,
  y_kien_chi_dao TEXT, trang_thai CHECK IN ('cho','dang','xong'), ghi_chu TEXT, giao_boi, giao_luc, cap_nhat_luc, phien_ban)`.
  Mỗi văn bản tối đa một chủ trì đang hoạt động; người nhận phải là thành viên hoạt động.
- `kho_giao_viec_nhat_ky(id, giao_viec_id, nguoi, hanh_dong, truoc JSONB, sau JSONB, luc)` — chỉ ghi thêm (không hàm sửa / xóa).
- Ô `kho_van_ban.giao_cho` (chữ tự do) giữ nguyên cho cá nhân; ở tổ chức giao diện dùng `kho_giao_viec`.
- `kho_van_ban` thêm `cong_khai_noi_bo BOOLEAN DEFAULT false`.

### 2.3 Ví credit (phương án A)

- `credit_lo`, `credit_so`, `tac_vu_chay` thêm `vi_loai TEXT DEFAULT 'ca_nhan' CHECK IN ('ca_nhan','to_chuc')`, `vi_id UUID`
  (điền `vi_id = user_id` cho dòng cũ trong cùng migration; chỉ mục mới theo `(vi_loai, vi_id)`).
- Hàm credit (`tac_vu_mo`, `credit_chot`, `credit_hoan`, `credit_cap_ky`, `credit_so_du`, `credit_cap_nap`, `credit_dieu_chinh`) có
  bản mới nhận ví; bản cũ giữ chữ ký, gọi bản mới với ví cá nhân → bản cài máy 1.0.20+ và web hiện tại không đổi hành vi.
- Khóa dòng: ví cá nhân khóa dòng `users` như hiện nay; ví tổ chức khóa dòng `to_chuc`.
- `user_id` trên `tac_vu_chay` / `credit_so` vẫn là người bấm chạy (truy vết, giới hạn tốc độ theo người).

### 2.4 Gói tổ chức + đơn

- `goi_to_chuc(ma, ten, so_thanh_vien_toi_da, credit_thang, dung_luong_mb, max_file_size_mb, gia_nam_vnd, dang_ban, thu_tu)`.
  Dữ liệu mẫu ba bậc 10 / 30 / 100 thành viên, `gia_nam_vnd = NULL`, `dang_ban = false` — chủ dự án chốt giá rồi mới bật.
- `orders` thêm `to_chuc_id UUID NULL`, `loai` thêm `goi_to_chuc`; `nap` và `dung_luong` có `to_chuc_id` thì cấp cho ví / kho tổ chức.
  Chỉ quản trị tạo đơn cho tổ chức. `pay_order` thêm nhánh (webhook SePay + admin dùng chung).
- `kho_han_muc`: với `to_chuc` cộng `goi_to_chuc.dung_luong_mb` khi gói còn hạn + `kho_dung_luong_mua` của tổ chức.
- Hết hạn gói: kho tổ chức chỉ đọc (dùng cơ chế chỉ đọc hiện có), không chạy AI bằng ví tổ chức, không xóa dữ liệu.
  Nhắc gia hạn email cho quản trị: mở rộng `kho_nhac_can_gui` (làm sau nếu kịp; không chặn duyệt).

## 3. Quyền

Ngữ cảnh: header `X-VBP-To-Chuc: <uuid>` (thêm vào danh sách CORS `DAU` ở `vanbanplus-api/src/lib/phien/cors.ts`).
`canNguoiDung` (`src/lib/kho/tra-loi.ts`) đọc header → hàm SQL `to_chuc_vai_tro(p_to_chuc, p_user)` → trả
`{ user, chu: { chu_loai: 'to_chuc', chu_id }, vai_tro }`; không phải thành viên hoạt động → 403 `KHONG_THUOC_TO_CHUC`.
Không header → cá nhân như hiện nay. Route credit / tác vụ AI / đơn hàng cũng nhận header này để chọn ví.

| Việc | Quản trị | Văn thư | Cán bộ |
| --- | --- | --- | --- |
| Hồ sơ tổ chức, thành viên, lời mời, gói, mua | ✔ | – | – |
| Tạo / sửa / xóa văn bản, thư mục, tệp | ✔ | ✔ | tạo được; sửa / xóa văn bản do mình tạo |
| Giao việc, sửa giao việc | ✔ | ✔ | – |
| Xem văn bản | tất cả | tất cả | do mình tạo + được giao (chủ trì / phối hợp) + `cong_khai_noi_bo` |
| Cập nhật trạng thái + ghi chú việc | ✔ | ✔ | việc của mình |
| Lịch họp, album, mẫu riêng | sửa | sửa | xem |
| Chạy AI (ví tổ chức) | ✔ | ✔ | ✔ trên văn bản xem được |

**Lọc quyền nằm trong hàm SQL**, không chỉ ở API: các hàm `kho_van_ban_*`, `kho_tep_*` (tệp gắn văn bản), `kho_giao_viec_*` nhận thêm
`p_nguoi UUID, p_vai_tro TEXT` (mặc định NULL = chủ toàn quyền, đúng hành vi cá nhân) và tự áp luật bảng trên khi `p_vai_tro = 'can_bo'`.
Văn bản ngoài phạm vi trả «không tìm thấy» (404), không phải 403, để không lộ sự tồn tại.

## 4. API (route mới, `/api/v2/to-chuc/*`)

- `GET /to-chuc` (các tổ chức của tôi + vai trò), `POST /to-chuc` (tạo; người tạo là quản trị; mỗi tài khoản tạo tối đa 3 tổ chức chưa có gói).
- `GET|PUT /to-chuc/:id` (hồ sơ + hồ sơ Cơ quan), `GET /to-chuc/:id/thanh-vien`, `PATCH|DELETE /to-chuc/:id/thanh-vien/:uid`, `POST /to-chuc/:id/roi`.
- `POST /to-chuc/:id/loi-moi`, `GET /to-chuc/:id/loi-moi`, `DELETE /to-chuc/:id/loi-moi/:lid`, `POST /loi-moi/nhan` (thân `{ ma }`).
  Thư mời gửi qua `src/lib/email/gui.ts`; đường nhận `https://app.vanbanplus.giakiemso.com/loi-moi?ma=…` (mã trong query, `serve
  --no-request-logging` đã chặn log).
- `GET|POST /kho/van-ban/:id/giao-viec`, `PATCH /giao-viec/:gid` (trạng thái / ghi chú / sửa giao), `GET /giao-viec/cua-toi`.
- `GET /goi-to-chuc` (bảng gói đang bán). Đơn: `POST /api/orders` nhận `to_chuc_id`.
- Giới hạn tốc độ: tạo lời mời 30 / giờ / tổ chức; nhận lời mời sai mã 10 / giờ / người.

## 5. Web

- Ngữ cảnh: `src/api/khach.ts` gắn `X-VBP-To-Chuc` khi ngữ cảnh ≠ cá nhân (một chỗ cho mọi lời gọi). Ngữ cảnh đang chọn lưu
  `localStorage` theo `uid` (tiện ích; mất thì về cá nhân). Đổi ngữ cảnh → xóa bộ đệm cấp mô-đun (hồ sơ Cơ quan, danh sách…) như
  khi đổi phiên.
- Nút chuyển «Cá nhân / Tổ chức X» trên thanh menu (os-kit có chỗ đặt thì dùng; không thì phần tử của web, không sửa `src/os-kit/`).
- Phân hệ mới (một dòng mỗi phân hệ trong `src/ung-dung.ts`): **Tổ chức** (tạo, hồ sơ, thành viên + lời mời, gói / credit /
  dung lượng) và **Việc của tôi** (danh sách việc, lọc trạng thái, quá hạn, cập nhật nhanh). Trang `/loi-moi` nhận lời mời.
- Chi tiết văn bản (ngữ cảnh tổ chức): khối «Giao xử lý» (chủ trì, phối hợp, hạn, ý kiến chỉ đạo, nhật ký) + ô «Công khai trong
  cơ quan». Cán bộ thấy khối ở dạng chỉ đọc + nút đổi trạng thái việc của mình.
- Quét (ngữ cảnh tổ chức): so cơ quan ban hành với hồ sơ Cơ quan của tổ chức.
- Mua (ngữ cảnh tổ chức, quản trị): chọn gói tổ chức, nạp credit, dung lượng; đơn gắn `to_chuc_id`.
- Ẩn: toàn bộ ở nhánh `dot-b-to-chuc`; không có cờ trên prod.

## 6. Kiểm

- API `node:test` + Postgres thử (`scripts/csdl-thu.sh`): thêm 017–020 vào `TEP_LUOC_DO` của `tests/ho-tro/csdl-thu.ts` (kèm 016
  đang thiếu). Bài kiểm bắt buộc:
  - ma trận quyền mục 3 cho từng hàm SQL (cán bộ không đọc / sửa / tìm / tải tệp văn bản ngoài phạm vi; 404 không phải 403);
  - người ngoài tổ chức gửi header tổ chức → 403; thành viên `da_roi` → 403;
  - quản trị cuối không rời được; vượt số thành viên tối đa → từ chối lời mời;
  - lời mời: sai email, hết hạn, dùng lại, đoán mã → từ chối;
  - ví: chạy AI ở ngữ cảnh tổ chức trừ ví tổ chức, hoàn credit về đúng ví; bản hàm cũ (ví cá nhân) cho kết quả giống hệt trước
    migration (chạy lại bộ kiểm credit hiện có không sửa);
  - `pay_order` gói tổ chức kích hoạt đúng tổ chức, chạy lại webhook không cộng đôi;
  - hết hạn gói → chỉ đọc + chặn AI ví tổ chức.
- Kịch bản tấn công (theo bộ nhớ «kế hoạch có kịch bản tấn công»): giả header tổ chức khác, IDOR `van_ban_id` / `giao_viec_id` của
  tổ chức khác, cán bộ tự giao việc / tự nâng vai trò, nhận lời mời bằng tài khoản email khác, đơn `goi_to_chuc` do người không
  phải quản trị tạo.
- Web vitest cho ngữ cảnh, quyền hiển thị theo vai trò, Việc của tôi; chụp Playwright 1440 + 390 (sáng + tối) các màn mới + video
  thao tác một vòng (tạo tổ chức → mời → văn thư vào sổ → giao việc → cán bộ cập nhật) gửi Telegram để duyệt.
- Migration: CSDL thử → bản sao (chạy 2 lần). **Không chạy prod** trước khi chủ dự án duyệt.

## 7. Lên prod (sau khi duyệt)

Migrate prod (giao dịch, `ON_ERROR_STOP`) → chạy lại bộ kiểm credit trên bản sao sau migrate → đẩy `main` API → đẩy `main` web →
thử prod bằng tài khoản thử → ghi `THAY-DOI.md` (cách lùi) + đánh dấu kế hoạch. Chủ dự án chốt giá `goi_to_chuc` và bật `dang_ban`.
