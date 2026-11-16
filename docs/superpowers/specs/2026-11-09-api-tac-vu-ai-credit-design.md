# API tác vụ AI và credit — thiết kế (bước 1 lộ trình đa nền tảng)

Ngày: 09/11/2026. Trạng thái: đã duyệt 12/11/2026; đợt 1 (lớp credit + tom-tat) lên production 12/11/2026 — kế hoạch `docs/superpowers/plans/2026-11-12-lop-credit-va-khung-tac-vu.md`; đợt 2a (kiem-tra, tham-muu, bao-cao-dinh-ky) lên production 16/11/2026; đợt 2b (trich-xuat, soan-thao) lên production 16/11/2026; đợt 2c (gói nạp, lưu hỗ trợ, v2 chỉ Vertex) lên production 16/11/2026; đợt 3 (bản cài máy v1.0.20 gọi v2) phát hành 17/11/2026.

## 1. Mục tiêu

Chuyển prompt và nghiệp vụ AI từ bản cài máy C# về `vanbanplus-api`, thành các tác vụ có tên, tính tiền bằng credit. Web, app iOS/Android (Capacitor) và bản cài máy WPF sau này cùng gọi một API.

Bản này là bước 1 của lộ trình: API tác vụ AI → bản web cá nhân → phần tổ chức → đóng gói app di động.

**Tiêu chí hoàn thành**

1. Sáu tác vụ chạy qua `/api/v2/tac-vu/*`, trả kết quả tương đương bản cũ với cùng đầu vào.
2. Credit giữ, trừ, hoàn đúng, kể cả khi lỗi và khi gọi đồng thời; sổ cái khớp số dư.
3. Bản cài máy mới không còn prompt nghiệp vụ và không còn gọi thẳng Gemini.
4. Bản cài máy cũ vẫn chạy qua `/api/ai/*` và bị trừ credit khi dùng khóa hệ thống.
5. Mặc định không có nội dung văn bản nào lưu trên máy chủ.

**Ngoài phạm vi:** dữ liệu văn bản trên máy chủ (bước 1b, spec riêng), giao diện làm việc trên web, streaming, hàng đợi tác vụ, đăng nhập bằng token cho web/app (bước 2), cải tiến nội dung prompt.

## 2. Hiện trạng đã kiểm

- `vanbanplus-api`: Next.js 16, CSDL Postgres trên Megacore. Đã có đăng nhập, khóa API (`validateApiKey`), gói (`subscription_plans`), đơn hàng (`orders`), webhook SePay, ghi lượt dùng (`usage_records`), và cổng `/api/ai/generate|extract|read-text` không streaming.
- `callAI` hỗ trợ Gemini, OpenAI, Claude, Grok. `resolveAIKey` ưu tiên khóa riêng của người dùng.
- Khóa hệ thống Gemini đi qua Vertex AI (commit `24dd6de`, biến môi trường đã có trên prod), bằng credit dùng thử hết hạn 18/12/2026; lỗi thì tự về AI Studio bậc miễn phí 5 lượt/phút.
- Nhật ký hiện không ghi nội dung văn bản, chỉ ghi token và thời gian.
- CSDL prod chưa có đơn hàng nào, `usage_records` mới có 4 dòng: không phải chuyển đổi khách cũ.
- Prompt nằm trong C#:
  - `DocumentReviewService`, `DocumentAdvisoryService`, `DocumentSummaryService`, `PeriodicReportService`;
  - `AIComposeDialog.xaml.cs`: 26 hàm prompt, nằm lẫn trong mã giao diện;
  - prompt trích xuất có hai bản: máy chủ và `GeminiAIService`.
- **Trích xuất nhiều tệp đang gọi thẳng Gemini** bằng khóa phía máy người dùng (`ExtractDocumentFromMultipleFilesAsync`), không qua API.
- **Prompt soạn thảo ghi cứng địa phương** «xã Gia Kiệm, thành phố Thống Nhất, tỉnh Đồng Nai» (`AIComposeDialog.xaml.cs`, `BuildSystemInstruction`). Đo thật: đầu vào là xã Minh An, nhưng bản soạn vẫn ghi cơ quan chủ quản «UBND thành phố Thống Nhất»; Flash-Lite còn đổi hẳn sang «xã Gia Kiệm».
- **`kiem-tra` hỏng với văn bản dài.** Prompt yêu cầu viết lại toàn văn trong `suggested_content`, văn bản 37.800 ký tự chạm trần 16.384 token nên JSON bị cắt, không phân tích được.
- Prompt chưa có bối cảnh chính quyền hai cấp: cả 3 model đều coi Phòng Văn hóa cấp huyện là đơn vị đang tồn tại.

## 3. Quyết định đã chốt

| Vấn đề | Quyết định |
| --- | --- |
| Công nghệ đa nền tảng | TypeScript, web + Capacitor |
| Phạm vi bước 1 | Chỉ AI; dữ liệu văn bản là bước 1b |
| Đơn vị | «credit», không dùng «điểm» hay «tín dụng» |
| Tính credit | Theo mức tiêu hao thật, báo trước khoảng, hoàn khi lỗi |
| Cấp credit | Gói tháng cấp credit (không cộng dồn) + gói nạp dùng 90 ngày |
| Lưu nội dung | Mặc định không lưu; người dùng tự bật để được hỗ trợ |
| Nơi đặt | Trong `vanbanplus-api`, không tách dịch vụ |

## 4. Kiến trúc

```
Bản cài máy / Web / App
        │  dữ liệu nghiệp vụ (không có prompt)
        ▼
/api/v2/tac-vu/<tên>   ──►  chay-tac-vu
                              1. xác thực
                              2. kiểm đầu vào (khai báo của tác vụ)
                              3. ước tính → giữ credit (hàm Postgres)
                              4. dựng prompt → callAI (chỉ khóa hệ thống)
                              5. kiểm kết quả → trừ thật, hoàn phần dư
                              6. (nếu đã bật) lưu mã hóa
```

**Cấu trúc thư mục**

```
src/lib/tac-vu/
  chay-tac-vu.ts          luồng chung, không biết gì về từng tác vụ
  dang-ky.ts              danh sách tác vụ
  <tên>/dinh-nghia.ts     khai báo đầu vào, đầu ra, cấu hình AI
  <tên>/prompt.ts         dựng prompt từ đầu vào, là hàm thuần
src/lib/credit/
  uoc-tinh.ts             quy đổi token ra credit, là hàm thuần
  so-du.ts                gọi các hàm Postgres
src/app/api/v2/tac-vu/[ten]/route.ts
src/app/api/v2/tac-vu/[ten]/uoc-tinh/route.ts
src/app/api/v2/credit/route.ts
```

Prompt là mã nguồn, có kiểm thử và lịch sử sửa. Bảng giá nằm trong CSDL để chỉnh không cần triển khai lại.

## 5. Sáu tác vụ

| Tác vụ | Đầu vào chính | Đầu ra | Nguồn prompt |
| --- | --- | --- | --- |
| `tom-tat` | `noi_dung`, `loai_van_ban`, `tieu_de`, `co_quan` | JSON theo `DocumentSummary` | `DocumentSummaryService` |
| `kiem-tra` | như trên + `van_ban_doi_chieu` | JSON theo `DocumentReviewResult` | `DocumentReviewService` |
| `tham-muu` | như trên + `ngu_canh` (`DocumentAdvisoryContext`) | JSON theo `DocumentAdvisory` | `DocumentAdvisoryService` |
| `bao-cao-dinh-ky` | loại kỳ, kỳ, lĩnh vực, đơn vị, số liệu thô, báo cáo kỳ trước, người ký, chức danh | văn bản | `PeriodicReportService` |
| `trich-xuat` | 1 hoặc nhiều tệp (base64 + mime), chế độ `cau-truc` hoặc `chi-chu` | JSON theo `ExtractedDocumentData`, hoặc văn bản | Máy chủ + `GeminiAIService` |
| `soan-thao` | `loai_van_ban` (tên enum `DocumentType`), `truong` (khóa → giá trị) | văn bản | `AIComposeDialog` (26 hàm) |

Với tác vụ trả JSON: máy chủ phân tích và kiểm kết quả, hỏng thì thử lại 1 lần. Tên trường JSON giữ đúng như prompt hiện nay để bản cài máy dùng lại model C#.

API không tạo tệp Word. Bản cài máy giữ `WordExportService`.

Thời điểm hiện tại, nếu prompt có dùng, được truyền vào hàm dựng prompt như một tham số, để kiểm thử so khớp được.

## 6. Hợp đồng API

Xác thực: header `X-API-Key` như hiện nay, gom vào một hàm để bước 2 thêm token.

**`POST /api/v2/tac-vu/<tên>/uoc-tinh`**: không gọi AI, không giữ credit.

```json
{ "success": true, "message": "Thành công",
  "data": { "credit_min": 6, "credit_max": 9, "so_du": 180, "du_credit": true } }
```

**`POST /api/v2/tac-vu/<tên>`**

```json
{ "success": true, "message": "Thành công",
  "data": { "ket_qua": {}, "credit_da_tru": 7, "so_du": 173, "ma_luot": "uuid" } }
```

**`GET /api/v2/credit`**: tổng số dư, các lô còn hạn, ngày hết hạn gần nhất.

**Mã lỗi** (`{ "success": false, "message", "code" }`):

| Mã | HTTP | Khi nào | Credit |
| --- | --- | --- | --- |
| `KHONG_DU_CREDIT` | 402 | Số dư thấp hơn `credit_max`; chưa gọi AI | Không trừ |
| `DAU_VAO_SAI` | 400 | Sai hoặc thiếu trường | Không trừ |
| `QUA_DAI` | 413 | Vượt giới hạn tác vụ, hoặc tệp vượt `max_file_size_mb` của gói | Không trừ |
| `QUA_NHIEU_LUOT` | 429 | Đang có 3 lượt chạy cùng lúc | Không trừ |
| `AI_QUA_TAI` | 503 + `Retry-After` | Nhà cung cấp trả 429 | Hoàn hết |
| `AI_LOI` | 502 | AI lỗi, hoặc JSON hỏng sau 1 lần thử lại | Hoàn hết |

Kết quả bị cắt vì chạm trần token ra vẫn tính là thành công và có trừ credit, đồng thời trả cờ `bi_cat: true`.

## 7. Credit

### Quy đổi

```
credit = ceil((token_vào × đơn_giá_vào + token_ra × đơn_giá_ra) / giá_trị_credit)
```

- `giá_trị_credit` = 100đ chi phí AI (chỉnh được). Token ra gồm cả token suy luận.
- Đơn giá theo nhà cung cấp và mô hình, nằm trong bảng giá.
- **Ước tính:** `token_vào` = số ký tự / 3 cộng phần prompt của tác vụ; với tệp, ước lượng theo số tệp và dung lượng. `ra_min`, `ra_max` = hệ số × `token_vào` + hằng số, khai cho từng tác vụ, cộng mức `toi_thieu` của tác vụ.
- `ra_max` đồng thời là trần token ra khi gọi AI, nên chi phí bị chặn trên.
- **Credit trừ thật** = max(`toi_thieu`, theo token thật), không vượt `credit_max` đã giữ. Vượt thì hệ thống chịu.

### Model mặc định: `gemini-3.6-flash`

Đo ngày 09/11/2026 trên Vertex, vùng global. Giá chính thức tính trên 1 triệu token (vào / ra gồm suy luận):

| Model | Giá | Công văn cài 7 lỗi (2 lần) | Ghi chú |
| --- | --- | --- | --- |
| `gemini-3.5-flash` | $1,50 / $9,00 | 5,5–6 lỗi | Có lần tự thêm «UBND HUYỆN YÊN BÌNH» vào bản đề xuất |
| `gemini-3.6-flash` | $0,75 / $3,75, từ 01/01/2027 là $1,50 / $7,50 | 6–6,5 lỗi | Chọn |
| `gemini-3.5-flash-lite` | $0,30 / $2,50 | 3,5–4 lỗi | Loại: bỏ sót lỗi số liệu, nêu quy tắc sai, tự đặt số văn bản và căn cứ không có trong đầu vào |

Mẫu thử nhỏ (một văn bản, hai lần chạy mỗi model), đủ để loại Flash-Lite, chưa đủ để kết luận 3.6 hơn 3.5. Đổi model cũng là đổi `DEFAULT_MODELS`, nên phải đo lại khi đổi.

### Chi phí đo được

Token đo trên `gemini-3.5-flash`, 20 lượt, văn bản mẫu từ Nghị định 30 và số liệu tự tạo. Tính theo giá 3.6 **từ 2027** để không phụ thuộc giá ưu đãi; tỷ giá giả định 26.000đ/USD. Chưa gồm thuế hoặc phí phát sinh trên hóa đơn Google.

| Tác vụ | Khoảng chi phí | Khoảng credit | Trung vị |
| --- | --- | --- | --- |
| `soan-thao` | 317–1.250đ | 4–13 | 9 |
| `kiem-tra` (đến 10.000 ký tự) | 909–1.374đ | 10–14 | 12 |
| `kiem-tra` (37.800 ký tự) | Trên 3.680đ, bị cắt | Trên 37 | — |
| `tham-muu` | 675–1.099đ | 7–11 | 8 |
| `tom-tat` | 373–1.283đ | 4–13 | 10 |
| `bao-cao-dinh-ky` | 805–2.131đ | 9–22 | 15 |
| `trich-xuat` | 249–831đ | 3–9 (1–6 trang) | 3 |

Trung vị toàn bộ 870đ, trung bình 1.028đ, tức khoảng 10 credit một tác vụ. Lượt chạy lâu nhất 47 giây. Token suy luận thường lớn hơn token ra, nên phải tính vào chi phí. Từ các số này đặt hệ số `ra_min`, `ra_max` của từng tác vụ; riêng `kiem-tra` trần token ra phải tăng theo độ dài đầu vào.

### Mức cấp

| Gói | Giá/tháng | Credit/tháng | Khoảng tác vụ | Chi phí AI nếu dùng hết |
| --- | --- | --- | --- | --- |
| free | 0đ | 30 | 3 | 3.000đ |
| starter | 99.000đ | 400 | 40 | 40% doanh thu |
| pro | 249.000đ | 1.100 | 110 | 44% doanh thu |
| business | 599.000đ | 3.000 | 300 | 50% doanh thu |

Gói nạp, dùng trong 90 ngày: 49.000đ = 160, 99.000đ = 350, 199.000đ = 750 (chi phí AI 33–38% doanh thu).

Tới 31/12/2026 chi phí AI bằng khoảng một nửa nhờ giá ưu đãi. Bảng giá hiện tại (150/500/2.000 lượt/tháng) sẽ lỗ khi khách dùng hết: 150 lượt × khoảng 1.000đ đã vượt 99.000đ. Xem lại mức cấp sau tháng đầu có khách thật, dựa trên `tac_vu_chay`.

### Lược đồ

```sql
credit_lo(id, user_id, nguon,          -- 'goi_thang' | 'nap' | 'tang' | 'dieu_chinh'
          so_cap, so_con check (so_con >= 0), het_han,
          plan_id, ky_bat_dau, order_id, tao_luc)
  unique (user_id, plan_id, ky_bat_dau) where nguon = 'goi_thang'

credit_so(id, user_id, lo_id, luot_id,
          loai,                        -- 'cap' | 'giu' | 'tru' | 'hoan' | 'dieu_chinh'
          so, ghi_chu, boi, tao_luc)   -- chỉ ghi thêm, không sửa, không xóa

tac_vu_chay(id, user_id, tac_vu, trang_thai,  -- 'dang_chay' | 'xong' | 'loi'
            credit_min, credit_max, credit_giu, credit_tru,
            provider, model, token_vao, token_ra, ma_loi,
            nguon_goi, phien_ban_ung_dung, bat_dau_luc, xong_luc)
            -- KHÔNG có cột nội dung

credit_bang_gia(loai,                  -- 'tac_vu' | 'mo_hinh' | 'goi_nap' | 'cau_hinh'
                ma, gia_tri jsonb, sua_luc, sua_boi)
                primary key (loai, ma)

tac_vu_luu(luot_id, user_id, du_lieu_ma_hoa bytea, iv, het_han)

subscription_plans + credit_thang int
users              + cho_phep_luu_ho_tro boolean default false
orders             + loai ('goi' | 'nap'), goi_nap_ma; plan_id, cycle thành nullable
                     kèm check: 'goi' phải có plan_id + cycle, 'nap' phải có goi_nap_ma
```

Các bảng mới bật RLS, không có policy, chỉ service role dùng, giống `orders`. Không tạo CSDL mới nên vẫn nằm trong đường sao lưu `supabase-db-*`.

### Hàm Postgres (giao dịch + khóa dòng)

- `credit_cap_ky(user)`: cấp lô tháng của kỳ hiện tại nếu chưa có. Kỳ tính từ ngày bắt đầu gói; `free` tính từ ngày tạo tài khoản. Gói năm vẫn cấp từng tháng. Gói hết hạn thì rơi về `free`, theo `effectivePlanId` hiện có. Lô hết hạn vào cuối kỳ.
- `credit_giu(user, luot, so)`: khóa các lô còn hạn, trừ từ lô **hết hạn sớm nhất**, ghi `giu` theo từng lô. Không đủ thì không đổi gì và báo thiếu.
- `credit_chot(luot, so_that)`: ghi `tru`, trả phần dư về đúng các lô đã giữ.
- `credit_hoan(luot)`: trả toàn bộ về các lô đã giữ.
- `credit_quet_treo(user)`: lượt `dang_chay` quá 15 phút thì hoàn và đánh `loi`.

`credit_cap_ky` và `credit_quet_treo` chạy mỗi khi gọi tác vụ và khi xem số dư, nên không cần lịch chạy.

**Cấp khi thanh toán:** đơn `goi` đã trả làm kỳ mới bắt đầu, lô được cấp ở lượt gọi kế tiếp. Đổi gói giữa kỳ thì lô cũ vẫn dùng được tới hạn. Đơn `nap` đã trả cấp lô 90 ngày trong cùng thao tác xác nhận (webhook SePay và admin), an toàn khi gọi lại.

### Đường cũ `/api/ai/*`

- Bỏ `checkQuota` theo lượt, thay bằng kiểm số dư còn tối thiểu 1 credit.
- Sau khi gọi: trừ theo token thật, tối đa bằng số dư hiện có.
- Dùng khóa riêng của người dùng thì không trừ credit, vì chi phí do người dùng tự trả.
- Tác vụ v2 chỉ dùng khóa hệ thống: thêm tùy chọn vào `resolveAIKey`.

## 8. Lưu tùy chọn để hỗ trợ

- Công tắc `cho_phep_luu_ho_tro` trên trang tài khoản, mặc định tắt.
- Tắt: không có bản ghi `tac_vu_luu`.
- Bật: đầu vào và kết quả mã hóa AES-256-GCM, khóa `TAC_VU_LUU_KHOA` ở biến môi trường prod, `het_han` = 7 ngày.
- Xóa khi hết hạn bằng `pg_cron` nếu CSDL này có; không có thì dùng tác vụ định kỳ gọi một đường nội bộ. Phải kiểm được đã chạy, vì đây là cam kết với khách.
- Quản trị mở xem: ghi nhật ký ai, lúc nào, lượt nào.
- Giới hạn phải ghi trong chính sách: bản mã hóa còn tồn trong bản sao lưu cũ thêm một thời gian sau 7 ngày.
- Mất khóa thì không đọc lại được phần đã lưu. Chấp nhận được, vì đây chỉ là dữ liệu hỗ trợ ngắn hạn.

## 9. Giao diện đi kèm (tối thiểu)

- `/bang-gia`, `/dang-ky`: hiện credit/tháng thay cho lượt/tháng.
- `/tai-khoan`: số dư, hạn dùng, công tắc lưu hỗ trợ.
- `/admin/credit`: sửa bảng giá, cộng/trừ tay (ghi sổ `dieu_chinh`), xem lượt chạy, mở bản lưu.

## 10. Chuyển bản cài máy

**Thứ tự:** `tom-tat` → `kiem-tra` → `tham-muu` → `bao-cao-dinh-ky` → `trich-xuat` → `soan-thao`.

Mỗi tác vụ gồm: chép prompt sang TypeScript, bộ kiểm so khớp, thay lời gọi trong C#, xóa prompt C#.

- Trước `soan-thao`: tách 26 hàm prompt khỏi `AIComposeDialog.xaml.cs` sang lớp thuần trong `AIVanBan.Core`, không đổi hành vi, để sinh được dữ liệu mẫu.
- `trich-xuat` thay luôn đường gọi thẳng Gemini của luồng nhiều tệp.
- **Ngoại lệ của nguyên tắc chép nguyên văn:** phần địa phương ghi cứng trong prompt `soan-thao` chuyển thành đầu vào `dia_phuong` (xã/phường, cơ quan cấp trên, tỉnh), bản cài máy lấy từ thiết lập cơ quan. Không gửi thì không đưa bối cảnh địa phương vào prompt. Bộ kiểm so khớp lấy dữ liệu mẫu có giá trị Gia Kiệm để vẫn so được từng ký tự.
- Sửa nội dung prompt khác (chính quyền hai cấp, `suggested_content` với văn bản dài) là việc riêng sau khi chuyển xong, trừ trần token của `kiem-tra` đã nêu ở mục 7.
- Các hộp thoại AI hiện khoảng credit lấy từ `uoc-tinh` trước khi chạy, và xử lý đủ mã lỗi ở mục 6.
- Bước cuối, trong một bản phát hành: bỏ chế độ gọi thẳng Gemini (`rbGeminiDirect`, dự phòng `_fallbackGeminiKey`, `DevModePolicy`).

## 11. Kiểm thử

**Tự động** (`tsx --test`, nhà cung cấp AI giả):

- `uoc-tinh`: quy đổi, biên, mức tối thiểu.
- Hàm Postgres trên bản sao: giữ, chốt, hoàn; thứ tự lô; 5 yêu cầu đồng thời không tiêu lố; lượt treo; cấp kỳ gọi lại không cấp đôi; `sum(credit_so)` khớp `so_con`.
- Luồng tác vụ: mỗi mã lỗi ở mục 6 đúng HTTP và đúng số credit; `tac_vu_chay` không chứa nội dung; tắt công tắc thì không có `tac_vu_luu`.
- **So khớp prompt:** `AIVanBan.Tests` sinh tệp JSON (đầu vào, system prompt, user prompt) cho mỗi tác vụ và đủ 26 nhánh soạn thảo; bộ kiểm TypeScript so từng ký tự. Lệch một chữ là hỏng.

**Chạy thật, trước khi mở bán:**

1. Đã đo token ngày 09/11/2026 (mục 7). Đo lại trên `gemini-3.6-flash` và văn bản thật của khách trước khi chốt hệ số `ra_min`, `ra_max`.
2. Đã đo 16/11/2026: `kiem-tra` 30.292 ký tự qua Cloudflare + Traefik, máy chủ chạy 49 giây (10.611 token vào, 7.005 token ra), phía gọi nhận HTTP 200 sau 49,3 giây, không bị cắt; `bi_cat` false. Rủi ro còn lại: Cloudflare cắt ở 100 giây; lượt mà AI viết lại toàn văn (tới ~37.000 token ra ở 60.000 ký tự) có thể vượt mức đó — trước khi mở bán chọn: tắt proxy Cloudflare cho `vanbanplus.giakiemso.com`, hoặc đưa streaming / hàng đợi lên sớm.
3. Bản cài máy mới chạy đủ 6 tác vụ trên API thật.

## 12. Triển khai

Đẩy `main` của `vanbanplus-api` là lên production, nên mỗi lần đẩy phải build và bộ kiểm xanh.

1. Migration: thử trên bản sao, rồi prod, **trước khi** đẩy mã cần lược đồ mới.
2. Lớp credit + đường cũ chuyển sang credit + giao diện mục 9. Từ đây người dùng hiện có đã tính bằng credit.
3. Từng tác vụ v2, theo thứ tự mục 10.
4. Bản cài máy mới phát hành sau khi đủ 6 tác vụ, cập nhật qua kênh tự cập nhật hiện có.
5. Cập nhật `PROJECT_STATUS.md`, nguyên tắc «không gọi thẳng Gemini», bài Trợ giúp còn ghi giá cũ.

## 13. Điều kiện trước khi mở bán

Không chặn việc lập trình, nhưng phải xong trước khi nhận tiền thật:

1. **Bỏ đường tự về AI Studio bậc miễn phí cho tác vụ v2.** Hiện Vertex lỗi thì tự chuyển sang khóa miễn phí. Trang giá AI Studio ghi rõ bậc miễn phí «Content used to improve our products», bậc trả phí thì «not used»; văn bản cơ quan nhà nước không được đi đường đó. Đổi lại: khi Vertex lỗi, khách nhận `AI_QUA_TAI` hoặc `AI_LOI` và được hoàn credit.
2. Câu cam kết chỉ ghi đúng phạm vi: «máy chủ VanBanPlus mặc định không lưu văn bản». Nhà cung cấp AI vẫn nhận văn bản khi xử lý.
3. Trang Điều khoản và Chính sách bảo mật đang là link trống: phải có nội dung, gồm cả mục 8.
4. **Có đường trả phí thật trước 18/12/2026.** Vertex hiện chạy bằng credit dùng thử, hết hạn ngày đó và không tự trừ thẻ. Phải nâng tài khoản Google Cloud đang dùng hoặc trả nợ tài khoản `ericphan28`, và đọc điều khoản dữ liệu của loại tài khoản đã chọn.
5. Kiểm tỷ giá và thuế trên hóa đơn Google thực tế, đo lại token trên `gemini-3.6-flash`, rồi xác nhận mức cấp ở mục 7.
6. Việc còn tồn đã ghi: tài khoản nhận tiền, đổi mật khẩu admin, các dòng chưa kiểm chứng trên bảng giá.

## 14. Rủi ro

| Rủi ro | Cách giảm |
| --- | --- |
| Tác vụ dài bị proxy cắt | Đo sớm (mục 11), xử lý trước khi mở bán |
| Người gọi mất kết nối giữa chừng (kể cả do proxy cắt) | Không có tính năng lấy lại kết quả ở bản đầu. Nếu `req.signal` báo đã ngắt lúc chốt: hoàn hết credit, hệ thống chịu chi phí AI, ghi `ma_loi = NGAT_KET_NOI`. Không phát hiện được thì lượt vẫn trừ. Theo dõi `tac_vu_chay`, nếu hay gặp thì ưu tiên hàng đợi |
| Prompt chép lệch | Bộ kiểm so khớp từng ký tự |
| Mức credit sai, lỗ hoặc quá đắt | Bảng giá chỉnh được; xem token thật trong `tac_vu_chay` theo từng tác vụ |
| Bản cài máy cũ gọi đường cũ lâu dài | Đường cũ vẫn trừ credit; xem `phien_ban_ung_dung` trước khi gỡ |
| Phiên khác đang sửa cùng `vanbanplus-api` | Kiểm `git status` và `git log` trước mỗi đợt làm |
