# 🏛️ AI VĂN BẢN PLUS — Phân tích nghiệp vụ & Lộ trình AI

> **Phiên bản:** 1.0  
> **Ngày tạo:** 11/02/2026  
> **Mục tiêu:** Tích hợp AI giải quyết nghiệp vụ thực tế hàng ngày của chuyên viên, cán bộ xã/phường  

---

## 📋 MỤC LỤC

1. [Khảo sát nghiệp vụ thực tế](#1-khảo-sát-nghiệp-vụ-thực-tế)
2. [Nỗi đau hiện tại](#2-nỗi-đau-hiện-tại)
3. [Tính năng AI đã có (Phase 1)](#3-tính-năng-ai-đã-có-phase-1)
4. [Đề xuất tính năng AI mới](#4-đề-xuất-tính-năng-ai-mới)
5. [Chi tiết từng tính năng](#5-chi-tiết-từng-tính-năng)
6. [Lộ trình triển khai](#6-lộ-trình-triển-khai)
7. [Nguyên tắc thiết kế UI](#7-nguyên-tắc-thiết-kế-ui)
8. [Tracking tiến độ](#8-tracking-tiến-độ)

---

## 1. Khảo sát nghiệp vụ thực tế

### 1.1. Đối tượng người dùng

| Vai trò | Số lượng (1 xã) | Công việc chính |
|---------|-----------------|-----------------|
| Văn thư | 1–2 người | Tiếp nhận, vào sổ, phát hành VB |
| Chuyên viên VPUB | 2–4 người | Soạn thảo, tham mưu, tổng hợp |
| Kế toán | 1–2 người | Tờ trình, quyết toán, báo cáo tài chính |
| CB Tư pháp | 1 người | Hộ tịch, chứng thực, văn bản pháp lý |
| CB Địa chính | 1 người | Đất đai, xây dựng, môi trường |
| CB VHXH | 1–2 người | Văn hóa, y tế, giáo dục, LĐTBXH |
| Lãnh đạo (PCT, CT) | 2–3 người | Ký duyệt, bút phê, chỉ đạo |

### 1.2. Một ngày làm việc điển hình

#### 🕖 Buổi sáng (7h30 – 11h30)

| Thời gian | Công việc | Chi tiết |
|-----------|-----------|----------|
| 7h30–8h00 | Tiếp nhận VB đến | Nhận từ bưu điện, email huyện/tỉnh, scan, người dân nộp |
| 8h00–8h30 | Vào sổ, phân loại | Ghi sổ công văn đến, phân loại lĩnh vực, trình lãnh đạo |
| 8h30–9h00 | Lãnh đạo bút phê | Chuyển chuyên viên xử lý, ghi deadline |
| 9h00–11h30 | **Soạn thảo văn bản** | Chiếm phần lớn thời gian, là nỗi đau lớn nhất |

#### 🕐 Buổi chiều (13h30 – 17h00)

| Thời gian | Công việc | Chi tiết |
|-----------|-----------|----------|
| 13h30–15h00 | Tiếp tục soạn thảo | Hoàn thiện VB, sửa theo ý kiến lãnh đạo |
| 15h00–16h00 | Trình ký | In, trình lãnh đạo ký → hay bị trả lại |
| 16h00–16h30 | Sửa, trình lại | Sửa theo bút phê, trình lại |
| 16h30–17h00 | Phát hành | Đóng dấu, gửi đi, lưu hồ sơ |

### 1.3. Các loại văn bản thường xuyên

| Loại VB | Tần suất/tháng | Ai soạn | Độ khó |
|---------|---------------|---------|--------|
| **Công văn** (đề nghị, trả lời, đôn đốc) | 20–40 | Mọi CV | ⭐⭐ |
| **Báo cáo** (tuần, tháng, quý, năm, đột xuất) | 8–15 | Mọi CV | ⭐⭐⭐ |
| **Quyết định** (nhân sự, khen thưởng, phê duyệt) | 5–15 | CV chuyên môn | ⭐⭐⭐ |
| **Tờ trình** (xin ý kiến, đề xuất KP) | 5–10 | CV chuyên môn | ⭐⭐⭐⭐ |
| **Thông báo** (họp, kết quả, nội bộ) | 10–20 | Văn thư/CV | ⭐ |
| **Giấy mời** | 5–10 | Văn thư | ⭐ |
| **Kế hoạch** (công tác, sự kiện, kiểm tra) | 3–8 | CV chuyên môn | ⭐⭐⭐⭐ |
| **Biên bản** (họp, kiểm tra, vi phạm) | 4–8 | CV ghi chép | ⭐⭐ |
| **Nghị quyết** (HĐND, Đảng ủy) | 1–3 | CV tổng hợp | ⭐⭐⭐⭐⭐ |

### 1.4. Quy trình xử lý văn bản đến

```
VB đến (giấy/email/scan)
    │
    ▼
[Văn thư tiếp nhận]
    │ Vào sổ, đánh số đến
    ▼
[Trình lãnh đạo bút phê]
    │ Ai xử lý? Deadline?
    ▼
[Chuyển chuyên viên phụ trách]
    │
    ├── Nếu cần trả lời → Soạn dự thảo CV trả lời
    ├── Nếu cần triển khai → Soạn KH, QĐ, TB
    ├── Nếu cần báo cáo → Soạn BC theo yêu cầu
    └── Nếu chỉ để biết → Lưu hồ sơ
    │
    ▼
[Trình ký] → Bị trả lại 30-50% lần đầu
    │
    ▼
[Phát hành] → Đóng dấu, gửi, lưu
```

---

## 2. Nỗi đau hiện tại

### 2.1. Top 5 nỗi đau lớn nhất

| # | Nỗi đau | Mức độ | Tần suất | Hậu quả |
|---|---------|--------|----------|---------|
| 1 | **Soạn thảo chậm** — không biết bắt đầu từ đâu, tìm mẫu cũ mất thời gian | 🔴 Rất cao | Hàng ngày | Trễ deadline, OT |
| 2 | **Bị trả VB** — sai thể thức, sai căn cứ, văn phong không chuẩn | 🔴 Rất cao | 30–50% VB | Mất uy tín, tốn thời gian |
| 3 | **Không biết căn cứ pháp lý** — luật nào, NĐ/TT nào, đã hết hiệu lực chưa | 🟡 Cao | Mỗi VB | Căn cứ sai → VB vô giá trị |
| 4 | **VB đến dài, đọc không kịp** — 10-20 trang, nhiều VB/ngày | 🟡 Cao | Hàng ngày | Bỏ sót yêu cầu, trễ deadline |
| 5 | **Tìm VB cũ khó** — "năm ngoái soạn cái tương tự mà tìm không ra" | 🟡 Cao | 2–3 lần/tuần | Copy-paste quên sửa → scandal |

### 2.2. Lỗi thường gặp khi soạn văn bản

| Lỗi | Ví dụ | % VB mắc lỗi |
|-----|-------|--------------|
| **Sai thể thức** (NĐ 30/2020) | Sai font, sai cỡ chữ, sai vị trí quốc hiệu | 40% |
| **Căn cứ pháp lý sai/hết hiệu lực** | Trích TT 01/2011 (đã bị thay bởi TT 04/2023) | 25% |
| **Văn phong không chuẩn HC** | "kính mong", "xin phép" thay vì "đề nghị" | 30% |
| **Chính tả, ngữ pháp** | Typo, thiếu dấu, câu tối nghĩa | 20% |
| **Nơi nhận thiếu/sai** | Quên gửi cơ quan liên quan, gửi nhầm cấp | 15% |
| **Số/ký hiệu sai format** | "CV số 01" thay vì "Số: 01/UBND-VP" | 10% |

---

## 3. Tính năng AI đã có (Phase 1) ✅

| # | Tính năng | Mô tả | Entry point | Service |
|---|-----------|-------|-------------|---------|
| F1 | **AI Soạn văn bản** | Chọn loại VB → điền thông tin → AI tạo nội dung hoàn chỉnh | Sidebar "AI Tạo văn bản" → AIComposeDialog | `GenerateContentAsync()` |
| F2 | **AI Kiểm tra văn bản** | Kiểm tra chính tả, văn phong HC, xung đột, đề xuất sửa | Preview panel → DocumentReviewDialog | `DocumentReviewService.ReviewAsync()` |
| F3 | **AI Trích xuất scan/PDF** | Upload scan → AI đọc + điền tự động tất cả fields | Toolbar "Nhập từ scan" → ScanImportDialog | `ExtractDocumentFromFileAsync()` |
| F4 | **17 mẫu kịch bản soạn sẵn** | Công văn, QĐ, BC, Tờ trình, KH, TB, NQ... | AIComposeDialog ComboBox | — |

### Đánh giá Phase 1 vs nỗi đau:

| Nỗi đau | Phase 1 giải quyết? | Mức độ |
|---------|---------------------|--------|
| Soạn thảo chậm | ✅ AI Soạn (F1) | 70% — cần thêm mẫu thực tế |
| Bị trả VB | ✅ AI Kiểm tra (F2) | 50% — cần nâng cấp kiểm tra NĐ30 |
| Không biết căn cứ | ❌ Chưa có | 0% |
| VB đến dài, đọc không kịp | ❌ Chưa có | 0% |
| Tìm VB cũ khó | ❌ Chưa có | 0% |

---

## 4. Đề xuất tính năng AI mới

### 4.1. Ma trận ưu tiên

```
           Giá trị nghiệp vụ ↑
           │
     CAO   │  P2 Tra cứu      P1 Tham mưu
           │  căn cứ PL        xử lý VB đến
           │
     VỪA   │  P5 BC định kỳ   P4 Kiểm tra NĐ30
           │                   (nâng cấp F2)
           │
     THẤP  │  P8 Dịch VB      P7 Biên bản họp
           │  P9 Gợi ý nơi nhận
           ├────────────────────────────→
              CAO              THẤP
                    Effort →
```

### 4.2. Danh sách tính năng đề xuất

#### 🔴 Nhóm A — Ưu tiên cao (giải quyết nỗi đau hàng ngày)

| ID | Tên | Giải quyết nỗi đau | Effort | UI |
|----|-----|---------------------|--------|----|
| **P3** | **AI Tóm tắt văn bản** | #4 VB dài đọc không kịp | ⭐ Thấp | Inline trong Preview panel |
| **P4** | **AI Kiểm tra NĐ30** (nâng cấp) | #2 Bị trả VB | ⭐⭐ Vừa | Nâng cấp DocumentReviewDialog |
| **P2** | **AI Tra cứu căn cứ pháp lý** | #3 Không biết căn cứ | ⭐⭐ Vừa | Popup trong form soạn VB |

#### 🟡 Nhóm B — Ưu tiên vừa (nâng cao năng suất)

| ID | Tên | Giải quyết nỗi đau | Effort | UI |
|----|-----|---------------------|--------|----|
| **P1** | **AI Tham mưu xử lý VB đến** | #4 + quy trình xử lý | ⭐⭐⭐ Cao | Dialog mới hoặc mở rộng ScanImport |
| **P6** | **AI Tìm VB tương tự** | #5 Tìm VB cũ khó | ⭐⭐ Vừa | Search box nâng cấp |
| **P5** | **AI Soạn báo cáo định kỳ** | #1 Soạn chậm (BC phức tạp) | ⭐⭐⭐ Cao | Mẫu mới trong AIComposeDialog |

#### 🟢 Nhóm C — Tiện ích bổ sung

| ID | Tên | Mô tả | Effort |
|----|-----|-------|--------|
| **P7** | **AI Soạn biên bản + kết luận họp** | Từ ghi chú → biên bản chuẩn | ⭐⭐ Vừa |
| **P8** | **AI Dịch văn bản** | Việt ↔ Anh cho VB đối ngoại | ⭐ Thấp |
| **P9** | **AI Gợi ý nơi nhận** | Dựa nội dung → gợi ý nơi nhận | ⭐ Thấp |

---

## 5. Chi tiết từng tính năng

### 5.1. P3 — AI Tóm tắt văn bản ⭐ TRIỂN KHAI ĐẦU TIÊN

#### Mô tả nghiệp vụ
Chuyên viên nhận công văn dài 5–20 trang từ huyện/tỉnh. Cần nhanh chóng nắm được:
- Nội dung chính nói gì?
- Yêu cầu mình phải làm gì?
- Deadline bao giờ?
- Liên quan đến lĩnh vực gì?

#### Đầu vào
- Nội dung văn bản (text) — lấy từ document đã lưu trong DB

#### Đầu ra (JSON)
```json
{
  "summary": "Công văn số 123/UBND-VP yêu cầu các xã báo cáo tiến độ xây dựng nông thôn mới giai đoạn 2024-2025, bao gồm kết quả đạt được, khó khăn vướng mắc và đề xuất kiến nghị.",
  "action_items": [
    "Lập báo cáo tiến độ NTM theo mẫu đính kèm",
    "Cử 02 cán bộ tham gia lớp bồi dưỡng ngày 20/3",
    "Tổ chức rà soát các tiêu chí chưa đạt"
  ],
  "deadlines": [
    { "task": "Gửi báo cáo về huyện", "date": "15/03/2026" },
    { "task": "Cử CB tham gia bồi dưỡng", "date": "18/03/2026" }
  ],
  "priority": "high",
  "related_field": "Nông thôn mới",
  "document_type_hint": "Công văn chỉ đạo"
}
```

#### UI — Tích hợp vào Preview Panel (DocumentListPage)
- **Vị trí:** Thêm 1 card "📌 TÓM TẮT AI" ngay sau docInfoCard
- **Trạng thái ban đầu:** Ẩn, chỉ hiện nút "📌 Tóm tắt" ở action bar
- **Click nút:** Gọi AI → hiện loading trong card → hiện kết quả
- **Cache:** Lưu tạm kết quả theo documentId, không gọi lại nếu đã có

#### Service method cần thêm
```
DocumentSummaryService.SummarizeAsync(string content, string documentType)
→ trả về DocumentSummary model
```

#### Prompt AI (hướng)
```
Bạn là chuyên viên văn phòng UBND xã. Phân tích văn bản sau và trả về JSON:
- summary: Tóm tắt ngắn gọn (3-5 câu)
- action_items: Các việc cần thực hiện (mảng string)  
- deadlines: Các mốc thời gian quan trọng (mảng {task, date})
- priority: high/medium/low
- related_field: Lĩnh vực liên quan
```

---

### 5.2. P4 — Nâng cấp AI Kiểm tra theo NĐ 30/2020

#### Mô tả nghiệp vụ
DocumentReviewDialog hiện kiểm tra: chính tả, văn phong, xung đột, logic.
Cần bổ sung kiểm tra **thể thức văn bản** theo NĐ 30/2020/NĐ-CP:

#### Các hạng mục kiểm tra NĐ 30 cần thêm

| Hạng mục | Quy định | Lỗi thường gặp |
|----------|----------|-----------------|
| Quốc hiệu, tiêu ngữ | Font Times New Roman, cỡ 12-13, in hoa | Sai font, sai cỡ chữ |
| Tên cơ quan ban hành | In hoa, đậm, căn giữa | Thiếu cơ quan chủ quản |
| Số, ký hiệu | Format: Số: XX/Loại-Đơnvị | Sai format, thiếu năm |
| Địa danh, ngày tháng | "Hà Nội, ngày 15 tháng 3 năm 2026" | Viết tắt, sai format |
| Tên loại + trích yếu | Phù hợp với nội dung | Trích yếu quá dài/ngắn |
| Nơi nhận | Đủ các cơ quan liên quan | Thiếu "Lưu: VT" |
| Chức danh người ký | Đúng thẩm quyền | Ký thay không đúng |

#### UI
- Nâng cấp DocumentReviewDialog: thêm category "Thể thức NĐ30" trong danh sách issues
- Thêm icon/badge riêng cho lỗi thể thức

#### Effort
- Chỉ cần nâng cấp system prompt của `DocumentReviewService`
- Thêm category mới vào model + UI badge

---

### 5.3. P2 — AI Tra cứu căn cứ pháp lý

#### Mô tả nghiệp vụ
Khi soạn văn bản, phần "Căn cứ" là khó nhất:
- Phải biết luật/NĐ/TT nào liên quan
- Phải kiểm tra còn hiệu lực không
- Phải trích dẫn đúng format

#### Đầu vào
- Loại văn bản (Quyết định, Tờ trình, KH...)
- Lĩnh vực (Đất đai, Tài chính, Nhân sự...)
- Nội dung/mục đích VB

#### Đầu ra
```json
{
  "legal_bases": [
    {
      "name": "Luật Tổ chức chính quyền địa phương năm 2015 (sửa đổi, bổ sung năm 2019)",
      "article": "Khoản 2 Điều 28",
      "status": "active",
      "relevance": "Thẩm quyền ban hành QĐ của UBND cấp xã"
    },
    {
      "name": "Thông tư 01/2011/TT-BNV",
      "status": "expired",
      "replaced_by": "Thông tư 04/2023/TT-BNV",
      "warning": "⚠️ Đã hết hiệu lực từ 15/8/2023"
    }
  ],
  "formatted_text": "Căn cứ Luật Tổ chức chính quyền địa phương ngày 19 tháng 6 năm 2015; Luật sửa đổi, bổ sung một số điều của Luật Tổ chức Chính phủ và Luật Tổ chức chính quyền địa phương ngày 22 tháng 11 năm 2019;\nCăn cứ Nghị định số 30/2020/NĐ-CP ngày 05 tháng 3 năm 2020 của Chính phủ về công tác văn thư;..."
}
```

#### UI
- Nút "⚖️ Gợi ý căn cứ" trong AIComposeDialog (cạnh nút Tạo)
- Hoặc popup nhỏ khi focus vào field "Căn cứ" trong form soạn/sửa VB

#### Lưu ý
- AI có thể hallucinate về luật → cần disclaimer: "Vui lòng kiểm tra lại trước khi sử dụng"
- Nên có DB/cache các luật phổ biến cấp xã để prompt chính xác hơn

---

### 5.4. P1 — AI Tham mưu xử lý văn bản đến

#### Mô tả nghiệp vụ
Khi nhận VB đến (scan/PDF/text), thay vì chỉ trích xuất metadata (F3), AI còn:
- Phân tích nội dung → đề xuất ai xử lý
- Xác định deadline
- Soạn sẵn dự thảo trả lời (nếu cần)
- Gợi ý VB liên quan trong DB

#### Đầu vào
- Nội dung VB đến (đã OCR hoặc nhập tay)
- Danh sách bộ phận/CB trong xã (lấy từ config)

#### Đầu ra
```json
{
  "summary": "CV yêu cầu báo cáo tiến độ NTM...",
  "suggested_handler": "CB Địa chính - Nông nghiệp",
  "deadline": "15/03/2026",
  "response_needed": true,
  "response_type": "Báo cáo",
  "draft_response": "... dự thảo báo cáo ...",
  "related_documents": ["CV 45/UBND-VP ngày 10/1/2026"]
}
```

#### UI
- Mở rộng ScanImportDialog: sau khi trích xuất, thêm tab/section "Tham mưu xử lý"
- Hoặc dialog mới mở từ nút trong Preview panel

#### Effort: Cao
- Cần config danh sách bộ phận/CB
- Cần search VB liên quan trong DB (semantic search hoặc keyword)

---

### 5.5. P6 — AI Tìm văn bản tương tự

#### Mô tả nghiệp vụ
"Năm ngoái mình soạn công văn xin kinh phí sửa trường học, tìm lại để tham khảo"

#### Cách tiếp cận
- **Đơn giản (Phase đầu):** Dùng AI tạo keywords từ mô tả → search text trong DB
- **Nâng cao (sau):** Embedding + vector search

#### UI
- Nâng cấp ô tìm kiếm hiện có: thêm toggle "🔍 Tìm thường" / "🤖 Tìm AI"
- Hoặc nút "Tìm VB tương tự" trong preview panel (dựa vào VB đang chọn)

---

### 5.6. P5 — AI Soạn báo cáo định kỳ

#### Mô tả nghiệp vụ
Mỗi tháng/quý/năm, chuyên viên phải tổng hợp báo cáo từ nhiều nguồn:
- Số liệu từ các bộ phận
- So sánh với kỳ trước
- Đánh giá, kiến nghị

#### Đầu vào
- Loại BC (tuần/tháng/quý/năm)
- Lĩnh vực
- Số liệu (nhập tay hoặc copy-paste)
- BC kỳ trước (nếu có, từ DB)

#### Đầu ra
- Báo cáo hoàn chỉnh theo mẫu
- Có bảng số liệu so sánh
- Có phần đánh giá + kiến nghị

#### UI
- Thêm template "Báo cáo định kỳ" vào AIComposeDialog
- Form nhập số liệu theo bảng

---

### 5.7. P7 — AI Soạn biên bản + kết luận họp

#### Mô tả nghiệp vụ
Sau mỗi cuộc họp, cần:
- Biên bản ghi đầy đủ nội dung
- Kết luận cuộc họp (quan trọng hơn)
- Phân công nhiệm vụ

#### UI
- Tích hợp vào module Cuộc họp đã có
- Nút "AI Soạn biên bản" trong MeetingEditDialog

---

### 5.8. P8 — AI Dịch văn bản

#### Mô tả
Một số xã biên giới, có hợp tác quốc tế cần dịch VB Việt ↔ Anh.

#### UI
- Nút "Dịch" trong preview panel hoặc dialog riêng

---

### 5.9. P9 — AI Gợi ý nơi nhận

#### Mô tả
Dựa vào loại VB + nội dung → gợi ý danh sách nơi nhận phù hợp.

#### UI
- Auto-suggest khi focus vào field "Nơi nhận" trong form soạn/sửa

---

## 6. Lộ trình triển khai

### Phase 2: Tóm tắt & Kiểm tra nâng cao (2–3 tuần)

| Tuần | Task | Chi tiết | Deliverable |
|------|------|----------|-------------|
| 1 | **P3 — AI Tóm tắt VB** | Model + Service + UI inline | Nút "Tóm tắt" trong preview |
| 2 | **P4 — Nâng cấp AI Kiểm tra** | Bổ sung kiểm tra NĐ30 | Category mới trong Review |
| 2 | Bug fix & polish | Sửa lỗi, cải thiện prompt | Stable release |

### Phase 3: Tra cứu & Tìm kiếm (2–3 tuần)

| Tuần | Task | Chi tiết | Deliverable |
|------|------|----------|-------------|
| 3 | **P2 — Tra cứu căn cứ PL** | Service + popup UI | Nút "Gợi ý căn cứ" |
| 4 | **P6 — Tìm VB tương tự** | AI keyword search | Tìm kiếm AI trong search box |
| 5 | Polish & test | Kiểm thử với VB thực | Stable release |

### Phase 4: Tham mưu & Báo cáo (3–4 tuần)

| Tuần | Task | Chi tiết | Deliverable |
|------|------|----------|-------------|
| 6–7 | **P1 — Tham mưu xử lý VB đến** | Full dialog, config bộ phận | Tham mưu từ scan |
| 8–9 | **P5 — BC định kỳ** | Template mới, nhập số liệu | BC tháng/quý/năm |

### Phase 5: Tiện ích bổ sung (2 tuần)

| Tuần | Task |
|------|------|
| 10 | P7 Biên bản họp, P9 Gợi ý nơi nhận |
| 11 | P8 Dịch VB, polish toàn bộ |

---

## 7. Nguyên tắc thiết kế UI

### 7.1. Nguyên tắc chung

| # | Nguyên tắc | Giải thích |
|---|-----------|-----------|
| 1 | **Không thêm page/menu nếu có thể** | Tích hợp vào chỗ đang có, tránh rối |
| 2 | **Lazy load AI** | Chỉ gọi AI khi user bấm, không tự động → tiết kiệm quota |
| 3 | **Cache kết quả** | Đã tóm tắt/kiểm tra rồi → lưu tạm, không gọi lại |
| 4 | **Loading state rõ ràng** | Spinner + text "Đang phân tích..." → user biết đang chờ |
| 5 | **Error state thân thiện** | Lỗi API → hiện message + nút Thử lại, không crash |
| 6 | **Disclaimer AI** | Kết quả AI để tham khảo, cần kiểm tra lại |
| 7 | **Không lộ tên công nghệ** | Không hiện "Gemini", "API Key" cho end user |

### 7.2. Vị trí tích hợp UI cho từng tính năng

```
┌──────────────────────────────────────────────────────┐
│                    MAIN WINDOW                        │
│ ┌──────┐ ┌──────────────────────────────────────────┐│
│ │      │ │ DocumentListPage                         ││
│ │ SIDE │ │ ┌─────────────────────────────────────┐  ││
│ │ BAR  │ │ │ Toolbar: [Thêm] [📸Scan] [Refresh]  │  ││
│ │      │ │ ├─────────────────────────────────────┤  ││
│ │ Nav  │ │ │ Filter: [Loại VB] [Năm] [Tìm kiếm] │  ││
│ │ Menu │ │ │                    ↑ P6: AI Tìm      │  ││
│ │      │ │ ├──────┬────────────┬────────────────┤  ││
│ │      │ │ │ Tree │ DataGrid   │ Preview Panel  │  ││
│ │      │ │ │      │            │                │  ││
│ │ [AI  │ │ │      │            │ [docInfo]      │  ││
│ │ Tạo  │ │ │      │            │ [P3: Tóm tắt] │  ││
│ │ VB]  │ │ │      │            │ [noiNhan]      │  ││
│ │  ↓   │ │ │      │            │ [canCu]        │  ││
│ │  AI  │ │ │      │            │ [noiDung]      │  ││
│ │ Com- │ │ │      │            ├────────────────┤  ││
│ │ pose │ │ │      │            │ [📌Tóm tắt]    │  ││
│ │ Dia- │ │ │      │            │ [🔍Kiểm tra]   │  ││
│ │ log  │ │ │      │            │ [Sửa][Mở][Xóa]│  ││
│ │  ↓   │ │ └──────┴────────────┴────────────────┘  ││
│ │ P2:  │ │                                          ││
│ │ Gợi  │ └──────────────────────────────────────────┘│
│ │ ý    │                                             │
│ │ căn  │ DocumentReviewDialog (popup)                │
│ │ cứ   │  └─ P4: Thêm category "Thể thức NĐ30"     │
│ │      │                                             │
│ └──────┘ ScanImportDialog (popup)                    │
│           └─ P1: Thêm section "Tham mưu xử lý"     │
└──────────────────────────────────────────────────────┘
```

### 7.3. Bảng màu cho các nút AI

| Nút | Màu | Hex | Lý do |
|-----|------|-----|-------|
| 📌 Tóm tắt | Teal | `#00796B` | Thông tin, passive |
| 🔍 Kiểm tra | Blue | `#1565C0` | Action chính, đã có |
| ⚖️ Căn cứ PL | Purple | `#6A1B9A` | Pháp lý, nghiêm túc |
| 🤖 Tham mưu | Orange | `#E65100` | Chú ý, quan trọng |
| 🔎 Tìm AI | Green | `#2E7D32` | Tìm kiếm, khám phá |

---

## 8. Tracking tiến độ

### Phase 2 — AI Tóm tắt + Kiểm tra NĐ30

| # | Task | Status | Files |
|---|------|--------|-------|
| 2.1 | Tạo model `DocumentSummary` | ⬜ Chưa | `AIVanBan.Core/Models/DocumentSummary.cs` |
| 2.2 | Tạo service `DocumentSummaryService` | ⬜ Chưa | `AIVanBan.Core/Services/DocumentSummaryService.cs` |
| 2.3 | Thêm nút "📌 Tóm tắt" vào preview actions | ⬜ Chưa | `DocumentListPage.xaml` |
| 2.4 | Thêm card Tóm tắt AI vào preview panel | ⬜ Chưa | `DocumentListPage.xaml` |
| 2.5 | Code-behind: gọi AI, hiện kết quả, cache | ⬜ Chưa | `DocumentListPage.xaml.cs` |
| 2.6 | Nâng cấp prompt kiểm tra NĐ30 | ⬜ Chưa | `DocumentReviewService.cs` |
| 2.7 | Thêm category "Thể thức" vào ReviewDialog | ⬜ Chưa | `DocumentReviewDialog.xaml` |
| 2.8 | Test với VB thực tế | ⬜ Chưa | — |

### Phase 3–5

> Sẽ chi tiết hóa khi Phase 2 hoàn thành.

---

## Phụ lục

### A. Tham khảo NĐ 30/2020/NĐ-CP — Thể thức văn bản

| Thành phần | Quy định chính |
|-----------|----------------|
| Quốc hiệu | "CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM" — Times New Roman, cỡ 12-13, in hoa, đậm |
| Tiêu ngữ | "Độc lập - Tự do - Hạnh phúc" — cỡ 13-14, in thường, đậm |
| Tên CQ ban hành | In hoa, đậm, gạch chân |
| Số/ký hiệu | "Số: XX/Loại VB-Chữ viết tắt đơn vị" |
| Địa danh + ngày | Đúng tên hành chính, viết đầy đủ |
| Tên loại VB | 29 loại theo NĐ30, in hoa, đậm |
| Trích yếu | Ngắn gọn, phản ánh nội dung chính |
| Nội dung | Font Times New Roman, cỡ 13-14 |
| Nơi nhận | Liệt kê đầy đủ, có "Lưu: VT, ..." |
| Ký tên | Chức vụ + họ tên, đóng dấu |

### B. 29 loại văn bản hành chính (NĐ 30/2020)

1. Nghị quyết (cá biệt)
2. Quyết định (cá biệt)
3. Chỉ thị
4. Quy chế
5. Quy định
6. Thông cáo
7. Thông báo
8. Hướng dẫn
9. Chương trình
10. Kế hoạch
11. Phương án
12. Đề án
13. Dự án
14. Báo cáo
15. Biên bản
16. Tờ trình
17. Hợp đồng
18. Công văn
19. Công điện
20. Bản ghi nhớ
21. Bản thỏa thuận
22. Giấy ủy quyền
23. Giấy mời
24. Giấy giới thiệu
25. Giấy nghỉ phép
26. Phiếu gửi
27. Phiếu chuyển
28. Phiếu báo
29. Thư công

### C. Prompt guideline cho AI

> Mọi prompt gửi AI đều phải tuân thủ:
> 1. Vai trò: "Bạn là chuyên viên văn phòng UBND cấp xã/phường tại Việt Nam"
> 2. Ngôn ngữ: Tiếng Việt, văn phong hành chính chuẩn
> 3. Chuẩn: Theo NĐ 30/2020/NĐ-CP về công tác văn thư
> 4. Output: JSON có schema rõ ràng, dễ parse
> 5. Disclaimer: Kết quả AI để tham khảo, cần kiểm tra lại
