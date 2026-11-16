# Đợt 2b: tác vụ `trich-xuat` và `soan-thao`, kế hoạch triển khai

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Đưa hai tác vụ AI cuối cùng của spec mục 5 về máy chủ: `POST /api/v2/tac-vu/trich-xuat` (đọc tệp ảnh/PDF, có cấu trúc hoặc chỉ chữ) và `POST /api/v2/tac-vu/soan-thao` (soạn văn bản theo loại + trường), kèm ước tính credit, bảng giá `008`, rồi lên production.

**Architecture:** Luồng chung `chayTacVu` được mở rộng theo kiểu **chỉ thêm, mọi thứ đều tùy chọn**: `PromptTacVu` có thêm tệp gửi kèm, lược đồ JSON, cờ tắt suy nghĩ và kiểu kết quả ghi đè. `PhuThuoc` có thêm trần dung lượng theo gói. Tác vụ chỉ có chữ (`tom-tat`, ba tác vụ `dot2a`) gửi AI đúng như cũ. Prompt `trich-xuat` lấy từ máy chủ (nguồn chuẩn), được chuyển sang một module dùng chung mà route cũ `/api/ai/*` và tác vụ mới cùng import. Prompt `soan-thao` trước hết được tách khỏi `AIComposeDialog.xaml.cs` sang lớp thuần `ComposePromptBuilder` trong `AIVanBan.Core` (không đổi hành vi, có bằng chứng so với hộp thoại cũ). Lớp này sinh tệp mẫu, và bộ kiểm TypeScript so từng ký tự với tệp mẫu đó.

**Tech Stack:** Next.js 16 (App Router), TypeScript, `tsx --test`, `pg` (chỉ trong bộ kiểm), Postgres 17 (`supabase/postgres:17.6.1.155`), .NET 9 + xUnit, WPF (chỉ để đọc hộp thoại cũ bằng reflection trong bộ kiểm), Node ≥ 20 cho script một lần.

**Spec:** [docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md](../specs/2026-11-09-api-tac-vu-ai-credit-design.md). Hợp đồng dùng chung: [2026-11-16-hop-dong-tac-vu-v2.md](2026-11-16-hop-dong-tac-vu-v2.md). Brief: `.superpowers/lap-ke-hoach-dot2-3.md`.

## Tiền đề

- `dot2a` (kiem-tra, tham-muu, bao-cao-dinh-ky) **đã gộp vào `main`** của `vanbanplus-api` và đã chạy trên prod. Phần của `dot2a` mà kế hoạch này chạm tới: thêm mục trong `src/lib/tac-vu/dang-ky.ts`, thêm hàm sinh trong `AIVanBan.Tests/MauPromptTests.cs`, và migration `007_bang_gia_dot2a.sql`. Task 0 kiểm lại trước khi bắt đầu.
- `dot2c` chạy SAU kế hoạch này. Nó thêm phụ thuộc lưu hỗ trợ vào `PhuThuoc`, nên kế hoạch này chỉ thêm các trường tùy chọn và không đổi chữ ký nào đã có.

## Global Constraints

- Hai repo: mã C# + tài liệu ở `D:/AIVanBanCaNhan` (nhánh `main`, chỉ commit tại chỗ, **không đẩy**). API ở `D:/AIVanBanCaNhan/vanbanplus-api` (git riêng). **Đẩy `main` của `vanbanplus-api` là lên production** (Coolify tự build). Làm trên nhánh `dot2b`, chỉ gộp vào `main` ở Task 10.
- Không đụng `docs/thue/` hay bất kỳ thay đổi chưa commit lạ nào (đó là của phiên khác). Mỗi lần commit chỉ `git add` đúng các tệp đã liệt kê.
- Tệp dùng chung chỉ sửa kiểu THÊM, xác định chỗ sửa bằng mốc văn bản: `src/lib/tac-vu/dang-ky.ts` (thêm import + mục), `AIVanBan.Tests/MauPromptTests.cs` (thêm một hàm sinh). Không sửa `tests/ho-tro/csdl-thu.ts` và `tests/chay-tac-vu.test.ts`; bài kiểm mới nằm ở tệp mới.
- `chay-tac-vu.ts`: chỉ thêm trường/phụ thuộc **tùy chọn**. Không có tệp, lược đồ hay cờ thì lời gọi AI (contents + tùy chọn) phải giống hệt trước. Có bài kiểm chứng minh việc này (Task 2).
- **Migrate prod TRƯỚC khi đẩy mã cần lược đồ mới.** `008_bang_gia_dot2b.sql` chỉ `INSERT … ON CONFLICT DO NOTHING`, thử trên bản sao prod trước, chạy bằng `psql -1 -v ON_ERROR_STOP=1`, xong thì `NOTIFY pgrst, 'reload schema'`.
- CSDL prod: container `supabase-db-q323olywghtnjcvpyvtjzo9v` (`ssh gikidi-dev`, `sudo docker exec …`). Không in khóa API, chuỗi kết nối, bí mật nào ra chat hay log.
- CSDL thử: `bash scripts/csdl-thu.sh bat` + đường hầm `ssh -N -L 55990:127.0.0.1:55990 gikidi-dev`, rồi `TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres`.
- Tác vụ v2 chỉ dùng khóa hệ thống (đã có trong `phuThuocThat`). Mô hình đọc từ `credit_bang_gia` (`cau_hinh/chung`).
- Lời hiện cho khách (thông báo lỗi): tiếng Việt có dấu, **không đại từ nhân xưng** (tôi, bạn, mình, anh, chị, em, chúng tôi, ta), không dạ/vâng/ạ. Prompt AI chép từ nguồn thì giữ nguyên văn (kể cả «Bạn là…», «Tôi gửi…»).
- Không lưu, không log nội dung văn bản hay tệp. `tac_vu_chay` không có cột nội dung.
- Prompt chép từ C#/route: **không sửa tệp mẫu bằng tay**. Bộ kiểm so khớp hỏng thì sửa mã TS cho khớp tệp mẫu.
- Đơn vị gọi là «credit».
- Commit message không dấu (giống lịch sử repo), kết thúc bằng dòng trống rồi `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Script một lần (sinh tệp mẫu, tách mã C#, tạo tệp thử) đặt ở `D:/AIVanBanCaNhan/.superpowers/tam/` (thư mục chưa theo dõi), **không commit**.

## Quyết định thiết kế (đọc trước khi làm)

1. **Nguồn chuẩn của prompt trích xuất là máy chủ** (`EXTRACT_PROMPT`, `EXTRACT_SCHEMA`, `READ_PROMPT` trong `src/app/api/ai/[action]/route.ts` + `multiPagePrompt` trong `src/lib/extract-input.ts`). Spec mục 2 ghi hai bản. Bản cài máy ở chế độ API đã dùng bản máy chủ; bản `GeminiAIService` chỉ chạy ở chế độ gọi thẳng Gemini, và đợt 3 sẽ bỏ chế độ đó. Khác biệt của bản C# (ghi lại, KHÔNG chép sang):

   | Điểm | Máy chủ (chuẩn) | `GeminiAIService` (C#) |
   | --- | --- | --- |
   | `noi_dung` | «Trích xuất NGUYÊN VĂN **toàn bộ** nội dung văn bản gốc» | «CHỈ lấy PHẦN NỘI DUNG CHÍNH», loại trừ quốc hiệu, số, ngày, căn cứ, nơi nhận, chữ ký; bắt buộc `\n` giữa điều/khoản |
   | Danh sách `loai_van_ban` | Chỉ nêu «chọn 1 trong 32 loại»; danh sách nằm trong `enum` của lược đồ | Liệt kê 33 tên ngay trong prompt |
   | `huong_van_ban` | Không có quy tắc riêng | Quy tắc 4 «chọn 1 trong Den\|Di\|NoiBo» |
   | Lược đồ | 15 trường, `required` 7 trường, mô tả ngắn | 15 trường, `required` 14 trường, mô tả dài hơn (vd `dia_danh` «VD: Gia Kiểm, Biên Hòa, Hà Nội») |
   | Nhiều tệp | Prompt một tệp + đoạn «GHÉP TRANG» (`multiPagePrompt`) | Prompt riêng «Tôi gửi cho bạn {N} ảnh/file…», 9 quy tắc |
   | Đọc chữ | 2 dòng, `maxTokens` 8192, không tắt suy nghĩ | 3 dòng (có dấu cách thừa cuối dòng 2), `maxTokens` 16384 |
   | Cấu hình | `temperature` 0,1; lược đồ + `disableThinking` cho trích xuất | như máy chủ |

2. **`trich-xuat` trên luồng chung:** `he_thong = ""` (route cũ không gửi system instruction; `callAI` bỏ qua chuỗi rỗng). `nguoi_dung` là prompt, các tệp gửi sau phần chữ theo đúng thứ tự (giống route cũ). `cau-truc` dùng lược đồ + tắt suy nghĩ, kết quả JSON (tên trường snake_case như lược đồ, đúng chỗ `ParseExtractedDocument` của C# đang đọc). `chi-chu` trả chuỗi, **nhận đúng 1 tệp** (đường `/api/ai/read-text` chỉ có một tệp), và **cũng tắt suy nghĩ**. Đây là lệch chủ ý so với route cũ: OCR không cần suy luận, tắt thì đỡ credit cho khách và trần token ra mới có nghĩa. Văn bản prompt không đổi.
3. **Trần dung lượng:** tổng byte sau giải base64 > `min(max_file_size_mb của gói hiện hành, 20 MB)` thì trả `QUA_DAI` 413. Gói hiện hành dùng `effectivePlanId` (gói hết hạn thì tính như `free`). 20 MB là giới hạn dữ liệu nhúng của Gemini (`MAX_EXTRACT_BYTES`). `next.config.ts` đã nâng `proxyClientMaxBodySize` lên 30 MB nên thân yêu cầu không bị cắt.
4. **`toi_da_ky_tu` với tác vụ có tệp:** vẫn giữ một nghĩa, là số ký tự tối đa của **phần chữ** (`nguoi_dung`). Phần chữ của `trich-xuat` cố định (khoảng 1.200 ký tự) nên đặt 10.000. Tệp thì giới hạn theo byte (mục 3) và theo số tệp (1–20, sai thì `DAU_VAO_SAI`).
5. **Ước tính có tệp (spec mục 7):** `token_vao = ceil(số ký tự / 3) + số trang × token_moi_trang`. Số ký tự gồm cả lược đồ JSON (lược đồ cũng tính là token vào). Ảnh tính 1 trang. PDF đếm `/Type /Page` trong tệp; không đếm được (luồng đối tượng nén) thì lấy `ceil(byte / 150 KB)`, tối thiểu 1. `token_moi_trang` là trường **tùy chọn** mới của cấu hình tác vụ trong bảng giá (sửa được ở `/admin/credit`), đặt 1.100 (Gemini 3: ảnh khoảng 1.120 token, trang PDF khoảng 560; lấy mức cao cho an toàn, đo lại trên `tac_vu_chay` sau khi có khách). Hệ số đã chỉnh để khớp spec «3–9 credit cho 1–6 trang»: 1 ảnh → 3–11, 6 ảnh → 8–29 (`credit_max` là phần giữ tạm và cũng là trần token ra; trừ thật theo token).
6. **`soan-thao`:** đầu vào `loai_van_ban` (tên enum `DocumentType`), `truong` (khóa → chuỗi, giữ thứ tự), cùng hai trường tùy chọn **bổ sung vào hợp đồng** (Task 0):
   - `ten_mau`: tên mẫu văn bản. Các hàm dựng QuyetDinh, BaoCao, ThongBao, NghiQuyet (`Contains("HĐND")`) và nhánh chung dùng `_selectedTemplate.Name`. Thiếu trường này thì không tái tạo được prompt cũ.
   - `dia_phuong.dac_diem`: mảng chuỗi. Khối «BỐI CẢNH ĐỊA PHƯƠNG» cũ có ba dòng riêng của Gia Kiệm (dân số, ấp/giáo xứ, xã loại I) mà ba trường `xa`/`co_quan_cap_tren`/`tinh` không sinh ra được. Thiếu trường này thì tệp mẫu Gia Kiệm không khớp từng ký tự.
   - `dia_phuong.co_quan_cap_tren` thành tùy chọn (chính quyền hai cấp). Bỏ trống thì cấp trên là tỉnh.

   Địa phương dẫn ra các chuỗi sau (Gia Kiệm cho ra đúng chữ cũ, đã kiểm bằng Python trên tệp nguồn):

   | Chỗ trong prompt | Có `dia_phuong` | Không có |
   | --- | --- | --- |
   | Hệ thống, mục 2 «Cơ quan cấp trên (nếu có)» | `UBND ` + HOA(`co_quan_cap_tren` hoặc `tinh`) | `[CƠ QUAN CẤP TRÊN]` |
   | Hệ thống, mục 2 «Cơ quan ban hành»; ThongBao «2. Tên cơ quan:» | `UBND ` + HOA(`xa`) | `[CƠ QUAN BAN HÀNH]` |
   | Hệ thống, mục 4 địa danh | `xa` bỏ tiền tố «xã/phường/thị trấn/đặc khu» | `[Địa danh]` |
   | Hệ thống, khối «BỐI CẢNH ĐỊA PHƯƠNG» | dựng lại từ `xa`, `co_quan_cap_tren`, `tinh`, `dac_diem` | bỏ cả khối |
   | CongVan «… xin trả lời như sau»; KeHoach khi thiếu `org_name` | `UBND ` + `xa` | `[cơ quan ban hành]` |
   | QuyetDinh «2. Tên cơ quan ban hành (…)» | ` (UBND ` + HOA(`xa`) + `)` | (bỏ phần ngoặc) |
   | NghiQuyet tên cơ quan, «… QUYẾT NGHỊ:» | HOA(`xa`) | `[TÊN XÃ/PHƯỜNG]` |

   «năm 2026» ghi cứng ở mục 4 của prompt hệ thống được giữ nguyên văn. Sửa nội dung prompt là việc riêng (spec mục 10).
7. **Bằng chứng không đổi hành vi khi tách C#:** trước khi tạo lớp mới, chạy hai hàm prompt riêng tư của hộp thoại **cũ** bằng reflection trên một đối tượng chưa khởi tạo (`RuntimeHelpers.GetUninitializedObject`, luồng STA), chụp kết quả cho 42 ca Gia Kiệm vào `AIVanBan.Tests/MauCu/soan-thao-gia-kiem.json`. Sau đó cả lớp mới lẫn hộp thoại đã sửa đều phải khớp bản chụp đó. Bản chụp không bao giờ được sinh lại sau Task 5.

## Sơ đồ tệp

```
vanbanplus-api/
  src/lib/tac-vu/tep.ts                    MỚI  TepGui, soByteGiai, demTrang (Task 1)
  src/lib/credit/uoc-tinh.ts               SỬA  token_moi_trang?, uocTinh(..., soTrang = 0) (Task 1)
  src/lib/credit/bang-gia.ts               SỬA  đọc token_moi_trang tùy chọn (Task 1)
  src/lib/extract-input.ts                 SỬA  export decodedBytes (Task 1)
  src/lib/tac-vu/chay-tac-vu.ts            SỬA  tệp, lược đồ, tắt suy nghĩ, kiểu ghi đè, trần dung lượng (Task 2)
  src/lib/tac-vu/gioi-han-tep.ts           MỚI  trần byte theo gói (Task 2)
  src/lib/usage.ts                         SỬA  export effectivePlanId (Task 2)
  src/lib/tac-vu/phu-thuoc.ts              SỬA  toiDaTepByte (Task 2)
  src/lib/tac-vu/trich-xuat/prompt.ts      MỚI  hằng prompt chuyển từ route + dựng prompt (Task 3)
  src/app/api/ai/[action]/route.ts         SỬA  import hằng từ module trên (Task 3)
  src/lib/tac-vu/trich-xuat/dinh-nghia.ts  MỚI  (Task 4)
  src/lib/tac-vu/soan-thao/nhan-truong.ts  MỚI  sinh từ C# (Task 9)
  src/lib/tac-vu/soan-thao/prompt.ts       MỚI  (Task 9)
  src/lib/tac-vu/soan-thao/dinh-nghia.ts   MỚI  (Task 9)
  src/lib/tac-vu/dang-ky.ts                SỬA  thêm 2 mục (Task 4, 9)
  supabase/migrations/008_bang_gia_dot2b.sql  MỚI  (Task 4)
  tests/tep.test.ts, tests/chay-tac-vu-tep.test.ts, tests/tac-vu-trich-xuat.test.ts,
  tests/bang-gia-dot2b-csdl.test.ts, tests/tac-vu-soan-thao.test.ts       MỚI
  tests/mau-prompt/trich-xuat.json         MỚI  sinh từ route cũ (Task 3)
  tests/mau-prompt/soan-thao.json          MỚI  sinh từ C# (Task 8)
AIVanBan.Core/Services/ComposePromptBuilder.cs     MỚI  sinh bằng script từ hộp thoại (Task 6)
AIVanBan.Desktop/Views/AIComposeDialog.xaml.cs     SỬA  gọi lớp mới, xóa 26 hàm prompt (Task 7)
AIVanBan.Tests/AIVanBan.Tests.csproj               SỬA  tham chiếu AIVanBan.Desktop (Task 5)
AIVanBan.Tests/MauSoanThao.cs                      MỚI  các ca đầu vào (Task 5, 8)
AIVanBan.Tests/SoanThaoTuongDuongTests.cs          MỚI  so với hộp thoại cũ (Task 5, 6)
AIVanBan.Tests/MauCu/soan-thao-gia-kiem.json       MỚI  bản chụp hộp thoại cũ (Task 5)
AIVanBan.Tests/MauPromptTests.cs                   SỬA  thêm GhiMauSoanThao (Task 8)
docs/superpowers/plans/2026-11-16-hop-dong-tac-vu-v2.md   SỬA  bổ sung trường tùy chọn (Task 0)
```

---

### Task 0: Chuẩn bị nhánh, bổ sung hợp đồng

**Files:**
- Modify: `D:/AIVanBanCaNhan/docs/superpowers/plans/2026-11-16-hop-dong-tac-vu-v2.md`

**Interfaces:**
- Produces: hợp đồng có `ten_mau`, `dia_phuong.dac_diem`, `co_quan_cap_tren` tùy chọn, quy tắc mime/số tệp/chế độ của `trich-xuat`. Kế hoạch `dot3` (bản cài máy) dựa vào đây.

**Điểm dừng:** bước 2 là đổi hợp đồng dùng chung, nên phải được người điều phối / chủ dự án đồng ý (xem mục «Điểm cần chủ dự án quyết» ở cuối). Chưa được đồng ý thì dừng.

- [ ] **Step 1: Kiểm tiền đề**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git status --short && git checkout main && git pull --ff-only \
  && ls supabase/migrations/ && grep -n "tomTat\|kiemTra\|thamMuu\|baoCao" src/lib/tac-vu/dang-ky.ts && git log --oneline -5
```
Expected: cây sạch, có `007_bang_gia_dot2a.sql`, `dang-ky.ts` có mục của `dot2a`. Thiếu một trong ba thì DỪNG (dot2a chưa gộp). Cây không sạch thì DỪNG, hỏi trước (có thể là việc dở của phiên khác).

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git checkout -b dot2b && cd /d/AIVanBanCaNhan && git status --short
```
Expected: đang ở `dot2b`. Ở repo gốc chỉ có thay đổi lạ đã biết (`docs/thue/`, `.superpowers/`), không đụng.

- [ ] **Step 2: Bổ sung hợp đồng**

Trong `docs/superpowers/plans/2026-11-16-hop-dong-tac-vu-v2.md`, thay dòng `trich-xuat` và dòng `soan-thao` của bảng «Từng tác vụ»:

```markdown
| `trich-xuat` | `tep`* = mảng `{ base64, mime }` (1..20; mime ∈ `application/pdf`, `image/png`, `image/jpeg`, `image/webp`, `image/heic`, `image/heif`), `che_do` = `cau-truc` (mặc định) \| `chi-chu` (đúng 1 tệp) | `cau-truc`: JSON `ExtractedDocumentData` (tên trường snake_case của lược đồ máy chủ: `so_van_ban`, `trich_yeu`, …); `chi-chu`: chuỗi | `/api/ai/extract` + `/api/ai/read-text` của máy chủ (nguồn chuẩn); `GeminiAIService` chỉ để đối chiếu |
| `soan-thao` | `loai_van_ban`* (tên enum `DocumentType` C#, vd `CongVan`), `truong`* (đối tượng khóa → chuỗi, giữ thứ tự), `ten_mau` (tên mẫu văn bản đang chọn), `dia_phuong` (đối tượng, dưới) | chuỗi | `AIComposeDialog` (tách sang `AIVanBan.Core` `ComposePromptBuilder`) |
```

Thay đoạn **`dia_phuong` của `soan-thao`** bằng:

```markdown
**`dia_phuong` của `soan-thao`**: `{ xa*, co_quan_cap_tren, tinh*, dac_diem }` (`xa`, `tinh` bắt buộc khi có `dia_phuong`;
`co_quan_cap_tren` bỏ trống thì cấp trên là tỉnh, theo chính quyền hai cấp; `dac_diem` là mảng chuỗi tối đa 10 phần tử,
mỗi phần tử thành một dòng «- …» trong khối BỐI CẢNH ĐỊA PHƯƠNG). Không gửi thì prompt không có bối cảnh địa phương
(spec §10). Bộ kiểm so khớp dùng giá trị Gia Kiệm (`xa` = «xã Gia Kiệm», `co_quan_cap_tren` = «thành phố Thống Nhất»,
`tinh` = «tỉnh Đồng Nai», `dac_diem` = [«Dân số: 79.274 nhân khẩu, 19.818 hộ», «17 ấp, 5 giáo xứ Công giáo (96% dân theo đạo)»,
«Xã loại I, đang xây dựng nông thôn mới nâng cao»]) để so được từng ký tự với prompt cũ.
`ten_mau` thiếu thì nhánh soạn chung ghi «Công văn» làm tên loại (như hộp thoại khi chưa chọn mẫu).
```

Thay đoạn **`trich-xuat`** bằng:

```markdown
**`trich-xuat`**: tổng dung lượng tệp (sau giải base64) vượt `min(max_file_size_mb` của gói hiện hành, 20 MB) → `QUA_DAI` 413.
Quá 20 tệp, mime khác danh sách, base64 hỏng, `chi-chu` khác 1 tệp → `DAU_VAO_SAI` 400. Ảnh GIF/BMP/TIFF: bản cài máy đổi
sang PNG/JPEG trước khi gửi. Ước tính theo số tệp, số trang PDF và dung lượng (spec §7).
```

- [ ] **Step 3: Commit (repo gốc, chỉ tệp hợp đồng)**

```bash
cd /d/AIVanBanCaNhan && git add docs/superpowers/plans/2026-11-16-hop-dong-tac-vu-v2.md && git commit -F - <<'EOF'
docs: hop dong tac vu v2 bo sung ten_mau, dia_phuong.dac_diem, quy tac tep trich-xuat

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
```

---

### Task 1: Nền ước tính theo tệp

**Files:**
- Create: `vanbanplus-api/src/lib/tac-vu/tep.ts`
- Modify: `vanbanplus-api/src/lib/credit/uoc-tinh.ts` (interface `CauHinhTacVu`, hàm `uocTinh`)
- Modify: `vanbanplus-api/src/lib/credit/bang-gia.ts` (vòng `d.loai === "tac_vu"`)
- Modify: `vanbanplus-api/src/lib/extract-input.ts` (`function decodedBytes` → `export function decodedBytes`)
- Test: `vanbanplus-api/tests/tep.test.ts`

**Interfaces:**
- Produces:
  - `interface TepGui { base64: string; mime: string }` (`tep.ts`)
  - `soByteGiai(base64: string): number` (byte sau giải mã)
  - `demTrang(t: TepGui): number`, `BYTE_MOI_TRANG_PDF_UOC = 153600`
  - `CauHinhTacVu.token_moi_trang?: number`
  - `uocTinh(soKyTuVao, ch, gia, chung, soTrang = 0): UocTinh`. Thiếu `soTrang` hoặc thiếu `token_moi_trang` thì kết quả y như cũ.
  - `decodedBytes(base64: string): number` được export từ `extract-input.ts`.

- [ ] **Step 1: Viết bộ kiểm hỏng**

`vanbanplus-api/tests/tep.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { demTrang, soByteGiai, BYTE_MOI_TRANG_PDF_UOC } from "../src/lib/tac-vu/tep";
import { uocTinh, type CauHinhTacVu } from "../src/lib/credit/uoc-tinh";
import { dungBangGia, type DongBangGia } from "../src/lib/credit/bang-gia";

const b64 = (s: string, enc: BufferEncoding = "latin1") => Buffer.from(s, enc).toString("base64");

test("số byte sau giải base64", () => {
  assert.equal(soByteGiai("AAAA"), 3);
  assert.equal(soByteGiai("AAA="), 2);
  assert.equal(soByteGiai("AA=="), 1);
  assert.equal(soByteGiai(b64("x".repeat(1000))), 1000);
});

test("đếm trang: ảnh là 1 trang; PDF đếm /Type /Page, không tính /Pages", () => {
  assert.equal(demTrang({ base64: "AAAA", mime: "image/png" }), 1);
  const pdf =
    "%PDF-1.4\n1 0 obj << /Type /Pages /Kids [2 0 R 3 0 R 4 0 R] /Count 3 >> endobj\n" +
    "2 0 obj << /Type /Page /Parent 1 0 R >> endobj\n3 0 obj <</Type/Page/Parent 1 0 R>> endobj\n" +
    "4 0 obj << /Type  /Page >> endobj\n%%EOF";
  assert.equal(demTrang({ base64: b64(pdf), mime: "application/pdf" }), 3);
});

test("PDF không đếm được trang: ước theo dung lượng, tối thiểu 1", () => {
  assert.equal(demTrang({ base64: b64("%PDF-1.5 nen"), mime: "application/pdf" }), 1);
  const lon = "%PDF-1.5\n" + "x".repeat(BYTE_MOI_TRANG_PDF_UOC * 2 + 10);
  assert.equal(demTrang({ base64: b64(lon), mime: "application/pdf" }), 3);
});

const CH: CauHinhTacVu = { toi_thieu: 1, ra_min_he_so: 0, ra_min_hang: 0, ra_max_he_so: 1, ra_max_hang: 0, toi_da_ky_tu: 100, token_moi_trang: 1000 };
const GIA = { vao_usd_trieu: 1.5, ra_usd_trieu: 7.5 };
const CHUNG = { gia_tri_credit_vnd: 100, ty_gia_vnd_usd: 26000, mo_hinh_mac_dinh: "m" };

test("uocTinh: cộng số trang × token_moi_trang; không có trang hoặc không khai thì như cũ", () => {
  assert.equal(uocTinh(300, CH, GIA, CHUNG, 2).token_vao, 2100);
  assert.equal(uocTinh(300, CH, GIA, CHUNG, 2).ra_max, 2100);
  assert.deepEqual(uocTinh(300, CH, GIA, CHUNG), uocTinh(300, CH, GIA, CHUNG, 0));
  assert.equal(uocTinh(300, CH, GIA, CHUNG).token_vao, 100);
  const { token_moi_trang: _bo, ...khongTrang } = CH;
  assert.equal(uocTinh(300, khongTrang, GIA, CHUNG, 5).token_vao, 100);
});

const DONG: DongBangGia[] = [
  { loai: "cau_hinh", ma: "chung", gia_tri: CHUNG },
  { loai: "mo_hinh", ma: "m", gia_tri: GIA },
  { loai: "tac_vu", ma: "tom-tat", gia_tri: { toi_thieu: 2, ra_min_he_so: 0.2, ra_min_hang: 1500, ra_max_he_so: 0.6, ra_max_hang: 7000, toi_da_ky_tu: 120000 } },
  { loai: "tac_vu", ma: "trich-xuat", gia_tri: { toi_thieu: 3, ra_min_he_so: 0.3, ra_min_hang: 300, ra_max_he_so: 1.5, ra_max_hang: 2000, toi_da_ky_tu: 10000, token_moi_trang: 1100 } },
];

test("bảng giá: token_moi_trang là tùy chọn; có thì phải là số không âm", () => {
  const bg = dungBangGia(DONG);
  assert.equal(bg.tac_vu["trich-xuat"].token_moi_trang, 1100);
  assert.equal("token_moi_trang" in bg.tac_vu["tom-tat"], false);
  const hong = DONG.map((d) => (d.ma === "trich-xuat" ? { ...d, gia_tri: { ...(d.gia_tri as object), token_moi_trang: -1 } } : d));
  assert.throws(() => dungBangGia(hong), /trich-xuat\.token_moi_trang/);
});
```

Run: `cd /d/AIVanBanCaNhan/vanbanplus-api && npx tsx --test tests/tep.test.ts`
Expected: FAIL (`Cannot find module '../src/lib/tac-vu/tep'`).

- [ ] **Step 2: Viết mã**

`src/lib/extract-input.ts`: đổi đúng một dòng `function decodedBytes(base64: string): number {` thành `export function decodedBytes(base64: string): number {`.

`src/lib/tac-vu/tep.ts`:

```ts
import { decodedBytes } from "../extract-input";

/** Một tệp gửi kèm prompt (base64 + mime), dùng cho trich-xuat. */
export interface TepGui {
  base64: string;
  mime: string;
}

/** Số byte sau giải base64 (không giải thật). */
export const soByteGiai = (base64: string): number => decodedBytes(base64);

/** Không đếm được trang PDF thì ước 1 trang mỗi 150 KB (bản scan thường 100–300 KB/trang). */
export const BYTE_MOI_TRANG_PDF_UOC = 150 * 1024;

/** «/Type /Page» (không phải «/Pages») trong đối tượng PDF không nén. */
const TRANG_PDF = /\/Type\s*\/Page(?![A-Za-z])/g;

/**
 * Số trang để ước tính token: ảnh là 1 trang. PDF đếm đối tượng trang; PDF dùng luồng đối tượng nén
 * thì không đếm được, khi đó ước theo dung lượng. Chỉ dùng cho ước tính, không dùng để kiểm đầu vào.
 */
export function demTrang(t: TepGui): number {
  if (t.mime !== "application/pdf") return 1;
  const dem = Buffer.from(t.base64, "base64").toString("latin1").match(TRANG_PDF)?.length ?? 0;
  return dem > 0 ? dem : Math.max(1, Math.ceil(soByteGiai(t.base64) / BYTE_MOI_TRANG_PDF_UOC));
}
```

`src/lib/credit/uoc-tinh.ts`: thêm trường cuối `CauHinhTacVu` và tham số cuối `uocTinh`:

```ts
export interface CauHinhTacVu {
  toi_thieu: number;
  ra_min_he_so: number;
  ra_min_hang: number;
  ra_max_he_so: number;
  ra_max_hang: number;
  /** Số ký tự tối đa của phần chữ (nguoi_dung). Tệp gửi kèm giới hạn theo byte, không theo trường này. */
  toi_da_ky_tu: number;
  /** Token vào cho mỗi trang ảnh/PDF gửi kèm (tác vụ có tệp). Không khai thì tệp không cộng token. */
  token_moi_trang?: number;
}
```

```ts
/** Khoảng credit báo trước. ra_max đồng thời là trần token ra khi gọi AI. soTrang: tổng trang ảnh/PDF gửi kèm. */
export function uocTinh(soKyTuVao: number, ch: CauHinhTacVu, gia: GiaMoHinh, chung: CauHinhChung, soTrang = 0): UocTinh {
  const token_vao = lenTron(soKyTuVao / KY_TU_MOI_TOKEN) + lenTron(soTrang * (ch.token_moi_trang ?? 0));
  const ra_min = lenTron(ch.ra_min_he_so * token_vao + ch.ra_min_hang);
  const ra_max = lenTron(ch.ra_max_he_so * token_vao + ch.ra_max_hang);
  const credit_min = Math.max(ch.toi_thieu, creditTuToken(token_vao, ra_min, gia, chung));
  const credit_max = Math.max(credit_min, creditTuToken(token_vao, ra_max, gia, chung));
  return { token_vao, ra_min, ra_max, credit_min, credit_max };
}
```

`src/lib/credit/bang-gia.ts`: trong nhánh `else if (d.loai === "tac_vu") {`, ngay sau dòng `for (const k of TRUONG_TAC_VU) t[k] = soKhongAm(g[k], \`${d.ma}.${k}\`);` thêm:

```ts
      if (g.token_moi_trang !== undefined) t.token_moi_trang = soKhongAm(g.token_moi_trang, `${d.ma}.token_moi_trang`);
```

- [ ] **Step 3: Chạy, thấy qua; bộ kiểm cũ vẫn qua**

Run: `npx tsx --test tests/tep.test.ts tests/uoc-tinh.test.ts tests/bang-gia.test.ts tests/extract-input.test.ts && npx tsc --noEmit -p .`
Expected: mọi bài pass, `tsc` không lỗi.

- [ ] **Step 4: Commit**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git add src/lib/tac-vu/tep.ts src/lib/credit/uoc-tinh.ts src/lib/credit/bang-gia.ts src/lib/extract-input.ts tests/tep.test.ts && git commit -F - <<'EOF'
feat(credit): uoc tinh theo so trang tep (token_moi_trang tuy chon), dem trang PDF

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
```

---

### Task 2: Luồng chung nhận tệp, lược đồ, tắt suy nghĩ, trần dung lượng theo gói

**Files:**
- Modify: `vanbanplus-api/src/lib/tac-vu/chay-tac-vu.ts` (thay toàn tệp, nội dung dưới; chỉ thêm, mọi thứ cũ giữ nguyên)
- Create: `vanbanplus-api/src/lib/tac-vu/gioi-han-tep.ts`
- Modify: `vanbanplus-api/src/lib/usage.ts` (`function effectivePlanId` → `export function effectivePlanId`)
- Modify: `vanbanplus-api/src/lib/tac-vu/phu-thuoc.ts`
- Test: `vanbanplus-api/tests/chay-tac-vu-tep.test.ts`

**Interfaces:**
- Consumes: `TepGui`, `soByteGiai`, `demTrang` (Task 1), `uocTinh(..., soTrang)` (Task 1).
- Produces:
  - `PromptTacVu` thêm `tep?: TepGui[]`, `kieu_ket_qua?: "json" | "van_ban"`, `luoc_do?: Record<string, unknown>`, `tat_suy_nghi?: boolean`. Re-export `type TepGui` từ `chay-tac-vu.ts`.
  - `DinhNghiaTacVu` thêm `toi_da_byte_tep?: number`.
  - `PhuThuoc` thêm `toiDaTepByte?: (user: ApiUser) => Promise<number>`.
  - `gioi-han-tep.ts`: `MB_MAC_DINH = 5`, `byteTuMb(mb: unknown): number`, `docToiDaTepByte(user: ApiUser): Promise<number>`.
  - `usage.ts`: `export function effectivePlanId(user): string`.

- [ ] **Step 1: Viết bộ kiểm hỏng**

`vanbanplus-api/tests/chay-tac-vu-tep.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { chayTacVu, uocTinhTacVu, type DinhNghiaTacVu, type PhuThuoc, type PromptTacVu } from "../src/lib/tac-vu/chay-tac-vu";
import { uocTinh, type BangGia } from "../src/lib/credit/uoc-tinh";
import type { KetQuaMo, KhoCredit } from "../src/lib/credit/so-du";
import type { AICallOptions, AICallResult, ApiUser } from "../src/types";
import { byteTuMb } from "../src/lib/tac-vu/gioi-han-tep";
import { effectivePlanId } from "../src/lib/usage";

const BG: BangGia = {
  chung: { gia_tri_credit_vnd: 100, ty_gia_vnd_usd: 26000, mo_hinh_mac_dinh: "gemini-3.6-flash" },
  mo_hinh: { "gemini-3.6-flash": { vao_usd_trieu: 1.5, ra_usd_trieu: 7.5 } },
  tac_vu: {
    chu: { toi_thieu: 2, ra_min_he_so: 0, ra_min_hang: 1000, ra_max_he_so: 0, ra_max_hang: 5000, toi_da_ky_tu: 100 },
    tep: { toi_thieu: 1, ra_min_he_so: 0, ra_min_hang: 0, ra_max_he_so: 1, ra_max_hang: 0, toi_da_ky_tu: 100, token_moi_trang: 1000 },
  },
};
const USER = { id: "U1", api_key: "vbp_x" } as ApiUser;
const CTX = { user: USER, nguonGoi: "kiem-thu", phienBan: null };
const AI_OK = (content: string): AICallResult => ({
  success: true, content, promptTokens: 10, completionTokens: 100, thoughtsTokens: 0, finishReason: "STOP",
});

/** Định nghĩa thử: body chính là PromptTacVu, để mỗi bài tự chọn phần tùy chọn. */
function dn(ten: "chu" | "tep", them: Partial<DinhNghiaTacVu<PromptTacVu>> = {}): DinhNghiaTacVu<PromptTacVu> {
  return {
    ten,
    kieu_ket_qua: "json",
    nhiet_do: 0.2,
    kiemDauVao: (b) => ({ ok: true, du_lieu: b as PromptTacVu }),
    dungPrompt: (p) => p,
    ...them,
  };
}

function dungPhu(tra: AICallResult[], toiDaTepByte?: PhuThuoc["toiDaTepByte"]) {
  const goi: { contents: unknown[]; opts: AICallOptions }[] = [];
  const trinhTu: string[] = [];
  const moTra: KetQuaMo = { ma: "ok", luot_id: "L1", so_du: 500 };
  const kho: KhoCredit = {
    soDu: async () => 500,
    mo: async () => { trinhTu.push("mo"); return moTra; },
    chot: async (_id, c) => { trinhTu.push("chot"); return { credit_tru: c.credit_that, so_du: 490, trang_thai: "xong" }; },
    hoan: async () => { trinhTu.push("hoan"); return { so_du: 500 }; },
  };
  const phu: PhuThuoc = {
    bangGia: async () => BG,
    kho,
    layKhoa: async () => ({ keyId: "k", provider: "gemini", apiKey: "x", model: "gemini-3.5-flash", source: "system" }),
    goiAI: async (contents, opts) => {
      trinhTu.push("goiAI");
      goi.push({ contents, opts });
      const t = tra.shift();
      if (!t) throw new Error("hết phản hồi giả");
      return t;
    },
    bayGio: () => new Date("2026-11-16T00:00:00Z"),
    ...(toiDaTepByte ? { toiDaTepByte } : {}),
  };
  return { phu, goi, trinhTu };
}

const ANH = { base64: "AAAA", mime: "image/png" };
const PDF = { base64: "JVBERi0=", mime: "application/pdf" }; // "%PDF-", không đếm được trang → 1

test("tác vụ chỉ có chữ: contents và tùy chọn gọi AI giữ nguyên như trước", async () => {
  const { phu, goi } = dungPhu([AI_OK('{"a":1}')], async () => { throw new Error("không được gọi"); });
  const kq = await chayTacVu(dn("chu"), { he_thong: "HT", nguoi_dung: "abc" }, CTX, phu);
  assert.equal(kq.ok, true);
  assert.deepEqual(goi[0].contents, [{ parts: [{ text: "abc" }] }]);
  assert.deepEqual(Object.keys(goi[0].opts).sort(), [
    "clientIp", "endpoint", "maxTokens", "requestType", "resolvedKey", "systemInstruction", "temperature", "user",
  ]);
  assert.equal(goi[0].opts.maxTokens, 5000);
});

test("có tệp: phần chữ trước, tệp theo đúng thứ tự; truyền lược đồ và tắt suy nghĩ; trần token ra tính cả trang", async () => {
  const { phu, goi, trinhTu } = dungPhu([AI_OK('{"noi_dung":"x"}')], async () => 10_000_000);
  const luoc_do = { type: "object" };
  const kq = await chayTacVu(dn("tep"), { he_thong: "", nguoi_dung: "đọc", tep: [ANH, PDF], luoc_do, tat_suy_nghi: true }, CTX, phu);
  assert.deepEqual(kq.ok && kq.ket_qua, { noi_dung: "x" });
  assert.deepEqual(trinhTu, ["mo", "goiAI", "chot"]);
  assert.deepEqual(goi[0].contents, [{
    parts: [
      { text: "đọc" },
      { inline_data: { mime_type: "image/png", data: "AAAA" } },
      { inline_data: { mime_type: "application/pdf", data: "JVBERi0=" } },
    ],
  }]);
  assert.deepEqual(goi[0].opts.responseSchema, luoc_do);
  assert.equal(goi[0].opts.disableThinking, true);
  assert.equal(goi[0].opts.systemInstruction, "");
  // ceil((0 + 3 + 17 ký tự lược đồ) / 3) = 7, cộng 2 trang × 1000
  assert.equal(goi[0].opts.maxTokens, 2007);
});

test("kieu_ket_qua của prompt ghi đè định nghĩa: van_ban trả nguyên văn, không thử lại", async () => {
  const { phu, trinhTu } = dungPhu([AI_OK("chữ thường, không phải json")]);
  const kq = await chayTacVu(dn("tep"), { he_thong: "", nguoi_dung: "đọc", tep: [ANH], kieu_ket_qua: "van_ban" }, CTX, phu);
  assert.equal(kq.ok && kq.ket_qua, "chữ thường, không phải json");
  assert.deepEqual(trinhTu, ["mo", "goiAI", "chot"]);
});

test("tổng dung lượng vượt trần gói: QUA_DAI 413, không mở lượt, không gọi AI; ước tính cũng 413; đúng bằng trần thì qua", async () => {
  const tep = [{ base64: "AAAAAAAA", mime: "image/png" }]; // 6 byte
  const a = dungPhu([], async () => 5);
  const kq = await chayTacVu(dn("tep"), { he_thong: "", nguoi_dung: "x", tep }, CTX, a.phu);
  assert.deepEqual([!kq.ok && kq.code, !kq.ok && kq.status], ["QUA_DAI", 413]);
  assert.match(!kq.ok ? kq.message : "", /MB/);
  assert.deepEqual(a.trinhTu, []);
  const u = await uocTinhTacVu(dn("tep"), { he_thong: "", nguoi_dung: "x", tep }, USER, a.phu);
  assert.deepEqual([!u.ok && u.code, !u.ok && u.status], ["QUA_DAI", 413]);
  const b = dungPhu([AI_OK('{"a":1}')], async () => 6);
  assert.equal((await chayTacVu(dn("tep"), { he_thong: "", nguoi_dung: "x", tep }, CTX, b.phu)).ok, true);
});

test("trần của tác vụ áp cả khi gói cho nhiều hơn, và khi không có phụ thuộc trần gói", async () => {
  const tep = [{ base64: "AAAAAAAA", mime: "image/png" }];
  const voiGoi = dungPhu([], async () => 1_000_000);
  const a = await chayTacVu(dn("tep", { toi_da_byte_tep: 3 }), { he_thong: "", nguoi_dung: "x", tep }, CTX, voiGoi.phu);
  assert.equal(!a.ok && a.code, "QUA_DAI");
  const khongGoi = dungPhu([]);
  const b = await chayTacVu(dn("tep", { toi_da_byte_tep: 3 }), { he_thong: "", nguoi_dung: "x", tep }, CTX, khongGoi.phu);
  assert.equal(!b.ok && b.code, "QUA_DAI");
});

test("ước tính có tệp: cộng số trang × token_moi_trang, lược đồ tính vào số ký tự", async () => {
  const { phu } = dungPhu([], async () => 1_000_000);
  const luoc_do = { type: "object" };
  const kq = await uocTinhTacVu(dn("tep"), { he_thong: "", nguoi_dung: "abc", tep: [ANH, ANH, PDF], luoc_do }, USER, phu);
  const mong = uocTinh(3 + JSON.stringify(luoc_do).length, BG.tac_vu.tep, BG.mo_hinh["gemini-3.6-flash"], BG.chung, 3);
  assert.deepEqual(kq, { ok: true, credit_min: mong.credit_min, credit_max: mong.credit_max, so_du: 500, du_credit: 500 >= mong.credit_max });
  const khongTep = await uocTinhTacVu(dn("tep"), { he_thong: "", nguoi_dung: "abc" }, USER, phu);
  assert.ok(khongTep.ok && kq.ok && khongTep.credit_max < kq.credit_max);
});

test("trần byte theo gói: MB → byte, thiếu / sai thì 5 MB", () => {
  assert.equal(byteTuMb(5), 5 * 1024 * 1024);
  assert.equal(byteTuMb(20), 20 * 1024 * 1024);
  for (const sai of [null, undefined, 0, -1, "10", Number.NaN]) assert.equal(byteTuMb(sai), 5 * 1024 * 1024);
});

test("gói hiện hành: hết hạn thì free, còn hạn hoặc không có hạn thì giữ gói", () => {
  const qua = new Date(Date.now() - 86_400_000).toISOString();
  const toi = new Date(Date.now() + 86_400_000).toISOString();
  assert.equal(effectivePlanId({ subscription_plan_id: "pro", subscription_end_date: qua }), "free");
  assert.equal(effectivePlanId({ subscription_plan_id: "pro", subscription_end_date: toi }), "pro");
  assert.equal(effectivePlanId({ subscription_plan_id: "starter", subscription_end_date: null }), "starter");
});
```

Run: `npx tsx --test tests/chay-tac-vu-tep.test.ts`
Expected: FAIL (`gioi-han-tep` chưa có; `effectivePlanId` không được export).

- [ ] **Step 2: Viết mã**

`src/lib/usage.ts`: đổi dòng `function effectivePlanId(user: Pick<ApiUser, "subscription_plan_id" | "subscription_end_date">): string {` thành `export function effectivePlanId(user: Pick<ApiUser, "subscription_plan_id" | "subscription_end_date">): string {`. Thân hàm giữ nguyên.

`src/lib/tac-vu/gioi-han-tep.ts`:

```ts
import type { ApiUser } from "@/types";
import { getSupabase } from "../supabase";
import { effectivePlanId } from "../usage";

/** Mặc định của cột subscription_plans.max_file_size_mb (schema.sql). */
export const MB_MAC_DINH = 5;

/** max_file_size_mb → byte. Thiếu, không phải số dương thì dùng mặc định. */
export function byteTuMb(mb: unknown): number {
  const so = typeof mb === "number" && Number.isFinite(mb) && mb > 0 ? mb : MB_MAC_DINH;
  return Math.floor(so * 1024 * 1024);
}

/** Trần tổng dung lượng tệp mỗi lần gửi theo gói hiện hành (gói hết hạn tính như free). Lỗi CSDL thì ném ra. */
export async function docToiDaTepByte(user: ApiUser): Promise<number> {
  const goi = effectivePlanId(user) || "free";
  const { data, error } = await getSupabase()
    .from("subscription_plans")
    .select("max_file_size_mb")
    .eq("id", goi)
    .maybeSingle();
  if (error) throw new Error(`Không đọc được giới hạn tệp của gói ${goi}: ${error.message}`);
  return byteTuMb((data as { max_file_size_mb?: unknown } | null)?.max_file_size_mb);
}
```

`src/lib/tac-vu/phu-thuoc.ts`: thêm import và một dòng trong `phuThuocThat`:

```ts
import { callAI } from "../ai-providers";
import { resolveAIKey } from "../ai-keys";
import { docBangGia } from "../credit/bang-gia";
import { khoCreditSupabase } from "../credit/so-du";
import type { PhuThuoc } from "./chay-tac-vu";
import { docToiDaTepByte } from "./gioi-han-tep";

export const phuThuocThat: PhuThuoc = {
  bangGia: docBangGia,
  kho: khoCreditSupabase,
  layKhoa: (userId) => resolveAIKey(userId, "gemini", { chiHeThong: true }),
  goiAI: (contents, opts) => callAI(contents as any[], opts),
  bayGio: () => new Date(),
  toiDaTepByte: docToiDaTepByte,
};
```

`src/lib/tac-vu/chay-tac-vu.ts` (toàn tệp; so với bản hiện tại chỉ thêm các khối có chú thích «đợt 2b»):

```ts
import type { AICallOptions, AICallResult, ApiUser, ResolvedAIKey } from "@/types";
import { creditThat, giaCua, uocTinh, type BangGia, type CauHinhTacVu, type UocTinh } from "../credit/uoc-tinh";
import type { KhoCredit } from "../credit/so-du";
import { docJson } from "./doc-json";
import { demTrang, soByteGiai, type TepGui } from "./tep";

export type { TepGui } from "./tep";

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
  // ---- đợt 2b: tùy chọn; không có thì lời gọi AI y như tác vụ chỉ có chữ ----
  /** Tệp gửi sau phần chữ, theo đúng thứ tự. */
  tep?: TepGui[];
  /** Ghi đè kieu_ket_qua của định nghĩa cho riêng lượt này (vd trich-xuat chi-chu trả văn bản). */
  kieu_ket_qua?: "json" | "van_ban";
  /** responseSchema truyền cho callAI (Gemini trả JSON đúng lược đồ). Tính vào số ký tự khi ước tính. */
  luoc_do?: Record<string, unknown>;
  /** disableThinking của callAI. */
  tat_suy_nghi?: boolean;
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
  /** đợt 2b: trần tổng byte tệp (sau giải base64) của riêng tác vụ, áp cùng trần của gói. */
  toi_da_byte_tep?: number;
}

export interface PhuThuoc {
  bangGia(): Promise<BangGia>;
  kho: KhoCredit;
  layKhoa(userId: string): Promise<ResolvedAIKey | null>;
  goiAI(contents: unknown[], opts: AICallOptions): Promise<AICallResult>;
  bayGio(): Date;
  /** đợt 2b: trần tổng byte tệp theo gói hiện hành. Chỉ gọi khi prompt có tệp. Không có thì không áp trần gói. */
  toiDaTepByte?: (user: ApiUser) => Promise<number>;
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

/** Số token hợp lệ: hữu hạn, dương. Thiếu / NaN / Infinity / âm tính là 0 (NaN sẽ làm credit_chot nhận null). */
const so = (x?: number) => (Number.isFinite(x) && x! > 0 ? x! : 0);

const soMb = (b: number) => (b / (1024 * 1024)).toFixed(1).replace(".", ",");

type ChuanBi =
  | { ok: true; prompt: PromptTacVu; bg: BangGia; ch: CauHinhTacVu; model: string; uoc: UocTinh }
  | Loi;

async function chuanBi<V>(dn: DinhNghiaTacVu<V>, body: unknown, user: ApiUser, phu: PhuThuoc): Promise<ChuanBi> {
  const dv = dn.kiemDauVao(body);
  if (!dv.ok) return loi("DAU_VAO_SAI", 400, dv.loi);

  const bg = await phu.bangGia();
  const ch = bg.tac_vu[dn.ten];
  if (!ch) throw new Error(`Bảng giá thiếu tác vụ ${dn.ten}`);

  const prompt = dn.dungPrompt(dv.du_lieu, phu.bayGio());
  if (prompt.nguoi_dung.length > ch.toi_da_ky_tu) {
    return loi("QUA_DAI", 413, `Văn bản quá dài cho tác vụ này (tối đa ${ch.toi_da_ky_tu.toLocaleString("vi-VN")} ký tự).`);
  }

  // đợt 2b: trần dung lượng tệp (gói ∧ tác vụ) và số trang để ước tính
  let soTrang = 0;
  if (prompt.tep?.length) {
    const tong = prompt.tep.reduce((s, t) => s + soByteGiai(t.base64), 0);
    const tranGoi = phu.toiDaTepByte ? await phu.toiDaTepByte(user) : Infinity;
    const tran = Math.min(tranGoi, dn.toi_da_byte_tep ?? Infinity);
    if (tong > tran) {
      return loi(
        "QUA_DAI",
        413,
        `Tệp quá lớn (~${soMb(tong)} MB). Tối đa ${soMb(tran)} MB mỗi lần gửi; cần giảm dung lượng ảnh hoặc gửi ít trang hơn.`
      );
    }
    soTrang = prompt.tep.reduce((s, t) => s + demTrang(t), 0);
  }

  const model = bg.chung.mo_hinh_mac_dinh;
  const soKyTu =
    prompt.he_thong.length + prompt.nguoi_dung.length + (prompt.luoc_do ? JSON.stringify(prompt.luoc_do).length : 0);
  const uoc = uocTinh(soKyTu, ch, giaCua(bg, model), bg.chung, soTrang);
  return { ok: true, prompt, bg, ch, model, uoc };
}

export async function uocTinhTacVu<V>(
  dn: DinhNghiaTacVu<V>,
  body: unknown,
  user: ApiUser,
  phu: PhuThuoc
): Promise<KetQuaUocTinh> {
  const cb = await chuanBi(dn, body, user, phu);
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
  const cb = await chuanBi(dn, body, ctx.user, phu);
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

  const ngat = async (): Promise<Loi> => {
    await phu.kho.hoan(luot, "NGAT_KET_NOI");
    return loi("NGAT_KET_NOI", 499, "Kết nối đã ngắt trước khi nhận kết quả. Credit đã được hoàn.");
  };

  // đợt 2b: kiểu kết quả của lượt, phần tệp và tùy chọn AI (không có tùy chọn thì không thêm khóa nào)
  const kieu = cb.prompt.kieu_ket_qua ?? dn.kieu_ket_qua;
  const parts: unknown[] = [{ text: cb.prompt.nguoi_dung }];
  for (const t of cb.prompt.tep ?? []) parts.push({ inline_data: { mime_type: t.mime, data: t.base64 } });
  const opts: AICallOptions = {
    user: ctx.user,
    resolvedKey: khoaChay,
    endpoint: `/api/v2/tac-vu/${dn.ten}`,
    requestType: `v2:${dn.ten}`,
    clientIp: ctx.clientIp,
    systemInstruction: cb.prompt.he_thong,
    temperature: dn.nhiet_do,
    maxTokens: cb.uoc.ra_max,
  };
  if (cb.prompt.luoc_do) opts.responseSchema = cb.prompt.luoc_do;
  if (cb.prompt.tat_suy_nghi) opts.disableThinking = true;

  let tokenVao = 0;
  let tokenRa = 0;
  try {
    for (let lan = 1; ; lan++) {
      const r = await phu.goiAI([{ parts }], opts);

      if (!r.success) {
        await phu.kho.hoan(luot, "AI_LOI");
        return /\b429\b/.test(r.error ?? "")
          ? loi("AI_QUA_TAI", 503, "Máy chủ AI đang quá tải. Credit đã được hoàn, thử lại sau ít phút.", 30)
          : loi("AI_LOI", 502, "AI xử lý không thành công. Credit đã được hoàn.");
      }

      tokenVao += so(r.promptTokens);
      tokenRa += so(r.completionTokens) + so(r.thoughtsTokens);
      const biCat = LY_DO_BI_CAT.has(r.finishReason ?? "");
      const noiDung = r.content ?? "";

      let ketQua: unknown = noiDung;
      if (kieu === "json") {
        const obj = docJson(noiDung);
        const dung = obj !== undefined && (dn.kiemKetQua?.(obj) ?? true);
        if (dung) {
          ketQua = obj;
        } else if (!biCat) {
          if (lan < SO_LAN_GOI_TOI_DA) {
            if (ctx.signal?.aborted) return await ngat();
            continue;
          }
          await phu.kho.hoan(luot, "JSON_HONG");
          return loi("AI_LOI", 502, "AI trả kết quả sai định dạng. Credit đã được hoàn.");
        }
      }

      if (ctx.signal?.aborted) return await ngat();

      const credit = creditThat(tokenVao, tokenRa, cb.ch, giaCua(cb.bg, cb.model), cb.bg.chung, cb.uoc.credit_max);
      const chot = await phu.kho.chot(luot, { credit_that: credit, token_vao: tokenVao, token_ra: tokenRa, bi_cat: biCat });
      if (chot.trang_thai !== "xong") {
        // Thường do quét treo đã hoàn lượt này trong lúc AI chạy quá lâu.
        console.warn(`🟡 [TAC-VU ${dn.ten}] lượt ${luot} chốt xong nhưng trạng thái là «${chot.trang_thai}»`);
      }
      return { ok: true, ket_qua: ketQua, credit_da_tru: chot.credit_tru, so_du: chot.so_du, ma_luot: luot, bi_cat: biCat };
    }
  } catch (e) {
    console.error(`🔴 [TAC-VU ${dn.ten}] lượt ${luot} lỗi: ${(e as Error).message}`);
    await phu.kho.hoan(luot, "LOI_MAY_CHU").catch((e2: unknown) => {
      // Quét treo 15 phút là lưới đỡ cuối cùng.
      console.error(`🔴 [TAC-VU ${dn.ten}] lượt ${luot} hoàn credit thất bại: ${(e2 as Error).message}`);
    });
    return loi("AI_LOI", 502, "AI xử lý không thành công. Credit đã được hoàn.");
  }
}
```

Nếu `main` lúc làm đã khác bản đọc ngày 16/11/2026 (`git diff main~N -- src/lib/tac-vu/chay-tac-vu.ts` có thay đổi ngoài kế hoạch này), thì **không** chép đè cả tệp. Áp từng khối «đợt 2b» vào bản hiện tại: import `tep`, 4 trường `PromptTacVu`, `toi_da_byte_tep`, `toiDaTepByte`, `soMb`, tham số `user` của `chuanBi`, khối trần dung lượng, `soKyTu` + `soTrang`, khối `kieu`/`parts`/`opts`, `if (kieu === "json")`.

- [ ] **Step 3: Chạy, thấy qua; bộ kiểm luồng cũ và tom-tat vẫn qua không sửa**

Run: `npx tsx --test tests/chay-tac-vu-tep.test.ts tests/chay-tac-vu.test.ts tests/tac-vu-tom-tat.test.ts && npm test && npx tsc --noEmit -p .`
Expected: mọi bài pass (bài CSDL tự bỏ qua khi chưa đặt `TEST_DATABASE_URL`). `tests/chay-tac-vu.test.ts` và các bài của `tom-tat`/`dot2a` qua mà **không sửa dòng nào**: đó là bằng chứng tác vụ cũ không bị ảnh hưởng.

- [ ] **Step 4: Commit**

```bash
git add src/lib/tac-vu/chay-tac-vu.ts src/lib/tac-vu/gioi-han-tep.ts src/lib/tac-vu/phu-thuoc.ts src/lib/usage.ts tests/chay-tac-vu-tep.test.ts && git commit -F - <<'EOF'
feat(tac-vu): luong chung nhan tep, luoc do, tat suy nghi, tran dung luong theo goi (tuy chon)

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
```

---

### Task 3: Hằng prompt trích xuất thành module dùng chung + tệp mẫu khóa văn bản

**Files:**
- Create (một lần, không commit): `D:/AIVanBanCaNhan/.superpowers/tam/sinh-mau-trich-xuat.ts`
- Create (sinh ra): `vanbanplus-api/tests/mau-prompt/trich-xuat.json`
- Create: `vanbanplus-api/src/lib/tac-vu/trich-xuat/prompt.ts`
- Modify: `vanbanplus-api/src/app/api/ai/[action]/route.ts` (xóa mục `// ==================== PROMPTS ====================`, thêm import)
- Test: `vanbanplus-api/tests/tac-vu-trich-xuat.test.ts` (phần so khớp; Task 4 thêm phần còn lại)

**Interfaces:**
- Consumes: `PromptTacVu`, `TepGui` (Task 2), `multiPagePrompt` (`extract-input.ts`).
- Produces (`trich-xuat/prompt.ts`): `EXTRACT_PROMPT: string`, `EXTRACT_SCHEMA` (đối tượng như cũ), `READ_PROMPT: string`, `type CheDo = "cau-truc" | "chi-chu"`, `interface DauVaoTrichXuat { tep: TepGui[]; che_do: CheDo }`, `dungPromptTrichXuat(d: DauVaoTrichXuat): PromptTacVu`.
- Tệp mẫu: mảng `{ ten, dau_vao, he_thong, nguoi_dung, luoc_do (null khi chi-chu), kieu_ket_qua }`.

- [ ] **Step 1: Sinh tệp mẫu từ route CŨ (trước khi chuyển hằng)**

`D:/AIVanBanCaNhan/.superpowers/tam/sinh-mau-trich-xuat.ts`:

```ts
// Chạy MỘT lần, TRƯỚC khi chuyển hằng khỏi route: đọc văn bản route cũ, ghi tests/mau-prompt/trich-xuat.json.
import { readFileSync, writeFileSync } from "node:fs";
import { multiPagePrompt } from "../../vanbanplus-api/src/lib/extract-input";

const API = "D:/AIVanBanCaNhan/vanbanplus-api";
const src = readFileSync(`${API}/src/app/api/ai/[action]/route.ts`, "utf8").replace(/\r\n/g, "\n");

function chuoi(ten: string): string {
  const a = src.indexOf(`const ${ten} = \``);
  if (a < 0) throw new Error(`Không thấy ${ten} trong route (đã chuyển rồi?)`);
  const dau = src.indexOf("`", a) + 1;
  const cuoi = src.indexOf("`;", dau);
  const s = src.slice(dau, cuoi);
  if (s.includes("\\") || s.includes("${")) throw new Error(`${ten} có ký tự thoát, cần đọc kiểu khác`);
  return s;
}

const a = src.indexOf("const EXTRACT_SCHEMA = {");
if (a < 0) throw new Error("Không thấy EXTRACT_SCHEMA");
const b = src.indexOf("\n};", a);
const LUOC_DO = Function(`return (${src.slice(src.indexOf("{", a), b + 2).replace(/ as const/g, "")});`)();
const EXTRACT = chuoi("EXTRACT_PROMPT");
const READ = chuoi("READ_PROMPT");

const anh = { base64: "AAAA", mime: "image/png" };
const pdf = { base64: "JVBERi0=", mime: "application/pdf" };
const mau = [
  { ten: "cau-truc-mac-dinh-1-tep", dau_vao: { tep: [anh] }, he_thong: "", nguoi_dung: EXTRACT, luoc_do: LUOC_DO, kieu_ket_qua: "json" },
  {
    ten: "cau-truc-3-tep",
    dau_vao: { tep: [anh, { base64: "AAAA", mime: "image/jpeg" }, pdf], che_do: "cau-truc" },
    he_thong: "", nguoi_dung: multiPagePrompt(EXTRACT, 3), luoc_do: LUOC_DO, kieu_ket_qua: "json",
  },
  { ten: "chi-chu", dau_vao: { tep: [pdf], che_do: "chi-chu" }, he_thong: "", nguoi_dung: READ, luoc_do: null, kieu_ket_qua: "van_ban" },
];
writeFileSync(`${API}/tests/mau-prompt/trich-xuat.json`, JSON.stringify(mau, null, 2) + "\n");
console.log("Đã ghi", mau.length, "mẫu; EXTRACT", EXTRACT.length, "ký tự, READ", READ.length, "ký tự");
```

Run: `cd /d/AIVanBanCaNhan/vanbanplus-api && npx tsx ../.superpowers/tam/sinh-mau-trich-xuat.ts`
Expected: `Đã ghi 3 mẫu; EXTRACT 838 ký tự, READ 165 ký tự`.

- [ ] **Step 2: Viết bộ kiểm hỏng**

`vanbanplus-api/tests/tac-vu-trich-xuat.test.ts` (Task 4 thêm bài vào cuối tệp này):

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { dungPromptTrichXuat, type DauVaoTrichXuat } from "../src/lib/tac-vu/trich-xuat/prompt";

interface MauTX {
  ten: string;
  dau_vao: { tep: DauVaoTrichXuat["tep"]; che_do?: string };
  he_thong: string;
  nguoi_dung: string;
  luoc_do: Record<string, unknown> | null;
  kieu_ket_qua: "json" | "van_ban";
}
const MAU_TX: MauTX[] = JSON.parse(readFileSync(join(__dirname, "mau-prompt", "trich-xuat.json"), "utf8"));

test("prompt trích xuất khớp từng ký tự với route /api/ai/* trước khi chuyển (nguồn chuẩn: máy chủ)", () => {
  assert.equal(MAU_TX.length, 3);
  for (const m of MAU_TX) {
    const p = dungPromptTrichXuat({ tep: m.dau_vao.tep, che_do: (m.dau_vao.che_do ?? "cau-truc") as DauVaoTrichXuat["che_do"] });
    assert.equal(p.he_thong, m.he_thong, m.ten);
    assert.equal(p.nguoi_dung, m.nguoi_dung, m.ten);
    assert.deepEqual(p.luoc_do ?? null, m.luoc_do, m.ten);
    assert.equal(p.kieu_ket_qua, m.kieu_ket_qua, m.ten);
    assert.deepEqual(p.tep, m.dau_vao.tep, m.ten);
    assert.equal(p.tat_suy_nghi, true, m.ten);
  }
});

test("route cũ dùng chung hằng prompt, không còn bản sao riêng", () => {
  const route = readFileSync(join(__dirname, "..", "src", "app", "api", "ai", "[action]", "route.ts"), "utf8");
  assert.match(route, /from "@lib\/tac-vu\/trich-xuat\/prompt"/);
  assert.doesNotMatch(route, /const (EXTRACT_PROMPT|EXTRACT_SCHEMA|READ_PROMPT)\b/);
});
```

Run: `npx tsx --test tests/tac-vu-trich-xuat.test.ts`
Expected: FAIL (không có module `trich-xuat/prompt`).

- [ ] **Step 3: Chuyển hằng (cắt nguyên văn, không gõ lại)**

Tạo `src/lib/tac-vu/trich-xuat/prompt.ts`. Ba khối `const EXTRACT_PROMPT = …;`, `const EXTRACT_SCHEMA = { … };`, `const READ_PROMPT = …;` được **cắt** từ route (dòng 16–57 tại ngày 16/11/2026: từ `const EXTRACT_PROMPT` tới hết `READ_PROMPT`) và dán vào chỗ đánh dấu, mỗi khối thêm `export ` ở đầu:

```ts
import { multiPagePrompt } from "../../extract-input";
import type { PromptTacVu, TepGui } from "../chay-tac-vu";

/*
 * Nguồn chuẩn của prompt trích xuất là MÁY CHỦ (route /api/ai/extract + /api/ai/read-text cũ), không phải bản C#.
 * GeminiAIService (chế độ gọi thẳng Gemini, bỏ ở đợt 3) có prompt khác: noi_dung «chỉ phần nội dung chính» thay vì
 * toàn văn, liệt kê 33 loại VB trong prompt, required 14 trường, prompt nhiều trang riêng, prompt đọc chữ 3 dòng.
 * Chi tiết: docs/superpowers/plans/2026-11-16-dot2b-trich-xuat-soan-thao.md, «Quyết định thiết kế» mục 1.
 * Route cũ /api/ai/* import chính các hằng này: sửa ở đây là sửa cả hai đường.
 */

// <<< DÁN 3 KHỐI CẮT TỪ ROUTE Ở ĐÂY, mỗi khối thêm «export » đầu dòng: export const EXTRACT_PROMPT, export const EXTRACT_SCHEMA, export const READ_PROMPT >>>

export type CheDo = "cau-truc" | "chi-chu";

export interface DauVaoTrichXuat {
  tep: TepGui[];
  che_do: CheDo;
}

/**
 * cau-truc: như /api/ai/extract (một tệp giữ nguyên prompt, nhiều tệp thêm đoạn GHÉP TRANG; lược đồ + tắt suy nghĩ).
 * chi-chu: như /api/ai/read-text, trả chuỗi; tắt suy nghĩ (lệch chủ ý so với route cũ, xem kế hoạch).
 * Không có system instruction: route cũ không gửi, callAI bỏ qua chuỗi rỗng.
 */
export function dungPromptTrichXuat(d: DauVaoTrichXuat): PromptTacVu {
  if (d.che_do === "chi-chu") {
    return { he_thong: "", nguoi_dung: READ_PROMPT, tep: d.tep, kieu_ket_qua: "van_ban", tat_suy_nghi: true };
  }
  return {
    he_thong: "",
    nguoi_dung: d.tep.length === 1 ? EXTRACT_PROMPT : multiPagePrompt(EXTRACT_PROMPT, d.tep.length),
    tep: d.tep,
    kieu_ket_qua: "json",
    luoc_do: EXTRACT_SCHEMA,
    tat_suy_nghi: true,
  };
}
```

(Xóa dòng chú thích `// <<< DÁN … >>>` sau khi dán.)

Trong `src/app/api/ai/[action]/route.ts`: xóa cả mục từ dòng `// ==================== PROMPTS ====================` tới ngay trước `// ==================== HANDLERS ====================`, rồi thêm một dòng import sau dòng `import { readExtractInput, multiPagePrompt } from "@lib/extract-input";`:

```ts
import { EXTRACT_PROMPT, EXTRACT_SCHEMA, READ_PROMPT } from "@lib/tac-vu/trich-xuat/prompt";
```

Kiểm đã cắt nguyên văn (so khối cũ trong git với khối mới):

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && node -e '
const { execSync } = require("child_process"); const fs = require("fs");
const cu = execSync("git show HEAD:\"src/app/api/ai/[action]/route.ts\"").toString().replace(/\r\n/g, "\n");
const moi = fs.readFileSync("src/lib/tac-vu/trich-xuat/prompt.ts", "utf8").replace(/\r\n/g, "\n");
const khoiCu = cu.slice(cu.indexOf("const EXTRACT_PROMPT"), cu.indexOf("`;", cu.indexOf("const READ_PROMPT")) + 2);
const khoiMoi = moi.slice(moi.indexOf("export const EXTRACT_PROMPT"), moi.indexOf("`;", moi.indexOf("export const READ_PROMPT")) + 2).replace(/^export /gm, "");
console.log(khoiCu === khoiMoi ? "KHỚP nguyên văn" : "LỆCH"); process.exit(khoiCu === khoiMoi ? 0 : 1);'
```
Expected: `KHỚP nguyên văn`.

- [ ] **Step 4: Chạy, thấy qua**

Run: `npx tsx --test tests/tac-vu-trich-xuat.test.ts tests/extract-input.test.ts && npx tsc --noEmit -p .`
Expected: pass; `tsc` không lỗi (route vẫn dùng ba hằng qua import).

- [ ] **Step 5: Commit**

```bash
git add src/lib/tac-vu/trich-xuat/prompt.ts "src/app/api/ai/[action]/route.ts" tests/tac-vu-trich-xuat.test.ts tests/mau-prompt/trich-xuat.json && git commit -F - <<'EOF'
refactor(ai): hang prompt trich xuat chuyen sang module dung chung; tep mau khoa van ban

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
```

---

### Task 4: Tác vụ `trich-xuat` + bảng giá `008`

**Files:**
- Create: `vanbanplus-api/src/lib/tac-vu/trich-xuat/dinh-nghia.ts`
- Modify: `vanbanplus-api/src/lib/tac-vu/dang-ky.ts` (thêm import + một mục)
- Create: `vanbanplus-api/supabase/migrations/008_bang_gia_dot2b.sql`
- Test: `vanbanplus-api/tests/tac-vu-trich-xuat.test.ts` (thêm bài), `vanbanplus-api/tests/bang-gia-dot2b-csdl.test.ts`

**Interfaces:**
- Consumes: `dungPromptTrichXuat`, `DauVaoTrichXuat` (Task 3), `MAX_EXTRACT_BYTES` (`extract-input.ts`), `uocTinhTacVu` (Task 2).
- Produces: `trichXuat: DinhNghiaTacVu<DauVaoTrichXuat>` (`ten: "trich-xuat"`, `toi_da_byte_tep: MAX_EXTRACT_BYTES`), `SO_TEP_TOI_DA = 20`, `MIME_NHAN`. Dòng bảng giá `tac_vu/trich-xuat` và `tac_vu/soan-thao`.

- [ ] **Step 1: Viết bộ kiểm hỏng**

Thêm vào **cuối** `tests/tac-vu-trich-xuat.test.ts` (giữ nguyên hai bài của Task 3; gộp các dòng import mới lên đầu tệp):

```ts
import { trichXuat, SO_TEP_TOI_DA } from "../src/lib/tac-vu/trich-xuat/dinh-nghia";
import { layTacVu } from "../src/lib/tac-vu/dang-ky";
import { uocTinhTacVu, type PhuThuoc } from "../src/lib/tac-vu/chay-tac-vu";
import type { BangGia, CauHinhTacVu } from "../src/lib/credit/uoc-tinh";
import type { KhoCredit } from "../src/lib/credit/so-du";
import type { ApiUser } from "../src/types";
import { MAX_EXTRACT_BYTES } from "../src/lib/extract-input";

const ANH = { base64: "AAAA", mime: "image/png" };

test("đầu vào trich-xuat: tệp 1..20, base64 hợp lệ, mime trong danh sách, chế độ đúng", () => {
  const sai = (b: unknown) => assert.equal(trichXuat.kiemDauVao(b).ok, false, JSON.stringify(b)?.slice(0, 80));
  sai({});
  sai(null);
  sai({ tep: [] });
  sai({ tep: "AAAA" });
  sai({ tep: Array.from({ length: SO_TEP_TOI_DA + 1 }, () => ANH) });
  sai({ tep: [{ base64: "AAAA", mime: "image/gif" }] });
  sai({ tep: [{ base64: "AAA", mime: "image/png" }] });
  sai({ tep: [{ base64: "AA!A", mime: "image/png" }] });
  sai({ tep: [{ base64: "", mime: "image/png" }] });
  sai({ tep: [null] });
  sai({ tep: [ANH], che_do: "ocr" });
  sai({ tep: [ANH, ANH], che_do: "chi-chu" });
  assert.deepEqual(trichXuat.kiemDauVao({ tep: [ANH] }), { ok: true, du_lieu: { tep: [ANH], che_do: "cau-truc" } });
  assert.deepEqual(trichXuat.kiemDauVao({ tep: [ANH], che_do: "" }), { ok: true, du_lieu: { tep: [ANH], che_do: "cau-truc" } });
  const pdf = { base64: "JVBERi0=", mime: "application/pdf", thua: 1 };
  assert.deepEqual(trichXuat.kiemDauVao({ tep: [pdf], che_do: "chi-chu" }), {
    ok: true, du_lieu: { tep: [{ base64: "JVBERi0=", mime: "application/pdf" }], che_do: "chi-chu" },
  });
});

test("kết quả cấu trúc dùng được khi có noi_dung là chuỗi (kể cả rỗng: trang trắng vẫn là kết quả)", () => {
  assert.equal(trichXuat.kiemKetQua!({ noi_dung: "Điều 1." }), true);
  assert.equal(trichXuat.kiemKetQua!({ noi_dung: "" }), true);
  assert.equal(trichXuat.kiemKetQua!({ so_van_ban: "1" }), false);
  assert.equal(trichXuat.kiemKetQua!(null), false);
});

test("định nghĩa: tên, nhiệt độ, trần 20 MB; có trong danh sách tác vụ", () => {
  assert.equal(trichXuat.ten, "trich-xuat");
  assert.equal(trichXuat.nhiet_do, 0.1);
  assert.equal(trichXuat.toi_da_byte_tep, MAX_EXTRACT_BYTES);
  assert.equal(layTacVu("trich-xuat"), trichXuat);
});

const SQL_008 = readFileSync(join(__dirname, "..", "supabase", "migrations", "008_bang_gia_dot2b.sql"), "utf8");
export function giaTriTrong008(ma: string): CauHinhTacVu {
  const m = SQL_008.match(new RegExp(`'tac_vu',\\s*'${ma}',\\s*'(\\{[^']*\\})'`));
  if (!m) throw new Error(`008 thiếu dòng ${ma}`);
  return JSON.parse(m[1]) as CauHinhTacVu;
}

test("hiệu chỉnh theo bảng giá 008: 1 trang tối thiểu 3 credit, 6 trang 7–9 (spec mục 7: 3–9 credit cho 1–6 trang)", async () => {
  const bg: BangGia = {
    chung: { gia_tri_credit_vnd: 100, ty_gia_vnd_usd: 26000, mo_hinh_mac_dinh: "gemini-3.6-flash" },
    mo_hinh: { "gemini-3.6-flash": { vao_usd_trieu: 1.5, ra_usd_trieu: 7.5 } },
    tac_vu: { "trich-xuat": giaTriTrong008("trich-xuat") },
  };
  const phu: PhuThuoc = {
    bangGia: async () => bg,
    kho: { soDu: async () => 1000 } as unknown as KhoCredit,
    layKhoa: async () => null,
    goiAI: async () => { throw new Error("không gọi AI khi ước tính"); },
    bayGio: () => new Date(),
    toiDaTepByte: async () => 20 * 1024 * 1024,
  };
  const user = { id: "U1" } as ApiUser;
  const anh = (n: number) => Array.from({ length: n }, () => ANH);
  const mot = await uocTinhTacVu(trichXuat, { tep: anh(1) }, user, phu);
  const sau = await uocTinhTacVu(trichXuat, { tep: anh(6) }, user, phu);
  const chiChu = await uocTinhTacVu(trichXuat, { tep: anh(1), che_do: "chi-chu" }, user, phu);
  assert.ok(mot.ok && sau.ok && chiChu.ok);
  assert.equal(mot.credit_min, 3);
  assert.ok(mot.credit_max <= 12, `1 trang credit_max ${mot.credit_max}`);
  assert.ok(sau.credit_min >= 7 && sau.credit_min <= 9, `6 trang credit_min ${sau.credit_min}`);
  assert.ok(sau.credit_max <= 30, `6 trang credit_max ${sau.credit_max}`);
  assert.equal(chiChu.credit_min, 3);
});
```

`vanbanplus-api/tests/bang-gia-dot2b-csdl.test.ts`:

```ts
import { after, before, describe, test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { Client } from "pg";
import { CO_CSDL_THU, taoCsdlThu } from "./ho-tro/csdl-thu";
import { dungBangGia, type DongBangGia } from "../src/lib/credit/bang-gia";

const SQL_008 = readFileSync(join(__dirname, "..", "supabase", "migrations", "008_bang_gia_dot2b.sql"), "utf8");

describe("008: bảng giá trich-xuat, soan-thao", { skip: !CO_CSDL_THU }, () => {
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

  test("chạy hai lần không lỗi, thêm đúng 2 dòng, không đè giá quản trị đã sửa, bảng giá đọc được", async () => {
    await c.query(SQL_008);
    await c.query(
      `UPDATE credit_bang_gia SET gia_tri = jsonb_set(gia_tri, '{toi_thieu}', '5') WHERE loai = 'tac_vu' AND ma = 'soan-thao'`
    );
    await c.query(SQL_008);
    const { rows } = await c.query(`SELECT loai, ma, gia_tri FROM credit_bang_gia`);
    assert.equal(rows.filter((r) => r.loai === "tac_vu" && ["trich-xuat", "soan-thao"].includes(r.ma)).length, 2);
    const bg = dungBangGia(rows as DongBangGia[]);
    assert.equal(bg.tac_vu["soan-thao"].toi_thieu, 5);
    assert.equal(bg.tac_vu["trich-xuat"].token_moi_trang, 1100);
    assert.equal(bg.tac_vu["trich-xuat"].toi_da_ky_tu, 10000);
  });
});
```

Run: `npx tsx --test tests/tac-vu-trich-xuat.test.ts`
Expected: FAIL (không có `trich-xuat/dinh-nghia`).

- [ ] **Step 2: Viết mã**

`src/lib/tac-vu/trich-xuat/dinh-nghia.ts`:

```ts
import { MAX_EXTRACT_BYTES } from "../../extract-input";
import type { DinhNghiaTacVu, TepGui } from "../chay-tac-vu";
import { dungPromptTrichXuat, type CheDo, type DauVaoTrichXuat } from "./prompt";

export const SO_TEP_TOI_DA = 20;
/** Định dạng Gemini nhận nhúng trực tiếp. GIF/BMP/TIFF: bản cài máy đổi sang PNG/JPEG trước khi gửi. */
export const MIME_NHAN: readonly string[] = ["application/pdf", "image/png", "image/jpeg", "image/webp", "image/heic", "image/heif"];
const BASE64 = /^[A-Za-z0-9+/]+={0,2}$/;

export const trichXuat: DinhNghiaTacVu<DauVaoTrichXuat> = {
  ten: "trich-xuat",
  kieu_ket_qua: "json", // chi-chu ghi đè bằng PromptTacVu.kieu_ket_qua = "van_ban"
  nhiet_do: 0.1, // như route /api/ai/extract và /api/ai/read-text
  toi_da_byte_tep: MAX_EXTRACT_BYTES, // giới hạn dữ liệu nhúng của Gemini; gói còn có trần riêng (max_file_size_mb)
  kiemDauVao(body) {
    const b = (body && typeof body === "object" ? body : {}) as Record<string, unknown>;
    if (!Array.isArray(b.tep) || b.tep.length === 0) {
      return { ok: false, loi: "Thiếu tệp (tep): cần mảng 1–20 tệp dạng { base64, mime }." };
    }
    if (b.tep.length > SO_TEP_TOI_DA) {
      return { ok: false, loi: `Tối đa ${SO_TEP_TOI_DA} tệp mỗi lần (đang gửi ${b.tep.length}).` };
    }
    const che_do = (b.che_do == null || b.che_do === "" ? "cau-truc" : b.che_do) as CheDo;
    if (che_do !== "cau-truc" && che_do !== "chi-chu") {
      return { ok: false, loi: "che_do phải là «cau-truc» hoặc «chi-chu»." };
    }
    if (che_do === "chi-chu" && b.tep.length !== 1) {
      return { ok: false, loi: "Chế độ chi-chu nhận đúng 1 tệp." };
    }
    const tep: TepGui[] = [];
    for (let i = 0; i < b.tep.length; i++) {
      const t = (b.tep[i] ?? {}) as Record<string, unknown>;
      if (typeof t.base64 !== "string" || t.base64.length === 0 || t.base64.length % 4 !== 0 || !BASE64.test(t.base64)) {
        return { ok: false, loi: `Tệp ${i + 1}: base64 không hợp lệ.` };
      }
      if (typeof t.mime !== "string" || !MIME_NHAN.includes(t.mime)) {
        return { ok: false, loi: `Tệp ${i + 1}: định dạng chưa hỗ trợ (nhận PDF, PNG, JPEG, WEBP, HEIC, HEIF).` };
      }
      tep.push({ base64: t.base64, mime: t.mime });
    }
    return { ok: true, du_lieu: { tep, che_do } };
  },
  dungPrompt: (d) => dungPromptTrichXuat(d),
  // Lược đồ bắt buộc noi_dung; chuỗi rỗng vẫn hợp lệ (trang trắng), không thử lại vô ích.
  kiemKetQua: (kq) => typeof (kq as { noi_dung?: unknown } | null)?.noi_dung === "string",
};
```

`src/lib/tac-vu/dang-ky.ts`: thêm dòng import ngay sau dòng import tác vụ cuối cùng hiện có (sau các import của `dot2a`):

```ts
import { trichXuat } from "./trich-xuat/dinh-nghia";
```

và thêm một mục ngay trước dấu `};` đóng `DANH_SACH`:

```ts
  [trichXuat.ten]: trichXuat,
```

`supabase/migrations/008_bang_gia_dot2b.sql`:

```sql
-- ============================================================
-- 008: Bảng giá đợt 2b (trich-xuat, soan-thao). Chỉ thêm dòng; chạy lại không đổi gì, không đè giá quản trị đã sửa.
-- Spec: docs/superpowers/specs/2026-11-09-api-tac-vu-ai-credit-design.md mục 7.
-- trich-xuat: token_vao = ceil(ký tự prompt + lược đồ / 3) + số trang × token_moi_trang (1.100: ảnh Gemini 3 ≈ 1.120 token,
--   trang PDF ≈ 560; lấy mức cao). 1 ảnh → 3–11 credit, 6 ảnh → 8–29 (spec: trừ thật 3–9 cho 1–6 trang).
--   toi_da_ky_tu chỉ áp phần chữ (prompt cố định ~1.200 ký tự); tệp giới hạn theo byte (min(gói, 20 MB)) và 1–20 tệp.
-- soan-thao: spec 4–13 credit (trung vị 9); văn bản thường 4–20 credit giữ tạm. ra_max cũng là trần token ra
--   (gồm suy nghĩ): ~9.000 token với prompt điển hình, cao hơn lượt dài nhất đo được (~6.000).
-- Hệ số đo trên gemini-3.5-flash ngày 09/11/2026; đo lại trên 3.6 trước khi mở bán (spec mục 11).
-- ============================================================

SET lock_timeout = '5s';

INSERT INTO credit_bang_gia (loai, ma, gia_tri) VALUES
    ('tac_vu', 'trich-xuat', '{"toi_thieu": 3, "ra_min_he_so": 0.3, "ra_min_hang": 300, "ra_max_he_so": 1.5, "ra_max_hang": 2000, "toi_da_ky_tu": 10000, "token_moi_trang": 1100}'),
    ('tac_vu', 'soan-thao', '{"toi_thieu": 4, "ra_min_he_so": 0.5, "ra_min_hang": 600, "ra_max_he_so": 1.0, "ra_max_hang": 7000, "toi_da_ky_tu": 30000}')
ON CONFLICT (loai, ma) DO NOTHING;
```

- [ ] **Step 3: Chạy, thấy qua (cả bài CSDL)**

```bash
bash scripts/csdl-thu.sh bat   # rồi mở đường hầm ở cửa sổ khác: ssh -N -L 55990:127.0.0.1:55990 gikidi-dev
TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npx tsx --test tests/tac-vu-trich-xuat.test.ts tests/bang-gia-dot2b-csdl.test.ts && npx tsc --noEmit -p .
```
Expected: mọi bài pass, bài CSDL **không** bị bỏ qua (`# skipped 0`).

- [ ] **Step 4: Commit**

```bash
git add src/lib/tac-vu/trich-xuat/dinh-nghia.ts src/lib/tac-vu/dang-ky.ts supabase/migrations/008_bang_gia_dot2b.sql tests/tac-vu-trich-xuat.test.ts tests/bang-gia-dot2b-csdl.test.ts && git commit -F - <<'EOF'
feat(tac-vu): trich-xuat (cau-truc, chi-chu) qua /api/v2; bang gia 008 cho trich-xuat, soan-thao

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
```

---

### Task 5: C#: chụp prompt của hộp thoại soạn thảo CŨ

**Files:**
- Modify: `AIVanBan.Tests/AIVanBan.Tests.csproj` (thêm `ProjectReference` tới Desktop)
- Create: `AIVanBan.Tests/MauSoanThao.cs`
- Create: `AIVanBan.Tests/SoanThaoTuongDuongTests.cs`
- Create (sinh ra): `AIVanBan.Tests/MauCu/soan-thao-gia-kiem.json`

**Interfaces:**
- Consumes: `AIComposeDialog` (riêng tư: trường `_selectedTemplate`, hàm `BuildSystemInstruction()`, `BuildStructuredPrompt(DocumentType, Dictionary<string,string>)`).
- Produces: `MauSoanThao.Ca(string Ten, DocumentType Loai, string? TenMau, (string Khoa, string GiaTri)[] Truong, ComposeLocality? DiaPhuong)`, với `Ca.TuDien()`, `MauSoanThao.GiaKiem` (42 ca), `MauSoanThao.ThemCa`, `MauSoanThao.TatCa`, `MauSoanThao.DiaPhuongJson(...)`. Bản chụp `MauCu/soan-thao-gia-kiem.json`: mảng `{ ten, he_thong, nguoi_dung }`, xuống dòng chuẩn hóa `\n`.

`ComposeLocality` chưa có ở task này, nên `MauSoanThao.cs` ở Step 2 tạm dùng `object?` cho địa phương. Task 6 đổi sang kiểu thật. Cách này giữ được thứ tự «chụp cũ trước, viết lớp mới sau».

- [ ] **Step 1: Tham chiếu Desktop từ dự án kiểm**

Trong `AIVanBan.Tests/AIVanBan.Tests.csproj`, ngay sau dòng `<ProjectReference Include="..\AIVanBan.Core\AIVanBan.Core.csproj" />` thêm:

```xml
    <ProjectReference Include="..\AIVanBan.Desktop\AIVanBan.Desktop.csproj" />
```

Run: `cd /d/AIVanBanCaNhan && dotnet build AIVanBan.Tests`
Expected: build thành công. Nếu lỗi `NETSDK1150`/`NETSDK1151` (tham chiếu tệp chạy khác chế độ self-contained), thêm vào `<PropertyGroup>` của csproj kiểm dòng `<ValidateExecutableReferencesMatchSelfContained>false</ValidateExecutableReferencesMatchSelfContained>` rồi build lại. Nếu bản cài máy đang chạy và khóa tệp `bin` thì đóng ứng dụng trước.

- [ ] **Step 2: Các ca đầu vào**

`AIVanBan.Tests/MauSoanThao.cs` (bản tạm của Task 5: địa phương kiểu `object?`, chỉ có danh sách Gia Kiệm):

```csharp
using AIVanBan.Core.Models;

namespace AIVanBan.Tests;

/// <summary>
/// Đầu vào mẫu cho prompt soạn thảo: phủ mọi DocumentType và mọi nhánh của từng hàm dựng trong AIComposeDialog.
/// Thứ tự trường = thứ tự ô nhập (nhánh soạn chung liệt kê theo thứ tự này).
/// </summary>
internal static class MauSoanThao
{
    internal sealed record Ca(string Ten, DocumentType Loai, string? TenMau, (string Khoa, string GiaTri)[] Truong, object? DiaPhuong)
    {
        public Dictionary<string, string> TuDien()
        {
            var d = new Dictionary<string, string>();
            foreach (var (k, v) in Truong) d[k] = v;
            return d;
        }
    }

    private static (string Khoa, string GiaTri)[] T(params string[] kv)
    {
        if (kv.Length % 2 != 0) throw new ArgumentException("Cần cặp khóa, giá trị");
        return Enumerable.Range(0, kv.Length / 2).Select(i => (kv[2 * i], kv[2 * i + 1])).ToArray();
    }

    private static Ca G(string ten, DocumentType loai, string tenMau, params string[] kv) => new(ten, loai, tenMau, T(kv), "GiaKiem");

    /// <summary>Ca chạy được trên hộp thoại cũ: địa phương Gia Kiệm, luôn có tên mẫu (hộp thoại luôn có mẫu khi soạn).</summary>
    public static IReadOnlyList<Ca> GiaKiem { get; } = new[]
    {
        G("cong-van-day-du", DocumentType.CongVan, "Công văn đề nghị",
            "from_org", "UBND xã Minh An", "to_org", "Sở Tài chính tỉnh Lâm Đồng", "subject", "Đề nghị bổ sung kinh phí sửa chữa trường học",
            "signer_name", "Trần Văn Bình", "signer_title", "Chủ tịch", "content", "Trường Tiểu học Minh An xuống cấp.\nMái dột 8 phòng học.",
            "reply_to_number", "15/STC-NS ngày 02/11/2026", "proposal", "Bổ sung 500 triệu đồng trong quý IV/2026"),
        G("cong-van-du-phong", DocumentType.CongVan, "",
            "to_department", "Phòng Kinh tế", "chairman_name", "Lê Thị Hoa", "signer_title", "Phó Chủ tịch", "content", "Báo cáo tiến độ giải ngân."),
        G("quyet-dinh-day-du", DocumentType.QuyetDinh, "Quyết định khen thưởng",
            "person_name", "Phạm Văn Cường", "current_position", "Công chức Văn phòng", "from_unit", "Văn phòng UBND", "to_unit", "Ban Địa chính",
            "new_position", "Phó ban", "effective_date", "01/12/2026", "award_type", "Giấy khen", "recipient", "Thôn 3",
            "achievement", "Dẫn đầu phong trào xây dựng nông thôn mới", "org_name", "Tổ công tác chuyển đổi số", "members", "5 thành viên",
            "tasks", "Hướng dẫn người dân dùng dịch vụ công", "project_name", "Đề án chuyển đổi số", "objectives", "100% hồ sơ trực tuyến",
            "budget", "200 triệu đồng", "implementing_unit", "Văn phòng UBND xã", "signer_name", "Trần Văn Bình", "signer_title", "Chủ tịch"),
        G("quyet-dinh-chu-tich", DocumentType.QuyetDinh, "Quyết định điều động",
            "person_name", "Phạm Văn Cường", "chairman_name", "Lê Thị Hoa"),
        G("quyet-dinh-rong", DocumentType.QuyetDinh, ""),
        G("bao-cao-day-du", DocumentType.BaoCao, "Báo cáo tình hình tháng",
            "org_name", "UBND xã Minh An", "period", "Tháng 10/2026", "signer_name", "Trần Văn Bình", "signer_title", "Chủ tịch",
            "achievements", "Thu ngân sách đạt 92%.", "situation", "Mưa lớn kéo dài.", "results", "Sửa 3 km đường.",
            "challenges", "Thiếu biên chế.", "future_plans", "Hoàn thành giải ngân.", "proposals", "Hỗ trợ 200 triệu đồng."),
        G("bao-cao-linh-vuc", DocumentType.BaoCao, "Báo cáo chuyên đề",
            "org_name", "UBND xã Minh An", "field", "Phòng chống thiên tai", "situation", "Ngập cục bộ 2 thôn."),
        G("to-trinh-day-du", DocumentType.ToTrinh, "Tờ trình",
            "org_name", "UBND xã Minh An", "recipient", "UBND tỉnh Lâm Đồng", "subject", "Xin chủ trương xây nhà văn hóa",
            "reason", "Nhà văn hóa cũ xuống cấp.", "content", "Xây mới nhà văn hóa thôn 3.", "budget", "1,2 tỷ đồng",
            "signer_name", "Trần Văn Bình", "signer_title", "Chủ tịch"),
        G("to-trinh-de-xuat", DocumentType.ToTrinh, "Tờ trình đề xuất",
            "org_name", "UBND xã Minh An", "recipient", "Sở Nội vụ", "proposal", "Bổ sung 2 biên chế công chức", "reason", "Khối lượng công việc tăng."),
        G("ke-hoach-cong-tac", DocumentType.KeHoach, "Kế hoạch công tác",
            "org_name", "UBND xã Minh An", "period", "năm 2027", "objectives", "Hoàn thành 100% chỉ tiêu.",
            "tasks", "1. Chuyển đổi số.\n2. Cải cách hành chính.", "budget", "300 triệu đồng", "signer_name", "Trần Văn Bình", "signer_title", "Chủ tịch"),
        G("ke-hoach-su-kien", DocumentType.KeHoach, "Kế hoạch tổ chức sự kiện",
            "event_name", "Ngày hội Đại đoàn kết", "time_place", "8h00 ngày 18/11/2026, sân văn hóa thôn 2", "purpose", "Tăng cường đoàn kết.",
            "program", "Văn nghệ, thể thao."),
        G("ke-hoach-chuong-trinh", DocumentType.KeHoach, "Kế hoạch",
            "program", "Chương trình tập huấn", "period", "quý I/2027"),
        G("thong-bao-day-du", DocumentType.ThongBao, "Thông báo kết luận cuộc họp",
            "meeting_name", "Họp giao ban tháng 11", "event_name", "Giao ban", "time", "14h00 ngày 20/11/2026", "location", "Hội trường UBND xã",
            "participants", "Lãnh đạo UBND, công chức", "agenda", "Đánh giá tháng 11", "content", "Kết luận của Chủ tịch",
            "conclusion", "Hoàn thành giải ngân trước 15/12.", "tasks", "Văn phòng tổng hợp.", "signer_name", "Trần Văn Bình", "signer_title", "Chủ tịch"),
        G("thong-bao-rong", DocumentType.ThongBao, "Thông báo"),
        G("nghi-quyet-hdnd", DocumentType.NghiQuyet, "Nghị quyết HĐND về dự toán",
            "level", "xã", "subject", "Dự toán ngân sách 2027", "articles", "Điều 1. Thông qua dự toán.\nĐiều 2. Giao UBND tổ chức thực hiện.",
            "effective_date", "01/01/2027", "implementing_unit", "UBND xã Minh An", "chairman_name", "Nguyễn Thị Lan"),
        G("nghi-quyet-ubnd", DocumentType.NghiQuyet, "Nghị quyết UBND",
            "level", "xã", "subject", "Quy chế làm việc", "articles", "Điều 1. Ban hành quy chế.", "chairman_name", "Trần Văn Bình"),
        G("chi-thi", DocumentType.ChiThi, "Chỉ thị",
            "from_org", "UBND xã Minh An", "subject", "Tăng cường phòng cháy", "content", "Kiểm tra 100% cơ sở kinh doanh.",
            "signer_name", "Trần Văn Bình", "signer_title", "Chủ tịch"),
        G("quy-che", DocumentType.QuyChE, "Quy chế", "subject", "Quy chế làm việc", "content", "Phạm vi, đối tượng, chế độ họp."),
        G("quy-dinh", DocumentType.QuyDinh, "Quy định", "subject", "Quy định tiếp công dân", "content", "Lịch tiếp thứ Ba hằng tuần."),
        G("thong-cao", DocumentType.ThongCao, "Thông cáo", "subject", "Kết quả bầu cử", "content", "Tỷ lệ cử tri đi bầu 99%."),
        G("huong-dan", DocumentType.HuongDan, "Hướng dẫn", "subject", "Đăng ký tạm trú", "content", "Nộp hồ sơ trực tuyến.", "from_org", "Công an xã"),
        G("chuong-trinh", DocumentType.ChuongTrinh, "Chương trình", "subject", "Chương trình công tác năm", "content", "12 nhiệm vụ trọng tâm.", "timeline", "Năm 2027"),
        G("phuong-an", DocumentType.PhuongAn, "Phương án", "subject", "Phương án phòng chống bão", "objectives", "Không thiệt hại người",
            "content", "Sơ tán 50 hộ.", "budget", "50 triệu đồng"),
        G("de-an", DocumentType.DeAn, "Đề án", "subject", "Đề án chuyển đổi số", "objectives", "Chính quyền số", "content", "3 trụ cột.", "budget", "1 tỷ đồng"),
        G("du-an", DocumentType.DuAn, "Dự án", "subject", "Dự án đường liên thôn", "objectives", "Nhựa hóa 5 km", "content", "Làm mới mặt đường."),
        G("bien-ban", DocumentType.BienBan, "Biên bản", "subject", "Họp dân thôn 3", "time", "19h00 ngày 10/11/2026", "location", "Nhà văn hóa thôn 3",
            "attendees", "45 hộ dân", "content", "Thống nhất góp đất làm đường."),
        G("hop-dong", DocumentType.HopDong, "Hợp đồng", "subject", "Thuê bảo vệ", "party_a", "UBND xã Minh An", "party_b", "Công ty Bảo vệ An Toàn",
            "content", "Bảo vệ trụ sở 24/24."),
        G("cong-dien", DocumentType.CongDien, "Công điện", "subject", "Ứng phó bão số 9", "to_org", "Trưởng các thôn", "content", "Sơ tán dân vùng trũng."),
        G("ban-ghi-nho", DocumentType.BanGhiNho, "Bản ghi nhớ", "subject", "Hợp tác đào tạo", "content", "Đào tạo kỹ năng số.", "parties", "UBND xã và Trường Cao đẳng"),
        G("ban-thoa-thuan", DocumentType.BanThoaThuan, "Bản thỏa thuận", "subject", "Dùng chung sân bóng", "content", "Luân phiên theo tuần.", "parties", "Thôn 1 và Thôn 2"),
        G("giay-uy-quyen", DocumentType.GiayUyQuyen, "Giấy ủy quyền", "grantor", "Trần Văn Bình", "grantee", "Lê Thị Hoa", "content", "Ký văn bản thường xuyên.",
            "duration", "Từ 01/12 đến 31/12/2026"),
        G("giay-moi", DocumentType.GiayMoi, "Giấy mời", "subject", "Dự hội nghị tổng kết", "time", "8h00 ngày 28/12/2026", "location", "Hội trường UBND xã",
            "to_org", "Trưởng các thôn"),
        G("giay-gioi-thieu", DocumentType.GiayGioiThieu, "Giấy giới thiệu", "person", "Phạm Văn Cường", "to_org", "Sở Nội vụ", "content", "Liên hệ công tác biên chế."),
        G("giay-nghi-phep", DocumentType.GiayNghiPhep, "Giấy nghỉ phép", "person", "Lê Thị Hoa", "from_date", "01/12/2026", "to_date", "05/12/2026", "reason", "Nghỉ phép năm."),
        G("phieu-gui", DocumentType.PhieuGui, "Phiếu gửi", "to_org", "Văn phòng UBND tỉnh", "content", "Gửi báo cáo tháng.", "documents", "Báo cáo số 12/BC-UBND"),
        G("phieu-chuyen", DocumentType.PhieuChuyen, "Phiếu chuyển", "to_org", "Ban Địa chính", "content", "Chuyển đơn của công dân.", "documents", "Đơn ngày 03/11/2026"),
        G("phieu-bao", DocumentType.PhieuBao, "Phiếu báo", "to_org", "Các thôn", "content", "Báo lịch cắt điện.", "documents", "Thông báo của Điện lực"),
        G("thu-cong", DocumentType.ThuCong, "Thư công", "letter_type", "cảm ơn", "to_org", "Công ty Bảo vệ An Toàn", "content", "Cảm ơn hỗ trợ trong mùa mưa bão."),
        G("khac-chung", DocumentType.Khac, "Mẫu văn bản khác",
            "subject", "Xác nhận hộ nghèo", "so_ho_dan", "125", "ghi_chu_rieng", "   ", "signer_name", "", "location", "Thôn 4",
            "content", "Nội dung có \"ngoặc kép\", dấu \\ và ký tự ``` ."),
        G("luat", DocumentType.Luat, "Luật", "subject", "Luật mẫu", "content", "Nội dung luật."),
        G("nghi-dinh", DocumentType.NghiDinh, "Nghị định", "subject", "Nghị định mẫu", "content", "Nội dung nghị định."),
        G("thong-tu", DocumentType.ThongTu, "Thông tư", "subject", "Thông tư mẫu", "content", "Nội dung thông tư."),
    };
}
```

Đếm: 42 ca, phủ 33 loại `DocumentType`.

- [ ] **Step 3: Bộ kiểm chụp hộp thoại cũ**

`AIVanBan.Tests/SoanThaoTuongDuongTests.cs`:

```csharp
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using AIVanBan.Core.Models;
using AIVanBan.Desktop.Views;

namespace AIVanBan.Tests;

/// <summary>
/// Bằng chứng tách prompt soạn thảo sang AIVanBan.Core không đổi hành vi (spec mục 10).
/// Lần chạy đầu (trên hộp thoại CŨ, Task 5 kế hoạch đợt 2b) chụp prompt vào MauCu/soan-thao-gia-kiem.json; từ đó mọi
/// lần chạy so hộp thoại (và lớp mới) với bản chụp. KHÔNG xóa / sinh lại bản chụp sau khi hộp thoại đã sửa.
/// </summary>
public class SoanThaoTuongDuongTests
{
    private const BindingFlags RIENG = BindingFlags.NonPublic | BindingFlags.Instance;

    private static readonly JsonSerializerOptions TUY_CHON = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    internal sealed record Chup(string ten, string he_thong, string nguoi_dung);

    internal static string TepChup()
    {
        for (var d = new DirectoryInfo(AppContext.BaseDirectory); d != null; d = d.Parent)
        {
            if (File.Exists(Path.Combine(d.FullName, "AIVanBan.sln")))
                return Path.Combine(d.FullName, "AIVanBan.Tests", "MauCu", "soan-thao-gia-kiem.json");
        }
        throw new DirectoryNotFoundException($"Không tìm thấy AIVanBan.sln khi đi ngược từ {AppContext.BaseDirectory}");
    }

    internal static string ChuanHoa(string s) => s.Replace("\r\n", "\n");

    internal static List<Chup> DocChup() =>
        JsonSerializer.Deserialize<List<Chup>>(File.ReadAllText(TepChup())) ?? throw new InvalidDataException("Bản chụp rỗng");

    /// <summary>
    /// Gọi hai hàm prompt riêng tư của hộp thoại trên một đối tượng CHƯA khởi tạo (không InitializeComponent, không cần
    /// giao diện). Hai hàm này chỉ đọc _selectedTemplate và tham số, nên đủ để tái tạo prompt. Chạy trên luồng STA cho chắc.
    /// </summary>
    private static (string HeThong, string NguoiDung) QuaHopThoai(MauSoanThao.Ca ca)
    {
        (string, string) kq = default;
        Exception? loi = null;
        var t = new Thread(() =>
        {
            try
            {
                var kieu = typeof(AIComposeDialog);
                var dlg = RuntimeHelpers.GetUninitializedObject(kieu);
                kieu.GetField("_selectedTemplate", RIENG)!
                    .SetValue(dlg, new DocumentTemplate { Type = ca.Loai, Name = ca.TenMau ?? "" });
                var heThong = (string)kieu.GetMethod("BuildSystemInstruction", RIENG, Type.EmptyTypes)!.Invoke(dlg, null)!;
                var nguoiDung = (string)kieu.GetMethod("BuildStructuredPrompt", RIENG)!.Invoke(dlg, new object[] { ca.Loai, ca.TuDien() })!;
                kq = (heThong, nguoiDung);
            }
            catch (Exception e)
            {
                loi = e;
            }
        });
        t.SetApartmentState(ApartmentState.STA);
        t.Start();
        t.Join();
        if (loi != null) throw new InvalidOperationException($"Không gọi được hàm prompt của hộp thoại (ca {ca.Ten})", loi);
        return kq;
    }

    [Fact]
    public void HopThoaiKhopBanChup()
    {
        var hienTai = MauSoanThao.GiaKiem.Select(ca =>
        {
            var (h, n) = QuaHopThoai(ca);
            return new Chup(ca.Ten, ChuanHoa(h), ChuanHoa(n));
        }).ToList();

        var duongDan = TepChup();
        if (!File.Exists(duongDan))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(duongDan)!);
            File.WriteAllText(duongDan, JsonSerializer.Serialize(hienTai, TUY_CHON).Replace("\r\n", "\n") + "\n");
            Assert.Fail($"Vừa chụp prompt của hộp thoại vào {duongDan}: kiểm tra, commit, rồi chạy lại.");
        }

        var chup = DocChup();
        Assert.Equal(chup.Select(c => c.ten), hienTai.Select(c => c.ten));
        for (var i = 0; i < chup.Count; i++)
        {
            Assert.True(chup[i].he_thong == hienTai[i].he_thong, $"{chup[i].ten}: he_thong lệch bản chụp");
            Assert.True(chup[i].nguoi_dung == hienTai[i].nguoi_dung, $"{chup[i].ten}: nguoi_dung lệch bản chụp");
        }
    }
}
```

- [ ] **Step 4: Chụp (lần 1 hỏng có chủ ý, lần 2 qua)**

```bash
cd /d/AIVanBanCaNhan && git diff --quiet -- AIVanBan.Desktop/Views/AIComposeDialog.xaml.cs && echo "hộp thoại chưa sửa"
dotnet test AIVanBan.Tests --filter "FullyQualifiedName~SoanThaoTuongDuongTests"; dotnet test AIVanBan.Tests --filter "FullyQualifiedName~SoanThaoTuongDuongTests"
```
Expected: dòng đầu in `hộp thoại chưa sửa` (bắt buộc: bản chụp phải lấy từ mã CŨ). Lần chạy 1 FAIL với «Vừa chụp prompt…», lần 2 `Passed! … 1`. Mở `AIVanBan.Tests/MauCu/soan-thao-gia-kiem.json` xem nhanh: 42 mục, `he_thong` có «BỐI CẢNH ĐỊA PHƯƠNG (xã Gia Kiệm)», mục `khac-chung` có «📝 So Ho Dan: 125» và không có dòng `ghi_chu_rieng` hay `signer_name`.

Nếu `GetUninitializedObject` hoặc reflection lỗi (ngoại lệ trong `QuaHopThoai`), dùng **đường dự phòng**: chép nguyên văn khối từ `    private string BuildSystemInstruction()` tới ngay trước `    private void Save_Click(`, cùng `GetFieldLabel` + `FormatFieldName`, của tệp hộp thoại ở commit hiện tại vào `AIVanBan.Tests/MauCu/HopThoaiCu.cs`. Đặt vào một lớp `internal sealed class HopThoaiCu { private DocumentTemplate? _selectedTemplate; … }`, chỉ thêm hàm khởi tạo nhận `DocumentTemplate`, rồi gọi lớp này thay cho reflection. Ghi lại quyết định trong mô tả commit.

- [ ] **Step 5: Commit (repo gốc)**

```bash
git add AIVanBan.Tests/AIVanBan.Tests.csproj AIVanBan.Tests/MauSoanThao.cs AIVanBan.Tests/SoanThaoTuongDuongTests.cs AIVanBan.Tests/MauCu/soan-thao-gia-kiem.json && git commit -F - <<'EOF'
test: chup prompt soan thao cua AIComposeDialog cu (42 ca Gia Kiem) lam moc tuong duong

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
```

---

### Task 6: C#: lớp thuần `ComposePromptBuilder` trong `AIVanBan.Core` (tách bằng script) + so tương đương

**Files:**
- Create (một lần, không commit): `D:/AIVanBanCaNhan/.superpowers/tam/tach-soan-thao.mjs`
- Create (sinh ra): `AIVanBan.Core/Services/ComposePromptBuilder.cs`
- Modify: `AIVanBan.Tests/MauSoanThao.cs` (địa phương thành `ComposeLocality?`)
- Modify: `AIVanBan.Tests/SoanThaoTuongDuongTests.cs` (thêm `LopMoiKhopBanChup`)

**Interfaces:**
- Produces (`AIVanBan.Core.Services`):
  - `public sealed record ComposeLocality(string Xa, string? CoQuanCapTren, string Tinh, IReadOnlyList<string>? DacDiem = null)`, `static ComposeLocality GiaKiem`, `static string TenDiaDanh(string xa)`
  - `public sealed class ComposePromptBuilder`:
    - `static string BuildSystemInstruction(DocumentType type, ComposeLocality? dp)`
    - `static string BuildUserPrompt(DocumentType type, string? templateName, Dictionary<string, string> fields, ComposeLocality? dp)`
    - `static string GetFieldLabel(string field)`
    - `static string FormatFieldName(string field)`
- Hộp thoại CHƯA đổi ở task này (Task 7 mới đổi).

- [ ] **Step 1: Script tách**

`D:/AIVanBanCaNhan/.superpowers/tam/tach-soan-thao.mjs`:

```js
// Dùng: node tach-soan-thao.mjs tao-lop      (Task 6: sinh AIVanBan.Core/Services/ComposePromptBuilder.cs từ hộp thoại)
//       node tach-soan-thao.mjs sua-hop-thoai (Task 7: xóa các hàm prompt khỏi hộp thoại, thay bằng lời gọi lớp mới)
// Chép NGUYÊN VĂN, chỉ thay đúng các chuỗi liệt kê; mỗi chuỗi phải khớp đúng số lần, sai là dừng.
import { readFileSync, writeFileSync, existsSync } from "node:fs";

const GOC = "D:/AIVanBanCaNhan";
const HOP_THOAI = `${GOC}/AIVanBan.Desktop/Views/AIComposeDialog.xaml.cs`;
const LOP = `${GOC}/AIVanBan.Core/Services/ComposePromptBuilder.cs`;
const VACH = "═".repeat(39);

const tho = readFileSync(HOP_THOAI, "utf8");
const BOM = tho.startsWith("\uFEFF") ? "\uFEFF" : "";
const NL = tho.includes("\r\n") ? "\r\n" : "\n";
const src = tho.slice(BOM.length).replace(/\r\n/g, "\n");

function doan(tu, den) {
  const a = src.indexOf(tu);
  if (a < 0 || src.indexOf(tu, a + 1) >= 0) throw new Error(`Mốc đầu không có hoặc không duy nhất: ${tu}`);
  const b = src.indexOf(den, a);
  if (b < 0) throw new Error(`Không thấy mốc cuối: ${den}`);
  return [a, b];
}
function thay(s, cu, moi, soLan = 1) {
  const n = s.split(cu).length - 1;
  if (n !== soLan) throw new Error(`«${cu.slice(0, 70)}» xuất hiện ${n} lần, cần ${soLan}`);
  return s.split(cu).join(moi);
}
const MOC = {
  nhan: ["    private string GetFieldLabel(string field)", "    private string GetFieldHint(string field)"],
  heThong: ["    private string BuildSystemInstruction()", "    private string BuildPrompt()"],
  dung: ["    private string BuildStructuredPrompt(", "    private void Save_Click("],
};

const DAU = `using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Địa phương đưa vào prompt soạn thảo (spec mục 10; hợp đồng tác vụ v2, trường «dia_phuong»).
/// CoQuanCapTren bỏ trống: cấp trên là tỉnh (chính quyền hai cấp). DacDiem: mỗi phần tử thành một dòng «- …».
/// </summary>
public sealed record ComposeLocality(string Xa, string? CoQuanCapTren, string Tinh, IReadOnlyList<string>? DacDiem = null)
{
    /// <summary>Giá trị trước đây ghi cứng trong AIComposeDialog. Hộp thoại dùng tới khi đợt 3 nối thiết lập cơ quan.</summary>
    public static ComposeLocality GiaKiem { get; } = new(
        "xã Gia Kiệm", "thành phố Thống Nhất", "tỉnh Đồng Nai",
        new[]
        {
            "Dân số: 79.274 nhân khẩu, 19.818 hộ",
            "17 ấp, 5 giáo xứ Công giáo (96% dân theo đạo)",
            "Xã loại I, đang xây dựng nông thôn mới nâng cao",
        });

    /// <summary>Địa danh ở dòng ngày tháng: bỏ tiền tố «xã», «phường», «thị trấn», «đặc khu».</summary>
    public static string TenDiaDanh(string xa)
    {
        foreach (var p in new[] { "xã ", "phường ", "thị trấn ", "đặc khu " })
            if (xa.StartsWith(p, StringComparison.OrdinalIgnoreCase)) return xa[p.Length..].Trim();
        return xa.Trim();
    }
}

/// <summary>
/// Prompt soạn thảo văn bản, tách NGUYÊN VĂN từ AIComposeDialog (đợt 2b). Đầu vào thuần: loại văn bản, tên mẫu,
/// các trường (khóa → giá trị, theo thứ tự ô nhập) và địa phương. Không có địa phương thì prompt không nêu địa danh nào.
/// Máy chủ (vanbanplus-api, tác vụ soan-thao) dựng lại y hệt; bộ kiểm so khớp tệp mẫu do lớp này sinh ra.
/// </summary>
public sealed class ComposePromptBuilder
{
    private readonly string? _templateName;
    private readonly ComposeLocality? _dp;

    private ComposePromptBuilder(string? templateName, ComposeLocality? dp)
    {
        if (dp != null && (string.IsNullOrWhiteSpace(dp.Xa) || string.IsNullOrWhiteSpace(dp.Tinh)))
            throw new ArgumentException("Địa phương phải có xã/phường và tỉnh", nameof(dp));
        _templateName = templateName;
        _dp = dp;
    }

    /// <summary>System instruction. dp = null: không có bối cảnh địa phương.</summary>
    public static string BuildSystemInstruction(DocumentType type, ComposeLocality? dp) =>
        new ComposePromptBuilder(null, dp).BuildSystemInstructionCore(type);

    /// <summary>User prompt. fields theo thứ tự ô nhập; templateName là tên mẫu đang chọn (null: chưa chọn mẫu).</summary>
    public static string BuildUserPrompt(DocumentType type, string? templateName, Dictionary<string, string> fields, ComposeLocality? dp) =>
        new ComposePromptBuilder(templateName, dp).BuildStructuredPrompt(type, fields);

    // ---- Địa phương. Có _dp: dựng đúng chữ cũ (Gia Kiệm khớp từng ký tự). Không có: chỗ trống trung tính. ----

    // Xuống dòng đúng kiểu của tệp nguồn này (CRLF khi checkout trên Windows), giống mọi chuỗi @"..." khác của lớp.
    private static readonly string NL = @"
";
    private const string VACH = "${VACH}";

    private string? CapTren => string.IsNullOrWhiteSpace(_dp?.CoQuanCapTren) ? null : _dp!.CoQuanCapTren;
    private string CoQuanThuong => _dp == null ? "[cơ quan ban hành]" : $"UBND {_dp.Xa}";
    private string CoQuanHoa => _dp == null ? "[CƠ QUAN BAN HÀNH]" : $"UBND {_dp.Xa.ToUpperInvariant()}";
    private string CapTrenHoa => _dp == null ? "[CƠ QUAN CẤP TRÊN]" : $"UBND {(CapTren ?? _dp.Tinh).ToUpperInvariant()}";
    private string TenXaHoa => _dp == null ? "[TÊN XÃ/PHƯỜNG]" : _dp.Xa.ToUpperInvariant();
    private string DiaDanh => _dp == null ? "[Địa danh]" : ComposeLocality.TenDiaDanh(_dp.Xa);
    private string TenCoQuanQuyetDinh => _dp == null ? "" : $" (UBND {_dp.Xa.ToUpperInvariant()})";

    private string KhoiDiaPhuong
    {
        get
        {
            if (_dp == null) return "";
            var sb = new System.Text.StringBuilder();
            sb.Append(VACH).Append(NL);
            sb.Append("BỐI CẢNH ĐỊA PHƯƠNG (").Append(_dp.Xa).Append("):").Append(NL);
            sb.Append(VACH).Append(NL);
            sb.Append("- ").Append(char.ToUpperInvariant(_dp.Xa[0])).Append(_dp.Xa, 1, _dp.Xa.Length - 1).Append(", ");
            if (CapTren != null) sb.Append(CapTren).Append(", ");
            sb.Append(_dp.Tinh).Append(NL);
            foreach (var d in _dp.DacDiem ?? Array.Empty<string>()) sb.Append("- ").Append(d).Append(NL);
            sb.Append("- Cơ quan cấp trên: ");
            if (CapTren != null) sb.Append("UBND ").Append(CapTren).Append(", ");
            sb.Append("UBND ").Append(_dp.Tinh).Append(NL).Append(NL);
            return sb.ToString();
        }
    }

    // ==== Dưới đây chép nguyên văn từ AIComposeDialog.xaml.cs (chỉ thay các chỗ địa phương / tên mẫu) ====

`;

function taoLop() {
  if (existsSync(LOP)) throw new Error(`${LOP} đã có`);
  let [a, b] = doan(...MOC.heThong);
  let heThong = src.slice(a, b);
  heThong = thay(heThong, "    private string BuildSystemInstruction()", "    private string BuildSystemInstructionCore(DocumentType type)");
  heThong = thay(heThong, 'var templateType = _selectedTemplate?.Type.ToString() ?? "CongVan";', "var templateType = type.ToString();");
  heThong = thay(heThong,
    "   - Cơ quan cấp trên (nếu có): UBND THÀNH PHỐ THỐNG NHẤT\n   - Cơ quan ban hành: UBND XÃ GIA KIỆM (viết hoa, in đậm)",
    "   - Cơ quan cấp trên (nếu có): {CapTrenHoa}\n   - Cơ quan ban hành: {CoQuanHoa} (viết hoa, in đậm)");
  heThong = thay(heThong, '   ""Gia Kiệm, ngày ... tháng ... năm 2026""', '   ""{DiaDanh}, ngày ... tháng ... năm 2026""');
  heThong = thay(heThong,
    `${VACH}\nBỐI CẢNH ĐỊA PHƯƠNG (xã Gia Kiệm):\n${VACH}\n- Xã Gia Kiệm, thành phố Thống Nhất, tỉnh Đồng Nai\n` +
    "- Dân số: 79.274 nhân khẩu, 19.818 hộ\n- 17 ấp, 5 giáo xứ Công giáo (96% dân theo đạo)\n" +
    "- Xã loại I, đang xây dựng nông thôn mới nâng cao\n- Cơ quan cấp trên: UBND thành phố Thống Nhất, UBND tỉnh Đồng Nai\n\nHãy soạn",
    "{KhoiDiaPhuong}Hãy soạn");

  [a, b] = doan(...MOC.dung);
  let dung = src.slice(a, b);
  dung = thay(dung, '_selectedTemplate?.Name?.Contains("HĐND") == true', '_templateName?.Contains("HĐND") == true');
  dung = thay(dung, '_selectedTemplate?.Name ?? "Công văn"', '_templateName ?? "Công văn"');
  dung = thay(dung, '_selectedTemplate?.Name ?? ""', '_templateName ?? ""', 3);
  dung = thay(dung, "UBND xã Gia Kiệm xin trả lời như sau:", "{CoQuanThuong} xin trả lời như sau:");
  dung = thay(dung, "2. Tên cơ quan ban hành (UBND XÃ GIA KIỆM)", '2. Tên cơ quan ban hành" + TenCoQuanQuyetDinh + @"');
  dung = thay(dung, '? "UBND xã Gia Kiệm" : get("org_name")', '? CoQuanThuong : get("org_name")');
  dung = thay(dung, "2. Tên cơ quan: UBND XÃ GIA KIỆM", "2. Tên cơ quan: {CoQuanHoa}");
  dung = thay(dung, '"ỦY BAN NHÂN DÂN")} XÃ GIA KIỆM', '"ỦY BAN NHÂN DÂN")} {TenXaHoa}');
  dung = thay(dung, '"HỘI ĐỒNG NHÂN DÂN XÃ GIA KIỆM QUYẾT NGHỊ:"', '$"HỘI ĐỒNG NHÂN DÂN {TenXaHoa} QUYẾT NGHỊ:"');

  [a, b] = doan(...MOC.nhan);
  let nhan = src.slice(a, b);
  nhan = thay(nhan, "    private string GetFieldLabel(string field)", "    public static string GetFieldLabel(string field)");
  nhan = thay(nhan, "    private static string FormatFieldName(string field)", "    public static string FormatFieldName(string field)");

  const than = heThong + dung + nhan;
  if (/Gia Kiệm|GIA KIỆM|Thống Nhất|THỐNG NHẤT|Đồng Nai|ĐỒNG NAI|_selectedTemplate/.test(than)) {
    throw new Error("Còn sót địa phương ghi cứng hoặc _selectedTemplate trong phần chép");
  }
  const lop = (DAU + than).replace(/\s+$/, "") + "\n}\n";
  writeFileSync(LOP, BOM + lop.replace(/\n/g, NL), "utf8");
  console.log(`Đã ghi ${LOP}`);
}

function suaHopThoai() {
  if (!existsSync(LOP)) throw new Error("Chưa có ComposePromptBuilder.cs (chạy tao-lop trước)");
  const vung = [MOC.dung, MOC.heThong, MOC.nhan].map(([tu, den]) => [...doan(tu, den), tu]).sort((x, y) => y[0] - x[0]);
  const THAY = {
    [MOC.nhan[0]]:
      "    private string GetFieldLabel(string field) => ComposePromptBuilder.GetFieldLabel(field);\n\n" +
      "    private static string FormatFieldName(string field) => ComposePromptBuilder.FormatFieldName(field);\n\n",
    [MOC.heThong[0]]:
      "    // Prompt soạn thảo nằm ở AIVanBan.Core (ComposePromptBuilder). Địa phương Gia Kiệm giữ tới khi đợt 3 nối thiết lập cơ quan.\n" +
      "    private string BuildSystemInstruction() =>\n" +
      "        ComposePromptBuilder.BuildSystemInstruction(_selectedTemplate?.Type ?? DocumentType.CongVan, ComposeLocality.GiaKiem);\n\n",
    [MOC.dung[0]]:
      "    private string BuildStructuredPrompt(DocumentType templateType, Dictionary<string, string> fields) =>\n" +
      "        ComposePromptBuilder.BuildUserPrompt(templateType, _selectedTemplate?.Name, fields, ComposeLocality.GiaKiem);\n\n",
  };
  let s = src;
  for (const [a, b, tu] of vung) s = s.slice(0, a) + THAY[tu] + s.slice(b);
  writeFileSync(HOP_THOAI, BOM + s.replace(/\n/g, NL), "utf8");
  console.log(`Đã sửa ${HOP_THOAI}`);
}

const lenh = process.argv[2];
if (lenh === "tao-lop") taoLop();
else if (lenh === "sua-hop-thoai") suaHopThoai();
else { console.error("Dùng: node tach-soan-thao.mjs tao-lop | sua-hop-thoai"); process.exit(2); }
```

Lưu ý: trong script, `"${VACH}"` nằm bên trong template literal `DAU` nên được thay bằng 39 ký tự «═». Kiểm sau khi sinh: dòng `private const string VACH = "═…";` có đúng 39 ký tự giữa hai dấu nháy.

- [ ] **Step 2: Đổi kiểu địa phương trong các ca + thêm bài so lớp mới (bài hỏng)**

Trong `AIVanBan.Tests/MauSoanThao.cs`:
- thêm `using AIVanBan.Core.Services;` dưới `using AIVanBan.Core.Models;`;
- trong khai báo `record Ca`, đổi `object? DiaPhuong` thành `ComposeLocality? DiaPhuong`;
- trong hàm `G`, đổi `T(kv), "GiaKiem")` thành `T(kv), ComposeLocality.GiaKiem)`.

Thêm vào lớp `SoanThaoTuongDuongTests` (sau `HopThoaiKhopBanChup`), cùng `using AIVanBan.Core.Services;` ở đầu tệp:

```csharp
    [Fact]
    public void LopMoiKhopBanChup()
    {
        var chup = DocChup();
        Assert.Equal(chup.Select(c => c.ten), MauSoanThao.GiaKiem.Select(c => c.Ten));
        foreach (var (ca, c) in MauSoanThao.GiaKiem.Zip(chup))
        {
            var h = ChuanHoa(ComposePromptBuilder.BuildSystemInstruction(ca.Loai, ca.DiaPhuong));
            var n = ChuanHoa(ComposePromptBuilder.BuildUserPrompt(ca.Loai, ca.TenMau, ca.TuDien(), ca.DiaPhuong));
            Assert.True(c.he_thong == h, $"{ca.Ten}: he_thong của lớp mới lệch hộp thoại cũ");
            Assert.True(c.nguoi_dung == n, $"{ca.Ten}: nguoi_dung của lớp mới lệch hộp thoại cũ");
        }
    }
```

Run: `dotnet build AIVanBan.Tests`
Expected: FAIL biên dịch (`ComposeLocality`, `ComposePromptBuilder` chưa có).

- [ ] **Step 3: Sinh lớp**

```bash
cd /d/AIVanBanCaNhan && node .superpowers/tam/tach-soan-thao.mjs tao-lop && grep -c "private string Build" AIVanBan.Core/Services/ComposePromptBuilder.cs
```
Expected: `Đã ghi …ComposePromptBuilder.cs`; đếm được **27** (`BuildSystemInstructionCore`, `BuildStructuredPrompt` và 25 hàm dựng theo loại). Script báo «xuất hiện N lần, cần M» thì dừng: tệp hộp thoại đã khác lúc lập kế hoạch. Mở đúng chỗ đó, sửa chuỗi trong script cho khớp văn bản thật (vẫn chỉ thay phần địa phương / tên mẫu), chạy lại.

- [ ] **Step 4: Build + so tương đương**

```bash
dotnet build AIVanBan.Core && dotnet test AIVanBan.Tests --filter "FullyQualifiedName~SoanThaoTuongDuongTests"
```
Expected: `Passed! … 2` (hộp thoại cũ vẫn khớp bản chụp, lớp mới khớp bản chụp ở cả 42 ca). Lệch thì sửa `ComposePromptBuilder.cs` (khối địa phương hoặc chỗ thay); **không** sửa bản chụp.

- [ ] **Step 5: Commit (repo gốc)**

```bash
git add AIVanBan.Core/Services/ComposePromptBuilder.cs AIVanBan.Tests/MauSoanThao.cs AIVanBan.Tests/SoanThaoTuongDuongTests.cs && git commit -F - <<'EOF'
refactor(core): tach prompt soan thao sang ComposePromptBuilder (dau vao thuan, dia phuong la tham so), khop hop thoai cu 42 ca

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
```

---

### Task 7: C#: hộp thoại gọi lớp mới, xóa 26 hàm prompt

**Files:**
- Modify: `AIVanBan.Desktop/Views/AIComposeDialog.xaml.cs` (bằng script, chế độ `sua-hop-thoai`)

**Interfaces:**
- Consumes: `ComposePromptBuilder`, `ComposeLocality.GiaKiem` (Task 6).
- Produces: hộp thoại giữ nguyên các hàm riêng `GetFieldLabel(string)`, `FormatFieldName(string)`, `BuildSystemInstruction()`, `BuildStructuredPrompt(DocumentType, Dictionary<string,string>)` (cùng chữ ký, nay chỉ chuyển lời gọi), nên `Generate_Click`, `CreateInputFields`, `GetFieldHint`… không đổi. Bài `HopThoaiKhopBanChup` tiếp tục kiểm hộp thoại.

- [ ] **Step 1: Sửa hộp thoại**

```bash
cd /d/AIVanBanCaNhan && node .superpowers/tam/tach-soan-thao.mjs sua-hop-thoai && git diff --stat -- AIVanBan.Desktop/Views/AIComposeDialog.xaml.cs
```
Expected: `Đã sửa …`; diff khoảng `+13 / −750` dòng.

```bash
grep -n "Gia Kiệm\|GIA KIỆM" AIVanBan.Desktop/Views/AIComposeDialog.xaml.cs | head -20
```
Expected: chỉ còn dữ liệu mẫu «Tải mẫu» (`GetScenarioSamples`), `[Địa danh]` → «Gia Kiệm» trong xem trước mẫu, `Location = "Gia Kiệm"` khi lưu và chú thích phân tích nội dung. Đây không phải prompt và để `dot3` xử lý cùng thiết lập cơ quan. Không còn dòng nào nằm trong prompt.

- [ ] **Step 2: Build ứng dụng + bộ kiểm**

```bash
dotnet build AIVanBan.Desktop && dotnet test AIVanBan.Tests
```
Expected: build 0 lỗi; toàn bộ bộ kiểm pass, gồm `HopThoaiKhopBanChup` (hộp thoại đã sửa vẫn cho prompt y như bản chụp, tức là đã nối đúng tên mẫu và Gia Kiệm) và `LopMoiKhopBanChup`.

- [ ] **Step 3: Commit (repo gốc)**

```bash
git add AIVanBan.Desktop/Views/AIComposeDialog.xaml.cs && git commit -F - <<'EOF'
refactor(desktop): AIComposeDialog goi ComposePromptBuilder (dia phuong Gia Kiem toi dot 3), bo 26 ham prompt

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
```

---

### Task 8: C#: sinh tệp mẫu `soan-thao.json` (mọi nhánh + không địa phương)

**Files:**
- Modify: `AIVanBan.Tests/MauSoanThao.cs` (thêm `ThemCa`, `TatCa`, `DiaPhuongJson`)
- Modify: `AIVanBan.Tests/MauPromptTests.cs` (thêm một hàm sinh)
- Create (sinh ra): `vanbanplus-api/tests/mau-prompt/soan-thao.json`

**Interfaces:**
- Produces: tệp mẫu là mảng `{ ten, dau_vao: { loai_van_ban, ten_mau?, truong, dia_phuong? }, he_thong, nguoi_dung }`. `truong` giữ thứ tự. `dia_phuong` = `{ xa, co_quan_cap_tren?, tinh, dac_diem? }`. Xuống dòng là `\n`.

- [ ] **Step 1: Thêm ca ngoài Gia Kiệm**

Trong `AIVanBan.Tests/MauSoanThao.cs`, thêm vào cuối lớp `MauSoanThao` (trước dấu `}` cuối):

```csharp
    /// <summary>Ca chỉ có ở tệp mẫu (lớp mới): không địa phương, địa phương khác, không tên mẫu.</summary>
    public static IReadOnlyList<Ca> ThemCa { get; } = new[]
    {
        new Ca("khong-dia-phuong-cong-van", DocumentType.CongVan, "Công văn",
            T("from_org", "UBND xã Minh An", "to_org", "Sở Xây dựng", "subject", "Trả lời kiến nghị", "reply_to_number", "20/SXD-QLN",
              "content", "Đã kiểm tra hiện trạng."), null),
        new Ca("khong-dia-phuong-quyet-dinh", DocumentType.QuyetDinh, "Quyết định", T("person_name", "Phạm Văn Cường"), null),
        new Ca("khong-dia-phuong-ke-hoach", DocumentType.KeHoach, "Kế hoạch", T("period", "năm 2027", "tasks", "Chuyển đổi số."), null),
        new Ca("khong-dia-phuong-thong-bao", DocumentType.ThongBao, "Thông báo", T("content", "Lịch tiếp công dân."), null),
        new Ca("khong-dia-phuong-nghi-quyet-hdnd", DocumentType.NghiQuyet, "Nghị quyết HĐND",
            T("level", "xã", "subject", "Dự toán", "articles", "Điều 1.", "chairman_name", "Nguyễn Thị Lan"), null),
        new Ca("khong-dia-phuong-nghi-quyet-ubnd", DocumentType.NghiQuyet, "Nghị quyết UBND",
            T("level", "xã", "subject", "Quy chế", "articles", "Điều 1.", "chairman_name", "Trần Văn Bình"), null),
        new Ca("khong-dia-phuong-chi-thi", DocumentType.ChiThi, "Chỉ thị", T("subject", "Phòng cháy", "content", "Tăng cường kiểm tra."), null),
        new Ca("dia-phuong-khong-cap-tren", DocumentType.CongVan, "Công văn",
            T("from_org", "UBND phường Bình Minh", "reply_to_number", "20/SXD", "content", "Trả lời kiến nghị."),
            new ComposeLocality("phường Bình Minh", null, "tỉnh Lâm Đồng")),
        new Ca("dia-phuong-co-dac-diem", DocumentType.KeHoach, "Kế hoạch", T("period", "năm 2027"),
            new ComposeLocality("xã Minh An", "thành phố Minh Châu", "tỉnh Lâm Đồng", new[] { "Dân số: 12.000 người", "8 thôn" })),
        new Ca("khac-khong-ten-mau", DocumentType.Khac, null, T("subject", "Giấy xác nhận"), ComposeLocality.GiaKiem),
    };

    public static IReadOnlyList<Ca> TatCa { get; } = GiaKiem.Concat(ThemCa).ToList();

    public static Dictionary<string, object> DiaPhuongJson(ComposeLocality dp)
    {
        var d = new Dictionary<string, object> { ["xa"] = dp.Xa };
        if (dp.CoQuanCapTren != null) d["co_quan_cap_tren"] = dp.CoQuanCapTren;
        d["tinh"] = dp.Tinh;
        if (dp.DacDiem != null) d["dac_diem"] = dp.DacDiem;
        return d;
    }
```

- [ ] **Step 2: Hàm sinh trong `MauPromptTests`**

Trong `AIVanBan.Tests/MauPromptTests.cs`: thêm `using AIVanBan.Core.Models;` cạnh các `using` hiện có (nếu chưa có). Thêm hàm sau vào **cuối lớp** `MauPromptTests` (ngay trước dấu `}` đóng lớp, sau mọi hàm sinh hiện có của tom-tat và `dot2a`):

```csharp
    [Fact]
    public void GhiMauSoanThao()
    {
        var thieu = Enum.GetValues<DocumentType>().Where(t => MauSoanThao.TatCa.All(c => c.Loai != t)).ToList();
        Assert.True(thieu.Count == 0, $"Tệp mẫu soan-thao thiếu loại: {string.Join(", ", thieu)}");

        var mau = MauSoanThao.TatCa.Select(ca =>
        {
            var dauVao = new Dictionary<string, object> { ["loai_van_ban"] = ca.Loai.ToString() };
            if (ca.TenMau != null) dauVao["ten_mau"] = ca.TenMau;
            dauVao["truong"] = ca.TuDien();
            if (ca.DiaPhuong != null) dauVao["dia_phuong"] = MauSoanThao.DiaPhuongJson(ca.DiaPhuong);
            return new
            {
                ten = ca.Ten,
                dau_vao = dauVao,
                he_thong = ChuanHoa(ComposePromptBuilder.BuildSystemInstruction(ca.Loai, ca.DiaPhuong)),
                nguoi_dung = ChuanHoa(ComposePromptBuilder.BuildUserPrompt(ca.Loai, ca.TenMau, ca.TuDien(), ca.DiaPhuong)),
            };
        }).ToList();

        Ghi("soan-thao.json", mau);
    }
```

- [ ] **Step 3: Sinh (lần 1 hỏng có chủ ý, lần 2 qua)**

```bash
cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests --filter "FullyQualifiedName~MauPromptTests.GhiMauSoanThao"; dotnet test AIVanBan.Tests --filter "FullyQualifiedName~MauPromptTests"
```
Expected: lần 1 FAIL «Tệp mẫu … vừa được sinh lại…»; lần 2 `Passed!` (mọi hàm sinh, kể cả tom-tat và `dot2a`, không đổi tệp nào khác). Kiểm nhanh:

```bash
node -e 'const m=require("./vanbanplus-api/tests/mau-prompt/soan-thao.json"); console.log(m.length, new Set(m.map(x=>x.dau_vao.loai_van_ban)).size, m.filter(x=>!x.dau_vao.dia_phuong).every(x=>!/Gia Kiệm|GIA KIỆM|Thống Nhất|Đồng Nai/.test(x.he_thong+x.nguoi_dung)))'
```
Expected: `52 33 true`.

- [ ] **Step 4: Commit (repo gốc, chỉ C#; tệp mẫu commit ở Task 9 bên vanbanplus-api)**

```bash
git add AIVanBan.Tests/MauSoanThao.cs AIVanBan.Tests/MauPromptTests.cs && git commit -F - <<'EOF'
test: sinh tep mau prompt soan-thao cho vanbanplus-api (33 loai, moi nhanh, co va khong dia phuong)

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
```

---

### Task 9: Tác vụ `soan-thao` trên máy chủ

**Files:**
- Create (một lần, không commit): `D:/AIVanBanCaNhan/.superpowers/tam/sinh-nhan-truong.mjs`
- Create (sinh ra): `vanbanplus-api/src/lib/tac-vu/soan-thao/nhan-truong.ts`
- Create: `vanbanplus-api/src/lib/tac-vu/soan-thao/prompt.ts`
- Create: `vanbanplus-api/src/lib/tac-vu/soan-thao/dinh-nghia.ts`
- Modify: `vanbanplus-api/src/lib/tac-vu/dang-ky.ts` (thêm import + một mục)
- Test: `vanbanplus-api/tests/tac-vu-soan-thao.test.ts`; commit kèm `tests/mau-prompt/soan-thao.json` (Task 8)

**Interfaces:**
- Consumes: `PromptTacVu`, `DinhNghiaTacVu` (Task 2), tệp mẫu `soan-thao.json` (Task 8), dòng giá `soan-thao` của `008` (Task 4).
- Produces:
  - `LOAI_VAN_BAN` (33 tên), `type LoaiVanBan`
  - `interface DiaPhuong { xa: string; co_quan_cap_tren: string | null; tinh: string; dac_diem: string[] | null }`
  - `interface DauVaoSoanThao { loai_van_ban: LoaiVanBan; ten_mau: string | null; truong: Array<[string, string]>; dia_phuong: DiaPhuong | null }`
  - `tenDiaDanh(xa)`, `dinhDangTenTruong(k)`, `nhanTruong(k)`, `heThongSoanThao(loai, dp)`, `nguoiDungSoanThao(d)`, `dungPromptSoanThao(d): PromptTacVu`
  - `soanThao: DinhNghiaTacVu<DauVaoSoanThao>`, `NHAN_TRUONG: ReadonlyMap<string, string>`

- [ ] **Step 1: Sinh bảng nhãn trường từ C#**

`D:/AIVanBanCaNhan/.superpowers/tam/sinh-nhan-truong.mjs`:

```js
// Sinh vanbanplus-api/src/lib/tac-vu/soan-thao/nhan-truong.ts từ ComposePromptBuilder.GetFieldLabel (159 nhãn, gồm biểu tượng).
import { readFileSync, writeFileSync } from "node:fs";

const cs = readFileSync("D:/AIVanBanCaNhan/AIVanBan.Core/Services/ComposePromptBuilder.cs", "utf8").replace(/\r\n/g, "\n");
const a = cs.indexOf("public static string GetFieldLabel(string field)");
const b = cs.indexOf("public static string FormatFieldName(string field)", a);
if (a < 0 || b < 0) throw new Error("Không thấy GetFieldLabel / FormatFieldName");
const cap = [...cs.slice(a, b).matchAll(/^\s*"([^"\\]+)" => "([^"\\]*)",\s*$/gm)].map((m) => [m[1], m[2]]);
if (new Set(cap.map(([k]) => k)).size !== cap.length) throw new Error("Khóa trùng");
if (cap.length !== 159) throw new Error(`Đọc được ${cap.length} nhãn, cần 159: GetFieldLabel đã đổi, xem lại`);

const dong = cap.map(([k, v]) => `  [${JSON.stringify(k)}, ${JSON.stringify(v)}],`).join("\n");
writeFileSync(
  "D:/AIVanBanCaNhan/vanbanplus-api/src/lib/tac-vu/soan-thao/nhan-truong.ts",
  `// Sinh tự động từ AIVanBan.Core/Services/ComposePromptBuilder.cs (GetFieldLabel) bằng .superpowers/tam/sinh-nhan-truong.mjs.
// Không sửa tay: tệp mẫu tests/mau-prompt/soan-thao.json bắt mọi chỗ lệch.

const DS: ReadonlyArray<readonly [string, string]> = [
${dong}
];

/** Nhãn ô nhập (có biểu tượng) mà nhánh soạn chung ghi vào prompt. Map, không dùng object, để khóa lạ không chạm prototype. */
export const NHAN_TRUONG: ReadonlyMap<string, string> = new Map(DS);
`
);
console.log("Đã ghi", cap.length, "nhãn");
```

Run: `cd /d/AIVanBanCaNhan && mkdir -p vanbanplus-api/src/lib/tac-vu/soan-thao && node .superpowers/tam/sinh-nhan-truong.mjs`
Expected: `Đã ghi 159 nhãn`.

- [ ] **Step 2: Viết bộ kiểm hỏng**

`vanbanplus-api/tests/tac-vu-soan-thao.test.ts`:

```ts
import { test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { soanThao } from "../src/lib/tac-vu/soan-thao/dinh-nghia";
import { LOAI_VAN_BAN, dinhDangTenTruong, nhanTruong, tenDiaDanh } from "../src/lib/tac-vu/soan-thao/prompt";
import { layTacVu } from "../src/lib/tac-vu/dang-ky";
import { uocTinhTacVu, type PhuThuoc } from "../src/lib/tac-vu/chay-tac-vu";
import type { BangGia, CauHinhTacVu } from "../src/lib/credit/uoc-tinh";
import type { KhoCredit } from "../src/lib/credit/so-du";
import type { ApiUser } from "../src/types";

interface Mau {
  ten: string;
  dau_vao: { loai_van_ban: string; ten_mau?: string; truong: Record<string, string>; dia_phuong?: Record<string, unknown> };
  he_thong: string;
  nguoi_dung: string;
}
const MAU: Mau[] = JSON.parse(readFileSync(join(__dirname, "mau-prompt", "soan-thao.json"), "utf8"));

test("prompt soạn thảo khớp từng ký tự với bản C# (33 loại, mọi nhánh, có và không có địa phương)", () => {
  assert.ok(MAU.length >= 50);
  const coLoai = new Set(MAU.map((m) => m.dau_vao.loai_van_ban));
  for (const l of LOAI_VAN_BAN) assert.ok(coLoai.has(l), `tệp mẫu thiếu loại ${l}`);
  for (const m of MAU) {
    const dv = soanThao.kiemDauVao(m.dau_vao);
    assert.ok(dv.ok, `${m.ten}: đầu vào mẫu phải hợp lệ (${!dv.ok ? dv.loi : ""})`);
    const p = soanThao.dungPrompt(dv.du_lieu, new Date());
    assert.equal(p.he_thong, m.he_thong, `${m.ten}: he_thong`);
    assert.equal(p.nguoi_dung, m.nguoi_dung, `${m.ten}: nguoi_dung`);
  }
});

test("không gửi dia_phuong: prompt không nêu Gia Kiệm / Thống Nhất / Đồng Nai, không có khối bối cảnh địa phương", () => {
  const khong = MAU.filter((m) => !m.dau_vao.dia_phuong);
  assert.ok(khong.length >= 7);
  for (const m of khong) {
    assert.doesNotMatch(m.he_thong + m.nguoi_dung, /Gia Kiệm|GIA KIỆM|Thống Nhất|THỐNG NHẤT|Đồng Nai|BỐI CẢNH ĐỊA PHƯƠNG/, m.ten);
  }
});

test("đầu vào soan-thao: loại đúng tên enum, trường là chuỗi, địa phương đủ xã + tỉnh", () => {
  const sai = (b: unknown) => assert.equal(soanThao.kiemDauVao(b).ok, false, JSON.stringify(b));
  sai({});
  sai(null);
  sai({ loai_van_ban: "congvan", truong: {} });
  sai({ loai_van_ban: "toString", truong: {} });
  sai({ loai_van_ban: "CongVan" });
  sai({ loai_van_ban: "CongVan", truong: [] });
  sai({ loai_van_ban: "CongVan", truong: { content: 5 } });
  sai({ loai_van_ban: "CongVan", truong: { "1abc": "x" } });
  sai({ loai_van_ban: "CongVan", truong: JSON.parse('{"__proto__":"x"}') });
  sai({ loai_van_ban: "CongVan", truong: {}, ten_mau: 3 });
  sai({ loai_van_ban: "CongVan", truong: {}, dia_phuong: "xã A" });
  sai({ loai_van_ban: "CongVan", truong: {}, dia_phuong: { xa: "", tinh: "tỉnh B" } });
  sai({ loai_van_ban: "CongVan", truong: {}, dia_phuong: { xa: "xã A", tinh: "tỉnh B", dac_diem: "x" } });
  sai({ loai_van_ban: "CongVan", truong: {}, dia_phuong: { xa: "xã A", tinh: "tỉnh B", dac_diem: Array(11).fill("x") } });
  assert.deepEqual(soanThao.kiemDauVao({ loai_van_ban: "CongVan", truong: { subject: "a", content: "b" }, ten_mau: "" }), {
    ok: true,
    du_lieu: { loai_van_ban: "CongVan", ten_mau: null, truong: [["subject", "a"], ["content", "b"]], dia_phuong: null },
  });
  assert.deepEqual(
    soanThao.kiemDauVao({ loai_van_ban: "Khac", truong: {}, dia_phuong: { xa: "xã A", co_quan_cap_tren: "  ", tinh: "tỉnh B" } }),
    { ok: true, du_lieu: { loai_van_ban: "Khac", ten_mau: null, truong: [], dia_phuong: { xa: "xã A", co_quan_cap_tren: null, tinh: "tỉnh B", dac_diem: null } } }
  );
});

test("hàm phụ: địa danh, tên trường dự phòng, nhãn trường", () => {
  assert.equal(tenDiaDanh("xã Gia Kiệm"), "Gia Kiệm");
  assert.equal(tenDiaDanh("Phường Bình Minh"), "Bình Minh");
  assert.equal(tenDiaDanh("thị trấn Dầu Giây"), "Dầu Giây");
  assert.equal(tenDiaDanh("Hà Nội"), "Hà Nội");
  assert.equal(dinhDangTenTruong("so_ho_dan"), "So Ho Dan");
  assert.equal(dinhDangTenTruong("a__b"), "A  B");
  assert.equal(nhanTruong("subject"), "📋 Vấn đề/Tiêu đề");
  assert.equal(nhanTruong("constructor"), "📝 Constructor");
});

test("định nghĩa: kiểu văn bản, nhiệt độ 0,7 như /api/ai/generate; có trong danh sách tác vụ", () => {
  assert.equal(soanThao.ten, "soan-thao");
  assert.equal(soanThao.kieu_ket_qua, "van_ban");
  assert.equal(soanThao.nhiet_do, 0.7);
  assert.equal(layTacVu("soan-thao"), soanThao);
});

const SQL_008 = readFileSync(join(__dirname, "..", "supabase", "migrations", "008_bang_gia_dot2b.sql"), "utf8");
const giaSoanThao = (): CauHinhTacVu => JSON.parse(SQL_008.match(/'tac_vu',\s*'soan-thao',\s*'(\{[^']*\})'/)![1]);

test("hiệu chỉnh theo bảng giá 008: công văn điển hình 4–6 credit tối thiểu, giữ tạm không quá 22 (spec mục 7: 4–13)", async () => {
  const bg: BangGia = {
    chung: { gia_tri_credit_vnd: 100, ty_gia_vnd_usd: 26000, mo_hinh_mac_dinh: "gemini-3.6-flash" },
    mo_hinh: { "gemini-3.6-flash": { vao_usd_trieu: 1.5, ra_usd_trieu: 7.5 } },
    tac_vu: { "soan-thao": giaSoanThao() },
  };
  const phu: PhuThuoc = {
    bangGia: async () => bg,
    kho: { soDu: async () => 1000 } as unknown as KhoCredit,
    layKhoa: async () => null,
    goiAI: async () => { throw new Error("không gọi AI khi ước tính"); },
    bayGio: () => new Date(),
  };
  const mau = MAU.find((m) => m.ten === "cong-van-day-du")!;
  const kq = await uocTinhTacVu(soanThao, mau.dau_vao, { id: "U1" } as ApiUser, phu);
  assert.ok(kq.ok);
  assert.ok(kq.credit_min >= 4 && kq.credit_min <= 6, `credit_min ${kq.credit_min}`);
  assert.ok(kq.credit_max <= 22, `credit_max ${kq.credit_max}`);
});
```

Run: `cd /d/AIVanBanCaNhan/vanbanplus-api && npx tsx --test tests/tac-vu-soan-thao.test.ts`
Expected: FAIL (không có `soan-thao/dinh-nghia`).

- [ ] **Step 3: Viết `prompt.ts`**

`src/lib/tac-vu/soan-thao/prompt.ts` (chép nguyên văn từ `ComposePromptBuilder`: `""` → `"`; backtick thoát bằng `` \` ``; `\\n` của chuỗi `@"..."` C# là HAI dấu gạch ngược + n, trong template TS viết `\\\\n`):

```ts
import type { PromptTacVu } from "../chay-tac-vu";
import { NHAN_TRUONG } from "./nhan-truong";

/** Tên enum DocumentType của bản cài máy (AIVanBan.Core.Models.DocumentType), đúng thứ tự khai báo. */
export const LOAI_VAN_BAN = [
  "Luat", "NghiDinh", "ThongTu", "NghiQuyet", "QuyetDinh", "ChiThi", "QuyChE", "QuyDinh", "ThongCao", "ThongBao",
  "HuongDan", "ChuongTrinh", "KeHoach", "PhuongAn", "DeAn", "DuAn", "BaoCao", "BienBan", "ToTrinh", "HopDong",
  "CongVan", "CongDien", "BanGhiNho", "BanThoaThuan", "GiayUyQuyen", "GiayMoi", "GiayGioiThieu", "GiayNghiPhep",
  "PhieuGui", "PhieuChuyen", "PhieuBao", "ThuCong", "Khac",
] as const;
export type LoaiVanBan = (typeof LOAI_VAN_BAN)[number];

export interface DiaPhuong {
  xa: string;
  /** null: không có cấp trung gian, cấp trên là tỉnh. */
  co_quan_cap_tren: string | null;
  tinh: string;
  dac_diem: string[] | null;
}

export interface DauVaoSoanThao {
  loai_van_ban: LoaiVanBan;
  /** Tên mẫu văn bản (C# _selectedTemplate.Name). null: chưa chọn mẫu. */
  ten_mau: string | null;
  /** Đúng thứ tự gửi lên; nhánh soạn chung liệt kê theo thứ tự này. */
  truong: Array<[string, string]>;
  dia_phuong: DiaPhuong | null;
}

type Lay = (khoa: string) => string;

const VACH = "═".repeat(39);

/** DocumentType.GetDisplayName() của các loại có dùng tên trong prompt. */
const TEN_HIEN_THI: Partial<Record<LoaiVanBan, string>> = {
  PhuongAn: "Phương án",
  DeAn: "Đề án",
  DuAn: "Dự án",
  BanGhiNho: "Bản ghi nhớ",
  BanThoaThuan: "Bản thỏa thuận",
  PhieuGui: "Phiếu gửi",
  PhieuChuyen: "Phiếu chuyển",
  PhieuBao: "Phiếu báo",
};

/** C# ComposeLocality.TenDiaDanh. */
export function tenDiaDanh(xa: string): string {
  const thuong = xa.toLowerCase();
  for (const p of ["xã ", "phường ", "thị trấn ", "đặc khu "]) if (thuong.startsWith(p)) return xa.slice(p.length).trim();
  return xa.trim();
}

/** C# FormatFieldName: «school_name» → «School Name». */
export function dinhDangTenTruong(k: string): string {
  return k.split("_").map((w) => (w.length > 0 ? w[0].toUpperCase() + w.slice(1) : w)).join(" ");
}

/** C# GetFieldLabel. */
export function nhanTruong(k: string): string {
  return NHAN_TRUONG.get(k) ?? `📝 ${dinhDangTenTruong(k)}`;
}

/** Ngữ cảnh địa phương + tên mẫu; đúng các thuộc tính cùng tên của ComposePromptBuilder. */
class NguCanh {
  constructor(readonly tenMau: string | null, readonly dp: DiaPhuong | null) {}
  get coQuanThuong() { return this.dp ? `UBND ${this.dp.xa}` : "[cơ quan ban hành]"; }
  get coQuanHoa() { return this.dp ? `UBND ${this.dp.xa.toUpperCase()}` : "[CƠ QUAN BAN HÀNH]"; }
  get capTrenHoa() { return this.dp ? `UBND ${(this.dp.co_quan_cap_tren ?? this.dp.tinh).toUpperCase()}` : "[CƠ QUAN CẤP TRÊN]"; }
  get tenXaHoa() { return this.dp ? this.dp.xa.toUpperCase() : "[TÊN XÃ/PHƯỜNG]"; }
  get diaDanh() { return this.dp ? tenDiaDanh(this.dp.xa) : "[Địa danh]"; }
  get tenCoQuanQuyetDinh() { return this.dp ? ` (UBND ${this.dp.xa.toUpperCase()})` : ""; }
  get khoiDiaPhuong() {
    const dp = this.dp;
    if (!dp) return "";
    const capTren = dp.co_quan_cap_tren;
    let s = `${VACH}\nBỐI CẢNH ĐỊA PHƯƠNG (${dp.xa}):\n${VACH}\n`;
    s += `- ${dp.xa[0].toUpperCase()}${dp.xa.slice(1)}, ${capTren ? `${capTren}, ` : ""}${dp.tinh}\n`;
    for (const d of dp.dac_diem ?? []) s += `- ${d}\n`;
    s += `- Cơ quan cấp trên: ${capTren ? `UBND ${capTren}, ` : ""}UBND ${dp.tinh}\n\n`;
    return s;
  }
}

const dong = (get: Lay, k: string, nhan: string) => (get(k) === "" ? "" : `- ${nhan}: ${get(k)}`);

export function heThongSoanThao(loai: LoaiVanBan, dp: DiaPhuong | null): string {
  const c = new NguCanh(null, dp);
  return `Bạn là CHUYÊN VIÊN VĂN THƯ CAO CẤP tại UBND cấp xã/phường Việt Nam với 20 năm kinh nghiệm soạn thảo văn bản hành chính. Bạn nắm vững:
- Luật Ban hành VBQPPL năm 2015 (sửa đổi 2020)
- Nghị định 30/2020/NĐ-CP ngày 05/3/2020 về công tác văn thư
- Nghị định 154/2020/NĐ-CP sửa đổi NĐ 34/2016 về VBQPPL
- Thông tư 01/2011/TT-BNV hướng dẫn thể thức và kỹ thuật trình bày văn bản hành chính
- Quy trình soạn thảo, trình ký, ban hành văn bản tại UBND cấp xã

NHIỆM VỤ: Soạn thảo văn bản hành chính HOÀN CHỈNH, ĐÚNG THỂ THỨC, sẵn sàng in ấn và ban hành.

${VACH}
QUY TẮC THỂ THỨC (Theo NĐ 30/2020):
${VACH}

1. QUỐC HIỆU VÀ TIÊU NGỮ (bắt buộc):
   CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
   Độc lập - Tự do - Hạnh phúc
   (Tiêu ngữ có gạch ngang ở giữa, có gạch liền phía dưới)

2. TÊN CƠ QUAN BAN HÀNH:
   - Cơ quan cấp trên (nếu có): ${c.capTrenHoa}
   - Cơ quan ban hành: ${c.coQuanHoa} (viết hoa, in đậm)
   - Giữa hai dòng có gạch ngang

3. SỐ VÀ KÝ HIỆU (Phụ lục III, NĐ 30/2020 — đầy đủ 29 loại VB hành chính):
   - Nghị quyết: Số:    /NQ-HĐND hoặc /NQ-UBND
   - Quyết định: Số:    /QĐ-UBND
   - Chỉ thị: Số:    /CT-UBND
   - Quy chế: Số:    /QC-UBND
   - Quy định: Số:    /QyĐ-UBND
   - Thông cáo: Số:    /TC-UBND
   - Thông báo: Số:    /TB-UBND
   - Hướng dẫn: Số:    /HD-UBND
   - Chương trình: Số:    /CTr-UBND
   - Kế hoạch: Số:    /KH-UBND
   - Phương án: Số:    /PA-UBND
   - Đề án: Số:    /ĐA-UBND
   - Dự án: Số:    /DA-UBND
   - Báo cáo: Số:    /BC-UBND
   - Biên bản: (không đánh số ký hiệu)
   - Tờ trình: Số:    /TTr-UBND
   - Hợp đồng: Số:    /HĐ-UBND
   - Công văn: Số:    /CV-UBND
   - Công điện: Số:    /CĐ-UBND
   - Bản ghi nhớ: (không đánh số ký hiệu)
   - Bản thỏa thuận: (không đánh số ký hiệu)
   - Giấy ủy quyền: Số:    /GUQ-UBND
   - Giấy mời: Số:    /GM-UBND
   - Giấy giới thiệu: Số:    /GGT-UBND
   - Giấy nghỉ phép: Số:    /GNP-UBND
   - Phiếu gửi: Số:    /PG-UBND
   - Phiếu chuyển: Số:    /PC-UBND
   - Phiếu báo: Số:    /PB-UBND
   - Thư công: (không đánh số ký hiệu)

4. ĐỊA DANH VÀ NGÀY THÁNG:
   "${c.diaDanh}, ngày ... tháng ... năm 2026"

5. TRÍCH YẾU NỘI DUNG (V/v):
   - Công văn: "V/v [nội dung]"
   - QĐ/NQ: "QUYẾT ĐỊNH / NGHỊ QUYẾT" + "Về việc [nội dung]"
   - Báo cáo: "BÁO CÁO" + "[Về nội dung / Kết quả...]"

6. NỘI DUNG VĂN BẢN:
   - Công văn: Câu dẫn → Nội dung chính → Đề nghị → Kết
   - QĐ: Căn cứ → QUYẾT ĐỊNH: Điều 1, 2, 3...
   - Báo cáo: Phần I (Kết quả) → Phần II (Tồn tại) → Phần III (Phương hướng)
   - Tờ trình: Căn cứ → Lý do → Nội dung đề xuất → Kinh phí → Kiến nghị
   - Kế hoạch: Mục đích → Yêu cầu → Nội dung → Tổ chức thực hiện → Kinh phí
   - Thông báo: Nội dung thông báo → Thời gian → Địa điểm → Yêu cầu
   - Nghị quyết: Căn cứ → Điều khoản

7. NƠI NHẬN:
   "Nơi nhận:" (in nghiêng, in đậm)
   - Như trên;
   - [Các đơn vị liên quan];
   - Lưu: VT, [bộ phận].

8. CHỮ KÝ:
   [CHỨC DANH IN HOA]
   (Chữ ký, đóng dấu)
   [Họ và tên]

${VACH}
QUY TẮC VIẾT:
${VACH}

- VĂN PHONG: Hành chính chuẩn, trang trọng, mạch lạc, không dùng khẩu ngữ
- NGÔI THỨ: Ngôi thứ ba ("UBND xã", "Chủ tịch UBND"), không dùng "tôi", "chúng tôi"
- CÂU CHỮ: Ngắn gọn, rõ ràng, chính xác, không mơ hồ
- SỐ LIỆU: Ghi kèm đơn vị, viết bằng số + chữ nếu là tiền/diện tích quan trọng
- VIỆN DẪN: Ghi đầy đủ số hiệu VB (VD: "Theo Quyết định số 15/QĐ-UBND ngày 10/01/2026")
- PLAIN TEXT: KHÔNG dùng markdown (**, *, #, \`\`\`), KHÔNG dùng emoji
- XUỐNG DÒNG: Bình thường, KHÔNG viết literal \\\\n
- Gạch đầu dòng dùng dấu "-"
- TRÌNH BÀY: Sạch sẽ, có thể in trực tiếp lên giấy A4

${c.khoiDiaPhuong}Hãy soạn văn bản loại: ${loai}`;
}

function congVan(get: Lay, c: NguCanh): string {
  let prompt = `Soạn CÔNG VĂN hoàn chỉnh, đúng thể thức NĐ 30/2020:

THÔNG TIN:
- Cơ quan ban hành: ${get("from_org")}
- Nơi nhận chính: ${get("to_org") === "" ? get("to_department") : get("to_org")}
- Vấn đề (V/v): ${get("subject")}
- Người ký: ${get("signer_name") === "" ? get("chairman_name") : get("signer_name")}
- Chức danh: ${get("signer_title")}

NỘI DUNG CHÍNH CẦN ĐƯA VÀO:
${get("content")}`;

  if (get("reply_to_number") !== "")
    prompt += `\n\nĐÂY LÀ CÔNG VĂN TRẢ LỜI công văn số: ${get("reply_to_number")}. Mở đầu bằng: "Phúc đáp Công văn số... ngày... của... về việc..., ${c.coQuanThuong} xin trả lời như sau:"`;

  if (get("proposal") !== "") prompt += `\n\nĐỀ XUẤT/KIẾN NGHỊ:\n${get("proposal")}`;

  prompt += `

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ (đầy đủ)
2. Tên cơ quan cấp trên + cơ quan ban hành
3. Số/ký hiệu: Số:    /CV-UBND
4. Địa danh, ngày tháng
5. V/v: [trích yếu]
6. Kính gửi: [nơi nhận]
7. Thân văn bản:
   - Câu dẫn nhập (lý do, căn cứ)
   - Nội dung chính (diễn giải chi tiết, có số liệu nếu cần)
   - Đề nghị/kiến nghị (nêu rõ yêu cầu cụ thể)
   - Câu kết ("Kính đề nghị... xem xét, giải quyết./.")
8. Nơi nhận (liệt kê đầy đủ)
9. Chức danh + tên người ký

VĂN PHONG: Hành chính chuẩn, trang trọng, mạch lạc. Viện dẫn căn cứ pháp lý phù hợp nếu có.`;
  return prompt;
}

function quyetDinh(get: Lay, c: NguCanh): string {
  let prompt = `Soạn QUYẾT ĐỊNH hoàn chỉnh, đúng thể thức NĐ 30/2020:

LOẠI QUYẾT ĐỊNH: ${c.tenMau ?? ""}

THÔNG TIN:`;
  const them = (k: string, nhan: string) => {
    if (get(k) !== "") prompt += `\n- ${nhan}: ${get(k)}`;
  };
  them("person_name", "Đối tượng");
  them("current_position", "Chức vụ hiện tại");
  them("from_unit", "Đơn vị cũ");
  them("to_unit", "Đơn vị mới");
  them("new_position", "Chức vụ mới");
  them("effective_date", "Ngày hiệu lực");
  them("award_type", "Hình thức khen thưởng");
  them("recipient", "Đối tượng khen thưởng");
  them("achievement", "Thành tích");
  them("org_name", "Tên tổ chức");
  them("members", "Thành viên");
  them("tasks", "Nhiệm vụ");
  them("project_name", "Tên đề án/dự án");
  them("objectives", "Mục tiêu/Nội dung");
  them("budget", "Kinh phí");
  them("implementing_unit", "Đơn vị thực hiện");

  const signerName = get("signer_name") === "" ? get("chairman_name") : get("signer_name");
  const signerTitle = get("signer_title");
  if (signerName !== "") prompt += `\n- Người ký: ${signerName}`;
  if (signerTitle !== "") prompt += `\n- Chức danh: ${signerTitle}`;

  prompt += `

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan ban hành${c.tenCoQuanQuyetDinh}
3. Số/ký hiệu: Số:    /QĐ-UBND
4. Tiêu đề: QUYẾT ĐỊNH + "Về việc [nội dung]"
5. Phần CĂN CỨ (bắt buộc):
   - Căn cứ Luật Tổ chức chính quyền địa phương 2015 (sửa đổi 2019)
   - Căn cứ các luật/nghị định chuyên ngành liên quan
   - Căn cứ tờ trình, đề nghị (nếu có)
   - Xét đề nghị của... (nếu có)
6. QUYẾT ĐỊNH:
   - Điều 1: Nội dung chính (chi tiết, cụ thể)
   - Điều 2: Trách nhiệm thi hành, hiệu lực
   - Điều 3: Tổ chức thực hiện, nơi gửi
7. Nơi nhận
8. Chức danh + tên người ký

LƯU Ý: Viện dẫn đúng căn cứ pháp lý. Mỗi Điều phải rõ ràng, cụ thể, có tính bắt buộc thi hành.`;
  return prompt;
}

function baoCao(get: Lay, c: NguCanh): string {
  const khoi = (k: string, tieuDe: string) => (get(k) === "" ? "" : `${tieuDe}:\n${get(k)}\n`);
  return `Soạn BÁO CÁO hoàn chỉnh, đúng thể thức NĐ 30/2020:

LOẠI BÁO CÁO: ${c.tenMau ?? ""}

THÔNG TIN:
- Đơn vị báo cáo: ${get("org_name")}
- Kỳ báo cáo/Lĩnh vực: ${get("period") === "" ? get("field") : get("period")}
- Người ký: ${get("signer_name")}, ${get("signer_title")}

DỮ LIỆU ĐẦU VÀO:
${khoi("achievements", "KẾT QUẢ ĐẠT ĐƯỢC")}
${khoi("situation", "TÌNH HÌNH")}
${khoi("results", "KẾT QUẢ CỤ THỂ")}
${khoi("challenges", "TỒN TẠI, HẠN CHẾ")}
${khoi("future_plans", "PHƯƠNG HƯỚNG")}
${khoi("proposals", "ĐỀ XUẤT, KIẾN NGHỊ")}

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan: ${get("org_name")}
3. Số/ký hiệu: Số:    /BC-UBND
4. Tiêu đề: BÁO CÁO + trích yếu nội dung
5. Nơi gửi (Kính gửi)
6. Câu dẫn nhập: "Thực hiện [căn cứ]..., ${get("org_name")} báo cáo kết quả... như sau:"
7. Thân báo cáo:
   PHẦN I: KẾT QUẢ THỰC HIỆN
   - Chia theo mục, có đánh số I, II, III hoặc 1, 2, 3
   - Mỗi mục: trình bày cụ thể, có số liệu
   - Dùng gạch đầu dòng cho chi tiết

   PHẦN II: TỒN TẠI, HẠN CHẾ VÀ NGUYÊN NHÂN
   - Nêu rõ khó khăn, vướng mắc
   - Phân tích nguyên nhân (chủ quan/khách quan)

   PHẦN III: PHƯƠNG HƯỚNG, NHIỆM VỤ [kỳ tiếp]
   - Nhiệm vụ trọng tâm
   - Giải pháp thực hiện
   - Kiến nghị cấp trên (nếu cần)

8. Câu kết: "Trên đây là báo cáo... Kính đề nghị [cấp trên] xem xét, chỉ đạo./."
9. Nơi nhận
10. Chức danh + tên người ký

LƯU Ý: Số liệu phải cụ thể, rõ ràng. Đánh giá khách quan, trung thực. Kiến nghị phải khả thi, sát thực tế.`;
}

function toTrinh(get: Lay): string {
  return `Soạn TỜ TRÌNH hoàn chỉnh, đúng thể thức NĐ 30/2020:

THÔNG TIN:
- Cơ quan trình: ${get("org_name")}
- Nơi nhận: ${get("recipient")}
- Vấn đề trình: ${get("subject") === "" ? get("proposal") : get("subject")}
- Lý do: ${get("reason")}
- Nội dung đề xuất: ${get("content") === "" ? get("proposal") : get("content")}
${dong(get, "budget", "Kinh phí")}
- Người ký: ${get("signer_name")}, ${get("signer_title")}

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan: ${get("org_name")}
3. Số/ký hiệu: Số:    /TTr-UBND
4. Tiêu đề: TỜ TRÌNH + "Về việc [nội dung]"
5. Kính gửi: ${get("recipient")}
6. Thân tờ trình:
   I. CĂN CỨ, LÝ DO:
   - Căn cứ pháp lý (Luật, NĐ, QĐ liên quan)
   - Tình hình thực tế, sự cần thiết

   II. NỘI DUNG ĐỀ XUẤT:
   - Nội dung cụ thể, chi tiết
   - Phương án thực hiện
   - Nguồn lực cần thiết
   ${get("budget") === "" ? "" : "- Kinh phí dự kiến (ghi rõ nguồn vốn)"}

   III. TỔ CHỨC THỰC HIỆN:
   - Phân công trách nhiệm
   - Tiến độ dự kiến

7. Câu kết: "Kính trình ${get("recipient")} xem xét, phê duyệt./."
8. Nơi nhận
9. Chức danh + tên người ký

LƯU Ý: Lập luận chặt chẽ, viện dẫn căn cứ đầy đủ. Nội dung phải thuyết phục, khả thi.`;
}

function keHoach(get: Lay, c: NguCanh): string {
  const isEvent = get("event_name") !== "" || get("program") !== "";
  const title = isEvent ? get("event_name") : `Công tác ${get("period")}`;
  return `Soạn KẾ HOẠCH hoàn chỉnh, đúng thể thức NĐ 30/2020:

THÔNG TIN:
- Cơ quan ban hành: ${get("org_name") === "" ? c.coQuanThuong : get("org_name")}
- Nội dung: ${title}
${dong(get, "period", "Thời kỳ")}
${dong(get, "objectives", "Mục tiêu")}
${dong(get, "tasks", "Nội dung nhiệm vụ")}
${dong(get, "time_place", "Thời gian, địa điểm")}
${dong(get, "purpose", "Mục đích")}
${dong(get, "program", "Chương trình")}
${dong(get, "budget", "Kinh phí")}
- Người ký: ${get("signer_name")}, ${get("signer_title")}

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan ban hành
3. Số/ký hiệu: Số:    /KH-UBND
4. Tiêu đề: KẾ HOẠCH + trích yếu
5. Thân kế hoạch:

   I. MỤC ĐÍCH, YÊU CẦU:
   1. Mục đích (rõ ràng, cụ thể)
   2. Yêu cầu (khả thi, sát thực tế)

   II. NỘI DUNG:
   - Chi tiết từng nhiệm vụ/hoạt động
   - Thời gian thực hiện cụ thể
   - Đơn vị/cá nhân chịu trách nhiệm

   III. TỔ CHỨC THỰC HIỆN:
   - Phân công nhiệm vụ cho từng bộ phận
   - Chế độ báo cáo, kiểm tra

   IV. KINH PHÍ THỰC HIỆN:
   - Tổng kinh phí, nguồn vốn
   - Phân bổ (nếu có)

6. Câu kết: "Yêu cầu các ban ngành, đoàn thể nghiêm túc triển khai thực hiện./."
7. Nơi nhận
8. Chức danh + tên người ký

LƯU Ý: Nội dung phải cụ thể, có mốc thời gian, có phân công rõ ràng. Tránh chung chung.`;
}

function thongBao(get: Lay, c: NguCanh): string {
  return `Soạn THÔNG BÁO hoàn chỉnh, đúng thể thức NĐ 30/2020:

LOẠI THÔNG BÁO: ${c.tenMau ?? ""}

THÔNG TIN:
${dong(get, "meeting_name", "Nội dung")}
${dong(get, "event_name", "Sự kiện")}
${dong(get, "time", "Thời gian")}
${dong(get, "location", "Địa điểm")}
${dong(get, "participants", "Thành phần")}
${dong(get, "agenda", "Nội dung/Chương trình")}
${dong(get, "content", "Nội dung")}
${dong(get, "conclusion", "Kết luận")}
${dong(get, "tasks", "Nhiệm vụ")}
- Người ký: ${get("signer_name")}, ${get("signer_title")}

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan: ${c.coQuanHoa}
3. Số/ký hiệu: Số:    /TB-UBND
4. Tiêu đề: THÔNG BÁO + trích yếu
5. Thân thông báo:
   - Câu dẫn (lý do, căn cứ)
   - Nội dung chính (rõ ràng, cụ thể)
   - Thời gian, địa điểm (nếu có)
   - Thành phần, đối tượng
   - Yêu cầu (chuẩn bị, lưu ý)
6. Câu kết
7. Nơi nhận
8. Chức danh + tên người ký

LƯU Ý: Thông báo cần ngắn gọn, đầy đủ thông tin, dễ hiểu.`;
}

function nghiQuyet(get: Lay, c: NguCanh): string {
  const level = get("level");
  const isHDND = c.tenMau?.includes("HĐND") === true;
  return `Soạn NGHỊ QUYẾT hoàn chỉnh, đúng thể thức NĐ 30/2020:

LOẠI: Nghị quyết ${isHDND ? "HĐND" : "UBND"} cấp ${level}

THÔNG TIN:
- Chủ đề: ${get("subject")}
- Nội dung các điều: ${get("articles")}
${dong(get, "effective_date", "Ngày hiệu lực")}
${dong(get, "implementing_unit", "Đơn vị thực hiện")}
- Chủ tịch: ${get("chairman_name")}

YÊU CẦU CẤU TRÚC:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan: ${isHDND ? "HỘI ĐỒNG NHÂN DÂN" : "ỦY BAN NHÂN DÂN"} ${c.tenXaHoa}
3. Số/ký hiệu: Số:    /NQ-${isHDND ? "HĐND" : "UBND"}
4. Tiêu đề: NGHỊ QUYẾT + "Về việc [nội dung]"
5. Phần CĂN CỨ (bắt buộc, quan trọng):
   - Căn cứ Luật Tổ chức chính quyền địa phương 2015 (sửa đổi 2019)
   ${isHDND ? "- Căn cứ Luật Hoạt động giám sát của Quốc hội và HĐND 2015" : ""}
   - Căn cứ các luật/nghị định chuyên ngành liên quan
   - Căn cứ tờ trình của UBND (nếu là NQ HĐND)
   - Xét [tình hình thực tế/đề nghị...]
6. ${isHDND ? `HỘI ĐỒNG NHÂN DÂN ${c.tenXaHoa} QUYẾT NGHỊ:` : "QUYẾT NGHỊ:"}
   - Các điều khoản (Điều 1, 2, 3...)
   - Điều cuối: Giao trách nhiệm tổ chức thực hiện
7. Nơi nhận
8. CHỦ TỊCH + tên

LƯU Ý: Nghị quyết phải có tính pháp lý cao, viện dẫn căn cứ đầy đủ, nội dung chặt chẽ, từng điều khoản rõ ràng.`;
}

const chiThi = (get: Lay) => `Soạn CHỈ THỊ hoàn chỉnh, đúng thể thức NĐ 30/2020:

THÔNG TIN:
- Cơ quan: ${get("from_org")}
- Vấn đề: ${get("subject")}
- Nội dung chỉ đạo: ${get("content")}
- Người ký: ${get("signer_name")}, Chức danh: ${get("signer_title")}

CẤU TRÚC CHỈ THỊ:
1. Quốc hiệu, tiêu ngữ
2. Tên cơ quan, Số/CT-UBND
3. Tiêu đề: CHỈ THỊ + "Về việc..."
4. Phần mở đầu: nêu tình hình, lý do ban hành
5. Nội dung chỉ đạo (đánh số 1, 2, 3...)
6. Yêu cầu thực hiện
7. Nơi nhận + Chữ ký`;

const quyChe = (get: Lay) => `Soạn QUY CHẾ hoàn chỉnh:
- Tên quy chế: ${get("subject")}
- Nội dung: ${get("content")}

CẤU TRÚC: Chương I (Quy định chung: phạm vi, đối tượng), Chương II (Nội dung cụ thể), Chương III (Tổ chức thực hiện).
Lưu ý: Quy chế thường ban hành kèm theo Quyết định.`;

const quyDinh = (get: Lay) => `Soạn QUY ĐỊNH hoàn chỉnh:
- Tên quy định: ${get("subject")}
- Nội dung: ${get("content")}

CẤU TRÚC: Chương I (Quy định chung), Chương II (Quy định cụ thể), Chương III (Tổ chức thực hiện).`;

const thongCao = (get: Lay) => `Soạn THÔNG CÁO hoàn chỉnh:
- Vấn đề: ${get("subject")}
- Nội dung: ${get("content")}

THÔNG CÁO thường ngắn gọn, thông tin chính thức đến công chúng. Văn phong rõ ràng, khách quan.`;

const huongDan = (get: Lay) => `Soạn HƯỚNG DẪN hoàn chỉnh, đúng thể thức NĐ 30/2020:

- Vấn đề: ${get("subject")}
- Nội dung: ${get("content")}
- Cơ quan: ${get("from_org")}

CẤU TRÚC: I. Mục đích, yêu cầu; II. Nội dung hướng dẫn (chi tiết từng bước); III. Tổ chức thực hiện.`;

const chuongTrinh = (get: Lay) => `Soạn CHƯƠNG TRÌNH hoàn chỉnh:
- Tên: ${get("subject")}
- Nội dung: ${get("content")}
- Thời gian: ${get("timeline")}

CẤU TRÚC: I. Mục đích, yêu cầu; II. Nội dung (liệt kê hoạt động, thời gian, người chịu trách nhiệm); III. Tổ chức thực hiện.`;

function deAnDuAn(loai: LoaiVanBan, get: Lay): string {
  const typeName = TEN_HIEN_THI[loai]!.toUpperCase();
  return `Soạn ${typeName} hoàn chỉnh:
- Tên: ${get("subject")}
- Mục tiêu: ${get("objectives")}
- Nội dung: ${get("content")}
- Kinh phí: ${get("budget")}

CẤU TRÚC: I. Sự cần thiết; II. Mục tiêu; III. Nội dung; IV. Giải pháp; V. Kinh phí; VI. Tổ chức thực hiện.`;
}

const bienBan = (get: Lay) => `Soạn BIÊN BẢN hoàn chỉnh:
- Cuộc họp/Làm việc: ${get("subject")}
- Thời gian: ${get("time")}
- Địa điểm: ${get("location")}
- Thành phần: ${get("attendees")}
- Nội dung: ${get("content")}

CẤU TRÚC BIÊN BẢN: Thời gian, Địa điểm, Thành phần (Chủ trì, Tham dự, Thư ký), Nội dung, Kết luận.
Biên bản KHÔNG đánh số ký hiệu. Có CHỦ TRÌ và THƯ KÝ ký.`;

const hopDong = (get: Lay) => `Soạn HỢP ĐỒNG hoàn chỉnh:
- Tên: ${get("subject")}
- Bên A: ${get("party_a")}
- Bên B: ${get("party_b")}
- Nội dung: ${get("content")}

CẤU TRÚC: Căn cứ pháp lý, Thông tin các bên, Điều 1 (Nội dung), Điều 2 (Thời gian), Điều 3 (Giá trị), Điều 4 (Quyền/Nghĩa vụ), Điều 5 (Điều khoản chung).`;

const congDien = (get: Lay) => `Soạn CÔNG ĐIỆN hoàn chỉnh, khẩn cấp:
- Vấn đề: ${get("subject")}
- Nơi nhận: ${get("to_org")}
- Nội dung: ${get("content")}

CÔNG ĐIỆN phải ngắn gọn, khẩn trương. Mở đầu: "[CƠ QUAN] ĐIỆN:" + nơi nhận. Kết thúc yêu cầu khẩn trương thực hiện.`;

function thoaThuan(loai: LoaiVanBan, get: Lay): string {
  const typeName = TEN_HIEN_THI[loai]!.toUpperCase();
  return `Soạn ${typeName} hoàn chỉnh:
- Vấn đề: ${get("subject")}
- Nội dung: ${get("content")}
- Các bên: ${get("parties")}

CẤU TRÚC: Thông tin các bên, Nội dung thỏa thuận, Cam kết thực hiện. ${typeName} KHÔNG đánh số ký hiệu.`;
}

const giayUyQuyen = (get: Lay) => `Soạn GIẤY ỦY QUYỀN hoàn chỉnh:
- Người ủy quyền: ${get("grantor")}
- Người được ủy quyền: ${get("grantee")}
- Nội dung: ${get("content")}
- Thời hạn: ${get("duration")}

Theo Điều 13 NĐ 30/2020: Người được ủy quyền KHÔNG được ủy quyền lại.`;

const giayMoi = (get: Lay) => `Soạn GIẤY MỜI hoàn chỉnh:
- Nội dung mời: ${get("subject")}
- Thời gian: ${get("time")}
- Địa điểm: ${get("location")}
- Người nhận: ${get("to_org")}

Giấy mời ngắn gọn, lịch sự, đầy đủ thông tin (nội dung, thời gian, địa điểm, thành phần tham dự).`;

const giayGioiThieu = (get: Lay) => `Soạn GIẤY GIỚI THIỆU hoàn chỉnh:
- Người được giới thiệu: ${get("person")}
- Đến cơ quan: ${get("to_org")}
- Nội dung: ${get("content")}

Giấy giới thiệu phải ghi rõ: Họ tên, chức vụ, đơn vị; Nơi đến; Mục đích; Thời hạn giá trị.`;

const giayNghiPhep = (get: Lay) => `Soạn GIẤY NGHỈ PHÉP hoàn chỉnh:
- Người xin nghỉ: ${get("person")}
- Từ ngày: ${get("from_date")}
- Đến ngày: ${get("to_date")}
- Lý do: ${get("reason")}

Giấy nghỉ phép ghi rõ: Họ tên, chức vụ, đơn vị; Thời gian nghỉ; Lý do; Địa chỉ liên lạc.`;

function phieu(loai: LoaiVanBan, get: Lay): string {
  const typeName = TEN_HIEN_THI[loai]!.toUpperCase();
  return `Soạn ${typeName} hoàn chỉnh:
- Nơi nhận: ${get("to_org")}
- Nội dung: ${get("content")}
- Văn bản kèm theo: ${get("documents")}

${typeName} là văn bản nghiệp vụ văn thư, ngắn gọn, rõ ràng.`;
}

const thuCong = (get: Lay) => `Soạn THƯ CÔNG hoàn chỉnh:
- Loại thư: ${get("letter_type")} (chúc mừng / cảm ơn / chia buồn / thăm hỏi)
- Nơi nhận: ${get("to_org")}
- Nội dung: ${get("content")}

Thư công phải trang trọng, lịch sự, thể hiện tình cảm chân thành. Ký tên và đóng dấu.`;

/** C# BuildGenericPrompt: StringBuilder.AppendLine (CRLF trên Windows, tệp mẫu chuẩn hóa về \n). */
function chung(truong: Array<[string, string]>, c: NguCanh): string {
  let s = `Soạn văn bản hành chính loại ${c.tenMau ?? "Công văn"} hoàn chỉnh, đúng thể thức NĐ 30/2020:\n`;
  s += "\n";
  s += "THÔNG TIN:\n";
  for (const [k, v] of truong) if (v.trim() !== "") s += `- ${nhanTruong(k)}: ${v}\n`;
  s += "\n";
  s += "YÊU CẦU: Soạn văn bản đầy đủ thể thức (quốc hiệu, tiêu ngữ, số/ký hiệu, nội dung, nơi nhận, chữ ký). Văn phong hành chính chuẩn, trang trọng.\n";
  return s;
}

export function nguoiDungSoanThao(d: DauVaoSoanThao): string {
  const bang = new Map(d.truong);
  const get: Lay = (k) => bang.get(k) ?? "";
  const c = new NguCanh(d.ten_mau, d.dia_phuong);
  switch (d.loai_van_ban) {
    case "CongVan": return congVan(get, c);
    case "QuyetDinh": return quyetDinh(get, c);
    case "BaoCao": return baoCao(get, c);
    case "ToTrinh": return toTrinh(get);
    case "KeHoach": return keHoach(get, c);
    case "ThongBao": return thongBao(get, c);
    case "NghiQuyet": return nghiQuyet(get, c);
    case "ChiThi": return chiThi(get);
    case "QuyChE": return quyChe(get);
    case "QuyDinh": return quyDinh(get);
    case "ThongCao": return thongCao(get);
    case "HuongDan": return huongDan(get);
    case "ChuongTrinh": return chuongTrinh(get);
    case "PhuongAn": case "DeAn": case "DuAn": return deAnDuAn(d.loai_van_ban, get);
    case "BienBan": return bienBan(get);
    case "HopDong": return hopDong(get);
    case "CongDien": return congDien(get);
    case "BanGhiNho": case "BanThoaThuan": return thoaThuan(d.loai_van_ban, get);
    case "GiayUyQuyen": return giayUyQuyen(get);
    case "GiayMoi": return giayMoi(get);
    case "GiayGioiThieu": return giayGioiThieu(get);
    case "GiayNghiPhep": return giayNghiPhep(get);
    case "PhieuGui": case "PhieuChuyen": case "PhieuBao": return phieu(d.loai_van_ban, get);
    case "ThuCong": return thuCong(get);
    default: return chung(d.truong, c); // Luat, NghiDinh, ThongTu, Khac
  }
}

export function dungPromptSoanThao(d: DauVaoSoanThao): PromptTacVu {
  return { he_thong: heThongSoanThao(d.loai_van_ban, d.dia_phuong), nguoi_dung: nguoiDungSoanThao(d) };
}
```

- [ ] **Step 4: Viết `dinh-nghia.ts` và đăng ký**

`src/lib/tac-vu/soan-thao/dinh-nghia.ts`:

```ts
import type { DinhNghiaTacVu } from "../chay-tac-vu";
import { dungPromptSoanThao, LOAI_VAN_BAN, type DauVaoSoanThao, type DiaPhuong, type LoaiVanBan } from "./prompt";

const KHOA_TRUONG = /^[A-Za-z][A-Za-z0-9_]{0,63}$/;
const SO_TRUONG_TOI_DA = 60;
const DAC_DIEM_TOI_DA = 10;
const DAI_TOI_DA = 300;

const laDoiTuong = (v: unknown): v is Record<string, unknown> => typeof v === "object" && v !== null && !Array.isArray(v);
const coChu = (v: unknown): v is string => typeof v === "string" && v.trim() !== "" && v.length <= DAI_TOI_DA;
type Ket<T> = { ok: true; v: T } | { ok: false; loi: string };

function docDiaPhuong(v: unknown): Ket<DiaPhuong | null> {
  if (v == null) return { ok: true, v: null };
  if (!laDoiTuong(v)) return { ok: false, loi: "dia_phuong phải là đối tượng { xa, co_quan_cap_tren, tinh, dac_diem }." };
  if (!coChu(v.xa) || !coChu(v.tinh)) return { ok: false, loi: "dia_phuong cần xa và tinh (chuỗi, tối đa 300 ký tự)." };
  if (v.co_quan_cap_tren != null && typeof v.co_quan_cap_tren !== "string") {
    return { ok: false, loi: "dia_phuong.co_quan_cap_tren phải là chuỗi." };
  }
  let dac_diem: string[] | null = null;
  if (v.dac_diem != null) {
    if (!Array.isArray(v.dac_diem) || v.dac_diem.length > DAC_DIEM_TOI_DA || !v.dac_diem.every((x) => typeof x === "string" && x.length <= DAI_TOI_DA)) {
      return { ok: false, loi: `dia_phuong.dac_diem phải là mảng tối đa ${DAC_DIEM_TOI_DA} chuỗi.` };
    }
    dac_diem = v.dac_diem as string[];
  }
  const capTren = typeof v.co_quan_cap_tren === "string" && v.co_quan_cap_tren.trim() !== "" ? v.co_quan_cap_tren : null;
  return { ok: true, v: { xa: v.xa, co_quan_cap_tren: capTren, tinh: v.tinh, dac_diem } };
}

export const soanThao: DinhNghiaTacVu<DauVaoSoanThao> = {
  ten: "soan-thao",
  kieu_ket_qua: "van_ban",
  // Bản cài máy gọi /api/ai/generate không truyền temperature, máy chủ dùng 0,7.
  nhiet_do: 0.7,
  kiemDauVao(body) {
    const b: Record<string, unknown> = laDoiTuong(body) ? body : {};
    if (typeof b.loai_van_ban !== "string" || !(LOAI_VAN_BAN as readonly string[]).includes(b.loai_van_ban)) {
      return { ok: false, loi: "loai_van_ban phải là tên loại văn bản (vd CongVan, QuyetDinh, BaoCao)." };
    }
    if (!laDoiTuong(b.truong)) return { ok: false, loi: "Thiếu truong: đối tượng khóa → chuỗi." };
    const truong: Array<[string, string]> = [];
    for (const [k, v] of Object.entries(b.truong)) {
      if (!KHOA_TRUONG.test(k)) return { ok: false, loi: `Tên trường không hợp lệ: ${k.slice(0, 40)}` };
      if (typeof v !== "string") return { ok: false, loi: `Trường ${k} phải là chuỗi.` };
      truong.push([k, v]);
    }
    if (truong.length > SO_TRUONG_TOI_DA) return { ok: false, loi: `Tối đa ${SO_TRUONG_TOI_DA} trường.` };
    if (b.ten_mau != null && (typeof b.ten_mau !== "string" || b.ten_mau.length > DAI_TOI_DA)) {
      return { ok: false, loi: "ten_mau phải là chuỗi, tối đa 300 ký tự." };
    }
    const dp = docDiaPhuong(b.dia_phuong);
    if (!dp.ok) return { ok: false, loi: dp.loi };
    return {
      ok: true,
      du_lieu: {
        loai_van_ban: b.loai_van_ban as LoaiVanBan,
        ten_mau: typeof b.ten_mau === "string" && b.ten_mau !== "" ? b.ten_mau : null,
        truong,
        dia_phuong: dp.v,
      },
    };
  },
  dungPrompt: (d) => dungPromptSoanThao(d),
};
```

`src/lib/tac-vu/dang-ky.ts`: thêm `import { soanThao } from "./soan-thao/dinh-nghia";` ngay sau dòng `import { trichXuat } from "./trich-xuat/dinh-nghia";`, và thêm `  [soanThao.ten]: soanThao,` ngay sau dòng `  [trichXuat.ten]: trichXuat,`.

- [ ] **Step 5: Chạy, thấy qua**

Run: `npx tsx --test tests/tac-vu-soan-thao.test.ts && npx tsc --noEmit -p .`
Expected: `# pass 6`. Bài so khớp hỏng thì thông báo đã nêu tên ca và trường (`he_thong`/`nguoi_dung`). In vị trí khác đầu tiên:

```bash
npx tsx -e 'import { readFileSync } from "node:fs"; import { soanThao } from "./src/lib/tac-vu/soan-thao/dinh-nghia";
const m = JSON.parse(readFileSync("tests/mau-prompt/soan-thao.json", "utf8"));
for (const x of m as any[]) { const d = soanThao.kiemDauVao(x.dau_vao) as any; const p = soanThao.dungPrompt(d.du_lieu, new Date());
for (const k of ["he_thong","nguoi_dung"]) { const a = (p as any)[k], b = x[k]; if (a !== b) { let i = 0; while (a[i] === b[i]) i++;
console.log(x.ten, k, i, JSON.stringify(a.slice(i-20,i+40)), "≠", JSON.stringify(b.slice(i-20,i+40))); } } }'
```
rồi sửa `prompt.ts` cho khớp tệp mẫu (không sửa tệp mẫu).

- [ ] **Step 6: Bộ kiểm đầy đủ, build, commit**

```bash
TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npm test && npx next build
git add src/lib/tac-vu/soan-thao src/lib/tac-vu/dang-ky.ts tests/tac-vu-soan-thao.test.ts tests/mau-prompt/soan-thao.json && git commit -F - <<'EOF'
feat(tac-vu): soan-thao qua /api/v2 (prompt so khop tung ky tu voi ComposePromptBuilder C#, dia_phuong la dau vao)

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
```
Expected: mọi bài pass (bài CSDL không bị bỏ qua), build xong.

---

### Task 10: Thử trên bản sao prod, migrate prod, đẩy `main`, chạy thật, kiểm sổ cái

**Files:**
- Create (một lần, không commit): `D:/AIVanBanCaNhan/.superpowers/tam/tao-tep-thu.mjs`, `D:/AIVanBanCaNhan/.superpowers/tam/kiem-so-cai.sql`
- Modify: `D:/AIVanBanCaNhan/PROJECT_STATUS.md` (một dòng), `D:/Thang/gikidi-server/THAY-DOI.md` (một dòng)

**Điểm dừng:** trước Step 3 (migrate prod), báo chủ dự án một dòng: «Sắp thêm 2 dòng bảng giá (trich-xuat, soan-thao) vào CSDL prod VanBanPlus và đẩy main; mở /api/v2/tac-vu/trich-xuat và /soan-thao». Lệnh «làm đi» cho cả kế hoạch được tính là đồng ý, trừ khi chủ dự án dặn khác.

- [ ] **Step 1: Bộ kiểm đầy đủ hai repo + build trên nhánh**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git status --short && TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npm test && npx next build
cd /d/AIVanBanCaNhan && dotnet test AIVanBan.Tests && dotnet build AIVanBan.Desktop
```
Expected: cây `vanbanplus-api` sạch; mọi bài TS pass (kể cả `bang-gia-dot2b-csdl`, `credit-csdl`); build Next xong; mọi bài C# pass; Desktop build 0 lỗi.

- [ ] **Step 2: Thử migration trên bản sao prod**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && scp -q supabase/migrations/008_bang_gia_dot2b.sql gikidi-dev:/tmp/008_bang_gia_dot2b.sql
ssh gikidi-dev "set -e
  PROD=supabase-db-q323olywghtnjcvpyvtjzo9v
  sudo docker exec \$PROD pg_dump -U postgres -d postgres -n public --no-owner > /tmp/vbp-ban-sao.sql
  export PGPASSWORD=thu; P='psql -h 127.0.0.1 -p 55990 -U postgres -q'
  \$P -d postgres -c 'drop database if exists vbp_ban_sao with (force)' -c 'create database vbp_ban_sao'
  \$P -d vbp_ban_sao -f /tmp/vbp-ban-sao.sql >/dev/null 2>/tmp/vbp-nap.err || true
  echo 'Lỗi khi nạp bản sao:'; grep ERROR /tmp/vbp-nap.err | sort | uniq -c | head
  \$P -d vbp_ban_sao -Atc \"select count(*) from credit_bang_gia\"
  \$P -d vbp_ban_sao -v ON_ERROR_STOP=1 -1 -f /tmp/008_bang_gia_dot2b.sql
  \$P -d vbp_ban_sao -v ON_ERROR_STOP=1 -1 -f /tmp/008_bang_gia_dot2b.sql
  \$P -d vbp_ban_sao -Atc \"select ma, gia_tri from credit_bang_gia where loai = 'tac_vu' order by ma\"
  \$P -d postgres -c 'drop database vbp_ban_sao with (force)'
  rm -f /tmp/vbp-ban-sao.sql /tmp/vbp-nap.err"
```
Expected: lỗi khi nạp chỉ ở phần không liên quan (như đợt 1: `schema "public" already exists`, kiểu `vector` của bảng cloud). `credit_bang_gia` nạp đủ (dòng của 004 + 007). Cả hai lần chạy 008 không lỗi. Danh sách `tac_vu` có `api-cu`, `tom-tat`, ba tác vụ `dot2a`, `soan-thao`, `trich-xuat` (đúng giá trị của 008). Bản sao và tệp dump bị xóa ngay (có dữ liệu thật).

- [ ] **Step 3: Migrate prod + nạp lại lược đồ PostgREST + ghi sổ thay đổi**

```bash
PROD=supabase-db-q323olywghtnjcvpyvtjzo9v
ssh gikidi-dev "sudo docker exec -i $PROD psql -U postgres -d postgres -v ON_ERROR_STOP=1 -1 -q < /tmp/008_bang_gia_dot2b.sql && sudo docker exec $PROD psql -U postgres -d postgres -c \"NOTIFY pgrst, 'reload schema'\" && sudo docker exec $PROD psql -U postgres -d postgres -Atc \"select ma from credit_bang_gia where loai = 'tac_vu' and ma in ('trich-xuat','soan-thao') order by ma\"; rm -f /tmp/008_bang_gia_dot2b.sql"
```
Expected: `soan-thao` và `trich-xuat`. Thêm một dòng vào `D:\Thang\gikidi-server\THAY-DOI.md` (theo kiểu các dòng sẵn có): ngày giờ, «VanBanPlus: migrate 008_bang_gia_dot2b lên `supabase-db-q323…` (2 dòng credit_bang_gia)», cách lùi: `DELETE FROM credit_bang_gia WHERE loai = 'tac_vu' AND ma IN ('trich-xuat','soan-thao');` (chỉ sau khi đã lùi mã). Không có bảng / CSDL / volume mới nên không đổi gì về sao lưu.

Mã cũ đang chạy trên prod không đọc hai dòng này (`layTacVu` chưa có hai tác vụ), nên migrate trước khi đẩy là an toàn.

- [ ] **Step 4: Gộp và đẩy `main` (lên production)**

```bash
cd /d/AIVanBanCaNhan/vanbanplus-api && git checkout main && git pull --ff-only && git merge --no-ff dot2b -m "Merge dot2b: tac vu trich-xuat, soan-thao" && TEST_DATABASE_URL=postgres://postgres:thu@127.0.0.1:55990/postgres npm test && npx next build && git push origin main
```
Expected: đẩy xong. Theo dõi Coolify: `python D:\Thang\gikidi-server\cong-cu\tu-trien-khai.py xem` (hoặc chờ khoảng 3–5 phút).

```bash
curl -s https://vanbanplus.giakiemso.com/api/health; echo
for t in trich-xuat soan-thao; do curl -s -o /dev/null -w "$t %{http_code}\n" -X POST https://vanbanplus.giakiemso.com/api/v2/tac-vu/$t; done
```
Expected: `401` cho cả hai (thiếu khóa, nghĩa là tác vụ đã có; `404` nghĩa là bản mới chưa lên).

- [ ] **Step 5: Tạo tệp thử (không dùng dữ liệu khách)**

Ảnh PNG có chữ (PowerShell, System.Drawing):

```powershell
Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap 900, 520
$g = [System.Drawing.Graphics]::FromImage($bmp); $g.Clear([System.Drawing.Color]::White); $g.TextRenderingHint = 'AntiAlias'
$f = New-Object System.Drawing.Font 'Times New Roman', 16; $b = [System.Drawing.Brushes]::Black
$dong = @('ỦY BAN NHÂN DÂN XÃ MINH AN', 'Số: 12/UBND-VP', 'V/v thử nghiệm trích xuất văn bản', 'Kính gửi: Văn phòng UBND xã Minh An',
  'Đề nghị kiểm tra chức năng trích xuất trước ngày 30/11/2026.', 'Nơi nhận: Như trên; Lưu: VT.', 'CHỦ TỊCH', 'Nguyễn Văn Thử')
$y = 20; foreach ($d in $dong) { $g.DrawString($d, $f, $b, 30, $y); $y += 60 }
$bmp.Save('D:\AIVanBanCaNhan\.superpowers\tam\vbp-thu.png', [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $bmp.Dispose()
```

`D:/AIVanBanCaNhan/.superpowers/tam/tao-tep-thu.mjs` dựng một PDF chữ ASCII một trang và ba thân yêu cầu JSON:

```js
import { readFileSync, writeFileSync } from "node:fs";
const TAM = "D:/AIVanBanCaNhan/.superpowers/tam";

const dong = ["UBND XA MINH AN", "So: 13/UBND-VP", "V/v thu nghiem doc chu tu tep PDF", "Noi dung: kiem tra che do chi-chu cua VanBanPlus."];
const luong = "BT /F1 14 Tf 50 780 Td 22 TL " + dong.map((d) => `(${d}) Tj T*`).join(" ") + " ET";
const obj = [
  "<< /Type /Catalog /Pages 2 0 R >>",
  "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
  "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
  "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
  `<< /Length ${luong.length} >>\nstream\n${luong}\nendstream`,
];
let pdf = "%PDF-1.4\n";
const vt = [];
obj.forEach((o, i) => { vt.push(pdf.length); pdf += `${i + 1} 0 obj\n${o}\nendobj\n`; });
const xref = pdf.length;
pdf += `xref\n0 ${obj.length + 1}\n0000000000 65535 f \n` + vt.map((v) => `${String(v).padStart(10, "0")} 00000 n \n`).join("");
pdf += `trailer\n<< /Size ${obj.length + 1} /Root 1 0 R >>\nstartxref\n${xref}\n%%EOF\n`;
writeFileSync(`${TAM}/vbp-thu.pdf`, pdf, "latin1");

const png = readFileSync(`${TAM}/vbp-thu.png`).toString("base64");
const pdfB64 = Buffer.from(pdf, "latin1").toString("base64");
writeFileSync(`${TAM}/vbp-tx-cau-truc.json`, JSON.stringify({ tep: [{ base64: png, mime: "image/png" }] }));
writeFileSync(`${TAM}/vbp-tx-chi-chu.json`, JSON.stringify({ tep: [{ base64: pdfB64, mime: "application/pdf" }], che_do: "chi-chu" }));
writeFileSync(`${TAM}/vbp-st.json`, JSON.stringify({
  loai_van_ban: "CongVan",
  ten_mau: "Công văn thử",
  truong: { from_org: "UBND xã Minh An", to_org: "Văn phòng UBND xã Minh An", subject: "Thử nghiệm soạn thảo",
            signer_name: "Nguyễn Văn Thử", signer_title: "Chủ tịch", content: "Kiểm tra chức năng soạn công văn qua API mới." },
  dia_phuong: { xa: "xã Minh An", tinh: "tỉnh Lâm Đồng" },
}));
console.log("Đã tạo PDF + 3 thân yêu cầu");
```

`D:/AIVanBanCaNhan/.superpowers/tam/kiem-so-cai.sql`:

```sql
\pset footer off
-- Lượt dot2b của tài khoản quản trị trong 30 phút qua: tổng sổ cái của lượt phải bằng −credit_tru (giữ −max, hoàn +max, trừ −thật).
WITH u AS (SELECT id FROM users WHERE role = 'admin' ORDER BY created_date LIMIT 1)
SELECT t.tac_vu, t.trang_thai, t.credit_min, t.credit_max, t.credit_giu, t.credit_tru, t.token_vao, t.token_ra, t.bi_cat, t.ma_loi,
       (SELECT COALESCE(SUM(s.so), 0) FROM credit_so s WHERE s.luot_id = t.id) AS tong_so_cai,
       (SELECT COALESCE(SUM(s.so), 0) FROM credit_so s WHERE s.luot_id = t.id) = -t.credit_tru AS khop
  FROM tac_vu_chay t
 WHERE t.user_id = (SELECT id FROM u) AND t.tac_vu IN ('trich-xuat', 'soan-thao') AND t.bat_dau_luc > NOW() - INTERVAL '30 minutes'
 ORDER BY t.bat_dau_luc;
-- Lô lệch sổ cái của tài khoản đó (phải rỗng).
WITH u AS (SELECT id FROM users WHERE role = 'admin' ORDER BY created_date LIMIT 1)
SELECT l.id, l.so_con, COALESCE(SUM(s.so), 0) AS tong
  FROM credit_lo l LEFT JOIN credit_so s ON s.lo_id = l.id
 WHERE l.user_id = (SELECT id FROM u)
 GROUP BY l.id, l.so_con HAVING l.so_con <> COALESCE(SUM(s.so), 0);
-- Không có cột nội dung nào trong tac_vu_chay (spec mục 1 tiêu chí 5).
SELECT column_name FROM information_schema.columns WHERE table_name = 'tac_vu_chay' AND column_name ~ 'noi_dung|prompt|ket_qua';
```

```bash
cd /d/AIVanBanCaNhan && node .superpowers/tam/tao-tep-thu.mjs && scp -q .superpowers/tam/vbp-tx-cau-truc.json .superpowers/tam/vbp-tx-chi-chu.json .superpowers/tam/vbp-st.json .superpowers/tam/kiem-so-cai.sql gikidi-dev:/tmp/
```

- [ ] **Step 6: Chạy thật mỗi tác vụ một lượt trên production**

Khóa API quản trị lấy ngay trên máy chủ, **không in ra**:

```bash
ssh gikidi-dev 'K=$(sudo docker exec supabase-db-q323olywghtnjcvpyvtjzo9v psql -U postgres -d postgres -Atc "select api_key from users where role = '"'"'admin'"'"' order by created_date limit 1");
  U=https://vanbanplus.giakiemso.com; H="-H X-API-Key:$K -H X-VBP-Nguon:kiem-thu -H Content-Type:application/json"
  curl -s -H "X-API-Key: $K" $U/api/v2/credit; echo
  echo "--- trich-xuat cau-truc: uoc-tinh, chay";  curl -s $H -d @/tmp/vbp-tx-cau-truc.json $U/api/v2/tac-vu/trich-xuat/uoc-tinh; echo
  curl -s $H -d @/tmp/vbp-tx-cau-truc.json $U/api/v2/tac-vu/trich-xuat | head -c 700; echo
  echo "--- trich-xuat chi-chu";  curl -s $H -d @/tmp/vbp-tx-chi-chu.json $U/api/v2/tac-vu/trich-xuat | head -c 500; echo
  echo "--- soan-thao: uoc-tinh, chay";  curl -s $H -d @/tmp/vbp-st.json $U/api/v2/tac-vu/soan-thao/uoc-tinh; echo
  curl -s $H -d @/tmp/vbp-st.json $U/api/v2/tac-vu/soan-thao | head -c 900; echo
  echo "--- dau vao sai (khong tru credit)";  curl -s $H -d "{\"tep\":[{\"base64\":\"AAAA\",\"mime\":\"image/gif\"}]}" $U/api/v2/tac-vu/trich-xuat; echo'
```
Expected:
- `/api/v2/credit`: `tong` > 0. Nếu số dư thấp hơn `credit_max` của một tác vụ (`KHONG_DU_CREDIT`), cộng 50 credit tại `/admin/credit` (ghi chú «thử dot2b»), rồi chạy lại.
- `trich-xuat` `uoc-tinh`: `credit_min` 3, `credit_max` khoảng 11. Lượt chạy trả `success: true`, `ket_qua` là đối tượng có `so_van_ban` («12/UBND-VP»), `trich_yeu`, `noi_dung`; `credit_da_tru` nằm trong khoảng ước tính.
- `chi-chu`: `ket_qua` là chuỗi chứa «UBND XA MINH AN».
- `soan-thao`: `credit_min` 4–6. Lượt chạy trả `ket_qua` là chuỗi văn bản công văn có «UBND XÃ MINH AN» và **không** có «Gia Kiệm».
- Đầu vào sai: `{"success":false,…,"code":"DAU_VAO_SAI"}`.

Nếu lượt `trich-xuat` báo `AI_LOI` mà `/admin/credit` / log Coolify cho thấy Vertex từ chối `thinkingConfig` hoặc `responseSchema` với `gemini-3.6-flash`: đổi `cau_hinh/chung.mo_hinh_mac_dinh` về `gemini-3.5-flash` ở `/admin/credit` (mô hình route cũ đang chạy được), chạy lại, ghi lại và báo chủ dự án. Credit của lượt lỗi đã tự hoàn.

- [ ] **Step 7: Kiểm sổ cái, dọn**

```bash
ssh gikidi-dev 'sudo docker exec -i supabase-db-q323olywghtnjcvpyvtjzo9v psql -U postgres -d postgres < /tmp/kiem-so-cai.sql; rm -f /tmp/vbp-tx-cau-truc.json /tmp/vbp-tx-chi-chu.json /tmp/vbp-st.json /tmp/kiem-so-cai.sql'
rm -f /d/AIVanBanCaNhan/.superpowers/tam/vbp-*
```
Expected:
- truy vấn 1 có 3 lượt (`trich-xuat` ×2, `soan-thao` ×1), tất cả `xong`, `ma_loi` rỗng, `credit_giu = credit_max`, `credit_min ≤ credit_tru ≤ credit_max`, `token_vao`/`token_ra` > 0, cột `khop` = `t`;
- truy vấn 2 rỗng;
- truy vấn 3 rỗng.

Lượt `DAU_VAO_SAI` không tạo dòng nào.

- [ ] **Step 8: Cập nhật tài liệu, commit repo gốc**

- `PROJECT_STATUS.md`: thêm một dòng vào bảng tính năng, cùng kiểu dòng credit đợt 1: `| ✅ | Tác vụ v2 trich-xuat, soan-thao (đợt 2b, <ngày>) | vanbanplus-api: migration 008, /api/v2/tac-vu/trich-xuat (cau-truc, chi-chu), /api/v2/tac-vu/soan-thao; C#: ComposePromptBuilder (prompt soạn thảo tách khỏi hộp thoại, địa phương là tham số) | Bản cài máy vẫn gọi đường cũ tới đợt 3 |`. Chỉ thêm dòng này; nếu tệp đang có thay đổi dở của phiên khác thì chỉ commit khi thay đổi dở đó cũng thuộc VanBanPlus và đã đúng, không chắc thì hỏi.
- `bash vanbanplus-api/scripts/csdl-thu.sh tat`; tắt đường hầm SSH.

```bash
cd /d/AIVanBanCaNhan && git add PROJECT_STATUS.md vanbanplus-api && git commit -F - <<'EOF'
docs: dot 2b (trich-xuat, soan-thao) len production; cap nhat vanbanplus-api

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
```
(Repo gốc không đẩy, trừ khi chủ dự án yêu cầu.)

#### Kịch bản lùi

| Tình huống | Làm |
| --- | --- |
| Build Coolify hỏng | Coolify giữ bản cũ đang chạy. Sửa trên nhánh, đẩy lại; không cần đụng CSDL. |
| Bản mới lỗi trên prod (đường cũ `/api/ai/*` hay tác vụ v2) | `cd vanbanplus-api && git revert -m 1 <mã commit merge dot2b> && npm test && npx next build && git push origin main`. Lùi mã là lùi cả việc chuyển hằng prompt (route cũ có lại hằng riêng). |
| Cần gỡ bảng giá (sau khi đã lùi mã) | `ssh gikidi-dev "sudo docker exec supabase-db-q323olywghtnjcvpyvtjzo9v psql -U postgres -d postgres -1 -c \"DELETE FROM credit_bang_gia WHERE loai = 'tac_vu' AND ma IN ('trich-xuat','soan-thao')\""`. Dòng `tac_vu_chay` / sổ cái của các lượt thử giữ nguyên (sổ cái chỉ ghi thêm). Ghi vào `THAY-DOI.md`. Không lùi mã mà xóa bảng giá thì hai tác vụ trả 500 (`Bảng giá thiếu tác vụ`): luôn lùi mã trước. |
| Giá sai (giữ quá nhiều / quá ít) | Sửa trực tiếp ở `/admin/credit` (`tac_vu/trich-xuat`, `tac_vu/soan-thao`, gồm `token_moi_trang`); có hiệu lực sau ≤ 60 giây, không cần triển khai. |
| C# (repo gốc) | Chưa đẩy, bản cài máy chưa phát hành: `git revert` các commit Task 5–8 nếu cần. Hộp thoại cho prompt y như cũ (bài `HopThoaiKhopBanChup` chứng minh). |

---

## Điểm cần chủ dự án quyết

1. **Bổ sung hợp đồng (Task 0):** thêm `ten_mau` và `dia_phuong.dac_diem`, `co_quan_cap_tren` thành tùy chọn. Không có hai trường đầu thì không tái tạo được prompt cũ từng ký tự (tên mẫu dùng ở 5 hàm dựng; ba dòng dân số/ấp/xã loại I của Gia Kiệm không suy ra được từ xã/cấp trên/tỉnh). Người điều phối cần báo cho kế hoạch `dot3`: bản cài máy gửi `ten_mau` = tên mẫu đang chọn, `dac_diem` lấy từ thiết lập cơ quan (nếu có).
2. **`chi-chu` tắt suy nghĩ** (route cũ `/api/ai/read-text` không tắt). Văn bản prompt không đổi, chỉ đổi cấu hình gọi.
3. **Mime nhận của `trich-xuat`** chỉ gồm PDF/PNG/JPEG/WEBP/HEIC/HEIF. Bản C# hiện có thể gửi GIF/BMP/TIFF; `dot3` phải đổi sang PNG/JPEG trước khi gửi.
4. **Chỗ trống khi không có địa phương** (`[CƠ QUAN BAN HÀNH]`, `[Địa danh]`, `[TÊN XÃ/PHƯỜNG]`…) là chữ mới. Muốn văn phong khác thì sửa ở `ComposePromptBuilder` + `prompt.ts` + sinh lại tệp mẫu.

## Giả định đã đặt

- `dot2a` đã gộp và không đổi `chay-tac-vu.ts`, `uoc-tinh.ts`, `bang-gia.ts`, `extract-input.ts`, `usage.ts`, `route.ts` của `/api/ai`. Task 2 có hướng dẫn áp từng khối nếu `chay-tac-vu.ts` đã khác.
- `RuntimeHelpers.GetUninitializedObject` dùng được với `AIComposeDialog`. Nếu không, có đường dự phòng (chép khối cũ vào lớp kiểm) ở Task 5.
- Tài khoản quản trị trên prod có đủ credit cho ba lượt thử (khoảng 40 credit giữ tạm lần lượt). Thiếu thì cộng tay ở `/admin/credit`.
- `token_moi_trang` = 1.100 là ước lượng theo tài liệu Gemini 3, chưa đo. Xem `token_vao` thật trong `tac_vu_chay` sau tuần đầu.

## Tự soát kế hoạch so với spec

| Spec / yêu cầu | Task |
| --- | --- |
| §5 `trich-xuat` (tệp base64 + mime, `cau-truc` / `chi-chu`, JSON `ExtractedDocumentData` hoặc văn bản) | 3, 4 |
| §2 prompt trích xuất hai bản → máy chủ là chuẩn, ghi khác biệt C# | Quyết định 1, Task 3 (chú thích trong `prompt.ts`) |
| Hằng prompt không nhân đôi: route cũ và tác vụ mới cùng import | 3 (bài kiểm nguồn route + so nguyên văn khi cắt) |
| `PromptTacVu` thêm tệp, lược đồ, `disableThinking` tùy chọn; tác vụ cũ không đổi, có bài chứng minh | 2 (`chay-tac-vu-tep.test.ts` bài 1; `chay-tac-vu.test.ts` + tom-tat qua không sửa) |
| §6 `QUA_DAI` 413 khi tệp vượt `max_file_size_mb` của gói hiện hành (`effectivePlanId`) | 2 |
| `toi_da_ky_tu` hợp lý với tác vụ có tệp | Quyết định 4, 008 (10.000, chỉ phần chữ) |
| §7 ước tính theo số tệp và dung lượng; hiệu chỉnh 3–9 credit cho 1–6 trang | 1, 4 (bài hiệu chỉnh đọc giá từ 008) |
| §5 `soan-thao` (`loai_van_ban` enum, `truong` map) + kiểm đầu vào | 9 |
| §10 tách 26 hàm khỏi `AIComposeDialog` sang lớp thuần trong `AIVanBan.Core`, không đổi hành vi, có bằng chứng trước khi làm TS | 5 (chụp cũ), 6 (lớp mới khớp), 7 (hộp thoại khớp) |
| §10 địa phương ghi cứng → `dia_phuong`; không gửi thì không có bối cảnh; tệp mẫu Gia Kiệm khớp từng ký tự; hộp thoại giữ Gia Kiệm tới đợt 3 | 6, 7, 8, 9 (bài «không gửi dia_phuong») |
| §11 so khớp prompt: C# sinh tệp mẫu đủ mọi nhánh soạn thảo, TS so từng ký tự | 8 (33 loại, 52 ca, kiểm đủ enum), 9 |
| §7 bảng giá trong CSDL: `008` (INSERT … ON CONFLICT DO NOTHING), soan-thao 4–13 | 4 (+ bài CSDL chạy hai lần), 9 (bài hiệu chỉnh soan-thao) |
| §12 thử bản sao → migrate prod → đẩy main → chạy thật → kiểm sổ cái, có lùi | 10 |
| §1 tiêu chí 2 (sổ cái khớp), 5 (không lưu nội dung) | 10 Step 7 (truy vấn 1–3) |
| §8 lưu hỗ trợ, gói nạp, bỏ đường AI Studio miễn phí | `dot2c` (ngoài phạm vi) |
| §10 bản cài máy gọi v2, xóa prompt C#, bỏ gọi thẳng Gemini | `dot3` (ngoài phạm vi) |
