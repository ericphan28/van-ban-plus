# 📊 VanBanPlus — Project Status & Feature Tracker

> **⚠️ ĐÂY LÀ FILE DUY NHẤT THEO DÕI TRẠNG THÁI DỰ ÁN.**
> Copilot/AI agent: **Luôn đọc file này đầu tiên** trước khi làm bất kỳ tính năng nào.
> Sau khi hoàn thành tính năng: **Cập nhật file này** ngay lập tức.
>
> **Phiên bản hiện tại:** v1.0.14 (UI/UX improvements — 2026-06-14)
> **Cập nhật lần cuối:** 2026-06-14
> **Kiến trúc:** WPF .NET 9 + LiteDB + MaterialDesign-in-XAML

---

## 🎯 TRIẾT LÝ SẢN PHẨM — CÁ NHÂN HÓA

> **VanBanPlus = "Sổ tay công việc thông minh" cho CÁ NHÂN cán bộ, công chức.**
> KHÔNG phải hệ thống quản lý văn bản tập trung của cơ quan.

### Nguyên tắc cốt lõi:
| # | Nguyên tắc | Giải thích |
|---|-----------|-----------|
| 1 | **Mỗi người = 1 app riêng** | Cài trên máy cá nhân, dữ liệu LiteDB cục bộ, không chia sẻ |
| 2 | **Không thay thế hệ thống cơ quan** | Số VB chính thức do Văn thư cấp. App này chỉ GHI LẠI để theo dõi cá nhân |
| 3 | **Phục vụ cả Chuyên viên lẫn Lãnh đạo** | CV: theo dõi VB mình xử lý, soạn thảo AI. LĐ: theo dõi chỉ đạo, bút phê, deadline |
| 4 | **AI là lợi thế chính** | Soạn VB nhanh, kiểm tra lỗi, tóm tắt, tham mưu — tiết kiệm 3-5h/ngày |
| 5 | **Dữ liệu mỗi người khác nhau** | VB tôi nhận ≠ VB bạn nhận. Ghi chú, bút phê, deadline — đều là góc nhìn cá nhân |

### Ai dùng app này? Dùng như thế nào?
| Vai trò | Cách dùng | Ví dụ |
|---------|-----------|-------|
| **Chuyên viên VP** | Nhập VB đến (scan/tay) → AI soạn trả lời → theo dõi deadline → lưu hồ sơ cá nhân | Nhận CV huyện → ghi vào app → AI soạn BC → xuất Word → in trình ký |
| **Lãnh đạo (CT/PCT)** | Xem VB đến → ghi bút phê (cho chính mình nhớ) → theo dõi "tôi đã giao gì" → duyệt VB | Đọc CV → note "giao A xử lý trước 20/2" → sau kiểm tra app xem còn gì chưa xong |
| **Văn thư** | Nhập VB đến/đi → lưu sổ theo dõi cá nhân → AI scan hàng loạt → xuất danh sách | Nhận 20 VB giấy → scan OCR → lưu tất cả → cuối tháng xuất danh sách |
| **CB chuyên môn** (Tư pháp, Địa chính, VHXH) | Quản lý VB theo lĩnh vực → AI soạn chuyên ngành → album ảnh hiện trường | Soạn QĐ cấp GCN → AI kiểm tra → chụp ảnh thực địa vào album |

### Những gì KHÔNG thuộc phạm vi app:
- ❌ Đánh số VB chính thức cho cơ quan (do Văn thư làm trên hệ thống chung)
- ❌ Chia sẻ dữ liệu real-time giữa nhiều người dùng
- ❌ Phân quyền truy cập (mỗi người 1 DB riêng)
- ❌ Quy trình duyệt/ký chính thức (dùng hệ thống eGov)
- ❌ Sổ Công Văn chính thức (nghĩa vụ pháp lý của Văn thư, không phải của cá nhân)

---

## 📁 Cấu trúc dự án

| Project | Vai trò | Ghi chú |
|---------|---------|---------|
| `AIVanBan.Core` | Business logic, Models, Services | Không có UI |
| `AIVanBan.Desktop` | WPF Desktop app, code-behind | Giao diện chính |
| `AIVanBan.API` | ASP.NET Core API | Ít dùng, backup |
| `vanbanplus-api` | Next.js API (Vercel) | API chính cho cloud |

**Pháp quy tham chiếu:** `docs/van-ban-phap-quy/` → Xem `_MAPPING.md` để biết điều khoản ↔ tính năng
**Ánh xạ chi tiết:** `docs/van-ban-phap-quy/_MAPPING.md`
**Roadmap AI:** `AI_NGHIEP_VU_ROADMAP.md`
**Yêu cầu AI:** `REQUIREMENTS_AI_FEATURES.md`, `REQUIREMENT_AI_CANBO.md`

---

## ✅ A. TÍNH NĂNG ĐÃ CÓ (Implemented)

### A1. Quản lý Văn bản (Core)
| # | Tính năng | File chính | Ghi chú |
|---|-----------|-----------|---------|
| ✅ | 29+3 loại VB theo NĐ 30/2020 Điều 7 | `Document.cs` (enum `DocumentType`) | Đầy đủ |
| ✅ | Tạo/Sửa/Xóa văn bản | `DocumentEditDialog.xaml` | Form 20+ trường |
| ✅ | Xem chi tiết VB (read-only) | `DocumentViewDialog.xaml` | |
| ✅ | DataGrid danh sách VB | `DocumentListPage.xaml` | Cột: Số, Tiêu đề, Loại, Ngày, CQ, Hướng |
| ✅ | Tìm kiếm full-text | `DocumentListPage.xaml.cs` | Hỗ trợ bỏ dấu tiếng Việt |
| ✅ | Lọc nâng cao | `DocumentListPage.xaml.cs` | Theo loại, hướng, ngày, số, người ký |
| ✅ | Lọc nhanh (Hôm nay/Tuần/Tháng) | `DocumentListPage.xaml.cs` | |
| ✅ | Cây thư mục (Folder tree) | `DocumentListPage.xaml` | Phân cấp cha-con |
| ✅ | Thùng rác (soft delete/restore) | `DocumentListPage.xaml.cs` | Toggle view |
| ✅ | Bulk actions (xóa, di chuyển, xuất) | `DocumentListPage.xaml.cs` | Multi-select |
| ✅ | Nút "Chọn tất cả" (Select All) | `DocumentListPage.xaml` | **v1.0.12:** 1-click chọn tất cả VB trong DataGrid |
| ✅ | Cột Trạng thái + Quick-switch | `DocumentListPage.xaml`, `DocumentListPage.xaml.cs` | **v1.0.13:** Badge trạng thái màu trong DataGrid, click → ContextMenu 7 trạng thái, tooltip giải thích |
| ✅ | Startup notification tổng hợp | `MainWindow.xaml.cs` | **v1.0.13:** Cảnh báo khi khởi động: VB quá hạn + VB sắp hết hạn (3 ngày) + Cuộc họp hôm nay |
| ✅ | Tự động cấp số VB (Điều 15) | `DocumentService.cs` | `GetNextDocumentNumber()` |
| ✅ | Ký hiệu VB chuẩn `Số/Loại-CQ` | `DocumentService.cs` | `GenerateDocumentSymbol()` |
| ✅ | Số đến tự tăng theo năm (Điều 22) | `DocumentService.cs` | `GetNextArrivalNumber()` |
| ✅ | Sao VB: Sao y, Sao lục, Trích sao (Điều 25-27) | `CopyDocumentDialog.xaml` | 3 hình thức |
| ✅ | Mức độ khẩn (Thường/Khẩn/TK/HT) | `Document.cs` | Enum `UrgencyLevel` |
| ✅ | Độ mật (Thường/Mật/TM/TuyM) | `Document.cs` | Enum `SecurityLevel` |
| ✅ | Trạng thái VB (Draft→Archived) | `Document.cs` | Enum `DocumentStatus` — 7 trạng thái |
| ✅ | File đính kèm (nhiều file) | `AttachmentService.cs` | Word, PDF, Excel, ảnh |
| ✅ | Tags tự do | `Document.cs` | |
| ✅ | Keyboard shortcuts | `DocumentListPage.xaml.cs` | Ctrl+N, Ctrl+F, Delete, F5 |
| ✅ | 50 mẫu dữ liệu demo | `SeedDataService.cs` | 18 đi + 18 đến + 14 nội bộ |

### A2. Mẫu văn bản (Templates)
| # | Tính năng | File chính | Ghi chú |
|---|-----------|-----------|---------|
| ✅ | Quản lý mẫu (CRUD) | `TemplatePage.xaml` | |
| ✅ | 41 mẫu VB mặc định | `TemplateSeeder.cs` | |
| ✅ | Template Store (online) | `TemplatePage.xaml.cs` | Từ `template-store.json` |
| ✅ | Tìm kiếm & lọc mẫu | `TemplatePage.xaml` | |
| ✅ | Template UX nâng cao | `TemplateEditDialog.cs`, `TemplateManagementPage.xaml` | **v1.0.12:** Banner hướng dẫn, placeholder nội dung, field Phân loại + Tags, nút chèn {biến}, DataGrid grouping theo loại |
| ✅ | Template View: Times New Roman + "Sử dụng mẫu này" | `TemplateViewDialog.cs` | **v1.0.13:** Rewrite toàn bộ dialog — badges info, font Times New Roman, nút "Sử dụng mẫu này" → mở AI Soạn thảo, tags display |
| ✅ | Template: Icon "Soạn VB" thay Play | `TemplateManagementPage.xaml` | **v1.0.13:** Đổi icon Play xanh → nút text "📝 Soạn VB" rõ ràng hơn |

### A3. Trang chủ (Dashboard)
| # | Tính năng | File chính | Ghi chú |
|---|-----------|-----------|---------|
| ✅ | 5 stat cards (tổng, đi, đến, nội bộ, họp) | `DashboardPage.xaml` | Có delta tuần |
| ✅ | Panel cảnh báo (quá hạn, sắp hạn) | `DashboardPage.xaml.cs` | Điều 24 |
| ✅ | Biểu đồ VB theo loại | `DashboardPage.xaml` | Bar chart thủ công |
| ✅ | Xu hướng 12 tháng | `DashboardPage.xaml` | Canvas lines |
| ✅ | Hoạt động gần đây (10 VB) | `DashboardPage.xaml.cs` | Time-ago format |
| ✅ | Nhiệm vụ từ cuộc họp | `DashboardPage.xaml.cs` | Pending/overdue |
| ✅ | 5 mẫu hay dùng nhất | `DashboardPage.xaml.cs` | Quick-use |

### A4. AI Features
| # | Tính năng | File chính | Ghi chú |
|---|-----------|-----------|---------|
| ✅ | AI Soạn VB (từ template + prompt) | `AIComposeDialog.xaml` | Streaming output |
| ✅ | AI Kiểm tra VB (8 loại lỗi) | `DocumentReviewDialog.xaml` | Chính tả, văn phong, xung đột... |
| ✅ | AI Kiểm tra: Xuất Word + Soạn tiếp | `DocumentReviewDialog.xaml.cs` | **v1.0.12:** 2 options sau kiểm tra: 📄 Xuất Word chuẩn NĐ 30/2020, ✏️ Chuyển sang AI Soạn thảo |
| ✅ | AI Scan OCR (ảnh/PDF → trích xuất) | `ScanImportDialog.xaml` | Gemini Vision |
| ✅ | Upload file cho AI Kiểm tra/Tóm tắt/Tham mưu | `DocumentReviewDialog`, `DocumentSummaryDialog`, `DocumentAdvisoryDialog` | **v1.0.12:** Tải .docx/.pdf/.txt thay vì chỉ paste text. Dùng WordReaderService + GeminiAI.ReadTextFromFileAsync |
| ✅ | File mẫu đối chiếu (AI Kiểm tra) | `DocumentReviewDialog.xaml.cs` | **v1.0.12:** Upload VB mẫu để AI so sánh, chỉ ra điểm khác biệt/thiếu sót |
| ✅ | AI Tham mưu xử lý | `DocumentAdvisoryDialog.xaml` | Phân tích VB đến |
| ✅ | AI Tóm tắt VB | `DocumentSummaryDialog.xaml` | 10 mục tóm tắt |
| ✅ | AI Báo cáo định kỳ | `PeriodicReportDialog.xaml` | Tuần/Tháng/Quý/Năm |
| ✅ | Dual-mode: Proxy API + Direct Gemini | `ApiSettingsDialog.xaml` | Dev mode tự hết hạn 1h |

### A5. Cuộc họp (Meetings)
| # | Tính năng | File chính | Ghi chú |
|---|-----------|-----------|---------|
| ✅ | Danh sách cuộc họp (timeline) | `MeetingListPage.xaml` | 22 loại họp, grouped by date |
| ✅ | Tạo/Sửa cuộc họp (6 tab) | `MeetingEditDialog.xaml` | Người tham dự, nhiệm vụ, tài liệu, album |
| ✅ | Xuất Word: Biên bản, Kết luận, BC | `MeetingMinuteService.cs` | 3 loại xuất. **v1.0.12:** Fix SectionProperties position trong cả 4 methods (ExportBienBan, ExportKetLuan, ExportBaoCaoTongHop, ExportTongHopNhieuCuocHop) |
| ✅ | Lọc theo loại, trạng thái, ngày | `MeetingListPage.xaml.cs` | |
| ✅ | Quick filter chips (Hôm nay/Tuần này/Tháng này/Sắp tới) | `MeetingListPage.xaml.cs` | **v1.0.12:** 4 nút lọc nhanh 1-click |
| ✅ | Tìm kiếm realtime (debounce 300ms) | `MeetingListPage.xaml.cs` | 2026-02-24 |
| ✅ | Dashboard 5 stat cards | `MeetingListPage.xaml` | Tổng, Tháng, Sắp tới, NV, Quá hạn |
| ✅ | Card meeting: live badge + relative time | `MeetingListPage.xaml.cs` | "● LIVE", "Sau 2h", "Ngày mai" |
| ✅ | Card meeting: status tint + hover + strikethrough | `MeetingListPage.xaml.cs` | 2026-02-24 |
| ✅ | Task progress bar trên card | `MeetingListPage.xaml.cs` | Mini progress bar 50px |
| ✅ | Cảnh báo trùng lịch khi lưu | `MeetingEditDialog.xaml.cs` | Overlap detection + confirm |
| ✅ | Calendar: auto-load hôm nay | `CalendarPage.xaml.cs` | Mở → thấy sự kiện ngay |
| ✅ | Calendar: click ngày → tạo cuộc họp | `CalendarPage.xaml.cs` | Pre-set date |
| ✅ | Calendar: click event → mở sửa họp | `CalendarPage.xaml.cs` | MeetingId on CalendarEvent |
| ✅ | Calendar: "Sắp tới trong tuần" | `CalendarPage.xaml.cs` | 5 cuộc họp kế tiếp |
| ✅ | Calendar: GridSplitter resizable panel | `CalendarPage.xaml` | **v1.0.12:** Kéo thay đổi kích thước panel chi tiết (MinWidth=280, MaxWidth=600) |
| ✅ | Meeting: Tạo nhanh (Quick Create) | `MeetingEditDialog.xaml.cs`, `MeetingListPage.xaml` | **v1.0.13:** Chế độ tạo nhanh — chỉ hiện Tab 1, ẩn 5 tab còn lại, nút ⚡ Tạo nhanh |
| ✅ | Meeting: Mẫu cuộc họp (Templates) | `MeetingService.cs`, `MeetingListPage.xaml.cs` | **v1.0.13:** Lưu mẫu từ cuộc họp (menu ⋮), Tạo từ mẫu (nút "Từ mẫu"), Xóa mẫu |
| ✅ | Calendar: Tạo nhanh từ lịch | `CalendarPage.xaml.cs` | **v1.0.13:** Click "Thêm cuộc họp" → mở Quick Create với ngày đã chọn |

### A6. Album ảnh
| # | Tính năng | File chính | Ghi chú |
|---|-----------|-----------|---------|
| ✅ | Quản lý album + folder cây | `PhotoAlbumPageSimple.xaml` | Đang dùng version "Simple" |
| ✅ | Import/xem/xóa ảnh | `PhotoAlbumPageSimple.xaml.cs` | |
| ✅ | Cấu trúc album theo CQ (70+ phân loại) | `AlbumStructureService.cs` | 12 danh mục |
| ✅ | Upload/Download cloud | `AlbumUploadDialog.xaml` | |

### A7. Tra cứu pháp quy
| # | Tính năng | File chính | Ghi chú |
|---|-----------|-----------|---------|
| ✅ | NĐ 30/2020 toàn văn (tree view) | `LegalReferencePage.xaml` | 38 Điều, 7 Chương, 6 Phụ lục |
| ✅ | Tìm kiếm full-text | `LegalReferencePage.xaml.cs` | Bỏ dấu TV |
| ✅ | Feature tags per article | `LegalReferencePage.xaml.cs` | |
| ✅ | Kiểm tra cập nhật pháp quy online | `LegalUpdateService.cs` | Tương tự TemplateStoreService, manifest.json |

### A8. Thống kê
| # | Tính năng | File chính | Ghi chú |
|---|-----------|-----------|---------|
| ✅ | So sánh kỳ (tháng/quý/năm) | `StatisticsPage.xaml` | Delta với kỳ trước |
| ✅ | Phân tích theo loại/khẩn/mật | `StatisticsPage.xaml.cs` | DataGrid tables |
| ✅ | Xu hướng 12 tháng | `StatisticsPage.xaml` | Bar chart |

### A9. Hệ thống
| # | Tính năng | File chính | Ghi chú |
|---|-----------|-----------|---------|
| ✅ | Thiết lập cơ quan (50+ loại CQ) | `OrganizationSetupDialog.xaml` | |
| ✅ | Backup/Restore (ZIP) | `BackupPage.xaml` | Auto-backup on startup |
| ✅ | Auto-update (ClickOnce-style) | `AppUpdateService.cs` | Từ `update.xml` |
| ✅ | Đăng nhập/Đăng ký | `LoginDialog.xaml` | Email + password |
| ✅ | Admin dashboard | `AdminDashboardPage.xaml` | Quản lý user, stats |
| ✅ | Trang trợ giúp (F1) | `HelpPage.xaml` | Context-sensitive |
| ✅ | Xuất Word văn bản | `WordExportService.cs` | NĐ 30/2020 format. **v1.0.12:** Rewrite toàn bộ — fix OpenXML ordering (RunProperties trước Text, ParagraphProperties trước Run, SectionProperties cuối Body), Unicode 4 font slots, helper `CreateStyledRun` |

---

## 🔲 B. TÍNH NĂNG CHƯA CÓ — Checklist triển khai (Góc nhìn CÁ NHÂN)

> **Quy ước trạng thái:**
> - `[ ]` Chưa bắt đầu
> - `[~]` Đang làm (ghi ngày bắt đầu)
> - `[x]` Đã hoàn thành (ghi ngày xong)
> - `[!]` Bị chặn / cần thảo luận
>
> **⚠️ LƯU Ý:** Mọi tính năng dưới đây đều phục vụ CÁ NHÂN, không phải hệ thống tập trung.
> Dữ liệu mỗi người là riêng biệt. Không có đồng bộ giữa các user.

---

### B1. 📋 Sổ theo dõi VB cá nhân (Personal Document Tracker)
> **Ưu tiên:** 🥇 #1 | **Effort:** ⭐⭐ Vừa
> **Đối tượng:** Tất cả vai trò

**Mô tả:** Sổ ghi chép CÁ NHÂN — ghi lại VB tôi nhận/gửi, theo dõi deadline, trạng thái xử lý.
**KHÔNG PHẢI** sổ công văn chính thức của cơ quan. Số VB do Văn thư cấp — tôi chỉ nhập lại vào app để theo dõi.

**Ví dụ thực tế:**
- CV: "Hôm nay nhận CV 123/UBND-VP, hạn trả lời 20/2" → nhập vào app → app nhắc deadline
- LĐ: "Tôi đã giao anh A làm CV 123, hạn 20/2" → nhập vào app → app nhắc kiểm tra

| # | Task | Status | File cần tạo/sửa | Ghi chú |
|---|------|--------|-------------------|---------|
| B1.1 | Thêm field vào Document: `MyStatus` (Chưa XL/Đang XL/Đã XL/Chuyển tiếp), `AssignedTo`, `AssignedBy`, `PersonalDeadline`, `PersonalNote`, `IsStarred`, `Priority` | [x] | `Document.cs` | Thêm PersonalStatus enum, PersonalNoteEntry class, NoteType enum (21/06) |
| B1.2 | UI: Cột trạng thái cá nhân trong DataGrid (icon/badge) | [x] | `DocumentListPage.xaml` | Cột ⭐ Star + cột "Cá nhân" badge (21/06) |
| B1.3 | UI: Panel "Theo dõi cá nhân" trong Preview panel | [x] | `DocumentListPage.xaml` | Card vàng với status, priority, deadline, notes timeline (21/06) |
| B1.4 | UI: Quick-action buttons (Đánh dấu XL xong, Đặt deadline, Ghi chú, Star) | [x] | `DocumentListPage.xaml` | Click star toggle, click badge đổi status (21/06) |
| B1.5 | Lọc: "VB chưa xử lý" / "VB quá hạn" / "VB đánh dấu sao" / "VB tôi giao" | [x] | `DocumentListPage.xaml.cs` | 3 filter buttons: ⭐ Quan trọng, ⬜ Chưa XL, ⚠ Quá hạn (21/06) |
| B1.6 | Dashboard: Card "VB cần xử lý hôm nay" + "VB quá hạn" (theo trạng thái cá nhân) | [ ] | `DashboardPage.xaml` | Thay thế/bổ sung alert hiện có |
| B1.7 | Xuất Excel: Danh sách VB tôi đang theo dõi (lọc theo kỳ/trạng thái) | [ ] | `ExcelExportService.cs` | Để báo cáo công việc cá nhân |
| B1.8 | Test + sửa lỗi | [ ] | | |

---

### B2. ✍️ Ghi chú bút phê cá nhân (Personal Resolution Notes)
> **Ưu tiên:** 🥇 #2 | **Effort:** ⭐⭐ Vừa
> **Đối tượng:** Chủ yếu Lãnh đạo, CV cũng dùng để ghi ý kiến

**Mô tả:** Ghi lại bút phê/ý kiến chỉ đạo MÀ TÔI GHI, trên mỗi VB. Đây là ghi chú CÁ NHÂN.
- LĐ ghi: "Giao Phòng TC-KH tham mưu, hạn 20/2" → ghi vào app để TỰ NHẮC MÌNH
- CV ghi: "Đã báo cáo PCT Nguyễn Văn A, chờ ý kiến" → ghi để nhớ

**KHÔNG PHẢI** bút phê chính thức (bút phê chính thức ghi trên giấy/hệ thống eGov).

| # | Task | Status | File cần tạo/sửa | Ghi chú |
|---|------|--------|-------------------|---------|
| B2.1 | Model `PersonalNote` (nội dung, ngày, loại: BútPhê/GhiChú/NhắcNhở/Liên hệ, giao cho ai, hạn) | [x] | `AIVanBan.Core/Models/` | PersonalNoteEntry class + NoteType enum (21/06) |
| B2.2 | Thêm `List<PersonalNote>` vào `Document` | [x] | `Document.cs` | List<PersonalNoteEntry> Notes (21/06) |
| B2.3 | UI: Panel ghi chú trong Preview (danh sách notes + nút thêm) | [x] | `DocumentListPage.xaml` | icPreviewNotes + txtQuickNote + AddQuickNote_Click (21/06) |
| B2.4 | UI: Quick-add note (textbox + Enter = thêm note) | [x] | `DocumentListPage.xaml` | Enter key handler + Send button (21/06) |
| B2.5 | UI: Hiển thị notes trong DocumentViewDialog | [x] | `DocumentViewDialog.xaml` | Timeline card + add note inline (21/06) |
| B2.6 | Dashboard: "Bút phê/Ghi chú gần đây" | [ ] | `DashboardPage.xaml` | Top 10 notes mới nhất |
| B2.7 | Tìm kiếm trong notes | [ ] | `DocumentListPage.xaml.cs` | Tìm VB theo nội dung ghi chú |
| B2.8 | Test + sửa lỗi | [ ] | | |

---

### B3. 📅 Lịch & Nhắc nhở cá nhân (Personal Calendar & Reminders)
> **Ưu tiên:** 🥇 #3 | **Effort:** ⭐⭐ Vừa
> **Đối tượng:** Tất cả vai trò

**Mô tả:** Lịch tổng hợp CÁ NHÂN — gom hết deadline VB, cuộc họp, nhiệm vụ vào 1 view.
Nhắc nhở khi mở app + Toast notification.

| # | Task | Status | File cần tạo/sửa | Ghi chú |
|---|------|--------|-------------------|---------|
| B3.1 | UI `CalendarPage.xaml` — lịch tháng với event markers | [x] ✅ 2025-01 | `AIVanBan.Desktop/Views/` | WPF Calendar control + overlay |
| B3.2 | Load events: VB deadline (PersonalDeadline) + Meeting + Task từ meeting | [x] ✅ 2025-01 | `CalendarPage.xaml.cs` | |
| B3.3 | Click event → mở VB/họp/task tương ứng | [x] ✅ 2026-02-24 | `CalendarPage.xaml.cs` | Click meeting card → MeetingEditDialog |
| B3.4 | Color-code: 🔴 quá hạn, 🟡 sắp hạn, 🔵 họp, 🟢 task hoàn thành | [x] ✅ 2025-01 | `CalendarPage.xaml` | |
| B3.5 | Toast notification khi mở app (VB quá hạn, sắp hạn, họp hôm nay) | [x] ✅ 2025-06 | `MainWindow.xaml.cs` | MeetingReminderService + Snackbar 2 phút/lần |
| B3.6 | Thêm vào sidebar + navigation | [x] ✅ 2025-01 | `MainWindow.xaml` | |
| B3.7 | Chế độ xem tuần (Weekly view) | [x] ✅ 2025-06 | `CalendarPage.xaml/.cs` | Toggle Month/Week, time-slot 7:00-18:00 |
| B3.8 | Test + sửa lỗi | [ ] | | |

---

### B4. 🖨️ In ấn trực tiếp
> **Ưu tiên:** 🥈 #4 | **Effort:** ⭐⭐ Vừa
> **Đối tượng:** Tất cả vai trò (ai cũng cần in)

**Mô tả:** In VB trực tiếp từ app thay vì Export Word → mở Word → in. Tiết kiệm 2-3 phút mỗi lần.

| # | Task | Status | File cần tạo/sửa | Ghi chú |
|---|------|--------|-------------------|---------|
| B4.1 | Service `PrintService` (FlowDocument → WPF PrintDialog) | [ ] | `AIVanBan.Desktop/Services/` | |
| B4.2 | Template FlowDocument theo NĐ 30/2020 (Quốc hiệu, tiêu ngữ, ký tên) | [ ] | `PrintService.cs` | |
| B4.3 | Nút "In" trong Preview panel + context menu | [ ] | `DocumentListPage.xaml` | |
| B4.4 | Print Preview dialog | [ ] | `PrintPreviewDialog.xaml` | |
| B4.5 | In danh sách VB (VB tôi đang theo dõi) | [ ] | `PrintService.cs` | |
| B4.6 | Test + sửa lỗi | [ ] | | |

---

### B5. 🔗 Liên kết VB cá nhân (Personal Document Links)
> **Ưu tiên:** 🥈 #5 | **Effort:** ⭐⭐ Vừa
> **Đối tượng:** Tất cả vai trò

**Mô tả:** TỰ GHI liên kết giữa các VB trong kho của mình.
"CV 45 này là trả lời CV 32 tôi nhận tuần trước" → link lại → sau nhìn thấy chuỗi.

| # | Task | Status | File cần tạo/sửa | Ghi chú |
|---|------|--------|-------------------|---------|
| B5.1 | Model `DocumentLink` (sourceId, targetId, type, note) | [ ] | `AIVanBan.Core/Models/` | Loại: TrảLời, ThayThế, BổSung, ĐínhChính, LiênQuan |
| B5.2 | Service methods (thêm/xóa link, tìm linked docs) | [ ] | `DocumentService.cs` | |
| B5.3 | UI: Nút "Liên kết VB" trong Preview panel | [ ] | `DocumentListPage.xaml` | Picker chọn VB + loại liên kết |
| B5.4 | UI: Hiện danh sách VB liên kết trong Preview + ViewDialog | [ ] | `DocumentListPage.xaml`, `DocumentViewDialog.xaml` | Click → nhảy sang VB đó |
| B5.5 | Test + sửa lỗi | [ ] | | |

---

### B6. 📁 Hồ sơ công việc cá nhân (Personal Dossier)
> **Ưu tiên:** 🥈 #6 | **Effort:** ⭐⭐⭐ Cao
> **Đối tượng:** Chuyên viên, Lãnh đạo

**Mô tả:** Gom VB liên quan vào 1 "vụ việc" CÁ NHÂN để tiện theo dõi.
"Vụ GPMB khu dân cư" → gom: QĐ thu hồi + Tờ trình + BB họp dân + CV trả lời → 1 hồ sơ.
Đây là cách TỔ CHỨC cá nhân, không phải hồ sơ lưu trữ chính thức.

| # | Task | Status | File cần tạo/sửa | Ghi chú |
|---|------|--------|-------------------|---------|
| B6.1 | Model `PersonalDossier` (tên, mô tả, tags, trạng thái: Đang xử lý/Xong/Lưu trữ) | [ ] | `AIVanBan.Core/Models/` | |
| B6.2 | Service `DossierService` (CRUD, thêm/bớt VB) | [ ] | `AIVanBan.Core/Services/` | |
| B6.3 | UI: Sidebar section "Hồ sơ" hoặc tab trong DocumentListPage | [ ] | `AIVanBan.Desktop/Views/` | |
| B6.4 | UI: Thêm VB vào hồ sơ (từ context menu hoặc drag) | [ ] | `DocumentListPage.xaml` | |
| B6.5 | UI: Xem timeline hồ sơ (VB theo thời gian) | [ ] | | |
| B6.6 | Xuất Word: Mục lục hồ sơ cá nhân | [ ] | `WordExportService.cs` | |
| B6.7 | Test + sửa lỗi | [ ] | | |

---

### B7. 📊 Xuất báo cáo công việc cá nhân (Personal Work Report)
> **Ưu tiên:** 🥈 #7 | **Effort:** ⭐⭐ Vừa
> **Đối tượng:** Tất cả vai trò

**Mô tả:** Xuất Excel/Word THỐNG KÊ CÔNG VIỆC CÁ NHÂN — để tự báo cáo lãnh đạo hoặc tổng kết.
"Tháng này tôi xử lý 45 VB, 3 VB quá hạn, soạn 12 VB đi."

| # | Task | Status | File cần tạo/sửa | Ghi chú |
|---|------|--------|-------------------|---------|
| B7.1 | Service `ExcelExportService` (ClosedXML) | [x] ✅ 2025-01 | `AIVanBan.Core/Services/` | NuGet: ClosedXML |
| B7.2 | Nút "Xuất Excel" trong StatisticsPage | [x] ✅ 2025-01 | `StatisticsPage.xaml` | |
| B7.3 | Xuất: Danh sách VB tôi xử lý (lọc theo kỳ) | [x] ✅ 2025-01 | `ExcelExportService.cs` | |
| B7.4 | Xuất: Thống kê tổng hợp (biểu đồ dạng bảng) | [ ] | `ExcelExportService.cs` | |
| B7.5 | Nút "Xuất Excel" trong DocumentListPage (VB đang hiển thị) | [ ] | `DocumentListPage.xaml` | |
| B7.6 | Test + sửa lỗi | [ ] | | |

---

### B8. 🏷️ Quản lý công việc từ VB (Personal Task from Document)
> **Ưu tiên:** 🥈 #8 | **Effort:** ⭐⭐ Vừa
> **Đối tượng:** Tất cả vai trò

**Mô tả:** Từ 1 VB → tạo nhiều "việc cần làm" CÁ NHÂN. Theo dõi tiến độ.
"CV 123 yêu cầu 3 việc: (1) Lập BC, (2) Họp dân, (3) Gửi phản hồi" → tạo 3 task → track từng cái.

| # | Task | Status | File cần tạo/sửa | Ghi chú |
|---|------|--------|-------------------|---------|
| B8.1 | Model `PersonalTask` (tiêu đề, mô tả, deadline, trạng thái, documentId, priority) | [ ] | `AIVanBan.Core/Models/` | |
| B8.2 | Service `TaskService` (CRUD, lọc theo trạng thái/deadline) | [ ] | `AIVanBan.Core/Services/` | |
| B8.3 | UI: Panel tasks trong Preview panel (VB được chọn → tasks của nó) | [ ] | `DocumentListPage.xaml` | |
| B8.4 | UI: Nút "Tạo việc cần làm" từ VB | [ ] | `DocumentListPage.xaml` | Quick-add |
| B8.5 | UI: Trang "Việc cần làm" tổng hợp (tất cả tasks từ mọi VB) | [ ] | `TaskPage.xaml` hoặc trong Dashboard | Kanban-like hoặc list |
| B8.6 | Dashboard: Card "Việc cần làm hôm nay" / "Việc quá hạn" | [ ] | `DashboardPage.xaml` | |
| B8.7 | Calendar: Tasks hiện trên lịch | [ ] | `CalendarPage.xaml.cs` | |
| B8.8 | Test + sửa lỗi | [ ] | | |

---

### B9. 🏥 Mẫu VB theo chuyên ngành (Sector-specific Templates)
> **Ưu tiên:** 🥉 #9 | **Effort:** ⭐⭐ Vừa
> **Đối tượng:** CB chuyên ngành

**Mô tả:** Bổ sung mẫu VB + prompt AI cho các lĩnh vực đặc thù.
Khi user chọn loại CQ = "Bệnh viện" → app gợi ý mẫu y tế. Chọn "Trường học" → mẫu giáo dục.

| # | Task | Status | File cần tạo/sửa | Ghi chú |
|---|------|--------|-------------------|---------|
| B9.1 | Mẫu VB đặc thù Bệnh viện (QT KCB, BC y tế, TB trực...) | [x] ✅ 2025-01 | `TemplateSeeder.cs` | 8 mẫu |
| B9.2 | Mẫu VB đặc thù Trường học (KH dạy học, QĐ khen HS, BC chất lượng) | [x] ✅ 2025-01 | `TemplateSeeder.cs` | 8 mẫu |
| B9.3 | Mẫu VB đặc thù UBND xã (BC KT-XH, QĐ hộ nghèo, KH NTM) | [x] ✅ 2025-01 | `TemplateSeeder.cs` | 8 mẫu |
| B9.4 | Auto-suggest mẫu theo loại CQ đã thiết lập | [ ] | `TemplateSeeder.cs` | |
| B9.5 | Test + sửa lỗi | [ ] | | |

---

### B10. 📱 Chia sẻ nhanh (Quick Share)
> **Ưu tiên:** 🥉 #10 | **Effort:** ⭐ Thấp
> **Đối tượng:** Tất cả vai trò

**Mô tả:** Xuất nhanh 1 VB → gửi qua Zalo/email cho đồng nghiệp hoặc cấp trên duyệt.

| # | Task | Status | File cần tạo/sửa | Ghi chú |
|---|------|--------|-------------------|---------|
| B10.1 | Nút "Xuất PDF nhanh" (1 click, lưu tạm → mở Explorer) | [ ] | `DocumentListPage.xaml` | Dùng Word → PDF conversion |
| B10.2 | Nút "Copy đường dẫn file" (để dán vào Zalo/email) | [ ] | `DocumentListPage.xaml` | Clipboard |
| B10.3 | Nút "Gửi Email" (mở mailto: với file đính kèm) | [ ] | | |
| B10.4 | Test + sửa lỗi | [ ] | | |

---

## 🔧 C. CẢI TIẾN KỸ THUẬT (Technical Debt)

| # | Vấn đề | Status | Ghi chú |
|---|--------|--------|---------|
| C1 | [x] ✅ Xóa PhotoAlbumPage + PhotoAlbumPageNew, chỉ giữ Simple | v1.0.12 |
| C2 | [x] ✅ MeetingSeeder đã có guard clause (skip nếu count > 0) | OK |
| C7 | [x] ✅ WordExportService.cs rewrite — fix OpenXML ordering bugs | v1.0.12 — ~21 RunProperties + ~20 ParagraphProperties violations fixed |
| C8 | [x] ✅ MeetingWordExportService.cs — fix SectionProperties position | v1.0.12 — 4 export methods fixed |
| C9 | [x] ✅ TemplateEditDialog.cs rewrite — UX cải tiến toàn diện | v1.0.12 — Từ feedback tập huấn |
| C10 | [x] ✅ P7: Xuất Word + Soạn tiếp sau AI Kiểm tra | v1.0.12 — 2 options: Xuất Word chuẩn, Chuyển sang Compose |
| C11 | [x] ✅ P6: Upload file .docx/.pdf/.txt cho 3 AI dialogs | v1.0.12 — Review + Summary + Advisory + File mẫu đối chiếu |
| C12 | [x] ✅ TemplateViewDialog.cs rewrite — Times New Roman + badge UI | v1.0.13 — Từ feedback tập huấn |
| C13 | [x] ✅ DocumentListPage: Status badge column + quick-switch | v1.0.13 — Click badge → ContextMenu đổi trạng thái |
| C14 | [x] ✅ MeetingService: Template system (SaveAsTemplate, CreateFromTemplate) | v1.0.13 |
| C15 | [x] ✅ MeetingEditDialog: Quick Create mode (ẩn 5/6 tab) | v1.0.13 |
| C16 | [x] ✅ Startup notification: VB quá hạn + sắp hạn + cuộc họp hôm nay | v1.0.13 |
| C17 | [x] ✅ SnackbarHelper — Toast thay MessageBox cho success/info | v1.0.14 — DocumentListPage, MeetingListPage, MainWindow |
| C18 | [x] ✅ Search debounce 300ms + Escape clear | v1.0.14 — DocumentListPage UX |
| C19 | [x] ✅ AI Sidebar consolidation (Expander gom 4 tool phụ) | v1.0.14 — MainWindow sidebar |
| C20 | [x] ✅ DocumentEditDialog: 3 GroupBox sections | v1.0.14 — Thông tin cơ bản / Phân loại / Nội dung |
| C21 | [x] ✅ MeetingListPage: 3 buttons → 1 dropdown menu | v1.0.14 — Thêm cuộc họp ▼ |
| C22 | [x] ✅ GridSplitter visible (#E0E0E0) — Document, Calendar | v1.0.14 — Người dùng nhìn thấy thanh kéo |
| C23 | [x] ✅ Preview status: relative time + workflow status | v1.0.14 — "Hôm nay", "3 ngày trước" thay thế "🟢 Mới" |
| C24 | [x] ✅ PhotoAlbum: Vietnamese text (Nhấn đúp, Nhấn chuột phải) | v1.0.14 |
| C25 | [x] ✅ Abbreviation tooltips (Ngày BH → Ngày ban hành) | v1.0.14 |
| C3 | [ ] Biểu đồ vẽ tay bằng Rectangle — không có chart library | Cân nhắc LiveCharts2 |
| C4 | [ ] AI results không cache | Tốn quota gọi lại |
| C5 | [ ] Chỉ có NĐ 30/2020 trong Legal Reference | Thêm TT, Luật Lưu trữ |
| C6 | [ ] Không có pagination — load hết vào memory | OK cho <10k VB |

---

## 📋 D. LỘ TRÌNH TRIỂN KHAI ĐỀ XUẤT

### Phase A — Nền tảng theo dõi cá nhân (Quan trọng nhất)
> Giải quyết nỗi đau: "Tôi không biết VB nào đang chờ tôi xử lý, cái nào quá hạn"

| Thứ tự | Feature | Est. | Giá trị |
|--------|---------|------|---------|
| 1 | **B1 — Sổ theo dõi VB cá nhân** | 2 ngày | Biết ngay: VB nào chưa xử lý, cái nào quá hạn |
| 2 | **B2 — Ghi chú bút phê cá nhân** | 1-2 ngày | Ghi lại ý kiến/chỉ đạo để không quên |
| 3 | **B3 — Lịch & Nhắc nhở** | 2 ngày | Nhìn lịch thấy hết deadline, không quên việc |

### Phase B — Nâng cao hiệu quả cá nhân
> Giải quyết nỗi đau: "Tìm VB cũ khó, không biết VB nào liên quan"

| Thứ tự | Feature | Est. | Giá trị |
|--------|---------|------|---------|
| 4 | **B4 — In ấn trực tiếp** | 1-2 ngày | Bớt 2 phút mỗi lần in (5-10 lần/ngày) |
| 5 | **B5 — Liên kết VB** | 1 ngày | Thấy chuỗi: CV này trả lời CV kia |
| 6 | **B7 — Xuất Excel công việc** | 1 ngày | Báo cáo công việc cá nhân cho LĐ |
| 7 | **B8 — Tasks từ VB** | 2 ngày | Chia VB thành việc nhỏ, track từng cái |

### Phase C — Tổ chức nâng cao
> Giải quyết nỗi đau: "15 VB cùng 1 vụ việc mà nằm rải rác"

| Thứ tự | Feature | Est. | Giá trị |
|--------|---------|------|---------|
| 8 | **B6 — Hồ sơ công việc** | 3 ngày | Gom VB theo vụ việc |
| 9 | **B9 — Mẫu VB chuyên ngành** | 1-2 ngày | Mẫu phù hợp BV/trường/xã |
| 10 | **B10 — Chia sẻ nhanh** | 0.5 ngày | Gửi Zalo/email nhanh |

---

## 📝 E. QUY TẮC LÀM VIỆC VỚI COPILOT

### Khi bắt đầu phiên mới:
1. **Đọc file này** (`PROJECT_STATUS.md`) để biết trạng thái hiện tại
2. **Đọc `copilot-instructions.md`** (`.github/`) để biết quy tắc code
3. **Xác nhận task** với user trước khi code

### Khi implement feature:
1. **Đánh dấu `[~]`** task đang làm + ghi ngày
2. **Code theo thứ tự**: Model → Service → UI → Test
3. **Build sau mỗi bước** — không để lỗi tích lũy
4. **Ghi comment** `// Theo Điều X, NĐ 30/2020` cho code liên quan pháp quy

### Khi hoàn thành:
1. **Build toàn bộ solution** — 0 errors
2. **Đánh dấu `[x]`** + ghi ngày hoàn thành trong file này
3. **Cập nhật `CHANGELOG.md`** nếu là feature lớn
4. **KHÔNG tạo file .md mới** — cập nhật file này thôi

### LiteDB Gotchas:
- Dùng `FindAll().Where()` thay vì `Find()` cho fields có thể null trong BSON cũ
- Khi thêm field mới vào Model → cần xử lý null cho documents đã tồn tại
- Dùng `DropCollection()` khi cần clear data (không dùng `DeleteAll()`)

---

## 📂 F. CÁC FILE .MD CŨ (Tham khảo, không cập nhật nữa)

> Các file này đã được tổng hợp vào `PROJECT_STATUS.md`. Giữ lại để tham khảo chi tiết.

| File | Nội dung | Còn giá trị? |
|------|---------|-------------|
| `AI_NGHIEP_VU_ROADMAP.md` | Phân tích nghiệp vụ + đề xuất AI features | ✅ Vẫn hữu ích (chi tiết prompt, UI design) |
| `REQUIREMENTS_AI_FEATURES.md` | Spec chi tiết AI features | ✅ Vẫn hữu ích (spec kỹ thuật) |
| `REQUIREMENT_AI_CANBO.md` | Góc nhìn cán bộ — nỗi đau thực tế | ✅ Vẫn hữu ích (user stories) |
| `CHANGELOG.md` | Lịch sử thay đổi theo version | ✅ Tiếp tục cập nhật |
| `PHASE1_COMPLETE.md` | Album structure implementation | 📁 Chỉ tham khảo |
| `PHASE2_COMPLETE.md` | Album UI implementation | 📁 Chỉ tham khảo |
| `IMPLEMENTATION_SUMMARY.md` | Album structure summary | 📁 Chỉ tham khảo |
| `DOCUMENT_MANAGEMENT_FEATURES.md` | Feature list cũ | 📁 Chỉ tham khảo |
| `CAN_CU_FEATURE_GUIDE.md` | Hướng dẫn căn cứ pháp lý | 📁 Chỉ tham khảo |
| `ALBUM_STRUCTURE_GUIDE.md` | Hướng dẫn album structure | 📁 Chỉ tham khảo |
| `ALBUM_FOLDER_HIERARCHY_PROPOSAL.md` | Đề xuất cấu trúc album | 📁 Chỉ tham khảo |
| `HUONG_DAN_SU_DUNG.md` | Hướng dẫn sử dụng | ✅ Cập nhật khi thêm feature |
| `MSIX_*.md`, `CLICKONCE_GUIDE.md` | Packaging guides | 📁 Chỉ tham khảo |
| `READY_TO_PACKAGE.md` | Packaging checklist | 📁 Chỉ tham khảo |

---

> **📌 Lưu ý cuối:** File này là "single source of truth".
> Mọi thay đổi trạng thái feature đều cập nhật tại đây.
> Không tạo thêm file .md mới cho mỗi feature/phase.
