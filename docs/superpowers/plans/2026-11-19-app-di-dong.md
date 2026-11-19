# App iOS / Android VanBanPlus — kế hoạch thực hiện

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or
> superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bọc SPA `vanbanplus-web` thành app Android (APK debug chạy thử) + giữ sẵn cấu hình iOS bằng Capacitor: giữ phiên an
toàn, Google native, quét tài liệu bằng camera, mở / lưu tệp kiểu di động, nút Back, ẩn hẳn mọi lối vào Mua — web giữ nguyên.

**Architecture:** API `vanbanplus-api` chỉ thêm: yêu cầu có `X-VBP-Nguon: app` **và** Origin là nguồn WebView của app
(`capacitor://localhost`, `https://localhost`) thì các route phiên trả `token_lam_moi` trong thân, không đặt cookie; Google nhận
nhiều `aud`. Web `vanbanplus-web` thêm một mặt tiền `src/nen-tang.ts` (mọi khác biệt web ↔ app đi qua đây) và lớp máy
`src/app-di-dong/` (bọc plugin Capacitor, chỉ nạp khi build `--mode app`; Vite loại hẳn khỏi gói web). Thử trên thiết bị dùng
một app Coolify tạm chạy nhánh `dot-b-app` của API (cùng CSDL / biến môi trường prod), không đẩy `main`.

**Tech Stack:** Next.js 16 + `tsx --test` (node:test); React 18 + React Router 7 + Vite 6 + Vitest 3 + Testing Library +
Playwright 1.65 (`_android` để lái WebView trên máy ảo); Capacitor 8.6.2 + plugin (bảng «Gói đã ghim»); Android SDK 36,
JDK 21, Gradle 8.14.3 (wrapper của Capacitor); Coolify (app tạm) qua `D:/Thang/gikidi-server`.

**Spec:** `docs/superpowers/specs/2026-11-19-app-di-dong-design.md` · hợp đồng `docs/superpowers/plans/2026-11-17-hop-dong-web-dot-a.md`
(đọc §3, §4, §6, dòng §9 «App di động (sau) cần `token_lam_moi` trong thân trả về — bổ sung khi bắt đầu app»).

## Global Constraints

- **Nhánh `dot-b-app`, worktree riêng mỗi repo:** API `D:/AIVanBanCaNhan/_wt-b-app-api`, web `D:/AIVanBanCaNhan/_wt-b-app-web`
  (tạo ở Task 1 / Task 4 từ `origin/main`).
- **TUYỆT ĐỐI không đẩy `main`** (cả hai repo: `main` = production, Coolify tự build). Lệnh đẩy duy nhất được dùng:
  `git push origin dot-b-app`. Không `git push origin dot-b-app:main`, không merge vào `main`, không mở PR tự gộp.
- **API chỉ thêm:** yêu cầu không có `X-VBP-Nguon: app` (hoặc có mà Origin không phải nguồn app) → hành vi y như cũ. Bộ kiểm
  hiện có trong `vanbanplus-api/tests/` **không sửa dòng nào**, phải xanh (`npm test`; bài kiểm CSDL tự bỏ qua khi không có
  `TEST_DATABASE_URL`). Bài kiểm mới để ở tệp mới.
- **Web build thường (không `--mode app`) giống hệt hành vi hiện nay;** `npm test` (vitest) hiện có xanh. Gói web không chứa mã
  Capacitor (kiểm bằng `rg` trên `dist/` ở Task 4). Bài kiểm cũ không sửa; bài kiểm mới ở tệp mới.
- **Ghim phiên bản chính xác** (`npm install --save-exact`, không `^`): bảng «Gói đã ghim» bên dưới. Đổi bản = sửa bảng này trước.
- **Chữ giao diện tiếng Việt có dấu đầy đủ, KHÔNG đại từ nhân xưng** (tôi, chúng tôi, ta, chúng ta, anh, chị, em, bạn, mình),
  không «dạ / vâng / ạ» — kể cả chữ trong `Info.plist`, hộp thoại chia sẻ. Soát bằng
  `grep -nE "\b([Tt]ôi|[Tt]a|[Aa]nh|[Ee]m|[Bb]ạn|[Cc]hị|[Mm]ình|[Dd]ạ|[Vv]âng)\b|ạ[.,!? ]" <tệp>` (bỏ qua «tiếng Anh»).
- Commit theo lệ repo: tiếng Việt không dấu, thêm dòng cuối bằng `-m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"`.
- Web: mọi lời gọi API qua `src/api/khach.ts`; **không sửa `src/os-kit/`** (liên kết ngoài của os-kit được chặn từ phía web,
  Task 7 — xem «Quyết định»). Mọi khác biệt theo nền tảng đi qua `src/nen-tang.ts`; chỉ `src/app-di-dong/` được import
  `@capacitor/*` và plugin.
- **Kịch bản tấn công là bài kiểm** (bảng «Kịch bản tấn công»): token làm mới trong thân chỉ cho nguồn app; `aud` Google lạ bị
  từ chối; bản app không còn đường nào tới `/mua`.
- Không in, không commit, không dán khóa / token / mật khẩu / URL ký Storage. Biến Coolify chép sang tệp chỉ ở
  `C:/Users/PC/.gikidi/` (xóa khi xong). Client ID Google là thông tin công khai (được commit trong `.env.app`).
- Android build trên PC Windows (JDK 21 + Android SDK, không cần mở Android Studio). iOS: chỉ tạo và giữ `ios/`, **không build,
  không ký** (chưa có Mac / tài khoản Apple Developer).
- Máy ảo: dùng AVD `Medium_Phone_API_36.0` (ảnh có Play Store). **Không đụng AVD `Bida51`** (của project bida).
- Không phát hành, không đưa lên kho ứng dụng, không đổi gì ở bản web đang chạy.

## Quyết định đã chốt (gồm chỗ làm khác / hoãn so với spec)

| Mục | Quyết định |
| --- | --- |
| Token trong thân (spec §3) | **Chặt hơn spec:** cần cả `X-VBP-Nguon: app` **và** `Origin ∈ VBP_NGUON_APP` (mặc định `capacitor://localhost,https://localhost`). Origin do trình duyệt đặt, script không giả được → XSS trên `app.vanbanplus…` không biến được cookie HttpOnly thành token đọc được. Ở chế độ app, `lam-moi` / `dang-xuat` **chỉ đọc token trong thân, bỏ qua cookie** |
| Route trả token | `dang-nhap`, `google`, `2-lop`, `otp/xac-nhan` (qua `traLoi`), `lam-moi` (qua `traLoiLamMoi`). `dang-xuat`: app đọc token trong thân. Các route `toi/*` không cấp phiên nên không đổi |
| Google `aud` (spec §4) | `VBP_GOOGLE_CLIENT_ID` = danh sách dấu phẩy. Android (Credential Manager, `webClientId` = client Web) → `aud` là client **Web**; iOS (GoogleSignIn) → `aud` là client **iOS**. Danh sách đặt web + iOS + Android (Android không bao giờ là `aud`, để theo spec, vô hại). `aud` phải là chuỗi khác rỗng nằm trong danh sách |
| Thử trên thiết bị | **Không đẩy `main` API** → app Coolify tạm `vanbanplus-api-thu` (nhánh `dot-b-app`, chép biến môi trường của app API prod, tên `https://vanbanplus-api-thu.app.giakiemso.com`, DNS đại diện `*.app.giakiemso.com` có sẵn). APK thử build với `.env.app.local` trỏ tên này. Gộp API vào `main` để sau khi chủ dự án duyệt (ngoài kế hoạch) |
| Web ↔ app | `VITE_NEN_TANG=app` nằm trong `.env.app`, build bằng `vite build --mode app`. `vite.config.ts` biến `import.meta.env.VITE_NEN_TANG` thành hằng **lúc build** → nhánh app và `import("@/app-di-dong/…")` bị loại khỏi gói web. Bài kiểm bật chế độ app bằng `datNenTangThu()` (không qua biến môi trường) |
| Liên kết ngoài (spec §5) | **Không sửa os-kit, không sửa từng tệp:** bản app gắn một bộ chặn `click` (pha capture) trên `document` — `<a href>` http(s) khác nguồn → `Browser.open` (trình duyệt trong app, không có cầu nối Capacitor). Phủ cả `os-kit/dieu-huong.tsx` (`<a>` của `Link`), `DangKy`, `KhungXacThuc`, `ChiTietCuocHop`. Riêng `useDieuHuong().push(url ngoài)` của os-kit gọi `location.assign` → Capacitor tự mở trình duyệt hệ thống (chấp nhận, ghi rủi ro) |
| PDF / tệp (spec §5) | App: «Xem» PDF và «Tải về» mọi tệp → `@capacitor/file-transfer` tải về `Cache/mo/` (native, không qua CORS) → `@capacitor-community/file-opener` mở bằng ứng dụng của máy. Ảnh vẫn xem trong trang (thẻ `img` chạy tốt trong WebView). `Cache/mo/` + `Cache/xuat/` xóa ở mỗi lần mở app |
| Xuất Word, mã dự phòng | `luuTep()`: web = blob + `a[download]` như cũ (mã dự phòng đổi hạn thu hồi URL 0 → 1000 ms, không thấy khác); app = `Filesystem.writeFile` vào `Cache/xuat/` → `Share.share` |
| Cookie `osk_gd` (spec §5) | Android (`https://localhost`) dùng cookie bình thường → **kiểm ở Task 14** (đổi hình nền, tắt app, mở lại). **iOS hoãn** (scheme `capacitor://` có thể không giữ `document.cookie`; xử lý khi có Mac: bật `CapacitorCookies` hoặc gương sang Preferences) |
| Quyền camera | iOS: `NSCameraUsageDescription`. **Android không khai `CAMERA`:** ML Kit Document Scanner chạy trong Google Play services, app không cần quyền (ít bề mặt duyệt Play hơn) — làm khác spec §6 |
| Nút Back | Đang có hộp thoại (`dialog[open]`, `[role=dialog]`, `[role=alertdialog]`) → gửi phím Esc; ở `/` hoặc `/dang-nhap` → thoát app; còn lịch sử → lùi; hết lịch sử → về `/` |
| Ẩn Mua (spec §7) | `coTheMua() = !laApp()`; phân hệ «Mua thêm» lọc khỏi danh sách; tuyến `/mua/*` → `Navigate("/")`; mọi liên kết Mua bọc `coTheMua()`; câu báo trung tính; `HET_DUNG_LUONG` của API (chữ có «mua thêm») đổi chữ ở `khach.ts` khi là app; nhãn lô credit `nap` «Mua thêm» → «Nạp thêm». Bài kiểm nguồn chặn ai thêm lối Mua mới mà quên `coTheMua()` |
| Hoãn | Thông báo đẩy, ngoại tuyến, CSP dạng thẻ meta cho bản app, build / ký iOS, xác minh Google iOS + cookie iOS, khóa ký phát hành Android + SHA-1 phát hành, phát hành kho |

## Gói đã ghim (kiểm `npm view <gói> version` ngày 19/11/2026)

| Gói | Bản | Ngày phát hành | Dùng cho | Lý do chọn | Dự phòng |
| --- | --- | --- | --- | --- | --- |
| `@capacitor/core`, `@capacitor/cli`, `@capacitor/android`, `@capacitor/ios` | **8.6.2** | 12/11/2026 | Lõi | Bản ổn định mới nhất (8.6.0 28/10 + vá); Android SDK 36, minSdk 24, JDK 21, Gradle 8.14.3; iOS 15+, mẫu SPM (`cap add ios` chạy được trên Windows — đã thử 19/11) | 8.6.0 |
| `@capacitor/app` | **8.1.2** | 18/11/2026 | Nút Back, thoát app | Plugin chính thức | — |
| `@capacitor/filesystem` | **8.1.5** | 09/11/2026 | Ghi tệp xuất, dọn bộ nhớ tạm | Chính thức | — |
| `@capacitor/file-transfer` | **2.0.6** | 02/10/2026 | Tải URL ký về máy | Chính thức; `Filesystem.downloadFile` đã bị đánh dấu bỏ; tải native nên không phụ thuộc CORS | `Filesystem.downloadFile` (8.1.5 vẫn còn) |
| `@capacitor/share` | **8.0.3** | 02/10/2026 | Bảng Chia sẻ (`files: [uri]`) | Chính thức | — |
| `@capacitor/browser` | **8.0.5** | 18/11/2026 | Liên kết ngoài | Chính thức (Custom Tabs / SFSafariViewController) | Để Capacitor mở trình duyệt hệ thống |
| `@capacitor/preferences` | **8.0.1** | 18/11/2026 | Cờ «đã cài» (dọn Keychain cũ) | Chính thức | — |
| `@aparajita/capacitor-secure-storage` | **8.0.1** | 23/09/2026 | Token làm mới | Keychain (iOS) / AES-GCM khóa Keystore (Android); tắt đồng bộ iCloud được; peer Capacitor 8 | `capacitor-secure-storage-plugin` 0.14.2 |
| `@capgo/capacitor-social-login` | **8.7.11** | 13/11/2026 | Google native | Bản kế tục `@codetrix-studio/capacitor-google-auth` (đã bỏ, 3.4.0-rc.4 năm 2024); Android dùng Credential Manager, iOS GoogleSignIn; tắt được Facebook / Apple / Twitter | Ẩn nút Google trong app (đăng nhập mật khẩu / mã email vẫn chạy) |
| `@capgo/capacitor-document-scanner` | **8.4.20** | 13/11/2026 | Quét tài liệu | Một plugin cho cả ML Kit Document Scanner (Android) và VisionKit `VNDocumentCameraViewController` (iOS, API công khai), nhiều trang, trả JPEG đã cắt góc | `@capacitor-mlkit/document-scanner` 8.2.2 (chỉ Android) + iOS dùng `<input capture>` cũ |
| `@capacitor-community/file-opener` | **8.0.1** | 29/05/2026 | Mở PDF / ảnh bằng ứng dụng của máy | Ổn định, peer Capacitor 8, có FileProvider sẵn | `@capawesome-team/capacitor-file-opener` 8.1.1 |
| `@capacitor/assets` (chỉ chạy `npx`, không vào `package.json`) | **4.0.0** | 30/10/2026 | Sinh biểu tượng + màn chờ | Chính thức | Giữ biểu tượng mặc định cho bản thử |

Lưu ý đã đọc README: **ML Kit Document Scanner không chạy trên máy ảo Android** → quét thử trên điện thoại thật (Task 14).
Plugin Google trên `BridgeActivity` gốc: **không** truyền `scopes`; đặt `skipAuthOnlyAuthorizationAfterSignIn: true` (chỉ cần
`idToken`).

## Kịch bản tấn công

| Bề mặt | Cách lạm dụng | Cách chặn (task, bài kiểm) |
| --- | --- | --- |
| Token làm mới trong thân | XSS trên `app.vanbanplus.giakiemso.com` gọi `lam-moi` với `credentials: "include"` + tự thêm `X-VBP-Nguon: app` để đổi cookie HttpOnly thành token đọc được | `laNguonApp` cần Origin ∈ nguồn app — Origin web → xử lý như web (cookie, không token trong thân) (T1 `phien-app.test.ts`; T3 bước 2) |
| | Trang ở `https://localhost` trên máy tính người dùng (nguồn trùng app Android) gửi cookie + header app | Chế độ app **bỏ qua cookie**, chỉ đọc token trong thân; cookie `SameSite=Lax` cũng không đi khác site (T1; T3 bước 6) |
| | `curl` gửi header app không Origin | Không Origin → như web (T1; T3 bước 3) |
| | Lộ token qua log | API không log thân; script thử chỉ in ✔/✘ |
| Token trên máy | Đọc token từ sao lưu Android / iCloud | Keystore AES-GCM (khóa không rời máy) + `android:allowBackup="false"` (T11); `setSynchronize(false)` — không đồng bộ iCloud Keychain (T4) |
| | Gỡ app iOS rồi cài lại: Keychain còn token cũ → tự vào phiên người trước | Lần chạy đầu (thiếu cờ `vbp_da_cai` trong Preferences) xóa token (T4) |
| | Máy mất | Token xoay mỗi lần làm mới, dùng lại token cũ quá 30 giây → thu hồi cả phiên (có sẵn); «Thiết bị» trên web thu hồi được (có sẵn) |
| Google | `id_token` hợp lệ của **app khác** (aud lạ), `aud` dạng mảng, `aud` rỗng | `aud` phải là chuỗi khác rỗng thuộc danh sách (T2 `phien-google-nhieu-aud.test.ts`; T3 bước 8) |
| Mua | Bản app còn lối tới `/mua` (Apple / Google đòi In-App Purchase) | Lọc phân hệ, `Navigate` cho `/mua/*`, bọc `coTheMua()` mọi liên kết, đổi câu báo của API; bài kiểm nguồn quét `src/` + bài kiểm hiển thị (T6); kiểm trên máy ảo: không có chữ «Mua», `/mua?loai=credit` về `/` (T14) |
| WebView | Liên kết ngoài mở **trong** WebView (trang lạ chạm cầu nối Capacitor) | Bộ chặn click → `Browser.open` (tách khỏi WebView) (T7); Capacitor mặc định không điều hướng WebView ra nguồn khác |
| | `CapacitorHttp` vượt CORS / bỏ Origin | Tắt `CapacitorHttp`, `CapacitorCookies` (T11) — `fetch` vẫn trong WebView, Origin vẫn gửi |
| Tệp | Tên tệp từ máy chủ chứa `../` → ghi ra ngoài thư mục tạm | `tenTepAnToan` bỏ `/ \ : * ? " < > |`, ký tự điều khiển, dấu chấm đầu (T4 `tep.test.ts`) |
| | Văn bản tải về nằm lại trong bộ nhớ máy | Ghi vào `Cache/`, xóa `Cache/mo`, `Cache/xuat` mỗi lần mở app (T4) |
| | URL ký lộ ra log / trình duyệt khác | Tải bằng FileTransfer (native), không mở URL ký bằng Browser (T7) |
| APK thử | APK debug lan ra ngoài | Chỉ gửi Telegram chủ dự án; không đăng ở đâu |

## Cấu trúc tệp

**vanbanplus-api** (`_wt-b-app-api`)

| Tệp | Việc |
| --- | --- |
| `src/lib/phien/nguon-app.ts` (mới) | `NGUON_APP_MAC_DINH`, `laNguonApp(req)` |
| `src/lib/phien/tra-loi.ts` | `traLoi(kq, tc?)`, `traLoiLamMoi(kq, tc?)`, `tokenLamMoi(req, than, chiThan?)` |
| `src/app/api/v2/phien/{dang-nhap,google,2-lop,otp/xac-nhan,lam-moi,dang-xuat}/route.ts` | Truyền `laNguonApp(req)` |
| `src/lib/phien/google.ts`, `src/lib/phien/phu-thuoc.ts` | Nhiều `aud`; `docDsClientGoogle` |
| `tests/phien-app.test.ts`, `tests/phien-google-nhieu-aud.test.ts` (mới) | Kiểm thuần + dây nối route |
| `scripts/thu-phien-app.mjs` (mới) | Kiểm thật trên API thử |

**vanbanplus-web** (`_wt-b-app-web`)

| Tệp | Việc |
| --- | --- |
| `src/nen-tang.ts` (mới) | Mặt tiền nền tảng: `laApp`, `coTheMua`, `dauNenTang`, `layMay`, `datNenTangThu`, `luuTep`, `moTep`, `moNgoai`, `chuLoiTrungTinh`, kiểu `MayApp` |
| `src/app-di-dong/may.ts` (mới) | `may: MayApp` + `khoiTaoMay()` — chỉ tệp này import plugin |
| `src/app-di-dong/tep.ts` (mới) | `tenTepAnToan`, `blobSangBase64` |
| `src/app-di-dong/khoi-dong.ts` (mới) | `khoiDong()` chạy trước khi vẽ (bản app) |
| `src/app-di-dong/lien-ket-ngoai.ts` (mới) | `chanLienKetNgoai(goc, mo)` |
| `src/app-di-dong/NutBack.tsx` (mới) | Nút Back Android |
| `src/test/may-gia.ts` (mới) | `taoMayGia()` cho bài kiểm |
| `vite.config.ts`, `src/vite-env.d.ts`, `src/main.tsx`, `.env.app` (mới), `.env.example`, `package.json` | Build `--mode app`, khởi động app |
| `src/api/khach.ts`, `src/api/phien.ts` | Phiên app (token trong kho máy) |
| `src/ung-dung.ts`, `src/App.tsx` + 11 tệp lối Mua | Ẩn Mua |
| `src/phan-he/soan-thao/xuat-word/dung-docx.ts`, `src/phan-he/tai-khoan/BatHaiLop.tsx`, `src/phan-he/lich-hop/KhoiTep.tsx`, `src/phan-he/album/ChiTietAlbum.tsx`, `src/phan-he/van-ban/DanhSachTep.tsx` | Lưu / mở tệp qua `nen-tang` |
| `src/dang-nhap/NutGoogle.tsx`, `src/giao-dien/goc.css` | Nút Google native |
| `src/phan-he/quet/ChonTep.tsx` | «Chụp ảnh» mở trình quét |
| `capacitor.config.ts`, `android/`, `ios/`, `assets/logo.png`, `cong-cu/bieu-tuong-app.mjs` (mới) | Dự án native |
| `cong-cu/thu-app-android.mjs` (mới) | Lái app trên máy ảo, chụp ảnh, quay video |
| `CLAUDE.md` | Ghi cách build app |

---

## Phần A — API (`D:/AIVanBanCaNhan/_wt-b-app-api`)

### Task 1: Token làm mới trong thân cho nguồn app

**Files:**
- Create: `src/lib/phien/nguon-app.ts`
- Modify: `src/lib/phien/tra-loi.ts:10-18` (`traLoi`), `:38-42` (`traLoiLamMoi`), `:83-85` (`tokenLamMoi`)
- Modify: `src/app/api/v2/phien/dang-nhap/route.ts`, `google/route.ts`, `2-lop/route.ts`, `otp/xac-nhan/route.ts`,
  `lam-moi/route.ts`, `dang-xuat/route.ts`
- Test: `tests/phien-app.test.ts`

**Interfaces:**
- Produces: `export const NGUON_APP_MAC_DINH: string`; `export function laNguonApp(req: NextRequest): boolean`;
  `export type TuyChonTraLoi = { app?: boolean }`; `traLoi(kq: KetQua, tc?: TuyChonTraLoi): NextResponse` (app + `loai: "phien"`
  → `data.token_lam_moi`, không `Set-Cookie`); `traLoiLamMoi(kq: KetQua, tc?: TuyChonTraLoi)`;
  `tokenLamMoi(req: NextRequest, than: Record<string, unknown>, chiThan?: boolean): string | null`.
- Hợp đồng cho web (Task 5): yêu cầu có `X-VBP-Nguon: app` từ WebView app → thân trả về
  `{ success, message, data: { access_token, het_han_giay, nguoi_dung, token_lam_moi } }`; `lam-moi` / `dang-xuat` nhận
  `{ token_lam_moi }` trong thân.

- [ ] **Step 0: Tạo worktree**

```bash
cd D:/AIVanBanCaNhan/vanbanplus-api && git fetch origin
git worktree add D:/AIVanBanCaNhan/_wt-b-app-api -b dot-b-app origin/main
cd D:/AIVanBanCaNhan/_wt-b-app-api && npm ci && npm test
```
Mong đợi: `npm test` 0 fail (bài kiểm CSDL bỏ qua). Ghi số bài pass để so ở Step 6.

- [ ] **Step 1: Viết bài kiểm hỏng** `tests/phien-app.test.ts`

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { NextRequest } from "next/server";
import { LOI, type KetQua } from "../src/lib/phien/dich-vu";
import { laNguonApp, NGUON_APP_MAC_DINH } from "../src/lib/phien/nguon-app";
import { tokenLamMoi, traLoi, traLoiLamMoi } from "../src/lib/phien/tra-loi";

const ND = { id: "u", email: "a@b.vn", ho_ten: "A", goi: "free", goi_het_han: null, co_mat_khau: true, hai_lop_bat: false, google_gan: false };
const PHIEN: KetQua = { loai: "phien", access_token: "jwt", het_han_giay: 900, nguoi_dung: ND, token_lam_moi: "bi-mat-app" };
const yc = (dau: Record<string, string>) => new NextRequest("https://x.vn/api/v2/phien/lam-moi", { method: "POST", headers: dau });

test("app: token làm mới trong thân, KHÔNG Set-Cookie, no-store", async () => {
  const r = traLoi(PHIEN, { app: true });
  assert.equal(r.status, 200);
  assert.equal(r.headers.get("cache-control"), "no-store");
  assert.equal(r.headers.get("set-cookie"), null);
  assert.deepEqual(await r.json(), {
    success: true, message: "Đã đăng nhập.",
    data: { access_token: "jwt", het_han_giay: 900, nguoi_dung: ND, token_lam_moi: "bi-mat-app" },
  });
});

test("web (không tùy chọn, hoặc app: false): như cũ — cookie, token không trong thân", async () => {
  for (const r of [traLoi(PHIEN), traLoi(PHIEN, { app: false })]) {
    assert.match(r.headers.get("set-cookie") ?? "", /^vbp_lam_moi=bi-mat-app; Path=\/api\/v2\/phien;/);
    assert.doesNotMatch(JSON.stringify(await r.json()), /bi-mat-app/);
  }
});

test("app: can_2_lop, lỗi, xong giống web (không token, không cookie)", async () => {
  const a = traLoi({ loai: "can_2_lop", ma_tam: "mt" }, { app: true });
  assert.equal(a.headers.get("set-cookie"), null);
  assert.equal((await a.json()).code, "CAN_2_LOP");
  const b = traLoi(LOI.saiDangNhap(), { app: true });
  assert.equal(b.status, 401);
  assert.equal(b.headers.get("set-cookie"), null);
});

test("traLoiLamMoi app: PHIEN_THU_HOI không đụng cookie; phiên mới có token trong thân", async () => {
  assert.equal(traLoiLamMoi(LOI.phienThuHoi(), { app: true }).headers.get("set-cookie"), null);
  const moi = traLoiLamMoi(PHIEN, { app: true });
  assert.equal(moi.headers.get("set-cookie"), null);
  assert.equal((await moi.json()).data.token_lam_moi, "bi-mat-app");
  // web giữ nguyên: PHIEN_THU_HOI xóa cookie
  assert.match(traLoiLamMoi(LOI.phienThuHoi()).headers.get("set-cookie") ?? "", /Max-Age=0/);
});

test("laNguonApp: cần cả X-VBP-Nguon: app VÀ Origin của WebView app", () => {
  assert.equal(NGUON_APP_MAC_DINH, "capacitor://localhost,https://localhost");
  assert.equal(laNguonApp(yc({ "x-vbp-nguon": "app", origin: "capacitor://localhost" })), true);
  assert.equal(laNguonApp(yc({ "x-vbp-nguon": " APP ", origin: "https://localhost" })), true);
  // tấn công: script trên trang web tự thêm header app — Origin vẫn do trình duyệt đặt là nguồn web
  assert.equal(laNguonApp(yc({ "x-vbp-nguon": "app", origin: "https://app.vanbanplus.giakiemso.com" })), false);
  assert.equal(laNguonApp(yc({ "x-vbp-nguon": "app", origin: "https://evil.example" })), false);
  assert.equal(laNguonApp(yc({ "x-vbp-nguon": "app", origin: "capacitor://localhost.evil.example" })), false);
  assert.equal(laNguonApp(yc({ "x-vbp-nguon": "app" })), false); // curl không Origin
  assert.equal(laNguonApp(yc({ origin: "capacitor://localhost" })), false);
  assert.equal(laNguonApp(yc({ "x-vbp-nguon": "web", origin: "capacitor://localhost" })), false);
});

test("laNguonApp: VBP_NGUON_APP đổi danh sách, đọc lúc gọi", () => {
  const goc = process.env.VBP_NGUON_APP;
  try {
    process.env.VBP_NGUON_APP = "capacitor://localhost";
    assert.equal(laNguonApp(yc({ "x-vbp-nguon": "app", origin: "https://localhost" })), false);
    assert.equal(laNguonApp(yc({ "x-vbp-nguon": "app", origin: "capacitor://localhost" })), true);
  } finally {
    if (goc === undefined) delete process.env.VBP_NGUON_APP; else process.env.VBP_NGUON_APP = goc;
  }
});

test("tấn công: cookie web trình qua đường app → bị bỏ qua, chỉ token trong thân được đọc", () => {
  const r = yc({ cookie: "vbp_lam_moi=tu-cookie", "x-vbp-nguon": "app", origin: "https://localhost" });
  assert.equal(tokenLamMoi(r, {}, laNguonApp(r)), null);
  assert.equal(tokenLamMoi(r, { token_lam_moi: "tu-than" }, laNguonApp(r)), "tu-than");
  assert.equal(tokenLamMoi(r, { token_lam_moi: 5 }, true), null);
  // web: cookie trước rồi thân (như cũ)
  assert.equal(tokenLamMoi(r, { token_lam_moi: "tu-than" }), "tu-cookie");
});

test("dây nối: mọi route cấp phiên truyền laNguonApp(req); dang-xuat đọc token theo nguồn", () => {
  for (const ten of ["dang-nhap", "google", "2-lop", "otp/xac-nhan", "lam-moi", "dang-xuat"]) {
    const s = readFileSync(`src/app/api/v2/phien/${ten}/route.ts`, "utf8");
    assert.match(s, /laNguonApp\(req\)/, ten);
  }
  assert.match(readFileSync("src/app/api/v2/phien/lam-moi/route.ts", "utf8"), /tokenLamMoi\(req, than, app\)/);
  assert.match(readFileSync("src/app/api/v2/phien/dang-xuat/route.ts", "utf8"), /tokenLamMoi\(req, than, laNguonApp\(req\)\)/);
});
```

- [ ] **Step 2: Chạy, thấy hỏng** — `npx tsx --test tests/phien-app.test.ts` → FAIL «Cannot find module …/nguon-app».

- [ ] **Step 3: Tạo `src/lib/phien/nguon-app.ts`**

```ts
import type { NextRequest } from "next/server";

/** Nguồn (Origin) của WebView app: Android `https://localhost`, iOS `capacitor://localhost`. */
export const NGUON_APP_MAC_DINH = "capacitor://localhost,https://localhost";

/**
 * Yêu cầu từ app di động: tự khai `X-VBP-Nguon: app` VÀ Origin (do trình duyệt đặt, script không giả được) là nguồn WebView
 * của app. Thiếu một trong hai → coi như web (hành vi cũ): XSS trên trang web không đổi được cookie HttpOnly thành token
 * đọc được. Đọc `VBP_NGUON_APP` lúc gọi để biến Coolify luôn có hiệu lực.
 */
export function laNguonApp(req: NextRequest): boolean {
  if ((req.headers.get("x-vbp-nguon") ?? "").trim().toLowerCase() !== "app") return false;
  const nguon = req.headers.get("origin")?.trim();
  if (!nguon) return false;
  const ds = (process.env.VBP_NGUON_APP?.trim() || NGUON_APP_MAC_DINH).split(",").map((x) => x.trim()).filter(Boolean);
  return ds.includes(nguon);
}
```

- [ ] **Step 4: Sửa `src/lib/phien/tra-loi.ts`**

Thay `traLoi` (dòng 10-18 phần nhánh `phien`), `traLoiLamMoi`, `tokenLamMoi`:

```ts
export type TuyChonTraLoi = { app?: boolean };

export function traLoi(kq: KetQua, tc: TuyChonTraLoi = {}): NextResponse {
  if (kq.loai === "phien") {
    const data = {
      access_token: kq.access_token, het_han_giay: kq.het_han_giay, nguoi_dung: kq.nguoi_dung,
      ...(tc.app ? { token_lam_moi: kq.token_lam_moi } : {}),
    };
    const r = NextResponse.json({ success: true, message: "Đã đăng nhập.", data }, { headers: KHONG_LUU });
    // App (khác site với API, không mang cookie): token làm mới đi trong thân, app cất vào Keychain / Keystore.
    if (!tc.app) r.headers.append("Set-Cookie", cookieDat(kq.token_lam_moi));
    return r;
  }
  // … các nhánh can_2_lop / xong / lỗi giữ nguyên …
}

export function traLoiLamMoi(kq: KetQua, tc: TuyChonTraLoi = {}): NextResponse {
  const r = traLoi(kq, tc);
  if (!tc.app && kq.loai === "loi" && kq.code === "PHIEN_THU_HOI") r.headers.append("Set-Cookie", cookieXoa());
  return r;
}

/** Web: cookie trước rồi thân (như cũ). App (`chiThan`): CHỈ thân — không bao giờ đổi cookie trình duyệt thành token trả trong thân. */
export function tokenLamMoi(req: NextRequest, than: Record<string, unknown>, chiThan = false): string | null {
  const tuThan = typeof than.token_lam_moi === "string" ? than.token_lam_moi : null;
  if (chiThan) return tuThan;
  return req.cookies.get(TEN_COOKIE)?.value || tuThan;
}
```
Giữ nguyên chú thích JSDoc cũ của `traLoiLamMoi`.

- [ ] **Step 5: Sửa 6 route** (thêm `import { laNguonApp } from "@lib/phien/nguon-app";`)

```ts
// dang-nhap/route.ts
return chay(async () => traLoi(await dangNhapMatKhau(pt, await docThan(req), ngonCanh(req)), { app: laNguonApp(req) }));
// google/route.ts
return chay(async () => traLoi(await dangNhapGoogle(pt, await docThan(req), ngonCanh(req)), { app: laNguonApp(req) }));
// 2-lop/route.ts
return chay(async () => traLoi(await xacNhanHaiLopDangNhap(pt, await docThan(req), ngonCanh(req)), { app: laNguonApp(req) }));
// otp/xac-nhan/route.ts
return chay(async () => traLoi(await xacNhanOtp(pt, await docThan(req), ngonCanh(req)), { app: laNguonApp(req) }));
// lam-moi/route.ts — thân hàm trong chay:
const app = laNguonApp(req);
const than = await docThan(req);
return traLoiLamMoi(await lamMoi(pt, tokenLamMoi(req, than, app), ngonCanh(req)), { app });
// dang-xuat/route.ts — dòng token:
token: tokenLamMoi(req, than, laNguonApp(req)),
```
`dang-xuat` vẫn luôn xóa cookie (vô hại với app).

- [ ] **Step 6: Chạy** — `npx tsx --test tests/phien-app.test.ts` → PASS; `npm test` → 0 fail, số bài cũ không đổi;
  `npx tsc --noEmit` → 0 lỗi.
- [ ] **Step 7: Commit**

```bash
git add src/lib/phien/nguon-app.ts src/lib/phien/tra-loi.ts src/app/api/v2/phien tests/phien-app.test.ts
git commit -m "phien app: token lam moi trong than khi X-VBP-Nguon app + Origin cua WebView app, khong dat cookie; app chi doc token trong than" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

### Task 2: Google nhận nhiều `aud`

**Files:**
- Modify: `src/lib/phien/google.ts:12-15` (kiểu `tc`), `:38` (so `aud`); thêm `docDsClientGoogle`
- Modify: `src/lib/phien/phu-thuoc.ts:16-20`
- Test: `tests/phien-google-nhieu-aud.test.ts`

**Interfaces:**
- Produces: `kiemIdTokenGoogle(token, tc: { clientId: string | readonly string[]; layKhoa; bayGioGiay })`;
  `export function docDsClientGoogle(giaTri: string | undefined): string[]`. Biến `VBP_GOOGLE_CLIENT_ID` = `web,ios,android`.

- [ ] **Step 1: Bài kiểm hỏng** `tests/phien-google-nhieu-aud.test.ts`

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { generateKeyPairSync, sign } from "node:crypto";
import { docDsClientGoogle, kiemIdTokenGoogle, type KhoaJwk } from "../src/lib/phien/google";

const { privateKey, publicKey } = generateKeyPairSync("rsa", { modulusLength: 2048 });
const jwk = publicKey.export({ format: "jwk" }) as { n: string; e: string };
const KHOA: KhoaJwk[] = [{ kid: "k1", kty: "RSA", n: jwk.n, e: jwk.e, alg: "RS256" }];
const WEB = "web-1.apps.googleusercontent.com";
const IOS = "ios-1.apps.googleusercontent.com";
const LA = "app-khac.apps.googleusercontent.com";
const BAY = 1_800_000_000;

function token(than: Record<string, unknown>) {
  const a = Buffer.from(JSON.stringify({ alg: "RS256", kid: "k1", typ: "JWT" })).toString("base64url");
  const b = Buffer.from(JSON.stringify(than)).toString("base64url");
  return `${a}.${b}.${sign("RSA-SHA256", Buffer.from(`${a}.${b}`), privateKey).toString("base64url")}`;
}
const than = (aud: unknown) => ({ iss: "https://accounts.google.com", aud, sub: "1098", email: "an@gmail.com", email_verified: true, name: "An", iat: BAY - 10, exp: BAY + 3600 });
const kiem = (t: string, clientId: string | readonly string[]) => kiemIdTokenGoogle(t, { clientId, layKhoa: async () => KHOA, bayGioGiay: BAY });

test("danh sách client: aud web hoặc aud iOS đều nhận", async () => {
  assert.equal((await kiem(token(than(WEB)), [WEB, IOS]))?.sub, "1098");
  assert.equal((await kiem(token(than(IOS)), [WEB, IOS]))?.sub, "1098");
  assert.equal((await kiem(token(than(WEB)), WEB))?.sub, "1098"); // dạng chuỗi cũ vẫn chạy
});

test("tấn công: aud của app khác, aud mảng, aud rỗng, danh sách rỗng đều bị từ chối", async () => {
  assert.equal(await kiem(token(than(LA)), [WEB, IOS]), null);
  assert.equal(await kiem(token(than([WEB, LA])), [WEB, IOS]), null);
  assert.equal(await kiem(token(than("")), [WEB, ""]), null);
  assert.equal(await kiem(token(than(WEB)), []), null);
});

test("docDsClientGoogle: tách dấu phẩy, bỏ khoảng trắng và mục rỗng", () => {
  assert.deepEqual(docDsClientGoogle(` ${WEB} ,${IOS},, `), [WEB, IOS]);
  assert.deepEqual(docDsClientGoogle(WEB), [WEB]);
  assert.deepEqual(docDsClientGoogle(undefined), []);
  assert.deepEqual(docDsClientGoogle(" , "), []);
});

test("dây nối: phu-thuoc đọc danh sách bằng docDsClientGoogle", () => {
  assert.match(readFileSync("src/lib/phien/phu-thuoc.ts", "utf8"), /docDsClientGoogle\(process\.env\.VBP_GOOGLE_CLIENT_ID\)/);
});
```

- [ ] **Step 2: Chạy, thấy hỏng** — `npx tsx --test tests/phien-google-nhieu-aud.test.ts` → FAIL (`docDsClientGoogle` chưa có).

- [ ] **Step 3: Sửa `google.ts`**

```ts
/** `VBP_GOOGLE_CLIENT_ID` = danh sách dấu phẩy: client Web (web + app Android qua Credential Manager), client iOS, client Android. */
export function docDsClientGoogle(giaTri: string | undefined): string[] {
  return (giaTri ?? "").split(",").map((x) => x.trim()).filter(Boolean);
}

export async function kiemIdTokenGoogle(
  token: string,
  tc: { clientId: string | readonly string[]; layKhoa: (lamMoi: boolean) => Promise<KhoaJwk[]>; bayGioGiay: number }
): Promise<ThongTinGoogle | null> {
  const dsClient: readonly string[] = typeof tc.clientId === "string" ? [tc.clientId] : tc.clientId;
  // … giữ nguyên tới dòng kiểm iss/aud, thay dòng đó bằng:
  if (!NHA_PHAT_HANH.has(than.iss) || typeof than.aud !== "string" || !than.aud || !dsClient.includes(than.aud)) return null;
  // … phần còn lại giữ nguyên
}
```

- [ ] **Step 4: Sửa `phu-thuoc.ts`**

```ts
import { docDsClientGoogle, kiemIdTokenGoogle, layKhoaGoogle } from "./google";
// …
  kiemGoogle: async (idToken) => {
    const dsClient = docDsClientGoogle(process.env.VBP_GOOGLE_CLIENT_ID);
    if (!dsClient.length) throw new Error("CHUA_CAU_HINH");
    return kiemIdTokenGoogle(idToken, { clientId: dsClient, layKhoa: layKhoaGoogle, bayGioGiay: Math.floor(Date.now() / 1000) });
  },
```

- [ ] **Step 5: Chạy** — bài mới PASS; `npm test` 0 fail (gồm `tests/phien-google.test.ts` cũ không sửa); `npx tsc --noEmit` 0 lỗi.
- [ ] **Step 6: Commit**

```bash
git add src/lib/phien/google.ts src/lib/phien/phu-thuoc.ts tests/phien-google-nhieu-aud.test.ts
git commit -m "google: VBP_GOOGLE_CLIENT_ID nhan danh sach (web, ios, android); aud phai la chuoi khac rong trong danh sach" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

### Task 3: API thử nhánh `dot-b-app` trên máy chủ + kiểm tấn công trên API thật

**Files:**
- Create: `scripts/thu-phien-app.mjs`
- Ngoài repo: `C:/Users/PC/.gikidi/vanbanplus-api-thu.env` (bí mật, xóa ở Task 15 khi app tạm bị gỡ), script tạm trong thư mục
  nháp của phiên.

**Interfaces:**
- Produces: `https://vanbanplus-api-thu.app.giakiemso.com` — API nhánh `dot-b-app`, cùng CSDL / Storage prod. Task 13 đổi biến
  `VBP_GOOGLE_CLIENT_ID` của app này; Task 14 build APK trỏ vào đây. Ghi `uuid` Coolify của app tạm vào ghi chú phiên.

- [ ] **Step 1: Viết `scripts/thu-phien-app.mjs`**

```js
// Kiểm thật luồng phiên app trên một API (mặc định API thử của nhánh dot-b-app). Không in token, mật khẩu.
// Dùng: node scripts/thu-phien-app.mjs [--api https://vanbanplus-api-thu.app.giakiemso.com]
import { randomBytes } from "node:crypto";

const i = process.argv.indexOf("--api");
const API = (i > 0 ? process.argv[i + 1] : "https://vanbanplus-api-thu.app.giakiemso.com").replace(/\/$/, "");
const WEB = "https://app.vanbanplus.giakiemso.com";
let hong = 0;
const buoc = (ten, dung, them = "") => { console.log(`${dung ? "✔" : "✘"} ${ten}${them ? ` — ${them}` : ""}`); if (!dung) hong++; };

async function goi(duong, { than, nguon, app = false, cookie } = {}) {
  const r = await fetch(API + duong, {
    method: "POST", signal: AbortSignal.timeout(30_000),
    headers: {
      "Content-Type": "application/json", "User-Agent": "thu-phien-app/1.0 (Linux; Android 16; wv)",
      ...(nguon ? { Origin: nguon } : {}), ...(app ? { "X-VBP-Nguon": "app" } : {}), ...(cookie ? { Cookie: cookie } : {}),
    },
    body: JSON.stringify(than ?? {}),
  });
  let j = null; try { j = await r.json(); } catch {}
  const sc = r.headers.get("set-cookie") ?? "";
  return { r, j, coCookie: sc.includes("vbp_lam_moi=") && !sc.includes("Max-Age=0"), cookie: sc.split(";")[0] };
}

const email = `thu-app-${randomBytes(4).toString("hex")}@vanbanplus.test`;
const matKhau = randomBytes(15).toString("base64url");
const dk = await goi("/api/auth/register", { than: { email, password: matKhau, fullName: "Thử app" } });
buoc("0. đăng ký tài khoản thử", dk.r.ok, String(dk.r.status));
if (!dk.r.ok) process.exit(1);
const dn = (nguon, app) => goi("/api/v2/phien/dang-nhap", { than: { email, mat_khau: matKhau }, nguon, app });
const khongToken = (x) => x.j?.data?.token_lam_moi === undefined;

const w = await dn(WEB, false);
buoc("1. web: cookie, không token trong thân", w.r.ok && w.coCookie && khongToken(w));
const x = await dn(WEB, true);
buoc("2. tấn công: nguồn web tự thêm X-VBP-Nguon: app → vẫn như web", x.r.ok && x.coCookie && khongToken(x));
const k = await dn(undefined, true);
buoc("3. header app không Origin → như web", k.r.ok && khongToken(k));
const a = await dn("https://localhost", true);
const t1 = a.j?.data?.token_lam_moi;
buoc("4. app Android: token trong thân, không cookie", a.r.ok && typeof t1 === "string" && t1.length > 20 && !a.coCookie);
const ai = await dn("capacitor://localhost", true);
buoc("4b. app iOS: token trong thân, không cookie", ai.r.ok && typeof ai.j?.data?.token_lam_moi === "string" && !ai.coCookie);
const lm = await goi("/api/v2/phien/lam-moi", { than: { token_lam_moi: t1 }, nguon: "https://localhost", app: true });
const t2 = lm.j?.data?.token_lam_moi;
buoc("5. làm mới app: token mới trong thân, khác token cũ, không cookie", lm.r.ok && typeof t2 === "string" && t2 !== t1 && !lm.coCookie);
const lc = await goi("/api/v2/phien/lam-moi", { cookie: w.cookie, nguon: "https://localhost", app: true });
buoc("6. tấn công: cookie web trình qua đường app → 401, không lộ token", lc.r.status === 401 && khongToken(lc));
const lw = await goi("/api/v2/phien/lam-moi", { cookie: w.cookie, nguon: WEB });
buoc("7. web làm mới bằng cookie như cũ: 200 + cookie mới, không token trong thân", lw.r.ok && lw.coCookie && khongToken(lw));
const g = await goi("/api/v2/phien/google", { than: { id_token: "a.b.c" }, nguon: "https://localhost", app: true });
buoc("8. id_token giả bị từ chối", !g.r.ok && khongToken(g), String(g.r.status));
const dx = await goi("/api/v2/phien/dang-xuat", { than: { token_lam_moi: t2 }, nguon: "https://localhost", app: true });
const sau = await goi("/api/v2/phien/lam-moi", { than: { token_lam_moi: t2 }, nguon: "https://localhost", app: true });
buoc("9. đăng xuất app thu hồi token gửi trong thân", dx.r.ok && sau.r.status === 401);
console.log(`Tài khoản thử: ${email} — dọn bằng SQL (khuôn docs/superpowers/plans/2026-11-17-web-l4.md Task 2 Step 3)`);
process.exit(hong ? 1 : 0);
```
Commit: `git add scripts/thu-phien-app.mjs && git commit -m "kiem that phien app tren API thu" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"`.

- [ ] **Step 2: Đẩy nhánh** — `npm test && npx tsc --noEmit && git push -u origin dot-b-app` (chặn theo mã thoát).

- [ ] **Step 3: Chép biến môi trường của app API prod (không in giá trị)** — ghi script tạm vào thư mục nháp của phiên
  (ví dụ `<nháp>/chep-env-coolify.py`), chạy `py -3.13 <nháp>/chep-env-coolify.py vanbanplus.giakiemso.com C:/Users/PC/.gikidi/vanbanplus-api-thu.env`:

```python
# Chép biến môi trường (không phải preview) của app Coolify có tên miền chứa argv[1] sang tệp argv[2]. KHÔNG in giá trị.
import os, sys
sys.path.insert(0, r"D:\Thang\gikidi-server\chuyen")
from coolify import api

ten_mien, ra = sys.argv[1], sys.argv[2]
ung = [a for a in api("GET", "/applications") if ten_mien in (a.get("fqdn") or "") and "vanbanplus-api-thu" not in (a.get("fqdn") or "")]
if len(ung) != 1:
    sys.exit(f"tìm thấy {len(ung)} app có tên miền {ten_mien} — dừng")
envs = [e for e in api("GET", f"/applications/{ung[0]['uuid']}/envs") if not e.get("is_preview")]
giu = [e for e in envs if "\n" not in (e.get("value") or "")]
bo = [e["key"] for e in envs if "\n" in (e.get("value") or "")]
with open(ra, "w", encoding="utf-8", newline="\n") as f:
    for e in giu:
        f.write(f"{e['key']}={e.get('value') or ''}\n")
print(f"app {ung[0]['uuid'][:6]}…: ghi {len(giu)} biến; bỏ {len(bo)} biến nhiều dòng: {bo}")
```
Mong đợi: ghi ≥ 10 biến, `bo` rỗng. `bo` không rỗng → dừng, báo chủ dự án (không tự xử lý biến nhiều dòng).

- [ ] **Step 4: Tạo app tạm trên Coolify** (đọc `D:/Thang/gikidi-server/CLAUDE.md` mục `du-an-moi.py` trước)

```bash
cd D:/Thang/gikidi-server
MSYS_NO_PATHCONV=1 py -3.13 cong-cu/du-an-moi.py tao vanbanplus-api-thu --repo ericphan28/vanbanplus-api --nhanh dot-b-app \
  --env C:/Users/PC/.gikidi/vanbanplus-api-thu.env --ram 2g --thu
# xem bản kế hoạch in ra, đúng thì chạy lại không --thu
MSYS_NO_PATHCONV=1 py -3.13 cong-cu/du-an-moi.py tao vanbanplus-api-thu --repo ericphan28/vanbanplus-api --nhanh dot-b-app \
  --env C:/Users/PC/.gikidi/vanbanplus-api-thu.env --ram 2g
```
Không chạy `tu-trien-khai.py bat` (app tạm, triển khai lại tay). Không thêm vào Uptime Kuma. App không giữ dữ liệu riêng
(CSDL + Storage là của prod, đã có sao lưu) → không cần `--ben`. Ghi `uuid` in ra.

- [ ] **Step 5: Kiểm sống + CORS** — `curl -s -o /dev/null -w "%{http_code}\n" https://vanbanplus-api-thu.app.giakiemso.com/api/health` → `200`;

```bash
curl -s -o /dev/null -D - -X OPTIONS https://vanbanplus-api-thu.app.giakiemso.com/api/v2/phien/lam-moi \
  -H "Origin: https://localhost" -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: content-type,x-vbp-nguon,x-vbp-phien-ban" | grep -i "^access-control-allow-origin"
```
Mong đợi `access-control-allow-origin: https://localhost` (Storage đã trả `*` cho cả hai nguồn app — kiểm 19/11).

- [ ] **Step 6: Chạy kịch bản tấn công trên API thật** — `node scripts/thu-phien-app.mjs` → 11 dòng ✔, mã thoát 0. Dọn tài khoản thử
  bằng SQL trong giao dịch theo khuôn `docs/superpowers/plans/2026-11-17-web-l4.md` Task 2 Step 3 (lệnh `psql` vào
  `supabase-db-q323olywghtnjcvpyvtjzo9v` ở Global Constraints của `docs/superpowers/plans/2026-11-18-web-mo-ban.md`), đếm dòng
  `users` = 1 trước `COMMIT`.

- [ ] **Step 7: Ghi `D:/Thang/gikidi-server/THAY-DOI.md`** một dòng: «VanBanPlus: app Coolify TẠM `vanbanplus-api-thu` (nhánh
  `dot-b-app` của vanbanplus-api, biến chép từ app API prod, CSDL / Storage prod) để thử app di động · gỡ: `DELETE
  /api/v1/applications/<uuid>` + xóa `C:/Users/PC/.gikidi/vanbanplus-api-thu.env` khi API app lên `main`». Commit riêng tệp đó ở
  repo gikidi-server (`git add THAY-DOI.md`), đẩy `main` của gikidi-server (repo công cụ, không triển khai app — như các kế hoạch
  trước).

---

## Phần B — Web (`D:/AIVanBanCaNhan/_wt-b-app-web`)

Mỗi task web: `npx vitest run <tệp kiểm>` khi làm; trước commit `npm test` + `npx tsc -b` xanh.

### Task 4: Lớp nền tảng (`nen-tang.ts`, `app-di-dong/`, gói ghim, build `--mode app`)

**Files:**
- Create: `src/nen-tang.ts`, `src/app-di-dong/may.ts`, `src/app-di-dong/tep.ts`, `src/app-di-dong/khoi-dong.ts`,
  `src/test/may-gia.ts`, `.env.app`
- Modify: `vite.config.ts`, `src/vite-env.d.ts`, `src/main.tsx`, `.env.example`, `package.json`, `package-lock.json`
- Test: `src/nen-tang.test.ts`, `src/app-di-dong/tep.test.ts`

**Interfaces:**
- Produces (`src/nen-tang.ts`):
  - `type KetQuaQuet = { loai: "anh"; tep: File[] } | { loai: "huy" } | { loai: "loi"; thongBao: string }`
  - `type KhoToken = { doc(): Promise<string | null>; ghi(token: string): Promise<void>; xoa(): Promise<void> }`
  - `type MayApp = { khoToken: KhoToken; ghiVaChiaSe(tep: Blob, ten: string): Promise<void>; taiVaMo(url: string, ten: string, mime: string): Promise<void>; moTrinhDuyet(url: string): Promise<void>; googleIdToken(): Promise<string | null>; quetTaiLieu(toiDa: number): Promise<KetQuaQuet>; ngheNutBack(fn: (coTheLui: boolean) => void): () => void; thoatApp(): void }`
  - `laApp(): boolean` · `coTheMua(): boolean` · `dauNenTang(): Record<string, string>` · `layMay(): Promise<MayApp>` ·
    `datNenTangThu(t: { app: boolean; may?: MayApp } | null): void` (chỉ bài kiểm) · `luuTep(tep: Blob, ten: string): Promise<void>` ·
    `moTep(layUrl: () => Promise<string>, ten: string, mime: string): Promise<void>` · `moNgoai(url: string): Promise<void>` ·
    `chuLoiTrungTinh(code: string, message: string): string`
- Produces (`src/app-di-dong/`): `may: MayApp`, `khoiTaoMay(): Promise<void>` (may.ts); `tenTepAnToan(ten: string): string`,
  `blobSangBase64(b: Blob): Promise<string>` (tep.ts); `khoiDong(): Promise<void>` (khoi-dong.ts).
- Produces (`src/test/may-gia.ts`): `taoMayGia(tokenDau?: string | null): { may: MayApp & { …vi.fn… }; tokenHienTai(): string | null }`.

- [ ] **Step 0: Worktree + gói ghim**

```bash
cd D:/AIVanBanCaNhan/vanbanplus-web && git fetch origin
git worktree add D:/AIVanBanCaNhan/_wt-b-app-web -b dot-b-app origin/main
cd D:/AIVanBanCaNhan/_wt-b-app-web && npm ci && npm test
npm install --save-exact @capacitor/core@8.6.2 @capacitor/android@8.6.2 @capacitor/ios@8.6.2 @capacitor/app@8.1.2 \
  @capacitor/filesystem@8.1.5 @capacitor/file-transfer@2.0.6 @capacitor/share@8.0.3 @capacitor/browser@8.0.5 \
  @capacitor/preferences@8.0.1 @aparajita/capacitor-secure-storage@8.0.1 @capgo/capacitor-social-login@8.7.11 \
  @capgo/capacitor-document-scanner@8.4.20 @capacitor-community/file-opener@8.0.1
npm install --save-exact -D @capacitor/cli@8.6.2
```
Kiểm `package.json`: 13 gói trên **không có** `^`. (Coolify build web sẽ cài thêm các gói này — chỉ tốn thời gian cài, không
vào gói web.)

- [ ] **Step 1: Bài kiểm hỏng** `src/nen-tang.test.ts`

```ts
import { afterEach, beforeEach, describe, expect, test, vi } from "vitest";
import { chuLoiTrungTinh, coTheMua, datNenTangThu, dauNenTang, laApp, layMay, luuTep, moNgoai, moTep } from "./nen-tang";
import { taoMayGia } from "@/test/may-gia";

afterEach(() => { datNenTangThu(null); vi.useRealTimers(); vi.restoreAllMocks(); });

describe("bản web (mặc định): hành vi như trước", () => {
  test("không phải app, mua được, không header nền tảng, không nạp lớp máy", async () => {
    expect(laApp()).toBe(false);
    expect(coTheMua()).toBe(true);
    expect(dauNenTang()).toEqual({});
    await expect(layMay()).rejects.toThrow("KHONG_PHAI_APP");
  });
  test("luuTep: blob + a[download], gỡ thẻ ngay, thu hồi URL sau 1 giây", async () => {
    vi.useFakeTimers();
    URL.createObjectURL = vi.fn(() => "blob:x");
    URL.revokeObjectURL = vi.fn();
    const bam = vi.spyOn(HTMLAnchorElement.prototype, "click").mockImplementation(() => {});
    await luuTep(new Blob(["a"]), "Van-ban.docx");
    const a = bam.mock.instances[0] as unknown as HTMLAnchorElement;
    expect(a.download).toBe("Van-ban.docx");
    expect(a.getAttribute("href")).toBe("blob:x");
    expect(a.isConnected).toBe(false);
    expect(URL.revokeObjectURL).not.toHaveBeenCalled();
    vi.advanceTimersByTime(1000);
    expect(URL.revokeObjectURL).toHaveBeenCalledWith("blob:x");
  });
  test("moTep: mở thẻ trước khi chờ URL, cắt opener, rồi chuyển tới URL", async () => {
    const w = { opener: {} as unknown, location: { href: "" }, close: vi.fn() };
    vi.spyOn(window, "open").mockReturnValue(w as unknown as Window);
    await moTep(async () => "https://s.vn/a.pdf", "a.pdf", "application/pdf");
    expect(window.open).toHaveBeenCalledWith("", "_blank");
    expect(w.opener).toBeNull();
    expect(w.location.href).toBe("https://s.vn/a.pdf");
  });
  test("moTep: lấy URL lỗi → đóng thẻ đã mở, ném lỗi", async () => {
    const w = { opener: {}, location: { href: "" }, close: vi.fn() };
    vi.spyOn(window, "open").mockReturnValue(w as unknown as Window);
    await expect(moTep(async () => { throw new Error("hong"); }, "a.pdf", "application/pdf")).rejects.toThrow("hong");
    expect(w.close).toHaveBeenCalled();
  });
  test("moNgoai: tab mới noopener; chuLoiTrungTinh giữ nguyên chữ", async () => {
    vi.spyOn(window, "open").mockReturnValue(null);
    await moNgoai("https://vanbanplus.giakiemso.com/dieu-khoan");
    expect(window.open).toHaveBeenCalledWith("https://vanbanplus.giakiemso.com/dieu-khoan", "_blank", "noopener,noreferrer");
    expect(chuLoiTrungTinh("HET_DUNG_LUONG", "mua thêm dung lượng")).toBe("mua thêm dung lượng");
  });
});

describe("bản app", () => {
  let gia: ReturnType<typeof taoMayGia>;
  beforeEach(() => { gia = taoMayGia(); datNenTangThu({ app: true, may: gia.may }); });
  test("là app, không mua, header X-VBP-Nguon app + phiên bản", () => {
    expect(laApp()).toBe(true);
    expect(coTheMua()).toBe(false);
    expect(dauNenTang()).toEqual({ "X-VBP-Nguon": "app", "X-VBP-Phien-Ban": expect.stringMatching(/^app-/) });
  });
  test("luuTep → ghi + bảng Chia sẻ; không tạo a[download]", async () => {
    const bam = vi.spyOn(HTMLAnchorElement.prototype, "click");
    const b = new Blob(["x"]);
    await luuTep(b, "Van-ban.docx");
    expect(gia.may.ghiVaChiaSe).toHaveBeenCalledWith(b, "Van-ban.docx");
    expect(bam).not.toHaveBeenCalled();
  });
  test("moTep → tải về rồi mở bằng ứng dụng của máy; không window.open", async () => {
    const mo = vi.spyOn(window, "open");
    await moTep(async () => "https://s.vn/a.pdf", "a.pdf", "application/pdf");
    expect(gia.may.taiVaMo).toHaveBeenCalledWith("https://s.vn/a.pdf", "a.pdf", "application/pdf");
    expect(mo).not.toHaveBeenCalled();
  });
  test("moNgoai → trình duyệt trong app", async () => {
    await moNgoai("https://meet.google.com/abc");
    expect(gia.may.moTrinhDuyet).toHaveBeenCalledWith("https://meet.google.com/abc");
  });
  test("HET_DUNG_LUONG: câu báo trung tính, không nhắc mua; mã khác giữ nguyên", () => {
    const c = chuLoiTrungTinh("HET_DUNG_LUONG", "Đã dùng hết dung lượng lưu trữ. Kho đang chỉ đọc: mua thêm dung lượng hoặc xóa bớt tệp.");
    expect(c).toBe("Kho đã đầy — đang ở chế độ chỉ đọc. Xóa bớt tệp để thêm mới.");
    expect(c).not.toMatch(/mua|web|giá/i);
    expect(chuLoiTrungTinh("OTP_SAI", "Mã không đúng.")).toBe("Mã không đúng.");
  });
});
```

`src/app-di-dong/tep.test.ts`:

```ts
import { expect, test } from "vitest";
import { blobSangBase64, tenTepAnToan } from "./tep";

test("tenTepAnToan: giữ tiếng Việt, bỏ ký tự đường dẫn / điều khiển / dấu chấm đầu (chống ghi ra ngoài thư mục tạm)", () => {
  expect(tenTepAnToan("Công văn số 12.pdf")).toBe("Công văn số 12.pdf");
  expect(tenTepAnToan("../../etc/passwd")).toBe("-etc-passwd");
  expect(tenTepAnToan("..\\..\\a:b*c?.docx")).toBe("-a-b-c-.docx");
  expect(tenTepAnToan("a\u0000b\nc.txt")).toBe("a-b-c.txt");
  expect(tenTepAnToan("...")).toBe("tep");
  expect(tenTepAnToan("x".repeat(300)).length).toBe(120);
});

test("blobSangBase64: chỉ phần base64, không tiền tố data:", async () => {
  expect(await blobSangBase64(new Blob(["Xin chào"], { type: "text/plain" }))).toBe(Buffer.from("Xin chào").toString("base64"));
});
```

- [ ] **Step 2: Chạy, thấy hỏng** — `npx vitest run src/nen-tang.test.ts src/app-di-dong/tep.test.ts` → FAIL (chưa có mô-đun).

- [ ] **Step 3: `src/nen-tang.ts`**

```ts
// Một chỗ cho mọi khác biệt web ↔ app (spec §2). Bản web: hành vi như trước và KHÔNG nạp mã Capacitor — vite.config.ts biến
// import.meta.env.VITE_NEN_TANG thành hằng lúc build nên nhánh `import("@/app-di-dong/may")` bị loại khỏi gói web.
export type KetQuaQuet = { loai: "anh"; tep: File[] } | { loai: "huy" } | { loai: "loi"; thongBao: string };
export type KhoToken = { doc(): Promise<string | null>; ghi(token: string): Promise<void>; xoa(): Promise<void> };
/** Việc cần máy làm (src/app-di-dong/may.ts hiện thực bằng plugin Capacitor; bài kiểm dùng src/test/may-gia.ts). */
export type MayApp = {
  khoToken: KhoToken;
  /** Ghi tệp vào bộ nhớ tạm rồi mở bảng Chia sẻ (lưu vào Tệp / gửi Zalo…). */
  ghiVaChiaSe(tep: Blob, ten: string): Promise<void>;
  /** Tải URL (ký) về bộ nhớ tạm rồi mở bằng ứng dụng xem của máy. */
  taiVaMo(url: string, ten: string, mime: string): Promise<void>;
  moTrinhDuyet(url: string): Promise<void>;
  /** `id_token` Google; null = người dùng đóng hộp chọn tài khoản. */
  googleIdToken(): Promise<string | null>;
  quetTaiLieu(toiDa: number): Promise<KetQuaQuet>;
  ngheNutBack(fn: (coTheLui: boolean) => void): () => void;
  thoatApp(): void;
};

const LA_APP_BUILD = import.meta.env.VITE_NEN_TANG === "app";
let thu: { app: boolean; may?: MayApp } | null = null;
let mayDaNap: Promise<MayApp> | null = null;

/** CHỈ bài kiểm: bật / tắt chế độ app và gắn máy giả; null = trả về theo bản build. */
export function datNenTangThu(t: { app: boolean; may?: MayApp } | null): void { thu = t; mayDaNap = null; }

export const laApp = (): boolean => thu?.app ?? LA_APP_BUILD;
/** Bản app không bán hàng (tránh In-App Purchase): mọi lối vào trang Mua đi qua hàm này. */
export const coTheMua = (): boolean => !laApp();

export function layMay(): Promise<MayApp> {
  if (thu?.may) return Promise.resolve(thu.may);
  if (import.meta.env.VITE_NEN_TANG === "app") return (mayDaNap ??= import("@/app-di-dong/may").then((m) => m.may));
  return Promise.reject(new Error("KHONG_PHAI_APP"));
}

/** Header gắn vào MỌI lời gọi API (src/api/khach.ts). Web: không thêm gì. */
export function dauNenTang(): Record<string, string> {
  if (!laApp()) return {};
  return { "X-VBP-Nguon": "app", "X-VBP-Phien-Ban": `app-${import.meta.env.VITE_PHIEN_BAN || "dev"}` };
}

/** Lưu tệp người dùng tạo (Word, mã dự phòng). Web: blob + a[download]. App: ghi + bảng Chia sẻ. */
export async function luuTep(tep: Blob, ten: string): Promise<void> {
  if (laApp()) return (await layMay()).ghiVaChiaSe(tep, ten);
  const url = URL.createObjectURL(tep);
  const a = document.createElement("a");
  a.href = url; a.download = ten; a.rel = "noopener";
  document.body.appendChild(a); a.click(); a.remove();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}

/** Mở tệp qua đường ký. Web: mở thẻ TRƯỚC khi chờ API (trình chặn cửa sổ bật lên không chặn). App: tải về + mở bằng máy. */
export async function moTep(layUrl: () => Promise<string>, ten: string, mime: string): Promise<void> {
  if (laApp()) return (await layMay()).taiVaMo(await layUrl(), ten, mime);
  const w = window.open("", "_blank");
  try {
    const url = await layUrl();
    if (w) { w.opener = null; w.location.href = url; } else window.location.assign(url);
  } catch (e) {
    w?.close();
    throw e;
  }
}

/** Liên kết ra ngoài. Web: thẻ mới. App: trình duyệt trong app (tách khỏi WebView có cầu nối Capacitor). */
export async function moNgoai(url: string): Promise<void> {
  if (laApp()) return (await layMay()).moTrinhDuyet(url);
  window.open(url, "_blank", "noopener,noreferrer");
}

/** Câu báo lỗi từ API mà bản app không được nói (nhắc mua). Web: giữ nguyên. */
export function chuLoiTrungTinh(code: string, message: string): string {
  if (!laApp()) return message;
  if (code === "HET_DUNG_LUONG") return "Kho đã đầy — đang ở chế độ chỉ đọc. Xóa bớt tệp để thêm mới.";
  return message;
}
```

- [ ] **Step 4: `src/app-di-dong/tep.ts`**

```ts
/** Tên tệp dùng làm đường dẫn trong bộ nhớ tạm: giữ chữ Việt, bỏ ký tự đường dẫn / điều khiển, không bắt đầu bằng dấu chấm. */
export function tenTepAnToan(ten: string): string {
  const s = ten.normalize("NFC")
    .replace(/[\\/:*?"<>|\u0000-\u001f]+/g, "-")
    .replace(/-*\.{2,}-*/g, "-")
    .replace(/-{2,}/g, "-")
    .replace(/^[.\s]+/, "")
    .trim()
    .slice(0, 120);
  return /^[-.\s]*$/.test(s) ? "tep" : s;
}

/** Blob → base64 thuần (Filesystem.writeFile trên máy nhận chuỗi base64). */
export function blobSangBase64(b: Blob): Promise<string> {
  return new Promise((ok, hong) => {
    const r = new FileReader();
    r.onload = () => { const s = String(r.result); ok(s.slice(s.indexOf(",") + 1)); };
    r.onerror = () => hong(r.error ?? new Error("doc-tep"));
    r.readAsDataURL(b);
  });
}
```
(Đã chạy thử hàm này bằng Node lúc viết kế hoạch: đủ 6 ca của `tep.test.ts`.)

- [ ] **Step 5: `src/app-di-dong/may.ts`** (chỉ tệp này import plugin)

```ts
// Lớp máy của bản app: mỗi hàm bọc đúng một việc của plugin Capacitor. Chỉ nạp khi build --mode app (src/nen-tang.ts).
import { App } from "@capacitor/app";
import { Browser } from "@capacitor/browser";
import { Capacitor } from "@capacitor/core";
import { FileTransfer } from "@capacitor/file-transfer";
import { Directory, Filesystem } from "@capacitor/filesystem";
import { Preferences } from "@capacitor/preferences";
import { Share } from "@capacitor/share";
import { SecureStorage } from "@aparajita/capacitor-secure-storage";
import { SocialLogin } from "@capgo/capacitor-social-login";
import { DocumentScanner, ResponseType, ScanDocumentResponseStatus } from "@capgo/capacitor-document-scanner";
import { FileOpener } from "@capacitor-community/file-opener";
import { LoiApi } from "@/api/khach";
import type { KetQuaQuet, MayApp } from "@/nen-tang";
import { blobSangBase64, tenTepAnToan } from "./tep";

const KHOA_TOKEN = "token_lam_moi";
const CO_DA_CAI = "vbp_da_cai";
const huyBoi = (e: unknown) => /cancel/i.test(String((e as { message?: unknown } | null)?.message ?? e));

/** Một lần khi mở app, trước khi vẽ giao diện. */
export async function khoiTaoMay(): Promise<void> {
  await SecureStorage.setSynchronize(false); // không đồng bộ iCloud Keychain
  // iOS giữ Keychain sau khi gỡ app: lần chạy đầu của một bản cài mới thì bỏ token cũ (không tự vào phiên người trước).
  if ((await Preferences.get({ key: CO_DA_CAI })).value !== "1") {
    await SecureStorage.remove(KHOA_TOKEN).catch(() => false);
    await Preferences.set({ key: CO_DA_CAI, value: "1" });
  }
  // văn bản đã tải / đã xuất lần trước không nằm lại trên máy
  for (const path of ["mo", "xuat"]) await Filesystem.rmdir({ path, directory: Directory.Cache, recursive: true }).catch(() => {});
  const web = import.meta.env.VITE_GOOGLE_CLIENT_ID;
  if (web) {
    await SocialLogin.initialize({
      google: {
        webClientId: web, iOSClientId: import.meta.env.VITE_GOOGLE_IOS_CLIENT_ID || undefined, iOSServerClientId: web,
        mode: "online", skipAuthOnlyAuthorizationAfterSignIn: true,
      },
    });
  }
}

async function ghiTam(thuMuc: "mo" | "xuat", ten: string): Promise<string> {
  await Filesystem.mkdir({ path: thuMuc, directory: Directory.Cache, recursive: true }).catch(() => {});
  return (await Filesystem.getUri({ path: `${thuMuc}/${Date.now()}-${tenTepAnToan(ten)}`, directory: Directory.Cache })).uri;
}

export const may: MayApp = {
  khoToken: {
    doc: async () => { const v = await SecureStorage.get(KHOA_TOKEN, false); return typeof v === "string" && v ? v : null; },
    ghi: async (token) => { await SecureStorage.set(KHOA_TOKEN, token, false); },
    xoa: async () => { await SecureStorage.remove(KHOA_TOKEN); },
  },
  async ghiVaChiaSe(tep, ten) {
    await Filesystem.mkdir({ path: "xuat", directory: Directory.Cache, recursive: true }).catch(() => {});
    const { uri } = await Filesystem.writeFile({
      path: `xuat/${tenTepAnToan(ten)}`, data: await blobSangBase64(tep), directory: Directory.Cache, recursive: true,
    });
    try { await Share.share({ title: ten, files: [uri], dialogTitle: "Lưu hoặc gửi tệp" }); }
    catch (e) { if (!huyBoi(e)) throw e; }
  },
  async taiVaMo(url, ten, mime) {
    const uri = await ghiTam("mo", ten);
    await FileTransfer.downloadFile({ url, path: uri });
    try { await FileOpener.open({ filePath: uri, contentType: mime || undefined, openWithDefault: true }); }
    catch { throw new LoiApi(0, "KHONG_MO_DUOC", "Máy chưa có ứng dụng mở được loại tệp này."); }
  },
  async moTrinhDuyet(url) { await Browser.open({ url }); },
  async googleIdToken() {
    try {
      const r = await SocialLogin.login({ provider: "google", options: {} });
      const t = (r.result as { idToken?: string | null }).idToken;
      if (!t) throw new Error("khong-co-id-token");
      return t;
    } catch (e) {
      if (huyBoi(e)) return null;
      throw e;
    }
  },
  async quetTaiLieu(toiDa): Promise<KetQuaQuet> {
    try {
      const r = await DocumentScanner.scanDocument({
        responseType: ResponseType.ImageFilePath, maxNumDocuments: Math.max(1, Math.min(24, toiDa)), croppedImageQuality: 90,
      });
      if (r.status === ScanDocumentResponseStatus.Cancel || !r.scannedImages?.length) return { loai: "huy" };
      const tep = await Promise.all(r.scannedImages.map(async (p, i) => {
        const b = await (await fetch(Capacitor.convertFileSrc(p))).blob();
        return new File([b], `trang-${String(i + 1).padStart(2, "0")}.jpg`, { type: "image/jpeg" });
      }));
      return { loai: "anh", tep };
    } catch (e) {
      if (huyBoi(e)) return { loai: "huy" };
      if (/permission|denied|not authorized/i.test(String((e as { message?: unknown } | null)?.message ?? e))) {
        return { loai: "loi", thongBao: "Chưa có quyền dùng máy ảnh. Cấp quyền trong Cài đặt của máy, hoặc bấm «Chọn tệp» để chọn ảnh có sẵn." };
      }
      return { loai: "loi", thongBao: "Máy này chưa mở được trình quét tài liệu. Bấm «Chọn tệp» để chọn ảnh có sẵn." };
    }
  },
  ngheNutBack(fn) {
    const h = App.addListener("backButton", (e) => fn(e.canGoBack));
    return () => { void h.then((x) => x.remove()); };
  },
  thoatApp() { void App.exitApp(); },
};
```
`npx tsc -b` phải qua với kiểu thật của plugin (đã đọc `definitions.d.ts` 19/11: `SecureStorage.get(key, convertDate?, sync?)`,
`set(key, data, convertDate?, sync?)`, `SocialLogin.login({ provider: "google", options })` trả `{ provider, result }`,
`ResponseType.ImageFilePath`, `ScanDocumentResponseStatus.Cancel`, `FileTransfer.downloadFile({ url, path })`). Lệch kiểu thì
sửa theo `node_modules/<gói>/dist/**/definitions.d.ts`, giữ nguyên hành vi.

- [ ] **Step 6: `src/app-di-dong/khoi-dong.ts`**

```ts
import { khoiTaoMay } from "./may";

/** Chạy một lần trước khi vẽ giao diện (src/main.tsx, chỉ bản app). */
export async function khoiDong(): Promise<void> {
  await khoiTaoMay();
}
```

- [ ] **Step 7: `src/test/may-gia.ts`**

```ts
import { vi } from "vitest";
import type { KetQuaQuet, MayApp } from "@/nen-tang";

/** Máy giả cho bài kiểm chế độ app: kho token trong bộ nhớ, mọi việc khác là vi.fn. */
export function taoMayGia(tokenDau: string | null = null) {
  let token = tokenDau;
  const may = {
    khoToken: {
      doc: vi.fn(async () => token),
      ghi: vi.fn(async (t: string) => { token = t; }),
      xoa: vi.fn(async () => { token = null; }),
    },
    ghiVaChiaSe: vi.fn(async (_tep: Blob, _ten: string) => {}),
    taiVaMo: vi.fn(async (_url: string, _ten: string, _mime: string) => {}),
    moTrinhDuyet: vi.fn(async (_url: string) => {}),
    googleIdToken: vi.fn(async (): Promise<string | null> => "id-token-gia"),
    quetTaiLieu: vi.fn(async (_toiDa: number): Promise<KetQuaQuet> => ({ loai: "huy" })),
    ngheNutBack: vi.fn((_fn: (coTheLui: boolean) => void) => () => {}),
    thoatApp: vi.fn(),
  } satisfies MayApp;
  return { may, tokenHienTai: () => token };
}
```

- [ ] **Step 8: `vite.config.ts`, `src/vite-env.d.ts`, `.env.app`, `.env.example`, `package.json`**

```ts
/// <reference types="vitest/config" />
import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import { fileURLToPath, URL } from "node:url";

export default defineConfig(({ command, mode }) => {
  const env = loadEnv(mode, process.cwd(), "VITE_");
  return {
    plugins: [react()],
    // Lúc build, VITE_NEN_TANG thành hằng → bản web loại hẳn nhánh app (không có mã Capacitor). Bài kiểm (serve) không đổi.
    define: command === "build" ? { "import.meta.env.VITE_NEN_TANG": JSON.stringify(env.VITE_NEN_TANG ?? "") } : {},
    resolve: { alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) } },
    server: { port: 5173 },
    build: {
      rollupOptions: {
        // os-kit có "use client" (cho Next) — vô hại ở Vite
        onwarn(w, warn) { if (w.code === "MODULE_LEVEL_DIRECTIVE") return; warn(w); },
      },
    },
    test: { globals: true, environment: "jsdom", setupFiles: ["./src/test/setup.ts"], include: ["src/**/*.test.{ts,tsx}"], css: false, testTimeout: 15000 },
  };
});
```
`src/vite-env.d.ts` dòng 2 thêm `readonly VITE_NEN_TANG?: string; readonly VITE_GOOGLE_IOS_CLIENT_ID?: string;`.

Lấy client ID Web Google đang dùng (công khai — nằm sẵn trong gói JS của web):

```bash
py -3.13 -c "import sys; sys.path.insert(0, r'D:\Thang\gikidi-server\chuyen'); from coolify import api; a=[x for x in api('GET','/applications') if 'app.vanbanplus.giakiemso.com' in (x.get('fqdn') or '')][0]; print([e['value'] for e in api('GET', '/applications/%s/envs' % a['uuid']) if e['key']=='VITE_GOOGLE_CLIENT_ID' and not e.get('is_preview')][0])"
```
`.env.app` (commit — chỉ giá trị công khai):

```
# Bản app di động: npm run build:app (vite build --mode app && cap sync). Ghi đè cục bộ (API thử…) ở .env.app.local (không commit).
VITE_NEN_TANG=app
VITE_PHIEN_BAN=0.1.0
VITE_API_GOC=https://vanbanplus.giakiemso.com
VITE_API_GOC_TAC_VU=https://vanbanplus-api.app.giakiemso.com
VITE_STORAGE_GOC=https://api-vanbanplus.giakiemso.com
VITE_GOOGLE_CLIENT_ID=<giá trị vừa in, dạng …apps.googleusercontent.com>
VITE_GOOGLE_IOS_CLIENT_ID=
```
`.env.example` thêm hai dòng chú thích: `# VITE_NEN_TANG=app chỉ đặt trong .env.app (bản app); bản web để trống.` và
`VITE_GOOGLE_IOS_CLIENT_ID=`. `package.json` `scripts` thêm `"build:app": "tsc -b && vite build --mode app && cap sync"`.

- [ ] **Step 9: `src/main.tsx`**

```tsx
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router";
import "@/os-kit/os-kit.css";
import "@/giao-dien/goc.css";
import { App } from "@/App";
import { LoiTai } from "@/vo/LoiTai";
import { caiLamMoiKhiLoiChunk } from "@/vo/lam-moi";

caiLamMoiKhiLoiChunk();

function ve() {
  createRoot(document.getElementById("goc")!).render(
    <StrictMode><LoiTai><BrowserRouter><App /></BrowserRouter></LoiTai></StrictMode>,
  );
}

// Bản app: khởi động lớp máy (kho token, Google, liên kết ngoài) trước khi vẽ; lỗi khởi động vẫn vẽ (mật khẩu vẫn đăng nhập được).
if (import.meta.env.VITE_NEN_TANG === "app") {
  void import("@/app-di-dong/khoi-dong").then((m) => m.khoiDong()).catch((e) => console.error("Khởi động app lỗi:", e)).finally(ve);
} else {
  ve();
}
```

- [ ] **Step 10: Chạy kiểm**

```bash
npx vitest run src/nen-tang.test.ts src/app-di-dong/tep.test.ts   # PASS
npm test && npx tsc -b                                              # toàn bộ xanh, bài cũ không sửa
npm run build && (rg -l "capacitor|SocialLogin|DocumentScanner|SecureStorage|app-di-dong" dist/assets && echo "HỎNG: gói web có mã app" || echo "gói web sạch")
npx vite build --mode app && rg -l "SocialLogin" dist/assets && npm run build
```
Mong đợi: «gói web sạch»; bản `--mode app` có `SocialLogin` (đối chứng); lệnh cuối dựng lại `dist/` bản web. Gói web còn mã app →
thêm vào `vite.config.ts` `resolve.alias` cho mode khác `app`: `"@/app-di-dong/may"` → tệp rỗng `src/app-di-dong/may-web.ts`
(`export const may = null as never;`), chạy lại cho tới khi sạch.

- [ ] **Step 11: Commit**

```bash
git add src/nen-tang.ts src/nen-tang.test.ts src/app-di-dong src/test/may-gia.ts src/main.tsx src/vite-env.d.ts vite.config.ts .env.app .env.example package.json package-lock.json
git commit -m "nen tang web/app: nen-tang.ts, lop may Capacitor (ghim ban), build --mode app; goi web khong co ma app" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

### Task 5: Phiên trong app (token làm mới trong kho an toàn của máy)

**Files:**
- Modify: `src/api/khach.ts` (`datPhien`, `guiMot`, `guiLamMoi`, `lamMoiMotLan`, `goi`; thêm `thanLamMoi`, `catTokenLamMoi`, `thanDangXuat`)
- Modify: `src/api/phien.ts:24-32` (`dangXuat`)
- Test: `src/api/khach-app.test.ts`

**Interfaces:**
- Consumes: `laApp`, `layMay`, `dauNenTang`, `chuLoiTrungTinh` (Task 4); hợp đồng API Task 1.
- Produces: `export async function thanDangXuat(): Promise<Record<string, string>>` (web `{}`; app `{ token_lam_moi }`). Giao diện
  (PhienProvider, `api.dangNhap`…) **không bao giờ** thấy `token_lam_moi`.

- [ ] **Step 1: Bài kiểm hỏng** `src/api/khach-app.test.ts`

```ts
import { afterEach, beforeEach, describe, expect, test, vi } from "vitest";
import { caiKhach, datLaiTacVuQuaGoc, datPhien, goi, khiDoiPhien, lamMoi, LoiApi } from "./khach";
import { api } from "./phien";
import type { PhienTraVe } from "./loai";
import { datNenTangThu } from "@/nen-tang";
import { taoMayGia } from "@/test/may-gia";

const ND = { id: "u1", email: "a@b.vn", ho_ten: "A", goi: "free", goi_het_han: null, co_mat_khau: true, hai_lop_bat: false, google_gan: false };
const phien = (t: string): PhienTraVe => ({ access_token: t, het_han_giay: 900, nguoi_dung: ND });
const tl = (http: number, than: unknown) => new Response(JSON.stringify(than), { status: http, headers: { "Content-Type": "application/json" } });
let daGoi: { url: string; init: RequestInit }[];
function giaFetch(...traLoi: Response[]) {
  const f = vi.fn(async (url: string, init: RequestInit) => {
    daGoi.push({ url, init });
    const r = traLoi.shift();
    if (!r) throw new Error("hết trả lời giả");
    return r;
  });
  caiKhach({ goc: "https://api.thu", gocTacVu: "https://api.thu", fetch: f as unknown as typeof fetch, cho: async () => {}, khoa: undefined });
  return f;
}
const than = (i: number) => JSON.parse(String(daGoi[i].init.body));

let gia: ReturnType<typeof taoMayGia>;
beforeEach(() => {
  daGoi = []; datLaiTacVuQuaGoc(); datPhien(null);
  gia = taoMayGia("TLM-cu"); datNenTangThu({ app: true, may: gia.may });
});
afterEach(() => datNenTangThu(null));

describe("bản app", () => {
  test("mọi yêu cầu: X-VBP-Nguon app (đè header web của tác vụ), không gửi cookie", async () => {
    giaFetch(tl(200, { success: true, message: "ok", data: 1 }), tl(200, { success: true, message: "ok", data: 1 }));
    datPhien(phien("T1"));
    await goi("/api/v2/toi");
    await goi("/api/v2/tac-vu/tom-tat", { phuongThuc: "POST", than: {}, dau: { "X-VBP-Nguon": "web", "X-VBP-Phien-Ban": "web-x" } });
    for (const g of daGoi) {
      const h = g.init.headers as Record<string, string>;
      expect(g.init.credentials).toBe("omit");
      expect(h["X-VBP-Nguon"]).toBe("app");
      expect(h["X-VBP-Phien-Ban"]).toMatch(/^app-/);
    }
  });

  test("làm mới: token trong thân, cất token mới, phiên phát ra KHÔNG chứa token làm mới", async () => {
    giaFetch(tl(200, { success: true, message: "Đã đăng nhập.", data: { ...phien("T2"), token_lam_moi: "TLM-moi" } }));
    const nghe = vi.fn();
    const go = khiDoiPhien(nghe);
    const p = await lamMoi();
    go();
    expect(daGoi[0].url).toBe("https://api.thu/api/v2/phien/lam-moi");
    expect(than(0)).toEqual({ token_lam_moi: "TLM-cu" });
    expect(gia.tokenHienTai()).toBe("TLM-moi");
    expect(p).toEqual(phien("T2"));
    expect(JSON.stringify(nghe.mock.calls)).not.toContain("TLM-moi");
  });

  test("máy chưa có token → không gọi mạng, về khách", async () => {
    gia = taoMayGia(null); datNenTangThu({ app: true, may: gia.may });
    const f = giaFetch();
    expect(await lamMoi()).toBeNull();
    expect(f).not.toHaveBeenCalled();
  });

  test("làm mới bị từ chối (401) → xóa token trong máy", async () => {
    giaFetch(tl(401, { success: false, code: "PHIEN_THU_HOI", message: "Phiên đăng nhập đã kết thúc. Đăng nhập lại." }));
    expect(await lamMoi()).toBeNull();
    await vi.waitFor(() => expect(gia.may.khoToken.xoa).toHaveBeenCalled());
    expect(gia.tokenHienTai()).toBeNull();
  });

  test("máy chủ bận (503) khi làm mới → GIỮ token (không đăng xuất vì lỗi tạm)", async () => {
    giaFetch(tl(503, { success: false, code: "LOI_MAY_CHU", message: "bận" }));
    await expect(lamMoi()).rejects.toBeInstanceOf(LoiApi);
    expect(gia.may.khoToken.xoa).not.toHaveBeenCalled();
    expect(gia.tokenHienTai()).toBe("TLM-cu");
  });

  test("đăng nhập mật khẩu: token làm mới vào kho máy, không tới tay giao diện", async () => {
    giaFetch(tl(200, { success: true, message: "Đã đăng nhập.", data: { ...phien("T3"), token_lam_moi: "TLM-dn" } }));
    expect(await api.dangNhap("a@b.vn", "mat-khau-dai")).toEqual({ loai: "phien", phien: phien("T3") });
    expect(gia.tokenHienTai()).toBe("TLM-dn");
  });

  test("Google + 2 lớp cũng cất token", async () => {
    giaFetch(
      tl(200, { success: true, message: "Đã đăng nhập.", data: { ...phien("T4"), token_lam_moi: "TLM-gg" } }),
      tl(200, { success: true, message: "Đã đăng nhập.", data: { ...phien("T5"), token_lam_moi: "TLM-2l" } }),
    );
    expect(await api.google("id")).toEqual({ loai: "phien", phien: phien("T4") });
    expect(gia.tokenHienTai()).toBe("TLM-gg");
    expect(await api.haiLop("mt", "123456")).toEqual(phien("T5"));
    expect(gia.tokenHienTai()).toBe("TLM-2l");
  });

  test("đăng xuất: gửi token làm mới trong thân rồi xóa khỏi máy", async () => {
    giaFetch(tl(200, { success: true, message: "Đã đăng xuất.", data: null }));
    datPhien(phien("T1"));
    expect(await api.dangXuat()).toBe(true);
    expect(than(0)).toEqual({ token_lam_moi: "TLM-cu" });
    await vi.waitFor(() => expect(gia.tokenHienTai()).toBeNull());
  });

  test("HET_DUNG_LUONG: câu báo không nhắc mua", async () => {
    giaFetch(tl(403, { success: false, code: "HET_DUNG_LUONG", message: "Đã dùng hết dung lượng lưu trữ. Kho đang chỉ đọc: mua thêm dung lượng hoặc xóa bớt tệp." }));
    datPhien(phien("T1"));
    await expect(goi("/api/v2/kho/van-ban", { phuongThuc: "POST", than: {} })).rejects.toMatchObject({
      code: "HET_DUNG_LUONG", message: "Kho đã đầy — đang ở chế độ chỉ đọc. Xóa bớt tệp để thêm mới.",
    });
  });
});

describe("bản web không đổi", () => {
  test("làm mới: thân {}, credentials include, không header nền tảng", async () => {
    datNenTangThu(null);
    giaFetch(tl(200, { success: true, message: "Đã đăng nhập.", data: phien("T9") }));
    await lamMoi();
    expect(than(0)).toEqual({});
    expect(daGoi[0].init.credentials).toBe("include");
    expect((daGoi[0].init.headers as Record<string, string>)["X-VBP-Nguon"]).toBeUndefined();
  });
});
```

- [ ] **Step 2: Chạy, thấy hỏng** — `npx vitest run src/api/khach-app.test.ts` → FAIL (thân `{}`, header `web`…).

- [ ] **Step 3: Sửa `src/api/khach.ts`**

```ts
import { chuLoiTrungTinh, dauNenTang, laApp, layMay } from "@/nen-tang";

export function datPhien(p: PhienTraVe | null) {
  // … thân cũ giữ nguyên …
  // App: phiên kết thúc (đăng xuất, 401, bị thu hồi) → bỏ token làm mới trong kho máy.
  if (!p && laApp()) void layMay().then((m) => m.khoToken.xoa()).catch(() => {});
}

/** Thân gửi /phien/lam-moi. Web: {} (token làm mới là cookie HttpOnly). App: token trong kho an toàn của máy;
 *  null = máy chưa có token → coi như chưa đăng nhập, khỏi gọi mạng. */
async function thanLamMoi(): Promise<Record<string, string> | null> {
  if (!laApp()) return {};
  const t = await (await layMay()).khoToken.doc();
  return t ? { token_lam_moi: t } : null;
}

/** App: tách token làm mới khỏi phiên trả về, cất vào kho an toàn (giao diện không bao giờ thấy). Web: giữ nguyên. */
async function catTokenLamMoi<T>(data: T): Promise<T> {
  if (!laApp() || !data || typeof data !== "object" || !("token_lam_moi" in data)) return data;
  const { token_lam_moi: t, ...con } = data as Record<string, unknown>;
  if (typeof t === "string" && t) await (await layMay()).khoToken.ghi(t);
  return con as T;
}

/** Thân gửi /phien/dang-xuat: app kèm token làm mới để máy chủ thu hồi đúng phiên; web: {} như cũ. */
export async function thanDangXuat(): Promise<Record<string, string>> {
  return (await thanLamMoi()) ?? {};
}
```
Trong `guiMot`: dòng tạo `dau` → `const dau: Record<string, string> = { ...(t.dau ?? {}), ...dauNenTang() };` (header nền tảng đè
`X-VBP-Nguon: web` của `tac-vu.ts`); `credentials: laApp() || thang ? "omit" : "include",`.

`guiLamMoi` nhận thân:

```ts
async function guiLamMoi(than: Record<string, string>) {
  try { return await guiTho("/api/v2/phien/lam-moi", { phuongThuc: "POST", than }, null); }
  catch (e) {
    if (!(e instanceof LoiApi && e.code === "MAT_MANG") || loiHetGio.has(e)) throw e;
    return guiTho("/api/v2/phien/lam-moi", { phuongThuc: "POST", than }, null);
  }
}

async function lamMoiMotLan(): Promise<PhienTraVe | null> {
  for (let lan = 0; lan < 2; lan++) {
    const than = await thanLamMoi(); // đọc lại mỗi vòng: LAM_MOI_TRUNG lần trước có thể đã xoay token
    if (than === null) break;
    const { r, j } = await guiLamMoi(than);
    if (r.ok && j.success && j.data) {
      const p = await catTokenLamMoi(j.data as PhienTraVe);
      datPhien(p);
      return p;
    }
    // … phần còn lại của vòng lặp giữ nguyên …
  }
  datPhien(null);
  return null;
}
```
Cuối `goi`:

```ts
  if (!r.ok || j.success === false) {
    throw new LoiApi(r.status, j.code ?? (r.status === 401 ? "CHUA_DANG_NHAP" : "LOI_MAY_CHU"),
      chuLoiTrungTinh(j.code ?? "", j.message || "Có lỗi. Thử lại sau."), j.data as Record<string, unknown> | undefined);
  }
  const data = duong.startsWith("/api/v2/phien/") ? await catTokenLamMoi(j.data) : j.data;
  return { data: data as T, code: j.code, message: j.message ?? "" };
```
Sửa chú thích đầu tệp: thêm câu «Bản app: không cookie; token làm mới gửi / nhận trong thân, cất ở Keychain / Keystore qua
`src/nen-tang.ts`.»

- [ ] **Step 4: Sửa `src/api/phien.ts`** — `import { datPhien, goi, LoiApi, thanDangXuat } from "./khach";` và trong `dangXuat`:
  `try { await P("/api/v2/phien/dang-xuat", await thanDangXuat()); }`.

- [ ] **Step 5: Chạy** — `npx vitest run src/api` → PASS (cả `khach.test.ts`, `dang-xuat.test.ts` cũ không sửa); `npm test`; `npx tsc -b`.
- [ ] **Step 6: Commit**

```bash
git add src/api/khach.ts src/api/phien.ts src/api/khach-app.test.ts
git commit -m "phien app: token lam moi trong than + kho an toan cua may, khong cookie, header X-VBP-Nguon app; web khong doi" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

### Task 6: Ẩn mọi lối vào Mua ở bản app

**Files:**
- Modify: `src/ung-dung.ts:14-30`, `src/App.tsx`, `src/phan-he/ai/ThanhPhanTacVu.tsx:17,53`, `src/phan-he/ai/thong-bao-loi.ts:13`,
  `src/phan-he/van-ban/BieuMau.tsx:22,62`, `src/phan-he/van-ban/DaiChiDoc.tsx:56`, `src/phan-he/van-ban/TaiLenTep.tsx:58`,
  `src/phan-he/album/BangTaiLen.tsx:29`, `src/phan-he/lich-hop/DaiChiDoc.tsx:37,45`, `src/phan-he/mau/loi-mau.tsx:12`,
  `src/phan-he/tai-khoan/ThanhDungLuong.tsx:29`, `src/phan-he/tai-khoan/TongQuan.tsx:15,43,77`
- Test: `src/an-mua.test.tsx`

**Interfaces:**
- Consumes: `coTheMua()` (Task 4).
- Produces: `export function locPhanHe(ds: PhanHe[], coMua: boolean): PhanHe[]` (`src/ung-dung.ts`);
  `export function tuyenChanMua(): JSX.Element | null` (`src/App.tsx`).

- [ ] **Step 1: Bài kiểm hỏng** `src/an-mua.test.tsx`

```tsx
import { readdirSync, readFileSync } from "node:fs";
import { join } from "node:path";
import type { ReactNode } from "react";
import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router";
import { afterEach, describe, expect, test } from "vitest";
import { LoiApi } from "@/api/khach";
import { tuyenChanMua } from "@/App";
import { datNenTangThu } from "@/nen-tang";
import { BaoLoiTacVu, NutTacVu } from "@/phan-he/ai/ThanhPhanTacVu";
import { loiTacVu } from "@/phan-he/ai/thong-bao-loi";
import { DaiChiDoc as DaiLichHop, loiHetDungLuong } from "@/phan-he/lich-hop/DaiChiDoc";
import { loiMau } from "@/phan-he/mau/loi-mau";
import { DaiChiDoc as DaiVanBan } from "@/phan-he/van-ban/DaiChiDoc";
import { taoMayGia } from "@/test/may-gia";
import { locPhanHe, PHAN_HE } from "@/ung-dung";

const app = () => datNenTangThu({ app: true, may: taoMayGia().may });
afterEach(() => datNenTangThu(null));
const boc = (n: ReactNode) => render(<MemoryRouter>{n}</MemoryRouter>);
const UOC_THIEU = { credit_min: 3, credit_max: 5, so_du: 1, du_credit: false };
const KHONG_CREDIT = new LoiApi(402, "KHONG_DU_CREDIT", "Không đủ credit: tác vụ cần tối đa 5, số dư còn 1.");
const HET_KHO = new LoiApi(403, "HET_DUNG_LUONG", "Kho đã đầy — đang ở chế độ chỉ đọc. Xóa bớt tệp để thêm mới.");
function moDuong(d: string) {
  render(
    <MemoryRouter initialEntries={[d]}>
      <Routes>{tuyenChanMua()}<Route path="/" element={<p>màn hình chính</p>} /><Route path="*" element={<p>trang khác</p>} /></Routes>
    </MemoryRouter>,
  );
}

test("nguồn: mọi tệp dẫn tới trang Mua đều đi qua coTheMua() (chặn lối Mua mới quên ẩn ở app)", () => {
  const BO_QUA = [/\.test\.tsx?$/, /[\\/]phan-he[\\/]mua[\\/]/, /[\\/]api[\\/](mua|kho)\.ts$/, /[\\/]os-kit[\\/]/, /[\\/]test[\\/]/];
  const DAU_MUA = /MUA_CREDIT|MUA_DUNG_LUONG|MUA_THEM_DUNG_LUONG|BANG_GIA|duongMua\(|["'`]\/mua\b/;
  const ds = (readdirSync("src", { recursive: true }) as string[]).map((p) => join("src", p))
    .filter((p) => /\.tsx?$/.test(p) && !BO_QUA.some((r) => r.test(p)));
  const thieu = ds.filter((p) => { const s = readFileSync(p, "utf8"); return DAU_MUA.test(s) && !s.includes("coTheMua()"); });
  expect(thieu).toEqual([]);
});

describe("bản app: không còn lối vào Mua", () => {
  test("danh sách phân hệ bỏ «Mua thêm»", () => {
    expect(locPhanHe(PHAN_HE, false).map((p) => p.ma)).not.toContain("mua");
    expect(locPhanHe(PHAN_HE, true)).toEqual(PHAN_HE);
  });
  test("/mua?loai=credit chuyển về màn hình chính", () => {
    app(); moDuong("/mua?loai=credit");
    expect(screen.getByText("màn hình chính")).toBeInTheDocument();
  });
  test("thiếu credit: còn số dư, không liên kết mua", () => {
    app(); boc(<NutTacVu nhan="Tóm tắt" uoc={UOC_THIEU} dangChay={false} onBam={() => {}} />);
    expect(screen.getByText(/credit chưa đủ/)).toBeInTheDocument();
    expect(screen.queryByRole("link")).toBeNull();
  });
  test("báo lỗi KHONG_DU_CREDIT: không nút mua", () => {
    app(); boc(<BaoLoiTacVu loi={loiTacVu(KHONG_CREDIT)} />);
    expect(screen.getByRole("alert")).toHaveTextContent("Không đủ credit");
    expect(screen.queryByRole("link")).toBeNull();
  });
  test("dải chỉ đọc (văn bản, lịch họp), lỗi hết dung lượng, lỗi mẫu: không liên kết", () => {
    app();
    boc(<><DaiVanBan chiDoc /><DaiLichHop chiDoc /><p>{loiHetDungLuong(HET_KHO)}</p><p>{loiMau(HET_KHO)}</p></>);
    expect(screen.getAllByRole("alert")).toHaveLength(2);
    expect(screen.queryByRole("link")).toBeNull();
    expect(document.body.textContent).not.toMatch(/mua/i);
  });
});

describe("bản web giữ nguyên", () => {
  test("thiếu credit có «Mua thêm credit» tới /mua?loai=credit", () => {
    boc(<NutTacVu nhan="Tóm tắt" uoc={UOC_THIEU} dangChay={false} onBam={() => {}} />);
    expect(screen.getByRole("link", { name: "Mua thêm credit" })).toHaveAttribute("href", "/mua?loai=credit");
  });
  test("/mua không bị chuyển hướng", () => {
    moDuong("/mua");
    expect(screen.getByText("trang khác")).toBeInTheDocument();
  });
  test("dải chỉ đọc văn bản có liên kết mua dung lượng", () => {
    boc(<DaiVanBan chiDoc />);
    expect(screen.getByRole("link", { name: "Mua thêm dung lượng" })).toBeInTheDocument();
  });
});
```

- [ ] **Step 2: Chạy, thấy hỏng** — `npx vitest run src/an-mua.test.tsx` → FAIL (`locPhanHe`, `tuyenChanMua` chưa có; bài nguồn liệt kê 12 tệp).

- [ ] **Step 3: Sửa từng chỗ** (mỗi tệp thêm `import { coTheMua } from "@/nen-tang";`)

`src/ung-dung.ts`: đổi `export const PHAN_HE: PhanHe[] = [` thành `const TAT_CA_PHAN_HE: PhanHe[] = [` (giữ nguyên 15 dòng), thêm sau mảng:

```ts
/** Bản app không bán hàng (spec §7): bỏ phân hệ «Mua thêm». */
export function locPhanHe(ds: PhanHe[], coMua: boolean): PhanHe[] {
  return coMua ? ds : ds.filter((p) => p.ma !== "mua");
}
export const PHAN_HE: PhanHe[] = locPhanHe(TAT_CA_PHAN_HE, coTheMua());
```
(`ung-dung.test.ts` cũ vẫn thấy 15 phân hệ vì chạy chế độ web.)

`src/App.tsx`: `import { Navigate, Route, Routes } from "react-router";`, `import { coTheMua } from "@/nen-tang";`, thêm hàm và đổi `Routes`:

```tsx
/** Bản app không bán hàng (spec §7): mọi đường /mua* về màn hình chính. Bản web: không có tuyến này. */
export function tuyenChanMua() {
  return coTheMua() ? null : <Route path="/mua/*" element={<Navigate to="/" replace />} />;
}
// trong <Routes>, ngay trước PHAN_HE.map:
        {tuyenChanMua()}
        {PHAN_HE.filter((p) => p.trang && (coTheMua() || p.ma !== "mua")).map((p) => {
```

`src/phan-he/ai/ThanhPhanTacVu.tsx`:

```tsx
// dòng 17
      {u && thieu && <small className="chu-phu">Số dư {dinhDangSo(u.so_du)} credit chưa đủ.{coTheMua() && <> <a className="lien-ket" href={MUA_CREDIT} target="_blank" rel="noopener noreferrer">Mua thêm credit</a></>}</small>}
// dòng 53
      {loi.muaThem && coTheMua() && <a className="nut nut--nho" href={loi.muaThem} target="_blank" rel="noopener noreferrer">Mua thêm credit</a>}
```
`src/phan-he/ai/thong-bao-loi.ts:13`: `case "KHONG_DU_CREDIT": return coTheMua() ? { thongBao: e.message, muaThem: MUA_CREDIT } : { thongBao: e.message };`

`src/phan-he/van-ban/BieuMau.tsx:22`: thay hằng `MUA` bằng hàm (tính lúc vẽ):
`const MUA = () => (coTheMua() ? <a className="lien-ket" href={MUA_DUNG_LUONG} target="_blank" rel="noopener noreferrer">Mua thêm dung lượng</a> : null);`
và dòng 62 `{MUA}` → `{MUA()}`.

`src/phan-he/van-ban/DaiChiDoc.tsx:56`: `{coTheMua() && <a className="lien-ket" href={MUA_DUNG_LUONG} target="_blank" rel="noopener noreferrer">Mua thêm dung lượng</a>}`

`src/phan-he/van-ban/TaiLenTep.tsx:58`:

```ts
capNhat(key, { tt: "loi", loi: coTheMua() ? "Không đủ dung lượng cho tệp này. Xóa bớt tệp hoặc mua thêm dung lượng." : "Không đủ dung lượng cho tệp này. Xóa bớt tệp để tải lên.", muaThem: coTheMua() });
```
(dòng 108 không đổi: `muaThem` là false ở app.)

`src/phan-he/album/BangTaiLen.tsx:29`: `Đã dùng hết dung lượng lưu trữ: các ảnh còn lại chưa tải lên.{coTheMua() && <> <Link className="lien-ket" to={MUA_THEM_DUNG_LUONG}>Mua thêm dung lượng</Link></>}`

`src/phan-he/lich-hop/DaiChiDoc.tsx`: dòng 37 `{coTheMua() && <Link className="lien-ket" to={MUA_THEM_DUNG_LUONG}>Mua thêm dung lượng</Link>}`;
dòng 45 `return <>{e.message}{coTheMua() && <> <Link className="lien-ket" to={MUA_THEM_DUNG_LUONG}>Mua thêm dung lượng</Link></>}</>;`

`src/phan-he/mau/loi-mau.tsx:11-12`: `<>Kho đang chỉ đọc vì đã dùng hết dung lượng nên chưa tạo được mẫu mới. Mẫu có sẵn vẫn xem và dùng được.{coTheMua() && <>{" "}<Link className="lien-ket" to={MUA_DUNG_LUONG}>Mua thêm dung lượng</Link></>}</>`

`src/phan-he/tai-khoan/ThanhDungLuong.tsx:29`: `{coTheMua() && <Link className="lien-ket" to={MUA_DUNG_LUONG}>Mua thêm dung lượng</Link>}`

`src/phan-he/tai-khoan/TongQuan.tsx`:

```tsx
// dòng 15 — nhãn lô credit không nhắc mua ở app
const NGUON: Record<string, string> = { goi_thang: "Gói tháng", nap: coTheMua() ? "Mua thêm" : "Nạp thêm", tang: "Tặng", dieu_chinh: "Điều chỉnh" };
// dòng 42-44
      {coTheMua() && <div className="hang-nut the__nut"><Link className="nut nut--chinh" to={BANG_GIA}>Gia hạn / Nâng gói</Link></div>}
// dòng 76-78
      {coTheMua() && <div className="hang-nut the__nut"><Link className="nut" to={MUA_CREDIT}>Mua thêm credit</Link></div>}
```

- [ ] **Step 4: Chạy** — `npx vitest run src/an-mua.test.tsx` → PASS; `npm test` (bài cũ của các phân hệ trên không sửa, vẫn xanh vì
  chạy chế độ web); `npx tsc -b`; soát chữ mới bằng lệnh grep ở Global Constraints trên các tệp vừa sửa.
- [ ] **Step 5: Commit**

```bash
git add src/an-mua.test.tsx src/ung-dung.ts src/App.tsx src/phan-he
git commit -m "app: an moi loi vao Mua (phan he, tuyen /mua, lien ket, cau bao trung tinh) qua coTheMua(); bai kiem nguon chan loi Mua moi" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

### Task 7: Chỗ hỏng trong WebView — Word, mã dự phòng, mở tệp, liên kết ngoài

**Files:**
- Modify: `src/phan-he/soan-thao/xuat-word/dung-docx.ts:107-114`, `src/phan-he/tai-khoan/BatHaiLop.tsx:36-45`,
  `src/phan-he/lich-hop/KhoiTep.tsx:8-18,55`, `src/phan-he/album/ChiTietAlbum.tsx:193`, `src/phan-he/van-ban/DanhSachTep.tsx:64-69`,
  `src/app-di-dong/khoi-dong.ts`
- Create: `src/app-di-dong/lien-ket-ngoai.ts`
- Test: `src/app-di-dong/lien-ket-ngoai.test.ts`, `src/phan-he/mo-tep-app.test.tsx`, `src/phan-he/soan-thao/xuat-word/xuat-word-app.test.ts`

**Interfaces:**
- Consumes: `luuTep`, `moTep`, `moNgoai`, `laApp` (Task 4).
- Produces: `export function chanLienKetNgoai(goc: Document, mo: (url: string) => void): () => void`;
  `KhoiTep.moTep(t: { id: string; ten: string; mime: string }): Promise<void>` (đổi chữ ký từ `moTep(id)`).

- [ ] **Step 1: Bài kiểm hỏng**

`src/app-di-dong/lien-ket-ngoai.test.ts`:

```ts
import { readdirSync, readFileSync } from "node:fs";
import { join } from "node:path";
import { afterEach, expect, test, vi } from "vitest";
import { chanLienKetNgoai } from "./lien-ket-ngoai";

let go: (() => void) | null = null;
afterEach(() => { go?.(); go = null; document.body.innerHTML = ""; });
function bam(href: string) {
  const a = document.createElement("a");
  a.setAttribute("href", href);
  a.innerHTML = "<span>chữ</span>";
  document.body.appendChild(a);
  const ev = new MouseEvent("click", { bubbles: true, cancelable: true, button: 0 });
  a.querySelector("span")!.dispatchEvent(ev);
  return ev;
}

test("liên kết http(s) khác nguồn → mở bằng trình duyệt trong app, WebView không tự điều hướng", () => {
  const mo = vi.fn();
  go = chanLienKetNgoai(document, mo);
  const ev = bam("https://vanbanplus.giakiemso.com/dieu-khoan");
  expect(ev.defaultPrevented).toBe(true);
  expect(mo).toHaveBeenCalledWith("https://vanbanplus.giakiemso.com/dieu-khoan");
});

test("liên kết trong app, mailto:, tel: → để nguyên", () => {
  const mo = vi.fn();
  go = chanLienKetNgoai(document, mo);
  for (const h of ["/van-ban", `${location.origin}/tai-khoan`, "mailto:hotro@giakiemso.com", "tel:0900000000"]) {
    expect(bam(h).defaultPrevented).toBe(false);
  }
  expect(mo).not.toHaveBeenCalled();
});

test("gỡ bộ chặn → không chặn nữa", () => {
  const mo = vi.fn();
  chanLienKetNgoai(document, mo)();
  bam("https://meet.google.com/abc");
  expect(mo).not.toHaveBeenCalled();
});

test("nguồn: chỉ src/nen-tang.ts còn a.download / window.open (mọi chỗ khác đi qua nen-tang)", () => {
  const ds = (readdirSync("src", { recursive: true }) as string[]).map((p) => join("src", p))
    .filter((p) => /\.tsx?$/.test(p) && !/\.test\.tsx?$/.test(p) && !/[\\/]os-kit[\\/]/.test(p) && !/nen-tang\.ts$/.test(p));
  const sai = ds.filter((p) => /\.download\s*=|window\.open\(/.test(readFileSync(p, "utf8")));
  expect(sai).toEqual([]);
});
```

`src/phan-he/mo-tep-app.test.tsx`:

```tsx
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, expect, test, vi } from "vitest";
import type { Tep } from "@/api/kho";
import { datNenTangThu } from "@/nen-tang";
import { taoMayGia } from "@/test/may-gia";
import { DanhSachTep } from "@/phan-he/van-ban/DanhSachTep";
import { KhoiTep } from "@/phan-he/lich-hop/KhoiTep";

const URL_KY = "https://api-vanbanplus.giakiemso.com/storage/v1/object/sign/kho/a.pdf?token=gia";
vi.mock("@/api/kho", async (goc) => ({
  ...(await goc<typeof import("@/api/kho")>()),
  kho: { taiVe: vi.fn(async () => ({ url: URL_KY, het_han: "" })), xoaTep: vi.fn(async () => {}), taiTepLen: vi.fn() },
}));
const PDF: Tep = { id: "t1", ten: "Cong-van.pdf", mime: "application/pdf", byte: 1000, so_trang: 1, co_anh_nho: false, tao_luc: "2026-11-19T00:00:00Z" };
afterEach(() => datNenTangThu(null));

test("app: «Xem» PDF tải về rồi mở bằng ứng dụng của máy, không nhúng khung", async () => {
  const gia = taoMayGia(); datNenTangThu({ app: true, may: gia.may });
  render(<DanhSachTep vanBanId="v1" tep={[PDF]} chiDoc={false} onDoi={() => {}} />);
  await userEvent.click(screen.getByRole("button", { name: "Xem Cong-van.pdf" }));
  await waitFor(() => expect(gia.may.taiVaMo).toHaveBeenCalledWith(URL_KY, "Cong-van.pdf", "application/pdf"));
  expect(document.querySelector("iframe")).toBeNull();
});

test("app: «Tải về» cũng mở bằng máy, không hiện liên kết _blank chứa URL ký", async () => {
  const gia = taoMayGia(); datNenTangThu({ app: true, may: gia.may });
  render(<DanhSachTep vanBanId="v1" tep={[PDF]} chiDoc={false} onDoi={() => {}} />);
  await userEvent.click(screen.getByRole("button", { name: "Tải về Cong-van.pdf" }));
  await waitFor(() => expect(gia.may.taiVaMo).toHaveBeenCalledTimes(1));
  expect(screen.queryByRole("link", { name: "Mở / tải tệp" })).toBeNull();
});

test("app: tệp lịch họp mở bằng máy", async () => {
  const gia = taoMayGia(); datNenTangThu({ app: true, may: gia.may });
  render(<KhoiTep gan={{ loai: "cuoc_hop", id: "h1" }} tep={[PDF]} khiDoi={async () => {}} chiDoc={false} />);
  await userEvent.click(screen.getByRole("button", { name: "Cong-van.pdf" }));
  await waitFor(() => expect(gia.may.taiVaMo).toHaveBeenCalledWith(URL_KY, "Cong-van.pdf", "application/pdf"));
});

test("web: «Xem» PDF vẫn nhúng khung như cũ", async () => {
  render(<DanhSachTep vanBanId="v1" tep={[PDF]} chiDoc={false} onDoi={() => {}} />);
  await userEvent.click(screen.getByRole("button", { name: "Xem Cong-van.pdf" }));
  await waitFor(() => expect(document.querySelector("iframe")).not.toBeNull());
});
```

`src/phan-he/soan-thao/xuat-word/xuat-word-app.test.ts`:

```ts
// @vitest-environment node
import { afterEach, expect, test } from "vitest";
import { datNenTangThu } from "@/nen-tang";
import { taoMayGia } from "@/test/may-gia";
import { phanTichVanBan } from "./bo-cuc";
import { xuatWord } from "./dung-docx";

afterEach(() => datNenTangThu(null));

test("app: Xuất Word ghi tệp .docx rồi mở bảng Chia sẻ", async () => {
  const gia = taoMayGia(); datNenTangThu({ app: true, may: gia.may });
  await xuatWord(phanTichVanBan("QUYẾT ĐỊNH\nVề việc thử\nĐiều 1. Thử app."), "Thu.docx");
  const [blob, ten] = gia.may.ghiVaChiaSe.mock.calls[0];
  expect(ten).toBe("Thu.docx");
  expect(blob.size).toBeGreaterThan(3000);
});
```

- [ ] **Step 2: Chạy, thấy hỏng** — `npx vitest run src/app-di-dong/lien-ket-ngoai.test.ts src/phan-he/mo-tep-app.test.tsx src/phan-he/soan-thao/xuat-word/xuat-word-app.test.ts` → FAIL.

- [ ] **Step 3: `src/app-di-dong/lien-ket-ngoai.ts`**

```ts
/** Bản app: bấm liên kết http(s) ra ngoài (điều khoản, link họp trực tuyến, link trong os-kit…) → mở bằng trình duyệt trong app
 *  (plugin Browser, tách khỏi WebView có cầu nối Capacitor); WebView không tự điều hướng. mailto: / tel: để hệ thống lo.
 *  Chặn ở pha capture trên document nên phủ cả mã không sửa được (src/os-kit/). */
export function chanLienKetNgoai(goc: Document, mo: (url: string) => void): () => void {
  const xuLy = (e: MouseEvent) => {
    if (e.defaultPrevented || e.button !== 0) return;
    const a = (e.target as Element | null)?.closest?.("a[href]") as HTMLAnchorElement | null;
    if (!a) return;
    let u: URL;
    try { u = new URL(a.getAttribute("href")!, goc.baseURI); } catch { return; }
    if (u.protocol !== "https:" && u.protocol !== "http:") return;
    if (u.origin === new URL(goc.baseURI).origin) return;
    e.preventDefault();
    mo(u.href);
  };
  goc.addEventListener("click", xuLy, true);
  return () => goc.removeEventListener("click", xuLy, true);
}
```

`src/app-di-dong/khoi-dong.ts` (toàn bộ):

```ts
import { moNgoai } from "@/nen-tang";
import { chanLienKetNgoai } from "./lien-ket-ngoai";
import { khoiTaoMay } from "./may";

/** Chạy một lần trước khi vẽ giao diện (src/main.tsx, chỉ bản app). */
export async function khoiDong(): Promise<void> {
  chanLienKetNgoai(document, (url) => void moNgoai(url));
  await khoiTaoMay();
}
```

- [ ] **Step 4: Sửa nơi dùng**

`dung-docx.ts` — bỏ thân cũ của `xuatWord`, thêm `import { luuTep } from "@/nen-tang";`:

```ts
/** Web: tải xuống. App: ghi tệp + bảng Chia sẻ (src/nen-tang.ts). */
export async function xuatWord(bc: BoCuc, tenTep: string): Promise<void> {
  await luuTep(await Packer.toBlob(dungTaiLieu(bc)), tenTep);
}
```
`BatHaiLop.tsx` — thay `taiTep`:

```ts
/** Tải mã dự phòng thành tệp .txt (web: tải xuống, Blob URL thu hồi sau 1 giây; app: bảng Chia sẻ). */
function taiTep(maDu: string[]) {
  void luuTep(new Blob([[DONG_DAU, ...maDu].join("\n") + "\n"], { type: "text/plain;charset=utf-8" }), TEP);
}
```
`KhoiTep.tsx` — thay hàm `moTep` (dòng 8-18) và lời gọi dòng 55:

```ts
import { moTep as moTepNenTang } from "@/nen-tang";
/** Mở tệp qua đường ký 10 phút: web mở thẻ mới (mở trước khi chờ API để trình chặn cửa sổ bật lên không chặn);
 *  app tải về bộ nhớ tạm rồi mở bằng ứng dụng xem của máy. */
export function moTep(t: { id: string; ten: string; mime: string }): Promise<void> {
  return moTepNenTang(async () => (await kho.taiVe(t.id)).url, t.ten, t.mime);
}
// dòng 55: onClick={() => void moTep(t).catch((e) => setLoi(thongBaoLoi(e)))}
```
`ChiTietAlbum.tsx:193`: `khiTaiGoc={(a) => void moTep(a).catch((e) => setLoi(thongBaoLoi(e)))}` (`Anh` có `id`, `ten`, `mime`).

`DanhSachTep.tsx` — thêm `import { laApp, moTep as moTepNenTang } from "@/nen-tang";`, thay hai dòng `moXem` / `taiVe`:

```tsx
  // Bản app: WebView không hiện PDF trong khung và không mở thẻ mới → tải về bộ nhớ tạm, mở bằng ứng dụng xem của máy.
  async function moBangMay(t: Tep) {
    setDang(t.id); setLoi((c) => ({ ...c, [t.id]: "" }));
    try { await moTepNenTang(async () => (await kho.taiVe(t.id)).url, t.ten, t.mime); }
    catch (e) { setLoi((c) => ({ ...c, [t.id]: lyDo(e) })); }
    finally { setDang(null); }
  }
  const moXem = (t: Tep) => (laApp() && t.mime === "application/pdf" ? void moBangMay(t)
    : xem?.id === t.id ? setXem(null) : void lay(t, (url) => setXem({ id: t.id, url })));
  const taiVe = (t: Tep) => (laApp() ? void moBangMay(t) : void lay(t, (url) => setTai((c) => ({ ...c, [t.id]: url }))));
```

- [ ] **Step 5: Chạy** — ba tệp kiểm mới PASS; `npm test` (gồm `dung-docx.test.ts`, `chi-tiet.test.tsx`, `album.test.tsx`,
  `cuoc-hop.test.tsx` cũ không sửa); `npx tsc -b`.
- [ ] **Step 6: Commit**

```bash
git add src/app-di-dong src/phan-he src/nen-tang.ts
git commit -m "app: Xuat Word / ma du phong qua bang Chia se, PDF va tep mo bang ung dung cua may, lien ket ngoai mo trinh duyet trong app; web khong doi" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

### Task 8: Đăng nhập Google native trong app

**Files:**
- Modify: `src/dang-nhap/NutGoogle.tsx`, `src/giao-dien/goc.css:205`
- Test: `src/dang-nhap/google-app.test.tsx`

**Interfaces:**
- Consumes: `laApp`, `layMay().googleIdToken()` (Task 4); `api.google(idToken)` (có sẵn, Task 5 cất token).
- Produces: `NutGoogle` giữ nguyên props `{ onToken(idToken: string): void; chu?: "signin_with" | "signup_with" | "continue_with" }`.

- [ ] **Step 1: Bài kiểm hỏng** `src/dang-nhap/google-app.test.tsx`

```tsx
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, expect, test, vi } from "vitest";
import { taoMayGia } from "@/test/may-gia";

afterEach(() => { vi.unstubAllEnvs(); delete window.google; });

async function napApp() {
  vi.stubEnv("VITE_GOOGLE_CLIENT_ID", "web-1.apps.googleusercontent.com");
  vi.resetModules();
  const nt = await import("@/nen-tang");
  const gia = taoMayGia();
  nt.datNenTangThu({ app: true, may: gia.may });
  const { NutGoogle } = await import("./NutGoogle");
  return { NutGoogle, gia, nt };
}

test("app: nút native, bấm → id_token tới onToken; không nạp thư viện GIS", async () => {
  const { NutGoogle, gia, nt } = await napApp();
  const onToken = vi.fn();
  render(<NutGoogle chu="signup_with" onToken={onToken} />);
  await userEvent.click(screen.getByRole("button", { name: "Đăng ký bằng Google" }));
  await waitFor(() => expect(onToken).toHaveBeenCalledWith("id-token-gia"));
  expect(gia.may.googleIdToken).toHaveBeenCalledTimes(1);
  expect(document.querySelector('script[src*="accounts.google.com/gsi"]')).toBeNull();
  nt.datNenTangThu(null);
});

test("app: đóng hộp chọn tài khoản → không gọi onToken, không báo lỗi", async () => {
  const { NutGoogle, gia, nt } = await napApp();
  gia.may.googleIdToken.mockResolvedValueOnce(null);
  const onToken = vi.fn();
  render(<NutGoogle onToken={onToken} />);
  await userEvent.click(screen.getByRole("button", { name: "Tiếp tục với Google" }));
  await waitFor(() => expect(gia.may.googleIdToken).toHaveBeenCalled());
  expect(onToken).not.toHaveBeenCalled();
  expect(screen.queryByRole("alert")).toBeNull();
  nt.datNenTangThu(null);
});

test("app: lỗi plugin (thiếu SHA-1, không có Play services) → câu báo, gợi ý đăng nhập mật khẩu", async () => {
  const { NutGoogle, gia, nt } = await napApp();
  gia.may.googleIdToken.mockRejectedValueOnce(new Error("[28444] Developer console is not set up correctly"));
  render(<NutGoogle onToken={vi.fn()} />);
  await userEvent.click(screen.getByRole("button", { name: "Tiếp tục với Google" }));
  expect(await screen.findByRole("alert")).toHaveTextContent("Chưa đăng nhập được bằng Google trên máy này. Dùng email và mật khẩu, hoặc thử lại sau.");
  nt.datNenTangThu(null);
});
```

- [ ] **Step 2: Chạy, thấy hỏng** — `npx vitest run src/dang-nhap/google-app.test.tsx` → FAIL (không có nút).

- [ ] **Step 3: Sửa `NutGoogle.tsx`** — đổi tên hàm hiện có `export function NutGoogle` thành `function NutGoogleWeb` (thân giữ
  nguyên), thêm `import { useState } from "react";` vào dòng import React có sẵn và `import { laApp, layMay } from "@/nen-tang";`, rồi thêm:

```tsx
type Props = { onToken(idToken: string): void; chu?: "signin_with" | "signup_with" | "continue_with" };
const CHU_APP = { signin_with: "Đăng nhập bằng Google", signup_with: "Đăng ký bằng Google", continue_with: "Tiếp tục với Google" } as const;

/** Web: nút Google Identity Services. App: Google chặn OAuth trong WebView → đăng nhập native lấy id_token (plugin). */
export function NutGoogle(p: Props) {
  return laApp() ? <NutGoogleApp {...p} /> : <NutGoogleWeb {...p} />;
}

function NutGoogleApp({ onToken, chu = "continue_with" }: Props) {
  const [loi, setLoi] = useState<string | null>(null);
  const [dang, setDang] = useState(false);
  if (!ID) return null;
  const bam = async () => {
    setLoi(null); setDang(true);
    try {
      const t = await (await layMay()).googleIdToken();
      if (t) onToken(t);
    } catch {
      setLoi("Chưa đăng nhập được bằng Google trên máy này. Dùng email và mật khẩu, hoặc thử lại sau.");
    } finally {
      setDang(false);
    }
  };
  return (
    <>
      <button type="button" className="nut nut-google-app" disabled={dang} aria-busy={dang} onClick={() => void bam()}>{CHU_APP[chu]}</button>
      {loi && <p className="hop-loi" role="alert">{loi}</p>}
      <div className="gach-hoac"><span>hoặc</span></div>
    </>
  );
}
```
`NutGoogleWeb` dùng `Props` cho tham số. `goc.css` sau dòng 205 thêm
`.nut-google-app { display: block; width: 100%; max-width: 400px; min-height: 44px; margin: 0 auto; }`.

- [ ] **Step 4: Chạy** — bài mới PASS; `src/dang-nhap/thanh-phan.test.tsx` (bài GIS cũ) PASS không sửa; `npm test`; `npx tsc -b`.
- [ ] **Step 5: Commit**

```bash
git add src/dang-nhap/NutGoogle.tsx src/dang-nhap/google-app.test.tsx src/giao-dien/goc.css
git commit -m "app: nut Google native (id_token qua plugin), web giu GIS" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

### Task 9: «Chụp ảnh» của Quét mở trình quét tài liệu

**Files:**
- Modify: `src/phan-he/quet/ChonTep.tsx`
- Test: `src/phan-he/quet/quet-app.test.tsx`

**Interfaces:**
- Consumes: `laApp`, `layMay().quetTaiLieu(toiDa)` (Task 4); `TOI_DA_TEP` (`./chuan-bi-tep`). Ảnh đi vào `onThem(files)` như tệp
  chọn tay → `useTepQuet` (ước tính → đọc → soát) giữ nguyên.

- [ ] **Step 1: Bài kiểm hỏng** `src/phan-he/quet/quet-app.test.tsx`

```tsx
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, expect, test, vi } from "vitest";
import { datNenTangThu } from "@/nen-tang";
import { taoMayGia } from "@/test/may-gia";
import { ChonTep } from "./ChonTep";

afterEach(() => datNenTangThu(null));
const mo = (onThem = vi.fn()) => { render(<ChonTep ds={[]} khoa={false} onThem={onThem} onXoa={() => {}} onDoi={() => {}} />); return onThem; };

test("app: «Chụp ảnh» mở trình quét, 3 trang JPEG vào danh sách như tệp chọn tay", async () => {
  const gia = taoMayGia(); datNenTangThu({ app: true, may: gia.may });
  const ba = [1, 2, 3].map((i) => new File(["x"], `trang-0${i}.jpg`, { type: "image/jpeg" }));
  gia.may.quetTaiLieu.mockResolvedValueOnce({ loai: "anh", tep: ba });
  const onThem = mo();
  await userEvent.click(screen.getByRole("button", { name: "Chụp ảnh" }));
  await waitFor(() => expect(onThem).toHaveBeenCalledWith(ba));
  expect(gia.may.quetTaiLieu).toHaveBeenCalledWith(20);
});

test("app: hủy quét → không thêm gì, không báo lỗi", async () => {
  const gia = taoMayGia(); datNenTangThu({ app: true, may: gia.may });
  const onThem = mo();
  await userEvent.click(screen.getByRole("button", { name: "Chụp ảnh" }));
  await waitFor(() => expect(gia.may.quetTaiLieu).toHaveBeenCalled());
  expect(onThem).not.toHaveBeenCalled();
  expect(screen.queryByRole("alert")).toBeNull();
});

test("app: từ chối quyền máy ảnh → nhắc chọn ảnh có sẵn", async () => {
  const gia = taoMayGia(); datNenTangThu({ app: true, may: gia.may });
  gia.may.quetTaiLieu.mockResolvedValueOnce({ loai: "loi", thongBao: "Chưa có quyền dùng máy ảnh. Cấp quyền trong Cài đặt của máy, hoặc bấm «Chọn tệp» để chọn ảnh có sẵn." });
  mo();
  await userEvent.click(screen.getByRole("button", { name: "Chụp ảnh" }));
  expect(await screen.findByRole("alert")).toHaveTextContent("«Chọn tệp»");
});

test("web máy tính (không cảm ứng): không có nút «Chụp ảnh» như cũ", () => {
  mo();
  expect(screen.queryByRole("button", { name: "Chụp ảnh" })).toBeNull();
});
```

- [ ] **Step 2: Chạy, thấy hỏng** — `npx vitest run src/phan-he/quet/quet-app.test.tsx` → FAIL.

- [ ] **Step 3: Sửa `ChonTep.tsx`** — thêm `import { laApp, layMay } from "@/nen-tang";`, `import { TOI_DA_TEP } from "./chuan-bi-tep";`.
  Trong thân hàm, sau `const camUng = laCamUng();`:

```tsx
  const app = laApp();
  const [loiQuet, setLoiQuet] = useState<string | null>(null);
  const [dangQuet, setDangQuet] = useState(false);
  /** Bản app: trình quét tài liệu của máy (ML Kit / VisionKit): tìm mép, cắt góc, nhiều trang, trả JPEG. */
  const quet = async () => {
    setLoiQuet(null); setDangQuet(true);
    try {
      const kq = await (await layMay()).quetTaiLieu(Math.max(1, TOI_DA_TEP - ds.length));
      if (kq.loai === "anh") onThem(kq.tep);
      else if (kq.loai === "loi") setLoiQuet(kq.thongBao);
    } catch {
      setLoiQuet("Máy này chưa mở được trình quét tài liệu. Bấm «Chọn tệp» để chọn ảnh có sẵn.");
    } finally {
      setDangQuet(false);
    }
  };
```
Đổi dòng nút «Chụp ảnh» (dòng 28) và thêm hộp lỗi ngay sau `</div>` của `quet-nut-lon`:

```tsx
          {(camUng || app) && <button type="button" className="nut nut--chinh quet-nut" disabled={khoa || dangQuet} onClick={app ? () => void quet() : () => oChup.current?.click()}>Chụp ảnh</button>}
          <button type="button" className={"nut quet-nut" + (camUng || app ? "" : " nut--chinh")} disabled={khoa} onClick={() => oTep.current?.click()}>Chọn tệp</button>
        </div>
        {loiQuet && <p className="hop-loi" role="alert">{loiQuet}</p>}
```

- [ ] **Step 4: Chạy** — bài mới PASS; `src/phan-he/quet/quet.test.tsx` cũ PASS không sửa; `npm test`; `npx tsc -b`.
- [ ] **Step 5: Commit**

```bash
git add src/phan-he/quet/ChonTep.tsx src/phan-he/quet/quet-app.test.tsx
git commit -m "app: Chup anh cua Quet mo trinh quet tai lieu (ML Kit / VisionKit), loi quyen nhac chon anh co san" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

### Task 10: Nút Back Android

**Files:**
- Create: `src/app-di-dong/NutBack.tsx`
- Modify: `src/App.tsx` (thêm `<NutBack />` sau `<GoCoLamMoi />`)
- Test: `src/app-di-dong/nut-back.test.tsx`

**Interfaces:**
- Consumes: `laApp`, `layMay().ngheNutBack`, `thoatApp` (Task 4).
- Produces: `export function NutBack(): null` (đặt trong Router).

- [ ] **Step 1: Bài kiểm hỏng** `src/app-di-dong/nut-back.test.tsx`

```tsx
import { act, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router";
import { afterEach, expect, test } from "vitest";
import { datNenTangThu } from "@/nen-tang";
import { taoMayGia } from "@/test/may-gia";
import { NutBack } from "./NutBack";

afterEach(() => datNenTangThu(null));
async function moApp(lichSu: string[]) {
  const gia = taoMayGia(); datNenTangThu({ app: true, may: gia.may });
  render(
    <MemoryRouter initialEntries={lichSu} initialIndex={lichSu.length - 1}>
      <NutBack />
      <Routes>
        <Route path="/" element={<p>màn hình chính</p>} />
        <Route path="/van-ban" element={<p>danh sách văn bản</p>} />
        <Route path="/van-ban/:id" element={<p>chi tiết văn bản</p>} />
      </Routes>
    </MemoryRouter>,
  );
  await waitFor(() => expect(gia.may.ngheNutBack).toHaveBeenCalled());
  const back = gia.may.ngheNutBack.mock.calls[0][0];
  return { gia, back };
}

test("còn lịch sử → lùi một trang", async () => {
  const { back } = await moApp(["/van-ban", "/van-ban/1"]);
  act(() => back(true));
  expect(await screen.findByText("danh sách văn bản")).toBeInTheDocument();
});

test("hết lịch sử ở trang con → về màn hình chính", async () => {
  const { back } = await moApp(["/van-ban/1"]);
  act(() => back(false));
  expect(await screen.findByText("màn hình chính")).toBeInTheDocument();
});

test("ở màn hình chính → thoát app", async () => {
  const { gia, back } = await moApp(["/"]);
  act(() => back(false));
  expect(gia.may.thoatApp).toHaveBeenCalled();
});

test("đang mở hộp thoại → gửi Esc cho hộp thoại, không đổi trang", async () => {
  const { back } = await moApp(["/van-ban", "/van-ban/1"]);
  const hop = document.createElement("div");
  hop.setAttribute("role", "dialog");
  let nhanEsc = false;
  hop.addEventListener("keydown", (e) => { if (e.key === "Escape") nhanEsc = true; });
  document.body.appendChild(hop);
  act(() => back(true));
  expect(nhanEsc).toBe(true);
  expect(screen.getByText("chi tiết văn bản")).toBeInTheDocument();
  hop.remove();
});

test("bản web: không đăng ký nghe nút Back", () => {
  const gia = taoMayGia(); // không datNenTangThu → chế độ web
  render(<MemoryRouter><NutBack /></MemoryRouter>);
  expect(gia.may.ngheNutBack).not.toHaveBeenCalled();
});
```

- [ ] **Step 2: Chạy, thấy hỏng** — `npx vitest run src/app-di-dong/nut-back.test.tsx` → FAIL.

- [ ] **Step 3: `src/app-di-dong/NutBack.tsx`**

```tsx
import { useEffect, useRef } from "react";
import { useLocation, useNavigate } from "react-router";
import { laApp, layMay } from "@/nen-tang";

const THOAT_O = new Set(["/", "/dang-nhap"]);

/** Nút Back Android (spec §1): đang mở hộp thoại → gửi Esc để đóng; ở màn hình chính / đăng nhập → thoát app; còn lịch sử →
 *  lùi một trang; hết lịch sử → về màn hình chính. Bản web: không làm gì. */
export function NutBack() {
  const di = useNavigate();
  const { pathname } = useLocation();
  const duong = useRef(pathname);
  duong.current = pathname;
  useEffect(() => {
    if (!laApp()) return;
    let go: (() => void) | null = null;
    let huy = false;
    void layMay().then((m) => {
      if (huy) return;
      go = m.ngheNutBack((coTheLui) => {
        const hop = document.querySelector<HTMLElement>("dialog[open], [role='dialog'], [role='alertdialog']");
        if (hop) { hop.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true, cancelable: true })); return; }
        if (THOAT_O.has(duong.current)) m.thoatApp();
        else if (coTheLui) void di(-1);
        else void di("/", { replace: true });
      });
    });
    return () => { huy = true; go?.(); };
  }, [di]);
  return null;
}
```
`src/App.tsx`: `import { NutBack } from "@/app-di-dong/NutBack";` và `<GoCoLamMoi />` → `<GoCoLamMoi /><NutBack />`.
(`NutBack` không import plugin nên gói web chỉ có thêm vài dòng; kiểm lại bằng lệnh `rg` của Task 4 Step 10.)

Ghi chú: `canGoBack` của WebView đo lịch sử WebView; ở lần mở đầu `MemoryRouter` của bài kiểm giả lập đúng hai trường hợp.

- [ ] **Step 4: Chạy** — bài mới PASS; `npm test`; `npx tsc -b`; `npm run build` + lệnh `rg` Task 4 Step 10 → «gói web sạch».
- [ ] **Step 5: Commit**

```bash
git add src/app-di-dong/NutBack.tsx src/app-di-dong/nut-back.test.tsx src/App.tsx
git commit -m "app: nut Back Android (dong hop thoai, lui trang, ve man hinh chinh, thoat app)" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

## Phần C — Dự án native, build, thử, duyệt

### Task 11: Dự án Capacitor (`capacitor.config.ts`, `android/`, `ios/`, biểu tượng)

**Files:**
- Create: `capacitor.config.ts`, `android/` (sinh), `ios/` (sinh), `cong-cu/bieu-tuong-app.mjs`, `assets/logo.png`
- Modify: `android/app/src/main/AndroidManifest.xml`, `android/app/build.gradle`, `ios/App/App/Info.plist`, `.gitignore`

**Interfaces:**
- Produces: `appId` `com.giakiemso.vanbanplus`, tên «VanBanPlus», `webDir` `dist`; `npm run build:app` sinh bản web app + chép vào
  `android/app/src/main/assets/public`, `ios/App/App/public`.

- [ ] **Step 1: `capacitor.config.ts`**

```ts
import type { CapacitorConfig } from "@capacitor/cli";

const config: CapacitorConfig = {
  appId: "com.giakiemso.vanbanplus",
  appName: "VanBanPlus",
  webDir: "dist",
  // Android: https://localhost, iOS: capacitor://localhost — hai nguồn đã có trong CORS của API (src/lib/phien/cors.ts).
  android: { allowMixedContent: false },
  plugins: {
    SystemBars: { insetsHandling: "css", initialViewportFitValueHint: "cover" },
    // fetch / cookie vẫn của WebView: CORS + Origin còn nguyên (API dựa vào Origin để nhận diện app).
    CapacitorHttp: { enabled: false },
    CapacitorCookies: { enabled: false },
    SocialLogin: { providers: { google: true, facebook: false, apple: false, twitter: false } },
  },
};

export default config;
```

- [ ] **Step 2: Sinh dự án native**

```bash
npx tsc -b && npx vite build --mode app
npx cap add android
npx cap add ios
npx cap sync
npx cap doctor
```
Mong đợi: `cap add ios` chạy được trên Windows (mẫu SPM, đã thử 19/11), `cap doctor` báo Capacitor 8.6.2 cho cả ba gói.
Gradle lỗi vì tắt Facebook / Apple / Twitter ở Task 12 → đổi `SocialLogin.providers` về mặc định (bỏ khóa `providers`),
`npx cap sync` lại.

- [ ] **Step 3: Android** — `android/app/src/main/AndroidManifest.xml`, thẻ `<application …>` đổi `android:allowBackup="true"` →
  `android:allowBackup="false"` (token làm mới không đi theo sao lưu đám mây). **Không** thêm quyền `CAMERA` (ML Kit Document
  Scanner chạy trong Play services). `android/app/build.gradle`: `versionCode 1`, `versionName "0.1.0"`.

- [ ] **Step 4: iOS** — `ios/App/App/Info.plist`, thêm trước `</dict>` cuối:

```xml
	<key>NSCameraUsageDescription</key>
	<string>VanBanPlus dùng máy ảnh để quét văn bản giấy thành ảnh.</string>
	<key>NSPhotoLibraryAddUsageDescription</key>
	<string>VanBanPlus lưu ảnh vào Thư viện khi chọn «Lưu hình ảnh» trong bảng Chia sẻ.</string>
	<key>ITSAppUsesNonExemptEncryption</key>
	<false/>
```
`CFBundleURLTypes` cho Google iOS thêm ở Task 13 (cần client iOS). `SceneDelegate.swift` của mẫu đã chuyển
`openURLContexts` cho `SceneDelegateProxy` — không sửa.

- [ ] **Step 5: Biểu tượng** — `cong-cu/bieu-tuong-app.mjs`:

```js
// Dựng assets/logo.png (1024×1024) từ public/bieu-tuong.svg cho @capacitor/assets. Chạy: node cong-cu/bieu-tuong-app.mjs
import { chromium } from "playwright";
import { mkdirSync, readFileSync } from "node:fs";

const svg = readFileSync("public/bieu-tuong.svg", "utf8");
mkdirSync("assets", { recursive: true });
const b = await chromium.launch();
const p = await b.newPage({ viewport: { width: 1024, height: 1024 } });
await p.setContent(`<!doctype html><style>html,body{margin:0;background:transparent}svg{width:1024px;height:1024px;display:block}</style>${svg}`);
await p.screenshot({ path: "assets/logo.png", omitBackground: true });
await b.close();
console.log("đã ghi assets/logo.png");
```
Chạy `node cong-cu/bieu-tuong-app.mjs && npx @capacitor/assets@4.0.0 generate --android --ios --iconBackgroundColor "#1e40af" --splashBackgroundColor "#1e40af"`.
Xem `assets/logo.png` bằng Read (ảnh) trước khi sinh. `@capacitor/assets` lỗi (thư viện ảnh) → bỏ bước sinh, giữ biểu tượng mặc
định cho bản thử, ghi vào báo cáo Task 15.

- [ ] **Step 6: `.gitignore`** — kiểm `git check-ignore .env.app.local` in ra tên tệp (mẫu `.env*.local` có sẵn đã phủ); thêm hai
  dòng `*.apk` và `android/app/release/`. `android/.gitignore`, `ios/.gitignore` do Capacitor sinh — giữ.

- [ ] **Step 7: Kiểm + commit**

```bash
npm test && npx tsc -b && npm run build && (rg -l "SocialLogin" dist/assets && echo HỎNG || echo "gói web sạch")
git add capacitor.config.ts android ios assets cong-cu/bieu-tuong-app.mjs .gitignore package.json
git commit -m "app: du an Capacitor 8.6.2 (android, ios), bieu tuong, Info.plist, allowBackup=false" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

### Task 12: Công cụ Android trên PC + APK debug

**Files:** không đổi mã (chỉ `android/local.properties`, đã bị `android/.gitignore` loại).

Khảo sát 19/11: PC có `%LOCALAPPDATA%\Android\Sdk` (platforms `android-36`, build-tools `36.0.0`, emulator, AVD
`Medium_Phone_API_36.0` ảnh Play Store x86_64), JBR 21.0.6 của Android Studio ở `C:\Program Files\Android\Android Studio\jbr`,
**chưa có `cmdline-tools`**, `java` trên PATH là 1.8 (`JAVA_HOME` = JDK 17) — Capacitor 8 cần JDK 21. WHPX dùng được.

- [ ] **Step 1: JDK 21** (PowerShell, chỉ cho phiên này)

```powershell
$jbr = "C:\Program Files\Android\Android Studio\jbr"
if (Test-Path "$jbr\bin\java.exe") { $env:JAVA_HOME = $jbr }
else {
  winget install --id EclipseAdoptium.Temurin.21.JDK -e --accept-source-agreements --accept-package-agreements
  $env:JAVA_HOME = (Get-ChildItem "C:\Program Files\Eclipse Adoptium" -Directory | Where-Object Name -like "jdk-21*" | Select-Object -First 1).FullName
}
$env:Path = "$env:JAVA_HOME\bin;$env:Path"
java -version   # phải in 21.x
```
Không đổi `JAVA_HOME` hệ thống (project khác đang dùng JDK 17).

- [ ] **Step 2: Android SDK bằng cmdline-tools (không mở Android Studio)**

```powershell
$env:ANDROID_HOME = "$env:LOCALAPPDATA\Android\Sdk"
if (-not (Test-Path "$env:ANDROID_HOME\cmdline-tools\latest\bin\sdkmanager.bat")) {
  $z = "$env:TEMP\cmdline-tools.zip"
  Invoke-WebRequest https://dl.google.com/android/repository/commandlinetools-win-13114758_latest.zip -OutFile $z
  Expand-Archive $z "$env:ANDROID_HOME\cmdline-tools\tam" -Force
  Move-Item "$env:ANDROID_HOME\cmdline-tools\tam\cmdline-tools" "$env:ANDROID_HOME\cmdline-tools\latest"
  Remove-Item "$env:ANDROID_HOME\cmdline-tools\tam" -Recurse -Force; Remove-Item $z
}
$sdk = "$env:ANDROID_HOME\cmdline-tools\latest\bin\sdkmanager.bat"
(1..10 | ForEach-Object { "y" }) -join "`n" | & $sdk --licenses
& $sdk "platform-tools" "platforms;android-36" "build-tools;36.0.0"
& $sdk --list_installed
```
(URL `commandlinetools-win-13114758_latest.zip` trả 200 ngày 19/11.)

- [ ] **Step 3: Build APK debug**

```powershell
cd D:\AIVanBanCaNhan\_wt-b-app-web
"sdk.dir=$($env:ANDROID_HOME -replace '\\','\\')" | Set-Content -Encoding ascii android\local.properties
npm run build:app
cd android; .\gradlew.bat assembleDebug --no-daemon; cd ..
Get-Item android\app\build\outputs\apk\debug\app-debug.apk | Select-Object Length
```
Mong đợi: `BUILD SUCCESSFUL`, APK 10–40 MB. Lỗi lớp Facebook / Apple thiếu → làm theo ghi chú Task 11 Step 2. Lỗi khác: đọc
`--stacktrace`, sửa cấu hình (không hạ phiên bản ghim khi chưa ghi vào bảng «Gói đã ghim»).

- [ ] **Step 4: Ghi cách build vào `CLAUDE.md` của web** (một mục «Bản app di động»): `npm run build:app`; JDK 21 (JBR của Android
  Studio), `ANDROID_HOME`; `android\gradlew.bat assembleDebug`; `.env.app` (công khai) + `.env.app.local` (ghi đè cục bộ);
  không đẩy `main` khi chưa duyệt; mọi khác biệt nền tảng qua `src/nen-tang.ts`, chỉ `src/app-di-dong/` import plugin; liên kết
  Mua mới phải bọc `coTheMua()` (bài kiểm `src/an-mua.test.tsx` chặn). Commit:
  `git add CLAUDE.md && git commit -m "docs: cach build ban app di dong" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"`.

### Task 13: OAuth Google cho Android / iOS + API thử nhận nhiều `aud`

**Files:**
- Modify: `.env.app` (`VITE_GOOGLE_IOS_CLIENT_ID`), `ios/App/App/Info.plist` (`CFBundleURLTypes`)
- Ngoài repo: dự án GCP `vanbanplus`, biến `VBP_GOOGLE_CLIENT_ID` của app Coolify tạm `vanbanplus-api-thu`

- [ ] **Step 1: SHA-1 khóa debug**

```powershell
& "$env:JAVA_HOME\bin\keytool.exe" -list -v -keystore "$env:USERPROFILE\.android\debug.keystore" -alias androiddebugkey -storepass android -keypass android | Select-String "SHA1:"
```
SHA-1 là thông tin công khai (không phải bí mật). Khóa debug chỉ có trên PC này: APK build máy khác sẽ không đăng nhập Google được.

- [ ] **Step 2: Tạo hai OAuth client trong dự án GCP `vanbanplus`** (Google Cloud Console → APIs & Services → Credentials →
  Create credentials → OAuth client ID), cùng dự án với client Web đang dùng:
  - **Android:** package `com.giakiemso.vanbanplus`, SHA-1 ở Step 1, tên «VanBanPlus Android (debug)».
  - **iOS:** bundle ID `com.giakiemso.vanbanplus`, tên «VanBanPlus iOS».
  Làm qua trình duyệt đã đăng nhập (skill `lai-trinh-duyet`, hồ sơ có tài khoản quản trị dự án `vanbanplus`); không vào được →
  nhờ chủ dự án làm (5 phút, gửi lại hai client ID). Màn đồng ý OAuth đang ở chế độ nào thì giữ nguyên (web đã đăng nhập Google
  thật từ 18/11); nếu là «Testing», thêm tài khoản Google dùng thử vào «Test users».

- [ ] **Step 3: Web app** — `.env.app`: `VITE_GOOGLE_IOS_CLIENT_ID=<client iOS>`. `Info.plist` thêm trước `</dict>` cuối (đảo
  client iOS `123-abc.apps.googleusercontent.com` → `com.googleusercontent.apps.123-abc`):

```xml
	<key>CFBundleURLTypes</key>
	<array>
		<dict>
			<key>CFBundleURLSchemes</key>
			<array>
				<string>com.googleusercontent.apps.<phần trước .apps.googleusercontent.com của client iOS></string>
			</array>
		</dict>
	</array>
```

- [ ] **Step 4: API thử nhận ba client** (ghi `uuid` của app tạm từ Task 3 Step 4 vào `UUID`):

```bash
py -3.13 -c "import sys; sys.path.insert(0, r'D:\Thang\gikidi-server\chuyen'); from coolify import api; api('PATCH', '/applications/UUID/envs', json={'key': 'VBP_GOOGLE_CLIENT_ID', 'value': '<web>,<ios>,<android>', 'is_preview': False}); print(api('POST', '/deploy', params={'uuid': 'UUID'}))"
```
Chờ triển khai xong (`/api/health` 200), chạy lại `node scripts/thu-phien-app.mjs` (repo API) → 11 ✔. **Không** đổi biến của app
API prod (prod chỉ đổi khi API app lên `main`, ngoài kế hoạch).

- [ ] **Step 5: Commit**

```bash
git add .env.app ios/App/App/Info.plist
git commit -m "app: client Google iOS + URL scheme; Android dung client Web lam serverClientId" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

### Task 14: Chạy thử trên máy ảo + điện thoại, quay video, gửi chủ dự án duyệt

**Files:**
- Create: `cong-cu/thu-app-android.mjs`; `.env.app.local` (không commit)
- Ảnh / video: `.tam/thu-app/` (đã gitignore)

- [ ] **Step 1: Build APK thử trỏ API thử** — `.env.app.local`:

```
VITE_API_GOC=https://vanbanplus-api-thu.app.giakiemso.com
VITE_API_GOC_TAC_VU=https://vanbanplus-api-thu.app.giakiemso.com
```
rồi `npm run build:app` và `android\gradlew.bat assembleDebug` (PowerShell, `JAVA_HOME` / `ANDROID_HOME` như Task 12).

- [ ] **Step 2: Mở máy ảo + cài**

```powershell
$adb = "$env:ANDROID_HOME\platform-tools\adb.exe"
Start-Process "$env:ANDROID_HOME\emulator\emulator.exe" -ArgumentList "-avd","Medium_Phone_API_36.0","-no-snapshot-save","-no-boot-anim"
& $adb wait-for-device
while ((& $adb shell getprop sys.boot_completed).Trim() -ne "1") { Start-Sleep 3 }
& $adb install -r android\app\build\outputs\apk\debug\app-debug.apk
```
PC không chạy được máy ảo (WHPX lỗi, thiếu RAM) → **phương án:** (a) máy ảo Android ở máy chủ, user `clip` (đã có máy ảo
Android cho việc clip — xem `/data/clip/SO-ROOT.md`; `adb connect` qua Tailscale, chạy cùng script với `ADB_SERVER_SOCKET`);
(b) chủ dự án cài APK tay (Step 5) và quay màn hình điện thoại.

- [ ] **Step 3: Viết `cong-cu/thu-app-android.mjs`**

```js
// Lái bản app VanBanPlus trên máy Android đang nối adb (máy ảo hoặc thật), chụp ảnh từng bước, quay video.
// Dùng: node cong-cu/thu-app-android.mjs [--api https://vanbanplus-api-thu.app.giakiemso.com]
// Cần: APK debug đã cài (WebView gỡ lỗi được), adb trên PATH hoặc ANDROID_HOME. Không in mật khẩu, token, URL ký.
import { _android as android } from "playwright";
import { execFileSync } from "node:child_process";
import { randomBytes } from "node:crypto";
import { mkdirSync } from "node:fs";

const i = process.argv.indexOf("--api");
const API = (i > 0 ? process.argv[i + 1] : "https://vanbanplus-api-thu.app.giakiemso.com").replace(/\/$/, "");
const PKG = "com.giakiemso.vanbanplus";
const RA = ".tam/thu-app";
mkdirSync(RA, { recursive: true });
let hong = 0; let so = 0;
const buoc = (ten, dung, them = "") => { console.log(`${dung ? "✔" : "✘"} ${ten}${them ? ` — ${them}` : ""}`); if (!dung) hong++; };
const ngu = (ms) => new Promise((r) => setTimeout(r, ms));

// ---- dữ liệu thử bằng API (Node) ----
async function goi(duong, { method = "POST", than, bearer, tho, mime } = {}) {
  const r = await fetch(duong.startsWith("http") ? duong : API + duong, {
    method, signal: AbortSignal.timeout(60_000),
    headers: { ...(than ? { "Content-Type": "application/json" } : {}), ...(mime ? { "Content-Type": mime } : {}),
      ...(bearer ? { Authorization: `Bearer ${bearer}` } : {}), "X-VBP-Nguon": "web" },
    body: tho ?? (than ? JSON.stringify(than) : undefined),
  });
  let j = null; try { j = await r.json(); } catch {}
  return { r, j };
}
function pdfMotTrang(chu) {
  const o = ["<< /Type /Catalog /Pages 2 0 R >>", "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
    "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>"];
  const nd = `BT /F1 24 Tf 72 760 Td (${chu}) Tj ET`;
  o.push(`<< /Length ${nd.length} >>\nstream\n${nd}\nendstream`, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
  let s = "%PDF-1.4\n"; const vt = [];
  o.forEach((x, k) => { vt.push(s.length); s += `${k + 1} 0 obj\n${x}\nendobj\n`; });
  const xref = s.length;
  s += `xref\n0 ${o.length + 1}\n0000000000 65535 f \n${vt.map((v) => `${String(v).padStart(10, "0")} 00000 n \n`).join("")}`;
  return Buffer.from(`${s}trailer\n<< /Size ${o.length + 1} /Root 1 0 R >>\nstartxref\n${xref}\n%%EOF\n`, "latin1");
}

const email = `thu-app-${randomBytes(4).toString("hex")}@vanbanplus.test`;
const matKhau = randomBytes(15).toString("base64url");
const dk = await goi("/api/auth/register", { than: { email, password: matKhau, fullName: "Thử app" } });
buoc("0a. đăng ký tài khoản thử", dk.r.ok, String(dk.r.status));
const dn = await goi("/api/v2/phien/dang-nhap", { than: { email, mat_khau: matKhau } });
const at = dn.j?.data?.access_token;
const vb = await goi("/api/v2/kho/van-ban", { bearer: at, than: { huong: "den", trich_yeu: "Thử app di động", noi_dung: "Điều 1. Thử app di động.\nĐiều 2. Văn bản sẽ bị xóa sau khi thử." } });
const vbId = vb.j?.data?.id;
const pdf = pdfMotTrang("Thu app VanBanPlus");
const xin = await goi("/api/v2/kho/tep/xin-tai-len", { bearer: at, than: { ten: "Thu-app.pdf", mime: "application/pdf", byte: pdf.length, gan: { loai: "van_ban", id: vbId } } });
const put = xin.j?.data?.url_tai_len ? await fetch(xin.j.data.url_tai_len, { method: "PUT", headers: { "Content-Type": "application/pdf" }, body: pdf }) : null;
const xn = await goi("/api/v2/kho/tep/xac-nhan", { bearer: at, than: { id: xin.j?.data?.id } });
buoc("0b. văn bản + PDF đính kèm qua API", Boolean(vbId) && Boolean(put?.ok) && xn.r.ok);
if (hong) { console.log(`Tài khoản thử: ${email}`); process.exit(1); }

// ---- máy Android ----
const [may] = await android.devices();
if (!may) { console.log("✘ không thấy máy Android qua adb"); process.exit(1); }
const sh = async (lenh) => (await may.shell(lenh)).toString();
const anh = async (ten) => { so++; await may.screenshot({ path: `${RA}/${String(so).padStart(2, "0")}-${ten}.png` }); };
const moApp = async () => {
  await sh(`am force-stop ${PKG}`); await sh(`monkey -p ${PKG} -c android.intent.category.LAUNCHER 1`);
  const wv = await may.webView({ pkg: PKG }, { timeout: 30_000 });
  const p = await wv.page();
  await p.waitForLoadState("domcontentloaded");
  return p;
};
const di = (p, duong) => p.evaluate((d) => { history.pushState({}, "", d); dispatchEvent(new PopStateEvent("popstate")); }, duong);
// Quay màn hình (tối đa 170 giây / đoạn); kéo tệp bằng `adb pull` (đọc nhị phân qua shell có thể hỏng byte).
const quay = (ten) => {
  const vo = sh(`screenrecord --time-limit 170 /sdcard/${ten}.mp4`);
  return async () => {
    await sh("pkill -INT screenrecord").catch(() => {}); await vo.catch(() => {}); await ngu(1500);
    execFileSync("adb", ["pull", `/sdcard/${ten}.mp4`, `${RA}/${ten}.mp4`]);
  };
};

await sh(`pm clear ${PKG}`);
const dungQuay1 = quay("1-dang-nhap-phien-mua");
let p = await moApp();
await p.getByRole("heading", { name: "Đăng nhập VanBanPlus" }).waitFor({ timeout: 30_000 });
await anh("man-dang-nhap");
if (!(await p.getByLabel("Mật khẩu").isVisible().catch(() => false))) await p.getByRole("button", { name: /mật khẩu/i }).first().click();
await p.getByLabel("Email").fill(email);
await p.getByLabel("Mật khẩu").fill(matKhau);
await p.getByRole("button", { name: "Đăng nhập" }).click();
await p.waitForURL((u) => !u.pathname.startsWith("/dang-nhap"), { timeout: 30_000 });
buoc("1. đăng nhập mật khẩu trong app", true);
await anh("man-hinh-chinh");

const coChuMua = async () => (await p.locator("text=/Mua thêm|Nâng gói|Gia hạn/").count()) + (await p.locator('a[href^="/mua"]').count());
buoc("2a. màn hình chính không có lối Mua", (await coChuMua()) === 0);
await di(p, "/tai-khoan"); await ngu(2500); await anh("tai-khoan");
buoc("2b. Tài khoản không có «Mua thêm credit» / «Gia hạn / Nâng gói»", (await coChuMua()) === 0);
await di(p, "/mua?loai=credit"); await ngu(1500);
buoc("2c. /mua?loai=credit về màn hình chính", (await p.evaluate(() => location.pathname)) === "/");

p = await moApp(); await ngu(4000); await anh("mo-lai-con-phien");
buoc("3. tắt app, mở lại vẫn còn phiên (token trong Keystore)", !(await p.evaluate(() => location.pathname)).startsWith("/dang-nhap"));
await dungQuay1();

const dungQuay2 = quay("2-word-pdf-back");
await di(p, "/van-ban"); await ngu(1500);
await di(p, `/van-ban/${vbId}`);
await p.getByRole("button", { name: "Xuất Word" }).click(); await ngu(3500); await anh("xuat-word-bang-chia-se");
buoc("4. Xuất Word mở bảng Chia sẻ của máy", (await sh("dumpsys window | grep -E 'mCurrentFocus|mFocusedApp'")).toLowerCase().includes("chooser") || (await sh("dumpsys activity activities | grep -i resumed")).toLowerCase().includes("chooser"));
await may.input.press("Back"); await ngu(1500);
await p.getByRole("button", { name: "Xem Thu-app.pdf" }).click(); await ngu(5000); await anh("mo-pdf");
const truoc = (await sh("dumpsys activity activities | grep -i resumed")).toLowerCase();
buoc("5. PDF mở bằng ứng dụng khác của máy (không còn là app VanBanPlus ở trên cùng)", !truoc.includes(PKG), truoc.trim().slice(0, 120));
await may.input.press("Back"); await ngu(2000);
p = (await (await may.webView({ pkg: PKG })).page());
const truocBack = await p.evaluate(() => location.pathname);
await may.input.press("Back"); await ngu(1500);
const sauBack = await p.evaluate(() => location.pathname);
buoc("6a. Back trong app lùi một trang", truocBack !== sauBack, `${truocBack} → ${sauBack}`);
await di(p, "/"); await ngu(1000);
await may.input.press("Back"); await ngu(2000); await anh("back-o-man-chinh");
buoc("6b. Back ở màn hình chính thoát app", !(await sh("dumpsys activity activities | grep -i resumed")).includes(PKG));
await dungQuay2();

console.log(`Ảnh + video: ${RA}`);
console.log(`Tài khoản thử: ${email} — dọn bằng SQL (khuôn docs/superpowers/plans/2026-11-17-web-l4.md Task 2 Step 3)`);
await may.close();
process.exit(hong ? 1 : 0);
```
Cách lấy trạng thái trên cùng (`dumpsys`) đổi theo bản Android: bước 4 / 5 / 6b báo ✘ mà ảnh chụp cho thấy đúng → sửa câu
`grep` theo đầu ra thật của máy (in `dumpsys` ra màn hình một lần), không bỏ bước.

- [ ] **Step 4: Chạy** — `$env:Path = "$env:ANDROID_HOME\platform-tools;$env:Path"; node cong-cu/thu-app-android.mjs` → 0a–6b ✔.
  Thêm tay trên máy ảo, có ảnh: đổi hình nền ở Cài đặt → tắt app → mở lại → hình nền còn (**cookie `osk_gd` trên Android**);
  mở «Điều khoản» ở màn đăng nhập → trình duyệt trong app (Custom Tabs) → Back về app. Tự xem từng ảnh (Read) trước khi gửi:
  không có chữ «Mua», không lộ email / URL ký trên ảnh. Máy ảo không có ứng dụng xem PDF (bước 5 báo «Máy chưa có ứng dụng mở
  được loại tệp này.») → cài «Google Drive» từ Play Store trên máy ảo (cần tài khoản Google) hoặc để bước này cho điện thoại.

- [ ] **Step 5: Điện thoại thật — quét 3 trang + Google** (ML Kit không chạy trên máy ảo):
  - Có điện thoại cắm USB / `adb pair` Wi-Fi → `adb install -r …apk`, quay `adb shell screenrecord`, tự làm: Quét → «Chụp ảnh»
    → quét 3 trang giấy → «Đọc» → soát; Đăng nhập → «Tiếp tục với Google».
  - Không có → gửi APK cho chủ dự án (Step 6) kèm hướng dẫn ngắn: cài APK (cho phép «nguồn không rõ»), quét 3 trang, thử Google,
    quay màn hình điện thoại, gửi lại video.

- [ ] **Step 6: Gửi Telegram chủ dự án** (khóa bot chỉ đọc trên máy chủ, không in):

```bash
ssh gikidi-dev "mkdir -p /tmp/vbp-app-thu"
scp .tam/thu-app/*.png .tam/thu-app/*.mp4 android/app/build/outputs/apk/debug/app-debug.apk gikidi-dev:/tmp/vbp-app-thu/
ssh gikidi-dev 'sudo bash -s' <<'EOF'
set -a; . /etc/gikidi/telegram.env; set +a
for f in /tmp/vbp-app-thu/*; do
  curl -s -o /dev/null -w "%{http_code} $(basename "$f")\n" -F chat_id="$TELEGRAM_CHAT_ID" \
    -F caption="VanBanPlus app thử (nhánh dot-b-app) — $(basename "$f")" -F document=@"$f" \
    "https://api.telegram.org/bot$TELEGRAM_BOT_TOKEN/sendDocument"
done
rm -rf /tmp/vbp-app-thu
EOF
```
Mỗi dòng phải `200`. Video > 50 MB (giới hạn Bot API) → nén trước bằng `ffmpeg -i in.mp4 -vf scale=540:-2 -crf 30 out.mp4`.
Đồng thời gửi MP4 lên điện thoại bằng SendUserFile nếu phiên có công cụ đó. Tin nhắn kèm (dán vào Telegram, theo luật Zalo:
không chào, không nhắm người nhận):

```
VanBanPlus app Android — bản chạy thử (nhánh dot-b-app, chưa phát hành)
- Đăng nhập mật khẩu, tắt mở lại vẫn còn phiên
- Không có lối vào Mua; /mua tự về màn hình chính
- Xuất Word mở bảng Chia sẻ; PDF mở bằng ứng dụng của máy
- Nút Back: lùi trang, ở màn hình chính thì thoát
- Quét 3 trang + Google: thử trên điện thoại thật (APK kèm theo)
- API dùng bản thử vanbanplus-api-thu, dữ liệu thật; web không đổi
```

- [ ] **Step 7: Dọn** — tài khoản thử (email in cuối script) bằng SQL như Task 3 Step 6; `adb uninstall com.giakiemso.vanbanplus`
  không cần (máy ảo dùng riêng cho thử).

### Task 15: Tài liệu, đẩy nhánh, bàn giao

- [ ] **Step 1: Hợp đồng** `docs/superpowers/plans/2026-11-17-hop-dong-web-dot-a.md` (repo gốc `D:/AIVanBanCaNhan`): §4 thêm đoạn
  «App di động: `X-VBP-Nguon: app` + Origin `capacitor://localhost` / `https://localhost` (`VBP_NGUON_APP`) → `dang-nhap`,
  `google`, `2-lop`, `otp/xac-nhan`, `lam-moi` trả `data.token_lam_moi`, không `Set-Cookie`; `lam-moi` / `dang-xuat` chỉ đọc
  `token_lam_moi` trong thân. `VBP_GOOGLE_CLIENT_ID` = danh sách dấu phẩy (web, iOS, Android)»; §9 thêm một dòng ngày 19/11/2026
  «App (nhánh `dot-b-app`, chưa lên `main`)». Commit **chỉ tệp này** ở repo gốc, **không đẩy** (chủ dự án quyết).
- [ ] **Step 2: Spec** — đầu `docs/superpowers/specs/2026-11-19-app-di-dong-design.md` thêm dòng trạng thái «Kế hoạch
  `docs/superpowers/plans/2026-11-19-app-di-dong.md`; APK thử gửi duyệt <ngày>». Commit cùng Step 1.
- [ ] **Step 3: Đẩy nhánh hai repo** — API: `npm test && npx tsc --noEmit && git push origin dot-b-app`; web:
  `npm test && npx tsc -b && npm run build && git push origin dot-b-app`. Kiểm `git log origin/main -1` của cả hai repo **không
  đổi** so với lúc bắt đầu.
- [ ] **Step 4: Bàn giao (báo cáo cuối phiên)** — liệt kê: số commit mỗi nhánh; kết quả 0a–6b; ảnh / video đã gửi; việc chờ chủ
  dự án duyệt; việc sau khi duyệt (ngoài kế hoạch): (1) gộp API `dot-b-app` vào `main` (chỉ thêm, web không đổi) + đặt
  `VBP_GOOGLE_CLIENT_ID` ba client trên app API prod; (2) xóa `.env.app.local`, build lại APK trỏ prod; (3) gỡ app Coolify
  `vanbanplus-api-thu` + xóa `C:/Users/PC/.gikidi/vanbanplus-api-thu.env` + ghi `THAY-DOI.md`; (4) gộp web `dot-b-app` vào `main`
  (web build thường không đổi hành vi — Coolify chỉ tốn thêm thời gian cài gói); (5) tài khoản Apple Developer + Google Play,
  khóa ký phát hành + SHA-1 phát hành (client Android thứ hai), build iOS (Mac / Codemagic), kiểm cookie `osk_gd` + Google trên
  iOS; (6) hoãn: thông báo đẩy, ngoại tuyến, CSP thẻ meta cho bản app.

---

## Tự soát (đã chạy khi viết kế hoạch)

**1. Phủ spec**

| Spec | Task |
| --- | --- |
| §1 bọc web + camera quét + giữ phiên + Back; ẩn Mua; đóng gói sẵn; duyệt bằng APK + ảnh / video Telegram | 4, 9, 5, 10, 6, 11 (`webDir: dist`, không `server.url`), 14 |
| §2 Capacitor ghim bản, `appId`, tên; `VITE_NEN_TANG=app`; `nen-tang.ts` (`laApp`, `coTheMua`, `luuTep`, `moTep`, `moNgoai`); BrowserRouter, không service worker; `X-VBP-Nguon: app` + `X-VBP-Phien-Ban` | 4, 11, 5 |
| §3 API trả token trong thân cho app, không cookie; chiều nhận; app lưu Keychain / Keystore, gửi khi làm mới, xóa khi đăng xuất; CORS | 1, 5, 3 (CORS đã có — kiểm preflight) |
| §4 Google native, `serverClientId` = web; `aud` nhiều client; tạo client iOS + Android (SHA-1 debug) | 8, 4 (`initialize`), 2, 13 |
| §5 Word / mã dự phòng → Filesystem + Share; PDF / `window.open` / `_blank` → tải tạm + mở bằng máy; liên kết ngoài → Browser; cookie `osk_gd` | 7, 14 (Android), hoãn iOS |
| §6 quét: ML Kit / VisionKit, vào `useTepQuet`, quyền camera, từ chối → nhắc chọn ảnh | 9, 4 (`quetTaiLieu`), 11 (Info.plist; Android không cần quyền — ghi ở Quyết định) |
| §7 `coTheMua()`, ~20 lối vào, câu trung tính, `/mua*` → trang chủ | 6, 5 (`HET_DUNG_LUONG`) |
| §8 nhánh `dot-b-app`; Android JDK + SDK + APK debug; iOS giữ cấu hình; kiểm API / web / máy thật; phát hành chỉ khi duyệt | Global Constraints, 12, 11, 1–2, 4–10, 14, 15 |

**2. Quét chỗ trống** — không còn «TBD / làm sau / tương tự Task N». Hai giá trị chỉ có lúc chạy, có lệnh lấy cụ thể: client ID
Web (Task 4 Step 8, lệnh Coolify), client iOS / Android + `uuid` app tạm (Task 13, Task 3 Step 4).

**3. Khớp tên / kiểu** — `MayApp` (Task 4) dùng đúng ở `may-gia.ts`, `khach.ts` (`khoToken.doc/ghi/xoa`), `NutGoogle`
(`googleIdToken`), `ChonTep` (`quetTaiLieu(toiDa)`), `NutBack` (`ngheNutBack`, `thoatApp`), `DanhSachTep` / `KhoiTep`
(`moTep(layUrl, ten, mime)` → `taiVaMo(url, ten, mime)`), `dung-docx` / `BatHaiLop` (`luuTep(tep, ten)` → `ghiVaChiaSe(tep, ten)`);
API `traLoi(kq, { app })`, `traLoiLamMoi(kq, { app })`, `tokenLamMoi(req, than, chiThan)`, `laNguonApp(req)`,
`docDsClientGoogle`, `kiemIdTokenGoogle({ clientId: string | readonly string[] })` khớp giữa Task 1–3 và bài kiểm.

## Rủi ro

- **Google Android cần SHA-1 đúng khóa ký:** khóa debug chỉ ở PC này; APK build nơi khác / bản phát hành / Play App Signing cần
  client Android riêng. Lỗi thường gặp `[28444] Developer console is not set up correctly` (app hiện câu báo, mật khẩu vẫn dùng
  được). Máy không có Google Play services (Huawei…) → không Google, không quét ML Kit (câu báo nhắc chọn ảnh).
- **ML Kit Document Scanner không chạy trên máy ảo** → quét chỉ thử được trên điện thoại thật.
- **HEIC:** trình quét trả JPEG nên đường chính an toàn. «Chọn tệp» ảnh HEIC trên Android WebView không giải mã được bằng canvas
  (như Chrome Android hiện nay) → `chuanBiTep` báo «Không xử lý được ảnh…»; iOS WKWebView giải mã được. Không đổi trong đợt này.
- **Cookie `osk_gd`:** Android kiểm ở Task 14 (cookie WebView có thể chưa ghi xuống đĩa nếu app bị giết ngay sau khi đổi — thấy
  mất thì chuyển sang Preferences qua `nen-tang.ts`); iOS `capacitor://` chưa kiểm được.
- **Back khi hộp thoại không nghe phím Esc:** Back «không làm gì» cho tới khi đóng hộp thoại — quan sát ở Task 14, ghi các hộp
  thoại cần thêm xử lý Esc.
- **`useDieuHuong().push(url ngoài)` của os-kit** đi trình duyệt hệ thống thay vì Browser trong app (không sửa os-kit).
- **Plugin cộng đồng ra bản liên tục** (capgo ra bản gần như mỗi ngày): đã ghim chính xác; đổi bản phải qua bảng «Gói đã ghim».
- **App Coolify tạm dùng CSDL / Storage prod**: tài khoản thử phải dọn; quên gỡ app tạm thì tốn RAM 2 GB — Task 15 ghi việc gỡ.
- **Gói web cài thêm 14 gói Capacitor** khi Coolify build (khi nào gộp `main`): tăng thời gian cài, không đổi gói chạy (đã kiểm
  bằng `rg` trên `dist/`).
- **Máy ảo có thể không có ứng dụng xem PDF** → bước 5 thử trên điện thoại.
