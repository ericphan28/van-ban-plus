# Đợt B — Tổ chức thuê VanBanPlus, phần API: kế hoạch thực hiện

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or
> superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Thêm vào `vanbanplus-api` (nhánh `dot-b-to-chuc`, chưa lên prod) tổ chức thuê VanBanPlus: tổ chức + thành viên +
lời mời, kho chung có lọc quyền theo vai trò ngay trong hàm SQL, giao việc, ví credit tổ chức, gói tổ chức trả năm qua
SePay — kèm hợp đồng JSON để web làm song song.

**Architecture:** Bốn migration chỉ thêm (017 tổ chức, 018 giao việc + quyền kho, 019 ví credit, 020 đơn tổ chức). Mọi
luật nằm trong hàm `SECURITY DEFINER` trả `jsonb`; hàm kho cũ giữ nguyên chữ ký (thành lớp gọi bản mới có thêm `p_nguoi`),
hàm credit cũ của ví cá nhân giữ nguyên thân. Ngữ cảnh do header `X-VBP-To-Chuc` chọn: một chỗ đọc
(`src/lib/to-chuc/ngu-canh.ts`) gọi `to_chuc_vai_tro` rồi trả `Chu` mang `nguoi` + `vai_tro`; dịch vụ kho chỉ thêm
`p_nguoi` khi là kho tổ chức nên lời gọi cá nhân giữ đúng như cũ.

**Tech Stack:** Next.js 16 App Router, TypeScript, `@supabase/supabase-js` 2.95, Postgres 17 (Supabase tự host), bộ kiểm
`node:test` qua `tsx` + Postgres thử.

**Spec:** `docs/superpowers/specs/2026-11-19-dot-b-to-chuc-design.md` (đọc hết, nhất là §2, §3, §6) · nền đợt A:
`docs/superpowers/specs/2026-11-17-ban-web-dot-a-design.md`, hợp đồng `docs/superpowers/plans/2026-11-17-hop-dong-web-dot-a.md`
(§3 mã lỗi, §5 chủ sở hữu). Kế hoạch web dùng hợp đồng ở mục «Hợp đồng API cho web» dưới đây:
`docs/superpowers/plans/2026-11-19-dot-b-to-chuc-web.md`.

## Global Constraints

- Repo `vanbanplus-api`, nhánh **`dot-b-to-chuc`**, worktree riêng `D:/AIVanBanCaNhan/_wt-b-to-chuc-api` (tạo ở Task 1,
  `node_modules` là junction sang `D:/AIVanBanCaNhan/vanbanplus-api/node_modules`). Không `cd` sang repo gốc, không sửa
  tệp ở repo gốc.
- **TUYỆT ĐỐI không đẩy `main`, không merge vào `main`** (đẩy `main` = production, Coolify tự build). Được
  `git push -u origin dot-b-to-chuc` để giữ bản.
- Migration chỉ chạy trên **CSDL thử** (bộ kiểm tự nạp) và bản sao nếu có; **KHÔNG chạy trên CSDL prod**
  (`supabase-db-q323…`). Lên prod là việc sau khi chủ dự án duyệt (spec §7), không thuộc kế hoạch này.
- Migration **chỉ thêm**, chạy lại lần hai không lỗi, đầu tệp `SET lock_timeout = '5s';`. Bảng mới: RLS bật, không policy,
  `REVOKE ALL … FROM PUBLIC, anon, authenticated, service_role`. Hàm API: `SECURITY DEFINER SET search_path = public, pg_temp`,
  `REVOKE ALL ON FUNCTION … FROM PUBLIC, anon, authenticated` + `GRANT EXECUTE … TO service_role`. Hàm nội bộ: thu hồi cả
  `service_role`.
- Hàm kho cũ (`kho_van_ban_*`, `kho_tep_*`) **giữ chữ ký cũ** làm lớp gọi bản mới với `p_nguoi = NULL`; hàm credit cũ
  (`tac_vu_mo`, `credit_cap_ky`, `credit_so_du`, `credit_tong`, `credit_giu`, `credit_tra_giu`, `credit_tru_cu`,
  `credit_dieu_chinh`, `credit_quet_treo`) **giữ nguyên chữ ký và thân**; `credit_chot`, `credit_hoan`, `credit_cap_nap`,
  `pay_order`, `kho_cap_dung_luong` thay thân (cùng chữ ký) mà nhánh ví cá nhân y như cũ. **`tests/credit-csdl.test.ts`,
  `tests/goi-nap-csdl.test.ts`, `tests/kho-*.test.ts`, `tests/mo-ban-csdl.test.ts`, `tests/hop-csdl.test.ts`,
  `tests/mau-csdl.test.ts` phải xanh mà không sửa một dòng.**
- Khóa dòng: ví cá nhân khóa `users` (FOR NO KEY UPDATE) như 004; ví tổ chức khóa `to_chuc` (FOR NO KEY UPDATE). Thứ tự
  chung: `orders` → `users` → `to_chuc` → các bảng còn lại. Đường ví tổ chức không bao giờ khóa `users`.
- Chữ trả cho người dùng (message, thư mời): tiếng Việt có dấu đầy đủ, **không đại từ nhân xưng** (tôi, chúng tôi, ta,
  chúng ta, anh, chị, em, bạn, mình), không «dạ / vâng / ạ». Spec viết «Việc của tôi»: ở API và web gọi là
  **«Việc được giao»** (đường `/kho/giao-viec/cua-toi` giữ nguyên vì là mã, không phải chữ hiện ra).
- Thân trả về `{ success, message, data?, code? }`; trường JSON snake_case tiếng Việt không dấu; ngày giờ ISO 8601; ngày
  `YYYY-MM-DD`. Văn bản / giao việc ngoài phạm vi của cán bộ trả **404 `KHONG_TIM_THAY`** (không phải 403).
- Lệnh kiểm: `npm test` (= `tsx --test tests/*.test.ts`) **có** `TEST_DATABASE_URL` — không có biến thì bộ kiểm CSDL tự bỏ
  qua, coi như chưa kiểm. CSDL thử riêng của đợt này: container `vbp-csdl-thu-dot-b`, cổng **55996** (chép lệnh
  `scripts/csdl-thu.sh`, đổi tên + cổng; **không sửa script dùng chung**), đường hầm `ssh -N -L 55996:127.0.0.1:55996 gikidi-dev`,
  `TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55996/postgres`.
- Kiểu: `npx tsc --noEmit`. Build trong worktree: `npx next build --webpack` (Turbopack từ chối junction `node_modules`).
- Commit: thông điệp ASCII không dấu theo lệ repo (`feat(to-chuc): ...`), dòng cuối
  `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`. Không in, không commit bí mật; mã mời / khóa giả trong bài
  kiểm không trông như khóa thật.
- Kịch bản tấn công (mục dưới) là bài kiểm thật; Task 16 gom và chạy lại toàn bộ.

## Quyết định khi viết kế hoạch (khác / chi tiết hơn spec)

| Mục spec | Kế hoạch làm | Lý do |
| --- | --- | --- |
| §3 hàm nhận `p_nguoi, p_vai_tro` | Hàm kho mới chỉ nhận **`p_nguoi`**; vai trò do SQL tự tra (`kho_vai_tro`) | Không tin vai trò do API truyền: lỗi ở TS cũng không nâng quyền được |
| §3 sửa hàm kho cũ thêm tham số mặc định | Thêm **bản nạp chồng** cùng tên có `p_nguoi`; bản cũ thành lớp gọi bản mới với `NULL` (từ chối kho tổ chức) | Không `DROP FUNCTION`: bản cũ giữ quyền + chữ ký, bài kiểm cũ nhắc chữ ký cũ vẫn xanh, PostgREST phân biệt theo tên tham số |
| §2.3 bản cũ gọi bản mới với ví cá nhân | Ngược lại: bản `_vi` mới **gọi bản cũ** khi ví cá nhân; chỉ `credit_chot` / `credit_hoan` / `credit_cap_nap` thay thân (rẽ nhánh theo ví của lượt / đơn) | Đường cá nhân không đổi một dòng → bản cài 1.0.20+ và web hiện tại chắc chắn không đổi hành vi |
| §2.3 `credit_so` thêm `vi_loai`, `vi_id` | **Không thêm** vào `credit_so` (ví suy từ `lo_id → credit_lo`); dòng sổ của ví tổ chức có `user_id = NULL`, người chạy truy qua `luot_id → tac_vu_chay.user_id` | `credit_so` chỉ ghi thêm (trigger chặn UPDATE) nên không điền lại được; `user_id` có `ON DELETE CASCADE` — admin xóa một tài khoản sẽ xóa luôn sổ của ví tổ chức nếu ghi `user_id` |
| §2.3 `credit_lo.user_id` | Lô ví tổ chức `user_id = NULL` (bỏ `NOT NULL`, thêm CHECK theo ví) | Cùng lý do cascade |
| §2.2 «tối đa một chủ trì đang hoạt động» | Thêm cột `huy_luc` (thu hồi giao việc) + `DELETE /kho/giao-viec/:gid`; chỉ mục duy nhất từng phần theo `huy_luc IS NULL` | Spec có «sửa giao việc» nhưng không có cách bỏ một người đã giao nhầm |
| §3 «tạo / sửa / xóa thư mục» cán bộ tạo được | Cán bộ **chỉ xem** thư mục; tạo / sửa / xóa thư mục, lịch họp, album, mẫu riêng: quản trị + văn thư (chặn ở API, `KHONG_DU_QUYEN`) | Thư mục là khung sổ văn thư; `kho_thu_muc_json` không trả `tao_boi` nên «thư mục do người đó tạo» cần thêm hàm SQL — ngoài phạm vi |
| §2.1 «chưa đủ số thành viên tối đa của gói» | Chưa có gói / gói hết hạn: tối đa **5** thành viên (để chuẩn bị trước khi mua); kho chỉ đọc; không chạy AI ví tổ chức | Spec không nói tổ chức chưa mua gói được gì; giá gói còn `NULL` nên phải có trạng thái này — **chủ dự án xác nhận con số 5** |
| §2.4 dữ liệu mẫu gói | `tc-10` / `tc-30` / `tc-100`: credit 3.000 / 8.000 / 25.000 mỗi tháng, dung lượng 50 / 150 / 500 GB, tệp AI 10 MB, giá `NULL`, `dang_ban = false` | **Số giữ chỗ** — chủ dự án chốt trước khi bật `dang_ban` |
| §2.1 nhận lời mời | Thêm điều kiện **email đã xác minh** (`users.email_xac_minh_luc`) | Đăng ký kiểu cũ `/api/auth/register` không xác minh email: kẻ khác đăng ký trước bằng email được mời rồi nhận lời mời |
| §2.4 nhắc gia hạn email (`kho_nhac_can_gui`) | **Hoãn** (spec ghi «làm sau nếu kịp; không chặn duyệt») | Giữ phạm vi |
| §4 `GET|PUT /to-chuc/:id` gồm hồ sơ Cơ quan | Làm; thêm: `GET|PUT /api/v2/toi/co-quan` có header tổ chức thì đọc / ghi Cơ quan của tổ chức | Web (Soạn thảo, Quét, Báo cáo) đang đọc Cơ quan qua `/toi/co-quan`; không phải sửa từng chỗ |
| §4 đường giao việc | `/api/v2/kho/van-ban/:id/giao-viec`, `/api/v2/kho/giao-viec/:gid`, `/api/v2/kho/giao-viec/cua-toi` | Đặt dưới `kho/` như các route kho khác (cùng `canNguoiDung`) |

## Kịch bản tấn công

| Bề mặt | Cách lạm dụng | Cách chặn (task) |
| --- | --- | --- |
| Header `X-VBP-To-Chuc` | Gửi id tổ chức khác / không phải uuid / của tổ chức đã rời | `docNguCanh` tra `to_chuc_vai_tro` (chỉ `hoat_dong`) → 403 `KHONG_THUOC_TO_CHUC`; mọi hàm kho tổ chức tự tra lại vai trò bằng `p_nguoi` (T2, T4, T9, T16) |
| | Gọi bản hàm kho cũ (không `p_nguoi`) với `p_chu_loai='to_chuc'` | Bản cũ gọi bản mới với `NULL` → `kho_vai_tro` ném lỗi (T4, T5) |
| IDOR `van_ban_id`, `tep id`, `giao_viec_id` | Dùng id của tổ chức khác hoặc của cá nhân | Mọi truy vấn lọc `chu_loai + chu_id`; giao việc lọc `chu_id`; ngoài phạm vi → 404 (T4–T6, T16) |
| Cán bộ | Đọc / tìm / tải tệp văn bản không được giao | `kho_van_ban_xem_duoc` trong `doc`, `tim`, `tep_doc`; 404 (T4, T5) |
| | Tự giao việc, sửa giao việc của người khác, đổi người nhận / hạn / ý kiến chỉ đạo của việc được giao cho chính người đó | `kho_giao_viec_tao` chỉ quản trị / văn thư; `kho_giao_viec_sua` cán bộ chỉ `trang_thai`, `ghi_chu` của việc được giao cho chính người đó → `KHONG_DU_QUYEN` (T6) |
| | Tự nâng vai trò, đuổi người khác, sửa hồ sơ tổ chức | `to_chuc_thanh_vien_sua/bo`, `to_chuc_sua` chỉ quản trị (T2) |
| | Đoán `khoa_tao` của người khác để đọc văn bản qua đường «tạo trùng» | Kho tổ chức: khóa trùng của người tạo khác → `DAU_VAO_SAI`, không trả văn bản (T4) |
| | Tải tệp lên cuộc họp / album | `kho_tep_xin` chặn cán bộ với `gan_loai <> 'van_ban'`; route ghi lịch họp / album / mẫu / thư mục `chiQuanLy` (T5, T10) |
| Quản trị cuối | Rời / tự hạ vai trò / bị quản trị khác đuổi khiến tổ chức không còn quản trị | Khóa dòng `to_chuc` rồi đếm quản trị khác → `QUAN_TRI_CUOI` (T2) |
| Lời mời | Đoán mã | Mã 32 byte ngẫu nhiên, chỉ lưu SHA-256; sai mã 10 lần / giờ / người → 429 (T2, T12) |
| | Nhận bằng tài khoản email khác; email chưa xác minh | So `lower(email)`; `email_xac_minh_luc IS NULL` → `EMAIL_CHUA_XAC_MINH` (T2) |
| | Dùng lại / hết hạn / đã hủy | `dung_luc`, `het_han`, `huy_luc` trong cùng khóa dòng (T2) |
| | Vượt số thành viên tối đa | Đếm sau khi khóa `to_chuc` lúc tạo và lúc nhận (T2) |
| | Spam thư mời | 30 lời mời / giờ / tổ chức (T12) |
| | Lộ mã qua log / Referer | Mã chỉ ở thư, không trả trong JSON; web xóa `?ma=` khỏi địa chỉ ngay khi đọc (kế hoạch web) |
| Tạo tổ chức | Tạo hàng loạt tổ chức để có kho / thành viên miễn phí | Tối đa 3 tổ chức chưa có gói / tài khoản, đếm sau khi khóa dòng `users` (T2); tổ chức chưa có gói kho 0 MB, chỉ đọc (T3) |
| Đơn hàng | Đơn `goi_to_chuc` / nạp / dung lượng cho tổ chức do người không phải quản trị tạo | API kiểm quản trị (T15) + trigger `orders_kiem_to_chuc` trong CSDL (T8) |
| | Mua gói chưa mở bán / sửa giá | Giá + `dang_ban` đọc ở máy chủ; gói `gia_nam_vnd NULL` không tạo đơn (T15) |
| | Webhook SePay gửi lại | `pay_order` trả `already_paid`; `to_chuc_cap_goi` đọc lại kỳ đã ghi; lô nạp `uq_credit_lo_nap` (T8) |
| Ví credit | Chạy AI bằng ví tổ chức khi không còn là thành viên / gói hết hạn | `tac_vu_mo_vi` tự tra thành viên + gói sau khi khóa `to_chuc` (T7) |
| | Hoàn credit sai ví | `credit_chot` / `credit_hoan` đọc ví từ dòng `tac_vu_chay` (T7) |
| | Khóa chéo giữa ví cá nhân và ví tổ chức | Đường tổ chức không khóa `users`; bài kiểm chạy song song hai ví (T7) |
| Hết hạn gói | Ghi kho tổ chức khi gói hết hạn nhờ dung lượng mua thêm | `kho_dung_luong_xem.het_goi` → `chi_doc`; `kho_tep_xin` chặn khi `het_goi` (T3, T5) |

## Hợp đồng API cho web

Mọi đường dưới `https://vanbanplus.giakiemso.com` (web) — tác vụ AI chữ vẫn đi thẳng `vanbanplus-api.app.giakiemso.com`.
Thân trả `{ success, message, data?, code? }`. Web gắn `X-VBP-To-Chuc: <uuid>` vào **mọi** lời gọi khi đang ở ngữ cảnh tổ
chức (trừ `/api/v2/phien/*`); route tài khoản / tổ chức bỏ qua header.

**Mã lỗi mới** (thêm vào hợp đồng §3): `KHONG_THUOC_TO_CHUC` 403 · `KHONG_DU_QUYEN` 403 · `QUAN_TRI_CUOI` 409 ·
`DU_THANH_VIEN` 409 · `DA_LA_THANH_VIEN` 409 · `QUA_NHIEU_TO_CHUC` 409 · `LOI_MOI_KHONG_HOP_LE` 400 · `SAI_EMAIL_MOI` 403 ·
`EMAIL_CHUA_XAC_MINH` 403 · `CHU_TRI_DA_CO` 409 · `DA_GIAO` 409 · `NGUOI_NHAN_KHONG_HOP_LE` 400 · `GOI_TO_CHUC_HET_HAN` 403 (tác
vụ AI ví tổ chức) · `CHUA_GUI_DUOC_THU` 503 (thư mời không gửi được; lời mời đã tự hủy). Dùng lại: `KHONG_TIM_THAY` 404, `XUNG_DOT_PHIEN_BAN` 409 (`data.ban_moi`), `DAU_VAO_SAI` 400,
`QUA_NHIEU_YEU_CAU` 429 (`data.thu_lai_sau_giay`), `HET_DUNG_LUONG` 403.

**Kiểu dùng chung**

```ts
type VaiTro = "quan_tri" | "van_thu" | "can_bo";
type CoQuan = { ten_co_quan: string; xa: string; tinh: string; dac_diem: string[]; nguoi_ky: string; chuc_danh: string };
type ToChucTomTat = {
  id: string; ten: string; vai_tro: VaiTro; goi_ma: string | null; goi_ten: string | null;
  goi_bat_dau: string | null; goi_het_han: string | null; con_hieu_luc: boolean;
};
type ToChuc = ToChucTomTat & {
  mst: string | null; dia_chi: string | null; email_hoa_don: string | null; phien_ban: number;
  so_thanh_vien: number; so_thanh_vien_toi_da: number; co_quan: CoQuan | null;
};
type ThanhVien = { user_id: string; ho_ten: string; email: string; vai_tro: VaiTro; trang_thai: "hoat_dong"; them_luc: string };
type LoiMoi = { id: string; email: string; vai_tro: VaiTro; het_han: string; tao_luc: string; trang_thai: "cho" | "da_dung" | "da_huy" | "het_han" };
type GoiToChuc = {
  ma: string; ten: string; so_thanh_vien_toi_da: number; credit_thang: number; dung_luong_mb: number;
  max_file_size_mb: number; gia_nam_vnd: number | null; dang_ban: boolean;
};
type GiaoViec = {
  id: string; van_ban_id: string; nguoi_nhan: string; nguoi_nhan_ten: string | null; vai: "chu_tri" | "phoi_hop";
  han_xu_ly: string | null; y_kien_chi_dao: string | null; trang_thai: "cho" | "dang" | "xong"; ghi_chu: string | null;
  giao_boi: string; giao_boi_ten: string | null; giao_luc: string; cap_nhat_luc: string; phien_ban: number;
};
type NhatKyGiaoViec = {
  id: number; giao_viec_id: string; nguoi_ten: string | null; hanh_dong: "giao" | "sua" | "trang_thai" | "ghi_chu" | "thu_hoi";
  truoc: Record<string, unknown> | null; sau: Record<string, unknown> | null; luc: string;
};
type ViecDuocGiao = GiaoViec & { van_ban: { id: string; so: string | null; ky_hieu: string | null; trich_yeu: string; do_khan: string } };
```

**Tổ chức** (không đọc header)

| Đường | Ai | Thân gửi | `data` trả |
| --- | --- | --- | --- |
| `GET /api/v2/to-chuc` | đã đăng nhập | — | `ToChucTomTat[]` (chỉ tổ chức đang là thành viên hoạt động) |
| `POST /api/v2/to-chuc` | đã đăng nhập | `{ ten, mst?, dia_chi?, email_hoa_don? }` | `ToChucTomTat` (người tạo là quản trị) · lỗi `QUA_NHIEU_TO_CHUC` |
| `GET /api/v2/to-chuc/:id` | thành viên | — | `ToChuc` · không phải thành viên: 404 |
| `PUT /api/v2/to-chuc/:id` | quản trị | `{ phien_ban, ten?, mst?, dia_chi?, email_hoa_don?, co_quan?: CoQuan }` | `ToChuc` · `XUNG_DOT_PHIEN_BAN` kèm `ban_moi: ToChuc` |
| `GET /api/v2/to-chuc/:id/thanh-vien` | thành viên | — | `ThanhVien[]` (quản trị → văn thư → cán bộ) |
| `PATCH /api/v2/to-chuc/:id/thanh-vien/:uid` | quản trị | `{ vai_tro }` | `ThanhVien` · `QUAN_TRI_CUOI` |
| `DELETE /api/v2/to-chuc/:id/thanh-vien/:uid` | quản trị (hoặc người tự rời) | — | `null` · `QUAN_TRI_CUOI` |
| `POST /api/v2/to-chuc/:id/roi` | thành viên | `{}` | `null` · `QUAN_TRI_CUOI` |
| `POST /api/v2/to-chuc/:id/loi-moi` | quản trị | `{ email, vai_tro }` | `LoiMoi` (mã chỉ đi trong thư) · `DA_LA_THANH_VIEN`, `DU_THANH_VIEN`, `CHUA_GUI_DUOC_THU`, 429 |
| `GET /api/v2/to-chuc/:id/loi-moi` | quản trị | — | `LoiMoi[]` (50 gần nhất) |
| `DELETE /api/v2/to-chuc/:id/loi-moi/:lid` | quản trị | — | `null` |
| `POST /api/v2/loi-moi/nhan` | đã đăng nhập | `{ ma }` (43 ký tự base64url) | `{ to_chuc: ToChucTomTat }` · `LOI_MOI_KHONG_HOP_LE`, `SAI_EMAIL_MOI`, `EMAIL_CHUA_XAC_MINH`, `DU_THANH_VIEN`, 429 |
| `GET /api/v2/goi-to-chuc` | công khai | — | `GoiToChuc[]` (cả gói chưa bán, `dang_ban` cho biết) |

Thư mời dẫn tới `https://app.vanbanplus.giakiemso.com/loi-moi?ma=<ma>`.

**Kho trong ngữ cảnh tổ chức** (đọc header): mọi route `/api/v2/kho/*`, `/api/v2/mau*`, `/api/v2/toi/co-quan`,
`/api/v2/credit`, `/api/v2/tac-vu/*` làm việc trên kho / ví / Cơ quan của tổ chức. Khác cá nhân:

- `VanBan` thêm `cong_khai_noi_bo: boolean` (chỉ có ở kho tổ chức) và `sua_duoc: boolean` (người gọi sửa / xóa / gắn tệp
  được không). Thân `POST|PATCH /kho/van-ban` nhận thêm `cong_khai_noi_bo: boolean`.
- `GET /kho/dung-luong` thêm `het_goi: boolean` (kho tổ chức chưa có gói / gói hết hạn → `chi_doc: true`); kho cá nhân luôn
  `het_goi: false`.
- Cán bộ: văn bản ngoài phạm vi → 404; ghi thư mục / lịch họp / album / mẫu riêng → 403 `KHONG_DU_QUYEN`; sửa / xóa văn bản
  người khác tạo → 403 `KHONG_DU_QUYEN`.
- `GET /toi/co-quan` → `{ co_quan: CoQuan | null }` của tổ chức; `PUT` chỉ quản trị (403 `KHONG_DU_QUYEN`).
- Tác vụ AI trừ ví tổ chức; gói hết hạn → 403 `GOI_TO_CHUC_HET_HAN`.

**Giao việc** (đọc header, chỉ kho tổ chức)

| Đường | Ai | Thân gửi | `data` trả |
| --- | --- | --- | --- |
| `GET /api/v2/kho/van-ban/:id/giao-viec` | ai xem được văn bản | — | `{ muc: GiaoViec[], nhat_ky: NhatKyGiaoViec[], quyen: { giao: boolean } }` |
| `POST /api/v2/kho/van-ban/:id/giao-viec` | quản trị, văn thư | `{ nguoi_nhan, vai, han_xu_ly?, y_kien_chi_dao? }` (thiếu `han_xu_ly` → lấy hạn của văn bản) | `GiaoViec` · `CHU_TRI_DA_CO`, `DA_GIAO`, `NGUOI_NHAN_KHONG_HOP_LE` |
| `PATCH /api/v2/kho/giao-viec/:gid` | quản trị, văn thư: mọi trường · cán bộ: `trang_thai`, `ghi_chu` của việc được giao cho chính người đó | `{ phien_ban, trang_thai?, ghi_chu?, han_xu_ly?, y_kien_chi_dao?, vai?, nguoi_nhan? }` | `GiaoViec` · `XUNG_DOT_PHIEN_BAN` (`ban_moi: GiaoViec`) |
| `DELETE /api/v2/kho/giao-viec/:gid` | quản trị, văn thư | — | `null` (thu hồi, có nhật ký) |
| `GET /api/v2/kho/giao-viec/cua-toi?trang_thai=cho,dang&qua_han=1&trang=1&so_dong=20` | thành viên | — | `{ muc: ViecDuocGiao[], tong: number }` (chưa xong trước, hạn gần trước); kho cá nhân trả `{ muc: [], tong: 0 }` |

**Đơn hàng** (`POST /api/orders`, Bearer + CORS như hiện nay)

| Thân | Ai | Ghi chú |
| --- | --- | --- |
| `{ loai: "goi_to_chuc", goi_to_chuc_ma, to_chuc_id }` | quản trị | Trả năm; gói phải `dang_ban` + có giá |
| `{ goiNap, to_chuc_id }` | quản trị | Lô nạp vào ví tổ chức |
| `{ loai: "dung_luong", goi_dung_luong_ma, so_thang, to_chuc_id }` | quản trị | Dung lượng cộng vào kho tổ chức |

Đơn trả về (`orderView`) thêm `toChucId: string | null`, `goiToChucMa: string | null`; `loai` thêm `"goi_to_chuc"`;
`planName` của đơn gói tổ chức: `Gói tổ chức <tên gói> · 1 năm`. Người không phải quản trị: 403 `KHONG_DU_QUYEN`
(không thuộc tổ chức: 403 `KHONG_THUOC_TO_CHUC`).

## Cấu trúc tệp

| Tệp | Trách nhiệm |
| --- | --- |
| `supabase/migrations/017_to_chuc.sql` (mới) | Bảng `goi_to_chuc`, `to_chuc`, `to_chuc_thanh_vien`, `to_chuc_loi_moi`, `to_chuc_co_quan`; hàm tổ chức / thành viên / lời mời / Cơ quan; `kho_han_muc` + `kho_dung_luong_xem` cho tổ chức |
| `supabase/migrations/018_giao_viec.sql` (mới) | `kho_van_ban.cong_khai_noi_bo`; bảng `kho_giao_viec` + nhật ký; `kho_vai_tro`, `kho_van_ban_xem_duoc/sua_duoc`; bản `kho_van_ban_*`, `kho_tep_*` có `p_nguoi` + lớp gọi cũ; hàm giao việc |
| `supabase/migrations/019_vi_credit.sql` (mới) | Cột ví ở `credit_lo`, `tac_vu_chay`; hàm ví tổ chức; `credit_chot`, `credit_hoan` rẽ theo ví |
| `supabase/migrations/020_don_to_chuc.sql` (mới) | `orders.to_chuc_id`, `goi_to_chuc_ma`, loại `goi_to_chuc`, trigger quyền; `to_chuc_cap_goi`; `credit_cap_nap`, `kho_cap_dung_luong`, `pay_order` có nhánh tổ chức |
| `tests/ho-tro/csdl-thu.ts` | `TEP_LUOC_DO` thêm 016–020 |
| `tests/ho-tro/to-chuc.ts` (mới) | Trợ giúp kiểm: tạo tổ chức, thành viên, gói, người đã xác minh, bộ ba vai trò |
| `src/lib/kho/chu-so-huu.ts` | `VaiTro`, `Chu` thêm `nguoi?`, `vai_tro?`; `chuToChuc`, `thamSoNguoi`, `laQuanLy` |
| `src/lib/to-chuc/ngu-canh.ts` (mới) | `DAU_TO_CHUC`, `docNguCanh` |
| `src/lib/to-chuc/loi.ts` (mới) | `LOI_TO_CHUC`, `tuLoiSqlToChuc` |
| `src/lib/to-chuc/kiem-dau-vao.ts` (mới) | `kiemToChuc`, `kiemLoiMoi`, `kiemMaMoi`, `kiemGiaoViec`, `docLocViec` |
| `src/lib/to-chuc/to-chuc.ts` (mới) | Dịch vụ tổ chức + thành viên + gói |
| `src/lib/to-chuc/loi-moi.ts` (mới) | Dịch vụ lời mời (mã, thư, giới hạn tốc độ) |
| `src/lib/to-chuc/giao-viec.ts` (mới) | Dịch vụ giao việc |
| `src/lib/to-chuc/vi.ts` (mới) | `Vi`, `chonVi` |
| `src/lib/to-chuc/don.ts` (mới) | `docGoiToChuc`, `kiemQuyenDonToChuc`, `goiBanDuoc` |
| `src/lib/to-chuc/phu-thuoc.ts` (mới) | `PhuThuocToChuc` + bản thật |
| `src/lib/kho/tra-loi.ts` | `canNguoiDung(req, { chiQuanLy })` đọc header; `canTaiKhoan` |
| `src/lib/kho/loi.ts` | `tuLoiSql` thêm `KHONG_DU_QUYEN` |
| `src/lib/kho/kiem-dau-vao.ts` | `kiemVanBan` nhận `cong_khai_noi_bo` |
| `src/lib/kho/van-ban.ts`, `tep.ts`, `gan-tep.ts`, `han-muc.ts`, `co-quan.ts` | Truyền `p_nguoi` cho kho tổ chức; `het_goi`; Cơ quan theo ngữ cảnh |
| `src/lib/phien/cors.ts` | `DAU` thêm `X-VBP-To-Chuc` |
| `src/lib/credit/so-du.ts`, `src/lib/tac-vu/chay-tac-vu.ts` | Ví trong `mo` / `soDu`; mã `GOI_TO_CHUC_HET_HAN` |
| `src/lib/orders.ts`, `src/app/api/orders/route.ts`, `src/app/api/webhooks/sepay/route.ts` | Đơn tổ chức |
| `src/app/api/v2/to-chuc/**`, `src/app/api/v2/loi-moi/nhan/route.ts`, `src/app/api/v2/goi-to-chuc/route.ts`, `src/app/api/v2/kho/van-ban/[id]/giao-viec/route.ts`, `src/app/api/v2/kho/giao-viec/**` (mới) | Route mỏng |
| `src/app/api/v2/kho/{thu-muc,cuoc-hop,album}/**`, `src/app/api/v2/mau/rieng/**` | Route ghi thêm `{ chiQuanLy: true }` |
| `src/app/api/v2/toi/co-quan/route.ts`, `src/app/api/v2/credit/route.ts`, `src/app/api/v2/tac-vu/[ten]/route.ts`, `…/uoc-tinh/route.ts` | Đọc ngữ cảnh |
| `tests/to-chuc-csdl.test.ts`, `tests/giao-viec-csdl.test.ts`, `tests/vi-credit-csdl.test.ts`, `tests/don-to-chuc-csdl.test.ts`, `tests/to-chuc-thuan.test.ts`, `tests/to-chuc-dich-vu.test.ts`, `tests/chay-tac-vu-vi.test.ts`, `tests/to-chuc-tan-cong.test.ts` (mới) | Bộ kiểm |

---

### Task 1: Worktree, CSDL thử riêng, migration 017 phần A — gói và bảng tổ chức

**Files:**
- Create: `supabase/migrations/017_to_chuc.sql`
- Create: `tests/ho-tro/to-chuc.ts`
- Create: `tests/to-chuc-csdl.test.ts`
- Modify: `tests/ho-tro/csdl-thu.ts` (`TEP_LUOC_DO`)

**Interfaces:**
- Produces: bảng `goi_to_chuc(ma, ten, so_thanh_vien_toi_da, credit_thang, dung_luong_mb, max_file_size_mb, gia_nam_vnd, dang_ban, thu_tu)`,
  `to_chuc`, `to_chuc_thanh_vien`, `to_chuc_loi_moi`, `to_chuc_co_quan`; trợ giúp kiểm `taoToChucThu`, `themThanhVien`,
  `datGoiToChuc`, `nguoiXacMinh`, `boToChuc`, `chuTC`.

- [ ] **Step 1: Tạo worktree + CSDL thử riêng**

```powershell
cd D:/AIVanBanCaNhan/vanbanplus-api
git fetch origin
git worktree add -b dot-b-to-chuc D:/AIVanBanCaNhan/_wt-b-to-chuc-api origin/main
New-Item -ItemType Junction -Path D:/AIVanBanCaNhan/_wt-b-to-chuc-api/node_modules -Target D:/AIVanBanCaNhan/vanbanplus-api/node_modules
```

```bash
ssh gikidi-dev "sudo docker rm -f vbp-csdl-thu-dot-b >/dev/null 2>&1 || true
  sudo docker run -d --name vbp-csdl-thu-dot-b -e POSTGRES_PASSWORD=thu -p 127.0.0.1:55996:5432 supabase/postgres:17.6.1.155 >/dev/null
  for i in \$(seq 1 60); do PGPASSWORD=thu psql -h 127.0.0.1 -p 55996 -U postgres -Atc 'select 1' >/dev/null 2>&1 && sleep 5 && exit 0; sleep 1; done; exit 1"
```

Mở đường hầm ở cửa sổ riêng (chạy nền suốt phiên): `ssh -N -L 55996:127.0.0.1:55996 gikidi-dev`. Kiểm nền xanh trước khi
sửa gì: `$env:TEST_DATABASE_URL="postgres://postgres:thu@127.0.0.1:55996/postgres"; npm test` → toàn bộ PASS.

- [ ] **Step 2: Viết `supabase/migrations/017_to_chuc.sql` (phần A)**

```sql
-- ============================================================
-- 017: Tổ chức thuê VanBanPlus (spec đợt B §2.1, §2.4)
-- Bảng mới: RLS bật, không policy, thu hồi mọi quyền kể cả service_role; API chỉ đi qua hàm SECURITY DEFINER trả jsonb.
-- Khóa dòng to_chuc bằng FOR NO KEY UPDATE (không chặn FOR KEY SHARE của khóa ngoại thành viên / lô credit).
-- Thứ tự khóa chung: orders → users → to_chuc → bảng con.
-- ============================================================

SET lock_timeout = '5s';

-- ---------- gói tổ chức (trả năm) ----------
CREATE TABLE IF NOT EXISTS goi_to_chuc (
    ma TEXT PRIMARY KEY CHECK (ma ~ '^[a-z0-9-]{2,40}$'),
    ten TEXT NOT NULL CHECK (char_length(ten) BETWEEN 1 AND 100),
    so_thanh_vien_toi_da INT NOT NULL CHECK (so_thanh_vien_toi_da > 0),
    credit_thang INT NOT NULL CHECK (credit_thang >= 0),
    dung_luong_mb INT NOT NULL CHECK (dung_luong_mb >= 0),
    max_file_size_mb INT NOT NULL CHECK (max_file_size_mb > 0),
    gia_nam_vnd INT CHECK (gia_nam_vnd > 0),
    dang_ban BOOLEAN NOT NULL DEFAULT FALSE,
    thu_tu INT NOT NULL DEFAULT 0,
    CONSTRAINT ck_goi_to_chuc_ban CHECK (NOT dang_ban OR gia_nam_vnd IS NOT NULL)
);
-- Số giữ chỗ: chủ dự án chốt giá, credit, dung lượng rồi mới bật dang_ban. Chạy lại không đè giá trị đã sửa tay.
INSERT INTO goi_to_chuc (ma, ten, so_thanh_vien_toi_da, credit_thang, dung_luong_mb, max_file_size_mb, gia_nam_vnd, dang_ban, thu_tu)
VALUES ('tc-10', 'Tổ chức 10 người', 10, 3000, 51200, 10, NULL, FALSE, 1),
       ('tc-30', 'Tổ chức 30 người', 30, 8000, 153600, 10, NULL, FALSE, 2),
       ('tc-100', 'Tổ chức 100 người', 100, 25000, 512000, 10, NULL, FALSE, 3)
ON CONFLICT (ma) DO NOTHING;

-- ---------- tổ chức ----------
CREATE TABLE IF NOT EXISTS to_chuc (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ten TEXT NOT NULL CHECK (char_length(ten) BETWEEN 1 AND 200),
    mst TEXT CHECK (mst IS NULL OR mst ~ '^[0-9]{10}(-[0-9]{3})?$'),
    dia_chi TEXT CHECK (char_length(dia_chi) <= 300),
    email_hoa_don TEXT CHECK (char_length(email_hoa_don) <= 254),
    goi_ma TEXT REFERENCES goi_to_chuc(ma),
    goi_bat_dau TIMESTAMPTZ,
    goi_het_han TIMESTAMPTZ,
    tao_boi UUID REFERENCES users(id) ON DELETE SET NULL,
    tao_luc TIMESTAMPTZ NOT NULL DEFAULT now(),
    sua_luc TIMESTAMPTZ NOT NULL DEFAULT now(),
    phien_ban INT NOT NULL DEFAULT 1,
    CONSTRAINT ck_to_chuc_goi CHECK ((goi_ma IS NULL AND goi_bat_dau IS NULL AND goi_het_han IS NULL)
                                  OR (goi_ma IS NOT NULL AND goi_bat_dau IS NOT NULL AND goi_het_han > goi_bat_dau))
);
CREATE INDEX IF NOT EXISTS idx_to_chuc_tao_boi ON to_chuc(tao_boi);

CREATE TABLE IF NOT EXISTS to_chuc_thanh_vien (
    to_chuc_id UUID NOT NULL REFERENCES to_chuc(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    vai_tro TEXT NOT NULL CHECK (vai_tro IN ('quan_tri', 'van_thu', 'can_bo')),
    trang_thai TEXT NOT NULL DEFAULT 'hoat_dong' CHECK (trang_thai IN ('hoat_dong', 'da_roi')),
    them_luc TIMESTAMPTZ NOT NULL DEFAULT now(),
    them_boi UUID,
    sua_luc TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (to_chuc_id, user_id)
);
CREATE INDEX IF NOT EXISTS idx_to_chuc_thanh_vien_user ON to_chuc_thanh_vien(user_id) WHERE trang_thai = 'hoat_dong';

-- Mã mời 32 byte ngẫu nhiên do API sinh; CSDL chỉ giữ SHA-256 (hex).
CREATE TABLE IF NOT EXISTS to_chuc_loi_moi (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    to_chuc_id UUID NOT NULL REFERENCES to_chuc(id) ON DELETE CASCADE,
    email_thuong TEXT NOT NULL CHECK (email_thuong = lower(btrim(email_thuong)) AND char_length(email_thuong) BETWEEN 3 AND 254),
    vai_tro TEXT NOT NULL CHECK (vai_tro IN ('quan_tri', 'van_thu', 'can_bo')),
    ma_bam TEXT NOT NULL UNIQUE CHECK (ma_bam ~ '^[0-9a-f]{64}$'),
    het_han TIMESTAMPTZ NOT NULL,
    moi_boi UUID,
    tao_luc TIMESTAMPTZ NOT NULL DEFAULT now(),
    dung_luc TIMESTAMPTZ,
    dung_boi UUID,
    huy_luc TIMESTAMPTZ
);
CREATE INDEX IF NOT EXISTS idx_to_chuc_loi_moi_to_chuc ON to_chuc_loi_moi(to_chuc_id, tao_luc DESC);

-- Hồ sơ Cơ quan của tổ chức: cùng dạng nguoi_dung_co_quan (015).
CREATE TABLE IF NOT EXISTS to_chuc_co_quan (
    to_chuc_id UUID PRIMARY KEY REFERENCES to_chuc(id) ON DELETE CASCADE,
    du_lieu JSONB NOT NULL CHECK (jsonb_typeof(du_lieu) = 'object' AND octet_length(du_lieu::text) <= 8192),
    sua_luc TIMESTAMPTZ NOT NULL DEFAULT now()
);

ALTER TABLE goi_to_chuc ENABLE ROW LEVEL SECURITY;
ALTER TABLE to_chuc ENABLE ROW LEVEL SECURITY;
ALTER TABLE to_chuc_thanh_vien ENABLE ROW LEVEL SECURITY;
ALTER TABLE to_chuc_loi_moi ENABLE ROW LEVEL SECURITY;
ALTER TABLE to_chuc_co_quan ENABLE ROW LEVEL SECURITY;
REVOKE ALL ON goi_to_chuc, to_chuc, to_chuc_thanh_vien, to_chuc_loi_moi, to_chuc_co_quan
    FROM PUBLIC, anon, authenticated, service_role;
```

- [ ] **Step 3: Thêm 016 + 017 vào `TEP_LUOC_DO`**

Trong `tests/ho-tro/csdl-thu.ts`, cuối mảng `TEP_LUOC_DO` (sau `"migrations/015_mo_ban.sql",`) thêm:

```ts
  "migrations/016_gioi_han_tep_goi.sql",
  "migrations/017_to_chuc.sql",
```

- [ ] **Step 4: Viết trợ giúp kiểm `tests/ho-tro/to-chuc.ts`**

```ts
import type { Client } from "pg";
import { taoNguoiDung } from "./csdl-thu";

export type VaiTroThu = "quan_tri" | "van_thu" | "can_bo";
const NGAY = 86_400_000;

/** Tham số chủ sở hữu tổ chức cho các hàm kho_*. */
export const chuTC = (toChuc: string) => ({ p_chu_loai: "to_chuc", p_chu_id: toChuc });

/** Tạo tổ chức thẳng bằng SQL (không qua to_chuc_tao) + người tạo là quản trị. */
export async function taoToChucThu(c: Client, quanTri: string, ten = "UBND xã Thử"): Promise<string> {
  const { rows } = await c.query(`INSERT INTO to_chuc (ten, tao_boi) VALUES ($1, $2) RETURNING id`, [ten, quanTri]);
  await themThanhVien(c, rows[0].id, quanTri, "quan_tri");
  return rows[0].id as string;
}

export async function themThanhVien(
  c: Client, toChuc: string, user: string, vai: VaiTroThu, trangThai: "hoat_dong" | "da_roi" = "hoat_dong"
): Promise<void> {
  await c.query(
    `INSERT INTO to_chuc_thanh_vien (to_chuc_id, user_id, vai_tro, trang_thai) VALUES ($1, $2, $3, $4)
     ON CONFLICT (to_chuc_id, user_id) DO UPDATE SET vai_tro = EXCLUDED.vai_tro, trang_thai = EXCLUDED.trang_thai`,
    [toChuc, user, vai, trangThai]
  );
}

/** Đặt gói cho tổ chức. Mặc định bắt đầu hôm qua, hết hạn sau 365 ngày; truyền het trong quá khứ để giả lập hết hạn. */
export async function datGoiToChuc(
  c: Client, toChuc: string, ma = "tc-10",
  batDau = new Date(Date.now() - NGAY), het = new Date(Date.now() + 365 * NGAY)
): Promise<void> {
  await c.query(`UPDATE to_chuc SET goi_ma = $2, goi_bat_dau = $3, goi_het_han = $4 WHERE id = $1`,
    [toChuc, ma, batDau.toISOString(), het.toISOString()]);
}

export async function hetGoiToChuc(c: Client, toChuc: string): Promise<void> {
  await datGoiToChuc(c, toChuc, "tc-10", new Date(Date.now() - 400 * NGAY), new Date(Date.now() - 35 * NGAY));
}

/** Người dùng đã xác minh email (nhận được lời mời). */
export async function nguoiXacMinh(c: Client): Promise<{ id: string; email: string }> {
  const id = await taoNguoiDung(c);
  const { rows } = await c.query(`UPDATE users SET email_xac_minh_luc = now() WHERE id = $1 RETURNING email`, [id]);
  return { id, email: rows[0].email as string };
}

/** Một tổ chức có gói tc-10 còn hạn: quản trị, văn thư, hai cán bộ. */
export async function boToChuc(c: Client) {
  const quanTri = await taoNguoiDung(c);
  const vanThu = await taoNguoiDung(c);
  const canBo = await taoNguoiDung(c);
  const canBo2 = await taoNguoiDung(c);
  const toChuc = await taoToChucThu(c, quanTri);
  await themThanhVien(c, toChuc, vanThu, "van_thu");
  await themThanhVien(c, toChuc, canBo, "can_bo");
  await themThanhVien(c, toChuc, canBo2, "can_bo");
  await datGoiToChuc(c, toChuc);
  return { toChuc, quanTri, vanThu, canBo, canBo2 };
}
```

- [ ] **Step 5: Viết bài kiểm phần A `tests/to-chuc-csdl.test.ts`**

```ts
import { after, before, describe, test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { Client } from "pg";
import { CO_CSDL_THU, taoCsdlThu, taoNguoiDung } from "./ho-tro/csdl-thu";

describe("017 tổ chức: bảng, quyền, gói mẫu", { skip: !CO_CSDL_THU }, () => {
  let csdl: Awaited<ReturnType<typeof taoCsdlThu>>;
  let c: Client;

  before(async () => {
    csdl = await taoCsdlThu();
    c = new Client({ connectionString: csdl.url });
    await c.connect();
  });
  after(async () => {
    await c?.end();
    await csdl?.xoa();
  });

  test("bảng tổ chức không ai đọc / ghi được trừ chủ bảng (kể cả service_role)", async () => {
    const loi: string[] = [];
    for (const b of ["goi_to_chuc", "to_chuc", "to_chuc_thanh_vien", "to_chuc_loi_moi", "to_chuc_co_quan"]) {
      for (const v of ["anon", "authenticated", "service_role"]) {
        for (const q of ["SELECT", "INSERT", "UPDATE", "DELETE"]) {
          const { rows } = await c.query("SELECT has_table_privilege($1, $2, $3) AS co", [v, `public.${b}`, q]);
          if (rows[0].co) loi.push(`${v} có ${q} trên ${b}`);
        }
      }
    }
    assert.deepEqual(loi, []);
  });

  test("gói mẫu: ba bậc 10 / 30 / 100, chưa có giá, chưa bán", async () => {
    const { rows } = await c.query(`SELECT ma, so_thanh_vien_toi_da, gia_nam_vnd, dang_ban FROM goi_to_chuc ORDER BY thu_tu`);
    assert.deepEqual(rows.map((r) => [r.ma, r.so_thanh_vien_toi_da, r.gia_nam_vnd, r.dang_ban]), [
      ["tc-10", 10, null, false], ["tc-30", 30, null, false], ["tc-100", 100, null, false],
    ]);
    await assert.rejects(c.query(`UPDATE goi_to_chuc SET dang_ban = TRUE WHERE ma = 'tc-10'`), /ck_goi_to_chuc_ban/);
  });

  test("to_chuc: gói phải đủ ba cột hoặc trống cả ba; mã số thuế đúng dạng", async () => {
    const u = await taoNguoiDung(c);
    await assert.rejects(c.query(`INSERT INTO to_chuc (ten, tao_boi, goi_ma) VALUES ('A', $1, 'tc-10')`, [u]), /ck_to_chuc_goi/);
    await assert.rejects(c.query(`INSERT INTO to_chuc (ten, tao_boi, mst) VALUES ('A', $1, '12ab')`, [u]), /mst/);
    await c.query(`INSERT INTO to_chuc (ten, tao_boi, mst) VALUES ('A', $1, '0312345678-001')`, [u]);
  });

  test("lời mời: email phải viết thường, mã băm đúng 64 ký tự hex", async () => {
    const u = await taoNguoiDung(c);
    const { rows } = await c.query(`INSERT INTO to_chuc (ten, tao_boi) VALUES ('B', $1) RETURNING id`, [u]);
    const tc = rows[0].id;
    await assert.rejects(c.query(`INSERT INTO to_chuc_loi_moi (to_chuc_id, email_thuong, vai_tro, ma_bam, het_han)
      VALUES ($1, 'A@b.vn', 'can_bo', repeat('a', 64), now())`, [tc]));
    await assert.rejects(c.query(`INSERT INTO to_chuc_loi_moi (to_chuc_id, email_thuong, vai_tro, ma_bam, het_han)
      VALUES ($1, 'a@b.vn', 'can_bo', 'xyz', now())`, [tc]));
  });

  test("chạy lại 017 lần hai không lỗi", async () => {
    await c.query(readFileSync(join(__dirname, "..", "supabase", "migrations", "017_to_chuc.sql"), "utf8"));
  });
});
```

- [ ] **Step 6: Chạy kiểm**

Run: `$env:TEST_DATABASE_URL="postgres://postgres:thu@127.0.0.1:55996/postgres"; npx tsx --test tests/to-chuc-csdl.test.ts`
Expected: 5 PASS. Rồi `npm test` (cùng biến) — toàn bộ cũ vẫn xanh (016 nạp thêm không đổi bài nào).

- [ ] **Step 7: Commit**

```bash
git add supabase/migrations/017_to_chuc.sql tests/ho-tro/csdl-thu.ts tests/ho-tro/to-chuc.ts tests/to-chuc-csdl.test.ts
git commit -m "feat(to-chuc): migration 017 phan A - goi to chuc, to chuc, thanh vien, loi moi, co quan

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 2: Migration 017 phần B — hàm tổ chức, thành viên, lời mời, Cơ quan

**Files:**
- Modify: `supabase/migrations/017_to_chuc.sql` (nối cuối tệp)
- Test: `tests/to-chuc-csdl.test.ts` (thêm `describe`)

**Interfaces:**
- Consumes: bảng Task 1; `kho_loi_dau_vao(text)` (010).
- Produces (rpc, tham số đặt tên; lỗi `{ loi: MA, ... }`, thành công `{ ok: true, ... }`):
  - `to_chuc_vai_tro(p_to_chuc uuid, p_user uuid) → text | null` (chỉ thành viên `hoat_dong`)
  - `to_chuc_con_goi(t to_chuc) → boolean`, `to_chuc_toi_da(t to_chuc) → int` (nội bộ)
  - `to_chuc_json(t to_chuc, p_vai text, p_day_du boolean) → jsonb` (nội bộ; dạng `ToChucTomTat` / `ToChuc` của hợp đồng)
  - `to_chuc_ds(p_user) → ToChucTomTat[]`
  - `to_chuc_tao(p_user, p_du_lieu) → { ok, to_chuc } | { loi: 'QUA_NHIEU_TO_CHUC' }`
  - `to_chuc_doc(p_to_chuc, p_user) → ToChuc | null`
  - `to_chuc_sua(p_to_chuc, p_user, p_phien_ban, p_du_lieu) → { ok, to_chuc } | { loi }`
  - `to_chuc_thanh_vien_ds(p_to_chuc, p_user) → ThanhVien[] | null`
  - `to_chuc_thanh_vien_sua(p_to_chuc, p_user, p_muc_tieu, p_vai_tro) → { ok, thanh_vien } | { loi }`
  - `to_chuc_thanh_vien_bo(p_to_chuc, p_user, p_muc_tieu) → { ok } | { loi }` (tự rời: `p_muc_tieu = p_user`)
  - `to_chuc_loi_moi_tao(p_to_chuc, p_user, p_email, p_vai_tro, p_ma_bam) → { ok, loi_moi, ten_to_chuc } | { loi }`
  - `to_chuc_loi_moi_ds(p_to_chuc, p_user) → LoiMoi[] | null`, `to_chuc_loi_moi_huy(p_to_chuc, p_user, p_id) → { ok } | { loi }`
  - `to_chuc_loi_moi_nhan(p_user, p_ma_bam) → { ok, to_chuc } | { loi }`
  - `to_chuc_co_quan_doc(p_to_chuc, p_user) → jsonb | null`, `to_chuc_co_quan_ghi(p_to_chuc, p_user, p_du_lieu) → { ok, co_quan } | { loi }`
  - `goi_to_chuc_ds() → GoiToChuc[]`, `to_chuc_dat_goi(...)` không có (gói chỉ đặt qua `pay_order`, Task 8).

- [ ] **Step 1: Viết bài kiểm (thêm vào cuối `tests/to-chuc-csdl.test.ts`)**

```ts
import { createHash, randomBytes } from "node:crypto";
import { rpcPg } from "./ho-tro/rpc-pg";
import { boToChuc, datGoiToChuc, nguoiXacMinh, taoToChucThu, themThanhVien } from "./ho-tro/to-chuc";
import type { GoiRpc } from "../src/lib/phien/rpc";

const bam = (s: string) => createHash("sha256").update(s, "utf8").digest("hex");
const maMoi = () => randomBytes(32).toString("base64url");

describe("017 tổ chức: hàm tổ chức, thành viên, lời mời, Cơ quan", { skip: !CO_CSDL_THU }, () => {
  let csdl: Awaited<ReturnType<typeof taoCsdlThu>>;
  let c: Client;
  let rpc: GoiRpc;

  before(async () => {
    csdl = await taoCsdlThu();
    c = new Client({ connectionString: csdl.url });
    await c.connect();
    rpc = rpcPg(c);
  });
  after(async () => {
    await c?.end();
    await csdl?.xoa();
  });

  test("tạo: người tạo là quản trị; tối đa 3 tổ chức chưa có gói; có gói thì không tính", async () => {
    const u = await taoNguoiDung(c);
    const ids: string[] = [];
    for (const ten of ["A", "B", "C"]) {
      const r = await rpc("to_chuc_tao", { p_user: u, p_du_lieu: { ten } });
      assert.equal(r.ok, true);
      assert.equal(r.to_chuc.vai_tro, "quan_tri");
      assert.equal(r.to_chuc.con_hieu_luc, false);
      ids.push(r.to_chuc.id);
    }
    assert.deepEqual(await rpc("to_chuc_tao", { p_user: u, p_du_lieu: { ten: "D" } }), { loi: "QUA_NHIEU_TO_CHUC" });
    await datGoiToChuc(c, ids[0]);
    assert.equal((await rpc("to_chuc_tao", { p_user: u, p_du_lieu: { ten: "D" } })).ok, true);
    const ds = await rpc("to_chuc_ds", { p_user: u });
    assert.equal(ds.length, 4);
    assert.equal(ds.find((t: any) => t.id === ids[0]).con_hieu_luc, true);
  });

  test("vai trò: chỉ thành viên hoạt động; đã rời thì null", async () => {
    const b = await boToChuc(c);
    assert.equal(await rpc("to_chuc_vai_tro", { p_to_chuc: b.toChuc, p_user: b.canBo }), "can_bo");
    await themThanhVien(c, b.toChuc, b.canBo, "can_bo", "da_roi");
    assert.equal(await rpc("to_chuc_vai_tro", { p_to_chuc: b.toChuc, p_user: b.canBo }), null);
    assert.equal(await rpc("to_chuc_vai_tro", { p_to_chuc: b.toChuc, p_user: await taoNguoiDung(c) }), null);
  });

  test("đọc / sửa hồ sơ: người ngoài null, cán bộ không sửa, quản trị sửa kèm Cơ quan, sai phiên bản → xung đột", async () => {
    const b = await boToChuc(c);
    assert.equal(await rpc("to_chuc_doc", { p_to_chuc: b.toChuc, p_user: await taoNguoiDung(c) }), null);
    const doc = await rpc("to_chuc_doc", { p_to_chuc: b.toChuc, p_user: b.canBo });
    assert.equal(doc.so_thanh_vien, 4);
    assert.equal(doc.so_thanh_vien_toi_da, 10);
    assert.equal(doc.co_quan, null);
    const cq = { ten_co_quan: "UBND xã Thử", xa: "Xã Thử", tinh: "Tỉnh Thử", dac_diem: [], nguoi_ky: "A", chuc_danh: "Chủ tịch" };
    assert.deepEqual(await rpc("to_chuc_sua", { p_to_chuc: b.toChuc, p_user: b.canBo, p_phien_ban: 1, p_du_lieu: { ten: "X" } }),
      { loi: "KHONG_DU_QUYEN" });
    const s = await rpc("to_chuc_sua", { p_to_chuc: b.toChuc, p_user: b.quanTri, p_phien_ban: 1, p_du_lieu: { ten: "UBND xã Mới", co_quan: cq } });
    assert.equal(s.to_chuc.ten, "UBND xã Mới");
    assert.deepEqual(s.to_chuc.co_quan, cq);
    assert.equal(s.to_chuc.phien_ban, 2);
    const x = await rpc("to_chuc_sua", { p_to_chuc: b.toChuc, p_user: b.quanTri, p_phien_ban: 1, p_du_lieu: { ten: "Y" } });
    assert.equal(x.loi, "XUNG_DOT_PHIEN_BAN");
    assert.equal(x.ban_moi.ten, "UBND xã Mới");
    assert.deepEqual(await rpc("to_chuc_co_quan_doc", { p_to_chuc: b.toChuc, p_user: b.canBo }), cq);
    assert.equal(await rpc("to_chuc_co_quan_doc", { p_to_chuc: b.toChuc, p_user: await taoNguoiDung(c) }), null);
    assert.deepEqual(await rpc("to_chuc_co_quan_ghi", { p_to_chuc: b.toChuc, p_user: b.vanThu, p_du_lieu: cq }), { loi: "KHONG_DU_QUYEN" });
    assert.equal((await rpc("to_chuc_co_quan_ghi", { p_to_chuc: b.toChuc, p_user: b.quanTri, p_du_lieu: cq })).ok, true);
  });

  test("quản trị cuối: không tự rời, không tự hạ, không bị đuổi; có quản trị thứ hai thì được", async () => {
    const b = await boToChuc(c);
    const bo = (ai: string, ai2: string) => rpc("to_chuc_thanh_vien_bo", { p_to_chuc: b.toChuc, p_user: ai, p_muc_tieu: ai2 });
    const sua = (ai: string, ai2: string, vai: string) =>
      rpc("to_chuc_thanh_vien_sua", { p_to_chuc: b.toChuc, p_user: ai, p_muc_tieu: ai2, p_vai_tro: vai });
    assert.deepEqual(await bo(b.quanTri, b.quanTri), { loi: "QUAN_TRI_CUOI" });
    assert.deepEqual(await sua(b.quanTri, b.quanTri, "can_bo"), { loi: "QUAN_TRI_CUOI" });
    assert.deepEqual(await sua(b.canBo, b.canBo, "quan_tri"), { loi: "KHONG_DU_QUYEN" });
    assert.deepEqual(await bo(b.canBo, b.canBo2), { loi: "KHONG_DU_QUYEN" });
    assert.equal((await sua(b.quanTri, b.vanThu, "quan_tri")).thanh_vien.vai_tro, "quan_tri");
    assert.equal((await bo(b.quanTri, b.quanTri)).ok, true);
    assert.equal(await rpc("to_chuc_vai_tro", { p_to_chuc: b.toChuc, p_user: b.quanTri }), null);
    assert.deepEqual(await bo(b.vanThu, b.vanThu), { loi: "QUAN_TRI_CUOI" });
    assert.equal((await bo(b.canBo, b.canBo)).ok, true); // cán bộ tự rời được
    const ds = await rpc("to_chuc_thanh_vien_ds", { p_to_chuc: b.toChuc, p_user: b.vanThu });
    assert.deepEqual(ds.map((t: any) => t.vai_tro), ["quan_tri", "can_bo"]);
  });

  test("hai quản trị hạ nhau cùng lúc: luôn còn ít nhất một quản trị", async () => {
    const b = await boToChuc(c);
    await themThanhVien(c, b.toChuc, b.vanThu, "quan_tri");
    const c2 = new Client({ connectionString: csdl.url });
    await c2.connect();
    try {
      const r2 = rpcPg(c2);
      const kq = await Promise.all([
        rpc("to_chuc_thanh_vien_sua", { p_to_chuc: b.toChuc, p_user: b.quanTri, p_muc_tieu: b.vanThu, p_vai_tro: "can_bo" }),
        r2("to_chuc_thanh_vien_sua", { p_to_chuc: b.toChuc, p_user: b.vanThu, p_muc_tieu: b.quanTri, p_vai_tro: "can_bo" }),
      ]);
      assert.equal(kq.filter((k) => k.ok).length, 1);
      const { rows } = await c.query(`SELECT count(*)::int AS n FROM to_chuc_thanh_vien WHERE to_chuc_id = $1 AND vai_tro = 'quan_tri' AND trang_thai = 'hoat_dong'`, [b.toChuc]);
      assert.equal(rows[0].n, 1);
    } finally {
      await c2.end();
    }
  });

  test("lời mời: tạo, nhận đúng email; sai email, chưa xác minh, dùng lại, đã hủy, hết hạn, đoán mã → từ chối", async () => {
    const b = await boToChuc(c);
    const moi = async (email: string, vai = "can_bo", ai = b.quanTri) => {
      const ma = maMoi();
      const r = await rpc("to_chuc_loi_moi_tao", { p_to_chuc: b.toChuc, p_user: ai, p_email: email, p_vai_tro: vai, p_ma_bam: bam(ma) });
      return { ma, r };
    };
    const nhan = (u: string, ma: string) => rpc("to_chuc_loi_moi_nhan", { p_user: u, p_ma_bam: bam(ma) });

    const khach = await nguoiXacMinh(c);
    assert.deepEqual((await moi(khach.email, "can_bo", b.canBo)).r, { loi: "KHONG_DU_QUYEN" });
    const a = await moi(`  ${khach.email.toUpperCase()} `, "van_thu");
    assert.equal(a.r.loi_moi.email, khach.email);
    assert.equal(a.r.loi_moi.trang_thai, "cho");

    const nguoiKhac = await nguoiXacMinh(c);
    assert.deepEqual(await nhan(nguoiKhac.id, a.ma), { loi: "SAI_EMAIL_MOI" });
    assert.deepEqual(await nhan(khach.id, maMoi()), { loi: "LOI_MOI_KHONG_HOP_LE" });
    const ok = await nhan(khach.id, a.ma);
    assert.equal(ok.to_chuc.vai_tro, "van_thu");
    assert.deepEqual(await nhan(khach.id, a.ma), { loi: "LOI_MOI_KHONG_HOP_LE" }); // dùng lại

    assert.deepEqual((await moi(khach.email)).r, { loi: "DA_LA_THANH_VIEN" });

    const chuaXm = await taoNguoiDung(c);
    const emailChuaXm = (await c.query(`SELECT email FROM users WHERE id = $1`, [chuaXm])).rows[0].email;
    const d = await moi(emailChuaXm);
    assert.deepEqual(await nhan(chuaXm, d.ma), { loi: "EMAIL_CHUA_XAC_MINH" });

    const h = await moi((await nguoiXacMinh(c)).email);
    assert.equal((await rpc("to_chuc_loi_moi_huy", { p_to_chuc: b.toChuc, p_user: b.quanTri, p_id: h.r.loi_moi.id })).ok, true);
    const nguoiH = (await c.query(`SELECT id FROM users WHERE email = $1`, [h.r.loi_moi.email])).rows[0].id;
    assert.deepEqual(await nhan(nguoiH, h.ma), { loi: "LOI_MOI_KHONG_HOP_LE" });

    const tre = await nguoiXacMinh(c);
    const e = await moi(tre.email);
    await c.query(`UPDATE to_chuc_loi_moi SET het_han = now() - interval '1 minute' WHERE id = $1`, [e.r.loi_moi.id]);
    assert.deepEqual(await nhan(tre.id, e.ma), { loi: "LOI_MOI_KHONG_HOP_LE" });

    const ds = await rpc("to_chuc_loi_moi_ds", { p_to_chuc: b.toChuc, p_user: b.quanTri });
    const tt = Object.fromEntries(ds.map((l: any) => [l.id, l.trang_thai]));
    assert.equal(tt[a.r.loi_moi.id], "da_dung");
    assert.equal(tt[h.r.loi_moi.id], "da_huy");
    assert.equal(tt[e.r.loi_moi.id], "het_han");
    assert.equal(await rpc("to_chuc_loi_moi_ds", { p_to_chuc: b.toChuc, p_user: b.canBo }), null);
  });

  test("mời lại cùng email hủy lời mời cũ còn chờ", async () => {
    const b = await boToChuc(c);
    const k = await nguoiXacMinh(c);
    const m1 = maMoi(), m2 = maMoi();
    await rpc("to_chuc_loi_moi_tao", { p_to_chuc: b.toChuc, p_user: b.quanTri, p_email: k.email, p_vai_tro: "can_bo", p_ma_bam: bam(m1) });
    await rpc("to_chuc_loi_moi_tao", { p_to_chuc: b.toChuc, p_user: b.quanTri, p_email: k.email, p_vai_tro: "van_thu", p_ma_bam: bam(m2) });
    assert.deepEqual(await rpc("to_chuc_loi_moi_nhan", { p_user: k.id, p_ma_bam: bam(m1) }), { loi: "LOI_MOI_KHONG_HOP_LE" });
    assert.equal((await rpc("to_chuc_loi_moi_nhan", { p_user: k.id, p_ma_bam: bam(m2) })).to_chuc.vai_tro, "van_thu");
  });

  test("vượt số thành viên: chưa có gói tối đa 5 — từ chối lúc mời và lúc nhận", async () => {
    const qt = await taoNguoiDung(c);
    const tc = await taoToChucThu(c, qt);
    for (let i = 0; i < 3; i++) await themThanhVien(c, tc, await taoNguoiDung(c), "can_bo");
    const k1 = await nguoiXacMinh(c), k2 = await nguoiXacMinh(c);
    const m1 = maMoi(), m2 = maMoi();
    await rpc("to_chuc_loi_moi_tao", { p_to_chuc: tc, p_user: qt, p_email: k1.email, p_vai_tro: "can_bo", p_ma_bam: bam(m1) });
    await rpc("to_chuc_loi_moi_tao", { p_to_chuc: tc, p_user: qt, p_email: k2.email, p_vai_tro: "can_bo", p_ma_bam: bam(m2) });
    assert.equal((await rpc("to_chuc_loi_moi_nhan", { p_user: k1.id, p_ma_bam: bam(m1) })).ok, true); // thành viên thứ 5
    assert.deepEqual(await rpc("to_chuc_loi_moi_nhan", { p_user: k2.id, p_ma_bam: bam(m2) }), { loi: "DU_THANH_VIEN" });
    assert.deepEqual(await rpc("to_chuc_loi_moi_tao", { p_to_chuc: tc, p_user: qt, p_email: "moi@vanbanplus.test", p_vai_tro: "can_bo", p_ma_bam: bam(maMoi()) }),
      { loi: "DU_THANH_VIEN" });
  });

  test("thành viên đã rời được mời lại thì hoạt động lại với vai trò mới", async () => {
    const b = await boToChuc(c);
    const k = await nguoiXacMinh(c);
    await themThanhVien(c, b.toChuc, k.id, "van_thu", "da_roi");
    const m = maMoi();
    await rpc("to_chuc_loi_moi_tao", { p_to_chuc: b.toChuc, p_user: b.quanTri, p_email: k.email, p_vai_tro: "can_bo", p_ma_bam: bam(m) });
    assert.equal((await rpc("to_chuc_loi_moi_nhan", { p_user: k.id, p_ma_bam: bam(m) })).to_chuc.vai_tro, "can_bo");
    assert.equal(await rpc("to_chuc_vai_tro", { p_to_chuc: b.toChuc, p_user: k.id }), "can_bo");
  });

  test("gói: danh sách đủ ba bậc kèm dang_ban", async () => {
    const g = await rpc("goi_to_chuc_ds", {});
    assert.deepEqual(g.map((x: any) => x.ma), ["tc-10", "tc-30", "tc-100"]);
    assert.equal("thu_tu" in g[0], false);
  });

  test("quyền hàm: anon / authenticated không gọi được; service_role gọi được hàm API, không gọi được hàm nội bộ", async () => {
    const api = ["to_chuc_vai_tro(uuid,uuid)", "to_chuc_ds(uuid)", "to_chuc_tao(uuid,jsonb)", "to_chuc_doc(uuid,uuid)",
      "to_chuc_sua(uuid,uuid,integer,jsonb)", "to_chuc_thanh_vien_ds(uuid,uuid)", "to_chuc_thanh_vien_sua(uuid,uuid,uuid,text)",
      "to_chuc_thanh_vien_bo(uuid,uuid,uuid)", "to_chuc_loi_moi_tao(uuid,uuid,text,text,text)", "to_chuc_loi_moi_ds(uuid,uuid)",
      "to_chuc_loi_moi_huy(uuid,uuid,uuid)", "to_chuc_loi_moi_nhan(uuid,text)", "to_chuc_co_quan_doc(uuid,uuid)",
      "to_chuc_co_quan_ghi(uuid,uuid,jsonb)", "goi_to_chuc_ds()"];
    const noiBo = ["to_chuc_con_goi(to_chuc)", "to_chuc_toi_da(to_chuc)", "to_chuc_json(to_chuc,text,boolean)",
      "to_chuc_thanh_vien_json(to_chuc_thanh_vien)", "to_chuc_loi_moi_json(to_chuc_loi_moi)", "to_chuc_con_quan_tri_khac(uuid,uuid)"];
    const loi: string[] = [];
    const co = async (v: string, f: string) =>
      (await c.query("SELECT has_function_privilege($1, $2, 'EXECUTE') AS co", [v, f])).rows[0].co as boolean;
    for (const f of [...api, ...noiBo]) for (const v of ["anon", "authenticated"]) if (await co(v, f)) loi.push(`${v} gọi được ${f}`);
    for (const f of api) if (!(await co("service_role", f))) loi.push(`service_role không gọi được ${f}`);
    for (const f of noiBo) if (await co("service_role", f)) loi.push(`service_role gọi được ${f}`);
    assert.deepEqual(loi, []);
  });
});
```

(Gộp các `import` mới lên đầu tệp cùng các `import` sẵn có.)

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx tsx --test tests/to-chuc-csdl.test.ts` (có `TEST_DATABASE_URL`)
Expected: FAIL — `function to_chuc_tao(p_user => uuid, p_du_lieu => jsonb) does not exist`.

- [ ] **Step 3: Nối phần B vào `017_to_chuc.sql`**

```sql
-- ============================================================
-- Phần B: hàm tổ chức, thành viên, lời mời, Cơ quan
-- ============================================================

-- Gói còn hiệu lực (đã bắt đầu, chưa hết hạn).
CREATE OR REPLACE FUNCTION to_chuc_con_goi(t to_chuc) RETURNS BOOLEAN
LANGUAGE sql STABLE SET search_path = public, pg_temp AS $$
    SELECT t.goi_ma IS NOT NULL AND t.goi_bat_dau <= now() AND t.goi_het_han > now();
$$;

-- Số thành viên tối đa: theo gói còn hiệu lực; chưa có gói / hết hạn: 5.
CREATE OR REPLACE FUNCTION to_chuc_toi_da(t to_chuc) RETURNS INT
LANGUAGE sql STABLE SET search_path = public, pg_temp AS $$
    SELECT COALESCE((SELECT g.so_thanh_vien_toi_da FROM goi_to_chuc g WHERE g.ma = t.goi_ma AND to_chuc_con_goi(t)), 5);
$$;

-- Vai trò của người dùng trong tổ chức; NULL nếu không phải thành viên đang hoạt động. Điểm duy nhất xác định quyền.
CREATE OR REPLACE FUNCTION to_chuc_vai_tro(p_to_chuc UUID, p_user UUID) RETURNS TEXT
LANGUAGE sql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT vai_tro FROM to_chuc_thanh_vien
     WHERE to_chuc_id = p_to_chuc AND user_id = p_user AND trang_thai = 'hoat_dong';
$$;

CREATE OR REPLACE FUNCTION to_chuc_json(t to_chuc, p_vai TEXT, p_day_du BOOLEAN) RETURNS JSONB
LANGUAGE sql STABLE SET search_path = public, pg_temp AS $$
    SELECT jsonb_build_object(
               'id', t.id, 'ten', t.ten, 'vai_tro', p_vai, 'goi_ma', t.goi_ma,
               'goi_ten', (SELECT g.ten FROM goi_to_chuc g WHERE g.ma = t.goi_ma),
               'goi_bat_dau', t.goi_bat_dau, 'goi_het_han', t.goi_het_han, 'con_hieu_luc', to_chuc_con_goi(t))
        || CASE WHEN p_day_du THEN jsonb_build_object(
               'mst', t.mst, 'dia_chi', t.dia_chi, 'email_hoa_don', t.email_hoa_don, 'phien_ban', t.phien_ban,
               'so_thanh_vien', (SELECT count(*) FROM to_chuc_thanh_vien v
                                  WHERE v.to_chuc_id = t.id AND v.trang_thai = 'hoat_dong'),
               'so_thanh_vien_toi_da', to_chuc_toi_da(t),
               'co_quan', (SELECT c.du_lieu FROM to_chuc_co_quan c WHERE c.to_chuc_id = t.id))
           ELSE '{}'::jsonb END;
$$;

CREATE OR REPLACE FUNCTION to_chuc_ds(p_user UUID) RETURNS JSONB
LANGUAGE sql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT COALESCE(jsonb_agg(to_chuc_json(t, v.vai_tro, FALSE) ORDER BY t.ten, t.id), '[]'::jsonb)
      FROM to_chuc_thanh_vien v JOIN to_chuc t ON t.id = v.to_chuc_id
     WHERE v.user_id = p_user AND v.trang_thai = 'hoat_dong';
$$;

-- p_du_lieu đã kiểm ở API (src/lib/to-chuc/kiem-dau-vao.ts kiemToChuc): { ten, mst?, dia_chi?, email_hoa_don? }.
CREATE OR REPLACE FUNCTION to_chuc_tao(p_user UUID, p_du_lieu JSONB) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE t to_chuc%ROWTYPE;
BEGIN
    -- Xếp hàng theo người tạo: hai yêu cầu song song không cùng vượt trần 3 tổ chức chưa có gói.
    PERFORM 1 FROM users WHERE id = p_user FOR NO KEY UPDATE;
    IF NOT FOUND THEN RAISE EXCEPTION 'to_chuc_tao: không có người dùng %', p_user; END IF;
    IF (SELECT count(*) FROM to_chuc WHERE tao_boi = p_user AND goi_ma IS NULL) >= 3 THEN
        RETURN jsonb_build_object('loi', 'QUA_NHIEU_TO_CHUC');
    END IF;
    INSERT INTO to_chuc (ten, mst, dia_chi, email_hoa_don, tao_boi)
    VALUES (p_du_lieu->>'ten', p_du_lieu->>'mst', p_du_lieu->>'dia_chi', p_du_lieu->>'email_hoa_don', p_user)
    RETURNING * INTO t;
    INSERT INTO to_chuc_thanh_vien (to_chuc_id, user_id, vai_tro, them_boi) VALUES (t.id, p_user, 'quan_tri', p_user);
    RETURN jsonb_build_object('ok', TRUE, 'to_chuc', to_chuc_json(t, 'quan_tri', FALSE));
END;
$$;

CREATE OR REPLACE FUNCTION to_chuc_doc(p_to_chuc UUID, p_user UUID) RETURNS JSONB
LANGUAGE plpgsql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE t to_chuc%ROWTYPE; v_vai TEXT;
BEGIN
    v_vai := to_chuc_vai_tro(p_to_chuc, p_user);
    IF v_vai IS NULL THEN RETURN NULL; END IF;
    SELECT * INTO t FROM to_chuc WHERE id = p_to_chuc;
    RETURN to_chuc_json(t, v_vai, TRUE);
END;
$$;

-- p_du_lieu đã kiểm ở API: { ten?, mst?, dia_chi?, email_hoa_don?, co_quan? (object đã làm sạch bằng kiemCoQuan) }.
CREATE OR REPLACE FUNCTION to_chuc_sua(p_to_chuc UUID, p_user UUID, p_phien_ban INT, p_du_lieu JSONB) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE t to_chuc%ROWTYPE; v_vai TEXT; p JSONB := p_du_lieu;
BEGIN
    SELECT * INTO t FROM to_chuc WHERE id = p_to_chuc FOR NO KEY UPDATE;
    IF NOT FOUND THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    v_vai := to_chuc_vai_tro(p_to_chuc, p_user);
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF v_vai <> 'quan_tri' THEN RETURN jsonb_build_object('loi', 'KHONG_DU_QUYEN'); END IF;
    IF t.phien_ban <> p_phien_ban THEN
        RETURN jsonb_build_object('loi', 'XUNG_DOT_PHIEN_BAN', 'ban_moi', to_chuc_json(t, v_vai, TRUE));
    END IF;
    UPDATE to_chuc SET
        ten = CASE WHEN p ? 'ten' THEN p->>'ten' ELSE ten END,
        mst = CASE WHEN p ? 'mst' THEN p->>'mst' ELSE mst END,
        dia_chi = CASE WHEN p ? 'dia_chi' THEN p->>'dia_chi' ELSE dia_chi END,
        email_hoa_don = CASE WHEN p ? 'email_hoa_don' THEN p->>'email_hoa_don' ELSE email_hoa_don END,
        sua_luc = now(), phien_ban = phien_ban + 1
     WHERE id = t.id
    RETURNING * INTO t;
    IF jsonb_typeof(p->'co_quan') = 'object' THEN
        INSERT INTO to_chuc_co_quan (to_chuc_id, du_lieu, sua_luc) VALUES (t.id, p->'co_quan', now())
        ON CONFLICT (to_chuc_id) DO UPDATE SET du_lieu = EXCLUDED.du_lieu, sua_luc = now();
    END IF;
    RETURN jsonb_build_object('ok', TRUE, 'to_chuc', to_chuc_json(t, v_vai, TRUE));
END;
$$;

CREATE OR REPLACE FUNCTION to_chuc_co_quan_doc(p_to_chuc UUID, p_user UUID) RETURNS JSONB
LANGUAGE sql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT c.du_lieu FROM to_chuc_co_quan c
     WHERE c.to_chuc_id = p_to_chuc AND to_chuc_vai_tro(p_to_chuc, p_user) IS NOT NULL;
$$;

CREATE OR REPLACE FUNCTION to_chuc_co_quan_ghi(p_to_chuc UUID, p_user UUID, p_du_lieu JSONB) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE v_vai TEXT := to_chuc_vai_tro(p_to_chuc, p_user);
BEGIN
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF v_vai <> 'quan_tri' THEN RETURN jsonb_build_object('loi', 'KHONG_DU_QUYEN'); END IF;
    INSERT INTO to_chuc_co_quan (to_chuc_id, du_lieu, sua_luc) VALUES (p_to_chuc, p_du_lieu, now())
    ON CONFLICT (to_chuc_id) DO UPDATE SET du_lieu = EXCLUDED.du_lieu, sua_luc = now();
    RETURN jsonb_build_object('ok', TRUE, 'co_quan', p_du_lieu);
END;
$$;

-- ---------- thành viên ----------
CREATE OR REPLACE FUNCTION to_chuc_thanh_vien_json(v to_chuc_thanh_vien) RETURNS JSONB
LANGUAGE sql STABLE SET search_path = public, pg_temp AS $$
    SELECT jsonb_build_object('user_id', v.user_id, 'ho_ten', u.full_name, 'email', u.email, 'vai_tro', v.vai_tro,
                              'trang_thai', v.trang_thai, 'them_luc', v.them_luc)
      FROM users u WHERE u.id = v.user_id;
$$;

CREATE OR REPLACE FUNCTION to_chuc_thanh_vien_ds(p_to_chuc UUID, p_user UUID) RETURNS JSONB
LANGUAGE sql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT CASE WHEN to_chuc_vai_tro(p_to_chuc, p_user) IS NULL THEN NULL ELSE COALESCE((
        SELECT jsonb_agg(to_chuc_thanh_vien_json(v)
                         ORDER BY array_position(ARRAY['quan_tri', 'van_thu', 'can_bo'], v.vai_tro), v.them_luc, v.user_id)
          FROM to_chuc_thanh_vien v WHERE v.to_chuc_id = p_to_chuc AND v.trang_thai = 'hoat_dong'), '[]'::jsonb) END;
$$;

-- Còn quản trị hoạt động nào khác p_tru không. Người gọi đã khóa dòng to_chuc.
CREATE OR REPLACE FUNCTION to_chuc_con_quan_tri_khac(p_to_chuc UUID, p_tru UUID) RETURNS BOOLEAN
LANGUAGE sql STABLE SET search_path = public, pg_temp AS $$
    SELECT EXISTS (SELECT 1 FROM to_chuc_thanh_vien
                    WHERE to_chuc_id = p_to_chuc AND user_id <> p_tru AND vai_tro = 'quan_tri' AND trang_thai = 'hoat_dong');
$$;

CREATE OR REPLACE FUNCTION to_chuc_thanh_vien_sua(p_to_chuc UUID, p_user UUID, p_muc_tieu UUID, p_vai_tro TEXT) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE v_vai TEXT; m to_chuc_thanh_vien%ROWTYPE;
BEGIN
    IF p_vai_tro IS NULL OR p_vai_tro NOT IN ('quan_tri', 'van_thu', 'can_bo') THEN
        RETURN kho_loi_dau_vao('Vai trò không hợp lệ.');
    END IF;
    -- Khóa tổ chức trước: mọi thay đổi vai trò / thành viên của một tổ chức xếp hàng (luật «còn ít nhất một quản trị»).
    PERFORM 1 FROM to_chuc WHERE id = p_to_chuc FOR NO KEY UPDATE;
    IF NOT FOUND THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    v_vai := to_chuc_vai_tro(p_to_chuc, p_user);
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF v_vai <> 'quan_tri' THEN RETURN jsonb_build_object('loi', 'KHONG_DU_QUYEN'); END IF;
    SELECT * INTO m FROM to_chuc_thanh_vien
     WHERE to_chuc_id = p_to_chuc AND user_id = p_muc_tieu AND trang_thai = 'hoat_dong' FOR UPDATE;
    IF NOT FOUND THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF m.vai_tro = 'quan_tri' AND p_vai_tro <> 'quan_tri' AND NOT to_chuc_con_quan_tri_khac(p_to_chuc, p_muc_tieu) THEN
        RETURN jsonb_build_object('loi', 'QUAN_TRI_CUOI');
    END IF;
    UPDATE to_chuc_thanh_vien SET vai_tro = p_vai_tro, sua_luc = now()
     WHERE to_chuc_id = p_to_chuc AND user_id = p_muc_tieu
    RETURNING * INTO m;
    RETURN jsonb_build_object('ok', TRUE, 'thanh_vien', to_chuc_thanh_vien_json(m));
END;
$$;

-- Bỏ một thành viên (quản trị) hoặc tự rời (p_muc_tieu = p_user, mọi vai trò). Chỉ đổi trạng thái, không xóa dòng.
CREATE OR REPLACE FUNCTION to_chuc_thanh_vien_bo(p_to_chuc UUID, p_user UUID, p_muc_tieu UUID) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE v_vai TEXT; m to_chuc_thanh_vien%ROWTYPE;
BEGIN
    PERFORM 1 FROM to_chuc WHERE id = p_to_chuc FOR NO KEY UPDATE;
    IF NOT FOUND THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    v_vai := to_chuc_vai_tro(p_to_chuc, p_user);
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF p_muc_tieu <> p_user AND v_vai <> 'quan_tri' THEN RETURN jsonb_build_object('loi', 'KHONG_DU_QUYEN'); END IF;
    SELECT * INTO m FROM to_chuc_thanh_vien
     WHERE to_chuc_id = p_to_chuc AND user_id = p_muc_tieu AND trang_thai = 'hoat_dong' FOR UPDATE;
    IF NOT FOUND THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF m.vai_tro = 'quan_tri' AND NOT to_chuc_con_quan_tri_khac(p_to_chuc, p_muc_tieu) THEN
        RETURN jsonb_build_object('loi', 'QUAN_TRI_CUOI');
    END IF;
    UPDATE to_chuc_thanh_vien SET trang_thai = 'da_roi', sua_luc = now()
     WHERE to_chuc_id = p_to_chuc AND user_id = p_muc_tieu;
    RETURN jsonb_build_object('ok', TRUE);
END;
$$;

-- ---------- lời mời ----------
CREATE OR REPLACE FUNCTION to_chuc_loi_moi_json(l to_chuc_loi_moi) RETURNS JSONB
LANGUAGE sql STABLE SET search_path = public, pg_temp AS $$
    SELECT jsonb_build_object('id', l.id, 'email', l.email_thuong, 'vai_tro', l.vai_tro, 'het_han', l.het_han,
        'tao_luc', l.tao_luc,
        'trang_thai', CASE WHEN l.huy_luc IS NOT NULL THEN 'da_huy' WHEN l.dung_luc IS NOT NULL THEN 'da_dung'
                           WHEN l.het_han <= now() THEN 'het_han' ELSE 'cho' END);
$$;

-- p_ma_bam: SHA-256 hex của mã mời (API sinh 32 byte, mã chỉ đi trong thư). Mời lại cùng email: hủy lời mời cũ còn chờ.
CREATE OR REPLACE FUNCTION to_chuc_loi_moi_tao(p_to_chuc UUID, p_user UUID, p_email TEXT, p_vai_tro TEXT, p_ma_bam TEXT)
RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE t to_chuc%ROWTYPE; v_vai TEXT; v_email TEXT := lower(btrim(p_email)); l to_chuc_loi_moi%ROWTYPE;
BEGIN
    IF p_vai_tro IS NULL OR p_vai_tro NOT IN ('quan_tri', 'van_thu', 'can_bo') THEN
        RETURN kho_loi_dau_vao('Vai trò không hợp lệ.');
    END IF;
    SELECT * INTO t FROM to_chuc WHERE id = p_to_chuc FOR NO KEY UPDATE;
    IF NOT FOUND THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    v_vai := to_chuc_vai_tro(p_to_chuc, p_user);
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF v_vai <> 'quan_tri' THEN RETURN jsonb_build_object('loi', 'KHONG_DU_QUYEN'); END IF;
    IF EXISTS (SELECT 1 FROM to_chuc_thanh_vien v JOIN users u ON u.id = v.user_id
                WHERE v.to_chuc_id = t.id AND v.trang_thai = 'hoat_dong' AND lower(u.email) = v_email) THEN
        RETURN jsonb_build_object('loi', 'DA_LA_THANH_VIEN');
    END IF;
    IF (SELECT count(*) FROM to_chuc_thanh_vien WHERE to_chuc_id = t.id AND trang_thai = 'hoat_dong') >= to_chuc_toi_da(t) THEN
        RETURN jsonb_build_object('loi', 'DU_THANH_VIEN');
    END IF;
    UPDATE to_chuc_loi_moi SET huy_luc = now()
     WHERE to_chuc_id = t.id AND email_thuong = v_email AND huy_luc IS NULL AND dung_luc IS NULL;
    INSERT INTO to_chuc_loi_moi (to_chuc_id, email_thuong, vai_tro, ma_bam, het_han, moi_boi)
    VALUES (t.id, v_email, p_vai_tro, p_ma_bam, now() + interval '7 days', p_user)
    RETURNING * INTO l;
    RETURN jsonb_build_object('ok', TRUE, 'loi_moi', to_chuc_loi_moi_json(l), 'ten_to_chuc', t.ten);
END;
$$;

CREATE OR REPLACE FUNCTION to_chuc_loi_moi_ds(p_to_chuc UUID, p_user UUID) RETURNS JSONB
LANGUAGE sql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT CASE WHEN to_chuc_vai_tro(p_to_chuc, p_user) IS DISTINCT FROM 'quan_tri' THEN NULL ELSE COALESCE((
        SELECT jsonb_agg(to_chuc_loi_moi_json(l) ORDER BY l.tao_luc DESC)
          FROM to_chuc_loi_moi l
         WHERE l.id IN (SELECT id FROM to_chuc_loi_moi WHERE to_chuc_id = p_to_chuc ORDER BY tao_luc DESC LIMIT 50)), '[]'::jsonb) END;
$$;

CREATE OR REPLACE FUNCTION to_chuc_loi_moi_huy(p_to_chuc UUID, p_user UUID, p_id UUID) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE v_vai TEXT := to_chuc_vai_tro(p_to_chuc, p_user);
BEGIN
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF v_vai <> 'quan_tri' THEN RETURN jsonb_build_object('loi', 'KHONG_DU_QUYEN'); END IF;
    UPDATE to_chuc_loi_moi SET huy_luc = now()
     WHERE id = p_id AND to_chuc_id = p_to_chuc AND huy_luc IS NULL AND dung_luc IS NULL;
    IF NOT FOUND AND NOT EXISTS (SELECT 1 FROM to_chuc_loi_moi WHERE id = p_id AND to_chuc_id = p_to_chuc) THEN
        RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY');
    END IF;
    RETURN jsonb_build_object('ok', TRUE);
END;
$$;

-- Nhận lời mời. Thứ tự khóa như mọi hàm tổ chức: to_chuc → lời mời → thành viên.
-- Sai mã / đã dùng / đã hủy / hết hạn: cùng một mã lỗi (không lộ lời mời nào tồn tại).
CREATE OR REPLACE FUNCTION to_chuc_loi_moi_nhan(p_user UUID, p_ma_bam TEXT) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE v_tc UUID; l to_chuc_loi_moi%ROWTYPE; u users%ROWTYPE; t to_chuc%ROWTYPE; m to_chuc_thanh_vien%ROWTYPE;
BEGIN
    SELECT to_chuc_id INTO v_tc FROM to_chuc_loi_moi WHERE ma_bam = p_ma_bam;
    IF NOT FOUND THEN RETURN jsonb_build_object('loi', 'LOI_MOI_KHONG_HOP_LE'); END IF;
    SELECT * INTO t FROM to_chuc WHERE id = v_tc FOR NO KEY UPDATE;
    SELECT * INTO l FROM to_chuc_loi_moi WHERE ma_bam = p_ma_bam FOR UPDATE;
    IF l.huy_luc IS NOT NULL OR l.dung_luc IS NOT NULL OR l.het_han <= now() THEN
        RETURN jsonb_build_object('loi', 'LOI_MOI_KHONG_HOP_LE');
    END IF;
    SELECT * INTO u FROM users WHERE id = p_user;
    IF NOT FOUND OR lower(u.email) <> l.email_thuong THEN RETURN jsonb_build_object('loi', 'SAI_EMAIL_MOI'); END IF;
    IF u.email_xac_minh_luc IS NULL THEN RETURN jsonb_build_object('loi', 'EMAIL_CHUA_XAC_MINH'); END IF;
    SELECT * INTO m FROM to_chuc_thanh_vien WHERE to_chuc_id = t.id AND user_id = p_user FOR UPDATE;
    IF FOUND AND m.trang_thai = 'hoat_dong' THEN
        UPDATE to_chuc_loi_moi SET dung_luc = now(), dung_boi = p_user WHERE id = l.id;
        RETURN jsonb_build_object('ok', TRUE, 'to_chuc', to_chuc_json(t, m.vai_tro, FALSE));
    END IF;
    IF (SELECT count(*) FROM to_chuc_thanh_vien WHERE to_chuc_id = t.id AND trang_thai = 'hoat_dong') >= to_chuc_toi_da(t) THEN
        RETURN jsonb_build_object('loi', 'DU_THANH_VIEN');
    END IF;
    INSERT INTO to_chuc_thanh_vien (to_chuc_id, user_id, vai_tro, them_boi)
    VALUES (t.id, p_user, l.vai_tro, l.moi_boi)
    ON CONFLICT (to_chuc_id, user_id) DO UPDATE
        SET vai_tro = EXCLUDED.vai_tro, trang_thai = 'hoat_dong', them_luc = now(), them_boi = EXCLUDED.them_boi, sua_luc = now()
    RETURNING * INTO m;
    UPDATE to_chuc_loi_moi SET dung_luc = now(), dung_boi = p_user WHERE id = l.id;
    RETURN jsonb_build_object('ok', TRUE, 'to_chuc', to_chuc_json(t, m.vai_tro, FALSE));
END;
$$;

-- ---------- gói ----------
CREATE OR REPLACE FUNCTION goi_to_chuc_ds() RETURNS JSONB
LANGUAGE sql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT COALESCE(jsonb_agg(to_jsonb(g) - 'thu_tu' ORDER BY g.thu_tu, g.ma), '[]'::jsonb) FROM goi_to_chuc g;
$$;

DO $$
DECLARE f TEXT;
BEGIN
    FOREACH f IN ARRAY ARRAY[
        'to_chuc_vai_tro(uuid,uuid)', 'to_chuc_ds(uuid)', 'to_chuc_tao(uuid,jsonb)', 'to_chuc_doc(uuid,uuid)',
        'to_chuc_sua(uuid,uuid,integer,jsonb)', 'to_chuc_thanh_vien_ds(uuid,uuid)',
        'to_chuc_thanh_vien_sua(uuid,uuid,uuid,text)', 'to_chuc_thanh_vien_bo(uuid,uuid,uuid)',
        'to_chuc_loi_moi_tao(uuid,uuid,text,text,text)', 'to_chuc_loi_moi_ds(uuid,uuid)',
        'to_chuc_loi_moi_huy(uuid,uuid,uuid)', 'to_chuc_loi_moi_nhan(uuid,text)', 'to_chuc_co_quan_doc(uuid,uuid)',
        'to_chuc_co_quan_ghi(uuid,uuid,jsonb)', 'goi_to_chuc_ds()'
    ] LOOP
        EXECUTE format('REVOKE ALL ON FUNCTION %s FROM PUBLIC, anon, authenticated', f);
        EXECUTE format('GRANT EXECUTE ON FUNCTION %s TO service_role', f);
    END LOOP;
    FOREACH f IN ARRAY ARRAY[
        'to_chuc_con_goi(to_chuc)', 'to_chuc_toi_da(to_chuc)', 'to_chuc_json(to_chuc,text,boolean)',
        'to_chuc_thanh_vien_json(to_chuc_thanh_vien)', 'to_chuc_loi_moi_json(to_chuc_loi_moi)',
        'to_chuc_con_quan_tri_khac(uuid,uuid)'
    ] LOOP
        EXECUTE format('REVOKE ALL ON FUNCTION %s FROM PUBLIC, anon, authenticated, service_role', f);
    END LOOP;
END;
$$;
```

- [ ] **Step 4: Chạy kiểm**

Run: `npx tsx --test tests/to-chuc-csdl.test.ts`
Expected: mọi bài PASS (5 bài Task 1 + 11 bài Task 2).

- [ ] **Step 5: Commit**

```bash
git add supabase/migrations/017_to_chuc.sql tests/to-chuc-csdl.test.ts
git commit -m "feat(to-chuc): 017 phan B - ham to chuc, thanh vien, quan tri cuoi, loi moi, co quan, goi

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 3: Migration 017 phần C — hạn mức kho tổ chức và chỉ đọc khi hết gói

**Files:**
- Modify: `supabase/migrations/017_to_chuc.sql` (nối cuối)
- Test: `tests/to-chuc-csdl.test.ts` (thêm `describe`)

**Interfaces:**
- Consumes: `to_chuc_con_goi` (Task 2); `kho_dung_luong_mua` (011).
- Produces: `kho_han_muc(text, uuid)` (thay 011, thêm nhánh `to_chuc`); `kho_dung_luong_xem(text, uuid)` (thay 010) trả thêm
  `het_goi: boolean`, `chi_doc = het_goi OR da_dung > han_muc`.

- [ ] **Step 1: Viết bài kiểm (thêm vào `tests/to-chuc-csdl.test.ts`)**

```ts
import { chuCua } from "./ho-tro/kho";
import { chuTC, hetGoiToChuc } from "./ho-tro/to-chuc";

describe("017 phần C: dung lượng kho tổ chức", { skip: !CO_CSDL_THU }, () => {
  let csdl: Awaited<ReturnType<typeof taoCsdlThu>>;
  let c: Client;
  let rpc: GoiRpc;

  before(async () => {
    csdl = await taoCsdlThu();
    c = new Client({ connectionString: csdl.url });
    await c.connect();
    rpc = rpcPg(c);
  });
  after(async () => {
    await c?.end();
    await csdl?.xoa();
  });

  test("chưa có gói: hạn mức 0, het_goi, chỉ đọc", async () => {
    const qt = await taoNguoiDung(c);
    const tc = await taoToChucThu(c, qt);
    const d = await rpc("kho_dung_luong_xem", chuTC(tc));
    assert.deepEqual([d.goi_mb, d.han_muc_byte, d.het_goi, d.chi_doc], [0, 0, true, true]);
  });

  test("gói tc-10 còn hạn: 51.200 MB, không chỉ đọc; hết hạn: chỉ đọc dù còn dung lượng mua thêm", async () => {
    const b = await boToChuc(c);
    let d = await rpc("kho_dung_luong_xem", chuTC(b.toChuc));
    assert.deepEqual([d.goi_mb, d.het_goi, d.chi_doc], [51200, false, false]);
    assert.equal(Number(d.han_muc_byte), 51200 * 1048576);
    const { rows } = await c.query(
      `INSERT INTO orders (code, user_id, loai, goi_dung_luong_ma, so_thang, amount_vnd, status, paid_at)
       VALUES ('VBPTCDLAAAA', $1, 'dung_luong', 'dl-10', 12, 348000, 'paid', now()) RETURNING id`, [b.quanTri]);
    await c.query(`INSERT INTO kho_dung_luong_mua (chu_loai, chu_id, goi_ma, them_mb, bat_dau, het_han, order_id)
                   VALUES ('to_chuc', $1, 'dl-10', 10240, now() - interval '1 day', now() + interval '300 days', $2)`, [b.toChuc, rows[0].id]);
    d = await rpc("kho_dung_luong_xem", chuTC(b.toChuc));
    assert.equal(d.them_mb, 10240);
    await hetGoiToChuc(c, b.toChuc);
    d = await rpc("kho_dung_luong_xem", chuTC(b.toChuc));
    assert.deepEqual([d.goi_mb, d.them_mb, d.het_goi, d.chi_doc], [0, 10240, true, true]);
  });

  test("kho cá nhân: het_goi luôn false, các khóa cũ giữ nguyên", async () => {
    const u = await taoNguoiDung(c);
    const d = await rpc("kho_dung_luong_xem", chuCua(u));
    assert.equal(d.het_goi, false);
    assert.equal(d.chi_doc, false);
    for (const k of ["goi_mb", "them_mb", "han_muc_byte", "goi_them", "da_dung_byte", "cho_byte"]) assert.ok(k in d, k);
  });
});
```

(Mã đơn khớp `^VBP[A-HJ-NP-Z2-9]{8}$`. Đơn này vẫn là đơn cá nhân — Task 3 chỉ cần một `order_id` hợp lệ cho
`kho_dung_luong_mua`; đơn tổ chức thật làm ở Task 8.)

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx tsx --test tests/to-chuc-csdl.test.ts`
Expected: FAIL — `d.het_goi` là `undefined`.

- [ ] **Step 3: Nối phần C vào `017_to_chuc.sql`**

```sql
-- ============================================================
-- Phần C: hạn mức kho tổ chức (spec §2.4)
-- ============================================================

-- Thay bản 011: thêm nhánh to_chuc (dung lượng của gói tổ chức còn hiệu lực). Nhánh ca_nhan + gói mua thêm GIỮ NGUYÊN 011.
CREATE OR REPLACE FUNCTION kho_han_muc(p_chu_loai TEXT, p_chu_id UUID) RETURNS JSONB
LANGUAGE plpgsql STABLE SET search_path = public, pg_temp AS $$
DECLARE v_goi TEXT; v_het TIMESTAMPTZ; v_mb INT := 0; v_them INT; v_ds JSONB;
BEGIN
    IF p_chu_loai = 'ca_nhan' THEN
        SELECT subscription_plan_id, subscription_end_date INTO v_goi, v_het FROM users WHERE id = p_chu_id;
        IF FOUND THEN
            IF v_het IS NOT NULL AND v_het < now() THEN v_goi := 'free'; END IF;
            SELECT cloud_storage_mb INTO v_mb FROM subscription_plans WHERE id = v_goi;
            v_mb := COALESCE(v_mb, 0);
        END IF;
    ELSIF p_chu_loai = 'to_chuc' THEN
        SELECT g.dung_luong_mb INTO v_mb FROM to_chuc t JOIN goi_to_chuc g ON g.ma = t.goi_ma
         WHERE t.id = p_chu_id AND to_chuc_con_goi(t);
        v_mb := COALESCE(v_mb, 0);
    END IF;
    SELECT COALESCE(sum(them_mb) FILTER (WHERE bat_dau <= now()), 0),
           COALESCE(jsonb_agg(jsonb_build_object('ma', goi_ma, 'them_mb', them_mb, 'bat_dau', bat_dau, 'het_han', het_han)
                              ORDER BY het_han), '[]'::jsonb)
      INTO v_them, v_ds
      FROM kho_dung_luong_mua WHERE chu_loai = p_chu_loai AND chu_id = p_chu_id AND het_han > now();
    RETURN jsonb_build_object('goi_mb', v_mb, 'them_mb', v_them, 'han_muc_byte', (v_mb + v_them)::BIGINT * 1048576,
                              'goi_them', v_ds);
END;
$$;

-- Thay bản 010: thêm het_goi (kho tổ chức chưa có gói / gói hết hạn → chỉ đọc, dùng cơ chế chỉ đọc sẵn có).
CREATE OR REPLACE FUNCTION kho_dung_luong_xem(p_chu_loai TEXT, p_chu_id UUID) RETURNS JSONB
LANGUAGE plpgsql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE h JSONB; v_da BIGINT; v_cho BIGINT; v_het_goi BOOLEAN := FALSE;
BEGIN
    PERFORM kho_kiem_chu(p_chu_loai, p_chu_id);
    h := kho_han_muc(p_chu_loai, p_chu_id);
    SELECT COALESCE((SELECT byte_da_dung FROM kho_dung_luong WHERE chu_loai = p_chu_loai AND chu_id = p_chu_id), 0)
      INTO v_da;
    SELECT COALESCE(sum(byte_khai), 0) INTO v_cho FROM kho_tep
     WHERE chu_loai = p_chu_loai AND chu_id = p_chu_id AND trang_thai = 'cho' AND het_han_cho > now();
    IF p_chu_loai = 'to_chuc' THEN
        v_het_goi := NOT COALESCE((SELECT to_chuc_con_goi(t) FROM to_chuc t WHERE t.id = p_chu_id), FALSE);
    END IF;
    RETURN h || jsonb_build_object('da_dung_byte', v_da, 'cho_byte', v_cho, 'het_goi', v_het_goi,
                                   'chi_doc', v_het_goi OR v_da > (h->>'han_muc_byte')::BIGINT);
END;
$$;

REVOKE ALL ON FUNCTION kho_han_muc(text, uuid) FROM PUBLIC, anon, authenticated, service_role;
REVOKE ALL ON FUNCTION kho_dung_luong_xem(text, uuid) FROM PUBLIC, anon, authenticated;
GRANT EXECUTE ON FUNCTION kho_dung_luong_xem(text, uuid) TO service_role;
```

- [ ] **Step 4: Chạy kiểm**

Run: `npx tsx --test tests/to-chuc-csdl.test.ts tests/kho-csdl.test.ts tests/kho-dung-luong-csdl.test.ts tests/mau-csdl.test.ts tests/hop-csdl.test.ts`
Expected: tất cả PASS (bài cũ không sửa).

- [ ] **Step 5: Commit**

```bash
git add supabase/migrations/017_to_chuc.sql tests/to-chuc-csdl.test.ts
git commit -m "feat(to-chuc): 017 phan C - han muc kho to chuc theo goi, chi doc khi het goi

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 4: Migration 018 phần A — bảng giao việc, quyền theo vai trò, hàm văn bản có `p_nguoi`

**Files:**
- Create: `supabase/migrations/018_giao_viec.sql`
- Modify: `tests/ho-tro/csdl-thu.ts` (`TEP_LUOC_DO` thêm `"migrations/018_giao_viec.sql",`)
- Create: `tests/giao-viec-csdl.test.ts`

**Interfaces:**
- Consumes: `to_chuc_vai_tro` (Task 2), `kho_dung_luong_xem` (Task 3), hàm 010/015.
- Produces:
  - cột `kho_van_ban.cong_khai_noi_bo`; bảng `kho_giao_viec`, `kho_giao_viec_nhat_ky`;
  - `kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi) → 'chu' | 'quan_tri' | 'van_thu' | 'can_bo' | NULL` (ném lỗi khi kho tổ chức
    thiếu `p_nguoi`); `kho_van_ban_xem_duoc(kho_van_ban, text, uuid)`, `kho_van_ban_sua_duoc(kho_van_ban, text, uuid)` (nội bộ);
  - `kho_van_ban_json_q(kho_van_ban, boolean, text, uuid)` (nội bộ: kho tổ chức thêm `sua_duoc`);
  - bản mới (gọi rpc kèm `p_nguoi`): `kho_van_ban_tao(p_chu_loai, p_chu_id, p_tao_boi, p_du_lieu, p_nguoi)`,
    `kho_van_ban_doc|co|xoa_mem|khoi_phuc|xoa_han(p_chu_loai, p_chu_id, p_id, p_nguoi)`,
    `kho_van_ban_sua(p_chu_loai, p_chu_id, p_id, p_phien_ban, p_du_lieu, p_nguoi)`, `kho_van_ban_tim(p_chu_loai, p_chu_id, p_loc, p_nguoi)`;
    bản cũ cùng tên (không `p_nguoi`) gọi bản mới với `NULL`.

- [ ] **Step 1: Viết bài kiểm ma trận quyền văn bản `tests/giao-viec-csdl.test.ts`**

```ts
import { after, before, describe, test } from "node:test";
import assert from "node:assert/strict";
import { randomUUID } from "node:crypto";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { Client } from "pg";
import { CO_CSDL_THU, taoCsdlThu, taoNguoiDung } from "./ho-tro/csdl-thu";
import { rpcPg } from "./ho-tro/rpc-pg";
import { chuCua } from "./ho-tro/kho";
import { boToChuc, chuTC, themThanhVien } from "./ho-tro/to-chuc";
import type { GoiRpc } from "../src/lib/phien/rpc";

type Bo = Awaited<ReturnType<typeof boToChuc>>;

describe("018 phần A: văn bản theo vai trò", { skip: !CO_CSDL_THU }, () => {
  let csdl: Awaited<ReturnType<typeof taoCsdlThu>>;
  let c: Client;
  let rpc: GoiRpc;
  let b: Bo;
  const vb: Record<string, string> = {};

  const tao = async (nguoi: string, than: Record<string, unknown> = {}) => {
    const r = await rpc("kho_van_ban_tao", {
      ...chuTC(b.toChuc), p_tao_boi: nguoi, p_nguoi: nguoi, p_du_lieu: { huong: "den", trich_yeu: "Thử", ...than },
    });
    assert.equal(r.ok, true, JSON.stringify(r));
    return r.van_ban.id as string;
  };
  const doc = (nguoi: string, id: string) => rpc("kho_van_ban_doc", { ...chuTC(b.toChuc), p_id: id, p_nguoi: nguoi });
  const giao = (id: string, nguoi: string, vai = "phoi_hop") =>
    c.query(`INSERT INTO kho_giao_viec (chu_id, van_ban_id, nguoi_nhan, vai, giao_boi) VALUES ($1, $2, $3, $4, $5)`,
      [b.toChuc, id, nguoi, vai, b.vanThu]);

  before(async () => {
    csdl = await taoCsdlThu();
    c = new Client({ connectionString: csdl.url });
    await c.connect();
    rpc = rpcPg(c);
    b = await boToChuc(c);
    vb.quanTri = await tao(b.quanTri, { trich_yeu: "Riêng của quản trị" });
    vb.canBo = await tao(b.canBo, { trich_yeu: "Do cán bộ tạo" });
    vb.congKhai = await tao(b.vanThu, { trich_yeu: "Công khai nội bộ", cong_khai_noi_bo: true });
    vb.duocGiao = await tao(b.vanThu, { trich_yeu: "Được giao cho cán bộ" });
    await giao(vb.duocGiao, b.canBo, "chu_tri");
  });
  after(async () => {
    await c?.end();
    await csdl?.xoa();
  });

  test("xem: quản trị / văn thư thấy hết; cán bộ chỉ thấy văn bản do chính người đó tạo, được giao, công khai", async () => {
    for (const ai of [b.quanTri, b.vanThu]) for (const id of Object.values(vb)) assert.ok(await doc(ai, id), `${ai} ${id}`);
    assert.equal(await doc(b.canBo, vb.quanTri), null);
    for (const id of [vb.canBo, vb.congKhai, vb.duocGiao]) assert.ok(await doc(b.canBo, id));
    assert.equal(await doc(b.canBo2, vb.canBo), null);
    assert.equal(await doc(b.canBo2, vb.duocGiao), null);
    assert.ok(await doc(b.canBo2, vb.congKhai));
  });

  test("tìm: cán bộ chỉ nhận văn bản trong phạm vi, tổng đếm cũng theo phạm vi", async () => {
    const tim = (ai: string) => rpc("kho_van_ban_tim", { ...chuTC(b.toChuc), p_loc: { so_dong: 100 }, p_nguoi: ai });
    const r = await tim(b.canBo);
    assert.deepEqual(new Set(r.muc.map((v: any) => v.id)), new Set([vb.canBo, vb.congKhai, vb.duocGiao]));
    assert.equal(Number(r.tong), 3);
    assert.equal(Number((await tim(b.vanThu)).tong), 4);
  });

  test("JSON kho tổ chức: có cong_khai_noi_bo + sua_duoc theo người gọi; không lộ tao_boi", async () => {
    const a = await doc(b.canBo, vb.duocGiao);
    assert.equal(a.sua_duoc, false);
    assert.equal(a.cong_khai_noi_bo, false);
    assert.equal("tao_boi" in a, false);
    assert.equal((await doc(b.canBo, vb.canBo)).sua_duoc, true);
    assert.equal((await doc(b.vanThu, vb.canBo)).sua_duoc, true);
  });

  test("sửa / xóa / khôi phục: cán bộ chỉ văn bản do chính người đó tạo; ngoài phạm vi 404, xem được mà không sửa được 403", async () => {
    const sua = (ai: string, id: string, pb = 1) =>
      rpc("kho_van_ban_sua", { ...chuTC(b.toChuc), p_id: id, p_phien_ban: pb, p_du_lieu: { ghi_chu: "x" }, p_nguoi: ai });
    assert.deepEqual(await sua(b.canBo, vb.quanTri), { loi: "KHONG_TIM_THAY" });
    assert.deepEqual(await sua(b.canBo, vb.duocGiao), { loi: "KHONG_DU_QUYEN" });
    assert.equal((await sua(b.canBo, vb.canBo)).ok, true);
    assert.equal((await sua(b.vanThu, vb.canBo, 2)).ok, true);
    const lam = (ham: string, ai: string, id: string) => rpc(ham, { ...chuTC(b.toChuc), p_id: id, p_nguoi: ai });
    assert.deepEqual(await lam("kho_van_ban_xoa_mem", b.canBo, vb.congKhai), { loi: "KHONG_DU_QUYEN" });
    assert.deepEqual(await lam("kho_van_ban_xoa_han", b.canBo, vb.quanTri), { loi: "KHONG_TIM_THAY" });
    const rieng = await tao(b.canBo, { trich_yeu: "Sẽ xóa" });
    assert.equal((await lam("kho_van_ban_xoa_mem", b.canBo, rieng)).ok, true);
    assert.equal((await lam("kho_van_ban_khoi_phuc", b.canBo, rieng)).ok, true);
    assert.equal((await lam("kho_van_ban_xoa_mem", b.canBo, rieng)).ok, true);
    assert.equal((await lam("kho_van_ban_xoa_han", b.canBo, rieng)).ok, true);
    assert.equal(await lam("kho_van_ban_co", b.canBo, vb.duocGiao), false);
    assert.equal(await lam("kho_van_ban_co", b.canBo, vb.canBo), true);
  });

  test("tạo: cán bộ tạo được; khóa chống trùng của người khác → DAU_VAO_SAI, không trả văn bản", async () => {
    const k = randomUUID();
    await tao(b.vanThu, { khoa_tao: k, trich_yeu: "Bản của văn thư" });
    const r = await rpc("kho_van_ban_tao", {
      ...chuTC(b.toChuc), p_tao_boi: b.canBo, p_nguoi: b.canBo, p_du_lieu: { huong: "den", trich_yeu: "Đoán khóa", khoa_tao: k },
    });
    assert.equal(r.loi, "DAU_VAO_SAI");
    assert.equal("van_ban" in r, false);
  });

  test("người ngoài, thành viên đã rời, tổ chức khác: không thấy gì", async () => {
    const ngoai = await taoNguoiDung(c);
    assert.equal(await doc(ngoai, vb.congKhai), null);
    assert.deepEqual(await rpc("kho_van_ban_tao", { ...chuTC(b.toChuc), p_tao_boi: ngoai, p_nguoi: ngoai,
      p_du_lieu: { huong: "den", trich_yeu: "x" } }), { loi: "KHONG_TIM_THAY" });
    const roi = await taoNguoiDung(c);
    await themThanhVien(c, b.toChuc, roi, "quan_tri", "da_roi");
    assert.equal(await doc(roi, vb.congKhai), null);
    const khac = await boToChuc(c);
    assert.equal(await rpc("kho_van_ban_doc", { ...chuTC(khac.toChuc), p_id: vb.congKhai, p_nguoi: khac.quanTri }), null);
    assert.equal(await rpc("kho_van_ban_doc", { ...chuTC(b.toChuc), p_id: vb.congKhai, p_nguoi: khac.quanTri }), null);
  });

  test("bản cũ không p_nguoi: kho tổ chức bị từ chối; kho cá nhân như trước (không có khóa mới trong JSON)", async () => {
    await assert.rejects(rpc("kho_van_ban_doc", { ...chuTC(b.toChuc), p_id: vb.congKhai }), /thiếu người gọi/);
    const u = await taoNguoiDung(c);
    const r = await rpc("kho_van_ban_tao", { ...chuCua(u), p_tao_boi: u, p_du_lieu: { huong: "den", trich_yeu: "Cá nhân" } });
    assert.equal(r.ok, true);
    assert.equal("cong_khai_noi_bo" in r.van_ban, false);
    assert.equal("sua_duoc" in r.van_ban, false);
    const d = await rpc("kho_van_ban_doc", { ...chuCua(u), p_id: r.van_ban.id });
    assert.equal(d.trich_yeu, "Cá nhân");
  });

  test("quyền: bản mới gọi được bằng service_role, không bằng anon; hàm quyền là nội bộ", async () => {
    const co = async (v: string, f: string) =>
      (await c.query("SELECT has_function_privilege($1, $2, 'EXECUTE') AS co", [v, f])).rows[0].co as boolean;
    const moi = ["kho_van_ban_tao(text,uuid,uuid,jsonb,uuid)", "kho_van_ban_doc(text,uuid,uuid,uuid)",
      "kho_van_ban_co(text,uuid,uuid,uuid)", "kho_van_ban_sua(text,uuid,uuid,integer,jsonb,uuid)",
      "kho_van_ban_xoa_mem(text,uuid,uuid,uuid)", "kho_van_ban_khoi_phuc(text,uuid,uuid,uuid)",
      "kho_van_ban_xoa_han(text,uuid,uuid,uuid)", "kho_van_ban_tim(text,uuid,jsonb,uuid)"];
    for (const f of moi) {
      assert.equal(await co("service_role", f), true, f);
      assert.equal(await co("anon", f), false, f);
    }
    for (const f of ["kho_vai_tro(text,uuid,uuid)", "kho_van_ban_xem_duoc(kho_van_ban,text,uuid)",
      "kho_van_ban_sua_duoc(kho_van_ban,text,uuid)", "kho_van_ban_json_q(kho_van_ban,boolean,text,uuid)"]) {
      assert.equal(await co("service_role", f), false, f);
    }
    for (const t of ["kho_giao_viec", "kho_giao_viec_nhat_ky"]) {
      assert.equal((await c.query("SELECT has_table_privilege('service_role', $1, 'SELECT') AS co", [t])).rows[0].co, false, t);
    }
  });

  test("chạy lại 018 lần hai không lỗi", async () => {
    await c.query(readFileSync(join(__dirname, "..", "supabase", "migrations", "018_giao_viec.sql"), "utf8"));
  });
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx tsx --test tests/giao-viec-csdl.test.ts`
Expected: FAIL — `relation "kho_giao_viec" does not exist` / hàm 5 tham số không có.

- [ ] **Step 3: Viết `supabase/migrations/018_giao_viec.sql` (phần A) và thêm vào `TEP_LUOC_DO`**

```sql
-- ============================================================
-- 018: Giao việc + quyền kho theo vai trò (spec đợt B §2.2, §3)
-- Lọc quyền nằm trong hàm SQL: bản mới của kho_van_ban_* / kho_tep_* nhận p_nguoi và TỰ tra vai trò (kho_vai_tro);
-- không nhận vai trò từ API. Bản cũ (không p_nguoi) giữ chữ ký, gọi bản mới với NULL → chỉ dùng được cho kho cá nhân.
-- Cán bộ: xem văn bản do chính người đó tạo + được giao (chủ trì / phối hợp, chưa thu hồi) + công khai trong cơ quan; sửa / xóa /
-- gắn tệp văn bản do chính người đó tạo. Ngoài phạm vi xem: KHONG_TIM_THAY; xem được mà không sửa được: KHONG_DU_QUYEN.
-- ============================================================

SET lock_timeout = '5s';

ALTER TABLE kho_van_ban ADD COLUMN IF NOT EXISTS cong_khai_noi_bo BOOLEAN NOT NULL DEFAULT FALSE;

CREATE TABLE IF NOT EXISTS kho_giao_viec (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    chu_loai TEXT NOT NULL DEFAULT 'to_chuc' CHECK (chu_loai = 'to_chuc'),
    chu_id UUID NOT NULL REFERENCES to_chuc(id) ON DELETE CASCADE,
    van_ban_id UUID NOT NULL REFERENCES kho_van_ban(id) ON DELETE CASCADE,
    nguoi_nhan UUID NOT NULL,
    vai TEXT NOT NULL CHECK (vai IN ('chu_tri', 'phoi_hop')),
    han_xu_ly DATE,
    y_kien_chi_dao TEXT CHECK (char_length(y_kien_chi_dao) <= 2000),
    trang_thai TEXT NOT NULL DEFAULT 'cho' CHECK (trang_thai IN ('cho', 'dang', 'xong')),
    ghi_chu TEXT CHECK (char_length(ghi_chu) <= 2000),
    giao_boi UUID NOT NULL,
    giao_luc TIMESTAMPTZ NOT NULL DEFAULT now(),
    cap_nhat_luc TIMESTAMPTZ NOT NULL DEFAULT now(),
    huy_luc TIMESTAMPTZ,
    phien_ban INT NOT NULL DEFAULT 1
);
-- Mỗi văn bản tối đa một chủ trì chưa thu hồi; một người không được giao hai lần cùng văn bản.
CREATE UNIQUE INDEX IF NOT EXISTS uq_kho_giao_viec_chu_tri ON kho_giao_viec(van_ban_id) WHERE vai = 'chu_tri' AND huy_luc IS NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_kho_giao_viec_nguoi ON kho_giao_viec(van_ban_id, nguoi_nhan) WHERE huy_luc IS NULL;
CREATE INDEX IF NOT EXISTS idx_kho_giao_viec_nguoi ON kho_giao_viec(chu_id, nguoi_nhan, trang_thai) WHERE huy_luc IS NULL;

CREATE TABLE IF NOT EXISTS kho_giao_viec_nhat_ky (
    id BIGSERIAL PRIMARY KEY,
    giao_viec_id UUID NOT NULL REFERENCES kho_giao_viec(id) ON DELETE CASCADE,
    nguoi UUID NOT NULL,
    hanh_dong TEXT NOT NULL CHECK (hanh_dong IN ('giao', 'sua', 'trang_thai', 'ghi_chu', 'thu_hoi')),
    truoc JSONB,
    sau JSONB,
    luc TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS idx_kho_giao_viec_nhat_ky ON kho_giao_viec_nhat_ky(giao_viec_id, luc DESC);

-- Nhật ký chỉ ghi thêm. Xóa theo dây chuyền (xóa hẳn văn bản / tổ chức) thì cho qua, như credit_so (004).
CREATE OR REPLACE FUNCTION kho_giao_viec_nhat_ky_chi_ghi_them() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
    IF TG_OP = 'DELETE' AND pg_trigger_depth() > 1 THEN RETURN OLD; END IF;
    RAISE EXCEPTION 'kho_giao_viec_nhat_ky chỉ ghi thêm, không sửa, không xóa';
END;
$$;
DROP TRIGGER IF EXISTS trg_kho_giao_viec_nhat_ky_chi_ghi_them ON kho_giao_viec_nhat_ky;
CREATE TRIGGER trg_kho_giao_viec_nhat_ky_chi_ghi_them BEFORE UPDATE OR DELETE ON kho_giao_viec_nhat_ky
    FOR EACH ROW EXECUTE FUNCTION kho_giao_viec_nhat_ky_chi_ghi_them();

ALTER TABLE kho_giao_viec ENABLE ROW LEVEL SECURITY;
ALTER TABLE kho_giao_viec_nhat_ky ENABLE ROW LEVEL SECURITY;
REVOKE ALL ON kho_giao_viec, kho_giao_viec_nhat_ky FROM PUBLIC, anon, authenticated, service_role;
REVOKE ALL ON SEQUENCE kho_giao_viec_nhat_ky_id_seq FROM PUBLIC, anon, authenticated, service_role;
REVOKE ALL ON FUNCTION kho_giao_viec_nhat_ky_chi_ghi_them() FROM PUBLIC, anon, authenticated, service_role;

-- ---------- vai trò + luật xem / sửa ----------
-- 'chu' cho kho cá nhân (p_nguoi bỏ qua, giữ hành vi đợt A); vai trò thành viên hoạt động cho kho tổ chức; NULL = không quyền.
CREATE OR REPLACE FUNCTION kho_vai_tro(p_chu_loai TEXT, p_chu_id UUID, p_nguoi UUID) RETURNS TEXT
LANGUAGE plpgsql STABLE SET search_path = public, pg_temp AS $$
BEGIN
    PERFORM kho_kiem_chu(p_chu_loai, p_chu_id);
    IF p_chu_loai = 'ca_nhan' THEN RETURN 'chu'; END IF;
    IF p_nguoi IS NULL THEN RAISE EXCEPTION 'kho: thiếu người gọi cho kho tổ chức'; END IF;
    RETURN to_chuc_vai_tro(p_chu_id, p_nguoi);
END;
$$;

CREATE OR REPLACE FUNCTION kho_van_ban_xem_duoc(v kho_van_ban, p_vai TEXT, p_nguoi UUID) RETURNS BOOLEAN
LANGUAGE sql STABLE SET search_path = public, pg_temp AS $$
    SELECT COALESCE(p_vai IN ('chu', 'quan_tri', 'van_thu')
        OR (p_vai = 'can_bo' AND (v.tao_boi = p_nguoi OR v.cong_khai_noi_bo
            OR EXISTS (SELECT 1 FROM kho_giao_viec g
                        WHERE g.van_ban_id = v.id AND g.nguoi_nhan = p_nguoi AND g.huy_luc IS NULL))), FALSE);
$$;

CREATE OR REPLACE FUNCTION kho_van_ban_sua_duoc(v kho_van_ban, p_vai TEXT, p_nguoi UUID) RETURNS BOOLEAN
LANGUAGE sql STABLE SET search_path = public, pg_temp AS $$
    SELECT COALESCE(p_vai IN ('chu', 'quan_tri', 'van_thu') OR (p_vai = 'can_bo' AND v.tao_boi = p_nguoi), FALSE);
$$;

-- Thay bản 015: kho cá nhân bỏ thêm cong_khai_noi_bo (JSON cá nhân giữ đúng như đợt A).
CREATE OR REPLACE FUNCTION kho_van_ban_json(v kho_van_ban, p_day_du BOOLEAN) RETURNS JSONB
LANGUAGE sql STABLE SET search_path = public, pg_temp AS $$
    SELECT (to_jsonb(v) - ARRAY['tim', 'chu_loai', 'chu_id', 'tao_boi', 'khoa_tao']
                        - CASE WHEN v.chu_loai = 'ca_nhan' THEN ARRAY['cong_khai_noi_bo'] ELSE '{}'::TEXT[] END
                        - CASE WHEN p_day_du THEN '{}'::TEXT[] ELSE ARRAY['noi_dung'] END)
        || jsonb_build_object('tep', COALESCE((
               SELECT jsonb_agg(kho_tep_json(t) ORDER BY t.tao_luc)
                 FROM kho_tep t
                WHERE t.gan_loai = 'van_ban' AND t.gan_id = v.id AND t.trang_thai = 'xong'), '[]'::jsonb));
$$;

-- JSON theo người gọi: kho tổ chức thêm sua_duoc.
CREATE OR REPLACE FUNCTION kho_van_ban_json_q(v kho_van_ban, p_day_du BOOLEAN, p_vai TEXT, p_nguoi UUID) RETURNS JSONB
LANGUAGE sql STABLE SET search_path = public, pg_temp AS $$
    SELECT kho_van_ban_json(v, p_day_du)
        || CASE WHEN v.chu_loai = 'to_chuc' THEN jsonb_build_object('sua_duoc', kho_van_ban_sua_duoc(v, p_vai, p_nguoi))
                ELSE '{}'::jsonb END;
$$;

-- ---------- văn bản: bản mới có p_nguoi ----------
-- Như 015 + vai trò; kho tổ chức: người tạo phải là người gọi; khóa chống trùng chỉ trả văn bản của chính người tạo.
CREATE OR REPLACE FUNCTION kho_van_ban_tao(p_chu_loai TEXT, p_chu_id UUID, p_tao_boi UUID, p_du_lieu JSONB, p_nguoi UUID)
RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE p JSONB := p_du_lieu; v kho_van_ban%ROWTYPE; l JSONB; v_khoa UUID := (p_du_lieu->>'khoa_tao')::UUID;
        v_vai TEXT := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
BEGIN
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF p_chu_loai = 'to_chuc' AND p_tao_boi IS DISTINCT FROM p_nguoi THEN
        RAISE EXCEPTION 'kho_van_ban_tao: người tạo phải là người gọi';
    END IF;
    IF v_khoa IS NOT NULL THEN
        SELECT * INTO v FROM kho_van_ban WHERE chu_loai = p_chu_loai AND chu_id = p_chu_id AND khoa_tao = v_khoa;
        IF FOUND THEN
            IF p_chu_loai = 'to_chuc' AND v.tao_boi IS DISTINCT FROM p_tao_boi THEN
                RETURN kho_loi_dau_vao('Khóa chống trùng không hợp lệ.');
            END IF;
            RETURN jsonb_build_object('ok', TRUE, 'van_ban', kho_van_ban_json_q(v, TRUE, v_vai, p_nguoi), 'da_co', TRUE);
        END IF;
    END IF;
    IF (kho_dung_luong_xem(p_chu_loai, p_chu_id)->>'chi_doc')::BOOLEAN THEN
        RETURN jsonb_build_object('loi', 'HET_DUNG_LUONG');
    END IF;
    l := kho_van_ban_kiem_thu_muc(p_chu_loai, p_chu_id, (p->>'thu_muc_id')::UUID, p->>'huong');
    IF l IS NOT NULL THEN RETURN l; END IF;
    BEGIN
        INSERT INTO kho_van_ban (chu_loai, chu_id, tao_boi, khoa_tao, thu_muc_id, huong, so, ky_hieu, ngay_ban_hanh, co_quan,
            trich_yeu, loai, do_khan, do_mat, so_den, ngay_den, han_xu_ly, giao_cho, can_cu, noi_nhan, nguoi_ky, chuc_danh,
            noi_dung, tom_tat, the, lien_quan, gan_sao, ghi_chu, ban_sao, cong_khai_noi_bo)
        VALUES (p_chu_loai, p_chu_id, p_tao_boi, v_khoa, (p->>'thu_muc_id')::UUID, p->>'huong', p->>'so', p->>'ky_hieu',
            (p->>'ngay_ban_hanh')::DATE, p->>'co_quan', p->>'trich_yeu', p->>'loai',
            COALESCE(p->>'do_khan', 'thuong'), COALESCE(p->>'do_mat', 'thuong'), (p->>'so_den')::INT,
            (p->>'ngay_den')::DATE, (p->>'han_xu_ly')::DATE, p->>'giao_cho', kho_mang(p->'can_cu'), kho_mang(p->'noi_nhan'),
            p->>'nguoi_ky', p->>'chuc_danh', p->>'noi_dung', p->>'tom_tat', kho_mang(p->'the'), kho_mang(p->'lien_quan'),
            COALESCE((p->>'gan_sao')::BOOLEAN, FALSE), p->>'ghi_chu',
            CASE WHEN jsonb_typeof(p->'ban_sao') = 'object' THEN p->'ban_sao' END,
            p_chu_loai = 'to_chuc' AND COALESCE((p->>'cong_khai_noi_bo')::BOOLEAN, FALSE))
        RETURNING * INTO v;
    EXCEPTION WHEN unique_violation THEN
        SELECT * INTO v FROM kho_van_ban WHERE chu_loai = p_chu_loai AND chu_id = p_chu_id AND khoa_tao = v_khoa;
        IF v_khoa IS NULL OR NOT FOUND THEN RAISE; END IF;
        IF p_chu_loai = 'to_chuc' AND v.tao_boi IS DISTINCT FROM p_tao_boi THEN
            RETURN kho_loi_dau_vao('Khóa chống trùng không hợp lệ.');
        END IF;
        RETURN jsonb_build_object('ok', TRUE, 'van_ban', kho_van_ban_json_q(v, TRUE, v_vai, p_nguoi), 'da_co', TRUE);
    END;
    RETURN jsonb_build_object('ok', TRUE, 'van_ban', kho_van_ban_json_q(v, TRUE, v_vai, p_nguoi));
END;
$$;

CREATE OR REPLACE FUNCTION kho_van_ban_doc(p_chu_loai TEXT, p_chu_id UUID, p_id UUID, p_nguoi UUID) RETURNS JSONB
LANGUAGE plpgsql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE v kho_van_ban%ROWTYPE; v_vai TEXT := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
BEGIN
    IF v_vai IS NULL THEN RETURN NULL; END IF;
    SELECT * INTO v FROM kho_van_ban WHERE id = p_id AND chu_loai = p_chu_loai AND chu_id = p_chu_id;
    IF NOT FOUND OR NOT kho_van_ban_xem_duoc(v, v_vai, p_nguoi) THEN RETURN NULL; END IF;
    RETURN kho_van_ban_json_q(v, TRUE, v_vai, p_nguoi);
END;
$$;

-- Văn bản có, thuộc chủ, ngoài thùng rác, người gọi gắn tệp được (= sửa được).
CREATE OR REPLACE FUNCTION kho_van_ban_co(p_chu_loai TEXT, p_chu_id UUID, p_id UUID, p_nguoi UUID) RETURNS JSONB
LANGUAGE plpgsql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE v kho_van_ban%ROWTYPE; v_vai TEXT := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
BEGIN
    IF v_vai IS NULL THEN RETURN to_jsonb(FALSE); END IF;
    SELECT * INTO v FROM kho_van_ban WHERE id = p_id AND chu_loai = p_chu_loai AND chu_id = p_chu_id AND xoa_luc IS NULL;
    RETURN to_jsonb(FOUND AND kho_van_ban_sua_duoc(v, v_vai, p_nguoi));
END;
$$;

CREATE OR REPLACE FUNCTION kho_van_ban_sua(p_chu_loai TEXT, p_chu_id UUID, p_id UUID, p_phien_ban INT, p_du_lieu JSONB,
    p_nguoi UUID) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE p JSONB := p_du_lieu; v kho_van_ban%ROWTYPE; l JSONB; v_huong TEXT; v_thu_muc UUID;
        v_vai TEXT := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
BEGIN
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    SELECT * INTO v FROM kho_van_ban WHERE id = p_id AND chu_loai = p_chu_loai AND chu_id = p_chu_id FOR UPDATE;
    IF NOT FOUND OR NOT kho_van_ban_xem_duoc(v, v_vai, p_nguoi) THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF NOT kho_van_ban_sua_duoc(v, v_vai, p_nguoi) THEN RETURN jsonb_build_object('loi', 'KHONG_DU_QUYEN'); END IF;
    IF v.xoa_luc IS NOT NULL THEN RETURN jsonb_build_object('loi', 'DANG_O_THUNG_RAC'); END IF;
    IF v.phien_ban <> p_phien_ban THEN
        RETURN jsonb_build_object('loi', 'XUNG_DOT_PHIEN_BAN', 'ban_moi', kho_van_ban_json_q(v, TRUE, v_vai, p_nguoi));
    END IF;
    v_huong := CASE WHEN p ? 'huong' THEN p->>'huong' ELSE v.huong END;
    v_thu_muc := CASE WHEN p ? 'thu_muc_id' THEN (p->>'thu_muc_id')::UUID ELSE v.thu_muc_id END;
    IF p ? 'huong' OR p ? 'thu_muc_id' THEN
        l := kho_van_ban_kiem_thu_muc(p_chu_loai, p_chu_id, v_thu_muc, v_huong);
        IF l IS NOT NULL THEN RETURN l; END IF;
    END IF;
    UPDATE kho_van_ban SET
        huong = v_huong,
        thu_muc_id = v_thu_muc,
        so = CASE WHEN p ? 'so' THEN p->>'so' ELSE so END,
        ky_hieu = CASE WHEN p ? 'ky_hieu' THEN p->>'ky_hieu' ELSE ky_hieu END,
        ngay_ban_hanh = CASE WHEN p ? 'ngay_ban_hanh' THEN (p->>'ngay_ban_hanh')::DATE ELSE ngay_ban_hanh END,
        co_quan = CASE WHEN p ? 'co_quan' THEN p->>'co_quan' ELSE co_quan END,
        trich_yeu = CASE WHEN p ? 'trich_yeu' THEN p->>'trich_yeu' ELSE trich_yeu END,
        loai = CASE WHEN p ? 'loai' THEN p->>'loai' ELSE loai END,
        do_khan = CASE WHEN p ? 'do_khan' THEN p->>'do_khan' ELSE do_khan END,
        do_mat = CASE WHEN p ? 'do_mat' THEN p->>'do_mat' ELSE do_mat END,
        so_den = CASE WHEN p ? 'so_den' THEN (p->>'so_den')::INT ELSE so_den END,
        ngay_den = CASE WHEN p ? 'ngay_den' THEN (p->>'ngay_den')::DATE ELSE ngay_den END,
        han_xu_ly = CASE WHEN p ? 'han_xu_ly' THEN (p->>'han_xu_ly')::DATE ELSE han_xu_ly END,
        giao_cho = CASE WHEN p ? 'giao_cho' THEN p->>'giao_cho' ELSE giao_cho END,
        can_cu = CASE WHEN p ? 'can_cu' THEN kho_mang(p->'can_cu') ELSE can_cu END,
        noi_nhan = CASE WHEN p ? 'noi_nhan' THEN kho_mang(p->'noi_nhan') ELSE noi_nhan END,
        nguoi_ky = CASE WHEN p ? 'nguoi_ky' THEN p->>'nguoi_ky' ELSE nguoi_ky END,
        chuc_danh = CASE WHEN p ? 'chuc_danh' THEN p->>'chuc_danh' ELSE chuc_danh END,
        noi_dung = CASE WHEN p ? 'noi_dung' THEN p->>'noi_dung' ELSE noi_dung END,
        tom_tat = CASE WHEN p ? 'tom_tat' THEN p->>'tom_tat' ELSE tom_tat END,
        the = CASE WHEN p ? 'the' THEN kho_mang(p->'the') ELSE the END,
        lien_quan = CASE WHEN p ? 'lien_quan' THEN kho_mang(p->'lien_quan') ELSE lien_quan END,
        gan_sao = CASE WHEN p ? 'gan_sao' THEN (p->>'gan_sao')::BOOLEAN ELSE gan_sao END,
        ghi_chu = CASE WHEN p ? 'ghi_chu' THEN p->>'ghi_chu' ELSE ghi_chu END,
        ban_sao = CASE WHEN p ? 'ban_sao'
                       THEN CASE WHEN jsonb_typeof(p->'ban_sao') = 'object' THEN p->'ban_sao' END
                       ELSE ban_sao END,
        cong_khai_noi_bo = CASE WHEN p_chu_loai = 'to_chuc' AND p ? 'cong_khai_noi_bo'
                                THEN (p->>'cong_khai_noi_bo')::BOOLEAN ELSE cong_khai_noi_bo END,
        sua_luc = now(),
        phien_ban = phien_ban + 1
     WHERE id = v.id
    RETURNING * INTO v;
    RETURN jsonb_build_object('ok', TRUE, 'van_ban', kho_van_ban_json_q(v, TRUE, v_vai, p_nguoi));
END;
$$;

CREATE OR REPLACE FUNCTION kho_van_ban_xoa_mem(p_chu_loai TEXT, p_chu_id UUID, p_id UUID, p_nguoi UUID) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE v kho_van_ban%ROWTYPE; v_vai TEXT := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
BEGIN
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    SELECT * INTO v FROM kho_van_ban WHERE id = p_id AND chu_loai = p_chu_loai AND chu_id = p_chu_id FOR UPDATE;
    IF NOT FOUND OR NOT kho_van_ban_xem_duoc(v, v_vai, p_nguoi) THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF NOT kho_van_ban_sua_duoc(v, v_vai, p_nguoi) THEN RETURN jsonb_build_object('loi', 'KHONG_DU_QUYEN'); END IF;
    UPDATE kho_van_ban SET xoa_luc = now(), sua_luc = now(), phien_ban = phien_ban + 1
     WHERE id = p_id AND xoa_luc IS NULL;
    RETURN jsonb_build_object('ok', TRUE);
END;
$$;

CREATE OR REPLACE FUNCTION kho_van_ban_khoi_phuc(p_chu_loai TEXT, p_chu_id UUID, p_id UUID, p_nguoi UUID) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE v kho_van_ban%ROWTYPE; v_vai TEXT := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
BEGIN
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    SELECT * INTO v FROM kho_van_ban WHERE id = p_id AND chu_loai = p_chu_loai AND chu_id = p_chu_id FOR UPDATE;
    IF NOT FOUND OR NOT kho_van_ban_xem_duoc(v, v_vai, p_nguoi) THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF NOT kho_van_ban_sua_duoc(v, v_vai, p_nguoi) THEN RETURN jsonb_build_object('loi', 'KHONG_DU_QUYEN'); END IF;
    IF v.xoa_luc IS NOT NULL THEN
        UPDATE kho_van_ban SET xoa_luc = NULL, sua_luc = now(), phien_ban = phien_ban + 1 WHERE id = p_id
        RETURNING * INTO v;
    END IF;
    RETURN jsonb_build_object('ok', TRUE, 'van_ban', kho_van_ban_json_q(v, TRUE, v_vai, p_nguoi));
END;
$$;

CREATE OR REPLACE FUNCTION kho_van_ban_xoa_han(p_chu_loai TEXT, p_chu_id UUID, p_id UUID, p_nguoi UUID) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE v kho_van_ban%ROWTYPE; v_vai TEXT := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
BEGIN
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    SELECT * INTO v FROM kho_van_ban WHERE id = p_id AND chu_loai = p_chu_loai AND chu_id = p_chu_id FOR UPDATE;
    IF NOT FOUND OR NOT kho_van_ban_xem_duoc(v, v_vai, p_nguoi) THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF NOT kho_van_ban_sua_duoc(v, v_vai, p_nguoi) THEN RETURN jsonb_build_object('loi', 'KHONG_DU_QUYEN'); END IF;
    IF v.xoa_luc IS NULL THEN RETURN jsonb_build_object('loi', 'CHUA_O_THUNG_RAC'); END IF;
    RETURN jsonb_build_object('ok', TRUE, 'duong_dan', to_jsonb(kho_xoa_van_ban_noi(p_id)));
END;
$$;

-- Như 015 + lọc phạm vi xem của người gọi (cán bộ). Tổng đếm cũng theo phạm vi.
CREATE OR REPLACE FUNCTION kho_van_ban_tim(p_chu_loai TEXT, p_chu_id UUID, p_loc JSONB, p_nguoi UUID) RETURNS JSONB
LANGUAGE plpgsql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE
    v_q TSQUERY; v_so INT; v_trang INT; v_tong BIGINT; v_muc JSONB;
    v_rac BOOLEAN := COALESCE((p_loc->>'thung_rac')::BOOLEAN, FALSE);
    v_xep_han BOOLEAN := COALESCE(p_loc->>'xep', '') = 'han_xu_ly';
    v_khan TEXT[] := CASE WHEN jsonb_typeof(p_loc->'do_khan') = 'array'
                          THEN ARRAY(SELECT jsonb_array_elements_text(p_loc->'do_khan')) END;
    v_vai TEXT := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
BEGIN
    IF v_vai IS NULL THEN RETURN jsonb_build_object('muc', '[]'::jsonb, 'tong', 0); END IF;
    v_so := LEAST(GREATEST(COALESCE((p_loc->>'so_dong')::INT, 20), 1), 100);
    v_trang := LEAST(GREATEST(COALESCE((p_loc->>'trang')::INT, 1), 1), 10000);
    SELECT to_tsquery('simple', string_agg(quote_literal(t) || ':*', ' & ')) INTO v_q
      FROM jsonb_array_elements_text(CASE WHEN jsonb_typeof(p_loc->'tu_khoa') = 'array'
                                          THEN p_loc->'tu_khoa' ELSE '[]'::jsonb END) AS t
     WHERE t ~ '^[a-z0-9]{1,40}$';

    WITH loc AS (
        SELECT v.id, v.ngay_ban_hanh, v.han_xu_ly, v.tao_luc,
               CASE WHEN v_q IS NULL THEN 0 ELSE ts_rank(v.tim, v_q) END AS hang
          FROM kho_van_ban v
         WHERE v.chu_loai = p_chu_loai AND v.chu_id = p_chu_id
           AND (v.xoa_luc IS NOT NULL) = v_rac
           AND (v_vai <> 'can_bo' OR kho_van_ban_xem_duoc(v, v_vai, p_nguoi))
           AND (v_q IS NULL OR v.tim @@ v_q)
           AND (p_loc->>'huong' IS NULL OR v.huong = p_loc->>'huong')
           AND (p_loc->>'loai' IS NULL OR v.loai = p_loc->>'loai')
           AND (p_loc->>'tu' IS NULL OR v.ngay_ban_hanh >= (p_loc->>'tu')::DATE)
           AND (p_loc->>'den' IS NULL OR v.ngay_ban_hanh <= (p_loc->>'den')::DATE)
           AND (p_loc->>'thu_muc' IS NULL OR v.thu_muc_id = (p_loc->>'thu_muc')::UUID)
           AND (p_loc->>'the' IS NULL OR v.the @> ARRAY[p_loc->>'the'])
           AND (p_loc->>'sao' IS NULL OR v.gan_sao = (p_loc->>'sao')::BOOLEAN)
           AND (v_khan IS NULL OR v.do_khan = ANY (v_khan))
           AND (p_loc->>'han_tu' IS NULL OR v.han_xu_ly >= (p_loc->>'han_tu')::DATE)
           AND (p_loc->>'han_den' IS NULL OR v.han_xu_ly <= (p_loc->>'han_den')::DATE)
    ), trang AS (
        SELECT id, hang, ngay_ban_hanh, han_xu_ly, tao_luc FROM loc
         ORDER BY CASE WHEN v_xep_han THEN han_xu_ly END ASC NULLS LAST,
                  hang DESC, ngay_ban_hanh DESC NULLS LAST, tao_luc DESC, id
         LIMIT v_so OFFSET (v_trang - 1) * v_so
    )
    SELECT (SELECT count(*) FROM loc),
           COALESCE((SELECT jsonb_agg(kho_van_ban_json_q(v, FALSE, v_vai, p_nguoi)
                                      ORDER BY CASE WHEN v_xep_han THEN tr.han_xu_ly END ASC NULLS LAST,
                                               tr.hang DESC, tr.ngay_ban_hanh DESC NULLS LAST, tr.tao_luc DESC, tr.id)
                       FROM trang tr JOIN kho_van_ban v ON v.id = tr.id), '[]'::jsonb)
      INTO v_tong, v_muc;
    RETURN jsonb_build_object('muc', v_muc, 'tong', v_tong);
END;
$$;

-- ---------- văn bản: bản cũ (giữ chữ ký) gọi bản mới với p_nguoi = NULL ----------
CREATE OR REPLACE FUNCTION kho_van_ban_tao(p_chu_loai TEXT, p_chu_id UUID, p_tao_boi UUID, p_du_lieu JSONB) RETURNS JSONB
LANGUAGE sql SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT kho_van_ban_tao(p_chu_loai, p_chu_id, p_tao_boi, p_du_lieu, NULL::UUID);
$$;
CREATE OR REPLACE FUNCTION kho_van_ban_doc(p_chu_loai TEXT, p_chu_id UUID, p_id UUID) RETURNS JSONB
LANGUAGE sql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT kho_van_ban_doc(p_chu_loai, p_chu_id, p_id, NULL::UUID);
$$;
CREATE OR REPLACE FUNCTION kho_van_ban_co(p_chu_loai TEXT, p_chu_id UUID, p_id UUID) RETURNS JSONB
LANGUAGE sql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT kho_van_ban_co(p_chu_loai, p_chu_id, p_id, NULL::UUID);
$$;
CREATE OR REPLACE FUNCTION kho_van_ban_sua(p_chu_loai TEXT, p_chu_id UUID, p_id UUID, p_phien_ban INT, p_du_lieu JSONB)
RETURNS JSONB
LANGUAGE sql SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT kho_van_ban_sua(p_chu_loai, p_chu_id, p_id, p_phien_ban, p_du_lieu, NULL::UUID);
$$;
CREATE OR REPLACE FUNCTION kho_van_ban_xoa_mem(p_chu_loai TEXT, p_chu_id UUID, p_id UUID) RETURNS JSONB
LANGUAGE sql SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT kho_van_ban_xoa_mem(p_chu_loai, p_chu_id, p_id, NULL::UUID);
$$;
CREATE OR REPLACE FUNCTION kho_van_ban_khoi_phuc(p_chu_loai TEXT, p_chu_id UUID, p_id UUID) RETURNS JSONB
LANGUAGE sql SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT kho_van_ban_khoi_phuc(p_chu_loai, p_chu_id, p_id, NULL::UUID);
$$;
CREATE OR REPLACE FUNCTION kho_van_ban_xoa_han(p_chu_loai TEXT, p_chu_id UUID, p_id UUID) RETURNS JSONB
LANGUAGE sql SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT kho_van_ban_xoa_han(p_chu_loai, p_chu_id, p_id, NULL::UUID);
$$;
CREATE OR REPLACE FUNCTION kho_van_ban_tim(p_chu_loai TEXT, p_chu_id UUID, p_loc JSONB) RETURNS JSONB
LANGUAGE sql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT kho_van_ban_tim(p_chu_loai, p_chu_id, p_loc, NULL::UUID);
$$;

DO $$
DECLARE f TEXT;
BEGIN
    FOREACH f IN ARRAY ARRAY[
        'kho_van_ban_tao(text,uuid,uuid,jsonb,uuid)', 'kho_van_ban_doc(text,uuid,uuid,uuid)',
        'kho_van_ban_co(text,uuid,uuid,uuid)', 'kho_van_ban_sua(text,uuid,uuid,integer,jsonb,uuid)',
        'kho_van_ban_xoa_mem(text,uuid,uuid,uuid)', 'kho_van_ban_khoi_phuc(text,uuid,uuid,uuid)',
        'kho_van_ban_xoa_han(text,uuid,uuid,uuid)', 'kho_van_ban_tim(text,uuid,jsonb,uuid)',
        'kho_van_ban_tao(text,uuid,uuid,jsonb)', 'kho_van_ban_doc(text,uuid,uuid)', 'kho_van_ban_co(text,uuid,uuid)',
        'kho_van_ban_sua(text,uuid,uuid,integer,jsonb)', 'kho_van_ban_xoa_mem(text,uuid,uuid)',
        'kho_van_ban_khoi_phuc(text,uuid,uuid)', 'kho_van_ban_xoa_han(text,uuid,uuid)', 'kho_van_ban_tim(text,uuid,jsonb)'
    ] LOOP
        EXECUTE format('REVOKE ALL ON FUNCTION %s FROM PUBLIC, anon, authenticated', f);
        EXECUTE format('GRANT EXECUTE ON FUNCTION %s TO service_role', f);
    END LOOP;
    FOREACH f IN ARRAY ARRAY[
        'kho_vai_tro(text,uuid,uuid)', 'kho_van_ban_xem_duoc(kho_van_ban,text,uuid)',
        'kho_van_ban_sua_duoc(kho_van_ban,text,uuid)', 'kho_van_ban_json(kho_van_ban,boolean)',
        'kho_van_ban_json_q(kho_van_ban,boolean,text,uuid)'
    ] LOOP
        EXECUTE format('REVOKE ALL ON FUNCTION %s FROM PUBLIC, anon, authenticated, service_role', f);
    END LOOP;
END;
$$;
```

Thêm `"migrations/018_giao_viec.sql",` vào cuối `TEP_LUOC_DO`.

- [ ] **Step 4: Chạy kiểm (mới + toàn bộ cũ)**

Run: `npx tsx --test tests/giao-viec-csdl.test.ts` rồi `npm test`
Expected: 9 bài mới PASS; mọi bài cũ (kho, mo-ban, hop, mau, credit…) PASS không sửa — đường cá nhân giờ đi qua bản mới.

- [ ] **Step 5: Commit**

```bash
git add supabase/migrations/018_giao_viec.sql tests/ho-tro/csdl-thu.ts tests/giao-viec-csdl.test.ts
git commit -m "feat(to-chuc): 018 phan A - giao viec, quyen van ban theo vai tro trong ham SQL, cong khai noi bo

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 5: Migration 018 phần B — tệp theo vai trò

**Files:**
- Modify: `supabase/migrations/018_giao_viec.sql` (nối cuối)
- Test: `tests/giao-viec-csdl.test.ts` (thêm `describe`)

**Interfaces:**
- Consumes: Task 4 (`kho_vai_tro`, `kho_van_ban_xem_duoc`, `kho_van_ban_sua_duoc`), 013 (`kho_gan_con`).
- Produces: `kho_tep_xem_duoc(kho_tep, text, uuid)`, `kho_tep_sua_duoc(kho_tep, text, uuid)` (nội bộ); bản mới
  `kho_tep_xin(p_chu_loai, p_chu_id, p_tao_boi, p_gan_loai, p_gan_id, p_ten, p_mime, p_byte, p_nguoi)`,
  `kho_tep_doc|xoa|bo(p_chu_loai, p_chu_id, p_id, p_nguoi)`, `kho_tep_xac_nhan(p_chu_loai, p_chu_id, p_id, p_byte, p_nguoi)`;
  bản cũ cùng tên gọi bản mới với `NULL`.

- [ ] **Step 1: Viết bài kiểm (thêm vào `tests/giao-viec-csdl.test.ts`)**

```ts
import { hetGoiToChuc } from "./ho-tro/to-chuc";

describe("018 phần B: tệp theo vai trò", { skip: !CO_CSDL_THU }, () => {
  let csdl: Awaited<ReturnType<typeof taoCsdlThu>>;
  let c: Client;
  let rpc: GoiRpc;
  let b: Bo;
  const vb: Record<string, string> = {};
  let hop: string;

  const taoVb = async (nguoi: string, trichYeu: string) =>
    (await rpc("kho_van_ban_tao", { ...chuTC(b.toChuc), p_tao_boi: nguoi, p_nguoi: nguoi,
      p_du_lieu: { huong: "den", trich_yeu: trichYeu } })).van_ban.id as string;
  /** Tệp đã xác nhận, chèn thẳng (bỏ qua Storage). */
  const tepXong = async (gan: string, ganId: string, nguoi: string) =>
    (await c.query(`INSERT INTO kho_tep (chu_loai, chu_id, gan_loai, gan_id, duong_dan, ten, mime, byte_khai, byte, trang_thai, tao_boi)
       VALUES ('to_chuc', $1, $2, $3, $5, 'a.pdf', 'application/pdf', 10, 10, 'xong', $4)
       RETURNING id`, [b.toChuc, gan, ganId, nguoi, `to_chuc/${b.toChuc}/${randomUUID()}`])).rows[0].id as string;
  const xin = (nguoi: string, gan: string, ganId: string, mime = "application/pdf") =>
    rpc("kho_tep_xin", { ...chuTC(b.toChuc), p_tao_boi: nguoi, p_gan_loai: gan, p_gan_id: ganId, p_ten: "a.pdf",
      p_mime: mime, p_byte: 10, p_nguoi: nguoi });
  const lam = (ham: string, nguoi: string, id: string) => rpc(ham, { ...chuTC(b.toChuc), p_id: id, p_nguoi: nguoi });

  before(async () => {
    csdl = await taoCsdlThu();
    c = new Client({ connectionString: csdl.url });
    await c.connect();
    rpc = rpcPg(c);
    b = await boToChuc(c);
    vb.rieng = await taoVb(b.quanTri, "Riêng");
    vb.canBo = await taoVb(b.canBo, "Của cán bộ");
    vb.giao = await taoVb(b.vanThu, "Được giao");
    await c.query(`INSERT INTO kho_giao_viec (chu_id, van_ban_id, nguoi_nhan, vai, giao_boi) VALUES ($1, $2, $3, 'phoi_hop', $4)`,
      [b.toChuc, vb.giao, b.canBo, b.vanThu]);
    hop = (await c.query(`INSERT INTO kho_cuoc_hop (chu_loai, chu_id, tieu_de, bat_dau, tao_boi)
                          VALUES ('to_chuc', $1, 'Giao ban', now(), $2) RETURNING id`, [b.toChuc, b.quanTri])).rows[0].id;
  });
  after(async () => {
    await c?.end();
    await csdl?.xoa();
  });

  test("xin tải lên: cán bộ chỉ vào văn bản do chính người đó tạo; văn bản được giao 403, ngoài phạm vi 404, cuộc họp 403", async () => {
    assert.equal((await xin(b.canBo, "van_ban", vb.canBo)).ok, true);
    assert.deepEqual(await xin(b.canBo, "van_ban", vb.giao), { loi: "KHONG_DU_QUYEN" });
    assert.deepEqual(await xin(b.canBo, "van_ban", vb.rieng), { loi: "KHONG_TIM_THAY" });
    assert.deepEqual(await xin(b.canBo, "cuoc_hop", hop), { loi: "KHONG_DU_QUYEN" });
    assert.equal((await xin(b.vanThu, "cuoc_hop", hop)).ok, true);
    assert.equal((await xin(b.vanThu, "van_ban", vb.rieng)).ok, true);
  });

  test("đọc / tải tệp: cán bộ không thấy tệp của văn bản ngoài phạm vi; thấy tệp văn bản được giao và cuộc họp", async () => {
    const tRieng = await tepXong("van_ban", vb.rieng, b.quanTri);
    const tGiao = await tepXong("van_ban", vb.giao, b.vanThu);
    const tHop = await tepXong("cuoc_hop", hop, b.quanTri);
    assert.equal(await lam("kho_tep_doc", b.canBo, tRieng), null);
    assert.ok(await lam("kho_tep_doc", b.canBo, tGiao));
    assert.ok(await lam("kho_tep_doc", b.canBo, tHop));
    assert.ok(await lam("kho_tep_doc", b.vanThu, tRieng));
    assert.equal(await lam("kho_tep_doc", await taoNguoiDung(c), tGiao), null);
  });

  test("xóa tệp: cán bộ không xóa tệp văn bản được giao / cuộc họp; xóa được tệp văn bản do chính người đó tạo", async () => {
    const tGiao = await tepXong("van_ban", vb.giao, b.vanThu);
    const tHop = await tepXong("cuoc_hop", hop, b.quanTri);
    const tMinh = await tepXong("van_ban", vb.canBo, b.canBo);
    assert.deepEqual(await lam("kho_tep_xoa", b.canBo, tGiao), { loi: "KHONG_DU_QUYEN" });
    assert.deepEqual(await lam("kho_tep_xoa", b.canBo, tHop), { loi: "KHONG_DU_QUYEN" });
    assert.equal((await lam("kho_tep_xoa", b.canBo, tMinh)).ok, true);
  });

  test("xác nhận / bỏ tệp chờ: cán bộ không đụng được tệp chờ của người khác", async () => {
    const x = await xin(b.vanThu, "van_ban", vb.canBo); // văn thư gắn tệp vào văn bản của cán bộ
    assert.deepEqual(await rpc("kho_tep_xac_nhan", { ...chuTC(b.toChuc), p_id: x.id, p_byte: 10, p_nguoi: b.canBo }),
      { loi: "KHONG_TIM_THAY" });
    assert.deepEqual(await lam("kho_tep_bo", b.canBo, x.id), { loi: "KHONG_TIM_THAY" });
    assert.equal((await rpc("kho_tep_xac_nhan", { ...chuTC(b.toChuc), p_id: x.id, p_byte: 10, p_nguoi: b.vanThu })).ok, true);
  });

  test("gói hết hạn: không xin tải lên được (kể cả quản trị)", async () => {
    const k = await boToChuc(c);
    const id = (await rpc("kho_van_ban_tao", { ...chuTC(k.toChuc), p_tao_boi: k.quanTri, p_nguoi: k.quanTri,
      p_du_lieu: { huong: "den", trich_yeu: "x" } })).van_ban.id;
    await hetGoiToChuc(c, k.toChuc);
    assert.deepEqual(await rpc("kho_tep_xin", { ...chuTC(k.toChuc), p_tao_boi: k.quanTri, p_gan_loai: "van_ban", p_gan_id: id,
      p_ten: "a.pdf", p_mime: "application/pdf", p_byte: 10, p_nguoi: k.quanTri }), { loi: "HET_DUNG_LUONG" });
    assert.deepEqual(await rpc("kho_van_ban_tao", { ...chuTC(k.toChuc), p_tao_boi: k.quanTri, p_nguoi: k.quanTri,
      p_du_lieu: { huong: "den", trich_yeu: "y" } }), { loi: "HET_DUNG_LUONG" });
  });

  test("bản cũ không p_nguoi: kho tổ chức bị từ chối; chữ ký cũ vẫn có quyền như trước", async () => {
    await assert.rejects(rpc("kho_tep_doc", { ...chuTC(b.toChuc), p_id: randomUUID() }), /thiếu người gọi/);
    const co = async (v: string, f: string) =>
      (await c.query("SELECT has_function_privilege($1, $2, 'EXECUTE') AS co", [v, f])).rows[0].co as boolean;
    for (const f of ["kho_tep_xin(text,uuid,uuid,text,uuid,text,text,bigint,uuid)", "kho_tep_doc(text,uuid,uuid,uuid)",
      "kho_tep_xac_nhan(text,uuid,uuid,bigint,uuid)", "kho_tep_xoa(text,uuid,uuid,uuid)", "kho_tep_bo(text,uuid,uuid,uuid)",
      "kho_tep_xin(text,uuid,uuid,text,uuid,text,text,bigint)", "kho_tep_bo(text,uuid,uuid)"]) {
      assert.equal(await co("service_role", f), true, f);
      assert.equal(await co("anon", f), false, f);
    }
    for (const f of ["kho_tep_xem_duoc(kho_tep,text,uuid)", "kho_tep_sua_duoc(kho_tep,text,uuid)"]) {
      assert.equal(await co("service_role", f), false, f);
    }
  });
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx tsx --test tests/giao-viec-csdl.test.ts`
Expected: FAIL — `function kho_tep_xin(... p_nguoi => uuid) does not exist`.

- [ ] **Step 3: Nối phần B vào `018_giao_viec.sql`**

```sql
-- ============================================================
-- Phần B: tệp theo vai trò. Cán bộ: thấy tệp của văn bản xem được + mọi tệp cuộc họp / album; gắn / xóa tệp chỉ ở văn bản
-- do chính người đó tạo; không đụng tệp chờ của người khác. Kho tổ chức hết gói: không xin tải lên (het_goi).
-- ============================================================

CREATE OR REPLACE FUNCTION kho_tep_xem_duoc(t kho_tep, p_vai TEXT, p_nguoi UUID) RETURNS BOOLEAN
LANGUAGE sql STABLE SET search_path = public, pg_temp AS $$
    SELECT COALESCE(p_vai IN ('chu', 'quan_tri', 'van_thu')
        OR (p_vai = 'can_bo' AND (t.gan_loai <> 'van_ban' OR t.tao_boi = p_nguoi
            OR EXISTS (SELECT 1 FROM kho_van_ban v WHERE v.id = t.gan_id AND kho_van_ban_xem_duoc(v, p_vai, p_nguoi)))), FALSE);
$$;

CREATE OR REPLACE FUNCTION kho_tep_sua_duoc(t kho_tep, p_vai TEXT, p_nguoi UUID) RETURNS BOOLEAN
LANGUAGE sql STABLE SET search_path = public, pg_temp AS $$
    SELECT COALESCE(p_vai IN ('chu', 'quan_tri', 'van_thu')
        OR (p_vai = 'can_bo' AND t.gan_loai = 'van_ban'
            AND EXISTS (SELECT 1 FROM kho_van_ban v WHERE v.id = t.gan_id AND kho_van_ban_sua_duoc(v, p_vai, p_nguoi))), FALSE);
$$;

-- Như 013 + vai trò + kiểm văn bản gắn ngay trong SQL + chặn khi kho tổ chức hết gói.
CREATE OR REPLACE FUNCTION kho_tep_xin(p_chu_loai TEXT, p_chu_id UUID, p_tao_boi UUID, p_gan_loai TEXT, p_gan_id UUID,
    p_ten TEXT, p_mime TEXT, p_byte BIGINT, p_nguoi UUID) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE v_da BIGINT; v_cho BIGINT; v_so_cho INT; v_id UUID := gen_random_uuid(); v_duong TEXT; vb kho_van_ban%ROWTYPE;
        v_vai TEXT := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
BEGIN
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF p_chu_loai = 'to_chuc' AND p_tao_boi IS DISTINCT FROM p_nguoi THEN
        RAISE EXCEPTION 'kho_tep_xin: người tải lên phải là người gọi';
    END IF;
    IF p_gan_loai = 'album' AND p_mime NOT IN ('image/jpeg', 'image/png', 'image/webp', 'image/gif') THEN
        RETURN kho_loi_dau_vao('Album chỉ nhận ảnh JPEG, PNG, WebP hoặc GIF.');
    END IF;
    IF p_gan_loai = 'van_ban' THEN
        SELECT * INTO vb FROM kho_van_ban
         WHERE id = p_gan_id AND chu_loai = p_chu_loai AND chu_id = p_chu_id AND xoa_luc IS NULL;
        IF NOT FOUND OR NOT kho_van_ban_xem_duoc(vb, v_vai, p_nguoi) THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
        IF NOT kho_van_ban_sua_duoc(vb, v_vai, p_nguoi) THEN RETURN jsonb_build_object('loi', 'KHONG_DU_QUYEN'); END IF;
    ELSIF v_vai = 'can_bo' THEN
        RETURN jsonb_build_object('loi', 'KHONG_DU_QUYEN');
    END IF;
    IF (kho_dung_luong_xem(p_chu_loai, p_chu_id)->>'het_goi')::BOOLEAN THEN
        RETURN jsonb_build_object('loi', 'HET_DUNG_LUONG');
    END IF;
    v_da := kho_khoa_dung_luong(p_chu_loai, p_chu_id);
    IF p_gan_loai IN ('cuoc_hop', 'album') AND NOT kho_gan_con(p_gan_loai, p_gan_id) THEN
        RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY');
    END IF;
    SELECT count(*), COALESCE(sum(byte_khai), 0) INTO v_so_cho, v_cho FROM kho_tep
     WHERE chu_loai = p_chu_loai AND chu_id = p_chu_id AND trang_thai = 'cho' AND het_han_cho > now();
    IF v_so_cho >= 20 THEN RETURN jsonb_build_object('loi', 'QUA_NHIEU_YEU_CAU'); END IF;
    IF v_da + v_cho + p_byte > (kho_han_muc(p_chu_loai, p_chu_id)->>'han_muc_byte')::BIGINT THEN
        RETURN jsonb_build_object('loi', 'HET_DUNG_LUONG');
    END IF;
    v_duong := p_chu_loai || '/' || p_chu_id::TEXT || '/' || v_id::TEXT;
    INSERT INTO kho_tep (id, chu_loai, chu_id, gan_loai, gan_id, duong_dan, ten, mime, byte_khai, trang_thai, het_han_cho, tao_boi)
    VALUES (v_id, p_chu_loai, p_chu_id, p_gan_loai, p_gan_id, v_duong, p_ten, p_mime, p_byte, 'cho',
            now() + interval '1 hour', p_tao_boi);
    RETURN jsonb_build_object('ok', TRUE, 'id', v_id, 'duong_dan', v_duong);
END;
$$;

CREATE OR REPLACE FUNCTION kho_tep_doc(p_chu_loai TEXT, p_chu_id UUID, p_id UUID, p_nguoi UUID) RETURNS JSONB
LANGUAGE plpgsql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE t kho_tep%ROWTYPE; v_vai TEXT := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
BEGIN
    IF v_vai IS NULL THEN RETURN NULL; END IF;
    SELECT * INTO t FROM kho_tep WHERE id = p_id AND chu_loai = p_chu_loai AND chu_id = p_chu_id;
    IF NOT FOUND OR NOT kho_tep_xem_duoc(t, v_vai, p_nguoi) THEN RETURN NULL; END IF;
    RETURN jsonb_build_object('id', t.id, 'duong_dan', t.duong_dan, 'ten', t.ten, 'mime', t.mime,
                              'byte_khai', t.byte_khai, 'trang_thai', t.trang_thai, 'tep', kho_tep_json(t));
END;
$$;

CREATE OR REPLACE FUNCTION kho_tep_xac_nhan(p_chu_loai TEXT, p_chu_id UUID, p_id UUID, p_byte BIGINT, p_nguoi UUID) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE v_da BIGINT; t kho_tep%ROWTYPE; v_vai TEXT := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
BEGIN
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    v_da := kho_khoa_dung_luong(p_chu_loai, p_chu_id);
    SELECT * INTO t FROM kho_tep WHERE id = p_id AND chu_loai = p_chu_loai AND chu_id = p_chu_id FOR UPDATE;
    IF NOT FOUND OR (v_vai = 'can_bo' AND t.tao_boi IS DISTINCT FROM p_nguoi) THEN
        RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY');
    END IF;
    IF t.trang_thai = 'xong' THEN RETURN jsonb_build_object('ok', TRUE, 'tep', kho_tep_json(t)); END IF;
    IF NOT kho_gan_con(t.gan_loai, t.gan_id) THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF v_da + p_byte > (kho_han_muc(p_chu_loai, p_chu_id)->>'han_muc_byte')::BIGINT THEN
        RETURN jsonb_build_object('loi', 'HET_DUNG_LUONG');
    END IF;
    UPDATE kho_tep SET trang_thai = 'xong', byte = p_byte, het_han_cho = NULL, xac_nhan_luc = now(), sua_luc = now(),
                       phien_ban = phien_ban + 1
     WHERE id = t.id RETURNING * INTO t;
    UPDATE kho_dung_luong SET byte_da_dung = byte_da_dung + p_byte, sua_luc = now()
     WHERE chu_loai = p_chu_loai AND chu_id = p_chu_id;
    RETURN jsonb_build_object('ok', TRUE, 'tep', kho_tep_json(t));
END;
$$;

CREATE OR REPLACE FUNCTION kho_tep_xoa(p_chu_loai TEXT, p_chu_id UUID, p_id UUID, p_nguoi UUID) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE t kho_tep%ROWTYPE; v_vai TEXT := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
BEGIN
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    PERFORM kho_khoa_dung_luong(p_chu_loai, p_chu_id);
    SELECT * INTO t FROM kho_tep WHERE id = p_id AND chu_loai = p_chu_loai AND chu_id = p_chu_id FOR UPDATE;
    IF NOT FOUND OR NOT kho_tep_xem_duoc(t, v_vai, p_nguoi) THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF NOT kho_tep_sua_duoc(t, v_vai, p_nguoi) THEN RETURN jsonb_build_object('loi', 'KHONG_DU_QUYEN'); END IF;
    IF t.trang_thai = 'cho' THEN
        UPDATE kho_tep SET het_han_cho = LEAST(het_han_cho, now() + interval '3 minutes'), sua_luc = now() WHERE id = t.id;
        RETURN jsonb_build_object('ok', TRUE, 'duong_dan', NULL);
    END IF;
    DELETE FROM kho_tep WHERE id = t.id;
    UPDATE kho_dung_luong SET byte_da_dung = GREATEST(0, byte_da_dung - t.byte), sua_luc = now()
     WHERE chu_loai = p_chu_loai AND chu_id = p_chu_id;
    RETURN jsonb_build_object('ok', TRUE, 'duong_dan', t.duong_dan);
END;
$$;

CREATE OR REPLACE FUNCTION kho_tep_bo(p_chu_loai TEXT, p_chu_id UUID, p_id UUID, p_nguoi UUID) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE t kho_tep%ROWTYPE; v_vai TEXT := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
BEGIN
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    PERFORM kho_khoa_dung_luong(p_chu_loai, p_chu_id);
    SELECT * INTO t FROM kho_tep
     WHERE id = p_id AND chu_loai = p_chu_loai AND chu_id = p_chu_id AND trang_thai = 'cho' FOR UPDATE;
    IF NOT FOUND OR (v_vai = 'can_bo' AND t.tao_boi IS DISTINCT FROM p_nguoi) THEN
        RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY');
    END IF;
    UPDATE kho_tep SET het_han_cho = LEAST(het_han_cho, now() + interval '3 minutes'), sua_luc = now() WHERE id = t.id;
    RETURN jsonb_build_object('ok', TRUE, 'duong_dan', t.duong_dan);
END;
$$;

-- Bản cũ (giữ chữ ký) gọi bản mới với p_nguoi = NULL.
CREATE OR REPLACE FUNCTION kho_tep_xin(p_chu_loai TEXT, p_chu_id UUID, p_tao_boi UUID, p_gan_loai TEXT, p_gan_id UUID,
    p_ten TEXT, p_mime TEXT, p_byte BIGINT) RETURNS JSONB
LANGUAGE sql SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT kho_tep_xin(p_chu_loai, p_chu_id, p_tao_boi, p_gan_loai, p_gan_id, p_ten, p_mime, p_byte, NULL::UUID);
$$;
CREATE OR REPLACE FUNCTION kho_tep_doc(p_chu_loai TEXT, p_chu_id UUID, p_id UUID) RETURNS JSONB
LANGUAGE sql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT kho_tep_doc(p_chu_loai, p_chu_id, p_id, NULL::UUID);
$$;
CREATE OR REPLACE FUNCTION kho_tep_xac_nhan(p_chu_loai TEXT, p_chu_id UUID, p_id UUID, p_byte BIGINT) RETURNS JSONB
LANGUAGE sql SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT kho_tep_xac_nhan(p_chu_loai, p_chu_id, p_id, p_byte, NULL::UUID);
$$;
CREATE OR REPLACE FUNCTION kho_tep_xoa(p_chu_loai TEXT, p_chu_id UUID, p_id UUID) RETURNS JSONB
LANGUAGE sql SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT kho_tep_xoa(p_chu_loai, p_chu_id, p_id, NULL::UUID);
$$;
CREATE OR REPLACE FUNCTION kho_tep_bo(p_chu_loai TEXT, p_chu_id UUID, p_id UUID) RETURNS JSONB
LANGUAGE sql SECURITY DEFINER SET search_path = public, pg_temp AS $$
    SELECT kho_tep_bo(p_chu_loai, p_chu_id, p_id, NULL::UUID);
$$;

DO $$
DECLARE f TEXT;
BEGIN
    FOREACH f IN ARRAY ARRAY[
        'kho_tep_xin(text,uuid,uuid,text,uuid,text,text,bigint,uuid)', 'kho_tep_doc(text,uuid,uuid,uuid)',
        'kho_tep_xac_nhan(text,uuid,uuid,bigint,uuid)', 'kho_tep_xoa(text,uuid,uuid,uuid)', 'kho_tep_bo(text,uuid,uuid,uuid)',
        'kho_tep_xin(text,uuid,uuid,text,uuid,text,text,bigint)', 'kho_tep_doc(text,uuid,uuid)',
        'kho_tep_xac_nhan(text,uuid,uuid,bigint)', 'kho_tep_xoa(text,uuid,uuid)', 'kho_tep_bo(text,uuid,uuid)'
    ] LOOP
        EXECUTE format('REVOKE ALL ON FUNCTION %s FROM PUBLIC, anon, authenticated', f);
        EXECUTE format('GRANT EXECUTE ON FUNCTION %s TO service_role', f);
    END LOOP;
    FOREACH f IN ARRAY ARRAY['kho_tep_xem_duoc(kho_tep,text,uuid)', 'kho_tep_sua_duoc(kho_tep,text,uuid)'] LOOP
        EXECUTE format('REVOKE ALL ON FUNCTION %s FROM PUBLIC, anon, authenticated, service_role', f);
    END LOOP;
END;
$$;
```

- [ ] **Step 4: Chạy kiểm**

Run: `npx tsx --test tests/giao-viec-csdl.test.ts tests/kho-csdl.test.ts tests/kho-dich-vu.test.ts tests/hop-csdl.test.ts tests/hop-dich-vu.test.ts`
Expected: tất cả PASS. Lưu ý: kho cá nhân giờ kiểm văn bản gắn ngay trong `kho_tep_xin` (trước chỉ kiểm ở API) — bài cũ
không gửi văn bản lạ nên vẫn xanh; nếu bài nào hỏng vì gửi `gan_id` không tồn tại, dừng lại báo controller (không sửa bài cũ).

- [ ] **Step 5: Commit**

```bash
git add supabase/migrations/018_giao_viec.sql tests/giao-viec-csdl.test.ts
git commit -m "feat(to-chuc): 018 phan B - tep theo vai tro, chan tai len khi het goi

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 6: Migration 018 phần C — hàm giao việc

**Files:**
- Modify: `supabase/migrations/018_giao_viec.sql` (nối cuối)
- Test: `tests/giao-viec-csdl.test.ts` (thêm `describe`)

**Interfaces:**
- Consumes: Task 4.
- Produces (rpc): `kho_giao_viec_ds(p_chu_loai, p_chu_id, p_nguoi, p_van_ban) → { muc, nhat_ky, quyen: { giao } } | null`;
  `kho_giao_viec_tao(p_chu_loai, p_chu_id, p_nguoi, p_van_ban, p_du_lieu) → { ok, giao_viec } | { loi }`;
  `kho_giao_viec_sua(p_chu_loai, p_chu_id, p_nguoi, p_id, p_phien_ban, p_du_lieu) → { ok, giao_viec } | { loi, ban_moi? }`;
  `kho_giao_viec_thu_hoi(p_chu_loai, p_chu_id, p_nguoi, p_id) → { ok } | { loi }`;
  `kho_giao_viec_cua_toi(p_chu_loai, p_chu_id, p_nguoi, p_loc) → { muc, tong }`. `GiaoViec` / `NhatKyGiaoViec` /
  `ViecDuocGiao` đúng hợp đồng.

- [ ] **Step 1: Viết bài kiểm (thêm vào `tests/giao-viec-csdl.test.ts`)**

```ts
describe("018 phần C: giao việc", { skip: !CO_CSDL_THU }, () => {
  let csdl: Awaited<ReturnType<typeof taoCsdlThu>>;
  let c: Client;
  let rpc: GoiRpc;
  let b: Bo;

  const taoVb = async (than: Record<string, unknown> = {}) =>
    (await rpc("kho_van_ban_tao", { ...chuTC(b.toChuc), p_tao_boi: b.vanThu, p_nguoi: b.vanThu,
      p_du_lieu: { huong: "den", trich_yeu: "Giao xử lý", han_xu_ly: "2099-01-10", ...than } })).van_ban.id as string;
  const giao = (ai: string, vbId: string, d: Record<string, unknown>) =>
    rpc("kho_giao_viec_tao", { ...chuTC(b.toChuc), p_nguoi: ai, p_van_ban: vbId, p_du_lieu: d });
  const sua = (ai: string, id: string, pb: number, d: Record<string, unknown>) =>
    rpc("kho_giao_viec_sua", { ...chuTC(b.toChuc), p_nguoi: ai, p_id: id, p_phien_ban: pb, p_du_lieu: d });
  const cuaToi = (ai: string, loc: Record<string, unknown> = {}) =>
    rpc("kho_giao_viec_cua_toi", { ...chuTC(b.toChuc), p_nguoi: ai, p_loc: loc });

  before(async () => {
    csdl = await taoCsdlThu();
    c = new Client({ connectionString: csdl.url });
    await c.connect();
    rpc = rpcPg(c);
    b = await boToChuc(c);
  });
  after(async () => {
    await c?.end();
    await csdl?.xoa();
  });

  test("giao: văn thư giao chủ trì + phối hợp; hạn mặc định theo văn bản; cán bộ giao → 403; một chủ trì; không giao trùng", async () => {
    const vb = await taoVb();
    const g1 = await giao(b.vanThu, vb, { nguoi_nhan: b.canBo, vai: "chu_tri", y_kien_chi_dao: "Tham mưu trước 05/01" });
    assert.equal(g1.ok, true);
    assert.equal(g1.giao_viec.han_xu_ly, "2099-01-10");
    assert.equal(g1.giao_viec.trang_thai, "cho");
    assert.equal(g1.giao_viec.nguoi_nhan_ten, "Người thử");
    assert.deepEqual(await giao(b.canBo, vb, { nguoi_nhan: b.canBo2, vai: "phoi_hop" }), { loi: "KHONG_DU_QUYEN" });
    assert.deepEqual(await giao(b.quanTri, vb, { nguoi_nhan: b.canBo2, vai: "chu_tri" }), { loi: "CHU_TRI_DA_CO" });
    assert.deepEqual(await giao(b.quanTri, vb, { nguoi_nhan: b.canBo, vai: "phoi_hop" }), { loi: "DA_GIAO" });
    assert.deepEqual(await giao(b.quanTri, vb, { nguoi_nhan: await taoNguoiDung(c), vai: "phoi_hop" }), { loi: "NGUOI_NHAN_KHONG_HOP_LE" });
    const g2 = await giao(b.quanTri, vb, { nguoi_nhan: b.canBo2, vai: "phoi_hop", han_xu_ly: null });
    assert.equal(g2.giao_viec.han_xu_ly, null);
    // phối hợp được giao thì xem được văn bản
    assert.ok(await rpc("kho_van_ban_doc", { ...chuTC(b.toChuc), p_id: vb, p_nguoi: b.canBo2 }));
  });

  test("cán bộ: đổi trạng thái + ghi chú việc của chính người đó; không đổi hạn / người nhận / việc người khác", async () => {
    const vb = await taoVb();
    const g = (await giao(b.vanThu, vb, { nguoi_nhan: b.canBo, vai: "chu_tri" })).giao_viec;
    const p = (await giao(b.vanThu, vb, { nguoi_nhan: b.canBo2, vai: "phoi_hop" })).giao_viec;
    assert.deepEqual(await sua(b.canBo, g.id, 1, { han_xu_ly: "2099-02-01" }), { loi: "KHONG_DU_QUYEN" });
    assert.deepEqual(await sua(b.canBo, g.id, 1, { nguoi_nhan: b.canBo2 }), { loi: "KHONG_DU_QUYEN" });
    assert.deepEqual(await sua(b.canBo, p.id, 1, { trang_thai: "xong" }), { loi: "KHONG_DU_QUYEN" });
    const s = await sua(b.canBo, g.id, 1, { trang_thai: "dang", ghi_chu: "Đang lấy ý kiến phòng" });
    assert.equal(s.giao_viec.trang_thai, "dang");
    assert.equal(s.giao_viec.phien_ban, 2);
    const x = await sua(b.canBo, g.id, 1, { trang_thai: "xong" });
    assert.equal(x.loi, "XUNG_DOT_PHIEN_BAN");
    assert.equal(x.ban_moi.trang_thai, "dang");
  });

  test("văn thư sửa giao: đổi người chủ trì; nâng phối hợp lên chủ trì khi đã có chủ trì → CHU_TRI_DA_CO", async () => {
    const vb = await taoVb();
    const g = (await giao(b.vanThu, vb, { nguoi_nhan: b.canBo, vai: "chu_tri" })).giao_viec;
    const p = (await giao(b.vanThu, vb, { nguoi_nhan: b.canBo2, vai: "phoi_hop" })).giao_viec;
    assert.deepEqual(await sua(b.vanThu, p.id, 1, { vai: "chu_tri" }), { loi: "CHU_TRI_DA_CO" });
    assert.deepEqual(await sua(b.vanThu, g.id, 1, { nguoi_nhan: b.canBo2 }), { loi: "DA_GIAO" });
    assert.equal((await sua(b.vanThu, g.id, 1, { vai: "phoi_hop" })).ok, true);
    assert.equal((await sua(b.vanThu, p.id, 1, { vai: "chu_tri", han_xu_ly: "2099-03-01" })).giao_viec.han_xu_ly, "2099-03-01");
  });

  test("thu hồi: cán bộ không thu hồi được; thu hồi xong cán bộ mất quyền xem văn bản; nhật ký ghi đủ, chỉ ghi thêm", async () => {
    const vb = await taoVb();
    const g = (await giao(b.vanThu, vb, { nguoi_nhan: b.canBo, vai: "chu_tri" })).giao_viec;
    await sua(b.canBo, g.id, 1, { trang_thai: "dang" });
    assert.deepEqual(await rpc("kho_giao_viec_thu_hoi", { ...chuTC(b.toChuc), p_nguoi: b.canBo, p_id: g.id }), { loi: "KHONG_DU_QUYEN" });
    assert.equal((await rpc("kho_giao_viec_thu_hoi", { ...chuTC(b.toChuc), p_nguoi: b.vanThu, p_id: g.id })).ok, true);
    assert.equal(await rpc("kho_van_ban_doc", { ...chuTC(b.toChuc), p_id: vb, p_nguoi: b.canBo }), null);
    const ds = await rpc("kho_giao_viec_ds", { ...chuTC(b.toChuc), p_nguoi: b.vanThu, p_van_ban: vb });
    assert.deepEqual(ds.muc, []);
    assert.deepEqual(ds.nhat_ky.map((n: any) => n.hanh_dong), ["thu_hoi", "trang_thai", "giao"]);
    assert.deepEqual(ds.nhat_ky[1].truoc, { trang_thai: "cho" });
    assert.deepEqual(ds.nhat_ky[1].sau, { trang_thai: "dang" });
    assert.equal(ds.quyen.giao, true);
    await assert.rejects(c.query(`DELETE FROM kho_giao_viec_nhat_ky WHERE giao_viec_id = $1`, [g.id]), /chỉ ghi thêm/);
    await assert.rejects(c.query(`UPDATE kho_giao_viec_nhat_ky SET hanh_dong = 'sua' WHERE giao_viec_id = $1`, [g.id]), /chỉ ghi thêm/);
  });

  test("danh sách của văn bản: cán bộ ngoài phạm vi → null; xóa hẳn văn bản kéo theo giao việc + nhật ký", async () => {
    const vb = await taoVb();
    await giao(b.vanThu, vb, { nguoi_nhan: b.canBo, vai: "chu_tri" });
    assert.equal(await rpc("kho_giao_viec_ds", { ...chuTC(b.toChuc), p_nguoi: b.canBo2, p_van_ban: vb }), null);
    assert.equal((await rpc("kho_giao_viec_ds", { ...chuTC(b.toChuc), p_nguoi: b.canBo, p_van_ban: vb })).quyen.giao, false);
    await rpc("kho_van_ban_xoa_mem", { ...chuTC(b.toChuc), p_id: vb, p_nguoi: b.vanThu });
    assert.equal((await rpc("kho_van_ban_xoa_han", { ...chuTC(b.toChuc), p_id: vb, p_nguoi: b.vanThu })).ok, true);
    const { rows } = await c.query(`SELECT count(*)::int AS n FROM kho_giao_viec WHERE van_ban_id = $1`, [vb]);
    assert.equal(rows[0].n, 0);
  });

  test("việc được giao: chỉ việc của người gọi, chưa xong trước, lọc trạng thái + quá hạn; kho cá nhân rỗng", async () => {
    const k = await boToChuc(c);
    const save = b;
    b = k;
    try {
      const v1 = await taoVb({ han_xu_ly: "2000-01-01", trich_yeu: "Quá hạn" });
      const v2 = await taoVb({ han_xu_ly: "2099-01-01", trich_yeu: "Còn hạn" });
      const v3 = await taoVb({ trich_yeu: "Đã xong" });
      const v4 = await taoVb({ trich_yeu: "Của người khác" });
      await giao(b.vanThu, v1, { nguoi_nhan: b.canBo, vai: "chu_tri" });
      await giao(b.vanThu, v2, { nguoi_nhan: b.canBo, vai: "phoi_hop" });
      const g3 = (await giao(b.vanThu, v3, { nguoi_nhan: b.canBo, vai: "chu_tri" })).giao_viec;
      await sua(b.canBo, g3.id, 1, { trang_thai: "xong" });
      await giao(b.vanThu, v4, { nguoi_nhan: b.canBo2, vai: "chu_tri" });
      const tat = await cuaToi(b.canBo);
      assert.equal(tat.tong, 3);
      assert.deepEqual(tat.muc.map((m: any) => m.van_ban.trich_yeu), ["Quá hạn", "Còn hạn", "Đã xong"]);
      assert.equal((await cuaToi(b.canBo, { qua_han: true })).tong, 1);
      assert.equal((await cuaToi(b.canBo, { trang_thai: ["xong"] })).tong, 1);
      const u = await taoNguoiDung(c);
      assert.deepEqual(await rpc("kho_giao_viec_cua_toi", { ...chuCua(u), p_nguoi: u, p_loc: {} }), { muc: [], tong: 0 });
    } finally {
      b = save;
    }
  });

  test("IDOR: giao việc của tổ chức khác → 404", async () => {
    const vb = await taoVb();
    const g = (await giao(b.vanThu, vb, { nguoi_nhan: b.canBo, vai: "chu_tri" })).giao_viec;
    const k = await boToChuc(c);
    assert.deepEqual(await rpc("kho_giao_viec_sua", { ...chuTC(k.toChuc), p_nguoi: k.quanTri, p_id: g.id, p_phien_ban: 1,
      p_du_lieu: { trang_thai: "xong" } }), { loi: "KHONG_TIM_THAY" });
    assert.deepEqual(await rpc("kho_giao_viec_thu_hoi", { ...chuTC(k.toChuc), p_nguoi: k.quanTri, p_id: g.id }), { loi: "KHONG_TIM_THAY" });
    assert.deepEqual(await rpc("kho_giao_viec_tao", { ...chuTC(k.toChuc), p_nguoi: k.quanTri, p_van_ban: vb,
      p_du_lieu: { nguoi_nhan: k.canBo, vai: "chu_tri" } }), { loi: "KHONG_TIM_THAY" });
  });
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx tsx --test tests/giao-viec-csdl.test.ts`
Expected: FAIL — `function kho_giao_viec_tao(...) does not exist`.

- [ ] **Step 3: Nối phần C vào `018_giao_viec.sql`**

```sql
-- ============================================================
-- Phần C: giao việc (chỉ kho tổ chức). Khóa dòng văn bản trước khi giao / sửa / thu hồi → các thay đổi giao việc của
-- một văn bản xếp hàng (luật một chủ trì, không giao trùng không chạy đua).
-- ============================================================

CREATE OR REPLACE FUNCTION kho_giao_viec_json(g kho_giao_viec) RETURNS JSONB
LANGUAGE sql STABLE SET search_path = public, pg_temp AS $$
    SELECT jsonb_build_object(
        'id', g.id, 'van_ban_id', g.van_ban_id, 'nguoi_nhan', g.nguoi_nhan,
        'nguoi_nhan_ten', (SELECT u.full_name FROM users u WHERE u.id = g.nguoi_nhan),
        'vai', g.vai, 'han_xu_ly', g.han_xu_ly, 'y_kien_chi_dao', g.y_kien_chi_dao, 'trang_thai', g.trang_thai,
        'ghi_chu', g.ghi_chu, 'giao_boi', g.giao_boi, 'giao_boi_ten', (SELECT u.full_name FROM users u WHERE u.id = g.giao_boi),
        'giao_luc', g.giao_luc, 'cap_nhat_luc', g.cap_nhat_luc, 'phien_ban', g.phien_ban);
$$;

CREATE OR REPLACE FUNCTION kho_giao_viec_ghi(p_gv UUID, p_nguoi UUID, p_hanh_dong TEXT, p_truoc JSONB, p_sau JSONB) RETURNS VOID
LANGUAGE sql SET search_path = public, pg_temp AS $$
    INSERT INTO kho_giao_viec_nhat_ky (giao_viec_id, nguoi, hanh_dong, truoc, sau) VALUES (p_gv, p_nguoi, p_hanh_dong, p_truoc, p_sau);
$$;

CREATE OR REPLACE FUNCTION kho_giao_viec_ds(p_chu_loai TEXT, p_chu_id UUID, p_nguoi UUID, p_van_ban UUID) RETURNS JSONB
LANGUAGE plpgsql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE v kho_van_ban%ROWTYPE; v_vai TEXT;
BEGIN
    IF p_chu_loai <> 'to_chuc' THEN RETURN NULL; END IF;
    v_vai := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
    IF v_vai IS NULL THEN RETURN NULL; END IF;
    SELECT * INTO v FROM kho_van_ban WHERE id = p_van_ban AND chu_loai = p_chu_loai AND chu_id = p_chu_id;
    IF NOT FOUND OR NOT kho_van_ban_xem_duoc(v, v_vai, p_nguoi) THEN RETURN NULL; END IF;
    RETURN jsonb_build_object(
        'muc', COALESCE((SELECT jsonb_agg(kho_giao_viec_json(g) ORDER BY g.vai, g.giao_luc, g.id)
                           FROM kho_giao_viec g WHERE g.van_ban_id = v.id AND g.huy_luc IS NULL), '[]'::jsonb),
        'nhat_ky', COALESCE((SELECT jsonb_agg(x.j ORDER BY x.luc DESC, x.id DESC) FROM (
                     SELECT n.id, n.luc, jsonb_build_object('id', n.id, 'giao_viec_id', n.giao_viec_id, 'nguoi_ten', u.full_name,
                                   'hanh_dong', n.hanh_dong, 'truoc', n.truoc, 'sau', n.sau, 'luc', n.luc) AS j
                       FROM kho_giao_viec_nhat_ky n JOIN kho_giao_viec g ON g.id = n.giao_viec_id
                       LEFT JOIN users u ON u.id = n.nguoi
                      WHERE g.van_ban_id = v.id ORDER BY n.luc DESC, n.id DESC LIMIT 200) x), '[]'::jsonb),
        'quyen', jsonb_build_object('giao', v_vai IN ('quan_tri', 'van_thu')));
END;
$$;

-- p_du_lieu đã kiểm ở API (kiemGiaoViec "tao"): { nguoi_nhan: uuid, vai, han_xu_ly?: 'YYYY-MM-DD' | null, y_kien_chi_dao? }.
-- Thiếu khóa han_xu_ly → lấy hạn của văn bản; có khóa mà null → không hạn.
CREATE OR REPLACE FUNCTION kho_giao_viec_tao(p_chu_loai TEXT, p_chu_id UUID, p_nguoi UUID, p_van_ban UUID, p_du_lieu JSONB)
RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE p JSONB := p_du_lieu; v kho_van_ban%ROWTYPE; g kho_giao_viec%ROWTYPE; v_vai TEXT; v_nhan UUID;
BEGIN
    IF p_chu_loai <> 'to_chuc' THEN RETURN kho_loi_dau_vao('Giao việc chỉ có ở kho tổ chức.'); END IF;
    v_vai := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    SELECT * INTO v FROM kho_van_ban WHERE id = p_van_ban AND chu_loai = p_chu_loai AND chu_id = p_chu_id FOR UPDATE;
    IF NOT FOUND OR NOT kho_van_ban_xem_duoc(v, v_vai, p_nguoi) THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF v_vai NOT IN ('quan_tri', 'van_thu') THEN RETURN jsonb_build_object('loi', 'KHONG_DU_QUYEN'); END IF;
    IF v.xoa_luc IS NOT NULL THEN RETURN jsonb_build_object('loi', 'DANG_O_THUNG_RAC'); END IF;
    v_nhan := (p->>'nguoi_nhan')::UUID;
    IF to_chuc_vai_tro(p_chu_id, v_nhan) IS NULL THEN RETURN jsonb_build_object('loi', 'NGUOI_NHAN_KHONG_HOP_LE'); END IF;
    IF p->>'vai' = 'chu_tri' AND EXISTS (SELECT 1 FROM kho_giao_viec
                                          WHERE van_ban_id = v.id AND vai = 'chu_tri' AND huy_luc IS NULL) THEN
        RETURN jsonb_build_object('loi', 'CHU_TRI_DA_CO');
    END IF;
    IF EXISTS (SELECT 1 FROM kho_giao_viec WHERE van_ban_id = v.id AND nguoi_nhan = v_nhan AND huy_luc IS NULL) THEN
        RETURN jsonb_build_object('loi', 'DA_GIAO');
    END IF;
    INSERT INTO kho_giao_viec (chu_id, van_ban_id, nguoi_nhan, vai, han_xu_ly, y_kien_chi_dao, giao_boi)
    VALUES (p_chu_id, v.id, v_nhan, p->>'vai',
            CASE WHEN p ? 'han_xu_ly' THEN (p->>'han_xu_ly')::DATE ELSE v.han_xu_ly END,
            p->>'y_kien_chi_dao', p_nguoi)
    RETURNING * INTO g;
    PERFORM kho_giao_viec_ghi(g.id, p_nguoi, 'giao', NULL, kho_giao_viec_json(g));
    RETURN jsonb_build_object('ok', TRUE, 'giao_viec', kho_giao_viec_json(g));
END;
$$;

-- p_du_lieu đã kiểm ở API (kiemGiaoViec "sua"): khóa trong { trang_thai, ghi_chu, han_xu_ly, y_kien_chi_dao, vai, nguoi_nhan }.
-- Cán bộ: chỉ trang_thai, ghi_chu của việc được giao cho chính người đó. Nhật ký ghi đúng các khóa đã gửi (trước → sau).
CREATE OR REPLACE FUNCTION kho_giao_viec_sua(p_chu_loai TEXT, p_chu_id UUID, p_nguoi UUID, p_id UUID, p_phien_ban INT,
    p_du_lieu JSONB) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE p JSONB := p_du_lieu; g kho_giao_viec%ROWTYPE; v kho_van_ban%ROWTYPE; v_vai TEXT; v_van_ban UUID;
        v_truoc JSONB; v_sau JSONB; v_nhan UUID;
BEGIN
    IF p_chu_loai <> 'to_chuc' THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    v_vai := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    SELECT van_ban_id INTO v_van_ban FROM kho_giao_viec WHERE id = p_id AND chu_id = p_chu_id AND huy_luc IS NULL;
    IF NOT FOUND THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    SELECT * INTO v FROM kho_van_ban WHERE id = v_van_ban FOR UPDATE;
    IF NOT kho_van_ban_xem_duoc(v, v_vai, p_nguoi) THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    SELECT * INTO g FROM kho_giao_viec WHERE id = p_id FOR UPDATE;
    IF g.huy_luc IS NOT NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF v_vai = 'can_bo' AND (g.nguoi_nhan <> p_nguoi OR p ?| ARRAY['han_xu_ly', 'y_kien_chi_dao', 'vai', 'nguoi_nhan']) THEN
        RETURN jsonb_build_object('loi', 'KHONG_DU_QUYEN');
    END IF;
    IF g.phien_ban <> p_phien_ban THEN
        RETURN jsonb_build_object('loi', 'XUNG_DOT_PHIEN_BAN', 'ban_moi', kho_giao_viec_json(g));
    END IF;
    IF p ? 'nguoi_nhan' THEN
        v_nhan := (p->>'nguoi_nhan')::UUID;
        IF to_chuc_vai_tro(p_chu_id, v_nhan) IS NULL THEN RETURN jsonb_build_object('loi', 'NGUOI_NHAN_KHONG_HOP_LE'); END IF;
        IF v_nhan <> g.nguoi_nhan AND EXISTS (SELECT 1 FROM kho_giao_viec
                                               WHERE van_ban_id = g.van_ban_id AND nguoi_nhan = v_nhan AND huy_luc IS NULL) THEN
            RETURN jsonb_build_object('loi', 'DA_GIAO');
        END IF;
    END IF;
    IF p->>'vai' = 'chu_tri' AND g.vai <> 'chu_tri' AND EXISTS (SELECT 1 FROM kho_giao_viec
            WHERE van_ban_id = g.van_ban_id AND vai = 'chu_tri' AND huy_luc IS NULL AND id <> g.id) THEN
        RETURN jsonb_build_object('loi', 'CHU_TRI_DA_CO');
    END IF;
    v_truoc := kho_giao_viec_json(g);
    UPDATE kho_giao_viec SET
        trang_thai = CASE WHEN p ? 'trang_thai' THEN p->>'trang_thai' ELSE trang_thai END,
        ghi_chu = CASE WHEN p ? 'ghi_chu' THEN p->>'ghi_chu' ELSE ghi_chu END,
        han_xu_ly = CASE WHEN p ? 'han_xu_ly' THEN (p->>'han_xu_ly')::DATE ELSE han_xu_ly END,
        y_kien_chi_dao = CASE WHEN p ? 'y_kien_chi_dao' THEN p->>'y_kien_chi_dao' ELSE y_kien_chi_dao END,
        vai = CASE WHEN p ? 'vai' THEN p->>'vai' ELSE vai END,
        nguoi_nhan = CASE WHEN p ? 'nguoi_nhan' THEN v_nhan ELSE nguoi_nhan END,
        cap_nhat_luc = now(), phien_ban = phien_ban + 1
     WHERE id = g.id
    RETURNING * INTO g;
    v_sau := kho_giao_viec_json(g);
    PERFORM kho_giao_viec_ghi(g.id, p_nguoi,
        CASE WHEN p ?| ARRAY['han_xu_ly', 'y_kien_chi_dao', 'vai', 'nguoi_nhan'] THEN 'sua'
             WHEN p ? 'trang_thai' THEN 'trang_thai' ELSE 'ghi_chu' END,
        (SELECT jsonb_object_agg(k, v_truoc->k) FROM jsonb_object_keys(p) k),
        (SELECT jsonb_object_agg(k, v_sau->k) FROM jsonb_object_keys(p) k));
    RETURN jsonb_build_object('ok', TRUE, 'giao_viec', v_sau);
END;
$$;

CREATE OR REPLACE FUNCTION kho_giao_viec_thu_hoi(p_chu_loai TEXT, p_chu_id UUID, p_nguoi UUID, p_id UUID) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE g kho_giao_viec%ROWTYPE; v kho_van_ban%ROWTYPE; v_vai TEXT; v_van_ban UUID;
BEGIN
    IF p_chu_loai <> 'to_chuc' THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    v_vai := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
    IF v_vai IS NULL THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    SELECT van_ban_id INTO v_van_ban FROM kho_giao_viec WHERE id = p_id AND chu_id = p_chu_id AND huy_luc IS NULL;
    IF NOT FOUND THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    SELECT * INTO v FROM kho_van_ban WHERE id = v_van_ban FOR UPDATE;
    IF NOT kho_van_ban_xem_duoc(v, v_vai, p_nguoi) THEN RETURN jsonb_build_object('loi', 'KHONG_TIM_THAY'); END IF;
    IF v_vai NOT IN ('quan_tri', 'van_thu') THEN RETURN jsonb_build_object('loi', 'KHONG_DU_QUYEN'); END IF;
    UPDATE kho_giao_viec SET huy_luc = now(), cap_nhat_luc = now(), phien_ban = phien_ban + 1
     WHERE id = p_id AND huy_luc IS NULL
    RETURNING * INTO g;
    IF FOUND THEN PERFORM kho_giao_viec_ghi(g.id, p_nguoi, 'thu_hoi', kho_giao_viec_json(g), NULL); END IF;
    RETURN jsonb_build_object('ok', TRUE);
END;
$$;

-- «Việc được giao» của người gọi. p_loc: { trang_thai?: text[], qua_han?: boolean, trang?, so_dong? (≤ 100) }.
-- Quá hạn: han_xu_ly < hôm nay (giờ Việt Nam) và chưa xong. Thứ tự: chưa xong trước, hạn gần trước, giao mới trước.
CREATE OR REPLACE FUNCTION kho_giao_viec_cua_toi(p_chu_loai TEXT, p_chu_id UUID, p_nguoi UUID, p_loc JSONB) RETURNS JSONB
LANGUAGE plpgsql STABLE SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE
    v_vai TEXT; v_so INT; v_trang INT; v_tong BIGINT; v_muc JSONB;
    v_tt TEXT[] := CASE WHEN jsonb_typeof(p_loc->'trang_thai') = 'array'
                        THEN ARRAY(SELECT jsonb_array_elements_text(p_loc->'trang_thai')) END;
    v_qua_han BOOLEAN := COALESCE((p_loc->>'qua_han')::BOOLEAN, FALSE);
    v_hom_nay DATE := (now() AT TIME ZONE 'Asia/Ho_Chi_Minh')::DATE;
BEGIN
    IF p_chu_loai <> 'to_chuc' THEN RETURN jsonb_build_object('muc', '[]'::jsonb, 'tong', 0); END IF;
    v_vai := kho_vai_tro(p_chu_loai, p_chu_id, p_nguoi);
    IF v_vai IS NULL THEN RETURN jsonb_build_object('muc', '[]'::jsonb, 'tong', 0); END IF;
    v_so := LEAST(GREATEST(COALESCE((p_loc->>'so_dong')::INT, 20), 1), 100);
    v_trang := LEAST(GREATEST(COALESCE((p_loc->>'trang')::INT, 1), 1), 10000);
    WITH loc AS (
        SELECT g.id, g.trang_thai = 'xong' AS xong, g.han_xu_ly, g.giao_luc
          FROM kho_giao_viec g JOIN kho_van_ban v ON v.id = g.van_ban_id
         WHERE g.chu_id = p_chu_id AND g.nguoi_nhan = p_nguoi AND g.huy_luc IS NULL AND v.xoa_luc IS NULL
           AND (v_tt IS NULL OR g.trang_thai = ANY (v_tt))
           AND (NOT v_qua_han OR (g.han_xu_ly < v_hom_nay AND g.trang_thai <> 'xong'))
    ), trang AS (
        SELECT * FROM loc ORDER BY xong, han_xu_ly ASC NULLS LAST, giao_luc DESC, id
         LIMIT v_so OFFSET (v_trang - 1) * v_so
    )
    SELECT (SELECT count(*) FROM loc),
           COALESCE((SELECT jsonb_agg(kho_giao_viec_json(g) || jsonb_build_object('van_ban', jsonb_build_object(
                                 'id', v.id, 'so', v.so, 'ky_hieu', v.ky_hieu, 'trich_yeu', v.trich_yeu, 'do_khan', v.do_khan))
                                ORDER BY tr.xong, tr.han_xu_ly ASC NULLS LAST, tr.giao_luc DESC, tr.id)
                       FROM trang tr JOIN kho_giao_viec g ON g.id = tr.id JOIN kho_van_ban v ON v.id = g.van_ban_id), '[]'::jsonb)
      INTO v_tong, v_muc;
    RETURN jsonb_build_object('muc', v_muc, 'tong', v_tong);
END;
$$;

DO $$
DECLARE f TEXT;
BEGIN
    FOREACH f IN ARRAY ARRAY[
        'kho_giao_viec_ds(text,uuid,uuid,uuid)', 'kho_giao_viec_tao(text,uuid,uuid,uuid,jsonb)',
        'kho_giao_viec_sua(text,uuid,uuid,uuid,integer,jsonb)', 'kho_giao_viec_thu_hoi(text,uuid,uuid,uuid)',
        'kho_giao_viec_cua_toi(text,uuid,uuid,jsonb)'
    ] LOOP
        EXECUTE format('REVOKE ALL ON FUNCTION %s FROM PUBLIC, anon, authenticated', f);
        EXECUTE format('GRANT EXECUTE ON FUNCTION %s TO service_role', f);
    END LOOP;
    FOREACH f IN ARRAY ARRAY['kho_giao_viec_json(kho_giao_viec)', 'kho_giao_viec_ghi(uuid,uuid,text,jsonb,jsonb)'] LOOP
        EXECUTE format('REVOKE ALL ON FUNCTION %s FROM PUBLIC, anon, authenticated, service_role', f);
    END LOOP;
END;
$$;
```

- [ ] **Step 4: Chạy kiểm**

Run: `npx tsx --test tests/giao-viec-csdl.test.ts`
Expected: mọi bài PASS (9 + 6 + 7).

- [ ] **Step 5: Commit**

```bash
git add supabase/migrations/018_giao_viec.sql tests/giao-viec-csdl.test.ts
git commit -m "feat(to-chuc): 018 phan C - giao viec chu tri/phoi hop, thu hoi, nhat ky chi ghi them, viec duoc giao

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 7: Migration 019 — ví credit tổ chức

**Files:**
- Create: `supabase/migrations/019_vi_credit.sql`
- Modify: `tests/ho-tro/csdl-thu.ts` (`TEP_LUOC_DO` thêm `"migrations/019_vi_credit.sql",`)
- Create: `tests/vi-credit-csdl.test.ts`

**Interfaces:**
- Consumes: 004 (`credit_ky_hien_tai`, `credit_tra_giu`, `tac_vu_mo`, `credit_so_du`, `credit_dieu_chinh`), Task 2
  (`to_chuc_vai_tro`, `to_chuc_con_goi`).
- Produces:
  - cột `credit_lo.vi_loai|vi_id`, `tac_vu_chay.vi_loai|vi_id` (dòng cũ / hàm cũ: ví cá nhân của `user_id`);
  - `tac_vu_mo_vi(p_user, p_vi_loai, p_vi_id, p_tac_vu, p_credit_min, p_credit_max, p_provider, p_model, p_nguon_goi, p_phien_ban)`
    → `{ ma: 'ok' | 'KHONG_DU_CREDIT' | 'QUA_NHIEU_LUOT' | 'GOI_HET_HAN' | 'KHONG_THUOC_TO_CHUC', luot_id?, so_du }`;
  - `credit_so_du_vi(p_user, p_vi_loai, p_vi_id) → SoDu | null`;
  - `credit_dieu_chinh_vi(p_vi_loai, p_vi_id, p_so, p_ghi_chu, p_boi) → { da_dieu_chinh, so_du }`;
  - `credit_chot`, `credit_hoan` (cùng chữ ký): ví lấy theo lượt; `so_du` trả là số dư của ví đó;
  - nội bộ: `credit_tong_vi(text, uuid)`, `credit_cap_ky_to_chuc(uuid)`, `credit_quet_treo_to_chuc(uuid)`,
    `credit_giu_to_chuc(uuid, uuid, int)`, `credit_vi_mac_dinh()` (trigger).

- [ ] **Step 1: Viết bài kiểm `tests/vi-credit-csdl.test.ts`**

```ts
import { after, before, describe, test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { Client } from "pg";
import { CO_CSDL_THU, rpc as goi, taoCsdlThu, taoNguoiDung } from "./ho-tro/csdl-thu";
import { rpcPg } from "./ho-tro/rpc-pg";
import { boToChuc, hetGoiToChuc, themThanhVien } from "./ho-tro/to-chuc";
import type { GoiRpc } from "../src/lib/phien/rpc";

describe("019 ví credit tổ chức", { skip: !CO_CSDL_THU }, () => {
  let csdl: Awaited<ReturnType<typeof taoCsdlThu>>;
  let c: Client;
  let rpc: GoiRpc;

  before(async () => {
    csdl = await taoCsdlThu();
    c = new Client({ connectionString: csdl.url });
    await c.connect();
    rpc = rpcPg(c);
  });
  after(async () => {
    await c?.end();
    await csdl?.xoa();
  });

  const moVi = (r: GoiRpc, user: string, viLoai: string, viId: string, max = 100) =>
    r("tac_vu_mo_vi", { p_user: user, p_vi_loai: viLoai, p_vi_id: viId, p_tac_vu: "tom-tat", p_credit_min: 1,
      p_credit_max: max, p_provider: "gemini", p_model: "gemini-3.5-flash", p_nguon_goi: "kiem-thu", p_phien_ban: null });
  const soDuTC = (user: string, tc: string) => rpc("credit_so_du_vi", { p_user: user, p_vi_loai: "to_chuc", p_vi_id: tc });
  /** Sổ cái của ví tổ chức: mỗi lô so_con = tổng sổ của lô. */
  async function kiemSoCaiTC(tc: string) {
    const { rows } = await c.query(
      `SELECT l.id, l.so_con, COALESCE(SUM(s.so), 0)::int AS tong FROM credit_lo l LEFT JOIN credit_so s ON s.lo_id = l.id
        WHERE l.vi_loai = 'to_chuc' AND l.vi_id = $1 GROUP BY l.id, l.so_con`, [tc]);
    for (const r of rows) assert.equal(r.so_con, r.tong, `lô ${r.id} lệch sổ cái`);
  }

  test("dòng cũ + hàm cũ: ví cá nhân của user_id; CHECK chặn lô tổ chức có user_id", async () => {
    const u = await taoNguoiDung(c);
    await goi(c, "credit_so_du", u);
    const { rows } = await c.query(`SELECT vi_loai, vi_id FROM credit_lo WHERE user_id = $1`, [u]);
    assert.deepEqual(rows, [{ vi_loai: "ca_nhan", vi_id: u }]);
    await assert.rejects(c.query(`INSERT INTO credit_lo (user_id, vi_loai, vi_id, nguon, so_cap, so_con, het_han)
      VALUES ($1, 'to_chuc', gen_random_uuid(), 'tang', 1, 1, now() + interval '1 day')`, [u]), /ck_credit_lo_vi/);
  });

  test("lô tháng ví tổ chức: cấp theo gói, gọi lại không cấp đôi, không thuộc ví cá nhân của ai", async () => {
    const b = await boToChuc(c);
    assert.equal((await soDuTC(b.canBo, b.toChuc)).tong, 3000);
    assert.equal((await soDuTC(b.quanTri, b.toChuc)).tong, 3000);
    const { rows } = await c.query(`SELECT count(*)::int AS n, bool_and(user_id IS NULL) AS khong_nguoi FROM credit_lo
                                     WHERE vi_loai = 'to_chuc' AND vi_id = $1`, [b.toChuc]);
    assert.deepEqual(rows[0], { n: 1, khong_nguoi: true });
    assert.equal((await goi(c, "credit_so_du", b.quanTri)).tong, 30); // ví cá nhân chỉ có lô free
    assert.equal(await soDuTC(await taoNguoiDung(c), b.toChuc), null);
    await kiemSoCaiTC(b.toChuc);
  });

  test("chạy AI bằng ví tổ chức: giữ / chốt / hoàn đúng ví; ví cá nhân người chạy không đổi", async () => {
    const b = await boToChuc(c);
    const caNhan = (await goi(c, "credit_so_du", b.canBo)).tong;
    const m = await moVi(rpc, b.canBo, "to_chuc", b.toChuc, 100);
    assert.equal(m.ma, "ok");
    assert.equal(m.so_du, 2900);
    const ch = await goi(c, "credit_chot", m.luot_id, 40, 10, 20, false);
    assert.deepEqual([ch.credit_tru, ch.so_du, ch.trang_thai], [40, 2960, "xong"]);
    const m2 = await moVi(rpc, b.canBo, "to_chuc", b.toChuc, 100);
    assert.equal((await goi(c, "credit_hoan", m2.luot_id, "AI_LOI")).so_du, 2960);
    assert.equal((await goi(c, "credit_so_du", b.canBo)).tong, caNhan);
    const { rows } = await c.query(`SELECT user_id, vi_loai, vi_id FROM tac_vu_chay WHERE id = $1`, [m.luot_id]);
    assert.deepEqual(rows[0], { user_id: b.canBo, vi_loai: "to_chuc", vi_id: b.toChuc }); // truy vết người chạy
    await kiemSoCaiTC(b.toChuc);
  });

  test("ví cá nhân qua tac_vu_mo_vi y như tac_vu_mo; ví cá nhân của người khác bị từ chối", async () => {
    const u = await taoNguoiDung(c);
    const m = await moVi(rpc, u, "ca_nhan", u, 10);
    assert.deepEqual([m.ma, m.so_du], ["ok", 20]);
    await assert.rejects(moVi(rpc, u, "ca_nhan", await taoNguoiDung(c), 10), /ví cá nhân phải của người chạy/);
  });

  test("gói hết hạn → GOI_HET_HAN; không thuộc / đã rời → KHONG_THUOC_TO_CHUC; không đủ credit", async () => {
    const b = await boToChuc(c);
    assert.equal((await moVi(rpc, b.canBo, "to_chuc", b.toChuc, 5000)).ma, "KHONG_DU_CREDIT");
    assert.equal((await moVi(rpc, await taoNguoiDung(c), "to_chuc", b.toChuc)).ma, "KHONG_THUOC_TO_CHUC");
    await themThanhVien(c, b.toChuc, b.canBo2, "can_bo", "da_roi");
    assert.equal((await moVi(rpc, b.canBo2, "to_chuc", b.toChuc)).ma, "KHONG_THUOC_TO_CHUC");
    await hetGoiToChuc(c, b.toChuc);
    assert.equal((await moVi(rpc, b.canBo, "to_chuc", b.toChuc)).ma, "GOI_HET_HAN");
  });

  test("giới hạn 3 lượt đang chạy tính theo người, gộp cả hai ví", async () => {
    const b = await boToChuc(c);
    await moVi(rpc, b.canBo, "ca_nhan", b.canBo, 5);
    await moVi(rpc, b.canBo, "to_chuc", b.toChuc, 5);
    await moVi(rpc, b.canBo, "to_chuc", b.toChuc, 5);
    assert.equal((await moVi(rpc, b.canBo, "to_chuc", b.toChuc, 5)).ma, "QUA_NHIEU_LUOT");
  });

  test("lượt treo của ví tổ chức được hoàn về ví tổ chức khi xem số dư", async () => {
    const b = await boToChuc(c);
    const m = await moVi(rpc, b.vanThu, "to_chuc", b.toChuc, 300);
    await c.query(`UPDATE tac_vu_chay SET bat_dau_luc = now() - interval '20 minutes' WHERE id = $1`, [m.luot_id]);
    assert.equal((await soDuTC(b.vanThu, b.toChuc)).tong, 3000);
    const { rows } = await c.query(`SELECT trang_thai, ma_loi FROM tac_vu_chay WHERE id = $1`, [m.luot_id]);
    assert.deepEqual(rows[0], { trang_thai: "loi", ma_loi: "TREO" });
    await kiemSoCaiTC(b.toChuc);
  });

  test("5 thành viên chạy song song trên ví 3.000: đúng 3 lượt 1.000, không tiêu lố", async () => {
    const b = await boToChuc(c);
    const nguoi = [b.quanTri, b.vanThu, b.canBo, b.canBo2];
    const them = await taoNguoiDung(c);
    await themThanhVien(c, b.toChuc, them, "can_bo");
    nguoi.push(them);
    const kn = await Promise.all(nguoi.map(async () => {
      const k = new Client({ connectionString: csdl.url });
      await k.connect();
      return k;
    }));
    try {
      const kq = await Promise.all(nguoi.map((u, i) => moVi(rpcPg(kn[i]), u, "to_chuc", b.toChuc, 1000)));
      assert.equal(kq.filter((k) => k.ma === "ok").length, 3);
      assert.equal(kq.filter((k) => k.ma === "KHONG_DU_CREDIT").length, 2);
      assert.equal((await soDuTC(b.quanTri, b.toChuc)).tong, 0);
      await kiemSoCaiTC(b.toChuc);
    } finally {
      await Promise.all(kn.map((k) => k.end()));
    }
  });

  test("hai ví cùng lúc của một người (có lượt treo ví tổ chức): không khóa chéo, lượt treo hoàn đúng một lần", async () => {
    const b = await boToChuc(c);
    const m = await moVi(rpc, b.canBo, "to_chuc", b.toChuc, 200);
    await c.query(`UPDATE tac_vu_chay SET bat_dau_luc = now() - interval '20 minutes' WHERE id = $1`, [m.luot_id]);
    const kn = await Promise.all([0, 1, 2, 3].map(async () => {
      const k = new Client({ connectionString: csdl.url });
      await k.connect();
      await k.query(`SET lock_timeout = '10s'`);
      return k;
    }));
    try {
      await Promise.all([
        goi(kn[0], "credit_so_du", b.canBo),                       // quét treo đường cá nhân: khóa users → to_chuc
        moVi(rpcPg(kn[1]), b.canBo, "to_chuc", b.toChuc, 50),     // khóa to_chuc
        moVi(rpcPg(kn[2]), b.canBo, "ca_nhan", b.canBo, 5),       // khóa users
        rpcPg(kn[3])("credit_so_du_vi", { p_user: b.canBo, p_vi_loai: "to_chuc", p_vi_id: b.toChuc }),
      ]);
      const { rows } = await c.query(`SELECT count(*)::int AS n FROM credit_so WHERE luot_id = $1 AND loai = 'hoan'`, [m.luot_id]);
      assert.equal(rows[0].n, 1);
      await kiemSoCaiTC(b.toChuc);
    } finally {
      await Promise.all(kn.map((k) => k.end()));
    }
  });

  test("điều chỉnh ví tổ chức: cộng lô 90 ngày, trừ không làm âm; ví cá nhân đi đường cũ", async () => {
    const b = await boToChuc(c);
    const a = await rpc("credit_dieu_chinh_vi", { p_vi_loai: "to_chuc", p_vi_id: b.toChuc, p_so: 500, p_ghi_chu: "Tặng dùng thử", p_boi: "admin@vbp" });
    assert.deepEqual([a.da_dieu_chinh, a.so_du], [500, 3500]);
    const t = await rpc("credit_dieu_chinh_vi", { p_vi_loai: "to_chuc", p_vi_id: b.toChuc, p_so: -9999, p_ghi_chu: "Thu hồi", p_boi: "admin@vbp" });
    assert.deepEqual([t.da_dieu_chinh, t.so_du], [-3500, 0]);
    const u = await taoNguoiDung(c);
    assert.equal((await rpc("credit_dieu_chinh_vi", { p_vi_loai: "ca_nhan", p_vi_id: u, p_so: 10, p_ghi_chu: "x", p_boi: "y" })).so_du, 40);
    await kiemSoCaiTC(b.toChuc);
  });

  test("quyền: hàm ví API chỉ service_role; hàm nội bộ thu hồi cả service_role", async () => {
    const co = async (v: string, f: string) =>
      (await c.query("SELECT has_function_privilege($1, $2, 'EXECUTE') AS co", [v, f])).rows[0].co as boolean;
    for (const f of ["tac_vu_mo_vi(uuid,text,uuid,text,integer,integer,text,text,text,text)", "credit_so_du_vi(uuid,text,uuid)",
      "credit_dieu_chinh_vi(text,uuid,integer,text,text)", "credit_chot(uuid,integer,integer,integer,boolean)", "credit_hoan(uuid,text)"]) {
      assert.equal(await co("service_role", f), true, f);
      assert.equal(await co("anon", f), false, f);
      assert.equal(await co("authenticated", f), false, f);
    }
    for (const f of ["credit_tong_vi(text,uuid)", "credit_cap_ky_to_chuc(uuid)", "credit_quet_treo_to_chuc(uuid)",
      "credit_giu_to_chuc(uuid,uuid,integer)", "credit_vi_mac_dinh()"]) {
      assert.equal(await co("service_role", f), false, f);
      assert.equal(await co("anon", f), false, f);
    }
  });

  test("chạy lại 019 lần hai không lỗi", async () => {
    await c.query(readFileSync(join(__dirname, "..", "supabase", "migrations", "019_vi_credit.sql"), "utf8"));
  });
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx tsx --test tests/vi-credit-csdl.test.ts`
Expected: FAIL — `column "vi_loai" does not exist`.

- [ ] **Step 3: Viết `supabase/migrations/019_vi_credit.sql` + thêm vào `TEP_LUOC_DO`**

```sql
-- ============================================================
-- 019: Ví credit theo chủ (spec đợt B §2.3, phương án A)
-- credit_lo / tac_vu_chay thêm vi_loai ('ca_nhan' | 'to_chuc') + vi_id. Dòng cũ và mọi hàm 004/005 cũ (không biết ví)
-- là ví cá nhân của user_id (trigger điền vi_id). Hàm cũ GIỮ NGUYÊN chữ ký + thân; bản *_vi mới gọi bản cũ khi ví cá nhân.
-- Ví tổ chức: lô và dòng sổ có user_id = NULL (xóa một tài khoản không xóa theo tiền của tổ chức); người chạy truy qua
-- credit_so.luot_id → tac_vu_chay.user_id. Khóa dòng to_chuc (FOR NO KEY UPDATE) thay cho users; đường ví tổ chức không
-- khóa users → không khóa chéo với đường cá nhân (cá nhân có thể khóa users → to_chuc khi quét lượt treo của ví tổ chức).
-- credit_chot / credit_hoan thay thân (cùng chữ ký): đọc ví của lượt; nhánh cá nhân y như 004.
-- ============================================================

SET lock_timeout = '5s';

ALTER TABLE credit_lo ADD COLUMN IF NOT EXISTS vi_loai TEXT NOT NULL DEFAULT 'ca_nhan';
ALTER TABLE credit_lo ADD COLUMN IF NOT EXISTS vi_id UUID;
ALTER TABLE tac_vu_chay ADD COLUMN IF NOT EXISTS vi_loai TEXT NOT NULL DEFAULT 'ca_nhan';
ALTER TABLE tac_vu_chay ADD COLUMN IF NOT EXISTS vi_id UUID;

-- Hàm cũ chèn không có vi_id: ví cá nhân của user_id.
CREATE OR REPLACE FUNCTION credit_vi_mac_dinh() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
    IF NEW.vi_loai = 'ca_nhan' AND NEW.vi_id IS NULL THEN NEW.vi_id := NEW.user_id; END IF;
    RETURN NEW;
END;
$$;
DROP TRIGGER IF EXISTS trg_credit_lo_vi ON credit_lo;
CREATE TRIGGER trg_credit_lo_vi BEFORE INSERT ON credit_lo FOR EACH ROW EXECUTE FUNCTION credit_vi_mac_dinh();
DROP TRIGGER IF EXISTS trg_tac_vu_chay_vi ON tac_vu_chay;
CREATE TRIGGER trg_tac_vu_chay_vi BEFORE INSERT ON tac_vu_chay FOR EACH ROW EXECUTE FUNCTION credit_vi_mac_dinh();

UPDATE credit_lo SET vi_id = user_id WHERE vi_id IS NULL;
UPDATE tac_vu_chay SET vi_id = user_id WHERE vi_id IS NULL;
ALTER TABLE credit_lo ALTER COLUMN vi_id SET NOT NULL;
ALTER TABLE tac_vu_chay ALTER COLUMN vi_id SET NOT NULL;
ALTER TABLE credit_lo ALTER COLUMN user_id DROP NOT NULL;
ALTER TABLE credit_so ALTER COLUMN user_id DROP NOT NULL;

ALTER TABLE credit_lo DROP CONSTRAINT IF EXISTS ck_credit_lo_vi;
ALTER TABLE credit_lo ADD CONSTRAINT ck_credit_lo_vi CHECK (
       (vi_loai = 'ca_nhan' AND user_id IS NOT NULL AND vi_id = user_id)
    OR (vi_loai = 'to_chuc' AND user_id IS NULL));
ALTER TABLE tac_vu_chay DROP CONSTRAINT IF EXISTS ck_tac_vu_chay_vi;
ALTER TABLE tac_vu_chay ADD CONSTRAINT ck_tac_vu_chay_vi CHECK (
       (vi_loai = 'ca_nhan' AND vi_id = user_id) OR vi_loai = 'to_chuc');

CREATE INDEX IF NOT EXISTS idx_credit_lo_vi ON credit_lo(vi_loai, vi_id, het_han);
CREATE UNIQUE INDEX IF NOT EXISTS uq_credit_lo_ky_to_chuc ON credit_lo(vi_id, ky_bat_dau)
    WHERE nguon = 'goi_thang' AND vi_loai = 'to_chuc';
CREATE INDEX IF NOT EXISTS idx_tac_vu_chay_vi ON tac_vu_chay(vi_loai, vi_id, trang_thai);

-- ---------- hàm nội bộ ví tổ chức ----------
CREATE OR REPLACE FUNCTION credit_tong_vi(p_vi_loai TEXT, p_vi_id UUID) RETURNS INT
LANGUAGE sql STABLE SET search_path = public, pg_temp AS $$
    SELECT COALESCE(SUM(so_con), 0)::INT FROM credit_lo
     WHERE vi_loai = p_vi_loai AND vi_id = p_vi_id AND het_han > NOW() AND so_con > 0;
$$;

-- Lô tháng của ví tổ chức theo gói còn hiệu lực: kỳ tính từ goi_bat_dau, lô không quá goi_het_han. Không khóa to_chuc:
-- chống cấp đôi bằng uq_credit_lo_ky_to_chuc + ON CONFLICT (như credit_cap_ky của 004).
CREATE OR REPLACE FUNCTION credit_cap_ky_to_chuc(p_to_chuc UUID) RETURNS VOID
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE t to_chuc%ROWTYPE; v_so INT; v_ky TIMESTAMPTZ; v_het TIMESTAMPTZ; v_lo UUID; v_moi BOOLEAN;
BEGIN
    SELECT * INTO t FROM to_chuc WHERE id = p_to_chuc;
    IF NOT FOUND OR NOT to_chuc_con_goi(t) THEN RETURN; END IF;
    SELECT credit_thang INTO v_so FROM goi_to_chuc WHERE ma = t.goi_ma;
    IF COALESCE(v_so, 0) <= 0 THEN RETURN; END IF;
    SELECT k.ky_bat_dau, k.het_han INTO v_ky, v_het FROM credit_ky_hien_tai(t.goi_bat_dau, NOW()) k;
    v_het := LEAST(v_het, t.goi_het_han);
    INSERT INTO credit_lo (user_id, vi_loai, vi_id, nguon, so_cap, so_con, het_han, ky_bat_dau)
    VALUES (NULL, 'to_chuc', p_to_chuc, 'goi_thang', v_so, v_so, v_het, v_ky)
    ON CONFLICT (vi_id, ky_bat_dau) WHERE nguon = 'goi_thang' AND vi_loai = 'to_chuc'
    DO UPDATE SET het_han = EXCLUDED.het_han WHERE credit_lo.het_han < EXCLUDED.het_han
    RETURNING id, (xmax = 0) INTO v_lo, v_moi;
    IF v_lo IS NOT NULL AND v_moi THEN
        INSERT INTO credit_so (user_id, lo_id, loai, so, ghi_chu)
        VALUES (NULL, v_lo, 'cap', v_so,
                'Gói tổ chức ' || t.goi_ma || ' kỳ ' || to_char(v_ky AT TIME ZONE 'Asia/Ho_Chi_Minh', 'DD/MM/YYYY'));
    END IF;
END;
$$;

-- Giữ credit của ví tổ chức, lô hết hạn sớm nhất trước. Người gọi đã khóa dòng to_chuc.
CREATE OR REPLACE FUNCTION credit_giu_to_chuc(p_to_chuc UUID, p_luot UUID, p_so INT) RETURNS BOOLEAN
LANGUAGE plpgsql SET search_path = public, pg_temp AS $$
DECLARE r RECORD; v_con INT := p_so; v_lay INT;
BEGIN
    IF p_so <= 0 THEN RETURN TRUE; END IF;
    IF credit_tong_vi('to_chuc', p_to_chuc) < p_so THEN RETURN FALSE; END IF;
    FOR r IN SELECT id, so_con FROM credit_lo
              WHERE vi_loai = 'to_chuc' AND vi_id = p_to_chuc AND het_han > NOW() AND so_con > 0
              ORDER BY het_han, id FOR UPDATE LOOP
        EXIT WHEN v_con = 0;
        v_lay := LEAST(r.so_con, v_con);
        UPDATE credit_lo SET so_con = so_con - v_lay WHERE id = r.id;
        INSERT INTO credit_so (user_id, lo_id, luot_id, loai, so) VALUES (NULL, r.id, p_luot, 'giu', -v_lay);
        v_con := v_con - v_lay;
    END LOOP;
    IF v_con > 0 THEN RAISE EXCEPTION 'credit_giu_to_chuc: thiếu % credit', v_con; END IF;
    RETURN TRUE;
END;
$$;

-- ---------- credit_chot / credit_hoan: ví theo lượt (nhánh cá nhân GIỮ NGUYÊN 004) ----------
CREATE OR REPLACE FUNCTION credit_hoan(p_luot UUID, p_ma_loi TEXT) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE v_user UUID; v_vi_loai TEXT; v_vi_id UUID; t tac_vu_chay%ROWTYPE;
BEGIN
    SELECT user_id, vi_loai, vi_id INTO v_user, v_vi_loai, v_vi_id FROM tac_vu_chay WHERE id = p_luot;
    IF NOT FOUND THEN RAISE EXCEPTION 'credit_hoan: không có lượt %', p_luot; END IF;
    IF v_vi_loai = 'to_chuc' THEN
        PERFORM 1 FROM to_chuc WHERE id = v_vi_id FOR NO KEY UPDATE;
        SELECT * INTO t FROM tac_vu_chay WHERE id = p_luot FOR UPDATE;
        IF t.trang_thai = 'dang_chay' THEN
            PERFORM credit_tra_giu(NULL, p_luot, 0);
            UPDATE tac_vu_chay SET trang_thai = 'loi', ma_loi = p_ma_loi, xong_luc = NOW() WHERE id = p_luot;
        END IF;
        RETURN jsonb_build_object('so_du', credit_tong_vi('to_chuc', v_vi_id));
    END IF;
    PERFORM 1 FROM users WHERE id = v_user FOR NO KEY UPDATE;
    SELECT * INTO t FROM tac_vu_chay WHERE id = p_luot FOR UPDATE;
    IF t.trang_thai = 'dang_chay' THEN
        PERFORM credit_tra_giu(v_user, p_luot, 0);
        UPDATE tac_vu_chay SET trang_thai = 'loi', ma_loi = p_ma_loi, xong_luc = NOW() WHERE id = p_luot;
    END IF;
    RETURN jsonb_build_object('so_du', credit_tong(v_user));
END;
$$;

CREATE OR REPLACE FUNCTION credit_chot(p_luot UUID, p_credit_that INT, p_token_vao INT, p_token_ra INT, p_bi_cat BOOLEAN)
RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE v_user UUID; v_vi_loai TEXT; v_vi_id UUID; t tac_vu_chay%ROWTYPE; v_tru INT;
BEGIN
    IF p_credit_that IS NULL OR p_credit_that < 0 THEN RAISE EXCEPTION 'DAU_VAO_SAI'; END IF;
    SELECT user_id, vi_loai, vi_id INTO v_user, v_vi_loai, v_vi_id FROM tac_vu_chay WHERE id = p_luot;
    IF NOT FOUND THEN RAISE EXCEPTION 'credit_chot: không có lượt %', p_luot; END IF;
    IF v_vi_loai = 'to_chuc' THEN
        PERFORM 1 FROM to_chuc WHERE id = v_vi_id FOR NO KEY UPDATE;
        SELECT * INTO t FROM tac_vu_chay WHERE id = p_luot FOR UPDATE;
        IF t.trang_thai <> 'dang_chay' THEN
            RETURN jsonb_build_object('credit_tru', t.credit_tru, 'so_du', credit_tong_vi('to_chuc', v_vi_id),
                                      'trang_thai', t.trang_thai);
        END IF;
        v_tru := credit_tra_giu(NULL, p_luot, LEAST(p_credit_that, t.credit_giu));
        UPDATE tac_vu_chay
           SET trang_thai = 'xong', credit_tru = v_tru, token_vao = p_token_vao, token_ra = p_token_ra,
               bi_cat = COALESCE(p_bi_cat, FALSE), xong_luc = NOW()
         WHERE id = p_luot;
        RETURN jsonb_build_object('credit_tru', v_tru, 'so_du', credit_tong_vi('to_chuc', v_vi_id), 'trang_thai', 'xong');
    END IF;
    PERFORM 1 FROM users WHERE id = v_user FOR NO KEY UPDATE;
    SELECT * INTO t FROM tac_vu_chay WHERE id = p_luot FOR UPDATE;
    IF t.trang_thai <> 'dang_chay' THEN
        RETURN jsonb_build_object('credit_tru', t.credit_tru, 'so_du', credit_tong(v_user), 'trang_thai', t.trang_thai);
    END IF;
    v_tru := credit_tra_giu(v_user, p_luot, LEAST(p_credit_that, t.credit_giu));
    UPDATE tac_vu_chay
       SET trang_thai = 'xong', credit_tru = v_tru, token_vao = p_token_vao, token_ra = p_token_ra,
           bi_cat = COALESCE(p_bi_cat, FALSE), xong_luc = NOW()
     WHERE id = p_luot;
    RETURN jsonb_build_object('credit_tru', v_tru, 'so_du', credit_tong(v_user), 'trang_thai', 'xong');
END;
$$;

-- Lượt «dang_chay» quá 15 phút của ví tổ chức: hoàn (credit_hoan tự khóa to_chuc).
CREATE OR REPLACE FUNCTION credit_quet_treo_to_chuc(p_to_chuc UUID) RETURNS INT
LANGUAGE plpgsql SET search_path = public, pg_temp AS $$
DECLARE r RECORD; n INT := 0;
BEGIN
    FOR r IN SELECT id FROM tac_vu_chay
              WHERE vi_loai = 'to_chuc' AND vi_id = p_to_chuc AND trang_thai = 'dang_chay'
                AND bat_dau_luc < NOW() - INTERVAL '15 minutes' LOOP
        PERFORM credit_hoan(r.id, 'TREO');
        n := n + 1;
    END LOOP;
    RETURN n;
END;
$$;

-- ---------- hàm API theo ví ----------
-- Ví cá nhân: đúng tac_vu_mo (004). Ví tổ chức: khóa to_chuc, kiểm thành viên + gói, quét treo, cấp kỳ, giới hạn 3 lượt
-- đang chạy THEO NGƯỜI (gộp hai ví), kiểm số dư, ghi lượt (user_id = người chạy), giữ credit_max.
CREATE OR REPLACE FUNCTION tac_vu_mo_vi(
    p_user UUID, p_vi_loai TEXT, p_vi_id UUID, p_tac_vu TEXT, p_credit_min INT, p_credit_max INT,
    p_provider TEXT, p_model TEXT, p_nguon_goi TEXT, p_phien_ban TEXT
) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE t to_chuc%ROWTYPE; v_luot UUID; v_dang INT;
BEGIN
    IF p_vi_loai = 'ca_nhan' THEN
        IF p_vi_id IS DISTINCT FROM p_user THEN RAISE EXCEPTION 'tac_vu_mo_vi: ví cá nhân phải của người chạy'; END IF;
        RETURN tac_vu_mo(p_user, p_tac_vu, p_credit_min, p_credit_max, p_provider, p_model, p_nguon_goi, p_phien_ban);
    END IF;
    IF p_vi_loai IS DISTINCT FROM 'to_chuc' THEN RAISE EXCEPTION 'tac_vu_mo_vi: loại ví sai'; END IF;
    IF p_credit_max IS NULL OR p_credit_min IS NULL OR p_credit_max <= 0 OR p_credit_min < 0 OR p_credit_min > p_credit_max THEN
        RAISE EXCEPTION 'DAU_VAO_SAI';
    END IF;
    SELECT * INTO t FROM to_chuc WHERE id = p_vi_id FOR NO KEY UPDATE;
    IF NOT FOUND OR to_chuc_vai_tro(p_vi_id, p_user) IS NULL THEN
        RETURN jsonb_build_object('ma', 'KHONG_THUOC_TO_CHUC', 'so_du', 0);
    END IF;
    IF NOT to_chuc_con_goi(t) THEN
        RETURN jsonb_build_object('ma', 'GOI_HET_HAN', 'so_du', credit_tong_vi('to_chuc', p_vi_id));
    END IF;
    PERFORM credit_quet_treo_to_chuc(p_vi_id);
    PERFORM credit_cap_ky_to_chuc(p_vi_id);
    SELECT COUNT(*) INTO v_dang FROM tac_vu_chay WHERE user_id = p_user AND trang_thai = 'dang_chay';
    IF v_dang >= 3 THEN
        RETURN jsonb_build_object('ma', 'QUA_NHIEU_LUOT', 'so_du', credit_tong_vi('to_chuc', p_vi_id));
    END IF;
    IF credit_tong_vi('to_chuc', p_vi_id) < p_credit_max THEN
        RETURN jsonb_build_object('ma', 'KHONG_DU_CREDIT', 'so_du', credit_tong_vi('to_chuc', p_vi_id));
    END IF;
    INSERT INTO tac_vu_chay (user_id, vi_loai, vi_id, tac_vu, credit_min, credit_max, credit_giu, provider, model,
                             nguon_goi, phien_ban_ung_dung)
    VALUES (p_user, 'to_chuc', p_vi_id, p_tac_vu, p_credit_min, p_credit_max, p_credit_max, p_provider, p_model,
            p_nguon_goi, p_phien_ban)
    RETURNING id INTO v_luot;
    IF NOT credit_giu_to_chuc(p_vi_id, v_luot, p_credit_max) THEN RAISE EXCEPTION 'KHONG_DU_CREDIT'; END IF;
    RETURN jsonb_build_object('ma', 'ok', 'luot_id', v_luot, 'so_du', credit_tong_vi('to_chuc', p_vi_id));
END;
$$;

-- Số dư theo ví. Ví cá nhân: chỉ của chính p_user (đúng credit_so_du). Ví tổ chức: thành viên hoạt động; không thì NULL.
CREATE OR REPLACE FUNCTION credit_so_du_vi(p_user UUID, p_vi_loai TEXT, p_vi_id UUID) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
BEGIN
    IF p_vi_loai = 'ca_nhan' THEN
        IF p_vi_id IS DISTINCT FROM p_user THEN RETURN NULL; END IF;
        RETURN credit_so_du(p_user);
    END IF;
    IF p_vi_loai IS DISTINCT FROM 'to_chuc' OR to_chuc_vai_tro(p_vi_id, p_user) IS NULL THEN RETURN NULL; END IF;
    PERFORM credit_quet_treo_to_chuc(p_vi_id);
    PERFORM credit_cap_ky_to_chuc(p_vi_id);
    RETURN jsonb_build_object(
        'tong', credit_tong_vi('to_chuc', p_vi_id),
        'lo', COALESCE((
            SELECT jsonb_agg(jsonb_build_object('nguon', nguon, 'so_cap', so_cap, 'so_con', so_con, 'het_han', het_han)
                             ORDER BY het_han, id)
              FROM credit_lo WHERE vi_loai = 'to_chuc' AND vi_id = p_vi_id AND het_han > NOW() AND so_con > 0), '[]'::jsonb),
        'het_han_gan_nhat', (SELECT MIN(het_han) FROM credit_lo
                              WHERE vi_loai = 'to_chuc' AND vi_id = p_vi_id AND het_han > NOW() AND so_con > 0));
END;
$$;

-- Quản trị hệ thống cộng / trừ tay. Ví cá nhân: đúng credit_dieu_chinh (004).
CREATE OR REPLACE FUNCTION credit_dieu_chinh_vi(p_vi_loai TEXT, p_vi_id UUID, p_so INT, p_ghi_chu TEXT, p_boi TEXT) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE v_lo UUID; v_da INT := 0; v_con INT; v_lay INT; r RECORD;
BEGIN
    IF p_vi_loai = 'ca_nhan' THEN RETURN credit_dieu_chinh(p_vi_id, p_so, p_ghi_chu, p_boi); END IF;
    IF p_vi_loai IS DISTINCT FROM 'to_chuc' OR p_so IS NULL OR p_so = 0 THEN RAISE EXCEPTION 'DAU_VAO_SAI'; END IF;
    PERFORM 1 FROM to_chuc WHERE id = p_vi_id FOR NO KEY UPDATE;
    IF NOT FOUND THEN RAISE EXCEPTION 'credit_dieu_chinh_vi: không có tổ chức %', p_vi_id; END IF;
    PERFORM credit_cap_ky_to_chuc(p_vi_id);
    IF p_so > 0 THEN
        INSERT INTO credit_lo (user_id, vi_loai, vi_id, nguon, so_cap, so_con, het_han)
        VALUES (NULL, 'to_chuc', p_vi_id, 'dieu_chinh', p_so, p_so, NOW() + INTERVAL '90 days')
        RETURNING id INTO v_lo;
        INSERT INTO credit_so (user_id, lo_id, loai, so, ghi_chu, boi) VALUES (NULL, v_lo, 'dieu_chinh', p_so, p_ghi_chu, p_boi);
        v_da := p_so;
    ELSE
        v_con := -p_so;
        FOR r IN SELECT id, so_con FROM credit_lo
                  WHERE vi_loai = 'to_chuc' AND vi_id = p_vi_id AND het_han > NOW() AND so_con > 0
                  ORDER BY het_han, id FOR UPDATE LOOP
            EXIT WHEN v_con = 0;
            v_lay := LEAST(r.so_con, v_con);
            UPDATE credit_lo SET so_con = so_con - v_lay WHERE id = r.id;
            INSERT INTO credit_so (user_id, lo_id, loai, so, ghi_chu, boi)
            VALUES (NULL, r.id, 'dieu_chinh', -v_lay, p_ghi_chu, p_boi);
            v_con := v_con - v_lay;
            v_da := v_da - v_lay;
        END LOOP;
    END IF;
    RETURN jsonb_build_object('da_dieu_chinh', v_da, 'so_du', credit_tong_vi('to_chuc', p_vi_id));
END;
$$;

DO $$
DECLARE f TEXT;
BEGIN
    FOREACH f IN ARRAY ARRAY[
        'tac_vu_mo_vi(uuid,text,uuid,text,integer,integer,text,text,text,text)', 'credit_so_du_vi(uuid,text,uuid)',
        'credit_dieu_chinh_vi(text,uuid,integer,text,text)', 'credit_chot(uuid,integer,integer,integer,boolean)',
        'credit_hoan(uuid,text)'
    ] LOOP
        EXECUTE format('REVOKE ALL ON FUNCTION %s FROM PUBLIC, anon, authenticated', f);
        EXECUTE format('GRANT EXECUTE ON FUNCTION %s TO service_role', f);
    END LOOP;
    FOREACH f IN ARRAY ARRAY[
        'credit_vi_mac_dinh()', 'credit_tong_vi(text,uuid)', 'credit_cap_ky_to_chuc(uuid)',
        'credit_giu_to_chuc(uuid,uuid,integer)', 'credit_quet_treo_to_chuc(uuid)'
    ] LOOP
        EXECUTE format('REVOKE ALL ON FUNCTION %s FROM PUBLIC, anon, authenticated, service_role', f);
    END LOOP;
END;
$$;
```

- [ ] **Step 4: Chạy kiểm — mới + credit cũ KHÔNG sửa**

Run: `npx tsx --test tests/vi-credit-csdl.test.ts tests/credit-csdl.test.ts tests/goi-nap-csdl.test.ts tests/luu-ho-tro-csdl.test.ts`
Expected: tất cả PASS. Nếu một bài trong `credit-csdl.test.ts` / `goi-nap-csdl.test.ts` hỏng: **không sửa bài kiểm** — sửa
migration cho tới khi đường cá nhân y như cũ.

- [ ] **Step 5: Commit**

```bash
git add supabase/migrations/019_vi_credit.sql tests/ho-tro/csdl-thu.ts tests/vi-credit-csdl.test.ts
git commit -m "feat(to-chuc): 019 vi credit to chuc - tac_vu_mo_vi, so du vi, chot/hoan theo vi cua luot

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 8: Migration 020 — đơn hàng tổ chức và `pay_order`

**Files:**
- Create: `supabase/migrations/020_don_to_chuc.sql`
- Modify: `tests/ho-tro/csdl-thu.ts` (`TEP_LUOC_DO` thêm `"migrations/020_don_to_chuc.sql",`)
- Create: `tests/don-to-chuc-csdl.test.ts`

**Interfaces:**
- Consumes: Task 2 (`to_chuc_vai_tro`), Task 7 (`credit_tong_vi`, cột ví); 005/011 (`pay_order`, `credit_cap_nap`,
  `kho_cap_dung_luong`).
- Produces: `orders.to_chuc_id`, `orders.goi_to_chuc_ma`, `orders.loai = 'goi_to_chuc'`; trigger `trg_orders_kiem_to_chuc`;
  `to_chuc_cap_goi(p_order uuid) → { bat_dau, het_han } | null` (nội bộ, gọi từ `pay_order`); `credit_cap_nap`,
  `kho_cap_dung_luong`, `pay_order` (cùng chữ ký) có nhánh tổ chức.

- [ ] **Step 1: Viết bài kiểm `tests/don-to-chuc-csdl.test.ts`**

```ts
import { after, before, describe, test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { Client } from "pg";
import { randomInt } from "node:crypto";
import { CO_CSDL_THU, rpc as goi, taoCsdlThu, taoNguoiDung } from "./ho-tro/csdl-thu";
import { rpcPg } from "./ho-tro/rpc-pg";
import { boToChuc, chuTC, taoToChucThu } from "./ho-tro/to-chuc";
import type { GoiRpc } from "../src/lib/phien/rpc";

/** Mã đơn đúng khuôn CHECK của orders.code (^VBP[A-HJ-NP-Z2-9]{8}$). */
const CHU = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
const maMoi = () => "VBP" + Array.from({ length: 8 }, () => CHU[randomInt(CHU.length)]).join("");

describe("020 đơn tổ chức", { skip: !CO_CSDL_THU }, () => {
  let csdl: Awaited<ReturnType<typeof taoCsdlThu>>;
  let c: Client;
  let rpc: GoiRpc;

  before(async () => {
    csdl = await taoCsdlThu();
    c = new Client({ connectionString: csdl.url });
    await c.connect();
    rpc = rpcPg(c);
    await c.query(`UPDATE goi_to_chuc SET gia_nam_vnd = 6000000, dang_ban = TRUE WHERE ma = 'tc-30'`);
  });
  after(async () => {
    await c?.end();
    await csdl?.xoa();
  });

  const donToChuc = async (user: string, tc: string, loai: "goi_to_chuc" | "nap" | "dung_luong") => {
    const code = maMoi();
    const cot = loai === "goi_to_chuc" ? `goi_to_chuc_ma` : loai === "nap" ? `goi_nap_ma` : `goi_dung_luong_ma`;
    const gia = loai === "goi_to_chuc" ? "tc-30" : loai === "nap" ? "nap-99" : "dl-10";
    const them = loai === "dung_luong" ? ", so_thang" : "";
    const giaTriThem = loai === "dung_luong" ? ", 3" : "";
    await c.query(`INSERT INTO orders (code, user_id, loai, ${cot}, to_chuc_id, amount_vnd${them})
                   VALUES ($1, $2, $3, $4, $5, 99000${giaTriThem})`, [code, user, loai, gia, tc]);
    return code;
  };

  test("ràng buộc: goi_to_chuc cần mã gói + to_chuc_id; đơn gói cá nhân không gắn tổ chức", async () => {
    const b = await boToChuc(c);
    await assert.rejects(c.query(`INSERT INTO orders (code, user_id, loai, goi_to_chuc_ma, amount_vnd) VALUES ($1, $2, 'goi_to_chuc', 'tc-30', 1)`,
      [maMoi(), b.quanTri]), /ck_orders_loai/);
    await assert.rejects(c.query(`INSERT INTO orders (code, user_id, loai, plan_id, cycle, to_chuc_id, amount_vnd) VALUES ($1, $2, 'goi', 'pro', 'month', $3, 1)`,
      [maMoi(), b.quanTri, b.toChuc]), /ck_orders_loai/);
  });

  test("trigger: chỉ quản trị của tổ chức tạo được đơn cho tổ chức", async () => {
    const b = await boToChuc(c);
    await assert.rejects(donToChuc(b.vanThu, b.toChuc, "nap"), /chỉ quản trị/);
    await assert.rejects(donToChuc(await taoNguoiDung(c), b.toChuc, "goi_to_chuc"), /chỉ quản trị/);
    await donToChuc(b.quanTri, b.toChuc, "nap");
  });

  test("pay_order gói tổ chức: kích hoạt đúng tổ chức 1 năm; gọi lại already_paid, không cộng đôi; mua tiếp cùng gói nối hạn", async () => {
    const qt = await taoNguoiDung(c);
    const tc = await taoToChucThu(c, qt);
    const code = await donToChuc(qt, tc, "goi_to_chuc");
    assert.equal(await goi(c, "pay_order", code, "sepay", "ref1"), "paid");
    const t1 = (await c.query(`SELECT goi_ma, goi_bat_dau, goi_het_han FROM to_chuc WHERE id = $1`, [tc])).rows[0];
    assert.equal(t1.goi_ma, "tc-30");
    const nam = (t1.goi_het_han.getTime() - t1.goi_bat_dau.getTime()) / 86_400_000;
    assert.ok(nam >= 365 && nam <= 366, String(nam));
    const o = (await c.query(`SELECT period_start, period_end FROM orders WHERE code = $1`, [code])).rows[0];
    assert.equal(o.period_end.getTime(), t1.goi_het_han.getTime());
    assert.equal(await goi(c, "pay_order", code, "admin", "x"), "already_paid");
    assert.equal((await c.query(`SELECT goi_het_han FROM to_chuc WHERE id = $1`, [tc])).rows[0].goi_het_han.getTime(), t1.goi_het_han.getTime());
    const code2 = await donToChuc(qt, tc, "goi_to_chuc");
    await goi(c, "pay_order", code2, "sepay", "ref2");
    const t2 = (await c.query(`SELECT goi_bat_dau, goi_het_han FROM to_chuc WHERE id = $1`, [tc])).rows[0];
    assert.equal(t2.goi_bat_dau.getTime(), t1.goi_bat_dau.getTime());
    assert.ok(t2.goi_het_han.getTime() > t1.goi_het_han.getTime() + 364 * 86_400_000);
    // ví tổ chức có lô tháng của gói tc-30
    assert.equal((await rpc("credit_so_du_vi", { p_user: qt, p_vi_loai: "to_chuc", p_vi_id: tc })).tong, 8000);
  });

  test("đơn nạp cho tổ chức: lô vào ví tổ chức, ví cá nhân người mua không đổi; gọi lại không cấp đôi", async () => {
    const b = await boToChuc(c);
    const truoc = (await goi(c, "credit_so_du", b.quanTri)).tong;
    const code = await donToChuc(b.quanTri, b.toChuc, "nap");
    await goi(c, "pay_order", code, "sepay", "r");
    await goi(c, "pay_order", code, "sepay", "r");
    const id = (await c.query(`SELECT id FROM orders WHERE code = $1`, [code])).rows[0].id;
    await goi(c, "credit_cap_nap", id);
    const { rows } = await c.query(`SELECT vi_loai, vi_id, user_id, so_cap FROM credit_lo WHERE order_id = $1`, [id]);
    assert.deepEqual(rows, [{ vi_loai: "to_chuc", vi_id: b.toChuc, user_id: null, so_cap: 350 }]);
    assert.equal((await goi(c, "credit_so_du", b.quanTri)).tong, truoc);
    assert.equal((await rpc("credit_so_du_vi", { p_user: b.canBo, p_vi_loai: "to_chuc", p_vi_id: b.toChuc })).tong, 3350);
  });

  test("đơn dung lượng cho tổ chức: cộng vào kho tổ chức, không vào kho cá nhân người mua", async () => {
    const b = await boToChuc(c);
    const code = await donToChuc(b.quanTri, b.toChuc, "dung_luong");
    await goi(c, "pay_order", code, "sepay", "r");
    assert.equal((await rpc("kho_dung_luong_xem", chuTC(b.toChuc))).them_mb, 10240);
    assert.equal((await rpc("kho_dung_luong_xem", { p_chu_loai: "ca_nhan", p_chu_id: b.quanTri })).them_mb, 0);
  });

  test("đơn cá nhân cũ (gói, nạp, dung lượng) giữ hành vi", async () => {
    const u = await taoNguoiDung(c);
    const code = maMoi();
    await c.query(`INSERT INTO orders (code, user_id, loai, goi_nap_ma, amount_vnd) VALUES ($1, $2, 'nap', 'nap-49', 49000)`, [code, u]);
    assert.equal(await goi(c, "pay_order", code, "sepay", "r"), "paid");
    const { rows } = await c.query(`SELECT vi_loai, vi_id, user_id FROM credit_lo WHERE order_id = (SELECT id FROM orders WHERE code = $1)`, [code]);
    assert.deepEqual(rows, [{ vi_loai: "ca_nhan", vi_id: u, user_id: u }]);
  });

  test("quyền + chạy lại 020", async () => {
    const co = async (v: string, f: string) =>
      (await c.query("SELECT has_function_privilege($1, $2, 'EXECUTE') AS co", [v, f])).rows[0].co as boolean;
    assert.equal(await co("service_role", "to_chuc_cap_goi(uuid)"), false);
    assert.equal(await co("anon", "pay_order(text,text,text)"), false);
    assert.equal(await co("service_role", "pay_order(text,text,text)"), true);
    await c.query(readFileSync(join(__dirname, "..", "supabase", "migrations", "020_don_to_chuc.sql"), "utf8"));
  });
});
```

(Không dùng `generateOrderCode` của `src/lib/order-rules.ts` để bài kiểm CSDL không phụ thuộc mã TS.)

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx tsx --test tests/don-to-chuc-csdl.test.ts`
Expected: FAIL — `column "to_chuc_id" of relation "orders" does not exist`.

- [ ] **Step 3: Viết `supabase/migrations/020_don_to_chuc.sql` + thêm vào `TEP_LUOC_DO`**

```sql
-- ============================================================
-- 020: Đơn hàng của tổ chức (spec đợt B §2.4)
-- orders.to_chuc_id: đơn nạp / dung lượng / gói tổ chức cấp cho ví / kho / gói của tổ chức. Chỉ quản trị tạo đơn cho tổ chức
-- (API kiểm + trigger trong CSDL). pay_order (webhook SePay + xác nhận tay) giữ thứ tự khóa orders → users (người mua)
-- → to_chuc; gọi lại trả already_paid.
-- ============================================================

SET lock_timeout = '5s';

ALTER TABLE orders ADD COLUMN IF NOT EXISTS to_chuc_id UUID REFERENCES to_chuc(id) ON DELETE RESTRICT;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS goi_to_chuc_ma TEXT REFERENCES goi_to_chuc(ma);
CREATE INDEX IF NOT EXISTS idx_orders_to_chuc ON orders(to_chuc_id, created_at DESC) WHERE to_chuc_id IS NOT NULL;
ALTER TABLE orders DROP CONSTRAINT IF EXISTS ck_orders_loai;
ALTER TABLE orders ADD CONSTRAINT ck_orders_loai CHECK (
       (loai = 'goi' AND plan_id IS NOT NULL AND cycle IS NOT NULL AND goi_nap_ma IS NULL
            AND goi_dung_luong_ma IS NULL AND so_thang IS NULL AND to_chuc_id IS NULL AND goi_to_chuc_ma IS NULL)
    OR (loai = 'nap' AND goi_nap_ma IS NOT NULL AND plan_id IS NULL AND cycle IS NULL
            AND goi_dung_luong_ma IS NULL AND so_thang IS NULL AND goi_to_chuc_ma IS NULL)
    OR (loai = 'dung_luong' AND goi_dung_luong_ma IS NOT NULL AND so_thang BETWEEN 1 AND 12
            AND plan_id IS NULL AND cycle IS NULL AND goi_nap_ma IS NULL AND goi_to_chuc_ma IS NULL)
    OR (loai = 'goi_to_chuc' AND goi_to_chuc_ma IS NOT NULL AND to_chuc_id IS NOT NULL
            AND plan_id IS NULL AND cycle IS NULL AND goi_nap_ma IS NULL AND goi_dung_luong_ma IS NULL AND so_thang IS NULL)
);

-- Lớp chặn thứ hai (API đã kiểm): đơn gắn tổ chức chỉ do quản trị hoạt động của tổ chức đó tạo.
CREATE OR REPLACE FUNCTION orders_kiem_to_chuc() RETURNS trigger
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
BEGIN
    IF NEW.to_chuc_id IS NOT NULL AND to_chuc_vai_tro(NEW.to_chuc_id, NEW.user_id) IS DISTINCT FROM 'quan_tri' THEN
        RAISE EXCEPTION 'orders: chỉ quản trị của tổ chức mới tạo đơn cho tổ chức' USING ERRCODE = '42501';
    END IF;
    RETURN NEW;
END;
$$;
DROP TRIGGER IF EXISTS trg_orders_kiem_to_chuc ON orders;
CREATE TRIGGER trg_orders_kiem_to_chuc BEFORE INSERT OR UPDATE OF to_chuc_id, user_id ON orders
    FOR EACH ROW EXECUTE FUNCTION orders_kiem_to_chuc();

-- Kích hoạt gói tổ chức của đơn ĐÃ trả (trả năm). Cùng gói còn hạn → nối từ ngày hết hạn; khác gói / hết hạn → từ lúc trả.
-- Gọi lại sau khi đã ghi kỳ vào orders: trả lại kỳ đó, không đổi to_chuc.
CREATE OR REPLACE FUNCTION to_chuc_cap_goi(p_order UUID) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE o orders%ROWTYPE; t to_chuc%ROWTYPE; v_goc TIMESTAMPTZ; v_noi BOOLEAN; v_bat TIMESTAMPTZ; v_het TIMESTAMPTZ;
BEGIN
    SELECT * INTO o FROM orders WHERE id = p_order;
    IF NOT FOUND OR o.loai <> 'goi_to_chuc' OR o.status <> 'paid' THEN RETURN NULL; END IF;
    SELECT * INTO t FROM to_chuc WHERE id = o.to_chuc_id FOR NO KEY UPDATE;
    IF o.period_start IS NOT NULL THEN
        RETURN jsonb_build_object('bat_dau', o.period_start, 'het_han', o.period_end);
    END IF;
    v_goc := COALESCE(o.paid_at, now());
    v_noi := t.goi_ma = o.goi_to_chuc_ma AND COALESCE(t.goi_het_han > v_goc, FALSE);
    v_bat := CASE WHEN v_noi THEN t.goi_het_han ELSE v_goc END;
    v_het := v_bat + interval '1 year';
    UPDATE to_chuc
       SET goi_ma = o.goi_to_chuc_ma,
           goi_bat_dau = CASE WHEN v_noi THEN t.goi_bat_dau ELSE v_goc END,
           goi_het_han = v_het, sua_luc = now(), phien_ban = phien_ban + 1
     WHERE id = t.id;
    RETURN jsonb_build_object('bat_dau', v_bat, 'het_han', v_het);
END;
$$;

-- Thay bản 005: đơn có to_chuc_id → lô vào ví tổ chức (khóa to_chuc); không có → GIỮ NGUYÊN 005 (khóa users).
CREATE OR REPLACE FUNCTION credit_cap_nap(p_order UUID) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE o orders%ROWTYPE; g JSONB; v_so INT; v_ngay INT; v_lo UUID; v_het TIMESTAMPTZ;
BEGIN
    SELECT * INTO o FROM orders WHERE id = p_order;
    IF NOT FOUND OR o.loai <> 'nap' OR o.status <> 'paid' THEN RETURN NULL; END IF;
    IF o.to_chuc_id IS NOT NULL THEN
        PERFORM 1 FROM to_chuc WHERE id = o.to_chuc_id FOR NO KEY UPDATE;
    ELSE
        PERFORM 1 FROM users WHERE id = o.user_id FOR NO KEY UPDATE;
    END IF;
    SELECT gia_tri INTO g FROM credit_bang_gia WHERE loai = 'goi_nap' AND ma = o.goi_nap_ma;
    IF g IS NULL THEN
        RAISE EXCEPTION 'credit_cap_nap: không có gói nạp % trong bảng giá', o.goi_nap_ma;
    END IF;
    v_so := (g->>'credit')::INT;
    v_ngay := COALESCE((g->>'so_ngay')::INT, 90);
    IF v_so IS NULL OR v_so <= 0 OR v_ngay <= 0 THEN
        RAISE EXCEPTION 'credit_cap_nap: gói nạp % sai (credit %, so_ngay %)', o.goi_nap_ma, v_so, v_ngay;
    END IF;
    v_het := COALESCE(o.paid_at, NOW()) + make_interval(days => v_ngay);

    INSERT INTO credit_lo (user_id, vi_loai, vi_id, nguon, so_cap, so_con, het_han, order_id)
    VALUES (CASE WHEN o.to_chuc_id IS NULL THEN o.user_id END,
            CASE WHEN o.to_chuc_id IS NULL THEN 'ca_nhan' ELSE 'to_chuc' END,
            COALESCE(o.to_chuc_id, o.user_id), 'nap', v_so, v_so, v_het, o.id)
    ON CONFLICT (order_id) WHERE nguon = 'nap' DO NOTHING
    RETURNING id INTO v_lo;
    IF v_lo IS NOT NULL THEN
        INSERT INTO credit_so (user_id, lo_id, loai, so, ghi_chu, boi)
        VALUES (CASE WHEN o.to_chuc_id IS NULL THEN o.user_id END, v_lo, 'cap', v_so,
                'Gói nạp ' || o.goi_nap_ma || ' · đơn ' || o.code, o.paid_source);
    END IF;

    RETURN (SELECT jsonb_build_object('lo_id', id, 'so_cap', so_cap, 'het_han', het_han)
              FROM credit_lo WHERE order_id = o.id AND nguon = 'nap');
END;
$$;

-- Thay bản 011: đơn có to_chuc_id → cộng vào kho tổ chức (khóa to_chuc); không có → GIỮ NGUYÊN 011.
CREATE OR REPLACE FUNCTION kho_cap_dung_luong(p_order UUID) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE o orders%ROWTYPE; g JSONB; v_mb INT; v_goc TIMESTAMPTZ; v_bat_dau TIMESTAMPTZ; m kho_dung_luong_mua%ROWTYPE;
        v_chu_loai TEXT; v_chu_id UUID;
BEGIN
    SELECT * INTO o FROM orders WHERE id = p_order;
    IF NOT FOUND OR o.loai <> 'dung_luong' OR o.status <> 'paid' THEN RETURN NULL; END IF;
    v_chu_loai := CASE WHEN o.to_chuc_id IS NULL THEN 'ca_nhan' ELSE 'to_chuc' END;
    v_chu_id := COALESCE(o.to_chuc_id, o.user_id);
    IF o.to_chuc_id IS NOT NULL THEN
        PERFORM 1 FROM to_chuc WHERE id = o.to_chuc_id FOR NO KEY UPDATE;
    ELSE
        PERFORM 1 FROM users WHERE id = o.user_id FOR NO KEY UPDATE;
    END IF;

    SELECT * INTO m FROM kho_dung_luong_mua WHERE order_id = o.id;
    IF NOT FOUND THEN
        SELECT gia_tri INTO g FROM credit_bang_gia WHERE loai = 'goi_dung_luong' AND ma = o.goi_dung_luong_ma;
        IF g IS NULL THEN
            RAISE EXCEPTION 'kho_cap_dung_luong: không có gói dung lượng % trong bảng giá', o.goi_dung_luong_ma;
        END IF;
        v_mb := (g->>'them_mb')::INT;
        IF v_mb IS NULL OR v_mb <= 0 THEN
            RAISE EXCEPTION 'kho_cap_dung_luong: gói % sai (them_mb %)', o.goi_dung_luong_ma, v_mb;
        END IF;
        v_goc := COALESCE(o.paid_at, now());
        SELECT GREATEST(v_goc, COALESCE(max(het_han), v_goc)) INTO v_bat_dau FROM kho_dung_luong_mua
         WHERE chu_loai = v_chu_loai AND chu_id = v_chu_id AND goi_ma = o.goi_dung_luong_ma AND het_han > v_goc;
        INSERT INTO kho_dung_luong_mua (chu_loai, chu_id, goi_ma, them_mb, bat_dau, het_han, order_id)
        VALUES (v_chu_loai, v_chu_id, o.goi_dung_luong_ma, v_mb, v_bat_dau,
                v_bat_dau + make_interval(months => o.so_thang), o.id)
        ON CONFLICT (order_id) DO NOTHING;
        SELECT * INTO m FROM kho_dung_luong_mua WHERE order_id = o.id;
    END IF;
    RETURN jsonb_build_object('mua_id', m.id, 'them_mb', m.them_mb, 'bat_dau', m.bat_dau, 'het_han', m.het_han);
END;
$$;

-- Thay bản 011: thêm nhánh goi_to_chuc. Mọi nhánh khác GIỮ NGUYÊN 011.
CREATE OR REPLACE FUNCTION pay_order(p_code TEXT, p_source TEXT, p_ref TEXT)
RETURNS TEXT
LANGUAGE plpgsql
SET search_path = public
AS $$
DECLARE
    o orders%ROWTYPE;
    u users%ROWTYPE;
    v_extend BOOLEAN;
    v_start TIMESTAMPTZ;
    v_end TIMESTAMPTZ;
    v_lo JSONB;
    v_mua JSONB;
    v_ky JSONB;
BEGIN
    SELECT * INTO o FROM orders WHERE code = p_code FOR UPDATE;
    IF NOT FOUND THEN RETURN 'not_found'; END IF;
    IF o.status = 'paid' THEN RETURN 'already_paid'; END IF;
    IF o.status = 'cancelled' AND p_source <> 'admin' THEN RETURN 'cancelled'; END IF;

    SELECT * INTO u FROM users WHERE id = o.user_id FOR NO KEY UPDATE;

    IF o.loai = 'nap' THEN
        UPDATE orders
           SET status = 'paid', paid_source = p_source, paid_ref = p_ref, paid_at = NOW(), period_start = NOW()
         WHERE id = o.id;
        v_lo := credit_cap_nap(o.id);
        UPDATE orders SET period_end = (v_lo->>'het_han')::TIMESTAMPTZ WHERE id = o.id;
        RETURN 'paid';
    END IF;

    IF o.loai = 'dung_luong' THEN
        UPDATE orders SET status = 'paid', paid_source = p_source, paid_ref = p_ref, paid_at = NOW() WHERE id = o.id;
        v_mua := kho_cap_dung_luong(o.id);
        UPDATE orders SET period_start = (v_mua->>'bat_dau')::TIMESTAMPTZ, period_end = (v_mua->>'het_han')::TIMESTAMPTZ
         WHERE id = o.id;
        RETURN 'paid';
    END IF;

    IF o.loai = 'goi_to_chuc' THEN
        UPDATE orders SET status = 'paid', paid_source = p_source, paid_ref = p_ref, paid_at = NOW() WHERE id = o.id;
        v_ky := to_chuc_cap_goi(o.id);
        UPDATE orders SET period_start = (v_ky->>'bat_dau')::TIMESTAMPTZ, period_end = (v_ky->>'het_han')::TIMESTAMPTZ
         WHERE id = o.id;
        RETURN 'paid';
    END IF;

    v_extend := u.subscription_plan_id = o.plan_id AND COALESCE(u.subscription_end_date > NOW(), FALSE);
    v_start := CASE WHEN v_extend THEN u.subscription_end_date ELSE NOW() END;
    v_end := v_start + CASE o.cycle WHEN 'year' THEN INTERVAL '1 year' ELSE INTERVAL '1 month' END;

    UPDATE users
       SET subscription_plan_id = o.plan_id,
           subscription_start_date = CASE WHEN v_extend THEN u.subscription_start_date ELSE v_start END,
           subscription_end_date = v_end
     WHERE id = u.id;

    UPDATE orders
       SET status = 'paid', paid_source = p_source, paid_ref = p_ref, paid_at = NOW(),
           period_start = v_start, period_end = v_end
     WHERE id = o.id;

    RETURN 'paid';
END;
$$;

DO $$
DECLARE f TEXT;
BEGIN
    FOREACH f IN ARRAY ARRAY['credit_cap_nap(uuid)', 'kho_cap_dung_luong(uuid)', 'pay_order(text,text,text)'] LOOP
        EXECUTE format('REVOKE ALL ON FUNCTION %s FROM PUBLIC, anon, authenticated', f);
        EXECUTE format('GRANT EXECUTE ON FUNCTION %s TO service_role', f);
    END LOOP;
    FOREACH f IN ARRAY ARRAY['to_chuc_cap_goi(uuid)', 'orders_kiem_to_chuc()'] LOOP
        EXECUTE format('REVOKE ALL ON FUNCTION %s FROM PUBLIC, anon, authenticated, service_role', f);
    END LOOP;
END;
$$;
```

- [ ] **Step 4: Chạy kiểm — mới + đơn / nạp / dung lượng cũ**

Run: `npx tsx --test tests/don-to-chuc-csdl.test.ts tests/goi-nap-csdl.test.ts tests/kho-dung-luong-csdl.test.ts tests/credit-csdl.test.ts`
Expected: tất cả PASS, bài cũ không sửa.

- [ ] **Step 5: Commit**

```bash
git add supabase/migrations/020_don_to_chuc.sql tests/ho-tro/csdl-thu.ts tests/don-to-chuc-csdl.test.ts
git commit -m "feat(to-chuc): 020 don to chuc - goi_to_chuc tra nam, nap va dung luong cho to chuc, trigger quan tri

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 9: Ngữ cảnh tổ chức ở API — `Chu` có người gọi + vai trò, header `X-VBP-To-Chuc`, CORS

**Files:**
- Modify: `src/lib/kho/chu-so-huu.ts`
- Create: `src/lib/to-chuc/loi.ts`, `src/lib/to-chuc/ngu-canh.ts`
- Modify: `src/lib/kho/tra-loi.ts` (`canNguoiDung` đọc header, thêm `canTaiKhoan`)
- Modify: `src/lib/kho/loi.ts` (`tuLoiSql` thêm `KHONG_DU_QUYEN`)
- Modify: `src/lib/phien/cors.ts` (`DAU`)
- Test: `tests/to-chuc-thuan.test.ts` (mới)

**Interfaces:**
- Produces:
  - `type VaiTro = "quan_tri" | "van_thu" | "can_bo"`; `type Chu = { chu_loai; chu_id; nguoi?: string; vai_tro?: VaiTro }`;
    `chuToChuc(toChucId, userId, vaiTro): Chu`; `thamSoNguoi(c: Chu): { p_nguoi?: string }`; `laQuanLy(c: Chu): boolean`.
  - `DAU_TO_CHUC = "x-vbp-to-chuc"`; `docNguCanh(rpc: GoiRpc, userId: string, giaTri: string | null): Promise<{ chu: Chu } | KetQua>`.
  - `LOI_TO_CHUC.{ khongThuoc, khongDuQuyen, goiHetHan, chuaGuiThu }`; `tuLoiSqlToChuc(r): KetQua`;
    `tuKetQuaToChuc(r, khoa, message): KetQua`.
  - `canNguoiDung(req, tuy?: { chiQuanLy?: boolean }): Promise<{ user: ApiUser; chu: Chu } | NextResponse>`;
    `canTaiKhoan(req): Promise<{ user: ApiUser } | NextResponse>`.

- [ ] **Step 1: Viết bài kiểm thuần `tests/to-chuc-thuan.test.ts`**

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { chuCuaNguoiDung, chuToChuc, laQuanLy, thamSoNguoi } from "../src/lib/kho/chu-so-huu";
import { docNguCanh } from "../src/lib/to-chuc/ngu-canh";
import { tuLoiSqlToChuc } from "../src/lib/to-chuc/loi";
import { docNguon, quyetDinhCors } from "../src/lib/phien/cors";
import type { GoiRpc } from "../src/lib/phien/rpc";

const TC = "6f1c2b9e-1d2a-4c3b-9a8e-7f6d5c4b3a21";
const U = "11111111-2222-4333-8444-555555555555";

function rpcGia(tra: unknown) {
  const goi: Array<[string, Record<string, unknown>]> = [];
  const rpc: GoiRpc = async (ten, ts) => { goi.push([ten, ts]); return tra as any; };
  return { rpc, goi };
}

test("thamSoNguoi: kho cá nhân không gửi p_nguoi (giữ chữ ký cũ); kho tổ chức gửi người gọi", () => {
  assert.deepEqual(thamSoNguoi(chuCuaNguoiDung({ id: U })), {});
  assert.deepEqual(thamSoNguoi(chuToChuc(TC, U, "can_bo")), { p_nguoi: U });
});

test("laQuanLy: cá nhân, quản trị, văn thư được ghi khung kho; cán bộ không", () => {
  assert.equal(laQuanLy(chuCuaNguoiDung({ id: U })), true);
  assert.equal(laQuanLy(chuToChuc(TC, U, "quan_tri")), true);
  assert.equal(laQuanLy(chuToChuc(TC, U, "van_thu")), true);
  assert.equal(laQuanLy(chuToChuc(TC, U, "can_bo")), false);
});

test("docNguCanh: không header → cá nhân, không gọi CSDL", async () => {
  const g = rpcGia(null);
  for (const v of [null, "", "   "]) {
    const r = await docNguCanh(g.rpc, U, v);
    assert.deepEqual(r, { chu: { chu_loai: "ca_nhan", chu_id: U } });
  }
  assert.equal(g.goi.length, 0);
});

test("docNguCanh: header không phải uuid → 403, không gọi CSDL", async () => {
  const g = rpcGia("quan_tri");
  const r = await docNguCanh(g.rpc, U, "' OR 1=1 --");
  assert.equal((r as any).code, "KHONG_THUOC_TO_CHUC");
  assert.equal((r as any).http, 403);
  assert.equal(g.goi.length, 0);
});

test("docNguCanh: không phải thành viên / vai trò lạ → 403; thành viên → Chu tổ chức (id viết thường)", async () => {
  assert.equal(((await docNguCanh(rpcGia(null).rpc, U, TC)) as any).code, "KHONG_THUOC_TO_CHUC");
  assert.equal(((await docNguCanh(rpcGia("admin").rpc, U, TC)) as any).code, "KHONG_THUOC_TO_CHUC");
  const g = rpcGia("van_thu");
  const r = await docNguCanh(g.rpc, U, TC.toUpperCase());
  assert.deepEqual(r, { chu: { chu_loai: "to_chuc", chu_id: TC, nguoi: U, vai_tro: "van_thu" } });
  assert.deepEqual(g.goi, [["to_chuc_vai_tro", { p_to_chuc: TC, p_user: U }]]);
});

test("tuLoiSqlToChuc: mã tổ chức → đúng HTTP; mã kho dùng lại; mã lạ ném lỗi", () => {
  const ma = (x: any) => [x.http, x.code];
  assert.deepEqual(ma(tuLoiSqlToChuc({ loi: "QUAN_TRI_CUOI" })), [409, "QUAN_TRI_CUOI"]);
  assert.deepEqual(ma(tuLoiSqlToChuc({ loi: "SAI_EMAIL_MOI" })), [403, "SAI_EMAIL_MOI"]);
  assert.deepEqual(ma(tuLoiSqlToChuc({ loi: "LOI_MOI_KHONG_HOP_LE" })), [400, "LOI_MOI_KHONG_HOP_LE"]);
  assert.deepEqual(ma(tuLoiSqlToChuc({ loi: "KHONG_DU_QUYEN" })), [403, "KHONG_DU_QUYEN"]);
  assert.deepEqual(ma(tuLoiSqlToChuc({ loi: "KHONG_TIM_THAY" })), [404, "KHONG_TIM_THAY"]);
  const x: any = tuLoiSqlToChuc({ loi: "XUNG_DOT_PHIEN_BAN", ban_moi: { id: 1 } });
  assert.deepEqual([x.http, x.data.ban_moi], [409, { id: 1 }]);
  assert.throws(() => tuLoiSqlToChuc({ loi: "LA_LAM" }));
});

test("chữ báo lỗi không có đại từ nhân xưng", () => {
  const ma = ["KHONG_THUOC_TO_CHUC", "KHONG_DU_QUYEN", "QUAN_TRI_CUOI", "DU_THANH_VIEN", "DA_LA_THANH_VIEN", "QUA_NHIEU_TO_CHUC",
    "LOI_MOI_KHONG_HOP_LE", "SAI_EMAIL_MOI", "EMAIL_CHUA_XAC_MINH", "CHU_TRI_DA_CO", "DA_GIAO", "NGUOI_NHAN_KHONG_HOP_LE"];
  const cam = /(^|[^\p{L}])(tôi|chúng tôi|ta|chúng ta|anh|chị|em|bạn|mình|dạ|vâng|ạ)(?=$|[^\p{L}])/iu;
  for (const m of ma) {
    const x: any = tuLoiSqlToChuc({ loi: m });
    assert.equal(cam.test(x.message), false, `${m}: ${x.message}`);
  }
});

test("CORS: header X-VBP-To-Chuc được phép cả nguồn cho phép lẫn «*»", () => {
  const ds = docNguon("https://app.vanbanplus.giakiemso.com");
  const a = quyetDinhCors("/api/v2/kho/van-ban", "OPTIONS", "https://app.vanbanplus.giakiemso.com", ds);
  assert.match(a.dau["Access-Control-Allow-Headers"], /X-VBP-To-Chuc/);
  const b = quyetDinhCors("/api/v2/kho/van-ban", "GET", null, ds);
  assert.match(b.dau["Access-Control-Allow-Headers"], /X-VBP-To-Chuc/);
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx tsx --test tests/to-chuc-thuan.test.ts`
Expected: FAIL — `Cannot find module '../src/lib/to-chuc/ngu-canh'`.

- [ ] **Step 3: Viết mã**

`src/lib/kho/chu-so-huu.ts` (thay toàn bộ tệp):

```ts
/** Chủ sở hữu bản ghi kho (hợp đồng §5). Đợt B: kho tổ chức mang thêm người gọi + vai trò (spec đợt B §3). */
export type VaiTro = "quan_tri" | "van_thu" | "can_bo";
export const VAI_TRO: readonly VaiTro[] = ["quan_tri", "van_thu", "can_bo"];
export type Chu = { chu_loai: "ca_nhan" | "to_chuc"; chu_id: string; nguoi?: string; vai_tro?: VaiTro };

export function chuCuaNguoiDung(user: { id: string }): Chu {
  return { chu_loai: "ca_nhan", chu_id: user.id };
}

export function chuToChuc(toChucId: string, userId: string, vaiTro: VaiTro): Chu {
  return { chu_loai: "to_chuc", chu_id: toChucId, nguoi: userId, vai_tro: vaiTro };
}

/** Tham số đặt tên cho các hàm SQL kho_*. */
export const thamSoChu = (c: Chu) => ({ p_chu_loai: c.chu_loai, p_chu_id: c.chu_id });

/** p_nguoi cho bản hàm có lọc quyền (kho_van_ban_*, kho_tep_*): chỉ gửi ở kho tổ chức → kho cá nhân gọi đúng chữ ký cũ. */
export const thamSoNguoi = (c: Chu): { p_nguoi?: string } => (c.chu_loai === "to_chuc" ? { p_nguoi: c.nguoi } : {});

/** Ghi khung kho (thư mục, lịch họp, album, mẫu riêng): chủ kho cá nhân, quản trị, văn thư. */
export const laQuanLy = (c: Chu): boolean => c.chu_loai === "ca_nhan" || c.vai_tro === "quan_tri" || c.vai_tro === "van_thu";
```

`src/lib/to-chuc/loi.ts`:

```ts
import { loi, type KetQua } from "../phien/dich-vu";
import { tuLoiSql, xong } from "../kho/loi";

export const LOI_TO_CHUC = {
  khongThuoc: () => loi(403, "KHONG_THUOC_TO_CHUC", "Tài khoản không thuộc tổ chức này hoặc đã rời tổ chức."),
  khongDuQuyen: () => loi(403, "KHONG_DU_QUYEN", "Vai trò trong tổ chức không làm được thao tác này."),
  goiHetHan: () =>
    loi(403, "GOI_TO_CHUC_HET_HAN", "Gói của tổ chức chưa kích hoạt hoặc đã hết hạn. Quản trị gia hạn gói để dùng credit của tổ chức."),
  chuaGuiThu: () => loi(503, "CHUA_GUI_DUOC_THU", "Chưa gửi được thư mời. Thử lại sau ít phút."),
};

const THONG_BAO: Record<string, [number, string]> = {
  KHONG_THUOC_TO_CHUC: [403, "Tài khoản không thuộc tổ chức này hoặc đã rời tổ chức."],
  QUAN_TRI_CUOI: [409, "Tổ chức cần ít nhất một quản trị. Giao quyền quản trị cho người khác trước."],
  DU_THANH_VIEN: [409, "Tổ chức đã đủ số thành viên tối đa của gói. Nâng gói hoặc bớt thành viên rồi thử lại."],
  DA_LA_THANH_VIEN: [409, "Email này đã là thành viên của tổ chức."],
  QUA_NHIEU_TO_CHUC: [409, "Mỗi tài khoản tạo tối đa 3 tổ chức chưa có gói."],
  LOI_MOI_KHONG_HOP_LE: [400, "Lời mời không hợp lệ, đã dùng, đã hủy hoặc đã hết hạn."],
  SAI_EMAIL_MOI: [403, "Lời mời gửi tới một email khác. Đăng nhập đúng tài khoản được mời rồi mở lại đường dẫn."],
  EMAIL_CHUA_XAC_MINH: [403, "Email của tài khoản chưa được xác minh. Đăng nhập một lần bằng mã gửi qua email rồi mở lại đường dẫn."],
  CHU_TRI_DA_CO: [409, "Văn bản đã có người chủ trì. Đổi người chủ trì hiện tại hoặc giao phối hợp."],
  DA_GIAO: [409, "Người này đã được giao văn bản này."],
  NGUOI_NHAN_KHONG_HOP_LE: [400, "Người nhận phải là thành viên đang hoạt động của tổ chức."],
};

/** Lỗi của hàm SQL to_chuc_* / kho_giao_viec_* → KetQua. Mã kho dùng chung (KHONG_TIM_THAY, KHONG_DU_QUYEN…) đi qua tuLoiSql. */
export function tuLoiSqlToChuc(r: { loi: string; thong_bao?: string; ban_moi?: unknown }): KetQua {
  if (r.loi === "XUNG_DOT_PHIEN_BAN") {
    return loi(409, "XUNG_DOT_PHIEN_BAN", "Thông tin vừa được sửa ở nơi khác. Tải lại bản mới rồi sửa tiếp.", { ban_moi: r.ban_moi });
  }
  const t = THONG_BAO[r.loi];
  return t ? loi(t[0], r.loi, t[1]) : tuLoiSql(r);
}

/** { ok, <khoa> } | { loi } → KetQua. */
export function tuKetQuaToChuc(r: any, khoa: string | null, message = "Thành công"): KetQua {
  if (r && typeof r === "object" && "loi" in r) return tuLoiSqlToChuc(r);
  return xong(khoa ? r[khoa] : null, message);
}
```

`src/lib/to-chuc/ngu-canh.ts`:

```ts
import type { KetQua } from "../phien/dich-vu";
import type { GoiRpc } from "../phien/rpc";
import { chuCuaNguoiDung, chuToChuc, VAI_TRO, type Chu, type VaiTro } from "../kho/chu-so-huu";
import { laUuid } from "../kho/kiem-dau-vao";
import { LOI_TO_CHUC } from "./loi";

/** Header chọn ngữ cảnh (web gắn ở src/api/khach.ts). Next chuẩn hóa tên header về chữ thường. */
export const DAU_TO_CHUC = "x-vbp-to-chuc";

/**
 * Ngữ cảnh của yêu cầu: không header → kho / ví cá nhân như đợt A (không gọi CSDL). Có header → phải là uuid của một
 * tổ chức mà người gọi là thành viên hoạt động (to_chuc_vai_tro); sai → 403 KHONG_THUOC_TO_CHUC.
 */
export async function docNguCanh(rpc: GoiRpc, userId: string, giaTri: string | null): Promise<{ chu: Chu } | KetQua> {
  const id = (giaTri ?? "").trim();
  if (!id) return { chu: chuCuaNguoiDung({ id: userId }) };
  if (!laUuid(id)) return LOI_TO_CHUC.khongThuoc();
  const toChuc = id.toLowerCase();
  const vai = await rpc<string | null>("to_chuc_vai_tro", { p_to_chuc: toChuc, p_user: userId });
  if (!vai || !(VAI_TRO as readonly string[]).includes(vai)) return LOI_TO_CHUC.khongThuoc();
  return { chu: chuToChuc(toChuc, userId, vai as VaiTro) };
}
```

`src/lib/kho/loi.ts` — trong `switch (r.loi)` của `tuLoiSql`, thêm trước `default`:

```ts
    case "KHONG_DU_QUYEN":
      return loi(403, "KHONG_DU_QUYEN", "Vai trò trong tổ chức không làm được thao tác này.");
```

`src/lib/kho/tra-loi.ts` — thay `canNguoiDung` và thêm `canTaiKhoan`:

```ts
import { rpcSupabase } from "../phien/rpc";
import { DAU_TO_CHUC, docNguCanh } from "../to-chuc/ngu-canh";
import { LOI_TO_CHUC } from "../to-chuc/loi";
import { laQuanLy } from "./chu-so-huu";

/** Lỗi xác thực trả đúng mã để web biết gọi lam-moi. */
function loiXacThuc(loi: "CHUA_DANG_NHAP" | "PHIEN_HET_HAN" | "PHIEN_THU_HOI"): NextResponse {
  return traLoi(loi === "PHIEN_HET_HAN" ? LOI.phienHetHan() : loi === "PHIEN_THU_HOI" ? LOI.phienThuHoi() : LOI.chuaDangNhap());
}

/**
 * Route kho: Bearer (web / app) hoặc X-API-Key, rồi ngữ cảnh theo header X-VBP-To-Chuc (không header = cá nhân).
 * `chiQuanLy`: route ghi khung kho (thư mục, lịch họp, album, mẫu riêng) — cán bộ của tổ chức nhận 403 KHONG_DU_QUYEN.
 */
export async function canNguoiDung(
  req: NextRequest, tuy: { chiQuanLy?: boolean } = {}
): Promise<{ user: ApiUser; chu: Chu } | NextResponse> {
  const r = await xacThucChiTiet(req);
  if ("loi" in r) return loiXacThuc(r.loi);
  const nc = await docNguCanh(rpcSupabase, r.user.id, req.headers.get(DAU_TO_CHUC));
  if (!("chu" in nc)) return traLoi(nc);
  if (tuy.chiQuanLy && !laQuanLy(nc.chu)) return traLoi(LOI_TO_CHUC.khongDuQuyen());
  return { user: r.user, chu: nc.chu };
}

/** Route tài khoản / tổ chức (/api/v2/to-chuc/*, /loi-moi/nhan): chỉ xác thực, BỎ QUA header ngữ cảnh. */
export async function canTaiKhoan(req: NextRequest): Promise<{ user: ApiUser } | NextResponse> {
  const r = await xacThucChiTiet(req);
  if ("loi" in r) return loiXacThuc(r.loi);
  return { user: r.user };
}
```

(Bỏ `import { chuCuaNguoiDung, type Chu } from "./chu-so-huu";` cũ, thay bằng `import { laQuanLy, type Chu } from "./chu-so-huu";`.)

`src/lib/phien/cors.ts`:

```ts
const DAU = "Content-Type,X-API-Key,Authorization,X-VBP-Nguon,X-VBP-Phien-Ban,X-VBP-To-Chuc";
```

- [ ] **Step 4: Chạy kiểm**

Run: `npx tsx --test tests/to-chuc-thuan.test.ts tests/kho-tra-loi.test.ts tests/kho-don-hang.test.ts tests/mo-ban-thuan.test.ts` rồi `npx tsc --noEmit`
Expected: PASS; tsc không lỗi.

- [ ] **Step 5: Commit**

```bash
git add src/lib/kho/chu-so-huu.ts src/lib/to-chuc/loi.ts src/lib/to-chuc/ngu-canh.ts src/lib/kho/tra-loi.ts src/lib/kho/loi.ts src/lib/phien/cors.ts tests/to-chuc-thuan.test.ts
git commit -m "feat(to-chuc): ngu canh to chuc qua header X-VBP-To-Chuc, Chu co nguoi goi + vai tro, CORS

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 10: Kho theo vai trò ở lớp dịch vụ + route ghi khung kho + Cơ quan theo ngữ cảnh

**Files:**
- Modify: `src/lib/kho/van-ban.ts`, `src/lib/kho/tep.ts`, `src/lib/kho/gan-tep.ts`, `src/lib/kho/han-muc.ts`,
  `src/lib/kho/kiem-dau-vao.ts`, `src/lib/kho/co-quan.ts`
- Modify (thêm `{ chiQuanLy: true }`): `src/app/api/v2/kho/thu-muc/route.ts` (POST), `src/app/api/v2/kho/thu-muc/[id]/route.ts`
  (PATCH, DELETE), `src/app/api/v2/kho/cuoc-hop/route.ts` (POST), `src/app/api/v2/kho/cuoc-hop/[id]/route.ts` (PATCH, DELETE),
  `src/app/api/v2/kho/album/route.ts` (POST), `src/app/api/v2/kho/album/[id]/route.ts` (PATCH, DELETE),
  `src/app/api/v2/kho/album/[id]/xin-tai-len/route.ts` (POST), `src/app/api/v2/kho/album/[id]/anh/[tep_id]/route.ts` (PATCH),
  `src/app/api/v2/mau/rieng/route.ts` (POST), `src/app/api/v2/mau/rieng/[id]/route.ts` (PATCH, DELETE)
- Modify: `src/app/api/v2/toi/co-quan/route.ts`
- Test: `tests/to-chuc-dich-vu.test.ts` (mới), `tests/to-chuc-thuan.test.ts` (thêm)

**Interfaces:**
- Consumes: Task 9 (`thamSoNguoi`, `chuToChuc`, `docNguCanh`), Task 4–5 (hàm SQL có `p_nguoi`), Task 2 (`to_chuc_co_quan_*`).
- Produces: `DungLuong.het_goi: boolean`; `kiemVanBan` nhận `cong_khai_noi_bo`; `docCoQuanChu(pt, chu): Promise<KetQua>`,
  `ghiCoQuanChu(pt, chu, than: unknown): Promise<KetQua>`.

- [ ] **Step 1: Viết bài kiểm**

Thêm vào `tests/to-chuc-thuan.test.ts`:

```ts
import { kiemVanBan } from "../src/lib/kho/kiem-dau-vao";

test("kiemVanBan: cong_khai_noi_bo phải là boolean", () => {
  assert.deepEqual((kiemVanBan({ cong_khai_noi_bo: true }, "sua") as any).du_lieu, { cong_khai_noi_bo: true });
  assert.ok("loi" in kiemVanBan({ cong_khai_noi_bo: "co" }, "sua"));
});
```

Tạo `tests/to-chuc-dich-vu.test.ts`:

```ts
import { after, before, describe, test } from "node:test";
import assert from "node:assert/strict";
import { Client } from "pg";
import { CO_CSDL_THU, taoCsdlThu, taoNguoiDung } from "./ho-tro/csdl-thu";
import { rpcPg } from "./ho-tro/rpc-pg";
import { khoTepGia } from "./ho-tro/kho";
import { boToChuc, hetGoiToChuc } from "./ho-tro/to-chuc";
import type { PhuThuocKho } from "../src/lib/kho/phu-thuoc";
import { chuCuaNguoiDung, chuToChuc, type VaiTro } from "../src/lib/kho/chu-so-huu";
import { docNguCanh } from "../src/lib/to-chuc/ngu-canh";
import { docVanBan, suaVanBan, taoVanBan, timVanBan, xoaVanBan } from "../src/lib/kho/van-ban";
import { taiVeTep, xacNhanTep, xinTaiLen, xoaTep } from "../src/lib/kho/tep";
import { docDungLuong } from "../src/lib/kho/han-muc";
import { docCoQuanChu, ghiCoQuanChu } from "../src/lib/kho/co-quan";
import type { KetQua } from "../src/lib/phien/dich-vu";

const du = (k: KetQua) => {
  assert.equal(k.loai, "xong", JSON.stringify(k));
  return (k as any).data;
};
const ma = (k: KetQua) => (k.loai === "loi" ? `${k.http} ${k.code}` : k.loai);
const NK = { ip: "1.2.3.4", thiet_bi: "thu" };
const CQ = { ten_co_quan: "UBND xã Gia Kiệm", xa: "Xã Gia Kiệm", tinh: "Tỉnh Đồng Nai", dac_diem: ["17 ấp"], nguoi_ky: "A", chuc_danh: "Chủ tịch" };

describe("dịch vụ kho trong ngữ cảnh tổ chức", { skip: !CO_CSDL_THU }, () => {
  let csdl: Awaited<ReturnType<typeof taoCsdlThu>>;
  let c: Client;
  let pt: PhuThuocKho;
  let st: ReturnType<typeof khoTepGia>;
  let b: Awaited<ReturnType<typeof boToChuc>>;
  const chu = (u: string, vai: VaiTro) => chuToChuc(b.toChuc, u, vai);

  before(async () => {
    csdl = await taoCsdlThu();
    c = new Client({ connectionString: csdl.url });
    await c.connect();
    st = khoTepGia();
    pt = { rpc: rpcPg(c), tep: st.k, bayGio: () => new Date(), ghiLog: () => {} };
    b = await boToChuc(c);
  });
  after(async () => {
    await c?.end();
    await csdl?.xoa();
  });

  test("docNguCanh trên CSDL thật: thành viên → Chu tổ chức; người ngoài → 403", async () => {
    const r = await docNguCanh(pt.rpc, b.canBo, b.toChuc);
    assert.deepEqual(r, { chu: chuToChuc(b.toChuc, b.canBo, "can_bo") });
    assert.equal(ma((await docNguCanh(pt.rpc, await taoNguoiDung(c), b.toChuc)) as KetQua), "403 KHONG_THUOC_TO_CHUC");
  });

  test("văn bản: cán bộ tạo được, không đọc văn bản riêng của văn thư (404), không sửa văn bản được giao (403)", async () => {
    const cuaVt = du(await taoVanBan(pt, { id: b.vanThu }, chu(b.vanThu, "van_thu"), { huong: "den", trich_yeu: "Văn thư lưu" }));
    const cuaCb = du(await taoVanBan(pt, { id: b.canBo }, chu(b.canBo, "can_bo"), { huong: "di", trich_yeu: "Cán bộ soạn" }));
    assert.equal(cuaCb.sua_duoc, true);
    assert.equal(ma(await docVanBan(pt, chu(b.canBo, "can_bo"), cuaVt.id)), "404 KHONG_TIM_THAY");
    await c.query(`INSERT INTO kho_giao_viec (chu_id, van_ban_id, nguoi_nhan, vai, giao_boi) VALUES ($1, $2, $3, 'chu_tri', $4)`,
      [b.toChuc, cuaVt.id, b.canBo, b.vanThu]);
    assert.equal(du(await docVanBan(pt, chu(b.canBo, "can_bo"), cuaVt.id)).sua_duoc, false);
    assert.equal(ma(await suaVanBan(pt, chu(b.canBo, "can_bo"), cuaVt.id, { phien_ban: 1, ghi_chu: "x" })), "403 KHONG_DU_QUYEN");
    assert.equal(ma(await xoaVanBan(pt, chu(b.canBo, "can_bo"), cuaVt.id, false, { user_id: b.canBo, ...NK })), "403 KHONG_DU_QUYEN");
    const ds = du(await timVanBan(pt, chu(b.canBo2, "can_bo"), new URLSearchParams()));
    assert.equal(ds.tong, 0);
  });

  test("tệp: cán bộ không xin tải lên vào văn bản được giao; tải về tệp văn bản ngoài phạm vi → 404", async () => {
    const vt = chu(b.vanThu, "van_thu");
    const vb = du(await taoVanBan(pt, { id: b.vanThu }, vt, { huong: "den", trich_yeu: "Có tệp" })).id;
    const x = du(await xinTaiLen(pt, { id: b.vanThu }, vt, { ten: "a.pdf", mime: "application/pdf", byte: 100, gan: { loai: "van_ban", id: vb } }));
    st.tep.set(`to_chuc/${b.toChuc}/${x.id}`, { byte: 100, mime: "application/pdf" });
    du(await xacNhanTep(pt, vt, { id: x.id }));
    assert.equal(ma(await taiVeTep(pt, chu(b.canBo2, "can_bo"), x.id)), "404 KHONG_TIM_THAY");
    assert.equal(ma(await xoaTep(pt, chu(b.canBo2, "can_bo"), x.id)), "404 KHONG_TIM_THAY");
    await c.query(`INSERT INTO kho_giao_viec (chu_id, van_ban_id, nguoi_nhan, vai, giao_boi) VALUES ($1, $2, $3, 'phoi_hop', $4)`,
      [b.toChuc, vb, b.canBo2, b.vanThu]);
    assert.match(du(await taiVeTep(pt, chu(b.canBo2, "can_bo"), x.id)).url, /^https:\/\/gia\.test\/tai-ve\//);
    assert.equal(ma(await xinTaiLen(pt, { id: b.canBo2 }, chu(b.canBo2, "can_bo"),
      { ten: "b.pdf", mime: "application/pdf", byte: 10, gan: { loai: "van_ban", id: vb } })), "404 KHONG_TIM_THAY");
  });

  test("dung lượng: het_goi có trong DungLuong; kho tổ chức hết gói → chỉ đọc", async () => {
    const k = await boToChuc(c);
    assert.equal((await docDungLuong(chuToChuc(k.toChuc, k.quanTri, "quan_tri"), pt.rpc)).het_goi, false);
    await hetGoiToChuc(c, k.toChuc);
    const d = await docDungLuong(chuToChuc(k.toChuc, k.quanTri, "quan_tri"), pt.rpc);
    assert.deepEqual([d.het_goi, d.chi_doc], [true, true]);
    assert.equal((await docDungLuong(chuCuaNguoiDung({ id: k.quanTri }), pt.rpc)).het_goi, false);
  });

  test("Cơ quan theo ngữ cảnh: thành viên đọc Cơ quan tổ chức; chỉ quản trị ghi; cá nhân giữ hồ sơ riêng", async () => {
    assert.deepEqual(du(await docCoQuanChu(pt, chu(b.canBo, "can_bo"))), { co_quan: null });
    assert.equal(ma(await ghiCoQuanChu(pt, chu(b.vanThu, "van_thu"), CQ)), "403 KHONG_DU_QUYEN");
    assert.deepEqual(du(await ghiCoQuanChu(pt, chu(b.quanTri, "quan_tri"), CQ)).co_quan, CQ);
    assert.deepEqual(du(await docCoQuanChu(pt, chu(b.canBo, "can_bo"))).co_quan, CQ);
    assert.deepEqual(du(await docCoQuanChu(pt, chuCuaNguoiDung({ id: b.canBo }))), { co_quan: null });
  });
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx tsx --test tests/to-chuc-dich-vu.test.ts tests/to-chuc-thuan.test.ts`
Expected: FAIL — `docCoQuanChu` không có; văn bản cán bộ đọc được văn bản riêng (dịch vụ chưa gửi `p_nguoi` → bản cũ ném
«thiếu người gọi»).

- [ ] **Step 3: Viết mã**

`src/lib/kho/van-ban.ts` — mọi lời gọi `kho_van_ban_*` thêm `...thamSoNguoi(chu)` (import `thamSoNguoi` từ `./chu-so-huu`):

```ts
export async function timVanBan(pt: PhuThuocKho, chu: Chu, sp: URLSearchParams): Promise<KetQua> {
  const l = docLoc(sp);
  if ("loi" in l) return tuKiem(l);
  return xong(await pt.rpc("kho_van_ban_tim", { ...thamSoChu(chu), ...thamSoNguoi(chu), p_loc: l.du_lieu }));
}

export async function taoVanBan(pt: PhuThuocKho, user: { id: string }, chu: Chu, than: Record<string, unknown>): Promise<KetQua> {
  const k = kiemVanBan(than, "tao");
  if ("loi" in k) return tuKiem(k);
  const r = await pt.rpc("kho_van_ban_tao", { ...thamSoChu(chu), ...thamSoNguoi(chu), p_tao_boi: user.id, p_du_lieu: k.du_lieu });
  return tuKetQuaSql(r, "van_ban", r?.da_co ? "Văn bản đã được lưu trước đó." : "Đã lưu văn bản.");
}

export async function docVanBan(pt: PhuThuocKho, chu: Chu, id: string): Promise<KetQua> {
  if (!laUuid(id)) return LOI_KHO.khongTimThay();
  const v = await pt.rpc("kho_van_ban_doc", { ...thamSoChu(chu), ...thamSoNguoi(chu), p_id: id });
  return v ? xong(v) : LOI_KHO.khongTimThay();
}
```

`suaVanBan`: `{ ...thamSoChu(chu), ...thamSoNguoi(chu), p_id: id, p_phien_ban: pb, p_du_lieu: k.du_lieu }`;
`khoiPhucVanBan`: `{ ...thamSoChu(chu), ...thamSoNguoi(chu), p_id: id }`; trong `xoaVanBan` hai lời gọi `kho_van_ban_xoa_mem`
và `kho_van_ban_xoa_han` đều thành `{ ...thamSoChu(chu), ...thamSoNguoi(chu), p_id: id }`. Phần còn lại giữ nguyên.

`src/lib/kho/tep.ts` — import `thamSoNguoi`; mọi lời gọi `kho_tep_bo`, `kho_tep_xin`, `kho_tep_doc`, `kho_tep_xac_nhan`,
`kho_tep_xoa` đổi `...thamSoChu(chu)` thành `...thamSoChu(chu), ...thamSoNguoi(chu)` (6 chỗ: `boTep`, `xinTaiLenDaKiem` ×2,
`xacNhanTep` ×2, `taiVeTep`, `xoaTep`). Ví dụ:

```ts
async function boTep(pt: PhuThuocKho, chu: Chu, id: string): Promise<void> {
  const r = await pt.rpc("kho_tep_bo", { ...thamSoChu(chu), ...thamSoNguoi(chu), p_id: id });
  // … phần còn lại giữ nguyên
}
```

`src/lib/kho/gan-tep.ts` — bộ kiểm `van_ban`:

```ts
import { thamSoChu, thamSoNguoi, type Chu } from "./chu-so-huu";
// …
const bang = new Map<LoaiGan, KiemGan>([
  ["van_ban", async (rpc, chu, id) => (await rpc("kho_van_ban_co", { ...thamSoChu(chu), ...thamSoNguoi(chu), p_id: id })) === true],
]);
```

`src/lib/kho/han-muc.ts` — `DungLuong` thêm `het_goi: boolean;` và trong `docDungLuong` thêm `het_goi: r.het_goi === true,`.

`src/lib/kho/kiem-dau-vao.ts` — trong `kiemVanBan`, ngay sau khối `gan_sao`:

```ts
    if (co("cong_khai_noi_bo")) {
      if (typeof than.cong_khai_noi_bo !== "boolean") sai("Công khai trong cơ quan phải là true hoặc false.");
      d.cong_khai_noi_bo = than.cong_khai_noi_bo;
    }
```

`src/lib/kho/co-quan.ts` — thêm (giữ `docCoQuan` / `ghiCoQuan` cũ):

```ts
import type { Chu } from "./chu-so-huu";
import { tuLoiSqlToChuc } from "../to-chuc/loi";

/** Cơ quan theo ngữ cảnh: cá nhân → nguoi_dung_co_quan (như 4.9); tổ chức → to_chuc_co_quan (thành viên đọc, quản trị ghi). */
export async function docCoQuanChu(pt: PhuThuocKho, chu: Chu): Promise<KetQua> {
  if (chu.chu_loai === "ca_nhan") return docCoQuan(pt, chu.chu_id);
  return xong({ co_quan: (await pt.rpc("to_chuc_co_quan_doc", { p_to_chuc: chu.chu_id, p_user: chu.nguoi })) ?? null });
}

export async function ghiCoQuanChu(pt: PhuThuocKho, chu: Chu, than: unknown): Promise<KetQua> {
  if (chu.chu_loai === "ca_nhan") return ghiCoQuan(pt, chu.chu_id, than);
  if (typeof than !== "object" || than === null || Array.isArray(than)) return LOI_KHO.dauVaoSai("Thông tin cơ quan không hợp lệ.");
  const k = kiemCoQuan(than as Record<string, unknown>);
  if ("loi" in k) return tuKiem(k);
  const d = await pt.rpc("tan_suat_dem", { p_khoa: `to-chuc-co-quan:${chu.chu_id}`, p_cua_so_giay: GHI_CUA_SO_GIAY, p_toi_da: GHI_TOI_DA });
  if (!d?.duoc) return quaNhieu(d?.thu_lai_sau_giay);
  const r = await pt.rpc("to_chuc_co_quan_ghi", { p_to_chuc: chu.chu_id, p_user: chu.nguoi, p_du_lieu: k.du_lieu });
  if (r?.loi) return tuLoiSqlToChuc(r);
  return xong({ co_quan: r.co_quan }, "Đã lưu thông tin cơ quan.");
}
```

`src/app/api/v2/toi/co-quan/route.ts` — thay hai lời gọi:

```ts
import { docCoQuanChu, ghiCoQuanChu } from "@lib/kho/co-quan";
// GET:
    return traLoi(await docCoQuanChu(pt, a.chu));
// PUT (sau đoạn đọc thân có trần):
    return traLoi(await ghiCoQuanChu(pt, a.chu, doc.duLieu ?? undefined));
```

Route ghi khung kho: ở mỗi handler ghi nêu trong **Files**, đổi `await canNguoiDung(req)` thành
`await canNguoiDung(req, { chiQuanLy: true })`. Ví dụ `src/app/api/v2/kho/thu-muc/route.ts`:

```ts
export async function POST(req: NextRequest) {
  return chay(async () => {
    const a = await canNguoiDung(req, { chiQuanLy: true });
    if (a instanceof NextResponse) return a;
    return traLoi(await taoThuMuc(pt, a.user, a.chu, await docThan(req)));
  });
}
```

(Handler GET của cùng tệp giữ `canNguoiDung(req)`.)

- [ ] **Step 4: Chạy kiểm**

Run: `npx tsx --test tests/to-chuc-dich-vu.test.ts tests/to-chuc-thuan.test.ts tests/kho-dich-vu.test.ts tests/hop-dich-vu.test.ts tests/mau-dich-vu.test.ts tests/mo-ban-dich-vu.test.ts` rồi `npx tsc --noEmit`
Expected: PASS (bài cũ không sửa: kho cá nhân không gửi `p_nguoi`).

- [ ] **Step 5: Commit**

```bash
git add src/lib/kho src/app/api/v2/kho src/app/api/v2/mau src/app/api/v2/toi/co-quan tests/to-chuc-dich-vu.test.ts tests/to-chuc-thuan.test.ts
git commit -m "feat(to-chuc): dich vu kho gui p_nguoi o kho to chuc, chan can bo ghi khung kho, co quan theo ngu canh

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 11: Dịch vụ + route tổ chức, thành viên, gói

**Files:**
- Create: `src/lib/to-chuc/kiem-dau-vao.ts`, `src/lib/to-chuc/to-chuc.ts`
- Create: `src/app/api/v2/to-chuc/route.ts`, `src/app/api/v2/to-chuc/[id]/route.ts`,
  `src/app/api/v2/to-chuc/[id]/thanh-vien/route.ts`, `src/app/api/v2/to-chuc/[id]/thanh-vien/[uid]/route.ts`,
  `src/app/api/v2/to-chuc/[id]/roi/route.ts`, `src/app/api/v2/goi-to-chuc/route.ts`
- Test: `tests/to-chuc-thuan.test.ts` (thêm), `tests/to-chuc-dich-vu.test.ts` (thêm `describe`)

**Interfaces:**
- Consumes: Task 2 (hàm SQL `to_chuc_*`, `goi_to_chuc_ds`), Task 9 (`canTaiKhoan`, `tuKetQuaToChuc`), `kiemCoQuan` (015).
- Produces:
  - `kiemToChuc(than, cheDo: "tao" | "sua"): KQ<Record<string, unknown>>`, `kiemLoiMoi(than): KQ<{ email: string; vai_tro: VaiTro }>`,
    `laMaMoi(v): v is string`, `kiemGiaoViec(than, cheDo): KQ<Record<string, unknown>>`, `docLocViec(sp): KQ<Record<string, unknown>>`
    (đều ở `src/lib/to-chuc/kiem-dau-vao.ts`; Task 12–13 dùng ba hàm sau);
  - `type CoRpc = { rpc: GoiRpc }`; `dsToChuc(pt, userId)`, `taoToChuc(pt, userId, than)`, `docToChuc(pt, userId, id)`,
    `suaToChuc(pt, userId, id, than)`, `dsThanhVien(pt, userId, id)`, `suaThanhVien(pt, userId, id, uid, than)`,
    `boThanhVien(pt, userId, id, uid)`, `roiToChuc(pt, userId, id)`, `dsGoiToChuc(pt)` — đều `Promise<KetQua>`.

- [ ] **Step 1: Viết bài kiểm**

Thêm vào `tests/to-chuc-thuan.test.ts`:

```ts
import { docLocViec, kiemGiaoViec, kiemLoiMoi, kiemToChuc, laMaMoi } from "../src/lib/to-chuc/kiem-dau-vao";

const dl = (k: any) => { assert.ok(!("loi" in k), JSON.stringify(k)); return k.du_lieu; };

test("kiemToChuc: tên bắt buộc lúc tạo, gộp khoảng trắng / xuống dòng; mã số thuế, email đúng dạng", () => {
  assert.deepEqual(dl(kiemToChuc({ ten: "  UBND xã\r\nGia Kiệm  " }, "tao")), { ten: "UBND xã Gia Kiệm" });
  assert.ok("loi" in kiemToChuc({}, "tao"));
  assert.ok("loi" in kiemToChuc({ ten: "A", mst: "12345" }, "tao"));
  assert.equal(dl(kiemToChuc({ ten: "A", mst: "0312345678-001" }, "tao")).mst, "0312345678-001");
  assert.equal(dl(kiemToChuc({ ten: "A", email_hoa_don: "KeToan@XA.gov.vn" }, "tao")).email_hoa_don, "ketoan@xa.gov.vn");
  assert.ok("loi" in kiemToChuc({ ten: "A", email_hoa_don: "khong-phai-email" }, "tao"));
  assert.ok("loi" in kiemToChuc({}, "sua"));
  assert.ok("loi" in kiemToChuc({ co_quan: [] }, "sua"));
  assert.equal(dl(kiemToChuc({ co_quan: { ten_co_quan: "UBND", xa: "", tinh: "" } }, "sua")).co_quan.ten_co_quan, "UBND");
  assert.equal("co_quan" in dl(kiemToChuc({ ten: "A", co_quan: { ten_co_quan: "X" } }, "tao")), false); // tạo không nhận Cơ quan
});

test("kiemLoiMoi / laMaMoi: email chữ thường, vai trò trong danh sách; mã đúng 43 ký tự base64url", () => {
  assert.deepEqual(dl(kiemLoiMoi({ email: " VanThu@Xa.vn ", vai_tro: "van_thu" })), { email: "vanthu@xa.vn", vai_tro: "van_thu" });
  assert.ok("loi" in kiemLoiMoi({ email: "a@b.vn", vai_tro: "admin" }));
  assert.ok("loi" in kiemLoiMoi({ email: "abc", vai_tro: "can_bo" }));
  assert.equal(laMaMoi("A".repeat(43)), true);
  assert.equal(laMaMoi("A".repeat(42)), false);
  assert.equal(laMaMoi("A".repeat(42) + "="), false);
});

test("kiemGiaoViec: tạo cần người nhận + vai; sửa không nhận khóa lạ; docLocViec lọc trạng thái", () => {
  const U = "11111111-2222-4333-8444-555555555555";
  assert.deepEqual(dl(kiemGiaoViec({ nguoi_nhan: U.toUpperCase(), vai: "chu_tri" }, "tao")), { nguoi_nhan: U, vai: "chu_tri" });
  assert.equal(dl(kiemGiaoViec({ nguoi_nhan: U, vai: "phoi_hop", han_xu_ly: null }, "tao")).han_xu_ly, null);
  assert.ok("loi" in kiemGiaoViec({ vai: "chu_tri" }, "tao"));
  assert.ok("loi" in kiemGiaoViec({ trang_thai: "huy" }, "sua"));
  assert.ok("loi" in kiemGiaoViec({}, "sua"));
  assert.deepEqual(dl(kiemGiaoViec({ trang_thai: "dang", ghi_chu: " Đang làm " }, "sua")), { trang_thai: "dang", ghi_chu: "Đang làm" });
  assert.deepEqual(dl(docLocViec(new URLSearchParams("trang_thai=cho,dang&qua_han=1"))),
    { trang_thai: ["cho", "dang"], qua_han: true, trang: 1, so_dong: 20 });
  assert.ok("loi" in docLocViec(new URLSearchParams("trang_thai=cho,xoa")));
});
```

Thêm vào `tests/to-chuc-dich-vu.test.ts`:

```ts
import { boThanhVien, docToChuc, dsGoiToChuc, dsThanhVien, dsToChuc, roiToChuc, suaThanhVien, suaToChuc, taoToChuc }
  from "../src/lib/to-chuc/to-chuc";

describe("dịch vụ tổ chức + thành viên", { skip: !CO_CSDL_THU }, () => {
  let csdl: Awaited<ReturnType<typeof taoCsdlThu>>;
  let c: Client;
  let pt: { rpc: ReturnType<typeof rpcPg> };

  before(async () => {
    csdl = await taoCsdlThu();
    c = new Client({ connectionString: csdl.url });
    await c.connect();
    pt = { rpc: rpcPg(c) };
  });
  after(async () => {
    await c?.end();
    await csdl?.xoa();
  });

  test("tạo → danh sách → đọc → sửa hồ sơ + Cơ quan; đầu vào sai 400; id rác 404", async () => {
    const u = await taoNguoiDung(c);
    const t = du(await taoToChuc(pt, u, { ten: "UBND xã Gia Kiệm", mst: "3600123456" }));
    assert.equal(t.vai_tro, "quan_tri");
    assert.equal(du(await dsToChuc(pt, u)).length, 1);
    assert.equal(ma(await taoToChuc(pt, u, { ten: "" })), "400 DAU_VAO_SAI");
    assert.equal(ma(await docToChuc(pt, u, "khong-phai-uuid")), "404 KHONG_TIM_THAY");
    const doc = du(await docToChuc(pt, u, t.id));
    assert.equal(doc.mst, "3600123456");
    const s = du(await suaToChuc(pt, u, t.id, { phien_ban: doc.phien_ban, dia_chi: "Ấp 1", co_quan: CQ }));
    assert.deepEqual([s.dia_chi, s.co_quan.ten_co_quan], ["Ấp 1", CQ.ten_co_quan]);
    assert.equal(ma(await suaToChuc(pt, u, t.id, { dia_chi: "x" })), "400 DAU_VAO_SAI"); // thiếu phiên bản
    assert.equal(ma(await suaToChuc(pt, u, t.id, { phien_ban: 1, dia_chi: "x" })), "409 XUNG_DOT_PHIEN_BAN");
    assert.equal(ma(await docToChuc(pt, await taoNguoiDung(c), t.id)), "404 KHONG_TIM_THAY");
  });

  test("thành viên: cán bộ không đổi vai trò; quản trị cuối không rời; tự rời được", async () => {
    const b = await boToChuc(c);
    assert.equal(du(await dsThanhVien(pt, b.canBo, b.toChuc)).length, 4);
    assert.equal(ma(await suaThanhVien(pt, b.canBo, b.toChuc, b.canBo, { vai_tro: "quan_tri" })), "403 KHONG_DU_QUYEN");
    assert.equal(ma(await suaThanhVien(pt, b.quanTri, b.toChuc, b.canBo, { vai_tro: "chu" })), "400 DAU_VAO_SAI");
    assert.equal(du(await suaThanhVien(pt, b.quanTri, b.toChuc, b.canBo, { vai_tro: "van_thu" })).vai_tro, "van_thu");
    assert.equal(ma(await roiToChuc(pt, b.quanTri, b.toChuc)), "409 QUAN_TRI_CUOI");
    assert.equal(ma(await boThanhVien(pt, b.canBo2, b.toChuc, b.vanThu)), "403 KHONG_DU_QUYEN");
    assert.equal((await roiToChuc(pt, b.canBo2, b.toChuc)).loai, "xong");
    assert.equal(ma(await dsThanhVien(pt, b.canBo2, b.toChuc)), "404 KHONG_TIM_THAY");
  });

  test("gói tổ chức: đủ ba bậc", async () => {
    assert.deepEqual(du(await dsGoiToChuc(pt)).map((g: any) => g.ma), ["tc-10", "tc-30", "tc-100"]);
  });
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx tsx --test tests/to-chuc-thuan.test.ts tests/to-chuc-dich-vu.test.ts`
Expected: FAIL — không có `src/lib/to-chuc/kiem-dau-vao` / `to-chuc`.

- [ ] **Step 3: Viết `src/lib/to-chuc/kiem-dau-vao.ts`**

```ts
/** Kiểm + chuẩn hóa đầu vào API tổ chức, lời mời, giao việc (hàm thuần). Dùng chung khuôn bat / sai của kho. */
import { kiemCoQuan } from "../kho/co-quan";
import { VAI_TRO, type VaiTro } from "../kho/chu-so-huu";
import { bat, chuoiTuy, laUuid, motTrong, ngay, sai, soThamSo, type KQ } from "../kho/kiem-dau-vao";

const EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
/** Gộp mọi khoảng trắng / xuống dòng thành một dấu cách (tên tổ chức đi vào tiêu đề thư: không được có xuống dòng). */
const motDong = (s: string | null): string | null => (s === null ? null : s.replace(/\s+/g, " ").trim() || null);

export function kiemToChuc(than: Record<string, unknown>, cheDo: "tao" | "sua"): KQ<Record<string, unknown>> {
  return bat(() => {
    const d: Record<string, unknown> = {};
    const co = (k: string) => Object.hasOwn(than, k);
    if (cheDo === "tao" || co("ten")) {
      const t = motDong(chuoiTuy(than.ten, "Tên tổ chức", 200));
      if (!t) sai("Thiếu tên tổ chức.");
      d.ten = t;
    }
    if (co("mst")) {
      const m = motDong(chuoiTuy(than.mst, "Mã số thuế", 14));
      if (m !== null && !/^\d{10}(-\d{3})?$/.test(m)) sai("Mã số thuế gồm 10 chữ số; chi nhánh thêm «-» và 3 chữ số.");
      d.mst = m;
    }
    if (co("dia_chi")) d.dia_chi = motDong(chuoiTuy(than.dia_chi, "Địa chỉ", 300));
    if (co("email_hoa_don")) {
      const e = motDong(chuoiTuy(than.email_hoa_don, "Email nhận hóa đơn", 254));
      if (e !== null && !EMAIL.test(e)) sai("Email nhận hóa đơn không hợp lệ.");
      d.email_hoa_don = e === null ? null : e.toLowerCase();
    }
    if (cheDo === "sua" && co("co_quan")) {
      const cq = than.co_quan;
      if (typeof cq !== "object" || cq === null || Array.isArray(cq)) sai("Thông tin cơ quan không hợp lệ.");
      const k = kiemCoQuan(cq as Record<string, unknown>);
      if ("loi" in k) sai(k.loi);
      d.co_quan = (k as { du_lieu: unknown }).du_lieu;
    }
    if (cheDo === "sua" && Object.keys(d).length === 0) sai("Không có trường nào để sửa.");
    return d;
  });
}

export function kiemLoiMoi(than: Record<string, unknown>): KQ<{ email: string; vai_tro: VaiTro }> {
  return bat(() => {
    const e = chuoiTuy(than.email, "Email", 254);
    if (!e || !EMAIL.test(e)) sai("Email không hợp lệ.");
    return { email: (e as string).toLowerCase(), vai_tro: motTrong(than.vai_tro, "Vai trò", VAI_TRO) };
  });
}

/** Mã mời: 32 byte ngẫu nhiên, base64url không đệm = đúng 43 ký tự. */
export const laMaMoi = (v: unknown): v is string => typeof v === "string" && /^[A-Za-z0-9_-]{43}$/.test(v);

const VAI_XU_LY = ["chu_tri", "phoi_hop"] as const;
const TRANG_THAI_VIEC = ["cho", "dang", "xong"] as const;

export function kiemGiaoViec(than: Record<string, unknown>, cheDo: "tao" | "sua"): KQ<Record<string, unknown>> {
  return bat(() => {
    const d: Record<string, unknown> = {};
    const co = (k: string) => Object.hasOwn(than, k);
    if (cheDo === "tao" || co("nguoi_nhan")) {
      if (!laUuid(than.nguoi_nhan)) sai("Người nhận không hợp lệ.");
      d.nguoi_nhan = (than.nguoi_nhan as string).toLowerCase();
    }
    if (cheDo === "tao" || co("vai")) d.vai = motTrong(than.vai, "Vai trò xử lý", VAI_XU_LY);
    if (co("han_xu_ly")) d.han_xu_ly = ngay(than.han_xu_ly, "Hạn xử lý");
    if (co("y_kien_chi_dao")) d.y_kien_chi_dao = chuoiTuy(than.y_kien_chi_dao, "Ý kiến chỉ đạo", 2000);
    if (cheDo === "sua") {
      if (co("trang_thai")) d.trang_thai = motTrong(than.trang_thai, "Trạng thái", TRANG_THAI_VIEC);
      if (co("ghi_chu")) d.ghi_chu = chuoiTuy(than.ghi_chu, "Ghi chú", 2000);
      if (Object.keys(d).length === 0) sai("Không có trường nào để sửa.");
    }
    return d;
  });
}

export function docLocViec(sp: URLSearchParams): KQ<Record<string, unknown>> {
  return bat(() => {
    const d: Record<string, unknown> = {};
    const tt = sp.get("trang_thai");
    if (tt) {
      const ds = [...new Set(tt.split(","))];
      for (const t of ds) motTrong(t, "Trạng thái", TRANG_THAI_VIEC);
      d.trang_thai = ds;
    }
    if (sp.get("qua_han") === "1") d.qua_han = true;
    d.trang = soThamSo(sp.get("trang"), "Trang", 1, 10000, 1);
    d.so_dong = soThamSo(sp.get("so_dong"), "Số dòng", 1, 100, 20);
    return d;
  });
}
```

- [ ] **Step 4: Viết `src/lib/to-chuc/to-chuc.ts`**

```ts
import { quaNhieu, type KetQua } from "../phien/dich-vu";
import type { GoiRpc } from "../phien/rpc";
import { VAI_TRO } from "../kho/chu-so-huu";
import { laUuid } from "../kho/kiem-dau-vao";
import { LOI_KHO, tuKiem, xong } from "../kho/loi";
import { kiemToChuc } from "./kiem-dau-vao";
import { tuKetQuaToChuc } from "./loi";

export type CoRpc = { rpc: GoiRpc };

/** Tạo tổ chức: 10 lần / giờ / người (ngoài trần 3 tổ chức chưa có gói của CSDL). */
export const TAO_TOI_DA = 10;
export const TAO_CUA_SO_GIAY = 3600;

const id_ = (id: string) => id.toLowerCase();

export async function dsToChuc(pt: CoRpc, userId: string): Promise<KetQua> {
  return xong(await pt.rpc("to_chuc_ds", { p_user: userId }));
}

export async function taoToChuc(pt: CoRpc, userId: string, than: Record<string, unknown>): Promise<KetQua> {
  const k = kiemToChuc(than, "tao");
  if ("loi" in k) return tuKiem(k);
  const d = await pt.rpc("tan_suat_dem", { p_khoa: `to-chuc-tao:${userId}`, p_cua_so_giay: TAO_CUA_SO_GIAY, p_toi_da: TAO_TOI_DA });
  if (!d?.duoc) return quaNhieu(d?.thu_lai_sau_giay);
  return tuKetQuaToChuc(await pt.rpc("to_chuc_tao", { p_user: userId, p_du_lieu: k.du_lieu }), "to_chuc", "Đã tạo tổ chức.");
}

export async function docToChuc(pt: CoRpc, userId: string, id: string): Promise<KetQua> {
  if (!laUuid(id)) return LOI_KHO.khongTimThay();
  const t = await pt.rpc("to_chuc_doc", { p_to_chuc: id_(id), p_user: userId });
  return t ? xong(t) : LOI_KHO.khongTimThay();
}

export async function suaToChuc(pt: CoRpc, userId: string, id: string, than: Record<string, unknown>): Promise<KetQua> {
  if (!laUuid(id)) return LOI_KHO.khongTimThay();
  const { phien_ban: pb, ...con } = than;
  if (typeof pb !== "number" || !Number.isInteger(pb) || pb < 1) {
    return LOI_KHO.dauVaoSai("Thiếu phiên bản (phien_ban) của hồ sơ tổ chức.");
  }
  const k = kiemToChuc(con, "sua");
  if ("loi" in k) return tuKiem(k);
  return tuKetQuaToChuc(
    await pt.rpc("to_chuc_sua", { p_to_chuc: id_(id), p_user: userId, p_phien_ban: pb, p_du_lieu: k.du_lieu }),
    "to_chuc", "Đã lưu hồ sơ tổ chức."
  );
}

export async function dsThanhVien(pt: CoRpc, userId: string, id: string): Promise<KetQua> {
  if (!laUuid(id)) return LOI_KHO.khongTimThay();
  const ds = await pt.rpc("to_chuc_thanh_vien_ds", { p_to_chuc: id_(id), p_user: userId });
  return ds ? xong(ds) : LOI_KHO.khongTimThay();
}

export async function suaThanhVien(pt: CoRpc, userId: string, id: string, uid: string, than: Record<string, unknown>): Promise<KetQua> {
  if (!laUuid(id) || !laUuid(uid)) return LOI_KHO.khongTimThay();
  if (typeof than.vai_tro !== "string" || !(VAI_TRO as readonly string[]).includes(than.vai_tro)) {
    return LOI_KHO.dauVaoSai("Vai trò không hợp lệ.");
  }
  return tuKetQuaToChuc(
    await pt.rpc("to_chuc_thanh_vien_sua", { p_to_chuc: id_(id), p_user: userId, p_muc_tieu: id_(uid), p_vai_tro: than.vai_tro }),
    "thanh_vien", "Đã đổi vai trò."
  );
}

export async function boThanhVien(pt: CoRpc, userId: string, id: string, uid: string): Promise<KetQua> {
  if (!laUuid(id) || !laUuid(uid)) return LOI_KHO.khongTimThay();
  return tuKetQuaToChuc(
    await pt.rpc("to_chuc_thanh_vien_bo", { p_to_chuc: id_(id), p_user: userId, p_muc_tieu: id_(uid) }),
    null, uid.toLowerCase() === userId ? "Đã rời tổ chức." : "Đã bỏ thành viên khỏi tổ chức."
  );
}

export const roiToChuc = (pt: CoRpc, userId: string, id: string) => boThanhVien(pt, userId, id, userId);

export async function dsGoiToChuc(pt: CoRpc): Promise<KetQua> {
  return xong(await pt.rpc("goi_to_chuc_ds", {}));
}
```

- [ ] **Step 5: Viết route**

`src/app/api/v2/to-chuc/route.ts`:

```ts
import { NextRequest, NextResponse } from "next/server";
import { phuThuocKhoThat as pt } from "@lib/kho/phu-thuoc";
import { canTaiKhoan, chay, docThan, traLoi } from "@lib/kho/tra-loi";
import { dsToChuc, taoToChuc } from "@lib/to-chuc/to-chuc";

export const dynamic = "force-dynamic";

/** Tổ chức của người đang đăng nhập (bỏ qua header ngữ cảnh). */
export async function GET(req: NextRequest) {
  return chay(async () => {
    const a = await canTaiKhoan(req);
    if (a instanceof NextResponse) return a;
    return traLoi(await dsToChuc(pt, a.user.id));
  });
}

export async function POST(req: NextRequest) {
  return chay(async () => {
    const a = await canTaiKhoan(req);
    if (a instanceof NextResponse) return a;
    return traLoi(await taoToChuc(pt, a.user.id, await docThan(req)));
  });
}
```

`src/app/api/v2/to-chuc/[id]/route.ts`:

```ts
import { NextRequest, NextResponse } from "next/server";
import { phuThuocKhoThat as pt } from "@lib/kho/phu-thuoc";
import { LOI_KHO } from "@lib/kho/loi";
import { canTaiKhoan, chay, traLoi } from "@lib/kho/tra-loi";
import { docJsonCoTran, TRAN_THAN_CO_QUAN, traLoiQuaLon } from "@lib/tac-vu/doc-than";
import { docToChuc, suaToChuc } from "@lib/to-chuc/to-chuc";

export const dynamic = "force-dynamic";
type NgCanh = { params: Promise<{ id: string }> };

export async function GET(req: NextRequest, { params }: NgCanh) {
  return chay(async () => {
    const a = await canTaiKhoan(req);
    if (a instanceof NextResponse) return a;
    return traLoi(await docToChuc(pt, a.user.id, (await params).id));
  });
}

/** Sửa hồ sơ + Cơ quan (quản trị). Thân có trần 16 KB; JSON hỏng / không phải đối tượng → 400 (không coi là «{}»). */
export async function PUT(req: NextRequest, { params }: NgCanh) {
  return chay(async () => {
    const a = await canTaiKhoan(req);
    if (a instanceof NextResponse) return a;
    const doc = await docJsonCoTran(req, TRAN_THAN_CO_QUAN);
    if (doc.quaTran) return traLoiQuaLon(TRAN_THAN_CO_QUAN);
    const than = doc.duLieu;
    if (typeof than !== "object" || than === null || Array.isArray(than)) {
      return traLoi(LOI_KHO.dauVaoSai("Thông tin tổ chức không hợp lệ."));
    }
    return traLoi(await suaToChuc(pt, a.user.id, (await params).id, than as Record<string, unknown>));
  });
}
```

`src/app/api/v2/to-chuc/[id]/thanh-vien/route.ts`:

```ts
import { NextRequest, NextResponse } from "next/server";
import { phuThuocKhoThat as pt } from "@lib/kho/phu-thuoc";
import { canTaiKhoan, chay, traLoi } from "@lib/kho/tra-loi";
import { dsThanhVien } from "@lib/to-chuc/to-chuc";

export const dynamic = "force-dynamic";

export async function GET(req: NextRequest, { params }: { params: Promise<{ id: string }> }) {
  return chay(async () => {
    const a = await canTaiKhoan(req);
    if (a instanceof NextResponse) return a;
    return traLoi(await dsThanhVien(pt, a.user.id, (await params).id));
  });
}
```

`src/app/api/v2/to-chuc/[id]/thanh-vien/[uid]/route.ts`:

```ts
import { NextRequest, NextResponse } from "next/server";
import { phuThuocKhoThat as pt } from "@lib/kho/phu-thuoc";
import { canTaiKhoan, chay, docThan, traLoi } from "@lib/kho/tra-loi";
import { boThanhVien, suaThanhVien } from "@lib/to-chuc/to-chuc";

export const dynamic = "force-dynamic";
type NgCanh = { params: Promise<{ id: string; uid: string }> };

export async function PATCH(req: NextRequest, { params }: NgCanh) {
  return chay(async () => {
    const a = await canTaiKhoan(req);
    if (a instanceof NextResponse) return a;
    const { id, uid } = await params;
    return traLoi(await suaThanhVien(pt, a.user.id, id, uid, await docThan(req)));
  });
}

export async function DELETE(req: NextRequest, { params }: NgCanh) {
  return chay(async () => {
    const a = await canTaiKhoan(req);
    if (a instanceof NextResponse) return a;
    const { id, uid } = await params;
    return traLoi(await boThanhVien(pt, a.user.id, id, uid));
  });
}
```

`src/app/api/v2/to-chuc/[id]/roi/route.ts`:

```ts
import { NextRequest, NextResponse } from "next/server";
import { phuThuocKhoThat as pt } from "@lib/kho/phu-thuoc";
import { canTaiKhoan, chay, traLoi } from "@lib/kho/tra-loi";
import { roiToChuc } from "@lib/to-chuc/to-chuc";

export const dynamic = "force-dynamic";

export async function POST(req: NextRequest, { params }: { params: Promise<{ id: string }> }) {
  return chay(async () => {
    const a = await canTaiKhoan(req);
    if (a instanceof NextResponse) return a;
    return traLoi(await roiToChuc(pt, a.user.id, (await params).id));
  });
}
```

`src/app/api/v2/goi-to-chuc/route.ts`:

```ts
import { NextResponse } from "next/server";
import { rpcSupabase } from "@lib/phien/rpc";

export const dynamic = "force-dynamic";

/** Bảng gói tổ chức (công khai; cả gói chưa bán, cờ dang_ban). Thành công cache 60 giây, lỗi không cache. */
export async function GET() {
  try {
    const ds = await rpcSupabase("goi_to_chuc_ds", {});
    return NextResponse.json({ success: true, message: "Thành công", data: ds }, { headers: { "Cache-Control": "public, max-age=60" } });
  } catch (e) {
    console.error("[GOI-TO-CHUC] đọc bảng gói lỗi", (e as Error).message);
    return NextResponse.json({ success: false, message: "Không đọc được bảng gói tổ chức.", code: "LOI_MAY_CHU" },
      { status: 500, headers: { "Cache-Control": "no-store" } });
  }
}
```

- [ ] **Step 6: Chạy kiểm**

Run: `npx tsx --test tests/to-chuc-thuan.test.ts tests/to-chuc-dich-vu.test.ts` rồi `npx tsc --noEmit`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add src/lib/to-chuc src/app/api/v2/to-chuc src/app/api/v2/goi-to-chuc tests/to-chuc-thuan.test.ts tests/to-chuc-dich-vu.test.ts
git commit -m "feat(to-chuc): API to chuc, thanh vien, roi to chuc, bang goi to chuc

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 12: Lời mời — mã, thư, giới hạn tốc độ, route

**Files:**
- Create: `src/lib/to-chuc/phu-thuoc.ts`, `src/lib/to-chuc/loi-moi.ts`
- Create: `src/app/api/v2/to-chuc/[id]/loi-moi/route.ts`, `src/app/api/v2/to-chuc/[id]/loi-moi/[lid]/route.ts`,
  `src/app/api/v2/loi-moi/nhan/route.ts`
- Test: `tests/to-chuc-dich-vu.test.ts` (thêm `describe`)

**Interfaces:**
- Consumes: Task 2 (`to_chuc_loi_moi_*`, `to_chuc_vai_tro`), Task 11 (`kiemLoiMoi`, `laMaMoi`), `taoTokenNgauNhien`, `bamHex`
  (`src/lib/phien/ma.ts`), `guiEmail` (`src/lib/email/gui.ts`), `tan_suat_dem` / `tan_suat_xem` (009).
- Produces: `type PhuThuocToChuc = { rpc; guiEmail(t: ThuEmail): Promise<void>; gocWeb(): string; ghiLog(s): void }`,
  `phuThuocToChucThat`; `thuMoi(d): ThuEmail`; `taoLoiMoi(pt, userId, id, than)`, `dsLoiMoi(pt, userId, id)`,
  `huyLoiMoi(pt, userId, id, lid)`, `nhanLoiMoi(pt, userId, than)` — `Promise<KetQua>`; hằng `MOI_TOI_DA = 30`,
  `NHAN_SAI_TOI_DA = 10`, cửa sổ 3600 giây.

- [ ] **Step 1: Viết bài kiểm (thêm vào `tests/to-chuc-dich-vu.test.ts`)**

```ts
import type { ThuEmail } from "../src/lib/email/gui";
import type { PhuThuocToChuc } from "../src/lib/to-chuc/phu-thuoc";
import { dsLoiMoi, huyLoiMoi, MOI_TOI_DA, NHAN_SAI_TOI_DA, nhanLoiMoi, taoLoiMoi } from "../src/lib/to-chuc/loi-moi";
import { nguoiXacMinh } from "./ho-tro/to-chuc";

describe("dịch vụ lời mời", { skip: !CO_CSDL_THU }, () => {
  let csdl: Awaited<ReturnType<typeof taoCsdlThu>>;
  let c: Client;
  let thu: ThuEmail[];
  let hongThu = false;
  let pt: PhuThuocToChuc;
  const maTrong = (t: ThuEmail) => /\/loi-moi\?ma=([A-Za-z0-9_-]{43})/.exec(t.van_ban)![1];

  before(async () => {
    csdl = await taoCsdlThu();
    c = new Client({ connectionString: csdl.url });
    await c.connect();
    thu = [];
    pt = {
      rpc: rpcPg(c),
      guiEmail: async (t) => { if (hongThu) throw new Error("Gmail 500"); thu.push(t); },
      gocWeb: () => "https://app.thu",
      ghiLog: () => {},
    };
  });
  after(async () => {
    await c?.end();
    await csdl?.xoa();
  });

  test("mời → thư có đường nhận, không lộ mã trong JSON → nhận đúng email → thành viên", async () => {
    const b = await boToChuc(c);
    const k = await nguoiXacMinh(c);
    const r = du(await taoLoiMoi(pt, b.quanTri, b.toChuc, { email: k.email.toUpperCase(), vai_tro: "van_thu" }));
    assert.deepEqual(Object.keys(r).sort(), ["email", "het_han", "id", "tao_luc", "trang_thai", "vai_tro"]); // không có mã
    const t = thu.at(-1)!;
    assert.equal(t.den, k.email);
    assert.match(t.tieu_de, /^Lời mời tham gia «UBND xã Thử» trên VanBanPlus$/);
    assert.match(t.van_ban, /vai trò Văn thư/);
    const cam = /(^|[^\p{L}])(tôi|chúng tôi|ta|anh|chị|em|bạn|mình|dạ|vâng|ạ)(?=$|[^\p{L}])/iu;
    assert.equal(cam.test(t.tieu_de + "\n" + t.van_ban), false, t.van_ban);
    const n = du(await nhanLoiMoi(pt, k.id, { ma: maTrong(t) }));
    assert.deepEqual([n.to_chuc.id, n.to_chuc.vai_tro], [b.toChuc, "van_thu"]);
    assert.equal(du(await dsLoiMoi(pt, b.quanTri, b.toChuc))[0].trang_thai, "da_dung");
  });

  test("cán bộ mời / xem / hủy lời mời → 403, không tốn lượt của tổ chức", async () => {
    const b = await boToChuc(c);
    assert.equal(ma(await taoLoiMoi(pt, b.canBo, b.toChuc, { email: "x@y.vn", vai_tro: "can_bo" })), "403 KHONG_DU_QUYEN");
    assert.equal(ma(await dsLoiMoi(pt, b.canBo, b.toChuc)), "403 KHONG_DU_QUYEN");
    const { rows } = await c.query(`SELECT count(*)::int AS n FROM dem_tan_suat WHERE khoa = $1`, [`to-chuc-moi:${b.toChuc}`]);
    assert.equal(rows[0].n, 0);
  });

  test("gửi thư hỏng → 503, lời mời bị hủy (mã đã mất)", async () => {
    const b = await boToChuc(c);
    hongThu = true;
    try {
      assert.equal(ma(await taoLoiMoi(pt, b.quanTri, b.toChuc, { email: "hong@vanbanplus.test", vai_tro: "can_bo" })), "503 CHUA_GUI_DUOC_THU");
    } finally {
      hongThu = false;
    }
    assert.equal(du(await dsLoiMoi(pt, b.quanTri, b.toChuc))[0].trang_thai, "da_huy");
  });

  test(`quá ${MOI_TOI_DA} lời mời / giờ / tổ chức → 429`, async () => {
    const b = await boToChuc(c);
    await c.query(`UPDATE goi_to_chuc SET so_thanh_vien_toi_da = 1000 WHERE ma = 'tc-10'`);
    try {
      for (let i = 0; i < MOI_TOI_DA; i++) {
        du(await taoLoiMoi(pt, b.quanTri, b.toChuc, { email: `moi${i}@vanbanplus.test`, vai_tro: "can_bo" }));
      }
      assert.equal(ma(await taoLoiMoi(pt, b.quanTri, b.toChuc, { email: "them@vanbanplus.test", vai_tro: "can_bo" })), "429 QUA_NHIEU_YEU_CAU");
    } finally {
      await c.query(`UPDATE goi_to_chuc SET so_thanh_vien_toi_da = 10 WHERE ma = 'tc-10'`);
    }
  });

  test(`đoán mã: ${NHAN_SAI_TOI_DA} lần sai / giờ thì chặn cả mã đúng; mã sai dạng cũng tính`, async () => {
    const b = await boToChuc(c);
    const k = await nguoiXacMinh(c);
    du(await taoLoiMoi(pt, b.quanTri, b.toChuc, { email: k.email, vai_tro: "can_bo" }));
    const dung = maTrong(thu.at(-1)!);
    assert.equal(ma(await nhanLoiMoi(pt, k.id, { ma: "ngan" })), "400 LOI_MOI_KHONG_HOP_LE");
    for (let i = 1; i < NHAN_SAI_TOI_DA; i++) {
      assert.equal(ma(await nhanLoiMoi(pt, k.id, { ma: "B".repeat(42) + String(i % 10) })), "400 LOI_MOI_KHONG_HOP_LE");
    }
    assert.equal(ma(await nhanLoiMoi(pt, k.id, { ma: dung })), "429 QUA_NHIEU_YEU_CAU");
  });

  test("nhận bằng tài khoản email khác → 403 SAI_EMAIL_MOI; hủy rồi thì không nhận được", async () => {
    const b = await boToChuc(c);
    const k = await nguoiXacMinh(c);
    const l = du(await taoLoiMoi(pt, b.quanTri, b.toChuc, { email: k.email, vai_tro: "can_bo" }));
    const m = maTrong(thu.at(-1)!);
    const khac = await nguoiXacMinh(c);
    assert.equal(ma(await nhanLoiMoi(pt, khac.id, { ma: m })), "403 SAI_EMAIL_MOI");
    assert.equal((await huyLoiMoi(pt, b.quanTri, b.toChuc, l.id)).loai, "xong");
    assert.equal(ma(await nhanLoiMoi(pt, k.id, { ma: m })), "400 LOI_MOI_KHONG_HOP_LE");
  });
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx tsx --test tests/to-chuc-dich-vu.test.ts`
Expected: FAIL — không có `src/lib/to-chuc/loi-moi`.

- [ ] **Step 3: Viết `src/lib/to-chuc/phu-thuoc.ts` và `src/lib/to-chuc/loi-moi.ts`**

```ts
// src/lib/to-chuc/phu-thuoc.ts
import { guiEmail, type ThuEmail } from "../email/gui";
import { rpcSupabase, type GoiRpc } from "../phien/rpc";

export type PhuThuocToChuc = {
  rpc: GoiRpc;
  guiEmail: (t: ThuEmail) => Promise<void>;
  /** Gốc web cho đường nhận lời mời (không dấu / cuối). */
  gocWeb: () => string;
  ghiLog: (s: string) => void;
};

export const phuThuocToChucThat: PhuThuocToChuc = {
  rpc: rpcSupabase,
  guiEmail,
  gocWeb: () => (process.env.VBP_WEB_GOC || "https://app.vanbanplus.giakiemso.com").replace(/\/+$/, ""),
  ghiLog: (s) => console.error(s),
};
```

```ts
// src/lib/to-chuc/loi-moi.ts
import type { ThuEmail } from "../email/gui";
import { quaNhieu, type KetQua } from "../phien/dich-vu";
import { bamHex, taoTokenNgauNhien } from "../phien/ma";
import type { VaiTro } from "../kho/chu-so-huu";
import { laUuid } from "../kho/kiem-dau-vao";
import { LOI_KHO, tuKiem, xong } from "../kho/loi";
import { kiemLoiMoi, laMaMoi } from "./kiem-dau-vao";
import { LOI_TO_CHUC, tuKetQuaToChuc, tuLoiSqlToChuc } from "./loi";
import type { PhuThuocToChuc } from "./phu-thuoc";

export const MOI_TOI_DA = 30;
export const MOI_CUA_SO_GIAY = 3600;
export const NHAN_SAI_TOI_DA = 10;
export const NHAN_CUA_SO_GIAY = 3600;

const TEN_VAI: Record<VaiTro, string> = { quan_tri: "Quản trị", van_thu: "Văn thư", can_bo: "Cán bộ" };

/** Thư mời: câu vô nhân xưng, tên tổ chức một dòng (đã gộp khoảng trắng ở kiemToChuc; gộp lại cho chắc). */
export function thuMoi(d: { den: string; tenToChuc: string; vaiTro: VaiTro; duongDan: string }): ThuEmail {
  const ten = d.tenToChuc.replace(/\s+/g, " ").trim();
  return {
    den: d.den,
    tieu_de: `Lời mời tham gia «${ten}» trên VanBanPlus`,
    van_ban: [
      `Tài khoản ${d.den} được mời tham gia tổ chức «${ten}» trên VanBanPlus với vai trò ${TEN_VAI[d.vaiTro]}.`,
      "",
      "Mở đường dẫn sau để nhận lời mời (hiệu lực 7 ngày):",
      d.duongDan,
      "",
      "Cần đăng nhập đúng email được mời. Không chờ lời mời này thì bỏ qua thư.",
    ].join("\n"),
  };
}

/** Chỉ quản trị: kiểm vai trò TRƯỚC khi đếm tần suất, để thành viên khác không đốt hết lượt mời của tổ chức. */
async function kiemQuanTri(pt: PhuThuocToChuc, userId: string, toChuc: string): Promise<KetQua | null> {
  const vai = await pt.rpc<string | null>("to_chuc_vai_tro", { p_to_chuc: toChuc, p_user: userId });
  if (!vai) return LOI_KHO.khongTimThay();
  return vai === "quan_tri" ? null : LOI_TO_CHUC.khongDuQuyen();
}

export async function taoLoiMoi(pt: PhuThuocToChuc, userId: string, id: string, than: Record<string, unknown>): Promise<KetQua> {
  if (!laUuid(id)) return LOI_KHO.khongTimThay();
  const toChuc = id.toLowerCase();
  const k = kiemLoiMoi(than);
  if ("loi" in k) return tuKiem(k);
  const chan = await kiemQuanTri(pt, userId, toChuc);
  if (chan) return chan;
  const dem = await pt.rpc("tan_suat_dem", { p_khoa: `to-chuc-moi:${toChuc}`, p_cua_so_giay: MOI_CUA_SO_GIAY, p_toi_da: MOI_TOI_DA });
  if (!dem?.duoc) return quaNhieu(dem?.thu_lai_sau_giay);
  const ma = taoTokenNgauNhien(32);
  const r = await pt.rpc("to_chuc_loi_moi_tao", {
    p_to_chuc: toChuc, p_user: userId, p_email: k.du_lieu.email, p_vai_tro: k.du_lieu.vai_tro, p_ma_bam: bamHex(ma),
  });
  if (r?.loi) return tuLoiSqlToChuc(r);
  try {
    await pt.guiEmail(thuMoi({
      den: r.loi_moi.email, tenToChuc: r.ten_to_chuc, vaiTro: k.du_lieu.vai_tro,
      duongDan: `${pt.gocWeb()}/loi-moi?ma=${ma}`,
    }));
  } catch (e) {
    // Mã chỉ có trong thư: thư không đi thì lời mời vô dụng → hủy ngay để danh sách không có lời mời «chờ» không ai nhận được.
    pt.ghiLog(`[TO-CHUC] gửi thư mời lỗi — ${(e as Error).message}`);
    await pt.rpc("to_chuc_loi_moi_huy", { p_to_chuc: toChuc, p_user: userId, p_id: r.loi_moi.id });
    return LOI_TO_CHUC.chuaGuiThu();
  }
  return xong(r.loi_moi, "Đã gửi lời mời.");
}

export async function dsLoiMoi(pt: PhuThuocToChuc, userId: string, id: string): Promise<KetQua> {
  if (!laUuid(id)) return LOI_KHO.khongTimThay();
  const chan = await kiemQuanTri(pt, userId, id.toLowerCase());
  if (chan) return chan;
  return xong(await pt.rpc("to_chuc_loi_moi_ds", { p_to_chuc: id.toLowerCase(), p_user: userId }));
}

export async function huyLoiMoi(pt: PhuThuocToChuc, userId: string, id: string, lid: string): Promise<KetQua> {
  if (!laUuid(id) || !laUuid(lid)) return LOI_KHO.khongTimThay();
  return tuKetQuaToChuc(
    await pt.rpc("to_chuc_loi_moi_huy", { p_to_chuc: id.toLowerCase(), p_user: userId, p_id: lid.toLowerCase() }),
    null, "Đã hủy lời mời."
  );
}

/** Nhận lời mời. Sai mã / sai email / mã sai dạng: đếm vào 10 lần / giờ / người; vượt thì chặn cả mã đúng tới hết giờ. */
export async function nhanLoiMoi(pt: PhuThuocToChuc, userId: string, than: Record<string, unknown>): Promise<KetQua> {
  const khoa = `loi-moi-sai:${userId}`;
  const xem = await pt.rpc("tan_suat_xem", { p_khoa: khoa, p_cua_so_giay: NHAN_CUA_SO_GIAY, p_toi_da: NHAN_SAI_TOI_DA });
  if (!xem?.duoc) return quaNhieu(xem?.thu_lai_sau_giay);
  const demSai = () => pt.rpc("tan_suat_dem", { p_khoa: khoa, p_cua_so_giay: NHAN_CUA_SO_GIAY, p_toi_da: NHAN_SAI_TOI_DA });
  if (!laMaMoi(than.ma)) {
    await demSai();
    return tuLoiSqlToChuc({ loi: "LOI_MOI_KHONG_HOP_LE" });
  }
  const r = await pt.rpc("to_chuc_loi_moi_nhan", { p_user: userId, p_ma_bam: bamHex(than.ma) });
  if (r?.loi) {
    if (r.loi === "LOI_MOI_KHONG_HOP_LE" || r.loi === "SAI_EMAIL_MOI") await demSai();
    return tuLoiSqlToChuc(r);
  }
  return xong({ to_chuc: r.to_chuc }, "Đã tham gia tổ chức.");
}
```

- [ ] **Step 4: Viết route**

`src/app/api/v2/to-chuc/[id]/loi-moi/route.ts`:

```ts
import { NextRequest, NextResponse } from "next/server";
import { canTaiKhoan, chay, docThan, traLoi } from "@lib/kho/tra-loi";
import { dsLoiMoi, taoLoiMoi } from "@lib/to-chuc/loi-moi";
import { phuThuocToChucThat as pt } from "@lib/to-chuc/phu-thuoc";

export const dynamic = "force-dynamic";
type NgCanh = { params: Promise<{ id: string }> };

export async function GET(req: NextRequest, { params }: NgCanh) {
  return chay(async () => {
    const a = await canTaiKhoan(req);
    if (a instanceof NextResponse) return a;
    return traLoi(await dsLoiMoi(pt, a.user.id, (await params).id));
  });
}

export async function POST(req: NextRequest, { params }: NgCanh) {
  return chay(async () => {
    const a = await canTaiKhoan(req);
    if (a instanceof NextResponse) return a;
    return traLoi(await taoLoiMoi(pt, a.user.id, (await params).id, await docThan(req)));
  });
}
```

`src/app/api/v2/to-chuc/[id]/loi-moi/[lid]/route.ts`:

```ts
import { NextRequest, NextResponse } from "next/server";
import { canTaiKhoan, chay, traLoi } from "@lib/kho/tra-loi";
import { huyLoiMoi } from "@lib/to-chuc/loi-moi";
import { phuThuocToChucThat as pt } from "@lib/to-chuc/phu-thuoc";

export const dynamic = "force-dynamic";

export async function DELETE(req: NextRequest, { params }: { params: Promise<{ id: string; lid: string }> }) {
  return chay(async () => {
    const a = await canTaiKhoan(req);
    if (a instanceof NextResponse) return a;
    const { id, lid } = await params;
    return traLoi(await huyLoiMoi(pt, a.user.id, id, lid));
  });
}
```

`src/app/api/v2/loi-moi/nhan/route.ts`:

```ts
import { NextRequest, NextResponse } from "next/server";
import { canTaiKhoan, chay, docThan, traLoi } from "@lib/kho/tra-loi";
import { nhanLoiMoi } from "@lib/to-chuc/loi-moi";
import { phuThuocToChucThat as pt } from "@lib/to-chuc/phu-thuoc";

export const dynamic = "force-dynamic";

/** Mã mời đi trong thân (không trong URL của API) — không rơi vào log truy cập. */
export async function POST(req: NextRequest) {
  return chay(async () => {
    const a = await canTaiKhoan(req);
    if (a instanceof NextResponse) return a;
    return traLoi(await nhanLoiMoi(pt, a.user.id, await docThan(req)));
  });
}
```

- [ ] **Step 5: Chạy kiểm**

Run: `npx tsx --test tests/to-chuc-dich-vu.test.ts` rồi `npx tsc --noEmit`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add src/lib/to-chuc src/app/api/v2/to-chuc src/app/api/v2/loi-moi tests/to-chuc-dich-vu.test.ts
git commit -m "feat(to-chuc): loi moi qua email - ma 32 byte chi luu SHA-256, gioi han moi 30/gio, sai ma 10/gio

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 13: Dịch vụ + route giao việc và «Việc được giao»

**Files:**
- Create: `src/lib/to-chuc/giao-viec.ts`
- Create: `src/app/api/v2/kho/van-ban/[id]/giao-viec/route.ts`, `src/app/api/v2/kho/giao-viec/[gid]/route.ts`,
  `src/app/api/v2/kho/giao-viec/cua-toi/route.ts`
- Test: `tests/to-chuc-dich-vu.test.ts` (thêm `describe`)

**Interfaces:**
- Consumes: Task 6 (hàm SQL `kho_giao_viec_*`), Task 11 (`kiemGiaoViec`, `docLocViec`), Task 9 (`canNguoiDung`, `Chu`).
- Produces: `dsGiaoViec(pt, chu, vanBanId)`, `taoGiaoViec(pt, chu, vanBanId, than)`, `suaGiaoViec(pt, chu, id, than)`,
  `thuHoiGiaoViec(pt, chu, id)`, `viecDuocGiao(pt, chu, sp)` — `Promise<KetQua>`; `chu.nguoi` bắt buộc ở kho tổ chức,
  kho cá nhân trả 404 / rỗng.

- [ ] **Step 1: Viết bài kiểm (thêm vào `tests/to-chuc-dich-vu.test.ts`)**

```ts
import { dsGiaoViec, suaGiaoViec, taoGiaoViec, thuHoiGiaoViec, viecDuocGiao } from "../src/lib/to-chuc/giao-viec";

describe("dịch vụ giao việc", { skip: !CO_CSDL_THU }, () => {
  let csdl: Awaited<ReturnType<typeof taoCsdlThu>>;
  let c: Client;
  let pt: PhuThuocKho;
  let b: Awaited<ReturnType<typeof boToChuc>>;
  const chu = (u: string, vai: VaiTro) => chuToChuc(b.toChuc, u, vai);

  before(async () => {
    csdl = await taoCsdlThu();
    c = new Client({ connectionString: csdl.url });
    await c.connect();
    pt = { rpc: rpcPg(c), tep: khoTepGia().k, bayGio: () => new Date(), ghiLog: () => {} };
    b = await boToChuc(c);
  });
  after(async () => {
    await c?.end();
    await csdl?.xoa();
  });

  test("một vòng: văn thư vào sổ → giao chủ trì → cán bộ thấy trong «Việc được giao» → cập nhật → nhật ký", async () => {
    const vt = chu(b.vanThu, "van_thu");
    const vb = du(await taoVanBan(pt, { id: b.vanThu }, vt, { huong: "den", trich_yeu: "Kế hoạch phòng cháy", han_xu_ly: "2099-12-01" }));
    const g = du(await taoGiaoViec(pt, vt, vb.id, { nguoi_nhan: b.canBo, vai: "chu_tri", y_kien_chi_dao: "Tham mưu trước 25/11" }));
    assert.equal(g.han_xu_ly, "2099-12-01");
    const cb = chu(b.canBo, "can_bo");
    const ds = du(await viecDuocGiao(pt, cb, new URLSearchParams("trang_thai=cho,dang")));
    assert.deepEqual([ds.tong, ds.muc[0].van_ban.trich_yeu], [1, "Kế hoạch phòng cháy"]);
    const s = du(await suaGiaoViec(pt, cb, g.id, { phien_ban: 1, trang_thai: "dang", ghi_chu: "Đã lấy ý kiến" }));
    assert.equal(s.trang_thai, "dang");
    const nk = du(await dsGiaoViec(pt, vt, vb.id)).nhat_ky;
    assert.deepEqual(nk.map((n: any) => n.hanh_dong), ["trang_thai", "giao"]);
  });

  test("đầu vào sai 400; thiếu phiên bản 400; cán bộ giao → 403; id rác 404; kho cá nhân 404 / rỗng", async () => {
    const vt = chu(b.vanThu, "van_thu");
    const vb = du(await taoVanBan(pt, { id: b.vanThu }, vt, { huong: "den", trich_yeu: "x" }));
    assert.equal(ma(await taoGiaoViec(pt, vt, vb.id, { vai: "chu_tri" })), "400 DAU_VAO_SAI");
    assert.equal(ma(await taoGiaoViec(pt, chu(b.canBo, "can_bo"), vb.id, { nguoi_nhan: b.canBo2, vai: "phoi_hop" })), "404 KHONG_TIM_THAY");
    const cb = await taoVanBan(pt, { id: b.canBo }, chu(b.canBo, "can_bo"), { huong: "den", trich_yeu: "Của cán bộ" });
    assert.equal(ma(await taoGiaoViec(pt, chu(b.canBo, "can_bo"), du(cb).id, { nguoi_nhan: b.canBo2, vai: "phoi_hop" })), "403 KHONG_DU_QUYEN");
    const g = du(await taoGiaoViec(pt, vt, vb.id, { nguoi_nhan: b.canBo2, vai: "phoi_hop" }));
    assert.equal(ma(await suaGiaoViec(pt, vt, g.id, { trang_thai: "xong" })), "400 DAU_VAO_SAI");
    assert.equal(ma(await suaGiaoViec(pt, vt, "khong-phai-id", { phien_ban: 1, trang_thai: "xong" })), "404 KHONG_TIM_THAY");
    assert.equal((await thuHoiGiaoViec(pt, vt, g.id)).loai, "xong");
    const caNhan = chuCuaNguoiDung({ id: b.vanThu });
    assert.equal(ma(await dsGiaoViec(pt, caNhan, vb.id)), "404 KHONG_TIM_THAY");
    assert.deepEqual(du(await viecDuocGiao(pt, caNhan, new URLSearchParams())), { muc: [], tong: 0 });
  });
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx tsx --test tests/to-chuc-dich-vu.test.ts`
Expected: FAIL — không có `src/lib/to-chuc/giao-viec`.

- [ ] **Step 3: Viết `src/lib/to-chuc/giao-viec.ts`**

```ts
import type { KetQua } from "../phien/dich-vu";
import { thamSoChu, type Chu } from "../kho/chu-so-huu";
import { laUuid } from "../kho/kiem-dau-vao";
import { LOI_KHO, tuKiem, xong } from "../kho/loi";
import type { PhuThuocKho } from "../kho/phu-thuoc";
import { docLocViec, kiemGiaoViec } from "./kiem-dau-vao";
import { tuKetQuaToChuc } from "./loi";

/** Người gọi của hàm giao việc: kho tổ chức có `nguoi`; kho cá nhân dùng chủ kho (SQL trả rỗng / không thấy). */
const thamSo = (chu: Chu) => ({ ...thamSoChu(chu), p_nguoi: chu.nguoi ?? chu.chu_id });

export async function dsGiaoViec(pt: PhuThuocKho, chu: Chu, vanBanId: string): Promise<KetQua> {
  if (!laUuid(vanBanId)) return LOI_KHO.khongTimThay();
  const r = await pt.rpc("kho_giao_viec_ds", { ...thamSo(chu), p_van_ban: vanBanId.toLowerCase() });
  return r ? xong(r) : LOI_KHO.khongTimThay();
}

export async function taoGiaoViec(pt: PhuThuocKho, chu: Chu, vanBanId: string, than: Record<string, unknown>): Promise<KetQua> {
  if (!laUuid(vanBanId)) return LOI_KHO.khongTimThay();
  const k = kiemGiaoViec(than, "tao");
  if ("loi" in k) return tuKiem(k);
  return tuKetQuaToChuc(
    await pt.rpc("kho_giao_viec_tao", { ...thamSo(chu), p_van_ban: vanBanId.toLowerCase(), p_du_lieu: k.du_lieu }),
    "giao_viec", "Đã giao xử lý."
  );
}

export async function suaGiaoViec(pt: PhuThuocKho, chu: Chu, id: string, than: Record<string, unknown>): Promise<KetQua> {
  if (!laUuid(id)) return LOI_KHO.khongTimThay();
  const { phien_ban: pb, ...con } = than;
  if (typeof pb !== "number" || !Number.isInteger(pb) || pb < 1) return LOI_KHO.dauVaoSai("Thiếu phiên bản (phien_ban) của việc.");
  const k = kiemGiaoViec(con, "sua");
  if ("loi" in k) return tuKiem(k);
  return tuKetQuaToChuc(
    await pt.rpc("kho_giao_viec_sua", { ...thamSo(chu), p_id: id.toLowerCase(), p_phien_ban: pb, p_du_lieu: k.du_lieu }),
    "giao_viec", "Đã cập nhật."
  );
}

export async function thuHoiGiaoViec(pt: PhuThuocKho, chu: Chu, id: string): Promise<KetQua> {
  if (!laUuid(id)) return LOI_KHO.khongTimThay();
  return tuKetQuaToChuc(await pt.rpc("kho_giao_viec_thu_hoi", { ...thamSo(chu), p_id: id.toLowerCase() }), null, "Đã thu hồi.");
}

export async function viecDuocGiao(pt: PhuThuocKho, chu: Chu, sp: URLSearchParams): Promise<KetQua> {
  const l = docLocViec(sp);
  if ("loi" in l) return tuKiem(l);
  return xong(await pt.rpc("kho_giao_viec_cua_toi", { ...thamSo(chu), p_loc: l.du_lieu }));
}
```

- [ ] **Step 4: Viết route**

`src/app/api/v2/kho/van-ban/[id]/giao-viec/route.ts`:

```ts
import { NextRequest, NextResponse } from "next/server";
import { phuThuocKhoThat as pt } from "@lib/kho/phu-thuoc";
import { canNguoiDung, chay, docThan, traLoi } from "@lib/kho/tra-loi";
import { dsGiaoViec, taoGiaoViec } from "@lib/to-chuc/giao-viec";

export const dynamic = "force-dynamic";
type NgCanh = { params: Promise<{ id: string }> };

export async function GET(req: NextRequest, { params }: NgCanh) {
  return chay(async () => {
    const a = await canNguoiDung(req);
    if (a instanceof NextResponse) return a;
    return traLoi(await dsGiaoViec(pt, a.chu, (await params).id));
  });
}

export async function POST(req: NextRequest, { params }: NgCanh) {
  return chay(async () => {
    const a = await canNguoiDung(req);
    if (a instanceof NextResponse) return a;
    return traLoi(await taoGiaoViec(pt, a.chu, (await params).id, await docThan(req)));
  });
}
```

`src/app/api/v2/kho/giao-viec/[gid]/route.ts`:

```ts
import { NextRequest, NextResponse } from "next/server";
import { phuThuocKhoThat as pt } from "@lib/kho/phu-thuoc";
import { canNguoiDung, chay, docThan, traLoi } from "@lib/kho/tra-loi";
import { suaGiaoViec, thuHoiGiaoViec } from "@lib/to-chuc/giao-viec";

export const dynamic = "force-dynamic";
type NgCanh = { params: Promise<{ gid: string }> };

export async function PATCH(req: NextRequest, { params }: NgCanh) {
  return chay(async () => {
    const a = await canNguoiDung(req);
    if (a instanceof NextResponse) return a;
    return traLoi(await suaGiaoViec(pt, a.chu, (await params).gid, await docThan(req)));
  });
}

export async function DELETE(req: NextRequest, { params }: NgCanh) {
  return chay(async () => {
    const a = await canNguoiDung(req);
    if (a instanceof NextResponse) return a;
    return traLoi(await thuHoiGiaoViec(pt, a.chu, (await params).gid));
  });
}
```

`src/app/api/v2/kho/giao-viec/cua-toi/route.ts`:

```ts
import { NextRequest, NextResponse } from "next/server";
import { phuThuocKhoThat as pt } from "@lib/kho/phu-thuoc";
import { canNguoiDung, chay, traLoi } from "@lib/kho/tra-loi";
import { viecDuocGiao } from "@lib/to-chuc/giao-viec";

export const dynamic = "force-dynamic";

/** «Việc được giao» của người gọi trong tổ chức đang chọn (header). */
export async function GET(req: NextRequest) {
  return chay(async () => {
    const a = await canNguoiDung(req);
    if (a instanceof NextResponse) return a;
    return traLoi(await viecDuocGiao(pt, a.chu, req.nextUrl.searchParams));
  });
}
```

- [ ] **Step 5: Chạy kiểm**

Run: `npx tsx --test tests/to-chuc-dich-vu.test.ts` rồi `npx tsc --noEmit`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add src/lib/to-chuc/giao-viec.ts src/app/api/v2/kho/van-ban src/app/api/v2/kho/giao-viec tests/to-chuc-dich-vu.test.ts
git commit -m "feat(to-chuc): API giao viec - giao, cap nhat, thu hoi, nhat ky, viec duoc giao

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 14: Ví credit ở lớp tác vụ AI — chọn ví theo ngữ cảnh

**Files:**
- Create: `src/lib/to-chuc/vi.ts`
- Modify: `src/lib/credit/so-du.ts` (`taoKhoCredit(rpc)`, ví trong `soDu` / `mo`, `docSoDuVi`)
- Modify: `src/lib/tac-vu/chay-tac-vu.ts` (`NguCanhGoi.vi`, `uocTinhTacVu(..., vi?)`, mã `GOI_TO_CHUC_HET_HAN`, `KHONG_THUOC_TO_CHUC`)
- Modify: `src/lib/tac-vu/gioi-han-tep.ts` (`docToiDaTepByte(user, vi?)` theo gói tổ chức)
- Modify: `src/app/api/v2/tac-vu/[ten]/route.ts`, `src/app/api/v2/tac-vu/[ten]/uoc-tinh/route.ts`, `src/app/api/v2/credit/route.ts`
- Test: `tests/chay-tac-vu-vi.test.ts` (mới), `tests/to-chuc-dich-vu.test.ts` (thêm `describe`)

**Interfaces:**
- Consumes: Task 7 (`tac_vu_mo_vi`, `credit_so_du_vi`), Task 9 (`docNguCanh`, `DAU_TO_CHUC`, `LOI_TO_CHUC`).
- Produces: `type Vi = { vi_loai: "ca_nhan" | "to_chuc"; vi_id: string }`; `chonVi(rpc, userId, dau): Promise<{ vi: Vi } | KetQua>`;
  `taoKhoCredit(rpc: GoiRpc): KhoCredit`; `KhoCredit.soDu(userId, vi?)`; `MoLuot.vi?: Vi`; `KetQuaMo.ma` thêm
  `"GOI_HET_HAN" | "KHONG_THUOC_TO_CHUC"`; `docSoDuVi(rpc, userId, vi): Promise<SoDu | null>`;
  `NguCanhGoi.vi?: Vi`; `uocTinhTacVu(dn, body, user, phu, vi?)`; `PhuThuoc.toiDaTepByte?(user, vi?)`.
  Ngữ cảnh cá nhân: **không** truyền `vi` (lời gọi y như cũ).

- [ ] **Step 1: Viết bài kiểm `tests/chay-tac-vu-vi.test.ts` (KhoCredit giả)**

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { chayTacVu, uocTinhTacVu, type DinhNghiaTacVu, type PhuThuoc } from "../src/lib/tac-vu/chay-tac-vu";
import type { BangGia } from "../src/lib/credit/uoc-tinh";
import type { KetQuaMo, KhoCredit, MoLuot } from "../src/lib/credit/so-du";
import type { ApiUser } from "../src/types";

const BG: BangGia = {
  chung: { gia_tri_credit_vnd: 100, ty_gia_vnd_usd: 26000, mo_hinh_mac_dinh: "gemini-3.6-flash" },
  mo_hinh: { "gemini-3.6-flash": { vao_usd_trieu: 1.5, ra_usd_trieu: 7.5 } },
  tac_vu: { thu: { toi_thieu: 2, ra_min_he_so: 0, ra_min_hang: 1000, ra_max_he_so: 0, ra_max_hang: 5000, toi_da_ky_tu: 100 } },
};
const thu: DinhNghiaTacVu<{ noi_dung: string }> = {
  ten: "thu", kieu_ket_qua: "van_ban", nhiet_do: 0.3,
  kiemDauVao: (b) => ({ ok: true, du_lieu: { noi_dung: String((b as any)?.noi_dung ?? "x") } }),
  dungPrompt: (d) => ({ he_thong: "HT", nguoi_dung: d.noi_dung }),
};
const USER = { id: "U1", api_key: "vbp_x" } as ApiUser;
const VI = { vi_loai: "to_chuc" as const, vi_id: "T1" };

function dungPhu(moTra: KetQuaMo) {
  const moDaGoi: MoLuot[] = [];
  const soDuGoi: unknown[][] = [];
  const kho: KhoCredit = {
    soDu: async (...a: unknown[]) => { soDuGoi.push(a); return 500; },
    mo: async (m) => { moDaGoi.push(m); return moTra; },
    chot: async (_id, c) => ({ credit_tru: c.credit_that, so_du: 480, trang_thai: "xong" }),
    hoan: async () => ({ so_du: 500 }),
  };
  const phu: PhuThuoc = {
    bangGia: async () => BG, kho,
    layKhoa: async () => ({ keyId: "k", provider: "gemini", apiKey: "x", model: "m", source: "system" }),
    goiAI: async () => ({ success: true, content: "Kết quả", promptTokens: 10, completionTokens: 100, finishReason: "STOP" }),
    bayGio: () => new Date("2026-11-19T00:00:00Z"),
  };
  return { phu, moDaGoi, soDuGoi };
}

test("ngữ cảnh tổ chức: mo nhận vi; ngữ cảnh cá nhân: không có khóa vi (lời gọi như cũ)", async () => {
  const a = dungPhu({ ma: "ok", luot_id: "L1", so_du: 400 });
  const kq = await chayTacVu(thu, { noi_dung: "abc" }, { user: USER, nguonGoi: "web", phienBan: null, vi: VI }, a.phu);
  assert.equal(kq.ok, true);
  assert.deepEqual(a.moDaGoi[0].vi, VI);
  const b = dungPhu({ ma: "ok", luot_id: "L2", so_du: 400 });
  await chayTacVu(thu, { noi_dung: "abc" }, { user: USER, nguonGoi: "web", phienBan: null }, b.phu);
  assert.equal("vi" in b.moDaGoi[0], false);
});

test("gói tổ chức hết hạn → 403 GOI_TO_CHUC_HET_HAN; không thuộc → 403 KHONG_THUOC_TO_CHUC; không gọi AI", async () => {
  const a = dungPhu({ ma: "GOI_HET_HAN", so_du: 100 });
  const kq = await chayTacVu(thu, { noi_dung: "abc" }, { user: USER, nguonGoi: "web", phienBan: null, vi: VI }, a.phu);
  assert.deepEqual([!kq.ok && kq.code, !kq.ok && kq.status], ["GOI_TO_CHUC_HET_HAN", 403]);
  const b = dungPhu({ ma: "KHONG_THUOC_TO_CHUC", so_du: 0 });
  const kq2 = await chayTacVu(thu, { noi_dung: "abc" }, { user: USER, nguonGoi: "web", phienBan: null, vi: VI }, b.phu);
  assert.deepEqual([!kq2.ok && kq2.code, !kq2.ok && kq2.status], ["KHONG_THUOC_TO_CHUC", 403]);
});

test("ước tính: số dư đọc theo ví khi có ví; không ví thì gọi soDu(userId) đúng một tham số", async () => {
  const a = dungPhu({ ma: "ok", so_du: 0 });
  await uocTinhTacVu(thu, { noi_dung: "abc" }, USER, a.phu, VI);
  assert.deepEqual(a.soDuGoi[0], ["U1", VI]);
  await uocTinhTacVu(thu, { noi_dung: "abc" }, USER, a.phu);
  assert.deepEqual(a.soDuGoi[1], ["U1"]);
});
```

Thêm vào `tests/to-chuc-dich-vu.test.ts` (KhoCredit thật trên CSDL thử):

```ts
import { docSoDuVi, taoKhoCredit } from "../src/lib/credit/so-du";
import { chonVi } from "../src/lib/to-chuc/vi";

describe("ví credit qua lớp TS", { skip: !CO_CSDL_THU }, () => {
  let csdl: Awaited<ReturnType<typeof taoCsdlThu>>;
  let c: Client;

  before(async () => {
    csdl = await taoCsdlThu();
    c = new Client({ connectionString: csdl.url });
    await c.connect();
  });
  after(async () => {
    await c?.end();
    await csdl?.xoa();
  });

  test("chonVi + taoKhoCredit: mở / chốt trên ví tổ chức; người ngoài 403; cá nhân dùng tac_vu_mo cũ", async () => {
    const rpc = rpcPg(c);
    const b = await boToChuc(c);
    const kho = taoKhoCredit(rpc);
    const cv = await chonVi(rpc, b.canBo, b.toChuc);
    assert.ok("vi" in cv);
    const vi = (cv as any).vi;
    assert.equal(await kho.soDu(b.canBo, vi), 3000);
    const m = await kho.mo({ user_id: b.canBo, tac_vu: "tom-tat", credit_min: 1, credit_max: 50, provider: "gemini",
      model: "m", nguon_goi: "web", phien_ban: null, vi });
    assert.equal(m.ma, "ok");
    assert.equal((await kho.chot(m.luot_id!, { credit_that: 20, token_vao: 1, token_ra: 1, bi_cat: false })).so_du, 2980);
    assert.equal((await docSoDuVi(rpc, b.canBo, vi))!.tong, 2980);
    assert.equal(ma((await chonVi(rpc, await taoNguoiDung(c), b.toChuc)) as KetQua), "403 KHONG_THUOC_TO_CHUC");
    const caNhan = await chonVi(rpc, b.canBo, null);
    assert.deepEqual(caNhan, { vi: { vi_loai: "ca_nhan", vi_id: b.canBo } });
    assert.equal(await kho.soDu(b.canBo), 30);
  });
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx tsx --test tests/chay-tac-vu-vi.test.ts tests/to-chuc-dich-vu.test.ts`
Expected: FAIL — `vi` không có trong `NguCanhGoi`; không có `taoKhoCredit`, `chonVi`.

- [ ] **Step 3: Viết mã**

`src/lib/to-chuc/vi.ts`:

```ts
import type { KetQua } from "../phien/dich-vu";
import type { GoiRpc } from "../phien/rpc";
import { docNguCanh } from "./ngu-canh";

/** Ví credit của một lượt AI (spec đợt B §2.3). */
export type Vi = { vi_loai: "ca_nhan" | "to_chuc"; vi_id: string };

/** Ví theo header X-VBP-To-Chuc: không header → ví cá nhân; tổ chức → phải là thành viên hoạt động (403 nếu không). */
export async function chonVi(rpc: GoiRpc, userId: string, dau: string | null): Promise<{ vi: Vi } | KetQua> {
  const nc = await docNguCanh(rpc, userId, dau);
  if (!("chu" in nc)) return nc;
  return { vi: { vi_loai: nc.chu.chu_loai, vi_id: nc.chu.chu_id } };
}
```

`src/lib/credit/so-du.ts` — giữ nguyên `LoCredit`, `SoDu`, `ChotLuot`, `goiHam`, `docSoDu`, `TruCu`, `truCreditCu`,
`dieuChinhCredit`; thay phần `MoLuot`, `KetQuaMo`, `KhoCredit`, `khoCreditSupabase`:

```ts
import { rpcSupabase, type GoiRpc } from "../phien/rpc";
import type { Vi } from "../to-chuc/vi";

export interface MoLuot {
  user_id: string;
  tac_vu: string;
  credit_min: number;
  credit_max: number;
  provider: string;
  model: string;
  nguon_goi: string;
  phien_ban: string | null;
  /** Đợt B: có = ví tổ chức (tac_vu_mo_vi); không có = ví cá nhân qua tac_vu_mo như cũ. */
  vi?: Vi;
}

export interface KetQuaMo {
  ma: "ok" | "KHONG_DU_CREDIT" | "QUA_NHIEU_LUOT" | "GOI_HET_HAN" | "KHONG_THUOC_TO_CHUC";
  luot_id?: string;
  so_du: number;
}

/** Mọi thao tác đổi số dư. Bản thật gọi hàm Postgres; bộ kiểm dùng bản giả. chot / hoan tự theo ví của lượt (019). */
export interface KhoCredit {
  soDu(userId: string, vi?: Vi): Promise<number>;
  mo(m: MoLuot): Promise<KetQuaMo>;
  chot(luotId: string, c: ChotLuot): Promise<{ credit_tru: number; so_du: number; trang_thai: string }>;
  hoan(luotId: string, maLoi: string): Promise<{ so_du: number }>;
}

/** Số dư theo ví; NULL khi không phải thành viên của tổ chức (hoặc ví cá nhân của người khác). */
export const docSoDuVi = (rpc: GoiRpc, userId: string, vi: Vi) =>
  rpc<SoDu | null>("credit_so_du_vi", { p_user: userId, p_vi_loai: vi.vi_loai, p_vi_id: vi.vi_id });

export function taoKhoCredit(rpc: GoiRpc): KhoCredit {
  return {
    soDu: async (userId, vi) =>
      vi?.vi_loai === "to_chuc"
        ? ((await docSoDuVi(rpc, userId, vi))?.tong ?? 0)
        : (await rpc<SoDu>("credit_so_du", { p_user: userId })).tong,
    mo: (m) => {
      const thamSo = {
        p_user: m.user_id, p_tac_vu: m.tac_vu, p_credit_min: m.credit_min, p_credit_max: m.credit_max,
        p_provider: m.provider, p_model: m.model, p_nguon_goi: m.nguon_goi, p_phien_ban: m.phien_ban,
      };
      const vi = m.vi;
      return vi?.vi_loai === "to_chuc"
        ? rpc<KetQuaMo>("tac_vu_mo_vi", { ...thamSo, p_vi_loai: vi.vi_loai, p_vi_id: vi.vi_id })
        : rpc<KetQuaMo>("tac_vu_mo", thamSo);
    },
    chot: (luotId, c) =>
      rpc("credit_chot", { p_luot: luotId, p_credit_that: c.credit_that, p_token_vao: c.token_vao, p_token_ra: c.token_ra, p_bi_cat: c.bi_cat }),
    hoan: (luotId, maLoi) => rpc("credit_hoan", { p_luot: luotId, p_ma_loi: maLoi }),
  };
}

export const khoCreditSupabase: KhoCredit = taoKhoCredit(rpcSupabase);
```

`src/lib/tac-vu/chay-tac-vu.ts`:

```ts
import type { Vi } from "../to-chuc/vi";

export type MaLoi =
  | "DAU_VAO_SAI"
  | "QUA_DAI"
  | "KHONG_DU_CREDIT"
  | "QUA_NHIEU_LUOT"
  | "AI_QUA_TAI"
  | "AI_LOI"
  | "NGAT_KET_NOI"
  | "GOI_TO_CHUC_HET_HAN"
  | "KHONG_THUOC_TO_CHUC";
```

Trong `PhuThuoc`: `toiDaTepByte?: (user: ApiUser, vi?: Vi) => Promise<number>;`. Trong `NguCanhGoi` thêm
`/** Đợt B: ví tổ chức; không có = ví cá nhân. */ vi?: Vi;`. `chuanBi` nhận thêm `vi?: Vi` và gọi
`phu.toiDaTepByte ? await (vi ? phu.toiDaTepByte(user, vi) : phu.toiDaTepByte(user)) : Infinity`.

```ts
export async function uocTinhTacVu<V>(
  dn: DinhNghiaTacVu<V>, body: unknown, user: ApiUser, phu: PhuThuoc, vi?: Vi
): Promise<KetQuaUocTinh> {
  const cb = await chuanBi(dn, body, user, phu, vi);
  if (!cb.ok) return cb;
  const so_du = vi ? await phu.kho.soDu(user.id, vi) : await phu.kho.soDu(user.id);
  return { ok: true, credit_min: cb.uoc.credit_min, credit_max: cb.uoc.credit_max, so_du, du_credit: so_du >= cb.uoc.credit_max };
}
```

Trong `chayTacVu`: `const cb = await chuanBi(dn, body, ctx.user, phu, ctx.vi);`; lời gọi `phu.kho.mo({ … })` thêm
`...(ctx.vi ? { vi: ctx.vi } : {}),` vào cuối đối tượng; ngay sau khối `if (mo.ma === "KHONG_DU_CREDIT") { … }` thêm:

```ts
  if (mo.ma === "GOI_HET_HAN") {
    return loi("GOI_TO_CHUC_HET_HAN", 403,
      "Gói của tổ chức chưa kích hoạt hoặc đã hết hạn. Quản trị gia hạn gói để dùng credit của tổ chức.");
  }
  if (mo.ma === "KHONG_THUOC_TO_CHUC") {
    return loi("KHONG_THUOC_TO_CHUC", 403, "Tài khoản không thuộc tổ chức này hoặc đã rời tổ chức.");
  }
```

`src/lib/tac-vu/gioi-han-tep.ts` — thêm nhánh ví tổ chức (giữ nguyên nhánh cá nhân):

```ts
import { rpcSupabase } from "../phien/rpc";
import type { Vi } from "../to-chuc/vi";

/** Trần tổng tệp mỗi lần gửi: ví tổ chức có gói còn hạn → max_file_size_mb của gói tổ chức; còn lại theo gói cá nhân. */
export async function docToiDaTepByte(user: ApiUser, vi?: Vi): Promise<number> {
  if (vi?.vi_loai === "to_chuc") {
    const t = await rpcSupabase<{ goi_ma: string | null; con_hieu_luc: boolean } | null>("to_chuc_doc", { p_to_chuc: vi.vi_id, p_user: user.id });
    if (t?.con_hieu_luc && t.goi_ma) {
      const ds = await rpcSupabase<Array<{ ma: string; max_file_size_mb: number }>>("goi_to_chuc_ds", {});
      const g = ds.find((x) => x.ma === t.goi_ma);
      if (g) return byteTuMb(g.max_file_size_mb);
    }
  }
  const goi = effectivePlanId(user) || "free";
  // … phần đọc subscription_plans giữ nguyên như hiện nay
}
```

`src/app/api/v2/tac-vu/[ten]/route.ts` — sau `if (!user) return unauthorized();` thêm chọn ví, và truyền vào ngữ cảnh:

```ts
import { rpcSupabase } from "@lib/phien/rpc";
import { traLoi as traLoiKetQua } from "@lib/phien/tra-loi";
import { DAU_TO_CHUC } from "@lib/to-chuc/ngu-canh";
import { chonVi } from "@lib/to-chuc/vi";
// …
  const cv = await chonVi(rpcSupabase, user.id, req.headers.get(DAU_TO_CHUC));
  if (!("vi" in cv)) return traLoiKetQua(cv);
  const viToChuc = cv.vi.vi_loai === "to_chuc" ? { vi: cv.vi } : {};
// … trong lời gọi chayTacVu:
      { user, clientIp: getClientIp(req), nguonGoi, phienBan, signal: req.signal, ...viToChuc },
```

`src/app/api/v2/tac-vu/[ten]/uoc-tinh/route.ts` — cùng ba import + đoạn `chonVi` như trên, rồi
`const kq = await uocTinhTacVu(dn, body, user, phuThuocThat, cv.vi.vi_loai === "to_chuc" ? cv.vi : undefined);`.

`src/app/api/v2/credit/route.ts`:

```ts
import { NextRequest } from "next/server";
import { docSoDu, docSoDuVi } from "@lib/credit/so-du";
import { rpcSupabase } from "@lib/phien/rpc";
import { traLoi as traLoiKetQua } from "@lib/phien/tra-loi";
import { fail, ok, unauthorized } from "@lib/response";
import { LOI_TO_CHUC } from "@lib/to-chuc/loi";
import { DAU_TO_CHUC } from "@lib/to-chuc/ngu-canh";
import { chonVi } from "@lib/to-chuc/vi";
import { xacThuc } from "@lib/tac-vu/xac-thuc";

export const dynamic = "force-dynamic";

/** GET /api/v2/credit — số dư của ví đang chọn (header X-VBP-To-Chuc), các lô còn hạn, ngày hết hạn gần nhất. */
export async function GET(req: NextRequest) {
  const user = await xacThuc(req);
  if (!user) return unauthorized();
  try {
    const cv = await chonVi(rpcSupabase, user.id, req.headers.get(DAU_TO_CHUC));
    if (!("vi" in cv)) return traLoiKetQua(cv);
    if (cv.vi.vi_loai === "ca_nhan") return ok(await docSoDu(user.id));
    const s = await docSoDuVi(rpcSupabase, user.id, cv.vi);
    return s ? ok(s) : traLoiKetQua(LOI_TO_CHUC.khongThuoc());
  } catch (e) {
    console.error("[CREDIT] Không đọc được số dư", e);
    return fail("Không đọc được số dư credit.", 500);
  }
}
```

- [ ] **Step 4: Chạy kiểm**

Run: `npx tsx --test tests/chay-tac-vu-vi.test.ts tests/chay-tac-vu.test.ts tests/chay-tac-vu-tep.test.ts tests/tac-vu-*.test.ts tests/to-chuc-dich-vu.test.ts` rồi `npx tsc --noEmit`
Expected: PASS; bài cũ của `chay-tac-vu` không sửa.

- [ ] **Step 5: Commit**

```bash
git add src/lib/to-chuc/vi.ts src/lib/credit/so-du.ts src/lib/tac-vu src/app/api/v2/tac-vu src/app/api/v2/credit tests/chay-tac-vu-vi.test.ts tests/to-chuc-dich-vu.test.ts
git commit -m "feat(to-chuc): tac vu AI tru vi to chuc theo header, so du theo vi, tran tep theo goi to chuc

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 15: Đơn hàng tổ chức ở API (`/api/orders`, webhook)

**Files:**
- Create: `src/lib/to-chuc/don.ts`
- Modify: `src/lib/orders.ts` (`OrderRow`, `tenDon`, `orderView`)
- Modify: `src/app/api/orders/route.ts`
- Modify: `src/app/api/webhooks/sepay/route.ts` (câu báo Telegram cho đơn tổ chức)
- Test: `tests/to-chuc-don.test.ts` (mới)

**Interfaces:**
- Consumes: Task 8 (`orders.to_chuc_id`, `goi_to_chuc_ma`, trigger), Task 2 (`to_chuc_vai_tro`, `goi_to_chuc_ds`).
- Produces: `type GoiToChuc` (đúng hợp đồng); `docGoiToChuc(rpc): Promise<GoiToChuc[]>`;
  `goiBanDuoc(g?: GoiToChuc): g is GoiToChuc & { gia_nam_vnd: number }`;
  `kiemQuyenDonToChuc(rpc, userId, toChucId: unknown): Promise<{ toChucId: string | null } | KetQua>`;
  `OrderRow.to_chuc_id`, `OrderRow.goi_to_chuc_ma`, `OrderRow.loai` thêm `"goi_to_chuc"`; `tenDon(o, tenGoi, nap, dl, gtc?)`;
  `orderView(...)` trả thêm `toChucId`, `goiToChucMa`.

- [ ] **Step 1: Viết bài kiểm `tests/to-chuc-don.test.ts`**

```ts
import { after, before, describe, test } from "node:test";
import assert from "node:assert/strict";
import { Client } from "pg";
import { orderView, tenDon, type OrderRow } from "../src/lib/orders";
import { goiBanDuoc, kiemQuyenDonToChuc, type GoiToChuc } from "../src/lib/to-chuc/don";
import { CO_CSDL_THU, taoCsdlThu, taoNguoiDung } from "./ho-tro/csdl-thu";
import { rpcPg } from "./ho-tro/rpc-pg";
import { boToChuc } from "./ho-tro/to-chuc";

const GTC: GoiToChuc = { ma: "tc-30", ten: "Tổ chức 30 người", so_thanh_vien_toi_da: 30, credit_thang: 8000,
  dung_luong_mb: 153600, max_file_size_mb: 10, gia_nam_vnd: 6000000, dang_ban: true };
const DON: OrderRow = {
  id: "1", code: "VBPABCDEFGH", user_id: "u", loai: "goi_to_chuc", plan_id: null, cycle: null, goi_nap_ma: null,
  goi_dung_luong_ma: null, so_thang: null, to_chuc_id: "t1", goi_to_chuc_ma: "tc-30", amount_vnd: 6000000, status: "pending",
  paid_source: null, paid_ref: null, created_at: "2026-11-19T00:00:00Z", expires_at: "2026-11-20T00:00:00Z", paid_at: null,
  period_start: null, period_end: null,
};

test("tenDon / orderView đơn gói tổ chức", () => {
  assert.equal(tenDon(DON, {}, null, null, GTC), "Gói tổ chức Tổ chức 30 người · 1 năm");
  assert.equal(tenDon(DON, {}, null, null), "Gói tổ chức tc-30 · 1 năm");
  const v = orderView(DON, "x", { bin: "970422", name: "MB", account: "1", holder: "A" });
  assert.deepEqual([v.loai, v.toChucId, v.goiToChucMa], ["goi_to_chuc", "t1", "tc-30"]);
  assert.equal(orderView({ ...DON, loai: "nap", to_chuc_id: null, goi_to_chuc_ma: null, goi_nap_ma: "nap-49" }, "x", null).toChucId, null);
});

test("goiBanDuoc: phải dang_ban và có giá dương", () => {
  assert.equal(goiBanDuoc(GTC), true);
  assert.equal(goiBanDuoc({ ...GTC, dang_ban: false }), false);
  assert.equal(goiBanDuoc({ ...GTC, gia_nam_vnd: null }), false);
  assert.equal(goiBanDuoc(undefined), false);
});

describe("kiemQuyenDonToChuc trên CSDL thử", { skip: !CO_CSDL_THU }, () => {
  let csdl: Awaited<ReturnType<typeof taoCsdlThu>>;
  let c: Client;
  before(async () => {
    csdl = await taoCsdlThu();
    c = new Client({ connectionString: csdl.url });
    await c.connect();
  });
  after(async () => {
    await c?.end();
    await csdl?.xoa();
  });

  test("không gửi to_chuc_id → đơn cá nhân; quản trị → được; văn thư → 403; người ngoài → 403; rác → 400", async () => {
    const rpc = rpcPg(c);
    const b = await boToChuc(c);
    assert.deepEqual(await kiemQuyenDonToChuc(rpc, b.vanThu, undefined), { toChucId: null });
    assert.deepEqual(await kiemQuyenDonToChuc(rpc, b.quanTri, b.toChuc.toUpperCase()), { toChucId: b.toChuc });
    assert.equal((await kiemQuyenDonToChuc(rpc, b.vanThu, b.toChuc) as any).code, "KHONG_DU_QUYEN");
    assert.equal((await kiemQuyenDonToChuc(rpc, await taoNguoiDung(c), b.toChuc) as any).code, "KHONG_THUOC_TO_CHUC");
    assert.equal((await kiemQuyenDonToChuc(rpc, b.quanTri, "abc") as any).code, "DAU_VAO_SAI");
  });
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx tsx --test tests/to-chuc-don.test.ts`
Expected: FAIL — không có `src/lib/to-chuc/don`.

- [ ] **Step 3: Viết `src/lib/to-chuc/don.ts`**

```ts
import type { KetQua } from "../phien/dich-vu";
import type { GoiRpc } from "../phien/rpc";
import { laUuid } from "../kho/kiem-dau-vao";
import { LOI_KHO } from "../kho/loi";
import { LOI_TO_CHUC } from "./loi";

export type GoiToChuc = {
  ma: string; ten: string; so_thanh_vien_toi_da: number; credit_thang: number; dung_luong_mb: number;
  max_file_size_mb: number; gia_nam_vnd: number | null; dang_ban: boolean;
};

export const docGoiToChuc = (rpc: GoiRpc) => rpc<GoiToChuc[]>("goi_to_chuc_ds", {});

export const goiBanDuoc = (g?: GoiToChuc | null): g is GoiToChuc & { gia_nam_vnd: number } =>
  !!g && g.dang_ban && typeof g.gia_nam_vnd === "number" && g.gia_nam_vnd > 0;

/** Đơn gắn tổ chức chỉ do quản trị hoạt động tạo (trigger orders_kiem_to_chuc là lớp chặn thứ hai). */
export async function kiemQuyenDonToChuc(rpc: GoiRpc, userId: string, toChucId: unknown): Promise<{ toChucId: string | null } | KetQua> {
  if (toChucId === undefined || toChucId === null || toChucId === "") return { toChucId: null };
  if (!laUuid(toChucId)) return LOI_KHO.dauVaoSai("Tổ chức không hợp lệ.");
  const id = toChucId.toLowerCase();
  const vai = await rpc<string | null>("to_chuc_vai_tro", { p_to_chuc: id, p_user: userId });
  if (!vai) return LOI_TO_CHUC.khongThuoc();
  if (vai !== "quan_tri") return LOI_TO_CHUC.khongDuQuyen();
  return { toChucId: id };
}
```

- [ ] **Step 4: Sửa `src/lib/orders.ts`**

```ts
import type { GoiToChuc } from "./to-chuc/don";

export interface OrderRow {
  id: string;
  code: string;
  user_id: string;
  loai: "goi" | "nap" | "dung_luong" | "goi_to_chuc";
  plan_id: string | null;
  cycle: "month" | "year" | null;
  goi_nap_ma: string | null;
  goi_dung_luong_ma: string | null;
  so_thang: number | null;
  /** Đợt B: đơn của tổ chức (nạp / dung lượng / gói tổ chức). */
  to_chuc_id?: string | null;
  goi_to_chuc_ma?: string | null;
  amount_vnd: number;
  // … các cột còn lại giữ nguyên
}

export function tenDon(
  o: Pick<OrderRow, "loai" | "plan_id" | "goi_nap_ma"> & Partial<Pick<OrderRow, "goi_dung_luong_ma" | "so_thang" | "goi_to_chuc_ma">>,
  tenGoi: Record<string, string>,
  nap: GoiNap | null,
  dl: GoiDungLuong | null = null,
  gtc: GoiToChuc | null = null
): string {
  if (o.loai === "goi_to_chuc") return `Gói tổ chức ${gtc?.ten ?? o.goi_to_chuc_ma ?? ""} · 1 năm`.replace(/\s+·/, " ·");
  // … các nhánh dung_luong / nap / goi giữ nguyên
}
```

Trong `orderView`, đối tượng trả thêm hai khóa (sau `themMb`):

```ts
    toChucId: o.to_chuc_id ?? null,
    goiToChucMa: o.goi_to_chuc_ma ?? null,
```

(Hai cột để `?:` trong `OrderRow` để các bài kiểm / dữ liệu mẫu cũ không có hai khóa này vẫn biên dịch.)

- [ ] **Step 5: Sửa `src/app/api/orders/route.ts`**

```ts
import { rpcSupabase } from "@lib/phien/rpc";
import { traLoi as traLoiKetQua } from "@lib/phien/tra-loi";
import { docGoiToChuc, goiBanDuoc, kiemQuyenDonToChuc, type GoiToChuc } from "@lib/to-chuc/don";

async function donDangCho(userId: string): Promise<OrderRow[]> { /* giữ nguyên */ }

/** Đơn cùng loại còn chờ: phải cùng tổ chức (null = cá nhân). */
const cungChu = (o: OrderRow, toChucId: string | null) => (o.to_chuc_id ?? null) === toChucId;

/** Đơn gói tổ chức (trả năm): giá đọc ở máy chủ, gói chưa mở bán → từ chối. */
async function taoDonGoiToChuc(user: ApiUser, ma: string, toChucId: string, bank: BankConfig) {
  let ds: GoiToChuc[];
  try {
    ds = await docGoiToChuc(rpcSupabase);
  } catch (e) {
    console.error("[ORDERS] doc goi to chuc loi", e);
    return fail("Không đọc được bảng gói tổ chức. Vui lòng thử lại.", 503);
  }
  const goi = ds.find((g) => g.ma === ma);
  if (!goiBanDuoc(goi)) return fail("Gói tổ chức chưa mở bán hoặc không hợp lệ.");
  const open = await donDangCho(user.id);
  const same = open.find((o) => o.loai === "goi_to_chuc" && o.goi_to_chuc_ma === ma && cungChu(o, toChucId) && o.amount_vnd === goi.gia_nam_vnd);
  if (same) return ok(orderView(same, tenDon(same, {}, null, null, goi), bank));
  if (open.length >= MAX_OPEN_ORDERS) return fail("Có quá nhiều đơn đang chờ thanh toán. Vui lòng thử lại sau.", 429);
  const { data: order, error } = await getSupabase()
    .from("orders")
    .insert({ code: generateOrderCode(), user_id: user.id, loai: "goi_to_chuc", goi_to_chuc_ma: ma, to_chuc_id: toChucId, amount_vnd: goi.gia_nam_vnd })
    .select("*")
    .single();
  if (error || !order) return fail("Không tạo được đơn. Vui lòng thử lại.", 500);
  const o = order as OrderRow;
  return ok(orderView(o, tenDon(o, {}, null, null, goi), bank));
}
```

`taoDonNap(user, ma, bank, toChucId: string | null)` và `taoDonDungLuong(user, ma, soThangVao, bank, toChucId: string | null)`:
điều kiện `same` thêm `&& cungChu(o, toChucId)`, đối tượng `insert` thêm `to_chuc_id: toChucId`. Trong `POST`:

```ts
  const body = await req.json().catch(() => ({}));
  const q = await kiemQuyenDonToChuc(rpcSupabase, user.id, body.to_chuc_id);
  if (!("toChucId" in q)) return traLoiKetQua(q);
  const toChucId = q.toChucId;
  if (body.loai === "goi_to_chuc") {
    if (!toChucId) return fail("Thiếu tổ chức của đơn.");
    return taoDonGoiToChuc(user, String(body.goi_to_chuc_ma ?? ""), toChucId, bank);
  }
  if (body.goiNap !== undefined) return taoDonNap(user, String(body.goiNap ?? ""), bank, toChucId);
  if (body.loai === "dung_luong") return taoDonDungLuong(user, String(body.goi_dung_luong_ma ?? ""), body.so_thang, bank, toChucId);
  if (toChucId) return fail("Gói cá nhân không mua cho tổ chức.");
  // … nhánh gói tháng / năm cá nhân giữ nguyên
```

Trong `GET` (lịch sử đơn): thêm `docGoiToChuc(rpcSupabase).catch(() => [])` vào `Promise.all`, và truyền
`gtc = ds.find((g) => g.ma === o.goi_to_chuc_ma) ?? null` làm tham số thứ năm của `tenDon`.

- [ ] **Step 6: Sửa `src/app/api/webhooks/sepay/route.ts`**

Trong `.select(...)` sau khi `pay_order` trả `paid`, thêm `goi_to_chuc_ma, to_chuc_id`; kiểu `p` thêm
`goi_to_chuc_ma: string | null; to_chuc_id: string | null;`; câu báo thêm nhánh đầu:

```ts
        p?.loai === "goi_to_chuc"
          ? `💰 VanBanPlus: nhận ${vnd(amount)} · đơn ${code}\n${p?.users?.email ?? "?"} · gói tổ chức ${p?.goi_to_chuc_ma ?? "?"}\n` +
            `Tổ chức ${p?.to_chuc_id ?? "?"} dùng tới ${han} · nhớ xuất HĐĐT (skill PA)`
          : p?.loai === "dung_luong"
          // … các nhánh cũ giữ nguyên
```

(Đơn nạp / dung lượng của tổ chức: thêm `${p?.to_chuc_id ? " · cho tổ chức " + p.to_chuc_id : ""}` vào dòng email
ở hai nhánh tương ứng.)

- [ ] **Step 7: Chạy kiểm**

Run: `npx tsx --test tests/to-chuc-don.test.ts tests/kho-don-hang.test.ts tests/don-nap.test.ts tests/order-rules.test.ts` rồi `npx tsc --noEmit`
Expected: PASS.

- [ ] **Step 8: Commit**

```bash
git add src/lib/to-chuc/don.ts src/lib/orders.ts src/app/api/orders src/app/api/webhooks/sepay tests/to-chuc-don.test.ts
git commit -m "feat(to-chuc): don hang to chuc - goi to chuc tra nam, nap va dung luong cho to chuc, chi quan tri

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 16: Kịch bản tấn công thành bài kiểm

**Files:**
- Create: `tests/to-chuc-tan-cong.test.ts`

**Interfaces:**
- Consumes: mọi dịch vụ Task 9–15, trợ giúp `tests/ho-tro/to-chuc.ts`.

- [ ] **Step 1: Viết bài kiểm (mỗi bài một dòng của bảng «Kịch bản tấn công»)**

```ts
import { after, before, describe, test } from "node:test";
import assert from "node:assert/strict";
import { createHash, randomUUID } from "node:crypto";
import { Client } from "pg";
import { CO_CSDL_THU, taoCsdlThu, taoNguoiDung } from "./ho-tro/csdl-thu";
import { rpcPg } from "./ho-tro/rpc-pg";
import { khoTepGia } from "./ho-tro/kho";
import { boToChuc, nguoiXacMinh, themThanhVien } from "./ho-tro/to-chuc";
import { chuToChuc } from "../src/lib/kho/chu-so-huu";
import { docNguCanh } from "../src/lib/to-chuc/ngu-canh";
import { docVanBan, taoVanBan } from "../src/lib/kho/van-ban";
import { taiVeTep } from "../src/lib/kho/tep";
import { suaGiaoViec, taoGiaoViec, thuHoiGiaoViec } from "../src/lib/to-chuc/giao-viec";
import { suaThanhVien, suaToChuc } from "../src/lib/to-chuc/to-chuc";
import { nhanLoiMoi, taoLoiMoi } from "../src/lib/to-chuc/loi-moi";
import { kiemQuyenDonToChuc } from "../src/lib/to-chuc/don";
import type { PhuThuocKho } from "../src/lib/kho/phu-thuoc";
import type { ThuEmail } from "../src/lib/email/gui";
import type { KetQua } from "../src/lib/phien/dich-vu";

const ma = (k: KetQua) => (k.loai === "loi" ? `${k.http} ${k.code}` : k.loai);
const du = (k: KetQua) => { assert.equal(k.loai, "xong", JSON.stringify(k)); return (k as any).data; };

describe("kịch bản tấn công đợt B", { skip: !CO_CSDL_THU }, () => {
  let csdl: Awaited<ReturnType<typeof taoCsdlThu>>;
  let c: Client;
  let pt: PhuThuocKho;
  let A: Awaited<ReturnType<typeof boToChuc>>;
  let B: Awaited<ReturnType<typeof boToChuc>>;
  let vbA: string;
  let gvA: string;
  const thu: ThuEmail[] = [];

  before(async () => {
    csdl = await taoCsdlThu();
    c = new Client({ connectionString: csdl.url });
    await c.connect();
    pt = { rpc: rpcPg(c), tep: khoTepGia().k, bayGio: () => new Date(), ghiLog: () => {} };
    A = await boToChuc(c);
    B = await boToChuc(c);
    vbA = du(await taoVanBan(pt, { id: A.vanThu }, chuToChuc(A.toChuc, A.vanThu, "van_thu"), { huong: "den", trich_yeu: "Mật của A" })).id;
    gvA = du(await taoGiaoViec(pt, chuToChuc(A.toChuc, A.vanThu, "van_thu"), vbA, { nguoi_nhan: A.canBo, vai: "chu_tri" })).id;
  });
  after(async () => {
    await c?.end();
    await csdl?.xoa();
  });

  test("giả header tổ chức khác → 403; thành viên đã rời → 403", async () => {
    assert.equal(ma((await docNguCanh(pt.rpc, B.quanTri, A.toChuc)) as KetQua), "403 KHONG_THUOC_TO_CHUC");
    const roi = await taoNguoiDung(c);
    await themThanhVien(c, A.toChuc, roi, "quan_tri", "da_roi");
    assert.equal(ma((await docNguCanh(pt.rpc, roi, A.toChuc)) as KetQua), "403 KHONG_THUOC_TO_CHUC");
  });

  test("IDOR van_ban_id của tổ chức khác (dùng ngữ cảnh riêng) → 404", async () => {
    assert.equal(ma(await docVanBan(pt, chuToChuc(B.toChuc, B.quanTri, "quan_tri"), vbA)), "404 KHONG_TIM_THAY");
  });

  test("IDOR giao_viec_id của tổ chức khác → 404 (sửa, thu hồi)", async () => {
    const chuB = chuToChuc(B.toChuc, B.quanTri, "quan_tri");
    assert.equal(ma(await suaGiaoViec(pt, chuB, gvA, { phien_ban: 1, trang_thai: "xong" })), "404 KHONG_TIM_THAY");
    assert.equal(ma(await thuHoiGiaoViec(pt, chuB, gvA)), "404 KHONG_TIM_THAY");
  });

  test("Chu giả có vai trò cao hơn ở TS vẫn bị SQL chặn (SQL tự tra vai trò)", async () => {
    const gia = chuToChuc(A.toChuc, A.canBo2, "quan_tri"); // canBo2 thật ra là cán bộ, không được giao vbA
    assert.equal(ma(await docVanBan(pt, gia, vbA)), "404 KHONG_TIM_THAY");
    assert.equal(ma(await taoGiaoViec(pt, gia, vbA, { nguoi_nhan: A.canBo2, vai: "phoi_hop" })), "404 KHONG_TIM_THAY");
  });

  test("cán bộ tự giao việc / sửa hạn việc của chính người đó / sửa việc của người khác → 403", async () => {
    const cb = chuToChuc(A.toChuc, A.canBo, "can_bo");
    assert.equal(ma(await taoGiaoViec(pt, cb, vbA, { nguoi_nhan: A.canBo2, vai: "phoi_hop" })), "403 KHONG_DU_QUYEN");
    assert.equal(ma(await suaGiaoViec(pt, cb, gvA, { phien_ban: 1, han_xu_ly: "2099-01-01" })), "403 KHONG_DU_QUYEN");
    const gv2 = du(await taoGiaoViec(pt, chuToChuc(A.toChuc, A.vanThu, "van_thu"), vbA, { nguoi_nhan: A.canBo2, vai: "phoi_hop" })).id;
    assert.equal(ma(await suaGiaoViec(pt, cb, gv2, { phien_ban: 1, trang_thai: "xong" })), "403 KHONG_DU_QUYEN");
  });

  test("cán bộ tự nâng vai trò / sửa hồ sơ tổ chức → 403", async () => {
    assert.equal(ma(await suaThanhVien(pt, A.canBo, A.toChuc, A.canBo, { vai_tro: "quan_tri" })), "403 KHONG_DU_QUYEN");
    assert.equal(ma(await suaToChuc(pt, A.canBo, A.toChuc, { phien_ban: 1, ten: "Chiếm" })), "403 KHONG_DU_QUYEN");
  });

  test("cán bộ đoán khoa_tao của người khác → 400, không lộ văn bản", async () => {
    const k = randomUUID();
    du(await taoVanBan(pt, { id: A.vanThu }, chuToChuc(A.toChuc, A.vanThu, "van_thu"), { huong: "den", trich_yeu: "Bí mật", khoa_tao: k }));
    const r = await taoVanBan(pt, { id: A.canBo2 }, chuToChuc(A.toChuc, A.canBo2, "can_bo"), { huong: "den", trich_yeu: "x", khoa_tao: k });
    assert.equal(ma(r), "400 DAU_VAO_SAI");
    assert.equal(JSON.stringify(r).includes("Bí mật"), false);
  });

  test("tải tệp của văn bản ngoài phạm vi bằng id tệp → 404", async () => {
    const t = (await c.query(`INSERT INTO kho_tep (chu_loai, chu_id, gan_loai, gan_id, duong_dan, ten, mime, byte_khai, byte, trang_thai, tao_boi)
      VALUES ('to_chuc', $1, 'van_ban', $2, $4, 'mat.pdf', 'application/pdf', 5, 5, 'xong', $3) RETURNING id`,
      [A.toChuc, vbA, A.vanThu, `to_chuc/${A.toChuc}/${randomUUID()}`])).rows[0].id;
    const cbMoi = await taoNguoiDung(c); // cán bộ chưa được giao gì (canBo / canBo2 đã được giao vbA ở bài trước)
    await themThanhVien(c, A.toChuc, cbMoi, "can_bo");
    assert.equal(ma(await taiVeTep(pt, chuToChuc(A.toChuc, cbMoi, "can_bo"), t)), "404 KHONG_TIM_THAY");
    assert.equal(ma(await taiVeTep(pt, chuToChuc(B.toChuc, B.quanTri, "quan_tri"), t)), "404 KHONG_TIM_THAY");
  });

  test("nhận lời mời bằng tài khoản email khác → 403; đoán mã không ra lời mời nào", async () => {
    const ptTC = { rpc: pt.rpc, guiEmail: async (t: ThuEmail) => { thu.push(t); }, gocWeb: () => "https://app.thu", ghiLog: () => {} };
    const k = await nguoiXacMinh(c);
    du(await taoLoiMoi(ptTC, A.quanTri, A.toChuc, { email: k.email, vai_tro: "quan_tri" }));
    const maMoi = /ma=([A-Za-z0-9_-]{43})/.exec(thu.at(-1)!.van_ban)![1];
    const keGian = await nguoiXacMinh(c);
    assert.equal(ma(await nhanLoiMoi(ptTC, keGian.id, { ma: maMoi })), "403 SAI_EMAIL_MOI");
    assert.equal(ma(await nhanLoiMoi(ptTC, keGian.id, { ma: "A".repeat(43) })), "400 LOI_MOI_KHONG_HOP_LE");
    const { rows } = await c.query(`SELECT count(*)::int AS n FROM to_chuc_loi_moi WHERE ma_bam = $1`,
      [createHash("sha256").update(maMoi).digest("hex")]);
    assert.equal(rows[0].n, 1); // CSDL chỉ giữ băm
    const { rows: tho } = await c.query(`SELECT count(*)::int AS n FROM to_chuc_loi_moi WHERE ma_bam = $1`, [maMoi]);
    assert.equal(tho[0].n, 0);
  });

  test("đơn cho tổ chức do người không phải quản trị tạo → 403 ở API và bị trigger chặn ở CSDL", async () => {
    assert.equal(((await kiemQuyenDonToChuc(pt.rpc, A.vanThu, A.toChuc)) as any).code, "KHONG_DU_QUYEN");
    await assert.rejects(c.query(`INSERT INTO orders (code, user_id, loai, goi_nap_ma, to_chuc_id, amount_vnd)
      VALUES ('VBPTANCUNG2', $1, 'nap', 'nap-49', $2, 49000)`, [A.vanThu, A.toChuc]), /chỉ quản trị/);
  });

  test("ví: người ngoài chạy AI bằng ví tổ chức → KHONG_THUOC_TO_CHUC, không giữ credit", async () => {
    const r = await pt.rpc("tac_vu_mo_vi", { p_user: B.quanTri, p_vi_loai: "to_chuc", p_vi_id: A.toChuc, p_tac_vu: "tom-tat",
      p_credit_min: 1, p_credit_max: 10, p_provider: "gemini", p_model: "m", p_nguon_goi: "x", p_phien_ban: null });
    assert.equal(r.ma, "KHONG_THUOC_TO_CHUC");
    const { rows } = await c.query(`SELECT count(*)::int AS n FROM tac_vu_chay WHERE vi_id = $1 AND user_id = $2`, [A.toChuc, B.quanTri]);
    assert.equal(rows[0].n, 0);
  });
});
```

- [ ] **Step 2: Chạy**

Run: `npx tsx --test tests/to-chuc-tan-cong.test.ts`
Expected: 11 PASS (các lớp chặn đã có từ Task 2–15). Bài nào hỏng = lỗ hổng thật: sửa mã / SQL ở task gốc, không nới bài kiểm.

- [ ] **Step 3: Commit**

```bash
git add tests/to-chuc-tan-cong.test.ts
git commit -m "test(to-chuc): kich ban tan cong - header gia, IDOR, can bo tu nang quyen, loi moi email khac, don khong phai quan tri

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 17: Kiểm cuối nhánh API, đẩy nhánh `dot-b-to-chuc`

**Files:**
- Không sửa mã (chỉ sửa nếu bước kiểm lộ lỗi; sửa ở task gốc rồi chạy lại).

- [ ] **Step 1: Migration chạy lại được trên CSDL đã có dữ liệu**

Bộ kiểm đã nạp 017–020 vào CSDL trống và chạy lại từng tệp một lần nữa (bài «chạy lại … lần hai»). Thêm một lần thủ công
đúng như lúc lên prod (một giao dịch, dừng ở lỗi đầu) trên một CSDL thử có dữ liệu cá nhân của đợt A:

Chạy bằng Git Bash từ thư mục worktree (psql nằm trong container, không cần cài ở PC):

```bash
CSDL="sudo docker exec -i vbp-csdl-thu-dot-b psql -U postgres -v ON_ERROR_STOP=1 -q"
ssh gikidi-dev "$CSDL -c 'CREATE DATABASE vbp_lan2'"
cat supabase/schema.sql supabase/migrations/002_orders.sql supabase/migrations/003_payment_settings.sql \
    supabase/migrations/004_credit.sql supabase/migrations/005_goi_nap.sql supabase/migrations/006_luu_ho_tro.sql \
    supabase/migrations/009_phien.sql supabase/migrations/010_kho.sql supabase/migrations/011_dung_luong_mua.sql \
    supabase/migrations/012_mau_phap_quy.sql supabase/du-lieu/012_mau_phap_quy_du_lieu.sql \
    supabase/migrations/013_hop_album.sql supabase/migrations/014_phien_cap_lai.sql supabase/migrations/015_mo_ban.sql \
    supabase/migrations/016_gioi_han_tep_goi.sql | ssh gikidi-dev "$CSDL -d vbp_lan2"
ssh gikidi-dev "$CSDL -d vbp_lan2 -c \"INSERT INTO users (email, full_name, password_hash, api_key) VALUES ('cu@vanbanplus.test','Cu','x','vbp_cu_thu_lan2'); SELECT credit_so_du(id) FROM users;\""
for lan in 1 2; do
  cat supabase/migrations/017_to_chuc.sql supabase/migrations/018_giao_viec.sql supabase/migrations/019_vi_credit.sql \
      supabase/migrations/020_don_to_chuc.sql | ssh gikidi-dev "$CSDL -1 -d vbp_lan2" && echo "lần $lan: xong"
done
ssh gikidi-dev "$CSDL -d vbp_lan2 -Atc 'SELECT vi_loai, vi_id = user_id FROM credit_lo'"
ssh gikidi-dev "$CSDL -c 'DROP DATABASE vbp_lan2 WITH (FORCE)'"
```

Expected: hai lần chạy 017–020 trong giao dịch đều không lỗi; dòng `credit_lo` cũ ra `ca_nhan|t`.

**Bản sao prod:** CSDL VanBanPlus **không** có trong `csdl-prod` (bộ nhớ L6). Kế hoạch này **không** chép, không chạy
trên bản sao — làm ở bước lên prod sau khi chủ dự án duyệt (spec §7).

- [ ] **Step 2: Toàn bộ bộ kiểm + kiểu + build**

```powershell
$env:TEST_DATABASE_URL="postgres://postgres:thu@127.0.0.1:55996/postgres"
npm test; if ($?) { npx tsc --noEmit }; if ($?) { npx next build --webpack }
```

Expected: `npm test` không bài nào FAIL / SKIP vì thiếu CSDL; tsc sạch; build xong. Soát đại từ nhân xưng trong chữ mới:

```bash
git diff origin/main --name-only -- src supabase | xargs grep -nE "\b([Tt]ôi|[Aa]nh|[Ee]m|[Bb]ạn|[Cc]hị|[Mm]ình|[Dd]ạ|[Vv]âng)\b|ạ[.,!? ]"
```

Expected: không dòng nào (bỏ qua khớp hợp lệ như «tiếng Anh»).

- [ ] **Step 3: Đẩy nhánh (KHÔNG đẩy `main`)**

```bash
git status --short          # sạch
git log --oneline origin/main..HEAD
git push -u origin dot-b-to-chuc
```

Expected: nhánh `dot-b-to-chuc` có trên origin; `main` không đổi. Báo controller: số bài kiểm, commit cuối, và nhắc
hợp đồng API ở đầu tệp này là nguồn duy nhất cho kế hoạch web.

- [ ] **Step 4: Tắt CSDL thử khi xong phiên**

```bash
ssh gikidi-dev "sudo docker rm -f vbp-csdl-thu-dot-b"
```

---

## Tự soát kế hoạch (đã làm khi viết)

- **Phủ spec:** §2.1 → T1–T3; §2.2 → T4–T6, T13; §2.3 → T7, T14; §2.4 → T1 (gói), T3 (hạn mức), T8, T15 (đơn) — nhắc
  gia hạn email hoãn; §3 → T4, T5, T9, T10; §4 → T11–T15; §6 → bài kiểm từng task + T16; §7 → ngoài phạm vi (không prod).
- **Không chỗ trống:** mọi bước mã có mã thật; các đoạn «giữ nguyên» chỉ dùng cho phần thân hàm có sẵn được nêu đích danh.
- **Tên nhất quán:** `p_nguoi` (SQL) ↔ `thamSoNguoi` (TS); `kho_vai_tro` trả `'chu'` cho cá nhân; `Vi` / `vi_loai` / `vi_id`;
  `ToChucTomTat`, `ToChuc`, `GiaoViec`, `ViecDuocGiao` khớp giữa SQL (`to_chuc_json`, `kho_giao_viec_json`) và hợp đồng.

