# Đợt 2c — gói nạp credit, lưu tùy chọn để hỗ trợ, bỏ đường tự về AI Studio cho v2 — kế hoạch triển khai

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bán gói nạp credit 90 ngày (cấp lô ngay trong lúc xác nhận thanh toán), cho người dùng tự bật lưu mã hóa 7 ngày để được hỗ trợ (có nhật ký mở xem và lịch dọn kiểm được), và chặn tác vụ v2 rơi sang AI Studio bậc miễn phí khi Vertex lỗi.

**Architecture:** Mọi thay đổi số dư vẫn nằm trong hàm Postgres: `pay_order` (002) được thay bằng bản rẽ nhánh theo `orders.loai`, đơn `nap` gọi `credit_cap_nap` trong cùng giao dịch, nên webhook SePay và xác nhận tay (cả hai đều gọi `pay_order`) cùng được cấp lô mà không phải sửa luồng gọi. Lưu hỗ trợ mã hóa AES-256-GCM ở tầng ứng dụng (khóa chỉ ở biến môi trường), CSDL chỉ giữ bản mã; mọi ghi / xóa / mở bản lưu đi qua hàm `SECURITY DEFINER` để không thể đọc bản mã mà không để lại nhật ký. Luồng `chayTacVu` nhận thêm một phụ thuộc TÙY CHỌN `luuHoTro`, gọi sau khi chốt thành công, lỗi thì nuốt. Đường v2 gọi `callAI` với cờ `chiVertex`, `sendGemini` thấy cờ thì không bao giờ đi AI Studio.

**Tech Stack:** Next.js 16 (App Router), TypeScript, Supabase JS (service role), Postgres 17 (ảnh `supabase/postgres:17.6.1.155`), `tsx --test`, `pg` (chỉ trong bộ kiểm), `node:crypto` (AES-256-GCM), Coolify API (`D:\Thang\gikidi-server\chuyen\coolify.py`).

**Spec:** [docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md](../specs/2026-11-09-api-tac-vu-ai-credit-design.md) — mục 7 («Mức cấp», «Lược đồ», «Cấp khi thanh toán»), mục 8, mục 9 («mở bản lưu»), mục 13.1 và 13.3.
**Hợp đồng dùng chung:** [2026-11-16-hop-dong-tac-vu-v2.md](2026-11-16-hop-dong-tac-vu-v2.md) (mục «Số migration»).
**Brief điều phối:** `D:/AIVanBanCaNhan/.superpowers/lap-ke-hoach-dot2-3.md`.

## Phạm vi và điều kiện bắt đầu

- Chạy SAU khi `dot2a` và `dot2b` đã gộp vào `main` của `vanbanplus-api`. Hai đợt đó chỉ thêm tác vụ vào `dang-ky.ts`, hàm sinh mẫu C#,
  và migration `007_bang_gia_dot2a.sql` / `008_bang_gia_dot2b.sql`; `dot2b` mở rộng `PromptTacVu` trong `chay-tac-vu.ts`.
  Kế hoạch này viết khi CHƯA thấy mã của hai đợt đó: mọi chỗ sửa tệp dùng chung đều tả bằng mốc văn bản, không bằng số dòng.
  Mốc nào không tìm thấy nguyên văn thì đọc tệp hiện tại, đặt đoạn mới vào vị trí tương đương, giữ nguyên phần `dot2a`/`dot2b` đã thêm.
- Không tạo thêm tác vụ, không đụng `dang-ky.ts`, `AIVanBan.Tests/MauPromptTests.cs`, `src/utils/constants.ts`.
- Không đụng mã C# (bản cài máy là việc của `dot3`).

## Điểm cần chủ dự án quyết

1. **Trước khi migrate prod (Task 13 Step 4):** báo một dòng, chờ đồng ý (lệnh «làm đi» cho cả kế hoạch được tính là đồng ý, trừ khi chủ dự án dặn khác).
2. **Cách thử thanh toán gói nạp trên prod (Task 14 Step 4):** mặc định xác nhận tay đơn 49.000đ của tài khoản quản trị (không có tiền thật, đơn
   hiện «đã thanh toán (xác nhận tay)» trong danh sách đơn); chỉ chuyển khoản thật 49.000đ để thử trọn đường SePay → webhook khi chủ dự án đồng ý.
3. **Lịch dọn bản lưu (Task 11):** chọn theo kết quả kiểm `pg_cron` trên prod, không cần hỏi; ghi lại nhánh đã chọn.

## Global Constraints

- Repo mã: `D:\AIVanBanCaNhan\vanbanplus-api` (git riêng, remote `ericphan28/vanbanplus-api`). Làm trên nhánh **`dot2c`** tách từ `main` mới nhất; chỉ gộp vào `main` ở Task 14.
- **Đẩy `main` của `vanbanplus-api` = lên production** (webhook GitHub → Coolify). Chỉ đẩy khi `npm test` (kèm CSDL thử) và `npx next build` đều xanh.
- **Migrate prod TRƯỚC khi đẩy mã cần lược đồ mới.** Thử trên bản sao prod trước (chạy 2 lần), prod chạy `psql -1 -v ON_ERROR_STOP=1` bằng `postgres`, xong `NOTIFY pgrst, 'reload schema'`.
- CSDL prod: container `supabase-db-q323olywghtnjcvpyvtjzo9v` (`ssh gikidi-dev`, `sudo docker exec …`). App Coolify: `wfxjk8gpdrwfanx6rxi28sqz` (theo `D:\Thang\gikidi-server\chuyen\vercel-app.json`, mục `vanbanplus-api`; Task 13 kiểm lại tên trước khi ghi biến).
- Không in khóa, chuỗi kết nối, API key, `TAC_VU_LUU_KHOA` ra chat hay log. Bí mật chỉ ở `C:\Users\PC\.gikidi\` và bản chép `/home/dev/.gikidi/` (600).
- Migration theo hợp đồng: chỉ đợt này tạo **`005_goi_nap.sql`** và **`006_luu_ho_tro.sql`**. Tên cột / bảng đúng hợp đồng: `orders.loai` (`'goi' | 'nap'`, mặc định `'goi'`), `orders.goi_nap_ma`, `tac_vu_luu`, `users.cho_phep_luu_ho_tro`.
- **Luật khóa của 004:** hàm đổi số dư khóa dòng `users` trước bằng `FOR NO KEY UPDATE`, rồi mới tới `tac_vu_chay` / `credit_lo`. Không hàm mới nào khóa `users` bằng `FOR UPDATE`.
- Bảng mới: bật RLS, không policy; `REVOKE ALL … FROM PUBLIC, anon, authenticated` (Supabase tự cấp ALL cho bảng / dãy số / hàm mới); dãy số `BIGSERIAL` thu hồi tường minh như `credit_so_id_seq`; hàm trigger thu hồi `EXECUTE`.
- Gói nạp (spec §7): **49.000đ = 160 credit, 99.000đ = 350, 199.000đ = 750, dùng trong 90 ngày**; là các dòng `credit_bang_gia` `loai='goi_nap'`, mã `nap-49` / `nap-99` / `nap-199`, `gia_tri = {gia_vnd, credit, so_ngay}` (đã có từ 004).
- Đơn `nap` đã trả **cấp lô 90 ngày trong cùng thao tác xác nhận (webhook SePay và admin), an toàn khi gọi lại**. Đơn `goi` giữ nguyên hành vi: lô cấp ở lượt gọi kế tiếp.
- Spec §8: công tắc `cho_phep_luu_ho_tro` mặc định **tắt**; tắt thì **không có bản ghi `tac_vu_luu`**; bật thì đầu vào + kết quả mã hóa **AES-256-GCM**, khóa **`TAC_VU_LUU_KHOA`** ở biến môi trường prod, **`het_han` = 7 ngày**; xóa khi hết hạn bằng `pg_cron` nếu có, không thì tác vụ định kỳ gọi đường nội bộ, **phải kiểm được đã chạy**; quản trị mở xem thì **ghi nhật ký ai, lúc nào, lượt nào**; chính sách ghi rõ bản mã còn trong bản sao lưu cũ thêm một thời gian.
- Spec §13.1: tác vụ v2 **không bao giờ** chuyển sang AI Studio; Vertex lỗi thì khách nhận `AI_QUA_TAI` (503) hoặc `AI_LOI` (502) và được hoàn credit. Đường cũ `/api/ai/*` giữ nguyên hành vi.
- Lời hiện cho khách (trang web, thông báo lỗi, chính sách): tiếng Việt có dấu, **không đại từ nhân xưng** (tôi, bạn, mình, anh, chị, em, chúng tôi, ta), không «dạ / vâng / ạ». Soát bằng:
  `grep -nE "\b([Tt]ôi|[Tt]a|[Aa]nh|[Ee]m|[Bb]ạn|[Cc]hị|[Mm]ình|[Dd]ạ|[Vv]âng)\b|ạ[.,!? ]" <tệp>` (bỏ qua «tiếng Anh»).
- Không ghi nội dung văn bản vào log: chỉ mã lượt, số byte, thông điệp lỗi kỹ thuật.
- CSDL thử: `bash scripts/csdl-thu.sh bat` (trên gikidi-dev) + đường hầm `ssh -N -L 55990:127.0.0.1:55990 gikidi-dev`;
  `TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres`.
- Commit kết thúc bằng một dòng trống rồi `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.

## Sơ đồ tệp

```
vanbanplus-api/
  supabase/migrations/005_goi_nap.sql             orders.loai/goi_nap_ma + check, uq_credit_lo_nap, credit_cap_nap, pay_order mới (Task 1)
  supabase/migrations/006_luu_ho_tro.sql          cho_phep_luu_ho_tro, tac_vu_luu, nhật ký mở xem, nhật ký dọn, 4 hàm (Task 6)
  supabase/lich/006-don-luu-ho-tro-pg-cron.sql    CHỈ nhánh A của Task 11: lịch pg_cron, chạy tay trên prod
  tests/ho-tro/csdl-thu.ts                        thêm 005, 006 vào danh sách nạp (Task 1, 6)
  tests/goi-nap-csdl.test.ts                      (Task 1)
  tests/luu-ho-tro-csdl.test.ts                   (Task 6)
  tests/bang-gia.test.ts                          thêm bài gói nạp (Task 2)
  tests/don-nap.test.ts                           tenDon / orderView (Task 3)
  tests/gemini-transport.test.ts                  thêm bài chiVertex (Task 5)
  tests/phu-thuoc.test.ts                         taoGoiAIv2 (Task 5)
  tests/luu-ho-tro.test.ts                        mã hóa + taoLuuHoTro + docBanLuu (Task 7)
  tests/chay-tac-vu.test.ts                       thêm bài lưu hỗ trợ (Task 8)
  tests/noi-bo.test.ts                            CHỈ nhánh B của Task 11
  src/lib/credit/bang-gia.ts                      GoiNap, kiemGoiNap, docGoiNap; dungBangGia kiểm dòng goi_nap (Task 2)
  src/app/api/goi-nap/route.ts                    GET công khai danh sách gói nạp (Task 2)
  src/lib/orders.ts                               OrderRow mới, tenDon, napTheoMa, orderView(…, nap) (Task 3)
  src/app/api/orders/route.ts, src/app/api/orders/[code]/route.ts   tạo / xem đơn nạp (Task 3)
  src/app/api/webhooks/sepay/route.ts             lời báo Telegram cho đơn nạp (Task 3)
  src/app/api/admin/orders/route.ts, src/app/admin/orders/page.tsx  cột loại đơn (Task 3)
  src/app/thanh-toan/page.tsx, src/app/bang-gia/{page,BangGiaClient}.tsx, src/app/dang-ky/DangKyClient.tsx   (Task 4)
  src/app/tai-khoan/page.tsx                      gói nạp (Task 4), công tắc lưu hỗ trợ (Task 9)
  src/lib/gemini-transport.ts, src/lib/ai-providers.ts, src/types/index.ts   chiVertex (Task 5)
  src/lib/tac-vu/phu-thuoc.ts                     taoGoiAIv2 (Task 5), luuHoTro (Task 8)
  src/lib/luu-ho-tro/ma-hoa.ts                    AES-256-GCM thuần (Task 7)
  src/lib/luu-ho-tro/kho.ts                       taoLuuHoTro, docBanLuu, gọi hàm Postgres (Task 7)
  src/lib/tac-vu/chay-tac-vu.ts                   PhuThuoc.luuHoTro tùy chọn + BanLuu (Task 8)
  src/app/api/tai-khoan/luu-ho-tro/route.ts       bật / tắt (Task 9)
  src/app/api/auth/[action]/route.ts              /me trả choPhepLuuHoTro (Task 9)
  src/app/api/admin/credit/route.ts, src/app/admin/credit/page.tsx   mở bản lưu, nhật ký, trạng thái dọn (Task 10)
  src/lib/noi-bo.ts, src/app/api/noi-bo/don-luu-ho-tro/route.ts      CHỈ nhánh B của Task 11
  src/app/chinh-sach-bao-mat/page.tsx, src/app/huong-dan/page.tsx    (Task 12)
D:\Thang\gikidi-server\THAY-DOI.md                một mục (Task 13)
D:\AIVanBanCaNhan\PROJECT_STATUS.md, spec dòng 3   (Task 14)
```

## Quyết định đã chốt khi lập kế hoạch (đọc trước khi làm)

**Mở rộng `pay_order` hay hàm riêng?** Cả hai: `credit_cap_nap(order)` là hàm riêng (kiểm được riêng, gọi lại an toàn), và `pay_order`
gọi nó trong cùng giao dịch khi `o.loai = 'nap'`. Webhook SePay (`src/app/api/webhooks/sepay/route.ts`) và xác nhận tay
(`src/app/api/admin/orders/route.ts`) đều chỉ gọi `rpc("pay_order")`, nên cả hai tự có cấp lô; không đường nào cần gọi thêm hàm.
Lô + dòng sổ `cap` + đơn chuyển `paid` cùng một giao dịch: lỗi ở bất kỳ bước nào thì đơn vẫn `pending`, webhook trả 500, SePay gửi lại.

**Khóa chéo với 004:** `pay_order` cũ khóa `orders FOR UPDATE` rồi `users FOR UPDATE`. `FOR UPDATE` chặn `FOR KEY SHARE` mà khóa ngoại
`credit_lo.user_id` cần khi `credit_cap_ky` (không khóa `users`) chèn lô kỳ. Hiện chưa khóa chéo vì `pay_order` không đụng `credit_lo`, nhưng
khi `pay_order` chèn lô nạp thì có thể: giao dịch A (`credit_so_du` → `credit_cap_ky`) đã chèn dòng `credit_lo` và chờ `KEY SHARE` trên
`users` do B (`pay_order`) giữ, trong khi B chèn `credit_lo`/`credit_so`… Chữa tận gốc: `pay_order` khóa `users` bằng **`FOR NO KEY UPDATE`**
(đúng luật 004; câu `UPDATE users SET subscription_*` cũng chỉ lấy khóa này vì không đổi cột khóa). `NO KEY UPDATE` không chặn
`KEY SHARE`, vẫn chặn các hàm 004 nên mọi thay đổi số dư của một người vẫn xếp hàng. Thứ tự khóa của `pay_order`: `orders` → `users` →
chèn lô; không hàm nào khóa `users` rồi mới khóa `orders` (lô kỳ chèn với `order_id = NULL` nên không chạm `orders`), nên không có vòng.
`credit_cap_nap` không gọi `credit_cap_ky` (gọi thì tranh khóa chỉ mục `uq_credit_lo_ky` với A → khóa chéo). Task 1 có bài kiểm tách
bạch hai kiểu khóa và bài chạy đồng thời.

**Giá gói lúc trả tiền:** `amount_vnd` chụp giá lúc tạo đơn (như đơn gói); số credit và số ngày đọc từ `credit_bang_gia` lúc cấp lô. Dòng
gói nạp bị xóa khỏi bảng giá thì `pay_order` báo lỗi (đơn giữ `pending`, admin nhận cảnh báo «lỗi máy chủ»), không cấp bừa.

**Kiểm dạng `goi_nap` trong `dungBangGia`:** bảng giá có dòng gói nạp sai dạng thì `dungBangGia` ném lỗi — trang `/admin/credit` từ chối lưu
(đã dùng `dungBangGia` để kiểm trước khi ghi). `BangGia` KHÔNG thêm trường (các bộ kiểm của `dot2a`/`dot2b` dựng `BangGia` bằng tay sẽ hỏng
khi `next build` kiểm kiểu); danh sách gói nạp đọc riêng bằng `docGoiNap()`.

**Lưu hỗ trợ:** khóa 32 byte, IV 12 byte ngẫu nhiên mỗi lần, thẻ xác thực 16 byte nối cuối bản mã, `luot_id` làm AAD (bản mã không dời
sang lượt khác được). CSDL nhận / trả base64. `service_role` chỉ được `SELECT (luot_id, user_id, het_han, tao_luc)`; đọc bản mã duy nhất
qua `tac_vu_luu_mo`, hàm này ghi nhật ký trước khi trả. Bản rõ quá 8 MB (vd `trich-xuat` nhiều tệp) thì không lưu, chỉ ghi cảnh báo.

**§13.1:** cờ `chiVertex` đi `PhuThuoc.goiAI` → `AICallOptions` → `sendGemini`. Có cờ: thiếu cấu hình Vertex hoặc không lấy được token thì
ném lỗi; Vertex trả lỗi thì trả nguyên phản hồi lỗi (sau 3 lần thử của `fetchWithTransientRetry`) để `callGeminiProvider` ném
`AI API error <mã>`; `chayTacVu` sẵn có đổi `429` → `AI_QUA_TAI`, còn lại → `AI_LOI`, cả hai đều hoàn credit. Không cờ: y như cũ.

---

### Task 1: Migration 005 — đơn nạp, `credit_cap_nap`, `pay_order` mới

**Files:**
- Create: `vanbanplus-api/supabase/migrations/005_goi_nap.sql`
- Modify: `vanbanplus-api/tests/ho-tro/csdl-thu.ts` (danh sách `TEP_LUOC_DO`)
- Test: `vanbanplus-api/tests/goi-nap-csdl.test.ts`

**Interfaces:**
- Consumes: bảng / hàm của 002 (`orders`, `pay_order`), 004 (`credit_lo`, `credit_so`, `credit_bang_gia` với 3 dòng `goi_nap`, `credit_so_du`, `tac_vu_mo`, `credit_chot`, `credit_dieu_chinh`).
- Produces (SQL):
  - `orders.loai TEXT NOT NULL DEFAULT 'goi'`, `orders.goi_nap_ma TEXT`, `plan_id` / `cycle` nullable, ràng buộc `ck_orders_loai`.
  - Chỉ mục `uq_credit_lo_nap ON credit_lo(order_id) WHERE nguon = 'nap'`.
  - `credit_cap_nap(p_order UUID) RETURNS JSONB` → `{lo_id, so_cap, het_han}` hoặc `NULL` (đơn chưa trả / không phải nạp).
  - `pay_order(p_code TEXT, p_source TEXT, p_ref TEXT) RETURNS TEXT` — cùng chữ ký, cùng kết quả (`paid` · `already_paid` · `cancelled` · `not_found`); đơn nạp: `period_start` = lúc trả, `period_end` = hạn lô.

- [ ] **Step 1: Chuẩn bị nhánh, kiểm điều kiện bắt đầu**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git status --short && git checkout main && git pull --ff-only && git log --oneline -8 && ls supabase/migrations && git checkout -b dot2c
```
Expected: cây sạch (thay đổi lạ của phiên khác thì dừng, hỏi); `git log` thấy hai lần gộp `dot2a`, `dot2b`; `supabase/migrations` có `001`–`004`, `007_bang_gia_dot2a.sql`, `008_bang_gia_dot2b.sql`, CHƯA có `005`, `006`. Thiếu `007`/`008` nghĩa là `dot2a`/`dot2b` chưa gộp: dừng, báo người điều phối.

Dựng CSDL thử và mở đường hầm (cửa sổ riêng, để chạy suốt kế hoạch):
```bash
bash scripts/csdl-thu.sh bat
ssh -N -L 55990:127.0.0.1:55990 gikidi-dev    # chạy nền
```

- [ ] **Step 2: Thêm 005 vào danh sách nạp của CSDL thử**

Trong `tests/ho-tro/csdl-thu.ts`, mảng `TEP_LUOC_DO`: thêm `"migrations/005_goi_nap.sql",` ngay SAU dòng `"migrations/004_credit.sql",` (trước dòng `007`/`008` nếu `dot2a`/`dot2b` đã thêm). Sửa chú thích trên hàm `taoCsdlThu` thành `/** Tạo CSDL mới, nạp lược đồ + các migration cần cho credit, gói nạp, lưu hỗ trợ. 001 (cloud) không cần nên bỏ. */`.

- [ ] **Step 3: Viết bộ kiểm hỏng trước**

Tạo `tests/goi-nap-csdl.test.ts`:

```ts
import { after, before, describe, test } from "node:test";
import assert from "node:assert/strict";
import { randomBytes } from "node:crypto";
import { Client } from "pg";
import { CO_CSDL_THU, rpc, taoCsdlThu, taoNguoiDung } from "./ho-tro/csdl-thu";

const NGAY = 24 * 3600 * 1000;
const BANG_CHU = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
const maDon = () => "VBP" + Array.from(randomBytes(8), (b) => BANG_CHU[b % BANG_CHU.length]).join("");

describe("gói nạp: đơn 'nap', pay_order cấp lô 90 ngày", { skip: !CO_CSDL_THU }, () => {
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

  /** Mỗi lô: so_con phải bằng tổng sổ cái của lô đó. */
  async function kiemSoCai(user: string) {
    const { rows } = await c.query(
      `SELECT l.id, l.so_con, COALESCE(SUM(s.so), 0)::int AS tong
         FROM credit_lo l LEFT JOIN credit_so s ON s.lo_id = l.id
        WHERE l.user_id = $1 GROUP BY l.id, l.so_con`,
      [user]
    );
    for (const r of rows) assert.equal(r.so_con, r.tong, `lô ${r.id} lệch sổ cái`);
  }

  /** Mỗi lời gọi một kết nối riêng để các giao dịch thật sự chồng lên nhau. */
  async function chayDongThoi<T>(n: number, viec: (k: Client, i: number) => Promise<T>): Promise<T[]> {
    const ketNoi = await Promise.all(
      Array.from({ length: n }, async () => {
        const k = new Client({ connectionString: csdl.url });
        await k.connect();
        return k;
      })
    );
    try {
      return await Promise.all(ketNoi.map((k, i) => viec(k, i)));
    } finally {
      await Promise.all(ketNoi.map((k) => k.end()));
    }
  }

  async function taoDonNap(user: string, ma = "nap-49", tien = 49000): Promise<string> {
    const code = maDon();
    await c.query("INSERT INTO orders (code, user_id, loai, goi_nap_ma, amount_vnd) VALUES ($1, $2, 'nap', $3, $4)", [code, user, ma, tien]);
    return code;
  }
  async function taoDonGoi(user: string, plan = "starter", cycle = "month", tien = 99000): Promise<string> {
    const code = maDon();
    await c.query("INSERT INTO orders (code, user_id, plan_id, cycle, amount_vnd) VALUES ($1, $2, $3, $4, $5)", [code, user, plan, cycle, tien]);
    return code;
  }
  const loNap = async (code: string) =>
    (
      await c.query(
        `SELECT l.so_cap, l.so_con, l.het_han = o.paid_at + interval '90 days' AS dung_han,
                l.user_id = o.user_id AS dung_nguoi, o.period_end = l.het_han AS dung_period_end
           FROM credit_lo l JOIN orders o ON o.id = l.order_id WHERE o.code = $1 AND l.nguon = 'nap'`,
        [code]
      )
    ).rows;
  const demCap = async (code: string): Promise<number> =>
    (
      await c.query(
        `SELECT count(*)::int AS n FROM credit_so s JOIN credit_lo l ON l.id = s.lo_id JOIN orders o ON o.id = l.order_id
          WHERE o.code = $1 AND s.loai = 'cap'`,
        [code]
      )
    ).rows[0].n;
  const mo = (u: string, min: number, max: number, cl: Client = c) =>
    rpc(cl, "tac_vu_mo", u, "thu", min, max, "gemini", "gemini-3.6-flash", "kiem-thu", "0.0.0");

  test("ràng buộc loại đơn: 'goi' cần plan_id + cycle, 'nap' cần goi_nap_ma và không có plan_id / cycle", async () => {
    const u = await taoNguoiDung(c);
    await assert.rejects(c.query("INSERT INTO orders (code, user_id, loai, amount_vnd) VALUES ($1, $2, 'nap', 49000)", [maDon(), u]), /ck_orders_loai/);
    await assert.rejects(
      c.query("INSERT INTO orders (code, user_id, loai, goi_nap_ma, plan_id, cycle, amount_vnd) VALUES ($1, $2, 'nap', 'nap-49', 'starter', 'month', 49000)", [maDon(), u]),
      /ck_orders_loai/
    );
    await assert.rejects(c.query("INSERT INTO orders (code, user_id, amount_vnd) VALUES ($1, $2, 99000)", [maDon(), u]), /ck_orders_loai/);
    await assert.rejects(
      c.query("INSERT INTO orders (code, user_id, loai, plan_id, cycle, amount_vnd) VALUES ($1, $2, 'khac', 'starter', 'month', 99000)", [maDon(), u]),
      /ck_orders_loai/
    );
    const code = await taoDonGoi(u);
    assert.equal((await c.query("SELECT loai FROM orders WHERE code = $1", [code])).rows[0].loai, "goi");
  });

  test("trả đơn nạp: cấp lô 'nap' 160 credit hạn 90 ngày gắn đơn, ghi sổ 'cap', không đổi gói", async () => {
    const u = await taoNguoiDung(c);
    const code = await taoDonNap(u);
    assert.equal(await rpc(c, "pay_order", code, "sepay", "FT123"), "paid");
    assert.deepEqual(await loNap(code), [{ so_cap: 160, so_con: 160, dung_han: true, dung_nguoi: true, dung_period_end: true }]);
    const { rows: so } = await c.query(
      `SELECT s.loai, s.so, s.boi, s.ghi_chu FROM credit_so s JOIN credit_lo l ON l.id = s.lo_id
        WHERE l.nguon = 'nap' AND l.user_id = $1`,
      [u]
    );
    assert.deepEqual(so, [{ loai: "cap", so: 160, boi: "sepay", ghi_chu: `Gói nạp nap-49 · đơn ${code}` }]);
    const { rows: nd } = await c.query("SELECT subscription_plan_id, subscription_end_date FROM users WHERE id = $1", [u]);
    assert.deepEqual(nd[0], { subscription_plan_id: "free", subscription_end_date: null });
    const { rows: don } = await c.query("SELECT status, paid_source, paid_ref, period_start = paid_at AS dung_dau FROM orders WHERE code = $1", [code]);
    assert.deepEqual(don[0], { status: "paid", paid_source: "sepay", paid_ref: "FT123", dung_dau: true });
    assert.equal((await rpc(c, "credit_so_du", u)).tong, 190);
    await kiemSoCai(u);
  });

  test("gói 199: 750 credit, cũng 90 ngày", async () => {
    const u = await taoNguoiDung(c);
    const code = await taoDonNap(u, "nap-199", 199000);
    assert.equal(await rpc(c, "pay_order", code, "admin", "admin@test"), "paid");
    assert.deepEqual(await loNap(code), [{ so_cap: 750, so_con: 750, dung_han: true, dung_nguoi: true, dung_period_end: true }]);
    await kiemSoCai(u);
  });

  test("trả lại đơn đã trả (SePay gửi lại, admin xác nhận thêm, gọi thẳng credit_cap_nap): không cấp thêm", async () => {
    const u = await taoNguoiDung(c);
    const code = await taoDonNap(u);
    assert.equal(await rpc(c, "pay_order", code, "sepay", "FT1"), "paid");
    const lan1 = await c.query("SELECT id FROM credit_lo WHERE user_id = $1 AND nguon = 'nap'", [u]);
    assert.equal(await rpc(c, "pay_order", code, "sepay", "FT1"), "already_paid");
    assert.equal(await rpc(c, "pay_order", code, "admin", "admin@test"), "already_paid");
    const id = (await c.query("SELECT id FROM orders WHERE code = $1", [code])).rows[0].id;
    const goiThang = await rpc(c, "credit_cap_nap", id);
    assert.equal(goiThang.lo_id, lan1.rows[0].id);
    assert.equal((await c.query("SELECT count(*)::int AS n FROM credit_lo WHERE user_id = $1 AND nguon = 'nap'", [u])).rows[0].n, 1);
    assert.equal(await demCap(code), 1);
    assert.equal((await rpc(c, "credit_so_du", u)).tong, 190);
    await kiemSoCai(u);
  });

  test("webhook gửi lại đồng thời với xác nhận tay, trả đơn gói, xem số dư, mở lượt, điều chỉnh: đúng một lô nạp, không khóa chéo, sổ khớp", async () => {
    for (let vong = 0; vong < 8; vong++) {
      const u = await taoNguoiDung(c);
      const nap = await taoDonNap(u);
      const goi = await taoDonGoi(u);
      const kq = await chayDongThoi(12, (k, i) => {
        if (i % 6 === 0) return rpc(k, "pay_order", nap, "sepay", `FT-${vong}`);
        if (i % 6 === 1) return rpc(k, "pay_order", nap, "admin", "admin@test");
        if (i % 6 === 2) return rpc(k, "pay_order", goi, "admin", "admin@test");
        if (i % 6 === 3) return mo(u, 1, 10, k);
        if (i % 6 === 4) return rpc(k, "credit_dieu_chinh", u, -5, "đồng thời", "admin@test");
        return rpc(k, "credit_so_du", u);
      });
      const traNap = kq.filter((_, i) => i % 6 === 0 || i % 6 === 1);
      assert.equal(traNap.filter((r) => r === "paid").length, 1, "đúng một lần trả thành công");
      assert.equal(traNap.filter((r) => r === "already_paid").length, traNap.length - 1);
      assert.equal(await demCap(nap), 1);
      assert.equal((await c.query("SELECT count(*)::int AS n FROM credit_lo WHERE user_id = $1 AND nguon = 'nap'", [u])).rows[0].n, 1);
      assert.equal((await c.query("SELECT subscription_plan_id FROM users WHERE id = $1", [u])).rows[0].subscription_plan_id, "starter");
      const { rows } = await c.query("SELECT count(*)::int AS am FROM credit_lo WHERE user_id = $1 AND so_con < 0", [u]);
      assert.equal(rows[0].am, 0);
      await kiemSoCai(u);
    }
  });

  test("lô nạp vào thứ tự hết hạn cùng lô tháng: lô tháng hết hạn sớm hơn bị trừ trước", async () => {
    const u = await taoNguoiDung(c);
    await rpc(c, "credit_so_du", u); // lô kỳ free 30
    const code = await taoDonNap(u);
    await rpc(c, "pay_order", code, "sepay", "FT2");
    const m = await mo(u, 5, 40);
    assert.equal(m.ma, "ok");
    assert.equal(m.so_du, 150);
    assert.deepEqual(await rpc(c, "credit_chot", m.luot_id, 35, 1, 1, false), { credit_tru: 35, so_du: 155, trang_thai: "xong" });
    const { rows } = await c.query("SELECT nguon, so_con FROM credit_lo WHERE user_id = $1 ORDER BY het_han", [u]);
    assert.deepEqual(rows, [{ nguon: "goi_thang", so_con: 0 }, { nguon: "nap", so_con: 155 }]);
    await kiemSoCai(u);
  });

  test("lô nạp hết hạn sớm hơn lô tháng thì lô nạp bị trừ trước", async () => {
    const u = await taoNguoiDung(c, { plan: "pro", ketThuc: new Date(Date.now() + 30 * NGAY) });
    await rpc(c, "credit_so_du", u);
    const code = await taoDonNap(u);
    await rpc(c, "pay_order", code, "sepay", "FT3");
    await c.query("UPDATE credit_lo SET het_han = NOW() + interval '2 days' WHERE user_id = $1 AND nguon = 'nap'", [u]);
    const m = await mo(u, 5, 20);
    await rpc(c, "credit_chot", m.luot_id, 20, 1, 1, false);
    const { rows } = await c.query("SELECT nguon, so_con FROM credit_lo WHERE user_id = $1 ORDER BY het_han", [u]);
    assert.deepEqual(rows, [{ nguon: "nap", so_con: 140 }, { nguon: "goi_thang", so_con: 1100 }]);
    await kiemSoCai(u);
  });

  test("gói nạp không có trong bảng giá: pay_order báo lỗi, đơn vẫn chờ, không có lô", async () => {
    const u = await taoNguoiDung(c);
    const code = await taoDonNap(u, "nap-khong-co", 10000);
    await assert.rejects(rpc(c, "pay_order", code, "sepay", "FT4"), /không có gói nạp/);
    assert.equal((await c.query("SELECT status FROM orders WHERE code = $1", [code])).rows[0].status, "pending");
    assert.equal((await c.query("SELECT count(*)::int AS n FROM credit_lo WHERE user_id = $1 AND nguon = 'nap'", [u])).rows[0].n, 0);
  });

  test("đơn nạp đã hủy: SePay nhận 'cancelled', admin xác nhận thì cấp lô", async () => {
    const u = await taoNguoiDung(c);
    const code = await taoDonNap(u);
    await c.query("UPDATE orders SET status = 'cancelled' WHERE code = $1", [code]);
    assert.equal(await rpc(c, "pay_order", code, "sepay", "FT5"), "cancelled");
    assert.equal(await demCap(code), 0);
    assert.equal(await rpc(c, "pay_order", code, "admin", "admin@test"), "paid");
    assert.equal(await demCap(code), 1);
    await kiemSoCai(u);
  });

  test("credit_cap_nap với đơn chưa trả hoặc đơn gói: trả NULL, không cấp", async () => {
    const u = await taoNguoiDung(c);
    const nap = await taoDonNap(u);
    const goi = await taoDonGoi(u);
    await rpc(c, "pay_order", goi, "admin", "admin@test");
    for (const code of [nap, goi]) {
      const id = (await c.query("SELECT id FROM orders WHERE code = $1", [code])).rows[0].id;
      assert.equal(await rpc(c, "credit_cap_nap", id), null);
    }
    assert.equal((await c.query("SELECT count(*)::int AS n FROM credit_lo WHERE user_id = $1 AND nguon = 'nap'", [u])).rows[0].n, 0);
  });

  test("đơn gói giữ hành vi cũ: kích hoạt gói, pay_order không chèn lô, lô kỳ cấp ở lượt gọi sau", async () => {
    const u = await taoNguoiDung(c);
    const code = await taoDonGoi(u, "starter", "month", 99000);
    assert.equal(await rpc(c, "pay_order", code, "sepay", "FT6"), "paid");
    const { rows } = await c.query(
      `SELECT u.subscription_plan_id, u.subscription_end_date = o.period_end AS dung_han,
              o.period_end BETWEEN NOW() + interval '27 days' AND NOW() + interval '32 days' AS mot_thang
         FROM users u JOIN orders o ON o.user_id = u.id WHERE o.code = $1`,
      [code]
    );
    assert.deepEqual(rows[0], { subscription_plan_id: "starter", dung_han: true, mot_thang: true });
    assert.equal((await c.query("SELECT count(*)::int AS n FROM credit_lo WHERE user_id = $1", [u])).rows[0].n, 0);
    assert.equal((await rpc(c, "credit_so_du", u)).tong, 400);
  });

  test("pay_order khóa users bằng NO KEY UPDATE: chèn lô (khóa ngoại cần KEY SHARE) không bị chặn khi đang thanh toán", async () => {
    const u = await taoNguoiDung(c);
    const code = await taoDonGoi(u);
    const k1 = new Client({ connectionString: csdl.url });
    const k2 = new Client({ connectionString: csdl.url });
    await k1.connect();
    await k2.connect();
    try {
      await k1.query("BEGIN");
      assert.equal(await rpc(k1, "pay_order", code, "admin", "admin@test"), "paid");
      await k2.query("SET lock_timeout = '2s'");
      // Bản cũ (FOR UPDATE) làm lệnh này chờ rồi báo «lock timeout».
      await k2.query(
        "INSERT INTO credit_lo (user_id, nguon, so_cap, so_con, het_han) VALUES ($1, 'tang', 1, 1, NOW() + interval '1 day')",
        [u]
      );
      await k1.query("ROLLBACK");
    } finally {
      await k1.end();
      await k2.end();
    }
  });

  test("quyền: anon, authenticated không gọi được pay_order, credit_cap_nap; service_role gọi được", async () => {
    for (const ham of ["pay_order(text,text,text)", "credit_cap_nap(uuid)"]) {
      for (const v of ["anon", "authenticated"]) {
        const { rows } = await c.query("SELECT has_function_privilege($1, $2, 'EXECUTE') AS co", [v, ham]);
        assert.equal(rows[0].co, false, `${v} có EXECUTE trên ${ham}`);
      }
      const { rows } = await c.query("SELECT has_function_privilege('service_role', $1, 'EXECUTE') AS co", [ham]);
      assert.equal(rows[0].co, true, `service_role thiếu EXECUTE trên ${ham}`);
    }
  });
});
```

- [ ] **Step 4: Chạy để thấy hỏng**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npx tsx --test tests/goi-nap-csdl.test.ts
```
Expected: FAIL ngay ở `before` vì `ENOENT … 005_goi_nap.sql` (tệp chưa có).

- [ ] **Step 5: Viết migration**

Tạo `supabase/migrations/005_goi_nap.sql`:

```sql
-- ============================================================
-- 005: Gói nạp credit (spec mục 7 «Mức cấp», «Lược đồ», «Cấp khi thanh toán»)
-- orders thêm loai ('goi' | 'nap') + goi_nap_ma; plan_id / cycle thành nullable kèm ràng buộc theo loại.
-- Đơn 'nap' đã trả: cấp lô 'nap' (hạn so_ngay ngày của gói, mặc định 90) gắn order_id, trong CÙNG giao dịch của
-- pay_order — webhook SePay và xác nhận tay đều gọi pay_order nên cùng được cấp.
-- Thứ tự khóa trong pay_order: orders (FOR UPDATE) → users (FOR NO KEY UPDATE, như 004) → chèn lô.
--   Bản 002 khóa users FOR UPDATE: chặn FOR KEY SHARE mà khóa ngoại credit_lo.user_id cần khi credit_cap_ky (không khóa
--   users) chèn lô kỳ; khi pay_order cũng chèn lô thì thành khóa chéo. NO KEY UPDATE không chặn KEY SHARE, vẫn chặn các
--   hàm 004 (cùng NO KEY UPDATE) nên mọi thay đổi số dư của một người vẫn xếp hàng. credit_cap_nap KHÔNG gọi
--   credit_cap_ky (tranh khóa chỉ mục uq_credit_lo_ky → khóa chéo).
-- Chống cấp đôi: pay_order trả 'already_paid' khi đơn đã trả + chỉ mục duy nhất uq_credit_lo_nap + ON CONFLICT.
-- ============================================================

SET lock_timeout = '5s';

ALTER TABLE orders ADD COLUMN IF NOT EXISTS loai TEXT NOT NULL DEFAULT 'goi';
ALTER TABLE orders ADD COLUMN IF NOT EXISTS goi_nap_ma TEXT;
ALTER TABLE orders ALTER COLUMN plan_id DROP NOT NULL;
ALTER TABLE orders ALTER COLUMN cycle DROP NOT NULL;
ALTER TABLE orders DROP CONSTRAINT IF EXISTS ck_orders_loai;
ALTER TABLE orders ADD CONSTRAINT ck_orders_loai CHECK (
       (loai = 'goi' AND plan_id IS NOT NULL AND cycle IS NOT NULL AND goi_nap_ma IS NULL)
    OR (loai = 'nap' AND goi_nap_ma IS NOT NULL AND plan_id IS NULL AND cycle IS NULL)
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_credit_lo_nap ON credit_lo(order_id) WHERE nguon = 'nap';

-- Cấp lô 'nap' cho đơn nạp ĐÃ trả. Gọi lại không cấp thêm. Đơn chưa trả / không phải đơn nạp: trả NULL.
-- Gọi từ pay_order (đã khóa users); gọi riêng cũng an toàn vì tự khóa users trước theo luật 004.
CREATE OR REPLACE FUNCTION credit_cap_nap(p_order UUID) RETURNS JSONB
LANGUAGE plpgsql SET search_path = public AS $$
DECLARE
    o orders%ROWTYPE;
    g JSONB;
    v_so INT;
    v_ngay INT;
    v_lo UUID;
    v_het TIMESTAMPTZ;
BEGIN
    SELECT * INTO o FROM orders WHERE id = p_order;
    IF NOT FOUND OR o.loai <> 'nap' OR o.status <> 'paid' THEN RETURN NULL; END IF;
    PERFORM 1 FROM users WHERE id = o.user_id FOR NO KEY UPDATE;

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

    INSERT INTO credit_lo (user_id, nguon, so_cap, so_con, het_han, order_id)
    VALUES (o.user_id, 'nap', v_so, v_so, v_het, o.id)
    ON CONFLICT (order_id) WHERE nguon = 'nap' DO NOTHING
    RETURNING id INTO v_lo;
    IF v_lo IS NOT NULL THEN
        INSERT INTO credit_so (user_id, lo_id, loai, so, ghi_chu, boi)
        VALUES (o.user_id, v_lo, 'cap', v_so, 'Gói nạp ' || o.goi_nap_ma || ' · đơn ' || o.code, o.paid_source);
    END IF;

    RETURN (SELECT jsonb_build_object('lo_id', id, 'so_cap', so_cap, 'het_han', het_han)
              FROM credit_lo WHERE order_id = o.id AND nguon = 'nap');
END;
$$;

-- Ghi nhận đơn đã trả: khóa đơn + người dùng, một giao dịch, gọi lại nhiều lần cũng chỉ áp một lần.
-- Đơn gói: cùng gói còn hạn → cộng nối tiếp ngày hết hạn; khác gói hoặc đã hết hạn → tính từ lúc thanh toán
-- (lô credit tháng cấp ở lượt gọi kế tiếp qua credit_cap_ky). Đơn nạp: cấp lô ngay qua credit_cap_nap.
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

REVOKE EXECUTE ON FUNCTION credit_cap_nap(UUID), pay_order(TEXT, TEXT, TEXT) FROM PUBLIC, anon, authenticated;
GRANT EXECUTE ON FUNCTION credit_cap_nap(UUID), pay_order(TEXT, TEXT, TEXT) TO service_role;
```

- [ ] **Step 6: Chạy bộ kiểm mới + bộ kiểm credit cũ**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npx tsx --test tests/goi-nap-csdl.test.ts tests/credit-csdl.test.ts tests/csdl-thu.test.ts
```
Expected: PASS hết (13 bài mới; bộ `credit-csdl` không đổi). Nếu bài «NO KEY UPDATE» hỏng vì `lock timeout`: `pay_order` vẫn còn `FOR UPDATE` trên `users`.

- [ ] **Step 7: Migration chạy lại được (idempotent)**

```bash
ssh gikidi-dev "PGPASSWORD=thu psql -h 127.0.0.1 -p 55990 -U postgres -q -c 'drop database if exists vbp_hai_lan with (force)' -c 'create database vbp_hai_lan'"
for f in schema.sql migrations/002_orders.sql migrations/003_payment_settings.sql migrations/004_credit.sql migrations/005_goi_nap.sql migrations/005_goi_nap.sql; do
  PGPASSWORD=thu psql -h 127.0.0.1 -p 55990 -U postgres -d vbp_hai_lan -q -v ON_ERROR_STOP=1 -1 -f supabase/$f || echo "LỖI ở $f"
done
ssh gikidi-dev "PGPASSWORD=thu psql -h 127.0.0.1 -p 55990 -U postgres -q -c 'drop database vbp_hai_lan with (force)'"
```
Expected: không dòng «LỖI ở …» (trên PC cần `psql` trong PATH; không có thì chạy vòng lặp trên `gikidi-dev` sau khi `scp` các tệp lên `/tmp`).

- [ ] **Step 8: Commit**

```bash
git add supabase/migrations/005_goi_nap.sql tests/goi-nap-csdl.test.ts tests/ho-tro/csdl-thu.ts
git commit -m "feat(credit): 005 goi nap - don loai nap, pay_order cap lo 90 ngay trong cung giao dich, khoa users NO KEY UPDATE

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 2: Kiểm dạng gói nạp trong bảng giá, đọc danh sách gói nạp

**Files:**
- Modify: `vanbanplus-api/src/lib/credit/bang-gia.ts`
- Create: `vanbanplus-api/src/app/api/goi-nap/route.ts`
- Test: `vanbanplus-api/tests/bang-gia.test.ts` (thêm bài)

**Interfaces:**
- Consumes: `DongBangGia`, `dungBangGia` hiện có.
- Produces:
  - `export interface GoiNap { gia_vnd: number; credit: number; so_ngay: number }`
  - `export interface GoiNapCoMa extends GoiNap { ma: string }`
  - `export function kiemGoiNap(ma: string, gia_tri: unknown): GoiNap` — ném `Error("Bảng giá: <ma>.<trường> phải là số nguyên dương")` hoặc `Error("Bảng giá: mã gói nạp «<ma>» chỉ gồm a-z, 0-9, dấu gạch")`.
  - `dungBangGia` ném lỗi khi có dòng `goi_nap` sai dạng (kiểu trả về `BangGia` KHÔNG đổi).
  - `export async function docGoiNap(): Promise<GoiNapCoMa[]>` — sắp theo `gia_vnd` tăng dần, bỏ qua dòng sai dạng (ghi `console.error`).
  - `GET /api/goi-nap` → `{ success: true, data: GoiNapCoMa[] }`.

- [ ] **Step 1: Viết bộ kiểm hỏng trước**

Thêm vào CUỐI `tests/bang-gia.test.ts`, và sửa dòng import đầu tệp thành
`import { dungBangGia, kiemGoiNap, type DongBangGia } from "../src/lib/credit/bang-gia";`:

```ts
test("gói nạp đúng dạng: dựng bảng giá được, kiemGoiNap trả đúng ba trường", () => {
  assert.doesNotThrow(() => dungBangGia(DONG));
  assert.deepEqual(kiemGoiNap("nap-49", { gia_vnd: 49000, credit: 160, so_ngay: 90, thua: 1 }), { gia_vnd: 49000, credit: 160, so_ngay: 90 });
});

test("gói nạp sai dạng: dựng bảng giá báo lỗi, nêu rõ mã và trường", () => {
  const hong: Array<[string, unknown]> = [
    ["gia_vnd", 0], ["gia_vnd", "49000"], ["credit", -1], ["credit", 1.5], ["so_ngay", 0], ["so_ngay", undefined],
  ];
  for (const [truong, giaTri] of hong) {
    const dong = DONG.map((d) => (d.ma === "nap-49" ? { ...d, gia_tri: { ...(d.gia_tri as object), [truong]: giaTri } } : d));
    assert.throws(() => dungBangGia(dong), new RegExp(`nap-49\\.${truong}`), `${truong} = ${String(giaTri)}`);
  }
});

test("mã gói nạp lạ (khoảng trắng, chữ hoa, ký tự đặc biệt) bị từ chối", () => {
  const dong = [...DONG, { loai: "goi_nap", ma: "Nap 49!", gia_tri: { gia_vnd: 49000, credit: 160, so_ngay: 90 } }];
  assert.throws(() => dungBangGia(dong), /mã gói nạp/);
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && npx tsx --test tests/bang-gia.test.ts
```
Expected: FAIL — `kiemGoiNap` không phải hàm (export chưa có).

- [ ] **Step 3: Viết mã**

Trong `src/lib/credit/bang-gia.ts`:

a) Ngay sau hàm `soKhongAm`, thêm:

```ts
function soNguyenDuong(v: unknown, ten: string): number {
  if (typeof v !== "number" || !Number.isInteger(v) || v <= 0) {
    throw new Error(`Bảng giá: ${ten} phải là số nguyên dương`);
  }
  return v;
}

export interface GoiNap {
  gia_vnd: number;
  credit: number;
  so_ngay: number;
}

export interface GoiNapCoMa extends GoiNap {
  ma: string;
}

const MA_GOI_NAP = /^[a-z0-9-]{1,40}$/;

/** Kiểm một dòng credit_bang_gia loai = 'goi_nap'. Sai thì ném lỗi nêu rõ mã và trường. */
export function kiemGoiNap(ma: string, gia_tri: unknown): GoiNap {
  if (!MA_GOI_NAP.test(ma)) throw new Error(`Bảng giá: mã gói nạp «${ma}» chỉ gồm a-z, 0-9, dấu gạch`);
  const g = (gia_tri ?? {}) as Record<string, unknown>;
  return {
    gia_vnd: soNguyenDuong(g.gia_vnd, `${ma}.gia_vnd`),
    credit: soNguyenDuong(g.credit, `${ma}.credit`),
    so_ngay: soNguyenDuong(g.so_ngay, `${ma}.so_ngay`),
  };
}
```

b) Trong vòng lặp của `dungBangGia`, sau nhánh `} else if (d.loai === "tac_vu") { … }`, thêm nhánh:

```ts
    } else if (d.loai === "goi_nap") {
      kiemGoiNap(d.ma, d.gia_tri);
```
(đóng ngoặc theo cấu trúc `if / else if` hiện có — nhánh mới nằm trước dấu `}` đóng chuỗi `else if`).

c) Cuối tệp, thêm:

```ts
/** Các gói nạp đang bán, rẻ trước. Dòng sai dạng thì bỏ qua (trang công khai không được hỏng vì một dòng). */
export async function docGoiNap(): Promise<GoiNapCoMa[]> {
  const { data, error } = await getSupabase().from("credit_bang_gia").select("ma, gia_tri").eq("loai", "goi_nap");
  if (error) throw new Error(`Không đọc được gói nạp: ${error.message}`);
  const ds: GoiNapCoMa[] = [];
  for (const d of (data ?? []) as Array<{ ma: string; gia_tri: unknown }>) {
    try {
      ds.push({ ma: d.ma, ...kiemGoiNap(d.ma, d.gia_tri) });
    } catch (e) {
      console.error(`🟠 [GOI-NAP] bỏ qua dòng ${d.ma}: ${(e as Error).message}`);
    }
  }
  return ds.sort((a, b) => a.gia_vnd - b.gia_vnd);
}
```

d) Tạo `src/app/api/goi-nap/route.ts`:

```ts
import { docGoiNap } from "@lib/credit/bang-gia";
import { fail, ok } from "@lib/response";

export const dynamic = "force-dynamic";

/** GET /api/goi-nap — các gói nạp credit đang bán (công khai, giá + số credit + số ngày dùng). */
export async function GET() {
  try {
    return ok(await docGoiNap());
  } catch (e) {
    console.error("[GOI-NAP] đọc lỗi", (e as Error).message);
    return fail("Không tải được gói nạp.", 500);
  }
}
```

- [ ] **Step 4: Chạy bộ kiểm**

```bash
npx tsx --test tests/bang-gia.test.ts tests/chay-tac-vu.test.ts tests/uoc-tinh.test.ts
```
Expected: PASS hết.

- [ ] **Step 5: Commit**

```bash
git add src/lib/credit/bang-gia.ts src/app/api/goi-nap/route.ts tests/bang-gia.test.ts
git commit -m "feat(credit): kiem dang goi nap trong bang gia, docGoiNap, GET /api/goi-nap

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 3: Đơn nạp phía máy chủ — tạo đơn, xem đơn, webhook, quản trị đơn

**Files:**
- Modify: `vanbanplus-api/src/lib/orders.ts`
- Modify: `vanbanplus-api/src/app/api/orders/route.ts` (viết lại toàn tệp)
- Modify: `vanbanplus-api/src/app/api/orders/[code]/route.ts`
- Modify: `vanbanplus-api/src/app/api/webhooks/sepay/route.ts`
- Modify: `vanbanplus-api/src/app/api/admin/orders/route.ts`, `vanbanplus-api/src/app/admin/orders/page.tsx`
- Test: `vanbanplus-api/tests/don-nap.test.ts`

**Interfaces:**
- Consumes: `GoiNap`, `docGoiNap` (Task 2); cột `orders.loai`, `orders.goi_nap_ma` (Task 1).
- Produces:
  - `OrderRow` có `loai: "goi" | "nap"`, `plan_id: string | null`, `cycle: "month" | "year" | null`, `goi_nap_ma: string | null`.
  - `export function tenDon(o: Pick<OrderRow, "loai" | "plan_id" | "goi_nap_ma">, tenGoi: Record<string, string>, nap: GoiNap | null): string`
  - `export async function napTheoMa(): Promise<Record<string, GoiNap>>`
  - `export function orderView(o: OrderRow, ten: string, bankOpen: BankConfig | null, nap?: GoiNap | null)` — thêm trường `loai`, `goiNapMa`, `credit`, `soNgay` (hai trường sau là `null` với đơn gói).
  - `POST /api/orders` nhận `{ goiNap: "nap-49" }` (tạo đơn nạp) hoặc `{ planId, cycle }` (như cũ).

- [ ] **Step 1: Viết bộ kiểm hỏng trước**

Tạo `tests/don-nap.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { orderView, tenDon, type OrderRow } from "../src/lib/orders";

const NAP = { gia_vnd: 49000, credit: 160, so_ngay: 90 };
const goc: OrderRow = {
  id: "o1", code: "VBPABCDEFGH", user_id: "u1", loai: "nap", plan_id: null, cycle: null, goi_nap_ma: "nap-49",
  amount_vnd: 49000, status: "pending", paid_source: null, paid_ref: null, created_at: "2026-11-16T00:00:00Z",
  expires_at: "2026-11-17T00:00:00Z", paid_at: null, period_start: null, period_end: null,
};
const BANK = { bin: "970422", name: "MB", account: "717683979", holder: "CONG TY TNHH GIA KIEM SO" };

test("tên đơn nạp lấy số credit của gói; thiếu gói thì ghi mã", () => {
  assert.equal(tenDon(goc, {}, NAP), "Nạp 160 credit");
  assert.equal(tenDon(goc, {}, null), "Gói nạp nap-49");
});

test("tên đơn gói lấy tên gói, thiếu tên thì ghi mã gói", () => {
  const goi = { loai: "goi" as const, plan_id: "pro", goi_nap_ma: null };
  assert.equal(tenDon(goi, { pro: "Chuyên nghiệp" }, null), "Chuyên nghiệp");
  assert.equal(tenDon(goi, {}, null), "pro");
});

test("orderView đơn nạp đang chờ: có loại, mã gói, credit, số ngày, mã QR đúng số tiền", () => {
  const v = orderView(goc, "Nạp 160 credit", BANK, NAP);
  assert.equal(v.loai, "nap");
  assert.equal(v.goiNapMa, "nap-49");
  assert.equal(v.credit, 160);
  assert.equal(v.soNgay, 90);
  assert.equal(v.cycle, null);
  assert.ok(v.qr && v.qr.includes("49000"));
});

test("orderView đơn gói: credit và số ngày là null, đơn đã trả không hiện QR", () => {
  const goi: OrderRow = { ...goc, loai: "goi", plan_id: "pro", cycle: "month", goi_nap_ma: null, status: "paid" };
  const v = orderView(goi, "Chuyên nghiệp", BANK);
  assert.deepEqual([v.loai, v.credit, v.soNgay, v.qr, v.bank], ["goi", null, null, null, null]);
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

```bash
npx tsx --test tests/don-nap.test.ts
```
Expected: FAIL — `tenDon` không phải hàm.

- [ ] **Step 3: Sửa `src/lib/orders.ts`**

a) Dòng import đầu tệp, thêm: `import { docGoiNap, type GoiNap } from "./credit/bang-gia";`

b) Thay `interface OrderRow` bằng:

```ts
export interface OrderRow {
  id: string;
  code: string;
  user_id: string;
  loai: "goi" | "nap";
  plan_id: string | null;
  cycle: "month" | "year" | null;
  goi_nap_ma: string | null;
  amount_vnd: number;
  status: "pending" | "paid" | "cancelled";
  paid_source: string | null;
  paid_ref: string | null;
  created_at: string;
  expires_at: string;
  paid_at: string | null;
  period_start: string | null;
  period_end: string | null;
}
```

c) Thay hàm `orderView` bằng:

```ts
/** Tên hiển thị của đơn: tên gói (đơn gói) hoặc «Nạp N credit» (đơn nạp). */
export function tenDon(
  o: Pick<OrderRow, "loai" | "plan_id" | "goi_nap_ma">,
  tenGoi: Record<string, string>,
  nap: GoiNap | null
): string {
  if (o.loai === "nap") {
    return nap ? `Nạp ${nap.credit.toLocaleString("vi-VN")} credit` : `Gói nạp ${o.goi_nap_ma ?? ""}`.trim();
  }
  return (o.plan_id && tenGoi[o.plan_id]) || o.plan_id || "";
}

/** Thông tin trả cho trình duyệt: đơn đang chờ thì kèm chỗ chuyển tiền. */
export function orderView(o: OrderRow, ten: string, bankOpen: BankConfig | null, nap: GoiNap | null = null) {
  const bank = o.status === "pending" ? bankOpen : null;
  const laNap = o.loai === "nap";
  return {
    code: o.code,
    loai: o.loai,
    planId: o.plan_id,
    planName: ten,
    cycle: o.cycle,
    goiNapMa: o.goi_nap_ma,
    credit: laNap ? nap?.credit ?? null : null,
    soNgay: laNap ? nap?.so_ngay ?? null : null,
    amountVnd: o.amount_vnd,
    status: o.status,
    createdAt: o.created_at,
    expiresAt: o.expires_at,
    paidAt: o.paid_at,
    periodEnd: o.period_end,
    bank: bank && { name: bank.name, account: bank.account, holder: bank.holder },
    qr: bank && vietQrString({ bin: bank.bin, account: bank.account, amount: o.amount_vnd, content: o.code }),
  };
}
```

d) Cuối tệp, thêm:

```ts
/** Gói nạp theo mã; lỗi đọc thì trả rỗng (tên đơn rơi về «Gói nạp <mã>»). */
export async function napTheoMa(): Promise<Record<string, GoiNap>> {
  const ds = await docGoiNap().catch(() => []);
  return Object.fromEntries(ds.map(({ ma, ...g }) => [ma, g]));
}
```

- [ ] **Step 4: Viết lại `src/app/api/orders/route.ts`**

```ts
import { NextRequest } from "next/server";
import { getSupabase } from "@lib/supabase";
import { validateApiKey } from "@lib/auth";
import { ok, fail, unauthorized } from "@lib/response";
import { bankConfig, napTheoMa, orderView, planNames, tenDon, type BankConfig, type OrderRow } from "@lib/orders";
import { generateOrderCode, priceOf, purchaseBlockReason, type Cycle } from "@lib/order-rules";
import type { ApiUser } from "@/types";

export const dynamic = "force-dynamic";

const MAX_OPEN_ORDERS = 10;

/** GET /api/orders — lịch sử đơn của người đang đăng nhập. */
export async function GET(req: NextRequest) {
  const user = await validateApiKey(req);
  if (!user) return unauthorized();

  const { data, error } = await getSupabase()
    .from("orders")
    .select("*")
    .eq("user_id", user.id)
    .order("created_at", { ascending: false })
    .limit(20);
  if (error) return fail("Không tải được danh sách đơn.", 500);

  const [names, bank, nap] = await Promise.all([planNames(), bankConfig(), napTheoMa()]);
  return ok(
    (data as OrderRow[]).map((o) => {
      const g = o.goi_nap_ma && Object.hasOwn(nap, o.goi_nap_ma) ? nap[o.goi_nap_ma] : null;
      return orderView(o, tenDon(o, names, g), bank, g);
    })
  );
}

async function donDangCho(userId: string): Promise<OrderRow[]> {
  const { data } = await getSupabase()
    .from("orders")
    .select("*")
    .eq("user_id", userId)
    .eq("status", "pending")
    .gt("expires_at", new Date().toISOString());
  return (data as OrderRow[] | null) ?? [];
}

/** Đơn nạp credit: giá lấy từ credit_bang_gia (loai goi_nap) lúc tạo đơn; số credit cấp theo bảng giá lúc trả tiền. */
async function taoDonNap(user: ApiUser, ma: string, bank: BankConfig) {
  const nap = await napTheoMa();
  const goi = Object.hasOwn(nap, ma) ? nap[ma] : null;
  if (!goi) return fail("Gói nạp không hợp lệ.");

  const open = await donDangCho(user.id);
  const same = open.find((o) => o.loai === "nap" && o.goi_nap_ma === ma && o.amount_vnd === goi.gia_vnd);
  if (same) return ok(orderView(same, tenDon(same, {}, goi), bank, goi));
  if (open.length >= MAX_OPEN_ORDERS) return fail("Có quá nhiều đơn đang chờ thanh toán. Vui lòng thử lại sau.", 429);

  const { data: order, error } = await getSupabase()
    .from("orders")
    .insert({ code: generateOrderCode(), user_id: user.id, loai: "nap", goi_nap_ma: ma, amount_vnd: goi.gia_vnd })
    .select("*")
    .single();
  if (error || !order) return fail("Không tạo được đơn. Vui lòng thử lại.", 500);
  const o = order as OrderRow;
  return ok(orderView(o, tenDon(o, {}, goi), bank, goi));
}

/**
 * POST /api/orders — tạo đơn, hoặc trả lại đơn cùng loại còn hiệu lực để không sinh mã mới mỗi lần tải lại trang.
 * Thân: { goiNap: "nap-49" } (gói nạp) hoặc { planId, cycle } (gói tháng / năm).
 */
export async function POST(req: NextRequest) {
  const user = await validateApiKey(req);
  if (!user) return unauthorized();

  const bank = await bankConfig().catch(() => null);
  if (!bank) return fail("Thanh toán trực tuyến chưa mở. Vui lòng liên hệ support@giakiemso.com.", 503);

  const body = await req.json().catch(() => ({}));
  if (body.goiNap !== undefined) return taoDonNap(user, String(body.goiNap ?? ""), bank);

  const planId = String(body.planId ?? "");
  const cycle = body.cycle as Cycle;
  if (cycle !== "month" && cycle !== "year") return fail("Kỳ thanh toán không hợp lệ.");

  const sb = getSupabase();
  const { data: plans } = await sb
    .from("subscription_plans")
    .select("id, name, price_per_month, price_per_year, is_active")
    .in("id", [planId, user.subscription_plan_id]);
  const target = plans?.find((p) => p.id === planId && p.is_active);
  if (!target) return fail("Gói không hợp lệ.");
  const current = plans?.find((p) => p.id === user.subscription_plan_id) ?? null;

  const blocked = purchaseBlockReason(current, user.subscription_end_date, target);
  if (blocked) return fail(blocked);

  const amount = priceOf(target, cycle);
  const open = await donDangCho(user.id);
  const same = open.find((o) => o.loai === "goi" && o.plan_id === planId && o.cycle === cycle && o.amount_vnd === amount);
  if (same) return ok(orderView(same, target.name, bank));
  if (open.length >= MAX_OPEN_ORDERS) return fail("Có quá nhiều đơn đang chờ thanh toán. Vui lòng thử lại sau.", 429);

  const { data: order, error } = await sb
    .from("orders")
    .insert({ code: generateOrderCode(), user_id: user.id, plan_id: planId, cycle, amount_vnd: amount })
    .select("*")
    .single();
  if (error || !order) return fail("Không tạo được đơn. Vui lòng thử lại.", 500);

  return ok(orderView(order as OrderRow, target.name, bank));
}
```

- [ ] **Step 5: Sửa `src/app/api/orders/[code]/route.ts`**

Đổi dòng import `@lib/orders` thành `import { bankConfig, napTheoMa, orderView, planNames, tenDon, type OrderRow } from "@lib/orders";` và thay hai dòng cuối hàm (từ `const [names, bank] = …` tới `return ok(…)`) bằng:

```ts
  const [names, bank, nap] = await Promise.all([
    planNames(),
    order.status === "pending" ? bankConfig() : null,
    napTheoMa(),
  ]);
  const g = order.goi_nap_ma && Object.hasOwn(nap, order.goi_nap_ma) ? nap[order.goi_nap_ma] : null;
  return ok(orderView(order, tenDon(order, names, g), bank, g));
```

- [ ] **Step 6: Lời báo Telegram của webhook cho đơn nạp**

Trong `src/app/api/webhooks/sepay/route.ts`, thay khối `if (result === "paid") { … }` (từ `const { data: paid } = await sb` tới hết lời gọi `notifyAdmin(…)` của nhánh đó) bằng:

```ts
    if (result === "paid") {
      const { data: paid } = await sb
        .from("orders")
        .select("loai, goi_nap_ma, cycle, period_end, users(email), subscription_plans(name)")
        .eq("code", code)
        .maybeSingle();
      const p = paid as unknown as {
        loai: string; goi_nap_ma: string | null; cycle: string | null; period_end: string | null;
        users: { email: string } | null; subscription_plans: { name: string } | null;
      } | null;
      const han = p?.period_end
        ? new Date(p.period_end).toLocaleDateString("vi-VN", { timeZone: "Asia/Ho_Chi_Minh" })
        : "?";
      await notifyAdmin(
        p?.loai === "nap"
          ? `💰 VanBanPlus: nhận ${vnd(amount)} · đơn ${code}\n${p?.users?.email ?? "?"} · gói nạp ${p?.goi_nap_ma ?? "?"}\n` +
            `Đã cộng credit, dùng tới ${han}`
          : `💰 VanBanPlus: nhận ${vnd(amount)} · đơn ${code}\n` +
            `${p?.users?.email ?? "?"} · gói ${p?.subscription_plans?.name ?? "?"} ${p?.cycle === "year" ? "1 năm" : "1 tháng"}\n` +
            `Đã tự kích hoạt tới ${han}`
      );
    } else if (result === "cancelled") {
```
(giữ nguyên phần `warn("đơn đã hủy trước đó…")` và phần sau). Luồng gọi `pay_order` KHÔNG đổi: lô nạp đã cấp trong `pay_order`.

- [ ] **Step 7: Quản trị đơn hiện loại đơn**

`src/app/api/admin/orders/route.ts`:
- Trong `GET`, chuỗi `select` của `orders` thêm `loai, goi_nap_ma` ngay sau `code, `: `"code, loai, goi_nap_ma, plan_id, cycle, amount_vnd, status, paid_source, paid_ref, created_at, paid_at, period_end, users(email, full_name)"`.
- Trong nhánh `confirm`, đổi `return ok({ result }, "Đã kích hoạt gói.");` thành `return ok({ result }, "Đã xác nhận thanh toán.");`.

`src/app/admin/orders/page.tsx`:
- Thay `interface AdminOrder` bằng:
```ts
interface AdminOrder {
  code: string;
  loai: "goi" | "nap";
  goi_nap_ma: string | null;
  plan_id: string | null;
  cycle: "month" | "year" | null;
  amount_vnd: number;
  status: "pending" | "paid" | "cancelled";
  paid_source: string | null;
  created_at: string;
  period_end: string | null;
  users: { email: string; full_name: string } | null;
}
```
- Trong `act`, câu hỏi xác nhận: `? \`Xác nhận đã nhận ${formatNumber(order.amount_vnd)}đ cho đơn ${order.code}? ${order.loai === "nap" ? "Credit được cộng ngay." : "Gói sẽ được kích hoạt ngay."}\``.
- Thêm `<th className="px-4 py-3">Loại</th>` ngay trước `<th className="px-4 py-3">Gói</th>`; đổi `colSpan={7}` thành `colSpan={8}`.
- Thay ô gói `<td className="px-4 py-3">{PLAN_LABELS[o.plan_id] ?? o.plan_id} · {o.cycle === "year" ? "năm" : "tháng"}</td>` bằng:
```tsx
                <td className="px-4 py-3">{o.loai === "nap" ? "Gói nạp" : "Gói"}</td>
                <td className="px-4 py-3">
                  {o.loai === "nap"
                    ? o.goi_nap_ma
                    : `${PLAN_LABELS[o.plan_id ?? ""] ?? o.plan_id} · ${o.cycle === "year" ? "năm" : "tháng"}`}
                </td>
```
- `PageHeader` description: `"Đơn mua gói, gói nạp và tiền về chưa khớp đơn"`.

- [ ] **Step 8: Chạy bộ kiểm + kiểm kiểu**

```bash
npx tsx --test tests/don-nap.test.ts tests/order-rules.test.ts && npx tsc --noEmit -p .
```
Expected: PASS; `tsc` không lỗi (lỗi kiểu còn sót thường ở chỗ dùng `o.plan_id` như chuỗi — sửa bằng `?? ""`).

- [ ] **Step 9: Commit**

```bash
git add src/lib/orders.ts src/app/api/orders src/app/api/webhooks/sepay/route.ts src/app/api/admin/orders/route.ts src/app/admin/orders/page.tsx tests/don-nap.test.ts
git commit -m "feat(don-hang): tao va xem don goi nap, bao Telegram don nap, quan tri don hien loai don

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 4: Giao diện mua gói nạp — `/bang-gia`, `/thanh-toan`, `/tai-khoan`, `/dang-ky`

**Files:**
- Modify: `vanbanplus-api/src/app/bang-gia/page.tsx`, `vanbanplus-api/src/app/bang-gia/BangGiaClient.tsx`
- Modify: `vanbanplus-api/src/app/thanh-toan/page.tsx`
- Modify: `vanbanplus-api/src/app/tai-khoan/page.tsx` (phần gói nạp + lịch sử đơn)
- Modify: `vanbanplus-api/src/app/dang-ky/DangKyClient.tsx` (một dòng)

**Interfaces:**
- Consumes: `docGoiNap`, `GoiNapCoMa` (Task 2); `GET /api/goi-nap`; `POST /api/orders { goiNap }` và trường `loai`, `credit`, `soNgay` của `orderView` (Task 3).
- Produces: đường `/thanh-toan?nap=<mã>`; mục `#goi-nap` trên `/bang-gia`.

- [ ] **Step 1: `/bang-gia` tải gói nạp phía máy chủ**

`src/app/bang-gia/page.tsx`: thêm `import { docGoiNap, type GoiNapCoMa } from "@lib/credit/bang-gia";`, thêm hàm và đổi trang:

```tsx
async function loadGoiNap(): Promise<GoiNapCoMa[]> {
  try {
    return await docGoiNap();
  } catch {
    return [];
  }
}

export default async function PricingPage() {
  const [plans, goiNap] = await Promise.all([loadPlans(), loadGoiNap()]);
  return <BangGiaClient plans={plans} goiNap={goiNap} />;
}
```

- [ ] **Step 2: `BangGiaClient` hiện mục gói nạp + một câu hỏi thường gặp**

a) Đầu tệp thêm `import type { GoiNapCoMa } from "@lib/credit/bang-gia";`.

b) Đổi chữ ký: `export default function BangGiaClient({ plans, goiNap = [] }: { plans: Plan[]; goiNap?: GoiNapCoMa[] }) {`.

c) Ngay SAU thẻ đóng `</section>` của mục `{/* Plans grid */}` và TRƯỚC `{/* FAQ */}`, thêm:

```tsx
        {goiNap.length > 0 && (
          <section id="goi-nap" className="pb-16">
            <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
              <h2 className="text-2xl font-extrabold text-gray-900 text-center">Gói nạp thêm credit</h2>
              <p className="text-center text-gray-500 mt-2 text-sm">
                Mua thêm khi hết credit của tháng, áp dụng cho mọi gói kể cả Miễn phí. Khi trừ credit, phần sắp hết hạn được dùng trước.
              </p>
              <div className="mt-8 grid grid-cols-1 sm:grid-cols-3 gap-4">
                {goiNap.map((g) => (
                  <div key={g.ma} className="bg-white rounded-2xl border border-gray-200 p-6 text-center flex flex-col">
                    <p className="text-3xl font-extrabold text-gray-900">{formatNumber(g.credit)}</p>
                    <p className="text-sm text-gray-500">credit · dùng trong {g.so_ngay} ngày</p>
                    <p className="mt-4 text-xl font-bold text-gray-900">{formatNumber(g.gia_vnd)}đ</p>
                    <Link
                      href={`/thanh-toan?nap=${encodeURIComponent(g.ma)}`}
                      className="mt-5 py-2.5 rounded-xl text-sm font-semibold text-center bg-primary-600 hover:bg-primary-700 text-white transition-colors"
                    >
                      Mua gói nạp
                    </Link>
                  </div>
                ))}
              </div>
            </div>
          </section>
        )}
```

d) Trong mảng câu hỏi của FAQ, thêm ngay sau mục `{ q: "Credit là gì?", … }`:

```tsx
                ...(goiNap.length > 0
                  ? [{
                      q: "Gói nạp là gì?",
                      a: `Gói nạp cộng thêm credit ngoài credit hằng tháng, mua được ở mọi gói kể cả Miễn phí, dùng trong ${goiNap[0].so_ngay} ngày kể từ lúc thanh toán. Khi trừ credit, phần sắp hết hạn được dùng trước, nên credit tháng thường được dùng trước credit nạp.`,
                    }]
                  : []),
```

- [ ] **Step 3: `/thanh-toan` nhận `?nap=`**

Trong `src/app/thanh-toan/page.tsx`:

a) Thay `interface OrderView` bằng:
```ts
interface OrderView {
  code: string;
  loai: "goi" | "nap";
  planName: string;
  cycle: "month" | "year" | null;
  credit: number | null;
  soNgay: number | null;
  amountVnd: number;
  status: "pending" | "paid" | "cancelled";
  periodEnd: string | null;
  bank: { name: string; account: string; holder: string } | null;
  qr: string | null;
}
```

b) Sửa chú thích hàm: `/** Trang Thanh toán — /thanh-toan?goi=pro&ky=month (gói), ?nap=nap-49 (gói nạp) hoặc ?ma=VBP… (đơn đang chờ). */`

c) Trong khối `if (!isAuthenticated) { … }`, thay lệnh `router.replace(…)` bằng:
```ts
      // Mở lại đơn cũ hoặc mua gói nạp thì cần tài khoản sẵn có; mua gói mới thì sang trang đăng ký với gói đã chọn sẵn
      router.replace(
        params.get("ma") || params.get("nap")
          ? `/dang-nhap?next=${here}`
          : `/dang-ky?goi=${encodeURIComponent(params.get("goi") ?? "")}&ky=${params.get("ky") === "year" ? "year" : "month"}`
      );
```

d) Thay khối tạo / đọc đơn trong `load`:
```ts
      const code = params.get("ma");
      const nap = params.get("nap");
      const res = code
        ? await api.get<OrderView>(`/api/orders/${encodeURIComponent(code)}`)
        : nap
          ? await api.post<OrderView>("/api/orders", { goiNap: nap })
          : await api.post<OrderView>("/api/orders", { planId: params.get("goi"), cycle: params.get("ky") ?? "month" });
```

e) Khối `order?.status === "paid"`: thay `<h2>…</h2>`, đoạn `periodEnd` và đoạn «Ứng dụng trên máy…» bằng:
```tsx
              <h2 className="text-xl font-bold text-gray-900 mt-3">
                {order.loai === "nap" ? `Đã cộng ${formatNumber(order.credit ?? 0)} credit` : `Đã kích hoạt gói ${order.planName}`}
              </h2>
              {order.periodEnd && (
                <p className="text-gray-600 mt-2">
                  {order.loai === "nap" ? "Credit nạp dùng tới" : "Hạn sử dụng tới"} {formatDate(order.periodEnd)}.
                </p>
              )}
              <p className="text-sm text-gray-500 mt-2">
                {order.loai === "nap"
                  ? "Số dư mới hiện ngay ở trang tài khoản và trong ứng dụng."
                  : "Ứng dụng trên máy dùng được gói mới ngay, không cần cài lại."}
              </p>
```

f) Khối `order?.status === "pending"`: thay tiêu đề `<h2 className="font-bold text-gray-900">Gói {order.planName} · …</h2>` bằng:
```tsx
                <h2 className="font-bold text-gray-900">
                  {order.loai === "nap"
                    ? `${order.planName} · dùng trong ${order.soNgay ?? 90} ngày`
                    : `Gói ${order.planName} · ${order.cycle === "year" ? "1 năm" : "1 tháng"}`}
                </h2>
```

- [ ] **Step 4: `/dang-ky` giữ đích `/thanh-toan?nap=` sau khi đăng ký**

Trong `src/app/dang-ky/DangKyClient.tsx`, thay dòng
`    : next && !next.startsWith("/thanh-toan") ? next : "/tai-khoan";`
bằng
`    : next && (!next.startsWith("/thanh-toan") || next.startsWith("/thanh-toan?nap=")) ? next : "/tai-khoan";`
(đường: `/thanh-toan?nap=…` chưa đăng nhập → `/dang-nhap?next=…` → liên kết «Đăng ký» giữ `next` → đăng ký gói Miễn phí xong quay lại trang thanh toán gói nạp).

- [ ] **Step 5: `/tai-khoan` hiện gói nạp và đơn nạp**

Trong `src/app/tai-khoan/page.tsx`:

a) Thay `interface OrderItem` bằng:
```ts
interface OrderItem {
  code: string;
  loai?: "goi" | "nap";
  planName: string;
  cycle: "month" | "year" | null;
  amountVnd: number;
  status: "pending" | "paid" | "cancelled";
  createdAt: string;
  periodEnd: string | null;
}

interface GoiNapItem {
  ma: string;
  gia_vnd: number;
  credit: number;
  so_ngay: number;
}
```

b) Sau dòng `const [orders, setOrders] = useState<OrderItem[]>([]);` thêm `const [goiNap, setGoiNap] = useState<GoiNapItem[]>([]);`.

c) Trong `useEffect` đầu, ngay sau lời gọi `api.get<OrderItem[]>("/api/orders").then(…);` thêm:
```ts
    api.get<GoiNapItem[]>("/api/goi-nap").then((res) => {
      if (res.success && res.data) setGoiNap(res.data);
    });
```

d) Dòng tên đơn trong «Lịch sử đơn»: thay `{o.planName} · {o.cycle === "year" ? "1 năm" : "1 tháng"} · {formatNumber(o.amountVnd)}đ` bằng
`{o.planName} · {o.loai === "nap" ? "gói nạp" : o.cycle === "year" ? "1 năm" : "1 tháng"} · {formatNumber(o.amountVnd)}đ`.

e) Trong thẻ «🪙 Credit AI», ngay SAU đoạn `<p className="text-xs text-gray-500">Mỗi tác vụ AI trừ credit … không cộng dồn sang tháng sau.</p>`, thêm:
```tsx
                    {goiNap.length > 0 && (
                      <div className="pt-3 border-t border-gray-100">
                        <p className="text-xs font-semibold text-gray-500 uppercase tracking-wider">Mua thêm credit</p>
                        <div className="mt-2 grid grid-cols-1 sm:grid-cols-3 gap-2">
                          {goiNap.map((g) => (
                            <Link
                              key={g.ma}
                              href={`/thanh-toan?nap=${encodeURIComponent(g.ma)}`}
                              className="rounded-lg border border-gray-200 px-3 py-2 text-sm hover:border-primary-300 hover:bg-primary-50/50 transition-colors"
                            >
                              <span className="font-semibold text-gray-900">{formatNumber(g.credit)} credit</span>
                              <span className="block text-xs text-gray-500">{formatNumber(g.gia_vnd)}đ · dùng {g.so_ngay} ngày</span>
                            </Link>
                          ))}
                        </div>
                        <p className="text-xs text-gray-400 mt-2">Credit được trừ từ phần hết hạn sớm nhất trước.</p>
                      </div>
                    )}
```

- [ ] **Step 6: Kiểm kiểu, soát lời**

```bash
npx tsc --noEmit -p . && for f in src/app/bang-gia/BangGiaClient.tsx src/app/thanh-toan/page.tsx src/app/tai-khoan/page.tsx; do grep -nE "\b([Tt]ôi|[Tt]a|[Aa]nh|[Ee]m|[Bb]ạn|[Cc]hị|[Mm]ình|[Dd]ạ|[Vv]âng)\b|ạ[.,!? ]" $f; done
```
Expected: `tsc` không lỗi; `grep` không in dòng nào thuộc phần vừa thêm (dòng có sẵn từ trước thì để nguyên).

- [ ] **Step 7: Commit**

```bash
git add src/app/bang-gia src/app/thanh-toan/page.tsx src/app/tai-khoan/page.tsx src/app/dang-ky/DangKyClient.tsx
git commit -m "feat(ui): ban goi nap tren bang gia, thanh toan, tai khoan

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 5: Spec §13.1 — tác vụ v2 không bao giờ chuyển sang AI Studio

**Files:**
- Modify: `vanbanplus-api/src/lib/gemini-transport.ts`
- Modify: `vanbanplus-api/src/lib/ai-providers.ts` (một dòng trong `callGeminiProvider`)
- Modify: `vanbanplus-api/src/types/index.ts` (`AICallOptions`)
- Modify: `vanbanplus-api/src/lib/tac-vu/phu-thuoc.ts`
- Test: `vanbanplus-api/tests/gemini-transport.test.ts` (thêm bài), `vanbanplus-api/tests/phu-thuoc.test.ts` (mới)

**Interfaces:**
- Consumes: `sendGemini`, `callAI`, `PhuThuoc` hiện có.
- Produces:
  - `GeminiRequest.chiVertex?: boolean`; `AICallOptions.chiVertex?: boolean`.
  - `export function taoGoiAIv2(goi: (contents: any[], opts: AICallOptions) => Promise<AICallResult>): (contents: unknown[], opts: AICallOptions) => Promise<AICallResult>` trong `phu-thuoc.ts`; `phuThuocThat.goiAI = taoGoiAIv2(callAI)`.

- [ ] **Step 1: Viết bộ kiểm hỏng trước**

Thêm vào CUỐI `tests/gemini-transport.test.ts`:

```ts
// ---------- chiVertex (tác vụ v2, spec §13.1) ----------

const YEU_CAU_V2 = { ...YEU_CAU, chiVertex: true };

test("chiVertex: Vertex trả 200 thì dùng Vertex như thường", async () => {
  const { fn, goi } = fetchGia(jsonRes(200, { candidates: [] }));
  const r = await sendGemini(YEU_CAU_V2, { vertex: CAU_HINH, fetchFn: fn, wait: khongCho, tokens: new VertexTokenSource(CAU_HINH, fn) });
  assert.equal(r.status, 200);
  assert.equal(goi.length, 1);
  assert.match(goi[0].url, /aiplatform/);
});

test("chiVertex: Vertex từ chối (403) thì trả nguyên lỗi, KHÔNG gọi AI Studio", async () => {
  const { fn, goi } = fetchGia(jsonRes(403, { error: "het credit" }));
  const r = await sendGemini(YEU_CAU_V2, { vertex: CAU_HINH, fetchFn: fn, wait: khongCho, tokens: new VertexTokenSource(CAU_HINH, fn) });
  assert.equal(r.status, 403);
  assert.equal(goi.length, 1);
  assert.ok(goi.every((g) => !/generativelanguage/.test(g.url)));
});

test("chiVertex: Vertex quá tải kéo dài (429) thì thử lại trên Vertex rồi trả 429, KHÔNG gọi AI Studio", async () => {
  const { fn, goi } = fetchGia(jsonRes(429), jsonRes(429), jsonRes(429));
  const r = await sendGemini(YEU_CAU_V2, { vertex: CAU_HINH, fetchFn: fn, wait: khongCho, tokens: new VertexTokenSource(CAU_HINH, fn) });
  assert.equal(r.status, 429);
  assert.equal(goi.length, 3);
  assert.ok(goi.every((g) => /aiplatform/.test(g.url)));
});

test("chiVertex: không xin được token thì báo lỗi, KHÔNG gọi AI Studio", async () => {
  const { fn, goi } = fetchGia(jsonRes(200));
  const hong = { get: async () => { throw new Error("mat khoa"); } };
  await assert.rejects(() => sendGemini(YEU_CAU_V2, { vertex: CAU_HINH, fetchFn: fn, wait: khongCho, tokens: hong }), /mat khoa/);
  assert.equal(goi.length, 0);
});

test("chiVertex: chưa cấu hình Vertex thì báo lỗi, KHÔNG gọi AI Studio", async () => {
  const { fn, goi } = fetchGia(jsonRes(200));
  await assert.rejects(() => sendGemini(YEU_CAU_V2, { vertex: null, fetchFn: fn, wait: khongCho }), /AI Studio/);
  assert.equal(goi.length, 0);
});

test("chiVertex với khóa riêng của người dùng: báo lỗi, không gọi gì", async () => {
  const { fn, goi } = fetchGia(jsonRes(200));
  await assert.rejects(() =>
    sendGemini({ ...YEU_CAU_V2, source: "user" }, { vertex: CAU_HINH, fetchFn: fn, wait: khongCho, tokens: new VertexTokenSource(CAU_HINH, fn) })
  );
  assert.equal(goi.length, 0);
});
```

Tạo `tests/phu-thuoc.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { taoGoiAIv2 } from "../src/lib/tac-vu/phu-thuoc";
import type { AICallOptions, ApiUser } from "../src/types";

test("đường v2 luôn gọi AI với chiVertex: true, giữ nguyên tùy chọn khác, không sửa đối tượng gốc", async () => {
  const nhan: AICallOptions[] = [];
  const goiAI = taoGoiAIv2(async (_c, o) => {
    nhan.push(o);
    return { success: true, content: "ok" };
  });
  const opts: AICallOptions = {
    user: { id: "U1" } as ApiUser,
    resolvedKey: { keyId: "k", provider: "gemini", apiKey: "x", model: "gemini-3.6-flash", source: "system" },
    endpoint: "/api/v2/tac-vu/thu",
    requestType: "v2:thu",
    maxTokens: 5000,
  };
  const kq = await goiAI([{ parts: [{ text: "a" }] }], opts);
  assert.equal(kq.content, "ok");
  assert.equal(nhan[0].chiVertex, true);
  assert.equal(nhan[0].maxTokens, 5000);
  assert.equal(nhan[0].requestType, "v2:thu");
  assert.equal(opts.chiVertex, undefined);
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

```bash
npx tsx --test tests/gemini-transport.test.ts tests/phu-thuoc.test.ts
```
Expected: FAIL — bài 403 nhận 200 từ AI Studio (2 lần gọi); `taoGoiAIv2` không phải hàm.

- [ ] **Step 3: Sửa `src/lib/gemini-transport.ts`**

a) Trong `interface GeminiRequest`, sau `body: any;` thêm:
```ts
  /** Tác vụ v2 (spec §13.1): chỉ đi Vertex. Lỗi thì trả lỗi, không bao giờ chuyển sang AI Studio bậc miễn phí. */
  chiVertex?: boolean;
```

b) Ngay trước chú thích của `sendGemini`, thêm hàm:
```ts
async function goiVertex(
  vertex: VertexConfig,
  request: GeminiRequest,
  fetchFn: typeof fetch,
  tokens: { get(): Promise<string> },
  retry: { wait?: (ms: number) => Promise<void> }
): Promise<Response> {
  const token = await tokens.get();
  const url =
    `https://aiplatform.googleapis.com/v1/projects/${vertex.project}/locations/${vertex.location}` +
    `/publishers/google/models/${request.model}:generateContent`;
  const vertexBody = toVertexBody(request.body);
  return fetchWithTransientRetry(() => postJson(fetchFn, url, { Authorization: `Bearer ${token}` }, vertexBody), retry);
}
```

c) Thay toàn bộ hàm `sendGemini` (giữ chú thích, thêm một dòng) bằng:
```ts
/**
 * Gửi yêu cầu Gemini. Khóa hệ thống đi Vertex nếu có cấu hình, tính vào Cloud Billing của dự án.
 * Vertex lỗi (hết credit, mất quyền, quá tải kéo dài) thì chuyển sang AI Studio để dịch vụ không dừng —
 * TRỪ khi request.chiVertex (tác vụ v2): khi đó trả lỗi để nơi gọi hoàn credit.
 * Khóa riêng của người dùng luôn đi AI Studio.
 */
export async function sendGemini(request: GeminiRequest, deps: TransportDeps = {}): Promise<Response> {
  const fetchFn = deps.fetchFn ?? ((...args) => fetch(...args));
  const retry = { wait: deps.wait };
  const vertex = deps.vertex === undefined ? readVertexConfig() : deps.vertex;

  if (request.chiVertex) {
    if (request.source === "user") throw new Error("Yêu cầu chỉ-Vertex không dùng khóa riêng của người dùng");
    if (!vertex) throw new Error("Chưa cấu hình Vertex; tác vụ v2 không chuyển sang AI Studio");
    const response = await goiVertex(vertex, request, fetchFn, deps.tokens ?? defaultTokens(vertex), retry);
    if (!response.ok) console.error(`🔴 [Vertex] ${response.status}, tác vụ v2 không chuyển sang AI Studio`);
    return response;
  }

  if (vertex && request.source !== "user") {
    try {
      const response = await goiVertex(vertex, request, fetchFn, deps.tokens ?? defaultTokens(vertex), retry);
      if (response.ok) return response;

      const detail = (await response.text()).slice(0, 300);
      console.error(`🔴 [Vertex] ${response.status}, chuyển sang AI Studio: ${detail}`);
    } catch (err: any) {
      console.error(`🔴 [Vertex] ${err.message}, chuyển sang AI Studio`);
    }
  }

  const url = `${AI_STUDIO_BASE}/${request.model}:generateContent?key=${request.apiKey}`;
  return fetchWithTransientRetry(() => postJson(fetchFn, url, {}, request.body), retry);
}
```

- [ ] **Step 4: Truyền cờ qua `AICallOptions` và `callGeminiProvider`**

`src/types/index.ts`, trong `interface AICallOptions`, sau `disableThinking?: boolean;` thêm:
```ts
  /** Tác vụ v2: chỉ đi Vertex, không tự chuyển sang AI Studio khi lỗi (spec §13.1). */
  chiVertex?: boolean;
```

`src/lib/ai-providers.ts`, trong `callGeminiProvider`, lời gọi `sendGemini({ … })` thêm một dòng sau `body,`:
```ts
    chiVertex: options.chiVertex === true,
```

- [ ] **Step 5: `phu-thuoc.ts` bật cờ cho đường v2**

Trong `src/lib/tac-vu/phu-thuoc.ts`:
- Thêm import: `import type { AICallOptions, AICallResult } from "@/types";`
- Trước `export const phuThuocThat`, thêm:
```ts
/** Đường v2 không bao giờ rơi sang AI Studio bậc miễn phí (spec §13.1): mọi lời gọi AI mang cờ chiVertex. */
export function taoGoiAIv2(goi: (contents: any[], opts: AICallOptions) => Promise<AICallResult>) {
  return (contents: unknown[], opts: AICallOptions) => goi(contents as any[], { ...opts, chiVertex: true });
}
```
- Trong `phuThuocThat`, thay `goiAI: (contents, opts) => callAI(contents as any[], opts),` bằng `goiAI: taoGoiAIv2(callAI),`.

(Giữ nguyên các trường khác của `phuThuocThat`, kể cả trường `dot2a`/`dot2b` có thể đã thêm.)

- [ ] **Step 6: Chạy bộ kiểm**

```bash
npx tsx --test tests/gemini-transport.test.ts tests/phu-thuoc.test.ts tests/chay-tac-vu.test.ts && npx tsc --noEmit -p .
```
Expected: PASS hết, kể cả mọi bài cũ của `gemini-transport` (đường cũ không đổi hành vi). `chay-tac-vu` sẵn có bài «nhà cung cấp trả 429 → AI_QUA_TAI, hoàn credit» và «lỗi khác → AI_LOI, hoàn credit» — đó là đường mà lỗi Vertex của v2 đi qua.

- [ ] **Step 7: Commit**

```bash
git add src/lib/gemini-transport.ts src/lib/ai-providers.ts src/types/index.ts src/lib/tac-vu/phu-thuoc.ts tests/gemini-transport.test.ts tests/phu-thuoc.test.ts
git commit -m "feat(ai): tac vu v2 chi di Vertex, khong tu chuyen sang AI Studio mien phi (spec 13.1)

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 6: Migration 006 — `tac_vu_luu`, công tắc, nhật ký mở xem, nhật ký dọn

**Files:**
- Create: `vanbanplus-api/supabase/migrations/006_luu_ho_tro.sql`
- Modify: `vanbanplus-api/tests/ho-tro/csdl-thu.ts` (`TEP_LUOC_DO`)
- Test: `vanbanplus-api/tests/luu-ho-tro-csdl.test.ts`

**Interfaces:**
- Consumes: `users`, `tac_vu_chay` (004).
- Produces (SQL, mọi hàm `SECURITY DEFINER`, chỉ `service_role` gọi được):
  - `users.cho_phep_luu_ho_tro BOOLEAN NOT NULL DEFAULT FALSE`.
  - Bảng `tac_vu_luu(luot_id PK → tac_vu_chay, user_id → users, du_lieu_ma_hoa BYTEA, iv BYTEA (12 byte), het_han, tao_luc)`; `service_role` chỉ `SELECT (luot_id, user_id, het_han, tao_luc)`.
  - Bảng `tac_vu_luu_nhat_ky(id, luot_id, user_id, boi, luc)` chỉ ghi thêm; `tac_vu_luu_don(id, luc, so_xoa, nguon)`.
  - `tac_vu_luu_ghi(p_luot UUID, p_user UUID, p_du_lieu TEXT /*base64*/, p_iv TEXT /*base64*/) RETURNS BOOLEAN` — ghi khi công tắc bật và lượt thuộc người đó; gọi lại cùng lượt thì `FALSE`.
  - `tac_vu_luu_dat(p_user UUID, p_bat BOOLEAN) RETURNS JSONB` → `{bat, da_xoa}`; tắt thì xóa ngay mọi bản lưu của người đó.
  - `tac_vu_luu_mo(p_luot UUID, p_boi TEXT) RETURNS JSONB` → `{luot_id, user_id, du_lieu, iv, het_han, tao_luc}` (base64 không xuống dòng) hoặc `NULL`; trả thì ghi nhật ký.
  - `tac_vu_luu_xoa_het_han(p_nguon TEXT DEFAULT 'tay') RETURNS INT` — xóa dòng hết hạn, ghi một dòng `tac_vu_luu_don`, dọn nhật ký dọn quá 90 ngày.

- [ ] **Step 1: Thêm 006 vào danh sách nạp**

`tests/ho-tro/csdl-thu.ts`, mảng `TEP_LUOC_DO`: thêm `"migrations/006_luu_ho_tro.sql",` ngay sau dòng `"migrations/005_goi_nap.sql",`.

- [ ] **Step 2: Viết bộ kiểm hỏng trước**

Tạo `tests/luu-ho-tro-csdl.test.ts`:

```ts
import { after, before, describe, test } from "node:test";
import assert from "node:assert/strict";
import { Client } from "pg";
import { CO_CSDL_THU, rpc, taoCsdlThu, taoNguoiDung } from "./ho-tro/csdl-thu";

const B64 = (s: string) => Buffer.from(s).toString("base64");
const IV = Buffer.alloc(12, 7).toString("base64");

describe("lưu hỗ trợ (006): tac_vu_luu, nhật ký mở xem, dọn hết hạn", { skip: !CO_CSDL_THU }, () => {
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

  async function taoLuot(u: string): Promise<string> {
    const m = await rpc(c, "tac_vu_mo", u, "thu", 1, 2, "gemini", "gemini-3.6-flash", "kiem-thu", null);
    await rpc(c, "credit_chot", m.luot_id, 1, 1, 1, false);
    return m.luot_id as string;
  }
  const dat = (u: string, bat: boolean) => rpc(c, "tac_vu_luu_dat", u, bat);
  const ghi = (luot: string, u: string, du = B64("ban-ma"), iv = IV) => rpc(c, "tac_vu_luu_ghi", luot, u, du, iv);
  const soDong = async (u: string): Promise<number> =>
    (await c.query("SELECT count(*)::int AS n FROM tac_vu_luu WHERE user_id = $1", [u])).rows[0].n;

  test("mặc định tắt: ghi không tạo dòng nào", async () => {
    const u = await taoNguoiDung(c);
    assert.equal((await c.query("SELECT cho_phep_luu_ho_tro AS b FROM users WHERE id = $1", [u])).rows[0].b, false);
    const luot = await taoLuot(u);
    assert.equal(await ghi(luot, u), false);
    assert.equal(await soDong(u), 0);
  });

  test("bật: ghi một dòng hạn 7 ngày, giữ nguyên byte; ghi lại cùng lượt không thêm dòng", async () => {
    const u = await taoNguoiDung(c);
    assert.deepEqual(await dat(u, true), { bat: true, da_xoa: 0 });
    const luot = await taoLuot(u);
    assert.equal(await ghi(luot, u), true);
    const { rows } = await c.query(
      `SELECT het_han = tao_luc + interval '7 days' AS bay_ngay, encode(du_lieu_ma_hoa, 'base64') = $2 AS dung_du_lieu,
              encode(iv, 'base64') = $3 AS dung_iv
         FROM tac_vu_luu WHERE luot_id = $1`,
      [luot, B64("ban-ma"), IV]
    );
    assert.deepEqual(rows[0], { bay_ngay: true, dung_du_lieu: true, dung_iv: true });
    assert.equal(await ghi(luot, u), false);
    assert.equal(await soDong(u), 1);
  });

  test("lượt của người khác: không ghi; iv sai độ dài: báo lỗi", async () => {
    const a = await taoNguoiDung(c);
    const b = await taoNguoiDung(c);
    await dat(a, true);
    await dat(b, true);
    const luotB = await taoLuot(b);
    assert.equal(await ghi(luotB, a), false);
    assert.equal(await soDong(a), 0);
    await assert.rejects(ghi(luotB, b, B64("x"), Buffer.alloc(8).toString("base64")), /iv/);
  });

  test("tắt: xóa ngay mọi bản lưu của người đó, không đụng người khác; tắt rồi thì ghi không lọt", async () => {
    const a = await taoNguoiDung(c);
    const b = await taoNguoiDung(c);
    await dat(a, true);
    await dat(b, true);
    await ghi(await taoLuot(a), a);
    await ghi(await taoLuot(a), a);
    await ghi(await taoLuot(b), b);
    assert.deepEqual(await dat(a, false), { bat: false, da_xoa: 2 });
    assert.equal(await soDong(a), 0);
    assert.equal(await soDong(b), 1);
    assert.equal(await ghi(await taoLuot(a), a), false);
    assert.equal(await soDong(a), 0);
  });

  test("mở bản lưu: trả bản mã (base64 liền) + ghi nhật ký ai, lúc nào, lượt nào; hết hạn thì không trả, không ghi", async () => {
    const u = await taoNguoiDung(c);
    await dat(u, true);
    const luot = await taoLuot(u);
    const dai = B64("x".repeat(500)); // đủ dài để encode() của Postgres xuống dòng nếu không bỏ
    await ghi(luot, u, dai);
    const r = await rpc(c, "tac_vu_luu_mo", luot, "admin@test");
    assert.equal(r.du_lieu, dai);
    assert.equal(r.iv, IV);
    assert.equal(r.user_id, u);
    const { rows } = await c.query(
      "SELECT luot_id, user_id, boi, luc > NOW() - interval '1 minute' AS vua FROM tac_vu_luu_nhat_ky WHERE luot_id = $1",
      [luot]
    );
    assert.deepEqual(rows, [{ luot_id: luot, user_id: u, boi: "admin@test", vua: true }]);

    await c.query("UPDATE tac_vu_luu SET het_han = NOW() - interval '1 second' WHERE luot_id = $1", [luot]);
    assert.equal(await rpc(c, "tac_vu_luu_mo", luot, "admin@test"), null);
    assert.equal((await c.query("SELECT count(*)::int AS n FROM tac_vu_luu_nhat_ky WHERE luot_id = $1", [luot])).rows[0].n, 1);
    await assert.rejects(rpc(c, "tac_vu_luu_mo", luot, " "), /DAU_VAO_SAI/);
  });

  test("nhật ký mở xem chỉ ghi thêm", async () => {
    const u = await taoNguoiDung(c);
    await dat(u, true);
    const luot = await taoLuot(u);
    await ghi(luot, u);
    await rpc(c, "tac_vu_luu_mo", luot, "admin@test");
    await assert.rejects(c.query("UPDATE tac_vu_luu_nhat_ky SET boi = 'khac' WHERE luot_id = $1", [luot]), /chỉ ghi thêm/);
    await assert.rejects(c.query("DELETE FROM tac_vu_luu_nhat_ky WHERE luot_id = $1", [luot]), /chỉ ghi thêm/);
  });

  test("dọn hết hạn: chỉ xóa dòng quá hạn, mỗi lần chạy ghi một dòng kiểm được", async () => {
    const u = await taoNguoiDung(c);
    await dat(u, true);
    const cu = await taoLuot(u);
    const moi = await taoLuot(u);
    await ghi(cu, u);
    await ghi(moi, u);
    await c.query("UPDATE tac_vu_luu SET het_han = NOW() - interval '1 minute' WHERE luot_id = $1", [cu]);
    const n = await rpc(c, "tac_vu_luu_xoa_het_han", "kiem-thu");
    assert.ok(n >= 1);
    const con = (await c.query("SELECT luot_id FROM tac_vu_luu WHERE user_id = $1", [u])).rows.map((r) => r.luot_id);
    assert.deepEqual(con, [moi]);
    const lan1 = (await c.query("SELECT so_xoa, nguon FROM tac_vu_luu_don ORDER BY id DESC LIMIT 1")).rows[0];
    assert.deepEqual(lan1, { so_xoa: n, nguon: "kiem-thu" });
    assert.equal(await rpc(c, "tac_vu_luu_xoa_het_han", "kiem-thu"), 0);
    const lan2 = (await c.query("SELECT so_xoa, nguon FROM tac_vu_luu_don ORDER BY id DESC LIMIT 1")).rows[0];
    assert.deepEqual(lan2, { so_xoa: 0, nguon: "kiem-thu" });
  });

  test("xóa người dùng thì xóa theo bản lưu", async () => {
    const u = await taoNguoiDung(c);
    await dat(u, true);
    await ghi(await taoLuot(u), u);
    await c.query("DELETE FROM users WHERE id = $1", [u]);
    assert.equal(await soDong(u), 0);
  });

  test("quyền: anon/authenticated không có gì; service_role chỉ đọc cột không phải bản mã, không ghi thẳng", async () => {
    const loi: string[] = [];
    const bang = ["tac_vu_luu", "tac_vu_luu_nhat_ky", "tac_vu_luu_don"];
    for (const v of ["anon", "authenticated"]) {
      for (const b of bang)
        for (const q of ["SELECT", "INSERT", "UPDATE", "DELETE", "TRUNCATE", "REFERENCES", "TRIGGER"]) {
          if ((await c.query("SELECT has_table_privilege($1, $2, $3) AS co", [v, `public.${b}`, q])).rows[0].co) loi.push(`${v} ${q} ${b}`);
        }
      const ham = await c.query(
        `SELECT p.oid::regprocedure::text AS ten FROM pg_proc p JOIN pg_namespace n ON n.oid = p.pronamespace
          WHERE n.nspname = 'public' AND p.proname LIKE 'tac\\_vu\\_luu\\_%'`
      );
      assert.ok(ham.rows.length >= 5, `chỉ thấy ${ham.rows.length} hàm tac_vu_luu_*`);
      for (const h of ham.rows) {
        if ((await c.query("SELECT has_function_privilege($1, $2::regprocedure, 'EXECUTE') AS co", [v, h.ten])).rows[0].co) loi.push(`${v} EXECUTE ${h.ten}`);
      }
    }
    for (const b of ["tac_vu_luu_nhat_ky", "tac_vu_luu_don"]) {
      const day = (await c.query("SELECT pg_get_serial_sequence($1, 'id') AS ten", [b])).rows[0].ten as string;
      for (const v of ["anon", "authenticated", "service_role"])
        for (const q of ["USAGE", "SELECT", "UPDATE"]) {
          if ((await c.query("SELECT has_sequence_privilege($1, $2, $3) AS co", [v, day, q])).rows[0].co) loi.push(`${v} ${q} ${day}`);
        }
    }
    assert.deepEqual(loi, []);

    const cot = async (ten: string) =>
      (await c.query("SELECT has_column_privilege('service_role', 'public.tac_vu_luu', $1, 'SELECT') AS co", [ten])).rows[0].co;
    for (const ten of ["luot_id", "user_id", "het_han", "tao_luc"]) assert.equal(await cot(ten), true, ten);
    for (const ten of ["du_lieu_ma_hoa", "iv"]) assert.equal(await cot(ten), false, ten);
    for (const q of ["INSERT", "UPDATE", "DELETE"]) {
      assert.equal((await c.query("SELECT has_table_privilege('service_role', 'public.tac_vu_luu', $1) AS co", [q])).rows[0].co, false, q);
    }
    assert.equal((await c.query("SELECT has_table_privilege('service_role', 'public.tac_vu_luu_nhat_ky', 'SELECT') AS co")).rows[0].co, true);
    assert.equal((await c.query("SELECT has_table_privilege('service_role', 'public.tac_vu_luu_nhat_ky', 'INSERT') AS co")).rows[0].co, false);
    assert.equal((await c.query("SELECT has_table_privilege('service_role', 'public.tac_vu_luu_don', 'SELECT') AS co")).rows[0].co, true);
  });

  test("service_role: ghi / mở qua hàm được; đọc thẳng bản mã, chèn thẳng thì bị từ chối", async (t) => {
    const u = await taoNguoiDung(c);
    const luot = await taoLuot(u);
    const k = new Client({ connectionString: csdl.url });
    await k.connect();
    try {
      try {
        await k.query("SET ROLE service_role");
      } catch (e) {
        t.skip(`không SET ROLE service_role được trên CSDL thử: ${(e as Error).message}`);
        return;
      }
      await rpc(k, "tac_vu_luu_dat", u, true);
      assert.equal(await rpc(k, "tac_vu_luu_ghi", luot, u, B64("ban-ma"), IV), true);
      assert.equal((await k.query("SELECT luot_id FROM tac_vu_luu WHERE user_id = $1", [u])).rows.length, 1);
      await assert.rejects(k.query("SELECT du_lieu_ma_hoa FROM tac_vu_luu WHERE user_id = $1", [u]), /permission denied/);
      await assert.rejects(
        k.query("INSERT INTO tac_vu_luu (luot_id, user_id, du_lieu_ma_hoa, iv, het_han) VALUES ($1, $2, 'x', 'x', NOW())", [luot, u]),
        /permission denied/
      );
      assert.equal((await rpc(k, "tac_vu_luu_mo", luot, "admin@test")).user_id, u);
    } finally {
      await k.end();
    }
  });
});
```

- [ ] **Step 3: Chạy để thấy hỏng**

```bash
TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npx tsx --test tests/luu-ho-tro-csdl.test.ts
```
Expected: FAIL ở `before` — `ENOENT … 006_luu_ho_tro.sql`.

- [ ] **Step 4: Viết migration**

Tạo `supabase/migrations/006_luu_ho_tro.sql`:

```sql
-- ============================================================
-- 006: Lưu tùy chọn để hỗ trợ (spec mục 8)
-- users.cho_phep_luu_ho_tro (mặc định TẮT). tac_vu_luu giữ đầu vào + kết quả của một lượt, mã hóa AES-256-GCM PHÍA
-- ỨNG DỤNG (khóa TAC_VU_LUU_KHOA chỉ ở biến môi trường prod; CSDL không bao giờ thấy khóa hay bản rõ), hết hạn 7 ngày.
-- Mọi ghi / xóa / mở đi qua hàm SECURITY DEFINER: service_role không INSERT / UPDATE / DELETE thẳng, chỉ SELECT các cột
-- không phải bản mã — không đọc được bản lưu mà không để lại dòng tac_vu_luu_nhat_ky (chỉ ghi thêm).
-- Dọn hết hạn: tac_vu_luu_xoa_het_han(). Lịch chạy đặt NGOÀI migration (pg_cron hoặc Scheduled Task của Coolify, kế hoạch
-- dot2c Task 11) vì CSDL thử không có pg_cron; mỗi lần chạy ghi một dòng tac_vu_luu_don để kiểm được lịch có chạy.
-- Thứ tự khóa như 004: users (FOR NO KEY UPDATE) trước.
-- ============================================================

SET lock_timeout = '5s';

ALTER TABLE users ADD COLUMN IF NOT EXISTS cho_phep_luu_ho_tro BOOLEAN NOT NULL DEFAULT FALSE;

CREATE TABLE IF NOT EXISTS tac_vu_luu (
    luot_id UUID PRIMARY KEY REFERENCES tac_vu_chay(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    du_lieu_ma_hoa BYTEA NOT NULL,                       -- bản mã + thẻ xác thực GCM 16 byte ở cuối
    iv BYTEA NOT NULL CONSTRAINT ck_tac_vu_luu_iv CHECK (octet_length(iv) = 12),
    het_han TIMESTAMPTZ NOT NULL,
    tao_luc TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS idx_tac_vu_luu_het_han ON tac_vu_luu(het_han);
CREATE INDEX IF NOT EXISTS idx_tac_vu_luu_user ON tac_vu_luu(user_id);

-- Ai mở bản lưu nào, lúc nào. Không khóa ngoại: nhật ký phải còn sau khi bản lưu / lượt / người dùng bị xóa.
CREATE TABLE IF NOT EXISTS tac_vu_luu_nhat_ky (
    id BIGSERIAL PRIMARY KEY,
    luot_id UUID NOT NULL,
    user_id UUID NOT NULL,          -- chủ bản lưu
    boi TEXT NOT NULL,              -- email quản trị đã mở
    luc TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS idx_tac_vu_luu_nhat_ky_luc ON tac_vu_luu_nhat_ky(luc DESC);

-- Mỗi lần chạy dọn một dòng: bằng chứng lịch dọn có chạy (cam kết với khách).
CREATE TABLE IF NOT EXISTS tac_vu_luu_don (
    id BIGSERIAL PRIMARY KEY,
    luc TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    so_xoa INT NOT NULL,
    nguon TEXT NOT NULL             -- 'pg_cron' | 'lich-coolify' | 'tay' | 'kiem-thu'
);
CREATE INDEX IF NOT EXISTS idx_tac_vu_luu_don_luc ON tac_vu_luu_don(luc DESC);

CREATE OR REPLACE FUNCTION tac_vu_luu_nhat_ky_chi_ghi_them() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
    RAISE EXCEPTION 'tac_vu_luu_nhat_ky chỉ ghi thêm, không sửa, không xóa';
END;
$$;
DROP TRIGGER IF EXISTS trg_tac_vu_luu_nhat_ky_chi_ghi_them ON tac_vu_luu_nhat_ky;
CREATE TRIGGER trg_tac_vu_luu_nhat_ky_chi_ghi_them BEFORE UPDATE OR DELETE ON tac_vu_luu_nhat_ky
    FOR EACH ROW EXECUTE FUNCTION tac_vu_luu_nhat_ky_chi_ghi_them();

ALTER TABLE tac_vu_luu ENABLE ROW LEVEL SECURITY;
ALTER TABLE tac_vu_luu_nhat_ky ENABLE ROW LEVEL SECURITY;
ALTER TABLE tac_vu_luu_don ENABLE ROW LEVEL SECURITY;
-- Supabase tự cấp ALL trên bảng / dãy số / hàm mới trong public cho anon, authenticated, service_role: thu hồi hết rồi cấp đúng phần cần.
REVOKE ALL ON tac_vu_luu, tac_vu_luu_nhat_ky, tac_vu_luu_don FROM PUBLIC, anon, authenticated, service_role;
GRANT SELECT (luot_id, user_id, het_han, tao_luc) ON tac_vu_luu TO service_role;
GRANT SELECT ON tac_vu_luu_nhat_ky, tac_vu_luu_don TO service_role;
REVOKE ALL ON SEQUENCE tac_vu_luu_nhat_ky_id_seq, tac_vu_luu_don_id_seq FROM PUBLIC, anon, authenticated, service_role;

-- Ghi bản lưu của một lượt: chỉ khi người dùng đang bật công tắc và lượt thuộc người đó. Trả TRUE nếu đã ghi.
-- Khóa users trước (như 004) để xếp hàng với tac_vu_luu_dat: tắt xong thì không lọt thêm bản nào.
CREATE OR REPLACE FUNCTION tac_vu_luu_ghi(p_luot UUID, p_user UUID, p_du_lieu TEXT, p_iv TEXT) RETURNS BOOLEAN
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
BEGIN
    IF p_du_lieu IS NULL OR p_iv IS NULL THEN RAISE EXCEPTION 'DAU_VAO_SAI'; END IF;
    PERFORM 1 FROM users WHERE id = p_user AND cho_phep_luu_ho_tro FOR NO KEY UPDATE;
    IF NOT FOUND THEN RETURN FALSE; END IF;
    PERFORM 1 FROM tac_vu_chay WHERE id = p_luot AND user_id = p_user;
    IF NOT FOUND THEN RETURN FALSE; END IF;
    INSERT INTO tac_vu_luu (luot_id, user_id, du_lieu_ma_hoa, iv, het_han)
    VALUES (p_luot, p_user, decode(p_du_lieu, 'base64'), decode(p_iv, 'base64'), NOW() + INTERVAL '7 days')
    ON CONFLICT (luot_id) DO NOTHING;
    RETURN FOUND;
END;
$$;

-- Bật / tắt công tắc. Tắt thì xóa ngay mọi bản lưu của người đó (spec: tắt thì không có bản ghi tac_vu_luu).
CREATE OR REPLACE FUNCTION tac_vu_luu_dat(p_user UUID, p_bat BOOLEAN) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE
    n INT := 0;
BEGIN
    IF p_bat IS NULL THEN RAISE EXCEPTION 'DAU_VAO_SAI'; END IF;
    PERFORM 1 FROM users WHERE id = p_user FOR NO KEY UPDATE;
    IF NOT FOUND THEN RAISE EXCEPTION 'tac_vu_luu_dat: không có người dùng %', p_user; END IF;
    UPDATE users SET cho_phep_luu_ho_tro = p_bat WHERE id = p_user;
    IF NOT p_bat THEN
        DELETE FROM tac_vu_luu WHERE user_id = p_user;
        GET DIAGNOSTICS n = ROW_COUNT;
    END IF;
    RETURN jsonb_build_object('bat', p_bat, 'da_xoa', n);
END;
$$;

-- Quản trị mở bản lưu còn hạn: ghi nhật ký rồi mới trả bản mã (base64, bỏ dấu xuống dòng của encode()).
CREATE OR REPLACE FUNCTION tac_vu_luu_mo(p_luot UUID, p_boi TEXT) RETURNS JSONB
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE
    r tac_vu_luu%ROWTYPE;
BEGIN
    IF p_boi IS NULL OR btrim(p_boi) = '' THEN RAISE EXCEPTION 'DAU_VAO_SAI'; END IF;
    SELECT * INTO r FROM tac_vu_luu WHERE luot_id = p_luot AND het_han > NOW();
    IF NOT FOUND THEN RETURN NULL; END IF;
    INSERT INTO tac_vu_luu_nhat_ky (luot_id, user_id, boi) VALUES (r.luot_id, r.user_id, btrim(p_boi));
    RETURN jsonb_build_object(
        'luot_id', r.luot_id, 'user_id', r.user_id,
        'du_lieu', translate(encode(r.du_lieu_ma_hoa, 'base64'), E'\n', ''),
        'iv', translate(encode(r.iv, 'base64'), E'\n', ''),
        'het_han', r.het_han, 'tao_luc', r.tao_luc);
END;
$$;

-- Xóa bản lưu hết hạn, ghi một dòng tac_vu_luu_don (kể cả khi không có gì để xóa), giữ nhật ký dọn 90 ngày.
CREATE OR REPLACE FUNCTION tac_vu_luu_xoa_het_han(p_nguon TEXT DEFAULT 'tay') RETURNS INT
LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, pg_temp AS $$
DECLARE
    n INT;
BEGIN
    DELETE FROM tac_vu_luu WHERE het_han <= NOW();
    GET DIAGNOSTICS n = ROW_COUNT;
    INSERT INTO tac_vu_luu_don (so_xoa, nguon) VALUES (n, COALESCE(NULLIF(btrim(p_nguon), ''), 'tay'));
    DELETE FROM tac_vu_luu_don WHERE luc < NOW() - INTERVAL '90 days';
    RETURN n;
END;
$$;

REVOKE EXECUTE ON FUNCTION tac_vu_luu_ghi(UUID, UUID, TEXT, TEXT), tac_vu_luu_dat(UUID, BOOLEAN), tac_vu_luu_mo(UUID, TEXT),
    tac_vu_luu_xoa_het_han(TEXT), tac_vu_luu_nhat_ky_chi_ghi_them() FROM PUBLIC, anon, authenticated;
GRANT EXECUTE ON FUNCTION tac_vu_luu_ghi(UUID, UUID, TEXT, TEXT), tac_vu_luu_dat(UUID, BOOLEAN), tac_vu_luu_mo(UUID, TEXT),
    tac_vu_luu_xoa_het_han(TEXT) TO service_role;
```

- [ ] **Step 5: Chạy bộ kiểm**

```bash
TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npx tsx --test tests/luu-ho-tro-csdl.test.ts tests/goi-nap-csdl.test.ts tests/credit-csdl.test.ts
```
Expected: PASS hết (bài `SET ROLE` có thể báo SKIP kèm lý do nếu `postgres` của ảnh thử không phải thành viên `service_role`; các bài quyền bằng `has_*_privilege` vẫn phải PASS). Bài «iv sai độ dài» báo lỗi có chữ `ck_tac_vu_luu_iv` (khớp `/iv/`).

- [ ] **Step 6: Chạy lại migration hai lần trên CSDL trống (như Task 1 Step 7, thêm `migrations/006_luu_ho_tro.sql` hai lần cuối danh sách)**

Expected: không lỗi.

- [ ] **Step 7: Commit**

```bash
git add supabase/migrations/006_luu_ho_tro.sql tests/luu-ho-tro-csdl.test.ts tests/ho-tro/csdl-thu.ts
git commit -m "feat(luu-ho-tro): 006 tac_vu_luu, cong tac, nhat ky mo xem chi ghi them, nhat ky don het han

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 7: Mã hóa AES-256-GCM và kho lưu hỗ trợ (mã thuần + lời gọi hàm Postgres)

**Files:**
- Create: `vanbanplus-api/src/lib/luu-ho-tro/ma-hoa.ts`
- Create: `vanbanplus-api/src/lib/luu-ho-tro/kho.ts`
- Test: `vanbanplus-api/tests/luu-ho-tro.test.ts`

**Interfaces:**
- Consumes: hàm SQL `tac_vu_luu_ghi`, `tac_vu_luu_dat`, `tac_vu_luu_mo` (Task 6). Kiểu `BanLuu` do Task 8 khai trong `chay-tac-vu.ts` — Task này tự khai `BanLuu` cục bộ trong `kho.ts` cùng hình dạng rồi Task 8 chuyển sang import (xem Step 3).
- Produces:
  - `ma-hoa.ts`: `docKhoa(giaTri: string | undefined): Buffer | null`; `maHoa(ro: Buffer, khoa: Buffer, gan: string): { du_lieu: Buffer; iv: Buffer }`; `giaiMa(duLieu: Buffer, iv: Buffer, khoa: Buffer, gan: string): Buffer`.
  - `kho.ts`: `GIOI_HAN_BYTE = 8 * 1024 * 1024`; `interface BanGhi { p_luot; p_user; p_du_lieu; p_iv }`; `interface BanLuuMaHoa { luot_id; du_lieu; iv }`;
    `taoLuuHoTro(phu: { khoa(): Buffer | null; ghi(b: BanGhi): Promise<boolean> }): (b: BanLuu) => Promise<void>`;
    `docBanLuu(b: BanLuuMaHoa, khoa: Buffer): unknown`; `ghiBanLuuSupabase(b: BanGhi): Promise<boolean>`;
    `datLuuHoTro(userId: string, bat: boolean): Promise<{ bat: boolean; da_xoa: number }>`; `moBanLuu(luotId: string, boi: string): Promise<BanLuuMaHoa | null>`.

- [ ] **Step 1: Viết bộ kiểm hỏng trước**

Tạo `tests/luu-ho-tro.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { randomBytes } from "node:crypto";
import { docKhoa, giaiMa, maHoa } from "../src/lib/luu-ho-tro/ma-hoa";
import { docBanLuu, GIOI_HAN_BYTE, taoLuuHoTro, type BanGhi } from "../src/lib/luu-ho-tro/kho";

const KHOA = randomBytes(32);
const LUOT = "11111111-2222-3333-4444-555555555555";
const RO = Buffer.from(JSON.stringify({ noi_dung: "Quyết định số 12/QĐ-UBND về chuyển đổi số" }), "utf8");

test("mã hóa rồi giải mã ra đúng dữ liệu gốc", () => {
  const { du_lieu, iv } = maHoa(RO, KHOA, LUOT);
  assert.deepEqual(giaiMa(du_lieu, iv, KHOA, LUOT), RO);
});

test("bản mã không chứa văn bản gốc; iv 12 byte, mỗi lần một iv khác; bản mã dài hơn bản rõ đúng 16 byte (thẻ GCM)", () => {
  const a = maHoa(RO, KHOA, LUOT);
  const b = maHoa(RO, KHOA, LUOT);
  assert.ok(!a.du_lieu.includes("Quyết định"));
  assert.equal(a.iv.length, 12);
  assert.notDeepEqual(a.iv, b.iv);
  assert.equal(a.du_lieu.length, RO.length + 16);
});

test("sửa một byte bản mã, đổi iv, sai khóa, sai lượt (AAD): giải mã báo lỗi", () => {
  const { du_lieu, iv } = maHoa(RO, KHOA, LUOT);
  const sua = Buffer.from(du_lieu);
  sua[0] ^= 1;
  assert.throws(() => giaiMa(sua, iv, KHOA, LUOT));
  assert.throws(() => giaiMa(du_lieu, randomBytes(12), KHOA, LUOT));
  assert.throws(() => giaiMa(du_lieu, iv, randomBytes(32), LUOT));
  assert.throws(() => giaiMa(du_lieu, iv, KHOA, "99999999-2222-3333-4444-555555555555"));
  assert.throws(() => giaiMa(Buffer.alloc(5), iv, KHOA, LUOT), /quá ngắn/);
});

test("docKhoa: base64 đúng 32 byte (cho phép khoảng trắng hai đầu); thiếu, rỗng, sai độ dài thì null", () => {
  assert.deepEqual(docKhoa(KHOA.toString("base64")), KHOA);
  assert.deepEqual(docKhoa(`  ${KHOA.toString("base64")}\n`), KHOA);
  assert.equal(docKhoa(undefined), null);
  assert.equal(docKhoa(""), null);
  assert.equal(docKhoa(randomBytes(16).toString("base64")), null);
});

test("maHoa từ chối khóa sai độ dài", () => {
  assert.throws(() => maHoa(RO, randomBytes(16), LUOT), /32 byte/);
});

test("taoLuuHoTro: ghi bản base64 gắn đúng lượt và người; docBanLuu đọc lại đúng nội dung", async () => {
  const ghi: BanGhi[] = [];
  const luu = taoLuuHoTro({ khoa: () => KHOA, ghi: async (b) => { ghi.push(b); return true; } });
  const ban = { luot_id: LUOT, user_id: "U1", tac_vu: "tom-tat", dau_vao: { noi_dung: "Quyết định số 12" }, ket_qua: { brief: "ok" }, bi_cat: false };
  await luu(ban);
  assert.equal(ghi.length, 1);
  assert.equal(ghi[0].p_luot, LUOT);
  assert.equal(ghi[0].p_user, "U1");
  assert.ok(!Buffer.from(ghi[0].p_du_lieu, "base64").includes("Quyết định"));
  assert.deepEqual(docBanLuu({ luot_id: LUOT, du_lieu: ghi[0].p_du_lieu, iv: ghi[0].p_iv }, KHOA), {
    tac_vu: "tom-tat", dau_vao: { noi_dung: "Quyết định số 12" }, ket_qua: { brief: "ok" }, bi_cat: false,
  });
});

test("docBanLuu đọc được base64 có xuống dòng (kiểu encode() của Postgres)", () => {
  const { du_lieu, iv } = maHoa(RO, KHOA, LUOT);
  const coXuongDong = du_lieu.toString("base64").replace(/(.{76})/g, "$1\n");
  assert.deepEqual(docBanLuu({ luot_id: LUOT, du_lieu: coXuongDong, iv: iv.toString("base64") }, KHOA), JSON.parse(RO.toString("utf8")));
});

test("taoLuuHoTro: thiếu khóa thì không ghi, chỉ báo lỗi không kèm nội dung", async () => {
  let goi = 0;
  const nhat: string[] = [];
  const goc = console.error;
  console.error = (...a: unknown[]) => { nhat.push(a.join(" ")); };
  try {
    await taoLuuHoTro({ khoa: () => null, ghi: async () => { goi++; return true; } })({
      luot_id: LUOT, user_id: "U1", tac_vu: "tom-tat", dau_vao: { noi_dung: "bí mật" }, ket_qua: "x", bi_cat: false,
    });
  } finally {
    console.error = goc;
  }
  assert.equal(goi, 0);
  assert.ok(nhat.some((d) => d.includes(LUOT)));
  assert.ok(!nhat.some((d) => d.includes("bí mật")));
});

test("taoLuuHoTro: bản rõ vượt giới hạn thì không ghi", async () => {
  let goi = 0;
  const goc = console.warn;
  console.warn = () => {};
  try {
    await taoLuuHoTro({ khoa: () => KHOA, ghi: async () => { goi++; return true; } })({
      luot_id: LUOT, user_id: "U1", tac_vu: "trich-xuat", dau_vao: { tep: "a".repeat(GIOI_HAN_BYTE + 1) }, ket_qua: "x", bi_cat: false,
    });
  } finally {
    console.warn = goc;
  }
  assert.equal(goi, 0);
});

test("taoLuuHoTro: ghi ném lỗi thì lỗi đi lên (chayTacVu chặn lại ở Task 8)", async () => {
  const luu = taoLuuHoTro({ khoa: () => KHOA, ghi: async () => { throw new Error("csdl đứt"); } });
  await assert.rejects(luu({ luot_id: LUOT, user_id: "U1", tac_vu: "t", dau_vao: {}, ket_qua: {}, bi_cat: false }), /csdl đứt/);
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

```bash
npx tsx --test tests/luu-ho-tro.test.ts
```
Expected: FAIL — không tìm thấy module `../src/lib/luu-ho-tro/ma-hoa`.

- [ ] **Step 3: Viết mã**

Tạo `src/lib/luu-ho-tro/ma-hoa.ts`:

```ts
import { createCipheriv, createDecipheriv, randomBytes } from "node:crypto";

/** AES-256-GCM cho bản lưu hỗ trợ (spec §8). Hàm thuần, không đụng CSDL. */

export const DO_DAI_KHOA = 32;
const DO_DAI_IV = 12;
const DO_DAI_THE = 16;

/** Khóa từ biến môi trường (base64 của đúng 32 byte). Thiếu hoặc sai thì null — nơi gọi bỏ qua việc lưu. */
export function docKhoa(giaTri: string | undefined): Buffer | null {
  if (!giaTri || !giaTri.trim()) return null;
  const k = Buffer.from(giaTri.trim(), "base64");
  return k.length === DO_DAI_KHOA ? k : null;
}

/** Mã hóa; `gan` (mã lượt) là dữ liệu xác thực kèm: bản mã không dời sang lượt khác được. Thẻ GCM nối cuối bản mã. */
export function maHoa(ro: Buffer, khoa: Buffer, gan: string): { du_lieu: Buffer; iv: Buffer } {
  if (khoa.length !== DO_DAI_KHOA) throw new Error("Khóa lưu hỗ trợ phải đúng 32 byte");
  const iv = randomBytes(DO_DAI_IV);
  const c = createCipheriv("aes-256-gcm", khoa, iv);
  c.setAAD(Buffer.from(gan, "utf8"));
  const than = Buffer.concat([c.update(ro), c.final()]);
  return { du_lieu: Buffer.concat([than, c.getAuthTag()]), iv };
}

/** Giải mã; sai khóa, sai iv, sai lượt hay bản mã bị sửa đều ném lỗi. */
export function giaiMa(duLieu: Buffer, iv: Buffer, khoa: Buffer, gan: string): Buffer {
  if (duLieu.length < DO_DAI_THE) throw new Error("Bản mã quá ngắn");
  const d = createDecipheriv("aes-256-gcm", khoa, iv);
  d.setAAD(Buffer.from(gan, "utf8"));
  d.setAuthTag(duLieu.subarray(duLieu.length - DO_DAI_THE));
  return Buffer.concat([d.update(duLieu.subarray(0, duLieu.length - DO_DAI_THE)), d.final()]);
}
```

Tạo `src/lib/luu-ho-tro/kho.ts`:

```ts
import { getSupabase } from "../supabase";
import { giaiMa, maHoa } from "./ma-hoa";

/** Bản rõ lớn hơn mức này (vd trích xuất nhiều tệp) thì không lưu. */
export const GIOI_HAN_BYTE = 8 * 1024 * 1024;

/** Một lượt đã chốt thành công, cần lưu (Task 8 chuyển sang dùng kiểu cùng tên ở chay-tac-vu.ts). */
export interface BanLuu {
  luot_id: string;
  user_id: string;
  tac_vu: string;
  dau_vao: unknown;
  ket_qua: unknown;
  bi_cat: boolean;
}

/** Tham số của hàm Postgres tac_vu_luu_ghi. */
export interface BanGhi {
  p_luot: string;
  p_user: string;
  p_du_lieu: string;
  p_iv: string;
}

/** Kết quả của tac_vu_luu_mo (base64). */
export interface BanLuuMaHoa {
  luot_id: string;
  du_lieu: string;
  iv: string;
}

/**
 * Dựng hàm lưu hỗ trợ. Không có khóa hoặc bản quá lớn thì bỏ qua (chỉ ghi nhật ký, không kèm nội dung).
 * Lỗi khi ghi thì ném ra: chayTacVu bắt và nuốt, lượt chạy không bị ảnh hưởng.
 */
export function taoLuuHoTro(phu: { khoa(): Buffer | null; ghi(b: BanGhi): Promise<boolean> }) {
  return async (b: BanLuu): Promise<void> => {
    const khoa = phu.khoa();
    if (!khoa) {
      console.error(`🟠 [LUU-HO-TRO] thiếu hoặc sai TAC_VU_LUU_KHOA, không lưu lượt ${b.luot_id}`);
      return;
    }
    const ro = Buffer.from(
      JSON.stringify({ tac_vu: b.tac_vu, dau_vao: b.dau_vao, ket_qua: b.ket_qua, bi_cat: b.bi_cat }),
      "utf8"
    );
    if (ro.length > GIOI_HAN_BYTE) {
      console.warn(`🟠 [LUU-HO-TRO] lượt ${b.luot_id}: ${ro.length} byte, quá ${GIOI_HAN_BYTE}, không lưu`);
      return;
    }
    const { du_lieu, iv } = maHoa(ro, khoa, b.luot_id);
    await phu.ghi({ p_luot: b.luot_id, p_user: b.user_id, p_du_lieu: du_lieu.toString("base64"), p_iv: iv.toString("base64") });
  };
}

/** Giải mã bản lưu đã mở bằng tac_vu_luu_mo. */
export function docBanLuu(b: BanLuuMaHoa, khoa: Buffer): unknown {
  const ro = giaiMa(Buffer.from(b.du_lieu, "base64"), Buffer.from(b.iv, "base64"), khoa, b.luot_id);
  return JSON.parse(ro.toString("utf8"));
}

export async function ghiBanLuuSupabase(b: BanGhi): Promise<boolean> {
  const { data, error } = await getSupabase().rpc("tac_vu_luu_ghi", b);
  if (error) throw new Error(`tac_vu_luu_ghi: ${error.message}`);
  return data === true;
}

export async function datLuuHoTro(userId: string, bat: boolean): Promise<{ bat: boolean; da_xoa: number }> {
  const { data, error } = await getSupabase().rpc("tac_vu_luu_dat", { p_user: userId, p_bat: bat });
  if (error) throw new Error(`tac_vu_luu_dat: ${error.message}`);
  return data as { bat: boolean; da_xoa: number };
}

/** Mở bản lưu còn hạn (hàm Postgres ghi nhật ký trước khi trả). Không có thì null. */
export async function moBanLuu(luotId: string, boi: string): Promise<BanLuuMaHoa | null> {
  const { data, error } = await getSupabase().rpc("tac_vu_luu_mo", { p_luot: luotId, p_boi: boi });
  if (error) throw new Error(`tac_vu_luu_mo: ${error.message}`);
  return (data as BanLuuMaHoa | null) ?? null;
}
```

- [ ] **Step 4: Chạy bộ kiểm**

```bash
npx tsx --test tests/luu-ho-tro.test.ts
```
Expected: PASS hết.

- [ ] **Step 5: Commit**

```bash
git add src/lib/luu-ho-tro tests/luu-ho-tro.test.ts
git commit -m "feat(luu-ho-tro): ma hoa AES-256-GCM gan ma luot, dung ban luu, doc ban luu

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 8: Nối lưu hỗ trợ vào `chayTacVu` (phụ thuộc tùy chọn)

**Files:**
- Modify: `vanbanplus-api/src/lib/tac-vu/chay-tac-vu.ts` (thêm kiểu `BanLuu`, trường tùy chọn `PhuThuoc.luuHoTro`, một khối sau khi chốt)
- Modify: `vanbanplus-api/src/lib/luu-ho-tro/kho.ts` (dùng `BanLuu` từ `chay-tac-vu.ts`)
- Modify: `vanbanplus-api/src/lib/tac-vu/phu-thuoc.ts`
- Modify: `vanbanplus-api/src/types/index.ts` (`ApiUser.cho_phep_luu_ho_tro?`)
- Test: `vanbanplus-api/tests/chay-tac-vu.test.ts` (thêm bài cuối tệp)

**Interfaces:**
- Consumes: `taoLuuHoTro`, `ghiBanLuuSupabase` (Task 7); `docKhoa` (Task 7).
- Produces:
  - `export interface BanLuu { luot_id: string; user_id: string; tac_vu: string; dau_vao: unknown; ket_qua: unknown; bi_cat: boolean }` trong `chay-tac-vu.ts`.
  - `PhuThuoc.luuHoTro?(b: BanLuu): Promise<void>` — gọi đúng một lần, sau `kho.chot`, chỉ khi `ctx.user.cho_phep_luu_ho_tro === true`; lỗi bị nuốt (ghi `console.error` có mã lượt, không có nội dung).
  - `ApiUser.cho_phep_luu_ho_tro?: boolean`.

- [ ] **Step 1: Viết bộ kiểm hỏng trước**

Thêm vào đầu `tests/chay-tac-vu.test.ts` (sau các dòng import hiện có) một dòng:
```ts
import type { BanLuu } from "../src/lib/tac-vu/chay-tac-vu";
```
Thêm vào CUỐI tệp:

```ts
// ---- Lưu hỗ trợ (spec §8) ----

function dungPhuLuu(tra: Array<AICallResult | Error>, luu: PhuThuoc["luuHoTro"], moTra?: KetQuaMo) {
  const d = dungPhu(tra, moTra);
  return { ...d, phu: { ...d.phu, luuHoTro: luu } as PhuThuoc };
}
const USER_BAT = { ...USER, cho_phep_luu_ho_tro: true } as ApiUser;
const CTX_BAT = { ...CTX, user: USER_BAT };

test("đã bật lưu: gọi lưu hỗ trợ đúng một lần SAU khi chốt, kèm đầu vào và kết quả", async () => {
  const banLuu: BanLuu[] = [];
  const d = dungPhu([AI_OK('{"brief":"ok"}')]);
  const phu: PhuThuoc = { ...d.phu, luuHoTro: async (b) => { d.goiKho.push(["luu"]); banLuu.push(b); } };
  const body = { noi_dung: "abc" };
  const kq = await chayTacVu(thuJson, body, CTX_BAT, phu);
  assert.equal(kq.ok, true);
  assert.deepEqual(d.trinhTu(), ["mo", "goiAI", "chot", "luu"]);
  assert.deepEqual(banLuu, [{ luot_id: "L1", user_id: "U1", tac_vu: "thu", dau_vao: body, ket_qua: { brief: "ok" }, bi_cat: false }]);
});

test("chưa bật lưu (mặc định hoặc false): không gọi lưu hỗ trợ", async () => {
  for (const user of [USER, { ...USER, cho_phep_luu_ho_tro: false } as ApiUser]) {
    let goi = 0;
    const { phu } = dungPhuLuu([AI_OK('{"brief":"ok"}')], async () => { goi++; });
    assert.equal((await chayTacVu(thuJson, { noi_dung: "abc" }, { ...CTX, user }, phu)).ok, true);
    assert.equal(goi, 0);
  }
});

test("đã bật lưu nhưng lượt không thành công (AI lỗi, JSON hỏng, thiếu credit, lỗi bất ngờ): không lưu", async () => {
  const truongHop: Array<[Array<AICallResult | Error>, KetQuaMo | undefined]> = [
    [[{ success: false, error: "AI API error 500: boom" }], undefined],
    [[AI_OK("x"), AI_OK('{"khac":1}')], undefined],
    [[], { ma: "KHONG_DU_CREDIT", so_du: 0 }],
    [[new Error("mạng đứt")], undefined],
  ];
  const goc = console.error;
  console.error = () => {};
  try {
    for (const [tra, moTra] of truongHop) {
      let goi = 0;
      const { phu } = dungPhuLuu(tra, async () => { goi++; }, moTra);
      assert.equal((await chayTacVu(thuJson, { noi_dung: "abc" }, CTX_BAT, phu)).ok, false);
      assert.equal(goi, 0);
    }
  } finally {
    console.error = goc;
  }
});

test("lưu hỗ trợ lỗi (bất đồng bộ hoặc ném ngay): lượt vẫn thành công y hệt, không hoàn credit, nhật ký không có nội dung", async () => {
  const hong: Array<PhuThuoc["luuHoTro"]> = [
    async () => { throw new Error("csdl đứt"); },
    (() => { throw new Error("csdl đứt"); }) as unknown as PhuThuoc["luuHoTro"],
  ];
  for (const luu of hong) {
    const nhat: string[] = [];
    const goc = console.error;
    console.error = (...a: unknown[]) => { nhat.push(a.join(" ")); };
    try {
      const { phu, trinhTu } = dungPhuLuu([AI_OK('{"brief":"ok"}')], luu);
      const kq = await chayTacVu(thuJson, { noi_dung: "abc" }, CTX_BAT, phu);
      assert.deepEqual(kq, { ok: true, ket_qua: { brief: "ok" }, credit_da_tru: 2, so_du: 18, ma_luot: "L1", bi_cat: false });
      assert.deepEqual(trinhTu(), ["mo", "goiAI", "chot"]);
    } finally {
      console.error = goc;
    }
    assert.ok(nhat.some((d) => d.includes("L1") && d.includes("csdl đứt")));
    assert.ok(!nhat.some((d) => d.includes("abc")), "nhật ký không được chứa nội dung văn bản");
  }
});

test("đã bật lưu nhưng không có phụ thuộc lưu: chạy bình thường", async () => {
  const { phu } = dungPhu([AI_OK('{"brief":"ok"}')]);
  assert.equal((await chayTacVu(thuJson, { noi_dung: "abc" }, CTX_BAT, phu)).ok, true);
});
```

(Nếu `dot2b` đã đổi chữ ký `dungPhu`, giữ nguyên chữ ký mới và chỉnh `dungPhuLuu` / các lời gọi cho khớp; ý nghĩa bài kiểm không đổi.)

- [ ] **Step 2: Chạy để thấy hỏng**

```bash
npx tsx --test tests/chay-tac-vu.test.ts
```
Expected: FAIL — bài «đã bật lưu: gọi … SAU khi chốt» thấy `trinhTu()` thiếu `"luu"`.

- [ ] **Step 3: Sửa `chay-tac-vu.ts`**

a) Trong `export interface PhuThuoc { … }`, sau dòng `bayGio(): Date;` thêm:
```ts
  /**
   * Tùy chọn (spec §8): lưu mã hóa đầu vào + kết quả để hỗ trợ. Chỉ gọi sau khi chốt thành công và khi người dùng
   * đã bật cho_phep_luu_ho_tro. Lỗi không làm hỏng lượt, không đổi credit.
   */
  luuHoTro?(b: BanLuu): Promise<void>;
```

b) Ngay SAU khối `interface PhuThuoc { … }` thêm:
```ts
export interface BanLuu {
  luot_id: string;
  user_id: string;
  tac_vu: string;
  dau_vao: unknown;
  ket_qua: unknown;
  bi_cat: boolean;
}
```

c) Trong vòng lặp của `chayTacVu`, ngay TRƯỚC dòng `return { ok: true, ket_qua: ketQua, credit_da_tru: chot.credit_tru, …` (sau khối `if (chot.trang_thai !== "xong") { … }`), thêm:
```ts
      if (ctx.user.cho_phep_luu_ho_tro === true && phu.luuHoTro) {
        try {
          await phu.luuHoTro({ luot_id: luot, user_id: ctx.user.id, tac_vu: dn.ten, dau_vao: body, ket_qua: ketQua, bi_cat: biCat });
        } catch (e) {
          // Lưu hỗ trợ không bao giờ làm hỏng lượt đã chốt (không rơi xuống catch ngoài, nơi sẽ hoàn credit).
          console.error(`🟠 [TAC-VU ${dn.ten}] lượt ${luot} lưu hỗ trợ lỗi: ${(e as Error).message}`);
        }
      }
```
Biến `body`, `luot`, `ketQua`, `biCat`, `dn`, `ctx`, `phu` đều có sẵn trong phạm vi đó (nếu `dot2b` đổi tên biến, dùng tên mới tương ứng).

d) `src/types/index.ts`, trong `interface ApiUser`, sau `reset_token_expires?: string | null;` thêm:
```ts
  /** Spec §8: người dùng cho phép lưu mã hóa 7 ngày để hỗ trợ (mặc định false). */
  cho_phep_luu_ho_tro?: boolean;
```

e) `src/lib/luu-ho-tro/kho.ts`: xóa `export interface BanLuu { … }` cục bộ, thay bằng dòng đầu tệp
`import type { BanLuu } from "../tac-vu/chay-tac-vu";` và thêm `export type { BanLuu };` để ai đang import từ `kho` không hỏng.

f) `src/lib/tac-vu/phu-thuoc.ts`: thêm import
```ts
import { ghiBanLuuSupabase, taoLuuHoTro } from "../luu-ho-tro/kho";
import { docKhoa } from "../luu-ho-tro/ma-hoa";
```
và trong `phuThuocThat`, sau `bayGio: () => new Date(),` thêm:
```ts
  // Đọc khóa lúc chạy (không lúc nạp mô-đun) để biến môi trường Coolify luôn có hiệu lực.
  luuHoTro: taoLuuHoTro({ khoa: () => docKhoa(process.env.TAC_VU_LUU_KHOA), ghi: ghiBanLuuSupabase }),
```

- [ ] **Step 4: Chạy bộ kiểm**

```bash
npx tsx --test tests/chay-tac-vu.test.ts tests/luu-ho-tro.test.ts tests/phu-thuoc.test.ts && npx tsc --noEmit -p .
```
Expected: PASS hết; mọi bài cũ của `chay-tac-vu` không đổi kết quả.

- [ ] **Step 5: Commit**

```bash
git add src/lib/tac-vu/chay-tac-vu.ts src/lib/tac-vu/phu-thuoc.ts src/lib/luu-ho-tro/kho.ts src/types/index.ts tests/chay-tac-vu.test.ts
git commit -m "feat(tac-vu): luu ho tro tuy chon sau khi chot, chi khi nguoi dung bat, loi khong anh huong luot

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 9: Công tắc «Lưu để hỗ trợ» trên trang tài khoản

**Files:**
- Create: `vanbanplus-api/src/app/api/tai-khoan/luu-ho-tro/route.ts`
- Modify: `vanbanplus-api/src/app/api/auth/[action]/route.ts` (`handleMe`)
- Modify: `vanbanplus-api/src/app/tai-khoan/page.tsx`

**Interfaces:**
- Consumes: `datLuuHoTro` (Task 7); `ApiUser.cho_phep_luu_ho_tro` (Task 8).
- Produces: `POST /api/tai-khoan/luu-ho-tro { bat: boolean }` → `{ success, message, data: { bat, da_xoa } }`; `/api/auth/me` thêm `choPhepLuuHoTro: boolean`.

- [ ] **Step 1: Đường API**

Tạo `src/app/api/tai-khoan/luu-ho-tro/route.ts`:

```ts
import { NextRequest } from "next/server";
import { validateApiKey } from "@lib/auth";
import { datLuuHoTro } from "@lib/luu-ho-tro/kho";
import { fail, ok, unauthorized } from "@lib/response";

export const dynamic = "force-dynamic";

/** POST /api/tai-khoan/luu-ho-tro {bat} — bật / tắt lưu để hỗ trợ (spec §8). Tắt thì xóa ngay các bản đã lưu. */
export async function POST(req: NextRequest) {
  const user = await validateApiKey(req);
  if (!user) return unauthorized();

  const body = await req.json().catch(() => ({}));
  if (typeof body.bat !== "boolean") return fail("Giá trị bật / tắt không hợp lệ.");

  try {
    const kq = await datLuuHoTro(user.id, body.bat);
    return ok(
      kq,
      kq.bat
        ? "Đã bật lưu để hỗ trợ: từ nay mỗi tác vụ AI được mã hóa và lưu 7 ngày."
        : `Đã tắt lưu để hỗ trợ và xóa ${kq.da_xoa} bản đã lưu.`
    );
  } catch (e) {
    console.error("[LUU-HO-TRO] đổi công tắc lỗi", (e as Error).message);
    return fail("Không đổi được thiết lập. Thử lại sau.", 500);
  }
}
```

- [ ] **Step 2: `/api/auth/me` trả trạng thái công tắc**

Trong `handleMe` của `src/app/api/auth/[action]/route.ts`, sau dòng `lastLoginDate: user.last_login_date,` thêm:
```ts
    choPhepLuuHoTro: user.cho_phep_luu_ho_tro === true,
```

- [ ] **Step 3: Thẻ công tắc trên `/tai-khoan`**

Trong `src/app/tai-khoan/page.tsx`:

a) `interface UsageSummary`: sau `subscriptionExpiry: string | null;` thêm `choPhepLuuHoTro?: boolean;`.

b) Sau `const [goiNap, setGoiNap] = …` (Task 4) thêm:
```ts
  const [luu, setLuu] = useState(false);
  const [luuDang, setLuuDang] = useState(false);
  const [luuTB, setLuuTB] = useState<string | null>(null);
```

c) Trong `loadUsage`, thay `if (res.data) setUsage(res.data);` bằng:
```ts
    if (res.data) {
      setUsage(res.data);
      setLuu(res.data.choPhepLuuHoTro === true);
    }
```

d) Sau hàm `copyApiKey`, thêm:
```ts
  const doiLuu = async () => {
    const bat = !luu;
    if (!bat && !window.confirm("Tắt lưu để hỗ trợ sẽ xóa ngay các bản đã lưu. Tiếp tục?")) return;
    setLuuDang(true);
    const res = await api.post<{ bat: boolean; da_xoa: number }>("/api/tai-khoan/luu-ho-tro", { bat });
    setLuuDang(false);
    setLuuTB(res.message);
    if (res.success && res.data) setLuu(res.data.bat);
  };
```

e) Ngay SAU thẻ đóng `</div>` của thẻ «🪙 Credit AI» (trước chú thích `{/* ☁️ Cloud Sync Status */}`), thêm:
```tsx
              {/* Lưu để hỗ trợ (spec §8) */}
              <div className="bg-white rounded-2xl border border-gray-200 p-6">
                <div className="flex items-start justify-between gap-4">
                  <div>
                    <h3 className="font-bold text-gray-900">🛟 Lưu để hỗ trợ</h3>
                    <p className="text-sm text-gray-500 mt-1">
                      Mặc định tắt: máy chủ không lưu nội dung văn bản gửi cho AI. Khi bật, đầu vào và kết quả của mỗi tác vụ AI
                      được mã hóa và lưu 7 ngày để bộ phận hỗ trợ xem lại khi có lỗi hoặc khiếu nại; mỗi lần xem đều được ghi nhật ký.
                      Tắt thì các bản đã lưu bị xóa ngay. Chi tiết ở{" "}
                      <Link href="/chinh-sach-bao-mat" className="text-primary-600 hover:underline">Chính sách bảo mật</Link>.
                    </p>
                  </div>
                  <button
                    type="button"
                    role="switch"
                    aria-checked={luu}
                    aria-label="Lưu để hỗ trợ"
                    disabled={luuDang || loading}
                    onClick={doiLuu}
                    className={`relative inline-flex h-6 w-11 flex-shrink-0 rounded-full transition-colors disabled:opacity-50 ${luu ? "bg-primary-600" : "bg-gray-300"}`}
                  >
                    <span className={`inline-block h-5 w-5 mt-0.5 rounded-full bg-white shadow transition-transform ${luu ? "translate-x-5" : "translate-x-0.5"}`} />
                  </button>
                </div>
                {luuTB && <p className="text-xs text-gray-600 mt-3">{luuTB}</p>}
              </div>
```

- [ ] **Step 4: Kiểm kiểu, soát lời**

```bash
npx tsc --noEmit -p . && grep -nE "\b([Tt]ôi|[Tt]a|[Aa]nh|[Ee]m|[Bb]ạn|[Cc]hị|[Mm]ình|[Dd]ạ|[Vv]âng)\b|ạ[.,!? ]" src/app/api/tai-khoan/luu-ho-tro/route.ts src/app/tai-khoan/page.tsx
```
Expected: `tsc` sạch; `grep` không in dòng nào thuộc phần mới.

- [ ] **Step 5: Commit**

```bash
git add src/app/api/tai-khoan src/app/api/auth src/app/tai-khoan/page.tsx
git commit -m "feat(tai-khoan): cong tac luu de ho tro, tat thi xoa ngay ban da luu

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 10: Quản trị «mở bản lưu», nhật ký mở xem, trạng thái lịch dọn

**Files:**
- Modify: `vanbanplus-api/src/app/api/admin/credit/route.ts`
- Modify: `vanbanplus-api/src/app/admin/credit/page.tsx`

**Interfaces:**
- Consumes: `moBanLuu`, `docBanLuu` (Task 7); `docKhoa` (Task 7); bảng `tac_vu_luu` (cột đọc được), `tac_vu_luu_nhat_ky`, `tac_vu_luu_don` (Task 6).
- Produces:
  - `GET /api/admin/credit` thêm `ban_luu: string[]` (mã lượt còn bản lưu trong 100 lượt đang hiện), `don_gan_nhat: { luc, so_xoa, nguon } | null`, `nhat_ky: { luot_id, boi, luc }[]` (20 dòng gần nhất).
  - `POST /api/admin/credit { action: "mo_ban_luu", luot_id }` → `{ luot_id, noi_dung }` (nội dung đã giải mã).

- [ ] **Step 1: Đường API**

Trong `src/app/api/admin/credit/route.ts`:

a) Thêm import:
```ts
import { docBanLuu, moBanLuu } from "@lib/luu-ho-tro/kho";
import { docKhoa } from "@lib/luu-ho-tro/ma-hoa";
```

b) Trong `GET`, thay dòng `return ok({ bang_gia: bang_gia ?? [], luot: luot ?? [] });` bằng:
```ts
  const ids = (luot ?? []).map((l) => l.id as string);
  const coLuu = ids.length
    ? await sb.from("tac_vu_luu").select("luot_id").in("luot_id", ids).gt("het_han", new Date().toISOString())
    : { data: [], error: null };
  const [don, nhatKy] = await Promise.all([
    sb.from("tac_vu_luu_don").select("luc, so_xoa, nguon").order("luc", { ascending: false }).limit(1),
    sb.from("tac_vu_luu_nhat_ky").select("luot_id, boi, luc").order("luc", { ascending: false }).limit(20),
  ]);
  if (coLuu.error || don.error || nhatKy.error) {
    console.error("[ADMIN CREDIT] đọc dữ liệu lưu hỗ trợ lỗi", coLuu.error ?? don.error ?? nhatKy.error);
  }
  return ok({
    bang_gia: bang_gia ?? [],
    luot: luot ?? [],
    ban_luu: ((coLuu.data ?? []) as Array<{ luot_id: string }>).map((r) => r.luot_id),
    don_gan_nhat: (don.data?.[0] as { luc: string; so_xoa: number; nguon: string } | undefined) ?? null,
    nhat_ky: nhatKy.data ?? [],
  });
```

c) Trong `POST`, trước dòng cuối `return fail("Thao tác không hợp lệ.");` thêm:
```ts
  if (body.action === "mo_ban_luu") {
    const luotId = typeof body.luot_id === "string" ? body.luot_id.trim() : "";
    if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(luotId)) return fail("Mã lượt không hợp lệ.");
    const khoa = docKhoa(process.env.TAC_VU_LUU_KHOA);
    if (!khoa) return fail("Máy chủ chưa có khóa lưu hỗ trợ (TAC_VU_LUU_KHOA).", 500);
    let ban;
    try {
      ban = await moBanLuu(luotId, admin.email);
    } catch (e) {
      console.error("[ADMIN CREDIT] mở bản lưu lỗi", (e as Error).message);
      return fail("Không mở được bản lưu.", 500);
    }
    if (!ban) return fail("Không có bản lưu cho lượt này (chưa bật lưu, hoặc đã quá 7 ngày).", 404);
    try {
      return ok({ luot_id: luotId, noi_dung: docBanLuu(ban, khoa) }, "Đã mở bản lưu. Lần mở này đã được ghi nhật ký.");
    } catch (e) {
      console.error("[ADMIN CREDIT] giải mã bản lưu lỗi", (e as Error).message);
      return fail("Không giải mã được bản lưu (khóa đã đổi?). Lần mở vẫn được ghi nhật ký.", 500);
    }
  }
```

- [ ] **Step 2: Trang `/admin/credit`**

Trong `src/app/admin/credit/page.tsx`:

a) Sau `const TRANG_THAI …` thêm:
```ts
interface DonGanNhat {
  luc: string;
  so_xoa: number;
  nguon: string;
}

interface NhatKyMo {
  luot_id: string;
  boi: string;
  luc: string;
}

/** Lịch dọn chạy mỗi giờ: quá 3 giờ không có lần chạy nào là lịch hỏng. */
const DON_TRE_MS = 3 * 3600 * 1000;
```

b) Sau `const [dc, setDc] = …` thêm:
```ts
  const [banLuu, setBanLuu] = useState<Set<string>>(new Set());
  const [don, setDon] = useState<DonGanNhat | null>(null);
  const [nhatKy, setNhatKy] = useState<NhatKyMo[]>([]);
  const [banMo, setBanMo] = useState<{ luot_id: string; noi_dung: unknown } | null>(null);
```

c) Trong `load`, đổi kiểu `api.get<{ bang_gia: DongGia[]; luot: Luot[] }>` thành
`api.get<{ bang_gia: DongGia[]; luot: Luot[]; ban_luu: string[]; don_gan_nhat: DonGanNhat | null; nhat_ky: NhatKyMo[] }>`
và sau `setLuot(res.data.luot);` thêm:
```ts
      setBanLuu(new Set(res.data.ban_luu ?? []));
      setDon(res.data.don_gan_nhat ?? null);
      setNhatKy(res.data.nhat_ky ?? []);
```

d) Sau hàm `dieuChinh`, thêm:
```ts
  const moBan = async (l: Luot) => {
    if (!window.confirm(`Mở bản lưu của lượt ${l.tac_vu} (${l.users?.email ?? "?"})? Lần mở được ghi nhật ký.`)) return;
    const res = await api.post<{ luot_id: string; noi_dung: unknown }>("/api/admin/credit", { action: "mo_ban_luu", luot_id: l.id });
    setMessage(res.message);
    if (res.success && res.data) setBanMo(res.data);
    load();
  };
  const donTre = !don || Date.now() - new Date(don.luc).getTime() > DON_TRE_MS;
```

e) Ngay TRƯỚC `<section className="bg-white rounded-xl border border-gray-200 overflow-x-auto">` (bảng lượt chạy), thêm:
```tsx
      <section className="bg-white rounded-xl border border-gray-200 p-4 mb-6">
        <h2 className="font-semibold text-gray-900 mb-2">Lưu để hỗ trợ</h2>
        <p className={`text-sm ${donTre ? "text-red-600 font-semibold" : "text-gray-600"}`}>
          {don
            ? `Dọn bản lưu hết hạn: lần gần nhất ${new Date(don.luc).toLocaleString("vi-VN")} (${don.nguon}), xóa ${don.so_xoa} bản.`
            : "Chưa có lần dọn nào."}
          {donTre && " Lịch dọn không chạy quá 3 giờ — kiểm lại (kế hoạch dot2c Task 11)."}
        </p>
        {nhatKy.length > 0 && (
          <ul className="mt-3 text-xs text-gray-500 space-y-1">
            {nhatKy.map((n, i) => (
              <li key={i}>{new Date(n.luc).toLocaleString("vi-VN")} · {n.boi} mở lượt <span className="font-mono">{n.luot_id}</span></li>
            ))}
          </ul>
        )}
        {banMo && (
          <div className="mt-4">
            <div className="flex justify-between text-sm mb-1">
              <span className="font-mono">{banMo.luot_id}</span>
              <button className="text-gray-500 hover:underline" onClick={() => setBanMo(null)}>Đóng</button>
            </div>
            <pre className="text-xs bg-gray-50 border rounded p-3 max-h-96 overflow-auto whitespace-pre-wrap">
              {JSON.stringify(banMo.noi_dung, null, 2)}
            </pre>
          </div>
        )}
      </section>
```

f) Bảng lượt chạy: thêm `<th className="px-4 py-3">Bản lưu</th>` sau `<th className="px-4 py-3">Mô hình · nguồn</th>`; đổi `colSpan={7}` thành `colSpan={8}`; trong mỗi hàng, sau ô «Mô hình · nguồn», thêm:
```tsx
                <td className="px-4 py-3">
                  {banLuu.has(l.id) && (
                    <button className="text-primary-600 hover:underline" onClick={() => moBan(l)}>Mở</button>
                  )}
                </td>
```

g) `PageHeader` description: `"Bảng giá credit, điều chỉnh tay, lượt chạy AI gần đây và bản lưu hỗ trợ"`.

- [ ] **Step 3: Kiểm kiểu**

```bash
npx tsc --noEmit -p .
```
Expected: sạch.

- [ ] **Step 4: Commit**

```bash
git add src/app/api/admin/credit/route.ts src/app/admin/credit/page.tsx
git commit -m "feat(admin): mo ban luu ho tro co ghi nhat ky, hien trang thai lich don

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 11: Lịch dọn bản lưu hết hạn — kiểm `pg_cron`, chọn đường

**Files (theo nhánh):**
- Nhánh A (có `pg_cron`): Create `vanbanplus-api/supabase/lich/006-don-luu-ho-tro-pg-cron.sql`
- Nhánh B (không có): Create `vanbanplus-api/src/lib/noi-bo.ts`, `vanbanplus-api/src/app/api/noi-bo/don-luu-ho-tro/route.ts`; Test `vanbanplus-api/tests/noi-bo.test.ts`

**Interfaces:**
- Consumes: `tac_vu_luu_xoa_het_han(p_nguon)` (Task 6).
- Produces: nhánh A — job pg_cron tên `vbp-don-luu-ho-tro`, lịch `17 * * * *`, ghi `nguon = 'pg_cron'`. Nhánh B — `POST /api/noi-bo/don-luu-ho-tro` (header `Authorization: Bearer <VBP_NOI_BO_KHOA>`) → `{ success: true, so_xoa }`, ghi `nguon = 'lich-coolify'`; `export function khoaNoiBoDung(header: string | null, khoa: string | undefined): boolean`.
  Cả hai nhánh: kiểm được bằng `tac_vu_luu_don` và dòng «Dọn bản lưu hết hạn» ở `/admin/credit`.

- [ ] **Step 1: Kiểm `pg_cron` trên prod (CHỈ ĐỌC)**

```bash
ssh gikidi-dev bash -s <<'EOF'
D=supabase-db-q323olywghtnjcvpyvtjzo9v
sudo docker exec $D psql -U postgres -d postgres -Atc "select name, default_version, coalesce(installed_version, '-') from pg_available_extensions where name = 'pg_cron'"
sudo docker exec $D psql -U postgres -d postgres -Atc "show shared_preload_libraries"
EOF
```
Quy tắc chọn:
- Dòng đầu có `pg_cron|<phiên bản>|…` **và** `shared_preload_libraries` có chữ `pg_cron` → **nhánh A** (Step 2A).
- Còn lại → **nhánh B** (Step 2B–5B). Không sửa `shared_preload_libraries` của prod (phải khởi động lại CSDL).

Ghi kết quả (A hoặc B + hai dòng đầu ra) vào thân commit của task này.

- [ ] **Step 2A (nhánh A): tệp lịch pg_cron**

Tạo `supabase/lich/006-don-luu-ho-tro-pg-cron.sql`:

```sql
-- Lịch dọn bản lưu hỗ trợ hết hạn (spec §8), phút 17 mỗi giờ.
-- Chạy TAY trên prod SAU migration 006 (không nằm trong migration vì CSDL thử không nạp pg_cron).
-- Chạy lại an toàn: cron.schedule cùng tên thì cập nhật job cũ. Gỡ: SELECT cron.unschedule('vbp-don-luu-ho-tro');
-- Kiểm: SELECT luc, so_xoa, nguon FROM tac_vu_luu_don ORDER BY luc DESC LIMIT 3;
--       SELECT status, start_time FROM cron.job_run_details d JOIN cron.job j USING (jobid)
--        WHERE j.jobname = 'vbp-don-luu-ho-tro' ORDER BY start_time DESC LIMIT 3;
CREATE EXTENSION IF NOT EXISTS pg_cron;
SELECT cron.schedule('vbp-don-luu-ho-tro', '17 * * * *', $$SELECT public.tac_vu_luu_xoa_het_han('pg_cron')$$);
```
Thử cú pháp trên CSDL thử (`vbp-csdl-thu` cùng ảnh): `ssh gikidi-dev "PGPASSWORD=thu psql -h 127.0.0.1 -p 55990 -U postgres -d postgres -v ON_ERROR_STOP=1 -f -" < supabase/lich/006-don-luu-ho-tro-pg-cron.sql`.
Chạy được thì gỡ ngay: `ssh gikidi-dev "PGPASSWORD=thu psql -h 127.0.0.1 -p 55990 -U postgres -d postgres -c \"select cron.unschedule('vbp-don-luu-ho-tro')\""` (job trên container thử sẽ chạy lỗi vì CSDL `postgres` ở đó không có 006 — không sao, gỡ là xong).
Container thử báo `pg_cron … must be loaded via shared_preload_libraries` thì bỏ qua lần thử này: kết quả Step 1 trên prod là căn cứ, lệnh chạy thật ở Task 13 Step 6. Bỏ qua Step 2B–5B.

Commit:
```bash
git add supabase/lich/006-don-luu-ho-tro-pg-cron.sql
git commit -m "chore(luu-ho-tro): lich pg_cron don ban luu het han (prod co pg_cron: <dán 2 dòng kết quả Step 1>)

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

- [ ] **Step 2B (nhánh B): bộ kiểm khóa nội bộ, hỏng trước**

Tạo `tests/noi-bo.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { khoaNoiBoDung } from "../src/lib/noi-bo";

const KHOA = "k".repeat(43);

test("đúng Bearer + khóa thì qua", () => {
  assert.equal(khoaNoiBoDung(`Bearer ${KHOA}`, KHOA), true);
});

test("sai khóa, thiếu header, thiếu chữ Bearer thì chặn", () => {
  assert.equal(khoaNoiBoDung(`Bearer ${KHOA}x`, KHOA), false);
  assert.equal(khoaNoiBoDung(null, KHOA), false);
  assert.equal(khoaNoiBoDung(KHOA, KHOA), false);
});

test("máy chủ chưa đặt khóa hoặc khóa quá ngắn thì luôn chặn", () => {
  assert.equal(khoaNoiBoDung("Bearer ", undefined), false);
  assert.equal(khoaNoiBoDung("Bearer ", ""), false);
  assert.equal(khoaNoiBoDung("Bearer ngan", "ngan"), false);
});
```
Run: `npx tsx --test tests/noi-bo.test.ts` → FAIL (module không có).

- [ ] **Step 3B: Mã**

Tạo `src/lib/noi-bo.ts`:

```ts
import { createHash, timingSafeEqual } from "crypto";

const bam = (s: string) => createHash("sha256").update(s).digest();

/** Đường nội bộ (lịch Coolify gọi): header phải là «Bearer <VBP_NOI_BO_KHOA>»; khóa máy chủ thiếu / ngắn hơn 32 ký tự thì luôn chặn. */
export function khoaNoiBoDung(header: string | null, khoa: string | undefined): boolean {
  if (!khoa || khoa.length < 32) return false;
  return timingSafeEqual(bam(header ?? ""), bam(`Bearer ${khoa}`));
}
```

Tạo `src/app/api/noi-bo/don-luu-ho-tro/route.ts`:

```ts
import { NextRequest, NextResponse } from "next/server";
import { getSupabase } from "@lib/supabase";
import { khoaNoiBoDung } from "@lib/noi-bo";

export const dynamic = "force-dynamic";

/** POST /api/noi-bo/don-luu-ho-tro — Scheduled Task của Coolify gọi mỗi giờ: xóa bản lưu hỗ trợ hết hạn (spec §8). */
export async function POST(req: NextRequest) {
  if (!khoaNoiBoDung(req.headers.get("authorization"), process.env.VBP_NOI_BO_KHOA)) {
    return NextResponse.json({ success: false }, { status: 401 });
  }
  const { data, error } = await getSupabase().rpc("tac_vu_luu_xoa_het_han", { p_nguon: "lich-coolify" });
  if (error) {
    console.error("[NOI-BO] dọn bản lưu lỗi", error.message);
    return NextResponse.json({ success: false }, { status: 500 });
  }
  return NextResponse.json({ success: true, so_xoa: data });
}
```

- [ ] **Step 4B: Chạy bộ kiểm**

```bash
npx tsx --test tests/noi-bo.test.ts && npx tsc --noEmit -p .
```
Expected: PASS; `tsc` sạch.

- [ ] **Step 5B: Commit**

```bash
git add src/lib/noi-bo.ts src/app/api/noi-bo tests/noi-bo.test.ts
git commit -m "feat(luu-ho-tro): duong noi bo don ban luu het han cho lich Coolify (prod khong co pg_cron: <dán 2 dòng kết quả Step 1>)

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```
(Lịch Coolify tạo ở Task 14 Step 3, SAU khi mã đã lên — tạo trước thì lịch gọi vào 404.)

---

### Task 12: Chính sách bảo mật và hướng dẫn nói đúng mục 8 và §13.1

**Files:**
- Modify: `vanbanplus-api/src/app/chinh-sach-bao-mat/page.tsx`
- Modify: `vanbanplus-api/src/app/huong-dan/page.tsx` (một câu trả lời FAQ)

**Interfaces:**
- Consumes: không.
- Produces: mục mới «Lưu để hỗ trợ (tùy chọn)» là mục thứ 5 (ngay sau «Nội dung gửi cho AI», nên câu «như mục 4» ở mục «Chia sẻ dữ liệu» vẫn đúng).

- [ ] **Step 1: Kiểm thời gian giữ bản sao lưu trước khi ghi con số**

```bash
grep -n "forget" /d/Thang/gikidi-server/may-chu/sao-luu/sao-luu
```
Expected: `restic forget --tag dem --keep-daily 7 --keep-weekly 4 --keep-monthly 6 --prune` → bản sao lưu cũ nhất khoảng 6 tháng. Nếu số khác, sửa cụm «khoảng 6 tháng» ở Step 2 cho khớp.

- [ ] **Step 2: Sửa `src/app/chinh-sach-bao-mat/page.tsx`**

a) Mục «Dữ liệu được thu thập», danh sách đầu: thêm phần tử ngay sau phần tử «Sử dụng AI: …»:
```ts
        "Lưu để hỗ trợ (chỉ khi người dùng bật ở trang Tài khoản, mặc định tắt): đầu vào và kết quả của từng tác vụ AI, ở dạng mã hóa, giữ 7 ngày.",
```

b) Mục «Nội dung gửi cho AI»: thay phần tử bắt đầu bằng `"Khi Vertex AI gián đoạn, yêu cầu có thể được chuyển qua Gemini API của Google AI Studio.` bằng:
```ts
        "Các tác vụ AI tính credit không bao giờ chuyển sang Google AI Studio: khi Vertex AI gián đoạn, tác vụ báo lỗi và hoàn credit. Riêng phần mềm phiên bản cũ còn gọi AI qua đường cũ: khi Vertex AI gián đoạn, yêu cầu có thể được chuyển qua Gemini API của Google AI Studio; với dịch vụ không trả phí của AI Studio, điều khoản của Google cho phép Google dùng nội dung để cải thiện sản phẩm.",
```
và trong đoạn đầu của mục này đổi cụm `Máy chủ VanBanPlus mặc định không lưu nội dung này;` thành `Máy chủ VanBanPlus mặc định không lưu nội dung này (trừ khi người dùng bật «Lưu để hỗ trợ», xem mục 5);`.

c) Ngay SAU đối tượng mục «Nội dung gửi cho AI» (trước mục «Chia sẻ dữ liệu với bên thứ ba»), thêm:
```ts
  {
    heading: "Lưu để hỗ trợ (tùy chọn)",
    body: [
      "Mặc định tắt: máy chủ VanBanPlus không lưu nội dung văn bản gửi cho AI.",
      [
        "Khi bật công tắc «Lưu để hỗ trợ» ở trang Tài khoản, đầu vào và kết quả của mỗi tác vụ AI được mã hóa (AES-256-GCM) rồi lưu trên máy chủ để bộ phận hỗ trợ xem lại khi có lỗi hoặc khiếu nại.",
        "Bản lưu tự xóa sau 7 ngày. Tắt công tắc thì các bản đã lưu bị xóa ngay.",
        "Chỉ nhân sự hỗ trợ được giao mới mở được bản lưu; mỗi lần mở đều ghi nhật ký người mở, thời điểm và lượt tác vụ.",
        "Khóa giải mã nằm riêng ở máy chủ ứng dụng, không nằm trong cơ sở dữ liệu. Khóa bị thay hoặc mất thì các bản đã lưu không đọc lại được.",
      ],
      "Giới hạn: sau khi bị xóa, bản mã hóa vẫn còn trong các bản sao lưu định kỳ cũ của máy chủ thêm một thời gian, tối đa khoảng 6 tháng, tới khi bản sao lưu đó hết hạn. Bản sao lưu cũng được mã hóa và chỉ dùng để khôi phục khi có sự cố.",
    ],
  },
```

d) Mục «Thời gian lưu trữ», danh sách: thêm phần tử ngay sau phần tử «Nhật ký sử dụng AI và credit: …»:
```ts
        "Bản lưu để hỗ trợ (nếu bật): 7 ngày, hoặc tới khi tắt công tắc.",
```

e) `effectiveDate="16/11/2026"` → ngày đẩy `main` của Task 14, dạng `dd/MM/yyyy`.

- [ ] **Step 3: Sửa câu trả lời FAQ ở `src/app/huong-dan/page.tsx`**

Thay `a` của mục `q: "AI có gửi nội dung VB lên server không?"` bằng:
```ts
"Nội dung VB đi qua máy chủ VanBanPlus tới nhà cung cấp AI để xử lý. Máy chủ VanBanPlus mặc định không lưu nội dung VB; chỉ khi bật «Lưu để hỗ trợ» ở trang Tài khoản thì đầu vào và kết quả được mã hóa và lưu 7 ngày."
```

- [ ] **Step 4: Soát lời + kiểm kiểu**

```bash
grep -nE "\b([Tt]ôi|[Tt]a|[Aa]nh|[Ee]m|[Bb]ạn|[Cc]hị|[Mm]ình|[Dd]ạ|[Vv]âng)\b|ạ[.,!? ]" src/app/chinh-sach-bao-mat/page.tsx src/app/huong-dan/page.tsx; npx tsc --noEmit -p .
```
Expected: không dòng nào thuộc phần vừa sửa (mục FAQ «Tôi quên mật khẩu thì sao?» có sẵn từ trước, ngoài phạm vi — để nguyên, ghi vào báo cáo cuối như một việc tồn); `tsc` sạch.

- [ ] **Step 5: Commit**

```bash
git add src/app/chinh-sach-bao-mat/page.tsx src/app/huong-dan/page.tsx
git commit -m "docs(chinh-sach): muc luu de ho tro (ma hoa, 7 ngay, nhat ky, gioi han sao luu), v2 khong chuyen AI Studio

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 13: Chuẩn bị prod — bộ kiểm đủ, thử trên bản sao, migrate, khóa, lịch pg_cron

**Files:**
- Modify: `D:\Thang\gikidi-server\THAY-DOI.md` (một mục)
- Bí mật (không vào git): `C:\Users\PC\.gikidi\vanbanplus-tac-vu-luu.env`, bản chép `/home/dev/.gikidi/vanbanplus-tac-vu-luu.env`; nhánh B thêm `vanbanplus-noi-bo.env`.

**Interfaces:**
- Consumes: mọi task trước; kết quả nhánh A/B của Task 11.
- Produces: CSDL prod có 005 + 006; biến Coolify `TAC_VU_LUU_KHOA` (nhánh B thêm `VBP_NOI_BO_KHOA`), chỉ lúc chạy; nhánh A: job `vbp-don-luu-ho-tro`.

- [ ] **Step 1: Đồng bộ với `main`, bộ kiểm đủ + build**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git fetch origin && git merge --no-edit origin/main
TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npm test && npx next build
```
Expected: không xung đột (có thì giải, chạy lại); mọi bộ kiểm PASS, kể cả `credit-csdl`, `goi-nap-csdl`, `luu-ho-tro-csdl` (không bị SKIP vì thiếu `TEST_DATABASE_URL`); build xong.

- [ ] **Step 2: Thử 005 + 006 trên bản sao prod (mỗi tệp chạy 2 lần), xóa bản sao ngay**

```bash
scp -q supabase/migrations/005_goi_nap.sql supabase/migrations/006_luu_ho_tro.sql gikidi-dev:/tmp/
ssh gikidi-dev bash -s <<'EOF'
set -e
PROD=supabase-db-q323olywghtnjcvpyvtjzo9v
sudo docker exec $PROD pg_dump -U postgres -d postgres -n public --no-owner > /tmp/vbp-ban-sao.sql
export PGPASSWORD=thu; P='psql -h 127.0.0.1 -p 55990 -U postgres -q'
$P -d postgres -c 'drop database if exists vbp_ban_sao with (force)' -c 'create database vbp_ban_sao'
$P -d vbp_ban_sao -f /tmp/vbp-ban-sao.sql >/dev/null 2>/tmp/vbp-nap.err || true
echo 'Lỗi khi nạp bản sao:'; grep ERROR /tmp/vbp-nap.err | sort | uniq -c | head
$P -d vbp_ban_sao -Atc 'select count(*) from users; select count(*) from orders; select count(*) from credit_lo'
for f in 005_goi_nap 005_goi_nap 006_luu_ho_tro 006_luu_ho_tro; do $P -d vbp_ban_sao -v ON_ERROR_STOP=1 -1 -f /tmp/$f.sql && echo "ok $f"; done
$P -d vbp_ban_sao -Atc "select loai, count(*) from orders group by loai"
# Thử chức năng trên bản sao: đơn nạp của người dùng đầu tiên, trả, xem lô; bật / tắt lưu.
$P -d vbp_ban_sao -Atc "
  with u as (select id from users order by created_date limit 1)
  insert into orders (code, user_id, loai, goi_nap_ma, amount_vnd) select 'VBPTHU23456', id, 'nap', 'nap-49', 49000 from u;
  select pay_order('VBPTHU23456', 'admin', 'thu-ban-sao');
  select l.nguon, l.so_cap, l.het_han - o.paid_at from credit_lo l join orders o on o.id = l.order_id where o.code = 'VBPTHU23456';
  select tac_vu_luu_dat((select id from users order by created_date limit 1), true);
  select tac_vu_luu_dat((select id from users order by created_date limit 1), false);
  select tac_vu_luu_xoa_het_han('thu');"
$P -d postgres -c 'drop database vbp_ban_sao with (force)'
rm -f /tmp/vbp-ban-sao.sql /tmp/vbp-nap.err
EOF
```
Expected: lỗi khi nạp chỉ ở phần không liên quan (vd `schema "public" already exists`, kiểu `vector` của bảng cloud); `users`, `orders`, `credit_lo` nạp đủ; bốn dòng `ok …`; mọi đơn cũ có `loai = goi`; thử chức năng in `paid`, `nap|160|90 days`, `{"bat": true, "da_xoa": 0}`, `{"bat": false, "da_xoa": 0}`, `0`. Bản sao và tệp dump bị xóa ngay (có dữ liệu người dùng thật). Có lỗi thì dừng, tìm nguyên nhân trước khi migrate prod.

- [ ] **Step 3: Tạo khóa `TAC_VU_LUU_KHOA` (không in), sao lưu, đặt biến Coolify**

Tạo khóa trên PC, không bao giờ ghi đè khóa đã có (ghi đè = mất mọi bản đã lưu):
```bash
C:/Python313/python.exe -c "
import base64, pathlib, secrets
p = pathlib.Path.home() / '.gikidi' / 'vanbanplus-tac-vu-luu.env'
assert not p.exists(), 'đã có khóa, không ghi đè'
p.write_text('TAC_VU_LUU_KHOA=' + base64.b64encode(secrets.token_bytes(32)).decode() + '\n')
print('đã tạo', p, p.stat().st_size, 'byte')"
scp -q C:/Users/PC/.gikidi/vanbanplus-tac-vu-luu.env gikidi-dev:.gikidi/ && ssh gikidi-dev 'chmod 600 ~/.gikidi/vanbanplus-tac-vu-luu.env && ls -l ~/.gikidi/vanbanplus-tac-vu-luu.env'
```
Expected: «đã tạo … 61 byte»; bản chép trên `dev` quyền `-rw-------`.

Nhánh B thêm khóa đường nội bộ (cùng cách, tệp khác):
```bash
C:/Python313/python.exe -c "
import pathlib, secrets
p = pathlib.Path.home() / '.gikidi' / 'vanbanplus-noi-bo.env'
assert not p.exists(), 'đã có khóa, không ghi đè'
p.write_text('VBP_NOI_BO_KHOA=' + secrets.token_urlsafe(32) + '\n')
print('đã tạo', p)"
scp -q C:/Users/PC/.gikidi/vanbanplus-noi-bo.env gikidi-dev:.gikidi/ && ssh gikidi-dev 'chmod 600 ~/.gikidi/vanbanplus-noi-bo.env'
```

Đặt biến Coolify (chỉ lúc chạy, không vào image — như `--env-chay` của `cong-cu/du-an-moi.py`), kiểm đúng app trước khi ghi:
```bash
cd /d/Thang/gikidi-server && C:/Python313/python.exe - <<'PY'
import pathlib, sys
sys.path.insert(0, 'chuyen')
from coolify import api
APP = 'wfxjk8gpdrwfanx6rxi28sqz'
app = api('GET', f'applications/{APP}')
ten = f"{app.get('name', '')} {app.get('fqdn', '')}"
assert 'vanbanplus' in ten, f'sai app: {ten}'
tep = ['vanbanplus-tac-vu-luu.env'] + (['vanbanplus-noi-bo.env'] if (pathlib.Path.home() / '.gikidi' / 'vanbanplus-noi-bo.env').exists() else [])
co = {e['key'] for e in api('GET', f'applications/{APP}/envs')}
for t in tep:
    for dong in (pathlib.Path.home() / '.gikidi' / t).read_text().splitlines():
        if '=' not in dong:
            continue
        k, v = dong.split('=', 1)
        body = {'key': k, 'value': v.strip(), 'is_preview': False, 'is_literal': True, 'is_shown_once': True,
                'is_buildtime': False, 'is_runtime': True}
        api('PATCH' if k in co else 'POST', f'applications/{APP}/envs', json=body)
        print(k, 'đã đặt')
sai = [e['key'] for e in api('GET', f'applications/{APP}/envs')
       if e['key'] in ('TAC_VU_LUU_KHOA', 'VBP_NOI_BO_KHOA') and not e.get('is_preview') and (e.get('is_buildtime') or not e.get('is_runtime'))]
print('cờ build SAI:', sai) if sai else print('chỉ lúc chạy: đúng')
print('cổng app:', app.get('ports_exposes'))
PY
```
Expected: `TAC_VU_LUU_KHOA đã đặt` (nhánh B thêm `VBP_NOI_BO_KHOA đã đặt`), `chỉ lúc chạy: đúng`, in cổng app (ghi lại cho Task 14 Step 3, thường `3000`). Báo «cờ build SAI» thì sửa tay trong giao diện Coolify trước khi đẩy `main`. Biến có hiệu lực ở lần triển khai kế tiếp (Task 14 đẩy `main`).

- [ ] **Step 4: ĐIỂM DỪNG — báo chủ dự án trước khi migrate prod**

Báo một dòng: «Sắp migrate CSDL prod VanBanPlus (005: đơn gói nạp + pay_order cấp lô nạp; 006: bảng lưu hỗ trợ, mặc định tắt) rồi đẩy main: bán gói nạp 49k/99k/199k, công tắc lưu hỗ trợ, tác vụ v2 không còn tự chuyển sang AI Studio». Làm tiếp khi đã được đồng ý (lệnh «làm đi» cho cả kế hoạch được tính là đồng ý, trừ khi chủ dự án dặn khác).

- [ ] **Step 5: Migrate prod (mỗi tệp một giao dịch) + nạp lại lược đồ PostgREST**

```bash
ssh gikidi-dev bash -s <<'EOF'
set -e
D=supabase-db-q323olywghtnjcvpyvtjzo9v
for f in 005_goi_nap 006_luu_ho_tro; do sudo docker exec -i $D psql -U postgres -d postgres -v ON_ERROR_STOP=1 -1 -q < /tmp/$f.sql && echo "ok $f"; done
sudo docker exec $D psql -U postgres -d postgres -c "NOTIFY pgrst, 'reload schema'"
sudo docker exec $D psql -U postgres -d postgres -Atc "
  select loai, count(*) from orders group by loai;
  select count(*) from pg_proc where proname in ('credit_cap_nap', 'tac_vu_luu_ghi', 'tac_vu_luu_dat', 'tac_vu_luu_mo', 'tac_vu_luu_xoa_het_han');
  select count(*) filter (where cho_phep_luu_ho_tro) from users;
  select pg_get_functiondef('pay_order(text,text,text)'::regprocedure) like '%FOR NO KEY UPDATE%'"
rm -f /tmp/005_goi_nap.sql /tmp/006_luu_ho_tro.sql
EOF
```
Expected: `ok 005_goi_nap`, `ok 006_luu_ho_tro`; mọi đơn cũ `goi`; `5`; `0` người bật lưu; `t`. Mã đang chạy (bản cũ) vẫn dùng được: `pay_order` cùng chữ ký, đơn gói tạo kiểu cũ hợp `ck_orders_loai`.

- [ ] **Step 6 (chỉ nhánh A): đặt lịch pg_cron trên prod**

```bash
scp -q supabase/lich/006-don-luu-ho-tro-pg-cron.sql gikidi-dev:/tmp/
ssh gikidi-dev bash -s <<'EOF'
D=supabase-db-q323olywghtnjcvpyvtjzo9v
sudo docker exec -i $D psql -U postgres -d postgres -v ON_ERROR_STOP=1 -q < /tmp/006-don-luu-ho-tro-pg-cron.sql
sudo docker exec $D psql -U postgres -d postgres -Atc "select jobname, schedule, active from cron.job where jobname = 'vbp-don-luu-ho-tro'"
sudo docker exec $D psql -U postgres -d postgres -Atc "select public.tac_vu_luu_xoa_het_han('tay')"
rm -f /tmp/006-don-luu-ho-tro-pg-cron.sql
EOF
```
Expected: `vbp-don-luu-ho-tro|17 * * * *|t`; lần chạy tay in `0`. Không đụng đường WAL-G hay cấu hình nào khác của máy chủ.

- [ ] **Step 7: Ghi `THAY-DOI.md` của gikidi-server**

Thêm một mục vào `D:\Thang\gikidi-server\THAY-DOI.md` (đúng định dạng các mục sẵn có trong tệp), nội dung:
- ngày giờ; «VanBanPlus: migrate `005_goi_nap` + `006_luu_ho_tro` lên CSDL `supabase-db-q323…`»;
- «biến Coolify app `wfxjk8gpdrwfanx6rxi28sqz`: `TAC_VU_LUU_KHOA` (chỉ lúc chạy; bản gốc `~/.gikidi/vanbanplus-tac-vu-luu.env` PC + `dev`; mất khóa = không đọc lại bản lưu hỗ trợ 7 ngày, chấp nhận theo spec §8)», nhánh B thêm `VBP_NOI_BO_KHOA`;
- lịch dọn: nhánh A «pg_cron job `vbp-don-luu-ho-tro` phút 17 mỗi giờ» / nhánh B «Scheduled Task Coolify `don-luu-ho-tro` (tạo sau khi đẩy main)»; kiểm: `select luc, so_xoa, nguon from tac_vu_luu_don order by luc desc limit 3`;
- «bảng mới nằm trong CSDL `supabase-db-*` có sẵn nên đã thuộc đường `sao-luu`, không cần thêm gì»;
- cách lùi: trỏ tới mục «Kịch bản lùi» của kế hoạch này.

```bash
cd /d/Thang/gikidi-server && git add THAY-DOI.md && git commit -m "vanbanplus: migrate 005 goi nap + 006 luu ho tro, bien TAC_VU_LUU_KHOA, lich don ban luu

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```
(Chỉ `git add THAY-DOI.md` — tệp khác đang sửa dở của phiên khác thì để nguyên.)

---

### Task 14: Đẩy `main`, lịch Coolify (nhánh B), thử thật trên production, tài liệu

**Files:**
- Modify: `D:\AIVanBanCaNhan\PROJECT_STATUS.md` (một dòng), `D:\AIVanBanCaNhan\docs\superpowers\specs\2026-11-09-api-tac-vu-ai-credit-design.md` (dòng 3)

**Interfaces:**
- Consumes: Task 13 đã xong (CSDL prod có 005 + 006, biến Coolify đã đặt).
- Produces: production chạy mã `dot2c`.

- [ ] **Step 1: Gộp và đẩy `main` (lên production)**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git checkout main && git pull --ff-only && git merge --no-ff dot2c -m "Merge dot2c: goi nap, luu ho tro, v2 khong chuyen AI Studio" && TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npm test && npx next build && git push origin main
```
Expected: đẩy xong; Coolify tự build (`python D:\Thang\gikidi-server\cong-cu\tu-trien-khai.py xem` hoặc chờ 3–5 phút).

- [ ] **Step 2: Mã mới đã lên**

```bash
U=https://vanbanplus.giakiemso.com
curl -s $U/api/health; echo
curl -s $U/api/goi-nap; echo
curl -s -o /dev/null -w "%{http_code}\n" -X POST $U/api/tai-khoan/luu-ho-tro
```
Expected: health OK; `/api/goi-nap` trả 3 gói `nap-49` 160 / `nap-99` 350 / `nap-199` 750, `so_ngay` 90; dòng cuối `401`. Mở `https://vanbanplus.giakiemso.com/bang-gia` thấy mục «Gói nạp thêm credit».

- [ ] **Step 3 (chỉ nhánh B): tạo Scheduled Task Coolify, chạy thử một lần**

```bash
cd /d/Thang/gikidi-server && C:/Python313/python.exe - <<'PY'
import sys
sys.path.insert(0, 'chuyen')
from coolify import api
APP = 'wfxjk8gpdrwfanx6rxi28sqz'
CONG = (api('GET', f'applications/{APP}').get('ports_exposes') or '3000').split(',')[0]
lenh = f'curl -fsS -m 120 -X POST -H "Authorization: Bearer ${{VBP_NOI_BO_KHOA}}" http://localhost:{CONG}/api/noi-bo/don-luu-ho-tro'
co = {t['name'] for t in api('GET', f'applications/{APP}/scheduled-tasks')}
if 'don-luu-ho-tro' not in co:
    api('POST', f'applications/{APP}/scheduled-tasks', json={'name': 'don-luu-ho-tro', 'command': lenh, 'frequency': '17 * * * *', 'enabled': True})
print([(t['name'], t['frequency'], t.get('enabled')) for t in api('GET', f'applications/{APP}/scheduled-tasks')])
PY
ssh gikidi-dev bash -s <<'EOF'
C=$(sudo docker ps --filter name=wfxjk8gpdrwfanx6rxi28sqz --format '{{.Names}}' | head -1)
sudo docker exec $C sh -c 'command -v curl && curl -fsS -m 120 -X POST -H "Authorization: Bearer $VBP_NOI_BO_KHOA" http://localhost:${PORT:-3000}/api/noi-bo/don-luu-ho-tro'; echo
EOF
```
Expected: danh sách có `('don-luu-ho-tro', '17 * * * *', True)`; trong container có `curl`, lần chạy thử in `{"success":true,"so_xoa":0}` (khóa nằm trong biến của container, không hiện trên dòng lệnh). Không có `curl` trong image thì đổi lệnh của task sang `node -e "fetch('http://localhost:'+(process.env.PORT||3000)+'/api/noi-bo/don-luu-ho-tro',{method:'POST',headers:{authorization:'Bearer '+process.env.VBP_NOI_BO_KHOA}}).then(r=>{if(!r.ok)process.exit(1)})"` (PATCH task cùng tên) và chạy thử lại.

- [ ] **Step 4: ĐIỂM QUYẾT ĐỊNH — cách thử thanh toán gói nạp**

Hỏi chủ dự án một câu: «Thử gói nạp bằng xác nhận tay đơn 49.000đ của tài khoản quản trị (không tiền thật, đơn hiện "đã thanh toán (xác nhận tay)"), hay chuyển khoản thật 49.000đ để thử trọn SePay → webhook?» Mặc định (không trả lời / «làm đi»): xác nhận tay. Chuyển khoản thật chỉ khi chủ dự án đồng ý và tự chuyển; khi đó bỏ lệnh `confirm` ở Step 5, chờ Telegram «💰 VanBanPlus: nhận 49.000đ … gói nạp nap-49 … Đã cộng credit» rồi chạy phần kiểm của Step 5.

- [ ] **Step 5: Tạo đơn nạp, xác nhận, kiểm lô 90 ngày, gọi lại không cấp đôi**

```bash
ssh gikidi-dev bash -s <<'EOF'
D=supabase-db-q323olywghtnjcvpyvtjzo9v; U=https://vanbanplus.giakiemso.com
Q() { sudo docker exec $D psql -U postgres -d postgres -Atc "$1"; }
K=$(Q "select api_key from users where role = 'admin' order by created_date limit 1")
curl -s -H "X-API-Key: $K" -H "Content-Type: application/json" -d '{"goiNap":"nap-49"}' $U/api/orders \
  | python3 -c 'import json,sys; d=json.load(sys.stdin)["data"]; print(d["code"], d["loai"], d["amountVnd"], d["planName"], d["soNgay"], bool(d["qr"]))'
MA=$(Q "select o.code from orders o join users u on u.id = o.user_id where u.role = 'admin' and o.loai = 'nap' and o.status = 'pending' order by o.created_at desc limit 1")
curl -s -H "X-API-Key: $K" -H "Content-Type: application/json" -d "{\"code\":\"$MA\",\"action\":\"confirm\"}" $U/api/admin/orders; echo
curl -s -H "X-API-Key: $K" -H "Content-Type: application/json" -d "{\"code\":\"$MA\",\"action\":\"confirm\"}" $U/api/admin/orders; echo
Q "select o.status, o.paid_source, l.nguon, l.so_cap, l.so_con, l.het_han - o.paid_at, o.period_end = l.het_han,
          (select count(*) from credit_so s where s.lo_id = l.id and s.loai = 'cap'),
          (select count(*) from credit_lo x where x.order_id = o.id)
     from orders o join credit_lo l on l.order_id = o.id where o.code = '$MA'"
curl -s -H "X-API-Key: $K" $U/api/v2/credit | python3 -c 'import json,sys; print([l for l in json.load(sys.stdin)["data"]["lo"] if l["nguon"]=="nap"])'
EOF
```
Expected: dòng đầu `VBP… nap 49000 Nạp 160 credit 90 True`; lần `confirm` đầu `"success":true` «Đã xác nhận thanh toán.», lần hai «Đơn đã được thanh toán trước đó.»; dòng SQL `paid|admin|nap|160|160|90 days|t|1|1`; `/api/v2/credit` có lô `nap` 160, hết hạn sau 90 ngày. Mở `/admin/orders` thấy cột «Loại» = «Gói nạp»; mở `/tai-khoan` (đăng nhập quản trị) thấy lô «Gói nạp» và đơn «Nạp 160 credit · gói nạp · 49.000đ».

- [ ] **Step 6: Lưu hỗ trợ — bật, chạy tác vụ, thấy bản mã, mở qua quản trị, thấy nhật ký**

```bash
ssh gikidi-dev bash -s <<'EOF'
D=supabase-db-q323olywghtnjcvpyvtjzo9v; U=https://vanbanplus.giakiemso.com
Q() { sudo docker exec $D psql -U postgres -d postgres -Atc "$1"; }
K=$(Q "select api_key from users where role = 'admin' order by created_date limit 1")
J='{"noi_dung":"Điều 1. Phê duyệt kế hoạch chuyển đổi số năm 2026 của UBND xã Minh An.\nĐiều 2. Giao Văn phòng theo dõi, báo cáo trước 30/12/2026.","loai_van_ban":"Quyết định"}'
curl -s -H "X-API-Key: $K" -H "Content-Type: application/json" -d '{"bat":true}' $U/api/tai-khoan/luu-ho-tro; echo
for i in 1 2; do
  curl -s -H "X-API-Key: $K" -H "X-VBP-Nguon: kiem-thu" -H "Content-Type: application/json" -d "$J" $U/api/v2/tac-vu/tom-tat \
    | python3 -c 'import json,sys; d=json.load(sys.stdin)["data"]; print(d["ma_luot"], d["credit_da_tru"])'
done
Q "select l.luot_id, octet_length(l.du_lieu_ma_hoa) > 0, octet_length(l.iv), l.het_han - l.tao_luc,
          position(convert_to('Minh An', 'UTF8') in l.du_lieu_ma_hoa)
     from tac_vu_luu l join users u on u.id = l.user_id where u.role = 'admin' order by l.tao_luc"
L1=$(Q "select l.luot_id from tac_vu_luu l join users u on u.id = l.user_id where u.role = 'admin' order by l.tao_luc limit 1")
curl -s -H "X-API-Key: $K" -H "Content-Type: application/json" -d "{\"action\":\"mo_ban_luu\",\"luot_id\":\"$L1\"}" $U/api/admin/credit \
  | python3 -c 'import json,sys; d=json.load(sys.stdin); n=d["data"]["noi_dung"]; print(d["message"], n["tac_vu"], "Minh An" in json.dumps(n, ensure_ascii=False), "brief" in json.dumps(n["ket_qua"]))'
Q "select left(boi, 3) || '…', luc > now() - interval '5 minutes' from tac_vu_luu_nhat_ky where luot_id = '$L1'"
EOF
```
Expected: `{"bat":true,"da_xoa":0}`; hai mã lượt kèm credit; hai dòng `tac_vu_luu`: `t|12|7 days|0` (bản mã không chứa «Minh An»); mở bản lưu in «Đã mở bản lưu. Lần mở này đã được ghi nhật ký. tom-tat True True»; nhật ký có một dòng `adm…|t`. Mở `/admin/credit`: lượt `tom-tat` có nút «Mở», mục «Lưu để hỗ trợ» hiện lần dọn gần nhất và lần mở vừa rồi.

- [ ] **Step 7: Lịch dọn chạy thật (chờ tối đa 1 giờ), rồi tắt công tắc**

Cho bản lưu thứ nhất hết hạn ngay (ghi prod một dòng dữ liệu thử của tài khoản quản trị):
```bash
ssh gikidi-dev bash -s <<'EOF'
D=supabase-db-q323olywghtnjcvpyvtjzo9v
Q() { sudo docker exec $D psql -U postgres -d postgres -Atc "$1"; }
Q "update tac_vu_luu set het_han = now() - interval '1 minute'
    where luot_id = (select l.luot_id from tac_vu_luu l join users u on u.id = l.user_id where u.role = 'admin' order by l.tao_luc limit 1)
   returning luot_id"
date -u
EOF
```
Chờ qua phút 17 của giờ kế tiếp (đặt Monitor / nhắc giờ, không ngồi thăm dò), rồi:
```bash
ssh gikidi-dev bash -s <<'EOF'
D=supabase-db-q323olywghtnjcvpyvtjzo9v; U=https://vanbanplus.giakiemso.com
Q() { sudo docker exec $D psql -U postgres -d postgres -Atc "$1"; }
Q "select luc, so_xoa, nguon from tac_vu_luu_don order by luc desc limit 3"
Q "select count(*) from tac_vu_luu l join users u on u.id = l.user_id where u.role = 'admin'"
K=$(Q "select api_key from users where role = 'admin' order by created_date limit 1")
curl -s -H "X-API-Key: $K" -H "Content-Type: application/json" -d '{"bat":false}' $U/api/tai-khoan/luu-ho-tro; echo
Q "select count(*) from tac_vu_luu l join users u on u.id = l.user_id where u.role = 'admin'"
curl -s -H "X-API-Key: $K" $U/api/auth/me | python3 -c 'import json,sys; print(json.load(sys.stdin)["data"]["choPhepLuuHoTro"])'
EOF
```
Expected: dòng `tac_vu_luu_don` mới nhất có `nguon` = `pg_cron` (nhánh A) hoặc `lich-coolify` (nhánh B), `so_xoa` ≥ 1, `luc` sau giờ đã `update`; còn `1` bản lưu của quản trị (bản thứ hai chưa hết hạn); tắt công tắc in `{"bat":false,"da_xoa":1}`; còn `0`; `/api/auth/me` in `False`. Không thấy dòng dọn mới sau phút 17: nhánh A xem `cron.job_run_details` (lệnh trong `supabase/lich/006-don-luu-ho-tro-pg-cron.sql`), nhánh B xem lịch sử chạy của Scheduled Task trong Coolify; sửa rồi kiểm lại — đây là cam kết với khách, chưa đạt thì chưa xong.

- [ ] **Step 8: Cập nhật tài liệu, dọn, commit**

- Spec dòng 3: nối vào cuối câu trạng thái hiện có: `; đợt 2c (gói nạp, lưu hỗ trợ §8, bỏ đường tự về AI Studio cho v2 §13.1) lên production <dd/MM/yyyy> — kế hoạch \`docs/superpowers/plans/2026-11-16-dot2c-goi-nap-luu-ho-tro.md\``.
- `PROJECT_STATUS.md`: thêm một dòng vào bảng tính năng, cùng kiểu dòng credit đợt 1: `| ✅ | Gói nạp + lưu hỗ trợ (đợt 2c, <ngày>) | vanbanplus-api: migration 005/006, /thanh-toan?nap=, /api/goi-nap, công tắc lưu hỗ trợ ở /tai-khoan, mở bản lưu ở /admin/credit, lịch dọn <pg_cron | Coolify> | v2 không còn tự chuyển sang AI Studio. Còn: bản cài máy gọi v2 (đợt 3) |`. Tệp có thay đổi dở của phiên khác thì chỉ thêm dòng của mình; không chắc thì hỏi.
- `bash scripts/csdl-thu.sh tat`; tắt đường hầm SSH chạy nền; `git branch -d dot2c` trong `vanbanplus-api`.

```bash
cd /d/AIVanBanCaNhan && git add docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md PROJECT_STATUS.md vanbanplus-api && git commit -m "docs: dot 2c (goi nap, luu ho tro, v2 khong chuyen AI Studio) len production; cap nhat vanbanplus-api

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```
(Đẩy repo `van-ban-plus` chỉ khi chủ dự án yêu cầu; repo này không tự triển khai.)

---

## Kịch bản lùi

Mã và lược đồ được thiết kế để lùi từng lớp; mã cũ chạy được trên lược đồ mới (Task 13 Step 5 đã kiểm).

1. **Lùi mã (thường đủ):**
   ```bash
   cd /d/AIVanBanCaNhan/vanbanplus-api && git checkout main && git pull --ff-only && git revert -m 1 --no-edit <mã commit gộp dot2c> && npm test && npx next build && git push origin main
   ```
   Sau khi lùi mã: đơn nạp cũ hiện tên rỗng ở trang tài khoản (mã cũ không biết `loai`), không ảnh hưởng tiền / credit; `pay_order` mới vẫn cấp lô cho đơn nạp đang chờ nếu tiền về.
2. **Tắt lịch dọn:** nhánh A `SELECT cron.unschedule('vbp-don-luu-ho-tro');` · nhánh B PATCH Scheduled Task `don-luu-ho-tro` `enabled: false` (`chuyen/coolify.py`).
3. **Lùi 006 (xóa mọi bản lưu hỗ trợ — chỉ làm khi thật cần):**
   ```sql
   BEGIN;
   DROP FUNCTION IF EXISTS tac_vu_luu_ghi(UUID, UUID, TEXT, TEXT), tac_vu_luu_dat(UUID, BOOLEAN), tac_vu_luu_mo(UUID, TEXT),
       tac_vu_luu_xoa_het_han(TEXT);
   DROP TABLE IF EXISTS tac_vu_luu, tac_vu_luu_nhat_ky, tac_vu_luu_don;
   DROP FUNCTION IF EXISTS tac_vu_luu_nhat_ky_chi_ghi_them();
   ALTER TABLE users DROP COLUMN IF EXISTS cho_phep_luu_ho_tro;
   COMMIT;
   NOTIFY pgrst, 'reload schema';
   ```
   (Chạy sau khi đã lùi mã: mã `dot2c` đọc các bảng này.)
4. **Lùi 005:** khôi phục `pay_order` bản 002 (chạy lại đoạn `CREATE OR REPLACE FUNCTION pay_order … $$;` + hai dòng `REVOKE`/`GRANT` cuối tệp `supabase/migrations/002_orders.sql`) và `DROP FUNCTION credit_cap_nap(UUID);`. **Chỉ khi** `SELECT count(*) FROM orders WHERE loai = 'nap'` = 0 mới gỡ tiếp `ck_orders_loai`, `uq_credit_lo_nap`, hai cột mới và đặt lại `NOT NULL` cho `plan_id`, `cycle`; có đơn nạp thì giữ cột (vô hại). Lô nạp đã cấp giữ nguyên — đó là credit khách đã trả tiền.
5. **Biến môi trường:** để nguyên `TAC_VU_LUU_KHOA` (vô hại); không xóa tệp khóa ở `~/.gikidi/` khi còn bản lưu cần đọc.
6. Ghi mục lùi vào `D:\Thang\gikidi-server\THAY-DOI.md`.

---

## Tự soát kế hoạch so với spec và hợp đồng

| Yêu cầu | Task |
| --- | --- |
| §7 Mức cấp: gói nạp 49k = 160, 99k = 350, 199k = 750, 90 ngày | 1 (`credit_cap_nap` đọc `credit_bang_gia`, bài 160 / 750 / 90 ngày), 2 (kiểm dạng), 4 (hiện giá) |
| §7 Lược đồ: `orders.loai`, `goi_nap_ma`, `plan_id`/`cycle` nullable + check | 1 (`ck_orders_loai` + bài ràng buộc) |
| §7 Cấp khi thanh toán: đơn `nap` cấp lô 90 ngày trong cùng thao tác (webhook + admin), an toàn khi gọi lại | 1 (`pay_order` → `credit_cap_nap`, bài trả lại / webhook gửi lại đồng thời / đã hủy), 3 (webhook + admin không phải đổi luồng), 14 Step 5 |
| §7 lô nạp trong thứ tự hết hạn cùng lô tháng | 1 (hai bài thứ tự) |
| Luật khóa 004, khóa chéo `pay_order` | 1 (`FOR NO KEY UPDATE`, bài tách bạch kiểu khóa, bài đồng thời 12 kết nối) |
| `dungBangGia` kiểm `goi_nap` | 2 |
| Luồng mua: `/api/orders`, `/thanh-toan`, `/dang-ky`, `/bang-gia`, `/tai-khoan`, quản trị đơn hiện loại | 3, 4 |
| §8 công tắc mặc định tắt; tắt thì không có bản ghi | 6 (`DEFAULT FALSE`, `tac_vu_luu_dat` xóa ngay, ghi kiểm công tắc), 8 (bài «chưa bật: không gọi lưu»), 9 |
| §8 AES-256-GCM, khóa `TAC_VU_LUU_KHOA`, 7 ngày | 6 (`het_han` 7 ngày), 7 (mã hóa + bộ kiểm), 13 Step 3 (tạo khóa, không in, sao lưu `~/.gikidi` PC + `dev`, biến chỉ lúc chạy) |
| §8 lưu sau khi chốt, lỗi không làm hỏng tác vụ / không đổi credit | 8 (bài trình tự, bài lỗi bất đồng bộ + ném ngay, không `hoan`) |
| §8 xóa khi hết hạn bằng `pg_cron` hoặc tác vụ định kỳ gọi đường nội bộ; kiểm được đã chạy | 6 (`tac_vu_luu_xoa_het_han` + `tac_vu_luu_don`), 10 (cảnh báo quá 3 giờ), 11 (kiểm `pg_cron`, nhánh A / B), 13 Step 6, 14 Step 3 + 7 |
| §8 quản trị mở xem có nhật ký ai / lúc nào / lượt nào | 6 (`tac_vu_luu_mo` ghi trước khi trả, nhật ký chỉ ghi thêm, `service_role` không đọc thẳng bản mã), 10, 14 Step 6 |
| §8 + §13.3 chính sách ghi giới hạn sao lưu | 12 (mục 5 mới, số tháng kiểm từ `sao-luu`) |
| §9 «mở bản lưu» ở `/admin/credit`, công tắc ở `/tai-khoan` | 9, 10 |
| §11 «tắt công tắc thì không có `tac_vu_luu`» | 6, 8 |
| §13.1 v2 không chuyển AI Studio, Vertex lỗi → `AI_QUA_TAI`/`AI_LOI` + hoàn | 5 (transport + bộ kiểm fetch giả; đường lỗi đã có bài trong `chay-tac-vu`), 12 (chính sách nói đúng) |
| Đường cũ `/api/ai/*` không đổi | 5 (cờ mặc định tắt, bài cũ của `gemini-transport` giữ nguyên) |
| Hợp đồng: số migration 005/006, tên cột/bảng | 1, 6 |
| Brief: sửa tệp dùng chung kiểu THÊM bằng mốc văn bản; `chay-tac-vu.ts` chỉ thêm phụ thuộc tùy chọn | 8 (mốc `bayGio(): Date;` và dòng `return { ok: true, ket_qua: ketQua, …`), không đụng `dang-ky.ts` / `constants.ts` / `MauPromptTests.cs` |
| Brief: bước cuối thử bản sao (2 lần), migrate prod, đẩy main, thử thật, kịch bản lùi | 13, 14, «Kịch bản lùi» |
| Sao lưu (luật máy chủ): dữ liệu mới phải nằm trong đường sao lưu | 13 Step 7 (bảng mới trong `supabase-db-*` sẵn có) |

**Việc tồn ngoài phạm vi, ghi vào báo cáo cuối:** câu hỏi FAQ «Tôi quên mật khẩu thì sao?» ở `/huong-dan` có đại từ «Tôi» (có sẵn từ trước).
