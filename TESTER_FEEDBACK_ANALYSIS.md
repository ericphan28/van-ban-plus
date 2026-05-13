# 📋 PHÂN TÍCH FEEDBACK TESTER — VanBanPlus v1.0.14

> **Ngày phân tích:** 2026-03-31  
> **Nguồn:** Feedback tester từ buổi kiểm thử  
> **Trạng thái:** Đang phân tích & đề xuất cải tiến

---

## 📊 TỔNG HỢP CÁC VẤN ĐỀ

| # | Vấn đề | Mức độ | Trạng thái hiện tại | Đề xuất |
|---|--------|--------|---------------------|---------|
| 1 | AI Kiểm tra VB: "Soạn tiếp" chưa rõ flow | 🟡 UX | Đã code nhưng UX mơ hồ | Cải tiến UX + hướng dẫn |
| 2 | Xuất Word đã đúng format nhưng VB chưa rõ trạng thái | 🟡 UX | Đã có trạng thái | Cải tiến hiển thị + auto-status |
| 3 | Load dữ liệu mẫu lên lâu khi soạn thảo xong | 🔴 Performance | Chưa tối ưu | Cache + lazy load |
| 4 | Mẫu cuộc họp: Ngày/Tháng chưa được tối ưu | 🟢 Minor | Done nhưng cần polish | Tối ưu date picker |
| 5 | Trạng thái VB chưa rõ duyệt như thế nào | 🔴 UX/Flow | Có 7 trạng thái nhưng thiếu hướng dẫn | Visual workflow + guide |

---

## 🔍 CHI TIẾT TỪNG VẤN ĐỀ

---

### VẤN ĐỀ #1: AI Kiểm tra VB — Nút "Soạn tiếp" chưa rõ flow

**Mô tả từ tester:**
> *"Xuất word đã format nhưng phần soạn tiếp văn bản thì chưa rõ nó vận hành như nào"*

**Hiện trạng code:**
- File: `AIVanBan.Desktop/Views/DocumentReviewDialog.xaml.cs` — region P7
- Sau khi AI kiểm tra xong, hiện 2 nút:
  - **📄 Xuất Word** → mở SaveFileDialog → xuất .docx chuẩn NĐ 30/2020 ✅ Hoạt động tốt
  - **✏️ Soạn tiếp** → hiện MessageBox confirm → đóng ReviewDialog → mở AIComposeDialog mới với nội dung đã sửa
- `AIComposeDialog.SetPrefilledContent()` → điền text vào RichTextBox

**Vấn đề cụ thể:**
1. **User không hiểu "Soạn tiếp" là gì** — Nút tên "Soạn tiếp" mơ hồ, không biết sẽ chuyển đi đâu
2. **MessageBox confirm thừa** — Phải bấm Yes/No trước khi chuyển, gây khó chịu
3. **Mất ngữ cảnh** — Khi mở AIComposeDialog mới, user không biết đang ở đâu trong flow
4. **Không có hướng dẫn** — Sau khi chuyển sang, user không biết bước tiếp theo (lưu? xuất? sửa gì?)
5. **Nội dung chỉ dạng text thuần** — Khi paste vào RichTextBox, mất hết structure/heading

**🔧 ĐỀ XUẤT CẢI TIẾN:**

| # | Cải tiến | Chi tiết | Effort |
|---|---------|---------|--------|
| 1.1 | **Đổi tên nút** | "✏️ Soạn tiếp" → "✏️ Chỉnh sửa & Lưu vào hệ thống" — rõ mục đích hơn | ⭐ Thấp |
| 1.2 | **Bỏ MessageBox confirm** | Chuyển thẳng sang AIComposeDialog, không hỏi lại | ⭐ Thấp |
| 1.3 | **Thêm tooltip chi tiết** | Tooltip: "Mở trình soạn thảo AI để chỉnh sửa nội dung, sau đó lưu thành văn bản mới hoặc xuất Word" | ⭐ Thấp |
| 1.4 | **Thêm banner hướng dẫn** trong AIComposeDialog | Khi mở từ Review → hiện banner vàng: "📌 Nội dung từ AI Kiểm tra. Bạn có thể: chỉnh sửa → Lưu VB hoặc Xuất Word" | ⭐⭐ Vừa |
| 1.5 | **Thêm option #3: Lưu thẳng thành VB mới** | Nút "💾 Lưu thành VB mới" — tạo Document mới với nội dung đã sửa, không cần mở Compose | ⭐⭐ Vừa |
| 1.6 | **Panel tổng hợp 3 options** | Thay vì 3 nút cạnh nhau → hiện panel đẹp giải thích rõ 3 con đường | ⭐⭐ Vừa |

**Mockup panel đề xuất (sau khi kiểm tra xong):**
```
┌─────────────────────────────────────────────┐
│  ✅ Kiểm tra hoàn tất — Điểm: 8/10         │
│                                              │
│  Bạn muốn làm gì với nội dung đã sửa?      │
│                                              │
│  [📄 Xuất Word]     Tải file .docx chuẩn    │
│                      NĐ 30/2020 về máy       │
│                                              │
│  [✏️ Chỉnh sửa]    Mở trình soạn thảo AI   │
│                      để sửa thêm, lưu VB     │
│                                              │
│  [💾 Lưu VB mới]   Tạo văn bản mới ngay     │
│                      trong hệ thống           │
│                                              │
│  [📋 Copy]  [❌ Đóng]                        │
└─────────────────────────────────────────────┘
```

---

### VẤN ĐỀ #2: Xuất Word đã format nhưng trạng thái VB chưa rõ

**Mô tả từ tester:**
> *"Xuất ra file word đã đúng dạng format, trạng thái văn bản vẫn..."*

**Hiện trạng code:**
- File: `AIVanBan.Core/Services/WordExportService.cs` — `ExportContent()` method
- Xuất Word chuẩn NĐ 30/2020 ✅ đã fix OpenXML ordering
- **NHƯNG:** Sau khi xuất Word, trạng thái VB (WorkflowStatus) **không tự động thay đổi**
- User vẫn phải tự tay click badge → đổi trạng thái

**Vấn đề cụ thể:**
1. Sau khi xuất Word → VB vẫn ở "Nháp" — gây nhầm lẫn
2. Không có flow tự động: Soạn → Kiểm tra → Xuất → Trạng thái tự cập nhật
3. Tester không biết cần tự đổi trạng thái

**🔧 ĐỀ XUẤT CẢI TIẾN:**

| # | Cải tiến | Chi tiết | Effort |
|---|---------|---------|--------|
| 2.1 | **Auto-update status sau Export** | Khi xuất Word thành công → hỏi "Đổi trạng thái VB thành 'Đã ký' / 'Đã phát hành'?" | ⭐ Thấp |
| 2.2 | **Status suggestion banner** | Sau khi lưu VB từ Compose → hiện toast: "💡 Gợi ý: Đổi trạng thái sang 'Trình ký'" | ⭐ Thấp |
| 2.3 | **Workflow wizard nhỏ** | Trong DocumentEditDialog, thêm stepper visual: Nháp → Trình ký → Duyệt → Ký → Phát hành | ⭐⭐ Vừa |

---

### VẤN ĐỀ #3: Load dữ liệu mẫu lên lâu

**Mô tả từ tester:**
> *"Load dữ liệu mẫu lên lâu và khi soạn thảo xong văn bản thì..."*

**Hiện trạng code:**
- File: `AIVanBan.Desktop/Views/AIComposeDialog.xaml.cs` — `LoadTemplates()`
- Gọi `_documentService.GetAllTemplates()` → load toàn bộ template từ LiteDB
- Hiện tại có **41+ mẫu mặc định** + mẫu user tạo

**Vấn đề cụ thể:**
1. `GetAllTemplates()` load TẤT CẢ mẫu cùng lúc — bao gồm content dài
2. Không có caching — mỗi lần mở dialog đều query lại
3. ComboBox render tất cả items cùng lúc

**🔧 ĐỀ XUẤT CẢI TIẾN:**

| # | Cải tiến | Chi tiết | Effort |
|---|---------|---------|--------|
| 3.1 | **Cache templates** | Load 1 lần khi app khởi động, cache trong memory (static). Refresh khi user thêm/sửa mẫu | ⭐⭐ Vừa |
| 3.2 | **Lazy load content** | `GetAllTemplates()` chỉ trả Name, Type, Id. Content chỉ load khi user chọn mẫu | ⭐⭐ Vừa |
| 3.3 | **Loading indicator** | Hiện skeleton/spinner trong ComboBox dropdown khi đang load | ⭐ Thấp |
| 3.4 | **Gợi ý mẫu thông minh** | Hiện 5 mẫu hay dùng nhất trước (có sẵn ở Dashboard), còn lại lazy load | ⭐⭐ Vừa |

---

### VẤN ĐỀ #4: Mẫu cuộc họp — Ngày/Tháng chưa tối ưu

**Mô tả từ tester:**
> *"BỔ SUNG CUỘC HỌP MẪU (GIAO BAN,...) — Done (còn chỗ Ngày/Tháng chưa được tối ưu)"*

**Hiện trạng code:**
- File: `AIVanBan.Core/Services/MeetingService.cs` — `CreateFromTemplate()`
- Khi tạo từ mẫu, date được set bằng `startDate` parameter
- **NHƯNG:** Nếu mẫu gốc có EndDate cách StartDate 2h → meeting mới cũng nên giữ duration 2h

**Vấn đề cụ thể:**
1. Khi tạo từ mẫu → user phải tự chọn lại ngày/giờ bắt đầu VÀ kết thúc
2. Duration từ mẫu gốc không được preserve
3. Nếu mẫu "Giao ban sáng" thì nên auto-set 8:00-9:30 (khung giờ phổ biến)

**🔧 ĐỀ XUẤT CẢI TIẾN:**

| # | Cải tiến | Chi tiết | Effort |
|---|---------|---------|--------|
| 4.1 | **Preserve duration** | `CreateFromTemplate()` → tính `duration = template.EndDate - template.StartDate` → meeting mới: `EndDate = startDate + duration` | ⭐ Thấp |
| 4.2 | **Default time from template** | Giữ giờ từ mẫu (VD: mẫu 8:00 → meeting mới cũng 8:00 ngày được chọn) | ⭐ Thấp |
| 4.3 | **Quick-set giờ phổ biến** | Thêm chip buttons: "Sáng (8:00)", "Chiều (14:00)", "Cả ngày" | ⭐⭐ Vừa |

---

### VẤN ĐỀ #5: Trạng thái VB chưa rõ duyệt như thế nào ⚠️ QUAN TRỌNG

**Mô tả từ tester:**
> *"TRANG THÁI VĂN BẢN CHƯA RÕ DUYỆT NHƯ THẾ NÀO — Vẫn chưa rõ duyệt như thế nào"*

**Hiện trạng code:**
- File: `AIVanBan.Core/Models/Document.cs` — enum `DocumentStatus` có 7 giá trị
- File: `AIVanBan.Desktop/Views/DocumentListPage.xaml.cs` — `StatusBadge_Click()` cho phép click badge → ContextMenu đổi trạng thái
- **7 trạng thái:** Nháp → Trình ký → Đã duyệt → Đã ký → Đã phát hành → Đã gửi → Lưu trữ
- Mỗi trạng thái có màu + tooltip giải thích

**Vấn đề CỐT LÕI:**
1. **Không có visual workflow** — User chỉ thấy 1 badge, không thấy cả pipeline
2. **Không có validation** — Có thể nhảy từ "Nháp" thẳng sang "Lưu trữ" (skip tất cả)
3. **Không có lịch sử chuyển trạng thái** — Ai đổi? Khi nào? Lý do?
4. **"Duyệt" mơ hồ** — Đây là app CÁ NHÂN, không có người duyệt thật → "duyệt" chỉ là GHI NHẬN cá nhân
5. **Không có hướng dẫn sử dụng** — Tester không biết click vào đâu để đổi trạng thái
6. **Tooltip chỉ hiện khi hover** — Nhiều người dùng không biết hover

**🔧 ĐỀ XUẤT CẢI TIẾN:**

| # | Cải tiến | Mức ưu tiên | Chi tiết | Effort |
|---|---------|------------|---------|--------|
| 5.1 | **Visual workflow stepper** | 🔴 Cao | Trong DocumentViewDialog/EditDialog, thêm thanh stepper ngang hiển thị 7 bước. Step hiện tại highlight, các step đã qua có ✅ | ⭐⭐⭐ Cao |
| 5.2 | **Validation chuyển trạng thái** | 🟡 Vừa | Chỉ cho phép chuyển sang trạng thái liền kề (VD: Nháp → Trình ký ✅, Nháp → Đã ký ❌). Hoặc soft-warn | ⭐⭐ Vừa |
| 5.3 | **Ghi log chuyển trạng thái** | 🟡 Vừa | Thêm `List<StatusChangeLog>` vào Document: ai đổi, khi nào, từ trạng thái nào → sang trạng thái nào | ⭐⭐ Vừa |
| 5.4 | **Rename cho phù hợp app CÁ NHÂN** | 🔴 Cao | Thay vì "Duyệt" → dùng ngôn ngữ phù hợp cá nhân. VD: "Đã trình sếp" / "Sếp đã duyệt" / "Đã ký xong" / "Đã gửi đi" | ⭐ Thấp |
| 5.5 | **Banner hướng dẫn lần đầu** | 🔴 Cao | Khi user lần đầu mở DocumentListPage → hiện banner: "💡 Click vào badge trạng thái để đổi. VD: Nháp → Trình ký → Đã ký..." | ⭐ Thấp |
| 5.6 | **Quick workflow buttons** | 🟡 Vừa | Trong Preview panel, thêm nút "Chuyển bước tiếp →" (auto chuyển sang trạng thái kế) | ⭐⭐ Vừa |
| 5.7 | **Context-aware tooltips** | 🟢 Thấp | Tooltip badge hiện: "Nháp — Click để chuyển sang 'Trình ký'" (gợi ý bước tiếp) | ⭐ Thấp |

**Mockup Visual Workflow Stepper:**
```
┌─────────────────────────────────────────────────────────────────────────────┐
│  📋 QUY TRÌNH VĂN BẢN                                                      │
│                                                                             │
│  ✅ Nháp  →  🔵 Trình ký  →  ○ Đã duyệt  →  ○ Đã ký  →  ○ Phát hành  →  ○ Gửi  →  ○ Lưu trữ │
│     ↑                                                                       │
│  Hiện tại                    [Chuyển bước tiếp →]                           │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Rename đề xuất cho phù hợp app cá nhân:**

| Hiện tại | Đề xuất (góc nhìn cá nhân) | Giải thích |
|----------|---------------------------|-----------|
| Nháp | **📝 Đang soạn** | Tôi đang soạn thảo VB này |
| Trình ký | **📤 Đã trình sếp** | Tôi đã đưa cho LĐ xem/ký |
| Đã duyệt | **✅ Sếp đã duyệt** | LĐ đã OK, chờ ký chính thức |
| Đã ký | **✍️ Đã ký** | Giữ nguyên |
| Đã phát hành | **📢 Đã phát hành** | Giữ nguyên |
| Đã gửi | **📨 Đã gửi** | Giữ nguyên |
| Lưu trữ | **🗄️ Xong — Lưu hồ sơ** | VB này tôi đã xử lý xong |

---

## 📌 ƯU TIÊN TRIỂN KHAI

### 🔴 Ưu tiên cao — Cần fix ngay (1-2 ngày)

| # | Việc cần làm | File | Est. |
|---|-------------|------|------|
| 1 | **VĐ #5:** Banner hướng dẫn trạng thái VB (lần đầu mở) | `DocumentListPage.xaml` | 2h |
| 2 | **VĐ #5:** Rename 7 trạng thái cho phù hợp cá nhân | `Document.cs`, `DocumentListPage.xaml.cs` | 1h |
| 3 | **VĐ #5:** Context-aware tooltip (gợi ý bước tiếp) | `DocumentListPage.xaml.cs` | 1h |
| 4 | **VĐ #1:** Đổi tên nút + bỏ confirm + cải tiến tooltip | `DocumentReviewDialog.xaml`, `.xaml.cs` | 1h |
| 5 | **VĐ #1:** Panel 3 options đẹp (thay 3 nút cạnh nhau) | `DocumentReviewDialog.xaml` | 3h |
| 6 | **VĐ #2:** Auto-suggest đổi status sau Export Word | `DocumentReviewDialog.xaml.cs` | 1h |

### 🟡 Ưu tiên vừa — Sprint tiếp theo (3-5 ngày)

| # | Việc cần làm | File | Est. |
|---|-------------|------|------|
| 7 | **VĐ #5:** Visual workflow stepper trong View/Edit dialog | `DocumentViewDialog.xaml`, `DocumentEditDialog.xaml` | 4h |
| 8 | **VĐ #5:** Validation chuyển trạng thái (soft-warn) | `DocumentListPage.xaml.cs` | 2h |
| 9 | **VĐ #5:** Log lịch sử chuyển trạng thái | `Document.cs`, `DocumentService.cs` | 3h |
| 10 | **VĐ #3:** Cache templates + lazy load content | `DocumentService.cs`, `AIComposeDialog.xaml.cs` | 3h |
| 11 | **VĐ #4:** Preserve duration khi tạo meeting từ mẫu | `MeetingService.cs` | 1h |
| 12 | **VĐ #1:** Nút "Lưu thành VB mới" trực tiếp từ Review | `DocumentReviewDialog.xaml.cs` | 3h |

### 🟢 Ưu tiên thấp — Nice to have

| # | Việc cần làm | File | Est. |
|---|-------------|------|------|
| 13 | **VĐ #5:** Quick workflow button "Chuyển bước tiếp →" | `DocumentListPage.xaml` | 2h |
| 14 | **VĐ #3:** Gợi ý mẫu thông minh (top 5) | `AIComposeDialog.xaml.cs` | 2h |
| 15 | **VĐ #4:** Quick-set giờ phổ biến cho meeting | `MeetingEditDialog.xaml` | 2h |

---

## 📈 TỔNG KẾT

| Metric | Giá trị |
|--------|---------|
| Tổng số vấn đề từ tester | **5** |
| Vấn đề nghiêm trọng (🔴) | **2** (#3 Performance, #5 UX Flow) |
| Vấn đề trung bình (🟡) | **2** (#1 UX, #2 UX) |
| Vấn đề nhẹ (🟢) | **1** (#4 Minor polish) |
| Tổng effort ước tính | **~3-4 ngày** |
| Ưu tiên fix đợt 1 | **6 items** (~1 ngày) |
| Ưu tiên fix đợt 2 | **6 items** (~3 ngày) |

---

> **Ghi chú:** Tất cả cải tiến phù hợp với triết lý sản phẩm — app CÁ NHÂN cho cán bộ, không phải hệ thống tập trung.
> Trạng thái VB là GHI NHẬN CÁ NHÂN, không phải quy trình duyệt chính thức.
