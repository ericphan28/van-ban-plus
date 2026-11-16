# Đợt 3 — Bản cài máy gọi API tác vụ v2 — kế hoạch triển khai

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bản cài máy WPF v1.0.20 chạy đủ 6 tác vụ AI qua `/api/v2/tac-vu/*`, báo trước khoảng credit và số dư, xử lý đủ mã lỗi, không còn prompt nghiệp vụ, không còn gọi thẳng Gemini; dựng sẵn bộ cài, chạy thật trên production, rồi DỪNG chờ chủ dự án cho phép phát hành.

**Architecture:** Một lớp `TacVuClient` trong `AIVanBan.Core` gửi dữ liệu nghiệp vụ (JSON snake_case theo hợp đồng) tới máy chủ, đọc `data.ket_qua` thẳng vào model C# hiện có (tên trường JSON không đổi), đổi mọi lỗi thành `TacVuException` có mã. Bốn service hiện có (`DocumentSummaryService`, `DocumentReviewService`, `DocumentAdvisoryService`, `PeriodicReportService`) giữ chữ ký công khai nhưng bên trong gọi `TacVuClient`; trích xuất chuyển sang `TrichXuatService` (đổi tên từ `GeminiAIService`), soạn thảo sang `TacVuSoanThao`. Phía giao diện, một lớp tĩnh `TacVuGiaoDien` dùng chung cho mọi hộp thoại: gọi `/uoc-tinh`, hỏi xác nhận bằng `MessageBox` (giống kiểu các hộp thoại hiện có), báo lỗi, mời mua credit.

**Tech Stack:** .NET 9 (`net9.0-windows`), WPF + MaterialDesignInXAML 5.3, System.Text.Json, xUnit 2.9 (`HttpMessageHandler` giả), AutoUpdater.NET 1.9.2 (kênh `update.xml` của GitHub `ericphan28/van-ban-plus-releases`), Inno Setup 6.

**Spec:** [docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md](../specs/2026-11-09-api-tac-vu-ai-credit-design.md) — §1 tiêu chí 3, §6, §10, §11 (chạy thật mục 3), §12 bước 4–5.
**Hợp đồng (nguồn DUY NHẤT về hành vi máy chủ):** [docs/superpowers/plans/2026-11-16-hop-dong-tac-vu-v2.md](2026-11-16-hop-dong-tac-vu-v2.md).


> **Bổ sung của người điều phối (16/11/2026, sau khi kế hoạch dot2b chốt — đọc trước khi làm):** hợp đồng
> `2026-11-16-hop-dong-tac-vu-v2.md` được dot2b Task 0 mở rộng. Lấy bản hợp đồng MỚI NHẤT trên `main` của repo này làm chuẩn:
> - `soan-thao` có thêm `ten_mau` (tên mẫu văn bản đang chọn trong hộp thoại) và `dia_phuong.dac_diem` (mảng chuỗi, tối đa 10);
>   `dia_phuong` = `{ xa*, co_quan_cap_tren, tinh*, dac_diem }`, `co_quan_cap_tren` bỏ trống được. Task 7 phải gửi cả hai
>   trường và thêm ô «đặc điểm địa phương» (nhiều dòng) vào Thiết lập → Cơ quan; mặc định Gia Kiệm khi chưa lưu gồm cả ba
>   dòng đặc điểm như prompt cũ.
> - `trich-xuat` chỉ nhận PDF, PNG, JPEG, WEBP, HEIC, HEIF; `chi-chu` nhận đúng 1 tệp. Task 9 phải đổi GIF/BMP/TIFF sang PNG
>   trước khi gửi và gọi `chi-chu` từng tệp một.

## Phạm vi

Làm: client v2 + kiểm thử; đổi 6 đường gọi (kể cả đường đọc chữ PDF trong 3 hộp thoại và đường ghép trang gọi thẳng Gemini); thiết lập địa phương cho `soan-thao`; hộp thoại báo credit + xử lý lỗi; xóa prompt C# và bộ sinh tệp mẫu; bỏ chế độ gọi thẳng Gemini; tài liệu; nâng phiên bản, build bộ cài, chạy thật 6 tác vụ; phát hành sau khi chủ dự án cho phép.

Không đụng:

- Mã máy chủ `vanbanplus-api/src/lib/**`, `src/app/api/**`, migration. Chỉ Task 13 (sau khi được phép) sửa trang `tai-xuong`, `huong-dan`, chuỗi phiên bản ở chân trang và trang đăng nhập.
- Đường cũ `/api/ai/*` trên máy chủ: giữ cho bản cài máy cũ (spec §1 tiêu chí 4).
- `AIVanBan.API` (máy chủ C# cũ), `AIVanBan.Package` (MSIX), `docs/thue/`, `.superpowers/`, mọi thay đổi chưa commit của phiên khác.
- Thanh trạng thái `MainWindow` vẫn hiện requests/tokens từ `/api/usage`; đổi sang số dư credit là việc riêng.
- `VercelBypassToken`: các hộp thoại tài khoản/quản trị còn dùng; client v2 không gửi header này (máy chủ đã chạy trên Megacore). Gỡ hẳn là việc riêng.
- `AIComposeDialog` khi lưu văn bản vẫn ghi `Location = "Gia Kiệm"` (không phải prompt). Ghi nhận, việc riêng.

## Global Constraints

- Hành vi máy chủ chỉ lấy từ hợp đồng: `POST /api/v2/tac-vu/<ten>` và `POST /api/v2/tac-vu/<ten>/uoc-tinh`; header `X-API-Key`, `X-VBP-Nguon: desktop`, `X-VBP-Phien-Ban: <phiên bản ứng dụng>`; thân JSON, tên trường snake_case tiếng Việt không dấu.
- Thành công: `{ success: true, data: { ket_qua, credit_da_tru, so_du, ma_luot, bi_cat } }`. Ước tính: `{ success: true, data: { credit_min, credit_max, so_du, du_credit } }`.
- Lỗi: `{ success: false, message, code }`: `KHONG_DU_CREDIT` 402, `DAU_VAO_SAI` 400, `QUA_DAI` 413, `QUA_NHIEU_LUOT` 429, `AI_QUA_TAI` 503 + `Retry-After`, `AI_LOI` 502, `NGAT_KET_NOI` 499. 401, 404, 500 không có `code`.
- Ngày tháng gửi đi: chuỗi ISO `yyyy-MM-dd`. Trường chuỗi tùy chọn: thiếu / `null` / `""` như nhau.
- Hạn chờ: lượt chạy 180 giây (lượt dài nhất đo được 47 giây, spec §7), `trich-xuat` 300 giây, `/uoc-tinh` 30 giây. Client không tự thử lại (mỗi lần chạy là một lượt giữ credit).
- Lời hiện cho khách: tiếng Việt có dấu đầy đủ, **không đại từ nhân xưng** (tôi, chúng tôi, ta, chúng ta, anh, chị, em, bạn, mình), không «dạ/vâng/ạ». Prompt không còn trong C# nên không có ngoại lệ.
- Đơn vị gọi là «credit». Trang mua credit: `<VanBanPlusApiUrl>/bang-gia`.
- Repo `D:\AIVanBanCaNhan` (C# + tài liệu): làm trên nhánh `dot3-ban-cai-may-v2` tách từ `main` mới nhất, chỉ commit tại chỗ, **không đẩy**. `git add` đúng tệp đã nêu, không `git add -A`.
- Repo `D:\AIVanBanCaNhan\vanbanplus-api`: **ĐẨY `main` = LÊN PRODUCTION** (Coolify). Chỉ đụng ở Task 13, sau khi chủ dự án cho phép.
- Không in API key, chuỗi kết nối, nội dung tệp bí mật ra chat hay log. Bí mật chỉ nằm ở `C:\Users\PC\.gikidi\`.
- Mỗi commit kết thúc bằng một dòng trống rồi `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.

## Hiện trạng mã (đã kiểm 16/11/2026, trước đợt 2)

| Đường gọi AI | Gọi từ | Đổi sang (task) |
| --- | --- | --- |
| `DocumentSummaryService.SummarizeAsync` → `GeminiAIService.GenerateContentAsync` → `/api/ai/generate` | `DocumentSummaryDialog.StartSummary` | `tom-tat` (3) |
| `DocumentReviewService.ReviewDocumentAsync` → như trên | `DocumentReviewDialog.StartReview` | `kiem-tra` (4) |
| `DocumentAdvisoryService.AdviseAsync` → như trên | `DocumentAdvisoryDialog.StartAdvisory` | `tham-muu` (5) |
| `PeriodicReportService.GenerateReportAsync` → như trên | `PeriodicReportDialog.Generate_Click` | `bao-cao-dinh-ky` (6) |
| `AIComposeDialog.Generate_Click` → `GenerateContentAsync` (prompt trong dialog, dot2b tách sang Core) | — | `soan-thao` (7) |
| `GeminiAIService.ExtractDocumentFromMultipleFilesAsync` (khóa máy người dùng → gọi thẳng Gemini khi không qua VanBanPlus) | `ScanImportDialog.AnalyzeMergeAsync` | `trich-xuat` `cau-truc` (9) |
| `GeminiAIService.ExtractDocumentFromFileAsync` | `ScanImportDialog.AnalyzeSeparateAsync` | `trich-xuat` `cau-truc` (9) |
| `GeminiAIService.ReadTextFromFileAsync` → `/api/ai/read-text` hoặc Gemini thẳng | `DocumentSummaryDialog.UploadFile_Click`, `DocumentReviewDialog.UploadFile_Click` + `UploadReference_Click`, `DocumentAdvisoryDialog.UploadFile_Click` (nhánh `.pdf`) | `trich-xuat` `chi-chu` (9) |
| `ApiSettingsDialog.TestDirectConnection_Click` → `generativelanguage.googleapis.com` | nút ẩn chế độ bảo trì | xóa (10) |

Nơi mở các hộp thoại (không đổi, vì hộp thoại giữ hàm dựng; chỉ bỏ tham số tùy chọn `geminiApiKey` không ai truyền): `MainWindow.xaml.cs` (449, 457, 474, 514, 529), `DashboardPage.xaml.cs` (835, 845), `DocumentListPage.xaml.cs` (1153, 1462, 1549, 1606), `AIGeneratorPage.xaml.cs` (29), `TemplateManagementPage.xaml.cs` (191, dùng tham số có tên `preSelectedTemplateId:`).

Phiên bản ứng dụng: `AIVanBan.Desktop/AIVanBan.Desktop.csproj` (`<Version>`, `<AssemblyVersion>`, `<FileVersion>`), đọc lúc chạy bằng `Assembly.GetEntryAssembly()?.GetName().Version` (giống `AppUpdateService.cs:105`).

Thiết lập cơ quan: `OrganizationConfig` (LiteDB, `DocumentService.GetOrganizationConfig()`) chỉ có `Name`, `Type`, `Abbreviation`; `ExtraSettings` của `settings.json` có `Org.DefaultLocation` (địa danh, ví dụ «Gia Kiệm»), `Org.DefaultSigner`, `Org.DefaultSigningTitle`. **Không có trường xã/cơ quan cấp trên/tỉnh** → Task 7 thêm ba khóa `ExtraSettings` và ba ô ở tab «🏛️ Cơ quan».

Kênh tự cập nhật: `AppUpdateService.cs` đọc `https://raw.githubusercontent.com/ericphan28/van-ban-plus-releases/main/update.xml`. Bản 1.0.17–1.0.19 phát hành bằng: nâng phiên bản ở csproj + `setup-script.iss` + `setup-vanbanplus.iss` + `update.xml` (bản sao trong repo này) + `CHANGELOG_v1.0.x.md` + mục «Có gì mới» trong `HelpPage.xaml`; build bộ cài `Installer/VanBanPlus-Setup-1.0.x-x64.exe` (Inno Setup, `PublishDir` = `AIVanBan.Desktop\bin\Release\net9.0-windows\win-x64\publish`, self-contained); GitHub release `v1.0.x` có tệp exe; `update.xml` trên nhánh `main` của repo releases; trang `vanbanplus-api/src/app/tai-xuong/page.tsx`. Bản clone `D:\van-ban-plus-releases` đang cũ (1.0.16) — phải `pull` trước khi sửa.

## Sơ đồ tệp

```
AIVanBan.Core/Services/
  TacVuClient.cs            (mới, Task 2)  client v2: UocTinhAsync, ChayAsync<T>, mã lỗi, lời báo lỗi
  TacVuException.cs         (mới, Task 2)  MaLoiTacVu + TacVuException
  TacVuSoanThao.cs          (mới, Task 7)  soan-thao
  ThietLapDiaPhuong.cs      (mới, Task 7)  DiaPhuongCoQuan + đọc/ghi ExtraSettings
  TrichXuatService.cs       (đổi tên từ GeminiAIService.cs, Task 9)  trich-xuat
  DocumentSummaryService.cs, DocumentReviewService.cs, DocumentAdvisoryService.cs, PeriodicReportService.cs  (Task 3–6, 8)
  AppSettingsService.cs     (Task 10)  bỏ DevModePolicy và thiết lập chế độ gọi thẳng
  <lớp prompt thuần dot2b tách từ AIComposeDialog>  (XÓA, Task 8)
AIVanBan.Desktop/Views/
  TacVuGiaoDien.cs          (mới, Task 3; thêm DocChuTuPdfAsync ở Task 9)
  DocumentSummaryDialog.xaml.cs (3, 9), DocumentReviewDialog.xaml.cs (4, 9), DocumentAdvisoryDialog.xaml.cs (5, 9),
  PeriodicReportDialog.xaml.cs (6), AIComposeDialog.xaml.cs (7), ScanImportDialog.xaml.cs (9),
  ApiSettingsDialog.xaml + .xaml.cs (7, 10), LoginRegisterDialog.xaml.cs (10), HelpPage.xaml (11)
AIVanBan.Desktop/MainWindow.xaml.cs (10)
AIVanBan.Tests/
  HoTro/HandlerTacVuGia.cs  (mới, Task 2)  HttpMessageHandler giả dùng chung
  TacVuClientTests.cs       (mới, Task 2)
  TacVuDichVuTests.cs       (mới, Task 3; Task 4–7 thêm bài)
  TrichXuatServiceTests.cs  (mới, Task 9)
  TacVuThatTests.cs         (mới, Task 12)  chạy thật trên production, chỉ khi có VBP_E2E_API_KEY
  MauPromptTests.cs         (XÓA, Task 8)
  GeminiAIServiceTests.cs   (XÓA, Task 9)
PROJECT_STATUS.md, HUONG_DAN_SU_DUNG.md, CHANGELOG_v1.0.20.md (mới) — Task 11
AIVanBan.Desktop/AIVanBan.Desktop.csproj, setup-script.iss, setup-vanbanplus.iss, update.xml — Task 12
```

## Lệnh build / test và lưu ý WPF

Chạy trên PC Windows (Git Bash). Bộ kiểm `net9.0-windows` + `UseWPF` **không chạy trên máy chủ Linux** (`gikidi-dev`).

```bash
# Build: chỉ project Desktop (kéo theo Core). KHÔNG build cả AIVanBan.sln:
# sln có AIVanBan.Package (MSIX, cần Windows App SDK + chứng chỉ ký) và AIVanBan.API (máy chủ C# cũ), không thuộc đợt này.
cd /d/AIVanBanCaNhan && dotnet build AIVanBan.Desktop/AIVanBan.Desktop.csproj -nologo -v q 2>&1 | tail -5
# Mong đợi: "Build succeeded." và "0 Error(s)" (cảnh báo nullable có sẵn từ trước, bỏ qua).

# Kiểm thử (build Core + Tests, không build Desktop):
cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests/AIVanBan.Tests.csproj -nologo 2>&1 | tail -5
# Mong đợi: "Passed!  - Failed:     0".
```

Lưu ý:

- Tắt VanBanPlus đang chạy trước khi build (tệp `AIVanBan.Desktop.exe`/`AIVanBan.Core.dll` bị khóa → lỗi MSB3027/MSB3021 «file is locked»).
- Xóa `x:Name` hoặc handler sự kiện trong `.xaml` mà code-behind còn tham chiếu → lỗi CS0103/CS1061 lúc build Desktop; ngược lại handler còn trong XAML mà xóa trong `.cs` → lỗi CS1061 ở tệp `*.g.cs`. Sửa XAML và code-behind trong cùng bước, build Desktop ngay.
- Build Desktop sinh project tạm `*_wpftmp.csproj` cho XAML; lỗi XAML báo theo tệp `.xaml` gốc.
- `dotnet test` không biên dịch Desktop: mọi task đụng Desktop phải chạy cả lệnh build.

---

### Task 1: Cổng phụ thuộc + kiểm kê sau đợt 2

**Files:** không sửa tệp nào (chỉ tạo nhánh).

**Interfaces:**
- Consumes: production `https://vanbanplus.giakiemso.com`, repo đã gộp `dot2a`, `dot2b`, `dot2c`.
- Produces: nhánh `dot3-ban-cai-may-v2`; bản kiểm kê (ghi vào báo cáo task, không commit): tên lớp prompt dot2b, các hàm/lớp prompt dot2a đã đổi, danh sách hàm sinh trong `MauPromptTests.cs`, danh sách tệp mẫu đã commit.

- [ ] **Step 1: Kiểm 6 tác vụ đã có trên production (không cần khóa)**

```bash
U=https://vanbanplus.giakiemso.com
for t in tom-tat kiem-tra tham-muu bao-cao-dinh-ky trich-xuat soan-thao khong-co-tac-vu; do
  a=$(curl -s -o /dev/null -w "%{http_code}" -X POST -H "Content-Type: application/json" -d '{}' $U/api/v2/tac-vu/$t/uoc-tinh)
  b=$(curl -s -o /dev/null -w "%{http_code}" -X POST -H "Content-Type: application/json" -d '{}' $U/api/v2/tac-vu/$t)
  echo "$t uoc-tinh=$a chay=$b"
done
```

Mong đợi: 6 tác vụ thật đều `uoc-tinh=401 chay=401`; dòng đối chứng `khong-co-tac-vu uoc-tinh=404` (chứng tỏ máy chủ tra tác vụ trước khi xác thực, nên 401 nghĩa là tác vụ có thật; cột `chay` của dòng đối chứng chỉ để tham khảo).

**DỪNG nếu** bất kỳ tác vụ thật nào trả 404 (đợt 2 chưa lên production) hoặc `uoc-tinh` của dòng đối chứng không phải 404 (cổng không phân biệt được — báo chủ dự án, không tự đoán). Báo chủ dự án tác vụ nào thiếu; không làm tiếp.

- [ ] **Step 2: Repo sạch, nhánh mới, vanbanplus-api ở bản production**

```bash
cd /d/AIVanBanCaNhan && git switch main && git status --short && git log --oneline -8
git -C vanbanplus-api status --short && git -C vanbanplus-api fetch origin && git -C vanbanplus-api log --oneline -1 origin/main && git -C vanbanplus-api log --oneline -1 HEAD
```

Mong đợi: `git status` chỉ có các mục chưa theo dõi đã biết của phiên khác (`.superpowers/`, `docs/thue/`, tệp kế hoạch chưa commit) — không có tệp `.cs`/`.xaml` sửa dở. `vanbanplus-api` sạch. Nếu `HEAD` của `vanbanplus-api` khác `origin/main`: `git -C vanbanplus-api switch main && git -C vanbanplus-api pull --ff-only`. Có thay đổi dở ở đâu đó thì DỪNG và hỏi.

```bash
cd /d/AIVanBanCaNhan && git switch -c dot3-ban-cai-may-v2
```

- [ ] **Step 3: Kiểm kê thay đổi của đợt 2 (ghi kết quả vào báo cáo task)**

```bash
cd /d/AIVanBanCaNhan
# Lớp prompt thuần dot2b tách từ AIComposeDialog (tên chưa biết trước):
grep -rlE "CHUYÊN VIÊN VĂN THƯ CAO CẤP|BuildCongVanPrompt|BuildStructuredPrompt|BỐI CẢNH ĐỊA PHƯƠNG" AIVanBan.Core AIVanBan.Desktop --include=*.cs
# Hàm/lớp prompt của 4 service (dot2a có thể đổi sang internal static hoặc tách lớp thuần):
grep -rnE "(internal|private|public) static string Build[A-Za-z]*\(|class [A-Za-z]*Prompt" AIVanBan.Core --include=*.cs
# Bộ sinh tệp mẫu:
grep -nE "public void Ghi|[A-Z][A-Za-z]+\.(Build|Tao)[A-Za-z]*\(" AIVanBan.Tests/MauPromptTests.cs
git -C vanbanplus-api ls-files tests/mau-prompt
# Tên mới của đợt 3 chưa được dùng:
grep -rnE "class (TacVuClient|TacVuException|TrichXuatService|TacVuSoanThao|ThietLapDiaPhuong|TacVuGiaoDien|HandlerTacVuGia)\b|record DiaPhuongCoQuan|enum MaLoiTacVu" AIVanBan.Core AIVanBan.Desktop AIVanBan.Tests --include=*.cs
```

Mong đợi:
- Lệnh 1: `AIComposeDialog.xaml.cs` (nếu còn hàm bọc) và **một tệp trong `AIVanBan.Core`** — đó là «lớp prompt thuần dot2b tách từ AIComposeDialog», gọi tắt là **LỚP-DOT2B** trong kế hoạch này. Ghi đường dẫn và tên lớp.
- Lệnh 2: các `BuildSystemPrompt`/`BuildUserPrompt` của 4 service (và lớp prompt riêng nếu dot2a tách). Ghi lại; Task 8 xóa hết.
- Lệnh 4: có ít nhất `tom-tat.json`, `kiem-tra.json`, `tham-muu.json`, `bao-cao-dinh-ky.json`, `soan-thao.json`.
- Lệnh 5: trống. Nếu trùng tên, đổi tên mới của đợt 3 (thêm hậu tố `V2`) trong mọi task và ghi vào báo cáo.

- [ ] **Step 4: Mốc xanh trước khi sửa**

```bash
cd /d/AIVanBanCaNhan && dotnet build AIVanBan.Desktop/AIVanBan.Desktop.csproj -nologo -v q 2>&1 | tail -5
cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests/AIVanBan.Tests.csproj -nologo 2>&1 | tail -5
git -C vanbanplus-api status --short tests/mau-prompt
```

Mong đợi: build `0 Error(s)`; test `Passed!`; lệnh cuối trống (tệp mẫu C# sinh ra khớp từng ký tự với tệp đã commit — `MauPromptTests` sẽ làm hỏng bài và ghi tệp mới nếu lệch). Đỏ hoặc có tệp mẫu thay đổi → DỪNG, báo chủ dự án (đợt 2 chưa khép).

Không commit ở task này.

---

### Task 2: `TacVuClient` — client v2 dùng chung

**Files:**
- Create: `AIVanBan.Core/Services/TacVuException.cs`
- Create: `AIVanBan.Core/Services/TacVuClient.cs`
- Create: `AIVanBan.Tests/HoTro/HandlerTacVuGia.cs`
- Test: `AIVanBan.Tests/TacVuClientTests.cs`

**Interfaces:**
- Consumes: `AppSettingsService.Load()` (`VanBanPlusApiUrl`, `VanBanPlusApiKey`).
- Produces (namespace `AIVanBan.Core.Services`):
  - `public sealed record UocTinhKetQua(int CreditMin, int CreditMax, int SoDu, bool DuCredit)`
  - `public sealed record ThongTinLuot(int CreditDaTru, int SoDu, string MaLuot, bool BiCat)`
  - `public sealed record TacVuKetQua<T>(T KetQua, ThongTinLuot Luot)`
  - `public enum MaLoiTacVu { KhongDuCredit, QuaNhieuLuot, QuaDai, DauVaoSai, AiQuaTai, AiLoi, NgatKetNoi, ChuaDangNhap, KhongCoTacVu, LoiMayChu, HetThoiGian, MatKetNoi }`
  - `public sealed class TacVuException : Exception` — `Ma`, `MaHttp` (`int?`), `ThuLaiSau` (`TimeSpan?`), `LoiMayChu` (`string?`); `Message` là lời hiện cho khách.
  - `public sealed class TacVuClient` — `TacVuClient()`; `internal TacVuClient(HttpMessageHandler handler, string diaChi, string khoa, string phienBan, TimeSpan? hanChay = null)`; `Task<UocTinhKetQua> UocTinhAsync(string ten, object input, CancellationToken ct = default)`; `Task<TacVuKetQua<T>> ChayAsync<T>(string ten, object input, CancellationToken ct = default)`; `internal static MaLoiTacVu PhanLoai(int maHttp, string? code)`; `internal static TacVuException TaoLoi(MaLoiTacVu ma, int? maHttp, string ten, string? chiTiet, TimeSpan? thuLaiSau, TimeSpan han, Exception? goc = null)`.
  - Test: `internal sealed class HandlerTacVuGia` (namespace `AIVanBan.Tests.HoTro`) — `TraVe(HttpStatusCode, string json, Action<HttpResponseMessage>? sua = null)`, `ThanhCong(string ketQuaJson, int creditDaTru = 7, int soDu = 173, bool biCat = false)`, `DaNhan` (`List<YeuCau>`), `Treo`, `NemLoi`.

- [ ] **Step 1: Viết handler giả dùng chung**

`AIVanBan.Tests/HoTro/HandlerTacVuGia.cs`:

```csharp
using System.Net;
using System.Net.Http;
using System.Text;

namespace AIVanBan.Tests.HoTro;

/// <summary>
/// HttpMessageHandler giả cho client tác vụ v2: trả lần lượt các phản hồi đã định, ghi lại từng yêu cầu
/// (phương thức, URL, header, thân). Không có phản hồi nào trong hàng đợi thì bài kiểm hỏng rõ ràng.
/// </summary>
internal sealed class HandlerTacVuGia : HttpMessageHandler
{
    public sealed record YeuCau(HttpMethod Method, string Url, Dictionary<string, string> Header, string Body);

    private readonly Queue<Func<HttpResponseMessage>> _phanHoi = new();

    public List<YeuCau> DaNhan { get; } = new();

    /// <summary>true: không bao giờ trả lời (để thử hết hạn chờ).</summary>
    public bool Treo { get; init; }

    /// <summary>Khác null: ném lỗi này thay vì trả lời (để thử mất kết nối).</summary>
    public Exception? NemLoi { get; init; }

    public HandlerTacVuGia TraVe(HttpStatusCode ma, string json, Action<HttpResponseMessage>? sua = null)
    {
        _phanHoi.Enqueue(() =>
        {
            var r = new HttpResponseMessage(ma) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
            sua?.Invoke(r);
            return r;
        });
        return this;
    }

    public HandlerTacVuGia ThanhCong(string ketQuaJson, int creditDaTru = 7, int soDu = 173, bool biCat = false) =>
        TraVe(HttpStatusCode.OK,
            $$"""{"success":true,"message":"Thành công","data":{"ket_qua":{{ketQuaJson}},"credit_da_tru":{{creditDaTru}},"so_du":{{soDu}},"ma_luot":"luot-1","bi_cat":{{(biCat ? "true" : "false")}}}}""");

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var header = request.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value), StringComparer.OrdinalIgnoreCase);
        if (request.Content != null)
            foreach (var h in request.Content.Headers)
                header[h.Key] = string.Join(",", h.Value);
        var body = request.Content == null ? "" : await request.Content.ReadAsStringAsync(ct);
        DaNhan.Add(new YeuCau(request.Method, request.RequestUri!.ToString(), header, body));

        if (NemLoi != null) throw NemLoi;
        if (Treo) await Task.Delay(Timeout.Infinite, ct);
        return _phanHoi.Dequeue()();
    }
}
```

- [ ] **Step 2: Viết bài kiểm (đỏ)**

`AIVanBan.Tests/TacVuClientTests.cs`:

```csharp
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using AIVanBan.Tests.HoTro;

namespace AIVanBan.Tests;

public class TacVuClientTests
{
    private static TacVuClient TaoClient(HandlerTacVuGia h, TimeSpan? han = null, string khoa = "khoa-thu") =>
        new(h, "https://vbp.thu", khoa, "1.0.20", han);

    private static JsonElement Than(HandlerTacVuGia h, int i = 0)
    {
        using var doc = JsonDocument.Parse(h.DaNhan[i].Body);
        return doc.RootElement.Clone();
    }

    [Fact]
    public async Task ChayAsync_GuiDungDuongDanHeaderVaBoTruongNull()
    {
        var h = new HandlerTacVuGia().ThanhCong("\"kết quả\"");

        await TaoClient(h).ChayAsync<string>("tom-tat", new { noi_dung = "Điều 1.", tieu_de = (string?)null });

        var y = Assert.Single(h.DaNhan);
        Assert.Equal(HttpMethod.Post, y.Method);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/tom-tat", y.Url);
        Assert.Equal("khoa-thu", y.Header["X-API-Key"]);
        Assert.Equal("desktop", y.Header["X-VBP-Nguon"]);
        Assert.Equal("1.0.20", y.Header["X-VBP-Phien-Ban"]);
        Assert.StartsWith("application/json", y.Header["Content-Type"]);
        var b = Than(h);
        Assert.Equal("Điều 1.", b.GetProperty("noi_dung").GetString());
        Assert.False(b.TryGetProperty("tieu_de", out _));
    }

    [Fact]
    public async Task ChayAsync_DocKetQuaVaoModelCuVaThongTinLuot()
    {
        var h = new HandlerTacVuGia().ThanhCong(
            """{"brief":"Quyết định phê duyệt kế hoạch","key_points":[{"heading":"Điều 1","content":"Phê duyệt"}]}""",
            creditDaTru: 7, soDu: 173, biCat: true);

        var kq = await TaoClient(h).ChayAsync<DocumentSummary>("tom-tat", new { noi_dung = "x" });

        Assert.Equal("Quyết định phê duyệt kế hoạch", kq.KetQua.Brief);
        Assert.Equal("Điều 1", kq.KetQua.KeyPoints[0].Heading);
        Assert.Equal(new ThongTinLuot(7, 173, "luot-1", true), kq.Luot);
    }

    [Fact]
    public async Task ChayAsync_KetQuaChuoi()
    {
        var h = new HandlerTacVuGia().ThanhCong("\"I. KẾT QUẢ\\nII. PHƯƠNG HƯỚNG\"");

        var kq = await TaoClient(h).ChayAsync<string>("bao-cao-dinh-ky", new { loai_ky = "Tháng" });

        Assert.Equal("I. KẾT QUẢ\nII. PHƯƠNG HƯỚNG", kq.KetQua);
    }

    [Fact]
    public async Task ChayAsync_ThieuKetQua_BaoLoiMayChu()
    {
        var h = new HandlerTacVuGia().TraVe(HttpStatusCode.OK, """{"success":true,"data":{"credit_da_tru":1,"so_du":2}}""");

        var loi = await Assert.ThrowsAsync<TacVuException>(() => TaoClient(h).ChayAsync<string>("tom-tat", new { noi_dung = "x" }));

        Assert.Equal(MaLoiTacVu.LoiMayChu, loi.Ma);
    }

    [Fact]
    public async Task UocTinhAsync_DocKhoangCreditVaSoDu()
    {
        var h = new HandlerTacVuGia().TraVe(HttpStatusCode.OK,
            """{"success":true,"message":"Thành công","data":{"credit_min":6,"credit_max":9,"so_du":180,"du_credit":true}}""");

        var u = await TaoClient(h).UocTinhAsync("tom-tat", new { noi_dung = "x" });

        Assert.Equal(new UocTinhKetQua(6, 9, 180, true), u);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/tom-tat/uoc-tinh", h.DaNhan[0].Url);
        Assert.Equal("desktop", h.DaNhan[0].Header["X-VBP-Nguon"]);
    }

    [Theory]
    [InlineData(402, "KHONG_DU_CREDIT", MaLoiTacVu.KhongDuCredit)]
    [InlineData(429, "QUA_NHIEU_LUOT", MaLoiTacVu.QuaNhieuLuot)]
    [InlineData(413, "QUA_DAI", MaLoiTacVu.QuaDai)]
    [InlineData(400, "DAU_VAO_SAI", MaLoiTacVu.DauVaoSai)]
    [InlineData(503, "AI_QUA_TAI", MaLoiTacVu.AiQuaTai)]
    [InlineData(502, "AI_LOI", MaLoiTacVu.AiLoi)]
    [InlineData(499, "NGAT_KET_NOI", MaLoiTacVu.NgatKetNoi)]
    [InlineData(401, null, MaLoiTacVu.ChuaDangNhap)]
    [InlineData(404, null, MaLoiTacVu.KhongCoTacVu)]
    [InlineData(500, null, MaLoiTacVu.LoiMayChu)]
    [InlineData(413, null, MaLoiTacVu.QuaDai)]
    [InlineData(504, null, MaLoiTacVu.HetThoiGian)]
    [InlineData(503, null, MaLoiTacVu.LoiMayChu)]
    public async Task ChayAsync_LoiMayChu_DoiRaDungMaVaKhongTuThuLai(int maHttp, string? code, MaLoiTacVu mong)
    {
        var than = code is null
            ? """{"success":false,"message":"Lỗi"}"""
            : $$"""{"success":false,"message":"Lỗi","code":"{{code}}"}""";
        var h = new HandlerTacVuGia().TraVe((HttpStatusCode)maHttp, than);

        var loi = await Assert.ThrowsAsync<TacVuException>(() => TaoClient(h).ChayAsync<string>("tom-tat", new { noi_dung = "x" }));

        Assert.Equal(mong, loi.Ma);
        Assert.Equal(maHttp, loi.MaHttp);
        Assert.Single(h.DaNhan);
    }

    [Fact]
    public async Task AiQuaTai_DocRetryAfterVaGhiSoGiay()
    {
        var h = new HandlerTacVuGia().TraVe(HttpStatusCode.ServiceUnavailable,
            """{"success":false,"message":"Quá tải","code":"AI_QUA_TAI"}""",
            r => r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(20)));

        var loi = await Assert.ThrowsAsync<TacVuException>(() => TaoClient(h).ChayAsync<string>("tom-tat", new { noi_dung = "x" }));

        Assert.Equal(TimeSpan.FromSeconds(20), loi.ThuLaiSau);
        Assert.Contains("20 giây", loi.Message);
        Assert.Contains("hoàn lại", loi.Message);
    }

    [Fact]
    public async Task QuaDai_KemChiTietMayChu()
    {
        var h = new HandlerTacVuGia().TraVe((HttpStatusCode)413,
            """{"success":false,"message":"Tổng dung lượng 12 MB vượt 10 MB của gói.","code":"QUA_DAI"}""");

        var loi = await Assert.ThrowsAsync<TacVuException>(() => TaoClient(h).ChayAsync<string>("trich-xuat", new { tep = Array.Empty<object>() }));

        Assert.Contains("12 MB vượt 10 MB", loi.Message);
        Assert.Equal("Tổng dung lượng 12 MB vượt 10 MB của gói.", loi.LoiMayChu);
    }

    [Fact]
    public async Task KhoaRong_BaoChuaDangNhapKhongGoiMayChu()
    {
        var h = new HandlerTacVuGia();

        var loi = await Assert.ThrowsAsync<TacVuException>(() => TaoClient(h, khoa: "").UocTinhAsync("tom-tat", new { noi_dung = "x" }));

        Assert.Equal(MaLoiTacVu.ChuaDangNhap, loi.Ma);
        Assert.Empty(h.DaNhan);
    }

    [Fact]
    public async Task MayChuTreo_HetHanChoBaoHetThoiGian()
    {
        var h = new HandlerTacVuGia { Treo = true };

        var loi = await Assert.ThrowsAsync<TacVuException>(
            () => TaoClient(h, han: TimeSpan.FromMilliseconds(200)).ChayAsync<string>("tom-tat", new { noi_dung = "x" })
                .WaitAsync(TimeSpan.FromSeconds(10)));

        Assert.Equal(MaLoiTacVu.HetThoiGian, loi.Ma);
    }

    [Fact]
    public async Task MatMang_BaoMatKetNoi()
    {
        var h = new HandlerTacVuGia { NemLoi = new HttpRequestException("No such host is known.") };

        var loi = await Assert.ThrowsAsync<TacVuException>(() => TaoClient(h).ChayAsync<string>("tom-tat", new { noi_dung = "x" }));

        Assert.Equal(MaLoiTacVu.MatKetNoi, loi.Ma);
    }

    [Fact]
    public void HanChay_DuDaiChoTacVuDaiNhat()
    {
        Assert.True(TacVuClient.HanChayTheoTacVu("kiem-tra") >= TimeSpan.FromSeconds(150));
        Assert.True(TacVuClient.HanChayTheoTacVu("trich-xuat") >= TimeSpan.FromSeconds(300));
    }

    [Fact]
    public void LoiBaoKhach_CoDauVaKhongCoDaiTuNhanXung()
    {
        var cam = new Regex(@"\b(tôi|chúng tôi|ta|chúng ta|anh|chị|em|bạn|mình|dạ|vâng)\b|(^|\s)ạ([\s.,!?]|$)",
            RegexOptions.IgnoreCase);
        foreach (var ma in Enum.GetValues<MaLoiTacVu>())
        {
            foreach (int? maHttp in new int?[] { null, 500 })
            {
                var loi = TacVuClient.TaoLoi(ma, maHttp, "tom-tat", "chi tiết", TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(180));
                Assert.DoesNotMatch(cam, loi.Message);
                Assert.Matches("[ăâđêôơưàáạảãèéẹẻẽìíịỉĩòóọỏõùúụủũỳýỵỷỹ]", loi.Message.ToLowerInvariant());
            }
        }
    }
}
```

- [ ] **Step 3: Chạy để thấy đỏ**

Run: `cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests/AIVanBan.Tests.csproj -nologo --filter "FullyQualifiedName~TacVuClientTests" 2>&1 | tail -5`
Expected: lỗi biên dịch `The type or namespace name 'TacVuClient' could not be found`.

- [ ] **Step 4: Viết `TacVuException.cs`**

```csharp
namespace AIVanBan.Core.Services;

/// <summary>Mã lỗi tác vụ v2 (theo hợp đồng) cộng các lỗi phía máy: hết hạn chờ, mất kết nối.</summary>
public enum MaLoiTacVu
{
    /// <summary>402 KHONG_DU_CREDIT — chưa gọi AI, không trừ.</summary>
    KhongDuCredit,
    /// <summary>429 QUA_NHIEU_LUOT — đang có 3 lượt chạy cùng lúc, không trừ.</summary>
    QuaNhieuLuot,
    /// <summary>413 QUA_DAI — vượt giới hạn tác vụ hoặc dung lượng tệp của gói, không trừ.</summary>
    QuaDai,
    /// <summary>400 DAU_VAO_SAI — không trừ.</summary>
    DauVaoSai,
    /// <summary>503 AI_QUA_TAI + Retry-After — hoàn hết.</summary>
    AiQuaTai,
    /// <summary>502 AI_LOI — hoàn hết.</summary>
    AiLoi,
    /// <summary>499 NGAT_KET_NOI — hoàn hết.</summary>
    NgatKetNoi,
    /// <summary>401, hoặc máy chưa có khóa.</summary>
    ChuaDangNhap,
    /// <summary>404 — máy chủ chưa có tác vụ.</summary>
    KhongCoTacVu,
    /// <summary>500 và mọi phản hồi không đúng hợp đồng.</summary>
    LoiMayChu,
    /// <summary>Hết hạn chờ phía máy, hoặc 504 từ proxy.</summary>
    HetThoiGian,
    /// <summary>Không tới được máy chủ.</summary>
    MatKetNoi,
}

/// <summary>Lỗi khi gọi tác vụ v2. <see cref="Exception.Message"/> là lời hiện thẳng cho khách.</summary>
public sealed class TacVuException : Exception
{
    public TacVuException(MaLoiTacVu ma, string thongDiep, int? maHttp = null, TimeSpan? thuLaiSau = null,
        string? loiMayChu = null, Exception? goc = null)
        : base(thongDiep, goc)
    {
        Ma = ma;
        MaHttp = maHttp;
        ThuLaiSau = thuLaiSau;
        LoiMayChu = loiMayChu;
    }

    public MaLoiTacVu Ma { get; }

    /// <summary>Mã HTTP máy chủ trả; null khi lỗi xảy ra trước khi có phản hồi.</summary>
    public int? MaHttp { get; }

    /// <summary>Từ header Retry-After (AI_QUA_TAI).</summary>
    public TimeSpan? ThuLaiSau { get; }

    /// <summary>Trường message gốc của máy chủ (để ghi log / hỗ trợ).</summary>
    public string? LoiMayChu { get; }
}
```

- [ ] **Step 5: Viết `TacVuClient.cs`**

```csharp
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIVanBan.Core.Services;

/// <summary>Kết quả /uoc-tinh: khoảng credit dự kiến và số dư hiện có.</summary>
public sealed record UocTinhKetQua(int CreditMin, int CreditMax, int SoDu, bool DuCredit);

/// <summary>Thông tin một lượt chạy xong.</summary>
public sealed record ThongTinLuot(int CreditDaTru, int SoDu, string MaLuot, bool BiCat);

public sealed record TacVuKetQua<T>(T KetQua, ThongTinLuot Luot);

/// <summary>
/// Gọi API tác vụ v2 của VanBanPlus: POST /api/v2/tac-vu/&lt;ten&gt; và /uoc-tinh.
/// Bản cài máy chỉ gửi dữ liệu nghiệp vụ; máy chủ dựng prompt, gọi AI bằng khóa hệ thống và tính credit.
/// Không tự thử lại: mỗi lần chạy là một lượt giữ credit, để người dùng quyết định.
/// </summary>
public sealed class TacVuClient
{
    /// <summary>Hạn chờ /uoc-tinh (không gọi AI).</summary>
    public static readonly TimeSpan HanUocTinh = TimeSpan.FromSeconds(30);

    /// <summary>Hạn chờ một lượt chạy. Lượt dài nhất đo được 47 giây (spec §7), để rộng gần 4 lần.</summary>
    public static readonly TimeSpan HanChay = TimeSpan.FromSeconds(180);

    /// <summary>trich-xuat gửi tới 20 tệp, phần tải lên lâu hơn.</summary>
    public static readonly TimeSpan HanTrichXuat = TimeSpan.FromSeconds(300);

    private static readonly HttpClient HttpDungChung = new() { Timeout = Timeout.InfiniteTimeSpan };

    private static readonly JsonSerializerOptions TuyChonGui = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    internal static readonly JsonSerializerOptions TuyChonDoc = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    private readonly HttpClient _http;
    private readonly Func<(string DiaChi, string Khoa)> _ketNoi;
    private readonly string _phienBan;
    private readonly Func<string, TimeSpan> _hanChay;

    /// <summary>Đọc địa chỉ máy chủ và khóa từ settings.json ở MỖI lần gọi (đăng nhập lại có hiệu lực ngay).</summary>
    public TacVuClient() : this(HttpDungChung, DocKetNoiTuThietLap, PhienBanUngDung(), HanChayTheoTacVu) { }

    /// <summary>Cho bộ kiểm: handler giả, địa chỉ và khóa cố định, không đọc settings.json.</summary>
    internal TacVuClient(HttpMessageHandler handler, string diaChi, string khoa, string phienBan, TimeSpan? hanChay = null)
        : this(new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan },
               () => (diaChi.TrimEnd('/'), khoa),
               phienBan,
               hanChay is { } h ? _ => h : HanChayTheoTacVu)
    {
    }

    private TacVuClient(HttpClient http, Func<(string DiaChi, string Khoa)> ketNoi, string phienBan, Func<string, TimeSpan> hanChay)
    {
        _http = http;
        _ketNoi = ketNoi;
        _phienBan = phienBan;
        _hanChay = hanChay;
    }

    private static (string DiaChi, string Khoa) DocKetNoiTuThietLap()
    {
        var s = AppSettingsService.Load();
        return ((s.VanBanPlusApiUrl ?? "").Trim().TrimEnd('/'), (s.VanBanPlusApiKey ?? "").Trim());
    }

    /// <summary>Phiên bản ứng dụng gửi trong X-VBP-Phien-Ban, vd "1.0.20" (từ &lt;Version&gt; của AIVanBan.Desktop.csproj).</summary>
    internal static string PhienBanUngDung() =>
        Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "0.0.0";

    internal static TimeSpan HanChayTheoTacVu(string ten) => ten == "trich-xuat" ? HanTrichXuat : HanChay;

    /// <summary>POST /api/v2/tac-vu/&lt;ten&gt;/uoc-tinh — không gọi AI, không giữ credit.</summary>
    public async Task<UocTinhKetQua> UocTinhAsync(string ten, object input, CancellationToken ct = default)
    {
        var data = await GoiAsync(ten, $"/api/v2/tac-vu/{ten}/uoc-tinh", input, HanUocTinh, ct);
        return new UocTinhKetQua(
            DocSo(data, "credit_min"),
            DocSo(data, "credit_max"),
            DocSo(data, "so_du"),
            data.TryGetProperty("du_credit", out var du) && du.ValueKind == JsonValueKind.True);
    }

    /// <summary>POST /api/v2/tac-vu/&lt;ten&gt; — đọc data.ket_qua vào T (model C# hiện có, hoặc string).</summary>
    public async Task<TacVuKetQua<T>> ChayAsync<T>(string ten, object input, CancellationToken ct = default)
    {
        var han = _hanChay(ten);
        var data = await GoiAsync(ten, $"/api/v2/tac-vu/{ten}", input, han, ct);

        if (!data.TryGetProperty("ket_qua", out var kq) || kq.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            throw TaoLoi(MaLoiTacVu.LoiMayChu, 200, ten, "Thiếu ket_qua.", null, han);

        T? ketQua;
        try
        {
            ketQua = kq.Deserialize<T>(TuyChonDoc);
        }
        catch (JsonException ex)
        {
            throw TaoLoi(MaLoiTacVu.LoiMayChu, 200, ten, ex.Message, null, han, ex);
        }
        if (ketQua is null)
            throw TaoLoi(MaLoiTacVu.LoiMayChu, 200, ten, "ket_qua rỗng.", null, han);

        var luot = new ThongTinLuot(
            DocSo(data, "credit_da_tru"),
            DocSo(data, "so_du"),
            data.TryGetProperty("ma_luot", out var ml) && ml.ValueKind == JsonValueKind.String ? ml.GetString()! : "",
            data.TryGetProperty("bi_cat", out var bc) && bc.ValueKind == JsonValueKind.True);
        return new TacVuKetQua<T>(ketQua, luot);
    }

    private async Task<JsonElement> GoiAsync(string ten, string duongDan, object input, TimeSpan han, CancellationToken ct)
    {
        var (diaChi, khoa) = _ketNoi();
        if (string.IsNullOrWhiteSpace(khoa))
            throw TaoLoi(MaLoiTacVu.ChuaDangNhap, null, ten, null, null, han);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(han);

        using var yeuCau = new HttpRequestMessage(HttpMethod.Post, diaChi + duongDan)
        {
            Content = new StringContent(JsonSerializer.Serialize(input, TuyChonGui), Encoding.UTF8, "application/json"),
        };
        yeuCau.Headers.TryAddWithoutValidation("X-API-Key", khoa);
        yeuCau.Headers.TryAddWithoutValidation("X-VBP-Nguon", "desktop");
        yeuCau.Headers.TryAddWithoutValidation("X-VBP-Phien-Ban", _phienBan);

        HttpResponseMessage phanHoi;
        string than;
        try
        {
            phanHoi = await _http.SendAsync(yeuCau, cts.Token);
            than = await phanHoi.Content.ReadAsStringAsync(cts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw TaoLoi(MaLoiTacVu.HetThoiGian, null, ten, null, null, han);
        }
        catch (HttpRequestException ex)
        {
            throw TaoLoi(MaLoiTacVu.MatKetNoi, null, ten, ex.Message, null, han, ex);
        }

        using (phanHoi)
        {
            JsonElement? goc = null;
            try
            {
                using var doc = JsonDocument.Parse(than);
                goc = doc.RootElement.Clone();
            }
            catch (JsonException)
            {
                // Thân không phải JSON (vd trang lỗi của proxy): phân loại theo mã HTTP.
            }

            if (phanHoi.IsSuccessStatusCode
                && goc is { ValueKind: JsonValueKind.Object } g
                && g.TryGetProperty("success", out var s) && s.ValueKind == JsonValueKind.True
                && g.TryGetProperty("data", out var d) && d.ValueKind == JsonValueKind.Object)
            {
                return d;
            }

            string? code = null, message = null;
            if (goc is { ValueKind: JsonValueKind.Object } e)
            {
                if (e.TryGetProperty("code", out var c) && c.ValueKind == JsonValueKind.String) code = c.GetString();
                if (e.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String) message = m.GetString();
            }

            TimeSpan? thuLaiSau = phanHoi.Headers.RetryAfter?.Delta;
            if (thuLaiSau is null && phanHoi.Headers.RetryAfter?.Date is { } luc)
                thuLaiSau = luc - DateTimeOffset.UtcNow;

            var maHttp = (int)phanHoi.StatusCode;
            throw TaoLoi(PhanLoai(maHttp, code), maHttp, ten, message, thuLaiSau, han);
        }
    }

    private static int DocSo(JsonElement data, string ten)
    {
        if (!data.TryGetProperty(ten, out var v)) return 0;
        if (v.ValueKind == JsonValueKind.Number)
            return v.TryGetInt32(out var i) ? i : (int)Math.Ceiling(v.GetDouble());
        if (v.ValueKind == JsonValueKind.String && int.TryParse(v.GetString(), out var j)) return j;
        return 0;
    }

    /// <summary>Ưu tiên trường code của hợp đồng; không có code thì xét mã HTTP.</summary>
    internal static MaLoiTacVu PhanLoai(int maHttp, string? code) => code switch
    {
        "KHONG_DU_CREDIT" => MaLoiTacVu.KhongDuCredit,
        "QUA_NHIEU_LUOT" => MaLoiTacVu.QuaNhieuLuot,
        "QUA_DAI" => MaLoiTacVu.QuaDai,
        "DAU_VAO_SAI" => MaLoiTacVu.DauVaoSai,
        "AI_QUA_TAI" => MaLoiTacVu.AiQuaTai,
        "AI_LOI" => MaLoiTacVu.AiLoi,
        "NGAT_KET_NOI" => MaLoiTacVu.NgatKetNoi,
        _ => maHttp switch
        {
            400 => MaLoiTacVu.DauVaoSai,
            401 => MaLoiTacVu.ChuaDangNhap,
            402 => MaLoiTacVu.KhongDuCredit,
            404 => MaLoiTacVu.KhongCoTacVu,
            413 => MaLoiTacVu.QuaDai,
            429 => MaLoiTacVu.QuaNhieuLuot,
            499 => MaLoiTacVu.NgatKetNoi,
            504 => MaLoiTacVu.HetThoiGian,
            _ => MaLoiTacVu.LoiMayChu,
        },
    };

    /// <summary>Dựng lỗi kèm lời hiện cho khách (tiếng Việt có dấu, không đại từ nhân xưng).</summary>
    internal static TacVuException TaoLoi(MaLoiTacVu ma, int? maHttp, string ten, string? chiTiet,
        TimeSpan? thuLaiSau, TimeSpan han, Exception? goc = null)
    {
        static string KemChiTiet(string? s) => string.IsNullOrWhiteSpace(s) ? "" : $"\nChi tiết: {s}";

        var thongDiep = ma switch
        {
            MaLoiTacVu.KhongDuCredit =>
                "Số dư credit không đủ cho tác vụ này. Credit chưa bị trừ. Có thể mua thêm credit trên trang Bảng giá của VanBanPlus.",
            MaLoiTacVu.QuaNhieuLuot =>
                "Tài khoản đang có 3 tác vụ AI chạy cùng lúc. Chờ một tác vụ xong rồi thử lại. Credit chưa bị trừ.",
            MaLoiTacVu.QuaDai =>
                "Nội dung hoặc tệp gửi lên vượt giới hạn của gói hiện tại. Cần rút ngắn nội dung, giảm dung lượng tệp, bớt số trang hoặc nâng cấp gói. Credit chưa bị trừ."
                + KemChiTiet(chiTiet),
            MaLoiTacVu.DauVaoSai =>
                "Dữ liệu gửi lên chưa hợp lệ. Credit chưa bị trừ." + KemChiTiet(chiTiet),
            MaLoiTacVu.AiQuaTai =>
                "Máy chủ AI đang quá tải. Credit đã được hoàn lại. "
                + (thuLaiSau is { } t && t > TimeSpan.Zero
                    ? $"Thử lại sau khoảng {Math.Ceiling(t.TotalSeconds):0} giây."
                    : "Thử lại sau ít phút."),
            MaLoiTacVu.AiLoi =>
                "AI gặp lỗi khi xử lý. Credit đã được hoàn lại. Thử lại sau ít phút.",
            MaLoiTacVu.NgatKetNoi =>
                "Kết nối bị ngắt trước khi nhận được kết quả. Credit đã được hoàn lại. Thử lại.",
            MaLoiTacVu.ChuaDangNhap => maHttp is null
                ? "Máy này chưa đăng nhập tài khoản VanBanPlus. Cần bấm «Đăng nhập» trước khi dùng tính năng AI."
                : "Tài khoản VanBanPlus trên máy này không còn hợp lệ (mã 401). Cần bấm «Đăng nhập» để đăng nhập lại.",
            MaLoiTacVu.KhongCoTacVu =>
                $"Máy chủ VanBanPlus chưa có tác vụ «{ten}» (mã 404). Cần cập nhật phần mềm lên bản mới nhất hoặc liên hệ hỗ trợ.",
            MaLoiTacVu.HetThoiGian => maHttp == 504
                ? "Máy chủ phản hồi quá lâu (mã 504). Thử lại sau ít phút. Số dư credit xem ở trang Tài khoản trên web VanBanPlus."
                : $"Máy chủ phản hồi quá lâu (hơn {han.TotalSeconds:0} giây). Thử lại sau ít phút. Số dư credit xem ở trang Tài khoản trên web VanBanPlus.",
            MaLoiTacVu.MatKetNoi =>
                "Không kết nối được máy chủ VanBanPlus. Kiểm tra kết nối Internet rồi thử lại.",
            _ => maHttp is null or (>= 200 and < 300)
                ? "Máy chủ VanBanPlus trả kết quả không đúng định dạng. Thử lại sau ít phút."
                : $"Máy chủ VanBanPlus gặp lỗi (mã {maHttp}). Thử lại sau ít phút.",
        };

        return new TacVuException(ma, thongDiep, maHttp, thuLaiSau, chiTiet, goc);
    }
}
```

- [ ] **Step 6: Chạy bài kiểm, thấy xanh**

Run: `cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests/AIVanBan.Tests.csproj -nologo --filter "FullyQualifiedName~TacVuClientTests" 2>&1 | tail -5`
Expected: `Passed!  - Failed:     0, Passed:    25` (12 Fact + 13 InlineData).

- [ ] **Step 7: Toàn bộ xanh**

Run cả hai lệnh ở mục «Lệnh build / test». Expected: build `0 Error(s)`, test `Passed!`.

- [ ] **Step 8: Commit**

```bash
cd /d/AIVanBanCaNhan && git add AIVanBan.Core/Services/TacVuClient.cs AIVanBan.Core/Services/TacVuException.cs AIVanBan.Tests/HoTro/HandlerTacVuGia.cs AIVanBan.Tests/TacVuClientTests.cs
git commit -m "$(cat <<'EOF'
feat(core): TacVuClient goi API tac vu v2 (uoc-tinh, chay, ma loi, han cho 180/300 giay)

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
)"
```

---

### Task 3: `tom-tat` + giao diện credit dùng chung

**Files:**
- Modify: `AIVanBan.Core/Services/DocumentSummaryService.cs`
- Create: `AIVanBan.Desktop/Views/TacVuGiaoDien.cs`
- Modify: `AIVanBan.Desktop/Views/DocumentSummaryDialog.xaml.cs` (`QuickAction_Click`, `StartSummary`, `Retry_Click`)
- Test: `AIVanBan.Tests/TacVuDichVuTests.cs` (mới)

**Interfaces:**
- Consumes: `TacVuClient`, `UocTinhKetQua`, `ThongTinLuot`, `TacVuException`, `MaLoiTacVu` (Task 2).
- Produces:
  - `DocumentSummaryService()`, `DocumentSummaryService(TacVuClient tacVu)`; `ThongTinLuot? LuotCuoi`; `Task<UocTinhKetQua> UocTinhAsync(string content, string documentType = "", string title = "", string issuer = "")`; `Task<DocumentSummary> SummarizeAsync(...)` (chữ ký cũ). Hàm dựng cũ `DocumentSummaryService(GeminiAIService)` bị bỏ (không ai gọi).
  - `AIVanBan.Desktop.Views.TacVuGiaoDien` (static): `Task<bool> XacNhanCreditAsync(Window owner, string tenViec, Func<Task<UocTinhKetQua>> uocTinh, string? moTaThem = null)`; `string KhoangCredit(UocTinhKetQua u)`; `string MoTaLoi(Exception ex)`; `void HienLoi(Window owner, Exception ex, string tieuDe)`; `void MoiMuaCreditNeuThieu(Window owner, Exception ex)`; `void MoiMuaCredit(Window owner, string loiDan)`; `string DiaChiMuaCredit()`; `void CanhBaoNeuBiCat(Window owner, ThongTinLuot? luot)`.

- [ ] **Step 1: Bài kiểm service (đỏ)**

`AIVanBan.Tests/TacVuDichVuTests.cs` (Task 4–7 thêm bài vào lớp này, ngay trước dấu `}` cuối lớp):

```csharp
using System.Text.Json;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using AIVanBan.Tests.HoTro;

namespace AIVanBan.Tests;

/// <summary>Các service nghiệp vụ gửi đúng đầu vào theo hợp đồng v2 và trả lại model C# cũ.</summary>
public class TacVuDichVuTests
{
    private static TacVuClient TaoClient(HandlerTacVuGia h) => new(h, "https://vbp.thu", "khoa-thu", "1.0.20");

    private static JsonElement Than(HandlerTacVuGia h, int i = 0)
    {
        using var doc = JsonDocument.Parse(h.DaNhan[i].Body);
        return doc.RootElement.Clone();
    }

    [Fact]
    public async Task TomTat_GuiDuBonTruongVaTraModelCu()
    {
        var h = new HandlerTacVuGia().ThanhCong(
            """{"brief":"Quyết định phê duyệt kế hoạch","key_points":[{"heading":"Điều 1","content":"Phê duyệt"}],"legal_bases":["Luật Tổ chức chính quyền địa phương"]}""");
        var dv = new DocumentSummaryService(TaoClient(h));

        var kq = await dv.SummarizeAsync("Điều 1. Phê duyệt kế hoạch.", "Quyết định", "Về kế hoạch năm 2026", "UBND xã Minh An");

        Assert.Equal("Quyết định phê duyệt kế hoạch", kq.Brief);
        Assert.Equal("Điều 1", kq.KeyPoints[0].Heading);
        Assert.Single(kq.LegalBases);
        Assert.Equal(7, dv.LuotCuoi!.CreditDaTru);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/tom-tat", h.DaNhan[0].Url);
        var b = Than(h);
        Assert.Equal("Điều 1. Phê duyệt kế hoạch.", b.GetProperty("noi_dung").GetString());
        Assert.Equal("Quyết định", b.GetProperty("loai_van_ban").GetString());
        Assert.Equal("Về kế hoạch năm 2026", b.GetProperty("tieu_de").GetString());
        Assert.Equal("UBND xã Minh An", b.GetProperty("co_quan").GetString());
    }

    [Fact]
    public async Task TomTat_UocTinhCungDauVao()
    {
        var h = new HandlerTacVuGia().TraVe(System.Net.HttpStatusCode.OK,
            """{"success":true,"data":{"credit_min":4,"credit_max":13,"so_du":30,"du_credit":true}}""");

        var u = await new DocumentSummaryService(TaoClient(h)).UocTinhAsync("Điều 1.", "Quyết định");

        Assert.Equal(13, u.CreditMax);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/tom-tat/uoc-tinh", h.DaNhan[0].Url);
        Assert.Equal("Điều 1.", Than(h).GetProperty("noi_dung").GetString());
    }

    [Fact]
    public async Task TomTat_NoiDungRong_KhongGoiMayChu()
    {
        var h = new HandlerTacVuGia();

        await Assert.ThrowsAsync<ArgumentException>(() => new DocumentSummaryService(TaoClient(h)).SummarizeAsync("   "));

        Assert.Empty(h.DaNhan);
    }
}
```

Run: `cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests/AIVanBan.Tests.csproj -nologo --filter "FullyQualifiedName~TacVuDichVuTests" 2>&1 | tail -5`
Expected: lỗi biên dịch — `DocumentSummaryService` không có hàm dựng nhận `TacVuClient` / không có `UocTinhAsync`.

- [ ] **Step 2: Đổi `DocumentSummaryService` sang v2**

Trong `AIVanBan.Core/Services/DocumentSummaryService.cs`:

1. Thay khối từ dòng `    private readonly GeminiAIService _aiService;` tới hết phương thức `SummarizeAsync` (tới dấu `}` đóng nó, ngay trước `internal static string BuildSystemPrompt()`) bằng:

```csharp
    private readonly TacVuClient _tacVu;

    public DocumentSummaryService() : this(new TacVuClient()) { }

    public DocumentSummaryService(TacVuClient tacVu)
    {
        _tacVu = tacVu;
    }

    /// <summary>Credit đã trừ, số dư, cờ bị cắt của lượt chạy gần nhất.</summary>
    public ThongTinLuot? LuotCuoi { get; private set; }

    /// <summary>Khoảng credit dự kiến cho đúng đầu vào sẽ gửi (không gọi AI, không giữ credit).</summary>
    public Task<UocTinhKetQua> UocTinhAsync(string content, string documentType = "", string title = "", string issuer = "")
        => _tacVu.UocTinhAsync("tom-tat", DauVao(content, documentType, title, issuer));

    /// <summary>Tóm tắt văn bản qua tác vụ v2 tom-tat.</summary>
    public async Task<DocumentSummary> SummarizeAsync(
        string content,
        string documentType = "",
        string title = "",
        string issuer = "")
    {
        var kq = await _tacVu.ChayAsync<DocumentSummary>("tom-tat", DauVao(content, documentType, title, issuer));
        LuotCuoi = kq.Luot;
        return kq.KetQua;
    }

    private static object DauVao(string content, string documentType, string title, string issuer)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung văn bản không được để trống.");
        return new { noi_dung = content, loai_van_ban = documentType, tieu_de = title, co_quan = issuer };
    }
```

2. Xóa các phương thức không còn dùng: `ParseResult`, `TryRepairTruncatedJson`, `ExtractFieldsByRegex`, `ExtractStringArray` (máy chủ đã phân tích và kiểm JSON, thử lại 1 lần — spec §5).
3. **Giữ** `BuildSystemPrompt` và `BuildUserPrompt` (bộ sinh tệp mẫu còn dùng; Task 8 xóa).

Run lại lệnh Step 1. Expected: `Passed!  - Failed:     0, Passed:     3`.

- [ ] **Step 3: Viết `TacVuGiaoDien.cs`**

`AIVanBan.Desktop/Views/TacVuGiaoDien.cs`:

```csharp
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

/// <summary>
/// Giao diện dùng chung cho tác vụ AI v2: báo trước khoảng credit và số dư, báo lỗi theo mã,
/// mời mua credit khi thiếu. Lời hiện cho khách: không đại từ nhân xưng, không «dạ/vâng/ạ».
/// </summary>
public static class TacVuGiaoDien
{
    /// <summary>
    /// Gọi /uoc-tinh rồi hỏi xác nhận. Trả true khi được phép chạy. Thiếu credit thì mời mua và trả false;
    /// lỗi khi ước tính thì báo lỗi và trả false.
    /// </summary>
    public static async Task<bool> XacNhanCreditAsync(Window owner, string tenViec,
        Func<Task<UocTinhKetQua>> uocTinh, string? moTaThem = null)
    {
        UocTinhKetQua? uoc = null;
        Exception? loi = null;
        Mouse.OverrideCursor = Cursors.Wait;
        try
        {
            uoc = await uocTinh();
        }
        catch (Exception ex)
        {
            loi = ex;
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }

        if (loi != null || uoc == null)
        {
            HienLoi(owner, loi ?? new InvalidOperationException("Không ước tính được credit."), tenViec);
            return false;
        }

        if (!uoc.DuCredit)
        {
            MoiMuaCredit(owner,
                $"{tenViec} dự kiến dùng {KhoangCredit(uoc)}, số dư hiện có {uoc.SoDu} credit: không đủ để chạy. Credit chưa bị trừ.");
            return false;
        }

        var noiDung = (string.IsNullOrWhiteSpace(moTaThem) ? "" : moTaThem.TrimEnd() + "\n\n")
            + $"💳 {tenViec} dự kiến dùng {KhoangCredit(uoc)}.\n"
            + $"Số dư hiện có: {uoc.SoDu} credit.\n\n"
            + "Credit trừ theo mức dùng thật, không vượt mức trên; tác vụ lỗi được hoàn credit.\n\n"
            + "Tiếp tục?";
        return MessageBox.Show(owner, noiDung, "Xác nhận dùng credit",
            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    public static string KhoangCredit(UocTinhKetQua u) =>
        u.CreditMin == u.CreditMax ? $"{u.CreditMax} credit" : $"{u.CreditMin}–{u.CreditMax} credit";

    /// <summary>Lời báo lỗi để hiện trong khung lỗi của hộp thoại.</summary>
    public static string MoTaLoi(Exception ex) => ex switch
    {
        TacVuException t => t.Message,
        ArgumentException a => a.Message,
        _ => $"Lỗi: {ex.Message}",
    };

    /// <summary>Báo lỗi bằng hộp thoại; thiếu credit thì mời mua credit.</summary>
    public static void HienLoi(Window owner, Exception ex, string tieuDe)
    {
        if (ex is TacVuException { Ma: MaLoiTacVu.KhongDuCredit })
        {
            MoiMuaCredit(owner, ex.Message);
            return;
        }
        MessageBox.Show(owner, MoTaLoi(ex), tieuDe, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    /// <summary>Cho hộp thoại tự hiện lỗi trong khung: chỉ bật lời mời mua khi lỗi là thiếu credit.</summary>
    public static void MoiMuaCreditNeuThieu(Window owner, Exception ex)
    {
        if (ex is TacVuException { Ma: MaLoiTacVu.KhongDuCredit })
            MoiMuaCredit(owner, ex.Message);
    }

    public static void MoiMuaCredit(Window owner, string loiDan)
    {
        var url = DiaChiMuaCredit();
        var chon = MessageBox.Show(owner, $"{loiDan}\n\nMở trang mua credit?\n{url}", "Không đủ credit",
            MessageBoxButton.YesNo, MessageBoxImage.Information);
        if (chon != MessageBoxResult.Yes) return;
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(owner, $"Không mở được trình duyệt. Địa chỉ trang mua credit: {url}\n\n{ex.Message}",
                "Mua credit", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public static string DiaChiMuaCredit()
    {
        var goc = AppSettingsService.Load().VanBanPlusApiUrl;
        if (string.IsNullOrWhiteSpace(goc)) goc = "https://vanbanplus.giakiemso.com";
        return goc.Trim().TrimEnd('/') + "/bang-gia";
    }

    /// <summary>Kết quả chạm trần độ dài thì báo (vẫn tính là thành công, có trừ credit — spec §6).</summary>
    public static void CanhBaoNeuBiCat(Window owner, ThongTinLuot? luot)
    {
        if (luot?.BiCat == true)
        {
            MessageBox.Show(owner,
                "Kết quả dài vượt giới hạn nên bị cắt bớt ở cuối. Nên kiểm tra phần cuối trước khi dùng.",
                "Kết quả bị cắt", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
```

- [ ] **Step 4: Nối `DocumentSummaryDialog`**

Trong `AIVanBan.Desktop/Views/DocumentSummaryDialog.xaml.cs`:

1. Thêm trường ngay dưới `private int _elapsedSeconds;`:

```csharp
    private readonly DocumentSummaryService _service = new();
```

2. Trong `QuickAction_Click`, thay:

```csharp
        // Ẩn input panel, hiện loading
        pnlQuickInput.Visibility = Visibility.Collapsed;

        await StartSummary();
```

bằng:

```csharp
        if (!await XacNhanCreditAsync()) return;

        // Ẩn input panel, hiện loading
        pnlQuickInput.Visibility = Visibility.Collapsed;

        await StartSummary();
```

3. Trong `StartSummary`, thay khối `try { ... } catch (Exception ex) { ... }` (giữ `finally`) bằng:

```csharp
        try
        {
            _result = await _service.SummarizeAsync(_content, _documentType, _title, _issuer);
            DisplayResults(_result);
            TacVuGiaoDien.CanhBaoNeuBiCat(this, _service.LuotCuoi);
        }
        catch (Exception ex)
        {
            ShowError(TacVuGiaoDien.MoTaLoi(ex));
            TacVuGiaoDien.MoiMuaCreditNeuThieu(this, ex);
        }
```

4. Thay `Retry_Click`:

```csharp
    private async void Retry_Click(object sender, RoutedEventArgs e)
    {
        if (!await XacNhanCreditAsync()) return;
        btnCopy.Visibility = Visibility.Collapsed;
        await StartSummary();
    }

    private Task<bool> XacNhanCreditAsync() =>
        TacVuGiaoDien.XacNhanCreditAsync(this, "Tóm tắt văn bản",
            () => _service.UocTinhAsync(_content, _documentType, _title, _issuer));
```

(Nhánh `.pdf` của `UploadFile_Click` còn gọi `GeminiAIService.ReadTextFromFileAsync`; Task 9 đổi.)

- [ ] **Step 5: Build + test + soát lời**

```bash
cd /d/AIVanBanCaNhan && dotnet build AIVanBan.Desktop/AIVanBan.Desktop.csproj -nologo -v q 2>&1 | tail -5
cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests/AIVanBan.Tests.csproj -nologo 2>&1 | tail -5
git diff -U0 | grep "^+" | grep -nE "\b([Tt]ôi|[Tt]a|[Aa]nh|[Ee]m|[Bb]ạn|[Cc]hị|[Mm]ình|[Dd]ạ|[Vv]âng)\b|(^|\s)ạ[.,!? ]"
```

Expected: build `0 Error(s)`; test `Passed!`; lệnh soát trống (bỏ qua «tiếng Anh», tên riêng). Lệnh `git diff` không gồm tệp mới chưa add — chạy thêm `grep -nE "...(cùng mẫu)..." AIVanBan.Desktop/Views/TacVuGiaoDien.cs`, mong đợi trống.

- [ ] **Step 6: Commit**

```bash
cd /d/AIVanBanCaNhan && git add AIVanBan.Core/Services/DocumentSummaryService.cs AIVanBan.Desktop/Views/TacVuGiaoDien.cs AIVanBan.Desktop/Views/DocumentSummaryDialog.xaml.cs AIVanBan.Tests/TacVuDichVuTests.cs
git commit -m "$(cat <<'EOF'
feat(desktop): tom-tat goi v2; hop thoai bao truoc khoang credit va xu ly ma loi dung chung

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
)"
```

---

### Task 4: `kiem-tra`

**Files:**
- Modify: `AIVanBan.Core/Services/DocumentReviewService.cs`
- Modify: `AIVanBan.Desktop/Views/DocumentReviewDialog.xaml.cs` (`QuickCheck_Click`, `StartReview`)
- Test: `AIVanBan.Tests/TacVuDichVuTests.cs` (thêm bài)

**Interfaces:**
- Consumes: `TacVuClient` (Task 2), `TacVuGiaoDien` (Task 3).
- Produces: `DocumentReviewService()`, `DocumentReviewService(TacVuClient)`; `ThongTinLuot? LuotCuoi`; `Task<UocTinhKetQua> UocTinhAsync(string content, string documentType = "", string title = "", string issuer = "", string? referenceContent = null)`; `Task<DocumentReviewResult> ReviewDocumentAsync(...)` (chữ ký cũ).

- [ ] **Step 1: Bài kiểm (đỏ)** — thêm vào `TacVuDichVuTests`:

```csharp
    [Fact]
    public async Task KiemTra_GuiVanBanDoiChieu_VaBoMarkdownTrongBanDeXuat()
    {
        var h = new HandlerTacVuGia()
            .ThanhCong("""{"overall_score":7,"summary":"Còn lỗi thể thức","issues":[{"severity":"critical","category":"format","description":"Thiếu nơi nhận"}],"suggested_content":"**Điều 1.** Nội dung"}""")
            .ThanhCong("""{"overall_score":9,"summary":"Tốt","issues":[]}""");
        var dv = new DocumentReviewService(TaoClient(h));

        var kq = await dv.ReviewDocumentAsync("Nội dung cần soát", "Công văn", "V/v báo cáo", "UBND xã Minh An", "Văn bản mẫu");

        Assert.Equal(7, kq.OverallScore);
        Assert.Equal(1, kq.CriticalCount);
        Assert.Equal("Điều 1. Nội dung", kq.SuggestedContent);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/kiem-tra", h.DaNhan[0].Url);
        var b = Than(h, 0);
        Assert.Equal("Nội dung cần soát", b.GetProperty("noi_dung").GetString());
        Assert.Equal("Công văn", b.GetProperty("loai_van_ban").GetString());
        Assert.Equal("Văn bản mẫu", b.GetProperty("van_ban_doi_chieu").GetString());

        await dv.ReviewDocumentAsync("Nội dung cần soát");
        Assert.False(Than(h, 1).TryGetProperty("van_ban_doi_chieu", out _));
    }
```

Run: `dotnet test ... --filter "FullyQualifiedName~TacVuDichVuTests"` → Expected: lỗi biên dịch (không có hàm dựng nhận `TacVuClient`).

- [ ] **Step 2: Đổi `DocumentReviewService`**

Thay khối từ `    private readonly GeminiAIService _aiService;` tới hết phương thức `ReviewDocumentAsync` bằng:

```csharp
    private readonly TacVuClient _tacVu;

    public DocumentReviewService() : this(new TacVuClient()) { }

    public DocumentReviewService(TacVuClient tacVu)
    {
        _tacVu = tacVu;
    }

    /// <summary>Credit đã trừ, số dư, cờ bị cắt của lượt chạy gần nhất.</summary>
    public ThongTinLuot? LuotCuoi { get; private set; }

    public Task<UocTinhKetQua> UocTinhAsync(string content, string documentType = "", string title = "",
        string issuer = "", string? referenceContent = null)
        => _tacVu.UocTinhAsync("kiem-tra", DauVao(content, documentType, title, issuer, referenceContent));

    /// <summary>Kiểm tra nội dung văn bản qua tác vụ v2 kiem-tra.</summary>
    /// <param name="referenceContent">Nội dung văn bản mẫu để đối chiếu (tùy chọn).</param>
    public async Task<DocumentReviewResult> ReviewDocumentAsync(
        string content,
        string documentType = "",
        string title = "",
        string issuer = "",
        string? referenceContent = null)
    {
        var kq = await _tacVu.ChayAsync<DocumentReviewResult>("kiem-tra",
            DauVao(content, documentType, title, issuer, referenceContent));
        LuotCuoi = kq.Luot;

        var result = kq.KetQua;
        // Hậu xử lý hiển thị (không phải prompt): văn bản hành chính là chữ thuần, bỏ ký hiệu markdown.
        if (!string.IsNullOrWhiteSpace(result.SuggestedContent))
            result.SuggestedContent = StripMarkdown(result.SuggestedContent);
        return result;
    }

    private static object DauVao(string content, string documentType, string title, string issuer, string? referenceContent)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung văn bản không được để trống.");
        return new
        {
            noi_dung = content,
            loai_van_ban = documentType,
            tieu_de = title,
            co_quan = issuer,
            van_ban_doi_chieu = string.IsNullOrWhiteSpace(referenceContent) ? null : referenceContent,
        };
    }
```

Xóa `ParseReviewResult`. Giữ `StripMarkdown`, `BuildSystemPrompt`, `BuildUserPrompt` (prompt xóa ở Task 8).

Run bài kiểm → Expected: `Passed!`.

- [ ] **Step 3: Nối `DocumentReviewDialog`**

1. Thêm trường dưới `private string? _uploadedFilePath;`:

```csharp
    private readonly DocumentReviewService _service = new();
```

2. Trong `QuickCheck_Click`, ngay trước dòng `// Ẩn input panel, hiện loading` chèn:

```csharp
        if (!await TacVuGiaoDien.XacNhanCreditAsync(this, "Kiểm tra văn bản",
                () => _service.UocTinhAsync(_content, _documentType, _title, _issuer, _referenceContent)))
            return;

```

3. Trong `StartReview`, thay khối `try { ... } catch (Exception ex) { ... }` (giữ `finally`) bằng:

```csharp
        try
        {
            _result = await _service.ReviewDocumentAsync(_content, _documentType, _title, _issuer, _referenceContent);
            DisplayResults(_result);
            TacVuGiaoDien.CanhBaoNeuBiCat(this, _service.LuotCuoi);
        }
        catch (Exception ex)
        {
            ShowError(TacVuGiaoDien.MoTaLoi(ex));
            TacVuGiaoDien.MoiMuaCreditNeuThieu(this, ex);
        }
```

Kiểm: `grep -n "StartReview()" AIVanBan.Desktop/Views/DocumentReviewDialog.xaml.cs` — chỉ có lời gọi trong `QuickCheck_Click` (nếu có thêm nơi gọi, chèn cùng bước xác nhận trước đó).

- [ ] **Step 4: Build + test + soát lời** (ba lệnh như Task 3 Step 5). Expected: xanh, soát trống.

- [ ] **Step 5: Commit**

```bash
cd /d/AIVanBanCaNhan && git add AIVanBan.Core/Services/DocumentReviewService.cs AIVanBan.Desktop/Views/DocumentReviewDialog.xaml.cs AIVanBan.Tests/TacVuDichVuTests.cs
git commit -m "$(cat <<'EOF'
feat(desktop): kiem-tra goi v2 (van_ban_doi_chieu), bao truoc credit

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
)"
```

---

### Task 5: `tham-muu` + ánh xạ `ngu_canh`

**Files:**
- Modify: `AIVanBan.Core/Services/DocumentAdvisoryService.cs`
- Modify: `AIVanBan.Desktop/Views/DocumentAdvisoryDialog.xaml.cs` (`QuickAction_Click`, `StartAdvisory`, `Retry_Click`)
- Test: `AIVanBan.Tests/TacVuDichVuTests.cs`

**Interfaces:**
- Consumes: `TacVuClient`, `TacVuGiaoDien`.
- Produces: `DocumentAdvisoryService()`, `DocumentAdvisoryService(TacVuClient)`; `ThongTinLuot? LuotCuoi`; `Task<UocTinhKetQua> UocTinhAsync(string content, string documentType = "", string title = "", string issuer = "", DocumentAdvisoryContext? context = null)`; `Task<DocumentAdvisory> AdviseAsync(...)` (chữ ký cũ); `internal static Dictionary<string, object>? TaoNguCanh(DocumentAdvisoryContext? c)`.

Ánh xạ (đúng hợp đồng): `huong`←Direction, `nhom`←Category, `so_van_ban`←DocumentNumber, `ngay_ban_hanh`←IssuedDate (ISO), `do_khan`←UrgencyLevel, `do_mat`←SecurityLevel, `han_xu_ly`←DueDate (ISO), `nguoi_ky`←SignerName, `chuc_danh_ky`←SignerTitle, `noi_nhan`←Recipients (mảng), `giao_cho`←AssignedTo, `ghi_chu_xu_ly`←ProcessingNotes, `can_cu`←BasedOn (mảng), `van_ban_lien_quan`←RelatedDocumentsSummary. Trường rỗng/null và mảng rỗng: không gửi (giống prompt cũ bỏ qua bằng `IsNullOrWhiteSpace`/`Length > 0`; hợp đồng coi thiếu = rỗng). Việc tính «còn N ngày» trước đây làm trong C# nay máy chủ làm.

- [ ] **Step 1: Bài kiểm (đỏ)** — thêm vào `TacVuDichVuTests`:

```csharp
    [Fact]
    public async Task ThamMuu_AnhXaNguCanhDungHopDong()
    {
        var h = new HandlerTacVuGia().ThanhCong(
            """{"summary":"UBND tỉnh yêu cầu báo cáo","urgency_level":"khan","action_items":["Bước 1: Vào sổ"],"deadlines":[{"task":"Gửi báo cáo","date":"20/11/2026"}],"response_needed":true,"priority":"high"}""");
        var ctx = new DocumentAdvisoryContext
        {
            Direction = "Đến",
            Category = "Đất đai",
            DocumentNumber = "15/UBND-KT",
            IssuedDate = new DateTime(2026, 11, 3, 14, 30, 0),
            UrgencyLevel = "Khẩn",
            SecurityLevel = "Thường",
            DueDate = new DateTime(2026, 11, 20),
            SignerName = "Nguyễn Văn A",
            SignerTitle = "CHỦ TỊCH",
            Recipients = new[] { "UBND các xã", "Lưu: VT" },
            AssignedTo = "Địa chính",
            ProcessingNotes = "Khẩn trương thực hiện",
            BasedOn = new[] { "Luật Đất đai 2024" },
            RelatedDocumentsSummary = "Công văn 10/UBND ngày 01/10/2026",
        };
        var dv = new DocumentAdvisoryService(TaoClient(h));

        var kq = await dv.AdviseAsync("Đề nghị báo cáo trước 20/11/2026.", "Công văn", "V/v báo cáo", "UBND tỉnh Đồng Nai", ctx);

        Assert.Equal("UBND tỉnh yêu cầu báo cáo", kq.Summary);
        Assert.True(kq.ResponseNeeded);
        Assert.Equal("20/11/2026", kq.Deadlines[0].Date);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/tham-muu", h.DaNhan[0].Url);
        var b = Than(h);
        Assert.Equal("UBND tỉnh Đồng Nai", b.GetProperty("co_quan").GetString());
        var n = b.GetProperty("ngu_canh");
        Assert.Equal("Đến", n.GetProperty("huong").GetString());
        Assert.Equal("Đất đai", n.GetProperty("nhom").GetString());
        Assert.Equal("15/UBND-KT", n.GetProperty("so_van_ban").GetString());
        Assert.Equal("2026-11-03", n.GetProperty("ngay_ban_hanh").GetString());
        Assert.Equal("Khẩn", n.GetProperty("do_khan").GetString());
        Assert.Equal("Thường", n.GetProperty("do_mat").GetString());
        Assert.Equal("2026-11-20", n.GetProperty("han_xu_ly").GetString());
        Assert.Equal("Nguyễn Văn A", n.GetProperty("nguoi_ky").GetString());
        Assert.Equal("CHỦ TỊCH", n.GetProperty("chuc_danh_ky").GetString());
        Assert.Equal(2, n.GetProperty("noi_nhan").GetArrayLength());
        Assert.Equal("Lưu: VT", n.GetProperty("noi_nhan")[1].GetString());
        Assert.Equal("Địa chính", n.GetProperty("giao_cho").GetString());
        Assert.Equal("Khẩn trương thực hiện", n.GetProperty("ghi_chu_xu_ly").GetString());
        Assert.Equal("Luật Đất đai 2024", n.GetProperty("can_cu")[0].GetString());
        Assert.Equal("Công văn 10/UBND ngày 01/10/2026", n.GetProperty("van_ban_lien_quan").GetString());
    }

    [Fact]
    public void ThamMuu_NguCanhChiGuiTruongCoGiaTri()
    {
        Assert.Null(DocumentAdvisoryService.TaoNguCanh(null));

        var d = DocumentAdvisoryService.TaoNguCanh(new DocumentAdvisoryContext
        {
            Direction = "Đến",
            Category = "  ",
            Recipients = Array.Empty<string>(),
        })!;

        Assert.Equal(new[] { "huong" }, d.Keys.ToArray());
    }

    [Fact]
    public async Task ThamMuu_KhongCoNguCanh_KhongGuiTruong()
    {
        var h = new HandlerTacVuGia().ThanhCong("""{"summary":"Tóm tắt"}""");

        await new DocumentAdvisoryService(TaoClient(h)).AdviseAsync("Nội dung văn bản đến");

        Assert.False(Than(h).TryGetProperty("ngu_canh", out _));
    }
```

Run → Expected: lỗi biên dịch.

- [ ] **Step 2: Đổi `DocumentAdvisoryService`**

Thêm `using System.Globalization;` ở đầu tệp. Thay khối từ `    private readonly GeminiAIService _aiService;` tới hết phương thức `AdviseAsync` bằng:

```csharp
    private readonly TacVuClient _tacVu;

    public DocumentAdvisoryService() : this(new TacVuClient()) { }

    public DocumentAdvisoryService(TacVuClient tacVu)
    {
        _tacVu = tacVu;
    }

    /// <summary>Credit đã trừ, số dư, cờ bị cắt của lượt chạy gần nhất.</summary>
    public ThongTinLuot? LuotCuoi { get; private set; }

    public Task<UocTinhKetQua> UocTinhAsync(string content, string documentType = "", string title = "",
        string issuer = "", DocumentAdvisoryContext? context = null)
        => _tacVu.UocTinhAsync("tham-muu", DauVao(content, documentType, title, issuer, context));

    /// <summary>Phân tích văn bản đến và tham mưu xử lý qua tác vụ v2 tham-muu.</summary>
    public async Task<DocumentAdvisory> AdviseAsync(
        string content,
        string documentType = "",
        string title = "",
        string issuer = "",
        DocumentAdvisoryContext? context = null)
    {
        var kq = await _tacVu.ChayAsync<DocumentAdvisory>("tham-muu", DauVao(content, documentType, title, issuer, context));
        LuotCuoi = kq.Luot;
        return kq.KetQua;
    }

    private static object DauVao(string content, string documentType, string title, string issuer, DocumentAdvisoryContext? context)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung văn bản không được để trống.");
        return new
        {
            noi_dung = content,
            loai_van_ban = documentType,
            tieu_de = title,
            co_quan = issuer,
            ngu_canh = TaoNguCanh(context),
        };
    }

    /// <summary>
    /// DocumentAdvisoryContext → ngu_canh theo hợp đồng v2. Ngày gửi dạng ISO yyyy-MM-dd.
    /// Chỉ gửi trường có giá trị (chuỗi không trắng, mảng không rỗng).
    /// </summary>
    internal static Dictionary<string, object>? TaoNguCanh(DocumentAdvisoryContext? c)
    {
        if (c == null) return null;
        var d = new Dictionary<string, object>();
        void Chuoi(string khoa, string? v) { if (!string.IsNullOrWhiteSpace(v)) d[khoa] = v; }
        void Ngay(string khoa, DateTime? v) { if (v.HasValue) d[khoa] = v.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); }
        void Mang(string khoa, string[]? v) { if (v is { Length: > 0 }) d[khoa] = v; }

        Chuoi("huong", c.Direction);
        Chuoi("nhom", c.Category);
        Chuoi("so_van_ban", c.DocumentNumber);
        Ngay("ngay_ban_hanh", c.IssuedDate);
        Chuoi("do_khan", c.UrgencyLevel);
        Chuoi("do_mat", c.SecurityLevel);
        Ngay("han_xu_ly", c.DueDate);
        Chuoi("nguoi_ky", c.SignerName);
        Chuoi("chuc_danh_ky", c.SignerTitle);
        Mang("noi_nhan", c.Recipients);
        Chuoi("giao_cho", c.AssignedTo);
        Chuoi("ghi_chu_xu_ly", c.ProcessingNotes);
        Mang("can_cu", c.BasedOn);
        Chuoi("van_ban_lien_quan", c.RelatedDocumentsSummary);
        return d;
    }
```

Xóa `ParseResult`. Giữ `BuildSystemPrompt`, `BuildUserPrompt` (Task 8 xóa). Giữ nguyên lớp `DocumentAdvisoryContext` và `FromDocument`.

Run bài kiểm → Expected: `Passed!`.

- [ ] **Step 3: Nối `DocumentAdvisoryDialog`**

1. Thêm trường dưới `private int _elapsedSeconds;`:

```csharp
    private readonly DocumentAdvisoryService _service = new();
```

2. Trong `QuickAction_Click`, ngay trước `// Ẩn input panel, hiện loading` chèn:

```csharp
        if (!await XacNhanCreditAsync()) return;

```

3. Trong `StartAdvisory`, thay khối `try/catch` (giữ `finally`):

```csharp
        try
        {
            _result = await _service.AdviseAsync(_content, _documentType, _title, _issuer, _context);
            DisplayResults(_result);
            TacVuGiaoDien.CanhBaoNeuBiCat(this, _service.LuotCuoi);
        }
        catch (Exception ex)
        {
            ShowError(TacVuGiaoDien.MoTaLoi(ex));
            TacVuGiaoDien.MoiMuaCreditNeuThieu(this, ex);
        }
```

4. Trong `Retry_Click`, chèn dòng đầu thân hàm `if (!await XacNhanCreditAsync()) return;` (giữ các dòng cũ phía sau), và thêm phương thức:

```csharp
    private Task<bool> XacNhanCreditAsync() =>
        TacVuGiaoDien.XacNhanCreditAsync(this, "Tham mưu xử lý văn bản",
            () => _service.UocTinhAsync(_content, _documentType, _title, _issuer, _context));
```

- [ ] **Step 4: Build + test + soát lời.** Expected: xanh.

- [ ] **Step 5: Commit**

```bash
cd /d/AIVanBanCaNhan && git add AIVanBan.Core/Services/DocumentAdvisoryService.cs AIVanBan.Desktop/Views/DocumentAdvisoryDialog.xaml.cs AIVanBan.Tests/TacVuDichVuTests.cs
git commit -m "$(cat <<'EOF'
feat(desktop): tham-muu goi v2, chuyen DocumentAdvisoryContext sang ngu_canh (ngay ISO)

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
)"
```

---

### Task 6: `bao-cao-dinh-ky`

**Files:**
- Modify: `AIVanBan.Core/Services/PeriodicReportService.cs`
- Modify: `AIVanBan.Desktop/Views/PeriodicReportDialog.xaml.cs` (`Generate_Click`)
- Test: `AIVanBan.Tests/TacVuDichVuTests.cs`

**Interfaces:**
- Consumes: `TacVuClient`, `TacVuGiaoDien`.
- Produces: `PeriodicReportService(TacVuClient? tacVu = null)` (thay `PeriodicReportService(GeminiAIService? aiService = null)`); `ThongTinLuot? LuotCuoi`; `Task<UocTinhKetQua> UocTinhAsync(string reportPeriodType, string reportPeriod, string field, string orgName, string rawData, string? previousReport, string signerName, string signerTitle)`; `Task<string> GenerateReportAsync(...)` (chữ ký cũ). Các hàm tĩnh (`GetCommonFields`, `GetPeriodTypes`, `GetPeriodSuggestions`, `ExtractStatsFromDB`, …) giữ nguyên.

- [ ] **Step 1: Bài kiểm (đỏ)**:

```csharp
    [Fact]
    public async Task BaoCaoDinhKy_GuiDuTruongVaTraChuoi()
    {
        var h = new HandlerTacVuGia()
            .ThanhCong("\"Thực hiện chỉ đạo của UBND tỉnh...\\nI. KẾT QUẢ ĐẠT ĐƯỢC\"")
            .ThanhCong("\"Nội dung\"");
        var dv = new PeriodicReportService(TaoClient(h));

        var kq = await dv.GenerateReportAsync("Tháng", "Tháng 10/2026", "Kinh tế - Xã hội", "UBND xã Minh An",
            "Thu ngân sách 1,2 tỷ đồng", "Kỳ trước 1,0 tỷ đồng", "Nguyễn Văn An", "CHỦ TỊCH");

        Assert.StartsWith("Thực hiện chỉ đạo", kq);
        Assert.Contains("\nI. KẾT QUẢ", kq);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/bao-cao-dinh-ky", h.DaNhan[0].Url);
        var b = Than(h, 0);
        Assert.Equal("Tháng", b.GetProperty("loai_ky").GetString());
        Assert.Equal("Tháng 10/2026", b.GetProperty("ky").GetString());
        Assert.Equal("Kinh tế - Xã hội", b.GetProperty("linh_vuc").GetString());
        Assert.Equal("UBND xã Minh An", b.GetProperty("don_vi").GetString());
        Assert.Equal("Thu ngân sách 1,2 tỷ đồng", b.GetProperty("so_lieu").GetString());
        Assert.Equal("Kỳ trước 1,0 tỷ đồng", b.GetProperty("bao_cao_ky_truoc").GetString());
        Assert.Equal("Nguyễn Văn An", b.GetProperty("nguoi_ky").GetString());
        Assert.Equal("CHỦ TỊCH", b.GetProperty("chuc_danh").GetString());

        await dv.GenerateReportAsync("Quý", "Quý IV/2026", "CCHC", "UBND xã Minh An", "Số liệu", null, "", "");
        Assert.False(Than(h, 1).TryGetProperty("bao_cao_ky_truoc", out _));
    }
```

Run → Expected: lỗi biên dịch (hàm dựng nhận `GeminiAIService`).

- [ ] **Step 2: Đổi `PeriodicReportService`**

Thay khối từ `    private readonly GeminiAIService _aiService;` tới hết phương thức `GenerateReportAsync` bằng:

```csharp
    private readonly TacVuClient _tacVu;

    public PeriodicReportService(TacVuClient? tacVu = null)
    {
        _tacVu = tacVu ?? new TacVuClient();
    }

    /// <summary>Credit đã trừ, số dư, cờ bị cắt của lượt chạy gần nhất.</summary>
    public ThongTinLuot? LuotCuoi { get; private set; }

    public Task<UocTinhKetQua> UocTinhAsync(string reportPeriodType, string reportPeriod, string field, string orgName,
        string rawData, string? previousReport, string signerName, string signerTitle)
        => _tacVu.UocTinhAsync("bao-cao-dinh-ky",
            DauVao(reportPeriodType, reportPeriod, field, orgName, rawData, previousReport, signerName, signerTitle));

    /// <summary>Tạo báo cáo định kỳ từ số liệu qua tác vụ v2 bao-cao-dinh-ky.</summary>
    public async Task<string> GenerateReportAsync(
        string reportPeriodType,   // Tuần / Tháng / Quý / Năm
        string reportPeriod,       // VD: "Tháng 02/2026", "Quý I/2026"
        string field,              // Lĩnh vực: KT-XH, CCHC, Tài chính...
        string orgName,            // Tên đơn vị
        string rawData,            // Số liệu thô (paste)
        string? previousReport,    // Nội dung BC kỳ trước (nếu có)
        string signerName,         // Người ký
        string signerTitle)        // Chức danh
    {
        var kq = await _tacVu.ChayAsync<string>("bao-cao-dinh-ky",
            DauVao(reportPeriodType, reportPeriod, field, orgName, rawData, previousReport, signerName, signerTitle));
        LuotCuoi = kq.Luot;
        return kq.KetQua;
    }

    private static object DauVao(string reportPeriodType, string reportPeriod, string field, string orgName,
        string rawData, string? previousReport, string signerName, string signerTitle) => new
    {
        loai_ky = reportPeriodType,
        ky = reportPeriod,
        linh_vuc = field,
        don_vi = orgName,
        so_lieu = rawData,
        bao_cao_ky_truoc = string.IsNullOrWhiteSpace(previousReport) ? null : previousReport,
        nguoi_ky = signerName,
        chuc_danh = signerTitle,
    };
```

Giữ `BuildSystemPrompt`, `BuildUserPrompt` (Task 8 xóa) và mọi hàm tĩnh phía sau.

Run bài kiểm → Expected: `Passed!`.

- [ ] **Step 3: Nối `PeriodicReportDialog.Generate_Click`**

Thay đoạn từ dòng `var confirmMsg = $"📊 Tạo báo cáo {periodType.ToLower()} — {field}\n" +` tới hết khối `if (MessageBox.Show(confirmMsg, ...) != MessageBoxResult.OK) return;` bằng:

```csharp
        var moTa = $"📊 Tạo báo cáo {periodType.ToLower()} — {field}\n" +
                   $"📅 Kỳ: {period}\n" +
                   $"🏛️ Đơn vị: {txtOrgName.Text}\n" +
                   $"📋 So sánh kỳ trước: {(hasPrevious ? "Có" : "Không")}";
        var kyTruoc = hasPrevious ? txtPreviousReport.Text : null;

        if (!await TacVuGiaoDien.XacNhanCreditAsync(this, "Báo cáo định kỳ",
                () => _reportService.UocTinhAsync(periodType, period, field, txtOrgName.Text, txtRawData.Text,
                    kyTruoc, txtSignerName.Text, txtSignerTitle.Text),
                moTa))
            return;
```

Trong khối `try` ngay sau đó, thay lời gọi `_reportService.GenerateReportAsync(...)` (tham số `string.IsNullOrWhiteSpace(txtPreviousReport.Text) ? null : txtPreviousReport.Text` đổi thành `kyTruoc`), và sau dòng `ActionButtons.Visibility = Visibility.Visible;` thêm `TacVuGiaoDien.CanhBaoNeuBiCat(this, _reportService.LuotCuoi);`. Trong `catch`, thay `MessageBox.Show($"Lỗi khi gọi AI:\n\n{ex.Message}", ...)` bằng `TacVuGiaoDien.HienLoi(this, ex, "Lỗi AI");` (giữ hai dòng đổi `Visibility` phía trước).

Kiểm `_reportService` khai báo `PeriodicReportService` và được tạo `new PeriodicReportService()` trong hàm dựng (không đổi).

- [ ] **Step 4: Build + test + soát lời.** Expected: xanh.

- [ ] **Step 5: Commit**

```bash
cd /d/AIVanBanCaNhan && git add AIVanBan.Core/Services/PeriodicReportService.cs AIVanBan.Desktop/Views/PeriodicReportDialog.xaml.cs AIVanBan.Tests/TacVuDichVuTests.cs
git commit -m "$(cat <<'EOF'
feat(desktop): bao-cao-dinh-ky goi v2, gop xac nhan credit vao hop thoai xac nhan cu

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
)"
```

---

### Task 7: `soan-thao` + thiết lập địa phương

**Files:**
- Create: `AIVanBan.Core/Services/ThietLapDiaPhuong.cs`
- Create: `AIVanBan.Core/Services/TacVuSoanThao.cs`
- Modify: `AIVanBan.Desktop/Views/ApiSettingsDialog.xaml` (tab «🏛️ Cơ quan»), `AIVanBan.Desktop/Views/ApiSettingsDialog.xaml.cs` (`LoadOrganizationTab`, `Save_Click`)
- Modify: `AIVanBan.Desktop/Views/AIComposeDialog.xaml.cs`
- Test: `AIVanBan.Tests/TacVuDichVuTests.cs`

**Interfaces:**
- Consumes: `TacVuClient`, `TacVuGiaoDien`, `AppSettingsService.Load/Save`, `DocumentType` (`AIVanBan.Core.Models`).
- Produces:
  - `public sealed record DiaPhuongCoQuan(string Xa, string CoQuanCapTren, string Tinh)`
  - `public static class ThietLapDiaPhuong` — hằng `KhoaXa = "Org.DiaPhuong.Xa"`, `KhoaCoQuanCapTren = "Org.DiaPhuong.CoQuanCapTren"`, `KhoaTinh = "Org.DiaPhuong.Tinh"`; `MacDinhGiaKiem`; `DiaPhuongCoQuan? Doc(IReadOnlyDictionary<string, string> extra)`; `DiaPhuongCoQuan? Doc()`; `void Luu(string xa, string coQuanCapTren, string tinh)`.
  - `public sealed class TacVuSoanThao` — `TacVuSoanThao()`, `TacVuSoanThao(TacVuClient)`; `ThongTinLuot? LuotCuoi`; `Task<UocTinhKetQua> UocTinhAsync(DocumentType loai, IReadOnlyDictionary<string, string> truong, DiaPhuongCoQuan? diaPhuong)`; `Task<string> SoanAsync(DocumentType loai, IReadOnlyDictionary<string, string> truong, DiaPhuongCoQuan? diaPhuong)`.
  - `AIComposeDialog(DocumentService documentService, string? preSelectedTemplateId = null)` (bỏ tham số `geminiApiKey`).

Quy tắc mặc định địa phương (theo yêu cầu: «mặc định Gia Kiệm chỉ khi trống»): **chưa từng lưu** (không có khóa nào trong `ExtraSettings`) → `xã Gia Kiệm` / `thành phố Thống Nhất` / `tỉnh Đồng Nai` (giữ đúng hành vi các bản trước, giá trị trùng bộ kiểm so khớp của hợp đồng); **đã lưu mà cả ba rỗng** → không gửi `dia_phuong` (prompt không có bối cảnh địa phương, spec §10); đã lưu một phần → gửi đúng phần đã lưu (máy chủ coi `""` là không có).

- [ ] **Step 1: Bài kiểm (đỏ)**:

```csharp
    [Fact]
    public void DiaPhuong_ChuaTungLuu_MacDinhGiaKiem()
    {
        var dp = ThietLapDiaPhuong.Doc(new Dictionary<string, string>());

        Assert.Equal(new DiaPhuongCoQuan("xã Gia Kiệm", "thành phố Thống Nhất", "tỉnh Đồng Nai"), dp);
    }

    [Fact]
    public void DiaPhuong_DaLuuRongCaBa_KhongGui()
    {
        var dp = ThietLapDiaPhuong.Doc(new Dictionary<string, string>
        {
            [ThietLapDiaPhuong.KhoaXa] = "",
            [ThietLapDiaPhuong.KhoaCoQuanCapTren] = " ",
            [ThietLapDiaPhuong.KhoaTinh] = "",
        });

        Assert.Null(dp);
    }

    [Fact]
    public void DiaPhuong_DaLuuMotPhan_GiuDungPhanDaLuu()
    {
        var dp = ThietLapDiaPhuong.Doc(new Dictionary<string, string> { [ThietLapDiaPhuong.KhoaXa] = "  phường Long Bình " });

        Assert.Equal(new DiaPhuongCoQuan("phường Long Bình", "", ""), dp);
    }

    [Fact]
    public async Task SoanThao_GuiTenEnumTruongVaDiaPhuong()
    {
        var h = new HandlerTacVuGia()
            .ThanhCong("\"CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM\\nĐộc lập - Tự do - Hạnh phúc\"")
            .ThanhCong("\"Nội dung\"");
        var dv = new TacVuSoanThao(TaoClient(h));
        var truong = new Dictionary<string, string> { ["subject"] = "Báo cáo phòng chống thiên tai", ["signer_name"] = "" };

        var kq = await dv.SoanAsync(DocumentType.CongVan, truong, new DiaPhuongCoQuan("xã Minh An", "tỉnh Đồng Nai", "tỉnh Đồng Nai"));

        Assert.StartsWith("CỘNG HÒA", kq);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/soan-thao", h.DaNhan[0].Url);
        var b = Than(h, 0);
        Assert.Equal("CongVan", b.GetProperty("loai_van_ban").GetString());
        Assert.Equal("Báo cáo phòng chống thiên tai", b.GetProperty("truong").GetProperty("subject").GetString());
        Assert.Equal("", b.GetProperty("truong").GetProperty("signer_name").GetString());
        var dp = b.GetProperty("dia_phuong");
        Assert.Equal("xã Minh An", dp.GetProperty("xa").GetString());
        Assert.Equal("tỉnh Đồng Nai", dp.GetProperty("co_quan_cap_tren").GetString());
        Assert.Equal("tỉnh Đồng Nai", dp.GetProperty("tinh").GetString());

        await dv.SoanAsync(DocumentType.QuyetDinh, truong, null);
        Assert.False(Than(h, 1).TryGetProperty("dia_phuong", out _));
        Assert.Equal("QuyetDinh", Than(h, 1).GetProperty("loai_van_ban").GetString());
    }
```

Run → Expected: lỗi biên dịch (`ThietLapDiaPhuong`, `TacVuSoanThao` chưa có).

- [ ] **Step 2: Viết `ThietLapDiaPhuong.cs`**

```csharp
namespace AIVanBan.Core.Services;

/// <summary>Địa phương của cơ quan, gửi trong dia_phuong của tác vụ soan-thao. Ghi cả chữ «xã», «phường», «tỉnh»...</summary>
public sealed record DiaPhuongCoQuan(string Xa, string CoQuanCapTren, string Tinh);

/// <summary>
/// Đọc/ghi địa phương ở ExtraSettings của settings.json (tab «🏛️ Cơ quan» trong Cài đặt).
/// Chưa từng lưu → mặc định Gia Kiệm (giữ hành vi các bản trước). Đã lưu mà cả ba rỗng → null (không gửi dia_phuong).
/// </summary>
public static class ThietLapDiaPhuong
{
    public const string KhoaXa = "Org.DiaPhuong.Xa";
    public const string KhoaCoQuanCapTren = "Org.DiaPhuong.CoQuanCapTren";
    public const string KhoaTinh = "Org.DiaPhuong.Tinh";

    public static readonly DiaPhuongCoQuan MacDinhGiaKiem = new("xã Gia Kiệm", "thành phố Thống Nhất", "tỉnh Đồng Nai");

    public static DiaPhuongCoQuan? Doc(IReadOnlyDictionary<string, string> extra)
    {
        var daLuu = extra.ContainsKey(KhoaXa) || extra.ContainsKey(KhoaCoQuanCapTren) || extra.ContainsKey(KhoaTinh);
        if (!daLuu) return MacDinhGiaKiem;

        string Lay(string khoa) => extra.TryGetValue(khoa, out var v) ? (v ?? "").Trim() : "";
        var dp = new DiaPhuongCoQuan(Lay(KhoaXa), Lay(KhoaCoQuanCapTren), Lay(KhoaTinh));
        return dp.Xa.Length == 0 && dp.CoQuanCapTren.Length == 0 && dp.Tinh.Length == 0 ? null : dp;
    }

    public static DiaPhuongCoQuan? Doc() => Doc(AppSettingsService.Load().ExtraSettings);

    public static void Luu(string xa, string coQuanCapTren, string tinh)
    {
        var s = AppSettingsService.Load();
        s.ExtraSettings[KhoaXa] = (xa ?? "").Trim();
        s.ExtraSettings[KhoaCoQuanCapTren] = (coQuanCapTren ?? "").Trim();
        s.ExtraSettings[KhoaTinh] = (tinh ?? "").Trim();
        AppSettingsService.Save(s);
    }
}
```

- [ ] **Step 3: Viết `TacVuSoanThao.cs`**

```csharp
using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Soạn văn bản qua tác vụ v2 soan-thao. Prompt (26 nhánh theo loại văn bản) nằm ở máy chủ;
/// bản cài máy chỉ gửi loại văn bản (tên enum DocumentType), các trường người dùng nhập, và địa phương.
/// </summary>
public sealed class TacVuSoanThao
{
    private readonly TacVuClient _tacVu;

    public TacVuSoanThao() : this(new TacVuClient()) { }

    public TacVuSoanThao(TacVuClient tacVu)
    {
        _tacVu = tacVu;
    }

    /// <summary>Credit đã trừ, số dư, cờ bị cắt của lượt chạy gần nhất.</summary>
    public ThongTinLuot? LuotCuoi { get; private set; }

    public Task<UocTinhKetQua> UocTinhAsync(DocumentType loai, IReadOnlyDictionary<string, string> truong, DiaPhuongCoQuan? diaPhuong)
        => _tacVu.UocTinhAsync("soan-thao", DauVao(loai, truong, diaPhuong));

    public async Task<string> SoanAsync(DocumentType loai, IReadOnlyDictionary<string, string> truong, DiaPhuongCoQuan? diaPhuong)
    {
        var kq = await _tacVu.ChayAsync<string>("soan-thao", DauVao(loai, truong, diaPhuong));
        LuotCuoi = kq.Luot;
        return kq.KetQua;
    }

    private static object DauVao(DocumentType loai, IReadOnlyDictionary<string, string> truong, DiaPhuongCoQuan? dp) => new
    {
        loai_van_ban = loai.ToString(),
        truong,
        dia_phuong = dp is null ? null : new { xa = dp.Xa, co_quan_cap_tren = dp.CoQuanCapTren, tinh = dp.Tinh },
    };
}
```

Run bài kiểm `TacVuDichVuTests` → Expected: `Passed!`.

- [ ] **Step 4: Thêm ba ô địa phương vào Cài đặt → «🏛️ Cơ quan»**

`ApiSettingsDialog.xaml`: chèn ngay sau `</GroupBox>` đóng GroupBox `Header="Thông tin cơ quan"` (trước `<GroupBox Header="Người ký mặc định"`):

```xml
                        <GroupBox Header="Địa phương (dùng cho AI soạn thảo)" Margin="0,0,0,12"
                                  materialDesign:ColorZoneAssist.Mode="PrimaryLight">
                            <StackPanel Margin="8">
                                <TextBlock Text="Xã/phường" FontSize="12" FontWeight="SemiBold" Margin="0,0,0,4"/>
                                <TextBox x:Name="txtDpXa"
                                         materialDesign:HintAssist.Hint="VD: xã Gia Kiệm, phường Long Bình"
                                         Style="{StaticResource MaterialDesignOutlinedTextBox}"
                                         Margin="0,0,0,12"/>

                                <TextBlock Text="Cơ quan cấp trên" FontSize="12" FontWeight="SemiBold" Margin="0,0,0,4"/>
                                <TextBox x:Name="txtDpCapTren"
                                         materialDesign:HintAssist.Hint="VD: thành phố Thống Nhất, tỉnh Đồng Nai"
                                         Style="{StaticResource MaterialDesignOutlinedTextBox}"
                                         Margin="0,0,0,12"/>

                                <TextBlock Text="Tỉnh/thành phố" FontSize="12" FontWeight="SemiBold" Margin="0,0,0,4"/>
                                <TextBox x:Name="txtDpTinh"
                                         materialDesign:HintAssist.Hint="VD: tỉnh Đồng Nai"
                                         Style="{StaticResource MaterialDesignOutlinedTextBox}"
                                         Margin="0,0,0,4"/>
                                <TextBlock Text="💡 Ghi cả chữ «xã», «phường», «tỉnh»... Để trống cả ba thì AI soạn thảo không kèm bối cảnh địa phương."
                                           FontSize="11" Opacity="0.6" TextWrapping="Wrap"/>
                            </StackPanel>
                        </GroupBox>
```

`ApiSettingsDialog.xaml.cs`:
- Trong `LoadOrganizationTab`, ngay sau dòng `txtDefaultSigningTitle.Text = AppSettingsService.GetRawSettingValue(KEY_DEFAULT_SIGNING_TITLE) ?? "";` chèn:

```csharp
            var diaPhuong = ThietLapDiaPhuong.Doc();
            txtDpXa.Text = diaPhuong?.Xa ?? "";
            txtDpCapTren.Text = diaPhuong?.CoQuanCapTren ?? "";
            txtDpTinh.Text = diaPhuong?.Tinh ?? "";
```

- Trong `Save_Click`, ngay sau dòng `SaveOrganizationTab();` chèn:

```csharp
        ThietLapDiaPhuong.Luu(txtDpXa.Text, txtDpCapTren.Text, txtDpTinh.Text);
```

(Lưu ngoài `SaveOrganizationTab` vì hàm đó `return` sớm khi tên và viết tắt cơ quan đều trống.)

- [ ] **Step 5: Đổi `AIComposeDialog` sang `soan-thao`**

1. Thay trường `private readonly GeminiAIService _aiService;` bằng:

```csharp
    private readonly TacVuSoanThao _soanThao = new();
```

2. Hàm dựng: đổi chữ ký thành `public AIComposeDialog(DocumentService documentService, string? preSelectedTemplateId = null)` và xóa dòng `_aiService = string.IsNullOrEmpty(geminiApiKey) ? ... ;`. Kiểm hai nơi gọi vẫn biên dịch: `AIGeneratorPage.xaml.cs:29` (`new AIComposeDialog(_documentService)`), `TemplateManagementPage.xaml.cs:191` (`preSelectedTemplateId: template.Id`). Nếu `grep -rn "new AIComposeDialog(" AIVanBan.Desktop` cho thấy nơi nào truyền tham số thứ hai theo vị trí, đổi sang tham số có tên.

3. Trong `Generate_Click`, ngay trước dòng `// Show loading` chèn:

```csharp
        var loai = _selectedTemplate.Type;
        var truong = _fieldInputs.ToDictionary(kv => kv.Key, kv => kv.Value.Text);
        var diaPhuong = ThietLapDiaPhuong.Doc();
        if (!await TacVuGiaoDien.XacNhanCreditAsync(this, "Soạn văn bản",
                () => _soanThao.UocTinhAsync(loai, truong, diaPhuong)))
            return;

```

4. Trong khối `try` của `Generate_Click`, thay toàn bộ đoạn từ dòng dựng prompt (hiện là `// Build prompt từ template` / `var prompt = BuildPrompt();`; sau dot2b có thể là lời gọi LỚP-DOT2B) tới hết dòng `var content = await _aiService.GenerateContentAsync(prompt, systemInstruction);` bằng:

```csharp
            var content = await _soanThao.SoanAsync(loai, truong, diaPhuong);
            TacVuGiaoDien.CanhBaoNeuBiCat(this, _soanThao.LuotCuoi);
```

5. Trong `catch (Exception ex)` của `Generate_Click`, thay `MessageBox.Show($"Lỗi khi tạo nội dung với AI:\n\n{ex.Message}", ...)` bằng `TacVuGiaoDien.HienLoi(this, ex, "Lỗi AI");`.

6. Xóa mọi hàm dựng prompt còn trong dialog (dot2b có thể để lại hàm bọc):

```bash
cd /d/AIVanBanCaNhan && grep -nE "private (static )?string (BuildSystemInstruction|BuildPrompt|BuildStructuredPrompt|Build[A-Za-z]+Prompt)\(" AIVanBan.Desktop/Views/AIComposeDialog.xaml.cs
```

Xóa trọn từng phương thức tìm thấy (không đụng `BuildTemplatePreviewContent`, `BuildDefaultRecipients` — không khớp mẫu trên). LỚP-DOT2B trong Core **chưa xóa** (bộ sinh tệp mẫu còn dùng; Task 8 xóa).

- [ ] **Step 6: Build + test + soát lời**

```bash
cd /d/AIVanBanCaNhan && dotnet build AIVanBan.Desktop/AIVanBan.Desktop.csproj -nologo -v q 2>&1 | tail -5
cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests/AIVanBan.Tests.csproj -nologo 2>&1 | tail -5
grep -n "GenerateContentAsync\|_aiService" AIVanBan.Desktop/Views/AIComposeDialog.xaml.cs
```

Expected: build `0 Error(s)`; test `Passed!`; lệnh grep trống. Soát lời trên `git diff -U0` và hai tệp mới như Task 3.

- [ ] **Step 7: Commit**

```bash
cd /d/AIVanBanCaNhan && git add AIVanBan.Core/Services/ThietLapDiaPhuong.cs AIVanBan.Core/Services/TacVuSoanThao.cs AIVanBan.Desktop/Views/ApiSettingsDialog.xaml AIVanBan.Desktop/Views/ApiSettingsDialog.xaml.cs AIVanBan.Desktop/Views/AIComposeDialog.xaml.cs AIVanBan.Tests/TacVuDichVuTests.cs
git commit -m "$(cat <<'EOF'
feat(desktop): soan-thao goi v2; dia_phuong lay tu Cai dat > Co quan (mac dinh Gia Kiem khi chua luu)

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
)"
```

---

### Task 8: Xóa prompt khỏi C# và bộ sinh tệp mẫu

Sau Task 3–7 không đường gọi nào dùng prompt C#; chỉ `MauPromptTests` còn tham chiếu. Tệp mẫu đã commit ở `vanbanplus-api/tests/mau-prompt/*.json` trở thành mốc cố định (hợp đồng, mục «Tệp mẫu prompt»).

**Files:**
- Delete: `AIVanBan.Tests/MauPromptTests.cs`
- Delete: LỚP-DOT2B (đường dẫn ghi ở Task 1 Step 3) và mọi lớp prompt thuần dot2a đã tách (nếu có)
- Modify: `AIVanBan.Core/Services/DocumentSummaryService.cs`, `DocumentReviewService.cs`, `DocumentAdvisoryService.cs`, `PeriodicReportService.cs`

**Interfaces:**
- Consumes: kiểm kê Task 1 Step 3.
- Produces: C# không còn prompt nghiệp vụ của 4 service + soạn thảo. (Prompt trích xuất gọi thẳng Gemini trong `GeminiAIService.cs` xóa ở Task 9.)

- [ ] **Step 1: Xác nhận mốc cố định khớp lần cuối**

```bash
cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests/AIVanBan.Tests.csproj -nologo --filter "FullyQualifiedName~MauPromptTests" 2>&1 | tail -3
git -C vanbanplus-api status --short tests/mau-prompt
git -C vanbanplus-api ls-files tests/mau-prompt
```

Expected: `Passed!`; `status` trống; `ls-files` liệt kê đủ tệp như Task 1. Có tệp đổi → DỪNG (prompt C# đã lệch với máy chủ sau Task 1 — điều tra trước khi xóa).

- [ ] **Step 2: Xóa bộ sinh và lớp prompt**

```bash
cd /d/AIVanBanCaNhan && git rm AIVanBan.Tests/MauPromptTests.cs
git rm <đường dẫn LỚP-DOT2B ghi ở Task 1>
# và từng tệp lớp prompt thuần dot2a đã tách (nếu kiểm kê Task 1 có)
```

- [ ] **Step 3: Xóa hàm prompt trong 4 service**

Trong mỗi tệp `DocumentSummaryService.cs`, `DocumentReviewService.cs`, `DocumentAdvisoryService.cs`, `PeriodicReportService.cs`: xóa trọn phương thức `BuildSystemPrompt` và `BuildUserPrompt` (bất kể `private`/`internal static`). Bỏ `using System.Text.Json;` nếu tệp không còn dùng (chỉ để gọn; không bắt buộc).

- [ ] **Step 4: Không còn prompt nghiệp vụ**

```bash
cd /d/AIVanBanCaNhan && grep -rnE "BuildSystemPrompt|BuildUserPrompt|BuildSystemInstruction|Build[A-Za-z]*Prompt\(|Bạn là CHUY|Bạn là CHÁNH|MauPrompt|mau-prompt" AIVanBan.Core AIVanBan.Desktop AIVanBan.Tests --include=*.cs
```

Expected: trống. (`GeminiAIService.cs` còn prompt OCR «Bạn là chuyên gia OCR…» chữ thường — Task 9 xóa cùng tệp.)

- [ ] **Step 5: Build + test**

Expected: build `0 Error(s)`; test `Passed!` (số bài giảm đúng bằng số hàm sinh trong `MauPromptTests`).

- [ ] **Step 6: Commit**

```bash
cd /d/AIVanBanCaNhan && git add -u AIVanBan.Core AIVanBan.Desktop AIVanBan.Tests
git commit -m "$(cat <<'EOF'
refactor: xoa prompt nghiep vu khoi C# va bo sinh tep mau (tep mau o vanbanplus-api thanh moc co dinh)

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
)"
```

---

### Task 9: `trich-xuat` — `GeminiAIService` → `TrichXuatService`

**Files:**
- Rename + rewrite: `AIVanBan.Core/Services/GeminiAIService.cs` → `AIVanBan.Core/Services/TrichXuatService.cs`
- Modify: `AIVanBan.Desktop/Views/TacVuGiaoDien.cs` (thêm `DocChuTuPdfAsync`)
- Modify: `AIVanBan.Desktop/Views/ScanImportDialog.xaml.cs`
- Modify: `AIVanBan.Desktop/Views/DocumentSummaryDialog.xaml.cs`, `DocumentReviewDialog.xaml.cs` (2 chỗ), `DocumentAdvisoryDialog.xaml.cs` — nhánh `.pdf`
- Delete: `AIVanBan.Tests/GeminiAIServiceTests.cs`
- Test: `AIVanBan.Tests/TrichXuatServiceTests.cs`

**Interfaces:**
- Consumes: `TacVuClient`, `TacVuGiaoDien`.
- Produces:
  - `public class TrichXuatService` — `const int SoTepToiDa = 20`; `TrichXuatService()`, `TrichXuatService(TacVuClient)`; `ThongTinLuot? LuotCuoi`; lớp lồng `ExtractedDocumentData` (giữ nguyên thuộc tính); `static (string Base64, string MimeType) DocTep(string filePath)`; `Task<UocTinhKetQua> UocTinhAsync(IReadOnlyList<(string Base64, string MimeType)> tep, bool chiChu = false)`; `Task<ExtractedDocumentData> ExtractDocumentFromMultipleFilesAsync(IReadOnlyList<(string Base64, string MimeType)> files)`; `Task<ExtractedDocumentData> ExtractDocumentFromBytesAsync(string base64Data, string mimeType)`; `Task<ExtractedDocumentData> ExtractDocumentFromFileAsync(string filePath)`; `Task<string> ReadTextAsync(string base64Data, string mimeType)`; `Task<string> ReadTextFromFileAsync(string filePath)`.
  - `TacVuGiaoDien.DocChuTuPdfAsync(Window owner, string filePath) : Task<string?>` (null = đã hủy hoặc đã báo lỗi).
  - `ScanImportDialog(DocumentService documentService)` (bỏ tham số `geminiApiKey`).
- Lớp `GeminiAIService` không còn tồn tại.

- [ ] **Step 1: Bài kiểm (đỏ)**

`AIVanBan.Tests/TrichXuatServiceTests.cs`:

```csharp
using System.Text.Json;
using AIVanBan.Core.Services;
using AIVanBan.Tests.HoTro;

namespace AIVanBan.Tests;

public class TrichXuatServiceTests
{
    private static TrichXuatService TaoDichVu(HandlerTacVuGia h) =>
        new(new TacVuClient(h, "https://vbp.thu", "khoa-thu", "1.0.20"));

    private static JsonElement Than(HandlerTacVuGia h, int i = 0)
    {
        using var doc = JsonDocument.Parse(h.DaNhan[i].Body);
        return doc.RootElement.Clone();
    }

    [Fact]
    public async Task GhepTrang_GuiMotYeuCauDuCacTep_VaDocKetQuaCauTruc()
    {
        var h = new HandlerTacVuGia().ThanhCong(
            """{"so_van_ban":"15/GM-UBND","loai_van_ban":"GiayMoi","noi_dung":"Điều 1. A.\nĐiều 2. B.\nĐiều 3. C.","noi_nhan":["Như trên","Lưu: VT"],"can_cu":[]}""");
        var trang = new List<(string, string)> { ("AQID", "image/png"), ("BAUG", "image/jpeg") };

        var kq = await TaoDichVu(h).ExtractDocumentFromMultipleFilesAsync(trang);

        Assert.Equal("15/GM-UBND", kq.SoVanBan);
        Assert.Equal("GiayMoi", kq.LoaiVanBan);
        Assert.Equal("Điều 1. A.\nĐiều 2. B.\nĐiều 3. C.", kq.NoiDung);
        Assert.Equal(new[] { "Như trên", "Lưu: VT" }, kq.NoiNhan);
        Assert.Equal("Thuong", kq.DoKhan);
        var y = Assert.Single(h.DaNhan);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/trich-xuat", y.Url);
        var b = Than(h);
        Assert.Equal("cau-truc", b.GetProperty("che_do").GetString());
        var tep = b.GetProperty("tep");
        Assert.Equal(2, tep.GetArrayLength());
        Assert.Equal("BAUG", tep[1].GetProperty("base64").GetString());
        Assert.Equal("image/jpeg", tep[1].GetProperty("mime").GetString());
    }

    [Fact]
    public async Task DocChu_GuiCheDoChiChuVaTraChuoi()
    {
        var h = new HandlerTacVuGia().ThanhCong("\"GIẤY MỜI\\nHọp giao ban\"");

        var kq = await TaoDichVu(h).ReadTextAsync("JVBERi0=", "application/pdf");

        Assert.Equal("GIẤY MỜI\nHọp giao ban", kq);
        var b = Than(h);
        Assert.Equal("chi-chu", b.GetProperty("che_do").GetString());
        Assert.Equal(1, b.GetProperty("tep").GetArrayLength());
        Assert.Equal("application/pdf", b.GetProperty("tep")[0].GetProperty("mime").GetString());
    }

    [Fact]
    public async Task QuaHaiMuoiTep_BaoLoiTruocKhiGui()
    {
        var h = new HandlerTacVuGia();
        var trang = Enumerable.Range(0, 21).Select(_ => ("AQID", "image/png")).ToList();

        await Assert.ThrowsAsync<ArgumentException>(() => TaoDichVu(h).ExtractDocumentFromMultipleFilesAsync(trang));

        Assert.Empty(h.DaNhan);
    }

    [Fact]
    public async Task UocTinh_CungThanVoiLuotChay()
    {
        var h = new HandlerTacVuGia().TraVe(System.Net.HttpStatusCode.OK,
            """{"success":true,"data":{"credit_min":3,"credit_max":9,"so_du":50,"du_credit":true}}""");

        var u = await TaoDichVu(h).UocTinhAsync(new[] { ("AQID", "image/png") }, chiChu: true);

        Assert.Equal(9, u.CreditMax);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/trich-xuat/uoc-tinh", h.DaNhan[0].Url);
        Assert.Equal("chi-chu", Than(h).GetProperty("che_do").GetString());
    }

    [Fact]
    public void DocTep_DoanMimeTheoDuoi()
    {
        var duongDan = Path.Combine(Path.GetTempPath(), $"thu-trich-xuat-{Guid.NewGuid():N}.pdf");
        File.WriteAllBytes(duongDan, new byte[] { 1, 2, 3 });
        try
        {
            var (base64, mime) = TrichXuatService.DocTep(duongDan);
            Assert.Equal("AQID", base64);
            Assert.Equal("application/pdf", mime);
        }
        finally
        {
            File.Delete(duongDan);
        }
    }
}
```

Run: `dotnet test ... --filter "FullyQualifiedName~TrichXuatServiceTests"` → Expected: lỗi biên dịch (`TrichXuatService` chưa có).

- [ ] **Step 2: Đổi tên tệp và viết lại**

```bash
cd /d/AIVanBanCaNhan && git mv AIVanBan.Core/Services/GeminiAIService.cs AIVanBan.Core/Services/TrichXuatService.cs
```

Trong `TrichXuatService.cs`: **giữ nguyên văn** năm phương thức xử lý kết quả (hậu xử lý hiển thị, không phải prompt): `ParseExtractedDocument(string jsonText)`, `FormatExtractedContent(string content)`, `StripHeaderFromContent(string content)`, `GetJsonString(JsonElement root, string propertyName, string defaultValue = "")`, `ExtractJsonField(string text, string fieldName)` (hiện nằm khoảng dòng 837–1083, trong `#region PDF/Image OCR with Vision`). **Xóa mọi thứ khác** (hằng/khóa Gemini, ba hàm dựng, `IsUsingVanBanPlusApi`, `GeminiDirectKey`, retry, `GenerateContentAsync`, `GenerateContentStreamAsync`, `PostVanBanPlusExtractAsync`, `CallGeminiDirect*`, prompt OCR, mọi DTO Gemini và VanBanPlus, các `#region`). Phần đầu tệp thay bằng:

```csharp
using System.Text.Json;

namespace AIVanBan.Core.Services;

/// <summary>
/// Trích xuất văn bản từ ảnh/PDF scan qua tác vụ v2 trich-xuat (chế độ cau-truc hoặc chi-chu).
/// Prompt OCR và khóa AI nằm ở máy chủ VanBanPlus; bản cài máy không gọi thẳng nhà cung cấp AI.
/// </summary>
public class TrichXuatService
{
    /// <summary>Hợp đồng: mỗi lần gửi 1..20 tệp.</summary>
    public const int SoTepToiDa = 20;

    private readonly TacVuClient _tacVu;

    public TrichXuatService() : this(new TacVuClient()) { }

    public TrichXuatService(TacVuClient tacVu)
    {
        _tacVu = tacVu;
    }

    /// <summary>Credit đã trừ, số dư, cờ bị cắt của lượt chạy gần nhất.</summary>
    public ThongTinLuot? LuotCuoi { get; private set; }

    /// <summary>DTO chứa dữ liệu văn bản trích xuất từ ảnh/PDF scan (tên trường JSON snake_case ở ParseExtractedDocument).</summary>
    public class ExtractedDocumentData
    {
        public string SoVanBan { get; set; } = "";
        public string TrichYeu { get; set; } = "";
        public string LoaiVanBan { get; set; } = "";
        public string NgayBanHanh { get; set; } = "";
        public string CoQuanBanHanh { get; set; } = "";
        public string NguoiKy { get; set; } = "";
        public string NoiDung { get; set; } = "";
        public string[] NoiNhan { get; set; } = Array.Empty<string>();
        public string[] CanCu { get; set; } = Array.Empty<string>();
        public string HuongVanBan { get; set; } = ""; // Đi/Đến/Nội bộ
        public string DoKhan { get; set; } = "Thuong"; // Thuong/Khan/ThuongKhan/HoaToc
        public string DoMat { get; set; } = "Thuong"; // Thuong/Mat/ToiMat/TuyetMat
        public string LinhVuc { get; set; } = "";
        public string DiaDanh { get; set; } = "";
        public string ChucDanhKy { get; set; } = "";
        public string ThamQuyenKy { get; set; } = "";
    }

    /// <summary>Đọc tệp thành base64 + mime theo đuôi tệp.</summary>
    public static (string Base64, string MimeType) DocTep(string filePath)
    {
        var base64 = Convert.ToBase64String(File.ReadAllBytes(filePath));
        var mime = Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".tiff" or ".tif" => "image/tiff",
            _ => "application/octet-stream",
        };
        return (base64, mime);
    }

    /// <summary>Ước tính theo số tệp và dung lượng; thân giống hệt lượt chạy.</summary>
    public Task<UocTinhKetQua> UocTinhAsync(IReadOnlyList<(string Base64, string MimeType)> tep, bool chiChu = false)
        => _tacVu.UocTinhAsync("trich-xuat", DauVao(tep, chiChu));

    /// <summary>Nhiều tệp là các TRANG của cùng một văn bản → một kết quả (một lượt chạy).</summary>
    public async Task<ExtractedDocumentData> ExtractDocumentFromMultipleFilesAsync(IReadOnlyList<(string Base64, string MimeType)> files)
    {
        var kq = await _tacVu.ChayAsync<JsonElement>("trich-xuat", DauVao(files, chiChu: false));
        LuotCuoi = kq.Luot;
        return ParseExtractedDocument(kq.KetQua.GetRawText());
    }

    public Task<ExtractedDocumentData> ExtractDocumentFromBytesAsync(string base64Data, string mimeType)
        => ExtractDocumentFromMultipleFilesAsync(new[] { (base64Data, mimeType) });

    public Task<ExtractedDocumentData> ExtractDocumentFromFileAsync(string filePath)
    {
        var (base64, mime) = DocTep(filePath);
        return ExtractDocumentFromBytesAsync(base64, mime);
    }

    /// <summary>Chỉ đọc chữ (không trích trường), chế độ chi-chu.</summary>
    public async Task<string> ReadTextAsync(string base64Data, string mimeType)
    {
        var kq = await _tacVu.ChayAsync<string>("trich-xuat", DauVao(new[] { (base64Data, mimeType) }, chiChu: true));
        LuotCuoi = kq.Luot;
        return kq.KetQua;
    }

    public Task<string> ReadTextFromFileAsync(string filePath)
    {
        var (base64, mime) = DocTep(filePath);
        return ReadTextAsync(base64, mime);
    }

    private static object DauVao(IReadOnlyList<(string Base64, string MimeType)> tep, bool chiChu)
    {
        if (tep.Count == 0)
            throw new ArgumentException("Cần ít nhất 1 tệp để trích xuất.");
        if (tep.Count > SoTepToiDa)
            throw new ArgumentException($"Mỗi lần chỉ gửi tối đa {SoTepToiDa} tệp. Cần chia thành nhiều lần.");
        return new
        {
            tep = tep.Select(t => new { base64 = t.Base64, mime = t.MimeType }).ToArray(),
            che_do = chiChu ? "chi-chu" : "cau-truc",
        };
    }

    // ===== Giữ nguyên văn từ GeminiAIService: ParseExtractedDocument, FormatExtractedContent,
    // ===== StripHeaderFromContent, GetJsonString, ExtractJsonField — dán ngay dưới dòng này, rồi đóng lớp bằng "}".
```

(Phần chú thích hai dòng cuối là chỉ dẫn cho người làm — sau khi dán năm phương thức thì xóa hai dòng chú thích đó và đóng lớp.)

Xóa `AIVanBan.Tests/GeminiAIServiceTests.cs` (mọi bài trong đó kiểm mã vừa xóa; 401/hết hạn/quá tải đã có ở `TacVuClientTests`):

```bash
cd /d/AIVanBanCaNhan && git rm AIVanBan.Tests/GeminiAIServiceTests.cs
```

Run `dotnet test ... --filter "FullyQualifiedName~TrichXuatServiceTests"` → Expected: `Passed!  - Failed: 0, Passed: 5`.

- [ ] **Step 3: `TacVuGiaoDien.DocChuTuPdfAsync`**

Thêm vào lớp `TacVuGiaoDien`:

```csharp
    /// <summary>
    /// Đọc chữ từ tệp PDF qua trich-xuat (chi-chu): báo trước credit, báo lỗi.
    /// Trả null khi người dùng hủy hoặc đã báo lỗi.
    /// </summary>
    public static async Task<string?> DocChuTuPdfAsync(Window owner, string filePath)
    {
        (string Base64, string MimeType) tep;
        try
        {
            tep = TrichXuatService.DocTep(filePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(owner, $"Không đọc được tệp: {ex.Message}", "Lỗi đọc tệp",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        var dichVu = new TrichXuatService();
        var motTep = new[] { tep };
        if (!await XacNhanCreditAsync(owner, "Đọc chữ từ tệp PDF", () => dichVu.UocTinhAsync(motTep, chiChu: true)))
            return null;

        Mouse.OverrideCursor = Cursors.Wait;
        try
        {
            return await dichVu.ReadTextAsync(tep.Base64, tep.MimeType);
        }
        catch (Exception ex)
        {
            Mouse.OverrideCursor = null;
            HienLoi(owner, ex, "Lỗi đọc tệp PDF");
            return null;
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }
```

- [ ] **Step 4: Bốn nhánh `.pdf` trong ba hộp thoại**

Trong `DocumentSummaryDialog.UploadFile_Click`, `DocumentReviewDialog.UploadFile_Click`, `DocumentReviewDialog.UploadReference_Click`, `DocumentAdvisoryDialog.UploadFile_Click`: thay hai dòng

```csharp
                        var aiService = new GeminiAIService();
                        extractedText = await aiService.ReadTextFromFileAsync(filePath);
```

bằng

```csharp
                        var docDuoc = await TacVuGiaoDien.DocChuTuPdfAsync(this, filePath);
                        if (docDuoc == null) return; // đã hủy hoặc đã báo lỗi
                        extractedText = docDuoc;
```

(giữ nguyên khối `if (string.IsNullOrWhiteSpace(extractedText)) { MessageBox ... return; }` ngay sau).

- [ ] **Step 5: `ScanImportDialog`**

1. Trường: thay `private readonly GeminiAIService _aiService;` bằng `private readonly TrichXuatService _trichXuat = new();`; thay hai chỗ `GeminiAIService.ExtractedDocumentData` trong khai báo trường (`_extractedData`, `_separateResults`) và hai chỗ trong chữ ký `PopulateForm`, `BuildDocumentFromData` bằng `TrichXuatService.ExtractedDocumentData`.
2. Hàm dựng: `public ScanImportDialog(DocumentService documentService)`, xóa dòng `_aiService = ...`.
3. Trong `Analyze_Click`:
   - Câu cảnh báo dung lượng ở chế độ ghép: thay `"Bạn vẫn muốn tiếp tục?"` bằng `"Vẫn tiếp tục?"` (lời hiện cho khách không đại từ nhân xưng).
   - Ngay trước dòng `// Disable UI` chèn:

```csharp
        if (!IsSeparateMode && _files.Count > TrichXuatService.SoTepToiDa)
        {
            MessageBox.Show(
                $"Chế độ «Ghép trang» nhận tối đa {TrichXuatService.SoTepToiDa} tệp mỗi lần. Cần bớt tệp hoặc chuyển sang «Tách riêng».",
                "Quá nhiều tệp", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var tep = new List<(string Base64, string MimeType)>();
        try
        {
            foreach (var f in _files)
                tep.Add((Convert.ToBase64String(await File.ReadAllBytesAsync(f.FilePath)), f.MimeType));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không đọc được tệp: {ex.Message}", "Lỗi đọc tệp", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var duocChay = IsSeparateMode
            ? await TacVuGiaoDien.XacNhanCreditAsync(this, $"Trích xuất {tep.Count} tệp (tách riêng)", () => UocTinhTachRiengAsync(tep))
            : await TacVuGiaoDien.XacNhanCreditAsync(this, $"Trích xuất {tep.Count} tệp (ghép trang)", () => _trichXuat.UocTinhAsync(tep));
        if (!duocChay) return;

```

   - Đổi `await AnalyzeSeparateAsync();` → `await AnalyzeSeparateAsync(tep);` và `await AnalyzeMergeAsync();` → `await AnalyzeMergeAsync(tep);`.
4. Thêm phương thức (ngay sau `Analyze_Click`):

```csharp
    /// <summary>Tách riêng = mỗi tệp một lượt: cộng khoảng credit của từng tệp.</summary>
    private async Task<UocTinhKetQua> UocTinhTachRiengAsync(List<(string Base64, string MimeType)> tep)
    {
        int min = 0, max = 0, soDu = 0;
        foreach (var t in tep)
        {
            var u = await _trichXuat.UocTinhAsync(new[] { t });
            min += u.CreditMin;
            max += u.CreditMax;
            soDu = u.SoDu;
        }
        return new UocTinhKetQua(min, max, soDu, soDu >= max);
    }
```

5. `AnalyzeMergeAsync`: đổi chữ ký thành `private async Task AnalyzeMergeAsync(List<(string Base64, string MimeType)> tep)`; xóa khối đọc tệp (từ `// Đọc tất cả file → base64` tới hết vòng `for`); thay dòng `_extractedData = await _aiService.ExtractDocumentFromMultipleFilesAsync(fileDataList);` bằng:

```csharp
            _extractedData = await _trichXuat.ExtractDocumentFromMultipleFilesAsync(tep);
            TacVuGiaoDien.CanhBaoNeuBiCat(this, _trichXuat.LuotCuoi);
```

6. `AnalyzeSeparateAsync`: đổi chữ ký thành `private async Task AnalyzeSeparateAsync(List<(string Base64, string MimeType)> tep)`; ngay trước vòng `for` thêm `Exception? loiDung = null;`; thay khối `try { var data = await _aiService.ExtractDocumentFromFileAsync(file.FilePath); ... } catch (Exception ex) { errors.Add(...); }` bằng:

```csharp
            try
            {
                var data = await _trichXuat.ExtractDocumentFromBytesAsync(tep[i].Base64, tep[i].MimeType);
                _separateResults.Add((data, file.FilePath));
            }
            catch (Exception ex)
            {
                errors.Add($"❌ {file.FileName}: {TacVuGiaoDien.MoTaLoi(ex)}");
                if (ex is TacVuException { Ma: MaLoiTacVu.KhongDuCredit or MaLoiTacVu.ChuaDangNhap })
                {
                    loiDung = ex; // các tệp sau cũng sẽ hỏng y như vậy
                    break;
                }
            }
```

   và ngay sau dòng `pbBatch.Value = 100;` thêm `if (loiDung != null) TacVuGiaoDien.MoiMuaCreditNeuThieu(this, loiDung);`.
7. `ShowAnalysisError`: chèn ở đầu thân hàm:

```csharp
        if (ex is TacVuException)
        {
            TacVuGiaoDien.HienLoi(this, ex, "Lỗi phân tích");
            return;
        }
```

- [ ] **Step 6: Không còn gọi thẳng Gemini / đường cũ trong Core**

```bash
cd /d/AIVanBanCaNhan && grep -rnE "GeminiAIService|generativelanguage|/api/ai/|Bạn là" AIVanBan.Core AIVanBan.Desktop AIVanBan.Tests --include=*.cs
```

Expected: chỉ còn `ApiSettingsDialog.xaml.cs` (`TestDirectConnection_Click`, `generativelanguage`) — Task 10 xóa. Không còn dòng nào trong Core.

- [ ] **Step 7: Build + test + soát lời.** Expected: xanh, soát trống.

- [ ] **Step 8: Commit**

```bash
cd /d/AIVanBanCaNhan && git add -u AIVanBan.Core AIVanBan.Desktop AIVanBan.Tests
git add AIVanBan.Core/Services/TrichXuatService.cs AIVanBan.Tests/TrichXuatServiceTests.cs
git commit -m "$(cat <<'EOF'
feat(desktop): trich-xuat goi v2 (ghep trang, tach rieng, doc chu PDF); GeminiAIService -> TrichXuatService, bo goi thang Gemini

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
)"
```

---

### Task 10: Bỏ chế độ gọi thẳng Gemini

**Files:**
- Modify: `AIVanBan.Core/Services/AppSettingsService.cs`
- Modify: `AIVanBan.Desktop/Views/ApiSettingsDialog.xaml`, `AIVanBan.Desktop/Views/ApiSettingsDialog.xaml.cs`
- Modify: `AIVanBan.Desktop/MainWindow.xaml.cs` (`QuickLogin_Click`, `LoadApiStatusBar`)
- Modify: `AIVanBan.Desktop/Views/LoginRegisterDialog.xaml.cs`

**Interfaces:**
- Consumes: `ThietLapDiaPhuong.Luu` (Task 7).
- Produces: `AppSettings` không còn `UseVanBanPlusApi`, `GeminiApiKey`, `DevModeActivatedAt`; không còn lớp `DevModePolicy`; `AppSettingsService.GetEffectiveApiKey()` trả `VanBanPlusApiKey`. Giữ `VanBanPlusApiUrl`, `VanBanPlusApiKey`, `AiEnabled`, thông tin người dùng, `CloudSync`, `ExtraSettings`, `VercelBypassToken` (đường máy chủ còn dùng ở hộp thoại tài khoản/quản trị). `settings.json` cũ có các khóa đã bỏ vẫn đọc được (System.Text.Json bỏ qua khóa lạ); lần lưu sau tự rơi khóa cũ.

- [ ] **Step 1: `AppSettingsService.cs`**

- Xóa trọn lớp `DevModePolicy` (kèm chú thích `/// <summary>Cấu hình ứng dụng</summary>` đứng trước nó).
- Trong `AppSettings`, xóa: khối `// ===== Chế độ API =====` cùng thuộc tính `UseVanBanPlusApi`; khối `// ===== Gemini trực tiếp (legacy/bảo trì) =====` cùng `GeminiApiKey`; thuộc tính `DevModeActivatedAt` và chú thích của nó.
- Thay `GetEffectiveApiKey`:

```csharp
    /// <summary>Khóa API VanBanPlus hiện tại (bản cài máy chỉ gọi AI qua máy chủ VanBanPlus).</summary>
    public static string GetEffectiveApiKey() => Load().VanBanPlusApiKey;
```

- [ ] **Step 2: `ApiSettingsDialog.xaml`**

- Dòng `<StackPanel Grid.Row="0" Margin="0,0,0,16" MouseLeftButtonDown="Header_Click">` → `<StackPanel Grid.Row="0" Margin="0,0,0,16">`; chú thích phía trên đổi thành `<!-- Header -->`.
- Xóa trọn `<GroupBox x:Name="grpModeSelector" ...> ... </GroupBox>` (kèm chú thích `<!-- Chế độ kết nối (ẩn, chỉ dev thấy) -->`).
- Xóa trọn `<GroupBox x:Name="grpGeminiDirect" ...> ... </GroupBox>` (kèm chú thích `<!-- Dev mode: AI Direct (ẩn mặc định) -->`).

- [ ] **Step 3: `ApiSettingsDialog.xaml.cs`**

Thay toàn bộ phần từ dòng `public partial class ApiSettingsDialog : Window` tới hết phương thức `Cancel_Click` (ngay trước khối chú thích `// ORGANIZATION TAB`) bằng:

```csharp
public partial class ApiSettingsDialog : Window
{
    private DocumentService? _documentService;

    public ApiSettingsDialog()
    {
        InitializeComponent();
        LoadSettings();
        LoadOrganizationTab();
    }

    public ApiSettingsDialog(DocumentService documentService) : this()
    {
        _documentService = documentService;
        LoadOrganizationTab();
    }

    private void LoadSettings()
    {
        var settings = AppSettingsService.Load();

        tglAiEnabled.IsChecked = settings.AiEnabled;
        UpdateAiToggleVisual(settings.AiEnabled);

        txtApiUrl.Text = settings.VanBanPlusApiUrl;
        txtApiKey.Text = settings.VanBanPlusApiKey;

        UpdateCurrentStatus(settings);
    }

    private void UpdateCurrentStatus(AppSettings settings)
    {
        if (!string.IsNullOrEmpty(settings.VanBanPlusApiKey))
        {
            var maskedKey = settings.VanBanPlusApiKey.Length > 10
                ? settings.VanBanPlusApiKey[..10] + "..."
                : settings.VanBanPlusApiKey;
            txtCurrentStatus.Text = $"✅ Đã kích hoạt\n" +
                                    $"🌐 Server: {settings.VanBanPlusApiUrl}\n" +
                                    $"🔑 Mã: {maskedKey}\n" +
                                    $"👤 {(string.IsNullOrEmpty(settings.UserEmail) ? "(chưa xác thực)" : settings.UserEmail)}";
        }
        else
        {
            txtCurrentStatus.Text = "⚠️ Chưa kích hoạt AI\n\nCần nhập mã kích hoạt để sử dụng các tính năng AI.\n📞 Liên hệ Zalo: Thắng Phan — 0907136029";
        }
    }
```

rồi **giữ nguyên** `TestConnection_Click` (chép lại đúng như hiện có), tiếp theo:

```csharp
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var settings = AppSettingsService.Load();

        settings.AiEnabled = tglAiEnabled.IsChecked == true;
        settings.VanBanPlusApiUrl = txtApiUrl.Text.Trim().TrimEnd('/');
        settings.VanBanPlusApiKey = txtApiKey.Text.Trim();

        if (settings.AiEnabled && string.IsNullOrEmpty(settings.VanBanPlusApiKey))
        {
            MessageBox.Show("Cần nhập mã kích hoạt.\n\n📞 Liên hệ Zalo: Thắng Phan — 0907136029\n💰 Chỉ từ 99.000đ/tháng",
                "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        AppSettingsService.Save(settings);
        SaveOrganizationTab();
        ThietLapDiaPhuong.Luu(txtDpXa.Text, txtDpCapTren.Text, txtDpTinh.Text);
        MessageBox.Show("✅ Đã lưu cài đặt thành công!\n\nCần khởi động lại ứng dụng để áp dụng thay đổi.",
            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
```

Thay `UpdateAiToggleVisual` (ở cuối tệp) bằng:

```csharp
    private void UpdateAiToggleVisual(bool enabled)
    {
        if (grpVanBanPlus == null) return; // not yet loaded

        grpVanBanPlus.IsEnabled = enabled;
        grpVanBanPlus.Opacity = enabled ? 1.0 : 0.4;

        if (enabled)
        {
            txtAiToggleHint.Text = "✅ Tính năng AI đã được bật";
            brdAiToggle.BorderBrush = System.Windows.Media.Brushes.Green;
        }
        else
        {
            txtAiToggleHint.Text = "Bật để sử dụng các tính năng AI nâng cao";
            brdAiToggle.BorderBrush = (System.Windows.Media.Brush)FindResource("MaterialDesignDivider");
        }
    }
```

Đã xóa theo: các trường `_headerClickCount`, `_lastHeaderClick`, `_devModeActive`, `_countdownTimer`; `Header_Click`, `CloseDevMode_Click`, `StartCountdownTimer`, `UpdateDevModeCountdown`, `ApiMode_Changed`, `UpdateVisibility`, `TestDirectConnection_Click`. Giữ phần ORGANIZATION TAB (đã có dòng `ThietLapDiaPhuong.Doc()` từ Task 7), `OrgTypeDisplay`, `AiToggle_Changed`, DTO `MeResponse`/`MeData`.

- [ ] **Step 4: `MainWindow.xaml.cs` và `LoginRegisterDialog.xaml.cs`**

- `QuickLogin_Click`: `if (settings.UseVanBanPlusApi && !string.IsNullOrEmpty(settings.VanBanPlusApiKey)` → `if (!string.IsNullOrEmpty(settings.VanBanPlusApiKey)` (giữ phần `&& !string.IsNullOrEmpty(settings.UserEmail))`).
- `LoadApiStatusBar`: `if (settings.UseVanBanPlusApi && !string.IsNullOrEmpty(settings.VanBanPlusApiKey))` → `if (!string.IsNullOrEmpty(settings.VanBanPlusApiKey))`; xóa trọn nhánh `else if (!string.IsNullOrEmpty(settings.GeminiApiKey)) { // AI direct mode ... }`.
- `LoginRegisterDialog.xaml.cs`: xóa ba dòng `settings.UseVanBanPlusApi = true;`.

- [ ] **Step 5: Không còn dấu vết chế độ gọi thẳng**

```bash
cd /d/AIVanBanCaNhan && grep -rnE "DevModePolicy|UseVanBanPlusApi|GeminiApiKey|DevModeActivatedAt|rbGeminiDirect|rbVanBanPlus|grpGeminiDirect|grpModeSelector|_fallbackGeminiKey|txtGeminiKey|Header_Click|generativelanguage|/api/ai/" AIVanBan.Core AIVanBan.Desktop AIVanBan.Tests --include=*.cs --include=*.xaml
```

Expected: trống.

- [ ] **Step 6: Build + test + soát lời.** Expected: xanh.

- [ ] **Step 7: Commit**

```bash
cd /d/AIVanBanCaNhan && git add AIVanBan.Core/Services/AppSettingsService.cs AIVanBan.Desktop/Views/ApiSettingsDialog.xaml AIVanBan.Desktop/Views/ApiSettingsDialog.xaml.cs AIVanBan.Desktop/MainWindow.xaml.cs AIVanBan.Desktop/Views/LoginRegisterDialog.xaml.cs
git commit -m "$(cat <<'EOF'
refactor(desktop): bo che do goi thang Gemini (rbGeminiDirect, DevModePolicy, GeminiApiKey)

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
)"
```

---

### Task 11: Tài liệu — Trợ giúp, hướng dẫn, `PROJECT_STATUS.md`, changelog

**Files:**
- Modify: `AIVanBan.Desktop/Views/HelpPage.xaml`
- Modify: `HUONG_DAN_SU_DUNG.md`
- Modify: `PROJECT_STATUS.md`
- Create: `CHANGELOG_v1.0.20.md`

**Interfaces:**
- Consumes: hành vi đã làm ở Task 3–10.
- Produces: tài liệu khớp bản 1.0.20 (spec §12 bước 5).

- [ ] **Step 1: Kiểm trang Bảng giá có gói nạp chưa (dot2c)**

```bash
curl -s https://vanbanplus.giakiemso.com/bang-gia | grep -o "nạp[^<]\{0,40\}" | head -3
```

Có kết quả → dùng câu «gói nạp» ở Step 2 (biến thể A). Trống → biến thể B.

- [ ] **Step 2: `HelpPage.xaml` — câu hỏi về credit**

Thay đoạn trả lời của «Hỏi: Hết credit AI miễn phí thì sao?» (khối `TextBlock` bắt đầu bằng `<Bold>Trả lời:</Bold> Nâng cấp lên gói Khởi đầu`) bằng:

Biến thể A:

```xml
                            <TextBlock Style="{StaticResource BodyText}" Margin="0,4,0,0">
                                <Bold>Trả lời:</Bold> Nâng cấp lên gói Khởi đầu (99.000đ/tháng — 400 credit), Chuyên nghiệp (249.000đ — 1.100 credit)
                                hoặc Doanh nghiệp (599.000đ — 3.000 credit), hoặc mua gói nạp thêm (dùng trong 90 ngày) tại vanbanplus.giakiemso.com/bang-gia.
                                Credit gói tháng được cấp mới mỗi tháng, không cộng dồn sang tháng sau.
                                Trước mỗi tác vụ AI, phần mềm báo khoảng credit dự kiến và số dư hiện có; credit trừ theo mức dùng thật, không vượt mức đã báo;
                                tác vụ lỗi được hoàn credit. Số dư không đủ thì phần mềm mở sẵn trang mua credit.
                            </TextBlock>
```

Biến thể B: như A nhưng bỏ cụm `, hoặc mua gói nạp thêm (dùng trong 90 ngày)`.

- [ ] **Step 3: `HelpPage.xaml` — mục «Có gì mới»**

Trong thẻ v1.0.19, xóa khối huy hiệu «MỚI NHẤT»:

```xml
                                <Border Background="#FF9800" CornerRadius="4" Padding="6,2">
                                    <TextBlock Text="🆕 MỚI NHẤT" Foreground="White" FontWeight="Bold" FontSize="10"/>
                                </Border>
```

và đổi chú thích `<!-- ════ Version 1.0.19 — Tháng 11/2026 (MỚI NHẤT) ════ -->` thành `<!-- ════ Version 1.0.19 — Tháng 11/2026 ════ -->`. Chèn ngay trước chú thích đó:

```xml
                    <!-- ════ Version 1.0.20 — Tháng 11/2026 (MỚI NHẤT) ════ -->
                    <Border Style="{StaticResource ChangelogCard}" BorderBrush="#1976D2" BorderThickness="2">
                        <StackPanel>
                            <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                                <Border Background="#1976D2" CornerRadius="4" Padding="8,2" Margin="0,0,8,0">
                                    <TextBlock Text="v1.0.20 — Tháng 11/2026" Foreground="White" FontWeight="Bold" FontSize="12"/>
                                </Border>
                                <Border Background="#FF9800" CornerRadius="4" Padding="6,2">
                                    <TextBlock Text="🆕 MỚI NHẤT" Foreground="White" FontWeight="Bold" FontSize="10"/>
                                </Border>
                            </StackPanel>
                            <TextBlock Style="{StaticResource BodyText}" FontWeight="Bold" Foreground="#0D47A1" Margin="4,4,0,4">
                                🔒 AI chạy hoàn toàn qua máy chủ VanBanPlus
                            </TextBlock>
                            <TextBlock Style="{StaticResource BodyText}" Margin="4,0,0,0" TextWrapping="Wrap">
                                ✅ <Bold>Báo trước số credit</Bold>: trước mỗi tác vụ AI hiện khoảng credit dự kiến và số dư hiện có
                            </TextBlock>
                            <TextBlock Style="{StaticResource BodyText}" Margin="4,0,0,0" TextWrapping="Wrap">
                                ✅ <Bold>Thông báo lỗi rõ ràng</Bold>: hết credit (mở sẵn trang mua), quá tải, quá dài, mất kết nối; tác vụ lỗi được hoàn credit
                            </TextBlock>
                            <TextBlock Style="{StaticResource BodyText}" Margin="4,0,0,0" TextWrapping="Wrap">
                                ✅ <Bold>Soạn thảo theo địa phương của cơ quan</Bold>: khai xã/phường, cơ quan cấp trên, tỉnh tại Cài đặt → 🏛️ Cơ quan
                            </TextBlock>
                            <TextBlock Style="{StaticResource BodyText}" Margin="4,0,0,0" TextWrapping="Wrap">
                                ✅ <Bold>Bỏ chế độ kết nối AI trực tiếp</Bold>: phần mềm không còn gọi thẳng nhà cung cấp AI
                            </TextBlock>
                        </StackPanel>
                    </Border>

```

- [ ] **Step 4: `HUONG_DAN_SU_DUNG.md`**

- `3. AI Gemini Vision tự động đọc và trích xuất **14 trường dữ liệu**:` → `3. AI tự động đọc và trích xuất **14 trường dữ liệu**:` (phần sau giữ nguyên).
- `VanBanPlus tích hợp **7 tính năng AI** chạy bằng Google Gemini 2.5 Flash:` → `VanBanPlus tích hợp **7 tính năng AI**, chạy qua máy chủ VanBanPlus và tính bằng credit. Trước mỗi tác vụ, phần mềm báo khoảng credit dự kiến và số dư hiện có; tác vụ lỗi được hoàn credit.`
- `- Hỗ trợ **hiển thị kết quả theo thời gian thực** (streaming)` → `- Địa phương trong văn bản (xã/phường, cơ quan cấp trên, tỉnh) lấy từ **Cài đặt → 🏛️ Cơ quan**`.
- Mục `### 14.3 Cài đặt API (nâng cao)`: thay đoạn từ dòng `Ứng dụng hỗ trợ 2 chế độ kết nối AI:` tới hết bảng hai dòng bằng:

```markdown
Tính năng AI chỉ kết nối qua máy chủ VanBanPlus bằng mã kích hoạt của tài khoản (từ bản 1.0.20 không còn chế độ kết nối trực tiếp tới nhà cung cấp AI). Tab **🏛️ Cơ quan** khai thêm xã/phường, cơ quan cấp trên, tỉnh để AI soạn thảo đúng địa phương; để trống cả ba thì văn bản không kèm bối cảnh địa phương.
```

- Mục `### 14.4`: `- ☁️ **Trạng thái API**: Đang kết nối / Bảo trì / Chưa cấu hình` → `- ☁️ **Trạng thái API**: Đang kết nối / Chưa cấu hình`.

- [ ] **Step 5: `PROJECT_STATUS.md`**

Sửa theo mốc văn bản (đợt 2 có thể đã thêm dòng; không xóa dòng của người khác):

- Dòng `> **Phiên bản hiện tại:** ...` → `> **Phiên bản hiện tại:** v1.0.20 (AI chạy qua API tác vụ v2, báo trước credit, bỏ gọi thẳng Gemini — chuẩn bị phát hành <ngày chạy bước này, yyyy-MM-dd>)`; dòng `> **Cập nhật lần cuối:**` → ngày chạy bước này.
- Bảng «Nguyên tắc cốt lõi», thêm dòng cuối:

```markdown
| 6 | **Không gọi thẳng Gemini, không giữ prompt nghiệp vụ** | Bản cài máy chỉ gửi dữ liệu tới `/api/v2/tac-vu/*` (`TacVuClient`); prompt, khóa AI, tính credit nằm ở `vanbanplus-api` (spec 2026-11-09 §1 tiêu chí 3). Không thêm lại khóa AI riêng hay chế độ gọi thẳng nhà cung cấp AI |
```

- Dòng `| ✅ | AI Soạn VB (từ template + prompt) | \`AIComposeDialog.xaml\` | Streaming output |` → `| ✅ | AI Soạn VB (từ template) | \`AIComposeDialog.xaml\`, \`TacVuSoanThao.cs\` | **v1.0.20:** tác vụ v2 \`soan-thao\`, địa phương từ Cài đặt → Cơ quan; không streaming |`.
- Dòng chứa `AI Scan OCR (ảnh/PDF → trích xuất)`: cột cuối `Gemini Vision` → `**v1.0.20:** tác vụ v2 \`trich-xuat\` (\`TrichXuatService.cs\`)`.
- Dòng chứa `Upload file cho AI Kiểm tra/Tóm tắt/Tham mưu`: `GeminiAI.ReadTextFromFileAsync` → `TrichXuatService (trich-xuat, chế độ chi-chu; v1.0.20)`.
- Dòng `| ✅ | Dual-mode: Proxy API + Direct Gemini | \`ApiSettingsDialog.xaml\` | Dev mode tự hết hạn 1h |` → `| ✅ | Kết nối AI qua VanBanPlus (API tác vụ v2) | \`TacVuClient.cs\`, \`TacVuGiaoDien.cs\`, \`ApiSettingsDialog.xaml\` | **v1.0.20:** báo trước khoảng credit, xử lý đủ mã lỗi; bỏ chế độ gọi thẳng Gemini (DevModePolicy) |`.
- Dòng `| ✅ | Credit + API tác vụ v2 (đợt 1, 12/11/2026) | ...`: nối vào cuối cột ghi chú (trước `|` cuối) câu ` Đợt 3: bản cài máy v1.0.20 gọi đủ 6 tác vụ v2, không còn prompt C# (chờ phát hành).`.

- [ ] **Step 6: `CHANGELOG_v1.0.20.md`**

```markdown
# VanBanPlus v1.0.20

**Ngày phát hành:** <ngày phát hành, dd/MM/yyyy — điền ở Task 13>
**Loại bản cập nhật:** Cập nhật tính năng (không bắt buộc)

## 🔒 AI chạy hoàn toàn qua máy chủ VanBanPlus

- **Báo trước số credit**: trước mỗi tác vụ AI (soạn thảo, kiểm tra, tóm tắt, tham mưu, báo cáo định kỳ, trích xuất), phần mềm hiện khoảng credit dự kiến và số dư hiện có.
- **Thông báo lỗi rõ ràng**: hết credit (mở sẵn trang mua credit), máy chủ AI quá tải (kèm thời gian thử lại), nội dung hoặc tệp quá lớn, mất kết nối. Tác vụ lỗi được hoàn credit.
- **Soạn thảo theo địa phương của cơ quan**: khai xã/phường, cơ quan cấp trên, tỉnh tại Cài đặt → 🏛️ Cơ quan.
- **Bỏ chế độ kết nối AI trực tiếp**: phần mềm không còn gọi thẳng nhà cung cấp AI, không còn lưu khóa AI riêng.

## 🔧 Kỹ thuật

- Gọi `/api/v2/tac-vu/<tên>` và `/uoc-tinh` (`TacVuClient`), header `X-VBP-Nguon: desktop`, `X-VBP-Phien-Ban: 1.0.20`.
- Xóa prompt nghiệp vụ khỏi C# (`DocumentSummaryService`, `DocumentReviewService`, `DocumentAdvisoryService`, `PeriodicReportService`, soạn thảo 26 loại văn bản, trích xuất). Tệp mẫu prompt ở `vanbanplus-api/tests/mau-prompt` là mốc cố định.
- `GeminiAIService` → `TrichXuatService`; bỏ `DevModePolicy`, `UseVanBanPlusApi`, `GeminiApiKey`.
- Bump version: `1.0.19` → `1.0.20` (csproj, AssemblyVersion, FileVersion, Inno Setup).
```

- [ ] **Step 7: Build (XAML) + soát lời**

```bash
cd /d/AIVanBanCaNhan && dotnet build AIVanBan.Desktop/AIVanBan.Desktop.csproj -nologo -v q 2>&1 | tail -5
git diff -U0 -- AIVanBan.Desktop/Views/HelpPage.xaml HUONG_DAN_SU_DUNG.md | grep "^+" | grep -nE "\b([Tt]ôi|[Tt]a|[Aa]nh|[Ee]m|[Bb]ạn|[Cc]hị|[Mm]ình|[Dd]ạ|[Vv]âng)\b|(^|\s)ạ[.,!? ]"
grep -nE "\b([Tt]ôi|[Tt]a|[Aa]nh|[Ee]m|[Bb]ạn|[Cc]hị|[Mm]ình|[Dd]ạ|[Vv]âng)\b|(^|\s)ạ[.,!? ]" CHANGELOG_v1.0.20.md
```

Expected: build `0 Error(s)`; hai lệnh soát trống.

- [ ] **Step 8: Commit**

```bash
cd /d/AIVanBanCaNhan && git add AIVanBan.Desktop/Views/HelpPage.xaml HUONG_DAN_SU_DUNG.md PROJECT_STATUS.md CHANGELOG_v1.0.20.md
git commit -m "$(cat <<'EOF'
docs: tro giup, huong dan, PROJECT_STATUS cho v1.0.20 (khong goi thang Gemini, bao truoc credit)

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
)"
```

---

### Task 12: Nâng phiên bản, build bộ cài, chạy thật 6 tác vụ trên production — rồi DỪNG

**Files:**
- Modify: `AIVanBan.Desktop/AIVanBan.Desktop.csproj`, `setup-script.iss`, `setup-vanbanplus.iss`, `update.xml`
- Create: `AIVanBan.Tests/TacVuThatTests.cs`
- Tạo (không commit, bị `.gitignore`): `Installer/VanBanPlus-Setup-1.0.20-x64.exe`

**Interfaces:**
- Consumes: toàn bộ service v2 (Task 3–9).
- Produces: bộ cài 1.0.20; bằng chứng 6 tác vụ chạy thật (spec §11 «Chạy thật» mục 3); không phát hành gì ra ngoài.

- [ ] **Step 1: Nâng phiên bản**

- `AIVanBan.Desktop/AIVanBan.Desktop.csproj`: `<Version>1.0.19</Version>` → `1.0.20`; `<AssemblyVersion>1.0.19.0</AssemblyVersion>` → `1.0.20.0`; `<FileVersion>1.0.19.0</FileVersion>` → `1.0.20.0`.
- `setup-script.iss`, `setup-vanbanplus.iss`: `#define MyAppVersion "1.0.19"` → `"1.0.20"`.
- `update.xml` (bản sao trong repo này): ba chỗ `1.0.19` → `1.0.20`, giữ `<mandatory>false</mandatory>`.

```bash
cd /d/AIVanBanCaNhan && grep -n "1\.0\.19\|1\.0\.20" AIVanBan.Desktop/AIVanBan.Desktop.csproj setup-script.iss setup-vanbanplus.iss update.xml
```

Expected: chỉ còn `1.0.20` (6 dòng csproj/iss + 3 dòng update.xml).

- [ ] **Step 2: Bộ kiểm chạy thật (chỉ chạy khi có khóa thử)**

`AIVanBan.Tests/TacVuThatTests.cs`:

```csharp
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using Xunit.Abstractions;

namespace AIVanBan.Tests;

/// <summary>
/// Chạy thật 6 tác vụ v2 trên production bằng tài khoản thử (tốn credit thật, khoảng 60–150 credit một lượt).
/// Chỉ chạy khi có biến môi trường VBP_E2E_API_KEY; không có thì in «BỎ QUA» và kết thúc (bộ kiểm thường vẫn xanh).
/// </summary>
public class TacVuThatTests
{
    private readonly ITestOutputHelper _out;

    public TacVuThatTests(ITestOutputHelper output) => _out = output;

    private TacVuClient? TaoClient()
    {
        var khoa = Environment.GetEnvironmentVariable("VBP_E2E_API_KEY");
        if (string.IsNullOrWhiteSpace(khoa))
        {
            _out.WriteLine("BỎ QUA: thiếu VBP_E2E_API_KEY");
            return null;
        }
        var diaChi = Environment.GetEnvironmentVariable("VBP_E2E_URL") ?? "https://vanbanplus.giakiemso.com";
        return new TacVuClient(new HttpClientHandler(), diaChi, khoa.Trim(), "1.0.20");
    }

    private void Ghi(string ten, UocTinhKetQua u, ThongTinLuot? l)
    {
        Assert.NotNull(l);
        _out.WriteLine($"OK {ten}: uoc-tinh {u.CreditMin}-{u.CreditMax}, tru {l!.CreditDaTru}, so du {l.SoDu}, bi_cat {l.BiCat}, ma_luot {l.MaLuot}");
        Assert.InRange(l.CreditDaTru, 1, u.CreditMax);
    }

    private static string TaoAnhGiayMoi()
    {
        using var bmp = new Bitmap(1240, 900);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using var font = new Font("Times New Roman", 26);
            var dong = new[]
            {
                "UBND XÃ MINH AN", "Số: 15/GM-UBND", "GIẤY MỜI", "Về việc họp giao ban tháng 11/2026",
                "Thời gian: 8 giờ 00, ngày 20/11/2026", "Địa điểm: Hội trường UBND xã Minh An",
                "CHỦ TỊCH", "Nguyễn Văn An",
            };
            for (var i = 0; i < dong.Length; i++)
                g.DrawString(dong[i], font, Brushes.Black, 60, 50 + i * 95);
        }
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return Convert.ToBase64String(ms.ToArray());
    }

    [Fact]
    public async Task SauTacVu_ChayThatTrenProduction()
    {
        var c = TaoClient();
        if (c == null) return;

        var tomTat = new DocumentSummaryService(c);
        const string qd = "Điều 1. Phê duyệt kế hoạch chuyển đổi số năm 2026 của UBND xã Minh An.\nĐiều 2. Giao Văn phòng theo dõi, báo cáo trước 30/12/2026.";
        var u = await tomTat.UocTinhAsync(qd, "Quyết định", "Phê duyệt kế hoạch chuyển đổi số", "UBND xã Minh An");
        var s = await tomTat.SummarizeAsync(qd, "Quyết định", "Phê duyệt kế hoạch chuyển đổi số", "UBND xã Minh An");
        Assert.False(string.IsNullOrWhiteSpace(s.Brief));
        Ghi("tom-tat", u, tomTat.LuotCuoi);

        var kiemTra = new DocumentReviewService(c);
        const string cv = "Kính gửi: Phòng Văn hóa.\nUBND xã Minh An đề nghị cung cấp số liệu về du lịch trươc ngày 20/11/2026.\nTrân trọng cảm ơn.";
        u = await kiemTra.UocTinhAsync(cv, "Công văn", "V/v cung cấp số liệu", "UBND xã Minh An");
        var r = await kiemTra.ReviewDocumentAsync(cv, "Công văn", "V/v cung cấp số liệu", "UBND xã Minh An");
        Assert.False(string.IsNullOrWhiteSpace(r.Summary));
        Ghi("kiem-tra", u, kiemTra.LuotCuoi);

        var thamMuu = new DocumentAdvisoryService(c);
        var ctx = new DocumentAdvisoryContext
        {
            Direction = "Đến", DocumentNumber = "120/UBND-VX", IssuedDate = new DateTime(2026, 11, 10),
            DueDate = new DateTime(2026, 11, 25), UrgencyLevel = "Khẩn", Recipients = new[] { "UBND các xã" },
        };
        const string den = "UBND tỉnh đề nghị UBND các xã báo cáo kết quả chuyển đổi số năm 2026 trước ngày 25/11/2026.";
        u = await thamMuu.UocTinhAsync(den, "Công văn", "V/v báo cáo chuyển đổi số", "UBND tỉnh Đồng Nai", ctx);
        var a = await thamMuu.AdviseAsync(den, "Công văn", "V/v báo cáo chuyển đổi số", "UBND tỉnh Đồng Nai", ctx);
        Assert.False(string.IsNullOrWhiteSpace(a.Summary));
        Ghi("tham-muu", u, thamMuu.LuotCuoi);

        var baoCao = new PeriodicReportService(c);
        const string soLieu = "Thu ngân sách: 1,2 tỷ đồng (kỳ trước 1,0 tỷ đồng).\nCấp 25 giấy chứng nhận quyền sử dụng đất.";
        u = await baoCao.UocTinhAsync("Tháng", "Tháng 10/2026", "Kinh tế - Xã hội", "UBND xã Minh An", soLieu, null, "Nguyễn Văn An", "CHỦ TỊCH");
        var bc = await baoCao.GenerateReportAsync("Tháng", "Tháng 10/2026", "Kinh tế - Xã hội", "UBND xã Minh An", soLieu, null, "Nguyễn Văn An", "CHỦ TỊCH");
        Assert.True(bc.Length > 100);
        Ghi("bao-cao-dinh-ky", u, baoCao.LuotCuoi);

        var trichXuat = new TrichXuatService(c);
        var anh = new[] { (TaoAnhGiayMoi(), "image/png") };
        u = await trichXuat.UocTinhAsync(anh);
        var x = await trichXuat.ExtractDocumentFromMultipleFilesAsync(anh);
        Assert.Contains("15", x.SoVanBan);
        Ghi("trich-xuat cau-truc", u, trichXuat.LuotCuoi);
        u = await trichXuat.UocTinhAsync(anh, chiChu: true);
        var chu = await trichXuat.ReadTextAsync(anh[0].Item1, "image/png");
        Assert.Contains("giấy mời", chu.ToLowerInvariant());
        Ghi("trich-xuat chi-chu", u, trichXuat.LuotCuoi);

        var soanThao = new TacVuSoanThao(c);
        var truong = new Dictionary<string, string>
        {
            ["from_org"] = "UBND xã Minh An",
            ["to_org"] = "UBND tỉnh Đồng Nai",
            ["subject"] = "Báo cáo công tác chuẩn bị mùa mưa bão năm 2026",
            ["content"] = "Báo cáo ngắn về công tác chuẩn bị phòng chống thiên tai mùa mưa bão năm 2026.",
            ["signer_name"] = "Nguyễn Văn An",
            ["signer_title"] = "CHỦ TỊCH",
        };
        var dp = new DiaPhuongCoQuan("xã Minh An", "tỉnh Đồng Nai", "tỉnh Đồng Nai");
        u = await soanThao.UocTinhAsync(DocumentType.CongVan, truong, dp);
        var vb = await soanThao.SoanAsync(DocumentType.CongVan, truong, dp);
        Assert.Contains("Minh An", vb);
        Assert.DoesNotContain("Gia Kiệm", vb);
        Ghi("soan-thao", u, soanThao.LuotCuoi);
    }
}
```

Run: `cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests/AIVanBan.Tests.csproj -nologo 2>&1 | tail -5`
Expected: `Passed!` (không có biến môi trường → bài chạy thật kết thúc ngay).

- [ ] **Step 3: Build Desktop + test, commit phiên bản**

Run hai lệnh ở mục «Lệnh build / test». Expected: xanh.

```bash
cd /d/AIVanBanCaNhan && git add AIVanBan.Desktop/AIVanBan.Desktop.csproj setup-script.iss setup-vanbanplus.iss update.xml AIVanBan.Tests/TacVuThatTests.cs
git commit -m "$(cat <<'EOF'
v1.0.20: ban cai may goi API tac vu v2 (bao truoc credit, khong prompt C#, khong goi thang Gemini)

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
)"
```

- [ ] **Step 4: Tài khoản thử có credit (cần chủ dự án)**

Hỏi chủ dự án email tài khoản thử dùng cho bản cài máy (tạo mới ở `/dang-ky` nếu chưa có) và xin cộng **≥ 150 credit** cho tài khoản đó ở `/admin/credit` (ghi chú «thử bản cài máy 1.0.20»). Chờ trả lời; không tự cộng credit trên prod.

Lấy khóa API vào tệp bí mật **không in ra** (thay `EMAIL_THU` bằng email chủ dự án đưa):

```bash
mkdir -p /c/Users/PC/.gikidi
ssh gikidi-dev "sudo docker exec supabase-db-q323olywghtnjcvpyvtjzo9v psql -U postgres -d postgres -Atc \"select api_key from users where email = 'EMAIL_THU'\"" | sed 's/^/VBP_E2E_API_KEY=/' > /c/Users/PC/.gikidi/vanbanplus-thu.env
wc -c /c/Users/PC/.gikidi/vanbanplus-thu.env
```

Expected: tệp dài hơn 20 byte (một dòng `VBP_E2E_API_KEY=vbp_…`). Không `cat` tệp này.

- [ ] **Step 5: Chạy thật 6 tác vụ (đường Core, chính mã của bản cài)**

```bash
cd /d/AIVanBanCaNhan && set -a && . /c/Users/PC/.gikidi/vanbanplus-thu.env && set +a && \
dotnet test AIVanBan.Tests/AIVanBan.Tests.csproj -nologo --filter "FullyQualifiedName~TacVuThatTests" --logger "console;verbosity=detailed" 2>&1 | grep -E "OK |BỎ QUA|Passed!|Failed|Assert|TacVuException" | head -30
```

Expected: 7 dòng `OK …` (tom-tat, kiem-tra, tham-muu, bao-cao-dinh-ky, trich-xuat cau-truc, trich-xuat chi-chu, soan-thao), mỗi dòng `tru` nằm trong khoảng ước tính; `Passed!`. Không có dòng `BỎ QUA`.

Hỏng → đọc `TacVuException.Message`/`MaHttp`. Lỗi do bản cài (sai tên trường, sai kiểu) → sửa, thêm bài kiểm đơn vị bắt lỗi đó, commit, chạy lại. Lỗi phía máy chủ (500, `AI_LOI` lặp lại, kết quả sai model) → DỪNG, báo chủ dự án kèm `ma_luot`; không sửa `vanbanplus-api` trong kế hoạch này.

- [ ] **Step 6: Đối chiếu sổ lượt trên production (chỉ đọc)**

```bash
ssh gikidi-dev "sudo docker exec supabase-db-q323olywghtnjcvpyvtjzo9v psql -U postgres -d postgres -Atc \"select tac_vu, trang_thai, credit_min, credit_max, credit_tru, ma_loi from tac_vu_chay where nguon_goi = 'desktop' and phien_ban_ung_dung = '1.0.20' and bat_dau_luc > now() - interval '2 hours' order by bat_dau_luc\""
```

Expected: 7 dòng `xong`, `credit_tru` khớp các dòng `OK` ở Step 5, `ma_loi` rỗng — chứng tỏ máy chủ nhận `X-VBP-Nguon: desktop` và `X-VBP-Phien-Ban: 1.0.20`. (Tên cột theo spec §7; khác thì xem `\d tac_vu_chay`.)

- [ ] **Step 7: Build bộ cài**

Tắt VanBanPlus đang chạy.

```bash
cd /d/AIVanBanCaNhan && rm -rf AIVanBan.Desktop/bin/Release/net9.0-windows/win-x64/publish && \
dotnet publish AIVanBan.Desktop/AIVanBan.Desktop.csproj -c Release -r win-x64 --self-contained true -nologo 2>&1 | tail -3 && \
"/c/Users/PC/AppData/Local/Programs/Inno Setup 6/ISCC.exe" setup-vanbanplus.iss 2>&1 | tail -3 && \
ls -la Installer/VanBanPlus-Setup-1.0.20-x64.exe
```

Expected: publish xong vào `AIVanBan.Desktop/bin/Release/net9.0-windows/win-x64/publish/`; ISCC báo `Successful compile`; tệp exe khoảng 50–60 MB.

Kiểm tiêu chí spec §1.3 trên chính bản build (PowerShell):

```powershell
$p = 'D:\AIVanBanCaNhan\AIVanBan.Desktop\bin\Release\net9.0-windows\win-x64\publish'
(Get-Item "$p\AIVanBan.Desktop.exe").VersionInfo.FileVersion
$banLa = 'B' + [char]0x1EA1 + 'n l' + [char]0x00E0
foreach ($f in 'AIVanBan.Core.dll', 'AIVanBan.Desktop.dll') {
  $s = [Text.Encoding]::Unicode.GetString([IO.File]::ReadAllBytes("$p\$f"))
  "$f prompt=$($s.Contains($banLa)) gemini=$($s.Contains('generativelanguage')) apiai=$($s.Contains('/api/ai/')) v2=$($s.Contains('/api/v2/tac-vu/'))"
}
```

Expected: `1.0.20.0`; `AIVanBan.Core.dll prompt=False gemini=False apiai=False v2=True`; `AIVanBan.Desktop.dll prompt=False gemini=False apiai=False v2=False`.

- [ ] **Step 8: Kiểm giao diện (chủ dự án hoặc người làm, trên PC)**

Sao lưu thiết lập trước (bản mới lưu lại `settings.json` không còn khóa chế độ cũ):

```bash
cp "/c/Users/PC/Documents/AIVanBan/settings.json" "C:/Users/PC/AppData/Local/Temp/claude/settings-truoc-1.0.20.json"
```

Mở `AIVanBan.Desktop/bin/Release/net9.0-windows/win-x64/publish/AIVanBan.Desktop.exe` (không cần cài; kênh cập nhật vẫn báo 1.0.19 nên không hiện hộp thoại cập nhật), dùng tài khoản đang đăng nhập trên máy. Mỗi mục: hộp «Xác nhận dùng credit» hiện khoảng + số dư; bấm «No» thì không chạy; bấm «Yes» thì ra kết quả như bản cũ.

1. Tóm tắt (danh sách văn bản → Tóm tắt), có thử «Thử lại».
2. Kiểm tra văn bản, có tải tệp mẫu `.pdf` đối chiếu (hộp xác nhận «Đọc chữ từ tệp PDF»).
3. Tham mưu văn bản đến có số, ngày, hạn xử lý.
4. Báo cáo định kỳ (hộp xác nhận gộp mô tả cũ + credit).
5. Scan Import: 2 ảnh ghép trang, rồi 2 ảnh tách riêng.
6. AI Soạn thảo một Công văn; Cài đặt → 🏛️ Cơ quan thấy ba ô địa phương (mặc định Gia Kiệm nếu chưa lưu); đổi sang xã khác, lưu, soạn lại → văn bản theo xã mới.
7. Cài đặt → bấm tiêu đề 5 lần: không còn chế độ bảo trì.

Ghi kết quả từng mục (đạt/không, ảnh chụp nếu lỗi). Xong, trả lại thiết lập nếu cần: `cp "C:/Users/PC/AppData/Local/Temp/claude/settings-truoc-1.0.20.json" "/c/Users/PC/Documents/AIVanBan/settings.json"`.

- [ ] **Step 9: 🛑 DỪNG — chờ chủ dự án cho phép phát hành**

Phát hành = mọi máy đang chạy VanBanPlus nhận lời mời cập nhật. **Không** tạo GitHub release, **không** sửa `update.xml` trên repo releases, **không** đẩy `vanbanplus-api`, **không** gộp nhánh vào `main` khi chưa có lời cho phép rõ ràng của chủ dự án trong phiên.

Báo chủ dự án (không đại từ nhân xưng): kết quả 7 lượt chạy thật (credit ước tính/trừ), kết quả 7 mục kiểm giao diện, đường dẫn bộ cài `D:\AIVanBanCaNhan\Installer\VanBanPlus-Setup-1.0.20-x64.exe`, cờ `mandatory=false` (hỏi có muốn bắt buộc không), và câu hỏi: phát hành 1.0.20 cho mọi người dùng?

---

### Task 13: Phát hành (CHỈ sau khi chủ dự án cho phép)

**Files:**
- Repo releases `D:\van-ban-plus-releases`: `update.xml`
- Repo `vanbanplus-api`: `src/app/tai-xuong/page.tsx`, `src/app/dang-nhap/page.tsx`, `src/components/layout/Footer.tsx`, `src/app/huong-dan/page.tsx`
- Repo này: `CHANGELOG_v1.0.20.md` (ngày), `docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md` (dòng trạng thái), `PROJECT_STATUS.md` (dòng phiên bản), con trỏ `vanbanplus-api`

**Interfaces:**
- Consumes: bộ cài Task 12; lời cho phép của chủ dự án (kèm quyết định `mandatory`).
- Produces: bản 1.0.20 trên kênh tự cập nhật và trang tải xuống.

Thứ tự bắt buộc: tệp exe lên GitHub trước → `update.xml` sau (máy khách không tải phải đường dẫn 404) → trang web.

- [ ] **Step 1: GitHub release**

```bash
cd /d/AIVanBanCaNhan && gh release create v1.0.20 Installer/VanBanPlus-Setup-1.0.20-x64.exe \
  -R ericphan28/van-ban-plus-releases --title "VanBanPlus v1.0.20" --notes-file CHANGELOG_v1.0.20.md
curl -sIL https://github.com/ericphan28/van-ban-plus-releases/releases/download/v1.0.20/VanBanPlus-Setup-1.0.20-x64.exe | grep -iE "^HTTP|content-length" | tail -2
```

Expected: release tạo xong; `HTTP/2 200` và `content-length` bằng kích thước tệp exe. (Điền ngày phát hành vào `CHANGELOG_v1.0.20.md` trước lệnh này.)

- [ ] **Step 2: Kênh tự cập nhật**

```bash
cd /d/van-ban-plus-releases && git status --short && git pull --ff-only && cat update.xml | head -3
cp /d/AIVanBanCaNhan/update.xml update.xml   # nếu chủ dự án chọn bắt buộc: sửa <mandatory>true</mandatory> ở cả hai bản
git add update.xml && git commit -m "$(cat <<'EOF'
Bump update.xml to v1.0.20

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
)" && git push
curl -s https://raw.githubusercontent.com/ericphan28/van-ban-plus-releases/main/update.xml
```

Expected: trước khi sửa, `update.xml` là 1.0.19 (bản clone đã kéo về đúng); sau khi đẩy, URL raw trả `<version>1.0.20</version>` (raw.githubusercontent có thể chậm vài phút).

- [ ] **Step 3: Trang web (`vanbanplus-api`, đẩy main = production)**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git status --short && git switch main && git pull --ff-only && git switch -c tai-xuong-1.0.20
ls -l ../Installer/VanBanPlus-Setup-1.0.20-x64.exe | awk '{printf "%.1f MB\n", $5/1048576}'
```

Sửa:
- `src/app/tai-xuong/page.tsx`: thêm phần tử đầu mảng `CHANGELOG`:

```tsx
  {
    version: "1.0.20",
    date: "<ngày phát hành, dd/MM/yyyy>",
    changes: [
      "💳 Báo trước khoảng credit và số dư trước mỗi tác vụ AI",
      "💬 Thông báo lỗi rõ ràng: hết credit (mở sẵn trang mua), quá tải, quá dài, mất kết nối; tác vụ lỗi được hoàn credit",
      "🏛️ AI soạn thảo theo địa phương của cơ quan (Cài đặt → Cơ quan)",
      "🔒 AI chạy hoàn toàn qua máy chủ VanBanPlus, bỏ chế độ kết nối AI trực tiếp",
    ],
  },
```

  và đổi ba chỗ `v1.0.19`/`1.0.19` (dòng «Phiên bản mới nhất», `href` tải về, nhãn nút) sang `1.0.20`; `53.9 MB` → kích thước vừa in.
- `src/app/dang-nhap/page.tsx`, `src/components/layout/Footer.tsx`: `v1.0.19` → `v1.0.20`.
- `src/app/huong-dan/page.tsx`: xóa phần tử `{ q: "Chọn AI provider", a: "Vào Cài đặt → AI Provider → ..." },`; thay phần tử FAQ `{ q: "AI có gửi nội dung VB lên server không?", a: "..." }` bằng `{ q: "AI có gửi nội dung văn bản lên máy chủ không?", a: "Văn bản được gửi qua máy chủ VanBanPlus tới nhà cung cấp AI để xử lý. Máy chủ VanBanPlus mặc định không lưu văn bản." },`.

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && grep -rn "1\.0\.19" src | grep -v "version: \"1.0.19\"" ; npm test 2>&1 | tail -3 && npx next build 2>&1 | tail -5
```

Expected: grep trống (chỉ còn mục 1.0.19 trong lịch sử thay đổi); `npm test` xanh; `next build` xong không lỗi.

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git add src/app/tai-xuong/page.tsx src/app/dang-nhap/page.tsx src/components/layout/Footer.tsx src/app/huong-dan/page.tsx
git commit -m "$(cat <<'EOF'
feat: trang tai xuong v1.0.20; huong dan bo muc chon AI provider, sua cau luu noi dung

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
)"
git switch main && git merge --ff-only tai-xuong-1.0.20 && git push origin main
```

Chờ Coolify build xong (2–5 phút), rồi:

```bash
curl -s https://vanbanplus.giakiemso.com/tai-xuong | grep -o "Phiên bản mới nhất: v1.0.2[0-9]"
```

Expected: `Phiên bản mới nhất: v1.0.20`.

- [ ] **Step 4: Repo này — gộp nhánh, ghi trạng thái**

- `docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md` dòng 3: nối vào cuối câu trạng thái `; đợt 3 (bản cài máy v1.0.20 gọi v2) phát hành <ngày>`.
- `PROJECT_STATUS.md`: dòng `> **Phiên bản hiện tại:**` → `v1.0.20 (AI chạy qua API tác vụ v2 — phát hành <ngày yyyy-MM-dd>)`; dòng đợt 3 trong bảng: `(chờ phát hành)` → `(phát hành <ngày>)`.

```bash
cd /d/AIVanBanCaNhan && git add CHANGELOG_v1.0.20.md PROJECT_STATUS.md docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md vanbanplus-api
git commit -m "$(cat <<'EOF'
docs: phat hanh v1.0.20; cap nhat vanbanplus-api (trang tai xuong v1.0.20)

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
)"
git switch main && git merge --no-ff dot3-ban-cai-may-v2 -m "$(cat <<'EOF'
Merge dot3: ban cai may v1.0.20 goi API tac vu v2

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
)"
git log --oneline -3
```

Không đẩy repo này (repo không tự triển khai; đẩy khi chủ dự án yêu cầu).

- [ ] **Step 5: Theo dõi sau phát hành**

Sau 24 giờ, đếm lượt theo phiên bản (chỉ đọc):

```bash
ssh gikidi-dev "sudo docker exec supabase-db-q323olywghtnjcvpyvtjzo9v psql -U postgres -d postgres -Atc \"select phien_ban_ung_dung, nguon_goi, trang_thai, ma_loi, count(*) from tac_vu_chay where bat_dau_luc > now() - interval '1 day' group by 1,2,3,4 order by 5 desc\""
```

Báo chủ dự án nếu `ma_loi` nào (`AI_QUA_TAI`, `NGAT_KET_NOI`, `QUA_DAI`) chiếm phần lớn lượt của bản 1.0.20.

---

## Tự soát kế hoạch so với yêu cầu

| Yêu cầu | Task |
| --- | --- |
| Client C# v2: `UocTinhAsync(ten, input)`, `ChayAsync<T>(ten, input)`, dùng URL/khóa từ thiết lập, header `X-VBP-Nguon`/`X-VBP-Phien-Ban` (phiên bản từ csproj qua assembly) | 2 |
| Lỗi có kiểu cho `KHONG_DU_CREDIT`, `QUA_NHIEU_LUOT`, `QUA_DAI`, `DAU_VAO_SAI`, `AI_QUA_TAI` (+Retry-After), `AI_LOI`, `NGAT_KET_NOI`, 401, 404, 500; lời tiếng Việt có dấu, không đại từ | 2 (`MaLoiTacVu`, `TaoLoi`, bài `LoiBaoKhach_…`) |
| Hạn chờ rộng (47 giây đo được → 180 / 300 giây) | 2 |
| Kiểm thử bằng HttpMessageHandler giả | 2–7, 9 |
| 6 đường gọi giữ model cũ: tom-tat, kiem-tra, tham-muu, bao-cao-dinh-ky, trich-xuat (kể cả ghép trang gọi thẳng Gemini + đọc chữ), soan-thao | 3, 4, 5, 6, 9, 7 |
| Mọi nơi gọi đã rà (bảng «Hiện trạng mã»), không còn `/api/ai/*` | 9 Step 6, 10 Step 5, 12 Step 7 |
| `dia_phuong` từ thiết lập cơ quan; thêm trường vì chưa có; mặc định Gia Kiệm khi chưa lưu | 7 |
| `ngu_canh` đúng hợp đồng, ngày ISO | 5 |
| Hộp thoại hiện khoảng credit + số dư trước khi chạy, xử lý mã lỗi, `KHONG_DU_CREDIT` mở `/bang-gia` | 3 (`TacVuGiaoDien`), 4–7, 9 |
| Xóa prompt C#, LỚP-DOT2B, `MauPromptTests` (tệp mẫu thành mốc cố định) | 8, 9 (prompt OCR) |
| Bỏ `rbGeminiDirect`, `_fallbackGeminiKey`, `DevModePolicy`, khóa riêng chỉ phục vụ chế độ thẳng | 9 (`_fallbackGeminiKey` cùng `GeminiAIService`), 10 |
| Phiên bản, kênh cập nhật, build, chạy thật 6 tác vụ với tài khoản thử, DỪNG chờ cho phép | 12 |
| Trợ giúp + `PROJECT_STATUS.md` (nguyên tắc «không gọi thẳng Gemini», giá cũ) | 11, 13 (trang web) |
| Cổng phụ thuộc: 6 tác vụ trả 401 không phải 404 | 1 |
| Mỗi task giữ build xanh; lệnh `dotnet build`/`dotnet test`; lưu ý WPF | mục «Lệnh build / test», mọi task |

## Điểm cần chủ dự án quyết

1. **Mặc định địa phương:** máy chưa từng lưu ba ô địa phương sẽ gửi Gia Kiệm (giữ đúng hành vi các bản trước). Khách ở xã khác chưa mở Cài đặt vẫn nhận bối cảnh Gia Kiệm như hiện nay. Phương án khác: chỉ mặc định Gia Kiệm khi tên cơ quan trống hoặc chứa «Gia Kiệm».
2. **Tài khoản thử + credit** cho Task 12 (≥ 150 credit, cộng ở `/admin/credit`).
3. **Cờ `mandatory`** của bản 1.0.20 (kế hoạch để `false`; bản cũ vẫn chạy qua `/api/ai/*`).
4. **Hộp xác nhận trước mỗi lần chạy** (Yes/No, kiểu các hộp thoại hiện có). Phương án khác: chỉ hiện khoảng credit trong dòng trạng thái, không hỏi.
5. **Không tự thử lại** khi `AI_QUA_TAI` (bản cũ tự thử 3 lần): mỗi lần chạy là một lượt giữ credit; lời báo ghi số giây chờ.

## Giả định

- `dot2a`, `dot2b`, `dot2c` đã gộp vào `main` và lên production trước khi bắt đầu (Task 1 kiểm).
- `/uoc-tinh` nhận cùng thân với lượt chạy (hợp đồng không nêu thân riêng); với `trich-xuat` nghĩa là gửi tệp hai lần.
- `ket_qua` của tác vụ JSON dùng đúng tên trường của model C# (hợp đồng); `trich-xuat` `cau-truc` dùng tên snake_case mà `ParseExtractedDocument` đang đọc (`so_van_ban`, `noi_nhan`, …).
- Trang mua credit là `/bang-gia` (cả gói tháng lẫn gói nạp, nếu dot2c đã đưa lên).
- Phiên bản phát hành là 1.0.20.
