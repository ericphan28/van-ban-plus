# VanBanPlus v1.0.20

**Ngày phát hành:** 17/11/2026
**Loại bản cập nhật:** Cập nhật tính năng (không bắt buộc)

## 🔒 AI chạy hoàn toàn qua máy chủ VanBanPlus

- **Báo trước số credit**: trước mỗi tác vụ AI (soạn thảo, kiểm tra, tóm tắt, tham mưu, báo cáo định kỳ, trích xuất), phần mềm hiện khoảng credit dự kiến và số dư hiện có.
- **Thông báo lỗi rõ ràng**: hết credit (mở sẵn trang mua credit), máy chủ AI quá tải (kèm thời gian thử lại), nội dung hoặc tệp quá lớn, mất kết nối. Tác vụ lỗi được hoàn credit.
- **Soạn thảo theo địa phương của cơ quan**: khai xã/phường, cơ quan cấp trên, tỉnh tại Cài đặt → 🏛️ Cơ quan.
- **Bỏ chế độ kết nối AI trực tiếp**: phần mềm không còn gọi thẳng nhà cung cấp AI, không còn lưu khóa AI riêng.

## 💳 Credit và lưu hỗ trợ

- **Credit gói tháng** (cấp mới mỗi tháng): Miễn phí 30 · Khởi đầu 400 · Chuyên nghiệp 1.100 · Doanh nghiệp 3.000.
- **Gói nạp** (dùng trong 90 ngày): 49.000đ = 160 credit · 99.000đ = 350 credit · 199.000đ = 750 credit.
- **Lưu để hỗ trợ** là tùy chọn, mặc định tắt: bản lưu được mã hóa, giữ 7 ngày, xóa ngay khi tắt công tắc.

## 🔧 Kỹ thuật

- Gọi `/api/v2/tac-vu/<tên>` và `/uoc-tinh` (`TacVuClient`), header `X-VBP-Nguon: desktop`, `X-VBP-Phien-Ban: 1.0.20`.
- Xóa prompt nghiệp vụ khỏi C# (`DocumentSummaryService`, `DocumentReviewService`, `DocumentAdvisoryService`, `PeriodicReportService`, soạn thảo 26 loại văn bản, trích xuất). Tệp mẫu prompt ở `vanbanplus-api/tests/mau-prompt` là mốc cố định.
- `GeminiAIService` → `TrichXuatService`; bỏ `DevModePolicy`, `UseVanBanPlusApi`, `GeminiApiKey`.
- Bump version: `1.0.19` → `1.0.20` (csproj, AssemblyVersion, FileVersion, Inno Setup).
