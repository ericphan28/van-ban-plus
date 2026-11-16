# Hợp đồng giữa các luồng — bản web đợt A

Ngày 17/11/2026. Spec: `docs/superpowers/specs/2026-11-17-ban-web-dot-a-design.md` (đọc trước). Tệp này chốt những gì
các phiên chạy song song phải giống nhau: luồng, nhánh, thư mục, số migration, route, dạng dữ liệu, mã lỗi, luật lên
production. Luồng nào cần đổi một điều ở đây → sửa tệp này trong cùng commit và ghi một dòng ở mục 9.

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
| `009_phien.sql` | L1 | `phien`, `dang_nhap_ngoai`, `otp_email`, `hai_lop` (bí mật TOTP mã hóa + băm mã dự phòng), `nhat_ky_bao_mat`, cột `users.khoa_den`, `users.so_lan_sai` |
| `010_kho.sql` | L3 | `kho_thu_muc`, `kho_van_ban`, `kho_tep`, `kho_dung_luong`; bỏ bảng `cloud_*` của 001 (kiểm trống trên prod trước); `unaccent` |
| `011_dung_luong_mua.sql` | L3 | dòng bảng giá `goi_dung_luong`, `orders.loai='dung_luong'` + cột, `kho_dung_luong_mua`, nhánh mới của `pay_order`, `nhac_email`, pg_cron dọn hàng chờ + nhắc |
| `012_mau_phap_quy.sql` | L5 | `kho_mau`, `mau_chung`, `phap_quy` |
| `013_hop_album.sql` | L6 | `kho_cuoc_hop`, `kho_album`, `kho_anh` |

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
| `LAM_MOI_TRUNG` | 409 | `lam-moi` gửi token vừa được xoay < 30 giây trước (tab khác vừa làm mới) — phiên **không** bị thu hồi; chờ khoảng 1 giây rồi gọi `lam-moi` lại một lần (cookie đã là token mới) |
| `SAI_DANG_NHAP` | 401 | Sai email / mật khẩu (không nói rõ cái nào) |
| `TAI_KHOAN_KHOA` | 423 | Khóa tạm; `data.mo_luc` ISO |
| `CAN_2_LOP` | 200 | Không phải lỗi: `data = { can_2_lop: true, ma_tam }` |
| `OTP_SAI` · `OTP_HET_HAN` | 400 | Mã sai / hết hạn hoặc đã hủy |
| `QUA_NHIEU_YEU_CAU` | 429 | Giới hạn tần suất; `data.thu_lai_sau_giay` |
| `KHONG_TIM_THAY` | 404 | Không có, hoặc không thuộc người gọi |
| `XUNG_DOT_PHIEN_BAN` | 409 | `phien_ban` gửi lên khác bản trong CSDL; `data.ban_moi` |
| `HET_DUNG_LUONG` | 403 | Đang chỉ đọc vì vượt hạn mức |
| `TEP_KHONG_HOP_LE` | 400 | Mime không cho phép, quá 50 MB, kích thước thật khác khai báo |

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
| `POST /api/v2/phien/otp/xac-nhan` | `{ email, ma, muc_dich, mat_khau?, ho_ten? }` | phiên · `CAN_2_LOP` · `OTP_SAI` · `OTP_HET_HAN`. `dang-ky` cần `mat_khau` + `ho_ten`, tạo tài khoản Free; `dat-lai-mat-khau` cần `mat_khau`, thu hồi mọi phiên cũ |
| `POST /api/v2/phien/2-lop` | `{ ma_tam, ma }` (6 số TOTP hoặc mã dự phòng) | phiên · `OTP_SAI` |
| `POST /api/v2/phien/lam-moi` | cookie, hoặc `{ token_lam_moi }` | phiên mới + cookie mới · `LAM_MOI_TRUNG` · `PHIEN_THU_HOI` |
| `POST /api/v2/phien/dang-xuat` | — | `200`, xóa cookie, thu hồi chuỗi hiện tại |
| `GET /api/v2/phien/thiet-bi` | — | `[{ id, thiet_bi, ip, tao_luc, dung_luc, la_phien_nay }]` |
| `DELETE /api/v2/phien/thiet-bi/[id]` · `POST /api/v2/phien/thiet-bi/thu-hoi-tat-ca` | — | `200` |
| `GET /api/v2/toi` | — | `NguoiDung` |
| `PATCH /api/v2/toi` | `{ ho_ten }` | `NguoiDung` |
| `POST /api/v2/toi/mat-khau` | `{ mat_khau_cu?, mat_khau_moi }` (bỏ `mat_khau_cu` khi tài khoản chưa có mật khẩu) | `200` |
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
`src/lib/kho/han-muc.ts` → `kiemConGhi(chu): Promise<void>` (ném lỗi mang `HET_DUNG_LUONG`). Gắn tệp vào `cuoc_hop` /
`album` kiểm chủ sở hữu bằng hàm do L6 đăng ký qua `src/lib/kho/gan-tep.ts` (L3 tạo bảng đăng ký, `van_ban` sẵn).

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
