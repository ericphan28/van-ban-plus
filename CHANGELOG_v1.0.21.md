# VanBanPlus v1.0.21

**Ngày phát hành:** 17/11/2026
**Loại bản cập nhật:** Sửa lỗi (bắt buộc)

## 🏛️ AI theo chính quyền địa phương hai cấp

- **AI kiểm tra, soạn thảo, tham mưu, báo cáo** theo mô hình chính quyền hai cấp (tỉnh, xã) từ 01/7/2025; không còn khuyên gửi qua cấp huyện, không dẫn luật đã hết hiệu lực (Luật Tổ chức chính quyền địa phương 2015, Luật Ngân sách nhà nước 2015).
- **Ký hiệu công văn** đúng Nghị định 30/2020: «Số: 15/UBND-VP», không có «CV». Biên bản, bản ghi nhớ, bản thỏa thuận có ký hiệu BB, BGN, BTT.
- **Bản «đã sửa»** của AI Kiểm tra không tự thêm căn cứ, số liệu mà văn bản gốc không có; chỗ cần bổ sung ghi trong ngoặc vuông.

## 📝 Soạn thảo đúng tên cơ quan

- Địa phương lấy tại Cài đặt → 🏛️ Cơ quan; máy chưa khai không còn tự điền một xã mặc định.
- Bỏ ô «Cơ quan cấp trên» (không còn cấp trung gian giữa xã và tỉnh).
- Địa danh khi lưu văn bản do AI soạn lấy theo Cài đặt → Cơ quan.
- Dữ liệu mẫu trong hộp thoại soạn thảo đổi sang cơ quan cấp tỉnh.

## 📅 Ô ngày tháng

- Gõ tay «05/02/2026» được hiểu là ngày 05 tháng 02 (trước đây máy để vùng tiếng Anh đọc thành ngày 02 tháng 5).

## 🔧 Kỹ thuật

- Prompt máy chủ (`vanbanplus-api` 95cf997): khối pháp lý dùng chung `phap-ly-hien-hanh.ts`; `soan-thao` bỏ `co_quan_cap_tren`, bỏ bối cảnh địa phương khi cơ quan nhập nêu xã khác.
- `ThietLapDiaPhuong`: chưa lưu → chỉ máy có địa danh «Gia Kiệm» dùng mặc định Gia Kiệm; `DiaDanh()` cho Location và «[Địa danh]».
- `DinhDangNgayVietNam`: phần ngày giờ của culture theo vi-VN (dd/MM/yyyy), giữ định dạng số của máy.
- Bump version: `1.0.20` → `1.0.21` (csproj, AssemblyVersion, FileVersion, Inno Setup).
