# Lớp credit + khung tác vụ v2 (đợt 1) — kế hoạch triển khai

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Đưa credit vào `vanbanplus-api` (lược đồ, hàm Postgres, quy đổi token), chuyển đường cũ `/api/ai/*` sang trừ credit, dựng khung `/api/v2/tac-vu/*` và chạy được tác vụ đầu tiên `tom-tat` trên production.

**Architecture:** Mọi thay đổi số dư nằm trong hàm Postgres (giao dịch + khóa dòng `users` trước, rồi tới lô). Lớp TypeScript chỉ điều phối: `chay-tac-vu.ts` nhận phụ thuộc qua tham số (bảng giá, kho credit, khóa AI, hàm gọi AI) nên kiểm thử được bằng đồ giả. Mỗi tác vụ là một thư mục `dinh-nghia.ts` + `prompt.ts`; prompt là hàm thuần, so khớp từng ký tự với bản C# bằng tệp mẫu do bộ kiểm C# sinh ra.

**Tech Stack:** Next.js 16 (App Router), TypeScript, Supabase JS (service role), Postgres 17 (ảnh `supabase/postgres:17.6.1.155`), `tsx --test`, `pg` (chỉ trong bộ kiểm), xUnit (.NET 9) cho tệp mẫu prompt.

**Spec:** [docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md](../specs/2026-11-09-api-tac-vu-ai-credit-design.md)

## Phạm vi đợt này

Làm: spec mục 6 (hợp đồng API), mục 7 (credit: lược đồ, hàm, quy đổi, đường cũ), mục 9 (giao diện tối thiểu, trừ phần «mở bản lưu»), mục 10–11 cho riêng `tom-tat`, mục 12 bước 1–2 và tác vụ đầu của bước 3.

Để đợt sau (kế hoạch riêng):

- **Đợt 2:** 5 tác vụ còn lại (`kiem-tra`, `tham-muu`, `bao-cao-dinh-ky`, `trich-xuat`, `soan-thao`), lưu tùy chọn để hỗ trợ (mục 8: `tac_vu_luu`, `cho_phep_luu_ho_tro`, mã hóa, `pg_cron`), gói nạp (đơn `loai = 'nap'`, `pay_order` cấp lô 90 ngày, giao diện mua).
- **Đợt 3:** bản cài máy gọi v2 (thay lời gọi C#, xóa prompt C#, hộp thoại hiện khoảng credit, bỏ chế độ gọi thẳng Gemini) — phát hành một lần khi đủ 6 tác vụ (spec mục 12 bước 4).

Bản cài máy **không đổi hành vi** trong đợt này: chỉ đổi hai hàm prompt của `DocumentSummaryService` sang `internal static` để sinh tệp mẫu.

## Global Constraints

- Repo mã: `D:\AIVanBanCaNhan\vanbanplus-api` (git riêng, remote `ericphan28/vanbanplus-api`). Repo tài liệu + C#: `D:\AIVanBanCaNhan` (`ericphan28/van-ban-plus`).
- **Đẩy `main` của `vanbanplus-api` = lên production** (Coolify tự build). Chỉ đẩy khi `npm test` và `npx next build` đều xanh. Làm trên nhánh `credit-dot-1`, chỉ gộp vào `main` ở Task 13.
- **Migrate prod TRƯỚC khi đẩy mã cần lược đồ mới.** Thử trên bản sao prod trước, chạy trong một giao dịch (`psql -1`), xong thì `NOTIFY pgrst, 'reload schema'`.
- CSDL prod: container `supabase-db-q323olywghtnjcvpyvtjzo9v` trên máy chủ (`ssh gikidi-dev`, `sudo docker exec …`). Không in khóa, chuỗi kết nối, API key ra chat hay log.
- Bảng mới: bật RLS, không policy, `REVOKE ALL … FROM anon, authenticated`, `GRANT ALL … TO service_role` (giống `orders`). Hàm mới: `REVOKE EXECUTE … FROM PUBLIC, anon, authenticated; GRANT EXECUTE … TO service_role`.
- Đơn vị gọi là «credit» (không «điểm», không «tín dụng»).
- Lời hiện cho khách (thông báo lỗi, trang web): tiếng Việt có dấu, **không đại từ nhân xưng** (tôi, bạn, anh, chị, em, mình, chúng tôi, ta), không «dạ/vâng/ạ».
- Không lưu nội dung văn bản trên máy chủ: `tac_vu_chay` không có cột nội dung, không `console.log` nội dung văn bản.
- Tác vụ v2 chỉ dùng khóa hệ thống (`resolveAIKey(..., { chiHeThong: true })`).
- Mô hình mặc định v2: `gemini-3.6-flash` (đọc từ `credit_bang_gia`, dòng `cau_hinh/chung`).
- Hằng số SQL: tối đa **3** lượt `dang_chay` mỗi người; lượt treo quá **15 phút** thì hoàn.
- Quy đổi: `credit = ceil((token_vào × giá_vào + token_ra × giá_ra) / giá_trị_credit)`, giá theo USD/1 triệu token × tỷ giá; token ra gồm token suy luận.
- Mức cấp/tháng: free 30 · starter 400 · pro 1.100 · business 3.000.

## Sơ đồ tệp

```
vanbanplus-api/
  supabase/migrations/004_credit.sql          lược đồ + bảng giá mẫu + toàn bộ hàm credit (Task 2–4)
  scripts/csdl-thu.sh                         dựng/xóa Postgres thử trên máy chủ dev (Task 1)
  tests/ho-tro/csdl-thu.ts                    tạo CSDL thử mới cho mỗi tệp kiểm (Task 1)
  tests/csdl-thu.test.ts                      kiểm khung CSDL thử (Task 1)
  tests/credit-csdl.test.ts                   kiểm hàm Postgres (Task 2–4)
  tests/uoc-tinh.test.ts                      (Task 5)
  tests/bang-gia.test.ts                      (Task 6)
  tests/doc-json.test.ts, tests/chay-tac-vu.test.ts   (Task 7)
  tests/tac-vu-tom-tat.test.ts + tests/mau-prompt/tom-tat.json   (Task 8)
  tests/api-cu.test.ts                        (Task 10)
  src/lib/credit/uoc-tinh.ts                  quy đổi token ↔ credit, hàm thuần
  src/lib/credit/bang-gia.ts                  đọc + kiểm bảng giá, nhớ 60 giây
  src/lib/credit/so-du.ts                     gọi hàm Postgres (KhoCredit)
  src/lib/credit/api-cu.ts                    credit cho /api/ai/*
  src/lib/tac-vu/chay-tac-vu.ts               luồng chung
  src/lib/tac-vu/doc-json.ts                  đọc JSON AI trả về
  src/lib/tac-vu/dang-ky.ts                   danh sách tác vụ
  src/lib/tac-vu/phu-thuoc.ts                 phụ thuộc thật
  src/lib/tac-vu/xac-thuc.ts, tra-loi.ts      xác thực, trả lỗi
  src/lib/tac-vu/tom-tat/{dinh-nghia,prompt}.ts
  src/app/api/v2/tac-vu/[ten]/route.ts
  src/app/api/v2/tac-vu/[ten]/uoc-tinh/route.ts
  src/app/api/v2/credit/route.ts
  src/app/api/admin/credit/route.ts, src/app/admin/credit/page.tsx
  sửa: src/lib/ai-keys.ts, src/app/api/ai/[action]/route.ts, src/app/api/auth/[action]/route.ts,
       src/proxy.ts, src/utils/constants.ts, src/app/tai-khoan/page.tsx,
       src/app/bang-gia/BangGiaClient.tsx, src/app/dang-ky/DangKyClient.tsx, package.json
AIVanBan.Core/Services/DocumentSummaryService.cs   2 hàm prompt → internal static (Task 8)
AIVanBan.Tests/MauPromptTests.cs                    sinh tests/mau-prompt/tom-tat.json (Task 8)
```

---

### Task 1: Khung CSDL thử

**Files:**
- Create: `vanbanplus-api/scripts/csdl-thu.sh`
- Create: `vanbanplus-api/tests/ho-tro/csdl-thu.ts`
- Create: `vanbanplus-api/tests/csdl-thu.test.ts`
- Modify: `vanbanplus-api/package.json` (thêm `@types/pg` vào devDependencies)

**Interfaces:**
- Produces: `taoCsdlThu(): Promise<{ url: string; xoa(): Promise<void> }>`, `CO_CSDL_THU: boolean`, `rpc(c: Client, ham: string, ...thamSo: unknown[]): Promise<any>`, `taoNguoiDung(c: Client, tuyChon?: { plan?: string; batDau?: Date; ketThuc?: Date | null; taoLuc?: Date }): Promise<string>`.

- [ ] **Step 1: Tạo nhánh làm việc**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git status --short && git checkout -b credit-dot-1
```
Expected: `git status` trống (nếu có thay đổi của phiên khác thì dừng, hỏi chủ dự án).

- [ ] **Step 2: Viết script dựng Postgres thử**

`scripts/csdl-thu.sh`:

```bash
#!/usr/bin/env bash
# Postgres thử dùng cùng ảnh với prod (supabase/postgres 17.6), chạy trên máy chủ dev.
# bat: dựng container (xóa cái cũ nếu có) · tat: xóa container.
# Sau «bat», mở đường hầm ở một cửa sổ khác:  ssh -N -L 55990:127.0.0.1:55990 gikidi-dev
# rồi đặt: TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres
set -euo pipefail
CONG=55990
case "${1:-bat}" in
  bat)
    ssh gikidi-dev "sudo docker rm -f vbp-csdl-thu >/dev/null 2>&1 || true
      sudo docker run -d --name vbp-csdl-thu -e POSTGRES_PASSWORD=thu -p 127.0.0.1:$CONG:5432 supabase/postgres:17.6.1.155 >/dev/null
      for i in \$(seq 1 60); do
        if PGPASSWORD=thu psql -h 127.0.0.1 -p $CONG -U postgres -Atc 'select 1' >/dev/null 2>&1; then
          sleep 5
          PGPASSWORD=thu psql -h 127.0.0.1 -p $CONG -U postgres -Atc 'select 1' >/dev/null && exit 0
        fi
        sleep 1
      done
      exit 1"
    echo "Đã dựng. Mở đường hầm: ssh -N -L $CONG:127.0.0.1:$CONG gikidi-dev"
    ;;
  tat)
    ssh gikidi-dev "sudo docker rm -f vbp-csdl-thu" >/dev/null && echo "Đã xóa container thử."
    ;;
  *) echo "Dùng: bash scripts/csdl-thu.sh bat|tat"; exit 2 ;;
esac
```

- [ ] **Step 3: Viết bộ trợ giúp**

`tests/ho-tro/csdl-thu.ts`:

```ts
import { randomBytes } from "node:crypto";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { Client } from "pg";

/** Chuỗi kết nối tới Postgres thử (scripts/csdl-thu.sh). Không đặt thì bỏ qua các bộ kiểm CSDL. */
export const URL_THU = process.env.TEST_DATABASE_URL;
export const CO_CSDL_THU = Boolean(URL_THU);

const TEP_LUOC_DO = [
  "schema.sql",
  "migrations/002_orders.sql",
  "migrations/003_payment_settings.sql",
  "migrations/004_credit.sql",
];

/** Tạo CSDL mới, nạp lược đồ + các migration cần cho credit. 001 (cloud) không cần nên bỏ. */
export async function taoCsdlThu(): Promise<{ url: string; xoa(): Promise<void> }> {
  if (!URL_THU) throw new Error("Thiếu TEST_DATABASE_URL");
  const ten = `vbp_thu_${randomBytes(5).toString("hex")}`;

  const goc = new Client({ connectionString: URL_THU });
  await goc.connect();
  await goc.query(`CREATE DATABASE ${ten}`);
  await goc.end();

  const u = new URL(URL_THU);
  u.pathname = `/${ten}`;
  const url = u.toString();

  const c = new Client({ connectionString: url });
  await c.connect();
  try {
    for (const tep of TEP_LUOC_DO) {
      await c.query(readFileSync(join(__dirname, "..", "..", "supabase", tep), "utf8"));
    }
  } finally {
    await c.end();
  }

  return {
    url,
    async xoa() {
      const g = new Client({ connectionString: URL_THU });
      await g.connect();
      await g.query(`DROP DATABASE IF EXISTS ${ten} WITH (FORCE)`);
      await g.end();
    },
  };
}

/** Gọi một hàm SQL, trả cột đầu của dòng đầu. */
export async function rpc(c: Client, ham: string, ...thamSo: unknown[]): Promise<any> {
  const dau = thamSo.map((_, i) => `$${i + 1}`).join(", ");
  const { rows } = await c.query(`SELECT ${ham}(${dau}) AS r`, thamSo);
  return rows[0].r;
}

export async function taoNguoiDung(
  c: Client,
  tuyChon: { plan?: string; batDau?: Date; ketThuc?: Date | null; taoLuc?: Date } = {}
): Promise<string> {
  const bay = new Date();
  const { rows } = await c.query(
    `INSERT INTO users (email, full_name, password_hash, api_key, subscription_plan_id,
                        subscription_start_date, subscription_end_date, created_date)
     VALUES ($1, 'Người thử', 'x', $2, $3, $4, $5, $6) RETURNING id`,
    [
      `thu-${randomBytes(4).toString("hex")}@vanbanplus.test`,
      `vbp_${randomBytes(16).toString("hex")}`,
      tuyChon.plan ?? "free",
      (tuyChon.batDau ?? bay).toISOString(),
      tuyChon.ketThuc ? tuyChon.ketThuc.toISOString() : null,
      (tuyChon.taoLuc ?? bay).toISOString(),
    ]
  );
  return rows[0].id as string;
}
```

- [ ] **Step 4: Viết bộ kiểm khung**

`tests/csdl-thu.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { Client } from "pg";
import { CO_CSDL_THU, taoCsdlThu } from "./ho-tro/csdl-thu";

test("CSDL thử nạp đủ lược đồ và bốn gói", { skip: !CO_CSDL_THU }, async () => {
  const csdl = await taoCsdlThu();
  const c = new Client({ connectionString: csdl.url });
  await c.connect();
  try {
    const { rows } = await c.query("SELECT id FROM subscription_plans ORDER BY sort_order");
    assert.deepEqual(rows.map((r) => r.id), ["free", "starter", "pro", "business"]);
  } finally {
    await c.end();
    await csdl.xoa();
  }
});
```

Tạm thời tạo `supabase/migrations/004_credit.sql` rỗng chỉ có dòng chú thích để bộ trợ giúp đọc được:

```sql
-- 004: Credit + lượt tác vụ AI. Spec: docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md mục 7.
```

- [ ] **Step 5: Cài kiểu `pg`, dựng CSDL thử, chạy**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && npm i -D @types/pg && bash scripts/csdl-thu.sh bat
```
Mở đường hầm bằng một lệnh chạy nền (Bash `run_in_background: true`): `ssh -N -L 55990:127.0.0.1:55990 gikidi-dev`.

```bash
TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npx tsx --test tests/csdl-thu.test.ts
```
Expected: `# pass 1`. Rồi `npm test` (không đặt biến) → bộ kiểm CSDL báo `skipped`, các bộ cũ vẫn pass.

- [ ] **Step 6: Commit**

```bash
git add scripts/csdl-thu.sh tests/ho-tro/csdl-thu.ts tests/csdl-thu.test.ts supabase/migrations/004_credit.sql package.json package-lock.json
git commit -m "test: khung CSDL thu (supabase/postgres tren may chu dev) cho ham credit"
```

---

### Task 2: Lược đồ credit, bảng giá mẫu, cấp lô theo kỳ, số dư

**Files:**
- Modify: `vanbanplus-api/supabase/migrations/004_credit.sql`
- Create: `vanbanplus-api/tests/credit-csdl.test.ts`

**Interfaces:**
- Consumes: `taoCsdlThu`, `rpc`, `taoNguoiDung` (Task 1).
- Produces (SQL): bảng `credit_lo`, `credit_so`, `tac_vu_chay`, `credit_bang_gia`; cột `subscription_plans.credit_thang`; hàm `credit_tong(p_user uuid) → int`, `credit_cap_ky(p_user uuid) → void`, `credit_so_du(p_user uuid) → jsonb {tong, lo:[{nguon,so_cap,so_con,het_han}], het_han_gan_nhat}`. (`credit_so_du` gọi `credit_quet_treo`, viết ở Task 3; ở task này tạo `credit_quet_treo` bản tạm trả 0.)

- [ ] **Step 1: Viết bộ kiểm hỏng**

`tests/credit-csdl.test.ts`:

```ts
import { after, before, describe, test } from "node:test";
import assert from "node:assert/strict";
import { Client } from "pg";
import { CO_CSDL_THU, rpc, taoCsdlThu, taoNguoiDung } from "./ho-tro/csdl-thu";

const NGAY = 24 * 3600 * 1000;

describe("hàm credit trong Postgres", { skip: !CO_CSDL_THU }, () => {
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

  test("người mới gói free được 30 credit, gọi lại không cấp đôi", async () => {
    const u = await taoNguoiDung(c);
    assert.equal((await rpc(c, "credit_so_du", u)).tong, 30);
    assert.equal((await rpc(c, "credit_so_du", u)).tong, 30);
    const { rows } = await c.query("SELECT count(*)::int AS n FROM credit_lo WHERE user_id = $1", [u]);
    assert.equal(rows[0].n, 1);
    await kiemSoCai(u);
  });

  test("gói trả phí còn hạn cấp theo credit_thang của gói", async () => {
    const u = await taoNguoiDung(c, { plan: "pro", ketThuc: new Date(Date.now() + 30 * NGAY) });
    const sd = await rpc(c, "credit_so_du", u);
    assert.equal(sd.tong, 1100);
    assert.equal(sd.lo.length, 1);
    assert.equal(sd.lo[0].nguon, "goi_thang");
  });

  test("gói trả phí hết hạn thì rơi về free", async () => {
    const u = await taoNguoiDung(c, {
      plan: "pro",
      batDau: new Date(Date.now() - 60 * NGAY),
      ketThuc: new Date(Date.now() - 30 * NGAY),
    });
    assert.equal((await rpc(c, "credit_so_du", u)).tong, 30);
    const { rows } = await c.query("SELECT plan_id FROM credit_lo WHERE user_id = $1", [u]);
    assert.equal(rows[0].plan_id, "free");
  });

  test("kỳ tính từ ngày bắt đầu gói; lô hết hạn cuối kỳ", async () => {
    const batDau = new Date(Date.now() - 40 * NGAY);
    const u = await taoNguoiDung(c, { plan: "starter", batDau, ketThuc: new Date(Date.now() + 320 * NGAY) });
    await rpc(c, "credit_so_du", u);
    const { rows } = await c.query(
      `SELECT ky_bat_dau = $2::timestamptz + interval '1 month' AS dung_ky,
              het_han = ky_bat_dau + interval '1 month' AS dung_han, so_cap
         FROM credit_lo WHERE user_id = $1`,
      [u, batDau.toISOString()]
    );
    assert.deepEqual(rows[0], { dung_ky: true, dung_han: true, so_cap: 400 });
  });

  test("lô của gói tháng không vượt quá ngày hết hạn gói", async () => {
    const ketThuc = new Date(Date.now() + 5 * NGAY);
    const u = await taoNguoiDung(c, { plan: "starter", batDau: new Date(Date.now() - 10 * NGAY), ketThuc });
    await rpc(c, "credit_so_du", u);
    const { rows } = await c.query("SELECT het_han = $2::timestamptz AS dung FROM credit_lo WHERE user_id = $1", [u, ketThuc.toISOString()]);
    assert.equal(rows[0].dung, true);
  });

  test("sổ cái chỉ ghi thêm; xóa người dùng thì xóa theo dữ liệu credit", async () => {
    const u = await taoNguoiDung(c);
    await rpc(c, "credit_so_du", u);
    await assert.rejects(c.query("UPDATE credit_so SET so = 999 WHERE user_id = $1", [u]), /chỉ ghi thêm/);
    await assert.rejects(c.query("DELETE FROM credit_so WHERE user_id = $1", [u]), /chỉ ghi thêm/);
    await c.query("DELETE FROM users WHERE id = $1", [u]);
    const { rows } = await c.query("SELECT count(*)::int AS n FROM credit_lo WHERE user_id = $1", [u]);
    assert.equal(rows[0].n, 0);
  });
});
```

- [ ] **Step 2: Chạy, thấy hỏng**

```bash
TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npx tsx --test tests/credit-csdl.test.ts
```
Expected: FAIL, `function credit_so_du(unknown) does not exist`.

- [ ] **Step 3: Viết lược đồ + hàm**

Thay toàn bộ `supabase/migrations/004_credit.sql` bằng:

```sql
-- ============================================================
-- 004: Credit + lượt tác vụ AI
-- Spec: docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md mục 7.
-- Bảng mới: RLS bật, không policy, chỉ service_role dùng.
-- Mọi hàm đổi số dư khóa dòng users trước, rồi mới tới tac_vu_chay và credit_lo (thứ tự het_han, id)
-- để các lời gọi đồng thời của cùng một người xếp hàng, không tiêu lố, không khóa chéo.
-- ============================================================

ALTER TABLE subscription_plans ADD COLUMN IF NOT EXISTS credit_thang INT NOT NULL DEFAULT 0 CHECK (credit_thang >= 0);
UPDATE subscription_plans
   SET credit_thang = CASE id WHEN 'free' THEN 30 WHEN 'starter' THEN 400 WHEN 'pro' THEN 1100 WHEN 'business' THEN 3000 ELSE credit_thang END;

CREATE TABLE IF NOT EXISTS credit_lo (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    nguon TEXT NOT NULL CHECK (nguon IN ('goi_thang', 'nap', 'tang', 'dieu_chinh')),
    so_cap INT NOT NULL CHECK (so_cap >= 0),
    so_con INT NOT NULL CHECK (so_con >= 0),
    het_han TIMESTAMPTZ NOT NULL,
    plan_id TEXT REFERENCES subscription_plans(id),
    ky_bat_dau TIMESTAMPTZ,
    order_id UUID REFERENCES orders(id),
    tao_luc TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE UNIQUE INDEX IF NOT EXISTS uq_credit_lo_ky ON credit_lo(user_id, plan_id, ky_bat_dau) WHERE nguon = 'goi_thang';
CREATE INDEX IF NOT EXISTS idx_credit_lo_user ON credit_lo(user_id, het_han);

CREATE TABLE IF NOT EXISTS credit_so (
    id BIGSERIAL PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    lo_id UUID NOT NULL REFERENCES credit_lo(id) ON DELETE CASCADE,
    luot_id UUID,
    loai TEXT NOT NULL CHECK (loai IN ('cap', 'giu', 'tru', 'hoan', 'dieu_chinh')),
    so INT NOT NULL,
    ghi_chu TEXT,
    boi TEXT,
    tao_luc TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS idx_credit_so_luot ON credit_so(luot_id) WHERE luot_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_credit_so_user ON credit_so(user_id, tao_luc DESC);

-- Sổ cái chỉ ghi thêm. Xóa theo dây chuyền khi xóa người dùng thì cho qua (gọi từ trigger khóa ngoại, độ sâu > 1).
CREATE OR REPLACE FUNCTION credit_so_chi_ghi_them() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
    IF TG_OP = 'DELETE' AND pg_trigger_depth() > 1 THEN RETURN OLD; END IF;
    RAISE EXCEPTION 'credit_so chỉ ghi thêm, không sửa, không xóa';
END;
$$;
DROP TRIGGER IF EXISTS trg_credit_so_chi_ghi_them ON credit_so;
CREATE TRIGGER trg_credit_so_chi_ghi_them BEFORE UPDATE OR DELETE ON credit_so
    FOR EACH ROW EXECUTE FUNCTION credit_so_chi_ghi_them();

-- Một dòng mỗi lượt chạy. KHÔNG có cột nội dung văn bản.
CREATE TABLE IF NOT EXISTS tac_vu_chay (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    tac_vu TEXT NOT NULL,
    trang_thai TEXT NOT NULL DEFAULT 'dang_chay' CHECK (trang_thai IN ('dang_chay', 'xong', 'loi')),
    credit_min INT NOT NULL DEFAULT 0,
    credit_max INT NOT NULL DEFAULT 0,
    credit_giu INT NOT NULL DEFAULT 0,
    credit_tru INT NOT NULL DEFAULT 0,
    provider TEXT,
    model TEXT,
    token_vao INT,
    token_ra INT,
    bi_cat BOOLEAN NOT NULL DEFAULT FALSE,
    ma_loi TEXT,
    nguon_goi TEXT,
    phien_ban_ung_dung TEXT,
    bat_dau_luc TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    xong_luc TIMESTAMPTZ
);
CREATE INDEX IF NOT EXISTS idx_tac_vu_chay_user ON tac_vu_chay(user_id, trang_thai);
CREATE INDEX IF NOT EXISTS idx_tac_vu_chay_luc ON tac_vu_chay(bat_dau_luc DESC);

CREATE TABLE IF NOT EXISTS credit_bang_gia (
    loai TEXT NOT NULL CHECK (loai IN ('tac_vu', 'mo_hinh', 'goi_nap', 'cau_hinh')),
    ma TEXT NOT NULL,
    gia_tri JSONB NOT NULL,
    sua_luc TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    sua_boi TEXT,
    PRIMARY KEY (loai, ma)
);

-- Giá mô hình tính theo giá chính thức từ 01/01/2027 (spec mục 7), USD / 1 triệu token, ra gồm suy luận.
-- Hệ số tác vụ đo ngày 09/11/2026 trên gemini-3.5-flash; đo lại trên 3.6 trước khi mở bán (spec mục 11).
INSERT INTO credit_bang_gia (loai, ma, gia_tri) VALUES
    ('cau_hinh', 'chung',  '{"gia_tri_credit_vnd": 100, "ty_gia_vnd_usd": 26000, "mo_hinh_mac_dinh": "gemini-3.6-flash"}'),
    ('mo_hinh', 'gemini-3.6-flash',      '{"vao_usd_trieu": 1.5, "ra_usd_trieu": 7.5}'),
    ('mo_hinh', 'gemini-3.5-flash',      '{"vao_usd_trieu": 1.5, "ra_usd_trieu": 9}'),
    ('mo_hinh', 'gemini-3.5-flash-lite', '{"vao_usd_trieu": 0.3, "ra_usd_trieu": 2.5}'),
    ('tac_vu', 'tom-tat', '{"toi_thieu": 2, "ra_min_he_so": 0.2, "ra_min_hang": 1500, "ra_max_he_so": 0.6, "ra_max_hang": 7000, "toi_da_ky_tu": 120000}'),
    ('tac_vu', 'api-cu',  '{"toi_thieu": 1, "ra_min_he_so": 0, "ra_min_hang": 0, "ra_max_he_so": 0, "ra_max_hang": 0, "toi_da_ky_tu": 0}'),
    ('goi_nap', 'nap-49',  '{"gia_vnd": 49000,  "credit": 160, "so_ngay": 90}'),
    ('goi_nap', 'nap-99',  '{"gia_vnd": 99000,  "credit": 350, "so_ngay": 90}'),
    ('goi_nap', 'nap-199', '{"gia_vnd": 199000, "credit": 750, "so_ngay": 90}')
ON CONFLICT (loai, ma) DO NOTHING;

ALTER TABLE credit_lo ENABLE ROW LEVEL SECURITY;
ALTER TABLE credit_so ENABLE ROW LEVEL SECURITY;
ALTER TABLE tac_vu_chay ENABLE ROW LEVEL SECURITY;
ALTER TABLE credit_bang_gia ENABLE ROW LEVEL SECURITY;
REVOKE ALL ON credit_lo, credit_so, tac_vu_chay, credit_bang_gia FROM anon, authenticated;
GRANT ALL ON credit_lo, credit_so, tac_vu_chay, credit_bang_gia TO service_role;
GRANT USAGE, SELECT ON SEQUENCE credit_so_id_seq TO service_role;

-- Tổng credit còn dùng được (lô còn hạn).
CREATE OR REPLACE FUNCTION credit_tong(p_user UUID) RETURNS INT
LANGUAGE sql STABLE SET search_path = public AS $$
    SELECT COALESCE(SUM(so_con), 0)::INT FROM credit_lo
     WHERE user_id = p_user AND het_han > NOW() AND so_con > 0;
$$;

-- Cấp lô tháng của kỳ hiện tại nếu chưa có. Kỳ tính từ ngày bắt đầu gói; free tính từ ngày tạo tài khoản.
-- Gói năm vẫn cấp từng tháng. Gói hết hạn thì tính như free.
CREATE OR REPLACE FUNCTION credit_cap_ky(p_user UUID) RETURNS VOID
LANGUAGE plpgsql SET search_path = public AS $$
DECLARE
    u users%ROWTYPE;
    v_plan TEXT;
    v_moc TIMESTAMPTZ;
    v_thang INT;
    v_ky TIMESTAMPTZ;
    v_het TIMESTAMPTZ;
    v_so INT;
    v_lo UUID;
BEGIN
    SELECT * INTO u FROM users WHERE id = p_user;
    IF NOT FOUND THEN RETURN; END IF;

    IF COALESCE(u.subscription_plan_id, 'free') = 'free'
       OR (u.subscription_end_date IS NOT NULL AND u.subscription_end_date <= NOW()) THEN
        v_plan := 'free';
        v_moc := COALESCE(u.created_date, NOW());
    ELSE
        v_plan := u.subscription_plan_id;
        v_moc := COALESCE(u.subscription_start_date, u.created_date, NOW());
    END IF;

    SELECT credit_thang INTO v_so FROM subscription_plans WHERE id = v_plan;
    IF COALESCE(v_so, 0) <= 0 THEN RETURN; END IF;

    v_thang := (EXTRACT(YEAR FROM age(NOW(), v_moc)) * 12 + EXTRACT(MONTH FROM age(NOW(), v_moc)))::INT;
    v_ky := v_moc + make_interval(months => v_thang);
    IF v_ky > NOW() THEN v_ky := v_moc + make_interval(months => v_thang - 1); END IF;
    v_het := v_ky + INTERVAL '1 month';
    IF v_plan <> 'free' AND u.subscription_end_date IS NOT NULL THEN
        v_het := LEAST(v_het, u.subscription_end_date);
    END IF;

    INSERT INTO credit_lo (user_id, nguon, so_cap, so_con, het_han, plan_id, ky_bat_dau)
    VALUES (p_user, 'goi_thang', v_so, v_so, v_het, v_plan, v_ky)
    ON CONFLICT (user_id, plan_id, ky_bat_dau) WHERE nguon = 'goi_thang' DO NOTHING
    RETURNING id INTO v_lo;

    IF v_lo IS NOT NULL THEN
        INSERT INTO credit_so (user_id, lo_id, loai, so, ghi_chu)
        VALUES (p_user, v_lo, 'cap', v_so,
                'Gói ' || v_plan || ' kỳ ' || to_char(v_ky AT TIME ZONE 'Asia/Ho_Chi_Minh', 'DD/MM/YYYY'));
    END IF;
END;
$$;

-- Bản tạm, Task 3 thay bằng bản thật.
CREATE OR REPLACE FUNCTION credit_quet_treo(p_user UUID) RETURNS INT
LANGUAGE plpgsql SET search_path = public AS $$ BEGIN RETURN 0; END; $$;

-- Số dư để hiển thị và để trả trong API. Tự quét lượt treo và cấp kỳ mới.
CREATE OR REPLACE FUNCTION credit_so_du(p_user UUID) RETURNS JSONB
LANGUAGE plpgsql SET search_path = public AS $$
BEGIN
    PERFORM credit_quet_treo(p_user);
    PERFORM credit_cap_ky(p_user);
    RETURN jsonb_build_object(
        'tong', credit_tong(p_user),
        'lo', COALESCE((
            SELECT jsonb_agg(jsonb_build_object('nguon', nguon, 'so_cap', so_cap, 'so_con', so_con, 'het_han', het_han)
                             ORDER BY het_han, id)
              FROM credit_lo WHERE user_id = p_user AND het_han > NOW() AND so_con > 0), '[]'::jsonb),
        'het_han_gan_nhat', (SELECT MIN(het_han) FROM credit_lo
                              WHERE user_id = p_user AND het_han > NOW() AND so_con > 0)
    );
END;
$$;

REVOKE EXECUTE ON FUNCTION credit_tong(UUID), credit_cap_ky(UUID), credit_quet_treo(UUID), credit_so_du(UUID)
    FROM PUBLIC, anon, authenticated;
GRANT EXECUTE ON FUNCTION credit_tong(UUID), credit_cap_ky(UUID), credit_quet_treo(UUID), credit_so_du(UUID)
    TO service_role;
```

- [ ] **Step 4: Chạy, thấy qua**

Cùng lệnh Step 2. Expected: `# pass 6`. Nếu bài «xóa người dùng» hỏng ở trigger: xóa trực tiếp chạy trigger ở độ sâu 1, xóa dây chuyền từ khóa ngoại chạy ở độ sâu ≥ 2. Thêm `RAISE NOTICE 'depth %', pg_trigger_depth();` tạm vào trigger để xem số thật, chỉnh ngưỡng cho khớp, rồi bỏ dòng NOTICE. Không bỏ bài kiểm.

- [ ] **Step 5: Commit**

```bash
git add supabase/migrations/004_credit.sql tests/credit-csdl.test.ts
git commit -m "feat(credit): bang credit_lo/credit_so/tac_vu_chay/credit_bang_gia, cap lo theo ky, so du"
```

---

### Task 3: Giữ / chốt / hoàn credit cho một lượt, quét lượt treo

**Files:**
- Modify: `vanbanplus-api/supabase/migrations/004_credit.sql` (thêm cuối tệp; thay bản tạm `credit_quet_treo`)
- Modify: `vanbanplus-api/tests/credit-csdl.test.ts` (thêm bài trong `describe`)

**Interfaces:**
- Produces (SQL):
  - `tac_vu_mo(p_user uuid, p_tac_vu text, p_credit_min int, p_credit_max int, p_provider text, p_model text, p_nguon_goi text, p_phien_ban text) → jsonb {ma: 'ok'|'KHONG_DU_CREDIT'|'QUA_NHIEU_LUOT', luot_id?, so_du}`
  - `credit_giu(p_user uuid, p_luot uuid, p_so int) → boolean` (nội bộ, `tac_vu_mo` gọi)
  - `credit_chot(p_luot uuid, p_credit_that int, p_token_vao int, p_token_ra int, p_bi_cat boolean) → jsonb {credit_tru, so_du}`
  - `credit_hoan(p_luot uuid, p_ma_loi text) → jsonb {so_du}`
  - `credit_quet_treo(p_user uuid) → int`
- Quy ước sổ cái: giữ ghi `giu` (−x) từng lô; khi kết thúc lượt luôn ghi `hoan` (+x) trả lại phần đã giữ, và nếu có trừ thật thì ghi thêm `tru` (−y). Nhờ vậy `SUM(so)` của mỗi lô luôn bằng `so_con`.

- [ ] **Step 1: Viết bộ kiểm hỏng**

Thêm vào trong `describe(...)` của `tests/credit-csdl.test.ts` (sau bài cuối):

```ts
  const mo = (u: string, min: number, max: number, cl: Client = c) =>
    rpc(cl, "tac_vu_mo", u, "thu", min, max, "gemini", "gemini-3.6-flash", "kiem-thu", "0.0.0");

  test("giữ rồi chốt: trừ đúng số thật, trả phần dư", async () => {
    const u = await taoNguoiDung(c);
    const m = await mo(u, 5, 12);
    assert.equal(m.ma, "ok");
    assert.equal(m.so_du, 18);
    const ch = await rpc(c, "credit_chot", m.luot_id, 7, 4000, 3000, false);
    assert.deepEqual(ch, { credit_tru: 7, so_du: 23 });
    const { rows } = await c.query("SELECT trang_thai, credit_tru, token_vao, token_ra FROM tac_vu_chay WHERE id = $1", [m.luot_id]);
    assert.deepEqual(rows[0], { trang_thai: "xong", credit_tru: 7, token_vao: 4000, token_ra: 3000 });
    await kiemSoCai(u);
  });

  test("chốt không vượt phần đã giữ, gọi lại không trừ đôi", async () => {
    const u = await taoNguoiDung(c);
    const m = await mo(u, 2, 10);
    assert.deepEqual(await rpc(c, "credit_chot", m.luot_id, 50, 1, 1, true), { credit_tru: 10, so_du: 20 });
    assert.deepEqual(await rpc(c, "credit_chot", m.luot_id, 5, 1, 1, false), { credit_tru: 10, so_du: 20 });
    await kiemSoCai(u);
  });

  test("hoàn khi lỗi trả đủ phần giữ", async () => {
    const u = await taoNguoiDung(c);
    const m = await mo(u, 2, 12);
    assert.deepEqual(await rpc(c, "credit_hoan", m.luot_id, "AI_LOI"), { so_du: 30 });
    const { rows } = await c.query("SELECT trang_thai, ma_loi, credit_tru FROM tac_vu_chay WHERE id = $1", [m.luot_id]);
    assert.deepEqual(rows[0], { trang_thai: "loi", ma_loi: "AI_LOI", credit_tru: 0 });
    await kiemSoCai(u);
  });

  test("không đủ credit thì không giữ, không tạo lượt", async () => {
    const u = await taoNguoiDung(c);
    const m = await mo(u, 5, 31);
    assert.deepEqual(m, { ma: "KHONG_DU_CREDIT", so_du: 30 });
    const { rows } = await c.query("SELECT count(*)::int AS n FROM tac_vu_chay WHERE user_id = $1", [u]);
    assert.equal(rows[0].n, 0);
  });

  test("tối đa 3 lượt cùng lúc", async () => {
    const u = await taoNguoiDung(c, { plan: "pro", ketThuc: new Date(Date.now() + 30 * NGAY) });
    for (let i = 0; i < 3; i++) assert.equal((await mo(u, 1, 5)).ma, "ok");
    assert.equal((await mo(u, 1, 5)).ma, "QUA_NHIEU_LUOT");
  });

  test("lượt treo quá 15 phút được hoàn khi xem số dư", async () => {
    const u = await taoNguoiDung(c);
    const m = await mo(u, 2, 12);
    await c.query("UPDATE tac_vu_chay SET bat_dau_luc = NOW() - interval '16 minutes' WHERE id = $1", [m.luot_id]);
    assert.equal((await rpc(c, "credit_so_du", u)).tong, 30);
    const { rows } = await c.query("SELECT trang_thai, ma_loi FROM tac_vu_chay WHERE id = $1", [m.luot_id]);
    assert.deepEqual(rows[0], { trang_thai: "loi", ma_loi: "TREO" });
    await kiemSoCai(u);
  });

  test("5 yêu cầu đồng thời không tiêu lố", async () => {
    const u = await taoNguoiDung(c);
    await rpc(c, "credit_so_du", u);
    const ketNoi = await Promise.all(
      Array.from({ length: 5 }, async () => {
        const k = new Client({ connectionString: csdl.url });
        await k.connect();
        return k;
      })
    );
    try {
      const kq = await Promise.all(ketNoi.map((k) => mo(u, 5, 12, k)));
      assert.equal(kq.filter((k) => k.ma === "ok").length, 2);
      assert.equal((await rpc(c, "credit_so_du", u)).tong, 6);
    } finally {
      await Promise.all(ketNoi.map((k) => k.end()));
    }
    await kiemSoCai(u);
  });
```

- [ ] **Step 2: Chạy, thấy hỏng**

Expected: FAIL, `function tac_vu_mo(...) does not exist`.

- [ ] **Step 3: Viết hàm**

Thêm vào cuối `004_credit.sql` (khối `CREATE OR REPLACE FUNCTION credit_quet_treo` ở đây thay bản tạm ở trên; xóa bản tạm và dòng chú thích «Bản tạm» đi, giữ nguyên dòng REVOKE/GRANT chung):

```sql
-- Giữ p_so credit cho lượt p_luot, lấy từ lô hết hạn sớm nhất. Người gọi phải đã khóa dòng users.
CREATE OR REPLACE FUNCTION credit_giu(p_user UUID, p_luot UUID, p_so INT) RETURNS BOOLEAN
LANGUAGE plpgsql SET search_path = public AS $$
DECLARE
    r RECORD;
    v_con INT := p_so;
    v_lay INT;
BEGIN
    IF p_so <= 0 THEN RETURN TRUE; END IF;
    IF credit_tong(p_user) < p_so THEN RETURN FALSE; END IF;
    FOR r IN SELECT id, so_con FROM credit_lo
              WHERE user_id = p_user AND het_han > NOW() AND so_con > 0
              ORDER BY het_han, id FOR UPDATE LOOP
        EXIT WHEN v_con = 0;
        v_lay := LEAST(r.so_con, v_con);
        UPDATE credit_lo SET so_con = so_con - v_lay WHERE id = r.id;
        INSERT INTO credit_so (user_id, lo_id, luot_id, loai, so) VALUES (p_user, r.id, p_luot, 'giu', -v_lay);
        v_con := v_con - v_lay;
    END LOOP;
    IF v_con > 0 THEN RAISE EXCEPTION 'credit_giu: thiếu % credit', v_con; END IF;
    RETURN TRUE;
END;
$$;

-- Trả toàn bộ phần đã giữ của lượt về từng lô, rồi trừ thật p_tru theo cùng thứ tự lô. Trả về số đã trừ.
CREATE OR REPLACE FUNCTION credit_tra_giu(p_user UUID, p_luot UUID, p_tru INT) RETURNS INT
LANGUAGE plpgsql SET search_path = public AS $$
DECLARE
    r RECORD;
    v_con INT := GREATEST(p_tru, 0);
    v_lay INT;
    v_da_tru INT := 0;
BEGIN
    PERFORM 1 FROM credit_lo
     WHERE id IN (SELECT lo_id FROM credit_so WHERE luot_id = p_luot AND loai = 'giu')
     ORDER BY het_han, id FOR UPDATE;
    FOR r IN SELECT s.lo_id, (-SUM(s.so))::INT AS da_giu
               FROM credit_so s JOIN credit_lo l ON l.id = s.lo_id
              WHERE s.luot_id = p_luot AND s.loai = 'giu'
              GROUP BY s.lo_id, l.het_han
              ORDER BY l.het_han, s.lo_id LOOP
        v_lay := LEAST(v_con, r.da_giu);
        INSERT INTO credit_so (user_id, lo_id, luot_id, loai, so) VALUES (p_user, r.lo_id, p_luot, 'hoan', r.da_giu);
        IF v_lay > 0 THEN
            INSERT INTO credit_so (user_id, lo_id, luot_id, loai, so) VALUES (p_user, r.lo_id, p_luot, 'tru', -v_lay);
        END IF;
        UPDATE credit_lo SET so_con = so_con + r.da_giu - v_lay WHERE id = r.lo_id;
        v_con := v_con - v_lay;
        v_da_tru := v_da_tru + v_lay;
    END LOOP;
    RETURN v_da_tru;
END;
$$;

-- Mở một lượt: quét treo, cấp kỳ, kiểm số lượt đang chạy, kiểm số dư, ghi lượt, giữ credit_max.
CREATE OR REPLACE FUNCTION tac_vu_mo(
    p_user UUID, p_tac_vu TEXT, p_credit_min INT, p_credit_max INT,
    p_provider TEXT, p_model TEXT, p_nguon_goi TEXT, p_phien_ban TEXT
) RETURNS JSONB
LANGUAGE plpgsql SET search_path = public AS $$
DECLARE
    v_luot UUID;
    v_dang INT;
BEGIN
    PERFORM 1 FROM users WHERE id = p_user FOR UPDATE;
    IF NOT FOUND THEN RAISE EXCEPTION 'tac_vu_mo: không có người dùng %', p_user; END IF;
    PERFORM credit_quet_treo(p_user);
    PERFORM credit_cap_ky(p_user);

    SELECT COUNT(*) INTO v_dang FROM tac_vu_chay WHERE user_id = p_user AND trang_thai = 'dang_chay';
    IF v_dang >= 3 THEN
        RETURN jsonb_build_object('ma', 'QUA_NHIEU_LUOT', 'so_du', credit_tong(p_user));
    END IF;
    IF credit_tong(p_user) < p_credit_max THEN
        RETURN jsonb_build_object('ma', 'KHONG_DU_CREDIT', 'so_du', credit_tong(p_user));
    END IF;

    INSERT INTO tac_vu_chay (user_id, tac_vu, credit_min, credit_max, credit_giu, provider, model, nguon_goi, phien_ban_ung_dung)
    VALUES (p_user, p_tac_vu, p_credit_min, p_credit_max, p_credit_max, p_provider, p_model, p_nguon_goi, p_phien_ban)
    RETURNING id INTO v_luot;
    PERFORM credit_giu(p_user, v_luot, p_credit_max);

    RETURN jsonb_build_object('ma', 'ok', 'luot_id', v_luot, 'so_du', credit_tong(p_user));
END;
$$;

-- Chốt lượt thành công: trừ thật min(p_credit_that, phần đã giữ). Gọi lại trên lượt đã xong thì không đổi gì.
CREATE OR REPLACE FUNCTION credit_chot(p_luot UUID, p_credit_that INT, p_token_vao INT, p_token_ra INT, p_bi_cat BOOLEAN)
RETURNS JSONB
LANGUAGE plpgsql SET search_path = public AS $$
DECLARE
    v_user UUID;
    t tac_vu_chay%ROWTYPE;
    v_tru INT;
BEGIN
    SELECT user_id INTO v_user FROM tac_vu_chay WHERE id = p_luot;
    IF NOT FOUND THEN RAISE EXCEPTION 'credit_chot: không có lượt %', p_luot; END IF;
    PERFORM 1 FROM users WHERE id = v_user FOR UPDATE;
    SELECT * INTO t FROM tac_vu_chay WHERE id = p_luot FOR UPDATE;
    IF t.trang_thai <> 'dang_chay' THEN
        RETURN jsonb_build_object('credit_tru', t.credit_tru, 'so_du', credit_tong(v_user));
    END IF;

    v_tru := credit_tra_giu(v_user, p_luot, LEAST(GREATEST(p_credit_that, 0), t.credit_giu));
    UPDATE tac_vu_chay
       SET trang_thai = 'xong', credit_tru = v_tru, token_vao = p_token_vao, token_ra = p_token_ra,
           bi_cat = COALESCE(p_bi_cat, FALSE), xong_luc = NOW()
     WHERE id = p_luot;
    RETURN jsonb_build_object('credit_tru', v_tru, 'so_du', credit_tong(v_user));
END;
$$;

-- Hoàn toàn bộ phần đã giữ, đánh lượt là lỗi. Gọi lại trên lượt đã kết thúc thì không đổi gì.
CREATE OR REPLACE FUNCTION credit_hoan(p_luot UUID, p_ma_loi TEXT) RETURNS JSONB
LANGUAGE plpgsql SET search_path = public AS $$
DECLARE
    v_user UUID;
    t tac_vu_chay%ROWTYPE;
BEGIN
    SELECT user_id INTO v_user FROM tac_vu_chay WHERE id = p_luot;
    IF NOT FOUND THEN RAISE EXCEPTION 'credit_hoan: không có lượt %', p_luot; END IF;
    PERFORM 1 FROM users WHERE id = v_user FOR UPDATE;
    SELECT * INTO t FROM tac_vu_chay WHERE id = p_luot FOR UPDATE;
    IF t.trang_thai = 'dang_chay' THEN
        PERFORM credit_tra_giu(v_user, p_luot, 0);
        UPDATE tac_vu_chay SET trang_thai = 'loi', ma_loi = p_ma_loi, xong_luc = NOW() WHERE id = p_luot;
    END IF;
    RETURN jsonb_build_object('so_du', credit_tong(v_user));
END;
$$;

-- Lượt «dang_chay» quá 15 phút: hoàn và đánh lỗi TREO.
CREATE OR REPLACE FUNCTION credit_quet_treo(p_user UUID) RETURNS INT
LANGUAGE plpgsql SET search_path = public AS $$
DECLARE
    r RECORD;
    n INT := 0;
BEGIN
    FOR r IN SELECT id FROM tac_vu_chay
              WHERE user_id = p_user AND trang_thai = 'dang_chay' AND bat_dau_luc < NOW() - INTERVAL '15 minutes' LOOP
        PERFORM credit_hoan(r.id, 'TREO');
        n := n + 1;
    END LOOP;
    RETURN n;
END;
$$;

REVOKE EXECUTE ON FUNCTION credit_giu(UUID, UUID, INT), credit_tra_giu(UUID, UUID, INT),
    tac_vu_mo(UUID, TEXT, INT, INT, TEXT, TEXT, TEXT, TEXT),
    credit_chot(UUID, INT, INT, INT, BOOLEAN), credit_hoan(UUID, TEXT), credit_quet_treo(UUID)
    FROM PUBLIC, anon, authenticated;
GRANT EXECUTE ON FUNCTION credit_giu(UUID, UUID, INT), credit_tra_giu(UUID, UUID, INT),
    tac_vu_mo(UUID, TEXT, INT, INT, TEXT, TEXT, TEXT, TEXT),
    credit_chot(UUID, INT, INT, INT, BOOLEAN), credit_hoan(UUID, TEXT), credit_quet_treo(UUID)
    TO service_role;
```

Lưu ý thứ tự trong tệp: `credit_so_du` (Task 2) gọi `credit_quet_treo` lúc chạy, không phải lúc tạo, nên đặt bản thật ở cuối tệp là được. `credit_quet_treo` gọi `credit_hoan` — cả hai đều đã có khi hàm chạy.

- [ ] **Step 4: Chạy, thấy qua**

Expected: `# pass 13`.

- [ ] **Step 5: Commit**

```bash
git add supabase/migrations/004_credit.sql tests/credit-csdl.test.ts
git commit -m "feat(credit): tac_vu_mo/credit_chot/credit_hoan giu-tru-hoan theo lo, quet luot treo"
```

---

### Task 4: Trừ credit cho đường cũ, điều chỉnh tay

**Files:**
- Modify: `vanbanplus-api/supabase/migrations/004_credit.sql` (thêm cuối tệp)
- Modify: `vanbanplus-api/tests/credit-csdl.test.ts`

**Interfaces:**
- Produces (SQL):
  - `credit_tru_cu(p_user uuid, p_tac_vu text, p_so int, p_provider text, p_model text, p_token_vao int, p_token_ra int, p_phien_ban text) → jsonb {credit_tru, so_du}`: trừ tối đa bằng số dư, ghi một dòng `tac_vu_chay` (`nguon_goi = 'api-cu'`, `trang_thai = 'xong'`).
  - `credit_dieu_chinh(p_user uuid, p_so int, p_ghi_chu text, p_boi text) → jsonb {da_dieu_chinh, so_du}`: dương thì tạo lô `dieu_chinh` hạn 90 ngày; âm thì trừ từ lô hết hạn sớm nhất, không làm âm.

- [ ] **Step 1: Viết bộ kiểm hỏng**

Thêm vào `describe(...)`:

```ts
  test("đường cũ trừ tối đa bằng số dư và ghi lượt", async () => {
    const u = await taoNguoiDung(c);
    assert.deepEqual(
      await rpc(c, "credit_tru_cu", u, "/api/ai/generate", 50, "gemini", "gemini-3.5-flash", 9000, 7000, "1.0.18"),
      { credit_tru: 30, so_du: 0 }
    );
    const { rows } = await c.query("SELECT tac_vu, trang_thai, nguon_goi, credit_tru FROM tac_vu_chay WHERE user_id = $1", [u]);
    assert.deepEqual(rows[0], { tac_vu: "/api/ai/generate", trang_thai: "xong", nguon_goi: "api-cu", credit_tru: 30 });
    await kiemSoCai(u);
  });

  test("điều chỉnh dương tạo lô 90 ngày; lô hết hạn sớm bị dùng trước", async () => {
    const u = await taoNguoiDung(c);
    assert.deepEqual(await rpc(c, "credit_dieu_chinh", u, 50, "tặng thử", "admin@test"), { da_dieu_chinh: 50, so_du: 80 });
    const m = await mo(u, 2, 40);
    await rpc(c, "credit_chot", m.luot_id, 35, 1, 1, false);
    const { rows } = await c.query("SELECT nguon, so_con FROM credit_lo WHERE user_id = $1 ORDER BY het_han", [u]);
    assert.deepEqual(rows, [{ nguon: "goi_thang", so_con: 0 }, { nguon: "dieu_chinh", so_con: 45 }]);
    await kiemSoCai(u);
  });

  test("điều chỉnh âm không làm âm số dư", async () => {
    const u = await taoNguoiDung(c);
    assert.deepEqual(await rpc(c, "credit_dieu_chinh", u, -100, "thu hồi", "admin@test"), { da_dieu_chinh: -30, so_du: 0 });
    await kiemSoCai(u);
  });
```

- [ ] **Step 2: Chạy, thấy hỏng**

Expected: FAIL, `function credit_tru_cu(...) does not exist`.

- [ ] **Step 3: Viết hàm**

Thêm vào cuối `004_credit.sql`:

```sql
-- Trừ lần lượt từ lô hết hạn sớm nhất, tối đa p_so. Người gọi phải đã khóa dòng users. Trả về số đã trừ.
CREATE OR REPLACE FUNCTION credit_tru_lan_luot(p_user UUID, p_luot UUID, p_so INT, p_loai TEXT, p_ghi_chu TEXT, p_boi TEXT)
RETURNS INT
LANGUAGE plpgsql SET search_path = public AS $$
DECLARE
    r RECORD;
    v_con INT := GREATEST(p_so, 0);
    v_lay INT;
    v_da INT := 0;
BEGIN
    FOR r IN SELECT id, so_con FROM credit_lo
              WHERE user_id = p_user AND het_han > NOW() AND so_con > 0
              ORDER BY het_han, id FOR UPDATE LOOP
        EXIT WHEN v_con = 0;
        v_lay := LEAST(r.so_con, v_con);
        UPDATE credit_lo SET so_con = so_con - v_lay WHERE id = r.id;
        INSERT INTO credit_so (user_id, lo_id, luot_id, loai, so, ghi_chu, boi)
        VALUES (p_user, r.id, p_luot, p_loai, -v_lay, p_ghi_chu, p_boi);
        v_con := v_con - v_lay;
        v_da := v_da + v_lay;
    END LOOP;
    RETURN v_da;
END;
$$;

-- /api/ai/* (bản cài máy cũ): đã gọi AI xong mới trừ, theo token thật, tối đa bằng số dư hiện có.
CREATE OR REPLACE FUNCTION credit_tru_cu(
    p_user UUID, p_tac_vu TEXT, p_so INT, p_provider TEXT, p_model TEXT,
    p_token_vao INT, p_token_ra INT, p_phien_ban TEXT
) RETURNS JSONB
LANGUAGE plpgsql SET search_path = public AS $$
DECLARE
    v_luot UUID;
    v_tru INT;
BEGIN
    PERFORM 1 FROM users WHERE id = p_user FOR UPDATE;
    IF NOT FOUND THEN RAISE EXCEPTION 'credit_tru_cu: không có người dùng %', p_user; END IF;
    PERFORM credit_cap_ky(p_user);
    INSERT INTO tac_vu_chay (user_id, tac_vu, trang_thai, credit_min, credit_max, provider, model,
                             token_vao, token_ra, nguon_goi, phien_ban_ung_dung, xong_luc)
    VALUES (p_user, p_tac_vu, 'xong', p_so, p_so, p_provider, p_model,
            p_token_vao, p_token_ra, 'api-cu', p_phien_ban, NOW())
    RETURNING id INTO v_luot;
    v_tru := credit_tru_lan_luot(p_user, v_luot, p_so, 'tru', NULL, NULL);
    UPDATE tac_vu_chay SET credit_tru = v_tru WHERE id = v_luot;
    RETURN jsonb_build_object('credit_tru', v_tru, 'so_du', credit_tong(p_user));
END;
$$;

-- Quản trị cộng/trừ tay. Dương: lô mới hạn 90 ngày. Âm: trừ từ lô hết hạn sớm nhất, không làm âm.
CREATE OR REPLACE FUNCTION credit_dieu_chinh(p_user UUID, p_so INT, p_ghi_chu TEXT, p_boi TEXT) RETURNS JSONB
LANGUAGE plpgsql SET search_path = public AS $$
DECLARE
    v_lo UUID;
    v_da INT;
BEGIN
    PERFORM 1 FROM users WHERE id = p_user FOR UPDATE;
    IF NOT FOUND THEN RAISE EXCEPTION 'credit_dieu_chinh: không có người dùng %', p_user; END IF;
    PERFORM credit_cap_ky(p_user);
    IF p_so > 0 THEN
        INSERT INTO credit_lo (user_id, nguon, so_cap, so_con, het_han)
        VALUES (p_user, 'dieu_chinh', p_so, p_so, NOW() + INTERVAL '90 days')
        RETURNING id INTO v_lo;
        INSERT INTO credit_so (user_id, lo_id, loai, so, ghi_chu, boi) VALUES (p_user, v_lo, 'dieu_chinh', p_so, p_ghi_chu, p_boi);
        v_da := p_so;
    ELSE
        v_da := -credit_tru_lan_luot(p_user, NULL, -p_so, 'dieu_chinh', p_ghi_chu, p_boi);
    END IF;
    RETURN jsonb_build_object('da_dieu_chinh', v_da, 'so_du', credit_tong(p_user));
END;
$$;

REVOKE EXECUTE ON FUNCTION credit_tru_lan_luot(UUID, UUID, INT, TEXT, TEXT, TEXT),
    credit_tru_cu(UUID, TEXT, INT, TEXT, TEXT, INT, INT, TEXT), credit_dieu_chinh(UUID, INT, TEXT, TEXT)
    FROM PUBLIC, anon, authenticated;
GRANT EXECUTE ON FUNCTION credit_tru_lan_luot(UUID, UUID, INT, TEXT, TEXT, TEXT),
    credit_tru_cu(UUID, TEXT, INT, TEXT, TEXT, INT, INT, TEXT), credit_dieu_chinh(UUID, INT, TEXT, TEXT)
    TO service_role;
```

- [ ] **Step 4: Chạy, thấy qua**

Expected: `# pass 16`. Chạy thêm một lần nữa toàn bộ `004_credit.sql` lên chính CSDL thử đó để chắc migration chạy lại được (mọi lệnh dùng `IF NOT EXISTS` / `OR REPLACE` / `ON CONFLICT`):

```bash
ssh gikidi-dev 'cat > /tmp/004.sql' < supabase/migrations/004_credit.sql && ssh gikidi-dev 'PGPASSWORD=thu psql -h 127.0.0.1 -p 55990 -U postgres -d postgres -v ON_ERROR_STOP=1 -q -c "create database vbp_lap" && for i in 1 2; do PGPASSWORD=thu psql -h 127.0.0.1 -p 55990 -U postgres -d vbp_lap -v ON_ERROR_STOP=1 -q -f /tmp/schema.sql -f /tmp/002_orders.sql -f /tmp/003_payment_settings.sql -f /tmp/004.sql || exit 1; done; echo LAP-OK; PGPASSWORD=thu psql -h 127.0.0.1 -p 55990 -U postgres -d postgres -q -c "drop database vbp_lap with (force)"'
```
(Nếu `/tmp/schema.sql`… chưa có trên máy chủ thì `scp` chúng lên trước.) Expected: `LAP-OK`.

- [ ] **Step 5: Commit**

```bash
git add supabase/migrations/004_credit.sql tests/credit-csdl.test.ts
git commit -m "feat(credit): credit_tru_cu cho /api/ai/*, credit_dieu_chinh cho quan tri"
```

---

### Task 5: Quy đổi token ↔ credit

**Files:**
- Create: `vanbanplus-api/src/lib/credit/uoc-tinh.ts`
- Test: `vanbanplus-api/tests/uoc-tinh.test.ts`

**Interfaces:**
- Produces: types `GiaMoHinh`, `CauHinhTacVu`, `CauHinhChung`, `BangGia`, `UocTinh`; `KY_TU_MOI_TOKEN = 3`; `lenTron(x)`, `giaCua(bg, model)`, `creditTuToken(vao, ra, gia, chung)`, `uocTinh(soKyTu, ch, gia, chung)`, `creditThat(vao, ra, ch, gia, chung, creditMax)`.

- [ ] **Step 1: Viết bộ kiểm hỏng**

`tests/uoc-tinh.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { creditThat, creditTuToken, giaCua, uocTinh, type BangGia } from "../src/lib/credit/uoc-tinh";

const chung = { gia_tri_credit_vnd: 100, ty_gia_vnd_usd: 26000, mo_hinh_mac_dinh: "gemini-3.6-flash" };
const g36 = { vao_usd_trieu: 1.5, ra_usd_trieu: 7.5 };
const tomTat = { toi_thieu: 2, ra_min_he_so: 0.2, ra_min_hang: 1500, ra_max_he_so: 0.6, ra_max_hang: 7000, toi_da_ky_tu: 120000 };

test("quy đổi token ra credit, làm tròn lên", () => {
  assert.equal(creditTuToken(1_000_000, 0, g36, chung), 390);
  assert.equal(creditTuToken(10_000, 4_000, g36, chung), 12); // 1.170đ
  assert.equal(creditTuToken(0, 0, g36, chung), 0);
});

test("đúng 100đ là đúng 1 credit, không bị sai số đẩy lên 2", () => {
  const gia = { vao_usd_trieu: 1, ra_usd_trieu: 4 };
  const c = { ...chung, ty_gia_vnd_usd: 25000 };
  assert.equal(creditTuToken(4000, 0, gia, c), 1);
  assert.equal(creditTuToken(4001, 0, gia, c), 2);
});

test("ước tính khoảng credit cho tóm tắt 12.000 ký tự", () => {
  assert.deepEqual(uocTinh(12_000, tomTat, g36, chung), {
    token_vao: 4000, ra_min: 2300, ra_max: 9400, credit_min: 7, credit_max: 20,
  });
});

test("ước tính không dưới mức tối thiểu", () => {
  const u = uocTinh(3, { ...tomTat, ra_min_hang: 0, ra_max_hang: 0, toi_thieu: 5 }, g36, chung);
  assert.equal(u.credit_min, 5);
  assert.equal(u.credit_max, 5);
});

test("credit thật: theo token, không dưới tối thiểu, không vượt phần giữ", () => {
  assert.equal(creditThat(4000, 3000, tomTat, g36, chung, 20), 8); // 741đ
  assert.equal(creditThat(100, 10, tomTat, g36, chung, 20), 2);
  assert.equal(creditThat(4000, 50_000, tomTat, g36, chung, 20), 20);
});

test("mô hình lạ dùng giá mô hình mặc định; thiếu cả hai thì báo lỗi", () => {
  const bg: BangGia = { chung, mo_hinh: { "gemini-3.6-flash": g36 }, tac_vu: {} };
  assert.equal(giaCua(bg, "gemini-9"), g36);
  assert.throws(() => giaCua({ ...bg, mo_hinh: {} }, "gemini-9"), /thiếu mô hình/);
});
```

- [ ] **Step 2: Chạy, thấy hỏng**

Run: `npx tsx --test tests/uoc-tinh.test.ts` — Expected: FAIL, không tìm thấy module.

- [ ] **Step 3: Viết mã**

`src/lib/credit/uoc-tinh.ts`:

```ts
/** Quy đổi token ↔ credit (spec mục 7). Hàm thuần, không đụng CSDL. */

export interface GiaMoHinh {
  vao_usd_trieu: number;
  ra_usd_trieu: number;
}

export interface CauHinhTacVu {
  toi_thieu: number;
  ra_min_he_so: number;
  ra_min_hang: number;
  ra_max_he_so: number;
  ra_max_hang: number;
  toi_da_ky_tu: number;
}

export interface CauHinhChung {
  gia_tri_credit_vnd: number;
  ty_gia_vnd_usd: number;
  mo_hinh_mac_dinh: string;
}

export interface BangGia {
  chung: CauHinhChung;
  mo_hinh: Record<string, GiaMoHinh>;
  tac_vu: Record<string, CauHinhTacVu>;
}

export interface UocTinh {
  token_vao: number;
  ra_min: number;
  ra_max: number;
  credit_min: number;
  credit_max: number;
}

export const KY_TU_MOI_TOKEN = 3;

/** Làm tròn lên số không âm, bỏ sai số dấu phẩy động (7,0000000001 vẫn là 7). */
export const lenTron = (x: number) => Math.max(0, Math.ceil(x - 1e-9));

export function giaCua(bg: BangGia, model: string): GiaMoHinh {
  const gia = bg.mo_hinh[model] ?? bg.mo_hinh[bg.chung.mo_hinh_mac_dinh];
  if (!gia) throw new Error(`Bảng giá thiếu mô hình ${model}`);
  return gia;
}

export function creditTuToken(tokenVao: number, tokenRa: number, gia: GiaMoHinh, chung: CauHinhChung): number {
  const vnd = ((tokenVao * gia.vao_usd_trieu + tokenRa * gia.ra_usd_trieu) * chung.ty_gia_vnd_usd) / 1_000_000;
  return lenTron(vnd / chung.gia_tri_credit_vnd);
}

/** Khoảng credit báo trước. ra_max đồng thời là trần token ra khi gọi AI. */
export function uocTinh(soKyTuVao: number, ch: CauHinhTacVu, gia: GiaMoHinh, chung: CauHinhChung): UocTinh {
  const token_vao = lenTron(soKyTuVao / KY_TU_MOI_TOKEN);
  const ra_min = lenTron(ch.ra_min_he_so * token_vao + ch.ra_min_hang);
  const ra_max = lenTron(ch.ra_max_he_so * token_vao + ch.ra_max_hang);
  const credit_min = Math.max(ch.toi_thieu, creditTuToken(token_vao, ra_min, gia, chung));
  const credit_max = Math.max(credit_min, creditTuToken(token_vao, ra_max, gia, chung));
  return { token_vao, ra_min, ra_max, credit_min, credit_max };
}

/** Credit trừ thật = max(tối thiểu, theo token thật), không vượt phần đã giữ. */
export function creditThat(
  tokenVao: number,
  tokenRa: number,
  ch: CauHinhTacVu,
  gia: GiaMoHinh,
  chung: CauHinhChung,
  creditMax: number
): number {
  return Math.min(creditMax, Math.max(ch.toi_thieu, creditTuToken(tokenVao, tokenRa, gia, chung)));
}
```

- [ ] **Step 4: Chạy, thấy qua** — `npx tsx --test tests/uoc-tinh.test.ts` → `# pass 6`.

- [ ] **Step 5: Commit**

```bash
git add src/lib/credit/uoc-tinh.ts tests/uoc-tinh.test.ts
git commit -m "feat(credit): quy doi token sang credit, uoc tinh khoang credit"
```

---

### Task 6: Đọc bảng giá, kho credit, khóa hệ thống

**Files:**
- Create: `vanbanplus-api/src/lib/credit/bang-gia.ts`
- Create: `vanbanplus-api/src/lib/credit/so-du.ts`
- Modify: `vanbanplus-api/src/lib/ai-keys.ts:9-37` (thêm tùy chọn `chiHeThong`)
- Test: `vanbanplus-api/tests/bang-gia.test.ts`

**Interfaces:**
- Consumes: types từ `uoc-tinh.ts` (Task 5); hàm SQL Task 2–4.
- Produces:
  - `bang-gia.ts`: `interface DongBangGia { loai: string; ma: string; gia_tri: unknown }`, `dungBangGia(dong: DongBangGia[]): BangGia` (ném lỗi nếu thiếu/sai), `docBangGia(): Promise<BangGia>` (nhớ 60 giây), `xoaBoNhoBangGia(): void`.
  - `so-du.ts`: `LoCredit`, `SoDu { tong; lo: LoCredit[]; het_han_gan_nhat: string | null }`, `MoLuot`, `KetQuaMo`, `ChotLuot`, `interface KhoCredit { soDu(userId): Promise<number>; mo(m: MoLuot): Promise<KetQuaMo>; chot(luotId, c: ChotLuot): Promise<{ credit_tru: number; so_du: number }>; hoan(luotId, maLoi): Promise<{ so_du: number }> }`, `khoCreditSupabase: KhoCredit`, `docSoDu(userId): Promise<SoDu>`, `truCreditCu(a: TruCu): Promise<{ credit_tru: number; so_du: number }>`, `dieuChinhCredit(userId, so, ghiChu, boi): Promise<{ da_dieu_chinh: number; so_du: number }>`.
  - `resolveAIKey(userId, provider?, tuyChon?: { chiHeThong?: boolean })`.

- [ ] **Step 1: Viết bộ kiểm hỏng**

`tests/bang-gia.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { dungBangGia, type DongBangGia } from "../src/lib/credit/bang-gia";

const DONG: DongBangGia[] = [
  { loai: "cau_hinh", ma: "chung", gia_tri: { gia_tri_credit_vnd: 100, ty_gia_vnd_usd: 26000, mo_hinh_mac_dinh: "gemini-3.6-flash" } },
  { loai: "mo_hinh", ma: "gemini-3.6-flash", gia_tri: { vao_usd_trieu: 1.5, ra_usd_trieu: 7.5 } },
  { loai: "tac_vu", ma: "tom-tat", gia_tri: { toi_thieu: 2, ra_min_he_so: 0.2, ra_min_hang: 1500, ra_max_he_so: 0.6, ra_max_hang: 7000, toi_da_ky_tu: 120000 } },
  { loai: "goi_nap", ma: "nap-49", gia_tri: { gia_vnd: 49000, credit: 160, so_ngay: 90 } },
];

test("dựng bảng giá từ các dòng CSDL", () => {
  const bg = dungBangGia(DONG);
  assert.equal(bg.chung.mo_hinh_mac_dinh, "gemini-3.6-flash");
  assert.equal(bg.mo_hinh["gemini-3.6-flash"].ra_usd_trieu, 7.5);
  assert.equal(bg.tac_vu["tom-tat"].ra_max_hang, 7000);
});

test("thiếu dòng chung hoặc thiếu giá mô hình mặc định thì báo lỗi", () => {
  assert.throws(() => dungBangGia(DONG.slice(1)), /cau_hinh\/chung/);
  assert.throws(() => dungBangGia(DONG.filter((d) => d.loai !== "mo_hinh")), /mô hình mặc định/);
});

test("giá trị không phải số không âm thì báo lỗi, nêu rõ trường", () => {
  const hong = DONG.map((d) => (d.ma === "tom-tat" ? { ...d, gia_tri: { ...(d.gia_tri as object), toi_thieu: "2" } } : d));
  assert.throws(() => dungBangGia(hong), /tom-tat\.toi_thieu/);
  const am = DONG.map((d) => (d.loai === "mo_hinh" ? { ...d, gia_tri: { vao_usd_trieu: -1, ra_usd_trieu: 1 } } : d));
  assert.throws(() => dungBangGia(am), /vao_usd_trieu/);
});
```

- [ ] **Step 2: Chạy, thấy hỏng** — `npx tsx --test tests/bang-gia.test.ts` → FAIL (không có module).

- [ ] **Step 3: Viết mã**

`src/lib/credit/bang-gia.ts`:

```ts
import { getSupabase } from "../supabase";
import type { BangGia, CauHinhChung, CauHinhTacVu, GiaMoHinh } from "./uoc-tinh";

export interface DongBangGia {
  loai: string;
  ma: string;
  gia_tri: unknown;
}

function soKhongAm(v: unknown, ten: string): number {
  if (typeof v !== "number" || !Number.isFinite(v) || v < 0) {
    throw new Error(`Bảng giá: ${ten} phải là số không âm`);
  }
  return v;
}

const TRUONG_TAC_VU = ["toi_thieu", "ra_min_he_so", "ra_min_hang", "ra_max_he_so", "ra_max_hang", "toi_da_ky_tu"] as const;

/** Dựng và kiểm bảng giá từ các dòng credit_bang_gia. Sai thì ném lỗi nêu rõ trường. */
export function dungBangGia(dong: DongBangGia[]): BangGia {
  let chung: CauHinhChung | null = null;
  const mo_hinh: Record<string, GiaMoHinh> = {};
  const tac_vu: Record<string, CauHinhTacVu> = {};

  for (const d of dong) {
    const g = (d.gia_tri ?? {}) as Record<string, unknown>;
    if (d.loai === "cau_hinh" && d.ma === "chung") {
      if (typeof g.mo_hinh_mac_dinh !== "string" || !g.mo_hinh_mac_dinh) {
        throw new Error("Bảng giá: chung.mo_hinh_mac_dinh phải là tên mô hình");
      }
      chung = {
        gia_tri_credit_vnd: soKhongAm(g.gia_tri_credit_vnd, "chung.gia_tri_credit_vnd"),
        ty_gia_vnd_usd: soKhongAm(g.ty_gia_vnd_usd, "chung.ty_gia_vnd_usd"),
        mo_hinh_mac_dinh: g.mo_hinh_mac_dinh,
      };
    } else if (d.loai === "mo_hinh") {
      mo_hinh[d.ma] = {
        vao_usd_trieu: soKhongAm(g.vao_usd_trieu, `${d.ma}.vao_usd_trieu`),
        ra_usd_trieu: soKhongAm(g.ra_usd_trieu, `${d.ma}.ra_usd_trieu`),
      };
    } else if (d.loai === "tac_vu") {
      const t = {} as CauHinhTacVu;
      for (const k of TRUONG_TAC_VU) t[k] = soKhongAm(g[k], `${d.ma}.${k}`);
      tac_vu[d.ma] = t;
    }
  }

  if (!chung) throw new Error("Bảng giá: thiếu dòng cau_hinh/chung");
  if (chung.gia_tri_credit_vnd <= 0) throw new Error("Bảng giá: chung.gia_tri_credit_vnd phải lớn hơn 0");
  if (!mo_hinh[chung.mo_hinh_mac_dinh]) {
    throw new Error(`Bảng giá: thiếu giá mô hình mặc định ${chung.mo_hinh_mac_dinh}`);
  }
  return { chung, mo_hinh, tac_vu };
}

const HAN_BO_NHO_MS = 60_000;
let boNho: { luc: number; bg: BangGia } | null = null;

export async function docBangGia(): Promise<BangGia> {
  if (boNho && Date.now() - boNho.luc < HAN_BO_NHO_MS) return boNho.bg;
  const { data, error } = await getSupabase().from("credit_bang_gia").select("loai, ma, gia_tri");
  if (error) throw new Error(`Không đọc được bảng giá credit: ${error.message}`);
  const bg = dungBangGia((data ?? []) as DongBangGia[]);
  boNho = { luc: Date.now(), bg };
  return bg;
}

export function xoaBoNhoBangGia(): void {
  boNho = null;
}
```

`src/lib/credit/so-du.ts`:

```ts
import { getSupabase } from "../supabase";

export interface LoCredit {
  nguon: string;
  so_cap: number;
  so_con: number;
  het_han: string;
}

export interface SoDu {
  tong: number;
  lo: LoCredit[];
  het_han_gan_nhat: string | null;
}

export interface MoLuot {
  user_id: string;
  tac_vu: string;
  credit_min: number;
  credit_max: number;
  provider: string;
  model: string;
  nguon_goi: string;
  phien_ban: string | null;
}

export interface KetQuaMo {
  ma: "ok" | "KHONG_DU_CREDIT" | "QUA_NHIEU_LUOT";
  luot_id?: string;
  so_du: number;
}

export interface ChotLuot {
  credit_that: number;
  token_vao: number;
  token_ra: number;
  bi_cat: boolean;
}

/** Mọi thao tác đổi số dư. Bản thật gọi hàm Postgres; bộ kiểm dùng bản giả. */
export interface KhoCredit {
  soDu(userId: string): Promise<number>;
  mo(m: MoLuot): Promise<KetQuaMo>;
  chot(luotId: string, c: ChotLuot): Promise<{ credit_tru: number; so_du: number }>;
  hoan(luotId: string, maLoi: string): Promise<{ so_du: number }>;
}

async function goiHam<T>(ten: string, thamSo: Record<string, unknown>): Promise<T> {
  const { data, error } = await getSupabase().rpc(ten, thamSo);
  if (error) throw new Error(`${ten}: ${error.message}`);
  return data as T;
}

export const docSoDu = (userId: string) => goiHam<SoDu>("credit_so_du", { p_user: userId });

export const khoCreditSupabase: KhoCredit = {
  soDu: async (userId) => (await docSoDu(userId)).tong,
  mo: (m) =>
    goiHam<KetQuaMo>("tac_vu_mo", {
      p_user: m.user_id,
      p_tac_vu: m.tac_vu,
      p_credit_min: m.credit_min,
      p_credit_max: m.credit_max,
      p_provider: m.provider,
      p_model: m.model,
      p_nguon_goi: m.nguon_goi,
      p_phien_ban: m.phien_ban,
    }),
  chot: (luotId, c) =>
    goiHam("credit_chot", {
      p_luot: luotId,
      p_credit_that: c.credit_that,
      p_token_vao: c.token_vao,
      p_token_ra: c.token_ra,
      p_bi_cat: c.bi_cat,
    }),
  hoan: (luotId, maLoi) => goiHam("credit_hoan", { p_luot: luotId, p_ma_loi: maLoi }),
};

export interface TruCu {
  user_id: string;
  tac_vu: string;
  so: number;
  provider: string;
  model: string;
  token_vao: number;
  token_ra: number;
  phien_ban: string | null;
}

export const truCreditCu = (a: TruCu) =>
  goiHam<{ credit_tru: number; so_du: number }>("credit_tru_cu", {
    p_user: a.user_id,
    p_tac_vu: a.tac_vu,
    p_so: a.so,
    p_provider: a.provider,
    p_model: a.model,
    p_token_vao: a.token_vao,
    p_token_ra: a.token_ra,
    p_phien_ban: a.phien_ban,
  });

export const dieuChinhCredit = (userId: string, so: number, ghiChu: string, boi: string) =>
  goiHam<{ da_dieu_chinh: number; so_du: number }>("credit_dieu_chinh", {
    p_user: userId,
    p_so: so,
    p_ghi_chu: ghiChu,
    p_boi: boi,
  });
```

`src/lib/ai-keys.ts` — đổi chữ ký và bước 1 của `resolveAIKey`:

```ts
/**
 * Lấy AI key cho 1 user + provider cụ thể.
 * Ưu tiên: user key → system default → env var fallback.
 * chiHeThong: bỏ qua khóa riêng của người dùng (tác vụ v2 tính credit nên chỉ chạy bằng khóa hệ thống).
 */
export async function resolveAIKey(
  userId: string,
  provider?: AIProvider,
  tuyChon: { chiHeThong?: boolean } = {}
): Promise<ResolvedAIKey | null> {
  const sb = getSupabase();
  const targetProvider = provider || "gemini";
  console.log(`  [resolveAIKey] userId=${userId}, provider=${targetProvider}`);

  // 1. User's own key
  if (!tuyChon.chiHeThong) {
    const t1 = Date.now();
    const { data: userKey } = await sb
      .from("ai_api_keys")
      .select("*")
      .eq("user_id", userId)
      .eq("provider", targetProvider)
      .eq("is_active", true)
      .eq("is_default", true)
      .single();

    if (userKey) {
      console.log(`  [resolveAIKey] Found USER key in ${Date.now() - t1}ms (id=${userKey.id})`);
      return {
        keyId: userKey.id,
        provider: targetProvider,
        apiKey: userKey.api_key,
        model: userKey.model || DEFAULT_MODELS[targetProvider],
        source: "user",
      };
    }
  }
```
(Phần bước 2, 3 giữ nguyên.)

- [ ] **Step 4: Chạy bộ kiểm + kiểm kiểu**

```bash
npx tsx --test tests/bang-gia.test.ts && npx tsc --noEmit -p .
```
Expected: `# pass 3`; `tsc` không báo lỗi.

- [ ] **Step 5: Commit**

```bash
git add src/lib/credit/bang-gia.ts src/lib/credit/so-du.ts src/lib/ai-keys.ts tests/bang-gia.test.ts
git commit -m "feat(credit): doc bang gia, kho credit goi ham Postgres, resolveAIKey chi khoa he thong"
```

---

### Task 7: Luồng chung chạy tác vụ

**Files:**
- Create: `vanbanplus-api/src/lib/tac-vu/doc-json.ts`
- Create: `vanbanplus-api/src/lib/tac-vu/chay-tac-vu.ts`
- Test: `vanbanplus-api/tests/doc-json.test.ts`, `vanbanplus-api/tests/chay-tac-vu.test.ts`

**Interfaces:**
- Consumes: `BangGia`, `CauHinhTacVu`, `UocTinh`, `uocTinh`, `creditThat`, `giaCua` (Task 5); `KhoCredit` (Task 6); `AICallOptions`, `AICallResult`, `ApiUser`, `ResolvedAIKey` từ `src/types`.
- Produces:
  - `docJson(text: string): unknown` — `undefined` nếu hỏng.
  - `type MaLoi = "DAU_VAO_SAI" | "QUA_DAI" | "KHONG_DU_CREDIT" | "QUA_NHIEU_LUOT" | "AI_QUA_TAI" | "AI_LOI" | "NGAT_KET_NOI"`
  - `interface PromptTacVu { he_thong: string; nguoi_dung: string }`
  - `interface DinhNghiaTacVu<V> { ten; kieu_ket_qua: "json" | "van_ban"; nhiet_do: number; kiemDauVao(body: unknown): { ok: true; du_lieu: V } | { ok: false; loi: string }; dungPrompt(du_lieu: V, bayGio: Date): PromptTacVu; kiemKetQua?(kq: unknown): boolean }`
  - `interface PhuThuoc { bangGia(): Promise<BangGia>; kho: KhoCredit; layKhoa(userId): Promise<ResolvedAIKey | null>; goiAI(contents: unknown[], opts: AICallOptions): Promise<AICallResult>; bayGio(): Date }`
  - `interface NguCanhGoi { user: ApiUser; clientIp?: string; nguonGoi: string; phienBan: string | null; signal?: AbortSignal }`
  - `type Loi = { ok: false; code: MaLoi; message: string; status: number; retryAfter?: number }`
  - `chayTacVu(dn, body, ctx, phu): Promise<{ ok: true; ket_qua; credit_da_tru; so_du; ma_luot; bi_cat } | Loi>`
  - `uocTinhTacVu(dn, body, user, phu): Promise<{ ok: true; credit_min; credit_max; so_du; du_credit } | Loi>`

- [ ] **Step 1: Viết bộ kiểm hỏng**

`tests/doc-json.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { docJson } from "../src/lib/tac-vu/doc-json";

test("đọc JSON thuần, JSON trong rào ```json, JSON lẫn chữ", () => {
  assert.deepEqual(docJson('{"a":1}'), { a: 1 });
  assert.deepEqual(docJson('```json\n{"a":1}\n```'), { a: 1 });
  assert.deepEqual(docJson('Kết quả:\n{"a":{"b":2}}\nHết.'), { a: { b: 2 } });
});

test("JSON hỏng hoặc bị cắt thì undefined", () => {
  assert.equal(docJson('{"a":'), undefined);
  assert.equal(docJson(""), undefined);
});
```

`tests/chay-tac-vu.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { chayTacVu, uocTinhTacVu, type DinhNghiaTacVu, type PhuThuoc } from "../src/lib/tac-vu/chay-tac-vu";
import type { BangGia } from "../src/lib/credit/uoc-tinh";
import type { KetQuaMo, KhoCredit } from "../src/lib/credit/so-du";
import type { AICallOptions, AICallResult, ApiUser } from "../src/types";

const BG: BangGia = {
  chung: { gia_tri_credit_vnd: 100, ty_gia_vnd_usd: 26000, mo_hinh_mac_dinh: "gemini-3.6-flash" },
  mo_hinh: { "gemini-3.6-flash": { vao_usd_trieu: 1.5, ra_usd_trieu: 7.5 } },
  tac_vu: { thu: { toi_thieu: 2, ra_min_he_so: 0, ra_min_hang: 1000, ra_max_he_so: 0, ra_max_hang: 5000, toi_da_ky_tu: 100 } },
};

const thuJson: DinhNghiaTacVu<{ noi_dung: string }> = {
  ten: "thu",
  kieu_ket_qua: "json",
  nhiet_do: 0.3,
  kiemDauVao: (b) => {
    const nd = (b as { noi_dung?: unknown } | null)?.noi_dung;
    return typeof nd === "string" && nd ? { ok: true, du_lieu: { noi_dung: nd } } : { ok: false, loi: "Thiếu noi_dung." };
  },
  dungPrompt: (d) => ({ he_thong: "HT", nguoi_dung: d.noi_dung }),
  kiemKetQua: (k) => typeof (k as { brief?: unknown })?.brief === "string",
};

const USER = { id: "U1", api_key: "vbp_x" } as ApiUser;
const AI_OK = (content: string, finishReason = "STOP"): AICallResult => ({
  success: true, content, promptTokens: 10, completionTokens: 500, thoughtsTokens: 500, finishReason,
});

function dungPhu(tra: Array<AICallResult | Error>, moTra: KetQuaMo = { ma: "ok", luot_id: "L1", so_du: 20 }) {
  const goiKho: unknown[][] = [];
  const optsAI: AICallOptions[] = [];
  const kho: KhoCredit = {
    soDu: async () => 30,
    mo: async (m) => { goiKho.push(["mo", m]); return moTra; },
    chot: async (id, c) => { goiKho.push(["chot", id, c]); return { credit_tru: c.credit_that, so_du: 18 }; },
    hoan: async (id, ma) => { goiKho.push(["hoan", id, ma]); return { so_du: 30 }; },
  };
  const phu: PhuThuoc = {
    bangGia: async () => BG,
    kho,
    layKhoa: async () => ({ keyId: "k", provider: "gemini", apiKey: "x", model: "gemini-3.5-flash", source: "system" }),
    goiAI: async (_c, o) => {
      optsAI.push(o);
      const t = tra.shift();
      if (!t) throw new Error("hết phản hồi giả");
      if (t instanceof Error) throw t;
      return t;
    },
    bayGio: () => new Date("2026-11-12T00:00:00Z"),
  };
  return { phu, goiKho, optsAI };
}

const CTX = { user: USER, nguonGoi: "kiem-thu", phienBan: null };

test("đầu vào sai: 400, không giữ credit, không gọi AI", async () => {
  const { phu, goiKho, optsAI } = dungPhu([]);
  const kq = await chayTacVu(thuJson, {}, CTX, phu);
  assert.deepEqual([kq.ok, !kq.ok && kq.code, !kq.ok && kq.status], [false, "DAU_VAO_SAI", 400]);
  assert.equal(goiKho.length + optsAI.length, 0);
});

test("quá dài: 413, không giữ credit", async () => {
  const { phu, goiKho } = dungPhu([]);
  const kq = await chayTacVu(thuJson, { noi_dung: "x".repeat(101) }, CTX, phu);
  assert.equal(!kq.ok && kq.code, "QUA_DAI");
  assert.equal(!kq.ok && kq.status, 413);
  assert.equal(goiKho.length, 0);
});

test("không đủ credit: 402; quá nhiều lượt: 429; đều không gọi AI", async () => {
  const a = dungPhu([], { ma: "KHONG_DU_CREDIT", so_du: 3 });
  const kqA = await chayTacVu(thuJson, { noi_dung: "abc" }, CTX, a.phu);
  assert.deepEqual([!kqA.ok && kqA.code, !kqA.ok && kqA.status], ["KHONG_DU_CREDIT", 402]);
  const b = dungPhu([], { ma: "QUA_NHIEU_LUOT", so_du: 30 });
  const kqB = await chayTacVu(thuJson, { noi_dung: "abc" }, CTX, b.phu);
  assert.deepEqual([!kqB.ok && kqB.code, !kqB.ok && kqB.status], ["QUA_NHIEU_LUOT", 429]);
  assert.equal(a.optsAI.length + b.optsAI.length, 0);
});

test("thành công: giữ credit_max, gọi AI bằng mô hình mặc định và trần token ra, chốt theo token thật", async () => {
  const { phu, goiKho, optsAI } = dungPhu([AI_OK('{"brief":"ok"}')]);
  const kq = await chayTacVu(thuJson, { noi_dung: "abc" }, CTX, phu);
  assert.deepEqual(kq, { ok: true, ket_qua: { brief: "ok" }, credit_da_tru: 2, so_du: 18, ma_luot: "L1", bi_cat: false });
  const mo = goiKho[0][1] as { credit_min: number; credit_max: number; model: string };
  assert.deepEqual([mo.credit_min, mo.credit_max, mo.model], [2, 10, "gemini-3.6-flash"]);
  assert.equal(optsAI[0].resolvedKey.model, "gemini-3.6-flash");
  assert.equal(optsAI[0].maxTokens, 5000);
  assert.equal(optsAI[0].systemInstruction, "HT");
  assert.equal(optsAI[0].temperature, 0.3);
  assert.deepEqual(goiKho[1], ["chot", "L1", { credit_that: 2, token_vao: 10, token_ra: 1000, bi_cat: false }]);
});

test("JSON hỏng lần đầu thì thử lại một lần, cộng token cả hai lần", async () => {
  const { phu, goiKho, optsAI } = dungPhu([AI_OK("không phải json"), AI_OK('{"brief":"ok"}')]);
  const kq = await chayTacVu(thuJson, { noi_dung: "abc" }, CTX, phu);
  assert.equal(kq.ok, true);
  assert.equal(optsAI.length, 2);
  assert.deepEqual(goiKho[1], ["chot", "L1", { credit_that: 4, token_vao: 20, token_ra: 2000, bi_cat: false }]);
});

test("JSON hỏng cả hai lần: AI_LOI 502, hoàn credit", async () => {
  const { phu, goiKho } = dungPhu([AI_OK("x"), AI_OK('{"khac":1}')]);
  const kq = await chayTacVu(thuJson, { noi_dung: "abc" }, CTX, phu);
  assert.deepEqual([!kq.ok && kq.code, !kq.ok && kq.status], ["AI_LOI", 502]);
  assert.deepEqual(goiKho[1], ["hoan", "L1", "JSON_HONG"]);
});

test("nhà cung cấp trả 429: AI_QUA_TAI 503 kèm Retry-After, hoàn credit", async () => {
  const { phu, goiKho } = dungPhu([{ success: false, error: "AI API error 429: quota" }]);
  const kq = await chayTacVu(thuJson, { noi_dung: "abc" }, CTX, phu);
  assert.deepEqual([!kq.ok && kq.code, !kq.ok && kq.status, !kq.ok && kq.retryAfter], ["AI_QUA_TAI", 503, 30]);
  assert.deepEqual(goiKho[1], ["hoan", "L1", "AI_LOI"]);
});

test("bị cắt vì chạm trần token: vẫn thành công, có trừ, trả cờ bi_cat và văn bản thô", async () => {
  const { phu, goiKho } = dungPhu([AI_OK('{"brief":"dở', "MAX_TOKENS")]);
  const kq = await chayTacVu(thuJson, { noi_dung: "abc" }, CTX, phu);
  assert.equal(kq.ok && kq.bi_cat, true);
  assert.equal(kq.ok && kq.ket_qua, '{"brief":"dở');
  assert.equal(goiKho[1][0], "chot");
});

test("người gọi đã ngắt kết nối: hoàn credit, mã NGAT_KET_NOI", async () => {
  const { phu, goiKho } = dungPhu([AI_OK('{"brief":"ok"}')]);
  const ac = new AbortController();
  ac.abort();
  const kq = await chayTacVu(thuJson, { noi_dung: "abc" }, { ...CTX, signal: ac.signal }, phu);
  assert.equal(!kq.ok && kq.code, "NGAT_KET_NOI");
  assert.deepEqual(goiKho[1], ["hoan", "L1", "NGAT_KET_NOI"]);
});

test("lỗi bất ngờ khi gọi AI: hoàn credit, AI_LOI", async () => {
  const { phu, goiKho } = dungPhu([new Error("mạng đứt")]);
  const kq = await chayTacVu(thuJson, { noi_dung: "abc" }, CTX, phu);
  assert.equal(!kq.ok && kq.code, "AI_LOI");
  assert.deepEqual(goiKho[1], ["hoan", "L1", "LOI_MAY_CHU"]);
});

test("ước tính: trả khoảng credit và số dư, không giữ credit", async () => {
  const { phu, goiKho } = dungPhu([]);
  assert.deepEqual(await uocTinhTacVu(thuJson, { noi_dung: "abc" }, USER, phu), {
    ok: true, credit_min: 2, credit_max: 10, so_du: 30, du_credit: true,
  });
  assert.equal(goiKho.length, 0);
});
```

- [ ] **Step 2: Chạy, thấy hỏng** — `npx tsx --test tests/doc-json.test.ts tests/chay-tac-vu.test.ts` → FAIL (không có module).

- [ ] **Step 3: Viết mã**

`src/lib/tac-vu/doc-json.ts`:

```ts
/** Đọc JSON AI trả về: bỏ rào ```json, lấy từ «{» đầu tới «}» cuối. Hỏng thì undefined. */
export function docJson(text: string): unknown {
  let t = text.trim();
  if (t.startsWith("```")) {
    const xuong = t.indexOf("\n");
    t = xuong > 0 ? t.slice(xuong + 1) : t.slice(3);
    if (t.endsWith("```")) t = t.slice(0, -3);
    t = t.trim();
  }
  const dau = t.indexOf("{");
  const cuoi = t.lastIndexOf("}");
  if (dau >= 0 && cuoi > dau) t = t.slice(dau, cuoi + 1);
  try {
    return JSON.parse(t);
  } catch {
    return undefined;
  }
}
```

`src/lib/tac-vu/chay-tac-vu.ts`:

```ts
import type { AICallOptions, AICallResult, ApiUser, ResolvedAIKey } from "@/types";
import { creditThat, giaCua, uocTinh, type BangGia, type CauHinhTacVu, type UocTinh } from "../credit/uoc-tinh";
import type { KhoCredit } from "../credit/so-du";
import { docJson } from "./doc-json";

export type MaLoi =
  | "DAU_VAO_SAI"
  | "QUA_DAI"
  | "KHONG_DU_CREDIT"
  | "QUA_NHIEU_LUOT"
  | "AI_QUA_TAI"
  | "AI_LOI"
  | "NGAT_KET_NOI";

export interface PromptTacVu {
  he_thong: string;
  nguoi_dung: string;
}

export interface DinhNghiaTacVu<V = unknown> {
  ten: string;
  kieu_ket_qua: "json" | "van_ban";
  nhiet_do: number;
  kiemDauVao(body: unknown): { ok: true; du_lieu: V } | { ok: false; loi: string };
  /** Hàm thuần. Thời điểm hiện tại (nếu prompt cần) lấy từ bayGio để bộ kiểm so khớp được. */
  dungPrompt(du_lieu: V, bayGio: Date): PromptTacVu;
  /** Chỉ với kiểu json: kết quả đã phân tích có dùng được không. */
  kiemKetQua?(ket_qua: unknown): boolean;
}

export interface PhuThuoc {
  bangGia(): Promise<BangGia>;
  kho: KhoCredit;
  layKhoa(userId: string): Promise<ResolvedAIKey | null>;
  goiAI(contents: unknown[], opts: AICallOptions): Promise<AICallResult>;
  bayGio(): Date;
}

export interface NguCanhGoi {
  user: ApiUser;
  clientIp?: string;
  nguonGoi: string;
  phienBan: string | null;
  signal?: AbortSignal;
}

export type Loi = { ok: false; code: MaLoi; message: string; status: number; retryAfter?: number };

export type KetQuaChay =
  | { ok: true; ket_qua: unknown; credit_da_tru: number; so_du: number; ma_luot: string; bi_cat: boolean }
  | Loi;

export type KetQuaUocTinh =
  | { ok: true; credit_min: number; credit_max: number; so_du: number; du_credit: boolean }
  | Loi;

const loi = (code: MaLoi, status: number, message: string, retryAfter?: number): Loi =>
  retryAfter ? { ok: false, code, status, message, retryAfter } : { ok: false, code, status, message };

const LY_DO_BI_CAT = new Set(["MAX_TOKENS", "length", "max_tokens"]);
const SO_LAN_GOI_TOI_DA = 2;

type ChuanBi =
  | { ok: true; prompt: PromptTacVu; bg: BangGia; ch: CauHinhTacVu; model: string; uoc: UocTinh }
  | Loi;

async function chuanBi<V>(dn: DinhNghiaTacVu<V>, body: unknown, phu: PhuThuoc): Promise<ChuanBi> {
  const dv = dn.kiemDauVao(body);
  if (!dv.ok) return loi("DAU_VAO_SAI", 400, dv.loi);

  const bg = await phu.bangGia();
  const ch = bg.tac_vu[dn.ten];
  if (!ch) throw new Error(`Bảng giá thiếu tác vụ ${dn.ten}`);

  const prompt = dn.dungPrompt(dv.du_lieu, phu.bayGio());
  if (prompt.nguoi_dung.length > ch.toi_da_ky_tu) {
    return loi("QUA_DAI", 413, `Văn bản quá dài cho tác vụ này (tối đa ${ch.toi_da_ky_tu.toLocaleString("vi-VN")} ký tự).`);
  }

  const model = bg.chung.mo_hinh_mac_dinh;
  const uoc = uocTinh(prompt.he_thong.length + prompt.nguoi_dung.length, ch, giaCua(bg, model), bg.chung);
  return { ok: true, prompt, bg, ch, model, uoc };
}

export async function uocTinhTacVu<V>(
  dn: DinhNghiaTacVu<V>,
  body: unknown,
  user: ApiUser,
  phu: PhuThuoc
): Promise<KetQuaUocTinh> {
  const cb = await chuanBi(dn, body, phu);
  if (!cb.ok) return cb;
  const so_du = await phu.kho.soDu(user.id);
  return { ok: true, credit_min: cb.uoc.credit_min, credit_max: cb.uoc.credit_max, so_du, du_credit: so_du >= cb.uoc.credit_max };
}

export async function chayTacVu<V>(
  dn: DinhNghiaTacVu<V>,
  body: unknown,
  ctx: NguCanhGoi,
  phu: PhuThuoc
): Promise<KetQuaChay> {
  const cb = await chuanBi(dn, body, phu);
  if (!cb.ok) return cb;

  const khoa = await phu.layKhoa(ctx.user.id);
  if (!khoa) return loi("AI_LOI", 502, "Chưa cấu hình khóa AI của hệ thống. Liên hệ quản trị.");
  const khoaChay: ResolvedAIKey = { ...khoa, model: cb.model };

  const mo = await phu.kho.mo({
    user_id: ctx.user.id,
    tac_vu: dn.ten,
    credit_min: cb.uoc.credit_min,
    credit_max: cb.uoc.credit_max,
    provider: khoaChay.provider,
    model: khoaChay.model,
    nguon_goi: ctx.nguonGoi,
    phien_ban: ctx.phienBan,
  });
  if (mo.ma === "KHONG_DU_CREDIT") {
    return loi("KHONG_DU_CREDIT", 402, `Không đủ credit: tác vụ cần tối đa ${cb.uoc.credit_max}, số dư còn ${mo.so_du}.`);
  }
  if (mo.ma === "QUA_NHIEU_LUOT" || !mo.luot_id) {
    return loi("QUA_NHIEU_LUOT", 429, "Đang có 3 tác vụ chạy cùng lúc. Chờ một tác vụ xong rồi thử lại.");
  }
  const luot = mo.luot_id;

  let tokenVao = 0;
  let tokenRa = 0;
  try {
    for (let lan = 1; ; lan++) {
      const r = await phu.goiAI([{ parts: [{ text: cb.prompt.nguoi_dung }] }], {
        user: ctx.user,
        resolvedKey: khoaChay,
        endpoint: `/api/v2/tac-vu/${dn.ten}`,
        requestType: `v2:${dn.ten}`,
        clientIp: ctx.clientIp,
        systemInstruction: cb.prompt.he_thong,
        temperature: dn.nhiet_do,
        maxTokens: cb.uoc.ra_max,
      });

      if (!r.success) {
        await phu.kho.hoan(luot, "AI_LOI");
        return /\b429\b/.test(r.error ?? "")
          ? loi("AI_QUA_TAI", 503, "Máy chủ AI đang quá tải. Credit đã được hoàn, thử lại sau ít phút.", 30)
          : loi("AI_LOI", 502, "AI xử lý không thành công. Credit đã được hoàn.");
      }

      tokenVao += r.promptTokens ?? 0;
      tokenRa += (r.completionTokens ?? 0) + (r.thoughtsTokens ?? 0);
      const biCat = LY_DO_BI_CAT.has(r.finishReason ?? "");
      const noiDung = r.content ?? "";

      let ketQua: unknown = noiDung;
      if (dn.kieu_ket_qua === "json") {
        const obj = docJson(noiDung);
        const dung = obj !== undefined && (dn.kiemKetQua?.(obj) ?? true);
        if (dung) {
          ketQua = obj;
        } else if (!biCat) {
          if (lan < SO_LAN_GOI_TOI_DA) continue;
          await phu.kho.hoan(luot, "JSON_HONG");
          return loi("AI_LOI", 502, "AI trả kết quả sai định dạng. Credit đã được hoàn.");
        }
      }

      if (ctx.signal?.aborted) {
        await phu.kho.hoan(luot, "NGAT_KET_NOI");
        return loi("NGAT_KET_NOI", 499, "Kết nối đã ngắt trước khi nhận kết quả. Credit đã được hoàn.");
      }

      const credit = creditThat(tokenVao, tokenRa, cb.ch, giaCua(cb.bg, cb.model), cb.bg.chung, cb.uoc.credit_max);
      const chot = await phu.kho.chot(luot, { credit_that: credit, token_vao: tokenVao, token_ra: tokenRa, bi_cat: biCat });
      return { ok: true, ket_qua: ketQua, credit_da_tru: chot.credit_tru, so_du: chot.so_du, ma_luot: luot, bi_cat: biCat };
    }
  } catch (e) {
    console.error(`🔴 [TAC-VU ${dn.ten}] lượt ${luot} lỗi: ${(e as Error).message}`);
    await phu.kho.hoan(luot, "LOI_MAY_CHU").catch(() => {});
    return loi("AI_LOI", 502, "AI xử lý không thành công. Credit đã được hoàn.");
  }
}
```

- [ ] **Step 4: Chạy, thấy qua**

```bash
npx tsx --test tests/doc-json.test.ts tests/chay-tac-vu.test.ts && npx tsc --noEmit -p .
```
Expected: `# pass 13`, `tsc` sạch.

- [ ] **Step 5: Commit**

```bash
git add src/lib/tac-vu/doc-json.ts src/lib/tac-vu/chay-tac-vu.ts tests/doc-json.test.ts tests/chay-tac-vu.test.ts
git commit -m "feat(tac-vu): luong chung giu-goi AI-chot/hoan credit, thu lai JSON 1 lan, ma loi theo spec"
```

---

### Task 8: Tác vụ `tom-tat` + tệp mẫu prompt từ C#

**Files:**
- Modify: `AIVanBan.Core/Services/DocumentSummaryService.cs:43,106` (`private string` → `internal static string`)
- Create: `AIVanBan.Tests/MauPromptTests.cs`
- Create (sinh ra): `vanbanplus-api/tests/mau-prompt/tom-tat.json`
- Create: `vanbanplus-api/src/lib/tac-vu/tom-tat/prompt.ts`
- Create: `vanbanplus-api/src/lib/tac-vu/tom-tat/dinh-nghia.ts`
- Create: `vanbanplus-api/src/lib/tac-vu/dang-ky.ts`
- Test: `vanbanplus-api/tests/tac-vu-tom-tat.test.ts`

**Interfaces:**
- Consumes: `DinhNghiaTacVu` (Task 7).
- Produces: `interface DauVaoTomTat { noi_dung: string; loai_van_ban: string; tieu_de: string; co_quan: string }`, `HE_THONG_TOM_TAT: string`, `dungPromptTomTat(d: DauVaoTomTat): PromptTacVu`, `tomTat: DinhNghiaTacVu<DauVaoTomTat>`, `layTacVu(ten: string): DinhNghiaTacVu<any> | null`.
- Tệp mẫu: mảng `{ dau_vao: { noi_dung, loai_van_ban, tieu_de, co_quan }, he_thong, nguoi_dung }`, xuống dòng chuẩn hóa về `\n` (C# chạy trên Windows đọc chuỗi `@"..."` với CRLF của tệp nguồn; khác biệt này không đổi nghĩa prompt).

- [ ] **Step 1: Mở hai hàm prompt C# cho bộ kiểm**

Trong `AIVanBan.Core/Services/DocumentSummaryService.cs`: dòng 43 `private string BuildSystemPrompt()` → `internal static string BuildSystemPrompt()`; dòng 106 `private string BuildUserPrompt(string content, string documentType, string title, string issuer)` → `internal static string BuildUserPrompt(string content, string documentType, string title, string issuer)`. (`AIVanBan.Core.csproj` đã có `InternalsVisibleTo AIVanBan.Tests`.) Hành vi không đổi.

- [ ] **Step 2: Viết bộ sinh tệp mẫu C#**

`AIVanBan.Tests/MauPromptTests.cs`:

```csharp
using System.Text.Encodings.Web;
using System.Text.Json;
using AIVanBan.Core.Services;

namespace AIVanBan.Tests;

/// <summary>
/// Sinh tệp mẫu prompt cho vanbanplus-api (tests/mau-prompt/*.json): đầu vào + system prompt + user prompt
/// do chính mã C# dựng. Bộ kiểm TypeScript so từng ký tự với prompt dựng ở máy chủ.
/// Xuống dòng chuẩn hóa về \n vì chuỗi @"..." mang CRLF của tệp nguồn khi build trên Windows.
/// </summary>
public class MauPromptTests
{
    private static readonly string ThuMucMau = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "vanbanplus-api", "tests", "mau-prompt"));

    private static string ChuanHoa(string s) => s.Replace("\r\n", "\n");

    private static void Ghi(string ten, object duLieu)
    {
        Directory.CreateDirectory(ThuMucMau);
        var json = JsonSerializer.Serialize(duLieu, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
        File.WriteAllText(Path.Combine(ThuMucMau, ten), json.Replace("\r\n", "\n") + "\n");
    }

    [Fact]
    public void GhiMauTomTat()
    {
        var cacMau = new[]
        {
            (NoiDung: "Điều 1. Phê duyệt kế hoạch.\nĐiều 2. Giao Văn phòng theo dõi.", Loai: "Quyết định", TieuDe: "Về việc phê duyệt kế hoạch năm 2026", CoQuan: "UBND xã Minh An"),
            (NoiDung: "Kính gửi: Các phòng chuyên môn.\nĐề nghị báo cáo trước ngày 20/11/2026.", Loai: "", TieuDe: "", CoQuan: ""),
            (NoiDung: "Nội dung có \"ngoặc kép\", dấu \\ và ký tự ```.", Loai: "Công văn", TieuDe: "", CoQuan: "Sở Nội vụ"),
        };

        var mau = cacMau.Select(m => new
        {
            dau_vao = new { noi_dung = m.NoiDung, loai_van_ban = m.Loai, tieu_de = m.TieuDe, co_quan = m.CoQuan },
            he_thong = ChuanHoa(DocumentSummaryService.BuildSystemPrompt()),
            nguoi_dung = ChuanHoa(DocumentSummaryService.BuildUserPrompt(m.NoiDung, m.Loai, m.TieuDe, m.CoQuan)),
        }).ToList();

        Ghi("tom-tat.json", mau);
        Assert.True(File.Exists(Path.Combine(ThuMucMau, "tom-tat.json")));
    }
}
```

- [ ] **Step 3: Sinh tệp mẫu**

```bash
cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests --filter "FullyQualifiedName~MauPromptTests" && head -c 400 vanbanplus-api/tests/mau-prompt/tom-tat.json
```
Expected: `Passed!`; tệp có `"dau_vao"`, `"he_thong": "Bạn là CHUYÊN GIA PHÂN TÍCH…"`.

- [ ] **Step 4: Viết bộ kiểm hỏng**

`vanbanplus-api/tests/tac-vu-tom-tat.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { tomTat } from "../src/lib/tac-vu/tom-tat/dinh-nghia";
import { layTacVu } from "../src/lib/tac-vu/dang-ky";

interface Mau {
  dau_vao: Record<string, string>;
  he_thong: string;
  nguoi_dung: string;
}
const MAU: Mau[] = JSON.parse(readFileSync(join(__dirname, "mau-prompt", "tom-tat.json"), "utf8"));

test("prompt tóm tắt khớp từng ký tự với bản C#", () => {
  assert.ok(MAU.length >= 3);
  for (const m of MAU) {
    const dv = tomTat.kiemDauVao(m.dau_vao);
    assert.ok(dv.ok, "đầu vào mẫu phải hợp lệ");
    const p = tomTat.dungPrompt(dv.du_lieu, new Date());
    assert.equal(p.he_thong, m.he_thong);
    assert.equal(p.nguoi_dung, m.nguoi_dung);
  }
});

test("đầu vào: thiếu nội dung hoặc sai kiểu thì báo lỗi; trường phụ mặc định rỗng", () => {
  assert.equal(tomTat.kiemDauVao({}).ok, false);
  assert.equal(tomTat.kiemDauVao({ noi_dung: "   " }).ok, false);
  assert.equal(tomTat.kiemDauVao({ noi_dung: "x", tieu_de: 5 }).ok, false);
  assert.equal(tomTat.kiemDauVao(null).ok, false);
  assert.deepEqual(tomTat.kiemDauVao({ noi_dung: "x" }), {
    ok: true,
    du_lieu: { noi_dung: "x", loai_van_ban: "", tieu_de: "", co_quan: "" },
  });
});

test("kết quả dùng được khi có brief không rỗng", () => {
  assert.equal(tomTat.kiemKetQua!({ brief: "Quyết định về…" }), true);
  assert.equal(tomTat.kiemKetQua!({ brief: " " }), false);
  assert.equal(tomTat.kiemKetQua!(null), false);
});

test("danh sách tác vụ: có tom-tat, không nhận tên lạ hay khóa của Object", () => {
  assert.equal(layTacVu("tom-tat"), tomTat);
  assert.equal(layTacVu("khong-co"), null);
  assert.equal(layTacVu("constructor"), null);
});
```

Chạy: `cd vanbanplus-api && npx tsx --test tests/tac-vu-tom-tat.test.ts` → FAIL (không có module).

- [ ] **Step 5: Viết mã**

`src/lib/tac-vu/tom-tat/prompt.ts` (chép nguyên văn từ `DocumentSummaryService.BuildSystemPrompt`, `""` → `"`, backtick thoát bằng `\``):

```ts
import type { PromptTacVu } from "../chay-tac-vu";

export interface DauVaoTomTat {
  noi_dung: string;
  loai_van_ban: string;
  tieu_de: string;
  co_quan: string;
}

/** Nguyên văn DocumentSummaryService.BuildSystemPrompt (bản cài máy). Sửa nội dung là việc riêng (spec mục 10). */
export const HE_THONG_TOM_TAT = `Bạn là CHUYÊN GIA PHÂN TÍCH VĂN BẢN NHÀ NƯỚC Việt Nam, có kinh nghiệm 20 năm trong lĩnh vực pháp chế và hành chính công.

NHIỆM VỤ: Tóm tắt văn bản hành chính nhà nước một cách KHOA HỌC, LOGIC, CHÍNH XÁC.

NGUYÊN TẮC TÓM TẮT:
1. TRUNG THÀNH với nội dung gốc — KHÔNG thêm thông tin, KHÔNG suy diễn
2. SẮP XẾP LOGIC theo cấu trúc văn bản: mục đích → căn cứ → nội dung chính → thực hiện → hiệu lực
3. TRÍCH XUẤT CHÍNH XÁC: số liệu, ngày tháng, tên cơ quan, căn cứ pháp lý — không làm tròn, không ước lượng
4. SỬ DỤNG THUẬT NGỮ hành chính chuẩn mực, phù hợp Nghị định 30/2020/NĐ-CP về công tác văn thư
5. PHÂN LOẠI nội dung theo Điều/Khoản/Mục nếu văn bản có cấu trúc rõ ràng
6. NẾU văn bản không rõ cấu trúc (biên bản, công văn thường), phân chia theo chủ đề logic

CẤU TRÚC PHÂN TÍCH theo 9 mục:

1. brief: Tóm tắt ngắn gọn 1-2 câu — trả lời được: "Văn bản này nói về gì? Ai ban hành? Để làm gì?"
2. document_type: Xác định chính xác loại văn bản (Công văn, Quyết định, Nghị quyết, Báo cáo, Kế hoạch, Tờ trình, Thông báo, Biên bản, Chỉ thị, Nghị định...)
3. issuing_authority: Cơ quan ban hành + người ký (nếu có)
4. target_audience: Đối tượng áp dụng hoặc nơi nhận chính
5. key_points: Các nội dung chính, mỗi mục gồm {heading, content}:
   - Với VB có Điều/Khoản: heading = "Điều 1", "Điều 2"...
   - Với VB không có Điều: heading = "Mục tiêu", "Nội dung", "Tổ chức thực hiện"...
   - Với Báo cáo: heading = "Kết quả", "Tồn tại", "Phương hướng"...
   - Với Biên bản: heading = "Nội dung 1", "Nội dung 2", "Kết luận"...
   - content: tóm gọn nhưng ĐẦY ĐỦ ý chính, giữ nguyên số liệu và tên riêng
6. legal_bases: Danh sách căn cứ pháp lý được viện dẫn (giữ nguyên số hiệu, ngày ban hành)
7. effective_dates: Các mốc thời gian quan trọng (ngày hiệu lực, thời hạn, deadline)
8. key_figures: Các con số/chỉ tiêu quan trọng (kinh phí, số lượng, tỷ lệ, diện tích...)
9. impact: Tác động/ý nghĩa thực tiễn 1-2 câu
10. notes: Lưu ý đặc biệt (nếu có: thay thế VB cũ, hiệu lực trở về trước, áp dụng thí điểm...)

TRẢ VỀ JSON THUẦN TÚY, KHÔNG dùng markdown code fence (\`\`\`json\`\`\`).

Ví dụ output:
{
  "brief": "Quyết định phê duyệt kế hoạch phát triển kinh tế - xã hội năm 2026 của UBND thành phố X",
  "document_type": "Quyết định",
  "issuing_authority": "Chủ tịch UBND thành phố X — Nguyễn Văn A",
  "target_audience": "Các sở ban ngành thuộc UBND thành phố, UBND các xã/phường",
  "key_points": [
    {"heading": "Điều 1: Mục tiêu tổng quát", "content": "Tốc độ tăng trưởng kinh tế đạt 8,5%; thu ngân sách 120 tỷ đồng"},
    {"heading": "Điều 2: Nhiệm vụ trọng tâm", "content": "5 nhiệm vụ: (1) Phát triển nông nghiệp công nghệ cao, (2) Thu hút đầu tư, (3) Đẩy mạnh cải cách hành chính, (4) Xây dựng NTM, (5) An sinh xã hội"},
    {"heading": "Điều 3: Tổ chức thực hiện", "content": "Giao Phòng TC-KH chủ trì, các phòng ban phối hợp, báo cáo hàng quý"}
  ],
  "legal_bases": [
    "Luật Tổ chức chính quyền địa phương 2015",
    "Nghị quyết số 01/NQ-HĐND ngày 15/01/2026 của HĐND thành phố"
  ],
  "effective_dates": ["Có hiệu lực kể từ ngày ký (20/01/2026)", "Báo cáo kết quả trước 30/6/2026"],
  "key_figures": ["Tăng trưởng 8,5%", "Thu ngân sách 120 tỷ đồng", "Xây dựng 3 xã NTM nâng cao"],
  "impact": "Là cơ sở pháp lý để các đơn vị triển khai kế hoạch phát triển KT-XH năm 2026, gắn trách nhiệm cụ thể cho từng đơn vị.",
  "notes": "Thay thế Quyết định số 15/QĐ-UBND ngày 18/01/2025."
}

QUY TẮC QUAN TRỌNG:
- KHÔNG bịa số liệu, ngày tháng — nếu VB không đề cập thì bỏ trống mảng []
- key_points: tối thiểu 2 mục, tối đa 8 mục — ưu tiên nội dung quan trọng nhất
- legal_bases: giữ nguyên số hiệu VB, không rút gọn
- brief: PHẢI trả lời được 3 câu hỏi: Ai? Về gì? Để làm gì?
- KHÔNG dùng markdown \`\`\`json\`\`\` bao quanh kết quả`;

export function dungPromptTomTat(d: DauVaoTomTat): PromptTacVu {
  let p = "HÃY TÓM TẮT VĂN BẢN SAU:\n\n";
  if (d.loai_van_ban.trim()) p += `Loại văn bản: ${d.loai_van_ban}\n`;
  if (d.tieu_de.trim()) p += `Tiêu đề: ${d.tieu_de}\n`;
  if (d.co_quan.trim()) p += `Cơ quan ban hành: ${d.co_quan}\n`;
  p += `\n--- NỘI DUNG VĂN BẢN ---\n${d.noi_dung}\n--- HẾT ---`;
  return { he_thong: HE_THONG_TOM_TAT, nguoi_dung: p };
}
```

`src/lib/tac-vu/tom-tat/dinh-nghia.ts`:

```ts
import type { DinhNghiaTacVu } from "../chay-tac-vu";
import { dungPromptTomTat, type DauVaoTomTat } from "./prompt";

const TRUONG_PHU = ["loai_van_ban", "tieu_de", "co_quan"] as const;

export const tomTat: DinhNghiaTacVu<DauVaoTomTat> = {
  ten: "tom-tat",
  kieu_ket_qua: "json",
  // Bản cài máy gọi /api/ai/generate không truyền temperature, máy chủ dùng 0,7.
  nhiet_do: 0.7,
  kiemDauVao(body) {
    const b = (body && typeof body === "object" ? body : {}) as Record<string, unknown>;
    if (typeof b.noi_dung !== "string" || !b.noi_dung.trim()) {
      return { ok: false, loi: "Thiếu nội dung văn bản (noi_dung)." };
    }
    for (const k of TRUONG_PHU) {
      if (b[k] != null && typeof b[k] !== "string") return { ok: false, loi: `Trường ${k} phải là chuỗi.` };
    }
    return {
      ok: true,
      du_lieu: {
        noi_dung: b.noi_dung,
        loai_van_ban: (b.loai_van_ban as string | undefined) ?? "",
        tieu_de: (b.tieu_de as string | undefined) ?? "",
        co_quan: (b.co_quan as string | undefined) ?? "",
      },
    };
  },
  dungPrompt: (d) => dungPromptTomTat(d),
  kiemKetQua: (kq) => {
    const brief = (kq as { brief?: unknown } | null)?.brief;
    return typeof brief === "string" && brief.trim() !== "";
  },
};
```

`src/lib/tac-vu/dang-ky.ts`:

```ts
import type { DinhNghiaTacVu } from "./chay-tac-vu";
import { tomTat } from "./tom-tat/dinh-nghia";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const DANH_SACH: Record<string, DinhNghiaTacVu<any>> = {
  [tomTat.ten]: tomTat,
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function layTacVu(ten: string): DinhNghiaTacVu<any> | null {
  return Object.hasOwn(DANH_SACH, ten) ? DANH_SACH[ten] : null;
}
```

- [ ] **Step 6: Chạy, thấy qua**

```bash
npx tsx --test tests/tac-vu-tom-tat.test.ts && npx tsc --noEmit -p .
```
Expected: `# pass 4`. Nếu bài so khớp hỏng: in vị trí khác đầu tiên (`for i… if a[i] !== b[i]`) và sửa `prompt.ts` cho khớp C#, **không** sửa tệp mẫu bằng tay.

- [ ] **Step 7: Commit (hai repo)**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git add src/lib/tac-vu/tom-tat src/lib/tac-vu/dang-ky.ts tests/tac-vu-tom-tat.test.ts tests/mau-prompt/tom-tat.json && git commit -m "feat(tac-vu): tom-tat (prompt chep tu C#, so khop tung ky tu voi tep mau)"
cd /d/AIVanBanCaNhan && git add AIVanBan.Core/Services/DocumentSummaryService.cs AIVanBan.Tests/MauPromptTests.cs && git commit -m "test: sinh tep mau prompt tom-tat cho vanbanplus-api (ham prompt -> internal static)"
```

---

### Task 9: Đường `/api/v2/tac-vu/*` và `/api/v2/credit`

**Files:**
- Create: `vanbanplus-api/src/lib/tac-vu/xac-thuc.ts`, `tra-loi.ts`, `phu-thuoc.ts`
- Create: `vanbanplus-api/src/app/api/v2/tac-vu/[ten]/route.ts`
- Create: `vanbanplus-api/src/app/api/v2/tac-vu/[ten]/uoc-tinh/route.ts`
- Create: `vanbanplus-api/src/app/api/v2/credit/route.ts`
- Modify: `vanbanplus-api/src/proxy.ts` (cho phép header `X-VBP-Nguon`, `X-VBP-Phien-Ban`)

**Interfaces:**
- Consumes: `chayTacVu`, `uocTinhTacVu`, `Loi`, `PhuThuoc` (Task 7); `layTacVu` (Task 8); `docBangGia` (Task 6); `khoCreditSupabase`, `docSoDu` (Task 6); `resolveAIKey` (Task 6); `callAI`.
- Produces: `xacThuc(req): Promise<ApiUser | null>` (bước 2 thêm token web/app vào đây); `traLoi(l: Loi): NextResponse`; `phuThuocThat: PhuThuoc`. Header người gọi gửi: `X-API-Key`, `X-VBP-Nguon` (vd `desktop`, `web`), `X-VBP-Phien-Ban`.

- [ ] **Step 1: Viết mã**

`src/lib/tac-vu/xac-thuc.ts`:

```ts
import type { NextRequest } from "next/server";
import { validateApiKey } from "../auth";
import type { ApiUser } from "@/types";

/** Một chỗ xác thực cho v2. Bước 2 (web/app) thêm đăng nhập bằng token ở đây. */
export function xacThuc(req: NextRequest): Promise<ApiUser | null> {
  return validateApiKey(req);
}
```

`src/lib/tac-vu/tra-loi.ts`:

```ts
import { NextResponse } from "next/server";
import type { Loi } from "./chay-tac-vu";

export function traLoi(l: Loi): NextResponse {
  return NextResponse.json(
    { success: false, message: l.message, code: l.code },
    { status: l.status, headers: l.retryAfter ? { "Retry-After": String(l.retryAfter) } : undefined }
  );
}
```

`src/lib/tac-vu/phu-thuoc.ts`:

```ts
import { callAI } from "../ai-providers";
import { resolveAIKey } from "../ai-keys";
import { docBangGia } from "../credit/bang-gia";
import { khoCreditSupabase } from "../credit/so-du";
import type { PhuThuoc } from "./chay-tac-vu";

export const phuThuocThat: PhuThuoc = {
  bangGia: docBangGia,
  kho: khoCreditSupabase,
  layKhoa: (userId) => resolveAIKey(userId, "gemini", { chiHeThong: true }),
  goiAI: (contents, opts) => callAI(contents as any[], opts),
  bayGio: () => new Date(),
};
```

`src/app/api/v2/tac-vu/[ten]/route.ts`:

```ts
import { NextRequest } from "next/server";
import { getClientIp } from "@lib/auth";
import { notFound, ok, unauthorized } from "@lib/response";
import { chayTacVu } from "@lib/tac-vu/chay-tac-vu";
import { layTacVu } from "@lib/tac-vu/dang-ky";
import { phuThuocThat } from "@lib/tac-vu/phu-thuoc";
import { traLoi } from "@lib/tac-vu/tra-loi";
import { xacThuc } from "@lib/tac-vu/xac-thuc";

export const dynamic = "force-dynamic";
export const maxDuration = 300;

/** POST /api/v2/tac-vu/<tên> — chạy tác vụ AI, trừ credit (spec mục 6). */
export async function POST(req: NextRequest, { params }: { params: Promise<{ ten: string }> }) {
  const { ten } = await params;
  const dn = layTacVu(ten);
  if (!dn) return notFound(`Tác vụ ${ten} không tồn tại.`);

  const user = await xacThuc(req);
  if (!user) return unauthorized();

  const body = await req.json().catch(() => null);
  const kq = await chayTacVu(
    dn,
    body,
    {
      user,
      clientIp: getClientIp(req),
      nguonGoi: req.headers.get("x-vbp-nguon") ?? "khong-ro",
      phienBan: req.headers.get("x-vbp-phien-ban"),
      signal: req.signal,
    },
    phuThuocThat
  );
  if (!kq.ok) return traLoi(kq);
  return ok({ ket_qua: kq.ket_qua, credit_da_tru: kq.credit_da_tru, so_du: kq.so_du, ma_luot: kq.ma_luot, bi_cat: kq.bi_cat });
}
```

`src/app/api/v2/tac-vu/[ten]/uoc-tinh/route.ts`:

```ts
import { NextRequest } from "next/server";
import { notFound, ok, unauthorized } from "@lib/response";
import { uocTinhTacVu } from "@lib/tac-vu/chay-tac-vu";
import { layTacVu } from "@lib/tac-vu/dang-ky";
import { phuThuocThat } from "@lib/tac-vu/phu-thuoc";
import { traLoi } from "@lib/tac-vu/tra-loi";
import { xacThuc } from "@lib/tac-vu/xac-thuc";

export const dynamic = "force-dynamic";

/** POST /api/v2/tac-vu/<tên>/uoc-tinh — khoảng credit, không gọi AI, không giữ credit. */
export async function POST(req: NextRequest, { params }: { params: Promise<{ ten: string }> }) {
  const { ten } = await params;
  const dn = layTacVu(ten);
  if (!dn) return notFound(`Tác vụ ${ten} không tồn tại.`);

  const user = await xacThuc(req);
  if (!user) return unauthorized();

  const kq = await uocTinhTacVu(dn, await req.json().catch(() => null), user, phuThuocThat);
  if (!kq.ok) return traLoi(kq);
  return ok({ credit_min: kq.credit_min, credit_max: kq.credit_max, so_du: kq.so_du, du_credit: kq.du_credit });
}
```

`src/app/api/v2/credit/route.ts`:

```ts
import { NextRequest } from "next/server";
import { docSoDu } from "@lib/credit/so-du";
import { fail, ok, unauthorized } from "@lib/response";
import { xacThuc } from "@lib/tac-vu/xac-thuc";

export const dynamic = "force-dynamic";

/** GET /api/v2/credit — tổng số dư, các lô còn hạn, ngày hết hạn gần nhất. */
export async function GET(req: NextRequest) {
  const user = await xacThuc(req);
  if (!user) return unauthorized();
  try {
    return ok(await docSoDu(user.id));
  } catch (e) {
    console.error("[CREDIT] Không đọc được số dư", e);
    return fail("Không đọc được số dư credit.", 500);
  }
}
```

`src/proxy.ts`: ở cả hai chỗ đặt `Access-Control-Allow-Headers`, đổi giá trị thành `"Content-Type,X-API-Key,Authorization,X-VBP-Nguon,X-VBP-Phien-Ban"`.

- [ ] **Step 2: Kiểm kiểu + build**

```bash
npx tsc --noEmit -p . && npx next build
```
Expected: build xong, danh sách route có `/api/v2/tac-vu/[ten]`, `/api/v2/tac-vu/[ten]/uoc-tinh`, `/api/v2/credit`.

- [ ] **Step 3: Thử nhanh không cần CSDL**

```bash
npx next start -p 3999 &
sleep 5
curl -s -o /dev/null -w "%{http_code}\n" -X POST http://localhost:3999/api/v2/tac-vu/khong-co   # 404
curl -s -o /dev/null -w "%{http_code}\n" -X POST http://localhost:3999/api/v2/tac-vu/tom-tat    # 401
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:3999/api/v2/credit                    # 401
kill %1
```
Expected: `404`, `401`, `401`. (`.env` cục bộ trỏ CSDL cũ; có API key thì các lời gọi sẽ đi CSDL đó, nên chỉ thử không key.)

- [ ] **Step 4: Commit**

```bash
git add src/lib/tac-vu/xac-thuc.ts src/lib/tac-vu/tra-loi.ts src/lib/tac-vu/phu-thuoc.ts src/app/api/v2 src/proxy.ts
git commit -m "feat(api): /api/v2/tac-vu/[ten], /uoc-tinh, /api/v2/credit"
```

---

### Task 10: Đường cũ `/api/ai/*` trừ credit; `/api/auth/me` trả số dư

**Files:**
- Create: `vanbanplus-api/src/lib/credit/api-cu.ts`
- Modify: `vanbanplus-api/src/app/api/ai/[action]/route.ts`
- Modify: `vanbanplus-api/src/app/api/auth/[action]/route.ts:103-125` (`handleMe`)
- Test: `vanbanplus-api/tests/api-cu.test.ts`

**Interfaces:**
- Consumes: `docBangGia`, `docSoDu`, `truCreditCu` (Task 6); `creditTuToken`, `giaCua`, `BangGia` (Task 5).
- Produces: `creditApiCu(tokenVao, tokenRa, model, bg): number`, `kiemCreditCu(user, khoa): Promise<{ ok: true } | { ok: false; message: string }>`, `truCreditSauGoi(user, khoa, endpoint, r, phienBan): Promise<void>` (không bao giờ ném lỗi).
- Luật (spec mục 7 «Đường cũ»): khóa riêng của người dùng (`source === "user"`) thì không kiểm, không trừ. Khóa hệ thống/env: trước khi gọi cần số dư ≥ 1; sau khi gọi thành công trừ theo token thật (tối thiểu `api-cu.toi_thieu`), tối đa bằng số dư. Thông báo hết credit vẫn dùng mã `QUOTA_EXCEEDED` + HTTP 429 như cũ để bản cài máy cũ hiểu.

- [ ] **Step 1: Viết bộ kiểm hỏng**

`tests/api-cu.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { creditApiCu } from "../src/lib/credit/api-cu";
import type { BangGia } from "../src/lib/credit/uoc-tinh";

const BG: BangGia = {
  chung: { gia_tri_credit_vnd: 100, ty_gia_vnd_usd: 26000, mo_hinh_mac_dinh: "gemini-3.6-flash" },
  mo_hinh: {
    "gemini-3.6-flash": { vao_usd_trieu: 1.5, ra_usd_trieu: 7.5 },
    "gemini-3.5-flash": { vao_usd_trieu: 1.5, ra_usd_trieu: 9 },
  },
  tac_vu: { "api-cu": { toi_thieu: 1, ra_min_he_so: 0, ra_min_hang: 0, ra_max_he_so: 0, ra_max_hang: 0, toi_da_ky_tu: 0 } },
};

test("đường cũ tính theo giá của mô hình đã gọi", () => {
  assert.equal(creditApiCu(4000, 3000, "gemini-3.5-flash", BG), 9); // 858đ
});

test("mô hình không có trong bảng giá dùng giá mặc định", () => {
  assert.equal(creditApiCu(4000, 3000, "gemini-2.5-flash", BG), 8); // 741đ
});

test("lượt rất nhỏ vẫn trừ tối thiểu", () => {
  assert.equal(creditApiCu(10, 10, "gemini-3.5-flash", BG), 1);
});
```

- [ ] **Step 2: Chạy, thấy hỏng** — `npx tsx --test tests/api-cu.test.ts` → FAIL.

- [ ] **Step 3: Viết mã**

`src/lib/credit/api-cu.ts`:

```ts
import type { AICallResult, ApiUser, ResolvedAIKey } from "@/types";
import { docBangGia } from "./bang-gia";
import { docSoDu, truCreditCu } from "./so-du";
import { creditTuToken, giaCua, type BangGia } from "./uoc-tinh";

/** Credit cho một lượt /api/ai/* theo token thật. */
export function creditApiCu(tokenVao: number, tokenRa: number, model: string, bg: BangGia): number {
  const toiThieu = bg.tac_vu["api-cu"]?.toi_thieu ?? 1;
  return Math.max(toiThieu, creditTuToken(tokenVao, tokenRa, giaCua(bg, model), bg.chung));
}

/** Trước khi gọi AI bằng khóa hệ thống: còn ít nhất 1 credit. Khóa riêng của người dùng thì luôn cho qua. */
export async function kiemCreditCu(
  user: ApiUser,
  khoa: ResolvedAIKey
): Promise<{ ok: true } | { ok: false; message: string }> {
  if (khoa.source === "user") return { ok: true };
  const sd = await docSoDu(user.id);
  if (sd.tong >= 1) return { ok: true };
  return { ok: false, message: "Đã hết credit AI. Mua thêm gói hoặc chờ kỳ mới để tiếp tục." };
}

/** Sau khi gọi AI thành công bằng khóa hệ thống: trừ theo token thật, tối đa bằng số dư. Lỗi chỉ ghi log. */
export async function truCreditSauGoi(
  user: ApiUser,
  khoa: ResolvedAIKey,
  endpoint: string,
  r: AICallResult,
  phienBan: string | null
): Promise<void> {
  if (khoa.source === "user" || !r.success) return;
  try {
    const bg = await docBangGia();
    const tokenVao = r.promptTokens ?? 0;
    const tokenRa = (r.completionTokens ?? 0) + (r.thoughtsTokens ?? 0);
    await truCreditCu({
      user_id: user.id,
      tac_vu: endpoint,
      so: creditApiCu(tokenVao, tokenRa, khoa.model, bg),
      provider: khoa.provider,
      model: khoa.model,
      token_vao: tokenVao,
      token_ra: tokenRa,
      phien_ban: phienBan,
    });
  } catch (e) {
    console.error(`[CREDIT] Không trừ được credit ${endpoint} của ${user.id}`, e);
  }
}
```

`src/app/api/ai/[action]/route.ts`:

1. Dòng 3: `import { checkQuota } from "@lib/usage";` → `import { kiemCreditCu, truCreditSauGoi } from "@lib/credit/api-cu";`
2. `handleGenerate`: xóa hai dòng `const quota = await checkQuota(user);` / `if (!quota.allowed) return quotaExceeded(quota.error!);`. Ngay sau dòng `if (!resolvedKey) return fail(...)` thêm:

```ts
  const credit = await kiemCreditCu(user, resolvedKey);
  if (!credit.ok) return quotaExceeded(credit.message);
```
   Ngay sau khối `if (!result.success) { ... }` thêm:

```ts
  await truCreditSauGoi(user, resolvedKey, "/api/ai/generate", result, req.headers.get("x-vbp-phien-ban"));
```
3. `handleExtract`: tương tự — xóa hai dòng `checkQuota`; sau `if (!resolvedKey) return fail("Không tìm thấy API key. Liên hệ admin.", 500);` thêm hai dòng `kiemCreditCu` như trên; sau `if (!result.success) return fail(result.error ?? "Lỗi trích xuất", 500);` thêm `await truCreditSauGoi(user, resolvedKey, "/api/ai/extract", result, req.headers.get("x-vbp-phien-ban"));`
4. `handleReadText`: tương tự với `"/api/ai/read-text"`, chèn sau `if (!result.success) return fail(result.error ?? "Lỗi đọc text", 500);`.

`checkQuota` trong `src/lib/usage.ts` không còn ai dùng: xóa hàm `checkQuota` (giữ `effectivePlanId` vì `getUsageSummary` dùng).

`src/app/api/auth/[action]/route.ts`, `handleMe`: thêm import `import { docSoDu } from "@lib/credit/so-du";` và trong object trả về thêm trường (giữ nguyên các trường cũ vì bản cài máy cũ đọc chúng):

```ts
    credit: await docSoDu(user.id).catch(() => null),
```

- [ ] **Step 4: Chạy bộ kiểm + build**

```bash
npx tsx --test tests/api-cu.test.ts && npm test && npx tsc --noEmit -p . && npx next build
```
Expected: `api-cu` pass 3; toàn bộ `npm test` pass (bộ CSDL skip nếu không đặt biến); build xong.

- [ ] **Step 5: Commit**

```bash
git add src/lib/credit/api-cu.ts tests/api-cu.test.ts "src/app/api/ai/[action]/route.ts" "src/app/api/auth/[action]/route.ts" src/lib/usage.ts
git commit -m "feat(credit): /api/ai/* kiem va tru credit theo token that (khoa rieng khong tru); /api/auth/me tra so du"
```

---

### Task 11: Giao diện: tài khoản, bảng giá, đăng ký

**Files:**
- Modify: `vanbanplus-api/src/app/tai-khoan/page.tsx` (interface `UsageSummary` dòng ~17, khối «Usage overview» dòng ~247–300)
- Modify: `vanbanplus-api/src/app/bang-gia/BangGiaClient.tsx:16,37,145,214,262-263`
- Modify: `vanbanplus-api/src/app/dang-ky/DangKyClient.tsx` (interface gói + dòng 182)

**Interfaces:**
- Consumes: `/api/auth/me` trả thêm `credit: { tong, lo: [{ nguon, so_cap, so_con, het_han }], het_han_gan_nhat } | null` (Task 10); `/api/plans` trả thêm `credit_thang` (migration Task 2).

- [ ] **Step 1: Trang tài khoản**

Trong `src/app/tai-khoan/page.tsx`:

1. Trong `interface UsageSummary` thêm:

```ts
  credit?: {
    tong: number;
    lo: { nguon: string; so_cap: number; so_con: number; het_han: string }[];
    het_han_gan_nhat: string | null;
  } | null;
```
2. Ngay trên `export default function` (cấp tệp) thêm:

```ts
const NGUON_CREDIT: Record<string, string> = {
  goi_thang: "Gói tháng",
  nap: "Gói nạp",
  tang: "Tặng",
  dieu_chinh: "Điều chỉnh",
};
```
3. Thay toàn bộ khối từ `{/* Usage overview */}` tới hết thẻ `</div>` đóng khối đó (ngay trước `{/* ☁️ Cloud Sync Status */}`) bằng:

```tsx
              {/* Credit AI */}
              <div className="bg-white rounded-2xl border border-gray-200 p-6">
                <h3 className="font-bold text-gray-900 mb-4">🪙 Credit AI</h3>

                {loading ? (
                  <div className="flex justify-center py-8">
                    <div className="animate-spin h-6 w-6 border-3 border-primary-500 border-t-transparent rounded-full" />
                  </div>
                ) : usage?.credit ? (
                  <div className="space-y-4">
                    <div className="flex items-baseline gap-2">
                      <span className="text-3xl font-bold text-gray-900">{formatNumber(usage.credit.tong)}</span>
                      <span className="text-sm text-gray-500">credit còn dùng được</span>
                    </div>
                    {usage.credit.lo.length > 0 && (
                      <ul className="divide-y divide-gray-100 text-sm">
                        {usage.credit.lo.map((lo, i) => (
                          <li key={i} className="flex justify-between py-2">
                            <span className="text-gray-600">{NGUON_CREDIT[lo.nguon] ?? lo.nguon}</span>
                            <span className="text-gray-900">
                              {formatNumber(lo.so_con)} / {formatNumber(lo.so_cap)} · hết hạn {formatDate(lo.het_han)}
                            </span>
                          </li>
                        ))}
                      </ul>
                    )}
                    <p className="text-xs text-gray-500">
                      Mỗi tác vụ AI trừ credit theo độ dài văn bản và kết quả, thường 3–15 credit. Lỗi thì không trừ.
                      Credit của gói tháng không cộng dồn sang tháng sau.
                    </p>
                  </div>
                ) : (
                  <p className="text-sm text-gray-500 py-4">Không thể tải số dư credit.</p>
                )}
              </div>
```
4. Kiểm phần import đầu tệp: `formatNumber` và `formatDate` phải được import từ `@/utils/format` (thêm vào import có sẵn nếu thiếu). Nếu `formatTokens` không còn dùng ở đâu khác thì bỏ khỏi import để build không cảnh báo.
5. Dòng ~423 `Thêm lượt AI, mở khóa tính năng` → `Thêm credit AI, mở khóa tính năng`.

- [ ] **Step 2: Bảng giá**

`src/app/bang-gia/BangGiaClient.tsx`:
- dòng 16 (trong interface gói), thêm dòng sau `max_requests_per_month: number;`: `credit_thang: number;`
- dòng 37: `features.push(\`${formatNumber(plan.max_requests_per_month)} lượt soạn AI/tháng\`);` → `features.push(\`${formatNumber(plan.credit_thang)} credit AI/tháng\`);`
- dòng 145: `Bắt đầu miễn phí với 20 lượt AI mỗi tháng. Nâng cấp khi cần, không tự động trừ tiền.` → `Bắt đầu miễn phí với 30 credit AI mỗi tháng. Nâng cấp khi cần, không tự động trừ tiền.`
- dòng 214: `{formatNumber(plan.max_requests_per_month)} lượt • {formatTokens(plan.max_tokens_per_month)} tokens/tháng` → `{formatNumber(plan.credit_thang)} credit/tháng` (bỏ `formatTokens` khỏi import nếu không còn dùng).
- dòng 262–263, thay hai câu hỏi:

```tsx
                { q: "Credit là gì?", a: "Mỗi tác vụ AI (soạn, kiểm tra, tóm tắt, đọc ảnh) trừ credit theo độ dài văn bản và kết quả, thường 3–15 credit một lần. Lỗi thì không trừ. Tra cứu pháp quy và quản lý văn bản không trừ credit." },
                { q: "Gói Miễn phí có mất phí không?", a: "Không. Mỗi tháng có 30 credit, không cần thẻ." },
```

- [ ] **Step 3: Trang đăng ký**

`src/app/dang-ky/DangKyClient.tsx`: trong interface gói thêm `credit_thang: number;`; dòng 182 `{formatNumber(p.max_requests_per_month)} lượt AI/tháng` → `{formatNumber(p.credit_thang)} credit AI/tháng`.

- [ ] **Step 4: Soát lời + build**

```bash
grep -nE "\b([Tt]ôi|[Bb]ạn|[Mm]ình|[Dd]ạ|[Vv]âng)\b|ạ[.,!? ]" src/app/tai-khoan/page.tsx src/app/bang-gia/BangGiaClient.tsx src/app/dang-ky/DangKyClient.tsx src/lib/tac-vu src/lib/credit -r
npx next build
```
Expected: grep không ra dòng mới do đợt này thêm (dòng cũ có sẵn thì ghi lại, không sửa ngoài phạm vi); build xong.

- [ ] **Step 5: Commit**

```bash
git add src/app/tai-khoan/page.tsx src/app/bang-gia/BangGiaClient.tsx src/app/dang-ky/DangKyClient.tsx
git commit -m "feat(ui): tai khoan hien so du credit theo lo; bang gia, dang ky hien credit/thang"
```

---

### Task 12: Trang quản trị credit

**Files:**
- Create: `vanbanplus-api/src/app/api/admin/credit/route.ts`
- Create: `vanbanplus-api/src/app/admin/credit/page.tsx`
- Modify: `vanbanplus-api/src/utils/constants.ts:14` (thêm mục menu)

**Interfaces:**
- Consumes: `dungBangGia`, `xoaBoNhoBangGia`, `DongBangGia` (Task 6); `dieuChinhCredit` (Task 6).
- Produces: `GET /api/admin/credit` → `{ bang_gia: DongBangGia&{sua_luc, sua_boi}[], luot: [...] }`; `POST /api/admin/credit` với `{ action: "sua_bang_gia", loai, ma, gia_tri }` hoặc `{ action: "dieu_chinh", email, so, ghi_chu }`.

- [ ] **Step 1: API**

`src/app/api/admin/credit/route.ts`:

```ts
import { NextRequest } from "next/server";
import { validateApiKey } from "@lib/auth";
import { dungBangGia, xoaBoNhoBangGia, type DongBangGia } from "@lib/credit/bang-gia";
import { dieuChinhCredit } from "@lib/credit/so-du";
import { fail, forbidden, ok, unauthorized } from "@lib/response";
import { getSupabase } from "@lib/supabase";

export const dynamic = "force-dynamic";

const LOAI = ["tac_vu", "mo_hinh", "goi_nap", "cau_hinh"];

async function requireAdmin(req: NextRequest) {
  const admin = await validateApiKey(req);
  if (!admin) return { error: unauthorized() };
  if (admin.role !== "admin") return { error: forbidden() };
  return { admin };
}

/** GET /api/admin/credit — bảng giá + 100 lượt chạy gần nhất (không có nội dung văn bản). */
export async function GET(req: NextRequest) {
  const { error } = await requireAdmin(req);
  if (error) return error;

  const sb = getSupabase();
  const [{ data: bang_gia }, { data: luot }] = await Promise.all([
    sb.from("credit_bang_gia").select("loai, ma, gia_tri, sua_luc, sua_boi").order("loai").order("ma"),
    sb.from("tac_vu_chay")
      .select("id, tac_vu, trang_thai, credit_min, credit_max, credit_tru, model, token_vao, token_ra, bi_cat, ma_loi, nguon_goi, phien_ban_ung_dung, bat_dau_luc, xong_luc, users(email)")
      .order("bat_dau_luc", { ascending: false })
      .limit(100),
  ]);
  return ok({ bang_gia: bang_gia ?? [], luot: luot ?? [] });
}

/** POST /api/admin/credit — sửa một dòng bảng giá, hoặc cộng/trừ credit tay. */
export async function POST(req: NextRequest) {
  const { admin, error } = await requireAdmin(req);
  if (error) return error;

  const body = await req.json().catch(() => ({}));
  const sb = getSupabase();

  if (body.action === "sua_bang_gia") {
    const { loai, ma, gia_tri } = body;
    if (!LOAI.includes(loai) || typeof ma !== "string" || !ma.trim()) return fail("Sai loại hoặc mã.");
    if (!gia_tri || typeof gia_tri !== "object" || Array.isArray(gia_tri)) return fail("Giá trị phải là một đối tượng JSON.");

    const { data: hienCo, error: docLoi } = await sb.from("credit_bang_gia").select("loai, ma, gia_tri");
    if (docLoi) return fail("Không đọc được bảng giá.", 500);
    const sau = ((hienCo ?? []) as DongBangGia[]).filter((d) => !(d.loai === loai && d.ma === ma));
    sau.push({ loai, ma, gia_tri });
    try {
      dungBangGia(sau);
    } catch (e) {
      return fail((e as Error).message);
    }

    const { error: ghiLoi } = await sb.from("credit_bang_gia").upsert({
      loai, ma, gia_tri, sua_luc: new Date().toISOString(), sua_boi: admin.email,
    });
    if (ghiLoi) return fail("Không lưu được bảng giá.", 500);
    xoaBoNhoBangGia();
    return ok({ loai, ma }, "Đã lưu. Máy chủ dùng giá mới trong vòng 1 phút.");
  }

  if (body.action === "dieu_chinh") {
    const so = Number(body.so);
    const ghiChu = typeof body.ghi_chu === "string" ? body.ghi_chu.trim() : "";
    const email = typeof body.email === "string" ? body.email.trim() : "";
    if (!Number.isInteger(so) || so === 0 || Math.abs(so) > 100_000) return fail("Số credit phải là số nguyên khác 0, tối đa 100.000.");
    if (!ghiChu) return fail("Cần ghi lý do điều chỉnh.");
    const { data: u } = await sb.from("users").select("id").eq("email", email).maybeSingle();
    if (!u) return fail("Không tìm thấy tài khoản có email này.");
    try {
      const kq = await dieuChinhCredit(u.id, so, ghiChu, admin.email);
      return ok(kq, `Đã điều chỉnh ${kq.da_dieu_chinh} credit. Số dư còn ${kq.so_du}.`);
    } catch (e) {
      console.error("[ADMIN CREDIT] điều chỉnh lỗi", e);
      return fail("Không điều chỉnh được credit.", 500);
    }
  }

  return fail("Thao tác không hợp lệ.");
}
```

- [ ] **Step 2: Trang**

`src/app/admin/credit/page.tsx`:

```tsx
"use client";

import { useCallback, useEffect, useState } from "react";
import PageHeader from "@/components/layout/PageHeader";
import api from "@/utils/api-client";
import { formatDate, formatNumber } from "@/utils/format";

interface DongGia {
  loai: string;
  ma: string;
  gia_tri: Record<string, unknown>;
  sua_luc: string;
  sua_boi: string | null;
}

interface Luot {
  id: string;
  tac_vu: string;
  trang_thai: "dang_chay" | "xong" | "loi";
  credit_min: number;
  credit_max: number;
  credit_tru: number;
  model: string | null;
  token_vao: number | null;
  token_ra: number | null;
  bi_cat: boolean;
  ma_loi: string | null;
  nguon_goi: string | null;
  phien_ban_ung_dung: string | null;
  bat_dau_luc: string;
  xong_luc: string | null;
  users: { email: string } | null;
}

const TRANG_THAI: Record<Luot["trang_thai"], string> = { dang_chay: "Đang chạy", xong: "Xong", loi: "Lỗi" };

export default function AdminCreditPage() {
  const [bangGia, setBangGia] = useState<DongGia[]>([]);
  const [nhap, setNhap] = useState<Record<string, string>>({});
  const [luot, setLuot] = useState<Luot[]>([]);
  const [message, setMessage] = useState<string | null>(null);
  const [dc, setDc] = useState({ email: "", so: "", ghi_chu: "" });

  const load = useCallback(async () => {
    const res = await api.get<{ bang_gia: DongGia[]; luot: Luot[] }>("/api/admin/credit");
    if (res.success && res.data) {
      setBangGia(res.data.bang_gia);
      setNhap(Object.fromEntries(res.data.bang_gia.map((d) => [`${d.loai}/${d.ma}`, JSON.stringify(d.gia_tri, null, 2)])));
      setLuot(res.data.luot);
    } else {
      setMessage(res.message);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  const luuGia = async (d: DongGia) => {
    let gia_tri: unknown;
    try {
      gia_tri = JSON.parse(nhap[`${d.loai}/${d.ma}`] ?? "");
    } catch {
      setMessage(`${d.loai}/${d.ma}: JSON không hợp lệ.`);
      return;
    }
    const res = await api.post("/api/admin/credit", { action: "sua_bang_gia", loai: d.loai, ma: d.ma, gia_tri });
    setMessage(res.message);
    if (res.success) load();
  };

  const dieuChinh = async () => {
    const so = Number(dc.so);
    if (!window.confirm(`${so > 0 ? "Cộng" : "Trừ"} ${Math.abs(so)} credit cho ${dc.email}?`)) return;
    const res = await api.post("/api/admin/credit", { action: "dieu_chinh", email: dc.email, so, ghi_chu: dc.ghi_chu });
    setMessage(res.message);
    if (res.success) setDc({ email: "", so: "", ghi_chu: "" });
    load();
  };

  return (
    <div>
      <PageHeader title="Credit" description="Bảng giá credit, điều chỉnh tay và các lượt chạy AI gần đây" />
      {message && <p className="mb-4 text-sm text-gray-700">{message}</p>}

      <section className="bg-white rounded-xl border border-gray-200 p-4 mb-6">
        <h2 className="font-semibold text-gray-900 mb-3">Cộng / trừ credit</h2>
        <div className="grid gap-3 md:grid-cols-4">
          <input className="border rounded-lg px-3 py-2 text-sm" placeholder="Email tài khoản" value={dc.email}
            onChange={(e) => setDc({ ...dc, email: e.target.value })} />
          <input className="border rounded-lg px-3 py-2 text-sm" placeholder="Số credit (âm để trừ)" value={dc.so}
            onChange={(e) => setDc({ ...dc, so: e.target.value })} />
          <input className="border rounded-lg px-3 py-2 text-sm" placeholder="Lý do" value={dc.ghi_chu}
            onChange={(e) => setDc({ ...dc, ghi_chu: e.target.value })} />
          <button className="bg-primary-600 text-white rounded-lg px-3 py-2 text-sm disabled:opacity-50"
            disabled={!dc.email || !dc.so || !dc.ghi_chu} onClick={dieuChinh}>Ghi điều chỉnh</button>
        </div>
        <p className="text-xs text-gray-500 mt-2">Cộng: tạo lô mới hạn 90 ngày. Trừ: lấy từ lô hết hạn sớm nhất, không làm âm số dư.</p>
      </section>

      <section className="bg-white rounded-xl border border-gray-200 p-4 mb-6">
        <h2 className="font-semibold text-gray-900 mb-3">Bảng giá</h2>
        <div className="grid gap-4 md:grid-cols-2">
          {bangGia.map((d) => (
            <div key={`${d.loai}/${d.ma}`} className="border border-gray-100 rounded-lg p-3">
              <div className="flex justify-between text-sm mb-2">
                <span className="font-mono">{d.loai}/{d.ma}</span>
                <span className="text-gray-400">{d.sua_boi ?? "mặc định"} · {formatDate(d.sua_luc)}</span>
              </div>
              <textarea className="w-full font-mono text-xs border rounded p-2 h-28"
                value={nhap[`${d.loai}/${d.ma}`] ?? ""}
                onChange={(e) => setNhap({ ...nhap, [`${d.loai}/${d.ma}`]: e.target.value })} />
              <button className="mt-2 text-sm text-primary-600 hover:underline" onClick={() => luuGia(d)}>Lưu</button>
            </div>
          ))}
        </div>
      </section>

      <section className="bg-white rounded-xl border border-gray-200 overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 text-left text-gray-500">
            <tr>
              <th className="px-4 py-3">Lúc</th>
              <th className="px-4 py-3">Tài khoản</th>
              <th className="px-4 py-3">Tác vụ</th>
              <th className="px-4 py-3">Trạng thái</th>
              <th className="px-4 py-3 text-right">Credit (giữ → trừ)</th>
              <th className="px-4 py-3 text-right">Token vào / ra</th>
              <th className="px-4 py-3">Mô hình · nguồn</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {luot.length === 0 && (
              <tr><td colSpan={7} className="px-4 py-8 text-center text-gray-400">Chưa có lượt nào</td></tr>
            )}
            {luot.map((l) => (
              <tr key={l.id}>
                <td className="px-4 py-3 whitespace-nowrap">{formatDate(l.bat_dau_luc)}</td>
                <td className="px-4 py-3">{l.users?.email}</td>
                <td className="px-4 py-3 font-mono">{l.tac_vu}</td>
                <td className="px-4 py-3">
                  {TRANG_THAI[l.trang_thai]}
                  {l.ma_loi && <span className="text-red-500"> {l.ma_loi}</span>}
                  {l.bi_cat && <span className="text-amber-600"> bị cắt</span>}
                </td>
                <td className="px-4 py-3 text-right">{l.credit_max} → {l.credit_tru}</td>
                <td className="px-4 py-3 text-right">{formatNumber(l.token_vao ?? 0)} / {formatNumber(l.token_ra ?? 0)}</td>
                <td className="px-4 py-3 text-gray-500">{l.model} · {l.nguon_goi}{l.phien_ban_ung_dung ? ` ${l.phien_ban_ung_dung}` : ""}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  );
}
```

`src/utils/constants.ts`: sau dòng `{ id: "orders", label: "Đơn hàng", icon: "🧾", href: "/admin/orders" },` thêm:

```ts
  { id: "credit", label: "Credit", icon: "🪙", href: "/admin/credit" },
```

(Kiểm `formatDate` trong `src/utils/format.ts` nhận chuỗi ISO; nếu chữ ký khác thì dùng như `src/app/admin/orders/page.tsx` đang dùng.)

- [ ] **Step 3: Build**

Run: `npx tsc --noEmit -p . && npx next build` — Expected: build xong, có route `/admin/credit`, `/api/admin/credit`.

- [ ] **Step 4: Commit**

```bash
git add src/app/api/admin/credit src/app/admin/credit src/utils/constants.ts
git commit -m "feat(admin): trang credit - sua bang gia, cong/tru tay, xem luot chay"
```

---

### Task 13: Thử trên bản sao prod, migrate prod, đẩy `main`, thử thật

**Files:**
- Modify: `D:\AIVanBanCaNhan\PROJECT_STATUS.md`, `docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md` (dòng trạng thái)

**Điểm dừng:** trước Step 3 (migrate prod), báo chủ dự án một dòng: «Sắp migrate CSDL prod VanBanPlus (thêm 4 bảng credit) và đẩy main; từ lúc đó bản cài máy dùng khóa hệ thống bị trừ credit (free 30/tháng)». Làm tiếp khi đã được đồng ý (lệnh «làm đi» cho cả kế hoạch được tính là đồng ý, trừ khi chủ dự án dặn khác).

- [ ] **Step 1: Bộ kiểm đầy đủ + build trên nhánh**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npm test && npx next build
```
Expected: mọi bộ kiểm pass, kể cả `credit-csdl` (16 bài) và `csdl-thu`; build xong.

- [ ] **Step 2: Thử migration trên bản sao prod**

```bash
scp -q supabase/migrations/004_credit.sql gikidi-dev:/tmp/004_credit.sql
ssh gikidi-dev "set -e
  PROD=supabase-db-q323olywghtnjcvpyvtjzo9v
  sudo docker exec \$PROD pg_dump -U postgres -d postgres -n public --no-owner > /tmp/vbp-ban-sao.sql
  export PGPASSWORD=thu; P='psql -h 127.0.0.1 -p 55990 -U postgres -q'
  \$P -d postgres -c 'drop database if exists vbp_ban_sao with (force)' -c 'create database vbp_ban_sao'
  \$P -d vbp_ban_sao -f /tmp/vbp-ban-sao.sql >/dev/null 2>/tmp/vbp-nap.err || true
  echo 'Lỗi khi nạp bản sao:'; grep ERROR /tmp/vbp-nap.err | sort | uniq -c | head
  \$P -d vbp_ban_sao -Atc 'select count(*) from users; select count(*) from subscription_plans; select count(*) from orders'
  \$P -d vbp_ban_sao -v ON_ERROR_STOP=1 -1 -f /tmp/004_credit.sql
  \$P -d vbp_ban_sao -Atc \"select email_ngan, (credit_so_du(id))->>'tong' from (select id, left(email,3)||'…' as email_ngan from users) u\"
  \$P -d postgres -c 'drop database vbp_ban_sao with (force)'
  rm -f /tmp/vbp-ban-sao.sql /tmp/vbp-nap.err"
```
Expected: lỗi khi nạp chỉ ở phần không liên quan credit (vd `schema "public" already exists`, kiểu `vector` của bảng cloud); `users`, `subscription_plans`, `orders` nạp đủ (3 / 4 / 1 tại ngày 12/11/2026); `004_credit.sql` chạy không lỗi; mỗi người dùng hiện số dư đúng gói (free 30, trả phí theo `credit_thang`). Nếu `users` hay `orders` không nạp được thì dừng, tìm nguyên nhân trước khi migrate prod. Bản sao và tệp dump bị xóa ngay sau khi thử (có dữ liệu người dùng thật).

- [ ] **Step 3: Migrate prod (một giao dịch) + nạp lại lược đồ PostgREST**

```bash
PROD=supabase-db-q323olywghtnjcvpyvtjzo9v
ssh gikidi-dev "sudo docker exec -i $PROD psql -U postgres -d postgres -v ON_ERROR_STOP=1 -1 -q < /tmp/004_credit.sql && sudo docker exec $PROD psql -U postgres -d postgres -c \"NOTIFY pgrst, 'reload schema'\" && sudo docker exec $PROD psql -U postgres -d postgres -Atc \"select count(*) from credit_bang_gia; select id, credit_thang from subscription_plans order by sort_order\""
```
Expected: `9`; bốn gói với 30 / 400 / 1100 / 3000. Ghi một dòng vào `D:\Thang\gikidi-server\THAY-DOI.md` (ngày giờ, «VanBanPlus: migrate 004_credit lên CSDL `supabase-db-q323…`», cách lùi: `DROP TABLE credit_so, credit_lo, tac_vu_chay, credit_bang_gia CASCADE; ALTER TABLE subscription_plans DROP COLUMN credit_thang;` + xóa các hàm `credit_*`, `tac_vu_mo`). Bảng mới nằm trong CSDL `supabase-db-*` có sẵn nên đã thuộc đường sao lưu `sao-luu`, không cần thêm gì.

- [ ] **Step 4: Gộp và đẩy `main` (lên production)**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git checkout main && git pull --ff-only && git merge --no-ff credit-dot-1 -m "Merge credit-dot-1: lop credit + /api/v2/tac-vu (tom-tat)" && npm test && npx next build && git push origin main
```
Expected: đẩy xong; Coolify tự build (theo dõi bằng `python D:\Thang\gikidi-server\cong-cu\tu-trien-khai.py xem` hoặc chờ ~3–5 phút).

- [ ] **Step 5: Thử thật trên production**

```bash
curl -s https://vanbanplus.giakiemso.com/api/health
curl -s -o /dev/null -w "%{http_code}\n" -X POST https://vanbanplus.giakiemso.com/api/v2/tac-vu/tom-tat   # 401 = mã mới đã lên
```
Rồi chạy một lượt thật bằng API key của tài khoản quản trị, lấy key ngay trên máy chủ, **không in ra**:

```bash
ssh gikidi-dev 'K=$(sudo docker exec supabase-db-q323olywghtnjcvpyvtjzo9v psql -U postgres -d postgres -Atc "select api_key from users where role = '"'"'admin'"'"' order by created_date limit 1");
  U=https://vanbanplus.giakiemso.com
  curl -s -H "X-API-Key: $K" $U/api/v2/credit; echo
  curl -s -H "X-API-Key: $K" -H "Content-Type: application/json" -d "{\"noi_dung\":\"Điều 1. Phê duyệt kế hoạch chuyển đổi số năm 2026 của UBND xã Minh An.\nĐiều 2. Giao Văn phòng theo dõi, báo cáo trước 30/12/2026.\",\"loai_van_ban\":\"Quyết định\"}" $U/api/v2/tac-vu/tom-tat/uoc-tinh; echo
  curl -s -H "X-API-Key: $K" -H "X-VBP-Nguon: kiem-thu" -H "Content-Type: application/json" -d "{\"noi_dung\":\"Điều 1. Phê duyệt kế hoạch chuyển đổi số năm 2026 của UBND xã Minh An.\nĐiều 2. Giao Văn phòng theo dõi, báo cáo trước 30/12/2026.\",\"loai_van_ban\":\"Quyết định\"}" $U/api/v2/tac-vu/tom-tat | head -c 600; echo'
```
Expected: `/api/v2/credit` trả `tong` > 0; `uoc-tinh` trả `credit_min ≤ credit_max`; `tom-tat` trả `success: true`, `ket_qua.brief` có chữ, `credit_da_tru` nằm trong khoảng ước tính. Mở `/admin/credit` thấy lượt `tom-tat` (xong) với token vào/ra. Nếu mô hình `gemini-3.6-flash` báo lỗi trên Vertex: đổi `cau_hinh/chung.mo_hinh_mac_dinh` về `gemini-3.5-flash` ở `/admin/credit`, ghi lại, báo chủ dự án.

Kiểm thêm đường cũ: dùng bản cài máy (hoặc `curl` `/api/ai/generate` với cùng key, prompt ngắn) → thành công, và `/admin/credit` có dòng `/api/ai/generate` nguồn `api-cu` với credit trừ.

- [ ] **Step 6: Cập nhật tài liệu, dọn, commit**

- `docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md` dòng 3: `Ngày: 09/11/2026. Trạng thái: chờ chủ dự án duyệt spec.` → `Ngày: 09/11/2026. Trạng thái: đã duyệt 12/11/2026; đợt 1 (lớp credit + tom-tat) lên production <ngày>.`
- `PROJECT_STATUS.md`: thêm dòng vào bảng tính năng (cùng kiểu dòng «Đường mua gói»): `| ✅ | Credit + API tác vụ v2 (đợt 1, <ngày>) | vanbanplus-api: migration 004, /api/v2/tac-vu/tom-tat, /api/v2/credit, /admin/credit | /api/ai/* trừ credit theo token khi dùng khóa hệ thống. Còn 5 tác vụ + lưu hỗ trợ + gói nạp (đợt 2), bản cài máy gọi v2 (đợt 3) |`. Không đụng các thay đổi chưa commit của phiên khác trong tệp này ngoài dòng vừa thêm — nếu tệp đang có thay đổi dở của người khác, thêm dòng rồi commit **cả** tệp chỉ khi thay đổi dở đó cũng thuộc VanBanPlus và đã đúng; không chắc thì hỏi.
- `bash scripts/csdl-thu.sh tat`; tắt đường hầm SSH chạy nền.

```bash
cd /d/AIVanBanCaNhan && git add docs/superpowers vanbanplus-api && git commit -m "docs: credit dot 1 len production; cap nhat vanbanplus-api"
```
(Đẩy repo `van-ban-plus` chỉ khi chủ dự án yêu cầu; repo này không tự triển khai.)

---

## Tự soát kế hoạch so với spec

| Spec | Task |
| --- | --- |
| §1 tiêu chí 2 (giữ/trừ/hoàn, đồng thời, sổ cái khớp) | 3, 4 (bài 5 yêu cầu đồng thời, `kiemSoCai`) |
| §1 tiêu chí 4 (bản cũ chạy qua `/api/ai/*`, trừ credit khi dùng khóa hệ thống) | 10 |
| §1 tiêu chí 5 (không lưu nội dung) | 2 (`tac_vu_chay` không cột nội dung), 7 (không log nội dung) |
| §1 tiêu chí 1, 3 | đợt 2, đợt 3 (ngoài phạm vi, đã ghi) |
| §4 kiến trúc, cấu trúc thư mục | 5–9 |
| §6 hợp đồng API + mã lỗi | 7 (mã lỗi, `bi_cat`), 9 (route, `Retry-After`) |
| §7 quy đổi, ước tính, credit thật | 5 |
| §7 model mặc định, bảng giá trong CSDL | 2 (seed), 6 (đọc), 12 (sửa) |
| §7 mức cấp, lược đồ, hàm Postgres | 2, 3, 4 |
| §7 đường cũ | 10 |
| §7 cấp khi thanh toán (gói) | 2 (`credit_cap_ky` theo `subscription_start_date` mà `pay_order` đặt) |
| §7 gói nạp, `orders.loai` | đợt 2 |
| §8 lưu hỗ trợ | đợt 2 |
| §9 giao diện | 11, 12 (trừ «mở bản lưu»: đợt 2) |
| §10 chuyển `tom-tat` (phía máy chủ + tệp mẫu) | 8 |
| §11 kiểm thử tự động | 1–8, 10 |
| §11 chạy thật | 13 (đo proxy với tác vụ dài: làm ở đợt 2 với `kiem-tra` văn bản dài) |
| §12 bước 1–2, đầu bước 3 | 13 |
| §13 điều kiện mở bán | ngoài phạm vi; không chặn đợt này |
