# Hợp đồng giữa các luồng — bản web đợt A

Ngày 17/11/2026. Spec: `docs/superpowers/specs/2026-11-17-ban-web-dot-a-design.md` (đọc trước). Tệp này chốt những gì
các phiên chạy song song phải giống nhau: luồng, nhánh, thư mục, số migration, route, dạng dữ liệu, mã lỗi, luật lên
production. Luồng nào cần đổi một điều ở đây → sửa tệp này trong cùng commit và ghi một dòng ở mục 9.

**Việc còn lại tới khi mở bán** (chờ chủ dự án, điều kiện mở bán, việc chưa luồng nào nhận, sửa còn tồn):
`docs/superpowers/plans/2026-11-17-ban-web-viec-con-lai.md` — đọc trước khi bắt đầu một luồng; làm xong mục nào thì đánh dấu ở đó.

## 1. Luồng và đợt

| Đợt | Luồng | Việc (spec §11) | Repo · nhánh · worktree |
| --- | --- | --- | --- |
| 1 | **L1 API phiên** | Phiên, 4 cách đăng nhập, CORS, module gửi email | `vanbanplus-api` · `web-phien` · `D:/AIVanBanCaNhan/_wt-web-phien` |
| 1 | **L2 Khung web** | Lớp nối os-kit; repo `vanbanplus-web`: khung, đăng nhập, tài khoản, Coolify + DNS | `D:/Thang/os-kit` · `web-dieu-huong`; `D:/AIVanBanCaNhan/vanbanplus-web` (repo mới, riêng tư) |
| 1 | **L3 API kho** | Kho văn bản + tệp + dung lượng + gói dung lượng + nhắc email; sao lưu Storage | `vanbanplus-api` · `web-kho` · `D:/AIVanBanCaNhan/_wt-web-kho` |
| 2 | **L4 Web văn bản** | Màn văn bản, quét tạo văn bản, 6 tác vụ AI, xuất Word | `vanbanplus-web` |
| 2 | **L5 Mẫu + pháp quy** | API + web | cả hai repo |
| 2 | **L6 Họp + album** | API + web | cả hai repo |

Đợt 2 bắt đầu khi L1, L2, L3 đã lên `main`. Mỗi luồng tự viết kế hoạch (`docs/superpowers/plans/2026-11-17-web-<luong>.md`)
từ spec + hợp đồng này, rồi thực hiện.

**Thư mục thuộc luồng** (luồng khác không sửa; cần thì ghi mục 9):

- L1: `src/lib/phien/**`, `src/lib/email/**`, `src/app/api/v2/phien/**`, `src/app/api/v2/toi/**`, `src/lib/tac-vu/xac-thuc.ts`,
  `src/proxy.ts`, `next.config.ts` (phần CORS).
- L3: `src/lib/kho/**`, `src/app/api/v2/kho/**`, `src/app/api/v2/goi-dung-luong/**`, `src/app/api/noi-bo/**`, nhánh
  `dung_luong` trong `src/lib/orders.ts` + `src/lib/order-rules.ts` + route `/api/orders`, trang `/thanh-toan`.
- Tệp kiểm: mỗi luồng đặt tên tệp `tests/<luong>-*.test.ts` (L1 `phien-`, L3 `kho-`).

## 2. Migration (số đã giữ chỗ)

| Số | Luồng | Nội dung |
| --- | --- | --- |
| `009_phien.sql` | L1 | `phien`, `dang_nhap_ngoai`, `otp_email`, `hai_lop` (bí mật TOTP mã hóa + băm mã dự phòng), `nhat_ky_bao_mat`, `phien_token`, `ma_tam_2_lop`, `dem_tan_suat`; cột `users.co_mat_khau`, `users.hai_lop_bat`, `users.email_xac_minh_luc` |
| `010_kho.sql` | L3 | `kho_thu_muc`, `kho_van_ban`, `kho_tep`, `kho_dung_luong`; bỏ bảng `cloud_*` của 001 (kiểm trống trên prod trước); `unaccent` |
| `011_dung_luong_mua.sql` | L3 | dòng bảng giá `goi_dung_luong`, `orders.loai='dung_luong'` + cột, `kho_dung_luong_mua`, nhánh mới của `pay_order`, `nhac_email`, pg_cron dọn hàng chờ + nhắc |
| `012_mau_phap_quy.sql` | L5 | `kho_mau`, `mau_chung`, `phap_quy` |
| `013_hop_album.sql` | L6 | `kho_cuoc_hop`, `kho_album`, `kho_anh` |
| `014_phien_cap_lai.sql` | L1 | `phien_lam_moi` cấp lại khi phản hồi xoay token bị mất |

Luồng lệ chung: RLS bật, không policy, `REVOKE ALL ... FROM anon, authenticated`; hàm `SECURITY DEFINER` đặt
`search_path`; mọi bản ghi kho có `chu_loai text check (chu_loai in ('ca_nhan','to_chuc'))`, `chu_id uuid`, `tao_boi uuid`,
`tao_luc`, `sua_luc`, `phien_ban int default 1`.

**CSDL thử song song** (không dùng chung container): L1 `vbp-csdl-thu-phien` cổng `55991`, L3 `vbp-csdl-thu-kho` cổng
`55992`, L5 `55993`, L6 `55994` — cùng ảnh `supabase/postgres:17.6.1.155` như `scripts/csdl-thu.sh` (chép lệnh, đổi tên +
cổng; không sửa script dùng chung).

## 3. Quy ước chung của API v2

- Thân trả về giữ khuôn hiện có (`src/lib/response.ts`): `{ success, message, data?, code? }`. Lỗi có `code` máy đọc được.
- Xác thực: `xacThuc(req): Promise<ApiUser | null>` (`src/lib/tac-vu/xac-thuc.ts`) — **chữ ký không đổi**. L1 thêm nhánh
  `Authorization: Bearer <jwt>`; không có Bearer thì giữ `X-API-Key` như cũ. Mọi route v2 mới chỉ gọi hàm này.
- Mã lỗi dùng chung:

| `code` | HTTP | Nghĩa |
| --- | --- | --- |
| `CHUA_DANG_NHAP` | 401 | Không có / sai token hoặc khóa |
| `PHIEN_HET_HAN` | 401 | JWT hết hạn → web gọi `lam-moi` rồi thử lại một lần |
| `PHIEN_THU_HOI` | 401 | Phiên bị thu hồi → về màn đăng nhập |
| `LAM_MOI_TRUNG` | 409 | `lam-moi` gửi token vừa được xoay < 30 giây trước **và token mới hơn đã được tab khác dùng** — phiên **không** bị thu hồi; chờ khoảng 1 giây rồi gọi `lam-moi` lại một lần (cookie đã là token mới). Từ 014: token mới hơn chưa ai dùng (phản hồi xoay bị mất) thì `lam-moi` cấp lại phiên + cookie mới (200), không trả mã này |
| `SAI_DANG_NHAP` | 401 | Sai email / mật khẩu (không nói rõ cái nào) |
| `TAI_KHOAN_KHOA` | 423 | Khóa tạm; `data.mo_luc` ISO |
| `CAN_2_LOP` | 200 | Không phải lỗi: `data = { can_2_lop: true, ma_tam }` |
| `OTP_SAI` · `OTP_HET_HAN` | 400 | Mã sai / hết hạn hoặc đã hủy |
| `QUA_NHIEU_YEU_CAU` | 429 | Giới hạn tần suất; `data.thu_lai_sau_giay` |
| `KHONG_TIM_THAY` | 404 | Không có, hoặc không thuộc người gọi |
| `XUNG_DOT_PHIEN_BAN` | 409 | `phien_ban` gửi lên khác bản trong CSDL; `data.ban_moi` |
| `HET_DUNG_LUONG` | 403 | Đang chỉ đọc vì vượt hạn mức |
| `TEP_KHONG_HOP_LE` | 400 | Mime không cho phép, quá 50 MB, kích thước thật khác khai báo |
| `CAN_MA_2_LOP` | 400 | Đặt lại mật khẩu trên tài khoản bật 2 lớp: thiếu / sai `ma_2_lop`; mã email đã dùng → lấy mã mới |
| `CHUA_CO_MAT_KHAU` | 409 | Đổi mật khẩu trên tài khoản chưa có mật khẩu → đặt qua OTP `dat-lai-mat-khau` |
| L3 thêm | | `THU_MUC_KHONG_TRONG` 409 · `CHUA_O_THUNG_RAC` 409 · `DANG_O_THUNG_RAC` 409 · `TEP_CHUA_TAI_LEN` 409 (xác nhận khi tệp chưa lên Storage — tải xong rồi gọi lại) · `DA_NGUNG` 410 |
| L1 thêm | | `DAU_VAO_SAI` 400 · `EMAIL_DA_CO` 409 · `TAI_KHOAN_NGUNG` 403 · `GOOGLE_SAI` 401 · `GOOGLE_LOI` 502 · `EMAIL_LOI` 502 · `CHUA_CAU_HINH` 503 · `SAI_MAT_KHAU` 400 · `DA_BAT` / `CHUA_BAT` 409 · `NGUON_LA` 403 · `LOI_MAY_CHU` 500 |

- **Route v2 có sẵn** (`/api/v2/credit`, `/api/v2/tac-vu/*`) trả 401 **không có `code`** khi Bearer hết hạn / bị thu hồi → web: mọi
  401 khi đang gửi Bearer thì gọi `lam-moi` một lần rồi thử lại; `lam-moi` hỏng thì về màn đăng nhập.

- Ngày giờ: ISO 8601 có múi giờ. Tên trường JSON: snake_case tiếng Việt không dấu như các route v2 hiện có.

## 4. Hợp đồng L1 — phiên và đăng nhập

**Token:** JWT HS256 khóa `VBP_PHIEN_KHOA`, 15 phút, claim `{ sub: user_id, sid: phien_id, iat, exp }`. `xacThuc` kiểm
chữ ký + hạn + `phien.thu_hoi_luc is null` (một truy vấn theo khóa chính).
**Token làm mới:** cookie `vbp_lam_moi`, `HttpOnly; Secure; SameSite=Lax; Path=/api/v2/phien`, không đặt `Domain`, 30 ngày.
App di động gửi trong thân `{ token_lam_moi }` thay cookie.

**Phiên trả về** (mọi cách đăng nhập thành công): `data = { access_token, het_han_giay: 900, nguoi_dung: NguoiDung }` + đặt
cookie. `NguoiDung = { id, email, ho_ten, goi, goi_het_han, co_mat_khau, hai_lop_bat, google_gan }`.

| Route | Thân | Trả |
| --- | --- | --- |
| `POST /api/v2/phien/dang-nhap` | `{ email, mat_khau }` | phiên · `CAN_2_LOP` · `SAI_DANG_NHAP` · `TAI_KHOAN_KHOA` |
| `POST /api/v2/phien/google` | `{ id_token }` | phiên · `CAN_2_LOP` (email mới → tạo tài khoản Free) |
| `POST /api/v2/phien/otp/gui` | `{ email, muc_dich: "dang-nhap" \| "dang-ky" \| "dat-lai-mat-khau" }` | luôn `200` (không lộ email có tồn tại) · `QUA_NHIEU_YEU_CAU` |
| `POST /api/v2/phien/otp/xac-nhan` | `{ email, ma, muc_dich, mat_khau?, ho_ten? }` | phiên · `CAN_2_LOP` · `OTP_SAI` · `OTP_HET_HAN` · `CAN_MA_2_LOP`. `dang-ky` cần `mat_khau` + `ho_ten`, tạo tài khoản Free; `dang-nhap` với email chưa có tài khoản → tạo tài khoản Free không mật khẩu; `dat-lai-mat-khau` cần `mat_khau` (+ `ma_2_lop` khi tài khoản bật 2 lớp), thu hồi mọi phiên cũ |
| `POST /api/v2/phien/2-lop` | `{ ma_tam, ma }` (6 số TOTP hoặc mã dự phòng) | phiên · `OTP_SAI` |
| `POST /api/v2/phien/lam-moi` | cookie, hoặc `{ token_lam_moi }` | phiên mới + cookie mới · `LAM_MOI_TRUNG` · `PHIEN_THU_HOI` |
| `POST /api/v2/phien/dang-xuat` | — | `200`, xóa cookie, thu hồi chuỗi hiện tại |
| `GET /api/v2/phien/thiet-bi` | — | `[{ id, thiet_bi, ip, tao_luc, dung_luc, la_phien_nay }]` |
| `DELETE /api/v2/phien/thiet-bi/[id]` · `POST /api/v2/phien/thiet-bi/thu-hoi-tat-ca` | — | `200` (`thu-hoi-tat-ca` giữ phiên đang dùng) |
| `GET /api/v2/toi` | — | `NguoiDung` |
| `PATCH /api/v2/toi` | `{ ho_ten }` | `NguoiDung` |
| `POST /api/v2/toi/mat-khau` | `{ mat_khau_cu, mat_khau_moi }` | `200` (thu hồi mọi phiên khác) · `SAI_MAT_KHAU` · `CHUA_CO_MAT_KHAU` |
| `POST /api/v2/toi/2-lop/bat-dau` | — | `{ otpauth_url, bi_mat }` |
| `POST /api/v2/toi/2-lop/xac-nhan` | `{ ma }` | `{ ma_du_phong: string[10] }` |
| `POST /api/v2/toi/2-lop/tat` | `{ ma }` | `200` |

**CORS** (`src/proxy.ts`, chỉ `/api/v2/**`): nguồn trong `VBP_CORS_NGUON` (phân cách dấu phẩy; mặc định
`https://app.vanbanplus.giakiemso.com,capacitor://localhost,https://localhost`) → trả đúng nguồn +
`Access-Control-Allow-Credentials: true`; xử lý `OPTIONS`. Route ngoài v2 giữ `*` như cũ (bản cài máy không gửi `Origin`).
Phát triển: thêm `http://localhost:5173` qua biến môi trường, không ghi cứng.

**Email** (`src/lib/email/gui.ts`, L1 đưa lên `main` sớm nhất có thể vì L3 dùng):

```ts
export type ThuEmail = { den: string; tieu_de: string; van_ban: string; html?: string };
export async function guiEmail(thu: ThuEmail): Promise<void>; // ném lỗi khi gửi hỏng
```

Bản Gmail API đọc khóa từ biến môi trường `VBP_GMAIL_*`; khi thiếu khóa (máy dev, CSDL thử) thì ghi log thay vì gửi.

## 5. Hợp đồng L3 — kho và dung lượng (đợt 2 dùng)

```ts
// Văn bản trả về (rút gọn; đủ trường theo spec §5)
type VanBan = { id; thu_muc_id; huong: "den" | "di" | "noi_bo"; so; ky_hieu; ngay_ban_hanh; co_quan; trich_yeu; loai;
  do_khan; do_mat; so_den; ngay_den; han_xu_ly; giao_cho; can_cu; noi_nhan; nguoi_ky; chuc_danh; noi_dung; tom_tat;
  the: string[]; lien_quan: string[]; gan_sao: boolean; ghi_chu; tep: Tep[]; tao_luc; sua_luc; xoa_luc; phien_ban };
type Tep = { id; ten; mime; byte; so_trang; co_anh_nho: boolean; tao_luc };
```

| Route | Ghi chú |
| --- | --- |
| `GET /api/v2/kho/dung-luong` | `{ da_dung_byte, han_muc_byte, goi_mb, them_mb, chi_doc, goi_them: [{ ma, them_mb, het_han }] }` |
| `GET/POST /api/v2/kho/thu-muc` · `PATCH/DELETE /api/v2/kho/thu-muc/[id]` | Cây theo `cha_id`; xóa thư mục còn văn bản → 409 |
| `GET /api/v2/kho/van-ban` | Lọc `q` (không dấu), `huong`, `loai`, `tu`, `den`, `thu_muc`, `the`, `sao`, `thung_rac`; `trang`, `so_dong` ≤ 100 → `{ muc: VanBan[], tong }` |
| `POST /api/v2/kho/van-ban` · `GET/PATCH/DELETE /api/v2/kho/van-ban/[id]` | `PATCH` bắt buộc `phien_ban`; `DELETE` = xóa mềm, `?vinh_vien=1` xóa hẳn (chỉ khi đã ở thùng rác) |
| `POST /api/v2/kho/van-ban/[id]/khoi-phuc` | Lấy ra khỏi thùng rác |
| `POST /api/v2/kho/tep/xin-tai-len` | `{ ten, mime, byte, gan: { loai: "van_ban" \| "cuoc_hop" \| "album", id } }` → `{ id, url_tai_len, het_han }` |
| `POST /api/v2/kho/tep/xac-nhan` | `{ id }` → `Tep` |
| `GET /api/v2/kho/tep/[id]/tai-ve` | `{ url, het_han }` (10 phút) |
| `DELETE /api/v2/kho/tep/[id]` | Trừ dung lượng |
| `GET /api/v2/goi-dung-luong` | `[{ ma, them_mb, gia_vnd_thang }]` |
| `POST /api/orders` | Thêm `loai: "dung_luong"`, `goi_dung_luong_ma`, `so_thang` 1–12 |

Hàm dùng chung cho L5, L6 (L3 xuất ra): `src/lib/kho/chu-so-huu.ts` → `chuCuaNguoiDung(user): { chu_loai, chu_id }`;
`src/lib/kho/han-muc.ts` → `kiemConGhi(chu): Promise<void>` (ném `LoiHetDungLuong`, `code = "HET_DUNG_LUONG"`; bọc route
bằng `chay` của `src/lib/kho/tra-loi.ts` thì tự thành 403). Gắn tệp vào `cuoc_hop` / `album`: L6 thêm hàm kiểm chủ sở hữu
vào **`src/lib/kho/gan-tep-dang-ky.ts`** (đăng ký tĩnh, nạp cùng `gan-tep.ts`; `dangKyGanTep` lúc chạy chỉ dùng cho kiểm
thử). Xóa cuộc họp / album phải xóa tệp qua `kho_tep_xoa` / `kho_xoa_van_ban_noi`-tương đương để trừ dung lượng.

**Bản đã lên production (17/11/2026, vanbanplus-api `ec97de8`) khác bảng trên ở:**

- Danh sách `GET /van-ban` không kèm `noi_dung` (đọc chi tiết `GET /van-ban/[id]` mới có).
- `xin-tai-len`: `het_han` = 120 giây (Storage prod), phải PUT ngay; giới hạn 60 lần / 15 phút / tài khoản (429). Tải lên
  bằng `PUT url_tai_len` thân là tệp, `Content-Type` = mime đã khai.
- `dung-luong` thêm `cho_byte` (tệp đang chờ); `goi_them` có thêm `bat_dau` (gói mua tiếp nối chưa bắt đầu vẫn liệt kê).
- `DELETE /tep/[id]` với tệp đang chờ: hàng chờ còn tính tối đa 3 phút rồi tự dọn.
- `POST /api/orders` + `GET /api/orders/[code]` nhận Bearer (ngoài X-API-Key), CORS như route v2. `orderView` thêm
  `goiDungLuongMa`, `soThang`, `themMb`, `periodStart`.
- `/api/cloud/*` (đồng bộ cloud cũ của bản cài máy) trả 410 `DA_NGUNG`; 14 bảng của 001 đã bỏ.

## 5b. Hợp đồng L5 — mẫu văn bản + tra cứu pháp quy

```ts
type TruongMau = { ma: string; nhan: string };            // ma: ^[A-Za-z][A-Za-z0-9_]{0,63}$ — khóa `truong` của tác vụ soan-thao
type MauTomTat = { nguon: "chung" | "rieng"; id: string;   // chung: mã chữ (vd san-cong-van) · rieng: uuid
  ten: string; loai: LoaiVanBan; nhom: string | null; mo_ta: string | null; the: string[]; co_noi_dung: boolean;
  moi: boolean; pho_bien: boolean; tu_mau_chung: string | null; tao_luc: string; sua_luc: string; phien_ban: number };
type Mau = MauTomTat & { noi_dung_mau: string | null; truong: TruongMau[] };
type NutCay = { ma: string; cha: string | null; loai: "van_ban" | "chuong" | "muc" | "dieu" | "phu_luc" | "phan"; tieu_de: string; thu_tu: number };
type MucPhapQuy = NutCay & { noi_dung: string; the: string[]; duong_dan: { ma: string; tieu_de: string }[];
  con: { ma: string; loai: string; tieu_de: string }[]; truoc: { ma: string; tieu_de: string } | null; sau: { ma: string; tieu_de: string } | null };
```

| Route | Thân / tham số | Trả |
| --- | --- | --- |
| `GET /api/v2/mau` | `q` (không dấu), `nguon` (`chung`\|`rieng`), `loai`, `nhom`, `trang`, `so_dong` ≤ 100 (mặc định 50) | `{ muc: MauTomTat[], tong, nhom: [{ ten, so }] }` — mẫu riêng lên trước (mới sửa trước), rồi mẫu có sẵn theo thứ tự bản cài máy; có `q` thì xếp theo độ khớp |
| `GET /api/v2/mau/chung/[ma]` | — | `Mau` · 404 |
| `POST /api/v2/mau/rieng` | `{ ten, loai, nhom?, mo_ta?, noi_dung_mau?, truong?: [{ nhan, ma? }], the? }` hoặc `{ tu_mau_chung: ma, ten? }` | `Mau` · 403 `HET_DUNG_LUONG` · 400 · 404 · 429 |
| `GET /api/v2/mau/rieng/[id]` | — | `Mau` · 404 |
| `PATCH /api/v2/mau/rieng/[id]` | `{ phien_ban, ...trường cần đổi }` | `Mau` · 409 `XUNG_DOT_PHIEN_BAN` (`data.ban_moi`) · 400 · 404 · 429 |
| `DELETE /api/v2/mau/rieng/[id]` | — | `null` · 404 · 429 (xóa hẳn, mẫu không có tệp) |
| `GET /api/v2/phap-quy` | — | `NutCay[]` (cả cây, không kèm nội dung) |
| `GET /api/v2/phap-quy/[ma]` | `ma` dạng `nd-30-2020` hoặc `nd-30-2020:dieu-8` | `MucPhapQuy` · 404 |
| `GET /api/v2/phap-quy/tim` | `q` | `{ muc: [{ ma, loai, tieu_de, duong_dan: string, trich: string }], tong }` (≤ 50 mục) |

**Nối với L4 (soạn thảo):** nút «Soạn bằng AI theo mẫu» mở `/soan-thao?mau=<chung|rieng>:<id>` (`duongSoanTheoMau` trong `src/phan-he/mau/api.ts`). L4 đọc bằng `docThamSoMau(searchParams.get("mau"))` rồi `apiMau.doc(nguon, id)` → `loai` = `loai_van_ban`, `ten` = `ten_mau`, `truong[].ma` = khóa của `truong`, `truong[].nhan` = nhãn ô nhập. Khi phân hệ `soan-thao` chưa có `trang`, nút trỏ `/sap-co/soan-thao`. Mẫu riêng: khóa trường là ASCII sinh từ nhãn gõ tay (`co_quan_nhan`); tác vụ soạn thảo dựng nhãn trong prompt bằng `nhanTruong(khoa)` nên nhãn có dấu chưa tới AI — cần tham số nhãn ở tác vụ soạn thảo (đợt sau).

## 5c. Hợp đồng L6 — lịch họp + album ảnh (prod 17/11/2026, migration `013_hop_album`, vanbanplus-api `a07e17e`)

Chi tiết + kịch bản tấn công: `docs/superpowers/plans/2026-11-17-web-l6.md`. Kiểu (snake_case như API):

- `CuocHopTom = { id, tieu_de, so_giay_moi, loai, hinh_thuc: truc_tiep|truc_tuyen|ket_hop, trang_thai: du_kien|dang_hop|da_hop|hoan|huy, bat_dau, ket_thuc, ca_ngay, dia_diem, chu_tri, the, phien_ban, so_nhiem_vu, so_nhiem_vu_xong, so_tep }`
- `CuocHop` = các cột của cuộc họp (không có ba trường `so_*`) + `lien_ket, thu_ky, don_vi, thanh_phan[], chuong_trinh, ghi_chep, ket_luan, ghi_chu, nhiem_vu[], van_ban_lien_quan[{ loai, van_ban_id, con, so?, ky_hieu?, trich_yeu?, o_thung_rac? }], tep: Tep[], album[{ id, ten }], tao_luc, sua_luc`
- `Album = { id, ten, mo_ta, ngay_su_kien, anh_bia_id, cuoc_hop: { id, tieu_de, bat_dau } | null, so_anh, byte, tao_luc, sua_luc, phien_ban }`; `Anh = { id (= kho_tep.id), ten, mime, byte, tao_luc, rong, cao, chup_luc, chu_thich, url_nho, url_xem }`

| Route | Thân / tham số | Trả |
| --- | --- | --- |
| `GET /api/v2/kho/cuoc-hop?tu=&den=` | ISO có múi giờ, `den - tu` ≤ 62 ngày | `{ muc: CuocHopTom[], tong }` (giao khoảng `[tu, den)`, ≤ 1000) |
| `GET /api/v2/kho/cuoc-hop?q=&trang=&so_dong=` | `q` luôn có (rỗng = tất cả); tìm không dấu tên, số giấy mời, địa điểm, chủ trì, đơn vị | `{ muc, tong }` |
| `POST /api/v2/kho/cuoc-hop` · `GET / PATCH / DELETE …/[id]` | `PATCH` bắt buộc `phien_ban` | `CuocHop` · `XUNG_DOT_PHIEN_BAN` · `DELETE` xóa hẳn kèm tệp (album liên kết giữ lại) |
| `GET /api/v2/kho/album?q=&trang=&so_dong≤60` | — | `{ muc: (Album & { anh_bia_url })[], tong, het_han }` |
| `POST /api/v2/kho/album` · `GET …/[id]?trang=&so_dong≤60` · `PATCH / DELETE …/[id]` | `PATCH` + `phien_ban`, `anh_bia_id` phải là ảnh đã tải của album | `Album` / `{ album, anh: Anh[], tong, het_han }` |
| `POST /api/v2/kho/album/[id]/xin-tai-len` | `{ tep: [{ ten, mime, byte, rong?, cao?, chup_luc? }] }` 1–10 ảnh | `{ muc: ({ id, url_tai_len, het_han } \| { loi: { code, message } })[] }` — tính 1 lần vào hạn `kho-xin` |
| `PATCH /api/v2/kho/album/[id]/anh/[tep_id]` | `{ chu_thich?, rong?, cao?, chup_luc? }` | ảnh (không url) |
| Tệp cuộc họp / ảnh lẻ | route L3 `/api/v2/kho/tep/*`, `gan: { loai: "cuoc_hop" \| "album", id }` | như L3; xóa ảnh = `DELETE /api/v2/kho/tep/[id]` |

Luật mới (áp cho mọi luồng dùng kho tệp): album chỉ nhận `image/jpeg|png|webp|gif` (cả đường xin chung L3 → 400 `TEP_KHONG_HOP_LE`); xác nhận tệp khi đối tượng gắn (văn bản / cuộc họp / album) đã xóa → 404 + bỏ tệp. Giới hạn: tạo cuộc họp + album 120 / 15 phút, sửa 600 / 15 phút, đọc trang ảnh 300 / 15 phút; ≤ 5.000 cuộc họp, ≤ 2.000 album / chủ; tổng chữ một cuộc họp ≤ 200.000 ký tự (API) / 800 KB (CSDL). Ảnh thu nhỏ = đường ký có biến đổi của imgproxy (`/storage/v1/render/image/sign/…`, 480 cover / 1600 contain); ảnh > 16 MP (giới hạn imgproxy) trả đường ký thường.

## 6. Hợp đồng L2 — os-kit và web

- os-kit bỏ `import` trực tiếp `next/link`, `next/navigation` khỏi thành phần; đi qua một tệp nối `src/dieu-huong.tsx`
  (`Link`, `usePathname`, `useRouter().push`). `cai.mjs` chép bản Next (mặc định) hoặc bản React Router
  (`--dieu-huong react-router`). Project Next đang dùng **không phải sửa mã** khi cài lại; soát gikidi-truonghoc theo
  `os-kit/CLAUDE.md` mục 6.
- `vanbanplus-web`: React 18 + Vite + TypeScript + React Router; gọi API qua một lớp duy nhất `src/api/khach.ts` (gắn Bearer,
  `credentials: "include"`, tự `lam-moi` khi `PHIEN_HET_HAN`, giữ access token trong bộ nhớ, không `localStorage`).
- Đăng ký phân hệ cho đợt 2: mỗi phân hệ một thư mục `src/phan-he/<ten>/` và **một dòng** trong `src/ung-dung.ts`
  (danh sách Launchpad + route). L4 `van-ban`, `soan-thao`, `quet`, `ai`; L5 `mau`, `phap-quy`; L6 `lich-hop`, `album`.
- Biến môi trường build: `VITE_API_GOC` (mặc định `https://vanbanplus.giakiemso.com`), `VITE_GOOGLE_CLIENT_ID`.
- Trước khi L1 lên prod, web chạy với giả lập theo mục 4 (MSW hoặc lớp giả trong `src/api/`), tắt khi build production.

## 7. Lên production

- Đẩy `main` = production (cả hai repo). Chỉ đẩy khi `npm test` + `npm run build` xanh.
- Migration chạy trên CSDL thử, rồi bản sao prod (`csdl-prod` trên `gikidi-dev`), rồi prod trong giao dịch — **trước** khi
  đẩy mã cần nó. Ghi cách lùi vào `D:\Thang\gikidi-server\THAY-DOI.md` (chỉ thêm dòng của mình, không commit thay phiên khác).
- Trước khi đẩy: `git fetch` + rebase lên `origin/main`; chạy lại kiểm sau rebase.
- Khóa mới (`VBP_PHIEN_KHOA`, `VBP_TOTP_KHOA`, `VBP_GMAIL_*`, `VBP_NOI_BO_KHOA`) đặt ở Coolify (chỉ lúc chạy) + bản gốc
  `~/.gikidi/vanbanplus-web.env` trên PC và `dev`. Không in, không commit, không dán vào chat.
- Thêm bucket / nơi lưu tệp → L3 tự kiểm đã vào `sao-luu` trước khi nhận tệp thật.
- Bản web chưa mở bán đến hết đợt 2 (spec §11).

## 8. Việc chủ dự án cần làm (không chặn bắt đầu)

1. **Google OAuth Client ID** (loại Web) trên Google Cloud, nguồn JS cho phép `https://app.vanbanplus.giakiemso.com` và
   `http://localhost:5173` — cần trước khi L1 / L2 bật nút Google.
2. **Hộp thư gửi email** cho Gmail API (OTP, nhắc gia hạn) và cấp quyền gửi — cần trước khi OTP chạy thật.

## 9. Sổ đổi hợp đồng

| Ngày | Luồng | Đổi gì |
| --- | --- | --- |
| 17/11/2026 | — | Bản đầu |
| 17/11/2026 | L1 | Thêm `LAM_MOI_TRUNG` (409): token làm mới bị dùng lại trong 30 giây sau khi xoay thì không thu hồi phiên — nhiều tab cùng làm mới khi mở trang. Quá 30 giây vẫn thu hồi cả phiên (`PHIEN_THU_HOI`). L2: `src/api/khach.ts` gặp mã này thì chờ ~1 giây, gọi `lam-moi` lại một lần; nên khóa làm mới giữa các tab bằng `navigator.locks` |
| 17/11/2026 | L1 | 009 không thêm `users.khoa_den` / `users.so_lan_sai`: khóa sai mật khẩu đếm trong `dem_tan_suat` theo băm email, đếm trước khi so mật khẩu (chống gửi song song), email không tồn tại cũng nhận `TAI_KHOAN_KHOA` như email có (không dò được email) |
| 17/11/2026 | L1 | Đặt lại mật khẩu bằng OTP trên tài khoản bật 2 lớp cần thêm `ma_2_lop` (`CAN_MA_2_LOP`); `toi/mat-khau` bắt buộc `mat_khau_cu`, tài khoản chưa có mật khẩu → `CHUA_CO_MAT_KHAU` (đặt mật khẩu đầu chỉ qua OTP). Thêm danh sách mã lỗi L1 và luật «401 khi có Bearer → `lam-moi` một lần» ở mục 3. Bảng thực tế: `phien` + `phien_token` (xoay token) thay cột `chuoi_id` / `bam_token` của spec. App di động (sau) cần `token_lam_moi` trong thân trả về — bổ sung khi bắt đầu app |
| 17/11/2026 | L3 | Lên production (`010_kho`, `011_dung_luong_mua`, `ec97de8`). Mã lỗi L3 ở mục 3; chỗ khác bảng route ở mục 5 (danh sách không `noi_dung`, tải lên 120 giây + giới hạn 60/15 phút, `cho_byte`, `goi_them.bat_dau`). Sửa ngoài thư mục L3: `src/lib/phien/cors.ts` (CORS nguồn cho phép cho `/api/orders*`), `/api/orders*` nhận Bearer, trang `/tai-khoan`, `/admin`, `/bang-gia` (bỏ quảng cáo đồng bộ cloud cũ), webhook SePay (tin Telegram đơn dung lượng). Đăng ký gắn tệp cho L6 đổi sang tệp tĩnh `src/lib/kho/gan-tep-dang-ky.ts`. Mua dung lượng: `/thanh-toan?dung-luong=dl-10&thang=3`. Việc định kỳ: pg_cron `vbp-kho-don` (mỗi giờ :37) + `vbp-kho-nhac` (01:13 UTC) → `/api/noi-bo/*` khóa `VBP_NOI_BO_KHOA` (Vault); chưa cấu hình Gmail thì nhắc email tạm dừng, không mất thư |
| 17/11/2026 | L2 | Lên production: `https://app.vanbanplus.giakiemso.com` (repo riêng tư `ericphan28/vanbanplus-web`, Coolify app `csywi7ct…`, chỉ-DNS, không proxy Cloudflare vì tên hai nhãn). os-kit 0.4.1: `cai.mjs --dieu-huong react-router` (lần sau tự nhớ), `BangCaiDat` nhận `tenMacDinh`. **Đăng ký phân hệ** (`src/ung-dung.ts`): kiểu `PhanHe = UngDung & { ma; trang?; the? }`, một dòng mỗi phân hệ — chưa có `trang` thì icon khóa sang `/sap-co/<ma>`; đợt 2 bỏ khóa bằng cách thêm `trang: lazy(() => import(...))` vào đúng dòng của mình; `the` = thẻ màn hình chính. Dock / thanh tab: L4 sửa `tab` trong `src/vo/Vo.tsx` khi Văn bản có trang. **CSP** `public/serve.json`: L4/L6 tự thêm nguồn Storage (đường ký tải lên / tải về) vào `connect-src`, `img-src`, `frame-src`; `connect-src` gắn với `VITE_API_GOC`. `khach.ts`: lỗi `lam-moi` khác 401 (503 lúc triển khai, 429) giữ phiên; timeout 15 giây → `MAT_MANG`. Có `LoiTai` + tự tải lại khi chunk cũ bị xóa sau triển khai |
| 17/11/2026 | L1 | Sửa hai lỗi L1 (nhánh `web-phien-sua`). (1) Đặt lại mật khẩu bằng OTP trên tài khoản bật 2 lớp mà không gửi `ma_2_lop`: trả `CAN_MA_2_LOP` ngay sau khi mã email đúng, **không** tính lượt đoán mã 2 lớp, **không** ghi `hai_lop_sai`; mã email đã tiêu nên web lấy mã email mới rồi gửi kèm `ma_2_lop`. Có `ma_2_lop` thì như cũ (đếm, kiểm, ghi khi sai). (2) `014_phien_cap_lai.sql`: trình lại token đã xoay < 30 giây khi phiên còn sống và token mới hơn chưa ai dùng (phản hồi `lam-moi` bị mất do tải lại / rớt mạng) → cấp lại token mới, `lam-moi` trả 200 + cookie mới thay vì `LAM_MOI_TRUNG`; tối đa 3 lần cấp lại cho mỗi token đã dùng, quá trần → `LAM_MOI_TRUNG`; token mới hơn đã được dùng → `LAM_MOI_TRUNG` như cũ; quá 30 giây → thu hồi cả phiên như cũ. Làm mới thường đánh dấu mọi token chưa dùng khác của phiên là đã dùng (gom nhánh, cột `phien_token.bi_gom`); token bị gom trình lại trong 30 giây → `LAM_MOI_TRUNG`. Cấp lại không đổi IP của phiên. Nhật ký bảo mật mới: `token_cap_lai`, `token_dung_lai_som` (kèm `phien_id`, IP, thiết bị). L2 không cần sửa `khach.ts` |
| 17/11/2026 | L5 | Thêm §5b (route mẫu + pháp quy, tham số `/soan-thao?mau=<chung\|rieng>:<id>` cho L4). Sửa ngoài thư mục L5: thêm `export` cho 6 hàm kiểm trong `src/lib/kho/kiem-dau-vao.ts` (không đổi hành vi); 2 dòng `TEP_LUOC_DO` trong `tests/ho-tro/csdl-thu.ts` |
| 17/11/2026 | L4 | **Lớp API dùng chung đã lên `main` web (`a5126f9`)** — đọc trước khi viết web L5/L6: `src/api/khach.ts` — `goi(duong, { phuongThuc, than, hanMs, signal, dau })`, mã `DA_HUY` (người dùng hủy), `QUA_GIO_CHO` (524 Cloudflare ở `/api/v2/tac-vu/*`), 504/522/524 khác → `LOI_MAY_CHU`; `taiLenStorage(url, blob, mime, signal?, khiTien?)` chỉ PUT tới nguồn Storage (`VITE_STORAGE_GOC`, mặc định `https://api-vanbanplus.giakiemso.com`), có `khiTien` thì XHR báo tiến trình, lỗi `TAI_LEN_LOI` / `MAT_MANG` / `DA_HUY`; `laUrlStorage(url)`. `src/api/kho.ts` — kiểu `VanBan`, `Tep`, `ThuMuc`, `Gan`; `kho.taiTepLen(gan, file, mime, signal?, khiTien?)` (xin → PUT → xác nhận, `TEP_CHUA_TAI_LEN` thử lại 1 lần) dùng được cho `cuoc_hop` / `album`; `kho.taiVe` kiểm nguồn Storage; `doanMime(file)`; `MIME_KHO`, `MIME_XEM_TAI_CHO`, `TEP_TOI_DA_BYTE`, `MUA_DUNG_LUONG`, `MUA_CREDIT`; lệnh ghi kho hạn 60 giây. L6: dùng `kho.taiTepLen` thay `src/api/tep.ts` riêng. CSP đã có nguồn Storage ở `connect-src`/`img-src`/`frame-src` — không thêm lại. Thử khói prod (`cong-cu/thu-khoi-l4.mjs`) 14/14: URL ký tải lên / tải về ở nguồn Storage, preflight PUT cho nguồn web, PDF tải về nhúng khung được (không `X-Frame-Options`, không `attachment`), tác vụ v2 cho `X-VBP-*`. L4 đọc `/soan-thao?mau=` theo §5b |
| 17/11/2026 | L5 | Lên production: migration `012_mau_phap_quy` + nạp 80 mẫu chung / 86 mục pháp quy (API `54cda8e`), web `205ff6b` (`/mau`, `/phap-quy`). Khác §5b: trần tổng chữ mẫu riêng 5 triệu ký tự / chủ kiểm cả khi tạo lẫn khi sửa làm nội dung dài ra (`DAU_VAO_SAI`); lệnh ghi mẫu riêng của web dùng hạn chờ 60 giây như lệnh ghi L4; `GET /api/v2/phap-quy*` cần đăng nhập như mọi route v2. Cập nhật dữ liệu chung: sửa `TemplateSeeder.cs` / `LegalReference.cs` → `dotnet run --project tools/xuat-du-lieu-web -- <api>/supabase/du-lieu` → `npx tsx scripts/sinh-du-lieu-mau-phap-quy.ts` → `psql -1` tệp `supabase/du-lieu/012_mau_phap_quy_du_lieu.sql` trên prod |
| 17/11/2026 | L6 | API lên production: migration `013_hop_album` (bảng `kho_cuoc_hop`, `kho_album`, `kho_anh`), vanbanplus-api `a07e17e`, route + luật ở §5c. Đổi hàm L3 trong 013: `kho_tep_xin` (album chỉ nhận ảnh; cuộc họp / album đã xóa → `KHONG_TIM_THAY`), `kho_tep_xac_nhan` (đối tượng gắn đã mất → từ chối với mọi loại gắn; văn bản như 010). Sửa tệp L3: `kiem-dau-vao.ts` thêm `export` 8 hàm kiểm (L5 cũng thêm — đã gộp), `tep.ts` tách `xinTaiLenDaKiem` (+ chặn mime album sớm → `TEP_KHONG_HOP_LE`), `storage.ts` thêm `kyAnh` (đường ký có biến đổi), `gan-tep-dang-ky.ts` đăng ký `cuoc_hop` + `album`, `tests/kho-dich-vu.test.ts` (gắn tệp vào `cuoc_hop` id lạ nay là 404). Không mã lỗi mới. imgproxy giới hạn ảnh nguồn 16,8 MP; container imgproxy chưa có trần CPU (đề xuất chủ dự án đặt). Web L6 dùng `kho.taiTepLen` / `taiLenStorage` của L4, không lớp tệp riêng |
| 17/11/2026 | L6 | Web lên production (vanbanplus-web `7b674d9`): phân hệ `/lich-hop/*` (tháng / tuần / danh sách, chi tiết, tạo / sửa, tệp đính kèm, thẻ «Lịch họp hôm nay») và `/album/*` (lưới ảnh thu nhỏ, tải nhiều ảnh theo lô 4, xem ảnh). Dùng `kho.taiTepLen` / `kho.taiVe` / `kho.xoaTep` / `taiLenStorage` của L4, không lớp tệp riêng; CSP không đổi. Gọi API L6 qua `src/api/hop.ts`, `src/api/album.ts` (hạn 60 giây cho lệnh ghi, `HAN_GHI_MS`). Đường ký ảnh: hạn tính theo đồng hồ máy chủ (`het-han.ts`) để máy lệch giờ không lặp tải lại. Thử thật trên prod 8/8 (CSP connect-src + img-src, imgproxy) |
| 17/11/2026 | L4 | **Web lên production (vanbanplus-web `03debc5`)**: Văn bản (3 cột PC / danh sách → chi tiết điện thoại, thư mục, tìm không dấu, thẻ lọc, vuốt sao / xóa + hoàn tác, chi tiết + tệp xem tại chỗ / tải lên có tiến trình / tải về hai bước, biểu mẫu tạo / sửa, xung đột phiên bản chỉ ghi đè trường người dùng sửa), Nhập từ ảnh/PDF (thu nhỏ ảnh, «Tính credit» theo yêu cầu, trích xuất → soát → lưu + tệp gốc), Soạn thảo AI (8 loại, trường đúng khóa prompt máy chủ, Quyết định bắt buộc chọn mẫu), 4 trang AI (Kiểm tra, Tóm tắt, Tham mưu, Báo cáo định kỳ), Thùng rác, thẻ màn hình chính, xuất Word NĐ 30 trên trình duyệt (`docx` chunk riêng). `kho_van_ban.loai` lưu **mã enum DocumentType** của bản cài máy (`CongVan`, `QuyetDinh`…; 33 mã ở `src/phan-he/van-ban/danh-muc.ts`), giao diện hiện tên tiếng Việt — L5/L6/bản cài máy dùng cùng mã. Soạn theo mẫu: đọc `/soan-thao?mau=<chung\|rieng>:<id>` đúng §5b (`truong[]` của mẫu thành ô nhập, trần 60 trường gồm ô người ký). Lệch spec §7: khung soạn là chữ thuần, định dạng NĐ 30 do bộ xuất Word áp; «Cơ quan» lưu trình duyệt theo tài khoản (`vbp-co-quan:<user_id>`). Đo hủy tác vụ qua Cloudflare trên prod: `NGAT_KET_NOI`, `credit_tru = 0` (hoàn), mô hình vẫn chạy xong; 524 do Cloudflare tự cắt chưa đo — web hỏi xác nhận trước khi chạy lại sau `QUA_GIO_CHO` / `MAT_MANG`. Thử prod (`cong-cu/thu-prod-l4.mjs`) 10/10: đăng nhập, tạo văn bản, tải + xem PDF tại chỗ, ước tính credit, Xuất Word, thùng rác; không lỗi CSP. Chưa làm (`ban-web-viec-con-lai.md` §4.6–4.10): xếp / lọc theo hạn xử lý + độ khẩn ở API, `Ctrl+K`, xem bố cục trang giấy, «Cơ quan» trên máy chủ, khóa chống trùng khi tạo văn bản |
