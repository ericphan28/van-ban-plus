# Đợt B — Tổ chức thuê VanBanPlus, phần web: kế hoạch thực hiện

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or
> superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Thêm vào `vanbanplus-web` (nhánh `dot-b-to-chuc`, chưa lên prod) ngữ cảnh «Cá nhân / Tổ chức X», phân hệ Tổ chức,
phân hệ «Việc được giao», trang nhận lời mời, khối «Giao xử lý» trong chi tiết văn bản, Mua cho tổ chức — rồi chụp
Playwright + quay video một vòng thao tác để chủ dự án duyệt.

**Architecture:** Một biến ngữ cảnh ở `src/api/khach.ts` (lớp gọi API duy nhất) gắn header `X-VBP-To-Chuc` vào mọi lời gọi
(trừ `/api/v2/phien/*`) nên mọi phân hệ cũ tự làm việc trên kho / ví / Cơ quan của tổ chức. `NguCanhProvider` đọc danh sách
tổ chức sau khi đăng nhập, khôi phục lựa chọn từ `localStorage` theo `uid`, đổi ngữ cảnh thì gắn lại toàn bộ cây trang
(`Routes key`) và xóa bộ đệm cấp mô-đun. Quyền hiển thị đọc từ dữ liệu API (`vai_tro`, `sua_duoc`, `quyen.giao`) — web
không tự suy luật quyền. Làm song song với API nhờ API giả `cong-cu/api-gia-to-chuc.mjs` theo đúng hợp đồng.

**Tech Stack:** React 18 + React Router 7 + Vite 6 + os-kit (không sửa `src/os-kit/`), Vitest + Testing Library, Playwright
1.65 (chụp + `recordVideo`).

**Spec:** `docs/superpowers/specs/2026-11-19-dot-b-to-chuc-design.md` (§3, §5, §6) · **hợp đồng API**: mục «Hợp đồng API cho
web» trong `docs/superpowers/plans/2026-11-19-dot-b-to-chuc-api.md` (nguồn duy nhất cho đường, thân gửi, `data` trả, mã lỗi)
· luật web: `vanbanplus-web/CLAUDE.md`, hợp đồng đợt A `docs/superpowers/plans/2026-11-17-hop-dong-web-dot-a.md` §6.

## Global Constraints

- Repo `vanbanplus-web`, nhánh **`dot-b-to-chuc`**, worktree riêng `D:/AIVanBanCaNhan/_wt-b-to-chuc-web` (tạo ở Task 1;
  `node_modules` là junction sang `D:/AIVanBanCaNhan/vanbanplus-web/node_modules`). Không sửa tệp ở repo gốc.
- **TUYỆT ĐỐI không đẩy `main`, không merge vào `main`** (đẩy `main` = production). Được `git push -u origin dot-b-to-chuc`.
  **Không có cờ, không có lối vào prod**: mọi thứ chỉ nằm trên nhánh; không đổi `VITE_API_GOC`, không trỏ web thật vào API thử.
- Mọi lời gọi API qua `src/api/khach.ts`; không `fetch` rải rác. **Không sửa `src/os-kit/`** — chỗ đặt nút ngữ cảnh là prop
  `phaiMenu` có sẵn của `OsShell`.
- Phân hệ mới = thư mục `src/phan-he/<ma>/` + **một dòng** trong `src/ung-dung.ts`.
- Chữ giao diện: tiếng Việt có dấu đầy đủ, **không đại từ nhân xưng** (tôi, chúng tôi, ta, chúng ta, anh, chị, em, bạn,
  mình), không «dạ / vâng / ạ». Tên phân hệ trong spec «Việc của tôi» → **«Việc được giao»**.
- Bộ kiểm cũ phải xanh **không sửa**, trừ `src/ung-dung.test.ts` (danh sách phân hệ đổi theo spec — Task 6 và 9 sửa đúng
  dòng danh sách).
- Lệnh: `npm test` (Vitest), `npm run build` (`tsc -b` có `noUnusedLocals` + Vite), `npm run chup` (Playwright, API giả).
- Commit: thông điệp ASCII không dấu theo lệ repo, dòng cuối `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Không in, không commit khóa / token / mã mời thật; mã mời trong API giả là chuỗi giả 43 ký tự.
- Task cuối: chụp 1440 + 390 (sáng + tối) mọi màn mới + quay video một vòng thao tác bằng `recordVideo`, **tự xem từng ảnh**
  trước khi giao.

## Quyết định khi viết kế hoạch

| Mục | Quyết định | Lý do |
| --- | --- | --- |
| Nút «Cá nhân / Tổ chức X» | Phần tử của web truyền qua `phaiMenu` của `OsShell`; ẩn khi tài khoản chưa thuộc tổ chức nào | os-kit có chỗ đặt sẵn; không sửa os-kit |
| Đổi ngữ cảnh | Về màn hình chính (`/`) và gắn lại cây trang (`<Routes key={ngữ cảnh}>`) | Không để trang đang mở (văn bản của kho cũ) hiện dưới ngữ cảnh mới |
| Bị bỏ khỏi tổ chức đang chọn | `khach.ts` gặp `KHONG_THUOC_TO_CHUC` thì tự về cá nhân | Không phải đối chiếu danh sách liên tục |
| Trang `/loi-moi` | Đọc `?ma=` rồi **xóa khỏi thanh địa chỉ ngay** (`history.replaceState`); chưa đăng nhập thì giữ mã ở `sessionStorage`, nút «Đăng nhập để nhận» | Mã không nằm lại trong lịch sử / Referer / tham số `?ve=` của trang đăng nhập |
| Cơ quan ở ngữ cảnh tổ chức | `useCoQuan` khóa bộ đệm theo `uid + tổ chức`; mục Cơ quan trong Cài đặt chỉ đọc nếu không phải quản trị | Soạn thảo / Quét / Báo cáo tự dùng hồ sơ tổ chức (API đọc header) |
| Mua cho tổ chức | Trong phân hệ Mua: ngữ cảnh tổ chức hiện bộ chọn riêng (Gói tổ chức · Credit AI · Dung lượng); không phải quản trị chỉ thấy lời báo | Spec §5; giá + `dang_ban` từ API, gói chưa bán hiện «Sắp mở bán» |
| Video thao tác | Ba đoạn `recordVideo` (quản trị → văn thư → cán bộ) dùng chung một kho API giả có trạng thái; ghép bằng `ffmpeg` nếu máy có, không thì giao ba tệp | Ba người dùng khác nhau = ba ngữ cảnh trình duyệt |

## Kịch bản tấn công (phía web)

| Bề mặt | Lạm dụng | Chặn (task) |
| --- | --- | --- |
| Header ngữ cảnh | Ngữ cảnh cũ lọt sang tài khoản khác cùng máy | `datPhien(null)` / đổi `uid` ⇒ về cá nhân; `localStorage` khóa theo `uid`; khôi phục chỉ khi id có trong danh sách API trả (T1, T3) |
| | Gửi header tổ chức tới route phiên (đăng nhập / làm mới) | Không gắn cho `/api/v2/phien/*` (T1) |
| Mã mời trong URL | Lộ qua lịch sử trình duyệt, Referer, tham số `?ve=` | Xóa `?ma=` khỏi địa chỉ ngay khi đọc; giữ ở `sessionStorage` tới khi nhận xong; trang không có liên kết ngoài (T8) |
| Hiển thị theo vai trò | Cán bộ bấm nút ẩn bằng cách sửa DOM | Web chỉ ẩn cho gọn; API trả 403 / 404 — web hiện câu báo của API (T10, T11) |
| Bộ đệm cấp mô-đun | Cơ quan / bản nháp của ngữ cảnh trước hiện ở ngữ cảnh sau | Khóa bộ đệm theo ngữ cảnh; bản nháp soạn thảo xóa khi đổi ngữ cảnh (T5) |
| Trang Mua | Sửa giá / tạo đơn cho tổ chức khi không phải quản trị | Web chỉ gửi mã gói + `to_chuc_id`; API kiểm quản trị + giá (T11) |
| Nội dung do người dùng nhập (tên tổ chức, ghi chú, ý kiến chỉ đạo) | Chèn HTML / script | Dựng bằng React (chữ thuần), bài kiểm hiện nguyên `<script>` (T7, T10) |

## Cấu trúc tệp

| Tệp | Việc |
| --- | --- |
| `src/api/khach.ts` | Ngữ cảnh tổ chức: `datToChuc`, `toChucHienTai`, `khiDoiToChuc`, gắn header; tự về cá nhân khi `KHONG_THUOC_TO_CHUC`; tùy chọn `toChuc` của một lời gọi |
| `src/api/to-chuc.ts` (mới) | Kiểu theo hợp đồng + `toChuc.*`, `giaoViec.*` |
| `src/api/kho.ts`, `src/api/loai.ts`, `src/api/phien.ts`, `src/api/mua.ts` | `VanBan.sua_duoc/cong_khai_noi_bo`, `DungLuong.het_goi`, `coQuan(tc?)`, đơn tổ chức |
| `src/to-chuc/NguCanh.tsx` (mới) | `NguCanhProvider`, `useNguCanh`, `useToChucId` |
| `src/to-chuc/ChonNguCanh.tsx`, `src/to-chuc/ngu-canh.css` (mới) | Nút + menu «Cá nhân / Tổ chức X» |
| `src/to-chuc/TrangLoiMoi.tsx` (mới) | Trang `/loi-moi` |
| `src/App.tsx`, `src/vo/Vo.tsx` | Gắn provider, `Routes key`, route `/loi-moi`, `phaiMenu`, chờ đọc ngữ cảnh |
| `src/phan-he/soan-thao/co-quan.ts`, `ban-soan.ts`, `CoQuanCaiDat.tsx` | Bộ đệm theo ngữ cảnh |
| `src/phan-he/to-chuc/*` (mới) | Phân hệ Tổ chức: danh sách + tạo, hồ sơ + Cơ quan, thành viên + lời mời, gói |
| `src/phan-he/viec/*` (mới) | Phân hệ «Việc được giao» |
| `src/phan-he/van-ban/KhoiGiaoViec.tsx`, `giao-viec.css` (mới), `ChiTiet.tsx`, `DaiChiDoc.tsx`, `VanBan.tsx` | Khối «Giao xử lý», công khai, nút theo `sua_duoc`, dải hết gói |
| `src/phan-he/mua/ChonMuaToChuc.tsx` (mới), `ChonMua.tsx`, `Mua.tsx` | Mua cho tổ chức |
| `src/ung-dung.ts`, `src/ung-dung.test.ts` | Hai dòng phân hệ mới |
| `cong-cu/api-gia-to-chuc.mjs` (mới), `cong-cu/api-gia.mjs`, `cong-cu/api-gia-mua.mjs` | API giả tổ chức (có trạng thái cho video) |
| `cong-cu/man-to-chuc.mjs` (mới), `cong-cu/chup.mjs` | Màn chụp mới |
| `cong-cu/quay-to-chuc.mjs` (mới) | Quay video một vòng |

---

### Task 1: Worktree + ngữ cảnh tổ chức trong `khach.ts`

**Files:**
- Modify: `src/api/khach.ts`
- Test: `src/api/ngu-canh.test.ts` (mới)

**Interfaces:**
- Produces: `datToChuc(id: string | null): void`, `toChucHienTai(): string | null`,
  `khiDoiToChuc(fn: (id: string | null) => void): () => void`; `TuyChonGoi.toChuc?: string | null` (bỏ trống = ngữ cảnh
  hiện tại; `null` = ép cá nhân). Header gắn: `X-VBP-To-Chuc`.

- [ ] **Step 1: Tạo worktree**

```powershell
cd D:/AIVanBanCaNhan/vanbanplus-web
git fetch origin
git worktree add -b dot-b-to-chuc D:/AIVanBanCaNhan/_wt-b-to-chuc-web origin/main
New-Item -ItemType Junction -Path D:/AIVanBanCaNhan/_wt-b-to-chuc-web/node_modules -Target D:/AIVanBanCaNhan/vanbanplus-web/node_modules
cd D:/AIVanBanCaNhan/_wt-b-to-chuc-web; npm test
```

Expected: nền xanh trước khi sửa.

- [ ] **Step 2: Viết bài kiểm `src/api/ngu-canh.test.ts`**

```ts
import { beforeEach, expect, test, vi } from "vitest";
import { caiKhach, datPhien, datToChuc, goi, khiDoiToChuc, LoiApi, toChucHienTai } from "./khach";
import type { PhienTraVe } from "./loai";

const ND = (id = "u1") => ({ id, email: "a@b.vn", ho_ten: "A", goi: "free", goi_het_han: null, co_mat_khau: true, hai_lop_bat: false, google_gan: false });
const phien = (t: string, id = "u1"): PhienTraVe => ({ access_token: t, het_han_giay: 900, nguoi_dung: ND(id) });
const tl = (http: number, than: unknown) => new Response(JSON.stringify(than), { status: http, headers: { "Content-Type": "application/json" } });
const TC = "7c1e0a52-0000-4000-8000-000000000001";
let dau: Array<Record<string, string>>;
let duong: string[];

function gia(...traLoi: Response[]) {
  caiKhach({
    goc: "https://api.thu", gocTacVu: "https://thang.thu", bayGio: () => Date.now(), khoa: undefined,
    fetch: (async (url: string, init: RequestInit) => {
      duong.push(url);
      dau.push(init.headers as Record<string, string>);
      return traLoi.shift() ?? tl(200, { success: true, message: "", data: null });
    }) as unknown as typeof fetch,
  });
}

beforeEach(() => { dau = []; duong = []; datPhien(null); datToChuc(null); });

test("cá nhân: không có header X-VBP-To-Chuc", async () => {
  gia();
  datPhien(phien("T1"));
  await goi("/api/v2/kho/van-ban");
  expect(dau[0]["X-VBP-To-Chuc"]).toBeUndefined();
});

test("tổ chức: gắn header cho kho, tác vụ AI đi thẳng, đơn hàng; KHÔNG gắn cho /api/v2/phien/*", async () => {
  gia();
  datPhien(phien("T1"));
  datToChuc(TC);
  await goi("/api/v2/kho/van-ban");
  await goi("/api/v2/tac-vu/tom-tat/uoc-tinh", { phuongThuc: "POST", than: {} });
  await goi("/api/orders", { phuongThuc: "POST", than: {} });
  await goi("/api/v2/phien/thiet-bi");
  expect(dau.slice(0, 3).map((d) => d["X-VBP-To-Chuc"])).toEqual([TC, TC, TC]);
  expect(duong[1].startsWith("https://thang.thu")).toBe(true);
  expect(dau[3]["X-VBP-To-Chuc"]).toBeUndefined();
});

test("tùy chọn toChuc của một lời gọi thắng ngữ cảnh hiện tại", async () => {
  gia();
  datPhien(phien("T1"));
  datToChuc(TC);
  await goi("/api/v2/toi/co-quan", { toChuc: null });
  await goi("/api/v2/toi/co-quan", { toChuc: "khac" });
  expect(dau[0]["X-VBP-To-Chuc"]).toBeUndefined();
  expect(dau[1]["X-VBP-To-Chuc"]).toBe("khac");
});

test("đăng xuất / đổi tài khoản → về cá nhân và báo người nghe; làm mới cùng tài khoản thì giữ", () => {
  const nghe = vi.fn();
  const go = khiDoiToChuc(nghe);
  datPhien(phien("T1", "u1"));
  datToChuc(TC);
  datPhien(phien("T2", "u1"));
  expect(toChucHienTai()).toBe(TC);
  datPhien(phien("T3", "u2"));
  expect(toChucHienTai()).toBeNull();
  datToChuc(TC);
  datPhien(null);
  expect(toChucHienTai()).toBeNull();
  expect(nghe.mock.calls.map((c) => c[0])).toEqual([TC, null, TC, null]);
  go();
});

test("API báo KHONG_THUOC_TO_CHUC → tự về cá nhân, vẫn ném lỗi cho trang", async () => {
  gia(tl(403, { success: false, code: "KHONG_THUOC_TO_CHUC", message: "Tài khoản không thuộc tổ chức này hoặc đã rời tổ chức." }));
  datPhien(phien("T1"));
  datToChuc(TC);
  await expect(goi("/api/v2/kho/van-ban")).rejects.toBeInstanceOf(LoiApi);
  expect(toChucHienTai()).toBeNull();
});
```

- [ ] **Step 3: Chạy để thấy hỏng**

Run: `npx vitest run src/api/ngu-canh.test.ts`
Expected: FAIL — `datToChuc is not exported`.

- [ ] **Step 4: Sửa `src/api/khach.ts`**

Thêm ngay sau khối `khiDoiPhien`:

```ts
// Đợt B: ngữ cảnh tổ chức. null = cá nhân. Gắn header X-VBP-To-Chuc vào MỌI lời gọi trừ /api/v2/phien/* (phiên luôn của tài khoản).
let toChuc: string | null = null;
let uidPhien: string | null = null;
const ngheToChuc = new Set<(id: string | null) => void>();

export function datToChuc(id: string | null) {
  if (id === toChuc) return;
  toChuc = id;
  for (const f of [...ngheToChuc]) {
    try { f(id); } catch (e) { console.error("Người nghe đổi ngữ cảnh bị lỗi:", e); }
  }
}
export const toChucHienTai = () => toChuc;
export function khiDoiToChuc(fn: (id: string | null) => void) {
  ngheToChuc.add(fn);
  return () => { ngheToChuc.delete(fn); };
}
```

Trong `datPhien`, ngay sau dòng `hetHanLuc = …`:

```ts
  // đăng xuất hoặc đổi sang tài khoản khác → về cá nhân (ngữ cảnh không lọt sang tài khoản sau); làm mới cùng tài khoản thì giữ
  const uidMoi = p?.nguoi_dung.id ?? null;
  if (uidMoi !== uidPhien) { uidPhien = uidMoi; datToChuc(null); }
```

`TuyChonGoi` thêm:

```ts
  /** Ngữ cảnh riêng của lời gọi này: bỏ trống = ngữ cảnh hiện tại; null = ép cá nhân; chuỗi = tổ chức đó. */
  toChuc?: string | null;
```

Trong `guiMot`, sau dòng `if (bearer) dau.Authorization = …`:

```ts
  const tc = t.toChuc !== undefined ? t.toChuc : toChuc;
  if (tc && !duong.startsWith("/api/v2/phien/")) dau["X-VBP-To-Chuc"] = tc;
```

Trong `goi`, ngay trước `throw new LoiApi(r.status, j.code ?? …)` của khối `if (!r.ok || j.success === false)`:

```ts
    // bị bỏ khỏi tổ chức / tổ chức không còn: về cá nhân (trang hiện lỗi một lần, lần sau đã đúng ngữ cảnh)
    if (j.code === "KHONG_THUOC_TO_CHUC" && tuy.toChuc === undefined) datToChuc(null);
```

- [ ] **Step 5: Chạy kiểm**

Run: `npx vitest run src/api/ngu-canh.test.ts src/api/khach.test.ts`
Expected: PASS (bài cũ của `khach.test.ts` không sửa).

- [ ] **Step 6: Commit**

```bash
git add src/api/khach.ts src/api/ngu-canh.test.ts
git commit -m "feat(to-chuc): ngu canh to chuc trong khach.ts - header X-VBP-To-Chuc, ve ca nhan khi doi tai khoan

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 2: Lớp gọi API tổ chức + kiểu theo hợp đồng

**Files:**
- Create: `src/api/to-chuc.ts`
- Modify: `src/api/kho.ts` (`VanBan` thêm `sua_duoc?`, `cong_khai_noi_bo?`), `src/api/loai.ts` (`DungLuong.het_goi?`),
  `src/api/phien.ts` (`coQuan(tc?)`, `luuCoQuan(cq, tc?)`)
- Test: `src/api/to-chuc.test.ts` (mới)

**Interfaces:**
- Consumes: Task 1 (`TuyChonGoi.toChuc`).
- Produces (đúng hợp đồng API): kiểu `VaiTro`, `ToChucTomTat`, `ToChuc`, `ThanhVien`, `LoiMoi`, `GoiToChuc`, `GiaoViec`,
  `NhatKyGiaoViec`, `ViecDuocGiao`, `TrangThaiViec`; hằng `TEN_VAI_TRO`, `TEN_TRANG_THAI`, `TEN_VAI_XU_LY`;
  `toChuc.{ ds, tao, doc, sua, thanhVien, doiVaiTro, boThanhVien, roi, moi, dsLoiMoi, huyLoiMoi, nhanLoiMoi, bangGoi }`;
  `giaoViec.{ ds, tao, sua, thuHoi, cuaToi }`.

- [ ] **Step 1: Viết bài kiểm `src/api/to-chuc.test.ts`**

```ts
import { beforeEach, expect, test, vi } from "vitest";

vi.mock("./khach", async (goc) => ({ ...(await goc<object>()), goi: vi.fn(async () => ({ data: "ok", message: "" })) }));
import { goi } from "./khach";
import { giaoViec, toChuc } from "./to-chuc";

const g = vi.mocked(goi);
beforeEach(() => g.mockClear());

test("tổ chức: đúng đường, phương thức, thân theo hợp đồng", async () => {
  await toChuc.ds();
  await toChuc.tao({ ten: "UBND xã" });
  await toChuc.sua("t/1", 3, { ten: "X" });
  await toChuc.doiVaiTro("t1", "u1", "van_thu");
  await toChuc.boThanhVien("t1", "u1");
  await toChuc.roi("t1");
  await toChuc.moi("t1", "a@b.vn", "can_bo");
  await toChuc.huyLoiMoi("t1", "l1");
  await toChuc.nhanLoiMoi("M".repeat(43));
  await toChuc.bangGoi();
  expect(g.mock.calls.map((c) => [c[0], (c[1] as any)?.phuongThuc ?? "GET", (c[1] as any)?.than])).toEqual([
    ["/api/v2/to-chuc", "GET", undefined],
    ["/api/v2/to-chuc", "POST", { ten: "UBND xã" }],
    ["/api/v2/to-chuc/t%2F1", "PUT", { ten: "X", phien_ban: 3 }],
    ["/api/v2/to-chuc/t1/thanh-vien/u1", "PATCH", { vai_tro: "van_thu" }],
    ["/api/v2/to-chuc/t1/thanh-vien/u1", "DELETE", undefined],
    ["/api/v2/to-chuc/t1/roi", "POST", {}],
    ["/api/v2/to-chuc/t1/loi-moi", "POST", { email: "a@b.vn", vai_tro: "can_bo" }],
    ["/api/v2/to-chuc/t1/loi-moi/l1", "DELETE", undefined],
    ["/api/v2/loi-moi/nhan", "POST", { ma: "M".repeat(43) }],
    ["/api/v2/goi-to-chuc", "GET", undefined],
  ]);
});

test("đọc tổ chức / thành viên / lời mời: đường theo id (route tổ chức của API bỏ qua header ngữ cảnh)", async () => {
  await toChuc.doc("t1");
  await toChuc.thanhVien("t1");
  await toChuc.dsLoiMoi("t1");
  expect(g.mock.calls.map((c) => c[0])).toEqual(["/api/v2/to-chuc/t1", "/api/v2/to-chuc/t1/thanh-vien", "/api/v2/to-chuc/t1/loi-moi"]);
});

test("giao việc: đường kho, lọc «Việc được giao» thành chuỗi truy vấn", async () => {
  await giaoViec.ds("v1");
  await giaoViec.tao("v1", { nguoi_nhan: "u2", vai: "chu_tri", han_xu_ly: null });
  await giaoViec.sua("g1", 2, { trang_thai: "dang" });
  await giaoViec.thuHoi("g1");
  await giaoViec.cuaToi({ trang_thai: ["cho", "dang"], qua_han: true });
  await giaoViec.cuaToi({});
  expect(g.mock.calls.map((c) => [c[0], (c[1] as any)?.phuongThuc ?? "GET", (c[1] as any)?.than])).toEqual([
    ["/api/v2/kho/van-ban/v1/giao-viec", "GET", undefined],
    ["/api/v2/kho/van-ban/v1/giao-viec", "POST", { nguoi_nhan: "u2", vai: "chu_tri", han_xu_ly: null }],
    ["/api/v2/kho/giao-viec/g1", "PATCH", { trang_thai: "dang", phien_ban: 2 }],
    ["/api/v2/kho/giao-viec/g1", "DELETE", undefined],
    ["/api/v2/kho/giao-viec/cua-toi?trang_thai=cho%2Cdang&qua_han=1", "GET", undefined],
    ["/api/v2/kho/giao-viec/cua-toi", "GET", undefined],
  ]);
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx vitest run src/api/to-chuc.test.ts`
Expected: FAIL — không có `./to-chuc`.

- [ ] **Step 3: Viết `src/api/to-chuc.ts`**

```ts
// Lớp mỏng gọi API tổ chức (đợt B). Hợp đồng: docs/superpowers/plans/2026-11-19-dot-b-to-chuc-api.md «Hợp đồng API cho web».
// Mọi luật quyền ở API; web chỉ hiện theo vai_tro / sua_duoc / quyen.giao mà API trả.
import { goi } from "./khach";
import type { CoQuan } from "@/phan-he/soan-thao/co-quan";

export type VaiTro = "quan_tri" | "van_thu" | "can_bo";
export const TEN_VAI_TRO: Record<VaiTro, string> = { quan_tri: "Quản trị", van_thu: "Văn thư", can_bo: "Cán bộ" };

export type ToChucTomTat = {
  id: string; ten: string; vai_tro: VaiTro; goi_ma: string | null; goi_ten: string | null;
  goi_bat_dau: string | null; goi_het_han: string | null; con_hieu_luc: boolean;
};
export type ToChuc = ToChucTomTat & {
  mst: string | null; dia_chi: string | null; email_hoa_don: string | null; phien_ban: number;
  so_thanh_vien: number; so_thanh_vien_toi_da: number; co_quan: CoQuan | null;
};
export type ThanhVien = { user_id: string; ho_ten: string; email: string; vai_tro: VaiTro; trang_thai: "hoat_dong"; them_luc: string };
export type LoiMoi = { id: string; email: string; vai_tro: VaiTro; het_han: string; tao_luc: string; trang_thai: "cho" | "da_dung" | "da_huy" | "het_han" };
export type GoiToChuc = {
  ma: string; ten: string; so_thanh_vien_toi_da: number; credit_thang: number; dung_luong_mb: number;
  max_file_size_mb: number; gia_nam_vnd: number | null; dang_ban: boolean;
};

export type TrangThaiViec = "cho" | "dang" | "xong";
export const TEN_TRANG_THAI: Record<TrangThaiViec, string> = { cho: "Chờ xử lý", dang: "Đang xử lý", xong: "Đã xong" };
export type VaiXuLy = "chu_tri" | "phoi_hop";
export const TEN_VAI_XU_LY: Record<VaiXuLy, string> = { chu_tri: "Chủ trì", phoi_hop: "Phối hợp" };
export type GiaoViec = {
  id: string; van_ban_id: string; nguoi_nhan: string; nguoi_nhan_ten: string | null; vai: VaiXuLy;
  han_xu_ly: string | null; y_kien_chi_dao: string | null; trang_thai: TrangThaiViec; ghi_chu: string | null;
  giao_boi: string; giao_boi_ten: string | null; giao_luc: string; cap_nhat_luc: string; phien_ban: number;
};
export type NhatKyGiaoViec = {
  id: number; giao_viec_id: string; nguoi_ten: string | null; hanh_dong: "giao" | "sua" | "trang_thai" | "ghi_chu" | "thu_hoi";
  truoc: Record<string, unknown> | null; sau: Record<string, unknown> | null; luc: string;
};
export type ViecDuocGiao = GiaoViec & { van_ban: { id: string; so: string | null; ky_hieu: string | null; trich_yeu: string; do_khan: string } };
export type GiaoViecVanBan = { muc: GiaoViec[]; nhat_ky: NhatKyGiaoViec[]; quyen: { giao: boolean } };

const GHI = 60_000; // lệnh ghi: hạn dài để không báo mất mạng giả khi máy chủ đã ghi xong
const ma_ = (x: string) => encodeURIComponent(x);
const du = async <T>(p: Promise<{ data: T }>) => (await p).data;
const xong = (p: Promise<unknown>) => p.then(() => undefined);

export const toChuc = {
  ds: () => du(goi<ToChucTomTat[]>("/api/v2/to-chuc")),
  tao: (t: { ten: string; mst?: string; dia_chi?: string; email_hoa_don?: string }) =>
    du(goi<ToChucTomTat>("/api/v2/to-chuc", { phuongThuc: "POST", than: t, hanMs: GHI })),
  doc: (id: string) => du(goi<ToChuc>(`/api/v2/to-chuc/${ma_(id)}`)),
  sua: (id: string, phien_ban: number, thay: Partial<Pick<ToChuc, "ten" | "mst" | "dia_chi" | "email_hoa_don">> & { co_quan?: CoQuan }) =>
    du(goi<ToChuc>(`/api/v2/to-chuc/${ma_(id)}`, { phuongThuc: "PUT", than: { ...thay, phien_ban }, hanMs: GHI })),
  thanhVien: (id: string) => du(goi<ThanhVien[]>(`/api/v2/to-chuc/${ma_(id)}/thanh-vien`)),
  doiVaiTro: (id: string, uid: string, vai_tro: VaiTro) =>
    du(goi<ThanhVien>(`/api/v2/to-chuc/${ma_(id)}/thanh-vien/${ma_(uid)}`, { phuongThuc: "PATCH", than: { vai_tro }, hanMs: GHI })),
  boThanhVien: (id: string, uid: string) =>
    xong(goi(`/api/v2/to-chuc/${ma_(id)}/thanh-vien/${ma_(uid)}`, { phuongThuc: "DELETE", hanMs: GHI })),
  roi: (id: string) => xong(goi(`/api/v2/to-chuc/${ma_(id)}/roi`, { phuongThuc: "POST", than: {}, hanMs: GHI })),
  moi: (id: string, email: string, vai_tro: VaiTro) =>
    du(goi<LoiMoi>(`/api/v2/to-chuc/${ma_(id)}/loi-moi`, { phuongThuc: "POST", than: { email, vai_tro }, hanMs: GHI })),
  dsLoiMoi: (id: string) => du(goi<LoiMoi[]>(`/api/v2/to-chuc/${ma_(id)}/loi-moi`)),
  huyLoiMoi: (id: string, lid: string) =>
    xong(goi(`/api/v2/to-chuc/${ma_(id)}/loi-moi/${ma_(lid)}`, { phuongThuc: "DELETE", hanMs: GHI })),
  nhanLoiMoi: (ma: string) =>
    du(goi<{ to_chuc: ToChucTomTat }>("/api/v2/loi-moi/nhan", { phuongThuc: "POST", than: { ma }, hanMs: GHI })),
  bangGoi: () => du(goi<GoiToChuc[]>("/api/v2/goi-to-chuc")),
};

export const giaoViec = {
  ds: (vanBanId: string) => du(goi<GiaoViecVanBan>(`/api/v2/kho/van-ban/${ma_(vanBanId)}/giao-viec`)),
  tao: (vanBanId: string, t: { nguoi_nhan: string; vai: VaiXuLy; han_xu_ly?: string | null; y_kien_chi_dao?: string }) =>
    du(goi<GiaoViec>(`/api/v2/kho/van-ban/${ma_(vanBanId)}/giao-viec`, { phuongThuc: "POST", than: t, hanMs: GHI })),
  sua: (gid: string, phien_ban: number, thay: Partial<Pick<GiaoViec, "trang_thai" | "ghi_chu" | "han_xu_ly" | "y_kien_chi_dao" | "vai" | "nguoi_nhan">>) =>
    du(goi<GiaoViec>(`/api/v2/kho/giao-viec/${ma_(gid)}`, { phuongThuc: "PATCH", than: { ...thay, phien_ban }, hanMs: GHI })),
  thuHoi: (gid: string) => xong(goi(`/api/v2/kho/giao-viec/${ma_(gid)}`, { phuongThuc: "DELETE", hanMs: GHI })),
  cuaToi: (loc: { trang_thai?: TrangThaiViec[]; qua_han?: boolean; trang?: number }) => {
    const sp = new URLSearchParams();
    if (loc.trang_thai?.length) sp.set("trang_thai", loc.trang_thai.join(","));
    if (loc.qua_han) sp.set("qua_han", "1");
    if (loc.trang && loc.trang > 1) sp.set("trang", String(loc.trang));
    const q = sp.toString();
    return du(goi<{ muc: ViecDuocGiao[]; tong: number }>(`/api/v2/kho/giao-viec/cua-toi${q ? `?${q}` : ""}`));
  },
};
```

`src/api/kho.ts` — trong `VanBan` thêm cuối kiểu:

```ts
  /** Đợt B, chỉ ở kho tổ chức: văn bản công khai cho mọi thành viên; người gọi sửa / xóa / gắn tệp được. */
  cong_khai_noi_bo?: boolean; sua_duoc?: boolean;
```

`src/api/loai.ts` — `DungLuong` thêm `het_goi?: boolean; /* đợt B: kho tổ chức chưa có gói / hết gói → chỉ đọc */`.

`src/api/phien.ts` — thay hai hàm Cơ quan (gọi cá nhân giữ nguyên lời gọi cũ):

```ts
  /** Cơ quan dùng cho soạn thảo AI (4.9); null = chưa lưu. `tc`: đọc hồ sơ Cơ quan của tổ chức đó (đợt B). */
  coQuan: async (tc?: string | null): Promise<CoQuan | null> =>
    (await (tc ? goi<{ co_quan: CoQuan | null }>("/api/v2/toi/co-quan", { toChuc: tc })
               : goi<{ co_quan: CoQuan | null }>("/api/v2/toi/co-quan"))).data.co_quan,
  luuCoQuan: async (cq: CoQuan, tc?: string | null): Promise<CoQuan> =>
    (await goi<{ co_quan: CoQuan }>("/api/v2/toi/co-quan",
      tc ? { phuongThuc: "PUT", than: cq, hanMs: 60_000, toChuc: tc } : { phuongThuc: "PUT", than: cq, hanMs: 60_000 })).data.co_quan,
```

- [ ] **Step 4: Chạy kiểm**

Run: `npx vitest run src/api` rồi `npx tsc -b`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/api
git commit -m "feat(to-chuc): lop goi API to chuc, giao viec theo hop dong; VanBan.sua_duoc, DungLuong.het_goi

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 3: `NguCanhProvider` — đọc danh sách tổ chức, khôi phục lựa chọn, gắn lại cây trang khi đổi

**Files:**
- Create: `src/to-chuc/NguCanh.tsx`
- Modify: `src/App.tsx` (bọc provider, `Routes key`), `src/vo/Vo.tsx` (chờ `san`)
- Test: `src/to-chuc/ngu-canh.test.tsx` (mới)

**Interfaces:**
- Consumes: Task 1 (`datToChuc`, `khiDoiToChuc`, `toChucHienTai`, `coPhien`), Task 2 (`toChuc.ds`, `ToChucTomTat`).
- Produces: `NguCanhProvider`; `useNguCanh(): NguCanh` với `NguCanh = { ds: ToChucTomTat[]; hienTai: ToChucTomTat | null;
  san: boolean; chon(id: string | null): void; taiLai(): Promise<ToChucTomTat[]> }` (ngoài provider trả mặc định cá nhân,
  `san: true`); `useToChucId(): string | null`. Khóa `localStorage`: `vbp-ngu-canh:<uid>`.

- [ ] **Step 1: Viết bài kiểm `src/to-chuc/ngu-canh.test.tsx`**

```tsx
import { act, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes, useLocation } from "react-router";
import { beforeEach, expect, test, vi } from "vitest";

vi.mock("@/phien/PhienProvider", () => ({ usePhien: () => ({ trangThai: "vao", nguoiDung: { id: "u1" } }) }));
vi.mock("@/api/to-chuc", async (goc) => ({ ...(await goc<object>()), toChuc: { ds: vi.fn() } }));
import { datPhien, datToChuc, toChucHienTai } from "@/api/khach";
import { toChuc } from "@/api/to-chuc";
import { NguCanhProvider, useNguCanh, useToChucId } from "./NguCanh";

const T = (id: string, ten: string) => ({ id, ten, vai_tro: "van_thu" as const, goi_ma: "tc-10", goi_ten: "Tổ chức 10 người",
  goi_bat_dau: null, goi_het_han: null, con_hieu_luc: true });
const DS = [T("t1", "UBND phường Bình Thạnh"), T("t2", "Trường THCS Gia Kiệm")];
const ND = { id: "u1", email: "a@b.vn", ho_ten: "A", goi: "free", goi_het_han: null, co_mat_khau: true, hai_lop_bat: false, google_gan: false };

function Hien() {
  const nc = useNguCanh();
  const id = useToChucId();
  const { pathname } = useLocation();
  return (
    <div>
      <p data-testid="san">{String(nc.san)}</p>
      <p data-testid="id">{id ?? "ca-nhan"}</p>
      <p data-testid="ten">{nc.hienTai?.ten ?? "Cá nhân"}</p>
      <p data-testid="duong">{pathname}</p>
      <button type="button" onClick={() => nc.chon("t2")}>chọn t2</button>
    </div>
  );
}
const mo = () => render(
  <MemoryRouter initialEntries={["/van-ban"]}><NguCanhProvider><Routes><Route path="*" element={<Hien />} /></Routes></NguCanhProvider></MemoryRouter>);

beforeEach(() => {
  vi.mocked(toChuc.ds).mockReset();
  localStorage.clear();
  datPhien(null);
  datPhien({ access_token: "T", het_han_giay: 900, nguoi_dung: ND });
});

test("chưa lưu lựa chọn: sẵn sàng ở cá nhân, có danh sách", async () => {
  vi.mocked(toChuc.ds).mockResolvedValue(DS);
  mo();
  await waitFor(() => expect(screen.getByTestId("san")).toHaveTextContent("true"));
  expect(screen.getByTestId("id")).toHaveTextContent("ca-nhan");
});

test("đã lưu t1 và t1 còn trong danh sách → khôi phục; không còn → về cá nhân", async () => {
  localStorage.setItem("vbp-ngu-canh:u1", "t1");
  vi.mocked(toChuc.ds).mockResolvedValue(DS);
  const r = mo();
  await waitFor(() => expect(screen.getByTestId("ten")).toHaveTextContent("UBND phường Bình Thạnh"));
  r.unmount();
  datToChuc(null);
  localStorage.setItem("vbp-ngu-canh:u1", "da-roi");
  mo();
  await waitFor(() => expect(screen.getByTestId("san")).toHaveTextContent("true"));
  expect(screen.getByTestId("id")).toHaveTextContent("ca-nhan");
});

test("trước khi đọc xong danh sách: san = false (trang chưa gọi API sai ngữ cảnh)", async () => {
  let xong!: (v: typeof DS) => void;
  vi.mocked(toChuc.ds).mockReturnValue(new Promise((r) => { xong = r; }));
  mo();
  expect(screen.getByTestId("san")).toHaveTextContent("false");
  await act(async () => { xong(DS); });
  expect(screen.getByTestId("san")).toHaveTextContent("true");
});

test("chọn tổ chức: đặt header, lưu theo uid, về màn hình chính", async () => {
  vi.mocked(toChuc.ds).mockResolvedValue(DS);
  mo();
  await waitFor(() => expect(screen.getByTestId("san")).toHaveTextContent("true"));
  act(() => screen.getByRole("button", { name: "chọn t2" }).click());
  expect(toChucHienTai()).toBe("t2");
  expect(localStorage.getItem("vbp-ngu-canh:u1")).toBe("t2");
  expect(screen.getByTestId("duong")).toHaveTextContent(/^\/$/);
});

test("đọc danh sách lỗi → làm việc ở cá nhân, không chặn", async () => {
  vi.mocked(toChuc.ds).mockRejectedValue(new Error("mạng"));
  mo();
  await waitFor(() => expect(screen.getByTestId("san")).toHaveTextContent("true"));
  expect(screen.getByTestId("id")).toHaveTextContent("ca-nhan");
});

test("đăng xuất không xóa lựa chọn đã lưu", async () => {
  vi.mocked(toChuc.ds).mockResolvedValue(DS);
  mo();
  await waitFor(() => expect(screen.getByTestId("san")).toHaveTextContent("true"));
  act(() => screen.getByRole("button", { name: "chọn t2" }).click());
  act(() => datPhien(null));
  expect(toChucHienTai()).toBeNull();
  expect(localStorage.getItem("vbp-ngu-canh:u1")).toBe("t2");
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx vitest run src/to-chuc/ngu-canh.test.tsx`
Expected: FAIL — không có `./NguCanh`.

- [ ] **Step 3: Viết `src/to-chuc/NguCanh.tsx`**

```tsx
import { createContext, useCallback, useContext, useEffect, useMemo, useState, useSyncExternalStore, type ReactNode } from "react";
import { useNavigate } from "react-router";
import { coPhien, datToChuc, khiDoiToChuc, toChucHienTai } from "@/api/khach";
import { toChuc as apiToChuc, type ToChucTomTat } from "@/api/to-chuc";
import { usePhien } from "@/phien/PhienProvider";

const khoa = (uid: string) => `vbp-ngu-canh:${uid}`;
function docDaLuu(uid: string): string | null {
  try { return localStorage.getItem(khoa(uid)); } catch { return null; }
}
function luuChon(uid: string, id: string | null) {
  try { if (id) localStorage.setItem(khoa(uid), id); else localStorage.removeItem(khoa(uid)); }
  catch { /* tiện ích: không lưu được thì lần mở sau về cá nhân */ }
}

/** id tổ chức đang chọn (null = cá nhân), đọc thẳng từ khach.ts — dùng được cả ngoài NguCanhProvider. */
export function useToChucId(): string | null {
  return useSyncExternalStore(khiDoiToChuc, toChucHienTai, toChucHienTai);
}

export type NguCanh = {
  /** Tổ chức đang là thành viên hoạt động (API trả). */
  ds: ToChucTomTat[];
  /** null = cá nhân. */
  hienTai: ToChucTomTat | null;
  /** Đã đọc xong danh sách + khôi phục lựa chọn: trang chỉ hiện khi san (không gọi API sai ngữ cảnh lúc mở). */
  san: boolean;
  /** Đổi ngữ cảnh rồi về màn hình chính. */
  chon(id: string | null): void;
  /** Đọc lại danh sách (sau khi tạo tổ chức, nhận lời mời, rời tổ chức). */
  taiLai(): Promise<ToChucTomTat[]>;
};

const MAC_DINH: NguCanh = { ds: [], hienTai: null, san: true, chon: () => {}, taiLai: async () => [] };
const Ctx = createContext<NguCanh>(MAC_DINH);

export function NguCanhProvider({ children }: { children: ReactNode }) {
  const { trangThai, nguoiDung } = usePhien();
  const uid = trangThai === "vao" ? nguoiDung?.id ?? "" : "";
  const id = useToChucId();
  const navigate = useNavigate();
  const [ds, setDs] = useState<ToChucTomTat[]>([]);
  const [daDoc, setDaDoc] = useState(""); // uid đã đọc xong danh sách

  useEffect(() => {
    if (!uid) { setDs([]); setDaDoc(""); return; }
    let huy = false;
    apiToChuc.ds().then((moi) => {
      if (huy) return;
      setDs(moi);
      const daLuu = docDaLuu(uid);
      datToChuc(daLuu && moi.some((t) => t.id === daLuu) ? daLuu : null);
      setDaDoc(uid);
    }, () => {
      if (huy) return;
      setDs([]); // không đọc được → làm việc ở cá nhân, không chặn người dùng
      datToChuc(null);
      setDaDoc(uid);
    });
    return () => { huy = true; };
  }, [uid]);

  // Mọi lần đổi ngữ cảnh khi còn phiên (chọn tay; tự về cá nhân vì KHONG_THUOC_TO_CHUC) đều ghi nhớ theo uid.
  // Đăng xuất: datPhien(null) xóa token TRƯỚC khi về cá nhân → coPhien() false → giữ lựa chọn đã lưu cho lần sau.
  useEffect(() => (uid ? khiDoiToChuc((moi) => { if (coPhien()) luuChon(uid, moi); }) : undefined), [uid]);

  const chon = useCallback((moi: string | null) => {
    datToChuc(moi);
    navigate("/");
  }, [navigate]);

  const taiLai = useCallback(async () => {
    const moi = await apiToChuc.ds();
    setDs(moi);
    return moi;
  }, []);

  const gt = useMemo<NguCanh>(() => ({
    ds, hienTai: ds.find((t) => t.id === id) ?? null, san: !uid || daDoc === uid, chon, taiLai,
  }), [ds, id, uid, daDoc, chon, taiLai]);
  return <Ctx.Provider value={gt}>{children}</Ctx.Provider>;
}

/** Ngoài NguCanhProvider (bài kiểm dựng thẳng một trang) trả mặc định: cá nhân, sẵn sàng. */
export const useNguCanh = (): NguCanh => useContext(Ctx);
```

- [ ] **Step 4: Gắn vào `src/App.tsx` và `src/vo/Vo.tsx`**

`src/App.tsx`:

```tsx
import { NguCanhProvider, useToChucId } from "@/to-chuc/NguCanh";

export function App() {
  return (
    <PhienProvider>
      <NguCanhProvider>
        <GoCoLamMoi />
        <CacTrang />
      </NguCanhProvider>
    </PhienProvider>
  );
}

/** Đổi ngữ cảnh (cá nhân ↔ tổ chức) gắn lại toàn bộ cây trang: không trang nào giữ dữ liệu của ngữ cảnh cũ. */
function CacTrang() {
  const tc = useToChucId();
  return (
    <Routes key={tc ?? "ca-nhan"}>
      {/* … các <Route> hiện có giữ nguyên … */}
    </Routes>
  );
}
```

`src/vo/Vo.tsx` — thêm `import { useNguCanh } from "@/to-chuc/NguCanh";`, trong thân `Vo` thêm `const nc = useNguCanh();`
và thay dòng `<Suspense …>{children}</Suspense>` thành:

```tsx
          <Suspense fallback={<div className="man-cho man-cho--nho"><span className="vong-xoay" /></div>}>
            {nc.san ? children : <div className="man-cho man-cho--nho" role="status" aria-label="Đang tải nơi làm việc"><span className="vong-xoay" /></div>}
          </Suspense>
```

- [ ] **Step 5: Chạy kiểm**

Run: `npx vitest run src/to-chuc src/vo src/phien` rồi `npx tsc -b`
Expected: PASS (bài kiểm `Vo` cũ không có provider → mặc định `san: true`).

- [ ] **Step 6: Commit**

```bash
git add src/to-chuc/NguCanh.tsx src/to-chuc/ngu-canh.test.tsx src/App.tsx src/vo/Vo.tsx
git commit -m "feat(to-chuc): NguCanhProvider - doc danh sach to chuc, khoi phuc lua chon theo uid, gan lai cay trang khi doi

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 4: Nút «Cá nhân / Tổ chức X» trên thanh menu

**Files:**
- Create: `src/to-chuc/ChonNguCanh.tsx`, `src/to-chuc/ngu-canh.css`
- Modify: `src/vo/Vo.tsx` (prop `phaiMenu`)
- Test: `src/to-chuc/chon-ngu-canh.test.tsx` (mới)

**Interfaces:**
- Consumes: Task 3 (`useNguCanh`), Task 2 (`TEN_VAI_TRO`).
- Produces: `ChonNguCanh()` — nút mở menu `role="menu"` gồm «Cá nhân», từng tổ chức (`role="menuitemradio"`), «Quản lý tổ
  chức…» (`/to-chuc`). Ẩn khi `ds` rỗng.

- [ ] **Step 1: Viết bài kiểm `src/to-chuc/chon-ngu-canh.test.tsx`**

```tsx
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router";
import { beforeEach, expect, test, vi } from "vitest";
import type { NguCanh } from "./NguCanh";

const chon = vi.fn();
let nc: NguCanh;
vi.mock("./NguCanh", () => ({ useNguCanh: () => nc }));
import { ChonNguCanh } from "./ChonNguCanh";

const T = (id: string, ten: string, vai: "quan_tri" | "can_bo", con = true) =>
  ({ id, ten, vai_tro: vai, goi_ma: null, goi_ten: null, goi_bat_dau: null, goi_het_han: null, con_hieu_luc: con });
const DS = [T("t1", "UBND phường Bình Thạnh", "quan_tri"), T("t2", "Trường THCS <b>Gia Kiệm</b>", "can_bo", false)];
const mo = () => render(<MemoryRouter><ChonNguCanh /></MemoryRouter>);

beforeEach(() => { chon.mockReset(); nc = { ds: DS, hienTai: null, san: true, chon, taiLai: async () => DS }; });

test("chưa thuộc tổ chức nào: không hiện nút", () => {
  nc = { ...nc, ds: [] };
  mo();
  expect(screen.queryByRole("button")).toBeNull();
});

test("hiện «Cá nhân» khi chưa chọn; mở menu thấy từng tổ chức kèm vai trò, gói chưa có", async () => {
  mo();
  const nut = screen.getByRole("button", { name: /Cá nhân/ });
  expect(nut).toHaveAttribute("aria-expanded", "false");
  await userEvent.click(nut);
  const menu = screen.getByRole("menu", { name: "Chọn nơi làm việc" });
  expect(within(menu).getByRole("menuitemradio", { name: /Cá nhân/ })).toHaveAttribute("aria-checked", "true");
  expect(within(menu).getByRole("menuitemradio", { name: /UBND phường Bình Thạnh.*Quản trị/ })).toBeInTheDocument();
  expect(within(menu).getByText("Trường THCS <b>Gia Kiệm</b>")).toBeInTheDocument(); // chữ thuần, không dựng HTML
  expect(within(menu).getByText(/Cán bộ · chưa có gói/)).toBeInTheDocument();
  expect(within(menu).getByRole("menuitem", { name: "Quản lý tổ chức…" })).toHaveAttribute("href", "/to-chuc");
});

test("chọn tổ chức → gọi chon(id), đóng menu; Esc đóng menu", async () => {
  nc = { ...nc, hienTai: DS[0] };
  mo();
  await userEvent.click(screen.getByRole("button", { name: /UBND phường Bình Thạnh/ }));
  await userEvent.keyboard("{Escape}");
  expect(screen.queryByRole("menu")).toBeNull();
  await userEvent.click(screen.getByRole("button", { name: /UBND phường Bình Thạnh/ }));
  await userEvent.click(screen.getByRole("menuitemradio", { name: /Cá nhân/ }));
  expect(chon).toHaveBeenCalledWith(null);
  expect(screen.queryByRole("menu")).toBeNull();
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx vitest run src/to-chuc/chon-ngu-canh.test.tsx`
Expected: FAIL — không có `./ChonNguCanh`.

- [ ] **Step 3: Viết `src/to-chuc/ChonNguCanh.tsx` + `src/to-chuc/ngu-canh.css`**

```tsx
import { useEffect, useRef, useState } from "react";
import { Link } from "react-router";
import { TEN_VAI_TRO } from "@/api/to-chuc";
import { useNguCanh } from "./NguCanh";
import "./ngu-canh.css";

/** Nút chọn nơi làm việc trên thanh menu (prop phaiMenu của OsShell). Đổi nơi làm việc → về màn hình chính. */
export function ChonNguCanh() {
  const { ds, hienTai, chon } = useNguCanh();
  const [mo, setMo] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!mo) return;
    const ngoai = (e: MouseEvent) => { if (!ref.current?.contains(e.target as Node)) setMo(false); };
    const esc = (e: KeyboardEvent) => { if (e.key === "Escape") setMo(false); };
    document.addEventListener("mousedown", ngoai);
    document.addEventListener("keydown", esc);
    return () => { document.removeEventListener("mousedown", ngoai); document.removeEventListener("keydown", esc); };
  }, [mo]);

  if (ds.length === 0) return null;
  const chonVa = (id: string | null) => { setMo(false); chon(id); };
  return (
    <div className="ngu-canh" ref={ref}>
      <button type="button" className="ngu-canh__nut" aria-haspopup="menu" aria-expanded={mo} title="Chọn nơi làm việc"
        onClick={() => setMo((x) => !x)}>
        <span className="ngu-canh__ten">{hienTai ? hienTai.ten : "Cá nhân"}</span>
        <span aria-hidden="true" className="ngu-canh__mui">▾</span>
      </button>
      {mo && (
        <div className="ngu-canh__menu" role="menu" aria-label="Chọn nơi làm việc">
          <button type="button" role="menuitemradio" aria-checked={!hienTai} className="ngu-canh__muc" onClick={() => chonVa(null)}>
            <span>Cá nhân</span><small>Kho và credit riêng</small>
          </button>
          {ds.map((t) => (
            <button key={t.id} type="button" role="menuitemradio" aria-checked={hienTai?.id === t.id} className="ngu-canh__muc"
              onClick={() => chonVa(t.id)}>
              <span>{t.ten}</span>
              <small>{TEN_VAI_TRO[t.vai_tro]}{t.con_hieu_luc ? "" : " · chưa có gói"}</small>
            </button>
          ))}
          <Link role="menuitem" className="ngu-canh__muc ngu-canh__muc--phu" to="/to-chuc" onClick={() => setMo(false)}>
            Quản lý tổ chức…
          </Link>
        </div>
      )}
    </div>
  );
}
```

```css
/* Nút chọn nơi làm việc trên thanh menu os-kit (prop phaiMenu). Màu theo biến của khung (--osk-*) để theo chế độ tối. */
.ngu-canh { position: relative; }
.ngu-canh__nut {
  display: inline-flex; align-items: center; gap: 4px; max-width: 220px; padding: 2px 10px; border: 0; border-radius: 6px;
  background: transparent; color: inherit; font: inherit; font-weight: 600; cursor: pointer;
}
.ngu-canh__nut:hover, .ngu-canh__nut[aria-expanded="true"] { background: rgb(127 127 127 / 0.18); }
.ngu-canh__nut:focus-visible { outline: 2px solid var(--osk-xanh); outline-offset: 1px; }
.ngu-canh__ten { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
/* Cùng nền / chữ / bóng với menu xổ .osk-xuong của os-kit (biến --osk-nen-xo, --osk-chu-menu đổi theo chế độ tối). */
.ngu-canh__menu {
  position: absolute; right: 0; top: calc(100% + 6px); z-index: 60; min-width: 260px; max-width: min(360px, 90vw);
  padding: 6px; border: 1px solid rgba(0, 0, 0, 0.08); border-radius: 10px;
  background: var(--osk-nen-xo, rgba(255, 255, 255, 0.95)); -webkit-backdrop-filter: blur(20px); backdrop-filter: blur(20px);
  color: var(--osk-chu-menu, #111827); font: 14px/1.3 var(--osk-chu); box-shadow: 0 12px 32px rgba(0, 0, 0, 0.25);
}
.ngu-canh__muc {
  display: flex; flex-direction: column; align-items: flex-start; width: 100%; padding: 8px 10px; border: 0; border-radius: 6px;
  background: transparent; color: inherit; font: inherit; text-align: left; text-decoration: none; cursor: pointer;
}
.ngu-canh__muc small { opacity: 0.7; }
.ngu-canh__muc[aria-checked="true"] { font-weight: 600; background: rgb(37 99 235 / 0.1); }
.ngu-canh__muc:hover, .ngu-canh__muc:focus-visible { background: rgb(10 102 216 / 0.12); outline: none; }
.ngu-canh__muc--phu { border-top: 1px solid rgb(127 127 127 / 0.25); border-radius: 0 0 6px 6px; margin-top: 4px; }
@media (max-width: 600px) { .ngu-canh__nut { max-width: 150px; } }
```

`src/vo/Vo.tsx` — thêm `import { ChonNguCanh } from "@/to-chuc/ChonNguCanh";` và prop của `OsShell`:
`phaiMenu={<ChonNguCanh />}`.

- [ ] **Step 4: Chạy kiểm**

Run: `npx vitest run src/to-chuc src/vo` rồi `npx tsc -b`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/to-chuc src/vo/Vo.tsx
git commit -m "feat(to-chuc): nut chon noi lam viec Ca nhan / To chuc tren thanh menu (phaiMenu cua os-kit)

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 5: Bộ đệm cấp mô-đun theo ngữ cảnh (Cơ quan, bản nháp soạn thảo)

**Files:**
- Modify: `src/phan-he/soan-thao/co-quan.ts`, `src/phan-he/soan-thao/ban-soan.ts`, `src/phan-he/soan-thao/CoQuanCaiDat.tsx`,
  `src/phan-he/soan-thao/soan-thao.css`
- Test: `src/phan-he/soan-thao/co-quan-ngu-canh.test.ts` (mới), `src/phan-he/soan-thao/co-quan-cai-dat-to-chuc.test.tsx` (mới)

**Interfaces:**
- Consumes: Task 1 (`datToChuc`, `khiDoiToChuc`), Task 2 (`api.coQuan(tc?)`, `api.luuCoQuan(cq, tc?)`), Task 3 (`useToChucId`,
  `useNguCanh`).
- Produces: `useCoQuan()` giữ chữ ký; bộ đệm khóa `uid|tổ chức`; Quét (`BuocSoat` nhận `tenCoQuan` từ `useCoQuan`), Soạn
  thảo, Báo cáo tự dùng hồ sơ Cơ quan của tổ chức.

- [ ] **Step 1: Viết bài kiểm**

`src/phan-he/soan-thao/co-quan-ngu-canh.test.ts`:

```ts
import { act, renderHook, waitFor } from "@testing-library/react";
import { beforeEach, expect, test, vi } from "vitest";

vi.mock("@/phien/PhienProvider", () => ({ usePhien: () => ({ nguoiDung: { id: "u1" } }) }));
vi.mock("@/api/phien", () => ({ api: { coQuan: vi.fn(), luuCoQuan: vi.fn() } }));
import { datPhien, datToChuc } from "@/api/khach";
import { api } from "@/api/phien";
import { docBanSoan, ganBanSoan } from "./ban-soan";
import { useCoQuan, type CoQuan } from "./co-quan";

const CQ = (ten: string): CoQuan => ({ ten_co_quan: ten, xa: "x", tinh: "t", dac_diem: [], nguoi_ky: "", chuc_danh: "" });

beforeEach(() => {
  vi.mocked(api.coQuan).mockReset();
  vi.mocked(api.luuCoQuan).mockReset();
  datPhien(null);
  datToChuc(null);
  localStorage.clear();
});

test("cá nhân và tổ chức có bộ đệm riêng; tổ chức gọi API kèm id tổ chức", async () => {
  vi.mocked(api.coQuan).mockImplementation(async (tc) => (tc ? CQ("UBND phường Bình Thạnh") : CQ("Cá nhân")));
  const { result } = renderHook(() => useCoQuan());
  await waitFor(() => expect(result.current.cq?.ten_co_quan).toBe("Cá nhân"));
  act(() => datToChuc("t1"));
  await waitFor(() => expect(result.current.cq?.ten_co_quan).toBe("UBND phường Bình Thạnh"));
  expect(vi.mocked(api.coQuan).mock.calls).toEqual([[], ["t1"]]);
  act(() => datToChuc(null));
  expect(result.current.cq?.ten_co_quan).toBe("Cá nhân"); // lấy lại từ bộ đệm, không gọi thêm
  expect(api.coQuan).toHaveBeenCalledTimes(2);
});

test("tổ chức chưa có hồ sơ: không chuyển bản Cơ quan cũ của trình duyệt (bản đó của cá nhân)", async () => {
  localStorage.setItem("vbp-co-quan:u1", JSON.stringify(CQ("Bản cũ cá nhân")));
  vi.mocked(api.coQuan).mockResolvedValue(null);
  datToChuc("t1");
  const { result } = renderHook(() => useCoQuan());
  await waitFor(() => expect(result.current.cq).not.toBeNull());
  expect(result.current.cq?.ten_co_quan).toBe("");
  expect(api.luuCoQuan).not.toHaveBeenCalled();
});

test("lưu ở ngữ cảnh tổ chức gửi kèm id tổ chức", async () => {
  vi.mocked(api.coQuan).mockResolvedValue(CQ("A"));
  vi.mocked(api.luuCoQuan).mockImplementation(async (cq) => cq);
  datToChuc("t1");
  const { result } = renderHook(() => useCoQuan());
  await waitFor(() => expect(result.current.cq).not.toBeNull());
  await act(() => result.current.luu(CQ("B")));
  expect(vi.mocked(api.luuCoQuan).mock.calls[0]).toEqual([CQ("B"), "t1"]);
});

test("đổi ngữ cảnh xóa bản nháp soạn thảo (chữ AI của ngữ cảnh cũ không lưu nhầm sang kho mới)", () => {
  ganBanSoan("u1", { chu: "Bản soạn", loai: "CongVan", ten_mau: null });
  datToChuc("t1");
  expect(docBanSoan("u1")).toBeNull();
});
```

`src/phan-he/soan-thao/co-quan-cai-dat-to-chuc.test.tsx`:

```tsx
import { render, screen } from "@testing-library/react";
import { expect, test, vi } from "vitest";

vi.mock("./co-quan", async (goc) => ({ ...(await goc<object>()),
  useCoQuan: () => ({ cq: { ten_co_quan: "UBND phường Bình Thạnh", xa: "", tinh: "", dac_diem: [], nguoi_ky: "", chuc_danh: "" }, loi: null, luu: vi.fn() }) }));
let vai: "quan_tri" | "can_bo" = "can_bo";
vi.mock("@/to-chuc/NguCanh", () => ({ useToChucId: () => "t1",
  useNguCanh: () => ({ hienTai: { id: "t1", ten: "UBND phường Bình Thạnh", vai_tro: vai } }) }));
import { CoQuanCaiDat } from "./CoQuanCaiDat";

test("ngữ cảnh tổ chức, không phải quản trị: hồ sơ chỉ đọc, không có nút Lưu, có lời giải thích", () => {
  vai = "can_bo";
  render(<CoQuanCaiDat />);
  expect(screen.getByText(/Hồ sơ Cơ quan của «UBND phường Bình Thạnh»/)).toBeInTheDocument();
  expect(screen.getByText(/Chỉ quản trị của tổ chức sửa được/)).toBeInTheDocument();
  expect(screen.getByLabelText("Tên cơ quan ban hành")).toBeDisabled();
  expect(screen.queryByRole("button", { name: "Lưu" })).toBeNull();
});

test("quản trị: sửa được", () => {
  vai = "quan_tri";
  render(<CoQuanCaiDat />);
  expect(screen.getByLabelText("Tên cơ quan ban hành")).toBeEnabled();
  expect(screen.getByRole("button", { name: "Lưu" })).toBeInTheDocument();
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx vitest run src/phan-he/soan-thao`
Expected: FAIL — `api.coQuan` gọi không kèm id; bản nháp không bị xóa; mục Cơ quan chưa có lời giải thích.

- [ ] **Step 3: Sửa `src/phan-he/soan-thao/co-quan.ts`**

```ts
import { useCallback, useEffect, useState } from "react";
import { khiDoiPhien } from "@/api/khach";
import { api } from "@/api/phien";
import { usePhien } from "@/phien/PhienProvider";
import { useToChucId } from "@/to-chuc/NguCanh";

// … CoQuan, CO_QUAN_TRONG, khoaCu, chuoi, chuanHoa, docBanCu, xoaBanCu giữ nguyên …

/** Khóa bộ đệm: một tài khoản có thể có Cơ quan riêng (cá nhân) và Cơ quan của từng tổ chức. */
const khoaDem = (uid: string, tc: string | null) => `${uid}|${tc ?? ""}`;

// Ngoại lệ có kiểm soát của luật «không cache cấp mô-đun»: một GET mỗi ngữ cảnh mỗi lần mở web; xóa khi đăng xuất / đổi tài khoản.
const daCo = new Map<string, CoQuan>();
const dangTai = new Map<string, Promise<CoQuan>>();
khiDoiPhien((p) => {
  for (const m of [daCo, dangTai]) for (const k of [...m.keys()]) if (!p || !k.startsWith(`${p.nguoi_dung.id}|`)) m.delete(k);
});

async function taiMayChu(uid: string, tc: string | null): Promise<CoQuan> {
  const may = await (tc ? api.coQuan(tc) : api.coQuan());
  if (may) return chuanHoa(may);
  if (tc) return { ...CO_QUAN_TRONG }; // tổ chức chưa có hồ sơ: bản cũ trong trình duyệt là của cá nhân, không chuyển sang
  const cu = docBanCu(uid);
  if (!cu) return { ...CO_QUAN_TRONG };
  try {
    const da = chuanHoa(await api.luuCoQuan(cu));
    xoaBanCu(uid);
    return da;
  } catch { return cu; }
}
function tai(uid: string, tc: string | null): Promise<CoQuan> {
  const k = khoaDem(uid, tc);
  const san = daCo.get(k);
  if (san) return Promise.resolve(san);
  let p = dangTai.get(k);
  if (!p) {
    p = taiMayChu(uid, tc).then((cq) => { if (dangTai.get(k) === p) daCo.set(k, cq); return cq; })
      .finally(() => { if (dangTai.get(k) === p) dangTai.delete(k); });
    dangTai.set(k, p);
  }
  return p;
}

const LOI_TAI = "Không tải được thông tin cơ quan. Tải lại trang để thử lại.";
const chuLoi = (e: unknown, mac: string) => (e instanceof Error && e.message ? e.message : mac);

/** Cơ quan của ngữ cảnh đang chọn (cá nhân hoặc tổ chức), lưu trên máy chủ. `cq` null = đang tải (hoặc `loi`). */
export function useCoQuan(): { cq: CoQuan | null; loi: string | null; luu(cq: CoQuan): Promise<void> } {
  const uid = usePhien().nguoiDung?.id ?? "";
  const tc = useToChucId();
  const k = khoaDem(uid, tc);
  const [st, setSt] = useState<{ k: string; cq: CoQuan | null; loi: string | null }>(() => ({ k, cq: daCo.get(k) ?? null, loi: null }));

  useEffect(() => {
    if (!uid) return;
    const san = daCo.get(k);
    if (san) { setSt((s) => (s.k === k && s.cq === san ? s : { k, cq: san, loi: null })); return; }
    let huy = false;
    setSt((s) => (s.k === k && s.cq === null && s.loi === null ? s : { k, cq: null, loi: null }));
    tai(uid, tc).then((cq) => { if (!huy) setSt({ k, cq, loi: null }); }, () => { if (!huy) setSt({ k, cq: null, loi: LOI_TAI }); });
    return () => { huy = true; };
  }, [uid, tc, k]);

  const luu = useCallback(async (moi: CoQuan) => {
    try {
      const da = chuanHoa(await (tc ? api.luuCoQuan(moi, tc) : api.luuCoQuan(moi)));
      daCo.set(k, da);
      setSt({ k, cq: da, loi: null });
    } catch (e) {
      setSt((s) => ({ ...s, loi: chuLoi(e, "Không lưu được. Thử lại.") }));
      throw e;
    }
  }, [tc, k]);

  // Đổi tài khoản / ngữ cảnh giữa hai lần dựng: chưa kịp nạp thì coi như đang tải, không lộ dữ liệu của ngữ cảnh trước.
  const hien = st.k === k ? st : { k, cq: daCo.get(k) ?? null, loi: null };
  return { cq: hien.cq, loi: hien.loi, luu };
}
```

(`diaPhuongTu` giữ nguyên.)

`src/phan-he/soan-thao/ban-soan.ts` — đổi import thành `import { khiDoiPhien, khiDoiToChuc } from "@/api/khach";` và thêm cuối tệp:

```ts
// Đổi ngữ cảnh (cá nhân ↔ tổ chức): bản nháp thuộc kho cũ → bỏ, không để «Lưu vào kho» ghi nhầm sang kho mới.
khiDoiToChuc(() => { ban = null; baoCao = null; });
```

`src/phan-he/soan-thao/CoQuanCaiDat.tsx`:

```tsx
import { useNguCanh } from "@/to-chuc/NguCanh";

export function CoQuanCaiDat() {
  const { cq, loi, luu } = useCoQuan();
  const { hienTai } = useNguCanh();
  const chiXem = !!hienTai && hienTai.vai_tro !== "quan_tri";
  return (
    <section className="the soan__co-quan">
      <h2>Cơ quan</h2>
      <p className="chu-phu">
        {hienTai ? `Hồ sơ Cơ quan của «${hienTai.ten}» — dùng chung cho mọi thành viên khi soạn, quét và lập báo cáo.`
          : "Dùng để điền sẵn khi soạn văn bản và để AI viết đúng địa phương."}
        {chiXem && " Chỉ quản trị của tổ chức sửa được."}
      </p>
      {cq ? <FormCoQuan goc={cq} luu={luu} chiXem={chiXem} />
        : loi ? <p className="chu-loi" role="alert">{loi}</p>
        : <p className="chu-phu" role="status">Đang tải…</p>}
    </section>
  );
}
```

Trong `FormCoQuan`: chữ ký `({ goc, luu: luuMayChu, chiXem = false }: { goc: CoQuan; luu: …; chiXem?: boolean })`; bọc hai
khối ô (`<div className="soan__co-quan-luoi">…</div>` và ô «Đặc điểm địa phương») trong
`<fieldset className="soan__co-quan-khung" disabled={chiXem}>…</fieldset>`; khối nút đổi thành
`{!chiXem && <div className="soan__co-quan-nut">…</div>}`.

`src/phan-he/soan-thao/soan-thao.css` thêm:

```css
.soan__co-quan-khung { border: 0; padding: 0; margin: 0; min-width: 0; }
```

- [ ] **Step 4: Chạy kiểm**

Run: `npx vitest run src/phan-he/soan-thao src/phan-he/quet src/phan-he/ai` rồi `npx tsc -b`
Expected: PASS — `co-quan.test.ts`, `soan-thao.test.tsx`, `quet.test.tsx` cũ không sửa.

- [ ] **Step 5: Commit**

```bash
git add src/phan-he/soan-thao
git commit -m "feat(to-chuc): Co quan va ban nhap soan thao theo ngu canh; Co quan to chuc chi quan tri sua

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 6: Phân hệ Tổ chức — danh sách, tạo, chi tiết + hồ sơ (kèm Cơ quan)

**Files:**
- Create: `src/phan-he/to-chuc/ToChuc.tsx`, `DanhSachToChuc.tsx`, `ChiTietToChuc.tsx`, `HoSoToChuc.tsx`, `to-chuc.css`
- Create (giữ chỗ, Task 7 viết thật): `src/phan-he/to-chuc/ThanhVienToChuc.tsx`, `src/phan-he/to-chuc/GoiToChucTab.tsx`
- Modify: `src/ung-dung.ts` (một dòng), `src/ung-dung.test.ts` (danh sách tên)
- Test: `src/phan-he/to-chuc/to-chuc.test.tsx` (mới)

**Interfaces:**
- Consumes: Task 2 (`toChuc.tao/doc/sua`, `ToChuc`, `TEN_VAI_TRO`), Task 3 (`useNguCanh`), `OChu`, `HopLoi`, `HopBao`, `NutGui`,
  `useGui` (`src/dang-nhap/O.tsx`), `dinhDangNgay` (`src/vo/dinh-dang.ts`).
- Produces: route `/to-chuc` (danh sách + tạo), `/to-chuc/:id` (hồ sơ), `/to-chuc/:id/thanh-vien`, `/to-chuc/:id/goi`;
  `ThanhVienToChuc({ t }: { t: ToChuc })`, `GoiToChucTab({ t }: { t: ToChuc })` (Task 7).

- [ ] **Step 1: Viết bài kiểm `src/phan-he/to-chuc/to-chuc.test.tsx`**

```tsx
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes, useLocation } from "react-router";
import { beforeEach, expect, test, vi } from "vitest";
import { LoiApi } from "@/api/khach";
import type { NguCanh } from "@/to-chuc/NguCanh";

vi.mock("@/api/to-chuc", async (goc) => ({ ...(await goc<object>()),
  toChuc: { tao: vi.fn(), doc: vi.fn(), sua: vi.fn(), thanhVien: vi.fn(async () => []), dsLoiMoi: vi.fn(async () => []), bangGoi: vi.fn(async () => []), roi: vi.fn() } }));
let nc: NguCanh;
vi.mock("@/to-chuc/NguCanh", () => ({ useNguCanh: () => nc, useToChucId: () => nc.hienTai?.id ?? null }));
import { toChuc } from "@/api/to-chuc";
import ToChuc from "./ToChuc";

const TOM = { id: "t1", ten: "UBND phường Bình Thạnh", vai_tro: "quan_tri" as const, goi_ma: "tc-10", goi_ten: "Tổ chức 10 người",
  goi_bat_dau: "2026-11-01T00:00:00Z", goi_het_han: "2027-11-01T00:00:00Z", con_hieu_luc: true };
const DAY_DU = { ...TOM, mst: "0312345678", dia_chi: "Bình Thạnh", email_hoa_don: null, phien_ban: 4, so_thanh_vien: 4,
  so_thanh_vien_toi_da: 10, co_quan: { ten_co_quan: "UBND phường Bình Thạnh", xa: "Phường Bình Thạnh", tinh: "TP Hồ Chí Minh",
  dac_diem: ["Nhiều nhà ở kết hợp kinh doanh"], nguoi_ky: "Trần Thị Mai", chuc_danh: "Phó Chủ tịch" } };
const ViTri = () => <p data-testid="vi-tri">{useLocation().pathname}</p>;
const mo = (duong: string) => render(
  <MemoryRouter initialEntries={[duong]}><Routes><Route path="/to-chuc/*" element={<><ToChuc /><ViTri /></>} /></Routes></MemoryRouter>);

beforeEach(() => {
  vi.clearAllMocks();
  nc = { ds: [], hienTai: null, san: true, chon: vi.fn(), taiLai: vi.fn(async () => [TOM]) };
});

test("chưa thuộc tổ chức: lời giới thiệu + form tạo; tạo xong đọc lại danh sách và mở tổ chức mới", async () => {
  vi.mocked(toChuc.tao).mockResolvedValue(TOM);
  vi.mocked(toChuc.doc).mockResolvedValue(DAY_DU);
  mo("/to-chuc");
  expect(screen.getByText(/chưa thuộc tổ chức nào/)).toBeInTheDocument();
  await userEvent.type(screen.getByLabelText("Tên tổ chức"), "  UBND phường Bình Thạnh ");
  await userEvent.click(screen.getByRole("button", { name: "Tạo tổ chức" }));
  expect(toChuc.tao).toHaveBeenCalledWith({ ten: "UBND phường Bình Thạnh" });
  expect(nc.taiLai).toHaveBeenCalled();
  await waitFor(() => expect(screen.getByTestId("vi-tri")).toHaveTextContent("/to-chuc/t1"));
});

test("tạo quá 3 tổ chức chưa có gói: hiện câu báo của API", async () => {
  vi.mocked(toChuc.tao).mockRejectedValue(new LoiApi(409, "QUA_NHIEU_TO_CHUC", "Mỗi tài khoản tạo tối đa 3 tổ chức chưa có gói."));
  mo("/to-chuc");
  await userEvent.type(screen.getByLabelText("Tên tổ chức"), "X");
  await userEvent.click(screen.getByRole("button", { name: "Tạo tổ chức" }));
  expect(await screen.findByRole("alert")).toHaveTextContent("Mỗi tài khoản tạo tối đa 3 tổ chức chưa có gói.");
});

test("danh sách: tổ chức đang dùng có nhãn; tổ chức khác có nút «Dùng tổ chức này»; tên có thẻ HTML hiện nguyên chữ", async () => {
  const khac = { ...TOM, id: "t2", ten: "<script>alert(1)</script>", vai_tro: "can_bo" as const, con_hieu_luc: false, goi_het_han: null };
  nc = { ...nc, ds: [TOM, khac], hienTai: TOM };
  mo("/to-chuc");
  expect(screen.getByText("Đang dùng")).toBeInTheDocument();
  expect(screen.getByText("<script>alert(1)</script>")).toBeInTheDocument();
  await userEvent.click(screen.getByRole("button", { name: "Dùng tổ chức này" }));
  expect(nc.chon).toHaveBeenCalledWith("t2");
});

test("hồ sơ: quản trị sửa thông tin + Cơ quan, gửi kèm phiên bản", async () => {
  vi.mocked(toChuc.doc).mockResolvedValue(DAY_DU);
  vi.mocked(toChuc.sua).mockResolvedValue({ ...DAY_DU, dia_chi: "Số 6 Phan Đăng Lưu", phien_ban: 5 });
  mo("/to-chuc/t1");
  const diaChi = await screen.findByLabelText("Địa chỉ");
  await userEvent.clear(diaChi);
  await userEvent.type(diaChi, "Số 6 Phan Đăng Lưu");
  await userEvent.click(screen.getByRole("button", { name: "Lưu hồ sơ" }));
  expect(toChuc.sua).toHaveBeenCalledWith("t1", 4, expect.objectContaining({
    dia_chi: "Số 6 Phan Đăng Lưu", co_quan: expect.objectContaining({ ten_co_quan: "UBND phường Bình Thạnh", dac_diem: ["Nhiều nhà ở kết hợp kinh doanh"] }),
  }));
  expect(await screen.findByText("Đã lưu hồ sơ tổ chức.")).toBeInTheDocument();
});

test("hồ sơ: không phải quản trị → ô bị khóa, không có nút lưu", async () => {
  vi.mocked(toChuc.doc).mockResolvedValue({ ...DAY_DU, vai_tro: "van_thu" });
  mo("/to-chuc/t1");
  expect(await screen.findByLabelText("Tên tổ chức")).toBeDisabled();
  expect(screen.queryByRole("button", { name: "Lưu hồ sơ" })).toBeNull();
  expect(screen.getByText("Chỉ quản trị sửa được hồ sơ tổ chức.")).toBeInTheDocument();
});

test("không phải thành viên / id lạ → báo không tìm thấy", async () => {
  vi.mocked(toChuc.doc).mockRejectedValue(new LoiApi(404, "KHONG_TIM_THAY", "Không tìm thấy."));
  mo("/to-chuc/xyz");
  expect(await screen.findByText(/Không tìm thấy tổ chức/)).toBeInTheDocument();
});
```

Sửa `src/ung-dung.test.ts` — danh sách tên thành (đổi cả tên bài kiểm):

```ts
test("đủ 16 phân hệ (14 theo spec §7 + Mua thêm 4.3 + Tổ chức đợt B), href duy nhất, icon có thật", () => {
  expect(PHAN_HE.map((p) => p.ten)).toEqual([
    "Văn bản", "Soạn thảo AI", "Nhập từ ảnh/PDF", "Kiểm tra", "Tóm tắt", "Tham mưu", "Báo cáo định kỳ",
    "Mẫu văn bản", "Tra cứu pháp quy", "Lịch họp", "Album ảnh", "Thùng rác", "Tổ chức", "Mua thêm", "Tài khoản", "Cài đặt",
  ]);
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx vitest run src/phan-he/to-chuc src/ung-dung.test.ts`
Expected: FAIL — không có `./ToChuc`; danh sách phân hệ thiếu «Tổ chức».

- [ ] **Step 3: Viết mã**

`src/ung-dung.ts` — thêm một dòng ngay trước dòng `mua`:

```ts
  { ma: "to-chuc", ten: "Tổ chức", href: "/to-chuc", icon: "don_vi", mau: "cham", tuKhoa: "to chuc co quan thanh vien loi moi van thu can bo goi", trang: lazy(() => import("@/phan-he/to-chuc/ToChuc")) },
```

`src/phan-he/to-chuc/ToChuc.tsx`:

```tsx
// Phân hệ Tổ chức (đợt B): /to-chuc = nơi làm việc + tạo; /to-chuc/:id = hồ sơ, thành viên + lời mời, gói.
import { Route, Routes } from "react-router";
import { ChiTietToChuc } from "./ChiTietToChuc";
import { DanhSachToChuc } from "./DanhSachToChuc";
import "./to-chuc.css";

export default function ToChuc() {
  return (
    <div className="tc">
      <Routes>
        <Route index element={<DanhSachToChuc />} />
        <Route path=":id/*" element={<ChiTietToChuc />} />
      </Routes>
    </div>
  );
}
```

`src/phan-he/to-chuc/DanhSachToChuc.tsx`:

```tsx
import { useId, useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router";
import { TEN_VAI_TRO, toChuc, type ToChucTomTat } from "@/api/to-chuc";
import { HopLoi, NutGui, OChu, useGui } from "@/dang-nhap/O";
import { useNguCanh } from "@/to-chuc/NguCanh";
import { dinhDangNgay } from "@/vo/dinh-dang";

export const moTaGoi = (t: Pick<ToChucTomTat, "con_hieu_luc" | "goi_ten" | "goi_ma" | "goi_het_han">) =>
  t.con_hieu_luc && t.goi_het_han ? `${t.goi_ten ?? t.goi_ma} tới ${dinhDangNgay(t.goi_het_han)}` : "Chưa có gói";

export function DanhSachToChuc() {
  const { ds, hienTai, chon, taiLai } = useNguCanh();
  const navigate = useNavigate();
  const [ten, setTen] = useState("");
  const [mst, setMst] = useState("");
  const { dangGui, loi, chay } = useGui();
  const tieuDe = useId();

  function tao(e: FormEvent) {
    e.preventDefault();
    void chay(async () => {
      const t = await toChuc.tao({ ten: ten.trim(), ...(mst.trim() ? { mst: mst.trim() } : {}) });
      await taiLai();
      navigate(`/to-chuc/${t.id}`);
    });
  }

  return (
    <>
      <h1 className="tc__tieu-de">Tổ chức</h1>
      <section className="the" aria-labelledby={tieuDe}>
        <h2 className="the__tieu-de" id={tieuDe}>Nơi làm việc</h2>
        {ds.length === 0
          ? <p className="chu-phu">Tài khoản chưa thuộc tổ chức nào. Tạo tổ chức mới, hoặc mở đường dẫn trong thư mời.</p>
          : (
            <ul className="tc-ds">
              {ds.map((t) => (
                <li key={t.id} className="tc-ds__muc">
                  <div className="tc-ds__chu">
                    <Link className="tc-ds__ten" to={`/to-chuc/${t.id}`}>{t.ten}</Link>
                    <small className="chu-phu">{TEN_VAI_TRO[t.vai_tro]} · {moTaGoi(t)}</small>
                  </div>
                  {hienTai?.id === t.id
                    ? <span className="nhan-nho nhan-nho--nhan">Đang dùng</span>
                    : <button type="button" className="nut nut--nho" onClick={() => chon(t.id)}>Dùng tổ chức này</button>}
                </li>
              ))}
            </ul>
          )}
        {hienTai && <button type="button" className="nut-chu" onClick={() => chon(null)}>Về kho cá nhân</button>}
      </section>
      <form className="the tc-tao" onSubmit={tao} aria-label="Tạo tổ chức">
        <h2 className="the__tieu-de">Tạo tổ chức</h2>
        <p className="chu-phu">Người tạo là quản trị. Tổ chức chưa có gói: tối đa 5 thành viên, kho chỉ xem, chưa dùng credit chung.</p>
        <OChu nhan="Tên tổ chức" required maxLength={200} placeholder="UBND xã Gia Kiệm" value={ten} onChange={(e) => setTen(e.target.value)} />
        <OChu nhan="Mã số thuế (không bắt buộc)" inputMode="numeric" maxLength={14} value={mst} onChange={(e) => setMst(e.target.value)} />
        <HopLoi loi={loi} />
        <NutGui dangGui={dangGui}>Tạo tổ chức</NutGui>
      </form>
    </>
  );
}
```

`src/phan-he/to-chuc/ChiTietToChuc.tsx`:

```tsx
import { useCallback, useEffect, useState } from "react";
import { Link, NavLink, Route, Routes, useNavigate, useParams } from "react-router";
import { LoiApi } from "@/api/khach";
import { TEN_VAI_TRO, toChuc, type ToChuc } from "@/api/to-chuc";
import { thongBaoLoi } from "@/dang-nhap/thong-bao-loi";
import { useNguCanh } from "@/to-chuc/NguCanh";
import { moTaGoi } from "./DanhSachToChuc";
import { GoiToChucTab } from "./GoiToChucTab";
import { HoSoToChuc } from "./HoSoToChuc";
import { ThanhVienToChuc } from "./ThanhVienToChuc";

type Tt = { loai: "tai" } | { loai: "ok"; t: ToChuc } | { loai: "khong-thay" } | { loai: "loi" };

export function ChiTietToChuc() {
  const { id = "" } = useParams();
  const { hienTai, chon, taiLai } = useNguCanh();
  const navigate = useNavigate();
  const [tt, setTt] = useState<Tt>({ loai: "tai" });
  const [lan, setLan] = useState(0);
  const [bao, setBao] = useState<string | null>(null);
  const tai = useCallback(() => setLan((n) => n + 1), []);

  useEffect(() => {
    let huy = false;
    setTt({ loai: "tai" });
    toChuc.doc(id).then((t) => { if (!huy) setTt({ loai: "ok", t }); },
      (e) => { if (!huy) setTt(e instanceof LoiApi && e.code === "KHONG_TIM_THAY" ? { loai: "khong-thay" } : { loai: "loi" }); });
    return () => { huy = true; };
  }, [id, lan]);

  if (tt.loai === "tai") return <div className="tc-ct" aria-busy="true"><span className="xuong" /><span className="xuong xuong--ngan" /></div>;
  if (tt.loai === "khong-thay") {
    return <div className="tc-ct"><p className="chu-phu">Không tìm thấy tổ chức (đã rời hoặc không thuộc tổ chức này).</p><Link className="lien-ket" to="/to-chuc">Về danh sách tổ chức</Link></div>;
  }
  if (tt.loai === "loi") {
    return <div className="tc-ct"><div className="hop-loi" role="alert">Không tải được tổ chức. <button type="button" className="nut-chu" onClick={tai}>Thử lại</button></div></div>;
  }
  const t = tt.t;
  async function roi() {
    if (!window.confirm(`Rời tổ chức «${t.ten}»? Văn bản của tổ chức vẫn ở lại với tổ chức.`)) return;
    setBao(null);
    try {
      await toChuc.roi(t.id);
      await taiLai();
      if (hienTai?.id === t.id) chon(null);
      else navigate("/to-chuc");
    } catch (e) { setBao(thongBaoLoi(e)); }
  }
  const goc = `/to-chuc/${t.id}`;
  return (
    <article className="tc-ct">
      <header className="tc-ct__dau">
        <div>
          <h1>{t.ten}</h1>
          <p className="chu-phu">{TEN_VAI_TRO[t.vai_tro]} · {moTaGoi(t)} · {t.so_thanh_vien}/{t.so_thanh_vien_toi_da} thành viên</p>
        </div>
        <div className="hang-nut">
          {hienTai?.id === t.id
            ? <span className="nhan-nho nhan-nho--nhan">Đang dùng</span>
            : <button type="button" className="nut nut--nho nut--chinh" onClick={() => chon(t.id)}>Dùng tổ chức này</button>}
          <button type="button" className="nut nut--nho" onClick={() => void roi()}>Rời tổ chức</button>
        </div>
      </header>
      {bao && <div className="hop-loi" role="alert">{bao}</div>}
      <nav className="tc-ct__tab" aria-label="Mục của tổ chức">
        <NavLink end to={goc}>Hồ sơ</NavLink>
        <NavLink to={`${goc}/thanh-vien`}>Thành viên</NavLink>
        <NavLink to={`${goc}/goi`}>Gói và credit</NavLink>
      </nav>
      <Routes>
        <Route index element={<HoSoToChuc t={t} onDoi={(moi) => setTt({ loai: "ok", t: moi })} />} />
        <Route path="thanh-vien" element={<ThanhVienToChuc t={t} />} />
        <Route path="goi" element={<GoiToChucTab t={t} />} />
      </Routes>
    </article>
  );
}
```

`src/phan-he/to-chuc/HoSoToChuc.tsx`:

```tsx
import { useId, useState, type FormEvent } from "react";
import { LoiApi } from "@/api/khach";
import { toChuc, type ToChuc } from "@/api/to-chuc";
import { HopBao, HopLoi, NutGui, OChu, useGui } from "@/dang-nhap/O";
import { CO_QUAN_TRONG, type CoQuan } from "@/phan-he/soan-thao/co-quan";

type ODon = keyof Omit<CoQuan, "dac_diem">;
const O_CQ: Array<[ODon, string]> = [
  ["ten_co_quan", "Tên cơ quan ban hành"], ["xa", "Xã/phường"], ["tinh", "Tỉnh/thành phố"], ["nguoi_ky", "Người ký mặc định"], ["chuc_danh", "Chức danh"],
];

export function HoSoToChuc({ t, onDoi }: { t: ToChuc; onDoi(t: ToChuc): void }) {
  const laQuanTri = t.vai_tro === "quan_tri";
  const cq0 = t.co_quan ?? CO_QUAN_TRONG;
  const [f, setF] = useState({ ten: t.ten, mst: t.mst ?? "", dia_chi: t.dia_chi ?? "", email_hoa_don: t.email_hoa_don ?? "" });
  const [cq, setCq] = useState<CoQuan>(cq0);
  const [dacDiem, setDacDiem] = useState(cq0.dac_diem.join("\n"));
  const [bao, setBao] = useState<string | null>(null);
  const { dangGui, loi, chay } = useGui();
  const idDacDiem = useId();
  const dat = (k: keyof typeof f) => (e: { target: { value: string } }) => { setF((x) => ({ ...x, [k]: e.target.value })); setBao(null); };

  function luu(e: FormEvent) {
    e.preventDefault();
    setBao(null);
    void chay(async () => {
      const moi = await toChuc.sua(t.id, t.phien_ban, {
        ten: f.ten.trim(), mst: f.mst.trim() || null, dia_chi: f.dia_chi.trim() || null, email_hoa_don: f.email_hoa_don.trim() || null,
        co_quan: { ...cq, dac_diem: dacDiem.split(/\r?\n/).map((s) => s.trim()).filter(Boolean).slice(0, 10) },
      });
      onDoi(moi);
      setBao("Đã lưu hồ sơ tổ chức.");
    }, (e) => (e instanceof LoiApi && e.code === "XUNG_DOT_PHIEN_BAN" ? "Hồ sơ vừa được sửa ở nơi khác. Tải lại trang rồi sửa tiếp." : undefined));
  }

  return (
    <form className="the tc-ho-so" onSubmit={luu} aria-label="Hồ sơ tổ chức">
      <fieldset className="tc-ho-so__khung" disabled={!laQuanTri}>
        <legend>Thông tin tổ chức</legend>
        <OChu nhan="Tên tổ chức" required maxLength={200} value={f.ten} onChange={dat("ten")} />
        <OChu nhan="Mã số thuế" inputMode="numeric" maxLength={14} value={f.mst} onChange={dat("mst")} />
        <OChu nhan="Địa chỉ" maxLength={300} value={f.dia_chi} onChange={dat("dia_chi")} />
        <OChu nhan="Email nhận hóa đơn" type="email" maxLength={254} value={f.email_hoa_don} onChange={dat("email_hoa_don")} />
      </fieldset>
      <fieldset className="tc-ho-so__khung" disabled={!laQuanTri}>
        <legend>Cơ quan — dùng chung khi soạn, quét, lập báo cáo</legend>
        {O_CQ.map(([k, nhan]) => (
          <OChu key={k} nhan={nhan} maxLength={300} value={cq[k]} onChange={(e) => { setCq((x) => ({ ...x, [k]: e.target.value })); setBao(null); }} />
        ))}
        <div className="o">
          <label htmlFor={idDacDiem}>Đặc điểm địa phương</label>
          <textarea id={idDacDiem} rows={4} placeholder="Mỗi dòng một ý, tối đa 10 dòng" value={dacDiem}
            onChange={(e) => { setDacDiem(e.target.value); setBao(null); }} />
        </div>
      </fieldset>
      {laQuanTri
        ? <><HopLoi loi={loi} /><HopBao bao={bao} /><NutGui dangGui={dangGui}>Lưu hồ sơ</NutGui></>
        : <p className="chu-phu">Chỉ quản trị sửa được hồ sơ tổ chức.</p>}
    </form>
  );
}
```

Giữ chỗ cho Task 7 (để biên dịch được):

```tsx
// src/phan-he/to-chuc/ThanhVienToChuc.tsx — Task 7 viết thật
import type { ToChuc } from "@/api/to-chuc";
export function ThanhVienToChuc({ t }: { t: ToChuc }) {
  return <section className="the" aria-label="Thành viên"><p className="chu-phu">{t.so_thanh_vien} thành viên</p></section>;
}
```

```tsx
// src/phan-he/to-chuc/GoiToChucTab.tsx — Task 7 viết thật
import type { ToChuc } from "@/api/to-chuc";
export function GoiToChucTab({ t }: { t: ToChuc }) {
  return <section className="the" aria-label="Gói và credit"><p className="chu-phu">{t.goi_ten ?? "Chưa có gói"}</p></section>;
}
```

`src/phan-he/to-chuc/to-chuc.css`:

```css
.tc { display: grid; gap: 16px; padding: 16px; max-width: 960px; margin: 0 auto; }
.tc__tieu-de { margin: 0; font-size: 1.5rem; }
.tc-ds { list-style: none; margin: 0; padding: 0; display: grid; gap: 8px; }
.tc-ds__muc { display: flex; align-items: center; justify-content: space-between; gap: 12px; padding: 10px 12px;
  border: 1px solid var(--vien, rgb(127 127 127 / 0.25)); border-radius: 10px; }
.tc-ds__chu { display: grid; gap: 2px; min-width: 0; }
.tc-ds__ten { font-weight: 600; overflow-wrap: anywhere; }
.tc-tao, .tc-ho-so { display: grid; gap: 12px; }
.tc-ct { display: grid; gap: 16px; }
.tc-ct__dau { display: flex; flex-wrap: wrap; align-items: flex-start; justify-content: space-between; gap: 12px; }
.tc-ct__dau h1 { margin: 0 0 4px; font-size: 1.4rem; overflow-wrap: anywhere; }
.tc-ct__tab { display: flex; gap: 4px; border-bottom: 1px solid var(--vien, rgb(127 127 127 / 0.25)); overflow-x: auto; }
.tc-ct__tab a { padding: 8px 12px; border-bottom: 2px solid transparent; color: inherit; text-decoration: none; white-space: nowrap; }
.tc-ct__tab a.active { border-bottom-color: var(--osk-xanh); font-weight: 600; }
.tc-ho-so__khung { display: grid; gap: 10px; border: 0; padding: 0; margin: 0; min-width: 0; }
.tc-ho-so__khung legend { font-weight: 600; margin-bottom: 4px; }
@media (min-width: 900px) { .tc-ho-so__khung { grid-template-columns: 1fr 1fr; } .tc-ho-so__khung legend { grid-column: 1 / -1; } }
```

- [ ] **Step 4: Chạy kiểm**

Run: `npx vitest run src/phan-he/to-chuc src/ung-dung.test.ts src/App.test.tsx` rồi `npx tsc -b`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/phan-he/to-chuc src/ung-dung.ts src/ung-dung.test.ts
git commit -m "feat(to-chuc): phan he To chuc - noi lam viec, tao to chuc, ho so + Co quan cua to chuc

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 7: Thành viên + lời mời, gói và credit của tổ chức

**Files:**
- Modify (viết thật): `src/phan-he/to-chuc/ThanhVienToChuc.tsx`, `src/phan-he/to-chuc/GoiToChucTab.tsx`, `src/phan-he/to-chuc/to-chuc.css`
- Test: `src/phan-he/to-chuc/thanh-vien.test.tsx` (mới)

**Interfaces:**
- Consumes: Task 2 (`toChuc.thanhVien/doiVaiTro/boThanhVien/moi/dsLoiMoi/huyLoiMoi/bangGoi`, `ThanhVien`, `LoiMoi`, `GoiToChuc`),
  Task 3 (`useNguCanh`), `api.credit()`, `api.dungLuong()` (`src/api/phien.ts`), `usePhien`.
- Produces: `ThanhVienToChuc({ t })`, `GoiToChucTab({ t })`.

- [ ] **Step 1: Viết bài kiểm `src/phan-he/to-chuc/thanh-vien.test.tsx`**

```tsx
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router";
import { beforeEach, expect, test, vi } from "vitest";
import { LoiApi } from "@/api/khach";

vi.mock("@/api/to-chuc", async (goc) => ({ ...(await goc<object>()),
  toChuc: { thanhVien: vi.fn(), doiVaiTro: vi.fn(), boThanhVien: vi.fn(), moi: vi.fn(), dsLoiMoi: vi.fn(), huyLoiMoi: vi.fn(), bangGoi: vi.fn() } }));
vi.mock("@/api/phien", () => ({ api: { credit: vi.fn(async () => ({ tong: 2860, lo: [], het_han_gan_nhat: null })),
  dungLuong: vi.fn(async () => ({ da_dung_byte: 1073741824, han_muc_byte: 53687091200, goi_mb: 51200, them_mb: 0, chi_doc: false, het_goi: false, goi_them: [] })) } }));
vi.mock("@/phien/PhienProvider", () => ({ usePhien: () => ({ nguoiDung: { id: "u_qt" } }) }));
let hienTai: { id: string } | null = { id: "t1" };
vi.mock("@/to-chuc/NguCanh", () => ({ useNguCanh: () => ({ hienTai, chon: vi.fn() }) }));
import { toChuc } from "@/api/to-chuc";
import { GoiToChucTab } from "./GoiToChucTab";
import { ThanhVienToChuc } from "./ThanhVienToChuc";

const T = (vai: "quan_tri" | "can_bo") => ({ id: "t1", ten: "UBND phường Bình Thạnh", vai_tro: vai, goi_ma: "tc-10", goi_ten: "Tổ chức 10 người",
  goi_bat_dau: "2026-11-01T00:00:00Z", goi_het_han: "2027-11-01T00:00:00Z", con_hieu_luc: true, mst: null, dia_chi: null, email_hoa_don: null,
  phien_ban: 1, so_thanh_vien: 3, so_thanh_vien_toi_da: 10, co_quan: null });
const TV = [
  { user_id: "u_qt", ho_ten: "Nguyễn Thị Minh Anh", email: "minhanh@ubnd.vn", vai_tro: "quan_tri" as const, trang_thai: "hoat_dong" as const, them_luc: "2026-11-01T00:00:00Z" },
  { user_id: "u_vt", ho_ten: "Trần Văn Hùng", email: "hung@ubnd.vn", vai_tro: "van_thu" as const, trang_thai: "hoat_dong" as const, them_luc: "2026-11-02T00:00:00Z" },
];
const LM = [{ id: "l1", email: "thu@ubnd.vn", vai_tro: "can_bo" as const, het_han: "2026-11-26T00:00:00Z", tao_luc: "2026-11-19T00:00:00Z", trang_thai: "cho" as const }];

beforeEach(() => {
  vi.clearAllMocks();
  hienTai = { id: "t1" };
  vi.mocked(toChuc.thanhVien).mockResolvedValue(TV);
  vi.mocked(toChuc.dsLoiMoi).mockResolvedValue(LM);
  vi.mocked(toChuc.bangGoi).mockResolvedValue([
    { ma: "tc-10", ten: "Tổ chức 10 người", so_thanh_vien_toi_da: 10, credit_thang: 3000, dung_luong_mb: 51200, max_file_size_mb: 10, gia_nam_vnd: null, dang_ban: false },
  ]);
});

test("quản trị: danh sách thành viên, đổi vai trò, mời qua email, hủy lời mời đang chờ", async () => {
  vi.mocked(toChuc.moi).mockResolvedValue({ ...LM[0], id: "l2", email: "bao@ubnd.vn" });
  render(<MemoryRouter><ThanhVienToChuc t={T("quan_tri")} /></MemoryRouter>);
  const ds = await screen.findByRole("list", { name: "Thành viên" });
  expect(within(ds).getByText("Trần Văn Hùng")).toBeInTheDocument();
  await userEvent.selectOptions(within(ds).getByLabelText("Vai trò của Trần Văn Hùng"), "can_bo");
  expect(toChuc.doiVaiTro).toHaveBeenCalledWith("t1", "u_vt", "can_bo");
  await userEvent.type(screen.getByLabelText("Email người được mời"), "bao@ubnd.vn");
  await userEvent.selectOptions(screen.getByLabelText("Vai trò khi tham gia"), "van_thu");
  await userEvent.click(screen.getByRole("button", { name: "Gửi lời mời" }));
  expect(toChuc.moi).toHaveBeenCalledWith("t1", "bao@ubnd.vn", "van_thu");
  expect(await screen.findByText("Đã gửi lời mời tới bao@ubnd.vn.")).toBeInTheDocument();
  await userEvent.click(screen.getByRole("button", { name: "Hủy lời mời thu@ubnd.vn" }));
  expect(toChuc.huyLoiMoi).toHaveBeenCalledWith("t1", "l1");
});

test("quản trị cuối bị API chặn → hiện câu báo", async () => {
  vi.mocked(toChuc.doiVaiTro).mockRejectedValue(new LoiApi(409, "QUAN_TRI_CUOI", "Tổ chức cần ít nhất một quản trị. Giao quyền quản trị cho người khác trước."));
  render(<MemoryRouter><ThanhVienToChuc t={T("quan_tri")} /></MemoryRouter>);
  await userEvent.selectOptions(await screen.findByLabelText("Vai trò của Nguyễn Thị Minh Anh"), "van_thu");
  expect(await screen.findByRole("alert")).toHaveTextContent("Tổ chức cần ít nhất một quản trị.");
});

test("cán bộ: chỉ xem danh sách, không có form mời, không gọi danh sách lời mời", async () => {
  render(<MemoryRouter><ThanhVienToChuc t={T("can_bo")} /></MemoryRouter>);
  await screen.findByText("Trần Văn Hùng");
  expect(screen.queryByLabelText("Email người được mời")).toBeNull();
  expect(screen.queryByRole("combobox")).toBeNull();
  expect(toChuc.dsLoiMoi).not.toHaveBeenCalled();
});

test("gói: tổ chức đang dùng → credit + dung lượng; gói chưa bán hiện «Sắp mở bán»", async () => {
  render(<MemoryRouter><GoiToChucTab t={T("quan_tri")} /></MemoryRouter>);
  expect(await screen.findByText(/2\.860 credit/)).toBeInTheDocument();
  expect(screen.getByText("Sắp mở bán")).toBeInTheDocument();
  expect(screen.getByRole("link", { name: "Mua gói, credit, dung lượng" })).toHaveAttribute("href", "/mua");
});

test("gói: tổ chức không phải nơi đang dùng → không đọc credit, mời chọn tổ chức", async () => {
  hienTai = null;
  const { api } = await import("@/api/phien");
  render(<MemoryRouter><GoiToChucTab t={T("quan_tri")} /></MemoryRouter>);
  await waitFor(() => expect(toChuc.bangGoi).toHaveBeenCalled());
  expect(screen.getByText(/Chọn «Dùng tổ chức này» để xem credit và dung lượng/)).toBeInTheDocument();
  expect(api.credit).not.toHaveBeenCalled();
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx vitest run src/phan-he/to-chuc/thanh-vien.test.tsx`
Expected: FAIL — giữ chỗ chưa có danh sách / form.

- [ ] **Step 3: Viết `src/phan-he/to-chuc/ThanhVienToChuc.tsx`**

```tsx
import { useEffect, useState, type FormEvent } from "react";
import { TEN_VAI_TRO, toChuc, type LoiMoi, type ThanhVien, type ToChuc, type VaiTro } from "@/api/to-chuc";
import { HopBao, HopLoi, NutGui, OChu, useGui } from "@/dang-nhap/O";
import { thongBaoLoi } from "@/dang-nhap/thong-bao-loi";
import { usePhien } from "@/phien/PhienProvider";
import { dinhDangNgay } from "@/vo/dinh-dang";

const VAI: VaiTro[] = ["quan_tri", "van_thu", "can_bo"];
const TEN_TT_MOI: Record<LoiMoi["trang_thai"], string> = { cho: "Đang chờ", da_dung: "Đã nhận", da_huy: "Đã hủy", het_han: "Hết hạn" };

export function ThanhVienToChuc({ t }: { t: ToChuc }) {
  const laQuanTri = t.vai_tro === "quan_tri";
  const toi = usePhien().nguoiDung?.id;
  const [tv, setTv] = useState<ThanhVien[] | null | "loi">(null);
  const [lm, setLm] = useState<LoiMoi[]>([]);
  const [lan, setLan] = useState(0);
  const [loiThaoTac, setLoiThaoTac] = useState<string | null>(null);
  const [email, setEmail] = useState("");
  const [vaiMoi, setVaiMoi] = useState<VaiTro>("can_bo");
  const [bao, setBao] = useState<string | null>(null);
  const { dangGui, loi, chay } = useGui();

  useEffect(() => {
    let huy = false;
    toChuc.thanhVien(t.id).then((d) => { if (!huy) setTv(d); }, () => { if (!huy) setTv("loi"); });
    if (laQuanTri) toChuc.dsLoiMoi(t.id).then((d) => { if (!huy) setLm(d); }, () => {});
    return () => { huy = true; };
  }, [t.id, laQuanTri, lan]);

  const lam = async (viec: () => Promise<unknown>) => {
    setLoiThaoTac(null);
    try { await viec(); } catch (e) { setLoiThaoTac(thongBaoLoi(e)); }
    setLan((n) => n + 1);
  };
  function moi(e: FormEvent) {
    e.preventDefault();
    setBao(null);
    const den = email.trim();
    void chay(async () => {
      await toChuc.moi(t.id, den, vaiMoi);
      setEmail("");
      setBao(`Đã gửi lời mời tới ${den}.`);
      setLan((n) => n + 1);
    });
  }

  return (
    <>
      <section className="the" aria-labelledby="tc-tv">
        <h2 className="the__tieu-de" id="tc-tv">Thành viên ({t.so_thanh_vien}/{t.so_thanh_vien_toi_da})</h2>
        {loiThaoTac && <div className="hop-loi" role="alert">{loiThaoTac}</div>}
        {tv === null ? <p className="xuong" /> : tv === "loi" ? <p className="chu-phu">Không tải được danh sách thành viên.</p> : (
          <ul className="tc-tv" aria-label="Thành viên">
            {tv.map((u) => (
              <li key={u.user_id} className="tc-tv__muc">
                <div className="tc-ds__chu"><span className="tc-ds__ten">{u.ho_ten}</span><small className="chu-phu">{u.email}</small></div>
                {laQuanTri ? (
                  <span className="hang-nut">
                    <select aria-label={`Vai trò của ${u.ho_ten}`} value={u.vai_tro}
                      onChange={(e) => void lam(() => toChuc.doiVaiTro(t.id, u.user_id, e.target.value as VaiTro))}>
                      {VAI.map((v) => <option key={v} value={v}>{TEN_VAI_TRO[v]}</option>)}
                    </select>
                    {u.user_id !== toi && (
                      <button type="button" className="nut nut--nho nut--nguy" aria-label={`Bỏ ${u.ho_ten} khỏi tổ chức`}
                        onClick={() => { if (window.confirm(`Bỏ ${u.ho_ten} khỏi tổ chức?`)) void lam(() => toChuc.boThanhVien(t.id, u.user_id)); }}>
                        Bỏ
                      </button>
                    )}
                  </span>
                ) : <span className="nhan-nho">{TEN_VAI_TRO[u.vai_tro]}</span>}
              </li>
            ))}
          </ul>
        )}
      </section>
      {laQuanTri && (
        <form className="the tc-moi" onSubmit={moi} aria-label="Mời thành viên">
          <h2 className="the__tieu-de">Mời thành viên</h2>
          <p className="chu-phu">Thư mời gửi tới email; người nhận đăng nhập đúng email đó để tham gia. Lời mời hết hạn sau 7 ngày.</p>
          <OChu nhan="Email người được mời" type="email" required maxLength={254} value={email} onChange={(e) => setEmail(e.target.value)} />
          <div className="o">
            <label htmlFor="tc-vai-moi">Vai trò khi tham gia</label>
            <select id="tc-vai-moi" value={vaiMoi} onChange={(e) => setVaiMoi(e.target.value as VaiTro)}>
              {VAI.map((v) => <option key={v} value={v}>{TEN_VAI_TRO[v]}</option>)}
            </select>
          </div>
          <HopLoi loi={loi} />
          <HopBao bao={bao} />
          <NutGui dangGui={dangGui}>Gửi lời mời</NutGui>
          {lm.length > 0 && (
            <ul className="tc-tv" aria-label="Lời mời gần đây">
              {lm.map((l) => (
                <li key={l.id} className="tc-tv__muc">
                  <div className="tc-ds__chu"><span>{l.email}</span>
                    <small className="chu-phu">{TEN_VAI_TRO[l.vai_tro]} · {TEN_TT_MOI[l.trang_thai]}{l.trang_thai === "cho" ? ` · hết hạn ${dinhDangNgay(l.het_han)}` : ""}</small></div>
                  {l.trang_thai === "cho" && (
                    <button type="button" className="nut nut--nho" aria-label={`Hủy lời mời ${l.email}`}
                      onClick={() => void lam(() => toChuc.huyLoiMoi(t.id, l.id))}>Hủy</button>
                  )}
                </li>
              ))}
            </ul>
          )}
        </form>
      )}
    </>
  );
}
```

- [ ] **Step 4: Viết `src/phan-he/to-chuc/GoiToChucTab.tsx`**

```tsx
import { useEffect, useState } from "react";
import { Link } from "react-router";
import type { DungLuong, SoDu } from "@/api/loai";
import { api } from "@/api/phien";
import { toChuc, type GoiToChuc, type ToChuc } from "@/api/to-chuc";
import { useNguCanh } from "@/to-chuc/NguCanh";
import { dinhDangByte, dinhDangNgay, dinhDangSo } from "@/vo/dinh-dang";

const gb = (mb: number) => `${dinhDangSo(Math.round(mb / 1024))} GB`;

export function GoiToChucTab({ t }: { t: ToChuc }) {
  const { hienTai, chon } = useNguCanh();
  const dangDung = hienTai?.id === t.id;
  const [bang, setBang] = useState<GoiToChuc[] | null>(null);
  const [soDu, setSoDu] = useState<SoDu | null>(null);
  const [dl, setDl] = useState<DungLuong | null>(null);

  useEffect(() => { toChuc.bangGoi().then(setBang, () => setBang([])); }, []);
  // credit / dung lượng đọc theo ngữ cảnh đang chọn (header) → chỉ đọc khi tổ chức này là nơi đang dùng
  useEffect(() => {
    if (!dangDung) return;
    api.credit().then(setSoDu, () => {});
    api.dungLuong().then(setDl, () => {});
  }, [dangDung]);

  return (
    <>
      <section className="the" aria-labelledby="tc-goi">
        <h2 className="the__tieu-de" id="tc-goi">Gói hiện tại</h2>
        {t.con_hieu_luc && t.goi_het_han
          ? <p>{t.goi_ten} · hết hạn {dinhDangNgay(t.goi_het_han)} · {t.so_thanh_vien}/{t.so_thanh_vien_toi_da} thành viên</p>
          : <p>Chưa có gói hoặc gói đã hết hạn: kho của tổ chức chỉ xem, chưa dùng credit chung, tối đa {t.so_thanh_vien_toi_da} thành viên.</p>}
        {dangDung ? (
          <dl className="tc-so-lieu">
            <div><dt>Credit chung</dt><dd>{soDu ? `${dinhDangSo(soDu.tong)} credit` : "…"}</dd></div>
            <div><dt>Dung lượng kho</dt><dd>{dl ? `${dinhDangByte(dl.da_dung_byte)} / ${dinhDangByte(dl.han_muc_byte)}` : "…"}</dd></div>
          </dl>
        ) : (
          <p className="chu-phu">
            Chọn «Dùng tổ chức này» để xem credit và dung lượng của tổ chức.{" "}
            <button type="button" className="nut-chu" onClick={() => chon(t.id)}>Dùng tổ chức này</button>
          </p>
        )}
        {t.vai_tro === "quan_tri" && dangDung && <Link className="nut nut--chinh" to="/mua">Mua gói, credit, dung lượng</Link>}
      </section>
      <section className="the" aria-labelledby="tc-bang-goi">
        <h2 className="the__tieu-de" id="tc-bang-goi">Bảng gói tổ chức (trả theo năm)</h2>
        {bang === null ? <p className="xuong" /> : (
          <table className="tc-bang">
            <thead><tr><th scope="col">Gói</th><th scope="col">Thành viên</th><th scope="col">Credit / tháng</th><th scope="col">Dung lượng</th><th scope="col">Giá / năm</th></tr></thead>
            <tbody>
              {bang.map((g) => (
                <tr key={g.ma} aria-current={g.ma === t.goi_ma ? "true" : undefined}>
                  <th scope="row">{g.ten}</th>
                  <td>{dinhDangSo(g.so_thanh_vien_toi_da)}</td>
                  <td>{dinhDangSo(g.credit_thang)}</td>
                  <td>{gb(g.dung_luong_mb)}</td>
                  <td>{g.dang_ban && g.gia_nam_vnd ? `${dinhDangSo(g.gia_nam_vnd)}đ` : <span className="nhan-nho">Sắp mở bán</span>}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>
    </>
  );
}
```

`src/phan-he/to-chuc/to-chuc.css` — thêm:

```css
.tc-tv { list-style: none; margin: 0; padding: 0; display: grid; gap: 6px; }
.tc-tv__muc { display: flex; flex-wrap: wrap; align-items: center; justify-content: space-between; gap: 8px; padding: 8px 0;
  border-bottom: 1px solid var(--vien, rgb(127 127 127 / 0.18)); }
.tc-moi { display: grid; gap: 12px; }
.tc-so-lieu { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 12px; margin: 12px 0; }
.tc-so-lieu dt { font-size: 0.85rem; opacity: 0.75; }
.tc-so-lieu dd { margin: 2px 0 0; font-size: 1.25rem; font-weight: 600; }
.tc-bang { width: 100%; border-collapse: collapse; font-size: 0.95rem; }
.tc-bang th, .tc-bang td { padding: 8px; text-align: left; border-bottom: 1px solid var(--vien, rgb(127 127 127 / 0.18)); }
.tc-bang tr[aria-current="true"] { background: rgb(37 99 235 / 0.08); }
@media (max-width: 600px) { .tc-bang thead { display: none; } .tc-bang tr { display: grid; grid-template-columns: 1fr 1fr; }
  .tc-bang th[scope="row"] { grid-column: 1 / -1; } }
```

- [ ] **Step 5: Chạy kiểm**

Run: `npx vitest run src/phan-he/to-chuc` rồi `npx tsc -b`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add src/phan-he/to-chuc
git commit -m "feat(to-chuc): thanh vien + loi moi qua email, goi va credit cua to chuc

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 8: Trang `/loi-moi` — nhận lời mời

**Files:**
- Create: `src/to-chuc/TrangLoiMoi.tsx`
- Modify: `src/App.tsx` (route `/loi-moi`)
- Test: `src/to-chuc/loi-moi.test.tsx` (mới)

**Interfaces:**
- Consumes: Task 2 (`toChuc.nhanLoiMoi`, `TEN_VAI_TRO`), Task 3 (`useNguCanh().taiLai/chon`), `usePhien`, `Vo`.
- Produces: route `/loi-moi?ma=<43 ký tự>`; khóa `sessionStorage` `vbp-loi-moi`.

- [ ] **Step 1: Viết bài kiểm `src/to-chuc/loi-moi.test.tsx`**

```tsx
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router";
import { beforeEach, expect, test, vi } from "vitest";
import { LoiApi } from "@/api/khach";

let trangThai: "vao" | "khach" = "khach";
vi.mock("@/phien/PhienProvider", () => ({ usePhien: () => ({ trangThai, nguoiDung: trangThai === "vao" ? { id: "u1", email: "hung@ubnd.vn" } : null }) }));
vi.mock("@/vo/Vo", () => ({ Vo: ({ children }: { children: React.ReactNode }) => <div data-testid="vo">{children}</div> }));
vi.mock("@/api/to-chuc", async (goc) => ({ ...(await goc<object>()), toChuc: { nhanLoiMoi: vi.fn() } }));
const chon = vi.fn();
const taiLai = vi.fn(async () => []);
vi.mock("./NguCanh", () => ({ useNguCanh: () => ({ chon, taiLai }) }));
import { toChuc } from "@/api/to-chuc";
import { TrangLoiMoi } from "./TrangLoiMoi";

const MA = "Q".repeat(43);
const mo = (search: string) => {
  window.history.replaceState(null, "", `/loi-moi${search}`);
  return render(<MemoryRouter><TrangLoiMoi /></MemoryRouter>);
};

beforeEach(() => { vi.clearAllMocks(); sessionStorage.clear(); trangThai = "khach"; });

test("chưa đăng nhập: xóa mã khỏi địa chỉ ngay, giữ trong sessionStorage, nút đăng nhập không mang mã", () => {
  mo(`?ma=${MA}`);
  expect(window.location.search).toBe("");
  expect(sessionStorage.getItem("vbp-loi-moi")).toBe(MA);
  const nut = screen.getByRole("link", { name: "Đăng nhập để nhận lời mời" });
  expect(nut.getAttribute("href")).toBe("/dang-nhap?ve=%2Floi-moi");
  expect(nut.getAttribute("href")).not.toContain(MA);
});

test("đã đăng nhập (quay lại sau đăng nhập): lấy mã từ sessionStorage, nhận → đọc lại danh sách, nút vào tổ chức", async () => {
  sessionStorage.setItem("vbp-loi-moi", MA);
  trangThai = "vao";
  vi.mocked(toChuc.nhanLoiMoi).mockResolvedValue({ to_chuc: { id: "t1", ten: "UBND phường Bình Thạnh", vai_tro: "van_thu",
    goi_ma: null, goi_ten: null, goi_bat_dau: null, goi_het_han: null, con_hieu_luc: false } });
  mo("");
  expect(screen.getByText("hung@ubnd.vn")).toBeInTheDocument();
  await userEvent.click(screen.getByRole("button", { name: "Nhận lời mời" }));
  expect(toChuc.nhanLoiMoi).toHaveBeenCalledWith(MA);
  expect(taiLai).toHaveBeenCalled();
  expect(sessionStorage.getItem("vbp-loi-moi")).toBeNull();
  expect(await screen.findByText(/Đã tham gia «UBND phường Bình Thạnh» với vai trò Văn thư/)).toBeInTheDocument();
  await userEvent.click(screen.getByRole("button", { name: "Bắt đầu làm việc ở tổ chức" }));
  expect(chon).toHaveBeenCalledWith("t1");
});

test("sai email: hiện câu báo của API, giữ mã để đăng nhập đúng tài khoản rồi nhận lại", async () => {
  trangThai = "vao";
  vi.mocked(toChuc.nhanLoiMoi).mockRejectedValue(new LoiApi(403, "SAI_EMAIL_MOI", "Lời mời gửi tới một email khác. Đăng nhập đúng tài khoản được mời rồi mở lại đường dẫn."));
  mo(`?ma=${MA}`);
  await userEvent.click(screen.getByRole("button", { name: "Nhận lời mời" }));
  expect(await screen.findByRole("alert")).toHaveTextContent("Lời mời gửi tới một email khác.");
  expect(sessionStorage.getItem("vbp-loi-moi")).toBe(MA);
});

test("mã sai dạng / không có mã → báo không dùng được, không gọi API", () => {
  trangThai = "vao";
  mo("?ma=ngan");
  expect(screen.getByText(/Không tìm thấy mã mời/)).toBeInTheDocument();
  expect(screen.queryByRole("button", { name: "Nhận lời mời" })).toBeNull();
  expect(window.location.search).toBe("");
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx vitest run src/to-chuc/loi-moi.test.tsx`
Expected: FAIL — không có `./TrangLoiMoi`.

- [ ] **Step 3: Viết `src/to-chuc/TrangLoiMoi.tsx`**

```tsx
import { useState } from "react";
import { Link } from "react-router";
import { LoiApi } from "@/api/khach";
import { TEN_VAI_TRO, toChuc, type ToChucTomTat } from "@/api/to-chuc";
import { HopLoi, useGui } from "@/dang-nhap/O";
import { usePhien } from "@/phien/PhienProvider";
import { Vo } from "@/vo/Vo";
import { useNguCanh } from "./NguCanh";

const KHOA = "vbp-loi-moi";
const laMa = (s: string | null): s is string => !!s && /^[A-Za-z0-9_-]{43}$/.test(s);
const boMa = () => { try { sessionStorage.removeItem(KHOA); } catch { /* không xóa được: mã hết hạn sau 7 ngày */ } };

/** Đọc mã một lần khi mở trang: có ?ma= thì XÓA khỏi thanh địa chỉ ngay (không nằm lại trong lịch sử, Referer, tham số ?ve=
 *  của trang đăng nhập) và giữ ở sessionStorage tới khi nhận xong. */
function layMa(): string | null {
  const ma = new URLSearchParams(window.location.search).get("ma");
  if (ma !== null) {
    window.history.replaceState(window.history.state, "", window.location.pathname);
    if (!laMa(ma)) return null;
    try { sessionStorage.setItem(KHOA, ma); } catch { /* vẫn dùng được trong lần mở trang này */ }
    return ma;
  }
  try {
    const s = sessionStorage.getItem(KHOA);
    return laMa(s) ? s : null;
  } catch { return null; }
}

export function TrangLoiMoi() {
  const { trangThai } = usePhien();
  const [ma] = useState(layMa);
  if (trangThai === "dang-tai") return <div className="man-cho" role="status" aria-label="Đang tải"><span className="vong-xoay" /></div>;
  if (trangThai !== "vao") {
    return (
      <main className="trang-don">
        <h1>Lời mời tham gia tổ chức trên VanBanPlus</h1>
        {ma ? (
          <>
            <p>Đăng nhập bằng đúng email nhận thư mời để tham gia tổ chức.</p>
            <Link className="nut nut--chinh" to={`/dang-nhap?ve=${encodeURIComponent("/loi-moi")}`}>Đăng nhập để nhận lời mời</Link>
          </>
        ) : <p>Không tìm thấy mã mời. Mở lại đường dẫn trong thư mời.</p>}
      </main>
    );
  }
  return <Vo><NhanLoiMoi ma={ma} /></Vo>;
}

function NhanLoiMoi({ ma }: { ma: string | null }) {
  const { taiLai, chon } = useNguCanh();
  const { nguoiDung } = usePhien();
  const { dangGui, loi, chay } = useGui();
  const [xong, setXong] = useState<ToChucTomTat | null>(null);

  if (!ma) {
    return <section className="the trang-loi-moi"><h1>Nhận lời mời</h1><p>Không tìm thấy mã mời. Mở lại đường dẫn trong thư mời.</p></section>;
  }
  const nhan = () => chay(async () => {
    const r = await toChuc.nhanLoiMoi(ma);
    boMa();
    await taiLai();
    setXong(r.to_chuc);
  }, (e) => {
    if (e instanceof LoiApi && e.code === "LOI_MOI_KHONG_HOP_LE") boMa(); // mã chết: không giữ nữa
    return undefined;
  });

  if (xong) {
    return (
      <section className="the trang-loi-moi" aria-live="polite">
        <h1>Đã tham gia tổ chức</h1>
        <p>Đã tham gia «{xong.ten}» với vai trò {TEN_VAI_TRO[xong.vai_tro]}.</p>
        <button type="button" className="nut nut--chinh" onClick={() => chon(xong.id)}>Bắt đầu làm việc ở tổ chức</button>
      </section>
    );
  }
  return (
    <section className="the trang-loi-moi">
      <h1>Nhận lời mời tham gia tổ chức</h1>
      <p>Lời mời chỉ dùng được với tài khoản có đúng email được mời. Đang đăng nhập: <b>{nguoiDung?.email}</b></p>
      <HopLoi loi={loi} />
      <button type="button" className="nut nut--chinh" disabled={dangGui} aria-busy={dangGui} onClick={() => void nhan()}>Nhận lời mời</button>
    </section>
  );
}
```

`src/App.tsx` — trong `CacTrang`, thêm route (không bọc `CanDangNhap`: trang tự xử lý khách / đã đăng nhập):

```tsx
        <Route path="/loi-moi" element={<TrangLoiMoi />} />
```

(cùng `import { TrangLoiMoi } from "@/to-chuc/TrangLoiMoi";`).

- [ ] **Step 4: Chạy kiểm**

Run: `npx vitest run src/to-chuc` rồi `npx tsc -b`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/to-chuc/TrangLoiMoi.tsx src/to-chuc/loi-moi.test.tsx src/App.tsx
git commit -m "feat(to-chuc): trang nhan loi moi - xoa ma khoi dia chi ngay, giu o sessionStorage qua buoc dang nhap

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 9: Phân hệ «Việc được giao»

**Files:**
- Create: `src/phan-he/viec/Viec.tsx`, `src/phan-he/viec/viec.css`
- Modify: `src/ung-dung.ts` (một dòng), `src/ung-dung.test.ts` (danh sách tên)
- Test: `src/phan-he/viec/viec.test.tsx` (mới)

**Interfaces:**
- Consumes: Task 2 (`giaoViec.cuaToi/sua`, `ViecDuocGiao`, `TEN_TRANG_THAI`, `TEN_VAI_XU_LY`), Task 3 (`useNguCanh`),
  `soNgayToiHan` (`src/phan-he/van-ban/ThongTinVanBan.tsx`), `isoSangDdMM`, `DO_KHAN`, `nhan` (`src/phan-he/van-ban/danh-muc.ts`).
- Produces: route `/viec`.

- [ ] **Step 1: Viết bài kiểm `src/phan-he/viec/viec.test.tsx`**

```tsx
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router";
import { beforeEach, expect, test, vi } from "vitest";
import { LoiApi } from "@/api/khach";

vi.mock("@/api/to-chuc", async (goc) => ({ ...(await goc<object>()), giaoViec: { cuaToi: vi.fn(), sua: vi.fn() } }));
let hienTai: { id: string; ten: string } | null = { id: "t1", ten: "UBND phường Bình Thạnh" };
vi.mock("@/to-chuc/NguCanh", () => ({ useNguCanh: () => ({ hienTai }) }));
import { giaoViec } from "@/api/to-chuc";
import Viec from "./Viec";

const V = (id: string, trich_yeu: string, han: string | null, tt: "cho" | "dang" | "xong", vai: "chu_tri" | "phoi_hop" = "chu_tri") => ({
  id, van_ban_id: `vb_${id}`, nguoi_nhan: "u_cb", nguoi_nhan_ten: "Lê Thị Thu", vai, han_xu_ly: han, y_kien_chi_dao: "Tham mưu trước hạn",
  trang_thai: tt, ghi_chu: null, giao_boi: "u_vt", giao_boi_ten: "Trần Văn Hùng", giao_luc: "2026-11-18T01:00:00Z", cap_nhat_luc: "2026-11-18T01:00:00Z",
  phien_ban: 1, van_ban: { id: `vb_${id}`, so: "123", ky_hieu: "UBND-VP", trich_yeu, do_khan: "khan" },
});
const mo = () => render(<MemoryRouter><Viec /></MemoryRouter>);

beforeEach(() => {
  vi.clearAllMocks();
  hienTai = { id: "t1", ten: "UBND phường Bình Thạnh" };
  vi.mocked(giaoViec.cuaToi).mockResolvedValue({ muc: [V("1", "Kế hoạch phòng cháy", "2000-01-01", "cho"), V("2", "Báo cáo quý IV", "2099-01-01", "dang", "phoi_hop")], tong: 2 });
});

test("mặc định lọc «Chưa xong»; việc quá hạn có nhãn; mỗi việc dẫn tới văn bản", async () => {
  mo();
  expect(vi.mocked(giaoViec.cuaToi).mock.calls[0][0]).toEqual({ trang_thai: ["cho", "dang"] });
  const ds = await screen.findByRole("list", { name: "Việc được giao" });
  const v1 = within(ds).getByRole("listitem", { name: /Kế hoạch phòng cháy/ });
  expect(within(v1).getByText(/quá hạn/)).toBeInTheDocument();
  expect(within(v1).getByText("Chủ trì")).toBeInTheDocument();
  expect(within(v1).getByRole("link", { name: "Kế hoạch phòng cháy" })).toHaveAttribute("href", "/van-ban/vb_1");
  expect(within(ds).getByText("Phối hợp")).toBeInTheDocument();
});

test("đổi bộ lọc: Quá hạn / Đã xong / Tất cả", async () => {
  mo();
  await screen.findByRole("list", { name: "Việc được giao" });
  await userEvent.click(screen.getByRole("button", { name: "Quá hạn" }));
  await userEvent.click(screen.getByRole("button", { name: "Đã xong" }));
  await userEvent.click(screen.getByRole("button", { name: "Tất cả" }));
  expect(vi.mocked(giaoViec.cuaToi).mock.calls.slice(1).map((c) => c[0])).toEqual([{ qua_han: true }, { trang_thai: ["xong"] }, {}]);
  expect(screen.getByRole("button", { name: "Tất cả" })).toHaveAttribute("aria-pressed", "true");
});

test("cập nhật nhanh trạng thái gửi kèm phiên bản rồi đọc lại; lỗi hiện câu báo", async () => {
  vi.mocked(giaoViec.sua).mockResolvedValueOnce(V("1", "x", null, "dang")).mockRejectedValueOnce(
    new LoiApi(409, "XUNG_DOT_PHIEN_BAN", "Thông tin vừa được sửa ở nơi khác. Tải lại bản mới rồi sửa tiếp."));
  mo();
  await userEvent.selectOptions(await screen.findByLabelText("Trạng thái: Kế hoạch phòng cháy"), "dang");
  expect(giaoViec.sua).toHaveBeenCalledWith("1", 1, { trang_thai: "dang" });
  await waitFor(() => expect(giaoViec.cuaToi).toHaveBeenCalledTimes(2));
  await userEvent.selectOptions(screen.getByLabelText("Trạng thái: Báo cáo quý IV"), "xong");
  expect(await screen.findByRole("alert")).toHaveTextContent("Thông tin vừa được sửa ở nơi khác.");
});

test("ngữ cảnh cá nhân: lời giải thích + đường tới phân hệ Tổ chức, không gọi API", () => {
  hienTai = null;
  mo();
  expect(screen.getByText(/chỉ có ở tổ chức/)).toBeInTheDocument();
  expect(screen.getByRole("link", { name: "Mở phân hệ Tổ chức" })).toHaveAttribute("href", "/to-chuc");
  expect(giaoViec.cuaToi).not.toHaveBeenCalled();
});

test("chưa có việc: câu báo trống", async () => {
  vi.mocked(giaoViec.cuaToi).mockResolvedValue({ muc: [], tong: 0 });
  mo();
  expect(await screen.findByText("Không có việc nào trong mục này.")).toBeInTheDocument();
});
```

Sửa `src/ung-dung.test.ts` — danh sách tên (17 phân hệ; «Việc được giao» ngay sau «Văn bản»):

```ts
test("đủ 17 phân hệ (14 theo spec §7 + Mua thêm 4.3 + Tổ chức, Việc được giao của đợt B), href duy nhất, icon có thật", () => {
  expect(PHAN_HE.map((p) => p.ten)).toEqual([
    "Văn bản", "Việc được giao", "Soạn thảo AI", "Nhập từ ảnh/PDF", "Kiểm tra", "Tóm tắt", "Tham mưu", "Báo cáo định kỳ",
    "Mẫu văn bản", "Tra cứu pháp quy", "Lịch họp", "Album ảnh", "Thùng rác", "Tổ chức", "Mua thêm", "Tài khoản", "Cài đặt",
  ]);
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx vitest run src/phan-he/viec src/ung-dung.test.ts`
Expected: FAIL — không có `./Viec`.

- [ ] **Step 3: Viết mã**

`src/ung-dung.ts` — một dòng ngay sau dòng `van-ban`:

```ts
  { ma: "viec", ten: "Việc được giao", href: "/viec", icon: "viec", mau: "cam", tuKhoa: "viec giao xu ly chu tri phoi hop han qua han", trang: lazy(() => import("@/phan-he/viec/Viec")) },
```

`src/phan-he/viec/Viec.tsx`:

```tsx
// «Việc được giao» (đợt B): việc của người đang đăng nhập trong tổ chức đang chọn — lọc trạng thái, quá hạn, cập nhật nhanh.
import { useEffect, useState } from "react";
import { Link } from "react-router";
import { giaoViec, TEN_TRANG_THAI, TEN_VAI_XU_LY, type TrangThaiViec, type ViecDuocGiao } from "@/api/to-chuc";
import { thongBaoLoi } from "@/dang-nhap/thong-bao-loi";
import { DO_KHAN, isoSangDdMM, nhan } from "@/phan-he/van-ban/danh-muc";
import { soNgayToiHan } from "@/phan-he/van-ban/ThongTinVanBan";
import { useNguCanh } from "@/to-chuc/NguCanh";
import "./viec.css";

type Loc = "chua-xong" | "qua-han" | "xong" | "tat-ca";
const LOC: Array<{ ma: Loc; ten: string }> = [
  { ma: "chua-xong", ten: "Chưa xong" }, { ma: "qua-han", ten: "Quá hạn" }, { ma: "xong", ten: "Đã xong" }, { ma: "tat-ca", ten: "Tất cả" },
];
const thamSo = (l: Loc): { trang_thai?: TrangThaiViec[]; qua_han?: boolean } =>
  l === "chua-xong" ? { trang_thai: ["cho", "dang"] } : l === "qua-han" ? { qua_han: true } : l === "xong" ? { trang_thai: ["xong"] } : {};
const TRANG_THAI: TrangThaiViec[] = ["cho", "dang", "xong"];

function Han({ v }: { v: ViecDuocGiao }) {
  if (!v.han_xu_ly) return <span className="chu-phu">Không có hạn</span>;
  const con = soNgayToiHan(v.han_xu_ly);
  const qua = con < 0 && v.trang_thai !== "xong";
  return (
    <span className={qua ? "viec__han viec__han--qua" : con <= 3 && v.trang_thai !== "xong" ? "viec__han viec__han--gan" : "viec__han"}>
      Hạn {isoSangDdMM(v.han_xu_ly)}{qua ? " · quá hạn" : v.trang_thai !== "xong" && con >= 0 && con <= 3 ? (con === 0 ? " · hết hạn hôm nay" : ` · còn ${con} ngày`) : ""}
    </span>
  );
}

export default function Viec() {
  const { hienTai } = useNguCanh();
  const [loc, setLoc] = useState<Loc>("chua-xong");
  const [ds, setDs] = useState<{ muc: ViecDuocGiao[]; tong: number } | null | "loi">(null);
  const [lan, setLan] = useState(0);
  const [bao, setBao] = useState<string | null>(null);

  useEffect(() => {
    if (!hienTai) return;
    let huy = false;
    setDs(null);
    giaoViec.cuaToi(thamSo(loc)).then((d) => { if (!huy) setDs(d); }, () => { if (!huy) setDs("loi"); });
    return () => { huy = true; };
  }, [hienTai, loc, lan]);

  if (!hienTai) {
    return (
      <div className="viec">
        <h1 className="viec__tieu-de">Việc được giao</h1>
        <section className="the">
          <p>Việc được giao chỉ có ở tổ chức. Chọn tổ chức ở nút nơi làm việc trên thanh menu, hoặc mở phân hệ Tổ chức.</p>
          <Link className="nut" to="/to-chuc">Mở phân hệ Tổ chức</Link>
        </section>
      </div>
    );
  }
  async function doi(v: ViecDuocGiao, tt: TrangThaiViec) {
    setBao(null);
    try { await giaoViec.sua(v.id, v.phien_ban, { trang_thai: tt }); }
    catch (e) { setBao(thongBaoLoi(e)); }
    setLan((n) => n + 1);
  }
  return (
    <div className="viec">
      <h1 className="viec__tieu-de">Việc được giao</h1>
      <p className="chu-phu">{hienTai.ten}</p>
      <div className="viec__loc" role="group" aria-label="Lọc việc">
        {LOC.map((l) => (
          <button key={l.ma} type="button" className="nut nut--nho" aria-pressed={loc === l.ma} onClick={() => setLoc(l.ma)}>{l.ten}</button>
        ))}
      </div>
      {bao && <div className="hop-loi" role="alert">{bao}</div>}
      {ds === null ? <p className="xuong" aria-busy="true" />
        : ds === "loi" ? <div className="hop-loi" role="alert">Không tải được danh sách việc. <button type="button" className="nut-chu" onClick={() => setLan((n) => n + 1)}>Thử lại</button></div>
        : ds.muc.length === 0 ? <p className="chu-phu">Không có việc nào trong mục này.</p>
        : (
          <ul className="viec__ds" aria-label="Việc được giao">
            {ds.muc.map((v) => (
              <li key={v.id} className="the viec__muc" aria-label={v.van_ban.trich_yeu}>
                <div className="viec__dau">
                  <Link className="viec__trich-yeu" to={`/van-ban/${v.van_ban_id}`}>{v.van_ban.trich_yeu}</Link>
                  <span className="nhan-nho nhan-nho--nhan">{TEN_VAI_XU_LY[v.vai]}</span>
                  {v.van_ban.do_khan !== "thuong" && <span className="nhan-nho nhan-nho--do">{nhan(DO_KHAN, v.van_ban.do_khan)}</span>}
                </div>
                <div className="viec__phu">
                  {[v.van_ban.so, v.van_ban.ky_hieu].filter(Boolean).join("/") && <span>{[v.van_ban.so, v.van_ban.ky_hieu].filter(Boolean).join("/")}</span>}
                  <Han v={v} />
                  {v.giao_boi_ten && <span className="chu-phu">Giao bởi {v.giao_boi_ten}</span>}
                </div>
                {v.y_kien_chi_dao && <p className="viec__y-kien">Ý kiến chỉ đạo: {v.y_kien_chi_dao}</p>}
                <label className="viec__tt">
                  <span className="an-chu">Trạng thái: {v.van_ban.trich_yeu}</span>
                  <select aria-label={`Trạng thái: ${v.van_ban.trich_yeu}`} value={v.trang_thai} onChange={(e) => void doi(v, e.target.value as TrangThaiViec)}>
                    {TRANG_THAI.map((t) => <option key={t} value={t}>{TEN_TRANG_THAI[t]}</option>)}
                  </select>
                </label>
              </li>
            ))}
          </ul>
        )}
    </div>
  );
}
```

`src/phan-he/viec/viec.css`:

```css
.viec { display: grid; gap: 12px; padding: 16px; max-width: 900px; margin: 0 auto; }
.viec__tieu-de { margin: 0; font-size: 1.5rem; }
.viec__loc { display: flex; flex-wrap: wrap; gap: 6px; }
.viec__loc .nut[aria-pressed="true"] { background: var(--osk-xanh); color: #fff; border-color: transparent; }
.viec__ds { list-style: none; margin: 0; padding: 0; display: grid; gap: 10px; }
.viec__muc { display: grid; gap: 6px; }
.viec__dau { display: flex; flex-wrap: wrap; align-items: center; gap: 6px; }
.viec__trich-yeu { font-weight: 600; overflow-wrap: anywhere; }
.viec__phu { display: flex; flex-wrap: wrap; gap: 10px; font-size: 0.9rem; }
.viec__han--qua { color: #dc2626; font-weight: 600; }
.viec__han--gan { color: #b45309; font-weight: 600; }
.viec__y-kien { margin: 0; font-size: 0.9rem; overflow-wrap: anywhere; }
.viec__tt select { min-height: 36px; }
```

- [ ] **Step 4: Chạy kiểm**

Run: `npx vitest run src/phan-he/viec src/ung-dung.test.ts` rồi `npx tsc -b`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/phan-he/viec src/ung-dung.ts src/ung-dung.test.ts
git commit -m "feat(to-chuc): phan he Viec duoc giao - loc trang thai, qua han, cap nhat nhanh

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 10: Chi tiết văn bản ở ngữ cảnh tổ chức — khối «Giao xử lý», công khai, nút theo quyền, dải hết gói

**Files:**
- Create: `src/phan-he/van-ban/KhoiGiaoViec.tsx`, `src/phan-he/van-ban/giao-viec.css`
- Modify: `src/phan-he/van-ban/ChiTiet.tsx`, `src/phan-he/van-ban/DaiChiDoc.tsx`, `src/phan-he/van-ban/VanBan.tsx`
- Test: `src/phan-he/van-ban/giao-viec.test.tsx` (mới)

**Interfaces:**
- Consumes: Task 2 (`giaoViec.*`, `toChuc.thanhVien`, `GiaoViecVanBan`, `ThanhVien`, `VanBan.sua_duoc`, `VanBan.cong_khai_noi_bo`,
  `DungLuong.het_goi`), Task 3 (`useNguCanh`, `useToChucId`), `kho.sua`, `usePhien`.
- Produces: `KhoiGiaoViec({ vb, onDoi })`; `DaiChiDoc({ chiDoc, hetGoi? })`.

- [ ] **Step 1: Viết bài kiểm `src/phan-he/van-ban/giao-viec.test.tsx`**

```tsx
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router";
import { beforeEach, expect, test, vi } from "vitest";
import type { VanBan } from "@/api/kho";

vi.mock("@/api/to-chuc", async (goc) => ({ ...(await goc<object>()),
  giaoViec: { ds: vi.fn(), tao: vi.fn(), sua: vi.fn(), thuHoi: vi.fn() }, toChuc: { thanhVien: vi.fn() } }));
vi.mock("@/api/kho", async (goc) => ({ ...(await goc<object>()), kho: { sua: vi.fn() } }));
let toi = "u_cb";
vi.mock("@/phien/PhienProvider", () => ({ usePhien: () => ({ nguoiDung: { id: toi } }) }));
vi.mock("@/to-chuc/NguCanh", () => ({ useNguCanh: () => ({ hienTai: { id: "t1" } }) }));
import { kho } from "@/api/kho";
import { giaoViec, toChuc } from "@/api/to-chuc";
import { DaiChiDoc } from "./DaiChiDoc";
import { KhoiGiaoViec } from "./KhoiGiaoViec";

const VB = (t: Partial<VanBan> = {}) => ({ id: "vb_3", trich_yeu: "Quyết định 4218", han_xu_ly: "2099-12-01", phien_ban: 2,
  sua_duoc: true, cong_khai_noi_bo: false, ...t }) as VanBan;
const G = (id: string, nguoi: string, ten: string, vai: "chu_tri" | "phoi_hop", tt: "cho" | "dang" | "xong" = "cho") => ({
  id, van_ban_id: "vb_3", nguoi_nhan: nguoi, nguoi_nhan_ten: ten, vai, han_xu_ly: "2099-12-01", y_kien_chi_dao: "<b>Khẩn</b> trình trước 25/11",
  trang_thai: tt, ghi_chu: null, giao_boi: "u_vt", giao_boi_ten: "Trần Văn Hùng", giao_luc: "2026-11-18T01:00:00Z", cap_nhat_luc: "2026-11-18T01:00:00Z", phien_ban: 1 });
const NK = [{ id: 2, giao_viec_id: "g1", nguoi_ten: "Lê Thị Thu", hanh_dong: "trang_thai" as const, truoc: { trang_thai: "cho" }, sau: { trang_thai: "dang" }, luc: "2026-11-18T03:00:00Z" },
  { id: 1, giao_viec_id: "g1", nguoi_ten: "Trần Văn Hùng", hanh_dong: "giao" as const, truoc: null, sau: {}, luc: "2026-11-18T01:00:00Z" }];
const onDoi = vi.fn();
const mo = (vb = VB()) => render(<MemoryRouter><KhoiGiaoViec vb={vb} onDoi={onDoi} /></MemoryRouter>);

beforeEach(() => {
  vi.clearAllMocks();
  toi = "u_cb";
  vi.mocked(toChuc.thanhVien).mockResolvedValue([
    { user_id: "u_cb", ho_ten: "Lê Thị Thu", email: "thu@x.vn", vai_tro: "can_bo", trang_thai: "hoat_dong", them_luc: "" },
    { user_id: "u_cb2", ho_ten: "Phạm Quốc Bảo", email: "bao@x.vn", vai_tro: "can_bo", trang_thai: "hoat_dong", them_luc: "" },
  ]);
});

test("văn thư: thấy danh sách + nhật ký, giao phối hợp (chủ trì đã có thì không chọn được), thu hồi", async () => {
  toi = "u_vt";
  vi.mocked(giaoViec.ds).mockResolvedValue({ muc: [G("g1", "u_cb", "Lê Thị Thu", "chu_tri")], nhat_ky: NK, quyen: { giao: true } });
  vi.mocked(giaoViec.tao).mockResolvedValue(G("g2", "u_cb2", "Phạm Quốc Bảo", "phoi_hop"));
  mo();
  const khoi = await screen.findByRole("region", { name: "Giao xử lý" });
  expect(within(khoi).getByText("Lê Thị Thu")).toBeInTheDocument();
  expect(within(khoi).getByText("<b>Khẩn</b> trình trước 25/11", { exact: false })).toBeInTheDocument(); // chữ thuần
  expect(within(khoi).getByRole("radio", { name: "Chủ trì" })).toBeDisabled();
  await within(khoi).findByRole("option", { name: "Phạm Quốc Bảo" }); // danh sách thành viên nạp sau danh sách việc
  expect(within(khoi).queryByRole("option", { name: "Lê Thị Thu" })).toBeNull(); // đã được giao thì không chọn lại
  await userEvent.selectOptions(within(khoi).getByLabelText("Người nhận"), "u_cb2");
  await userEvent.type(within(khoi).getByLabelText("Ý kiến chỉ đạo"), "Phối hợp số liệu");
  await userEvent.click(within(khoi).getByRole("button", { name: "Giao" }));
  expect(giaoViec.tao).toHaveBeenCalledWith("vb_3", { nguoi_nhan: "u_cb2", vai: "phoi_hop", han_xu_ly: "2099-12-01", y_kien_chi_dao: "Phối hợp số liệu" });
  vi.spyOn(window, "confirm").mockReturnValue(true);
  await userEvent.click(within(khoi).getByRole("button", { name: "Thu hồi việc của Lê Thị Thu" }));
  expect(giaoViec.thuHoi).toHaveBeenCalledWith("g1");
  await userEvent.click(within(khoi).getByText(/Nhật ký/));
  expect(within(khoi).getByText(/Lê Thị Thu · Đổi trạng thái/)).toBeInTheDocument();
});

test("cán bộ: chỉ đọc, không có form giao; đổi trạng thái việc của chính người đó", async () => {
  vi.mocked(giaoViec.ds).mockResolvedValue({ muc: [G("g1", "u_cb", "Lê Thị Thu", "chu_tri"), G("g2", "u_cb2", "Phạm Quốc Bảo", "phoi_hop")], nhat_ky: [], quyen: { giao: false } });
  vi.mocked(giaoViec.sua).mockResolvedValue(G("g1", "u_cb", "Lê Thị Thu", "chu_tri", "dang"));
  mo(VB({ sua_duoc: false }));
  const khoi = await screen.findByRole("region", { name: "Giao xử lý" });
  expect(within(khoi).queryByRole("button", { name: "Giao" })).toBeNull();
  expect(within(khoi).queryByLabelText("Công khai trong cơ quan")).toBeNull();
  expect(within(khoi).queryByLabelText("Trạng thái việc của Phạm Quốc Bảo")).toBeNull();
  await userEvent.selectOptions(within(khoi).getByLabelText("Trạng thái việc của Lê Thị Thu"), "dang");
  expect(giaoViec.sua).toHaveBeenCalledWith("g1", 1, { trang_thai: "dang" });
  expect(toChuc.thanhVien).not.toHaveBeenCalled();
});

test("công khai trong cơ quan: người sửa được bật / tắt qua kho.sua kèm phiên bản", async () => {
  vi.mocked(giaoViec.ds).mockResolvedValue({ muc: [], nhat_ky: [], quyen: { giao: false } });
  vi.mocked(kho.sua).mockResolvedValue(VB({ cong_khai_noi_bo: true, phien_ban: 3 }));
  mo();
  await userEvent.click(await screen.findByLabelText("Công khai trong cơ quan"));
  expect(kho.sua).toHaveBeenCalledWith("vb_3", 2, { cong_khai_noi_bo: true });
  await waitFor(() => expect(onDoi).toHaveBeenCalledWith(expect.objectContaining({ cong_khai_noi_bo: true })));
});

test("dải chỉ đọc: hết gói tổ chức thì câu báo riêng, không mời mua dung lượng", () => {
  render(<DaiChiDoc chiDoc hetGoi />);
  expect(screen.getByRole("alert")).toHaveTextContent("Gói của tổ chức chưa kích hoạt hoặc đã hết hạn");
  expect(screen.queryByRole("link", { name: "Mua thêm dung lượng" })).toBeNull();
});
```

Thêm vào `src/phan-he/van-ban/chi-tiet.test.tsx` **không sửa bài cũ** — tạo tệp riêng
`src/phan-he/van-ban/chi-tiet-to-chuc.test.tsx`:

```tsx
import { render, screen, within } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router";
import { beforeEach, expect, test, vi } from "vitest";

vi.mock("@/api/kho", async (goc) => ({ ...(await goc<object>()), kho: {
  tim: vi.fn(async () => ({ muc: [], tong: 0 })), dsThuMuc: vi.fn(async () => []), doc: vi.fn(), sua: vi.fn(), xoa: vi.fn(), khoiPhuc: vi.fn(),
  taiVe: vi.fn(), xoaTep: vi.fn(), taiTepLen: vi.fn() } }));
vi.mock("@/api/tac-vu", () => ({ tacVu: { uocTinh: vi.fn(async () => ({ credit_min: 4, credit_max: 13, so_du: 100, du_credit: true })), chay: vi.fn() } }));
vi.mock("@/api/phien", () => ({ api: { dungLuong: vi.fn(async () => ({ chi_doc: false, da_dung_byte: 0, han_muc_byte: 1, goi_mb: 1, them_mb: 0, goi_them: [] })) } }));
vi.mock("./su-kien", () => ({ baoKhoDoi: vi.fn(), useKhiKhoDoi: vi.fn() }));
vi.mock("./KhoiGiaoViec", () => ({ KhoiGiaoViec: () => <section aria-label="Giao xử lý" /> }));
let tc: string | null = "t1";
vi.mock("@/to-chuc/NguCanh", () => ({ useToChucId: () => tc, useNguCanh: () => ({ hienTai: tc ? { id: tc } : null }) }));
import { kho } from "@/api/kho";
import VanBan from "./VanBan";

const VB = (t: Record<string, unknown>) => ({ id: "v1", thu_muc_id: null, huong: "den", so: "1", ky_hieu: "X", trich_yeu: "Thử quyền", loai: null,
  co_quan: null, ngay_ban_hanh: null, do_khan: "thuong", do_mat: "thuong", so_den: null, ngay_den: null, han_xu_ly: null, giao_cho: null,
  can_cu: [], noi_nhan: [], nguoi_ky: null, chuc_danh: null, noi_dung: null, tom_tat: null, the: [], lien_quan: [], gan_sao: false,
  ghi_chu: null, tep: [], tao_luc: "2026-11-10T01:00:00Z", sua_luc: "2026-11-10T01:00:00Z", xoa_luc: null, phien_ban: 1, ...t });
const mo = () => render(<MemoryRouter initialEntries={["/van-ban/v1"]}><Routes><Route path="/van-ban/*" element={<VanBan />} /></Routes></MemoryRouter>);

beforeEach(() => { vi.clearAllMocks(); tc = "t1"; });

test("tổ chức, không sửa được: ẩn Sửa / Gắn sao / Xóa, vẫn có Xuất Word + khối Giao xử lý", async () => {
  vi.mocked(kho.doc).mockResolvedValue(VB({ sua_duoc: false }) as never);
  mo();
  const ct = within(await screen.findByRole("region", { name: "Chi tiết văn bản" }));
  await ct.findByRole("heading", { name: "Thử quyền" });
  expect(ct.queryByRole("link", { name: "Sửa" })).toBeNull();
  expect(ct.queryByRole("button", { name: "Gắn sao" })).toBeNull();
  expect(ct.queryByRole("button", { name: "Xóa" })).toBeNull();
  expect(ct.getByRole("button", { name: "Xuất Word" })).toBeInTheDocument();
  expect(ct.getByRole("region", { name: "Giao xử lý" })).toBeInTheDocument();
});

test("cá nhân: không có khối Giao xử lý, nút như cũ", async () => {
  tc = null;
  vi.mocked(kho.doc).mockResolvedValue(VB({}) as never);
  mo();
  const ct = within(await screen.findByRole("region", { name: "Chi tiết văn bản" }));
  await ct.findByRole("heading", { name: "Thử quyền" });
  expect(ct.queryByRole("region", { name: "Giao xử lý" })).toBeNull();
  expect(ct.getByRole("link", { name: "Sửa" })).toBeInTheDocument();
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx vitest run src/phan-he/van-ban/giao-viec.test.tsx src/phan-he/van-ban/chi-tiet-to-chuc.test.tsx`
Expected: FAIL — không có `./KhoiGiaoViec`; `DaiChiDoc` chưa nhận `hetGoi`; chi tiết vẫn hiện Sửa / Xóa.

- [ ] **Step 3: Viết `src/phan-he/van-ban/KhoiGiaoViec.tsx`**

```tsx
// Khối «Giao xử lý» của chi tiết văn bản (đợt B, chỉ ở ngữ cảnh tổ chức). Quyền do API trả (quyen.giao, sua_duoc);
// cán bộ thấy dạng chỉ đọc + đổi trạng thái việc của chính người đó.
import { useEffect, useId, useState, type FormEvent } from "react";
import { kho, type VanBan } from "@/api/kho";
import { giaoViec, TEN_TRANG_THAI, TEN_VAI_XU_LY, toChuc, type GiaoViec, type GiaoViecVanBan, type NhatKyGiaoViec,
  type ThanhVien, type TrangThaiViec, type VaiXuLy } from "@/api/to-chuc";
import { HopLoi, useGui } from "@/dang-nhap/O";
import { thongBaoLoi } from "@/dang-nhap/thong-bao-loi";
import { usePhien } from "@/phien/PhienProvider";
import { useNguCanh } from "@/to-chuc/NguCanh";
import { dinhDangNgay } from "@/vo/dinh-dang";
import { isoSangDdMM } from "./danh-muc";
import "./giao-viec.css";

const TRANG_THAI: TrangThaiViec[] = ["cho", "dang", "xong"];
const TEN_HANH_DONG: Record<NhatKyGiaoViec["hanh_dong"], string> = {
  giao: "Giao việc", sua: "Sửa giao việc", trang_thai: "Đổi trạng thái", ghi_chu: "Ghi chú", thu_hoi: "Thu hồi",
};

type Props = { vb: VanBan; onDoi: (moi: Partial<VanBan> & { id: string }) => void };

export function KhoiGiaoViec({ vb, onDoi }: Props) {
  const toi = usePhien().nguoiDung?.id;
  const { hienTai } = useNguCanh();
  const [dl, setDl] = useState<GiaoViecVanBan | null | "loi">(null);
  const [tv, setTv] = useState<ThanhVien[]>([]);
  const [lan, setLan] = useState(0);
  const [bao, setBao] = useState<string | null>(null);
  const tieuDe = useId();

  useEffect(() => {
    let huy = false;
    giaoViec.ds(vb.id).then((d) => { if (!huy) setDl(d); }, () => { if (!huy) setDl("loi"); });
    return () => { huy = true; };
  }, [vb.id, lan]);
  const coQuyenGiao = dl !== null && dl !== "loi" && dl.quyen.giao;
  useEffect(() => {
    if (!coQuyenGiao || !hienTai) return;
    toChuc.thanhVien(hienTai.id).then(setTv, () => setTv([]));
  }, [coQuyenGiao, hienTai]);

  const lam = async (viec: () => Promise<unknown>) => {
    setBao(null);
    try { await viec(); } catch (e) { setBao(thongBaoLoi(e)); }
    setLan((n) => n + 1);
  };
  async function doiCongKhai(bat: boolean) {
    setBao(null);
    try { onDoi(await kho.sua(vb.id, vb.phien_ban, { cong_khai_noi_bo: bat })); }
    catch (e) { setBao(thongBaoLoi(e)); }
  }

  return (
    <section className="the giao-viec" aria-labelledby={tieuDe}>
      <div className="giao-viec__dau">
        <h3 id={tieuDe}>Giao xử lý</h3>
        {vb.sua_duoc && (
          <label className="giao-viec__cong-khai">
            <input type="checkbox" checked={!!vb.cong_khai_noi_bo} onChange={(e) => void doiCongKhai(e.target.checked)} />
            Công khai trong cơ quan
          </label>
        )}
      </div>
      {bao && <div className="hop-loi" role="alert">{bao}</div>}
      {dl === null ? <p className="xuong" aria-busy="true" />
        : dl === "loi" ? <p className="chu-phu">Không tải được việc của văn bản này.</p>
        : (
          <>
            {dl.muc.length === 0 ? <p className="chu-phu">Chưa giao cho ai.</p> : (
              <ul className="giao-viec__ds">
                {dl.muc.map((g) => (
                  <MucViec key={g.id} g={g} laCuaToi={g.nguoi_nhan === toi} coQuyenGiao={dl.quyen.giao}
                    doiTrangThai={(tt) => void lam(() => giaoViec.sua(g.id, g.phien_ban, { trang_thai: tt }))}
                    thuHoi={() => { if (window.confirm(`Thu hồi việc của ${g.nguoi_nhan_ten ?? "người này"}?`)) void lam(() => giaoViec.thuHoi(g.id)); }} />
                ))}
              </ul>
            )}
            {dl.quyen.giao && (
              <FormGiao vb={vb} tv={tv} daCoChuTri={dl.muc.some((g) => g.vai === "chu_tri")}
                daGiao={new Set(dl.muc.map((g) => g.nguoi_nhan))} onDaGiao={() => setLan((n) => n + 1)} />
            )}
            {dl.nhat_ky.length > 0 && (
              <details className="giao-viec__nhat-ky">
                <summary>Nhật ký ({dl.nhat_ky.length})</summary>
                <ol>
                  {dl.nhat_ky.map((n) => (
                    <li key={n.id}><span>{n.nguoi_ten ?? "Không rõ"} · {TEN_HANH_DONG[n.hanh_dong]}</span>
                      {n.sau && typeof n.sau.trang_thai === "string" && <span> → {TEN_TRANG_THAI[n.sau.trang_thai as TrangThaiViec] ?? ""}</span>}
                      <small className="chu-phu"> · {dinhDangNgay(n.luc)}</small></li>
                  ))}
                </ol>
              </details>
            )}
          </>
        )}
    </section>
  );
}

function MucViec({ g, laCuaToi, coQuyenGiao, doiTrangThai, thuHoi }: {
  g: GiaoViec; laCuaToi: boolean; coQuyenGiao: boolean; doiTrangThai(tt: TrangThaiViec): void; thuHoi(): void;
}) {
  const ten = g.nguoi_nhan_ten ?? "Thành viên";
  return (
    <li className="giao-viec__muc">
      <div className="giao-viec__hang">
        <span className="giao-viec__ten">{ten}</span>
        <span className="nhan-nho nhan-nho--nhan">{TEN_VAI_XU_LY[g.vai]}</span>
        {g.han_xu_ly && <span className="chu-phu">Hạn {isoSangDdMM(g.han_xu_ly)}</span>}
        {laCuaToi || coQuyenGiao ? (
          <select aria-label={`Trạng thái việc của ${ten}`} value={g.trang_thai} onChange={(e) => doiTrangThai(e.target.value as TrangThaiViec)}>
            {TRANG_THAI.map((t) => <option key={t} value={t}>{TEN_TRANG_THAI[t]}</option>)}
          </select>
        ) : <span className="nhan-nho">{TEN_TRANG_THAI[g.trang_thai]}</span>}
        {coQuyenGiao && <button type="button" className="nut nut--nho" aria-label={`Thu hồi việc của ${ten}`} onClick={thuHoi}>Thu hồi</button>}
      </div>
      {g.y_kien_chi_dao && <p className="giao-viec__y-kien">Ý kiến chỉ đạo: {g.y_kien_chi_dao}</p>}
      {g.ghi_chu && <p className="giao-viec__y-kien chu-phu">Ghi chú: {g.ghi_chu}</p>}
    </li>
  );
}

function FormGiao({ vb, tv, daCoChuTri, daGiao, onDaGiao }: {
  vb: VanBan; tv: ThanhVien[]; daCoChuTri: boolean; daGiao: Set<string>; onDaGiao(): void;
}) {
  const [nguoi, setNguoi] = useState("");
  const [vai, setVai] = useState<VaiXuLy>(daCoChuTri ? "phoi_hop" : "chu_tri");
  const [han, setHan] = useState(vb.han_xu_ly ?? "");
  const [yKien, setYKien] = useState("");
  const { dangGui, loi, chay } = useGui();
  const id = useId();
  useEffect(() => { if (daCoChuTri) setVai("phoi_hop"); }, [daCoChuTri]);
  const conChon = tv.filter((u) => !daGiao.has(u.user_id));

  function gui(e: FormEvent) {
    e.preventDefault();
    if (!nguoi) return;
    void chay(async () => {
      await giaoViec.tao(vb.id, { nguoi_nhan: nguoi, vai, han_xu_ly: han || null, ...(yKien.trim() ? { y_kien_chi_dao: yKien.trim() } : {}) });
      setNguoi(""); setYKien("");
      onDaGiao();
    });
  }
  return (
    <form className="giao-viec__form" onSubmit={gui} aria-label="Giao việc">
      <div className="o">
        <label htmlFor={`${id}-n`}>Người nhận</label>
        <select id={`${id}-n`} required value={nguoi} onChange={(e) => setNguoi(e.target.value)}>
          <option value="">— Chọn thành viên —</option>
          {conChon.map((u) => <option key={u.user_id} value={u.user_id}>{u.ho_ten}</option>)}
        </select>
      </div>
      <fieldset className="giao-viec__vai">
        <legend>Vai trò xử lý</legend>
        <label><input type="radio" name={`${id}-v`} checked={vai === "chu_tri"} disabled={daCoChuTri} onChange={() => setVai("chu_tri")} /> Chủ trì</label>
        <label><input type="radio" name={`${id}-v`} checked={vai === "phoi_hop"} onChange={() => setVai("phoi_hop")} /> Phối hợp</label>
      </fieldset>
      <div className="o">
        <label htmlFor={`${id}-h`}>Hạn xử lý</label>
        <input id={`${id}-h`} type="date" value={han} onChange={(e) => setHan(e.target.value)} />
      </div>
      <div className="o giao-viec__o-dai">
        <label htmlFor={`${id}-y`}>Ý kiến chỉ đạo</label>
        <textarea id={`${id}-y`} rows={2} maxLength={2000} value={yKien} onChange={(e) => setYKien(e.target.value)} />
      </div>
      <HopLoi loi={loi} />
      <button type="submit" className="nut nut--chinh" disabled={dangGui || !nguoi} aria-busy={dangGui}>Giao</button>
    </form>
  );
}
```

Ghi chú bài kiểm: nút «Giao» bị `disabled` khi chưa chọn người nhận; bài kiểm chọn người trước khi bấm nên không vướng.

`src/phan-he/van-ban/giao-viec.css`:

```css
.giao-viec { display: grid; gap: 10px; margin-top: 12px; }
.giao-viec__dau { display: flex; flex-wrap: wrap; align-items: center; justify-content: space-between; gap: 8px; }
.giao-viec__dau h3 { margin: 0; font-size: 1.05rem; }
.giao-viec__cong-khai { display: inline-flex; align-items: center; gap: 6px; font-size: 0.95rem; }
.giao-viec__ds { list-style: none; margin: 0; padding: 0; display: grid; gap: 8px; }
.giao-viec__muc { display: grid; gap: 4px; padding: 8px 0; border-bottom: 1px solid var(--vien, rgb(127 127 127 / 0.18)); }
.giao-viec__hang { display: flex; flex-wrap: wrap; align-items: center; gap: 8px; }
.giao-viec__ten { font-weight: 600; }
.giao-viec__y-kien { margin: 0; font-size: 0.9rem; overflow-wrap: anywhere; }
.giao-viec__form { display: grid; gap: 10px; padding-top: 8px; }
.giao-viec__vai { display: flex; flex-wrap: wrap; gap: 14px; border: 0; padding: 0; margin: 0; }
.giao-viec__vai legend { font-weight: 600; margin-bottom: 4px; }
.giao-viec__nhat-ky ol { margin: 6px 0 0; padding-left: 18px; display: grid; gap: 4px; font-size: 0.9rem; }
@media (min-width: 900px) { .giao-viec__form { grid-template-columns: 1fr 1fr; } .giao-viec__o-dai, .giao-viec__form .hop-loi { grid-column: 1 / -1; } }
```

- [ ] **Step 4: Sửa `ChiTiet.tsx`, `DaiChiDoc.tsx`, `VanBan.tsx`**

`src/phan-he/van-ban/ChiTiet.tsx` — trong `NoiDung`:

```tsx
import { useToChucId } from "@/to-chuc/NguCanh";
import { KhoiGiaoViec } from "./KhoiGiaoViec";
// …
  const laToChuc = useToChucId() !== null;
  const suaDuoc = vb.sua_duoc !== false; // kho cá nhân không có khóa này → sửa được như cũ
```

Thay khối nút đầu trang:

```tsx
        <div className="vb-ct__nut">
          {!thungRac && suaDuoc && <Link className="nut nut--nho" to={{ pathname: `/van-ban/${vb.id}/sua`, search }}>Sửa</Link>}
          {!thungRac && suaDuoc && <button type="button" className="nut nut--nho" aria-pressed={vb.gan_sao} disabled={dangLam} onClick={() => void datSao()}>{vb.gan_sao ? "Bỏ sao" : "Gắn sao"}</button>}
          <button type="button" className="nut nut--nho" disabled={xuat === "dang"} aria-busy={xuat === "dang"} onClick={() => void xuatWord()}>Xuất Word</button>
          {!thungRac && suaDuoc && <button type="button" className="nut nut--nho nut--nguy" disabled={dangLam} onClick={xoa}>Xóa</button>}
        </div>
```

Nút «Khôi phục» trong dải thùng rác: thêm điều kiện `suaDuoc &&` trước `<button … onClick={() => void khoiPhuc()}>`. Trong
`vb-ct__chinh`, ngay sau `<ThongTinVanBan vb={vb} />`:

```tsx
          {laToChuc && !thungRac && <KhoiGiaoViec vb={vb} onDoi={onDoi} />}
```

`DanhSachTep`: `chiXem={thungRac || !suaDuoc}`; `NganAi`: `chiXem={thungRac || !suaDuoc}` (vẫn chạy AI được, chỉ không lưu
kết quả vào văn bản không sửa được).

`src/phan-he/van-ban/DaiChiDoc.tsx` — thay `DaiChiDoc`:

```tsx
export function DaiChiDoc({ chiDoc, hetGoi = false }: { chiDoc: boolean; hetGoi?: boolean }) {
  if (!chiDoc) return null;
  if (hetGoi) {
    return (
      <div className="dai-chi-doc vb__chi-doc" role="alert">
        <span>Gói của tổ chức chưa kích hoạt hoặc đã hết hạn — kho của tổ chức đang chỉ đọc. Quản trị gia hạn gói ở phân hệ Tổ chức.</span>
      </div>
    );
  }
  return (
    <div className="dai-chi-doc vb__chi-doc" role="alert">
      <span>Đã vượt dung lượng — kho đang ở chế độ chỉ đọc. Vẫn xem, tìm, tải về, sửa và xóa được; chưa thêm được văn bản hay tệp mới.</span>
      <a className="lien-ket" href={MUA_DUNG_LUONG} target="_blank" rel="noopener noreferrer">Mua thêm dung lượng</a>
    </div>
  );
}
```

`src/phan-he/van-ban/VanBan.tsx`: `<DaiChiDoc chiDoc={cd.chiDoc} hetGoi={!!cd.dl?.het_goi} />`.

- [ ] **Step 5: Chạy kiểm**

Run: `npx vitest run src/phan-he/van-ban` rồi `npx tsc -b`
Expected: PASS — `chi-tiet.test.tsx`, `dai-chi-doc.test.tsx` cũ không sửa (kho cá nhân không có `sua_duoc` → hành vi như cũ).

- [ ] **Step 6: Commit**

```bash
git add src/phan-he/van-ban
git commit -m "feat(to-chuc): chi tiet van ban o to chuc - khoi Giao xu ly, cong khai trong co quan, nut theo quyen, dai het goi

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 11: Mua cho tổ chức

**Files:**
- Create: `src/phan-he/mua/ChonMuaToChuc.tsx`
- Modify: `src/api/mua.ts` (`YeuCauDon` thêm gói tổ chức, `taoDon(y, toChucId?)`, `Don` thêm `toChucId`, `goiToChucMa`, `tieuDeDon`),
  `src/phan-he/mua/Mua.tsx` (tiêu đề + chọn bộ chọn theo ngữ cảnh)
- Test: `src/phan-he/mua/mua-to-chuc.test.tsx` (mới), `src/api/mua.test.ts` (thêm bài, không sửa bài cũ)

**Interfaces:**
- Consumes: Task 2 (`toChuc.bangGoi`, `GoiToChuc`), Task 3 (`useNguCanh`), `mua.bangGia`, `mua.taoDon`.
- Produces: `YeuCauDon` thêm `{ loai: "goi_to_chuc"; ma: string }`; `mua.taoDon(y, toChucId?: string | null)` (có id → thân thêm
  `to_chuc_id`); `ChonMuaToChuc()`.

- [ ] **Step 1: Viết bài kiểm**

Tạo `src/api/mua-to-chuc.test.ts` (cùng khuôn `fetch` giả của `mua.test.ts`; không sửa tệp cũ):

```ts
import { beforeEach, expect, test } from "vitest";
import { caiKhach, datPhien } from "./khach";
import { mua } from "./mua";

const ND = { id: "u1", email: "a@b.vn", ho_ten: "A", goi: "free", goi_het_han: null, co_mat_khau: true, hai_lop_bat: false, google_gan: false };
const tl = (than: unknown) => new Response(JSON.stringify(than), { status: 200, headers: { "Content-Type": "application/json" } });
let than: unknown[];
beforeEach(() => {
  than = [];
  caiKhach({ goc: "https://api.thu", cho: async () => {},
    fetch: (async (_url: string, init: RequestInit) => { than.push(JSON.parse(String(init.body))); return tl({ success: true, data: { code: "VBP1" } }); }) as unknown as typeof fetch });
  datPhien({ access_token: "T", het_han_giay: 900, nguoi_dung: ND });
});

test("đơn tổ chức: thân thêm to_chuc_id; gói tổ chức gửi loai + mã; không có id thì như cũ", async () => {
  await mua.taoDon({ loai: "goi_to_chuc", ma: "tc-30" }, "t1");
  await mua.taoDon({ loai: "nap", ma: "nap-99" }, "t1");
  await mua.taoDon({ loai: "nap", ma: "nap-99" });
  expect(than).toEqual([
    { loai: "goi_to_chuc", goi_to_chuc_ma: "tc-30", to_chuc_id: "t1" },
    { goiNap: "nap-99", to_chuc_id: "t1" },
    { goiNap: "nap-99" },
  ]);
});
```

`src/phan-he/mua/mua-to-chuc.test.tsx`:

```tsx
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes, useLocation } from "react-router";
import { beforeEach, expect, test, vi } from "vitest";

vi.mock("@/api/mua", async (goc) => ({ ...(await goc<object>()), mua: { bangGia: vi.fn(), taoDon: vi.fn(), lichSu: vi.fn(async () => []) } }));
vi.mock("@/api/to-chuc", async (goc) => ({ ...(await goc<object>()), toChuc: { bangGoi: vi.fn() } }));
let hienTai: { id: string; ten: string; vai_tro: "quan_tri" | "can_bo" } | null;
vi.mock("@/to-chuc/NguCanh", () => ({ useNguCanh: () => ({ hienTai }), useToChucId: () => hienTai?.id ?? null }));
import { mua } from "@/api/mua";
import { toChuc } from "@/api/to-chuc";
import Mua from "./Mua";

const ViTri = () => <p data-testid="vi-tri">{useLocation().pathname}</p>;
const mo = () => render(<MemoryRouter initialEntries={["/mua"]}><Routes><Route path="/mua/*" element={<><Mua /><ViTri /></>} /></Routes></MemoryRouter>);

beforeEach(() => {
  vi.clearAllMocks();
  hienTai = { id: "t1", ten: "UBND phường Bình Thạnh", vai_tro: "quan_tri" };
  vi.mocked(mua.bangGia).mockResolvedValue({ goi: [], nap: [{ ma: "nap-99", gia_vnd: 99000, credit: 350, so_ngay: 90 }],
    dungLuong: [{ ma: "dl-10", them_mb: 10240, gia_vnd_thang: 29000 }] });
  vi.mocked(toChuc.bangGoi).mockResolvedValue([
    { ma: "tc-10", ten: "Tổ chức 10 người", so_thanh_vien_toi_da: 10, credit_thang: 3000, dung_luong_mb: 51200, max_file_size_mb: 10, gia_nam_vnd: null, dang_ban: false },
    { ma: "tc-30", ten: "Tổ chức 30 người", so_thanh_vien_toi_da: 30, credit_thang: 8000, dung_luong_mb: 153600, max_file_size_mb: 10, gia_nam_vnd: 6000000, dang_ban: true },
  ]);
});

test("quản trị: tiêu đề theo tổ chức; gói chưa bán không chọn được; tạo đơn gói tổ chức gắn to_chuc_id rồi mở đơn", async () => {
  vi.mocked(mua.taoDon).mockResolvedValue({ code: "VBPTC0001" } as never);
  mo();
  expect(screen.getByRole("heading", { name: "Mua cho «UBND phường Bình Thạnh»" })).toBeInTheDocument();
  expect(await screen.findByRole("radio", { name: /Tổ chức 10 người.*Sắp mở bán/ })).toBeDisabled();
  await userEvent.click(screen.getByRole("radio", { name: /Tổ chức 30 người/ }));
  await userEvent.click(screen.getByRole("button", { name: "Tạo đơn chuyển khoản" }));
  expect(mua.taoDon).toHaveBeenCalledWith({ loai: "goi_to_chuc", ma: "tc-30" }, "t1");
  expect(await screen.findByTestId("vi-tri")).toHaveTextContent("/mua/don/VBPTC0001");
});

test("quản trị: nạp credit cho tổ chức", async () => {
  vi.mocked(mua.taoDon).mockResolvedValue({ code: "VBPTC0002" } as never);
  mo();
  await userEvent.click(await screen.findByRole("tab", { name: "Credit AI" }));
  await userEvent.click(screen.getByRole("radio", { name: /350 credit/ }));
  await userEvent.click(screen.getByRole("button", { name: "Tạo đơn chuyển khoản" }));
  expect(mua.taoDon).toHaveBeenCalledWith({ loai: "nap", ma: "nap-99" }, "t1");
});

test("không phải quản trị: chỉ lời báo, không có nút tạo đơn", async () => {
  hienTai = { id: "t1", ten: "UBND phường Bình Thạnh", vai_tro: "can_bo" };
  mo();
  expect(await screen.findByText("Chỉ quản trị của tổ chức mua gói, credit, dung lượng cho tổ chức.")).toBeInTheDocument();
  expect(screen.queryByRole("button", { name: "Tạo đơn chuyển khoản" })).toBeNull();
});
```

- [ ] **Step 2: Chạy để thấy hỏng**

Run: `npx vitest run src/phan-he/mua src/api`
Expected: FAIL — `taoDon` chưa nhận `toChucId`; không có `ChonMuaToChuc`.

- [ ] **Step 3: Sửa `src/api/mua.ts`**

```ts
export type Don = {
  code: string; loai: "goi" | "nap" | "dung_luong" | "goi_to_chuc"; planName: string; cycle: "month" | "year" | null;
  credit: number | null; soNgay: number | null; themMb: number | null; soThang: number | null; amountVnd: number;
  status: "pending" | "paid" | "cancelled"; createdAt: string; expiresAt: string; paidAt: string | null;
  periodStart: string | null; periodEnd: string | null; bank: { name: string; account: string; holder: string } | null; qr: string | null;
  /** Đợt B: đơn của tổ chức. */
  toChucId?: string | null; goiToChucMa?: string | null;
};
export type YeuCauDon =
  | { loai: "goi"; goi: string; ky: "month" | "year" }
  | { loai: "nap"; ma: string }
  | { loai: "dung_luong"; ma: string; thang: number }
  | { loai: "goi_to_chuc"; ma: string };

function than(y: YeuCauDon): Record<string, unknown> {
  if (y.loai === "nap") return { goiNap: y.ma };
  if (y.loai === "dung_luong") return { loai: "dung_luong", goi_dung_luong_ma: y.ma, so_thang: y.thang };
  if (y.loai === "goi_to_chuc") return { loai: "goi_to_chuc", goi_to_chuc_ma: y.ma };
  return { planId: y.goi, cycle: y.ky };
}
```

`tieuDeDon`: thêm dòng đầu `if (d.loai === "goi_to_chuc") return d.planName;`. `taoDon`:

```ts
  async taoDon(y: YeuCauDon, toChucId?: string | null): Promise<Don> {
    if (y.loai === "dung_luong" && !soThangHopLe(y.thang)) throw new LoiApi(0, "DAU_VAO_SAI", "Số tháng phải từ 1 đến 12.");
    const t = toChucId ? { ...than(y), to_chuc_id: toChucId } : than(y);
    return (await goi<Don>("/api/orders", { phuongThuc: "POST", than: t, hanMs: GHI })).data;
  },
```

`duongMua` không đổi (đường chọn sẵn chỉ cho cá nhân).

- [ ] **Step 4: Viết `src/phan-he/mua/ChonMuaToChuc.tsx`**

```tsx
// Mua cho tổ chức (đợt B): gói tổ chức trả năm · nạp credit vào ví tổ chức · dung lượng cho kho tổ chức.
// Giá, mở bán, quyền quản trị đều do API kiểm; web chỉ gửi mã + to_chuc_id.
import { useEffect, useId, useState } from "react";
import { useNavigate } from "react-router";
import { mua, soThangHopLe, tenDungLuong, type GoiDungLuong, type GoiNap, type YeuCauDon } from "@/api/mua";
import { toChuc, type GoiToChuc } from "@/api/to-chuc";
import { HopLoi, useGui } from "@/dang-nhap/O";
import { thongBaoLoi } from "@/dang-nhap/thong-bao-loi";
import { useNguCanh } from "@/to-chuc/NguCanh";
import { dinhDangSo } from "@/vo/dinh-dang";

type Tab = "goi" | "credit" | "dung-luong";
const TAB: Array<{ ma: Tab; ten: string }> = [{ ma: "goi", ten: "Gói tổ chức" }, { ma: "credit", ten: "Credit AI" }, { ma: "dung-luong", ten: "Dung lượng" }];
const tien = (n: number) => `${dinhDangSo(n)}đ`;

export function ChonMuaToChuc() {
  const { hienTai } = useNguCanh();
  const navigate = useNavigate();
  const [bang, setBang] = useState<{ goi: GoiToChuc[]; nap: GoiNap[]; dl: GoiDungLuong[] } | null | string>(null);
  const [tab, setTab] = useState<Tab>("goi");
  const [chon, setChon] = useState<string | null>(null);
  const [thang, setThang] = useState(12);
  const { dangGui, loi, chay } = useGui();
  const id = useId();
  const laQuanTri = hienTai?.vai_tro === "quan_tri";

  useEffect(() => {
    if (!laQuanTri) return;
    Promise.all([toChuc.bangGoi(), mua.bangGia()])
      .then(([goi, b]) => setBang({ goi, nap: b.nap, dl: b.dungLuong }), (e) => setBang(thongBaoLoi(e)));
  }, [laQuanTri]);

  if (!hienTai) return null;
  if (!laQuanTri) return <section className="the"><p>Chỉ quản trị của tổ chức mua gói, credit, dung lượng cho tổ chức.</p></section>;
  if (bang === null) return <section className="the" aria-busy="true" aria-label="Đang tải bảng giá"><p className="xuong" /></section>;
  if (typeof bang === "string") return <section className="the"><HopLoi loi={bang} /></section>;

  const yeuCau = (): YeuCauDon | null => {
    if (!chon) return null;
    if (tab === "goi") return { loai: "goi_to_chuc", ma: chon };
    if (tab === "credit") return { loai: "nap", ma: chon };
    return soThangHopLe(thang) ? { loai: "dung_luong", ma: chon, thang } : null;
  };
  const taoDon = () => {
    const y = yeuCau();
    if (!y) return;
    void chay(async () => { const d = await mua.taoDon(y, hienTai.id); navigate(`/mua/don/${encodeURIComponent(d.code)}`); });
  };
  const doiTab = (t: Tab) => { setTab(t); setChon(null); };

  // Cùng lớp CSS với ChonMua.tsx (mua-chon, mua-tab, mua-tab__the, mua-bang, mua-nhom, mua-luoi, mua-lua…): không thêm CSS mới.
  return (
    <section className="mua-chon" aria-label="Chọn mua cho tổ chức">
      <div className="mua-tab" role="tablist" aria-label="Loại mua cho tổ chức">
        {TAB.map((t) => (
          <button key={t.ma} type="button" role="tab" id={`${id}-${t.ma}`} aria-selected={tab === t.ma} className="mua-tab__the"
            onClick={() => doiTab(t.ma)}>
            <span className="mua-tab__ten">{t.ten}</span>
          </button>
        ))}
      </div>
      <div className="the mua-bang" role="tabpanel" aria-labelledby={`${id}-${tab}`}>
        <fieldset className="mua-nhom">
          <legend className="an-chu">{TAB.find((t) => t.ma === tab)!.ten}</legend>
          <div className="mua-luoi">
            {tab === "goi" && bang.goi.map((g) => {
              const ban = g.dang_ban && !!g.gia_nam_vnd;
              return (
                <label key={g.ma} className="mua-lua">
                  <input type="radio" name={`${id}-goi`} disabled={!ban} checked={chon === g.ma} onChange={() => setChon(g.ma)} />
                  <span className="mua-lua__than">
                    <span className="mua-lua__ten">{g.ten}</span>
                    <span className="mua-lua__gia">{ban ? `${tien(g.gia_nam_vnd!)}/năm` : "Sắp mở bán"}</span>
                    <span className="mua-lua__phu">{dinhDangSo(g.so_thanh_vien_toi_da)} thành viên · {dinhDangSo(g.credit_thang)} credit/tháng · {tenDungLuong(g.dung_luong_mb)}</span>
                  </span>
                </label>
              );
            })}
            {tab === "credit" && bang.nap.map((g) => (
              <label key={g.ma} className="mua-lua">
                <input type="radio" name={`${id}-nap`} checked={chon === g.ma} onChange={() => setChon(g.ma)} />
                <span className="mua-lua__than">
                  <span className="mua-lua__ten">{dinhDangSo(g.credit)} credit</span>
                  <span className="mua-lua__gia">{tien(g.gia_vnd)}</span>
                  <span className="mua-lua__phu">Dùng trong {g.so_ngay} ngày, cho mọi thành viên</span>
                </span>
              </label>
            ))}
            {tab === "dung-luong" && bang.dl.map((g) => (
              <label key={g.ma} className="mua-lua">
                <input type="radio" name={`${id}-dl`} checked={chon === g.ma} onChange={() => setChon(g.ma)} />
                <span className="mua-lua__than">
                  <span className="mua-lua__ten">Thêm {tenDungLuong(g.them_mb)}</span>
                  <span className="mua-lua__gia">{tien(g.gia_vnd_thang)}/tháng</span>
                </span>
              </label>
            ))}
          </div>
          {tab === "dung-luong" && (
            <div className="o">
              <label htmlFor={`${id}-thang`}>Số tháng</label>
              <select id={`${id}-thang`} value={thang} onChange={(e) => setThang(Number(e.target.value))}>
                {Array.from({ length: 12 }, (_, i) => i + 1).map((n) => <option key={n} value={n}>{n}</option>)}
              </select>
            </div>
          )}
        </fieldset>
        <HopLoi loi={loi} />
        <button type="button" className="nut nut--chinh" disabled={!yeuCau() || dangGui} aria-busy={dangGui} onClick={taoDon}>Tạo đơn chuyển khoản</button>
        <p className="chu-phu">Hóa đơn điện tử gửi về email nhận hóa đơn trong hồ sơ tổ chức sau khi thanh toán.</p>
      </div>
    </section>
  );
}
```

`src/phan-he/mua/Mua.tsx` — `ManChon`:

```tsx
import { useNguCanh } from "@/to-chuc/NguCanh";
import { ChonMuaToChuc } from "./ChonMuaToChuc";

function ManChon() {
  const [lanLich, setLanLich] = useState(0);
  const { hienTai } = useNguCanh();
  return (
    <div className="mua">
      <h1 className="mua__tieu-de">{hienTai ? `Mua cho «${hienTai.ten}»` : "Mua thêm"}</h1>
      {hienTai ? <ChonMuaToChuc /> : <ChonMua khiLoiMayChu={() => setLanLich((n) => n + 1)} />}
      <LichSuDon key={lanLich} />
    </div>
  );
}
```

- [ ] **Step 5: Chạy kiểm**

Run: `npx vitest run src/phan-he/mua src/api` rồi `npx tsc -b`
Expected: PASS; `mua.test.tsx`, `mua-phien.test.tsx` cũ không sửa (ngoài provider → `hienTai: null` → bộ chọn cá nhân).

- [ ] **Step 6: Commit**

```bash
git add src/api/mua.ts src/api/*.test.ts src/phan-he/mua
git commit -m "feat(to-chuc): mua cho to chuc - goi to chuc tra nam, nap credit, dung luong, chi quan tri

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 12: API giả tổ chức + chụp 1440 / 390 (sáng + tối) mọi màn mới

**Files:**
- Create: `cong-cu/api-gia-to-chuc.mjs`, `cong-cu/man-to-chuc.mjs`
- Modify: `cong-cu/api-gia.mjs` (kịch bản tổ chức, header CORS, `/to-chuc` rỗng cho kịch bản cũ), `cong-cu/chup.mjs` (thêm màn)

**Interfaces:**
- Produces: `TC_ID`, `MA_MOI_GIA`, `NGUOI`, `KICH_BAN_TO_CHUC`, `taoKhoGia({ coSan })`, `traLoiToChuc(kb, phuongThuc, url, than, tc, kho)`,
  `chonToChuc(uid)` (`truocKhiMo` ghi `vbp-ngu-canh:<uid>`); `ganApiGia(page, kichBan, pdf, kho = taoKhoGia())`;
  `MAN_TO_CHUC` (cùng khuôn `MAN` của `chup.mjs`).

- [ ] **Step 1: Viết `cong-cu/api-gia-to-chuc.mjs`**

```js
// API giả đợt B (tổ chức) cho bộ chụp + video — đúng «Hợp đồng API cho web» trong
// docs/superpowers/plans/2026-11-19-dot-b-to-chuc-api.md. Kịch bản: tc-quan-tri, tc-het-goi, tc-moi (Nguyễn Thị Minh Anh),
// tc-van-thu (Trần Văn Hùng), tc-can-bo (Lê Thị Thu). Kho trạng thái (taoKhoGia) dùng chung giữa các trang của MỘT lần quay
// video; bộ chụp tạo kho mới cho mỗi trang. Dữ liệu là giả; gói tổ chức chưa có giá (chủ dự án chốt) nên đều «Sắp mở bán».
import { VAN_BAN, traLoiVanBan } from "./api-gia-van-ban.mjs";

export const TC_ID = "7c1e0a52-0000-4000-8000-000000000001";
/** Mã mời giả đúng dạng (43 ký tự base64url) cho màn /loi-moi — không phải mã thật. */
export const MA_MOI_GIA = "MaMoiGiaChiDeChupManHinh".padEnd(43, "x");
const NGAY = 86_400_000;
const GB = 1024 ** 3;
const iso = (ms) => new Date(Date.now() + ms).toISOString();
const ngay = (lech) => new Date(Date.now() + 7 * 3_600_000 + lech * NGAY).toISOString().slice(0, 10);
const ok = (data, message = "") => ({ status: 200, body: { success: true, message, data } });
const loi = (status, code, message) => ({ status, body: { success: false, message, code } });

export const NGUOI = {
  qt: { id: "u_7f3a91", email: "minhanh.nguyen@ubnd-phuongbinhthanh.gov.vn", ho_ten: "Nguyễn Thị Minh Anh" },
  vt: { id: "u_vt_02", email: "hung.tran@ubnd-phuongbinhthanh.gov.vn", ho_ten: "Trần Văn Hùng" },
  cb: { id: "u_cb_03", email: "thu.le@ubnd-phuongbinhthanh.gov.vn", ho_ten: "Lê Thị Thu" },
  cb2: { id: "u_cb_04", email: "bao.pham@ubnd-phuongbinhthanh.gov.vn", ho_ten: "Phạm Quốc Bảo" },
};
const NGUOI_CUA = { "tc-quan-tri": NGUOI.qt, "tc-het-goi": NGUOI.qt, "tc-moi": NGUOI.qt, "tc-van-thu": NGUOI.vt, "tc-can-bo": NGUOI.cb };
export const KICH_BAN_TO_CHUC = Object.keys(NGUOI_CUA);
const nguoiTheoId = (id) => Object.values(NGUOI).find((n) => n.id === id);

const CO_QUAN = { ten_co_quan: "UBND phường Bình Thạnh", xa: "Phường Bình Thạnh", tinh: "Thành phố Hồ Chí Minh",
  dac_diem: ["Phường trung tâm, nhiều nhà ở kết hợp kinh doanh", "Có kênh Thanh Đa, hay ngập khi triều cường"],
  nguoi_ky: "Nguyễn Thị Minh Anh", chuc_danh: "Chủ tịch" };
export const GOI = [
  { ma: "tc-10", ten: "Tổ chức 10 người", so_thanh_vien_toi_da: 10, credit_thang: 3000, dung_luong_mb: 51200, max_file_size_mb: 10, gia_nam_vnd: null, dang_ban: false },
  { ma: "tc-30", ten: "Tổ chức 30 người", so_thanh_vien_toi_da: 30, credit_thang: 8000, dung_luong_mb: 153600, max_file_size_mb: 10, gia_nam_vnd: null, dang_ban: false },
  { ma: "tc-100", ten: "Tổ chức 100 người", so_thanh_vien_toi_da: 100, credit_thang: 25000, dung_luong_mb: 512000, max_file_size_mb: 10, gia_nam_vnd: null, dang_ban: false },
];
const toChucMau = (id, ten) => ({
  id, ten, mst: "0312345678", dia_chi: "6 Phan Đăng Lưu, phường Bình Thạnh", email_hoa_don: "ketoan@ubnd-phuongbinhthanh.gov.vn",
  // gói giả còn hạn để quay được vòng thao tác (gói thật chưa mở bán)
  goi_ma: "tc-30", goi_ten: "Tổ chức 30 người", goi_bat_dau: iso(-40 * NGAY), goi_het_han: iso(325 * NGAY), phien_ban: 1, co_quan: CO_QUAN,
});
const viec = (id, van_ban_id, n, vai, trang_thai, han, y) => ({
  id, van_ban_id, nguoi_nhan: n.id, nguoi_nhan_ten: n.ho_ten, vai, han_xu_ly: han, y_kien_chi_dao: y, trang_thai, ghi_chu: null,
  giao_boi: NGUOI.vt.id, giao_boi_ten: NGUOI.vt.ho_ten, giao_luc: iso(-2 * NGAY), cap_nhat_luc: iso(-NGAY), phien_ban: 1, huy: false,
});

/** coSan = false: kho trống (video bắt đầu từ «tạo tổ chức»). */
export function taoKhoGia({ coSan = true } = {}) {
  const tv = (user_id, vai_tro, lech) => ({ to_chuc_id: TC_ID, user_id, vai_tro, them_luc: iso(lech * NGAY) });
  return {
    dem: 100,
    toChuc: coSan ? [toChucMau(TC_ID, "UBND phường Bình Thạnh")] : [],
    thanhVien: coSan ? [tv(NGUOI.qt.id, "quan_tri", -40), tv(NGUOI.vt.id, "van_thu", -39), tv(NGUOI.cb.id, "can_bo", -38), tv(NGUOI.cb2.id, "can_bo", -30)] : [],
    loiMoi: coSan ? [{ id: "lm_1", to_chuc_id: TC_ID, email: "khoa.nguyen@ubnd-phuongbinhthanh.gov.vn", vai_tro: "can_bo",
      het_han: iso(5 * NGAY), tao_luc: iso(-2 * NGAY), trang_thai: "cho", ma: "y".repeat(43) }] : [],
    giaoViec: coSan ? [
      viec("g_1", "vb_3", NGUOI.cb, "chu_tri", "dang", ngay(4), "Tham mưu kế hoạch triển khai, trình trước ngày 25/11"),
      viec("g_2", "vb_3", NGUOI.cb2, "phoi_hop", "cho", ngay(4), "Phối hợp rà soát số liệu cán bộ, công chức"),
      viec("g_3", "vb_5", NGUOI.cb, "chu_tri", "cho", ngay(-2), "Báo cáo tiến độ giải phóng mặt bằng"),
      viec("g_4", "vb_2", NGUOI.cb, "phoi_hop", "xong", ngay(-6), null),
    ] : [],
    nhatKy: coSan ? [
      { id: 3, giao_viec_id: "g_1", nguoi_ten: NGUOI.cb.ho_ten, hanh_dong: "trang_thai", truoc: { trang_thai: "cho" }, sau: { trang_thai: "dang" }, luc: iso(-NGAY) },
      { id: 2, giao_viec_id: "g_2", nguoi_ten: NGUOI.vt.ho_ten, hanh_dong: "giao", truoc: null, sau: {}, luc: iso(-2 * NGAY) },
      { id: 1, giao_viec_id: "g_1", nguoi_ten: NGUOI.vt.ho_ten, hanh_dong: "giao", truoc: null, sau: {}, luc: iso(-2 * NGAY) },
    ] : [],
    vanBanMoi: [],
    ghiDe: {}, // sửa văn bản có sẵn (VAN_BAN dùng chung giữa các trang) → ghi đè trong kho, không sửa mảng gốc
  };
}

const vaiTro = (kho, uid, tc) => kho.thanhVien.find((t) => t.to_chuc_id === tc && t.user_id === uid)?.vai_tro ?? null;
function tomTat(t, vai, kb) {
  const het = kb === "tc-het-goi";
  return { id: t.id, ten: t.ten, vai_tro: vai, goi_ma: het ? null : t.goi_ma, goi_ten: het ? null : t.goi_ten,
    goi_bat_dau: het ? null : t.goi_bat_dau, goi_het_han: het ? null : t.goi_het_han, con_hieu_luc: !het && !!t.goi_ma };
}
const dayDu = (kho, t, vai, kb) => ({
  ...tomTat(t, vai, kb), mst: t.mst, dia_chi: t.dia_chi, email_hoa_don: t.email_hoa_don, phien_ban: t.phien_ban,
  so_thanh_vien: kho.thanhVien.filter((x) => x.to_chuc_id === t.id).length, so_thanh_vien_toi_da: kb === "tc-het-goi" || !t.goi_ma ? 5 : 30,
  co_quan: t.co_quan,
});
const THU_TU = ["quan_tri", "van_thu", "can_bo"];
const tvJson = (x) => { const n = nguoiTheoId(x.user_id); return { user_id: x.user_id, ho_ten: n?.ho_ten ?? "Thành viên", email: n?.email ?? "",
  vai_tro: x.vai_tro, trang_thai: "hoat_dong", them_luc: x.them_luc }; };
const lmJson = ({ ma: _ma, to_chuc_id: _tc, ...l }) => l;
const gvJson = ({ huy: _h, ...g }) => g;
const vbGoc = (kho, id) => kho.vanBanMoi.find((v) => v.id === id) ?? VAN_BAN.find((v) => v.id === id) ?? null;
const vbCua = (kho, id) => { const v = vbGoc(kho, id); return v ? { ...v, ...(kho.ghiDe[id] ?? {}) } : null; };
const congKhai = (v) => v.cong_khai_noi_bo ?? v.id === "vb_12";
function xemDuoc(kho, v, vai, uid) {
  if (vai !== "can_bo") return true;
  return v.tao_boi_gia === uid || congKhai(v) || kho.giaoViec.some((g) => !g.huy && g.van_ban_id === v.id && g.nguoi_nhan === uid);
}
const tang = (v, vai, uid) => ({ ...v, cong_khai_noi_bo: congKhai(v), sua_duoc: vai !== "can_bo" || v.tao_boi_gia === uid });

/** Trả { status, body } cho đường thuộc đợt B; null = để API giả cũ xử lý. `tc` = header X-VBP-To-Chuc (hoặc null). */
export function traLoiToChuc(kb, phuongThuc, url, than, tc, kho) {
  const toi = NGUOI_CUA[kb];
  if (!toi) return null;
  const r = url.pathname.replace(/^\/api\/v2/, "");
  const nguoiDung = { ...toi, goi: "starter", goi_het_han: iso(23 * NGAY), co_mat_khau: true, hai_lop_bat: false, google_gan: false };
  if (r === "/phien/lam-moi") return ok({ access_token: `token-gia-${kb}`, het_han_giay: 900, nguoi_dung: nguoiDung });
  if (r === "/toi" && phuongThuc === "GET") return ok(nguoiDung);

  // ---- tổ chức, thành viên, lời mời (không đọc header) ----
  if (r === "/goi-to-chuc") return ok(GOI);
  if (r === "/to-chuc" && phuongThuc === "GET") {
    return ok(kho.toChuc.filter((t) => vaiTro(kho, toi.id, t.id)).map((t) => tomTat(t, vaiTro(kho, toi.id, t.id), kb)));
  }
  if (r === "/to-chuc" && phuongThuc === "POST") {
    const t = { ...toChucMau(kho.toChuc.length ? `tc_${++kho.dem}` : TC_ID, String(than?.ten ?? "").trim() || "Tổ chức mới"),
      mst: than?.mst ?? null, co_quan: null };
    kho.toChuc.push(t);
    kho.thanhVien.push({ to_chuc_id: t.id, user_id: toi.id, vai_tro: "quan_tri", them_luc: iso(0) });
    return ok(tomTat(t, "quan_tri", kb), "Đã tạo tổ chức.");
  }
  let m = /^\/to-chuc\/([^/]+)(\/.*)?$/.exec(r);
  if (m) {
    const t = kho.toChuc.find((x) => x.id === decodeURIComponent(m[1]));
    const vai = t ? vaiTro(kho, toi.id, t.id) : null;
    if (!t || !vai) return loi(404, "KHONG_TIM_THAY", "Không tìm thấy.");
    const con = m[2] ?? "";
    const chiQuanTri = () => (vai === "quan_tri" ? null : loi(403, "KHONG_DU_QUYEN", "Vai trò trong tổ chức không làm được thao tác này."));
    if (con === "" && phuongThuc === "GET") return ok(dayDu(kho, t, vai, kb));
    if (con === "" && phuongThuc === "PUT") {
      const c = chiQuanTri(); if (c) return c;
      const { phien_ban: _pb, ...thay } = than ?? {};
      Object.assign(t, thay, { phien_ban: t.phien_ban + 1 });
      return ok(dayDu(kho, t, vai, kb), "Đã lưu hồ sơ tổ chức.");
    }
    if (con === "/thanh-vien" && phuongThuc === "GET") {
      return ok(kho.thanhVien.filter((x) => x.to_chuc_id === t.id)
        .sort((a, b) => THU_TU.indexOf(a.vai_tro) - THU_TU.indexOf(b.vai_tro)).map(tvJson));
    }
    let n = /^\/thanh-vien\/([^/]+)$/.exec(con);
    if (n) {
      const c = chiQuanTri(); if (c) return c;
      const i = kho.thanhVien.findIndex((x) => x.to_chuc_id === t.id && x.user_id === decodeURIComponent(n[1]));
      if (i < 0) return loi(404, "KHONG_TIM_THAY", "Không tìm thấy.");
      if (phuongThuc === "PATCH") { kho.thanhVien[i].vai_tro = than?.vai_tro; return ok(tvJson(kho.thanhVien[i]), "Đã đổi vai trò."); }
      kho.thanhVien.splice(i, 1);
      return ok(null, "Đã bỏ thành viên khỏi tổ chức.");
    }
    if (con === "/roi" && phuongThuc === "POST") {
      kho.thanhVien = kho.thanhVien.filter((x) => !(x.to_chuc_id === t.id && x.user_id === toi.id));
      return ok(null, "Đã rời tổ chức.");
    }
    if (con === "/loi-moi") {
      const c = chiQuanTri(); if (c) return c;
      if (phuongThuc === "GET") return ok(kho.loiMoi.filter((l) => l.to_chuc_id === t.id).map(lmJson));
      const l = { id: `lm_${++kho.dem}`, to_chuc_id: t.id, email: String(than?.email ?? "").trim().toLowerCase(), vai_tro: than?.vai_tro ?? "can_bo",
        het_han: iso(7 * NGAY), tao_luc: iso(0), trang_thai: "cho", ma: `MaMoiGia${kho.dem}`.padEnd(43, "x") };
      kho.loiMoi.unshift(l);
      return ok(lmJson(l), "Đã gửi lời mời.");
    }
    n = /^\/loi-moi\/([^/]+)$/.exec(con);
    if (n && phuongThuc === "DELETE") {
      const l = kho.loiMoi.find((x) => x.id === decodeURIComponent(n[1]));
      if (l && l.trang_thai === "cho") l.trang_thai = "da_huy";
      return ok(null, "Đã hủy lời mời.");
    }
    return null;
  }
  if (r === "/loi-moi/nhan" && phuongThuc === "POST") {
    const l = kho.loiMoi.find((x) => x.ma === than?.ma && x.trang_thai === "cho");
    if (!l) return loi(400, "LOI_MOI_KHONG_HOP_LE", "Lời mời không hợp lệ, đã dùng, đã hủy hoặc đã hết hạn.");
    if (l.email !== toi.email) return loi(403, "SAI_EMAIL_MOI", "Lời mời gửi tới một email khác. Đăng nhập đúng tài khoản được mời rồi mở lại đường dẫn.");
    l.trang_thai = "da_dung";
    kho.thanhVien.push({ to_chuc_id: l.to_chuc_id, user_id: toi.id, vai_tro: l.vai_tro, them_luc: iso(0) });
    return ok({ to_chuc: tomTat(kho.toChuc.find((x) => x.id === l.to_chuc_id), l.vai_tro, kb) }, "Đã tham gia tổ chức.");
  }

  // ---- ngữ cảnh tổ chức (header X-VBP-To-Chuc) ----
  if (!tc) return null;
  const vai = vaiTro(kho, toi.id, tc);
  if (!vai) return loi(403, "KHONG_THUOC_TO_CHUC", "Tài khoản không thuộc tổ chức này hoặc đã rời tổ chức.");
  const het = kb === "tc-het-goi";
  if (r === "/credit") {
    return ok({ tong: het ? 0 : 7340, het_han_gan_nhat: het ? null : iso(12 * NGAY), lo: het ? [] : [
      { nguon: "goi_thang", so_cap: 8000, so_con: 6840, het_han: iso(12 * NGAY) }, { nguon: "nap", so_cap: 500, so_con: 500, het_han: iso(80 * NGAY) }] });
  }
  if (r === "/kho/dung-luong") {
    return ok({ da_dung_byte: Math.round(18.4 * GB), han_muc_byte: het ? 0 : 150 * GB, goi_mb: het ? 0 : 153600, them_mb: 0, cho_byte: 0,
      chi_doc: het, het_goi: het, goi_them: [] });
  }
  if (r === "/toi/co-quan" && phuongThuc === "GET") return ok({ co_quan: kho.toChuc.find((t) => t.id === tc)?.co_quan ?? null });
  if (r === "/kho/giao-viec/cua-toi") {
    const tt = url.searchParams.get("trang_thai")?.split(",");
    const quaHan = url.searchParams.get("qua_han") === "1";
    const homNay = ngay(0);
    const muc = kho.giaoViec.filter((g) => !g.huy && g.nguoi_nhan === toi.id)
      .filter((g) => !tt || tt.includes(g.trang_thai))
      .filter((g) => !quaHan || (g.han_xu_ly && g.han_xu_ly < homNay && g.trang_thai !== "xong"))
      .sort((a, b) => (a.trang_thai === "xong") - (b.trang_thai === "xong") || String(a.han_xu_ly ?? "9").localeCompare(String(b.han_xu_ly ?? "9")))
      .map((g) => { const v = vbCua(kho, g.van_ban_id);
        return { ...gvJson(g), van_ban: { id: v.id, so: v.so, ky_hieu: v.ky_hieu, trich_yeu: v.trich_yeu, do_khan: v.do_khan } }; });
    return ok({ muc, tong: muc.length });
  }
  m = /^\/kho\/giao-viec\/([^/]+)$/.exec(r);
  if (m) {
    const g = kho.giaoViec.find((x) => x.id === decodeURIComponent(m[1]) && !x.huy);
    if (!g) return loi(404, "KHONG_TIM_THAY", "Không tìm thấy.");
    if (phuongThuc === "DELETE") {
      if (vai === "can_bo") return loi(403, "KHONG_DU_QUYEN", "Vai trò trong tổ chức không làm được thao tác này.");
      g.huy = true;
      kho.nhatKy.unshift({ id: ++kho.dem, giao_viec_id: g.id, nguoi_ten: toi.ho_ten, hanh_dong: "thu_hoi", truoc: null, sau: null, luc: iso(0) });
      return ok(null, "Đã thu hồi.");
    }
    if (vai === "can_bo" && g.nguoi_nhan !== toi.id) return loi(403, "KHONG_DU_QUYEN", "Vai trò trong tổ chức không làm được thao tác này.");
    const truoc = { trang_thai: g.trang_thai };
    const { phien_ban: _pb, ...thay } = than ?? {};
    Object.assign(g, thay, { phien_ban: g.phien_ban + 1, cap_nhat_luc: iso(0) });
    kho.nhatKy.unshift({ id: ++kho.dem, giao_viec_id: g.id, nguoi_ten: toi.ho_ten, hanh_dong: "trang_thai", truoc, sau: { trang_thai: g.trang_thai }, luc: iso(0) });
    return ok(gvJson(g), "Đã cập nhật.");
  }
  m = /^\/kho\/van-ban\/([^/]+)\/giao-viec$/.exec(r);
  if (m) {
    const id = decodeURIComponent(m[1]);
    const v = vbCua(kho, id);
    if (!v || !xemDuoc(kho, v, vai, toi.id)) return loi(404, "KHONG_TIM_THAY", "Không tìm thấy.");
    if (phuongThuc === "GET") {
      const cua = kho.giaoViec.filter((g) => g.van_ban_id === id);
      return ok({ muc: cua.filter((g) => !g.huy).map(gvJson), nhat_ky: kho.nhatKy.filter((n) => cua.some((g) => g.id === n.giao_viec_id)),
        quyen: { giao: vai !== "can_bo" } });
    }
    if (vai === "can_bo") return loi(403, "KHONG_DU_QUYEN", "Vai trò trong tổ chức không làm được thao tác này.");
    const n = nguoiTheoId(than?.nguoi_nhan);
    if (!n) return loi(400, "NGUOI_NHAN_KHONG_HOP_LE", "Người nhận phải là thành viên đang hoạt động của tổ chức.");
    const g = { ...viec(`g_${++kho.dem}`, id, n, than.vai, "cho", than.han_xu_ly ?? v.han_xu_ly ?? null, than.y_kien_chi_dao ?? null),
      giao_boi: toi.id, giao_boi_ten: toi.ho_ten, giao_luc: iso(0), cap_nhat_luc: iso(0) };
    kho.giaoViec.push(g);
    kho.nhatKy.unshift({ id: ++kho.dem, giao_viec_id: g.id, nguoi_ten: toi.ho_ten, hanh_dong: "giao", truoc: null, sau: {}, luc: iso(0) });
    return ok(gvJson(g), "Đã giao xử lý.");
  }
  if (r === "/kho/van-ban" && phuongThuc === "POST") {
    const v = { ...VAN_BAN[6], ...(than ?? {}), id: `vb_moi_${++kho.dem}`, thu_muc_id: null, tep: [], tao_boi_gia: toi.id,
      tao_luc: iso(0), sua_luc: iso(0), xoa_luc: null, phien_ban: 1, gan_sao: false };
    delete v.khoa_tao;
    kho.vanBanMoi.unshift(v);
    return ok(tang(v, vai, toi.id), "Đã lưu văn bản.");
  }
  if (r === "/kho/van-ban" && phuongThuc === "GET") {
    const goc = traLoiVanBan("vao", "GET", url).body.data.muc.map((v) => vbCua(kho, v.id));
    const ds = [...(url.searchParams.get("thung_rac") === "1" ? [] : kho.vanBanMoi), ...goc].filter((v) => xemDuoc(kho, v, vai, toi.id));
    return ok({ muc: ds.map((v) => tang(v, vai, toi.id)), tong: ds.length });
  }
  m = /^\/kho\/van-ban\/([^/]+)$/.exec(r);
  if (m) {
    const id = decodeURIComponent(m[1]);
    const v = vbCua(kho, id);
    if (!v || !xemDuoc(kho, v, vai, toi.id)) return loi(404, "KHONG_TIM_THAY", "Không tìm thấy.");
    if (phuongThuc === "GET") return ok(tang(v, vai, toi.id));
    if (phuongThuc === "PATCH") {
      if (!tang(v, vai, toi.id).sua_duoc) return loi(403, "KHONG_DU_QUYEN", "Vai trò trong tổ chức không làm được thao tác này.");
      const { phien_ban: _pb, ...thay } = than ?? {};
      kho.ghiDe[id] = { ...(kho.ghiDe[id] ?? {}), ...thay, phien_ban: v.phien_ban + 1 };
      return ok(tang(vbCua(kho, id), vai, toi.id), "Đã lưu.");
    }
  }
  return null; // thư mục, tệp, tác vụ AI, mẫu, lịch họp… dùng API giả cũ
}

/** truocKhiMo của bộ chụp: chọn sẵn tổ chức cho người dùng uid (NguCanhProvider khôi phục từ localStorage). */
export const chonToChuc = (uid) => (ctx) => ctx.addInitScript(([k, v]) => {
  try { localStorage.setItem(k, v); } catch { /* bỏ qua */ }
}, [`vbp-ngu-canh:${uid}`, TC_ID]);
```

- [ ] **Step 2: Sửa `cong-cu/api-gia.mjs`**

```js
import { KICH_BAN_TO_CHUC, taoKhoGia, traLoiToChuc } from "./api-gia-to-chuc.mjs";

const DAU_CHO_PHEP = "Content-Type, Authorization, X-VBP-Nguon, X-VBP-Phien-Ban, X-VBP-To-Chuc";
```

Trong `traLoi(...)`, ngay sau dòng `if (r === "/toi" && phuongThuc === "GET") return ok(nguoiDung());` thêm:

```js
  if (r === "/to-chuc" && phuongThuc === "GET") return ok([]); // kịch bản cũ: tài khoản chưa thuộc tổ chức nào
```

`ganApiGia`:

```js
export async function ganApiGia(page, kichBan, pdf, kho = taoKhoGia()) {
  if (!["khach", "vao", "chi-doc", ...KICH_BAN_TO_CHUC].includes(kichBan)) throw new Error(`Kịch bản lạ: ${kichBan}`);
  const kbCu = KICH_BAN_TO_CHUC.includes(kichBan) ? "vao" : kichBan; // phần ngoài đợt B dùng dữ liệu của kịch bản «vao»
  // … cors giữ nguyên …
  await page.route("**/api/v2/**", async (route) => {
    const yc = route.request();
    if (yc.method() === "OPTIONS") {
      await route.fulfill({ status: 204, headers: { ...cors, "Access-Control-Allow-Methods": "GET, POST, PUT, PATCH, DELETE, OPTIONS",
        "Access-Control-Allow-Headers": DAU_CHO_PHEP, "Access-Control-Max-Age": "600" } });
      return;
    }
    const u = new URL(yc.url());
    let than;
    try { than = yc.postData() ? JSON.parse(yc.postData()) : undefined; } catch { than = undefined; }
    const tc = traLoiToChuc(kichBan, yc.method(), u, than, yc.headers()["x-vbp-to-chuc"] ?? null, kho);
    if (tc) { await route.fulfill({ status: tc.status, headers: cors, contentType: "application/json; charset=utf-8", body: JSON.stringify(tc.body) }); return; }
    // … phần còn lại giữ nguyên, thay `kichBan` bằng `kbCu` trong hai lời gọi traLoiVanBan(...) và traLoi(...)
  });
  // route MAU_DUONG_MUA: "Access-Control-Allow-Headers": DAU_CHO_PHEP (thay chuỗi cũ); phần còn lại giữ nguyên
}
```

- [ ] **Step 3: Viết `cong-cu/man-to-chuc.mjs` và gắn vào `chup.mjs`**

```js
// Màn đợt B (tổ chức) cho bộ chụp (cùng khuôn MAN của chup.mjs). `truocKhiMo: chonToChuc(uid)` = đang làm việc ở tổ chức.
import { chonToChuc, MA_MOI_GIA, NGUOI, TC_ID } from "./api-gia-to-chuc.mjs";

const qt = chonToChuc(NGUOI.qt.id);
const vt = chonToChuc(NGUOI.vt.id);
const cb = chonToChuc(NGUOI.cb.id);

export const MAN_TO_CHUC = [
  // nút nơi làm việc trên thanh menu, đang mở menu (điện thoại: nút ở đầu màn hình chính)
  { ten: "tc-ngu-canh", kb: "tc-quan-tri", duong: "/", toi: true, giuKhung: true, truocKhiMo: qt, lam: async (p) => {
    const nut = p.getByRole("button", { name: /UBND phường Bình Thạnh/ }).first();
    if (await nut.isVisible()) await nut.click();
  } },
  { ten: "tc-danh-sach", kb: "tc-quan-tri", duong: "/to-chuc", toi: true },
  { ten: "tc-tao", kb: "tc-quan-tri", duong: "/to-chuc", lam: async (p) => {
    await p.getByLabel("Tên tổ chức").fill("Trường THCS Gia Kiệm");
  } },
  { ten: "tc-ho-so", kb: "tc-quan-tri", duong: `/to-chuc/${TC_ID}`, toi: true, truocKhiMo: qt },
  { ten: "tc-ho-so-van-thu", kb: "tc-van-thu", duong: `/to-chuc/${TC_ID}`, truocKhiMo: vt },
  { ten: "tc-thanh-vien", kb: "tc-quan-tri", duong: `/to-chuc/${TC_ID}/thanh-vien`, toi: true, truocKhiMo: qt },
  { ten: "tc-thanh-vien-can-bo", kb: "tc-can-bo", duong: `/to-chuc/${TC_ID}/thanh-vien`, truocKhiMo: cb },
  { ten: "tc-goi", kb: "tc-quan-tri", duong: `/to-chuc/${TC_ID}/goi`, toi: true, truocKhiMo: qt },
  { ten: "tc-loi-moi-khach", kb: "khach", duong: `/loi-moi?ma=${MA_MOI_GIA}` },
  { ten: "tc-loi-moi", kb: "vao", duong: `/loi-moi?ma=${MA_MOI_GIA}`, toi: true },
  { ten: "tc-viec", kb: "tc-can-bo", duong: "/viec", toi: true, truocKhiMo: cb },
  { ten: "tc-viec-qua-han", kb: "tc-can-bo", duong: "/viec", truocKhiMo: cb, lam: async (p) => {
    await p.getByRole("button", { name: "Quá hạn" }).click();
  } },
  { ten: "tc-viec-ca-nhan", kb: "vao", duong: "/viec" },
  { ten: "tc-van-ban", kb: "tc-van-thu", duong: "/van-ban", toi: true, truocKhiMo: vt },
  { ten: "tc-van-ban-giao-viec", kb: "tc-van-thu", duong: "/van-ban/vb_3", toi: true, truocKhiMo: vt, lam: async (p) => {
    await p.getByText(/Nhật ký/).first().click();
  } },
  { ten: "tc-van-ban-can-bo", kb: "tc-can-bo", duong: "/van-ban/vb_3", toi: true, truocKhiMo: cb },
  { ten: "tc-van-ban-ds-can-bo", kb: "tc-can-bo", duong: "/van-ban", truocKhiMo: cb },
  { ten: "tc-het-goi", kb: "tc-het-goi", duong: "/van-ban", toi: true, truocKhiMo: qt },
  { ten: "tc-mua", kb: "tc-quan-tri", duong: "/mua", toi: true, truocKhiMo: qt },
  { ten: "tc-mua-credit", kb: "tc-quan-tri", duong: "/mua", truocKhiMo: qt, lam: async (p) => {
    await p.getByRole("tab", { name: "Credit AI" }).click();
  } },
  { ten: "tc-mua-can-bo", kb: "tc-can-bo", duong: "/mua", truocKhiMo: cb },
  { ten: "tc-cai-dat-co-quan", kb: "tc-can-bo", duong: "/cai-dat", truocKhiMo: cb },
  { ten: "tc-trang-chu", kb: "tc-van-thu", duong: "/", toi: true, truocKhiMo: vt },
];
```

`cong-cu/chup.mjs`: `import { MAN_TO_CHUC } from "./man-to-chuc.mjs";` và trong mảng `MAN` thêm `...MAN_TO_CHUC,` sau
`...MAN_VAN_BAN,`. (`TOI` đã gom `m.toi`; `truocKhiMo`, `lam`, `giuKhung` dùng sẵn.)

- [ ] **Step 4: Kiểm nhanh API giả rồi chụp**

```bash
node --input-type=module -e "
import assert from 'node:assert/strict';
import { taoKhoGia, traLoiToChuc, TC_ID, NGUOI } from './cong-cu/api-gia-to-chuc.mjs';
const kho = taoKhoGia();
const u = (d) => new URL('https://api.thu/api/v2' + d);
assert.equal(traLoiToChuc('tc-can-bo', 'GET', u('/kho/van-ban/vb_1'), undefined, TC_ID, kho).status, 404);
assert.equal(traLoiToChuc('tc-can-bo', 'GET', u('/kho/van-ban/vb_3'), undefined, TC_ID, kho).body.data.sua_duoc, false);
assert.equal(traLoiToChuc('tc-van-thu', 'GET', u('/kho/van-ban/vb_3'), undefined, TC_ID, kho).body.data.sua_duoc, true);
assert.equal(traLoiToChuc('tc-can-bo', 'GET', u('/kho/giao-viec/cua-toi'), undefined, TC_ID, kho).body.data.tong, 3);
const tao = traLoiToChuc('tc-moi', 'POST', u('/to-chuc'), { ten: 'X' }, null, taoKhoGia({ coSan: false }));
assert.equal(tao.body.data.id, TC_ID);
console.log('api-gia-to-chuc: ổn');
"
npm run build
npm run chup -- tc-
```

Expected: `api-gia-to-chuc: ổn`; build xanh; `.tam/chup/tc-*-1440.png`, `tc-*-390.png` và bản `-toi` cho màn có `toi: true`;
`.tam/chup/csp.txt` rỗng.

- [ ] **Step 5: Tự xem từng ảnh (bắt buộc trước khi giao)**

Mở lần lượt mọi tệp `.tam/chup/tc-*.png` (công cụ Read đọc được ảnh). Soát: chữ có dấu đúng, **không đại từ nhân xưng**;
không tràn ngang ở 390; menu nơi làm việc không bị cắt; khối «Giao xử lý» đủ người, vai, hạn, nhật ký; cán bộ không thấy nút
Sửa / Xóa / form giao; dải «Gói của tổ chức chưa kích hoạt…» ở `tc-het-goi`; chế độ tối đọc được (chữ không chìm nền). Sửa
CSS / bố cục nếu cần rồi chụp lại màn đó (`npm run chup -- <tên>`).

Soát chữ trong mã mới:

```bash
git diff origin/main --name-only -- src cong-cu | xargs grep -nE "\b([Tt]ôi|[Aa]nh|[Ee]m|[Bb]ạn|[Cc]hị|[Mm]ình|[Dd]ạ|[Vv]âng)\b|ạ[.,!? ]"
```

Expected: không dòng nào (bỏ qua «tiếng Anh», tên riêng có «Anh» như «Nguyễn Thị Minh Anh»).

- [ ] **Step 6: Commit**

```bash
git add cong-cu/api-gia-to-chuc.mjs cong-cu/man-to-chuc.mjs cong-cu/api-gia.mjs cong-cu/chup.mjs
git commit -m "chup(to-chuc): API gia dot B co trang thai + 23 man to chuc o 1440/390 sang toi

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

(Không commit ảnh: `.tam/` không vào git.)

---

### Task 13: Quay video một vòng thao tác, kiểm cuối, đẩy nhánh, gửi duyệt

**Files:**
- Create: `cong-cu/quay-to-chuc.mjs`
- Modify: `package.json` (script `"quay-to-chuc": "node cong-cu/quay-to-chuc.mjs"`)

**Interfaces:**
- Consumes: Task 12 (`ganApiGia(page, kb, pdf, kho)`, `taoKhoGia({ coSan: false })`, `NGUOI`, `TC_ID`), `taoPdfMau`.
- Produces: `.tam/video-to-chuc/1-quan-tri.webm`, `2-van-thu.webm`, `3-can-bo.webm` (+ `vong-to-chuc.mp4` nếu có `ffmpeg`).

- [ ] **Step 1: Viết `cong-cu/quay-to-chuc.mjs`**

```js
// Quay video một vòng thao tác đợt B (1440×900, Playwright recordVideo, API giả có trạng thái dùng chung):
// quản trị tạo tổ chức + mời → văn thư nhận lời mời, vào sổ văn bản, giao việc → cán bộ nhận lời mời, xem «Việc được giao», cập nhật.
// Chạy: npm run build; npm run quay-to-chuc → .tam/video-to-chuc/
import { chromium } from "playwright";
import { mkdirSync, readdirSync, renameSync, rmSync, writeFileSync } from "node:fs";
import { spawn, spawnSync } from "node:child_process";
import { ganApiGia } from "./api-gia.mjs";
import { NGUOI, TC_ID, taoKhoGia } from "./api-gia-to-chuc.mjs";
import { taoPdfMau } from "./api-gia-van-ban.mjs";

const GOC = "http://localhost:4173";
const RA = ".tam/video-to-chuc";

async function choMayChu(ms = 20_000) {
  const den = Date.now() + ms;
  while (Date.now() < den) {
    try { if ((await fetch(GOC)).ok) return; } catch { /* chưa lên */ }
    await new Promise((r) => setTimeout(r, 250));
  }
  throw new Error(`vite preview không lên ở ${GOC} sau ${ms / 1000} giây (cổng 4173 bận?)`);
}
function dungMay(may) {
  if (may.exitCode !== null) return;
  if (process.platform === "win32") spawnSync("taskkill", ["/PID", String(may.pid), "/T", "/F"], { stdio: "ignore" });
  else may.kill();
}
const nhip = (p, ms = 900) => p.waitForTimeout(ms); // đủ chậm để người xem theo kịp
const mo = (p, duong) => p.goto(GOC + duong, { waitUntil: "networkidle" });

async function doan(b, ten, kb, kho, pdf, lam) {
  const thuMuc = `${RA}/${ten}`;
  const ctx = await b.newContext({ viewport: { width: 1440, height: 900 }, locale: "vi-VN", timezoneId: "Asia/Ho_Chi_Minh",
    recordVideo: { dir: thuMuc, size: { width: 1440, height: 900 } } });
  const p = await ctx.newPage();
  await ganApiGia(p, kb, pdf, kho);
  try { await lam(p); await nhip(p, 1500); }
  finally { await ctx.close(); } // video chỉ ghi xong khi đóng ngữ cảnh
  const tep = readdirSync(thuMuc).find((f) => f.endsWith(".webm"));
  renameSync(`${thuMuc}/${tep}`, `${RA}/${ten}.webm`);
  rmSync(thuMuc, { recursive: true, force: true });
}

async function nhanLoiMoi(p, kho, nguoi) {
  const l = kho.loiMoi.find((x) => x.email === nguoi.email && x.trang_thai === "cho");
  if (!l) throw new Error(`Không có lời mời chờ cho ${nguoi.email}`);
  await mo(p, `/loi-moi?ma=${l.ma}`);
  await nhip(p);
  await p.getByRole("button", { name: "Nhận lời mời" }).click();
  await p.getByRole("button", { name: "Bắt đầu làm việc ở tổ chức" }).click();
  await p.waitForURL(`${GOC}/`);
  await nhip(p);
}

rmSync(RA, { recursive: true, force: true });
mkdirSync(RA, { recursive: true });
const may = spawn("npx vite preview --port 4173 --strictPort", { shell: true, stdio: "ignore" });
let b;
try {
  await choMayChu();
  b = await chromium.launch({ channel: "chromium", args: ["--lang=vi-VN"] });
  const pdf = await taoPdfMau(b);
  const kho = taoKhoGia({ coSan: false });

  await doan(b, "1-quan-tri", "tc-moi", kho, pdf, async (p) => {
    await mo(p, "/to-chuc");
    await nhip(p);
    await p.getByLabel("Tên tổ chức").pressSequentially("UBND phường Bình Thạnh", { delay: 40 });
    await p.getByRole("button", { name: "Tạo tổ chức" }).click();
    await p.getByRole("heading", { name: "UBND phường Bình Thạnh" }).waitFor();
    await nhip(p);
    await p.getByRole("button", { name: "Dùng tổ chức này" }).click();
    await p.waitForURL(`${GOC}/`);
    await nhip(p);
    await mo(p, `/to-chuc/${TC_ID}/thanh-vien`);
    for (const [n, vai] of [[NGUOI.vt, "van_thu"], [NGUOI.cb, "can_bo"]]) {
      await p.getByLabel("Email người được mời").pressSequentially(n.email, { delay: 25 });
      await p.getByLabel("Vai trò khi tham gia").selectOption(vai);
      await p.getByRole("button", { name: "Gửi lời mời" }).click();
      await p.getByText(`Đã gửi lời mời tới ${n.email}.`).waitFor();
      await nhip(p);
    }
  });

  await doan(b, "2-van-thu", "tc-van-thu", kho, pdf, async (p) => {
    await nhanLoiMoi(p, kho, NGUOI.vt);
    await mo(p, "/van-ban/moi?huong=den");
    await p.getByLabel("Trích yếu", { exact: true }).pressSequentially("Về việc tăng cường phòng cháy, chữa cháy mùa khô 2026 – 2027", { delay: 20 });
    await p.getByLabel("Số", { exact: true }).fill("1532");
    await nhip(p);
    await p.getByRole("button", { name: "Lưu", exact: true }).click();
    await p.waitForURL(/\/van-ban\/vb_moi_/);
    await p.getByRole("region", { name: "Giao xử lý" }).waitFor();
    await nhip(p);
    await p.getByLabel("Người nhận").selectOption({ label: NGUOI.cb.ho_ten });
    await p.getByLabel("Ý kiến chỉ đạo").pressSequentially("Tham mưu kế hoạch, trình trước ngày 25/11", { delay: 20 });
    await p.getByRole("button", { name: "Giao", exact: true }).click();
    await p.getByRole("region", { name: "Giao xử lý" }).getByText(NGUOI.cb.ho_ten).first().waitFor();
    await nhip(p, 1500);
  });

  await doan(b, "3-can-bo", "tc-can-bo", kho, pdf, async (p) => {
    await nhanLoiMoi(p, kho, NGUOI.cb);
    await mo(p, "/viec");
    await nhip(p, 1500);
    const tt = p.getByLabel(/^Trạng thái: Về việc tăng cường phòng cháy/);
    await tt.selectOption("dang");
    await nhip(p, 1200);
    await p.getByRole("link", { name: /Về việc tăng cường phòng cháy/ }).click();
    await p.getByRole("region", { name: "Giao xử lý" }).waitFor();
    await p.getByText(/Nhật ký/).first().click();
    await nhip(p, 2000);
  });

  const coFfmpeg = spawnSync("ffmpeg", ["-version"], { stdio: "ignore" }).status === 0;
  if (coFfmpeg) {
    writeFileSync(`${RA}/ds.txt`, ["1-quan-tri", "2-van-thu", "3-can-bo"].map((t) => `file '${t}.webm'`).join("\n"));
    const r = spawnSync("ffmpeg", ["-y", "-f", "concat", "-safe", "0", "-i", "ds.txt", "-c:v", "libx264", "-pix_fmt", "yuv420p",
      "-r", "30", "vong-to-chuc.mp4"], { cwd: RA, stdio: "inherit" });
    if (r.status !== 0) console.warn("ffmpeg ghép lỗi — giao ba tệp .webm riêng");
  } else {
    console.warn("Máy không có ffmpeg — giao ba tệp .webm riêng (xem được bằng trình duyệt)");
  }
} finally {
  await b?.close();
  dungMay(may);
}
console.log(`Đã quay vào ${RA}`);
```

Bộ chọn bám theo nhãn / vai trò thật của các màn ở Task 3–11 (`Tên tổ chức`, `Email người được mời`, `Vai trò khi tham gia`,
`Người nhận`, `Ý kiến chỉ đạo`, nhãn `Trích yếu` / `Số` lấy từ `NHAN_TRUONG` của `src/phan-he/van-ban/nhan-truong.ts`). Chạy
thử hỏng ở bước nào thì sửa **bộ chọn trong script** cho khớp màn thật — không đổi giao diện để khớp script.

`package.json` — trong `scripts` thêm `"quay-to-chuc": "node cong-cu/quay-to-chuc.mjs"`.

- [ ] **Step 2: Kiểm cuối toàn nhánh**

```powershell
npm test; if ($?) { npm run build }; if ($?) { npm run chup }; if ($?) { npm run quay-to-chuc }
```

Expected: Vitest xanh (bài cũ không sửa ngoài `ung-dung.test.ts`); build xanh; `npm run chup` chụp lại **toàn bộ** màn (kể cả
màn cũ — đổi `Vo` / `App` không được làm hỏng màn nào), `csp.txt` rỗng; `.tam/video-to-chuc/` có ba `.webm` (và `.mp4` nếu
có ffmpeg). Xem lại vài màn cũ quan trọng (`trang-chu`, `van-ban-chi-tiet`, `mua`) ở 1440 + 390 để chắc không lệch.

Xem video: mở từng `.webm` (trình duyệt) — đủ ba đoạn, thao tác đọc được, không có màn lỗi / 404 / vòng xoay treo.

- [ ] **Step 3: Commit + đẩy nhánh (KHÔNG đẩy `main`)**

```bash
git add cong-cu/quay-to-chuc.mjs package.json
git commit -m "chup(to-chuc): quay video mot vong thao tac quan tri - van thu - can bo bang Playwright recordVideo

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
git status --short
git log --oneline origin/main..HEAD
git push -u origin dot-b-to-chuc
```

Expected: nhánh `dot-b-to-chuc` trên origin; `main` không đổi.

- [ ] **Step 4: Giao chủ dự án duyệt**

Gửi ảnh `.tam/chup/tc-*.png` (1440 + 390, sáng + tối) và video `.tam/video-to-chuc/` qua Telegram theo cách dự án đang dùng
(đọc `D:/Thang/gikidi-server/CLAUDE.md` trước; không in token). Không có công cụ gửi tệp trong phiên thì báo controller **đường
dẫn tuyệt đối** của thư mục ảnh và video, kèm danh sách màn. Lời nhắn gửi kèm: câu vô nhân xưng, ngắn, trong khối code.
Không dựng môi trường thử trên máy chủ, không trỏ web thật vào API nhánh.

---

## Tự soát kế hoạch (đã làm khi viết)

- **Phủ spec §5:** header một chỗ + `localStorage` theo `uid` + xóa bộ đệm khi đổi → T1, T3, T5; nút «Cá nhân / Tổ chức X» →
  T4; phân hệ Tổ chức → T6, T7; «Việc của tôi» (đổi tên «Việc được giao») → T9; trang `/loi-moi` → T8; chi tiết văn bản (khối
  giao, công khai, cán bộ chỉ đọc + đổi trạng thái việc của chính người đó) → T10; Quét so cơ quan theo hồ sơ tổ chức → T5 (`useCoQuan`
  theo ngữ cảnh, `BuocSoat` nhận `tenCoQuan` từ đó); Mua cho tổ chức → T11; ẩn trên prod → Global Constraints. **§6:** vitest
  ngữ cảnh / quyền hiển thị / Việc được giao → T1–T11; chụp + video → T12, T13.
- **Không chỗ trống:** mọi bước mã có mã đầy đủ; chỗ «giữ nguyên» chỉ trỏ đích danh phần mã có sẵn.
- **Tên nhất quán:** `datToChuc` / `toChucHienTai` / `khiDoiToChuc` (T1) ↔ `useToChucId` (T3); `toChuc.*`, `giaoViec.*` (T2) dùng
  đúng tên ở T3–T11; kiểu khớp hợp đồng API (`ToChucTomTat`, `GiaoViecVanBan`, `ViecDuocGiao`); API giả (T12) trả đúng các kiểu đó.

