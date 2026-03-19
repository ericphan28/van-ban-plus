# 📋 VanBanPlus — Changelog

---

## v1.0.14 — UI/UX Improvements (2026-06-14)

> **10 files changed**  
> Trọng tâm: Cải thiện trải nghiệm người dùng từ audit toàn diện

### ✨ Cải tiến UX

#### SnackbarHelper — Toast notification thay MessageBox
- Tạo `Services/SnackbarHelper.cs` — helper tập trung cho toast
- Thay **15+ MessageBox.Show** bằng Snackbar cho thông báo thành công
- Áp dụng: DocumentListPage, MeetingListPage, MainWindow

#### Tìm kiếm văn bản thông minh hơn
- **Debounce 300ms** — không gọi filter mỗi keystroke
- **Phím Escape** — xóa ô tìm kiếm và bỏ chọn dòng hiện tại
- **Enter** — tìm ngay lập tức

#### AI Sidebar gọn gàng
- 6 nút phẳng → 2 nút chính (Soạn VB, Kiểm tra VB) + Expander "Công cụ AI khác"
- 4 tool phụ (Scan OCR, Báo cáo, Tham mưu, Tóm tắt) gom vào nhóm mở rộng

#### DocumentEditDialog — Form phân nhóm rõ ràng
- 3 GroupBox: "📄 Thông tin cơ bản" / "🏷️ Phân loại & Thẩm quyền" / "📝 Nội dung & Đính kèm"
- MinHeight/MinWidth cho window

#### MeetingListPage — Dropdown thay 3 nút riêng
- "Thêm cuộc họp ▼" → ContextMenu: Tạo đầy đủ / Tạo nhanh / Từ mẫu

### 🎨 Polish

- **GridSplitter hiển thị rõ** (#E0E0E0) — DocumentListPage, CalendarPage
- **Preview status**: "🟢 Mới" → relative time ("Hôm nay", "3 ngày trước")
- **Vietnamese text**: "Double-click" → "Nhấn đúp", "Right-click" → "Nhấn chuột phải"
- **Abbreviation tooltips**: "Ngày BH:" → "Ngày ban hành:" + ToolTip
- **Version fix**: v1.0.9 → v1.0.13 trên status bar

---

## v1.0.9 — Tuân thủ NĐ 30/2020/NĐ-CP

> **59 files changed, +5,715 lines, -1,168 lines**  
> Trọng tâm: Chuẩn hóa nghiệp vụ văn thư theo Nghị định 30/2020/NĐ-CP

### ✨ Tính năng mới

#### Mô hình văn bản chuẩn NĐ 30/2020
- **29 loại VB hành chính** đầy đủ theo Điều 7 (thêm 9 loại: Chỉ thị, Quy chế, Quy định, Thông cáo, Hướng dẫn, Chương trình, Phương án, Đề án, Dự án)
- **Mức độ khẩn** (Thường/Khẩn/Thượng khẩn/Hỏa tốc) — Điều 8 khoản 3b
- **Độ mật** (Thường/Mật/Tối mật/Tuyệt mật) — Luật BVBMNN 2018
- **Người ký + Chức vụ người ký** — Điều 8 khoản 7
- **Số đến, Ngày đến, Hạn xử lý, Người xử lý** — Điều 22, 24
- Viết tắt chuẩn theo Phụ lục VI (QĐ, CV, BC, KH, TT...)

#### Tự động cấp số văn bản — Điều 15
- `GetNextDocumentNumber()` — số liên tiếp theo loại + năm
- `GenerateDocumentSymbol()` — format chuẩn `Số/LoạiVB-CQ` (VD: 15/QĐ-UBND)
- `GetNextArrivalNumber()` — số đến liên tiếp theo năm
- Nút **"Cấp số"** trên form nhập, tự lấy ký hiệu CQ từ cấu hình

#### Sao văn bản — Điều 25-27
- Dialog **Sao VB** với 3 hình thức: Sao y, Sao lục, Trích sao
- Tạo bản sao với số hiệu riêng (VD: 05/SY-UBND)
- Quy tắc: Sao lục/Trích sao không được sao tiếp
- Badge "📋 SAO Y" hiện trên danh sách dưới tiêu đề

#### 22 mẫu văn bản mới (tổng 41 mẫu)
- Templates cho tất cả loại VB theo NĐ 30/2020
- 17 prompt builder chuyên biệt trong AI Soạn thảo
- Mỗi prompt sinh đúng thể thức (Quốc hiệu, tiêu ngữ, nơi nhận, căn cứ, ký hiệu...)

### 🎨 Cải tiến giao diện

- **Tooltip pháp lý thân thiện** — 40+ tooltip trích dẫn NĐ 30/2020 trên toàn bộ giao diện:
  - *DocumentEditDialog*: 11 trường (Tiêu đề, Loại VB, Hướng, Ngày BH, CQ ban hành, Người ký, Nội dung...)
  - *DocumentViewDialog*: 13 label + 4 card header (Trích yếu, Căn cứ, Nơi nhận, Nội dung)
  - *DocumentRegisterPage*: Subtitle + 3 stat cards + 2 tab headers
  - *CopyDocumentDialog*: 4 trường (Hình thức sao, Người ký, Chức vụ, Nơi nhận)
  - *DashboardPage*: 3 stat cards (Tổng VB, VB Đến, VB Đi)
  - *MainWindow sidebar*: 5 navigation items (Quản lý VB, Sổ VB, Mẫu VB, AI soạn thảo, Kiểm tra VB)
  - Format thống nhất: `📐 Theo Điều X, NĐ 30/2020: [giải thích ngắn gọn]`

- **Form nhập VB**: Thêm 8 trường mới (khẩn, mật, số đến, ngày đến, hạn xử lý, người xử lý, chức vụ ký, họ tên ký). Panel VB đến tự ẩn/hiện. Tooltip NĐ 30/2020.
- **DataGrid**: Thêm cột **Hướng** (badge Đi/Đến/NB), badge bản sao, nút Sao VB
- **Dashboard**: Thêm nút **"Làm mới"** reload dữ liệu
- **Status bar**: Hiện **v1.0.9** cạnh branding
- **Thiết lập CQ**: Thêm trường **ký hiệu viết tắt** (UBND, SYT...) + auto-suggest 30+ loại CQ
- **Sổ đăng ký**: Thêm cột khẩn, mật, số đến, hạn xử lý
- Ẩn nút Demo khỏi user, đổi "Setup" → "Thiết lập"

### 🐛 Sửa lỗi

- **[Critical]** `SearchDocuments()` — NullReferenceException khi tìm kiếm (LiteDB trả null cho string fields)
- **[Critical]** `Direction` default sai `Den` → đã fix thành `Di`
- **[Critical]** `CopyDocumentDialog` — Close() trong constructor crash WPF
- **[Critical]** `CopyDocument()` — Null array (Tags, BasedOn) từ LiteDB deserialization
- **[Medium]** `CopySigningTitle` null formatting trong DocumentViewDialog
- **[Medium]** `BoolToVisConverter` không tồn tại — thay bằng DataTrigger
- **[Medium]** `SearchDocuments` mở rộng tìm thêm Issuer, SignedBy

### 🤖 Cập nhật AI Prompts (v1.0.9 chốt)

- **OCR Extract**: Cập nhật `loai_van_ban` từ 10 → 32 loại (đầy đủ theo Điều 7 NĐ 30/2020 + VBQPPL)
- **OCR Extract**: Thêm trích xuất `do_khan` (Thường/Khẩn/Thượng khẩn/Hỏa tốc) từ scan
- **OCR Extract**: Thêm trích xuất `do_mat` (Thường/Mật/Tối mật/Tuyệt mật) từ scan
- **ScanImportDialog**: Bổ sung ComboBox Độ khẩn + Độ mật + mở rộng 14 → 33 loại VB
- **vanbanplus-api**: Đồng bộ EXTRACT_SCHEMA với client (thêm enum, do_khan, do_mat)
- Các prompts khác (AI Soạn thảo, Kiểm tra, Tham mưu, Tóm tắt, Báo cáo) — ✅ đồng bộ tốt, không cần sửa

### 📁 Files thay đổi chính

| Khu vực | Files |
|---------|-------|
| Core Models | `Document.cs` (+252), `Folder.cs` |
| Core Services | `DocumentService.cs` (+202), `TemplateSeeder.cs` (+677), `OrganizationSetupService.cs` |
| AI Compose | `AIComposeDialog.xaml.cs` (+1,579) |
| Document UI | `DocumentEditDialog`, `DocumentViewDialog`, `DocumentListPage`, `DocumentRegisterPage` |
| New Dialog | `CopyDocumentDialog.xaml/.cs` |
| MainWindow | `MainWindow.xaml` (+243), `MainWindow.xaml.cs` (+362) |
| Settings | `AppUpdateService`, `ApiSettingsDialog`, `setup-vanbanplus.iss`, `update.xml` |

### 📊 Thống kê

| Metric | Giá trị |
|--------|---------|
| Files changed | 59 |
| Lines added | +5,715 |
| Lines removed | -1,168 |
| New enums | 3 (UrgencyLevel, SecurityLevel, CopyType) |
| New Document fields | 16 |
| New templates | 22 |
| New prompt builders | 17 |
| New dialogs | 1 (CopyDocumentDialog) |
| Bugs fixed | 7 |
| NĐ 30/2020 articles | Điều 7, 8, 15, 22, 24, 25-27 |

---

## v1.0.8 — AI Soạn thảo & Giao diện mới

- AI Soạn thảo văn bản (Gemini API)
- Template Store — kho mẫu văn bản
- Export Word với nơi nhận
- Quản lý ảnh + Album đơn giản
- Quản lý cuộc họp
- Scan & nhập VB từ PDF/ảnh
- Auto-update từ GitHub Releases

---

## v1.0.7 — Nền tảng

- Self-contained build (không cần cài .NET runtime)
- AI Tóm tắt VB, AI Tham mưu
- Template Store cơ bản
- Fix auto-update URL

---
