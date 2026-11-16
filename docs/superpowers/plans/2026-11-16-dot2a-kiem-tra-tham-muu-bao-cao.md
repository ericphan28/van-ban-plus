# Đợt 2a: `kiem-tra`, `tham-muu`, `bao-cao-dinh-ky` lên `/api/v2/tac-vu` — kế hoạch triển khai

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Chuyển ba tác vụ AI `kiem-tra` (DocumentReviewService), `tham-muu` (DocumentAdvisoryService), `bao-cao-dinh-ky` (PeriodicReportService) từ bản cài máy C# về `vanbanplus-api`, chạy trên production qua `/api/v2/tac-vu/<tên>` theo đúng khuôn `tom-tat`, có bảng giá credit và số đo thời gian chờ thật qua proxy.

**Architecture:** Mỗi tác vụ là một thư mục `src/lib/tac-vu/<tên>/` gồm `he-thong.ts` (system prompt C# trích nguyên văn bằng script, không sửa tay), `prompt.ts` (hàm thuần dựng user prompt, chép logic C#) và `dinh-nghia.ts` (kiểm đầu vào theo hợp đồng, kiểm kết quả JSON để bản cài máy đọc được), đăng ký vào `dang-ky.ts`. Luồng chung `chay-tac-vu.ts` không đổi. Phía C#: hàm prompt đổi sang `internal static` (riêng `tham-muu` nhận thêm tham số `now` thay cho `DateTime.Now`) để `MauPromptTests` sinh tệp mẫu; bộ kiểm TypeScript so từng ký tự. Bảng giá ba tác vụ nằm trong migration `007_bang_gia_dot2a.sql` (chỉ INSERT).

**Tech Stack:** Next.js 16 (App Router), TypeScript, `tsx --test`, `pg` (bộ kiểm CSDL), Postgres 17 (ảnh `supabase/postgres:17.6.1.155`), xUnit (.NET 9, `net9.0-windows`) sinh tệp mẫu.

**Spec:** [docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md](../specs/2026-11-09-api-tac-vu-ai-credit-design.md) (§5, §6, §7, §10, §11, §14)

**Hợp đồng (bắt buộc theo):** [docs/superpowers/plans/2026-11-16-hop-dong-tac-vu-v2.md](2026-11-16-hop-dong-tac-vu-v2.md)

**Mẫu định dạng và khung đã có:** kế hoạch đợt 1 `docs/superpowers/plans/2026-11-12-lop-credit-va-khung-tac-vu.md` (Task 8 `tom-tat`, Task 13 lên production). Mã thật đã khác kế hoạch đó ở vài chỗ: luôn đọc mã hiện tại.

## Phạm vi

Làm: phía máy chủ của ba tác vụ (spec §10 «chép prompt sang TypeScript, bộ kiểm so khớp»), bảng giá (§7), trần token ra của `kiem-tra` tăng theo độ dài (§7, §10), đo proxy với tác vụ dài (§11 điểm 2), lên production.

Không làm (kế hoạch khác): `trich-xuat`, `soan-thao` (`dot2b`); gói nạp, lưu hỗ trợ, bỏ đường về AI Studio miễn phí (`dot2c`); thay lời gọi trong bản cài máy, xóa prompt C#, hộp thoại hiện credit (`dot3`). Không sửa `chay-tac-vu.ts`, `PhuThuoc`, `PromptTacVu`, `src/utils/constants.ts`, `tests/ho-tro/csdl-thu.ts`, migration 004–006. Không sửa nội dung prompt (§10: việc riêng sau khi chuyển xong).

Bản cài máy **không đổi hành vi** trong đợt này.

## Global Constraints

- Hai repo. C# + tài liệu: `D:\AIVanBanCaNhan`, nhánh `main`, **chỉ commit tại chỗ, không đẩy**. API: `D:\AIVanBanCaNhan\vanbanplus-api` (git riêng), làm trên nhánh `dot2a` tách từ `main` mới nhất; **đẩy `main` = lên production** (Coolify tự build), chỉ gộp + đẩy ở Task 5.
- Thư mục `docs/thue/`, `.superpowers/` và mọi thay đổi chưa commit lạ là của phiên khác: không `git add` chúng. Luôn `git add` bằng đường dẫn cụ thể.
- Tệp dùng chung chỉ sửa kiểu THÊM, theo mốc văn bản: `src/lib/tac-vu/dang-ky.ts` (thêm 1 import + 1 mục mỗi tác vụ), `AIVanBan.Tests/MauPromptTests.cs` (thêm 1 hàm `[Fact]` mỗi tác vụ, không thêm `using`, không thêm thành viên cấp lớp).
- Tên tác vụ, tên trường đầu vào, kiểu `ket_qua` đúng hợp đồng: `kiem-tra` {`noi_dung`*, `loai_van_ban`, `tieu_de`, `co_quan`, `van_ban_doi_chieu`} → JSON `DocumentReviewResult`; `tham-muu` {`noi_dung`*, `loai_van_ban`, `tieu_de`, `co_quan`, `ngu_canh`} → JSON `DocumentAdvisory`; `bao-cao-dinh-ky` {`loai_ky`*, `ky`*, `linh_vuc`*, `don_vi`*, `so_lieu`*, `bao_cao_ky_truoc`, `nguoi_ky`, `chuc_danh`} → chuỗi.
- Trường chuỗi tùy chọn: thiếu, `null` hoặc `""` đều coi là không có. Ngày trong đầu vào: chuỗi ISO `yyyy-MM-dd`; máy chủ tự định dạng giống hệt C#.
- Prompt chép nguyên văn từ C#: system prompt trích bằng `scripts/trich-prompt-cs.mjs`; user prompt chép logic. Đúng hay sai do bộ kiểm so khớp tệp mẫu C# sinh quyết định — **không bao giờ sửa tay tệp `tests/mau-prompt/*.json`**.
- Lời hiện cho khách (thông báo lỗi): tiếng Việt có dấu, **không đại từ nhân xưng** (tôi, bạn, mình, anh, chị, em, chúng tôi, ta), không dạ/vâng/ạ.
- Không lưu, không `console.log` nội dung văn bản. Không in khóa API, chuỗi kết nối, mật khẩu ra chat hay log.
- Nhiệt độ cả ba tác vụ: `0.7` (bản cài máy gọi `/api/ai/generate` không truyền temperature, máy chủ dùng 0,7 — như `tom-tat`).
- Migrate prod **trước** khi đẩy mã cần dòng bảng giá mới (thiếu dòng → `chuanBi` ném «Bảng giá thiếu tác vụ» → 500).
- Commit message theo lệ hai repo (tiếng Việt không dấu), kết thúc bằng dòng trống rồi `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` (dùng hai cờ `-m`).

## Quyết định đã đặt khi lập kế hoạch

1. **`tham-muu` dùng `DateTime.Now`** trong `BuildUserPrompt` (`HẠN XỬ LÝ: … (N ngày nữa)`), nên không thuần. Tách tối thiểu: thêm tham số `DateTime now`, `AdviseAsync` truyền `DateTime.Now` → hành vi không đổi. Hai hàm `kiem-tra` và hai hàm `bao-cao-dinh-ky` không dùng trạng thái đối tượng (đã kiểm), chỉ đổi `private string` → `internal static string`.
2. **Định dạng ngày của `tham-muu`:** C# viết `{ctx.IssuedDate.Value:dd/MM/yyyy}` và `{ctx.DueDate.Value:dd/MM/yyyy}` trong chuỗi nội suy, tức dùng `CultureInfo.CurrentCulture`; ký tự `/` trong chuỗi định dạng tùy chỉnh là **dấu ngăn ngày của văn hóa**, không phải dấu `/` cố định. Ứng dụng không đặt văn hóa ở đâu (đã tìm `CultureInfo`/`CurrentCulture` toàn repo: chỉ `DashboardPage` tạo `vi-VN` cục bộ), nên theo cài đặt vùng của Windows. Máy chủ chốt theo `vi-VN` (dấu `/`, cũng là của `en-US`, `en-GB`): `yyyy-MM-dd` → `dd/MM/yyyy`. Bộ sinh mẫu C# ép `CurrentCulture = vi-VN` trong lúc sinh để tệp mẫu không phụ thuộc máy chạy test. `PeriodicReportService` dùng `reportPeriodType.ToLower()` (cũng theo văn hóa) — với chữ Việt `toLowerCase()` của JS cho kết quả như `vi-VN`, bộ sinh cũng ép `vi-VN`.
3. **Số ngày còn lại:** C# `(DueDate - DateTime.Now).Days` trên máy người dùng giờ Việt Nam; `DueDate` là ngày (00:00); `TimeSpan.Days` cắt phần lẻ về 0 (âm cũng cắt về 0, vd −0,4 ngày → 0). Máy chủ: hạn = 00:00 giờ Việt Nam (UTC+7 cố định, không có giờ mùa hè) của ngày ISO, `Math.trunc((han − bayGio) / 86.400.000)`, đổi `-0` thành `0`. `bayGio` lấy từ `PhuThuoc.bayGio()` (khung đã có).
4. **Bảng giá** (chi tiết và phép tính ở Task 1): hiệu chỉnh theo `tom-tat` đang chạy (đo 4–13 credit, ước 5–21 với văn bản 1.000–10.000 ký tự): `credit_min` sát mức đo thấp, `credit_max` khoảng 1,3–1,9 lần mức đo cao, `credit_max` với văn bản 10.000 ký tự không quá 30 để gói Free (30 credit) vẫn chạy được; `toi_thieu` ≈ nửa mức đo thấp nhất (như `tom-tat` 2/4).

   | Tác vụ | `toi_thieu` | `ra_min` | `ra_max` (= trần token ra) | `toi_da_ky_tu` | Đo (spec §7) | Ước 1.000 → 10.000 ký tự |
   | --- | --- | --- | --- | --- | --- | --- |
   | `kiem-tra` | 5 | 0,6 × vào + 2.000 | **1,5 × vào + 5.000** | 60.000 | 10–14 | 7–17 → 12–27 |
   | `tham-muu` | 3 | 0,1 × vào + 2.000 | 0,3 × vào + 7.000 | 120.000 | 7–11 | 6–17 → 8–20 |
   | `bao-cao-dinh-ky` | 4 | 0,3 × vào + 3.000 | 0,5 × vào + 11.000 | 80.000 | 9–22 | 7–23 → 10–28 |

   `kiem-tra`: `suggested_content` viết lại toàn văn nên token ra tỷ lệ với token vào; hệ số 1,5 = 1 cho bản viết lại + 0,5 cho danh sách lỗi và suy luận. Văn bản 37.800 ký tự: trần 26.327 token (trần cũ 16.384 làm JSON bị cắt); ở `toi_da_ky_tu` 60.000: 37.427 token, dưới trần 65.536 token ra của Gemini 3.x Flash. 60.000 ký tự là giới hạn mới (bản C# không giới hạn nhưng quá khoảng 35.000 ký tự là hỏng vì trần 16.384).
5. **Kiểm kết quả JSON** chặn đúng những gì làm `System.Text.Json` của bản cài máy ném lỗi khi đọc vào model C#: `overall_score` phải là số nguyên (`int`), `response_needed` phải là boolean, trường chuỗi là chuỗi/null/thiếu, mảng là mảng (không null). Thêm điều kiện nghiệp vụ tối thiểu: `summary` khác rỗng (C# `DocumentAdvisoryService.ParseResult` cũng đòi `Summary` khác rỗng). Kết quả hỏng → khung tự gọi lại 1 lần rồi `AI_LOI` hoàn credit (khung đã có).
6. **Hậu xử lý ở bản cài máy giữ nguyên:** `StripMarkdown` của `kiem-tra` và `CleanAIContent` của `bao-cao-dinh-ky` không chép sang máy chủ; `ket_qua` là đầu ra AI như hợp đồng nói, bản cài máy (`dot3`) vẫn tự làm sạch.
7. **`bao-cao-dinh-ky` trả văn bản:** khung `chay-tac-vu.ts` chỉ gọi `kiemKetQua` với tác vụ JSON, nên tác vụ này không khai `kiemKetQua` (không sửa khung trong đợt này).
8. **Script trích prompt** `scripts/trich-prompt-cs.mjs` được commit vào `vanbanplus-api` (đầu tệp `he-thong.ts` ghi rõ sinh từ đâu, để sinh lại được). Đã thử khi lập kế hoạch: trích `DocumentSummaryService.BuildSystemPrompt` ra đúng từng ký tự `HE_THONG_TOM_TAT` đang chạy; độ dài trích được: `kiem-tra` 4.729, `tham-muu` 6.948, `bao-cao-dinh-ky` 1.598 ký tự.
9. **Biểu tượng trong user prompt** viết bằng mã thoát (`\u{1F3DB}\uFE0F`…) vì chuỗi C# có thêm ký tự biến thể U+FE0F sau 🏛 và ⚠; gõ tay dễ mất.

## Sơ đồ tệp

```
vanbanplus-api/                                   (nhánh dot2a)
  supabase/migrations/007_bang_gia_dot2a.sql      Task 1  ba dòng credit_bang_gia (chỉ INSERT … DO NOTHING)
  tests/ho-tro/bang-gia-dot2a.ts                  Task 1  đọc dòng tac_vu của 007 cho bộ kiểm
  tests/bang-gia-dot2a.test.ts                    Task 1  khoảng credit so với spec §7, trần ra, 007 chạy 2 lần
  scripts/trich-prompt-cs.mjs                     Task 2  trích chuỗi @"..." C# thành hằng TypeScript
  src/lib/tac-vu/kiem-kieu.ts                     Task 2  kiểm kiểu dùng chung (đầu vào + kết quả)
  src/lib/tac-vu/kiem-tra/he-thong.ts             Task 2  sinh bằng script
  src/lib/tac-vu/kiem-tra/prompt.ts               Task 2
  src/lib/tac-vu/kiem-tra/dinh-nghia.ts           Task 2
  src/lib/tac-vu/tham-muu/{he-thong,prompt,dinh-nghia}.ts          Task 3
  src/lib/tac-vu/bao-cao-dinh-ky/{he-thong,prompt,dinh-nghia}.ts   Task 4
  src/lib/tac-vu/dang-ky.ts                       Task 2–4 thêm 1 import + 1 mục mỗi task
  tests/mau-prompt/kiem-tra.json, tham-muu.json, bao-cao-dinh-ky.json   C# sinh (Task 2–4)
  tests/tac-vu-kiem-tra.test.ts, tac-vu-tham-muu.test.ts, tac-vu-bao-cao-dinh-ky.test.ts
AIVanBan.Core/Services/DocumentReviewService.cs    Task 2  2 hàm prompt → internal static
AIVanBan.Core/Services/DocumentAdvisoryService.cs  Task 3  2 hàm → internal static, BuildUserPrompt nhận now
AIVanBan.Core/Services/PeriodicReportService.cs    Task 4  2 hàm → internal static
AIVanBan.Tests/MauPromptTests.cs                   Task 2–4 thêm GhiMauKiemTra, GhiMauThamMuu, GhiMauBaoCaoDinhKy
Task 5: PROJECT_STATUS.md, spec (§11 điểm 2 + dòng trạng thái), D:\Thang\gikidi-server\THAY-DOI.md
```

---

### Task 1: Bảng giá ba tác vụ (`007_bang_gia_dot2a.sql`)

**Files:**
- Create: `vanbanplus-api/supabase/migrations/007_bang_gia_dot2a.sql`
- Create: `vanbanplus-api/tests/ho-tro/bang-gia-dot2a.ts`
- Test: `vanbanplus-api/tests/bang-gia-dot2a.test.ts`

**Interfaces:**
- Consumes: `uocTinh(soKyTuVao, ch, gia, chung): UocTinh`, `type BangGia`, `type CauHinhTacVu` (`src/lib/credit/uoc-tinh.ts`); `dungBangGia(dong: DongBangGia[]): BangGia` (`src/lib/credit/bang-gia.ts`); `CO_CSDL_THU`, `taoCsdlThu()` (`tests/ho-tro/csdl-thu.ts`, nạp sẵn tới 004).
- Produces: dòng `credit_bang_gia` `('tac_vu','kiem-tra'|'tham-muu'|'bao-cao-dinh-ky', {...})`; trong `tests/ho-tro/bang-gia-dot2a.ts`: `SQL_007: string`, `dongTacVu007(): Record<string, CauHinhTacVu>`, `bangGiaSau007(): BangGia` (chung + giá `gemini-3.6-flash` như 004, `tac_vu` = ba dòng của 007). Task 2 dùng `bangGiaSau007()`.

Phép quy đổi dùng để chọn số (giá `gemini-3.6-flash` từ 2027 đã seed ở 004: 1,5 / 7,5 USD mỗi triệu token, 26.000đ/USD, 1 credit = 100đ): 1 token vào = 0,039đ = 0,00039 credit; 1 token ra (gồm suy luận) = 0,195đ = 0,00195 credit. `token_vao` ước = (độ dài system prompt + user prompt) / 3. Số ký tự cố định cộng thêm khi kiểm: `kiem-tra` 4.729 + ~125 (khung user prompt), `tham-muu` 6.948 + ~40, `bao-cao-dinh-ky` 1.598 + ~620.

- [ ] **Step 1: Tạo nhánh làm việc (repo API)**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git status --short && git checkout main && git pull --ff-only && git checkout -b dot2a
```
Expected: `git status` không có gì (có thay đổi lạ thì dừng, hỏi chủ dự án); đang ở nhánh `dot2a`.

- [ ] **Step 2: Viết bộ đọc 007 cho kiểm thử**

`vanbanplus-api/tests/ho-tro/bang-gia-dot2a.ts`:

```ts
import { readFileSync } from "node:fs";
import { join } from "node:path";
import type { BangGia, CauHinhTacVu } from "../../src/lib/credit/uoc-tinh";

export const SQL_007 = readFileSync(
  join(__dirname, "..", "..", "supabase", "migrations", "007_bang_gia_dot2a.sql"),
  "utf8"
);

/** Các dòng ('tac_vu', '<ma>', '<json>') trong 007, đọc thẳng từ tệp SQL. */
export function dongTacVu007(): Record<string, CauHinhTacVu> {
  const ra: Record<string, CauHinhTacVu> = {};
  for (const m of SQL_007.matchAll(/\('tac_vu',\s*'([a-z-]+)',\s*'(\{[^']*\})'\)/g)) {
    ra[m[1]] = JSON.parse(m[2]) as CauHinhTacVu;
  }
  return ra;
}

/** Bảng giá như prod sau 004 + 007: cấu hình chung và giá gemini-3.6-flash chép từ 004. */
export function bangGiaSau007(): BangGia {
  return {
    chung: { gia_tri_credit_vnd: 100, ty_gia_vnd_usd: 26000, mo_hinh_mac_dinh: "gemini-3.6-flash" },
    mo_hinh: { "gemini-3.6-flash": { vao_usd_trieu: 1.5, ra_usd_trieu: 7.5 } },
    tac_vu: dongTacVu007(),
  };
}
```

- [ ] **Step 3: Viết bộ kiểm hỏng**

`vanbanplus-api/tests/bang-gia-dot2a.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { Client } from "pg";
import { dungBangGia, type DongBangGia } from "../src/lib/credit/bang-gia";
import { uocTinh } from "../src/lib/credit/uoc-tinh";
import { bangGiaSau007, dongTacVu007, SQL_007 } from "./ho-tro/bang-gia-dot2a";
import { CO_CSDL_THU, taoCsdlThu } from "./ho-tro/csdl-thu";

/** Trần maxOutputTokens của gemini-3.x-flash. */
const TRAN_RA_GEMINI = 65_536;
/** Số ký tự system prompt C# + khung user prompt của từng tác vụ (đo khi lập kế hoạch). */
const CO_DINH = { "kiem-tra": 4729 + 125, "tham-muu": 6948 + 40, "bao-cao-dinh-ky": 1598 + 620 } as const;
type Ten = keyof typeof CO_DINH;

const BG = bangGiaSau007();
const uoc = (ten: Ten, soKyTuVanBan: number) =>
  uocTinh(CO_DINH[ten] + soKyTuVanBan, BG.tac_vu[ten], BG.mo_hinh["gemini-3.6-flash"], BG.chung);
const khoang = (ten: Ten, n: number) => {
  const u = uoc(ten, n);
  return [u.credit_min, u.credit_max];
};

test("007 chỉ thêm ba dòng tac_vu, đủ trường, qua được dungBangGia", () => {
  assert.deepEqual(Object.keys(dongTacVu007()).sort(), ["bao-cao-dinh-ky", "kiem-tra", "tham-muu"]);
  assert.match(SQL_007, /ON CONFLICT \(loai, ma\) DO NOTHING/);
  assert.doesNotMatch(SQL_007, /\b(UPDATE|DELETE|DROP|ALTER|TRUNCATE)\b/i);
  const dong: DongBangGia[] = [
    { loai: "cau_hinh", ma: "chung", gia_tri: BG.chung },
    { loai: "mo_hinh", ma: "gemini-3.6-flash", gia_tri: BG.mo_hinh["gemini-3.6-flash"] },
    ...Object.entries(dongTacVu007()).map(([ma, gia_tri]) => ({ loai: "tac_vu", ma, gia_tri })),
  ];
  assert.equal(Object.keys(dungBangGia(dong).tac_vu).length, 3);
});

test("khoảng credit ước tính phủ chi phí đo được ở spec §7 (văn bản 1.000–10.000 ký tự)", () => {
  // kiem-tra đo 10–14 credit
  assert.deepEqual([1000, 3000, 6000, 10000].map((n) => khoang("kiem-tra", n)), [[7, 17], [8, 19], [10, 22], [12, 27]]);
  // tham-muu đo 7–11 credit
  assert.deepEqual([1000, 3000, 6000, 10000].map((n) => khoang("tham-muu", n)), [[6, 17], [6, 17], [7, 18], [8, 20]]);
  // bao-cao-dinh-ky đo 9–22 credit
  assert.deepEqual([1000, 3000, 6000, 10000].map((n) => khoang("bao-cao-dinh-ky", n)), [[7, 23], [8, 24], [9, 26], [10, 28]]);
});

test("gói Free (30 credit) chạy được cả ba tác vụ với văn bản 10.000 ký tự", () => {
  for (const ten of Object.keys(CO_DINH) as Ten[]) {
    assert.ok(uoc(ten, 10_000).credit_max <= 30, `${ten}: credit_max ${uoc(ten, 10_000).credit_max}`);
  }
});

test("kiem-tra: trần token ra tăng theo độ dài (suggested_content viết lại toàn văn), vượt trần cũ 16.384", () => {
  assert.ok(BG.tac_vu["kiem-tra"].ra_max_he_so >= 1);
  const dai = uoc("kiem-tra", 37_800);
  assert.equal(dai.ra_max, 26_327);
  assert.ok(dai.ra_max > 16_384);
  assert.ok(uoc("kiem-tra", 37_800).ra_max > uoc("kiem-tra", 3_000).ra_max);
});

test("ở toi_da_ky_tu, trần token ra của mọi tác vụ mới vẫn dưới trần Gemini", () => {
  for (const ten of Object.keys(CO_DINH) as Ten[]) {
    const u = uoc(ten, BG.tac_vu[ten].toi_da_ky_tu);
    assert.ok(u.ra_max <= TRAN_RA_GEMINI, `${ten}: ra_max ${u.ra_max}`);
  }
  assert.equal(uoc("kiem-tra", 60_000).ra_max, 37_427);
});

test("007 chạy hai lần trên CSDL thử: thêm đúng ba dòng, không đè giá đã sửa tay, không đụng tom-tat", { skip: !CO_CSDL_THU }, async () => {
  const csdl = await taoCsdlThu();
  const c = new Client({ connectionString: csdl.url });
  await c.connect();
  try {
    await c.query(SQL_007);
    await c.query(
      `UPDATE credit_bang_gia SET gia_tri = jsonb_set(gia_tri, '{toi_thieu}', '9') WHERE loai = 'tac_vu' AND ma = 'kiem-tra'`
    );
    await c.query(SQL_007);
    const { rows } = await c.query("SELECT loai, ma, gia_tri FROM credit_bang_gia");
    const bg = dungBangGia(rows as DongBangGia[]);
    assert.deepEqual(Object.keys(bg.tac_vu).sort(), ["api-cu", "bao-cao-dinh-ky", "kiem-tra", "tham-muu", "tom-tat"]);
    assert.equal(bg.tac_vu["kiem-tra"].toi_thieu, 9);
    assert.equal(bg.tac_vu["tom-tat"].ra_max_hang, 7000);
  } finally {
    await c.end();
    await csdl.xoa();
  }
});
```

Chạy: `cd /d/AIVanBanCaNhan/vanbanplus-api && npx tsx --test tests/bang-gia-dot2a.test.ts` → FAIL (`ENOENT … 007_bang_gia_dot2a.sql`).

- [ ] **Step 4: Viết migration**

`vanbanplus-api/supabase/migrations/007_bang_gia_dot2a.sql`:

```sql
-- 007_bang_gia_dot2a.sql — bảng giá ba tác vụ đợt 2a: kiem-tra, tham-muu, bao-cao-dinh-ky.
-- Chỉ thêm dòng mới; ON CONFLICT DO NOTHING: chạy lại an toàn, không đè giá đã chỉnh ở /admin/credit.
-- Cách chọn số (spec mục 7 «Chi phí đo được»; giá gemini-3.6-flash từ 2027, 26.000đ/USD, 1 credit = 100đ):
--   1 token vào = 0,039đ; 1 token ra (gồm suy luận) = 0,195đ; token_vao ước = (system + user prompt) / 3 ký tự.
--   Hiệu chỉnh theo tom-tat (đo 4–13 credit, ước 5–21 với văn bản 1.000–10.000 ký tự): credit_min sát mức đo
--   thấp, credit_max khoảng 1,3–1,9 lần mức đo cao, văn bản 10.000 ký tự không quá 30 credit (gói Free chạy được).
--   toi_thieu bằng khoảng nửa mức đo thấp nhất, như tom-tat (2 so với 4).
-- kiem-tra: đo 10–14, ước 7–17 (1.000 ký tự) đến 12–27 (10.000 ký tự). Trần token ra = 1,5 × token_vao + 5.000,
--   tăng theo độ dài vì suggested_content viết lại toàn văn: 37.800 ký tự cho trần 26.327 (trần cũ 16.384 làm
--   JSON bị cắt). toi_da_ky_tu 60.000 cho trần 37.427, dưới trần 65.536 token ra của Gemini 3.x Flash.
-- tham-muu: đo 7–11, ước 6–17 đến 8–20. Đầu ra là JSON 15 mục, ít phụ thuộc độ dài văn bản.
-- bao-cao-dinh-ky: đo 9–22, ước 7–23 đến 10–28 (số liệu cộng báo cáo kỳ trước 1.000–10.000 ký tự).
INSERT INTO credit_bang_gia (loai, ma, gia_tri) VALUES
    ('tac_vu', 'kiem-tra',        '{"toi_thieu": 5, "ra_min_he_so": 0.6, "ra_min_hang": 2000, "ra_max_he_so": 1.5, "ra_max_hang": 5000, "toi_da_ky_tu": 60000}'),
    ('tac_vu', 'tham-muu',        '{"toi_thieu": 3, "ra_min_he_so": 0.1, "ra_min_hang": 2000, "ra_max_he_so": 0.3, "ra_max_hang": 7000, "toi_da_ky_tu": 120000}'),
    ('tac_vu', 'bao-cao-dinh-ky', '{"toi_thieu": 4, "ra_min_he_so": 0.3, "ra_min_hang": 3000, "ra_max_he_so": 0.5, "ra_max_hang": 11000, "toi_da_ky_tu": 80000}')
ON CONFLICT (loai, ma) DO NOTHING;
```

- [ ] **Step 5: Chạy, thấy qua (cả phần CSDL)**

Dựng CSDL thử (nếu chưa có) và mở đường hầm chạy nền:

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && bash scripts/csdl-thu.sh bat
ssh -N -L 55990:127.0.0.1:55990 gikidi-dev    # chạy nền (run_in_background), để mở tới hết Task 5 Step 1
```

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npx tsx --test tests/bang-gia-dot2a.test.ts && npx tsc --noEmit -p .
```
Expected: `# pass 6`, `# skip 0`. Nếu bảng khoảng credit lệch: **không** chỉnh số trong bộ kiểm cho khớp; kiểm lại tệp SQL đúng như Step 4.

- [ ] **Step 6: Commit**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git add supabase/migrations/007_bang_gia_dot2a.sql tests/ho-tro/bang-gia-dot2a.ts tests/bang-gia-dot2a.test.ts && git commit -m "feat(credit): 007 bang gia kiem-tra, tham-muu, bao-cao-dinh-ky (tran ra kiem-tra tang theo do dai)" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 2: Tác vụ `kiem-tra` + script trích prompt + kiểm kiểu dùng chung

**Files:**
- Modify: `AIVanBan.Core/Services/DocumentReviewService.cs` (hai dòng khai báo hàm)
- Modify: `AIVanBan.Tests/MauPromptTests.cs` (thêm hàm `GhiMauKiemTra`)
- Create (sinh ra): `vanbanplus-api/tests/mau-prompt/kiem-tra.json`
- Create: `vanbanplus-api/scripts/trich-prompt-cs.mjs`
- Create: `vanbanplus-api/src/lib/tac-vu/kiem-kieu.ts`
- Create (sinh ra): `vanbanplus-api/src/lib/tac-vu/kiem-tra/he-thong.ts`
- Create: `vanbanplus-api/src/lib/tac-vu/kiem-tra/prompt.ts`, `.../kiem-tra/dinh-nghia.ts`
- Modify: `vanbanplus-api/src/lib/tac-vu/dang-ky.ts` (thêm 1 import + 1 mục)
- Test: `vanbanplus-api/tests/tac-vu-kiem-tra.test.ts`

**Interfaces:**
- Consumes: `DinhNghiaTacVu<V>`, `PromptTacVu`, `chayTacVu`, `PhuThuoc` (`src/lib/tac-vu/chay-tac-vu.ts`, không sửa); `layTacVu` (`dang-ky.ts`); `bangGiaSau007()` (Task 1); `ApiUser` (`src/types`).
- Produces:
  - `scripts/trich-prompt-cs.mjs` — `node scripts/trich-prompt-cs.mjs <tep.cs> <TenHam> <tep-ra.ts> <TEN_HANG>` (Task 3, 4 dùng).
  - `src/lib/tac-vu/kiem-kieu.ts` — `laDoiTuong(v: unknown): v is Record<string, unknown>`, `laChuoiHoacRong(v: unknown): boolean`, `laMangChuoiHoacThieu(v: unknown): boolean`, `chuoiTuyChon(v: unknown): string | undefined` (Task 3, 4 dùng).
  - `HE_THONG_KIEM_TRA: string`, `interface DauVaoKiemTra { noi_dung: string; loai_van_ban: string; tieu_de: string; co_quan: string; van_ban_doi_chieu: string }`, `dungPromptKiemTra(d: DauVaoKiemTra): PromptTacVu`, `kiemTra: DinhNghiaTacVu<DauVaoKiemTra>` (`ten: "kiem-tra"`).
  - C#: `internal static string DocumentReviewService.BuildSystemPrompt()`, `internal static string DocumentReviewService.BuildUserPrompt(string content, string documentType, string title, string issuer, string? referenceContent = null)`.
  - Tệp mẫu: mảng `{ dau_vao: { noi_dung, loai_van_ban, tieu_de, co_quan, van_ban_doi_chieu: string | null }, he_thong, nguoi_dung }`, xuống dòng chuẩn hóa `\n`.

- [ ] **Step 1: Mở hai hàm prompt C# cho bộ kiểm**

`AIVanBan.Core/Services/DocumentReviewService.cs`:
- `    private string BuildSystemPrompt()` → `    internal static string BuildSystemPrompt()`
- `    private string BuildUserPrompt(string content, string documentType, string title, string issuer, string? referenceContent = null)` → `    internal static string BuildUserPrompt(string content, string documentType, string title, string issuer, string? referenceContent = null)`

Hai hàm chỉ dùng tham số (đã kiểm: không đụng `_aiService` hay trường nào); lời gọi trong `ReviewDocumentAsync` giữ nguyên. `AIVanBan.Core.csproj` đã có `InternalsVisibleTo AIVanBan.Tests`.

- [ ] **Step 2: Thêm hàm sinh tệp mẫu C#**

Trong `AIVanBan.Tests/MauPromptTests.cs`, thêm hàm sau **ngay trước dấu `}` đóng lớp `MauPromptTests`** (sau hàm sinh cuối cùng đang có; không đổi gì khác):

````csharp
    [Fact]
    public void GhiMauKiemTra()
    {
        var cacMau = new (string NoiDung, string Loai, string TieuDe, string CoQuan, string? DoiChieu)[]
        {
            ("ỦY BAN NHÂN DÂN XÃ MINH AN\nSố: 15/QĐ-UBND\nQUYẾT ĐỊNH\nVề việc thành lập Tổ công tác chuyển đổi số\nĐiều 1. Thành lập Tổ công tác.\nĐiều 2. Giao Văn phòng theo dõi, xử lý nghiêm.",
             "Quyết định", "Về việc thành lập Tổ công tác chuyển đổi số", "UBND xã Minh An",
             "QUYẾT ĐỊNH\nVề việc thành lập Tổ công tác\nĐiều 1. Thành lập Tổ công tác gồm các ông (bà) có tên sau.\nĐiều 2. Nhiệm vụ của Tổ công tác.\nĐiều 3. Chánh Văn phòng và các cá nhân có tên tại Điều 1 chịu trách nhiệm thi hành Quyết định này.\nNơi nhận:\n- Như Điều 3;\n- Lưu: VT, VP."),
            ("Kính gửi: Các phòng chuyên môn.\nĐề nghị báo cáo trước ngày 20/11/2026.", "", "", "", null),
            ("Nội dung có \"ngoặc kép\", dấu \\ , ký tự ``` và ${bien}.", "  ", "Tiêu đề `lạ`", "Sở Nội vụ", "   "),
        };

        var mau = cacMau.Select(m => new
        {
            dau_vao = new { noi_dung = m.NoiDung, loai_van_ban = m.Loai, tieu_de = m.TieuDe, co_quan = m.CoQuan, van_ban_doi_chieu = m.DoiChieu },
            he_thong = ChuanHoa(DocumentReviewService.BuildSystemPrompt()),
            nguoi_dung = ChuanHoa(DocumentReviewService.BuildUserPrompt(m.NoiDung, m.Loai, m.TieuDe, m.CoQuan, m.DoiChieu)),
        }).ToList();

        Ghi("kiem-tra.json", mau);
    }
````

Ba mẫu phủ: đủ trường + văn bản đối chiếu; không trường phụ + đối chiếu `null`; ký tự đặc biệt cho template literal + loại văn bản chỉ khoảng trắng + đối chiếu chỉ khoảng trắng (C# coi là không có).

- [ ] **Step 3: Sinh tệp mẫu (chạy hai lần)**

```bash
cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests --filter "FullyQualifiedName~MauPromptTests.GhiMauKiemTra"
cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests --filter "FullyQualifiedName~MauPromptTests"
```
Expected: lần 1 FAIL với «Tệp mẫu …kiem-tra.json vừa được sinh lại vì prompt C# đổi» (hàm `Ghi` cố ý báo khi tệp mới); lần 2 `Passed!` cho mọi hàm sinh (kể cả `GhiMauTomTat`). `vanbanplus-api/tests/mau-prompt/kiem-tra.json` có `"he_thong": "Bạn là CHUYÊN GIA SOÁT LỖI VĂN BẢN HÀNH CHÍNH…"` và mẫu 2 có `"van_ban_doi_chieu": null`.

- [ ] **Step 4: Viết script trích prompt và tự kiểm bằng `tom-tat`**

`vanbanplus-api/scripts/trich-prompt-cs.mjs`:

```js
#!/usr/bin/env node
// Trích nguyên văn chuỗi @"..." của một hàm prompt C# thành hằng TypeScript (template literal).
// Dùng (đứng ở vanbanplus-api):  node scripts/trich-prompt-cs.mjs <tep.cs> <TenHam> <tep-ra.ts> <TEN_HANG>
// Lấy chuỗi @"..." ĐẦU TIÊN sau "string <TenHam>(": "" → ", CRLF → LF, rồi thoát \ ` ${ cho template literal.
// Đúng hay sai do bộ kiểm so khớp tests/mau-prompt/*.json (C# sinh) quyết định.
import { readFileSync, writeFileSync } from "node:fs";

const [tepCs, ham, tepRa, tenHang] = process.argv.slice(2);
if (!tepCs || !ham || !tepRa || !tenHang) {
  console.error("Dùng: node scripts/trich-prompt-cs.mjs <tep.cs> <TenHam> <tep-ra.ts> <TEN_HANG>");
  process.exit(2);
}
const nguon = readFileSync(tepCs, "utf8");
const viTriHam = nguon.indexOf(`string ${ham}(`);
if (viTriHam < 0) throw new Error(`Không thấy hàm ${ham} trong ${tepCs}`);
const dau = nguon.indexOf('@"', viTriHam);
if (dau < 0) throw new Error(`Hàm ${ham} không có chuỗi @"..."`);

let s = "";
let i = dau + 2;
for (;;) {
  if (i >= nguon.length) throw new Error('Chuỗi @"..." không đóng');
  const c = nguon[i];
  if (c === '"') {
    if (nguon[i + 1] === '"') {
      s += '"';
      i += 2;
      continue;
    }
    break;
  }
  s += c;
  i++;
}
s = s.replace(/\r\n/g, "\n");
const ts = s.replace(/\\/g, "\\\\").replace(/`/g, "\\`").replace(/\$\{/g, "\\${");
const tenTep = tepCs.replace(/\\/g, "/").split("/").pop();
writeFileSync(
  tepRa,
  `// Sinh bằng scripts/trich-prompt-cs.mjs từ ${tenTep} (${ham}). KHÔNG sửa tay: sửa C# rồi sinh lại;\n` +
    `// bộ kiểm so khớp tests/mau-prompt/*.json canh từng ký tự. Sửa nội dung prompt là việc riêng (spec mục 10).\n` +
    `export const ${tenHang} = \`${ts}\`;\n`
);
console.log(`${tenHang}: ${s.length} ký tự → ${tepRa}`);
```

Tự kiểm với prompt `tom-tat` đang chạy:

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && node scripts/trich-prompt-cs.mjs ../AIVanBan.Core/Services/DocumentSummaryService.cs BuildSystemPrompt .tam-he-thong-tom-tat.ts HE_TT
cat > .tam-so.ts <<'EOF'
import { HE_TT } from "./.tam-he-thong-tom-tat";
import { HE_THONG_TOM_TAT } from "./src/lib/tac-vu/tom-tat/prompt";
console.log(HE_TT === HE_THONG_TOM_TAT ? "KHỚP tom-tat" : "LỆCH tom-tat");
EOF
npx tsx .tam-so.ts; rm -f .tam-so.ts .tam-he-thong-tom-tat.ts
```
Expected: `HE_TT: 3722 ký tự → …`, rồi `KHỚP tom-tat`.

- [ ] **Step 5: Sinh `he-thong.ts` của `kiem-tra`**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && mkdir -p src/lib/tac-vu/kiem-tra && node scripts/trich-prompt-cs.mjs ../AIVanBan.Core/Services/DocumentReviewService.cs BuildSystemPrompt src/lib/tac-vu/kiem-tra/he-thong.ts HE_THONG_KIEM_TRA
```
Expected: `HE_THONG_KIEM_TRA: 4729 ký tự → src/lib/tac-vu/kiem-tra/he-thong.ts`.

- [ ] **Step 6: Viết bộ kiểm hỏng**

`vanbanplus-api/tests/tac-vu-kiem-tra.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { chayTacVu, type PhuThuoc } from "../src/lib/tac-vu/chay-tac-vu";
import { kiemTra } from "../src/lib/tac-vu/kiem-tra/dinh-nghia";
import { layTacVu } from "../src/lib/tac-vu/dang-ky";
import type { ApiUser } from "../src/types";
import { bangGiaSau007 } from "./ho-tro/bang-gia-dot2a";

interface Mau {
  dau_vao: Record<string, unknown>;
  he_thong: string;
  nguoi_dung: string;
}
const MAU: Mau[] = JSON.parse(readFileSync(join(__dirname, "mau-prompt", "kiem-tra.json"), "utf8"));

/** Vị trí ký tự khác đầu tiên, để sửa prompt.ts nhanh khi lệch. */
function khacTai(a: string, b: string): string {
  let i = 0;
  while (i < a.length && a[i] === b[i]) i++;
  return `lệch tại ký tự ${i}: ${JSON.stringify(a.slice(i, i + 40))} ≠ ${JSON.stringify(b.slice(i, i + 40))}`;
}

test("prompt kiểm tra khớp từng ký tự với bản C#", () => {
  assert.ok(MAU.length >= 3);
  for (const m of MAU) {
    const dv = kiemTra.kiemDauVao(m.dau_vao);
    assert.ok(dv.ok, "đầu vào mẫu phải hợp lệ");
    const p = kiemTra.dungPrompt(dv.du_lieu, new Date());
    assert.equal(p.he_thong, m.he_thong, khacTai(p.he_thong, m.he_thong));
    assert.equal(p.nguoi_dung, m.nguoi_dung, khacTai(p.nguoi_dung, m.nguoi_dung));
  }
});

test("bộ mẫu phủ cả có và không có văn bản đối chiếu", () => {
  const dc = MAU.map((m) => m.dau_vao.van_ban_doi_chieu);
  assert.ok(dc.some((x) => typeof x === "string" && x.trim() !== ""));
  assert.ok(dc.some((x) => x === null));
  assert.ok(dc.some((x) => typeof x === "string" && x.trim() === ""));
});

test("đầu vào: thiếu nội dung hoặc sai kiểu thì báo lỗi; trường phụ thiếu / null thành rỗng", () => {
  assert.equal(kiemTra.kiemDauVao({}).ok, false);
  assert.equal(kiemTra.kiemDauVao(null).ok, false);
  assert.equal(kiemTra.kiemDauVao({ noi_dung: "  " }).ok, false);
  assert.equal(kiemTra.kiemDauVao({ noi_dung: "x", van_ban_doi_chieu: 5 }).ok, false);
  assert.equal(kiemTra.kiemDauVao({ noi_dung: "x", tieu_de: ["a"] }).ok, false);
  assert.deepEqual(kiemTra.kiemDauVao({ noi_dung: "x", co_quan: null }), {
    ok: true,
    du_lieu: { noi_dung: "x", loai_van_ban: "", tieu_de: "", co_quan: "", van_ban_doi_chieu: "" },
  });
});

const TOT = {
  overall_score: 7,
  summary: "Văn bản cơ bản đúng thể thức, còn thiếu nơi nhận.",
  strengths: ["Bố cục rõ ràng"],
  issues: [
    {
      severity: "warning",
      category: "format",
      location: "Nơi nhận",
      original_text: "",
      description: "Thiếu «Lưu: VT».",
      suggestion: "Thêm «- Lưu: VT, VP.»",
      reason: "Nghị định 30/2020/NĐ-CP",
    },
  ],
  suggested_content: "",
};

test("kết quả dùng được khi bản cài máy đọc được thành DocumentReviewResult", () => {
  const k = kiemTra.kiemKetQua!;
  assert.equal(k(TOT), true);
  assert.equal(k({ ...TOT, issues: undefined, strengths: undefined, suggested_content: null }), true);
  assert.equal(k(null), false);
  assert.equal(k([]), false);
  assert.equal(k({ ...TOT, overall_score: "7" }), false);
  assert.equal(k({ ...TOT, overall_score: 7.5 }), false);
  assert.equal(k({ ...TOT, summary: " " }), false);
  assert.equal(k({ ...TOT, issues: {} }), false);
  assert.equal(k({ ...TOT, issues: ["lỗi"] }), false);
  assert.equal(k({ ...TOT, issues: [{ ...TOT.issues[0], severity: 2 }] }), false);
  assert.equal(k({ ...TOT, strengths: [1] }), false);
  assert.equal(k({ ...TOT, suggested_content: 3 }), false);
});

test("trần token ra gửi AI tăng theo độ dài văn bản; 37.800 ký tự vượt trần cũ 16.384 (spec §7, §10)", async () => {
  const bg = bangGiaSau007();
  const tranRa: number[] = [];
  const phu: PhuThuoc = {
    bangGia: async () => bg,
    kho: {
      soDu: async () => 1000,
      mo: async () => ({ ma: "ok", luot_id: "L1", so_du: 1000 }),
      chot: async (_id, c) => ({ credit_tru: c.credit_that, so_du: 900, trang_thai: "xong" }),
      hoan: async () => ({ so_du: 1000 }),
    },
    layKhoa: async () => ({ keyId: "k", provider: "gemini", apiKey: "x", model: "gemini-3.6-flash", source: "system" }),
    goiAI: async (_c, o) => {
      tranRa.push(o.maxTokens ?? 0);
      return {
        success: true,
        content: JSON.stringify(TOT),
        promptTokens: 10,
        completionTokens: 10,
        thoughtsTokens: 0,
        finishReason: "STOP",
      };
    },
    bayGio: () => new Date("2026-11-16T02:30:00Z"),
  };
  const ctx = { user: { id: "U1", api_key: "vbp_x" } as ApiUser, nguonGoi: "kiem-thu", phienBan: null };
  for (const n of [3_000, 37_800]) {
    const kq = await chayTacVu(kiemTra, { noi_dung: "Đ".repeat(n) }, ctx, phu);
    assert.equal(kq.ok, true);
  }
  assert.ok(tranRa[1] > 16_384, `trần ra cho 37.800 ký tự là ${tranRa[1]}`);
  assert.ok(tranRa[1] >= 1.5 * (37_800 / 3), "trần ra phải đủ chỗ viết lại toàn văn (suggested_content)");
  assert.ok(tranRa[1] > tranRa[0]);
});

test("danh sách tác vụ có kiem-tra", () => {
  assert.equal(layTacVu("kiem-tra"), kiemTra);
});
```

Chạy: `cd /d/AIVanBanCaNhan/vanbanplus-api && npx tsx --test tests/tac-vu-kiem-tra.test.ts` → FAIL (không có module `kiem-tra/dinh-nghia`).

- [ ] **Step 7: Viết kiểm kiểu dùng chung**

`vanbanplus-api/src/lib/tac-vu/kiem-kieu.ts`:

```ts
/** Kiểm kiểu dùng chung cho kiemDauVao / kiemKetQua của các tác vụ. */

export const laDoiTuong = (v: unknown): v is Record<string, unknown> =>
  typeof v === "object" && v !== null && !Array.isArray(v);

/** Trường chuỗi trong kết quả AI: thiếu, null hoặc chuỗi (System.Text.Json của bản cài máy nhận cả ba). */
export const laChuoiHoacRong = (v: unknown): boolean => v === undefined || v === null || typeof v === "string";

/** Mảng chuỗi hoặc thiếu. Không nhận null: bản cài máy duyệt mảng sẽ lỗi. */
export const laMangChuoiHoacThieu = (v: unknown): boolean =>
  v === undefined || (Array.isArray(v) && v.every((x) => typeof x === "string"));

/** Đầu vào chuỗi tùy chọn (hợp đồng: thiếu / null / "" là không có): thiếu, null → ""; sai kiểu → undefined. */
export function chuoiTuyChon(v: unknown): string | undefined {
  if (v === undefined || v === null) return "";
  return typeof v === "string" ? v : undefined;
}
```

- [ ] **Step 8: Viết `prompt.ts` và `dinh-nghia.ts`**

`vanbanplus-api/src/lib/tac-vu/kiem-tra/prompt.ts` (chép logic `DocumentReviewService.BuildUserPrompt`):

```ts
import type { PromptTacVu } from "../chay-tac-vu";
import { HE_THONG_KIEM_TRA } from "./he-thong";

export interface DauVaoKiemTra {
  noi_dung: string;
  loai_van_ban: string;
  tieu_de: string;
  co_quan: string;
  van_ban_doi_chieu: string;
}

// Biểu tượng viết bằng mã để giữ đúng từng code point như chuỗi C# (🏛️ có thêm U+FE0F).
const BT_LOAI = "\u{1F4CB}"; // 📋
const BT_TIEU_DE = "\u{1F4CC}"; // 📌
const BT_CO_QUAN = "\u{1F3DB}\uFE0F"; // 🏛️

/** Nguyên logic DocumentReviewService.BuildUserPrompt (bản cài máy). */
export function dungPromptKiemTra(d: DauVaoKiemTra): PromptTacVu {
  let p = "KIỂM TRA VĂN BẢN SAU:\n\n";
  if (d.loai_van_ban.trim()) p += `${BT_LOAI} Loại văn bản: ${d.loai_van_ban}\n`;
  if (d.tieu_de.trim()) p += `${BT_TIEU_DE} Tiêu đề: ${d.tieu_de}\n`;
  if (d.co_quan.trim()) p += `${BT_CO_QUAN} Cơ quan ban hành: ${d.co_quan}\n`;

  p += `\n--- NỘI DUNG ---\n${d.noi_dung}\n--- HẾT NỘI DUNG ---\n\n`;

  if (d.van_ban_doi_chieu.trim()) {
    p += "\nĐỐI CHIẾU VỚI VĂN BẢN MẪU SAU:\n";
    p += "--- VĂN BẢN MẪU ---\n";
    p += d.van_ban_doi_chieu;
    p += "\n--- HẾT VĂN BẢN MẪU ---\n\n";
    p += "Hãy phân tích toàn diện văn bản cần kiểm tra, đồng thời SO SÁNH với văn bản mẫu.\n";
    p += "Chỉ ra những điểm KHÁC BIỆT, THIẾU SÓT so với mẫu, và gợi ý sửa theo mẫu.\n";
    p += "Trả về JSON.";
  } else {
    p += "Hãy phân tích toàn diện và trả về JSON.";
  }
  return { he_thong: HE_THONG_KIEM_TRA, nguoi_dung: p };
}
```

`vanbanplus-api/src/lib/tac-vu/kiem-tra/dinh-nghia.ts`:

```ts
import type { DinhNghiaTacVu } from "../chay-tac-vu";
import { chuoiTuyChon, laChuoiHoacRong, laDoiTuong, laMangChuoiHoacThieu } from "../kiem-kieu";
import { dungPromptKiemTra, type DauVaoKiemTra } from "./prompt";

const TRUONG_PHU = ["loai_van_ban", "tieu_de", "co_quan", "van_ban_doi_chieu"] as const;
/** Trường chuỗi của mỗi phần tử issues (ReviewIssue). */
const TRUONG_VAN_DE = ["severity", "category", "location", "original_text", "description", "suggestion", "reason"] as const;

export const kiemTra: DinhNghiaTacVu<DauVaoKiemTra> = {
  ten: "kiem-tra",
  kieu_ket_qua: "json",
  // Bản cài máy gọi /api/ai/generate không truyền temperature, máy chủ dùng 0,7.
  nhiet_do: 0.7,
  kiemDauVao(body) {
    const b: Record<string, unknown> = laDoiTuong(body) ? body : {};
    const noiDung = b.noi_dung;
    if (typeof noiDung !== "string" || !noiDung.trim()) {
      return { ok: false, loi: "Thiếu nội dung văn bản (noi_dung)." };
    }
    const phu = {} as Record<(typeof TRUONG_PHU)[number], string>;
    for (const k of TRUONG_PHU) {
      const v = chuoiTuyChon(b[k]);
      if (v === undefined) return { ok: false, loi: `Trường ${k} phải là chuỗi.` };
      phu[k] = v;
    }
    return { ok: true, du_lieu: { noi_dung: noiDung, ...phu } };
  },
  dungPrompt: (d) => dungPromptKiemTra(d),
  /**
   * Bản cài máy đọc ket_qua bằng System.Text.Json vào DocumentReviewResult: overall_score là int,
   * issues là List<ReviewIssue>, strengths là List<string>. Sai kiểu ở đó là ném lỗi, nên chặn ở đây.
   */
  kiemKetQua: (kq) => {
    if (!laDoiTuong(kq)) return false;
    if (!Number.isInteger(kq.overall_score)) return false;
    if (typeof kq.summary !== "string" || !kq.summary.trim()) return false;
    if (kq.issues !== undefined) {
      if (!Array.isArray(kq.issues)) return false;
      for (const it of kq.issues) {
        if (!laDoiTuong(it)) return false;
        for (const k of TRUONG_VAN_DE) if (!laChuoiHoacRong(it[k])) return false;
      }
    }
    return laMangChuoiHoacThieu(kq.strengths) && laChuoiHoacRong(kq.suggested_content);
  },
};
```

- [ ] **Step 9: Đăng ký tác vụ**

`vanbanplus-api/src/lib/tac-vu/dang-ky.ts` — chỉ thêm, không đổi dòng khác:
- Thêm dòng `import { kiemTra } from "./kiem-tra/dinh-nghia";` ngay sau dòng `import { tomTat } from "./tom-tat/dinh-nghia";`.
- Thêm dòng `  [kiemTra.ten]: kiemTra,` ngay sau dòng `  [tomTat.ten]: tomTat,` trong `DANH_SACH`.

- [ ] **Step 10: Chạy, thấy qua**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && npx tsx --test tests/tac-vu-kiem-tra.test.ts tests/tac-vu-tom-tat.test.ts && npx tsc --noEmit -p .
```
Expected: `# pass 10` (6 + 4 của `tom-tat`), không lỗi kiểu. Bài so khớp hỏng thì đọc thông báo «lệch tại ký tự N» và sửa `prompt.ts` (hoặc chạy lại script nếu lệch ở `he_thong`), **không** sửa tệp mẫu.

- [ ] **Step 11: Commit (hai repo)**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git add scripts/trich-prompt-cs.mjs src/lib/tac-vu/kiem-kieu.ts src/lib/tac-vu/kiem-tra src/lib/tac-vu/dang-ky.ts tests/tac-vu-kiem-tra.test.ts tests/mau-prompt/kiem-tra.json && git commit -m "feat(tac-vu): kiem-tra (prompt chep tu C#, so khop tep mau; kiem ket qua theo DocumentReviewResult)" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
cd /d/AIVanBanCaNhan && git add AIVanBan.Core/Services/DocumentReviewService.cs AIVanBan.Tests/MauPromptTests.cs && git commit -m "test: sinh tep mau prompt kiem-tra cho vanbanplus-api (ham prompt -> internal static)" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 3: Tác vụ `tham-muu` (ngày giờ khớp C#)

**Files:**
- Modify: `AIVanBan.Core/Services/DocumentAdvisoryService.cs` (`AdviseAsync`, `BuildSystemPrompt`, `BuildUserPrompt`)
- Modify: `AIVanBan.Tests/MauPromptTests.cs` (thêm hàm `GhiMauThamMuu`)
- Create (sinh ra): `vanbanplus-api/tests/mau-prompt/tham-muu.json`
- Create (sinh ra): `vanbanplus-api/src/lib/tac-vu/tham-muu/he-thong.ts`
- Create: `vanbanplus-api/src/lib/tac-vu/tham-muu/prompt.ts`, `.../tham-muu/dinh-nghia.ts`
- Modify: `vanbanplus-api/src/lib/tac-vu/dang-ky.ts` (thêm 1 import + 1 mục)
- Test: `vanbanplus-api/tests/tac-vu-tham-muu.test.ts`

**Interfaces:**
- Consumes: `scripts/trich-prompt-cs.mjs` (Task 2); `laDoiTuong`, `laChuoiHoacRong`, `laMangChuoiHoacThieu`, `chuoiTuyChon` (`src/lib/tac-vu/kiem-kieu.ts`, Task 2); `DinhNghiaTacVu`, `PromptTacVu` (khung).
- Produces:
  - `HE_THONG_THAM_MUU: string`; `interface NguCanhThamMuu { huong; nhom; so_van_ban; ngay_ban_hanh; do_khan; do_mat; han_xu_ly; nguoi_ky; chuc_danh_ky; giao_cho; ghi_chu_xu_ly; van_ban_lien_quan: string; noi_nhan: string[]; can_cu: string[] }` (ngày là `yyyy-MM-dd` hoặc `""`); `interface DauVaoThamMuu { noi_dung; loai_van_ban; tieu_de; co_quan: string; ngu_canh: NguCanhThamMuu | null }`; `ngayKieuCs(iso: string): string`; `soNgayConLai(iso: string, bayGio: Date): number`; `dungPromptThamMuu(d: DauVaoThamMuu, bayGio: Date): PromptTacVu`; `laNgayIso(s: string): boolean`; `thamMuu: DinhNghiaTacVu<DauVaoThamMuu>` (`ten: "tham-muu"`).
  - C#: `internal static string DocumentAdvisoryService.BuildSystemPrompt()`, `internal static string DocumentAdvisoryService.BuildUserPrompt(string content, string documentType, string title, string issuer, DocumentAdvisoryContext? ctx, DateTime now)`.
  - Tệp mẫu: mảng `{ bay_gio: string (ISO UTC), dau_vao: { noi_dung, loai_van_ban, tieu_de, co_quan, ngu_canh: {...14 trường như hợp đồng, ngày ISO, null khi không có} | null }, he_thong, nguoi_dung }`.

- [ ] **Step 1: Tách `DateTime.Now` khỏi hàm dựng prompt C# (hành vi không đổi)**

`AIVanBan.Core/Services/DocumentAdvisoryService.cs`:
- Trong `AdviseAsync`: `        var userPrompt = BuildUserPrompt(content, documentType, title, issuer, context);` → `        var userPrompt = BuildUserPrompt(content, documentType, title, issuer, context, DateTime.Now);`
- `    private string BuildSystemPrompt()` → `    internal static string BuildSystemPrompt()`
- Khai báo:
  ```csharp
      private string BuildUserPrompt(string content, string documentType, string title, string issuer,
          DocumentAdvisoryContext? ctx = null)
  ```
  thành
  ```csharp
      /// <summary>Hàm thuần: thời điểm hiện tại truyền vào (AdviseAsync truyền DateTime.Now) để bộ kiểm so khớp được.</summary>
      internal static string BuildUserPrompt(string content, string documentType, string title, string issuer,
          DocumentAdvisoryContext? ctx, DateTime now)
  ```
- Trong thân hàm: `(ctx.DueDate.Value - DateTime.Now).Days` → `(ctx.DueDate.Value - now).Days`.

Đã kiểm: `BuildUserPrompt` chỉ có một chỗ gọi (`AdviseAsync`); ngoài `DateTime.Now` không dùng trạng thái đối tượng. `BuildSystemPrompt` thuần.

- [ ] **Step 2: Thêm hàm sinh tệp mẫu C#**

Trong `AIVanBan.Tests/MauPromptTests.cs`, thêm **ngay trước dấu `}` đóng lớp `MauPromptTests`**:

````csharp
    [Fact]
    public void GhiMauThamMuu()
    {
        // Ép văn hóa vi-VN: C# định dạng {ngày:dd/MM/yyyy} theo CurrentCulture (dấu "/" là dấu ngăn ngày của văn hóa).
        var vanHoaCu = System.Globalization.CultureInfo.CurrentCulture;
        System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("vi-VN");
        try
        {
            // 09:30 ngày 16/11/2026 giờ máy người dùng (Việt Nam, UTC+7) = 02:30 UTC.
            var bayGio = new DateTime(2026, 11, 16, 9, 30, 0);
            const string bayGioIso = "2026-11-16T02:30:00.000Z";

            static string? Iso(DateTime? d) =>
                d?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            static object NguCanhJson(DocumentAdvisoryContext c) => new
            {
                huong = c.Direction,
                nhom = c.Category,
                so_van_ban = c.DocumentNumber,
                ngay_ban_hanh = Iso(c.IssuedDate),
                do_khan = c.UrgencyLevel,
                do_mat = c.SecurityLevel,
                han_xu_ly = Iso(c.DueDate),
                nguoi_ky = c.SignerName,
                chuc_danh_ky = c.SignerTitle,
                noi_nhan = c.Recipients,
                giao_cho = c.AssignedTo,
                ghi_chu_xu_ly = c.ProcessingNotes,
                can_cu = c.BasedOn,
                van_ban_lien_quan = c.RelatedDocumentsSummary,
            };

            // Đủ 14 trường; hạn còn 3,6 ngày → «3 ngày nữa».
            var day = new DocumentAdvisoryContext
            {
                Direction = "Đến",
                Category = "Đất đai",
                DocumentNumber = "123/UBND-KT",
                IssuedDate = new DateTime(2026, 11, 3),
                UrgencyLevel = "Khẩn",
                SecurityLevel = "Mật",
                DueDate = new DateTime(2026, 11, 20),
                SignerName = "Nguyễn Văn A",
                SignerTitle = "Phó Chủ tịch",
                Recipients = new[] { "UBND các xã, phường", "Phòng Kinh tế" },
                AssignedTo = "Địa chính - XD - MT",
                ProcessingNotes = "Giao Địa chính chủ trì, báo cáo trước hạn.",
                BasedOn = new[] { "Luật Đất đai 2024", "Nghị định 102/2024/NĐ-CP" },
                RelatedDocumentsSummary = "CV 98/UBND-KT ngày 10/10/2026 về rà soát đất công.\nĐã báo cáo lần 1.",
            };
            // «Thường» bị bỏ qua; chức danh không có người ký thì bỏ; mảng rỗng bỏ; hạn đã qua 6,4 ngày → «-6 ngày nữa».
            var mot = new DocumentAdvisoryContext
            {
                Direction = "Đến",
                UrgencyLevel = "Thường",
                SecurityLevel = "Thường",
                DueDate = new DateTime(2026, 11, 10),
                SignerTitle = "Chủ tịch",
                Recipients = Array.Empty<string>(),
                BasedOn = Array.Empty<string>(),
            };
            // Hạn đúng hôm nay (còn −0,4 ngày) → «0 ngày nữa»; người ký không chức danh.
            var homNay = new DocumentAdvisoryContext
            {
                SignerName = "Trần Thị B",
                DueDate = new DateTime(2026, 11, 16),
                IssuedDate = new DateTime(2026, 1, 5),
            };

            var cacMau = new (string NoiDung, string Loai, string TieuDe, string CoQuan, DocumentAdvisoryContext? NguCanh)[]
            {
                ("Đề nghị UBND các xã rà soát đất công, báo cáo trước ngày 20/11/2026.", "Công văn", "V/v rà soát đất công", "UBND tỉnh Đồng Nai", day),
                ("Thông báo lịch tiếp công dân tháng 12/2026.", "Thông báo", "", "Văn phòng UBND tỉnh", new DocumentAdvisoryContext()),
                ("Kính gửi: UBND xã.\nĐề nghị báo cáo số liệu dân số.", "", "", "", null),
                ("Nhắc lại hạn báo cáo; ký tự \"ngoặc kép\", \\ , ``` và ${bien}.", "Công văn", "Nhắc hạn", "Sở Nội vụ", mot),
                ("Mời họp giao ban.", "Giấy mời", "Mời họp", "UBND xã", homNay),
            };

            var mau = cacMau.Select(m => new
            {
                bay_gio = bayGioIso,
                dau_vao = new
                {
                    noi_dung = m.NoiDung,
                    loai_van_ban = m.Loai,
                    tieu_de = m.TieuDe,
                    co_quan = m.CoQuan,
                    ngu_canh = m.NguCanh == null ? null : NguCanhJson(m.NguCanh),
                },
                he_thong = ChuanHoa(DocumentAdvisoryService.BuildSystemPrompt()),
                nguoi_dung = ChuanHoa(DocumentAdvisoryService.BuildUserPrompt(m.NoiDung, m.Loai, m.TieuDe, m.CoQuan, m.NguCanh, bayGio)),
            }).ToList();

            Ghi("tham-muu.json", mau);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = vanHoaCu;
        }
    }
````

- [ ] **Step 3: Sinh tệp mẫu (chạy hai lần) và soát ngày**

```bash
cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests --filter "FullyQualifiedName~MauPromptTests.GhiMauThamMuu"
cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests --filter "FullyQualifiedName~MauPromptTests"
grep -o "NGÀY BAN HÀNH: [^\\]*\|HẠN XỬ LÝ: [^\\]*" vanbanplus-api/tests/mau-prompt/tham-muu.json
```
Expected: lần 1 FAIL «…tham-muu.json vừa được sinh lại…»; lần 2 `Passed!`. `grep` in ra `NGÀY BAN HÀNH: 03/11/2026`, `HẠN XỬ LÝ: 20/11/2026 (3 ngày nữa)`, `HẠN XỬ LÝ: 10/11/2026 (-6 ngày nữa)`, `NGÀY BAN HÀNH: 05/01/2026`, `HẠN XỬ LÝ: 16/11/2026 (0 ngày nữa)`. Nếu dấu ngăn ngày không phải `/` thì dừng, báo lại (giả định `vi-VN` dùng `/` sai), không sửa TypeScript theo cho khớp.

- [ ] **Step 4: Sinh `he-thong.ts` của `tham-muu`**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && mkdir -p src/lib/tac-vu/tham-muu && node scripts/trich-prompt-cs.mjs ../AIVanBan.Core/Services/DocumentAdvisoryService.cs BuildSystemPrompt src/lib/tac-vu/tham-muu/he-thong.ts HE_THONG_THAM_MUU
```
Expected: `HE_THONG_THAM_MUU: 6948 ký tự → src/lib/tac-vu/tham-muu/he-thong.ts`.

- [ ] **Step 5: Viết bộ kiểm hỏng**

`vanbanplus-api/tests/tac-vu-tham-muu.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { laNgayIso, thamMuu } from "../src/lib/tac-vu/tham-muu/dinh-nghia";
import { ngayKieuCs, soNgayConLai } from "../src/lib/tac-vu/tham-muu/prompt";
import { layTacVu } from "../src/lib/tac-vu/dang-ky";

interface Mau {
  bay_gio: string;
  dau_vao: Record<string, unknown>;
  he_thong: string;
  nguoi_dung: string;
}
const MAU: Mau[] = JSON.parse(readFileSync(join(__dirname, "mau-prompt", "tham-muu.json"), "utf8"));

function khacTai(a: string, b: string): string {
  let i = 0;
  while (i < a.length && a[i] === b[i]) i++;
  return `lệch tại ký tự ${i}: ${JSON.stringify(a.slice(i, i + 40))} ≠ ${JSON.stringify(b.slice(i, i + 40))}`;
}

test("prompt tham mưu khớp từng ký tự với bản C# (kể cả ngày dd/MM/yyyy và số ngày còn lại)", () => {
  assert.ok(MAU.length >= 5);
  for (const m of MAU) {
    const dv = thamMuu.kiemDauVao(m.dau_vao);
    assert.ok(dv.ok, "đầu vào mẫu phải hợp lệ");
    const p = thamMuu.dungPrompt(dv.du_lieu, new Date(m.bay_gio));
    assert.equal(p.he_thong, m.he_thong, khacTai(p.he_thong, m.he_thong));
    assert.equal(p.nguoi_dung, m.nguoi_dung, khacTai(p.nguoi_dung, m.nguoi_dung));
  }
});

test("bộ mẫu phủ đủ nhánh: ngu_canh đầy đủ, toàn null, không gửi; hạn còn / đã qua / đúng hôm nay", () => {
  const nc = MAU.map((m) => m.dau_vao.ngu_canh as Record<string, unknown> | null);
  assert.ok(nc.some((x) => x === null));
  assert.ok(nc.some((x) => x !== null && Object.values(x).every((v) => v === null)));
  assert.ok(nc.some((x) => x !== null && Object.values(x).every((v) => v !== null)));
  const soNgay = MAU.map((m) => /\((-?\d+) ngày nữa\)/.exec(m.nguoi_dung)?.[1]).filter((x): x is string => !!x);
  assert.deepEqual([...new Set(soNgay)].sort(), ["-6", "0", "3"]);
});

test("ngày ISO → dd/MM/yyyy; số ngày còn lại theo giờ Việt Nam, cắt phần lẻ về 0 như TimeSpan.Days", () => {
  assert.equal(ngayKieuCs("2026-03-05"), "05/03/2026");
  const luc = new Date("2026-11-16T02:30:00.000Z"); // 09:30 giờ Việt Nam
  assert.equal(soNgayConLai("2026-11-20", luc), 3);
  assert.equal(soNgayConLai("2026-11-10", luc), -6);
  assert.ok(Object.is(soNgayConLai("2026-11-16", luc), 0)); // không in «-0»
  // 03:00 ngày 17/11 giờ Việt Nam: hạn 18/11 còn 21 giờ → 0 (tính theo UTC sẽ ra 1).
  assert.equal(soNgayConLai("2026-11-18", new Date("2026-11-16T20:00:00.000Z")), 0);
  // Đúng 00:00 ngày 16/11 giờ Việt Nam, hạn 17/11 → tròn 1 ngày.
  assert.equal(soNgayConLai("2026-11-17", new Date("2026-11-15T17:00:00.000Z")), 1);
});

test("đầu vào: ngu_canh {} cho prompt giống hệt khi không gửi; sai kiểu hoặc sai ngày thì báo lỗi", () => {
  const luc = new Date("2026-11-16T02:30:00.000Z");
  const a = thamMuu.kiemDauVao({ noi_dung: "x", ngu_canh: {} });
  const b = thamMuu.kiemDauVao({ noi_dung: "x" });
  assert.ok(a.ok && b.ok);
  assert.equal(thamMuu.dungPrompt(a.du_lieu, luc).nguoi_dung, thamMuu.dungPrompt(b.du_lieu, luc).nguoi_dung);
  assert.deepEqual(b, { ok: true, du_lieu: { noi_dung: "x", loai_van_ban: "", tieu_de: "", co_quan: "", ngu_canh: null } });

  assert.equal(thamMuu.kiemDauVao({}).ok, false);
  assert.equal(thamMuu.kiemDauVao({ noi_dung: " " }).ok, false);
  assert.equal(thamMuu.kiemDauVao({ noi_dung: "x", co_quan: 1 }).ok, false);
  assert.equal(thamMuu.kiemDauVao({ noi_dung: "x", ngu_canh: "a" }).ok, false);
  assert.equal(thamMuu.kiemDauVao({ noi_dung: "x", ngu_canh: [] }).ok, false);
  assert.equal(thamMuu.kiemDauVao({ noi_dung: "x", ngu_canh: { huong: 1 } }).ok, false);
  assert.equal(thamMuu.kiemDauVao({ noi_dung: "x", ngu_canh: { han_xu_ly: "20/11/2026" } }).ok, false);
  assert.equal(thamMuu.kiemDauVao({ noi_dung: "x", ngu_canh: { ngay_ban_hanh: "2026-02-30" } }).ok, false);
  assert.equal(thamMuu.kiemDauVao({ noi_dung: "x", ngu_canh: { noi_nhan: "Sở Nội vụ" } }).ok, false);
  assert.equal(thamMuu.kiemDauVao({ noi_dung: "x", ngu_canh: { can_cu: [1] } }).ok, false);
  assert.equal(thamMuu.kiemDauVao({ noi_dung: "x", ngu_canh: { han_xu_ly: "", ngay_ban_hanh: null } }).ok, true);
  assert.equal(laNgayIso("2028-02-29"), true);
  assert.equal(laNgayIso("2026-13-01"), false);
});

const TOT = {
  summary: "UBND tỉnh đề nghị rà soát đất công, báo cáo trước 20/11/2026.",
  urgency_level: "khan",
  action_items: ["Bước 1: Văn thư vào sổ, trình CT UBND (ngay trong ngày)"],
  deadlines: [{ task: "Gửi báo cáo", date: "20/11/2026" }],
  suggested_handler: "Địa chính - XD - MT",
  coordination: ["Tư pháp"],
  signing_authority: "PCT phụ trách KT ký TM.",
  response_needed: true,
  response_type: "Báo cáo",
  draft_response_outline: "- Phúc đáp CV số...",
  legal_references: [],
  risk_warning: "Quá hạn báo cáo",
  priority: "high",
  related_field: "Đất đai",
  incoming_type: "Yêu cầu báo cáo",
};

test("kết quả dùng được khi bản cài máy đọc được thành DocumentAdvisory", () => {
  const k = thamMuu.kiemKetQua!;
  assert.equal(k(TOT), true);
  assert.equal(k({ summary: "Chỉ để biết." }), true);
  assert.equal(k({ ...TOT, response_type: null }), true);
  assert.equal(k(null), false);
  assert.equal(k({ ...TOT, summary: "" }), false);
  assert.equal(k({ ...TOT, response_needed: "true" }), false);
  assert.equal(k({ ...TOT, action_items: "Bước 1" }), false);
  assert.equal(k({ ...TOT, deadlines: null }), false);
  assert.equal(k({ ...TOT, deadlines: [{ task: "x", date: 20 }] }), false);
  assert.equal(k({ ...TOT, priority: 1 }), false);
});

test("danh sách tác vụ có tham-muu", () => {
  assert.equal(layTacVu("tham-muu"), thamMuu);
});
```

Chạy: `cd /d/AIVanBanCaNhan/vanbanplus-api && npx tsx --test tests/tac-vu-tham-muu.test.ts` → FAIL (không có module `tham-muu/dinh-nghia`).

- [ ] **Step 6: Viết `prompt.ts`**

`vanbanplus-api/src/lib/tac-vu/tham-muu/prompt.ts` (chép logic `DocumentAdvisoryService.BuildUserPrompt`):

```ts
import type { PromptTacVu } from "../chay-tac-vu";
import { HE_THONG_THAM_MUU } from "./he-thong";

/** ngu_canh của tham-muu — ánh xạ DocumentAdvisoryContext (hợp đồng). Chuỗi rỗng / mảng rỗng = không có. */
export interface NguCanhThamMuu {
  huong: string;
  nhom: string;
  so_van_ban: string;
  /** yyyy-MM-dd hoặc "" */
  ngay_ban_hanh: string;
  do_khan: string;
  do_mat: string;
  /** yyyy-MM-dd hoặc "" */
  han_xu_ly: string;
  nguoi_ky: string;
  chuc_danh_ky: string;
  noi_nhan: string[];
  giao_cho: string;
  ghi_chu_xu_ly: string;
  can_cu: string[];
  van_ban_lien_quan: string;
}

export interface DauVaoThamMuu {
  noi_dung: string;
  loai_van_ban: string;
  tieu_de: string;
  co_quan: string;
  ngu_canh: NguCanhThamMuu | null;
}

const GIO_VIET_NAM_MS = 7 * 3_600_000;
const NGAY_MS = 86_400_000;

/** C# {ngày:dd/MM/yyyy} với văn hóa vi-VN (dấu ngăn ngày «/»). iso đã được kiemDauVao kiểm là yyyy-MM-dd. */
export function ngayKieuCs(iso: string): string {
  const [y, m, d] = iso.split("-");
  return `${d}/${m}/${y}`;
}

/**
 * C# (DueDate - DateTime.Now).Days trên máy người dùng giờ Việt Nam: hạn là 00:00 giờ Việt Nam (UTC+7),
 * TimeSpan.Days cắt phần lẻ về 0 (cả khi âm). Trả 0 thay cho -0 để không in «-0».
 */
export function soNgayConLai(iso: string, bayGio: Date): number {
  const [y, m, d] = iso.split("-").map(Number);
  const n = Math.trunc((Date.UTC(y, m - 1, d) - GIO_VIET_NAM_MS - bayGio.getTime()) / NGAY_MS);
  return n === 0 ? 0 : n;
}

const co = (s: string) => s.trim() !== "";

/** Nguyên logic DocumentAdvisoryService.BuildUserPrompt (bản cài máy); bayGio thay DateTime.Now. */
export function dungPromptThamMuu(d: DauVaoThamMuu, bayGio: Date): PromptTacVu {
  const parts: string[] = [];

  if (co(d.loai_van_ban)) parts.push(`LOẠI VĂN BẢN: ${d.loai_van_ban}`);
  if (co(d.tieu_de)) parts.push(`TRÍCH YẾU: ${d.tieu_de}`);
  if (co(d.co_quan)) parts.push(`CƠ QUAN GỬI: ${d.co_quan}`);

  const c = d.ngu_canh;
  if (c) {
    if (co(c.huong)) parts.push(`HƯỚNG VĂN BẢN: ${c.huong}`);
    if (co(c.nhom)) parts.push(`LĨNH VỰC: ${c.nhom}`);
    if (co(c.so_van_ban)) parts.push(`SỐ KÝ HIỆU: ${c.so_van_ban}`);
    if (c.ngay_ban_hanh) parts.push(`NGÀY BAN HÀNH: ${ngayKieuCs(c.ngay_ban_hanh)}`);
    if (co(c.do_khan) && c.do_khan !== "Thường") parts.push(`MỨC ĐỘ KHẨN: ${c.do_khan}`);
    if (co(c.do_mat) && c.do_mat !== "Thường") parts.push(`ĐỘ MẬT: ${c.do_mat}`);
    if (c.han_xu_ly) {
      parts.push(`HẠN XỬ LÝ: ${ngayKieuCs(c.han_xu_ly)} (${soNgayConLai(c.han_xu_ly, bayGio)} ngày nữa)`);
    }
    if (co(c.nguoi_ky)) {
      parts.push(`NGƯỜI KÝ: ${c.nguoi_ky}` + (co(c.chuc_danh_ky) ? ` \u2014 ${c.chuc_danh_ky}` : ""));
    }
    if (c.noi_nhan.length > 0) parts.push(`NƠI NHẬN: ${c.noi_nhan.join("; ")}`);
    if (co(c.giao_cho)) parts.push(`PHÂN CÔNG XỬ LÝ: ${c.giao_cho}`);
    if (co(c.ghi_chu_xu_ly)) parts.push(`Ý KIẾN CHỈ ĐẠO: ${c.ghi_chu_xu_ly}`);
    if (c.can_cu.length > 0) {
      parts.push("CĂN CỨ PHÁP LÝ VIỆN DẪN TRONG VĂN BẢN:");
      for (const b of c.can_cu) parts.push(`  - ${b}`);
    }
    if (co(c.van_ban_lien_quan)) parts.push(`VĂN BẢN LIÊN QUAN TRƯỚC ĐÓ:\n${c.van_ban_lien_quan}`);
  }

  parts.push(`NỘI DUNG VĂN BẢN:\n\n${d.noi_dung}`);
  return { he_thong: HE_THONG_THAM_MUU, nguoi_dung: parts.join("\n\n") };
}
```

- [ ] **Step 7: Viết `dinh-nghia.ts`**

`vanbanplus-api/src/lib/tac-vu/tham-muu/dinh-nghia.ts`:

```ts
import type { DinhNghiaTacVu } from "../chay-tac-vu";
import { chuoiTuyChon, laChuoiHoacRong, laDoiTuong, laMangChuoiHoacThieu } from "../kiem-kieu";
import { dungPromptThamMuu, type DauVaoThamMuu, type NguCanhThamMuu } from "./prompt";

const TRUONG_PHU = ["loai_van_ban", "tieu_de", "co_quan"] as const;
const CHUOI_NGU_CANH = [
  "huong", "nhom", "so_van_ban", "do_khan", "do_mat", "nguoi_ky", "chuc_danh_ky", "giao_cho", "ghi_chu_xu_ly", "van_ban_lien_quan",
] as const;
const NGAY_NGU_CANH = ["ngay_ban_hanh", "han_xu_ly"] as const;
const MANG_NGU_CANH = ["noi_nhan", "can_cu"] as const;
/** Trường chuỗi / mảng chuỗi của DocumentAdvisory (C#). */
const CHUOI_KET_QUA = [
  "urgency_level", "suggested_handler", "signing_authority", "response_type", "draft_response_outline",
  "risk_warning", "priority", "related_field", "incoming_type",
] as const;
const MANG_KET_QUA = ["action_items", "coordination", "legal_references"] as const;

/** Chuỗi yyyy-MM-dd và là ngày có thật. */
export function laNgayIso(s: string): boolean {
  const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(s);
  if (!m) return false;
  const [y, th, d] = [Number(m[1]), Number(m[2]), Number(m[3])];
  const t = new Date(Date.UTC(y, th - 1, d));
  return t.getUTCFullYear() === y && t.getUTCMonth() === th - 1 && t.getUTCDate() === d;
}

type DocNguCanh = { ok: true; nc: NguCanhThamMuu | null } | { ok: false; loi: string };

function docNguCanh(v: unknown): DocNguCanh {
  if (v === undefined || v === null) return { ok: true, nc: null };
  if (!laDoiTuong(v)) return { ok: false, loi: "Trường ngu_canh phải là đối tượng." };
  const nc = {} as NguCanhThamMuu;
  for (const k of CHUOI_NGU_CANH) {
    const s = chuoiTuyChon(v[k]);
    if (s === undefined) return { ok: false, loi: `Trường ngu_canh.${k} phải là chuỗi.` };
    nc[k] = s;
  }
  for (const k of NGAY_NGU_CANH) {
    const s = chuoiTuyChon(v[k]);
    if (s === undefined || (s !== "" && !laNgayIso(s))) {
      return { ok: false, loi: `Trường ngu_canh.${k} phải là ngày dạng yyyy-MM-dd.` };
    }
    nc[k] = s;
  }
  for (const k of MANG_NGU_CANH) {
    const a = v[k];
    if (a === undefined || a === null) {
      nc[k] = [];
      continue;
    }
    if (!Array.isArray(a) || !a.every((x) => typeof x === "string")) {
      return { ok: false, loi: `Trường ngu_canh.${k} phải là mảng chuỗi.` };
    }
    nc[k] = a;
  }
  return { ok: true, nc };
}

export const thamMuu: DinhNghiaTacVu<DauVaoThamMuu> = {
  ten: "tham-muu",
  kieu_ket_qua: "json",
  // Bản cài máy gọi /api/ai/generate không truyền temperature, máy chủ dùng 0,7.
  nhiet_do: 0.7,
  kiemDauVao(body) {
    const b: Record<string, unknown> = laDoiTuong(body) ? body : {};
    const noiDung = b.noi_dung;
    if (typeof noiDung !== "string" || !noiDung.trim()) {
      return { ok: false, loi: "Thiếu nội dung văn bản (noi_dung)." };
    }
    const phu = {} as Record<(typeof TRUONG_PHU)[number], string>;
    for (const k of TRUONG_PHU) {
      const v = chuoiTuyChon(b[k]);
      if (v === undefined) return { ok: false, loi: `Trường ${k} phải là chuỗi.` };
      phu[k] = v;
    }
    const nc = docNguCanh(b.ngu_canh);
    if (!nc.ok) return nc;
    return { ok: true, du_lieu: { noi_dung: noiDung, ...phu, ngu_canh: nc.nc } };
  },
  dungPrompt: (d, bayGio) => dungPromptThamMuu(d, bayGio),
  /**
   * Bản cài máy đọc ket_qua bằng System.Text.Json vào DocumentAdvisory: response_needed là bool, các danh sách
   * là List<string> / List<DeadlineItem>. Như ParseResult của C#, summary phải khác rỗng.
   */
  kiemKetQua: (kq) => {
    if (!laDoiTuong(kq)) return false;
    const o = kq;
    if (typeof o.summary !== "string" || !o.summary.trim()) return false;
    if (o.response_needed !== undefined && typeof o.response_needed !== "boolean") return false;
    if (!CHUOI_KET_QUA.every((k) => laChuoiHoacRong(o[k]))) return false;
    if (!MANG_KET_QUA.every((k) => laMangChuoiHoacThieu(o[k]))) return false;
    if (o.deadlines === undefined) return true;
    return (
      Array.isArray(o.deadlines) &&
      o.deadlines.every((x) => laDoiTuong(x) && laChuoiHoacRong(x.task) && laChuoiHoacRong(x.date))
    );
  },
};
```

- [ ] **Step 8: Đăng ký tác vụ**

`vanbanplus-api/src/lib/tac-vu/dang-ky.ts` — chỉ thêm:
- Dòng `import { thamMuu } from "./tham-muu/dinh-nghia";` ngay sau dòng import tác vụ cuối cùng đang có (sau `import { kiemTra } from "./kiem-tra/dinh-nghia";`).
- Dòng `  [thamMuu.ten]: thamMuu,` ngay sau mục cuối cùng đang có trong `DANH_SACH` (sau `  [kiemTra.ten]: kiemTra,`).

- [ ] **Step 9: Chạy, thấy qua**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && npx tsx --test tests/tac-vu-tham-muu.test.ts tests/tac-vu-kiem-tra.test.ts && npx tsc --noEmit -p .
```
Expected: `# pass 12` (6 + 6), không lỗi kiểu. Lệch thì đọc «lệch tại ký tự N», sửa `prompt.ts`; không sửa tệp mẫu.

- [ ] **Step 10: Commit (hai repo)**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git add src/lib/tac-vu/tham-muu src/lib/tac-vu/dang-ky.ts tests/tac-vu-tham-muu.test.ts tests/mau-prompt/tham-muu.json && git commit -m "feat(tac-vu): tham-muu (ngu_canh, ngay dd/MM/yyyy va so ngay con lai theo gio Viet Nam nhu C#)" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
cd /d/AIVanBanCaNhan && git add AIVanBan.Core/Services/DocumentAdvisoryService.cs AIVanBan.Tests/MauPromptTests.cs && git commit -m "test: sinh tep mau prompt tham-muu (BuildUserPrompt nhan now thay DateTime.Now, hanh vi khong doi)" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 4: Tác vụ `bao-cao-dinh-ky`

**Files:**
- Modify: `AIVanBan.Core/Services/PeriodicReportService.cs` (hai dòng khai báo hàm)
- Modify: `AIVanBan.Tests/MauPromptTests.cs` (thêm hàm `GhiMauBaoCaoDinhKy`)
- Create (sinh ra): `vanbanplus-api/tests/mau-prompt/bao-cao-dinh-ky.json`
- Create (sinh ra): `vanbanplus-api/src/lib/tac-vu/bao-cao-dinh-ky/he-thong.ts`
- Create: `vanbanplus-api/src/lib/tac-vu/bao-cao-dinh-ky/prompt.ts`, `.../bao-cao-dinh-ky/dinh-nghia.ts`
- Modify: `vanbanplus-api/src/lib/tac-vu/dang-ky.ts` (thêm 1 import + 1 mục)
- Test: `vanbanplus-api/tests/tac-vu-bao-cao-dinh-ky.test.ts`

**Interfaces:**
- Consumes: `scripts/trich-prompt-cs.mjs` (Task 2); `laDoiTuong`, `chuoiTuyChon` (`kiem-kieu.ts`, Task 2); `DinhNghiaTacVu`, `PromptTacVu` (khung).
- Produces:
  - `HE_THONG_BAO_CAO_DINH_KY: string`; `interface DauVaoBaoCaoDinhKy { loai_ky; ky; linh_vuc; don_vi; so_lieu; bao_cao_ky_truoc; nguoi_ky; chuc_danh: string }`; `dungPromptBaoCaoDinhKy(d: DauVaoBaoCaoDinhKy): PromptTacVu`; `baoCaoDinhKy: DinhNghiaTacVu<DauVaoBaoCaoDinhKy>` (`ten: "bao-cao-dinh-ky"`, `kieu_ket_qua: "van_ban"`).
  - C#: `internal static string PeriodicReportService.BuildSystemPrompt()`, `internal static string PeriodicReportService.BuildUserPrompt(string reportPeriodType, string reportPeriod, string field, string orgName, string rawData, string? previousReport, string signerName, string signerTitle)`.
  - Tệp mẫu: mảng `{ dau_vao: { loai_ky, ky, linh_vuc, don_vi, so_lieu, bao_cao_ky_truoc: string | null, nguoi_ky, chuc_danh }, he_thong, nguoi_dung }`.

- [ ] **Step 1: Mở hai hàm prompt C# cho bộ kiểm**

`AIVanBan.Core/Services/PeriodicReportService.cs`:
- `    private string BuildSystemPrompt()` → `    internal static string BuildSystemPrompt()`
- `    private string BuildUserPrompt(` → `    internal static string BuildUserPrompt(` (tham số giữ nguyên)

Đã kiểm: hai hàm chỉ dùng tham số; `ToLower()` theo văn hóa hiện hành (bộ sinh ép `vi-VN`).

- [ ] **Step 2: Thêm hàm sinh tệp mẫu C#**

Trong `AIVanBan.Tests/MauPromptTests.cs`, thêm **ngay trước dấu `}` đóng lớp `MauPromptTests`**:

````csharp
    [Fact]
    public void GhiMauBaoCaoDinhKy()
    {
        // Ép văn hóa vi-VN: BuildUserPrompt dùng reportPeriodType.ToLower() theo CurrentCulture.
        var vanHoaCu = System.Globalization.CultureInfo.CurrentCulture;
        System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("vi-VN");
        try
        {
            var cacMau = new (string LoaiKy, string Ky, string LinhVuc, string DonVi, string SoLieu, string? KyTruoc, string NguoiKy, string ChucDanh)[]
            {
                // Số liệu thường + có báo cáo kỳ trước.
                ("Tháng", "Tháng 10/2026", "Cải cách hành chính", "UBND xã Minh An",
                 "- Hồ sơ TTHC tiếp nhận: 412\n- Giải quyết đúng hạn: 398\n- Trễ hạn: 14",
                 "- Hồ sơ TTHC tiếp nhận: 380\n- Giải quyết đúng hạn: 361", "Nguyễn Văn A", "Chủ tịch"),
                // Gộp nhiều tệp + có bảng, không có kỳ trước, không người ký.
                ("Quý", "Quý III/2026", "Kinh tế - Xã hội", "UBND xã Minh An",
                 "===== FILE 1: BC thang 7.docx =====\nThu ngân sách: 1,2 tỷ đồng\n===== FILE 2: BC thang 8.docx =====\nThu ngân sách: 1,5 tỷ đồng\n[BẢNG]\nChỉ tiêu | Tháng 7 | Tháng 8\nHộ nghèo | 52 | 50\n[/BẢNG]",
                 null, "", ""),
                // Chỉ có bảng; kỳ trước chỉ khoảng trắng (coi là không có); ký tự đặc biệt cho template literal.
                ("Năm", "Năm 2026", "Chuyển đổi số", "Văn phòng HĐND và UBND",
                 "[BẢNG]\nChỉ tiêu | Kết quả\nHồ sơ trực tuyến | 85%\n[/BẢNG]\nGhi chú: dấu \\ , ký tự ``` và ${bien}.",
                 "   ", "Trần Thị B", "Phó Chủ tịch"),
                // Loại kỳ có số và chữ thường sẵn.
                ("6 tháng", "6 tháng đầu năm 2026", "Tư pháp - Hộ tịch", "UBND phường Bình Đa",
                 "Đăng ký khai sinh: 230 trường hợp\nChứng thực: 1.120 việc", null, "Lê Văn C", "Chủ tịch"),
            };

            var mau = cacMau.Select(m => new
            {
                dau_vao = new
                {
                    loai_ky = m.LoaiKy,
                    ky = m.Ky,
                    linh_vuc = m.LinhVuc,
                    don_vi = m.DonVi,
                    so_lieu = m.SoLieu,
                    bao_cao_ky_truoc = m.KyTruoc,
                    nguoi_ky = m.NguoiKy,
                    chuc_danh = m.ChucDanh,
                },
                he_thong = ChuanHoa(PeriodicReportService.BuildSystemPrompt()),
                nguoi_dung = ChuanHoa(PeriodicReportService.BuildUserPrompt(
                    m.LoaiKy, m.Ky, m.LinhVuc, m.DonVi, m.SoLieu, m.KyTruoc, m.NguoiKy, m.ChucDanh)),
            }).ToList();

            Ghi("bao-cao-dinh-ky.json", mau);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = vanHoaCu;
        }
    }
````

- [ ] **Step 3: Sinh tệp mẫu (chạy hai lần)**

```bash
cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests --filter "FullyQualifiedName~MauPromptTests.GhiMauBaoCaoDinhKy"
cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests --filter "FullyQualifiedName~MauPromptTests"
```
Expected: lần 1 FAIL «…bao-cao-dinh-ky.json vừa được sinh lại…»; lần 2 `Passed!` cho cả bốn hàm sinh.

- [ ] **Step 4: Sinh `he-thong.ts`**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && mkdir -p src/lib/tac-vu/bao-cao-dinh-ky && node scripts/trich-prompt-cs.mjs ../AIVanBan.Core/Services/PeriodicReportService.cs BuildSystemPrompt src/lib/tac-vu/bao-cao-dinh-ky/he-thong.ts HE_THONG_BAO_CAO_DINH_KY
```
Expected: `HE_THONG_BAO_CAO_DINH_KY: 1598 ký tự → …`.

- [ ] **Step 5: Viết bộ kiểm hỏng**

`vanbanplus-api/tests/tac-vu-bao-cao-dinh-ky.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { baoCaoDinhKy } from "../src/lib/tac-vu/bao-cao-dinh-ky/dinh-nghia";
import { layTacVu } from "../src/lib/tac-vu/dang-ky";

interface Mau {
  dau_vao: Record<string, unknown>;
  he_thong: string;
  nguoi_dung: string;
}
const MAU: Mau[] = JSON.parse(readFileSync(join(__dirname, "mau-prompt", "bao-cao-dinh-ky.json"), "utf8"));

function khacTai(a: string, b: string): string {
  let i = 0;
  while (i < a.length && a[i] === b[i]) i++;
  return `lệch tại ký tự ${i}: ${JSON.stringify(a.slice(i, i + 40))} ≠ ${JSON.stringify(b.slice(i, i + 40))}`;
}

test("prompt báo cáo định kỳ khớp từng ký tự với bản C#", () => {
  assert.ok(MAU.length >= 4);
  for (const m of MAU) {
    const dv = baoCaoDinhKy.kiemDauVao(m.dau_vao);
    assert.ok(dv.ok, "đầu vào mẫu phải hợp lệ");
    const p = baoCaoDinhKy.dungPrompt(dv.du_lieu, new Date());
    assert.equal(p.he_thong, m.he_thong, khacTai(p.he_thong, m.he_thong));
    assert.equal(p.nguoi_dung, m.nguoi_dung, khacTai(p.nguoi_dung, m.nguoi_dung));
  }
});

test("bộ mẫu phủ đủ nhánh: gộp nhiều tệp, có bảng, thường; có / không / rỗng báo cáo kỳ trước", () => {
  const sl = MAU.map((m) => m.dau_vao.so_lieu as string);
  assert.ok(sl.some((s) => s.includes("===== FILE ")));
  assert.ok(sl.some((s) => s.includes("[BẢNG]") && !s.includes("===== FILE ")));
  assert.ok(sl.some((s) => !s.includes("[BẢNG]") && !s.includes("===== FILE ")));
  const kt = MAU.map((m) => m.dau_vao.bao_cao_ky_truoc as string | null);
  assert.ok(kt.some((x) => x === null));
  assert.ok(kt.some((x) => typeof x === "string" && x.trim() !== ""));
  assert.ok(kt.some((x) => typeof x === "string" && x.trim() === ""));
});

test("đầu vào: năm trường bắt buộc khác rỗng; ba trường tùy chọn mặc định rỗng", () => {
  const du = { loai_ky: "Tháng", ky: "Tháng 10/2026", linh_vuc: "Cải cách hành chính", don_vi: "UBND xã Minh An", so_lieu: "412 hồ sơ" };
  assert.deepEqual(baoCaoDinhKy.kiemDauVao(du), {
    ok: true,
    du_lieu: { ...du, bao_cao_ky_truoc: "", nguoi_ky: "", chuc_danh: "" },
  });
  for (const k of Object.keys(du)) {
    const thieu = baoCaoDinhKy.kiemDauVao({ ...du, [k]: undefined });
    assert.ok(!thieu.ok && thieu.loi.includes(k), `thiếu ${k} phải báo lỗi nêu tên trường`);
    assert.equal(baoCaoDinhKy.kiemDauVao({ ...du, [k]: "  " }).ok, false);
    assert.equal(baoCaoDinhKy.kiemDauVao({ ...du, [k]: 5 }).ok, false);
  }
  assert.equal(baoCaoDinhKy.kiemDauVao({ ...du, nguoi_ky: 5 }).ok, false);
  assert.equal(baoCaoDinhKy.kiemDauVao({ ...du, bao_cao_ky_truoc: null, chuc_danh: null }).ok, true);
  assert.equal(baoCaoDinhKy.kiemDauVao(null).ok, false);
});

test("trả văn bản: không khai kiemKetQua (khung chỉ kiểm tác vụ JSON)", () => {
  assert.equal(baoCaoDinhKy.kieu_ket_qua, "van_ban");
  assert.equal(baoCaoDinhKy.kiemKetQua, undefined);
});

test("danh sách tác vụ có bao-cao-dinh-ky", () => {
  assert.equal(layTacVu("bao-cao-dinh-ky"), baoCaoDinhKy);
});
```

Chạy: `cd /d/AIVanBanCaNhan/vanbanplus-api && npx tsx --test tests/tac-vu-bao-cao-dinh-ky.test.ts` → FAIL (không có module).

- [ ] **Step 6: Viết `prompt.ts`**

`vanbanplus-api/src/lib/tac-vu/bao-cao-dinh-ky/prompt.ts` (chép logic `PeriodicReportService.BuildUserPrompt`; các dòng trong template literal bắt đầu ở cột 0, không thụt lề; `\\n` cuối là chữ `\n` nguyên văn như chuỗi `@"..."` của C#):

```ts
import type { PromptTacVu } from "../chay-tac-vu";
import { HE_THONG_BAO_CAO_DINH_KY } from "./he-thong";

export interface DauVaoBaoCaoDinhKy {
  loai_ky: string;
  ky: string;
  linh_vuc: string;
  don_vi: string;
  so_lieu: string;
  bao_cao_ky_truoc: string;
  nguoi_ky: string;
  chuc_danh: string;
}

/** «⚠️» đúng hai code point như chuỗi C# (U+26A0 U+FE0F). */
const CANH_BAO = "\u26A0\uFE0F";

/** Nguyên logic PeriodicReportService.BuildUserPrompt (bản cài máy). */
export function dungPromptBaoCaoDinhKy(d: DauVaoBaoCaoDinhKy): PromptTacVu {
  let p = `Hãy soạn BÁO CÁO ĐỊNH KỲ với thông tin sau:

ĐƠN VỊ: ${d.don_vi}
LOẠI BÁO CÁO: Báo cáo ${d.loai_ky.toLowerCase()}
KỲ BÁO CÁO: ${d.ky}
LĨNH VỰC: ${d.linh_vuc}
NGƯỜI KÝ: ${d.nguoi_ky}
CHỨC DANH: ${d.chuc_danh}

===== SỐ LIỆU HIỆN TẠI =====
${d.so_lieu}`;

  // Như C#: dữ liệu gộp từ nhiều tệp Word (tổng hợp tháng → quý), dữ liệu có bảng.
  const gopNhieuTep = d.so_lieu.includes("===== FILE ") && d.so_lieu.includes("=====");
  const coBang = d.so_lieu.includes("[BẢNG]");

  if (gopNhieuTep) {
    p += `

${CANH_BAO} DỮ LIỆU TRÊN ĐƯỢC GỘP TỪ NHIỀU FILE BÁO CÁO THÁNG.
Mỗi phần '===== FILE X: ... =====' là nội dung từ 1 file báo cáo tháng riêng.
NHIỆM VỤ QUAN TRỌNG:
1. Đọc hiểu NỘI DUNG từng báo cáo tháng
2. TỔNG HỢP số liệu cả kỳ (cộng gộp hoặc lấy trung bình tùy chỉ tiêu)
3. SO SÁNH xu hướng giữa các tháng (tăng/giảm)
4. Viết thành 1 báo cáo tổng hợp hoàn chỉnh cho cả kỳ`;
  }

  if (coBang) {
    p += `

${CANH_BAO} DỮ LIỆU CÓ CHỨA BẢNG BIỂU (giữa [BẢNG] và [/BẢNG]).
- Đọc dữ liệu từ bảng theo cấu trúc cột: dòng đầu là tiêu đề, các dòng sau là dữ liệu.
- Trích xuất số liệu từ bảng để tính toán và tổng hợp.
- KHÔNG copy nguyên bảng vào báo cáo — hãy VIẾT LẠI thành câu văn có số liệu.`;
  }

  if (d.bao_cao_ky_truoc.trim()) {
    p += `

===== BÁO CÁO KỲ TRƯỚC (để so sánh) =====
${d.bao_cao_ky_truoc}

LƯU Ý: Hãy SO SÁNH số liệu hiện tại với kỳ trước. Tính % tăng/giảm cho mỗi chỉ tiêu.`;
  } else {
    p += `

LƯU Ý: Không có số liệu kỳ trước. Chỉ trình bày số liệu hiện tại, không cần so sánh.`;
  }

  p += `

Hãy soạn NỘI DUNG THÂN BÁO CÁO (chỉ phần body, KHÔNG gồm header/quốc hiệu/chữ ký/nơi nhận).
Bắt đầu từ câu dẫn nhập, kết thúc bằng câu kết luận.
PLAIN TEXT thuần — KHÔNG dùng markdown — KHÔNG viết literal \\n.`;

  return { he_thong: HE_THONG_BAO_CAO_DINH_KY, nguoi_dung: p };
}
```

- [ ] **Step 7: Viết `dinh-nghia.ts`**

`vanbanplus-api/src/lib/tac-vu/bao-cao-dinh-ky/dinh-nghia.ts`:

```ts
import type { DinhNghiaTacVu } from "../chay-tac-vu";
import { chuoiTuyChon, laDoiTuong } from "../kiem-kieu";
import { dungPromptBaoCaoDinhKy, type DauVaoBaoCaoDinhKy } from "./prompt";

/** Trường bắt buộc (hợp đồng) và tên hiện trong lời báo lỗi. */
const BAT_BUOC = [
  ["loai_ky", "loại kỳ báo cáo"],
  ["ky", "kỳ báo cáo"],
  ["linh_vuc", "lĩnh vực"],
  ["don_vi", "tên đơn vị"],
  ["so_lieu", "số liệu"],
] as const;
const TUY_CHON = ["bao_cao_ky_truoc", "nguoi_ky", "chuc_danh"] as const;

export const baoCaoDinhKy: DinhNghiaTacVu<DauVaoBaoCaoDinhKy> = {
  ten: "bao-cao-dinh-ky",
  // Trả văn bản; CleanAIContent của bản cài máy vẫn làm ở phía máy (dot3).
  kieu_ket_qua: "van_ban",
  // Bản cài máy gọi /api/ai/generate không truyền temperature, máy chủ dùng 0,7.
  nhiet_do: 0.7,
  kiemDauVao(body) {
    const b: Record<string, unknown> = laDoiTuong(body) ? body : {};
    const d = {} as DauVaoBaoCaoDinhKy;
    for (const [k, ten] of BAT_BUOC) {
      const v = b[k];
      if (typeof v !== "string" || !v.trim()) return { ok: false, loi: `Thiếu ${ten} (${k}).` };
      d[k] = v;
    }
    for (const k of TUY_CHON) {
      const v = chuoiTuyChon(b[k]);
      if (v === undefined) return { ok: false, loi: `Trường ${k} phải là chuỗi.` };
      d[k] = v;
    }
    return { ok: true, du_lieu: d };
  },
  dungPrompt: (d) => dungPromptBaoCaoDinhKy(d),
};
```

- [ ] **Step 8: Đăng ký tác vụ**

`vanbanplus-api/src/lib/tac-vu/dang-ky.ts` — chỉ thêm:
- Dòng `import { baoCaoDinhKy } from "./bao-cao-dinh-ky/dinh-nghia";` ngay sau dòng import tác vụ cuối cùng đang có.
- Dòng `  [baoCaoDinhKy.ten]: baoCaoDinhKy,` ngay sau mục cuối cùng đang có trong `DANH_SACH`.

Sau Task 4, `DANH_SACH` có đúng bốn mục: `tom-tat`, `kiem-tra`, `tham-muu`, `bao-cao-dinh-ky`.

- [ ] **Step 9: Chạy toàn bộ, thấy qua**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npm test && npx tsc --noEmit -p .
```
Expected: mọi bộ kiểm pass (gồm `tac-vu-bao-cao-dinh-ky` 5 bài, `tac-vu-tham-muu` 6, `tac-vu-kiem-tra` 6, `tac-vu-tom-tat` 4, `bang-gia-dot2a` 6 và các bộ đợt 1), `skip 0`. Lệch prompt thì sửa `prompt.ts`, không sửa tệp mẫu.

- [ ] **Step 10: Commit (hai repo)**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git add src/lib/tac-vu/bao-cao-dinh-ky src/lib/tac-vu/dang-ky.ts tests/tac-vu-bao-cao-dinh-ky.test.ts tests/mau-prompt/bao-cao-dinh-ky.json && git commit -m "feat(tac-vu): bao-cao-dinh-ky (prompt chep tu C#, so khop tep mau)" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
cd /d/AIVanBanCaNhan && git add AIVanBan.Core/Services/PeriodicReportService.cs AIVanBan.Tests/MauPromptTests.cs && git commit -m "test: sinh tep mau prompt bao-cao-dinh-ky cho vanbanplus-api (ham prompt -> internal static)" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 5: Thử trên bản sao prod, migrate prod, đẩy `main`, chạy thật, đo proxy

**Files:**
- Modify: `D:\AIVanBanCaNhan\docs\superpowers\specs\2026-11-09-api-tac-vu-ai-credit-design.md` (dòng 3 trạng thái; §11 điểm 2)
- Modify: `D:\AIVanBanCaNhan\PROJECT_STATUS.md` (thêm 1 dòng bảng tính năng)
- Modify: `D:\Thang\gikidi-server\THAY-DOI.md` (thêm 1 dòng)

**Interfaces:**
- Consumes: nhánh `dot2a` (Task 1–4) với `npm test` xanh; `007_bang_gia_dot2a.sql`.
- Produces: ba tác vụ chạy trên `https://vanbanplus.giakiemso.com/api/v2/tac-vu/{kiem-tra,tham-muu,bao-cao-dinh-ky}`; số đo thời gian chờ dài nhất qua proxy ghi vào spec §11 điểm 2.

**Điểm dừng:** trước Step 3, báo chủ dự án một dòng: «Sắp thêm 3 dòng bảng giá vào CSDL prod VanBanPlus (007, chỉ INSERT) và đẩy main: lên 3 tác vụ v2 kiem-tra, tham-muu, bao-cao-dinh-ky; sau đó chạy thử thật bằng tài khoản quản trị (có thể cộng tay 150 credit thử) và đo một lượt kiem-tra 30.000 ký tự qua proxy». Làm tiếp khi đã được đồng ý (lệnh «làm đi» cho cả kế hoạch được tính là đồng ý, trừ khi chủ dự án dặn khác).

- [ ] **Step 1: Bộ kiểm đầy đủ + build trên nhánh**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git status --short && TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npm test && npx next build
```
Expected: `git status` sạch; mọi bộ kiểm pass, `skip 0`; build xong, danh sách route có `/api/v2/tac-vu/[ten]`.

- [ ] **Step 2: Thử migration trên bản sao prod**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && scp -q supabase/migrations/007_bang_gia_dot2a.sql gikidi-dev:/tmp/007_bang_gia_dot2a.sql
ssh gikidi-dev "set -e
  PROD=supabase-db-q323olywghtnjcvpyvtjzo9v
  sudo docker exec \$PROD pg_dump -U postgres -d postgres -n public --no-owner > /tmp/vbp-ban-sao.sql
  export PGPASSWORD=thu; P='psql -h 127.0.0.1 -p 55990 -U postgres -q'
  \$P -d postgres -c 'drop database if exists vbp_ban_sao with (force)' -c 'create database vbp_ban_sao'
  \$P -d vbp_ban_sao -f /tmp/vbp-ban-sao.sql >/dev/null 2>/tmp/vbp-nap.err || true
  echo 'Lỗi khi nạp bản sao:'; grep ERROR /tmp/vbp-nap.err | sort | uniq -c | head
  \$P -d vbp_ban_sao -Atc \"select loai, ma from credit_bang_gia where loai = 'tac_vu' order by ma\"
  \$P -d vbp_ban_sao -v ON_ERROR_STOP=1 -1 -f /tmp/007_bang_gia_dot2a.sql
  \$P -d vbp_ban_sao -v ON_ERROR_STOP=1 -1 -f /tmp/007_bang_gia_dot2a.sql
  \$P -d vbp_ban_sao -Atc \"select ma, gia_tri from credit_bang_gia where loai = 'tac_vu' order by ma\"
  \$P -d postgres -c 'drop database vbp_ban_sao with (force)'
  rm -f /tmp/vbp-ban-sao.sql /tmp/vbp-nap.err"
```
Expected: lỗi khi nạp chỉ ở phần không liên quan (vd `schema "public" already exists`, kiểu `vector`); trước 007 có `api-cu`, `tom-tat` (cộng giá trị prod đã chỉnh tay nếu có); 007 chạy hai lần không lỗi (lần 2 `INSERT 0 0`); sau đó có thêm `bao-cao-dinh-ky`, `kiem-tra`, `tham-muu` đúng số ở Task 1, `tom-tat` không đổi. Bản sao + tệp dump xóa ngay (có dữ liệu người dùng thật). Nếu `credit_bang_gia` không nạp được thì dừng, tìm nguyên nhân.

- [ ] **Step 3: Migrate prod (một giao dịch)**

```bash
ssh gikidi-dev "PROD=supabase-db-q323olywghtnjcvpyvtjzo9v
  sudo docker exec -i \$PROD psql -U postgres -d postgres -v ON_ERROR_STOP=1 -1 -q < /tmp/007_bang_gia_dot2a.sql &&
  sudo docker exec \$PROD psql -U postgres -d postgres -c \"NOTIFY pgrst, 'reload schema'\" &&
  sudo docker exec \$PROD psql -U postgres -d postgres -Atc \"select ma, gia_tri from credit_bang_gia where loai = 'tac_vu' order by ma\" &&
  rm -f /tmp/007_bang_gia_dot2a.sql"
```
Expected: năm dòng `api-cu`, `bao-cao-dinh-ky`, `kiem-tra`, `tham-muu`, `tom-tat`. Dòng mới chưa ai dùng tới khi mã lên (Step 4). Ghi một dòng vào `D:\Thang\gikidi-server\THAY-DOI.md` (cùng khuôn các dòng có sẵn): ngày giờ, «VanBanPlus: migrate 007_bang_gia_dot2a lên CSDL `supabase-db-q323…` (3 dòng `credit_bang_gia` cho kiem-tra, tham-muu, bao-cao-dinh-ky; chỉ INSERT)», cách lùi: `DELETE FROM credit_bang_gia WHERE loai = 'tac_vu' AND ma IN ('kiem-tra','tham-muu','bao-cao-dinh-ky');` (chỉ sau khi mã đã lùi). Không có bảng / CSDL mới nên đường sao lưu không đổi.

- [ ] **Step 4: Gộp và đẩy `main` (lên production)**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git checkout main && git pull --ff-only && git merge --no-ff dot2a -m "Merge dot2a: tac vu v2 kiem-tra, tham-muu, bao-cao-dinh-ky" && TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npm test && npx next build && git push origin main && git rev-parse HEAD
```
Expected: đẩy xong; ghi lại mã commit gộp (dùng cho kịch bản lùi). Theo dõi Coolify: `python D:\Thang\gikidi-server\cong-cu\tu-trien-khai.py xem` (hoặc chờ 3–5 phút). Mã mới đã lên khi:

```bash
curl -s -o /dev/null -w "%{http_code}\n" -X POST https://vanbanplus.giakiemso.com/api/v2/tac-vu/kiem-tra
```
trả `401` (mã cũ trả `404` vì chưa có tác vụ). Sau đó `bash scripts/csdl-thu.sh tat` và tắt đường hầm SSH chạy nền.

- [ ] **Step 5: Chuẩn bị tài khoản thử (không in khóa)**

```bash
ssh gikidi-dev 'PROD=supabase-db-q323olywghtnjcvpyvtjzo9v
  Q() { sudo docker exec $PROD psql -U postgres -d postgres -Atc "$1"; }
  ID=$(Q "select id from users where role = '"'"'admin'"'"' order by created_date limit 1")
  echo "So du hien co: $(Q "select (credit_so_du('"'"'$ID'"'"'))->>'"'"'tong'"'"'")"'
```
Nếu số dư dưới 150: cộng tay một lô điều chỉnh (ghi sổ `dieu_chinh`, hạn 90 ngày; báo chủ dự án trong tin cuối):

```bash
ssh gikidi-dev 'PROD=supabase-db-q323olywghtnjcvpyvtjzo9v
  Q() { sudo docker exec $PROD psql -U postgres -d postgres -Atc "$1"; }
  ID=$(Q "select id from users where role = '"'"'admin'"'"' order by created_date limit 1")
  Q "select credit_dieu_chinh('"'"'$ID'"'"', 150, '"'"'Thu that dot2a: kiem-tra, tham-muu, bao-cao-dinh-ky, do proxy'"'"', '"'"'claude-dot2a'"'"')"'
```
Expected: `{"so_du": …, "da_dieu_chinh": 150}`.

- [ ] **Step 6: Chạy thật mỗi tác vụ một lượt**

```bash
ssh gikidi-dev 'bash -s' <<'SSH'
set -u
PROD=supabase-db-q323olywghtnjcvpyvtjzo9v
Q() { sudo docker exec $PROD psql -U postgres -d postgres -Atc "$1"; }
K=$(Q "select api_key from users where role = 'admin' order by created_date limit 1")
U=https://vanbanplus.giakiemso.com
mkdir -p /tmp/vbp-dot2a && cd /tmp/vbp-dot2a
python3 - <<'PY'
import json
def ghi(ten, d): json.dump(d, open(ten + ".json", "w", encoding="utf-8"), ensure_ascii=False)
ghi("kiem-tra", {
  "noi_dung": "ỦY BAN NHÂN DÂN XÃ MINH AN\nSố: 15/QĐ-UBND\nQUYẾT ĐỊNH\nVề việc thành lập Tổ công tác chuyển đổi số\nĐiều 1. Thành lập Tổ công tác chuyển đổi số xã gồm 05 thành viên.\nĐiều 2. Giao Văn phòng theo dõi, xử lý nghiêm.\nNơi nhận:\n- Như Điều 2;",
  "loai_van_ban": "Quyết định", "tieu_de": "Về việc thành lập Tổ công tác chuyển đổi số", "co_quan": "UBND xã Minh An",
  "van_ban_doi_chieu": "QUYẾT ĐỊNH\nĐiều 1. Thành lập Tổ công tác.\nĐiều 2. Nhiệm vụ.\nĐiều 3. Chánh Văn phòng và các cá nhân có tên tại Điều 1 chịu trách nhiệm thi hành Quyết định này.\nNơi nhận:\n- Như Điều 3;\n- Lưu: VT, VP."})
ghi("tham-muu", {
  "noi_dung": "Kính gửi: UBND các xã, phường.\nĐề nghị rà soát quỹ đất công trên địa bàn, báo cáo kết quả về Sở Nông nghiệp và Môi trường trước ngày 30/11/2026.",
  "loai_van_ban": "Công văn", "tieu_de": "V/v rà soát quỹ đất công", "co_quan": "UBND tỉnh Đồng Nai",
  "ngu_canh": {"huong": "Đến", "so_van_ban": "123/UBND-KT", "ngay_ban_hanh": "2026-11-10", "do_khan": "Khẩn", "han_xu_ly": "2026-11-30", "noi_nhan": ["UBND các xã, phường"], "can_cu": ["Luật Đất đai 2024"]}})
ghi("bao-cao-dinh-ky", {
  "loai_ky": "Tháng", "ky": "Tháng 10/2026", "linh_vuc": "Cải cách hành chính", "don_vi": "UBND xã Minh An",
  "so_lieu": "- Hồ sơ TTHC tiếp nhận: 412\n- Giải quyết đúng hạn: 398\n- Trễ hạn: 14\n- Hồ sơ trực tuyến toàn trình: 236",
  "bao_cao_ky_truoc": "- Hồ sơ TTHC tiếp nhận: 380\n- Giải quyết đúng hạn: 361\n- Hồ sơ trực tuyến toàn trình: 190",
  "nguoi_ky": "Nguyễn Văn A", "chuc_danh": "Chủ tịch"})
PY
for t in kiem-tra tham-muu bao-cao-dinh-ky; do
  echo "== $t uoc-tinh: $(curl -s -H "X-API-Key: $K" -H "Content-Type: application/json" --data-binary @$t.json $U/api/v2/tac-vu/$t/uoc-tinh)"
  curl -s -o $t.ra -w "== $t chay: HTTP %{http_code}, %{time_total}s\n" -H "X-API-Key: $K" -H "X-VBP-Nguon: kiem-thu" -H "X-VBP-Phien-Ban: dot2a" -H "Content-Type: application/json" --data-binary @$t.json $U/api/v2/tac-vu/$t
  python3 - "$t.ra" <<'PY'
import json, sys
r = json.load(open(sys.argv[1], encoding="utf-8"))
d = r.get("data") or {}
kq = d.get("ket_qua")
mo_ta = sorted(kq.keys()) if isinstance(kq, dict) else f"chuỗi {len(kq or '')} ký tự"
print("   success", r.get("success"), "| code", r.get("code"), "| credit_da_tru", d.get("credit_da_tru"),
      "| so_du", d.get("so_du"), "| bi_cat", d.get("bi_cat"), "| ket_qua", mo_ta)
PY
done
cd / && rm -rf /tmp/vbp-dot2a
SSH
```
Expected cho mỗi tác vụ: `uoc-tinh` trả `success: true`, `credit_min ≤ credit_max`, `du_credit: true`; lượt chạy `HTTP 200`, `success True`, `credit_da_tru` trong khoảng ước tính. `kiem-tra`: `ket_qua` có `issues`, `overall_score`, `summary`, `suggested_content`. `tham-muu`: có `summary`, `action_items`, `deadlines`, `response_needed`… `bao-cao-dinh-ky`: `ket_qua` là chuỗi vài nghìn ký tự. Nếu mô hình `gemini-3.6-flash` lỗi trên Vertex: đổi `cau_hinh/chung.mo_hinh_mac_dinh` về `gemini-3.5-flash` ở `/admin/credit`, ghi lại, báo chủ dự án.

- [ ] **Step 7: Kiểm `tac_vu_chay` và sổ cái**

```bash
ssh gikidi-dev 'PROD=supabase-db-q323olywghtnjcvpyvtjzo9v
  P="sudo docker exec $PROD psql -U postgres -d postgres"
  $P -c "select tac_vu, trang_thai, ma_loi, credit_min, credit_max, credit_tru, token_vao, token_ra, round(extract(epoch from xong_luc - bat_dau_luc)) as giay from tac_vu_chay where nguon_goi = '"'"'kiem-thu'"'"' and bat_dau_luc > now() - interval '"'"'2 hours'"'"' order by bat_dau_luc"
  $P -c "select t.tac_vu, t.credit_tru, -coalesce(sum(s.so), 0) as so_cai from tac_vu_chay t left join credit_so s on s.luot_id = t.id where t.nguon_goi = '"'"'kiem-thu'"'"' and t.bat_dau_luc > now() - interval '"'"'2 hours'"'"' group by t.id, t.tac_vu, t.credit_tru order by t.tac_vu"
  $P -Atc "select count(*) from (select l.id from credit_lo l left join credit_so s on s.lo_id = l.id where l.user_id = (select id from users where role = '"'"'admin'"'"' order by created_date limit 1) group by l.id, l.so_con having l.so_con <> coalesce(sum(s.so), 0)) x"'
```
Expected: ba dòng `xong`, `ma_loi` rỗng, `credit_min ≤ credit_tru ≤ credit_max`, `token_vao`/`token_ra` > 0; mỗi lượt `so_cai = credit_tru` (giữ − hoàn − trừ cộng lại đúng bằng phần trừ); số lô lệch sổ cái = `0`. Ghi lại token thật của ba lượt để so với hệ số ở Task 1 (vd `kiem-tra` token_ra / token_vao).

- [ ] **Step 8: Đo lượt dài nhất qua proxy (spec §11 điểm 2): `kiem-tra` khoảng 30.000 ký tự**

```bash
ssh gikidi-dev 'bash -s' <<'SSH'
set -u
PROD=supabase-db-q323olywghtnjcvpyvtjzo9v
Q() { sudo docker exec $PROD psql -U postgres -d postgres -Atc "$1"; }
K=$(Q "select api_key from users where role = 'admin' order by created_date limit 1")
U=https://vanbanplus.giakiemso.com
mkdir -p /tmp/vbp-dai && cd /tmp/vbp-dai
echo "== Đường đi: $(curl -sI $U/api/health | grep -iE '^(server|cf-ray|via):' | tr -d '\r' | tr '\n' ' ')"
echo "== Hạn chờ của coolify-proxy: $(sudo docker inspect coolify-proxy --format '{{json .Args}}' | tr ',' '\n' | grep -i timeout | tr '\n' ' ')"
python3 - <<'PY'
import json
dau = "ỦY BAN NHÂN DÂN XÃ MINH AN\nSố: 15/QĐ-UBND\nMinh An, ngày 10 tháng 11 năm 2026\nQUYẾT ĐỊNH\nBan hành Quy chế làm việc của Ủy ban nhân dân xã Minh An\n"
dieu = ("Điều {n}. Quy định về nhiệm vụ thứ {n}\n"
        "1. Văn phòng - Thống kê chủ trì, phối hợp với các công chức chuyên môn tham mưu Ủy ban nhân dân xã thực hiện nhiệm vụ số {n} theo đúng quy định của pháp luật, bảo đảm tiến độ và chất lượng.\n"
        "2. Các bộ phận liên quan có trách nhiệm phối hợp, cung cấp thông tin, số liệu trong thời hạn 05 ngày làm việc kể từ ngày nhận được đề nghị; trường hợp chậm trễ thì xử lý nghiêm.\n"
        "3. Kinh phí thực hiện từ nguồn ngân sách xã năm 2026 và các nguồn hợp pháp khác.\n")
t, n = dau, 1
while len(t) < 30000:
    t += dieu.format(n=n); n += 1
t += f"Điều {n}. Hiệu lực thi hành\nQuyết định này có hiệu lực kể từ ngày ký.\nNơi nhận:\n- Như Điều {n};\n- Lưu: VT."
json.dump({"noi_dung": t, "loai_van_ban": "Quyết định", "tieu_de": "Ban hành Quy chế làm việc", "co_quan": "UBND xã Minh An"},
          open("kt-dai.json", "w", encoding="utf-8"), ensure_ascii=False)
print("== Độ dài noi_dung:", len(t), "ký tự")
PY
echo "== uoc-tinh: $(curl -s -H "X-API-Key: $K" -H "Content-Type: application/json" --data-binary @kt-dai.json $U/api/v2/tac-vu/kiem-tra/uoc-tinh)"
curl -s -o kt-dai.ra --max-time 900 -w "== chay: HTTP %{http_code} | tong %{time_total}s | byte dau %{time_starttransfer}s\n" -H "X-API-Key: $K" -H "X-VBP-Nguon: kiem-thu" -H "X-VBP-Phien-Ban: dot2a-do-proxy" -H "Content-Type: application/json" --data-binary @kt-dai.json $U/api/v2/tac-vu/kiem-tra
echo "== curl thoát mã $?"
python3 - <<'PY'
import json
try:
    r = json.load(open("kt-dai.ra", encoding="utf-8"))
except Exception as e:
    print("== phản hồi không phải JSON:", repr(open("kt-dai.ra", encoding="utf-8", errors="replace").read(300)), e)
else:
    d = r.get("data") or {}
    kq = d.get("ket_qua")
    if isinstance(kq, dict):
        print("== success", r.get("success"), "| bi_cat", d.get("bi_cat"), "| credit_da_tru", d.get("credit_da_tru"),
              "| issues", len(kq.get("issues") or []), "| suggested_content", len(kq.get("suggested_content") or ""), "ký tự")
    else:
        print("== success", r.get("success"), "| code", r.get("code"), "| bi_cat", d.get("bi_cat"), "| ket_qua chuỗi", len(kq or ""))
PY
Q "select trang_thai, ma_loi, credit_min, credit_max, credit_tru, token_vao, token_ra, round(extract(epoch from xong_luc - bat_dau_luc)) from tac_vu_chay where nguon_goi = 'kiem-thu' order by bat_dau_luc desc limit 1"
cd / && rm -rf /tmp/vbp-dai
SSH
```

Đọc kết quả (ghi lại cả năm số: độ dài, HTTP, `time_total`, giây phía máy chủ `xong_luc − bat_dau_luc`, `bi_cat`):

| Thấy | Kết luận |
| --- | --- |
| `HTTP 200`, `time_total` ≈ giây phía máy chủ, `trang_thai = xong` | Proxy không cắt ở thời lượng này. |
| `HTTP 524` sau ~100 giây, dòng «Đường đi» có `server: cloudflare` / `cf-ray` | Cloudflare cắt ở 100 giây (đã biết, xem `D:\Thang\gikidi-server\docs\plans\2026-10-03-an-ninh.md`). Chạy lại một lần qua thẳng Traefik để tách nguyên nhân: cùng lệnh `curl` thêm `--resolve vanbanplus.giakiemso.com:443:127.0.0.1`. |
| `HTTP 504` hoặc `curl` thoát mã 52 / 56 (không có `cf-ray`) | Traefik `coolify-proxy` cắt; ghi thời điểm cắt và dòng «Hạn chờ của coolify-proxy». |
| `tac_vu_chay` là `loi` + `NGAT_KET_NOI` | Máy chủ phát hiện mất kết nối, đã hoàn credit. Là `xong` mà phía gọi không nhận được: lượt vẫn bị trừ (spec §14). |
| `bi_cat: true` | Trần token ra 1,5 × vào + 5.000 vẫn thiếu cho 30.000 ký tự: ghi `token_ra` thật, đề xuất tăng `ra_max_he_so` ở `/admin/credit` (không sửa mã). |

Nếu proxy cắt: **không** sửa proxy hay tên miền trong kế hoạch này (đổi cấu hình máy chủ dùng chung). Ghi số đo, báo chủ dự án các lựa chọn: tắt proxy Cloudflare cho `vanbanplus.giakiemso.com` (`cong-cu/cf-proxy.py tat`), nới hạn chờ Traefik, hoặc đưa streaming / hàng đợi lên trước khi mở bán (spec §11 điểm 2, §14).

Sau khi đo, chạy lại truy vấn sổ cái ở Step 7 (`so_cai = credit_tru` cho mọi lượt `kiem-thu`, số lô lệch = 0).

- [ ] **Step 9: Cập nhật tài liệu, commit**

- Spec `docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md`:
  - dòng 3: thêm vào cuối câu trạng thái `; đợt 2a (kiem-tra, tham-muu, bao-cao-dinh-ky) lên production <ngày>.` (giữ nguyên phần đang có).
  - §11 «Chạy thật», điểm 2: thay câu bắt đầu bằng `2. Chưa đo:` bằng `2. Đã đo <ngày>: kiem-tra <độ dài> ký tự qua <đường đi: Cloudflare + Traefik / chỉ Traefik>, máy chủ chạy <giây> giây, phía gọi nhận <HTTP> sau <time_total> giây, <không bị cắt / bị cắt ở X giây bởi Y>; bi_cat <true/false>. <Nếu bị cắt: hướng xử lý chủ dự án chọn, hoặc «chờ chủ dự án chọn»>.` (điền số đo thật).
- `PROJECT_STATUS.md`: thêm dòng ngay dưới dòng `| ✅ | Credit + API tác vụ v2 (đợt 1, 12/11/2026) | …`, cùng khuôn: `| ✅ | API tác vụ v2 đợt 2a (<ngày>) | vanbanplus-api: migration 007 (bảng giá), /api/v2/tac-vu/kiem-tra, /tham-muu, /bao-cao-dinh-ky | Prompt chép nguyên văn từ C#, so khớp tệp mẫu. Trần token ra kiem-tra tăng theo độ dài. Đo proxy: <kết quả một vế>. Còn trich-xuat, soan-thao (đợt 2b), gói nạp + lưu hỗ trợ (đợt 2c), bản cài máy gọi v2 (đợt 3) |`. Nếu tệp đang có thay đổi dở của phiên khác: chỉ thêm dòng của mình; không chắc thì hỏi trước khi commit.

```bash
cd /d/AIVanBanCaNhan && git add docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md PROJECT_STATUS.md vanbanplus-api && git commit -m "docs: dot 2a (kiem-tra, tham-muu, bao-cao-dinh-ky) len production; so do proxy; cap nhat vanbanplus-api" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```
(Không đẩy repo `van-ban-plus`; repo này không tự triển khai.)

Tin báo chủ dự án (ngắn, không đại từ nhân xưng): ba tác vụ đã lên, credit đã trừ từng lượt thử, số credit đã cộng tay (nếu có), số đo proxy và kết luận.

#### Kịch bản lùi

1. **Mã lỗi trên production** (tác vụ mới trả 500, hoặc ảnh hưởng `tom-tat`): `cd /d/AIVanBanCaNhan/vanbanplus-api && git checkout main && git pull --ff-only && git revert -m 1 <mã commit gộp ở Step 4> --no-edit && npm test && git push origin main`. Coolify dựng lại bản trước; ba tác vụ trở về `404`. Lượt đang chạy dở được `credit_quet_treo` hoàn sau 15 phút.
2. **Dòng bảng giá:** để nguyên cũng vô hại (không mã nào đọc khi tác vụ chưa đăng ký). Muốn xóa: chỉ **sau** khi bước 1 đã lên, `DELETE FROM credit_bang_gia WHERE loai = 'tac_vu' AND ma IN ('kiem-tra','tham-muu','bao-cao-dinh-ky');` bằng `psql -1` như Step 3. Xóa khi mã mới còn chạy thì `chuanBi` ném «Bảng giá thiếu tác vụ» → 500 (không giữ credit nào).
3. **Giá sai** (thu quá nhiều / quá ít): sửa dòng ở `/admin/credit`, có hiệu lực sau tối đa 60 giây (bộ nhớ bảng giá), không cần triển khai lại.
4. **Lượt thử bị trừ sai:** `credit_dieu_chinh(<user>, <số dương>, '<lý do>', 'claude-dot2a')` (ghi sổ `dieu_chinh`).
5. **Phía C#:** các commit chỉ đổi phạm vi truy cập hàm, thêm tham số `now` (gọi với `DateTime.Now`) và thêm hàm sinh mẫu — hành vi bản cài máy không đổi, không cần lùi; nếu cần: `git revert <mã>` từng commit ở `D:\AIVanBanCaNhan`.

---

## Tự soát kế hoạch so với spec và hợp đồng

| Yêu cầu | Task |
| --- | --- |
| §5 ba tác vụ, đầu vào / đầu ra; hợp đồng tên trường, `ket_qua` theo model C# | 2, 3, 4 (`kiemDauVao`, `kiemKetQua`) |
| §5 thời điểm hiện tại truyền vào hàm dựng prompt | 3 (C# `now`, TS `bayGio`) |
| Hợp đồng: ngày ISO, máy chủ định dạng giống C# | 3 (`ngayKieuCs`, `soNgayConLai`, văn hóa `vi-VN`) |
| Hợp đồng: chuỗi tùy chọn thiếu / null / "" là không có | 2–4 (`chuoiTuyChon`, kiểm `trim()` như `IsNullOrWhiteSpace`) |
| §6 mã lỗi `DAU_VAO_SAI`, `QUA_DAI`, … | khung có sẵn; lời lỗi đầu vào ở 2–4, `toi_da_ky_tu` ở 1 |
| §7 bảng giá từ chi phí đo được, hệ số `ra_min` / `ra_max` | 1 |
| §7, §10 trần token ra `kiem-tra` tăng theo độ dài | 1 (hệ số 1,5), 2 (bài kiểm `maxTokens` qua `chayTacVu`) |
| §10 chép prompt sang TypeScript + bộ kiểm so khớp | 2, 3, 4 (script trích + tệp mẫu C#) |
| §10 thay lời gọi C#, xóa prompt C# | `dot3` (ngoài phạm vi) |
| §11 so khớp prompt từng ký tự | 2, 3, 4 |
| §11 chạy thật điểm 2 (proxy, tác vụ dài) | 5 Step 8 |
| §12 migrate trước khi đẩy mã; build + bộ kiểm xanh | 5 Step 2–4 |
| §14 người gọi mất kết nối | 5 Step 8 (đọc `NGAT_KET_NOI` / `xong`) |
| Brief: tệp dùng chung chỉ thêm; 007 chỉ INSERT … ON CONFLICT DO NOTHING; kịch bản lùi | 1–5, «Kịch bản lùi» |
